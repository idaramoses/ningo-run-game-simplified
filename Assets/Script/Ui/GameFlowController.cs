using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// Home scene controller. Manages home UI, level selection, runner selection, settings.
/// Game-side logic (pause, fail, resume, countdown) is now in UImanager.
/// This script delegates to UImanager when called from the game scene for backward compatibility.
/// </summary>
public class GameFlowController : MonoBehaviour
{
    public static GameFlowController Instance { get; private set; }

    [Header("UI Panels (Home Scene)")]
    public GameObject levelSelectionPanel;
    public RunnerSelectionPanel runnerSelectionPanel;
    public GameObject settingsPanel;
    public GameObject infoPanel;

    [Header("Home Canvas")]
    public GameObject canvasHome;

    [Header("Fail Preview (Home Scene)")]
    public FailRunnerPreview failPreview;
    public GameObject failPreviewCameraObj;
    
    [Header("Info")]
    public InfoPanelController infoPanelController;

    // -------------------------
    // BACK BUTTON
    // -------------------------
    private float _lastBackTime = -10f;
    private const float BackExitInterval = 2f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            HandleBackButton();
    }

    private void HandleBackButton()
    {
        // Close panels in priority order
        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            HideSettings();
            return;
        }

        if (infoPanel != null && infoPanel.activeSelf)
        {
            HideInfo();
            return;
        }

        if (runnerSelectionPanel != null && runnerSelectionPanel.gameObject.activeSelf)
        {
            HideRunnerSelection();
            return;
        }

        if (levelSelectionPanel != null && levelSelectionPanel.activeSelf)
        {
            HideLevelSelection();
            return;
        }

        // Nothing open — double-tap to exit
        float now = Time.realtimeSinceStartup;
        if (now - _lastBackTime < BackExitInterval)
        {
            Application.Quit();
        }
        else
        {
            _lastBackTime = now;
            ShowExitToast();
        }
    }

    private void ShowExitToast()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        new AndroidJavaClass("android.widget.Toast")
            .CallStatic<AndroidJavaObject>("makeText", currentActivity, "Press back again to exit", 0)
            .Call("show");
#else
        Debug.Log("[GameFlowController] Press back again to exit");
#endif
    }

    // -------------------------
    // LIFECYCLE
    // -------------------------
    private void Awake()
    {
        Instance = this;
        Debug.Log($"[GameFlowController] Awake - Instance set. levelSelectionPanel assigned: {levelSelectionPanel != null}");
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        Debug.Log($"[GameFlowController] Start - levelSelectionPanel: {(levelSelectionPanel != null ? levelSelectionPanel.name : "NULL")}, canvasHome: {(canvasHome != null ? canvasHome.name : "NULL")}");
        
        // UI default state (Home scene)
        if (levelSelectionPanel) levelSelectionPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (infoPanel) infoPanel.SetActive(false);

        if (failPreview) failPreview.gameObject.SetActive(false);
        if (failPreviewCameraObj) failPreviewCameraObj.SetActive(false);

        if (canvasHome) canvasHome.SetActive(true);

        Time.timeScale = 1f;

        if (GameStateController.Instance != null)
            GameStateController.Instance.SetPlaying(false);

        RefreshHomeRunnerPreview();
    }

    private void RefreshHomeRunnerPreview()
    {
        if (failPreview == null) return;

        if (RunnerSelectionManager.Instance == null) return;
        RunnerData current = RunnerSelectionManager.Instance.GetCurrentRunner();
        if (current == null || current.runnerPrefab == null) return;

        failPreview.ChangeRunner(current.runnerPrefab, current.animatorController);
    }

    // -------------------------
    // LEVEL SELECTION (Home Scene)
    // -------------------------
    public void ShowLevelSelection()
    {
        Debug.Log($"[GameFlowController] ShowLevelSelection called. levelSelectionPanel ref: {(levelSelectionPanel != null ? levelSelectionPanel.name : "NULL")}");

        // Auto-find if Inspector reference is missing
        // LevelSelectionPanel removed - skip auto-find
        // if (levelSelectionPanel == null)
        // {
        //     var found = FindObjectOfType<LevelSelectionPanel>(true);
        //     if (found != null)
        //     {
        //         levelSelectionPanel = found.gameObject;
        //         Debug.Log("[GameFlowController] Auto-found levelSelectionPanel");
        //     }
        // }

        if (levelSelectionPanel != null)
        {
            levelSelectionPanel.SetActive(true);
            
            // LevelSelectionPanel removed - skip panel script
            // var panelScript = levelSelectionPanel.GetComponent<LevelSelectionPanel>();
            // if (panelScript != null)
            // {
            //     // Also activate panelRoot if it exists and is separate
            //     if (panelScript.panelRoot != null)
            //     {
            //         panelScript.panelRoot.SetActive(true);
            //         Debug.Log("[GameFlowController] Also activated panelRoot");
            //     }
            //     panelScript.OpenPanel();
            // }
            // else
            // {
            //     Debug.LogError("[GameFlowController] LevelSelectionPanel SCRIPT not found on the GameObject!");
            // }
            Debug.Log($"[GameFlowController] Level selection panel opened - active: {levelSelectionPanel.activeSelf}");
        }
        else
        {
            Debug.LogError("[GameFlowController] levelSelectionPanel is not assigned and could not be found!");
        }
    }

    public void HideLevelSelection()
    {
        if (levelSelectionPanel != null)
        {
            levelSelectionPanel.SetActive(false);
        }
    }

    // -------------------------
    // PLAY - Loads the GameScene
    // -------------------------
    public void StartLevelWithCountdown()
    {
        HideLevelSelection();

        Debug.Log("[GameFlowController] Loading GameScene for selected level");

        // Load the game scene - UImanager will handle everything from there
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadGameScene();
        }
        else
        {
            // Fallback if SceneLoader not yet in scene
            SceneManager.LoadSceneAsync(SceneLoader.GAME_SCENE);
        }
    }

    public void Play()
    {
        // Delegate to StartLevelWithCountdown (loads GameScene)
        StartLevelWithCountdown();
    }

    // -------------------------
    // GAME ACTIONS - Delegate to UImanager
    // These methods exist for backward compatibility with button references.
    // -------------------------
    public void Pause()
    {
        if (UImanager.uimanager != null)
            UImanager.uimanager.Pause();
    }

    public void Resume()
    {
        if (UImanager.uimanager != null)
            UImanager.uimanager.Resume();
    }

    public void Fail()
    {
        if (UImanager.uimanager != null)
            UImanager.uimanager.Fail();
    }

    public void Restart()
    {
        if (UImanager.uimanager != null)
        {
            UImanager.uimanager.Restart();
        }
        else
        {
            // Fallback: reload current scene
            Time.timeScale = 1f;
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void Home()
    {
        if (UImanager.uimanager != null)
        {
            UImanager.uimanager.Home();
        }
        else
        {
            // Already in home scene, just reset
            Time.timeScale = 1f;
            if (BackgroundMusicManager.Instance != null)
                BackgroundMusicManager.Instance.PlayHomeMusic();
            SceneManager.LoadSceneAsync(SceneLoader.HOME_SCENE);
        }
    }

    // -------------------------
    // LEVEL COMPLETION
    // -------------------------
    // LevelCompletionPanel removed - skip level completion
    public void ShowLevelCompletionWithStars(int levelNumber, int wordsCollected, int totalWords, int stars)
    {
        Debug.Log($"[GameFlowController] Level completion skipped - LevelCompletionPanel removed");
    }
    
    public void ShowLevelCompletion()
    {
        Debug.Log("[GameFlowController] ShowLevelCompletion skipped - LevelCompletionPanel removed");
    }

    public void HideLevelCompletion()
    {
        Debug.Log("[GameFlowController] HideLevelCompletion skipped - LevelCompletionPanel removed");
    }

    // -------------------------
    // RUNNER SELECTION (Home Scene)
    // -------------------------
    public void ShowRunnerSelection()
    {
        if (runnerSelectionPanel != null)
        {
            runnerSelectionPanel.gameObject.SetActive(true);
        }

        if (failPreviewCameraObj)
            failPreviewCameraObj.SetActive(true);

        if (failPreview)
        {
            failPreview.gameObject.SetActive(true);
            failPreview.PlayIdle();
        }

        Debug.Log("[GameFlowController] Runner selection panel opened");
    }

    public void HideRunnerSelection()
    {
        if (runnerSelectionPanel != null)
        {
            runnerSelectionPanel.gameObject.SetActive(false);
        }

        if (failPreview)
            failPreview.gameObject.SetActive(false);
        
        if (failPreviewCameraObj)
            failPreviewCameraObj.SetActive(false);

        Debug.Log("[GameFlowController] Runner selection panel closed");
    }
    
    // -------------------------
    // SETTINGS (Home Scene)
    // -------------------------
    public void ShowSettings()
    {
        if (settingsPanel != null)
        {
            AnimatedPanel animatedPanel = settingsPanel.GetComponent<AnimatedPanel>();
            if (animatedPanel != null)
            {
                animatedPanel.Show();
            }
            else
            {
                settingsPanel.SetActive(true);
            }
            Debug.Log("[GameFlowController] Settings panel opened");
        }
        else
        {
            Debug.LogWarning("[GameFlowController] settingsPanel is not assigned!");
        }
    }
    
    public void HideSettings()
    {
        if (settingsPanel != null)
        {
            AnimatedPanel animatedPanel = settingsPanel.GetComponent<AnimatedPanel>();
            if (animatedPanel != null)
            {
                animatedPanel.Hide();
            }
            else
            {
                settingsPanel.SetActive(false);
            }
            Debug.Log("[GameFlowController] Settings panel closed");
        }
    }

    // -------------------------
    // INFO (Home Scene)
    // -------------------------
    public void ShowInfo()
    {
        if (infoPanel != null)
        {
            AnimatedPanel animatedPanel = infoPanel.GetComponent<AnimatedPanel>();
            if (animatedPanel != null)
            {
                animatedPanel.Show();
            }
            else
            {
                infoPanel.SetActive(true);
            }
            Debug.Log("[GameFlowController] Info panel opened");
        }
        else
        {
            Debug.LogWarning("[GameFlowController] infoPanel is not assigned!");
        }
    }
    
    public void HideInfo()
    {
        if (infoPanel != null)
        {
            AnimatedPanel animatedPanel = infoPanel.GetComponent<AnimatedPanel>();
            if (animatedPanel != null)
            {
                animatedPanel.Hide();
            }
            else
            {
                infoPanel.SetActive(false);
            }
            Debug.Log("[GameFlowController] Info panel closed");
        }
    }
}

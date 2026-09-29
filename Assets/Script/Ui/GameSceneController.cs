using System.Collections;
using UnityEngine;

/// <summary>
/// Main controller for the GameScene. Handles gameplay flow: countdown, pause, fail, resume, level completion.
/// This replaces the game-side logic that was previously in GameFlowController (which now stays in Home scene only).
/// </summary>
public class GameSceneController : MonoBehaviour
{
    public static GameSceneController Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject pausePanel;
    public GameObject failPanel;
    public GameObject levelCompletionPanel;
    public GameObject continuePanel;

    [Header("Game Canvas")]
    public GameObject canvasGame;

    [Header("Runner")]
    public PlayerRunnerController runner;
    public GameObject mainRunnerModel;

    [Header("Countdown")]
    public TMPro.TMP_Text countdownText;

    [Header("Fail Preview")]
    public FailRunnerPreview failPreview;
    public GameObject failPreviewCameraObj;

    private bool isPaused;
    private bool isFailed;

    /// <summary>Fired once after the opening countdown finishes and the level is truly running.</summary>
    public static event System.Action OnLevelReady;

    // ---------- Back button ----------
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            HandleBackButton();
    }

    private void HandleBackButton()
    {
        if (isPaused)
        {
            Resume();
            return;
        }

        if (!isFailed)
        {
            Pause();
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        // Hide panels
        if (pausePanel) pausePanel.SetActive(false);
        if (failPanel) failPanel.SetActive(false);
        if (levelCompletionPanel) levelCompletionPanel.SetActive(false);
        
        // Hide continue panel
        if (continuePanel != null)
        {
            continuePanel.SetActive(false);
            Debug.Log("[GameSceneController] Continue panel found and hidden");
        }
        else
        {
            Debug.LogWarning("[GameSceneController] Continue panel is NOT assigned! Please assign it in the Inspector.");
        }

        if (failPreview) failPreview.gameObject.SetActive(false);
        if (failPreviewCameraObj)
        {
            failPreviewCameraObj.SetActive(false);
            DisableAudioListenerIfPresent(failPreviewCameraObj);
        }

        if (canvasGame) canvasGame.SetActive(true);

        isPaused = false;
        isFailed = false;
        Time.timeScale = 1f;

        // Get runner from RunnerManager
        if (RunnerManager.Instance != null)
        {
            mainRunnerModel = RunnerManager.Instance.GetCurrentRunnerInstance();
            runner = RunnerManager.Instance.GetCurrentRunner();
        }

        if (mainRunnerModel) mainRunnerModel.SetActive(true);

        if (GameStateController.Instance != null)
            GameStateController.Instance.SetPlaying(false);

        // Start the level
        StartCoroutine(StartLevelFlow());
    }

    private IEnumerator StartLevelFlow()
    {
        // Wait a frame for everything to initialize
        yield return null;

        // Re-fetch runner in case it wasn't ready in Start
        if (runner == null && RunnerManager.Instance != null)
        {
            mainRunnerModel = RunnerManager.Instance.GetCurrentRunnerInstance();
            runner = RunnerManager.Instance.GetCurrentRunner();
            if (mainRunnerModel) mainRunnerModel.SetActive(true);
        }

        // Start WordManager for this level
        WordManager wordManager = FindObjectOfType<WordManager>();
        if (wordManager != null)
        {
            wordManager.ResetForNewLevel();
            wordManager.StartRound();
        }

        // Start countdown
        yield return StartCoroutine(CountdownAndStart());
    }

    // -------------------------
    // COUNTDOWN
    // -------------------------
    private IEnumerator CountdownAndStart()
    {
        if (GameStateController.Instance != null)
            GameStateController.Instance.SetPlaying(true);

        if (runner != null)
        {
            Debug.Log($"[GameSceneController] Starting runner: {runner.gameObject.name}");
            runner.StartGameRun();
        }
        else
        {
            Debug.LogError("[GameSceneController] Runner is null!");
        }

        Time.timeScale = 1f;

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);

            countdownText.text = "3";
            countdownText.fontSize = 80;
            yield return new WaitForSeconds(1f);

            countdownText.text = "2";
            yield return new WaitForSeconds(1f);

            countdownText.text = "1";
            yield return new WaitForSeconds(1f);

            countdownText.text = "GO!";
            countdownText.fontSize = 100;
            yield return new WaitForSeconds(0.5f);

            countdownText.gameObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("[GameSceneController] Level started");
        OnLevelReady?.Invoke();
    }

    private IEnumerator CountdownAndResume()
    {
        Time.timeScale = 0f;

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);

            countdownText.text = "3";
            countdownText.fontSize = 80;
            yield return new WaitForSecondsRealtime(1f);

            countdownText.text = "2";
            yield return new WaitForSecondsRealtime(1f);

            countdownText.text = "1";
            yield return new WaitForSecondsRealtime(1f);

            countdownText.text = "GO!";
            countdownText.fontSize = 100;
            yield return new WaitForSecondsRealtime(0.5f);

            countdownText.gameObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSecondsRealtime(1f);
        }

        Time.timeScale = 1f;

        if (GameStateController.Instance != null)
            GameStateController.Instance.SetPlaying(true);

        if (runner != null && runner.anim != null)
        {
            runner.anim.Play(runner.runState, 0, 0f);
        }
    }

    // -------------------------
    // PAUSE
    // -------------------------
    public void Pause()
    {
        if (isPaused || isFailed) return;
        isPaused = true;

        Debug.Log("[GameSceneController] PAUSE");

        // Ad system disabled in simplified build

        if (canvasGame) canvasGame.SetActive(false);
        if (mainRunnerModel) mainRunnerModel.SetActive(false);

        if (pausePanel) pausePanel.SetActive(true);

        if (failPreviewCameraObj) failPreviewCameraObj.SetActive(true);
        if (failPreview)
        {
            failPreview.gameObject.SetActive(true);
            failPreview.PlayIdle();
        }

        if (GameStateController.Instance != null)
            GameStateController.Instance.SetPlaying(false);

        Time.timeScale = 0f;
    }

    public void Resume()
    {
        if (!isPaused || isFailed) return;
        isPaused = false;

        // Ad system disabled in simplified build

        if (pausePanel) pausePanel.SetActive(false);

        if (failPreview) failPreview.gameObject.SetActive(false);
        if (failPreviewCameraObj) failPreviewCameraObj.SetActive(false);

        if (canvasGame) canvasGame.SetActive(true);
        if (mainRunnerModel) mainRunnerModel.SetActive(true);

        StartCoroutine(CountdownAndResume());
    }

    // -------------------------
    // FAIL
    // -------------------------
    public void Fail()
    {
        // Don't show fail modal during tutorial
        if (GameTutorialController.IsActive)
        {
            Debug.Log("[GameSceneController] Fail blocked - tutorial active");
            return;
        }
        
        if (isFailed) return;
        isFailed = true;
        isPaused = false;

        Debug.Log("[GameSceneController] FAIL");

        if (GameStateController.Instance != null)
            GameStateController.Instance.SetPlaying(false);

        if (pausePanel) pausePanel.SetActive(false);

        if (runner != null) runner.TriggerFall();

        StartCoroutine(HideRunnerAfterFall());

        if (failPreviewCameraObj) failPreviewCameraObj.SetActive(true);
        if (failPreview)
        {
            failPreview.gameObject.SetActive(true);
            failPreview.PlayFall();
        }

        // Show continue panel first (like Subway Surfers)
        if (continuePanel != null)
        {
            Debug.Log("[GameSceneController] Continue panel is assigned - calling Show()");
            continuePanel.SetActive(true);
            var controller = continuePanel.GetComponent<ContinuePanelController>();
            if (controller != null)
            {
                controller.Show(
                    onContinueCallback: OnContinueAfterFail,
                    onGiveUpCallback: OnGiveUpAfterFail
                );
            }
        }
        else
        {
            Debug.LogWarning("[GameSceneController] Continue panel is NULL - showing fail panel directly");
            // No continue panel - show fail panel directly
            if (failPanel) failPanel.SetActive(true);
        }

        StartCoroutine(FreezeAfter(0.35f));
    }

    private IEnumerator HideRunnerAfterFall()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        if (canvasGame) canvasGame.SetActive(false);
        if (mainRunnerModel) mainRunnerModel.SetActive(false);
    }

    private IEnumerator FreezeAfter(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Called when player chooses to continue after failing (spending coins)
    /// </summary>
    private void OnContinueAfterFail()
    {
        Debug.Log("[GameSceneController] Player chose to continue!");

        // Reset fail state
        isFailed = false;

        // IMPORTANT: Clear WordManager's own isFailed flag so CollectLetter works again
        WordManager wordManager = FindObjectOfType<WordManager>();
        if (wordManager != null)
            wordManager.ResetFailState();

        // Resume time
        Time.timeScale = 1f;

        // Hide fail preview
        if (failPreviewCameraObj) failPreviewCameraObj.SetActive(false);
        if (failPreview) failPreview.gameObject.SetActive(false);

        // Show game canvas and runner again
        if (canvasGame) canvasGame.SetActive(true);
        if (mainRunnerModel) mainRunnerModel.SetActive(true);

        // Reset runner state and make them invulnerable briefly
        if (runner != null)
        {
            runner.ResetAfterContinue();
        }

        // Resume gameplay
        if (GameStateController.Instance != null)
            GameStateController.Instance.SetPlaying(true);
    }

    /// <summary>
    /// Called when player gives up or countdown expires
    /// </summary>
    private void OnGiveUpAfterFail()
    {
        Debug.Log("[GameSceneController] Player gave up - showing fail panel");

        // Show the fail panel
        if (failPanel) failPanel.SetActive(true);

        // Reset continue count for next level
        if (continuePanel != null)
        {
            var controller = continuePanel.GetComponent<ContinuePanelController>();
            if (controller != null)
                controller.ResetContinueCount();
        }
    }

    // -------------------------
    // LEVEL COMPLETION
    // -------------------------
    public void ShowLevelCompletionWithStars(int levelNumber, int wordsCollected, int totalWords, int stars)
    {
        Debug.Log($"[GameSceneController] Level {levelNumber} completed - {wordsCollected}/{totalWords} words, {stars} stars");

        if (canvasGame) canvasGame.SetActive(false);
        if (mainRunnerModel) mainRunnerModel.SetActive(false);

        Time.timeScale = 0f;

        if (GameStateController.Instance != null)
            GameStateController.Instance.SetPlaying(false);

        if (failPreviewCameraObj) failPreviewCameraObj.SetActive(true);
        if (failPreview)
        {
            failPreview.gameObject.SetActive(true);
            failPreview.PlayDance();
        }

        if (levelCompletionPanel != null)
        {
            levelCompletionPanel.SetActive(true);

            LevelCompletionPanel panelScript = levelCompletionPanel.GetComponent<LevelCompletionPanel>();
            if (panelScript != null)
            {
                panelScript.ShowCompletion(levelNumber, wordsCollected, totalWords, stars);
            }
        }
    }

    /// <summary>
    /// Prepares the completion preview (hide runner, show preview camera, play dance) without updating the panel.
    /// Used by the tutorial completion flow so Level 0 shows the dancing model too.
    /// </summary>
    public void ShowCompletionPreviewOnly()
    {
        if (canvasGame) canvasGame.SetActive(false);
        if (mainRunnerModel) mainRunnerModel.SetActive(false);

        Time.timeScale = 0f;

        if (GameStateController.Instance != null)
            GameStateController.Instance.SetPlaying(false);

        if (failPreviewCameraObj) failPreviewCameraObj.SetActive(true);
        if (failPreview)
        {
            failPreview.gameObject.SetActive(true);
            failPreview.PlayDance();
        }
    }

    private void DisableAudioListenerIfPresent(GameObject target)
    {
        var listener = target.GetComponent<AudioListener>();
        if (listener != null && listener.enabled)
        {
            listener.enabled = false;
            Debug.Log($"[GameSceneController] Disabled extra AudioListener on {target.name}");
        }
    }

    // -------------------------
    // NAVIGATION
    // -------------------------
    public void Restart()
    {
        Time.timeScale = 1f;
        Debug.Log("[GameSceneController] Restart");

        // Reset continue count when restarting
        if (continuePanel != null)
        {
            var controller = continuePanel.GetComponent<ContinuePanelController>();
            if (controller != null)
                controller.ResetContinueCount();
        }

        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.RestartGameScene();
        }
    }

    public void Home()
    {
        Time.timeScale = 1f;
        Debug.Log("[GameSceneController] Going Home");

        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadHomeScene();
        }
    }

    /// <summary>
    /// Opens ningoafrica.app to encourage players to improve their language skills.
    /// Called from the Learn More button on the fail panel.
    /// </summary>
    public void OnLearnMorePressed()
    {
        string url = "https://ningoafrica.app";
        
        Debug.Log($"[GameSceneController] Opening Learn More: {url}");
        
        Application.OpenURL(url);
    }
}

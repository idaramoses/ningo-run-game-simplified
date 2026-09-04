using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controller for the Pause Panel UI.
/// Displays player info, resume/restart/home/quit buttons, daily mission progress, and sound/music/settings toggles.
/// </summary>
public class PausePanelController : MonoBehaviour
{
    [Header("Player Info")]
    public Image playerAvatar;
    public TMP_Text playerNameText;
    public TMP_Text coinsText;
    public TMP_Text gemsText;

    [Header("Daily Mission Progress")]
    public GameObject missionProgressGroup;
    public TMP_Text missionDescriptionText;
    public TMP_Text missionProgressText;
    public Slider missionProgressSlider;

    [Header("Settings Toggles")]
    public Image soundIcon;
    public Image musicIcon;

    [Header("Toggle Sprites")]
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;
    public Sprite musicOnSprite;
    public Sprite musicOffSprite;

    [Header("3D Character Preview")]
    [SerializeField] private GameObject previewContainer;
    [SerializeField] private RawImage previewRawImage;

    private void OnEnable()
    {
        RefreshUI();
        SetupCharacterPreview();
    }

    private void OnDisable()
    {
        if (previewContainer != null)
        {
            previewContainer.SetActive(false);
        }
    }

    private void SetupCharacterPreview()
    {
        if (previewContainer == null)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == "CharacterPreviewContainer")
                {
                    previewContainer = root;
                    break;
                }
            }
        }

        if (previewContainer != null)
        {
            previewContainer.SetActive(true);
            var cam = previewContainer.GetComponentInChildren<Camera>(true);
            if (cam != null) cam.enabled = true;
        }

        CharacterPreviewController previewCtrl = null;
        if (previewContainer != null)
        {
            previewCtrl = previewContainer.GetComponentInChildren<CharacterPreviewController>(true);
        }
        if (previewCtrl == null)
        {
            previewCtrl = CharacterPreviewController.Instance;
        }

        if (previewCtrl != null)
        {
            GameObject prefab = null;
            if (RunnerSelectionManager.Instance != null)
            {
                var currentRunner = RunnerSelectionManager.Instance.GetCurrentRunner();
                if (currentRunner != null)
                {
                    prefab = currentRunner.runnerPrefab;
                }
            }

            if (prefab == null)
            {
                prefab = RunnerManager.GetSelectedRunnerPrefab();
            }
            if (prefab == null && RunnerManager.Instance != null && RunnerManager.Instance.runners.Count > 0)
            {
                int idx = RunnerManager.GetSelectedRunnerIndex();
                idx = Mathf.Clamp(idx, 0, RunnerManager.Instance.runners.Count - 1);
                prefab = RunnerManager.Instance.runners[idx].runnerGameObject;
            }

            if (prefab != null)
            {
                previewCtrl.UpdatePreview(prefab, playCelebrate: false);
            }
        }
    }

    public void RefreshUI()
    {
        // Player info
        if (playerNameText != null)
            playerNameText.text = PlayerPrefs.GetString("PlayerName", "PLAYER");

        if (coinsText != null)
            coinsText.text = PlayerPrefs.GetInt("Coins", 0).ToString("N0");

        if (gemsText != null)
            gemsText.text = PlayerPrefs.GetInt("Gems", 0).ToString();

        // Daily mission progress
        RefreshMissionProgress();

        // Sound/Music toggle states
        RefreshToggleStates();
    }

    private void RefreshMissionProgress()
    {
        if (missionProgressGroup == null) return;

        // Example: read current daily mission from PlayerPrefs or MissionsController
        string missionDesc = PlayerPrefs.GetString("DailyMissionDesc", "Collect 500 coins");
        int current = PlayerPrefs.GetInt("DailyMissionCurrent", 0);
        int target = PlayerPrefs.GetInt("DailyMissionTarget", 500);

        if (missionDescriptionText != null)
            missionDescriptionText.text = missionDesc;

        if (missionProgressText != null)
            missionProgressText.text = $"{current}/{target}";

        if (missionProgressSlider != null)
        {
            missionProgressSlider.maxValue = target;
            missionProgressSlider.value = current;
        }
    }

    private void RefreshToggleStates()
    {
        bool soundOn = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        bool musicOn = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;

        if (soundIcon != null && soundOnSprite != null && soundOffSprite != null)
            soundIcon.sprite = soundOn ? soundOnSprite : soundOffSprite;

        if (musicIcon != null && musicOnSprite != null && musicOffSprite != null)
            musicIcon.sprite = musicOn ? musicOnSprite : musicOffSprite;
    }

    // -------------------------
    // BUTTON HANDLERS (called from Inspector onClick)
    // -------------------------

    public void OnResumePressed()
    {
        Debug.Log("[PausePanel] Resume pressed");
        if (UImanager.uimanager != null)
            UImanager.uimanager.Resume();
        else
            Debug.LogError("[PausePanel] UImanager.uimanager is NULL!");
    }

    public void OnRestartPressed()
    {
        Debug.Log("[PausePanel] Restart pressed");
        if (UImanager.uimanager != null)
        {
            UImanager.uimanager.Restart();
        }
        else
        {
            Debug.LogWarning("[PausePanel] UImanager is NULL - using fallback");
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("GameScene");
        }
    }

    public void OnHomePressed()
    {
        Debug.Log("[PausePanel] Home pressed");
        if (UImanager.uimanager != null)
        {
            UImanager.uimanager.Home();
        }
        else
        {
            Debug.LogWarning("[PausePanel] UImanager is NULL - using fallback");
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Home");
        }
    }

    public void OnQuitPressed()
    {
        Debug.Log("[PausePanel] Quit pressed");
        PlayerPrefs.Save();

        if (UImanager.uimanager != null)
        {
            UImanager.uimanager.Home();
        }
        else
        {
            Debug.LogWarning("[PausePanel] UImanager is NULL - using fallback");
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Home");
        }
    }

    public void OnSoundToggle()
    {
        bool soundOn = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        soundOn = !soundOn;
        PlayerPrefs.SetInt("SoundEnabled", soundOn ? 1 : 0);
        PlayerPrefs.Save();

        AudioListener.volume = soundOn ? 1f : 0f;
        RefreshToggleStates();
    }

    public void OnMusicToggle()
    {
        bool musicOn = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        musicOn = !musicOn;
        PlayerPrefs.SetInt("MusicEnabled", musicOn ? 1 : 0);
        PlayerPrefs.Save();

        if (BackgroundMusicManager.Instance != null)
        {
            if (musicOn)
                BackgroundMusicManager.Instance.ResumeMusic();
            else
                BackgroundMusicManager.Instance.PauseMusic();
        }

        RefreshToggleStates();
    }

    public void OnSettingsPressed()
    {
        Debug.Log("[PausePanel] Settings pressed");
    }
}

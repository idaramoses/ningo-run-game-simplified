using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject settingsPanel;
    public Button closeButton;
    public Button closeIconButton;
    public Button musicToggleButton;
    public Button soundToggleButton;
    public TMP_Dropdown languageDropdown;
    
    [Header("Toggle Images")]
    public Sprite toggleOnSprite;
    public Sprite toggleOffSprite;
    
    [Header("Audio Managers")]
    public BackgroundMusicManager musicManager;
    public SoundEffectsManager soundEffectsManager;
    
    [Header("Language")]
    public LanguageAPI languageAPI;
    public GameObject languageLoadingIndicator;
    
    // PlayerPrefs keys
    private const string MUSIC_ENABLED_KEY = "MusicEnabled";
    private const string SOUND_ENABLED_KEY = "SoundEnabled";
    
    // Language options
    private readonly string[] languageOptions = { "yoruba", "hausa", "igbo", "ibibio" };
    private readonly string[] languageDisplayNames = { "Yoruba", "Hausa", "Igbo", "Ibibio" };
    
    private bool isMusicEnabled = true;
    private bool isSoundEnabled = true;
    
    private void Awake()
    {
        // Load saved settings
        isMusicEnabled = PlayerPrefs.GetInt(MUSIC_ENABLED_KEY, 1) == 1;
        isSoundEnabled = PlayerPrefs.GetInt(SOUND_ENABLED_KEY, 1) == 1;
        
        // Find BackgroundMusicManager if not assigned
        if (musicManager == null)
        {
            musicManager = FindObjectOfType<BackgroundMusicManager>();
        }
        
        // Find SoundEffectsManager if not assigned
        if (soundEffectsManager == null)
        {
            soundEffectsManager = FindObjectOfType<SoundEffectsManager>();
        }
        
        // Find LanguageAPI if not assigned
        if (languageAPI == null)
        {
            languageAPI = FindObjectOfType<LanguageAPI>();
        }
    }
    
    private void Start()
    {
        // Hide panel initially
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        // Auto-find music toggle button if not assigned
        if (musicToggleButton == null && settingsPanel != null)
        {
            musicToggleButton = settingsPanel.GetComponentInChildren<Button>(true);
            if (musicToggleButton != null)
                Debug.Log($"[SettingsManager] Auto-found musicToggleButton: {musicToggleButton.name}");
        }
        
        // Setup button listeners
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseSettings);
        }
        
        if (closeIconButton != null)
        {
            closeIconButton.onClick.AddListener(CloseSettings);
        }
        
        if (musicToggleButton != null)
        {
            musicToggleButton.onClick.AddListener(ToggleMusic);
            Debug.Log("[SettingsManager] Music toggle listener wired to button");
        }
        else
        {
            Debug.LogError("[SettingsManager] musicToggleButton is NULL! Assign it in the Inspector or ensure a Button exists inside settingsPanel.");
        }
        
        if (soundToggleButton != null)
        {
            soundToggleButton.onClick.AddListener(ToggleSound);
        }
        
        // Setup language dropdown
        if (languageDropdown != null)
        {
            SetupLanguageDropdown();
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        }
        
        // Apply saved settings
        ApplyMusicSetting();
        ApplySoundSetting();
        UpdateToggleButtons();
    }
    
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            Debug.Log("[SettingsManager] Settings panel opened");
        }
    }
    
    public void CloseSettings()
    {
        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.HideSettings();
        }
        else if (settingsPanel != null)
        {
            // Fallback if GameFlowController is not available
            settingsPanel.SetActive(false);
            Debug.Log("[SettingsManager] Settings panel closed (fallback)");
        }
    }
    
    public void ToggleMusic()
    {
        isMusicEnabled = !isMusicEnabled;
        PlayerPrefs.SetInt(MUSIC_ENABLED_KEY, isMusicEnabled ? 1 : 0);
        PlayerPrefs.Save();
        
        ApplyMusicSetting();
        UpdateToggleButtons();
        
        Debug.Log($"[SettingsManager] Music toggled: {(isMusicEnabled ? "ON" : "OFF")}");
    }
    
    /// <summary>External entry-point so other scripts (e.g. GameFlowController) can force-sync.</summary>
    public void RefreshMusicToggle()
    {
        ApplyMusicSetting();
        UpdateToggleButtons();
    }
    
    public void ToggleSound()
    {
        isSoundEnabled = !isSoundEnabled;
        PlayerPrefs.SetInt(SOUND_ENABLED_KEY, isSoundEnabled ? 1 : 0);
        PlayerPrefs.Save();
        
        ApplySoundSetting();
        UpdateToggleButtons();
        
        Debug.Log($"[SettingsManager] Sound toggled: {(isSoundEnabled ? "ON" : "OFF")}");
    }
    
    private void ApplyMusicSetting()
    {
        // Refresh reference in case it went stale (e.g. DontDestroyOnLoad swap)
        if (musicManager == null)
        {
            musicManager = FindObjectOfType<BackgroundMusicManager>();
        }
        
        if (musicManager != null)
        {
            if (isMusicEnabled)
            {
                musicManager.Unmute();
            }
            else
            {
                musicManager.Mute();
            }
        }
        else
        {
            Debug.LogWarning("[SettingsManager] musicManager is null — cannot apply music setting.");
        }
    }
    
    private void ApplySoundSetting()
    {
        // Control sound effects (button taps, coin collect, etc.) via SoundEffectsManager
        if (soundEffectsManager != null)
        {
            soundEffectsManager.SetSoundEnabled(isSoundEnabled);
        }
        else
        {
            Debug.LogWarning("[SettingsManager] SoundEffectsManager not found! Sound effects won't be controlled.");
        }
    }
    
    private void UpdateToggleButtons()
    {
        // Update music toggle button image
        if (musicToggleButton != null)
        {
            Image buttonImage = musicToggleButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = isMusicEnabled ? toggleOnSprite : toggleOffSprite;
            }
        }
        
        // Update sound toggle button image
        if (soundToggleButton != null)
        {
            Image buttonImage = soundToggleButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = isSoundEnabled ? toggleOnSprite : toggleOffSprite;
            }
        }
    }
    
    public bool IsMusicEnabled()
    {
        return isMusicEnabled;
    }
    
    public bool IsSoundEnabled()
    {
        return isSoundEnabled;
    }
    
    // -------------------------
    // LANGUAGE MANAGEMENT
    // -------------------------
    private void SetupLanguageDropdown()
    {
        if (languageDropdown == null) return;
        
        // Clear existing options
        languageDropdown.ClearOptions();
        
        // Add language options
        languageDropdown.AddOptions(new System.Collections.Generic.List<string>(languageDisplayNames));
        
        // Set current language
        string currentLanguage = PlayerPrefs.GetString("user_selected_language", "yoruba");
        int currentIndex = System.Array.IndexOf(languageOptions, currentLanguage);
        if (currentIndex >= 0)
        {
            languageDropdown.value = currentIndex;
        }
        
        Debug.Log($"[SettingsManager] Language dropdown setup with current language: {currentLanguage}");
    }
    
    private void OnLanguageChanged(int index)
    {
        if (index < 0 || index >= languageOptions.Length) return;
        
        string selectedLanguage = languageOptions[index];
        Debug.Log($"[SettingsManager] Language changed to: {selectedLanguage}");
        
        // Show loading indicator
        if (languageLoadingIndicator != null)
        {
            languageLoadingIndicator.SetActive(true);
        }
        
        // Update language via API
        if (languageAPI != null)
        {
            languageAPI.UpdateUserLanguage(selectedLanguage, (success) =>
            {
                // Hide loading indicator
                if (languageLoadingIndicator != null)
                {
                    languageLoadingIndicator.SetActive(false);
                }
                
                if (success)
                {
                    Debug.Log($"[SettingsManager] Language updated successfully to: {selectedLanguage}");
                    
                    // Notify WordManager to reload if it exists
                    WordManager wordManager = FindObjectOfType<WordManager>();
                    if (wordManager != null)
                    {
                        Debug.Log("[SettingsManager] WordManager found, it will use new dictionary on next word");
                    }
                }
                else
                {
                    Debug.LogWarning("[SettingsManager] Failed to update language");
                }
            });
        }
        else
        {
            Debug.LogWarning("[SettingsManager] LanguageAPI not found!");
            if (languageLoadingIndicator != null)
            {
                languageLoadingIndicator.SetActive(false);
            }
        }
    }
}

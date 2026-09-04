using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsController : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Audio Controls")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider soundEffectsSlider;

    [Header("Region & Language")]
    [SerializeField] private TMP_Text countryValueText;
    [SerializeField] private TMP_Text languageValueText;

    [Header("Footer")]
    [SerializeField] private TMP_Text versionText;

    [Header("Modals")]
    [SerializeField] private SelectionModalController languageModal;
    [SerializeField] private SelectionModalController countryModal;

    // PlayerPrefs keys
    private const string KEY_MUSIC_ON = "Settings_MusicOn";
    private const string KEY_MUSIC_VOLUME = "MusicVolume";
    private const string KEY_SFX_VOLUME = "SFXVolume";
    private const string KEY_COUNTRY = "Settings_Country";
    private const string KEY_LANGUAGE = "Settings_Language";

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        // Ensure CanvasGroup exists for fade animations
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        LoadSettings();
        SetupListeners();
        UpdateVersionText();
    }

    private void LoadSettings()
    {
        // Load audio settings
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = PlayerPrefs.GetFloat(KEY_MUSIC_VOLUME, 0.7f);

        if (soundEffectsSlider != null)
            soundEffectsSlider.value = PlayerPrefs.GetFloat(KEY_SFX_VOLUME, 0.5f);

        // Load region/language
        if (countryValueText != null)
            countryValueText.text = PlayerPrefs.GetString(KEY_COUNTRY, "Nigeria");

        if (languageValueText != null)
            languageValueText.text = PlayerPrefs.GetString(KEY_LANGUAGE, "English");
    }

    private void SetupListeners()
    {
        // Sliders still use code wiring (they don't have OnClick events).
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (soundEffectsSlider != null)
            soundEffectsSlider.onValueChanged.AddListener(OnSoundEffectsChanged);

        // NOTE: All Button OnClick events are wired in the Unity Inspector.
        // Select each button, scroll to On Click(), click +, drag Canvas_Settings,
        // and assign the public methods below.
    }

    private void UpdateVersionText()
    {
        if (versionText != null)
            versionText.text = $"Game Version {Application.version}";
    }

    #region Event Handlers

    private void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(KEY_MUSIC_VOLUME, value);
        PlayerPrefs.Save();
        Debug.Log($"[Settings] Music Volume: {value:F2}");
        if (BackgroundMusicManager.Instance != null)
        {
            BackgroundMusicManager.Instance.SetVolume(value);
        }
    }

    private void OnSoundEffectsChanged(float value)
    {
        PlayerPrefs.SetFloat(KEY_SFX_VOLUME, value);
        PlayerPrefs.Save();
        Debug.Log($"[Settings] SFX Volume: {value:F2}");
        if (SoundEffectsManager.Instance != null)
        {
            SoundEffectsManager.Instance.SetSFXVolume(value);
        }
    }

    /// <summary>
    /// Assign this to BackToHomeButton OnClick().
    /// </summary>
    public void OnBackPressed()
    {
        Debug.Log("[Settings] Back to Home pressed");
        // Prefer routing through UImanager so it handles Canvas_Home reactivation.
        UImanager homeCtrl = UImanager.uimanager;
        if (homeCtrl != null)
            homeCtrl.HideSettings();
        else
            Hide();
    }

    /// <summary>
    /// Assign this to CountryButton OnClick().
    /// </summary>
    public void OnCountryPressed()
    {
        Debug.Log("[Settings] Country/Region pressed");
        if (countryModal != null)
        {
            countryModal.OnConfirmed -= HandleCountryConfirmed;
            countryModal.OnConfirmed += HandleCountryConfirmed;
            countryModal.Show();
        }
    }

    /// <summary>
    /// Assign this to LanguageButton OnClick().
    /// </summary>
    public void OnLanguagePressed()
    {
        Debug.Log("[Settings] Language pressed");
        if (languageModal != null)
        {
            languageModal.OnConfirmed -= HandleLanguageConfirmed;
            languageModal.OnConfirmed += HandleLanguageConfirmed;
            languageModal.Show();
        }
    }

    private void HandleCountryConfirmed(int index, string country)
    {
        if (countryValueText != null) countryValueText.text = country;
        PlayerPrefs.SetString(KEY_COUNTRY, country);
        PlayerPrefs.Save();
    }

    private void HandleLanguageConfirmed(int index, string language)
    {
        if (languageValueText != null) languageValueText.text = language;
        PlayerPrefs.SetString(KEY_LANGUAGE, language);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Assign this to SupportButton OnClick().
    /// </summary>
    public void OnSupportPressed()
    {
        Debug.Log("[Settings] Support pressed");
        // TODO: Open support URL or panel
        // Application.OpenURL("https://your-support-url.com");
    }

    /// <summary>
    /// Assign this to TermsButton OnClick().
    /// </summary>
    public void OnTermsPressed()
    {
        Debug.Log("[Settings] Terms of Service pressed");
        // Application.OpenURL("https://your-terms-url.com");
    }

    /// <summary>
    /// Assign this to PrivacyButton OnClick().
    /// </summary>
    public void OnPrivacyPressed()
    {
        Debug.Log("[Settings] Privacy Policy pressed");
        // Application.OpenURL("https://your-privacy-url.com");
    }

    /// <summary>
    /// Assign this to RestorePurchasesButton OnClick().
    /// </summary>
    public void OnRestorePurchasesPressed()
    {
        Debug.Log("[Settings] Restore Purchases pressed");
        // TODO: Integrate with IAP system
    }

    /// <summary>
    /// Assign this to DeleteAccountButton OnClick().
    /// </summary>
    public void OnDeleteAccountPressed()
    {
        Debug.Log("[Settings] Delete Account pressed");
        // TODO: Show confirmation dialog and delete account
    }

    #endregion

    #region Show/Hide with Fade Animation

    public void Show()
    {
        gameObject.SetActive(true);
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeIn());
        Debug.Log("[SettingsController] Show -> " + gameObject.name + " fading in.");
    }

    public void Hide()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeOut());
        Debug.Log("[SettingsController] Hide -> " + gameObject.name + " fading out.");
    }

    private IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
    }

    private IEnumerator FadeOut()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    #endregion
}

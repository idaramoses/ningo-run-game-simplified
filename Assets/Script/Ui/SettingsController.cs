using System.Collections;
using UnityEngine;

public class SettingsController : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Modals")]
    [SerializeField] private SelectionModalController languageModal;

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        // Ensure CanvasGroup exists for fade animations
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    #region Event Handlers

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
        else
        {
            Debug.LogWarning("[Settings] languageModal is not assigned!");
        }
    }

    private void HandleLanguageConfirmed(int index, string language)
    {
        // SelectionModalController already saves "user_selected_language" and
        // routes the change through LanguageAPI - just refresh the pill display
        var pill = GetComponentInChildren<NingoLanguagePill>(true);
        if (pill != null) pill.Refresh();
    }

    /// <summary>
    /// Assign this to SupportButton OnClick().
    /// </summary>
    public void OnSupportPressed()
    {
        Debug.Log("[Settings] Support pressed");
        Application.OpenURL("mailto:support@ningoafrica.app");
    }

    /// <summary>
    /// Assign this to TermsButton OnClick().
    /// </summary>
    public void OnTermsPressed()
    {
        Debug.Log("[Settings] Terms of Service pressed");
        Application.OpenURL("https://ningoafrica.app/terms");
    }

    /// <summary>
    /// Assign this to PrivacyButton OnClick().
    /// </summary>
    public void OnPrivacyPressed()
    {
        Debug.Log("[Settings] Privacy Policy pressed");
        Application.OpenURL("https://ningoafrica.app/privacy");
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

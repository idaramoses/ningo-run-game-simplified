using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach to the LangPanel Button in Canvas_Home.
/// - Clicking opens the HomeLanguageModal.
/// - Automatically shows the current language name (native_name if available).
/// - Updates instantly when the user picks a different language.
/// </summary>
[RequireComponent(typeof(Button))]
public class LangPanelHandler : MonoBehaviour
{
    [Tooltip("Optional: assign the TMP_Text inside LangPanel that shows the language name. " +
             "If left empty the script finds it automatically.")]
    [SerializeField] private TMP_Text languageLabel;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);

        // Auto-find label if not assigned
        if (languageLabel == null)
            languageLabel = GetComponentInChildren<TMP_Text>();

        // Show current language immediately
        RefreshLabel(PlayerPrefs.GetString("user_selected_language", ""));

        // If cache is already loaded, resolve the native name now
        if (LanguageCache.Instance != null && LanguageCache.Instance.IsReady)
            RefreshLabel(PlayerPrefs.GetString("user_selected_language", ""));
        else if (LanguageCache.Instance != null)
            LanguageCache.Instance.OnLoaded += OnCacheLoaded;

        // Listen for future language changes
        HomeLanguageModal.OnLanguageChanged += RefreshLabel;
    }

    private void OnDestroy()
    {
        HomeLanguageModal.OnLanguageChanged -= RefreshLabel;
        if (LanguageCache.Instance != null)
            LanguageCache.Instance.OnLoaded -= OnCacheLoaded;
    }

    private void OnCacheLoaded(LanguageCache.LanguageItem[] _)
    {
        LanguageCache.Instance.OnLoaded -= OnCacheLoaded;
        RefreshLabel(PlayerPrefs.GetString("user_selected_language", ""));
    }

    private void RefreshLabel(string code)
    {
        if (languageLabel == null) return;

        // Try to show the native name from the cache
        if (!string.IsNullOrEmpty(code) &&
            LanguageCache.Instance != null &&
            LanguageCache.Instance.IsReady &&
            LanguageCache.Instance.Languages != null)
        {
            foreach (var lang in LanguageCache.Instance.Languages)
            {
                if (lang.language == code)
                {
                    languageLabel.text = string.IsNullOrEmpty(lang.native_name)
                        ? Capitalize(code)
                        : lang.native_name;
                    return;
                }
            }
        }

        // Fallback: just capitalise the code
        languageLabel.text = string.IsNullOrEmpty(code) ? "Language" : Capitalize(code);
    }

    private static string Capitalize(string s) =>
        s.Length == 0 ? s : char.ToUpper(s[0]) + s.Substring(1);

    private void OnClick()
    {
        if (GameFlowController.Instance != null)
            GameFlowController.Instance.ShowLanguageModal();
        else
            Debug.LogWarning("[LangPanelHandler] GameFlowController.Instance not found!");
    }
}

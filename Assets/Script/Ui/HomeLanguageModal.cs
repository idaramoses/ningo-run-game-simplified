using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Language-change modal for the Home scene.
/// 
/// SETUP IN UNITY:
/// 1. In Canvas_Home, create a new Panel (right-click Canvas_Home > UI > Panel)
///    Name it "LanguageModal". Set its RectTransform to stretch full screen.
///    Give it a semi-transparent dark color (Image alpha ~180).
/// 2. Inside it, create a child Panel named "Card" (white, ~600x750, centered).
/// 3. Inside Card, add:
///    - TMP_Text  "Title"
///    - ScrollRect "ScrollView" (with a Content child for buttons)
///    - Button    "CloseButton"
///    - GameObject "LoadingView" (with TMP_Text "LoadingLabel")
/// 4. Attach this script to "LanguageModal".
/// 5. Wire all references in Inspector.
/// 6. Assign LanguageModal to GameFlowController.languageModal.
/// 7. Drag LanguageModal into the scene (disabled by default).
/// </summary>
public class HomeLanguageModal : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform buttonContainer;   // Content inside the ScrollView
    [SerializeField] private GameObject languageButtonPrefab; // Button prefab with TMP_Text
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private GameObject loadingView;
    [SerializeField] private TMP_Text loadingLabel;
    [SerializeField] private TMP_Text statusText;         // optional error/status line

    /// <summary>Fired with the language code whenever the user confirms a new language.</summary>
    public static event System.Action<string> OnLanguageChanged;

    private List<GameObject> spawnedButtons = new List<GameObject>();
    private string selectedCode;
    private GameObject selectedButtonObj;
    private bool isApplyingLanguage = false;

    private static readonly Color HighlightColor  = new Color(0.18f, 0.62f, 0.95f);  // blue
    private static readonly Color DefaultColor     = Color.white;
    private static readonly Color HighlightTextCol = Color.white;
    private static readonly Color DefaultTextCol   = new Color(0.1f, 0.1f, 0.1f);

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (titleText != null) titleText.text = "Select Language";

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    private void OnEnable()
    {
        selectedCode = PlayerPrefs.GetString("user_selected_language", "");
        ClearButtons();
        SetStatus("");

        if (LanguageCache.Instance != null && LanguageCache.Instance.IsReady)
        {
            // Instant — no network wait
            SetLoading(false);
            BuildButtons(LanguageCache.Instance.Languages);
        }
        else
        {
            // Cache not ready yet — wait for background fetch
            SetLoading(true, "Loading languages...");
            if (LanguageCache.Instance != null)
                LanguageCache.Instance.OnLoaded += OnCacheLoaded;
            else
                Debug.LogWarning("[HomeLanguageModal] LanguageCache not found in scene.");
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (LanguageCache.Instance != null)
            LanguageCache.Instance.OnLoaded -= OnCacheLoaded;
    }

    private void OnCacheLoaded(LanguageCache.LanguageItem[] languages)
    {
        if (LanguageCache.Instance != null)
            LanguageCache.Instance.OnLoaded -= OnCacheLoaded;
        SetLoading(false);
        BuildButtons(languages);
    }

    // -------------------------------------------------------------------------
    // PUBLIC API
    // -------------------------------------------------------------------------

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // BUTTONS
    // -------------------------------------------------------------------------

    private void BuildButtons(LanguageCache.LanguageItem[] languages)
    {
        ClearButtons();

        if (buttonContainer == null || languageButtonPrefab == null)
        {
            Debug.LogError("[HomeLanguageModal] buttonContainer or languageButtonPrefab not assigned!");
            return;
        }

        foreach (var lang in languages)
        {
            GameObject btnObj = Instantiate(languageButtonPrefab, buttonContainer);
            spawnedButtons.Add(btnObj);

            TMP_Text label = btnObj.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = string.IsNullOrEmpty(lang.native_name) ? lang.language : $"{lang.native_name}";

            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
            {
                string code = lang.language;
                btn.onClick.AddListener(() => OnLanguageClicked(code, btnObj));
            }

            // Pre-highlight current language
            if (!string.IsNullOrEmpty(selectedCode) && lang.language == selectedCode)
            {
                selectedButtonObj = btnObj;
                ApplyHighlight(btnObj, true);
            }
        }
    }

    private void OnLanguageClicked(string code, GameObject btnObj)
    {
        // Deselect previous
        if (selectedButtonObj != null)
            ApplyHighlight(selectedButtonObj, false);

        selectedCode = code;
        selectedButtonObj = btnObj;
        ApplyHighlight(btnObj, true);

        ApplyLanguageChange(code);
    }

    private void ApplyHighlight(GameObject btnObj, bool on)
    {
        Image img = btnObj.GetComponent<Image>();
        if (img != null) img.color = on ? HighlightColor : DefaultColor;

        TMP_Text lbl = btnObj.GetComponentInChildren<TMP_Text>();
        if (lbl != null) lbl.color = on ? HighlightTextCol : DefaultTextCol;
    }

    private void ClearButtons()
    {
        foreach (var b in spawnedButtons)
            if (b != null) Destroy(b);
        spawnedButtons.Clear();
        selectedButtonObj = null;
    }

    // -------------------------------------------------------------------------
    // APPLY CHANGE
    // -------------------------------------------------------------------------

    private void ApplyLanguageChange(string code)
    {
        if (isApplyingLanguage)
        {
            Debug.Log("[HomeLanguageModal] Language change already in progress — ignoring duplicate click.");
            return;
        }

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            ToastManager.Show("Connect to the internet to change language");
            return;
        }

        isApplyingLanguage = true;
        SetLoading(true, "Updating...");

        if (LanguageAPI.Instance != null)
        {
            LanguageAPI.Instance.UpdateUserLanguage(code, success =>
            {
                SetLoading(false);

                if (success)
                {
                    PlayerPrefs.SetString("user_selected_language", code);
                    PlayerPrefs.Save();

                    // Only update UI and cache after API confirms dictionary fetched
                    OnLanguageChanged?.Invoke(code);
                    DictionaryManager.InvalidateMemoryCache();
                    LanguageCache.Instance?.InvalidateCache();

                    SetStatus("Language updated!");
                }
                else
                {
                    SetStatus("Update failed — please try again.");
                }

                isApplyingLanguage = false;
                StartCoroutine(CloseAfterDelay(1.2f));
            });
        }
        else
        {
            isApplyingLanguage = false;
            StartCoroutine(CloseAfterDelay(0.8f));
        }
    }

    private IEnumerator CloseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Close();
    }

    // -------------------------------------------------------------------------
    // HELPERS
    // -------------------------------------------------------------------------

    private void SetLoading(bool show, string message = "")
    {
        if (loadingView != null) loadingView.SetActive(show);
        if (loadingLabel != null) loadingLabel.text = message;
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }
}

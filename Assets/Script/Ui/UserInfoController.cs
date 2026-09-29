using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

/// <summary>
/// Onboarding controller with three steps:
/// 1. Login panel    - email/password login OR guest login
/// 2. Language panel - pick a language (populated from LanguageCache or API)
/// 3. Welcome panel  - final "Let's Go!" step
///
/// API endpoints mirror LoginForm.cs:
///   POST {apiBaseUrl}/api/auth/login
///   POST {apiBaseUrl}/api/auth/signup
///   GET  {apiBaseUrl}/api/dictionary?language={code}&limit=1000
/// </summary>
public class UserInfoController : MonoBehaviour
{
    [Header("API")]
    [SerializeField] private string apiBaseUrl = "https://ningoafrica.app";

    [Header("Panels")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject languagePanel;
    [SerializeField] private GameObject welcomePanel;

    [Header("Login Step")]
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button guestButton;
    [SerializeField] private Button signUpButton;
    [SerializeField] private TMP_Text loginStatusText;

    [Header("Button Loading Indicators")]
    [SerializeField] private GameObject loginButtonSpinner;
    [SerializeField] private TMP_Text loginButtonText;
    [SerializeField] private GameObject guestButtonSpinner;
    [SerializeField] private TMP_Text guestButtonText;

    [Header("Language Step")]
    [SerializeField] private Transform languageButtonContainer;
    [SerializeField] private GameObject languageButtonPrefab;
    [SerializeField] private Button languageNextButton;
    [SerializeField] private Button languageBackButton;
    [SerializeField] private GameObject languageNextButtonSpinner;
    [SerializeField] private TMP_Text languageNextButtonText;
    [SerializeField] private GameObject languageLoadingView;
    [SerializeField] private TMP_Text languageStatusText;

    [Header("Welcome Step")]
    [SerializeField] private Button letsGoButton;
    [SerializeField] private GameObject letsGoButtonSpinner;
    [SerializeField] private TMP_Text letsGoButtonText;

    [Header("Shared Loading")]
    [SerializeField] private GameObject loadingView;
    [SerializeField] private TMP_Text loadingStatusLabel;

    [Header("Navigation")]
    [SerializeField] private string homeSceneName = "Home";

    [Header("Animation")]
    [SerializeField] private float panelFadeInDuration = 0.5f;
    [SerializeField] private float panelStartScale = 0.85f;

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------
    private string selectedLanguageCode = "";
    private GameObject selectedLanguageButton;
    private readonly List<GameObject> spawnedLanguageButtons = new List<GameObject>();

    private bool isGuestMode = false;
    private bool isAuthenticating = false;
    private string authToken = "";
    private string refreshToken = "";

    private CanvasGroup loginPanelCanvasGroup;
    private CanvasGroup languagePanelCanvasGroup;
    private CanvasGroup welcomePanelCanvasGroup;

    private static readonly Color HighlightColor = new Color(0.18f, 0.62f, 0.95f);
    private static readonly Color DefaultColor = Color.white;
    private static readonly Color HighlightTextCol = Color.white;
    private static readonly Color DefaultTextCol = new Color(0.1f, 0.1f, 0.1f);

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------
    private void Start()
    {
        SetupCanvasGroups();
        ResolveLoginButtons();

        if (loginPanel != null) loginPanel.SetActive(true);
        if (languagePanel != null) languagePanel.SetActive(false);
        if (welcomePanel != null) welcomePanel.SetActive(false);

        ResolveButtonLoadingIndicators();
        SetLoginButtonLoading(false);
        SetGuestButtonLoading(false);
        SetLanguageNextButtonLoading(false);
        SetLetsGoButtonLoading(false);

        if (loginButton != null)
        {
            loginButton.onClick = new Button.ButtonClickedEvent();
            loginButton.onClick.AddListener(OnClickLogin);
        }

        if (guestButton != null)
        {
            guestButton.onClick = new Button.ButtonClickedEvent();
            guestButton.onClick.AddListener(OnClickGuest);
        }

        if (signUpButton != null)
        {
            signUpButton.onClick = new Button.ButtonClickedEvent();
            signUpButton.onClick.AddListener(OnClickSignUp);
        }

        if (languageNextButton != null)
        {
            languageNextButton.onClick = new Button.ButtonClickedEvent();
            languageNextButton.onClick.AddListener(OnLanguageNextPressed);
            languageNextButton.interactable = false;
        }

        if (languageBackButton != null)
        {
            languageBackButton.onClick = new Button.ButtonClickedEvent();
            languageBackButton.onClick.AddListener(OnLanguageBackPressed);
        }

        if (letsGoButton != null)
        {
            letsGoButton.onClick = new Button.ButtonClickedEvent();
            letsGoButton.onClick.AddListener(OnLetsGoPressed);
        }

        ShowLoginStep();
    }

    private void OnDestroy()
    {
        if (LanguageCache.Instance != null)
            LanguageCache.Instance.OnLoaded -= OnLanguagesLoaded;
    }

    private void SetupCanvasGroups()
    {
        loginPanelCanvasGroup = EnsureCanvasGroup(loginPanel);
        languagePanelCanvasGroup = EnsureCanvasGroup(languagePanel);
        welcomePanelCanvasGroup = EnsureCanvasGroup(welcomePanel);
    }

    private void ResolveButtonLoadingIndicators()
    {
        ResolveButtonLoadingIndicator(loginButton, ref loginButtonSpinner, ref loginButtonText);
        ResolveButtonLoadingIndicator(guestButton, ref guestButtonSpinner, ref guestButtonText);
        ResolveButtonLoadingIndicator(languageNextButton, ref languageNextButtonSpinner, ref languageNextButtonText);
        ResolveButtonLoadingIndicator(letsGoButton, ref letsGoButtonSpinner, ref letsGoButtonText);
    }

    private void ResolveButtonLoadingIndicator(Button button, ref GameObject spinner, ref TMP_Text label)
    {
        if (button == null) return;
        if (label == null) label = button.GetComponentInChildren<TMP_Text>(true);
        if (spinner != null) return;

        foreach (Transform child in button.GetComponentsInChildren<Transform>(true))
        {
            string childName = child.gameObject.name;
            bool isSpinner = childName.IndexOf("spinner", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                             childName.IndexOf("progress", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                             childName.IndexOf("loading", System.StringComparison.OrdinalIgnoreCase) >= 0;

            if (isSpinner && child.gameObject != button.gameObject)
            {
                spinner = child.gameObject;
                break;
            }
        }
    }

    private void SetLoginButtonLoading(bool loading)
    {
        if (loginButtonSpinner != null) loginButtonSpinner.SetActive(loading);
        if (loginButtonText != null) loginButtonText.gameObject.SetActive(!loading);
        if (loginButton != null) loginButton.interactable = !loading;
    }

    private void SetGuestButtonLoading(bool loading)
    {
        if (guestButtonSpinner != null) guestButtonSpinner.SetActive(loading);
        if (guestButtonText != null) guestButtonText.gameObject.SetActive(!loading);
        if (guestButton != null) guestButton.interactable = !loading && !isAuthenticating;
    }

    private void SetLanguageNextButtonLoading(bool loading)
    {
        if (languageNextButtonSpinner != null) languageNextButtonSpinner.SetActive(loading);
        if (languageNextButtonText != null) languageNextButtonText.gameObject.SetActive(!loading);
        if (languageNextButton != null)
            languageNextButton.interactable = !loading && !string.IsNullOrEmpty(selectedLanguageCode);
    }

    private void SetLetsGoButtonLoading(bool loading)
    {
        if (letsGoButtonSpinner != null) letsGoButtonSpinner.SetActive(loading);
        if (letsGoButtonText != null) letsGoButtonText.gameObject.SetActive(!loading);
        if (letsGoButton != null) letsGoButton.interactable = !loading;
    }

    private void ResolveLoginButtons()
    {
        if (loginPanel == null || loginButton != guestButton) return;

        Debug.LogError("[UserInfoController] Login Button and Guest Button reference the same object. Searching Login Panel for the guest button.");

        Button[] buttons = loginPanel.GetComponentsInChildren<Button>(true);
        foreach (Button button in buttons)
        {
            string buttonName = button.gameObject.name;
            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            string labelText = label != null ? label.text : "";

            bool isGuest = buttonName.IndexOf("guest", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                           labelText.IndexOf("guest", System.StringComparison.OrdinalIgnoreCase) >= 0;

            if (isGuest && button != loginButton)
            {
                guestButton = button;
                Debug.Log($"[UserInfoController] Automatically assigned Guest Button to '{buttonName}'.");
                return;
            }
        }

        guestButton = null;
        Debug.LogError("[UserInfoController] Could not find a separate Guest button. Assign Guest Button correctly in the Inspector.");
    }

    private void ResolveAndBindLetsGoButton()
    {
        if (welcomePanel == null)
        {
            Debug.LogError("[UserInfoController] Welcome Panel is not assigned.");
            return;
        }

        bool assignedButtonIsInWelcomePanel = letsGoButton != null &&
            letsGoButton.transform.IsChildOf(welcomePanel.transform);

        if (!assignedButtonIsInWelcomePanel)
        {
            letsGoButton = null;
            Button[] buttons = welcomePanel.GetComponentsInChildren<Button>(true);

            foreach (Button button in buttons)
            {
                string buttonName = button.gameObject.name;
                TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
                string labelText = label != null ? label.text : "";

                bool isLetsGo = buttonName.IndexOf("lets", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                                buttonName.IndexOf("go", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                                labelText.IndexOf("let's go", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                                labelText.IndexOf("lets go", System.StringComparison.OrdinalIgnoreCase) >= 0;

                if (isLetsGo)
                {
                    letsGoButton = button;
                    break;
                }
            }

            if (letsGoButton == null && buttons.Length == 1)
                letsGoButton = buttons[0];
        }

        if (letsGoButton == null)
        {
            Debug.LogError("[UserInfoController] Could not find the Let's Go button inside Welcome Panel.");
            return;
        }

        letsGoButton.onClick = new Button.ButtonClickedEvent();
        letsGoButton.onClick.AddListener(OnLetsGoPressed);
        ResolveButtonLoadingIndicator(letsGoButton, ref letsGoButtonSpinner, ref letsGoButtonText);
        SetLetsGoButtonLoading(false);
        Debug.Log($"[UserInfoController] Let's Go button bound to '{letsGoButton.gameObject.name}'.");
    }

    private CanvasGroup EnsureCanvasGroup(GameObject panel)
    {
        if (panel == null) return null;
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();
        return cg;
    }

    // -------------------------------------------------------------------------
    // LOGIN STEP
    // -------------------------------------------------------------------------
    private void ShowLoginStep()
    {
        if (languagePanel != null) languagePanel.SetActive(false);
        if (welcomePanel != null) welcomePanel.SetActive(false);

        if (loginPanel != null)
        {
            loginPanel.SetActive(true);
            StartCoroutine(AnimatePanelIn(loginPanel, loginPanelCanvasGroup));
        }

        SetLoginStatus("");
        ShowLoading(false);
        Debug.Log("[UserInfoController] Showing login step");
    }

    private void OnClickLogin()
    {
        if (isAuthenticating) return;

        if (emailInput == null || passwordInput == null)
        {
            Debug.LogError("[UserInfoController] Email or Password input not assigned.");
            return;
        }

        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            SetLoginStatus("Please enter your email and password.");
            return;
        }

        StartCoroutine(LoginCoroutine(email, password));
    }

    private void OnClickGuest()
    {
        if (isAuthenticating) return;
        StartCoroutine(BeginGuestFlow());
    }

    private IEnumerator BeginGuestFlow()
    {
        isAuthenticating = true;
        isGuestMode = false;
        authToken = "";
        refreshToken = "";
        SetGuestButtonLoading(true);
        if (loginButton != null) loginButton.interactable = false;
        SetLoginStatus("Creating guest account...");

        string guestId = System.Guid.NewGuid().ToString().Substring(0, 8);
        string guestEmail = $"guestuser{guestId}@ningorun.com";
        string guestPassword = System.Guid.NewGuid().ToString();
        string languageCode = PlayerPrefs.GetString("user_selected_language", "yoruba");
        if (string.IsNullOrEmpty(languageCode)) languageCode = "yoruba";

        string url = apiBaseUrl.TrimEnd('/') + "/api/auth/signup";
        var reqBody = new SignupRequest
        {
            email = guestEmail,
            password = guestPassword,
            firstName = "Guest",
            lastName = $"User{guestId}",
            selectedLanguage = languageCode,
            selectedAvatar = ""
        };
        string json = JsonUtility.ToJson(reqBody);
        Debug.Log($"[UserInfoController] Guest signup POST {url}");

        using (var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = 15;

            yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            bool hasError = req.result == UnityWebRequest.Result.ConnectionError ||
                            req.result == UnityWebRequest.Result.ProtocolError;
#else
            bool hasError = req.isNetworkError || req.isHttpError;
#endif
            long statusCode = req.responseCode;
            string body = req.downloadHandler != null ? req.downloadHandler.text : "";

            if (hasError)
            {
                Debug.LogError($"[UserInfoController] Guest signup failed ({statusCode}): {req.error}\n{body}");
                FinishGuestSignupFailure(statusCode == 0
                    ? "Could not connect to the server. Check your internet connection and try again."
                    : "Could not create a guest account. Please try again.");
                yield break;
            }

            SignupResponse resp = null;
            try { resp = JsonUtility.FromJson<SignupResponse>(body); }
            catch (System.Exception e) { Debug.LogError("[UserInfoController] Guest signup parse error: " + e.Message); }

            if (resp == null || resp.user == null || string.IsNullOrEmpty(resp.user.id))
            {
                Debug.LogError("[UserInfoController] Guest signup returned an invalid response.");
                FinishGuestSignupFailure("Could not create a guest account. Please try again.");
                yield break;
            }

            LoginResponse login = new LoginResponse
            {
                user = resp.user,
                token = resp.token,
                accessToken = resp.accessToken,
                refreshToken = resp.refreshToken
            };
            StoreLoginResponse(login, isGuest: true);
            SecureCredentials.SaveCredentials(guestEmail, guestPassword, isGuest: true);

            authToken = resp.accessToken ?? resp.token;
            isGuestMode = true;
            PlayerPrefs.SetString("user_mode", "guest");
            PlayerPrefs.Save();

            Debug.Log("[UserInfoController] Guest account created via API");
        }

        SetLoginStatus("");
        isAuthenticating = false;
        SetGuestButtonLoading(false);
        if (loginButton != null) loginButton.interactable = true;
        ShowLanguageStep();
    }

    private void FinishGuestSignupFailure(string message)
    {
        isAuthenticating = false;
        isGuestMode = false;
        SetGuestButtonLoading(false);
        if (loginButton != null) loginButton.interactable = true;
        SetLoginStatus(message);
    }

    private void OnClickSignUp()
    {
        string playStoreUrl = "https://play.google.com/store/apps/details?id=com.ningo.africa.app";
        Application.OpenURL(playStoreUrl);
        Debug.Log("[UserInfoController] Opened store URL for account creation: " + playStoreUrl);
    }

    private IEnumerator LoginCoroutine(string email, string password, int attempt = 0)
    {
        isAuthenticating = true;
        SetLoginButtonLoading(true);
        if (guestButton != null) guestButton.interactable = false;

        string progressMessage = attempt == 0 ? "Logging in..." : "Connection interrupted. Retrying...";
        SetLoginStatus(progressMessage);
        ShowLoading(true, progressMessage);

        string url = apiBaseUrl.TrimEnd('/') + "/api/auth/login";
        var body = new LoginRequest(email, password);
        string json = JsonUtility.ToJson(body);

        using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(jsonBytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;

            yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            bool hasError = request.result == UnityWebRequest.Result.ConnectionError ||
                           request.result == UnityWebRequest.Result.ProtocolError;
#else
            bool hasError = request.isNetworkError || request.isHttpError;
#endif

            ShowLoading(false);

            if (hasError)
            {
                string responseBody = request.downloadHandler != null ? request.downloadHandler.text : "";
                Debug.LogError($"[UserInfoController] Login HTTP {request.responseCode}: {request.error}\nResult: {request.result}\nReachability: {Application.internetReachability}\n{responseBody}");

                if (request.responseCode == 0 && attempt == 0)
                {
                    yield return new WaitForSecondsRealtime(1f);
                    yield return StartCoroutine(LoginCoroutine(email, password, attempt + 1));
                    yield break;
                }

                HandleLoginFailure(request.responseCode == 401
                    ? "Invalid email or password."
                    : request.responseCode == 0
                        ? "Could not connect to the server. Check your internet connection and try again."
                        : "Login failed. Please try again.");
                yield break;
            }

            string responseText = request.downloadHandler.text;
            Debug.Log("[UserInfoController] Login response: " + responseText);

            LoginResponse resp = null;
            try { resp = JsonUtility.FromJson<LoginResponse>(responseText); }
            catch (System.Exception e)
            {
                Debug.LogError("[UserInfoController] Failed to parse login response: " + e.Message);
                HandleLoginFailure("Unexpected server response.");
                yield break;
            }

            if (resp == null)
            {
                HandleLoginFailure("Empty server response.");
                yield break;
            }

            if (!string.IsNullOrEmpty(resp.error))
            {
                HandleLoginFailure(resp.error);
                yield break;
            }

            if (resp.loginType != "regular_user")
            {
                HandleLoginFailure("Only regular user login is supported on mobile right now.");
                yield break;
            }

            if (resp.user == null)
            {
                HandleLoginFailure("Login failed: user data missing.");
                yield break;
            }

            StoreLoginResponse(resp, isGuest: false);
            SecureCredentials.SaveCredentials(email, password, isGuest: false);

            isGuestMode = false;
            SetLoginStatus("Login successful!");
            ShowLoading(true, "Loading resources...");

            string selectedLanguage = PlayerPrefs.GetString("user_selected_language", "yoruba");
            StartCoroutine(FinalizeRegularUser(selectedLanguage, updateServerLanguage: false));
        }
    }

    private void HandleLoginFailure(string message)
    {
        isAuthenticating = false;
        SetLoginButtonLoading(false);
        SetGuestButtonLoading(false);
        if (loginPanel != null) loginPanel.SetActive(true);
        if (languagePanel != null) languagePanel.SetActive(false);
        if (welcomePanel != null) welcomePanel.SetActive(false);
        ShowLoading(false);
        SetLoginStatus(message);
    }

    private void StoreLoginResponse(LoginResponse resp, bool isGuest)
    {
        authToken = resp.accessToken ?? resp.token ?? "";
        refreshToken = resp.refreshToken ?? "";

        if (resp.user != null)
        {
            string displayName = isGuest
                ? "Guest"
                : string.IsNullOrWhiteSpace(resp.user.firstName)
                    ? (resp.user.email ?? "Player")
                    : (resp.user.firstName + " " + resp.user.lastName).Trim();

            PlayerPrefs.SetString("auth_token", authToken);
            PlayerPrefs.SetString("refresh_token", refreshToken);
            PlayerPrefs.SetString("user_id", resp.user.id ?? "");
            PlayerPrefs.SetString("user_email", resp.user.email ?? "");
            PlayerPrefs.SetString("user_first_name", resp.user.firstName ?? "");
            PlayerPrefs.SetString("user_last_name", resp.user.lastName ?? "");
            PlayerPrefs.SetString("user_avatar", resp.user.avatar ?? "");
            PlayerPrefs.SetString("user_selected_language", resp.user.selectedLanguage ?? "");
            PlayerPrefs.SetString("user_points", resp.user.points ?? "0");
            PlayerPrefs.SetString("user_role", resp.user.role ?? "");
            PlayerPrefs.SetString("user_org_id", resp.user.organizationId ?? "");
            PlayerPrefs.SetString("user_mode", isGuest ? "guest" : "regular");
            PlayerPrefs.SetString("user_display_name", displayName);
            PlayerPrefs.Save();
        }
    }

    // -------------------------------------------------------------------------
    // LANGUAGE STEP
    // -------------------------------------------------------------------------
    private void ShowLanguageStep()
    {
        if (loginPanel != null) loginPanel.SetActive(false);
        if (welcomePanel != null) welcomePanel.SetActive(false);

        if (languagePanel != null)
        {
            languagePanel.SetActive(true);
            StartCoroutine(AnimatePanelIn(languagePanel, languagePanelCanvasGroup));
        }

        selectedLanguageCode = "";
        selectedLanguageButton = null;
        ClearLanguageButtons();

        SetLanguageNextButtonLoading(false);
        if (languageBackButton != null)
            languageBackButton.interactable = true;

        SetLanguageLoading(true);

        if (LanguageCache.Instance != null && LanguageCache.Instance.IsReady)
        {
            BuildLanguageButtons(LanguageCache.Instance.Languages);
        }
        else if (LanguageCache.Instance != null)
        {
            LanguageCache.Instance.OnLoaded += OnLanguagesLoaded;
        }
        else
        {
            // No LanguageCache available — fall back to a direct API fetch
            StartCoroutine(FetchLanguagesFallback());
        }

        Debug.Log("[UserInfoController] Showing language step");
    }

    private void OnLanguagesLoaded(LanguageCache.LanguageItem[] languages)
    {
        if (LanguageCache.Instance != null)
            LanguageCache.Instance.OnLoaded -= OnLanguagesLoaded;

        BuildLanguageButtons(languages);
    }

    private IEnumerator FetchLanguagesFallback()
    {
        string url = apiBaseUrl.TrimEnd('/') + "/api/languages";
        using (var request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;
            yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            bool hasError = request.result == UnityWebRequest.Result.ConnectionError ||
                           request.result == UnityWebRequest.Result.ProtocolError;
#else
            bool hasError = request.isNetworkError || request.isHttpError;
#endif

            if (hasError)
            {
                Debug.LogError($"[UserInfoController] Failed to fetch languages: {request.error}");
                BuildLanguageButtons(null);
                yield break;
            }

            try
            {
                LanguagesResponse resp = JsonUtility.FromJson<LanguagesResponse>(request.downloadHandler.text);
                BuildLanguageButtons(resp?.languages);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[UserInfoController] Failed to parse languages: " + e.Message);
                BuildLanguageButtons(null);
            }
        }
    }

    private void BuildLanguageButtons(LanguageCache.LanguageItem[] languages)
    {
        SetLanguageLoading(false);

        if (languageButtonContainer == null || languageButtonPrefab == null)
        {
            Debug.LogError("[UserInfoController] languageButtonContainer or languageButtonPrefab not assigned!");
            SetLanguageStatus("Language UI not configured.");
            return;
        }

        ClearLanguageButtons();

        if (languages == null || languages.Length == 0)
        {
            SetLanguageStatus("No languages available.");
            return;
        }

        string preselected = PlayerPrefs.GetString("user_selected_language", "");

        foreach (var lang in languages)
        {
            string code = lang.language;
            GameObject btnObj = Instantiate(languageButtonPrefab, languageButtonContainer);
            btnObj.name = $"Language_{code}";
            btnObj.SetActive(true);
            spawnedLanguageButtons.Add(btnObj);

            TMP_Text label = btnObj.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = string.IsNullOrEmpty(lang.native_name) ? Capitalize(code) : lang.native_name;

            Button btn = btnObj.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => OnLanguageSelected(code, btnObj));

            if (!string.IsNullOrEmpty(preselected) && code == preselected)
            {
                selectedLanguageButton = btnObj;
                selectedLanguageCode = code;
                ApplyHighlight(btnObj, true);
                if (languageNextButton != null)
                    languageNextButton.interactable = true;
            }
        }

        SetLanguageStatus("");
    }

    private void ClearLanguageButtons()
    {
        foreach (var b in spawnedLanguageButtons)
            if (b != null) Destroy(b);
        spawnedLanguageButtons.Clear();
        selectedLanguageButton = null;
    }

    private void OnLanguageSelected(string code, GameObject btnObj)
    {
        if (selectedLanguageButton != null)
            ApplyHighlight(selectedLanguageButton, false);

        selectedLanguageCode = code;
        selectedLanguageButton = btnObj;
        ApplyHighlight(btnObj, true);

        if (languageNextButton != null)
            languageNextButton.interactable = true;

        Debug.Log($"[UserInfoController] Language selected: {code}");
    }

    private void ApplyHighlight(GameObject btnObj, bool on)
    {
        Image img = btnObj.GetComponent<Image>();
        if (img != null) img.color = on ? HighlightColor : DefaultColor;

        TMP_Text lbl = btnObj.GetComponentInChildren<TMP_Text>();
        if (lbl != null) lbl.color = on ? HighlightTextCol : DefaultTextCol;
    }

    private void OnLanguageBackPressed()
    {
        if (LanguageCache.Instance != null)
            LanguageCache.Instance.OnLoaded -= OnLanguagesLoaded;

        StopAllCoroutines();
        isAuthenticating = false;
        isGuestMode = false;
        SetGuestButtonLoading(false);
        SetLanguageNextButtonLoading(false);
        if (languageBackButton != null) languageBackButton.interactable = true;
        if (loginButton != null) loginButton.interactable = true;
        ShowLoginStep();
    }

    private void OnLanguageNextPressed()
    {
        if (string.IsNullOrEmpty(selectedLanguageCode))
        {
            Debug.LogWarning("[UserInfoController] No language selected");
            return;
        }

        PlayerPrefs.SetString("user_selected_language", selectedLanguageCode);
        PlayerPrefs.Save();

        SetLanguageNextButtonLoading(true);
        if (languageBackButton != null)
            languageBackButton.interactable = false;

        ShowLoading(true, "Setting up your account...");

        if (isGuestMode)
            StartCoroutine(FinalizeGuest(selectedLanguageCode));
        else
            StartCoroutine(FinalizeRegularUser(selectedLanguageCode));
    }

    private IEnumerator FinalizeRegularUser(string languageCode, bool updateServerLanguage = true)
    {
        // Try to update the user's selected language on the server only if they
        // actively picked a different language in the language panel.
        if (updateServerLanguage && LanguageAPI.Instance != null)
        {
            bool updateDone = false;
            LanguageAPI.Instance.UpdateUserLanguage(languageCode, success => updateDone = true, fetchDictionary: false);

            float timeout = 5f;
            while (!updateDone && timeout > 0f)
            {
                timeout -= Time.unscaledDeltaTime;
                yield return null;
            }
        }

        yield return StartCoroutine(FetchAndStoreDictionary(authToken, languageCode));

        ShowLoading(false);
        ShowWelcomeStep();
    }

    private IEnumerator FinalizeGuest(string languageCode)
    {
        if (string.IsNullOrEmpty(authToken))
            authToken = PlayerPrefs.GetString("auth_token", "");

        if (string.IsNullOrEmpty(authToken))
            Debug.LogWarning("[UserInfoController] Guest account has no auth token; dictionary will use cached/offline data if available.");

        yield return StartCoroutine(FinalizeRegularUser(languageCode, updateServerLanguage: true));
    }

    private IEnumerator FetchAndStoreDictionary(string token, string language)
    {
        if (string.IsNullOrEmpty(token))
        {
            Debug.LogWarning("[UserInfoController] No auth token available for dictionary fetch");
            yield break;
        }

        string url = apiBaseUrl.TrimEnd('/') + $"/api/dictionary?language={language}&limit=1000";
        ShowLoading(true, "Loading dictionary...");

        using (var request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", $"Bearer {token}");
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 15;

            yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            bool hasError = request.result == UnityWebRequest.Result.ConnectionError ||
                           request.result == UnityWebRequest.Result.ProtocolError;
#else
            bool hasError = request.isNetworkError || request.isHttpError;
#endif

            if (hasError)
            {
                Debug.LogError($"[UserInfoController] Failed to fetch dictionary: {request.error}");
                yield break;
            }

            string responseText = request.downloadHandler.text;
            PlayerPrefs.SetString("dictionary_data", responseText);
            PlayerPrefs.SetString("dictionary_language", language);
            PlayerPrefs.SetString("dictionary_fetch_time", System.DateTime.UtcNow.ToString("o"));
            PlayerPrefs.Save();
            DictionaryManager.InvalidateMemoryCache();

            Debug.Log($"[UserInfoController] Dictionary stored for language: {language}");
        }

        ShowLoading(false);
    }

    // -------------------------------------------------------------------------
    // WELCOME STEP
    // -------------------------------------------------------------------------
    private void ShowWelcomeStep()
    {
        isAuthenticating = false;
        SetLoginButtonLoading(false);
        SetGuestButtonLoading(false);

        if (loginPanel != null) loginPanel.SetActive(false);
        if (languagePanel != null) languagePanel.SetActive(false);

        if (welcomePanel != null)
        {
            welcomePanel.SetActive(true);
            ResolveAndBindLetsGoButton();
            StartCoroutine(AnimatePanelIn(welcomePanel, welcomePanelCanvasGroup));
        }

        Debug.Log("[UserInfoController] Showing welcome step");
    }

    private void OnLetsGoPressed()
    {
        PlayerPrefs.SetInt("user_info_completed", 1);
        PlayerPrefs.Save();

        Debug.Log("[UserInfoController] User info completed. Loading home.");

        SetLetsGoButtonLoading(true);
        StartCoroutine(LoadHomeScene());
    }

    private IEnumerator LoadHomeScene()
    {
        yield return new WaitForSeconds(0.3f);

        if (!string.IsNullOrEmpty(homeSceneName))
        {
            if (SceneLoader.Instance != null)
                SceneLoader.Instance.LoadHomeScene();
            else
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(homeSceneName);
        }
    }

    // -------------------------------------------------------------------------
    // UI HELPERS
    // -------------------------------------------------------------------------
    private void SetLoginStatus(string message)
    {
        if (loginStatusText != null)
        {
            loginStatusText.text = message;
            loginStatusText.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }

        if (!string.IsNullOrEmpty(message))
            Debug.Log("[UserInfoController] " + message);
    }

    private void SetLanguageLoading(bool show)
    {
        if (languageLoadingView != null)
            languageLoadingView.SetActive(show);

        if (languageStatusText != null)
        {
            languageStatusText.text = show ? "Loading languages..." : "";
            languageStatusText.gameObject.SetActive(show);
        }
    }

    private void SetLanguageStatus(string message)
    {
        if (languageStatusText != null)
        {
            languageStatusText.text = message;
            languageStatusText.gameObject.SetActive(!string.IsNullOrEmpty(message));
        }
    }

    private void ShowLoading(bool show, string message = null)
    {
        if (loadingView != null)
        {
            loadingView.SetActive(show);
            if (show && loadingStatusLabel != null && !string.IsNullOrEmpty(message))
                loadingStatusLabel.text = message;
        }
    }

    private IEnumerator AnimatePanelIn(GameObject panel, CanvasGroup cg)
    {
        if (panel == null) yield break;

        RectTransform rt = panel.GetComponent<RectTransform>();
        Vector3 targetScale = rt != null ? rt.localScale : Vector3.one;
        Vector3 startScale = targetScale * panelStartScale;

        if (cg != null) cg.alpha = 0f;
        if (rt != null) rt.localScale = startScale;

        float elapsed = 0f;
        while (elapsed < panelFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / panelFadeInDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            if (cg != null) cg.alpha = eased;
            if (rt != null) rt.localScale = Vector3.Lerp(startScale, targetScale, eased);

            yield return null;
        }

        if (cg != null) cg.alpha = 1f;
        if (rt != null) rt.localScale = targetScale;
    }

    private static string Capitalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        return char.ToUpper(s[0]) + s.Substring(1);
    }

    // -------------------------------------------------------------------------
    // PUBLIC HELPERS
    // -------------------------------------------------------------------------
    public static bool IsUserInfoCompleted()
    {
        return PlayerPrefs.GetInt("user_info_completed", 0) == 1;
    }

    public static string GetUsername()
    {
        return PlayerPrefs.GetString("user_display_name", "Player");
    }

    public static string GetSelectedLanguage()
    {
        return PlayerPrefs.GetString("user_selected_language", "");
    }

    // -------------------------------------------------------------------------
    // DATA TRANSFER OBJECTS
    // -------------------------------------------------------------------------
    [System.Serializable]
    private class LoginUser
    {
        public string id;
        public string email;
        public string firstName;
        public string lastName;
        public string avatar;
        public string selectedLanguage;
        public string points;
        public string role;
        public string organizationId;
        public bool isAuthenticated;
    }

    [System.Serializable]
    private class LoginResponse
    {
        public string loginType;
        public LoginUser user;
        public string token;
        public string accessToken;
        public string refreshToken;
        public string error;
    }

    [System.Serializable]
    private class LoginRequest
    {
        public string email;
        public string password;

        public LoginRequest(string email, string password)
        {
            this.email = email;
            this.password = password;
        }
    }

    [System.Serializable]
    private class SignupRequest
    {
        public string email;
        public string password;
        public string firstName;
        public string lastName;
        public string selectedLanguage;
        public string selectedAvatar;
    }

    [System.Serializable]
    private class SignupResponse
    {
        public LoginUser user;
        public string token;
        public string accessToken;
        public string refreshToken;
    }

    [System.Serializable]
    private class LanguagesResponse
    {
        public bool success;
        public LanguageCache.LanguageItem[] languages;
    }
}

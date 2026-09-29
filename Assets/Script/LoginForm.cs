using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;

public class LoginForm : MonoBehaviour
{
    [Header("API")]
    [Tooltip("Base URL of the Ningo production API (no trailing slash). Example: https://ningoafrica.app")] 
    [SerializeField] private string apiBaseUrl = "https://ningoafrica.app";

    [Header("UI Refs")]
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_Text statusLabel;
    [SerializeField] private GameObject loginFormPanel;
    [SerializeField] private GameObject languageSelectionModal;

    [Header("Scenes")]
    [SerializeField] private string homeSceneName = "Home";

    [Header("Guest Mode")]
    [SerializeField] private LanguageSelectionUI languageSelectionUI;

    [Header("Loading_view (shared loader for any API call)")]
    [Tooltip("Optional. If empty, falls back to SplashController.Instance.")]
    [SerializeField] private GameObject loadingView;
    [SerializeField] private TMP_Text loadingStatusLabel;

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
        public string error; // in case API returns an error field
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

    void Start()
    {
        if (languageSelectionModal != null) languageSelectionModal.SetActive(false);
        ShowLoadingView(false);
        Debug.Log($"[LoginForm] Initialized. API: {apiBaseUrl}");
    }

    public void ShowLoginPanel()
    {
        if (loginFormPanel != null) loginFormPanel.SetActive(true);
        if (languageSelectionModal != null) languageSelectionModal.SetActive(false);
        ShowLoadingView(false);
        SetStatus("");
    }

    public void OnClickLogin()
    {
        if (emailInput == null || passwordInput == null)
        {
            Debug.LogError("[LoginForm] Email or Password input not assigned.");
            return;
        }

        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(password))
        {
            SetStatus("Please enter your email and password.");
            return;
        }

        if (string.IsNullOrEmpty(email))
        {
            SetStatus("Please enter your email.");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            SetStatus("Please enter your password.");
            return;
        }

        StartCoroutine(LoginCoroutine(email, password));
    }

    public void OnClickSignUp()
    {
        string playStoreUrl = "https://play.google.com/store/apps/details?id=com.ningo.africa.app";
#if UNITY_ANDROID
        Application.OpenURL(playStoreUrl);
#elif UNITY_IOS
        // Replace with actual App Store URL when available; fallback to Play Store page for now
        Application.OpenURL(playStoreUrl);
#else
        Application.OpenURL(playStoreUrl);
#endif
        Debug.Log("[LoginForm] Opened store URL for account creation: " + playStoreUrl);
    }

    public void OnClickGuestLogin()
    {
        Debug.Log("[LoginForm] OnClickGuestLogin called - Starting guest login flow");
        SetStatus("Setting up guest mode...");
        
        int randomPoints = Random.Range(0, 100);
        
        PlayerPrefs.SetString("user_mode", "guest");
        PlayerPrefs.SetString("user_points", randomPoints.ToString());
        PlayerPrefs.SetString("user_id", "guest_" + System.Guid.NewGuid().ToString());
        PlayerPrefs.SetString("user_email", "guest@ningo.local");
        PlayerPrefs.SetString("user_first_name", "Guest");
        PlayerPrefs.SetString("user_last_name", "User");
        PlayerPrefs.SetString("user_role", "guest");
        PlayerPrefs.Save();

        // Route view switching through SplashController when available
        if (SplashController.Instance != null) SplashController.Instance.ShowLanguage();
        if (loginFormPanel != null) loginFormPanel.SetActive(false);
        if (languageSelectionModal != null) languageSelectionModal.SetActive(true);

        ShowLoadingView(true, "Loading languages...");
        StartCoroutine(FetchLanguagesAndShowModal());
}

    private IEnumerator FetchLanguagesAndShowModal()
    {
        if (!CheckInternetConnection())
        {
            Debug.LogWarning("[LoginForm] No internet connection for language fetch");
            SetDefaultLanguageAndContinue();
            yield break;
        }

        string url = apiBaseUrl.TrimEnd('/') + "/api/languages";
        Debug.Log($"[LoginForm] Fetching languages from: {url}");

        using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET))
        {
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;

            yield return request.SendWebRequest();

            ShowLoadingView(false);

#if UNITY_2020_1_OR_NEWER
            bool hasError = request.result == UnityWebRequest.Result.ConnectionError ||
                           request.result == UnityWebRequest.Result.ProtocolError;
#else
            bool hasError = request.isNetworkError || request.isHttpError;
#endif

            if (hasError)
            {
                Debug.LogError($"[LoginForm] Failed to fetch languages from {url}");
                Debug.LogError($"[LoginForm] Error: {request.error}");
                Debug.LogError($"[LoginForm] Response Code: {request.responseCode}");
                Debug.LogError($"[LoginForm] Response: {request.downloadHandler?.text}");
                SetDefaultLanguageAndContinue();
                yield break;
            }

            string responseText = request.downloadHandler.text;
            Debug.Log("[LoginForm] Languages response: " + responseText);

            try
            {
                LanguagesResponse response = JsonUtility.FromJson<LanguagesResponse>(responseText);
                
                if (response != null && response.success && response.languages != null && response.languages.Length > 0)
                {
                    PopulateLanguageButtons(response.languages);
                }
                else
                {
                    SetDefaultLanguageAndContinue();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("[LoginForm] Failed to parse languages: " + e.Message);
                SetDefaultLanguageAndContinue();
            }
        }
    }

    private void PopulateLanguageButtons(LanguageItem[] languages)
    {
        if (languageSelectionUI != null)
        {
            languageSelectionUI.SetLanguages(languages);
        }
    }

    private void SetDefaultLanguageAndContinue()
    {
        PlayerPrefs.SetString("user_selected_language", "english");
        PlayerPrefs.Save();
        ProceedAfterAuth();
    }

    private void ProceedAfterAuth()
    {
        if (SplashController.Instance != null)
            SplashController.Instance.ShowWelcome();
        else if (!string.IsNullOrEmpty(homeSceneName))
            SceneManager.LoadSceneAsync(homeSceneName);
    }

    public void OnLanguageSelected(string languageCode)
    {
        PlayerPrefs.SetString("user_selected_language", languageCode);
        PlayerPrefs.Save();

        if (languageSelectionModal != null) languageSelectionModal.SetActive(false);

        ShowLoadingView(true, "Setting up your account...");
        StartCoroutine(CreateGuestAccountAndLogin(languageCode));
    }

    /// <summary>
    /// Creates a guest account through the Ningo API: POST {apiBaseUrl}/api/auth/signup
    /// </summary>
    private IEnumerator CreateGuestAccountAndLogin(string languageCode)
    {
        if (!CheckInternetConnection())
        {
            ShowLoadingView(true, "No internet. Loading offline...");
            yield return new WaitForSeconds(1f);
            SetDefaultLanguageAndContinue();
            yield break;
        }

        string guestId = System.Guid.NewGuid().ToString().Substring(0, 8);
        string guestEmail = $"guestuser{guestId}@ningorun.com";
        string guestPassword = System.Guid.NewGuid().ToString();
        string guestFirstName = "Guest";
        string guestLastName = $"User{guestId}";

        string url = apiBaseUrl.TrimEnd('/') + "/api/auth/signup";
        var reqBody = new SignupRequest
        {
            email = guestEmail,
            password = guestPassword,
            firstName = guestFirstName,
            lastName = guestLastName,
            selectedLanguage = languageCode,
            selectedAvatar = ""
        };
        string json = JsonUtility.ToJson(reqBody);

        Debug.Log($"[LoginForm] Signup POST {url} body={json}");

        string accessToken = null;
        string refreshToken = null;
        string userId = null;
        string userEmail = guestEmail;

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
            long code = req.responseCode;
            string body = req.downloadHandler != null ? req.downloadHandler.text : "";

            Debug.Log($"[LoginForm] Signup HTTP {code}: {body}");

            if (hasError)
            {
                string apiError = TryExtractError(body);
                string shown = $"Signup failed ({code}): " + (string.IsNullOrEmpty(apiError) ? req.error : apiError);
                Debug.LogError("[LoginForm] " + shown);
                ShowLoadingView(true, shown);
                yield return new WaitForSeconds(3f);
                SetDefaultLanguageAndContinue();
                yield break;
            }

            SignupResponse resp = null;
            try { resp = JsonUtility.FromJson<SignupResponse>(body); }
            catch (System.Exception e) { Debug.LogError("[LoginForm] Signup parse error: " + e.Message); }

            if (resp == null || resp.user == null || string.IsNullOrEmpty(resp.user.id))
            {
                ShowLoadingView(true, "Signup error: invalid response");
                yield return new WaitForSeconds(3f);
                SetDefaultLanguageAndContinue();
                yield break;
            }

            accessToken = resp.accessToken;
            refreshToken = resp.refreshToken;
            userId = resp.user.id;
            if (!string.IsNullOrEmpty(resp.user.email)) userEmail = resp.user.email;
        }

        PlayerPrefs.SetString("auth_token", accessToken ?? "");
        PlayerPrefs.SetString("refresh_token", refreshToken ?? "");
        PlayerPrefs.SetString("user_id", userId);
        PlayerPrefs.SetString("user_email", userEmail);
        PlayerPrefs.SetString("user_first_name", guestFirstName);
        PlayerPrefs.SetString("user_last_name", guestLastName);
        PlayerPrefs.SetString("user_avatar", "");
        PlayerPrefs.SetString("user_selected_language", languageCode);
        PlayerPrefs.SetString("user_points", "0");
        PlayerPrefs.SetString("user_role", "student");
        PlayerPrefs.SetString("user_mode", "guest");
        PlayerPrefs.Save();

        // Save encrypted guest credentials for auto re-login
        SecureCredentials.SaveCredentials(guestEmail, guestPassword, isGuest: true);

        Debug.Log("[LoginForm] Guest account created via API");

        ShowLoadingView(true, "Loading resources...");
        yield return StartCoroutine(FetchAndStoreDictionary(accessToken, languageCode));

        ShowLoadingView(true, "Success! Loading home...");
        yield return new WaitForSeconds(0.5f);
        ShowLoadingView(false);
        ProceedAfterAuth();
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
        public string accessToken;
        public string refreshToken;
    }

    [System.Serializable]
    private class ApiErrorResponse
    {
        public string error;
        public string message;
        public string code;
        public string details;
    }

    /// <summary>
    /// Try to pull a readable error string out of an API JSON body.
    /// Looks at "error", "message", "details" fields. Returns empty if none.
    /// </summary>
    private string TryExtractError(string body)
    {
        if (string.IsNullOrEmpty(body)) return "";
        try
        {
            var err = JsonUtility.FromJson<ApiErrorResponse>(body);
            if (err == null) return "";
            if (!string.IsNullOrEmpty(err.error)) return err.error;
            if (!string.IsNullOrEmpty(err.message)) return err.message;
            if (!string.IsNullOrEmpty(err.details)) return err.details;
        }
        catch { /* not JSON or unexpected shape */ }
        return "";
    }

    [System.Serializable]
    private class LanguagesResponse
    {
        public bool success;
        public LanguageItem[] languages;
    }

    [System.Serializable]
    public class LanguageItem
    {
        public string id;
        public string language;
        public string native_name;
        public string flag_code;
        public bool is_active;
    }

    private IEnumerator LoginCoroutine(string email, string password)
    {
        if (!CheckInternetConnection())
        {
            SetStatus("No internet connection. Please check your network.");
            Debug.LogError("[LoginForm] No internet connection available");
            yield break;
        }

        SetStatus("Logging in...");
        ShowLoadingView(true, "Logging in...");

        if (string.IsNullOrEmpty(apiBaseUrl))
        {
            SetStatus("API URL not configured.");
            Debug.LogError("[LoginForm] apiBaseUrl is empty.");
            ShowLoadingView(false);
            yield break;
        }

        string url = apiBaseUrl.TrimEnd('/') + "/api/auth/login";
        Debug.Log($"[LoginForm] Attempting login to: {url}");

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
            bool hasNetworkError = request.result == UnityWebRequest.Result.ConnectionError ||
                                    request.result == UnityWebRequest.Result.ProtocolError;
#else
            bool hasNetworkError = request.isNetworkError || request.isHttpError;
#endif

            if (hasNetworkError)
            {
                Debug.LogError($"[LoginForm] HTTP error: {request.responseCode} - {request.error}\nBody: {request.downloadHandler.text}");
                SetStatus("Login failed. Check your connection or credentials.");
                ShowLoadingView(false);
                yield break;
            }

            string responseText = request.downloadHandler.text;
            Debug.Log("[LoginForm] Response: " + responseText);

            LoginResponse resp = null;
            try
            {
                resp = JsonUtility.FromJson<LoginResponse>(responseText);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[LoginForm] Failed to parse login response: " + e.Message);
                SetStatus("Unexpected server response.");
                ShowLoadingView(false);
                yield break;
            }

            if (resp == null)
            {
                SetStatus("Empty server response.");
                ShowLoadingView(false);
                yield break;
            }

            // If API sent an error field
            if (!string.IsNullOrEmpty(resp.error))
            {
                SetStatus(resp.error);
                ShowLoadingView(false);
                yield break;
            }

            // Only handle regular users for now
            if (resp.loginType != "regular_user")
            {
                SetStatus("Only regular user login is supported on mobile right now.");
                ShowLoadingView(false);
                yield break;
            }

            if (resp.user == null)
            {
                SetStatus("Login failed: user data missing.");
                ShowLoadingView(false);
                yield break;
            }

            // Store in PlayerPrefs
            PlayerPrefs.SetString("auth_token", resp.accessToken ?? resp.token ?? "");
            PlayerPrefs.SetString("refresh_token", resp.refreshToken ?? "");

            PlayerPrefs.SetString("user_id", resp.user.id ?? "");
            PlayerPrefs.SetString("user_email", resp.user.email ?? "");
            PlayerPrefs.SetString("user_first_name", resp.user.firstName ?? "");
            PlayerPrefs.SetString("user_last_name", resp.user.lastName ?? "");
            PlayerPrefs.SetString("user_avatar", resp.user.avatar ?? "");
            PlayerPrefs.SetString("user_selected_language", resp.user.selectedLanguage ?? "");
            PlayerPrefs.SetString("user_points", resp.user.points ?? "0");
            PlayerPrefs.SetString("user_role", resp.user.role ?? "");
            PlayerPrefs.SetString("user_org_id", resp.user.organizationId ?? "");

            PlayerPrefs.Save();

            // Save encrypted credentials for auto re-login on token expiry
            SecureCredentials.SaveCredentials(email, password, isGuest: false);

            SetStatus("Login successful! Loading resources...");

            yield return StartCoroutine(FetchAndStoreDictionary(resp.accessToken ?? resp.token ?? "", resp.user.selectedLanguage ?? "yoruba"));

            ShowLoadingView(false);
            ProceedAfterAuth();
        }
    }

    private void SetStatus(string message)
    {
        if (statusLabel != null)
        {
            statusLabel.text = message;
        }
        Debug.Log("[LoginForm] " + message);
    }

    /// <summary>
    /// Toggle the shared Loading_view. Falls back to SplashController.Instance if not wired here.
    /// </summary>
    private void ShowLoadingView(bool show, string message = null)
    {
        if (loadingView != null)
        {
            loadingView.SetActive(show);
            if (show && loadingStatusLabel != null && !string.IsNullOrEmpty(message))
                loadingStatusLabel.text = message;
            return;
        }

        if (SplashController.Instance != null)
            SplashController.Instance.ShowLoading(show, message);
    }

    private IEnumerator FetchAndStoreDictionary(string authToken, string language)
    {
        if (string.IsNullOrEmpty(authToken))
        {
            Debug.LogWarning("[LoginForm] No auth token available for dictionary fetch");
            yield break;
        }

        if (!CheckInternetConnection())
        {
            Debug.LogWarning("[LoginForm] No internet connection for dictionary fetch");
            yield break;
        }

        string url = apiBaseUrl.TrimEnd('/') + $"/api/dictionary?language={language}&limit=1000";
        Debug.Log($"[LoginForm] Fetching dictionary from: {url}");

        ShowLoadingView(true, "Loading dictionary...");

        using (var request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");
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
                Debug.LogError($"[LoginForm] Failed to fetch dictionary: {request.error}");
                yield break;
            }

            string responseText = request.downloadHandler.text;
            Debug.Log($"[LoginForm] Dictionary response: {responseText.Substring(0, Mathf.Min(200, responseText.Length))}...");

            PlayerPrefs.SetString("dictionary_data", responseText);
            PlayerPrefs.SetString("dictionary_language", language);
            PlayerPrefs.SetString("dictionary_fetch_time", System.DateTime.UtcNow.ToString("o"));
            PlayerPrefs.Save();

            Debug.Log($"[LoginForm] Dictionary data stored successfully for language: {language}");
        }

        ShowLoadingView(false);
    }

    private bool CheckInternetConnection()
    {
        NetworkReachability reachability = Application.internetReachability;
        
        if (reachability == NetworkReachability.NotReachable)
        {
            // Application.internetReachability is unreliable on some Android devices
            // (especially Samsung). Log a warning but still return true so the actual
            // HTTP request can proceed — it will fail with a proper error if offline.
            Debug.LogWarning($"[LoginForm] internetReachability reports NotReachable — attempting request anyway (may be a false negative on this device)");
        }
        
        return true;
    }
}

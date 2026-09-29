using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class LanguageAPI : MonoBehaviour
{
    public static LanguageAPI Instance { get; private set; }
    
    [Header("API Configuration")]
    [SerializeField] private string apiBaseUrl = "https://ningoafrica.app";
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoBootstrap()
    {
        if (Instance != null) return;
        new GameObject("LanguageAPI").AddComponent<LanguageAPI>();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning($"[LanguageAPI] Duplicate instance detected on {gameObject.name} — destroying.");
            Destroy(gameObject);
        }
    }

    private IEnumerator Start()
    {
        // Wait a couple of frames so other systems initialise first
        yield return new WaitForSeconds(2f);

        // Auto-heal: if the user changed language while offline last session,
        // the preference and the cached dictionary are out of sync — fix it now.
        string selectedLang = PlayerPrefs.GetString("user_selected_language", "");
        string dictLang     = PlayerPrefs.GetString("dictionary_language", "");

        bool mismatch = !string.IsNullOrEmpty(selectedLang) && selectedLang != dictLang;
        bool online   = Application.internetReachability != NetworkReachability.NotReachable;

        if (mismatch && online)
        {
            Debug.Log($"[LanguageAPI] Auto-healing dictionary mismatch: stored={dictLang}, selected={selectedLang}");
            string authToken = PlayerPrefs.GetString("auth_token", "");
            bool[] r = { false };
            yield return StartCoroutine(FetchDictionary(authToken, selectedLang, r));
            if (r[0]) DictionaryManager.InvalidateMemoryCache();
            Debug.Log($"[LanguageAPI] Auto-heal complete. Dictionary fetched: {r[0]}");
        }
    }
    
    public void UpdateUserLanguage(string languageCode, System.Action<bool> onComplete = null, bool fetchDictionary = true)
    {
        StartCoroutine(UpdateUserLanguageCoroutine(languageCode, onComplete, fetchDictionary));
    }
    
    private IEnumerator UpdateUserLanguageCoroutine(string languageCode, System.Action<bool> onComplete, bool fetchDictionary)
    {
        // Bail immediately if offline — preference is already saved in PlayerPrefs;
        // the auto-heal in Start() will fetch the dictionary next time with internet.
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogWarning("[LanguageAPI] Offline — language saved locally, dictionary will sync on reconnect.");
            onComplete?.Invoke(false);
            yield break;
        }

        string authToken = PlayerPrefs.GetString("auth_token", "");
        bool hasAuth = !string.IsNullOrEmpty(authToken);

        if (!hasAuth)
        {
            Debug.LogWarning("[LanguageAPI] No auth token — skipping PATCH, fetching dictionary only.");
            bool[] r = { false };
            yield return StartCoroutine(FetchDictionary("", languageCode, r));
            onComplete?.Invoke(r[0]);
            yield break;
        }
        
        // Update user language via API
        string url = apiBaseUrl.TrimEnd('/') + "/api/user/language";
        string jsonData = $"{{\"language\":\"{languageCode}\"}}";
        
        Debug.Log($"[LanguageAPI] Updating user language to: {languageCode}");
        
        using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");
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
                long updateCode = request.responseCode;
                Debug.LogWarning($"[LanguageAPI] Language update failed ({request.error}, code {updateCode})");
                
                if (updateCode == 401)
                {
                    // Token expired — no auto re-login in simplified build
                    Debug.LogWarning("[LanguageAPI] 401 on language update — re-login not available, continuing offline.");
                    ToastManager.Show("Playing offline. Language change saved locally.");
                    onComplete?.Invoke(false);
                    yield break;
                }
                
                // Other errors — still try to fetch dictionary
                bool[] r = { false };
                yield return StartCoroutine(FetchDictionary(authToken, languageCode, r));
                onComplete?.Invoke(r[0]);
                yield break;
            }
            
            Debug.Log($"[LanguageAPI] User language updated successfully to: {languageCode}");
            
            if (!fetchDictionary)
            {
                PlayerPrefs.SetString("user_selected_language", languageCode);
                PlayerPrefs.Save();
                onComplete?.Invoke(true);
                yield break;
            }
            
            // Fetch new dictionary for the selected language
            bool[] res = { false };
            yield return StartCoroutine(FetchDictionary(authToken, languageCode, res));
            
            if (res[0])
            {
                PlayerPrefs.SetString("user_selected_language", languageCode);
                PlayerPrefs.Save();
            }
            
            onComplete?.Invoke(res[0]);
        }
    }
    
    private IEnumerator FetchDictionary(string authToken, string language, bool[] success = null)
    {
        string url = apiBaseUrl.TrimEnd('/') + $"/api/dictionary?language={language}&limit=1000";
        Debug.Log($"[LanguageAPI] Fetching dictionary from: {url}");
        
        using (var request = UnityWebRequest.Get(url))
        {
            if (!string.IsNullOrEmpty(authToken))
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
                long code = request.responseCode;
                if (code == 400)
                {
                    Debug.LogWarning($"[LanguageAPI] Dictionary not available for '{language}' (400)");
                    ToastManager.Show($"\"{language}\" dictionary not available yet");
                }
                else if (code == 401)
                {
                    Debug.LogWarning("[LanguageAPI] Dictionary fetch 401 — re-login not available, using cached dictionary.");
                    ToastManager.Show("Playing offline. Using cached words.");
                }
                else
                {
                    Debug.LogError($"[LanguageAPI] Failed to fetch dictionary: {request.error}");
                }
                if (success != null) success[0] = false;
                yield break;
            }
            
            string responseText = request.downloadHandler.text;
            Debug.Log($"[LanguageAPI] Dictionary fetched successfully for language: {language}");
            
            // Store dictionary data
            PlayerPrefs.SetString("dictionary_data", responseText);
            PlayerPrefs.SetString("dictionary_language", language);
            PlayerPrefs.SetString("dictionary_fetch_time", System.DateTime.UtcNow.ToString("o"));
            PlayerPrefs.Save();
            
            Debug.Log($"[LanguageAPI] Dictionary data stored successfully");
            if (success != null) success[0] = true;
        }
    }
    
    public string GetCurrentLanguage()
    {
        return PlayerPrefs.GetString("user_selected_language", "yoruba");
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Fetches languages from the API once in the background and caches them
/// in memory + PlayerPrefs (24-hour TTL).  Lives across scene loads.
/// 
/// Usage:
///   LanguageCache.Instance.IsReady         -> true once data available
///   LanguageCache.Instance.Languages       -> the cached array
///   LanguageCache.Instance.OnLoaded += cb  -> subscribe for async notification
/// </summary>
public class LanguageCache : MonoBehaviour
{
    public static LanguageCache Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoBootstrap()
    {
        if (Instance != null) return;
        new GameObject("LanguageCache").AddComponent<LanguageCache>();
    }

    // -----------------------------------------------------------------------
    // Types
    // -----------------------------------------------------------------------
    [System.Serializable]
    public class LanguageItem
    {
        public string id;
        public string language;
        public string native_name;
    }

    [System.Serializable]
    private class LanguagesResponse
    {
        public bool success;
        public LanguageItem[] languages;
    }

    [System.Serializable]
    private class CacheWrapper
    {
        public LanguageItem[] items;
    }

    // -----------------------------------------------------------------------
    // Config
    // -----------------------------------------------------------------------
    [SerializeField] private string apiBaseUrl = "https://ningoafrica.app";
    [SerializeField] private float  fetchDelaySeconds = 2f;   // wait before background fetch
    [SerializeField] private float  cacheTTLHours     = 24f;  // how long PlayerPrefs cache is valid

    private const string CacheDataKey = "language_cache_json";
    private const string CacheTimeKey = "language_cache_time";

    // -----------------------------------------------------------------------
    // State
    // -----------------------------------------------------------------------
    public LanguageItem[] Languages { get; private set; }
    public bool IsReady { get; private set; }

    /// <summary>Fired (on main thread) when languages become available.</summary>
    public event System.Action<LanguageItem[]> OnLoaded;

    // -----------------------------------------------------------------------
    // Lifecycle
    // -----------------------------------------------------------------------
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Try to serve from disk cache immediately (no network needed)
        if (TryLoadFromDiskCache())
        {
            Debug.Log($"[LanguageCache] Loaded {Languages.Length} languages from cache.");
        }

        // Always kick off a background refresh to keep cache fresh
        StartCoroutine(BackgroundFetch());
    }

    // -----------------------------------------------------------------------
    // Disk cache
    // -----------------------------------------------------------------------
    private bool TryLoadFromDiskCache()
    {
        string json = PlayerPrefs.GetString(CacheDataKey, "");
        if (string.IsNullOrEmpty(json)) return false;

        // Check TTL
        string timeStr = PlayerPrefs.GetString(CacheTimeKey, "");
        if (!string.IsNullOrEmpty(timeStr) &&
            System.DateTime.TryParse(timeStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out System.DateTime saved))
        {
            if ((System.DateTime.UtcNow - saved).TotalHours > cacheTTLHours)
            {
                Debug.Log("[LanguageCache] Disk cache expired — will refresh in background.");
                return false;
            }
        }

        try
        {
            CacheWrapper wrapper = JsonUtility.FromJson<CacheWrapper>(json);
            if (wrapper?.items != null && wrapper.items.Length > 0)
            {
                Languages = wrapper.items;
                IsReady = true;
                return true;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[LanguageCache] Failed to parse disk cache: {e.Message}");
        }
        return false;
    }

    private void WriteToDiskCache(LanguageItem[] items)
    {
        try
        {
            string json = JsonUtility.ToJson(new CacheWrapper { items = items });
            PlayerPrefs.SetString(CacheDataKey, json);
            PlayerPrefs.SetString(CacheTimeKey, System.DateTime.UtcNow.ToString("o"));
            PlayerPrefs.Save();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[LanguageCache] Failed to write disk cache: {e.Message}");
        }
    }

    // -----------------------------------------------------------------------
    // Background fetch
    // -----------------------------------------------------------------------
    private IEnumerator BackgroundFetch()
    {
        // Small delay — let scene finish initialising first
        yield return new WaitForSeconds(fetchDelaySeconds);

        string url = apiBaseUrl.TrimEnd('/') + "/api/languages";
        Debug.Log($"[LanguageCache] Background fetch: {url}");

        using (var req = UnityWebRequest.Get(url))
        {
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = 10;
            yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            bool hasError = req.result == UnityWebRequest.Result.ConnectionError ||
                            req.result == UnityWebRequest.Result.ProtocolError;
#else
            bool hasError = req.isNetworkError || req.isHttpError;
#endif
            if (hasError)
            {
                Debug.LogWarning($"[LanguageCache] Background fetch failed: {req.error}");
                yield break;
            }

            try
            {
                LanguagesResponse resp = JsonUtility.FromJson<LanguagesResponse>(req.downloadHandler.text);
                if (resp != null && resp.success && resp.languages != null && resp.languages.Length > 0)
                {
                    Languages = resp.languages;
                    IsReady   = true;
                    WriteToDiskCache(Languages);
                    Debug.Log($"[LanguageCache] Fetched and cached {Languages.Length} languages.");
                    OnLoaded?.Invoke(Languages);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[LanguageCache] Parse error: {e.Message}");
            }
        }
    }

    // -----------------------------------------------------------------------
    // Public helpers
    // -----------------------------------------------------------------------

    /// <summary>Force-clear the disk cache (e.g. after a language update).</summary>
    public void InvalidateCache()
    {
        PlayerPrefs.DeleteKey(CacheDataKey);
        PlayerPrefs.DeleteKey(CacheTimeKey);
        PlayerPrefs.Save();
    }
}

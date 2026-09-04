using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UGSManager : MonoBehaviour
{
    private static UGSManager _instance;
    public static UGSManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("UGSManager");
                _instance = go.AddComponent<UGSManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    public event Action OnSignInComplete;

    public bool IsInitialized => false;
    public bool IsSignedIn => false;
    public string PlayerId => string.Empty;
    public string PlayerName => string.Empty;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeOnLoad()
    {
        _ = Instance;
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // UGS is disabled in this build.
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task EnsureSignedInAsync()
    {
        return Task.CompletedTask;
    }

    public Task<string> UpdatePlayerNameAsync(string desiredName)
    {
        return Task.FromResult(string.Empty);
    }

    public Task SubmitScoreAsync(string leaderboardId, double score)
    {
        Debug.Log($"[UGSManager] SubmitScoreAsync ignored: Unity Gaming Services is not enabled in this build.");
        return Task.CompletedTask;
    }

    public Task<List<LeaderboardEntryData>> GetScoresAsync(string leaderboardId, int limit = 30)
    {
        Debug.Log($"[UGSManager] GetScoresAsync ignored: Unity Gaming Services is not enabled in this build.");
        return Task.FromResult<List<LeaderboardEntryData>>(null);
    }

    private string CleanUsername(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Player";
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (char c in name)
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
            }
        }
        string clean = sb.ToString();
        return clean.Length >= 3 ? clean : "Player" + UnityEngine.Random.Range(100, 999);
    }
}

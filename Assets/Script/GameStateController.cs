using UnityEngine;

public class GameStateController : MonoBehaviour
{
    public static GameStateController Instance { get; private set; }

    private bool playing = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Auto-bootstrap if not in scene
    [RuntimeInitializeOnLoadMethod]
    private static void AutoBootstrap()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("GameStateController (Auto)");
            go.AddComponent<GameStateController>();
            DontDestroyOnLoad(go);
            Debug.Log("[GameStateController] Auto-bootstrapped");
        }
    }

    public bool IsPlaying() => playing;

    public void SetPlaying(bool value) => playing = value;

    // ✅ call this when wrong letter is picked
    public void GameOver(string reason = "")
    {
        Debug.Log($"GAME OVER: {reason}");
        playing = false;

        // Optional: freeze
        Time.timeScale = 0f;

        // Optional hook: if you have UIManager / FailPanel later, call it here.
        // UIManager.Instance.ShowFail();
    }

    // Optional helper
    public void Resume()
    {
        Time.timeScale = 1f;
        playing = true;
    }
}

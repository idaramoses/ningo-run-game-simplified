using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Utility for loading scenes with background preloading for instant (split-second) transitions.
/// Attach to a DontDestroyOnLoad manager object.
/// Scene names: "Home" (home/menu), "GameScene" (gameplay)
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    public const string HOME_SCENE = "Home";
    public const string GAME_SCENE = "GameScene";

    private AsyncOperation preloadGameOp;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Whenever SceneLoader starts, start preloading if we are in Splash or Home scene
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene != GAME_SCENE)
        {
            StartCoroutine(PreloadGameSceneRoutine());
        }

        // Listen for scene changes to automatically start preloading again when we enter Home
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // When we load any scene other than GameScene (e.g. returning to Home), start preloading GameScene again
        if (scene.name != GAME_SCENE)
        {
            StartCoroutine(PreloadGameSceneRoutine());
        }
    }

    public AsyncOperation GetPreloadOp()
    {
        AsyncOperation op = preloadGameOp;
        preloadGameOp = null; // Clear reference so it's consumed
        return op;
    }

    private System.Collections.IEnumerator PreloadGameSceneRoutine()
    {
        // If already preloading, skip
        if (preloadGameOp != null) yield break;

        // Wait a brief delay for active scene initialization to complete
        yield return new WaitForSeconds(0.5f);

        if (SceneManager.GetActiveScene().name != GAME_SCENE)
        {
            Debug.Log("[SceneLoader] Preloading GameScene in background...");
            preloadGameOp = SceneManager.LoadSceneAsync(GAME_SCENE);
            if (preloadGameOp != null)
            {
                preloadGameOp.allowSceneActivation = false;
                while (preloadGameOp.progress < 0.9f)
                {
                    yield return null;
                }
                Debug.Log("[SceneLoader] GameScene is fully preloaded in background and ready to activate.");
            }
        }
    }

    public void LoadHomeScene()
    {
        Time.timeScale = 1f;

        // Reset game state
        if (GameStateController.Instance != null)
        {
            GameStateController.Instance.SetPlaying(false);
        }

        // Switch to home music
        if (BackgroundMusicManager.Instance != null)
        {
            BackgroundMusicManager.Instance.PlayHomeMusic();
        }

        Debug.Log("[SceneLoader] Loading Home scene");
        SceneManager.LoadSceneAsync(HOME_SCENE);
    }

    public void LoadGameScene()
    {
        Time.timeScale = 1f;

        // Switch to gameplay music
        if (BackgroundMusicManager.Instance != null)
        {
            BackgroundMusicManager.Instance.PlayGameplayMusic();
        }

        Debug.Log("[SceneLoader] Loading GameScene");
        
        if (preloadGameOp != null)
        {
            Debug.Log("[SceneLoader] Activating preloaded GameScene instantly!");
            preloadGameOp.allowSceneActivation = true;
            preloadGameOp = null; // Clear reference
        }
        else
        {
            Debug.LogWarning("[SceneLoader] No preloaded GameScene found, loading asynchronously.");
            SceneManager.LoadSceneAsync(GAME_SCENE);
        }
    }

    public void RestartGameScene()
    {
        Time.timeScale = 1f;
        Debug.Log("[SceneLoader] Restarting GameScene");
        SceneManager.LoadSceneAsync(GAME_SCENE);
    }

    public bool IsGameScene()
    {
        return SceneManager.GetActiveScene().name == GAME_SCENE;
    }

    public bool IsHomeScene()
    {
        return SceneManager.GetActiveScene().name == HOME_SCENE;
    }
}

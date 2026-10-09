using System.Collections;
using System.Collections.Generic;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Loads scenes that live in remote Addressables bundles instead of the
/// built-in scene list. Keeps the initial WebGL download small — heavy scenes
/// stream from ServerData/ on demand.
/// RemoteNames holds scene names registered as remote addressables.
/// Falls back to SceneManager for anything else (and in the Editor when
/// play-mode delivery is "Use Asset Database", Addressables just works too).
/// </summary>
public static class RemoteScene
{
    private static readonly HashSet<string> RemoteNames = new HashSet<string> { "Home" };
    private static Runner runner;
    private class Runner : MonoBehaviour { }

    static RemoteScene()
    {
        // Bundles use the relative "ServerData/WebGL" load path so builds stay
        // host-agnostic. On WebGL a relative id is treated as a local file, so
        // resolve it against the page URL — same origin, works on any host.
        Addressables.InternalIdTransformFunc = location =>
        {
            string id = location.InternalId;
            if (!id.Contains("://") && id.StartsWith("ServerData/"))
            {
                string baseUrl = Application.absoluteURL;
                int slash = baseUrl.LastIndexOf('/');
                if (slash > 0) baseUrl = baseUrl.Substring(0, slash + 1);
                return baseUrl + id;
            }
            return id;
        };
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void NingoReportProgress(float p);
    [DllImport("__Internal")] private static extern void NingoReportSceneDone();
#else
    private static void NingoReportProgress(float p) { Debug.Log($"[RemoteScene] progress {p:P0}"); }
    private static void NingoReportSceneDone() { }
#endif

    public static bool IsRemote(string sceneName) => RemoteNames.Contains(sceneName);

    /// <summary>Drop-in for SceneManager.LoadSceneAsync — fire and forget.</summary>
    public static void Load(string sceneName)
    {
        if (!IsRemote(sceneName))
        {
            SceneManager.LoadSceneAsync(sceneName);
            return;
        }
        EnsureRunner().StartCoroutine(LoadRoutine(sceneName));
    }

    private static Runner EnsureRunner()
    {
        if (runner == null)
        {
            var go = new GameObject("[RemoteScene]");
            runner = go.AddComponent<Runner>();
            Object.DontDestroyOnLoad(go);
        }
        return runner;
    }

    private static IEnumerator LoadRoutine(string sceneName)
    {
        Debug.Log($"[RemoteScene] Streaming '{sceneName}' bundles…");

        // Phase 1: download all dependency bundles (0–90% of the bar).
        var dl = Addressables.DownloadDependenciesAsync(sceneName);
        while (!dl.IsDone)
        {
            NingoReportProgress(dl.PercentComplete * 0.9f);
            yield return null;
        }
        if (dl.Status == AsyncOperationStatus.Failed)
        {
            Debug.LogError($"[RemoteScene] '{sceneName}' download failed: {dl.OperationException}");
            yield break;
        }

        // Phase 2: load + activate the scene (90–100%).
        var handle = Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Single, activateOnLoad: true);
        while (!handle.IsDone)
        {
            NingoReportProgress(0.9f + handle.PercentComplete * 0.1f);
            yield return null;
        }
        if (handle.Status == AsyncOperationStatus.Failed)
            Debug.LogError($"[RemoteScene] '{sceneName}' failed: {handle.OperationException}");
        else
            Debug.Log($"[RemoteScene] '{sceneName}' loaded");
        NingoReportSceneDone();
    }
}

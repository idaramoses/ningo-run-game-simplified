using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lightweight toast notification.
/// On Android: uses the native Android Toast API.
/// On all other platforms (iOS, Editor, etc.): creates a temporary UI overlay.
/// Usage: ToastManager.Show("Your message here");
/// </summary>
public class ToastManager : MonoBehaviour
{
    private static ToastManager _instance;

    public Vector2 messagePosition = new Vector2(0f, -96f);
    public Vector2 messageSize = new Vector2(900f, 120f);
    public Color messageBackgroundColor = new Color(0.08f, 0.12f, 0.18f, 0.96f);
    public Image messageBackground;
    public TMP_Text messageText;
    public CanvasGroup messageCanvasGroup;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoBootstrap()
    {
        if (_instance != null) return;

        _instance = FindFirstObjectByType<ToastManager>();
        if (_instance != null) return;

        var go = new GameObject("ToastManager");
        _instance = go.AddComponent<ToastManager>();
        DontDestroyOnLoad(go);
    }

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Show a toast for <paramref name="duration"/> seconds.</summary>
    public static void Show(string message, float duration = 2.5f)
    {
        if (_instance == null) AutoBootstrap();
        if (_instance != null)
            _instance.StartCoroutine(_instance.ShowOverlay(message, duration));
        else
            Debug.Log($"[Toast] {message}");
    }

    // ── Android native ────────────────────────────────────────────────────────

    private static void ShowNativeAndroid(string message)
    {
        try
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                using var toast = new AndroidJavaClass("android.widget.Toast");
                toast.CallStatic<AndroidJavaObject>("makeText", activity, message, 1).Call("show");
            }));
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[ToastManager] Android toast failed: {e.Message}");
        }
    }

    // ── Unity UI overlay (iOS / Editor / fallback) ────────────────────────────

    private IEnumerator ShowOverlay(string message, float duration)
    {
        // Self-heal: find the ToastUI_Background in the HUD if refs were never
        // assigned (e.g. the bootstrapped instance won the singleton race)
        if (messageBackground == null || messageText == null)
        {
            foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (t.name != "ToastUI_Background") continue;
                messageBackground = t.GetComponent<Image>();
                messageText = t.GetComponentInChildren<TMP_Text>(true);
                if (messageCanvasGroup == null)
                {
                    messageCanvasGroup = t.GetComponent<CanvasGroup>();
                    if (messageCanvasGroup == null) messageCanvasGroup = t.gameObject.AddComponent<CanvasGroup>();
                }
                break;
            }
        }

        if (messageBackground == null || messageText == null)
        {
            Debug.LogWarning($"[ToastManager] No ToastUI_Background found - skipping toast: {message}");
            yield break;
        }

        messageBackground.color = messageBackgroundColor;
        messageText.text = message;
        messageBackground.gameObject.SetActive(true);

        if (messageCanvasGroup != null)
            messageCanvasGroup.alpha = 1f;

        yield return new WaitForSecondsRealtime(Mathf.Max(0f, duration - 0.4f));

        if (messageCanvasGroup != null)
        {
            float fadeTime = 0f;
            while (fadeTime < 0.4f)
            {
                fadeTime += Time.unscaledDeltaTime;
                messageCanvasGroup.alpha = 1f - (fadeTime / 0.4f);
                yield return null;
            }
        }

        messageBackground.gameObject.SetActive(false);
    }
}

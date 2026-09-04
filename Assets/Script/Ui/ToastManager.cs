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
        if (messageBackground != null && messageText != null)
        {
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
            yield break;
        }

        // Build a simple full-screen-safe overlay canvas
        var canvasGo = new GameObject("Toast_Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGo.AddComponent<GraphicRaycaster>();

        // Background button
        var bgGo = new GameObject("ToastUI_Background");
        bgGo.transform.SetParent(canvas.transform, false);
        var bg = bgGo.AddComponent<Image>();
        bg.color = messageBackgroundColor;
        var button = bgGo.AddComponent<Button>();
        button.targetGraphic = bg;
        var rect = bgGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.offsetMin = new Vector2(24f, 72f);
        rect.offsetMax = new Vector2(-24f, 124f);

        // Button text
        var labelGo = new GameObject("ToastUI_Text");
        labelGo.transform.SetParent(bgGo.transform, false);
        var label = labelGo.AddComponent<TextMeshProUGUI>();
        label.text = message;
        label.fontSize = 32;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        label.GetComponent<RectTransform>().anchorMax = Vector2.one;
        label.GetComponent<RectTransform>().offsetMin = new Vector2(20, 10);
        label.GetComponent<RectTransform>().offsetMax = new Vector2(-20, -10);

        // Hold
        yield return new WaitForSecondsRealtime(duration - 0.4f);

        // Fade out
        float t = 0f;
        var canvasGroup = bgGo.AddComponent<CanvasGroup>();
        while (t < 0.4f)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = 1f - (t / 0.4f);
            yield return null;
        }

        Destroy(canvasGo);
    }
}

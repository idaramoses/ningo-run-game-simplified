using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Full-screen black overlay used to hide jarring resets (camera whip, runner teleport).
/// Self-creates a topmost ScreenSpaceOverlay canvas the first time it is used.
/// </summary>
public class ScreenFader : MonoBehaviour
{
    private static ScreenFader instance;
    private CanvasGroup group;
    private Image image;

    /// <summary>Color of the fade overlay. Change this in the Inspector or set
    /// ScreenFader.FadeColor from code before the fade starts.</summary>
    public Color fadeColor = Color.black;
    public static Color FadeColor
    {
        get { return Ensure().fadeColor; }
        set { Ensure().fadeColor = value; if (Ensure().image != null) Ensure().image.color = value; }
    }

    private void Awake()
    {
        // Register a scene-placed instance so its serialized fadeColor is used
        instance = this;
        BuildOverlay();
    }

    private void BuildOverlay()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        if (GetComponent<CanvasScaler>() == null) gameObject.AddComponent<CanvasScaler>();

        group = GetComponent<CanvasGroup>();
        if (group == null) group = gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        var imgT = transform.Find("Overlay");
        if (imgT == null)
        {
            var imgGO = new GameObject("Overlay");
            imgGO.transform.SetParent(transform, false);
            imgT = imgGO.transform;
        }
        image = imgT.GetComponent<Image>();
        if (image == null) image = imgT.gameObject.AddComponent<Image>();
        image.color = fadeColor;
        image.raycastTarget = false;
        var rt = image.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static ScreenFader Ensure()
    {
        if (instance == null)
            instance = FindFirstObjectByType<ScreenFader>();
        if (instance != null)
        {
            instance.BuildOverlay();
            return instance;
        }

        var go = new GameObject("ScreenFader");
        instance = go.AddComponent<ScreenFader>();
        instance.BuildOverlay();
        return instance;
    }

    public static IEnumerator FadeToBlack(float duration)
    {
        var f = Ensure();
        yield return f.Fade(1f, duration);
    }

    public static IEnumerator FadeFromBlack(float duration)
    {
        var f = Ensure();
        yield return f.Fade(0f, duration);
    }

    private IEnumerator Fade(float target, float duration)
    {
        float start = group.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        group.alpha = target;
    }
}

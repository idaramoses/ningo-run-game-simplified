using UnityEngine;
using UnityEngine.UI;

public class ImageAnimator : MonoBehaviour
{
    public enum AnimationType
    {
        Bounce,
        FadeInOut,
        BounceFade,
        Pulse
    }

    [Header("Animation Type")]
    [SerializeField] private AnimationType animationType = AnimationType.Bounce;

    [Header("Bounce Settings")]
    [SerializeField] private float bounceHeight = 20f;
    [SerializeField] private float bounceSpeed = 2f;

    [Header("Fade Settings")]
    [SerializeField] private float minAlpha = 0.5f;
    [SerializeField] private float maxAlpha = 1f;
    [SerializeField] private float fadeSpeed = 1.5f;

    [Header("Pulse Settings")]
    [SerializeField] private float pulseMinScale = 0.95f;
    [SerializeField] private float pulseMaxScale = 1.05f;
    [SerializeField] private float pulseSpeed = 2f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Image image;
    private Vector2 startPosition;
    private Vector3 startScale;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        image = GetComponent<Image>();

        if (rectTransform != null)
        {
            startPosition = rectTransform.anchoredPosition;
            startScale = rectTransform.localScale;
        }

        // Add CanvasGroup if needed for fade and not present
        if (canvasGroup == null && (animationType == AnimationType.FadeInOut || animationType == AnimationType.BounceFade))
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void Update()
    {
        switch (animationType)
        {
            case AnimationType.Bounce:
                AnimateBounce();
                break;
            case AnimationType.FadeInOut:
                AnimateFade();
                break;
            case AnimationType.BounceFade:
                AnimateBounce();
                AnimateFade();
                break;
            case AnimationType.Pulse:
                AnimatePulse();
                break;
        }
    }

    private void AnimateBounce()
    {
        if (rectTransform == null) return;

        float offsetY = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
        rectTransform.anchoredPosition = new Vector2(startPosition.x, startPosition.y + offsetY);
    }

    private void AnimateFade()
    {
        if (canvasGroup == null && image == null) return;

        float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * fadeSpeed) + 1f) / 2f);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
        }
        else if (image != null)
        {
            Color c = image.color;
            c.a = alpha;
            image.color = c;
        }
    }

    private void AnimatePulse()
    {
        if (rectTransform == null) return;

        float scale = Mathf.Lerp(pulseMinScale, pulseMaxScale, (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f);
        rectTransform.localScale = startScale * scale;
    }

    private void OnDisable()
    {
        // Reset to original state
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = startPosition;
            rectTransform.localScale = startScale;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = maxAlpha;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class ButtonClickEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("Scale Effect")]
    [SerializeField] private bool useScaleEffect = true;
    [SerializeField] private float pressedScale = 0.92f;
    [SerializeField] private float animationDuration = 0.1f;

    [Header("Fade Effect")]
    [SerializeField] private bool useFadeEffect = false;
    [SerializeField] private float pressedAlpha = 0.7f;

    [Header("Audio")]
    [SerializeField] private bool playClickSound = true;
    [SerializeField] private string clickSoundName = "ui_click";

    private Button button;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalScale;
    private float originalAlpha;
    private bool isPressed;

    private void Awake()
    {
        button = GetComponent<Button>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (rectTransform != null)
            originalScale = rectTransform.localScale;

        if (canvasGroup != null)
            originalAlpha = canvasGroup.alpha;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (button != null && !button.interactable)
            return;

        isPressed = true;

        if (useScaleEffect && rectTransform != null)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateScale(pressedScale, animationDuration));
        }

        if (useFadeEffect && canvasGroup != null)
        {
            canvasGroup.alpha = pressedAlpha;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPressed) return;
        isPressed = false;

        if (useScaleEffect && rectTransform != null)
        {
            StopAllCoroutines();
            StartCoroutine(AnimateScale(1f, animationDuration));
        }

        if (useFadeEffect && canvasGroup != null)
        {
            canvasGroup.alpha = originalAlpha;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (playClickSound)
        {
            // TODO: AudioManager.Instance.PlaySound(clickSoundName);
            Debug.Log($"[ButtonClickEffect] Click sound: {clickSoundName}");
        }
    }

    private System.Collections.IEnumerator AnimateScale(float targetScale, float duration)
    {
        Vector3 startScale = rectTransform.localScale;
        Vector3 endScale = originalScale * targetScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            // Smooth ease out
            t = 1f - Mathf.Pow(1f - t, 3f);
            rectTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        rectTransform.localScale = endScale;
    }

    private void OnDisable()
    {
        // Reset scale when disabled
        if (rectTransform != null)
            rectTransform.localScale = originalScale;
        if (canvasGroup != null)
            canvasGroup.alpha = originalAlpha;
        isPressed = false;
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CanvasAnimator : MonoBehaviour
{
    public enum AnimationType { Fade, Bounce }

    [Header("Animation")]
    [SerializeField] private AnimationType animationType = AnimationType.Fade;
    [SerializeField] private float duration = 0.25f;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Coroutine animCoroutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        rectTransform = GetComponent<RectTransform>();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(PlayShow());
    }

    public void Hide()
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        animCoroutine = StartCoroutine(PlayHide());
    }

    private IEnumerator PlayShow()
    {
        canvasGroup.interactable = false;
        float elapsed = 0f;

        if (animationType == AnimationType.Bounce && rectTransform != null)
        {
            rectTransform.localScale = Vector3.zero;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                float scale = Mathf.Lerp(0f, 1.15f, t);
                if (t > 0.6f)
                {
                    float settleT = (t - 0.6f) / 0.4f;
                    scale = Mathf.Lerp(1.15f, 1f, settleT);
                }
                rectTransform.localScale = Vector3.one * scale;
                yield return null;
            }
            rectTransform.localScale = Vector3.one;
        }
        else
        {
            canvasGroup.alpha = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        canvasGroup.interactable = true;
    }

    private IEnumerator PlayHide()
    {
        canvasGroup.interactable = false;
        float elapsed = 0f;

        if (animationType == AnimationType.Bounce && rectTransform != null)
        {
            float hideDuration = duration * 0.5f;
            while (elapsed < hideDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / hideDuration;
                rectTransform.localScale = Vector3.one * (1f - t);
                yield return null;
            }
            rectTransform.localScale = Vector3.zero;
        }
        else
        {
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }

        gameObject.SetActive(false);
    }
}

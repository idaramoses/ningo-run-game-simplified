using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Simple script to fade out a UI overlay when the scene starts.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class SceneFadeOverlay : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] public float delayBeforeFade = 1.0f;
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] public Color overlayColor = Color.black;
    [SerializeField] private bool destroyOnComplete = true;

    private CanvasGroup canvasGroup;
    private Image overlayImage;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        overlayImage = GetComponent<Image>();
        
        UpdateColor();
        
        // Ensure it's fully visible at start
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    public void UpdateColor()
    {
        if (overlayImage == null) overlayImage = GetComponent<Image>();
        if (overlayImage != null)
        {
            overlayImage.color = overlayColor;
        }
    }

    private void Start()
    {
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
        if (delayBeforeFade > 0)
        {
            yield return new WaitForSeconds(delayBeforeFade);
        }

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        if (destroyOnComplete)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}

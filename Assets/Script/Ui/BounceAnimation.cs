using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Reusable bounce animation component that can be attached to any UI element.
/// Plays a bounce effect whenever the text content changes.
/// </summary>
public class BounceAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Duration of the bounce animation in seconds")]
    public float bounceDuration = 0.5f;
    
    [Tooltip("Maximum scale multiplier during bounce (1.3 = 30% larger)")]
    public float bounceScale = 1.3f;
    
    [Tooltip("Animation curve for smooth easing")]
    public AnimationCurve bounceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Tooltip("Auto-play bounce on Start")]
    public bool playOnStart = false;

    private Vector3 originalScale;
    private TMP_Text textComponent;
    private string lastText = "";
    private Coroutine bounceCoroutine;

    private void Awake()
    {
        // Store original scale
        originalScale = transform.localScale;
        
        // Try to get TMP_Text component for auto-trigger on text change
        textComponent = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        if (playOnStart)
        {
            PlayBounce();
        }
    }

    private void LateUpdate()
    {
        // Auto-trigger bounce when text changes
        if (textComponent != null && textComponent.text != lastText)
        {
            lastText = textComponent.text;
            PlayBounce();
        }
    }

    /// <summary>
    /// Manually trigger the bounce animation
    /// </summary>
    public void PlayBounce()
    {
        // Stop any existing animation
        if (bounceCoroutine != null)
        {
            StopCoroutine(bounceCoroutine);
        }
        
        // Start new bounce animation
        bounceCoroutine = StartCoroutine(BounceCoroutine());
    }

    private IEnumerator BounceCoroutine()
    {
        float elapsed = 0f;
        Vector3 startScale = originalScale;

        while (elapsed < bounceDuration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time so it works even when paused
            float progress = elapsed / bounceDuration;

            // Create bounce effect: scale up then back down using sine wave
            float curveValue = bounceCurve.Evaluate(progress);
            float scale = Mathf.Lerp(1f, bounceScale, Mathf.Sin(curveValue * Mathf.PI));

            transform.localScale = startScale * scale;

            yield return null;
        }

        // Ensure we end at original scale
        transform.localScale = originalScale;
        bounceCoroutine = null;
    }

    /// <summary>
    /// Reset the original scale (useful if parent scale changes)
    /// </summary>
    public void ResetOriginalScale()
    {
        originalScale = transform.localScale;
    }

    private void OnDisable()
    {
        // Stop animation when disabled
        if (bounceCoroutine != null)
        {
            StopCoroutine(bounceCoroutine);
            bounceCoroutine = null;
        }
        
        // Reset to original scale
        transform.localScale = originalScale;
    }
}

using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Reusable pulse/breathing animation component for UI elements.
/// Creates a gentle scale pulsing effect that loops continuously.
/// </summary>
public class PulseAnimation : MonoBehaviour
{
    [Header("Pulse Settings")]
    [Tooltip("Duration of one complete pulse cycle (in and out)")]
    public float pulseDuration = 1.5f;
    
    [Tooltip("Maximum scale multiplier during pulse (1.1 = 10% larger)")]
    public float pulseScale = 1.1f;
    
    [Tooltip("Animation curve for smooth easing")]
    public AnimationCurve pulseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Tooltip("Auto-start pulsing on Start")]
    public bool autoStart = true;
    
    [Tooltip("Loop the pulse animation continuously")]
    public bool loop = true;

    private Vector3 originalScale;
    private Coroutine pulseCoroutine;
    private bool isPulsing = false;

    private void Awake()
    {
        // Store original scale
        originalScale = transform.localScale;
    }

    private void Start()
    {
        if (autoStart)
        {
            StartPulse();
        }
    }

    /// <summary>
    /// Start the pulse animation
    /// </summary>
    public void StartPulse()
    {
        if (isPulsing) return;
        
        isPulsing = true;
        
        // Stop any existing animation
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }
        
        // Start pulse animation
        pulseCoroutine = StartCoroutine(PulseCoroutine());
    }

    /// <summary>
    /// Stop the pulse animation
    /// </summary>
    public void StopPulse()
    {
        isPulsing = false;
        
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            pulseCoroutine = null;
        }
        
        // Reset to original scale
        transform.localScale = originalScale;
    }

    private IEnumerator PulseCoroutine()
    {
        while (isPulsing)
        {
            float elapsed = 0f;

            // Pulse cycle: scale up and down
            while (elapsed < pulseDuration)
            {
                elapsed += Time.unscaledDeltaTime; // Use unscaled time so it works even when paused
                float progress = elapsed / pulseDuration;

                // Create pulse effect: smoothly scale up and down using sine wave
                float curveValue = pulseCurve.Evaluate(progress);
                float scale = Mathf.Lerp(1f, pulseScale, Mathf.Sin(curveValue * Mathf.PI));

                transform.localScale = originalScale * scale;

                yield return null;
            }

            // If not looping, stop after one cycle
            if (!loop)
            {
                isPulsing = false;
                break;
            }
        }

        // Ensure we end at original scale
        transform.localScale = originalScale;
        pulseCoroutine = null;
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
        StopPulse();
    }

    private void OnDestroy()
    {
        // Clean up coroutine
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }
    }
}

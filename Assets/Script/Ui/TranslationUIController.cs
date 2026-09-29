using UnityEngine;
using TMPro;
using System.Collections;

public class TranslationUIController : MonoBehaviour
{
    [Header("UI (TMP)")]
    public TMP_Text promptText;   // EnglishPanel/Text
    public TMP_Text typedText;    // optional: shows collected letters
    public TMP_Text targetText;   // LanguagePanel/Text

    [Header("Bounce Animation")]
    public float bounceDuration = 0.5f;
    public float bounceScale = 1.3f;
    public AnimationCurve bounceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 originalScale;

    private void Start()
    {
        // Store original scale of target text
        if (targetText != null)
        {
            originalScale = targetText.transform.localScale;
            Debug.Log($"[TranslationUIController] Stored original scale: {originalScale}");
        }
        else
        {
            Debug.LogWarning("[TranslationUIController] targetText is not assigned!");
        }
    }

    public void SetPrompt(string english)
    {
        if (promptText == null) return;
        promptText.text = $"Translate: {english.ToUpperInvariant()}";
    }

    /// <param name="collected">letters collected so far</param>
    /// <param name="target">full target word</param>
    /// <param name="revealTarget">if true, show the full target word</param>
    public void SetProgress(string collected, string target, bool revealTarget)
    {
        if (typedText != null)
            typedText.text = collected;

        if (targetText != null)
        {
            if (revealTarget)
            {
                targetText.text = target;
                // Play bounce animation when target is revealed
                PlayBounceAnimation();
            }
            else
            {
                // Hide the answer; show underscores so player knows length
                targetText.text = new string('_', target.Length);
                // Also animate the underscores to draw attention
                PlayBounceAnimation();
            }
        }
    }

    private void PlayBounceAnimation()
    {
        if (targetText == null)
        {
            Debug.LogWarning("[TranslationUIController] Cannot play bounce - targetText is null");
            return;
        }
        
        // Fallback: if originalScale is zero (Start hasn't run), capture it now
        if (originalScale == Vector3.zero)
        {
            originalScale = targetText.transform.localScale;
            Debug.Log($"[TranslationUIController] Captured originalScale on-demand: {originalScale}");
        }
        
        Debug.Log("[TranslationUIController] Playing bounce animation");
        
        // Stop any existing animation
        StopAllCoroutines();
        
        // Start bounce animation
        StartCoroutine(BounceCoroutine());
    }

    private IEnumerator BounceCoroutine()
    {
        float elapsed = 0f;
        Vector3 startScale = originalScale;
        
        while (elapsed < bounceDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / bounceDuration;
            
            // Create bounce effect: scale up then back down
            float curveValue = bounceCurve.Evaluate(progress);
            float scale = Mathf.Lerp(1f, bounceScale, Mathf.Sin(curveValue * Mathf.PI));
            
            targetText.transform.localScale = startScale * scale;
            
            yield return null;
        }
        
        // Ensure we end at original scale
        targetText.transform.localScale = originalScale;
    }
}

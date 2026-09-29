using System.Collections;
using UnityEngine;
using TMPro;

public class WordUIController : MonoBehaviour
{
    [Header("Data")]
    public WordPicker picker;

    [Header("UI (TMP)")]
    public TMP_Text englishText;
    public TMP_Text langText;

    [Header("Bounce Animation")]
    public float bounceDuration = 0.5f;
    public float bounceScale = 1.3f;
    public AnimationCurve bounceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector3 originalScale;

    IEnumerator Start()
    {
        if (picker == null) picker = FindObjectOfType<WordPicker>();

        // Store original scale of langText
        if (langText != null)
        {
            originalScale = langText.transform.localScale;
            Debug.Log($"[WordUIController] Stored original scale: {originalScale}");
        }

        yield return new WaitUntil(() =>
            picker != null &&
            DictionaryManager.HasDictionary()
        );

        ShowRandomWord();
    }

    public void ShowRandomWord()
    {
        if (picker == null || !DictionaryManager.HasDictionary())
        {
            Debug.LogError("[WordUIController] Picker or dictionary not ready");
            return;
        }

        picker.PickRandom();

        if (englishText) englishText.text = picker.GetPromptText();
        if (langText)
        {
            langText.text = picker.GetPickedWord();
            // Play bounce animation on the target translation text
            PlayBounceAnimation();
        }
    }

    private void PlayBounceAnimation()
    {
        if (langText == null)
        {
            Debug.LogWarning("[WordUIController] Cannot play bounce - langText is null");
            return;
        }

        // Fallback: if originalScale is zero, capture it now
        if (originalScale == Vector3.zero)
        {
            originalScale = langText.transform.localScale;
            Debug.Log($"[WordUIController] Captured originalScale on-demand: {originalScale}");
        }

        Debug.Log("[WordUIController] Playing bounce animation on langText");

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

            langText.transform.localScale = startScale * scale;

            yield return null;
        }

        // Ensure we end at original scale
        langText.transform.localScale = originalScale;
    }
}

using UnityEngine;
using System.Collections;


public class EyeBlinkController : MonoBehaviour
{
    [Header("Eyelid Quads (parent these to the head bone)")]
    [Tooltip("Quad positioned over the left eye")]
    [SerializeField] private Transform leftEyelid;
    [Tooltip("Quad positioned over the right eye")]
    [SerializeField] private Transform rightEyelid;

    [Header("Blink Timing")]
    [SerializeField] private float minBlinkInterval = 2.5f;
    [SerializeField] private float maxBlinkInterval = 6f;
    [Tooltip("How fast eyes close (seconds)")]
    [SerializeField] private float closeSpeed = 0.12f;
    [Tooltip("How fast eyes open (seconds)")]
    [SerializeField] private float openSpeed = 0.20f;
    [Tooltip("How long eyes stay closed (seconds)")]
    [SerializeField] private float holdClosedDuration = 0.08f;
    [Tooltip("Chance (0-1) of a second quick blink immediately after the first")]
    [SerializeField] private float doubleBlikChance = 0.2f;

    private bool isBlinking = false;
    private bool initialized = false;

    // Stored at Awake from the eyelid transforms
    private float leftClosedHeight;    // localScale.x = vertical height when closed
    private float leftOriginalWidth;   // localScale.y = horizontal width (constant)
    private Vector3 leftOpenPos;       // localPosition when eye is fully open

    private float rightClosedHeight;
    private float rightOriginalWidth;
    private Vector3 rightOpenPos;

    private void Awake()
    {
        // Auto-find eyelids by name (works on clones spawned by FailRunnerPreview too)
        if (leftEyelid == null)
            leftEyelid = FindDeep(transform, "LeftEyelid");
        if (rightEyelid == null)
            rightEyelid = FindDeep(transform, "RightEyelid");

        if (leftEyelid == null && rightEyelid == null)
        {
            Debug.LogWarning("[EyeBlinkController] Could not find LeftEyelid or RightEyelid in children.");
            enabled = false;
            return;
        }

        // Read closed dimensions from the capsule's current Inspector scale
        // Capsule is rotated Z:-90, so localScale.x = vertical, localScale.y = horizontal
        leftClosedHeight  = leftEyelid  != null ? leftEyelid.localScale.x  : 0.089f;
        leftOriginalWidth = leftEyelid  != null ? leftEyelid.localScale.y  : 0.049f;
        rightClosedHeight = rightEyelid != null ? rightEyelid.localScale.x : 0.089f;
        rightOriginalWidth = rightEyelid != null ? rightEyelid.localScale.y : 0.049f;

        // "Open" position = original position shifted up by half the height
        // so the top edge stays fixed when the eyelid grows downward
        if (leftEyelid  != null) leftOpenPos  = new Vector3(leftEyelid.localPosition.x,  leftEyelid.localPosition.y  + leftClosedHeight  / 2f, leftEyelid.localPosition.z);
        if (rightEyelid != null) rightOpenPos = new Vector3(rightEyelid.localPosition.x, rightEyelid.localPosition.y + rightClosedHeight / 2f, rightEyelid.localPosition.z);

        initialized = true;
        SetEyelidScale(0f);
    }

    private void OnEnable()
    {
        if (!initialized) return;
        isBlinking = false;
        StopAllCoroutines();
        StartCoroutine(BlinkRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isBlinking = false;
    }

    private Transform FindDeep(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform found = FindDeep(child, name);
            if (found != null) return found;
        }
        return null;
    }

    private IEnumerator BlinkRoutine()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(Random.Range(minBlinkInterval, maxBlinkInterval));
            if (!isBlinking)
                yield return StartCoroutine(DoBlink());
        }
    }

    private IEnumerator DoBlink()
    {
        isBlinking = true;
        yield return StartCoroutine(SingleBlink());

        // Occasional double blink - a quick second blink after a short pause
        if (Random.value < doubleBlikChance)
        {
            yield return new WaitForSecondsRealtime(0.08f);
            yield return StartCoroutine(SingleBlink());
        }

        isBlinking = false;
    }

    private IEnumerator SingleBlink()
    {
        // Close eyes fast - ease in (accelerate)
        float elapsed = 0f;
        while (elapsed < closeSpeed)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / closeSpeed);
            SetEyelidScale(t * t); // ease in: starts slow, ends fast
            yield return null;
        }
        SetEyelidScale(1f);

        // Hold closed briefly so the blink is visible
        yield return new WaitForSecondsRealtime(holdClosedDuration);

        // Open eyes slow - ease out (decelerate)
        elapsed = 0f;
        while (elapsed < openSpeed)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / openSpeed);
            SetEyelidScale(1f - (t * (2f - t))); // ease out: starts fast, ends slow
            yield return null;
        }
        SetEyelidScale(0f);
    }

    private void SetEyelidScale(float t)
    {
        // t=0 = open (height 0, positioned at top edge)
        // t=1 = closed (full height, centered on eye)
        // Animate localScale.x (vertical) and shift position so top edge stays fixed
        if (leftEyelid != null)
        {
            float h = leftClosedHeight * t;
            leftEyelid.localScale = new Vector3(h, leftOriginalWidth, leftEyelid.localScale.z);
            leftEyelid.localPosition = new Vector3(leftOpenPos.x, leftOpenPos.y - leftClosedHeight * t / 2f, leftOpenPos.z);
        }
        if (rightEyelid != null)
        {
            float h = rightClosedHeight * t;
            rightEyelid.localScale = new Vector3(h, rightOriginalWidth, rightEyelid.localScale.z);
            rightEyelid.localPosition = new Vector3(rightOpenPos.x, rightOpenPos.y - rightClosedHeight * t / 2f, rightOpenPos.z);
        }
    }
}

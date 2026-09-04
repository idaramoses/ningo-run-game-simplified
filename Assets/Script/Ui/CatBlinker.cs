using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CatBlinker : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The Image component for the eyes (sprite swap target)")]
    [SerializeField] private Image targetImage;

    [Header("Eye Sprites")]
    [SerializeField] private Sprite eyesOpenSprite;
    [SerializeField] private Sprite eyesClosedSprite;
    [SerializeField] private Sprite eyesHalfSprite;
    [SerializeField] private Sprite eyesWinkSprite;
    [SerializeField] private Sprite eyesWinkLeftSprite;
    [SerializeField] private Sprite eyesWinkRightSprite;

    [Header("Blink Settings")]
    [SerializeField] private bool autoBlink = true;
    [SerializeField] private float minBlinkInterval = 2f;
    [SerializeField] private float maxBlinkInterval = 5f;
    [Tooltip("Use 3-frame blink (open -> half -> closed -> half -> open) for smoother animation")]
    [SerializeField] private bool useSmoothBlink = true;
    [SerializeField] private float blinkFrameDuration = 0.06f;
    [SerializeField] private float closedHoldDuration = 0.08f;

    [Header("Wink Settings (Manual Trigger Only)")]
    [SerializeField] private float winkDuration = 0.4f;
    [Tooltip("Alternate between left and right wink randomly when triggered")]
    [SerializeField] private bool alternateWinkSides = true;

    [Header("Double Blink")]
    [Tooltip("Sometimes blink twice in a row")]
    [SerializeField] [Range(0f, 1f)] private float doubleBlinkChance = 0.15f;
    [SerializeField] private float doubleBlinkGap = 0.15f;

    private Coroutine blinkLoopCoroutine;

    private void Awake()
    {
        if (targetImage == null)
            targetImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        if (eyesOpenSprite != null && targetImage != null)
            targetImage.sprite = eyesOpenSprite;

        if (autoBlink)
            blinkLoopCoroutine = StartCoroutine(BlinkLoop());
    }

    private void OnDisable()
    {
        if (blinkLoopCoroutine != null)
        {
            StopCoroutine(blinkLoopCoroutine);
            blinkLoopCoroutine = null;
        }
    }

    private IEnumerator BlinkLoop()
    {
        while (true)
        {
            float waitTime = Random.Range(minBlinkInterval, maxBlinkInterval);
            yield return new WaitForSeconds(waitTime);

            // Just blink (no random winks)
            yield return DoBlink();

            // Chance for a double blink
            if (Random.value < doubleBlinkChance)
            {
                yield return new WaitForSeconds(doubleBlinkGap);
                yield return DoBlink();
            }
        }
    }

    private IEnumerator DoBlink()
    {
        if (targetImage == null) yield break;

        if (useSmoothBlink && eyesHalfSprite != null)
        {
            // open -> half -> closed -> half -> open
            targetImage.sprite = eyesHalfSprite;
            yield return new WaitForSeconds(blinkFrameDuration);

            if (eyesClosedSprite != null)
            {
                targetImage.sprite = eyesClosedSprite;
                yield return new WaitForSeconds(closedHoldDuration);
            }

            targetImage.sprite = eyesHalfSprite;
            yield return new WaitForSeconds(blinkFrameDuration);
        }
        else if (eyesClosedSprite != null)
        {
            // Simple blink: open -> closed -> open
            targetImage.sprite = eyesClosedSprite;
            yield return new WaitForSeconds(closedHoldDuration);
        }

        if (eyesOpenSprite != null)
            targetImage.sprite = eyesOpenSprite;
    }

    private IEnumerator DoWink()
    {
        if (targetImage == null) yield break;

        Sprite winkToUse = ChooseWinkSprite();
        if (winkToUse == null) yield break;

        targetImage.sprite = winkToUse;
        yield return new WaitForSeconds(winkDuration);

        if (eyesOpenSprite != null)
            targetImage.sprite = eyesOpenSprite;
    }

    private Sprite ChooseWinkSprite()
    {
        if (alternateWinkSides)
        {
            // Pick from available wink sprites
            bool hasLeft = eyesWinkLeftSprite != null;
            bool hasRight = eyesWinkRightSprite != null;

            if (hasLeft && hasRight)
                return Random.value < 0.5f ? eyesWinkLeftSprite : eyesWinkRightSprite;
            if (hasLeft) return eyesWinkLeftSprite;
            if (hasRight) return eyesWinkRightSprite;
        }

        return eyesWinkSprite ?? eyesWinkLeftSprite ?? eyesWinkRightSprite;
    }

    // Public API for manual triggering
    public void TriggerBlink()
    {
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(DoBlink());
    }

    public void TriggerWink()
    {
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(DoWink());
    }

    public void TriggerWinkLeft()
    {
        if (!gameObject.activeInHierarchy || eyesWinkLeftSprite == null) return;
        StartCoroutine(WinkSide(eyesWinkLeftSprite));
    }

    public void TriggerWinkRight()
    {
        if (!gameObject.activeInHierarchy || eyesWinkRightSprite == null) return;
        StartCoroutine(WinkSide(eyesWinkRightSprite));
    }

    private IEnumerator WinkSide(Sprite winkSprite)
    {
        if (targetImage == null) yield break;

        targetImage.sprite = winkSprite;
        yield return new WaitForSeconds(winkDuration);

        if (eyesOpenSprite != null)
            targetImage.sprite = eyesOpenSprite;
    }
}

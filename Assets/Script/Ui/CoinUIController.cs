using System.Collections;
using UnityEngine;
using TMPro;

public class CoinUIController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private bool updateInRealtime = true; // For game scene

    [Header("Punch Animation")]
    [SerializeField] private RectTransform coinIcon;
    [SerializeField] private float punchScale = 1.35f;
    [SerializeField] private float punchDuration = 0.22f;

    private int lastCoinCount = -1;
    private Coroutine punchCoroutine;
    private Vector3 originalIconScale = Vector3.one;
    private Vector3 originalTextScale = Vector3.one;

    private void Awake()
    {
        if (coinIcon == null)
        {
            Transform iconT = transform.Find("CoinIcon_Image");
            if (iconT == null) iconT = transform.Find("CoinIcon");
            if (iconT != null) coinIcon = iconT.GetComponent<RectTransform>();
        }

        if (coinIcon != null)
            originalIconScale = coinIcon.localScale;

        if (coinText != null)
            originalTextScale = coinText.rectTransform.localScale;
    }

    private void Start()
    {
        UpdateCoinDisplay();
    }

    private void Update()
    {
        if (updateInRealtime)
        {
            UpdateCoinDisplay();
        }
    }

    public void UpdateCoinDisplay()
    {
        int currentCoins = PlayerPrefs.GetInt(updateInRealtime ? "RunCoins" : "Coins", 0);
        
        // Only update text if value changed (optimization)
        if (currentCoins != lastCoinCount)
        {
            bool shouldAnimate = (lastCoinCount != -1 && currentCoins > lastCoinCount && gameObject.activeInHierarchy);
            lastCoinCount = currentCoins;
            if (coinText != null)
            {
                coinText.text = currentCoins.ToString();
            }

            if (shouldAnimate)
            {
                TriggerCoinPunch();
            }
        }
    }

    public void TriggerCoinPunch()
    {
        if (!gameObject.activeInHierarchy) return;

        if (punchCoroutine != null)
            StopCoroutine(punchCoroutine);

        punchCoroutine = StartCoroutine(PunchCoroutine());
    }

    private IEnumerator PunchCoroutine()
    {
        float elapsed = 0f;

        while (elapsed < punchDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / punchDuration);

            // Punch curve: fast scale up to punchScale then spring back
            float scaleMultiplier;
            if (t < 0.35f)
            {
                float upT = t / 0.35f;
                scaleMultiplier = Mathf.Lerp(1f, punchScale, upT);
            }
            else
            {
                float downT = (t - 0.35f) / 0.65f;
                // Ease out bounce back to 1.0
                scaleMultiplier = Mathf.Lerp(punchScale, 1f, Mathf.SmoothStep(0f, 1f, downT));
            }

            if (coinIcon != null)
            {
                // Scale punch with 3D Y-rotation flip effect
                float yRotation = Mathf.Sin(t * Mathf.PI) * 180f;
                coinIcon.localScale = originalIconScale * scaleMultiplier;
                coinIcon.localEulerAngles = new Vector3(0f, yRotation, 0f);
            }

            if (coinText != null)
                coinText.rectTransform.localScale = originalTextScale * scaleMultiplier;

            yield return null;
        }

        if (coinIcon != null)
        {
            coinIcon.localScale = originalIconScale;
            coinIcon.localEulerAngles = Vector3.zero;
        }

        if (coinText != null)
            coinText.rectTransform.localScale = originalTextScale;

        punchCoroutine = null;
    }

    public void RefreshDisplay()
    {
        lastCoinCount = -1; // Force refresh
        UpdateCoinDisplay();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Full Arcade Combo Coin Pickup Animation System:
/// - Screen-space / 3D-to-2D flying coins towards the HUD coin counter with curved bezier trajectories.
/// - HUD coin icon 3D spin/flip + scale bounce punch upon coin arrival.
/// - Floating +1 popup text bounce near the HUD coin counter.
/// - Sparkle / glow flare burst at the HUD counter.
/// </summary>
public class CoinPickupEffectManager : MonoBehaviour
{
    public static CoinPickupEffectManager Instance { get; private set; }

    [Header("HUD References")]
    [SerializeField] private RectTransform hudCoinTarget;
    [SerializeField] private RectTransform canvasRectTransform;
    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private Camera mainCamera;

    [Header("Flying Coins")]
    [SerializeField] private Sprite coinSprite;
    [SerializeField] private int coinsPerPickup = 1;
    [SerializeField] private float flyDuration = 0.55f;
    [SerializeField] private float coinStartSize = 38f;
    [SerializeField] private float coinEndSize = 28f;

    [Header("Floating +1 Text")]
    [SerializeField] private TMP_FontAsset fontAsset;
    [SerializeField] private Color textColor = new Color(1f, 0.88f, 0.2f, 1f);
    [SerializeField] private float textFloatDistance = 45f;
    [SerializeField] private float textDuration = 0.6f;

    // Object pools
    private readonly List<GameObject> flyingCoinPool = new List<GameObject>(16);
    private readonly List<GameObject> floatingTextPool = new List<GameObject>(12);
    private RectTransform flyingContainer;

    private void Awake()
    {
        if (Instance != null && Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        AutoFindReferences();
    }

    private void OnEnable()
    {
        AutoFindReferences();
    }

    public void AutoFindReferences()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (hudCanvas == null)
        {
            var hudObj = GameObject.Find("Canvas_HUD");
            if (hudObj != null) hudCanvas = hudObj.GetComponent<Canvas>();
            if (hudCanvas == null) hudCanvas = GetComponentInParent<Canvas>();
        }

        if (hudCanvas != null && canvasRectTransform == null)
            canvasRectTransform = hudCanvas.GetComponent<RectTransform>();

        if (hudCoinTarget == null && hudCanvas != null)
        {
            var icon = hudCanvas.transform.Find("TopPanel/CoinsPanel/CoinIcon_Image");
            if (icon != null) hudCoinTarget = icon.GetComponent<RectTransform>();
        }

        if (coinSprite == null && hudCoinTarget != null)
        {
            var img = hudCoinTarget.GetComponent<Image>();
            if (img != null) coinSprite = img.sprite;
        }

        if (flyingContainer == null && canvasRectTransform != null)
        {
            var existing = canvasRectTransform.Find("FlyingCoinsContainer");
            if (existing != null)
            {
                flyingContainer = existing.GetComponent<RectTransform>();
            }
            else
            {
                GameObject containerObj = new GameObject("FlyingCoinsContainer", typeof(RectTransform));
                containerObj.transform.SetParent(canvasRectTransform, false);
                containerObj.transform.SetAsLastSibling();
                flyingContainer = containerObj.GetComponent<RectTransform>();
                flyingContainer.anchorMin = Vector2.zero;
                flyingContainer.anchorMax = Vector2.one;
                flyingContainer.sizeDelta = Vector2.zero;
                flyingContainer.anchoredPosition = Vector2.zero;
            }
        }
    }

    /// <summary>
    /// Spawns flying coin tokens from world position and floats a +1 at the HUD.
    /// </summary>
    public void PlayCoinPickupAnimation(Vector3 worldPosition, int amount = 1)
    {
        if (!gameObject.activeInHierarchy || flyingContainer == null)
        {
            AutoFindReferences();
            if (flyingContainer == null) return;
        }

        if (mainCamera == null) mainCamera = Camera.main;

        // Convert world position of the collected coin to Canvas anchored position
        Vector2 screenPos;
        if (mainCamera != null)
        {
            screenPos = RectTransformUtility.WorldToScreenPoint(mainCamera, worldPosition);
        }
        else
        {
            screenPos = new Vector2(Screen.width * 0.5f, Screen.height * 0.3f);
        }

        Vector2 startAnchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, screenPos, hudCanvas != null && hudCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? hudCanvas.worldCamera : null, out startAnchoredPos);

        Vector2 targetAnchoredPos = Vector2.zero;
        if (hudCoinTarget != null)
        {
            Vector3 worldTarget = hudCoinTarget.position;
            Vector2 targetScreen = RectTransformUtility.WorldToScreenPoint(null, worldTarget);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, targetScreen, hudCanvas != null && hudCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? hudCanvas.worldCamera : null, out targetAnchoredPos);
        }
        else
        {
            targetAnchoredPos = new Vector2(Screen.width * 0.4f, Screen.height * 0.45f);
        }

        int count = Mathf.Clamp(coinsPerPickup, 1, 4);
        for (int i = 0; i < count; i++)
        {
            StartCoroutine(AnimateSingleFlyingCoin(startAnchoredPos, targetAnchoredPos, i * 0.05f, i == count - 1));
        }

        StartCoroutine(AnimateFloatingText(targetAnchoredPos, amount));
    }

    private IEnumerator AnimateSingleFlyingCoin(Vector2 startPos, Vector2 targetPos, float delay, bool isLastCoin)
    {
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        GameObject coinObj = GetPooledFlyingCoin();
        coinObj.SetActive(true);
        RectTransform rt = coinObj.GetComponent<RectTransform>();

        // Randomized control point for Bezier curve arc
        float arcX = Random.Range(-120f, 120f);
        float arcY = Random.Range(60f, 180f);
        Vector2 controlPoint = Vector2.Lerp(startPos, targetPos, 0.45f) + new Vector2(arcX, arcY);

        float elapsed = 0f;
        rt.anchoredPosition = startPos;
        rt.sizeDelta = Vector2.one * coinStartSize;
        rt.localScale = Vector3.zero;

        // Fast pop-in
        float popDuration = 0.1f;
        while (elapsed < popDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / popDuration;
            rt.localScale = Vector3.one * Mathf.Lerp(0f, 1.25f, t);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < flyDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / flyDuration);
            // Ease in out quad
            float easeT = t * t * (3f - 2f * t);

            // Quadratic bezier
            Vector2 p0 = Vector2.Lerp(startPos, controlPoint, easeT);
            Vector2 p1 = Vector2.Lerp(controlPoint, targetPos, easeT);
            rt.anchoredPosition = Vector2.Lerp(p0, p1, easeT);

            // 3D-like spin illusion
            float spin = (t * 720f) % 360f;
            float cosScale = Mathf.Abs(Mathf.Cos(spin * Mathf.Deg2Rad));
            rt.localScale = new Vector3(Mathf.Max(0.2f, cosScale), 1f, 1f);

            // Scale down towards HUD
            float currentSize = Mathf.Lerp(coinStartSize, coinEndSize, easeT);
            rt.sizeDelta = Vector2.one * currentSize;

            yield return null;
        }

        rt.anchoredPosition = targetPos;
        coinObj.SetActive(false);

        // When coin arrives at HUD, trigger the punch on the HUD coin counter
        if (isLastCoin)
        {
            TriggerHudCounterPunch();
        }
    }

    private void TriggerHudCounterPunch()
    {
        if (hudCoinTarget != null)
        {
            var controller = hudCoinTarget.GetComponentInParent<CoinUIController>();
            if (controller != null)
            {
                controller.TriggerCoinPunch();
            }
        }
    }

    private IEnumerator AnimateFloatingText(Vector2 hudTargetPos, int amount)
    {
        // Stagger popup so it appears as coins hit the HUD
        yield return new WaitForSecondsRealtime(flyDuration * 0.7f);

        GameObject textObj = GetPooledFloatingText();
        textObj.SetActive(true);
        RectTransform rt = textObj.GetComponent<RectTransform>();
        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();

        tmp.text = $"+{amount}";
        tmp.color = textColor;

        // Position slightly below and to the side of HUD coin counter
        Vector2 startPos = hudTargetPos + new Vector2(25f, -10f);
        Vector2 endPos = startPos + new Vector2(0f, textFloatDistance);

        float elapsed = 0f;
        while (elapsed < textDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / textDuration);

            // Smooth upward float
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, t));

            // Punch scale at start (pop to 1.35 then back to 1.0)
            float scale = 1f;
            if (t < 0.3f)
            {
                scale = Mathf.Lerp(0.5f, 1.35f, t / 0.3f);
            }
            else
            {
                scale = Mathf.Lerp(1.35f, 1f, (t - 0.3f) / 0.7f);
            }
            rt.localScale = Vector3.one * scale;

            // Fade out in last 40%
            float alpha = 1f;
            if (t > 0.6f)
            {
                alpha = Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f);
            }
            Color c = textColor;
            c.a = alpha;
            tmp.color = c;

            yield return null;
        }

        textObj.SetActive(false);
    }

    private GameObject GetPooledFlyingCoin()
    {
        for (int i = 0; i < flyingCoinPool.Count; i++)
        {
            if (flyingCoinPool[i] != null && !flyingCoinPool[i].activeSelf)
                return flyingCoinPool[i];
        }

        GameObject obj = new GameObject("FlyingCoin", typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(flyingContainer, false);
        Image img = obj.GetComponent<Image>();
        img.sprite = coinSprite;
        img.raycastTarget = false;
        flyingCoinPool.Add(obj);
        return obj;
    }

    private GameObject GetPooledFloatingText()
    {
        for (int i = 0; i < floatingTextPool.Count; i++)
        {
            if (floatingTextPool[i] != null && !floatingTextPool[i].activeSelf)
                return floatingTextPool[i];
        }

        GameObject obj = new GameObject("FloatingCoinText", typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(flyingContainer, false);
        TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
        if (fontAsset != null) tmp.font = fontAsset;
        tmp.fontSize = 26;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        floatingTextPool.Add(obj);
        return obj;
    }
}

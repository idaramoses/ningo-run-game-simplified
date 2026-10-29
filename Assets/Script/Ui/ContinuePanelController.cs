using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Continue panel similar to Subway Surfers "Save Me" - shown when the runner fails.
/// The player has a short countdown to either spend coins, spend gems, or watch a
/// rewarded video to keep running. Coin cost doubles each continue: 20, 40, 80, 160...
///
/// Rewarded ads: no ads SDK is installed in this project. Assign
/// <see cref="RewardedAdProvider"/> from your ads SDK wrapper (e.g. Unity Ads /
/// AdMob) to make the video button live. Signature:
///     RewardedAdProvider = (onRewarded, onUnavailable) => { ... };
/// In the Editor a simulated ad is used so the flow can be tested end to end.
/// </summary>
public class ContinuePanelController : MonoBehaviour
{
    /// <summary>
    /// Set by an ads SDK wrapper. Args: (onRewarded, onUnavailableOrSkipped).
    /// When null: in the Editor a simulated ad runs; in device builds the
    /// watch-ad button is hidden.
    /// </summary>
    public static System.Action<System.Action, System.Action> RewardedAdProvider;

    [Header("UI Elements")]
    public TMPro.TMP_Text titleText;
    public TMPro.TMP_Text subtitleText;
    public TMPro.TMP_Text countdownText;
    public UnityEngine.UI.Button continueButton;
    public TMPro.TMP_Text continueButtonText;
    public UnityEngine.UI.Button gemButton;
    public TMPro.TMP_Text gemButtonText;
    public UnityEngine.UI.Button watchAdButton;
    public TMPro.TMP_Text watchAdButtonText;
    public UnityEngine.UI.Button giveUpButton;

    [Header("Continue Settings")]
    [SerializeField] private int baseCost = 20;
    [SerializeField] private int baseGemCost = 2;
    [SerializeField] private float countdownDuration = 5f;

    private int currentCost;
    private int currentGemCost;
    private int continueCount = 0;
    private float countdownTimer;
    private bool isCountingDown = false;
    private System.Action onContinue;
    private System.Action onGiveUp;

    private void Awake()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinuePressed);

        if (gemButton != null)
            gemButton.onClick.AddListener(OnGemContinuePressed);

        if (watchAdButton != null)
            watchAdButton.onClick.AddListener(OnWatchAdPressed);

        if (giveUpButton != null)
            giveUpButton.onClick.AddListener(OnGiveUpPressed);
    }

    /// <summary>
    /// Builds a functional "Canvas_Retry" Save Me panel entirely in code so the
    /// feature works even when no hand-crafted panel is assigned in the scene.
    /// Assign UImanager.continuePanel to a designed panel to override this look.
    /// </summary>
    public static ContinuePanelController CreateDefault()
    {
        var canvasGo = new GameObject("Canvas_Retry");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 60;
        var scaler = canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        Object.DontDestroyOnLoad(canvasGo);

        var ctrl = canvasGo.AddComponent<ContinuePanelController>();

        TMP_FontAsset font = TMP_Settings.defaultFontAsset;

        // Full-screen dim backdrop (also acts as the give-up tap target)
        var backdrop = NewUI("Backdrop", canvasGo.transform);
        Stretch(backdrop);
        var backdropImg = backdrop.gameObject.AddComponent<Image>();
        backdropImg.color = new Color(0f, 0f, 0f, 0.65f);
        ctrl.giveUpButton = backdrop.gameObject.AddComponent<Button>();
        ctrl.giveUpButton.transition = Selectable.Transition.None;

        // Center panel
        var panel = NewUI("Panel", canvasGo.transform);
        var prt = (RectTransform)panel;
        prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0.55f);
        prt.sizeDelta = new Vector2(760f, 760f);
        var panelImg = panel.gameObject.AddComponent<Image>();
        panelImg.color = new Color(0.12f, 0.12f, 0.18f, 0.95f);

        ctrl.titleText = NewText("Title", panel, "SAVE ME !!!", 72,
            new Vector2(0.5f, 1f), new Vector2(700f, 110f), new Vector2(0f, -80f), font);
        ctrl.titleText.color = new Color(1f, 0.55f, 0.1f);

        ctrl.subtitleText = NewText("Subtitle", panel, "WOULD U LIKE TO KEEP RUNNING", 36,
            new Vector2(0.5f, 1f), new Vector2(700f, 60f), new Vector2(0f, -180f), font);
        ctrl.subtitleText.color = Color.white;

        ctrl.countdownText = NewText("Countdown", panel, "5", 96,
            new Vector2(0.5f, 1f), new Vector2(200f, 140f), new Vector2(0f, -290f), font);
        ctrl.countdownText.color = Color.white;

        ctrl.continueButton = NewButton("CoinButton", panel, new Vector2(0f, -430f),
            new Color(0.95f, 0.65f, 0.1f), out ctrl.continueButtonText, font);
        ctrl.gemButton = NewButton("GemButton", panel, new Vector2(0f, -555f),
            new Color(0.2f, 0.6f, 0.95f), out ctrl.gemButtonText, font);
        ctrl.watchAdButton = NewButton("AdButton", panel, new Vector2(0f, -680f),
            new Color(0.25f, 0.75f, 0.35f), out ctrl.watchAdButtonText, font);

        canvasGo.SetActive(false);
        return ctrl;
    }

    private static RectTransform NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    private static TMP_Text NewText(string name, Transform parent, string text, float size,
        Vector2 anchor, Vector2 rect, Vector2 pos, TMP_FontAsset font)
    {
        var rt = NewUI(name, parent);
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.sizeDelta = rect;
        rt.anchoredPosition = pos;
        var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        if (font != null) tmp.font = font;
        return tmp;
    }

    private static Button NewButton(string name, Transform parent, Vector2 pos,
        Color color, out TMP_Text label, TMP_FontAsset font)
    {
        var rt = NewUI(name, parent);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(620f, 100f);
        rt.anchoredPosition = pos;
        var img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        var btn = rt.gameObject.AddComponent<Button>();
        label = NewText("Label", rt, "", 42, new Vector2(0.5f, 0.5f),
            new Vector2(600f, 80f), Vector2.zero, font);
        label.color = Color.white;
        return btn;
    }

    /// <summary>
    /// Show the continue panel with callbacks for continue/give up
    /// </summary>
    public void Show(System.Action onContinueCallback, System.Action onGiveUpCallback)
    {
        onContinue = onContinueCallback;
        onGiveUp = onGiveUpCallback;

        // Costs double each time: 20, 40, 80... coins and 2, 4, 8... gems
        currentCost = baseCost * (int)Mathf.Pow(2, continueCount);
        currentGemCost = baseGemCost * (int)Mathf.Pow(2, continueCount);

        int playerCoins = PlayerPrefs.GetInt("Coins", 0);
        int playerGems = PlayerPrefs.GetInt("Gems", 0);

        if (titleText != null)
            titleText.text = "SAVE ME !!!";

        if (subtitleText != null)
            subtitleText.text = "WOULD U LIKE TO KEEP RUNNING";

        if (continueButtonText != null)
            continueButtonText.text = $"CONTINUE  X {currentCost}";

        if (continueButton != null)
        {
            bool canAfford = playerCoins >= currentCost;
            continueButton.interactable = canAfford;
            if (continueButtonText != null && !canAfford)
                continueButtonText.text = "NOT ENOUGH COINS";
        }

        if (gemButtonText != null)
            gemButtonText.text = $"GEMS  X {currentGemCost}";

        if (gemButton != null)
        {
            bool canAffordGems = playerGems >= currentGemCost;
            gemButton.interactable = canAffordGems;
            if (gemButtonText != null && !canAffordGems)
                gemButtonText.text = "NOT ENOUGH GEMS";
        }

        if (watchAdButton != null)
        {
#if UNITY_EDITOR
            bool adAvailable = true;
#else
            bool adAvailable = RewardedAdProvider != null;
#endif
            watchAdButton.gameObject.SetActive(adAvailable);
            watchAdButton.interactable = adAvailable;
            if (watchAdButtonText != null)
                watchAdButtonText.text = "WATCH VIDEO";
        }

        Debug.Log($"[ContinuePanel] Shown - CoinCost: {currentCost}, GemCost: {currentGemCost}, Coins: {playerCoins}, Gems: {playerGems}, Continues: {continueCount}");

        StartCountdown();
    }


    private void StartCountdown()
    {
        countdownTimer = countdownDuration;
        isCountingDown = true;
        StartCoroutine(CountdownCoroutine());
    }

    private IEnumerator CountdownCoroutine()
    {
        while (isCountingDown && countdownTimer > 0)
        {
            if (countdownText != null)
            {
                countdownText.text = Mathf.CeilToInt(countdownTimer).ToString();
            }

            countdownTimer -= Time.unscaledDeltaTime; // Use unscaled time so it works even when game is paused
            yield return null;
        }

        // Time's up - auto give up
        if (isCountingDown)
        {
            Debug.Log("[ContinuePanel] Countdown expired - auto give up");
            OnGiveUpPressed();
        }
    }

    private void OnContinuePressed()
    {
        int playerCoins = PlayerPrefs.GetInt("Coins", 0);

        if (playerCoins < currentCost)
        {
            Debug.LogWarning($"[ContinuePanel] Not enough coins! Need {currentCost}, have {playerCoins}");
            return;
        }

        playerCoins -= currentCost;
        PlayerPrefs.SetInt("Coins", playerCoins);
        PlayerPrefs.Save();

        Debug.Log($"[ContinuePanel] Continue with coins! Spent {currentCost}. Remaining: {playerCoins}");
        AcceptContinue();
    }

    private void OnGemContinuePressed()
    {
        int playerGems = PlayerPrefs.GetInt("Gems", 0);

        if (playerGems < currentGemCost)
        {
            Debug.LogWarning($"[ContinuePanel] Not enough gems! Need {currentGemCost}, have {playerGems}");
            return;
        }

        playerGems -= currentGemCost;
        PlayerPrefs.SetInt("Gems", playerGems);
        PlayerPrefs.Save();

        Debug.Log($"[ContinuePanel] Continue with gems! Spent {currentGemCost}. Remaining: {playerGems}");
        AcceptContinue();
    }

    private void OnWatchAdPressed()
    {
        if (watchAdButton != null) watchAdButton.interactable = false;

        System.Action rewarded = () =>
        {
            Debug.Log("[ContinuePanel] Rewarded video completed - continuing");
            AcceptContinue();
        };

        System.Action unavailable = () =>
        {
            Debug.LogWarning("[ContinuePanel] Rewarded video unavailable or skipped");
            if (watchAdButton != null) watchAdButton.interactable = true;
        };

        if (RewardedAdProvider != null)
        {
            RewardedAdProvider(rewarded, unavailable);
        }
#if UNITY_EDITOR
        else
        {
            // Simulated ad so the flow is testable in the Editor
            StartCoroutine(SimulateAdCoroutine(rewarded));
        }
#else
        else
        {
            unavailable();
        }
#endif
    }

#if UNITY_EDITOR
    private IEnumerator SimulateAdCoroutine(System.Action rewarded)
    {
        Debug.Log("[ContinuePanel] Simulating rewarded video (1s) - assign RewardedAdProvider for real ads");
        yield return new WaitForSecondsRealtime(1f);
        rewarded();
    }
#endif

    private void AcceptContinue()
    {
        continueCount++;
        StopAllCoroutines();
        isCountingDown = false;
        gameObject.SetActive(false);
        onContinue?.Invoke();
    }

    private void OnGiveUpPressed()
    {
        Debug.Log("[ContinuePanel] Give up pressed");

        StopAllCoroutines();
        isCountingDown = false;
        gameObject.SetActive(false);
        onGiveUp?.Invoke();
    }

    /// <summary>
    /// Reset the continue count (call this when level is completed or restarted)
    /// </summary>
    public void ResetContinueCount()
    {
        continueCount = 0;
        Debug.Log("[ContinuePanel] Continue count reset");
    }

    /// <summary>
    /// Get the current cost to continue
    /// </summary>
    public int GetCurrentCost()
    {
        return baseCost * (int)Mathf.Pow(2, continueCount);
    }

    /// <summary>
    /// Get how many times player has continued this session
    /// </summary>
    public int GetContinueCount()
    {
        return continueCount;
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinuePressed);

        if (gemButton != null)
            gemButton.onClick.RemoveListener(OnGemContinuePressed);

        if (watchAdButton != null)
            watchAdButton.onClick.RemoveListener(OnWatchAdPressed);

        if (giveUpButton != null)
            giveUpButton.onClick.RemoveListener(OnGiveUpPressed);
    }
}

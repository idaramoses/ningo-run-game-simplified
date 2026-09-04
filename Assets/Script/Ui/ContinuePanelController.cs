using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Continue panel similar to Subway Surfers - allows players to spend coins to continue after failing.
/// Cost doubles each time: 20, 40, 80, 160, etc.
/// </summary>
public class ContinuePanelController : MonoBehaviour
{
    [Header("UI Elements")]
    public TMPro.TMP_Text titleText;
    public TMPro.TMP_Text subtitleText;
    public TMPro.TMP_Text countdownText;
    public UnityEngine.UI.Button continueButton;
    public TMPro.TMP_Text continueButtonText;
    public UnityEngine.UI.Button giveUpButton;

    [Header("Continue Settings")]
    [SerializeField] private int baseCost = 20;
    [SerializeField] private float countdownDuration = 5f;
    [SerializeField] private bool resetCostOnLevelComplete = true;

    private int currentCost;
    private int continueCount = 0;
    private float countdownTimer;
    private bool isCountingDown = false;
    private System.Action onContinue;
    private System.Action onGiveUp;

    private void Awake()
    {
        // Wire up button listeners
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinuePressed);
        
        if (giveUpButton != null)
            giveUpButton.onClick.AddListener(OnGiveUpPressed);
    }

    /// <summary>
    /// Show the continue panel with callbacks for continue/give up
    /// </summary>
    public void Show(System.Action onContinueCallback, System.Action onGiveUpCallback)
    {
        onContinue = onContinueCallback;
        onGiveUp = onGiveUpCallback;

        // Calculate cost: 20, 40, 80, 160, etc. (doubles each time)
        currentCost = baseCost * (int)Mathf.Pow(2, continueCount);

        // Get player's current coins
        int playerCoins = PlayerPrefs.GetInt("Coins", 0);

        // Update UI - Title
        if (titleText != null)
            titleText.text = "SAVE ME !!!";

        // Update UI - Subtitle
        if (subtitleText != null)
            subtitleText.text = "WOULD U LIKE TO KEEP RUNNING";

        // Update continue button text: "CONTINUE X 20" format
        if (continueButtonText != null)
        {
            continueButtonText.text = $"CONTINUE  X {currentCost}";
        }

        // Enable/disable continue button based on if player has enough coins
        if (continueButton != null)
        {
            bool canAfford = playerCoins >= currentCost;
            continueButton.interactable = canAfford;
            
            // Update button text if can't afford
            if (continueButtonText != null && !canAfford)
            {
                continueButtonText.text = "NOT ENOUGH COINS";
            }
        }

        Debug.Log($"[ContinuePanel] Shown - Cost: {currentCost}, Player Coins: {playerCoins}, Continue Count: {continueCount}");
        
        // Start countdown (GameObject is already active by UImanager)
        StartCountdown();
    }


    private void StartCountdown()
    {
        countdownTimer = countdownDuration;
        isCountingDown = true;
        StartCoroutine(CountdownCoroutine());
    }

    private void StopCountdown()
    {
        isCountingDown = false;
        StopAllCoroutines();
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

        // Check if player has enough coins
        if (playerCoins < currentCost)
        {
            Debug.LogWarning($"[ContinuePanel] Not enough coins! Need {currentCost}, have {playerCoins}");
            return;
        }

        // Deduct coins
        playerCoins -= currentCost;
        PlayerPrefs.SetInt("Coins", playerCoins);
        PlayerPrefs.Save();

        Debug.Log($"[ContinuePanel] Continue! Spent {currentCost} coins. Remaining: {playerCoins}");

        // Increment continue count for next time
        continueCount++;

        // Hide panel and trigger continue callback
        StopAllCoroutines();
        gameObject.SetActive(false);
        onContinue?.Invoke();
    }

    private void OnGiveUpPressed()
    {
        Debug.Log("[ContinuePanel] Give up pressed");

        // Hide panel and trigger give up callback
        StopAllCoroutines();
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
        // Clean up button listeners
        if (continueButton != null)
            continueButton.onClick.RemoveListener(OnContinuePressed);
        
        if (giveUpButton != null)
            giveUpButton.onClick.RemoveListener(OnGiveUpPressed);
    }
}

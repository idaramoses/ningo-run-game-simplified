using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class DailyGiftController : MonoBehaviour
{
    public enum RewardType { Coins, MysteryBox, Speedstar }
    [Header("Header")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subtitleText;

    [Header("Today's Reward Panel")]
    [SerializeField] private TMP_Text currentDayText;
    [SerializeField] private TMP_Text rewardAmountText;
    [SerializeField] private Image rewardIconImage;
    [SerializeField] private TMP_Text claimButtonText;

    [Header("Day Cards")]
    [SerializeField] private DayCard[] dayCards = new DayCard[7];

    [Header("Reset Timer")]
    [SerializeField] private TMP_Text resetTimerText;

    [Header("Reward Settings")]
    [SerializeField] private int[] dailyRewards = { 50, 100, 150, 100, 150, 300, 500 };

    private bool hasClaimedToday = false;
    private DateTime lastClaimDate;

    [System.Serializable]
    public class DayCard
    {
        public GameObject cardObject;
        public Image backgroundImage;
        public Image rewardImage;
        public TMP_Text rewardText;
        public TMP_Text dayLabel;
        public GameObject checkmark;
        public GameObject plusIcon;
        public RewardType rewardType;
    }

    [Header("Activate Canvases")]
    [SerializeField] private GameObject canvasActivateReward;
    [SerializeField] private GameObject canvasActivateMysteryBox;
    [SerializeField] private GameObject canvasActivateSpeedstar;

    private void Start()
    {
        LoadDailyGiftData();
        SetupUI();
        
        // NOTE: Wire CloseButton OnClick() → OnClosePressed() in Inspector
        // NOTE: Wire ClaimButton OnClick() → ClaimReward() in Inspector
    }

    private void Update()
    {
        UpdateResetTimer();
    }

    private void LoadDailyGiftData()
    {
        DateTime today = DateTime.Today;
        
        // Find current week's Sunday
        int diff = (7 + (today.DayOfWeek - DayOfWeek.Sunday)) % 7;
        DateTime currentWeekSunday = today.AddDays(-diff).Date;
        string currentWeekSundayStr = currentWeekSunday.ToString("yyyy-MM-dd");

        string storedWeekSundayStr = PlayerPrefs.GetString("DailyGiftWeekStart", "");

        // If it's a new week, clear all 7 claim flags
        if (storedWeekSundayStr != currentWeekSundayStr)
        {
            for (int i = 0; i < 7; i++)
            {
                PlayerPrefs.SetInt($"DailyGiftClaimed_{i}", 0);
            }
            PlayerPrefs.SetString("DailyGiftWeekStart", currentWeekSundayStr);
            PlayerPrefs.Save();
        }

        int todayIndex = (int)today.DayOfWeek; // 0 = Sunday, 1 = Monday, ..., 6 = Saturday
        hasClaimedToday = PlayerPrefs.GetInt($"DailyGiftClaimed_{todayIndex}", 0) == 1;
    }

    private void SetupUI()
    {
        // Set header texts
        if (titleText != null)
            titleText.text = "DAILY GIFT";
        if (subtitleText != null)
            subtitleText.text = "Come back everyday and claim\nawesome rewards!";

        int todayIndex = (int)DateTime.Today.DayOfWeek;

        // Set current day display
        if (currentDayText != null)
        {
            string[] weekDaysUpper = { "SUNDAY", "MONDAY", "TUESDAY", "WEDNESDAY", "THURSDAY", "FRIDAY", "SATURDAY" };
            currentDayText.text = weekDaysUpper[todayIndex];
        }

        // Set reward amount text in Today's Reward Panel
        if (rewardAmountText != null && todayIndex < dayCards.Length)
        {
            DayCard todayCard = dayCards[todayIndex];
            if (todayCard != null)
            {
                if (todayCard.rewardType == RewardType.Coins && todayIndex < dailyRewards.Length)
                {
                    rewardAmountText.text = $"x{dailyRewards[todayIndex]}";
                }
                else if (todayCard.rewardType == RewardType.MysteryBox)
                {
                    rewardAmountText.text = "MYSTERY BOX";
                }
                else if (todayCard.rewardType == RewardType.Speedstar)
                {
                    rewardAmountText.text = "SPEEDSTAR";
                }
            }
        }

        // Update claim button
        UpdateClaimButton();

        // Setup day cards
        SetupDayCards();
    }

    private void SetupDayCards()
    {
        int todayIndex = (int)DateTime.Today.DayOfWeek;
        string[] weekDays = { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        for (int i = 0; i < dayCards.Length && i < 7; i++)
        {
            if (dayCards[i].cardObject == null) continue;

            DayCard card = dayCards[i];
            bool isClaimed = PlayerPrefs.GetInt($"DailyGiftClaimed_{i}", 0) == 1;
            bool isToday = (i == todayIndex) && !hasClaimedToday;
            bool isFuture = i > todayIndex;
            bool isPast = i < todayIndex;

            // Set day label
            if (card.dayLabel != null)
            {
                if (i == todayIndex)
                    card.dayLabel.text = "Today";
                else
                    card.dayLabel.text = weekDays[i];
            }

            // Set reward text
            if (card.rewardText != null)
            {
                if (card.rewardType == RewardType.Coins)
                {
                    if (i < dailyRewards.Length)
                        card.rewardText.text = $"x{dailyRewards[i]} COINS";
                }
                else if (card.rewardType == RewardType.MysteryBox)
                {
                    card.rewardText.text = "MYSTERY BOX";
                }
                else if (card.rewardType == RewardType.Speedstar)
                {
                    card.rewardText.text = "SPEEDSTAR";
                }
            }

            // Show/hide checkmark for claimed days
            if (card.checkmark != null)
                card.checkmark.SetActive(isClaimed);

            // Show/hide plus icon for future days
            if (card.plusIcon != null)
                card.plusIcon.SetActive((isFuture || (i == todayIndex && hasClaimedToday)) && !isToday && !isClaimed);

            // Set interactable state of the card's button (only clickable if it is today's unclaimed gift)
            UnityEngine.UI.Button cardButton = card.cardObject.GetComponent<UnityEngine.UI.Button>();
            if (cardButton != null)
            {
                cardButton.interactable = isToday;
            }

            // Update card appearance based on state
            UpdateCardAppearance(card, isClaimed, isToday, isFuture || isPast);
        }
    }

    private void UpdateCardAppearance(DayCard card, bool isClaimed, bool isToday, bool isOther)
    {
        if (card.backgroundImage == null) return;

        if (isToday)
        {
            // Highlight today's card (pink/magenta tint)
            card.backgroundImage.color = new Color(1f, 0.4f, 0.7f, 1f);
        }
        else if (isClaimed)
        {
            // Claimed cards (cyan/blue tint)
            card.backgroundImage.color = new Color(0.4f, 0.8f, 1f, 1f);
        }
        else
        {
            // Future / Missed cards (normal blue)
            card.backgroundImage.color = new Color(0.3f, 0.6f, 1f, 1f);
        }
    }

    private void UpdateClaimButton()
    {
        if (claimButtonText != null)
            claimButtonText.text = hasClaimedToday ? "CLAIMED" : "CLAIM";

        // Find and update the Claim Button component
        UnityEngine.UI.Button btn = null;
        if (claimButtonText != null)
            btn = claimButtonText.GetComponentInParent<UnityEngine.UI.Button>();

        if (btn != null)
        {
            btn.interactable = !hasClaimedToday;
        }
    }

    public void ClaimReward()
    {
        if (hasClaimedToday) return;

        int todayIndex = (int)DateTime.Today.DayOfWeek;
        DayCard card = dayCards[todayIndex];
        RewardType type = card.rewardType;

        int rewardCoins = dailyRewards[todayIndex];

        // 1. Process Reward addition based on Type and save to PlayerPrefs
        switch (type)
        {
            case RewardType.Coins:
                int currentCoins = PlayerPrefs.GetInt("Coins", 0);
                PlayerPrefs.SetInt("Coins", currentCoins + rewardCoins);
                
                if (canvasActivateReward != null)
                {
                    TMP_Text coinAddedText = canvasActivateReward.transform.Find("MainBody/CoinAddedText")?.GetComponent<TMP_Text>();
                    if (coinAddedText != null)
                    {
                        coinAddedText.text = $"+{rewardCoins} COINS";
                    }
                }
                break;

            case RewardType.MysteryBox:
                int coinsMB = PlayerPrefs.GetInt("Coins", 0);
                int gemsMB = PlayerPrefs.GetInt("Gems", 0);
                PlayerPrefs.SetInt("Coins", coinsMB + 300);
                PlayerPrefs.SetInt("Gems", gemsMB + 5);

                if (canvasActivateMysteryBox != null)
                {
                    TMP_Text mistryBoxText = canvasActivateMysteryBox.transform.Find("MainBody/MistryBoxText")?.GetComponent<TMP_Text>();
                    if (mistryBoxText != null)
                    {
                        mistryBoxText.text = "MYSTERY REWARD!\n+300 COINS\n+5 GEMS";
                    }
                }
                break;

            case RewardType.Speedstar:
                int coinsSS = PlayerPrefs.GetInt("Coins", 0);
                int energySS = PlayerPrefs.GetInt("Energy", 5);
                PlayerPrefs.SetInt("Coins", coinsSS + 200);
                PlayerPrefs.SetInt("Energy", energySS + 3);

                if (canvasActivateSpeedstar != null)
                {
                    TMP_Text speedstarText = canvasActivateSpeedstar.transform.Find("MainBody/Text")?.GetComponent<TMP_Text>();
                    if (speedstarText != null)
                    {
                        speedstarText.text = "SPEED BOOST!\n+200 COINS\n+3 ENERGY";
                    }
                }
                break;
        }

        // 2. Mark today's index as claimed
        PlayerPrefs.SetInt($"DailyGiftClaimed_{todayIndex}", 1);
        PlayerPrefs.SetString("LastDailyGiftClaim", DateTime.Today.ToString("yyyy-MM-dd"));
        PlayerPrefs.Save();

        hasClaimedToday = true;

        Debug.Log($"[DailyGift] Claimed reward! Type: {type}, day: {DateTime.Today.DayOfWeek}");

        // Hide daily gift canvas first
        gameObject.SetActive(false);

        // 3. Show Congratulations / Activation Screen
        switch (type)
        {
            case RewardType.Coins:
                if (canvasActivateReward != null) canvasActivateReward.SetActive(true);
                break;
            case RewardType.MysteryBox:
                if (canvasActivateMysteryBox != null) canvasActivateMysteryBox.SetActive(true);
                break;
            case RewardType.Speedstar:
                if (canvasActivateSpeedstar != null) canvasActivateSpeedstar.SetActive(true);
                break;
        }

        // Refresh UI
        SetupUI();
    }

    private void UpdateResetTimer()
    {
        if (resetTimerText == null) return;

        DateTime tomorrow = DateTime.Today.AddDays(1);
        TimeSpan timeUntilReset = tomorrow - DateTime.Now;
        
        resetTimerText.text = $"{timeUntilReset.Hours:D2}:{timeUntilReset.Minutes:D2}:{timeUntilReset.Seconds:D2}";
    }

    public void Show()
    {
        LoadDailyGiftData();
        SetupUI();
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Assign this to CloseButton OnClick() in the Inspector.
    /// </summary>
    public void OnClosePressed()
    {
        UImanager homeCtrl = UImanager.uimanager;
        if (homeCtrl != null)
            homeCtrl.HideDailyGift();
        else
            gameObject.SetActive(false);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Call this from each DayCard button OnClick() — pass the card index (0-based).
    /// </summary>
    public void OnDayCardPressed(int cardIndex)
    {
        if (cardIndex < 0 || cardIndex >= dayCards.Length) return;

        // Security check: Only allow claiming today's reward if not yet claimed
        int todayIndex = (int)DateTime.Today.DayOfWeek;
        if (cardIndex != todayIndex || hasClaimedToday) return;

        // Claim reward! This will automatically award the prize, update player data,
        // and open the appropriate congratulations canvas.
        ClaimReward();
    }
}

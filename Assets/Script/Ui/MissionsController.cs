using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class MissionsController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private MissionData missionData;

    [Header("Header")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text subtitleText;

    [Header("Mission Cards Container")]
    [SerializeField] private Transform missionsContent;
    [SerializeField] private Transform completeAllPanel; // Drag CompleteAllPanel here, will move into scroll
    [SerializeField] private Transform newMissionTimer;  // Drag NewMissionTimer here, will move into scroll

    [Header("Complete All Panel")]
    [SerializeField] private TMP_Text completeAllTitle;
    [SerializeField] private TMP_Text completeAllSubtitle;
    [SerializeField] private Image allProgressFill;
    [SerializeField] private TMP_Text allProgressText;

    [Header("Timer")]
    [SerializeField] private TMP_Text countdownText;

    [Header("Mission Card Prefab")]
    [SerializeField] private MissionCardItem missionCardPrefab;

    [Header("Mission Icons")]
    [SerializeField] private Sprite iconCoins;
    [SerializeField] private Sprite iconObstacles;
    [SerializeField] private Sprite iconSkateboard;
    [SerializeField] private Sprite iconRun;
    [SerializeField] private Sprite iconWords;
    [SerializeField] private Sprite iconScore;
    [SerializeField] private Sprite iconPowerup;
    [SerializeField] private Sprite iconCompleteDaily;
    [SerializeField] private Sprite iconInvite;

    private List<RuntimeMission> missions = new List<RuntimeMission>();

    // Deferred-save state: mission progress is updated many times per second during
    // gameplay (every meter ran / coin collected). Writing PlayerPrefs to disk each
    // time causes visible frame stutter, so we mark progress dirty and flush at most
    // once every MISSION_FLUSH_INTERVAL seconds (and at key moments like Show/Claim).
    private bool missionsDirty = false;
    private float lastMissionFlushTime = -999f;
    private const float MISSION_FLUSH_INTERVAL = 5f;

    [System.Serializable]
    public class RuntimeMission
    {
        public string id;
        public string title;
        public int targetValue;
        public int currentValue;
        public int rewardAmount;
        public MissionData.MissionType type;
        public bool claimed;

        public float Progress => Mathf.Clamp01((float)currentValue / targetValue);
        public bool IsComplete => currentValue >= targetValue;
    }
    private DateTime nextMissionTime;

    private void Start()
    {
        LoadMissions();
        
        // NOTE: Wire CloseButton OnClick() → OnClosePressed() in Inspector

        // Set next mission time to midnight
        nextMissionTime = DateTime.Today.AddDays(1);
    }

    private void Update()
    {
        UpdateCountdownTimer();
    }

    private void LoadMissions()
    {
        // Persist any unsaved in-memory progress before reloading from PlayerPrefs,
        // otherwise deferred progress would be wiped.
        FlushMissionProgress();

        missions.Clear();

        // Load from PlayerPrefs or create default missions
        int missionCount = PlayerPrefs.GetInt("MissionCount", 0);
        
        bool hasStaleMissions = false;
        if (missionCount > 0)
        {
            // Dry run read to check if we have any stale/invalid types (e.g. spelling words)
            for (int i = 0; i < missionCount; i++)
            {
                var type = (MissionData.MissionType)PlayerPrefs.GetInt($"Mission_{i}_Type", 0);
                if (type == MissionData.MissionType.CollectWords || 
                    type == MissionData.MissionType.WatchAd || 
                    type == MissionData.MissionType.InviteFriend)
                {
                    hasStaleMissions = true;
                    break;
                }
            }
        }
        
        if (missionCount == 0 || IsDailyReset() || hasStaleMissions)
        {
            CreateDailyMissions();
        }
        else
        {
            for (int i = 0; i < missionCount; i++)
            {
                RuntimeMission m = new RuntimeMission
                {
                    id = PlayerPrefs.GetString($"Mission_{i}_ID", $"mission_{i}"),
                    title = PlayerPrefs.GetString($"Mission_{i}_Title", "Mission"),
                    targetValue = PlayerPrefs.GetInt($"Mission_{i}_Target", 100),
                    currentValue = PlayerPrefs.GetInt($"Mission_{i}_Current", 0),
                    rewardAmount = PlayerPrefs.GetInt($"Mission_{i}_Reward", 100),
                    type = (MissionData.MissionType)PlayerPrefs.GetInt($"Mission_{i}_Type", 0),
                    claimed = PlayerPrefs.GetInt($"Mission_{i}_Claimed", 0) == 1
                };
                missions.Add(m);
            }
        }

        UpdateUI();
    }

    private bool IsDailyReset()
    {
        string lastDate = PlayerPrefs.GetString("MissionsLastDate", "");
        string today = DateTime.Today.ToString("yyyy-MM-dd");
        return lastDate != today;
    }

    private void CreateDailyMissions()
    {
        missions.Clear();
        
        if (missionData != null && missionData.Entries.Count > 0)
        {
            // Use day of year + year as a seed so it's consistent for the same calendar day but changes daily
            int seed = DateTime.Today.Year * 1000 + DateTime.Today.DayOfYear;
            System.Random rng = new System.Random(seed);
            
            // Create a copy to shuffle
            List<MissionData.MissionEntry> availableEntries = new List<MissionData.MissionEntry>(missionData.Entries);
            
            // Shuffle
            int n = availableEntries.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                var value = availableEntries[k];
                availableEntries[k] = availableEntries[n];
                availableEntries[n] = value;
            }

            int dailyCount = Mathf.Min(5, availableEntries.Count);
            for (int i = 0; i < dailyCount; i++)
            {
                var entry = availableEntries[i];
                missions.Add(new RuntimeMission
                {
                    id = entry.id,
                    title = entry.title,
                    targetValue = entry.targetValue,
                    currentValue = 0,
                    rewardAmount = entry.rewardAmount,
                    type = entry.type,
                    claimed = false
                });
            }
        }
        else
        {
            // Fallback: create 5 default missions (no words/ads/invites!)
            missions.Add(new RuntimeMission { id = "collect_coins", title = "Collect 500 coins", targetValue = 500, currentValue = 0, rewardAmount = 100, type = MissionData.MissionType.CollectCoins, claimed = false });
            missions.Add(new RuntimeMission { id = "dodge_obstacles", title = "Dodge 20 obstacles", targetValue = 20, currentValue = 0, rewardAmount = 150, type = MissionData.MissionType.DodgeObstacles, claimed = false });
            missions.Add(new RuntimeMission { id = "use_skateboard", title = "Use 2 skateboard", targetValue = 2, currentValue = 0, rewardAmount = 100, type = MissionData.MissionType.UseSkateboard, claimed = false });
            missions.Add(new RuntimeMission { id = "run_meters", title = "Run 1,000 meters", targetValue = 1000, currentValue = 0, rewardAmount = 100, type = MissionData.MissionType.RunMeters, claimed = false });
            missions.Add(new RuntimeMission { id = "score_points", title = "Score 10,000 points", targetValue = 10000, currentValue = 0, rewardAmount = 100, type = MissionData.MissionType.ScorePoints, claimed = false });
        }

        SaveMissions();
        PlayerPrefs.SetString("MissionsLastDate", DateTime.Today.ToString("yyyy-MM-dd"));
        PlayerPrefs.Save();
    }

    private void SaveMissions()
    {
        PlayerPrefs.SetInt("MissionCount", missions.Count);
        
        for (int i = 0; i < missions.Count; i++)
        {
            RuntimeMission m = missions[i];
            PlayerPrefs.SetString($"Mission_{i}_ID", m.id);
            PlayerPrefs.SetString($"Mission_{i}_Title", m.title);
            PlayerPrefs.SetInt($"Mission_{i}_Target", m.targetValue);
            PlayerPrefs.SetInt($"Mission_{i}_Current", m.currentValue);
            PlayerPrefs.SetInt($"Mission_{i}_Reward", m.rewardAmount);
            PlayerPrefs.SetInt($"Mission_{i}_Type", (int)m.type);
            PlayerPrefs.SetInt($"Mission_{i}_Claimed", m.claimed ? 1 : 0);
        }
        
        PlayerPrefs.Save();
    }

    private void UpdateUI()
    {
        // Update complete all progress
        int completedCount = 0;
        int claimedCount = 0;
        
        foreach (var m in missions)
        {
            if (m.IsComplete) completedCount++;
            if (m.claimed) claimedCount++;
        }

        if (allProgressFill != null)
        {
            float progress = missions.Count > 0 ? (float)claimedCount / missions.Count : 0;
            allProgressFill.rectTransform.anchorMax = new Vector2(progress, 1);
        }

        if (allProgressText != null)
        {
            allProgressText.text = $"{claimedCount}/{missions.Count}";
        }

        // Update incomplete missions count for badge
        int incomplete = missions.Count - claimedCount;
        PlayerPrefs.SetInt("IncompleteMissions", incomplete);
        PlayerPrefs.Save();

        // Refresh spawned cards
        for (int i = 0; i < spawnedCards.Count && i < missions.Count; i++)
        {
            var m = missions[i];
            spawnedCards[i].UpdateProgress(m.currentValue, m.targetValue);
        }
    }

    private void UpdateCountdownTimer()
    {
        if (countdownText == null) return;

        TimeSpan timeLeft = nextMissionTime - DateTime.Now;
        
        if (timeLeft.TotalSeconds > 0)
        {
            countdownText.text = $"{timeLeft.Hours:D2}:{timeLeft.Minutes:D2}:{timeLeft.Seconds:D2}";
        }
        else
        {
            countdownText.text = "00:00:00";
            // Could trigger mission refresh here
        }
    }

    public void ClaimMission(int index)
    {
        if (index < 0 || index >= missions.Count) return;
        
        RuntimeMission m = missions[index];
        
        if (!m.IsComplete || m.claimed) return;

        FlushMissionProgress();

        // Give reward
        int coins = PlayerPrefs.GetInt("Coins", 0);
        PlayerPrefs.SetInt("Coins", coins + m.rewardAmount);
        
        m.claimed = true;
        
        SaveMissions();
        UpdateUI();
        
        Debug.Log($"[Missions] Claimed {m.rewardAmount} coins for: {m.title}");

        // Progress "CompleteDaily" mission if we claimed a regular mission
        if (m.type != MissionData.MissionType.CompleteDaily)
        {
            ProgressMission(MissionData.MissionType.CompleteDaily, 1);
        }
    }

    public void UpdateMissionProgress(MissionData.MissionType type, int amount)
    {
        bool changed = false;
        foreach (var m in missions)
        {
            if (m.type == type && !m.claimed && m.currentValue < m.targetValue)
            {
                m.currentValue = Mathf.Min(m.currentValue + amount, m.targetValue);
                changed = true;
            }
        }

        if (!changed) return;

        // Defer the expensive PlayerPrefs write + UI refresh; flush periodically.
        missionsDirty = true;
        if (Time.unscaledTime - lastMissionFlushTime >= MISSION_FLUSH_INTERVAL)
        {
            FlushMissionProgress();
        }
    }

    private void FlushMissionProgress()
    {
        if (!missionsDirty) return;
        missionsDirty = false;
        lastMissionFlushTime = Time.unscaledTime;
        SaveMissions();
        UpdateUI();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) FlushMissionProgress();
    }

    private void OnApplicationQuit()
    {
        FlushMissionProgress();
    }

    public void SetMissionProgress(MissionData.MissionType type, int value)
    {
        foreach (var m in missions)
        {
            if (m.type == type && !m.claimed)
            {
                m.currentValue = Mathf.Min(value, m.targetValue);
            }
        }
        
        SaveMissions();
        UpdateUI();
    }

    private List<MissionCardItem> spawnedCards = new List<MissionCardItem>();

    public void Show()
    {
        FlushMissionProgress();
        LoadMissions();
        SpawnMissionCards();
        gameObject.SetActive(true);
    }

    private void SpawnMissionCards()
    {
        // Clear existing cards
        foreach (var card in spawnedCards)
        {
            if (card != null) Destroy(card.gameObject);
        }
        spawnedCards.Clear();

        if (missionCardPrefab == null || missionsContent == null) return;

        // Spawn mission cards first (at top)
        for (int i = 0; i < missions.Count; i++)
        {
            var mission = missions[i];
            MissionCardItem card = Instantiate(missionCardPrefab, missionsContent);
            Sprite missionIcon = GetMissionIcon(mission.type);
            card.Setup(mission.id, mission.title, mission.currentValue, mission.targetValue, mission.rewardAmount, missionIcon, OnMissionClaimed);
            spawnedCards.Add(card);
        }

        // Move CompleteAllPanel and NewMissionTimer into scroll content at bottom (after all cards)
        if (completeAllPanel != null)
        {
            completeAllPanel.SetParent(missionsContent, false);
            completeAllPanel.SetAsLastSibling();
        }
        if (newMissionTimer != null)
        {
            newMissionTimer.SetParent(missionsContent, false);
            newMissionTimer.SetAsLastSibling();
        }
    }

    private Sprite GetMissionIcon(MissionData.MissionType type)
    {
        return type switch
        {
            MissionData.MissionType.CollectCoins => iconCoins,
            MissionData.MissionType.DodgeObstacles => iconObstacles,
            MissionData.MissionType.UseSkateboard => iconSkateboard,
            MissionData.MissionType.RunMeters => iconRun,
            MissionData.MissionType.CollectWords => iconWords,
            MissionData.MissionType.ScorePoints => iconScore,
            MissionData.MissionType.UsePowerup => iconPowerup,
            MissionData.MissionType.CompleteDaily => iconCompleteDaily,
            MissionData.MissionType.InviteFriend => iconInvite,
            _ => iconCoins
        };
    }

    private void OnMissionClaimed(string missionId)
    {
        int index = missions.FindIndex(m => m.id == missionId);
        if (index >= 0) ClaimMission(index);
    }

    /// <summary>
    /// Assign this to CloseButton OnClick() in the Inspector.
    /// </summary>
    public void OnClosePressed()
    {
        UImanager homeCtrl = UImanager.uimanager;
        if (homeCtrl != null)
            homeCtrl.HideMissions();
        else
            gameObject.SetActive(false);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // Static helper to get controller instance
    private static MissionsController _instance;
    public static MissionsController Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<MissionsController>();
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    public static void ProgressMission(MissionData.MissionType type, int amount)
    {
        if (amount <= 0) return;
        
        if (_instance != null)
        {
            _instance.UpdateMissionProgress(type, amount);
        }
        else
        {
            // Update directly in PlayerPrefs
            int missionCount = PlayerPrefs.GetInt("MissionCount", 0);
            bool updated = false;
            for (int i = 0; i < missionCount; i++)
            {
                int missionTypeInt = PlayerPrefs.GetInt($"Mission_{i}_Type", -1);
                if (missionTypeInt == (int)type)
                {
                    int claimed = PlayerPrefs.GetInt($"Mission_{i}_Claimed", 0);
                    if (claimed == 0)
                    {
                        int current = PlayerPrefs.GetInt($"Mission_{i}_Current", 0);
                        int target = PlayerPrefs.GetInt($"Mission_{i}_Target", 100);
                        if (current < target)
                        {
                            int nextVal = Mathf.Min(current + amount, target);
                            PlayerPrefs.SetInt($"Mission_{i}_Current", nextVal);
                            updated = true;
                        }
                    }
                }
            }
            // NOTE: No PlayerPrefs.Save() here - this runs many times per second during
            // gameplay (every meter ran, every coin collected) and a synchronous disk
            // flush each time causes visible stutter. Values are still written via
            // SetInt (in-memory); UImanager saves at run end and Unity
            // auto-flushes prefs on pause/quit.
        }
    }
}

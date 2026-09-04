using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MissionData", menuName = "Game/Mission Data")]
public class MissionData : ScriptableObject
{
    [SerializeField] private List<MissionEntry> entries = new List<MissionEntry>();

    public List<MissionEntry> Entries => entries;

    private void OnEnable()
    {
        if (entries == null || entries.Count == 0)
        {
            entries = GenerateDefaultEntries();
        }
    }

    [ContextMenu("Auto Fill Entries")]
    public void AutoFillEntries()
    {
        entries = GenerateDefaultEntries();
    }

    public void SetEntries(List<MissionEntry> newEntries)
    {
        entries = newEntries;
    }

    [System.Serializable]
    public class MissionEntry
    {
        public string id;
        public string title;
        public int targetValue;
        public int rewardAmount;
        public MissionType type;
    }

    public enum MissionType
    {
        CollectCoins,
        DodgeObstacles,
        UseSkateboard,
        RunMeters,
        CollectWords,
        ScorePoints,
        UsePowerup,
        CompleteDaily,
        WatchAd,
        InviteFriend
    }

    public static List<MissionEntry> GenerateDefaultEntries()
    {
        var missions = new List<MissionEntry>();
        int reward = 50;
        
        var activeTypes = new MissionType[]
        {
            MissionType.CollectCoins,
            MissionType.DodgeObstacles,
            MissionType.UseSkateboard,
            MissionType.RunMeters,
            MissionType.ScorePoints,
            MissionType.UsePowerup,
            MissionType.CompleteDaily
        };

        for (int i = 0; i < 50; i++)
        {
            var type = activeTypes[i % activeTypes.Length];
            var (title, target) = GetMissionDetails(type, i);
            
            missions.Add(new MissionEntry
            {
                id = $"mission_{i + 1}",
                title = title,
                targetValue = target,
                rewardAmount = reward,
                type = type
            });
            
            reward += 10;
            if (reward > 500) reward = 50;
        }
        
        return missions;
    }

    private static (string title, int target) GetMissionDetails(MissionType type, int index)
    {
        int tier = (index / 10) + 1;
        int multiplier = tier * 10;
        
        return type switch
        {
            MissionType.CollectCoins => ($"Collect {100 * multiplier} coins", 100 * multiplier),
            MissionType.DodgeObstacles => ($"Dodge {5 * multiplier} obstacles", 5 * multiplier),
            MissionType.UseSkateboard => ($"Use skateboard {2 * multiplier} times", 2 * multiplier),
            MissionType.RunMeters => ($"Run {500 * multiplier} meters", 500 * multiplier),
            MissionType.CollectWords => ($"Collect {3 * multiplier} words", 3 * multiplier),
            MissionType.ScorePoints => ($"Score {1000 * multiplier} points", 1000 * multiplier),
            MissionType.UsePowerup => ($"Use powerup {2 * multiplier} times", 2 * multiplier),
            MissionType.CompleteDaily => ($"Complete {1 * multiplier} daily missions", 1 * multiplier),
            MissionType.InviteFriend => ($"Invite {1 * (multiplier / 10)} friends", 1 * (multiplier / 10)),
            _ => ("Complete mission", 10)
        };
    }
}

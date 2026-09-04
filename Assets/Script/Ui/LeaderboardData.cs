using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class LeaderboardEntryData
{
    public int rank;
    public string playerName;
    public int score;
    public Sprite avatar;
    public bool isMe;
}

[CreateAssetMenu(fileName = "LeaderboardData", menuName = "Game/Leaderboard Data")]
public class LeaderboardData : ScriptableObject
{
    [SerializeField] private List<LeaderboardEntryData> entries = new List<LeaderboardEntryData>();

    public List<LeaderboardEntryData> Entries => entries;

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

    public void SetEntries(List<LeaderboardEntryData> newEntries)
    {
        entries = newEntries;
    }

    public static List<LeaderboardEntryData> GenerateDefaultEntries()
    {
        return new List<LeaderboardEntryData>
        {
            new LeaderboardEntryData { rank = 1, playerName = "Amara Daniel", score = 15840 },
            new LeaderboardEntryData { rank = 2, playerName = "Dapo Abiodun", score = 14200 },
            new LeaderboardEntryData { rank = 3, playerName = "Chidi Nwosu", score = 13550 },
            new LeaderboardEntryData { rank = 4, playerName = "Daniel Nwankwo", score = 12890 },
            new LeaderboardEntryData { rank = 5, playerName = "Samuel Afolabi", score = 12100 },
            new LeaderboardEntryData { rank = 6, playerName = "Nneka Obiora", score = 11500 },
            new LeaderboardEntryData { rank = 7, playerName = "Bola Fashola", score = 10980 },
            new LeaderboardEntryData { rank = 8, playerName = "Obinna Ejike", score = 10450 },
            new LeaderboardEntryData { rank = 9, playerName = "Grace Okonkwo", score = 9900 },
            new LeaderboardEntryData { rank = 10, playerName = "Ifeanyi Okafor", score = 9420 },
            new LeaderboardEntryData { rank = 11, playerName = "Zainab Mohammed", score = 8980 },
            new LeaderboardEntryData { rank = 12, playerName = "Tunde Bakare", score = 8540 },
            new LeaderboardEntryData { rank = 13, playerName = "Yemi Alade", score = 8100 },
            new LeaderboardEntryData { rank = 14, playerName = "Kunle Adeyemi", score = 7650 },
            new LeaderboardEntryData { rank = 15, playerName = "Adaobi Eze", score = 7200 },
            new LeaderboardEntryData { rank = 16, playerName = "Femi Otedola", score = 6780 },
            new LeaderboardEntryData { rank = 17, playerName = "Aisha Bello", score = 6350 },
            new LeaderboardEntryData { rank = 18, playerName = "Emeka Ike", score = 5900 },
            new LeaderboardEntryData { rank = 19, playerName = "Tolani Ajayi", score = 5480 },
            new LeaderboardEntryData { rank = 20, playerName = "Chioma Nnamdi", score = 5050 },
            new LeaderboardEntryData { rank = 21, playerName = "Babatunde Fashola", score = 4620 },
            new LeaderboardEntryData { rank = 22, playerName = "Ngozi Adichie", score = 4180 },
            new LeaderboardEntryData { rank = 23, playerName = "Segun Arinze", score = 3750 },
            new LeaderboardEntryData { rank = 24, playerName = "Funke Akindele", score = 3320 },
            new LeaderboardEntryData { rank = 25, playerName = "Wole Soyinka", score = 2890 },
            new LeaderboardEntryData { rank = 26, playerName = "Omotola Jalade", score = 2450 },
            new LeaderboardEntryData { rank = 27, playerName = "Don Jazzy", score = 2020 },
            new LeaderboardEntryData { rank = 28, playerName = "Tiwa Savage", score = 1580 },
            new LeaderboardEntryData { rank = 29, playerName = "Davido Adeleke", score = 1150 },
            new LeaderboardEntryData { rank = 30, playerName = "Burna Boy", score = 720 },
        };
    }
}

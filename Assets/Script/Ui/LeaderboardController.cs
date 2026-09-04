using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;

public class LeaderboardController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private LeaderboardData leaderboardData; // Drag LeaderboardData asset here

    [Header("Podium Group")]
    [SerializeField] private Transform podiumGroup; // Drag the Podium parent object here
    [SerializeField] private LeaderboardRowItem firstPlaceItem;  // PodiumCard_Gold
    [SerializeField] private LeaderboardRowItem secondPlaceItem; // PodiumCard_Silver
    [SerializeField] private LeaderboardRowItem thirdPlaceItem;  // PodiumCard_Bronze

    [Header("List References")]
    [SerializeField] private Transform listContent;
    [SerializeField] private LeaderboardRowItem rowPrefab;

    [Header("Current User Row")]
    [SerializeField] private LeaderboardRowItem currentUserItem;

    [Header("UGS Settings")]
    [SerializeField] private string ugsLeaderboardId = "wiki_cat_rush";


    [Header("Player Data")]
    [SerializeField] private string playerName = "YOU";

    private List<LeaderboardEntryData> entries = new List<LeaderboardEntryData>();
    private List<LeaderboardRowItem> spawnedRows = new List<LeaderboardRowItem>();

    private void Awake()
    {
        ReparentItemsToScrollContent();
    }

    private void ReparentItemsToScrollContent()
    {
        if (listContent == null) return;

        // Automatically resolve podium card references if they are pointing to prefab assets
        if (podiumGroup != null)
        {
            if (firstPlaceItem != null && firstPlaceItem.gameObject.scene.name == null)
            {
                firstPlaceItem = podiumGroup.Find("PodiumCard_Gold")?.GetComponent<LeaderboardRowItem>();
                Debug.Log($"[LeaderboardController] Resolved firstPlaceItem prefab to scene instance: {firstPlaceItem != null}");
            }
            if (secondPlaceItem != null && secondPlaceItem.gameObject.scene.name == null)
            {
                secondPlaceItem = podiumGroup.Find("PodiumCard_Silver")?.GetComponent<LeaderboardRowItem>();
                Debug.Log($"[LeaderboardController] Resolved secondPlaceItem prefab to scene instance: {secondPlaceItem != null}");
            }
            if (thirdPlaceItem != null && thirdPlaceItem.gameObject.scene.name == null)
            {
                thirdPlaceItem = podiumGroup.Find("PodiumCard_Bronze")?.GetComponent<LeaderboardRowItem>();
                Debug.Log($"[LeaderboardController] Resolved thirdPlaceItem prefab to scene instance: {thirdPlaceItem != null}");
            }
        }

        // Move whole Podium group (Gold+Silver+Bronze) into Content at the top
        if (podiumGroup != null)
        {
            podiumGroup.SetParent(listContent, false);
            podiumGroup.SetAsFirstSibling();
        }

        // Player card starts hidden until rank is known. If it is a prefab asset, instantiate it!
        if (currentUserItem != null)
        {
            if (currentUserItem.gameObject.scene.name == null)
            {
                currentUserItem = Instantiate(currentUserItem, listContent);
                Debug.Log("[LeaderboardController] Instantiated currentUserItem from prefab asset.");
            }
            else
            {
                currentUserItem.transform.SetParent(listContent, false);
            }
        }
    }

    private void OnEnable()
    {
        if (UGSManager.Instance != null)
        {
            UGSManager.Instance.OnSignInComplete += HandleOnSignInComplete;
        }
    }

    private void OnDisable()
    {
        if (UGSManager.Instance != null)
        {
            UGSManager.Instance.OnSignInComplete -= HandleOnSignInComplete;
        }
    }

    private void HandleOnSignInComplete()
    {
        Debug.Log("[LeaderboardController] UGS sign-in complete. Reloading leaderboard...");
        LoadLeaderboard();
    }

    private void Start()
    {
        LoadLeaderboard();
    }

    /// <summary>
    /// Assign this to BackToHome button OnClick() in the Inspector.
    /// </summary>
    public void OnBackPressed()
    {
        Debug.Log("[Leaderboard] Back to Home pressed");
        UImanager homeCtrl = UImanager.uimanager;
        if (homeCtrl != null)
            homeCtrl.HideLeaderboard();
        else
            Hide();
    }

    public async void LoadLeaderboard()
    {
        entries = new List<LeaderboardEntryData>();

        if (UGSManager.Instance != null && UGSManager.Instance.IsSignedIn)
        {
            Debug.Log("[LeaderboardController] Attempting to load from UGS...");
            var ugsEntries = await UGSManager.Instance.GetScoresAsync(ugsLeaderboardId);
            if (ugsEntries != null && ugsEntries.Count > 0)
            {
                entries = ugsEntries;
                Debug.Log("[LeaderboardController] Successfully loaded entries from UGS.");
            }
        }

        RefreshUI();
    }

    /// <summary>
    /// Call this after a game ends to save the player's score and refresh the leaderboard.
    /// </summary>
    public void SavePlayerScore(string name, int score)
    {
        // Save to PlayerPrefs
        PlayerPrefs.SetString("PlayerName", name);
        PlayerPrefs.SetInt("HighScore", score);
        PlayerPrefs.Save();

        if (leaderboardData != null)
        {
            var list = new List<LeaderboardEntryData>(leaderboardData.Entries);

            // Clear any previous isMe entries
            foreach (var e in list) e.isMe = false;

            // Check if player already exists, update score; otherwise add new entry
            var existing = list.FirstOrDefault(e => e.playerName == name);
            if (existing != null)
            {
                existing.score = score;
                existing.isMe = true;
            }
            else
            {
                list.Add(new LeaderboardEntryData { playerName = name, score = score, isMe = true });
            }

            // Re-sort by score descending and re-assign ranks
            list = list.OrderByDescending(e => e.score).ToList();
            for (int i = 0; i < list.Count; i++)
                list[i].rank = i + 1;

            // Keep top 30 only
            if (list.Count > 30)
                list = list.Take(30).ToList();

            leaderboardData.SetEntries(list);
        }

        // Submit to UGS asynchronously if available
        if (UGSManager.Instance != null)
        {
            SubmitScoreToUGSAsync(score);
        }
        else
        {
            RefreshUI();
        }
    }

    private async void SubmitScoreToUGSAsync(int score)
    {
        if (UGSManager.Instance != null)
        {
            await UGSManager.Instance.SubmitScoreAsync(ugsLeaderboardId, score);
            // Refresh to fetch the latest online leaderboard rankings
            LoadLeaderboard();
        }
    }

    private void RefreshUI()
    {
        UpdatePodium();
        UpdateList();
        UpdateCurrentUser();
    }

    private void UpdatePodium()
    {
        var first = entries.FirstOrDefault(e => e.rank == 1);
        var second = entries.FirstOrDefault(e => e.rank == 2);
        var third = entries.FirstOrDefault(e => e.rank == 3);

        if (first != null && firstPlaceItem != null)
            firstPlaceItem.Setup(first.rank, first.playerName, first.score, first.avatar);

        if (second != null && secondPlaceItem != null)
            secondPlaceItem.Setup(second.rank, second.playerName, second.score, second.avatar);

        if (third != null && thirdPlaceItem != null)
            thirdPlaceItem.Setup(third.rank, third.playerName, third.score, third.avatar);
    }

    private void UpdateList()
    {
        if (listContent == null || rowPrefab == null) return;

        // Destroy only previously spawned rows (not podium/player items)
        foreach (var row in spawnedRows)
        {
            if (row != null) Destroy(row.gameObject);
        }
        spawnedRows.Clear();

        // Insert rows after podium (indices 0-2) and before player row
        int insertIndex = 3; // after 1st, 2nd, 3rd

        var rest = entries.Where(e => e.rank >= 4).OrderBy(e => e.rank);
        foreach (var entry in rest)
        {
            LeaderboardRowItem row = Instantiate(rowPrefab, listContent);
            row.transform.SetSiblingIndex(insertIndex);
            insertIndex++;
            row.Setup(entry.rank, entry.playerName, entry.score, entry.avatar);
            spawnedRows.Add(row);
        }

    }

    private void UpdateCurrentUser()
    {
        if (currentUserItem == null) return;

        var myEntry = entries.FirstOrDefault(e => e.isMe);
        int myRank;
        string myName;
        int myScore;
        Sprite myAvatar;

        if (myEntry != null)
        {
            myRank   = myEntry.rank;
            myName   = myEntry.playerName;
            myScore  = myEntry.score;
            myAvatar = myEntry.avatar;
        }
        else
        {
            myName   = PlayerPrefs.GetString("PlayerName", playerName);
            myScore  = PlayerPrefs.GetInt("HighScore", 0);
            myRank   = entries.Count > 0 ? entries.Last().rank + 1 : 1;
            myAvatar = null;
        }

        currentUserItem.Setup(myRank, myName, myScore, myAvatar);

        // Place at correct rank position in the list
        // Index 0 = podiumGroup, rows start at index 1
        // rank 4 → sibling index 1, rank 5 → index 2, etc.
        int siblingIndex = myRank >= 4 ? myRank - 3 : myRank - 1;
        int maxIndex = listContent.childCount - 1;
        siblingIndex = Mathf.Clamp(siblingIndex, 0, maxIndex);
        currentUserItem.transform.SetSiblingIndex(siblingIndex);
    }

    public void Hide()
    {
        UImanager homeCtrl = UImanager.uimanager;
        if (homeCtrl != null)
            homeCtrl.HideLeaderboard();
        else
            gameObject.SetActive(false);
    }
}

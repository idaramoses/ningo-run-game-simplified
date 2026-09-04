using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RunnerSelectionManager : MonoBehaviour
{
    public static RunnerSelectionManager Instance { get; private set; }

    [Header("Runner Database")]
    public List<RunnerData> allRunners = new List<RunnerData>();

    [Header("Current Selection")]
    private int currentRunnerIndex = 0;
    private const string SELECTED_RUNNER_KEY = "SelectedRunner";
    private const string UNLOCKED_RUNNERS_KEY = "UnlockedRunners";

    private static bool s_Migrating = false;

    private void Awake()
    {
        // If we're being created as part of a migration, skip normal Awake logic
        if (s_Migrating) return;

        if (Instance != null && Instance != this)
        {
            // Only destroy this component, not the entire GameObject,
            // so sibling components (e.g. RunnerManager) stay alive in the scene.
            Destroy(this);
            return;
        }

        // If this GameObject has other MonoBehaviours (e.g. RunnerManager),
        // move ourselves to a dedicated GameObject so DontDestroyOnLoad
        // doesn't drag scene-bound components along.
        var siblings = GetComponents<MonoBehaviour>();
        int siblingCount = 0;
        foreach (var s in siblings)
        {
            if (s != this) siblingCount++;
        }

        if (siblingCount > 0)
        {
            s_Migrating = true;
            GameObject persistent = new GameObject("RunnerSelectionManager_Persistent");
            RunnerSelectionManager moved = persistent.AddComponent<RunnerSelectionManager>();
            s_Migrating = false;

            moved.allRunners = new List<RunnerData>(this.allRunners);
            Instance = moved;
            DontDestroyOnLoad(persistent);
            moved.LoadRunnerProgress();
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadRunnerProgress();
    }

    public RunnerData GetCurrentRunner()
    {
#if UNITY_EDITOR
        Debug.Log($"[RunnerSelectionManager] GetCurrentRunner - allRunners.Count: {allRunners.Count}, currentRunnerIndex: {currentRunnerIndex}");
#endif
        if (allRunners.Count == 0)
        {
            Debug.LogWarning("[RunnerSelectionManager] allRunners list is EMPTY! This may happen if the DontDestroyOnLoad object lost its Inspector references.");
            return null;
        }
        if (currentRunnerIndex >= allRunners.Count)
        {
            Debug.LogWarning($"[RunnerSelectionManager] currentRunnerIndex ({currentRunnerIndex}) is out of range! Resetting to 0.");
            currentRunnerIndex = 0;
        }
        return allRunners[currentRunnerIndex];
    }

    public int GetCurrentRunnerIndex()
    {
#if UNITY_EDITOR
        Debug.Log($"[RunnerSelectionManager] GetCurrentRunnerIndex called, returning: {currentRunnerIndex}");
#endif
        return currentRunnerIndex;
    }

    public void SelectRunner(int index)
    {
        if (index >= 0 && index < allRunners.Count)
        {
            currentRunnerIndex = index;
            
            // Update the STATIC variable in RunnerManager - this persists across scene reloads
            RunnerManager.SetSelectedRunnerIndex(index);

            if (allRunners[index] != null && allRunners[index].runnerPrefab != null)
            {
                RunnerManager.SetSelectedRunnerPrefab(allRunners[index].runnerPrefab);
            }
            
#if UNITY_EDITOR
            Debug.Log($"[RunnerSelectionManager] Selected runner index: {index}, name: {allRunners[index].runnerName}");
#endif
            
            // Also update the active runner if RunnerManager exists in scene
            if (RunnerManager.Instance != null)
            {
                RunnerManager.Instance.SetActiveRunner(allRunners[index].runnerName);
            }
        }
    }

    public bool IsRunnerUnlocked(int index)
    {
        if (index >= 0 && index < allRunners.Count)
        {
            RunnerData runner = allRunners[index];
            
            if (runner.isUnlockedByDefault)
                return true;

            string key = $"{UNLOCKED_RUNNERS_KEY}_{index}";
            return PlayerPrefs.GetInt(key, 0) == 1;
        }
        return false;
    }

    public bool UnlockRunner(int index)
    {
        if (index >= 0 && index < allRunners.Count)
        {
            RunnerData runner = allRunners[index];

            if (IsRunnerUnlocked(index))
            {
#if UNITY_EDITOR
                Debug.Log($"[RunnerManager] Runner {runner.runnerName} already unlocked");
#endif
                return true;
            }

            bool canUnlock = false;

            switch (runner.unlockType)
            {
                case UnlockType.Free:
                    canUnlock = true;
                    break;

                case UnlockType.Coins:
                    if (CurrencyManager.Instance != null && CurrencyManager.Instance.GetCoins() >= runner.unlockCost)
                    {
                        CurrencyManager.Instance.SpendCoins(runner.unlockCost);
                        canUnlock = true;
                    }
                    break;

                case UnlockType.Gems:
                    if (CurrencyManager.Instance != null && CurrencyManager.Instance.GetGems() >= runner.unlockCost)
                    {
                        CurrencyManager.Instance.SpendGems(runner.unlockCost);
                        canUnlock = true;
                    }
                    break;

                case UnlockType.Level:
                    // LevelManager removed - always allow unlock by level
                    canUnlock = true;
                    // if (LevelManager.Instance != null)
                    // {
                    //     int highestUnlockedLevel = LevelManager.Instance.GetHighestUnlockedLevel();
                    //     canUnlock = highestUnlockedLevel >= runner.unlockLevel;
                    // }
                    break;

                case UnlockType.RewardedAd:
                    canUnlock = true;
                    break;

                case UnlockType.Special:
                    canUnlock = false;
                    break;
            }

            if (canUnlock)
            {
                string key = $"{UNLOCKED_RUNNERS_KEY}_{index}";
                PlayerPrefs.SetInt(key, 1);
                PlayerPrefs.Save();
#if UNITY_EDITOR
                Debug.Log($"[RunnerManager] Unlocked runner: {runner.runnerName}");
#endif
                return true;
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log($"[RunnerManager] Cannot unlock runner: {runner.runnerName}");
#endif
                return false;
            }
        }
        return false;
    }

    public bool CanAffordRunner(int index)
    {
        if (index >= 0 && index < allRunners.Count)
        {
            RunnerData runner = allRunners[index];

            if (IsRunnerUnlocked(index))
                return true;

            switch (runner.unlockType)
            {
                case UnlockType.Free:
                    return true;

                case UnlockType.Coins:
                    return CurrencyManager.Instance != null && CurrencyManager.Instance.GetCoins() >= runner.unlockCost;

                case UnlockType.Gems:
                    return CurrencyManager.Instance != null && CurrencyManager.Instance.GetGems() >= runner.unlockCost;

                case UnlockType.Level:
                    // LevelManager removed - always allow unlock by level
                    return true;
                    // if (LevelManager.Instance != null)
                    // {
                    //     int highestUnlockedLevel = LevelManager.Instance.GetHighestUnlockedLevel();
                    //     return highestUnlockedLevel >= runner.unlockLevel;
                    // }
                    // return false;

                case UnlockType.RewardedAd:
                    return true;

                case UnlockType.Special:
                    return false;
            }
        }
        return false;
    }

    private void LoadRunnerProgress()
    {
        currentRunnerIndex = PlayerPrefs.GetInt(SELECTED_RUNNER_KEY, 0);
        
#if UNITY_EDITOR
        Debug.Log($"[RunnerSelectionManager] LoadRunnerProgress - Loaded index: {currentRunnerIndex} from key '{SELECTED_RUNNER_KEY}'");
        Debug.Log($"[RunnerSelectionManager] PlayerPrefs HasKey '{SELECTED_RUNNER_KEY}': {PlayerPrefs.HasKey(SELECTED_RUNNER_KEY)}");
#endif

        if (allRunners.Count > 0 && allRunners[0] != null && allRunners[0].isUnlockedByDefault)
        {
            string key = $"{UNLOCKED_RUNNERS_KEY}_0";
            if (PlayerPrefs.GetInt(key, 0) == 0)
            {
                PlayerPrefs.SetInt(key, 1);
                PlayerPrefs.Save();
            }
        }

#if UNITY_EDITOR
        Debug.Log($"[RunnerSelectionManager] Loaded runner progress. Current runner index: {currentRunnerIndex}");
#endif
    }

    public List<RunnerData> GetAllRunners()
    {
        return allRunners;
    }
}

using UnityEngine;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    private Dictionary<int, LevelData> levels = new Dictionary<int, LevelData>();
    private int currentSelectedLevel = 0; // Start with Level 0 (Tutorial)
    private int highestUnlockedLevel = 3;

    private const string HIGHEST_UNLOCKED_KEY = "HighestUnlockedLevel";
    private const string LEVEL_STARS_KEY = "Level_{0}_Stars";
    private const string LEVEL_COMPLETED_KEY = "Level_{0}_Completed";
    private const string LEVEL_BEST_SCORE_KEY = "Level_{0}_BestScore";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAllLevelData();
    }

    public LevelData GetLevel(int levelNumber)
    {
        if (!levels.ContainsKey(levelNumber))
        {
            levels[levelNumber] = CreateLevel(levelNumber);
        }

        return levels[levelNumber];
    }

    private LevelData CreateLevel(int levelNumber)
    {
        LevelData level = new LevelData(levelNumber);
        
        // Level 0 (Tutorial) is always unlocked
        level.isUnlocked = (levelNumber == 0) || (levelNumber <= highestUnlockedLevel);
        level.starsEarned = PlayerPrefs.GetInt(string.Format(LEVEL_STARS_KEY, levelNumber), 0);
        level.isCompleted = PlayerPrefs.GetInt(string.Format(LEVEL_COMPLETED_KEY, levelNumber), 0) == 1;
        level.bestScore = PlayerPrefs.GetInt(string.Format(LEVEL_BEST_SCORE_KEY, levelNumber), 0);

        return level;
    }

    public void SetSelectedLevel(int levelNumber)
    {
        currentSelectedLevel = levelNumber;
        Debug.Log($"[LevelManager] Selected Level {levelNumber} - Words: {GetLevel(levelNumber).GetWordCount()}");
    }

    public int GetSelectedLevel()
    {
        return currentSelectedLevel;
    }

    public LevelData GetCurrentLevel()
    {
        return GetLevel(currentSelectedLevel);
    }

    private bool suppressAutoUnlock;

    public void SetSuppressAutoUnlock(bool suppress)
    {
        suppressAutoUnlock = suppress;
    }

    public void CompleteLevel(int levelNumber, int wordsCollected)
    {
        LevelData level = GetLevel(levelNumber);
        int earnedStars = level.CalculateStars(wordsCollected);

        if (earnedStars > level.starsEarned)
        {
            level.starsEarned = earnedStars;
            level.isCompleted = true;

            SaveLevelData(level);

            if (level.CanUnlockNextLevel() && !suppressAutoUnlock)
            {
                UnlockLevel(levelNumber + 1);
            }

            Debug.Log($"[LevelManager] Level {levelNumber} completed with {earnedStars} stars ({wordsCollected}/{level.GetWordCount()} words)");
        }
    }

    public void UnlockLevel(int levelNumber)
    {
        if (levelNumber > highestUnlockedLevel)
        {
            highestUnlockedLevel = levelNumber;
            PlayerPrefs.SetInt(HIGHEST_UNLOCKED_KEY, highestUnlockedLevel);
            PlayerPrefs.Save();

            LevelData level = GetLevel(levelNumber);
            level.isUnlocked = true;

            Debug.Log($"[LevelManager] Unlocked Level {levelNumber}");
        }
    }

    public bool IsLevelUnlocked(int levelNumber)
    {
        return levelNumber <= highestUnlockedLevel;
    }

    public int GetHighestUnlockedLevel()
    {
        return highestUnlockedLevel;
    }

    private void SaveLevelData(LevelData level)
    {
        PlayerPrefs.SetInt(string.Format(LEVEL_STARS_KEY, level.levelNumber), level.starsEarned);
        PlayerPrefs.SetInt(string.Format(LEVEL_COMPLETED_KEY, level.levelNumber), level.isCompleted ? 1 : 0);
        PlayerPrefs.SetInt(string.Format(LEVEL_BEST_SCORE_KEY, level.levelNumber), level.bestScore);
        PlayerPrefs.Save();
    }

    private void LoadAllLevelData()
    {
        highestUnlockedLevel = PlayerPrefs.GetInt(HIGHEST_UNLOCKED_KEY, 3);
        Debug.Log($"[LevelManager] Loaded progress - Highest unlocked: Level {highestUnlockedLevel}");
    }

    public void ResetAllProgress()
    {
        PlayerPrefs.DeleteKey(HIGHEST_UNLOCKED_KEY);
        
        foreach (var level in levels.Values)
        {
            PlayerPrefs.DeleteKey(string.Format(LEVEL_STARS_KEY, level.levelNumber));
            PlayerPrefs.DeleteKey(string.Format(LEVEL_COMPLETED_KEY, level.levelNumber));
            PlayerPrefs.DeleteKey(string.Format(LEVEL_BEST_SCORE_KEY, level.levelNumber));
        }

        PlayerPrefs.Save();
        levels.Clear();
        highestUnlockedLevel = 3;
        currentSelectedLevel = 1;

        Debug.Log("[LevelManager] All progress reset");
    }

    public LevelDifficultySettings GetCurrentDifficultySettings()
    {
        return LevelDifficultySettings.GetSettingsForLevel(currentSelectedLevel);
    }

    /// <summary>
    /// Calculate the maximum number of levels based on available dictionary words.
    /// Word progression: Levels 1-3 = 1 word, Level 4 = 3, Level 5 = 4, ..., Level 8+ = 7 (capped).
    /// Minimum 50 levels even if dictionary is small.
    /// </summary>
    public int GetMaxLevelCount()
    {
        int totalWords = DictionaryManager.GetTotalWordCount();
        
        if (totalWords == 0)
        {
            return 50; // Default if dictionary not loaded yet
        }

        // Calculate how many levels we can support
        // Levels 1-3: 1 word each = 3 words
        // Level 4: 3 words, Level 5: 4, Level 6: 5, Level 7: 6
        // Level 8+: 7 words each
        int wordsUsed = 0;
        int level = 0;

        while (wordsUsed < totalWords)
        {
            level++;
            int wordsForThisLevel;
            
            if (level <= 3)
                wordsForThisLevel = 1;
            else
                wordsForThisLevel = Mathf.Min(level - 1, 7);

            wordsUsed += wordsForThisLevel;
        }

        // Ensure minimum of 50 levels
        int maxLevel = Mathf.Max(level, 50);
        
        Debug.Log($"[LevelManager] Dictionary has {totalWords} words -> {maxLevel} levels available");
        return maxLevel;
    }
}

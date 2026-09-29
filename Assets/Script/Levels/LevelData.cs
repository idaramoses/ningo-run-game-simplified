using UnityEngine;

[System.Serializable]
public class LevelData
{
    public int levelNumber;
    public int starsEarned;
    public bool isUnlocked;
    public int bestScore;
    public bool isCompleted;

    public LevelData(int levelNum)
    {
        levelNumber = levelNum;
        starsEarned = 0;
        isUnlocked = levelNum <= 3;
        bestScore = 0;
        isCompleted = false;
    }

    public int GetWordCount()
    {
        // Level 0: Tutorial level (no word collection)
        if (levelNumber == 0) return 0;
        
        // Levels 1-3: 1 word each (beginner levels)
        if (levelNumber <= 3) return 1;
        
        // Progressive difficulty with max cap of 7 words
        int wordCount = levelNumber - 1;
        return Mathf.Min(wordCount, 7);
    }

    public int GetDifficultyTier()
    {
        return (levelNumber - 1) / 5;
    }

    public int GetStarThreshold(int starLevel)
    {
        int totalWords = GetWordCount();
        
        switch (starLevel)
        {
            case 1:
                return Mathf.CeilToInt(totalWords * 0.6f);
            case 2:
                return Mathf.CeilToInt(totalWords * 0.85f);
            case 3:
                return totalWords;
            default:
                return 0;
        }
    }

    public int CalculateStars(int wordsCollected)
    {
        int totalWords = GetWordCount();
        
        if (wordsCollected >= totalWords)
            return 3;
        else if (wordsCollected >= GetStarThreshold(2))
            return 2;
        else if (wordsCollected >= GetStarThreshold(1))
            return 1;
        else
            return 0;
    }

    public bool CanUnlockNextLevel()
    {
        return starsEarned >= 2;
    }

    public bool IsTutorialLevel()
    {
        return levelNumber == 0;
    }
}

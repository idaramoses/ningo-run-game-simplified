using UnityEngine;

[System.Serializable]
public class LevelDifficultySettings
{
    public int tier;
    public float speedMultiplier;
    public float obstacleSpawnRate;
    public int minWordLength;
    public int maxWordLength;
    public float wordTimeLimit;

    public static LevelDifficultySettings GetSettingsForTier(int tier)
    {
        LevelDifficultySettings settings = new LevelDifficultySettings();
        settings.tier = tier;

        switch (tier)
        {
            case 0:
                // Levels 1-5: previously 0.3 made obstacles too sparse (1-2 per segment).
                // 1.2 keeps base counts (2-5) and slightly increases them.
                settings.speedMultiplier = 1.0f;
                settings.obstacleSpawnRate = 1.2f;
                settings.minWordLength = 3;
                settings.maxWordLength = 5;
                settings.wordTimeLimit = 15f;
                break;

            case 1:
                settings.speedMultiplier = 1.15f;
                settings.obstacleSpawnRate = 1.4f;
                settings.minWordLength = 4;
                settings.maxWordLength = 7;
                settings.wordTimeLimit = 12f;
                break;

            case 2:
                settings.speedMultiplier = 1.3f;
                settings.obstacleSpawnRate = 0.6f;
                settings.minWordLength = 6;
                settings.maxWordLength = 9;
                settings.wordTimeLimit = 10f;
                break;

            case 3:
                settings.speedMultiplier = 1.5f;
                settings.obstacleSpawnRate = 0.75f;
                settings.minWordLength = 7;
                settings.maxWordLength = 11;
                settings.wordTimeLimit = 8f;
                break;

            default:
                float tierMultiplier = 1f + (tier * 0.15f);
                settings.speedMultiplier = Mathf.Min(tierMultiplier, 2.5f);
                settings.obstacleSpawnRate = Mathf.Min(0.3f + (tier * 0.15f), 0.95f);
                settings.minWordLength = Mathf.Min(3 + tier, 10);
                settings.maxWordLength = Mathf.Min(5 + (tier * 2), 15);
                settings.wordTimeLimit = Mathf.Max(15f - (tier * 1.5f), 5f);
                break;
        }

        return settings;
    }

    public static LevelDifficultySettings GetSettingsForLevel(int levelNumber)
    {
        int tier = (levelNumber - 1) / 5;
        return GetSettingsForTier(tier);
    }
}

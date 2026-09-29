using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WordManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LetterSpawner letterSpawner;

    [Header("UI")]
    [SerializeField] private TMP_Text englishPromptText;
    [SerializeField] private TMP_Text collectedLettersText;

    public enum CollectMode { InOrder, AnyOrder }

    [Header("Collection")]
    [SerializeField] private CollectMode collectMode = CollectMode.AnyOrder;

    [Tooltip("If true: wrong letter instantly fails the run.")]
    [SerializeField] private bool failOnWrongLetter = true;

    [Tooltip("Auto-start the first round once JSON is ready.")]
    [SerializeField] private bool autoStartOnReady = true;

    [Header("Word Filtering")]
    [SerializeField] private string wordCategory = "body"; // Filter words by category
    [SerializeField] private int maxLetters = 6; // Maximum letters per word
    
    [Header("Level System")]
    [SerializeField] private bool useLevelSystem = true;
    private int wordsSpawnedThisLevel = 0;
    private int targetWordsForLevel = 0;
    private int currentWordIndexInLevel = 0;

    // Events (WordUIBinder uses these)
    public event Action OnStartedRound;
    public event Action OnCompletedRound;
    public event Action OnFailedRound;

    // Current picked word from dictionary
    private DictionaryManager.DictionaryWord currentWord;
    public DictionaryManager.DictionaryWord CurrentWord => currentWord;

    // This is the ANSWER we spawn as letters (translation word, WITH tone marks)
    private string spawnWord = "";

    // The answer split into grapheme clusters (base letter + its tone marks as one unit).
    // This is what the game matches against, so tone marks are required (strict).
    private List<string> wordGraphemes = new List<string>();

    // InOrder progress
    private int expectedIndex = 0;

    // AnyOrder progress (keyed by UPPER-case grapheme cluster, e.g. "Á", "F", "Ọ́")
    private readonly Dictionary<string, int> remainingCounts = new Dictionary<string, int>();
    private int remainingTotal = 0;

    // Track collected letters for UI display (position-based, one grapheme per slot)
    private string[] collectedLettersArray;

    private bool startedOnce = false;
    private bool isFailed = false;

    private void Awake()
    {
        if (letterSpawner == null) letterSpawner = FindObjectOfType<LetterSpawner>();
        if (letterSpawner == null) Debug.LogError("[WordManager] LetterSpawner not found in scene.");
    }

    private IEnumerator Start()
    {
        // Wait until dictionary is loaded
        yield return new WaitUntil(() =>
            letterSpawner != null &&
            DictionaryManager.HasDictionary()
        );

        if (autoStartOnReady)
            StartRound();
    }

    /// <summary>
    /// Resets WordManager so it can start fresh for a new level.
    /// Call this before StartRound() when switching levels without a scene reload.
    /// </summary>
    public void ResetForNewLevel()
    {
        startedOnce = false;
        isFailed = false;
        wordsSpawnedThisLevel = 0;
        targetWordsForLevel = 0;
        currentWordIndexInLevel = 0;
        expectedIndex = 0;
        remainingCounts.Clear();
        remainingTotal = 0;
        spawnWord = "";
        wordGraphemes.Clear();
        currentWord = null;
        collectedLettersArray = null;

        // Clear any spawned letters from the previous level
        if (letterSpawner != null)
        {
            letterSpawner.ClearAllLetters();
        }

        // Clear UI
        if (englishPromptText != null) englishPromptText.text = "";
        if (collectedLettersText != null) collectedLettersText.text = "";

        Debug.Log("[WordManager] Reset for new level");
    }

    /// <summary>
    /// Call from PlayButtonHandler (or let autoStartOnReady handle it).
    /// Safe to call multiple times.
    /// </summary>
    public void StartRound()
    {
        if (startedOnce) return;
        startedOnce = true;

        isFailed = false;
        
        if (useLevelSystem && LevelManager.Instance != null)
        {
            LevelData currentLevel = LevelManager.Instance.GetCurrentLevel();
            targetWordsForLevel = currentLevel.GetWordCount();
            wordsSpawnedThisLevel = 0;
            currentWordIndexInLevel = 0;
            Debug.Log($"[WordManager] Starting Level {currentLevel.levelNumber} - Target: {targetWordsForLevel} words");
        }

        SpawnNextWord();
        OnStartedRound?.Invoke();
    }

    public void SpawnNextWord()
    {
        if (isFailed) return;

        // Ensure dictionary is loaded
        if (!DictionaryManager.HasDictionary())
        {
            Debug.LogError("[WordManager] SpawnNextWord: dictionary not loaded yet.");
            return;
        }

        if (letterSpawner == null)
        {
            Debug.LogError("[WordManager] SpawnNextWord: letterSpawner is NULL.");
            return;
        }

        // Use level-based word selection from dictionary
        if (useLevelSystem && LevelManager.Instance != null)
        {
            int levelNumber = LevelManager.Instance.GetCurrentLevel().levelNumber;
            currentWord = PickWordForLevel(levelNumber, currentWordIndexInLevel);
            
            if (currentWord != null)
            {
                spawnWord = (currentWord.translation ?? "").Trim();
                currentWordIndexInLevel++;
                
                Debug.Log($"[WordManager] Level {levelNumber} word {currentWordIndexInLevel}: {currentWord.word} -> {spawnWord}");
            }
            else
            {
                Debug.LogError($"[WordManager] No word available for level {levelNumber} at index {currentWordIndexInLevel}");
                return;
            }
        }
        else
        {
            // Fallback to random word picking if level system is disabled
            currentWord = PickRandomWord();
            if (currentWord == null)
            {
                Debug.LogError("[WordManager] SpawnNextWord: No words available in dictionary.");
                return;
            }
            spawnWord = (currentWord.translation ?? "").Trim();
            
            Debug.Log($"[WordManager] Picked random: {currentWord.word} = {currentWord.translation}");
        }

        if (string.IsNullOrWhiteSpace(spawnWord))
        {
            Debug.LogError("[WordManager] SpawnNextWord: picked translation is empty.");
            return;
        }

        // Split the word into grapheme clusters so tone marks stay attached to their
        // base letters (e.g. "áfọ́" -> ["á", "f", "ọ́"]). These are the units we match.
        wordGraphemes = TextUtils.SplitGraphemes(spawnWord);

#if UNITY_EDITOR
        System.Text.StringBuilder gb = new System.Text.StringBuilder("Graphemes: ");
        foreach (string g in wordGraphemes)
        {
            gb.Append($"'{g}' ");
        }
        Debug.Log($"[WordManager] '{spawnWord}' -> {wordGraphemes.Count} letters | {gb}");
#endif

        // Reset progress trackers
        expectedIndex = 0;
        BuildRemainingCounts(wordGraphemes);

        // Initialize collected letters array with underscores (one slot per grapheme)
        collectedLettersArray = new string[wordGraphemes.Count];
        for (int i = 0; i < wordGraphemes.Count; i++)
        {
            collectedLettersArray[i] = string.IsNullOrWhiteSpace(wordGraphemes[i]) ? " " : "_";
        }

        // Update UI with new English prompt
        UpdateEnglishPromptUI();
        UpdateCollectedLettersUI();

        // Spawn letters far ahead. Pass the toned word; LetterSpawner splits it into
        // the same grapheme clusters so spawned letters carry their tone marks.
        letterSpawner.SpawnWord(spawnWord);
    }

    private void BuildRemainingCounts(List<string> graphemes)
    {
        remainingCounts.Clear();
        remainingTotal = 0;

        foreach (string g in graphemes)
        {
            if (string.IsNullOrWhiteSpace(g)) continue;

            string key = g.ToUpperInvariant();

            if (!remainingCounts.ContainsKey(key))
                remainingCounts[key] = 0;

            remainingCounts[key]++;
            remainingTotal++;
        }
    }

    /// <summary>
    /// Called by PlayerCollector when player hits/picks a letter.
    /// </summary>
    public void CollectLetter(string collected)
    {
        if (isFailed) return;
        if (string.IsNullOrEmpty(collected)) return;
        if (wordGraphemes == null || wordGraphemes.Count == 0) return;

        // Notify spawner that letter was collected (for respawn tracking)
        if (letterSpawner != null)
        {
            letterSpawner.OnLetterCollected(collected);
        }

        string got = collected.ToUpperInvariant();

        if (collectMode == CollectMode.InOrder)
        {
            // Skip spaces
            while (expectedIndex < wordGraphemes.Count && string.IsNullOrWhiteSpace(wordGraphemes[expectedIndex]))
                expectedIndex++;

            if (expectedIndex >= wordGraphemes.Count) return;

            string expected = wordGraphemes[expectedIndex].ToUpperInvariant();

            if (got == expected)
            {
                // Fill in the letter at the correct position
                collectedLettersArray[expectedIndex] = collected;
                UpdateCollectedLettersUI();

                expectedIndex++;

                while (expectedIndex < wordGraphemes.Count && string.IsNullOrWhiteSpace(wordGraphemes[expectedIndex]))
                    expectedIndex++;

                if (expectedIndex >= wordGraphemes.Count)
                {
                    OnCompletedRound?.Invoke();
                    OnWordCompleted();
                }
            }
            else
            {
                if (failOnWrongLetter)
                    FailRound();
            }

            return;
        }

        // AnyOrder (best for runner)
        if (remainingCounts.TryGetValue(got, out int count) && count > 0)
        {
            // Find first unfilled position for this letter
            for (int i = 0; i < wordGraphemes.Count; i++)
            {
                if (wordGraphemes[i].ToUpperInvariant() == got && collectedLettersArray[i] == "_")
                {
                    collectedLettersArray[i] = collected;
                    break;
                }
            }
            UpdateCollectedLettersUI();

            remainingCounts[got] = count - 1;
            remainingTotal--;

            if (remainingTotal <= 0)
            {
                OnCompletedRound?.Invoke();
                OnWordCompleted();
            }
        }
        else
        {
            if (failOnWrongLetter)
                FailRound();
        }
    }

    private void OnWordCompleted()
    {
        if (useLevelSystem && LevelManager.Instance != null)
        {
            wordsSpawnedThisLevel++;
            
            Debug.Log($"[WordManager] Word completed: {wordsSpawnedThisLevel}/{targetWordsForLevel}");
            
            if (wordsSpawnedThisLevel < targetWordsForLevel)
            {
                Debug.Log($"[WordManager] Spawning next word ({wordsSpawnedThisLevel + 1}/{targetWordsForLevel})");
                SpawnNextWord();
            }
            else
            {
                Debug.Log($"[WordManager] Level complete! All {targetWordsForLevel} words collected");
                CompleteLevelNow();
            }
        }
        else
        {
            SpawnNextWord();
        }
    }
    
    private void CompleteLevelNow()
    {
        if (LevelManager.Instance == null) return;
        
        LevelData currentLevel = LevelManager.Instance.GetCurrentLevel();
        int stars = currentLevel.CalculateStars(wordsSpawnedThisLevel);
        
        // Save level progress
        LevelManager.Instance.CompleteLevel(currentLevel.levelNumber, wordsSpawnedThisLevel);
        
        Debug.Log($"[WordManager] Level {currentLevel.levelNumber} completed with {stars} stars!");
        
        // Show completion panel through GameSceneController (primary) or GameFlowController (fallback)
        if (GameSceneController.Instance != null)
        {
            GameSceneController.Instance.ShowLevelCompletionWithStars(currentLevel.levelNumber, wordsSpawnedThisLevel, targetWordsForLevel, stars);
        }
        else if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.ShowLevelCompletionWithStars(currentLevel.levelNumber, wordsSpawnedThisLevel, targetWordsForLevel, stars);
        }
        else
        {
            Debug.LogError("[WordManager] No GameSceneController or GameFlowController found!");
        }
    }

    public void FailRound()
    {
        if (isFailed) return;
        isFailed = true;

        OnFailedRound?.Invoke();
    }

    /// <summary>
    /// Clears the failed state so the player can continue collecting letters after
    /// spending coins on the ContinuePanel. Does NOT reset word progress.
    /// </summary>
    public void ResetFailState()
    {
        isFailed = false;
        Debug.Log("[WordManager] Fail state cleared - player can collect letters again");
    }

    // Level-based word selection
    private DictionaryManager.DictionaryWord PickWordForLevel(int levelNumber, int wordIndex)
    {
        // Get all words from dictionary
        List<DictionaryManager.DictionaryWord> allWords = DictionaryManager.GetAllWords();
        if (allWords.Count == 0) return null;
        
        // Filter words based on level
        List<DictionaryManager.DictionaryWord> validWords = new List<DictionaryManager.DictionaryWord>();
        
        // Levels 1-3: Only 3-4 letter words
        if (levelNumber <= 3)
        {
            foreach (var word in allWords)
            {
                string translation = (word.translation ?? "").Trim();
                int letterCount = CountLetters(translation);
                if (letterCount >= 3 && letterCount <= 4 && !string.IsNullOrWhiteSpace(translation))
                {
                    validWords.Add(word);
                }
            }
        }
        else
        {
            // Higher levels: Use words up to maxLetters
            foreach (var word in allWords)
            {
                string translation = (word.translation ?? "").Trim();
                if (CountLetters(translation) <= maxLetters && !string.IsNullOrWhiteSpace(translation))
                {
                    validWords.Add(word);
                }
            }
        }
        
        if (validWords.Count == 0)
        {
            Debug.LogWarning($"[WordManager] No valid words for level {levelNumber}, using all words");
            validWords = allWords;
        }
        
        // Use deterministic selection based on level number and word index
        // This ensures same words appear for the same level each time
        int seed = (levelNumber * 1000) + wordIndex;
        System.Random random = new System.Random(seed);
        int index = random.Next(validWords.Count);
        
        return validWords[index];
    }

    // UI helpers
    private DictionaryManager.DictionaryWord PickRandomWord()
    {
        // Filter by category if specified
        List<DictionaryManager.DictionaryWord> filteredWords;
        
        if (!string.IsNullOrEmpty(wordCategory))
        {
            filteredWords = DictionaryManager.GetWordsByCategory(wordCategory);
            
            // If no words in this category, fall back to all words
            if (filteredWords.Count == 0)
            {
                Debug.LogWarning($"[WordManager] No words found in category '{wordCategory}', using all words");
                filteredWords = DictionaryManager.GetAllWords();
            }
        }
        else
        {
            filteredWords = DictionaryManager.GetAllWords();
        }
        
        if (filteredWords.Count == 0) return null;
        
        // Filter by max letters
        List<DictionaryManager.DictionaryWord> validWords = new List<DictionaryManager.DictionaryWord>();
        foreach (var word in filteredWords)
        {
            string translation = (word.translation ?? "").Trim();
            if (CountLetters(translation) <= maxLetters && !string.IsNullOrWhiteSpace(translation))
            {
                validWords.Add(word);
            }
        }
        
        // If no valid words after filtering, use original filtered list
        if (validWords.Count == 0)
        {
            Debug.LogWarning($"[WordManager] No words found with max {maxLetters} letters, using unfiltered list");
            validWords = filteredWords;
        }
        
        int randomIndex = UnityEngine.Random.Range(0, validWords.Count);
        return validWords[randomIndex];
    }

    public string GetPromptText()
    {
        // "Translate: HELLO" (English prompt)
        if (currentWord == null) return "Translate: -";
        return $"Translate: {currentWord.word}";
    }

    public string GetPickedTranslationWord()
    {
        // The actual spawned/answer word (with tone marks)
        return spawnWord;
    }

    private void UpdateEnglishPromptUI()
    {
        if (englishPromptText != null)
        {
            englishPromptText.text = GetPromptText();
        }
    }

    private void UpdateCollectedLettersUI()
    {
        if (collectedLettersText != null && collectedLettersArray != null)
        {
            // Show letters with spaces between for readability
            string display = "";
            for (int i = 0; i < collectedLettersArray.Length; i++)
            {
                if (i > 0 && !string.IsNullOrWhiteSpace(collectedLettersArray[i - 1]))
                {
                    display += " ";
                }
                display += collectedLettersArray[i];
            }

            collectedLettersText.text = display;
        }
    }

    public string GetCollectedLetters()
    {
        return collectedLettersArray != null ? string.Concat(collectedLettersArray) : "";
    }

    /// <summary>
    /// Counts visible letters (grapheme clusters), so toned words like "áfọ́" count as 3,
    /// not 4. Used for level word-length filtering.
    /// </summary>
    private int CountLetters(string text)
    {
        return TextUtils.CountGraphemes(text);
    }
}

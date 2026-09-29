using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelObjectiveTracker : MonoBehaviour
{
    public static LevelObjectiveTracker Instance { get; private set; }

    [Header("UI References")]
    public Text objectiveText;
    public GameObject objectivePanel;

    private LevelData currentLevel;
    private int wordsCollected = 0;
    private int totalWords = 0;
    private bool levelCompleted = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        if (objectivePanel != null)
            objectivePanel.SetActive(false);
    }

    public void StartTracking()
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogError("[LevelObjectiveTracker] LevelManager not found!");
            return;
        }

        currentLevel = LevelManager.Instance.GetCurrentLevel();
        totalWords = currentLevel.GetWordCount();
        wordsCollected = 0;
        levelCompleted = false;

        if (objectivePanel != null)
            objectivePanel.SetActive(true);

        UpdateObjectiveUI();

        Debug.Log($"[LevelObjectiveTracker] Started tracking Level {currentLevel.levelNumber} - Target: {totalWords} words");
    }

    public void OnWordCollected()
    {
        if (levelCompleted)
        {
            Debug.LogWarning($"[LevelObjectiveTracker] OnWordCollected called but level already completed!");
            return;
        }

        wordsCollected++;
        UpdateObjectiveUI();

        Debug.Log($"[LevelObjectiveTracker] Word collected: {wordsCollected}/{totalWords}");

        if (wordsCollected >= totalWords)
        {
            Debug.Log($"[LevelObjectiveTracker] All words collected! Calling CompleteLevel()");
            CompleteLevel();
        }
        else
        {
            Debug.Log($"[LevelObjectiveTracker] Still need {totalWords - wordsCollected} more words");
        }
    }

    private void UpdateObjectiveUI()
    {
        if (objectiveText != null)
        {
            objectiveText.text = $"Words: {wordsCollected}/{totalWords}";
        }
    }

    private void CompleteLevel()
    {
        levelCompleted = true;

        int stars = currentLevel.CalculateStars(wordsCollected);
        
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.CompleteLevel(currentLevel.levelNumber, wordsCollected);
        }

        Debug.Log($"[LevelObjectiveTracker] Level {currentLevel.levelNumber} completed! Stars: {stars}");

        StartCoroutine(ShowLevelComplete(stars));
    }

    private IEnumerator ShowLevelComplete(int stars)
    {
        Debug.Log($"[LevelObjectiveTracker] ShowLevelComplete coroutine started - waiting 1 second");
        yield return new WaitForSeconds(1f);

        // Ad system disabled in simplified build
        Debug.Log("[LevelObjectiveTracker] Skipping ad flow - ad system disabled");

        Debug.Log($"[LevelObjectiveTracker] Pausing game and hiding gameplay elements");
        
        // Pause the game and show runner like pause does
        if (GameSceneController.Instance != null)
        {
            if (GameSceneController.Instance.canvasGame) 
                GameSceneController.Instance.canvasGame.SetActive(false);
                
            if (GameSceneController.Instance.mainRunnerModel) 
                GameSceneController.Instance.mainRunnerModel.SetActive(false);
                
            Time.timeScale = 0f;
            
            if (GameStateController.Instance != null)
                GameStateController.Instance.SetPlaying(false);
        }
        else
        {
            Debug.LogWarning($"[LevelObjectiveTracker] GameSceneController.Instance is NULL, trying fallback");
            Time.timeScale = 0f;
            if (GameStateController.Instance != null)
                GameStateController.Instance.SetPlaying(false);
        }

        if (LevelCompletionPanel.Instance != null)
        {
            Debug.Log($"[LevelObjectiveTracker] Calling LevelCompletionPanel.ShowCompletion with {stars} stars");
            LevelCompletionPanel.Instance.ShowCompletion(currentLevel.levelNumber, wordsCollected, totalWords, stars);
        }
        else
        {
            Debug.LogError($"[LevelObjectiveTracker] LevelCompletionPanel.Instance is NULL! Cannot show completion panel.");
        }
        
        Debug.Log($"[LevelObjectiveTracker] ShowLevelComplete coroutine finished");
    }

    public int GetWordsCollected()
    {
        return wordsCollected;
    }

    public int GetTotalWords()
    {
        return totalWords;
    }

    public bool IsLevelCompleted()
    {
        return levelCompleted;
    }

    public void StopTracking()
    {
        if (objectivePanel != null)
            objectivePanel.SetActive(false);

        levelCompleted = false;
    }
}

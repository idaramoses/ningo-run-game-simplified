using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LevelCompletionPanel : MonoBehaviour
{
    public static LevelCompletionPanel Instance { get; private set; }

    [Header("UI References")]
    public TMP_Text levelNumberText;
    public TMP_Text wordsCollectedText;
    public TMP_Text starsEarnedText;

    [Header("Star Prefabs")]
    public GameObject star1;
    public GameObject star2;
    public GameObject star3;

    [Header("Buttons")]
    public Button nextLevelButton;
    public Button retryButton;
    public Button homeButton;

    [Header("Animation")]
    public float starRevealDelay = 0.5f;

    [Header("Tutorial Mode")]
    public GameObject starsContainer;
    public TMP_Text tutorialMessageText;
    [Header("Tutorial Styling")]
    public float tutorialHintFontSize = 48f;
    public bool placeTutorialHintAboveButton = true;

    private System.Action _tutorialCallback;
    private bool _tutorialModeActive = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Debug.Log($"[LevelCompletionPanel] Instance created on GameObject: {gameObject.name}");
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        // Add button listeners
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(OnNextLevel);

        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetry);

        if (homeButton != null)
            homeButton.onClick.AddListener(OnHome);
    }

    public void ShowCompletion(int levelNumber, int wordsCollected, int totalWords, int stars)
    {
        // Any time we show a normal completion, ensure tutorial mode is fully reset
        _tutorialModeActive = false;
        _tutorialCallback = null;

        Debug.Log($"[LevelCompletionPanel] ShowCompletion called - Level {levelNumber}, Words {wordsCollected}/{totalWords}, Stars {stars}");
        
        // Ad system disabled in simplified build
        
        if (levelNumberText != null)
        {
            levelNumberText.text = $"Level {levelNumber} Complete!";
        }

        if (wordsCollectedText != null)
        {
            wordsCollectedText.text = $"Words: {wordsCollected}/{totalWords}";
        }

        StartCoroutine(RevealStars(stars));

        if (nextLevelButton != null)
        {
            bool canUnlock = stars >= 2;
            nextLevelButton.interactable = canUnlock;
        }
        
        Debug.Log($"[LevelCompletionPanel] Panel updated successfully");
    }

    private IEnumerator RevealStars(int stars)
    {
        // Hide all stars initially
        if (star1 != null) star1.SetActive(false);
        if (star2 != null) star2.SetActive(false);
        if (star3 != null) star3.SetActive(false);

        yield return new WaitForSecondsRealtime(starRevealDelay);

        // Reveal stars based on count
        if (stars >= 1 && star1 != null)
        {
            star1.SetActive(true);
            yield return new WaitForSecondsRealtime(starRevealDelay);
        }

        if (stars >= 2 && star2 != null)
        {
            star2.SetActive(true);
            yield return new WaitForSecondsRealtime(starRevealDelay);
        }

        if (stars >= 3 && star3 != null)
        {
            star3.SetActive(true);
        }

        if (starsEarnedText != null)
            starsEarnedText.text = $"{stars}/3 Stars";
    }

    public void ShowTutorialComplete(System.Action onStart = null)
    {
        Debug.Log("[LevelCompletionPanel] ShowTutorialComplete called");
        
        _tutorialCallback = onStart;
        _tutorialModeActive = true;
        
        // Show tutorial-specific content
        if (levelNumberText != null)
            levelNumberText.text = "Tutorial Complete!";
        
        if (tutorialMessageText != null)
        {
            tutorialMessageText.text = "You're ready to start your journey!\n\nCollect letters to spell Yoruba words and unlock new levels.";
            tutorialMessageText.gameObject.SetActive(true);
            if (tutorialHintFontSize > 0f)
            {
                tutorialMessageText.fontSize = tutorialHintFontSize;
            }
            if (placeTutorialHintAboveButton && nextLevelButton != null)
            {
                var hintTransform = tutorialMessageText.rectTransform;
                var buttonTransform = nextLevelButton.GetComponent<RectTransform>();
                if (hintTransform != null && buttonTransform != null)
                {
                    int buttonIndex = buttonTransform.GetSiblingIndex();
                    hintTransform.SetSiblingIndex(buttonIndex);
                }
            }
        }
        
        // Hide level-specific elements
        if (wordsCollectedText != null)
            wordsCollectedText.gameObject.SetActive(false);
        
        if (starsEarnedText != null)
            starsEarnedText.gameObject.SetActive(false);
        
        if (starsContainer != null)
            starsContainer.SetActive(false);
        
        // Configure buttons for tutorial mode
        if (nextLevelButton != null)
        {
            nextLevelButton.gameObject.SetActive(true);
            nextLevelButton.interactable = true;
            // Clear ALL existing listeners (including any stale Restart bindings)
            nextLevelButton.onClick.RemoveAllListeners();
            nextLevelButton.onClick.AddListener(OnNextLevel);
            Debug.Log("[LevelCompletionPanel] Cleared stale listeners and re-bound OnNextLevel");
            var buttonText = nextLevelButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
                buttonText.text = "Continue";
        }
        
        if (retryButton != null)
            retryButton.gameObject.SetActive(false);
        
        if (homeButton != null)
            homeButton.gameObject.SetActive(false);
        
        // Show panel
        gameObject.SetActive(true);
        Time.timeScale = 0f;
        
        Debug.Log("[LevelCompletionPanel] Tutorial completion panel shown");
    }

    public void HidePanel()
    {
        // Ad system disabled in simplified build
        
        gameObject.SetActive(false);
    }

    private void OnNextLevel()
    {
        // Check if this is tutorial mode
        if (_tutorialModeActive)
        {
            if (_tutorialCallback == null)
            {
                Debug.Log("[LevelCompletionPanel] Tutorial callback already handled - ignoring extra click");
                return;
            }

            Debug.Log("[LevelCompletionPanel] Tutorial mode - invoking callback");
            
            // Mark tutorial as done
            PlayerPrefs.SetInt("tutorial_v1_done", 1);
            PlayerPrefs.Save();
            
            gameObject.SetActive(false);
            Time.timeScale = 1f;
            
            // Reset tutorial-specific UI
            if (tutorialMessageText != null)
                tutorialMessageText.gameObject.SetActive(false);
            
            if (starsContainer != null)
                starsContainer.SetActive(true);
            
            if (wordsCollectedText != null)
                wordsCollectedText.gameObject.SetActive(true);
            
            if (starsEarnedText != null)
                starsEarnedText.gameObject.SetActive(true);
            
            if (retryButton != null)
                retryButton.gameObject.SetActive(true);
            
            if (homeButton != null)
                homeButton.gameObject.SetActive(true);
            
            var buttonText = nextLevelButton?.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
                buttonText.text = "Next Level";
            
            _tutorialCallback?.Invoke();
            _tutorialCallback = null;
            return;
        }
        
        // Normal level completion flow
        if (LevelManager.Instance != null)
        {
            int currentLevel = LevelManager.Instance.GetSelectedLevel();
            LevelManager.Instance.SetSelectedLevel(currentLevel + 1);
        }

        if (GameSceneController.Instance != null)
            GameSceneController.Instance.Restart();
        else if (GameFlowController.Instance != null)
            GameFlowController.Instance.Restart();
    }

    private void OnRetry()
    {
        if (GameSceneController.Instance != null)
            GameSceneController.Instance.Restart();
        else if (GameFlowController.Instance != null)
            GameFlowController.Instance.Restart();
    }

    private void OnHome()
    {
        gameObject.SetActive(false);

        if (GameSceneController.Instance != null)
            GameSceneController.Instance.Home();
        else if (GameFlowController.Instance != null)
            GameFlowController.Instance.Home();
    }
}

using UnityEngine;
using TMPro;

public class WordsCollectedUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text wordsCollectedText;
    
    private int wordsCollected = 0;

    private void Start()
    {
        UpdateDisplay();
        
        // Subscribe to WordManager events
        WordManager wordManager = FindObjectOfType<WordManager>();
        if (wordManager != null)
        {
            wordManager.OnCompletedRound += OnWordCompleted;
        }
        else
        {
            Debug.LogWarning("[WordsCollectedUI] WordManager not found in scene!");
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        WordManager wordManager = FindObjectOfType<WordManager>();
        if (wordManager != null)
        {
            wordManager.OnCompletedRound -= OnWordCompleted;
        }
    }

    private void OnWordCompleted()
    {
        wordsCollected++;
        UpdateDisplay();
        Debug.Log($"[WordsCollectedUI] Words collected: {wordsCollected}");
    }

    private void UpdateDisplay()
    {
        if (wordsCollectedText != null)
        {
            wordsCollectedText.text = wordsCollected.ToString();
        }
    }

    public void ResetCounter()
    {
        wordsCollected = 0;
        UpdateDisplay();
    }

    public int GetWordsCollected()
    {
        return wordsCollected;
    }
}

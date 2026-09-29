using UnityEngine;

public class WordPicker : MonoBehaviour
{
    private DictionaryManager.DictionaryWord currentWord;

    public void PickRandom()
    {
        var allWords = DictionaryManager.GetAllWords();
        if (allWords != null && allWords.Count > 0)
        {
            currentWord = allWords[Random.Range(0, allWords.Count)];
        }
        else
        {
            currentWord = null;
        }
    }

    public string GetPromptText()
    {
        if (currentWord == null) return "Translate: -";
        return $"Translate: {currentWord.word}";
    }

    public string GetPickedWord()
    {
        if (currentWord == null) return "";
        return currentWord.translation ?? "";
    }
}

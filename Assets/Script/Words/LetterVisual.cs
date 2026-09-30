using UnityEngine;
using TMPro;

public class LetterVisual : MonoBehaviour
{
    public TMP_Text tmp;
    public Color correctColor = Color.white;
    public Color wrongColor = Color.red;
    
    private LetterPickup pickup;
    private bool isCorrect = true;

    void Awake()
    {
        pickup = GetComponent<LetterPickup>();

        // Bounce/bob animation (self-attach so the prefab doesn't need it pre-added)
        if (GetComponent<LetterBounce>() == null)
            gameObject.AddComponent<LetterBounce>();

        // Auto-find TMP_Text if not assigned
        if (tmp == null)
        {
            tmp = GetComponentInChildren<TMP_Text>();
            if (tmp == null)
            {
                Debug.LogError($"[LetterVisual] TMP_Text not found on {gameObject.name}. Please assign it in Inspector or add Text (TMP) as child.");
            }
        }
    }

    public void SetLetter(string c)
    {
        if (string.IsNullOrEmpty(c)) c = "?";

        if (tmp != null) tmp.text = c;
        if (pickup != null) pickup.SetLetter(c);

        // Use uppercase for consistency in GameObject names (keeps tone marks attached)
        string displayChar = c.ToUpperInvariant();
        name = $"Letter_{displayChar}";

#if UNITY_EDITOR
        // Debug log to verify letter assignment
        Debug.Log($"[LetterVisual] Set letter '{c}' on {name}");
#endif
    }

    public void SetCorrect(bool correct)
    {
        isCorrect = correct;
        if (pickup != null) pickup.SetCorrect(correct);
        
        // Keep same color for both correct and wrong letters
        // This tests the user's translation knowledge without visual hints
        if (tmp != null)
        {
            tmp.color = correctColor;
        }
    }

    public bool IsCorrect()
    {
        return isCorrect;
    }
}

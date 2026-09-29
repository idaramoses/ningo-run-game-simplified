using UnityEngine;

public class LetterPickup : MonoBehaviour
{
    public string letter = "A";
    private bool isCorrect = true;

    public void SetLetter(string c)
    {
        letter = c;
    }

    public void SetCorrect(bool correct)
    {
        isCorrect = correct;
    }

    public bool IsCorrect()
    {
        return isCorrect;
    }
}

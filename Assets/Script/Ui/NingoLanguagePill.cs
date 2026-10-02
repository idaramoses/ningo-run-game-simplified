using UnityEngine;
using TMPro;

/// <summary>
/// Displays the currently selected learning language ("user_selected_language"
/// PlayerPrefs key) on the settings pill. Refreshes every time the panel opens.
/// </summary>
public class NingoLanguagePill : MonoBehaviour
{
    public TMP_Text text;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (text == null) return;
        string lang = PlayerPrefs.GetString("user_selected_language", "");
        if (string.IsNullOrEmpty(lang)) lang = "Yoruba";
        text.text = char.ToUpperInvariant(lang[0]) + lang.Substring(1).ToLowerInvariant();
    }
}

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Minimal audio toggle for panels without dedicated on/off sprites.
/// Flips the SoundEnabled/MusicEnabled PlayerPrefs key, applies it live,
/// and tints the button image to show the state.
/// </summary>
[RequireComponent(typeof(Button))]
public class SimpleAudioToggle : MonoBehaviour
{
    public enum Kind { Sound, Music }

    public Kind kind;
    public Color offTint = new Color(1f, 1f, 1f, 0.35f);

    private Button btn;
    private Image img;

    private void Awake()
    {
        btn = GetComponent<Button>();
        img = GetComponent<Image>();
        btn.onClick.AddListener(Toggle);
    }

    private void OnEnable()
    {
        RefreshTint();
    }

    private void Toggle()
    {
        string key = kind == Kind.Sound ? "SoundEnabled" : "MusicEnabled";
        bool on = PlayerPrefs.GetInt(key, 1) == 1;
        on = !on;
        PlayerPrefs.SetInt(key, on ? 1 : 0);
        PlayerPrefs.Save();

        if (kind == Kind.Sound)
        {
            if (SoundEffectsManager.Instance != null)
                SoundEffectsManager.Instance.SetSoundEnabled(on);
        }
        else
        {
            if (BackgroundMusicManager.Instance != null)
            {
                if (on) BackgroundMusicManager.Instance.Unmute();
                else BackgroundMusicManager.Instance.Mute();
            }
        }

        if (SoundEffectsManager.Instance != null)
            SoundEffectsManager.Instance.PlayButtonClick();

        RefreshTint();
    }

    private void RefreshTint()
    {
        if (img == null) return;
        string key = kind == Kind.Sound ? "SoundEnabled" : "MusicEnabled";
        bool on = PlayerPrefs.GetInt(key, 1) == 1;
        img.color = on ? Color.white : offTint;
    }
}

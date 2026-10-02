using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pill-style on/off toggle for the settings screen. Swaps between the
/// toggle_on / toggle_off sprites and persists state in PlayerPrefs.
/// Sound  -> "SoundEnabled"  + SoundEffectsManager
/// Music  -> "MusicEnabled"  + BackgroundMusicManager
/// Vibration -> "VibrationEnabled" (pref only; gameplay reads it)
/// </summary>
[RequireComponent(typeof(Button))]
public class NingoPillToggle : MonoBehaviour
{
    public enum Kind { Sound, Music, Vibration }

    public Kind kind;
    public Sprite onSprite;
    public Sprite offSprite;

    private Button btn;
    private Image img;

    private string PrefKey
    {
        get
        {
            switch (kind)
            {
                case Kind.Sound: return "SoundEnabled";
                case Kind.Music: return "MusicEnabled";
                default: return "VibrationEnabled";
            }
        }
    }

    private void Awake()
    {
        btn = GetComponent<Button>();
        img = GetComponent<Image>();
        btn.onClick.AddListener(Toggle);
    }

    private void OnEnable()
    {
        RefreshSprite();
    }

    public bool IsOn => PlayerPrefs.GetInt(PrefKey, 1) == 1;

    private void Toggle()
    {
        bool on = !IsOn;
        PlayerPrefs.SetInt(PrefKey, on ? 1 : 0);
        PlayerPrefs.Save();

        switch (kind)
        {
            case Kind.Sound:
                if (SoundEffectsManager.Instance != null)
                    SoundEffectsManager.Instance.SetSoundEnabled(on);
                break;
            case Kind.Music:
                if (BackgroundMusicManager.Instance != null)
                {
                    if (on) BackgroundMusicManager.Instance.Unmute();
                    else BackgroundMusicManager.Instance.Mute();
                }
                break;
        }

        if (SoundEffectsManager.Instance != null)
            SoundEffectsManager.Instance.PlayButtonClick();

        RefreshSprite();
    }

    private void RefreshSprite()
    {
        if (img == null) return;
        img.sprite = IsOn ? onSprite : offSprite;
    }
}

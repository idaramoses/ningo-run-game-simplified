using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives Canvas_CompleteLevel: LEVEL COMPLETE heading, stars, level/language pill,
/// words-completed list (English -> translation + letter slots), run stats,
/// Next / Replay / Home buttons and Sound / Music toggles.
/// </summary>
public class LevelCompleteCanvas : MonoBehaviour
{
    public static LevelCompleteCanvas Instance { get; private set; }

    public class WordPair
    {
        public string english;
        public string translation;
    }

    [Header("Pill")]
    public TMP_Text levelLanguageText;

    [Header("Stars")]
    public RectTransform[] stars; // 3 star images; middle one bigger
    public float starRevealDelay = 0.4f;
    public float starPopScale = 1.4f;

    [Header("Words Panel")]
    public TMP_Text wordsCountText;          // "1 / 1"
    public RectTransform wordRowsParent;     // vertical layout container
    public Sprite letterSlotSprite;          // slot_letter_collected.png
    public Sprite arrowSprite;               // icon_translation_arrow.png
    public Sprite checkSprite;               // icon_check.png
    public TMP_FontAsset font;               // Fredoka-Bold SDF
    public float slotSize = 46f;
    public float slotSpacing = 6f;

    [Header("Stats")]
    public TMP_Text coinsText;
    public TMP_Text distanceText;

    [Header("Buttons")]
    public Button nextButton;
    public Button replayButton;
    public Button homeButton;
    public Button soundButton;
    public Button musicButton;
    public Image soundIcon;
    public Image musicIcon;
    public Color offTint = new Color(1f, 1f, 1f, 0.35f);

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        if (nextButton != null) nextButton.onClick.AddListener(OnNextLevel);
        if (replayButton != null) replayButton.onClick.AddListener(OnReplay);
        if (homeButton != null) homeButton.onClick.AddListener(OnHome);
        if (soundButton != null) soundButton.onClick.AddListener(ToggleSound);
        if (musicButton != null) musicButton.onClick.AddListener(ToggleMusic);
        RefreshToggleIcons();
        gameObject.SetActive(false);
    }

    // ---------------------------------------------------------------
    // SHOW
    // ---------------------------------------------------------------
    public void Show(int levelNumber, int wordsCollected, int totalWords, int stars, List<WordPair> words)
    {
        string lang = PlayerPrefs.GetString("user_selected_language", "").ToUpperInvariant();
        if (levelLanguageText != null)
            levelLanguageText.text = $"LEVEL {levelNumber}  \u2022  {lang}";

        if (wordsCountText != null)
            wordsCountText.text = $"{wordsCollected} / {totalWords}";

        BuildWordRows(words);

        int coins = PlayerPrefs.GetInt("RunCoins", 0);
        if (coinsText != null) coinsText.text = coins.ToString("N0");

        float dist = 0f;
        if (UImanager.uimanager != null) dist = UImanager.uimanager.GetRunDistance();
        if (distanceText != null) distanceText.text = $"{Mathf.FloorToInt(dist)} m";

        RefreshToggleIcons();
        gameObject.SetActive(true);
        StartCoroutine(RevealStars(stars));
    }

    private IEnumerator RevealStars(int count)
    {
        foreach (var s in stars)
            if (s != null) s.gameObject.SetActive(false);

        for (int i = 0; i < stars.Length && i < count; i++)
        {
            yield return new WaitForSecondsRealtime(starRevealDelay);
            if (stars[i] != null)
            {
                stars[i].gameObject.SetActive(true);
                StartCoroutine(Pop(stars[i]));
            }
        }
    }

    private IEnumerator Pop(RectTransform t)
    {
        t.localScale = Vector3.one * starPopScale;
        float d = 0.25f, e = 0f;
        while (e < d)
        {
            e += Time.unscaledDeltaTime;
            float k = 1f - (e / d);
            t.localScale = Vector3.one * Mathf.Lerp(1f, starPopScale, k);
            yield return null;
        }
        t.localScale = Vector3.one;
    }

    // ---------------------------------------------------------------
    // WORD ROWS
    // ---------------------------------------------------------------
    private void BuildWordRows(List<WordPair> words)
    {
        if (wordRowsParent == null) return;

        for (int i = wordRowsParent.childCount - 1; i >= 0; i--)
            Destroy(wordRowsParent.GetChild(i).gameObject);

        if (words == null) return;

        foreach (var w in words)
        {
            GameObject row = CreateWordRow(w);
            row.transform.SetParent(wordRowsParent, false);
        }
    }

    private GameObject CreateWordRow(WordPair pair)
    {
        // Root: vertical group -> [word line] [letter slots line]
        var row = new GameObject("WordRow", typeof(RectTransform), typeof(VerticalLayoutGroup));
        var vlg = row.GetComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 4f;
        var rowLE = row.AddComponent<LayoutElement>();
        rowLE.flexibleWidth = 1f;

        // Line 1: "WATER -> OMI ✓"
        var line = new GameObject("Line", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        line.transform.SetParent(row.transform, false);
        var hlg = line.GetComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.spacing = 10f;
        var lineLE = line.AddComponent<LayoutElement>();
        lineLE.preferredHeight = 30f;
        lineLE.flexibleWidth = 1f;

        CreateTMP(line.transform, pair.english.ToUpperInvariant(), 24f, Color.white);
        CreateIcon(line.transform, arrowSprite, 26f, 18f);
        CreateTMP(line.transform, pair.translation.ToUpperInvariant(), 24f, new Color(1f, 0.84f, 0f));
        CreateIcon(line.transform, checkSprite, 26f, 20f);

        // Line 2: letter slots
        var slots = new GameObject("Slots", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        slots.transform.SetParent(row.transform, false);
        var slg = slots.GetComponent<HorizontalLayoutGroup>();
        slg.childAlignment = TextAnchor.MiddleCenter;
        slg.childControlWidth = false;
        slg.childControlHeight = false;
        slg.childForceExpandWidth = false;
        slg.childForceExpandHeight = false;
        slg.spacing = slotSpacing;
        var slotsLE = slots.AddComponent<LayoutElement>();
        slotsLE.preferredHeight = slotSize;
        slotsLE.flexibleWidth = 1f;

        foreach (string g in TextUtils.SplitGraphemesNoWhitespace(pair.translation))
        {
            var slot = CreateIcon(slots.transform, letterSlotSprite, slotSize, slotSize);
            var letterTMP = CreateTMP(slot.transform, g.ToUpperInvariant(), 20f, Color.white);
            var lrt = letterTMP.rectTransform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.sizeDelta = Vector2.zero;
            letterTMP.alignment = TextAlignmentOptions.Center;
        }

        return row;
    }

    private Image CreateIcon(Transform parent, Sprite sprite, float w, float h)
    {
        var go = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = w;
        le.preferredHeight = h;
        return img;
    }

    private TMP_Text CreateTMP(Transform parent, string text, float size, Color color)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        if (font != null) tmp.font = font;
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = size + 10f;
        return tmp;
    }

    // ---------------------------------------------------------------
    // BUTTONS
    // ---------------------------------------------------------------
    private void OnNextLevel()
    {
        PlayClick();
        if (LevelManager.Instance != null)
            LevelManager.Instance.SetSelectedLevel(LevelManager.Instance.GetSelectedLevel() + 1);
        RestartRun();
    }

    private void OnReplay()
    {
        PlayClick();
        RestartRun();
    }

    private void OnHome()
    {
        PlayClick();
        Time.timeScale = 1f;
        if (UImanager.uimanager != null) UImanager.uimanager.Home();
    }

    private void RestartRun()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
        if (UImanager.uimanager != null) UImanager.uimanager.Restart();
    }

    private void ToggleSound()
    {
        bool on = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        on = !on;
        PlayerPrefs.SetInt("SoundEnabled", on ? 1 : 0);
        PlayerPrefs.Save();
        if (SoundEffectsManager.Instance != null)
            SoundEffectsManager.Instance.SetSoundEnabled(on);
        RefreshToggleIcons();
    }

    private void ToggleMusic()
    {
        bool on = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        on = !on;
        PlayerPrefs.SetInt("MusicEnabled", on ? 1 : 0);
        PlayerPrefs.Save();
        if (BackgroundMusicManager.Instance != null)
        {
            if (on) BackgroundMusicManager.Instance.Unmute();
            else BackgroundMusicManager.Instance.Mute();
        }
        RefreshToggleIcons();
    }

    private void RefreshToggleIcons()
    {
        bool soundOn = PlayerPrefs.GetInt("SoundEnabled", 1) == 1;
        bool musicOn = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        if (soundIcon != null) soundIcon.color = soundOn ? Color.white : offTint;
        if (musicIcon != null) musicIcon.color = musicOn ? Color.white : offTint;
    }

    private void PlayClick()
    {
        if (SoundEffectsManager.Instance != null)
            SoundEffectsManager.Instance.PlayButtonClick();
    }
}

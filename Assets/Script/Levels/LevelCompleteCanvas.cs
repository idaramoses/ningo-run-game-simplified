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

    [Header("Entrance Animation")]
    public float popDuration = 0.4f;         // scale 0 -> 1 duration per element
    public float popStagger = 0.08f;         // delay between elements
    public float overshoot = 1.15f;          // back-ease overshoot amount
    public float countUpDuration = 1.2f;     // coin/distance count-up time

    [Header("Words Panel")]
    public TMP_Text wordsCountText;          // "1 / 1"
    public RectTransform wordRowsParent;     // vertical layout container
    public Sprite letterSlotSprite;          // slot_letter_collected.png
    public Sprite arrowSprite;               // icon_translation_arrow.png
    public Sprite checkSprite;               // icon_check.png
    public TMP_FontAsset font;               // Fredoka-Bold SDF

    [Header("Words Panel Layout (adjust in Inspector)")]
    public float rowSpacing = 10f;           // space between word rows
    public float wordFontSize = 24f;         // "WATER -> OMI" size
    public float wordLineHeight = 30f;
    public float wordLineSpacing = 10f;      // gap between word, arrow, translation, check
    public Color wordEnglishColor = Color.white;
    public Color wordTranslationColor = new Color(1f, 0.84f, 0f);
    public float arrowWidth = 26f;
    public float arrowHeight = 18f;
    public float checkWidth = 26f;
    public float checkHeight = 20f;
    public float letterFontSize = 20f;       // letters inside slots
    public float slotSize = 46f;
    public float slotSpacing = 6f;
    public float lineToSlotGap = 4f;         // gap between text line and slots row

    [Header("Stats")]
    public TMP_Text coinsText;
    public TMP_Text distanceText;

    [Header("Dance Preview")]
    public GameObject dancePreviewRig; // world-space rig: camera + HighScoreRunnerPreview

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

        // Wire buttons here (not Start) so they're ready even if the panel
        // is activated for the first time by Show().
        if (nextButton != null) nextButton.onClick.AddListener(OnNextLevel);
        if (replayButton != null) replayButton.onClick.AddListener(OnReplay);
        if (homeButton != null) homeButton.onClick.AddListener(OnHome);
        if (soundButton != null) soundButton.onClick.AddListener(ToggleSound);
        if (musicButton != null) musicButton.onClick.AddListener(ToggleMusic);
        RefreshToggleIcons();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
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
        if (dancePreviewRig != null) dancePreviewRig.SetActive(true);
        gameObject.SetActive(true);
        StartCoroutine(AnimateEntrance(stars, coins, Mathf.FloorToInt(dist)));
    }

    private void OnDisable()
    {
        if (dancePreviewRig != null) dancePreviewRig.SetActive(false);
    }

    // ---------------------------------------------------------------
    // ENTRANCE ANIMATION
    // ---------------------------------------------------------------
    private IEnumerator AnimateEntrance(int starCount, int coins, int dist)
    {
        // Staggered pop-in for each section (by name, so missing ones are skipped)
        string[] order =
        {
            "Heading", "GreatRun", "LevelLangPill", "DancingPanel",
            "WordsPanel", "StatsPanel", "NextLevelButton",
            "BottomButtons", "AudioToggles"
        };

        for (int i = 0; i < order.Length; i++)
        {
            Transform t = transform.Find(order[i]);
            if (t != null) StartCoroutine(PopIn((RectTransform)t, i * popStagger));
        }

        // Stars come in after the main sections
        yield return new WaitForSecondsRealtime(order.Length * popStagger + 0.15f);
        StartCoroutine(RevealStars(starCount));

        // Coin / distance count-up
        if (coinsText != null) StartCoroutine(CountUp(coinsText, coins, ""));
        if (distanceText != null) StartCoroutine(CountUp(distanceText, dist, " m"));
    }

    private IEnumerator PopIn(RectTransform t, float delay)
    {
        if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
        t.localScale = Vector3.zero;
        float e = 0f;
        while (e < popDuration)
        {
            e += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(e / popDuration);
            // ease-out-back: 1 + (c+1)*(k-1)^3 + c*(k-1)^2
            float s = 1f + (overshoot + 1f) * Mathf.Pow(k - 1f, 3f) + overshoot * Mathf.Pow(k - 1f, 2f);
            t.localScale = Vector3.one * s;
            yield return null;
        }
        t.localScale = Vector3.one;
    }

    private IEnumerator CountUp(TMP_Text label, int target, string suffix)
    {
        float e = 0f;
        label.text = "0" + suffix;
        while (e < countUpDuration)
        {
            e += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(e / countUpDuration);
            int v = Mathf.RoundToInt(Mathf.Lerp(0f, target, 1f - Mathf.Pow(1f - k, 3f)));
            label.text = v.ToString("N0") + suffix;
            yield return null;
        }
        label.text = target.ToString("N0") + suffix;
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

        // Apply inspector spacing to the container's layout group
        var vlg = wordRowsParent.GetComponent<VerticalLayoutGroup>();
        if (vlg != null) vlg.spacing = rowSpacing;

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
        var row = new GameObject($"WordRow_{pair.english}", typeof(RectTransform), typeof(VerticalLayoutGroup));
        var vlg = row.GetComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;
        vlg.spacing = lineToSlotGap;
        var rowLE = row.AddComponent<LayoutElement>();
        rowLE.flexibleWidth = 1f;
        rowLE.preferredHeight = wordLineHeight + lineToSlotGap + slotSize;

        // Line 1: "WATER -> OMI ✓"
        var line = new GameObject("Line", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        line.transform.SetParent(row.transform, false);
        var hlg = line.GetComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.spacing = wordLineSpacing;
        var lineLE = line.AddComponent<LayoutElement>();
        lineLE.preferredHeight = wordLineHeight;
        lineLE.flexibleWidth = 1f;

        CreateTMP(line.transform, pair.english.ToUpperInvariant(), wordFontSize, wordEnglishColor);
        CreateIcon(line.transform, arrowSprite, arrowWidth, arrowHeight);
        CreateTMP(line.transform, pair.translation.ToUpperInvariant(), wordFontSize, wordTranslationColor);
        CreateIcon(line.transform, checkSprite, checkWidth, checkHeight);

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
            var letterTMP = CreateTMP(slot.transform, g.ToUpperInvariant(), letterFontSize, Color.white);
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
        Time.timeScale = 1f;
        gameObject.SetActive(false);
        if (UImanager.uimanager == null) return;

        // 1) Reset the game in the background - runner back to roadside idle,
        //    gameplay state cleared (same reset the fail panel's Home does)
        UImanager.uimanager.ResetRunnerToHomePose();

        // 2) Show Canvas_Level, pulse the newly-unlocked node, then auto-start it
        UImanager.uimanager.ShowLevelSelect();
        if (LevelMapCanvas.Instance != null)
            LevelMapCanvas.Instance.AnimateCurrentNodeThenStart();
    }

    private void OnReplay()
    {
        PlayClick();
        Time.timeScale = 1f;
        gameObject.SetActive(false);
        if (UImanager.uimanager == null) return;

        // 1) Reset the game in the background - runner back to roadside idle
        UImanager.uimanager.ResetRunnerToHomePose();

        // 2) Start the SAME level through the normal node-click flow
        //    (no Canvas_Level shown - goes straight into the run)
        int current = LevelManager.Instance != null ? LevelManager.Instance.GetSelectedLevel() : 1;
        UImanager.uimanager.StartLevelAndRun(current);
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

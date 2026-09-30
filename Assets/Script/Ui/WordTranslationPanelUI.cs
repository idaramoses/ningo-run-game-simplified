using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives the WordTranslationPanel on Canvas_HUD: level/language/word pills,
/// "Translate: X" prompt, the per-letter slot row (empty dashed box vs green
/// filled box + check), and the "n / m letters" counter.
/// If a WordManager exists it binds automatically and mirrors progress.
/// </summary>
public class WordTranslationPanelUI : MonoBehaviour
{
    [Header("Pills")]
    public TMP_Text levelText;
    public TMP_Text languageText;
    public TMP_Text wordCountText;

    [Header("Prompt & Counter")]
    public TMP_Text promptText;
    public TMP_Text lettersCountText;
    public Color promptWordColor = new Color(1f, 0.84f, 0f); // gold

    [Header("Letter Slots")]
    public RectTransform slotsContainer;
    [Tooltip("Inactive template: Image + Letter TMP + Check Image children.")]
    public GameObject slotTemplate;
    public Sprite emptySlotSprite;
    public Sprite filledSlotSprite;
    public Sprite connectorSprite;
    public float connectorSize = 16f;

    [Header("Data")]
    public WordManager wordManager;

    private string lastCollected = null;

    private void Start()
    {
        BindManager();
    }

    private void BindManager()
    {
        if (wordManager == null) wordManager = FindFirstObjectByType<WordManager>();
        if (wordManager != null)
        {
            wordManager.OnStartedRound += HandleRoundStarted;
            wordManager.OnWordChanged += HandleWordChanged;
            SyncFromManager();
        }
    }

    private void OnDestroy()
    {
        if (wordManager != null)
        {
            wordManager.OnStartedRound -= HandleRoundStarted;
            wordManager.OnWordChanged -= HandleWordChanged;
        }
    }

    private void OnEnable()
    {
        if (wordManager == null) BindManager();
        RefreshPills();
        SyncFromManager();
    }

    /// <summary>Level pill from LevelManager, language pill from saved user selection.</summary>
    private void RefreshPills()
    {
        if (LevelManager.Instance != null)
            SetLevel(LevelManager.Instance.GetSelectedLevel());

        string lang = PlayerPrefs.GetString("user_selected_language", "yoruba");
        SetLanguage(lang);
    }

    private void HandleWordChanged() => SyncFromManager();

    private void Update()
    {
        // Poll collected letters - WordManager has no per-letter event
        if (wordManager == null) return;
        string collected = wordManager.GetCollectedLetters();
        if (collected == lastCollected) return;
        lastCollected = collected;
        ApplyCollected(collected);
    }

    private void HandleRoundStarted() => SyncFromManager();

    private void SyncFromManager()
    {
        if (wordManager == null) return;
        string target = wordManager.GetPickedTranslationWord();
        if (string.IsNullOrEmpty(target)) return;

        SetPrompt(wordManager.CurrentWord != null ? wordManager.CurrentWord.word : "");
        SetWordCount(wordManager.WordIndexInLevel, wordManager.TargetWordsForLevel);
        BuildSlots(TextUtils.CountGraphemes(target));
        lastCollected = wordManager.GetCollectedLetters();
        ApplyCollected(lastCollected);
    }

    // --- Public API (usable without WordManager) ---

    public void SetLevel(int level) { if (levelText != null) levelText.text = $"LEVEL {level}"; }
    public void SetLanguage(string lang) { if (languageText != null) languageText.text = lang.ToUpperInvariant(); }
    public void SetWordCount(int index, int total) { if (wordCountText != null) wordCountText.text = $"Word {index} of {total}"; }

    public void SetPrompt(string english)
    {
        if (promptText == null) return;
        string hex = ColorUtility.ToHtmlStringRGB(promptWordColor);
        promptText.text = $"Translate: <color=#{hex}>{english.ToUpperInvariant()}</color>";
    }

    public void SetLettersCount(int collected, int total)
    {
        if (lettersCountText != null) lettersCountText.text = $"{collected} / {total} letters";
    }

    /// <summary>(Re)build the slot row with <paramref name="count"/> empty slots.</summary>
    public void BuildSlots(int count)
    {
        if (slotsContainer == null || slotTemplate == null || count <= 0) return;

        for (int i = slotsContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = slotsContainer.GetChild(i);
            if (child.gameObject != slotTemplate) Destroy(child.gameObject);
        }

        for (int i = 0; i < count; i++)
        {
            if (i > 0 && connectorSprite != null)
            {
                var conn = new GameObject("Connector", typeof(RectTransform), typeof(Image));
                conn.transform.SetParent(slotsContainer, false);
                conn.GetComponent<Image>().sprite = connectorSprite;
                conn.GetComponent<Image>().raycastTarget = false;
                conn.GetComponent<RectTransform>().sizeDelta = new Vector2(connectorSize, connectorSize);
            }

            GameObject slot = Instantiate(slotTemplate, slotsContainer);
            slot.name = $"Slot_{i}";
            slot.SetActive(true);
        }
    }

    /// <summary>Mark a slot as filled (green + letter + check badge).</summary>
    public void SetSlotCollected(int index, string letter, bool collected)
    {
        if (slotsContainer == null) return;
        Transform slot = slotsContainer.Find($"Slot_{index}");
        if (slot == null) return;

        var img = slot.GetComponent<Image>();
        if (img != null && filledSlotSprite != null)
            img.sprite = collected ? filledSlotSprite : emptySlotSprite;

        var letterT = slot.Find("Letter");
        if (letterT != null)
        {
            var tmp = letterT.GetComponent<TMP_Text>();
            if (tmp != null)
            {
                tmp.text = collected ? letter.ToUpperInvariant() : "";
                letterT.gameObject.SetActive(collected);
            }
        }

        var check = slot.Find("Check");
        if (check != null) check.gameObject.SetActive(collected);
    }

    private void ApplyCollected(string collected)
    {
        if (string.IsNullOrEmpty(collected)) return;
        int filled = 0, total = 0;
        for (int i = 0; i < collected.Length; i++)
        {
            char c = collected[i];
            bool isFilled = c != '_' && c != ' ';
            if (c != ' ') total++;
            if (isFilled) filled++;
            if (c != ' ') SetSlotCollected(total - 1, c.ToString(), isFilled);
        }
        SetLettersCount(filled, total);
    }
}

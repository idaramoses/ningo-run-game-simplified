using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Populates the shared "LEVEL X • LANGUAGE" pill + word progress section used by
/// the fail and pause panels. Reads live state from WordManager/LevelManager and
/// rebuilds the letter slots row (collected = filled slot + letter + check icon,
/// empty = dashed slot). Populates itself in OnEnable so it works whether the
/// panel is activated by UImanager or manually.
/// </summary>
public class NingoProgressHeader : MonoBehaviour
{
    [Header("Text")]
    public TMP_Text levelLanguageText;     // "LEVEL 4 • YORUBA"
    public TMP_Text wordIndexText;         // "Word 1 of 1"
    public TMP_Text englishWordText;       // "WATER"
    public TMP_Text lettersCountText;      // "1 / 3 letters collected"

    [Header("Letter Slots")]
    public RectTransform slotsParent;      // horizontal container for slot icons
    public Sprite slotCollectedSprite;     // filled slot (round on fail, square on pause)
    public Sprite slotEmptySprite;         // dashed empty slot
    public Sprite checkSprite;             // green check badge on collected slots
    public TMP_FontAsset font;             // Fredoka-Bold SDF
    public float slotSize = 64f;
    public float slotSpacing = 14f;
    public float letterFontSize = 34f;

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        int level = LevelManager.Instance != null ? LevelManager.Instance.GetSelectedLevel() : 1;
        string lang = PlayerPrefs.GetString("user_selected_language", "").ToUpperInvariant();
        if (string.IsNullOrEmpty(lang)) lang = "LANGUAGE";

        if (levelLanguageText != null)
            levelLanguageText.text = $"LEVEL {level}  \u2022  {lang}";

        var wm = WordManager.Instance;
        if (wm == null)
        {
            if (wordIndexText != null) wordIndexText.text = "";
            if (englishWordText != null) englishWordText.text = "";
            if (lettersCountText != null) lettersCountText.text = "";
            BuildSlots(null, null);
            return;
        }

        int wordIndex = Mathf.Max(1, wm.WordIndexInLevel);
        int target = Mathf.Max(1, wm.TargetWordsForLevel);
        if (wordIndexText != null)
            wordIndexText.text = $"Word {wordIndex} of {target}";

        if (englishWordText != null)
            englishWordText.text = wm.CurrentWord != null ? wm.CurrentWord.word.ToUpperInvariant() : "";

        var graphemes = wm.GetWordGraphemes();
        var slots = wm.GetCollectedSlots();
        int total = graphemes != null ? graphemes.Count : 0;
        int collected = wm.GetCollectedCount();

        if (lettersCountText != null)
            lettersCountText.text = $"{collected} / {total} letters collected";

        BuildSlots(graphemes, slots);
    }

    private void BuildSlots(System.Collections.Generic.IReadOnlyList<string> graphemes, string[] slots)
    {
        if (slotsParent == null) return;

        for (int i = slotsParent.childCount - 1; i >= 0; i--)
            Destroy(slotsParent.GetChild(i).gameObject);

        if (graphemes == null || graphemes.Count == 0) return;

        var hlg = slotsParent.GetComponent<HorizontalLayoutGroup>();
        if (hlg == null) hlg = slotsParent.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.spacing = slotSpacing;

        for (int i = 0; i < graphemes.Count; i++)
        {
            bool collected = slots != null && i < slots.Length &&
                             !string.IsNullOrWhiteSpace(slots[i]) && slots[i] != "_";

            var slot = new GameObject($"Slot_{i}", typeof(RectTransform), typeof(Image));
            slot.transform.SetParent(slotsParent, false);
            var img = slot.GetComponent<Image>();
            img.sprite = collected ? slotCollectedSprite : slotEmptySprite;
            img.preserveAspect = true;
            img.raycastTarget = false;
            var le = slot.AddComponent<LayoutElement>();
            le.preferredWidth = slotSize;
            le.preferredHeight = slotSize;

            if (collected)
            {
                // letter inside the filled slot
                var letterGo = new GameObject("Letter", typeof(RectTransform), typeof(TextMeshProUGUI));
                letterGo.transform.SetParent(slot.transform, false);
                var tmp = letterGo.GetComponent<TextMeshProUGUI>();
                tmp.text = graphemes[i].ToUpperInvariant();
                tmp.fontSize = letterFontSize;
                tmp.color = Color.white;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.raycastTarget = false;
                if (font != null) tmp.font = font;
                var lrt = letterGo.GetComponent<RectTransform>();
                lrt.anchorMin = Vector2.zero;
                lrt.anchorMax = Vector2.one;
                lrt.sizeDelta = Vector2.zero;

                // small check badge bottom-right of the slot
                if (checkSprite != null)
                {
                    var chk = new GameObject("Check", typeof(RectTransform), typeof(Image));
                    chk.transform.SetParent(slot.transform, false);
                    var ci = chk.GetComponent<Image>();
                    ci.sprite = checkSprite;
                    ci.preserveAspect = true;
                    ci.raycastTarget = false;
                    var crt = chk.GetComponent<RectTransform>();
                    crt.anchorMin = new Vector2(1f, 0f);
                    crt.anchorMax = new Vector2(1f, 0f);
                    crt.pivot = new Vector2(0.5f, 0.5f);
                    crt.sizeDelta = new Vector2(slotSize * 0.42f, slotSize * 0.42f);
                    crt.anchoredPosition = new Vector2(-slotSize * 0.12f, slotSize * 0.12f);
                }
            }
        }
    }
}

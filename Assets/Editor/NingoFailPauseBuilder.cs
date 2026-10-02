using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Rebuilds Canvas_Fail and Canvas_Pause to match the ningo-ui reference screens.
/// Run via Devin/BuildFailPauseScreens or call Rebuild() directly.
/// </summary>
public static class NingoFailPauseBuilder
{
    const string FAIL_DIR = "Assets/UI/ningo-ui/Home/fail/";
    const string PAUSE_DIR = "Assets/UI/ningo-ui/Home/pause/";
    const string FONT_PATH = "Assets/Font/Fredoka-Bold SDF.asset";

    static TMP_FontAsset font;

    static Sprite F(string n) => AssetDatabase.LoadAssetAtPath<Sprite>(FAIL_DIR + n + ".png");
    static Sprite P(string n) => AssetDatabase.LoadAssetAtPath<Sprite>(PAUSE_DIR + n + ".png");

    // ---------------------------------------------------------------
    // ENTRY
    // ---------------------------------------------------------------
    [MenuItem("Devin/Build Fail+Pause Screens")]
    public static void Rebuild()
    {
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATH);
        BuildFail();
        BuildPause();
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("[NingoFailPauseBuilder] Fail + Pause screens rebuilt.");
    }

    // ---------------------------------------------------------------
    // HELPERS
    // ---------------------------------------------------------------
    static GameObject Img(Transform parent, string name, Sprite sprite, Vector2 size, Vector2 anchoredPos,
        float anchorMinY = 1f, bool raycast = false)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0.5f, anchorMinY);
        rt.anchorMax = new Vector2(0.5f, anchorMinY);
        rt.pivot = new Vector2(0.5f, anchorMinY);
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = raycast;
        return go;
    }

    static TextMeshProUGUI Txt(Transform parent, string name, string text, float size, Color color,
        Vector2 size_, Vector2 anchoredPos, FontStyles style = FontStyles.Bold, float anchorMinY = 1f)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0.5f, anchorMinY);
        rt.anchorMax = new Vector2(0.5f, anchorMinY);
        rt.pivot = new Vector2(0.5f, anchorMinY);
        rt.sizeDelta = size_;
        rt.anchoredPosition = anchoredPos;
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        if (font != null) tmp.font = font;
        return tmp;
    }

    static GameObject Btn(Transform parent, string name, Sprite sprite, Vector2 size, Vector2 anchoredPos)
    {
        var go = Img(parent, name, sprite, size, anchoredPos, 1f, true);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = go.GetComponent<Image>();
        var fx = go.AddComponent<ButtonClickEffect>();
        return go;
    }

    static GameObject Stretch(Transform parent, string name, Sprite sprite)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Sliced;
        img.raycastTarget = false;
        return go;
    }

    // ---------------------------------------------------------------
    // FAIL SCREEN  (reference: heading TRY AGAIN!, sub "You can do this!",
    // pill, YOUR PROGRESS panel w/ word + round slots, stats bar,
    // TRY AGAIN button, HOME button, sound/music toggles)
    // ---------------------------------------------------------------
    static void BuildFail()
    {
        var root = FindCanvas("Canvas_Fail");
        if (root == null) { Debug.LogError("[Builder] Canvas_Fail not found"); return; }

        // wipe existing children
        for (int i = root.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(root.GetChild(i).gameObject);

        var failCtrl = root.GetComponent<FailPanelController>();

        // 1) opaque background
        Stretch(root, "Background", F("background_fail"));

        // 2) heading TRY AGAIN!
        Img(root, "Heading", F("heading_try_again"), new Vector2(320, 90), new Vector2(0, -70));

        // 3) "You can do this!"
        Txt(root, "Subtitle", "You can do this!", 17f, Color.white,
            new Vector2(300, 24), new Vector2(0, -155));

        // 4) level/language pill
        var pill = Img(root, "LevelLangPill", F("pill_level_language"), new Vector2(180, 34), new Vector2(0, -195));
        var pillText = Txt(pill.transform, "Text", "LEVEL 1  •  YORUBA", 13f, Color.white,
            new Vector2(170, 28), new Vector2(0, -15));

        // 5) progress panel
        var prog = Img(root, "ProgressPanel", F("panel_progress"), new Vector2(330, 210), new Vector2(0, -232));
        var progImg = prog.GetComponent<Image>();
        progImg.type = Image.Type.Sliced;

        Txt(prog.transform, "ProgressTitle", "YOUR PROGRESS", 15f, Color.white,
            new Vector2(280, 22), new Vector2(0, -20));
        var wordIndex = Txt(prog.transform, "WordIndex", "Word 1 of 1", 13f, new Color(1f, 1f, 1f, 0.8f),
            new Vector2(280, 20), new Vector2(0, -48));
        var wordText = Txt(prog.transform, "EnglishWord", "WATER", 26f, new Color(1f, 0.84f, 0f),
            new Vector2(280, 34), new Vector2(0, -72));

        // slots container (filled by NingoProgressHeader)
        var slots = new GameObject("LetterSlots", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        slots.transform.SetParent(prog.transform, false);
        var srt = (RectTransform)slots.transform;
        srt.anchorMin = new Vector2(0.5f, 1f);
        srt.anchorMax = new Vector2(0.5f, 1f);
        srt.pivot = new Vector2(0.5f, 1f);
        srt.sizeDelta = new Vector2(280, 64);
        srt.anchoredPosition = new Vector2(0, -105);

        var lettersCount = Txt(prog.transform, "LettersCount", "0 / 0 letters collected", 12f,
            new Color(1f, 1f, 1f, 0.75f), new Vector2(280, 18), new Vector2(0, -180));

        // attach progress header to the canvas root
        var header = root.gameObject.AddComponent<NingoProgressHeader>();
        header.levelLanguageText = pillText;
        header.wordIndexText = wordIndex;
        header.englishWordText = wordText;
        header.lettersCountText = lettersCount;
        header.slotsParent = srt;
        header.slotCollectedSprite = F("slot_collected_round");
        header.slotEmptySprite = F("slot_empty_round");
        header.checkSprite = F("icon_check");
        header.font = font;
        header.slotWidth = 64f;
        header.slotHeight = 64f;
        header.slotSpacing = 14f;
        header.letterFontSize = 32f;

        // 6) stats bar
        var stats = Img(root, "StatsPanel", F("panel_run_stats"), new Vector2(330, 62), new Vector2(0, -452));
        stats.GetComponent<Image>().type = Image.Type.Sliced;

        Img(stats.transform, "CoinIcon", F("icon_coin"), new Vector2(28, 28), new Vector2(-75, -17));
        var coinsVal = Txt(stats.transform, "CoinsValue", "0", 17f, Color.white,
            new Vector2(80, 22), new Vector2(-35, -12), FontStyles.Bold);
        Txt(stats.transform, "CoinsLabel", "Coins", 11f, new Color(1f, 1f, 1f, 0.75f),
            new Vector2(80, 16), new Vector2(-35, -36));

        Img(stats.transform, "DistanceIcon", F("icon_distance"), new Vector2(28, 28), new Vector2(45, -17));
        var distVal = Txt(stats.transform, "DistanceValue", "0 m", 17f, Color.white,
            new Vector2(80, 22), new Vector2(85, -12), FontStyles.Bold);
        Txt(stats.transform, "DistanceLabel", "Distance", 11f, new Color(1f, 1f, 1f, 0.75f),
            new Vector2(80, 16), new Vector2(85, -36));

        // 7) TRY AGAIN + HOME buttons
        var tryBtn = Btn(root, "TryAgainButton", F("button_try_again"), new Vector2(330, 62), new Vector2(0, -524));
        var homeBtn = Btn(root, "HomeButton", F("button_home"), new Vector2(240, 50), new Vector2(0, -596));

        // 8) sound / music toggles
        var sndBtn = Btn(root, "SoundButton", F("button_sound"), new Vector2(44, 44), new Vector2(-40, -670));
        Txt(root, "SoundLabel", "Sound", 10f, new Color(1f, 1f, 1f, 0.75f),
            new Vector2(70, 16), new Vector2(-40, -708));
        var musBtn = Btn(root, "MusicButton", F("button_music"), new Vector2(44, 44), new Vector2(40, -670));
        Txt(root, "MusicLabel", "Music", 10f, new Color(1f, 1f, 1f, 0.75f),
            new Vector2(70, 16), new Vector2(40, -708));

        // wire buttons
        if (failCtrl != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                tryBtn.GetComponent<Button>().onClick, failCtrl.OnRestartPressed);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                homeBtn.GetComponent<Button>().onClick, failCtrl.OnHomePressed);
        }
        WireToggle(sndBtn.GetComponent<Button>(), "Sound");
        WireToggle(musBtn.GetComponent<Button>(), "Music");

        // stats texts -> FailPanelController
        if (failCtrl != null)
        {
            failCtrl.coinsText = coinsVal;
            failCtrl.distanceText = distVal;
        }

        EditorUtility.SetDirty(root.gameObject);
    }

    // ---------------------------------------------------------------
    // PAUSE SCREEN (reference: PAUSED heading, pill, word + square slots,
    // RESUME big button, RESTART | HOME row, sound/music toggles w/ labels)
    // ---------------------------------------------------------------
    static void BuildPause()
    {
        var root = FindCanvas("Canvas_Pause");
        if (root == null) { Debug.LogError("[Builder] Canvas_Pause not found"); return; }

        for (int i = root.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(root.GetChild(i).gameObject);

        var pauseCtrl = root.GetComponent<PausePanelController>();

        // 1) background
        Stretch(root, "Background", P("background_pause"));

        // 2) PAUSED heading
        Img(root, "Heading", P("heading_paused"), new Vector2(300, 90), new Vector2(0, -70));

        // 3) pill
        var pill = Img(root, "LevelLangPill", P("pill_level_language"), new Vector2(180, 34), new Vector2(0, -155));
        var pillText = Txt(pill.transform, "Text", "LEVEL 1  •  YORUBA", 13f, Color.white,
            new Vector2(170, 28), new Vector2(0, -15));

        // 4) word + slots (no frame — straight on background)
        var wordIndex = Txt(root, "WordIndex", "Word 1 of 1", 14f, new Color(1f, 1f, 1f, 0.85f),
            new Vector2(280, 20), new Vector2(0, -212));
        var wordText = Txt(root, "EnglishWord", "WATER", 30f, new Color(1f, 0.84f, 0f),
            new Vector2(280, 40), new Vector2(0, -238));

        var slots = new GameObject("LetterSlots", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        slots.transform.SetParent(root, false);
        var srt = (RectTransform)slots.transform;
        srt.anchorMin = new Vector2(0.5f, 1f);
        srt.anchorMax = new Vector2(0.5f, 1f);
        srt.pivot = new Vector2(0.5f, 1f);
        srt.sizeDelta = new Vector2(280, 68);
        srt.anchoredPosition = new Vector2(0, -288);

        var lettersCount = Txt(root, "LettersCount", "0 / 0 letters", 12f, new Color(1f, 1f, 1f, 0.75f),
            new Vector2(280, 18), new Vector2(0, -366));

        var header = root.gameObject.AddComponent<NingoProgressHeader>();
        header.levelLanguageText = pillText;
        header.wordIndexText = wordIndex;
        header.englishWordText = wordText;
        header.lettersCountText = lettersCount;
        header.slotsParent = srt;
        header.slotCollectedSprite = P("slot_collected");
        header.slotEmptySprite = P("slot_empty");
        header.checkSprite = P("icon_check");
        header.font = font;
        header.slotWidth = 66f;
        header.slotHeight = 66f;
        header.slotSpacing = 16f;
        header.letterFontSize = 34f;

        // 5) RESUME
        var resumeBtn = Btn(root, "ResumeButton", P("button_resume"), new Vector2(330, 66), new Vector2(0, -420));

        // 6) RESTART | HOME side by side
        var restartBtn = Btn(root, "RestartButton", P("button_restart"), new Vector2(158, 56), new Vector2(-84, -506));
        var homeBtn = Btn(root, "HomeButton", P("button_home"), new Vector2(158, 56), new Vector2(84, -506));

        // 7) sound / music toggles with labels
        var sndBtn = Btn(root, "SoundButton", P("button_sound_on"), new Vector2(46, 46), new Vector2(-45, -600));
        Txt(root, "SoundLabel", "Sound", 10f, new Color(1f, 1f, 1f, 0.75f),
            new Vector2(70, 16), new Vector2(-45, -642));
        var musBtn = Btn(root, "MusicButton", P("button_music_on"), new Vector2(46, 46), new Vector2(45, -600));
        Txt(root, "MusicLabel", "Music", 10f, new Color(1f, 1f, 1f, 0.75f),
            new Vector2(70, 16), new Vector2(45, -642));

        // wire buttons
        if (pauseCtrl != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                resumeBtn.GetComponent<Button>().onClick, pauseCtrl.OnResumePressed);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                restartBtn.GetComponent<Button>().onClick, pauseCtrl.OnRestartPressed);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                homeBtn.GetComponent<Button>().onClick, pauseCtrl.OnHomePressed);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                sndBtn.GetComponent<Button>().onClick, pauseCtrl.OnSoundToggle);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                musBtn.GetComponent<Button>().onClick, pauseCtrl.OnMusicToggle);

            // on/off sprite swap
            pauseCtrl.soundIcon = sndBtn.GetComponent<Image>();
            pauseCtrl.musicIcon = musBtn.GetComponent<Image>();
            pauseCtrl.soundOnSprite = P("button_sound_on");
            pauseCtrl.soundOffSprite = P("button_sound_off");
            pauseCtrl.musicOnSprite = P("button_music_on");
            pauseCtrl.musicOffSprite = P("button_music_off");
        }

        EditorUtility.SetDirty(root.gameObject);
    }

    static void WireToggle(Button btn, string kind)
    {
        // fail panel doesn't have on/off sprite pairs — hook to PausePanelController-style
        // PlayerPrefs toggles via a small runtime listener added at build time.
        var go = btn.gameObject;
        var toggler = go.AddComponent<SimpleAudioToggle>();
        toggler.kind = kind == "Sound" ? SimpleAudioToggle.Kind.Sound : SimpleAudioToggle.Kind.Music;
    }

    static Transform FindCanvas(string name)
    {
        foreach (var g in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (!g.scene.IsValid()) continue;
            if (g.name == name) return g.transform;
        }
        return null;
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// Rebuilds Canvas_Settings, Canvas_Missions, Canvas_Shop, Canvas_PowerUps and
/// Canvas_Daily_Gift to match the new ningo-ui menu artwork. Existing controllers
/// are kept and their serialized references are re-pointed at the new objects.
/// Run via Devin/Build Menu Screens.
/// </summary>
public static class NingoMenuScreensBuilder
{
    const string DIR = "Assets/UI/ningo-ui/Home/reward,mission,shop,powerup,setings/";
    const string FONT_PATH = "Assets/Font/Fredoka-Bold SDF.asset";
    const string MISSION_CARD_PREFAB = "Assets/Prefab/MissionCardItem.prefab";

    static TMP_FontAsset font;

    static Sprite S(string n) => AssetDatabase.LoadAssetAtPath<Sprite>(DIR + n + ".png");

    [MenuItem("Devin/Build Menu Screens")]
    public static void Rebuild()
    {
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATH);
        BuildSettings();
        BuildMissions();
        BuildShop();
        BuildPowerUps();
        BuildDailyGift();

        // All menu canvases must share Canvas_Home's scaler settings so the
        // same fixed-position layout looks identical on every screen.
        foreach (var name in new[] { "Canvas_Settings", "Canvas_Missions", "Canvas_Shop", "Canvas_PowerUps", "Canvas_Daily_Gift" })
        {
            var c = FindCanvas(name);
            if (c == null) continue;
            var scaler = c.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(390, 844);
                scaler.matchWidthOrHeight = 1f;
            }
            var rt = c.GetComponent<RectTransform>();
            if (rt != null) rt.localScale = Vector3.one;
            EditorUtility.SetDirty(c.gameObject);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        AssetDatabase.SaveAssets();
        Debug.Log("[NingoMenuScreensBuilder] Menu screens rebuilt.");
    }

    // ---------------------------------------------------------------
    // HELPERS
    // ---------------------------------------------------------------
    static RectTransform NewRect(Transform parent, string name, Vector2 size, Vector2 pos,
        float anchorX = 0.5f, float anchorY = 1f)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(anchorX, anchorY);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        return rt;
    }

    static GameObject Img(Transform parent, string name, Sprite sprite, Vector2 size, Vector2 pos,
        float anchorX = 0.5f, float anchorY = 1f, bool raycast = false, bool sliced = false)
    {
        var rt = NewRect(parent, name, size, pos, anchorX, anchorY);
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = !sliced;
        if (sliced) img.type = Image.Type.Sliced;
        img.raycastTarget = raycast;
        return rt.gameObject;
    }

    static TextMeshProUGUI Txt(Transform parent, string name, string text, float size, Color color,
        Vector2 sizeDelta, Vector2 pos, FontStyles style = FontStyles.Bold,
        float anchorX = 0.5f, float anchorY = 1f, TextAlignmentOptions align = TextAlignmentOptions.Center)
    {
        var rt = NewRect(parent, name, sizeDelta, pos, anchorX, anchorY);
        var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = align;
        tmp.raycastTarget = false;
        if (font != null) tmp.font = font;
        return tmp;
    }

    static Button Btn(Transform parent, string name, Sprite sprite, Vector2 size, Vector2 pos,
        float anchorX = 0.5f, float anchorY = 1f)
    {
        var go = Img(parent, name, sprite, size, pos, anchorX, anchorY, true);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = go.GetComponent<Image>();
        go.AddComponent<ButtonClickEffect>();
        return btn;
    }

    static void Stretch(Transform parent, string name, Sprite sprite)
    {
        var rt = NewRect(parent, name, Vector2.zero, Vector2.zero, 0f, 1f);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Sliced;
        img.raycastTarget = false;
    }

    static Transform FindCanvas(string name)
    {
        foreach (var g in Resources.FindObjectsOfTypeAll<GameObject>())
            if (g.scene.IsValid() && g.name == name) return g.transform;
        return null;
    }

    static void WipeChildren(Transform root, params string[] keepNames)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            var ch = root.GetChild(i);
            bool keep = false;
            foreach (var k in keepNames) if (ch.name.Contains(k)) keep = true;
            if (!keep) Object.DestroyImmediate(ch.gameObject);
        }
    }

    static SerializedObject SO(Object o) => new SerializedObject(o);

    static void SetRef(SerializedObject so, string prop, Object value)
    {
        var p = so.FindProperty(prop);
        if (p != null) p.objectReferenceValue = value;
    }

    // ---------------------------------------------------------------
    // SETTINGS
    // ---------------------------------------------------------------
    static void BuildSettings()
    {
        var root = FindCanvas("Canvas_Settings");
        if (root == null) { Debug.LogError("[Builder] Canvas_Settings missing"); return; }

        // keep the modal dialogs (controller references them)
        WipeChildren(root, "Modal");

        var ctrl = root.GetComponent<SettingsController>();

        Stretch(root, "Background", S("background_menu"));

        var back = Btn(root, "BackButton", S("button_back"), new Vector2(56, 56), new Vector2(45, -60), 0f, 1f);
        Img(root, "Heading", S("heading_settings"), new Vector2(300, 105), new Vector2(0, -62));

        // ---- toggle rows ----
        float y = -215f, step = 78f;
        BuildSettingsRow(root, "SoundRow", S("icon_sound"), "Sound", true, y, NingoPillToggle.Kind.Sound);
        y -= step;
        BuildSettingsRow(root, "MusicRow", S("icon_music"), "Music", true, y, NingoPillToggle.Kind.Music);
        y -= step;
        BuildSettingsRow(root, "VibrationRow", S("icon_vibration"), "Vibration", true, y, NingoPillToggle.Kind.Vibration);

        // ---- language row ----
        y -= step;
        var langRow = Img(root, "LanguageRow", S("panel_row_blank"), new Vector2(350, 64), new Vector2(0, y), 0.5f, 1f, false, true);
        Img(langRow.transform, "Icon", S("icon_language"), new Vector2(38, 38), new Vector2(-140, -13));
        Txt(langRow.transform, "Label", "Learning language", 18f, Color.white,
            new Vector2(160, 30), new Vector2(-35, -17), FontStyles.Bold);
        var pill = Btn(langRow.transform, "LangPill", S("pill_blank"), new Vector2(125, 44), new Vector2(105, -10));
        var langTxt = Txt(pill.transform, "Text", "Yoruba", 15f, new Color(1f, 0.84f, 0f),
            new Vector2(90, 30), new Vector2(-10, -7));
        Img(pill.transform, "Arrow", S("icon_arrow_right"), new Vector2(18, 18), new Vector2(45, -13));
        var pillComp = langRow.AddComponent<NingoLanguagePill>();
        pillComp.text = langTxt;

        // ---- guest / sign in row ----
        y -= step;
        var guestRow = Img(root, "GuestRow", S("panel_row_blank"), new Vector2(350, 64), new Vector2(0, y), 0.5f, 1f, false, true);
        Img(guestRow.transform, "Icon", S("icon_guest"), new Vector2(38, 38), new Vector2(-140, -13));
        Txt(guestRow.transform, "Label", "Playing as Guest", 17f, Color.white,
            new Vector2(160, 30), new Vector2(-35, -17), FontStyles.Bold);
        var signIn = Btn(guestRow.transform, "SignInButton", S("button_green_blank"), new Vector2(115, 48), new Vector2(105, -8));
        signIn.GetComponent<Image>().type = Image.Type.Sliced;
        Txt(signIn.transform, "Text", "SIGN IN", 17f, Color.white, new Vector2(110, 30), new Vector2(0, -9));

        // ---- how to play / support row ----
        y -= step + 4f;
        var htp = Btn(root, "HowToPlayButton", S("panel_row_blank"), new Vector2(168, 64), new Vector2(-88, y), 0.5f, 1f);
        htp.GetComponent<Image>().type = Image.Type.Sliced;
        Img(htp.transform, "Icon", S("icon_how_to_play"), new Vector2(34, 34), new Vector2(-52, -15));
        Txt(htp.transform, "Label", "HOW TO PLAY", 13f, Color.white, new Vector2(115, 30), new Vector2(18, -17));

        var sup = Btn(root, "SupportButton", S("panel_row_blank"), new Vector2(168, 64), new Vector2(88, y), 0.5f, 1f);
        sup.GetComponent<Image>().type = Image.Type.Sliced;
        Img(sup.transform, "Icon", S("icon_support"), new Vector2(34, 34), new Vector2(-52, -15));
        Txt(sup.transform, "Label", "SUPPORT", 13f, Color.white, new Vector2(115, 30), new Vector2(18, -17));

        // ---- footer privacy | terms ----
        var privBtn = Btn(root, "PrivacyButton", null, new Vector2(80, 28), new Vector2(-55, -790));
        privBtn.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        Txt(privBtn.transform, "Text", "Privacy", 14f, new Color(1f, 1f, 1f, 0.75f), new Vector2(80, 28), new Vector2(0, -14), FontStyles.Normal);
        Txt(root, "Sep", "|", 14f, new Color(1f, 1f, 1f, 0.5f), new Vector2(20, 28), new Vector2(0, -804), FontStyles.Normal);
        var termBtn = Btn(root, "TermsButton", null, new Vector2(80, 28), new Vector2(55, -790));
        termBtn.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        Txt(termBtn.transform, "Text", "Terms", 14f, new Color(1f, 1f, 1f, 0.75f), new Vector2(80, 28), new Vector2(0, -14), FontStyles.Normal);

        // ---- language modal (API-driven via SelectionModalController + LanguageCache) ----
        var optPrefab = BuildOptionItemPrefab();
        var langModal = root.Find("Language_Modal");
        if (langModal == null)
        {
            langModal = BuildSelectionModal(root, "Language_Modal");
        }
        SelectionModalController langModalCtrl = langModal.GetComponent<SelectionModalController>();
        if (langModalCtrl != null)
        {
            var lso = SO(langModalCtrl);
            SetRef(lso, "optionPrefab", optPrefab);
            var pk = lso.FindProperty("playerPrefsKey");
            if (pk != null) pk.stringValue = "user_selected_language";
            // options are populated at runtime from LanguageCache (API) - keep the
            // serialized list empty so no stale hardcoded languages are shown
            var od = lso.FindProperty("optionDataList");
            if (od != null) od.arraySize = 0;
            lso.ApplyModifiedPropertiesWithoutUndo();
        }
        langModal.gameObject.SetActive(false);

        // ---- how to play modal ----
        var htpModal = root.Find("HowToPlay_Modal");
        if (htpModal == null)
        {
            var mrt = NewRect(root, "HowToPlay_Modal", Vector2.zero, Vector2.zero, 0.5f, 0.5f);
            mrt.anchorMin = Vector2.zero;
            mrt.anchorMax = Vector2.one;
            mrt.offsetMin = mrt.offsetMax = Vector2.zero;
            var dim = mrt.gameObject.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.75f);
            var dismiss = mrt.gameObject.AddComponent<Button>();
            dismiss.transition = Selectable.Transition.None;
            htpModal = mrt;

            var card = Img(htpModal, "Panel", S("panel_card_blank"), new Vector2(330, 470),
                new Vector2(0, -180), 0.5f, 1f, false, true);
            Txt(card.transform, "Title", "HOW TO PLAY", 24f, Color.white,
                new Vector2(300, 34), new Vector2(0, -24));
            Txt(card.transform, "Body",
                "1. Tap the play button to start a level.\n\n" +
                "2. Swipe left or right to change lanes.\n\n" +
                "3. Tap or swipe up to jump over obstacles.\n\n" +
                "4. Collect the letters of the translated word in order.\n\n" +
                "5. Wrong letters end the run - watch the word!\n\n" +
                "6. Finish the word to complete the level.",
                15f, new Color(1f, 1f, 1f, 0.9f), new Vector2(285, 300),
                new Vector2(0, -75), FontStyles.Normal, 0.5f, 1f, TextAlignmentOptions.TopLeft);

            var close = Btn(htpModal, "CloseButton", S("button_back"), new Vector2(56, 56),
                new Vector2(-140, -190), 0.5f, 1f);
            UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(
                close.onClick, htpModal.gameObject.SetActive, false);
            UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(
                dismiss.onClick, htpModal.gameObject.SetActive, false);
            htpModal.gameObject.SetActive(false);
        }

        if (ctrl != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(back.onClick, ctrl.OnBackPressed);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(pill.onClick, ctrl.OnLanguagePressed);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(sup.onClick, ctrl.OnSupportPressed);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(privBtn.onClick, ctrl.OnPrivacyPressed);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(termBtn.onClick, ctrl.OnTermsPressed);
            UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(
                htp.onClick, htpModal.gameObject.SetActive, true);

            // sign-in: open the user-info/login canvas and hide settings
            var ui = Object.FindObjectOfType<UImanager>(true);
            if (ui != null && ui.canvasUserInfo != null)
            {
                UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(
                    signIn.onClick, ui.canvasUserInfo.SetActive, true);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(signIn.onClick, ctrl.OnBackPressed);
            }

            var cso2 = SO(ctrl);
            if (langModalCtrl != null) SetRef(cso2, "languageModal", langModalCtrl);
            cso2.ApplyModifiedPropertiesWithoutUndo();
        }
        else
        {
            UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(
                htp.onClick, htpModal.gameObject.SetActive, true);
        }

        // ---- shared modal artwork + scrollable option lists ----
        var modalSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/UI/ningo-ui/Home/compete-level/modal.png");
        for (int i = 0; i < root.childCount; i++)
        {
            var ch = root.GetChild(i);
            if (!ch.name.Contains("Modal")) continue;
            var panel = ch.Find("Panel");
            if (panel == null) continue;
            var pimg = panel.GetComponent<Image>();
            if (pimg != null && modalSprite != null)
            {
                pimg.sprite = modalSprite;
                pimg.type = Image.Type.Sliced;
                pimg.preserveAspect = false;
            }
            MakeScrollableOptions(panel);
        }

        // modals must render on top of every other settings element
        var modals = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < root.childCount; i++)
        {
            var ch = root.GetChild(i);
            if (ch.name.Contains("Modal")) modals.Add(ch);
        }
        foreach (var m in modals) m.SetAsLastSibling();

        EditorUtility.SetDirty(root.gameObject);
    }

    /// <summary>Wraps Panel/OptionsList in a ScrollRect viewport so a long list
    /// (e.g. API languages) scrolls instead of overflowing the panel.</summary>
    static void MakeScrollableOptions(Transform panel)
    {
        var list = panel.Find("OptionsList");
        if (list == null) return;
        var listRt = list.GetComponent<RectTransform>();
        if (listRt == null) return;

        RectTransform viewport;
        if (list.parent.name == "ScrollView")
        {
            viewport = list.parent.GetComponent<RectTransform>();
            var existing = list.parent.GetComponent<Image>();
            if (existing == null)
            {
                existing = list.parent.gameObject.AddComponent<Image>();
                existing.color = new Color(0f, 0f, 0f, 0f);
            }
            existing.raycastTarget = true;
        }
        else
        {
            // capture the list's current layout inside the panel
            var anchorMin = listRt.anchorMin;
            var anchorMax = listRt.anchorMax;
            var size = listRt.sizeDelta;
            var pos = listRt.anchoredPosition;

            var sv = new GameObject("ScrollView", typeof(RectTransform), typeof(RectMask2D), typeof(Image));
            sv.transform.SetParent(panel, false);
            sv.transform.SetSiblingIndex(list.GetSiblingIndex());
            // transparent graphic so the ScrollRect can receive drag events
            var svImg = sv.GetComponent<Image>();
            svImg.color = new Color(0f, 0f, 0f, 0f);
            svImg.raycastTarget = true;
            viewport = (RectTransform)sv.transform;
            viewport.anchorMin = anchorMin;
            viewport.anchorMax = anchorMax;
            viewport.pivot = listRt.pivot;
            viewport.sizeDelta = size;
            viewport.anchoredPosition = pos;
            var svRect = sv.AddComponent<ScrollRect>();

            list.SetParent(viewport, false);
            svRect.content = listRt;
            svRect.horizontal = false;
            svRect.vertical = true;
            svRect.movementType = ScrollRect.MovementType.Clamped;
            svRect.scrollSensitivity = 25f;
        }

        // content: top-anchored, stretches horizontally, grows vertically
        listRt.anchorMin = new Vector2(0f, 1f);
        listRt.anchorMax = new Vector2(1f, 1f);
        listRt.pivot = new Vector2(0.5f, 1f);
        listRt.anchoredPosition = Vector2.zero;
        listRt.offsetMin = new Vector2(listRt.offsetMin.x, 0f);
        listRt.offsetMax = new Vector2(listRt.offsetMax.x, listRt.offsetMax.y);

        var vlg = list.GetComponent<VerticalLayoutGroup>();
        if (vlg == null) vlg = list.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.spacing = 8f;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;

        var csf = list.GetComponent<ContentSizeFitter>();
        if (csf == null) csf = list.gameObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    /// <summary>Creates the modal structure that SelectionModalController
    /// auto-discovers: Overlay + Panel(CloseButton, Subtitle, OptionsList, ConfirmButton).</summary>
    static Transform BuildSelectionModal(Transform root, string name)
    {
        var mrt = NewRect(root, name, Vector2.zero, Vector2.zero, 0.5f, 0.5f);
        mrt.anchorMin = Vector2.zero;
        mrt.anchorMax = Vector2.one;
        mrt.offsetMin = mrt.offsetMax = Vector2.zero;
        mrt.gameObject.AddComponent<SelectionModalController>();

        // dimmed overlay that dismisses on click
        var ov = NewRect(mrt, "Overlay", Vector2.zero, Vector2.zero, 0.5f, 0.5f);
        ov.anchorMin = Vector2.zero;
        ov.anchorMax = Vector2.one;
        ov.offsetMin = ov.offsetMax = Vector2.zero;
        var ovImg = ov.gameObject.AddComponent<Image>();
        ovImg.color = new Color(0f, 0f, 0f, 0.6f);
        var ovBtn = ov.gameObject.AddComponent<Button>();
        ovBtn.transition = Selectable.Transition.None;

        var panel = Img(mrt, "Panel", S("panel_card_blank"), new Vector2(362, 540),
            Vector2.zero, 0.5f, 0.5f, false, true);

        var close = Img(panel.transform, "CloseButton", S("button_back"), new Vector2(34, 34),
            new Vector2(165, -14), 0.5f, 1f, true);
        close.AddComponent<Button>().targetGraphic = close.GetComponent<Image>();

        Txt(panel.transform, "Subtitle", "Choose an option", 15f, new Color(1f, 1f, 1f, 0.85f),
            new Vector2(300, 24), new Vector2(0, -52), FontStyles.Normal);

        var list = NewRect(panel.transform, "OptionsList", new Vector2(332, 350),
            new Vector2(0, -95));
        var vlg = list.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.spacing = 8f;

        var confirm = Img(panel.transform, "ConfirmButton", S("button_green_blank"),
            new Vector2(180, 48), new Vector2(0, -480), 0.5f, 1f, true, true);
        confirm.AddComponent<Button>().targetGraphic = confirm.GetComponent<Image>();
        confirm.AddComponent<ButtonClickEffect>();
        Txt(confirm.transform, "Text", "CONFIRM", 17f, Color.white,
            new Vector2(170, 30), new Vector2(0, -9));

        return mrt;
    }

    static void BuildSettingsRow(Transform root, string name, Sprite icon, string label, bool toggle,
        float y, NingoPillToggle.Kind kind)
    {
        var row = Img(root, name, S("panel_row_blank"), new Vector2(350, 64), new Vector2(0, y), 0.5f, 1f, false, true);
        Img(row.transform, "Icon", icon, new Vector2(38, 38), new Vector2(-140, -13));
        Txt(row.transform, "Label", label, 18f, Color.white, new Vector2(170, 30), new Vector2(-35, -17), FontStyles.Bold);

        var tog = Btn(row.transform, "Toggle", S("toggle_on"), new Vector2(92, 40), new Vector2(112, -12));
        var nt = tog.gameObject.AddComponent<NingoPillToggle>();
        nt.kind = kind;
        nt.onSprite = S("toggle_on");
        nt.offSprite = S("toggle_off");
    }

    // ---------------------------------------------------------------
    // MISSIONS
    // ---------------------------------------------------------------
    static void BuildMissions()
    {
        var root = FindCanvas("Canvas_Missions");
        if (root == null) { Debug.LogError("[Builder] Canvas_Missions missing"); return; }
        WipeChildren(root);

        var ctrl = root.GetComponent<MissionsController>();

        Stretch(root, "Background", S("background_menu"));
        var back = Btn(root, "BackButton", S("button_back"), new Vector2(56, 56), new Vector2(45, -60), 0f, 1f);
        Img(root, "Heading", S("heading_missions"), new Vector2(300, 105), new Vector2(0, -62));

        // tabs (visual only - weekly not implemented in data yet)
        var dailyTab = Btn(root, "DailyTab", S("tab_active_blank"), new Vector2(165, 52), new Vector2(-85, -182));
        dailyTab.GetComponent<Image>().type = Image.Type.Sliced;
        Txt(dailyTab.transform, "Text", "DAILY", 17f, Color.white, new Vector2(160, 34), new Vector2(0, -9));
        var weeklyTab = Btn(root, "WeeklyTab", S("tab_inactive_blank"), new Vector2(165, 52), new Vector2(85, -182));
        weeklyTab.GetComponent<Image>().type = Image.Type.Sliced;
        Txt(weeklyTab.transform, "Text", "WEEKLY", 17f, new Color(1f, 1f, 1f, 0.6f), new Vector2(160, 34), new Vector2(0, -9));

        // card container (missionsContent)
        var content = NewRect(root, "Content", new Vector2(350, 490), new Vector2(0, -250));
        var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.spacing = 12f;
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;

        Txt(root, "Footer", "New missions each day", 15f, new Color(1f, 1f, 1f, 0.8f),
            new Vector2(320, 26), new Vector2(0, -792), FontStyles.Normal);

        // build the card template as a real prefab asset
        var card = BuildMissionCard();
        var prefab = PrefabUtility.SaveAsPrefabAsset(card, MISSION_CARD_PREFAB);
        Object.DestroyImmediate(card);

        if (ctrl != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(back.onClick, ctrl.OnClosePressed);

            var so = SO(ctrl);
            SetRef(so, "missionsContent", content);
            SetRef(so, "missionCardPrefab", prefab.GetComponent<MissionCardItem>());
            SetRef(so, "iconCoins", S("icon_collect_coins"));
            SetRef(so, "iconRun", S("icon_run_road"));
            SetRef(so, "iconWords", S("icon_word_blocks"));
            SetRef(so, "iconPowerup", S("icon_coin_magnet"));
            SetRef(so, "iconObstacles", S("icon_shield"));
            SetRef(so, "iconScore", S("icon_coin"));
            SetRef(so, "iconCompleteDaily", S("icon_check"));
            SetRef(so, "countdownText", null);
            SetRef(so, "completeAllPanel", null);
            SetRef(so, "newMissionTimer", null);
            SetRef(so, "allProgressFill", null);
            SetRef(so, "allProgressText", null);
            SetRef(so, "titleText", null);
            SetRef(so, "subtitleText", null);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        EditorUtility.SetDirty(root.gameObject);
    }

    static GameObject BuildMissionCard()
    {
        var card = new GameObject("MissionCardItem", typeof(RectTransform), typeof(Image), typeof(MissionCardItem));
        var crt = (RectTransform)card.transform;
        crt.sizeDelta = new Vector2(330, 105);
        var bg = card.GetComponent<Image>();
        bg.sprite = S("panel_row_blank");
        bg.type = Image.Type.Sliced;

        var icon = Img(card.transform, "Icon", S("icon_collect_coins"), new Vector2(70, 70), new Vector2(-128, -52));

        var title = Txt(card.transform, "Title", "Collect 100 coins", 17f, Color.white,
            new Vector2(150, 26), new Vector2(-22, -16), FontStyles.Bold, 0.5f, 1f, TextAlignmentOptions.Left);

        // progress bar: track + fill + count text
        var track = Img(card.transform, "ProgressTrack", S("progress_track"), new Vector2(155, 30), new Vector2(-38, -58));
        track.GetComponent<Image>().type = Image.Type.Sliced;
        var fill = Img(track.transform, "Fill", S("progress_fill_gold"), new Vector2(155, 30), new Vector2(0, -15));
        fill.GetComponent<Image>().type = Image.Type.Filled;
        fill.GetComponent<Image>().fillMethod = Image.FillMethod.Horizontal;
        fill.GetComponent<Image>().fillAmount = 0.65f;
        var progTxt = Txt(track.transform, "ProgressText", "0/100", 15f, Color.white,
            new Vector2(155, 26), new Vector2(0, -15));

        // right side: reward pill + CLAIM overlay button (shown when complete)
        var reward = Img(card.transform, "RewardPill", S("pill_blank"), new Vector2(98, 40), new Vector2(116, -32));
        reward.GetComponent<Image>().type = Image.Type.Sliced;
        Img(reward.transform, "Coin", S("icon_coin"), new Vector2(30, 30), new Vector2(-28, -20));
        var rewTxt = Txt(reward.transform, "RewardText", "x50", 15f, Color.white,
            new Vector2(55, 26), new Vector2(10, -22));

        var claim = Btn(card.transform, "ClaimButton", S("button_green_blank"), new Vector2(98, 46), new Vector2(116, -29));
        claim.GetComponent<Image>().type = Image.Type.Sliced;
        Txt(claim.transform, "Text", "CLAIM", 15f, Color.white, new Vector2(94, 30), new Vector2(0, -8));
        claim.gameObject.SetActive(false); // shown via completedCheckmark when ready

        // wire MissionCardItem private fields
        var item = card.GetComponent<MissionCardItem>();
        var so = SO(item);
        SetRef(so, "titleText", title);
        SetRef(so, "progressText", progTxt);
        SetRef(so, "progressFillImage", fill.GetComponent<Image>());
        SetRef(so, "rewardText", rewTxt);
        SetRef(so, "claimButton", claim);
        SetRef(so, "completedCheckmark", claim.gameObject);
        SetRef(so, "missionIcon", icon.GetComponent<Image>());
        so.ApplyModifiedPropertiesWithoutUndo();

        return card;
    }

    // ---------------------------------------------------------------
    // SHOP
    // ---------------------------------------------------------------
    static void BuildShop()
    {
        var root = FindCanvas("Canvas_Shop");
        if (root == null) { Debug.LogError("[Builder] Canvas_Shop missing"); return; }
        WipeChildren(root);

        var shopCtrl = root.GetComponent<ShopController>();
        var panel = root.gameObject.GetComponent<NingoShopPanel>();
        if (panel == null) panel = root.gameObject.AddComponent<NingoShopPanel>();

        Stretch(root, "Background", S("background_menu"));
        var back = Btn(root, "BackButton", S("button_back"), new Vector2(56, 56), new Vector2(45, -60), 0f, 1f);

        // currency pills top-right corner (right-anchored so they never collide with the heading)
        var coinsPill = BuildCurrencyPill(root, "CoinsPill", S("icon_coin"), 0, new Vector2(-140, -40), 1f);
        var gemsPill = BuildCurrencyPill(root, "GemsPill", S("icon_gem"), 1, new Vector2(-8, -40), 1f);

        Img(root, "Heading", S("heading_shop"), new Vector2(300, 105), new Vector2(0, -62));

        // tabs COINS | GEMS
        var coinsTab = Btn(root, "CoinsTab", S("tab_active_blank"), new Vector2(165, 50), new Vector2(-85, -180));
        coinsTab.GetComponent<Image>().type = Image.Type.Sliced;
        Img(coinsTab.transform, "CoinIcon", S("icon_coin"), new Vector2(26, 26), new Vector2(-45, -12));
        Txt(coinsTab.transform, "Text", "COINS", 16f, Color.white, new Vector2(110, 30), new Vector2(15, -10));

        var gemsTab = Btn(root, "GemsTab", S("tab_inactive_blank"), new Vector2(165, 50), new Vector2(85, -180));
        gemsTab.GetComponent<Image>().type = Image.Type.Sliced;
        Img(gemsTab.transform, "GemIcon", S("icon_gem"), new Vector2(26, 26), new Vector2(-45, -12));
        Txt(gemsTab.transform, "Text", "GEMS", 16f, new Color(1f, 1f, 1f, 0.6f), new Vector2(110, 30), new Vector2(15, -10));

        // coins content
        var coinsContent = NewRect(root, "CoinsContent", new Vector2(360, 560), new Vector2(0, -235));
        Txt(coinsContent, "Hint", "Choose a coin pack", 16f, new Color(1f, 1f, 1f, 0.85f),
            new Vector2(320, 24), new Vector2(0, 0), FontStyles.Normal);

        var packIcons = new[] { "icon_coin_pack_small", "icon_coin_pack_small", "icon_coin_pack_large", "icon_coin_pack_large" };
        var amounts = new[] { "500", "1,200", "3,000", "7,500" };
        for (int i = 0; i < 4; i++)
        {
            float cx = i % 2 == 0 ? -84f : 84f;
            float cy = i < 2 ? -140f : -375f;
            BuildCoinPack(coinsContent, i, packIcons[i], amounts[i], new Vector2(cx, cy), panel);
        }

        // gems content - same 2x2 pack grid as coins
        var gemsContent = NewRect(root, "GemsContent", new Vector2(360, 560), new Vector2(0, -235));
        Txt(gemsContent, "Hint", "Choose a gem pack", 16f, new Color(1f, 1f, 1f, 0.85f),
            new Vector2(320, 24), new Vector2(0, 0), FontStyles.Normal);

        var gemPackIcons = new[] { "icon_gem", "icon_gem", "icon_gem_cluster", "icon_gem_cluster" };
        var gemAmounts = new[] { "10", "30", "80", "200" };
        for (int i = 0; i < 4; i++)
        {
            float cx = i % 2 == 0 ? -84f : 84f;
            float cy = i < 2 ? -140f : -375f;
            BuildGemPack(gemsContent, i, gemPackIcons[i], gemAmounts[i], new Vector2(cx, cy), panel);
        }
        gemsContent.gameObject.SetActive(false);

        // wire panel
        panel.coinsTab = coinsTab;
        panel.gemsTab = gemsTab;
        panel.tabActiveSprite = S("tab_active_blank");
        panel.tabInactiveSprite = S("tab_inactive_blank");
        panel.coinsContent = coinsContent.gameObject;
        panel.gemsContent = gemsContent.gameObject;

        // ---- purchase flow modals (modal.png artwork) ----
        var modalSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/UI/ningo-ui/Home/compete-level/modal.png");
        var purchase = BuildShopModal(root, "Purchase_Modal", modalSprite);
        var success = BuildShopModal(root, "Purchase_Success_Modal", modalSprite);
        var failed = BuildShopModal(root, "Purchase_Failed_Modal", modalSprite);

        // confirm modal contents
        var pPanel = purchase.Find("Panel");
        Txt(pPanel, "Title", "PURCHASE", 22f, Color.white, new Vector2(300, 30), new Vector2(0, -35));
        var pIcon = Img(pPanel, "PackIcon", S("icon_coin_pack_small"), new Vector2(110, 90), new Vector2(0, -80));
        var pAmt = Txt(pPanel, "Amount", "500", 26f, new Color(1f, 0.84f, 0f), new Vector2(200, 32), new Vector2(0, -180));
        var pType = Txt(pPanel, "Type", "COINS", 16f, Color.white, new Vector2(200, 24), new Vector2(0, -214), FontStyles.Normal);
        var pPrice = Txt(pPanel, "Price", "$0.99", 20f, Color.white, new Vector2(150, 28), new Vector2(0, -245));
        var pBuy = Btn(pPanel, "BuyButton", S("button_green_blank"), new Vector2(200, 52), new Vector2(0, -285));
        pBuy.GetComponent<Image>().type = Image.Type.Sliced;
        Txt(pBuy.transform, "Text", "BUY NOW", 18f, Color.white, new Vector2(190, 30), new Vector2(0, -10));
        var pProc = Txt(pPanel, "Processing", "Processing...", 18f, new Color(1f, 0.84f, 0f), new Vector2(280, 30), new Vector2(0, -345));
        var pCancel = Btn(pPanel, "CancelButton", S("button_back"), new Vector2(56, 56), new Vector2(-125, -395));

        // success modal contents
        var sPanel = success.Find("Panel");
        Img(sPanel, "Check", S("icon_check"), new Vector2(80, 80), new Vector2(0, -50));
        Txt(sPanel, "Title", "PURCHASE SUCCESSFUL", 20f, Color.white, new Vector2(300, 30), new Vector2(0, -150));
        Txt(sPanel, "Body", "Your purchase has been added!", 15f, new Color(1f, 1f, 1f, 0.85f), new Vector2(280, 30), new Vector2(0, -185), FontStyles.Normal);
        var sOk = Btn(sPanel, "ContinueButton", S("button_green_blank"), new Vector2(200, 52), new Vector2(0, -240));
        sOk.GetComponent<Image>().type = Image.Type.Sliced;
        Txt(sOk.transform, "Text", "CONTINUE", 18f, Color.white, new Vector2(190, 30), new Vector2(0, -10));

        // failed modal contents
        var fPanel = failed.Find("Panel");
        var xTxt = Txt(fPanel, "X", "X", 60f, new Color(0.95f, 0.3f, 0.3f), new Vector2(80, 80), new Vector2(0, -50));
        Txt(fPanel, "Title", "PURCHASE FAILED", 20f, Color.white, new Vector2(300, 30), new Vector2(0, -150));
        Txt(fPanel, "Body", "Something went wrong.\nPlease try again.", 15f, new Color(1f, 1f, 1f, 0.85f), new Vector2(280, 50), new Vector2(0, -185), FontStyles.Normal);
        var fRetry = Btn(fPanel, "RetryButton", S("button_green_blank"), new Vector2(200, 52), new Vector2(0, -245));
        fRetry.GetComponent<Image>().type = Image.Type.Sliced;
        Txt(fRetry.transform, "Text", "TRY AGAIN", 18f, Color.white, new Vector2(190, 30), new Vector2(0, -10));
        var fClose = Btn(fPanel, "CloseButton", S("button_back"), new Vector2(56, 56), new Vector2(-125, -315));

        // wire purchase flow
        UnityEditor.Events.UnityEventTools.AddPersistentListener(pBuy.onClick, panel.ConfirmPurchase);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(pCancel.onClick, panel.CancelPurchase);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(sOk.onClick, panel.CloseSuccessModal);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(fRetry.onClick, panel.RetryPurchase);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(fClose.onClick, panel.CloseFailedModal);

        purchase.gameObject.SetActive(false);
        success.gameObject.SetActive(false);
        failed.gameObject.SetActive(false);
        purchase.SetAsLastSibling();
        success.SetAsLastSibling();
        failed.SetAsLastSibling();

        var pso = SO(panel);
        SetRef(pso, "purchaseModal", purchase.gameObject);
        SetRef(pso, "successModal", success.gameObject);
        SetRef(pso, "failedModal", failed.gameObject);
        SetRef(pso, "packIconImage", pIcon.GetComponent<Image>());
        SetRef(pso, "packAmountText", pAmt);
        SetRef(pso, "packTypeText", pType);
        SetRef(pso, "priceText", pPrice);
        SetRef(pso, "processingObject", pProc.gameObject);
        var cps = pso.FindProperty("coinPackSprites");
        if (cps != null)
        {
            cps.arraySize = 4;
            for (int i = 0; i < 4; i++) cps.GetArrayElementAtIndex(i).objectReferenceValue = S(i < 2 ? "icon_coin_pack_small" : "icon_coin_pack_large");
        }
        var gps = pso.FindProperty("gemPackSprites");
        if (gps != null)
        {
            gps.arraySize = 4;
            for (int i = 0; i < 4; i++) gps.GetArrayElementAtIndex(i).objectReferenceValue = S(i < 2 ? "icon_gem" : "icon_gem_cluster");
        }
        pso.ApplyModifiedPropertiesWithoutUndo();

        if (shopCtrl != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(back.onClick, shopCtrl.OnBackPressed);
            var so = SO(shopCtrl);
            SetRef(so, "canvasShop", root.gameObject);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        EditorUtility.SetDirty(root.gameObject);
    }

    /// <summary>Dim overlay + modal.png panel skeleton used by all shop modals.</summary>
    static Transform BuildShopModal(Transform root, string name, Sprite modalSprite)
    {
        var mrt = NewRect(root, name, Vector2.zero, Vector2.zero, 0.5f, 0.5f);
        mrt.anchorMin = Vector2.zero;
        mrt.anchorMax = Vector2.one;
        mrt.offsetMin = mrt.offsetMax = Vector2.zero;
        var dim = mrt.gameObject.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.75f);

        var panel = NewRect(mrt, "Panel", new Vector2(340, 430), Vector2.zero, 0.5f, 0.5f);
        var pimg = panel.gameObject.AddComponent<Image>();
        pimg.sprite = modalSprite;
        pimg.type = Image.Type.Sliced;
        return mrt;
    }

    static GameObject BuildCurrencyPill(Transform root, string name, Sprite icon, int currency, Vector2 pos,
        float anchorX = 0.5f)
    {
        var pill = Img(root, name, S("pill_blank"), new Vector2(125, 40), pos, anchorX, 1f, false, true);
        Img(pill.transform, "Icon", icon, new Vector2(30, 30), new Vector2(-40, -20));
        var txt = Txt(pill.transform, "Value", "0", 16f, Color.white, new Vector2(70, 30), new Vector2(6, -5));
        var cp = pill.AddComponent<CurrencyPill>();
        cp.currency = currency == 0 ? CurrencyPill.Currency.Coins : CurrencyPill.Currency.Gems;
        cp.valueText = txt;
        Btn(pill.transform, "Plus", S("button_plus"), new Vector2(34, 34), new Vector2(52, -3));
        return pill;
    }

    static void BuildCoinPack(Transform parent, int index, string iconName, string amount, Vector2 pos,
        NingoShopPanel panel)
    {
        var card = Img(parent, "Pack_" + index, S("panel_card_blank"), new Vector2(165, 215), pos, 0.5f, 1f, false, true);
        Img(card.transform, "CoinIcon", S("icon_coin"), new Vector2(28, 28), new Vector2(-42, -14));
        Txt(card.transform, "Amount", amount, 20f, Color.white, new Vector2(90, 30), new Vector2(24, -14));
        Img(card.transform, "PackImage", S(iconName), new Vector2(110, 90), new Vector2(0, -60));
        var offer = Btn(card.transform, "ViewOffer", S("button_green_blank"), new Vector2(135, 46), new Vector2(0, -160));
        offer.GetComponent<Image>().type = Image.Type.Sliced;
        Txt(offer.transform, "Text", "VIEW OFFER", 15f, Color.white, new Vector2(130, 30), new Vector2(0, -8));

        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(offer.onClick, panel.OnViewOffer, index);
    }

    static void BuildGemPack(Transform parent, int index, string iconName, string amount, Vector2 pos,
        NingoShopPanel panel)
    {
        var card = Img(parent, "GemPack_" + index, S("panel_card_blank"), new Vector2(165, 215), pos, 0.5f, 1f, false, true);
        Img(card.transform, "GemIcon", S("icon_gem"), new Vector2(28, 28), new Vector2(-42, -14));
        Txt(card.transform, "Amount", amount, 20f, Color.white, new Vector2(90, 30), new Vector2(24, -14));
        Img(card.transform, "PackImage", S(iconName), new Vector2(110, 90), new Vector2(0, -60));
        var offer = Btn(card.transform, "ViewOffer", S("button_green_blank"), new Vector2(135, 46), new Vector2(0, -160));
        offer.GetComponent<Image>().type = Image.Type.Sliced;
        Txt(offer.transform, "Text", "VIEW OFFER", 15f, Color.white, new Vector2(130, 30), new Vector2(0, -8));

        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(offer.onClick, panel.OnViewGemOffer, index);
    }

    // ---------------------------------------------------------------
    // OPTION ITEM PREFAB (SelectionModalController rows)
    // ---------------------------------------------------------------
    static OptionItem BuildOptionItemPrefab()
    {
        var go = new GameObject("OptionItem", typeof(RectTransform), typeof(Image), typeof(Button), typeof(OptionItem));
        var rt = (RectTransform)go.transform;
        rt.sizeDelta = new Vector2(280, 48);
        var img = go.GetComponent<Image>();
        img.sprite = S("panel_row_blank");
        img.type = Image.Type.Sliced;
        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;

        var label = Txt(go.transform, "Label", "Option", 17f, Color.white,
            new Vector2(230, 30), new Vector2(0, -9));
        var chk = Img(go.transform, "Selected", S("icon_check"), new Vector2(26, 26), new Vector2(115, -11));
        chk.SetActive(false);

        var item = go.GetComponent<OptionItem>();
        var so = SO(item);
        SetRef(so, "label", label);
        SetRef(so, "selectedIndicator", chk);
        so.ApplyModifiedPropertiesWithoutUndo();

        UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, item.OnClick);

        var saved = PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefab/OptionItem.prefab");
        Object.DestroyImmediate(go);
        return saved.GetComponent<OptionItem>();
    }

    // ---------------------------------------------------------------
    // POWER-UPS
    // ---------------------------------------------------------------
    static void BuildPowerUps()
    {
        var root = FindCanvas("Canvas_PowerUps");
        if (root == null)
        {
            // create the canvas - copy settings from Canvas_Shop
            var shop = FindCanvas("Canvas_Shop");
            var go = new GameObject("Canvas_PowerUps", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            var scaler = go.GetComponent<CanvasScaler>();
            if (shop != null)
            {
                var sc = shop.GetComponent<Canvas>();
                var ss = shop.GetComponent<CanvasScaler>();
                canvas.renderMode = sc.renderMode;
                canvas.sortingOrder = sc.sortingOrder;
                UnityEditorInternal.ComponentUtility.CopyComponent(ss);
                UnityEditorInternal.ComponentUtility.PasteComponentValues(scaler);
            }
            else
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(390, 844);
                scaler.matchWidthOrHeight = 1f;
            }
            go.SetActive(false);
            root = go.transform;
        }
        WipeChildren(root);

        var panel = root.gameObject.GetComponent<NingoPowerUpsPanel>();
        if (panel == null) panel = root.gameObject.AddComponent<NingoPowerUpsPanel>();

        Stretch(root, "Background", S("background_menu"));
        var back = Btn(root, "BackButton", S("button_back"), new Vector2(56, 56), new Vector2(45, -60), 0f, 1f);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(back.onClick, panel.OnBackPressed);

        BuildCurrencyPill(root, "CoinsPill", S("icon_coin"), 0, new Vector2(-8, -40), 1f);

        Img(root, "Heading", S("heading_powerups"), new Vector2(300, 105), new Vector2(0, -62));

        var names = new[] { "Coin Magnet", "Shield", "Double Coins" };
        var descs = new[] { "Attract nearby coins", "Block one collision", "Earn twice the coins" };
        var icons = new[] { "icon_coin_magnet", "icon_shield", "icon_double_coins" };
        var keys = new[] { "CoinMagnet", "Shield", "DoubleCoins" };
        var prices = new[] { 200, 300, 250 };

        panel.rows = new NingoPowerUpsPanel.Row[3];
        float y = -225f;
        for (int i = 0; i < 3; i++)
        {
            var row = Img(root, "Row_" + keys[i], S("panel_row_blank"), new Vector2(345, 120), new Vector2(0, y), 0.5f, 1f, false, true);
            Img(row.transform, "Icon", S(icons[i]), new Vector2(78, 78), new Vector2(-125, -21));
            Txt(row.transform, "Name", names[i], 20f, Color.white, new Vector2(170, 28), new Vector2(30, -16), FontStyles.Bold);
            Txt(row.transform, "Desc", descs[i], 13f, new Color(1f, 1f, 1f, 0.8f), new Vector2(170, 22), new Vector2(30, -46), FontStyles.Normal);
            var owned = Txt(row.transform, "Owned", "Owned: 0", 13f, Color.white,
                new Vector2(110, 24), new Vector2(-100, -78), FontStyles.Bold);
            var buy = Btn(row.transform, "BuyButton", S("button_green_blank"), new Vector2(115, 48), new Vector2(105, -62));
            buy.GetComponent<Image>().type = Image.Type.Sliced;
            Img(buy.transform, "Coin", S("icon_coin"), new Vector2(26, 26), new Vector2(-32, -11));
            Txt(buy.transform, "Price", prices[i].ToString(), 17f, Color.white, new Vector2(70, 30), new Vector2(12, -9));

            UnityEditor.Events.UnityEventTools.AddIntPersistentListener(buy.onClick, panel.Buy, i);

            panel.rows[i] = new NingoPowerUpsPanel.Row
            {
                key = keys[i],
                price = prices[i],
                ownedText = owned,
                buyButton = buy
            };
            y -= 140f;
        }

        Txt(root, "Footer", "Use boosts during your run", 15f, new Color(1f, 1f, 1f, 0.8f),
            new Vector2(320, 26), new Vector2(0, -792), FontStyles.Normal);

        // register with UImanager
        var ui = Object.FindObjectOfType<UImanager>(true);
        if (ui != null)
        {
            var so = SO(ui);
            SetRef(so, "canvasPowerUps", root.gameObject);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(ui);
        }

        EditorUtility.SetDirty(root.gameObject);
    }

    // ---------------------------------------------------------------
    // DAILY GIFT (Rewards)
    // ---------------------------------------------------------------
    static void BuildDailyGift()
    {
        var root = FindCanvas("Canvas_Daily_Gift");
        if (root == null) { Debug.LogError("[Builder] Canvas_Daily_Gift missing"); return; }
        WipeChildren(root);

        var ctrl = root.GetComponent<DailyGiftController>();

        Stretch(root, "Background", S("background_menu"));
        var back = Btn(root, "BackButton", S("button_back"), new Vector2(56, 56), new Vector2(45, -60), 0f, 1f);
        Img(root, "Heading", S("heading_rewards"), new Vector2(300, 105), new Vector2(0, -62));
        Txt(root, "Subtitle", "Come back daily for a gift.", 16f, new Color(1f, 1f, 1f, 0.85f),
            new Vector2(320, 24), new Vector2(0, -170), FontStyles.Normal);

        // day cards 1-6 (3x2 grid), day 7 wide bonus card
        var cardSprites = S("panel_card_blank");
        var types = new[] {
            DailyGiftController.RewardType.Coins, DailyGiftController.RewardType.Coins,
            DailyGiftController.RewardType.Gems, DailyGiftController.RewardType.Coins,
            DailyGiftController.RewardType.Gems, DailyGiftController.RewardType.Coins,
            DailyGiftController.RewardType.MysteryBox
        };
        var rewardIcons = new[] { "icon_coin", "icon_coin", "icon_gem", "icon_coin", "icon_gem", "icon_coin", "icon_chest_large" };
        var labels = new[] { "50 Coins", "100 Coins", "3 Gems", "250 Coins", "5 Gems", "500 Coins", "Bonus gift" };

        var so = ctrl != null ? SO(ctrl) : null;
        var dayCardsProp = so != null ? so.FindProperty("dayCards") : null;
        if (dayCardsProp != null) dayCardsProp.arraySize = 7;

        for (int i = 0; i < 7; i++)
        {
            bool wide = (i == 6);
            Vector2 size = wide ? new Vector2(345, 140) : new Vector2(105, 145);
            Vector2 pos;
            if (!wide)
            {
                int col = i % 3, rowIdx = i / 3;
                pos = new Vector2(-112f + col * 112f, -210f - rowIdx * 165f);
            }
            else pos = new Vector2(0, -540f);

            var card = Img(root, "Day" + (i + 1), cardSprites, size, pos, 0.5f, 1f, true, true);
            var cardBtn = card.AddComponent<Button>();
            cardBtn.targetGraphic = card.GetComponent<Image>();
            if (ctrl != null)
                UnityEditor.Events.UnityEventTools.AddIntPersistentListener(cardBtn.onClick, ctrl.OnDayCardPressed, i);

            var dayLabel = Txt(card.transform, "DayLabel", "DAY " + (i + 1), wide ? 20f : 14f, Color.white,
                new Vector2(size.x - 10, 24), new Vector2(0, -8));

            var iconSize = wide ? new Vector2(110, 80) : new Vector2(56, 56);
            var rewardImg = Img(card.transform, "RewardIcon", S(rewardIcons[i]), iconSize,
                new Vector2(wide ? 55 : 0, wide ? -45 : -40));

            if (wide)
                Txt(card.transform, "BonusLabel", "Bonus gift", 14f, new Color(1f, 0.84f, 0f),
                    new Vector2(120, 22), new Vector2(-105, -75));

            // bottom amount label on a small pill
            var amtPill = Img(card.transform, "AmountPill", S("panel_reward_amount_blank"),
                wide ? new Vector2(130, 34) : new Vector2(92, 32),
                new Vector2(wide ? -95 : 0, wide ? -100 : -105), 0.5f, 1f, false, true);
            var rewTxt = Txt(amtPill.transform, "Text", labels[i], 13f, Color.white,
                new Vector2(120, 26), new Vector2(0, -17));

            // claimed checkmark badge
            var chk = Img(card.transform, "Checkmark", S("icon_check"), new Vector2(34, 34),
                new Vector2(size.x / 2 - 14, 6));
            chk.SetActive(false);

            if (dayCardsProp != null)
            {
                var el = dayCardsProp.GetArrayElementAtIndex(i);
                el.FindPropertyRelative("cardObject").objectReferenceValue = card;
                el.FindPropertyRelative("backgroundImage").objectReferenceValue = card.GetComponent<Image>();
                el.FindPropertyRelative("rewardImage").objectReferenceValue = rewardImg.GetComponent<Image>();
                el.FindPropertyRelative("rewardText").objectReferenceValue = rewTxt;
                el.FindPropertyRelative("dayLabel").objectReferenceValue = dayLabel;
                el.FindPropertyRelative("checkmark").objectReferenceValue = chk;
                el.FindPropertyRelative("plusIcon").objectReferenceValue = null;
                el.FindPropertyRelative("rewardType").enumValueIndex = (int)types[i];
                el.FindPropertyRelative("customRewardText").stringValue = labels[i];
                el.FindPropertyRelative("customDayLabel").stringValue = "DAY " + (i + 1);
            }
        }

        // CLAIM REWARD button
        var claim = Btn(root, "ClaimButton", S("button_green_blank"), new Vector2(330, 62), new Vector2(0, -715));
        claim.GetComponent<Image>().type = Image.Type.Sliced;
        var claimTxt = Txt(claim.transform, "Text", "CLAIM REWARD", 20f, Color.white,
            new Vector2(320, 40), new Vector2(0, -11));
        if (ctrl != null)
            UnityEditor.Events.UnityEventTools.AddPersistentListener(claim.onClick, ctrl.ClaimReward);

        if (so != null)
        {
            SetRef(so, "claimButtonText", claimTxt);
            SetRef(so, "titleText", null);
            SetRef(so, "subtitleText", null);
            SetRef(so, "currentDayText", null);
            SetRef(so, "rewardAmountText", null);
            SetRef(so, "rewardIconImage", null);
            SetRef(so, "resetTimerText", null);
            SetRef(so, "cardNormalSprite", S("panel_card_blank"));
            SetRef(so, "cardTodaySprite", S("card_active_blank"));
            SetRef(so, "cardClaimedSprite", S("card_claimed_blank"));
            // reward amounts matching the new design: 50c,100c,3g,250c,5g,500c,bonus
            var dr = so.FindProperty("dailyRewards");
            if (dr != null)
            {
                dr.arraySize = 7;
                int[] vals = { 50, 100, 3, 250, 5, 500, 0 };
                for (int i = 0; i < 7; i++) dr.GetArrayElementAtIndex(i).intValue = vals[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        if (ctrl != null)
            UnityEditor.Events.UnityEventTools.AddPersistentListener(back.onClick, ctrl.OnClosePressed);

        EditorUtility.SetDirty(root.gameObject);
    }
}

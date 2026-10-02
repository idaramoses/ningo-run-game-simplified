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
        Img(langRow.transform, "Icon", S("icon_language"), new Vector2(38, 38), new Vector2(-145, -32));
        Txt(langRow.transform, "Label", "Learning language", 18f, Color.white,
            new Vector2(200, 30), new Vector2(-30, -17), FontStyles.Bold);
        var pill = Btn(langRow.transform, "LangPill", S("pill_blank"), new Vector2(125, 44), new Vector2(105, -10));
        var langTxt = Txt(pill.transform, "Text", "Yoruba", 15f, new Color(1f, 0.84f, 0f),
            new Vector2(90, 30), new Vector2(-8, -22));
        Img(pill.transform, "Arrow", S("icon_arrow_right"), new Vector2(18, 18), new Vector2(45, -13));
        var pillComp = langRow.AddComponent<NingoLanguagePill>();
        pillComp.text = langTxt;

        // ---- guest / sign in row ----
        y -= step;
        var guestRow = Img(root, "GuestRow", S("panel_row_blank"), new Vector2(350, 64), new Vector2(0, y), 0.5f, 1f, false, true);
        Img(guestRow.transform, "Icon", S("icon_guest"), new Vector2(38, 38), new Vector2(-145, -32));
        Txt(guestRow.transform, "Label", "Playing as Guest", 17f, Color.white,
            new Vector2(180, 30), new Vector2(-40, -17), FontStyles.Bold);
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

        if (ctrl != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(back.onClick, ctrl.OnBackPressed);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(pill.onClick, ctrl.OnLanguagePressed);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(sup.onClick, ctrl.OnSupportPressed);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(privBtn.onClick, ctrl.OnPrivacyPressed);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(termBtn.onClick, ctrl.OnTermsPressed);

            // sign-in: open the user-info/login canvas and hide settings
            var ui = Object.FindObjectOfType<UImanager>(true);
            if (ui != null && ui.canvasUserInfo != null)
            {
                UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(
                    signIn.onClick, ui.canvasUserInfo.SetActive, true);
                UnityEditor.Events.UnityEventTools.AddPersistentListener(signIn.onClick, ctrl.OnBackPressed);
            }
        }

        EditorUtility.SetDirty(root.gameObject);
    }

    static void BuildSettingsRow(Transform root, string name, Sprite icon, string label, bool toggle,
        float y, NingoPillToggle.Kind kind)
    {
        var row = Img(root, name, S("panel_row_blank"), new Vector2(350, 64), new Vector2(0, y), 0.5f, 1f, false, true);
        Img(row.transform, "Icon", icon, new Vector2(38, 38), new Vector2(-145, -32));
        Txt(row.transform, "Label", label, 18f, Color.white, new Vector2(200, 30), new Vector2(-45, -17), FontStyles.Bold);

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

        // currency pills top-right
        BuildCurrencyPill(root, "CoinsPill", S("icon_coin"), 0, new Vector2(-95, -62));
        BuildCurrencyPill(root, "GemsPill", S("icon_gem"), 1, new Vector2(55, -62));

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

        // gems content placeholder
        var gemsContent = NewRect(root, "GemsContent", new Vector2(360, 560), new Vector2(0, -235));
        Txt(gemsContent, "ComingSoon", "Gem packs coming soon", 18f, new Color(1f, 1f, 1f, 0.6f),
            new Vector2(320, 30), new Vector2(0, -200), FontStyles.Normal);
        gemsContent.gameObject.SetActive(false);

        // wire panel
        panel.coinsTab = coinsTab;
        panel.gemsTab = gemsTab;
        panel.tabActiveSprite = S("tab_active_blank");
        panel.tabInactiveSprite = S("tab_inactive_blank");
        panel.coinsContent = coinsContent.gameObject;
        panel.gemsContent = gemsContent.gameObject;

        if (shopCtrl != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(back.onClick, shopCtrl.OnBackPressed);
            var so = SO(shopCtrl);
            SetRef(so, "canvasShop", root.gameObject);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        EditorUtility.SetDirty(root.gameObject);
    }

    static void BuildCurrencyPill(Transform root, string name, Sprite icon, int currency, Vector2 pos)
    {
        var pill = Img(root, name, S("pill_blank"), new Vector2(125, 40), pos, 0.5f, 1f, false, true);
        Img(pill.transform, "Icon", icon, new Vector2(30, 30), new Vector2(-40, -20));
        var txt = Txt(pill.transform, "Value", "0", 16f, Color.white, new Vector2(70, 26), new Vector2(6, -22));
        var cp = pill.AddComponent<CurrencyPill>();
        cp.currency = currency == 0 ? CurrencyPill.Currency.Coins : CurrencyPill.Currency.Gems;
        cp.valueText = txt;
        Btn(pill.transform, "Plus", S("button_plus"), new Vector2(34, 34), new Vector2(52, -3));
    }

    static void BuildCoinPack(Transform parent, int index, string iconName, string amount, Vector2 pos,
        NingoShopPanel panel)
    {
        var card = Img(parent, "Pack_" + index, S("panel_card_blank"), new Vector2(165, 215), pos, 0.5f, 1f, false, true);
        Img(card.transform, "CoinIcon", S("icon_coin"), new Vector2(28, 28), new Vector2(-42, -14));
        Txt(card.transform, "Amount", amount, 20f, Color.white, new Vector2(110, 30), new Vector2(14, -14));
        Img(card.transform, "PackImage", S(iconName), new Vector2(110, 90), new Vector2(0, -60));
        var offer = Btn(card.transform, "ViewOffer", S("button_green_blank"), new Vector2(135, 46), new Vector2(0, -160));
        offer.GetComponent<Image>().type = Image.Type.Sliced;
        Txt(offer.transform, "Text", "VIEW OFFER", 15f, Color.white, new Vector2(130, 30), new Vector2(0, -8));

        UnityEditor.Events.UnityEventTools.AddIntPersistentListener(offer.onClick, panel.OnViewOffer, index);
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

        BuildCurrencyPill(root, "CoinsPill", S("icon_coin"), 0, new Vector2(105, -62));

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
                new Vector2(110, 24), new Vector2(-100, -88), FontStyles.Bold);
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
            new Vector2(320, 24), new Vector2(0, -148), FontStyles.Normal);

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
                pos = new Vector2(-112f + col * 112f, -185f - rowIdx * 165f);
            }
            else pos = new Vector2(0, -515f);

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
        var claim = Btn(root, "ClaimButton", S("button_green_blank"), new Vector2(330, 62), new Vector2(0, -700));
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

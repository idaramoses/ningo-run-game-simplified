using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoPanelController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The Card object inside InfoPanel where content will be generated")]
    public RectTransform contentParent;
    [Tooltip("The CloseIcon button already in the panel")]
    public Button closeIconButton;
    
    [Header("Styling")]
    public TMP_FontAsset fontAsset;
    public Color headerColor = new Color(1f, 0.84f, 0f);
    public Color bodyColor = Color.white;
    public Color subtitleColor = new Color(0.85f, 0.85f, 0.85f);
    public Color buttonTextColor = Color.white;
    public Color buttonBgColor = new Color(0.2f, 0.6f, 0.2f);
    
    [Header("Spacing")]
    public float sectionSpacing = 30f;
    public float itemSpacing = 10f;
    public float headerFontSize = 28f;
    public float bodyFontSize = 20f;
    public float titleFontSize = 36f;
    public float subtitleFontSize = 22f;
    public float sidePadding = 40f;
    
    private ScrollRect scrollRect;
    private bool contentGenerated = false;
    
    private void Start()
    {
        if (closeIconButton != null)
            closeIconButton.onClick.AddListener(CloseInfo);
        
        GenerateContent();
    }
    
    private void OnEnable()
    {
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
    }
    
    private void GenerateContent()
    {
        if (contentGenerated || contentParent == null) return;
        contentGenerated = true;
        
        // Create ScrollView inside the Card
        GameObject scrollObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
        scrollObj.transform.SetParent(contentParent, false);
        RectTransform scrollRT = scrollObj.GetComponent<RectTransform>();
        scrollRT.anchorMin = Vector2.zero;
        scrollRT.anchorMax = Vector2.one;
        scrollRT.offsetMin = new Vector2(0, 0);
        scrollRT.offsetMax = new Vector2(0, -80f);
        
        scrollRect = scrollObj.GetComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.movementType = ScrollRect.MovementType.Elastic;
        scrollRect.elasticity = 0.1f;
        scrollRect.scrollSensitivity = 30f;
        
        Image scrollBg = scrollObj.GetComponent<Image>();
        scrollBg.color = new Color(0, 0, 0, 0);
        
        // Viewport
        GameObject viewportObj = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewportObj.transform.SetParent(scrollObj.transform, false);
        RectTransform viewportRT = viewportObj.GetComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.offsetMin = Vector2.zero;
        viewportRT.offsetMax = Vector2.zero;
        
        Image viewportImg = viewportObj.GetComponent<Image>();
        viewportImg.color = new Color(1, 1, 1, 0.01f);
        viewportObj.GetComponent<Mask>().showMaskGraphic = false;
        
        scrollRect.viewport = viewportRT;
        
        // Content container
        GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObj.transform.SetParent(viewportObj.transform, false);
        RectTransform contentRT = contentObj.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.offsetMin = new Vector2(sidePadding, 0);
        contentRT.offsetMax = new Vector2(-sidePadding, 0);
        
        VerticalLayoutGroup vlg = contentObj.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = itemSpacing;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.padding = new RectOffset(0, 0, 20, 40);
        
        ContentSizeFitter csf = contentObj.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        scrollRect.content = contentRT;
        
        // --- BUILD ALL SECTIONS ---
        
        // Welcome
        CreateText(contentObj.transform, "Welcome to Ningo Run!", titleFontSize, headerColor, FontStyles.Bold, TextAlignmentOptions.Center);
        CreateText(contentObj.transform, "Run and Learn African Languages!", subtitleFontSize, subtitleColor, FontStyles.Normal, TextAlignmentOptions.Center);
        CreateSpacer(contentObj.transform, sectionSpacing);
        
        // What is Ningo Run?
        CreateText(contentObj.transform, "What is Ningo Run?", headerFontSize, headerColor, FontStyles.Bold);
        CreateText(contentObj.transform,
            "Ningo Run is part of the <b>Ningo Africa</b> family ,an AI-powered African language learning platform by <b>Maradi Studio</b>. Run through vibrant African-themed environments, collect letters, and build words in languages like <b>Yoruba, Igbo, Hausa, Ibibio,French</b>, and more!",
            bodyFontSize, bodyColor);
        CreateSpacer(contentObj.transform, sectionSpacing);
        
        // How to Play
        CreateText(contentObj.transform, "How to Play", headerFontSize, headerColor, FontStyles.Bold);
        CreateText(contentObj.transform,
            "\u2022 <b>Swipe left/right</b> to switch lanes\n" +
            "\u2022 <b>Swipe up</b> to jump over obstacles\n" +
            "\u2022 <b>Swipe down</b> to slide under barriers\n" +
            "\u2022 <b>Collect letters</b> in the correct order to complete words\n" +
            "\u2022 Avoid obstacles and enemies to stay alive!",
            bodyFontSize, bodyColor);
        CreateSpacer(contentObj.transform, sectionSpacing);
        
        // Your Goal
        CreateText(contentObj.transform, "Your Goal", headerFontSize, headerColor, FontStyles.Bold);
        CreateText(contentObj.transform,
            "Run as far as you can in Lagos! Avoid incoming vehicles, dodge obstacles, and collect coins to unlock amazing new runners.",
            bodyFontSize, bodyColor);
        CreateSpacer(contentObj.transform, sectionSpacing);
        
        // What You'll Learn
        CreateText(contentObj.transform, "What You'll Learn", headerFontSize, headerColor, FontStyles.Bold);
        CreateText(contentObj.transform,
            "\u2022 Explore the vibrant streets of Lagos while testing your reflexes\n" +
            "\u2022 Collect and manage coins to unlock diverse character runners\n" +
            "\u2022 Beat your high score in this exciting endless run adventure!",
            bodyFontSize, bodyColor);
        CreateSpacer(contentObj.transform, sectionSpacing);
        
        // Runners
        CreateText(contentObj.transform, "Runners", headerFontSize, headerColor, FontStyles.Bold);
        CreateText(contentObj.transform,
            "Unlock and choose from different character runners, each representing the rich diversity of Africa.",
            bodyFontSize, bodyColor);
        CreateSpacer(contentObj.transform, sectionSpacing);
        
        // Tips
        CreateText(contentObj.transform, "Tips", headerFontSize, headerColor, FontStyles.Bold);
        CreateText(contentObj.transform,
            "\u2022 Keep an eye on oncoming traffic and switch lanes quickly!\n" +
            "\u2022 Coins let you unlock cool new runners\n" +
            "\u2022 Complete daily missions for bonus rewards",
            bodyFontSize, bodyColor);
        CreateSpacer(contentObj.transform, sectionSpacing);
        
        // Ningo Africa
        CreateText(contentObj.transform, "Ningo Africa", headerFontSize, headerColor, FontStyles.Bold);
        CreateText(contentObj.transform,
            "Ningo Run is one of the fun ways to learn on the <b>Ningo Africa</b> platform. Explore more at <b>ningoafrica.app</b>:\n\n" +
            "\u2022 <b>AI-Powered Lessons</b> \u2014 Interactive lessons with native audio\n" +
            "\u2022 <b>AI Language Tutor</b> \u2014 Personal tutor for pronunciation, quizzes & translation\n" +
            "\u2022 <b>Language Community</b> \u2014 Connect with learners and native speakers\n" +
            "\u2022 <b>More Games</b> \u2014 Word Scramble, daily challenges & language quests",
            bodyFontSize, bodyColor);
        CreateSpacer(contentObj.transform, 15f);
        
        // Visit Website Button
        CreateButton(contentObj.transform, "Visit ningoafrica.app", () =>
        {
            Application.OpenURL("https://ningoafrica.app");
            Debug.Log("[InfoPanelController] Opening Ningo Africa website");
        });
        CreateSpacer(contentObj.transform, sectionSpacing);
        
        // Credits
        CreateText(contentObj.transform,
            "Developed by <b>Maradi Studio</b>\nPlatform: <b>Ningo Africa</b>\n<b>Free, Fun & Effective</b>",
            bodyFontSize - 2f, subtitleColor, FontStyles.Normal, TextAlignmentOptions.Center);
        
        Debug.Log("[InfoPanelController] Content generated successfully");
    }
    
    private TMP_Text CreateText(Transform parent, string text, float fontSize, Color color,
        FontStyles style = FontStyles.Normal, TextAlignmentOptions alignment = TextAlignmentOptions.Left)
    {
        GameObject obj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        
        TextMeshProUGUI tmp = obj.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.fontStyle = style;
        tmp.alignment = alignment;
        tmp.richText = true;
        tmp.enableWordWrapping = true;
        tmp.overflowMode = TextOverflowModes.Overflow;
        
        if (fontAsset != null)
            tmp.font = fontAsset;
        
        // Let layout group handle sizing
        LayoutElement le = obj.AddComponent<LayoutElement>();
        le.minHeight = fontSize + 10f;
        
        return tmp;
    }
    
    private void CreateSpacer(Transform parent, float height)
    {
        GameObject spacer = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
        spacer.transform.SetParent(parent, false);
        spacer.GetComponent<LayoutElement>().preferredHeight = height;
    }
    
    private void CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject("Button", typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);
        
        Image btnImg = btnObj.GetComponent<Image>();
        btnImg.color = buttonBgColor;
        
        // Round the button slightly
        btnImg.type = Image.Type.Sliced;
        
        LayoutElement btnLE = btnObj.AddComponent<LayoutElement>();
        btnLE.preferredHeight = 60f;
        btnLE.preferredWidth = 400f;
        
        // Button text
        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;
        
        TextMeshProUGUI btnText = textObj.GetComponent<TextMeshProUGUI>();
        btnText.text = label;
        btnText.fontSize = bodyFontSize;
        btnText.color = buttonTextColor;
        btnText.fontStyle = FontStyles.Bold;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.enableWordWrapping = false;
        
        if (fontAsset != null)
            btnText.font = fontAsset;
        
        Button btn = btnObj.GetComponent<Button>();
        btn.onClick.AddListener(onClick);
    }
    
    public void OpenInfo()
    {
        gameObject.SetActive(true);
        Debug.Log("[InfoPanelController] Info panel opened");
    }
    
    public void CloseInfo()
    {
        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.HideInfo();
        }
        else
        {
            gameObject.SetActive(false);
            Debug.Log("[InfoPanelController] Info panel closed (fallback)");
        }
    }
}

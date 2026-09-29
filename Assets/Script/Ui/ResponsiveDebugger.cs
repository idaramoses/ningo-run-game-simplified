using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResponsiveDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool showOnScreenInfo = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;
    
    [Header("References")]
    public ScreenBreakpoints breakpoints;
    
    private TMP_Text debugText;
    private GameObject debugPanel;
    private bool isVisible = true;
    
    void Awake()
    {
        if (breakpoints == null)
        {
            breakpoints = ScriptableObject.CreateInstance<ScreenBreakpoints>();
        }
        
        if (showOnScreenInfo)
        {
            CreateDebugUI();
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleDebugUI();
        }
        
        if (showDebugInfo && debugText != null && isVisible)
        {
            UpdateDebugInfo();
        }
    }
    
    void CreateDebugUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        
        debugPanel = new GameObject("ResponsiveDebugPanel");
        debugPanel.transform.SetParent(canvas.transform, false);
        
        RectTransform panelRect = debugPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 1);
        panelRect.anchorMax = new Vector2(0, 1);
        panelRect.pivot = new Vector2(0, 1);
        panelRect.anchoredPosition = new Vector2(10, -10);
        panelRect.sizeDelta = new Vector2(400, 200);
        
        Image panelImage = debugPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        
        GameObject textObj = new GameObject("DebugText");
        textObj.transform.SetParent(debugPanel.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 10);
        textRect.offsetMax = new Vector2(-10, -10);
        
        debugText = textObj.AddComponent<TextMeshProUGUI>();
        debugText.fontSize = 14;
        debugText.color = Color.white;
        debugText.alignment = TextAlignmentOptions.TopLeft;
    }
    
    void UpdateDebugInfo()
    {
        ScreenBreakpoints.DeviceType deviceType = breakpoints.GetDeviceType();
        ScreenBreakpoints.AspectRatioType aspectRatio = breakpoints.GetAspectRatioType();
        bool isPortrait = breakpoints.IsPortrait();
        
        float dpi = Screen.dpi > 0 ? Screen.dpi : 160f;
        float diagonalInches = Mathf.Sqrt(Mathf.Pow(Screen.width / dpi, 2) + Mathf.Pow(Screen.height / dpi, 2));
        
        string info = $"<b>RESPONSIVE DEBUG INFO</b>\n" +
                     $"Press {toggleKey} to toggle\n\n" +
                     $"<b>Screen:</b> {Screen.width}×{Screen.height}\n" +
                     $"<b>Device:</b> {deviceType}\n" +
                     $"<b>Aspect:</b> {aspectRatio} ({(float)Screen.width / Screen.height:F2})\n" +
                     $"<b>Orientation:</b> {(isPortrait ? "Portrait" : "Landscape")}\n" +
                     $"<b>DPI:</b> {dpi:F0}\n" +
                     $"<b>Diagonal:</b> {diagonalInches:F1}\"\n" +
                     $"<b>Safe Area:</b> {Screen.safeArea.width:F0}×{Screen.safeArea.height:F0}";
        
        debugText.text = info;
    }
    
    void ToggleDebugUI()
    {
        isVisible = !isVisible;
        if (debugPanel != null)
        {
            debugPanel.SetActive(isVisible);
        }
    }
    
    public void LogResponsiveComponents()
    {
        Debug.Log("=== RESPONSIVE COMPONENTS REPORT ===");
        
        var canvasManagers = FindObjectsOfType<CanvasResponsiveManager>(true);
        Debug.Log($"CanvasResponsiveManager: {canvasManagers.Length} found");
        
        var textAdjusters = FindObjectsOfType<ResponsiveTextAdjuster>(true);
        Debug.Log($"ResponsiveTextAdjuster: {textAdjusters.Length} found");
        
        var buttonScalers = FindObjectsOfType<ResponsiveButtonScaler>(true);
        Debug.Log($"ResponsiveButtonScaler: {buttonScalers.Length} found");
        
        var imageScalers = FindObjectsOfType<ResponsiveImageScaler>(true);
        Debug.Log($"ResponsiveImageScaler: {imageScalers.Length} found");
        
        var safeAreaHandlers = FindObjectsOfType<SafeAreaHandler>(true);
        Debug.Log($"SafeAreaHandler: {safeAreaHandlers.Length} found");
        
        var layoutManagers = FindObjectsOfType<UILayoutManager>(true);
        Debug.Log($"UILayoutManager: {layoutManagers.Length} found");
        
        var orientationHandlers = FindObjectsOfType<OrientationHandler>(true);
        Debug.Log($"OrientationHandler: {orientationHandlers.Length} found");
        
        Debug.Log("=== END REPORT ===");
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Adjusts text properties (size, width, height) based on screen size breakpoints
/// Works with both Unity Text and TextMeshPro
/// </summary>
public class ResponsiveTextAdjuster : MonoBehaviour
{
    [Header("Breakpoints Reference")]
    public ScreenBreakpoints breakpoints;
    
    [Header("Text Component (Auto-detected if empty)")]
    public Text unityText;
    public TMP_Text tmpText;
    
    [Header("Font Size Settings")]
    public bool adjustFontSize = true;
    public FontSizeBreakpoints fontSizes;
    
    [Header("Width Settings")]
    public bool adjustWidth = false;
    public WidthBreakpoints widths;
    
    [Header("Height Settings")]
    public bool adjustHeight = false;
    public HeightBreakpoints heights;
    
    [Header("Advanced")]
    public bool adjustLineSpacing = false;
    public LineSpacingBreakpoints lineSpacing;
    
    private RectTransform rectTransform;
    private ScreenBreakpoints.DeviceType currentDeviceType;
    
    [System.Serializable]
    public class FontSizeBreakpoints
    {
        [Header("Font Sizes per Device Type")]
        public float smallPhone = 24f;
        public float mediumPhone = 32f;
        public float largePhone = 36f;
        public float smallTablet = 48f;
        public float largeTablet = 64f;
        public float desktop = 72f;
    }
    
    [System.Serializable]
    public class WidthBreakpoints
    {
        [Header("Width per Device Type")]
        public float smallPhone = 300f;
        public float mediumPhone = 400f;
        public float largePhone = 500f;
        public float smallTablet = 600f;
        public float largeTablet = 800f;
        public float desktop = 1000f;
    }
    
    [System.Serializable]
    public class HeightBreakpoints
    {
        [Header("Height per Device Type")]
        public float smallPhone = 50f;
        public float mediumPhone = 60f;
        public float largePhone = 70f;
        public float smallTablet = 80f;
        public float largeTablet = 100f;
        public float desktop = 120f;
    }
    
    [System.Serializable]
    public class LineSpacingBreakpoints
    {
        [Header("Line Spacing per Device Type")]
        public float smallPhone = 1.0f;
        public float mediumPhone = 1.1f;
        public float largePhone = 1.2f;
        public float smallTablet = 1.3f;
        public float largeTablet = 1.4f;
        public float desktop = 1.5f;
    }
    
    void Awake()
    {
        // Auto-detect components
        if (unityText == null)
            unityText = GetComponent<Text>();
        
        if (tmpText == null)
            tmpText = GetComponent<TMP_Text>();
        
        rectTransform = GetComponent<RectTransform>();
        
        // Create default breakpoints if none assigned
        if (breakpoints == null)
        {
            breakpoints = ScriptableObject.CreateInstance<ScreenBreakpoints>();
            Debug.LogWarning("[ResponsiveTextAdjuster] No ScreenBreakpoints assigned. Using defaults.");
        }
        
        // Initialize default values if not set
        if (fontSizes == null) fontSizes = new FontSizeBreakpoints();
        if (widths == null) widths = new WidthBreakpoints();
        if (heights == null) heights = new HeightBreakpoints();
        if (lineSpacing == null) lineSpacing = new LineSpacingBreakpoints();
    }
    
    void Start()
    {
        ApplyResponsiveSettings();
    }
    
    void Update()
    {
        ScreenBreakpoints.DeviceType newDeviceType = breakpoints.GetDeviceType();
        
        if (newDeviceType != currentDeviceType)
        {
            currentDeviceType = newDeviceType;
            ApplyResponsiveSettings();
        }
    }
    
    void ApplyResponsiveSettings()
    {
        currentDeviceType = breakpoints.GetDeviceType();
        
        // Apply font size
        if (adjustFontSize)
        {
            float fontSize = GetFontSizeForDevice(currentDeviceType);
            SetFontSize(fontSize);
        }
        
        // Apply width
        if (adjustWidth && rectTransform != null)
        {
            float width = GetWidthForDevice(currentDeviceType);
            Vector2 sizeDelta = rectTransform.sizeDelta;
            sizeDelta.x = width;
            rectTransform.sizeDelta = sizeDelta;
        }
        
        // Apply height
        if (adjustHeight && rectTransform != null)
        {
            float height = GetHeightForDevice(currentDeviceType);
            Vector2 sizeDelta = rectTransform.sizeDelta;
            sizeDelta.y = height;
            rectTransform.sizeDelta = sizeDelta;
        }
        
        // Apply line spacing
        if (adjustLineSpacing)
        {
            float spacing = GetLineSpacingForDevice(currentDeviceType);
            SetLineSpacing(spacing);
        }
        
        Debug.Log($"[ResponsiveTextAdjuster] Applied settings for {currentDeviceType}: Font={GetFontSizeForDevice(currentDeviceType)}, Width={GetWidthForDevice(currentDeviceType)}, Height={GetHeightForDevice(currentDeviceType)}");
    }
    
    float GetFontSizeForDevice(ScreenBreakpoints.DeviceType deviceType)
    {
        switch (deviceType)
        {
            case ScreenBreakpoints.DeviceType.SmallPhone:
                return fontSizes.smallPhone;
            case ScreenBreakpoints.DeviceType.MediumPhone:
                return fontSizes.mediumPhone;
            case ScreenBreakpoints.DeviceType.LargePhone:
                return fontSizes.largePhone;
            case ScreenBreakpoints.DeviceType.SmallTablet:
                return fontSizes.smallTablet;
            case ScreenBreakpoints.DeviceType.LargeTablet:
                return fontSizes.largeTablet;
            case ScreenBreakpoints.DeviceType.Desktop:
                return fontSizes.desktop;
            default:
                return fontSizes.mediumPhone;
        }
    }
    
    float GetWidthForDevice(ScreenBreakpoints.DeviceType deviceType)
    {
        switch (deviceType)
        {
            case ScreenBreakpoints.DeviceType.SmallPhone:
                return widths.smallPhone;
            case ScreenBreakpoints.DeviceType.MediumPhone:
                return widths.mediumPhone;
            case ScreenBreakpoints.DeviceType.LargePhone:
                return widths.largePhone;
            case ScreenBreakpoints.DeviceType.SmallTablet:
                return widths.smallTablet;
            case ScreenBreakpoints.DeviceType.LargeTablet:
                return widths.largeTablet;
            case ScreenBreakpoints.DeviceType.Desktop:
                return widths.desktop;
            default:
                return widths.mediumPhone;
        }
    }
    
    float GetHeightForDevice(ScreenBreakpoints.DeviceType deviceType)
    {
        switch (deviceType)
        {
            case ScreenBreakpoints.DeviceType.SmallPhone:
                return heights.smallPhone;
            case ScreenBreakpoints.DeviceType.MediumPhone:
                return heights.mediumPhone;
            case ScreenBreakpoints.DeviceType.LargePhone:
                return heights.largePhone;
            case ScreenBreakpoints.DeviceType.SmallTablet:
                return heights.smallTablet;
            case ScreenBreakpoints.DeviceType.LargeTablet:
                return heights.largeTablet;
            case ScreenBreakpoints.DeviceType.Desktop:
                return heights.desktop;
            default:
                return heights.mediumPhone;
        }
    }
    
    float GetLineSpacingForDevice(ScreenBreakpoints.DeviceType deviceType)
    {
        switch (deviceType)
        {
            case ScreenBreakpoints.DeviceType.SmallPhone:
                return lineSpacing.smallPhone;
            case ScreenBreakpoints.DeviceType.MediumPhone:
                return lineSpacing.mediumPhone;
            case ScreenBreakpoints.DeviceType.LargePhone:
                return lineSpacing.largePhone;
            case ScreenBreakpoints.DeviceType.SmallTablet:
                return lineSpacing.smallTablet;
            case ScreenBreakpoints.DeviceType.LargeTablet:
                return lineSpacing.largeTablet;
            case ScreenBreakpoints.DeviceType.Desktop:
                return lineSpacing.desktop;
            default:
                return lineSpacing.mediumPhone;
        }
    }
    
    void SetFontSize(float size)
    {
        if (unityText != null)
        {
            unityText.fontSize = Mathf.RoundToInt(size);
        }
        
        if (tmpText != null)
        {
            tmpText.fontSize = size;
        }
    }
    
    void SetLineSpacing(float spacing)
    {
        if (unityText != null)
        {
            unityText.lineSpacing = spacing;
        }
        
        if (tmpText != null)
        {
            tmpText.lineSpacing = spacing;
        }
    }
    
    // Public methods for manual control
    public void SetCustomFontSize(float size)
    {
        SetFontSize(size);
    }
    
    public void SetCustomWidth(float width)
    {
        if (rectTransform != null)
        {
            Vector2 sizeDelta = rectTransform.sizeDelta;
            sizeDelta.x = width;
            rectTransform.sizeDelta = sizeDelta;
        }
    }
    
    public void SetCustomHeight(float height)
    {
        if (rectTransform != null)
        {
            Vector2 sizeDelta = rectTransform.sizeDelta;
            sizeDelta.y = height;
            rectTransform.sizeDelta = sizeDelta;
        }
    }
}

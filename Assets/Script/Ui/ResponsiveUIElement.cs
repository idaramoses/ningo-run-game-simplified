using UnityEngine;

/// <summary>
/// General-purpose responsive UI element adjuster
/// Adjusts any RectTransform's width, height, position, and scale based on device type
/// </summary>
public class ResponsiveUIElement : MonoBehaviour
{
    [Header("Breakpoints Reference")]
    public ScreenBreakpoints breakpoints;
    
    [Header("What to Adjust")]
    public bool adjustWidth = false;
    public bool adjustHeight = false;
    public bool adjustPosition = false;
    public bool adjustScale = false;
    
    [Header("Width Settings")]
    public WidthSettings widthSettings;
    
    [Header("Height Settings")]
    public HeightSettings heightSettings;
    
    [Header("Position Settings")]
    public PositionSettings positionSettings;
    
    [Header("Scale Settings")]
    public ScaleSettings scaleSettings;
    
    private RectTransform rectTransform;
    private ScreenBreakpoints.DeviceType currentDeviceType;
    
    [System.Serializable]
    public class WidthSettings
    {
        public float smallPhone = 300f;
        public float mediumPhone = 400f;
        public float largePhone = 500f;
        public float smallTablet = 600f;
        public float largeTablet = 800f;
        public float desktop = 1000f;
    }
    
    [System.Serializable]
    public class HeightSettings
    {
        public float smallPhone = 50f;
        public float mediumPhone = 60f;
        public float largePhone = 70f;
        public float smallTablet = 80f;
        public float largeTablet = 100f;
        public float desktop = 120f;
    }
    
    [System.Serializable]
    public class PositionSettings
    {
        [Header("X Position")]
        public float smallPhoneX = 0f;
        public float mediumPhoneX = 0f;
        public float largePhoneX = 0f;
        public float smallTabletX = 0f;
        public float largeTabletX = 0f;
        public float desktopX = 0f;
        
        [Header("Y Position")]
        public float smallPhoneY = 0f;
        public float mediumPhoneY = 0f;
        public float largePhoneY = 0f;
        public float smallTabletY = 0f;
        public float largeTabletY = 0f;
        public float desktopY = 0f;
    }
    
    [System.Serializable]
    public class ScaleSettings
    {
        public float smallPhone = 0.8f;
        public float mediumPhone = 1.0f;
        public float largePhone = 1.1f;
        public float smallTablet = 1.3f;
        public float largeTablet = 1.5f;
        public float desktop = 2.0f;
    }
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        
        if (breakpoints == null)
        {
            breakpoints = ScriptableObject.CreateInstance<ScreenBreakpoints>();
            Debug.LogWarning("[ResponsiveUIElement] No ScreenBreakpoints assigned. Using defaults.");
        }
        
        // Initialize default values
        if (widthSettings == null) widthSettings = new WidthSettings();
        if (heightSettings == null) heightSettings = new HeightSettings();
        if (positionSettings == null) positionSettings = new PositionSettings();
        if (scaleSettings == null) scaleSettings = new ScaleSettings();
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
        if (rectTransform == null) return;
        
        currentDeviceType = breakpoints.GetDeviceType();
        
        // Apply width
        if (adjustWidth)
        {
            float width = GetWidthForDevice(currentDeviceType);
            Vector2 sizeDelta = rectTransform.sizeDelta;
            sizeDelta.x = width;
            rectTransform.sizeDelta = sizeDelta;
        }
        
        // Apply height
        if (adjustHeight)
        {
            float height = GetHeightForDevice(currentDeviceType);
            Vector2 sizeDelta = rectTransform.sizeDelta;
            sizeDelta.y = height;
            rectTransform.sizeDelta = sizeDelta;
        }
        
        // Apply position
        if (adjustPosition)
        {
            Vector2 position = GetPositionForDevice(currentDeviceType);
            rectTransform.anchoredPosition = position;
        }
        
        // Apply scale
        if (adjustScale)
        {
            float scale = GetScaleForDevice(currentDeviceType);
            rectTransform.localScale = Vector3.one * scale;
        }
        
        Debug.Log($"[ResponsiveUIElement] {gameObject.name} adjusted for {currentDeviceType}");
    }
    
    float GetWidthForDevice(ScreenBreakpoints.DeviceType deviceType)
    {
        switch (deviceType)
        {
            case ScreenBreakpoints.DeviceType.SmallPhone: return widthSettings.smallPhone;
            case ScreenBreakpoints.DeviceType.MediumPhone: return widthSettings.mediumPhone;
            case ScreenBreakpoints.DeviceType.LargePhone: return widthSettings.largePhone;
            case ScreenBreakpoints.DeviceType.SmallTablet: return widthSettings.smallTablet;
            case ScreenBreakpoints.DeviceType.LargeTablet: return widthSettings.largeTablet;
            case ScreenBreakpoints.DeviceType.Desktop: return widthSettings.desktop;
            default: return widthSettings.mediumPhone;
        }
    }
    
    float GetHeightForDevice(ScreenBreakpoints.DeviceType deviceType)
    {
        switch (deviceType)
        {
            case ScreenBreakpoints.DeviceType.SmallPhone: return heightSettings.smallPhone;
            case ScreenBreakpoints.DeviceType.MediumPhone: return heightSettings.mediumPhone;
            case ScreenBreakpoints.DeviceType.LargePhone: return heightSettings.largePhone;
            case ScreenBreakpoints.DeviceType.SmallTablet: return heightSettings.smallTablet;
            case ScreenBreakpoints.DeviceType.LargeTablet: return heightSettings.largeTablet;
            case ScreenBreakpoints.DeviceType.Desktop: return heightSettings.desktop;
            default: return heightSettings.mediumPhone;
        }
    }
    
    Vector2 GetPositionForDevice(ScreenBreakpoints.DeviceType deviceType)
    {
        float x = 0f, y = 0f;
        
        switch (deviceType)
        {
            case ScreenBreakpoints.DeviceType.SmallPhone:
                x = positionSettings.smallPhoneX;
                y = positionSettings.smallPhoneY;
                break;
            case ScreenBreakpoints.DeviceType.MediumPhone:
                x = positionSettings.mediumPhoneX;
                y = positionSettings.mediumPhoneY;
                break;
            case ScreenBreakpoints.DeviceType.LargePhone:
                x = positionSettings.largePhoneX;
                y = positionSettings.largePhoneY;
                break;
            case ScreenBreakpoints.DeviceType.SmallTablet:
                x = positionSettings.smallTabletX;
                y = positionSettings.smallTabletY;
                break;
            case ScreenBreakpoints.DeviceType.LargeTablet:
                x = positionSettings.largeTabletX;
                y = positionSettings.largeTabletY;
                break;
            case ScreenBreakpoints.DeviceType.Desktop:
                x = positionSettings.desktopX;
                y = positionSettings.desktopY;
                break;
        }
        
        return new Vector2(x, y);
    }
    
    float GetScaleForDevice(ScreenBreakpoints.DeviceType deviceType)
    {
        switch (deviceType)
        {
            case ScreenBreakpoints.DeviceType.SmallPhone: return scaleSettings.smallPhone;
            case ScreenBreakpoints.DeviceType.MediumPhone: return scaleSettings.mediumPhone;
            case ScreenBreakpoints.DeviceType.LargePhone: return scaleSettings.largePhone;
            case ScreenBreakpoints.DeviceType.SmallTablet: return scaleSettings.smallTablet;
            case ScreenBreakpoints.DeviceType.LargeTablet: return scaleSettings.largeTablet;
            case ScreenBreakpoints.DeviceType.Desktop: return scaleSettings.desktop;
            default: return scaleSettings.mediumPhone;
        }
    }
}

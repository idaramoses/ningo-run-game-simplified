using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ResponsiveButtonScaler : MonoBehaviour
{
    [Header("Breakpoints")]
    public ScreenBreakpoints breakpoints;
    
    [Header("Scale Settings")]
    [SerializeField] private bool scaleButton = true;
    [SerializeField] private bool adjustMinSize = true;
    
    [Header("Scale Multipliers")]
    [SerializeField] private ScaleSettings scaleSettings;
    
    [Header("Minimum Size Settings")]
    [SerializeField] private MinSizeSettings minSizeSettings;
    
    [System.Serializable]
    public class ScaleSettings
    {
        public float smallPhone = 0.7f;
        public float mediumPhone = 0.85f;
        public float largePhone = 1.0f;
        public float smallTablet = 1.15f;
        public float largeTablet = 1.3f;
        public float desktop = 1.5f;
    }
    
    [System.Serializable]
    public class MinSizeSettings
    {
        public Vector2 smallPhone = new Vector2(100, 50);
        public Vector2 mediumPhone = new Vector2(120, 60);
        public Vector2 largePhone = new Vector2(140, 70);
        public Vector2 smallTablet = new Vector2(160, 80);
        public Vector2 largeTablet = new Vector2(180, 90);
        public Vector2 desktop = new Vector2(200, 100);
    }
    
    private Button button;
    private RectTransform rectTransform;
    private ScreenBreakpoints.DeviceType currentDeviceType;
    
    void Awake()
    {
        button = GetComponent<Button>();
        rectTransform = GetComponent<RectTransform>();
        
        if (breakpoints == null)
        {
            breakpoints = ScriptableObject.CreateInstance<ScreenBreakpoints>();
        }
        
        if (scaleSettings == null)
        {
            scaleSettings = new ScaleSettings();
        }
        
        if (minSizeSettings == null)
        {
            minSizeSettings = new MinSizeSettings();
        }
    }
    
    void Start()
    {
        ApplyScaling();
    }
    
    void Update()
    {
        ScreenBreakpoints.DeviceType newDeviceType = breakpoints.GetDeviceType();
        
        if (newDeviceType != currentDeviceType)
        {
            currentDeviceType = newDeviceType;
            ApplyScaling();
        }
    }
    
    void ApplyScaling()
    {
        currentDeviceType = breakpoints.GetDeviceType();
        
        if (scaleButton)
        {
            float scale = GetScaleForDevice(currentDeviceType);
            rectTransform.localScale = Vector3.one * scale;
        }
        
        if (adjustMinSize)
        {
            Vector2 minSize = GetMinSizeForDevice(currentDeviceType);
            
            LayoutElement layoutElement = GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = gameObject.AddComponent<LayoutElement>();
            }
            
            layoutElement.minWidth = minSize.x;
            layoutElement.minHeight = minSize.y;
        }
    }
    
    float GetScaleForDevice(ScreenBreakpoints.DeviceType deviceType)
    {
        switch (deviceType)
        {
            case ScreenBreakpoints.DeviceType.SmallPhone:
                return scaleSettings.smallPhone;
            case ScreenBreakpoints.DeviceType.MediumPhone:
                return scaleSettings.mediumPhone;
            case ScreenBreakpoints.DeviceType.LargePhone:
                return scaleSettings.largePhone;
            case ScreenBreakpoints.DeviceType.SmallTablet:
                return scaleSettings.smallTablet;
            case ScreenBreakpoints.DeviceType.LargeTablet:
                return scaleSettings.largeTablet;
            case ScreenBreakpoints.DeviceType.Desktop:
                return scaleSettings.desktop;
            default:
                return scaleSettings.mediumPhone;
        }
    }
    
    Vector2 GetMinSizeForDevice(ScreenBreakpoints.DeviceType deviceType)
    {
        switch (deviceType)
        {
            case ScreenBreakpoints.DeviceType.SmallPhone:
                return minSizeSettings.smallPhone;
            case ScreenBreakpoints.DeviceType.MediumPhone:
                return minSizeSettings.mediumPhone;
            case ScreenBreakpoints.DeviceType.LargePhone:
                return minSizeSettings.largePhone;
            case ScreenBreakpoints.DeviceType.SmallTablet:
                return minSizeSettings.smallTablet;
            case ScreenBreakpoints.DeviceType.LargeTablet:
                return minSizeSettings.largeTablet;
            case ScreenBreakpoints.DeviceType.Desktop:
                return minSizeSettings.desktop;
            default:
                return minSizeSettings.mediumPhone;
        }
    }
}

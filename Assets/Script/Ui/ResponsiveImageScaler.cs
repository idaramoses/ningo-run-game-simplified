using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ResponsiveImageScaler : MonoBehaviour
{
    [Header("Breakpoints")]
    public ScreenBreakpoints breakpoints;
    
    [Header("Scale Settings")]
    [SerializeField] private bool maintainAspectRatio = true;
    [SerializeField] private ScaleMode scaleMode = ScaleMode.ScaleToFit;
    
    [Header("Size Multipliers per Device")]
    [SerializeField] private SizeMultipliers sizeMultipliers;
    
    public enum ScaleMode
    {
        ScaleToFit,
        FixedSize,
        ProportionalToScreen
    }
    
    [System.Serializable]
    public class SizeMultipliers
    {
        public float smallPhone = 0.7f;
        public float mediumPhone = 0.85f;
        public float largePhone = 1.0f;
        public float smallTablet = 1.2f;
        public float largeTablet = 1.4f;
        public float desktop = 1.6f;
    }
    
    private Image image;
    private RectTransform rectTransform;
    private Vector2 originalSize;
    private ScreenBreakpoints.DeviceType currentDeviceType;
    
    void Awake()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        originalSize = rectTransform.sizeDelta;
        
        if (breakpoints == null)
        {
            breakpoints = ScriptableObject.CreateInstance<ScreenBreakpoints>();
        }
        
        if (sizeMultipliers == null)
        {
            sizeMultipliers = new SizeMultipliers();
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
        float multiplier = GetMultiplierForDevice(currentDeviceType);
        
        switch (scaleMode)
        {
            case ScaleMode.ScaleToFit:
                rectTransform.localScale = Vector3.one * multiplier;
                break;
                
            case ScaleMode.FixedSize:
                Vector2 newSize = originalSize * multiplier;
                rectTransform.sizeDelta = newSize;
                break;
                
            case ScaleMode.ProportionalToScreen:
                float screenScale = Mathf.Min(Screen.width, Screen.height) / 1080f;
                rectTransform.localScale = Vector3.one * screenScale * multiplier;
                break;
        }
        
        if (maintainAspectRatio && image.sprite != null)
        {
            float aspectRatio = image.sprite.rect.width / image.sprite.rect.height;
            Vector2 size = rectTransform.sizeDelta;
            size.y = size.x / aspectRatio;
            rectTransform.sizeDelta = size;
        }
    }
    
    float GetMultiplierForDevice(ScreenBreakpoints.DeviceType deviceType)
    {
        switch (deviceType)
        {
            case ScreenBreakpoints.DeviceType.SmallPhone:
                return sizeMultipliers.smallPhone;
            case ScreenBreakpoints.DeviceType.MediumPhone:
                return sizeMultipliers.mediumPhone;
            case ScreenBreakpoints.DeviceType.LargePhone:
                return sizeMultipliers.largePhone;
            case ScreenBreakpoints.DeviceType.SmallTablet:
                return sizeMultipliers.smallTablet;
            case ScreenBreakpoints.DeviceType.LargeTablet:
                return sizeMultipliers.largeTablet;
            case ScreenBreakpoints.DeviceType.Desktop:
                return sizeMultipliers.desktop;
            default:
                return sizeMultipliers.mediumPhone;
        }
    }
}

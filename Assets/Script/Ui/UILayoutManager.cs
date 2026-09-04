using UnityEngine;
using System.Collections.Generic;

public class UILayoutManager : MonoBehaviour
{
    [Header("Breakpoints")]
    public ScreenBreakpoints breakpoints;
    
    [Header("Layout Groups")]
    [SerializeField] private List<LayoutGroup> layoutGroups = new List<LayoutGroup>();
    
    [Header("Safe Area")]
    [SerializeField] private bool applySafeArea = true;
    [SerializeField] private RectTransform safeAreaPanel;
    
    [Header("Dynamic Spacing")]
    [SerializeField] private bool adjustSpacing = true;
    [SerializeField] private SpacingSettings spacingSettings;
    
    private ScreenBreakpoints.DeviceType currentDeviceType;
    private Rect lastSafeArea;
    
    [System.Serializable]
    public class LayoutGroup
    {
        public string groupName;
        public RectTransform container;
        public LayoutType layoutType;
        public LayoutSettings settings;
    }
    
    public enum LayoutType
    {
        Vertical,
        Horizontal,
        Grid
    }
    
    [System.Serializable]
    public class LayoutSettings
    {
        public float smallPhoneSpacing = 10f;
        public float mediumPhoneSpacing = 15f;
        public float largePhoneSpacing = 20f;
        public float tabletSpacing = 30f;
        public float desktopSpacing = 40f;
        
        public Vector2 smallPhonePadding = new Vector2(10, 10);
        public Vector2 mediumPhonePadding = new Vector2(15, 15);
        public Vector2 largePhonePadding = new Vector2(20, 20);
        public Vector2 tabletPadding = new Vector2(30, 30);
        public Vector2 desktopPadding = new Vector2(40, 40);
    }
    
    [System.Serializable]
    public class SpacingSettings
    {
        public float smallPhone = 10f;
        public float mediumPhone = 15f;
        public float largePhone = 20f;
        public float smallTablet = 25f;
        public float largeTablet = 30f;
        public float desktop = 40f;
    }
    
    void Awake()
    {
        if (breakpoints == null)
        {
            breakpoints = ScriptableObject.CreateInstance<ScreenBreakpoints>();
        }
        
        if (spacingSettings == null)
        {
            spacingSettings = new SpacingSettings();
        }
    }
    
    void Start()
    {
        ApplyLayout();
        
        if (applySafeArea && safeAreaPanel != null)
        {
            ApplySafeAreaToPanel();
        }
    }
    
    void Update()
    {
        ScreenBreakpoints.DeviceType newDeviceType = breakpoints.GetDeviceType();
        
        if (newDeviceType != currentDeviceType)
        {
            currentDeviceType = newDeviceType;
            ApplyLayout();
        }
        
        if (applySafeArea && safeAreaPanel != null && lastSafeArea != Screen.safeArea)
        {
            ApplySafeAreaToPanel();
        }
    }
    
    void ApplyLayout()
    {
        currentDeviceType = breakpoints.GetDeviceType();
        
        foreach (var group in layoutGroups)
        {
            if (group.container == null) continue;
            
            float spacing = GetSpacingForDevice(group.settings);
            Vector2 padding = GetPaddingForDevice(group.settings);
            
            var verticalLayout = group.container.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
            var horizontalLayout = group.container.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            var gridLayout = group.container.GetComponent<UnityEngine.UI.GridLayoutGroup>();
            
            if (verticalLayout != null)
            {
                verticalLayout.spacing = spacing;
                verticalLayout.padding = new RectOffset(
                    Mathf.RoundToInt(padding.x),
                    Mathf.RoundToInt(padding.x),
                    Mathf.RoundToInt(padding.y),
                    Mathf.RoundToInt(padding.y)
                );
            }
            
            if (horizontalLayout != null)
            {
                horizontalLayout.spacing = spacing;
                horizontalLayout.padding = new RectOffset(
                    Mathf.RoundToInt(padding.x),
                    Mathf.RoundToInt(padding.x),
                    Mathf.RoundToInt(padding.y),
                    Mathf.RoundToInt(padding.y)
                );
            }
            
            if (gridLayout != null)
            {
                gridLayout.spacing = new Vector2(spacing, spacing);
                gridLayout.padding = new RectOffset(
                    Mathf.RoundToInt(padding.x),
                    Mathf.RoundToInt(padding.x),
                    Mathf.RoundToInt(padding.y),
                    Mathf.RoundToInt(padding.y)
                );
            }
        }
    }
    
    float GetSpacingForDevice(LayoutSettings settings)
    {
        switch (currentDeviceType)
        {
            case ScreenBreakpoints.DeviceType.SmallPhone:
                return settings.smallPhoneSpacing;
            case ScreenBreakpoints.DeviceType.MediumPhone:
                return settings.mediumPhoneSpacing;
            case ScreenBreakpoints.DeviceType.LargePhone:
                return settings.largePhoneSpacing;
            case ScreenBreakpoints.DeviceType.SmallTablet:
            case ScreenBreakpoints.DeviceType.LargeTablet:
                return settings.tabletSpacing;
            case ScreenBreakpoints.DeviceType.Desktop:
                return settings.desktopSpacing;
            default:
                return settings.mediumPhoneSpacing;
        }
    }
    
    Vector2 GetPaddingForDevice(LayoutSettings settings)
    {
        switch (currentDeviceType)
        {
            case ScreenBreakpoints.DeviceType.SmallPhone:
                return settings.smallPhonePadding;
            case ScreenBreakpoints.DeviceType.MediumPhone:
                return settings.mediumPhonePadding;
            case ScreenBreakpoints.DeviceType.LargePhone:
                return settings.largePhonePadding;
            case ScreenBreakpoints.DeviceType.SmallTablet:
            case ScreenBreakpoints.DeviceType.LargeTablet:
                return settings.tabletPadding;
            case ScreenBreakpoints.DeviceType.Desktop:
                return settings.desktopPadding;
            default:
                return settings.mediumPhonePadding;
        }
    }
    
    void ApplySafeAreaToPanel()
    {
        Rect safeArea = Screen.safeArea;
        lastSafeArea = safeArea;
        
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;
        
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        
        safeAreaPanel.anchorMin = anchorMin;
        safeAreaPanel.anchorMax = anchorMax;
    }
}

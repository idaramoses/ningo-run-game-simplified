using UnityEngine;
using System;

[CreateAssetMenu(fileName = "ScreenBreakpoints", menuName = "UI/Screen Breakpoints")]
public class ScreenBreakpoints : ScriptableObject
{
    [Header("Breakpoint Definitions")]
    [Tooltip("Small phones (e.g., iPhone SE)")]
    public int smallPhoneMaxWidth = 750;
    
    [Tooltip("Medium phones (e.g., iPhone 14)")]
    public int mediumPhoneMaxWidth = 1170;
    
    [Tooltip("Large phones (e.g., iPhone 14 Pro Max)")]
    public int largePhoneMaxWidth = 1440;
    
    [Tooltip("Small tablets (e.g., iPad Mini)")]
    public int smallTabletMaxWidth = 1536;
    
    [Tooltip("Large tablets (e.g., iPad Pro)")]
    public int largeTabletMaxWidth = 2048;
    
    [Header("Aspect Ratio Breakpoints")]
    [Tooltip("Ultra-wide aspect ratio threshold")]
    public float ultraWideAspect = 2.1f;
    
    [Tooltip("Wide aspect ratio threshold")]
    public float wideAspect = 1.9f;
    
    [Tooltip("Standard aspect ratio threshold")]
    public float standardAspect = 1.6f;
    
    [Tooltip("Square-ish aspect ratio threshold")]
    public float squareAspect = 1.4f;
    
    public enum DeviceType
    {
        SmallPhone,
        MediumPhone,
        LargePhone,
        SmallTablet,
        LargeTablet,
        Desktop
    }
    
    public enum AspectRatioType
    {
        Square,      // 4:3, 1:1
        Standard,    // 16:9
        Wide,        // 18:9, 19:9
        UltraWide    // 21:9+
    }
    
    public DeviceType GetDeviceType()
    {
        int width = Screen.width;
        
        if (width <= smallPhoneMaxWidth)
            return DeviceType.SmallPhone;
        else if (width <= mediumPhoneMaxWidth)
            return DeviceType.MediumPhone;
        else if (width <= largePhoneMaxWidth)
            return DeviceType.LargePhone;
        else if (width <= smallTabletMaxWidth)
            return DeviceType.SmallTablet;
        else if (width <= largeTabletMaxWidth)
            return DeviceType.LargeTablet;
        else
            return DeviceType.Desktop;
    }
    
    public AspectRatioType GetAspectRatioType()
    {
        float aspect = (float)Screen.width / Screen.height;
        
        if (aspect >= ultraWideAspect)
            return AspectRatioType.UltraWide;
        else if (aspect >= wideAspect)
            return AspectRatioType.Wide;
        else if (aspect >= standardAspect)
            return AspectRatioType.Standard;
        else
            return AspectRatioType.Square;
    }
    
    public bool IsPortrait()
    {
        return Screen.height > Screen.width;
    }
    
    public bool IsLandscape()
    {
        return Screen.width > Screen.height;
    }
}

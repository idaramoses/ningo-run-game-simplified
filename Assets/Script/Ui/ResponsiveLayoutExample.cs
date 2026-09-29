using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Example implementation showing how to use ResponsiveMediaQuery
/// This demonstrates common responsive patterns like CSS media queries
/// </summary>
public class ResponsiveLayoutExample : MonoBehaviour
{
    [Header("References")]
    public ResponsiveMediaQuery mediaQuery;
    
    [Header("UI Elements to Adjust")]
    public Text titleText;
    public Button playButton;
    public RectTransform menuPanel;
    
    void Start()
    {
        if (mediaQuery == null)
            mediaQuery = GetComponent<ResponsiveMediaQuery>();
        
        // You can also check device type programmatically
        AdjustLayoutBasedOnDevice();
    }
    
    void AdjustLayoutBasedOnDevice()
    {
        if (mediaQuery == null) return;
        
        // Example: Adjust font sizes based on device
        if (mediaQuery.IsDeviceType(ScreenBreakpoints.DeviceType.SmallPhone))
        {
            if (titleText != null) titleText.fontSize = 32;
            Debug.Log("Small phone detected - using smaller fonts");
        }
        else if (mediaQuery.IsDeviceType(ScreenBreakpoints.DeviceType.LargeTablet))
        {
            if (titleText != null) titleText.fontSize = 64;
            Debug.Log("Large tablet detected - using larger fonts");
        }
        else
        {
            if (titleText != null) titleText.fontSize = 48;
        }
        
        // Example: Adjust button size based on aspect ratio
        if (mediaQuery.IsAspectRatio(ScreenBreakpoints.AspectRatioType.UltraWide))
        {
            if (playButton != null)
            {
                RectTransform buttonRect = playButton.GetComponent<RectTransform>();
                buttonRect.sizeDelta = new Vector2(400, 100); // Wider button
            }
        }
        
        // Example: Change layout based on orientation
        if (mediaQuery.IsPortrait())
        {
            // Portrait layout: vertical menu
            if (menuPanel != null)
            {
                var layoutGroup = menuPanel.GetComponent<VerticalLayoutGroup>();
                if (layoutGroup != null) layoutGroup.spacing = 20;
            }
        }
        else
        {
            // Landscape layout: horizontal menu
            if (menuPanel != null)
            {
                var layoutGroup = menuPanel.GetComponent<HorizontalLayoutGroup>();
                if (layoutGroup != null) layoutGroup.spacing = 40;
            }
        }
    }
}

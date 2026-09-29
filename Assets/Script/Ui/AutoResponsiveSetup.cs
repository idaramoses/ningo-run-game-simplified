using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Automatically sets up responsive components on UI elements
/// Run this from Unity Editor: Tools > Setup Responsive UI
/// </summary>
public class AutoResponsiveSetup : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Setup Responsive UI")]
    static void SetupResponsiveUI()
    {
        // Find or create ScreenBreakpoints asset
        ScreenBreakpoints breakpoints = FindOrCreateBreakpoints();
        
        // Find all canvases in the scene
        Canvas[] canvases = FindObjectsOfType<Canvas>(true);
        
        int textCount = 0;
        int imageCount = 0;
        int buttonCount = 0;
        
        foreach (Canvas canvas in canvases)
        {
            // Process all Text and TextMeshPro components
            Text[] texts = canvas.GetComponentsInChildren<Text>(true);
            foreach (Text text in texts)
            {
                if (text.GetComponent<ResponsiveTextAdjuster>() == null)
                {
                    ResponsiveTextAdjuster adjuster = text.gameObject.AddComponent<ResponsiveTextAdjuster>();
                    adjuster.breakpoints = breakpoints;
                    adjuster.adjustFontSize = true;
                    
                    // Set default font sizes based on current size
                    float baseFontSize = text.fontSize;
                    adjuster.fontSizes = new ResponsiveTextAdjuster.FontSizeBreakpoints
                    {
                        smallPhone = baseFontSize * 0.7f,
                        mediumPhone = baseFontSize * 0.85f,
                        largePhone = baseFontSize,
                        smallTablet = baseFontSize * 1.2f,
                        largeTablet = baseFontSize * 1.5f,
                        desktop = baseFontSize * 2f
                    };
                    
                    textCount++;
                    EditorUtility.SetDirty(text.gameObject);
                }
            }
            
            TMP_Text[] tmpTexts = canvas.GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text tmpText in tmpTexts)
            {
                if (tmpText.GetComponent<ResponsiveTextAdjuster>() == null)
                {
                    ResponsiveTextAdjuster adjuster = tmpText.gameObject.AddComponent<ResponsiveTextAdjuster>();
                    adjuster.breakpoints = breakpoints;
                    adjuster.adjustFontSize = true;
                    
                    float baseFontSize = tmpText.fontSize;
                    adjuster.fontSizes = new ResponsiveTextAdjuster.FontSizeBreakpoints
                    {
                        smallPhone = baseFontSize * 0.7f,
                        mediumPhone = baseFontSize * 0.85f,
                        largePhone = baseFontSize,
                        smallTablet = baseFontSize * 1.2f,
                        largeTablet = baseFontSize * 1.5f,
                        desktop = baseFontSize * 2f
                    };
                    
                    textCount++;
                    EditorUtility.SetDirty(tmpText.gameObject);
                }
            }
            
            // Process buttons
            Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
            foreach (Button button in buttons)
            {
                RectTransform rectTransform = button.GetComponent<RectTransform>();
                if (rectTransform != null && button.GetComponent<ResponsiveUIElement>() == null)
                {
                    ResponsiveUIElement element = button.gameObject.AddComponent<ResponsiveUIElement>();
                    element.breakpoints = breakpoints;
                    element.adjustScale = true;
                    
                    element.scaleSettings = new ResponsiveUIElement.ScaleSettings
                    {
                        smallPhone = 0.7f,
                        mediumPhone = 0.85f,
                        largePhone = 1f,
                        smallTablet = 1.2f,
                        largeTablet = 1.4f,
                        desktop = 1.6f
                    };
                    
                    buttonCount++;
                    EditorUtility.SetDirty(button.gameObject);
                }
            }
            
            // Process images (logos, icons, etc.)
            Image[] images = canvas.GetComponentsInChildren<Image>(true);
            foreach (Image image in images)
            {
                // Only process images that are likely logos or important UI elements
                if (image.name.ToLower().Contains("logo") || 
                    image.name.ToLower().Contains("title") ||
                    image.name.ToLower().Contains("icon"))
                {
                    if (image.GetComponent<ResponsiveUIElement>() == null)
                    {
                        ResponsiveUIElement element = image.gameObject.AddComponent<ResponsiveUIElement>();
                        element.breakpoints = breakpoints;
                        element.adjustScale = true;
                        
                        element.scaleSettings = new ResponsiveUIElement.ScaleSettings
                        {
                            smallPhone = 0.7f,
                            mediumPhone = 0.85f,
                            largePhone = 1f,
                            smallTablet = 1.2f,
                            largeTablet = 1.5f,
                            desktop = 2f
                        };
                        
                        imageCount++;
                        EditorUtility.SetDirty(image.gameObject);
                    }
                }
            }
        }
        
        Debug.Log($"[AutoResponsiveSetup] Setup complete!\n" +
                  $"- {textCount} text elements configured\n" +
                  $"- {buttonCount} buttons configured\n" +
                  $"- {imageCount} images configured");
        
        EditorUtility.DisplayDialog("Responsive Setup Complete", 
            $"Successfully configured:\n" +
            $"• {textCount} text elements\n" +
            $"• {buttonCount} buttons\n" +
            $"• {imageCount} images\n\n" +
            $"Test in Game View with different resolutions!", 
            "OK");
    }
    
    static ScreenBreakpoints FindOrCreateBreakpoints()
    {
        // Try to find existing breakpoints
        string[] guids = AssetDatabase.FindAssets("t:ScreenBreakpoints");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<ScreenBreakpoints>(path);
        }
        
        // Create new breakpoints
        ScreenBreakpoints breakpoints = ScriptableObject.CreateInstance<ScreenBreakpoints>();
        
        // Ensure directory exists
        string dir = "Assets/Data";
        if (!AssetDatabase.IsValidFolder(dir))
        {
            AssetDatabase.CreateFolder("Assets", "Data");
        }
        
        string assetPath = "Assets/Data/GameScreenBreakpoints.asset";
        AssetDatabase.CreateAsset(breakpoints, assetPath);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"[AutoResponsiveSetup] Created ScreenBreakpoints at {assetPath}");
        
        return breakpoints;
    }
    
    [MenuItem("Tools/Remove All Responsive Components")]
    static void RemoveResponsiveComponents()
    {
        if (!EditorUtility.DisplayDialog("Remove Responsive Components", 
            "This will remove all ResponsiveTextAdjuster and ResponsiveUIElement components from the scene. Continue?", 
            "Yes", "Cancel"))
        {
            return;
        }
        
        int removed = 0;
        
        ResponsiveTextAdjuster[] textAdjusters = FindObjectsOfType<ResponsiveTextAdjuster>(true);
        foreach (var adjuster in textAdjusters)
        {
            DestroyImmediate(adjuster);
            removed++;
        }
        
        ResponsiveUIElement[] uiElements = FindObjectsOfType<ResponsiveUIElement>(true);
        foreach (var element in uiElements)
        {
            DestroyImmediate(element);
            removed++;
        }
        
        Debug.Log($"[AutoResponsiveSetup] Removed {removed} responsive components");
        EditorUtility.DisplayDialog("Components Removed", $"Removed {removed} responsive components", "OK");
    }
#endif
}

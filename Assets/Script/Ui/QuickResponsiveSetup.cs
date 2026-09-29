using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class QuickResponsiveSetup : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Quick Setup/Setup Canvas Responsiveness")]
    static void SetupCanvasResponsiveness()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>(true);
        
        if (canvases.Length == 0)
        {
            EditorUtility.DisplayDialog("No Canvas Found", "No Canvas objects found in the scene.", "OK");
            return;
        }
        
        int canvasCount = 0;
        
        foreach (Canvas canvas in canvases)
        {
            if (canvas.GetComponent<CanvasResponsiveManager>() == null)
            {
                CanvasResponsiveManager manager = canvas.gameObject.AddComponent<CanvasResponsiveManager>();
                EditorUtility.SetDirty(canvas.gameObject);
                canvasCount++;
                Debug.Log($"[QuickSetup] Added CanvasResponsiveManager to {canvas.name}");
            }
        }
        
        EditorUtility.DisplayDialog("Canvas Setup Complete", 
            $"Added CanvasResponsiveManager to {canvasCount} canvas(es).\n\n" +
            "Next steps:\n" +
            "1. Run 'Setup All Text Elements'\n" +
            "2. Run 'Setup All Buttons'\n" +
            "3. Run 'Setup Safe Area'", 
            "OK");
    }
    
    [MenuItem("Tools/Quick Setup/Setup All Text Elements")]
    static void SetupAllTextElements()
    {
        ScreenBreakpoints breakpoints = FindOrCreateBreakpoints();
        
        Canvas[] canvases = FindObjectsOfType<Canvas>(true);
        int textCount = 0;
        
        foreach (Canvas canvas in canvases)
        {
            Text[] texts = canvas.GetComponentsInChildren<Text>(true);
            foreach (Text text in texts)
            {
                if (text.GetComponent<ResponsiveTextAdjuster>() == null)
                {
                    ResponsiveTextAdjuster adjuster = text.gameObject.AddComponent<ResponsiveTextAdjuster>();
                    adjuster.breakpoints = breakpoints;
                    adjuster.adjustFontSize = true;
                    
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
        }
        
        Debug.Log($"[QuickSetup] Setup {textCount} text elements");
        EditorUtility.DisplayDialog("Text Setup Complete", 
            $"Added ResponsiveTextAdjuster to {textCount} text element(s).", 
            "OK");
    }
    
    [MenuItem("Tools/Quick Setup/Setup All Buttons")]
    static void SetupAllButtons()
    {
        ScreenBreakpoints breakpoints = FindOrCreateBreakpoints();
        
        Canvas[] canvases = FindObjectsOfType<Canvas>(true);
        int buttonCount = 0;
        
        foreach (Canvas canvas in canvases)
        {
            Button[] buttons = canvas.GetComponentsInChildren<Button>(true);
            foreach (Button button in buttons)
            {
                if (button.GetComponent<ResponsiveButtonScaler>() == null)
                {
                    ResponsiveButtonScaler scaler = button.gameObject.AddComponent<ResponsiveButtonScaler>();
                    scaler.breakpoints = breakpoints;
                    
                    buttonCount++;
                    EditorUtility.SetDirty(button.gameObject);
                }
            }
        }
        
        Debug.Log($"[QuickSetup] Setup {buttonCount} buttons");
        EditorUtility.DisplayDialog("Button Setup Complete", 
            $"Added ResponsiveButtonScaler to {buttonCount} button(s).", 
            "OK");
    }
    
    [MenuItem("Tools/Quick Setup/Setup Safe Area")]
    static void SetupSafeArea()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>(true);
        int safeAreaCount = 0;
        
        foreach (Canvas canvas in canvases)
        {
            Transform firstChild = canvas.transform.childCount > 0 ? canvas.transform.GetChild(0) : null;
            
            if (firstChild != null)
            {
                RectTransform rectTransform = firstChild.GetComponent<RectTransform>();
                if (rectTransform != null && firstChild.GetComponent<SafeAreaHandler>() == null)
                {
                    SafeAreaHandler handler = firstChild.gameObject.AddComponent<SafeAreaHandler>();
                    safeAreaCount++;
                    EditorUtility.SetDirty(firstChild.gameObject);
                    Debug.Log($"[QuickSetup] Added SafeAreaHandler to {firstChild.name}");
                }
            }
        }
        
        EditorUtility.DisplayDialog("Safe Area Setup Complete", 
            $"Added SafeAreaHandler to {safeAreaCount} panel(s).\n\n" +
            "This handles device notches and safe areas automatically.", 
            "OK");
    }
    
    [MenuItem("Tools/Quick Setup/Setup Everything (Recommended)")]
    static void SetupEverything()
    {
        SetupCanvasResponsiveness();
        SetupAllTextElements();
        SetupAllButtons();
        SetupSafeArea();
        
        EditorUtility.DisplayDialog("Complete Setup Finished", 
            "All responsive components have been added!\n\n" +
            "Test your UI:\n" +
            "1. Enter Play mode\n" +
            "2. Change Game View resolution\n" +
            "3. Check different device sizes\n\n" +
            "See UI_RESPONSIVENESS_SETUP.md for details.", 
            "OK");
    }
    
    [MenuItem("Tools/Quick Setup/Add Logo Scaler to Selected")]
    static void AddLogoScalerToSelected()
    {
        if (Selection.activeGameObject == null)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select a GameObject with an Image component.", "OK");
            return;
        }
        
        Image image = Selection.activeGameObject.GetComponent<Image>();
        if (image == null)
        {
            EditorUtility.DisplayDialog("No Image Component", "Selected GameObject must have an Image component.", "OK");
            return;
        }
        
        ScreenBreakpoints breakpoints = FindOrCreateBreakpoints();
        
        if (Selection.activeGameObject.GetComponent<ResponsiveImageScaler>() == null)
        {
            ResponsiveImageScaler scaler = Selection.activeGameObject.AddComponent<ResponsiveImageScaler>();
            scaler.breakpoints = breakpoints;
            EditorUtility.SetDirty(Selection.activeGameObject);
            
            Debug.Log($"[QuickSetup] Added ResponsiveImageScaler to {Selection.activeGameObject.name}");
            EditorUtility.DisplayDialog("Image Scaler Added", 
                $"Added ResponsiveImageScaler to {Selection.activeGameObject.name}", 
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Already Has Component", 
                "Selected GameObject already has ResponsiveImageScaler.", 
                "OK");
        }
    }
    
    static ScreenBreakpoints FindOrCreateBreakpoints()
    {
        string[] guids = AssetDatabase.FindAssets("t:ScreenBreakpoints");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<ScreenBreakpoints>(path);
        }
        
        ScreenBreakpoints breakpoints = ScriptableObject.CreateInstance<ScreenBreakpoints>();
        
        string dir = "Assets/Data";
        if (!AssetDatabase.IsValidFolder(dir))
        {
            AssetDatabase.CreateFolder("Assets", "Data");
        }
        
        string assetPath = "Assets/Data/GameScreenBreakpoints.asset";
        AssetDatabase.CreateAsset(breakpoints, assetPath);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"[QuickSetup] Created ScreenBreakpoints at {assetPath}");
        
        return breakpoints;
    }
    
    [MenuItem("Tools/Quick Setup/Remove All Responsive Components")]
    static void RemoveAllResponsiveComponents()
    {
        if (!EditorUtility.DisplayDialog("Remove All Responsive Components", 
            "This will remove ALL responsive components from the scene:\n" +
            "- CanvasResponsiveManager\n" +
            "- ResponsiveTextAdjuster\n" +
            "- ResponsiveButtonScaler\n" +
            "- ResponsiveImageScaler\n" +
            "- SafeAreaHandler\n" +
            "- UILayoutManager\n" +
            "- OrientationHandler\n\n" +
            "Continue?", 
            "Yes, Remove All", "Cancel"))
        {
            return;
        }
        
        int removed = 0;
        
        removed += RemoveComponents<CanvasResponsiveManager>();
        removed += RemoveComponents<ResponsiveTextAdjuster>();
        removed += RemoveComponents<ResponsiveButtonScaler>();
        removed += RemoveComponents<ResponsiveImageScaler>();
        removed += RemoveComponents<SafeAreaHandler>();
        removed += RemoveComponents<UILayoutManager>();
        removed += RemoveComponents<OrientationHandler>();
        
        Debug.Log($"[QuickSetup] Removed {removed} responsive components");
        EditorUtility.DisplayDialog("Components Removed", 
            $"Removed {removed} responsive components from the scene.", 
            "OK");
    }
    
    static int RemoveComponents<T>() where T : Component
    {
        T[] components = FindObjectsOfType<T>(true);
        foreach (var component in components)
        {
            DestroyImmediate(component);
        }
        return components.Length;
    }
#endif
}

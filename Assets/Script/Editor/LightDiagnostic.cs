using UnityEngine;
using UnityEditor;

public class LightDiagnostic : EditorWindow
{
    [MenuItem("Tools/Lighting/🔍 Diagnose Light Issues")]
    public static void DiagnoseLights()
    {
        Debug.Log("\n=== LIGHT DIAGNOSTIC REPORT ===\n");
        
        // Check all lights in scene
        Light[] allLights = FindObjectsOfType<Light>();
        Debug.Log($"Total Light components in scene: {allLights.Length}\n");
        
        if (allLights.Length == 0)
        {
            Debug.LogError("❌ NO LIGHTS FOUND IN SCENE!");
            Debug.Log("Solution: Add StreetLightController or CarHeadlightController components and click Setup buttons");
            return;
        }
        
        int enabledCount = 0;
        int disabledCount = 0;
        int tooWeakCount = 0;
        
        foreach (Light light in allLights)
        {
            string status = light.enabled ? "✓ ON" : "✗ OFF";
            string parent = light.transform.parent != null ? light.transform.parent.name : "ROOT";
            
            Debug.Log($"{status} | {light.type} | {light.gameObject.name} (parent: {parent})");
            Debug.Log($"    Intensity: {light.intensity} | Range: {light.range} | Color: {light.color}");
            
            if (light.enabled)
            {
                enabledCount++;
                
                if (light.intensity < 0.5f)
                {
                    Debug.LogWarning($"    ⚠️ Intensity too low! Increase to at least 1.0");
                    tooWeakCount++;
                }
                
                if (light.range < 5f)
                {
                    Debug.LogWarning($"    ⚠️ Range too small! Increase to at least 10");
                }
            }
            else
            {
                disabledCount++;
                Debug.LogWarning($"    ⚠️ Light is DISABLED! Turn it on.");
            }
            
            Debug.Log("");
        }
        
        Debug.Log("=== SUMMARY ===");
        Debug.Log($"Enabled Lights: {enabledCount}");
        Debug.Log($"Disabled Lights: {disabledCount}");
        Debug.Log($"Too Weak: {tooWeakCount}");
        
        if (disabledCount > 0)
        {
            Debug.LogWarning($"\n⚠️ {disabledCount} lights are DISABLED!");
            Debug.Log("Fix: Select the light object and check the 'enabled' checkbox in Inspector");
        }
        
        if (tooWeakCount > 0)
        {
            Debug.LogWarning($"\n⚠️ {tooWeakCount} lights are too weak to see!");
            Debug.Log("Fix: Increase intensity to 2.0 or higher");
        }
        
        // Check render settings
        Debug.Log("\n=== RENDER SETTINGS ===");
        Debug.Log($"Ambient Mode: {RenderSettings.ambientMode}");
        Debug.Log($"Ambient Intensity: {RenderSettings.ambientIntensity}");
        
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Debug.Log($"\nMain Camera HDR: {(mainCam.allowHDR ? "✓ Enabled" : "✗ Disabled")}");
        }
        
        Debug.Log("\n=== QUICK FIXES ===");
        Debug.Log("1. Run: Tools → Lighting → 🔧 Force Enable All Lights");
        Debug.Log("2. Increase intensity in StreetLightController component");
        Debug.Log("3. Check Scene view lighting is enabled (sun icon in toolbar)");
    }
    
    [MenuItem("Tools/Lighting/🔧 Force Enable All Lights")]
    public static void ForceEnableAllLights()
    {
        Light[] allLights = FindObjectsOfType<Light>();
        int count = 0;
        
        foreach (Light light in allLights)
        {
            if (!light.enabled)
            {
                light.enabled = true;
                EditorUtility.SetDirty(light);
                count++;
            }
            
            // Boost weak lights
            if (light.intensity < 1.0f)
            {
                light.intensity = 2.0f;
                EditorUtility.SetDirty(light);
            }
        }
        
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );
        
        Debug.Log($"✓ Enabled {count} lights and boosted weak lights to intensity 2.0");
    }
    
    [MenuItem("Tools/Lighting/⚡ Boost All Light Intensity")]
    public static void BoostAllLights()
    {
        Light[] allLights = FindObjectsOfType<Light>();
        
        foreach (Light light in allLights)
        {
            if (light.type == LightType.Point)
            {
                light.intensity = 3.0f;
                light.range = 20f;
            }
            else if (light.type == LightType.Spot)
            {
                light.intensity = 4.0f;
                light.range = 25f;
            }
            
            light.enabled = true;
            EditorUtility.SetDirty(light);
        }
        
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );
        
        Debug.Log($"✓ Boosted {allLights.Length} lights to high intensity");
        Debug.Log("Point Lights: Intensity 3.0, Range 20");
        Debug.Log("Spot Lights: Intensity 4.0, Range 25");
    }
}

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class SunsetLightingSetup : EditorWindow
{
    [MenuItem("Tools/Lighting/Apply Professional Sunset (URP)")]
    public static void ApplySunsetLighting()
    {
        Debug.Log("=== APPLYING PROFESSIONAL SUNSET LIGHTING (URP) ===");
        Debug.Log("Using skybox-based lighting (70% of lighting comes from sky)\n");
        
        ConfigureSkyboxLighting();
        ConfigureDirectionalLight();
        AddFillLight();
        ConfigureFog();
        
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("\n✓ Professional sunset lighting applied!");
        Debug.Log("Next: Add Character Light and Post Processing (see menu options)");
    }
    
    private static void ConfigureSkyboxLighting()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 1.0f;
        
        Material skybox = RenderSettings.skybox;
        
        if (skybox != null && skybox.shader.name.Contains("Skybox/Procedural"))
        {
            skybox.SetFloat("_SunSize", 0.05f);
            skybox.SetFloat("_SunSizeConvergence", 4f);
            skybox.SetFloat("_AtmosphereThickness", 1.0f);
            skybox.SetColor("_SkyTint", new Color(0.7f, 0.85f, 1.0f));
            skybox.SetColor("_GroundColor", new Color(0.8f, 0.5f, 0.3f));
            skybox.SetFloat("_Exposure", 1.0f);
            
            Debug.Log("✓ Configured Procedural Skybox (70% of lighting)");
            Debug.Log("  - Sun Size: 0.05");
            Debug.Log("  - Atmosphere Thickness: 1.0");
            Debug.Log("  - Sky Tint: Light blue");
            Debug.Log("  - Ground: Warm brown/orange");
            Debug.Log("  - Exposure: 1.0 (reduced)");
        }
        else if (skybox != null)
        {
            if (skybox.HasProperty("_Tint"))
                skybox.SetColor("_Tint", new Color(1.0f, 0.85f, 0.7f));
            if (skybox.HasProperty("_Exposure"))
                skybox.SetFloat("_Exposure", 1.0f);
            
            Debug.Log("✓ Configured existing skybox with warm tint");
        }
        else
        {
            Debug.LogWarning("⚠️ No skybox found! Create one: Window → Rendering → Lighting → Skybox Material");
            Debug.LogWarning("Recommended: Use Skybox/Procedural shader");
        }
    }
    
    private static void ConfigureDirectionalLight()
    {
        Light[] allLights = FindObjectsOfType<Light>();
        Light mainLight = null;
        
        foreach (Light light in allLights)
        {
            if (light.type == LightType.Directional && light.name.Contains("Directional Light") && light.enabled)
            {
                if (mainLight == null)
                {
                    mainLight = light;
                }
            }
        }
        
        if (mainLight == null)
        {
            GameObject lightObj = new GameObject("Directional Light (Sun)");
            mainLight = lightObj.AddComponent<Light>();
            mainLight.type = LightType.Directional;
            Debug.Log("Created new Directional Light (Sun)");
        }
        
        Color sunsetOrange = new Color(1.0f, 0.824f, 0.631f);
        mainLight.color = sunsetOrange;
        mainLight.intensity = 1.2f;
        mainLight.transform.rotation = Quaternion.Euler(35f, -30f, 0f);
        mainLight.shadows = LightShadows.Soft;
        mainLight.shadowStrength = 0.8f;
        mainLight.shadowResolution = UnityEngine.Rendering.LightShadowResolution.High;
        mainLight.shadowBias = 0.05f;
        mainLight.shadowNormalBias = 0.4f;
        
        EditorUtility.SetDirty(mainLight);
        Debug.Log($"\n✓ Configured Main Directional Light (Sun): {mainLight.name}");
        Debug.Log($"  - Color: Warm Orange #FFD2A1 (NEVER white for sunset!)");
        Debug.Log($"  - Intensity: {mainLight.intensity}");
        Debug.Log($"  - Rotation: X={mainLight.transform.rotation.eulerAngles.x}, Y={mainLight.transform.rotation.eulerAngles.y}");
        Debug.Log($"  - Shadows: Soft, Strength 0.8");
    }
    
    private static void AddFillLight()
    {
        Light[] allLights = FindObjectsOfType<Light>();
        Light fillLight = null;
        
        foreach (Light light in allLights)
        {
            if (light.name.Contains("Fill Light"))
            {
                fillLight = light;
                break;
            }
        }
        
        if (fillLight == null)
        {
            GameObject fillLightObj = new GameObject("Fill Light");
            fillLight = fillLightObj.AddComponent<Light>();
            fillLight.type = LightType.Directional;
        }
        
        fillLight.color = new Color(0.7f, 0.8f, 1.0f);
        fillLight.intensity = 0.3f;
        fillLight.transform.rotation = Quaternion.Euler(35f, 150f, 0f);
        fillLight.shadows = LightShadows.None;
        
        EditorUtility.SetDirty(fillLight);
        Debug.Log($"\n✓ Added Fill Light (opposite side of sun)");
        Debug.Log($"  - Color: Slightly blue");
        Debug.Log($"  - Intensity: 0.3");
        Debug.Log($"  - Shadows: OFF");
        Debug.Log($"  - Purpose: Brightens buildings, keeps shadows soft, makes characters pop");
    }
    
    
    private static void ConfigureFog()
    {
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(1.0f, 0.8f, 0.6f);
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.005f;
        
        Debug.Log("✓ Configured Fog");
        Debug.Log("  - Color: Warm golden");
        Debug.Log("  - Density: Light (for depth)");
    }
    
    
    [MenuItem("Tools/Lighting/Add Character Light (Spot)")]
    public static void AddCharacterLight()
    {
        GameObject charLightObj = new GameObject("Character Light");
        Light charLight = charLightObj.AddComponent<Light>();
        
        charLight.type = LightType.Spot;
        charLight.color = new Color(1.0f, 0.95f, 0.85f);
        charLight.intensity = 1.0f;
        charLight.range = 5f;
        charLight.spotAngle = 60f;
        charLight.shadows = LightShadows.None;
        charLight.transform.position = new Vector3(0, 2, -2);
        charLight.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
        
        EditorUtility.SetDirty(charLight);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        
        Debug.Log("\n✓ Added Character Spot Light");
        Debug.Log("  - Type: Spot Light");
        Debug.Log("  - Color: Warm white");
        Debug.Log("  - Intensity: 1.0");
        Debug.Log("  - Range: 5");
        Debug.Log("  - Shadows: OFF");
        Debug.Log("  - Position: Slightly above & front of runner");
        Debug.Log("\n  Purpose: Bright face, clean skin tones, 'game menu' look");
        Debug.Log("  ⚠️ IMPORTANT: Position this light to follow your character!");
    }
    
    [MenuItem("Tools/Lighting/Reset to Default Lighting")]
    public static void ResetLighting()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 1.0f;
        RenderSettings.fog = false;
        
        Light[] allLights = FindObjectsOfType<Light>();
        foreach (Light light in allLights)
        {
            if (light.type == LightType.Directional)
            {
                light.color = Color.white;
                light.intensity = 1.0f;
                light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }
        }
        
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("✓ Reset lighting to Unity defaults");
    }
    
    [MenuItem("Tools/Lighting/🌅 COMPLETE SETUP - Professional Sunset")]
    public static void CompleteSetup()
    {
        ApplySunsetLighting();
        AddCharacterLight();
        
        Debug.Log("\n\n=== ✓ COMPLETE PROFESSIONAL SETUP DONE ===");
        Debug.Log("\nYour scene now has:");
        Debug.Log("  ✓ Skybox-based lighting (70% of lighting)");
        Debug.Log("  ✓ Warm sunset directional light");
        Debug.Log("  ✓ Fill light (prevents dull look)");
        Debug.Log("  ✓ Character spot light (makes runner pop)");
        Debug.Log("  ✓ Atmospheric fog");
        Debug.Log("\n⚠️ MANDATORY NEXT STEP: Enable Post Processing");
        Debug.Log("Run: Tools → Lighting → Setup URP Post Processing");
        Debug.Log("\nWithout Post Processing, you won't get the cinematic look!");
    }
    
    [MenuItem("Tools/Lighting/Setup URP Post Processing")]
    public static void SetupPostProcessing()
    {
        Debug.Log("\n=== URP POST PROCESSING SETUP ===");
        Debug.Log("\nMANDATORY: Without this, you won't get the cinematic look!\n");
        
        Debug.Log("STEP 1: Enable Post Processing in URP Asset");
        Debug.Log("  - Go to: Edit → Project Settings → Graphics");
        Debug.Log("  - Find your URP Asset (Universal Render Pipeline Asset)");
        Debug.Log("  - Check 'Post Processing' is enabled\n");
        
        Debug.Log("STEP 2: Add Global Volume");
        Debug.Log("  - Right-click in Hierarchy");
        Debug.Log("  - Create → Volume → Global Volume");
        Debug.Log("  - In Inspector: Check 'Is Global'\n");
        
        Debug.Log("STEP 3: Create Profile & Add Effects");
        Debug.Log("  - Click 'New' next to Profile");
        Debug.Log("  - Click 'Add Override' and add these:\n");
        
        Debug.Log("🌈 Color Adjustments:");
        Debug.Log("  - Post Exposure: +0.2");
        Debug.Log("  - Contrast: +10");
        Debug.Log("  - Saturation: +8\n");
        
        Debug.Log("☀️ Bloom (ESSENTIAL):");
        Debug.Log("  - Intensity: 0.3");
        Debug.Log("  - Threshold: 1.1\n");
        
        Debug.Log("🎥 Vignette (subtle):");
        Debug.Log("  - Intensity: 0.15\n");
        
        Debug.Log("STEP 4: Enable on Camera");
        Debug.Log("  - Select Main Camera");
        Debug.Log("  - In Inspector → Rendering section");
        Debug.Log("  - Check 'Post Processing' checkbox\n");
        
        Debug.Log("=== This gives you the cinematic polish! ===");
    }
    
    [MenuItem("Tools/Lighting/Lighting Report")]
    public static void GenerateLightingReport()
    {
        Debug.Log("\n=== LIGHTING CONFIGURATION REPORT ===\n");
        
        Light[] allLights = FindObjectsOfType<Light>();
        Debug.Log($"Total Lights: {allLights.Length}");
        
        int directionalCount = 0;
        int spotCount = 0;
        int pointCount = 0;
        
        foreach (Light light in allLights)
        {
            string status = light.enabled ? "✓ ACTIVE" : "✗ Disabled";
            Debug.Log($"\n{status} | {light.type} | {light.name}");
            Debug.Log($"  Color: {light.color} | Intensity: {light.intensity}");
            
            if (light.type == LightType.Directional)
            {
                directionalCount++;
                Debug.Log($"  Rotation: {light.transform.rotation.eulerAngles}");
                Debug.Log($"  Shadows: {light.shadows}");
            }
            else if (light.type == LightType.Spot)
            {
                spotCount++;
                Debug.Log($"  Position: {light.transform.position}");
                Debug.Log($"  Range: {light.range} | Angle: {light.spotAngle}");
            }
            else if (light.type == LightType.Point)
            {
                pointCount++;
                Debug.Log($"  Position: {light.transform.position}");
                Debug.Log($"  Range: {light.range}");
            }
        }
        
        Debug.Log($"\n--- Summary ---");
        Debug.Log($"Directional: {directionalCount} (should have 2: Sun + Fill)");
        Debug.Log($"Spot: {spotCount} (should have 1: Character Light)");
        Debug.Log($"Point: {pointCount}");
        
        Debug.Log($"\n--- Environment ---");
        Debug.Log($"Ambient Mode: {RenderSettings.ambientMode} (should be Skybox)");
        Debug.Log($"Ambient Intensity: {RenderSettings.ambientIntensity}");
        Debug.Log($"Skybox: {(RenderSettings.skybox != null ? RenderSettings.skybox.name : "⚠️ NONE - Add one!")}");
        
        Debug.Log($"\nFog: {(RenderSettings.fog ? "✓ Enabled" : "✗ Disabled")}");
        if (RenderSettings.fog)
        {
            Debug.Log($"  Color: {RenderSettings.fogColor}");
            Debug.Log($"  Density: {RenderSettings.fogDensity}");
        }
        
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Debug.Log($"\n--- Main Camera ---");
            Debug.Log($"HDR: {(mainCam.allowHDR ? "✓ Enabled" : "✗ Disabled")}");
            Debug.Log($"MSAA: {(mainCam.allowMSAA ? "✓ Enabled" : "✗ Disabled")}");
        }
    }
}

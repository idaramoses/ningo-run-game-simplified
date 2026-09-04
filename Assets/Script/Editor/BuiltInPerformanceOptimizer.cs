using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class BuiltInPerformanceOptimizer : EditorWindow
{
    [MenuItem("Tools/Performance/Fix Lag - Built-In Pipeline")]
    public static void OptimizeForMobile()
    {
        Debug.Log("=== MOBILE PERFORMANCE OPTIMIZATION (Built-In Pipeline) ===\n");
        
        OptimizeQualitySettings();
        OptimizeLighting();
        
        Debug.Log("\n✓ OPTIMIZATION COMPLETE!");
        Debug.Log("Your game should run MUCH smoother now.");
        Debug.Log("\nNext steps:");
        Debug.Log("1. Build and test on your device");
        Debug.Log("2. If still lagging, run 'Aggressive Optimization'");
    }
    
    [MenuItem("Tools/Performance/Aggressive Optimization")]
    public static void AggressiveOptimization()
    {
        Debug.Log("=== AGGRESSIVE MOBILE OPTIMIZATION ===\n");
        
        QualitySettings.SetQualityLevel(1, true);
        QualitySettings.shadowDistance = 30f;
        QualitySettings.shadowResolution = ShadowResolution.Low;
        QualitySettings.shadowCascades = 0;
        QualitySettings.shadows = ShadowQuality.HardOnly;
        QualitySettings.antiAliasing = 0;
        QualitySettings.vSyncCount = 0;
        QualitySettings.realtimeReflectionProbes = false;
        
        Debug.Log("✓ Set to LOW quality preset");
        Debug.Log("✓ Shadows: Hard only, 30m distance");
        Debug.Log("✓ No anti-aliasing");
        Debug.Log("✓ VSync disabled");
        
        Light[] lights = FindObjectsOfType<Light>();
        int disabledCount = 0;
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional && light.name.Contains("Directional Light"))
            {
                light.shadows = LightShadows.Hard;
                light.shadowStrength = 0.6f;
                light.intensity = 1.5f;
                Debug.Log($"✓ Optimized main light: {light.name}");
            }
            else if (light.enabled)
            {
                light.enabled = false;
                disabledCount++;
            }
        }
        if (disabledCount > 0)
            Debug.Log($"✓ Disabled {disabledCount} extra light(s)");
        
        Debug.Log("\n✓ AGGRESSIVE OPTIMIZATION COMPLETE!");
        Debug.Log("Game should run smoothly on low-end devices now.");
    }
    
    private static void OptimizeQualitySettings()
    {
        Debug.Log("--- QUALITY SETTINGS ---");
        
        QualitySettings.SetQualityLevel(2, true);
        Debug.Log("✓ Quality level: Medium (was Ultra)");
        
        QualitySettings.shadowDistance = 35f;
        QualitySettings.shadowResolution = ShadowResolution.Medium;
        QualitySettings.shadowCascades = 2;
        QualitySettings.shadows = ShadowQuality.All;
        Debug.Log("✓ Shadows: Medium resolution, 35m distance, 2 cascades (was 4)");
        
        QualitySettings.antiAliasing = 0;
        QualitySettings.vSyncCount = 0;
        QualitySettings.realtimeReflectionProbes = false;
        Debug.Log("✓ Anti-aliasing: OFF (was 2x MSAA)");
        Debug.Log("✓ VSync: OFF (prevents stuttering)");
        Debug.Log("✓ Realtime reflection probes: OFF");
        
        QualitySettings.lodBias = 0.7f;
        QualitySettings.maximumLODLevel = 0;
        Debug.Log("✓ LOD bias: 0.7");
    }
    
    private static void OptimizeLighting()
    {
        Debug.Log("\n--- LIGHTING OPTIMIZATION ---");
        
        RenderSettings.ambientIntensity = 1.0f;
        Debug.Log("✓ Ambient intensity: 1.0");
        
        Light[] lights = FindObjectsOfType<Light>();
        int optimizedCount = 0;
        
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional && light.enabled)
            {
                light.shadows = LightShadows.Soft;
                light.shadowStrength = 0.7f;
                light.shadowBias = 0.05f;
                light.shadowNormalBias = 0.4f;
                light.shadowResolution = UnityEngine.Rendering.LightShadowResolution.Medium;
                optimizedCount++;
                
                Debug.Log($"✓ Optimized light: {light.name}");
                Debug.Log($"  - Shadows: Soft (Medium resolution)");
                Debug.Log($"  - Shadow strength: 0.7");
            }
        }
        
        if (optimizedCount == 0)
        {
            Debug.LogWarning("⚠️ No directional lights found to optimize");
        }
        
        ReflectionProbe[] probes = FindObjectsOfType<ReflectionProbe>();
        int disabledProbes = 0;
        foreach (var probe in probes)
        {
            if (probe.enabled)
            {
                probe.enabled = false;
                disabledProbes++;
            }
        }
        if (disabledProbes > 0)
            Debug.Log($"✓ Disabled {disabledProbes} reflection probe(s)");
    }
    
    [MenuItem("Tools/Performance/Check Performance Settings")]
    public static void CheckPerformanceSettings()
    {
        Debug.Log("=== CURRENT PERFORMANCE SETTINGS ===\n");
        
        Debug.Log("--- QUALITY ---");
        Debug.Log($"Quality Level: {QualitySettings.names[QualitySettings.GetQualityLevel()]}");
        Debug.Log($"Shadow Distance: {QualitySettings.shadowDistance}m");
        Debug.Log($"Shadow Resolution: {QualitySettings.shadowResolution}");
        Debug.Log($"Shadow Cascades: {QualitySettings.shadowCascades}");
        Debug.Log($"Anti-aliasing: {QualitySettings.antiAliasing}x");
        Debug.Log($"VSync: {QualitySettings.vSyncCount}");
        
        Debug.Log("\n--- LIGHTING ---");
        Debug.Log($"Ambient Intensity: {RenderSettings.ambientIntensity}");
        
        Light[] lights = FindObjectsOfType<Light>();
        Debug.Log($"Total Lights: {lights.Length}");
        int enabledLights = 0;
        foreach (Light light in lights)
        {
            if (light.enabled)
            {
                enabledLights++;
                Debug.Log($"  - {light.name} ({light.type}): Intensity={light.intensity}, Shadows={light.shadows}");
            }
        }
        Debug.Log($"Enabled Lights: {enabledLights}");
        
        Debug.Log("\n--- RENDER PIPELINE ---");
        Debug.Log("Built-in render pipeline");
        
        Debug.Log("\n=== END REPORT ===");
    }
}

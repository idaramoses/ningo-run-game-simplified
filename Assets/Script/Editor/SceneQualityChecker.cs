using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

public class SceneQualityChecker : EditorWindow
{
    [MenuItem("Tools/Fix Lighting/Check Scene Quality Settings")]
    public static void CheckQualitySettings()
    {
        Debug.Log("=== SCENE QUALITY & LIGHTING CHECK ===\n");
        
        // Check Lighting Settings
        Debug.Log("--- LIGHTING SETTINGS ---");
        Debug.Log($"Ambient Mode: {RenderSettings.ambientMode}");
        Debug.Log($"Ambient Intensity: {RenderSettings.ambientIntensity}");
        Debug.Log($"Ambient Sky Color: {RenderSettings.ambientSkyColor}");
        Debug.Log($"Reflection Intensity: {RenderSettings.reflectionIntensity}");
        Debug.Log($"Fog Enabled: {RenderSettings.fog}");
        
        // Check Shadow Settings
        Debug.Log("\n--- SHADOW SETTINGS ---");
        Debug.Log($"Shadow Distance: {QualitySettings.shadowDistance}");
        Debug.Log($"Shadow Resolution: {QualitySettings.shadowResolution}");
        Debug.Log($"Shadow Projection: {QualitySettings.shadowProjection}");
        Debug.Log($"Shadow Cascades: {QualitySettings.shadowCascades}");
        
        // Check for Reflection Probes
        ReflectionProbe[] probes = FindObjectsOfType<ReflectionProbe>();
        Debug.Log($"\n--- REFLECTION PROBES ---");
        Debug.Log($"Total Reflection Probes: {probes.Length}");
        foreach (var probe in probes)
        {
            Debug.Log($"  - {probe.name}: Intensity={probe.intensity}, Mode={probe.mode}");
        }
        
        // Check for Light Probes
        LightProbeGroup[] lightProbeGroups = FindObjectsOfType<LightProbeGroup>();
        Debug.Log($"\n--- LIGHT PROBE GROUPS ---");
        Debug.Log($"Total Light Probe Groups: {lightProbeGroups.Length}");
        
        // Check Camera Settings
        Camera[] cameras = FindObjectsOfType<Camera>();
        Debug.Log($"\n--- CAMERAS ---");
        Debug.Log($"Total Cameras: {cameras.Length}");
        foreach (var cam in cameras)
        {
            string status = cam.enabled ? "ENABLED" : "disabled";
            Debug.Log($"  [{status}] {cam.name}: HDR={cam.allowHDR}, MSAA={cam.allowMSAA}");
        }
        
        Debug.Log("\n=== END REPORT ===");
    }
    
    [MenuItem("Tools/Fix Lighting/Optimize Lighting for Mobile")]
    public static void OptimizeLightingForMobile()
    {
        Debug.Log("=== OPTIMIZING LIGHTING FOR MOBILE ===\n");
        
        // Optimize shadow settings
        QualitySettings.shadowDistance = 50f;
        QualitySettings.shadowResolution = ShadowResolution.Medium;
        QualitySettings.shadowCascades = 2;
        Debug.Log("✓ Optimized shadow settings for mobile");
        
        // Adjust ambient lighting
        RenderSettings.ambientIntensity = 1.0f;
        Debug.Log("✓ Set ambient intensity to 1.0");
        
        // Find and adjust directional light
        Light[] lights = FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional && light.enabled)
            {
                light.shadows = LightShadows.Soft;
                light.shadowStrength = 0.8f;
                light.shadowBias = 0.05f;
                Debug.Log($"✓ Optimized directional light: {light.name}");
            }
        }
        
        // Disable reflection probes for performance
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
            Debug.Log($"✓ Disabled {disabledProbes} reflection probe(s) for better performance");
        
        Debug.Log("\n✓ Mobile lighting optimization complete!");
    }
}

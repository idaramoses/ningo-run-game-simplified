using UnityEngine;
using UnityEditor;

#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

public class PostProcessingSetupHelper : EditorWindow
{
    [MenuItem("Tools/Lighting/Post Processing Setup Guide")]
    public static void ShowSetupGuide()
    {
        Debug.Log("\n=== POST PROCESSING SETUP GUIDE ===\n");
        
        #if UNITY_POST_PROCESSING_STACK_V2
        Debug.Log("✓ Post Processing Stack v2 is installed!");
        Debug.Log("\nTo complete your sunset lighting setup:\n");
        #else
        Debug.Log("⚠️ Post Processing Stack v2 not detected.");
        Debug.Log("\nTo install Post Processing:\n");
        Debug.Log("1. Open Window → Package Manager");
        Debug.Log("2. Search for 'Post Processing'");
        Debug.Log("3. Click Install\n");
        Debug.Log("After installation, follow these steps:\n");
        #endif
        
        Debug.Log("STEP 1: Create Post Processing Volume");
        Debug.Log("  - Right-click in Hierarchy");
        Debug.Log("  - Select 'Create → Volume → Post-process Volume' (or Global Volume for URP)");
        Debug.Log("  - Check 'Is Global' in Inspector\n");
        
        Debug.Log("STEP 2: Create Profile");
        Debug.Log("  - Click 'New' next to Profile");
        Debug.Log("  - Save as 'SunsetPostProcessing.asset'\n");
        
        Debug.Log("STEP 3: Add Effects (Click 'Add effect...')");
        Debug.Log("\n📊 Color Grading:");
        Debug.Log("  - Temperature: +15 (warmer)");
        Debug.Log("  - Tint: +5 (slight magenta)");
        Debug.Log("  - Saturation: +15 (vibrant)");
        Debug.Log("  - Contrast: +10 (pop)");
        Debug.Log("  - Post Exposure: +0.3 (brighter)");
        
        Debug.Log("\n✨ Bloom:");
        Debug.Log("  - Intensity: 0.3");
        Debug.Log("  - Threshold: 1.0");
        Debug.Log("  - Soft Knee: 0.5");
        Debug.Log("  - Diffusion: 7");
        
        Debug.Log("\n🎥 Vignette:");
        Debug.Log("  - Intensity: 0.2");
        Debug.Log("  - Smoothness: 0.4");
        Debug.Log("  - Rounded: true");
        
        Debug.Log("\n🎨 Tonemapping:");
        Debug.Log("  - Mode: ACES (cinematic)");
        
        Debug.Log("\nSTEP 4: Enable on Camera");
        Debug.Log("  - Select Main Camera");
        Debug.Log("  - Add 'Post-process Layer' component");
        Debug.Log("  - Set Layer to 'Everything' or create 'PostProcessing' layer");
        Debug.Log("  - Enable Anti-aliasing (FXAA or SMAA)\n");
        
        Debug.Log("=== DONE! Your game will look cinematic! ===\n");
    }
    
    [MenuItem("Tools/Lighting/Check Post Processing Status")]
    public static void CheckPostProcessingStatus()
    {
        Debug.Log("\n=== POST PROCESSING STATUS ===\n");
        
        #if UNITY_POST_PROCESSING_STACK_V2
        Debug.Log("✓ Post Processing Stack v2: INSTALLED");
        
        var volumes = FindObjectsOfType<PostProcessVolume>();
        Debug.Log($"\nPost Process Volumes in scene: {volumes.Length}");
        
        foreach (var volume in volumes)
        {
            Debug.Log($"  - {volume.name}");
            Debug.Log($"    Is Global: {volume.isGlobal}");
            Debug.Log($"    Profile: {(volume.profile != null ? volume.profile.name : "None")}");
            Debug.Log($"    Weight: {volume.weight}");
        }
        
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            var ppLayer = mainCam.GetComponent<PostProcessLayer>();
            if (ppLayer != null)
            {
                Debug.Log("\n✓ Main Camera has Post Process Layer");
                Debug.Log($"  Anti-aliasing: {ppLayer.antialiasingMode}");
            }
            else
            {
                Debug.LogWarning("\n⚠️ Main Camera missing Post Process Layer component!");
            }
        }
        #else
        Debug.LogWarning("✗ Post Processing Stack v2: NOT INSTALLED");
        Debug.Log("\nInstall via Window → Package Manager");
        #endif
    }
    
    [MenuItem("Tools/Lighting/Apply Recommended Settings")]
    public static void ApplyRecommendedSettings()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("No Main Camera found in scene!");
            return;
        }
        
        mainCam.clearFlags = CameraClearFlags.Skybox;
        mainCam.backgroundColor = new Color(1.0f, 0.8f, 0.6f);
        mainCam.farClipPlane = 1000f;
        mainCam.allowHDR = true;
        mainCam.allowMSAA = true;
        
        EditorUtility.SetDirty(mainCam);
        
        Debug.Log("✓ Applied recommended camera settings");
        Debug.Log("  - HDR: Enabled");
        Debug.Log("  - MSAA: Enabled");
        Debug.Log("  - Background: Warm sunset color");
    }
}

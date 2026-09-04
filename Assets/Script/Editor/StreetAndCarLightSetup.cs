using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class StreetAndCarLightSetup : EditorWindow
{
    [MenuItem("Tools/Lighting/💡 Add Lights to Streetlights")]
    public static void AddStreetLights()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int count = 0;
        
        Debug.Log("=== ADDING STREETLIGHT POINT LIGHTS ===\n");
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains("street") && obj.name.ToLower().Contains("light"))
            {
                StreetLightController controller = obj.GetComponent<StreetLightController>();
                
                if (controller == null)
                {
                    controller = obj.AddComponent<StreetLightController>();
                }
                
                controller.SetupStreetLight();
                EditorUtility.SetDirty(obj);
                count++;
                
                Debug.Log($"✓ Added light to: {obj.name}");
            }
        }
        
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        
        Debug.Log($"\n=== COMPLETE ===");
        Debug.Log($"Added lights to {count} streetlights");
        Debug.Log("\nStreetlight Settings:");
        Debug.Log("  - Type: Point Light");
        Debug.Log("  - Color: Warm white (1.0, 0.9, 0.7)");
        Debug.Log("  - Intensity: 2.0");
        Debug.Log("  - Range: 15 meters");
        Debug.Log("  - Shadows: Soft");
    }
    
    [MenuItem("Tools/Lighting/🚗 Add Lights to Car Headlights")]
    public static void AddCarHeadlights()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int count = 0;
        
        Debug.Log("=== ADDING CAR HEADLIGHTS ===\n");
        
        foreach (GameObject obj in allObjects)
        {
            string objName = obj.name.ToLower();
            if (objName.Contains("car") || objName.Contains("vehicle"))
            {
                if (obj.GetComponent<MeshRenderer>() != null || obj.GetComponentInChildren<MeshRenderer>() != null)
                {
                    CarHeadlightController controller = obj.GetComponent<CarHeadlightController>();
                    
                    if (controller == null)
                    {
                        controller = obj.AddComponent<CarHeadlightController>();
                    }
                    
                    controller.SetupHeadlights();
                    EditorUtility.SetDirty(obj);
                    count++;
                    
                    Debug.Log($"✓ Added headlights to: {obj.name}");
                }
            }
        }
        
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        
        Debug.Log($"\n=== COMPLETE ===");
        Debug.Log($"Added headlights to {count} cars");
        Debug.Log("\nHeadlight Settings:");
        Debug.Log("  - Type: Spot Light (2 per car)");
        Debug.Log("  - Color: Bright white (1.0, 0.95, 0.85)");
        Debug.Log("  - Intensity: 3.0");
        Debug.Log("  - Range: 20 meters");
        Debug.Log("  - Spot Angle: 60°");
        Debug.Log("  - Shadows: OFF (for performance)");
        Debug.Log("\n⚠️ Adjust positions in Inspector if needed!");
    }
    
    [MenuItem("Tools/Lighting/🌃 Setup All Scene Lights (Streetlights + Cars)")]
    public static void SetupAllSceneLights()
    {
        Debug.Log("=== SETTING UP ALL SCENE LIGHTS ===\n");
        
        AddStreetLights();
        Debug.Log("\n");
        AddCarHeadlights();
        
        Debug.Log("\n\n=== ALL SCENE LIGHTS CONFIGURED ===");
        Debug.Log("Your scene now has:");
        Debug.Log("  ✓ Streetlight point lights");
        Debug.Log("  ✓ Car headlight spot lights");
        Debug.Log("\nCheck Scene view to see the lighting!");
    }
    
    [MenuItem("Tools/Lighting/🔧 Adjust Streetlight Intensity")]
    public static void ShowStreetlightIntensityWindow()
    {
        StreetlightIntensityWindow.ShowWindow();
    }
}

public class StreetlightIntensityWindow : EditorWindow
{
    private float streetlightIntensity = 2.0f;
    private float streetlightRange = 15f;
    private float carHeadlightIntensity = 3.0f;
    private float carHeadlightRange = 20f;
    
    public static void ShowWindow()
    {
        GetWindow<StreetlightIntensityWindow>("Light Intensity");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Scene Light Intensity Adjuster", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox("Adjust intensity for all streetlights and car headlights in the scene.", MessageType.Info);
        GUILayout.Space(10);
        
        GUILayout.Label("💡 Streetlights", EditorStyles.boldLabel);
        streetlightIntensity = EditorGUILayout.Slider("Intensity", streetlightIntensity, 0.5f, 5.0f);
        streetlightRange = EditorGUILayout.Slider("Range", streetlightRange, 5f, 30f);
        
        if (GUILayout.Button("Apply to All Streetlights"))
        {
            ApplyToStreetlights();
        }
        
        GUILayout.Space(15);
        GUILayout.Label("🚗 Car Headlights", EditorStyles.boldLabel);
        carHeadlightIntensity = EditorGUILayout.Slider("Intensity", carHeadlightIntensity, 1.0f, 8.0f);
        carHeadlightRange = EditorGUILayout.Slider("Range", carHeadlightRange, 10f, 40f);
        
        if (GUILayout.Button("Apply to All Car Headlights"))
        {
            ApplyToCarHeadlights();
        }
        
        GUILayout.Space(15);
        
        if (GUILayout.Button("Turn All Lights ON"))
        {
            ToggleAllLights(true);
        }
        
        if (GUILayout.Button("Turn All Lights OFF"))
        {
            ToggleAllLights(false);
        }
    }
    
    private void ApplyToStreetlights()
    {
        StreetLightController[] streetlights = FindObjectsOfType<StreetLightController>();
        
        foreach (StreetLightController light in streetlights)
        {
            light.SetIntensity(streetlightIntensity);
            EditorUtility.SetDirty(light);
        }
        
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"✓ Updated {streetlights.Length} streetlights");
    }
    
    private void ApplyToCarHeadlights()
    {
        CarHeadlightController[] headlights = FindObjectsOfType<CarHeadlightController>();
        
        foreach (CarHeadlightController light in headlights)
        {
            light.SetIntensity(carHeadlightIntensity);
            EditorUtility.SetDirty(light);
        }
        
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"✓ Updated {headlights.Length} car headlights");
    }
    
    private void ToggleAllLights(bool on)
    {
        StreetLightController[] streetlights = FindObjectsOfType<StreetLightController>();
        CarHeadlightController[] headlights = FindObjectsOfType<CarHeadlightController>();
        
        foreach (StreetLightController light in streetlights)
        {
            if (on) light.TurnOn();
            else light.TurnOff();
            EditorUtility.SetDirty(light);
        }
        
        foreach (CarHeadlightController light in headlights)
        {
            if (on) light.TurnOn();
            else light.TurnOff();
            EditorUtility.SetDirty(light);
        }
        
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"✓ Turned {(on ? "ON" : "OFF")} all scene lights");
    }
}

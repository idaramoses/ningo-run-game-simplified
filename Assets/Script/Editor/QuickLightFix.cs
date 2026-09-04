using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class QuickLightFix
{
    [MenuItem("Tools/Lighting/⚡ Quick Fix Selected Light")]
    public static void QuickFixSelectedLight()
    {
        GameObject selected = Selection.activeGameObject;
        
        if (selected == null)
        {
            Debug.LogError("Please select a streetlight object first!");
            return;
        }
        
        Debug.Log($"=== FIXING LIGHT ON: {selected.name} ===\n");
        
        // Get or add StreetLightController
        StreetLightController controller = selected.GetComponent<StreetLightController>();
        if (controller == null)
        {
            controller = selected.AddComponent<StreetLightController>();
            Debug.Log("✓ Added StreetLightController");
        }
        
        // Find or create Light_Position child
        Transform lightPos = selected.transform.Find("Light_Position");
        if (lightPos == null)
        {
            GameObject lightPosObj = new GameObject("Light_Position");
            lightPosObj.transform.SetParent(selected.transform);
            lightPosObj.transform.localPosition = new Vector3(0, 5, 0); // Default height
            lightPos = lightPosObj.transform;
            Debug.Log("✓ Created Light_Position child object");
        }
        
        // Get or add Light component to Light_Position
        Light light = lightPos.GetComponent<Light>();
        if (light == null)
        {
            light = lightPos.gameObject.AddComponent<Light>();
            Debug.Log("✓ Added Light component");
        }
        
        // Configure the light
        light.type = LightType.Point;
        light.color = new Color(1.0f, 0.9f, 0.7f); // Warm white
        light.intensity = 3.0f; // Boosted for visibility
        light.range = 20f; // Increased range
        light.shadows = LightShadows.Soft;
        light.enabled = true; // ENABLE IT!
        
        Debug.Log("✓ Configured Light:");
        Debug.Log($"  - Type: Point");
        Debug.Log($"  - Intensity: 3.0");
        Debug.Log($"  - Range: 20");
        Debug.Log($"  - Color: Warm White");
        Debug.Log($"  - Enabled: TRUE");
        
        // Assign lightPosition to controller
        SerializedObject so = new SerializedObject(controller);
        SerializedProperty lightPositionProp = so.FindProperty("lightPosition");
        lightPositionProp.objectReferenceValue = lightPos;
        so.ApplyModifiedProperties();
        
        Debug.Log("✓ Linked Light_Position to controller");
        
        EditorUtility.SetDirty(controller);
        EditorUtility.SetDirty(light);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );
        
        // Select the Light_Position so user can adjust position
        Selection.activeGameObject = lightPos.gameObject;
        SceneView.lastActiveSceneView.FrameSelected();
        
        Debug.Log("\n=== DONE! ===");
        Debug.Log("Now drag the 'Light_Position' object to the bulb location in Scene view!");
        Debug.Log("The light should now be visible and emitting!");
    }
    
    [MenuItem("Tools/Lighting/🔥 Fix ALL Streetlights in Scene")]
    public static void FixAllStreetlights()
    {
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
        int fixedCount = 0;
        
        Debug.Log("=== FIXING ALL STREETLIGHTS ===\n");
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains("street") && obj.name.ToLower().Contains("light"))
            {
                // Save current selection
                GameObject previousSelection = Selection.activeGameObject;
                
                // Select and fix this object
                Selection.activeGameObject = obj;
                QuickFixSelectedLight();
                
                fixedCount++;
            }
        }
        
        Debug.Log($"\n=== COMPLETE ===");
        Debug.Log($"Fixed {fixedCount} streetlights!");
        Debug.Log("All lights should now be emitting!");
    }
}

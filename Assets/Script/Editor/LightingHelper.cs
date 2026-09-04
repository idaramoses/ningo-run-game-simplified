using UnityEngine;
using UnityEditor;

public class LightingHelper : EditorWindow
{
    [MenuItem("Tools/Fix Lighting/Find Duplicate Lights")]
    public static void FindDuplicateLights()
    {
        Light[] allLights = FindObjectsOfType<Light>();
        
        Debug.Log($"=== LIGHTING REPORT ===");
        Debug.Log($"Total lights in scene: {allLights.Length}");
        
        int directionalCount = 0;
        int pointCount = 0;
        int spotCount = 0;
        
        foreach (Light light in allLights)
        {
            string status = light.enabled ? "ENABLED" : "disabled";
            string path = GetGameObjectPath(light.gameObject);
            
            switch (light.type)
            {
                case LightType.Directional:
                    directionalCount++;
                    Debug.Log($"[{status}] Directional Light: {path} | Intensity: {light.intensity} | Color: {light.color}");
                    break;
                case LightType.Point:
                    pointCount++;
                    Debug.Log($"[{status}] Point Light: {path} | Intensity: {light.intensity}");
                    break;
                case LightType.Spot:
                    spotCount++;
                    Debug.Log($"[{status}] Spot Light: {path} | Intensity: {light.intensity}");
                    break;
            }
        }
        
        Debug.Log($"\n=== SUMMARY ===");
        Debug.Log($"Directional Lights: {directionalCount}");
        Debug.Log($"Point Lights: {pointCount}");
        Debug.Log($"Spot Lights: {spotCount}");
        
        if (directionalCount > 1)
        {
            Debug.LogWarning($"⚠️ WARNING: Found {directionalCount} Directional Lights! You should only have 1 sun/directional light.");
            Debug.LogWarning("Recommendation: Disable or delete extra directional lights.");
        }
    }
    
    [MenuItem("Tools/Fix Lighting/Disable Extra Directional Lights")]
    public static void DisableExtraDirectionalLights()
    {
        Light[] allLights = FindObjectsOfType<Light>();
        Light mainDirectional = null;
        int disabledCount = 0;
        
        // Find the first enabled directional light
        foreach (Light light in allLights)
        {
            if (light.type == LightType.Directional && light.enabled)
            {
                if (mainDirectional == null)
                {
                    mainDirectional = light;
                    Debug.Log($"✓ Keeping main directional light: {GetGameObjectPath(light.gameObject)}");
                }
                else
                {
                    light.enabled = false;
                    disabledCount++;
                    Debug.Log($"✗ Disabled extra directional light: {GetGameObjectPath(light.gameObject)}");
                }
            }
        }
        
        if (disabledCount > 0)
        {
            Debug.Log($"\n✓ Disabled {disabledCount} extra directional light(s)");
            EditorUtility.SetDirty(mainDirectional.gameObject);
        }
        else
        {
            Debug.Log("No extra directional lights found.");
        }
    }
    
    private static string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform current = obj.transform.parent;
        
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }
        
        return path;
    }
}

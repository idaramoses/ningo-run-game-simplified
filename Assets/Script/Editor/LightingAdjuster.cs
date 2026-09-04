using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class LightingAdjuster : EditorWindow
{
    private float ambientIntensity = 1.0f;
    private float skyboxExposure = 1.0f;
    private float directionalIntensity = 1.2f;
    private float fillLightIntensity = 0.3f;
    
    [MenuItem("Tools/Lighting/⚙️ Adjust Lighting Intensity")]
    public static void ShowWindow()
    {
        GetWindow<LightingAdjuster>("Lighting Adjuster");
    }
    
    void OnEnable()
    {
        ambientIntensity = RenderSettings.ambientIntensity;
        
        Material skybox = RenderSettings.skybox;
        if (skybox != null && skybox.HasProperty("_Exposure"))
        {
            skyboxExposure = skybox.GetFloat("_Exposure");
        }
        
        Light[] lights = FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                if (light.name.Contains("Sun") || light.name.Contains("Directional Light"))
                {
                    directionalIntensity = light.intensity;
                }
                else if (light.name.Contains("Fill"))
                {
                    fillLightIntensity = light.intensity;
                }
            }
        }
    }
    
    void OnGUI()
    {
        GUILayout.Label("Sky Lighting Adjustment", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox("Adjust these sliders to control lighting intensity in real-time.", MessageType.Info);
        GUILayout.Space(10);
        
        GUILayout.Label("🌤️ Sky & Ambient", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        ambientIntensity = EditorGUILayout.Slider("Ambient Intensity", ambientIntensity, 0.5f, 2.0f);
        if (EditorGUI.EndChangeCheck())
        {
            RenderSettings.ambientIntensity = ambientIntensity;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
        
        Material skybox = RenderSettings.skybox;
        if (skybox != null && skybox.HasProperty("_Exposure"))
        {
            EditorGUI.BeginChangeCheck();
            skyboxExposure = EditorGUILayout.Slider("Skybox Exposure", skyboxExposure, 0.5f, 2.0f);
            if (EditorGUI.EndChangeCheck())
            {
                skybox.SetFloat("_Exposure", skyboxExposure);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
        
        GUILayout.Space(15);
        GUILayout.Label("☀️ Lights", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        directionalIntensity = EditorGUILayout.Slider("Sun Light Intensity", directionalIntensity, 0.5f, 2.0f);
        if (EditorGUI.EndChangeCheck())
        {
            UpdateDirectionalLight(directionalIntensity);
        }
        
        EditorGUI.BeginChangeCheck();
        fillLightIntensity = EditorGUILayout.Slider("Fill Light Intensity", fillLightIntensity, 0.1f, 0.8f);
        if (EditorGUI.EndChangeCheck())
        {
            UpdateFillLight(fillLightIntensity);
        }
        
        GUILayout.Space(15);
        
        if (GUILayout.Button("Reset to Default Values"))
        {
            ambientIntensity = 1.0f;
            skyboxExposure = 1.0f;
            directionalIntensity = 1.2f;
            fillLightIntensity = 0.3f;
            
            RenderSettings.ambientIntensity = ambientIntensity;
            if (skybox != null && skybox.HasProperty("_Exposure"))
            {
                skybox.SetFloat("_Exposure", skyboxExposure);
            }
            UpdateDirectionalLight(directionalIntensity);
            UpdateFillLight(fillLightIntensity);
            
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
        
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "💡 Tips:\n" +
            "• Lower Ambient/Skybox = Darker overall scene\n" +
            "• Lower Sun = Softer shadows\n" +
            "• Lower Fill = More contrast",
            MessageType.None
        );
    }
    
    private void UpdateDirectionalLight(float intensity)
    {
        Light[] lights = FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional && 
                (light.name.Contains("Sun") || light.name.Contains("Directional Light")) &&
                !light.name.Contains("Fill"))
            {
                light.intensity = intensity;
                EditorUtility.SetDirty(light);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                break;
            }
        }
    }
    
    private void UpdateFillLight(float intensity)
    {
        Light[] lights = FindObjectsOfType<Light>();
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional && light.name.Contains("Fill"))
            {
                light.intensity = intensity;
                EditorUtility.SetDirty(light);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                break;
            }
        }
    }
}

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StreetLightController))]
public class StreetLightControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        StreetLightController controller = (StreetLightController)target;
        
        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "SETUP STEPS:\n" +
            "1. Click 'Create Light Position' button below\n" +
            "2. In Scene view, drag the new child object to the bulb location\n" +
            "3. Click Play to see the light",
            MessageType.Info
        );
        
        if (GUILayout.Button("Create Light Position at Bulb", GUILayout.Height(30)))
        {
            CreateLightPosition(controller);
        }
        
        GUILayout.Space(5);
        
        if (GUILayout.Button("Setup Light Now (Editor Mode)"))
        {
            controller.SetupStreetLight();
            EditorUtility.SetDirty(controller);
        }
    }
    
    private void CreateLightPosition(StreetLightController controller)
    {
        GameObject lightPos = new GameObject("Light_Position");
        lightPos.transform.SetParent(controller.transform);
        lightPos.transform.localPosition = Vector3.zero;
        
        SerializedObject so = new SerializedObject(controller);
        SerializedProperty lightPositionProp = so.FindProperty("lightPosition");
        lightPositionProp.objectReferenceValue = lightPos.transform;
        so.ApplyModifiedProperties();
        
        Selection.activeGameObject = lightPos;
        SceneView.lastActiveSceneView.FrameSelected();
        
        Debug.Log($"✓ Created Light_Position child object. Now drag it to the bulb location in Scene view!");
        EditorUtility.SetDirty(controller);
    }
}

[CustomEditor(typeof(CarHeadlightController))]
public class CarHeadlightControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        CarHeadlightController controller = (CarHeadlightController)target;
        
        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "SETUP STEPS:\n" +
            "1. Adjust 'Left/Right Headlight Position' values in Inspector\n" +
            "2. Click 'Setup Headlights Now' to see preview\n" +
            "3. Adjust positions until lights are at headlight locations",
            MessageType.Info
        );
        
        if (GUILayout.Button("Setup Headlights Now (Editor Mode)", GUILayout.Height(30)))
        {
            controller.SetupHeadlights();
            EditorUtility.SetDirty(controller);
        }
    }
}

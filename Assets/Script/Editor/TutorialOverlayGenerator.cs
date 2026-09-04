using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class TutorialOverlayGenerator
{
    [MenuItem("Tools/Ningo Run/Generate Tutorial Overlay Prefab")]
    public static void GenerateTutorialOverlay()
    {
        string prefabPath = "Assets/Resources/TutorialOverlay.prefab";
        
        // Check if prefab already exists
        if (System.IO.File.Exists(prefabPath))
        {
            if (!EditorUtility.DisplayDialog("Prefab Exists", 
                "TutorialOverlay.prefab already exists. Do you want to overwrite it?", 
                "Overwrite", "Cancel"))
            {
                return;
            }
        }

        // Create the root Canvas
        GameObject canvasGO = new GameObject("TutorialOverlay");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // Create Dim background
        GameObject dimGO = new GameObject("Dim");
        dimGO.transform.SetParent(canvasGO.transform, false);
        RectTransform dimRT = dimGO.AddComponent<RectTransform>();
        dimRT.anchorMin = Vector2.zero;
        dimRT.anchorMax = Vector2.one;
        dimRT.offsetMin = Vector2.zero;
        dimRT.offsetMax = Vector2.zero;
        Image dimImage = dimGO.AddComponent<Image>();
        dimImage.color = new Color(0f, 0f, 0f, 0.5f);

        // Create HandIndicator
        GameObject handGO = new GameObject("HandIndicator");
        handGO.transform.SetParent(canvasGO.transform, false);
        RectTransform handRT = handGO.AddComponent<RectTransform>();
        handRT.anchorMin = new Vector2(0.5f, 0.55f);
        handRT.anchorMax = new Vector2(0.5f, 0.55f);
        handRT.sizeDelta = new Vector2(300f, 130f);
        handRT.anchoredPosition = Vector2.zero;
        TextMeshProUGUI handLabel = handGO.AddComponent<TextMeshProUGUI>();
        handLabel.fontSize = 90f;
        handLabel.alignment = TextAlignmentOptions.Center;
        handLabel.color = Color.white;
        handLabel.text = "←";

        // Create HintBar
        GameObject barGO = new GameObject("HintBar");
        barGO.transform.SetParent(canvasGO.transform, false);
        RectTransform barRT = barGO.AddComponent<RectTransform>();
        barRT.anchorMin = new Vector2(0f, 0f);
        barRT.anchorMax = new Vector2(1f, 0f);
        barRT.pivot = new Vector2(0.5f, 0f);
        barRT.anchoredPosition = new Vector2(0f, 140f);
        barRT.sizeDelta = new Vector2(0f, 120f);
        Image barImage = barGO.AddComponent<Image>();
        barImage.color = new Color(0f, 0f, 0f, 0.75f);

        // Create HintText
        GameObject textGO = new GameObject("HintText");
        textGO.transform.SetParent(barGO.transform, false);
        RectTransform textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(24f, 6f);
        textRT.offsetMax = new Vector2(-24f, -6f);
        TextMeshProUGUI hintLabel = textGO.AddComponent<TextMeshProUGUI>();
        hintLabel.fontSize = 38f;
        hintLabel.alignment = TextAlignmentOptions.Center;
        hintLabel.color = Color.white;
        hintLabel.enableWordWrapping = true;
        hintLabel.text = "Swipe to move left/right";

        // Ensure directory exists
        string directory = System.IO.Path.GetDirectoryName(prefabPath);
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        // Save as prefab
        PrefabUtility.SaveAsPrefabAsset(canvasGO, prefabPath);
        
        // Add instance to current scene
        GameObject instance = PrefabUtility.InstantiatePrefab(canvasGO) as GameObject;
        instance.name = "TutorialOverlay (Instance)";
        instance.SetActive(false);

        // Destroy the temporary GameObject
        Object.DestroyImmediate(canvasGO);

        Debug.Log($"[TutorialOverlayGenerator] Created prefab at {prefabPath} and added instance to scene");
    }
}

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor utility that generates simple primitive-based placeholder prefabs for the
/// road theme conversion (cars, truck, barriers, overpass supports, street light, road tiles).
/// Use these to test the road theme before final art is ready - just assign them into the
/// Makesupway component's roadTilePrefabs[] / obstaclePrefabs[] arrays.
/// </summary>
public static class RoadThemePlaceholderCreator
{
    private const string OutputFolder = "Assets/Prefap/RoadPlaceholders";

    [MenuItem("Tools/WikiCat Rush/Create Road Theme Placeholder Prefabs")]
    public static void CreatePlaceholders()
    {
        EnsureFolder(OutputFolder);

        // Road tiles: base + 3 variants, flat wide planes with slightly different tint
        CreateRoadTile("RoadTile_Base", new Color(0.25f, 0.25f, 0.25f));
        CreateRoadTile("RoadTile_Variant1", new Color(0.28f, 0.28f, 0.3f));
        CreateRoadTile("RoadTile_Variant2", new Color(0.3f, 0.26f, 0.26f));
        CreateRoadTile("RoadTile_Variant3", new Color(0.26f, 0.3f, 0.26f));

        // Cars / truck obstacles
        CreateBoxObstacle("Car_VariantA", new Vector3(1.8f, 1.2f, 4f), new Color(0.8f, 0.1f, 0.1f));
        CreateBoxObstacle("Car_VariantB", new Vector3(1.8f, 1.2f, 4f), new Color(0.1f, 0.3f, 0.8f));
        CreateBoxObstacle("Car_Oncoming", new Vector3(1.8f, 1.2f, 4f), new Color(0.9f, 0.5f, 0.05f));
        CreateBoxObstacle("Truck", new Vector3(2.2f, 2.2f, 6f), new Color(0.4f, 0.4f, 0.45f));

        // Barriers
        CreateBoxObstacle("Barrier_Low", new Vector3(2.5f, 0.5f, 0.3f), new Color(0.95f, 0.85f, 0.1f));
        CreateBoxObstacle("Barrier_High", new Vector3(2.5f, 1.4f, 0.3f), new Color(0.95f, 0.85f, 0.1f));
        CreateBoxObstacle("Barrier_ExtraHigh", new Vector3(2.5f, 2.2f, 0.3f), new Color(0.95f, 0.85f, 0.1f));

        // Overpass supports (pillars)
        CreateBoxObstacle("Overpass_Mid", new Vector3(1f, 7f, 1f), new Color(0.5f, 0.5f, 0.5f));
        CreateBoxObstacle("Overpass_LegLeft", new Vector3(1f, 7f, 1f), new Color(0.5f, 0.5f, 0.5f));
        CreateBoxObstacle("Overpass_LegRight", new Vector3(1f, 7f, 1f), new Color(0.5f, 0.5f, 0.5f));
        CreateBoxObstacle("Overpass_LegBoth", new Vector3(4f, 7f, 1f), new Color(0.5f, 0.5f, 0.5f));

        // Street light (pole + small head)
        CreateStreetLight("StreetLight");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[RoadThemePlaceholderCreator] Created placeholder prefabs in '{OutputFolder}'. " +
                  "Assign them to the Makesupway component's roadTilePrefabs[] / obstaclePrefabs[] arrays " +
                  "in the order shown in the array tooltips.");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        string[] parts = path.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }
            current = next;
        }
    }

    private static Material CreateColorMaterial(string name, Color color)
    {
        string matPath = $"{OutputFolder}/{name}_Mat.mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (existing != null) return existing;

        Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Diffuse");
        Material mat = new Material(shader);
        mat.color = color;
        AssetDatabase.CreateAsset(mat, matPath);
        return mat;
    }

    private static GameObject SavePrefab(GameObject go, string name)
    {
        string prefabPath = $"{OutputFolder}/{name}.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static void CreateBoxObstacle(string name, Vector3 size, Color color)
    {
        string prefabPath = $"{OutputFolder}/{name}.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null) return;

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.localScale = size;
        go.GetComponent<Renderer>().sharedMaterial = CreateColorMaterial(name, color);

        SavePrefab(go, name);
    }

    private static void CreateRoadTile(string name, Color color)
    {
        string prefabPath = $"{OutputFolder}/{name}.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null) return;

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        go.name = name;
        // Default Unity plane is 10x10 units; scale down/adjust to a typical road-tile size.
        go.transform.localScale = new Vector3(0.8f, 1f, 4f);
        go.GetComponent<Renderer>().sharedMaterial = CreateColorMaterial(name, color);

        SavePrefab(go, name);
    }

    private static void CreateStreetLight(string name)
    {
        string prefabPath = $"{OutputFolder}/{name}.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null) return;

        GameObject root = new GameObject(name);

        GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.name = "Pole";
        pole.transform.SetParent(root.transform);
        pole.transform.localPosition = new Vector3(0, 2.5f, 0);
        pole.transform.localScale = new Vector3(0.15f, 2.5f, 0.15f);
        pole.GetComponent<Renderer>().sharedMaterial = CreateColorMaterial("StreetLight_Pole", new Color(0.2f, 0.2f, 0.2f));

        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
        head.name = "Head";
        head.transform.SetParent(root.transform);
        head.transform.localPosition = new Vector3(0.4f, 5f, 0);
        head.transform.localScale = new Vector3(0.8f, 0.2f, 0.2f);
        head.GetComponent<Renderer>().sharedMaterial = CreateColorMaterial("StreetLight_Head", new Color(0.9f, 0.9f, 0.5f));

        SavePrefab(root, name);
    }
}
#endif

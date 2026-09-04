using UnityEngine;
using UnityEditor;

public static class WikiCatMaterialFix
{
    private const string PrefabPath = "Assets/Characters/WikiCat/Prefabs/WikiPrefab.prefab";
    private const string TexturePath = "Assets/Characters/WikiCat/Textures/tripo_mat_8547c237_Pbr_Diffuse.jpg";
    private const string MaterialFolder = "Assets/Characters/WikiCat/Materials";
    private const string MaterialPath = MaterialFolder + "/WikiCat_Unlit.mat";

    [MenuItem("Tools/WikiCat/Fix Dark Material", false, 1)]
    public static void FixDarkMaterial()
    {
        GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefabRoot == null)
        {
            Debug.LogError($"[WikiCatMaterialFix] Could not find prefab at {PrefabPath}");
            return;
        }

        Texture2D diffuse = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        if (diffuse == null)
        {
            Debug.LogError($"[WikiCatMaterialFix] Could not find diffuse texture at {TexturePath}");
            return;
        }

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (mat == null)
        {
            if (!AssetDatabase.IsValidFolder(MaterialFolder))
            {
                string parentFolder = System.IO.Path.GetDirectoryName(MaterialFolder).Replace('\\', '/');
                string newFolderName = System.IO.Path.GetFileName(MaterialFolder);
                string created = AssetDatabase.CreateFolder(parentFolder, newFolderName);
                if (string.IsNullOrEmpty(created))
                {
                    Debug.LogError($"[WikiCatMaterialFix] Failed to create folder {MaterialFolder}");
                    return;
                }
            }

            Shader unlit = Shader.Find("Unlit/Texture");
            if (unlit == null)
            {
                Debug.LogError("[WikiCatMaterialFix] Could not find built-in 'Unlit/Texture' shader.");
                return;
            }

            mat = new Material(unlit)
            {
                name = "WikiCat_Unlit",
                mainTexture = diffuse
            };
            AssetDatabase.CreateAsset(mat, MaterialPath);
        }

        bool modified = false;
        foreach (Renderer renderer in prefabRoot.GetComponentsInChildren<Renderer>(true))
        {
            Material[] sharedMats = renderer.sharedMaterials;
            for (int i = 0; i < sharedMats.Length; i++)
            {
                if (sharedMats[i] == null)
                    continue;

                if (sharedMats[i].shader == null || sharedMats[i].shader.name.Contains("Standard"))
                {
                    Undo.RecordObject(renderer, "Assign WikiCat Unlit material");
                    sharedMats[i] = mat;
                    modified = true;
                }
            }
            renderer.sharedMaterials = sharedMats;
        }

        if (modified)
        {
            EditorUtility.SetDirty(prefabRoot);
            AssetDatabase.SaveAssets();
            Debug.Log($"[WikiCatMaterialFix] Replaced Standard materials on WikiPrefab with {mat.name}.");
        }
        else
        {
            Debug.Log($"[WikiCatMaterialFix] WikiPrefab already uses a non-Standard material. Material asset is at {MaterialPath}.");
        }
    }
}

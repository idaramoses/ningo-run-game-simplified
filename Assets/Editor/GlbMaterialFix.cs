using UnityEngine;
using UnityEditor;
using System.IO;

public static class GlbMaterialFix
{
    private const string PrefabPath = "Assets/glb files/prefabs/bank+building+3d+model.prefab";
    private const string MaterialFolder = "Assets/glb files/Materials";
    private const string MaterialPath = MaterialFolder + "/BankBuilding_MobileDiffuse.mat";

    [MenuItem("Tools/Glb/Fix Bank Building Material", false, 1)]
    public static void FixBankBuildingMaterial()
    {
        GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefabRoot == null)
        {
            Debug.LogError($"[GlbMaterialFix] Could not find prefab at {PrefabPath}");
            return;
        }

        Renderer[] renderers = prefabRoot.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            Debug.LogError("[GlbMaterialFix] No renderers found on bank building prefab.");
            return;
        }

        // Grab the main texture off the first existing material we find (the glTF-imported material),
        // so the new Mobile/Diffuse material still shows the correct diffuse texture.
        Texture mainTexture = null;
        foreach (Renderer renderer in renderers)
        {
            foreach (Material sharedMat in renderer.sharedMaterials)
            {
                if (sharedMat == null)
                    continue;

                if (sharedMat.HasProperty("_MainTex") && sharedMat.mainTexture != null)
                {
                    mainTexture = sharedMat.mainTexture;
                    break;
                }
                if (sharedMat.HasProperty("_BaseMap") && sharedMat.GetTexture("_BaseMap") != null)
                {
                    mainTexture = sharedMat.GetTexture("_BaseMap");
                    break;
                }
            }
            if (mainTexture != null)
                break;
        }

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (mat == null)
        {
            if (!AssetDatabase.IsValidFolder(MaterialFolder))
            {
                string parentFolder = Path.GetDirectoryName(MaterialFolder).Replace('\\', '/');
                string newFolderName = Path.GetFileName(MaterialFolder);
                string created = AssetDatabase.CreateFolder(parentFolder, newFolderName);
                if (string.IsNullOrEmpty(created))
                {
                    Debug.LogError($"[GlbMaterialFix] Failed to create folder {MaterialFolder}");
                    return;
                }
            }

            Shader mobileDiffuse = Shader.Find("Mobile/Diffuse");
            if (mobileDiffuse == null)
            {
                Debug.LogError("[GlbMaterialFix] Could not find built-in 'Mobile/Diffuse' shader.");
                return;
            }

            mat = new Material(mobileDiffuse) { name = "BankBuilding_MobileDiffuse" };
            if (mainTexture != null)
                mat.mainTexture = mainTexture;

            AssetDatabase.CreateAsset(mat, MaterialPath);
        }

        bool modified = false;
        foreach (Renderer renderer in renderers)
        {
            Material[] sharedMats = renderer.sharedMaterials;
            for (int i = 0; i < sharedMats.Length; i++)
            {
                sharedMats[i] = mat;
                modified = true;
            }
            renderer.sharedMaterials = sharedMats;
        }

        if (modified)
        {
            EditorUtility.SetDirty(prefabRoot);
            AssetDatabase.SaveAssets();
            Debug.Log($"[GlbMaterialFix] Assigned {mat.name} (Mobile/Diffuse) to bank building prefab.");
        }
    }
}

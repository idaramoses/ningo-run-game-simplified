using UnityEngine;
using UnityEditor;

public static class TmpShaderFixer
{
    [MenuItem("Tools/Fix TMP SDF-URP Lit Shader Variant Explosion")]
    public static void Fix()
    {
        const string litShaderName = "TextMeshPro/SRP/TMP_SDF-URP Lit";

        // Prefer the project's custom Unlit Shader Graph, then fall back to the built-in URP unlit shader.
        Shader unlit = Shader.Find("TextMeshPro/SRP/TMP_SDF-URP Unlit")
                       ?? Shader.Find("TextMeshPro/SRP/TMP_SDF-URP");

        if (unlit == null)
        {
            Debug.LogError("TmpShaderFixer: Could not find an unlit TMP_SDF-URP shader. Make sure packages are fully imported before running this tool.");
            return;
        }

        int fixedCount = 0;
        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (Object asset in assets)
            {
                if (asset is Material mat && mat.shader != null && mat.shader.name == litShaderName)
                {
                    mat.shader = unlit;
                    EditorUtility.SetDirty(mat);
                    Debug.Log($"TmpShaderFixer: Replaced shader on '{mat.name}' ({path})");
                    fixedCount++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"TmpShaderFixer: Done. Replaced shader on {fixedCount} material(s).");
    }
}

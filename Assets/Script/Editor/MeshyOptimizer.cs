using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class MeshyOptimizer
{
    private static readonly string[] RedundantFolders = new string[]
    {
        "Assets/MeshyImports/Lagos Character 2 Elderly Man Rigged_20260712_182554",
        "Assets/MeshyImports/Lagos Character 3 Young Woman Rigged_20260712_182638",
        "Assets/MeshyImports/Lagos Character 4 Schoolboy Rigged_20260712_182828",
        "Assets/MeshyImports/Lagos Character 5 Okada Rider Rigged_20260712_182931",
        "Assets/MeshyImports/Lagos Character 6 Office Woman Rigged_20260712_183155",
        "Assets/MeshyImports/Lagos Character 8 Schoolgirl Rigged_20260712_183431",
        "Assets/MeshyImports/Lagos Char 9 Groundnut Hawker Rigged_20260712_183826",
        "Assets/MeshyImports/Lagos Char 10 Pure Water Hawker Rigged_20260712_184016",
        "Assets/MeshyImports/Lagos Char 11 Bread Hawker Rigged_20260712_184133",
        "Assets/MeshyImports/Lagos Char 12 Newspaper Hawker Rigged_20260712_184209"
    };

    private static readonly string[] ActiveFolders = new string[]
    {
        "Assets/MeshyImports/Lagos Character 1 Young Man Rigged_20260712_182012",
        "Assets/MeshyImports/Lagos Character 2 Elderly Man Rigged_20260712_182549",
        "Assets/MeshyImports/Lagos Character 3 Young Woman Rigged_20260712_182629",
        "Assets/MeshyImports/Lagos Character 4 Schoolboy Rigged_20260712_182823",
        "Assets/MeshyImports/Lagos Character 5 Okada Rider Rigged_20260712_182923",
        "Assets/MeshyImports/Lagos Character 6 Office Woman Rigged_20260712_183151",
        "Assets/MeshyImports/Lagos Character 8 Schoolgirl Rigged_20260712_183426",
        "Assets/MeshyImports/Lagos Char 9 Groundnut Hawker Rigged_20260712_183819",
        "Assets/MeshyImports/Lagos Char 10 Pure Water Hawker Rigged_20260712_184010",
        "Assets/MeshyImports/Lagos Char 11 Bread Hawker Rigged_20260712_184128",
        "Assets/MeshyImports/Lagos Char 12 Newspaper Hawker Rigged_20260712_184204"
    };

    [MenuItem("Tools/Optimize Meshy Imports")]
    public static void OptimizeAndClean()
    {
        Debug.Log("=== Starting Meshy Imports Optimization ===");

        // 1. Delete redundant folders
        int deletedCount = 0;
        foreach (string folder in RedundantFolders)
        {
            if (AssetDatabase.IsValidFolder(folder) || Directory.Exists(folder))
            {
                if (AssetDatabase.DeleteAsset(folder))
                {
                    Debug.Log($"✓ Deleted redundant folder: {folder}");
                    deletedCount++;
                }
                else
                {
                    Debug.LogError($"✗ Failed to delete folder: {folder}");
                }
            }
            else
            {
                Debug.Log($"Redundant folder does not exist (already deleted): {folder}");
            }
        }

        // 2. Optimize textures and meshes in active folders
        int optimizedTextures = 0;
        int optimizedModels = 0;

        foreach (string folder in ActiveFolders)
        {
            if (!AssetDatabase.IsValidFolder(folder) && !Directory.Exists(folder))
            {
                Debug.LogWarning($"Active folder not found: {folder}");
                continue;
            }

            // Optimize Textures (PNG)
            string[] pngFiles = Directory.GetFiles(folder, "*.png", SearchOption.AllDirectories);
            foreach (string file in pngFiles)
            {
                string assetPath = file.Replace("\\", "/");
                TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (textureImporter != null)
                {
                    bool changed = false;
                    if (!textureImporter.crunchedCompression)
                    {
                        textureImporter.crunchedCompression = true;
                        changed = true;
                    }
                    if (textureImporter.compressionQuality != 50)
                    {
                        textureImporter.compressionQuality = 50;
                        changed = true;
                    }
                    if (textureImporter.maxTextureSize != 512)
                    {
                        textureImporter.maxTextureSize = 512;
                        changed = true;
                    }

                    if (changed)
                    {
                        textureImporter.SaveAndReimport();
                        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                        Debug.Log($"✓ Optimized texture: {assetPath} (Crunched: True, Quality: 50, MaxSize: 512)");
                        optimizedTextures++;
                    }
                }
            }

            // Optimize Models (FBX)
            string[] fbxFiles = Directory.GetFiles(folder, "*.fbx", SearchOption.AllDirectories);
            foreach (string file in fbxFiles)
            {
                string assetPath = file.Replace("\\", "/");
                ModelImporter modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                if (modelImporter != null)
                {
                    bool modelChanged = false;
                    if (modelImporter.meshCompression != ModelImporterMeshCompression.High)
                    {
                        modelImporter.meshCompression = ModelImporterMeshCompression.High;
                        modelChanged = true;
                    }
                    if (modelImporter.maxBonesPerVertex > 2)
                    {
                        modelImporter.maxBonesPerVertex = 2;
                        modelChanged = true;
                    }
                    if (modelImporter.importCameras)
                    {
                        modelImporter.importCameras = false;
                        modelChanged = true;
                    }
                    if (modelImporter.importLights)
                    {
                        modelImporter.importLights = false;
                        modelChanged = true;
                    }
                    if (modelImporter.importBlendShapes)
                    {
                        modelImporter.importBlendShapes = false;
                        modelChanged = true;
                    }

                    if (modelChanged)
                    {
                        modelImporter.SaveAndReimport();
                        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                        Debug.Log($"✓ Optimized model: {assetPath} (MeshCompression: High, MaxBones: 2)");
                        optimizedModels++;
                    }
                }
            }
        }

        AssetDatabase.Refresh();
        Debug.Log("=== Meshy Imports Optimization Complete! ===");
        Debug.Log($"Deleted Folders: {deletedCount}");
        Debug.Log($"Optimized Textures: {optimizedTextures}");
        Debug.Log($"Optimized Models: {optimizedModels}");
    }
}

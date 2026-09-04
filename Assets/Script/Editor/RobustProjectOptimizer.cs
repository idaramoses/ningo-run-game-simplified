using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class RobustProjectOptimizer : EditorWindow
{
    [MenuItem("Tools/Performance/Robust Project Optimizer/Optimize All Assets")]
    public static void OptimizeAllAssets()
    {
        Debug.Log("=== STARTING ROBUST GLOBAL PROJECT OPTIMIZATION ===");
        
        OptimizeModels();
        OptimizeTextures();
        OptimizeAudio();
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("=== ROBUST GLOBAL PROJECT OPTIMIZATION COMPLETE ===");
    }
    
    [MenuItem("Tools/Performance/Robust Project Optimizer/Optimize Models Only")]
    public static void OptimizeModels()
    {
        Debug.Log("--- OPTIMIZING MODELS ---");
        string[] modelGuids = AssetDatabase.FindAssets("t:Model");
        int count = 0;
        
        foreach (var guid in modelGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.StartsWith("Assets/")) continue;
            
            ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) continue;
            
            bool changed = false;
            
            // 1. Mesh Compression
            if (importer.meshCompression != ModelImporterMeshCompression.Medium)
            {
                importer.meshCompression = ModelImporterMeshCompression.Medium;
                changed = true;
            }
            
            // 2. Disable Read/Write to free up CPU memory
            if (importer.isReadable)
            {
                importer.isReadable = false;
                changed = true;
            }
            
            // 3. Optimize Mesh
            #if UNITY_2019_3_OR_NEWER
            if (!importer.optimizeMeshVertices)
            {
                importer.optimizeMeshVertices = true;
                changed = true;
            }
            if (!importer.optimizeMeshPolygons)
            {
                importer.optimizeMeshPolygons = true;
                changed = true;
            }
            #endif
            
            if (changed)
            {
                importer.SaveAndReimport();
                count++;
            }
        }
        
        Debug.Log($"✓ Optimized {count} models (Mesh Compression: Medium, Read/Write: Disabled, Optimize Mesh: True)");
    }
    
    [MenuItem("Tools/Performance/Robust Project Optimizer/Optimize Textures Only")]
    public static void OptimizeTextures()
    {
        Debug.Log("--- OPTIMIZING TEXTURES ---");
        string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D");
        int count = 0;
        
        foreach (var guid in textureGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.StartsWith("Assets/")) continue;
            
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;
            
            bool changed = false;
            bool isUI = path.Contains("/UI/") || path.Contains("/HUD/") || path.Contains("/Sprites/") || importer.textureType == TextureImporterType.Sprite;
            
            // 1. Mipmaps: UI should NOT have mipmaps; 3D textures MUST have mipmaps (prevents shimmering and GPU cache thrashing)
            if (isUI)
            {
                if (importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    changed = true;
                }
            }
            else
            {
                if (!importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = true;
                    changed = true;
                }
            }
            
            // 2. Max Size: Cap 3D environment textures at 1024 or 512 to save mobile memory. UI can remain 2048 if needed.
            int maxAllowedSize = isUI ? 2048 : 1024;
            // Exceptions: Some very large backgrounds or skyboxes
            if (path.Contains("Skybox") || path.Contains("skybox"))
            {
                maxAllowedSize = 2048;
            }
            
            if (importer.maxTextureSize > maxAllowedSize)
            {
                importer.maxTextureSize = maxAllowedSize;
                changed = true;
            }
            
            // 3. Compression: Ensure compressed is used
            if (importer.textureCompression == TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Compressed;
                changed = true;
            }
            
            // 4. Disable Read/Write unless needed
            if (importer.isReadable)
            {
                importer.isReadable = false;
                changed = true;
            }
            
            if (changed)
            {
                importer.SaveAndReimport();
                count++;
            }
        }
        
        Debug.Log($"✓ Optimized {count} textures (Mipmaps configured, Max Size capped, Compression: Enabled, Read/Write: Disabled)");
    }
    
    [MenuItem("Tools/Performance/Robust Project Optimizer/Optimize Audio Only")]
    public static void OptimizeAudio()
    {
        Debug.Log("--- OPTIMIZING AUDIO ---");
        string[] audioGuids = AssetDatabase.FindAssets("t:AudioClip");
        int count = 0;
        
        foreach (var guid in audioGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.StartsWith("Assets/")) continue;
            
            AudioImporter importer = AssetImporter.GetAtPath(path) as AudioImporter;
            if (importer == null) continue;
            
            bool changed = false;
            
            // 1. Force to Mono (Saves 50% file size and memory, perfect for mobile!)
            if (!importer.forceToMono)
            {
                importer.forceToMono = true;
                changed = true;
            }
            
            // Retrieve default sample settings
            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            
            // Check file size / length to determine best compression
            FileInfo fileInfo = new FileInfo(path);
            long fileLength = fileInfo.Exists ? fileInfo.Length : 0;
            
            // 2. Compression Format & Load Type based on size/importance
            if (fileLength > 1024 * 1024) // > 1MB: Music or long loops
            {
                if (settings.compressionFormat != AudioCompressionFormat.Vorbis)
                {
                    settings.compressionFormat = AudioCompressionFormat.Vorbis;
                    changed = true;
                }
                if (settings.loadType != AudioClipLoadType.Streaming)
                {
                    settings.loadType = AudioClipLoadType.Streaming;
                    changed = true;
                }
                if (settings.quality != 0.4f)
                {
                    settings.quality = 0.4f; // 40% Vorbis quality is plenty for mobile speakers
                    changed = true;
                }
            }
            else // Short sound effects
            {
                if (settings.compressionFormat != AudioCompressionFormat.ADPCM && settings.compressionFormat != AudioCompressionFormat.Vorbis)
                {
                    settings.compressionFormat = AudioCompressionFormat.ADPCM; // Highly efficient for short sounds
                    changed = true;
                }
                if (settings.loadType != AudioClipLoadType.DecompressOnLoad)
                {
                    settings.loadType = AudioClipLoadType.DecompressOnLoad;
                    changed = true;
                }
            }
            
            if (changed)
            {
                importer.defaultSampleSettings = settings;
                importer.SaveAndReimport();
                count++;
            }
        }
        
        Debug.Log($"✓ Optimized {count} audio clips (Force Mono, Vorbis/ADPCM compression, Streaming for large files)");
    }
}

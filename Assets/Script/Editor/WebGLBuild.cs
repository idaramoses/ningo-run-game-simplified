using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Builds the game for WebGL into Builds/WebGL.
/// Run from the menu (Build > WebGL) or headless:
///   Unity -batchmode -projectPath <proj> -executeMethod WebGLBuild.BuildWebGL -quit
/// Uses Brotli + decompressionFallback so the output works on any static host.
/// </summary>
public static class WebGLBuild
{
    private const string OutputPath = "Builds/WebGL";

    [MenuItem("Build/WebGL")]
    public static void BuildWebGL()
    {
        // Brotli + decompressionFallback: ~4x smaller download and still works
        // on any static host (the loader decompresses in JS when the server
        // doesn't send Content-Encoding: br).
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.dataCaching = true;
        // Custom landing-page template: marketing page with a Play button
        // that boots the game in a fullscreen overlay. No Unity branding.
        PlayerSettings.WebGL.template = "PROJECT:Ningo";

        ApplyTextureCompression();

        var options = new BuildPlayerOptions
        {
            scenes = EnabledScenes(),
            locationPathName = OutputPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None,
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            throw new System.Exception($"[Build] WebGL build failed: {report.summary.result}");

        Debug.Log($"[Build] WebGL build succeeded: {report.summary.outputPath} ({report.summary.totalSize / (1024 * 1024)} MB)");
    }

    private static readonly string[] Platforms = { "WebGL", "Android", "iPhone" };

    /// <summary>
    /// Size reduction WITHOUT touching resolution: keeps every texture at its
    /// native size and enables GPU texture compression for WebGL/Android/iOS
    /// (DXT on desktop WebGL, ASTC/ETC2 on mobile — visually near-lossless).
    /// Idempotent — safe to run before every build.
    /// </summary>
    [MenuItem("Build/Apply Texture Compression (all platforms)")]
    public static void ApplyTextureCompression()
    {
        var guids = AssetDatabase.FindAssets("t:Texture");
        int changed = 0;
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) continue;

                bool dirty = false;
                if (importer.textureCompression != TextureImporterCompression.Compressed)
                {
                    importer.textureCompression = TextureImporterCompression.Compressed;
                    dirty = true;
                }

                foreach (var platform in Platforms)
                {
                    var s = importer.GetPlatformTextureSettings(platform);
                    if (s.overridden && s.textureCompression == TextureImporterCompression.Compressed) continue;

                    s.overridden = true;
                    s.format = TextureImporterFormat.Automatic;
                    s.textureCompression = TextureImporterCompression.Compressed;
                    importer.SetPlatformTextureSettings(s);
                    dirty = true;
                }

                if (dirty)
                {
                    importer.SaveAndReimport();
                    changed++;
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
        Debug.Log($"[Build] Texture compression (full-res) applied to {changed} textures");
    }

    private static string[] EnabledScenes()
    {
        var scenes = new List<string>();
        foreach (var scene in EditorBuildSettings.scenes)
            if (scene.enabled) scenes.Add(scene.path);
        return scenes.ToArray();
    }
}

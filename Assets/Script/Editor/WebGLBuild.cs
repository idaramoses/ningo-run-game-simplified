using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
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
    private static HashSet<string> currentBundleNames;

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
        ApplyAudioCompression();
        ApplyMeshCompression();
        ApplyUITextureCap();

        // Streaming split: remote bundles (Home scene + deps) must exist
        // before the player build so the catalog is included.
        BuildAddressablesContent();

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

        CopyServerDataIntoBuild();
        Debug.Log($"[Build] WebGL build succeeded: {report.summary.outputPath} ({report.summary.totalSize / (1024 * 1024)} MB)");
    }

    /// <summary>
    /// Quantizes mesh vertex data (ModelImporter meshCompression = High).
    /// Shrinks bundles; visually imperceptible on low-poly assets.
    /// </summary>
    [MenuItem("Build/Apply Mesh Compression (all models)")]
    public static void ApplyMeshCompression()
    {
        var guids = AssetDatabase.FindAssets("t:Model");
        int changed = 0;
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!(AssetImporter.GetAtPath(path) is ModelImporter importer)) continue;
                if (importer.meshCompression == ModelImporterMeshCompression.High) continue;
                importer.meshCompression = ModelImporterMeshCompression.High;
                importer.SaveAndReimport();
                changed++;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
        Debug.Log($"[Build] Mesh compression (High) applied to {changed} models");
    }

    /// <summary>
    /// Caps every texture at 1024px on the long edge and disables mipmaps on
    /// UI sprites (mipmaps are ~33% of sprite bytes and useless for fixed-size
    /// UI). 1024px is invisible on phones and imperceptible for this art style.
    /// </summary>
    [MenuItem("Build/Apply UI Texture Cap (1024px)")]
    public static void ApplyUITextureCap()
    {
        const int cap = 1024;
        var guids = AssetDatabase.FindAssets("t:Texture");
        int capped = 0, mipsStripped = 0;
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.StartsWith("Assets/")) continue;
                if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) continue;

                bool dirty = false;
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex != null && Mathf.Max(tex.width, tex.height) > cap)
                {
                    if (importer.maxTextureSize > cap)
                    {
                        importer.maxTextureSize = cap;
                        dirty = true;
                    }
                    // A platform override's own maxTextureSize wins over the
                    // base value — must cap it too or WebGL keeps full res.
                    foreach (var platform in Platforms)
                    {
                        var s = importer.GetPlatformTextureSettings(platform);
                        if (s.overridden && s.maxTextureSize > cap)
                        {
                            s.maxTextureSize = cap;
                            importer.SetPlatformTextureSettings(s);
                            dirty = true;
                        }
                    }
                }
                // Sprites render at fixed size — mipmaps are dead bytes.
                if (importer.textureType == TextureImporterType.Sprite && importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    mipsStripped++;
                    dirty = true;
                }
                if (dirty)
                {
                    importer.SaveAndReimport();
                    capped++;
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
        Debug.Log($"[Build] Texture cap ({cap}px) applied to {capped} textures; mipmaps stripped on {mipsStripped} sprites");
    }

    /// <summary>Rebuilds Addressables content only (no player rebuild).</summary>
    [MenuItem("Build/Addressables Content Only")]
    public static void BuildContentOnly()
    {
        ApplyTextureCompression();
        ApplyAudioCompression();
        ApplyMeshCompression();
        ApplyUITextureCap();
        BuildAddressablesContent();
        CopyServerDataIntoBuild();
    }

    /// <summary>
    /// Copies remote Addressables bundles (ServerData/WebGL) into the build
    /// output so they're served same-origin next to index.html.
    /// </summary>
    private static void CopyServerDataIntoBuild()
    {
        string src = Path.Combine(Directory.GetCurrentDirectory(), "ServerData", "WebGL");
        string dst = Path.Combine(Directory.GetCurrentDirectory(), OutputPath, "ServerData", "WebGL");
        if (!Directory.Exists(src)) { Debug.LogWarning("[Build] No ServerData/WebGL — remote bundles missing?"); return; }
        if (Directory.Exists(dst)) Directory.Delete(dst, true); // drop stale bundles
        Directory.CreateDirectory(dst);
        foreach (string f in Directory.GetFiles(src))
        {
            if (Path.GetExtension(f) == ".bundle" && currentBundleNames != null
                && !currentBundleNames.Contains(Path.GetFileName(f))) continue;
            File.Copy(f, Path.Combine(dst, Path.GetFileName(f)), true);
        }
        Debug.Log($"[Build] Copied remote bundles -> {dst}");
        WriteBundleManifest(dst);
        SyncCatalogIntoBuild();
    }

    /// <summary>
    /// Writes ServerData/WebGL/manifest.json listing every remote bundle, so
    /// the landing page can prefetch them into the browser cache while the
    /// user is still on the page — the in-game download then hits warm cache.
    /// </summary>
    private static void WriteBundleManifest(string bundleDir)
    {
        var names = new List<string>();
        foreach (string f in Directory.GetFiles(bundleDir, "*.bundle"))
            names.Add(Path.GetFileName(f));
        string json = "{\"bundles\":[\"" + string.Join("\",\"", names) + "\"]}";
        File.WriteAllText(Path.Combine(bundleDir, "manifest.json"), json);
        Debug.Log($"[Build] Bundle manifest written ({names.Count} bundles)");
    }

    /// <summary>
    /// Copies the freshly-built Addressables catalog + local bundles from
    /// Library/com.unity.addressables/aa/<target> into the build output's
    /// StreamingAssets/aa — without this, content-only rebuilds leave a stale
    /// catalog pointing at bundle names that no longer exist.
    /// </summary>
    private static void SyncCatalogIntoBuild()
    {
        string src = Path.Combine(Directory.GetCurrentDirectory(), "Library", "com.unity.addressables", "aa", "WebGL");
        string dst = Path.Combine(Directory.GetCurrentDirectory(), OutputPath, "StreamingAssets", "aa");
        if (!Directory.Exists(src)) { Debug.LogWarning("[Build] No aa/ catalog source"); return; }
        if (Directory.Exists(dst)) Directory.Delete(dst, true);
        Directory.CreateDirectory(dst);
        foreach (string f in Directory.GetFiles(src))
            File.Copy(f, Path.Combine(dst, Path.GetFileName(f)), true);
        foreach (string d in Directory.GetDirectories(src))
        {
            string name = Path.GetFileName(d);
            string dd = Path.Combine(dst, name);
            Directory.CreateDirectory(dd);
            foreach (string f in Directory.GetFiles(d))
                File.Copy(f, Path.Combine(dd, Path.GetFileName(f)), true);
        }
        Debug.Log($"[Build] Synced catalog + local bundles -> {dst}");
    }

    public static void BuildAddressablesContent()
    {
        UnityEditor.AddressableAssets.Build.AddressablesPlayerBuildResult result;
        AddressableAssetSettings.BuildPlayerContent(out result);
        if (!string.IsNullOrEmpty(result.Error))
            throw new System.InvalidOperationException(result.Error);
        currentBundleNames = new HashSet<string>();
        long bytes = 0;
        foreach (var bundle in result.AssetBundleBuildResults)
        {
            currentBundleNames.Add(Path.GetFileName(bundle.FilePath));
            if (File.Exists(bundle.FilePath)) bytes += new FileInfo(bundle.FilePath).Length;
        }
        string summary = $"[SizeAudit] WebGL Addressables: {bytes / (1024f * 1024f):F2} MB ({currentBundleNames.Count} bundles)";
        SessionState.SetString("NingoSizeAuditResult", summary);
        Debug.Log(summary);
    }

    private static readonly string[] Platforms = { "WebGL", "Android" };

    [MenuItem("Build/Apply Size Optimizations (Android + WebGL)")]
    public static void ApplySizeOptimizations()
    {
        ApplyTextureCompression();
        ApplyAudioCompression();
    }

    private static string[] BuildAssetPaths()
    {
        var roots = new HashSet<string>();
        foreach (var scene in EditorBuildSettings.scenes)
            if (scene.enabled) roots.Add(scene.path);
        var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
        if (settings != null)
            foreach (var group in settings.groups)
                if (group != null)
                    foreach (var entry in group.entries)
                        if (!string.IsNullOrEmpty(entry.AssetPath)) roots.Add(entry.AssetPath);
        foreach (string path in AssetDatabase.GetAllAssetPaths())
            if (path.StartsWith("Assets/") && path.Contains("/Resources/") && File.Exists(path))
                roots.Add(path);
        var paths = new List<string>(AssetDatabase.GetDependencies(new List<string>(roots).ToArray(), true));
        paths.RemoveAll(path => !path.StartsWith("Assets/"));
        return paths.ToArray();
    }

    public static void ApplyAudioCompression()
    {
        int changed = 0;
        foreach (string path in BuildAssetPaths())
        {
            if (!(AssetImporter.GetAtPath(path) is AudioImporter importer)) continue;
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null) continue;
            float quality = clip.length >= 15f ? 0.7f : 0.85f;
            var defaults = importer.defaultSampleSettings;
            bool dirty = defaults.compressionFormat != AudioCompressionFormat.Vorbis || defaults.quality > quality;
            if (dirty)
            {
                defaults.compressionFormat = AudioCompressionFormat.Vorbis;
                defaults.quality = quality;
                importer.defaultSampleSettings = defaults;
            }
            foreach (string platform in Platforms)
            {
                if (platform == "WebGL") continue;
                var samples = importer.ContainsSampleSettingsOverride(platform)
                    ? importer.GetOverrideSampleSettings(platform)
                    : importer.defaultSampleSettings;
                if (samples.compressionFormat == AudioCompressionFormat.Vorbis && samples.quality <= quality)
                    continue;
                samples.compressionFormat = AudioCompressionFormat.Vorbis;
                samples.quality = quality;
                if (!importer.SetOverrideSampleSettings(platform, samples))
                    throw new System.InvalidOperationException($"Cannot set {platform} audio compression: {path}");
                dirty = true;
            }
            if (!dirty) continue;
            importer.SaveAndReimport();
            changed++;
        }
        Debug.Log($"[Build] Platform audio compression applied to {changed} clips; channels and sample rates preserved");
    }

    /// <summary>
    /// Size reduction WITHOUT touching resolution: keeps every texture at its
    /// native size and enables high-quality GPU texture compression for
    /// WebGL/Android/iOS (HQ compressor — no visible softening of UI art).
    /// Idempotent — safe to run before every build.
    /// </summary>
    [MenuItem("Build/Apply Texture Compression (all platforms)")]
    public static void ApplyTextureCompression()
    {
        var paths = BuildAssetPaths();
        int changed = 0;
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (string path in paths)
            {
                if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) continue;

                bool dirty = false;
                if (importer.textureCompression != TextureImporterCompression.CompressedHQ)
                {
                    importer.textureCompression = TextureImporterCompression.CompressedHQ;
                    dirty = true;
                }
                // Crunch requires power-of-two dimensions; on NPOT textures
                // Unity silently falls back to uncompressed RGBA32 (~4MB per
                // 1024px sprite). Source PNGs are padded to POT / multiple-of-4
                // by the padding script — here we only request crunch when the
                // imported size really is POT, else plain DXT5 still applies.
                var loaded = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                bool isPot = loaded != null
                    && (loaded.width & (loaded.width - 1)) == 0
                    && (loaded.height & (loaded.height - 1)) == 0;

                foreach (var platform in Platforms)
                {
                    var s = importer.GetPlatformTextureSettings(platform);
                    // AutomaticCompressed resolves to RGBA32 when the project's
                    // default WebGL compression format is unset — pin it to DXT
                    // explicitly (Unity software-decompresses where s3tc is
                    // missing, e.g. mobile Safari).
                    var fmt = importer.textureType == TextureImporterType.NormalMap
                        ? TextureImporterFormat.Automatic
                        : importer.DoesSourceTextureHaveAlpha()
                            ? TextureImporterFormat.DXT5
                            : TextureImporterFormat.DXT1;
                    if (platform == "Android" && importer.textureType != TextureImporterType.NormalMap)
                        fmt = importer.DoesSourceTextureHaveAlpha()
                            ? TextureImporterFormat.ETC2_RGBA8
                            : TextureImporterFormat.ETC_RGB4;
                    if (importer.textureType == TextureImporterType.SingleChannel)
                        fmt = TextureImporterFormat.R8;
                    int maxSize = Mathf.Min(s.maxTextureSize, importer.maxTextureSize);
                    bool crunch = isPot && (fmt == TextureImporterFormat.DXT1 || fmt == TextureImporterFormat.DXT5
                        || fmt == TextureImporterFormat.ETC_RGB4 || fmt == TextureImporterFormat.ETC2_RGBA8);
                    if (crunch)
                    {
                        if (fmt == TextureImporterFormat.DXT1) fmt = TextureImporterFormat.DXT1Crunched;
                        else if (fmt == TextureImporterFormat.DXT5) fmt = TextureImporterFormat.DXT5Crunched;
                        else if (fmt == TextureImporterFormat.ETC_RGB4) fmt = TextureImporterFormat.ETC_RGB4Crunched;
                        else if (fmt == TextureImporterFormat.ETC2_RGBA8) fmt = TextureImporterFormat.ETC2_RGBA8Crunched;
                    }
                    if (s.overridden && s.textureCompression == TextureImporterCompression.CompressedHQ
                        && s.format == fmt && s.maxTextureSize == maxSize && s.crunchedCompression == crunch
                        && s.compressionQuality == 100) continue;

                    s.overridden = true;
                    s.format = fmt;
                    s.maxTextureSize = maxSize;
                    s.crunchedCompression = crunch;
                    s.compressionQuality = 100;
                    s.textureCompression = TextureImporterCompression.CompressedHQ;
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
        Debug.Log($"[Build] Texture compression (full-res, HQ) applied to {changed} textures");
    }

    private static string[] EnabledScenes()
    {
        // Home stays enabled in Build Settings so Android/editor can load it
        // with SceneManager — exclude it here because WebGL streams it as a
        // remote Addressables scene (RemoteScene.RemoteNames).
        var scenes = new List<string>();
        foreach (var scene in EditorBuildSettings.scenes)
            if (scene.enabled && !scene.path.EndsWith("/Home.unity"))
                scenes.Add(scene.path);
        return scenes.ToArray();
    }
}

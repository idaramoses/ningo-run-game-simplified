using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

/// <summary>
/// One-time Addressables setup for the WebGL streaming split:
///  - Remote.BuildPath / Remote.LoadPath -> ServerData/WebGL (same-origin
///    relative URL — resolved against the page URL at runtime, so it works
///    on localhost and any deploy host)
///  - Home.unity goes into a remote group with address "Home"
///  - Home's dependency tree is bin-packed into multiple remote groups so
///    no single bundle exceeds Vercel's 100 MB per-file limit
///  - Home removed from Build Settings so it isn't baked into .data
/// Run headless: -executeMethod AddressablesSetup.Run
/// </summary>
public static class AddressablesSetup
{
    private const string HomeScenePath = "Assets/Scene/Home.unity";
    private const long MaxGroupBytes = 55L * 1024 * 1024; // ~55 MB source -> <100 MB packed

    [MenuItem("Build/Setup Addressables (remote Home scene)")]
    public static void Run()
    {
        var settings = AddressableAssetSettingsDefaultObject.GetSettings(true);
        var ps = settings.profileSettings;
        string profileId = settings.activeProfileId;

        ps.SetValue(profileId, "Remote.BuildPath", "ServerData/WebGL");
        ps.SetValue(profileId, "Remote.LoadPath", "ServerData/WebGL");
        ps.SetValue(profileId, "Local.BuildPath", "[UnityEngine.AddressableAssets.Addressables.BuildPath]/[BuildTarget]");
        ps.SetValue(profileId, "Local.LoadPath", "{UnityEngine.AddressableAssets.Addressables.RuntimePath}/[BuildTarget]");
        settings.BuildRemoteCatalog = false; // local catalog points at remote bundle URL

        // --- Scene group -------------------------------------------------
        var sceneGroup = CreateRemoteGroup(settings, "Remote-Scene");
        string sceneGuid = AssetDatabase.AssetPathToGUID(HomeScenePath);
        var entry = settings.CreateOrMoveEntry(sceneGuid, sceneGroup, false, false);
        entry.SetAddress("Home", true);

        // --- Dependency groups (bin-packed under the host's file limit) --
        string[] deps = AssetDatabase.GetDependencies(HomeScenePath, true)
            .Where(p => p.StartsWith("Assets/") && p != HomeScenePath)
            .Where(p => File.Exists(p)) // files only, not folders
            .ToArray();

        var bySize = deps
            .Select(p => new { path = p, size = new FileInfo(p).Length })
            .OrderByDescending(d => d.size);

        var groups = new List<AddressableAssetGroup>();
        long currentSize = 0;
        int groupIndex = 0;
        AddressableAssetGroup current = null;

        foreach (var dep in bySize)
        {
            if (current == null || currentSize + dep.size > MaxGroupBytes)
            {
                current = CreateRemoteGroup(settings, $"Remote-{groupIndex++}");
                groups.Add(current);
                currentSize = 0;
            }
            currentSize += dep.size;
            string guid = AssetDatabase.AssetPathToGUID(dep.path);
            if (!string.IsNullOrEmpty(guid))
                settings.CreateOrMoveEntry(guid, current, false, false);
        }

        // Home ships as bundles now — remove it from the player build list.
        var scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.RemoveAll(s => s.path == HomeScenePath) > 0)
            EditorBuildSettings.scenes = scenes.ToArray();

        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true, true);
        AssetDatabase.SaveAssets();
        Debug.Log($"[Addressables] Setup done — 'Home' + {groups.Count} dependency groups stream from ServerData/WebGL");
    }

    private static AddressableAssetGroup CreateRemoteGroup(AddressableAssetSettings settings, string name)
    {
        var schemaTemplate = settings.DefaultGroup != null ? settings.DefaultGroup.Schemas.ToList() : null;
        var group = settings.FindGroup(name);
        if (group == null)
            group = settings.CreateGroup(name, false, false, true, schemaTemplate,
                typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));

        var bundleSchema = group.GetSchema<BundledAssetGroupSchema>();
        bundleSchema.BuildPath.SetVariableByName(settings, "Remote.BuildPath");
        bundleSchema.LoadPath.SetVariableByName(settings, "Remote.LoadPath");
        bundleSchema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
        // WebGL can only decompress LZ4 or uncompressed bundles at runtime —
        // LZMA fails with "Decompressing this format (1) is not supported".
        bundleSchema.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZ4;
        return group;
    }
}

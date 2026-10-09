using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Lists assets not referenced by any enabled build scene, the Resources
/// folder, or StreamingAssets — i.e. files Unity would never include in a
/// build. Run headless:
///   Unity -batchmode -executeMethod UnusedAssetAudit.Run -quit
/// Writes Assets/../unused_assets.txt (project root) with sizes.
/// </summary>
public static class UnusedAssetAudit
{
    [MenuItem("Build/Audit Unused Assets")]
    public static void Run()
    {
        var used = new HashSet<string>();

        // Everything reachable from enabled scenes (scripts, prefabs,
        // materials, textures, audio — recursively).
        foreach (var scene in EditorBuildSettings.scenes.Where(s => s.enabled))
            foreach (var dep in AssetDatabase.GetDependencies(scene.path, true))
                used.Add(dep);

        // Everything in any Resources folder ships with the build regardless —
        // AND so does anything those assets reference (TMP Settings -> fonts,
        // prefabs -> materials/controllers/etc).
        var resRoots = new List<string>();
        foreach (var dir in Directory.GetDirectories("Assets", "Resources", SearchOption.AllDirectories))
            foreach (var guid in AssetDatabase.FindAssets("", new[] { dir }))
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                used.Add(p);
                resRoots.Add(p);
            }
        foreach (var dep in AssetDatabase.GetDependencies(resRoots.ToArray(), true))
            used.Add(dep);

        // StreamingAssets ships verbatim too.
        if (Directory.Exists("Assets/StreamingAssets"))
            foreach (var guid in AssetDatabase.FindAssets("", new[] { "Assets/StreamingAssets" }))
                used.Add(AssetDatabase.GUIDToAssetPath(guid));

        // ProjectSettings-referenced assets (URP pipeline assets, quality
        // settings, icons, splash) aren't scene deps — pull them in.
        foreach (var guid in AssetDatabase.FindAssets("", new[] { "Assets/Settings", "ProjectSettings" }))
            foreach (var dep in AssetDatabase.GetDependencies(AssetDatabase.GUIDToAssetPath(guid), true))
                used.Add(dep);
        foreach (var guid in AssetDatabase.FindAssets("t:RenderPipelineAsset t:Shader t:ShaderInclude t:ComputeShader"))
            foreach (var dep in AssetDatabase.GetDependencies(AssetDatabase.GUIDToAssetPath(guid), true))
                used.Add(dep);

        // Scripts & ScriptableObjects live outside scenes but must stay —
        // code may reference types by name. Keep every .cs anyway.
        foreach (var guid in AssetDatabase.FindAssets("t:Script"))
            used.Add(AssetDatabase.GUIDToAssetPath(guid));

        // WebGL template + its art aren't scene deps but ship in the build.
        if (Directory.Exists("Assets/WebGLTemplates"))
            foreach (var guid in AssetDatabase.FindAssets("", new[] { "Assets/WebGLTemplates" }))
                used.Add(AssetDatabase.GUIDToAssetPath(guid));

        string root = Directory.GetCurrentDirectory();
        var unused = new List<(string path, long bytes)>();
        foreach (var guid in AssetDatabase.FindAssets(""))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (used.Contains(path)) continue;
            if (Directory.Exists(Path.Combine(root, path))) continue; // folder
            if (path.StartsWith("Packages/")) continue;               // packages untouched
            string abs = Path.Combine(root, path);
            if (!File.Exists(abs)) continue;
            unused.Add((path, new FileInfo(abs).Length));
        }

        long total = unused.Sum(u => u.bytes);
        string report = Path.Combine(root, "unused_assets.txt");
        File.WriteAllLines(report,
            new[] { $"# {unused.Count} unused assets, {total / (1024 * 1024)} MB total",
                    "" }
                .Concat(unused.OrderByDescending(u => u.bytes)
                              .Select(u => $"{u.bytes / 1024f,10:F0} KB  {u.path}")));
        Debug.Log($"[Audit] {unused.Count} unused assets ({total / (1024 * 1024)} MB) -> {report}");
    }
}

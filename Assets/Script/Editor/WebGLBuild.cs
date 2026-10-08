using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Builds the game for WebGL into Builds/WebGL.
/// Run from the menu (Build > WebGL) or headless:
///   Unity -batchmode -projectPath <proj> -executeMethod WebGLBuild.BuildWebGL -quit
/// Compression is disabled so the output works on any static host with no
/// special Content-Encoding headers.
/// </summary>
public static class WebGLBuild
{
    private const string OutputPath = "Builds/WebGL";

    [MenuItem("Build/WebGL")]
    public static void BuildWebGL()
    {
        // Uncompressed output keeps deployment simple: any static file server works
        // and local testing via `python3 -m http.server` just works.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.dataCaching = true;
        // Minimal template has no Unity logo/loader UI — just the canvas.
        PlayerSettings.WebGL.template = "APPLICATION:Minimal";

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

    private static string[] EnabledScenes()
    {
        var scenes = new List<string>();
        foreach (var scene in EditorBuildSettings.scenes)
            if (scene.enabled) scenes.Add(scene.path);
        return scenes.ToArray();
    }
}

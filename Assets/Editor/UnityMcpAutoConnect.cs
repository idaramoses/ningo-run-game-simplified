using MCPForUnity.Editor.Services;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class UnityMcpAutoConnect
{
    static UnityMcpAutoConnect()
    {
        EditorApplication.delayCall += StartBridge;
    }

    private static async void StartBridge()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;

        EditorConfigurationCache.Instance.SetUseHttpTransport(false);
        EditorPrefs.SetBool("MCPForUnity.ResumeStdioAfterReload", true);

        bool started = await MCPServiceLocator.Bridge.StartAsync();
        Debug.Log(started
            ? "[UnityMcpAutoConnect] Unity MCP stdio bridge started."
            : "[UnityMcpAutoConnect] Unity MCP bridge was already running or could not start.");
    }
}

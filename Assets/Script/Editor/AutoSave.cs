using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Automatically saves open scenes and project assets:
/// - Every X minutes (configurable below)
/// - Right before entering Play Mode
/// Toggle via menu: Tools > Auto Save > Enabled
/// </summary>
[InitializeOnLoad]
public static class AutoSave
{
    private const float SaveIntervalMinutes = 5f;
    private const string PrefKey = "AutoSave_Enabled";
    private const string MenuPath = "Tools/Auto Save/Enabled";

    private static double _nextSaveTime;

    static AutoSave()
    {
        _nextSaveTime = EditorApplication.timeSinceStartup + SaveIntervalMinutes * 60f;
        EditorApplication.update += OnEditorUpdate;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static bool Enabled
    {
        get => EditorPrefs.GetBool(PrefKey, true);
        set => EditorPrefs.SetBool(PrefKey, value);
    }

    [MenuItem(MenuPath)]
    private static void ToggleEnabled()
    {
        Enabled = !Enabled;
        Debug.Log($"[AutoSave] {(Enabled ? "Enabled" : "Disabled")}");
    }

    [MenuItem(MenuPath, true)]
    private static bool ToggleEnabledValidate()
    {
        Menu.SetChecked(MenuPath, Enabled);
        return true;
    }

    private static void OnEditorUpdate()
    {
        if (!Enabled) return;
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (EditorApplication.timeSinceStartup < _nextSaveTime) return;

        _nextSaveTime = EditorApplication.timeSinceStartup + SaveIntervalMinutes * 60f;
        SaveAll("timed");
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (!Enabled) return;

        // Save just before entering Play Mode so a crash never loses work
        if (state == PlayModeStateChange.ExitingEditMode)
            SaveAll("before Play Mode");
    }

    private static void SaveAll(string reason)
    {
        // Don't interrupt the user mid-drag or while a modal is open
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;

        bool sceneDirty = false;
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            if (EditorSceneManager.GetSceneAt(i).isDirty)
            {
                sceneDirty = true;
                break;
            }
        }

        if (sceneDirty)
        {
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();
            Debug.Log($"[AutoSave] Saved open scenes + assets ({reason}) at {System.DateTime.Now:HH:mm:ss}");
        }
    }
}

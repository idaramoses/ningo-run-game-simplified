using System.Linq;
using UnityEditor;
using UnityEngine;

public static class TMPDiagnostics
{
    [MenuItem("Tools/TMP/Find Broken TextMeshPro Objects")]
    public static void FindBrokenTMPObjects()
    {
        int broken = 0;
        int total = 0;

        var allObjects = Resources.FindObjectsOfTypeAll(typeof(TMPro.TextMeshProUGUI)) as TMPro.TextMeshProUGUI[];

        foreach (var text in allObjects)
        {
            if (EditorUtility.IsPersistent(text.gameObject)) continue; // skip assets

            total++;
            bool isBroken = false;
            string issues = "";

            if (text.font == null) { isBroken = true; issues += "missing font, "; }
            if (text.materialForRendering == null) { isBroken = true; issues += "missing material, "; }
            if (text.canvasRenderer == null) { isBroken = true; issues += "missing canvasRenderer, "; }
            if (text.rectTransform == null) { isBroken = true; issues += "missing rectTransform, "; }
            if (text.maskable && text.GetComponentInParent<UnityEngine.UI.RectMask2D>() != null && text.canvas == null) { isBroken = true; issues += "no canvas under mask, "; }

            if (isBroken)
            {
                broken++;
                Debug.LogError($"Broken TMP object: {text.name} in scene {text.gameObject.scene.name}. Issues: {issues.TrimEnd(',', ' ')}", text.gameObject);
            }
        }

        Debug.Log($"TMP diagnostic complete. Total active TMP objects: {total}, broken: {broken}");
    }
}

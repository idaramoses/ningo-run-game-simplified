using UnityEngine;

public static class TMPAutoFix
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CheckBrokenTMPObjects()
    {
        var allText = Resources.FindObjectsOfTypeAll<TMPro.TextMeshProUGUI>();
        int brokenCount = 0;

        foreach (var text in allText)
        {
            if (text == null) continue;

            // Only check objects that are actually in a scene. Skip prefabs, assets, and editor-only objects.
            if (text.gameObject == null || !text.gameObject.scene.IsValid())
                continue;

            // Access the private m_canvasRenderer field via reflection.
            var field = typeof(TMPro.TextMeshProUGUI).GetField("m_canvasRenderer",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var cachedRenderer = field?.GetValue(text) as CanvasRenderer;

            if (cachedRenderer == null)
            {
                // Try to restore the cached reference from the actual CanvasRenderer component.
                var actualRenderer = text.GetComponent<CanvasRenderer>();
                if (actualRenderer != null && field != null)
                {
                    field.SetValue(text, actualRenderer);
                    Debug.Log($"[TMPAutoFix] Restored missing CanvasRenderer reference on '{text.name}'.", text.gameObject);
                    continue;
                }

                Debug.LogError($"[TMPAutoFix] Broken TextMeshProUGUI on '{text.name}' (CanvasRenderer: null, Font: {(text.font == null ? "null" : "ok")}). Use Tools > TMP > Find Broken TextMeshPro Objects to locate and fix it.", text.gameObject);
                brokenCount++;
            }
        }

        if (brokenCount > 0)
            Debug.Log($"[TMPAutoFix] Found {brokenCount} broken TextMeshProUGUI object(s). They were NOT modified; fix them manually to stop NullReferenceExceptions.");
        else
            Debug.Log("[TMPAutoFix] No broken TextMeshProUGUI components found.");
    }
}

using UnityEngine;

/// <summary>
/// Auto-detects low-end devices at startup and reduces render cost so the
/// game stays smooth on smaller/older phones. Runs before the first scene
/// loads - no scene wiring needed.
/// </summary>
public static class PerfTuner
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Tune()
    {
#if UNITY_ANDROID || UNITY_IOS
        // GPU memory is not a reliable device-tier signal on shared-memory phones.
        int mem = SystemInfo.systemMemorySize;        // MB; zero means unknown
        int cores = SystemInfo.processorCount;
        int gpuMem = SystemInfo.graphicsMemorySize;   // diagnostic only
        bool lowEnd = (mem > 0 && mem < 4096) || (cores > 0 && cores <= 4);
        bool veryLow = (mem > 0 && mem < 3072) || (cores > 0 && cores <= 2);

        // Save on lighting, not resolution or distant-road texture clarity.
        QualitySettings.globalTextureMipmapLimit = 0;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
        Application.targetFrameRate = 60;

        if (veryLow)
        {
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.shadowDistance = 0f;
            QualitySettings.pixelLightCount = 0;
            QualitySettings.lodBias = 0.5f;
            Application.targetFrameRate = 30;
            Time.fixedDeltaTime = 1f / 30f; // match physics rate to frame rate
        }
        else if (lowEnd)
        {
            QualitySettings.shadows = ShadowQuality.HardOnly;
            QualitySettings.shadowDistance = 15f;
            QualitySettings.shadowResolution = ShadowResolution.Low;
            QualitySettings.shadowCascades = 0;
            QualitySettings.pixelLightCount = 1;
            QualitySettings.lodBias = 0.7f;
        }

        Debug.Log($"[PerfTuner] mem={mem}MB cores={cores} gpu={gpuMem}MB lowEnd={lowEnd} veryLow={veryLow} quality={QualitySettings.names[QualitySettings.GetQualityLevel()]} resolution={Screen.width}x{Screen.height} textureLimit={QualitySettings.globalTextureMipmapLimit} filtering={QualitySettings.anisotropicFiltering} targetFPS={Application.targetFrameRate}");
#endif
    }
}

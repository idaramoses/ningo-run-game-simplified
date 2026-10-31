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
        // Device tier detection
        int mem = SystemInfo.systemMemorySize;        // MB
        int cores = SystemInfo.processorCount;
        int gpuMem = SystemInfo.graphicsMemorySize;   // MB
        bool lowEnd = mem < 4096 || cores <= 4 || gpuMem < 1000;
        bool veryLow = mem < 3072 || cores <= 2;

        if (veryLow)
        {
            // Weakest devices: maximum savings, still playable
            QualitySettings.shadows = ShadowQuality.Disable;
            QualitySettings.shadowDistance = 0f;
            QualitySettings.pixelLightCount = 0;
            QualitySettings.lodBias = 0.5f;
            QualitySettings.globalTextureMipmapLimit = 1; // halve texture memory/bandwidth
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            Application.targetFrameRate = 30;
        }
        else if (lowEnd)
        {
            QualitySettings.shadows = ShadowQuality.HardOnly;
            QualitySettings.shadowDistance = 15f;
            QualitySettings.shadowResolution = ShadowResolution.Low;
            QualitySettings.shadowCascades = 0;
            QualitySettings.pixelLightCount = 1;
            QualitySettings.lodBias = 0.7f;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            Application.targetFrameRate = 60;
        }

        if (lowEnd)
            Debug.Log($"[PerfTuner] Low-end device (mem={mem}MB cores={cores} gpu={gpuMem}MB) veryLow={veryLow} - reduced render cost");
#endif
    }
}

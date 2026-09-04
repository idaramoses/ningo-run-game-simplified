using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ResponsiveCameraController : MonoBehaviour
{
    [Header("Aspect Ratio Settings")]
    [Tooltip("Target aspect ratio (e.g., 16:9 = 1.777)")]
    public float targetAspect = 16f / 9f;
    
    [Tooltip("Enable letterboxing/pillarboxing for extreme aspect ratios")]
    public bool enableLetterboxing = true;
    
    [Header("Field of View Adjustment")]
    [Tooltip("Adjust FOV based on aspect ratio")]
    public bool adjustFOV = false;
    
    [Tooltip("Base FOV for target aspect ratio")]
    public float baseFOV = 60f;
    
    private Camera cam;
    private float lastAspect = 0f;

    void Awake()
    {
        cam = GetComponent<Camera>();
        UpdateCameraSettings();
    }

    void Update()
    {
        float currentAspect = (float)Screen.width / Screen.height;
        
        if (Mathf.Abs(currentAspect - lastAspect) > 0.01f)
        {
            UpdateCameraSettings();
            lastAspect = currentAspect;
        }
    }

    void UpdateCameraSettings()
    {
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (enableLetterboxing)
        {
            if (scaleHeight < 1.0f)
            {
                Rect rect = cam.rect;
                rect.width = 1.0f;
                rect.height = scaleHeight;
                rect.x = 0;
                rect.y = (1.0f - scaleHeight) / 2.0f;
                cam.rect = rect;
            }
            else
            {
                float scaleWidth = 1.0f / scaleHeight;
                Rect rect = cam.rect;
                rect.width = scaleWidth;
                rect.height = 1.0f;
                rect.x = (1.0f - scaleWidth) / 2.0f;
                rect.y = 0;
                cam.rect = rect;
            }
        }

        if (adjustFOV && cam.orthographic == false)
        {
            float fovMultiplier = Mathf.Clamp(windowAspect / targetAspect, 0.8f, 1.2f);
            cam.fieldOfView = baseFOV * fovMultiplier;
        }

        Debug.Log($"[ResponsiveCameraController] Screen: {Screen.width}x{Screen.height}, Aspect: {windowAspect:F2}, Target: {targetAspect:F2}");
    }
}

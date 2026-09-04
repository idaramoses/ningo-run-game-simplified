using UnityEngine;

public class PropSway : MonoBehaviour
{
    [Header("What to Sway")]
    [Tooltip("For billboards, signs, flags, etc.")]
    [SerializeField] private SwayType swayType = SwayType.Billboard;
    
    [Header("Sway Settings")]
    [SerializeField] private float swayAmount = 1f;
    [SerializeField] private float swaySpeed = 2f;
    
    [Header("Wind")]
    [SerializeField] private bool enableWind = true;
    [SerializeField] private float windChangeSpeed = 0.3f;
    [SerializeField] private float maxWindStrength = 2f;
    
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private float swayOffset;
    private float windOffset;

    public enum SwayType
    {
        Billboard,      // Sways like a sign on a pole
        Flag,          // Waves like a flag
        Tree,          // Gentle tree movement
        Grass,         // Fast grass movement
        Custom         // Use custom settings
    }

    void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        originalScale = transform.localScale;

#if UNITY_ANDROID || UNITY_IOS
        // Per-prop wind sway is too expensive on mobile. Keep props static.
        enabled = false;
        return;
#endif

        swayOffset = Random.Range(0f, 100f);
        windOffset = Random.Range(0f, 100f);
        
        ApplyPreset();
    }

    void ApplyPreset()
    {
        switch (swayType)
        {
            case SwayType.Billboard:
                swayAmount = 1.5f;
                swaySpeed = 1.5f;
                windChangeSpeed = 0.5f;
                maxWindStrength = 1.5f;
                break;
                
            case SwayType.Flag:
                swayAmount = 3f;
                swaySpeed = 4f;
                windChangeSpeed = 0.8f;
                maxWindStrength = 2.5f;
                break;
                
            case SwayType.Tree:
                swayAmount = 2f;
                swaySpeed = 1f;
                windChangeSpeed = 0.3f;
                maxWindStrength = 1.5f;
                break;
                
            case SwayType.Grass:
                swayAmount = 4f;
                swaySpeed = 3f;
                windChangeSpeed = 1f;
                maxWindStrength = 3f;
                break;
        }
    }

    void Update()
    {
        float windStrength = 1f;
        
        if (enableWind)
        {
            windOffset += Time.deltaTime * windChangeSpeed;
            windStrength = Mathf.PerlinNoise(windOffset, 0f) * maxWindStrength;
        }
        
        swayOffset += Time.deltaTime * swaySpeed;
        
        switch (swayType)
        {
            case SwayType.Billboard:
                SwayBillboard(windStrength);
                break;
                
            case SwayType.Flag:
                SwayFlag(windStrength);
                break;
                
            case SwayType.Tree:
            case SwayType.Grass:
            case SwayType.Custom:
                SwayGeneric(windStrength);
                break;
        }
    }

    void SwayBillboard(float windStrength)
    {
        // Billboard sways on Y axis (rotation) and slight tilt
        float rotY = Mathf.Sin(swayOffset) * swayAmount * windStrength;
        float rotZ = Mathf.Sin(swayOffset * 1.3f) * swayAmount * 0.5f * windStrength;
        
        Quaternion swayRot = Quaternion.Euler(0f, rotY, rotZ);
        transform.localRotation = originalRotation * swayRot;
    }

    void SwayFlag(float windStrength)
    {
        // Flag waves with position and rotation
        float waveX = Mathf.Sin(swayOffset) * swayAmount * 0.02f * windStrength;
        float waveY = Mathf.Sin(swayOffset * 2f) * swayAmount * 0.01f * windStrength;
        
        transform.localPosition = originalPosition + new Vector3(waveX, waveY, 0f);
        
        float rotZ = Mathf.Sin(swayOffset * 1.5f) * swayAmount * windStrength;
        transform.localRotation = originalRotation * Quaternion.Euler(0f, 0f, rotZ);
    }

    void SwayGeneric(float windStrength)
    {
        // Generic sway for trees, grass, etc.
        float swayX = Mathf.Sin(swayOffset) * swayAmount * windStrength;
        float swayZ = Mathf.Sin(swayOffset * 0.8f) * swayAmount * 0.5f * windStrength;
        
        Quaternion swayRot = Quaternion.Euler(swayX, 0f, swayZ);
        transform.localRotation = originalRotation * swayRot;
    }

    void OnDisable()
    {
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
        transform.localScale = originalScale;
    }
}

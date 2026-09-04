using UnityEngine;

public class SkyboxRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("Base speed of skybox rotation")]
    [SerializeField] private float rotationSpeed = 0.5f;
    
    [Header("Realistic Wind Movement")]
    [Tooltip("Enable back-and-forth rotation (like real wind)")]
    [SerializeField] private bool realisticWind = true;
    
    [Tooltip("How often wind changes direction (seconds)")]
    [SerializeField] private float windChangeSpeed = 3f;
    
    [Tooltip("Maximum rotation range (degrees left/right from center)")]
    [SerializeField] private float windSwayRange = 15f;
    
    [Header("Optional Settings")]
    [SerializeField] private bool enableRotation = true;
    [SerializeField] private bool randomizeStartRotation = true;

    [Header("Performance")]
    [Tooltip("How often the skybox material is updated (seconds). Higher values reduce CPU overhead.")]
    [SerializeField] private float materialUpdateInterval = 0.1f;

    private Material skyboxMaterial;
    private float currentRotation = 0f;
    private float windOffset = 0f;
    private float updateTimer = 0f;

    void Start()
    {
        skyboxMaterial = RenderSettings.skybox;
        
        if (skyboxMaterial == null)
        {
            enabled = false;
            return;
        }
        
        if (randomizeStartRotation)
        {
            currentRotation = Random.Range(0f, 360f);
        }
        
        windOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (!enableRotation || skyboxMaterial == null) return;
        
        if (realisticWind)
        {
            windOffset += Time.deltaTime * windChangeSpeed;
            
            float windSway = Mathf.PerlinNoise(windOffset, 0f) * 2f - 1f;
            windSway *= windSwayRange;
            
            currentRotation += windSway * rotationSpeed * Time.deltaTime;
        }
        else
        {
            currentRotation += rotationSpeed * Time.deltaTime;
        }
        
        if (currentRotation >= 360f)
            currentRotation -= 360f;
        else if (currentRotation < 0f)
            currentRotation += 360f;

        updateTimer += Time.deltaTime;
        if (updateTimer >= materialUpdateInterval)
        {
            updateTimer = 0f;
            skyboxMaterial.SetFloat("_Rotation", currentRotation);
        }
    }
    
    public void SetRotationSpeed(float speed)
    {
        rotationSpeed = speed;
    }
    
    public void SetEnabled(bool enabled)
    {
        enableRotation = enabled;
    }
    
    public void ResetRotation()
    {
        currentRotation = 0f;
        if (skyboxMaterial != null)
            skyboxMaterial.SetFloat("_Rotation", 0f);
    }
}

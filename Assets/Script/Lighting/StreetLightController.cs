using UnityEngine;

public class StreetLightController : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private Color lightColor = new Color(1.0f, 0.9f, 0.7f);
    [SerializeField] private float lightIntensity = 2.0f;
    [SerializeField] private float lightRange = 15f;
    [SerializeField] private bool castShadows = false;
    
    [Header("Manual Position (Drag light to bulb location)")]
    [Tooltip("Create a child GameObject and position it at the bulb, then assign it here")]
    [SerializeField] private Transform lightPosition;
    
    [Header("Debug")]
    [SerializeField] private bool showGizmo = true;
    [SerializeField] private float gizmoSize = 0.3f;
    
    private Light streetLight;
    
    void Start()
    {
        SetupStreetLight();
    }
    
    public void SetupStreetLight()
    {
        if (lightPosition == null)
        {
            streetLight = GetComponentInChildren<Light>();
            if (streetLight != null)
            {
                lightPosition = streetLight.transform;
            }
        }
        
        if (lightPosition != null)
        {
            streetLight = lightPosition.GetComponent<Light>();
            
            if (streetLight == null)
            {
                streetLight = lightPosition.gameObject.AddComponent<Light>();
            }
            
            streetLight.type = LightType.Point;
            streetLight.color = lightColor;
            streetLight.intensity = lightIntensity;
            streetLight.range = lightRange;
            streetLight.shadows = castShadows ? LightShadows.Soft : LightShadows.None;
            streetLight.renderMode = LightRenderMode.Auto;
            streetLight.enabled = true;
        }
        else
        {
            Debug.LogWarning($"No light position set for {gameObject.name}. Create a child object at the bulb location and assign it to 'Light Position'");
        }
    }
    
    public void TurnOn()
    {
        if (streetLight != null)
            streetLight.enabled = true;
    }
    
    public void TurnOff()
    {
        if (streetLight != null)
            streetLight.enabled = false;
    }
    
    public void SetIntensity(float intensity)
    {
        lightIntensity = intensity;
        if (streetLight != null)
            streetLight.intensity = intensity;
    }
    
    void OnValidate()
    {
        if (streetLight != null)
        {
            streetLight.color = lightColor;
            streetLight.intensity = lightIntensity;
            streetLight.range = lightRange;
            streetLight.shadows = castShadows ? LightShadows.Hard : LightShadows.None;
            streetLight.renderMode = LightRenderMode.Auto;
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showGizmo) return;
        
        if (lightPosition != null)
        {
            Gizmos.color = lightColor;
            Gizmos.DrawWireSphere(lightPosition.position, gizmoSize);
            Gizmos.DrawSphere(lightPosition.position, gizmoSize * 0.5f);
            
            Gizmos.color = new Color(lightColor.r, lightColor.g, lightColor.b, 0.2f);
            Gizmos.DrawWireSphere(lightPosition.position, lightRange);
        }
    }
}

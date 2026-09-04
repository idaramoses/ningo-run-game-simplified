using UnityEngine;

public class CarHeadlightController : MonoBehaviour
{
    [Header("Headlight Settings")]
    [SerializeField] private Color headlightColor = new Color(1.0f, 0.95f, 0.85f);
    [SerializeField] private float headlightIntensity = 3.0f;
    [SerializeField] private float headlightRange = 20f;
    [SerializeField] private float spotAngle = 60f;
    [SerializeField] private bool castShadows = false;
    
    [Header("Headlight Positions (Local Space)")]
    [SerializeField] private Vector3 leftHeadlightPosition = new Vector3(-0.5f, 0.5f, 1.5f);
    [SerializeField] private Vector3 rightHeadlightPosition = new Vector3(0.5f, 0.5f, 1.5f);
    [SerializeField] private Vector3 headlightRotation = new Vector3(0, 0, 0);
    
    [Header("Auto Setup")]
    [SerializeField] private bool autoAddLights = true;
    [SerializeField] private bool lightsOn = true;
    
    private Light leftHeadlight;
    private Light rightHeadlight;
    
    void Start()
    {
        if (autoAddLights)
        {
            SetupHeadlights();
        }
    }
    
    public void SetupHeadlights()
    {
        CreateHeadlight(ref leftHeadlight, "Headlight_Left", leftHeadlightPosition);
        CreateHeadlight(ref rightHeadlight, "Headlight_Right", rightHeadlightPosition);
        
        if (lightsOn)
        {
            TurnOn();
        }
        else
        {
            TurnOff();
        }
        
        Debug.Log($"Car headlights configured on {gameObject.name}");
    }
    
    private void CreateHeadlight(ref Light headlight, string name, Vector3 position)
    {
        Transform existingLight = transform.Find(name);
        
        if (existingLight != null)
        {
            headlight = existingLight.GetComponent<Light>();
        }
        
        if (headlight == null)
        {
            GameObject lightObj = new GameObject(name);
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = position;
            lightObj.transform.localRotation = Quaternion.Euler(headlightRotation);
            
            headlight = lightObj.AddComponent<Light>();
        }
        
        headlight.type = LightType.Spot;
        headlight.color = headlightColor;
        headlight.intensity = headlightIntensity;
        headlight.range = headlightRange;
        headlight.spotAngle = spotAngle;
        headlight.shadows = castShadows ? LightShadows.Hard : LightShadows.None;
        headlight.renderMode = LightRenderMode.Auto;
        headlight.enabled = true;
    }
    
    public void TurnOn()
    {
        if (leftHeadlight != null)
            leftHeadlight.enabled = true;
        if (rightHeadlight != null)
            rightHeadlight.enabled = true;
    }
    
    public void TurnOff()
    {
        if (leftHeadlight != null)
            leftHeadlight.enabled = false;
        if (rightHeadlight != null)
            rightHeadlight.enabled = false;
    }
    
    public void SetIntensity(float intensity)
    {
        headlightIntensity = intensity;
        if (leftHeadlight != null)
            leftHeadlight.intensity = intensity;
        if (rightHeadlight != null)
            rightHeadlight.intensity = intensity;
    }
    
    void OnValidate()
    {
        if (leftHeadlight != null)
        {
            leftHeadlight.color = headlightColor;
            leftHeadlight.intensity = headlightIntensity;
            leftHeadlight.range = headlightRange;
            leftHeadlight.spotAngle = spotAngle;
            leftHeadlight.shadows = castShadows ? LightShadows.Soft : LightShadows.None;
            leftHeadlight.transform.localPosition = leftHeadlightPosition;
            leftHeadlight.transform.localRotation = Quaternion.Euler(headlightRotation);
        }
        
        if (rightHeadlight != null)
        {
            rightHeadlight.color = headlightColor;
            rightHeadlight.intensity = headlightIntensity;
            rightHeadlight.range = headlightRange;
            rightHeadlight.spotAngle = spotAngle;
            rightHeadlight.shadows = castShadows ? LightShadows.Soft : LightShadows.None;
            rightHeadlight.transform.localPosition = rightHeadlightPosition;
            rightHeadlight.transform.localRotation = Quaternion.Euler(headlightRotation);
        }
    }
}

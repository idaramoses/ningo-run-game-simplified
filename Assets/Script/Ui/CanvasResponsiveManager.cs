using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
public class CanvasResponsiveManager : MonoBehaviour
{
    [Header("Canvas Scaler Settings")]
    [SerializeField] private Vector2 referenceResolution = new Vector2(1080, 1920);
    [SerializeField] private float matchWidthOrHeight = 0.5f;
    
    [Header("Adaptive Settings")]
    [SerializeField] private bool adaptToOrientation = true;
    [SerializeField] private bool adaptToAspectRatio = true;
    
    [Header("Orientation-Specific Settings")]
    [SerializeField] private Vector2 portraitReferenceResolution = new Vector2(1080, 1920);
    [SerializeField] private Vector2 landscapeReferenceResolution = new Vector2(1920, 1080);
    [SerializeField] private float portraitMatch = 0.5f;
    [SerializeField] private float landscapeMatch = 0.5f;
    
    [Header("Aspect Ratio Adjustments")]
    [SerializeField] private bool useAspectRatioFitting = true;
    [SerializeField] private float ultraWideMatchAdjustment = 0.2f;
    [SerializeField] private float wideMatchAdjustment = 0.1f;
    
    private CanvasScaler canvasScaler;
    private Canvas canvas;
    private ScreenOrientation lastOrientation;
    private float lastAspectRatio;
    private Vector2Int lastScreenSize;
    
    void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvasScaler = GetComponent<CanvasScaler>();
        
        ConfigureCanvasScaler();
        lastOrientation = Screen.orientation;
        lastAspectRatio = GetAspectRatio();
        lastScreenSize = new Vector2Int(Screen.width, Screen.height);
    }
    
    void Start()
    {
        ApplyResponsiveSettings();
    }
    
    void Update()
    {
        bool screenChanged = false;
        
        if (lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height)
        {
            screenChanged = true;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        }
        
        if (adaptToOrientation && lastOrientation != Screen.orientation)
        {
            screenChanged = true;
            lastOrientation = Screen.orientation;
        }
        
        if (adaptToAspectRatio && Mathf.Abs(lastAspectRatio - GetAspectRatio()) > 0.01f)
        {
            screenChanged = true;
            lastAspectRatio = GetAspectRatio();
        }
        
        if (screenChanged)
        {
            ApplyResponsiveSettings();
        }
    }
    
    void ConfigureCanvasScaler()
    {
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
    }
    
    void ApplyResponsiveSettings()
    {
        bool isPortrait = Screen.height > Screen.width;
        float aspectRatio = GetAspectRatio();
        
        if (adaptToOrientation)
        {
            if (isPortrait)
            {
                canvasScaler.referenceResolution = portraitReferenceResolution;
                canvasScaler.matchWidthOrHeight = portraitMatch;
            }
            else
            {
                canvasScaler.referenceResolution = landscapeReferenceResolution;
                canvasScaler.matchWidthOrHeight = landscapeMatch;
            }
        }
        else
        {
            canvasScaler.referenceResolution = referenceResolution;
            canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
        }
        
        if (useAspectRatioFitting)
        {
            float matchAdjustment = 0f;
            
            if (aspectRatio >= 2.1f)
            {
                matchAdjustment = ultraWideMatchAdjustment;
            }
            else if (aspectRatio >= 1.9f)
            {
                matchAdjustment = wideMatchAdjustment;
            }
            
            canvasScaler.matchWidthOrHeight = Mathf.Clamp01(canvasScaler.matchWidthOrHeight + matchAdjustment);
        }
        
        Debug.Log($"[CanvasResponsiveManager] Updated canvas settings - Resolution: {canvasScaler.referenceResolution}, Match: {canvasScaler.matchWidthOrHeight:F2}, Aspect: {aspectRatio:F2}, Portrait: {isPortrait}");
    }
    
    float GetAspectRatio()
    {
        return (float)Screen.width / Screen.height;
    }
    
    public void SetReferenceResolution(Vector2 resolution)
    {
        referenceResolution = resolution;
        ApplyResponsiveSettings();
    }
    
    public void SetMatchWidthOrHeight(float match)
    {
        matchWidthOrHeight = Mathf.Clamp01(match);
        ApplyResponsiveSettings();
    }
}

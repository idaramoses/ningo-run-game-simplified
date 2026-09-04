using UnityEngine;
using UnityEngine.Events;

public class OrientationHandler : MonoBehaviour
{
    [Header("Orientation Events")]
    public UnityEvent onPortrait;
    public UnityEvent onLandscape;
    public UnityEvent onOrientationChange;
    
    [Header("Auto-Adjust Settings")]
    [SerializeField] private bool autoAdjustLayout = true;
    [SerializeField] private RectTransform targetPanel;
    
    [Header("Portrait Layout")]
    [SerializeField] private Vector2 portraitAnchorMin = new Vector2(0, 0);
    [SerializeField] private Vector2 portraitAnchorMax = new Vector2(1, 1);
    [SerializeField] private Vector2 portraitPivot = new Vector2(0.5f, 0.5f);
    
    [Header("Landscape Layout")]
    [SerializeField] private Vector2 landscapeAnchorMin = new Vector2(0, 0);
    [SerializeField] private Vector2 landscapeAnchorMax = new Vector2(1, 1);
    [SerializeField] private Vector2 landscapePivot = new Vector2(0.5f, 0.5f);
    
    private ScreenOrientation lastOrientation;
    private bool isPortrait;
    
    void Awake()
    {
        lastOrientation = Screen.orientation;
        isPortrait = Screen.height > Screen.width;
        
        if (targetPanel == null)
        {
            targetPanel = GetComponent<RectTransform>();
        }
    }
    
    void Start()
    {
        ApplyOrientation();
    }
    
    void Update()
    {
        if (lastOrientation != Screen.orientation || isPortrait != (Screen.height > Screen.width))
        {
            lastOrientation = Screen.orientation;
            isPortrait = Screen.height > Screen.width;
            ApplyOrientation();
        }
    }
    
    void ApplyOrientation()
    {
        if (isPortrait)
        {
            onPortrait?.Invoke();
            
            if (autoAdjustLayout && targetPanel != null)
            {
                targetPanel.anchorMin = portraitAnchorMin;
                targetPanel.anchorMax = portraitAnchorMax;
                targetPanel.pivot = portraitPivot;
            }
            
            Debug.Log("[OrientationHandler] Switched to Portrait mode");
        }
        else
        {
            onLandscape?.Invoke();
            
            if (autoAdjustLayout && targetPanel != null)
            {
                targetPanel.anchorMin = landscapeAnchorMin;
                targetPanel.anchorMax = landscapeAnchorMax;
                targetPanel.pivot = landscapePivot;
            }
            
            Debug.Log("[OrientationHandler] Switched to Landscape mode");
        }
        
        onOrientationChange?.Invoke();
    }
    
    public bool IsPortrait()
    {
        return isPortrait;
    }
    
    public bool IsLandscape()
    {
        return !isPortrait;
    }
}

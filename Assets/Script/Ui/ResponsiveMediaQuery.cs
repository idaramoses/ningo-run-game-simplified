using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class ResponsiveMediaQuery : MonoBehaviour
{
    [Header("Breakpoints Configuration")]
    [Tooltip("Reference to ScreenBreakpoints ScriptableObject")]
    public ScreenBreakpoints breakpoints;
    
    [Header("Device Type Rules")]
    public MediaQueryRule[] deviceRules;
    
    [Header("Aspect Ratio Rules")]
    public MediaQueryRule[] aspectRatioRules;
    
    [Header("Orientation Rules")]
    public MediaQueryRule[] orientationRules;
    
    [Header("Custom Rules")]
    public CustomMediaQuery[] customQueries;
    
    private ScreenBreakpoints.DeviceType currentDeviceType;
    private ScreenBreakpoints.AspectRatioType currentAspectRatio;
    private bool currentIsPortrait;
    private Vector2Int lastScreenSize;
    
    [Serializable]
    public class MediaQueryRule
    {
        public string ruleName;
        public bool enabled = true;
        
        [Header("Conditions")]
        public ScreenBreakpoints.DeviceType deviceType;
        public ScreenBreakpoints.AspectRatioType aspectRatioType;
        public bool isPortrait;
        
        [Header("Actions")]
        public GameObject[] objectsToActivate;
        public GameObject[] objectsToDeactivate;
        public ComponentToggle[] componentsToToggle;
        public TransformAdjustment[] transformAdjustments;
        public UnityEvent onRuleMatch;
    }
    
    [Serializable]
    public class CustomMediaQuery
    {
        public string queryName;
        public bool enabled = true;
        
        [Header("Custom Conditions")]
        public int minWidth;
        public int maxWidth = 9999;
        public int minHeight;
        public int maxHeight = 9999;
        public float minAspectRatio;
        public float maxAspectRatio = 10f;
        
        [Header("Actions")]
        public GameObject[] objectsToActivate;
        public GameObject[] objectsToDeactivate;
        public UnityEvent onQueryMatch;
        
        public bool Matches()
        {
            if (!enabled) return false;
            
            bool widthMatch = Screen.width >= minWidth && Screen.width <= maxWidth;
            bool heightMatch = Screen.height >= minHeight && Screen.height <= maxHeight;
            float aspect = (float)Screen.width / Screen.height;
            bool aspectMatch = aspect >= minAspectRatio && aspect <= maxAspectRatio;
            
            return widthMatch && heightMatch && aspectMatch;
        }
    }
    
    [Serializable]
    public class ComponentToggle
    {
        public Component component;
        public bool enableState;
    }
    
    [Serializable]
    public class TransformAdjustment
    {
        public RectTransform rectTransform;
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;
        public Vector3 scale = Vector3.one;
    }
    
    void Start()
    {
        if (breakpoints == null)
        {
            Debug.LogWarning("[ResponsiveMediaQuery] No ScreenBreakpoints assigned! Creating default.");
            breakpoints = ScriptableObject.CreateInstance<ScreenBreakpoints>();
        }
        
        EvaluateMediaQueries();
    }
    
    void Update()
    {
        Vector2Int currentScreenSize = new Vector2Int(Screen.width, Screen.height);
        
        if (currentScreenSize != lastScreenSize)
        {
            EvaluateMediaQueries();
            lastScreenSize = currentScreenSize;
        }
    }
    
    void EvaluateMediaQueries()
    {
        currentDeviceType = breakpoints.GetDeviceType();
        currentAspectRatio = breakpoints.GetAspectRatioType();
        currentIsPortrait = breakpoints.IsPortrait();
        
        Debug.Log($"[ResponsiveMediaQuery] Screen: {Screen.width}x{Screen.height}, Device: {currentDeviceType}, Aspect: {currentAspectRatio}, Portrait: {currentIsPortrait}");
        
        // Evaluate device type rules
        foreach (var rule in deviceRules)
        {
            if (!rule.enabled) continue;
            
            if (rule.deviceType == currentDeviceType)
            {
                ApplyRule(rule);
            }
        }
        
        // Evaluate aspect ratio rules
        foreach (var rule in aspectRatioRules)
        {
            if (!rule.enabled) continue;
            
            if (rule.aspectRatioType == currentAspectRatio)
            {
                ApplyRule(rule);
            }
        }
        
        // Evaluate orientation rules
        foreach (var rule in orientationRules)
        {
            if (!rule.enabled) continue;
            
            if (rule.isPortrait == currentIsPortrait)
            {
                ApplyRule(rule);
            }
        }
        
        // Evaluate custom queries
        foreach (var query in customQueries)
        {
            if (query.Matches())
            {
                ApplyCustomQuery(query);
            }
        }
    }
    
    void ApplyRule(MediaQueryRule rule)
    {
        Debug.Log($"[ResponsiveMediaQuery] Applying rule: {rule.ruleName}");
        
        // Activate objects
        foreach (var obj in rule.objectsToActivate)
        {
            if (obj != null) obj.SetActive(true);
        }
        
        // Deactivate objects
        foreach (var obj in rule.objectsToDeactivate)
        {
            if (obj != null) obj.SetActive(false);
        }
        
        // Toggle components
        foreach (var toggle in rule.componentsToToggle)
        {
            if (toggle.component != null)
            {
                if (toggle.component is Behaviour behaviour)
                    behaviour.enabled = toggle.enableState;
            }
        }
        
        // Adjust transforms
        foreach (var adjustment in rule.transformAdjustments)
        {
            if (adjustment.rectTransform != null)
            {
                adjustment.rectTransform.anchoredPosition = adjustment.anchoredPosition;
                adjustment.rectTransform.sizeDelta = adjustment.sizeDelta;
                adjustment.rectTransform.localScale = adjustment.scale;
            }
        }
        
        // Invoke events
        rule.onRuleMatch?.Invoke();
    }
    
    void ApplyCustomQuery(CustomMediaQuery query)
    {
        Debug.Log($"[ResponsiveMediaQuery] Custom query matched: {query.queryName}");
        
        foreach (var obj in query.objectsToActivate)
        {
            if (obj != null) obj.SetActive(true);
        }
        
        foreach (var obj in query.objectsToDeactivate)
        {
            if (obj != null) obj.SetActive(false);
        }
        
        query.onQueryMatch?.Invoke();
    }
    
    // Public API for manual checks
    public bool IsDeviceType(ScreenBreakpoints.DeviceType type)
    {
        return currentDeviceType == type;
    }
    
    public bool IsAspectRatio(ScreenBreakpoints.AspectRatioType type)
    {
        return currentAspectRatio == type;
    }
    
    public bool IsPortrait()
    {
        return currentIsPortrait;
    }
}

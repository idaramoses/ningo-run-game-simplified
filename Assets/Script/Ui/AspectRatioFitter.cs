using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class AspectRatioFitter : MonoBehaviour
{
    [Header("Aspect Ratio Settings")]
    [Tooltip("Target aspect ratio (width / height)")]
    public float targetAspectRatio = 9f / 16f;
    
    [Tooltip("Fit mode")]
    public FitMode fitMode = FitMode.FitInParent;
    
    public enum FitMode
    {
        FitInParent,
        EnvelopeParent
    }
    
    private RectTransform rectTransform;
    private float lastAspect = 0f;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        UpdateAspectRatio();
    }

    void Update()
    {
        float currentAspect = (float)Screen.width / Screen.height;
        
        if (Mathf.Abs(currentAspect - lastAspect) > 0.01f)
        {
            UpdateAspectRatio();
            lastAspect = currentAspect;
        }
    }

    void UpdateAspectRatio()
    {
        if (rectTransform == null) return;
        
        float parentWidth = rectTransform.parent != null 
            ? ((RectTransform)rectTransform.parent).rect.width 
            : Screen.width;
        float parentHeight = rectTransform.parent != null 
            ? ((RectTransform)rectTransform.parent).rect.height 
            : Screen.height;
        
        float parentAspect = parentWidth / parentHeight;
        
        if (fitMode == FitMode.FitInParent)
        {
            if (parentAspect > targetAspectRatio)
            {
                float width = parentHeight * targetAspectRatio;
                rectTransform.sizeDelta = new Vector2(width, parentHeight);
            }
            else
            {
                float height = parentWidth / targetAspectRatio;
                rectTransform.sizeDelta = new Vector2(parentWidth, height);
            }
        }
        else
        {
            if (parentAspect < targetAspectRatio)
            {
                float width = parentHeight * targetAspectRatio;
                rectTransform.sizeDelta = new Vector2(width, parentHeight);
            }
            else
            {
                float height = parentWidth / targetAspectRatio;
                rectTransform.sizeDelta = new Vector2(parentWidth, height);
            }
        }
    }
}

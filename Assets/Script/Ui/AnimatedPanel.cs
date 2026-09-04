using UnityEngine;

/// <summary>
/// Helper component to manage panel visibility with animations
/// </summary>
[RequireComponent(typeof(PanelAnimator))]
public class AnimatedPanel : MonoBehaviour
{
    private PanelAnimator panelAnimator;
    private GameObject panelObject;
    
    private void Awake()
    {
        panelAnimator = GetComponent<PanelAnimator>();
        panelObject = gameObject;
    }
    
    /// <summary>
    /// Show panel with animation
    /// </summary>
    public void Show()
    {
        if (!panelObject.activeSelf)
        {
            panelObject.SetActive(true);
        }
        
        if (panelAnimator != null)
        {
            panelAnimator.AnimateIn();
        }
    }
    
    /// <summary>
    /// Hide panel with animation
    /// </summary>
    public void Hide(System.Action onComplete = null)
    {
        if (panelAnimator != null)
        {
            panelAnimator.AnimateOut(() =>
            {
                panelObject.SetActive(false);
                onComplete?.Invoke();
            });
        }
        else
        {
            panelObject.SetActive(false);
            onComplete?.Invoke();
        }
    }
    
    /// <summary>
    /// Instantly show without animation
    /// </summary>
    public void ShowInstant()
    {
        panelObject.SetActive(true);
    }
    
    /// <summary>
    /// Instantly hide without animation
    /// </summary>
    public void HideInstant()
    {
        panelObject.SetActive(false);
    }
}

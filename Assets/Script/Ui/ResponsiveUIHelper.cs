using UnityEngine;
using UnityEngine.UI;

public class ResponsiveUIHelper : MonoBehaviour
{
    [Header("Responsive Settings")]
    [Tooltip("Adjust font size based on screen height")]
    public bool adjustFontSize = false;
    
    [Tooltip("Base font size at reference resolution (1920px height)")]
    public float baseFontSize = 24f;
    
    [Tooltip("Adjust spacing based on screen size")]
    public bool adjustSpacing = false;
    
    [Tooltip("Base spacing value")]
    public float baseSpacing = 10f;
    
    private Text textComponent;
    private TMPro.TMP_Text tmpTextComponent;
    private LayoutGroup layoutGroup;
    private float lastScreenHeight = 0f;

    void Start()
    {
        textComponent = GetComponent<Text>();
        tmpTextComponent = GetComponent<TMPro.TMP_Text>();
        layoutGroup = GetComponent<LayoutGroup>();
        
        UpdateResponsiveSettings();
    }

    void Update()
    {
        if (Mathf.Abs(Screen.height - lastScreenHeight) > 10f)
        {
            UpdateResponsiveSettings();
            lastScreenHeight = Screen.height;
        }
    }

    void UpdateResponsiveSettings()
    {
        float scaleFactor = Screen.height / 1920f;
        
        if (adjustFontSize)
        {
            if (textComponent != null)
            {
                textComponent.fontSize = Mathf.RoundToInt(baseFontSize * scaleFactor);
            }
            else if (tmpTextComponent != null)
            {
                tmpTextComponent.fontSize = baseFontSize * scaleFactor;
            }
        }
        
        if (adjustSpacing && layoutGroup != null)
        {
            float newSpacing = baseSpacing * scaleFactor;
            
            if (layoutGroup is HorizontalLayoutGroup)
            {
                ((HorizontalLayoutGroup)layoutGroup).spacing = newSpacing;
            }
            else if (layoutGroup is VerticalLayoutGroup)
            {
                ((VerticalLayoutGroup)layoutGroup).spacing = newSpacing;
            }
            else if (layoutGroup is GridLayoutGroup)
            {
                ((GridLayoutGroup)layoutGroup).spacing = new Vector2(newSpacing, newSpacing);
            }
        }
    }
}

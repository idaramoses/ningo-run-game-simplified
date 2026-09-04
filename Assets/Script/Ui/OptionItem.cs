using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionItem : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private Image icon;
    [SerializeField] private GameObject selectedIndicator;

    private int index;
    private System.Action<int> onSelected;

    public void Setup(int idx, OptionData data, System.Action<int> callback)
    {
        index = idx;
        onSelected = callback;
        if (label != null) label.text = data.label;
        if (icon != null && data.icon != null) icon.sprite = data.icon;
        SetSelected(false);
    }

    public void Setup(int idx, string labelText, Sprite iconSprite, System.Action<int> callback)
    {
        index = idx;
        onSelected = callback;
        if (label != null) label.text = labelText;
        if (icon != null && iconSprite != null) icon.sprite = iconSprite;
        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (selectedIndicator != null)
            selectedIndicator.SetActive(selected);
    }

    public void OnClick()
    {
        onSelected?.Invoke(index);
    }
}

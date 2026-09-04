using UnityEngine;
using UnityEngine.EventSystems;

public class Button3DEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private Vector3 pressedScale;
    private Vector3 originalPosition;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        pressedScale = originalScale * 0.96f; // slightly smaller
        originalPosition = rectTransform.localPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        rectTransform.localScale = pressedScale;
        rectTransform.localPosition = originalPosition + new Vector3(0, -5f, 0);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        rectTransform.localScale = originalScale;
        rectTransform.localPosition = originalPosition;
    }
}

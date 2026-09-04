using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DoubleTapToClose : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private float doubleTapThreshold = 0.6f;
    [SerializeField] private GameObject canvasToClose;

    private float lastTapTime = 0f;

    private void Awake()
    {
        if (canvasToClose == null)
            canvasToClose = transform.root.gameObject;

        // Ensure this GameObject has an Image so it catches raycasts
        Image img = GetComponent<Image>();
        if (img == null)
            img = gameObject.AddComponent<Image>();

        img.color = new Color(0, 0, 0, 0); // fully transparent
        img.raycastTarget = true;

        // Stretch to fill parent
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        float timeSinceLastTap = Time.time - lastTapTime;
        Debug.Log($"[DoubleTap] Click on {gameObject.name} | timeSince={timeSinceLastTap:F2} | threshold={doubleTapThreshold}");

        if (lastTapTime > 0f && timeSinceLastTap <= doubleTapThreshold && timeSinceLastTap > 0.05f)
        {
            Debug.Log($"[DoubleTap] Double tap detected! Closing {canvasToClose.name}");
            canvasToClose.SetActive(false);

            // Re-enable Home UI
            UImanager homeCtrl = UImanager.uimanager;
            if (homeCtrl != null)
            {
                homeCtrl.HideDailyGift();
            }

            lastTapTime = 0f;
        }
        else
        {
            lastTapTime = Time.time;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controller for the Danfo Chase warning progress bar, indicating how many seconds are left in the current lane.
/// Located at the bottom right of the GameHUD Canvas (above the MagnetProgressBar).
/// Note: LagosTrafficManager has been removed. This progress bar will remain hidden until re-wired.
/// </summary>
public class DanfoChaseWarningProgressBarController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject container;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Transform segmentsParent;

    private UnityEngine.UI.Image[] segmentImages;

    private void Start()
    {
        if (segmentsParent != null)
        {
            segmentImages = segmentsParent.GetComponentsInChildren<UnityEngine.UI.Image>();
        }

        if (container != null)
        {
            container.SetActive(false);
        }
    }

    private void Update()
    {
        // LagosTrafficManager has been removed. This progress bar will remain hidden until re-wired.
        if (container != null && container.activeSelf)
        {
            container.SetActive(false);
        }
    }

    private void UpdateSegments(float progress)
    {
        if (segmentImages == null || segmentImages.Length == 0) return;

        int totalSegments = segmentImages.Length;
        int activeCount = Mathf.CeilToInt(progress * totalSegments);

        for (int i = 0; i < totalSegments; i++)
        {
            if (segmentImages[i] == null) continue;

            Color c = segmentImages[i].color;
            if (i < activeCount)
            {
                c.a = 1.0f;
            }
            else
            {
                c.a = 0.2f;
            }
            segmentImages[i].color = c;
        }
    }
}

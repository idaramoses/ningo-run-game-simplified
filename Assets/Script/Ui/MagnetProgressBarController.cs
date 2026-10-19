using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controller for the Magnet progress bar, indicating how many seconds are left.
/// Located at the bottom right of the GameHUD Canvas.
/// Shows a segmented horizontal progress bar matching the reference image.
/// Note: Magnet tracking requires re-wiring to the active player script.
/// </summary>
public class MagnetProgressBarController : MonoBehaviour
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
        var player = SimplePlayerController.Instance;
        if (player != null && player.IsMagnetActive && player.MagnetTimer > 0f)
        {
            if (container != null && !container.activeSelf)
            {
                container.SetActive(true);
            }

            float duration = player.magnetDuration > 0f ? player.magnetDuration : 10f;
            float progress = Mathf.Clamp01(player.MagnetTimer / duration);
            UpdateSegments(progress);

            if (timerText != null)
            {
                timerText.text = Mathf.CeilToInt(player.MagnetTimer).ToString() + "s";
            }
        }
        else
        {
            if (container != null && container.activeSelf)
            {
                container.SetActive(false);
            }
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

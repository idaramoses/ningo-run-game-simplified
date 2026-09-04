using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controller for the Hang Glider progress bar, indicating how many seconds of flight remain.
/// Located on the Canvas_HUD (same layout approach as DanfoChaseWarningProgressBarController).
/// Note: Gliding state tracking was in SimplePlayerController (now removed).
/// This progress bar will remain hidden until re-wired to the active player.
/// </summary>
public class GliderProgressBarController : MonoBehaviour
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
        // Gliding state tracking was in SimplePlayerController (now removed).
        // This progress bar will remain hidden until re-wired to the active player.
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

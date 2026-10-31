using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controller for powerup progress bars, indicating how many seconds are left.
/// Located at the bottom right of the GameHUD Canvas.
/// Shows a segmented horizontal progress bar.
/// Works for Coin Magnet, Hang Glider and Skateboard - pick the type in the Inspector.
/// </summary>
public class MagnetProgressBarController : MonoBehaviour
{
    public enum PowerupType { Magnet, Glider, Skateboard }

    [Header("Powerup")]
    [SerializeField] private PowerupType powerupType = PowerupType.Magnet;

    [Header("UI References")]
    [SerializeField] private GameObject container;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private Transform segmentsParent;

    private UnityEngine.UI.Image[] segmentImages;
    private int lastShownSeconds = -1;

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

        bool active = false;
        float timer = 0f;
        float duration = 10f;

        if (player != null)
        {
            switch (powerupType)
            {
                case PowerupType.Magnet:
                    active = player.IsMagnetActive;
                    timer = player.MagnetTimer;
                    duration = player.magnetDuration > 0f ? player.magnetDuration : 10f;
                    break;
                case PowerupType.Glider:
                    active = player.IsGliding;
                    timer = player.GliderTimer;
                    duration = player.gliderDuration > 0f ? player.gliderDuration : 10f;
                    break;
                case PowerupType.Skateboard:
                    active = player.IsSkating;
                    timer = player.SkateTimer;
                    duration = player.skateboardDuration > 0f ? player.skateboardDuration : 10f;
                    break;
            }
        }

        if (active && timer > 0f)
        {
            if (container != null && !container.activeSelf)
            {
                container.SetActive(true);
            }

            float progress = Mathf.Clamp01(timer / duration);
            UpdateSegments(progress);

            if (timerText != null)
            {
                int secs = Mathf.CeilToInt(timer);
                if (secs != lastShownSeconds)
                {
                    lastShownSeconds = secs;
                    timerText.text = secs + "s";
                }
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

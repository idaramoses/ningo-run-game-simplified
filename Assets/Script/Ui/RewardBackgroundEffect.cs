using UnityEngine;

/// <summary>
/// Attach to the background-light Image inside Canvas_Activate_Reward,
/// Canvas_Activate_Mystery_Box, and Canvas_Activate_Speedstar.
/// Rotates + pulses the light, bounces the reward item, and dangles the close hint text.
/// </summary>
public class RewardBackgroundEffect : MonoBehaviour
{
    [Header("Background Light")]
    [SerializeField] private RectTransform lightImage;
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private float lightPulseScale = 0.06f;  // subtle size pulse
    [SerializeField] private float lightPulseSpeed = 1.2f;

    [Header("Reward Item Bounce")]
    [SerializeField] private RectTransform rewardItem;
    [SerializeField] private float bounceHeight = 20f;
    [SerializeField] private float bounceSpeed = 1.5f;

    [Header("Double Tap Close Text")]
    [SerializeField] private RectTransform closeTapText;
    [SerializeField] private float textDangleAngle = 8f;   // sway left/right degrees
    [SerializeField] private float textDangleSpeed = 1.2f;
    [SerializeField] private float textBounceHeight = 6f;  // subtle up/down
    [SerializeField] private float textBounceSpeed = 2f;

    private Vector2 rewardStartPos;
    private Vector2 closeTextStartPos;
    private Vector3 lightStartScale;

    private void Awake()
    {
        if (lightImage == null)
            lightImage = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        if (rewardItem != null)
            rewardStartPos = rewardItem.anchoredPosition;
        if (closeTapText != null)
            closeTextStartPos = closeTapText.anchoredPosition;
        if (lightImage != null)
            lightStartScale = lightImage.localScale;
    }

    private void Update()
    {
        float t = Time.time;

        // Rotate background light endlessly
        if (lightImage != null)
        {
            lightImage.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);

            // Subtle pulse scale on the light
            float pulse = 1f + Mathf.Sin(t * lightPulseSpeed * Mathf.PI) * lightPulseScale;
            lightImage.localScale = lightStartScale * pulse;
        }

        // Bounce reward item up and down
        if (rewardItem != null)
        {
            float offsetY = Mathf.Sin(t * bounceSpeed * Mathf.PI) * bounceHeight;
            rewardItem.anchoredPosition = rewardStartPos + new Vector2(0f, offsetY);
        }

        // Dangle + bounce the close hint text
        if (closeTapText != null)
        {
            float angle = Mathf.Sin(t * textDangleSpeed * Mathf.PI) * textDangleAngle;
            float offsetY = Mathf.Sin(t * textBounceSpeed * Mathf.PI) * textBounceHeight;
            closeTapText.localEulerAngles = new Vector3(0f, 0f, angle);
            closeTapText.anchoredPosition = closeTextStartPos + new Vector2(0f, offsetY);
        }
    }
}

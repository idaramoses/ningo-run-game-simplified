using UnityEngine;

/// <summary>
/// Attach to any UI element to give it an endless idle animation.
/// Supports bounce (up/down), dangle (rotation sway), and scale pulse.
/// </summary>
public class IdleButtonEffect : MonoBehaviour
{
    [Header("Bounce (Up/Down)")]
    [SerializeField] private bool enableBounce = true;
    [SerializeField] private float bounceHeight = 12f;
    [SerializeField] private float bounceSpeed = 1.2f;
    [SerializeField] private float bounceOffset = 0f; // phase offset so buttons don't all sync

    [Header("Dangle (Rotation Sway)")]
    [SerializeField] private bool enableDangle = false;
    [SerializeField] private float dangleAngle = 6f;
    [SerializeField] private float dangleSpeed = 1.0f;

    [Header("Scale Pulse")]
    [SerializeField] private bool enablePulse = false;
    [SerializeField] private float pulseAmount = 0.08f;
    [SerializeField] private float pulseSpeed = 1.0f;

    [Header("Time Scale")]
    [SerializeField] private bool useUnscaledTime = true;

    private RectTransform rt;
    private Vector2 startPos;
    private Vector3 startScale;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        startPos = rt.anchoredPosition;
        startScale = rt.localScale;
    }

    private void Update()
    {
        float currentTime = useUnscaledTime ? Time.unscaledTime : Time.time;
        float t = currentTime + bounceOffset;

        if (enableBounce)
        {
            float offsetY = Mathf.Sin(t * bounceSpeed * Mathf.PI) * bounceHeight;
            rt.anchoredPosition = startPos + new Vector2(0f, offsetY);
        }

        if (enableDangle)
        {
            float angle = Mathf.Sin(t * dangleSpeed * Mathf.PI) * dangleAngle;
            rt.localEulerAngles = new Vector3(0f, 0f, angle);
        }

        if (enablePulse)
        {
            float pulse = 1f + Mathf.Sin(t * pulseSpeed * Mathf.PI) * pulseAmount;
            rt.localScale = startScale * pulse;
        }
    }

    private void OnDisable()
    {
        rt.anchoredPosition = startPos;
        rt.localEulerAngles = Vector3.zero;
        rt.localScale = startScale;
    }
}

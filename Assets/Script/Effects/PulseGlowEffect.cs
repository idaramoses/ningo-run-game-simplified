using UnityEngine;

/// <summary>
/// Animates a clean pulsing glow aura on pickups (coins, letters).
/// Rotates towards the main camera (billboard) and pulses smoothly in scale and opacity.
/// </summary>
[DisallowMultipleComponent]
public class PulseGlowEffect : MonoBehaviour
{
    [Header("Color & Intensity")]
    [Tooltip("Base tint color of the glow aura.")]
    [SerializeField] private Color glowColor = new Color(1f, 0.85f, 0.2f, 0.8f);

    [Header("Pulse Animation")]
    [Tooltip("Pulse speed in cycles per second.")]
    [SerializeField] private float pulseSpeed = 2.5f;

    [Tooltip("Scale pulse range (multiplier on base scale).")]
    [SerializeField] private float minScaleMultiplier = 0.88f;
    [SerializeField] private float maxScaleMultiplier = 1.12f;

    [Tooltip("Alpha pulse range (multiplier on base color alpha).")]
    [SerializeField] private float minAlphaMultiplier = 0.7f;
    [SerializeField] private float maxAlphaMultiplier = 1.0f;

    [Header("Billboard")]
    [Tooltip("Keep facing the active camera?")]
    [SerializeField] private bool billboardToCamera = true;

    private SpriteRenderer spriteRenderer;
    private Vector3 baseScale;
    private Color baseColor;
    private Camera cachedCamera;
    private Transform cachedTransform;

    private void Awake()
    {
        cachedTransform = transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseScale = cachedTransform.localScale;

        if (spriteRenderer != null)
        {
            if (spriteRenderer.color != Color.white && spriteRenderer.color.a > 0.01f)
            {
                glowColor = spriteRenderer.color;
            }
            baseColor = glowColor;
            spriteRenderer.color = baseColor;
        }
        else
        {
            baseColor = glowColor;
        }
    }

    private void Start()
    {
        cachedCamera = Camera.main;
    }

    private void LateUpdate()
    {
        // Smooth sine wave [0, 1]
        float wave = (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;

        // Pulse scale
        float scaleMul = Mathf.Lerp(minScaleMultiplier, maxScaleMultiplier, wave);
        cachedTransform.localScale = baseScale * scaleMul;

        // Pulse alpha if SpriteRenderer attached
        if (spriteRenderer != null)
        {
            float alphaMul = Mathf.Lerp(minAlphaMultiplier, maxAlphaMultiplier, wave);
            Color c = baseColor;
            c.a = baseColor.a * alphaMul;
            spriteRenderer.color = c;
        }

        // Billboard rotation
        if (billboardToCamera)
        {
            if (cachedCamera == null)
            {
                cachedCamera = Camera.main;
            }

            if (cachedCamera != null)
            {
                cachedTransform.rotation = cachedCamera.transform.rotation;
            }
        }
    }

    /// <summary>
    /// Runtime helper to customize glow color dynamically.
    /// </summary>
    public void SetGlowColor(Color color)
    {
        glowColor = color;
        baseColor = color;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = color;
        }
    }
}

using UnityEngine;

/// <summary>
/// Attach to a single-mesh tree (trunk + leaves baked into one object).
/// The whole tree rotates from its BASE pivot, just like a real tree bending in wind.
/// Make sure the tree's pivot/origin is at the BASE of the trunk in your model.
/// </summary>
public class TreeSway : MonoBehaviour
{
    [Header("Sway Settings")]
    [Tooltip("Max degrees the tree bends (2-4 = gentle, 5-8 = windy)")]
    [SerializeField] private float swayAmount = 1.5f;

    [Tooltip("Speed of the sway cycle")]
    [SerializeField] private float swaySpeed = 0.4f;

    [Header("Wind Variation (Perlin Noise)")]
    [Tooltip("Enable natural wind gusts instead of constant speed")]
    [SerializeField] private bool enableWindGusts = true;

    [Tooltip("How fast wind changes strength")]
    [SerializeField] private float windChangeSpeed = 0.2f;

    [Tooltip("Peak wind strength multiplier")]
    [SerializeField] private float maxWindStrength = 1.2f;

    [Header("Pivot Offset")]
    [Tooltip("If the tree pivot is NOT at the base, set this to the height of the base. Leave at 0 if pivot is already at base.")]
    [SerializeField] private float pivotYOffset = 0f;

    private Quaternion originalRotation;
    private Vector3 originalPosition;
    private float swayOffset;
    private float windOffset;

    void Start()
    {
        originalRotation = transform.localRotation;
        originalPosition = transform.localPosition;

#if UNITY_ANDROID || UNITY_IOS
        enabled = false;
        return;
#endif

// Randomize so each tree in the scene sways out of sync
swayOffset = Random.Range(0f, 100f);
windOffset  = Random.Range(0f, 100f);
    }

    void Update()
    {
        // --- Wind strength (Perlin noise = smooth organic changes) ---
        float windStrength = 1f;
        if (enableWindGusts)
        {
            windOffset  += Time.deltaTime * windChangeSpeed;
            windStrength = Mathf.PerlinNoise(windOffset, 0f) * maxWindStrength;
        }

        swayOffset += Time.deltaTime * swaySpeed;

        // --- Bend angle: side-to-side + slight front-back ---
        float bendX = Mathf.Sin(swayOffset)         * swayAmount * windStrength;
        float bendZ = Mathf.Sin(swayOffset * 0.75f) * swayAmount * 0.5f * windStrength;

        // Rotate from base — whole tree bends, top moves most, base stays planted
        Quaternion targetRot = originalRotation * Quaternion.Euler(bendX, 0f, bendZ);

        // Smooth interpolation so movement is not snappy
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRot, Time.deltaTime * 5f);

        // If pivot is not at base, compensate position so base doesn't float
        if (pivotYOffset != 0f)
        {
            Vector3 offset = transform.rotation * new Vector3(0f, -pivotYOffset, 0f);
            transform.localPosition = originalPosition + offset;
        }
    }

    void OnDisable()
    {
        transform.localRotation = originalRotation;
        transform.localPosition = originalPosition;
    }
}

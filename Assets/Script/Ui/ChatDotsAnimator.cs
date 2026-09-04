using UnityEngine;
using System.Collections.Generic;

public class ChatDotsAnimator : MonoBehaviour
{
    [Header("Target Dots")]
    [Tooltip("The dot transforms to animate. If empty, will automatically find children with 'chat-dots' in their name.")]
    [SerializeField] private Transform[] dots;

    [Header("Scale Pulse Settings")]
    [SerializeField] private bool animateScale = true;
    [SerializeField] private float minScaleMultiplier = 0.8f;
    [SerializeField] private float maxScaleMultiplier = 1.3f;

    [Header("Position Bounce Settings")]
    [SerializeField] private bool animatePosition = true;
    [SerializeField] private Vector2 bounceAmount = new Vector2(0f, 4f);

    [Header("Wave Configuration")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float phaseDelay = 0.6f;

    private Vector3[] originalScales;
    private Vector2[] originalPositions;

    private void Awake()
    {
        // Auto-find dots if not assigned
        if (dots == null || dots.Length == 0)
        {
            List<Transform> foundDots = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.name.ToLower().Contains("chat-dots"))
                {
                    foundDots.Add(child);
                }
            }
            dots = foundDots.ToArray();
        }

        // Cache original values
        if (dots != null && dots.Length > 0)
        {
            originalScales = new Vector3[dots.Length];
            originalPositions = new Vector2[dots.Length];

            for (int i = 0; i < dots.Length; i++)
            {
                if (dots[i] != null)
                {
                    originalScales[i] = dots[i].localScale;
                    RectTransform rt = dots[i].GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        originalPositions[i] = rt.anchoredPosition;
                    }
                }
            }
        }
    }

    private void Update()
    {
        if (dots == null || dots.Length == 0) return;

        float time = Time.time * speed;

        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] == null) continue;

            // Compute offset sine wave
            float sineWave = Mathf.Sin(time - (i * phaseDelay));
            float normalizedWave = (sineWave + 1f) * 0.5f; // 0 to 1

            // Animate scale
            if (animateScale && originalScales != null && i < originalScales.Length)
            {
                float scaleMultiplier = Mathf.Lerp(minScaleMultiplier, maxScaleMultiplier, normalizedWave);
                dots[i].localScale = originalScales[i] * scaleMultiplier;
            }

            // Animate position
            if (animatePosition && originalPositions != null && i < originalPositions.Length)
            {
                RectTransform rt = dots[i].GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = originalPositions[i] + (bounceAmount * normalizedWave);
                }
            }
        }
    }

    private void OnDisable()
    {
        // Reset to original values when disabled to avoid leaving elements in weird states
        if (dots == null) return;

        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] == null) continue;

            if (originalScales != null && i < originalScales.Length)
            {
                dots[i].localScale = originalScales[i];
            }

            if (originalPositions != null && i < originalPositions.Length)
            {
                RectTransform rt = dots[i].GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = originalPositions[i];
                }
            }
        }
    }
}

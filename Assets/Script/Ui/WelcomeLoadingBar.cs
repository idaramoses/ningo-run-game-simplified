using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WelcomeLoadingBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image backgroundBar;
    [SerializeField] private Image fillBar;
    [SerializeField] private TMP_Text loadingText;

    [Header("Colors")]
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.2f, 0.4f, 0.8f);
    [SerializeField] private Color fillColor = new Color(0.2f, 0.6f, 1f, 1f);
    [SerializeField] private Color fillGlowColor = new Color(0.4f, 0.8f, 1f, 1f);

    [Header("Animation")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseIntensity = 0.1f;
    [SerializeField] private bool animateDots = true;
    [SerializeField] private float dotAnimationSpeed = 0.5f;

    private float currentProgress = 0f;
    private int dotCount = 0;
    private float dotTimer = 0f;

    private void Start()
    {
        if (backgroundBar != null)
            backgroundBar.color = backgroundColor;

        if (fillBar != null)
        {
            fillBar.color = fillColor;
            fillBar.fillAmount = 0f;
        }

        if (loadingText != null)
            loadingText.text = "LOADING...";
    }

    private void Update()
    {
        AnimateFillGlow();
        AnimateLoadingDots();
    }

    private void AnimateFillGlow()
    {
        if (fillBar == null) return;

        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
        fillBar.color = Color.Lerp(fillColor, fillGlowColor, (pulse - 1f) / pulseIntensity * 0.5f + 0.5f);
    }

    private void AnimateLoadingDots()
    {
        if (!animateDots || loadingText == null) return;

        dotTimer += Time.deltaTime;
        if (dotTimer >= dotAnimationSpeed)
        {
            dotTimer = 0f;
            dotCount = (dotCount + 1) % 4;
            loadingText.text = "LOADING" + new string('.', dotCount == 0 ? 3 : dotCount);
        }
    }

    public void SetProgress(float progress)
    {
        currentProgress = Mathf.Clamp01(progress);
        if (fillBar != null)
        {
            fillBar.fillAmount = currentProgress;
        }
    }

    public float GetProgress()
    {
        return currentProgress;
    }

    public void SetLoadingText(string text)
    {
        if (loadingText != null)
        {
            loadingText.text = text;
            animateDots = false;
        }
    }

    public void ResetLoadingText()
    {
        animateDots = true;
        dotCount = 0;
        dotTimer = 0f;
    }
}

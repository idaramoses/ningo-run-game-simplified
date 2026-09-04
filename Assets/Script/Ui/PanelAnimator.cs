using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PanelAnimator : MonoBehaviour
{
    public enum AnimationType
    {
        Scale,
        Fade,
        ScaleAndFade,
        Slide,
        SlideAndFade,
        Pop,          // Punchy scale pop with overshoot bounce
        PopAndFade    // Scale pop with overshoot bounce + fade
    }
    
    public enum SlideDirection
    {
        Top,
        Bottom,
        Left,
        Right
    }

    [Header("Animation Settings")]
    [SerializeField] private AnimationType animationType = AnimationType.PopAndFade;
    [SerializeField] private float animationDuration = 0.35f;
    [SerializeField] private float startDelay = 0f;
    [SerializeField] private bool useOvershoot = true;
    [SerializeField] private float overshootAmount = 1.08f;
    
    [Header("Scale Animation")]
    [SerializeField] private Vector3 startScale = new Vector3(0.5f, 0.5f, 1f);
    [SerializeField] private Vector3 endScale = Vector3.one;
    
    [Header("Slide Animation")]
    [SerializeField] private SlideDirection slideDirection = SlideDirection.Bottom;
    [SerializeField] private float slideDistance = 400f;
    
    [Header("Components")]
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    [SerializeField] private Vector2 originalPosition;
    private bool hasOriginalPosition = false;
    private Coroutine currentAnimation;
    
    private void Awake()
    {
        InitializeComponents();
    }

    private void InitializeComponents()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
        
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        if (rectTransform != null && !hasOriginalPosition)
        {
            originalPosition = rectTransform.anchoredPosition;
            hasOriginalPosition = true;
        }
    }
    
    private void OnEnable()
    {
        InitializeComponents();
        AnimateIn();
    }

    private void OnDisable()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }
        ResetToOriginal();
    }
    
    public void AnimateIn()
    {
        InitializeComponents();
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        
        if (gameObject.activeInHierarchy)
        {
            currentAnimation = StartCoroutine(AnimateInCoroutine());
        }
    }
    
    public void AnimateOut(System.Action onComplete = null)
    {
        InitializeComponents();
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        
        if (gameObject.activeInHierarchy)
        {
            currentAnimation = StartCoroutine(AnimateOutCoroutine(onComplete));
        }
        else
        {
            onComplete?.Invoke();
        }
    }
    
    private IEnumerator AnimateInCoroutine()
    {
        // 1. Immediately apply initial hidden/start state before frame renders
        ApplyInitialState();

        // 2. Wait for optional start delay
        if (startDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(startDelay);
        }

        // Re-apply initial state right as animation begins
        ApplyInitialState();

        float elapsed = 0f;
        Vector3 slideStartPos = GetSlideStartPosition();
        
        // 3. Animate over duration using unscaled delta time
        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            
            // Calculate easing
            float easedT = EvaluateEase(t);
            float alphaT = Mathf.Clamp01(t * 1.5f); // Fade in slightly faster for snappy feel

            // Apply Scale
            switch (animationType)
            {
                case AnimationType.Scale:
                case AnimationType.ScaleAndFade:
                    if (rectTransform != null)
                        rectTransform.localScale = Vector3.LerpUnclamped(startScale, endScale, easedT);
                    break;

                case AnimationType.Pop:
                case AnimationType.PopAndFade:
                    if (rectTransform != null)
                    {
                        float popScale = EvaluatePop(t);
                        rectTransform.localScale = Vector3.LerpUnclamped(startScale, endScale, popScale);
                    }
                    break;
                    
                case AnimationType.Slide:
                case AnimationType.SlideAndFade:
                    if (rectTransform != null)
                        rectTransform.anchoredPosition = Vector2.LerpUnclamped(slideStartPos, originalPosition, easedT);
                    break;
            }
            
            // Apply Alpha
            switch (animationType)
            {
                case AnimationType.Fade:
                case AnimationType.ScaleAndFade:
                case AnimationType.SlideAndFade:
                case AnimationType.PopAndFade:
                    if (canvasGroup != null)
                        canvasGroup.alpha = Mathf.Lerp(0f, 1f, alphaT);
                    break;
                default:
                    if (canvasGroup != null)
                        canvasGroup.alpha = 1f;
                    break;
            }
            
            yield return null;
        }
        
        // 4. Ensure exact final state
        ResetToOriginal();
        currentAnimation = null;
    }

    private void ApplyInitialState()
    {
        switch (animationType)
        {
            case AnimationType.Scale:
            case AnimationType.ScaleAndFade:
            case AnimationType.Pop:
            case AnimationType.PopAndFade:
                if (rectTransform != null)
                    rectTransform.localScale = startScale;
                break;
                
            case AnimationType.Slide:
            case AnimationType.SlideAndFade:
                if (rectTransform != null)
                    rectTransform.anchoredPosition = GetSlideStartPosition();
                break;
        }

        switch (animationType)
        {
            case AnimationType.Fade:
            case AnimationType.ScaleAndFade:
            case AnimationType.SlideAndFade:
            case AnimationType.PopAndFade:
                if (canvasGroup != null)
                    canvasGroup.alpha = 0f;
                break;
            default:
                if (canvasGroup != null)
                    canvasGroup.alpha = 1f;
                break;
        }
    }
    
    private IEnumerator AnimateOutCoroutine(System.Action onComplete)
    {
        float elapsed = 0f;
        Vector3 slideStartPos = GetSlideStartPosition();
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            float easeOutT = t * t; // Quadratic ease in for out-transition
            
            switch (animationType)
            {
                case AnimationType.Scale:
                case AnimationType.ScaleAndFade:
                case AnimationType.Pop:
                case AnimationType.PopAndFade:
                    if (rectTransform != null)
                        rectTransform.localScale = Vector3.Lerp(endScale, startScale, easeOutT);
                    break;
                    
                case AnimationType.Slide:
                case AnimationType.SlideAndFade:
                    if (rectTransform != null)
                        rectTransform.anchoredPosition = Vector2.Lerp(originalPosition, slideStartPos, easeOutT);
                    break;
            }
            
            switch (animationType)
            {
                case AnimationType.Fade:
                case AnimationType.ScaleAndFade:
                case AnimationType.SlideAndFade:
                case AnimationType.PopAndFade:
                    if (canvasGroup != null)
                        canvasGroup.alpha = Mathf.Lerp(1f, 0f, easeOutT);
                    break;
            }
            
            yield return null;
        }
        
        ResetToOriginal();
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        
        currentAnimation = null;
        onComplete?.Invoke();
    }

    private float EvaluateEase(float t)
    {
        if (useOvershoot)
        {
            // Overshoot curve: quick rise past 1.0 and settle back to 1.0
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
        // Smooth cubic ease out
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    private float EvaluatePop(float t)
    {
        // Smooth punchy elastic pop
        if (t < 0.65f)
        {
            float subT = t / 0.65f;
            // Ease out to overshoot
            float ease = 1f - Mathf.Pow(1f - subT, 3f);
            return Mathf.Lerp(0f, overshootAmount, ease);
        }
        else
        {
            float subT = (t - 0.65f) / 0.35f;
            // Settle from overshoot back to 1.0
            float ease = Mathf.SmoothStep(0f, 1f, subT);
            return Mathf.Lerp(overshootAmount, 1f, ease);
        }
    }

    private void ResetToOriginal()
    {
        if (rectTransform != null)
        {
            rectTransform.localScale = endScale;
            if (hasOriginalPosition)
                rectTransform.anchoredPosition = originalPosition;
        }
        
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }
    
    private Vector2 GetSlideStartPosition()
    {
        Vector2 offset = Vector2.zero;
        
        switch (slideDirection)
        {
            case SlideDirection.Top:
                offset = new Vector2(0, slideDistance);
                break;
            case SlideDirection.Bottom:
                offset = new Vector2(0, -slideDistance);
                break;
            case SlideDirection.Left:
                offset = new Vector2(-slideDistance, 0);
                break;
            case SlideDirection.Right:
                offset = new Vector2(slideDistance, 0);
                break;
        }
        
        return originalPosition + offset;
    }
    
    public void SetAnimationType(AnimationType type)
    {
        animationType = type;
    }
    
    public void SetAnimationDuration(float duration)
    {
        animationDuration = duration;
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PoliceThreatUI : MonoBehaviour
{
    private static PoliceThreatUI _instance;
    public static PoliceThreatUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Object.FindAnyObjectByType<PoliceThreatUI>(FindObjectsInactive.Include);
            }
            return _instance;
        }
    }

    [Header("UI Elements")]
    public GameObject container;
    public Image countdownBar;
    public Image countdownFillGlow;
    public TMP_Text warningTitleText;
    public TMP_Text countdownTimerText;
    public Image leftArrow;
    public Image rightArrow;
    public Image screenEdgeVignette;

    [Header("Colors")]
    public Color normalRedColor = new Color(1f, 0.15f, 0.15f, 1f);
    public Color normalBlueColor = new Color(0.15f, 0.5f, 1f, 1f);
    public Color successGreenColor = new Color(0.2f, 0.95f, 0.3f, 1f);
    public Color urgentColor = new Color(1f, 0f, 0f, 1f);

    private float pulseTimer = 0f;
    private int currentDodgeIdx = 1;
    private int totalDodgesCount = 3;
    private bool isRetargeting = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        if (container != null)
            container.SetActive(false);
        if (screenEdgeVignette != null)
            screenEdgeVignette.gameObject.SetActive(false);
    }

    public void ShowThreat(float duration, int currentLane, int dodgeIndex, int totalDodges)
    {
        gameObject.SetActive(true);
        isRetargeting = false;
        currentDodgeIdx = dodgeIndex;
        totalDodgesCount = totalDodges;

        if (container != null) container.SetActive(true);
        if (screenEdgeVignette != null) screenEdgeVignette.gameObject.SetActive(true);

        if (warningTitleText != null)
        {
            warningTitleText.text = $"DODGE MONKEY! ({currentDodgeIdx}/{totalDodgesCount})";
        }

        UpdateArrows(currentLane);
        UpdateProgress(1f, duration);
    }

    public void ShowDodgeSuccess(int completed, int total)
    {
        isRetargeting = true;
        if (warningTitleText != null)
        {
            warningTitleText.text = $"DODGED ({completed}/{total})! NEXT LANE!";
            warningTitleText.color = successGreenColor;
        }

        if (countdownTimerText != null)
        {
            countdownTimerText.text = "GOOD!";
            countdownTimerText.color = successGreenColor;
        }

        if (countdownBar != null)
        {
            countdownBar.fillAmount = 1f;
            countdownBar.color = successGreenColor;
        }
    }

    public void ShowEvaded()
    {
        isRetargeting = true;
        if (warningTitleText != null)
        {
            warningTitleText.text = "MONKEY OUTRUN! LETTERS SAFE!";
            warningTitleText.color = successGreenColor;
        }

        if (countdownTimerText != null)
        {
            countdownTimerText.text = "SAFE";
            countdownTimerText.color = successGreenColor;
        }

        if (countdownBar != null)
        {
            countdownBar.fillAmount = 1f;
            countdownBar.color = successGreenColor;
        }

        if (leftArrow != null) leftArrow.gameObject.SetActive(false);
        if (rightArrow != null) rightArrow.gameObject.SetActive(false);

        Invoke(nameof(HideThreat), 1.6f);
    }

    public void UpdateProgress(float normalizedTime, float remainingSeconds)
    {
        if (isRetargeting) return;

        if (countdownBar != null)
        {
            countdownBar.fillAmount = Mathf.Clamp01(normalizedTime);
            countdownBar.color = (normalizedTime < 0.35f) ? urgentColor : normalRedColor;
        }

        if (countdownTimerText != null)
        {
            countdownTimerText.text = $"{Mathf.Max(0f, remainingSeconds):0.0}s";
            countdownTimerText.color = (normalizedTime < 0.35f) ? urgentColor : Color.white;
        }

        pulseTimer += Time.deltaTime * 8f;
        float alpha = 0.4f + Mathf.PingPong(pulseTimer, 0.4f);

        if (warningTitleText != null)
        {
            warningTitleText.text = $"CHANGE LANE! ({currentDodgeIdx}/{totalDodgesCount})";
            warningTitleText.color = ((int)(pulseTimer * 2f) % 2 == 0) ? normalRedColor : normalBlueColor;
        }

        if (screenEdgeVignette != null)
        {
            Color vigColor = ((int)(pulseTimer * 2f) % 2 == 0) ? normalRedColor : normalBlueColor;
            vigColor.a = (normalizedTime < 0.35f) ? alpha * 1.5f : alpha * 0.7f;
            screenEdgeVignette.color = vigColor;
        }
    }

    public void UpdateArrows(int currentLane)
    {
        // 0 = Left (can only swipe right)
        // 1 = Center (can swipe left or right)
        // 2 = Right (can only swipe left)
        if (leftArrow != null)
        {
            leftArrow.gameObject.SetActive(currentLane > 0);
        }
        if (rightArrow != null)
        {
            rightArrow.gameObject.SetActive(currentLane < 2);
        }
    }

    public void HideThreat()
    {
        CancelInvoke(nameof(HideThreat));
        isRetargeting = false;
        if (container != null) container.SetActive(false);
        if (screenEdgeVignette != null) screenEdgeVignette.gameObject.SetActive(false);
    }
}


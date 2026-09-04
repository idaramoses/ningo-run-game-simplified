using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MissionCardItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Image progressFillImage; // Image with fill type (like splash screen)
    [SerializeField] private Image progressBackgroundImage; // Optional: background bar behind fill
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private Button claimButton;
    [SerializeField] private GameObject completedCheckmark;
    [SerializeField] private Image missionIcon;

    private string missionId;
    private System.Action<string> onClaimCallback;

    public void Setup(string id, string title, int current, int target, int reward, Sprite icon = null, System.Action<string> onClaim = null)
    {
        missionId = id;
        onClaimCallback = onClaim;

        if (titleText != null)
            titleText.text = title;

        float progress = Mathf.Clamp01((float)current / target);
        
        if (progressFillImage != null)
            progressFillImage.fillAmount = progress;

        if (progressText != null)
            progressText.text = $"{current}/{target}";

        if (rewardText != null)
            rewardText.text = $"x{reward}";

        if (missionIcon != null && icon != null)
            missionIcon.sprite = icon;

        bool isComplete = current >= target;
        
        if (claimButton != null)
        {
            claimButton.interactable = isComplete;
            claimButton.onClick.RemoveAllListeners();
            if (isComplete && onClaim != null)
                claimButton.onClick.AddListener(() => onClaim?.Invoke(missionId));
        }

        if (completedCheckmark != null)
            completedCheckmark.SetActive(isComplete);
    }

    public void UpdateProgress(int current, int target)
    {
        float progress = Mathf.Clamp01((float)current / target);
        
        if (progressFillImage != null)
            progressFillImage.fillAmount = progress;

        if (progressText != null)
            progressText.text = $"{current}/{target}";

        bool isComplete = current >= target;
        
        if (claimButton != null)
            claimButton.interactable = isComplete;

        if (completedCheckmark != null)
            completedCheckmark.SetActive(isComplete);
    }
}

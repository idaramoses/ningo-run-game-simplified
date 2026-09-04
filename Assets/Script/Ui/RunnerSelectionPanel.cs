using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RunnerSelectionPanel : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panelRoot;
    public CanvasGroup canvasGroup;
    public Button closeButton;
    public Button previousButton;
    public Button nextButton;
    public Button unlockButton;
    public Button selectButton;

    [Header("Runner Display")]
    public TMP_Text runnerNameText;
    public TMP_Text runnerRoleText;
    public TMP_Text runnerDescriptionText;
    public RawImage runnerPreviewRawImage;

    [Header("Stats Display")]
    public Image[] speedStars;
    public Image[] controlStars;
    public Image[] focusStars;
    public Sprite starFilled;
    public Sprite starEmpty;

    [Header("Unlock Display")]
    public TMP_Text unlockCostText;
    public Image unlockCostIcon;
    public Sprite coinIcon;
    public Sprite gemIcon;
    public GameObject lockedOverlay;

    [Header("Pagination")]
    public Image[] paginationDots;
    public Color dotActive;
    public Color dotInactive;

    private int currentIndex = 0;

    private void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(() => gameObject.SetActive(false));

        if (previousButton != null)
            previousButton.onClick.AddListener(ShowPreviousRunner);

        if (nextButton != null)
            nextButton.onClick.AddListener(ShowNextRunner);

        if (unlockButton != null)
            unlockButton.onClick.AddListener(UnlockCurrentRunner);

        if (selectButton != null)
            selectButton.onClick.AddListener(SelectCurrentRunner);
    }

    private void OnEnable()
    {
        if (RunnerSelectionManager.Instance != null)
        {
            currentIndex = RunnerSelectionManager.Instance.GetCurrentRunnerIndex();
            StartCoroutine(UpdateDisplayDelayed());
        }
    }

    private System.Collections.IEnumerator UpdateDisplayDelayed()
    {
        yield return null; // Wait one frame for failPreview to be ready
        UpdateDisplay();
    }

    private void ShowPreviousRunner()
    {
        if (RunnerSelectionManager.Instance == null) return;

        List<RunnerData> runners = RunnerSelectionManager.Instance.GetAllRunners();
        if (runners.Count == 0) return;

        currentIndex--;
        if (currentIndex < 0)
            currentIndex = runners.Count - 1;

        UpdateDisplay();
    }

    private void ShowNextRunner()
    {
        if (RunnerSelectionManager.Instance == null) return;

        List<RunnerData> runners = RunnerSelectionManager.Instance.GetAllRunners();
        if (runners.Count == 0) return;

        currentIndex++;
        if (currentIndex >= runners.Count)
            currentIndex = 0;

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (RunnerSelectionManager.Instance == null) return;

        List<RunnerData> runners = RunnerSelectionManager.Instance.GetAllRunners();
        if (runners.Count == 0 || currentIndex >= runners.Count) return;

        RunnerData runner = runners[currentIndex];
        bool isUnlocked = RunnerSelectionManager.Instance.IsRunnerUnlocked(currentIndex);
        bool isSelected = RunnerSelectionManager.Instance.GetCurrentRunnerIndex() == currentIndex;
        bool canAfford = RunnerSelectionManager.Instance.CanAffordRunner(currentIndex);

        if (runnerNameText != null)
            runnerNameText.text = runner.runnerName;

        if (runnerRoleText != null)
            runnerRoleText.text = runner.role;

        if (runnerDescriptionText != null)
            runnerDescriptionText.text = runner.description;

        UpdateStats(runner);
        UpdateUnlockDisplay(runner, isUnlocked, canAfford);
        UpdateButtons(isUnlocked, isSelected);
        UpdatePagination();
        UpdatePreviewRunner(runner);

        Debug.Log($"[RunnerSelectionPanel] Displaying runner: {runner.runnerName}, Unlocked: {isUnlocked}, Selected: {isSelected}");
    }

    private void UpdatePreviewRunner(RunnerData runner)
    {
        if (runner == null)
        {
            Debug.LogWarning("[RunnerSelectionPanel] UpdatePreviewRunner: runner is null");
            return;
        }
        
        if (runner.runnerPrefab == null)
        {
            Debug.LogWarning($"[RunnerSelectionPanel] UpdatePreviewRunner: runnerPrefab is NULL for runner '{runner.runnerName}' - please assign it in the RunnerData ScriptableObject!");
            return;
        }

        // Get the failPreview from GameFlowController
        if (GameFlowController.Instance != null && GameFlowController.Instance.failPreview != null)
        {
            GameFlowController.Instance.failPreview.ChangeRunner(runner.runnerPrefab, runner.animatorController);
        }
        else
        {
            Debug.LogWarning("[RunnerSelectionPanel] UpdatePreviewRunner: GameFlowController.Instance or failPreview is null");
        }
    }

    private void UpdateStats(RunnerData runner)
    {
        UpdateStarDisplay(speedStars, runner.speed);
        UpdateStarDisplay(controlStars, runner.control);
        UpdateStarDisplay(focusStars, runner.focus);
    }

    private void UpdateStarDisplay(Image[] stars, int value)
    {
        if (stars == null) return;

        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] != null)
            {
                stars[i].sprite = i < value ? starFilled : starEmpty;
            }
        }
    }

    private void UpdateUnlockDisplay(RunnerData runner, bool isUnlocked, bool canAfford)
    {
        if (lockedOverlay != null)
            lockedOverlay.SetActive(!isUnlocked);

        if (isUnlocked)
        {
            if (unlockCostText != null)
                unlockCostText.text = "UNLOCKED";
            return;
        }

        if (unlockCostText != null)
        {
            switch (runner.unlockType)
            {
                case UnlockType.Free:
                    unlockCostText.text = "FREE";
                    break;
                case UnlockType.Coins:
                    unlockCostText.text = runner.unlockCost.ToString();
                    if (unlockCostIcon != null && coinIcon != null)
                        unlockCostIcon.sprite = coinIcon;
                    break;
                case UnlockType.Gems:
                    unlockCostText.text = runner.unlockCost.ToString();
                    if (unlockCostIcon != null && gemIcon != null)
                        unlockCostIcon.sprite = gemIcon;
                    break;
                case UnlockType.Level:
                    unlockCostText.text = "FREE";
                    break;
                case UnlockType.RewardedAd:
                    unlockCostText.text = "FREE";
                    break;
                case UnlockType.Special:
                    unlockCostText.text = "SPECIAL EVENT";
                    break;
            }
        }
    }

    private void UpdateButtons(bool isUnlocked, bool isSelected)
    {
        if (unlockButton != null)
        {
            unlockButton.gameObject.SetActive(!isUnlocked);
            unlockButton.interactable = RunnerSelectionManager.Instance.CanAffordRunner(currentIndex);
        }

        if (selectButton != null)
        {
            selectButton.gameObject.SetActive(isUnlocked);
            selectButton.interactable = !isSelected;
            
            TMP_Text buttonText = selectButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
                buttonText.text = isSelected ? "SELECTED" : "SELECT";
        }
    }

    private void UpdatePagination()
    {
        if (paginationDots == null || RunnerSelectionManager.Instance == null) return;

        List<RunnerData> runners = RunnerSelectionManager.Instance.GetAllRunners();

        for (int i = 0; i < paginationDots.Length; i++)
        {
            if (paginationDots[i] != null)
            {
                if (i < runners.Count)
                {
                    paginationDots[i].gameObject.SetActive(true);
                    paginationDots[i].color = i == currentIndex ? dotActive : dotInactive;
                }
                else
                {
                    paginationDots[i].gameObject.SetActive(false);
                }
            }
        }
    }

    private void UnlockCurrentRunner()
    {
        if (RunnerSelectionManager.Instance == null) return;

        bool unlocked = RunnerSelectionManager.Instance.UnlockRunner(currentIndex);
        if (unlocked)
        {
            UpdateDisplay();
            Debug.Log("[RunnerSelectionPanel] Runner unlocked successfully");
        }
        else
        {
            Debug.Log("[RunnerSelectionPanel] Failed to unlock runner");
        }
    }

    private void SelectCurrentRunner()
    {
        if (RunnerSelectionManager.Instance == null) return;

        RunnerSelectionManager.Instance.SelectRunner(currentIndex);

        // Always update the preview and display so the Home scene reflects the change
        UpdateDisplay();

        List<RunnerData> runners = RunnerSelectionManager.Instance.GetAllRunners();
        if (currentIndex >= 0 && currentIndex < runners.Count)
        {
            UpdatePreviewRunner(runners[currentIndex]);
        }

        // Close panel and hide preview in Home scene
        if (GameFlowController.Instance != null)
        {
            gameObject.SetActive(false);
            GameFlowController.Instance.HideRunnerSelection();
        }
        
        Debug.Log($"[RunnerSelectionPanel] Runner selected: index {currentIndex}");
    }
}

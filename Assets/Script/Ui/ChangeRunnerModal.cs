using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeRunnerModal : MonoBehaviour
{
    [Header("Modal UI")]
    [SerializeField] private GameObject modalPanel;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button cancelButton;

    [Header("Runner Buttons")]
    [SerializeField] private Button amakaButton;
    [SerializeField] private Button musaButton;
    [SerializeField] private Button tundeButton;

    [Header("Runner Images (Optional)")]
    [SerializeField] private Image amakaImage;
    [SerializeField] private Image musaImage;
    [SerializeField] private Image tundeImage;

    [Header("References")]
    [SerializeField] private RunnerManager runnerManager;

    private string selectedRunner = "Amaka";

    private void Start()
    {
        // Load saved runner from RunnerSelectionManager
        if (RunnerSelectionManager.Instance != null)
        {
            var currentRunner = RunnerSelectionManager.Instance.GetCurrentRunner();
            if (currentRunner != null)
            {
                selectedRunner = currentRunner.runnerName;
            }
        }

        // Setup button listeners
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseModal);
        
        if (cancelButton != null)
            cancelButton.onClick.AddListener(CloseModal);

        if (amakaButton != null)
            amakaButton.onClick.AddListener(() => SelectRunner("Amaka", 0));
        
        if (musaButton != null)
            musaButton.onClick.AddListener(() => SelectRunner("Musa", 1));
        
        if (tundeButton != null)
            tundeButton.onClick.AddListener(() => SelectRunner("Tunde", 2));

        // Hide modal initially
        if (modalPanel != null)
            modalPanel.SetActive(false);

        UpdateButtonStates();
    }

    public void OpenModal()
    {
        if (modalPanel != null)
        {
            modalPanel.SetActive(true);
            UpdateButtonStates();
            Debug.Log($"[ChangeRunnerModal] Opened modal. Current runner: {selectedRunner}");
        }
    }

    public void CloseModal()
    {
        if (modalPanel != null)
        {
            modalPanel.SetActive(false);
            Debug.Log("[ChangeRunnerModal] Closed modal");
        }
    }

    private void SelectRunner(string runnerName, int runnerIndex)
    {
        selectedRunner = runnerName;
        
        // Use RunnerSelectionManager to save selection (saves as index)
        if (RunnerSelectionManager.Instance != null)
        {
            RunnerSelectionManager.Instance.SelectRunner(runnerIndex);
        }

        Debug.Log($"[ChangeRunnerModal] Selected runner: {runnerName} (index: {runnerIndex})");

        // Update RunnerManager to use this runner
        if (runnerManager != null)
        {
            runnerManager.SetActiveRunner(runnerName);
        }

        UpdateHomePreview(runnerIndex);

        UpdateButtonStates();
        CloseModal();
    }

    private void UpdateHomePreview(int runnerIndex)
    {
        var runnerList = RunnerSelectionManager.Instance != null
            ? RunnerSelectionManager.Instance.GetAllRunners()
            : null;

        if (runnerList == null || runnerIndex < 0 || runnerIndex >= runnerList.Count)
            return;

        RunnerData runnerData = runnerList[runnerIndex];
        if (runnerData == null || runnerData.runnerPrefab == null)
            return;

        if (GameFlowController.Instance != null && GameFlowController.Instance.failPreview != null)
        {
            GameFlowController.Instance.failPreview.ChangeRunner(runnerData.runnerPrefab, runnerData.animatorController);
        }
    }

    private void UpdateButtonStates()
    {
        // Highlight selected runner button
        UpdateButtonVisual(amakaButton, "Amaka");
        UpdateButtonVisual(musaButton, "Musa");
        UpdateButtonVisual(tundeButton, "Tunde");
    }

    private void UpdateButtonVisual(Button button, string runnerName)
    {
        if (button == null) return;

        // Change button appearance based on selection
        ColorBlock colors = button.colors;
        
        if (runnerName == selectedRunner)
        {
            // Selected state - brighter/highlighted
            colors.normalColor = new Color(0.3f, 0.8f, 0.3f); // Green tint
        }
        else
        {
            // Normal state
            colors.normalColor = Color.white;
        }
        
        button.colors = colors;
    }

    public string GetSelectedRunner()
    {
        return selectedRunner;
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LevelSelectionPanel : MonoBehaviour
{
    private const int FIRST_SELECTABLE_LEVEL = 1; // Tutorial (Level 0) handled separately
    public static LevelSelectionPanel Instance { get; private set; }

    [Header("UI References")]
    public GameObject panelRoot;
    public Button closeButton;

    [Header("Pagination")]
    public Button previousPageButton;
    public Button nextPageButton;
    public TMP_Text pageNumberText;

    [Header("Level Grid")]
    public Transform levelGridContainer;
    public GameObject levelCardPrefab;

    [Header("Settings")]
    public int levelsPerPage = 12;

    private List<LevelCard> levelCards = new List<LevelCard>();
    private int currentPage = 0;
    private int selectedLevelNumber = 1;
    private LevelCard selectedCard = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private bool isInitialized = false;

    private void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        if (previousPageButton != null)
            previousPageButton.onClick.AddListener(PreviousPage);

        if (nextPageButton != null)
            nextPageButton.onClick.AddListener(NextPage);

        // Pre-generate cards in background
        StartCoroutine(PreGenerateCardsInBackground());
    }

    private System.Collections.IEnumerator PreGenerateCardsInBackground()
    {
        // Wait for LevelManager to initialize
        yield return new WaitForSeconds(0.1f);
        
        // Temporarily activate to generate cards
        bool wasActive = this.gameObject.activeSelf;
        this.gameObject.SetActive(true);
        
        yield return null; // Wait one frame for layout
        
        // Generate initial page of cards
        if (LevelManager.Instance != null)
        {
            selectedLevelNumber = Mathf.Max(LevelManager.Instance.GetSelectedLevel(), FIRST_SELECTABLE_LEVEL);
            currentPage = Mathf.Max(0, (selectedLevelNumber - FIRST_SELECTABLE_LEVEL) / levelsPerPage);
        }
        
        RefreshLevelGrid();
        isInitialized = true;
        
        Debug.Log("[LevelSelectionPanel] Cards pre-generated in background");
        
        // Only deactivate if it wasn't active before (don't interfere if user opened panel)
        if (!wasActive)
        {
            this.gameObject.SetActive(false);
        }
    }

    public void OpenPanel()
    {
        Debug.Log($"[LevelSelectionPanel] OpenPanel called - isInitialized: {isInitialized}");

        if (LevelManager.Instance != null)
        {
            selectedLevelNumber = Mathf.Max(LevelManager.Instance.GetSelectedLevel(), FIRST_SELECTABLE_LEVEL);
            currentPage = Mathf.Max(0, (selectedLevelNumber - FIRST_SELECTABLE_LEVEL) / levelsPerPage);
        }

        // Refresh to show correct page (cards already pre-generated)
        RefreshLevelGrid();

        Debug.Log($"[LevelSelectionPanel] Opened panel - Page {currentPage + 1}");
    }

    public void ClosePanel()
    {
        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.HideLevelSelection();
        }
        else
        {
            Debug.Log("[LevelSelectionPanel] Closed panel");
        }
    }

    private void RefreshLevelGrid()
    {
        ClearLevelCards();

        int maxLevel = GetMaxLevel();
        if (maxLevel < FIRST_SELECTABLE_LEVEL)
        {
            Debug.LogWarning("[LevelSelectionPanel] No selectable levels available.");
            return;
        }

        int startLevel = FIRST_SELECTABLE_LEVEL + currentPage * levelsPerPage;
        if (startLevel > maxLevel)
        {
            // Clamp to last valid page
            int totalSelectableLevels = maxLevel - FIRST_SELECTABLE_LEVEL + 1;
            int maxPage = Mathf.Max(0, (totalSelectableLevels - 1) / levelsPerPage);
            currentPage = maxPage;
            startLevel = FIRST_SELECTABLE_LEVEL + currentPage * levelsPerPage;
        }

        int endLevel = Mathf.Min(startLevel + levelsPerPage - 1, maxLevel);

        for (int i = startLevel; i <= endLevel; i++)
        {
            CreateLevelCard(i);
        }

        // Force layout rebuild to ensure Grid Layout Group updates
        if (levelGridContainer != null)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(levelGridContainer.GetComponent<RectTransform>());
        }

        UpdatePaginationUI();
        UpdateSelectedCard();
    }

    private void CreateLevelCard(int levelNumber)
    {
        if (levelCardPrefab == null || levelGridContainer == null)
        {
            Debug.LogError("[LevelSelectionPanel] Level card prefab or grid container not assigned!");
            return;
        }

        GameObject cardObj = Instantiate(levelCardPrefab, levelGridContainer);
        LevelCard card = cardObj.GetComponent<LevelCard>();

        if (card != null && LevelManager.Instance != null)
        {
            // CRITICAL FIX: Force proper scale and size to ensure card is visible
            RectTransform rectTransform = cardObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // Force scale to 1,1,1 if it's zero
                if (rectTransform.localScale == Vector3.zero)
                {
                    rectTransform.localScale = Vector3.one;
                    Debug.LogWarning($"[LevelSelectionPanel] Card {levelNumber} had zero scale! Fixed to (1,1,1)");
                }
                
                // Force size if it's zero (GridLayoutGroup should handle this, but forcing it as backup)
                if (rectTransform.sizeDelta == Vector2.zero)
                {
                    rectTransform.sizeDelta = new Vector2(200f, 200f);
                    Debug.LogWarning($"[LevelSelectionPanel] Card {levelNumber} had zero size! Fixed to (200,200)");
                }
            }

            LevelData levelData = LevelManager.Instance.GetLevel(levelNumber);
            card.Initialize(levelData);
            levelCards.Add(card);

            // Debug: Check card visibility
            Image cardImage = card.cardImage;
            
            Debug.Log($"[LevelSelectionPanel] Created Level {levelNumber} card - Active: {cardObj.activeSelf}, " +
                      $"Size: {rectTransform.sizeDelta}, Scale: {rectTransform.localScale}, " +
                      $"Image: {(cardImage != null ? cardImage.sprite?.name ?? "NULL SPRITE" : "NULL IMAGE")}");
        }
    }

    private void ClearLevelCards()
    {
        foreach (var card in levelCards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }

        levelCards.Clear();
    }

    public void OnLevelSelected(int levelNumber)
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogError("[LevelSelectionPanel] LevelManager not found!");
            return;
        }

        LevelData selectedLevel = LevelManager.Instance.GetLevel(levelNumber);

        if (!selectedLevel.isUnlocked)
        {
            Debug.LogWarning($"[LevelSelectionPanel] Level {levelNumber} is locked!");
            return;
        }

        selectedLevelNumber = levelNumber;
        LevelManager.Instance.SetSelectedLevel(levelNumber);
        UpdateSelectedCard();

        Debug.Log($"[LevelSelectionPanel] Level {levelNumber} selected - Auto-starting game");

        StartLevel();
    }

    private void StartLevel()
    {
        ClosePanel();

        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.StartLevelWithCountdown();
        }
        else
        {
            // Fallback without countdown
            if (GameFlowController.Instance != null)
            {
                GameFlowController.Instance.Play();
            }
        }

        if (LevelObjectiveTracker.Instance != null)
        {
            LevelObjectiveTracker.Instance.StartTracking();
        }

        LevelData selectedLevel = LevelManager.Instance.GetLevel(selectedLevelNumber);
        Debug.Log($"[LevelSelectionPanel] Starting Level {selectedLevelNumber} - Words: {selectedLevel.GetWordCount()}");
    }

    private void UpdateSelectedCard()
    {
        if (selectedCard != null)
        {
            selectedCard.SetSelected(false);
        }

        foreach (var card in levelCards)
        {
            if (card.GetLevelNumber() == selectedLevelNumber)
            {
                card.SetSelected(true);
                selectedCard = card;
                break;
            }
        }
    }

    private void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            RefreshLevelGrid();
        }
    }

    private void NextPage()
    {
        int maxLevel = GetMaxLevel();
        int totalSelectableLevels = Mathf.Max(0, maxLevel - FIRST_SELECTABLE_LEVEL + 1);
        int maxPage = totalSelectableLevels > 0 ? (totalSelectableLevels - 1) / levelsPerPage : 0;
        
        if (currentPage < maxPage)
        {
            currentPage++;
            RefreshLevelGrid();
        }
    }

    private int GetMaxLevel()
    {
        if (LevelManager.Instance != null)
            return LevelManager.Instance.GetMaxLevelCount();
        return 50;
    }

    private void UpdatePaginationUI()
    {
        int maxLevel = GetMaxLevel();
        int totalSelectableLevels = Mathf.Max(0, maxLevel - FIRST_SELECTABLE_LEVEL + 1);
        int maxPage = totalSelectableLevels > 0 ? (totalSelectableLevels - 1) / levelsPerPage : 0;

        if (pageNumberText != null)
        {
            pageNumberText.text = $"{currentPage + 1} / {maxPage + 1}";
        }

        if (previousPageButton != null)
        {
            previousPageButton.interactable = currentPage > 0;
        }

        if (nextPageButton != null)
        {
            nextPageButton.interactable = currentPage < maxPage;
        }
    }

    public int GetSelectedLevelNumber()
    {
        return selectedLevelNumber;
    }
}

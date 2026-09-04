using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopController : MonoBehaviour
{
    [Header("Shop Canvas")]
    [SerializeField] private GameObject canvasShop;      // Main Canvas_Shop

    [Header("Header Image (changes per tab)")]
    [SerializeField] private Image headerImage;          // The header image component

    [Header("Header Sprites (one for each tab)")]
    [SerializeField] private Sprite featuredHeaderSprite;
    [SerializeField] private Sprite coinsHeaderSprite;
    [SerializeField] private Sprite powerUpsHeaderSprite;
    [SerializeField] private Sprite boardsHeaderSprite;

    [Header("Tab Content Panels")]
    [SerializeField] private GameObject featuredPanel;   // Featured tab content
    [SerializeField] private GameObject coinsPanel;      // Coins tab content
    [SerializeField] private GameObject powerUpsPanel;   // PowerUps tab content
    [SerializeField] private GameObject boardsPanel;     // Boards tab content

    [Header("Tab Buttons")]
    [SerializeField] private Button featuredTabButton;
    [SerializeField] private Button coinsTabButton;
    [SerializeField] private Button powerUpsTabButton;
    [SerializeField] private Button boardsTabButton;

    [Header("Tab Button Images - Active")]
    [SerializeField] private Sprite featuredTabActive;
    [SerializeField] private Sprite coinsTabActive;
    [SerializeField] private Sprite powerUpsTabActive;
    [SerializeField] private Sprite boardsTabActive;

    [Header("Tab Button Images - Inactive")]
    [SerializeField] private Sprite featuredTabInactive;
    [SerializeField] private Sprite coinsTabInactive;
    [SerializeField] private Sprite powerUpsTabInactive;
    [SerializeField] private Sprite boardsTabInactive;

    // NOTE: Wire BackButton OnClick() → OnBackPressed() in Inspector

    private GameObject[] allTabPanels;

    private void Start()
    {
        SetupTabButtons();
        // NOTE: Wire BackButton OnClick() → OnBackPressed() in Inspector
        
        // Show featured by default
        ShowTab(0);
    }

    private void InitializeTabPanels()
    {
        if (allTabPanels == null)
        {
            allTabPanels = new GameObject[] { featuredPanel, coinsPanel, powerUpsPanel, boardsPanel };
        }
    }

    private void SetupTabButtons()
    {
        if (featuredTabButton != null) featuredTabButton.onClick.AddListener(() => ShowTab(0));
        if (coinsTabButton != null) coinsTabButton.onClick.AddListener(() => ShowTab(1));
        if (powerUpsTabButton != null) powerUpsTabButton.onClick.AddListener(() => ShowTab(2));
        if (boardsTabButton != null) boardsTabButton.onClick.AddListener(() => ShowTab(3));
    }

    public void ShowTab(int tabIndex)
    {
        InitializeTabPanels();

        // Hide all panels
        foreach (var panel in allTabPanels)
        {
            if (panel != null) panel.SetActive(false);
        }

        // Show selected panel
        switch (tabIndex)
        {
            case 0: if (featuredPanel != null) featuredPanel.SetActive(true); break;
            case 1: if (coinsPanel != null) coinsPanel.SetActive(true); break;
            case 2: if (powerUpsPanel != null) powerUpsPanel.SetActive(true); break;
            case 3: if (boardsPanel != null) boardsPanel.SetActive(true); break;
        }

        // Update tab button visuals
        UpdateTabButtonVisuals(tabIndex);

        // Update header image
        UpdateHeaderImage(tabIndex);

        Debug.Log($"[Shop] Switched to tab index: {tabIndex}");
    }

    private void UpdateTabButtonVisuals(int activeTab)
    {
        // Featured tab
        SetTabButtonImage(featuredTabButton, 0 == activeTab ? featuredTabActive : featuredTabInactive);
        // Coins tab
        SetTabButtonImage(coinsTabButton, 1 == activeTab ? coinsTabActive : coinsTabInactive);
        // PowerUps tab
        SetTabButtonImage(powerUpsTabButton, 2 == activeTab ? powerUpsTabActive : powerUpsTabInactive);
        // Boards tab
        SetTabButtonImage(boardsTabButton, 3 == activeTab ? boardsTabActive : boardsTabInactive);
    }

    private void SetTabButtonImage(Button button, Sprite sprite)
    {
        if (button == null || sprite == null) return;
        var image = button.GetComponent<Image>();
        if (image != null) image.sprite = sprite;
    }

    private void UpdateHeaderImage(int activeTab)
    {
        if (headerImage == null) return;

        switch (activeTab)
        {
            case 0: headerImage.sprite = featuredHeaderSprite; break;
            case 1: headerImage.sprite = coinsHeaderSprite; break;
            case 2: headerImage.sprite = powerUpsHeaderSprite; break;
            case 3: headerImage.sprite = boardsHeaderSprite; break;
        }
    }

    public void ShowFeatured() => ShowTab(0);
    public void ShowCoins() => ShowTab(1);
    public void ShowPowerUps() => ShowTab(2);
    public void ShowBoards() => ShowTab(3);

    public void Show()
    {
        if (canvasShop != null) canvasShop.SetActive(true);
        ShowTab(0); // Default to Featured
    }

    /// <summary>
    /// Assign BackButton OnClick() → OnBackPressed() in Inspector.
    /// </summary>
    public void OnBackPressed()
    {
        if (canvasShop != null) canvasShop.SetActive(false);
        UImanager homeCtrl = UImanager.uimanager;
        if (homeCtrl != null)
            homeCtrl.HideShop();
    }

    public void Hide()
    {
        if (canvasShop != null) canvasShop.SetActive(false);
    }

    #region Purchase Methods

    public void PurchaseCoins(int packageIndex)
    {
        // Coin packages: 2500, 6500, 15000, 35000, 80000
        int[] coinAmounts = { 2500, 6500, 15000, 35000, 80000 };
        
        if (packageIndex < 0 || packageIndex >= coinAmounts.Length) return;

        int amount = coinAmounts[packageIndex];
        
        // TODO: Integrate with IAP system
        Debug.Log($"[Shop] Purchase coins package: {amount} coins");
        
        // For testing, add coins directly
        // int currentCoins = PlayerPrefs.GetInt("Coins", 0);
        // PlayerPrefs.SetInt("Coins", currentCoins + amount);
        // PlayerPrefs.Save();
    }

    public void PurchasePowerUp(int powerUpIndex)
    {
        string[] powerUpNames = { "HeadStart", "CoinMagnet", "Shield" };
        int[] prices = { 50, 70, 50 };
        
        if (powerUpIndex < 0 || powerUpIndex >= powerUpNames.Length) return;

        int currentCoins = PlayerPrefs.GetInt("Coins", 0);
        int price = prices[powerUpIndex];

        if (currentCoins >= price)
        {
            PlayerPrefs.SetInt("Coins", currentCoins - price);
            
            // Add power up to inventory
            string key = $"PowerUp_{powerUpNames[powerUpIndex]}";
            int count = PlayerPrefs.GetInt(key, 0);
            PlayerPrefs.SetInt(key, count + 1);
            PlayerPrefs.Save();
            
            Debug.Log($"[Shop] Purchased {powerUpNames[powerUpIndex]} for {price} coins");
        }
        else
        {
            Debug.Log($"[Shop] Not enough coins! Need {price}, have {currentCoins}");
        }
    }

    public void PurchaseBoard(int boardIndex)
    {
        string[] boardNames = { "VoltageDeck", "InfernoRide", "StormRunner", "NeonStrike" };
        int[] prices = { 50, 70, 100, 200 };
        
        if (boardIndex < 0 || boardIndex >= boardNames.Length) return;

        int currentCoins = PlayerPrefs.GetInt("Coins", 0);
        int price = prices[boardIndex];

        // Check if already owned
        string ownedKey = $"Board_{boardNames[boardIndex]}_Owned";
        if (PlayerPrefs.GetInt(ownedKey, 0) == 1)
        {
            Debug.Log($"[Shop] Board {boardNames[boardIndex]} already owned!");
            return;
        }

        if (currentCoins >= price)
        {
            PlayerPrefs.SetInt("Coins", currentCoins - price);
            PlayerPrefs.SetInt(ownedKey, 1);
            PlayerPrefs.Save();
            
            Debug.Log($"[Shop] Purchased {boardNames[boardIndex]} for {price} coins");
        }
        else
        {
            Debug.Log($"[Shop] Not enough coins! Need {price}, have {currentCoins}");
        }
    }

    public void PurchaseStarterPack(int packIndex)
    {
        // TODO: Integrate with IAP system
        Debug.Log($"[Shop] Purchase starter pack {packIndex}");
    }

    #endregion

    // Static instance for easy access
    private static ShopController _instance;
    public static ShopController Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<ShopController>();
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
        InitializeTabPanels();
    }
}

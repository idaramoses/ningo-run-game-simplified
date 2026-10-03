using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// New shop UI: coins/gems currency pills, COINS/GEMS tab visuals and the
/// coin-pack cards. Offers are IAP placeholders for now (logged).
/// </summary>
public class NingoShopPanel : MonoBehaviour
{
    [Header("Tabs")]
    public Button coinsTab;
    public Button gemsTab;
    public Sprite tabActiveSprite;
    public Sprite tabInactiveSprite;
    public GameObject coinsContent;
    public GameObject gemsContent;

    [Header("Coin Packs")]
    public int[] packCoins = { 500, 1200, 3000, 7500 };

    [Header("Gem Packs")]
    public int[] packGems = { 10, 30, 80, 200 };

    [Header("Pack Display Sprites")]
    public Sprite[] coinPackSprites;
    public Sprite[] gemPackSprites;

    [Header("Prices (USD)")]
    public float[] packPrices = { 0.99f, 1.99f, 3.99f, 7.99f };

    [Header("Purchase Flow UI")]
    public GameObject purchaseModal;
    public GameObject successModal;
    public GameObject failedModal;
    public Image packIconImage;
    public TMP_Text packAmountText;
    public TMP_Text packTypeText;
    public TMP_Text priceText;
    public GameObject processingObject;

    public enum SimResult { Random, AlwaysSuccess, AlwaysFail }

    [Header("Real IAP")]
    [Tooltip("When on, BUY NOW calls Unity IAP (IAPManager). When off, the simulated purchase runs instead.")]
    public bool useRealIAP = false;

    [Header("Simulated Purchase (used when Real IAP is off)")]
    public SimResult simulatedResult = SimResult.AlwaysSuccess;
    public float simulatedDelay = 1.0f;

    private int pendingIndex = -1;
    private bool pendingIsGems;
    private Coroutine purchaseRoutine;
    private bool awaitingIap;

    private void Awake()
    {
        if (coinsTab != null) coinsTab.onClick.AddListener(() => ShowTab(true));
        if (gemsTab != null) gemsTab.onClick.AddListener(() => ShowTab(false));

        IAPManager.EnsureExists();
        IAPManager.Instance.PurchaseSucceeded += OnIapSuccess;
        IAPManager.Instance.PurchaseFailed += OnIapFailed;
    }

    private void OnDestroy()
    {
        if (IAPManager.Instance != null)
        {
            IAPManager.Instance.PurchaseSucceeded -= OnIapSuccess;
            IAPManager.Instance.PurchaseFailed -= OnIapFailed;
        }
    }

    private void OnEnable()
    {
        ShowTab(true);
    }

    public void ShowTab(bool coins)
    {
        if (coinsContent != null) coinsContent.SetActive(coins);
        if (gemsContent != null) gemsContent.SetActive(!coins);
        if (coinsTab != null && coinsTab.GetComponent<Image>() != null)
            coinsTab.GetComponent<Image>().sprite = coins ? tabActiveSprite : tabInactiveSprite;
        if (gemsTab != null && gemsTab.GetComponent<Image>() != null)
            gemsTab.GetComponent<Image>().sprite = coins ? tabInactiveSprite : tabActiveSprite;
    }

    /// <summary>VIEW OFFER button handler - pack index into packCoins.</summary>
    public void OnViewOffer(int packIndex)
    {
        if (packIndex < 0 || packIndex >= packCoins.Length) return;
        if (SoundEffectsManager.Instance != null)
            SoundEffectsManager.Instance.PlayButtonClick();
        OpenPurchaseModal(packIndex, false);
    }

    /// <summary>VIEW OFFER button handler - pack index into packGems.</summary>
    public void OnViewGemOffer(int packIndex)
    {
        if (packIndex < 0 || packIndex >= packGems.Length) return;
        if (SoundEffectsManager.Instance != null)
            SoundEffectsManager.Instance.PlayButtonClick();
        OpenPurchaseModal(packIndex, true);
    }

    // ---------------------------------------------------------------------
    // PURCHASE FLOW
    // ---------------------------------------------------------------------

    private void OpenPurchaseModal(int index, bool gems)
    {
        if (purchaseModal == null) return;

        pendingIndex = index;
        pendingIsGems = gems;

        int amount = gems ? packGems[index] : packCoins[index];
        string typeName = gems ? "GEMS" : "COINS";
        float price = index < packPrices.Length ? packPrices[index] : 0.99f;

        if (packAmountText != null) packAmountText.text = amount.ToString("N0");
        if (packTypeText != null) packTypeText.text = typeName;
        if (priceText != null)
        {
            // prefer the store-localized price when real IAP is active
            string storePrice = null;
            if (useRealIAP && IAPManager.Instance != null)
                storePrice = IAPManager.Instance.GetLocalizedPrice(IAPManager.Instance.GetProductId(gems, index));
            priceText.text = storePrice ?? "$" + price.ToString("0.00");
        }

        var sprites = gems ? gemPackSprites : coinPackSprites;
        if (packIconImage != null && sprites != null && index < sprites.Length)
            packIconImage.sprite = sprites[index];

        if (processingObject != null) processingObject.SetActive(false);
        if (successModal != null) successModal.SetActive(false);
        if (failedModal != null) failedModal.SetActive(false);
        purchaseModal.SetActive(true);
    }

    /// <summary>BUY button inside the purchase confirm modal.</summary>
    public void ConfirmPurchase()
    {
        if (pendingIndex < 0 || purchaseRoutine != null || awaitingIap) return;
        if (SoundEffectsManager.Instance != null)
            SoundEffectsManager.Instance.PlayButtonClick();

        if (useRealIAP && IAPManager.Instance != null)
        {
            // Real store purchase - result arrives via OnIapSuccess / OnIapFailed
            string productId = IAPManager.Instance.GetProductId(pendingIsGems, pendingIndex);
            awaitingIap = true;
            if (processingObject != null) processingObject.SetActive(true);
            IAPManager.Instance.Buy(productId);
        }
        else
        {
            purchaseRoutine = StartCoroutine(SimulatedPurchase());
        }
    }

    /// <summary>CANCEL button / dim overlay inside the purchase confirm modal.</summary>
    public void CancelPurchase()
    {
        if (purchaseRoutine != null)
        {
            StopCoroutine(purchaseRoutine);
            purchaseRoutine = null;
        }
        awaitingIap = false;
        if (processingObject != null) processingObject.SetActive(false);
        if (purchaseModal != null) purchaseModal.SetActive(false);
        pendingIndex = -1;
    }

    /// <summary>CONTINUE button on the success modal.</summary>
    public void CloseSuccessModal()
    {
        if (successModal != null) successModal.SetActive(false);
        if (purchaseModal != null) purchaseModal.SetActive(false);
        pendingIndex = -1;
    }

    /// <summary>TRY AGAIN button on the failed modal - reopens the confirm step.</summary>
    public void RetryPurchase()
    {
        if (failedModal != null) failedModal.SetActive(false);
        if (purchaseModal != null) purchaseModal.SetActive(true);
        if (processingObject != null) processingObject.SetActive(false);
    }

    /// <summary>CLOSE button on the failed modal.</summary>
    public void CloseFailedModal()
    {
        if (failedModal != null) failedModal.SetActive(false);
        if (purchaseModal != null) purchaseModal.SetActive(false);
        pendingIndex = -1;
    }

    // ---- IAP callbacks ----

    private void OnIapSuccess(string productId)
    {
        if (!awaitingIap || pendingIndex < 0) return;
        awaitingIap = false;
        if (processingObject != null) processingObject.SetActive(false);

        int amount = pendingIsGems ? packGems[pendingIndex] : packCoins[pendingIndex];
        string key = pendingIsGems ? "Gems" : "Coins";
        PlayerPrefs.SetInt(key, PlayerPrefs.GetInt(key, 0) + amount);
        PlayerPrefs.Save();

        if (purchaseModal != null) purchaseModal.SetActive(false);
        if (successModal != null) successModal.SetActive(true);
        Debug.Log($"[Shop] IAP success: {productId} -> +{amount} {key}");
    }

    private void OnIapFailed(string productId, bool userCancelled)
    {
        if (!awaitingIap) return;
        awaitingIap = false;
        if (processingObject != null) processingObject.SetActive(false);

        if (userCancelled)
        {
            // player cancelled the store sheet - reopen confirm modal
            if (purchaseModal != null) purchaseModal.SetActive(true);
        }
        else
        {
            if (purchaseModal != null) purchaseModal.SetActive(false);
            if (failedModal != null) failedModal.SetActive(true);
        }
        Debug.Log($"[Shop] IAP failed: {productId} cancelled={userCancelled}");
    }

    private System.Collections.IEnumerator SimulatedPurchase()
    {
        if (processingObject != null) processingObject.SetActive(true);
        yield return new WaitForSecondsRealtime(simulatedDelay);
        purchaseRoutine = null;
        if (processingObject != null) processingObject.SetActive(false);

        bool success;
        switch (simulatedResult)
        {
            case SimResult.AlwaysSuccess: success = true; break;
            case SimResult.AlwaysFail: success = false; break;
            default: success = UnityEngine.Random.value < 0.85f; break;
        }

        if (success)
        {
            int amount = pendingIsGems ? packGems[pendingIndex] : packCoins[pendingIndex];
            string key = pendingIsGems ? "Gems" : "Coins";
            PlayerPrefs.SetInt(key, PlayerPrefs.GetInt(key, 0) + amount);
            PlayerPrefs.Save();

            if (purchaseModal != null) purchaseModal.SetActive(false);
            if (successModal != null) successModal.SetActive(true);
            Debug.Log($"[Shop] Purchase success: +{amount} {key}");
        }
        else
        {
            if (purchaseModal != null) purchaseModal.SetActive(false);
            if (failedModal != null) failedModal.SetActive(true);
            Debug.Log("[Shop] Purchase failed");
        }
    }
}

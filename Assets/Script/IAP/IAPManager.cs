using System;
using UnityEngine;
using UnityEngine.Purchasing;

/// <summary>
/// Singleton store front-end for Unity IAP.
///
/// Product IDs must match the in-app products created in Google Play Console /
/// App Store Connect:
///   coins_500, coins_1200, coins_3000, coins_7500
///   gems_10, gems_30, gems_80, gems_200
///
/// In the Unity Editor Unity IAP uses a fake store, so the full purchase
/// flow (success AND failure) can be tested without a device.
/// </summary>
public class IAPManager : MonoBehaviour, IStoreListener
{
    public static IAPManager Instance { get; private set; }

    public static readonly string[] CoinProductIds =
    {
        "coins_500", "coins_1200", "coins_3000", "coins_7500"
    };
    public static readonly string[] GemProductIds =
    {
        "gems_10", "gems_30", "gems_80", "gems_200"
    };

    /// <summary>Fired with the purchased product id after a verified purchase.</summary>
    public event Action<string> PurchaseSucceeded;
    /// <summary>Fired with (productId, userCancelled) when the purchase does not complete.</summary>
    public event Action<string, bool> PurchaseFailed;
    public event Action StoreInitialized;

    private IStoreController storeController;
    private IExtensionProvider extensionProvider;
    private bool initializing;

    public bool IsReady => storeController != null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        EnsureExists();
    }

    public static void EnsureExists()
    {
        if (Instance != null) return;
        new GameObject("IAPManager").AddComponent<IAPManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializePurchasing();
    }

    public void InitializePurchasing()
    {
        if (IsReady || initializing) return;
        initializing = true;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        foreach (var id in CoinProductIds) builder.AddProduct(id, ProductType.Consumable);
        foreach (var id in GemProductIds) builder.AddProduct(id, ProductType.Consumable);

        UnityPurchasing.Initialize(this, builder);
    }

    public string GetProductId(bool gems, int packIndex)
    {
        var arr = gems ? GemProductIds : CoinProductIds;
        return (packIndex >= 0 && packIndex < arr.Length) ? arr[packIndex] : null;
    }

    public void Buy(string productId)
    {
        if (productId == null || !IsReady)
        {
            Debug.LogWarning($"[IAP] Buy called but store not ready (product={productId})");
            PurchaseFailed?.Invoke(productId, false);
            return;
        }
        storeController.InitiatePurchase(productId);
    }

    /// <summary>Localized price string from the store ("₦1,500.00", "$0.99"), or null if unavailable.</summary>
    public string GetLocalizedPrice(string productId)
    {
        if (!IsReady || productId == null) return null;
        var p = storeController.products.WithID(productId);
        return (p != null && p.availableToPurchase) ? p.metadata.localizedPriceString : null;
    }

    // ---- IStoreListener ----

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        storeController = controller;
        extensionProvider = extensions;
        initializing = false;
        StoreInitialized?.Invoke();
        Debug.Log("[IAP] Store initialized");
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        initializing = false;
        Debug.LogError($"[IAP] Initialization failed: {error}");
    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
        initializing = false;
        Debug.LogError($"[IAP] Initialization failed: {error} - {message}");
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string id = args.purchasedProduct.definition.id;
        Debug.Log($"[IAP] Purchase succeeded: {id}");
        PurchaseSucceeded?.Invoke(id);
        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        string id = product != null ? product.definition.id : null;
        bool cancelled = reason == PurchaseFailureReason.UserCancelled;
        Debug.Log($"[IAP] Purchase failed: {id} ({reason})");
        PurchaseFailed?.Invoke(id, cancelled);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

/// <summary>
/// Singleton store front-end for Unity IAP v5.
///
/// Product IDs must match the in-app products created in Google Play Console /
/// App Store Connect:
///   coins_500, coins_1200, coins_3000, coins_7500
///   gems_10, gems_30, gems_80, gems_200
///
/// In the Unity Editor Unity IAP uses a fake store, so the full purchase
/// flow (success AND failure) can be tested without a device.
/// </summary>
public class IAPManager : MonoBehaviour
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

    private StoreController storeController;
    private bool connecting;
    private bool productsReady;

    public bool IsReady => productsReady;

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

    private void OnDestroy()
    {
        if (storeController == null) return;
        storeController.OnStoreConnected -= OnStoreConnected;
        storeController.OnStoreDisconnected -= OnStoreDisconnected;
        storeController.OnProductsFetched -= OnProductsFetched;
        storeController.OnProductsFetchFailed -= OnProductsFetchFailed;
        storeController.OnPurchasesFetched -= OnPurchasesFetched;
        storeController.OnPurchasesFetchFailed -= OnPurchasesFetchFailed;
        storeController.OnPurchasePending -= OnPurchasePending;
        storeController.OnPurchaseConfirmed -= OnPurchaseConfirmed;
        storeController.OnPurchaseFailed -= OnPurchaseFailed;
        storeController.OnPurchaseDeferred -= OnPurchaseDeferred;
    }

    public async void InitializePurchasing()
    {
        if (IsReady || connecting) return;
        connecting = true;

        storeController = UnityIAPServices.StoreController();

        // Subscribe to all events before Connect(): pending purchases can fire on reconnect.
        storeController.OnStoreConnected += OnStoreConnected;
        storeController.OnStoreDisconnected += OnStoreDisconnected;
        storeController.OnProductsFetched += OnProductsFetched;
        storeController.OnProductsFetchFailed += OnProductsFetchFailed;
        storeController.OnPurchasesFetched += OnPurchasesFetched;
        storeController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
        storeController.OnPurchasePending += OnPurchasePending;
        storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
        storeController.OnPurchaseFailed += OnPurchaseFailed;
        storeController.OnPurchaseDeferred += OnPurchaseDeferred;

        await storeController.Connect();
    }

    public string GetProductId(bool gems, int packIndex)
    {
        var arr = gems ? GemProductIds : CoinProductIds;
        return (packIndex >= 0 && packIndex < arr.Length) ? arr[packIndex] : null;
    }

    public void Buy(string productId)
    {
        var product = productId != null && storeController != null
            ? storeController.GetProductById(productId)
            : null;
        if (product == null || !IsReady)
        {
            Debug.LogWarning($"[IAP] Buy called but store not ready (product={productId})");
            PurchaseFailed?.Invoke(productId, false);
            return;
        }
        storeController.PurchaseProduct(product);
    }

    /// <summary>Localized price string from the store ("₦1,500.00", "$0.99"), or null if unavailable.</summary>
    public string GetLocalizedPrice(string productId)
    {
        if (storeController == null || productId == null) return null;
        var p = storeController.GetProductById(productId);
        return (p != null && p.availableToPurchase) ? p.metadata.localizedPriceString : null;
    }

    // ---- StoreController event handlers ----

    private void OnStoreConnected()
    {
        var products = new List<ProductDefinition>();
        foreach (var id in CoinProductIds) products.Add(new ProductDefinition(id, ProductType.Consumable));
        foreach (var id in GemProductIds) products.Add(new ProductDefinition(id, ProductType.Consumable));
        storeController.FetchProducts(products);
    }

    private void OnStoreDisconnected(StoreConnectionFailureDescription failure)
    {
        connecting = false;
        Debug.LogError($"[IAP] Store connection failed: {failure.Message}");
    }

    private void OnProductsFetched(List<Product> products)
    {
        connecting = false;
        productsReady = true;
        StoreInitialized?.Invoke();
        Debug.Log($"[IAP] Store initialized ({products.Count} products)");
        // Deliver any unfinished purchases left over from a previous session.
        storeController.FetchPurchases();
    }

    private void OnProductsFetchFailed(ProductFetchFailed failure)
    {
        connecting = false;
        Debug.LogError($"[IAP] Product fetch failed: {failure.FailureReason}");
    }

    private void OnPurchasesFetched(Orders orders)
    {
        // Pending orders are re-delivered through OnPurchasePending; nothing extra to do here.
        Debug.Log($"[IAP] Purchases fetched: {orders.PendingOrders.Count} pending");
    }

    private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failure)
    {
        Debug.LogError($"[IAP] Purchase fetch failed: {failure.Message}");
    }

    private void OnPurchasePending(PendingOrder order)
    {
        string id = FirstProductId(order);
        Debug.Log($"[IAP] Purchase succeeded: {id}");
        PurchaseSucceeded?.Invoke(id);
        storeController.ConfirmPurchase(order);
    }

    private void OnPurchaseConfirmed(Order order)
    {
        if (order is FailedOrder failed)
            Debug.LogError($"[IAP] Purchase confirmation failed: {failed.FailureReason}");
    }

    private void OnPurchaseFailed(FailedOrder order)
    {
        string id = FirstProductId(order);
        bool cancelled = order.FailureReason == PurchaseFailureReason.UserCancelled;
        Debug.Log($"[IAP] Purchase failed: {id} ({order.FailureReason})");
        PurchaseFailed?.Invoke(id, cancelled);
    }

    private void OnPurchaseDeferred(DeferredOrder order)
    {
        Debug.Log($"[IAP] Purchase deferred: {FirstProductId(order)}");
    }

    private static string FirstProductId(Order order)
    {
        return order?.CartOrdered?.Items()?.FirstOrDefault()?.Product?.definition.id;
    }
}

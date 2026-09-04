using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    private const string COINS_KEY = "PlayerCoins";
    private const string GEMS_KEY = "PlayerGems";

    private int coins = 0;
    private int gems = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadCurrency();
    }

    private void LoadCurrency()
    {
        coins = PlayerPrefs.GetInt(COINS_KEY, 0);
        gems = PlayerPrefs.GetInt(GEMS_KEY, 0);
        Debug.Log($"[CurrencyManager] Loaded - Coins: {coins}, Gems: {gems}");
    }

    public int GetCoins()
    {
        return coins;
    }

    public int GetGems()
    {
        return gems;
    }

    public void AddCoins(int amount)
    {
        if (amount < 0) return;
        coins += amount;
        SaveCurrency();
        Debug.Log($"[CurrencyManager] Added {amount} coins. Total: {coins}");
    }

    public void AddGems(int amount)
    {
        if (amount < 0) return;
        gems += amount;
        SaveCurrency();
        Debug.Log($"[CurrencyManager] Added {amount} gems. Total: {gems}");
    }

    public bool SpendCoins(int amount)
    {
        if (amount < 0 || coins < amount)
        {
            Debug.LogWarning($"[CurrencyManager] Cannot spend {amount} coins. Available: {coins}");
            return false;
        }

        coins -= amount;
        SaveCurrency();
        Debug.Log($"[CurrencyManager] Spent {amount} coins. Remaining: {coins}");
        return true;
    }

    public bool SpendGems(int amount)
    {
        if (amount < 0 || gems < amount)
        {
            Debug.LogWarning($"[CurrencyManager] Cannot spend {amount} gems. Available: {gems}");
            return false;
        }

        gems -= amount;
        SaveCurrency();
        Debug.Log($"[CurrencyManager] Spent {amount} gems. Remaining: {gems}");
        return true;
    }

    private void SaveCurrency()
    {
        PlayerPrefs.SetInt(COINS_KEY, coins);
        PlayerPrefs.SetInt(GEMS_KEY, gems);
        PlayerPrefs.Save();
    }

    public void ResetCurrency()
    {
        coins = 0;
        gems = 0;
        SaveCurrency();
        Debug.Log("[CurrencyManager] Currency reset");
    }
}

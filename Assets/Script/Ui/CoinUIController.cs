using UnityEngine;
using TMPro;

public class CoinUIController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private bool updateInRealtime = true; // For game scene
    
    private int lastCoinCount = -1;

    private void Start()
    {
        UpdateCoinDisplay();
    }

    private void Update()
    {
        if (updateInRealtime)
        {
            UpdateCoinDisplay();
        }
    }

    public void UpdateCoinDisplay()
    {
        int currentCoins = PlayerPrefs.GetInt(updateInRealtime ? "RunCoins" : "Coins", 0);
        
        // Only update text if value changed (optimization)
        if (currentCoins != lastCoinCount)
        {
            lastCoinCount = currentCoins;
            if (coinText != null)
            {
                coinText.text = currentCoins.ToString();
            }
        }
    }

    public void RefreshDisplay()
    {
        lastCoinCount = -1; // Force refresh
        UpdateCoinDisplay();
    }
}

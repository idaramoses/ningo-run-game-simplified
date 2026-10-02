using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Self-contained power-ups shop panel for the new Canvas_PowerUps UI.
/// Each row shows icon/name/desc/owned count/price; buying deducts coins
/// and increments the "PowerUp_<key>" inventory counter.
/// </summary>
public class NingoPowerUpsPanel : MonoBehaviour
{
    [System.Serializable]
    public class Row
    {
        public string key;            // PlayerPrefs suffix: PowerUp_<key>
        public int price;
        public TMP_Text ownedText;    // "Owned: N"
        public Button buyButton;
    }

    public Row[] rows = new Row[3];
    public TMP_Text coinsText;        // optional top pill text

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        foreach (var r in rows)
        {
            if (r.ownedText != null)
                r.ownedText.text = $"Owned: {PlayerPrefs.GetInt("PowerUp_" + r.key, 0)}";
        }
        if (coinsText != null)
            coinsText.text = PlayerPrefs.GetInt("Coins", 0).ToString("N0");
    }

    /// <summary>Wire each row's buy button to this with its index.</summary>
    public void Buy(int index)
    {
        if (index < 0 || index >= rows.Length) return;
        var r = rows[index];

        int coins = PlayerPrefs.GetInt("Coins", 0);
        if (coins < r.price)
        {
            Debug.Log($"[PowerUps] Not enough coins for {r.key} ({coins}/{r.price})");
            return;
        }

        PlayerPrefs.SetInt("Coins", coins - r.price);
        string key = "PowerUp_" + r.key;
        PlayerPrefs.SetInt(key, PlayerPrefs.GetInt(key, 0) + 1);
        PlayerPrefs.Save();

        if (SoundEffectsManager.Instance != null)
            SoundEffectsManager.Instance.PlayButtonClick();

        Refresh();
    }

    /// <summary>Wire the back button to this.</summary>
    public void OnBackPressed()
    {
        if (UImanager.uimanager != null)
            UImanager.uimanager.HidePowerUps();
        else
            gameObject.SetActive(false);
    }
}

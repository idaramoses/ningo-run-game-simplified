using UnityEngine;
using TMPro;

/// <summary>
/// Currency counter pill (coins / gems) shown on shop-style screens.
/// Reads PlayerPrefs ("Coins" / "Gems") and refreshes whenever the panel
/// opens plus on a short interval so purchases update live.
/// </summary>
public class CurrencyPill : MonoBehaviour
{
    public enum Currency { Coins, Gems }

    public Currency currency;
    public TMP_Text valueText;

    private float timer;

    private void OnEnable()
    {
        Refresh();
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.unscaledDeltaTime;
        if (timer >= 0.5f)
        {
            timer = 0f;
            Refresh();
        }
    }

    private void Refresh()
    {
        if (valueText == null) return;
        string key = currency == Currency.Coins ? "Coins" : "Gems";
        valueText.text = PlayerPrefs.GetInt(key, 0).ToString("N0");
    }
}

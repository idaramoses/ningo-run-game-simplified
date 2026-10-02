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

    private void Awake()
    {
        if (coinsTab != null) coinsTab.onClick.AddListener(() => ShowTab(true));
        if (gemsTab != null) gemsTab.onClick.AddListener(() => ShowTab(false));
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
        // TODO: wire to real IAP / offer screen
        Debug.Log($"[Shop] View offer: {packCoins[packIndex]} coins pack");
    }
}

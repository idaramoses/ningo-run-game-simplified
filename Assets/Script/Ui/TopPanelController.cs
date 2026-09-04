using UnityEngine;
using TMPro;

public class TopPanelController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text energyText;

    [Header("Settings")]
    [SerializeField] private bool updateInRealtime = true;

    private string lastUsername = "";
    private int lastCoins = -1;
    private int lastEnergy = -1;

    private void Start()
    {
        UpdateUI();
    }

    private void OnEnable()
    {
        UpdateUI();
    }

    private void Update()
    {
        if (updateInRealtime)
        {
            UpdateUI();
        }
    }

    public void UpdateUI()
    {
        // 1. Get and update Username
        string username = PlayerPrefs.GetString("user_display_name", "PLAYER");
        int level = PlayerPrefs.GetInt("PlayerLevel", 12);
        if (username != lastUsername)
        {
            lastUsername = username;
            if (usernameText != null)
            {
                usernameText.text = $"{username.ToUpper()}\n<color=#FFC700><size=80%>Level {level}</size></color>";
            }
        }

        // 2. Get and update Coins
        int currentCoins = PlayerPrefs.GetInt("Coins", 0);
        if (currentCoins != lastCoins)
        {
            lastCoins = currentCoins;
            if (coinsText != null)
            {
                coinsText.text = currentCoins.ToString();
            }
        }

        // 3. Get and update Skateboard/Energy Stats
        int currentEnergy = PlayerPrefs.GetInt("Energy", 5);
        if (currentEnergy != lastEnergy)
        {
            lastEnergy = currentEnergy;
            if (energyText != null)
            {
                energyText.text = currentEnergy.ToString();
            }
        }
    }

    public void Refresh()
    {
        lastUsername = "";
        lastCoins = -1;
        lastEnergy = -1;
        UpdateUI();
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// In-game HUD controller for the top bar during gameplay.
/// Manages: Pause button, coin count, distance run, and high score display.
/// Attach this to the UIManager GameObject in the Game scene.
/// </summary>
public class GameHUDController : MonoBehaviour
{
    [Header("HUD Text Elements")]
    public TMP_Text coinText;
    public TMP_Text distanceText;
    public TMP_Text highScoreText;

    [Header("Pause Button")]
    public Button pauseButton;

    [Header("Formatting")]
    public string distanceSuffix = "m";

    private int lastCoins = -1;
    private int lastDistance = -1;

    private void Start()
    {
        RefreshHighScore();
        AutoFindHUDTexts();
    }

    private void AutoFindHUDTexts()
    {
        if (coinText == null)
            coinText = FindHUDText("coin", "CoinText", "CoinsText", "CoinValue", "CoinCount", "RunCoins");

        if (distanceText == null)
            distanceText = FindHUDText("distance", "DistanceText", "DistanceValue", "ScoreText", "ScoreValue", "RunDistance");
    }

    private TMP_Text FindHUDText(string hint, params string[] exactNames)
    {
        // 1. Try exact names anywhere in the active hierarchy
        foreach (string name in exactNames)
        {
            GameObject go = GameObject.Find(name);
            if (go != null)
            {
                TMP_Text txt = go.GetComponent<TMP_Text>();
                if (txt != null)
                {
                    Debug.Log($"[GameHUDController] Auto-found {hint} text: {name}");
                    return txt;
                }
            }
        }

        // 2. Search under the active HUD canvas if available
        GameObject hudCanvas = UImanager.uimanager != null ? UImanager.uimanager.canvasHUD : null;
        if (hudCanvas != null)
        {
            TMP_Text[] texts = hudCanvas.GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text txt in texts)
            {
                string lower = txt.gameObject.name.ToLowerInvariant();
                if (lower.Contains(hint) || lower.Contains("coin") && hint == "coin")
                {
                    Debug.Log($"[GameHUDController] Auto-found {hint} text under Canvas_HUD: {txt.gameObject.name}");
                    return txt;
                }
            }
        }

        return null;
    }

    private void Update()
    {
        RefreshCoins();
        RefreshDistance();

        if (pauseButton != null && UImanager.uimanager != null)
        {
            pauseButton.interactable = !UImanager.uimanager.IsCountingDown;
        }
    }

    private void RefreshCoins()
    {
        if (coinText == null) return;

        int coins = PlayerPrefs.GetInt("RunCoins", 0);
        if (coins != lastCoins)
        {
            lastCoins = coins;
            coinText.text = coins.ToString("N0");
        }
    }

    public void ForceRefreshCoins()
    {
        lastCoins = -1;
        RefreshCoins();
    }

    private void RefreshDistance()
    {
        if (distanceText == null) return;

        float distance = 0f;
        if (UImanager.uimanager != null)
            distance = UImanager.uimanager.GetRunDistance();

        int distanceInt = Mathf.FloorToInt(distance);
        if (distanceInt != lastDistance)
        {
            lastDistance = distanceInt;
            distanceText.text = distanceInt.ToString("N0") + distanceSuffix;
        }
    }

    private void RefreshHighScore()
    {
        if (highScoreText == null) return;

        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = highScore.ToString("N0");
    }

    public void OnPausePressed()
    {
        if (UImanager.uimanager != null && !UImanager.uimanager.IsCountingDown)
            UImanager.uimanager.Pause();
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Controller for the "NEW HIGH SCORE" celebration panel.
/// Shows score, previous best, rewards, and back/play again buttons.
/// </summary>
public class HighScorePanelController : MonoBehaviour
{
    [Header("Header")]
    public TMP_Text titleText;
    public TMP_Text beatBestBannerText;

    [Header("Score Display")]
    public TMP_Text scoreLabel;
    public TMP_Text scoreValueText;
    public TMP_Text previousBestText;

    [Header("Rewards")]
    public GameObject rewardsGroup;
    public TMP_Text rewardCoinsText;
    public TMP_Text rewardGemsText;
    public Image rewardCoinsIcon;
    public Image rewardGemsIcon;

    [Header("Player Info Bar (Top)")]
    public Image playerAvatar;
    public TMP_Text playerNameText;
    public TMP_Text headerCoinsText;
    public TMP_Text headerGemsText;

    [Header("Animation")]
    public ParticleSystem confettiParticles;
    public UIConfettiController uiConfetti;
    public float scoreCountDuration = 1.5f;

    [Header("3D Preview")]
    [SerializeField] private GameObject previewContainer;

    private int finalScore;
    private int previousBest;
    private int rewardCoins;
    private int rewardGems;

    private void OnEnable()
    {
        RefreshPlayerInfo();

        if (previewContainer == null)
        {
            // Fallback: search root GameObjects in the active scene
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root.name == "CharacterPreviewContainer")
                {
                    previewContainer = root;
                    Debug.Log("[HighScorePanel] Found CharacterPreviewContainer in root GameObjects.");
                    break;
                }
            }
        }

        if (previewContainer != null)
        {
            Debug.Log("[HighScorePanel] Activating previewContainer: " + previewContainer.name);
            previewContainer.SetActive(true);
            var cam = previewContainer.GetComponentInChildren<Camera>(true);
            if (cam != null) cam.enabled = true;
        }
        else
        {
            Debug.LogWarning("[HighScorePanel] previewContainer is null!");
        }

        // Setup the character preview model inside the CharacterPreviewContainer
        // and play the celebrate animation!
        CharacterPreviewController previewCtrl = null;
        if (previewContainer != null)
        {
            previewCtrl = previewContainer.GetComponentInChildren<CharacterPreviewController>(true);
        }
        if (previewCtrl == null)
        {
            previewCtrl = CharacterPreviewController.Instance;
        }

        if (previewCtrl != null)
        {
            GameObject prefab = null;
            if (RunnerSelectionManager.Instance != null)
            {
                var currentRunner = RunnerSelectionManager.Instance.GetCurrentRunner();
                if (currentRunner != null)
                {
                    prefab = currentRunner.runnerPrefab;
                }
            }
            if (prefab == null)
            {
                prefab = RunnerManager.GetSelectedRunnerPrefab();
            }
            if (prefab == null && RunnerManager.Instance != null && RunnerManager.Instance.runners.Count > 0)
            {
                int idx = RunnerManager.GetSelectedRunnerIndex();
                idx = Mathf.Clamp(idx, 0, RunnerManager.Instance.runners.Count - 1);
                prefab = RunnerManager.Instance.runners[idx].runnerGameObject;
            }

            previewCtrl.UpdatePreview(prefab, playCelebrate: true);
        }
    }

    private void OnDisable()
    {
        if (previewContainer != null)
        {
            previewContainer.SetActive(false);
        }

        if (uiConfetti != null)
        {
            uiConfetti.Stop();
        }
    }

    /// <summary>
    /// Show the high score panel with celebration.
    /// </summary>
    public void ShowHighScore(int newScore, int prevBest, int bonusCoins = 500, int bonusGems = 2)
    {
        finalScore = newScore;
        previousBest = prevBest;
        rewardCoins = bonusCoins;
        rewardGems = bonusGems;

        // Title
        if (titleText != null)
            titleText.text = "NEW\nHIGH SCORE";

        if (beatBestBannerText != null)
            beatBestBannerText.text = "YOU BEAT YOUR BEST";

        // Score
        if (scoreLabel != null)
            scoreLabel.text = "SCORE";

        if (scoreValueText != null)
            scoreValueText.text = newScore.ToString("N0");

        if (previousBestText != null)
            previousBestText.text = $"<s>{prevBest.ToString("N0")}</s>";

        // Rewards
        if (rewardCoinsText != null)
            rewardCoinsText.text = bonusCoins.ToString();

        if (rewardGemsText != null)
            rewardGemsText.text = bonusGems.ToString();

        // Save new high score
        PlayerPrefs.SetInt("HighScore", newScore);

        // Award bonus rewards
        int currentCoins = PlayerPrefs.GetInt("Coins", 0);
        int currentGems = PlayerPrefs.GetInt("Gems", 0);
        PlayerPrefs.SetInt("Coins", currentCoins + bonusCoins);
        PlayerPrefs.SetInt("Gems", currentGems + bonusGems);
        PlayerPrefs.Save();

        RefreshPlayerInfo();

        // Play confetti
        if (confettiParticles != null)
            confettiParticles.Play();

        if (uiConfetti != null)
            uiConfetti.Play();

        // Animate score counting up
        StartCoroutine(AnimateScoreCount(newScore));
    }

    /// <summary>
    /// Check if current score beats high score, show appropriate panel.
    /// Returns true if it's a new high score.
    /// </summary>
    public bool CheckAndShow(int currentScore)
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);

        if (currentScore > highScore)
        {
            gameObject.SetActive(true);
            ShowHighScore(currentScore, highScore);
            return true;
        }

        return false;
    }

    private IEnumerator AnimateScoreCount(int targetScore)
    {
        if (scoreValueText == null) yield break;

        float elapsed = 0f;
        int startScore = 0;
        Vector3 origScale = scoreValueText.transform.localScale;

        while (elapsed < scoreCountDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / scoreCountDuration;
            // Ease out curve
            t = 1f - (1f - t) * (1f - t);

            int displayScore = Mathf.RoundToInt(Mathf.Lerp(startScore, targetScore, t));
            scoreValueText.text = displayScore.ToString("N0");
            yield return null;
        }

        scoreValueText.text = targetScore.ToString("N0");

        // Juicy punch scale animation on completion!
        float punchElapsed = 0f;
        float punchDuration = 0.15f;
        Vector3 punchScale = origScale * 1.2f;

        while (punchElapsed < punchDuration * 0.5f)
        {
            punchElapsed += Time.unscaledDeltaTime;
            scoreValueText.transform.localScale = Vector3.Lerp(origScale, punchScale, punchElapsed / (punchDuration * 0.5f));
            yield return null;
        }

        punchElapsed = 0f;
        while (punchElapsed < punchDuration * 0.5f)
        {
            punchElapsed += Time.unscaledDeltaTime;
            scoreValueText.transform.localScale = Vector3.Lerp(punchScale, origScale, punchElapsed / (punchDuration * 0.5f));
            yield return null;
        }
        scoreValueText.transform.localScale = origScale;
    }

    private void RefreshPlayerInfo()
    {
        if (playerNameText != null)
            playerNameText.text = PlayerPrefs.GetString("PlayerName", "PLAYER");

        if (headerCoinsText != null)
            headerCoinsText.text = PlayerPrefs.GetInt("Coins", 0).ToString("N0");

        if (headerGemsText != null)
            headerGemsText.text = PlayerPrefs.GetInt("Gems", 0).ToString();
    }

    // -------------------------
    // BUTTON HANDLERS (called from Inspector onClick)
    // -------------------------

    public void OnBackToHomePressed()
    {
        Debug.Log("[HighScorePanel] Back to Home pressed");
        if (UImanager.uimanager != null)
            UImanager.uimanager.Home();
        else
        {
            Debug.LogWarning("[HighScorePanel] UImanager is NULL - using fallback");
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Home");
        }
    }

    public void OnPlayAgainPressed()
    {
        Debug.Log("[HighScorePanel] Play Again pressed");
        if (UImanager.uimanager != null)
            UImanager.uimanager.Restart();
        else
        {
            Debug.LogWarning("[HighScorePanel] UImanager is NULL - using fallback");
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("GameScene");
        }
    }

    public void OnRestartPressed()
    {
        Debug.Log("[HighScorePanel] Restart pressed");
        if (UImanager.uimanager != null)
            UImanager.uimanager.Restart();
        else
        {
            Debug.LogWarning("[HighScorePanel] UImanager is NULL - using fallback");
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("GameScene");
        }
    }

    public void OnHomePressed()
    {
        Debug.Log("[HighScorePanel] Home pressed");
        if (UImanager.uimanager != null)
            UImanager.uimanager.Home();
        else
        {
            Debug.LogWarning("[HighScorePanel] UImanager is NULL - using fallback");
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Home");
        }
    }

    public void OnQuitPressed()
    {
        Debug.Log("[HighScorePanel] Quit pressed");
        PlayerPrefs.Save();

        if (UImanager.uimanager != null)
            UImanager.uimanager.Home();
        else
        {
            Debug.LogWarning("[HighScorePanel] UImanager is NULL - using fallback");
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Home");
        }
    }

    public void OnSharePressed()
    {
        Debug.Log($"[HighScorePanel] Share pressed - Score: {finalScore}");

        // Native share implementation
        string shareText = $"I just scored {finalScore:N0} in Wiki Cat Rush! Can you beat that? \ud83d\udc31\ud83c\udfc3";

#if UNITY_ANDROID
        using (AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent"))
        using (AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent"))
        {
            intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
            intentObject.Call<AndroidJavaObject>("setType", "text/plain");
            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), shareText);

            using (AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                AndroidJavaObject chooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "Share your score");
                currentActivity.Call("startActivity", chooser);
            }
        }
#elif UNITY_IOS
        // iOS share sheet - would need native plugin
        Application.OpenURL($"https://wikicatrush.app/share?score={finalScore}");
#else
        GUIUtility.systemCopyBuffer = shareText;
        Debug.Log("[HighScorePanel] Score copied to clipboard");
#endif
    }

}

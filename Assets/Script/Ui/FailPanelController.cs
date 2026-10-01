using UnityEngine;
using TMPro;
using System.Collections;

public class FailPanelController : MonoBehaviour
{
    [Header("Stats Display")]
    public TMP_Text scoreText;
    public TMP_Text distanceText;
    public TMP_Text coinsText;

    [Header("Animation Settings")]
    [SerializeField] private float countDuration = 1.2f;
    [SerializeField] private float punchScaleAmount = 1.15f;
    [SerializeField] private float punchDuration = 0.15f;

    private Coroutine animationCoroutine;
    private Vector3 scoreOrigScale = Vector3.one;
    private Vector3 distOrigScale = Vector3.one;
    private Vector3 coinsOrigScale = Vector3.one;

    private void Awake()
    {
        gameObject.SetActive(false);

        // Store original text scales for punch effects
        if (scoreText != null) scoreOrigScale = scoreText.transform.localScale;
        if (distanceText != null) distOrigScale = distanceText.transform.localScale;
        if (coinsText != null) coinsOrigScale = coinsText.transform.localScale;
    }

    public void Show()
    {
        gameObject.SetActive(true);
        StartCoroutine(ShowFailAd());
    }



    private IEnumerator ShowFailAd()

    {

        // FamiliesAdConfig and AdManager removed - skip ad flow

        Debug.Log("[FailPanelController] Ad system disabled - skipping fail ad");

        yield break;

        

        // Check if we should show ad based on Families Policy frequency

        // bool shouldShowAd = false;

        // if (FamiliesAdConfig.Instance != null)

        // {

        //     shouldShowAd = FamiliesAdConfig.Instance.ShouldShowFailureAd();

        // }



        // if (shouldShowAd && AdManager.Instance != null)

        // {

        //     Debug.Log("[FailPanelController] Showing interstitial ad for level failure");

        //     

        //     // Show warning overlay if enabled

        //     if (FamiliesAdConfig.Instance != null && FamiliesAdConfig.Instance.GetShowAdWarning() && AdWarningOverlay.Instance != null)

        //     {

        //         yield return AdWarningOverlay.Instance.ShowWarning(

        //             FamiliesAdConfig.Instance.GetAdWarningDuration(),

        //             null

        //         );

        //     }



        //     // Show interstitial ad first, then banner after it closes

        //     AdManager.Instance.ShowInterstitial(() =>

        //     {

        //         Debug.Log("[FailPanelController] Interstitial closed, showing banner");

        //         AdManager.Instance.ShowBanner();

        //     });

        // }

        // else

        // {

        //     Debug.Log("[FailPanelController] Skipping ad - frequency threshold not met, showing banner only");

        //     // Just show banner if no interstitial

        //     if (AdManager.Instance != null)

        //     {

        //         AdManager.Instance.ShowBanner();

        //     }

        // }

    }



    public void Hide()

    {

        gameObject.SetActive(false);

        

        // AdManager removed - skip banner hide

        // if (AdManager.Instance != null)

        // {

        //     AdManager.Instance.HideBanner();

        // }

    }



    public void OnRestartPressed()
    {
        Debug.Log("[FailPanelController] OnRestartPressed called");
        if (UImanager.uimanager != null)
        {
            // Same flow as the Complete Level Replay button: reset the runner
            // to the roadside idle pose in the background, then start the
            // current level through the normal node-click flow.
            Time.timeScale = 1f;
            gameObject.SetActive(false);
            UImanager.uimanager.ResetRunnerToHomePose();
            int current = LevelManager.Instance != null ? LevelManager.Instance.GetSelectedLevel() : 1;
            UImanager.uimanager.StartLevelAndRun(current);
        }
        else if (GameFlowController.Instance != null)
            GameFlowController.Instance.Restart();
        else
            Debug.LogError("[FailPanelController] No UImanager or GameFlowController found for restart!");
    }

    public void OnHomePressed()
    {
        Debug.Log("[FailPanelController] OnHomePressed called");
        if (UImanager.uimanager != null)
            UImanager.uimanager.Home();
        else if (GameFlowController.Instance != null)
            GameFlowController.Instance.Home();
        else
            Debug.LogError("[FailPanelController] No UImanager or GameFlowController found for home!");
    }

    public void ShowStats(int score, float distance, int coins)
    {
        Debug.Log($"[FailPanelController] Stats - Score: {score}, Distance: {distance:F1}, Coins: {coins}");

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        animationCoroutine = StartCoroutine(AnimateStatsCountUp(score, (int)distance, coins));
    }

    private IEnumerator AnimateStatsCountUp(int targetScore, int targetDist, int targetCoins)
    {
        float elapsed = 0f;

        // Reset scales
        if (scoreText != null) scoreText.transform.localScale = scoreOrigScale;
        if (distanceText != null) distanceText.transform.localScale = distOrigScale;
        if (coinsText != null) coinsText.transform.localScale = coinsOrigScale;

        // Count up phase
        while (elapsed < countDuration)
        {
            elapsed += Time.unscaledDeltaTime; // Must be unscaled because game is paused!
            float t = Mathf.Clamp01(elapsed / countDuration);
            
            // Nice ease-out curve
            t = 1f - Mathf.Pow(1f - t, 3);

            if (scoreText != null)
                scoreText.text = Mathf.RoundToInt(Mathf.Lerp(0, targetScore, t)).ToString();

            if (distanceText != null)
                distanceText.text = Mathf.RoundToInt(Mathf.Lerp(0, targetDist, t)).ToString();

            if (coinsText != null)
                coinsText.text = Mathf.RoundToInt(Mathf.Lerp(0, targetCoins, t)).ToString();

            yield return null;
        }

        // Set absolute final values
        if (scoreText != null) scoreText.text = targetScore.ToString();
        if (distanceText != null) distanceText.text = targetDist.ToString();
        if (coinsText != null) coinsText.text = targetCoins.ToString();

        // Punch effects on completion to feel super juicy!
        StartCoroutine(PunchScale(scoreText != null ? scoreText.transform : null, scoreOrigScale));
        yield return new WaitForSecondsRealtime(0.06f);
        StartCoroutine(PunchScale(distanceText != null ? distanceText.transform : null, distOrigScale));
        yield return new WaitForSecondsRealtime(0.06f);
        StartCoroutine(PunchScale(coinsText != null ? coinsText.transform : null, coinsOrigScale));
    }

    private IEnumerator PunchScale(Transform target, Vector3 origScale)
    {
        if (target == null) yield break;

        float elapsed = 0f;
        Vector3 punchScale = origScale * punchScaleAmount;

        // Scale Up
        while (elapsed < punchDuration * 0.5f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / (punchDuration * 0.5f);
            target.localScale = Vector3.Lerp(origScale, punchScale, t);
            yield return null;
        }

        // Scale Down
        elapsed = 0f;
        while (elapsed < punchDuration * 0.5f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / (punchDuration * 0.5f);
            target.localScale = Vector3.Lerp(punchScale, origScale, t);
            yield return null;
        }

        target.localScale = origScale;
    }

}


using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashController : MonoBehaviour
{
    public static SplashController Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject canvasSplash;
    [SerializeField] private GameObject canvasWelcome;
    [SerializeField] private CanvasGroup splashCanvasGroup;
    [SerializeField] private CanvasGroup welcomeCanvasGroup;
    [SerializeField] private CanvasGroup logoCanvasGroup;
    [SerializeField] private WelcomeLoadingBar welcomeLoadingBar;

    [Header("Next Screens (multi-scene mode)")]
    [SerializeField] private string userInfoSceneName = "UserInfo";
    [SerializeField] private string homeSceneName = "Home";
    [SerializeField] private GameObject canvasUserInfo;
    [SerializeField] private GameObject canvasMainMenu;

    [Header("Timing")] 
    [SerializeField] private float splashDisplayTime = 5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private float welcomeFadeInDuration = 0.6f;
    [SerializeField] private float loadingDuration = 3f;
    [SerializeField] private float logoFadeInDuration = 1f;
    [SerializeField] private float logoDisplayTime = 2f;
    [SerializeField] private float logoFadeOutDuration = 1f;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Start()
    {
        if (canvasSplash != null)
            canvasSplash.SetActive(true);

        if (canvasWelcome != null)
            canvasWelcome.SetActive(false);

        StartCoroutine(SplashToWelcomeFlow());
    }

    // Stubs for compatibility with the standalone LoginForm / LanguageSelectionUI scripts.
    // The integrated onboarding flow now lives in UserInfoController.
    public void ShowLoading(bool show, string message = null)
    {
        Debug.LogWarning($"[SplashController] ShowLoading stub called (show={show}, message={message}). Use UserInfoController instead.");
    }

    public void ShowLogin()
    {
        Debug.LogWarning("[SplashController] ShowLogin stub called. Use UserInfoController instead.");
    }

    public void ShowLanguage()
    {
        Debug.LogWarning("[SplashController] ShowLanguage stub called. Use UserInfoController instead.");
    }

    public void ShowWelcome()
    {
        Debug.LogWarning("[SplashController] ShowWelcome stub called. Use UserInfoController instead.");
    }

    private IEnumerator SplashToWelcomeFlow()
    {
        Debug.Log("[SplashController] Showing splash screen");

        // Start with logo invisible
        if (logoCanvasGroup != null)
            logoCanvasGroup.alpha = 0f;

        // Fade in logo
        if (logoCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < logoFadeInDuration)
            {
                elapsed += Time.deltaTime;
                logoCanvasGroup.alpha = Mathf.Clamp01(elapsed / logoFadeInDuration);
                yield return null;
            }
            logoCanvasGroup.alpha = 1f;
        }

        // Display logo
        yield return new WaitForSeconds(logoDisplayTime);

        // Fade out logo
        if (logoCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < logoFadeOutDuration)
            {
                elapsed += Time.deltaTime;
                logoCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / logoFadeOutDuration);
                yield return null;
            }
            logoCanvasGroup.alpha = 0f;
        }

        // Wait remaining splash time (if any)
        float remainingTime = splashDisplayTime - logoFadeInDuration - logoDisplayTime - logoFadeOutDuration;
        if (remainingTime > 0f)
            yield return new WaitForSeconds(remainingTime);

        // Activate welcome screen FULLY OPAQUE behind splash
        // (so as splash fades out, welcome is revealed - no Unity bg leak)
        if (canvasWelcome != null)
            canvasWelcome.SetActive(true);

        if (welcomeCanvasGroup != null)
            welcomeCanvasGroup.alpha = 1f;

        // Fade out splash to reveal welcome underneath
        if (splashCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                splashCanvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeOutDuration);
                yield return null;
            }
            splashCanvasGroup.alpha = 0f;
        }

        if (canvasSplash != null)
            canvasSplash.SetActive(false);

        Debug.Log("[SplashController] Showing welcome screen with loading bar");

        StartCoroutine(LoadNextSceneWithProgress());
    }

    private IEnumerator LoadNextSceneWithProgress()
    {
        float elapsed = 0f;
        while (elapsed < loadingDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / loadingDuration);
            progress = Mathf.SmoothStep(0f, 1f, progress);

            if (welcomeLoadingBar != null)
            {
                welcomeLoadingBar.SetProgress(progress);
            }

            yield return null;
        }

        if (welcomeLoadingBar != null)
            welcomeLoadingBar.SetProgress(1f);

        yield return new WaitForSeconds(0.3f);

        // In multi-scene mode, load the appropriate scene based on user info completion.
        bool userInfoCompleted = UserInfoController.IsUserInfoCompleted();
        Debug.Log($"[SplashController] User info completed: {userInfoCompleted}. Loading next scene.");

        if (userInfoCompleted)
        {
            if (!string.IsNullOrEmpty(homeSceneName))
            {
                if (SceneLoader.Instance != null)
                {
                    SceneLoader.Instance.LoadHomeScene();
                }
                else
                {
                    SceneManager.LoadSceneAsync(homeSceneName);
                }
            }
            else
            {
                if (canvasWelcome != null)
                    canvasWelcome.SetActive(false);
                if (canvasMainMenu != null)
                    canvasMainMenu.SetActive(true);
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(userInfoSceneName))
            {
                SceneManager.LoadSceneAsync(userInfoSceneName);
            }
            else
            {
                if (canvasWelcome != null)
                    canvasWelcome.SetActive(false);
                if (canvasUserInfo != null)
                    canvasUserInfo.SetActive(true);
            }
        }
    }
}

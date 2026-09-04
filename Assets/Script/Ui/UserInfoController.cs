using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UserInfoController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject usernamePanel;
    [SerializeField] private GameObject ageRangePanel;

    [Header("Username Step")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private Button nextButton;
    [SerializeField] private GameObject usernameExistWarning;

    [Header("Age Range Step")]
    [SerializeField] private Button[] ageButtons;
    [SerializeField] private Button letsRunButton;

    [Header("Loading State")]
    [SerializeField] private GameObject letsRunButtonText;
    [SerializeField] private GameObject letsRunButtonSpinner;

    [Header("Cat Animation")]
    [SerializeField] private CatBlinker catBlinker;
    [Tooltip("Delay before wink triggers after button click")]
    [SerializeField] private float winkDelay = 0.1f;

    [Header("Age Button Sprites")]
    [SerializeField] private Sprite normalButtonSprite;
    [SerializeField] private Sprite selectedButtonSprite;

    [Header("Next Screen (multi-scene mode)")]
    [SerializeField] private string homeSceneName = "Home";
    [SerializeField] private GameObject nextPanel;

    [Header("Animation")]
    [SerializeField] private float transitionDuration = 0.3f;
    [SerializeField] private float buttonFadeInDuration = 0.4f;
    [SerializeField] private float ageButtonStaggerDelay = 0.08f;
    [SerializeField] private float panelFadeInDuration = 0.5f;
    [SerializeField] private float panelStartScale = 0.85f;
    [SerializeField] private float panelEntryDelay = 0.2f;

    private string selectedAgeRange = "";
    private int selectedAgeIndex = -1;
    private CanvasGroup usernamePanelCanvasGroup;
    private CanvasGroup ageRangePanelCanvasGroup;

    private readonly string[] ageRanges = { "16 - 25", "26 - 35", "36 - 45", "46 - Above" };

    private void Start()
    {
        // Setup canvas groups for transitions
        SetupCanvasGroups();

        // Show username panel first
        ShowUsernameStep();

        // Initialize button text and spinner states
        if (letsRunButtonSpinner != null)
            letsRunButtonSpinner.SetActive(false);
        if (letsRunButtonText != null)
            letsRunButtonText.SetActive(true);

        // Setup button listeners
        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextPressed);

        if (letsRunButton != null)
        {
            letsRunButton.onClick.AddListener(OnLetsRunPressed);
            letsRunButton.interactable = false;
        }

        // Setup age buttons
        for (int i = 0; i < ageButtons.Length; i++)
        {
            int index = i;
            if (ageButtons[i] != null)
            {
                ageButtons[i].onClick.AddListener(() => OnAgeSelected(index));
            }
        }

        // Setup username input listener
        if (usernameInput != null)
        {
            usernameInput.onValueChanged.AddListener(OnUsernameChanged);
            
            // Fix for mobile: hide the native input dialog overlay
            // Text will appear directly in the TMP_InputField
            usernameInput.shouldHideMobileInput = true;
        }

        // Hide Next button initially - only show when user types
        if (nextButton != null)
            nextButton.gameObject.SetActive(false);

        // Find and hide username exist warning
        if (usernameExistWarning == null && usernamePanel != null)
        {
            usernameExistWarning = usernamePanel.transform.Find("UsernameExsitwarning")?.gameObject;
            if (usernameExistWarning == null)
            {
                usernameExistWarning = usernamePanel.transform.Find("UsernameExistWarning")?.gameObject;
            }
        }
        if (usernameExistWarning != null)
        {
            usernameExistWarning.SetActive(false);
        }

    }

    private void SetupCanvasGroups()
    {
        if (usernamePanel != null)
        {
            usernamePanelCanvasGroup = usernamePanel.GetComponent<CanvasGroup>();
            if (usernamePanelCanvasGroup == null)
                usernamePanelCanvasGroup = usernamePanel.AddComponent<CanvasGroup>();
        }

        if (ageRangePanel != null)
        {
            ageRangePanelCanvasGroup = ageRangePanel.GetComponent<CanvasGroup>();
            if (ageRangePanelCanvasGroup == null)
                ageRangePanelCanvasGroup = ageRangePanel.AddComponent<CanvasGroup>();
        }
    }

    private void ShowUsernameStep()
    {
        if (ageRangePanel != null)
            ageRangePanel.SetActive(false);

        if (usernamePanel != null)
        {
            usernamePanel.SetActive(true);
            StartCoroutine(AnimatePanelIn(usernamePanel, usernamePanelCanvasGroup, panelEntryDelay));
        }

        Debug.Log("[UserInfoController] Showing username step");
    }

    private IEnumerator AnimatePanelIn(GameObject panel, CanvasGroup cg, float delay = 0f)
    {
        if (panel == null) yield break;

        RectTransform rt = panel.GetComponent<RectTransform>();
        Vector3 targetScale = rt != null ? rt.localScale : Vector3.one;
        Vector3 startScale = targetScale * panelStartScale;

        // Set initial state
        if (cg != null) cg.alpha = 0f;
        if (rt != null) rt.localScale = startScale;

        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        while (elapsed < panelFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / panelFadeInDuration);
            // Ease out for a nice settle
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            if (cg != null) cg.alpha = eased;
            if (rt != null) rt.localScale = Vector3.Lerp(startScale, targetScale, eased);

            yield return null;
        }

        if (cg != null) cg.alpha = 1f;
        if (rt != null) rt.localScale = targetScale;
    }

    private bool nextButtonShown = false;
    private Coroutine checkUsernameCoroutine;

    private void OnUsernameChanged(string value)
    {
        bool hasText = !string.IsNullOrWhiteSpace(value);
        bool isValid = hasText && value.Length >= 3;

        if (nextButton == null) return;

        if (hasText && !nextButtonShown)
        {
            nextButtonShown = true;
            StartCoroutine(FadeInUI(nextButton.gameObject, buttonFadeInDuration));
        }
        else if (!hasText && nextButtonShown)
        {
            nextButtonShown = false;
            nextButton.gameObject.SetActive(false);
        }

        nextButton.interactable = isValid;

        // Cancel previous check coroutine
        if (checkUsernameCoroutine != null)
        {
            StopCoroutine(checkUsernameCoroutine);
        }

        if (isValid)
        {
            checkUsernameCoroutine = StartCoroutine(CheckUsernameExistsCoroutine(value.Trim()));
        }
        else
        {
            if (usernameExistWarning != null)
                usernameExistWarning.SetActive(false);
        }
    }

    private IEnumerator CheckUsernameExistsCoroutine(string username)
    {
        // Wait 0.3 seconds to debounce typing
        yield return new WaitForSeconds(0.3f);

        bool exists = false;

        // Search online UGS leaderboard if signed in
        if (UGSManager.Instance != null && UGSManager.Instance.IsSignedIn)
        {
            var task = UGSManager.Instance.GetScoresAsync("wiki_cat_rush");
            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.IsCompleted && task.Exception == null && task.Result != null)
            {
                foreach (var entry in task.Result)
                {
                    if (string.Equals(entry.playerName, username, System.StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }
            }
        }

        if (usernameExistWarning != null)
        {
            usernameExistWarning.SetActive(true);
            var tmp = usernameExistWarning.GetComponentInChildren<TMPro.TMP_Text>();
            if (tmp != null)
            {
                if (exists)
                {
                    tmp.text = "Username already taken";
                    tmp.color = new Color(0.9f, 0.2f, 0.2f); // red
                }
                else
                {
                    tmp.text = "Username is available";
                    tmp.color = new Color(0.2f, 0.8f, 0.3f); // green
                }
            }
        }

        nextButton.interactable = !exists;
    }

    private IEnumerator FadeInUI(GameObject target, float duration, float startDelay = 0f)
    {
        if (target == null) yield break;

        CanvasGroup cg = target.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = target.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        target.SetActive(true);

        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    public void OnNextPressed()
    {
        if (usernameInput == null || string.IsNullOrWhiteSpace(usernameInput.text))
        {
            Debug.LogWarning("[UserInfoController] Username is empty");
            return;
        }

        string username = usernameInput.text.Trim();
        PlayerPrefs.SetString("user_display_name", username);
        PlayerPrefs.Save();

        Debug.Log($"[UserInfoController] Username saved: {username}");

        // Sync with Unity Gaming Services in the background
        if (UGSManager.Instance != null)
        {
            _ = UGSManager.Instance.UpdatePlayerNameAsync(username);
        }

        // Trigger cat wink
        TriggerCatWink();

        StartCoroutine(TransitionToAgeStep());
    }

    private IEnumerator TransitionToAgeStep()
    {
        if (usernamePanel != null)
            usernamePanel.SetActive(false);

        if (ageRangePanel != null)
        {
            ageRangePanel.SetActive(true);
            StartCoroutine(AnimatePanelIn(ageRangePanel, ageRangePanelCanvasGroup, 0f));
        }

        // Hide Let's Run button initially (it shows after age is selected)
        if (letsRunButton != null)
            letsRunButton.gameObject.SetActive(false);

        // Fade in age buttons with stagger
        for (int i = 0; i < ageButtons.Length; i++)
        {
            if (ageButtons[i] != null)
            {
                StartCoroutine(FadeInUI(
                    ageButtons[i].gameObject,
                    buttonFadeInDuration,
                    i * ageButtonStaggerDelay
                ));
            }
        }

        Debug.Log("[UserInfoController] Showing age range step");
        yield break;
    }

    public void OnAgeSelected(int index)
    {
        if (index < 0 || index >= ageRanges.Length) return;

        selectedAgeIndex = index;
        selectedAgeRange = ageRanges[index];

        // Update button visuals
        for (int i = 0; i < ageButtons.Length; i++)
        {
            if (ageButtons[i] == null) continue;

            Image buttonImage = ageButtons[i].GetComponent<Image>();

            if (i == index)
            {
                // Selected
                if (buttonImage != null && selectedButtonSprite != null)
                    buttonImage.sprite = selectedButtonSprite;
            }
            else
            {
                // Not selected
                if (buttonImage != null && normalButtonSprite != null)
                    buttonImage.sprite = normalButtonSprite;
            }
        }

        // Fade in Let's Run button (only first time it appears)
        if (letsRunButton != null)
        {
            if (!letsRunButton.gameObject.activeSelf)
                StartCoroutine(FadeInUI(letsRunButton.gameObject, buttonFadeInDuration));
            letsRunButton.interactable = true;
        }

        Debug.Log($"[UserInfoController] Age range selected: {selectedAgeRange}");
    }

    public void OnLetsRunPressed()
    {
        if (string.IsNullOrEmpty(selectedAgeRange))
        {
            Debug.LogWarning("[UserInfoController] No age range selected");
            return;
        }

        PlayerPrefs.SetString("user_age_range", selectedAgeRange);
        PlayerPrefs.SetInt("user_info_completed", 1);
        PlayerPrefs.Save();

        Debug.Log("[UserInfoController] User info saved. Showing main menu.");

        // Hide text and show progress spinner
        if (letsRunButtonText != null)
            letsRunButtonText.SetActive(false);
        if (letsRunButtonSpinner != null)
            letsRunButtonSpinner.SetActive(true);

        // Make button non-interactable to prevent double-clicks
        if (letsRunButton != null)
            letsRunButton.interactable = false;

        // Trigger cat wink before showing main menu
        TriggerCatWink();

        StartCoroutine(ShowMainMenuAfterWink());
    }

    private CatBlinker GetActiveCatBlinker()
    {
        if (usernamePanel != null && usernamePanel.activeInHierarchy)
        {
            var blinker = usernamePanel.GetComponentInChildren<CatBlinker>(true);
            if (blinker != null) return blinker;
        }
        if (ageRangePanel != null && ageRangePanel.activeInHierarchy)
        {
            var blinker = ageRangePanel.GetComponentInChildren<CatBlinker>(true);
            if (blinker != null) return blinker;
        }
        return catBlinker;
    }

    private void TriggerCatWink()
    {
        if (GetActiveCatBlinker() != null)
            StartCoroutine(WinkAfterDelay());
    }

    private IEnumerator WinkAfterDelay()
    {
        yield return new WaitForSeconds(winkDelay);
        CatBlinker activeBlinker = GetActiveCatBlinker();
        if (activeBlinker != null && activeBlinker.gameObject.activeInHierarchy)
            activeBlinker.TriggerWink();
    }

    private IEnumerator ShowMainMenuAfterWink()
    {
        // Wait long enough for wink to be visible before showing the main menu / loading scene
        yield return new WaitForSeconds(0.6f);

        if (!string.IsNullOrEmpty(homeSceneName))
        {
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadHomeScene();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(homeSceneName);
            }
        }
        else
        {
            if (gameObject != null)
                gameObject.SetActive(false);

            if (nextPanel != null)
                nextPanel.SetActive(true);
        }
    }

    public static bool IsUserInfoCompleted()
    {
        return PlayerPrefs.GetInt("user_info_completed", 0) == 1;
    }

    public static string GetUsername()
    {
        return PlayerPrefs.GetString("user_display_name", "Player");
    }

    public static string GetAgeRange()
    {
        return PlayerPrefs.GetString("user_age_range", "");
    }
}

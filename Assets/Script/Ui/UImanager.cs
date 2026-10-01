using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// class quản lý taonf bộ hệ thống menu Ui vật phẩm và dữ liệu
/// </summary>
public class UImanager : MonoBehaviour
{
    public static UImanager uimanager;
    public int frameRate = 60;

    #region Splash / Welcome / UserInfo (single-scene)
    [Header("Splash / Welcome")]
    public GameObject canvasSplash;
    public GameObject canvasWelcome;
    public CanvasGroup splashCanvasGroup;
    public CanvasGroup welcomeCanvasGroup;
    public CanvasGroup logoCanvasGroup;
    public WelcomeLoadingBar welcomeLoadingBar;

    [Header("User Info")]
    public GameObject canvasUserInfo;
    public GameObject usernamePanel;
    public GameObject ageRangePanel;
    public TMP_InputField usernameInput;
    public Button nextButton;
    public GameObject usernameExistWarning;
    public Button[] ageButtons;
    public Button letsRunButton;
    public GameObject letsRunButtonText;
    public GameObject letsRunButtonSpinner;
    public CatBlinker catBlinker;
    public Sprite normalButtonSprite;
    public Sprite selectedButtonSprite;

    [Header("Flow Timing")]
    [Tooltip("If true, skip splash/welcome/userinfo and go straight to the home screen.")]
    public bool skipSplashWelcomeAndUserInfo = false;
    public float splashDisplayTime = 5f;
    public float fadeOutDuration = 0.5f;
    public float welcomeFadeInDuration = 0.6f;
    public float loadingDuration = 3f;
    public float logoFadeInDuration = 1f;
    public float logoDisplayTime = 2f;
    public float logoFadeOutDuration = 1f;
    public float userInfoTransitionDuration = 0.3f;
    public float userInfoButtonFadeInDuration = 0.4f;
    public float userInfoAgeButtonStaggerDelay = 0.08f;
    public float userInfoPanelFadeInDuration = 0.5f;
    public float userInfoPanelStartScale = 0.85f;
    public float userInfoPanelEntryDelay = 0.2f;

    private string selectedAgeRange = "";
    private int selectedAgeIndex = -1;
    private CanvasGroup usernamePanelCanvasGroup;
    private CanvasGroup ageRangePanelCanvasGroup;
    private bool nextButtonShown = false;
    private Coroutine checkUsernameCoroutine;
    private readonly string[] ageRanges = { "16 - 25", "26 - 35", "36 - 45", "46 - Above" };
    #endregion

    #region Canvas & Panel References
    [Header("Main Canvases")]
    public GameObject canvasHome;
    public GameObject canvasHUD;
    public GameObject canvasGame;

    [Header("Sub-Menu Canvases")]
    public GameObject canvasDailyGift;
    public GameObject canvasMissions;
    public GameObject canvasSettings;
    public GameObject canvasShop;
    public GameObject canvasLeaderboard;
    public GameObject canvasCharacters;
    public GameObject canvasLevel;

    [Header("Gameplay Panels")]
    public GameObject pausePanel;
    public GameObject failPanel;
    public GameObject highScorePanel;

    [Header("Countdown")]
    public TMPro.TMP_Text countdownText;
    public float failSequenceDelay = 2f;
    #endregion

    #region Game State
    private bool isPaused;
    private bool isFailed;
    private bool isCountingDown;
    public bool IsCountingDown => isCountingDown;

    /// <summary>Fired once after the opening countdown finishes and the level is truly running.</summary>
    public static event System.Action OnLevelReady;
    #endregion

    #region Home UI
    [Header("Home HUD Text")]
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private Image avatarImage;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text energyText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text tapToPlayText;
    [SerializeField] private string tapToPlayLabel = "TAP TO PLAY";
    [SerializeField] private TMP_Text learnToPlayText;
    [SerializeField] private Color learnToPlayColor = new Color(1f, 0.84f, 0f);
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text langText;
    [SerializeField] private TMP_Text localCoinsText;

    [Header("Badges")]
    [SerializeField] private GameObject dailyGiftBadge;
    [SerializeField] private GameObject missionsBadge;
    [SerializeField] private GameObject shopBadge;

    [Header("Sub-Panels (Home)")]
    [SerializeField] private GameObject canvasCoins;
    [SerializeField] private GameObject canvasPowerUps;
    [SerializeField] private GameObject canvasBoards;

    [Header("Controllers")]
    [SerializeField] private DailyGiftController dailyGiftController;
    [SerializeField] private MissionsController missionsController;
    [SerializeField] private ShopController shopController;
    [SerializeField] private SettingsController settingsController;
    [SerializeField] private LeaderboardController leaderboardController;
    [SerializeField] private CharactersController charactersController;

    private int lastLocalCoins = -1;
    private int lastHighScore = -1;
    private bool isTransitioning = false;
    private float uiRefreshTimer = 0f;
    private const float UI_REFRESH_INTERVAL = 0.5f;
    #endregion

    // Core lightweight game state
    public static int coinmuving;
    public static int coin;
    int needkey = 0;
    bool Closebox = true;
    bool alowopen = true;
    float Slidertimerdowload;
    public static bool selectkey;
    int checkthedie;
    bool calll;
    bool alowcall;
    public Animator amin;
    public GameObject camerafolow;

    void Awake()
    {
        Application.targetFrameRate = frameRate;
    }

    void Start()
    {
        StartCoroutine(MainEntryFlow());
    }

    void OnEnable()
    {
        lastLocalCoins = -1;
        lastHighScore = -1;
        UpdateUI();
    }

    IEnumerator MainEntryFlow()
    {
        // Splash / Welcome / UserInfo first
        if (canvasSplash != null) canvasSplash.SetActive(true);
        if (canvasWelcome != null) canvasWelcome.SetActive(false);
        if (canvasUserInfo != null) canvasUserInfo.SetActive(false);

        if (skipSplashWelcomeAndUserInfo)
        {
            // Skip splash, welcome, and user-info flows. Hide those canvases and go straight to home.
            if (canvasSplash != null) canvasSplash.SetActive(false);
            if (canvasWelcome != null) canvasWelcome.SetActive(false);
            if (canvasUserInfo != null) canvasUserInfo.SetActive(false);
            yield return null;
        }
        else
        {
            yield return StartCoroutine(SplashFlow());
            yield return StartCoroutine(WelcomeFlow());

            // Ensure splash/welcome canvases are fully hidden before the main menu flow starts
            if (canvasSplash != null) canvasSplash.SetActive(false);
            if (canvasWelcome != null) canvasWelcome.SetActive(false);
            if (canvasUserInfo != null) canvasUserInfo.SetActive(false);
            yield return null;

            if (!IsUserInfoCompleted())
            {
                yield return StartCoroutine(UserInfoFlow());
            }
        }

        // Original startup (legacy UI stripped)
        SetingInStart();
        getvan = true;
        Closebox = true;

        // Show home canvas after splash/welcome/userinfo flow completes
        if (canvasHome != null) canvasHome.SetActive(true);
        if (canvasHUD != null) canvasHUD.SetActive(false);

        // Home UI initialization (merged from HomeUIController)
        AutoDiscoverCanvases();
        UpdateUI();

        if (tapToPlayText != null && !string.IsNullOrEmpty(tapToPlayLabel))
            tapToPlayText.text = tapToPlayLabel;

        StartCoroutine(InitializePlayerPositionForHome());
    }

    private static readonly List<UnityEngine.EventSystems.RaycastResult> tapToPlayRaycastResults = new List<UnityEngine.EventSystems.RaycastResult>();

    /// <summary>
    /// Detects a tap/click anywhere on the Home screen that does NOT land on an actual
    /// interactive control (Button/Toggle/etc. - checked via Selectable), and starts the run.
    /// This is done via an explicit raycast rather than an invisible full-screen button because
    /// Canvas_Home may already contain raycastable background panels that would otherwise block
    /// a sibling-order-based catcher from ever receiving the click.
    /// </summary>
    private void CheckTapToPlayInput()
    {
        if (canvasHome == null)
        {
            if (Time.frameCount % 60 == 0)
                Debug.LogWarning("[UImanager] CheckTapToPlayInput: canvasHome is NULL - could not find/assign the Home canvas!");
            return;
        }
        if (!canvasHome.activeInHierarchy) return;
        if (isTransitioning) return;
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            // Log once per second so this doesn't spam if there's really no EventSystem.
            if (Time.frameCount % 60 == 0)
                Debug.LogWarning("[UImanager] CheckTapToPlayInput: no EventSystem.current found in the scene - taps cannot be detected!");
            return;
        }

        bool tapped = Input.GetMouseButtonDown(0);
        if (!tapped && Input.touchCount > 0)
        {
            tapped = Input.GetTouch(0).phase == UnityEngine.TouchPhase.Began;
        }
        if (!tapped) return;

        Vector2 screenPos = Input.touchCount > 0 ? (Vector2)Input.GetTouch(0).position : (Vector2)Input.mousePosition;
        Debug.Log($"[UImanager] CheckTapToPlayInput: tap detected at {screenPos}");

        var pointerData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
        {
            position = screenPos
        };

        tapToPlayRaycastResults.Clear();
        UnityEngine.EventSystems.EventSystem.current.RaycastAll(pointerData, tapToPlayRaycastResults);
        Debug.Log($"[UImanager] CheckTapToPlayInput: raycast hit {tapToPlayRaycastResults.Count} object(s): " +
                  string.Join(", ", tapToPlayRaycastResults.ConvertAll(r => r.gameObject.name)));

        for (int i = 0; i < tapToPlayRaycastResults.Count; i++)
        {
            var hitObj = tapToPlayRaycastResults[i].gameObject;
            if (hitObj == null) continue;

            Selectable sel = hitObj.GetComponentInParent<Selectable>();
            if (IsTapToPlayControl(hitObj, sel))
            {
                // Tapping directly on the TapToPlayText / play button should trigger TapToPlay
                Debug.Log($"[UImanager] CheckTapToPlayInput: tap landed on play control '{hitObj.name}' - starting game!");
                OnTapToPlay();
                return;
            }

            if (IsBlockingInteractiveControl(hitObj))
            {
                // The tap landed on a real interactive control (button, toggle, etc.) -
                // let it handle the click normally instead of starting the run.
                Debug.Log($"[UImanager] CheckTapToPlayInput: tap landed on interactive control '{(sel != null ? sel.gameObject.name : hitObj.name)}' - ignoring.");
                return;
            }
        }

        Debug.Log("[UImanager] CheckTapToPlayInput: no blocking interactive control hit - calling OnTapToPlay()");
        OnTapToPlay();
    }

    private bool IsTapToPlayControl(GameObject go, Selectable sel)
    {
        if (go != null)
        {
            string goName = go.name.ToLower();
            string parentName = go.transform.parent != null ? go.transform.parent.name.ToLower() : "";
            if (goName.Contains("taptoplay") || goName.Contains("tap_to_play") || goName.Contains("tap to play") ||
                goName.Contains("centerbutton") || goName.Contains("startbutton") || goName.Contains("playbutton") ||
                parentName.Contains("centerbutton") || parentName.Contains("startbutton") || parentName.Contains("playbutton"))
            {
                return true;
            }
        }

        if (sel != null)
        {
            string selName = sel.gameObject.name.ToLower();
            string parentName = sel.transform.parent != null ? sel.transform.parent.name.ToLower() : "";
            if (selName.Contains("taptoplay") || selName.Contains("tap_to_play") || selName.Contains("tap to play") ||
                selName.Contains("centerbutton") || selName.Contains("startbutton") || selName.Contains("playbutton") ||
                parentName.Contains("centerbutton") || parentName.Contains("startbutton") || parentName.Contains("playbutton"))
            {
                return true;
            }

            if (sel is Button btn)
            {
                int count = btn.onClick.GetPersistentEventCount();
                for (int j = 0; j < count; j++)
                {
                    string method = btn.onClick.GetPersistentMethodName(j);
                    if (method == nameof(OnTapToPlay) || method == nameof(OnHomePressed) ||
                        method == "OnTapToPlay" || method == "OnHomePressed" || method == "Play")
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool IsBlockingInteractiveControl(GameObject go)
    {
        if (go == null) return false;

        Selectable sel = go.GetComponentInParent<Selectable>();
        if (sel == null || !sel.isActiveAndEnabled || !sel.interactable)
        {
            return false;
        }

        // If it's a Tap-To-Play trigger / play button, it is not a blocking control
        if (IsTapToPlayControl(go, sel))
        {
            return false;
        }

        // If the selectable is a Button, ensure it has at least one valid persistent listener or custom handler
        if (sel is Button btn)
        {
            if (btn.onClick.GetPersistentEventCount() == 0)
            {
                return false;
            }
        }

        return true;
    }

    public void SetingInStart()
    {
        coin = 0;
        coinmuving = 0;
        amin = camerafolow != null ? camerafolow.gameObject.GetComponent<Animator>() : null;
        uimanager = this;

        // Splash/welcome canvas is the loading screen. Go straight to the main menu.
        if (Perencamera.managerscen != null)
            Perencamera.managerscen.GetComponent<Animator>().enabled = true;
        if (Soundmanager.soundmanager != null)
            Soundmanager.soundmanager.PlayBackgroudSound();

        alowcall = true;
        selectkey = false;
        calll = true;
    }

    public void Again()
    {
        coin = 0;
        coinmuving = 0;
        amin = camerafolow != null ? camerafolow.gameObject.GetComponent<Animator>() : null;
        uimanager = this;
        alowcall = true;
        selectkey = false;
        calll = true;
    }

    /// <summary>
    /// chơi lại
    /// </summary>
    public void playAgain()
    {
        if (calll)
        {
            Playermuving.player.GetComponent<CapsuleCollider>().center = new Vector3(0, -0.08f, 0);
            Playermuving.player.GetComponent<CapsuleCollider>().radius = 0.46f;
            Playermuving.player.GetComponent<CapsuleCollider>().height = 1.77f;
            Time.timeScale = 0.8F;
            Playermuving.speedmuving = 15;
            inthepanelpause.playagain = true;
            calll = false;
            coin = 0;
            coinmuving = 0;
            Perencamera.managerscen.playallgame();
            StartCoroutine(playagain());
            emty.emtyplayer.ResutTranformemty();

            // Switch back to HUD when the run resumes
            if (canvasHome != null) canvasHome.SetActive(false);
            if (canvasHUD != null) canvasHUD.SetActive(true);
        }
    }

    /// <summary>
    /// sửa lỗi animation không chạy
    /// </summary>
    IEnumerator playagain()
    {
        Playermuving.isplay = true;
        yield return new WaitForSeconds(0.5f);
        calll = true;
        Playermuving.player.clicontheplayagainseleckey();
        for (int j = 0; j < 4; j++)
        {
            yield return new WaitForSeconds(0.5f);
            Playermuving.player.clicontheplayagainseleckey();
        }
    }

    /// <summary>
    /// vào chơi
    /// </summary>
    public void play()
    {
        // Reset run state for a fresh run
        coin = 0;
        coinmuving = 0;
        PlayerPrefs.SetInt("RunCoins", 0);

        StartCoroutine(Playdelay());
        if (Soundmanager.soundmanager != null)
            Soundmanager.soundmanager.PlayPoliceSound();
    }

    IEnumerator Playdelay()
    {
        if (mapitro.instance != null) mapitro.instance.Muvingship();
        if (emty.emtyplayer != null) emty.emtyplayer.StartCoroutine(emty.emtyplayer.intheplay());
        yield return new WaitForSeconds(0.3f);
        if (Perencamera.managerscen != null)
        {
            Animator sceneAnim = Perencamera.managerscen.GetComponent<Animator>();
            if (sceneAnim != null) sceneAnim.SetBool("play", true);
        }
        if (Camerafolow.camfolowplayer != null) Camerafolow.camfolowplayer.distance = 10;
        yield return new WaitForSeconds(0.5f);
        Playermuving.isplay = true;
        Playermuving.speedmuving = 5;
        if (Playermuving.player != null) Playermuving.player.muvingtomodelonthestart();
        if (amin != null)
        {
            amin.SetBool("play", true);
            amin.SetBool("again", false);
        }
        if (emty.emtyplayer != null)
        {
            emty.emtyplayer.actac();
            emty.emtyplayer.animationrunplay();
        }
        emty.die = 1;

        // Switch from home canvas to HUD when the run starts
        if (canvasHome != null) canvasHome.SetActive(false);
        if (canvasHUD != null) canvasHUD.SetActive(true);

        isTransitioning = false;
    }

    /// <summary>
    /// thua
    /// </summary>
    public void Lost()
    {
        PlayerPrefs.Save();
        managerdata.manager.savemuving(coinmuving);
        managerdata.manager.savecoin(coin);
        if (alowcall)
        {
            alowcall = false;
            checkthedie++;
            StartCoroutine(delayforAgain());
        }
    }

    public static bool getvan;

    IEnumerator Delayvankey()
    {
        getvan = false;
        yield return new WaitForSeconds(3);
        getvan = true;
    }

    public IEnumerator delayforAgain()
    {
        yield return new WaitForSeconds(0.25f);
        showbane++;
        Manageritem.mngitem.DeleteAllItemWendie();
        if (checkthedie > 2)
        {
            needkey = (int)(Mathf.Pow(2, checkthedie));
        }
        else if (checkthedie == 1)
        {
            needkey = 1;
        }
        else if (checkthedie == 2)
        {
            needkey = 2;
        }
        selectkey = false;
        Playermuving.player.Enterdieinfart();
        for (int i = 0; i < 100; i++)
        {
            yield return new WaitForSeconds(0.001f);
            if (selectkey == true)
            {
                if (managerdata.manager.getkey() >= needkey)
                {
                    Soundmanager.soundmanager.PlayAgain();
                    if (getvan)
                    {
                        StartCoroutine(Delayvankey());
                    }
                    alowcall = true;
                    Playermuving.player.StartCoroutine(Playermuving.player.EffectWenHavaeItem());
                    Playermuving.player.OurtCut();
                    Makesupway.makemap.StartCoroutine(Makesupway.makemap.MuvingbackAllemtyWenhaveitemVan(false));
                    Playermuving.player.backtodie();
                    managerdata.manager.savekey(-needkey);
                    emty.emtyplayer.ResutTranformemty();
                    Perencamera.managerscen.height = 3;
                    break;
                }
                else if (managerdata.manager.getkey() < needkey)
                {
                    selectkey = false;
                }
            }
            if (i == 99)
            {
                goodCPU.intance.GetStartrotay(false);
                Camerafolow.camfolowplayer.gameObject.GetComponent<Camera>().farClipPlane = 3;
                Playermuving.player.OurtCut();
                inthepanelpause.playagain = true;
                if (selectkey == false)
                {
                    Playermuving.isplay = false;
                    needkey = 0;
                    checkthedie = 0;
                    coinforuplv = 5000;
                    Playermuving.player.StartCoroutine(Playermuving.player.playagain(0));
                }
            }
        }
        yield return new WaitForSeconds(1);
        if (selectkey == false)
        {
            Perencamera.managerscen.StartCoroutine(Perencamera.managerscen.delayfolowcameradie());
        }
        alowcall = true;
    }

    int showbane = 0;

    /// <summary>
    /// chọn khóa hồi sinh
    /// </summary>
    public void ontheseleckey()
    {
        selectkey = true;
    }

    /// <summary>
    /// về menu chính
    /// </summary>
    public void gotohome()
    {
        Time.timeScale = 1;
        IkEmty.iklegth1 = 0;
        IkEmty.iklegth = 0;
        IKanimation.iklegth = 0;
        Playermuving.backnowmuvingship = true;
        Application.LoadLevel("mainlv");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleBackButton();
        }

        CheckTapToPlayInput();

        uiRefreshTimer += Time.deltaTime;
        if (uiRefreshTimer >= UI_REFRESH_INTERVAL)
        {
            uiRefreshTimer = 0f;

            int localCoins = PlayerPrefs.GetInt("Coins", 0);
            if (localCoins != lastLocalCoins)
            {
                lastLocalCoins = localCoins;
                UpdateCoinsDisplay(localCoins);
            }

            int highScore = PlayerPrefs.GetInt("HighScore", 0);
            if (highScore != lastHighScore)
            {
                lastHighScore = highScore;
                UpdateHighScoreDisplay(highScore);
            }
        }

        // During gameplay, also drive the legacy coinText/coinsText with RunCoins
        // in case the same text object is wired to UImanager instead of GameHUDController.
        if (canvasHUD != null && canvasHUD.activeInHierarchy)
        {
            string runCoinsStr = PlayerPrefs.GetInt("RunCoins", 0).ToString("N0");
            if (coinText != null) coinText.text = runCoinsStr;
            if (coinsText != null) coinsText.text = runCoinsStr;

            // Fallback: update the first TMP_Text under Canvas_HUD whose name contains "coin"
            if (coinText == null && coinsText == null)
            {
                TMP_Text[] texts = canvasHUD.GetComponentsInChildren<TMP_Text>(true);
                foreach (TMP_Text t in texts)
                {
                    if (t.gameObject.name.ToLowerInvariant().Contains("coin"))
                    {
                        t.text = runCoinsStr;
                        break;
                    }
                }
            }
        }

        if (Playermuving.player != null)
        {
            if (Playermuving.isplay)
            {
                if (Time.timeScale <= 1.1f)
                {
                    if (coinmuving > coinforuplv)
                    {
                        coinforuplv = coinforuplv * 2;
                        Time.timeScale += 0.05f;
                    }
                }
            }
        }
    }

    public static int coinforuplv = 5000;

    /// <summary>
    /// thoát game
    /// </summary>
    void OnApplicationPause()
    {
        if (Playermuving.isplay)
        {
            inthepanelpause.pauses.pause();
        }
    }

    public IEnumerator DelayForLogin()
    {
        yield return new WaitForSeconds(1);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    // Stubs for methods still referenced by other scripts (legacy UI removed)
    public IEnumerator delayslideritem(int value, string nameitem)
    {
        yield break;
    }

    public IEnumerator delayslideritemhut(int value)
    {
        yield break;
    }

    public IEnumerator delayslideritemx2(int value)
    {
        yield break;
    }

    public IEnumerator delayslideritemgiay(int value)
    {
        yield break;
    }

    public IEnumerator delayslideritembay(int value)
    {
        yield break;
    }

    public void ShowCharacterLost(bool value)
    {
    }

    #region Splash / Welcome / UserInfo Flow
    IEnumerator SplashFlow()
    {
        Debug.Log("[UImanager] Showing splash screen");

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

        float remainingTime = splashDisplayTime - logoFadeInDuration - logoDisplayTime - logoFadeOutDuration;
        if (remainingTime > 0f)
            yield return new WaitForSeconds(remainingTime);
    }

    IEnumerator WelcomeFlow()
    {
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

        Debug.Log("[UImanager] Showing welcome screen with loading bar");

        float elapsedLoading = 0f;
        while (elapsedLoading < loadingDuration)
        {
            elapsedLoading += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedLoading / loadingDuration);
            progress = Mathf.SmoothStep(0f, 1f, progress);

            if (welcomeLoadingBar != null)
                welcomeLoadingBar.SetProgress(progress);

            yield return null;
        }

        if (welcomeLoadingBar != null)
            welcomeLoadingBar.SetProgress(1f);

        yield return new WaitForSeconds(0.3f);
    }

    IEnumerator UserInfoFlow()
    {
        if (canvasWelcome != null)
            canvasWelcome.SetActive(false);

        if (canvasUserInfo != null)
            canvasUserInfo.SetActive(true);

        SetupUserInfoCanvasGroups();
        SetupUserInfoListeners();
        ShowUsernameStep();

        // Wait until user info is completed
        while (!IsUserInfoCompleted())
        {
            yield return null;
        }

        if (canvasUserInfo != null)
            canvasUserInfo.SetActive(false);
    }

    void SetupUserInfoCanvasGroups()
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

    void SetupUserInfoListeners()
    {
        if (letsRunButtonSpinner != null)
            letsRunButtonSpinner.SetActive(false);
        if (letsRunButtonText != null)
            letsRunButtonText.SetActive(true);

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnUserInfoNextPressed);
        }

        if (letsRunButton != null)
        {
            letsRunButton.onClick.RemoveAllListeners();
            letsRunButton.onClick.AddListener(OnUserInfoLetsRunPressed);
            letsRunButton.interactable = false;
        }

        for (int i = 0; i < ageButtons.Length; i++)
        {
            int index = i;
            if (ageButtons[i] != null)
            {
                ageButtons[i].onClick.RemoveAllListeners();
                ageButtons[i].onClick.AddListener(() => OnUserInfoAgeSelected(index));
            }
        }

        if (usernameInput != null)
        {
            usernameInput.onValueChanged.RemoveAllListeners();
            usernameInput.onValueChanged.AddListener(OnUserInfoUsernameChanged);
            usernameInput.shouldHideMobileInput = true;
        }

        if (usernameExistWarning == null && usernamePanel != null)
        {
            usernameExistWarning = usernamePanel.transform.Find("UsernameExsitwarning")?.gameObject;
            if (usernameExistWarning == null)
                usernameExistWarning = usernamePanel.transform.Find("UsernameExistWarning")?.gameObject;
        }
        if (usernameExistWarning != null)
            usernameExistWarning.SetActive(false);

        nextButtonShown = false;
        if (nextButton != null)
            nextButton.gameObject.SetActive(false);
    }

    void ShowUsernameStep()
    {
        if (ageRangePanel != null)
            ageRangePanel.SetActive(false);

        if (usernamePanel != null)
        {
            usernamePanel.SetActive(true);
            StartCoroutine(AnimateUserInfoPanelIn(usernamePanel, usernamePanelCanvasGroup, userInfoPanelEntryDelay));
        }

        Debug.Log("[UImanager] Showing username step");
    }

    IEnumerator AnimateUserInfoPanelIn(GameObject panel, CanvasGroup cg, float delay = 0f)
    {
        if (panel == null) yield break;

        RectTransform rt = panel.GetComponent<RectTransform>();
        Vector3 targetScale = rt != null ? rt.localScale : Vector3.one;
        Vector3 startScale = targetScale * userInfoPanelStartScale;

        if (cg != null) cg.alpha = 0f;
        if (rt != null) rt.localScale = startScale;

        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        while (elapsed < userInfoPanelFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / userInfoPanelFadeInDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            if (cg != null) cg.alpha = eased;
            if (rt != null) rt.localScale = Vector3.Lerp(startScale, targetScale, eased);

            yield return null;
        }

        if (cg != null) cg.alpha = 1f;
        if (rt != null) rt.localScale = targetScale;
    }

    void OnUserInfoUsernameChanged(string value)
    {
        bool hasText = !string.IsNullOrWhiteSpace(value);
        bool isValid = hasText && value.Length >= 3;

        if (nextButton == null) return;

        if (hasText && !nextButtonShown)
        {
            nextButtonShown = true;
            StartCoroutine(FadeInUserInfoUI(nextButton.gameObject, userInfoButtonFadeInDuration));
        }
        else if (!hasText && nextButtonShown)
        {
            nextButtonShown = false;
            nextButton.gameObject.SetActive(false);
        }

        nextButton.interactable = isValid;

        if (checkUsernameCoroutine != null)
            StopCoroutine(checkUsernameCoroutine);

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

    IEnumerator CheckUsernameExistsCoroutine(string username)
    {
        yield return new WaitForSeconds(0.3f);
        bool exists = false;

        if (UGSManager.Instance != null && UGSManager.Instance.IsSignedIn)
        {
            var task = UGSManager.Instance.GetScoresAsync("wiki_cat_rush");
            while (!task.IsCompleted)
                yield return null;

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
            var tmp = usernameExistWarning.GetComponentInChildren<TMP_Text>();
            if (tmp != null)
            {
                if (exists)
                {
                    tmp.text = "Username already taken";
                    tmp.color = new Color(0.9f, 0.2f, 0.2f);
                }
                else
                {
                    tmp.text = "Username is available";
                    tmp.color = new Color(0.2f, 0.8f, 0.3f);
                }
            }
        }

        nextButton.interactable = !exists;
    }

    IEnumerator FadeInUserInfoUI(GameObject target, float duration, float startDelay = 0f)
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

    public void OnUserInfoNextPressed()
    {
        if (usernameInput == null || string.IsNullOrWhiteSpace(usernameInput.text))
        {
            Debug.LogWarning("[UImanager] Username is empty");
            return;
        }

        string username = usernameInput.text.Trim();
        PlayerPrefs.SetString("user_display_name", username);
        PlayerPrefs.Save();
        Debug.Log($"[UImanager] Username saved: {username}");

        if (UGSManager.Instance != null)
        {
            _ = UGSManager.Instance.UpdatePlayerNameAsync(username);
        }

        TriggerUserInfoCatWink();
        StartCoroutine(TransitionToAgeStep());
    }

    IEnumerator TransitionToAgeStep()
    {
        if (usernamePanel != null)
            usernamePanel.SetActive(false);

        if (ageRangePanel != null)
        {
            ageRangePanel.SetActive(true);
            StartCoroutine(AnimateUserInfoPanelIn(ageRangePanel, ageRangePanelCanvasGroup, 0f));
        }

        if (letsRunButton != null)
            letsRunButton.gameObject.SetActive(false);

        for (int i = 0; i < ageButtons.Length; i++)
        {
            if (ageButtons[i] != null)
            {
                StartCoroutine(FadeInUserInfoUI(
                    ageButtons[i].gameObject,
                    userInfoButtonFadeInDuration,
                    i * userInfoAgeButtonStaggerDelay
                ));
            }
        }

        Debug.Log("[UImanager] Showing age range step");
        yield break;
    }

    public void OnUserInfoAgeSelected(int index)
    {
        if (index < 0 || index >= ageRanges.Length) return;

        selectedAgeIndex = index;
        selectedAgeRange = ageRanges[index];

        for (int i = 0; i < ageButtons.Length; i++)
        {
            if (ageButtons[i] == null) continue;

            Image buttonImage = ageButtons[i].GetComponent<Image>();
            if (i == index)
            {
                if (buttonImage != null && selectedButtonSprite != null)
                    buttonImage.sprite = selectedButtonSprite;
            }
            else
            {
                if (buttonImage != null && normalButtonSprite != null)
                    buttonImage.sprite = normalButtonSprite;
            }
        }

        if (letsRunButton != null)
        {
            if (!letsRunButton.gameObject.activeSelf)
                StartCoroutine(FadeInUserInfoUI(letsRunButton.gameObject, userInfoButtonFadeInDuration));
            letsRunButton.interactable = true;
        }

        Debug.Log($"[UImanager] Age range selected: {selectedAgeRange}");
    }

    public void OnUserInfoLetsRunPressed()
    {
        if (string.IsNullOrEmpty(selectedAgeRange))
        {
            Debug.LogWarning("[UImanager] No age range selected");
            return;
        }

        PlayerPrefs.SetString("user_age_range", selectedAgeRange);
        PlayerPrefs.SetInt("user_info_completed", 1);
        PlayerPrefs.Save();
        Debug.Log("[UImanager] User info saved. Proceeding to main menu.");

        if (letsRunButtonText != null)
            letsRunButtonText.SetActive(false);
        if (letsRunButtonSpinner != null)
            letsRunButtonSpinner.SetActive(true);

        if (letsRunButton != null)
            letsRunButton.interactable = false;

        TriggerUserInfoCatWink();
        StartCoroutine(FinishUserInfoAfterWink());
    }

    CatBlinker GetActiveUserInfoCatBlinker()
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

    void TriggerUserInfoCatWink()
    {
        if (GetActiveUserInfoCatBlinker() != null)
            StartCoroutine(UserInfoWinkAfterDelay());
    }

    IEnumerator UserInfoWinkAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);
        CatBlinker activeBlinker = GetActiveUserInfoCatBlinker();
        if (activeBlinker != null && activeBlinker.gameObject.activeInHierarchy)
            activeBlinker.TriggerWink();
    }

    IEnumerator FinishUserInfoAfterWink()
    {
        yield return new WaitForSeconds(0.6f);
        // UserInfoFlow coroutine will detect the flag and continue
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
    #endregion

    #region Home UI Methods (merged from HomeUIController)
    /// <summary>
    /// Finds the active player/runner GameObject. Prefers RunnerManager (multi-character setups),
    /// but falls back to searching the scene directly for a SimplePlayerController if RunnerManager
    /// isn't present/active - which is the case in scenes where the runner is placed directly.
    /// </summary>
    private GameObject FindRunnerObject()
    {
        if (RunnerManager.Instance != null)
        {
            GameObject fromManager = RunnerManager.Instance.GetCurrentRunnerInstance();
            if (fromManager != null) return fromManager;
        }

        SimplePlayerController directController = FindObjectOfType<SimplePlayerController>();
        if (directController != null) return directController.gameObject;

        GameObject byTag = GameObject.FindGameObjectWithTag("Player");
        return byTag;
    }

    private IEnumerator InitializePlayerPositionForHome()
    {
        // Wait a frame for RunnerManager (if present) to instantiate and select the active runner
        yield return null;

        GameObject runnerObj = FindRunnerObject();
        Debug.Log($"[UImanager] InitializePlayerPositionForHome: runnerObj={(runnerObj != null ? runnerObj.name : "NULL")}");

        if (runnerObj != null)
        {
            SimplePlayerController runnerController = runnerObj.GetComponent<SimplePlayerController>();
            Debug.Log($"[UImanager] InitializePlayerPositionForHome: runnerController={(runnerController != null)}, currentPos={runnerObj.transform.position}, activeInHierarchy={runnerObj.activeInHierarchy}");

            if (runnerController != null)
            {
                // Keep whatever ground height (Y) the runner already has - only patch Y so the
                // model stays on the surface. X/Z and rotation are read from SimplePlayerController
                // so the Home screen pose can be tuned in the Inspector.
                float groundY = runnerObj.transform.position.y;
                Vector3 idle = runnerController.roadsideIdlePosition;
                runnerController.roadsideIdlePosition = new Vector3(idle.x, groundY, idle.z);
                runnerController.SetRoadsideIdlePose();
                Debug.Log($"[UImanager] InitializePlayerPositionForHome: after SetRoadsideIdlePose, pos={runnerObj.transform.position}, rot={runnerObj.transform.eulerAngles}, enabled={runnerController.enabled}");
            }
            else
            {
                runnerObj.transform.position = new Vector3(0f, runnerObj.transform.position.y, 7.90f);
                runnerObj.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                Animator anim = runnerObj.GetComponentInChildren<Animator>();
                if (anim != null)
                {
                    anim.Play("Idle", 0, 0f);
                }
            }
        }
    }

    private void AutoDiscoverCanvases()
    {
        // Core canvases
        if (canvasHome == null)
        {
            GameObject go = GameObject.Find("Canvas_Home");
            if (go != null) canvasHome = go;
        }
        if (canvasHUD == null)
        {
            GameObject go = GameObject.Find("Canvas_HUD");
            if (go != null) canvasHUD = go;
        }
        if (canvasGame == null)
        {
            GameObject go = GameObject.Find("Canvas_Game");
            if (go != null) canvasGame = go;
        }

        // Gameplay panels
        if (failPanel == null)
        {
            GameObject go = GameObject.Find("Canvas_Fail");
            if (go == null) go = GameObject.Find("FailPanel");
            if (go != null) failPanel = go;
        }
        if (highScorePanel == null)
        {
            GameObject go = GameObject.Find("Canvas_Highscore");
            if (go == null) go = GameObject.Find("HighScorePanel");
            if (go != null) highScorePanel = go;
        }
        if (pausePanel == null)
        {
            GameObject go = GameObject.Find("Canvas_Pause");
            if (go == null) go = GameObject.Find("PausePanel");
            if (go != null) pausePanel = go;
        }

        // Sub-menu canvases
        if (canvasLeaderboard == null)
        {
            GameObject go = GameObject.Find("Canvas_Leaderboard");
            if (go != null) canvasLeaderboard = go;
        }
        if (leaderboardController == null && canvasLeaderboard != null)
            leaderboardController = canvasLeaderboard.GetComponent<LeaderboardController>();
        if (canvasCharacters == null)
        {
            GameObject go = GameObject.Find("Canvas_Characters");
            if (go != null) canvasCharacters = go;
        }
        if (canvasSettings == null)
        {
            GameObject go = GameObject.Find("Canvas_Settings");
            if (go != null) canvasSettings = go;
        }
        if (settingsController == null && canvasSettings != null)
            settingsController = canvasSettings.GetComponent<SettingsController>();

        if (canvasDailyGift == null)
        {
            GameObject go = GameObject.Find("Canvas_Daily_Gift");
            if (go != null) canvasDailyGift = go;
        }
        if (dailyGiftController == null && canvasDailyGift != null)
            dailyGiftController = canvasDailyGift.GetComponent<DailyGiftController>();

        if (canvasMissions == null)
        {
            GameObject go = GameObject.Find("Canvas_Missions");
            if (go != null) canvasMissions = go;
        }
        if (missionsController == null && canvasMissions != null)
            missionsController = canvasMissions.GetComponent<MissionsController>();

        if (canvasShop == null)
        {
            GameObject go = GameObject.Find("Canvas_Shop");
            if (go != null) canvasShop = go;
        }
        if (shopController == null && canvasShop != null)
            shopController = canvasShop.GetComponent<ShopController>();

        if (canvasLevel == null)
        {
            GameObject go = GameObject.Find("Canvas_Level");
            if (go != null) canvasLevel = go;
        }
    }

    private void HandleBackButton()
    {
        // While gameplay or pause is active, handle pause/resume directly.
        bool inGameplay = (canvasHUD != null && canvasHUD.activeInHierarchy) ||
                          (pausePanel != null && pausePanel.activeInHierarchy);
        if (inGameplay)
        {
            HandleGameBackButton();
            return;
        }

        // Close any open sub-menu and return to Canvas_Home.
        if (canvasLevel != null && canvasLevel.activeInHierarchy)
        {
            HideLevelCanvas();
            return;
        }
        if ((dailyGiftController != null && dailyGiftController.gameObject.activeInHierarchy) ||
            (canvasDailyGift != null && canvasDailyGift.activeInHierarchy))
        {
            HideDailyGift();
            return;
        }
        if ((missionsController != null && missionsController.gameObject.activeInHierarchy) ||
            (canvasMissions != null && canvasMissions.activeInHierarchy))
        {
            HideMissions();
            return;
        }
        if ((charactersController != null && charactersController.gameObject.activeInHierarchy) ||
            (canvasCharacters != null && canvasCharacters.activeInHierarchy))
        {
            HideCharacters();
            return;
        }
        if (canvasSettings != null && canvasSettings.activeInHierarchy)
        {
            HideSettings();
            return;
        }
        if ((shopController != null && shopController.gameObject.activeInHierarchy) ||
            (canvasShop != null && canvasShop.activeInHierarchy))
        {
            HideShop();
            return;
        }
        if (canvasLeaderboard != null && canvasLeaderboard.activeInHierarchy)
        {
            HideLeaderboard();
            return;
        }
        if (canvasCoins != null && canvasCoins.activeInHierarchy)
        {
            canvasCoins.SetActive(false);
            if (canvasHome != null) canvasHome.SetActive(true);
            return;
        }
        if (canvasPowerUps != null && canvasPowerUps.activeInHierarchy)
        {
            canvasPowerUps.SetActive(false);
            if (canvasHome != null) canvasHome.SetActive(true);
            return;
        }
        if (canvasBoards != null && canvasBoards.activeInHierarchy)
        {
            canvasBoards.SetActive(false);
            if (canvasHome != null) canvasHome.SetActive(true);
            return;
        }

        // Already on Canvas_Home with nothing open: quit.
        if (canvasHome != null && canvasHome.activeInHierarchy)
        {
            Debug.Log("[HomeUI] Back button pressed - quitting game");
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    private void UpdateCoinsDisplay(int coins)
    {
        string coinsFormatted = coins.ToString("N0");

        if (coinsText != null)
            coinsText.text = coinsFormatted;
        if (localCoinsText != null)
            localCoinsText.text = coinsFormatted;
        if (coinText != null)
            coinText.text = coinsFormatted;
    }

    private void UpdateHighScoreDisplay(int score)
    {
        if (highScoreText != null)
            highScoreText.text = score.ToString("N0");
    }

    private void UpdateUI()
    {
        int localCoins = PlayerPrefs.GetInt("Coins", 0);
        lastLocalCoins = localCoins;
        UpdateCoinsDisplay(localCoins);

        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        lastHighScore = highScore;
        UpdateHighScoreDisplay(highScore);

        // Update username
        if (usernameText != null)
        {
            string username = PlayerPrefs.GetString("user_display_name", "PLAYER");
            usernameText.text = username.ToUpper();
        }

        // Update energy
        if (energyText != null)
        {
            int energy = PlayerPrefs.GetInt("Energy", 5);
            energyText.text = energy.ToString();
        }

        // Update user session data (legacy)
        if (UserSession.IsLoggedIn)
        {
            if (langText != null)
            {
                string language = UserSession.SelectedLanguage;
                langText.text = language;
            }
        }
        else
        {
            if (langText != null)
                langText.text = "guest";
        }

        if (learnToPlayText != null)
        {
            string learnLang = PlayerPrefs.GetString("user_selected_language", "");
            if (!string.IsNullOrEmpty(learnLang))
            {
                learnToPlayText.richText = true;
                string hex = ColorUtility.ToHtmlStringRGB(learnToPlayColor);
                learnToPlayText.text = "Learn <color=#" + hex + ">" + char.ToUpperInvariant(learnLang[0]) + learnLang.Substring(1) + "</color> as you run";
            }
        }

        // Update badge visibility
        UpdateBadges();
    }

    private void UpdateBadges()
    {
        // Daily gift badge - show if unclaimed today
        if (dailyGiftBadge != null)
        {
            string lastClaim = PlayerPrefs.GetString("LastDailyGiftClaim", "");
            bool canClaim = string.IsNullOrEmpty(lastClaim) ||
                           lastClaim != System.DateTime.Today.ToString("yyyy-MM-dd");
            dailyGiftBadge.SetActive(canClaim);
        }

        // Missions badge - show if there are incomplete missions
        if (missionsBadge != null)
        {
            int incompleteMissions = PlayerPrefs.GetInt("IncompleteMissions", 1);
            missionsBadge.SetActive(incompleteMissions > 0);
        }

        // Shop badge - can be used for new items notification
        if (shopBadge != null)
        {
            bool hasNewItems = PlayerPrefs.GetInt("ShopNewItems", 0) > 0;
            shopBadge.SetActive(hasNewItems);
        }
    }

    public void RefreshUI()
    {
        UpdateUI();
    }

    public void OnTapToPlay()
    {
        if (isTransitioning) return;

        // Show the level-select canvas first. The run only starts after a level is picked.
        if (canvasLevel != null)
        {
            if (canvasLevel.activeInHierarchy || AnySubMenuOpen()) return;

            ShowLevelSelect();
            return;
        }

        StartRunTransition();
    }

    private void StartRunTransition()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        Debug.Log("[HomeUI] Transitioning seamlessly in the SAME scene!");
        StartCoroutine(SeamlessInSceneTransitionCoroutine());
    }

    /// <summary>Resets the transition flag. Called when gameplay ends (complete/fail/home)
    /// so a transition that was interrupted mid-flight can't lock out the next run.</summary>
    public void ResetTransitionFlag()
    {
        isTransitioning = false;
    }

    /// <summary>
    /// Resets gameplay in the background: hides gameplay panels, stops the run state,
    /// resets the camera, re-enables menu vehicles and puts the runner back into its
    /// roadside idle pose - WITHOUT showing the home canvas. Used by Replay/Next on
    /// Canvas_CompleteLevel before starting a level through the normal run flow.
    /// </summary>
    public void ResetRunnerToHomePose()
    {
        isPaused = false;
        isFailed = false;
        isTransitioning = false;
        isCountingDown = false;

        // Hide gameplay panels
        if (pausePanel) pausePanel.SetActive(false);
        if (failPanel) failPanel.SetActive(false);
        if (highScorePanel) highScorePanel.SetActive(false);
        if (canvasHUD) canvasHUD.SetActive(false);
        if (canvasGame) canvasGame.SetActive(false);

        // Stop gameplay state
        if (GameStateController.Instance != null)
            GameStateController.Instance.SetPlaying(false);

        // Reset camera - glide back to the home framing instead of snapping
        CameraFollowRunner camFollow = Object.FindFirstObjectByType<CameraFollowRunner>();
        if (camFollow != null)
        {
            camFollow.forceFollow = false;
            if (cameraResetRoutine != null) StopCoroutine(cameraResetRoutine);
            cameraResetRoutine = StartCoroutine(SmoothCameraReturn(camFollow, 0.6f));
        }

        // Destroy spawned road tiles and rewind the spawn cursor so the
        // next run builds the road fresh from the starter tile
        RoadSpawner roadSpawner = Object.FindFirstObjectByType<RoadSpawner>();
        if (roadSpawner != null)
            roadSpawner.ResetSpawner();

        // Runner back to roadside idle pose
        GameObject runnerObj = FindRunnerObject();
        if (runnerObj != null)
        {
            SimplePlayerController runnerController = runnerObj.GetComponent<SimplePlayerController>();
            if (runnerController != null)
            {
                runnerController.enabled = true;
                runnerController.SetRoadsideIdlePose();
            }
        }

        SetMenuVehiclesActive(true);
    }

    private Coroutine cameraResetRoutine;

    /// <summary>Glides the camera back to its home pose over `duration` seconds.
    /// Bails early if a run transition takes over (forceFollow / playing), so the two
    /// don't fight over the camera.</summary>
    private IEnumerator SmoothCameraReturn(CameraFollowRunner cam, float duration)
    {
        Vector3 fromPos = cam.transform.position;
        Quaternion fromRot = cam.transform.rotation;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (cam.forceFollow || (GameStateController.Instance != null && GameStateController.Instance.IsPlaying()))
                yield break;
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float tSmooth = t * t * (3f - 2f * t);
            cam.transform.position = Vector3.Lerp(fromPos, cam.HomePosition, tSmooth);
            cam.transform.rotation = Quaternion.Slerp(fromRot, cam.HomeRotation, tSmooth);
            yield return null;
        }
        cam.ResetCamera();
        cameraResetRoutine = null;
    }

    public void ShowLevelSelect()
    {
        if (canvasLevel == null) return;

        canvasLevel.SetActive(true);
        Debug.Log("[HomeUI] Level select canvas shown");
    }

    public void HideLevelCanvas()
    {
        if (canvasLevel != null) canvasLevel.SetActive(false);
        Debug.Log("[HomeUI] Level select canvas closed");
    }

    /// <summary>Called by a level node on Canvas_Level - selects the level and starts the run.</summary>
    public void StartLevelAndRun(int levelNumber)
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.SetSelectedLevel(levelNumber);

        if (canvasLevel != null) canvasLevel.SetActive(false);

        Debug.Log($"[HomeUI] Level {levelNumber} selected -> starting run");
        StartRunTransition();
    }

    private bool AnySubMenuOpen()
    {
        return (canvasDailyGift != null && canvasDailyGift.activeInHierarchy) ||
               (canvasMissions != null && canvasMissions.activeInHierarchy) ||
               (canvasShop != null && canvasShop.activeInHierarchy) ||
               (canvasSettings != null && canvasSettings.activeInHierarchy) ||
               (canvasLeaderboard != null && canvasLeaderboard.activeInHierarchy) ||
               (canvasCharacters != null && canvasCharacters.activeInHierarchy) ||
               (canvasCoins != null && canvasCoins.activeInHierarchy) ||
               (canvasPowerUps != null && canvasPowerUps.activeInHierarchy) ||
               (canvasBoards != null && canvasBoards.activeInHierarchy);
    }

    private IEnumerator SeamlessInSceneTransitionCoroutine()
    {
        // Switch to gameplay music instantly to start the mood change
        if (BackgroundMusicManager.Instance != null)
        {
            BackgroundMusicManager.Instance.PlayGameplayMusic();
        }

        // Deactivate static menu vehicles so they don't block the running lanes
        SetMenuVehiclesActive(false);

        Camera characterPreviewCamera = GameObject.Find("CharacterPreviewCamera")?.GetComponent<Camera>();
        if (characterPreviewCamera != null)
            characterPreviewCamera.enabled = false;

        // Close any sub-menus
        if (canvasDailyGift != null) canvasDailyGift.SetActive(false);
        if (canvasMissions != null) canvasMissions.SetActive(false);
        if (canvasShop != null) canvasShop.SetActive(false);
        if (canvasSettings != null) canvasSettings.SetActive(false);
        if (canvasLeaderboard != null) canvasLeaderboard.SetActive(false);
        if (canvasCharacters != null) canvasCharacters.SetActive(false);
        if (canvasLevel != null) canvasLevel.SetActive(false);

        // Hide currency panels
        if (canvasCoins != null) canvasCoins.SetActive(false);

        // Fade out Canvas_Home
        if (canvasHome != null)
        {
            CanvasGroup cg = canvasHome.GetComponent<CanvasGroup>();
            if (cg == null) cg = canvasHome.AddComponent<CanvasGroup>();

            float elapsed = 0f;
            while (elapsed < 0.25f)
            {
                elapsed += Time.unscaledDeltaTime;
                cg.alpha = Mathf.Clamp01(1f - (elapsed / 0.25f));
                yield return null;
            }
            cg.alpha = 0f;
            canvasHome.SetActive(false);
        }

        // Smoothly rotate and move player runner from Home screen position to Gameplay starting position
        GameObject runnerObj = FindRunnerObject();
        SimplePlayerController runnerController = runnerObj != null ? runnerObj.GetComponent<SimplePlayerController>() : null;

        // Let the camera glide with the runner during the run-in intro instead of
        // snapping in at the end when playing starts
        CameraFollowRunner camFollow = Object.FindFirstObjectByType<CameraFollowRunner>();
        if (camFollow != null && camFollow.target != null)
            camFollow.forceFollow = true;
        Debug.Log($"[UImanager] SeamlessInSceneTransitionCoroutine: runnerObj={(runnerObj != null ? runnerObj.name : "NULL")}, runnerController={(runnerController != null)}");

        if (runnerController != null)
        {
            // Keep whatever ground height (Y) the runner already has for both the start and
            // end of the intro. X/Z and rotation are read from SimplePlayerController so they can
            // be tuned in the Inspector; only Y is patched to keep the model on the surface.
            float groundY = runnerObj.transform.position.y;
            Vector3 idle = runnerController.roadsideIdlePosition;
            Vector3 start = runnerController.roadStartPosition;
            runnerController.roadsideIdlePosition = new Vector3(idle.x, groundY, idle.z);
            runnerController.roadStartPosition = new Vector3(start.x, groundY, start.z);
            yield return StartCoroutine(runnerController.BeginRoadsideRun());
        }
        else if (runnerObj != null)
        {
            // Fallback for runners without a SimplePlayerController: animate manually.
            float groundY2 = runnerObj.transform.position.y;
            runnerObj.transform.position = new Vector3(0f, groundY2, 7.90f);
            runnerObj.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            Animator runnerAnim = runnerObj.GetComponentInChildren<Animator>();
            if (runnerAnim != null) runnerAnim.Play("Jump", 0, 0f);

            float turnElapsed = 0f;
            float turnDuration = 0.8f;
            float jumpHeight = 1.6f;
            Vector3 startPos = new Vector3(0f, groundY2, 7.90f);
            Vector3 endPos = new Vector3(0f, groundY2, 6.50f);
            Quaternion startRot = Quaternion.Euler(0f, 180f, 0f);
            Quaternion targetRot = Quaternion.identity;

            while (turnElapsed < turnDuration)
            {
                turnElapsed += Time.deltaTime;
                float t = turnElapsed / turnDuration;
                float tSmooth = t * t * (3f - 2f * t);

                if (runnerObj != null)
                {
                    runnerObj.transform.rotation = Quaternion.Slerp(startRot, targetRot, tSmooth);
                    Vector3 currentPos = Vector3.Lerp(startPos, endPos, tSmooth);
                    float arcY = 4f * jumpHeight * t * (1f - t);
                    currentPos.y += arcY;
                    runnerObj.transform.position = currentPos;
                }
                yield return null;
            }

            // Snap cleanly to final coordinates on land
            if (runnerObj != null)
            {
                runnerObj.transform.rotation = targetRot;
                runnerObj.transform.position = endPos;

                if (runnerAnim != null)
                {
                    runnerAnim.Play("Run", 0, 0f);
                    if (HasAnimatorParameter(runnerAnim, "ForwardSpeed"))
                        runnerAnim.SetFloat("ForwardSpeed", 1f);
                }
            }
        }

        // Mark the new state system as playing so road spawners, camera follow, and
        // distance tracking (which all check GameStateController.IsPlaying()) start moving.
        if (GameStateController.Instance != null)
        {
            GameStateController.Instance.SetPlaying(true);
        }

        // Playing now drives the follow gate - release the intro override
        if (camFollow != null) camFollow.forceFollow = false;

        // Also run the legacy play() flow for backward compatibility with any
        // remaining Playermuving-based systems still active in the scene.
        if (canvasHUD != null) canvasHUD.SetActive(true);

        // Start the word-collection round for the selected level (GameSceneController
        // does this in the full build; this scene is driven by UImanager instead).
        if (WordManager.Instance != null)
        {
            WordManager.Instance.ResetForNewLevel();
            WordManager.Instance.StartRound();
        }

        play();
        isTransitioning = false;
    }

    /// <summary>Helper to check if an Animator has a given parameter.</summary>
    private static bool HasAnimatorParameter(Animator anim, string paramName)
    {
        foreach (var p in anim.parameters)
        {
            if (p.name == paramName) return true;
        }
        return false;
    }

    public void OnCharactersPressed()
    {
        Debug.Log("[HomeUI] Characters pressed");
        ShowCharacters();
    }

    private void ShowCharacters()
    {
        if (charactersController != null)
        {
            charactersController.gameObject.SetActive(true);
        }
        else if (canvasCharacters != null)
        {
            canvasCharacters.SetActive(true);
        }
        if (canvasHome != null) canvasHome.SetActive(false);
    }

    public void HideCharacters()
    {
        // Deactivate whichever object was actually shown (mirror of ShowCharacters)
        if (charactersController != null)
            charactersController.gameObject.SetActive(false);
        if (canvasCharacters != null)
            canvasCharacters.SetActive(false);
        if (canvasHome != null) canvasHome.SetActive(true);

        // Reset position and rotation of the newly equipped runner to the Home screen start position
        GameObject runnerObj = FindRunnerObject();
        if (runnerObj != null)
        {
            SimplePlayerController runnerController = runnerObj.GetComponent<SimplePlayerController>();
            if (runnerController != null)
            {
                // Use the X/Z and rotation already configured on SimplePlayerController; only patch
                // Y to whatever surface the runner is currently above.
                float groundY = runnerObj.transform.position.y;
                Vector3 idle = runnerController.roadsideIdlePosition;
                runnerController.roadsideIdlePosition = new Vector3(idle.x, groundY, idle.z);
                runnerController.SetRoadsideIdlePose();
            }
            else
            {
                runnerObj.transform.position = new Vector3(0f, runnerObj.transform.position.y, 7.90f);
                runnerObj.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                Animator anim = runnerObj.GetComponentInChildren<Animator>();
                if (anim != null)
                {
                    anim.Play("Idle", 0, 0f);
                }
            }
        }

        Debug.Log("[HomeUI] HideCharacters -> returned to Home and reset runner position");
    }

    public void OnHomePressed()
    {
        OnTapToPlay();
    }

    public void OnSettingsPressed()
    {
        Debug.Log("[HomeUI] Settings pressed");
        ShowSettings();
    }

    private void ShowSettings()
    {
        if (canvasSettings != null)
        {
            canvasSettings.SetActive(true);
            Debug.Log("[HomeUI] Canvas_Settings shown.");
        }
        else
        {
            Debug.LogWarning("[HomeUI] canvasSettings is null - drag Canvas_Settings into UImanager inspector.");
        }

        if (canvasHome != null) canvasHome.SetActive(false);
    }

    public void HideSettings()
    {
        if (canvasSettings != null) canvasSettings.SetActive(false);
        if (canvasHome != null) canvasHome.SetActive(true);
    }

    public void OnDailyGiftPressed()
    {
        Debug.Log("[HomeUI] Daily Gift pressed");
        ShowDailyGift();
    }

    private void ShowDailyGift()
    {
        if (dailyGiftController != null)
        {
            dailyGiftController.Show();
        }
        else if (canvasDailyGift != null)
        {
            canvasDailyGift.SetActive(true);
        }
        if (canvasHome != null) canvasHome.SetActive(false);
    }

    public void HideDailyGift()
    {
        if (dailyGiftController != null)
            dailyGiftController.Hide();
        if (canvasDailyGift != null)
            canvasDailyGift.SetActive(false);
        if (canvasHome != null)
            canvasHome.SetActive(true);
        RefreshUI();
    }

    public void OnMissionsPressed()
    {
        Debug.Log("[HomeUI] Missions pressed");
        ShowMissions();
    }

    private void ShowMissions()
    {
        if (missionsController != null)
        {
            missionsController.Show();
        }
        else if (canvasMissions != null)
        {
            canvasMissions.SetActive(true);
        }
        if (canvasHome != null) canvasHome.SetActive(false);
    }

    public void HideMissions()
    {
        if (missionsController != null)
            missionsController.Hide();
        else if (canvasMissions != null)
            canvasMissions.SetActive(false);
        if (canvasHome != null)
            canvasHome.SetActive(true);
        RefreshUI();
    }

    public void OnShopPressed()
    {
        Debug.Log("[HomeUI] Shop pressed");
        ShowShop();
    }

    private void ShowShop()
    {
        if (shopController != null)
        {
            shopController.Show();
        }
        else if (canvasShop != null)
        {
            canvasShop.SetActive(true);
        }
        if (canvasHome != null) canvasHome.SetActive(false);
    }

    public void HideShop()
    {
        if (shopController != null)
            shopController.Hide();
        else if (canvasShop != null)
            canvasShop.SetActive(false);
        if (canvasHome != null)
            canvasHome.SetActive(true);
    }

    public void OnLeaderboardPressed()
    {
        Debug.Log("[HomeUI] Leaderboard pressed");
        ShowLeaderboard();
    }

    private void ShowLeaderboard()
    {
        if (canvasLeaderboard != null)
            canvasLeaderboard.SetActive(true);
        if (canvasHome != null)
            canvasHome.SetActive(false);

        if (leaderboardController != null)
            leaderboardController.LoadLeaderboard();
    }

    public void HideLeaderboard()
    {
        if (canvasLeaderboard != null) canvasLeaderboard.SetActive(false);
        if (canvasHome != null) canvasHome.SetActive(true);
    }

    public void OnCoinsAddPressed()
    {
        Debug.Log("[HomeUI] Add Coins pressed");
        if (shopController != null)
        {
            shopController.ShowCoins();
        }
        else if (canvasCoins != null)
        {
            canvasCoins.SetActive(true);
        }
        if (canvasHome != null) canvasHome.SetActive(false);
    }

    public void OnEnergyAddPressed()
    {
        Debug.Log("[HomeUI] Add Energy pressed");
        // Open energy purchase panel
    }

    public void SetMenuVehiclesActive(bool active)
    {
        string[] vehicleNames = { "danfo", "brt bus", "compact car" };
        foreach (string name in vehicleNames)
        {
            GameObject rootObj = FindRootObjectByName(name);
            if (rootObj != null)
            {
                rootObj.SetActive(active);
                Debug.Log($"[UImanager] Set vehicle '{name}' active state to: {active}");
            }
        }
    }

    private GameObject FindRootObjectByName(string name)
    {
        var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var roots = activeScene.GetRootGameObjects();
        foreach (var r in roots)
        {
            if (r.name == name)
            {
                return r;
            }
        }
        return null;
    }

    public void SetHomeScreenVisible(bool visible)
    {
        if (canvasHome != null) canvasHome.SetActive(visible);
        if (canvasCoins != null) canvasCoins.SetActive(visible);
    }

    public CanvasGroup GetHomeCanvasGroup()
    {
        return canvasHome != null ? canvasHome.GetComponent<CanvasGroup>() : null;
    }
    #endregion

    #region Game Scene Controller Methods (merged from GameSceneController)

    /// <summary>
    /// Backward-compatible entry point for Inspector buttons. Uses the legacy play() flow.
    /// </summary>
    public void StartGameFromSingleScene()
    {
        play();
    }

    private void StartGameImmediately()
    {
        if (GameStateController.Instance != null)
        {
            GameStateController.Instance.SetPlaying(true);
        }

        Time.timeScale = 1f;

        Debug.Log("[UImanager] Game started immediately (no countdown)");
        OnLevelReady?.Invoke();
    }

    // -------------------------
    // COUNTDOWN (for resume only)
    // -------------------------
    private IEnumerator CountdownAndResume()
    {
        isCountingDown = true;
        Time.timeScale = 0f;

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);

            countdownText.text = "3";
            countdownText.fontSize = 80;
            yield return new WaitForSecondsRealtime(1f);

            countdownText.text = "2";
            yield return new WaitForSecondsRealtime(1f);

            countdownText.text = "1";
            yield return new WaitForSecondsRealtime(1f);

            countdownText.text = "GO!";
            countdownText.fontSize = 100;
            yield return new WaitForSecondsRealtime(0.5f);

            countdownText.gameObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSecondsRealtime(1f);
        }

        Time.timeScale = 1f;

        if (GameStateController.Instance != null)
            GameStateController.Instance.SetPlaying(true);

        isCountingDown = false;
    }

    // -------------------------
    // PAUSE
    // -------------------------
    public void Pause()
    {
        if (isPaused || isFailed) return;
        isPaused = true;

        if (canvasGame) canvasGame.SetActive(false);
        if (canvasHUD) canvasHUD.SetActive(false);

        if (pausePanel) pausePanel.SetActive(true);

        if (GameStateController.Instance != null)
            GameStateController.Instance.SetPlaying(false);

        Time.timeScale = 0f;
    }

    public void Resume()
    {
        if (!isPaused || isFailed) return;
        isPaused = false;

        if (pausePanel) pausePanel.SetActive(false);

        if (canvasGame) canvasGame.SetActive(true);
        if (canvasHUD) canvasHUD.SetActive(true);

        StartCoroutine(CountdownAndResume());
    }

    // -------------------------
    // FAIL
    // -------------------------
    public void Fail()
    {
        if (isFailed) return;
        isFailed = true;
        isPaused = false;

        Debug.Log($"[UImanager] Fail() called. failPanel={failPanel != null}, highScorePanel={highScorePanel != null}");

        if (GameStateController.Instance != null)
            GameStateController.Instance.SetPlaying(false);

        if (pausePanel) pausePanel.SetActive(false);

        StartCoroutine(FailSequenceCoroutine());
    }

    private IEnumerator FailSequenceCoroutine()
    {
        // Wait for fall/knockback animation to play out before showing the UI
        yield return new WaitForSeconds(failSequenceDelay);

        // Safely check if a panel is nested inside a canvas before deactivating it.
        bool CanvasContains(Transform canvasTr, GameObject panel)
        {
            if (ReferenceEquals(canvasTr, null) || ReferenceEquals(panel, null)) return false;
            if (canvasTr == null || panel == null) return false;
            return panel.transform.IsChildOf(canvasTr);
        }

        if (canvasGame != null)
        {
            bool canvasHasPanel = CanvasContains(canvasGame.transform, failPanel) || CanvasContains(canvasGame.transform, highScorePanel);
            if (!canvasHasPanel)
                canvasGame.SetActive(false);
        }

        if (canvasHUD != null)
        {
            bool hudHasPanel = CanvasContains(canvasHUD.transform, failPanel) || CanvasContains(canvasHUD.transform, highScorePanel);
            if (!hudHasPanel)
                canvasHUD.SetActive(false);
        }

        // Show fail or high score panel
        ShowFailOrHighScorePanel();

        // Freeze game simulation
        Time.timeScale = 0f;
    }

    public float GetRunDistance()
    {
        if (SimplePlayerController.Instance != null)
            return SimplePlayerController.Instance.GetCurrentDistance();

        GameObject runnerObj = FindRunnerObject();
        if (runnerObj != null)
        {
            var sp = runnerObj.GetComponent<SimplePlayerController>();
            if (sp != null)
                return sp.GetCurrentDistance();
            return Mathf.Max(0f, runnerObj.transform.position.z - 6.5f);
        }

        // Use legacy Playermuving distance if available
        if (Playermuving.player != null)
            return Playermuving.player.transform.position.z;

        return 0f;
    }

    private void ShowFailOrHighScorePanel()
    {
        Debug.Log("[UImanager] Showing fail/high score panel");

        // Get run stats
        float distance = GetRunDistance();
        int score = Mathf.FloorToInt(distance);
        int coinsEarned = PlayerPrefs.GetInt("RunCoins", 0);

        // Progress missions for distance and score
        MissionsController.ProgressMission(MissionData.MissionType.RunMeters, Mathf.FloorToInt(distance));
        MissionsController.ProgressMission(MissionData.MissionType.ScorePoints, score);

        // Submit score to UGS Leaderboards in the background
        if (UGSManager.Instance != null)
        {
            _ = UGSManager.Instance.SubmitScoreAsync("wiki_cat_rush", score);
        }

        // Check for high score first
        bool isHighScore = false;
        if (highScorePanel != null)
        {
            var hsController = highScorePanel.GetComponent<HighScorePanelController>();
            if (hsController != null)
            {
                isHighScore = hsController.CheckAndShow(score);
                if (isHighScore)
                {
                    highScorePanel.SetActive(true);
                    Debug.Log("[UImanager] High score panel activated.");
                    if (BackgroundMusicManager.Instance != null)
                    {
                        BackgroundMusicManager.Instance.PlayHighScoreMusic();
                    }
                }
            }
        }

        // If not a high score, show the normal fail panel
        if (!isHighScore)
        {
            if (failPanel != null)
            {
                failPanel.SetActive(true);
                var failController = failPanel.GetComponent<FailPanelController>();
                if (failController != null)
                {
                    failController.Show();
                    failController.ShowStats(score, distance, coinsEarned);
                }

                if (BackgroundMusicManager.Instance != null)
                {
                    BackgroundMusicManager.Instance.PlayFailMusic();
                }
            }
        }
    }

    // -------------------------
    // NAVIGATION
    // -------------------------
    public void Restart()
    {
        Time.timeScale = 1f;

        string activeSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // Single-scene game (mainlv): restart in the same scene
        if (activeSceneName == "mainlv" || activeSceneName == "Home" || activeSceneName == "Home_new")
        {
            Debug.Log("[UImanager] In-scene Restart triggered!");
            StartCoroutine(InSceneRestartFlow());
            return;
        }

        if (SceneLoader.Instance != null)
            SceneLoader.Instance.RestartGameScene();
        else
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("GameScene");
    }

    private IEnumerator InSceneRestartFlow()
    {
        isPaused = false;
        isFailed = false;

        // Switch to gameplay music instantly to start the mood change
        if (BackgroundMusicManager.Instance != null)
        {
            BackgroundMusicManager.Instance.PlayGameplayMusic();
        }

        // Hide all panels
        if (pausePanel) pausePanel.SetActive(false);
        if (failPanel) failPanel.SetActive(false);
        if (highScorePanel) highScorePanel.SetActive(false);

        // Reset coins
        PlayerPrefs.SetInt("RunCoins", 0);
        PlayerPrefs.Save();

        // Activate Canvases
        if (canvasHUD) canvasHUD.SetActive(true);
        if (canvasGame) canvasGame.SetActive(true);

        // Destroy spawned road tiles and rewind the spawn cursor so the
        // restart builds the road fresh from the starter tile
        RoadSpawner roadSpawner = Object.FindFirstObjectByType<RoadSpawner>();
        if (roadSpawner != null)
            roadSpawner.ResetSpawner();

        // Reset player to gameplay start
        GameObject runnerObj = FindRunnerObject();
        if (runnerObj != null)
        {
            runnerObj.transform.position = new Vector3(0f, runnerObj.transform.position.y, 6.50f);
            runnerObj.transform.rotation = Quaternion.identity;
            Animator runnerAnim = runnerObj.GetComponentInChildren<Animator>();
            if (runnerAnim != null)
            {
                runnerAnim.Play("Run", 0, 0f);
                if (HasAnimatorParameter(runnerAnim, "ForwardSpeed"))
                    runnerAnim.SetFloat("ForwardSpeed", 1f);
            }
        }

        // Start gameplay using the legacy play() flow
        play();

        yield return null;
    }

    public void Home()
    {
        Time.timeScale = 1f;

        string activeSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // Single-scene game (mainlv): return to home in the same scene
        if (activeSceneName == "mainlv")
        {
            Debug.Log("[UImanager] Returning to Home in the same scene (mainlv).");
            ReturnToHomeMenuInScene();
            return;
        }

        if (activeSceneName == "Home" || activeSceneName == "Home_new" || activeSceneName == "Home_old")
        {
            Debug.Log($"[UImanager] Reloading scene '{activeSceneName}' to return to clean Home state!");
            UnityEngine.SceneManagement.SceneManager.LoadScene(activeSceneName);
            return;
        }

        if (SceneLoader.Instance != null)
            SceneLoader.Instance.LoadHomeScene();
        else
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Home");
    }

    private void ReturnToHomeMenuInScene()
    {
        isPaused = false;
        isFailed = false;

        // Hide gameplay panels
        if (pausePanel) pausePanel.SetActive(false);
        if (failPanel) failPanel.SetActive(false);
        if (highScorePanel) highScorePanel.SetActive(false);
        if (canvasHUD) canvasHUD.SetActive(false);
        if (canvasGame) canvasGame.SetActive(false);

        // Reset GameState
        if (GameStateController.Instance != null)
        {
            GameStateController.Instance.SetPlaying(false);
        }

        // Clear spawned road tiles so stale roads don't linger in the distance
        RoadSpawner roadSpawner = Object.FindFirstObjectByType<RoadSpawner>();
        if (roadSpawner != null)
            roadSpawner.ResetSpawner();

        // Play home music
        if (BackgroundMusicManager.Instance != null)
        {
            BackgroundMusicManager.Instance.PlayHomeMusic();
        }

        // Reset Camera
        CameraFollowRunner camFollow = Object.FindFirstObjectByType<CameraFollowRunner>();
        if (camFollow != null)
        {
            camFollow.ResetCamera();
        }

        // Find and Reactivate Home Screen Canvas
        if (uimanager != null)
        {
            uimanager.gameObject.SetActive(true);
            uimanager.enabled = true;
            CanvasGroup homeCg = uimanager.GetHomeCanvasGroup();
            if (homeCg != null) homeCg.alpha = 1f;
            uimanager.SetHomeScreenVisible(true);
            uimanager.RefreshUI();
            uimanager.SetMenuVehiclesActive(true);
        }
    }

    /// <summary>
    /// Opens wikicatrush.app to encourage players to improve their language skills.
    /// Called from the Learn More button on the fail panel.
    /// </summary>
    public void OnLearnMorePressed()
    {
        string url = "https://wikicatrush.app";
        Debug.Log($"[UImanager] Opening Learn More: {url}");
        Application.OpenURL(url);
    }

    private void HandleGameBackButton()
    {
        // While gameplay or pause is active, handle pause/resume
        bool hudOrPauseActive = (canvasHUD != null && canvasHUD.activeInHierarchy) ||
                                (pausePanel != null && pausePanel.activeInHierarchy);
        if (hudOrPauseActive)
        {
            if (isFailed || isCountingDown) return;

            if (isPaused)
                Resume();
            else
                Pause();
        }
    }
    #endregion
}

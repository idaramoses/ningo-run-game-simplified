using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameTutorialController : MonoBehaviour
{
    private const string GAME_SCENE_NAME = "GameScene";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoBootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private static void OnSceneUnloaded(Scene scene)
    {
        if (scene.name == GAME_SCENE_NAME)
        {
            _singleton = null;
            IsActive = false;
            Debug.Log("[Tutorial] Scene unloaded - cleared singleton");
        }
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != GAME_SCENE_NAME) return;

        // Only create tutorial controller if Level 0 is selected
        if (!ShouldRunTutorial())
        {
            Debug.Log("[Tutorial] Not Level 0 - skipping tutorial bootstrap");
            return;
        }

        // Destroy ANY existing GameTutorialController (active or inactive) to start fresh
        var existing = Resources.FindObjectsOfTypeAll<GameTutorialController>();
        foreach (var ctrl in existing)
        {
            // Skip prefab assets (only destroy scene instances)
            if (ctrl.gameObject.scene.IsValid())
            {
                Debug.Log($"[Tutorial] Destroying stale controller: {ctrl.gameObject.name} (active:{ctrl.gameObject.activeInHierarchy}, enabled:{ctrl.enabled})");
                Destroy(ctrl.gameObject);
            }
        }
        
        // Clear singleton in case it was pointing to a destroyed object
        _singleton = null;
        IsActive = false;

        var go = new GameObject("GameTutorialController");
        go.AddComponent<GameTutorialController>();
        Debug.Log("[Tutorial] Auto-bootstrapped fresh controller for Level 0");
    }

    public static bool IsActive { get; private set; }
    public static bool ObstacleHitDuringTutorial { get; set; }

    private static GameTutorialController _singleton;

    public static bool ShouldRunTutorial()
    {
        // Tutorial only runs on Level 0 (Tutorial Level)
        int level = LevelManager.Instance != null ? LevelManager.Instance.GetSelectedLevel() : 1;
        return level == 0;
    }

    [Header("UI Customization")]
    [Header("Tutorial Overlay Prefab")]
    [SerializeField] private GameObject tutorialOverlayPrefab;
    
    [Header("Overlay (fallback if no prefab)")]
    [SerializeField] private Color overlayColor = new Color(0f, 0f, 0f, 0.5f);
    
    [Header("Arrow Icon (fallback)")]
    [SerializeField] private float arrowFontSize = 90f;
    [SerializeField] private Color arrowColor = Color.white;
    [SerializeField] private Vector2 arrowAnchorPosition = new Vector2(0.5f, 0.55f);
    [SerializeField] private Vector2 arrowAnchorPositionDown = new Vector2(0.5f, 0.15f);
    [SerializeField] private Vector2 arrowSize = new Vector2(300f, 130f);
    
    [Header("Hint Bar (fallback)")]
    [SerializeField] private Color hintBarColor = new Color(0f, 0f, 0f, 0.75f);
    [SerializeField] private float hintBarHeight = 120f;
    [SerializeField] private float hintBarYPosition = 140f;
    
    [Header("Hint Text (fallback)")]
    [SerializeField] private float hintTextFontSize = 38f;
    [SerializeField] private Color hintTextColor = Color.white;
    [SerializeField] private Vector2 hintTextPadding = new Vector2(24f, 6f);
    
    [Header("Countdown (fallback)")]
    [SerializeField] private float countdownFontSize = 180f;
    [SerializeField] private Color countdownColor = Color.white;

    private Canvas _canvas;
    private RectTransform _handRT;
    private TMP_Text _handLabel;
    private TMP_Text _hintLabel;
    private Coroutine _handAnim;
    private Coroutine _hintAnim;

    private bool _levelReady;
    private GameObject _cachedPlayer;
    private GameObject[] _obstacles = new GameObject[3];
    private GameObject _letter;

    private void Awake()
    {
        // Singleton guard: destroy duplicate instances immediately without touching IsActive
        if (_singleton != null && _singleton != this)
        {
            Debug.Log("[Tutorial] Duplicate instance - destroying self");
            Destroy(gameObject);
            return;
        }
        _singleton = this;

        int level = LevelManager.Instance != null ? LevelManager.Instance.GetSelectedLevel() : 1;
        if (level == 0)
            IsActive = true;
    }

    private void Start()
    {
        Debug.Log($"[Tutorial] Start() called - _singleton==this:{_singleton == this}, level:{(LevelManager.Instance != null ? LevelManager.Instance.GetSelectedLevel() : -99)}");
        
        // If we were destroyed as a duplicate in Awake, do nothing
        if (_singleton != this)
        {
            Debug.Log("[Tutorial] Start() early exit: _singleton != this");
            return;
        }

        int level = LevelManager.Instance != null ? LevelManager.Instance.GetSelectedLevel() : 1;
        if (level != 0)
        {
            Debug.Log($"[Tutorial] Start() early exit: level={level} != 0");
            _singleton = null;
            IsActive = false;
            Destroy(gameObject);
            return;
        }

        Debug.Log("[Tutorial] Start() - proceeding to run tutorial");
        IsActive = true;
        GameSceneController.OnLevelReady += () => { Debug.Log("[Tutorial] OnLevelReady fired!"); _levelReady = true; };
        BuildUI();
        StartCoroutine(RunTutorial());
    }

    private void OnDestroy()
    {
        // Only the real singleton clears the flags
        if (_singleton == this)
        {
            _singleton = null;
            IsActive = false;
            PlayerRunnerController.InputBlocked = false;
            Time.timeScale = 1f;
        }
    }

    private IEnumerator RunTutorial()
    {
        yield return new WaitUntil(() => _levelReady);
        Debug.Log("[Tutorial] Level ready — starting tutorial");

        // Clear any pre-existing letters/coins/obstacles from the road
        SweepRoad();

        // STEP 1: Swipe left/right
        yield return Step1_SwipeLanes();

        // Wait 3 seconds for player to run forward
        yield return new WaitForSeconds(3f);

        // STEP 2: Swipe up to jump
        yield return Step2_Jump();

        // STEP 3: Collect letters
        yield return Step3_Letters();
        
        // Tutorial completion panel will handle the finish via callback
    }

    private IEnumerator Step1_SwipeLanes()
    {
        Debug.Log("[Tutorial] Step 1: Swipe lanes");
        
        // Pause game, block input, show hint
        Time.timeScale = 0f;
        PlayerRunnerController.InputBlocked = true;
        ShowHint("← →", "Swipe left or right\nto change lanes");
        StartHandAnim(AnimMode.Horizontal);

        // Wait 3 seconds for player to read
        yield return new WaitForSecondsRealtime(3f);

        // Unlock input and wait for swipe
        PlayerRunnerController.InputBlocked = false;
        bool swiped = false;
        System.Action onSwipe = () => swiped = true;
        PlayerRunnerController.OnSwipedHorizontal += onSwipe;

        yield return new WaitUntil(() => swiped);

        PlayerRunnerController.OnSwipedHorizontal -= onSwipe;
        HideHint();
        Time.timeScale = 1f;
        Debug.Log("[Tutorial] Step 1 complete");
    }

    private IEnumerator Step2_Jump()
    {
        Debug.Log("[Tutorial] Step 2: Jump");

        // Find obstacle prefab
        GameObject obstaclePrefab = FindObstaclePrefab();
        if (obstaclePrefab == null)
        {
            Debug.LogError("[Tutorial] Step 2: No obstacle prefab found! Skipping.");
            yield break;
        }

        bool passedObstacles = false;
        
        while (!passedObstacles)
        {
            // Place 3 obstacles - one per lane
            GameObject player = GetPlayer();
            if (player == null) yield break;

            float playerZ = player.transform.position.z;
            float obstacleZ = playerZ + 35f;
            float[] laneX = { -4.5f, 0f, 4.5f }; // Left, Center, Right
            
            // Clear old obstacles if retrying
            ClearObstacles();
            
            for (int i = 0; i < 3; i++)
            {
                Vector3 pos = new Vector3(laneX[i], 0.5f, obstacleZ);
                _obstacles[i] = Instantiate(obstaclePrefab, pos, Quaternion.identity);
                _obstacles[i].tag = "Obstacle";
                _obstacles[i].SetActive(true);
            }
            Debug.Log($"[Tutorial] Placed 3 obstacles (one per lane) at Z={obstacleZ:F1}");

            // Pause immediately and show jump hint
            Time.timeScale = 0f;
            ShowHint("↑", "Swipe UP to jump\nover obstacles!");
            StartHandAnim(AnimMode.Up);

            // Wait for jump
            bool jumped = false;
            System.Action onJump = () => jumped = true;
            PlayerRunnerController.OnSwipedUp += onJump;
            yield return new WaitUntil(() => jumped);
            PlayerRunnerController.OnSwipedUp -= onJump;
            
            HideHint();
            Time.timeScale = 1f;

            // Wait to see if player hits obstacle or passes safely
            ObstacleHitDuringTutorial = false;
            bool hitObstacle = false;
            float checkStartZ = player.transform.position.z;
            float obstacleEndZ = obstacleZ + 5f;
            
            while (player.transform.position.z < obstacleEndZ)
            {
                // Check if obstacle was hit (flag set by PlayerRunnerController)
                if (ObstacleHitDuringTutorial)
                {
                    hitObstacle = true;
                    Debug.Log("[Tutorial] Player hit obstacle - restarting Step 2");
                    break;
                }
                
                yield return null;
            }

            if (!hitObstacle)
            {
                passedObstacles = true;
                Debug.Log("[Tutorial] Player successfully passed obstacles");
            }
            else
            {
                // Reset player position back before obstacles
                if (player != null)
                {
                    CharacterController cc = player.GetComponent<CharacterController>();
                    if (cc != null)
                    {
                        cc.enabled = false;
                        player.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, obstacleZ - 25f);
                        cc.enabled = true;
                        Debug.Log("[Tutorial] Reset player position for retry");
                    }
                }
                yield return new WaitForSeconds(0.5f);
            }
        }

        // Clear obstacles
        ClearObstacles();
        Debug.Log("[Tutorial] Step 2 complete");
    }

    private GameObject FindObstaclePrefab()
    {
        // Try RoadSegment
        foreach (var seg in FindObjectsOfType<RoadSegment>())
        {
            if (seg.obstaclePrefabs != null && seg.obstaclePrefabs.Length > 0)
                return seg.obstaclePrefabs[0];
        }
        
        // Try RoadSpawner
        RoadSpawner spawner = FindObjectOfType<RoadSpawner>();
        if (spawner != null && spawner.roadPrefabs != null)
        {
            foreach (var roadPrefab in spawner.roadPrefabs)
            {
                if (roadPrefab != null)
                {
                    RoadSegment roadSeg = roadPrefab.GetComponent<RoadSegment>();
                    if (roadSeg != null && roadSeg.obstaclePrefabs != null && roadSeg.obstaclePrefabs.Length > 0)
                        return roadSeg.obstaclePrefabs[0];
                }
            }
        }
        
        return null;
    }

    private void ClearObstacles()
    {
        for (int i = 0; i < 3; i++)
        {
            if (_obstacles[i] != null)
            {
                Destroy(_obstacles[i]);
                _obstacles[i] = null;
            }
        }
    }

    private IEnumerator Step3_Letters()
    {
        Debug.Log("[Tutorial] Step 3: Letters");

        // Place a letter ahead
        GameObject player = GetPlayer();
        if (player == null) yield break;

        GameObject letterPrefab = null;
        foreach (var seg in FindObjectsOfType<RoadSegment>())
        {
            if (seg.letterPrefab != null)
            {
                letterPrefab = seg.letterPrefab;
                break;
            }
        }

        if (letterPrefab != null)
        {
            float playerZ = player.transform.position.z;
            float playerX = player.transform.position.x;
            Vector3 pos = new Vector3(playerX, 1.5f, playerZ + 35f);
            _letter = Instantiate(letterPrefab, pos, Quaternion.identity);
            
            var visual = _letter.GetComponent<LetterVisual>();
            if (visual != null)
            {
                visual.SetLetter("A");
                visual.SetCorrect(true);
            }
            
            // Mark as tutorial letter using layer (avoid tag requirement)
            _letter.layer = LayerMask.NameToLayer("Ignore Raycast");
            
            // Add a component to identify it as tutorial letter
            var tutorialMarker = _letter.AddComponent<TutorialLetterMarker>();
            
            Debug.Log($"[Tutorial] Placed tutorial letter at Z={playerZ + 35f}");
        }

        // Pause immediately and show hint
        Time.timeScale = 0f;
        
        // Get selected language and capitalize first letter
        string selectedLang = UserSession.SelectedLanguage;
        if (string.IsNullOrEmpty(selectedLang))
        {
            selectedLang = "your language";
        }
        else
        {
            selectedLang = char.ToUpper(selectedLang[0]) + selectedLang.Substring(1);
        }
        
        string hintText = $"Collect letters to form\nthe translated word below in {selectedLang}!";
        ShowHint("↓", hintText, positionBelowHintBar: true);
        StartHandAnim(AnimMode.Down);

        // Wait 7 seconds for player to read hint
        yield return new WaitForSecondsRealtime(7f);

        // Hide hint and let player run
        HideHint();
        Time.timeScale = 1f;

        // Wait 3 seconds for player to run and pick letter
        yield return new WaitForSecondsRealtime(3f);
        
        // Clean up the tutorial letter
        if (_letter != null)
        {
            Destroy(_letter);
            _letter = null;
        }
        
        Debug.Log("[Tutorial] Step 3 complete");
        
        // Show tutorial completion panel
        ShowTutorialComplete();
    }

    private IEnumerator ShowCountdown()
    {
        Debug.Log("[Tutorial] Starting countdown");
        Time.timeScale = 0f;
        
        // Hide hint bar and arrow, show only countdown text in center
        if (_canvas) _canvas.gameObject.SetActive(true);
        if (_handRT) _handRT.gameObject.SetActive(false);
        
        // Temporarily increase hint text size and center it
        float originalSize = _hintLabel.fontSize;
        TextAlignmentOptions originalAlignment = _hintLabel.alignment;
        _hintLabel.fontSize = countdownFontSize;
        _hintLabel.alignment = TextAlignmentOptions.Center;
        _hintLabel.color = countdownColor;
        
        // Position hint label in center of screen
        var hintParent = _hintLabel.transform.parent.GetComponent<RectTransform>();
        hintParent.anchorMin = new Vector2(0f, 0f);
        hintParent.anchorMax = new Vector2(1f, 1f);
        hintParent.anchoredPosition = Vector2.zero;
        hintParent.sizeDelta = Vector2.zero;
        hintParent.GetComponent<Image>().color = Color.clear; // Hide bar background
        
        // Show 3
        _hintLabel.text = "3";
        yield return new WaitForSecondsRealtime(1f);
        
        // Show 2
        _hintLabel.text = "2";
        yield return new WaitForSecondsRealtime(1f);
        
        // Show 1
        _hintLabel.text = "1";
        yield return new WaitForSecondsRealtime(1f);
        
        // Show GO!
        _hintLabel.text = "GO!";
        Time.timeScale = 1f;
        yield return new WaitForSecondsRealtime(0.5f);
        
        // Restore original settings
        _hintLabel.fontSize = originalSize;
        _hintLabel.alignment = originalAlignment;
        _hintLabel.color = hintTextColor;
        hintParent.GetComponent<Image>().color = hintBarColor;
        hintParent.anchorMin = new Vector2(0f, 0f);
        hintParent.anchorMax = new Vector2(1f, 0f);
        hintParent.anchoredPosition = new Vector2(0f, hintBarYPosition);
        hintParent.sizeDelta = new Vector2(0f, hintBarHeight);
        
        HideHint();
        if (_handRT) _handRT.gameObject.SetActive(true);
        
        Debug.Log("[Tutorial] Countdown complete");
    }

    private void ShowTutorialComplete()
    {
        Debug.Log("[Tutorial] Showing completion panel");
        
        // Find LevelCompletionPanel (including inactive ones)
        LevelCompletionPanel panel = LevelCompletionPanel.Instance;
        if (panel == null)
        {
            // Search inactive objects too
            var allPanels = Resources.FindObjectsOfTypeAll<LevelCompletionPanel>();
            foreach (var p in allPanels)
            {
                if (p.gameObject.scene.IsValid())
                {
                    panel = p;
                    Debug.Log($"[Tutorial] Found inactive LevelCompletionPanel: {p.gameObject.name}");
                    break;
                }
            }
        }
        
        if (panel != null)
        {
            // Hide tutorial UI
            if (_canvas != null) _canvas.gameObject.SetActive(false);

            // Ensure the dancing preview/camera matches normal completion flow
            if (GameSceneController.Instance != null)
            {
                GameSceneController.Instance.ShowCompletionPreviewOnly();
            }
            
            // Activate panel GameObject so Awake/Start run
            panel.gameObject.SetActive(true);
            
            panel.ShowTutorialComplete(onStart: () =>
            {
                Debug.Log("[Tutorial] Player pressed start - beginning main game");
                
                // Mark Level 0 done and switch to Level 1 BEFORE any scene reload
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.SetSuppressAutoUnlock(true);
                    LevelManager.Instance.CompleteLevel(0, 0);
                    LevelManager.Instance.SetSelectedLevel(1);
                    LevelManager.Instance.SetSuppressAutoUnlock(false);
                    Debug.Log("[Tutorial] Level 0 complete, Level 1 unlocked and selected");
                }
                
                Finish();
                LoadLevelOneAfterTutorial();
            });
        }
        else
        {
            Debug.LogWarning("[Tutorial] LevelCompletionPanel not found anywhere - creating built-in completion UI");
            StartCoroutine(ShowBuiltInCompletionUI());
        }
    }

    private IEnumerator ShowBuiltInCompletionUI()
    {
        // Reuse existing canvas for completion message
        Time.timeScale = 0f;
        
        if (_canvas != null) _canvas.gameObject.SetActive(true);
        if (_handRT != null) _handRT.gameObject.SetActive(false);
        
        // Show completion message
        if (_hintLabel != null)
        {
            var hintParent = _hintLabel.transform.parent.GetComponent<RectTransform>();
            hintParent.anchorMin = new Vector2(0f, 0f);
            hintParent.anchorMax = new Vector2(1f, 1f);
            hintParent.anchoredPosition = Vector2.zero;
            hintParent.sizeDelta = Vector2.zero;
            hintParent.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.9f);
            
            _hintLabel.fontSize = 48f;
            _hintLabel.alignment = TextAlignmentOptions.Center;
            _hintLabel.text = "Tutorial Complete!\n\nYou're ready to start your journey!\n\nTap anywhere to continue...";
        }
        
        Debug.Log("[Tutorial] Waiting for player tap to continue");
        
        // Wait for any touch/click
        yield return new WaitUntil(() => 
            (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || 
            Input.GetMouseButtonDown(0)
        );
        
        // Mark tutorial level (Level 0) as complete and unlock Level 1
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.SetSuppressAutoUnlock(true);
            LevelManager.Instance.CompleteLevel(0, 0); // Level 0 complete
            LevelManager.Instance.SetSelectedLevel(1); // Navigate to Level 1
            LevelManager.Instance.SetSuppressAutoUnlock(false);
            Debug.Log("[Tutorial] Level 0 (Tutorial) complete - Level 1 unlocked");
        }
        
        Finish();
        LoadLevelOneAfterTutorial();
    }

    private void Finish()
    {
        Debug.Log("[Tutorial] Starting main game!");
        Time.timeScale = 1f;

        // Enable normal spawning immediately
        IsActive = false;
        Debug.Log("[Tutorial] IsActive set to FALSE - spawning should resume");
        
        // Force trigger letter spawning
        GameObject player = GetPlayer();
        float playerZ = player != null ? player.transform.position.z : 0f;
        
        var letterSpawner = FindObjectOfType<LetterSpawner>();
        if (letterSpawner != null)
        {
            // Force spawn letters ahead of player using reflection
            Debug.Log($"[Tutorial] Triggering LetterSpawner to spawn ahead of player at Z={playerZ}");
            var method = letterSpawner.GetType().GetMethod("SpawnMoreLetters", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (method != null)
            {
                method.Invoke(letterSpawner, null);
                Debug.Log("[Tutorial] Called SpawnMoreLetters via reflection");
            }
        }
        
        // Re-spawn items on road segments ahead
        int spawned = 0;
        foreach (var seg in FindObjectsOfType<RoadSegment>())
        {
            if (seg.transform.position.z > playerZ)
            {
                seg.SpawnItems();
                spawned++;
            }
        }
        Debug.Log($"[Tutorial] Spawned items on {spawned} road segments");

        // Clean up tutorial UI
        if (_canvas != null) Destroy(_canvas.gameObject);
        Destroy(gameObject);
    }

    private void LoadLevelOneAfterTutorial()
    {
        if (SceneLoader.Instance != null)
        {
            Debug.Log("[Tutorial] Loading GameScene for Level 1");
            SceneLoader.Instance.LoadGameScene();
        }
        else
        {
            Debug.LogWarning("[Tutorial] SceneLoader not found - loading GameScene directly");
            SceneManager.LoadSceneAsync(GAME_SCENE_NAME);
        }
    }

    private GameObject GetPlayer()
    {
        if (_cachedPlayer != null && _cachedPlayer.activeInHierarchy) return _cachedPlayer;
        _cachedPlayer = GameObject.FindGameObjectWithTag("Player");
        if (_cachedPlayer == null && RunnerManager.Instance != null)
            _cachedPlayer = RunnerManager.Instance.GetCurrentRunnerInstance();
        return _cachedPlayer;
    }

    private enum AnimMode { Horizontal, Up, Down }

    private void StartHandAnim(AnimMode mode)
    {
        if (_handAnim != null) StopCoroutine(_handAnim);
        _handAnim = StartCoroutine(AnimateHand(mode));
    }

    private IEnumerator AnimateHand(AnimMode mode)
    {
        float t = 0f;
        while (true)
        {
            t += Time.unscaledDeltaTime * 1.8f;
            float ping = Mathf.PingPong(t, 1f);
            float ease = ping * ping * (3f - 2f * ping);

            Vector2 offset;
            if (mode == AnimMode.Horizontal) offset = new Vector2(Mathf.Lerp(-80f, 80f, ease), 0f);
            else if (mode == AnimMode.Up) offset = new Vector2(0f, Mathf.Lerp(0f, 100f, ease));
            else offset = new Vector2(0f, Mathf.Lerp(0f, -70f, ease));

            _handRT.anchoredPosition = offset;
            yield return null;
        }
    }

    private void ShowHint(string arrow, string hint, bool positionBelowHintBar = false)
    {
        if (_handLabel) _handLabel.text = arrow;
        if (_hintLabel) _hintLabel.text = hint;
        
        // Reposition arrow below hint bar for Step 3
        if (positionBelowHintBar && _handRT != null)
        {
            _handRT.anchorMin = new Vector2(0.5f, 0.15f);
            _handRT.anchorMax = new Vector2(0.5f, 0.15f);
            _handRT.anchoredPosition = Vector2.zero;
        }
        else if (_handRT != null)
        {
            // Reset to default position
            _handRT.anchorMin = new Vector2(0.5f, 0.55f);
            _handRT.anchorMax = new Vector2(0.5f, 0.55f);
            _handRT.anchoredPosition = Vector2.zero;
        }
        
        if (_canvas) _canvas.gameObject.SetActive(true);
        
        // Play hint animation
        if (_hintAnim != null) StopCoroutine(_hintAnim);
        _hintAnim = StartCoroutine(AnimateHintIn());
    }

    private void HideHint()
    {
        if (_canvas) _canvas.gameObject.SetActive(false);
        if (_handAnim != null) { StopCoroutine(_handAnim); _handAnim = null; }
        if (_hintAnim != null) { StopCoroutine(_hintAnim); _hintAnim = null; }
    }

    private IEnumerator AnimateHintIn()
    {
        if (_hintLabel == null) yield break;
        
        // Start with scale 0 and fade in
        _hintLabel.alpha = 0f;
        _hintLabel.transform.localScale = Vector3.zero;
        
        float duration = 0.4f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float ease = t * t * (3f - 2f * t); // Smoothstep
            
            _hintLabel.alpha = ease;
            _hintLabel.transform.localScale = Vector3.one * ease;
            
            yield return null;
        }
        
        _hintLabel.alpha = 1f;
        _hintLabel.transform.localScale = Vector3.one;
    }

    private void BuildUI()
    {
        GameObject prefab = tutorialOverlayPrefab;
        
        // Try loading from Resources if no prefab assigned
        if (prefab == null)
        {
            prefab = Resources.Load<GameObject>("TutorialOverlay");
            Debug.Log($"[Tutorial] Loaded prefab from Resources: {(prefab != null ? "SUCCESS" : "FAILED")}");
        }
        
        if (prefab != null)
        {
            var instance = Instantiate(prefab);
            instance.name = "TutorialOverlay (Instance)";
            _canvas = instance.GetComponent<Canvas>();
            
            // Find required child components
            _handRT = instance.transform.Find("HandIndicator")?.GetComponent<RectTransform>();
            _handLabel = _handRT?.GetComponentInChildren<TextMeshProUGUI>();
            
            var hintBar = instance.transform.Find("HintBar");
            _hintLabel = hintBar?.Find("HintText")?.GetComponent<TextMeshProUGUI>();
            
            Debug.Log($"[Tutorial] HandIndicator found: {_handRT != null}, HintBar found: {hintBar != null}, HintText found: {_hintLabel != null}");
            
            instance.SetActive(false);
            Debug.Log("[Tutorial] Built UI from prefab");
            return;
        }

        // Fallback: build programmatically
        var canvasGO = new GameObject("TutorialCanvas");
        _canvas = canvasGO.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 100;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();
        canvasGO.SetActive(false);

        var dimGO = new GameObject("Dim");
        dimGO.transform.SetParent(canvasGO.transform, false);
        var dimRT = dimGO.AddComponent<RectTransform>();
        dimRT.anchorMin = Vector2.zero;
        dimRT.anchorMax = Vector2.one;
        dimRT.offsetMin = dimRT.offsetMax = Vector2.zero;
        dimGO.AddComponent<Image>().color = overlayColor;

        var handGO = new GameObject("HandIndicator");
        handGO.transform.SetParent(canvasGO.transform, false);
        _handRT = handGO.AddComponent<RectTransform>();
        _handRT.anchorMin = _handRT.anchorMax = arrowAnchorPosition;
        _handRT.sizeDelta = arrowSize;
        _handRT.anchoredPosition = Vector2.zero;
        _handLabel = handGO.AddComponent<TextMeshProUGUI>();
        _handLabel.fontSize = arrowFontSize;
        _handLabel.alignment = TextAlignmentOptions.Center;
        _handLabel.color = arrowColor;

        var barGO = new GameObject("HintBar");
        barGO.transform.SetParent(canvasGO.transform, false);
        var barRT = barGO.AddComponent<RectTransform>();
        barRT.anchorMin = new Vector2(0f, 0f);
        barRT.anchorMax = new Vector2(1f, 0f);
        barRT.pivot = new Vector2(0.5f, 0f);
        barRT.anchoredPosition = new Vector2(0f, hintBarYPosition);
        barRT.sizeDelta = new Vector2(0f, hintBarHeight);
        barGO.AddComponent<Image>().color = hintBarColor;

        var textGO = new GameObject("HintText");
        textGO.transform.SetParent(barGO.transform, false);
        var textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(hintTextPadding.x, hintTextPadding.y);
        textRT.offsetMax = new Vector2(-hintTextPadding.x, -hintTextPadding.y);
        _hintLabel = textGO.AddComponent<TextMeshProUGUI>();
        _hintLabel.fontSize = hintTextFontSize;
        _hintLabel.alignment = TextAlignmentOptions.Center;
        _hintLabel.color = hintTextColor;
        _hintLabel.enableWordWrapping = true;
        
        Debug.Log("[Tutorial] Built UI programmatically (no prefab assigned)");
    }

    private void SweepRoad()
    {
        int cleared = 0;
        
        // Clear all letters
        var letters = GameObject.FindGameObjectsWithTag("Letter");
        foreach (var l in letters)
        {
            Destroy(l);
            cleared++;
        }
        
        // Clear all coins
        var coins = GameObject.FindGameObjectsWithTag("Coin");
        foreach (var c in coins)
        {
            Destroy(c);
            cleared++;
        }
        
        // Clear all obstacles
        var obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (var o in obstacles)
        {
            Destroy(o);
            cleared++;
        }
        
        Debug.Log($"[Tutorial] Swept {cleared} objects from road");
    }
}

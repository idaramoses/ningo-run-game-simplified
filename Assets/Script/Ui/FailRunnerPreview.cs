using UnityEngine;

public class FailRunnerPreview : MonoBehaviour
{
    [Header("References")]
    public Camera previewCamera;
    public GameObject runnerPrefabOrModel;
    public string fallStateName = "preview fall";

    [Header("Placement (local to this root)")]
    public Vector3 localSpawnPos = Vector3.zero;
    public Vector3 localSpawnEuler = new Vector3(0, 180, 0);
    public float scale = 1f;

    [Header("Layer")]
    public string previewLayerName = "PreviewRunner";
    public string idleStateName = "idle";
    
    [Header("Dance Animations")]
    public string dance1StateName = "dance 1";
    public string dance2StateName = "dance 2";
    public string dance3StateName = "dance 3";
    
    private GameObject instance;
    private Animator anim;
    private bool isPlayingFall = false;
    private bool isPlayingDance = false;
    private string currentDanceState = "";
    private bool fallAnimationCompleted = false;
    private float fallStartTime = 0f;
    private float fallDuration = 1.5f; // Duration in seconds before transitioning to idle
    private bool skipRunnerManagerLookup = false;

    private void OnEnable()
    {
        // Try to get runner immediately
        bool gotRunner = TryGetCurrentRunner();
        
        if (gotRunner)
        {
            SpawnFresh();
        }
        else
        {
            // RunnerManager might not be ready yet - wait a frame and try again
            StartCoroutine(SpawnFreshDelayed());
        }
    }
    
    private bool TryGetCurrentRunner()
    {
        // First try Instance
        if (RunnerManager.Instance != null)
        {
            GameObject currentRunner = RunnerManager.Instance.GetCurrentRunnerInstance();
            if (currentRunner != null)
            {
                runnerPrefabOrModel = currentRunner;
                Debug.Log($"[FailRunnerPreview] Got runner from RunnerManager.Instance: {currentRunner.name}");
                return true;
            }
        }
        
        // Fallback: Use static prefab reference (survives Instance destruction)
        GameObject staticPrefab = RunnerManager.GetSelectedRunnerPrefab();
        if (staticPrefab != null)
        {
            runnerPrefabOrModel = staticPrefab;
            Debug.Log($"[FailRunnerPreview] Got runner from static prefab: {staticPrefab.name}");
            return true;
        }
        
        return false;
    }
    
    private System.Collections.IEnumerator SpawnFreshDelayed()
    {
        // Retry multiple times with increasing delays to wait for RunnerManager
        int maxRetries = 10;
        for (int i = 0; i < maxRetries; i++)
        {
            yield return null; // Wait one frame
            
            if (TryGetCurrentRunner())
            {
                Debug.Log($"[FailRunnerPreview] Got runner on retry {i + 1}");
                SpawnFresh();
                yield break;
            }
            
            Debug.Log($"[FailRunnerPreview] Retry {i + 1}/{maxRetries} - RunnerManager.Instance null: {RunnerManager.Instance == null}");
        }
        
        // Fallback: spawn with whatever we have (Inspector-assigned model)
        Debug.LogWarning("[FailRunnerPreview] Could not get runner from RunnerManager after retries, using fallback");
        SpawnFresh();
    }

    private void OnDisable()
    {
        // Optional: cleanup so it always restarts clean
        if (instance != null) Destroy(instance);
        instance = null;
        anim = null;
        isPlayingFall = false;
    }

    private void Update()
    {
        // Monitor fall animation and transition to idle after duration
        if (isPlayingFall && anim != null && !fallAnimationCompleted)
        {
            // Check if enough time has passed since fall started
            float elapsedTime = Time.unscaledTime - fallStartTime;
            
            if (elapsedTime >= fallDuration)
            {
                // Fall animation duration complete, transition to idle
                fallAnimationCompleted = true;
                isPlayingFall = false; // Stop monitoring fall so idle can loop naturally
                anim.Play(idleStateName, 0, 0f);
                Debug.Log("[FailRunnerPreview] Fall animation complete, transitioning to idle");
            }
            else
            {
                // Keep playing fall animation, prevent transitions
                AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
                if (!stateInfo.IsName(fallStateName))
                {
                    anim.Play(fallStateName, 0, 0f);
                }
            }
        }
        
        // Keep dance animation playing - check if it's still in the dance state
        if (isPlayingDance && anim != null && !string.IsNullOrEmpty(currentDanceState))
        {
            // Check if animator has transitioned away from the dance state
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.IsName(currentDanceState))
            {
                // Force it back to the dance state to keep looping
                anim.Play(currentDanceState, 0, 0f);
            }
        }
    }

    private void SpawnFresh()
    {
        // If ChangeRunner() explicitly set the runner, skip the RunnerManager lookup
        if (!skipRunnerManagerLookup)
        {
            // ALWAYS try to get the current runner from RunnerManager first
            Debug.Log($"[FailRunnerPreview] SpawnFresh - RunnerManager.Instance is null: {RunnerManager.Instance == null}");
            
            if (RunnerManager.Instance != null)
            {
                GameObject currentRunner = RunnerManager.Instance.GetCurrentRunnerInstance();
                Debug.Log($"[FailRunnerPreview] SpawnFresh - GetCurrentRunnerInstance() returned: {(currentRunner != null ? currentRunner.name : "NULL")}");
                
                if (currentRunner != null)
                {
                    runnerPrefabOrModel = currentRunner;
                    Debug.Log($"[FailRunnerPreview] SpawnFresh - Using runner from RunnerManager: {currentRunner.name}");
                }
            }
            else
            {
                // Fallback: Use static prefab reference (survives Instance destruction)
                GameObject staticPrefab = RunnerManager.GetSelectedRunnerPrefab();
                if (staticPrefab != null)
                {
                    runnerPrefabOrModel = staticPrefab;
                    Debug.Log($"[FailRunnerPreview] SpawnFresh - Using static prefab fallback: {staticPrefab.name}");
                }
            }
        }
        else
        {
            Debug.Log($"[FailRunnerPreview] SpawnFresh - Skipping RunnerManager lookup, using explicitly set runner: {(runnerPrefabOrModel != null ? runnerPrefabOrModel.name : "NULL")}");
            skipRunnerManagerLookup = false;
        }
        
        if (runnerPrefabOrModel == null)
        {
            Debug.LogError("[FailRunnerPreview] runnerPrefabOrModel not assigned.");
            return;
        }

        if (instance != null) Destroy(instance);

        // Force source to be INACTIVE so the clone starts inactive and doesn't run Awake/OnEnable code yet
        bool wasActive = runnerPrefabOrModel.activeSelf;
        if (wasActive) runnerPrefabOrModel.SetActive(false);

        instance = Instantiate(runnerPrefabOrModel, transform);
        
        // Restore the source's original state
        if (wasActive) runnerPrefabOrModel.SetActive(true);

        // Disable components on the inactive clone so they never run when activated
        var charController = instance.GetComponent<CharacterController>();
        if (charController != null) charController.enabled = false;
        
        // Position and layer the inactive clone
        instance.transform.localPosition = localSpawnPos;
        instance.transform.localRotation = Quaternion.Euler(localSpawnEuler);
        instance.transform.localScale = Vector3.one * scale;

        int layer = LayerMask.NameToLayer(previewLayerName);
        if (layer == -1) layer = LayerMask.NameToLayer("mmk");
        if (layer == -1) layer = 8;
        if (layer != -1)
            SetLayerRecursively(instance, layer);

        // Configure animator
        anim = instance.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            // ✅ Keep anim running even when Time.timeScale = 0
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        // Now activate the cleanly configured clone safely
        instance.SetActive(true);
        
        Debug.Log($"[FailRunnerPreview] Spawned preview safely: {instance.name}, active: {instance.activeSelf}");
    }

    public void PlayFall()
    {
        if (anim == null) return;

        isPlayingFall = true;
        fallAnimationCompleted = false;
        fallStartTime = Time.unscaledTime; // Record start time for timer
        
        // Disable and re-enable to reset state machine
        anim.enabled = false;
        anim.SetFloat("ForwardSpeed", 0f);
        anim.SetBool("isGrounded", false);
        anim.SetBool("isSliding", false);
        anim.enabled = true;
        
        anim.speed = 1f;
        anim.updateMode = AnimatorUpdateMode.UnscaledTime;

        // ✅ Force jump into fall state at frame 0
        anim.Play(fallStateName, 0, 0f);
        anim.Update(0f);
        
        Debug.Log("[FailRunnerPreview] Started fall animation");
    }

    public void PlayIdle()
{
    if (anim == null) return;
    
    isPlayingFall = false;
    isPlayingDance = false;
    
    anim.updateMode = AnimatorUpdateMode.UnscaledTime;
    anim.speed = 1f;
    anim.Play(idleStateName, 0, 0f);
    anim.Update(0f);
}

public void PlayDance()
{
    if (anim == null) return;
    
    isPlayingFall = false;
    isPlayingDance = true;
    
    // Randomly select one of the three dance animations
    int randomDance = Random.Range(1, 4); // Returns 1, 2, or 3
    string selectedDance = "";
    
    switch (randomDance)
    {
        case 1:
            selectedDance = dance1StateName;
            Debug.Log("[FailRunnerPreview] Playing Dance 1");
            break;
        case 2:
            selectedDance = dance2StateName;
            Debug.Log("[FailRunnerPreview] Playing Dance 2");
            break;
        case 3:
            selectedDance = dance3StateName;
            Debug.Log("[FailRunnerPreview] Playing Dance 3");
            break;
    }
    
    anim.updateMode = AnimatorUpdateMode.UnscaledTime;
    anim.speed = 1f;
    anim.Play(selectedDance, 0, 0f);
    anim.Update(0f);
    
    // Store the selected dance state for continuous playback in Update()
    currentDanceState = selectedDance;
    
    Debug.Log($"[FailRunnerPreview] Started dance animation: {selectedDance}");
}

    public void FreezePose()
    {
        if (anim != null) anim.speed = 0f;
    }

    public void ChangeRunner(GameObject newRunnerPrefab, RuntimeAnimatorController animController = null)
    {
        if (newRunnerPrefab == null)
        {
            Debug.LogWarning("[FailRunnerPreview] Cannot change to null runner prefab");
            return;
        }

        runnerPrefabOrModel = newRunnerPrefab;
        skipRunnerManagerLookup = true;
        SpawnFresh();
        
        // Apply animator controller if provided
        if (animController != null && anim != null)
        {
            anim.runtimeAnimatorController = animController;
        }
        
        PlayIdle();
        
        Debug.Log($"[FailRunnerPreview] Changed runner to: {newRunnerPrefab.name}");
    }

    private static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}

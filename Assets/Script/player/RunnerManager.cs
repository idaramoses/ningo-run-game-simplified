using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class RunnerConfig
{
    public string runnerName;
    public GameObject runnerGameObject;
    public bool isActive = false;
}

public class RunnerManager : MonoBehaviour
{
    public static RunnerManager Instance { get; private set; }
    
    // STATIC variables - persist across scene reloads without needing DontDestroyOnLoad
    private static int s_SelectedRunnerIndex = -1;
    private static GameObject s_SelectedRunnerPrefab = null; // Static reference for preview fallback

    [Header("Runner GameObjects (Existing in Scene)")]
    public List<RunnerConfig> runners = new List<RunnerConfig>();

    private GameObject currentRunnerInstance;
    private string selectedRunner = "Amaka";
    public const string SELECTED_RUNNER_KEY = "SelectedRunner";
    
    // Static method to get selected runner prefab for preview (survives Instance destruction)
    public static GameObject GetSelectedRunnerPrefab()
    {
        return s_SelectedRunnerPrefab;
    }

    public static void SetSelectedRunnerPrefab(GameObject prefab)
    {
        s_SelectedRunnerPrefab = prefab;
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        Debug.Log($"[RunnerManager] OnDestroy called - gameObject: {gameObject.name}, this was Instance: {Instance == this}");
#endif
    }

    private void Awake()
    {
#if UNITY_EDITOR
        Debug.Log($"[RunnerManager] Awake START - gameObject: {gameObject.name}, Instance was: {(Instance != null ? Instance.gameObject.name : "null")}");
#endif
        Instance = this;
        
        // IMMEDIATELY deactivate all runners to prevent wrong runner showing
        for (int i = 0; i < runners.Count; i++)
        {
            if (runners[i].runnerGameObject != null)
            {
                runners[i].runnerGameObject.SetActive(false);
            }
        }
        
        // Load from PlayerPrefs on first run only
        if (s_SelectedRunnerIndex < 0)
        {
            s_SelectedRunnerIndex = PlayerPrefs.GetInt(SELECTED_RUNNER_KEY, 0);
#if UNITY_EDITOR
            Debug.Log($"[RunnerManager] First load - s_SelectedRunnerIndex from PlayerPrefs: {s_SelectedRunnerIndex}");
#endif
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log($"[RunnerManager] Scene reload - using static s_SelectedRunnerIndex: {s_SelectedRunnerIndex}");
#endif
        }
        
        // Activate the correct runner IMMEDIATELY (no delay)
        int indexToUse = Mathf.Clamp(s_SelectedRunnerIndex, 0, Mathf.Max(0, runners.Count - 1));
#if UNITY_EDITOR
        Debug.Log($"[RunnerManager] Awake - runners.Count: {runners.Count}, indexToUse: {indexToUse}");
#endif
        
        if (runners.Count > 0)
        {
#if UNITY_EDITOR
            Debug.Log($"[RunnerManager] Awake - runners[{indexToUse}].runnerGameObject is null: {runners[indexToUse].runnerGameObject == null}");
#endif
        }
        
        if (runners.Count > 0 && runners[indexToUse].runnerGameObject != null)
        {
            runners[indexToUse].runnerGameObject.SetActive(true);
            runners[indexToUse].isActive = true;
            currentRunnerInstance = runners[indexToUse].runnerGameObject;
            selectedRunner = runners[indexToUse].runnerName;
            s_SelectedRunnerPrefab = runners[indexToUse].runnerGameObject; // Store static reference for preview
#if UNITY_EDITOR
            Debug.Log($"[RunnerManager] Awake - Immediately activated: {selectedRunner}, stored static prefab");
#endif
        }
        else
        {
            Debug.LogWarning($"[RunnerManager] Awake - Could NOT activate runner! runners.Count={runners.Count}");
        }
    }
    
    private void OnEnable()
    {
        // Update system references IMMEDIATELY - don't wait
        UpdateSystemReferences();
    }

    private void Start()
    {
        StartCoroutine(InitializeActiveRunnerDelayed());
    }

    private IEnumerator InitializeActiveRunnerDelayed()
    {
        // Wait one frame to ensure all objects are ready
        yield return null;
        
        InitializeActiveRunner();
    }

    private void InitializeActiveRunner()
    {
#if UNITY_EDITOR
        Debug.Log($"[RunnerManager] InitializeActiveRunner - runners.Count: {runners.Count}, s_SelectedRunnerIndex: {s_SelectedRunnerIndex}");
#endif
        
        if (runners.Count == 0)
        {
            Debug.LogError("[RunnerManager] Runners list is empty!");
            return;
        }
        
        // Clamp index to valid range
        int indexToUse = Mathf.Clamp(s_SelectedRunnerIndex, 0, runners.Count - 1);
        
        string runnerToActivate = runners[indexToUse].runnerName;
        selectedRunner = runnerToActivate;
        
#if UNITY_EDITOR
        Debug.Log($"[RunnerManager] Activating runner at index {indexToUse}: '{runnerToActivate}'");
#endif
        
        ActivateRunnerByIndex(indexToUse);
    }
    
    // Called by RunnerSelectionManager when user selects a runner
    public static void SetSelectedRunnerIndex(int index)
    {
        s_SelectedRunnerIndex = index;
        PlayerPrefs.SetInt(SELECTED_RUNNER_KEY, index);
        PlayerPrefs.Save();
#if UNITY_EDITOR
        Debug.Log($"[RunnerManager] SetSelectedRunnerIndex: {index}");
#endif
    }
    
    public static int GetSelectedRunnerIndex()
    {
        return s_SelectedRunnerIndex;
    }
    
    private void ActivateRunnerByIndex(int index)
    {
        if (index < 0 || index >= runners.Count)
        {
            Debug.LogError($"[RunnerManager] Invalid runner index: {index}");
            return;
        }
        
        // Deactivate all runners
        for (int i = 0; i < runners.Count; i++)
        {
            if (runners[i].runnerGameObject != null)
            {
                runners[i].runnerGameObject.SetActive(false);
                runners[i].isActive = false;
            }
        }
        
        // Activate selected runner
        RunnerConfig runner = runners[index];
        if (runner.runnerGameObject != null)
        {
            runner.runnerGameObject.SetActive(true);
            runner.isActive = true;
            currentRunnerInstance = runner.runnerGameObject;
            selectedRunner = runner.runnerName;
            
#if UNITY_EDITOR
            Debug.Log($"[RunnerManager] Activated: {runner.runnerName}");
#endif
            
            UpdateSystemReferences();
        }
    }

    public void SetActiveRunner(string runnerName)
    {
#if UNITY_EDITOR
        Debug.Log($"[RunnerManager] SetActiveRunner called: '{runnerName}'");
#endif
        
        // Find index by name and activate
        for (int i = 0; i < runners.Count; i++)
        {
            if (string.Equals(runners[i].runnerName, runnerName, System.StringComparison.OrdinalIgnoreCase))
            {
                s_SelectedRunnerIndex = i;
                ActivateRunnerByIndex(i);
                return;
            }
        }
        
        Debug.LogWarning($"[RunnerManager] Runner '{runnerName}' not found!");
    }


    private void UpdateSystemReferences()
    {
        // Ensure runner is tagged as Player
        if (currentRunnerInstance != null)
        {
            currentRunnerInstance.tag = "Player";
        }

        // Update CameraFollowRunner reference
        CameraFollowRunner camera = FindObjectOfType<CameraFollowRunner>();
        if (camera != null && currentRunnerInstance != null)
        {
            camera.target = currentRunnerInstance.transform;
        }

        // Update FailRunnerPreview reference (for pause/fail panels)
        FailRunnerPreview failPreview = FindObjectOfType<FailRunnerPreview>();
        if (failPreview != null && currentRunnerInstance != null)
        {
            failPreview.runnerPrefabOrModel = currentRunnerInstance;
        }

        // Ensure the runner root has a trigger collider for pickups/collection
        if (currentRunnerInstance != null)
        {
            // Ensure the runner root has a trigger collider so OnTriggerEnter fires
            // for coin/powerup collection even if the model colliders are on child bones.
            bool hasTrigger = false;
            Collider[] cols = currentRunnerInstance.GetComponents<Collider>();
            foreach (Collider c in cols)
            {
                if (c.isTrigger)
                {
                    hasTrigger = true;
                    break;
                }
            }
            if (!hasTrigger)
            {
                SphereCollider sc = currentRunnerInstance.AddComponent<SphereCollider>();
                sc.radius = 0.8f;
                sc.isTrigger = true;
                Debug.Log("[RunnerManager] Auto-added root trigger collider to runner.");
            }
        }
    }

    public GameObject GetCurrentRunnerInstance()
    {
        return currentRunnerInstance;
    }

    public string GetSelectedRunner()
    {
        return selectedRunner;
    }

    public static string GetSavedRunner()
    {
        // Get saved index and map to runner name
        // RunnerSelectionManager saves as index, not string
        int savedIndex = PlayerPrefs.GetInt(SELECTED_RUNNER_KEY, 0);
        
        if (Instance != null && savedIndex >= 0 && savedIndex < Instance.runners.Count)
        {
            return Instance.runners[savedIndex].runnerName;
        }
        
        return "Amaka"; // Default
    }

    // Removed SaveRunnerSelection - use RunnerSelectionManager.SelectRunner() instead
    // to avoid conflicting PlayerPrefs data types
}

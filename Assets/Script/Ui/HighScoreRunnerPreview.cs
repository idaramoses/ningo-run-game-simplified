using UnityEngine;
using System.Collections;

/// <summary>
/// Handles spawning and playing the celebrate animation for the player model on the High Score screen.
/// Renders the 3D player model onto a RenderTexture shown in the Canvas.
/// </summary>
public class HighScoreRunnerPreview : MonoBehaviour
{
    [Header("References")]
    public Camera previewCamera;
    public GameObject runnerPrefabOrModel;
    public string celebrateStateName = "celebrate";

    [Header("Placement (local to this root)")]
    public Vector3 localSpawnPos = new Vector3(0f, -0.7f, 2.5f); // Centered and positioned forward
    public Vector3 localSpawnEuler = new Vector3(0f, 180f, 0f); // Face the camera
    public float scale = 1f;

    [Header("Layer")]
    public string previewLayerName = "PreviewRunner";

    private GameObject instance;
    private Animator anim;
    private bool isPlayingCelebrate = false;

    private void Update()
    {
        // If the animator drifts out of the celebrate state, force it back
        // (also covers a non-looping clip ending mid-pose)
        if (isPlayingCelebrate && anim != null)
        {
            AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
            if (!info.IsName(celebrateStateName))
                anim.Play(celebrateStateName, 0, 0f);
        }
    }

    private void OnEnable()
    {
        StartCoroutine(SetupRunnerDelayed());
    }

    private IEnumerator SetupRunnerDelayed()
    {
        // Wait a frame to ensure RunnerManager is fully ready
        yield return null;

        TryGetCurrentRunner();
        SpawnFresh();
        PlayCelebrate();
    }

    private bool TryGetCurrentRunner()
    {
        if (RunnerManager.Instance != null)
        {
            GameObject currentRunner = RunnerManager.Instance.GetCurrentRunnerInstance();
            if (currentRunner != null)
            {
                runnerPrefabOrModel = currentRunner;
                return true;
            }
        }
        
        GameObject staticPrefab = RunnerManager.GetSelectedRunnerPrefab();
        if (staticPrefab != null)
        {
            runnerPrefabOrModel = staticPrefab;
            return true;
        }
        
        return false;
    }

    private void OnDisable()
    {
        if (instance != null) Destroy(instance);
        instance = null;
        anim = null;
        isPlayingCelebrate = false;
    }

    private void SpawnFresh()
    {
        if (runnerPrefabOrModel == null)
        {
            Debug.LogWarning("[HighScoreRunnerPreview] runnerPrefabOrModel is null!");
            return;
        }

        if (instance != null) Destroy(instance);

        // Force source to be INACTIVE so the clone starts inactive and doesn't run Awake/OnEnable code yet
        bool wasActive = runnerPrefabOrModel.activeSelf;
        if (wasActive) runnerPrefabOrModel.SetActive(false);

        instance = Instantiate(runnerPrefabOrModel, transform);

        // Restore source state
        if (wasActive) runnerPrefabOrModel.SetActive(true);

        // Disable ALL scripts on the clone so they can't fight the preview
        // animation (e.g. SimplePlayerController forcing "idle" every frame)
        foreach (var mb in instance.GetComponentsInChildren<MonoBehaviour>(true))
        {
            mb.enabled = false;
        }

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
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        // Now activate the cleanly configured clone safely
        instance.SetActive(true);
    }

    public void PlayCelebrate()
    {
        if (anim == null) return;

        anim.updateMode = AnimatorUpdateMode.UnscaledTime;
        anim.speed = 1f;
        string stateToPlay = celebrateStateName;
        if (!anim.HasState(0, Animator.StringToHash(stateToPlay)))
        {
            if (anim.HasState(0, Animator.StringToHash("dance 1")))
            {
                stateToPlay = "dance 1";
            }
            else if (anim.HasState(0, Animator.StringToHash("dance 2")))
            {
                stateToPlay = "dance 2";
            }
            else if (anim.HasState(0, Animator.StringToHash("dance 3")))
            {
                stateToPlay = "dance 3";
            }
        }
        celebrateStateName = stateToPlay; // keep the resolved name for Update()
        isPlayingCelebrate = true;
        anim.Play(stateToPlay, 0, 0f);
        anim.Update(0f);
    }

    private static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}

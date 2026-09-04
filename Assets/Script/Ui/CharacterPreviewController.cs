using UnityEngine;
using System.Collections;

/// <summary>
/// Manages the 3D preview model shown in the Character selection UI.
/// Spawns the selected character model and plays its idle animation.
/// </summary>
public class CharacterPreviewController : MonoBehaviour
{
    public static CharacterPreviewController Instance { get; private set; }

    [Header("Placement Settings")]
    public Vector3 localSpawnPos = new Vector3(0f, -0.7f, 2.5f); // Same as high score preview
    public Vector3 localSpawnEuler = new Vector3(0f, 180f, 0f); // Facing the camera
    public float scale = 1f;

    [Header("Layer Name")]
    public string previewLayerName = "PreviewRunner";

    [Header("Animation State")]
    public string idleStateName = "idle";

    private GameObject currentInstance;
    private Animator currentAnim;

    private void Awake()
    {
        Instance = this;
        ClearChildren();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void OnDisable()
    {
        ClearChildren();
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i).gameObject;
            if (Application.isPlaying)
                Destroy(child);
            else
                DestroyImmediate(child);
        }
        currentInstance = null;
        currentAnim = null;
    }

    public void UpdatePreview(GameObject prefab, bool playCelebrate = false)
    {
        // 1. Clean up any existing preview models completely
        ClearChildren();

        if (prefab == null) return;

        // 2. Spawn the prefab as a child of this object
        currentInstance = Instantiate(prefab, transform);

        // 3. Disable components we don't want running in the UI preview
        var charController = currentInstance.GetComponent<CharacterController>();
        if (charController != null) charController.enabled = false;

        var monoBehaviours = currentInstance.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var mb in monoBehaviours)
        {
            if (mb != null) mb.enabled = false;
        }

        var colliders = currentInstance.GetComponentsInChildren<Collider>(true);
        foreach (var col in colliders)
        {
            if (col != null) col.enabled = false;
        }

        var rigidbodies = currentInstance.GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in rigidbodies)
        {
            if (rb != null) rb.isKinematic = true;
        }

        var audioSources = currentInstance.GetComponentsInChildren<AudioSource>(true);
        foreach (var audio in audioSources)
        {
            if (audio != null) audio.enabled = false;
        }

        // 4. Set positioning and scale
        currentInstance.transform.localPosition = localSpawnPos;
        currentInstance.transform.localRotation = Quaternion.Euler(localSpawnEuler);
        currentInstance.transform.localScale = Vector3.one * scale;

        // 5. Recursively assign the preview layer so only the preview camera renders it
        int layer = LayerMask.NameToLayer(previewLayerName);
        if (layer == -1) layer = LayerMask.NameToLayer("mmk");
        if (layer == -1) layer = 8;
        
        SetLayerRecursively(currentInstance, layer);

        // Ensure preview camera is enabled and matches the layer
        Camera cam = GetComponentInChildren<Camera>(true);
        if (cam == null && transform.parent != null)
            cam = transform.parent.GetComponentInChildren<Camera>(true);
        if (cam != null)
        {
            cam.enabled = true;
            cam.cullingMask = 1 << layer;
        }

        // Ensure preview light is enabled and illuminates the layer
        Light light = GetComponentInChildren<Light>(true);
        if (light == null && transform.parent != null)
            light = transform.parent.GetComponentInChildren<Light>(true);
        if (light != null)
        {
            light.enabled = true;
            light.cullingMask = 1 << layer;
        }

        // Now activate the cleanly configured clone safely before playing any animation!
        currentInstance.SetActive(true);

        // 6. Get and configure Animator to play idle or celebrate
        currentAnim = currentInstance.GetComponentInChildren<Animator>();
        if (currentAnim != null)
        {
            currentAnim.Rebind();
            currentAnim.updateMode = AnimatorUpdateMode.UnscaledTime;
            string stateToPlay = idleStateName;
            if (playCelebrate)
            {
                if (currentAnim.HasState(0, Animator.StringToHash("celebrate")))
                {
                    stateToPlay = "celebrate";
                }
                else if (currentAnim.HasState(0, Animator.StringToHash("dance 1")))
                {
                    stateToPlay = "dance 1";
                }
                else if (currentAnim.HasState(0, Animator.StringToHash("dance 2")))
                {
                    stateToPlay = "dance 2";
                }
                else if (currentAnim.HasState(0, Animator.StringToHash("dance 3")))
                {
                    stateToPlay = "dance 3";
                }
                else
                {
                    stateToPlay = "celebrate"; // Fallback
                }
            }
            currentAnim.Play(stateToPlay, 0, 0f);
            currentAnim.Update(0.02f);
        }
    }

    private static void SetLayerRecursively(GameObject obj, int layer)
    {
        if (obj == null) return;
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
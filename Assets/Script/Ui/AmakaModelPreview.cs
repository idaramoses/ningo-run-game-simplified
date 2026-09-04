using UnityEngine;

public class AmakaModelPreview : MonoBehaviour
{
    [Header("References")]
    public Camera previewCamera;
    public GameObject amakaModelPrefab;
    public string idleStateName = "idle";

    [Header("Placement (local to this root)")]
    public Vector3 localSpawnPos = Vector3.zero;
    public Vector3 localSpawnEuler = new Vector3(0, 180, 0);
    public float scale = 1f;

    [Header("Layer")]
    public string previewLayerName = "PreviewRunner";

    private GameObject instance;
    private Animator anim;

    private void OnEnable()
    {
        SpawnFresh();
    }

    private void OnDisable()
    {
        if (instance != null) Destroy(instance);
        instance = null;
        anim = null;
    }

    private void SpawnFresh()
    {
        if (amakaModelPrefab == null)
        {
            Debug.LogError("[AmakaModelPreview] amakaModelPrefab not assigned.");
            return;
        }

        if (instance != null) Destroy(instance);

        instance = Instantiate(amakaModelPrefab, transform);
        instance.transform.localPosition = localSpawnPos;
        instance.transform.localRotation = Quaternion.Euler(localSpawnEuler);
        instance.transform.localScale = Vector3.one * scale;

        int layer = LayerMask.NameToLayer(previewLayerName);
        if (layer != -1)
            SetLayerRecursively(instance, layer);

        anim = instance.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
            anim.Play(idleStateName, 0, 0f);
        }
    }

    public void PlayIdle()
    {
        if (anim == null) return;
        
        anim.updateMode = AnimatorUpdateMode.UnscaledTime;
        anim.speed = 1f;
        anim.Play(idleStateName, 0, 0f);
        anim.Update(0f);
    }

    private static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}

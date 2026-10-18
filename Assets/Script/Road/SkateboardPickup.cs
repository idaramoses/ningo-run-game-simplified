using UnityEngine;

public class SkateboardPickup : MonoBehaviour
{
    [Header("Settings")]
    public float duration = 15f;
    public GameObject collectEffectPrefab;
    public AudioClip collectSound;

    private bool isCollected = false;

    private void OnEnable()
    {
        isCollected = false;
    }

    public void Collect(SimplePlayerController player = null)
    {
        if (isCollected) return;
        isCollected = true;

        if (player == null)
            player = SimplePlayerController.Instance ?? Object.FindAnyObjectByType<SimplePlayerController>();

        if (player != null)
        {
            player.ActivateSkateboard(duration);
        }

        if (SoundEffectsManager.Instance != null)
        {
            if (collectSound != null)
                SoundEffectsManager.Instance.PlaySound(collectSound);
            else
                SoundEffectsManager.Instance.PlayLetterCollect();

            if (collectEffectPrefab != null)
            {
                GameObject effect = Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            else if (SoundEffectsManager.Instance.coinCollectEffectPrefab != null)
            {
                SoundEffectsManager.Instance.SpawnCoinCollectEffect(transform.position);
            }
        }

#if UNITY_EDITOR
        Debug.Log($"[SkateboardPickup] Collected! Duration: {duration}s");
#endif

        if (ObjectPoolManager.Instance != null)
            ObjectPoolManager.Instance.ReturnToPool(gameObject);
        else
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(gameObject);
            else
                Destroy(gameObject);
#else
            Destroy(gameObject);
#endif
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") ||
            other.GetComponent<SimpleRunner>() != null ||
            other.GetComponentInParent<SimpleRunner>() != null ||
            other.GetComponentInParent<SimplePlayerController>() != null)
        {
            SimplePlayerController spc = other.GetComponentInParent<SimplePlayerController>();
            Collect(spc);
        }
    }
}

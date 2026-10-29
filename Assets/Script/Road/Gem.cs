using UnityEngine;

public class Gem : MonoBehaviour
{
    [Header("Settings")]
    public int gemValue = 1;
    public GameObject collectEffectPrefab;
    public AudioClip collectSound;

    private bool isCollected = false;

    private void OnEnable()
    {
        isCollected = false;
    }

    public bool Collect()
    {
        if (isCollected) return false;
        isCollected = true;

        // Play audio & particle effects
        if (SoundEffectsManager.Instance != null)
        {
            if (collectSound != null)
            {
                SoundEffectsManager.Instance.PlaySound(collectSound);
            }
            else
            {
                SoundEffectsManager.Instance.PlayLetterCollect();
            }

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

        // Update player gems
        int totalGems = PlayerPrefs.GetInt("Gems", 0) + gemValue;
        PlayerPrefs.SetInt("Gems", totalGems);

        int runGems = PlayerPrefs.GetInt("RunGems", 0) + gemValue;
        PlayerPrefs.SetInt("RunGems", runGems);

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddGems(gemValue);
        }

        // Refresh UI
        if (GameHUDController.Instance != null)
        {
            GameHUDController.Instance.ForceRefreshCoins();
        }
        else
        {
            GameHUDController hud = Object.FindAnyObjectByType<GameHUDController>();
            if (hud != null) hud.ForceRefreshCoins();
        }

        TopPanelController top = Object.FindAnyObjectByType<TopPanelController>();
        if (top != null) top.UpdateUI();

#if UNITY_EDITOR
        Debug.Log($"[Gem] Collected! Total: {totalGems}, Run: {runGems}");
#endif

        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.ReturnToPool(gameObject);
        }
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

        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") ||
            other.GetComponent<SimpleRunner>() != null ||
            other.GetComponentInParent<SimpleRunner>() != null ||
            other.GetComponentInParent<SimplePlayerController>() != null)
        {
            Collect();
        }
    }
}

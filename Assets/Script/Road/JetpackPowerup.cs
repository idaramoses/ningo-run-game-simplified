using UnityEngine;

public class JetpackPowerup : MonoBehaviour
{
    [Header("Settings")]
    public float duration = 10f;
    public GameObject collectEffectPrefab;
    public AudioClip collectSound;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player collided with the jetpack
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        Debug.Log("[JetpackPowerup] Collected by player!");

        // Spawn collect particles if assigned
        if (collectEffectPrefab != null)
        {
            GameObject effect = Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        else if (SoundEffectsManager.Instance != null && SoundEffectsManager.Instance.coinCollectEffectPrefab != null)
        {
            SoundEffectsManager.Instance.SpawnCoinCollectEffect(transform.position);
        }

        // Play sound if assigned
        if (collectSound != null && SoundEffectsManager.Instance != null)
        {
            SoundEffectsManager.Instance.PlaySound(collectSound);
        }
        else if (SoundEffectsManager.Instance != null)
        {
            SoundEffectsManager.Instance.PlayLetterCollect(); // Use letter collect as a nice powerup sound fallback
        }

        // Return to pool if available, otherwise destroy
        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.ReturnToPool(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

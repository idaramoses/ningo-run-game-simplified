using UnityEngine;

public class Coin : MonoBehaviour
{
    public bool isPremium = false;
    private bool isCollected = false;

    private void OnEnable()
    {
        isCollected = false;
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the entering object is the player
        if (other.CompareTag("Player") ||
            other.GetComponent<SimpleRunner>() != null ||
            other.GetComponentInParent<SimpleRunner>() != null)
        {
            Collect();
        }
    }

    public bool Collect()
    {
        if (isCollected) return false;
        isCollected = true;

        if (SoundEffectsManager.Instance != null)
        {
            SoundEffectsManager.Instance.PlayCoinCollect();
            SoundEffectsManager.Instance.SpawnCoinCollectEffect(transform.position);
        }

        int totalCoins = PlayerPrefs.GetInt("Coins", 0);
        totalCoins++;
        PlayerPrefs.SetInt("Coins", totalCoins);

        int runCoins = PlayerPrefs.GetInt("RunCoins", 0);
        runCoins++;
        PlayerPrefs.SetInt("RunCoins", runCoins);
        // PlayerPrefs.Save(); is removed to prevent performance micro-stutters during gameplay. UImanager saves on run end.

#if UNITY_EDITOR
        Debug.Log($"[Coin] Collected! Total: {totalCoins}, Run: {runCoins}");
#endif

        // Progress daily mission
        MissionsController.ProgressMission(MissionData.MissionType.CollectCoins, 1);

        // Force UI refresh so the HUD updates immediately
        GameHUDController hud = Object.FindFirstObjectByType<GameHUDController>();
        if (hud != null) hud.ForceRefreshCoins();
        if (autoscale.atsc != null) autoscale.atsc.toscale();
        
        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.ReturnToPool(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        return true;
    }
}

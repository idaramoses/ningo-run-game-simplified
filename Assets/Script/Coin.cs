using UnityEngine;

public class Coin : MonoBehaviour
{
    public bool isPremium = false;
    private bool isCollected = false;

    private Transform magnetTarget = null;
    private float magnetFlySpeed = 20f;
    private bool isBeingAttracted = false;

    private void OnEnable()
    {
        isCollected = false;
        isBeingAttracted = false;
        magnetTarget = null;
    }

    private void Update()
    {
        if (isCollected) return;

        if (isBeingAttracted && magnetTarget != null)
        {
            // Accelerate towards player magnet point
            magnetFlySpeed += 30f * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, magnetTarget.position + Vector3.up * 0.8f, magnetFlySpeed * Time.deltaTime);

            float dist = Vector3.Distance(transform.position, magnetTarget.position + Vector3.up * 0.8f);
            if (dist < 0.8f)
            {
                Collect();
            }
        }
    }

    public void AttractTo(Transform target)
    {
        if (isCollected || isBeingAttracted) return;
        magnetTarget = target;
        isBeingAttracted = true;
        magnetFlySpeed = 18f;
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

        if (CoinPickupEffectManager.Instance != null)
        {
            CoinPickupEffectManager.Instance.PlayCoinPickupAnimation(transform.position, 1);
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
        if (GameHUDController.Instance != null)
        {
            GameHUDController.Instance.ForceRefreshCoins();
        }
        else
        {
            GameHUDController hud = Object.FindAnyObjectByType<GameHUDController>();
            if (hud != null) hud.ForceRefreshCoins();
        }
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

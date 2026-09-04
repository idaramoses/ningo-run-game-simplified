using UnityEngine;

public class PlayerCollector : MonoBehaviour
{
    public int coins = 0;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[PlayerCollector] Triggered by: {other.gameObject.name} with tag: {other.tag}");

        if (other.CompareTag("Coin"))
        {
            var coinComponent = other.GetComponent<Coin>();
            if (coinComponent == null) coinComponent = other.GetComponentInParent<Coin>();
            
            if (coinComponent != null)
            {
                // Call central Collect() which handles validation, play-sfx, PlayerPrefs incrementing, and object destruction
                if (coinComponent.Collect())
                {
                    coins++;
                }
            }
            else
            {
                // Fallback if coin has no Coin component attached
                if (SoundEffectsManager.Instance != null)
                {
                    SoundEffectsManager.Instance.PlayCoinCollect();
                    SoundEffectsManager.Instance.SpawnCoinCollectEffect(other.transform.position);
                }

                coins++;

                int totalCoins = PlayerPrefs.GetInt("Coins", 0);
                totalCoins++;
                PlayerPrefs.SetInt("Coins", totalCoins);

                int runCoins = PlayerPrefs.GetInt("RunCoins", 0);
                runCoins++;
                PlayerPrefs.SetInt("RunCoins", runCoins);
                // PlayerPrefs.Save(); is removed to prevent performance micro-stutters during gameplay. UImanager saves on run end.

                Debug.Log($"[PlayerCollector] Collected coin (Fallback)! Session: {coins}, Total: {totalCoins}");

                // Progress daily mission
                MissionsController.ProgressMission(MissionData.MissionType.CollectCoins, 1);
                Destroy(other.gameObject);
            }
        }
    }
}

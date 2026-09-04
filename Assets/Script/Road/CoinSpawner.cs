using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("References")]
    public Transform coinsParent;
    public GameObject coinPrefab;

    [Header("Spawn Settings")]
    [Range(0f, 1f)]
    public float coinSpawnChance = 0.6f; // 60% chance per road segment
    public int minCoinsPerSegment = 3;
    public int maxCoinsPerSegment = 8;

    [Header("Placement")]
    public float laneDistance = 2.5f; // lane spacing used by runner
    public float coinYOffset = 1.0f;
    public float coinSpacing = 2.5f; // Distance between coins in a line
    public float zMargin = 4f; // Keep away from segment edges

    private readonly List<GameObject> spawnedCoins = new List<GameObject>();

    public void SpawnCoinsOnSegment(Transform roadSegment, float segmentStartZ, float segmentLength)
    {
        if (coinPrefab == null)
        {
            Debug.LogWarning("[CoinSpawner] No coin prefab assigned!");
            return;
        }

        // Random chance to spawn coins on this segment
        if (Random.value > coinSpawnChance)
        {
                return;
        }

        int coinCount = Random.Range(minCoinsPerSegment, maxCoinsPerSegment + 1);
        int pattern = Random.Range(0, 3); // 0=line, 1=zigzag, 2=scattered


        switch (pattern)
        {
            case 0:
                SpawnCoinLine(roadSegment, segmentStartZ, segmentLength, coinCount);
                break;
            case 1:
                SpawnCoinZigzag(roadSegment, segmentStartZ, segmentLength, coinCount);
                break;
            case 2:
                SpawnCoinScattered(roadSegment, segmentStartZ, segmentLength, coinCount);
                break;
        }
    }

    private void SpawnCoinLine(Transform roadSegment, float segmentStartZ, float segmentLength, int count)
    {
        // Pick a random lane (0, 1, or 2)
        int lane = Random.Range(0, 3);
        float laneX = (lane - 1) * laneDistance;

        float startZ = segmentStartZ + zMargin;
        float endZ = segmentStartZ + segmentLength - zMargin;
        float availableLength = endZ - startZ;
        float spacing = availableLength / (count + 1);

        for (int i = 0; i < count; i++)
        {
            float z = startZ + spacing * (i + 1);
            Vector3 pos = new Vector3(laneX, coinYOffset, z);
            SpawnCoin(pos, roadSegment);
        }
    }

    private void SpawnCoinZigzag(Transform roadSegment, float segmentStartZ, float segmentLength, int count)
    {
        float startZ = segmentStartZ + zMargin;
        float endZ = segmentStartZ + segmentLength - zMargin;
        float availableLength = endZ - startZ;
        float spacing = availableLength / (count + 1);

        int currentLane = 1; // Start in center

        for (int i = 0; i < count; i++)
        {
            float laneX = (currentLane - 1) * laneDistance;
            float z = startZ + spacing * (i + 1);
            Vector3 pos = new Vector3(laneX, coinYOffset, z);
            SpawnCoin(pos, roadSegment);

            // Alternate lanes
            currentLane = (currentLane == 0) ? 2 : (currentLane == 2) ? 0 : Random.Range(0, 3);
        }
    }

    private void SpawnCoinScattered(Transform roadSegment, float segmentStartZ, float segmentLength, int count)
    {
        float startZ = segmentStartZ + zMargin;
        float endZ = segmentStartZ + segmentLength - zMargin;

        for (int i = 0; i < count; i++)
        {
            int lane = Random.Range(0, 3);
            float laneX = (lane - 1) * laneDistance;
            float z = Random.Range(startZ, endZ);
            Vector3 pos = new Vector3(laneX, coinYOffset, z);
            SpawnCoin(pos, roadSegment);
        }
    }

    private void SpawnCoin(Vector3 position, Transform parent)
    {
        GameObject coin = Instantiate(coinPrefab, position, Quaternion.identity, coinsParent != null ? coinsParent : parent);
        coin.name = "Coin"; // keep name clean for object-pool lookup
        coin.tag = "Coin";

        // Make every collider a trigger so the player can run through the coin.
        Collider[] allColliders = coin.GetComponentsInChildren<Collider>();
        if (allColliders.Length == 0)
        {
            allColliders = new Collider[] { coin.AddComponent<SphereCollider>() };
        }
        foreach (Collider c in allColliders)
        {
            if (c != null) c.isTrigger = true;
        }

        // Ensure the coin has the pickup script on a GameObject with a trigger collider.
        Coin coinPickup = coin.GetComponent<Coin>();
        if (coinPickup == null) coinPickup = coin.GetComponentInChildren<Coin>();
        if (coinPickup == null)
        {
            coinPickup = coin.AddComponent<Coin>();
            if (coin.GetComponent<Collider>() == null)
            {
                SphereCollider sc = coin.AddComponent<SphereCollider>();
                sc.isTrigger = true;
            }
        }
        else
        {
            Collider coinCol = coinPickup.GetComponent<Collider>();
            if (coinCol == null) coinCol = coinPickup.gameObject.AddComponent<SphereCollider>();
            coinCol.isTrigger = true;
            coinPickup.gameObject.tag = "Coin";
        }

        spawnedCoins.Add(coin);
    }

    public void CleanupOldCoins(Vector3 playerPosition, float cleanupDistance)
    {
        for (int i = spawnedCoins.Count - 1; i >= 0; i--)
        {
            if (spawnedCoins[i] == null)
            {
                spawnedCoins.RemoveAt(i);
                continue;
            }

            if (spawnedCoins[i].transform.position.z < playerPosition.z - cleanupDistance)
            {
                Destroy(spawnedCoins[i]);
                spawnedCoins.RemoveAt(i);
            }
        }
    }
}

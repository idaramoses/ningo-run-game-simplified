using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Simple endless road spawner.
/// Put this on an empty GameObject in your scene, then disable the old Makesupway script.
/// </summary>
public class RoadSpawner : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Drag the player here. If empty, it finds Playermuving automatically.")]
    public Transform player;

    [Header("Road")]
    [Tooltip("The road already placed in the scene where the player starts.")]
    public GameObject starterRoad;

    [Tooltip("Road tile prefabs that will be spawned ahead of the player.")]
    public GameObject[] roadTilePrefabs;
    public GameObject[] roadPrefabs => roadTilePrefabs;

    [Tooltip("Length of one road tile along Z.")]
    public float tileLength = 40f;

    [Tooltip("How far ahead of the player to keep spawning new road tiles.")]
    public float spawnDistanceAhead = 120f;

    [Tooltip("How far behind the player to keep road tiles before deleting them.")]
    public float removeDistanceBehind = 80f;

    [Header("Obstacles")]
    [Tooltip("Chance (0 = none, 1 = every tile) to spawn an obstacle on a new road tile.")]
    [Range(0f, 1f)]
    public float obstacleChance = 0.4f;

    [Tooltip("Obstacle prefabs to pick from.")]
    public GameObject[] obstaclePrefabs;

    [Tooltip("Lane X positions where obstacles can spawn. Leave empty for a single center lane.")]
    public float[] obstacleLanes = { -2f, 0f, 2f };

    [Header("Coins")]
    [Tooltip("Coin prefab to spawn on road tiles.")]
    public GameObject coinPrefab;

    [Tooltip("Chance (0 = none, 1 = every tile) to spawn coins on a new road tile.")]
    [Range(0f, 1f)]
    public float coinSpawnChance = 0.5f;

    [Tooltip("Minimum number of coins to spawn on a tile.")]
    public int minCoinsPerTile = 3;

    [Tooltip("Maximum number of coins to spawn on a tile.")]
    public int maxCoinsPerTile = 6;

    [Tooltip("Lane X positions where coins can spawn.")]
    public float[] coinLanes = { -2.5f, 0f, 2.5f };

    [Tooltip("Height above the road where coins are placed.")]
    public float coinYOffset = 1f;

    [Tooltip("Keep coins away from the start/end edges of a tile.")]
    public float coinZMargin = 4f;

    float nextSpawnZ;
    Queue<GameObject> activeRoads = new Queue<GameObject>();

    /// <summary>
    /// Swap the road tiles used for future spawns (called by CityManager when
    /// the player changes city). Empty/null arrays keep the current tiles.
    /// </summary>
    public void SetRoadTiles(GameObject[] tiles)
    {
        if (tiles == null || tiles.Length == 0) return;
        roadTilePrefabs = tiles;
    }

    void Start()
    {
        if (player == null)
        {
            Playermuving pm = FindObjectOfType<Playermuving>();
            if (pm != null) player = pm.transform;
        }

        if (starterRoad != null)
        {
            nextSpawnZ = starterRoad.transform.position.z + tileLength;
        }
        else
        {
            nextSpawnZ = transform.position.z;
        }
    }

    void Update()
    {
        if (player == null || roadTilePrefabs.Length == 0) return;

        // Spawn road tiles ahead of the player.
        while (player.position.z + spawnDistanceAhead > nextSpawnZ)
        {
            SpawnRoad(nextSpawnZ);
            nextSpawnZ += tileLength;
        }

        // Remove road tiles that are far behind the player.
        while (activeRoads.Count > 0)
        {
            GameObject road = activeRoads.Peek();
            if (road == null)
            {
                activeRoads.Dequeue();
                continue;
            }

            if (road.transform.position.z < player.position.z - removeDistanceBehind)
            {
                Destroy(road);
                activeRoads.Dequeue();
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// Destroys every spawned road tile and rewinds the spawn cursor back to
    /// just after the starter road, so a restart builds a fresh road from scratch.
    /// Obstacles/coins are children of the tiles, so they are cleaned up too.
    /// </summary>
    public void ResetSpawner()
    {
        while (activeRoads.Count > 0)
        {
            GameObject road = activeRoads.Dequeue();
            if (road != null) Destroy(road);
        }

        nextSpawnZ = starterRoad != null
            ? starterRoad.transform.position.z + tileLength
            : transform.position.z;
    }

    void SpawnRoad(float z)
    {
        int index = Random.Range(0, roadTilePrefabs.Length);
        GameObject prefab = roadTilePrefabs[index];
        if (prefab == null) return;

        Vector3 position = new Vector3(0, 0, z);
        GameObject road = Instantiate(prefab, position, Quaternion.identity);

        // Sometimes spawn an obstacle on this road tile.
        if (obstaclePrefabs.Length > 0 && Random.value < obstacleChance)
        {
            int obsIndex = Random.Range(0, obstaclePrefabs.Length);
            GameObject obsPrefab = obstaclePrefabs[obsIndex];
            if (obsPrefab != null)
            {
                float laneX = 0f;
                if (obstacleLanes.Length > 0)
                {
                    laneX = obstacleLanes[Random.Range(0, obstacleLanes.Length)];
                }

                Vector3 obsPos = road.transform.position + new Vector3(laneX, 0, 0);
                GameObject obstacle = Instantiate(obsPrefab, obsPos, Quaternion.identity);
                obstacle.transform.SetParent(road.transform);
            }
        }

        // Spawn coins on this road tile.
        if (coinPrefab != null && Random.value < coinSpawnChance)
        {
            SpawnCoinsOnRoad(road, z);
        }

        activeRoads.Enqueue(road);
    }

    void SpawnCoinsOnRoad(GameObject road, float roadStartZ)
    {
        if (coinLanes.Length == 0) return;

        int coinCount = Random.Range(minCoinsPerTile, maxCoinsPerTile + 1);
        int pattern = Random.Range(0, 3); // 0 = straight line, 1 = zigzag, 2 = scattered

        switch (pattern)
        {
            case 0:
                SpawnCoinLine(road, roadStartZ, coinCount);
                break;
            case 1:
                SpawnCoinZigzag(road, roadStartZ, coinCount);
                break;
            case 2:
                SpawnCoinScattered(road, roadStartZ, coinCount);
                break;
        }
    }

    void SpawnCoinLine(GameObject road, float roadStartZ, int count)
    {
        int lane = Random.Range(0, coinLanes.Length);
        float laneX = coinLanes[lane];

        float startZ = roadStartZ + coinZMargin;
        float endZ = roadStartZ + tileLength - coinZMargin;
        float spacing = (endZ - startZ) / (count + 1);

        for (int i = 0; i < count; i++)
        {
            float z = startZ + spacing * (i + 1);
            Vector3 pos = new Vector3(laneX, coinYOffset, z);
            SpawnCoin(pos, road.transform);
        }
    }

    void SpawnCoinZigzag(GameObject road, float roadStartZ, int count)
    {
        float startZ = roadStartZ + coinZMargin;
        float endZ = roadStartZ + tileLength - coinZMargin;
        float spacing = (endZ - startZ) / (count + 1);

        int currentLane = coinLanes.Length / 2; // start in the center

        for (int i = 0; i < count; i++)
        {
            float laneX = coinLanes[currentLane];
            float z = startZ + spacing * (i + 1);
            Vector3 pos = new Vector3(laneX, coinYOffset, z);
            SpawnCoin(pos, road.transform);

            // Alternate lanes: left -> right, right -> left, center -> random
            if (currentLane == 0)
                currentLane = coinLanes.Length - 1;
            else if (currentLane == coinLanes.Length - 1)
                currentLane = 0;
            else
                currentLane = Random.Range(0, coinLanes.Length);
        }
    }

    void SpawnCoinScattered(GameObject road, float roadStartZ, int count)
    {
        float startZ = roadStartZ + coinZMargin;
        float endZ = roadStartZ + tileLength - coinZMargin;

        for (int i = 0; i < count; i++)
        {
            int lane = Random.Range(0, coinLanes.Length);
            float laneX = coinLanes[lane];
            float z = Random.Range(startZ, endZ);
            Vector3 pos = new Vector3(laneX, coinYOffset, z);
            SpawnCoin(pos, road.transform);
        }
    }

    void SpawnCoin(Vector3 position, Transform parent)
    {
        GameObject coin = Instantiate(coinPrefab, position, Quaternion.identity, parent);
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
    }
}

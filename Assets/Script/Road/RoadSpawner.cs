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

    [Header("Pickups & Powerups")]
    [Tooltip("Gem prefab to spawn on road tiles.")]
    public GameObject gemPrefab;
    [Range(0f, 1f)]
    public float gemSpawnChance = 0.25f; // 25% chance per tile
    public float gemYOffset = 1.0f;

    [Tooltip("Skateboard pickup prefab.")]
    public GameObject skateboardPrefab;
    [Range(0f, 1f)]
    public float skateboardSpawnChance = 0.15f; // 15% chance per tile
    public float skateboardYOffset = 0.8f;

    [Tooltip("Hang Glider pickup prefab.")]
    public GameObject gliderPrefab;
    [Range(0f, 1f)]
    public float gliderSpawnChance = 0.15f; // 15% chance per tile
    public float gliderYOffset = 1.2f;

    [Tooltip("Coin Magnet pickup prefab.")]
    public GameObject magnetPrefab;
    [Range(0f, 1f)]
    public float magnetSpawnChance = 0.15f; // 15% chance per tile
    public float magnetYOffset = 1.0f;

    [Header("Glider Sky Pickups")]
    [Tooltip("Sky altitude for coins and gems during glider flight (matches SimplePlayerController.gliderHeight).")]
    public float skyPickupAltitude = 7.5f;

    float nextSpawnZ;
    int lightGateFrames;
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

    /// <summary>
    /// Swaps the tile prefabs AND rebuilds every already-spawned road tile
    /// at the same position, so a city change applies immediately instead
    /// of only affecting tiles spawned afterwards.
    /// </summary>
    public void ReplaceActiveTiles(GameObject[] tiles)
    {
        if (tiles == null || tiles.Length == 0) return;
        roadTilePrefabs = tiles;

        // Spawn replacements first so the road is never missing under the player.
        List<KeyValuePair<GameObject, GameObject>> oldAndNew = new List<KeyValuePair<GameObject, GameObject>>();
        foreach (var road in activeRoads)
        {
            if (road == null) continue;
            int index = Random.Range(0, roadTilePrefabs.Length);
            GameObject prefab = roadTilePrefabs[index];
            if (prefab == null) continue;
            GameObject replacement = Instantiate(prefab, road.transform.position, Quaternion.identity);
            if (LowQuality) DisableLocalLightsOn(replacement);
            if (CurvedWorldManager.Instance != null)
            {
                CurvedWorldManager.Instance.ApplyCurvatureToGameObject(replacement);
            }
            oldAndNew.Add(new KeyValuePair<GameObject, GameObject>(road, replacement));
        }
        activeRoads.Clear();

        foreach (var pair in oldAndNew)
        {
            Destroy(pair.Key);
            activeRoads.Enqueue(pair.Value);
        }
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

        DisableLocalLightsIfLowQuality();
    }

    /// <summary>
    /// Each street lamp carries point + spot lights (~170 realtime lights on a
    /// full stretch of road). They cost several ms per frame and are invisible
    /// in daylight — the emissive material already sells the "lit" look — so on
    /// the lower quality tiers we turn every non-directional light off.
    /// </summary>
    static bool LowQuality => QualitySettings.GetQualityLevel() <= 2;

    static void DisableLocalLightsIfLowQuality()
    {
        if (!LowQuality) return;
        foreach (Light l in FindObjectsOfType<Light>())
            if (l.type != LightType.Directional && l.enabled) l.enabled = false;
    }

    static void DisableLocalLightsOn(GameObject root)
    {
        foreach (Light l in root.GetComponentsInChildren<Light>())
            if (l.type != LightType.Directional && l.enabled) l.enabled = false;
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

        // Lamp lights can arrive through other paths too (starter-group swaps
        // on city change) — sweep roughly once a second and shut them back off.
        if (LowQuality && ++lightGateFrames >= 60)
        {
            lightGateFrames = 0;
            DisableLocalLightsIfLowQuality();
        }

        // Smoothly adjust active coins & gems height based on whether player is gliding or running
        UpdatePickupHeights();

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

        // New tiles bring fresh lamp lights — keep them gated on low quality.
        if (LowQuality) DisableLocalLightsOn(road);

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

        // Spawn other road pickups (gems, skateboard, glider)
        SpawnPickupsOnRoad(road, z);

        if (CurvedWorldManager.Instance != null)
        {
            CurvedWorldManager.Instance.ApplyCurvatureToGameObject(road);
        }

        activeRoads.Enqueue(road);
    }

    void SpawnCoinsOnRoad(GameObject road, float roadStartZ)
    {
        if (coinLanes.Length == 0) return;

        bool isGliding = SimplePlayerController.Instance != null && SimplePlayerController.Instance.IsGliding;
        float currentCoinY = isGliding ? skyPickupAltitude : coinYOffset;

        int coinCount = Random.Range(minCoinsPerTile, maxCoinsPerTile + 1);
        int pattern = Random.Range(0, 3); // 0 = straight line, 1 = zigzag, 2 = scattered

        switch (pattern)
        {
            case 0:
                SpawnCoinLine(road, roadStartZ, coinCount, currentCoinY);
                break;
            case 1:
                SpawnCoinZigzag(road, roadStartZ, coinCount, currentCoinY);
                break;
            case 2:
                SpawnCoinScattered(road, roadStartZ, coinCount, currentCoinY);
                break;
        }
    }

    void SpawnCoinLine(GameObject road, float roadStartZ, int count, float yHeight)
    {
        int lane = Random.Range(0, coinLanes.Length);
        float laneX = coinLanes[lane];

        float startZ = roadStartZ + coinZMargin;
        float endZ = roadStartZ + tileLength - coinZMargin;
        float spacing = (endZ - startZ) / (count + 1);

        for (int i = 0; i < count; i++)
        {
            float z = startZ + spacing * (i + 1);
            Vector3 pos = new Vector3(laneX, yHeight, z);
            SpawnCoin(pos, road.transform);
        }
    }

    void SpawnCoinZigzag(GameObject road, float roadStartZ, int count, float yHeight)
    {
        float startZ = roadStartZ + coinZMargin;
        float endZ = roadStartZ + tileLength - coinZMargin;
        float spacing = (endZ - startZ) / (count + 1);

        int currentLane = coinLanes.Length / 2; // start in the center

        for (int i = 0; i < count; i++)
        {
            float laneX = coinLanes[currentLane];
            float z = startZ + spacing * (i + 1);
            Vector3 pos = new Vector3(laneX, yHeight, z);
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

    void SpawnCoinScattered(GameObject road, float roadStartZ, int count, float yHeight)
    {
        float startZ = roadStartZ + coinZMargin;
        float endZ = roadStartZ + tileLength - coinZMargin;

        for (int i = 0; i < count; i++)
        {
            int lane = Random.Range(0, coinLanes.Length);
            float laneX = coinLanes[lane];
            float z = Random.Range(startZ, endZ);
            Vector3 pos = new Vector3(laneX, yHeight, z);
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

    void UpdatePickupHeights()
    {
        bool isGliding = SimplePlayerController.Instance != null && SimplePlayerController.Instance.IsGliding;
        float targetCoinY = isGliding ? skyPickupAltitude : coinYOffset;
        float targetGemY = isGliding ? skyPickupAltitude : gemYOffset;

        foreach (var road in activeRoads)
        {
            if (road == null) continue;
            if (player != null && road.transform.position.z + tileLength < player.position.z - 5f) continue;

            Coin[] coins = road.GetComponentsInChildren<Coin>();
            for (int i = 0; i < coins.Length; i++)
            {
                if (coins[i] == null) continue;
                Vector3 p = coins[i].transform.position;
                if (Mathf.Abs(p.y - targetCoinY) > 0.05f)
                {
                    p.y = Mathf.MoveTowards(p.y, targetCoinY, 14f * Time.deltaTime);
                    coins[i].transform.position = p;
                }
            }

            Gem[] gems = road.GetComponentsInChildren<Gem>();
            for (int i = 0; i < gems.Length; i++)
            {
                if (gems[i] == null) continue;
                Vector3 p = gems[i].transform.position;
                if (Mathf.Abs(p.y - targetGemY) > 0.05f)
                {
                    p.y = Mathf.MoveTowards(p.y, targetGemY, 14f * Time.deltaTime);
                    gems[i].transform.position = p;
                }
            }
        }
    }

    void SpawnPickupsOnRoad(GameObject road, float roadStartZ)
    {
        if (coinLanes == null || coinLanes.Length == 0) return;

        bool isGliding = SimplePlayerController.Instance != null && SimplePlayerController.Instance.IsGliding;
        float currentGemY = isGliding ? skyPickupAltitude : gemYOffset;

        float startZ = roadStartZ + coinZMargin + 2f;
        float endZ = roadStartZ + tileLength - coinZMargin - 2f;

        // 1. Gem spawn
        if (gemPrefab != null && Random.value < gemSpawnChance)
        {
            int lane = Random.Range(0, coinLanes.Length);
            float laneX = coinLanes[lane];
            float z = Random.Range(startZ, endZ);
            Vector3 pos = new Vector3(laneX, currentGemY, z);
            SpawnPickup(gemPrefab, pos, road.transform);
        }

        // 2. Skateboard pickup spawn
        if (skateboardPrefab != null && Random.value < skateboardSpawnChance)
        {
            int lane = Random.Range(0, coinLanes.Length);
            float laneX = coinLanes[lane];
            float z = Random.Range(startZ, endZ);
            Vector3 pos = new Vector3(laneX, skateboardYOffset, z);
            SpawnPickup(skateboardPrefab, pos, road.transform);
        }

        // 3. Glider pickup spawn
        if (gliderPrefab != null && Random.value < gliderSpawnChance)
        {
            int lane = Random.Range(0, coinLanes.Length);
            float laneX = coinLanes[lane];
            float z = Random.Range(startZ, endZ);
            Vector3 pos = new Vector3(laneX, gliderYOffset, z);
            SpawnPickup(gliderPrefab, pos, road.transform);
        }

        // 4. Magnet pickup spawn
        if (magnetPrefab != null && Random.value < magnetSpawnChance)
        {
            int lane = Random.Range(0, coinLanes.Length);
            float laneX = coinLanes[lane];
            float z = Random.Range(startZ, endZ);
            Vector3 pos = new Vector3(laneX, magnetYOffset, z);
            SpawnPickup(magnetPrefab, pos, road.transform);
        }
    }

    void SpawnPickup(GameObject prefab, Vector3 position, Transform parent)
    {
        if (prefab == null) return;
        GameObject item = Instantiate(prefab, position, Quaternion.identity, parent);

        // Ensure triggers on all colliders so runner passes through & triggers pickup
        Collider[] cols = item.GetComponentsInChildren<Collider>();
        if (cols.Length == 0)
        {
            SphereCollider sc = item.AddComponent<SphereCollider>();
            sc.radius = 0.8f;
            sc.isTrigger = true;
        }
        else
        {
            foreach (Collider c in cols)
            {
                if (c != null) c.isTrigger = true;
            }
        }
    }
}

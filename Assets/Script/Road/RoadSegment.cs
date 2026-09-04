using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class RoadSegment : MonoBehaviour
{
    [Header("Obstacles & Coins")]
    public GameObject[] obstaclePrefabs;
    public GameObject coinPrefab;
    public GameObject letterPrefab;

    [Header("Pre-Placed Obstacle Hiding")]
    [Tooltip("Minimum number of pre-placed obstacles to hide on this road segment.")]
    public int minHiddenPrePlacedObstacles = 1;
    [Tooltip("Maximum number of pre-placed obstacles to hide on this road segment.")]
    public int maxHiddenPrePlacedObstacles = 3;
    
    [Header("Spawn Settings")]
    public int minObstacles = 4;
    public int maxObstacles = 8;
    public int minCoins = 1;
    public int maxCoins = 2;
    public int minLetters = 1;
    public int maxLetters = 3;
    public float spawnZMin = 10f;
    public float spawnZMax = 65f;
    
    [Header("Lane Settings")]
    public float laneDistance = 2.5f; // lane spacing used by runner
    public float obstacleYOffset = 0f;
    public float coinYOffset = 1f;
    public float letterYOffset = 1.5f;
    
    [Header("Obstacle Spacing")]
    public float minObstacleSpacing = 5f; // Minimum distance between obstacles
    public bool ensureOneLaneClear = true; // Always keep at least one lane clear
    
    [Header("Coin Spacing")]
    public float minCoinSpacing = 4f; // Minimum distance between coins
    public bool ensureCoinSpacing = true; // Enable coin spacing
    
    [Header("Letter Spacing")]
    public float minLetterSpacing = 6f; // Minimum distance between letters
    public bool ensureLetterSpacing = true; // Enable letter spacing

    [Header("Powerups")]
    public GameObject magnetPrefab;
    [Range(0f, 1f)]
    public float magnetSpawnChance = 0.15f; // 15% chance per road segment
    public float magnetYOffset = 1.2f;

    public GameObject jetpackPrefab;
    [Range(0f, 1f)]
    public float jetpackSpawnChance = 0.10f; // 10% chance per road segment
    public float jetpackYOffset = 1.2f;

    [Header("Air Coins")]
    [Range(0f, 1f)]
    public float airCoinsSpawnChance = 0.35f; // 35% chance per road segment
    public float airCoinsYOffset = 7.5f; // height used by jetpack/flying state
    public int minAirCoins = 4;
    public int maxAirCoins = 10;

    [Header("Roadside Characters (Optional)")]
    [Tooltip("If left empty, characters will be automatically loaded from Resources/RoadsideCharacters/Pedestrians")]
    public GameObject[] customPedestrianPrefabs;
    [Tooltip("If left empty, characters will be automatically loaded from Resources/RoadsideCharacters/Hawkers")]
    public GameObject[] customHawkerPrefabs;
    [Range(0f, 1f)]
    public float roadsideCharacterVisibilityChance = 0.5f;

    // Track all spawned item positions for spacing
    private List<Vector3> spawnedPositions = new List<Vector3>();
    
    private float difficultyMultiplier = 1.0f;
    private int currentLevel = 1;
    
    // How many Instantiate calls per frame before yielding
    private const int SPAWNS_PER_FRAME = 6;

    // Mobile-reduced spawn counts
    private static bool isMobile =
#if UNITY_ANDROID || UNITY_IOS
        true;
#else
        false;
#endif

    private static GameObject[] cachedPedestrianPrefabs = null;
    private static GameObject[] cachedHawkerPrefabs = null;

    private GameObject[] pedestrianPrefabsToUse;
    private GameObject[] hawkerPrefabsToUse;

    private static void LoadCharacterPrefabs()
    {
        if (cachedPedestrianPrefabs == null)
        {
            cachedPedestrianPrefabs = Resources.LoadAll<GameObject>("RoadsideCharacters/Pedestrians");
        }
        if (cachedHawkerPrefabs == null)
        {
            cachedHawkerPrefabs = Resources.LoadAll<GameObject>("RoadsideCharacters/Hawkers");
        }
    }
    
    void Start()
    {
        // Resolve which character prefabs to use
        if (!isMobile)
        {
            if (customPedestrianPrefabs != null && customPedestrianPrefabs.Length > 0)
            {
                pedestrianPrefabsToUse = customPedestrianPrefabs;
            }
            else
            {
                LoadCharacterPrefabs();
                pedestrianPrefabsToUse = cachedPedestrianPrefabs;
            }

            if (customHawkerPrefabs != null && customHawkerPrefabs.Length > 0)
            {
                hawkerPrefabsToUse = customHawkerPrefabs;
            }
            else
            {
                LoadCharacterPrefabs();
                hawkerPrefabsToUse = cachedHawkerPrefabs;
            }
        }

        // Register pools if they don't exist
        if (ObjectPoolManager.Instance != null)
        {
            if (coinPrefab != null) 
                ObjectPoolManager.Instance.AddPool(coinPrefab.name, coinPrefab, isMobile ? 15 : 30);
                
            if (magnetPrefab != null)
                ObjectPoolManager.Instance.AddPool(magnetPrefab.name, magnetPrefab, isMobile ? 2 : 4);
            
            if (obstaclePrefabs != null)
            {
                foreach (var prefab in obstaclePrefabs)
                {
                    if (prefab != null)
                    {
                        ObjectPoolManager.Instance.AddPool(prefab.name, prefab, isMobile ? 5 : 10);
                    }
                }
            }

            if (pedestrianPrefabsToUse != null)
            {
                foreach (var prefab in pedestrianPrefabsToUse)
                {
                    if (prefab != null)
                    {
                        ObjectPoolManager.Instance.AddPool(prefab.name, prefab, isMobile ? 3 : 6);
                    }
                }
            }

            if (hawkerPrefabsToUse != null)
            {
                foreach (var prefab in hawkerPrefabsToUse)
                {
                    if (prefab != null)
                    {
                        ObjectPoolManager.Instance.AddPool(prefab.name, prefab, isMobile ? 2 : 4);
                    }
                }
            }
        }
    }
    
    public void SetDifficulty(float obstacleRate)
    {
        // Cache the original inspector values once so repeated calls (on recycled
        // tiles) scale from the base instead of compounding each time.
        if (baseMinObstacles < 0)
        {
            baseMinObstacles = minObstacles;
            baseMaxObstacles = maxObstacles;
        }

        difficultyMultiplier = obstacleRate;
        minObstacles = Mathf.CeilToInt(baseMinObstacles * difficultyMultiplier);
        maxObstacles = Mathf.CeilToInt(baseMaxObstacles * difficultyMultiplier);
    }
    
    public void SetCurrentLevel(int level)
    {
        currentLevel = level;
    }

    /// <summary>
    /// Returns the Z range where dynamic items (coins, magnets) should spawn.
    /// Uses SpawnPoint and SpawnEndPoint transforms when available, otherwise
    /// falls back to the legacy spawnZMin/spawnZMax offsets.
    /// </summary>
    private void GetSpawnZRange(out float startZ, out float endZ)
    {
        Transform spawnPoint = transform.Find("SpawnPoint");
        Transform spawnEndPoint = transform.Find("SpawnEndPoint");

        if (spawnPoint != null && spawnEndPoint != null)
        {
            startZ = spawnPoint.position.z;
            endZ = spawnEndPoint.position.z;
        }
        else if (spawnPoint != null)
        {
            // Old style prefab: SpawnPoint marks the end of the road
            startZ = transform.position.z;
            endZ = spawnPoint.position.z;
        }
        else
        {
            // Fallback to inspector offsets
            startZ = transform.position.z + spawnZMin;
            endZ = transform.position.z + spawnZMax;
        }

        // Safety: keep start before end
        if (endZ < startZ)
        {
            float temp = startZ;
            startZ = endZ;
            endZ = temp;
        }
    }

    /// <summary>
    /// Process pre-placed obstacles (vehicles, barriers, fences, etc.) baked
    /// into the road prefab. Randomly hides a small number of them and ensures
    /// the rest are shown with solid colliders.
    /// </summary>
    private void RemovePrePlacedObstacles()
    {
        // Only process once per tile instance; recycled tiles keep their kept obstacles.
        if (prePlacedProcessed) return;
        prePlacedProcessed = true;

        // Collect distinct obstacle root objects
        List<GameObject> prePlaced = new List<GameObject>();
        CollectObstacleRoots(transform, prePlaced);

        if (prePlaced.Count == 0) return;

        int hiddenCount = Mathf.Clamp(
            Random.Range(minHiddenPrePlacedObstacles, maxHiddenPrePlacedObstacles + 1),
            0, prePlaced.Count);

        HashSet<int> hiddenIndices = new HashSet<int>();
        while (hiddenIndices.Count < hiddenCount)
        {
            hiddenIndices.Add(Random.Range(0, prePlaced.Count));
        }

        // Set active state and ensure solid colliders only on active obstacles
        for (int i = 0; i < prePlaced.Count; i++)
        {
            GameObject obs = prePlaced[i];
            if (obs == null) continue;

            bool shouldHide = hiddenIndices.Contains(i);
            if (shouldHide)
            {
                obs.SetActive(false);
            }
            else
            {
                obs.SetActive(true);
                EnsureSolidCollider(obs);
            }
        }

#if UNITY_EDITOR
        Debug.Log($"[RoadSegment] Kept {prePlaced.Count - hiddenCount} of {prePlaced.Count} pre-placed obstacles active with solid colliders.");
#endif
    }

    private void EnsureSolidCollider(GameObject obs)
    {
        Collider[] colliders = obs.GetComponentsInChildren<Collider>(true);
        if (colliders == null || colliders.Length == 0)
        {
            BoxCollider box = obs.AddComponent<BoxCollider>();
            Vector3 scale = obs.transform.lossyScale;
            float sizeX = 1.5f / (scale.x > 0.001f ? scale.x : 1f);
            float sizeY = 2.0f / (scale.y > 0.001f ? scale.y : 1f);
            float sizeZ = 1.5f / (scale.z > 0.001f ? scale.z : 1f);
            box.size = new Vector3(sizeX, sizeY, sizeZ);
            box.isTrigger = false;
        }
        else
        {
            foreach (var col in colliders)
            {
                if (col != null && col.isTrigger)
                    col.isTrigger = false;
            }
        }
    }

    public void SetPrePlacedObstaclesActive(bool active)
    {
        List<GameObject> prePlaced = new List<GameObject>();
        CollectObstacleRoots(transform, prePlaced);
        foreach (GameObject obstacle in prePlaced)
        {
            if (obstacle != null)
                obstacle.SetActive(active);
        }

        if (!active)
            prePlacedProcessed = false;
    }

    private void CollectObstacleRoots(Transform current, List<GameObject> results)
    {
        for (int i = 0; i < current.childCount; i++)
        {
            Transform child = current.GetChild(i);
            if (child.CompareTag("Obstacle"))
            {
                results.Add(child.gameObject);
            }
            else
            {
                if (child.childCount > 0)
                {
                    CollectObstacleRoots(child, results);
                }
            }
        }
    }

    public bool DynamicSpawnsRequested { get; private set; }

    public void SpawnItems()
    {
        if (DynamicSpawnsRequested) return;

        DynamicSpawnsRequested = true;
        StartCoroutine(SpawnItemsOverFrames());
    }
    
    private IEnumerator SpawnItemsOverFrames()
    {
        // Clean up any previously spawned objects (important when road is reused from pool)
        ClearDynamicSpawns(false, false);
        
        // Remove pre-placed obstacles baked into the road prefab
        RemovePrePlacedObstacles();
        
        int spawnCount = 0;
        
        // Spawn obstacles with proper spacing
        if (obstaclePrefabs != null && obstaclePrefabs.Length > 0)
        {
            int obstacleCount = Random.Range(minObstacles, maxObstacles + 1);
            if (isMobile) obstacleCount = Mathf.Min(obstacleCount, 7);
            
            for (int i = 0; i < obstacleCount; i++)
            {
                int attempts = 0;
                bool validPosition = false;
                Vector3 spawnPos = Vector3.zero;
                
                // Try to find a valid position with proper spacing
                while (!validPosition && attempts < 20)
                {
                    int lane = Random.Range(0, 3);
                    float laneX = (lane - 1) * laneDistance;
                    float zPos = transform.position.z + Random.Range(spawnZMin, spawnZMax);
                    spawnPos = new Vector3(laneX, obstacleYOffset, zPos);
                    
                    validPosition = true;
                    foreach (Vector3 existingPos in spawnedPositions)
                    {
                        float xDiff = Mathf.Abs(spawnPos.x - existingPos.x);
                        float zDiff = Mathf.Abs(spawnPos.z - existingPos.z);

                        if (xDiff < 0.5f) // Same lane
                        {
                            if (zDiff < 16f) // Proportional spacing for long buses/cars
                            {
                                validPosition = false;
                                break;
                            }
                        }
                        else // Different lanes
                        {
                            if (zDiff < 6f) // Keep lanes clean and readable
                            {
                                validPosition = false;
                                break;
                            }
                        }
                    }
                    
                    if (validPosition && ensureOneLaneClear)
                    {
                        validPosition = !AreAllLanesBlockedAtZ(spawnedPositions, spawnPos.z, lane);
                    }
                    
                    attempts++;
                }
                
                if (validPosition)
                {
                    GameObject obstacle = null;
                    GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
                    
                    if (ObjectPoolManager.Instance != null && prefab != null)
                    {
                        // Dynamic safety check: make sure pool is added
                        ObjectPoolManager.Instance.AddPool(prefab.name, prefab, isMobile ? 5 : 10);
                        obstacle = ObjectPoolManager.Instance.SpawnFromPool(prefab.name, spawnPos, Quaternion.identity);
                        if (obstacle != null)
                            obstacle.transform.SetParent(transform);
                    }
                    else if (prefab != null)
                    {
                        obstacle = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
                    }

                    if (obstacle != null)
                    {
                        obstacle.tag = "Obstacle";

                        // Ensure obstacle has a non-trigger collider so the runner can hit it
                        Collider col = obstacle.GetComponent<Collider>();
                        if (col == null)
                        {
                            BoxCollider box = obstacle.AddComponent<BoxCollider>();
                            box.size = new Vector3(1.5f, 2f, 1.5f);
                            col = box;
                        }
                        if (col.isTrigger)
                            col.isTrigger = false;

                        spawnedObjects.Add(obstacle);
                    }
                    
                    spawnedPositions.Add(spawnPos);
                    
                    spawnCount++;
                    if (spawnCount >= SPAWNS_PER_FRAME)
                    {
                        spawnCount = 0;
                        yield return null;
                    }
                }
            }
        }

        // Spawn coins
        if (coinPrefab != null)
        {
            float coinStartZ, coinEndZ;
            GetSpawnZRange(out coinStartZ, out coinEndZ);

            int coinCount = Random.Range(minCoins, maxCoins + 1);
            coinCount = Mathf.Clamp(coinCount, 0, 2); // At most 2 coins per road prefab
            for (int i = 0; i < coinCount; i++)
            {
                int attempts = 0;
                bool validPosition = false;
                Vector3 spawnPos = Vector3.zero;
                
                while (!validPosition && attempts < 15)
                {
                    int lane = Random.Range(0, 3);
                    float laneX = (lane - 1) * laneDistance;
                    float zPos = Random.Range(coinStartZ, coinEndZ);
                    spawnPos = new Vector3(laneX, coinYOffset, zPos);
                    
                    validPosition = true;
                    foreach (Vector3 existingPos in spawnedPositions)
                    {
                        if (Vector3.Distance(spawnPos, existingPos) < minObstacleSpacing * 0.5f)
                        {
                            validPosition = false;
                            break;
                        }
                    }

                    if (validPosition && ensureCoinSpacing)
                    {
                        foreach (GameObject existingCoin in spawnedObjects)
                        {
                            if (existingCoin != null && existingCoin.CompareTag("Coin") &&
                                Vector3.Distance(spawnPos, existingCoin.transform.position) < minCoinSpacing)
                            {
                                validPosition = false;
                                break;
                            }
                        }
                    }
                    attempts++;
                }
                
                if (validPosition)
                {
                    GameObject coin = null;
                    if (ObjectPoolManager.Instance != null && coinPrefab != null)
                    {
                        ObjectPoolManager.Instance.AddPool(coinPrefab.name, coinPrefab, isMobile ? 15 : 30);
                        coin = ObjectPoolManager.Instance.SpawnFromPool(coinPrefab.name, spawnPos, Quaternion.identity);
                        if (coin != null)
                            coin.transform.SetParent(transform);
                    }
                    else if (coinPrefab != null)
                    {
                        coin = Instantiate(coinPrefab, spawnPos, Quaternion.identity, transform);
                    }

                    if (coin != null)
                    {
                        coin.tag = "Coin";
                        // Coins must be triggers so OnTriggerEnter fires on the player
                        Collider coinCol = coin.GetComponent<Collider>();
                        if (coinCol == null) coinCol = coin.AddComponent<SphereCollider>();
                        coinCol.isTrigger = true;
                        spawnedObjects.Add(coin);
                    }
                    
                    spawnCount++;
                    if (spawnCount >= SPAWNS_PER_FRAME)
                    {
                        spawnCount = 0;
                        yield return null;
                    }
                }
            }
        }

        // Spawn magnet powerup
        if (magnetPrefab != null && Random.value < magnetSpawnChance)
        {
            float magnetStartZ, magnetEndZ;
            GetSpawnZRange(out magnetStartZ, out magnetEndZ);

            int attempts = 0;
            bool validPosition = false;
            Vector3 spawnPos = Vector3.zero;
            
            while (!validPosition && attempts < 15)
            {
                int lane = Random.Range(0, 3);
                float laneX = (lane - 1) * laneDistance;
                float zPos = Random.Range(magnetStartZ, magnetEndZ);
                spawnPos = new Vector3(laneX, magnetYOffset, zPos);
                
                validPosition = true;
                foreach (Vector3 obstaclePos in spawnedPositions)
                {
                    if (Vector3.Distance(spawnPos, obstaclePos) < minObstacleSpacing * 0.5f)
                    {
                        validPosition = false;
                        break;
                    }
                }
                attempts++;
            }
            
            if (validPosition)
            {
                GameObject magnet = null;
                if (ObjectPoolManager.Instance != null && magnetPrefab != null)
                {
                    ObjectPoolManager.Instance.AddPool(magnetPrefab.name, magnetPrefab, isMobile ? 2 : 4);
                    magnet = ObjectPoolManager.Instance.SpawnFromPool(magnetPrefab.name, spawnPos, Quaternion.identity);
                    if (magnet != null)
                        magnet.transform.SetParent(transform);
                }
                else if (magnetPrefab != null)
                {
                    magnet = Instantiate(magnetPrefab, spawnPos, Quaternion.identity, transform);
                }

                if (magnet != null)
                {
                    // Ensure there is a trigger collider on it so the player collects it on collision
                    Collider magnetCol = magnet.GetComponent<Collider>();
                    if (magnetCol == null) magnetCol = magnet.AddComponent<SphereCollider>();
                    magnetCol.isTrigger = true;
                    
                    // Ensure MagnetPowerup script is attached to it
                    if (magnet.GetComponent<MagnetPowerup>() == null)
                    {
                        magnet.AddComponent<MagnetPowerup>();
                    }

                    spawnedObjects.Add(magnet);
                }
            }
        }

        // Spawn jetpack powerup
        if (jetpackPrefab != null && Random.value < jetpackSpawnChance)
        {
            float jetpackStartZ, jetpackEndZ;
            GetSpawnZRange(out jetpackStartZ, out jetpackEndZ);

            int attempts = 0;
            bool validPosition = false;
            Vector3 spawnPos = Vector3.zero;
            
            while (!validPosition && attempts < 15)
            {
                int lane = Random.Range(0, 3);
                float laneX = (lane - 1) * laneDistance;
                float zPos = Random.Range(jetpackStartZ, jetpackEndZ);
                spawnPos = new Vector3(laneX, jetpackYOffset, zPos);
                
                validPosition = true;
                foreach (Vector3 obstaclePos in spawnedPositions)
                {
                    if (Vector3.Distance(spawnPos, obstaclePos) < minObstacleSpacing * 0.5f)
                    {
                        validPosition = false;
                        break;
                    }
                }
                attempts++;
            }
            
            if (validPosition)
            {
                GameObject jetpack = null;
                if (ObjectPoolManager.Instance != null)
                {
                    ObjectPoolManager.Instance.AddPool(jetpackPrefab.name, jetpackPrefab, isMobile ? 2 : 4);
                    jetpack = ObjectPoolManager.Instance.SpawnFromPool(jetpackPrefab.name, spawnPos, Quaternion.identity);
                    if (jetpack != null)
                        jetpack.transform.SetParent(transform);
                }
                else
                {
                    jetpack = Instantiate(jetpackPrefab, spawnPos, Quaternion.identity, transform);
                }

                if (jetpack != null)
                {
                    // Ensure there is a trigger collider on it so the player collects it on collision
                    Collider jetpackCol = jetpack.GetComponent<Collider>();
                    if (jetpackCol == null) jetpackCol = jetpack.AddComponent<SphereCollider>();
                    jetpackCol.isTrigger = true;
                    
                    // Ensure JetpackPowerup script is attached to it
                    if (jetpack.GetComponent<JetpackPowerup>() == null)
                    {
                        jetpack.AddComponent<JetpackPowerup>();
                    }

                    spawnedObjects.Add(jetpack);
                }
            }
        }

        // Spawn Air Coins
        if (coinPrefab != null && (Random.value < airCoinsSpawnChance || (jetpackPrefab != null && Random.value < jetpackSpawnChance)))
        {
            float coinStartZ, coinEndZ;
            GetSpawnZRange(out coinStartZ, out coinEndZ);

            int airCoinCount = Random.Range(minAirCoins, maxAirCoins + 1);
            int airLane = Random.Range(0, 3);
            float laneX = (airLane - 1) * laneDistance;

            float spacing = (coinEndZ - coinStartZ) / (airCoinCount + 1);

            for (int i = 0; i < airCoinCount; i++)
            {
                float zPos = coinStartZ + spacing * (i + 1);
                Vector3 spawnPos = new Vector3(laneX, airCoinsYOffset, zPos);

                GameObject coin = null;
                if (ObjectPoolManager.Instance != null)
                {
                    coin = ObjectPoolManager.Instance.SpawnFromPool(coinPrefab.name, spawnPos, Quaternion.identity);
                    if (coin != null)
                        coin.transform.SetParent(transform);
                }
                else
                {
                    coin = Instantiate(coinPrefab, spawnPos, Quaternion.identity, transform);
                }

                if (coin != null)
                {
                    coin.tag = "Coin";
                    Collider coinCol = coin.GetComponent<Collider>();
                    if (coinCol == null) coinCol = coin.AddComponent<SphereCollider>();
                    coinCol.isTrigger = true;
                    spawnedObjects.Add(coin);
                }
            }
        }

        // Spawn Roadside Characters (Pedestrians & Hawkers)
        if (!isMobile && pedestrianPrefabsToUse != null && pedestrianPrefabsToUse.Length > 0 && Random.value < 0.7f && Random.value < roadsideCharacterVisibilityChance) // 70% chance to spawn some pedestrians per segment
        {
            int pCount = Random.Range(1, isMobile ? 3 : 5);
            for (int i = 0; i < pCount; i++)
            {
                float sideX = Random.value < 0.5f ? -9.5f : 9.5f; // Left or Right Sidewalk
                sideX += Random.Range(-1.0f, 1.0f); // Jitter slightly on the pavement width

                float zPos = transform.position.z + Random.Range(spawnZMin, spawnZMax);
                Vector3 spawnPos = new Vector3(sideX, 0f, zPos);

                GameObject pedPrefab = pedestrianPrefabsToUse[Random.Range(0, pedestrianPrefabsToUse.Length)];
                if (pedPrefab != null)
                {
                    GameObject pedestrian = null;
                    if (ObjectPoolManager.Instance != null)
                    {
                        pedestrian = ObjectPoolManager.Instance.SpawnFromPool(pedPrefab.name, spawnPos, Quaternion.identity);
                        if (pedestrian != null)
                            pedestrian.transform.SetParent(transform);
                    }
                    else
                    {
                        pedestrian = Instantiate(pedPrefab, spawnPos, Quaternion.identity, transform);
                    }

                    if (pedestrian != null)
                    {
                        var rsc = pedestrian.GetComponent<RoadsideCharacter>();
                        if (rsc == null) rsc = pedestrian.AddComponent<RoadsideCharacter>();

                        if (pedestrian.GetComponent<GroundSnap>() == null)
                            pedestrian.AddComponent<GroundSnap>();

                        spawnedObjects.Add(pedestrian);
                    }

                    spawnCount++;
                    if (spawnCount >= SPAWNS_PER_FRAME)
                    {
                        spawnCount = 0;
                        yield return null;
                    }
                }
            }
        }

        if (!isMobile && hawkerPrefabsToUse != null && hawkerPrefabsToUse.Length > 0 && Random.value < 0.5f && Random.value < roadsideCharacterVisibilityChance) // 50% chance to spawn a hawker per segment
        {
            int hCount = Random.Range(1, isMobile ? 2 : 3);
            for (int i = 0; i < hCount; i++)
            {
                float sideX = Random.value < 0.5f ? -9.5f : 9.5f; // Left or Right Sidewalk
                sideX += Random.Range(-0.5f, 0.5f); // Jitter slightly

                float zPos = transform.position.z + Random.Range(spawnZMin, spawnZMax);
                Vector3 spawnPos = new Vector3(sideX, 0f, zPos);

                GameObject hawkPrefab = hawkerPrefabsToUse[Random.Range(0, hawkerPrefabsToUse.Length)];
                if (hawkPrefab != null)
                {
                    GameObject hawker = null;
                    if (ObjectPoolManager.Instance != null)
                    {
                        hawker = ObjectPoolManager.Instance.SpawnFromPool(hawkPrefab.name, spawnPos, Quaternion.identity);
                        if (hawker != null)
                            hawker.transform.SetParent(transform);
                    }
                    else
                    {
                        hawker = Instantiate(hawkPrefab, spawnPos, Quaternion.identity, transform);
                    }

                    if (hawker != null)
                    {
                        var rsc = hawker.GetComponent<RoadsideCharacter>();
                        if (rsc == null) rsc = hawker.AddComponent<RoadsideCharacter>();
                        rsc.movementType = RoadsideCharacter.MovementType.Stationary;

                        if (hawker.GetComponent<GroundSnap>() == null)
                            hawker.AddComponent<GroundSnap>();

                        spawnedObjects.Add(hawker);
                    }

                    spawnCount++;
                    if (spawnCount >= SPAWNS_PER_FRAME)
                    {
                        spawnCount = 0;
                        yield return null;
                    }
                }
            }
        }

        ApplyCurvatureToSpawnedObjects();
    }

    /// <summary>
    /// Check if adding an obstacle at this lane/Z would block all 3 lanes
    /// </summary>
    private bool AreAllLanesBlockedAtZ(List<Vector3> existingObstacles, float zPos, int proposedLane)
    {
        // Track which lanes are occupied near this Z position
        bool[] lanesOccupied = new bool[3]; // 0=left, 1=center, 2=right
        lanesOccupied[proposedLane] = true; // This lane would be occupied
        
        // Check existing obstacles
        foreach (Vector3 obstaclePos in existingObstacles)
        {
            // If obstacle is close to this Z position (within spacing distance)
            if (Mathf.Abs(obstaclePos.z - zPos) < 10f)
            {
                // Determine which lane this obstacle is in
                int obstacleLane = Mathf.RoundToInt((obstaclePos.x / laneDistance) + 1);
                if (obstacleLane >= 0 && obstacleLane < 3)
                {
                    lanesOccupied[obstacleLane] = true;
                }
            }
        }
        
        // Check if all 3 lanes are occupied
        return lanesOccupied[0] && lanesOccupied[1] && lanesOccupied[2];
    }

    // Track letter positions for spacing
    private List<Vector3> spawnedLetterPositions = new List<Vector3>();
    
    // Track spawned objects for cleanup when road is pooled/reused
    private List<GameObject> spawnedObjects = new List<GameObject>();
    
    // Pre-placed obstacles are mutated only once; guarded so recycled tiles stay consistent.
    private bool prePlacedProcessed = false;
    
    // Cache base obstacle counts so difficulty doesn't compound on recycled tiles
    private int baseMinObstacles = -1;
    private int baseMaxObstacles = -1;

    [HideInInspector]
    public bool curvatureApplied = false;

    /// <summary>
    /// Apply curvature to only the dynamically-spawned items (obstacles, coins, powerups)
    /// instead of the whole heavy road prefab, saving substantial CPU and GC alloc on mobile.
    /// </summary>
    public void ApplyCurvatureToSpawnedObjects()
    {
        if (CurvedWorldManager.Instance == null) return;
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                CurvedWorldManager.Instance.ApplyCurvatureToGameObject(obj);
            }
        }
    }

    /// <summary>
    /// Spawn a specific letter on this road segment (called by WordManager)
    /// </summary>
    public void SpawnLetter(string letter, bool isCorrect, int lane, float zOffset)
    {
        // Letter spawning is disabled
    }
    
    /// <summary>
    /// Clear letter positions when road segment is disabled or destroyed
    /// </summary>
    private void OnDisable()
    {
        CleanupSpawnedItems();
    }

    private void OnDestroy()
    {
        CleanupSpawnedItems();
    }

    /// <summary>
    /// Destroy only the dynamically-spawned items (obstacles/coins/letters) and reset
    /// spacing trackers, so this road tile can be repositioned and reused by the pool.
    /// Road geometry and kept pre-placed obstacles are left intact.
    /// Called by RoadSpawner when recycling a road tile.
    /// </summary>
    public void ClearDynamicSpawns()
    {
        ClearDynamicSpawns(true, true);
    }

    private void ClearDynamicSpawns(bool resetSpawnRequest, bool stopCoroutines)
    {
        if (stopCoroutines)
        {
            StopAllCoroutines();
        }
        if (resetSpawnRequest)
        {
            DynamicSpawnsRequested = false;
        }
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                if (ObjectPoolManager.Instance != null)
                {
                    ObjectPoolManager.Instance.ReturnToPool(obj);
                }
                else
                {
                    Destroy(obj);
                }
            }
        }
        spawnedObjects.Clear();
        spawnedLetterPositions.Clear();
        spawnedPositions.Clear();
    }

    private void CleanupSpawnedItems()
    {
        ClearDynamicSpawns();
    }
}

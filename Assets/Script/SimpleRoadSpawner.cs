using UnityEngine;
using System.Collections.Generic;

public class SimpleRoadSpawner : MonoBehaviour
{
    private struct SpawnedSegment
    {
        public GameObject gameObject;
        public Transform transform;
        public Transform spawnPoint;
        public Transform spawnEndPoint;
        public float length;
        public bool isStarter;
        public List<GameObject> spawnedItems;
        public GameObject prefab;
    }

    private static bool isMobile =
#if UNITY_ANDROID || UNITY_IOS
        true;
#else
        false;
#endif

    [Header("Spawner Settings")]
    public Transform playerTransform;
    public Transform starterRoad; // Pre-placed starter road in the scene
    public GameObject[] roadPrefabs;
    public int numberOfSegments = 10;
    public float segmentLength = 20f;
    [Tooltip("Additional safety distance behind the player before a segment is recycled to prevent players/cameras from seeing it disappear.")]
    public float recycleBuffer = 30f;

    [Header("Obstacles & Coins")]
    [Tooltip("Prefabs for vehicles, barriers, potholes, etc.")]
    public GameObject[] obstaclePrefabs;
    public GameObject coinPrefab;
    [Range(0f, 1f)]
    public float obstacleSpawnChance = 0.9f;
    [Tooltip("How many extra times the crate barrier is added to the spawn pool so barriers appear more often.")]
    public int barrierSpawnWeight = 3;
    [Range(0f, 1f)]
    public float movingVehicleSpawnChance = 0.7f;
    public int maxObstaclesPerLaneInView = 2;
    public float laneOccupancyRange = 45f;

    [Header("Regular Traffic")]
    [Tooltip("A moving vehicle is spawned ahead of the player at a random interval within this range (seconds), keeping traffic steady instead of bursty.")]
    public float trafficIntervalMin = 5f;
    public float trafficIntervalMax = 9f;
    [Tooltip("How far ahead of the player traffic vehicles are spawned.")]
    public float trafficSpawnAhead = 120f;
    [Range(0f, 1f)]
    public float coinSpawnChance = 0.6f;
    public int minObstaclesPerSegment = 1;
    public int maxObstaclesPerSegment = 3;
    public int minCoinsPerSegment = 3;
    public int maxCoinsPerSegment = 8;
    public float laneDistance = 2.5f;
    public float obstacleYOffset = 0f;
    public float coinYOffset = 1f;
    public float spawnZMargin = 4f;
    public float minObstacleSpacing = 14f;
    public float minCoinSpacing = 2.5f;

    [Header("Hang Glider Powerup")]
    public GameObject gliderPrefab;
    [Range(0f, 1f)]
    public float gliderSpawnChance = 1f;
    public float gliderYOffset = 1.2f;
    [Tooltip("Never block all three lanes at the same Z so the player can always pass.")]
    public bool ensureOneLaneClear = true;
    [Tooltip("Z distance window used when checking if lanes are blocked side by side. Obstacles within this range count as being 'at the same time'.")]
    public float sameZWindow = 10f;

    [Header("Crate Barrier")]
    [Tooltip("Fixed crate barrier prefab. If left empty, loads from Resources/Prefabs/Obstacles/CrateBarrier.")]
    public GameObject crateBarrierPrefab;

    [Header("Moving Vehicle Safety")]
    [Tooltip("Extra spacing used when at least one of the two obstacles is a moving vehicle, so they don't overlap at different speeds.")]
    public float movingVehicleSpacing = 20f;

    [Header("Road Prefab Cleanup")]
    [Tooltip("If enabled, baked obstacles/props (car, keke, porthole, shop, etc.) inside road prefabs will be disabled. Keep this OFF to show the existing train/car/pothole models.")]
    public bool clearBakedObstacles = false;

    [Header("Roadside Characters")]
    [Tooltip("Prefabs with walking animations that will appear on the left/right roadside.")]
    public GameObject[] roadsideCharacterPrefabs;
    [Range(0f, 1f)]
    public float roadsideCharacterSpawnChance = 0.15f;
    public int minRoadsideCharactersPerSegment = 0;
    public int maxRoadsideCharactersPerSegment = 1;
    public float roadsideXOffset = 4f;
    public float minRoadsideCharacterSpacing = 6f;

    private List<SpawnedSegment> activeSegments = new List<SpawnedSegment>();
    private float spawnZ = 0f;
    private List<GameObject> roadPrefabPool = new List<GameObject>();
    private GameObject lastRoadPrefab;
    private int lastFreeLane = -1;
    private float nextTrafficTime;
    private bool wasGliding = false;

    // Pool road segment GameObjects to avoid Instantiate/Destroy churn.
    private Dictionary<GameObject, Queue<GameObject>> roadSegmentPools = new Dictionary<GameObject, Queue<GameObject>>();
    private const int ROAD_POOL_SIZE_PER_PREFAB = 3;

    // Cache expensive "fix visibility" pass so it only runs once per pooled instance.
    private HashSet<int> visibilityFixedInstances = new HashSet<int>();

    // Reusable buffers to avoid per-spawn GC allocations.
    private List<GameObject> reusableObstacleCandidates = new List<GameObject>(32);
    private List<GameObject> reusableMovingVehicleList = new List<GameObject>(32);
    private int[] reusableLaneOrder = new int[3] { 0, 1, 2 };
    private bool[] reusableBlockedLanes = new bool[3];

    // Cache whether a prefab is a moving vehicle.
    private Dictionary<GameObject, VehicleMover> prefabVehicleMoverCache = new Dictionary<GameObject, VehicleMover>();

    private void Awake()
    {
        if (crateBarrierPrefab == null)
            crateBarrierPrefab = Resources.Load<GameObject>("Prefabs/Obstacles/CrateBarrier");

        // Enforce fair-play minimums regardless of Inspector overrides:
        // generous spacing so the cat can dodge, and one lane always free as an escape route.
        minObstacleSpacing = Mathf.Max(minObstacleSpacing, 14f);
        movingVehicleSpacing = Mathf.Max(movingVehicleSpacing, 20f);
        sameZWindow = Mathf.Max(sameZWindow, 10f);
        ensureOneLaneClear = true;

        InitializeRoadPrefabPool();
        InitializeRoadSegmentPools();
    }

    private void InitializeRoadSegmentPools()
    {
        if (roadPrefabs == null) return;

        for (int i = 0; i < roadPrefabs.Length; i++)
        {
            GameObject prefab = roadPrefabs[i];
            if (prefab == null) continue;

            Queue<GameObject> pool = new Queue<GameObject>(ROAD_POOL_SIZE_PER_PREFAB);
            for (int p = 0; p < ROAD_POOL_SIZE_PER_PREFAB; p++)
            {
                GameObject segmentObj = Instantiate(prefab, transform);
                segmentObj.SetActive(false);
                ConfigureSegmentInstance(segmentObj);
                pool.Enqueue(segmentObj);
            }
            roadSegmentPools[prefab] = pool;
        }
    }

    private void ConfigureSegmentInstance(GameObject segmentObj)
    {
        if (clearBakedObstacles)
            OptimizeAndClearObstacles(segmentObj);

        OptimizeTileForMobile(segmentObj);

        if (CurvedWorldManager.Instance != null)
            CurvedWorldManager.Instance.ApplyCurvatureToGameObject(segmentObj);

        if (!clearBakedObstacles)
            EnsureBakedObstaclesVisible(segmentObj);
    }

    private GameObject GetRoadSegmentFromPool(GameObject prefab)
    {
        Queue<GameObject> pool;
        if (roadSegmentPools.TryGetValue(prefab, out pool) && pool.Count > 0)
        {
            GameObject segment = pool.Dequeue();
            segment.SetActive(true);
            return segment;
        }

        // Pool empty or missing - instantiate and configure once.
        GameObject segmentObj = Instantiate(prefab, transform);
        ConfigureSegmentInstance(segmentObj);
        return segmentObj;
    }

    private void Start()
    {
        EnsureObjectPoolManager();
        PreWarmDynamicItemPools();

        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        // Use a pre-placed starter road in the scene if available
        if (starterRoad != null)
        {
            RegisterStarterRoad();
            for (int i = 0; i < numberOfSegments - 1; i++)
            {
                SpawnSegment(false);
            }
        }
        else
        {
            // Spawn initial road segments
            for (int i = 0; i < numberOfSegments; i++)
            {
                SpawnSegment(i == 0);
            }
        }
    }

    private void EnsureObjectPoolManager()
    {
        if (ObjectPoolManager.Instance != null) return;

        // ObjectPoolManager must exist for cheap spawning/recycling. Create it at runtime if the scene does not have one.
        GameObject poolManagerGO = new GameObject("ObjectPoolManager");
        poolManagerGO.AddComponent<ObjectPoolManager>();
    }

    private void PreWarmDynamicItemPools()
    {
        if (ObjectPoolManager.Instance == null) return;

        if (coinPrefab != null)
            ObjectPoolManager.Instance.AddPool(coinPrefab.name, coinPrefab, isMobile ? 10 : 20);

        if (gliderPrefab != null)
            ObjectPoolManager.Instance.AddPool(gliderPrefab.name, gliderPrefab, isMobile ? 2 : 4);

        if (crateBarrierPrefab != null)
            ObjectPoolManager.Instance.AddPool(crateBarrierPrefab.name, crateBarrierPrefab, isMobile ? 5 : 10);

        if (obstaclePrefabs != null)
        {
            for (int i = 0; i < obstaclePrefabs.Length; i++)
            {
                GameObject p = obstaclePrefabs[i];
                if (p == null) continue;
                int size = IsMovingVehiclePrefab(p) ? (isMobile ? 5 : 10) : (isMobile ? 5 : 10);
                ObjectPoolManager.Instance.AddPool(p.name, p, size);
            }
        }

        if (roadsideCharacterPrefabs != null)
        {
            for (int i = 0; i < roadsideCharacterPrefabs.Length; i++)
            {
                GameObject p = roadsideCharacterPrefabs[i];
                if (p != null)
                    ObjectPoolManager.Instance.AddPool(p.name, p, isMobile ? 3 : 5);
            }
        }
    }

    private bool IsMovingVehiclePrefab(GameObject prefab)
    {
        VehicleMover vm;
        if (prefabVehicleMoverCache.TryGetValue(prefab, out vm))
            return vm != null;

        vm = prefab.GetComponent<VehicleMover>();
        prefabVehicleMoverCache[prefab] = vm;
        return vm != null;
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // Recycle when the player has fully passed the oldest segment plus the safety buffer distance
        // Using a while loop with a safety cap to catch up if player jumps/teleports, avoiding any stalling
        int safetyCap = 0;
        while (activeSegments.Count > 0 && 
               activeSegments[0].gameObject != null &&
               playerTransform.position.z - (activeSegments[0].length + recycleBuffer) > activeSegments[0].transform.position.z && 
               safetyCap < 5)
        {
            RecycleSegment();
            safetyCap++;
        }

        // Steady stream of oncoming traffic at a regular rhythm, independent of segment recycling.
        if (Time.timeScale > 0f && Time.time >= nextTrafficTime)
        {
            SpawnTrafficVehicle();
            nextTrafficTime = Time.time + Random.Range(trafficIntervalMin, trafficIntervalMax);
        }

        // Move coins up/down based on gliding state
        UpdateCoinHeights();
    }

    private void SpawnTrafficVehicle()
    {
        // Tutorial check removed (GameTutorialController deleted)

        // Collect prefabs that can actually drive.
        reusableMovingVehicleList.Clear();
        if (obstaclePrefabs != null)
        {
            for (int i = 0; i < obstaclePrefabs.Length; i++)
            {
                GameObject p = obstaclePrefabs[i];
                if (p != null && IsMovingVehiclePrefab(p))
                    reusableMovingVehicleList.Add(p);
            }
        }
        if (reusableMovingVehicleList.Count == 0) return;

        GameObject prefab = reusableMovingVehicleList[Random.Range(0, reusableMovingVehicleList.Count)];
        float z = playerTransform.position.z + trafficSpawnAhead;

        // Find the segment that contains the target Z so the vehicle is tracked and cleaned up with it.
        int segIndex = -1;
        for (int i = 0; i < activeSegments.Count; i++)
        {
            float s, e;
            GetSegmentZRange(activeSegments[i], out s, out e);
            if (z >= s && z <= e) { segIndex = i; break; }
        }
        if (segIndex < 0) return;

        SpawnedSegment seg = activeSegments[segIndex];
        if (seg.spawnedItems == null) return;

        // Try lanes in random order; safety checks guarantee a free lane and no impossible walls.
        ShuffleReusableLaneOrder();
        for (int i = 0; i < 3; i++)
        {
            int lane = reusableLaneOrder[i];
            Vector3 pos = new Vector3((lane - 1) * laneDistance, obstacleYOffset, z);
            float safeSpeed;
            if (!IsValidObstaclePosition(seg, pos, lane, prefab, out safeSpeed)) continue;

            GameObject go = SpawnDynamicItem(prefab, pos, seg.transform);
            if (go == null) return;

            go.tag = "Obstacle";
            EnsureNonTriggerCollider(go);
            seg.spawnedItems.Add(go);
            ConfigureVehicleMover(go, prefab.name, safeSpeed);

            if (CurvedWorldManager.Instance != null)
                CurvedWorldManager.Instance.ApplyCurvatureToGameObject(go);
            return;
        }
    }

    private void UpdateCoinHeights()
    {
        // Gliding state tracking was in SimplePlayerController (now removed).
        // Coins stay at default height.
    }

    private void SpawnSegment(bool isStarter = false)
    {
        GameObject prefab = GetRandomRoadPrefab();
        GameObject segmentObj;

        if (prefab != null)
        {
            segmentObj = GetRoadSegmentFromPool(prefab);
        }
        else
        {
            // Fallback primitive
            segmentObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            segmentObj.name = $"SimpleFallbackRoad_{spawnZ}";
            segmentObj.transform.localScale = new Vector3(1.5f, 1f, segmentLength / 10f);
            segmentObj.transform.parent = transform;

            Renderer r = segmentObj.GetComponent<Renderer>();
            if (r != null)
            {
                r.sharedMaterial = new Material(Shader.Find("Standard"));
                r.sharedMaterial.color = new Color(0.2f, 0.2f, 0.2f);
            }
        }

        // Detect and cache Spawn points
        Transform spawnPoint = FindChildIgnoreCase(segmentObj.transform, "spawnPoint");
        Transform spawnEndPoint = FindChildIgnoreCase(segmentObj.transform, "SpawnEndPoint");

        float startLocalZ = spawnPoint != null ? spawnPoint.localPosition.z : 0f;
        float actualLength = segmentLength;

        if (spawnEndPoint != null)
        {
            actualLength = spawnEndPoint.localPosition.z - startLocalZ;
        }

        // Place so its local spawnPoint aligns exactly with current world spawnZ
        segmentObj.transform.position = new Vector3(0f, 0f, spawnZ - startLocalZ);

        SpawnedSegment seg = new SpawnedSegment
        {
            gameObject = segmentObj,
            transform = segmentObj.transform,
            spawnPoint = spawnPoint,
            spawnEndPoint = spawnEndPoint,
            length = actualLength,
            isStarter = isStarter,
            spawnedItems = new List<GameObject>(),
            prefab = isStarter ? null : prefab
        };

        SpawnItemsOnSegment(ref seg);

        activeSegments.Add(seg);
        spawnZ += actualLength;
    }

    private void RegisterStarterRoad()
    {
        GameObject segmentObj = starterRoad.gameObject;

        Transform spawnPoint = FindChildIgnoreCase(segmentObj.transform, "spawnPoint");
        Transform spawnEndPoint = FindChildIgnoreCase(segmentObj.transform, "SpawnEndPoint");

        float startLocalZ = spawnPoint != null ? spawnPoint.localPosition.z : 0f;
        float actualLength = segmentLength;

        if (spawnEndPoint != null)
        {
            actualLength = spawnEndPoint.localPosition.z - startLocalZ;
        }

        if (actualLength <= 0f)
        {
            actualLength = segmentLength;
        }

        spawnZ = spawnEndPoint != null ? spawnEndPoint.position.z : starterRoad.position.z + actualLength;

        // Apply the same mobile/optimization setup and curvature as spawned segments
        if (clearBakedObstacles) OptimizeAndClearObstacles(segmentObj);
        OptimizeTileForMobile(segmentObj);
        if (CurvedWorldManager.Instance != null)
        {
            CurvedWorldManager.Instance.ApplyCurvatureToGameObject(segmentObj);
        }

        if (!clearBakedObstacles)
            EnsureBakedObstaclesVisible(segmentObj);

        SpawnedSegment seg = new SpawnedSegment
        {
            gameObject = segmentObj,
            transform = segmentObj.transform,
            spawnPoint = spawnPoint,
            spawnEndPoint = spawnEndPoint,
            length = actualLength,
            isStarter = true,
            spawnedItems = new List<GameObject>(),
            prefab = null
        };

        activeSegments.Add(seg);
    }

    private void RecycleSegment()
    {
        SpawnedSegment oldestSegment = activeSegments[0];
        activeSegments.RemoveAt(0);

        if (oldestSegment.gameObject != null)
        {
            // Return any leftover dynamic items to their pools so they are not destroyed.
            ClearSegmentItems(ref oldestSegment);

            if (oldestSegment.isStarter)
            {
                // Starter road is a scene object; destroy it.
                Destroy(oldestSegment.gameObject);
            }
            else if (oldestSegment.prefab != null)
            {
                oldestSegment.gameObject.SetActive(false);
                Queue<GameObject> pool;
                if (roadSegmentPools.TryGetValue(oldestSegment.prefab, out pool))
                {
                    pool.Enqueue(oldestSegment.gameObject);
                }
                else
                {
                    Destroy(oldestSegment.gameObject);
                }
            }
            else
            {
                Destroy(oldestSegment.gameObject);
            }
        }

        // Spawn a fresh segment using the non-repeating road selection so the same prefab is never reused until all others have spawned.
        SpawnSegment(false);
    }

    private Transform FindChildIgnoreCase(Transform parent, string name)
    {
        Transform t = parent.Find(name);
        if (t != null) return t;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (string.Equals(child.name, name, System.StringComparison.OrdinalIgnoreCase))
            {
                return child;
            }
        }
        return null;
    }

    private GameObject GetRandomRoadPrefab()
    {
        if (roadPrefabs == null || roadPrefabs.Length == 0) return null;
        if (roadPrefabs.Length == 1) return roadPrefabs[0];

        if (roadPrefabPool == null || roadPrefabPool.Count == 0)
        {
            // Refill the pool, but exclude the last spawned prefab so the same one doesn't repeat consecutively.
            roadPrefabPool = new List<GameObject>(roadPrefabs);
            if (lastRoadPrefab != null && roadPrefabPool.Contains(lastRoadPrefab))
                roadPrefabPool.Remove(lastRoadPrefab);

            Shuffle(roadPrefabPool);
        }

        GameObject selected = roadPrefabPool[roadPrefabPool.Count - 1];
        roadPrefabPool.RemoveAt(roadPrefabPool.Count - 1);
        lastRoadPrefab = selected;
        return selected;
    }

    private void InitializeRoadPrefabPool()
    {
        if (roadPrefabs == null || roadPrefabs.Length == 0) return;

        roadPrefabPool = new List<GameObject>(roadPrefabs);
        Shuffle(roadPrefabPool);
        lastRoadPrefab = null;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    private void ShuffleReusableLaneOrder()
    {
        reusableLaneOrder[0] = 0;
        reusableLaneOrder[1] = 1;
        reusableLaneOrder[2] = 2;
        for (int i = 2; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = reusableLaneOrder[i];
            reusableLaneOrder[i] = reusableLaneOrder[j];
            reusableLaneOrder[j] = temp;
        }
    }

    private void SpawnItemsOnSegment(ref SpawnedSegment seg)
    {
        if (seg.isStarter)
            return;

        ClearSegmentItems(ref seg);

        float startZ, endZ;
        GetSegmentZRange(seg, out startZ, out endZ);

        float usableStart = startZ + spawnZMargin;
        float usableEnd = endZ - spawnZMargin;
        if (usableEnd - usableStart < 1f)
            return;

        // Spawn obstacles (vehicles, barriers, etc.)
        reusableObstacleCandidates.Clear();
        if (obstaclePrefabs != null)
            reusableObstacleCandidates.AddRange(obstaclePrefabs);
        if (crateBarrierPrefab != null && !reusableObstacleCandidates.Contains(crateBarrierPrefab))
            reusableObstacleCandidates.Add(crateBarrierPrefab);

        // Weight barriers higher so they spawn more often than other obstacles.
        if (crateBarrierPrefab != null)
        {
            for (int w = 0; w < barrierSpawnWeight; w++)
                reusableObstacleCandidates.Add(crateBarrierPrefab);
        }

        if (reusableObstacleCandidates.Count > 0)
        {
            // Each segment blocks two separate lanes. The one safe lane is changed every time,
            // creating a consistent dodge rhythm instead of leaving one lane empty for a long time.
            bool[] laneUsedThisSegment = new bool[3];
            int safeLane = GetNextSafeLane();
            int firstBlockedLane = (safeLane + 1) % 3;
            int secondBlockedLane = (safeLane + 2) % 3;

            for (int i = 0; i < 2; i++)
            {
                int preferredLane = (i == 0) ? firstBlockedLane : secondBlockedLane;
                bool spawned = false;
                // Try a few different prefabs in case one (e.g. a moving vehicle) can't fit safely.
                for (int prefabAttempt = 0; prefabAttempt < 5; prefabAttempt++)
                {
                    GameObject prefab = GetObstaclePrefab(reusableObstacleCandidates);
                    Vector3 pos;
                    float safeSpeed;
                    if (TryGetObstaclePosition(seg, usableStart, usableEnd, laneUsedThisSegment, prefab, preferredLane, out pos, out safeSpeed))
                    {
                        GameObject go = SpawnDynamicItem(prefab, pos, seg.transform);
                        if (go != null)
                        {
                            go.tag = "Obstacle";
                            EnsureNonTriggerCollider(go);
                            seg.spawnedItems.Add(go);

                            ConfigureVehicleMover(go, prefab.name, safeSpeed);

                            if (CurvedWorldManager.Instance != null)
                                CurvedWorldManager.Instance.ApplyCurvatureToGameObject(go);

                            spawned = true;
                            break;
                        }
                    }
                }
                if (!spawned)
                    break;
            }

            // Store the intended safe lane; the next segment will always choose a different one.
            lastFreeLane = safeLane;
        }

        // Spawn coins
        if (coinPrefab != null && Random.value < coinSpawnChance)
        {
            int count = Random.Range(minCoinsPerSegment, maxCoinsPerSegment + 1);
            int pattern = Random.Range(0, 3);

            for (int i = 0; i < count; i++)
            {
                Vector3 pos;
                if (TryGetCoinPosition(seg, usableStart, usableEnd, count, i, pattern, out pos))
                {
                    GameObject go = SpawnDynamicItem(coinPrefab, pos, seg.transform);
                    if (go != null)
                    {
                        go.tag = "Coin";
                        EnsureCoinCollider(go);
                        seg.spawnedItems.Add(go);

                        if (CurvedWorldManager.Instance != null)
                            CurvedWorldManager.Instance.ApplyCurvatureToGameObject(go);
                    }
                }
            }
        }

        // Spawn hang glider powerup
        if (gliderPrefab != null && Random.value < gliderSpawnChance)
        {
            Vector3 gliderPos;
            if (TryGetGliderPosition(seg, usableStart, usableEnd, out gliderPos))
            {
                GameObject go = SpawnDynamicItem(gliderPrefab, gliderPos, seg.transform);
                if (go != null)
                {
                    go.tag = "Untagged";
                    EnsureGliderCollider(go);
                    seg.spawnedItems.Add(go);

                    if (CurvedWorldManager.Instance != null)
                        CurvedWorldManager.Instance.ApplyCurvatureToGameObject(go);
                }
            }
        }

        // Spawn roadside characters
        if (roadsideCharacterPrefabs != null && roadsideCharacterPrefabs.Length > 0 && Random.value < roadsideCharacterSpawnChance)
        {
            int count = Random.Range(minRoadsideCharactersPerSegment, maxRoadsideCharactersPerSegment + 1);
            for (int i = 0; i < count; i++)
            {
                Vector3 pos;
                if (TryGetRoadsideCharacterPosition(seg, usableStart, usableEnd, out pos))
                {
                    GameObject prefab = roadsideCharacterPrefabs[Random.Range(0, roadsideCharacterPrefabs.Length)];
                    GameObject go = SpawnDynamicItem(prefab, pos, seg.transform);
                    if (go != null)
                    {
                        go.transform.rotation = Quaternion.LookRotation(Vector3.forward);
                        float walkRangeZ = Mathf.Max(1f, seg.length * 0.4f);
                        PrepareRoadsideCharacter(go, walkRangeZ);
                        seg.spawnedItems.Add(go);

                        if (CurvedWorldManager.Instance != null)
                            CurvedWorldManager.Instance.ApplyCurvatureToGameObject(go);
                    }
                }
            }
        }
    }

    private void GetSegmentZRange(SpawnedSegment seg, out float startZ, out float endZ)
    {
        if (seg.spawnPoint != null && seg.spawnEndPoint != null)
        {
            startZ = seg.spawnPoint.position.z;
            endZ = seg.spawnEndPoint.position.z;
        }
        else
        {
            startZ = seg.transform.position.z;
            endZ = seg.transform.position.z + seg.length;
        }
    }

    private int GetNextSafeLane()
    {
        if (lastFreeLane < 0)
            return Random.Range(0, 3);

        // Pick either of the other lanes, never leaving the same lane safe twice in a row.
        return (lastFreeLane + Random.Range(1, 3)) % 3;
    }

    private bool TryGetObstaclePosition(SpawnedSegment seg, float startZ, float endZ, bool[] usedLanes, GameObject proposedPrefab, int preferredLane, out Vector3 pos, out float safeSpeed)
    {
        pos = Vector3.zero;
        safeSpeed = 0f;

        if (preferredLane < 0 || preferredLane >= 3)
            return false;
        if (usedLanes != null && usedLanes[preferredLane])
            return false;

        for (int attempt = 0; attempt < 20; attempt++)
        {
            // This segment has a planned lane pattern; never place an obstacle in its safe lane.
            int lane = preferredLane;
            float laneX = (lane - 1) * laneDistance;
            float z = Random.Range(startZ, endZ);
            pos = new Vector3(laneX, obstacleYOffset, z);

            if (IsValidObstaclePosition(seg, pos, lane, proposedPrefab, out safeSpeed))
            {
                if (usedLanes != null)
                    usedLanes[lane] = true;
                return true;
            }
        }
        return false;
    }

    private bool IsValidObstaclePosition(SpawnedSegment currentSeg, Vector3 pos, int lane, GameObject proposedPrefab, out float safeSpeed)
    {
        safeSpeed = 0f;

        VehicleMover proposedMover = null;
        bool proposedIsMoving = false;
        if (proposedPrefab != null)
        {
            proposedMover = proposedPrefab.GetComponent<VehicleMover>();
            proposedIsMoving = proposedMover != null && proposedMover.direction == VehicleMover.MoveDir.AgainstPlayer;
        }

        float minV = (proposedMover != null) ? proposedMover.minSpeed : 0f;
        float maxV = (proposedMover != null) ? proposedMover.maxSpeed : 0f;

        // Check spacing against obstacles in the current segment AND all active segments.
        if (!ProcessSegmentForObstaclePosition(currentSeg, pos, proposedIsMoving, ref minV, ref maxV))
            return false;

        foreach (SpawnedSegment seg in activeSegments)
        {
            if (seg.gameObject == currentSeg.gameObject) continue;
            if (!ProcessSegmentForObstaclePosition(seg, pos, proposedIsMoving, ref minV, ref maxV))
                return false;
        }

        if (proposedIsMoving)
        {
            if (minV > maxV)
                return false;
            safeSpeed = Random.Range(minV, maxV);
        }

        if (ensureOneLaneClear && AreAllLanesBlockedAtZ(currentSeg, pos.z, lane))
            return false;

        int laneObstacleLimit = Mathf.Clamp(maxObstaclesPerLaneInView, 1, 3);
        if (CountNearbyObstaclesInLane(pos, lane) >= laneObstacleLimit)
            return false;

        return true;
    }

    private GameObject GetObstaclePrefab(List<GameObject> obstacleCandidates)
    {
        reusableMovingVehicleList.Clear();
        for (int i = 0; i < obstacleCandidates.Count; i++)
        {
            GameObject candidate = obstacleCandidates[i];
            if (candidate != null && IsMovingVehiclePrefab(candidate))
                reusableMovingVehicleList.Add(candidate);
        }

        if (reusableMovingVehicleList.Count > 0 && Random.value < movingVehicleSpawnChance)
            return reusableMovingVehicleList[Random.Range(0, reusableMovingVehicleList.Count)];

        return obstacleCandidates[Random.Range(0, obstacleCandidates.Count)];
    }

    private int CountNearbyObstaclesInLane(Vector3 pos, int lane)
    {
        int count = 0;

        foreach (SpawnedSegment seg in activeSegments)
        {
            if (seg.spawnedItems == null) continue;

            foreach (GameObject go in seg.spawnedItems)
            {
                if (go == null || go.CompareTag("Coin")) continue;

                int otherLane = Mathf.RoundToInt((go.transform.position.x / laneDistance) + 1);
                if (otherLane == lane && Mathf.Abs(go.transform.position.z - pos.z) <= laneOccupancyRange)
                    count++;
            }
        }

        return count;
    }

    private bool ProcessSegmentForObstaclePosition(SpawnedSegment seg, Vector3 pos, bool proposedIsMoving, ref float minV, ref float maxV)
    {
        if (seg.spawnedItems == null) return true;

        foreach (GameObject go in seg.spawnedItems)
        {
            if (go == null || go.CompareTag("Coin")) continue;

            Vector3 other = go.transform.position;
            float xDiff = Mathf.Abs(pos.x - other.x);
            float zDiff = Mathf.Abs(pos.z - other.z);

            VehicleMover existingMover = go.GetComponent<VehicleMover>();
            bool existingIsMoving = existingMover != null && existingMover.direction == VehicleMover.MoveDir.AgainstPlayer;

            float spacing = (proposedIsMoving || existingIsMoving) ? movingVehicleSpacing : minObstacleSpacing;

            if (xDiff < 0.5f)
            {
                if (zDiff < spacing)
                    return false;

                if (proposedIsMoving)
                {
                    float otherSpeed = existingIsMoving ? existingMover.GetSpeed() : 0f;
                    float dz = other.z - pos.z;

                    if (dz < 0f)
                    {
                        // Existing is ahead of the proposed moving vehicle.
                        // Proposed is behind, so it must not be faster than the vehicle ahead.
                        if (otherSpeed <= 0f)
                            return false; // static ahead: moving vehicle would catch it

                        maxV = Mathf.Min(maxV, otherSpeed);
                    }
                    else if (dz > 0f)
                    {
                        // Existing is behind the proposed moving vehicle.
                        // Proposed must be at least as fast so it isn't caught from behind.
                        if (otherSpeed > 0f)
                            minV = Mathf.Max(minV, otherSpeed);
                        // static behind is safe, moving vehicle moves away from it
                    }
                    else
                    {
                        return false; // exact same Z in same lane
                    }
                }
                else if (existingIsMoving)
                {
                    float dz = other.z - pos.z;
                    // Existing moving vehicle is ahead of proposed static barrier and will run into it.
                    if (dz > 0f)
                        return false;
                    // existing moving behind static is safe, it moves away
                }
            }
            else
            {
                if (zDiff < spacing * 0.5f)
                    return false;
            }
        }

        return true;
    }

    private void ConfigureVehicleMover(GameObject go, string prefabName, float safeSpeed)
    {
        if (go == null) return;

        VehicleMover mover = go.GetComponent<VehicleMover>();
        if (mover == null) return;

        mover.poolTag = prefabName;
        if (safeSpeed > 0f)
            mover.SetCruiseSpeed(safeSpeed);
    }

    private bool AreAllLanesBlockedAtZ(SpawnedSegment currentSeg, float z, int proposedLane)
    {
        reusableBlockedLanes[0] = false;
        reusableBlockedLanes[1] = false;
        reusableBlockedLanes[2] = false;
        reusableBlockedLanes[proposedLane] = true;

        UpdateBlockedLanes(currentSeg, z, reusableBlockedLanes);

        foreach (SpawnedSegment seg in activeSegments)
        {
            if (seg.gameObject == currentSeg.gameObject) continue;
            UpdateBlockedLanes(seg, z, reusableBlockedLanes);
        }

        return reusableBlockedLanes[0] && reusableBlockedLanes[1] && reusableBlockedLanes[2];
    }

    private void UpdateBlockedLanes(SpawnedSegment seg, float z, bool[] lanes)
    {
        if (seg.spawnedItems == null) return;

        foreach (GameObject go in seg.spawnedItems)
        {
            if (go == null || go.CompareTag("Coin"))
                continue;

            Vector3 pos = go.transform.position;
            if (Mathf.Abs(pos.z - z) < sameZWindow)
            {
                int lane = Mathf.RoundToInt((pos.x / laneDistance) + 1);
                if (lane >= 0 && lane < 3)
                    lanes[lane] = true;
            }
        }
    }

    private bool TryGetCoinPosition(SpawnedSegment seg, float startZ, float endZ, int count, int index, int pattern, out Vector3 pos)
    {
        pos = Vector3.zero;
        for (int attempt = 0; attempt < 15; attempt++)
        {
            int lane;
            float z;

            switch (pattern)
            {
                case 0: // straight line
                    lane = Random.Range(0, 3);
                    z = startZ + (endZ - startZ) * (index + 1) / (count + 1);
                    break;
                case 1: // zigzag
                    lane = (index % 2 == 0) ? 0 : 2;
                    z = startZ + (endZ - startZ) * (index + 1) / (count + 1);
                    break;
                default: // scattered
                    lane = Random.Range(0, 3);
                    z = Random.Range(startZ, endZ);
                    break;
            }

            float laneX = (lane - 1) * laneDistance;
            pos = new Vector3(laneX, coinYOffset, z);

            if (IsValidCoinPosition(seg, pos))
                return true;
        }
        return false;
    }

    private bool IsValidCoinPosition(SpawnedSegment seg, Vector3 pos)
    {
        foreach (GameObject go in seg.spawnedItems)
        {
            if (go == null) continue;

            float dist = Vector3.Distance(pos, go.transform.position);

            if (go.CompareTag("Coin"))
            {
                if (dist < minCoinSpacing)
                    return false;
            }
            else
            {
                if (dist < minObstacleSpacing * 0.5f)
                    return false;
            }
        }
        return true;
    }

    private bool TryGetRoadsideCharacterPosition(SpawnedSegment seg, float startZ, float endZ, out Vector3 pos)
    {
        pos = Vector3.zero;
        for (int attempt = 0; attempt < 20; attempt++)
        {
            bool isRight = Random.value > 0.5f;
            float x = isRight ? roadsideXOffset : -roadsideXOffset;
            float z = Random.Range(startZ, endZ);
            pos = new Vector3(x, 0f, z);

            if (IsValidRoadsideCharacterPosition(seg, pos))
                return true;
        }
        return false;
    }

    private bool IsValidRoadsideCharacterPosition(SpawnedSegment seg, Vector3 pos)
    {
        foreach (GameObject go in seg.spawnedItems)
        {
            if (go == null) continue;

            float dist = Vector3.Distance(pos, go.transform.position);
            if (dist < minRoadsideCharacterSpacing)
                return false;
        }
        return true;
    }

    private void PrepareRoadsideCharacter(GameObject go, float walkRangeZ)
    {
        // Make sure the existing RoadsideCharacter behavior is attached and configured
        RoadsideCharacter roadside = go.GetComponent<RoadsideCharacter>();
        if (roadside == null)
            roadside = go.AddComponent<RoadsideCharacter>();

        // Walk back-and-forth along the road, with rangeX = 0 so it never drifts sideways onto the road.
        roadside.Initialize(RoadsideCharacter.MovementType.BackAndForth, 0f, walkRangeZ);

        // Keep decorative characters from blocking the runner
        Collider col = go.GetComponent<Collider>();
        if (col != null && !col.isTrigger)
            col.isTrigger = true;
    }

    private void ClearSegmentItems(ref SpawnedSegment seg)
    {
        if (seg.spawnedItems == null)
            return;

        for (int i = seg.spawnedItems.Count - 1; i >= 0; i--)
        {
            GameObject go = seg.spawnedItems[i];
            if (go == null)
                continue;

            // Only clean up objects still owned by this segment.
            // Collected coins may already be pooled/destroyed.
            if (go.transform.parent == seg.transform)
                ReturnDynamicItem(go);
        }

        seg.spawnedItems.Clear();
    }

    private Quaternion GetDynamicItemSpawnRotation(GameObject prefab)
    {
        switch (prefab.name)
        {
            case "danfo-bus":
            case "keke napep":
            case "tank+truck+3d+model":
                return Quaternion.Euler(0f, 180f, 0f);
            default:
                return Quaternion.identity;
        }
    }

    private GameObject SpawnDynamicItem(GameObject prefab, Vector3 position, Transform parent)
    {
        if (prefab == null) return null;

        Quaternion rotation = GetDynamicItemSpawnRotation(prefab);
        if (ObjectPoolManager.Instance != null)
        {
            GameObject go = ObjectPoolManager.Instance.SpawnFromPool(prefab.name, position, rotation);
            if (go != null)
            {
                go.transform.SetParent(parent, false);
                go.transform.position = position;
                EnsureSpawnedVisible(go);
                return go;
            }
        }

        GameObject instanced = Instantiate(prefab, position, rotation, parent);
        EnsureSpawnedVisible(instanced);
        return instanced;
    }

    private void EnsureSpawnedVisible(GameObject go)
    {
        if (go == null) return;

        go.SetActive(true);

        int instanceID = go.GetInstanceID();
        if (visibilityFixedInstances.Contains(instanceID))
            return;
        visibilityFixedInstances.Add(instanceID);

        // Some GLB source prefabs (e.g. compact car) have inactive children or disabled renderers.
        // Force the whole object visible so the player can see what hit them.
        Transform[] children = go.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != null)
                children[i].gameObject.SetActive(true);
        }

        Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].enabled = true;
        }
    }

    private void EnsureBakedObstaclesVisible(GameObject road)
    {
        if (road == null) return;

        Transform[] allChildren = road.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < allChildren.Length; i++)
        {
            Transform t = allChildren[i];
            if (t == null) continue;

            GameObject go = t.gameObject;
            bool isObstacle = go.CompareTag("Obstacle") || go.GetComponent<VehicleMover>() != null;
            if (!isObstacle) continue;

            // Try to make it fully visible first.
            EnsureSpawnedVisible(go);

            // An obstacle with a collider but no visible mesh (or only tiny leftover lights) is an invisible hazard.
            // Disable its colliders so the player cannot be hit by something they can't see.
            Collider[] colliders = go.GetComponentsInChildren<Collider>(true);
            Bounds colliderBounds = new Bounds();
            bool hasSolidCollider = false;
            for (int c = 0; c < colliders.Length; c++)
            {
                if (colliders[c] == null || colliders[c].isTrigger) continue;
                if (!hasSolidCollider) { colliderBounds = colliders[c].bounds; hasSolidCollider = true; }
                else colliderBounds.Encapsulate(colliders[c].bounds);
            }
            if (!hasSolidCollider) continue;

            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
            Bounds rendererBounds = new Bounds();
            bool hasRenderer = false;
            for (int r = 0; r < renderers.Length; r++)
            {
                Renderer rend = renderers[r];
                if (rend == null || !rend.enabled || !rend.gameObject.activeInHierarchy) continue;
                if (!hasRenderer) { rendererBounds = rend.bounds; hasRenderer = true; }
                else rendererBounds.Encapsulate(rend.bounds);
            }

            float colliderVolume = colliderBounds.size.x * colliderBounds.size.y * colliderBounds.size.z;
            float rendererVolume = hasRenderer ? rendererBounds.size.x * rendererBounds.size.y * rendererBounds.size.z : 0f;
            float minVisibleVolume = Mathf.Max(0.001f, colliderVolume * 0.15f);

            if (rendererVolume < minVisibleVolume)
            {
                for (int c = 0; c < colliders.Length; c++)
                {
                    if (colliders[c] != null && !colliders[c].isTrigger)
                        colliders[c].enabled = false;
                }

                Debug.LogWarning($"[SimpleRoadSpawner] Baked obstacle '{go.name}' has no usable visible mesh (renderer volume {rendererVolume:F2} vs collider volume {colliderVolume:F2}). Its colliders have been disabled so it cannot cause invisible collisions.");
            }
        }
    }

    private void ReturnDynamicItem(GameObject go)
    {
        if (go == null) return;

        if (ObjectPoolManager.Instance != null)
            ObjectPoolManager.Instance.ReturnToPool(go);
        else
            Destroy(go);
    }

    private void EnsureNonTriggerCollider(GameObject go)
    {
        Collider col = go.GetComponentInChildren<Collider>();
        if (col == null)
        {
            BoxCollider box = go.AddComponent<BoxCollider>();
            box.size = new Vector3(1.5f, 2f, 1.5f);
            box.center = new Vector3(0f, 1f, 0f);
            col = box;
        }

        if (col.isTrigger)
            col.isTrigger = false;

        go.tag = "Obstacle";
    }

    private void EnsureCoinCollider(GameObject go)
    {
        Collider col = go.GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphere = go.AddComponent<SphereCollider>();
            sphere.radius = 0.5f;
            col = sphere;
        }

        col.isTrigger = true;

        if (go.GetComponent<Coin>() == null)
            go.AddComponent<Coin>();
    }

    private bool TryGetGliderPosition(SpawnedSegment seg, float startZ, float endZ, out Vector3 pos)
    {
        pos = Vector3.zero;
        for (int attempt = 0; attempt < 15; attempt++)
        {
            int lane = Random.Range(0, 3);
            float laneX = (lane - 1) * laneDistance;
            float z = Random.Range(startZ, endZ);
            pos = new Vector3(laneX, gliderYOffset, z);

            if (IsValidGliderPosition(seg, pos))
                return true;
        }
        return false;
    }

    private bool IsValidGliderPosition(SpawnedSegment seg, Vector3 pos)
    {
        if (seg.spawnedItems == null) return true;

        foreach (GameObject go in seg.spawnedItems)
        {
            if (go == null) continue;

            float dist = Vector3.Distance(pos, go.transform.position);
            if (dist < minObstacleSpacing)
                return false;
        }
        return true;
    }

    private void EnsureGliderCollider(GameObject go)
    {
        Collider col = go.GetComponent<Collider>();
        if (col == null)
        {
            BoxCollider box = go.AddComponent<BoxCollider>();
            box.size = new Vector3(2f, 2f, 2f);
            col = box;
        }

        col.isTrigger = true;

        if (go.GetComponent<HangGliderPowerup>() == null)
            go.AddComponent<HangGliderPowerup>();
    }

    /// <summary>
    /// Deactivates obstacle children once upon instantiation so that 
    /// the main runner track is clean and high-performance.
    /// </summary>
    private void OptimizeAndClearObstacles(GameObject seg)
    {
        string[] obstacleKeywords = { "danfo", "keke", "car", "barrier", "porthole", "shop", "bakery", "kiosk", "koiks" };
        ClearChildrenWithKeywords(seg.transform, obstacleKeywords);
    }

    /// <summary>
    /// Strips prop weight that budget phones cannot afford to keep the game running smoothly.
    /// Disables realtime lights, stops renderers from casting/receiving shadows, and disables light/reflection probes.
    /// Runs only on fresh Instantiate, so pooled/recycled tiles pay nothing.
    /// </summary>
    private void OptimizeTileForMobile(GameObject road)
    {
#if UNITY_ANDROID || UNITY_IOS
        Light[] lights = road.GetComponentsInChildren<Light>(true);
        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].enabled = false;
        }

        Renderer[] renderers = road.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer rend = renderers[i];
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend.receiveShadows = false;
            rend.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            rend.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        }

        Animator[] animators = road.GetComponentsInChildren<Animator>(true);
        for (int i = 0; i < animators.Length; i++)
        {
            animators[i].cullingMode = AnimatorCullingMode.CullCompletely;
        }
#endif
    }

    private void ClearChildrenWithKeywords(Transform parent, string[] keywords)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            string nameLower = child.name.ToLower();

            bool isObstacle = false;
            foreach (var kw in keywords)
            {
                if (nameLower.Contains(kw))
                {
                    isObstacle = true;
                    break;
                }
            }

            // Exceptions: Do not disable essential environment or support elements
            if (nameLower.Contains("componett") || nameLower.Contains("point") || nameLower.Contains("light"))
            {
                isObstacle = false;
            }

            if (isObstacle)
            {
                child.gameObject.SetActive(false);
            }
            else if (child.childCount > 0)
            {
                ClearChildrenWithKeywords(child, keywords);
            }
        }
    }
}


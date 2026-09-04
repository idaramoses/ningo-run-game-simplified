using UnityEngine;
using System.Collections.Generic;

namespace SynthwaveRunner
{
    public class SynthwaveGridManager : MonoBehaviour
    {
        public static SynthwaveGridManager Instance { get; private set; }

        [Header("Prefabs for Pooling")]
        public GameObject roadSegmentPrefab;
        public GameObject dataBitPrefab;
        public GameObject obstacleSpikePrefab;    // Jump over
        public GameObject obstacleLaserPrefab;    // Slide under
        public GameObject obstacleWallPrefab;     // Switch lane
        public GameObject sideTowerPrefab;        // Side deco

        [Header("Grid Layout Settings")]
        public int initialSegmentsCount = 6;
        public float segmentLength = 30f;
        public float recycleThreshold = -30f;
        
        [Header("Side Decoration Settings")]
        public int initialTowersCount = 10;
        public float towerSpacing = 25f;
        public float towerOffset = 8f; // Distance from center lane

        [Header("Pool Initial Sizes")]
        public int roadPoolSize = 8;
        public int dataBitPoolSize = 30;
        public int spikePoolSize = 10;
        public int laserPoolSize = 10;
        public int wallPoolSize = 10;
        public int towerPoolSize = 15;

        // Custom, garbage-free Object Pools
        private readonly Queue<GameObject> roadPool = new Queue<GameObject>();
        private readonly Queue<GameObject> dataBitPool = new Queue<GameObject>();
        private readonly Queue<GameObject> spikePool = new Queue<GameObject>();
        private readonly Queue<GameObject> laserPool = new Queue<GameObject>();
        private readonly Queue<GameObject> wallPool = new Queue<GameObject>();
        private readonly Queue<GameObject> towerPool = new Queue<GameObject>();

        // Active instances in scene
        private readonly List<GameObject> activeSegments = new List<GameObject>();
        private readonly List<GameObject> activeTowers = new List<GameObject>();
        
        // Dictionary to track which active objects are currently spawned on which road segment
        private readonly Dictionary<GameObject, List<GameObject>> segmentToChildren = new Dictionary<GameObject, List<GameObject>>();

        private float farthestSegmentZ = 0f;
        private float farthestTowerZ = 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            InitializePools();
            BuildInitialTrack();

            if (SynthwaveGameManager.Instance != null)
            {
                SynthwaveGameManager.Instance.onStateChanged += OnGameStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (SynthwaveGameManager.Instance != null)
            {
                SynthwaveGameManager.Instance.onStateChanged -= OnGameStateChanged;
            }
        }

        private void OnGameStateChanged(SynthwaveGameManager.GameState state)
        {
            if (state == SynthwaveGameManager.GameState.Playing)
            {
                ResetTrack();
            }
        }

        private void Update()
        {
            if (SynthwaveGameManager.Instance == null || SynthwaveGameManager.Instance.currentState != SynthwaveGameManager.GameState.Playing)
            {
                return;
            }

            float scrollSpeed = SynthwaveGameManager.Instance.currentSpeed;
            float step = scrollSpeed * Time.deltaTime;

            // 1. Scroll and loop road segments
            for (int i = 0; i < activeSegments.Count; i++)
            {
                GameObject segment = activeSegments[i];
                segment.transform.Translate(0f, 0f, -step, Space.World);

                // If segment passes behind recycle threshold
                if (segment.transform.position.z <= recycleThreshold)
                {
                    RecycleSegment(segment);
                }
            }

            // 2. Scroll and loop side towers
            for (int i = 0; i < activeTowers.Count; i++)
            {
                GameObject tower = activeTowers[i];
                tower.transform.Translate(0f, 0f, -step, Space.World);

                if (tower.transform.position.z <= recycleThreshold)
                {
                    RecycleTower(tower);
                }
            }
        }

        #region Initialization & Pools

        private void InitializePools()
        {
            // Create root container for pools
            Transform poolsRoot = new GameObject("[SynthwavePools]").transform;
            poolsRoot.SetParent(transform);

            // Populate road pool
            PrepopulatePool(roadSegmentPrefab, roadPoolSize, roadPool, poolsRoot);
            
            // Populate items
            PrepopulatePool(dataBitPrefab, dataBitPoolSize, dataBitPool, poolsRoot);
            PrepopulatePool(obstacleSpikePrefab, spikePoolSize, spikePool, poolsRoot);
            PrepopulatePool(obstacleLaserPrefab, laserPoolSize, laserPool, poolsRoot);
            PrepopulatePool(obstacleWallPrefab, wallPoolSize, wallPool, poolsRoot);
            PrepopulatePool(sideTowerPrefab, towerPoolSize, towerPool, poolsRoot);
        }

        private void PrepopulatePool(GameObject prefab, int size, Queue<GameObject> queue, Transform parent)
        {
            if (prefab == null) return;
            for (int i = 0; i < size; i++)
            {
                GameObject obj = Instantiate(prefab, parent);
                obj.SetActive(false);
                queue.Enqueue(obj);
            }
        }

        #endregion

        #region Track Management

        private void BuildInitialTrack()
        {
            farthestSegmentZ = -segmentLength; // Start just behind player so the first one spawns at Z = 0
            
            for (int i = 0; i < initialSegmentsCount; i++)
            {
                SpawnNextSegment(i == 0); // First segment is guaranteed clean (no obstacles)
            }

            // Spawn initial side towers
            farthestTowerZ = 0f;
            for (int i = 0; i < initialTowersCount; i++)
            {
                SpawnTower(farthestTowerZ, i % 2 == 0);
                if (i % 2 == 1)
                {
                    farthestTowerZ += towerSpacing;
                }
            }
        }

        private void ResetTrack()
        {
            // Recycle all active child elements
            foreach (var segment in activeSegments)
            {
                ClearSegmentChildren(segment);
            }

            // Return segments to pool
            foreach (var segment in activeSegments)
            {
                segment.SetActive(false);
                segment.transform.SetParent(transform);
                roadPool.Enqueue(segment);
            }
            activeSegments.Clear();

            // Return towers to pool
            foreach (var tower in activeTowers)
            {
                tower.SetActive(false);
                tower.transform.SetParent(transform);
                towerPool.Enqueue(tower);
            }
            activeTowers.Clear();

            // Rebuild
            farthestSegmentZ = -segmentLength;
            for (int i = 0; i < initialSegmentsCount; i++)
            {
                SpawnNextSegment(i == 0);
            }

            farthestTowerZ = 0f;
            for (int i = 0; i < initialTowersCount; i++)
            {
                SpawnTower(farthestTowerZ, i % 2 == 0);
                if (i % 2 == 1)
                {
                    farthestTowerZ += towerSpacing;
                }
            }
        }

        private void SpawnNextSegment(bool cleanSegment)
        {
            if (roadPool.Count == 0) return;

            GameObject segment = roadPool.Dequeue();
            float spawnZ = farthestSegmentZ + segmentLength;
            segment.transform.position = new Vector3(0f, 0f, spawnZ);
            segment.transform.rotation = Quaternion.identity;
            segment.SetActive(true);
            
            activeSegments.Add(segment);
            farthestSegmentZ = spawnZ;

            if (!segmentToChildren.ContainsKey(segment))
            {
                segmentToChildren[segment] = new List<GameObject>();
            }

            if (!cleanSegment)
            {
                PopulateSegmentWithGameplay(segment);
            }
        }

        private void RecycleSegment(GameObject segment)
        {
            // 1. Return child obstacles and data bits to their pools
            ClearSegmentChildren(segment);

            // 2. Reposition segment at the far end
            float spawnZ = farthestSegmentZ + segmentLength;
            segment.transform.position = new Vector3(0f, 0f, spawnZ);
            segment.SetActive(true);

            farthestSegmentZ = spawnZ;

            // 3. Generate new gameplay layout on it
            PopulateSegmentWithGameplay(segment);
        }

        private void ClearSegmentChildren(GameObject segment)
        {
            if (!segmentToChildren.TryGetValue(segment, out List<GameObject> children)) return;

            for (int i = 0; i < children.Count; i++)
            {
                GameObject child = children[i];
                if (child == null) continue;

                child.SetActive(false);
                child.transform.SetParent(transform); // Return parent to grid manager root

                // Return to appropriate queue based on tag or name
                if (child.CompareTag("DataBit"))
                {
                    dataBitPool.Enqueue(child);
                }
                else if (child.name.Contains("Spike"))
                {
                    spikePool.Enqueue(child);
                }
                else if (child.name.Contains("Laser"))
                {
                    laserPool.Enqueue(child);
                }
                else if (child.name.Contains("Wall"))
                {
                    wallPool.Enqueue(child);
                }
            }

            children.Clear();
        }

        #endregion

        #region Side Deco

        private void SpawnTower(float zPos, bool leftSide)
        {
            if (towerPool.Count == 0) return;

            GameObject tower = towerPool.Dequeue();
            float xPos = leftSide ? -towerOffset : towerOffset;
            
            // Randomize height and depth scale slightly for sci-fi city silhouette variety
            float heightScale = Random.Range(0.8f, 1.8f);
            float widthScale = Random.Range(0.7f, 1.3f);
            tower.transform.localScale = new Vector3(widthScale, heightScale, widthScale);

            // Set height so its bottom rests on Y = 0
            float yPos = heightScale * 5f; // assuming standard base height is 10
            tower.transform.position = new Vector3(xPos, yPos - 5f, zPos);
            tower.transform.rotation = Quaternion.identity;
            tower.SetActive(true);

            activeTowers.Add(tower);
        }

        private void RecycleTower(GameObject tower)
        {
            activeTowers.Remove(tower);
            tower.SetActive(false);
            towerPool.Enqueue(tower);

            // Spawn at the far end
            float nextZ = farthestTowerZ + towerSpacing;
            farthestTowerZ = nextZ;

            // Determine side based on active instances ratio or simple parity
            bool leftSide = Random.value > 0.5f;
            SpawnTower(nextZ, leftSide);
        }

        #endregion

        #region Procedural Gameplay Populator (No-Allocation Layouts)

        private void PopulateSegmentWithGameplay(GameObject segment)
        {
            List<GameObject> children = segmentToChildren[segment];

            // Select a random layout pattern to inject (keeps gameplay engaging and unpredictable)
            int pattern = Random.Range(0, 6);

            float segZ = segment.transform.position.z;

            switch (pattern)
            {
                case 0: // Series of DataBits in Center Lane (easy)
                    SpawnDataBitLine(0, segZ - 10f, 3, children, segment.transform);
                    break;

                case 1: // Jump obstacle in center, bits on left/right
                    SpawnObstacle("Spike", 0, segZ, children, segment.transform);
                    SpawnDataBitLine(-1, segZ - 5f, 2, children, segment.transform);
                    SpawnDataBitLine(1, segZ - 5f, 2, children, segment.transform);
                    break;

                case 2: // Slide obstacle spanning center/left, bits on right
                    SpawnObstacle("Laser", 0, segZ, children, segment.transform);
                    SpawnDataBitLine(1, segZ - 8f, 3, children, segment.transform);
                    break;

                case 3: // Double Walls blocking Left & Center - must switch Right
                    SpawnObstacle("Wall", -1, segZ - 5f, children, segment.transform);
                    SpawnObstacle("Wall", 0, segZ - 5f, children, segment.transform);
                    SpawnDataBitLine(1, segZ, 2, children, segment.transform);
                    break;

                case 4: // Alternating lane obstacles (Zig-zag)
                    SpawnObstacle("Spike", -1, segZ - 10f, children, segment.transform);
                    SpawnObstacle("Spike", 0, segZ, children, segment.transform);
                    SpawnObstacle("Spike", 1, segZ + 10f, children, segment.transform);
                    // Reward bits in free lanes
                    SpawnDataBitLine(1, segZ - 10f, 1, children, segment.transform);
                    SpawnDataBitLine(-1, segZ + 10f, 1, children, segment.transform);
                    break;

                case 5: // Wall in Right, Laser in Center, Spike in Left
                    SpawnObstacle("Wall", 1, segZ - 5f, children, segment.transform);
                    SpawnObstacle("Laser", 0, segZ, children, segment.transform);
                    SpawnObstacle("Spike", -1, segZ + 5f, children, segment.transform);
                    break;
            }
        }

        private void SpawnDataBitLine(int laneIndex, float startZ, int count, List<GameObject> parentList, Transform roadParent)
        {
            float laneX = laneIndex * 2.5f;
            float spacing = 4f;

            for (int i = 0; i < count; i++)
            {
                if (dataBitPool.Count == 0) break;

                GameObject dataBit = dataBitPool.Dequeue();
                
                // Reposition
                dataBit.transform.position = new Vector3(laneX, 0.8f, startZ + (i * spacing));
                dataBit.transform.rotation = Quaternion.identity;
                dataBit.transform.SetParent(roadParent, true); // Keep relative movement
                dataBit.SetActive(true);

                parentList.Add(dataBit);
            }
        }

        private void SpawnObstacle(string type, int laneIndex, float zPos, List<GameObject> parentList, Transform roadParent)
        {
            GameObject obstacle = null;
            float laneX = laneIndex * 2.5f;
            float yPos = 0f;

            if (type == "Spike" && spikePool.Count > 0)
            {
                obstacle = spikePool.Dequeue();
                yPos = 0.5f; // rests on ground
            }
            else if (type == "Laser" && laserPool.Count > 0)
            {
                obstacle = laserPool.Dequeue();
                yPos = 1.8f; // floating (must slide under)
            }
            else if (type == "Wall" && wallPool.Count > 0)
            {
                obstacle = wallPool.Dequeue();
                yPos = 1.5f; // tall block
            }

            if (obstacle != null)
            {
                obstacle.transform.position = new Vector3(laneX, yPos, zPos);
                obstacle.transform.rotation = Quaternion.identity;
                obstacle.transform.SetParent(roadParent, true); // Scroll together
                obstacle.SetActive(true);

                parentList.Add(obstacle);
            }
        }

        #endregion
    }
}

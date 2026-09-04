using System.Collections.Generic;
using UnityEngine;

public class CloudManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The cloud prefabs to use (SM_Env_Cloud_01, 02, 03)")]
    [SerializeField] private GameObject[] cloudPrefabs;
    
    [Tooltip("The player/runner transform. Will find automatically if null.")]
    [SerializeField] private Transform player;

    [Header("Cloud Spawn Settings")]
    [SerializeField] private int maxCloudCount = 15;
    [SerializeField] private float minHeight = 35f;
    [SerializeField] private float maxHeight = 48f;
    [SerializeField] private float spawnWidth = 120f; // distance on X axis left and right
    [SerializeField] private float spawnForwardRange = 300f; // how far ahead of the player to spawn clouds
    [SerializeField] private float recycleDistanceBehind = 60f; // how far behind the player a cloud must be to recycle

    [Header("Wind & Movement Settings")]
    [SerializeField] private Vector3 windVelocity = new Vector3(1.5f, 0f, 0.5f);
    [SerializeField] private float minScale = 0.8f;
    [SerializeField] private float maxScale = 1.6f;

    private List<GameObject> activeClouds = new List<GameObject>();

    private void Awake()
    {
        // Find player if not assigned
        if (player == null)
        {
            FindActivePlayer();
        }
    }

    private void Start()
    {
        // If prefabs are not set, warn and disable
        if (cloudPrefabs == null || cloudPrefabs.Length == 0)
        {
            Debug.LogWarning("[CloudManager] No cloud prefabs assigned! Please assign the SM_Env_Cloud prefabs in the Inspector.");
            enabled = false;
            return;
        }

        // Adopt any pre-placed editor clouds in children
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.gameObject.activeSelf && !activeClouds.Contains(child.gameObject))
            {
                activeClouds.Add(child.gameObject);
                
                // Ensure curvature is applied to pre-placed clouds
                if (CurvedWorldManager.Instance != null)
                {
                    CurvedWorldManager.Instance.ApplyCurvatureToGameObject(child.gameObject);
                }
            }
        }

        // Spawn additional clouds if we have less than maxCloudCount
        int currentCount = activeClouds.Count;
        if (currentCount < maxCloudCount)
        {
            int toSpawn = maxCloudCount - currentCount;
            float startZ = player != null ? player.position.z - 30f : 0f;
            for (int i = 0; i < toSpawn; i++)
            {
                float progress = (float)i / toSpawn;
                float zPos = startZ + (progress * spawnForwardRange);
                SpawnCloudAtZ(zPos, true);
            }
        }
    }

    private void Update()
    {
        // Ensure player is reference-valid
        if (player == null)
        {
            FindActivePlayer();
            if (player == null) return;
        }

        // 1. Move active clouds (Wind)
        // 2. Check for recycling
        for (int i = activeClouds.Count - 1; i >= 0; i--)
        {
            GameObject cloud = activeClouds[i];
            if (cloud == null)
            {
                activeClouds.RemoveAt(i);
                continue;
            }

            // Apply wind movement
            cloud.transform.Translate(windVelocity * Time.deltaTime, Space.World);

            // Check if the cloud has fallen too far behind the player
            // Since player runs along positive Z, we check if cloud's Z is less than player's Z - recycleDistanceBehind
            if (cloud.transform.position.z < player.position.z - recycleDistanceBehind)
            {
                RecycleCloud(cloud);
            }
        }
    }

    private void SpawnCloudAtZ(float zPos, bool randomRotation)
    {
        if (cloudPrefabs == null || cloudPrefabs.Length == 0) return;

        int prefabIndex = Random.Range(0, cloudPrefabs.Length);
        GameObject prefab = cloudPrefabs[prefabIndex];
        if (prefab == null) return;

        // Randomize positions
        float xPos = Random.Range(-spawnWidth / 2f, spawnWidth / 2f);
        float yPos = Random.Range(minHeight, maxHeight);
        Vector3 position = new Vector3(xPos, yPos, zPos);

        // Randomize scale
        float scaleMultiplier = Random.Range(minScale, maxScale);
        Vector3 localScale = Vector3.one * scaleMultiplier;

        // Rotation
        Quaternion rotation = randomRotation ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) : Quaternion.identity;

        // Instantiate
        GameObject newCloud = Instantiate(prefab, position, rotation, transform);
        newCloud.name = $"Cloud_{prefabIndex}_{activeClouds.Count}";
        newCloud.transform.localScale = localScale;

        // Apply curvature so they curve with the world
        if (CurvedWorldManager.Instance != null)
        {
            CurvedWorldManager.Instance.ApplyCurvatureToGameObject(newCloud);
        }

        activeClouds.Add(newCloud);
    }

    private void RecycleCloud(GameObject cloud)
    {
        // Reposition cloud in front of the player
        float playerZ = player != null ? player.position.z : 0f;
        
        // Spawn it near the far end of our view range
        float xPos = Random.Range(-spawnWidth / 2f, spawnWidth / 2f);
        float yPos = Random.Range(minHeight, maxHeight);
        float zPos = playerZ + spawnForwardRange + Random.Range(-20f, 40f);

        cloud.transform.position = new Vector3(xPos, yPos, zPos);
        cloud.transform.localScale = Vector3.one * Random.Range(minScale, maxScale);
        cloud.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        // Re-apply curvature if the materials were re-cloned, though usually they carry their material over.
        // It doesn't hurt to update it.
        if (CurvedWorldManager.Instance != null)
        {
            CurvedWorldManager.Instance.ApplyCurvatureToGameObject(cloud);
        }
    }

    private void FindActivePlayer()
    {
        // Try tag first
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null && playerObj.activeInHierarchy)
        {
            player = playerObj.transform;
            return;
        }

        // Fallback to any active runner
        SimpleRunner runner = FindFirstObjectByType<SimpleRunner>();
        if (runner != null && runner.gameObject.activeInHierarchy)
        {
            player = runner.transform;
        }
    }
}

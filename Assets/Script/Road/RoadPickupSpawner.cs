using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Road pickup spawner for gems, skateboards, gliders, and coins.
/// Can be attached to road prefabs/segments or used alongside RoadSpawner.
/// </summary>
public class RoadPickupSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject gemPrefab;
    public GameObject skateboardPrefab;
    public GameObject gliderPrefab;
    public GameObject coinPrefab;

    [Header("Spawn Probabilities (per road segment)")]
    [Range(0f, 1f)] public float gemChance = 0.3f;
    [Range(0f, 1f)] public float skateboardChance = 0.2f;
    [Range(0f, 1f)] public float gliderChance = 0.15f;

    [Header("Lane & Placement")]
    public float[] lanes = { -2.5f, 0f, 2.5f };
    public float gemHeight = 1.0f;
    public float skateboardHeight = 0.8f;
    public float gliderHeight = 1.2f;
    public float marginZ = 5f;

    /// <summary>
    /// Spawns pickups on a given road segment transform along Z bounds.
    /// </summary>
    public void SpawnPickups(Transform segmentParent, float startZ, float length)
    {
        if (lanes == null || lanes.Length == 0) return;

        float minZ = startZ + marginZ;
        float maxZ = startZ + length - marginZ;

        // Gem spawn
        if (gemPrefab != null && Random.value < gemChance)
        {
            Vector3 pos = new Vector3(lanes[Random.Range(0, lanes.Length)], gemHeight, Random.Range(minZ, maxZ));
            SpawnItem(gemPrefab, pos, segmentParent);
        }

        // Skateboard spawn
        if (skateboardPrefab != null && Random.value < skateboardChance)
        {
            Vector3 pos = new Vector3(lanes[Random.Range(0, lanes.Length)], skateboardHeight, Random.Range(minZ, maxZ));
            SpawnItem(skateboardPrefab, pos, segmentParent);
        }

        // Glider spawn
        if (gliderPrefab != null && Random.value < gliderChance)
        {
            Vector3 pos = new Vector3(lanes[Random.Range(0, lanes.Length)], gliderHeight, Random.Range(minZ, maxZ));
            SpawnItem(gliderPrefab, pos, segmentParent);
        }
    }

    private GameObject SpawnItem(GameObject prefab, Vector3 position, Transform parent)
    {
        if (prefab == null) return null;
        GameObject go = Instantiate(prefab, position, Quaternion.identity, parent);

        Collider[] cols = go.GetComponentsInChildren<Collider>();
        if (cols.Length == 0)
        {
            SphereCollider sc = go.AddComponent<SphereCollider>();
            sc.radius = 0.8f;
            sc.isTrigger = true;
        }
        else
        {
            foreach (var col in cols)
            {
                if (col != null) col.isTrigger = true;
            }
        }

        if (CurvedWorldManager.Instance != null)
            CurvedWorldManager.Instance.ApplyCurvatureToGameObject(go);
        return go;
    }
}

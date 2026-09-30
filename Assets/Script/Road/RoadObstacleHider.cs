using UnityEngine;
using System.Collections.Generic;

public class RoadObstacleHider : MonoBehaviour
{
    public int minToHide = 3;
    public int maxToHide = 5;

    [Tooltip("TEST: hide every obstacle except one (kept = furthest ahead) per road tile.")]
    public bool keepOnlyOne = false;

    private bool hasHiddenThisEnable = true;

    void OnEnable()
    {
        if (!Application.isPlaying) return;
        hasHiddenThisEnable = false;
    }

    void LateUpdate()
    {
        if (hasHiddenThisEnable) return;
        hasHiddenThisEnable = true;
        HideRandomObstacles();
    }

    private void HideRandomObstacles()
    {
        List<GameObject> obstacles = new List<GameObject>();
        CollectObstacleRoots(transform, obstacles);

        if (obstacles.Count == 0) return;

        // Reset all obstacles to active first so recycled/re-enabled road tiles have a clean state
        for (int i = 0; i < obstacles.Count; i++)
        {
            if (obstacles[i] != null)
            {
                obstacles[i].SetActive(true);
            }
        }

        int hideCount;
        int keepIndex = -1;

        if (keepOnlyOne && obstacles.Count > 1)
        {
            hideCount = obstacles.Count - 1;
            // Keep the obstacle furthest ahead so the player gets runway to dodge it
            float bestZ = float.MinValue;
            for (int i = 0; i < obstacles.Count; i++)
            {
                float z = obstacles[i].transform.position.z - transform.position.z;
                if (z > bestZ) { bestZ = z; keepIndex = i; }
            }
        }
        else
        {
            hideCount = Mathf.Clamp(Random.Range(minToHide, maxToHide + 1), 0, obstacles.Count);
        }

        HashSet<int> hiddenIndices = new HashSet<int>();
        if (keepOnlyOne)
        {
            for (int i = 0; i < obstacles.Count; i++)
                if (i != keepIndex) hiddenIndices.Add(i);
        }
        else
        {
            while (hiddenIndices.Count < hideCount)
            {
                hiddenIndices.Add(Random.Range(0, obstacles.Count));
            }
        }

        for (int i = 0; i < obstacles.Count; i++)
        {
            GameObject obs = obstacles[i];
            if (obs == null) continue;

            bool shouldHide = hiddenIndices.Contains(i);
            if (shouldHide)
            {
                // Completely deactivate the entire obstacle unit (mesh, colliders, child components)
                obs.SetActive(false);
            }
            else
            {
                obs.SetActive(true);
                EnsureSolidCollider(obs);
            }
        }
    }

    private void EnsureSolidCollider(GameObject obs)
    {
        // Check if this obstacle or any of its children already has a collider
        Collider[] colliders = obs.GetComponentsInChildren<Collider>(true);
        if (colliders == null || colliders.Length == 0)
        {
            // Only add a BoxCollider if none exists anywhere in the obstacle hierarchy
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
            // Ensure all existing colliders on active obstacles are solid (non-trigger)
            foreach (var col in colliders)
            {
                if (col != null && col.isTrigger)
                    col.isTrigger = false;
            }
        }
    }

    /// <summary>
    /// Collects only top-level (root-most) GameObjects tagged "Obstacle" under the road segment.
    /// Does not recurse into children of an obstacle root, preventing duplicate child entries
    /// where child meshes get disabled while parent colliders stay active.
    /// </summary>
    private void CollectObstacleRoots(Transform current, List<GameObject> results)
    {
        for (int i = 0; i < current.childCount; i++)
        {
            Transform child = current.GetChild(i);
            if (child.CompareTag("Obstacle"))
            {
                results.Add(child.gameObject);
                // Do not recurse into children of an already collected obstacle root
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
}


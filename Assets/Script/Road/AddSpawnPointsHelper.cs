using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

/// <summary>
/// Helper script to automatically add SpawnPoints to road prefabs
/// </summary>
public class AddSpawnPointsHelper : MonoBehaviour
{
    [MenuItem("Tools/Add SpawnPoints to Selected Roads")]
    public static void AddSpawnPointsToSelected()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        int count = 0;

        foreach (GameObject obj in selectedObjects)
        {
            // Check if it already has a SpawnPoint
            Transform existingSpawnPoint = obj.transform.Find("SpawnPoint");
            if (existingSpawnPoint != null)
            {
                Debug.Log($"[AddSpawnPoints] {obj.name} already has a SpawnPoint, skipping");
                continue;
            }

            // Get the bounds of this road
            MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>();
            if (renderers.Length == 0)
            {
                Debug.LogWarning($"[AddSpawnPoints] {obj.name} has no MeshRenderers, skipping");
                continue;
            }

            // Calculate bounds
            Bounds bounds = renderers[0].bounds;
            foreach (MeshRenderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }

            // Create SpawnPoint at the end of the road (max Z)
            GameObject spawnPoint = new GameObject("SpawnPoint");
            spawnPoint.transform.SetParent(obj.transform);
            
            // Position at the end of the road
            Vector3 spawnPos = new Vector3(
                obj.transform.position.x,
                obj.transform.position.y,
                bounds.max.z
            );
            spawnPoint.transform.position = spawnPos;
            spawnPoint.transform.rotation = obj.transform.rotation;

            Debug.Log($"[AddSpawnPoints] Added SpawnPoint to {obj.name} at Z={spawnPos.z:F1}");
            count++;
        }

        Debug.Log($"[AddSpawnPoints] Added SpawnPoints to {count} objects");
    }

    [MenuItem("Tools/Add SpawnPoints to All Lagos Roads")]
    public static void AddSpawnPointsToAllLagosRoads()
    {
        // Find all GameObjects with "Lagos" and "Road" in their name
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        int count = 0;

        foreach (GameObject obj in allObjects)
        {
            if (!obj.name.Contains("Lagos") || !obj.name.Contains("Road"))
                continue;

            // Check if it already has a SpawnPoint
            Transform existingSpawnPoint = obj.transform.Find("SpawnPoint");
            if (existingSpawnPoint != null)
            {
                Debug.Log($"[AddSpawnPoints] {obj.name} already has a SpawnPoint, skipping");
                continue;
            }

            // Get the bounds of this road
            MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>();
            if (renderers.Length == 0)
            {
                Debug.LogWarning($"[AddSpawnPoints] {obj.name} has no MeshRenderers, skipping");
                continue;
            }

            // Calculate bounds
            Bounds bounds = renderers[0].bounds;
            foreach (MeshRenderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }

            // Create SpawnPoint at the end of the road (max Z)
            GameObject spawnPoint = new GameObject("SpawnPoint");
            spawnPoint.transform.SetParent(obj.transform);
            
            // Position at the end of the road
            Vector3 spawnPos = new Vector3(
                obj.transform.position.x,
                obj.transform.position.y,
                bounds.max.z
            );
            spawnPoint.transform.position = spawnPos;
            spawnPoint.transform.rotation = obj.transform.rotation;

            Debug.Log($"[AddSpawnPoints] Added SpawnPoint to {obj.name} at Z={spawnPos.z:F1}");
            count++;
        }

        Debug.Log($"[AddSpawnPoints] Added SpawnPoints to {count} Lagos roads");
    }
}
#endif

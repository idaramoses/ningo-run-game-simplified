using UnityEngine;

public class RoadVehicleSpawner : MonoBehaviour
{
    public Transform laneLeft;
    public Transform laneCenter;
    public Transform laneRight;

    public GameObject[] vehicles;

    public float spawnChance;
    public float minZ;
    public float maxZ;
    public float minDistanceBetweenVehicles;
}

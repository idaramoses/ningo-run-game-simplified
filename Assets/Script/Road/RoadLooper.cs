using UnityEngine;  // must always come first

public class RoadLooper : MonoBehaviour
{
    public Transform player;          // Amaka Runner
    public float roadLength = 150f;   // Length of one road piece
    public int numberOfRoads = 3;     // Total road segments

    private Transform[] roads;

    void Start()
    {
        roads = new Transform[numberOfRoads];
        for (int i = 0; i < numberOfRoads; i++)
        {
            roads[i] = transform.GetChild(i);
        }
    }

    void Update()
    {
        foreach (Transform road in roads)
        {
            // If the player has passed this road segment, move it to the back
            if (player.position.z - road.position.z > roadLength)
            {
                float newZ = road.position.z + roadLength * numberOfRoads;
                road.position = new Vector3(road.position.x, road.position.y, newZ);
            }
        }
    }
}

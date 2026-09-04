using UnityEngine;

[DefaultExecutionOrder(-10)] // Run before other logic to ensure correct alignment before rendering starts
public class GroundSnap : MonoBehaviour
{
    [Tooltip("Small Y offset to prevent Z-fighting with the road surface.")]
    public float yOffset = 0.01f;

    [Tooltip("Raycast distance to find the ground.")]
    public float maxRayDistance = 5f;

    private void Start()
    {
        Snap();
    }

    public void Snap()
    {
        // Raycast from slightly above the object's position
        Vector3 origin = transform.position + Vector3.up * 2f;
        RaycastHit hit;
        
        // Raycast downwards to find the road/ground collider
        if (Physics.Raycast(origin, Vector3.down, out hit, maxRayDistance))
        {
            // Check that we didn't hit a trigger or another obstacle
            if (!hit.collider.isTrigger && !hit.collider.CompareTag("Player") && !hit.collider.CompareTag("Obstacle"))
            {
                transform.position = new Vector3(transform.position.x, hit.point.y + yOffset, transform.position.z);
            }
        }
    }
}
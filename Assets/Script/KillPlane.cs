using UnityEngine;

/// <summary>
/// Prevents objects from falling below the road by destroying or resetting them.
/// Place this on a large plane collider below the road (e.g., at Y = -5).
/// </summary>
public class KillPlane : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Minimum Y position - anything below this gets reset")]
    public float minYPosition = -2f;
    
    [Tooltip("Reset player to this Y position if they fall through")]
    public float playerResetY = 0f;
    
    [Tooltip("Destroy fallen objects instead of resetting them")]
    public bool destroyFallenObjects = true;

    private void OnTriggerEnter(Collider other)
    {
        // Check if it's the player
        if (other.CompareTag("Player"))
        {
            ResetPlayer(other.gameObject);
        }
        // Check if it's a collectible or obstacle
        else if (other.CompareTag("Coin") || other.CompareTag("Obstacle"))
        {
            if (destroyFallenObjects)
                Destroy(other.gameObject);
        }
    }

    private void ResetPlayer(GameObject player)
    {
        // Reset player position to safe height
        Vector3 safePosition = player.transform.position;
        safePosition.y = playerResetY;
        player.transform.position = safePosition;
    }

    private void OnDrawGizmos()
    {
        // Draw the kill plane in the editor for visualization
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Gizmos.DrawCube(transform.position, new Vector3(1000f, 0.1f, 1000f));
    }
}

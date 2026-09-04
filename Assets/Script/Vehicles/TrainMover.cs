using UnityEngine;

public class TrainMover : MonoBehaviour
{
    public enum MovementAxis { X, Y, Z }

    [Header("Movement Settings")]
    public float speed = 8f;
    public float changeInterval = 5f;
    public bool isMoving = true;

    [Header("Triggered Start")]
    [Tooltip("If true, the train will not move until triggered.")]
    public bool triggerStart = false;

    [Tooltip("If true, the train only moves when the gameplay is active (after Tap to Play).")]
    public bool onlyMoveWhenPlaying = false;

    [Header("Direction Axis Configuration")]
    [Tooltip("Choose whether to move along X, Y, or Z axis.")]
    public MovementAxis movementAxis = MovementAxis.Z;

    [Tooltip("If true, moves along the local axis of this Transform. If false, moves along world axis.")]
    public bool moveLocal = true;

    [Tooltip("Inverts the initial movement direction.")]
    public bool invertDirection = false;

    [Header("Collision Safety")]
    [Tooltip("Decorative trains should not have colliders. If true, disables all Colliders on this object and children at start.")]
    public bool disableColliders = true;

    private float timer = 0f;
    private bool movingForward = true;

    void Start()
    {
        timer = 0f;
        if (triggerStart)
        {
            isMoving = false;
        }

        if (disableColliders)
        {
            Collider[] colliders = GetComponentsInChildren<Collider>(true);
            if (colliders.Length > 0)
            {
                foreach (Collider c in colliders)
                {
                    c.enabled = false;
                }
                Debug.Log($"[TrainMover] Disabled {colliders.Length} collider(s) on '{gameObject.name}' so it cannot cause false obstacle collisions.");
            }
        }
    }

    public void StartMoving()
    {
        isMoving = true;
    }

    void Update()
    {
        if (onlyMoveWhenPlaying)
        {
            // Only move when game is playing
            if (GameStateController.Instance == null || !GameStateController.Instance.IsPlaying())
                return;
        }

        if (!isMoving)
            return;

        // Update timer and toggle direction
        timer += Time.deltaTime;
        if (timer >= changeInterval)
        {
            movingForward = !movingForward;
            timer = 0f;
            Debug.Log($"[TrainMover] Direction changed. Moving forward: {movingForward}");
        }

        // Determine base axis vector
        Vector3 baseAxis = Vector3.forward;
        if (moveLocal)
        {
            if (movementAxis == MovementAxis.X) baseAxis = transform.right;
            else if (movementAxis == MovementAxis.Y) baseAxis = transform.up;
            else baseAxis = transform.forward;
        }
        else
        {
            if (movementAxis == MovementAxis.X) baseAxis = Vector3.right;
            else if (movementAxis == MovementAxis.Y) baseAxis = Vector3.up;
            else baseAxis = Vector3.forward;
        }

        // Apply direction inversion and forward/backward toggle
        Vector3 direction = baseAxis;
        if (invertDirection)
        {
            direction = -direction;
        }

        if (!movingForward)
        {
            direction = -direction;
        }

        // Apply movement
        transform.position += direction * speed * Time.deltaTime;
    }
}




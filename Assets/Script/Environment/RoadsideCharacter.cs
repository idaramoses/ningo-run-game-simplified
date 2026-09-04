using UnityEngine;

public class RoadsideCharacter : MonoBehaviour
{
    public enum MovementType
    {
        Stationary,
        BackAndForth,
        Random90Degrees,
        RandomAngle
    }

    [Header("Behavior Settings")]
    public MovementType movementType = MovementType.Random90Degrees;

    [Header("Movement Speed Settings")]
    public float minWalkSpeed = 0.5f;
    public float maxWalkSpeed = 1.2f;

    [Header("Patrol Limits (Relative to Spawn)")]
    public float walkRangeX = 1.5f; // Sidewalk width constraint
    public float walkRangeZ = 12f;  // Forward/backward range constraint

    [Header("Duration Settings (Seconds)")]
    public float minMoveDuration = 3f;
    public float maxMoveDuration = 7f;

    private float walkSpeed;
    private Vector3 spawnPosition;
    private float directionTimer;
    private Animator animator;

    private void OnEnable()
    {
        spawnPosition = transform.localPosition;
        walkSpeed = Random.Range(minWalkSpeed, maxWalkSpeed);
        ResetTimer();

        // Check for Animator component
        animator = GetComponentInChildren<Animator>();

        // Turn off root motion so the script fully controls position.
        // Otherwise the walk animation can push the character into the road.
        if (animator != null)
        {
            animator.applyRootMotion = false;
#if UNITY_ANDROID || UNITY_IOS
            animator.cullingMode = AnimatorCullingMode.CullCompletely;
#endif
        }

        // Apply CurvedWorld curvature dynamically so they bend nicely in the distance
        if (CurvedWorldManager.Instance != null)
        {
            CurvedWorldManager.Instance.ApplyCurvatureToGameObject(gameObject);
        }

        // Choose initial random direction
        ChooseNewDirection();
    }

    private void Update()
    {
        // Handle Stationary Stand Still Behavior
        if (movementType == MovementType.Stationary)
        {
            return;
        }

        // Move forward in local space
        transform.Translate(Vector3.forward * (walkSpeed * Time.deltaTime));

        // Constrain position relative to spawn to stay on the sidewalk
        Vector3 localPos = transform.localPosition;
        float clampedX = Mathf.Clamp(localPos.x, spawnPosition.x - walkRangeX, spawnPosition.x + walkRangeX);
        float clampedZ = Mathf.Clamp(localPos.z, spawnPosition.z - walkRangeZ, spawnPosition.z + walkRangeZ);

        // If hit boundary, force a direction change
        if (localPos.x != clampedX || localPos.z != clampedZ)
        {
            transform.localPosition = new Vector3(clampedX, localPos.y, clampedZ);
            ChooseNewDirection();
        }

        // Tick duration timer
        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0f)
        {
            ChooseNewDirection();
        }
    }

    private void ChooseNewDirection()
    {
        ResetTimer();
        walkSpeed = Random.Range(minWalkSpeed, maxWalkSpeed);

        float targetYaw = 0f;

        switch (movementType)
        {
            case MovementType.Stationary:
                targetYaw = transform.localEulerAngles.y;
                break;

            case MovementType.BackAndForth:
                // Turn 180 degrees from current local rotation
                targetYaw = (transform.localEulerAngles.y + 180f) % 360f;
                break;

            case MovementType.Random90Degrees:
                // Choose between 0, 90, 180, 270 relative to parent orientation
                float[] angles = { 0f, 90f, 180f, 270f };
                targetYaw = angles[Random.Range(0, angles.Length)];
                break;

            case MovementType.RandomAngle:
                // Choose any random yaw angle
                targetYaw = Random.Range(0f, 360f);
                break;
        }

        transform.localRotation = Quaternion.Euler(0f, targetYaw, 0f);
    }

    private void ResetTimer()
    {
        directionTimer = Random.Range(minMoveDuration, maxMoveDuration);
    }

    /// <summary>
    /// Configure the character for safe roadside walking.
    /// rangeX should be 0 to keep it on one side; rangeZ controls how far it walks along the road.
    /// </summary>
    public void Initialize(MovementType type, float rangeX, float rangeZ)
    {
        movementType = type;
        walkRangeX = rangeX;
        walkRangeZ = rangeZ;

        animator = GetComponentInChildren<Animator>();
        if (animator != null)
            animator.applyRootMotion = false;

        spawnPosition = transform.localPosition;
        walkSpeed = Random.Range(minWalkSpeed, maxWalkSpeed);
        ResetTimer();
        ChooseNewDirection();
    }
}


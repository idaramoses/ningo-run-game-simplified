using UnityEngine;
using UnityEngine.Serialization;

public class SimpleCameraFollow : MonoBehaviour
{
    public enum IdleMovementMode
    {
        PingPong,
        Once,
        Loop
    }

    [Header("Follow Settings")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 3.5f, -6f);
    public float smoothSpeed = 10f;
    public float lookAtHeight = 1f;

    [Header("Idle Camera")]
    public bool useFixedIdlePosition = false;

    [Header("Fixed Idle Positions (useFixedIdlePosition = true)")]
    [FormerlySerializedAs("idlePosition")]
    [Tooltip("Start position in world space when useFixedIdlePosition is true")]
    public Vector3 idleStartPosition = new Vector3(7.69f, 1.87f, 5.3f);
    [Tooltip("End position in world space when useFixedIdlePosition is true")]
    public Vector3 idleEndPosition = new Vector3(6.5f, 2.2f, 7.0f);

    [Header("Offset Idle Positions (useFixedIdlePosition = false)")]
    [FormerlySerializedAs("idleOffset")]
    [Tooltip("Start offset relative to target when useFixedIdlePosition is false")]
    public Vector3 idleStartOffset = new Vector3(-3.20f, 1.14f, -0.6f);
    [Tooltip("End offset relative to target when useFixedIdlePosition is false")]
    public Vector3 idleEndOffset = new Vector3(-2.60f, 1.45f, 0.8f);

    [Header("Idle Movement Settings")]
    [Tooltip("Height above target that the camera looks at during idle")]
    public float idleLookAtHeight = 0.58f;
    [Tooltip("Speed when blending between idle and gameplay follow")]
    public float idleBlendSpeed = 3f;
    [Tooltip("Speed of camera movement from start to end position")]
    public float idleMoveSpeed = 0.5f;
    [Tooltip("Movement behavior during idle: Once (single move from start to end), PingPong (back and forth), Loop (continuous loop)")]
    public IdleMovementMode idleMovementMode = IdleMovementMode.Once;

    private Vector3 currentOffset;
    private float currentLookAtHeight;
    private SimplePlayerController targetPlayerController;
    private float idleTimer = 0f;
    private bool wasIdle = false;

    private void Start()
    {
        if (target == null)
        {
            SimplePlayerController playerController = FindFirstObjectByType<SimplePlayerController>();
            if (playerController != null)
            {
                target = playerController.transform;
                targetPlayerController = playerController;
            }
            else
            {
                SimpleRunner runner = FindFirstObjectByType<SimpleRunner>();
                if (runner != null) target = runner.transform;
            }
        }
        else
        {
            targetPlayerController = target.GetComponent<SimplePlayerController>();
        }

        bool idle = IsIdle();
        wasIdle = idle;
        idleTimer = 0f;

        currentOffset = idle ? idleStartOffset : offset;
        currentLookAtHeight = idle ? idleLookAtHeight : lookAtHeight;

        if (idle)
        {
            if (useFixedIdlePosition)
            {
                transform.position = idleStartPosition;
            }
            else if (target != null)
            {
                transform.position = target.position + idleStartOffset;
            }
        }
        else if (target != null)
        {
            transform.position = target.position + offset;
        }

        if (target != null)
        {
            transform.LookAt(target.position + Vector3.up * currentLookAtHeight);
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        bool idle = IsIdle();

        // Reset timer when entering idle
        if (idle && !wasIdle)
        {
            idleTimer = 0f;
        }
        wasIdle = idle;

        if (idle)
        {
            idleTimer += Time.deltaTime * idleMoveSpeed;

            float progress = 0f;
            switch (idleMovementMode)
            {
                case IdleMovementMode.PingPong:
                    progress = Mathf.PingPong(idleTimer, 1f);
                    break;
                case IdleMovementMode.Loop:
                    progress = Mathf.Repeat(idleTimer, 1f);
                    break;
                case IdleMovementMode.Once:
                    progress = Mathf.Clamp01(idleTimer);
                    break;
            }

            float smoothT = Mathf.SmoothStep(0f, 1f, progress);

            Vector3 currentIdlePos = Vector3.Lerp(idleStartPosition, idleEndPosition, smoothT);
            Vector3 currentIdleOffset = Vector3.Lerp(idleStartOffset, idleEndOffset, smoothT);

            currentOffset = Vector3.Lerp(currentOffset, currentIdleOffset, Time.deltaTime * idleBlendSpeed);
            currentLookAtHeight = Mathf.Lerp(currentLookAtHeight, idleLookAtHeight, Time.deltaTime * idleBlendSpeed);

            if (useFixedIdlePosition)
            {
                float newX = Mathf.Lerp(transform.position.x, currentIdlePos.x, Time.deltaTime * idleBlendSpeed);
                float newY = Mathf.Lerp(transform.position.y, currentIdlePos.y, Time.deltaTime * idleBlendSpeed);
                float newZ = Mathf.Lerp(transform.position.z, currentIdlePos.z, Time.deltaTime * idleBlendSpeed);
                transform.position = new Vector3(newX, newY, newZ);
            }
            else
            {
                Vector3 targetPosition = target.position + currentOffset;
                // When the runner teleports back to the idle spot (restart/fail/complete reset)
                // the camera is far away - pan back at the gentler idle blend speed instead of
                // whipping across the level at gameplay follow speed
                float panSpeed = Vector3.Distance(transform.position, targetPosition) > 2f
                    ? idleBlendSpeed : smoothSpeed;
                float newX = Mathf.Lerp(transform.position.x, targetPosition.x, Time.deltaTime * panSpeed);
                float newY = Mathf.Lerp(transform.position.y, targetPosition.y, Time.deltaTime * panSpeed);
                float newZ = Mathf.Lerp(transform.position.z, targetPosition.z, Time.deltaTime * panSpeed);
                transform.position = new Vector3(newX, newY, newZ);
            }
        }
        else
        {
            // Gameplay follow
            currentOffset = Vector3.Lerp(currentOffset, offset, Time.deltaTime * idleBlendSpeed);
            currentLookAtHeight = Mathf.Lerp(currentLookAtHeight, lookAtHeight, Time.deltaTime * idleBlendSpeed);

            Vector3 targetPosition = target.position + currentOffset;

            // Smoothly interpolate camera position on X and Y to follow lane changes and hops
            float newX = Mathf.Lerp(transform.position.x, targetPosition.x, Time.deltaTime * smoothSpeed);
            float newY = Mathf.Lerp(transform.position.y, targetPosition.y, Time.deltaTime * smoothSpeed);

            // Z position matches target's Z position with 0 lag to prevent forward camera stuttering/shaking,
            // except while the camera is far away (e.g. blending in from the idle framing) - then it lerps
            // smoothly into place instead of snapping forward in a single frame
            float zDiff = Mathf.Abs(transform.position.z - targetPosition.z);
            float newZ = zDiff > 0.05f
                ? Mathf.Lerp(transform.position.z, targetPosition.z, Time.deltaTime * smoothSpeed)
                : targetPosition.z;

            transform.position = new Vector3(newX, newY, newZ);
        }

        // Make the camera look at the player
        Vector3 lookAtTarget = target.position + Vector3.up * currentLookAtHeight;
        transform.LookAt(lookAtTarget);
    }

    private bool IsIdle()
    {
        if (targetPlayerController != null)
            return targetPlayerController.IsInRoadsideIdlePose;

        if (GameStateController.Instance != null)
            return !GameStateController.Instance.IsPlaying();

        return false;
    }
}
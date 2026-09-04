using UnityEngine;

public class PlayerFacingController : MonoBehaviour
{
    [Header("Run Direction Reference (Drag RoadStarter here)")]
    public Transform runDirectionRef;

    [Header("Turn Settings")]
    public float rotateSpeed = 10f;
    public float completeAngle = 1.0f; // degrees

    private Quaternion homeRotation;
    private bool turningToRun = false;
    private bool facingRun = false;
    private Vector3 runForward = Vector3.forward;

    public bool IsFacingRun => facingRun;
    public Vector3 RunForward => runForward;

    void Awake()
    {
        homeRotation = transform.rotation;

        if (runDirectionRef != null)
            runForward = Flatten(runDirectionRef.forward);
    }

    public void SetHomeFacing()
    {
        turningToRun = false;
        facingRun = false;
        transform.rotation = homeRotation;
    }

    public void TurnToRun()
    {
        // If runDirectionRef is set and is NOT this object or a child, use its forward
        if (runDirectionRef != null && !runDirectionRef.IsChildOf(transform) && runDirectionRef != transform)
        {
            runForward = Flatten(runDirectionRef.forward);
        }
        else
        {
            // Default: turn to face positive Z direction (down the road)
            // This assumes the road runs along the Z axis
            runForward = Vector3.forward;
        }
        
        Debug.Log($"[PlayerFacingController] TurnToRun called. RunForward: {runForward}, Current forward: {Flatten(transform.forward)}");

        turningToRun = true;
        facingRun = false;
    }

    void Update()
    {
        if (!turningToRun) return;

        Quaternion targetRot = Quaternion.LookRotation(runForward, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);

        float angle = Quaternion.Angle(transform.rotation, targetRot);
        if (angle <= completeAngle)
        {
            transform.rotation = targetRot;
            turningToRun = false;
            facingRun = true;
        }
    }

    private static Vector3 Flatten(Vector3 v)
    {
        v.y = 0f;
        if (v.sqrMagnitude < 0.0001f) return Vector3.forward;
        return v.normalized;
    }
}

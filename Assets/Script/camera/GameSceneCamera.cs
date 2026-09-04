using UnityEngine;

/// <summary>
/// Game-scene-only camera. Stays behind the runner at all times from frame 1.
/// Assign this to the Main Camera in the GameScene.
/// Disable or remove CameraFollowRunner from the GameScene camera.
/// </summary>
public class GameSceneCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Position")]
    public float height = 4.5f;
    public float distance = 7.5f;

    [Header("Look")]
    public float lookAheadDistance = 2f;
    public float lookHeightOffset = 1.8f;

    [Header("Smoothing")]
    public float positionSmoothTime = 0.1f;
    public float rotationSmoothTime = 0.08f;

    private Vector3 posVelocity = Vector3.zero;
    private Vector3 rotVelocity = Vector3.zero;
    private bool snapped = false;
    private Transform lastTarget = null;

    void LateUpdate()
    {
        if (target == null)
            TryFindTarget();

        if (target == null) return;

        // Re-snap if target changed (e.g. runner switched)
        if (target != lastTarget)
        {
            lastTarget = target;
            snapped = false;
        }

        if (!snapped)
        {
            Snap();
        }
        else
        {
            Follow();
        }
    }

    private void TryFindTarget()
    {
        // Find the active player by tag first
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null && playerObj.activeInHierarchy)
        {
            target = playerObj.transform;
            return;
        }

        // Fallback to any active runner
        foreach (var runner in FindObjectsOfType<SimpleRunner>())
        {
            if (runner.gameObject.activeInHierarchy)
            {
                target = runner.transform;
                break;
            }
        }
    }

    private void Snap()
    {
        transform.position = DesiredPosition();
        transform.rotation = DesiredRotation();
        posVelocity = Vector3.zero;
        rotVelocity = Vector3.zero;
        snapped = true;
    }

    private void Follow()
    {
        transform.position = Vector3.SmoothDamp(transform.position, DesiredPosition(), ref posVelocity, positionSmoothTime);

        Vector3 currentEuler = transform.rotation.eulerAngles;
        Vector3 targetEuler = DesiredRotation().eulerAngles;
        transform.rotation = Quaternion.Euler(
            Mathf.SmoothDampAngle(currentEuler.x, targetEuler.x, ref rotVelocity.x, rotationSmoothTime),
            Mathf.SmoothDampAngle(currentEuler.y, targetEuler.y, ref rotVelocity.y, rotationSmoothTime),
            Mathf.SmoothDampAngle(currentEuler.z, targetEuler.z, ref rotVelocity.z, rotationSmoothTime)
        );
    }

    private Vector3 DesiredPosition()
    {
        Vector3 fwd = Forward();
        return target.position - fwd * distance + Vector3.up * height;
    }

    private Quaternion DesiredRotation()
    {
        Vector3 fwd = Forward();
        Vector3 lookAt = target.position + fwd * lookAheadDistance + Vector3.up * lookHeightOffset;
        Vector3 dir = lookAt - DesiredPosition();
        if (dir.sqrMagnitude < 0.001f) return transform.rotation;
        return Quaternion.LookRotation(dir, Vector3.up);
    }

    private Vector3 Forward()
    {
        Vector3 fwd = target.forward;
        fwd.y = 0f;
        return fwd.sqrMagnitude < 0.001f ? Vector3.forward : fwd.normalized;
    }
}

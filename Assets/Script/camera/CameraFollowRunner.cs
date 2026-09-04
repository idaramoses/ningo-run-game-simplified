using UnityEngine;

public class CameraFollowRunner : MonoBehaviour
{
    public Transform target; // drag AmakaModel here
    public bool followOnlyWhenPlaying = true;
    public bool forceFollow = false;

    [Header("Follow (local behind target)")]
    public float height = 4.5f; // Higher camera for better view (Subway Surfers style)
    public float distance = 7.5f; // Further back to see more of the road ahead

    [Header("Smoothing")]
    public float positionSmoothTime = 0.1f; // Smooth damping time for position
    public float rotationSmoothTime = 0.1f; // Smooth damping time for rotation
    
    private Vector3 positionVelocity = Vector3.zero;
    private Vector3 rotationVelocity = Vector3.zero;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private void Awake()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    public void ResetCamera()
    {
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        positionVelocity = Vector3.zero;
        rotationVelocity = Vector3.zero;
    }

    void LateUpdate()
    {
        if (!target) return;

        if (!forceFollow && followOnlyWhenPlaying &&
            (GameStateController.Instance == null || !GameStateController.Instance.IsPlaying()))
        {
            return;
        }

        // behind the runner
        Vector3 forward = target.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
        forward.Normalize();

        Vector3 desiredPos = target.position - forward * distance + Vector3.up * height;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPos, ref positionVelocity, positionSmoothTime);

        // Look slightly ahead of the runner (Subway Surfers style - see more of the road)
        Vector3 lookAt = target.position + forward * 2f + Vector3.up * 1.8f;
        Vector3 targetDirection = lookAt - transform.position;
        
        Vector3 currentEuler = transform.rotation.eulerAngles;
        Vector3 targetEuler = Quaternion.LookRotation(targetDirection, Vector3.up).eulerAngles;
        Vector3 smoothedEuler = new Vector3(
            Mathf.SmoothDampAngle(currentEuler.x, targetEuler.x, ref rotationVelocity.x, rotationSmoothTime),
            Mathf.SmoothDampAngle(currentEuler.y, targetEuler.y, ref rotationVelocity.y, rotationSmoothTime),
            Mathf.SmoothDampAngle(currentEuler.z, targetEuler.z, ref rotationVelocity.z, rotationSmoothTime)
        );
        transform.rotation = Quaternion.Euler(smoothedEuler);
    }
}

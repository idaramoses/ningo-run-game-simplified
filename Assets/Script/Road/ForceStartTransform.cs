using UnityEngine;

public class ForceStartTransform : MonoBehaviour
{
    [Header("Paste the exact values you want")]
    public Vector3 startPosition = new Vector3(-2.736f, -4.971f, -1.448f);
    public Vector3 startRotationEuler = new Vector3(46.042f, -150.4f, 22.176f);

    public bool lockPosition = true;
    public bool lockRotation = true;

    void Start()
    {
        Apply();
    }

    void LateUpdate()
    {
        // keeps it fixed even if other scripts/animations try to change it
        Apply();
    }

    void Apply()
    {
        if (lockPosition) transform.position = startPosition;
        if (lockRotation) transform.rotation = Quaternion.Euler(startRotationEuler);
    }
}

using UnityEngine;

public class PlayerPositionLock : MonoBehaviour
{
    [Header("Keep these fixed")]
    public bool lockX = true;
    public bool lockZ = true;

    private float startX;
    private float startZ;

    void Start()
    {
        startX = transform.position.x;
        startZ = transform.position.z;
    }

    void LateUpdate()
    {
        Vector3 p = transform.position;

        if (lockX) p.x = startX;
        if (lockZ) p.z = startZ;

        transform.position = p;
    }
}

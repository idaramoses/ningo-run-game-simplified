using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SnapToGround : MonoBehaviour
{
    public LayerMask groundMask;
    public float rayStartHeight = 2f;
    public float maxDistance = 20f;
    public float extraOffset = 0.02f;

    CharacterController cc;

    void Awake() => cc = GetComponent<CharacterController>();

    void Start()
    {
        Vector3 origin = transform.position + Vector3.up * rayStartHeight;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, maxDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            Debug.Log($"SnapToGround HIT: {hit.collider.name}  y={hit.point.y}");

            float bottomFromCenter = (cc.height * 0.5f) - cc.center.y;
            Vector3 p = transform.position;
            p.y = hit.point.y + bottomFromCenter + extraOffset;

            cc.enabled = false;
            transform.position = p;
            cc.enabled = true;
        }
        else
        {
            Debug.LogWarning("SnapToGround: NO HIT. Check road colliders + Ground layer.");
        }
    }
}

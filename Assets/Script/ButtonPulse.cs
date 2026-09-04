using UnityEngine;

public class ButtonPulse : MonoBehaviour
{
    public float speed = 1f;
    public float scaleAmount = 0.02f;
    private Vector3 baseScale;

    void Start()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        transform.localScale = baseScale + Vector3.one * Mathf.Sin(Time.time * speed) * scaleAmount;
    }
}

using UnityEngine;

public class SimpleRotate : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = -200f;

    private void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}

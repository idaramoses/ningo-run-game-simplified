using UnityEngine;

/// <summary>
/// Rotates UI elements endlessly. Uses unscaledDeltaTime so rotation is unaffected by Time.timeScale = 0.
/// </summary>
public class UIRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = -50f; // negative value for clockwise rotation

    private void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.unscaledDeltaTime);
    }
}

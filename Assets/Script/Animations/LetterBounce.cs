using UnityEngine;

public class LetterBounce : MonoBehaviour
{
    [Header("Bounce Settings")]
    [Tooltip("Height of the bounce in units")]
    [Range(0.01f, 0.5f)]
    public float bounceHeight = 0.1f;
    
    [Tooltip("Speed of the bounce animation")]
    [Range(0.5f, 5f)]
    public float bounceSpeed = 2f;
    
    [Tooltip("Random offset to desync multiple letters")]
    public bool randomizeStartOffset = true;
    
    private Vector3 startPosition;
    private float timeOffset;
    
    private void Start()
    {
        startPosition = transform.position;
        
        if (randomizeStartOffset)
        {
            timeOffset = Random.Range(0f, Mathf.PI * 2f);
        }
    }
    
    private void Update()
    {
        // Letters stay at ground height even while the player is gliding —
        // only coins/gems float up to sky altitude.
        float yOffset = Mathf.Sin((Time.time * bounceSpeed) + timeOffset) * bounceHeight;
        transform.position = new Vector3(transform.position.x, startPosition.y + yOffset, transform.position.z);
    }
    
    private void OnDisable()
    {
        if (startPosition != Vector3.zero)
        {
            transform.position = startPosition;
        }
    }
}

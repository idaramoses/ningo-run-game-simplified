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
        startPosition = transform.localPosition;
        
        if (randomizeStartOffset)
        {
            timeOffset = Random.Range(0f, Mathf.PI * 2f);
        }
    }
    
    private void Update()
    {
        float yOffset = Mathf.Sin((Time.time * bounceSpeed) + timeOffset) * bounceHeight;
        transform.localPosition = startPosition + new Vector3(0f, yOffset, 0f);
    }
    
    private void OnDisable()
    {
        if (startPosition != Vector3.zero)
        {
            transform.localPosition = startPosition;
        }
    }
}

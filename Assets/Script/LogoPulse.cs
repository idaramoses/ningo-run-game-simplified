using UnityEngine;
using UnityEngine.UI;

public class LogoPulse : MonoBehaviour
{
    private Image logoImage;
    private Vector3 startScale;
    private float pulseSpeed = 1.2f; // controls how fast it pulses
    private float pulseAmount = 0.05f; // controls how big the scale change is

    void Start()
    {
        logoImage = GetComponent<Image>();
        startScale = transform.localScale;
    }

    void Update()
    {
        // Create a smooth "breathing" effect
        float scale = 1 + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = startScale * scale;

        // Optional: light fade pulse on alpha for soft glow
        float alpha = Mathf.Lerp(0.9f, 1f, (Mathf.Sin(Time.time * pulseSpeed) + 1) / 2f);
        Color c = logoImage.color;
        c.a = alpha;
        logoImage.color = c;
    }
}

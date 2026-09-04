using UnityEngine;
using TMPro;

public class LoadingTextPulse : MonoBehaviour
{
    private TMP_Text text;

    void Start()
    {
        text = GetComponent<TMP_Text>();
    }

    void Update()
    {
        // PingPong oscillates between 0 and 1 over time
        float alpha = Mathf.PingPong(Time.time, 1f);

        // Apply that value to the text color’s alpha channel
        text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
    }
}

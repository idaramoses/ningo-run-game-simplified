using UnityEngine;
using TMPro;

public class DistanceTracker : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] TMP_Text distanceText;

    float startZ;
    float bestDistance;

    void Start()
    {
        if (!player) player = GameObject.FindWithTag("Player")?.transform;
        startZ = player ? player.position.z : 0f;
        UpdateUI(0);
    }

    private int lastDisplayedDistance = -1;

    void Update()
    {
        if (GameStateController.Instance == null || !GameStateController.Instance.IsPlaying())
            return;

        if (!player) return;

        float dist = Mathf.Max(0f, player.position.z - startZ);
        if (dist > bestDistance)
        {
            bestDistance = dist;
            int currentDistInt = Mathf.FloorToInt(bestDistance);
            if (currentDistInt != lastDisplayedDistance)
            {
                lastDisplayedDistance = currentDistInt;
                UpdateUI(bestDistance);
            }
        }
    }

    void UpdateUI(float meters)
    {
        if (distanceText) distanceText.text = $"{meters:0}m";
    }
}

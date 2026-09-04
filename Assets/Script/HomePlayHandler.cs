using UnityEngine;
using System.Collections;

public class HomePlayHandler : MonoBehaviour
{
    [Header("UI")]
    public GameObject homePanel;
    public GameObject gamePanel;

    [Header("Player")]
    public Transform player;
    public Animator playerAnimator;
    public float turnSpeed = 2.5f;

    private bool started = false;

    public void OnPlayButtonClick()
    {
        if (started) return;
        started = true;

        // Hide Home UI, show Game UI
        homePanel.SetActive(false);
        if (gamePanel != null)
            gamePanel.SetActive(true);

        // Start intro
        StartCoroutine(StartRunSequence());
    }

    IEnumerator StartRunSequence()
    {
        // Turn player to face road
        Quaternion startRot = player.rotation;
        Quaternion targetRot = Quaternion.Euler(0, 0, 0);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * turnSpeed;
            player.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        // Start running animation
        playerAnimator.Play("Run");
    }
}

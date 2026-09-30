using UnityEngine;

public class WordFailHandler : MonoBehaviour
{
    public WordManager wordManager;
    public PlayerRunnerController runner; // optional

    private void OnEnable()
    {
        // Auto-find if not assigned
        if (wordManager == null) wordManager = FindObjectOfType<WordManager>();
        if (runner == null) runner = FindObjectOfType<PlayerRunnerController>();

        if (wordManager != null)
            wordManager.OnFailedRound += HandleFail;
        else
            Debug.LogError("WordFailHandler: WordManager not found in scene.");
    }

    private void OnDisable()
    {
        if (wordManager != null)
            wordManager.OnFailedRound -= HandleFail;
    }

    private void HandleFail()
    {
        Debug.Log("WordFailHandler: FAIL -> stop game");

        if (SoundEffectsManager.Instance != null)
            SoundEffectsManager.Instance.PlayObstacleHit();

        // Same behavior as hitting an obstacle: knock the runner down first
        var simpleRunner = FindObjectOfType<SimplePlayerController>();
        if (simpleRunner != null)
            simpleRunner.TriggerFall();

        // Single-scene flow uses UImanager.Fail() (FailPanel + freeze + state)
        if (UImanager.uimanager != null)
        {
            UImanager.uimanager.Fail();
            return;
        }

        // Prefer unified fail flow (fall anim + FailPanel + stop gameplay)
        if (GameSceneController.Instance != null)
        {
            GameSceneController.Instance.Fail();
            return;
        }
        else if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.Fail();
            return;
        }

        // Fallback (if you didn't add GameFlowController.Instance)
        if (GameStateController.Instance != null)
            GameStateController.Instance.SetPlaying(false);

        if (runner != null)
            runner.TriggerFall();
    }
}

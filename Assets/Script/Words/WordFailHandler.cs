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

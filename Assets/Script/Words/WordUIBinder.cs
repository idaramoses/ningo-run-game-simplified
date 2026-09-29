using UnityEngine;

public class WordUIBinder : MonoBehaviour
{
    public WordManager manager;

    void OnEnable()
    {
        if (manager == null) manager = FindObjectOfType<WordManager>();
        if (manager == null) return;

        manager.OnStartedRound += HandleRoundStarted;
        manager.OnCompletedRound += HandleRoundCompleted;
        manager.OnFailedRound += HandleRoundFailed;
    }

    void OnDisable()
    {
        if (manager == null) return;

        manager.OnStartedRound -= HandleRoundStarted;
        manager.OnCompletedRound -= HandleRoundCompleted;
        manager.OnFailedRound -= HandleRoundFailed;
    }

    void HandleRoundStarted()
    {
        // WordManager already updates promptText + languageText internally.
        // Keep this for future UI updates if needed.
    }

    void HandleRoundCompleted()
    {
        // Optional: show "Correct!" UI, play sound, etc.
    }

    void HandleRoundFailed()
    {
        // Optional: show fail UI (later with GameFlowController)
    }
}

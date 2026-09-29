using System;
using UnityEngine;

public class PlayerRunnerController : MonoBehaviour
{
    public static bool InputBlocked { get; set; }
    public static event Action OnSwipedHorizontal;
    public static event Action OnSwipedUp;

    public static void TriggerSwipeHorizontal() => OnSwipedHorizontal?.Invoke();
    public static void TriggerSwipeUp() => OnSwipedUp?.Invoke();

    public float laneDistance = 2.5f;
    public string runState = "run";

    private Animator _cachedAnim;
    public Animator anim
    {
        get
        {
            if (_cachedAnim == null)
                _cachedAnim = GetComponentInChildren<Animator>();
            return _cachedAnim;
        }
    }

    public void StartGameRun()
    {
        var sp = GetComponent<SimplePlayerController>();
        if (sp != null)
        {
            sp.StartRunning();
        }
        else if (SimplePlayerController.Instance != null)
        {
            SimplePlayerController.Instance.StartRunning();
        }
        else if (anim != null && !string.IsNullOrEmpty(runState))
        {
            anim.Play(runState, 0, 0f);
        }
    }

    public void TriggerFall()
    {
        var sp = GetComponent<SimplePlayerController>();
        if (sp != null)
        {
            sp.TriggerFall();
        }
        else if (SimplePlayerController.Instance != null)
        {
            SimplePlayerController.Instance.TriggerFall();
        }
        else if (anim != null)
        {
            anim.Play("fall", 0, 0f);
        }
    }

    public void ResetAfterContinue()
    {
        var sp = GetComponent<SimplePlayerController>();
        if (sp != null)
        {
            sp.RestoreMovementAnimation();
        }
        else if (SimplePlayerController.Instance != null)
        {
            SimplePlayerController.Instance.RestoreMovementAnimation();
        }
        else if (anim != null && !string.IsNullOrEmpty(runState))
        {
            anim.Play(runState, 0, 0f);
        }
    }
}

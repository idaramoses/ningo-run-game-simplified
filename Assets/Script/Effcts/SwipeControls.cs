using UnityEngine;

public class SwipeControls : MonoBehaviour {

    /*
    *	NOTE: Input/swipe handling for the player is now done internally by
    *	SimplePlayerController (see Assets/Script/SimplePlayerController.cs),
    *	which reads taps/swipes itself in its own Update/HandleInputs methods.
    *	This component is kept only so existing scene references don't break,
    *	but it no longer performs any swipe detection to avoid double input.
    */

    public enum SwipeDirection
    {
        Null = 0,
        Duck = 1,
        Jump = 2,
        Right = 3,
        Left = 4
    }

    public SwipeDirection sSwipeDirection = SwipeDirection.Null;

    public SwipeDirection getSwipeDirection()
    {
        return SwipeDirection.Null;
    }
}

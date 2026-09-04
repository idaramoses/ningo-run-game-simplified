using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginButtons : MonoBehaviour
{
    // Call this when the user clicks "Login" or "Continue as Guest"
    public void GoToHome()
    {
        SceneManager.LoadScene("Home"); // make sure the scene name matches exactly
    }
}


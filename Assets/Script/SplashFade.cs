using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashFade : MonoBehaviour
{
    public float displayTime = 2.5f; // how long to wait before loading next scene
    public string nextSceneName = "Login"; // change if needed

    private void Start()
    {
        // Wait a few seconds, then load the next scene
        StartCoroutine(WaitAndLoad());
    }

    private System.Collections.IEnumerator WaitAndLoad()
    {
        yield return new WaitForSeconds(displayTime);
        SceneManager.LoadScene(nextSceneName);
    }
}

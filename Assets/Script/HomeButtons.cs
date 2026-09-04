using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeButtons : MonoBehaviour
{
    public void OnPlayButton() => SceneManager.LoadScene("CitySelect");
    
    public void OnRunnersButton()
    {
        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.ShowRunnerSelection();
        }
        else
        {
            Debug.LogWarning("[HomeButtons] GameFlowController.Instance is null!");
        }
    }
    
    public void OnLeaderboardButton() => SceneManager.LoadScene("Leaderboard");

    public void OnSettingsButton() => Debug.Log("Settings opened!");
    public void OnHelpButton() => Debug.Log("Help opened!");
    public void OnInfoButton() => Debug.Log("Info opened!");
}

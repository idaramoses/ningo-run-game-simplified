using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayButtonHandler : MonoBehaviour
{
    [Header("Level System (Disabled - kept for inspector compatibility)")]
    public GameObject levelSelectionPanel;

    private void Start()
    {
        // Auto-wire button listener as backup (in case Inspector onClick is broken)
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveListener(OnPlayClicked);
            btn.onClick.AddListener(OnPlayClicked);
            Debug.Log("[PlayButtonHandler] Auto-wired button onClick listener");
        }
    }

    public void OnPlayClicked()
    {
        Debug.Log($"[PlayButtonHandler] PLAY CLICKED - Starting Endless Run directly. GameFlowController: {(GameFlowController.Instance != null ? "OK" : "NULL")}, Time.timeScale: {Time.timeScale}");
        
        // Safety: ensure time is running
        Time.timeScale = 1f;

        // Directly use GameFlowController (Home scene controller) to load the GameScene
        if (GameFlowController.Instance != null)
        {
            GameFlowController.Instance.Play();
            return;
        }
        
        Debug.LogWarning("[PlayButtonHandler] GameFlowController.Instance is NULL, trying SceneLoader fallback...");
        
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadGameScene();
            return;
        }
        
        Debug.LogWarning("[PlayButtonHandler] SceneLoader.Instance is NULL, loading scene by name fallback...");
        SceneManager.LoadSceneAsync("GameScene");
    }
}

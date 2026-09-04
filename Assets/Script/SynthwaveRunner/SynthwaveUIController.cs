using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

namespace SynthwaveRunner
{
    public class SynthwaveUIController : MonoBehaviour
    {
        [Header("UI Panels")]
        public GameObject homePanel;
        public GameObject hudPanel;
        public GameObject gameOverPanel;

        [Header("HUD Text Elements")]
        public TextMeshProUGUI hudScoreText;
        public TextMeshProUGUI hudDataBitsText;

        [Header("Game Over Text Elements")]
        public TextMeshProUGUI finalScoreText;
        public TextMeshProUGUI finalDataBitsText;
        public TextMeshProUGUI highScoreText;

        [Header("Home Text Elements")]
        public TextMeshProUGUI homeHighScoreText;

        private void Start()
        {
            if (SynthwaveGameManager.Instance != null)
            {
                // Subscribe to events
                SynthwaveGameManager.Instance.onStateChanged += OnGameStateChanged;
                SynthwaveGameManager.Instance.onScoreChanged += OnScoreChanged;
                SynthwaveGameManager.Instance.onDataBitsChanged += OnDataBitsChanged;

                // Set initial values
                UpdateHighScoreDisplays();
                OnGameStateChanged(SynthwaveGameManager.Instance.currentState);
            }
            else
            {
                Debug.LogWarning("[SynthwaveUIController] SynthwaveGameManager instance not found!");
            }
        }

        private void OnDestroy()
        {
            if (SynthwaveGameManager.Instance != null)
            {
                SynthwaveGameManager.Instance.onStateChanged -= OnGameStateChanged;
                SynthwaveGameManager.Instance.onScoreChanged -= OnScoreChanged;
                SynthwaveGameManager.Instance.onDataBitsChanged -= OnDataBitsChanged;
            }
        }

        #region Event Callbacks

        private void OnGameStateChanged(SynthwaveGameManager.GameState state)
        {
            // Toggle panel active states depending on game state
            homePanel.SetActive(state == SynthwaveGameManager.GameState.Home);
            hudPanel.SetActive(state == SynthwaveGameManager.GameState.Playing);
            gameOverPanel.SetActive(state == SynthwaveGameManager.GameState.GameOver);

            if (state == SynthwaveGameManager.GameState.Home)
            {
                UpdateHighScoreDisplays();
            }
            else if (state == SynthwaveGameManager.GameState.GameOver)
            {
                UpdateGameOverDisplays();
            }
            else if (state == SynthwaveGameManager.GameState.Playing)
            {
                // Reset HUD display values immediately
                OnScoreChanged(0);
                OnDataBitsChanged(0);
            }
        }

        private void OnScoreChanged(int currentScore)
        {
            if (hudScoreText != null)
            {
                hudScoreText.text = currentScore.ToString();
            }
        }

        private void OnDataBitsChanged(int currentDataBits)
        {
            if (hudDataBitsText != null)
            {
                hudDataBitsText.text = currentDataBits.ToString();
            }
        }

        #endregion

        #region Display Helpers

        private void UpdateHighScoreDisplays()
        {
            int high = SynthwaveGameManager.Instance != null ? SynthwaveGameManager.Instance.GetHighScore() : 0;
            
            if (homeHighScoreText != null)
            {
                homeHighScoreText.text = "HIGH SCORE: " + high.ToString();
            }
        }

        private void UpdateGameOverDisplays()
        {
            if (SynthwaveGameManager.Instance == null) return;

            int score = SynthwaveGameManager.Instance.score;
            int bits = SynthwaveGameManager.Instance.dataBits;
            int high = SynthwaveGameManager.Instance.GetHighScore();

            if (finalScoreText != null)
            {
                finalScoreText.text = "DISTANCE: " + score.ToString() + "m";
            }
            if (finalDataBitsText != null)
            {
                finalDataBitsText.text = "BITS HARVESTED: " + bits.ToString();
            }
            if (highScoreText != null)
            {
                highScoreText.text = "BEST RECORD: " + high.ToString() + "m";
            }
        }

        #endregion

        #region Button Action Hooks (Assigned in Canvas Buttons)

        public void PlayGame()
        {
            if (SynthwaveGameManager.Instance != null)
            {
                SynthwaveGameManager.Instance.StartGame();
            }
        }

        public void GoToHome()
        {
            if (SynthwaveGameManager.Instance != null)
            {
                SynthwaveGameManager.Instance.TriggerGameOver(); // Fallback
            }
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ExitToMainGame()
        {
            // Load the main/home scene of the main game if it exists
            SceneManager.LoadScene("Home");
        }

        #endregion
    }
}

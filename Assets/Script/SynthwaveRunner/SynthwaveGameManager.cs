using UnityEngine;

namespace SynthwaveRunner
{
    public class SynthwaveGameManager : MonoBehaviour
    {
        public static SynthwaveGameManager Instance { get; private set; }

        public enum GameState { Home, Playing, GameOver }

        [Header("Game State")]
        public GameState currentState = GameState.Home;

        [Header("Speed Settings")]
        public float baseSpeed = 18f;
        public float currentSpeed = 18f;
        public float maxSpeed = 45f;
        public float speedAcceleration = 0.15f;

        [Header("Scoring")]
        public int score = 0;
        public int dataBits = 0;
        private float scoreAccumulator = 0f;
        private int highScore = 0;

        [Header("Events")]
        public System.Action<GameState> onStateChanged;
        public System.Action<int> onScoreChanged;
        public System.Action<int> onDataBitsChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // Load High Score
            highScore = PlayerPrefs.GetInt("SynthwaveHighScore", 0);
        }

        private void Start()
        {
            SetState(GameState.Home);
        }

        private void Update()
        {
            if (currentState != GameState.Playing) return;

            // Increase speed gradually over time
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, speedAcceleration * Time.deltaTime);

            // Accumulate score based on distance (speed)
            scoreAccumulator += currentSpeed * Time.deltaTime;
            int newScore = Mathf.FloorToInt(scoreAccumulator);
            if (newScore != score)
            {
                score = newScore;
                onScoreChanged?.Invoke(score);
            }
        }

        public void StartGame()
        {
            score = 0;
            dataBits = 0;
            scoreAccumulator = 0f;
            currentSpeed = baseSpeed;

            // Notify controllers
            SetState(GameState.Playing);
        }

        public void CollectDataBit()
        {
            if (currentState != GameState.Playing) return;
            
            dataBits++;
            onDataBitsChanged?.Invoke(dataBits);

            // Award immediate score bonus
            scoreAccumulator += 5f;
            int newScore = Mathf.FloorToInt(scoreAccumulator);
            if (newScore != score)
            {
                score = newScore;
                onScoreChanged?.Invoke(score);
            }
        }

        public void TriggerGameOver()
        {
            if (currentState != GameState.Playing) return;

            // Check and save high score
            if (score > highScore)
            {
                highScore = score;
                PlayerPrefs.SetInt("SynthwaveHighScore", highScore);
                PlayerPrefs.Save();
            }

            SetState(GameState.GameOver);
        }

        private void SetState(GameState newState)
        {
            currentState = newState;
            onStateChanged?.Invoke(currentState);

            // Set global timeScale to pause/unpause if needed, or handle it via movement
            Time.timeScale = (newState == GameState.Playing || newState == GameState.Home) ? 1f : 0f;
        }

        public int GetHighScore() => highScore;
    }
}

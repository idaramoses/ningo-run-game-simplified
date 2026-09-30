using System.Collections.Generic;
using UnityEngine;

public class LetterSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform lettersParent;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform player;

    [Header("Lane (match PlayerRunnerController.laneDistance)")]
    public float laneDistance = 2.5f;
    public float laneJitter = 0.8f; // Increased for more scattered placement
    public float spawnHeightOffset = 1.5f;

    [Header("Scatter / Long Run Feel")]
    public int maxLettersPerSegment = 5; // More letters per segment
    public float segmentLength = 40f; // 1 road = 40 units (spawn more frequently)
    public float segmentGap = 0f;
    public float wordGap = 0f; // No gap between words - endless letters
    public float initialSpawnDelay = 40f; // Delay before first letters (1 road length)

    [Header("Letters Per Road Segment")]
    [Tooltip("Max total letters (correct + wrong) per road segment")]
    public int maxLettersPerRoadSegment = 5; // Multiple letters per road

    [Header("Letter Prefab")]
    [Tooltip("Fallback prefab (LetterPickup + LetterVisual + TMP text) used when road tiles have no RoadSegment.")]
    public GameObject letterPrefab;

    [Header("Wrong Letters")]
    [Range(0f, 1f)]
    public float wrongLetterChance = 0.7f; // Default, overridden by level
    public int minWrongLettersPerSegment = 2;
    public int maxWrongLettersPerSegment = 4; // More wrong letters
    private const string wrongLettersPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private int currentLevel = 1;

    [Header("Cleanup")]
    public float destroyBehindDistance = 90f;

    [Header("Missed Letter Respawn")]
    public bool respawnMissedLetters = true;
    public float respawnDistanceAhead = 80f; // Respawn further ahead
    public int maxRespawnAttempts = 10; // Keep respawning until collected

    private readonly List<GameObject> spawned = new List<GameObject>();
    private readonly Dictionary<string, GameObject> correctLetterObjects = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, int> missedLetterCount = new Dictionary<string, int>(); // Track respawn attempts
    private float nextSegmentOffsetZ = 0f;
    private int lastLane = -1;
    
    // Track which lanes have letters on which road segments (for obstacle avoidance)
    private readonly Dictionary<float, List<int>> letterLanesBySegment = new Dictionary<float, List<int>>();
    
    // Current word being collected (grapheme clusters, e.g. "á", "f", "ọ́")
    private List<string> currentWordLetters = new List<string>();
    
    // Endless spawning
    private float lastSpawnedZ = 0f;
    private float spawnAheadDistance =
#if UNITY_ANDROID || UNITY_IOS
        100f;
#else
        200f;
#endif
    private bool endlessMode = true;

    private void Awake()
    {
        EnsureRefs();
        
        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
#if UNITY_EDITOR
                Debug.Log("[LetterSpawner] Auto-found player");
#endif
            }
        }
    }

    private void OnEnable()
    {
        EnsureRefs();
    }

    private void Update()
    {
        CleanupBehind();

        // Don't spawn letters during tutorial
        if (GameTutorialController.IsActive || GameTutorialController.ShouldRunTutorial())
            return;

        // Endless letter spawning - keep spawning as long as we have letters to spawn
        // Don't stop even if word is completed, just keep spawning from the current word pool
        if (endlessMode && currentWordLetters.Count > 0 && player != null)
        {
            // Check if we need to spawn more letters ahead of the player
            if (player.position.z + spawnAheadDistance > lastSpawnedZ)
            {
                SpawnMoreLetters();
            }
        }
    }

    public void SetSpawnPoint(Transform sp)
    {
        if (sp == null) return;
        spawnPoint = sp;
    }

    public void ResetSpawnCursor()
    {
        nextSegmentOffsetZ = 0f;
        lastLane = -1;
    }

    public void SpawnWord(string word)
    {
        EnsureRefs();

        if (spawnPoint == null)
        {
            Debug.LogError("[LetterSpawner] SpawnPoint is still null after EnsureRefs(). Check LettersParent/SpawnPoint in scene.");
            return;
        }

        if (string.IsNullOrWhiteSpace(word)) return;

        // Split into grapheme clusters so tone marks stay attached to their base letters.
        List<string> newWordLetters = TextUtils.SplitGraphemesNoWhitespace(word);
        if (newWordLetters.Count == 0) return;

        // Update current word letters (don't clear, just replace)
        currentWordLetters = newWordLetters;
        
        // Get current level from LevelManager
        if (LevelManager.Instance != null)
        {
            currentLevel = LevelManager.Instance.GetSelectedLevel();
#if UNITY_EDITOR
            Debug.Log($"[LetterSpawner] Current level: {currentLevel}");
#endif
        }
        
        // Only reset spawn position if this is the first word
        if (lastSpawnedZ == 0f)
        {
            lastSpawnedZ = spawnPoint.position.z;
            nextSegmentOffsetZ = initialSpawnDelay;
        }
        // Otherwise keep spawning from where we left off for seamless transition
        
#if UNITY_EDITOR
        Debug.Log($"[LetterSpawner] Starting new word: {word} - {newWordLetters.Count} letters - Endless mode enabled, continuing from Z={lastSpawnedZ:F1}");
#endif

        // Don't spawn letters during tutorial
        if (GameTutorialController.IsActive || GameTutorialController.ShouldRunTutorial())
        {
#if UNITY_EDITOR
            Debug.Log("[LetterSpawner] Skipping initial spawn - tutorial active");
#endif
            return;
        }

        // Spawn more letters immediately to ensure continuous flow
        SpawnMoreLetters();
    }

    private void SpawnMoreLetters()
    {
        if (currentWordLetters.Count == 0) return;

        float baseZ = lastSpawnedZ;
        
        // Get level-based wrong letter chance
        float levelBasedWrongChance = GetWrongLetterChanceForLevel(currentLevel);
        
        // Spawn letters across multiple segments
#if UNITY_ANDROID || UNITY_IOS
        int segmentsToSpawn = 2;
#else
        int segmentsToSpawn = 5;
#endif
        
        for (int seg = 0; seg < segmentsToSpawn; seg++)
        {
            float segStart = baseZ + (seg * segmentLength);
            float segEnd = segStart + segmentLength;

            int lettersThisSegment = Random.Range(2, maxLettersPerRoadSegment + 1);
            
            for (int i = 0; i < lettersThisSegment; i++)
            {
                float letterZ = Mathf.Lerp(segStart, segEnd, Random.Range(0.2f, 0.8f));
                int lane = PickLaneAvoiding(lastLane);
                
                // Decide if correct or wrong letter based on level
                bool spawnCorrect = Random.value > levelBasedWrongChance;
                
                if (spawnCorrect && currentWordLetters.Count > 0)
                {
                    // Spawn a random correct letter from current word
                    string correctLetter = currentWordLetters[Random.Range(0, currentWordLetters.Count)];
                    SpawnLetter(correctLetter, lane, letterZ, true);
                }
                else
                {
                    // Spawn wrong letter
                    string wrongLetter = GetRandomWrongLetter(currentWordLetters);
                    SpawnLetter(wrongLetter, lane, letterZ, false);
                }
                
                lastLane = lane;
            }
        }
        
        // Update last spawned position
        lastSpawnedZ = baseZ + (segmentsToSpawn * segmentLength);
#if UNITY_EDITOR
        Debug.Log($"[LetterSpawner] Spawned more letters up to Z={lastSpawnedZ:F1} with wrong chance={levelBasedWrongChance:F2}");
#endif
    }

    private void EnsureRefs()
    {
        // lettersParent
        if (lettersParent == null)
        {
            GameObject lp = GameObject.Find("LettersParent");
            if (lp != null) lettersParent = lp.transform;
        }

        if (lettersParent == null)
        {
            var lp = new GameObject("LettersParent");
            lettersParent = lp.transform;
        }

        // spawnPoint
        if (spawnPoint == null)
        {
            Transform sp = lettersParent.Find("SpawnPoint");
            if (sp == null)
            {
                var go = new GameObject("SpawnPoint");
                go.transform.SetParent(lettersParent, false);

                // default: same position as lettersParent
                go.transform.position = lettersParent.position;

                sp = go.transform;
            }
            spawnPoint = sp;
        }
    }

    private int PickLaneAvoiding(int avoidLane)
    {
        int lane = Random.Range(0, 3);
        if (avoidLane >= 0 && avoidLane <= 2)
        {
            for (int tries = 0; tries < 6 && lane == avoidLane; tries++)
                lane = Random.Range(0, 3);
        }
        return lane;
    }

    private float GetWrongLetterChanceForLevel(int level)
    {
        // Level 1-2: No wrong letters
        if (level <= 2)
        {
            return 0f;
        }
        // Level 3: Start with very few wrong letters (10-15% chance)
        else if (level == 3)
        {
            return 0.15f;
        }
        // Level 4-5: Slightly more wrong letters (20-25%)
        else if (level <= 5)
        {
            return 0.25f;
        }
        // Level 6-10: Moderate wrong letters (30-40%)
        else if (level <= 10)
        {
            return 0.35f;
        }
        // Level 11-15: More wrong letters (45-55%)
        else if (level <= 15)
        {
            return 0.50f;
        }
        // Level 16-20: High wrong letters (60-65%)
        else if (level <= 20)
        {
            return 0.65f;
        }
        // Level 21+: Maximum wrong letters (70-75%)
        else
        {
            return 0.75f;
        }
    }

    private string GetRandomWrongLetter(List<string> correctLetters)
    {
        // Get a random letter that's NOT in the correct letters
        string wrongChar;
        int attempts = 0;
        do
        {
            wrongChar = wrongLettersPool[Random.Range(0, wrongLettersPool.Length)].ToString();
            attempts++;
        } while (attempts < 20 && ContainsLetterIgnoreCase(correctLetters, wrongChar));

        return wrongChar;
    }

    private bool ContainsLetterIgnoreCase(List<string> letters, string letter)
    {
        string upper = letter.ToUpperInvariant();
        foreach (string c in letters)
        {
            if (c.ToUpperInvariant() == upper)
                return true;
        }
        return false;
    }

    // Cached road segments — refreshed periodically instead of every spawn
    private RoadSegment[] cachedSegments;
    private float segmentCacheTime;
    private const float SEGMENT_CACHE_INTERVAL = 1f;

    private RoadSegment FindSegmentAtZ(float worldZ)
    {
        if (cachedSegments == null || Time.time - segmentCacheTime > SEGMENT_CACHE_INTERVAL)
        {
            cachedSegments = FindObjectsOfType<RoadSegment>();
            segmentCacheTime = Time.time;
        }

        for (int i = 0; i < cachedSegments.Length; i++)
        {
            if (cachedSegments[i] == null) continue;
            float roadStart = cachedSegments[i].transform.position.z;
            if (worldZ >= roadStart && worldZ <= roadStart + 40f)
                return cachedSegments[i];
        }
        return null;
    }

    private void SpawnLetter(string c, int laneIndex, float worldZ, bool isCorrect)
    {
        RoadSegment targetSegment = FindSegmentAtZ(worldZ);
        
        if (targetSegment != null && targetSegment.letterPrefab != null)
        {
            // Calculate Z offset relative to the road segment
            float zOffset = worldZ - targetSegment.transform.position.z;
            
            // Use RoadSegment's SpawnLetter method
            targetSegment.SpawnLetter(c, isCorrect, laneIndex, zOffset);
            
#if UNITY_EDITOR
            Debug.Log($"[LetterSpawner] Spawned letter '{c}' (correct={isCorrect}) at lane {laneIndex} via RoadSegment");
#endif
            
            // Track letter lane for this segment (for obstacle avoidance)
            TrackLetterLane(worldZ, laneIndex);
        }
        else if (letterPrefab != null)
        {
            // Direct fallback: spawn the letter pickup at the world position.
            float laneX = (laneIndex - 1) * laneDistance;
            Vector3 pos = new Vector3(laneX, spawnHeightOffset, worldZ);
            GameObject obj = Instantiate(letterPrefab, pos, Quaternion.Euler(0f, 180f, 0f), lettersParent);

            var visual = obj.GetComponent<LetterVisual>();
            if (visual != null)
            {
                visual.SetLetter(c);
                visual.SetCorrect(isCorrect);
            }

            spawned.Add(obj);
            TrackLetterLane(worldZ, laneIndex);

#if UNITY_EDITOR
            Debug.Log($"[LetterSpawner] Spawned letter '{c}' (correct={isCorrect}) at {pos} via fallback prefab");
#endif
        }
        else
        {
            Debug.LogWarning($"[LetterSpawner] No RoadSegment found at Z={worldZ} and no letterPrefab fallback assigned");
        }
    }

    private void TrackLetterLane(float worldZ, int laneIndex)
    {
        // Round Z to nearest segment (120 units per segment)
        float segmentZ = Mathf.Floor(worldZ / segmentLength) * segmentLength;
        
        if (!letterLanesBySegment.ContainsKey(segmentZ))
        {
            letterLanesBySegment[segmentZ] = new List<int>();
        }
        
        if (!letterLanesBySegment[segmentZ].Contains(laneIndex))
        {
            letterLanesBySegment[segmentZ].Add(laneIndex);
#if UNITY_EDITOR
            Debug.Log($"[LetterSpawner] Tracked letter in lane {laneIndex} at segment Z={segmentZ:F1}");
#endif
        }
    }
    
    /// <summary>
    /// Get which lanes have letters in a specific road segment (for obstacle avoidance)
    /// </summary>
    public List<int> GetLetterLanesForSegment(float segmentStartZ, float segmentLength)
    {
        List<int> occupiedLanes = new List<int>();
        
        // Check if any letters exist in this segment range
        foreach (var kvp in letterLanesBySegment)
        {
            float letterSegmentZ = kvp.Key;
            // Check if letter segment overlaps with this road segment
            if (letterSegmentZ >= segmentStartZ && letterSegmentZ < segmentStartZ + segmentLength)
            {
                occupiedLanes.AddRange(kvp.Value);
            }
        }
        
        return occupiedLanes;
    }
    
    private void CleanupBehind()
    {
        if (spawnPoint == null) return;

        float cutoffZ = spawnPoint.position.z - destroyBehindDistance;

        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            var obj = spawned[i];
            if (obj == null)
            {
                spawned.RemoveAt(i);
                continue;
            }

            if (obj.transform.position.z < cutoffZ)
            {
                // Check if this is a correct letter that was missed
                if (respawnMissedLetters)
                {
                    var visual = obj.GetComponent<LetterVisual>();
                    if (visual != null && visual.IsCorrect())
                    {
                        var pickup = obj.GetComponent<LetterPickup>();
                        if (pickup != null)
                        {
                            string missedLetter = pickup.letter;
#if UNITY_EDITOR
                            Debug.Log($"[LetterSpawner] Player missed correct letter '{missedLetter}' - respawning ahead");
#endif
                            
                            // Respawn the letter ahead
                            RespawnMissedLetter(missedLetter);
                        }
                    }
                }
                
                Destroy(obj);
                spawned.RemoveAt(i);
            }
        }
    }

    private void RespawnMissedLetter(string letter)
    {
        if (spawnPoint == null) return;
        
        // Track respawn attempts
        if (!missedLetterCount.ContainsKey(letter))
        {
            missedLetterCount[letter] = 0;
        }
        
        missedLetterCount[letter]++;
        
        // Keep respawning until max attempts or collected
        if (missedLetterCount[letter] <= maxRespawnAttempts)
        {
            // Remove from tracking
            correctLetterObjects.Remove(letter);
            
            // Spawn ahead of player with some variation
            float respawnZ = spawnPoint.position.z + respawnDistanceAhead + Random.Range(-10f, 10f);
            int randomLane = Random.Range(0, 3);
            
            SpawnLetter(letter, randomLane, respawnZ, true);
            
#if UNITY_EDITOR
            Debug.Log($"[LetterSpawner] Respawned '{letter}' (attempt {missedLetterCount[letter]}/{maxRespawnAttempts})");
#endif
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log($"[LetterSpawner] Max respawn attempts reached for '{letter}'");
#endif
        }
    }

    public void ClearAllLetters()
    {
        // Destroy all spawned letter objects
        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] != null)
                Destroy(spawned[i]);
        }
        spawned.Clear();
        correctLetterObjects.Clear();
        missedLetterCount.Clear();
        letterLanesBySegment.Clear();
        currentWordLetters.Clear();
        lastSpawnedZ = 0f;
        nextSegmentOffsetZ = 0f;
        lastLane = -1;

        // Also destroy any letter children under lettersParent that weren't tracked
        if (lettersParent != null)
        {
            for (int i = lettersParent.childCount - 1; i >= 0; i--)
            {
                Transform child = lettersParent.GetChild(i);
                if (child.name != "SpawnPoint")
                    Destroy(child.gameObject);
            }
        }

#if UNITY_EDITOR
        Debug.Log("[LetterSpawner] Cleared all letters and reset state");
#endif
    }

    public void OnLetterCollected(string letter)
    {
        // Remove from tracking when collected
        correctLetterObjects.Remove(letter);
        
        // Reset respawn counter
        if (missedLetterCount.ContainsKey(letter))
        {
            missedLetterCount.Remove(letter);
        }
        
#if UNITY_EDITOR
        Debug.Log($"[LetterSpawner] Letter '{letter}' collected - respawn counter reset");
#endif
    }
}

using UnityEngine;
using System.Collections;

public class PoliceChaserController : MonoBehaviour
{
    private static PoliceChaserController _instance;
    public static PoliceChaserController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Object.FindAnyObjectByType<PoliceChaserController>(FindObjectsInactive.Include);
            }
            return _instance;
        }
    }

    public enum ChaseState
    {
        Inactive,     // Waiting for the next wave cooldown (hidden)
        IntroChase,   // Subway Surfers style start: right on player's heels at start, then drops back off screen
        Threatening,  // Actively chasing and threatening player's current lane with countdown
        Retargeting,  // Player just dodged, brief delay before next lane threat
        Surging,      // Player stumbled, temporary close surge
        Evading,      // All dodges completed! Cruiser slowing down and disappearing
        Busted        // Player failed to change lane or crashed
    }

    [Header("Chase Distance Settings")]
    [Tooltip("Normal distance in meters behind the player during a clean run")]
    public float normalTrailingDistance = 5.5f;

    [Tooltip("Close-up distance in meters behind the player when threatening or surging")]
    public float surgeDistance = 3.6f;

    [Tooltip("Distance behind player when halted on Game Over")]
    public float bustDistance = 2.6f;

    [Header("Intro Chase Settings (Subway Surfers Style)")]
    [Tooltip("Whether the police car starts right behind the runner when the game starts")]
    public bool enableIntroChase = true;

    [Tooltip("How close the police car origin is to the runner right at the start of the game")]
    public float introStartDistance = 3.6f;

    [Tooltip("Distance behind runner when the police car has fully left the screen behind the camera")]
    public float introLeaveDistance = 14.0f;

    [Tooltip("How many seconds the police car stays right behind the runner at start before gradually leaving the screen")]
    public float introStayDuration = 3.0f;

    [Tooltip("How many seconds it takes for the police car to gradually fade/leave the screen behind the player")]
    public float introLeaveDuration = 2.8f;

    [Header("Multi-Dodge Wave Settings (3-5 Lane Changes)")]
    [Tooltip("Whether periodic chase waves are enabled")]
    public bool enableChaseWaves = true;

    [Tooltip("Minimum number of lane changes required to evade in a wave")]
    public int minDodgesPerWave = 3;

    [Tooltip("Maximum number of lane changes required to evade in a wave")]
    public int maxDodgesPerWave = 5;

    [Tooltip("Minimum cooldown in seconds before the next chase wave starts")]
    public float minWaveInterval = 30f;

    [Tooltip("Maximum cooldown in seconds before the next chase wave starts")]
    public float maxWaveInterval = 45f;

    [Tooltip("Seconds player has to change lane for each threat before getting rammed (3.0 seconds)")]
    public float threatReactionTime = 3.0f;

    [Tooltip("Short breathing delay between consecutive lane changes in the same wave")]
    public float retargetDelay = 0.6f;

    [Header("Speed & Responsiveness")]
    [Tooltip("How fast the vehicle changes lanes to follow the player")]
    public float laneFollowSpeed = 7.0f;

    [Tooltip("How quickly distance adjusts when surging")]
    public float surgeSpeed = 8.0f;

    [Tooltip("How quickly distance backs off after recovery")]
    public float backoffSpeed = 3.0f;

    [Header("Audio & Siren")]
    public AudioSource sirenSource;
    public float trailingVolume = 0.45f;
    public float surgeVolume = 0.95f;

    [Header("Emergency Lights")]
    public Light redLight;
    public Light blueLight;
    public Renderer redLampRenderer;
    public Renderer blueLampRenderer;
    public float flashInterval = 0.12f;

    [Header("Ground Snapping")]
    public float yOffset = 0.05f;

    private Transform targetPlayer;
    private SimplePlayerController playerController;
    private ChaseState currentState = ChaseState.Inactive;
    private float currentDistance;
    private float targetDistance;
    private float flashTimer;
    private bool redLightOn = true;
    private Coroutine surgeRoutine;
    private Coroutine retargetRoutine;
    private Coroutine evadeRoutine;
    private Coroutine introChaseRoutine;

    // Wave Tracking
    private float waveTimer = 0f;
    private int targetDodges = 3;
    private int completedDodges = 0;
    private int threatenedLane = -1;
    private float threatTimeRemaining = 0f;

    public ChaseState CurrentState => currentState;
    public int CompletedDodges => completedDodges;
    public int TargetDodges => targetDodges;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        if (sirenSource == null)
            sirenSource = GetComponent<AudioSource>();

        currentDistance = normalTrailingDistance;
        targetDistance = normalTrailingDistance;
    }

    private void Start()
    {
        FindPlayerTarget();
        SetEmergencyLightsActive(false);
    }

    private void FindPlayerTarget()
    {
        if (targetPlayer == null || playerController == null)
        {
            var pc = SimplePlayerController.Instance != null 
                ? SimplePlayerController.Instance 
                : Object.FindAnyObjectByType<SimplePlayerController>(FindObjectsInactive.Include);
            if (pc != null)
            {
                playerController = pc;
                targetPlayer = pc.transform;
            }
        }
    }

    public float GetNextWaveInterval()
    {
        return Random.Range(minWaveInterval, maxWaveInterval);
    }

    private void Update()
    {
        if (targetPlayer == null || playerController == null)
        {
            FindPlayerTarget();
            if (targetPlayer == null) return;
        }

        UpdateThreatCycle();

        if (currentState != ChaseState.Inactive)
        {
            UpdateEmergencyLights();
            UpdatePosition();
            UpdateAudio();
        }
    }

    private void UpdateThreatCycle()
    {
        if (!enableChaseWaves || playerController == null || !playerController.IsMoving)
            return;

        if (currentState == ChaseState.Inactive)
        {
            waveTimer -= Time.deltaTime;
            if (waveTimer <= 0f)
            {
                StartChaseWave();
            }
        }
        else if (currentState == ChaseState.Threatening)
        {
            threatTimeRemaining -= Time.deltaTime;
            float progress = Mathf.Clamp01(threatTimeRemaining / threatReactionTime);

            if (PoliceThreatUI.Instance != null)
            {
                PoliceThreatUI.Instance.UpdateProgress(progress, threatTimeRemaining);
            }

            // Time expired and player is still in the threatened lane: RAM FROM BEHIND!
            if (threatTimeRemaining <= 0f)
            {
                OnThreatFailedRamPlayer();
            }
        }
    }

    /// <summary>
    /// Starts a new chase wave with a random 3 to 5 dodges required.
    /// If isStartOfGame is true, starts right on runner's heels (introStartDistance).
    /// </summary>
    public void StartChaseWave(bool isStartOfGame = false)
    {
        if (currentState == ChaseState.Busted) return;
        FindPlayerTarget();
        if (targetPlayer == null) return;

        targetDodges = Random.Range(minDodgesPerWave, maxDodgesPerWave + 1); // 3, 4, or 5
        completedDodges = 0;

        gameObject.SetActive(true);
        SetCarVisibility(true);

        if (isStartOfGame)
        {
            currentDistance = introStartDistance; // start right behind player (3.6m)
            targetDistance = introStartDistance;
        }
        else
        {
            currentDistance = 8.5f; // approach from behind
            targetDistance = surgeDistance;
        }

        float groundY = targetPlayer.position.y + yOffset;
        transform.position = new Vector3(targetPlayer.position.x, groundY, targetPlayer.position.z - currentDistance);
        transform.rotation = Quaternion.identity;

        SetEmergencyLightsActive(true);

        if (sirenSource != null)
        {
            sirenSource.volume = surgeVolume;
            sirenSource.pitch = 1.05f;
            if (!sirenSource.isPlaying)
                sirenSource.Play();
        }

        if (CurvedWorldManager.Instance != null)
        {
            CurvedWorldManager.Instance.ApplyCurvatureToGameObject(gameObject);
        }

        StartDodgeThreat(1, targetDodges);
    }

    private void StartDodgeThreat(int dodgeIndex, int totalDodges)
    {
        currentState = ChaseState.Threatening;
        threatenedLane = playerController.CurrentLane;
        threatTimeRemaining = threatReactionTime; // 3.0 seconds to change lane
        targetDistance = surgeDistance;

        if (PoliceThreatUI.Instance != null)
        {
            PoliceThreatUI.Instance.ShowThreat(threatReactionTime, threatenedLane, dodgeIndex, totalDodges);
        }

        if (sirenSource != null)
        {
            sirenSource.pitch = 1.2f;
            sirenSource.volume = surgeVolume;
        }
    }

    /// <summary>
    /// Called by SimplePlayerController whenever the player switches lanes.
    /// </summary>
    public void OnPlayerLaneChanged(int newLane)
    {
        if (currentState == ChaseState.Threatening)
        {
            // Player successfully changed out of the threatened lane!
            if (newLane != threatenedLane)
            {
                completedDodges++;
                if (completedDodges < targetDodges)
                {
                    // More dodges remaining in this wave: brief retarget pause
                    if (retargetRoutine != null) StopCoroutine(retargetRoutine);
                    retargetRoutine = StartCoroutine(RetargetNextDodgeCoroutine());
                }
                else
                {
                    // Successfully evaded all 3-5 dodges!
                    EndChaseWaveSuccess();
                }
            }
            else
            {
                if (PoliceThreatUI.Instance != null)
                {
                    PoliceThreatUI.Instance.UpdateArrows(newLane);
                }
            }
        }
    }

    private IEnumerator RetargetNextDodgeCoroutine()
    {
        currentState = ChaseState.Retargeting;
        if (PoliceThreatUI.Instance != null)
        {
            PoliceThreatUI.Instance.ShowDodgeSuccess(completedDodges, targetDodges);
        }

        if (sirenSource != null)
        {
            sirenSource.pitch = 1.05f;
            sirenSource.volume = trailingVolume;
        }

        yield return new WaitForSeconds(retargetDelay);

        if (currentState == ChaseState.Retargeting)
        {
            StartDodgeThreat(completedDodges + 1, targetDodges);
        }
        retargetRoutine = null;
    }

    private void EndChaseWaveSuccess()
    {
        if (retargetRoutine != null)
        {
            StopCoroutine(retargetRoutine);
            retargetRoutine = null;
        }

        if (PoliceThreatUI.Instance != null)
        {
            PoliceThreatUI.Instance.ShowEvaded();
        }

        currentState = ChaseState.Evading;
        if (evadeRoutine != null) StopCoroutine(evadeRoutine);
        evadeRoutine = StartCoroutine(EvadeAndDisappearCoroutine());
    }

    private IEnumerator EvadeAndDisappearCoroutine()
    {
        targetDistance = introLeaveDistance; // drop far back behind camera

        float timer = 0f;
        float startVol = sirenSource != null ? sirenSource.volume : surgeVolume;
        float startDist = currentDistance;

        while (timer < introLeaveDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / introLeaveDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            currentDistance = Mathf.Lerp(startDist, introLeaveDistance, smoothT);
            targetDistance = currentDistance;

            if (sirenSource != null)
            {
                sirenSource.volume = Mathf.Lerp(startVol, 0f, smoothT);
            }
            yield return null;
        }

        SetEmergencyLightsActive(false);
        if (sirenSource != null && sirenSource.isPlaying)
        {
            sirenSource.Stop();
        }

        SetCarVisibility(false);
        currentState = ChaseState.Inactive;
        waveTimer = GetNextWaveInterval(); // 30-45 second interval before next chase wave
        evadeRoutine = null;
    }

    private void OnThreatFailedRamPlayer()
    {
        if (retargetRoutine != null)
        {
            StopCoroutine(retargetRoutine);
            retargetRoutine = null;
        }

        if (PoliceThreatUI.Instance != null)
        {
            PoliceThreatUI.Instance.HideThreat();
        }

        currentState = ChaseState.Busted;
        targetDistance = 0.5f; // ram right into player

        if (SoundEffectsManager.Instance != null)
        {
            SoundEffectsManager.Instance.PlayObstacleHit();
        }

        if (playerController != null)
        {
            // Hit obstacle / knock down from behind
            playerController.TriggerFall();
            if (UImanager.uimanager != null)
            {
                UImanager.uimanager.Fail();
            }
            else if (GameStateController.Instance != null)
            {
                GameStateController.Instance.GameOver("Hit from behind by Police Cruiser");
            }
        }
    }

    private void UpdateEmergencyLights()
    {
        flashTimer += Time.deltaTime;
        if (flashTimer >= flashInterval)
        {
            flashTimer = 0f;
            redLightOn = !redLightOn;

            if (redLight != null) redLight.enabled = redLightOn;
            if (blueLight != null) blueLight.enabled = !redLightOn;

            if (redLampRenderer != null && redLampRenderer.material != null)
            {
                Color c = redLightOn ? Color.red * 2f : Color.red * 0.2f;
                redLampRenderer.material.color = c;
            }
            if (blueLampRenderer != null && blueLampRenderer.material != null)
            {
                Color c = !redLightOn ? Color.cyan * 2f : Color.cyan * 0.2f;
                blueLampRenderer.material.color = c;
            }
        }
    }

    private void UpdatePosition()
    {
        if (targetPlayer == null) return;

        // Smooth distance adjustments
        // During EvadeAndDisappearCoroutine, currentDistance is driven smoothly by the coroutine.
        if (currentState != ChaseState.Evading && currentState != ChaseState.IntroChase)
        {
            float adjustRate = (currentState == ChaseState.Surging || currentState == ChaseState.Threatening || currentState == ChaseState.Busted) ? surgeSpeed : backoffSpeed;
            currentDistance = Mathf.MoveTowards(currentDistance, targetDistance, adjustRate * Time.deltaTime);
        }

        // Target coordinates
        float desiredX = targetPlayer.position.x;
        float desiredZ = targetPlayer.position.z - currentDistance;
        float groundY = targetPlayer.position.y + yOffset;

        // Smooth X lane tracking with suspension roll
        Vector3 curPos = transform.position;
        float newX = Mathf.Lerp(curPos.x, desiredX, laneFollowSpeed * Time.deltaTime);
        float xDelta = newX - curPos.x;

        // Steering roll: slight tilt when turning lanes
        float rollAngle = Mathf.Clamp(-xDelta * 18f, -4f, 4f);
        float pitchAngle = (currentState == ChaseState.Busted) ? -2.5f : 0f; // nose-dive when braking hard
        Quaternion targetRot = Quaternion.Euler(pitchAngle, 0f, rollAngle);

        transform.position = new Vector3(newX, groundY, desiredZ);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.deltaTime);
    }

    private void UpdateAudio()
    {
        if (sirenSource == null || !sirenSource.isPlaying) return;

        // EvadeAndDisappearCoroutine handles siren volume fading during leave phase
        if (currentState == ChaseState.Evading || currentState == ChaseState.IntroChase) return;

        float targetVol = (currentState == ChaseState.Surging || currentState == ChaseState.Threatening || currentState == ChaseState.Busted) ? surgeVolume : trailingVolume;
        sirenSource.volume = Mathf.Lerp(sirenSource.volume, targetVol, 3f * Time.deltaTime);
        sirenSource.pitch = (currentState == ChaseState.Threatening) ? 1.2f : ((currentState == ChaseState.Surging) ? 1.05f : 1.0f);
    }

    private void SetEmergencyLightsActive(bool active)
    {
        if (redLight != null) redLight.enabled = active;
        if (blueLight != null) blueLight.enabled = active;
    }

    private void SetCarVisibility(bool visible)
    {
        var renderers = GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers)
        {
            if (r != null) r.enabled = visible;
        }
    }

    /// <summary>
    /// Called when the player starts running. Starts the Subway Surfers style police chase right on the runner's tail.
    /// Player has 3.0 seconds to change lane, must change lane between 3 and 5 times before police car stores/leaves the screen.
    /// </summary>
    public void StartChase()
    {
        // CRITICAL: Activate GameObject FIRST so coroutines and lifecycle methods run!
        gameObject.SetActive(true);

        FindPlayerTarget();
        if (targetPlayer == null) return;

        if (introChaseRoutine != null)
        {
            StopCoroutine(introChaseRoutine);
            introChaseRoutine = null;
        }
        if (surgeRoutine != null)
        {
            StopCoroutine(surgeRoutine);
            surgeRoutine = null;
        }
        if (retargetRoutine != null)
        {
            StopCoroutine(retargetRoutine);
            retargetRoutine = null;
        }
        if (evadeRoutine != null)
        {
            StopCoroutine(evadeRoutine);
            evadeRoutine = null;
        }

        completedDodges = 0;
        threatenedLane = -1;

        if (PoliceThreatUI.Instance != null)
        {
            PoliceThreatUI.Instance.HideThreat();
        }

        // First chase at start of the game: simple Subway Surfers style chase right behind runner,
        // stays for 3 seconds then gradually leaves the screen behind without hitting the player.
        if (enableIntroChase)
        {
            introChaseRoutine = StartCoroutine(IntroChaseSequenceCoroutine());
        }
        else
        {
            currentState = ChaseState.Inactive;
            waveTimer = GetNextWaveInterval();
            SetEmergencyLightsActive(false);
            if (sirenSource != null && sirenSource.isPlaying)
            {
                sirenSource.Stop();
            }
            SetCarVisibility(false);
        }
    }

    private IEnumerator IntroChaseSequenceCoroutine()
    {
        currentState = ChaseState.IntroChase;
        SetCarVisibility(true);

        // Start right behind the player in their current lane
        currentDistance = introStartDistance;
        targetDistance = introStartDistance;

        float groundY = targetPlayer.position.y + yOffset;
        transform.position = new Vector3(targetPlayer.position.x, groundY, targetPlayer.position.z - currentDistance);
        transform.rotation = Quaternion.identity;

        SetEmergencyLightsActive(true);

        if (sirenSource != null)
        {
            sirenSource.volume = surgeVolume;
            sirenSource.pitch = 1.05f;
            if (!sirenSource.isPlaying)
                sirenSource.Play();
        }

        if (CurvedWorldManager.Instance != null)
        {
            CurvedWorldManager.Instance.ApplyCurvatureToGameObject(gameObject);
        }

        // 1. Stay right behind runner for ~3 seconds (Subway Surfers inspector chase start)
        float elapsed = 0f;
        while (elapsed < introStayDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2. Gradually drop back and leave the screen from behind over introLeaveDuration (~2.8s)
        float leaveElapsed = 0f;
        float startVol = sirenSource != null ? sirenSource.volume : surgeVolume;

        while (leaveElapsed < introLeaveDuration)
        {
            leaveElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(leaveElapsed / introLeaveDuration);
            // Smooth ease-in-out curve for natural vehicle acceleration backward
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            currentDistance = Mathf.Lerp(introStartDistance, introLeaveDistance, smoothT);
            targetDistance = currentDistance;

            if (sirenSource != null)
            {
                sirenSource.volume = Mathf.Lerp(startVol, 0f, smoothT);
            }

            yield return null;
        }

        // 3. Completely hide once fully behind the camera off-screen
        currentDistance = introLeaveDistance;
        targetDistance = introLeaveDistance;

        SetEmergencyLightsActive(false);
        if (sirenSource != null && sirenSource.isPlaying)
        {
            sirenSource.Stop();
        }

        SetCarVisibility(false);

        // 4. Set to Inactive and arm the next wave countdown (30-45s)
        currentState = ChaseState.Inactive;
        waveTimer = GetNextWaveInterval();
        introChaseRoutine = null;
    }

    /// <summary>
    /// Called when player stumbles or clips an obstacle (Subway Surfers style surge).
    /// </summary>
    public void TriggerSurge(float duration = 4.0f)
    {
        if (currentState == ChaseState.Busted) return;

        gameObject.SetActive(true);
        FindPlayerTarget();
        if (targetPlayer == null) return;

        if (surgeRoutine != null)
            StopCoroutine(surgeRoutine);

        surgeRoutine = StartCoroutine(SurgeCoroutine(duration));
    }

    private IEnumerator SurgeCoroutine(float duration)
    {
        currentState = ChaseState.Surging;
        targetDistance = surgeDistance;

        if (currentDistance > 8f)
        {
            currentDistance = 7.5f;
            if (targetPlayer != null)
            {
                transform.position = new Vector3(targetPlayer.position.x, targetPlayer.position.y + yOffset, targetPlayer.position.z - currentDistance);
            }
        }

        SetEmergencyLightsActive(true);

        if (sirenSource != null)
        {
            sirenSource.pitch = 1.05f;
            sirenSource.volume = surgeVolume;
            if (!sirenSource.isPlaying)
                sirenSource.Play();
        }

        yield return new WaitForSeconds(duration);

        if (!enableChaseWaves)
        {
            // Gradually drop back and leave the screen from behind
            float leaveTimer = 0f;
            float leaveDur = introLeaveDuration;
            float startDist = currentDistance;
            float startVol = sirenSource != null ? sirenSource.volume : surgeVolume;

            while (leaveTimer < leaveDur)
            {
                leaveTimer += Time.deltaTime;
                float t = Mathf.Clamp01(leaveTimer / leaveDur);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                currentDistance = Mathf.Lerp(startDist, introLeaveDistance, smoothT);
                targetDistance = currentDistance;

                if (sirenSource != null)
                    sirenSource.volume = Mathf.Lerp(startVol, 0f, smoothT);

                yield return null;
            }

            SetEmergencyLightsActive(false);
            if (sirenSource != null && sirenSource.isPlaying)
            {
                sirenSource.Stop();
            }

            gameObject.SetActive(false);
            currentState = ChaseState.Inactive;
        }
        else
        {
            currentState = ChaseState.Threatening;
            targetDistance = normalTrailingDistance;
        }

        surgeRoutine = null;
    }

    /// <summary>
    /// Called when player hits an obstacle (Game Over).
    /// </summary>
    public void TriggerBusted()
    {
        gameObject.SetActive(true);
        SetCarVisibility(true);
        FindPlayerTarget();

        if (introChaseRoutine != null)
        {
            StopCoroutine(introChaseRoutine);
            introChaseRoutine = null;
        }
        if (retargetRoutine != null)
        {
            StopCoroutine(retargetRoutine);
            retargetRoutine = null;
        }
        if (evadeRoutine != null)
        {
            StopCoroutine(evadeRoutine);
            evadeRoutine = null;
        }
        if (surgeRoutine != null)
        {
            StopCoroutine(surgeRoutine);
            surgeRoutine = null;
        }

        currentState = ChaseState.Busted;
        targetDistance = bustDistance;

        if (currentDistance > 7.0f)
        {
            currentDistance = 6.5f;
            if (targetPlayer != null)
            {
                transform.position = new Vector3(targetPlayer.position.x, targetPlayer.position.y + yOffset, targetPlayer.position.z - currentDistance);
            }
        }

        SetEmergencyLightsActive(true);

        if (sirenSource != null)
        {
            sirenSource.pitch = 0.95f;
            sirenSource.volume = surgeVolume;
            if (!sirenSource.isPlaying) sirenSource.Play();
        }
    }

    /// <summary>
    /// Called when returning to menu, resetting game, or pausing.
    /// </summary>
    public void ResetChase()
    {
        if (introChaseRoutine != null)
        {
            StopCoroutine(introChaseRoutine);
            introChaseRoutine = null;
        }
        if (retargetRoutine != null)
        {
            StopCoroutine(retargetRoutine);
            retargetRoutine = null;
        }
        if (evadeRoutine != null)
        {
            StopCoroutine(evadeRoutine);
            evadeRoutine = null;
        }
        if (surgeRoutine != null)
        {
            StopCoroutine(surgeRoutine);
            surgeRoutine = null;
        }

        currentState = ChaseState.Inactive;
        completedDodges = 0;
        threatenedLane = -1;
        currentDistance = normalTrailingDistance;
        targetDistance = normalTrailingDistance;
        SetEmergencyLightsActive(false);

        if (PoliceThreatUI.Instance != null)
        {
            PoliceThreatUI.Instance.HideThreat();
        }

        if (sirenSource != null && sirenSource.isPlaying)
        {
            sirenSource.Stop();
        }

        SetCarVisibility(false);
        gameObject.SetActive(false);
    }
}

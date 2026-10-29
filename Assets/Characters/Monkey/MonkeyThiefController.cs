using UnityEngine;
using System.Collections;

public class MonkeyThiefController : MonoBehaviour
{
    private static MonkeyThiefController _instance;
    public static MonkeyThiefController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Object.FindAnyObjectByType<MonkeyThiefController>(FindObjectsInactive.Include);
            }
            return _instance;
        }
    }

    public enum ChaseState
    {
        Inactive,
        IntroChase,   // Runs right behind player at start for 3s then dashes off screen
        Threatening,  // Threatening player in current lane (3s countdown)
        Retargeting,  // Brief pause between dodges
        Evading,      // Dropping back after successful dodges
        StealingLetter,
        Busted        // Player crashed or failed to dodge
    }

    [Header("State")]
    [SerializeField] private ChaseState currentState = ChaseState.Inactive;
    public ChaseState CurrentState => currentState;

    [Header("Targeting & Distances")]
    public Transform targetPlayer;
    public SimplePlayerController playerController;
    public float currentDistance = 3.6f;
    public float targetDistance = 3.6f;
    public float introStartDistance = 3.2f;
    public float introLeaveDistance = 16.0f;
    public float surgeDistance = 3.2f;
    public float bustDistance = 2.0f;
    public float introStayDuration = 3.0f;
    public float introLeaveDuration = 2.8f;

    [Header("Persistent Follow (Subway Surfers style)")]
    [Tooltip("When enabled the monkey stays visible and directly behind the player at all times.")]
    public bool persistentFollow = true;
    [Tooltip("Distance the monkey keeps directly behind the player while following.")]
    public float followDistance = 3.0f;

    [Header("Multi-Dodge Wave Settings (3-5 Lane Changes)")]
    public bool enableChaseWaves = true;
    public int minDodgesPerWave = 3;
    public int maxDodgesPerWave = 5;
    public float minWaveInterval = 30f;
    public float maxWaveInterval = 45f;
    public float threatReactionTime = 3.0f;
    public float retargetDelay = 0.6f;
    public float laneFollowSpeed = 9.0f;
    public float surgeSpeed = 9.0f;
    public float backoffSpeed = 3.5f;

    [Header("Letter Stealing")]
    public float letterStealInterval = 20f;
    private float letterStealTimer = 0f;

    [Header("Audio & Visuals")]
    public Animator animator;
    public AudioSource audioSource;
    public AudioClip monkeyChatter;

    [Header("Pose Correction")]
    [Tooltip("Rotates the spine upright after the animation runs - fixes the hunched run pose. 0 = off.")]
    public float spineStraightenAngle = 20f;
    public float modelYawOffset = 45f;
    private Transform spineBone;

    // Internal trackers
    private int targetDodges = 3;
    private int completedDodges = 0;
    private int threatenedLane = -1;
    private float threatTimeRemaining = 0f;
    private float waveTimer = 35f;
    private float pivotToFeetOffset = 0f;

    private Coroutine introRoutine;
    private Coroutine retargetRoutine;
    private Coroutine evadeRoutine;
    private Coroutine stealRoutine;

    public int TargetDodges => targetDodges;
    public int CompletedDodges => completedDodges;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        CachePivotOffset();

        if (animator != null && animator.avatar != null && animator.avatar.isHuman)
            spineBone = animator.GetBoneTransform(HumanBodyBones.Spine);
    }

    // The model's pivot sits at its center, so the feet hang ~0.5m below transform.y.
    // Cache that gap once so ground placement can lift the pivot and keep feet on the floor.
    private void CachePivotOffset()
    {
        var rends = GetComponentsInChildren<Renderer>(true);
        float minY = float.MaxValue;
        foreach (var r in rends) minY = Mathf.Min(minY, r.bounds.min.y);
        pivotToFeetOffset = (minY == float.MaxValue) ? 0f : transform.position.y - minY;
    }

    private void Start()
    {
        FindPlayerTarget();
        if (persistentFollow)
        {
            FollowBehindPlayer();
        }
    }

    // The monkey object ships disabled in the scene, so enable it once the scene is ready.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapPersistentFollow()
    {
        var monkey = Object.FindAnyObjectByType<MonkeyThiefController>(FindObjectsInactive.Include);
        if (monkey != null && monkey.persistentFollow && !monkey.gameObject.activeSelf)
        {
            monkey.gameObject.SetActive(true);
        }
    }

    private void FindPlayerTarget()
    {
        if (targetPlayer == null)
        {
            var sp = SimplePlayerController.Instance != null
                ? SimplePlayerController.Instance
                : Object.FindAnyObjectByType<SimplePlayerController>(FindObjectsInactive.Include);
            if (sp != null)
            {
                playerController = sp;
                targetPlayer = sp.transform;
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

        if (persistentFollow)
        {
            FollowBehindPlayer();
            return;
        }

        UpdateThreatCycle();

        if (currentState != ChaseState.Inactive && currentState != ChaseState.StealingLetter)
        {
            UpdatePosition();
        }

        // Periodic letter snatching during gameplay if player is moving
        if (currentState == ChaseState.Inactive && playerController.IsMoving)
        {
            letterStealTimer += Time.deltaTime;
            if (letterStealTimer >= letterStealInterval)
            {
                letterStealTimer = 0f;
                TryStartLetterThreat();
            }
        }
    }

    // Runs after the Animator each frame - lifts the hunched run pose toward upright.
    private void LateUpdate()
    {
        if (spineBone != null && spineStraightenAngle != 0f)
        {
            spineBone.rotation = Quaternion.AngleAxis(-spineStraightenAngle, Vector3.Cross(Vector3.up, GetCameraViewDir())) * spineBone.rotation;
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

            if (threatTimeRemaining <= 0f)
            {
                OnThreatFailedStealLetter();
            }
        }
    }

    private void UpdatePosition()
    {
        if (targetPlayer == null) return;

        if (currentState != ChaseState.Evading && currentState != ChaseState.IntroChase)
        {
            float rate = (currentState == ChaseState.Threatening || currentState == ChaseState.Busted) ? surgeSpeed : backoffSpeed;
            currentDistance = Mathf.MoveTowards(currentDistance, targetDistance, rate * Time.deltaTime);
        }

        Vector3 viewDir = GetCameraViewDir();
        Vector3 lateralDir = Vector3.Cross(Vector3.up, viewDir);
        Vector3 desiredPos = targetPlayer.position - viewDir * currentDistance;

        // Lerp only the lateral (lane) component; the behind distance is driven by currentDistance
        float latOffset = Vector3.Dot(transform.position - desiredPos, lateralDir);
        latOffset = Mathf.Lerp(latOffset, 0f, laneFollowSpeed * Time.deltaTime);

        Vector3 newPos = desiredPos + lateralDir * latOffset;
        newPos.y = targetPlayer.position.y + pivotToFeetOffset;
        transform.position = newPos;
        transform.rotation = Quaternion.LookRotation(viewDir) * Quaternion.Euler(0f, modelYawOffset, 0f);

        if (animator != null)
        {
            animator.SetFloat("Speed", 1.2f);
        }
    }

    /// <summary>
    /// Horizontal direction from the camera toward the player (the run direction on screen).
    /// "Behind the player" = opposite this direction, toward the camera.
    /// </summary>
    private Vector3 GetCameraViewDir()
    {
        var cam = UnityEngine.Camera.main;
        if (cam == null || targetPlayer == null) return Vector3.forward;
        Vector3 dir = targetPlayer.position - cam.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return Vector3.forward;
        return dir.normalized;
    }

    private Renderer[] monkeyRenderers;
    private bool monkeyVisibleState = false;
    private bool monkeyRenderersCached = false;

    public void SetMonkeyVisibility(bool visible)
    {
        if (!monkeyRenderersCached || monkeyRenderers == null)
        {
            monkeyRenderers = GetComponentsInChildren<Renderer>(true);
            monkeyRenderersCached = true;
        }

        if (monkeyVisibleState == visible) return;
        monkeyVisibleState = visible;

        for (int i = 0; i < monkeyRenderers.Length; i++)
        {
            if (monkeyRenderers[i] != null) monkeyRenderers[i].enabled = visible;
        }
    }

    // Keeps the monkey visible and locked directly behind the player, Subway Surfers style.
    private void FollowBehindPlayer()
    {
        if (targetPlayer == null) return;

        if (!gameObject.activeSelf) gameObject.SetActive(true);
        if (!monkeyVisibleState) SetMonkeyVisibility(true);

        currentState = ChaseState.Inactive;
        currentDistance = followDistance;
        targetDistance = followDistance;

        Vector3 viewDir = GetCameraViewDir();
        Vector3 newPos = targetPlayer.position - viewDir * followDistance;
        newPos.y = targetPlayer.position.y + pivotToFeetOffset;
        transform.position = newPos;
        transform.rotation = Quaternion.LookRotation(viewDir) * Quaternion.Euler(0f, modelYawOffset, 0f);

        if (animator != null)
        {
            animator.SetFloat("Speed", (playerController != null && playerController.IsMoving) ? 1.2f : 0f);
        }
    }

    public void StartChase()
    {
        gameObject.SetActive(true);
        FindPlayerTarget();
        if (targetPlayer == null) return;

        StopAllChaseRoutines();

        completedDodges = 0;
        threatenedLane = -1;

        if (PoliceThreatUI.Instance != null)
        {
            PoliceThreatUI.Instance.HideThreat();
        }

        introRoutine = StartCoroutine(IntroChaseSequenceCoroutine());
    }

    private IEnumerator IntroChaseSequenceCoroutine()
    {
        currentState = ChaseState.IntroChase;
        SetMonkeyVisibility(true);

        currentDistance = introStartDistance;
        targetDistance = introStartDistance;

        float groundY = targetPlayer.position.y + pivotToFeetOffset;
        Vector3 viewDir = GetCameraViewDir();
        Vector3 startPos = targetPlayer.position - viewDir * currentDistance;
        startPos.y = groundY;
        transform.position = startPos;
        transform.rotation = Quaternion.LookRotation(viewDir) * Quaternion.Euler(0f, modelYawOffset, 0f);

        PlayMonkeySound();

        // 1. Stay right behind runner for 3 seconds
        float elapsed = 0f;
        while (elapsed < introStayDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 2. Gradually drop back and leave the screen from behind
        float leaveElapsed = 0f;
        while (leaveElapsed < introLeaveDuration)
        {
            leaveElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(leaveElapsed / introLeaveDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            currentDistance = Mathf.Lerp(introStartDistance, introLeaveDistance, smoothT);
            targetDistance = currentDistance;
            yield return null;
        }

        SetMonkeyVisibility(false);
        currentState = ChaseState.Inactive;
        waveTimer = GetNextWaveInterval();
        introRoutine = null;
    }

    public void StartChaseWave()
    {
        if (currentState == ChaseState.Busted) return;
        FindPlayerTarget();
        if (targetPlayer == null) return;

        targetDodges = Random.Range(minDodgesPerWave, maxDodgesPerWave + 1);
        completedDodges = 0;

        gameObject.SetActive(true);
        SetMonkeyVisibility(true);
        currentDistance = 8.5f;
        targetDistance = surgeDistance;
        Vector3 viewDir = GetCameraViewDir();
        Vector3 spawnPos = targetPlayer.position - viewDir * currentDistance;
        spawnPos.y = targetPlayer.position.y + pivotToFeetOffset;
        transform.position = spawnPos;
        transform.rotation = Quaternion.LookRotation(viewDir) * Quaternion.Euler(0f, modelYawOffset, 0f);

        PlayMonkeySound();
        StartDodgeThreat(1, targetDodges);
    }

    private void StartDodgeThreat(int dodgeIndex, int totalDodges)
    {
        currentState = ChaseState.Threatening;
        threatenedLane = playerController.CurrentLane;
        threatTimeRemaining = threatReactionTime; // 3.0 seconds
        targetDistance = surgeDistance;

        if (PoliceThreatUI.Instance != null)
        {
            PoliceThreatUI.Instance.ShowThreat(threatReactionTime, threatenedLane, dodgeIndex, totalDodges);
        }

        PlayMonkeySound();
    }

    public void OnPlayerLaneChanged(int newLane)
    {
        if (currentState == ChaseState.Threatening)
        {
            if (newLane != threatenedLane)
            {
                completedDodges++;
                if (completedDodges < targetDodges)
                {
                    if (retargetRoutine != null) StopCoroutine(retargetRoutine);
                    retargetRoutine = StartCoroutine(RetargetNextDodgeCoroutine());
                }
                else
                {
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
        targetDistance = introLeaveDistance;

        float timer = 0f;
        float startDist = currentDistance;

        while (timer < introLeaveDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / introLeaveDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            currentDistance = Mathf.Lerp(startDist, introLeaveDistance, smoothT);
            targetDistance = currentDistance;
            yield return null;
        }

        SetMonkeyVisibility(false);
        currentState = ChaseState.Inactive;
        waveTimer = GetNextWaveInterval();
        evadeRoutine = null;
    }

    private void OnThreatFailedStealLetter()
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

        if (stealRoutine != null) StopCoroutine(stealRoutine);
        stealRoutine = StartCoroutine(StealRoutine());
    }

    public void TriggerBusted()
    {
        gameObject.SetActive(true);
        SetMonkeyVisibility(true);
        FindPlayerTarget();

        StopAllChaseRoutines();

        currentState = ChaseState.Busted;
        targetDistance = bustDistance;

        if (animator != null)
        {
            animator.SetTrigger("Jump");
        }
        PlayMonkeySound();
    }

    public void ResetChase()
    {
        StopAllChaseRoutines();

        currentState = ChaseState.Inactive;
        completedDodges = 0;
        threatenedLane = -1;
        currentDistance = persistentFollow ? followDistance : 3.6f;
        targetDistance = currentDistance;

        if (PoliceThreatUI.Instance != null)
        {
            PoliceThreatUI.Instance.HideThreat();
        }

        if (persistentFollow)
        {
            SetMonkeyVisibility(true);
            gameObject.SetActive(true);
        }
        else
        {
            SetMonkeyVisibility(false);
            gameObject.SetActive(false);
        }
    }

    private void StopAllChaseRoutines()
    {
        if (introRoutine != null) { StopCoroutine(introRoutine); introRoutine = null; }
        if (retargetRoutine != null) { StopCoroutine(retargetRoutine); retargetRoutine = null; }
        if (evadeRoutine != null) { StopCoroutine(evadeRoutine); evadeRoutine = null; }
        if (stealRoutine != null) { StopCoroutine(stealRoutine); stealRoutine = null; }
    }

    private void PlayMonkeySound()
    {
        if (audioSource != null && monkeyChatter != null)
        {
            audioSource.PlayOneShot(monkeyChatter, 0.7f);
        }
    }

    private void TryStartLetterThreat()
    {
        var word = WordManager.Instance;
        if (word == null) return;
        foreach (char letter in word.GetCollectedLetters())
        {
            if (!char.IsWhiteSpace(letter) && letter != '_')
            {
                StartChaseWave();
                return;
            }
        }
    }

    private IEnumerator StealRoutine()
    {
        currentState = ChaseState.StealingLetter;
        SetMonkeyVisibility(true);
        string stolenLetter;
        bool stolen = WordManager.Instance != null && WordManager.Instance.TryStealCollectedLetter(out stolenLetter);
        if (stolen)
        {
            if (animator != null) animator.SetTrigger("Steal");
            PlayMonkeySound();
            ToastManager.Show("Word Thief stole a letter! Collect it again ahead.");
        }

        yield return new WaitForSeconds(0.3f);
        if (targetPlayer == null) yield break;
        if (animator != null)
        {
            animator.ResetTrigger("Steal");
            animator.Play("Run");
            animator.SetFloat("Speed", 1.5f);
        }
        float elapsed = 0f;
        Vector3 startOffset = transform.position - targetPlayer.position;
        Vector3 endOffset = Vector3.forward * 18f;
        while (elapsed < 2f && targetPlayer != null && playerController != null && playerController.IsMoving)
        {
            elapsed += Time.deltaTime;
            Vector3 offset = Vector3.Lerp(startOffset, endOffset, Mathf.Clamp01(elapsed / 2f));
            offset.y = pivotToFeetOffset;
            transform.position = targetPlayer.position + offset;
            transform.rotation = Quaternion.Euler(0f, modelYawOffset, 0f);
            yield return null;
        }

        SetMonkeyVisibility(false);
        currentState = ChaseState.Inactive;
        waveTimer = GetNextWaveInterval();
        letterStealTimer = 0f;
        stealRoutine = null;
    }
}

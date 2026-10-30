using System.Collections;
using UnityEngine;

public class SimplePlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardSpeed = 12f;
    public float laneDistance = 2.5f;
    public float laneChangeSpeed = 15f;
    [Tooltip("The time in seconds it takes to complete a lane change. Lower values make it faster and snappier.")]
    [Range(0.05f, 1.0f)]
    public float laneChangeDuration = 0.25f;

    [Header("Jump & Slide Settings")]
    public float jumpForce = 8.5f;
    public float gravity = 22f;
    public float slideDuration = 0.8f;
    public float laneSwitchHopForce = 3.5f; // Small bounce/hop force when changing lanes

    [Header("Gesture Settings")]
    public float minSwipeDistance = 45f;

    [Header("Home Intro")]
    public Vector3 roadsideIdlePosition = new Vector3(9.3f, 0f, 9.7f);
    public Vector3 roadsideIdleRotation = new Vector3(0f, -90f, 0f);
    public Vector3 roadStartPosition = new Vector3(0f, 0f, 6.5f);
    public Vector3 roadStartRotation = Vector3.zero;
    public string roadsideIdleAnimationName = "idle";
    public string standUpAnimationName = "idle";
    public float standUpDuration = 0.15f;
    public float introJumpDuration = 0.8f;
    public float introJumpHeight = 1.6f;

    [Header("Collision Detection")]
    [Tooltip("Radius of the player hit volume used to detect coins and obstacles.")]
    public float hitRadius = 0.6f;
    [Tooltip("Local offset of the hit volume center from the player's feet.")]
    public Vector3 hitCenterOffset = new Vector3(0f, 0.75f, 0.2f);
    public int maxOverlapResults = 16;
    [Range(1, 4)] public int collisionCheckInterval = 1;

    [Header("Fail / Knockback")]
    public string failAnimationName = "fall";
    public float fallKnockbackDistance = 3f;
    public float fallKnockbackHeight = 1.5f;
    public float fallKnockbackDuration = 0.6f;

    [Header("Skateboard")]
    public GameObject skateboardObject;
    public Transform skateboardAttachPoint;
    public float skateboardDuration = 15f;
    public float multiTapThreshold = 0.45f;
    public float tutorialMultiTapThreshold = 1.0f;
    [Tooltip("Number of quick taps needed to activate skateboard (2 for Subway Surfers double-tap).")]
    public int tapsToActivateSkateboard = 2;
    [Tooltip("When true, double-tapping or pressing E will summon the skateboard during gameplay.")]
    public bool enableDoubleTapSkateboard = true;
    [Tooltip("Temporarily disabled: when off, triple-tapping will not activate the skateboard during normal gameplay.")]
    public bool enableTripleTapSkateboard = false;
    [Tooltip("Name of the Animator state to play while skateboarding")]
    public string skateAnimationName = "skating";

    [Header("Hang Glider")]
    public GameObject gliderPrefab;
    public float gliderHeight = 7.5f;
    public float gliderAscentSpeed = 6.0f;
    public float gliderDescentSpeed = 4.5f;
    public float gliderDuration = 10f;
    public Transform gliderAttachPoint;
    public Vector3 gliderAttachOffset = new Vector3(0f, 1.2f, -0.2f);
    public Vector3 gliderAttachRotation = Vector3.zero;
    public Vector3 gliderAttachScale = new Vector3(3f, 3f, 3f);

    [Header("Coin Magnet")]
    public float magnetDuration = 10f;
    public float magnetRadius = 16f;
    [Tooltip("Magnet model shown in the runner's right hand while the powerup is active (Subway Surfers style).")]
    public GameObject magnetPrefab;
    public Vector3 magnetAttachOffset = new Vector3(0f, 0.05f, 0f);
    public Vector3 magnetAttachRotation = new Vector3(0f, 0f, 90f);
    public Vector3 magnetAttachScale = new Vector3(0.15f, 0.15f, 0.15f);
    [Tooltip("Raises the right arm while the magnet is active so it reads as held up in the air.")]
    public float magnetArmRaiseAngle = 105f;
    private bool isMagnetActive = false;
    private float magnetTimer = 0f;
    private GameObject activeMagnetInstance;
    private Transform rightUpperArmBone;

    private bool isGliding = false;
    private float gliderTimer = 0f;
    private GameObject activeGliderInstance;
    private bool isDescendingFromGlider = false;

    private Animator animator;
    private Collider[] overlapResults;
    private Collider[] magnetHits = new Collider[32];
    private int currentLane = 1; // 0 = Left, 1 = Center, 2 = Right
    private float startX = 0f;
    private float targetX = 0f;
    private float laneChangeTimer = 0.25f;
    private float verticalVelocity = 0f;
    private float yPosition = 0f;
    private bool isGrounded = true;
    private bool isSliding = false;
    private float slideTimer = 0f;
    private bool isMoving = false;
    private bool isInRoadsideIdlePose = false;
    private bool isFalling = false;
    private Coroutine fallCoroutine;
    private bool jumpOnlyInput = false;
    private bool inputLocked = false;
    private bool skateboardTapMode = false;
    private float lastClickTime = -100f;
    private int clickCount = 0;
    private float skateTimer = 0f;
    private int collisionCheckFrame;

    private bool isInvulnerable = false;
    private Coroutine invulnerabilityCoroutine;
    private Vector3 skateOriginalLocalPos;
    private Quaternion skateOriginalRot;

    public static SimplePlayerController Instance { get; private set; }
    private float initialZ = 6.5f;
    private float maxDistanceTraveled = 0f;

    public bool IsMoving
    {
        get { return isMoving; }
    }

    public int CurrentLane
    {
        get { return currentLane; }
    }

    public bool IsInRoadsideIdlePose
    {
        get { return isInRoadsideIdlePose; }
    }

    public bool IsSkating
    {
        get { return isSkatingInternal; }
        set { isSkatingInternal = value; }
    }
    private bool isSkatingInternal = false;

    public bool IsGliding => isGliding;
    public float GliderTimer => gliderTimer;

    public bool IsMagnetActive => isMagnetActive;
    public float MagnetTimer => magnetTimer;

    public void ActivateMagnet(float duration = -1f)
    {
        isMagnetActive = true;
        magnetTimer = duration > 0f ? duration : magnetDuration;
        rightUpperArmBone = animator != null && animator.avatar.isHuman
            ? animator.GetBoneTransform(HumanBodyBones.RightUpperArm)
            : null;
        ShowMagnetInHand();
    }

    private void LateUpdate()
    {
        // Lift the right arm after the Animator runs so the magnet looks held up
        if (isMagnetActive && rightUpperArmBone != null)
        {
            rightUpperArmBone.localRotation *= Quaternion.Euler(0f, 0f, magnetArmRaiseAngle);
        }
    }

    /// <summary>
    /// Attaches the magnet model to the runner's right hand bone so it appears
    /// held up while the magnet powerup is active (Subway Surfers style).
    /// </summary>
    private void ShowMagnetInHand()
    {
        HideMagnetInHand();
        if (magnetPrefab == null || animator == null || !animator.avatar.isHuman) return;

        Transform hand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        if (hand == null) return;

        activeMagnetInstance = Instantiate(magnetPrefab, hand);
        // Place the magnet just past the fingers along the forearm->hand direction
        Transform forearm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        Vector3 dir = forearm != null
            ? (hand.position - forearm.position).normalized
            : hand.forward;
        activeMagnetInstance.transform.position = hand.position + dir * 0.07f + magnetAttachOffset;
        activeMagnetInstance.transform.localRotation = Quaternion.Euler(magnetAttachRotation);
        activeMagnetInstance.transform.localScale = magnetAttachScale;

        // Strip pickup behaviour/collider so the held magnet is purely visual
        foreach (var mono in activeMagnetInstance.GetComponentsInChildren<MonoBehaviour>(true))
        {
            Destroy(mono);
        }
        foreach (var col in activeMagnetInstance.GetComponentsInChildren<Collider>(true))
        {
            Destroy(col);
        }
    }

    private void HideMagnetInHand()
    {
        if (activeMagnetInstance != null)
        {
            Destroy(activeMagnetInstance);
            activeMagnetInstance = null;
        }
    }

    public float SkateTimer => skateTimer;

    public float GetCurrentDistance()
    {
        if (isMoving)
        {
            float dist = Mathf.Max(0f, transform.position.z - initialZ);
            if (dist > maxDistanceTraveled) maxDistanceTraveled = dist;
            return maxDistanceTraveled;
        }
        return maxDistanceTraveled;
    }

    public void SetTutorialJumpOnlyInput(bool jumpOnly)
    {
        jumpOnlyInput = jumpOnly;
    }

    public void SetTutorialInputLocked(bool locked)
    {
        inputLocked = locked;
    }

    public void SetTutorialSkateboardTapMode(bool tapMode)
    {
        skateboardTapMode = tapMode;
        clickCount = 0;
        lastClickTime = -100f;
    }

    public void ActivateSkateboard(float duration)
    {
        if (isSkatingInternal) return;

        isSkatingInternal = true;
        skateTimer = duration;

        if (skateboardObject != null)
        {
            skateboardObject.SetActive(true);
        }

        if (animator != null)
        {
            if (HasParameter(animator, "isSkating")) animator.SetBool("isSkating", true);
        }

        PlayState(skateAnimationName);
        MakeInvulnerable(duration);
    }

    public void EndSkateboarding()
    {
        if (!isSkatingInternal) return;

        isSkatingInternal = false;
        skateTimer = 0f;

        if (skateboardObject != null)
        {
            skateboardObject.SetActive(false);
            skateboardObject.transform.localPosition = skateOriginalLocalPos;
            skateboardObject.transform.localRotation = skateOriginalRot;
        }

        if (animator != null)
        {
            if (HasParameter(animator, "isSkating")) animator.SetBool("isSkating", false);
        }

        PlayState(isMoving ? "run" : "idle");
    }

    public void ActivateHangGlider(float duration = -1f)
    {
        if (isFalling) return;

        isGliding = true;
        isDescendingFromGlider = false;
        gliderTimer = duration > 0f ? duration : gliderDuration;
        isGrounded = false;
        if (animator != null)
        {
            if (HasParameter(animator, "isGrounded")) animator.SetBool("isGrounded", false);
            if (HasParameter(animator, "isGliding")) animator.SetBool("isGliding", true);
            if (HasParameter(animator, "isSliding")) animator.SetBool("isSliding", false);
            if (HasParameter(animator, "isSkating")) animator.SetBool("isSkating", false);
        }

        if (isSkatingInternal)
        {
            EndSkateboarding();
        }

        // Deactivate slide state
        isSliding = false;

        // Spawn or reposition glider visual attached to runner
        if (activeGliderInstance != null)
        {
            Destroy(activeGliderInstance);
            activeGliderInstance = null;
        }

        if (gliderPrefab != null)
        {
            // Attach glider to player transform so it follows player perfectly
            Transform parentAttach = gliderAttachPoint != null ? gliderAttachPoint : transform;
            activeGliderInstance = Instantiate(gliderPrefab, parentAttach);
            activeGliderInstance.transform.localPosition = gliderAttachOffset;
            activeGliderInstance.transform.localRotation = Quaternion.Euler(gliderAttachRotation);
            activeGliderInstance.transform.localScale = gliderAttachScale;

            // Remove any pickup collider or rotation script from the held glider instance
            var pickupScript = activeGliderInstance.GetComponent<HangGliderPowerup>();
            if (pickupScript != null) Destroy(pickupScript);
            var rotScript = activeGliderInstance.GetComponent<CoinRotation>();
            if (rotScript != null) Destroy(rotScript);
            var col = activeGliderInstance.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }

        PlayState("glider");
        MakeInvulnerable(gliderTimer + 2f);

        // Airborne - the monkey thief can't reach us; call off the chase
        if (MonkeyThiefController.Instance != null)
        {
            MonkeyThiefController.Instance.ResetChase();
        }
    }

    public int LaneChangeCount
    {
        get; private set;
    }

    // Gesture tracking variables
    private Vector2 touchStartPos;
    private bool swipeDetected = false;

    private void Awake()
    {
        Instance = this;
        animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            // Movement is fully driven by this script (transform.position), not by animation
            // curves. If root motion is left on, any rotation/position baked into a clip
            // (e.g. a slight twist in the jump animation used for the lane-change hop) gets
            // applied directly to the transform, causing the cat to visibly rotate/drift.
            animator.applyRootMotion = false;
        }
        overlapResults = new Collider[maxOverlapResults];
        gameObject.tag = "Player";
        initialZ = roadStartPosition.z;
        maxDistanceTraveled = 0f;

        if (skateboardObject != null)
        {
            skateOriginalLocalPos = skateboardObject.transform.localPosition;
            skateOriginalRot = skateboardObject.transform.localRotation;
            skateboardObject.SetActive(false);
        }

        if (activeGliderInstance != null)
        {
            Destroy(activeGliderInstance);
            activeGliderInstance = null;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (activeGliderInstance != null)
        {
            Destroy(activeGliderInstance);
            activeGliderInstance = null;
        }
    }

    private void OnDisable()
    {
        if (activeGliderInstance != null)
        {
            Destroy(activeGliderInstance);
            activeGliderInstance = null;
        }
    }

    private void OnEnable()
    {
        if (isMoving && !isFalling)
        {
            RestoreMovementAnimation();
        }
    }

    public void ResumePlayer()
    {
        if (isMoving && !isFalling)
        {
            RestoreMovementAnimation();
        }
    }

    public void RestoreMovementAnimation()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator == null) return;

        if (isFalling)
        {
            PlayState(failAnimationName);
        }
        else if (isGliding)
        {
            if (HasParameter(animator, "isGrounded")) animator.SetBool("isGrounded", false);
            PlayState("glider");
        }
        else if (isSkatingInternal)
        {
            if (HasParameter(animator, "isSkating")) animator.SetBool("isSkating", true);
            PlayState(skateAnimationName);
        }
        else if (isSliding)
        {
            if (HasParameter(animator, "isSliding")) animator.SetBool("isSliding", true);
            PlayState("slide");
        }
        else if (!isGrounded)
        {
            if (HasParameter(animator, "isGrounded")) animator.SetBool("isGrounded", false);
            PlayState("jump");
        }
        else if (isMoving)
        {
            if (HasParameter(animator, "isGrounded")) animator.SetBool("isGrounded", true);
            if (HasParameter(animator, "isSliding")) animator.SetBool("isSliding", false);
            if (HasParameter(animator, "isSkating")) animator.SetBool("isSkating", false);
            PlayState("run");
        }
        else if (isInRoadsideIdlePose)
        {
            PlayState(HasAnimatorState(roadsideIdleAnimationName) ? roadsideIdleAnimationName : "idle");
        }
        else
        {
            PlayState("idle");
        }
    }

    public void EnsureRunningAnimation()
    {
        if (animator == null) return;
        string desiredState = isSkatingInternal ? skateAnimationName : "run";
        var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsName(desiredState) && !stateInfo.IsName(char.ToUpper(desiredState[0]) + desiredState.Substring(1)))
        {
            PlayState(desiredState);
        }
    }

    private void Start()
    {
        targetX = GetXForLane(currentLane);
        startX = targetX;
        laneChangeTimer = laneChangeDuration;
        yPosition = transform.position.y;

        // Auto-detect if game is already active (e.g. direct play testing)
        if (GameStateController.Instance != null && GameStateController.Instance.IsPlaying())
        {
            isMoving = true;
            initialZ = transform.position.z;
        }
        else
        {
            isMoving = false;
        }

        isInRoadsideIdlePose = !isMoving;

        if (animator != null)
        {
            if (HasParameter(animator, "isGrounded")) animator.SetBool("isGrounded", true);
            if (HasParameter(animator, "isSliding")) animator.SetBool("isSliding", false);
            PlayState(isMoving ? "run" : "idle");
        }
    }

    public void StartRunning()
    {
        isInRoadsideIdlePose = false;
        isFalling = false;
        isMoving = true;
        initialZ = transform.position.z;
        maxDistanceTraveled = 0f;
        PlayState("run");

        if (MonkeyThiefController.Instance != null)
        {
            MonkeyThiefController.Instance.StartChase();
        }
    }

    public void SetRoadsideIdlePose()
    {
        isInRoadsideIdlePose = true;
        isFalling = false;
        maxDistanceTraveled = 0f;
        StopMotionForIntro();
        transform.SetPositionAndRotation(roadsideIdlePosition, Quaternion.Euler(roadsideIdleRotation));
        PlayState(HasAnimatorState(roadsideIdleAnimationName) ? roadsideIdleAnimationName : "idle");

        if (MonkeyThiefController.Instance != null)
        {
            MonkeyThiefController.Instance.ResetChase();
        }
    }

    public IEnumerator BeginRoadsideRun()
    {
        isInRoadsideIdlePose = false;
        isFalling = false;
        StopMotionForIntro();
        PlayState(standUpAnimationName);
        yield return new WaitForSeconds(standUpDuration);

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(roadStartRotation);
        PlayState("jump");

        float elapsed = 0f;
        while (elapsed < introJumpDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / introJumpDuration);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            Vector3 position = Vector3.Lerp(startPosition, roadStartPosition, easedProgress);
            position.y += 4f * introJumpHeight * progress * (1f - progress);
            transform.SetPositionAndRotation(position, Quaternion.Slerp(startRotation, targetRotation, easedProgress));
            yield return null;
        }

        transform.SetPositionAndRotation(roadStartPosition, targetRotation);
        yPosition = roadStartPosition.y;
        isGrounded = true;
        if (animator != null && HasParameter(animator, "isGrounded")) animator.SetBool("isGrounded", true);
        StartRunning();
    }

    private void StopMotionForIntro()
    {
        isMoving = false;
        isFalling = false;
        isSliding = false;
        verticalVelocity = 0f;
        isGrounded = true;
        yPosition = transform.position.y;
        targetX = roadStartPosition.x;
        startX = targetX;
        currentLane = 1;
        laneChangeTimer = laneChangeDuration;
        if (animator != null)
        {
            if (HasParameter(animator, "isGrounded")) animator.SetBool("isGrounded", true);
            if (HasParameter(animator, "isSliding")) animator.SetBool("isSliding", false);
        }
    }

    private void Update()
    {
        if (!isMoving) return;

        HandleInputs();

        // Slide Timer
        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0f)
            {
                isSliding = false;
                if (animator != null)
                {
                    if (HasParameter(animator, "isSliding")) animator.SetBool("isSliding", false);
                }
                PlayState("run");
            }
        }

        // Skateboard Timer
        if (isSkatingInternal)
        {
            skateTimer -= Time.deltaTime;
            if (skateTimer <= 0f)
            {
                EndSkateboarding();
            }
        }

        // Magnet Timer & Coin Attraction
        if (isMagnetActive)
        {
            magnetTimer -= Time.deltaTime;
            if (magnetTimer <= 0f)
            {
                isMagnetActive = false;
                magnetTimer = 0f;
                HideMagnetInHand();
            }
            else
            {
                // Find all nearby coins and attract them towards player without GC allocation
                int magnetCount = Physics.OverlapSphereNonAlloc(transform.position + Vector3.up * 0.8f, magnetRadius, magnetHits);
                for (int i = 0; i < magnetCount; i++)
                {
                    if (magnetHits[i] == null) continue;
                    Coin nearbyCoin = magnetHits[i].GetComponentInParent<Coin>();
                    if (nearbyCoin != null)
                    {
                        nearbyCoin.AttractTo(transform);
                    }
                }
            }
        }

        // Glider Timer
        if (isGliding && !isDescendingFromGlider)
        {
            gliderTimer -= Time.deltaTime;
            if (gliderTimer <= 0f)
            {
                isDescendingFromGlider = true;
            }
        }

        // Gravity & Vertical Movement / Gliding
        if (isGliding)
        {
            if (!isDescendingFromGlider)
            {
                yPosition = Mathf.MoveTowards(yPosition, gliderHeight, gliderAscentSpeed * Time.deltaTime);
            }
            else
            {
                yPosition = Mathf.MoveTowards(yPosition, 0f, gliderDescentSpeed * Time.deltaTime);
                if (yPosition <= 0f)
                {
                    yPosition = 0f;
                    isGliding = false;
                    isDescendingFromGlider = false;
                    isGrounded = true;
                    if (animator != null)
                    {
                        if (HasParameter(animator, "isGliding")) animator.SetBool("isGliding", false);
                        if (HasParameter(animator, "isGrounded")) animator.SetBool("isGrounded", true);
                    }
                    PlayState("run");

                    // Destroy glider visual when cat touches the ground
                    if (activeGliderInstance != null)
                    {
                        Destroy(activeGliderInstance);
                        activeGliderInstance = null;
                    }
                }
            }
        }
        else if (!isGrounded)
        {
            verticalVelocity -= gravity * Time.deltaTime;
            yPosition += verticalVelocity * Time.deltaTime;

            if (yPosition <= 0f)
            {
                yPosition = 0f;
                verticalVelocity = 0f;
                isGrounded = true;
                if (animator != null)
                {
                    if (HasParameter(animator, "isGrounded")) animator.SetBool("isGrounded", true);
                }
                PlayState("run");
            }
        }

        // Ensure running / skating animation is actively playing when grounded on the move
        if (isGrounded && !isSliding && !isGliding && !isFalling)
        {
            EnsureRunningAnimation();
        }

        // Interpolate Lane Position smoothly over duration
        float newX;
        if (laneChangeTimer < laneChangeDuration)
        {
            laneChangeTimer += Time.deltaTime;
            float t = Mathf.Clamp01(laneChangeTimer / laneChangeDuration);
            // Use SmoothStep for a beautiful ease-in-ease-out curve
            t = Mathf.SmoothStep(0f, 1f, t);
            newX = Mathf.Lerp(startX, targetX, t);
        }
        else
        {
            newX = targetX;
        }

        // Move Forward
        float newZ = transform.position.z + forwardSpeed * Time.deltaTime;

        transform.position = new Vector3(newX, yPosition, newZ);

        float currentDist = Mathf.Max(0f, transform.position.z - initialZ);
        if (currentDist > maxDistanceTraveled)
        {
            maxDistanceTraveled = currentDist;
        }

        // Keep glider visual aligned if unparented, or maintain local offsets
        if (activeGliderInstance != null && isGliding && activeGliderInstance.transform.parent == null)
        {
            activeGliderInstance.transform.position = new Vector3(newX + gliderAttachOffset.x, yPosition + gliderAttachOffset.y, newZ + gliderAttachOffset.z);
            activeGliderInstance.transform.rotation = Quaternion.Euler(gliderAttachRotation);
        }

        collisionCheckFrame++;
        if (collisionCheckFrame >= collisionCheckInterval)
        {
            collisionCheckFrame = 0;
            CheckCollisions();
        }
    }

    private void HandleInputs()
    {
        // Keyboard shortcut 'E' or quick double-tap activates the skateboard (like Subway Surfers)
        if (enableDoubleTapSkateboard && !isSkatingInternal && Input.GetKeyDown(KeyCode.E))
        {
            ActivateSkateboard(skateboardDuration);
            return;
        }

        bool tapBegan = Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);

        if (tapBegan && (enableDoubleTapSkateboard || enableTripleTapSkateboard || skateboardTapMode))
        {
            float timeSinceLastClick = Time.unscaledTime - lastClickTime;
            float threshold = skateboardTapMode ? tutorialMultiTapThreshold : multiTapThreshold;

            if (timeSinceLastClick <= threshold)
                clickCount++;
            else
                clickCount = 1;

            lastClickTime = Time.unscaledTime;

            int neededTaps = skateboardTapMode ? 3 : (enableDoubleTapSkateboard ? tapsToActivateSkateboard : 3);
            if (clickCount >= neededTaps)
            {
                clickCount = 0;
                if (!isSkatingInternal)
                {
                    ActivateSkateboard(skateboardDuration);
                    return;
                }
            }
        }

        // During the tutorial skate step, ignore other input until the skateboard is activated.
        if (skateboardTapMode && !isSkatingInternal) return;

        if (inputLocked) return;

        // 1. Keyboard fallback controls
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            if (!jumpOnlyInput) MoveLane(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            if (!jumpOnlyInput) MoveLane(1);
        }
        else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            Jump();
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (!jumpOnlyInput) Slide();
        }

        // 2. Mobile Touch Swipes
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = touch.position;
                swipeDetected = false;
            }
            else if (touch.phase == TouchPhase.Moved && !swipeDetected)
            {
                Vector2 currentPos = touch.position;
                Vector2 direction = currentPos - touchStartPos;

                if (direction.magnitude >= minSwipeDistance)
                {
                    swipeDetected = true;
                    ProcessSwipe(direction);
                }
            }
        }

        // 3. Editor Mouse Drag Swipes
        if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = Input.mousePosition;
            swipeDetected = false;
        }
        else if (Input.GetMouseButton(0) && !swipeDetected)
        {
            Vector2 currentPos = Input.mousePosition;
            Vector2 direction = currentPos - touchStartPos;

            if (direction.magnitude >= minSwipeDistance)
            {
                swipeDetected = true;
                ProcessSwipe(direction);
            }
        }
    }

    private void ProcessSwipe(Vector2 direction)
    {
        direction.Normalize();

        // During the jump-only tutorial, ignore any swipe that isn't clearly upward.
        if (jumpOnlyInput)
        {
            if (direction.y <= 1f / Mathf.Sqrt(2f)) return;
            Jump();
            return;
        }

        // Check horizontal vs vertical swipe
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Horizontal swipe
            if (direction.x > 0f)
            {
                MoveLane(1); // Swipe Right
            }
            else
            {
                MoveLane(-1); // Swipe Left
            }
        }
        else
        {
            // Vertical swipe
            if (direction.y > 0f)
            {
                Jump(); // Swipe Up
            }
            else
            {
                Slide(); // Swipe Down
            }
        }
    }

    private void MoveLane(int direction)
    {
        int nextLane = currentLane + direction;
        if (nextLane >= 0 && nextLane <= 2)
        {
            currentLane = nextLane;
            startX = transform.position.x;
            targetX = GetXForLane(currentLane);
            laneChangeTimer = 0f;
            LaneChangeCount++;

            if (MonkeyThiefController.Instance != null)
            {
                MonkeyThiefController.Instance.OnPlayerLaneChanged(currentLane);
            }

            // Add hop/bounce when switching lanes while grounded
            if (isGrounded && !isSliding)
            {
                verticalVelocity = laneSwitchHopForce;
                isGrounded = false;
                if (animator != null)
                {
                    if (HasParameter(animator, "isGrounded")) animator.SetBool("isGrounded", false);
                }
                PlayState("jump");
            }
        }
    }

    private void Jump()
    {
        if (isGliding) return;

        if (isGrounded && !isSliding)
        {
            verticalVelocity = jumpForce;
            isGrounded = false;
            if (animator != null)
            {
                if (HasParameter(animator, "isGrounded")) animator.SetBool("isGrounded", false);
            }
            PlayState("jump");
        }
    }

    private void Slide()
    {
        if (isGliding) return;

        if (isGrounded && !isSliding)
        {
            isSliding = true;
            slideTimer = slideDuration;
            if (animator != null)
            {
                if (HasParameter(animator, "isSliding")) animator.SetBool("isSliding", true);
            }
            PlayState("slide");
        }
    }

    private void CheckCollisions()
    {
        if (!isMoving) return;

        Vector3 center = transform.position + hitCenterOffset;
        int count = Physics.OverlapSphereNonAlloc(center, hitRadius, overlapResults, -1, QueryTriggerInteraction.Collide);

        for (int i = 0; i < count; i++)
        {
            Collider col = overlapResults[i];
            if (col == null) continue;

            // Ignore the player's own colliders / body parts
            if (col.transform == transform || col.GetComponentInParent<SimplePlayerController>() != null)
                continue;

            // Coin - try to collect it
            Coin coin = col.GetComponentInParent<Coin>();
            if (coin != null)
            {
                coin.Collect();
                continue;
            }

            // Gem - try to collect it
            Gem gem = col.GetComponentInParent<Gem>();
            if (gem != null)
            {
                gem.Collect();
                continue;
            }

            // Skateboard pickup - collect and activate
            SkateboardPickup skatePickup = col.GetComponentInParent<SkateboardPickup>();
            if (skatePickup != null)
            {
                skatePickup.Collect(this);
                continue;
            }

            // Word letter pickup - feed it to WordManager so the HUD slots fill
            LetterPickup letterPickup = col.GetComponentInParent<LetterPickup>();
            if (letterPickup != null)
            {
                if (WordManager.Instance != null)
                    WordManager.Instance.CollectLetter(letterPickup.letter);

                if (SoundEffectsManager.Instance != null)
                {
                    SoundEffectsManager.Instance.PlayLetterCollect();
                    SoundEffectsManager.Instance.SpawnLetterCollectEffect(letterPickup.transform.position);
                }

                Destroy(letterPickup.gameObject);
                continue;
            }

            // Hang Glider powerup - collect it
            HangGliderPowerup gliderPickup = col.GetComponentInParent<HangGliderPowerup>();
            if (gliderPickup != null)
            {
                if (!isGliding)
                {
                    ActivateHangGlider(gliderPickup.duration);
                    if (SoundEffectsManager.Instance != null)
                        SoundEffectsManager.Instance.PlayLetterCollect();
                }
                if (ObjectPoolManager.Instance != null)
                    ObjectPoolManager.Instance.ReturnToPool(gliderPickup.gameObject);
                else
                    Destroy(gliderPickup.gameObject);
                continue;
            }

            // Coin Magnet powerup - collect it
            MagnetPowerup magnetPickup = col.GetComponentInParent<MagnetPowerup>();
            if (magnetPickup != null)
            {
                ActivateMagnet(magnetPickup.duration);
                if (SoundEffectsManager.Instance != null)
                {
                    SoundEffectsManager.Instance.PlayLetterCollect();
                    SoundEffectsManager.Instance.SpawnCoinCollectEffect(col.transform.position);
                }
                if (ObjectPoolManager.Instance != null)
                    ObjectPoolManager.Instance.ReturnToPool(magnetPickup.gameObject);
                else
                    Destroy(magnetPickup.gameObject);
                continue;
            }

            // Obstacle - game over (or skateboard crash)
            if (IsObstacle(col))
            {
                if (isSkatingInternal)
                {
                    TriggerSkateboardCrash(col.gameObject);
                    return;
                }

                if (isInvulnerable)
                    continue;

                // Ignore trigger volumes unless they belong to an active vehicle
                if (col.isTrigger && col.GetComponentInParent<VehicleMover>() == null && !col.CompareTag("Obstacle"))
                {
                    continue;
                }

                HitObstacle();
                return;
            }
        }
    }

    private bool IsObstacle(Collider col)
    {
        if (col == null) return false;

        // Direct tag match (zero GC allocation)
        if (col.CompareTag("Obstacle")) return true;

        // Parent hierarchy tag match (zero GC allocation)
        if (HasTagInHierarchy(col.transform, "Obstacle")) return true;

        // Root transform tag match
        Transform root = col.transform.root;
        if (root != null && root.CompareTag("Obstacle")) return true;

        // Moving vehicles (BRT, Danfo, Keke, Train, etc.) are always obstacles
        if (col.GetComponentInParent<VehicleMover>() != null) return true;
        if (col.GetComponentInParent<TrainMover>() != null) return true;

        return false;
    }

    private bool HasTagInHierarchy(Transform t, string tag)
    {
        while (t != null)
        {
            if (t.CompareTag(tag)) return true;
            t = t.parent;
        }
        return false;
    }

    private void HitObstacle()
    {
        if (MonkeyThiefController.Instance != null)
        {
            MonkeyThiefController.Instance.TriggerBusted();
        }

        if (SoundEffectsManager.Instance != null)
        {
            SoundEffectsManager.Instance.PlayObstacleHit();
        }

        // Always trigger the fall animation and backward knockback arc immediately!
        TriggerFall();

        if (UImanager.uimanager != null)
        {
            UImanager.uimanager.Fail();
        }
        else
        {
            if (GameStateController.Instance != null)
                GameStateController.Instance.GameOver("Hit obstacle");
            else
                Time.timeScale = 0f;
        }
    }

    public void TriggerFall()
    {
        if (isFalling) return;
        isFalling = true;
        isMoving = false;

        if (animator != null && !string.IsNullOrEmpty(failAnimationName))
        {
            animator.Play(failAnimationName, 0, 0f);
        }

        if (fallCoroutine != null) StopCoroutine(fallCoroutine);
        fallCoroutine = StartCoroutine(FallKnockbackCoroutine());
    }

    /// <summary>
    /// Revive the runner after the player pays to continue (Subway Surfers "Save Me").
    /// Clears the fallen state, resumes running in place, and grants brief invulnerability.
    /// </summary>
    public void ReviveAfterContinue(float invulnerabilitySeconds = 3f)
    {
        if (fallCoroutine != null)
        {
            StopCoroutine(fallCoroutine);
            fallCoroutine = null;
        }

        isFalling = false;
        isMoving = true;
        isSliding = false;
        isGrounded = true;
        verticalVelocity = 0f;

        Vector3 pos = transform.position;
        pos.y = 0f;
        transform.position = pos;
        yPosition = 0f;

        if (animator != null && HasParameter(animator, "isGrounded")) animator.SetBool("isGrounded", true);
        PlayState(isSkatingInternal ? skateAnimationName : "run");

        if (MonkeyThiefController.Instance != null)
        {
            MonkeyThiefController.Instance.ResetChase();
        }

        MakeInvulnerable(invulnerabilitySeconds);
    }

    private IEnumerator FallKnockbackCoroutine()
    {
        Vector3 startPos = transform.position;
        Vector3 knockbackTarget = startPos + Vector3.back * fallKnockbackDistance;

        float elapsed = 0f;
        while (elapsed < fallKnockbackDuration)
        {
            elapsed += Time.deltaTime;
            float p = Mathf.Clamp01(elapsed / fallKnockbackDuration);

            Vector3 pos = Vector3.Lerp(startPos, knockbackTarget, p);
            pos.y = Mathf.Sin(p * Mathf.PI) * fallKnockbackHeight;
            if (pos.y < 0f) pos.y = 0f;

            transform.position = pos;
            yield return null;
        }

        transform.position = new Vector3(knockbackTarget.x, 0f, knockbackTarget.z);
        fallCoroutine = null;
    }

    // -------------------------------------------------------------------------
    // Animation / skateboard helpers (mirrored from PlayerRunnerController)
    // -------------------------------------------------------------------------

    public void PlayState(string stateName)
    {
        if (isGliding && (stateName == "run" || stateName == "jump" || stateName == "slide" || stateName == "skating"))
        {
            stateName = "glider";
        }
        else if (isSkatingInternal && stateName == "run")
        {
            stateName = skateAnimationName;
        }

        if (animator == null || string.IsNullOrEmpty(stateName))
        {
#if UNITY_EDITOR
            Debug.LogWarning($"[SimplePlayerController] PlayState failed - animator={animator != null}, stateName={stateName}");
#endif
            return;
        }

        int stateHash = Animator.StringToHash(stateName);
        if (animator.HasState(0, stateHash))
        {
            var current = animator.GetCurrentAnimatorStateInfo(0);
            if (!current.IsName(stateName))
            {
                animator.Play(stateName, 0, 0f);
            }
        }
        else
        {
            string capitalizedName = char.ToUpper(stateName[0]) + stateName.Substring(1);
            int capHash = Animator.StringToHash(capitalizedName);
            if (animator.HasState(0, capHash))
            {
                var current = animator.GetCurrentAnimatorStateInfo(0);
                if (!current.IsName(capitalizedName))
                {
                    animator.Play(capitalizedName, 0, 0f);
                }
            }
            else
            {
#if UNITY_EDITOR
                if (stateName != "idle")
                    Debug.LogError($"[SimplePlayerController] Animation state '{stateName}' or '{capitalizedName}' NOT FOUND in animator!");
#endif
            }
        }
    }

    public static bool HasParameter(Animator anim, string paramName)
    {
        if (anim == null || anim.parameters == null) return false;
        foreach (var p in anim.parameters)
        {
            if (p.name == paramName) return true;
        }
        return false;
    }

    private bool HasAnimatorState(string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName)) return false;
        return animator.HasState(0, Animator.StringToHash(stateName)) ||
               animator.HasState(0, Animator.StringToHash(char.ToUpper(stateName[0]) + stateName.Substring(1)));
    }

    public void MakeInvulnerable(float duration)
    {
        if (invulnerabilityCoroutine != null) StopCoroutine(invulnerabilityCoroutine);
        invulnerabilityCoroutine = StartCoroutine(InvulnerableRoutine(duration));
    }

    private IEnumerator InvulnerableRoutine(float duration)
    {
        isInvulnerable = true;
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
        invulnerabilityCoroutine = null;
    }

    private void TriggerSkateboardCrash(GameObject obstacle)
    {
        Debug.Log($"[SimplePlayerController] Skateboard crashed into {obstacle.name}! Destroying skateboard and continuing on foot.");

        isSkatingInternal = false;
        skateTimer = 0f;

        if (skateboardObject != null)
        {
            skateboardObject.SetActive(false);
            skateboardObject.transform.localPosition = skateOriginalLocalPos;
            skateboardObject.transform.localRotation = skateOriginalRot;
        }

        if (animator != null)
        {
            if (HasParameter(animator, "isSkating")) animator.SetBool("isSkating", false);
        }

        PlayState("run");

        if (obstacle != null)
        {
            obstacle.SetActive(false);
        }

        if (SoundEffectsManager.Instance != null)
        {
            SoundEffectsManager.Instance.PlayObstacleHit();
        }

        MakeInvulnerable(2f);
    }

    private float GetXForLane(int lane)
    {
        if (lane == 0) return -laneDistance;
        if (lane == 2) return laneDistance;
        return 0f;
    }
}

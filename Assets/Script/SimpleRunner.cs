using UnityEngine;

public class SimpleRunner : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardSpeed = 10f;
    public float laneDistance = 2.5f;
    public float laneChangeSpeed = 12f;

    [Header("Jump & Physics Settings")]
    public float jumpForce = 8.5f;
    public float gravity = 22f;

    [Header("Slide Settings")]
    public float slideDuration = 0.8f;

    [Header("Gesture Detection")]
    [Tooltip("Minimum drag distance in pixels to register as a swipe gesture.")]
    public float minSwipeDistance = 45f;

    private Animator animator;
    private int currentLane = 1; // 0 = Left, 1 = Center, 2 = Right
    private float targetX = 0f;
    private float verticalVelocity = 0f;
    private float yPosition = 0f;
    private bool isGrounded = true;
    private bool isSliding = false;
    private float slideTimer = 0f;

    // Gesture tracking variables
    private Vector2 touchStartPos;
    private Vector2 touchEndPos;
    private bool swipeDetected = false;

    private void Awake()
    {
        // Dynamically destroy old script components if they exist to prevent interference
        string[] componentNames = { "PlayerCollector", "EyeBlinkController" };
        foreach (var name in componentNames)
        {
            Component comp = GetComponent(name);
            if (comp != null)
            {
                Debug.Log($"[SimpleRunner] Removing old component: {name}");
                DestroyImmediate(comp);
            }
        }

        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        targetX = GetXForLane(currentLane);
        yPosition = transform.position.y;

        if (animator != null)
        {
            animator.SetBool("isGrounded", true);
            animator.SetBool("isSliding", false);
            animator.Play("run", 0, 0f);
        }
    }

    private void Update()
    {
        // 1. Handle Gesture Inputs (Touch and Mouse)
        HandleSwipeInputs();

        // Handle Slide Timer
        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0f)
            {
                isSliding = false;
                if (animator != null)
                {
                    animator.SetBool("isSliding", false);
                    animator.Play("run", 0, 0f);
                }
            }
        }

        // 2. Apply Gravity & Vertical Movement
        if (!isGrounded)
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
                    animator.SetBool("isGrounded", true);
                    animator.Play("run", 0, 0f);
                }
            }
        }

        // 3. Update Position
        // Interpolate X (lane changing)
        float newX = Mathf.Lerp(transform.position.x, targetX, Time.deltaTime * laneChangeSpeed);

        // Move Forward (Z movement)
        float newZ = transform.position.z + forwardSpeed * Time.deltaTime;

        transform.position = new Vector3(newX, yPosition, newZ);
    }

    private void HandleSwipeInputs()
    {
        // --- 1. KEYBOARD FALLBACKS FOR EASY EDITOR TESTING ---
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            OnSwipeLeft();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            OnSwipeRight();
        }
        else if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            OnSwipeUp();
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            OnSwipeDown();
        }

        // --- 2. MOBILE TOUCH SWIPES ---
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

        // --- 3. EDITOR MOUSE DRAG SWIPES ---
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

        // Check horizontal vs vertical swipe
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Horizontal swipe
            if (direction.x > 0f)
            {
                OnSwipeRight();
            }
            else
            {
                OnSwipeLeft();
            }
        }
        else
        {
            // Vertical swipe
            if (direction.y > 0f)
            {
                OnSwipeUp();
            }
            else
            {
                OnSwipeDown();
            }
        }
    }

    private void OnSwipeLeft()
    {
        if (currentLane > 0)
        {
            currentLane--;
            targetX = GetXForLane(currentLane);
        }
    }

    private void OnSwipeRight()
    {
        if (currentLane < 2)
        {
            currentLane++;
            targetX = GetXForLane(currentLane);
        }
    }

    private void OnSwipeUp()
    {
        if (isGrounded && !isSliding)
        {
            verticalVelocity = jumpForce;
            isGrounded = false;
            if (animator != null)
            {
                animator.SetBool("isGrounded", false);
                animator.Play("jump", 0, 0f);
            }
        }
    }

    private void OnSwipeDown()
    {
        if (isGrounded && !isSliding)
        {
            isSliding = true;
            slideTimer = slideDuration;
            if (animator != null)
            {
                animator.SetBool("isSliding", true);
                animator.Play("slide", 0, 0f);
            }
        }
    }

    private float GetXForLane(int lane)
    {
        if (lane == 0) return -laneDistance;
        if (lane == 2) return laneDistance;
        return 0f;
    }
}


using UnityEngine;

namespace SynthwaveRunner
{
    public class NeonRunnerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        public float laneDistance = 2.5f;
        public float laneSwitchSpeed = 15f;
        
        [Header("Jump Settings")]
        public float jumpDuration = 0.6f;
        public float jumpHeight = 3.5f;
        
        [Header("Slide Settings")]
        public float slideDuration = 0.6f;
        public float slideHeightMultiplier = 0.4f;

        [Header("Visual Components")]
        public Transform modelTransform; // The child visual model to scale/rotate
        public BoxCollider playerCollider; // Collider to adjust during slide

        private int targetLane = 0; // -1 = Left, 0 = Center, 1 = Right
        private Vector3 targetPosition;
        
        // Jump variables
        private bool isJumping = false;
        private float jumpTimer = 0f;
        private float initialY = 0f;

        // Slide variables
        private bool isSliding = false;
        private float slideTimer = 0f;
        private Vector3 originalModelScale;
        private float originalColliderHeight;
        private Vector3 originalColliderCenter;

        // Swipe detection variables
        private Vector2 touchStartPos;
        private bool isSwipeRegistered = false;
        private readonly float minSwipeDistance = 30f;

        private void Start()
        {
            initialY = transform.position.y;
            targetPosition = transform.position;

            if (modelTransform != null)
            {
                originalModelScale = modelTransform.localScale;
            }
            if (playerCollider != null)
            {
                originalColliderHeight = playerCollider.size.y;
                originalColliderCenter = playerCollider.center;
            }

            // Listen to game manager state changes to reset player
            if (SynthwaveGameManager.Instance != null)
            {
                SynthwaveGameManager.Instance.onStateChanged += OnGameStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (SynthwaveGameManager.Instance != null)
            {
                SynthwaveGameManager.Instance.onStateChanged -= OnGameStateChanged;
            }
        }

        private void OnGameStateChanged(SynthwaveGameManager.GameState state)
        {
            if (state == SynthwaveGameManager.GameState.Playing)
            {
                ResetPlayer();
            }
        }

        public void ResetPlayer()
        {
            targetLane = 0;
            transform.position = new Vector3(0, initialY, 0);
            targetPosition = transform.position;
            
            // Cancel jump/slide states
            isJumping = false;
            jumpTimer = 0f;
            isSliding = false;
            slideTimer = 0f;

            if (modelTransform != null)
            {
                modelTransform.localScale = originalModelScale;
                modelTransform.localRotation = Quaternion.identity;
            }
            if (playerCollider != null)
            {
                playerCollider.size = new Vector3(playerCollider.size.x, originalColliderHeight, playerCollider.size.z);
                playerCollider.center = originalColliderCenter;
            }
        }

        private void Update()
        {
            // Only accept inputs and move when game is playing
            if (SynthwaveGameManager.Instance == null || SynthwaveGameManager.Instance.currentState != SynthwaveGameManager.GameState.Playing)
            {
                return;
            }

            HandleInput();
            ExecuteMovement();
        }

        private void HandleInput()
        {
            // 1. Keyboard Controls (Responsive for PC/Editor)
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                MoveLane(-1);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                MoveLane(1);
            }

            if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)) && !isJumping && !isSliding)
            {
                StartJump();
            }
            else if ((Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) && !isSliding && !isJumping)
            {
                StartSlide();
            }

            // 2. Touch / Mobile Swipe Controls
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    touchStartPos = touch.position;
                    isSwipeRegistered = false;
                }
                else if (touch.phase == TouchPhase.Moved && !isSwipeRegistered)
                {
                    Vector2 delta = touch.position - touchStartPos;
                    if (delta.magnitude > minSwipeDistance)
                    {
                        isSwipeRegistered = true;
                        
                        // Determine swipe axis
                        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                        {
                            // Horizontal Swipe
                            if (delta.x > 0) MoveLane(1);
                            else MoveLane(-1);
                        }
                        else
                        {
                            // Vertical Swipe
                            if (delta.y > 0 && !isJumping && !isSliding) StartJump();
                            else if (delta.y < 0 && !isSliding && !isJumping) StartSlide();
                        }
                    }
                }
            }
        }

        private void MoveLane(int direction)
        {
            targetLane = Mathf.Clamp(targetLane + direction, -1, 1);
        }

        private void StartJump()
        {
            isJumping = true;
            jumpTimer = 0f;
        }

        private void StartSlide()
        {
            isSliding = true;
            slideTimer = 0f;

            // Apply visual and collision scale changes
            if (modelTransform != null)
            {
                modelTransform.localScale = new Vector3(originalModelScale.x, originalModelScale.y * slideHeightMultiplier, originalModelScale.z);
            }
            if (playerCollider != null)
            {
                playerCollider.size = new Vector3(playerCollider.size.x, originalColliderHeight * slideHeightMultiplier, playerCollider.size.z);
                playerCollider.center = new Vector3(originalColliderCenter.x, originalColliderCenter.y * slideHeightMultiplier, originalColliderCenter.z);
            }
        }

        private void ExecuteMovement()
        {
            // Calculate target position based on lane and initial vertical position
            float targetX = targetLane * laneDistance;
            float targetY = initialY;

            // Handle parabolic jump
            if (isJumping)
            {
                jumpTimer += Time.deltaTime;
                float progress = jumpTimer / jumpDuration;
                if (progress >= 1.0f)
                {
                    isJumping = false;
                    progress = 1.0f;
                }

                // Parabolic trajectory formula: y = 4 * height * progress * (1 - progress)
                targetY += 4f * jumpHeight * progress * (1f - progress);
            }

            // Handle sliding timeout
            if (isSliding)
            {
                slideTimer += Time.deltaTime;
                if (slideTimer >= slideDuration)
                {
                    isSliding = false;
                    
                    // Restore original scales
                    if (modelTransform != null)
                    {
                        modelTransform.localScale = originalModelScale;
                    }
                    if (playerCollider != null)
                    {
                        playerCollider.size = new Vector3(playerCollider.size.x, originalColliderHeight, playerCollider.size.z);
                        playerCollider.center = originalColliderCenter;
                    }
                }
            }

            // Update target coordinates
            targetPosition.x = targetX;
            targetPosition.y = targetY;

            // Smoothly move the player X towards the lane
            Vector3 currentPos = transform.position;
            currentPos.x = Mathf.MoveTowards(currentPos.x, targetPosition.x, laneSwitchSpeed * Time.deltaTime);
            currentPos.y = targetPosition.y; // Y follows jump directly for clean response
            transform.position = currentPos;

            // Add a subtle banking rotation when moving lanes
            if (modelTransform != null)
            {
                float xDiff = targetPosition.x - transform.position.x;
                float targetZRotation = -xDiff * 8f; // Lean left/right depending on movement
                Quaternion targetRot = Quaternion.Euler(0, 0, targetZRotation);
                modelTransform.localRotation = Quaternion.Slerp(modelTransform.localRotation, targetRot, 15f * Time.deltaTime);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("DataBit"))
            {
                // Collect DataBit
                if (SynthwaveGameManager.Instance != null)
                {
                    SynthwaveGameManager.Instance.CollectDataBit();
                }

                // Disable bit and return it to its pool automatically via the GridManager (or simply deactivate it)
                other.gameObject.SetActive(false);
            }
            else if (other.CompareTag("Obstacle"))
            {
                // Hit obstacle - Game Over!
                if (SynthwaveGameManager.Instance != null)
                {
                    SynthwaveGameManager.Instance.TriggerGameOver();
                }
            }
        }
    }
}

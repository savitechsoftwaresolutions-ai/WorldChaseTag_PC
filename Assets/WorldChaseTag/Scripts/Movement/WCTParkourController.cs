using System;
using System.Collections;
using UnityEngine;
using WorldChaseTag.Core;
using WorldChaseTag.Combat;
using WorldChaseTag.Audio;

namespace WorldChaseTag.Movement
{
    [RequireComponent(typeof(CharacterController))]
    public class WCTParkourController : MonoBehaviour
    {
        [Header("Camera & View")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Transform cameraHolder;
        [SerializeField] private float mouseSensitivity = 2.2f;
        [SerializeField] private float minPitch = -85f;
        [SerializeField] private float maxPitch = 85f;
        [SerializeField] private bool invertY = false;

        [Header("Movement Speeds")]
        [SerializeField] private float walkSpeed = 5.2f;
        [SerializeField] private float sprintSpeed = 8.5f;
        [SerializeField] private float crouchSpeed = 2.8f;
        [SerializeField] private float slideInitialSpeed = 10.5f;
        [SerializeField] private float acceleration = 24f;
        [SerializeField] private float deceleration = 20f;
        [SerializeField] private float airControlMultiplier = 0.5f;

        [Header("Jump & Physics")]
        [SerializeField] private float jumpHeight = 1.35f;
        [SerializeField] private float gravity = -24f;
        [SerializeField] private float coyoteTimeDuration = 0.15f;
        [SerializeField] private float jumpBufferDuration = 0.15f;

        [Header("Parkour - Vault & Mantle")]
        [SerializeField] private LayerMask obstacleLayer = ~0;
        [SerializeField] private float maxVaultHeight = 1.25f;
        [SerializeField] private float minVaultHeight = 0.45f;
        [SerializeField] private float vaultDistance = 2.2f;
        [SerializeField] private float vaultDuration = 0.35f;
        [SerializeField] private float maxMantleHeight = 2.2f;
        [SerializeField] private float minMantleHeight = 1.3f;
        [SerializeField] private float mantleDuration = 0.45f;

        [Header("Stamina")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaDrainRate = 22f;
        [SerializeField] private float staminaRegenRate = 26f;
        [SerializeField] private float exhaustionThreshold = 25f;

        [Header("Head Bob & Dynamic FOV")]
        [SerializeField] private bool enableHeadBob = true;
        [SerializeField] private float bobIntensity = 1f;
        [SerializeField] private float baseFov = 75f;
        [SerializeField] private float sprintFov = 86f;
        [SerializeField] private float fovTransitionSpeed = 8f;

        // Runtime states
        private CharacterController controller;
        private Vector3 currentVelocity;
        private Vector3 horizontalVelocity;
        private float verticalVelocity;
        private float cameraPitch;
        private float coyoteTimer;
        private float jumpBufferTimer;
        private float currentStamina;
        private bool isExhausted;
        private bool isCrouching;
        private bool isSliding;
        private bool isVaulting;
        private bool isMantling;
        private float defaultControllerHeight = 1.8f;
        private float crouchControllerHeight = 0.95f;
        private Vector3 defaultCameraLocalPos;
        private float bobTimer = 0f;
        private float stepTimer = 0f;
        private bool wasGroundedLastFrame = true;

        // Role & state lock
        private ChaseRole currentRole = ChaseRole.Spectator;
        public ChaseRole CurrentRole
        {
            get => currentRole;
            set
            {
                currentRole = value;
                var arms = GetComponentInChildren<WorldChaseTag.Visuals.WCTFPPArms>();
                if (arms != null) arms.UpdateRoleVisuals(value);
            }
        }
        public bool ControlsEnabled { get; set; } = true;
        public float StaminaNormalized => currentStamina / maxStamina;
        public bool IsSprinting => Input.GetKey(KeyCode.LeftShift) && horizontalVelocity.magnitude > walkSpeed + 0.5f && !isExhausted;
        public bool IsVaulting => isVaulting;
        public bool IsMantling => isMantling;
        public bool IsCrouching => isCrouching;
        public bool IsSliding => isSliding;
        public Camera PlayerCamera => playerCamera;

        public event Action<string> OnParkourActionTriggered; // "VAULT", "MANTLE", "SLIDE", "TAG_ATTEMPT"

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            defaultControllerHeight = controller.height;

            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
            }

            if (cameraHolder == null && playerCamera != null)
            {
                cameraHolder = playerCamera.transform;
            }

            if (cameraHolder != null)
            {
                defaultCameraLocalPos = cameraHolder.localPosition;
                cameraPitch = cameraHolder.localEulerAngles.x;
                if (cameraPitch > 180f) cameraPitch -= 360f;

                // Ensure First-Person animated parkour arms exist
                if (cameraHolder.GetComponent<WorldChaseTag.Visuals.WCTFPPArms>() == null)
                {
                    cameraHolder.gameObject.AddComponent<WorldChaseTag.Visuals.WCTFPPArms>();
                }
            }

            currentStamina = maxStamina;
        }

        private void Start()
        {
            bool menuOpen = (WorldChaseTag.UI.WCTMenuController.Instance != null && WorldChaseTag.UI.WCTMenuController.Instance.IsMenuOpen);
            if (menuOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                ControlsEnabled = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                ControlsEnabled = true;
            }
        }

        private void Update()
        {
            bool isUIActive = (WorldChaseTag.UI.WCTMenuController.Instance != null && WorldChaseTag.UI.WCTMenuController.Instance.IsMenuOpen) ||
                             (WorldChaseTag.UI.WCTHUDController.Instance != null && WorldChaseTag.UI.WCTHUDController.Instance.IsPaused) ||
                             (WorldChaseTag.Core.WCTMatchController.Instance != null && 
                              (WorldChaseTag.Core.WCTMatchController.Instance.State == WorldChaseTag.Core.WCTMatchState.CHASE_RESULT || 
                               WorldChaseTag.Core.WCTMatchController.Instance.State == WorldChaseTag.Core.WCTMatchState.ROLE_ROTATION ||
                               WorldChaseTag.Core.WCTMatchController.Instance.State == WorldChaseTag.Core.WCTMatchState.MATCH_RESULT ||
                               WorldChaseTag.Core.WCTMatchController.Instance.State == WorldChaseTag.Core.WCTMatchState.POST_MATCH));

            if (isUIActive)
            {
                // UI is active: Cursor is free and visible; Freeze camera & movement
                if (Cursor.lockState != CursorLockMode.None)
                {
                    Cursor.lockState = CursorLockMode.None;
                }
                if (!Cursor.visible)
                {
                    Cursor.visible = true;
                }

                ApplyGravityOnly();
                return;
            }

            // Gameplay is active: Cursor is locked and hidden for FPS camera
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            if (Cursor.visible)
            {
                Cursor.visible = false;
            }

            // Update timers
            if (controller.isGrounded)
            {
                coyoteTimer = coyoteTimeDuration;
            }
            else
            {
                coyoteTimer -= Time.deltaTime;
            }

            if (jumpBufferTimer > 0f)
            {
                jumpBufferTimer -= Time.deltaTime;
            }

            // Camera angle / mouse look ALWAYS works in gameplay!
            HandleMouseLook();

            // When controls are disabled (e.g. 3-2-1 countdown), freeze character movement only
            if (!ControlsEnabled || isVaulting || isMantling)
            {
                ApplyGravityOnly();
                return;
            }

            HandleInputs();
            HandleParkourDetection();
            HandleMovement();
            HandleStamina();
            HandleCameraEffects();
        }

        private void HandleMouseLook()
        {
            float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            float rawMouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

            // Horizontal turn (yaw)
            transform.Rotate(Vector3.up * mouseX);

            // Vertical pitch: Moving mouse UP tilts camera UP towards sky
            float deltaPitch = invertY ? rawMouseY : -rawMouseY;
            cameraPitch = Mathf.Clamp(cameraPitch + deltaPitch, minPitch, maxPitch);
            if (cameraHolder != null)
            {
                cameraHolder.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
            }
        }

        private void HandleInputs()
        {
            // Jump Buffer
            if (Input.GetKeyDown(KeyCode.Space))
            {
                jumpBufferTimer = jumpBufferDuration;
            }

            // Crouch / Slide
            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.C))
            {
                if (IsSprinting && controller.isGrounded && !isSliding)
                {
                    StartSlide();
                }
                else
                {
                    SetCrouch(true);
                }
            }
            else if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.C))
            {
                if (!isSliding)
                {
                    SetCrouch(false);
                }
            }

            // Tag Attempt (Key F)
            if (Input.GetKeyDown(KeyCode.F))
            {
                AttemptTagAction();
            }
        }

        private void HandleMovement()
        {
            float inputX = Input.GetAxisRaw("Horizontal");
            float inputZ = Input.GetAxisRaw("Vertical");
            Vector3 inputDir = (transform.right * inputX + transform.forward * inputZ).normalized;

            // Determine target speed
            float targetSpeed = walkSpeed;

            if (isSliding)
            {
                // Slide speed decaying
                horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, deceleration * 0.8f * Time.deltaTime);
                if (horizontalVelocity.magnitude <= crouchSpeed)
                {
                    isSliding = false;
                    SetCrouch(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C));
                }
            }
            else
            {
                if (isCrouching)
                {
                    targetSpeed = crouchSpeed;
                }
                else if (Input.GetKey(KeyCode.LeftShift) && !isExhausted && inputDir.magnitude > 0.1f)
                {
                    targetSpeed = sprintSpeed;
                }

                Vector3 targetVelocity = inputDir * targetSpeed;
                float accelRate = controller.isGrounded ? (inputDir.magnitude > 0.1f ? acceleration : deceleration) : (acceleration * airControlMultiplier);
                horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, accelRate * Time.deltaTime);
            }

            // Jump
            if (jumpBufferTimer > 0f && coyoteTimer > 0f)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpBufferTimer = 0f;
                coyoteTimer = 0f;
            }

            // Ground stick & gravity
            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2.5f;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }

            currentVelocity = horizontalVelocity + Vector3.up * verticalVelocity;
            controller.Move(currentVelocity * Time.deltaTime);

            // Step sound
            if (controller.isGrounded && horizontalVelocity.magnitude > 0.8f && !isSliding)
            {
                float stepInterval = IsSprinting ? 0.3f : 0.46f;
                stepTimer += Time.deltaTime;
                if (stepTimer >= stepInterval)
                {
                    stepTimer = 0f;
                    WCTAudioSynthesizer.Instance?.PlayFootstep();
                }
            }
            else
            {
                stepTimer = 0f;
            }

            // Landing impact sound
            if (!wasGroundedLastFrame && controller.isGrounded && verticalVelocity < -2.5f)
            {
                WCTAudioSynthesizer.Instance?.PlayLanding();
            }
            wasGroundedLastFrame = controller.isGrounded;
        }

        private void HandleParkourDetection()
        {
            // Do not vault while already vaulting or crouching
            if (isVaulting || isMantling || isCrouching || isSliding) return;

            Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
            Vector3 forward = transform.forward;

            // Check if player is moving forward toward obstacle and hits Space or is sprinting
            bool parkourRequested = jumpBufferTimer > 0f || (IsSprinting && Input.GetAxisRaw("Vertical") > 0.5f);
            if (!parkourRequested) return;

            // Raycast forward to find obstacle wall
            if (Physics.Raycast(rayOrigin, forward, out RaycastHit wallHit, 1.35f, obstacleLayer, QueryTriggerInteraction.Ignore))
            {
                // Verify wall angle (nearly vertical)
                if (Vector3.Angle(wallHit.normal, Vector3.up) > 60f)
                {
                    // Cast downward from above to find top ledge surface
                    Vector3 testTopOrigin = wallHit.point + forward * 0.4f + Vector3.up * (maxMantleHeight + 0.3f);
                    if (Physics.Raycast(testTopOrigin, Vector3.down, out RaycastHit ledgeHit, maxMantleHeight + 0.5f, obstacleLayer, QueryTriggerInteraction.Ignore))
                    {
                        float obstacleHeight = ledgeHit.point.y - transform.position.y;

                        // Low Vault
                        if (obstacleHeight >= minVaultHeight && obstacleHeight <= maxVaultHeight)
                        {
                            // Check if landing zone beyond obstacle is clear
                            Vector3 targetLanding = wallHit.point + forward * vaultDistance;
                            targetLanding.y = transform.position.y;

                            StartCoroutine(ExecuteVault(ledgeHit.point, targetLanding));
                            jumpBufferTimer = 0f;
                            return;
                        }

                        // High Mantle / Ledge Grab
                        if (obstacleHeight > maxVaultHeight && obstacleHeight <= maxMantleHeight)
                        {
                            Vector3 targetLedgeTop = ledgeHit.point + forward * 0.4f + Vector3.up * 0.05f;
                            StartCoroutine(ExecuteMantle(targetLedgeTop));
                            jumpBufferTimer = 0f;
                            return;
                        }
                    }
                }
            }
        }

        private IEnumerator ExecuteVault(Vector3 vaultPeak, Vector3 landingPoint)
        {
            isVaulting = true;
            controller.enabled = false;
            OnParkourActionTriggered?.Invoke("VAULT");
            WCTAudioSynthesizer.Instance?.PlayVaultWhoosh();

            Vector3 startPos = transform.position;
            Vector3 peakPos = vaultPeak + Vector3.up * 0.4f;

            float elapsed = 0f;
            while (elapsed < vaultDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / vaultDuration);

                // Quadratic bezier curve over the obstacle
                Vector3 m1 = Vector3.Lerp(startPos, peakPos, t);
                Vector3 m2 = Vector3.Lerp(peakPos, landingPoint, t);
                transform.position = Vector3.Lerp(m1, m2, t);

                yield return null;
            }

            transform.position = landingPoint;
            controller.enabled = true;
            verticalVelocity = 0f;
            horizontalVelocity = transform.forward * sprintSpeed;
            isVaulting = false;
        }

        private IEnumerator ExecuteMantle(Vector3 targetLedgeTop)
        {
            isMantling = true;
            controller.enabled = false;
            OnParkourActionTriggered?.Invoke("MANTLE");

            Vector3 startPos = transform.position;
            Vector3 pullUpPos = new Vector3(startPos.x, targetLedgeTop.y + 0.2f, startPos.z);

            float elapsed = 0f;
            while (elapsed < mantleDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / mantleDuration);

                if (t < 0.6f)
                {
                    // Vertical pull up
                    float pullT = t / 0.6f;
                    transform.position = Vector3.Lerp(startPos, pullUpPos, pullT);
                }
                else
                {
                    // Step forward onto platform
                    float stepT = (t - 0.6f) / 0.4f;
                    transform.position = Vector3.Lerp(pullUpPos, targetLedgeTop, stepT);
                }

                yield return null;
            }

            transform.position = targetLedgeTop;
            controller.enabled = true;
            verticalVelocity = 0f;
            horizontalVelocity = transform.forward * walkSpeed;
            isMantling = false;
        }

        private void StartSlide()
        {
            isSliding = true;
            SetCrouch(true);
            horizontalVelocity = transform.forward * slideInitialSpeed;
            OnParkourActionTriggered?.Invoke("SLIDE");
            WCTAudioSynthesizer.Instance?.PlaySlide();
        }

        private void SetCrouch(bool crouch)
        {
            if (isCrouching == crouch) return;

            // Prevent standing up if blocked by overhead geometry
            if (!crouch)
            {
                if (Physics.SphereCast(transform.position + Vector3.up * 0.5f, 0.35f, Vector3.up, out _, defaultControllerHeight - 0.5f, obstacleLayer, QueryTriggerInteraction.Ignore))
                {
                    return; // Overhead obstacle blocks standing
                }
            }

            isCrouching = crouch;
            controller.height = isCrouching ? crouchControllerHeight : defaultControllerHeight;
            controller.center = Vector3.up * (controller.height * 0.5f);

            if (cameraHolder != null)
            {
                cameraHolder.localPosition = isCrouching ? defaultCameraLocalPos * 0.6f : defaultCameraLocalPos;
            }
        }

        private void HandleStamina()
        {
            if (IsSprinting)
            {
                currentStamina = Mathf.Max(0f, currentStamina - staminaDrainRate * Time.deltaTime);
                if (currentStamina <= 0f)
                {
                    isExhausted = true;
                }
            }
            else
            {
                currentStamina = Mathf.Min(maxStamina, currentStamina + staminaRegenRate * Time.deltaTime);
                if (isExhausted && currentStamina >= exhaustionThreshold)
                {
                    isExhausted = false;
                }
            }
        }

        private void HandleCameraEffects()
        {
            if (playerCamera == null) return;

            // Dynamic FOV
            float targetFov = IsSprinting ? sprintFov : baseFov;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFov, fovTransitionSpeed * Time.deltaTime);

            // Head Bobbing
            if (enableHeadBob && controller.isGrounded && !isVaulting && !isMantling && !isSliding)
            {
                float speed = horizontalVelocity.magnitude;
                if (speed > 0.5f)
                {
                    bobTimer += Time.deltaTime * speed * 2.2f;
                    float bobY = Mathf.Sin(bobTimer) * 0.05f * bobIntensity;
                    float bobX = Mathf.Cos(bobTimer * 0.5f) * 0.03f * bobIntensity;
                    Vector3 targetPos = (isCrouching ? defaultCameraLocalPos * 0.6f : defaultCameraLocalPos) + new Vector3(bobX, bobY, 0f);
                    cameraHolder.localPosition = Vector3.Lerp(cameraHolder.localPosition, targetPos, 14f * Time.deltaTime);
                }
                else
                {
                    bobTimer = 0f;
                    Vector3 targetPos = isCrouching ? defaultCameraLocalPos * 0.6f : defaultCameraLocalPos;
                    cameraHolder.localPosition = Vector3.Lerp(cameraHolder.localPosition, targetPos, 10f * Time.deltaTime);
                }
            }
        }

        private void AttemptTagAction()
        {
            OnParkourActionTriggered?.Invoke("TAG_ATTEMPT");
            if (WCTTagSystem.Instance != null)
            {
                WCTTagSystem.Instance.ExecuteTagAttempt(transform.position + Vector3.up * 1.0f, transform.forward);
            }
        }

        private void ApplyGravityOnly()
        {
            if (controller.enabled)
            {
                verticalVelocity += gravity * Time.deltaTime;
                controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);
            }
        }

        public void TeleportTo(Vector3 position, Quaternion rotation)
        {
            bool wasEnabled = controller.enabled;
            controller.enabled = false;
            transform.position = position;
            transform.rotation = rotation;
            horizontalVelocity = Vector3.zero;
            verticalVelocity = 0f;
            currentVelocity = Vector3.zero;
            controller.enabled = wasEnabled;
        }

        public void SetSensitivity(float sens) => mouseSensitivity = sens;
        public void SetBaseFov(float fov) => baseFov = fov;
        public void SetHeadBob(bool enabled) => enableHeadBob = enabled;
    }
}

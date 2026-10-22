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

        [Header("Gyroscope & Mobile Controls")]
        [SerializeField] private bool enableGyroscope = true;
        [SerializeField] private float gyroSensitivity = 2.4f;

        // Mobile touch & virtual inputs
        private Vector2 virtualMoveInput = Vector2.zero;
        private bool virtualSprint = false;
        private Vector2 touchLookDelta = Vector2.zero;

        // Runtime states
        private CharacterController controller;
        private Vector3 currentVelocity;
        private Vector3 horizontalVelocity;
        private Vector3 wallSlideNormal = Vector3.zero;
        private bool hasWallContact = false;
        private float lastWallContactTime = 0f;
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
        public bool IsSprinting => (Input.GetKey(KeyCode.LeftShift) || virtualSprint) && horizontalVelocity.magnitude > walkSpeed + 0.5f && !isExhausted;
        public bool IsVaulting => isVaulting;
        public bool IsMantling => isMantling;
        public bool IsCrouching => isCrouching;
        public bool IsSliding => isSliding;
        public Camera PlayerCamera => playerCamera;

        public bool EnableGyroscope
        {
            get => enableGyroscope;
            set
            {
                enableGyroscope = value;
                if (SystemInfo.supportsGyroscope)
                {
                    Input.gyro.enabled = value;
                }
            }
        }

        public void SetVirtualJoystick(Vector2 input, bool sprint)
        {
            virtualMoveInput = Vector2.ClampMagnitude(input, 1f);
            virtualSprint = sprint;
        }

        public void AddTouchLookDelta(Vector2 delta)
        {
            touchLookDelta += delta;
        }

        public void TriggerJump()
        {
            jumpBufferTimer = jumpBufferDuration;
        }

        public void TriggerSlideOrCrouch()
        {
            if (IsSprinting && controller.isGrounded && !isSliding)
            {
                StartSlide();
            }
            else
            {
                SetCrouch(!isCrouching);
            }
        }

        public void TriggerCatchOrTag()
        {
            AttemptTagAction();
        }

        [Header("Perspective")]
        [SerializeField] private bool isThirdPerson = false;
        public bool IsThirdPerson => isThirdPerson;
        public bool IsAutoRun { get => isAutoRun; set => isAutoRun = value; }
        private bool isAutoRun = false;

        public void TogglePerspective()
        {
            SetPerspective(!isThirdPerson);
        }

        public void SetPerspective(bool tpp)
        {
            isThirdPerson = tpp;
            if (cameraHolder != null)
            {
                cameraHolder.localPosition = isThirdPerson ? new Vector3(0.35f, 1.75f, -2.6f) : defaultCameraLocalPos;
            }

            var fppArms = GetComponentInChildren<WorldChaseTag.Visuals.WCTFPPArms>(true);
            if (fppArms != null)
            {
                fppArms.gameObject.SetActive(!isThirdPerson);
            }

            var athleteVisuals = GetComponent<WorldChaseTag.Visuals.WCTAthleteVisuals>();
            if (athleteVisuals != null)
            {
                athleteVisuals.SetBodyVisible(isThirdPerson);
            }
        }

        public void TriggerClimb()
        {
            jumpBufferTimer = jumpBufferDuration;
        }

        public void TriggerDrop()
        {
            SetCrouch(true);
        }

        public void TriggerDive()
        {
            if (controller.isGrounded)
            {
                StartSlide();
            }
        }

        public event Action<string> OnParkourActionTriggered; // "VAULT", "MANTLE", "SLIDE", "TAG_ATTEMPT"

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            controller.stepOffset = 0.5f;
            controller.slopeLimit = 60f;
            controller.skinWidth = 0.015f;
            controller.minMoveDistance = 0f;
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
            if (SystemInfo.supportsGyroscope && enableGyroscope)
            {
                Input.gyro.enabled = true;
            }
        }

        private void Start()
        {
            if (!Application.isMobilePlatform)
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
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
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

            bool isCustomizeOpen = WorldChaseTag.UI.WCTHUDController.Instance != null && WorldChaseTag.UI.WCTHUDController.Instance.IsCustomizeControlsOpen;
            bool freeCursor = isUIActive || isCustomizeOpen || Input.GetKey(KeyCode.LeftAlt);

            if (!Application.isMobilePlatform)
            {
                if (freeCursor)
                {
                    if (Cursor.lockState != CursorLockMode.None)
                    {
                        Cursor.lockState = CursorLockMode.None;
                    }
                    if (!Cursor.visible)
                    {
                        Cursor.visible = true;
                    }

                    if (isUIActive)
                    {
                        ApplyGravityOnly();
                        return;
                    }
                }
                else
                {
                    if (Cursor.lockState != CursorLockMode.Locked)
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                    }
                    if (Cursor.visible)
                    {
                        Cursor.visible = false;
                    }
                }
            }
            else
            {
                if (isUIActive)
                {
                    ApplyGravityOnly();
                    return;
                }
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

            // Camera angle / mouse look works when cursor is locked or via touch swipe
            if (!freeCursor || Application.isMobilePlatform)
            {
                HandleMouseLook();
            }

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
            EnforceArenaBoundaries();
        }

        private void HandleMouseLook()
        {
            float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            float rawMouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

            // Touch Swipe Delta on screen
            if (touchLookDelta.sqrMagnitude > 0.0001f)
            {
                mouseX += touchLookDelta.x;
                rawMouseY += touchLookDelta.y;
                touchLookDelta = Vector2.zero;
            }

            // Mobile Gyroscope look (Input.gyro.rotationRateUnbiased)
            if (enableGyroscope && SystemInfo.supportsGyroscope && Input.gyro.enabled)
            {
                Vector3 gyroRate = Input.gyro.rotationRateUnbiased;
                // gyroRate.x = pitch rate (tilt forward/back)
                // gyroRate.y = yaw rate (turn left/right)
                float gyroYaw = -gyroRate.y * Mathf.Rad2Deg * Time.deltaTime * gyroSensitivity;
                float gyroPitch = -gyroRate.x * Mathf.Rad2Deg * Time.deltaTime * gyroSensitivity;

                mouseX += gyroYaw;
                rawMouseY += gyroPitch;
            }

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

            // Crouch / Slide (LeftControl or LeftAlt)
            if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.LeftAlt))
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
            else if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.LeftAlt))
            {
                if (!isSliding)
                {
                    SetCrouch(false);
                }
            }

            // Catch / Tag Attempt on KEY C (and Left Mouse Click)
            if (Input.GetKeyDown(KeyCode.C) || Input.GetMouseButtonDown(0))
            {
                AttemptTagAction();
            }

            // Toggle Perspective (TPP / FPP) on Key V
            if (Input.GetKeyDown(KeyCode.V))
            {
                TogglePerspective();
            }
        }

        private void HandleMovement()
        {
            float inputX = Input.GetAxisRaw("Horizontal");
            float inputZ = Input.GetAxisRaw("Vertical");

            // Auto-run locks forward sprint
            if (isAutoRun)
            {
                inputZ = 1f;
                virtualSprint = true;
            }

            // Merge virtual joystick input
            if (virtualMoveInput.sqrMagnitude > 0.001f)
            {
                inputX += virtualMoveInput.x;
                inputZ += virtualMoveInput.y;
                inputX = Mathf.Clamp(inputX, -1f, 1f);
                inputZ = Mathf.Clamp(inputZ, -1f, 1f);
            }

            Vector3 inputDir = (transform.right * inputX + transform.forward * inputZ);
            if (inputDir.sqrMagnitude > 0.001f)
            {
                inputDir = inputDir.normalized;

                // 1. Deflect input using recent wall collision normal from OnControllerColliderHit
                if (hasWallContact && (Time.time - lastWallContactTime < 0.25f) && wallSlideNormal.sqrMagnitude > 0.01f)
                {
                    float dot = Vector3.Dot(inputDir, wallSlideNormal);
                    if (dot < -0.01f)
                    {
                        Vector3 tangent = Vector3.ProjectOnPlane(inputDir, wallSlideNormal);
                        if (tangent.sqrMagnitude > 0.001f)
                        {
                            inputDir = tangent.normalized;
                        }
                        else
                        {
                            inputDir = Vector3.zero;
                        }
                    }
                }
                else
                {
                    hasWallContact = false;
                }

                // 2. Proactive short raycast to glide along obstacle surfaces before snagging
                if (Physics.Raycast(transform.position + Vector3.up * 0.8f, inputDir, out RaycastHit proximityHit, 0.45f, obstacleLayer, QueryTriggerInteraction.Ignore))
                {
                    if (proximityHit.normal.y < 0.65f)
                    {
                        Vector3 n = new Vector3(proximityHit.normal.x, 0f, proximityHit.normal.z).normalized;
                        float dot = Vector3.Dot(inputDir, n);
                        if (dot < -0.01f)
                        {
                            Vector3 tangent = Vector3.ProjectOnPlane(inputDir, n);
                            if (tangent.sqrMagnitude > 0.001f)
                            {
                                inputDir = tangent.normalized;
                            }
                        }
                    }
                }
            }
            else
            {
                inputDir = Vector3.zero;
            }

            // Determine target speed
            float targetSpeed = walkSpeed;

            if (isSliding)
            {
                // Slide speed decaying
                horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, Vector3.zero, deceleration * 0.8f * Time.deltaTime);
                if (horizontalVelocity.magnitude <= crouchSpeed)
                {
                    isSliding = false;
                    SetCrouch(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftAlt));
                }
            }
            else
            {
                if (isCrouching)
                {
                    targetSpeed = crouchSpeed;
                }
                else if ((Input.GetKey(KeyCode.LeftShift) || virtualSprint) && !isExhausted && inputDir.magnitude > 0.1f)
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

            // Wall slide resolution: eliminate velocity components pushing directly into the wall surface
            if (hasWallContact && (Time.time - lastWallContactTime < 0.25f))
            {
                float dot = Vector3.Dot(horizontalVelocity, wallSlideNormal);
                if (dot < 0f)
                {
                    horizontalVelocity -= wallSlideNormal * dot;
                }
            }

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

            Vector3 forward = transform.forward;

            // Check if player is moving forward toward obstacle (keyboard or mobile joystick)
            bool isMovingForward = (Input.GetAxisRaw("Vertical") > 0.3f) || (virtualMoveInput.y > 0.3f);
            bool isSprintRequested = (Input.GetKey(KeyCode.LeftShift) || virtualSprint) && !isExhausted;
            bool parkourRequested = jumpBufferTimer > 0f || (isMovingForward && (isSprintRequested || horizontalVelocity.magnitude > 2.5f));
            if (!parkourRequested) return;

            // Raycast forward from both mid and low heights to catch both high blocks and low hurdles
            RaycastHit wallHit;
            bool hitWall = Physics.Raycast(transform.position + Vector3.up * 0.55f, forward, out wallHit, 1.4f, obstacleLayer, QueryTriggerInteraction.Ignore) ||
                           Physics.Raycast(transform.position + Vector3.up * 0.25f, forward, out wallHit, 1.4f, obstacleLayer, QueryTriggerInteraction.Ignore);

            if (hitWall)
            {
                // Never vault or mantle over arena perimeter containment walls!
                if (wallHit.collider != null)
                {
                    string colName = wallHit.collider.name;
                    if (colName.StartsWith("Barrier_") || colName.Contains("Perimeter") || wallHit.collider.CompareTag("Boundary"))
                    {
                        return;
                    }
                }

                // Verify wall angle (nearly vertical)
                if (Vector3.Angle(wallHit.normal, Vector3.up) > 50f)
                {
                    // Multi-point forward ledge sampling: catches thin hurdles (0.12m), medium rails (0.28m), and deep blocks (0.45m)
                    float[] sampleOffsets = new float[] { 0.12f, 0.28f, 0.45f };
                    RaycastHit ledgeHit = default;
                    bool foundLedge = false;

                    foreach (float offset in sampleOffsets)
                    {
                        Vector3 testTopOrigin = wallHit.point + forward * offset + Vector3.up * (maxMantleHeight + 0.3f);
                        if (Physics.Raycast(testTopOrigin, Vector3.down, out RaycastHit candidateHit, maxMantleHeight + 0.5f, obstacleLayer, QueryTriggerInteraction.Ignore))
                        {
                            float h = candidateHit.point.y - transform.position.y;
                            if (h >= minVaultHeight && h <= maxMantleHeight)
                            {
                                ledgeHit = candidateHit;
                                foundLedge = true;
                                break;
                            }
                        }
                    }

                    if (foundLedge)
                    {
                        float obstacleHeight = ledgeHit.point.y - transform.position.y;

                        // Low Vault
                        if (obstacleHeight >= minVaultHeight && obstacleHeight <= maxVaultHeight)
                        {
                            Vector3 targetLanding = wallHit.point + forward * vaultDistance;
                            targetLanding.y = transform.position.y;

                            StartCoroutine(ExecuteVault(ledgeHit.point, targetLanding));
                            jumpBufferTimer = 0f;
                            return;
                        }

                        // High Mantle / Ledge Grab
                        if (obstacleHeight > maxVaultHeight && obstacleHeight <= maxMantleHeight)
                        {
                            Vector3 targetLedgeTop = ledgeHit.point + forward * 0.3f + Vector3.up * 0.05f;
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
            Vector3 peakPos = vaultPeak + Vector3.up * 0.35f;

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

            // Athletic forward lunge burst when Chaser strikes for a tag
            if (currentRole == ChaseRole.Chaser)
            {
                horizontalVelocity += transform.forward * 3.2f;
            }

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

        /// <summary>
        /// Real-time collision contact resolver. Deflects player velocity smoothly along vertical obstacle faces
        /// to prevent friction sticking or snagging on corners.
        /// </summary>
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.normal.y < 0.65f && hit.normal.y > -0.65f)
            {
                Vector3 wallNormal = new Vector3(hit.normal.x, 0f, hit.normal.z).normalized;
                if (wallNormal.sqrMagnitude > 0.01f)
                {
                    wallSlideNormal = wallNormal;
                    hasWallContact = true;
                    lastWallContactTime = Time.time;

                    // Immediately remove velocity directed straight into the wall face
                    float normalSpeed = Vector3.Dot(horizontalVelocity, wallNormal);
                    if (normalSpeed < 0f)
                    {
                        horizontalVelocity -= wallNormal * normalSpeed;
                    }
                }
            }
        }

        /// <summary>
        /// Safety boundary clamp: Ensures player never falls out of the arena or tunnels outside.
        /// </summary>
        private void EnforceArenaBoundaries()
        {
            if (isVaulting || isMantling) return;

            Vector3 pos = transform.position;
            const float maxExtent = 5.65f; // Official Quad 12x12 inner boundary
            bool clamped = false;

            if (Mathf.Abs(pos.x) > maxExtent)
            {
                pos.x = Mathf.Clamp(pos.x, -maxExtent, maxExtent);
                clamped = true;
            }

            if (Mathf.Abs(pos.z) > maxExtent)
            {
                pos.z = Mathf.Clamp(pos.z, -maxExtent, maxExtent);
                clamped = true;
            }

            if (pos.y < -0.2f)
            {
                pos.y = 0.85f;
                verticalVelocity = 0f;
                clamped = true;
            }

            if (clamped)
            {
                controller.enabled = false;
                transform.position = pos;
                controller.enabled = true;
                horizontalVelocity = Vector3.zero;
            }
        }
    }
}

using System;
using UnityEngine;
using WorldChaseTag.Audio;
using WorldChaseTag.Core;
using WorldChaseTag.Movement;

namespace WorldChaseTag.UI
{
    /// <summary>
    /// In-game broadcast HUD controller matching Images 2, 3, 4, 5.
    /// Features:
    /// - Broadcast header with team scores, role tags, and mini athlete avatars
    /// - Circular compass radar mini-map with live distance & "Chaser Behind" alert
    /// - Running athlete icon stamina bar
    /// - WASD movement disc & action buttons with PC keycaps
    /// - Pause Menu with Current Match card, Tip box, and Quick Controls reminder
    /// - Chase Result Overlays: "EVADER ESCAPES!" & "EVADER TAGGED!" with 2D route replay maps
    /// - Post-Match tournament scoreboard with 16-chase history log
    /// </summary>
    public class WCTHUDController : MonoBehaviour
    {
        public static WCTHUDController Instance { get; private set; }

        [Header("References")]
        [SerializeField] private WCTMatchController matchController;
        [SerializeField] private WCTParkourController playerController;
        [SerializeField] private Transform opponentTransform;

        [Header("HUD Customization")]
        [SerializeField] private bool showCrosshair = true;
        [SerializeField] private bool showStaminaBar = true;
        [SerializeField] private bool showParkourPrompts = true;

        public void SetOpponentTransform(Transform target) => opponentTransform = target;

        // UI State
        private bool isPaused = false;
        public bool IsPaused => isPaused;
        private string activeBannerText = "";
        private float bannerAlpha = 0f;

        private GUIStyle timerStyle;
        private GUIStyle scoreStyle;
        private GUIStyle bannerStyle;
        private GUIStyle roleStyle;
        private GUIStyle promptStyle;
        private GUIStyle tableHeaderStyle;
        private GUIStyle tableRowStyle;
        private GUIStyle badgeStyle;
        private GUIStyle blueBtnStyle;
        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle headerStyle;
        private GUIStyle smallTextStyle;

        private Texture2D staminaFillTex;
        private Texture2D darkBoxTex;
        private Texture2D chaserAccentTex;
        private Texture2D evaderAccentTex;
        private Texture2D radarBgTex;
        private Texture2D alertRedTex;
        private Texture2D bluePillTex;
        private Texture2D panelBgTex;
        private Texture2D alphaCyanTex;
        private Texture2D circleTex;
        private Texture2D ringTex;
        private Texture2D semiDarkTex;

        // Mobile touch & virtual controls
        private int joystickFingerId = -1;
        private Vector2 joystickPos = Vector2.zero;
        private bool isMouseDraggingJoy = false;
        private int swipeLookFingerId = -1;
        private Vector2 lastSwipePos = Vector2.zero;

        // Customize Controls State & Presets matching exact reference UI
        public bool IsCustomizeControlsOpen => isCustomizeControlsOpen;
        private bool isCustomizeControlsOpen = false;
        private int currentPresetIndex = 0; // 0: Classic, 1: Two Thumb, 2: Left Hand, 3: Right Hand, 4: Large Buttons, 5: Compact
        private int customizeTabIndex = 0; // 0: Move, 1: Action, 2: View, 3: Combat
        private float joystickScale = 1.20f;
        private float joystickOpacity = 0.80f;
        private float buttonScale = 1.10f;
        private float buttonOpacity = 0.85f;
        private bool showLabels = true;
        private bool autoRunEnabled = true;
        private bool quickSprintEnabled = true;
        private bool holdToCrouch = false;
        private bool tapToSlide = true;
        private bool isAutoRunLocked = false;
        private float saveNotificationTimer = 0f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            CreateTextures();
        }

        private void Start()
        {
            if (matchController == null) matchController = WCTMatchController.Instance;
            if (playerController == null) playerController = FindAnyObjectByType<WCTParkourController>();

            if (matchController != null)
            {
                matchController.OnBannerAnnouncement += DisplayBanner;
            }

            LoadLayoutSettings();
        }

        private void OnDestroy()
        {
            if (matchController != null)
            {
                matchController.OnBannerAnnouncement -= DisplayBanner;
            }
        }

        private void CreateTextures()
        {
            staminaFillTex = MakeColorTex(new Color(0.08f, 0.72f, 0.98f, 0.95f));
            darkBoxTex = MakeColorTex(new Color(0.04f, 0.07f, 0.11f, 0.92f));
            semiDarkTex = MakeColorTex(new Color(0.06f, 0.10f, 0.16f, 0.85f));
            chaserAccentTex = MakeColorTex(new Color(0.96f, 0.44f, 0.08f, 0.95f));
            evaderAccentTex = MakeColorTex(new Color(0.06f, 0.65f, 0.98f, 0.95f));
            radarBgTex = MakeColorTex(new Color(0.04f, 0.07f, 0.11f, 0.88f));
            alertRedTex = MakeColorTex(new Color(0.88f, 0.18f, 0.15f, 0.95f));
            bluePillTex = MakeColorTex(new Color(0.08f, 0.48f, 0.94f, 0.95f));
            panelBgTex = MakeColorTex(new Color(0.08f, 0.11f, 0.16f, 0.94f));
            alphaCyanTex = MakeColorTex(new Color(0.08f, 0.72f, 0.98f, 0.95f));
            circleTex = MakeCircleTex(32, Color.white);
            ringTex = MakeRingTex(45, 3, new Color(0.35f, 0.5f, 0.68f, 0.8f));
        }

        private Texture2D MakeColorTex(Color col)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, col);
            tex.Apply();
            return tex;
        }

        private Texture2D MakeCircleTex(int radius, Color col)
        {
            int size = radius * 2;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float rSq = radius * radius;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - radius + 0.5f;
                    float dy = y - radius + 0.5f;
                    float distSq = dx * dx + dy * dy;
                    if (distSq <= rSq)
                    {
                        float alpha = Mathf.Clamp01((radius - Mathf.Sqrt(distSq)) * 1.5f) * col.a;
                        tex.SetPixel(x, y, new Color(col.r, col.g, col.b, alpha));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private Texture2D MakeRingTex(int radius, int thickness, Color col)
        {
            int size = radius * 2;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float outerSq = radius * radius;
            float innerRadius = radius - thickness;
            float innerSq = innerRadius * innerRadius;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = x - radius + 0.5f;
                    float dy = y - radius + 0.5f;
                    float distSq = dx * dx + dy * dy;
                    if (distSq <= outerSq && distSq >= innerSq)
                    {
                        tex.SetPixel(x, y, col);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            return tex;
        }

        private void Update()
        {
            if (isPaused)
            {
                if (Cursor.lockState != CursorLockMode.None)
                {
                    Cursor.lockState = CursorLockMode.None;
                }
                if (!Cursor.visible)
                {
                    Cursor.visible = true;
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Only toggle pause if master menu is not open
                if (WCTMenuController.Instance == null || !WCTMenuController.Instance.IsMenuOpen)
                {
                    TogglePause();
                }
            }

            if (bannerAlpha > 0f)
            {
                bannerAlpha -= Time.deltaTime * 0.4f;
            }

            if (Application.isMobilePlatform && !isPaused && (WCTMenuController.Instance == null || !WCTMenuController.Instance.IsMenuOpen))
            {
                UpdateMobileTouchInput();
            }
        }

        private void UpdateMobileTouchInput()
        {
            if (playerController == null)
            {
                playerController = FindAnyObjectByType<WCTParkourController>();
                if (playerController == null) return;
            }

            float sw = Screen.width;
            float sh = Screen.height;
            float joyRadius = 60f;
            Vector2 joyCenter = new Vector2(115f, sh - 115f);

            // Action button touch detection rectangles
            float spacing = 86f;
            float rightX = sw - (spacing * 3) - 25f;
            float actionY = sh - 110f;
            Rect jumpRect = new Rect(rightX - 15f, actionY - 15f, 90f, 90f);
            Rect slideRect = new Rect(rightX + spacing - 15f, actionY - 15f, 90f, 90f);
            Rect catchRect = new Rect(rightX + spacing * 2 - 20f, actionY - 24f, 110f, 110f);

            float gyroW = 120f;
            float gyroH = 28f;
            float gyroX = sw - gyroW - 225f;
            float gyroY = 12f;
            Rect gyroRect = new Rect(gyroX - 10f, gyroY - 6f, gyroW + 20f, gyroH + 16f);

            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    Vector2 screenPos = touch.position;
                    Vector2 guiPos = new Vector2(screenPos.x, sh - screenPos.y);

                    // 1. Direct Multi-Touch Action Buttons (Instant response on touch down)
                    if (touch.phase == TouchPhase.Began)
                    {
                        if (catchRect.Contains(guiPos))
                        {
                            playerController.TriggerCatchOrTag();
                            continue;
                        }
                        if (jumpRect.Contains(guiPos))
                        {
                            playerController.TriggerJump();
                            continue;
                        }
                        if (slideRect.Contains(guiPos))
                        {
                            playerController.TriggerSlideOrCrouch();
                            continue;
                        }
                        if (gyroRect.Contains(guiPos))
                        {
                            playerController.EnableGyroscope = !playerController.EnableGyroscope;
                            continue;
                        }
                    }

                    // 2. Left touch zone: Virtual Joystick
                    if (guiPos.x < sw * 0.45f && guiPos.y > sh * 0.25f)
                    {
                        if (touch.phase == TouchPhase.Began && joystickFingerId == -1)
                        {
                            joystickFingerId = touch.fingerId;
                        }
                    }

                    if (touch.fingerId == joystickFingerId)
                    {
                        if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                        {
                            Vector2 offset = guiPos - joyCenter;
                            float dist = offset.magnitude;
                            Vector2 dir = dist > 0.001f ? offset.normalized : Vector2.zero;
                            float clampedDist = Mathf.Min(dist, joyRadius);
                            joystickPos = joyCenter + dir * clampedDist;

                            Vector2 moveInput = new Vector2(offset.x / joyRadius, -offset.y / joyRadius);
                            bool sprint = dist > (joyRadius * 0.70f);
                            playerController.SetVirtualJoystick(moveInput, sprint);
                        }
                        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                        {
                            joystickFingerId = -1;
                            joystickPos = joyCenter;
                            playerController.SetVirtualJoystick(Vector2.zero, false);
                        }
                    }
                    // 3. Right touch zone: Swipe Look (excluding bottom-right buttons area)
                    else if (guiPos.x > sw * 0.40f && !catchRect.Contains(guiPos) && !jumpRect.Contains(guiPos) && !slideRect.Contains(guiPos))
                    {
                        if (touch.phase == TouchPhase.Began && swipeLookFingerId == -1)
                        {
                            swipeLookFingerId = touch.fingerId;
                            lastSwipePos = touch.position;
                        }
                        else if (touch.fingerId == swipeLookFingerId)
                        {
                            if (touch.phase == TouchPhase.Moved)
                            {
                                Vector2 delta = (touch.position - lastSwipePos) * 0.16f;
                                playerController.AddTouchLookDelta(new Vector2(delta.x, delta.y));
                                lastSwipePos = touch.position;
                            }
                            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                            {
                                swipeLookFingerId = -1;
                            }
                        }
                    }
                }
            }
            else
            {
                // Editor / PC Mouse fallback for testing joystick!
                if (Input.GetMouseButtonDown(0))
                {
                    Vector2 mousePos = Input.mousePosition;
                    Vector2 guiPos = new Vector2(mousePos.x, sh - mousePos.y);
                    if (Vector2.Distance(guiPos, joyCenter) < joyRadius * 1.5f)
                    {
                        isMouseDraggingJoy = true;
                    }
                }

                if (isMouseDraggingJoy)
                {
                    if (Input.GetMouseButton(0))
                    {
                        Vector2 mousePos = Input.mousePosition;
                        Vector2 guiPos = new Vector2(mousePos.x, sh - mousePos.y);
                        Vector2 offset = guiPos - joyCenter;
                        float dist = offset.magnitude;
                        Vector2 dir = dist > 0.001f ? offset.normalized : Vector2.zero;
                        float clampedDist = Mathf.Min(dist, joyRadius);
                        joystickPos = joyCenter + dir * clampedDist;

                        Vector2 moveInput = new Vector2(offset.x / joyRadius, -offset.y / joyRadius);
                        bool sprint = dist > (joyRadius * 0.75f);
                        playerController.SetVirtualJoystick(moveInput, sprint);
                    }
                    else
                    {
                        isMouseDraggingJoy = false;
                        joystickPos = joyCenter;
                        playerController.SetVirtualJoystick(Vector2.zero, false);
                    }
                }
                else if (joystickFingerId == -1)
                {
                    joystickPos = joyCenter;
                }
            }
        }

        public void DisplayBanner(string message)
        {
            DisplayBanner(message, 2.5f);
        }

        public void DisplayBanner(string message, float duration)
        {
            activeBannerText = message;
            bannerAlpha = 1.0f;
        }

        public void TogglePause()
        {
            isPaused = !isPaused;
            Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isPaused;
            Time.timeScale = isPaused ? 0f : 1f;

            if (playerController == null)
            {
                playerController = FindAnyObjectByType<WCTParkourController>();
            }

            if (playerController != null)
            {
                playerController.ControlsEnabled = !isPaused;
            }
        }

        private void InitStyles()
        {
            if (timerStyle == null)
            {
                timerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 34,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            if (scoreStyle == null)
            {
                scoreStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 20,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            if (bannerStyle == null)
            {
                bannerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 44,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            if (roleStyle == null)
            {
                roleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            if (promptStyle == null)
            {
                promptStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft
                };
            }

            if (tableHeaderStyle == null)
            {
                tableHeaderStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft
                };
            }

            if (tableRowStyle == null)
            {
                tableRowStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    alignment = TextAnchor.MiddleLeft
                };
            }

            if (badgeStyle == null)
            {
                badgeStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            if (blueBtnStyle == null)
            {
                blueBtnStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 36,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft
                };
            }

            if (subtitleStyle == null)
            {
                subtitleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleLeft
                };
            }

            if (headerStyle == null)
            {
                headerStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft
                };
            }

            if (smallTextStyle == null)
            {
                smallTextStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 11,
                    alignment = TextAnchor.MiddleLeft
                };
            }
        }

        private void OnGUI()
        {
            if (WCTMenuController.Instance != null && WCTMenuController.Instance.IsMenuOpen)
            {
                return;
            }

            InitStyles();

            if (matchController == null)
            {
                matchController = WCTMatchController.Instance;
                if (matchController == null) return;
            }

            if (playerController == null)
            {
                playerController = FindAnyObjectByType<WCTParkourController>();
            }

            float sw = Screen.width;
            float sh = Screen.height;

            bool isResultShowing = (matchController.State == WCTMatchState.CHASE_RESULT || matchController.State == WCTMatchState.ROLE_ROTATION);

            // 1. Center Crosshair Reticle (Crisp PC Reticle in gameplay)
            if (showCrosshair && !isPaused && !isResultShowing && (WCTMenuController.Instance == null || !WCTMenuController.Instance.IsMenuOpen))
            {
                float cx = sw * 0.5f;
                float cy = sh * 0.5f;
                GUI.color = new Color(1f, 1f, 1f, 0.75f);
                GUI.DrawTexture(new Rect(cx - 3, cy - 3, 6, 6), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(cx - 14, cy - 1, 8, 2), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(cx + 6, cy - 1, 8, 2), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(cx - 1, cy - 14, 2, 8), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(cx - 1, cy + 6, 2, 8), Texture2D.whiteTexture);
                GUI.color = Color.white;
            }

            // 2. Top Broadcast Scoreboard Bar
            DrawTopBroadcastHeader(sw);

            if (!isResultShowing)
            {
                // Top-Left: Tournament Logo & Team Players Card
                DrawWCTTournamentLogo(22f, 12f);
                DrawTeamPlayersCard(22f, 68f);

                // Top-Right: Arena header, Minimap & Current Objective
                DrawTopRightHeaderAndMinimap(sw, sh);

                // Center-Bottom: Stamina Bar
                if (showStaminaBar && playerController != null)
                {
                    DrawStaminaMeter(sw, sh);
                }

                // 6. Championship AAA Esports HUD Controls & Customize Panel (100% matching reference screenshot!)
                DrawChampionshipControlsLayout(sw, sh);
            }

            // Center Countdown / Announcement Banner (3, 2, 1, GO!, EVADED, TAGGED)
            if (!isResultShowing && !isPaused && bannerAlpha > 0.05f && !string.IsNullOrEmpty(activeBannerText))
            {
                DrawCenterAnnouncementBanner(sw, sh);
            }

            // 7. Dynamic Chase Result Overlay (Images 4 & 5)
            if (isResultShowing)
            {
                DrawChaseResultOverlay(sw, sh);
            }

            // 8. Post-Match Tournament Scoreboard (Screen 11)
            if (matchController.State == WCTMatchState.POST_MATCH || matchController.State == WCTMatchState.MATCH_RESULT)
            {
                DrawPostMatchScreen(sw, sh);
            }

            // 9. Game Paused Overlay (Image 3)
            if (isPaused)
            {
                DrawPauseMenu(sw, sh);
            }
        }

        // ==============================================================
        // TOP-LEFT: TOURNAMENT LOGO (Wireframe Globe + Title + Subtitle)
        // ==============================================================
        private void DrawWCTTournamentLogo(float x, float y)
        {
            // Globe Wireframe Icon
            float globeSize = 34f;
            GUI.color = new Color(0.85f, 0.92f, 1.0f);
            GUI.DrawTexture(new Rect(x, y + 2, globeSize, globeSize), ringTex);
            badgeStyle.fontSize = 18;
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(x + 5, y + 6, 24, 24), "🌐", badgeStyle);

            // Title & Subtitle
            headerStyle.fontSize = 16;
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(x + globeSize + 8, y, 180, 20), "WORLD", headerStyle);

            headerStyle.normal.textColor = new Color(0.95f, 0.28f, 0.24f);
            GUI.Label(new Rect(x + globeSize + 70, y, 180, 20), "CHASE TAG", headerStyle);

            smallTextStyle.fontSize = 9;
            smallTextStyle.fontStyle = FontStyle.Normal;
            smallTextStyle.normal.textColor = new Color(0.65f, 0.75f, 0.88f);
            GUI.Label(new Rect(x + globeSize + 8, y + 18, 200, 14), "PARKOUR. TAG. COMPETE.", smallTextStyle);
            GUI.color = Color.white;
        }

        // ==============================================================
        // TOP-LEFT: TEAM PLAYERS CARD ("ALPHA (3/4)")
        // ==============================================================
        private void DrawTeamPlayersCard(float cardX, float cardY)
        {
            float cardW = 190f;
            float cardH = 152f;

            // Frosted dark glass container with sleek border
            GUI.DrawTexture(new Rect(cardX, cardY, cardW, cardH), darkBoxTex);
            GUI.DrawTexture(new Rect(cardX, cardY, cardW, 2), alphaCyanTex);

            badgeStyle.fontSize = 12;
            badgeStyle.fontStyle = FontStyle.Bold;
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(cardX + 10, cardY + 6, cardW - 20, 18), "ALPHA (3/4)", badgeStyle);

            string[] names = { "Player01", "RohanK", "VikramS", "AaravP" };
            float rowH = 25f;
            float startY = cardY + 28f;

            for (int i = 0; i < 4; i++)
            {
                float ry = startY + i * (rowH + 3f);
                bool isUser = (i == 0);

                if (isUser)
                {
                    // Active player cyan highlight
                    GUI.DrawTexture(new Rect(cardX + 6, ry, cardW - 12, rowH), bluePillTex);
                }
                else
                {
                    GUI.DrawTexture(new Rect(cardX + 6, ry, cardW - 12, rowH), semiDarkTex);
                }

                // Mini Avatar Circle
                GUI.color = isUser ? Color.white : new Color(0.7f, 0.85f, 1.0f);
                GUI.DrawTexture(new Rect(cardX + 10, ry + 4, 18, 18), circleTex);
                badgeStyle.fontSize = 10;
                badgeStyle.normal.textColor = Color.black;
                GUI.Label(new Rect(cardX + 10, ry + 4, 18, 18), "👤", badgeStyle);
                GUI.color = Color.white;

                // Athlete Name
                smallTextStyle.fontSize = 11;
                smallTextStyle.normal.textColor = Color.white;
                GUI.Label(new Rect(cardX + 32, ry + 4, 90, 18), names[i], smallTextStyle);

                // User crown icon & cyan underline
                if (isUser)
                {
                    badgeStyle.fontSize = 12;
                    badgeStyle.normal.textColor = new Color(1.0f, 0.82f, 0.15f);
                    GUI.Label(new Rect(cardX + cardW - 30, ry + 2, 20, 18), "👑", badgeStyle);

                    GUI.DrawTexture(new Rect(cardX + 6, ry + rowH - 2, cardW - 12, 2), alphaCyanTex);
                }
            }
        }

        // ==============================================================
        // TOP BROADCAST SCOREBOARD BAR (Matching Screenshot!)
        // ==============================================================
        private void DrawTopBroadcastHeader(float sw)
        {
            float totalW = 660f;
            float rx = (sw - totalW) * 0.5f;
            float barY = 14f;
            float bannerH = 46f;

            // 1. Alpha Team Box (Left Trapezoid)
            float alphaW = 200f;
            GUI.DrawTexture(new Rect(rx, barY, alphaW, bannerH), bluePillTex);

            scoreStyle.fontSize = 20;
            scoreStyle.fontStyle = FontStyle.Bold;
            scoreStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(rx + 16, barY + 10, 110, 26), "▲  ALPHA", scoreStyle);

            scoreStyle.fontSize = 28;
            scoreStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(rx + alphaW - 45, barY + 6, 36, 32), $"{matchController.TeamAScore}", scoreStyle);

            // 2. Center Digital Timer Capsule
            float timerW = 160f;
            float timerX = rx + alphaW + 12f;
            GUI.DrawTexture(new Rect(timerX, barY - 2f, timerW, bannerH + 4f), darkBoxTex);
            GUI.DrawTexture(new Rect(timerX, barY - 2f, timerW, 2f), alphaCyanTex);

            string chaseStr = matchController.IsSuddenDeath
                ? "SUDDEN DEATH"
                : $"CHASE {matchController.ChaseNumber} / {matchController.MaxChases}";

            badgeStyle.fontSize = 10;
            badgeStyle.normal.textColor = new Color(0.7f, 0.8f, 0.95f);
            GUI.Label(new Rect(timerX, barY + 2f, timerW, 14), chaseStr, badgeStyle);

            float timeVal = matchController.TimeRemaining;
            if (matchController.State == WCTMatchState.PRE_CHASE)
            {
                timeVal = 20.0f;
            }
            else if (matchController.State == WCTMatchState.CHASE_RESULT || matchController.State == WCTMatchState.ROLE_ROTATION)
            {
                timeVal = (matchController.LastOutcome == ChaseOutcome.EvaderSurvived) ? 0f : matchController.TimeRemaining;
            }

            Color timerCol = Color.white;
            if (matchController.State == WCTMatchState.CHASE_RESULT || matchController.State == WCTMatchState.ROLE_ROTATION)
            {
                timerCol = (matchController.LastOutcome == ChaseOutcome.EvaderSurvived) ? Color.cyan : Color.red;
            }
            else if (matchController.IsChaseActive && timeVal <= 5.0f)
            {
                float pulse = (Mathf.Sin(Time.time * 12f) + 1f) * 0.5f;
                timerCol = Color.Lerp(Color.red, Color.yellow, pulse);
            }

            timerStyle.fontSize = 26;
            timerStyle.fontStyle = FontStyle.Bold;
            timerStyle.normal.textColor = timerCol;
            GUI.Label(new Rect(timerX, barY + 16f, timerW, 28), $"00:{(int)timeVal:00}", timerStyle);

            // 3. Omega Team Box (Right Trapezoid)
            float omegaW = 200f;
            float omegaX = timerX + timerW + 12f;
            GUI.DrawTexture(new Rect(omegaX, barY, omegaW, bannerH), alertRedTex);

            scoreStyle.fontSize = 28;
            scoreStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(omegaX + 16, barY + 6, 36, 32), $"{matchController.TeamBScore}", scoreStyle);

            scoreStyle.fontSize = 20;
            scoreStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(omegaX + 60, barY + 10, 130, 26), "OMEGA  Ω", scoreStyle);
        }

        // ==============================================================
        // TOP-RIGHT: ARENA HEADER, TACTICAL MINIMAP, CURRENT OBJECTIVE
        // ==============================================================
        private void DrawTopRightHeaderAndMinimap(float sw, float sh)
        {
            float cardW = 190f;
            float cardX = sw - cardW - 22f;

            // 1. Header: "QUAD ARENA (CLASSIC)" + Settings Gear Button
            float headerY = 12f;
            badgeStyle.fontSize = 11;
            badgeStyle.fontStyle = FontStyle.Bold;
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(cardX, headerY, 140, 18), "QUAD ARENA (CLASSIC)", badgeStyle);

            // Settings Gear Icon Button (Click to open/close CUSTOMIZE CONTROLS)
            Rect gearRect = new Rect(cardX + cardW - 24, headerY - 2, 22, 22);
            GUI.color = isCustomizeControlsOpen ? new Color(0.1f, 0.75f, 0.98f, 0.95f) : new Color(0.06f, 0.1f, 0.16f, 0.85f);
            GUI.DrawTexture(gearRect, darkBoxTex);
            GUI.color = Color.white;
            badgeStyle.fontSize = 12;
            badgeStyle.normal.textColor = isCustomizeControlsOpen ? Color.black : Color.white;
            GUI.Label(gearRect, "⚙", badgeStyle);
            if (Event.current.type == EventType.MouseDown && gearRect.Contains(Event.current.mousePosition))
            {
                isCustomizeControlsOpen = !isCustomizeControlsOpen;
                WCTAudioSynthesizer.Instance?.PlayUIClick();
            }

            // 2. Blueprint Tactical Minimap Card
            float mapY = 32f;
            float mapH = 145f;
            GUI.DrawTexture(new Rect(cardX, mapY, cardW, mapH), darkBoxTex);
            GUI.DrawTexture(new Rect(cardX, mapY, cardW, 2), alphaCyanTex);

            // Grid Background
            GUI.DrawTexture(new Rect(cardX + 6, mapY + 6, cardW - 12, mapH - 12), panelBgTex);

            float cx = cardX + cardW * 0.5f;
            float cy = mapY + mapH * 0.5f;

            // Draw Obstacle Blueprints
            GUI.color = new Color(0.25f, 0.35f, 0.50f, 0.9f);
            // Central mountain
            GUI.DrawTexture(new Rect(cx - 20, cy - 14, 40, 28), Texture2D.whiteTexture);
            // Angled ramp on right
            GUI.DrawTexture(new Rect(cx + 26, cy - 8, 22, 16), Texture2D.whiteTexture);
            // Side blocks
            GUI.DrawTexture(new Rect(cx - 48, cy - 20, 16, 12), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(cx + 32, cy - 35, 16, 12), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Player Cyan Chevron (Center-South)
            float px = cx;
            float py = cy;
            if (playerController != null)
            {
                Vector3 p = playerController.transform.position;
                px = Mathf.Clamp(cx + (p.x / 12f) * (cardW * 0.40f), cardX + 12, cardX + cardW - 12);
                py = Mathf.Clamp(cy - (p.z / 12f) * (mapH * 0.40f), mapY + 12, mapY + mapH - 12);
            }
            GUI.color = Color.cyan;
            GUI.DrawTexture(new Rect(px - 5, py - 5, 10, 10), circleTex);
            GUI.color = Color.white;

            // Opponent Dot (Orange / Red)
            if (opponentTransform != null)
            {
                Vector3 o = opponentTransform.position;
                float ox = Mathf.Clamp(cx + (o.x / 12f) * (cardW * 0.40f), cardX + 12, cardX + cardW - 12);
                float oy = Mathf.Clamp(cy - (o.z / 12f) * (mapH * 0.40f), mapY + 12, mapY + mapH - 12);
                GUI.color = new Color(1.0f, 0.45f, 0.1f);
                GUI.DrawTexture(new Rect(ox - 4, oy - 4, 8, 8), circleTex);
                GUI.color = Color.white;
            }

            // Bottom-Right Zoom [+] Icon
            GUI.DrawTexture(new Rect(cardX + cardW - 22, mapY + mapH - 22, 16, 16), darkBoxTex);
            badgeStyle.fontSize = 11;
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(cardX + cardW - 22, mapY + mapH - 22, 16, 16), "+", badgeStyle);

            // 3. Current Objective Card below Minimap
            float objY = mapY + mapH + 10f;
            float objH = 46f;
            GUI.DrawTexture(new Rect(cardX, objY, cardW, objH), darkBoxTex);

            smallTextStyle.fontSize = 9;
            smallTextStyle.normal.textColor = new Color(0.65f, 0.75f, 0.9f);
            GUI.Label(new Rect(cardX + 8, objY + 4, cardW - 16, 14), "CURRENT OBJECTIVE", smallTextStyle);

            badgeStyle.fontSize = 14;
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(cardX + 8, objY + 20, 18, 18), "⏱", badgeStyle);

            bool isEvader = playerController == null || playerController.CurrentRole == ChaseRole.Evader;
            string objStr = isEvader ? "Survive for 20 seconds" : "Tag the Evader within 20s";

            smallTextStyle.fontSize = 11;
            smallTextStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(cardX + 28, objY + 21, cardW - 32, 18), objStr, smallTextStyle);
        }

        // ==============================================================
        // BOTTOM-CENTER: STAMINA BAR (Matching Reference Screenshot!)
        // ==============================================================
        private void DrawStaminaMeter(float sw, float sh)
        {
            float barW = 260f;
            float barH = 12f;
            float sx = (sw - barW) * 0.5f;
            float sy = sh - 58f; // Positioned right above the bottom preset bar

            // Running Athlete Circle Icon
            float iconSize = 28f;
            float iconX = sx - 36f;
            float iconY = sy - 8f;
            GUI.color = new Color(0.04f, 0.08f, 0.14f, 0.90f);
            GUI.DrawTexture(new Rect(iconX, iconY, iconSize, iconSize), circleTex);
            GUI.color = new Color(0.15f, 0.65f, 0.95f, 0.95f);
            GUI.DrawTexture(new Rect(iconX, iconY, iconSize, iconSize), ringTex);
            GUI.color = Color.white;
            badgeStyle.fontSize = 13;
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(iconX, iconY, iconSize, iconSize), "🏃", badgeStyle);

            // Dark meter track capsule
            GUI.color = new Color(0.04f, 0.07f, 0.12f, 0.88f);
            GUI.DrawTexture(new Rect(sx, sy, barW, barH), darkBoxTex);

            // Glowing Cyan Fill
            float fill = playerController != null ? playerController.StaminaNormalized : 1.0f;
            GUI.color = Color.cyan;
            GUI.DrawTexture(new Rect(sx, sy, barW * fill, barH), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // "STAMINA" label below bar
            smallTextStyle.fontSize = 9;
            smallTextStyle.fontStyle = FontStyle.Bold;
            smallTextStyle.alignment = TextAnchor.MiddleCenter;
            smallTextStyle.normal.textColor = new Color(0.65f, 0.85f, 1.0f);
            GUI.Label(new Rect(sx, sy + barH + 1, barW, 12), "STAMINA", smallTextStyle);
            smallTextStyle.alignment = TextAnchor.MiddleLeft;
        }

        // ==============================================================
        // CHAMPIONSHIP AAA CONTROLS LAYOUT (100% MATCHING SCREENSHOT)
        // ==============================================================
        private void DrawChampionshipControlsLayout(float sw, float sh)
        {
            if (saveNotificationTimer > 0f) saveNotificationTimer -= Time.deltaTime;
            Color baseCol = GUI.color;

            // ==========================================================
            // 1. LEFT SIDE CONTROLS (Auto Run, Sprint, Joystick, Backpack)
            // ==========================================================
            float joyRadius = 55f * joystickScale;
            Vector2 joyCenter = new Vector2(125f * (joystickScale / 1.2f), sh - 125f * (joystickScale / 1.2f));
            float joyDiameter = joyRadius * 2f;

            // Auto Run Pill Button (above sprint)
            if (autoRunEnabled)
            {
                float autoW = 94f;
                float autoH = 26f;
                float autoX = 35f;
                float autoY = sh - 250f;
                Rect autoRect = new Rect(autoX, autoY, autoW, autoH);
                GUI.color = isAutoRunLocked ? new Color(0.1f, 0.75f, 0.98f, 0.95f) : new Color(0.06f, 0.1f, 0.16f, buttonOpacity);
                GUI.DrawTexture(autoRect, darkBoxTex);
                GUI.color = isAutoRunLocked ? Color.cyan : new Color(0.3f, 0.5f, 0.7f, 0.8f);
                GUI.DrawTexture(new Rect(autoX, autoY, autoW, 2), Texture2D.whiteTexture);
                GUI.color = Color.white;

                badgeStyle.fontSize = 10;
                badgeStyle.fontStyle = FontStyle.Bold;
                badgeStyle.normal.textColor = isAutoRunLocked ? Color.black : Color.white;
                GUI.Label(autoRect, isAutoRunLocked ? "🏃 Auto Run 🔒" : "🏃 Auto Run 🔓", badgeStyle);

                if (Event.current.type == EventType.MouseDown && autoRect.Contains(Event.current.mousePosition))
                {
                    isAutoRunLocked = !isAutoRunLocked;
                    if (playerController != null) playerController.IsAutoRun = isAutoRunLocked;
                    WCTAudioSynthesizer.Instance?.PlayUIClick();
                }
            }

            // Sprint Circle Button (above joystick)
            float sprintSize = 52f * buttonScale;
            float sprintX = 35f;
            float sprintY = sh - 205f;
            Rect sprintRect = new Rect(sprintX, sprintY, sprintSize, sprintSize);
            bool isSprintingNow = (playerController != null && playerController.IsSprinting) || Input.GetKey(KeyCode.LeftShift);

            GUI.color = isSprintingNow ? new Color(0.1f, 0.75f, 0.98f, buttonOpacity) : new Color(0.05f, 0.08f, 0.14f, buttonOpacity);
            GUI.DrawTexture(sprintRect, circleTex);
            GUI.color = isSprintingNow ? Color.cyan : new Color(0.3f, 0.5f, 0.7f, buttonOpacity);
            GUI.DrawTexture(sprintRect, ringTex);
            badgeStyle.fontSize = (int)(sprintSize * 0.40f);
            badgeStyle.normal.textColor = isSprintingNow ? Color.black : Color.white;
            GUI.Label(new Rect(sprintX, sprintY + sprintSize * 0.18f, sprintSize, sprintSize * 0.5f), "🏃", badgeStyle);

            if (showLabels)
            {
                smallTextStyle.fontSize = 9;
                smallTextStyle.alignment = TextAnchor.MiddleCenter;
                smallTextStyle.normal.textColor = Color.white;
                GUI.Label(new Rect(sprintX - 10, sprintY + sprintSize + 1, sprintSize + 20, 14), "Sprint", smallTextStyle);
                smallTextStyle.alignment = TextAnchor.MiddleLeft;
            }

            if (Event.current.type == EventType.MouseDown && sprintRect.Contains(Event.current.mousePosition))
            {
                if (playerController != null) playerController.SetVirtualJoystick(new Vector2(0f, 1f), true);
            }

            // Virtual Joystick Disc (Matching Reference Image!)
            GUI.color = new Color(0.04f, 0.08f, 0.14f, joystickOpacity);
            GUI.DrawTexture(new Rect(joyCenter.x - joyRadius, joyCenter.y - joyRadius, joyDiameter, joyDiameter), circleTex);
            GUI.color = new Color(0.2f, 0.6f, 0.85f, joystickOpacity);
            GUI.DrawTexture(new Rect(joyCenter.x - joyRadius, joyCenter.y - joyRadius, joyDiameter, joyDiameter), ringTex);

            // Cardinal direction arrow indicators
            badgeStyle.fontSize = (int)(13f * (joystickScale / 1.2f));
            badgeStyle.normal.textColor = new Color(0.6f, 0.85f, 1f, joystickOpacity);
            GUI.Label(new Rect(joyCenter.x - 10, joyCenter.y - joyRadius + 4, 20, 20), "▲", badgeStyle);
            GUI.Label(new Rect(joyCenter.x - 10, joyCenter.y + joyRadius - 22, 20, 20), "▼", badgeStyle);
            GUI.Label(new Rect(joyCenter.x - joyRadius + 4, joyCenter.y - 10, 20, 20), "◀", badgeStyle);
            GUI.Label(new Rect(joyCenter.x + joyRadius - 22, joyCenter.y - 10, 20, 20), "▶", badgeStyle);

            // Calculate Thumb Knob position (WASD or mouse drag or touch)
            Vector2 knobPos = joyCenter;
            float inputX = Input.GetAxisRaw("Horizontal");
            float inputZ = Input.GetAxisRaw("Vertical");

            if (isMouseDraggingJoy)
            {
                Vector2 mousePos = Event.current.mousePosition;
                Vector2 delta = mousePos - joyCenter;
                knobPos = joyCenter + Vector2.ClampMagnitude(delta, joyRadius * 0.75f);
                if (Event.current.type == EventType.MouseUp)
                {
                    isMouseDraggingJoy = false;
                    if (playerController != null) playerController.SetVirtualJoystick(Vector2.zero, false);
                }
                else
                {
                    Vector2 norm = (knobPos - joyCenter) / (joyRadius * 0.75f);
                    norm.y = -norm.y; // Invert GUI Y
                    if (playerController != null) playerController.SetVirtualJoystick(norm, isSprintingNow);
                }
            }
            else if (Mathf.Abs(inputX) > 0.1f || Mathf.Abs(inputZ) > 0.1f)
            {
                Vector2 dir = new Vector2(inputX, -inputZ).normalized * (joyRadius * 0.65f);
                knobPos = joyCenter + dir;
            }

            // Mouse down on joystick
            Rect joyTouchRect = new Rect(joyCenter.x - joyRadius, joyCenter.y - joyRadius, joyDiameter, joyDiameter);
            if (Event.current.type == EventType.MouseDown && joyTouchRect.Contains(Event.current.mousePosition))
            {
                isMouseDraggingJoy = true;
            }

            // Thumb Knob
            float knobRadius = 24f * joystickScale;
            float knobDiameter = knobRadius * 2f;
            GUI.color = new Color(0.12f, 0.55f, 0.90f, joystickOpacity);
            GUI.DrawTexture(new Rect(knobPos.x - knobRadius, knobPos.y - knobRadius, knobDiameter, knobDiameter), circleTex);
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(knobPos.x - knobRadius, knobPos.y - knobRadius, knobDiameter, knobDiameter), ringTex);

            if (showLabels)
            {
                smallTextStyle.fontSize = 10;
                smallTextStyle.alignment = TextAnchor.MiddleCenter;
                smallTextStyle.normal.textColor = new Color(0.7f, 0.88f, 1f);
                GUI.Label(new Rect(joyCenter.x - 70, joyCenter.y + joyRadius + 4, 140, 16), "Joystick (Move)", smallTextStyle);
                smallTextStyle.alignment = TextAnchor.MiddleLeft;
            }

            // Backpack Button (bottom-left)
            float bpSize = 36f * buttonScale;
            Rect bpRect = new Rect(28f, sh - 46f, bpSize, bpSize);
            GUI.color = new Color(0.06f, 0.10f, 0.16f, buttonOpacity);
            GUI.DrawTexture(bpRect, circleTex);
            GUI.color = new Color(0.3f, 0.5f, 0.7f, buttonOpacity);
            GUI.DrawTexture(bpRect, ringTex);
            badgeStyle.fontSize = (int)(bpSize * 0.45f);
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(bpRect, "🎒", badgeStyle);
            if (showLabels)
            {
                smallTextStyle.fontSize = 8;
                smallTextStyle.alignment = TextAnchor.MiddleCenter;
                smallTextStyle.normal.textColor = Color.white;
                GUI.Label(new Rect(28f - 10, sh - 14f, bpSize + 20, 12), "Backpack", smallTextStyle);
                smallTextStyle.alignment = TextAnchor.MiddleLeft;
            }

            // ==========================================================
            // 2. BOTTOM PRESETS BAR (Matching Reference Image!)
            // ==========================================================
            float presetBarY = sh - 32f;
            float barStartX = sw * 0.20f;
            
            // "Layout Presets (Example) v" dropdown pill
            float dropdownW = 145f;
            Rect dropRect = new Rect(barStartX, presetBarY, dropdownW, 22f);
            GUI.color = new Color(0.06f, 0.10f, 0.16f, 0.90f);
            GUI.DrawTexture(dropRect, darkBoxTex);
            GUI.color = Color.white;
            badgeStyle.fontSize = 9;
            badgeStyle.fontStyle = FontStyle.Bold;
            badgeStyle.normal.textColor = new Color(0.8f, 0.9f, 1.0f);
            GUI.Label(dropRect, "Layout Presets (Example)  ▾", badgeStyle);

            // Preset pills
            string[] presetNames = { "Classic (Default)", "Two Thumb", "Left Hand", "Right Hand", "Large Buttons", "Compact" };
            float curX = barStartX + dropdownW + 8f;
            for (int p = 0; p < presetNames.Length; p++)
            {
                float pillW = (p == 0) ? 105f : 88f;
                Rect pRect = new Rect(curX, presetBarY, pillW, 22f);
                bool isSelected = (currentPresetIndex == p);
                GUI.color = isSelected ? new Color(0.1f, 0.5f, 0.92f, 0.95f) : new Color(0.06f, 0.10f, 0.16f, 0.85f);
                GUI.DrawTexture(pRect, darkBoxTex);
                GUI.color = Color.white;
                badgeStyle.fontSize = 9;
                badgeStyle.normal.textColor = isSelected ? Color.white : new Color(0.65f, 0.75f, 0.88f);
                GUI.Label(pRect, presetNames[p], badgeStyle);

                if (Event.current.type == EventType.MouseDown && pRect.Contains(Event.current.mousePosition))
                {
                    ApplyPreset(p);
                    WCTAudioSynthesizer.Instance?.PlayUIClick();
                }

                curX += pillW + 6f;
            }

            // Plus (+) custom preset button
            Rect plusRect = new Rect(curX, presetBarY, 24f, 22f);
            GUI.color = new Color(0.06f, 0.10f, 0.16f, 0.85f);
            GUI.DrawTexture(plusRect, darkBoxTex);
            GUI.color = Color.white;
            badgeStyle.fontSize = 11;
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(plusRect, "+", badgeStyle);
            if (Event.current.type == EventType.MouseDown && plusRect.Contains(Event.current.mousePosition))
            {
                isCustomizeControlsOpen = true;
                WCTAudioSynthesizer.Instance?.PlayUIClick();
            }

            // ==========================================================
            // 3. RIGHT SIDE ACTION CONTROLS (Catch/Tag, Jump, Slide, TPP)
            // ==========================================================
            float rightAnchorX = sw - 30f;
            float rightAnchorY = sh - 30f;

            // Large Catch / Tag Button (Primary thumb position)
            float catchSize = 82f * buttonScale;
            float catchX = rightAnchorX - 100f * buttonScale;
            float catchY = rightAnchorY - 140f * buttonScale;
            Rect catchRect = new Rect(catchX, catchY, catchSize, catchSize);
            bool catchPressed = Input.GetKey(KeyCode.C) || Input.GetMouseButton(0);

            // Pulsing orange highlight
            GUI.color = catchPressed ? new Color(0.96f, 0.38f, 0.08f, buttonOpacity) : new Color(0.18f, 0.08f, 0.04f, buttonOpacity);
            GUI.DrawTexture(catchRect, circleTex);
            GUI.color = new Color(0.96f, 0.42f, 0.08f, buttonOpacity);
            GUI.DrawTexture(catchRect, ringTex);

            float pulse = (Mathf.Sin(Time.time * 7f) + 1f) * 0.5f;
            GUI.color = new Color(0.96f, 0.45f, 0.08f, 0.25f + pulse * 0.35f);
            GUI.DrawTexture(new Rect(catchX - 4, catchY - 4, catchSize + 8, catchSize + 8), ringTex);

            badgeStyle.fontSize = (int)(catchSize * 0.42f);
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(catchX, catchY + catchSize * 0.20f, catchSize, catchSize * 0.45f), "✋", badgeStyle);

            if (showLabels)
            {
                smallTextStyle.fontSize = 10;
                smallTextStyle.fontStyle = FontStyle.Bold;
                smallTextStyle.alignment = TextAnchor.MiddleCenter;
                smallTextStyle.normal.textColor = new Color(1.0f, 0.72f, 0.45f);
                GUI.Label(new Rect(catchX - 15, catchY + catchSize + 2, catchSize + 30, 14), "Catch / Tag", smallTextStyle);
                smallTextStyle.alignment = TextAnchor.MiddleLeft;
            }

            if (Event.current.type == EventType.MouseDown && catchRect.Contains(Event.current.mousePosition))
            {
                if (playerController != null) playerController.TriggerCatchOrTag();
            }

            // Jump Button
            float jumpSize = 64f * buttonScale;
            float jumpX = rightAnchorX - 225f * buttonScale;
            float jumpY = rightAnchorY - 85f * buttonScale;
            Rect jumpRect = new Rect(jumpX, jumpY, jumpSize, jumpSize);
            bool jumpPressed = Input.GetKey(KeyCode.Space);

            GUI.color = jumpPressed ? new Color(0.12f, 0.45f, 0.85f, buttonOpacity) : new Color(0.04f, 0.08f, 0.14f, buttonOpacity);
            GUI.DrawTexture(jumpRect, circleTex);
            GUI.color = new Color(0.25f, 0.55f, 0.85f, buttonOpacity);
            GUI.DrawTexture(jumpRect, ringTex);
            badgeStyle.fontSize = (int)(jumpSize * 0.38f);
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(jumpX, jumpY + jumpSize * 0.22f, jumpSize, jumpSize * 0.45f), "🏃", badgeStyle);

            if (showLabels)
            {
                smallTextStyle.fontSize = 10;
                smallTextStyle.fontStyle = FontStyle.Bold;
                smallTextStyle.alignment = TextAnchor.MiddleCenter;
                smallTextStyle.normal.textColor = Color.white;
                GUI.Label(new Rect(jumpX - 10, jumpY + jumpSize + 2, jumpSize + 20, 14), "Jump", smallTextStyle);
                smallTextStyle.alignment = TextAnchor.MiddleLeft;
            }

            if (Event.current.type == EventType.MouseDown && jumpRect.Contains(Event.current.mousePosition))
            {
                if (playerController != null) playerController.TriggerJump();
            }

            // Slide Button
            float slideSize = 64f * buttonScale;
            float slideX = rightAnchorX - 150f * buttonScale;
            float slideY = rightAnchorY - 72f * buttonScale;
            Rect slideRect = new Rect(slideX, slideY, slideSize, slideSize);
            bool slidePressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftAlt);

            GUI.color = slidePressed ? new Color(0.2f, 0.7f, 0.6f, buttonOpacity) : new Color(0.04f, 0.08f, 0.14f, buttonOpacity);
            GUI.DrawTexture(slideRect, circleTex);
            GUI.color = new Color(0.25f, 0.65f, 0.55f, buttonOpacity);
            GUI.DrawTexture(slideRect, ringTex);
            badgeStyle.fontSize = (int)(slideSize * 0.38f);
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(slideX, slideY + slideSize * 0.22f, slideSize, slideSize * 0.45f), "🤸", badgeStyle);

            if (showLabels)
            {
                smallTextStyle.fontSize = 10;
                smallTextStyle.fontStyle = FontStyle.Bold;
                smallTextStyle.alignment = TextAnchor.MiddleCenter;
                smallTextStyle.normal.textColor = Color.white;
                GUI.Label(new Rect(slideX - 10, slideY + slideSize + 2, slideSize + 20, 14), "Slide", smallTextStyle);
                smallTextStyle.alignment = TextAnchor.MiddleLeft;
            }

            if (Event.current.type == EventType.MouseDown && slideRect.Contains(Event.current.mousePosition))
            {
                if (playerController != null) playerController.TriggerSlideOrCrouch();
            }

            // Mini Vertical Action Buttons (Climb Up ▲, Climb Down ▼, Prone 🤾)
            float miniW = 34f * buttonScale;
            float miniH = 26f * buttonScale;
            float miniX = rightAnchorX - 68f;

            // Climb Up ▲
            Rect climbUpRect = new Rect(miniX, rightAnchorY - 110f, miniW, miniH);
            GUI.color = new Color(0.06f, 0.10f, 0.16f, buttonOpacity);
            GUI.DrawTexture(climbUpRect, darkBoxTex);
            GUI.color = Color.white;
            badgeStyle.fontSize = 11;
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(climbUpRect, "▲", badgeStyle);
            if (Event.current.type == EventType.MouseDown && climbUpRect.Contains(Event.current.mousePosition))
            {
                if (playerController != null) playerController.TriggerClimb();
            }

            // Climb Down ▼
            Rect climbDownRect = new Rect(miniX, rightAnchorY - 80f, miniW, miniH);
            GUI.color = new Color(0.06f, 0.10f, 0.16f, buttonOpacity);
            GUI.DrawTexture(climbDownRect, darkBoxTex);
            GUI.color = Color.white;
            GUI.Label(climbDownRect, "▼", badgeStyle);
            if (Event.current.type == EventType.MouseDown && climbDownRect.Contains(Event.current.mousePosition))
            {
                if (playerController != null) playerController.TriggerDrop();
            }

            // Prone / Dive
            Rect proneRect = new Rect(miniX - 4f, rightAnchorY - 50f, miniW + 8f, miniH);
            GUI.color = new Color(0.06f, 0.10f, 0.16f, buttonOpacity);
            GUI.DrawTexture(proneRect, darkBoxTex);
            GUI.color = Color.white;
            badgeStyle.fontSize = 10;
            GUI.Label(proneRect, "🤾", badgeStyle);
            if (Event.current.type == EventType.MouseDown && proneRect.Contains(Event.current.mousePosition))
            {
                if (playerController != null) playerController.TriggerDive();
            }

            // View (TPP/FPP) Toggle Button
            float viewSize = 48f * buttonScale;
            float viewX = rightAnchorX - 105f * buttonScale;
            float viewY = rightAnchorY - 225f * buttonScale;
            Rect viewRect = new Rect(viewX, viewY, viewSize, viewSize);
            bool isTPP = (playerController != null && playerController.IsThirdPerson);

            GUI.color = isTPP ? new Color(0.1f, 0.55f, 0.9f, buttonOpacity) : new Color(0.05f, 0.08f, 0.14f, buttonOpacity);
            GUI.DrawTexture(viewRect, circleTex);
            GUI.color = new Color(0.2f, 0.5f, 0.8f, buttonOpacity);
            GUI.DrawTexture(viewRect, ringTex);

            badgeStyle.fontSize = (int)(viewSize * 0.32f);
            badgeStyle.fontStyle = FontStyle.Bold;
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(viewX, viewY + viewSize * 0.22f, viewSize, viewSize * 0.45f), isTPP ? "TPP" : "FPP", badgeStyle);

            if (showLabels)
            {
                smallTextStyle.fontSize = 9;
                smallTextStyle.alignment = TextAnchor.MiddleCenter;
                smallTextStyle.normal.textColor = Color.white;
                GUI.Label(new Rect(viewX - 20, viewY + viewSize + 2, viewSize + 40, 14), "View (TPP/FPP)", smallTextStyle);
                smallTextStyle.alignment = TextAnchor.MiddleLeft;
            }

            if (Event.current.type == EventType.MouseDown && viewRect.Contains(Event.current.mousePosition))
            {
                if (playerController != null) playerController.TogglePerspective();
            }

            // Eye / Free Look Button
            float eyeSize = 34f * buttonScale;
            Rect eyeRect = new Rect(rightAnchorX - 44f, rightAnchorY - 220f, eyeSize, eyeSize);
            GUI.color = new Color(0.06f, 0.10f, 0.16f, buttonOpacity);
            GUI.DrawTexture(eyeRect, circleTex);
            GUI.color = new Color(0.3f, 0.5f, 0.7f, buttonOpacity);
            GUI.DrawTexture(eyeRect, ringTex);
            badgeStyle.fontSize = 13;
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(eyeRect, "👁", badgeStyle);

            // Quick Chat Button
            Rect chatRect = new Rect(rightAnchorX - 44f, rightAnchorY - 175f, eyeSize, eyeSize);
            GUI.color = new Color(0.06f, 0.10f, 0.16f, buttonOpacity);
            GUI.DrawTexture(chatRect, circleTex);
            GUI.color = new Color(0.3f, 0.5f, 0.7f, buttonOpacity);
            GUI.DrawTexture(chatRect, ringTex);
            badgeStyle.fontSize = 12;
            GUI.Label(chatRect, "💬", badgeStyle);
            if (Event.current.type == EventType.MouseDown && chatRect.Contains(Event.current.mousePosition))
            {
                DisplayBanner("TEAM ALPHA: Keep pressure on the perimeter!", 2.0f);
            }

            // ==========================================================
            // 4. "CUSTOMIZE CONTROLS" RIGHT-HAND DRAWER PANEL
            // ==========================================================
            if (isCustomizeControlsOpen)
            {
                DrawCustomizeControlsDrawer(sw, sh);
            }

            GUI.color = baseCol;
        }

        // ==============================================================
        // "CUSTOMIZE CONTROLS" DRAWER PANEL (Matching Reference Image!)
        // ==============================================================
        private void DrawCustomizeControlsDrawer(float sw, float sh)
        {
            float panelW = 320f;
            float panelH = sh - 30f;
            float panelX = sw - panelW - 15f;
            float panelY = 15f;
            Rect panelRect = new Rect(panelX, panelY, panelW, panelH);

            // Frosted background card
            GUI.color = new Color(0.05f, 0.08f, 0.14f, 0.96f);
            GUI.DrawTexture(panelRect, darkBoxTex);
            GUI.color = new Color(0.1f, 0.65f, 0.95f, 0.95f);
            GUI.DrawTexture(new Rect(panelX, panelY, panelW, 3), Texture2D.whiteTexture);
            GUI.color = Color.white;

            float curY = panelY + 14f;

            // Header: CUSTOMIZE CONTROLS + [X] Close
            titleStyle.fontSize = 16;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(panelX + 16, curY, 220, 22), "CUSTOMIZE CONTROLS", titleStyle);

            Rect closeRect = new Rect(panelX + panelW - 32, curY, 22, 22);
            badgeStyle.fontSize = 13;
            badgeStyle.normal.textColor = new Color(0.7f, 0.8f, 0.9f);
            GUI.Label(closeRect, "✕", badgeStyle);
            if (Event.current.type == EventType.MouseDown && closeRect.Contains(Event.current.mousePosition))
            {
                isCustomizeControlsOpen = false;
                WCTAudioSynthesizer.Instance?.PlayUIClick();
            }

            curY += 22f;
            smallTextStyle.fontSize = 10;
            smallTextStyle.normal.textColor = new Color(0.6f, 0.75f, 0.9f);
            GUI.Label(new Rect(panelX + 16, curY, panelW - 32, 16), "Drag and move buttons to adjust your layout", smallTextStyle);

            curY += 22f;

            // Control Preset Dropdown
            float dropW = panelW - 32f;
            Rect presetDropRect = new Rect(panelX + 16, curY, dropW, 26f);
            GUI.color = new Color(0.08f, 0.12f, 0.20f, 0.9f);
            GUI.DrawTexture(presetDropRect, darkBoxTex);
            GUI.color = Color.white;

            smallTextStyle.fontSize = 11;
            smallTextStyle.normal.textColor = new Color(0.75f, 0.85f, 1.0f);
            GUI.Label(new Rect(panelX + 24, curY + 4, 120, 18), "Control Preset", smallTextStyle);

            badgeStyle.fontSize = 11;
            badgeStyle.normal.textColor = Color.white;
            string[] presetLabels = { "Classic 1", "Two Thumb", "Left Hand", "Right Hand", "Large Buttons", "Compact" };
            GUI.Label(new Rect(panelX + dropW - 80, curY + 4, 90, 18), $"{presetLabels[currentPresetIndex]}  ▾", badgeStyle);

            if (Event.current.type == EventType.MouseDown && presetDropRect.Contains(Event.current.mousePosition))
            {
                currentPresetIndex = (currentPresetIndex + 1) % presetLabels.Length;
                ApplyPreset(currentPresetIndex);
                WCTAudioSynthesizer.Instance?.PlayUIClick();
            }

            curY += 34f;

            // Category Tabs: Move, Action, View, Combat
            string[] tabNames = { "Move", "Action", "View", "Combat" };
            float tabW = (panelW - 32f) / 4f;
            for (int t = 0; t < tabNames.Length; t++)
            {
                Rect tr = new Rect(panelX + 16 + t * tabW, curY, tabW - 4, 26f);
                bool isSel = (customizeTabIndex == t);
                GUI.color = isSel ? new Color(0.12f, 0.55f, 0.95f, 0.95f) : new Color(0.08f, 0.12f, 0.20f, 0.85f);
                GUI.DrawTexture(tr, darkBoxTex);
                GUI.color = Color.white;

                badgeStyle.fontSize = 11;
                badgeStyle.normal.textColor = isSel ? Color.white : new Color(0.6f, 0.75f, 0.9f);
                GUI.Label(tr, tabNames[t], badgeStyle);

                if (Event.current.type == EventType.MouseDown && tr.Contains(Event.current.mousePosition))
                {
                    customizeTabIndex = t;
                    WCTAudioSynthesizer.Instance?.PlayUIClick();
                }
            }

            curY += 36f;

            // Sliders:
            // 1. Joystick Size (50% - 150%)
            DrawCustomizerSlider(panelX + 16, ref curY, panelW - 32, "Joystick Size", ref joystickScale, 0.70f, 1.50f, "{0:0}%", 100f);

            // 2. Joystick Opacity (20% - 100%)
            DrawCustomizerSlider(panelX + 16, ref curY, panelW - 32, "Joystick Opacity", ref joystickOpacity, 0.20f, 1.0f, "{0:0}%", 100f);

            // 3. Button Size (50% - 150%)
            DrawCustomizerSlider(panelX + 16, ref curY, panelW - 32, "Button Size", ref buttonScale, 0.70f, 1.50f, "{0:0}%", 100f);

            // 4. Button Opacity (20% - 100%)
            DrawCustomizerSlider(panelX + 16, ref curY, panelW - 32, "Button Opacity", ref buttonOpacity, 0.20f, 1.0f, "{0:0}%", 100f);

            curY += 6f;

            // Toggles:
            DrawCustomizerToggle(panelX + 16, ref curY, panelW - 32, "Show Labels", ref showLabels);
            DrawCustomizerToggle(panelX + 16, ref curY, panelW - 32, "Auto Run Button", ref autoRunEnabled);
            DrawCustomizerToggle(panelX + 16, ref curY, panelW - 32, "Quick Sprint (Double Tap)", ref quickSprintEnabled);
            DrawCustomizerToggle(panelX + 16, ref curY, panelW - 32, "Hold to Crouch", ref holdToCrouch);
            DrawCustomizerToggle(panelX + 16, ref curY, panelW - 32, "Tap to Slide", ref tapToSlide);

            curY += 10f;

            // Layout Management: Save Layout | Reset | Default
            float btnW = (panelW - 32f - 12f) / 3f;
            float btnH = 28f;

            // Save Layout (Blue)
            Rect saveRect = new Rect(panelX + 16, curY, btnW, btnH);
            GUI.color = new Color(0.1f, 0.52f, 0.95f, 0.95f);
            GUI.DrawTexture(saveRect, darkBoxTex);
            GUI.color = Color.white;
            badgeStyle.fontSize = 10;
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(saveRect, "Save Layout", badgeStyle);
            if (Event.current.type == EventType.MouseDown && saveRect.Contains(Event.current.mousePosition))
            {
                SaveLayoutSettings();
                saveNotificationTimer = 2.5f;
                WCTAudioSynthesizer.Instance?.PlayUIClick();
            }

            // Reset (Dark)
            Rect resetRect = new Rect(panelX + 16 + btnW + 6f, curY, btnW, btnH);
            GUI.color = new Color(0.12f, 0.16f, 0.24f, 0.95f);
            GUI.DrawTexture(resetRect, darkBoxTex);
            GUI.color = Color.white;
            badgeStyle.normal.textColor = new Color(0.8f, 0.9f, 1f);
            GUI.Label(resetRect, "Reset", badgeStyle);
            if (Event.current.type == EventType.MouseDown && resetRect.Contains(Event.current.mousePosition))
            {
                LoadLayoutSettings();
                WCTAudioSynthesizer.Instance?.PlayUIClick();
            }

            // Default (Red accent with reload icon)
            Rect defRect = new Rect(panelX + 16 + (btnW + 6f) * 2f, curY, btnW, btnH);
            GUI.color = new Color(0.75f, 0.18f, 0.15f, 0.95f);
            GUI.DrawTexture(defRect, darkBoxTex);
            GUI.color = Color.white;
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(defRect, "🔄 Default", badgeStyle);
            if (Event.current.type == EventType.MouseDown && defRect.Contains(Event.current.mousePosition))
            {
                ResetToDefaults();
                WCTAudioSynthesizer.Instance?.PlayUIClick();
            }

            // Feedback banner if saved
            if (saveNotificationTimer > 0f)
            {
                Rect noteRect = new Rect(panelX + 16, curY + btnH + 6f, panelW - 32, 22f);
                GUI.color = new Color(0.08f, 0.55f, 0.25f, 0.95f);
                GUI.DrawTexture(noteRect, darkBoxTex);
                GUI.color = Color.white;
                badgeStyle.fontSize = 9;
                GUI.Label(noteRect, "✓ LAYOUT SAVED SUCCESSFULLY!", badgeStyle);
            }
        }

        private void DrawCustomizerSlider(float x, ref float y, float w, string label, ref float value, float minVal, float maxVal, string format, float multiplier)
        {
            smallTextStyle.fontSize = 11;
            smallTextStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(x, y, w - 50, 18), label, smallTextStyle);

            badgeStyle.fontSize = 11;
            badgeStyle.normal.textColor = new Color(0.7f, 0.85f, 1.0f);
            GUI.Label(new Rect(x + w - 45, y, 45, 18), string.Format(format, value * multiplier), badgeStyle);

            y += 18f;

            // Slider Track
            float trackH = 4f;
            float trackY = y + 7f;
            GUI.color = new Color(0.12f, 0.22f, 0.35f, 0.9f);
            GUI.DrawTexture(new Rect(x, trackY, w, trackH), Texture2D.whiteTexture);

            // Fill
            float frac = Mathf.Clamp01((value - minVal) / (maxVal - minVal));
            GUI.color = new Color(0.1f, 0.65f, 0.95f, 0.95f);
            GUI.DrawTexture(new Rect(x, trackY, w * frac, trackH), Texture2D.whiteTexture);

            // Knob
            float knobX = x + w * frac - 7f;
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(knobX, y + 2, 14, 14), circleTex);

            // Mouse interaction
            Rect slideRect = new Rect(x, y, w, 18);
            if (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag)
            {
                if (slideRect.Contains(Event.current.mousePosition))
                {
                    float mouseFrac = Mathf.Clamp01((Event.current.mousePosition.x - x) / w);
                    value = Mathf.Lerp(minVal, maxVal, mouseFrac);
                }
            }

            y += 24f;
        }

        private void DrawCustomizerToggle(float x, ref float y, float w, string label, ref bool state)
        {
            smallTextStyle.fontSize = 11;
            smallTextStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(x, y, w - 50, 20), label, smallTextStyle);

            // Modern slider switch
            float swW = 38f;
            float swH = 18f;
            float swX = x + w - swW;
            Rect swRect = new Rect(swX, y + 1, swW, swH);

            GUI.color = state ? new Color(0.1f, 0.65f, 0.95f, 0.95f) : new Color(0.12f, 0.16f, 0.24f, 0.9f);
            GUI.DrawTexture(swRect, darkBoxTex);

            float thumbX = state ? (swX + swW - 16f) : (swX + 2f);
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(thumbX, y + 2, 14, 14), circleTex);

            if (Event.current.type == EventType.MouseDown && swRect.Contains(Event.current.mousePosition))
            {
                state = !state;
                WCTAudioSynthesizer.Instance?.PlayUIClick();
            }

            y += 24f;
        }

        private void ApplyPreset(int index)
        {
            currentPresetIndex = index;
            switch (index)
            {
                case 0: // Classic (Default)
                    joystickScale = 1.20f;
                    joystickOpacity = 0.80f;
                    buttonScale = 1.10f;
                    buttonOpacity = 0.85f;
                    break;
                case 1: // Two Thumb
                    joystickScale = 1.30f;
                    joystickOpacity = 0.85f;
                    buttonScale = 1.25f;
                    buttonOpacity = 0.90f;
                    break;
                case 2: // Left Hand
                    joystickScale = 1.15f;
                    buttonScale = 1.15f;
                    break;
                case 3: // Right Hand
                    joystickScale = 1.20f;
                    buttonScale = 1.10f;
                    break;
                case 4: // Large Buttons
                    joystickScale = 1.35f;
                    buttonScale = 1.40f;
                    joystickOpacity = 0.90f;
                    buttonOpacity = 0.90f;
                    break;
                case 5: // Compact
                    joystickScale = 0.90f;
                    buttonScale = 0.85f;
                    joystickOpacity = 0.75f;
                    buttonOpacity = 0.80f;
                    break;
            }
        }

        private void SaveLayoutSettings()
        {
            PlayerPrefs.SetFloat("WCT_JoyScale", joystickScale);
            PlayerPrefs.SetFloat("WCT_JoyOpacity", joystickOpacity);
            PlayerPrefs.SetFloat("WCT_BtnScale", buttonScale);
            PlayerPrefs.SetFloat("WCT_BtnOpacity", buttonOpacity);
            PlayerPrefs.SetInt("WCT_ShowLabels", showLabels ? 1 : 0);
            PlayerPrefs.SetInt("WCT_AutoRun", autoRunEnabled ? 1 : 0);
            PlayerPrefs.SetInt("WCT_TapSlide", tapToSlide ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void LoadLayoutSettings()
        {
            joystickScale = PlayerPrefs.GetFloat("WCT_JoyScale", 1.20f);
            joystickOpacity = PlayerPrefs.GetFloat("WCT_JoyOpacity", 0.80f);
            buttonScale = PlayerPrefs.GetFloat("WCT_BtnScale", 1.10f);
            buttonOpacity = PlayerPrefs.GetFloat("WCT_BtnOpacity", 0.85f);
            showLabels = PlayerPrefs.GetInt("WCT_ShowLabels", 1) == 1;
            autoRunEnabled = PlayerPrefs.GetInt("WCT_AutoRun", 1) == 1;
            tapToSlide = PlayerPrefs.GetInt("WCT_TapSlide", 1) == 1;
        }

        private void ResetToDefaults()
        {
            ApplyPreset(0);
            showLabels = true;
            autoRunEnabled = true;
            quickSprintEnabled = true;
            holdToCrouch = false;
            tapToSlide = true;
        }

        // ==============================================================
        // GAME PAUSED OVERLAY (Image 3)
        // ==============================================================
        private void DrawPauseMenu(float sw, float sh)
        {
            // Dimmed semi-transparent overlay
            GUI.DrawTexture(new Rect(0, 0, sw, sh), new Texture2D(1, 1));

            // Top Broadcast header remains on top
            DrawTopBroadcastHeader(sw);

            // Left Side: Mini-Map + GAME PAUSED Card (Image 3)
            DrawTopRightHeaderAndMinimap(sw, sh);

            float cardW = 340f;
            float cardH = 360f;
            float cardX = 175f;
            float cardY = 120f;

            GUI.DrawTexture(new Rect(cardX, cardY, cardW, cardH), darkBoxTex);
            GUI.DrawTexture(new Rect(cardX, cardY, cardW, 3), alphaCyanTex);

            headerStyle.fontSize = 20;
            headerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(cardX + 20, cardY + 14, cardW - 40, 24), "GAME PAUSED", headerStyle);

            smallTextStyle.fontSize = 11;
            smallTextStyle.normal.textColor = new Color(0.6f, 0.7f, 0.85f);
            GUI.Label(new Rect(cardX + 20, cardY + 38, cardW - 40, 18), $"CHASE {matchController.ChaseNumber} / {matchController.MaxChases}  |  ALPHA {matchController.TeamAScore} - {matchController.TeamBScore} OMEGA", smallTextStyle);

            // 5 Menu Buttons (Image 3)
            float btnY = cardY + 68f;
            float btnH = 44f;
            float spacing = 52f;

            if (DrawPauseButton(new Rect(cardX + 16, btnY, cardW - 32, btnH), "> RESUME", "Continue the current chase", true))
            {
                TogglePause();
            }

            btnY += spacing;
            if (DrawPauseButton(new Rect(cardX + 16, btnY, cardW - 32, btnH), "RESTART CHASE", "Restart this chase", false))
            {
                TogglePause();
                matchController.StartMatch(matchController.Mode, matchController.Difficulty);
            }

            btnY += spacing;
            if (DrawPauseButton(new Rect(cardX + 16, btnY, cardW - 32, btnH), "SETTINGS", "Adjust game settings", false))
            {
                TogglePause();
                WCTMenuController.Instance?.OpenScreen(MenuScreen.Settings);
            }

            btnY += spacing;
            if (DrawPauseButton(new Rect(cardX + 16, btnY, cardW - 32, btnH), "VIEW CONTROLS", "Keyboard & mouse controls", false))
            {
                TogglePause();
                WCTMenuController.Instance?.OpenScreen(MenuScreen.Controls);
            }

            btnY += spacing;
            if (DrawPauseButton(new Rect(cardX + 16, btnY, cardW - 32, btnH), "MAIN MENU", "Exit to main menu", false))
            {
                TogglePause();
                WCTMenuController.Instance?.OpenScreen(MenuScreen.MainMenu);
            }

            // Bottom-Left Tip Box (Image 3)
            float tipY = cardY + cardH + 20f;
            GUI.DrawTexture(new Rect(cardX, tipY, cardW, 60), darkBoxTex);
            badgeStyle.fontSize = 10;
            badgeStyle.normal.textColor = Color.cyan;
            GUI.Label(new Rect(cardX + 10, tipY + 6, 60, 16), "(i) TIP:", badgeStyle);
            smallTextStyle.fontSize = 10;
            smallTextStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(cardX + 10, tipY + 22, cardW - 20, 36), "Use arena obstacles and multiple routes to create distance from the chaser. Keep an eye on stamina.", smallTextStyle);

            // Right Side: CURRENT MATCH Card (Image 3)
            float rightW = 340f;
            float rightX = sw - rightW - 175f;
            DrawCurrentMatchCard(new Rect(rightX, cardY, rightW, cardH));

            // Bottom-Right: QUICK CONTROLS REMINDER (Image 3)
            float remW = 540f;
            float remH = 50f;
            float remX = sw - remW - 50f;
            float remY = sh - remH - 20f;

            GUI.DrawTexture(new Rect(remX, remY, remW, remH), darkBoxTex);
            badgeStyle.fontSize = 10;
            badgeStyle.normal.textColor = new Color(0.7f, 0.8f, 0.9f);
            GUI.Label(new Rect(remX + 10, remY + 6, remW - 20, 16), "QUICK CONTROLS REMINDER", badgeStyle);
            smallTextStyle.fontSize = 10;
            smallTextStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(remX + 10, remY + 24, remW - 20, 20), "[W A S D] Move  •  [Shift] Sprint  •  [Space] Jump/Vault  •  [Ctrl] Slide  •  [C] Catch/Tag  •  [V] View TPP/FPP  •  [Esc] Pause", smallTextStyle);
        }

        private bool DrawPauseButton(Rect rect, string mainText, string subText, bool isActive)
        {
            GUI.DrawTexture(rect, isActive ? bluePillTex : darkBoxTex);
            headerStyle.fontSize = 13;
            headerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(rect.x + 14, rect.y + 6, rect.width - 28, 18), mainText, headerStyle);

            smallTextStyle.fontSize = 10;
            smallTextStyle.normal.textColor = isActive ? new Color(0.85f, 0.95f, 1.0f) : new Color(0.6f, 0.7f, 0.8f);
            GUI.Label(new Rect(rect.x + 14, rect.y + 24, rect.width - 28, 16), subText, smallTextStyle);

            bool clicked = GUI.Button(rect, GUIContent.none, GUIStyle.none);
            if (clicked) WCTAudioSynthesizer.Instance?.PlayUIClick();
            return clicked;
        }

        private void DrawCurrentMatchCard(Rect rect)
        {
            GUI.DrawTexture(rect, darkBoxTex);
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 3), alphaCyanTex);

            headerStyle.fontSize = 16;
            headerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(rect.x + 18, rect.y + 14, rect.width - 36, 22), "CURRENT MATCH", headerStyle);

            // Arena Photo box
            float prevH = 100f;
            GUI.DrawTexture(new Rect(rect.x + 16, rect.y + 40, rect.width - 32, prevH), panelBgTex);
            badgeStyle.fontSize = 12;
            badgeStyle.normal.textColor = Color.cyan;
            GUI.Label(new Rect(rect.x + 16, rect.y + 40 + prevH - 24, rect.width - 32, 20), "QUAD (CLASSIC)  12m x 12m", badgeStyle);

            // Match details list
            float listY = rect.y + 155f;
            float rowH = 24f;
            DrawPauseDetailRow(rect.x + 18, listY, rect.width - 36, "Team:", $"ALPHA {matchController.TeamAScore} - {matchController.TeamBScore} OMEGA");
            DrawPauseDetailRow(rect.x + 18, listY + rowH, rect.width - 36, "Chase Number:", $"{matchController.ChaseNumber} / {matchController.MaxChases}");
            DrawPauseDetailRow(rect.x + 18, listY + rowH * 2, rect.width - 36, "Your Role:", playerController != null ? playerController.CurrentRole.ToString() : "Evader");
            DrawPauseDetailRow(rect.x + 18, listY + rowH * 3, rect.width - 36, "Chase Time:", $"00:{(int)matchController.TimeRemaining:00} / 20s");
            DrawPauseDetailRow(rect.x + 18, listY + rowH * 4, rect.width - 36, "AI Difficulty:", matchController.Difficulty.ToString());
            DrawPauseDetailRow(rect.x + 18, listY + rowH * 5, rect.width - 36, "Arena:", "Quad (Classic)");
        }

        private void DrawPauseDetailRow(float x, float y, float w, string label, string val)
        {
            smallTextStyle.fontSize = 11;
            smallTextStyle.normal.textColor = new Color(0.6f, 0.7f, 0.85f);
            GUI.Label(new Rect(x, y, 110, 20), label, smallTextStyle);

            smallTextStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(x + 115, y, w - 115, 20), val, smallTextStyle);
        }

        // ==============================================================
        // CHASE RESULT OVERLAYS (Images 4 & 5)
        // ==============================================================
        private void DrawChaseResultOverlay(float sw, float sh)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // Fullscreen backdrop overlay (dims arena so modal stands out)
            GUI.color = new Color(0.04f, 0.06f, 0.09f, 0.90f);
            GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Top Broadcast header stays on
            DrawTopBroadcastHeader(sw);

            bool isEvaded = matchController.LastOutcome == ChaseOutcome.EvaderSurvived;

            float totalW = 740f;
            float cardW = 350f;
            float cardX = (sw - totalW) * 0.5f;
            float rightX = cardX + cardW + 40f;
            float cardY = 120f;

            // Large Title Banner
            titleStyle.fontSize = 32;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = isEvaded ? new Color(0.1f, 0.88f, 1.0f) : new Color(0.95f, 0.25f, 0.2f);
            string titleText = isEvaded ? "TIME'S UP — EVADER ESCAPES!" : (matchController.LastOutcome == ChaseOutcome.Tagged ? "TAGGED — CHASER WINS!" : "COURT FAULT — OUT OF BOUNDS!");
            GUI.Label(new Rect((sw - 740f) * 0.5f, cardY - 45f, 740f, 38f), titleText, titleStyle);

            subtitleStyle.fontSize = 13;
            subtitleStyle.alignment = TextAnchor.MiddleCenter;
            subtitleStyle.normal.textColor = Color.white;
            string subText = isEvaded ? "+1 EVASION POINT AWARDED  •  SURVIVED THE FULL 20.0s" : (matchController.LastOutcome == ChaseOutcome.Tagged ? "CHASER SCORES VALID TAG  •  NO EVASION POINTS AWARDED" : "STEPPED OUT OF 12m ARENA BOUNDARY");
            GUI.Label(new Rect((sw - 740f) * 0.5f, cardY - 10f, 740f, 22f), subText, subtitleStyle);
            titleStyle.alignment = TextAnchor.MiddleLeft;
            subtitleStyle.alignment = TextAnchor.MiddleLeft;

            // Left Stats Card
            float statY = cardY + 22f;
            GUI.DrawTexture(new Rect(cardX, statY, cardW, 230f), darkBoxTex);
            GUI.DrawTexture(new Rect(cardX, statY, 4, 230f), isEvaded ? evaderAccentTex : alertRedTex);

            float rowH = 38f;
            float startRY = statY + 16f;

            if (isEvaded)
            {
                DrawResultStatRow(cardX + 16, startRY, cardW - 32, "CHASE TIME", "20.00 / 20s");
                DrawResultStatRow(cardX + 16, startRY + rowH, cardW - 32, "DISTANCE COVERED", "154 meters");
                DrawResultStatRow(cardX + 16, startRY + rowH * 2, cardW - 32, "OBSTACLES USED", "12");
                DrawResultStatRow(cardX + 16, startRY + rowH * 3, cardW - 32, "CLOSE CALLS", "3");
            }
            else
            {
                float chaseDuration = 20.0f - matchController.TimeRemaining;
                DrawResultStatRow(cardX + 16, startRY, cardW - 32, "CHASE TIME", $"{chaseDuration:00.00} / 20s");
                DrawResultStatRow(cardX + 16, startRY + rowH, cardW - 32, "DISTANCE COVERED", "62 meters");
                DrawResultStatRow(cardX + 16, startRY + rowH * 2, cardW - 32, "OBSTACLES USED", "6");
                DrawResultStatRow(cardX + 16, startRY + rowH * 3, cardW - 32, "TOP SPEED", "5.8 m/s");
                DrawResultStatRow(cardX + 16, startRY + rowH * 4, cardW - 32, "TAGGED AT", "Central Platform");
            }

            // Right Tactical Route Map Card (Images 4 & 5)
            float mapW = 350f;
            GUI.DrawTexture(new Rect(rightX, statY, mapW, 230f), darkBoxTex);

            headerStyle.fontSize = 13;
            headerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(rightX + 16, statY + 10, mapW - 32, 20), isEvaded ? "CHASE ROUTE (YOU)" : "CHASE REPLAY MAP", headerStyle);

            // 2D Map Schematic
            float miniSize = 140f;
            float miniX = rightX + 16f;
            float miniY = statY + 35f;
            GUI.DrawTexture(new Rect(miniX, miniY, miniSize, miniSize), panelBgTex);

            // Mountain & Blocks
            GUI.DrawTexture(new Rect(miniX + 45, miniY + 45, 50, 50), darkBoxTex);
            // Trace Route Line (Cyan)
            GUI.color = Color.cyan;
            GUI.DrawTexture(new Rect(miniX + 15, miniY + 70, 70, 3), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(miniX + 85, miniY + 25, 3, 50), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Legend
            float legX = rightX + miniSize + 25f;
            float legY = statY + 45f;
            badgeStyle.fontSize = 10;
            badgeStyle.normal.textColor = Color.green;
            GUI.Label(new Rect(legX, legY, 110, 18), "● Start", badgeStyle);
            badgeStyle.normal.textColor = isEvaded ? Color.cyan : Color.red;
            GUI.Label(new Rect(legX, legY + 24, 110, 18), isEvaded ? "● End (Escaped)" : "● Tagged", badgeStyle);
            badgeStyle.normal.textColor = Color.cyan;
            GUI.Label(new Rect(legX, legY + 48, 110, 18), "— Your Route", badgeStyle);
            badgeStyle.normal.textColor = Color.red;
            GUI.Label(new Rect(legX, legY + 72, 110, 18), "- - Chaser Route", badgeStyle);

            // Bottom Buttons
            float btnW = 230f;
            float btnH = 46f;
            float btnY = sh - 80f;
            float centerBX = (sw - (btnW * 3 + 30f)) * 0.5f;

            if (GUI.Button(new Rect(centerBX, btnY, btnW, btnH), "PLAY REPLAY\nWatch chase camera"))
            {
                WCTAudioSynthesizer.Instance?.PlayUIClick();
            }

            string nextText = $"NEXT CHASE > ({(int)matchController.SubTimer + 1}s)\nPress SPACE or Click";
            if (GUI.Button(new Rect(centerBX + btnW + 15f, btnY, btnW, btnH), nextText, blueBtnStyle))
            {
                matchController.ProceedToNextChase();
                WCTAudioSynthesizer.Instance?.PlayBuzzer();
            }

            if (GUI.Button(new Rect(centerBX + (btnW + 15f) * 2, btnY, btnW, btnH), "MAIN MENU\nReturn to dashboard"))
            {
                WCTMenuController.Instance?.OpenScreen(MenuScreen.MainMenu);
            }
        }

        private void DrawCenterAnnouncementBanner(float sw, float sh)
        {
            float cx = sw * 0.5f;
            float cy = sh * 0.42f;

            bool isNumber = int.TryParse(activeBannerText, out int num);
            float bannerW = isNumber ? 140f : 440f;
            float bannerH = isNumber ? 100f : 56f;

            GUI.color = new Color(0.04f, 0.08f, 0.14f, 0.88f * bannerAlpha);
            GUI.DrawTexture(new Rect(cx - bannerW * 0.5f, cy - bannerH * 0.5f, bannerW, bannerH), darkBoxTex);

            bannerStyle.fontSize = isNumber ? 68 : 34;
            Color textColor = (activeBannerText == "GO!") ? Color.green : (isNumber ? Color.yellow : Color.cyan);
            textColor.a = bannerAlpha;
            bannerStyle.normal.textColor = textColor;

            GUI.Label(new Rect(cx - bannerW * 0.5f, cy - bannerH * 0.5f, bannerW, bannerH), activeBannerText, bannerStyle);
            GUI.color = Color.white;
        }

        private void DrawResultStatRow(float x, float y, float w, string label, string val)
        {
            smallTextStyle.fontSize = 11;
            smallTextStyle.normal.textColor = new Color(0.6f, 0.7f, 0.85f);
            GUI.Label(new Rect(x, y, 160, 18), label, smallTextStyle);

            headerStyle.fontSize = 14;
            headerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(x, y + 16, 160, 20), val, headerStyle);
        }

        // ==============================================================
        // POST-MATCH SCOREBOARD (Image 1, Screen 11)
        // ==============================================================
        private void DrawPostMatchScreen(float sw, float sh)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            float boxW = 860f;
            float boxH = 520f;
            float bx = (sw - boxW) * 0.5f;
            float by = (sh - boxH) * 0.5f;

            GUI.DrawTexture(new Rect(bx, by, boxW, boxH), darkBoxTex);

            bannerStyle.fontSize = 28;
            bannerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(bx, by + 15, boxW, 36), "MATCH COMPLETE", bannerStyle);

            string winnerText = matchController.TeamAScore > matchController.TeamBScore
                ? $"TEAM ALPHA WINS {matchController.TeamAScore} - {matchController.TeamBScore}!"
                : (matchController.TeamBScore > matchController.TeamAScore
                    ? $"TEAM OMEGA WINS {matchController.TeamBScore} - {matchController.TeamAScore}!"
                    : "MATCH TIED!");

            scoreStyle.fontSize = 18;
            scoreStyle.normal.textColor = Color.yellow;
            GUI.Label(new Rect(bx, by + 54, boxW, 26), winnerText, scoreStyle);

            // History Table
            float tx = bx + 40f;
            float ty = by + 95f;
            tableHeaderStyle.fontSize = 13;
            tableHeaderStyle.normal.textColor = Color.cyan;
            GUI.Label(new Rect(tx, ty, 60, 22), "CHASE", tableHeaderStyle);
            GUI.Label(new Rect(tx + 70, ty, 160, 22), "CHASER", tableHeaderStyle);
            GUI.Label(new Rect(tx + 240, ty, 160, 22), "EVADER", tableHeaderStyle);
            GUI.Label(new Rect(tx + 410, ty, 140, 22), "OUTCOME", tableHeaderStyle);
            GUI.Label(new Rect(tx + 560, ty, 90, 22), "TIME", tableHeaderStyle);
            GUI.Label(new Rect(tx + 660, ty, 100, 22), "SCORE", tableHeaderStyle);

            int displayCount = Mathf.Min(matchController.MatchHistory.Count, 10);
            for (int i = 0; i < displayCount; i++)
            {
                var r = matchController.MatchHistory[i];
                float ry = ty + 26 + (i * 24);
                tableRowStyle.fontSize = 12;
                tableRowStyle.normal.textColor = Color.white;

                GUI.Label(new Rect(tx, ry, 60, 20), $"#{r.chaseNumber}", tableRowStyle);
                GUI.Label(new Rect(tx + 70, ry, 160, 20), r.chaserName, tableRowStyle);
                GUI.Label(new Rect(tx + 240, ry, 160, 20), r.evaderName, tableRowStyle);

                string outcomeStr = r.outcome == ChaseOutcome.EvaderSurvived ? "EVADED" : (r.outcome == ChaseOutcome.Tagged ? "TAGGED" : "OUT OF BOUNDS");
                tableRowStyle.normal.textColor = r.outcome == ChaseOutcome.EvaderSurvived ? Color.green : Color.orange;
                GUI.Label(new Rect(tx + 410, ry, 140, 20), outcomeStr, tableRowStyle);

                tableRowStyle.normal.textColor = Color.white;
                GUI.Label(new Rect(tx + 560, ry, 90, 20), $"{r.duration:F2}s", tableRowStyle);
                GUI.Label(new Rect(tx + 660, ry, 100, 20), $"{r.teamAScoreAfter} - {r.teamBScoreAfter}", tableRowStyle);
            }

            // Rematch & Main Menu Buttons
            float bW = 210f;
            float bY = by + boxH - 55f;
            float startBX = bx + (boxW - (bW * 2 + 20f)) * 0.5f;

            if (GUI.Button(new Rect(startBX, bY, bW, 40), "REMATCH (PLAY AGAIN)", blueBtnStyle))
            {
                matchController.RestartMatch();
            }

            if (GUI.Button(new Rect(startBX + bW + 20f, bY, bW, 40), "MAIN MENU"))
            {
                WCTMenuController.Instance?.OpenScreen(MenuScreen.MainMenu);
            }
        }
    }
}

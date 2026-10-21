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

            if (!isPaused && (WCTMenuController.Instance == null || !WCTMenuController.Instance.IsMenuOpen))
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

            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    Vector2 screenPos = touch.position;
                    Vector2 guiPos = new Vector2(screenPos.x, sh - screenPos.y);

                    // Left touch zone: Virtual Joystick
                    if (guiPos.x < sw * 0.45f && guiPos.y > sh * 0.35f)
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
                            bool sprint = dist > (joyRadius * 0.75f);
                            playerController.SetVirtualJoystick(moveInput, sprint);
                        }
                        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                        {
                            joystickFingerId = -1;
                            joystickPos = joyCenter;
                            playerController.SetVirtualJoystick(Vector2.zero, false);
                        }
                    }
                    // Right touch zone: Swipe Look (excluding bottom right buttons area)
                    else if (guiPos.x > sw * 0.45f && guiPos.y < sh * 0.72f)
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
                                Vector2 delta = (touch.position - lastSwipePos) * 0.15f;
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

                // Bottom-Left: Virtual On-Screen Joystick & WASD movement disc
                DrawVirtualJoystickAndMovement(sw, sh);

                // Bottom-Right: Esports Action Badges (Jump, Slide, Catch/Tag)
                DrawBottomRightActionButtons(sw, sh);

                // Top: Gyroscope Status & Toggle Button
                DrawGyroscopeToggle(sw);
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

            // 1. Header: "QUAD ARENA (CLASSIC)" + "124 FPS / 32 ms"
            float headerY = 12f;
            badgeStyle.fontSize = 11;
            badgeStyle.fontStyle = FontStyle.Bold;
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(cardX, headerY, 130, 16), "QUAD ARENA (CLASSIC)", badgeStyle);

            smallTextStyle.fontSize = 9;
            smallTextStyle.normal.textColor = new Color(0.6f, 0.7f, 0.85f);
            GUI.Label(new Rect(cardX + 130, headerY, 60, 16), "124 FPS\n32 ms", smallTextStyle);

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
        // BOTTOM-LEFT: VIRTUAL JOYSTICK & MOVEMENT DISC
        // ==============================================================
        private void DrawVirtualJoystickAndMovement(float sw, float sh)
        {
            float joyRadius = 60f;
            Vector2 joyCenter = new Vector2(115f, sh - 115f);
            float joyDiameter = joyRadius * 2f;

            // Base circular track
            GUI.color = new Color(0.04f, 0.08f, 0.14f, 0.85f);
            GUI.DrawTexture(new Rect(joyCenter.x - joyRadius, joyCenter.y - joyRadius, joyDiameter, joyDiameter), circleTex);
            GUI.color = new Color(0.15f, 0.75f, 0.98f, 0.85f);
            GUI.DrawTexture(new Rect(joyCenter.x - joyRadius, joyCenter.y - joyRadius, joyDiameter, joyDiameter), ringTex);

            // Cardinal direction chevrons
            badgeStyle.fontSize = 13;
            badgeStyle.normal.textColor = new Color(0.6f, 0.85f, 1f, 0.85f);
            GUI.Label(new Rect(joyCenter.x - 10, joyCenter.y - joyRadius + 4, 20, 20), "▲", badgeStyle);
            GUI.Label(new Rect(joyCenter.x - 10, joyCenter.y + joyRadius - 22, 20, 20), "▼", badgeStyle);
            GUI.Label(new Rect(joyCenter.x - joyRadius + 4, joyCenter.y - 10, 20, 20), "◀", badgeStyle);
            GUI.Label(new Rect(joyCenter.x + joyRadius - 22, joyCenter.y - 10, 20, 20), "▶", badgeStyle);

            // Thumb Knob
            Vector2 knobPos = (joystickPos.sqrMagnitude > 1f) ? joystickPos : joyCenter;
            float knobRadius = 26f;
            float knobDiameter = knobRadius * 2f;

            GUI.color = new Color(0.1f, 0.5f, 0.85f, 0.95f);
            GUI.DrawTexture(new Rect(knobPos.x - knobRadius, knobPos.y - knobRadius, knobDiameter, knobDiameter), circleTex);
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(knobPos.x - knobRadius, knobPos.y - knobRadius, knobDiameter, knobDiameter), ringTex);
            badgeStyle.fontSize = 11;
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(knobPos.x - 12, knobPos.y - 10, 24, 20), "🕹", badgeStyle);

            // Label below
            smallTextStyle.fontSize = 10;
            smallTextStyle.alignment = TextAnchor.MiddleCenter;
            smallTextStyle.normal.textColor = new Color(0.7f, 0.88f, 1f);
            GUI.Label(new Rect(joyCenter.x - 65, joyCenter.y + joyRadius + 4, 130, 16), "JOYSTICK / SPRINT", smallTextStyle);
            smallTextStyle.alignment = TextAnchor.MiddleLeft;
            GUI.color = Color.white;
        }

        // ==============================================================
        // BOTTOM-CENTER: STAMINA BAR (Matching Screenshot!)
        // ==============================================================
        private void DrawStaminaMeter(float sw, float sh)
        {
            float barW = 280f;
            float barH = 10f;
            float sx = (sw - barW) * 0.5f;
            float sy = sh - 42f;

            // Running Athlete Circle Icon
            float iconSize = 26f;
            GUI.DrawTexture(new Rect(sx - 34, sy - 8, iconSize, iconSize), darkBoxTex);
            GUI.DrawTexture(new Rect(sx - 34, sy - 8, iconSize, iconSize), circleTex);
            badgeStyle.fontSize = 12;
            badgeStyle.normal.textColor = Color.black;
            GUI.Label(new Rect(sx - 34, sy - 7, iconSize, iconSize), "🏃", badgeStyle);

            // Dark meter track
            GUI.DrawTexture(new Rect(sx, sy, barW, barH), darkBoxTex);

            // Glowing Cyan Fill
            float fill = playerController != null ? playerController.StaminaNormalized : 1.0f;
            GUI.DrawTexture(new Rect(sx, sy, barW * fill, barH), staminaFillTex);

            // "STAMINA" label below bar
            smallTextStyle.fontSize = 10;
            smallTextStyle.fontStyle = FontStyle.Bold;
            smallTextStyle.normal.textColor = new Color(0.6f, 0.85f, 1.0f);
            GUI.Label(new Rect(sx, sy + barH + 2, barW, 14), "STAMINA", smallTextStyle);
        }

        // ==============================================================
        // BOTTOM-RIGHT: ESPORTS ACTION BUTTONS (Touch & Click Enabled)
        // ==============================================================
        private void DrawBottomRightActionButtons(float sw, float sh)
        {
            float spacing = 86f;
            float rightX = sw - (spacing * 3) - 25f;
            float actionY = sh - 110f;

            // 1. Jump / Vault Button
            float jumpSize = 64f;
            if (DrawInteractiveActionButton(rightX, actionY + 6, jumpSize, "🏃", "JUMP / VAULT", "Space / Tap", new Color(0.12f, 0.45f, 0.85f)))
            {
                if (playerController != null) playerController.TriggerJump();
            }

            // 2. Slide / Crouch Button
            float slideSize = 64f;
            if (DrawInteractiveActionButton(rightX + spacing, actionY + 6, slideSize, "🤸", "SLIDE / DUCK", "C / Ctrl", new Color(0.2f, 0.7f, 0.6f)))
            {
                if (playerController != null) playerController.TriggerSlideOrCrouch();
            }

            // 3. Catch / Tag Action Button (Larger, High-Priority, Vibrant Orange)
            float tagSize = 76f;
            if (DrawInteractiveActionButton(rightX + spacing * 2 - 4, actionY - 2, tagSize, "✋", "CATCH / TAG", "F / Tap", new Color(0.96f, 0.38f, 0.08f), true))
            {
                if (playerController != null) playerController.TriggerCatchOrTag();
            }
        }

        private bool DrawInteractiveActionButton(float x, float y, float size, string icon, string actionLabel, string subLabel, Color accentCol, bool isHighlight = false)
        {
            Rect btnRect = new Rect(x, y, size, size);
            bool clicked = false;

            // Draw circular background
            GUI.color = isHighlight ? new Color(0.25f, 0.1f, 0.05f, 0.95f) : new Color(0.04f, 0.08f, 0.14f, 0.88f);
            GUI.DrawTexture(btnRect, circleTex);

            // Ring border
            GUI.color = accentCol;
            GUI.DrawTexture(btnRect, ringTex);

            if (isHighlight)
            {
                // Pulsing outer halo for Catch/Tag button
                float pulse = (Mathf.Sin(Time.time * 8f) + 1f) * 0.5f;
                GUI.color = new Color(accentCol.r, accentCol.g, accentCol.b, 0.3f + pulse * 0.4f);
                GUI.DrawTexture(new Rect(x - 4, y - 4, size + 8, size + 8), ringTex);
            }

            // Icon
            badgeStyle.fontSize = (int)(size * 0.38f);
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(x, y + size * 0.22f, size, size * 0.45f), icon, badgeStyle);
            GUI.color = Color.white;

            // Action name below badge
            smallTextStyle.fontSize = 10;
            smallTextStyle.fontStyle = FontStyle.Bold;
            smallTextStyle.alignment = TextAnchor.MiddleCenter;
            smallTextStyle.normal.textColor = isHighlight ? new Color(1.0f, 0.7f, 0.4f) : Color.white;
            GUI.Label(new Rect(x - 20, y + size + 2, size + 40, 14), actionLabel, smallTextStyle);

            // Sub-label below
            badgeStyle.fontSize = 9;
            badgeStyle.fontStyle = FontStyle.Normal;
            badgeStyle.normal.textColor = new Color(0.7f, 0.85f, 1.0f);
            GUI.Label(new Rect(x - 20, y + size + 16, size + 40, 14), subLabel, badgeStyle);
            smallTextStyle.alignment = TextAnchor.MiddleLeft;

            // Interactive Click / Tap
            if (GUI.Button(btnRect, GUIContent.none, GUIStyle.none))
            {
                clicked = true;
            }

            return clicked;
        }

        // ==============================================================
        // GYROSCOPE HUD TOGGLE & INDICATOR
        // ==============================================================
        private void DrawGyroscopeToggle(float sw)
        {
            float btnW = 120f;
            float btnH = 26f;
            float btnX = sw - btnW - 225f;
            float btnY = 12f;

            bool gyroOn = (playerController != null && playerController.EnableGyroscope);
            Color gyroCol = gyroOn ? new Color(0.1f, 0.85f, 0.45f) : new Color(0.6f, 0.6f, 0.6f);

            GUI.color = new Color(0.04f, 0.08f, 0.14f, 0.85f);
            GUI.DrawTexture(new Rect(btnX, btnY, btnW, btnH), darkBoxTex);
            GUI.color = gyroCol;
            GUI.DrawTexture(new Rect(btnX, btnY, btnW, 2f), Texture2D.whiteTexture);
            GUI.color = Color.white;

            badgeStyle.fontSize = 11;
            badgeStyle.fontStyle = FontStyle.Bold;
            badgeStyle.normal.textColor = gyroCol;
            string gyroText = gyroOn ? "📱 GYRO: ON" : "📱 GYRO: OFF";

            if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), gyroText, badgeStyle))
            {
                if (playerController != null)
                {
                    playerController.EnableGyroscope = !playerController.EnableGyroscope;
                }
            }
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
            GUI.Label(new Rect(remX + 10, remY + 24, remW - 20, 20), "[W A S D] Move  •  [Shift] Sprint  •  [Space] Jump/Vault  •  [Ctrl] Slide  •  [F] Tag  •  [Esc] Pause", smallTextStyle);
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

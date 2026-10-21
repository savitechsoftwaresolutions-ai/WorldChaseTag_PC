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

            // 1. Center Crosshair Reticle (Crisp PC Reticle in gameplay)
            if (showCrosshair && !isPaused && (WCTMenuController.Instance == null || !WCTMenuController.Instance.IsMenuOpen))
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

            // 3. Top-Left Team Players Card & Current Objective
            DrawTeamPlayersCard(sw, sh);
            DrawCurrentObjectiveCard(sw, sh);

            // 4. Top-Right Mini Map Card & Status Pills
            DrawMiniMapCard(sw, sh);
            DrawStatusPills(sw, sh);

            // 5. Center-Bottom Stamina Bar
            if (showStaminaBar && playerController != null)
            {
                DrawStaminaMeter(sw, sh);
            }

            // 6. Clean PC Controls Legend (Esports PC HUD - no mobile joystick / touch circles)
            if (showParkourPrompts && !isPaused && (WCTMenuController.Instance == null || !WCTMenuController.Instance.IsMenuOpen))
            {
                DrawPCControlsLegend(sw, sh);
            }

            // 7. Dynamic Chase Result Overlay (Images 4 & 5)
            if (matchController.State == WCTMatchState.CHASE_RESULT)
            {
                DrawChaseResultOverlay(sw, sh);
            }

            // 8. Post-Match Tournament Scoreboard (Screen 11)
            if (matchController.State == WCTMatchState.POST_MATCH)
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
        // TOP BROADCAST SCOREBOARD BAR (Image 6 & Screenshot)
        // ==============================================================
        private void DrawTopBroadcastHeader(float sw)
        {
            float totalW = 680f;
            float rx = (sw - totalW) * 0.5f;

            // Header Label above Bar
            smallTextStyle.fontSize = 11;
            smallTextStyle.normal.textColor = new Color(0.7f, 0.8f, 0.95f);
            GUI.Label(new Rect(rx, 4f, totalW, 16), "Chase Timer", smallTextStyle);

            float barY = 22f;
            float bannerH = 46f;

            // Alpha Team Box (Left)
            float alphaW = 210f;
            GUI.DrawTexture(new Rect(rx, barY, alphaW, bannerH), darkBoxTex);
            GUI.DrawTexture(new Rect(rx, barY, 4f, bannerH), evaderAccentTex);

            scoreStyle.fontSize = 18;
            scoreStyle.normal.textColor = new Color(0.2f, 0.85f, 1.0f);
            GUI.Label(new Rect(rx + 16, barY + 10, 110, 24), "A  ALPHA", scoreStyle);

            scoreStyle.fontSize = 24;
            scoreStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(rx + alphaW - 48, barY + 8, 40, 28), $"{matchController.TeamAScore}", scoreStyle);

            // Center Digital Timer Box
            float timerW = 180f;
            float timerX = rx + alphaW + 15f;
            GUI.DrawTexture(new Rect(timerX, barY - 4f, timerW, bannerH + 8f), darkBoxTex);
            GUI.DrawTexture(new Rect(timerX, barY - 4f, timerW, 2f), evaderAccentTex);

            float timeVal = matchController.IsChaseActive ? matchController.TimeRemaining : 20.0f;
            Color timerCol = Color.white;
            if (matchController.IsChaseActive && timeVal <= 5.0f)
            {
                float pulse = (Mathf.Sin(Time.time * 12f) + 1f) * 0.5f;
                timerCol = Color.Lerp(Color.red, Color.yellow, pulse);
            }

            timerStyle.fontSize = 26;
            timerStyle.normal.textColor = timerCol;
            GUI.Label(new Rect(timerX, barY - 2f, timerW, 30), $"00:{(int)timeVal:00}", timerStyle);

            string chaseStr = matchController.IsSuddenDeath
                ? "SUDDEN DEATH"
                : $"CHASE {matchController.ChaseNumber} / {matchController.MaxChases}";

            badgeStyle.fontSize = 10;
            badgeStyle.normal.textColor = new Color(0.7f, 0.8f, 0.9f);
            GUI.Label(new Rect(timerX, barY + 28f, timerW, 16), chaseStr, badgeStyle);

            // Omega Team Box (Right)
            float omegaW = 210f;
            float omegaX = timerX + timerW + 15f;
            GUI.DrawTexture(new Rect(omegaX, barY, omegaW, bannerH), darkBoxTex);
            GUI.DrawTexture(new Rect(omegaX + omegaW - 4f, barY, 4f, bannerH), chaserAccentTex);

            scoreStyle.fontSize = 24;
            scoreStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(omegaX + 12, barY + 8, 40, 28), $"{matchController.TeamBScore}", scoreStyle);

            scoreStyle.fontSize = 18;
            scoreStyle.normal.textColor = new Color(1.0f, 0.55f, 0.15f);
            GUI.Label(new Rect(omegaX + 56, barY + 10, 135, 24), "OMEGA  Ω", scoreStyle);
        }

        // ==============================================================
        // TOP-LEFT: TEAM PLAYERS (ALIVE/TAGGED) CARD
        // ==============================================================
        private void DrawTeamPlayersCard(float sw, float sh)
        {
            float cardX = 22f;
            float cardY = 22f;
            float cardW = 220f;
            float cardH = 160f;

            GUI.DrawTexture(new Rect(cardX, cardY, cardW, cardH), darkBoxTex);
            GUI.DrawTexture(new Rect(cardX, cardY, cardW, 2), alphaCyanTex);

            badgeStyle.fontSize = 11;
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(cardX + 10, cardY + 8, cardW - 20, 18), "Team Players (Alive/Tagged)", badgeStyle);

            // 4 Athletes Rows: A1 Player01, A2 RohanK, A3 VikramS, A4 AaravP
            string[] names = { "Player01", "RohanK", "VikramS", "AaravP" };
            float rowH = 26f;
            float startY = cardY + 34f;

            for (int i = 0; i < 4; i++)
            {
                float ry = startY + i * (rowH + 4f);
                bool isActivePlayer = (i == 0); // Player01 is current active user athlete

                if (isActivePlayer)
                {
                    GUI.DrawTexture(new Rect(cardX + 6, ry, cardW - 12, rowH), bluePillTex);
                }
                else
                {
                    GUI.DrawTexture(new Rect(cardX + 6, ry, cardW - 12, rowH), semiDarkTex);
                }

                // Athlete Code Badge
                badgeStyle.fontSize = 10;
                badgeStyle.normal.textColor = isActivePlayer ? Color.cyan : new Color(0.7f, 0.8f, 0.9f);
                GUI.Label(new Rect(cardX + 10, ry + 4, 22, 18), $"A{i + 1}", badgeStyle);

                // Mini Avatar Circle
                GUI.DrawTexture(new Rect(cardX + 34, ry + 4, 18, 18), circleTex);

                // Athlete Name
                smallTextStyle.fontSize = 11;
                smallTextStyle.normal.textColor = Color.white;
                GUI.Label(new Rect(cardX + 56, ry + 4, 80, 18), names[i], smallTextStyle);

                // Health / Stamina Bar
                float barW = 60f;
                float bx = cardX + cardW - barW - 14f;
                GUI.DrawTexture(new Rect(bx, ry + 9, barW, 8), darkBoxTex);
                float fill = isActivePlayer ? (playerController != null ? playerController.StaminaNormalized : 1.0f) : 1.0f;
                GUI.DrawTexture(new Rect(bx, ry + 9, barW * fill, 8), isActivePlayer ? staminaFillTex : alphaCyanTex);
            }
        }

        // ==============================================================
        // LEFT-MIDDLE: CURRENT OBJECTIVE CARD
        // ==============================================================
        private void DrawCurrentObjectiveCard(float sw, float sh)
        {
            float cardX = 22f;
            float cardY = 192f;
            float cardW = 220f;
            float cardH = 62f;

            GUI.DrawTexture(new Rect(cardX, cardY, cardW, cardH), darkBoxTex);

            badgeStyle.fontSize = 11;
            badgeStyle.normal.textColor = new Color(0.7f, 0.8f, 0.9f);
            GUI.Label(new Rect(cardX + 10, cardY + 8, cardW - 20, 16), "Current Objective", badgeStyle);

            bool isEvader = playerController == null || playerController.CurrentRole == ChaseRole.Evader;
            string objText = isEvader ? "Survive for 20 seconds" : "Tag the Evader within 20s";

            badgeStyle.fontSize = 14;
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(cardX + 12, cardY + 28, 22, 22), "⏱", badgeStyle);

            smallTextStyle.fontSize = 12;
            smallTextStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(cardX + 38, cardY + 30, cardW - 46, 20), objText, smallTextStyle);
        }

        // ==============================================================
        // TOP-RIGHT: MINI MAP (ARENA LAYOUT) CARD
        // ==============================================================
        private void DrawMiniMapCard(float sw, float sh)
        {
            float cardW = 220f;
            float cardH = 160f;
            float cardX = sw - cardW - 22f;
            float cardY = 22f;

            GUI.DrawTexture(new Rect(cardX, cardY, cardW, cardH), darkBoxTex);
            GUI.DrawTexture(new Rect(cardX, cardY, cardW, 2), alphaCyanTex);

            badgeStyle.fontSize = 11;
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(cardX + 10, cardY + 8, cardW - 20, 18), "Mini Map (Arena Layout)", badgeStyle);

            // 2D Tactical Map Box
            float mapX = cardX + 12f;
            float mapY = cardY + 30f;
            float mapW = cardW - 24f;
            float mapH = cardH - 42f;

            GUI.DrawTexture(new Rect(mapX, mapY, mapW, mapH), panelBgTex);

            // Arena Obstacle Blocks (Central Mountain & Side blocks)
            float mapCX = mapX + mapW * 0.5f;
            float mapCY = mapY + mapH * 0.5f;

            GUI.color = new Color(0.18f, 0.24f, 0.35f, 0.95f);
            // Center Quad Platform
            GUI.DrawTexture(new Rect(mapCX - 18, mapCY - 18, 36, 36), Texture2D.whiteTexture);
            // Surrounding obstacles
            GUI.DrawTexture(new Rect(mapCX - 50, mapCY - 25, 20, 12), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(mapCX + 30, mapCY - 25, 20, 12), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(mapCX - 50, mapCY + 15, 20, 12), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(mapCX + 30, mapCY + 15, 20, 12), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // Player Indicator (Blue Arrow/Triangle)
            float playerMapX = mapCX;
            float playerMapY = mapCY;

            if (playerController != null)
            {
                Vector3 pPos = playerController.transform.position;
                playerMapX = Mathf.Clamp(mapCX + (pPos.x / 12f) * (mapW * 0.42f), mapX + 8, mapX + mapW - 8);
                playerMapY = Mathf.Clamp(mapCY - (pPos.z / 12f) * (mapH * 0.42f), mapY + 8, mapY + mapH - 8);
            }

            GUI.color = Color.cyan;
            GUI.DrawTexture(new Rect(playerMapX - 5, playerMapY - 5, 10, 10), circleTex);
            GUI.color = Color.white;

            // Teammate & Opponent Waypoints
            GUI.color = new Color(0.15f, 0.65f, 1.0f);
            GUI.DrawTexture(new Rect(mapCX - 35, mapCY, 6, 6), circleTex);
            GUI.DrawTexture(new Rect(mapCX + 10, mapCY - 30, 6, 6), circleTex);

            // Opponent Red Dot
            if (opponentTransform != null && playerController != null)
            {
                Vector3 oPos = opponentTransform.position;
                float oppMapX = Mathf.Clamp(mapCX + (oPos.x / 12f) * (mapW * 0.42f), mapX + 8, mapX + mapW - 8);
                float oppMapY = Mathf.Clamp(mapCY - (oPos.z / 12f) * (mapH * 0.42f), mapY + 8, mapY + mapH - 8);

                GUI.color = Color.red;
                GUI.DrawTexture(new Rect(oppMapX - 4, oppMapY - 4, 8, 8), circleTex);
            }
            else
            {
                GUI.color = Color.red;
                GUI.DrawTexture(new Rect(mapCX + 38, mapCY + 20, 7, 7), circleTex);
                GUI.DrawTexture(new Rect(mapCX + 42, mapCY - 22, 7, 7), circleTex);
            }
            GUI.color = Color.white;

            // Bottom-Right [M] Keycap
            GUI.DrawTexture(new Rect(mapX + mapW - 22, mapY + mapH - 22, 18, 18), darkBoxTex);
            badgeStyle.fontSize = 9;
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(mapX + mapW - 22, mapY + mapH - 22, 18, 18), "M", badgeStyle);
        }

        // ==============================================================
        // RIGHT-MIDDLE: STATUS PILLS
        // ==============================================================
        private void DrawStatusPills(float sw, float sh)
        {
            float cardW = 220f;
            float cardX = sw - cardW - 22f;
            float pillY = 192f;

            bool isEvader = playerController == null || playerController.CurrentRole == ChaseRole.Evader;

            // 1. Red Alert Pill
            GUI.DrawTexture(new Rect(cardX, pillY, cardW, 26), alertRedTex);
            badgeStyle.fontSize = 11;
            badgeStyle.normal.textColor = Color.white;
            string alertMsg = isEvader ? "🏃  You are being chased!" : "🏃  Pursue the Evader!";
            GUI.Label(new Rect(cardX + 8, pillY + 4, cardW - 16, 20), alertMsg, badgeStyle);

            // 2. Dark Blue Court Boundary Pill
            GUI.DrawTexture(new Rect(cardX, pillY + 32, cardW, 26), darkBoxTex);
            badgeStyle.fontSize = 11;
            badgeStyle.normal.textColor = new Color(0.8f, 0.9f, 1.0f);
            GUI.Label(new Rect(cardX + 8, pillY + 36, cardW - 16, 20), "⏱  Stay in the arena", badgeStyle);
        }

        // ==============================================================
        // ==============================================================
        // CLEAN PC CONTROLS LEGEND (Esports PC HUD - no mobile joystick)
        // ==============================================================
        private void DrawPCControlsLegend(float sw, float sh)
        {
            float barW = 580f;
            float barH = 26f;
            float x = (sw - barW) * 0.5f;
            float y = sh - 28f;

            GUI.DrawTexture(new Rect(x, y, barW, barH), darkBoxTex);
            badgeStyle.fontSize = 11;
            badgeStyle.normal.textColor = new Color(0.75f, 0.88f, 1.0f);
            GUI.Label(new Rect(x, y + 4, barW, 18), "[WASD] Move  •  [Shift] Sprint  •  [Space] Vault/Jump  •  [Ctrl/C] Slide  •  [F] Tag  •  [Esc] Pause", badgeStyle);
        }

        // ==============================================================
        // CENTER-BOTTOM: STAMINA BAR
        // ==============================================================
        private void DrawStaminaMeter(float sw, float sh)
        {
            float barW = 340f;
            float barH = 14f;
            float sx = (sw - barW) * 0.5f;
            float sy = sh - 56f;

            // Running Man Icon Pill on left
            float iconSize = 28f;
            GUI.DrawTexture(new Rect(sx - 36, sy - 7, iconSize, iconSize), darkBoxTex);
            GUI.DrawTexture(new Rect(sx - 36, sy - 7, iconSize, iconSize), ringTex);
            badgeStyle.fontSize = 13;
            badgeStyle.normal.textColor = Color.cyan;
            GUI.Label(new Rect(sx - 36, sy - 3, iconSize, iconSize), "🏃", badgeStyle);

            // Background & Border
            GUI.DrawTexture(new Rect(sx, sy, barW, barH), darkBoxTex);

            // Fill Bar (Normalized stamina)
            float fill = playerController != null ? playerController.StaminaNormalized : 1.0f;
            GUI.DrawTexture(new Rect(sx, sy, barW * fill, barH), staminaFillTex);

            // Label centered below
            smallTextStyle.fontSize = 10;
            smallTextStyle.normal.textColor = new Color(0.4f, 0.8f, 1.0f);
            GUI.Label(new Rect(sx, sy + barH + 2, barW, 14), "Stamina Bar", smallTextStyle);
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
            DrawMiniMapCard(sw, sh);

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

            // Top Broadcast header stays on
            DrawTopBroadcastHeader(sw);

            bool isEvaded = matchController.LastOutcome == ChaseOutcome.EvaderSurvived;

            float cardW = 340f;
            float cardX = 60f;
            float cardY = 120f;

            // Large Title Banner
            titleStyle.fontSize = 36;
            titleStyle.normal.textColor = isEvaded ? new Color(0.1f, 0.85f, 0.95f) : new Color(0.95f, 0.25f, 0.2f);
            string titleText = isEvaded ? "EVADER ESCAPES!" : (matchController.LastOutcome == ChaseOutcome.Tagged ? "EVADER TAGGED!" : "OUT OF BOUNDS!");
            GUI.Label(new Rect(cardX, cardY, 500, 42), titleText, titleStyle);

            subtitleStyle.fontSize = 14;
            subtitleStyle.normal.textColor = Color.white;
            string subText = isEvaded ? "YOU SURVIVED THE CHASE" : (matchController.LastOutcome == ChaseOutcome.Tagged ? "THE CHASER CAUGHT YOU" : "STEPPED OUT OF 12m COURT");
            GUI.Label(new Rect(cardX, cardY + 44, 400, 22), subText, subtitleStyle);

            // Left Stats Card
            float statY = cardY + 75f;
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
            float mapW = 340f;
            float mapX = sw - mapW - 60f;
            GUI.DrawTexture(new Rect(mapX, statY, mapW, 230f), darkBoxTex);

            headerStyle.fontSize = 13;
            headerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(mapX + 16, statY + 10, mapW - 32, 20), isEvaded ? "CHASE ROUTE (YOU)" : "CHASE REPLAY MAP", headerStyle);

            // 2D Map Schematic
            float miniSize = 140f;
            float miniX = mapX + 16f;
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
            float legX = mapX + miniSize + 25f;
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

            // Bottom Buttons (Images 4 & 5)
            float btnW = 200f;
            float btnY = sh - 75f;
            float centerBX = (sw - (btnW * 3 + 40f)) * 0.5f;

            if (GUI.Button(new Rect(centerBX, btnY, btnW, 42), "PLAY REPLAY"))
            {
                WCTAudioSynthesizer.Instance?.PlayUIClick();
            }

            if (GUI.Button(new Rect(centerBX + btnW + 20f, btnY, btnW, 42), "NEXT CHASE >", blueBtnStyle))
            {
                matchController.StartMatch(matchController.Mode, matchController.Difficulty);
                WCTAudioSynthesizer.Instance?.PlayBuzzer();
            }

            if (GUI.Button(new Rect(centerBX + (btnW + 20f) * 2, btnY, btnW, 42), "MAIN MENU"))
            {
                WCTMenuController.Instance?.OpenScreen(MenuScreen.MainMenu);
            }
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

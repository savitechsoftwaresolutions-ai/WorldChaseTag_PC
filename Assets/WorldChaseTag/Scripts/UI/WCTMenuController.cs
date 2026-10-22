using System;
using System.Collections.Generic;
using UnityEngine;
using WorldChaseTag.Core;
using WorldChaseTag.Movement;
using WorldChaseTag.Audio;

namespace WorldChaseTag.UI
{
    public enum MenuScreen
    {
        Splash,
        MainMenu,
        ModeSelect,
        TeamSelect,
        ArenaSelect,
        PreMatchIntro,
        TrainingDrills,
        Settings,
        Controls,
        RulesCodex,
        PlayerProfile,
        HowToPlay,
        MatchResults
    }

    /// <summary>
    /// Master PC Menu Controller implementing the complete 16-screen World Chase Tag UI/UX design.
    /// Features: Global navigation bar, player profile pill, game mode selection,
    /// Team Alpha vs Omega athlete card selection with special abilities and stat meters,
    /// Arena browser with mini-map schematics, Pre-Match setup, Training drills, and Settings.
    /// </summary>
    public class WCTMenuController : MonoBehaviour
    {
        public static WCTMenuController Instance { get; private set; }

        [Header("Menu State")]
        [SerializeField] private bool isMenuOpen = true;
        [SerializeField] private MenuScreen currentScreen = MenuScreen.MainMenu;

        public bool IsMenuOpen => isMenuOpen;
        public MenuScreen CurrentScreen => currentScreen;

        // Selected Configurations
        private GameMode selectedMode = GameMode.FullTournamentMatch;
        private AIDifficulty selectedDifficulty = AIDifficulty.Normal;
        private int selectedTeamIndex = 0; // 0 = Alpha, 1 = Omega
        private int selectedAthleteIndex = 0; // Index in active team roster
        private int selectedArenaIndex = 0; // 0 = Quad, 1 = Urban Rooftop, 2 = Warehouse, 3 = Night Arena, 4 = Custom
        private int settingsActiveTab = 0; // 0: Graphics, 1: Audio, 2: Controls, 3: Gameplay, 4: Accessibility
        private int trainingActiveDrill = 0;
        private int profileActiveTab = 0;
        private int howToPlayPage = 1;
        private int controlsActiveSubTab = 0;
        private int howToPlaySidebarIndex = 0;
        private int selectedTeamLogo = 0;
        private int selectedTeamColor = 0;
        private int selectedJerseyVariant = 0;

        // Player Profile State (matches top-right pill Player01, Rookie, Level 12)
        private string playerName = "Player01";
        private string playerRank = "Rookie";
        private int playerLevel = 12;
        private int currentXP = 420;
        private int maxXP = 1200;

        // Settings State
        private float mouseSensitivity = 2.2f;
        private bool invertY = false;
        private bool headBob = true;
        private float fov = 75f;
        private int qualityIndex = 2; // 0: Low, 1: Medium, 2: High, 3: Ultra
        private bool isFullscreen = true;
        private bool vSyncEnabled = true;
        private bool motionBlur = true;
        private bool bloomEffect = true;
        private float masterVolume = 1.0f;
        private float sfxVolume = 0.9f;

        // UI Textures & Styling
        private bool stylesInitialized = false;
        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle menuBtnStyle;
        private GUIStyle activeBtnStyle;
        private GUIStyle tabBtnStyle;
        private GUIStyle activeTabBtnStyle;
        private GUIStyle headerStyle;
        private GUIStyle cardTitleStyle;
        private GUIStyle cardStatStyle;
        private GUIStyle smallTextStyle;
        private GUIStyle badgeStyle;
        private GUIStyle blueBtnStyle;

        private Texture2D darkBgTex;
        private Texture2D topBarBgTex;
        private Texture2D panelBgTex;
        private Texture2D alphaCyanTex;
        private Texture2D alphaCyanGlowTex;
        private Texture2D omegaOrangeTex;
        private Texture2D goldAccentTex;
        private Texture2D cardBgTex;
        private Texture2D cardSelectedTex;
        private Texture2D statBarFillTex;
        private Texture2D statBarBgTex;
        private Texture2D bluePillTex;

        private float splashTimer = 0f;

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

            CreateColorTextures();
            LoadSettings();
        }

        private void Start()
        {
            if (isMenuOpen)
            {
                SetMenuOpen(true);
            }
        }

        private void CreateColorTextures()
        {
            darkBgTex = MakeColorTex(new Color(0.04f, 0.06f, 0.09f, 0.96f));
            topBarBgTex = MakeColorTex(new Color(0.05f, 0.08f, 0.12f, 0.98f));
            panelBgTex = MakeColorTex(new Color(0.08f, 0.11f, 0.16f, 0.94f));
            alphaCyanTex = MakeColorTex(new Color(0.06f, 0.65f, 0.98f, 0.95f));
            alphaCyanGlowTex = MakeColorTex(new Color(0.12f, 0.55f, 0.95f, 0.45f));
            omegaOrangeTex = MakeColorTex(new Color(0.96f, 0.44f, 0.08f, 0.95f));
            goldAccentTex = MakeColorTex(new Color(1.0f, 0.76f, 0.18f, 0.95f));
            cardBgTex = MakeColorTex(new Color(0.10f, 0.13f, 0.19f, 0.94f));
            cardSelectedTex = MakeColorTex(new Color(0.12f, 0.20f, 0.32f, 0.95f));
            statBarFillTex = MakeColorTex(new Color(0.14f, 0.85f, 0.50f, 0.95f));
            statBarBgTex = MakeColorTex(new Color(0.18f, 0.22f, 0.30f, 0.85f));
            bluePillTex = MakeColorTex(new Color(0.08f, 0.48f, 0.94f, 0.95f));
        }

        private Texture2D MakeColorTex(Color c)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, c);
            tex.Apply();
            return tex;
        }

        public void SetMenuOpen(bool open)
        {
            isMenuOpen = open;
            Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = open;

            var player = FindAnyObjectByType<WCTParkourController>();
            if (player != null)
            {
                player.ControlsEnabled = !open;
            }

            Time.timeScale = open ? 0f : 1f;
        }

        public void OpenScreen(MenuScreen screen)
        {
            currentScreen = screen;
            SetMenuOpen(true);
            WCTAudioSynthesizer.Instance?.PlayUIClick();
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 32,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };

            subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };

            menuBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(18, 10, 8, 8)
            };

            activeBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(18, 10, 8, 8)
            };

            tabBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            activeTabBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            blueBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };

            cardTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            cardStatStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };

            smallTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true
            };

            badgeStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            stylesInitialized = true;
        }

        private void Update()
        {
            if (isMenuOpen)
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

            if (currentScreen == MenuScreen.Splash)
            {
                splashTimer += Time.unscaledDeltaTime;
                if (splashTimer > 1.8f || Input.anyKeyDown)
                {
                    currentScreen = MenuScreen.MainMenu;
                }
            }

            if (Input.GetKeyDown(KeyCode.M) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (isMenuOpen)
                {
                    if (currentScreen == MenuScreen.MainMenu)
                    {
                        LaunchMatch();
                    }
                    else
                    {
                        currentScreen = MenuScreen.MainMenu;
                    }
                }
                else
                {
                    SetMenuOpen(true);
                }
            }
        }

        private void OnGUI()
        {
            if (!isMenuOpen) return;

            InitStyles();

            float sw = Screen.width;
            float sh = Screen.height;

            if (currentScreen == MenuScreen.Splash)
            {
                DrawSplashScreen(sw, sh);
                return;
            }

            // Global Top Navigation Bar (Images 2, 3, 4, 5)
            DrawGlobalTopNavigationBar(sw);

            // Screen Content below top bar (y: 60px to sh)
            switch (currentScreen)
            {
                case MenuScreen.MainMenu:
                    DrawMainMenu(sw, sh);
                    break;
                case MenuScreen.ModeSelect:
                    DrawModeSelect(sw, sh);
                    break;
                case MenuScreen.TeamSelect:
                    DrawTeamAndAthleteSelect(sw, sh);
                    break;
                case MenuScreen.ArenaSelect:
                    DrawArenaSelect(sw, sh);
                    break;
                case MenuScreen.PreMatchIntro:
                    DrawPreMatchIntro(sw, sh);
                    break;
                case MenuScreen.TrainingDrills:
                    DrawTrainingDrills(sw, sh);
                    break;
                case MenuScreen.Settings:
                    DrawSettings(sw, sh);
                    break;
                case MenuScreen.Controls:
                    DrawControls(sw, sh);
                    break;
                case MenuScreen.RulesCodex:
                    DrawRulesCodex(sw, sh);
                    break;
                case MenuScreen.PlayerProfile:
                    DrawPlayerProfile(sw, sh);
                    break;
                case MenuScreen.HowToPlay:
                    DrawHowToPlay(sw, sh);
                    break;
                case MenuScreen.MatchResults:
                    DrawMatchResults(sw, sh);
                    break;
            }
        }

        // ==============================================================
        // GLOBAL TOP NAVIGATION BAR (Matching Images 2, 3, 4, 5)
        // ==============================================================
        private void DrawGlobalTopNavigationBar(float sw)
        {
            float barH = 58f;
            GUI.DrawTexture(new Rect(0, 0, sw, barH), topBarBgTex);
            GUI.DrawTexture(new Rect(0, barH - 2, sw, 2), alphaCyanTex);

            // Left: WCT Logo + Brand
            float lx = 20f;
            GUI.DrawTexture(new Rect(lx, 10, 38, 38), darkBgTex);
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(lx, 15, 38, 28), "WCT", badgeStyle);

            titleStyle.fontSize = 20;
            titleStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(lx + 46, 8, 220, 26), "WORLD CHASE TAG", titleStyle);

            subtitleStyle.fontSize = 10;
            subtitleStyle.normal.textColor = new Color(0.85f, 0.3f, 0.3f);
            GUI.Label(new Rect(lx + 46, 32, 220, 18), "PARKOUR. TAG. COMPETE.", subtitleStyle);

            // Center Navigation Tabs
            float tabStartX = 280f;
            float tabW = 92f;
            float tabH = 34f;
            float tabY = 12f;
            float spacing = 5f;

            DrawNavTab(new Rect(tabStartX, tabY, tabW, tabH), "HOME", MenuScreen.MainMenu);
            DrawNavTab(new Rect(tabStartX + (tabW + spacing) * 1, tabY, tabW + 6, tabH), "GAME MODES", MenuScreen.ModeSelect);
            DrawNavTab(new Rect(tabStartX + (tabW + spacing) * 2 + 6, tabY, tabW - 8, tabH), "TEAMS", MenuScreen.TeamSelect);
            DrawNavTab(new Rect(tabStartX + (tabW + spacing) * 3 - 2, tabY, tabW - 8, tabH), "ARENA", MenuScreen.ArenaSelect);
            DrawNavTab(new Rect(tabStartX + (tabW + spacing) * 4 - 10, tabY, tabW - 4, tabH), "RESULTS", MenuScreen.MatchResults);
            DrawNavTab(new Rect(tabStartX + (tabW + spacing) * 5 - 14, tabY, tabW + 2, tabH), "SETTINGS", MenuScreen.Settings);
            DrawNavTab(new Rect(tabStartX + (tabW + spacing) * 6 - 12, tabY, tabW + 22, tabH), "HOW TO PLAY", MenuScreen.HowToPlay);

            // Right: Player Profile Pill (Player01, Rookie, Level 12, XP bar)
            float rx = sw - 330f;
            GUI.DrawTexture(new Rect(rx, 9, 310, 40), cardBgTex);

            // Avatar circle
            GUI.DrawTexture(new Rect(rx + 6, 12, 34, 34), darkBgTex);
            badgeStyle.normal.textColor = Color.cyan;
            GUI.Label(new Rect(rx + 6, 17, 34, 24), "P1", badgeStyle);

            // Name & Rank
            smallTextStyle.fontSize = 12;
            smallTextStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(rx + 46, 10, 110, 18), playerName, smallTextStyle);
            smallTextStyle.fontSize = 10;
            smallTextStyle.normal.textColor = new Color(0.6f, 0.7f, 0.85f);
            GUI.Label(new Rect(rx + 46, 26, 110, 16), playerRank, smallTextStyle);

            // Level Badge (12)
            GUI.DrawTexture(new Rect(rx + 160, 13, 26, 26), goldAccentTex);
            badgeStyle.normal.textColor = Color.black;
            GUI.Label(new Rect(rx + 160, 16, 26, 20), $"{playerLevel}", badgeStyle);

            // XP Bar (420 / 1200 XP)
            float xpBarX = rx + 192;
            float xpBarW = 105;
            GUI.DrawTexture(new Rect(xpBarX, 22, xpBarW, 8), statBarBgTex);
            GUI.DrawTexture(new Rect(xpBarX, 22, xpBarW * ((float)currentXP / maxXP), 8), statBarFillTex);

            smallTextStyle.fontSize = 9;
            smallTextStyle.normal.textColor = new Color(0.7f, 0.8f, 0.9f);
            GUI.Label(new Rect(xpBarX, 31, xpBarW, 14), $"{currentXP} / {maxXP} XP", smallTextStyle);
        }

        private void DrawNavTab(Rect rect, string label, MenuScreen screen)
        {
            bool isActive = currentScreen == screen;
            if (isActive)
            {
                GUI.DrawTexture(rect, bluePillTex);
            }

            tabBtnStyle.normal.textColor = isActive ? Color.white : new Color(0.75f, 0.82f, 0.92f);
            if (GUI.Button(rect, label, tabBtnStyle))
            {
                currentScreen = screen;
                WCTAudioSynthesizer.Instance?.PlayUIClick();
            }
        }

        // ==============================================================
        // 1. SPLASH SCREEN (Image 1, Screen 1)
        // ==============================================================
        private void DrawSplashScreen(float sw, float sh)
        {
            GUI.DrawTexture(new Rect(0, 0, sw, sh), darkBgTex);

            float cx = sw * 0.5f;
            float cy = sh * 0.5f;

            titleStyle.fontSize = 46;
            titleStyle.normal.textColor = Color.white;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(cx - 350, cy - 100, 700, 60), "WORLD CHASE TAG", titleStyle);

            subtitleStyle.fontSize = 18;
            subtitleStyle.normal.textColor = new Color(0.9f, 0.25f, 0.25f);
            subtitleStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(cx - 300, cy - 35, 600, 30), "PARKOUR. TAG. COMPETE.", subtitleStyle);

            // Blue Loading Bar (Loading... 85%)
            float barW = 380f;
            float barH = 12f;
            float barX = cx - barW * 0.5f;
            float barY = cy + 50f;

            smallTextStyle.normal.textColor = Color.white;
            smallTextStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(cx - 150, barY - 26, 300, 22), "LOADING... 85%", smallTextStyle);

            GUI.DrawTexture(new Rect(barX, barY, barW, barH), statBarBgTex);
            GUI.DrawTexture(new Rect(barX, barY, barW * 0.85f, barH), alphaCyanTex);

            smallTextStyle.normal.textColor = new Color(0.6f, 0.7f, 0.85f);
            GUI.Label(new Rect(cx - 200, barY + 25, 400, 22), "CLICK ANYWHERE OR PRESS ANY KEY TO ENTER", smallTextStyle);

            titleStyle.alignment = TextAnchor.MiddleLeft;
            subtitleStyle.alignment = TextAnchor.MiddleLeft;
            smallTextStyle.alignment = TextAnchor.MiddleLeft;

            if (Event.current.type == EventType.MouseDown)
            {
                currentScreen = MenuScreen.MainMenu;
                WCTAudioSynthesizer.Instance?.PlayUIClick();
            }
        }

        // ==============================================================
        // 2. MAIN MENU (Image 2 layout)
        // ==============================================================
        private void DrawMainMenu(float sw, float sh)
        {
            GUI.DrawTexture(new Rect(0, 58, sw, sh - 58), darkBgTex);

            float topY = 75f;
            float navW = 340f;
            float navX = 35f;

            // Brand Header in Left Drawer
            titleStyle.fontSize = 28;
            titleStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(navX, topY, 300, 36), "WORLD CHASE TAG", titleStyle);

            subtitleStyle.fontSize = 12;
            subtitleStyle.normal.textColor = new Color(0.9f, 0.25f, 0.25f);
            GUI.Label(new Rect(navX, topY + 34, 300, 20), "PARKOUR. TAG. COMPETE.", subtitleStyle);

            // Left Navigation Drawer - 8 Pills matching Image 2
            float btnY = topY + 68f;
            float btnSpacing = 52f;
            float btnW = 300f;
            float btnH = 46f;

            if (DrawMainMenuNavPill(new Rect(navX, btnY, btnW, btnH), "> PLAY NOW", "Launch championship match", true))
            {
                LaunchMatch();
            }

            btnY += btnSpacing;
            if (DrawMainMenuNavPill(new Rect(navX, btnY, btnW, btnH), "MATCH SETUP", "Configure match options & rules", false))
            {
                currentScreen = MenuScreen.PreMatchIntro;
            }

            btnY += btnSpacing;
            if (DrawMainMenuNavPill(new Rect(navX, btnY, btnW, btnH), "GAME MODES", "Quick Match, Practice, Training", false))
            {
                currentScreen = MenuScreen.ModeSelect;
            }

            btnY += btnSpacing;
            if (DrawMainMenuNavPill(new Rect(navX, btnY, btnW, btnH), "TEAMS", "Select teams and athletes", false))
            {
                currentScreen = MenuScreen.TeamSelect;
            }

            btnY += btnSpacing;
            if (DrawMainMenuNavPill(new Rect(navX, btnY, btnW, btnH), "ARENA", "Choose your arena", false))
            {
                currentScreen = MenuScreen.ArenaSelect;
            }

            btnY += btnSpacing;
            if (DrawMainMenuNavPill(new Rect(navX, btnY, btnW, btnH), "PROGRESS", "Stats, achievements, profile", false))
            {
                currentScreen = MenuScreen.PlayerProfile;
            }

            btnY += btnSpacing;
            if (DrawMainMenuNavPill(new Rect(navX, btnY, btnW, btnH), "SETTINGS", "Graphics, audio, controls", false))
            {
                currentScreen = MenuScreen.Settings;
            }

            btnY += btnSpacing;
            if (DrawMainMenuNavPill(new Rect(navX, btnY, btnW, btnH), "HOW TO PLAY", "Rules, controls, strategy", false))
            {
                currentScreen = MenuScreen.HowToPlay;
            }

            btnY += btnSpacing;
            if (DrawMainMenuNavPill(new Rect(navX, btnY, btnW, btnH), "EXIT", "Quit to desktop", false))
            {
                Application.Quit();
            }

            // Bottom-Left Version Info
            smallTextStyle.fontSize = 11;
            smallTextStyle.normal.textColor = new Color(0.5f, 0.6f, 0.7f);
            GUI.Label(new Rect(navX, sh - 35f, 320, 24), "v0.1.0 • A PARKOUR ESPORTS EXPERIENCE", smallTextStyle);

            // Bottom Arena Quick-Selector Cards matching Image 2
            DrawBottomArenaQuickSelector(sw, sh);
        }

        private bool DrawMainMenuNavPill(Rect rect, string mainText, string subText, bool isPlay)
        {
            GUI.DrawTexture(rect, isPlay ? bluePillTex : cardBgTex);
            if (isPlay)
            {
                GUI.DrawTexture(new Rect(rect.x, rect.y, 4, rect.height), alphaCyanTex);
            }

            headerStyle.fontSize = 15;
            headerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(rect.x + 18, rect.y + 6, rect.width - 30, 20), mainText, headerStyle);

            smallTextStyle.fontSize = 11;
            smallTextStyle.normal.textColor = isPlay ? new Color(0.85f, 0.95f, 1.0f) : new Color(0.6f, 0.7f, 0.8f);
            GUI.Label(new Rect(rect.x + 18, rect.y + 24, rect.width - 30, 16), subText, smallTextStyle);

            bool clicked = GUI.Button(rect, GUIContent.none, GUIStyle.none);
            if (clicked)
            {
                WCTAudioSynthesizer.Instance?.PlayUIClick();
            }
            return clicked;
        }

        private void DrawBottomArenaQuickSelector(float sw, float sh)
        {
            float barY = sh - 100f;
            float startX = 370f;
            float cardW = 195f;
            float cardH = 68f;

            // Card 1: Quad (Classic)
            DrawQuickArenaCard(new Rect(startX, barY, cardW, cardH), "QUAD (CLASSIC)", "Official-style arena", selectedArenaIndex == 0, 0);

            // Card 2: Urban Rooftop
            DrawQuickArenaCard(new Rect(startX + cardW + 12, barY, cardW, cardH), "URBAN ROOFTOP", "Faster routes, higher risk", selectedArenaIndex == 1, 1);

            // Card 3: Warehouse
            DrawQuickArenaCard(new Rect(startX + (cardW + 12) * 2, barY, cardW, cardH), "WAREHOUSE", "Tight spaces and quick turns", selectedArenaIndex == 2, 2);

            // Card 4: Latest News
            float newsX = startX + (cardW + 12) * 3;
            GUI.DrawTexture(new Rect(newsX, barY, cardW, cardH), cardBgTex);
            badgeStyle.normal.textColor = Color.yellow;
            GUI.Label(new Rect(newsX + 10, barY + 8, cardW - 20, 18), "LATEST NEWS", badgeStyle);
            smallTextStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(newsX + 10, barY + 28, cardW - 20, 32), "NEW ARENA AVAILABLE - Urban Rooftop", smallTextStyle);
        }

        private void DrawQuickArenaCard(Rect rect, string name, string sub, bool isSelected, int idx)
        {
            GUI.DrawTexture(rect, isSelected ? cardSelectedTex : cardBgTex);
            if (isSelected)
            {
                GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 3), alphaCyanTex);
            }

            headerStyle.fontSize = 13;
            headerStyle.normal.textColor = isSelected ? Color.cyan : Color.white;
            GUI.Label(new Rect(rect.x + 12, rect.y + 10, rect.width - 24, 20), name, headerStyle);

            smallTextStyle.fontSize = 10;
            smallTextStyle.normal.textColor = new Color(0.7f, 0.8f, 0.9f);
            GUI.Label(new Rect(rect.x + 12, rect.y + 32, rect.width - 24, 28), sub, smallTextStyle);

            if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                selectedArenaIndex = idx;
                WCTAudioSynthesizer.Instance?.PlayUIClick();
            }
        }

        // ==============================================================
        // 3. GAME MODE SELECTION (Image 3 layout)
        // ==============================================================
        private void DrawModeSelect(float sw, float sh)
        {
            float topY = 75f;
            headerStyle.fontSize = 24;
            headerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(50, topY, 400, 30), "GAME MODE SELECTION", headerStyle);

            smallTextStyle.fontSize = 12;
            smallTextStyle.normal.textColor = new Color(0.6f, 0.7f, 0.85f);
            GUI.Label(new Rect(50, topY + 28, 400, 20), "CHOOSE HOW YOU WANT TO PLAY", smallTextStyle);

            // 4 Mode Cards side by side matching Image 3
            float startY = topY + 65f;
            float cardSpacing = 16f;
            float startX = 50f;
            float cardW = (sw - 100f - (cardSpacing * 3)) / 4f;
            float cardH = 360f;

            // Card 1: Quick Match
            DrawModeSelectionCard(new Rect(startX, startY, cardW, cardH), "QUICK MATCH", "Play a full match against AI.\nStandard rules, up to 16 chases.", GameMode.FullTournamentMatch, selectedMode == GameMode.FullTournamentMatch, false);

            // Card 2: Practice
            DrawModeSelectionCard(new Rect(startX + cardW + cardSpacing, startY, cardW, cardH), "PRACTICE", "Explore the arena, learn movement,\nand practice at your own pace.", GameMode.Practice, selectedMode == GameMode.Practice, false);

            // Card 3: Training
            DrawModeSelectionCard(new Rect(startX + (cardW + cardSpacing) * 2, startY, cardW, cardH), "TRAINING", "Complete drills and challenges\nto improve your skills.", GameMode.QuickMatch, false, false);

            // Card 4: Online Match (Locked / Coming Soon)
            DrawModeSelectionCard(new Rect(startX + (cardW + cardSpacing) * 3, startY, cardW, cardH), "ONLINE MATCH", "Compete against real players\n(Coming Soon).", GameMode.FullTournamentMatch, false, true);

            // Bottom Left Tooltip: Game Mode Info
            float botY = startY + cardH + 20f;
            GUI.DrawTexture(new Rect(50, botY, 480, 52), cardBgTex);
            badgeStyle.normal.textColor = Color.cyan;
            GUI.Label(new Rect(60, botY + 12, 120, 26), "(i) GAME MODE INFO", badgeStyle);
            smallTextStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(190, botY + 12, 330, 26), "Each mode offers a different experience. You can always change modes later.", smallTextStyle);

            // Bottom Right Gold Badge: Recommended For First Play
            float recW = 420f;
            float recX = sw - recW - 50f;
            GUI.DrawTexture(new Rect(recX, botY, recW, 52), cardBgTex);
            GUI.DrawTexture(new Rect(recX, botY, recW, 3), goldAccentTex);
            badgeStyle.normal.textColor = Color.yellow;
            GUI.Label(new Rect(recX + 15, botY + 14, 180, 24), "★ RECOMMENDED FOR FIRST PLAY", badgeStyle);
            smallTextStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(recX + 15, botY + 30, recW - 30, 20), "Start with Quick Match to experience full World Chase Tag gameplay.", smallTextStyle);
        }

        private void DrawModeSelectionCard(Rect rect, string title, string desc, GameMode mode, bool isSelected, bool isLocked)
        {
            GUI.DrawTexture(rect, isSelected ? cardSelectedTex : cardBgTex);
            if (isSelected)
            {
                GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 4), alphaCyanTex);
                GUI.DrawTexture(new Rect(rect.x, rect.y, 4, rect.height), alphaCyanTex);
                GUI.DrawTexture(new Rect(rect.x + rect.width - 4, rect.y, 4, rect.height), alphaCyanTex);
            }

            // Preview / Icon box
            float iconH = 140f;
            GUI.DrawTexture(new Rect(rect.x + 12, rect.y + 12, rect.width - 24, iconH), darkBgTex);

            if (isLocked)
            {
                badgeStyle.normal.textColor = Color.gray;
                GUI.Label(new Rect(rect.x + 12, rect.y + 70, rect.width - 24, 24), "[ LOCKED ]", badgeStyle);
            }
            else
            {
                badgeStyle.normal.textColor = isSelected ? Color.cyan : Color.white;
                GUI.Label(new Rect(rect.x + 12, rect.y + 70, rect.width - 24, 24), title, badgeStyle);
            }

            // Title & Description
            headerStyle.fontSize = 18;
            headerStyle.normal.textColor = isLocked ? Color.gray : Color.white;
            GUI.Label(new Rect(rect.x + 16, rect.y + iconH + 24, rect.width - 32, 24), title, headerStyle);

            smallTextStyle.fontSize = 11;
            smallTextStyle.normal.textColor = isLocked ? Color.gray : new Color(0.75f, 0.85f, 0.95f);
            GUI.Label(new Rect(rect.x + 16, rect.y + iconH + 52, rect.width - 32, 60), desc, smallTextStyle);

            // Button
            float btnH = 38f;
            float btnY = rect.y + rect.height - btnH - 14f;
            Rect btnRect = new Rect(rect.x + 16, btnY, rect.width - 32, btnH);

            if (isLocked)
            {
                GUI.Button(btnRect, "COMING SOON");
            }
            else
            {
                if (GUI.Button(btnRect, isSelected ? "SELECTED >" : "SELECT >", isSelected ? blueBtnStyle : GUI.skin.button))
                {
                    selectedMode = mode;
                    WCTAudioSynthesizer.Instance?.PlayUIClick();
                    currentScreen = MenuScreen.TeamSelect;
                }
            }
        }

        // ==============================================================
        // 4. TEAMS MANAGEMENT & ATHLETE SELECTION (Image 3 layout)
        // ==============================================================
        private void DrawTeamAndAthleteSelect(float sw, float sh)
        {
            float topY = 70f;
            titleStyle.fontSize = 24;
            titleStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(50, topY, 400, 28), "TEAMS", titleStyle);

            smallTextStyle.fontSize = 11;
            smallTextStyle.normal.textColor = new Color(0.6f, 0.7f, 0.85f);
            GUI.Label(new Rect(50, topY + 28, 450, 18), "Manage your team, customize players and compete in style", smallTextStyle);

            float panelY = topY + 54f;
            float leftW = 200f;

            // 1. Left Sidebar: MY TEAMS & OFFICIAL TEAMS (Image 3)
            GUI.DrawTexture(new Rect(50, panelY, leftW, 390f), cardBgTex);

            badgeStyle.fontSize = 11;
            badgeStyle.normal.textColor = new Color(0.7f, 0.8f, 0.9f);
            GUI.Label(new Rect(60, panelY + 10, leftW - 20, 16), "MY TEAMS", badgeStyle);

            // Alpha Team (Selected)
            Rect alphaR = new Rect(56, panelY + 30, leftW - 12, 42);
            GUI.DrawTexture(alphaR, selectedTeamIndex == 0 ? bluePillTex : darkBgTex);
            badgeStyle.fontSize = 12;
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(alphaR.x + 8, alphaR.y + 6, alphaR.width - 16, 18), "A  ALPHA", badgeStyle);
            smallTextStyle.fontSize = 9;
            smallTextStyle.normal.textColor = new Color(0.7f, 0.85f, 1.0f);
            GUI.Label(new Rect(alphaR.x + 8, alphaR.y + 22, alphaR.width - 16, 16), "4 / 6 Players", smallTextStyle);
            if (Event.current.type == EventType.MouseDown && alphaR.Contains(Event.current.mousePosition))
            {
                selectedTeamIndex = 0;
                WCTAudioSynthesizer.Instance?.PlayUIClick();
            }

            // Omega Team
            Rect omegaR = new Rect(56, panelY + 76, leftW - 12, 42);
            GUI.DrawTexture(omegaR, selectedTeamIndex == 1 ? bluePillTex : darkBgTex);
            badgeStyle.fontSize = 12;
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(omegaR.x + 8, omegaR.y + 6, omegaR.width - 16, 18), "Ω  OMEGA", badgeStyle);
            smallTextStyle.fontSize = 9;
            smallTextStyle.normal.textColor = new Color(0.9f, 0.7f, 0.5f);
            GUI.Label(new Rect(omegaR.x + 8, omegaR.y + 22, omegaR.width - 16, 16), "5 / 6 Players", smallTextStyle);
            if (Event.current.type == EventType.MouseDown && omegaR.Contains(Event.current.mousePosition))
            {
                selectedTeamIndex = 1;
                WCTAudioSynthesizer.Instance?.PlayUIClick();
            }

            // + Create Team
            Rect createR = new Rect(56, panelY + 122, leftW - 12, 34);
            GUI.DrawTexture(createR, darkBgTex);
            badgeStyle.fontSize = 10;
            badgeStyle.normal.textColor = Color.cyan;
            GUI.Label(createR, "+ CREATE TEAM", badgeStyle);

            // Official Teams
            badgeStyle.fontSize = 11;
            badgeStyle.normal.textColor = new Color(0.7f, 0.8f, 0.9f);
            GUI.Label(new Rect(60, panelY + 168, leftW - 20, 16), "OFFICIAL TEAMS", badgeStyle);

            string[] offTeams = { "ALPHA Pro Team", "OMEGA Pro Team", "NOVA Pro Team", "PHOENIX Pro Team" };
            for (int t = 0; t < offTeams.Length; t++)
            {
                float ty = panelY + 188 + t * 30;
                smallTextStyle.fontSize = 10;
                smallTextStyle.normal.textColor = Color.white;
                GUI.Label(new Rect(64, ty, leftW - 28, 20), offTeams[t], smallTextStyle);
            }

            // < BACK Button
            if (GUI.Button(new Rect(56, panelY + 348, leftW - 12, 32), "< BACK"))
            {
                currentScreen = MenuScreen.MainMenu;
            }

            // 2. Center Panel: TEAM HERO SHOWCASE & TEAM MEMBERS (Image 3)
            float centerX = 50 + leftW + 15f;
            float centerW = 570f;
            GUI.DrawTexture(new Rect(centerX, panelY, centerW, 390f), cardBgTex);

            // Header: Logo + ALPHA + Motto + EDIT TEAM button
            titleStyle.fontSize = 20;
            titleStyle.normal.textColor = new Color(0.2f, 0.85f, 1.0f);
            GUI.Label(new Rect(centerX + 16, panelY + 12, 140, 24), "A  ALPHA", titleStyle);

            smallTextStyle.fontSize = 10;
            smallTextStyle.normal.textColor = new Color(0.6f, 0.75f, 0.95f);
            GUI.Label(new Rect(centerX + 140, panelY + 16, 260, 20), "FASTER  •  SMARTER  •  STRONGER", smallTextStyle);

            if (GUI.Button(new Rect(centerX + centerW - 110, panelY + 10, 95, 26), "EDIT TEAM"))
            {
                WCTAudioSynthesizer.Instance?.PlayUIClick();
            }

            // Team Hero Showcase: 4 Athletes in Jerseys (Player01, RohanK, VikramS, AaravP)
            float heroH = 180f;
            GUI.DrawTexture(new Rect(centerX + 12, panelY + 40, centerW - 24, heroH), darkBgTex);

            string[] athletes = { "A1  Player01\nCaptain", "A2  RohanK\nMember", "A3  VikramS\nMember", "A4  AaravP\nMember" };
            float athW = (centerW - 36f) / 4f;
            for (int a = 0; a < 4; a++)
            {
                float ax = centerX + 12 + a * athW;
                // Silhouette / Jersey
                GUI.color = selectedTeamIndex == 0 ? new Color(0.1f, 0.5f, 0.9f) : new Color(0.9f, 0.45f, 0.1f);
                GUI.DrawTexture(new Rect(ax + 12, panelY + 55, athW - 24, 100), Texture2D.whiteTexture);
                GUI.color = Color.white;

                badgeStyle.fontSize = 10;
                badgeStyle.normal.textColor = Color.white;
                GUI.Label(new Rect(ax, panelY + 160, athW, 36), athletes[a], badgeStyle);
            }

            // TEAM MEMBERS (4 / 6)
            float memY = panelY + 230f;
            headerStyle.fontSize = 12;
            headerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(centerX + 16, memY, 200, 18), "TEAM MEMBERS (4 / 6)", headerStyle);

            string[] memNames = { "Player01\nLvl 12", "RohanK\nLvl 10", "VikramS\nLvl 8", "AaravP\nLvl 6" };
            float mw = (centerW - 36f) / 6f;
            for (int m = 0; m < 4; m++)
            {
                float mx = centerX + 12 + m * mw;
                GUI.DrawTexture(new Rect(mx + 6, memY + 24, mw - 12, 70), darkBgTex);
                // Circular portrait
                GUI.DrawTexture(new Rect(mx + mw * 0.5f - 14, memY + 30, 28, 28), Texture2D.whiteTexture);
                // Online green dot
                GUI.color = Color.green;
                GUI.DrawTexture(new Rect(mx + mw * 0.5f + 8, memY + 50, 6, 6), Texture2D.whiteTexture);
                GUI.color = Color.white;

                badgeStyle.fontSize = 9;
                badgeStyle.normal.textColor = Color.white;
                GUI.Label(new Rect(mx, memY + 62, mw, 30), memNames[m], badgeStyle);
            }

            // 2 Invite Slots
            for (int s = 4; s < 6; s++)
            {
                float sx = centerX + 12 + s * mw;
                GUI.DrawTexture(new Rect(sx + 6, memY + 24, mw - 12, 70), darkBgTex);
                badgeStyle.fontSize = 10;
                badgeStyle.normal.textColor = Color.cyan;
                GUI.Label(new Rect(sx, memY + 45, mw, 30), "+ Add Player\nInvite friend", badgeStyle);
            }

            // 3. Right Column: TEAM STATS, TEAM COLOR & LOGO, TEAM JERSEY PREVIEW, SAVE CHANGES (Image 3)
            float rightX = centerX + centerW + 15f;
            float rightW = sw - rightX - 50f;
            GUI.DrawTexture(new Rect(rightX, panelY, rightW, 390f), cardBgTex);

            headerStyle.fontSize = 12;
            headerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(rightX + 14, panelY + 10, rightW - 28, 18), "TEAM STATS", headerStyle);

            float sY = panelY + 30f;
            DrawResultDetailRow(rightX + 14, sY, rightW - 28, "Total Matches", "18");
            DrawResultDetailRow(rightX + 14, sY + 20, rightW - 28, "Wins", "12");
            DrawResultDetailRow(rightX + 14, sY + 40, rightW - 28, "Losses", "6");
            DrawResultDetailRow(rightX + 14, sY + 60, rightW - 28, "Win Rate", "67%");
            DrawResultDetailRow(rightX + 14, sY + 80, rightW - 28, "Total Escapes", "44");
            DrawResultDetailRow(rightX + 14, sY + 100, rightW - 28, "Total Tags", "38");
            DrawResultDetailRow(rightX + 14, sY + 120, rightW - 28, "Avg. Chase Time", "00:31");

            // TEAM COLOR & LOGO
            float colY = sY + 145f;
            GUI.Label(new Rect(rightX + 14, colY, rightW - 28, 18), "TEAM COLOR & LOGO", headerStyle);

            Color[] colors = { Color.cyan, Color.red, new Color(1f, 0.5f, 0.1f), new Color(0.7f, 0.2f, 0.9f), Color.green, Color.yellow };
            for (int c = 0; c < colors.Length; c++)
            {
                GUI.color = colors[c];
                GUI.DrawTexture(new Rect(rightX + 14 + c * 24, colY + 22, 18, 18), Texture2D.whiteTexture);
            }
            GUI.color = Color.white;

            // TEAM JERSEY PREVIEW
            float jY = colY + 50f;
            GUI.Label(new Rect(rightX + 14, jY, rightW - 28, 18), "TEAM JERSEY PREVIEW", headerStyle);
            float jw = (rightW - 38f) / 4f;
            for (int j = 0; j < 4; j++)
            {
                GUI.DrawTexture(new Rect(rightX + 14 + j * (jw + 4), jY + 22, jw, 38), darkBgTex);
            }

            // SAVE CHANGES BUTTON
            if (GUI.Button(new Rect(rightX + 14, panelY + 345, rightW - 28, 36), "SAVE CHANGES", blueBtnStyle))
            {
                WCTAudioSynthesizer.Instance?.PlayUIClick();
                currentScreen = MenuScreen.PreMatchIntro;
            }
        }

        private void DrawTeamAlphaPanel(Rect rect)
        {
            GUI.DrawTexture(rect, cardBgTex);
            if (selectedTeamIndex == 0)
            {
                GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 3), alphaCyanTex);
            }

            // Header
            titleStyle.fontSize = 20;
            titleStyle.normal.textColor = new Color(0.2f, 0.8f, 1.0f);
            GUI.Label(new Rect(rect.x + 16, rect.y + 12, rect.width - 32, 24), "ALPHA", titleStyle);
            smallTextStyle.fontSize = 10;
            smallTextStyle.normal.textColor = new Color(0.6f, 0.75f, 0.9f);
            GUI.Label(new Rect(rect.x + 16, rect.y + 34, rect.width - 32, 16), "SPEED. CONTROL. PRECISION.", smallTextStyle);

            // 4 Athletes Grid (A1 RYAN, A2 KAI, A3 ZARA, A4 LEO)
            var roster = WCTMatchController.Instance?.TeamARoster;
            float cardW = (rect.width - 40f) * 0.5f;
            float cardH = 92f;
            float startX = rect.x + 14f;
            float startY = rect.y + 58f;

            if (roster != null && roster.Count >= 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    float cx = startX + (i % 2) * (cardW + 12f);
                    float cy = startY + (i / 2) * (cardH + 10f);
                    bool isSel = (selectedTeamIndex == 0 && selectedAthleteIndex == i);

                    DrawRosterAthleteThumbnail(new Rect(cx, cy, cardW, cardH), roster[i], isSel, 0, i);
                }
            }

            // Team Info Card
            float infoY = rect.y + rect.height - 95f;
            GUI.DrawTexture(new Rect(rect.x + 14, infoY, rect.width - 28, 85), darkBgTex);
            badgeStyle.normal.textColor = Color.cyan;
            GUI.Label(new Rect(rect.x + 20, infoY + 6, 120, 18), "TEAM INFO", badgeStyle);
            smallTextStyle.fontSize = 10;
            smallTextStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(rect.x + 20, infoY + 24, rect.width - 40, 56), "A well-balanced team focused on speed, control and precision movement. Perfect for tactical players.", smallTextStyle);
        }

        private void DrawTeamOmegaPanel(Rect rect)
        {
            GUI.DrawTexture(rect, cardBgTex);
            if (selectedTeamIndex == 1)
            {
                GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 3), omegaOrangeTex);
            }

            // Header
            titleStyle.fontSize = 20;
            titleStyle.normal.textColor = new Color(1.0f, 0.55f, 0.15f);
            GUI.Label(new Rect(rect.x + 16, rect.y + 12, rect.width - 32, 24), "OMEGA", titleStyle);
            smallTextStyle.fontSize = 10;
            smallTextStyle.normal.textColor = new Color(0.9f, 0.7f, 0.5f);
            GUI.Label(new Rect(rect.x + 16, rect.y + 34, rect.width - 32, 16), "POWER. AGGRESSION. PRESSURE.", smallTextStyle);

            // 4 Athletes Grid (O1 DANTE, O2 RAVI, O3 MAYA, O4 TYLER)
            var roster = WCTMatchController.Instance?.TeamBRoster;
            float cardW = (rect.width - 40f) * 0.5f;
            float cardH = 92f;
            float startX = rect.x + 14f;
            float startY = rect.y + 58f;

            if (roster != null && roster.Count >= 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    float cx = startX + (i % 2) * (cardW + 12f);
                    float cy = startY + (i / 2) * (cardH + 10f);
                    bool isSel = (selectedTeamIndex == 1 && selectedAthleteIndex == i);

                    DrawRosterAthleteThumbnail(new Rect(cx, cy, cardW, cardH), roster[i], isSel, 1, i);
                }
            }

            // Team Info Card
            float infoY = rect.y + rect.height - 95f;
            GUI.DrawTexture(new Rect(rect.x + 14, infoY, rect.width - 28, 85), darkBgTex);
            badgeStyle.normal.textColor = Color.orange;
            GUI.Label(new Rect(rect.x + 20, infoY + 6, 120, 18), "TEAM INFO", badgeStyle);
            smallTextStyle.fontSize = 10;
            smallTextStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(rect.x + 20, infoY + 24, rect.width - 40, 56), "An aggressive team built for high pressure play. Focuses on power, fast chases and dominant movement.", smallTextStyle);
        }

        private void DrawRosterAthleteThumbnail(Rect rect, WCTAthlete athlete, bool isSelected, int teamIdx, int athleteIdx)
        {
            GUI.DrawTexture(rect, isSelected ? cardSelectedTex : darkBgTex);
            if (isSelected)
            {
                GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 2), teamIdx == 0 ? alphaCyanTex : omegaOrangeTex);
            }

            badgeStyle.fontSize = 11;
            badgeStyle.normal.textColor = teamIdx == 0 ? Color.cyan : Color.orange;
            GUI.Label(new Rect(rect.x + 8, rect.y + 6, 40, 18), athlete.codeId, badgeStyle);

            headerStyle.fontSize = 13;
            headerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(rect.x + 48, rect.y + 6, rect.width - 55, 18), athlete.Name, headerStyle);

            smallTextStyle.fontSize = 9;
            smallTextStyle.normal.textColor = new Color(0.7f, 0.8f, 0.9f);
            GUI.Label(new Rect(rect.x + 8, rect.y + 26, rect.width - 16, 16), athlete.roleArchetype, smallTextStyle);

            // Mini stats
            GUI.Label(new Rect(rect.x + 8, rect.y + 45, rect.width - 16, 16), $"SPD {athlete.speedStat / 10}  AGI {athlete.agilityStat / 10}", smallTextStyle);
            GUI.Label(new Rect(rect.x + 8, rect.y + 62, rect.width - 16, 16), $"STA {athlete.staminaStat / 10}  CTRL {athlete.controlStat / 10}", smallTextStyle);

            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                selectedTeamIndex = teamIdx;
                selectedAthleteIndex = athleteIdx;
                WCTAudioSynthesizer.Instance?.PlayUIClick();
            }
        }

        private void DrawAthleteShowcase(Rect rect)
        {
            GUI.DrawTexture(rect, cardBgTex);

            var roster = (selectedTeamIndex == 0) ? WCTMatchController.Instance?.TeamARoster : WCTMatchController.Instance?.TeamBRoster;
            if (roster == null || selectedAthleteIndex >= roster.Count) return;
            var athlete = roster[selectedAthleteIndex];

            // 3D Athlete Silhouette Box (Left half)
            float viewW = 210f;
            float viewH = rect.height - 30f;
            GUI.DrawTexture(new Rect(rect.x + 15, rect.y + 15, viewW, viewH), darkBgTex);

            // Color block jersey inside view
            Color accent = selectedTeamIndex == 0 ? new Color(0.1f, 0.6f, 0.95f) : new Color(0.95f, 0.45f, 0.1f);
            GUI.DrawTexture(new Rect(rect.x + 45, rect.y + 65, viewW - 60, 130), selectedTeamIndex == 0 ? alphaCyanTex : omegaOrangeTex);

            badgeStyle.fontSize = 13;
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(rect.x + 15, rect.y + viewH - 45, viewW, 20), athlete.codeId + " • " + athlete.Name, badgeStyle);

            // Right half: Detailed Stat Sheet (Image 4)
            float sheetX = rect.x + viewW + 30f;
            float sheetW = rect.width - viewW - 45f;
            float sheetY = rect.y + 18f;

            titleStyle.fontSize = 20;
            titleStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(sheetX, sheetY, sheetW, 26), $"{athlete.codeId}  {athlete.Name}", titleStyle);

            subtitleStyle.fontSize = 12;
            subtitleStyle.normal.textColor = accent;
            GUI.Label(new Rect(sheetX, sheetY + 26, sheetW, 20), athlete.roleArchetype, subtitleStyle);

            // 4 Attribute Sliders (Speed, Agility, Stamina, Control)
            float barY = sheetY + 56f;
            DrawAttributePill(sheetX, barY, sheetW, "Speed", athlete.speedStat / 10);
            DrawAttributePill(sheetX, barY + 28, sheetW, "Agility", athlete.agilityStat / 10);
            DrawAttributePill(sheetX, barY + 56, sheetW, "Stamina", athlete.staminaStat / 10);
            DrawAttributePill(sheetX, barY + 84, sheetW, "Control", athlete.controlStat / 10);

            // Special Ability Card
            float abY = barY + 120f;
            GUI.DrawTexture(new Rect(sheetX, abY, sheetW, 70), darkBgTex);
            badgeStyle.fontSize = 11;
            badgeStyle.normal.textColor = Color.yellow;
            GUI.Label(new Rect(sheetX + 10, abY + 6, sheetW - 20, 18), $"SPECIAL ABILITY: {athlete.specialAbility}", badgeStyle);

            smallTextStyle.fontSize = 10;
            smallTextStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(sheetX + 10, abY + 26, sheetW - 20, 38), athlete.abilityDescription, smallTextStyle);

            // Select Athlete Button
            if (GUI.Button(new Rect(sheetX, abY + 82, sheetW, 36), "SELECT ATHLETE", blueBtnStyle))
            {
                WCTAudioSynthesizer.Instance?.PlayUIClick();
            }
        }

        private void DrawAttributePill(float x, float y, float w, string label, int val)
        {
            cardStatStyle.fontSize = 11;
            cardStatStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(x, y, 65, 18), label, cardStatStyle);

            float barX = x + 70;
            float barW = w - 105;
            GUI.DrawTexture(new Rect(barX, y + 4, barW, 10), statBarBgTex);
            GUI.DrawTexture(new Rect(barX, y + 4, barW * (val / 10f), 10), statBarFillTex);

            cardStatStyle.normal.textColor = new Color(0.85f, 0.95f, 1.0f);
            GUI.Label(new Rect(barX + barW + 8, y, 25, 18), $"{val}", cardStatStyle);
        }

        private void DrawTeamSelectionBottomBar(float sw, float sh)
        {
            float barH = 58f;
            float barY = sh - barH - 12f;
            GUI.DrawTexture(new Rect(50, barY, sw - 100f, barH), cardBgTex);

            float cx = 70f;
            smallTextStyle.fontSize = 11;
            smallTextStyle.normal.textColor = Color.white;

            // Match Format
            GUI.Label(new Rect(cx, barY + 18, 90, 24), "MATCH FORMAT:", smallTextStyle);
            if (GUI.Button(new Rect(cx + 95, barY + 12, 170, 32), $"< {selectedMode} >"))
            {
                selectedMode = (selectedMode == GameMode.FullTournamentMatch) ? GameMode.QuickMatch : GameMode.FullTournamentMatch;
            }

            // AI Difficulty
            cx += 295f;
            GUI.Label(new Rect(cx, barY + 18, 90, 24), "AI DIFFICULTY:", smallTextStyle);
            if (GUI.Button(new Rect(cx + 95, barY + 12, 130, 32), $"< {selectedDifficulty} >"))
            {
                selectedDifficulty = (AIDifficulty)(((int)selectedDifficulty + 1) % 4);
            }

            // Continue Button (Right)
            if (GUI.Button(new Rect(sw - 230, barY + 10, 160, 38), "CONTINUE >", blueBtnStyle))
            {
                currentScreen = MenuScreen.PreMatchIntro;
                WCTAudioSynthesizer.Instance?.PlayUIClick();
            }
        }

        // ==============================================================
        // 5. ARENA SELECTION (Image 5 layout)
        // ==============================================================
        private void DrawArenaSelect(float sw, float sh)
        {
            float topY = 70f;
            headerStyle.fontSize = 22;
            headerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(50, topY, 400, 26), "ARENA SELECTION", headerStyle);

            smallTextStyle.fontSize = 11;
            smallTextStyle.normal.textColor = new Color(0.6f, 0.7f, 0.85f);
            GUI.Label(new Rect(50, topY + 24, 400, 18), "CHOOSE YOUR ARENA", smallTextStyle);

            float panelY = topY + 48f;
            float listW = 280f;
            float detailW = sw - 100f - listW - 20f;

            // Left Arena List
            DrawArenaListPanel(new Rect(50, panelY, listW, 350f));

            // Right Arena Detailed View & Mini-Map Schematic (Image 5)
            DrawArenaDetailPanel(new Rect(50 + listW + 20, panelY, detailW, 350f));

            // Bottom Carousel Thumbnails
            DrawArenaCarouselBottom(sw, sh);
        }

        private void DrawArenaListPanel(Rect rect)
        {
            GUI.DrawTexture(rect, cardBgTex);

            string[] arenaNames = { "QUAD (CLASSIC)", "URBAN ROOFTOP", "WAREHOUSE", "NIGHT ARENA", "CUSTOM ARENA" };
            string[] arenaSubs = { "Official-style arena", "High risk, open routes", "Tight spaces, quick turns", "Neon lights, intense", "Community made" };

            float startY = rect.y + 12f;
            float itemH = 60f;

            for (int i = 0; i < arenaNames.Length; i++)
            {
                Rect itemRect = new Rect(rect.x + 10, startY + i * (itemH + 6), rect.width - 20, itemH);
                bool isSel = selectedArenaIndex == i;

                GUI.DrawTexture(itemRect, isSel ? cardSelectedTex : darkBgTex);
                if (isSel)
                {
                    GUI.DrawTexture(new Rect(itemRect.x, itemRect.y, 4, itemRect.height), alphaCyanTex);
                }

                headerStyle.fontSize = 13;
                headerStyle.normal.textColor = isSel ? Color.cyan : Color.white;
                GUI.Label(new Rect(itemRect.x + 14, itemRect.y + 8, itemRect.width - 28, 20), arenaNames[i], headerStyle);

                smallTextStyle.fontSize = 10;
                smallTextStyle.normal.textColor = new Color(0.7f, 0.8f, 0.9f);
                GUI.Label(new Rect(itemRect.x + 14, itemRect.y + 28, itemRect.width - 28, 24), arenaSubs[i], smallTextStyle);

                if (Event.current.type == EventType.MouseDown && itemRect.Contains(Event.current.mousePosition))
                {
                    selectedArenaIndex = i;
                    WCTAudioSynthesizer.Instance?.PlayUIClick();
                }
            }
        }

        private void DrawArenaDetailPanel(Rect rect)
        {
            GUI.DrawTexture(rect, cardBgTex);

            // Left 3D View / Mini-Map schematic placeholder (Image 5)
            float viewW = rect.width * 0.58f;
            float viewH = rect.height - 24f;
            GUI.DrawTexture(new Rect(rect.x + 12, rect.y + 12, viewW, viewH), darkBgTex);

            // 2D Mini-Map Schematic of Quad
            float mapSize = 160f;
            float mapX = rect.x + 12 + (viewW - mapSize) * 0.5f;
            float mapY = rect.y + 12 + (viewH - mapSize) * 0.5f;
            GUI.DrawTexture(new Rect(mapX, mapY, mapSize, mapSize), panelBgTex);
            GUI.DrawTexture(new Rect(mapX + mapSize * 0.35f, mapY + mapSize * 0.35f, mapSize * 0.3f, mapSize * 0.3f), alphaCyanTex); // Mountain
            GUI.DrawTexture(new Rect(mapX + 15, mapY + 15, 30, 30), omegaOrangeTex); // Block 1
            GUI.DrawTexture(new Rect(mapX + mapSize - 45, mapY + mapSize - 45, 30, 30), omegaOrangeTex); // Block 2

            badgeStyle.fontSize = 12;
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(rect.x + 12, rect.y + viewH - 30, viewW, 20), "[ 2D QUAD TACTICAL SCHEMATIC ]", badgeStyle);

            // Right Specs & Description Pane
            float specX = rect.x + viewW + 28f;
            float specW = rect.width - viewW - 40f;
            float specY = rect.y + 16f;

            titleStyle.fontSize = 22;
            titleStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(specX, specY, specW, 26), GetArenaName(selectedArenaIndex), titleStyle);

            subtitleStyle.fontSize = 11;
            subtitleStyle.normal.textColor = Color.cyan;
            GUI.Label(new Rect(specX, specY + 28, specW, 20), "Official-style arena with balanced routes.", subtitleStyle);

            smallTextStyle.fontSize = 11;
            smallTextStyle.normal.textColor = new Color(0.8f, 0.9f, 1.0f);
            GUI.Label(new Rect(specX, specY + 54, specW, 70), "The classic Quad arena inspired by official World Chase Tag events. Balanced routes, multiple levels and clear sightlines make it perfect for competitive matches.", smallTextStyle);

            // Specs Table
            float tblY = specY + 130f;
            DrawSpecRow(specX, tblY, specW, "Size:", "12m x 12m");
            DrawSpecRow(specX, tblY + 24, specW, "Difficulty:", "Medium");
            DrawSpecRow(specX, tblY + 48, specW, "Best for:", "All Modes");

            // Select Arena Button
            if (GUI.Button(new Rect(specX, tblY + 95, specW, 40), "SELECT ARENA >", blueBtnStyle))
            {
                currentScreen = MenuScreen.PreMatchIntro;
                WCTAudioSynthesizer.Instance?.PlayUIClick();
            }
        }

        private void DrawSpecRow(float x, float y, float w, string label, string val)
        {
            cardStatStyle.fontSize = 12;
            cardStatStyle.normal.textColor = new Color(0.6f, 0.7f, 0.85f);
            GUI.Label(new Rect(x, y, 90, 20), label, cardStatStyle);

            cardStatStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(x + 95, y, w - 95, 20), val, cardStatStyle);
        }

        private void DrawArenaCarouselBottom(float sw, float sh)
        {
            float barY = sh - 95f;
            float startX = 50f;
            float cardW = (sw - 100f - 40f) / 5f;
            float cardH = 68f;

            string[] names = { "QUAD (CLASSIC)", "URBAN ROOFTOP", "WAREHOUSE", "NIGHT ARENA", "CUSTOM ARENA" };
            for (int i = 0; i < 5; i++)
            {
                Rect r = new Rect(startX + i * (cardW + 10), barY, cardW, cardH);
                bool isSel = selectedArenaIndex == i;

                GUI.DrawTexture(r, isSel ? cardSelectedTex : cardBgTex);
                if (isSel)
                {
                    GUI.DrawTexture(new Rect(r.x, r.y, r.width, 3), alphaCyanTex);
                }

                badgeStyle.fontSize = 11;
                badgeStyle.normal.textColor = isSel ? Color.cyan : Color.white;
                GUI.Label(new Rect(r.x, r.y + 24, r.width, 20), names[i], badgeStyle);

                if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
                {
                    selectedArenaIndex = i;
                    WCTAudioSynthesizer.Instance?.PlayUIClick();
                }
            }
        }

        // ==============================================================
        // 6. PRE-MATCH INTRO / MATCH SETUP (Image 1 layout)
        // ==============================================================
        private void DrawPreMatchIntro(float sw, float sh)
        {
            float topY = 70f;
            headerStyle.fontSize = 24;
            headerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(50, topY, 400, 28), "MATCH SETUP", headerStyle);

            smallTextStyle.fontSize = 11;
            smallTextStyle.normal.textColor = new Color(0.6f, 0.7f, 0.85f);
            GUI.Label(new Rect(50, topY + 26, 400, 18), "CONFIGURE YOUR MATCH", smallTextStyle);

            float panelY = topY + 50f;
            float colH = sh - panelY - 40f;
            float col1W = 320f;
            float col2W = 340f;
            float col3W = sw - 100f - col1W - col2W - 30f;

            // -------------------------------------------------------------
            // Column 1: Match Configuration Options (Image 1 Left)
            // -------------------------------------------------------------
            float col1X = 50f;
            GUI.DrawTexture(new Rect(col1X, panelY, col1W, colH), cardBgTex);

            // Team Box (ALPHA vs OMEGA)
            float tBoxH = 65f;
            GUI.DrawTexture(new Rect(col1X + 12, panelY + 12, col1W - 24, tBoxH), darkBgTex);
            GUI.DrawTexture(new Rect(col1X + 12, panelY + 12, 4, tBoxH), alphaCyanTex);

            badgeStyle.fontSize = 11;
            badgeStyle.normal.textColor = Color.cyan;
            GUI.Label(new Rect(col1X + 22, panelY + 18, 90, 18), "ALPHA", badgeStyle);
            smallTextStyle.fontSize = 9;
            smallTextStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(col1X + 22, panelY + 36, 90, 16), "Player Team", smallTextStyle);

            badgeStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(col1X + (col1W * 0.5f) - 15, panelY + 26, 30, 20), "VS", badgeStyle);

            badgeStyle.normal.textColor = Color.orange;
            GUI.Label(new Rect(col1X + col1W - 110, panelY + 18, 90, 18), "OMEGA", badgeStyle);
            GUI.Label(new Rect(col1X + col1W - 110, panelY + 36, 90, 16), "AI Team", smallTextStyle);

            // Match Configuration Dropdowns / Toggles
            float cfgY = panelY + 90f;
            float rowH = 46f;

            DrawMatchSetupToggle(col1X + 12, cfgY, col1W - 24, "MATCH FORMAT", selectedMode == GameMode.FullTournamentMatch ? "Standard Match (16 Chases)" : "Quick Match (4 Chases)", () =>
            {
                selectedMode = selectedMode == GameMode.FullTournamentMatch ? GameMode.QuickMatch : GameMode.FullTournamentMatch;
            });

            DrawMatchSetupToggle(col1X + 12, cfgY + rowH, col1W - 24, "CHASE TIME", "20 Seconds", null);
            DrawMatchSetupToggle(col1X + 12, cfgY + rowH * 2, col1W - 24, "BETWEEN CHASES", "2.5 Seconds", null);

            DrawMatchSetupToggle(col1X + 12, cfgY + rowH * 3, col1W - 24, "AI DIFFICULTY", $"< {selectedDifficulty} >", () =>
            {
                selectedDifficulty = (AIDifficulty)(((int)selectedDifficulty + 1) % 4);
            });

            DrawMatchSetupToggle(col1X + 12, cfgY + rowH * 4, col1W - 24, "STARTING ROLE", "< Auto (Decided by Rules) >", null);
            DrawMatchSetupToggle(col1X + 12, cfgY + rowH * 5, col1W - 24, "WEATHER / TIME", "< Indoor (Default) >", null);

            // -------------------------------------------------------------
            // Column 2: Active Arena & Rules Summary (Image 1 Center)
            // -------------------------------------------------------------
            float col2X = col1X + col1W + 15f;
            GUI.DrawTexture(new Rect(col2X, panelY, col2W, colH), cardBgTex);

            // ACTIVE ARENA Card
            float aY = panelY + 12f;
            headerStyle.fontSize = 15;
            headerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(col2X + 14, aY, col2W - 28, 22), "ACTIVE ARENA", headerStyle);

            float aBoxH = 155f;
            GUI.DrawTexture(new Rect(col2X + 12, aY + 24, col2W - 24, aBoxH), darkBgTex);

            titleStyle.fontSize = 16;
            titleStyle.normal.textColor = Color.cyan;
            GUI.Label(new Rect(col2X + 22, aY + 30, col2W - 44, 22), GetArenaName(selectedArenaIndex), titleStyle);

            smallTextStyle.fontSize = 10;
            smallTextStyle.normal.textColor = new Color(0.8f, 0.9f, 1.0f);
            GUI.Label(new Rect(col2X + 22, aY + 52, col2W - 44, 30), "Official-style arena with balanced parkour routes.", smallTextStyle);

            // Specs
            DrawSpecRow(col2X + 22, aY + 86, col2W - 44, "Size:", "12m x 12m");
            DrawSpecRow(col2X + 22, aY + 104, col2W - 44, "Difficulty:", "Medium");
            DrawSpecRow(col2X + 22, aY + 122, col2W - 44, "Best for:", "All Modes");

            // RULES SUMMARY Box (Image 1)
            float rBoxY = aY + aBoxH + 40f;
            headerStyle.fontSize = 15;
            GUI.Label(new Rect(col2X + 14, rBoxY, col2W - 28, 22), "RULES SUMMARY", headerStyle);

            float rGridY = rBoxY + 26f;
            GUI.DrawTexture(new Rect(col2X + 12, rGridY, col2W - 24, 115f), darkBgTex);

            DrawRulePill(col2X + 18, rGridY + 12, (col2W - 44) * 0.5f, "20s", "Chase Time");
            DrawRulePill(col2X + 18 + (col2W - 44) * 0.5f + 8, rGridY + 12, (col2W - 44) * 0.5f, "16", "Total Chases");
            DrawRulePill(col2X + 18, rGridY + 62, (col2W - 44) * 0.5f, "2.5s", "Between Chases");
            DrawRulePill(col2X + 18 + (col2W - 44) * 0.5f + 8, rGridY + 62, (col2W - 44) * 0.5f, "Evader", "Winner Stays On");

            // -------------------------------------------------------------
            // Column 3: Controls PC Reference & Start Match (Image 1 Right)
            // -------------------------------------------------------------
            float col3X = col2X + col2W + 15f;
            GUI.DrawTexture(new Rect(col3X, panelY, col3W, colH), cardBgTex);

            headerStyle.fontSize = 15;
            headerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(col3X + 14, panelY + 12, col3W - 28, 22), "CONTROLS (PC)", headerStyle);

            float keyY = panelY + 40f;
            float kRowH = 26f;
            DrawKeybindRow(col3X + 14, keyY, col3W - 28, "Move Forward", "W");
            DrawKeybindRow(col3X + 14, keyY + kRowH, col3W - 28, "Move Backward", "S");
            DrawKeybindRow(col3X + 14, keyY + kRowH * 2, col3W - 28, "Move Left", "A");
            DrawKeybindRow(col3X + 14, keyY + kRowH * 3, col3W - 28, "Move Right", "D");
            DrawKeybindRow(col3X + 14, keyY + kRowH * 4, col3W - 28, "Jump / Vault", "Space");
            DrawKeybindRow(col3X + 14, keyY + kRowH * 5, col3W - 28, "Crouch / Slide", "Left Ctrl");
            DrawKeybindRow(col3X + 14, keyY + kRowH * 6, col3W - 28, "Sprint", "Left Shift");
            DrawKeybindRow(col3X + 14, keyY + kRowH * 7, col3W - 28, "Attempt Tag", "F");
            DrawKeybindRow(col3X + 14, keyY + kRowH * 8, col3W - 28, "Pause Menu", "Esc");
            DrawKeybindRow(col3X + 14, keyY + kRowH * 9, col3W - 28, "Look Around", "Mouse");

            // Large Blue Glowing "START MATCH >" Button (Image 1)
            float sBtnW = col3W - 28f;
            float sBtnH = 50f;
            float sBtnY = panelY + colH - sBtnH - 14f;

            if (GUI.Button(new Rect(col3X + 14, sBtnY, sBtnW, sBtnH), "►  START MATCH  ►", blueBtnStyle))
            {
                LaunchMatch();
            }
        }

        private void DrawMatchSetupToggle(float x, float y, float w, string label, string val, Action onClick)
        {
            smallTextStyle.fontSize = 10;
            smallTextStyle.normal.textColor = new Color(0.6f, 0.7f, 0.85f);
            GUI.Label(new Rect(x, y, w, 16), label, smallTextStyle);

            if (GUI.Button(new Rect(x, y + 16, w, 28), val))
            {
                onClick?.Invoke();
                WCTAudioSynthesizer.Instance?.PlayUIClick();
            }
        }

        private void DrawRulePill(float x, float y, float w, string topVal, string bottomLabel)
        {
            GUI.DrawTexture(new Rect(x, y, w, 44), panelBgTex);
            headerStyle.fontSize = 14;
            headerStyle.normal.textColor = Color.cyan;
            headerStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(x, y + 4, w, 18), topVal, headerStyle);

            smallTextStyle.fontSize = 9;
            smallTextStyle.normal.textColor = Color.white;
            smallTextStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(x, y + 22, w, 16), bottomLabel, smallTextStyle);

            headerStyle.alignment = TextAnchor.MiddleLeft;
            smallTextStyle.alignment = TextAnchor.MiddleLeft;
        }

        // ==============================================================
        // 7. TRAINING DRILLS (Image 1, Screen 12)
        // ==============================================================
        private void DrawTrainingDrills(float sw, float sh)
        {
            float topY = 70f;
            headerStyle.fontSize = 22;
            headerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(50, topY, 400, 26), "PARKOUR TRAINING & DRILLS", headerStyle);

            float panelY = topY + 48f;
            float listW = 280f;
            float detailW = sw - 100f - listW - 20f;

            // Left Drill List
            GUI.DrawTexture(new Rect(50, panelY, listW, 360f), cardBgTex);
            string[] drills = { "BASIC MOVEMENT", "JUMPS & VAULTS", "ROUTE PRACTICE", "TAG PRACTICE", "AI CHASE DRILL", "FREE RUN" };

            for (int i = 0; i < drills.Length; i++)
            {
                Rect r = new Rect(60, panelY + 14 + i * 56, listW - 20, 48);
                bool isSel = trainingActiveDrill == i;

                GUI.DrawTexture(r, isSel ? cardSelectedTex : darkBgTex);
                if (isSel)
                {
                    GUI.DrawTexture(new Rect(r.x, r.y, 4, r.height), alphaCyanTex);
                }

                badgeStyle.fontSize = 12;
                badgeStyle.normal.textColor = isSel ? Color.cyan : Color.white;
                GUI.Label(new Rect(r.x, r.y + 14, r.width, 20), drills[i], badgeStyle);

                if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
                {
                    trainingActiveDrill = i;
                    WCTAudioSynthesizer.Instance?.PlayUIClick();
                }
            }

            // Right Drill Detail & Start Button
            GUI.DrawTexture(new Rect(50 + listW + 20, panelY, detailW, 360f), cardBgTex);
            titleStyle.fontSize = 22;
            titleStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(50 + listW + 45, panelY + 20, detailW - 50, 28), drills[trainingActiveDrill], titleStyle);

            smallTextStyle.fontSize = 12;
            smallTextStyle.normal.textColor = new Color(0.8f, 0.9f, 1.0f);
            GUI.Label(new Rect(50 + listW + 45, panelY + 60, detailW - 90, 80), GetDrillDescription(trainingActiveDrill), smallTextStyle);

            if (GUI.Button(new Rect(50 + listW + 45, panelY + 280, 220, 44), "START DRILL", blueBtnStyle))
            {
                selectedMode = GameMode.Practice;
                LaunchMatch();
            }
        }

        private string GetDrillDescription(int drill)
        {
            switch (drill)
            {
                case 0: return "Master sprint momentum, responsive WASD directional strafing, and stamina budgeting.";
                case 1: return "Sprint towards low rails and press Space to execute fluid hurdle vaults without loss of speed.";
                case 2: return "Practice optimal Quad perimeter routes, mantle pull-ups on central mountain, and slide escapes.";
                case 3: return "Close distance to within 1.5m of the runner and press Key C or Left Click to land tags.";
                case 4: return "1-on-1 dynamic chase drill against adaptive AI to sharpen evade reaction times.";
                default: return "Full open arena free-roam with no clock limits.";
            }
        }

        // ==============================================================
        // 8. SETTINGS & GRAPHICS (Image 1, Screen 13)
        // ==============================================================
        private void DrawSettings(float sw, float sh)
        {
            float topY = 70f;
            headerStyle.fontSize = 22;
            headerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(50, topY, 400, 26), "SETTINGS", headerStyle);

            float panelY = topY + 45f;
            float panelW = sw - 100f;
            float panelH = sh - panelY - 45f;

            GUI.DrawTexture(new Rect(50, panelY, panelW, panelH), cardBgTex);

            // Settings Tabs: GRAPHICS, AUDIO, CONTROLS, GAMEPLAY, ACCESSIBILITY
            float tabStartX = 65f;
            float tabW = 140f;
            string[] sTabs = { "GRAPHICS", "AUDIO", "CONTROLS", "GAMEPLAY", "ACCESSIBILITY" };

            for (int i = 0; i < sTabs.Length; i++)
            {
                Rect tr = new Rect(tabStartX + i * (tabW + 10), panelY + 12, tabW, 34);
                bool isSel = settingsActiveTab == i;

                if (isSel) GUI.DrawTexture(tr, bluePillTex);
                tabBtnStyle.normal.textColor = isSel ? Color.white : new Color(0.7f, 0.8f, 0.9f);
                if (GUI.Button(tr, sTabs[i], tabBtnStyle))
                {
                    settingsActiveTab = i;
                    WCTAudioSynthesizer.Instance?.PlayUIClick();
                }
            }

            // Tab Content
            float cy = panelY + 65f;
            if (settingsActiveTab == 0) // Graphics
            {
                DrawGraphicsSettingsTab(50 + 25, cy, panelW - 50);
            }
            else if (settingsActiveTab == 1) // Audio
            {
                DrawAudioSettingsTab(50 + 25, cy, panelW - 50);
            }
            else if (settingsActiveTab == 2) // Controls
            {
                DrawControlsTab(50 + 25, cy, panelW - 50);
            }
            else
            {
                smallTextStyle.normal.textColor = Color.white;
                GUI.Label(new Rect(50 + 25, cy + 20, 400, 24), "Configured to optimal PC tournament standard profile.", smallTextStyle);
            }

            // Bottom Apply & Reset buttons
            float botY = panelY + panelH - 52f;
            if (GUI.Button(new Rect(panelW - 240, botY, 110, 36), "RESET"))
            {
                LoadSettings();
            }

            if (GUI.Button(new Rect(panelW - 115, botY, 130, 36), "APPLY", blueBtnStyle))
            {
                ApplySettings();
                WCTAudioSynthesizer.Instance?.PlayUIClick();
            }
        }

        private void DrawGraphicsSettingsTab(float x, float y, float w)
        {
            float halfW = (w - 40f) * 0.5f;

            // Left Column
            smallTextStyle.fontSize = 12;
            smallTextStyle.normal.textColor = Color.white;

            GUI.Label(new Rect(x, y + 10, 160, 22), "Graphics Quality:", smallTextStyle);
            float qbw = 65f;
            string[] qnames = { "Low", "Med", "High", "Ultra" };
            for (int i = 0; i < 4; i++)
            {
                if (GUI.Button(new Rect(x + 150 + i * (qbw + 4), y + 6, qbw, 26), qnames[i]))
                {
                    qualityIndex = i;
                    QualitySettings.SetQualityLevel(i, true);
                }
            }

            GUI.Label(new Rect(x, y + 45, 160, 22), "Fullscreen:", smallTextStyle);
            isFullscreen = GUI.Toggle(new Rect(x + 150, y + 45, 120, 22), isFullscreen, isFullscreen ? " Enabled" : " Windowed");
            Screen.fullScreen = isFullscreen;

            GUI.Label(new Rect(x, y + 80, 160, 22), "V-Sync (60 FPS Cap):", smallTextStyle);
            vSyncEnabled = GUI.Toggle(new Rect(x + 150, y + 80, 120, 22), vSyncEnabled, vSyncEnabled ? " On" : " Off");
            QualitySettings.vSyncCount = vSyncEnabled ? 1 : 0;

            // Right Column
            float rx = x + halfW + 40f;
            GUI.Label(new Rect(rx, y + 10, 160, 22), $"Field of View (FOV): {fov:F0}°", smallTextStyle);
            fov = GUI.HorizontalSlider(new Rect(rx + 170, y + 14, 180, 18), fov, 65f, 105f);

            GUI.Label(new Rect(rx, y + 45, 160, 22), $"Mouse Sensitivity: {mouseSensitivity:F1}", smallTextStyle);
            mouseSensitivity = GUI.HorizontalSlider(new Rect(rx + 170, y + 49, 180, 18), mouseSensitivity, 0.5f, 5.0f);

            GUI.Label(new Rect(rx, y + 80, 160, 22), "Motion Blur:", smallTextStyle);
            motionBlur = GUI.Toggle(new Rect(rx + 170, y + 80, 120, 22), motionBlur, motionBlur ? " On" : " Off");

            GUI.Label(new Rect(rx, y + 115, 160, 22), "Invert Y-Axis Pitch:", smallTextStyle);
            invertY = GUI.Toggle(new Rect(rx + 170, y + 115, 120, 22), invertY, invertY ? " Enabled" : " Disabled");
        }

        private void DrawAudioSettingsTab(float x, float y, float w)
        {
            smallTextStyle.fontSize = 12;
            smallTextStyle.normal.textColor = Color.white;

            GUI.Label(new Rect(x, y + 15, 180, 24), $"Master Volume: {Mathf.RoundToInt(masterVolume * 100)}%", smallTextStyle);
            masterVolume = GUI.HorizontalSlider(new Rect(x + 200, y + 18, 250, 18), masterVolume, 0f, 1f);
            AudioListener.volume = masterVolume;

            GUI.Label(new Rect(x, y + 55, 180, 24), $"SFX & Referee Horn: {Mathf.RoundToInt(sfxVolume * 100)}%", smallTextStyle);
            sfxVolume = GUI.HorizontalSlider(new Rect(x + 200, y + 58, 250, 18), sfxVolume, 0f, 1f);
        }

        private void DrawControlsTab(float x, float y, float w)
        {
            smallTextStyle.fontSize = 12;
            smallTextStyle.normal.textColor = Color.white;

            float rowH = 26f;
            DrawKeybindRow(x, y, 400, "Move Forward", "W");
            DrawKeybindRow(x, y + rowH, 400, "Move Backward", "S");
            DrawKeybindRow(x, y + rowH * 2, 400, "Move Left", "A");
            DrawKeybindRow(x, y + rowH * 3, 400, "Move Right", "D");
            DrawKeybindRow(x, y + rowH * 4, 400, "Sprint", "Left Shift");
            DrawKeybindRow(x + 440, y, 400, "Jump / Vault", "Space");
            DrawKeybindRow(x + 440, y + rowH, 400, "Crouch / Slide", "Left Ctrl / C");
            DrawKeybindRow(x + 440, y + rowH * 2, 400, "Attempt Tag", "F");
            DrawKeybindRow(x + 440, y + rowH * 3, 400, "Pause Menu", "Escape");
        }

        private void DrawKeybindRow(float x, float y, float w, string action, string key)
        {
            cardStatStyle.fontSize = 12;
            cardStatStyle.normal.textColor = new Color(0.7f, 0.8f, 0.9f);
            GUI.Label(new Rect(x, y, 160, 20), action, cardStatStyle);

            GUI.DrawTexture(new Rect(x + 170, y, 100, 20), darkBgTex);
            badgeStyle.fontSize = 11;
            badgeStyle.normal.textColor = Color.yellow;
            GUI.Label(new Rect(x + 170, y + 2, 100, 18), key, badgeStyle);
        }

        // ==============================================================
        // KEYBOARD & MOUSE CONTROLS (Image 2)
        // ==============================================================
        private void DrawControls(float sw, float sh)
        {
            float topY = 70f;
            titleStyle.fontSize = 24;
            titleStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(50, topY, 500, 28), "KEYBOARD & MOUSE CONTROLS", titleStyle);

            smallTextStyle.fontSize = 11;
            smallTextStyle.normal.textColor = new Color(0.6f, 0.7f, 0.85f);
            GUI.Label(new Rect(50, topY + 28, 500, 18), "Customize your controls for the best chase experience", smallTextStyle);

            float panelY = topY + 54f;

            // Subtabs under header: MOVEMENT & ACTIONS, CAMERA & VIEW, GAMEPLAY, UI & MENU
            float subW = 150f;
            string[] subtabs = { "MOVEMENT & ACTIONS", "CAMERA & VIEW", "GAMEPLAY", "UI & MENU" };
            for (int i = 0; i < subtabs.Length; i++)
            {
                Rect tr = new Rect(50 + i * (subW + 10), panelY, subW, 30);
                bool isSel = (controlsActiveSubTab == i);
                GUI.DrawTexture(tr, isSel ? bluePillTex : darkBgTex);
                badgeStyle.fontSize = 10;
                badgeStyle.normal.textColor = isSel ? Color.white : new Color(0.7f, 0.8f, 0.9f);
                GUI.Label(tr, subtabs[i], badgeStyle);
                if (Event.current.type == EventType.MouseDown && tr.Contains(Event.current.mousePosition))
                {
                    controlsActiveSubTab = i;
                    WCTAudioSynthesizer.Instance?.PlayUIClick();
                }
            }

            float contentY = panelY + 38f;
            float leftW = 390f;

            // 1. Left Table: ACTION | PRIMARY KEY | SECONDARY KEY
            GUI.DrawTexture(new Rect(50, contentY, leftW, 350f), cardBgTex);
            headerStyle.fontSize = 10;
            headerStyle.normal.textColor = new Color(0.6f, 0.7f, 0.85f);
            GUI.Label(new Rect(65, contentY + 8, 140, 18), "ACTION", headerStyle);
            GUI.Label(new Rect(230, contentY + 8, 90, 18), "PRIMARY KEY", headerStyle);
            GUI.Label(new Rect(340, contentY + 8, 90, 18), "SECONDARY", headerStyle);

            string[,] keybinds = {
                { "Move Forward", "W", "-" },
                { "Move Backward", "S", "-" },
                { "Move Left", "A", "-" },
                { "Move Right", "D", "-" },
                { "Sprint", "Left Shift", "Right Shift" },
                { "Jump / Vault", "Space", "-" },
                { "Slide (Under Obstacle)", "Left Ctrl", "C" },
                { "Climb / Mantle", "E", "-" },
                { "Tag (Chaser)", "F", "-" },
                { "Interact / Pickup", "E", "-" },
                { "Camera Look (Hold)", "Right Mouse", "-" },
                { "Toggle Perspective", "V", "-" },
                { "Pause Menu", "Esc", "-" },
                { "Show Scoreboard", "Tab", "-" }
            };

            float rowStartY = contentY + 30f;
            float rowH = 22f;
            for (int r = 0; r < 14; r++)
            {
                float ry = rowStartY + r * rowH;
                smallTextStyle.fontSize = 10;
                smallTextStyle.normal.textColor = Color.white;
                GUI.Label(new Rect(65, ry, 155, 18), keybinds[r, 0], smallTextStyle);

                // Primary Key Pill
                GUI.DrawTexture(new Rect(228, ry, 70, 18), darkBgTex);
                badgeStyle.fontSize = 9;
                badgeStyle.normal.textColor = Color.yellow;
                GUI.Label(new Rect(228, ry, 70, 18), keybinds[r, 1], badgeStyle);

                // Secondary Key Pill
                GUI.DrawTexture(new Rect(338, ry, 70, 18), darkBgTex);
                badgeStyle.normal.textColor = new Color(0.7f, 0.8f, 0.9f);
                GUI.Label(new Rect(338, ry, 70, 18), keybinds[r, 2], badgeStyle);
            }

            // Bottom Buttons for Table: RESET TO DEFAULTS, CLEAR ALL BINDINGS
            if (GUI.Button(new Rect(50, contentY + 356f, 150, 28), "RESET TO DEFAULTS"))
            {
                LoadSettings();
            }
            if (GUI.Button(new Rect(210, contentY + 356f, 150, 28), "CLEAR ALL BINDINGS"))
            {
                WCTAudioSynthesizer.Instance?.PlayUIClick();
            }

            // 2. Center/Right Top: MOUSE SETTINGS (Image 2)
            float rightX = 460f;
            float rightW = sw - rightX - 50f;
            float mouseCardW = 260f;

            GUI.DrawTexture(new Rect(rightX, contentY, mouseCardW, 160f), cardBgTex);
            headerStyle.fontSize = 12;
            headerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(rightX + 14, contentY + 10, mouseCardW - 28, 18), "MOUSE SETTINGS", headerStyle);

            smallTextStyle.fontSize = 10;
            smallTextStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(rightX + 14, contentY + 32, 120, 16), "Mouse Sensitivity", smallTextStyle);
            mouseSensitivity = GUI.HorizontalSlider(new Rect(rightX + 130, contentY + 34, 110, 14), mouseSensitivity, 0.2f, 4.0f);

            GUI.Label(new Rect(rightX + 14, contentY + 60, 120, 16), "Aim Sensitivity (Camera)", smallTextStyle);
            GUI.HorizontalSlider(new Rect(rightX + 130, contentY + 62, 110, 14), 1.0f, 0.2f, 4.0f);

            GUI.Label(new Rect(rightX + 14, contentY + 88, 120, 16), "Invert Y-Axis", smallTextStyle);
            if (GUI.Button(new Rect(rightX + 130, contentY + 86, 110, 20), invertY ? "< On >" : "< Off >"))
            {
                invertY = !invertY;
            }

            GUI.Label(new Rect(rightX + 14, contentY + 116, 120, 16), "Mouse Smoothing", smallTextStyle);
            GUI.Button(new Rect(rightX + 130, contentY + 114, 110, 20), "< Off >");

            // 3. Top-Right: TEST CONTROLS Box (Image 2)
            float testX = rightX + mouseCardW + 15f;
            float testW = sw - testX - 50f;
            GUI.DrawTexture(new Rect(testX, contentY, testW, 160f), cardBgTex);
            GUI.Label(new Rect(testX + 14, contentY + 10, testW - 28, 18), "TEST CONTROLS", headerStyle);
            smallTextStyle.fontSize = 9;
            smallTextStyle.normal.textColor = new Color(0.7f, 0.8f, 0.9f);
            GUI.Label(new Rect(testX + 14, contentY + 28, testW - 28, 16), "Try your controls here", smallTextStyle);

            // Preview box
            GUI.DrawTexture(new Rect(testX + 14, contentY + 46, testW - 28, 75), darkBgTex);
            badgeStyle.fontSize = 11;
            badgeStyle.normal.textColor = Color.cyan;
            GUI.Label(new Rect(testX + 14, contentY + 70, testW - 28, 20), "3D ARENA TEST VIEWPORT", badgeStyle);

            smallTextStyle.fontSize = 9;
            smallTextStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(testX + 14, contentY + 126, testW - 28, 30), "Move around, jump and try different actions to test your key bindings.", smallTextStyle);

            // 4. Center/Right Bottom: CONTROL GUIDE Keyboard & Mouse Diagram (Image 2)
            float guideY = contentY + 172f;
            float guideW = rightW;
            GUI.DrawTexture(new Rect(rightX, guideY, guideW, 178f), cardBgTex);
            GUI.Label(new Rect(rightX + 14, guideY + 10, guideW - 28, 18), "CONTROL GUIDE", headerStyle);

            // Keyboard graphic pill cluster
            float kx = rightX + 20f;
            float ky = guideY + 36f;
            DrawKeyClusterPill(kx + 45, ky, "W", "Move Forward", Color.cyan);
            DrawKeyClusterPill(kx, ky + 26, "A", "Move Left", Color.cyan);
            DrawKeyClusterPill(kx + 45, ky + 26, "S", "Move Backward", Color.cyan);
            DrawKeyClusterPill(kx + 90, ky + 26, "D", "Move Right", Color.cyan);
            DrawKeyClusterPill(kx - 10, ky + 54, "Shift", "Sprint", Color.yellow);
            DrawKeyClusterPill(kx + 50, ky + 54, "Ctrl", "Slide", Color.red);
            DrawKeyClusterPill(kx + 110, ky + 54, "Space", "Jump / Vault", new Color(1f, 0.6f, 0.1f));
            DrawKeyClusterPill(kx + 190, ky + 26, "F", "Tag (Chaser)", Color.red);

            // Mouse schematic on right side
            float mx = rightX + guideW - 140f;
            GUI.DrawTexture(new Rect(mx, guideY + 34, 110, 120), darkBgTex);
            badgeStyle.fontSize = 10;
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(mx, guideY + 40, 110, 18), "Left Click: Tag", badgeStyle);
            GUI.Label(new Rect(mx, guideY + 62, 110, 18), "Right: Hold Aim", badgeStyle);
            GUI.Label(new Rect(mx, guideY + 84, 110, 18), "Mouse: Look", badgeStyle);

            // Bottom Action Bar: CANCEL, APPLY SETTINGS
            float bY = sh - 46f;
            if (GUI.Button(new Rect(sw - 310, bY, 110, 34), "CANCEL"))
            {
                currentScreen = MenuScreen.Settings;
            }
            if (GUI.Button(new Rect(sw - 180, bY, 130, 34), "APPLY SETTINGS", blueBtnStyle))
            {
                ApplySettings();
                WCTAudioSynthesizer.Instance?.PlayUIClick();
                currentScreen = MenuScreen.Settings;
            }
        }

        private void DrawKeyClusterPill(float x, float y, string key, string label, Color col)
        {
            GUI.color = col;
            GUI.DrawTexture(new Rect(x, y, 36, 22), Texture2D.whiteTexture);
            GUI.color = Color.white;

            badgeStyle.fontSize = 10;
            badgeStyle.normal.textColor = Color.black;
            GUI.Label(new Rect(x, y + 2, 36, 20), key, badgeStyle);
        }

        // ==============================================================
        // 9. RULES CODEX & HOW TO PLAY (Image 4)
        // ==============================================================
        private void DrawRulesCodex(float sw, float sh)
        {
            DrawHowToPlay(sw, sh);
        }

        private void DrawHowToPlay(float sw, float sh)
        {
            float topY = 70f;
            titleStyle.fontSize = 24;
            titleStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(50, topY, 400, 28), "HOW TO PLAY", titleStyle);

            smallTextStyle.fontSize = 11;
            smallTextStyle.normal.textColor = new Color(0.6f, 0.7f, 0.85f);
            GUI.Label(new Rect(50, topY + 28, 450, 18), "Learn the rules, controls and everything you need to start chasing", smallTextStyle);

            float panelY = topY + 54f;
            float leftW = 210f;

            // 1. Left Sidebar (9 items matching Image 4)
            GUI.DrawTexture(new Rect(50, panelY, leftW, 390f), cardBgTex);
            string[] navItems = {
                "▶  Game Overview",
                "📄  Rules & Scoring",
                "🎮  Controls",
                "🏃  Role Guide",
                "📦  Arena Elements",
                "🏆  Game Modes",
                "💡  Tips & Strategies",
                "🎓  Tutorial",
                "❓  FAQ"
            };

            for (int i = 0; i < navItems.Length; i++)
            {
                Rect r = new Rect(56, panelY + 10 + i * 41, leftW - 12, 36);
                bool isSel = (howToPlaySidebarIndex == i);

                if (isSel) GUI.DrawTexture(r, bluePillTex);
                badgeStyle.fontSize = 11;
                badgeStyle.normal.textColor = isSel ? Color.white : new Color(0.75f, 0.85f, 0.95f);
                GUI.Label(new Rect(r.x + 8, r.y + 8, r.width - 16, 20), navItems[i], badgeStyle);

                if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
                {
                    howToPlaySidebarIndex = i;
                    WCTAudioSynthesizer.Instance?.PlayUIClick();
                }
            }

            // 2. Center Panel: Video Player & 4 Step Progression (Image 4)
            float centerW = 560f;
            float centerX = 50 + leftW + 15f;
            GUI.DrawTexture(new Rect(centerX, panelY, centerW, 390f), cardBgTex);

            // Large Video Showcase Card
            float vidH = 200f;
            GUI.DrawTexture(new Rect(centerX + 12, panelY + 12, centerW - 24, vidH), darkBgTex);

            // Central Play Button
            float playSize = 54f;
            float pcx = centerX + centerW * 0.5f - playSize * 0.5f;
            float pcy = panelY + 12 + vidH * 0.5f - playSize * 0.5f;
            GUI.DrawTexture(new Rect(pcx, pcy, playSize, playSize), bluePillTex);
            badgeStyle.fontSize = 24;
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(pcx, pcy + 10, playSize, playSize), "▶", badgeStyle);

            // Video Duration & Subtitle
            smallTextStyle.fontSize = 10;
            smallTextStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(centerX + 20, panelY + vidH - 18, 350, 18), "Watch this quick video to learn the basics of World Chase Tag", smallTextStyle);
            GUI.Label(new Rect(centerX + centerW - 60, panelY + vidH - 18, 45, 18), "02:15", smallTextStyle);

            // 4 Step Progression Cards below Video (Image 4)
            float stepY = panelY + vidH + 20f;
            float stepW = (centerW - 36f) / 4f;

            DrawHowToPlayStepCard(centerX + 12, stepY, stepW, "1 CHASE STARTS", "Players start from designated boxes. Timer begins and Chaser pursues Evader.");
            DrawHowToPlayStepCard(centerX + 12 + stepW + 4, stepY, stepW, "2 PARKOUR MOVES", "Use parkour moves like running, jumping, vaulting, climbing and sliding.");
            DrawHowToPlayStepCard(centerX + 12 + (stepW + 4) * 2, stepY, stepW, "3 TAG OR ESCAPE", "Chaser wins by tagging Evader. Evader wins by surviving the full 20 seconds.");
            DrawHowToPlayStepCard(centerX + 12 + (stepW + 4) * 3, stepY, stepW, "4 ROTATE ROLES", "After each chase, teams rotate roles based on tournament rules.");

            // 3. Right Column: GAME OVERVIEW, BASIC RULES, SCORING, START TUTORIAL (Image 4)
            float rightX = centerX + centerW + 15f;
            float rightW = sw - rightX - 50f;
            GUI.DrawTexture(new Rect(rightX, panelY, rightW, 390f), cardBgTex);

            headerStyle.fontSize = 13;
            headerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(rightX + 14, panelY + 10, rightW - 28, 18), "GAME OVERVIEW", headerStyle);

            smallTextStyle.fontSize = 10;
            smallTextStyle.normal.textColor = new Color(0.85f, 0.9f, 1.0f);
            GUI.Label(new Rect(rightX + 14, panelY + 30, rightW - 28, 65),
                "World Chase Tag is a fast-paced parkour game where one player (Chaser) pursues another player (Evader) in a closed arena using real parkour movement.", smallTextStyle);

            // BASIC RULES
            GUI.Label(new Rect(rightX + 14, panelY + 100, rightW - 28, 18), "BASIC RULES", headerStyle);
            float brY = panelY + 122f;
            DrawRuleBullet(rightX + 14, brY, rightW - 28, "⏱ Each chase lasts 20 seconds");
            DrawRuleBullet(rightX + 14, brY + 20, rightW - 28, "✋ Chaser must tag Evader with hand");
            DrawRuleBullet(rightX + 14, brY + 40, rightW - 28, "🔲 Evader must stay inside arena");
            DrawRuleBullet(rightX + 14, brY + 60, rightW - 28, "🔄 Teams rotate roles each chase");
            DrawRuleBullet(rightX + 14, brY + 80, rightW - 28, "🏆 Most points wins the match");

            // SCORING
            GUI.Label(new Rect(rightX + 14, panelY + 230, rightW - 28, 18), "SCORING", headerStyle);
            DrawRuleBullet(rightX + 14, panelY + 252, rightW - 28, "🏃 Evader Escape = 1 Point");
            DrawRuleBullet(rightX + 14, panelY + 272, rightW - 28, "🎯 Chaser Tag = 1 Point");

            // START TUTORIAL BUTTON
            if (GUI.Button(new Rect(rightX + 14, panelY + 335, rightW - 28, 42), "▶  START TUTORIAL", blueBtnStyle))
            {
                selectedMode = GameMode.Practice;
                LaunchMatch();
            }
        }

        private void DrawHowToPlayStepCard(float x, float y, float w, string title, string desc)
        {
            GUI.DrawTexture(new Rect(x, y, w, 145f), darkBgTex);
            GUI.DrawTexture(new Rect(x, y, w, 2), alphaCyanTex);

            badgeStyle.fontSize = 10;
            badgeStyle.normal.textColor = Color.cyan;
            GUI.Label(new Rect(x + 6, y + 6, w - 12, 18), title, badgeStyle);

            smallTextStyle.fontSize = 9;
            smallTextStyle.normal.textColor = new Color(0.8f, 0.85f, 0.95f);
            GUI.Label(new Rect(x + 6, y + 26, w - 12, 115), desc, smallTextStyle);
        }

        private void DrawRuleBullet(float x, float y, float w, string text)
        {
            smallTextStyle.fontSize = 10;
            smallTextStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(x, y, w, 18), text, smallTextStyle);
        }

        // ==============================================================
        // FINAL MATCH RESULTS SCREEN (Image 1)
        // ==============================================================
        private void DrawMatchResults(float sw, float sh)
        {
            // Dimmed arena background
            GUI.DrawTexture(new Rect(0, 0, sw, sh), darkBgTex);

            float cx = sw * 0.5f;
            float topY = 70f;

            // Small category label
            smallTextStyle.fontSize = 12;
            smallTextStyle.normal.textColor = new Color(0.2f, 0.75f, 1.0f);
            GUI.Label(new Rect(cx - 150, topY, 300, 18), "FINAL MATCH RESULTS", smallTextStyle);

            // Winner Banner with Laurel Wreaths
            bool alphaWon = WCTMatchController.Instance == null || WCTMatchController.Instance.TeamAScore >= WCTMatchController.Instance.TeamBScore;
            string winnerTitle = alphaWon ? "🌿  ALPHA WINS!  🌿" : "🌿  OMEGA WINS!  🌿";

            titleStyle.fontSize = 34;
            titleStyle.normal.textColor = alphaWon ? new Color(0.2f, 0.85f, 1.0f) : new Color(1.0f, 0.55f, 0.15f);
            GUI.Label(new Rect(cx - 250, topY + 22, 500, 42), winnerTitle, titleStyle);

            smallTextStyle.fontSize = 12;
            smallTextStyle.normal.textColor = new Color(0.85f, 0.9f, 1.0f);
            GUI.Label(new Rect(cx - 200, topY + 66, 400, 20), "Dominant performance. Well played!", smallTextStyle);

            // Left Score Card: ALPHA (Cyan)
            float scoreCardW = 200f;
            float scoreCardH = 110f;
            float leftCardX = cx - 340f;
            float cardY = topY + 95f;

            GUI.DrawTexture(new Rect(leftCardX, cardY, scoreCardW, scoreCardH), cardBgTex);
            if (alphaWon) GUI.DrawTexture(new Rect(leftCardX, cardY, scoreCardW, 3), alphaCyanTex);

            headerStyle.fontSize = 18;
            headerStyle.normal.textColor = new Color(0.2f, 0.85f, 1.0f);
            GUI.Label(new Rect(leftCardX + 16, cardY + 12, 100, 22), "A  ALPHA", headerStyle);

            titleStyle.fontSize = 38;
            titleStyle.normal.textColor = Color.white;
            int scoreA = WCTMatchController.Instance != null ? WCTMatchController.Instance.TeamAScore : 7;
            GUI.Label(new Rect(leftCardX + 16, cardY + 36, 80, 42), $"{scoreA}", titleStyle);

            if (alphaWon)
            {
                badgeStyle.fontSize = 11;
                badgeStyle.normal.textColor = Color.yellow;
                GUI.Label(new Rect(leftCardX + 16, cardY + 80, scoreCardW - 32, 18), "🌿 WINNER 🌿", badgeStyle);
            }

            // Right Score Card: OMEGA (Orange)
            float rightCardX = cx + 140f;
            GUI.DrawTexture(new Rect(rightCardX, cardY, scoreCardW, scoreCardH), cardBgTex);
            if (!alphaWon) GUI.DrawTexture(new Rect(rightCardX, cardY, scoreCardW, 3), omegaOrangeTex);

            headerStyle.fontSize = 18;
            headerStyle.normal.textColor = new Color(1.0f, 0.55f, 0.15f);
            GUI.Label(new Rect(rightCardX + 16, cardY + 12, 120, 22), "OMEGA  Ω", headerStyle);

            titleStyle.fontSize = 38;
            titleStyle.normal.textColor = Color.white;
            int scoreB = WCTMatchController.Instance != null ? WCTMatchController.Instance.TeamBScore : 5;
            GUI.Label(new Rect(rightCardX + 16, cardY + 36, 80, 42), $"{scoreB}", titleStyle);

            if (!alphaWon)
            {
                badgeStyle.fontSize = 11;
                badgeStyle.normal.textColor = Color.yellow;
                GUI.Label(new Rect(rightCardX + 16, cardY + 80, scoreCardW - 32, 18), "🌿 WINNER 🌿", badgeStyle);
            }

            // Center Celebrating Athlete Avatar
            float avatarSize = 100f;
            GUI.DrawTexture(new Rect(cx - avatarSize * 0.5f, cardY + 5, avatarSize, avatarSize), darkBgTex);
            badgeStyle.fontSize = 36;
            badgeStyle.normal.textColor = Color.cyan;
            GUI.Label(new Rect(cx - avatarSize * 0.5f, cardY + 22, avatarSize, avatarSize), "🏃‍♂️", badgeStyle);

            // Left Card: MATCH SUMMARY (Image 1)
            float sumW = 320f;
            float sumH = 175f;
            float sumX = cx - 340f;
            float sumY = cardY + scoreCardH + 18f;

            GUI.DrawTexture(new Rect(sumX, sumY, sumW, sumH), cardBgTex);
            headerStyle.fontSize = 14;
            headerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(sumX + 16, sumY + 12, sumW - 32, 20), "MATCH SUMMARY", headerStyle);

            float rY = sumY + 38f;
            DrawResultDetailRow(sumX + 16, rY, sumW - 32, "Total Chases", "8");
            DrawResultDetailRow(sumX + 16, rY + 28, sumW - 32, "Successful Tags", "5");
            DrawResultDetailRow(sumX + 16, rY + 56, sumW - 32, "Total Escapes", "3");
            DrawResultDetailRow(sumX + 16, rY + 84, sumW - 32, "Average Chase Time", "14.2s");

            // Right Card: CHASE HISTORY (Image 1)
            float histX = cx + 20f;
            GUI.DrawTexture(new Rect(histX, sumY, sumW, sumH), cardBgTex);
            GUI.Label(new Rect(histX + 16, sumY + 12, sumW - 32, 20), "CHASE HISTORY", headerStyle);

            float hrY = sumY + 36f;
            DrawChaseHistoryRow(histX + 16, hrY, sumW - 32, 1, "ALPHA", true, "20.0s");
            DrawChaseHistoryRow(histX + 16, hrY + 24, sumW - 32, 2, "OMEGA", false, "8.3s");
            DrawChaseHistoryRow(histX + 16, hrY + 48, sumW - 32, 3, "ALPHA", true, "20.0s");
            DrawChaseHistoryRow(histX + 16, hrY + 72, sumW - 32, 4, "OMEGA", false, "12.7s");
            DrawChaseHistoryRow(histX + 16, hrY + 96, sumW - 32, 5, "ALPHA", true, "20.0s");

            // Bottom Buttons (Image 1)
            float btnW = 210f;
            float btnH = 46f;
            float btnY = sumY + sumH + 20f;
            float btnStartX = (sw - (btnW * 3 + 30f)) * 0.5f;

            if (GUI.Button(new Rect(btnStartX, btnY, btnW, btnH), "PLAY AGAIN\nRematch this game", blueBtnStyle))
            {
                if (WCTMatchController.Instance != null) WCTMatchController.Instance.StartMatch(selectedMode, selectedDifficulty);
                SetMenuOpen(false);
            }

            if (GUI.Button(new Rect(btnStartX + btnW + 15, btnY, btnW, btnH), "VIEW DETAILS\nSee full match stats"))
            {
                currentScreen = MenuScreen.PlayerProfile;
                WCTAudioSynthesizer.Instance?.PlayUIClick();
            }

            if (GUI.Button(new Rect(btnStartX + (btnW + 15) * 2, btnY, btnW, btnH), "BACK TO MENU\nReturn to main menu"))
            {
                currentScreen = MenuScreen.MainMenu;
                WCTAudioSynthesizer.Instance?.PlayUIClick();
            }
        }

        private void DrawResultDetailRow(float x, float y, float w, string label, string val)
        {
            smallTextStyle.fontSize = 11;
            smallTextStyle.normal.textColor = new Color(0.65f, 0.75f, 0.88f);
            GUI.Label(new Rect(x, y, 160, 20), label, smallTextStyle);

            smallTextStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(x + w - 70, y, 70, 20), val, smallTextStyle);
        }

        private void DrawChaseHistoryRow(float x, float y, float w, int chaseNum, string team, bool escaped, string duration)
        {
            badgeStyle.fontSize = 10;
            badgeStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(x, y, 20, 18), $"{chaseNum}", badgeStyle);

            badgeStyle.normal.textColor = (team == "ALPHA") ? Color.cyan : Color.orange;
            GUI.Label(new Rect(x + 24, y, 65, 18), team, badgeStyle);

            GUI.DrawTexture(new Rect(x + 95, y, 56, 16), escaped ? alphaCyanTex : darkBgTex);
            badgeStyle.normal.textColor = escaped ? Color.black : Color.red;
            GUI.Label(new Rect(x + 95, y, 56, 16), escaped ? "Escaped" : "Tagged", badgeStyle);

            smallTextStyle.fontSize = 10;
            smallTextStyle.normal.textColor = new Color(0.7f, 0.8f, 0.9f);
            GUI.Label(new Rect(x + w - 48, y, 48, 16), $"({duration})", smallTextStyle);
        }

        // ==============================================================
        // 10. PLAYER PROFILE (Image 1, Screen 15)
        // ==============================================================
        private void DrawPlayerProfile(float sw, float sh)
        {
            float topY = 70f;
            headerStyle.fontSize = 22;
            headerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(50, topY, 400, 26), "PLAYER PROFILE", headerStyle);

            float panelY = topY + 45f;
            float panelW = sw - 100f;
            float panelH = sh - panelY - 45f;

            GUI.DrawTexture(new Rect(50, panelY, panelW, panelH), cardBgTex);

            // Left Profile Pill & Avatar
            float leftW = 280f;
            GUI.DrawTexture(new Rect(70, panelY + 20, leftW, 140), darkBgTex);

            badgeStyle.fontSize = 18;
            badgeStyle.normal.textColor = Color.cyan;
            GUI.Label(new Rect(80, panelY + 30, leftW - 20, 26), playerName, badgeStyle);

            smallTextStyle.fontSize = 11;
            smallTextStyle.normal.textColor = new Color(0.7f, 0.8f, 0.9f);
            GUI.Label(new Rect(80, panelY + 60, leftW - 20, 20), $"Rank: {playerRank}  •  Level {playerLevel}", smallTextStyle);
            GUI.Label(new Rect(80, panelY + 85, leftW - 20, 20), $"XP: {currentXP} / {maxXP} XP", smallTextStyle);

            // Stat Grid
            float gridX = 70 + leftW + 30f;
            float gridW = panelW - leftW - 80f;
            float gw = (gridW - 30f) * 0.33f;
            float gh = 85f;

            DrawStatBox(new Rect(gridX, panelY + 20, gw, gh), "MATCHES PLAYED", "12", "Career matches");
            DrawStatBox(new Rect(gridX + gw + 15, panelY + 20, gw, gh), "WINS", "8", "Win rate: 67%");
            DrawStatBox(new Rect(gridX + (gw + 15) * 2, panelY + 20, gw, gh), "TOTAL EVASIONS", "126", "Points scored");

            DrawStatBox(new Rect(gridX, panelY + 20 + gh + 15, gw, gh), "TAGS MADE", "43", "Valid hand tags");
            DrawStatBox(new Rect(gridX + gw + 15, panelY + 20 + gh + 15, gw, gh), "BEST STREAK", "7", "Consecutive evasions");
            DrawStatBox(new Rect(gridX + (gw + 15) * 2, panelY + 20 + gh + 15, gw, gh), "TOTAL PLAY TIME", "3.6 hrs", "Competitive chases");
        }

        private void DrawStatBox(Rect rect, string label, string val, string sub)
        {
            GUI.DrawTexture(rect, darkBgTex);
            smallTextStyle.fontSize = 10;
            smallTextStyle.normal.textColor = new Color(0.6f, 0.7f, 0.85f);
            GUI.Label(new Rect(rect.x + 12, rect.y + 10, rect.width - 24, 18), label, smallTextStyle);

            headerStyle.fontSize = 20;
            headerStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(rect.x + 12, rect.y + 28, rect.width - 24, 28), val, headerStyle);

            smallTextStyle.normal.textColor = new Color(0.2f, 0.85f, 0.5f);
            GUI.Label(new Rect(rect.x + 12, rect.y + 58, rect.width - 24, 18), sub, smallTextStyle);
        }

        // ==============================================================
        // ACTIONS & HELPERS
        // ==============================================================
        public void LaunchMatch()
        {
            SetMenuOpen(false);

            if (WCTMatchController.Instance != null)
            {
                WCTMatchController.Instance.StartMatch(selectedMode, selectedDifficulty);
            }

            WCTAudioSynthesizer.Instance?.PlayBuzzer();
        }

        private void ApplySettings()
        {
            var player = FindAnyObjectByType<WCTParkourController>();
            if (player != null)
            {
                player.SetSensitivity(mouseSensitivity);
                player.SetBaseFov(fov);
                player.SetHeadBob(headBob);
            }
        }

        private void LoadSettings()
        {
            mouseSensitivity = 2.2f;
            invertY = false;
            headBob = true;
            fov = 75f;
            qualityIndex = QualitySettings.GetQualityLevel();
            isFullscreen = Screen.fullScreen;
            vSyncEnabled = QualitySettings.vSyncCount > 0;
            masterVolume = AudioListener.volume;
            sfxVolume = 0.9f;
        }

        private string GetArenaName(int idx)
        {
            switch (idx)
            {
                case 1: return "URBAN ROOFTOP";
                case 2: return "WAREHOUSE";
                case 3: return "NIGHT ARENA";
                case 4: return "CUSTOM ARENA";
                default: return "QUAD (CLASSIC)";
            }
        }
    }
}

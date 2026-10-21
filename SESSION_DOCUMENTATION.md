# World Chase Tag — Windows PC High-End Game — Session Documentation

This document records the architecture, implemented systems, graphics overhaul, parkour mechanics, tournament rules engine, test verification, and build instructions for the **World Chase Tag (WCT) PC High-End Game** in Unity 6 (`6000.5.6f1`).

---

## 1. Project Overview & Specifications

- **Target Platform**: Windows Standalone 64-bit PC.
- **Engine**: Unity 6 (`6000.5.6f1`) with Universal Render Pipeline (URP) / Built-in PBR fallback and uGUI (`2.5.0`).
- **Core Concept**: First-person competitive World Chase Tag arena game adhering to official WCT regulations (including WCT8 tournament rules).
- **Primary Control Scheme**: First-Person Perspective (FPP) Keyboard & Mouse.
- **Tournament Loop**: 12m × 12m Quad court, 20.0-second countdown clock, winner-stays-on Evader rotation, Super-Six athlete rosters, and 16-chase championship series with Sudden Death tie-breaker format.

### File Locations
- **Active PC Project**: `C:\Users\SURYAWANSHI\.gemini\antigravity-ide\scratch\WorldChaseTag_PC`
- **Synchronized Starter Folder**: `C:\Users\SURYAWANSHI\Downloads\WorldChaseTag_Android_Unity_Starter`
- **Active Scene**: `Assets/WorldChaseTag/Scenes/MainArena.unity`
- **Output Executable**: `c:\Users\SURYAWANSHI\.gemini\antigravity-ide\scratch\WorldChaseTag_PC\Builds\WorldChaseTag.exe`

---

## 2. Implemented Architecture & Scripts

```text
Assets/WorldChaseTag/
├── Scenes/
│   └── MainArena.unity             # High-end Quad arena scene with lighting and rigs
└── Scripts/
    ├── Core/
    │   ├── WCTMatchEnums.cs         # 9-state FSM, roles, outcomes, difficulties, game modes
    │   ├── WCTAthlete.cs            # Super-Six athlete card data (OVR, Archetype, SPD/STA/AGI/VLT, Captain)
    │   ├── WCTMatchController.cs    # Single-authority match controller, timer, rotation & scoring
    │   └── WCTSceneCoordinator.cs   # Spawns actors, wires targets, manages chase transitions
    ├── Movement/
    │   └── WCTParkourController.cs  # FPP movement, pitch-corrected mouse look, vault, mantle, slide, stamina, footsteps
    ├── Visuals/
    │   ├── WCTFPPArms.cs            # Procedurally animated 3D parkour arms & gloves in player camera view
    │   ├── WCTMaterialPalette.cs    # PBR court, padded vinyl textures, steel pipes, team banners, box trusses
    │   └── WCTAthleteVisuals.cs     # 3D opponent athlete model (jersey, headband, shorts, sneakers, overhead billboard)
    ├── Arena/
    │   └── QuadArenaBuilder.cs      # 12m x 12m Quad: central mountain, corner blocks, vault rails, trusses, spotlights
    ├── AI/
    │   ├── WCTAgentMotor.cs         # CharacterController motor with physical leaps and ledge vaulting
    │   ├── WCTChaserAI.cs           # Pursuit pathing, predictive interception, corner cuts, tag strike
    │   └── WCTEvaderAI.cs           # Route scoring S(r), obstacle line-of-sight breaks, feints, boundary checks
    ├── Combat/
    │   ├── WCTTagSystem.cs          # Capsule cast sweep, LOS validation, anti-tunneling, visual effect
    │   └── WCTBoundaryTrigger.cs    # 12m x 12m perimeter violation detection
    ├── Audio/
    │   └── WCTAudioSynthesizer.cs   # Real-time procedural audio: buzzer, beeps, thumps, footsteps, vault whoosh
    ├── UI/
    │   ├── WCTHUDController.cs      # In-game broadcast bar, 20s digital timer, role badge, radar, stamina meter, pause
    │   └── WCTMenuController.cs     # Multi-screen PC UI: Splash, Main Menu, Mode select, Super-Six cards, Settings, Codex
    ├── Testing/
    │   └── WCTAutomatedTests.cs     # Runtime test suite covering all WCT tournament acceptance rules
    └── Editor/
        ├── WCTSceneBuilder.cs       # Scene generation, lighting bake, and Standalone Windows 64-bit build pipeline
        └── WCTTestRunner.cs         # Headless Unity batchmode test runner
```

---

## 3. High-End Visuals & Graphics Overhaul

### 3.1 PBR Materials & Textures (`WCTMaterialPalette.cs`)
Designed to match the high-end reference arena (Image 1):
- **Court Floor**: Dark reflective industrial rubber court floor (`#1F232B`) with crisp white boundary and lane markings.
- **Obstacle Padding**: Heavy-duty black vinyl with reinforced stitching seams and corner bracket guards.
- **Top Surfaces**: High-traction light-grey grip pads with diamond traction micro-textures.
- **Scaffolding Rails**: Industrial black steel pipes with bolted base flanges, cross-clamps, and metallic brushed chrome highlights.
- **Stadium Rigging**: Triangular overhead box trusses suspended 7m above court with 8 high-output spotlights angled downward.
- **Spectator Arena**: Tiered perimeter stands with ambient dark blue stadium illumination.
- **Team Banners**: ALPHA electric cyan (`#0EA5E9`) and OMEGA blaze orange (`#F97316`) stadium signage with WCT logos.

### 3.2 First-Person Parkour Arms (`WCTFPPArms.cs`)
- Attached directly to player's `CameraHolder`.
- Dual athletic arms and textured grip gloves dynamically positioned in the first-person perspective.
- Procedural animation states:
  - **Natural Running Bob**: Left/right arm swings in sync with player speed and head-bobbing.
  - **Vault Hand-Plant**: Two-handed downward plant on obstacle ledges during low vaults.
  - **High Mantle Pull-Up**: Vertical grip and heave when climbing onto elevated blocks.
  - **Sprint Slide Brace**: Ground-sliding arm brace when sliding under rails.
  - **Tag Strike**: Extended right arm strike when pressing `F` to attempt a tag.
  - **Team Visuals**: Wristbands and jersey trims automatically match the player's active role (Cyan for Alpha Evader, Orange for Omega Chaser).

### 3.3 Opponent Athlete Visibility & HUD Radar (`WCTAthleteVisuals.cs` & `WCTHUDController.cs`)
- Replaced plain capsules with a fully proportioned 3D athletic runner model (color-blocked jersey, headband, athletic shorts, sneakers).
- **Overhead 3D World Marker**: Billboards above opponent's head displaying role badge (`[CHASER]` / `[EVADER]`), athlete name, and real-time distance in meters.
- **Edge Radar & Off-Screen Bracket**: If opponent moves outside the camera's field of view, an orange/cyan warning indicator on the HUD edge points directly toward them.

---

## 4. First-Person Movement & Controls

### 4.1 Mouse-Look Correction
- **Issue**: Pitch was previously inverted when moving mouse up/down.
- **Fix**: Implemented intuitive PC standard pitch handling:
  - Moving mouse **UP** tilts camera **UP** toward the sky.
  - Moving mouse **DOWN** tilts camera **DOWN** toward the court floor.
  - Toggleable `Invert Y` setting provided in Settings menu.

### 4.2 Parkour Movement Mechanics (`WCTParkourController.cs`)
- **WASD Movement**: Walk ($5.2\text{ m/s}$), Sprint ($8.5\text{ m/s}$), Crouch ($2.8\text{ m/s}$).
- **Smooth Acceleration**: Responsive $24\text{ m/s}^2$ acceleration with air-control conservation.
- **Coyote Time & Jump Buffer**: $0.15\text{s}$ grace period for responsive jumping off edges.
- **Contextual Vaulting**: Sprinting toward waist-height hurdles ($0.4\text{m} - 1.1\text{m}$) and pressing Space executes a forward hurdle vault.
- **Ledge Mantling**: Jumping toward high walls ($1.1\text{m} - 2.1\text{m}$) initiates a two-phase ledge grab and pull-up.
- **Sprint Slide**: Pressing `Ctrl` or `C` while sprinting slides beneath overhead scaffolding pipes.
- **Tag Strike**: Pressing `F` performs a hand tag check within $1.5\text{m}$ and a $70^\circ$ forward cone.
- **Stamina System**: Depletes while sprinting; regenerates when walking or stationary; exhaustion penalty applies if fully depleted.

### 4.3 Procedural Audio Synthesis (`WCTAudioSynthesizer.cs`)
- **Footsteps**: Procedural athletic shoe squeaks and thuds triggered rhythmically based on ground movement speed.
- **Heavy Landing Impact**: Sub-bass thud played when transitioning from air to ground with downward velocity.
- **Vault Whoosh**: Wind-cutting swoosh during hurdle vaults.
- **Slide Scrape**: Friction slide sound during sprint slides.
- **Referee Buzzer**: Authoritative $850\text{ Hz} + 425\text{ Hz}$ horn on chase start and stop.
- **3-2-1 Countdown Beeps**: $440\text{ Hz}$ pre-chase tones and $880\text{ Hz}$ GO buzzer.
- **Tag Thump**: Impact punch on successful hand contact.

---

## 5. Tournament Rules Engine (`WCTMatchController.cs`)

Strictly adheres to official World Chase Tag regulations:
1. **Single-Authority Event Resolution**: Eliminates conflicting resolutions between timer expiration, tags, and boundary crossings.
2. **20.0-Second Chase Clock**: Millisecond-precision timer with HUD digital readout.
3. **Winner-Stays-On Evader Rotation**:
   - Evader survives 20.0s $\rightarrow$ Evader's team earns 1 point and stays as Evader. Losing team sends next Chaser.
   - Chaser tags Evader $\rightarrow$ No points awarded. Chaser's team takes over as Evader. Losing team sends next Chaser.
   - Evader crosses boundary $\rightarrow$ Treated as an immediate loss for Evader; Chaser's team takes over.
4. **16-Chase Tournament Match**: Full series tracking cumulative evasion points across 16 chases.
5. **Sudden Death Chase-Off**: In case of a tie after 16 chases, 4 sudden-death chases compare cumulative evasion survival time.

---

## 6. Complete PC User Interface (`WCTMenuController.cs` & `WCTHUDController.cs`)

### 6.1 Multi-Screen Menu System (`WCTMenuController.cs`)
1. **Splash Screen**: Cinematic World Chase Tag logo presentation with keypress prompt.
2. **Main Menu (Fast Run Navigation - Image 5 Reference)**:
   - Left-aligned sleek drawer: Play Match, Mode Selection, Super-Six Teams & Athletes, Arena Selection, Training Drills, Rules Codex, Settings, Career Profile, Exit Game.
   - Right showcase: 3D match hero display with team matchups, captain details, and active parameters.
3. **Mode Selection**:
   - Quick Match (4 chases).
   - Championship Match (16 chases official tournament).
   - Practice Run (Solo free-roam chase).
   - Online Netcode (Standby for Season 2).
4. **Super-Six Athlete Cards (Image 4 Reference)**:
   - Displays all 6 athletes per team (Alpha Cyan & Omega Orange).
   - Player cards display: Overall Rating (OVR 87–93), Captaincy Badge `[C]`, Role Archetype, and 4 attribute meters (Speed, Stamina, Agility, Vault).
5. **Arena Selection**: Championship Quad, Neon Cyber Quad, and Rooftop Parkour Arena.
6. **Training Drills**: Interactive guides for Speed Vaults, Ledge Mantles, Sprint Slides, Tag Strikes, and Line-of-Sight evasion.
7. **Rules Codex**: Comprehensive breakdown of official competition regulations.
8. **Settings & Controls**:
   - Graphics presets (Fast, Medium, High, Ultra), Fullscreen, VSync.
   - Mouse sensitivity slider, Field of View slider ($65^\circ - 105^\circ$), Invert Y toggle, Head Bob toggle.
   - Master and SFX audio sliders.
9. **Player Career Profile**: Career stats, match win rate, chases played, fastest tag time ($3.42\text{s}$), and streak records.

### 6.2 In-Game HUD (`WCTHUDController.cs`)
- Top broadcast bar with Team Alpha & Omega scores, active chase number, and Sudden Death indicators.
- Digital 20.0s countdown clock with millisecond precision and color state changes.
- Expanded role badge (`ROLE: CHASER (TAG)` / `ROLE: EVADER`).
- Bottom-left keycap hints (WASD, SHIFT, SPACE, CTRL, F).
- Bottom-center dynamic stamina bar.
- Pause menu with Resume, Restart Match, Main Menu, AI Difficulty, and Settings.
- Full post-match scoreboard with 12+ chase history records and Rematch button.

---

## 7. Automated Acceptance Test Verification

All 8 automated tests were executed in Unity 6 batchmode via `WCTTestRunner.RunTestsInBatchMode`:

| # | Test Name | Assertion | Result |
|---|---|---|---|
| 1 | `Test_EvaderSurvives20s` | Surviving 20.0s awards 1 point & Evader stays on Quad | **PASSED** |
| 2 | `Test_ValidTagEndsChase` | Tag ends chase immediately, 0 points, Chaser becomes Evader | **PASSED** |
| 3 | `Test_OutOfBoundsEndsChase` | Evader boundary exit counts as loss, 0 points, rotation occurs | **PASSED** |
| 4 | `Test_SingleAuthorityResolution` | Simultaneous tag and timeout resolves exactly once | **PASSED** |
| 5 | `Test_WinnerStaysOnRotation` | Losing team alternates Chaser, winning team stays Evader | **PASSED** |
| 6 | `Test_Full16ChaseMatch` | Completes all 16 chases and transitions to `MATCH_RESULT` | **PASSED** |
| 7 | `Test_SuddenDeathTiebreak` | Equal scores trigger 4-chase sudden death comparison | **PASSED** |
| 8 | `Test_MatchRestartCleanState` | Restart resets score, timer, and roster to clean starting state | **PASSED** |

**Summary**: **8 PASSED, 0 FAILED (100% Success Rate)**.

---

## 8. Build & Execution Instructions

### 8.1 Launching the Standalone Windows Game
The compiled standalone 64-bit Windows executable is ready at:
```text
C:\Users\SURYAWANSHI\.gemini\antigravity-ide\scratch\WorldChaseTag_PC\Builds\WorldChaseTag.exe
```
Simply double-click `WorldChaseTag.exe` to run the game in full resolution.

### 8.2 Rebuilding via Command Line
To re-bake the scene and rebuild the standalone player in Unity batchmode:
```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.5.6f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\SURYAWANSHI\.gemini\antigravity-ide\scratch\WorldChaseTag_PC" -executeMethod WorldChaseTag.Editor.WCTSceneBuilder.BuildWindowsPlayer -logFile build_final.log
```

### 8.3 Running Automated Tests via Command Line
```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.5.6f1\Editor\Unity.exe" -batchmode -quit -projectPath "C:\Users\SURYAWANSHI\.gemini\antigravity-ide\scratch\WorldChaseTag_PC" -executeMethod WorldChaseTag.Editor.WCTTestRunner.RunTestsInBatchMode -logFile test_exec.log
```

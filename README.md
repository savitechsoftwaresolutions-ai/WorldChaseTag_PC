# World Chase Tag — Windows PC Championship Game

[![Unity 6](https://img.shields.io/badge/Unity-6000.5.6f1-blue.svg)](https://unity.com/)
[![Target Platform](https://img.shields.io/badge/Platform-Windows%20PC%20x64-brightgreen.svg)]()
[![Graphics Pipeline](https://img.shields.io/badge/Graphics-Deferred%20PBR%20%7C%20HDR%20%7C%208x%20MSAA-orange.svg)]()
[![Tests](https://img.shields.io/badge/Acceptance%20Tests-8%2F8%20Passed-success.svg)]()

A high-end, tournament-accurate first-person competitive parkour chase game built in **Unity 6** recreating the official **World Chase Tag (WCT)** championship format.

---

## 🎮 Core Features

- **Official WCT Championship Engine**:
  - 12m × 12m Quad-style arena with 20.0-second countdown clock.
  - Winner-Stays-On Evader role progression across 16 tournament chases.
  - Team Alpha (Cyan) vs Team Omega (Orange) with full 6-athlete rosters, archetypes, and ratings.
  - Overtime / Sudden Death evasion tie-breaker support.
- **First-Person Parkour Traversal**:
  - Physics-driven momentum, sprint, coyote time, jump buffering, and air control.
  - Hurdle vaults with low rail clearance and 1.6m corner mantle platform climbs.
  - Low slides with camera dip and speed boosts under barriers.
  - Dual procedurally animated athletic arms with team glove visualizer.
- **Ultra High-End Realistic Graphics**:
  - Deferred shading with 16 dynamic per-pixel lights.
  - Realtime perspective box-projected reflection probes for floor and chrome rail specular sheen.
  - Procedural tangent normal maps (rubber floor micro-relief, vinyl cushion padding, knurled diamond grip, brushed scaffolding steel).
  - Volumetric stadium haze and atmospheric lighting.
  - 4-sided overhead suspended LED Jumbotron scoreboard rig.
- **Broadcast Tournament HUD & UI**:
  - Center Chase Timer bar (`ALPHA 3 | 00:14 (CHASE 5/8) | 1 OMEGA`).
  - Top-left Team Players card (Alive / Tagged status).
  - Left Current Objective tracker.
  - Top-right Arena Mini-Map with real-time player and opponent positions.
  - Right-side status alert pills (`You are being chased!`, `Stay in the arena`).
  - Bottom-left Analog Movement Dial with WASD keycap indicators.
  - Bottom-right Circular Action Discs (`Jump/Vault [Space]`, `Slide/Crouch [C/Ctrl]`, `Tag [F]`).
  - 16 interactive broadcast menu screens: Match Results, Controls & Keybindings, Teams Manager, and How to Play tutorial.
- **Integrated Synthesizer Audio**:
  - Real-time procedural audio synthesis for buzzer horns, heartbeat tension, vault impacts, footstep scuffs, and tag whooshes without external asset dependencies.
- **Autonomous AI Opponents**:
  - State machine AI with path avoidance, obstacle vaulting, choke point looping, and interception prediction.

---

## 🕹️ Controls

| Action | Primary | Alternate |
| :--- | :--- | :--- |
| **Move** | `W` / `A` / `S` / `D` | Arrow Keys |
| **Look / Aim** | `Mouse Movement` | Pitch-Corrected FPP |
| **Sprint** | `Left Shift` | — |
| **Jump / Vault / Mantle** | `Space` | Auto-vault on hurdle contact while sprinting |
| **Crouch / Low Slide** | `Left Ctrl` | `C` |
| **Tag Strike (Chaser)** | `F` | `Left Mouse Click` |
| **Scoreboard** | `Tab` | — |
| **Mini-Map Toggle** | `M` | — |
| **Pause & Settings** | `Escape` | `P` |

---

## 📁 Project Architecture

```text
Assets/WorldChaseTag/
├── Scenes/
│   └── MainArena.unity            # Championship Quad arena scene
└── Scripts/
    ├── AI/
    │   ├── WCTAgentMotor.cs       # Nav/Character motor with obstacle obstacle clearance
    │   ├── WCTChaserAI.cs         # Chaser pursuit & tag attempt logic
    │   └── WCTEvaderAI.cs         # Evader evasion, looping & perimeter avoidance
    ├── Arena/
    │   └── QuadArenaBuilder.cs    # Procedural Quad arena, rails, barriers, jumbotron, trusses
    ├── Audio/
    │   └── WCTAudioSynthesizer.cs # Dynamic synth sounds (buzzers, footsteps, heartbeats, tags)
    ├── Combat/
    │   ├── WCTBoundaryTrigger.cs  # Arena out-of-bounds enforcement
    │   └── WCTTagSystem.cs        # Accurate ray/sphere tag collision & strike cooldown
    ├── Core/
    │   ├── WCTAthlete.cs          # Super-Six roster cards & athlete attributes
    │   ├── WCTMatchController.cs  # 20.0s match clock, winner-stays-on tournament engine
    │   ├── WCTMatchEnums.cs       # Match state machine, roles, outcomes
    │   └── WCTSceneCoordinator.cs # Lifecycle manager, actor spawner, graphics initializers
    ├── Editor/
    │   ├── WCTSceneBuilder.cs     # Scene generation & standalone player build pipeline
    │   └── WCTTestRunner.cs       # Batchmode automated acceptance test runner
    ├── Movement/
    │   └── WCTParkourController.cs# Parkour physics, vaults, mantles, slides, stamina
    ├── Testing/
    │   └── WCTAutomatedTests.cs   # Automated runtime test suite (8/8 pass)
    ├── UI/
    │   ├── WCTHUDController.cs    # Tournament in-game broadcast HUD
    │   └── WCTMenuController.cs   # 16 reference screens (Results, Controls, Teams, Rules)
    └── Visuals/
        ├── WCTAthleteVisuals.cs   # 3D athlete jersey & overhead role marker
        ├── WCTFPPArms.cs          # First-person animated parkour arms & gloves
        └── WCTMaterialPalette.cs  # PBR materials, normal maps & procedural textures
```

---

## 🚀 Building & Running

### Prerequisites
- Windows 10/11 64-bit
- Unity 6 (`6000.5.6f1` or later)

### To Run in Unity Editor:
1. Open the project in Unity 6.
2. Load scene `Assets/WorldChaseTag/Scenes/MainArena.unity`.
3. Press **Play**.

### To Build Standalone Player:
From Unity menu:
- Select **World Chase Tag** -> **Build Windows Standalone Player**.
- Output will be generated at `Builds/WorldChaseTag.exe`.

### Automated Batchmode Testing:
```bash
"C:\Program Files\Unity\Hub\Editor\6000.5.6f1\Editor\Unity.exe" -batchmode -quit -projectPath . -executeMethod WorldChaseTag.Testing.WCTTestRunner.RunTestsInBatchMode -logFile test_exec.log
```

---

## 📜 License
MIT License. Developed for competitive parkour and World Chase Tag enthusiasts.

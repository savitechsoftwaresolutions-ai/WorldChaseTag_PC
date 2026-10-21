# Project Memory & Token-Optimized Context Bank

> **Agent Instruction:** Read this file at the start of each task to retrieve project decisions, past bug resolutions, and context without re-reading the entire codebase. This keeps token usage minimal and prevents regressions.

- **Last Updated:** [ISO_TIMESTAMP]
- **Estimated Token Footprint:** ~800 tokens

---

## 1. Active Project Context & Current Focus
- **Current Milestone:** [CURRENT_FOCUS]
- **Recently Modified Core Files:**
  - `[FILES]`

---

## 2. Past Bug Resolutions & Regressions Knowledge Base
*When any error matches a pattern below, apply the documented fix immediately without wasting tokens on redundant root-cause rediscovery.*

### Format:
<!--
#### [BUG-ID]: [Brief Issue Description]
- **Error Signature:** `[Exact error message or substring]`
- **Root Cause:** [What caused it]
- **Permanent Fix:** [Exact change or code snippet]
- **Files Affected:** `[file.ext:line]`
-->

#### [BUG-001]: Mouse Cursor Stuck / Camera Angle Frozen & Mobile Joystick on PC
- **Error Signature:** Cursor locked/unlocked incorrectly between menus and gameplay; camera pitch and yaw stuck because `HandleMouseLook` returned if not locked; mobile movement dial and touch buttons drawn on PC screen.
- **Root Cause:** 
  1. `WCTParkourController.Update` coupled `isUIActive` with `!ControlsEnabled`. During pre-chase countdown, `ControlsEnabled == false` forced the cursor to unlock and returned before `HandleMouseLook()`.
  2. `HandleMouseLook` had an early return if `Cursor.lockState != CursorLockMode.Locked`, meaning if the cursor was unlocked during pre-chase, camera look never resumed.
  3. Mobile circular movement dial (`DrawMovementDial`) and touch action buttons (`DrawActionButtons`) in `WCTHUDController` cluttered the PC screen.
- **Permanent Fix:** 
  1. Decoupled UI state (`isUIActive`) from movement input (`ControlsEnabled`). In gameplay, cursor is automatically locked and hidden; `HandleMouseLook()` runs unconditionally so players can always look 360 degrees. `!ControlsEnabled` only suppresses WASD character translation until "GO!".
  2. Removed `DrawMovementDial` and `DrawActionButtons`; replaced with clean esports PC controls legend (`[WASD] Move • [Shift] Sprint • [Space] Vault/Jump • [Ctrl] Slide • [F] Tag • [Esc] Pause`).
  3. Main menu allows launching with `> PLAY NOW` button or pressing Enter / Space.
- **Files Affected:** `Assets/WorldChaseTag/Scripts/Movement/WCTParkourController.cs`, `Assets/WorldChaseTag/Scripts/UI/WCTHUDController.cs`, `Assets/WorldChaseTag/Scripts/UI/WCTMenuController.cs`

#### [BUG-002]: No Chase Result Screen, Lack of Audio Feedback on Timer Finish / Game Over, Feeling Stuck
- **Error Signature:** Timer hits 0:00 (evasion finish) or tag occurs; arena keeps running with no sound/buzzer; timer jumps back to 20.0f; overlay was translucent with overlapping HUD cards; clicking "NEXT CHASE >" restarted the whole match and reset scoreboards.
- **Root Cause:**
  1. `WCTAudioSynthesizer` lacked 2D audio configuration (`spatialBlend = 0f`), volume was muted/low, and `PlayBuzzer()` was not invoked when the 20-second timer expired in `WCTMatchController.ResolveChase()`.
  2. `WCTHUDController`'s timer display checked `IsChaseActive ? TimeRemaining : 20.0f`. When the round finished, `IsChaseActive` turned false, snapping the broadcast timer display immediately back to 20.0s instead of displaying 00:00.
  3. `DrawChaseResultOverlay()` had no backdrop dimming, causing text to illegibly overlap the tactical minimap and player cards.
  4. The result button previously called `matchController.StartMatch()` instead of `matchController.ProceedToNextChase()`, wiping out match history and team scores.
  5. Pre-chase countdown beeps (3, 2, 1) and giant center-screen countdown numerals were not rendered.
- **Permanent Fix:**
  1. Set `audioSource.spatialBlend = 0f` and `volume = 1.0f` on `WCTAudioSynthesizer`. Trigger `PlayBuzzer()` immediately upon 20s timeout or tag in `ResolveChase()`, alongside outcome fanfare (`PlayEvasionWin()`, `PlayTagSound()`).
  2. Added countdown audio beeps on 3, 2, 1 and referee buzzer on "GO!".
  3. Added `DrawCenterAnnouncementBanner()` in `WCTHUDController` to render large visual cues ("3", "2", "1", "GO!").
  4. Added a 90% opacity fullscreen backdrop scrim (`GUI.color = new Color(0.04f, 0.06f, 0.09f, 0.90f)`) to `DrawChaseResultOverlay()`, hiding behind-the-scenes minimap/cards so the summary is clear and prominent.
  5. Fixed timer logic to display exact outcome time (`00:00` for evasion win, exact tag second for tag).
  6. Added `ProceedToNextChase()` in `WCTMatchController` which preserves team scores and seamlessly rotates the chaser/evader roles, bound to `[SPACE]`, `[ENTER]`, or clicking the button.
#### [BUG-003]: Recreate Reference Screenshot Visual Quality (Realistic Hands, Omega Runner, Quad Arena & HUD)
- **Error Signature:** Player hands rendered as procedural blocks; opponent lacked Omega Ω jersey and athletic running gait; obstacles lacked the signature electric-blue border trims and angled vault ramp; HUD layout lacked the tournament logo, team card styling, and circular esports badges.
- **Root Cause:**
  1. `WCTFPPArms.cs` used primitive cubes for limbs without realistic phalanges, knuckles, wrist taper, or the black digital sports watch seen on the left wrist in the reference screenshot.
  2. `WCTAthleteVisuals.cs` lacked human anatomical proportions, running cycle animation, and the team Omega burgundy jersey with the white **Ω** emblem and "WCT" brand.
  3. `QuadArenaBuilder.cs` lacked the electric blue edge trim along obstacle tops/corners, the angled metal vault ramp on the right, and the foreground starting ledge with tubular scaffolding side rails.
  4. `WCTHUDController.cs` lacked the official wireframe globe logo, "ALPHA (3/4)" frosted glass card with crown/avatars, bottom-left vertical WASD keycaps, and bottom-right circular esports action badges ([Space] Jump/Vault, [C/Ctrl] Slide/Crouch, [F] Tag).
- **Permanent Fix:**
  1. Rebuilt `WCTFPPArms.cs` with contoured forearms, digital sports watch with cyan telemetry, anatomical palm with thenar/hypothenar eminence, 5 articulated fingers with knuckles and natural resting curvature, and fingernails using PBR realistic skin materials (`WCTMaterialPalette.GetRealisticSkinMaterial()`).
  2. Enhanced `WCTAthleteVisuals.cs` with an athletic humanoid runner model wearing the burgundy Omega Ω jersey, black compression pants, white ankle socks, running sneakers, and procedural velocity-driven running animation (torso forward lean, alternating arm swings, knee flexion).
  3. Updated `QuadArenaBuilder.cs` with electric blue border trim on all vinyl obstacles, an angled metal vault ramp on the right with side guides, and the foreground starting box with tubular side rails positioned cleanly without collision clipping.
  4. Updated `WCTHUDController.cs` with pixel-perfect recreation of the reference screenshot HUD: WCT globe logo, "ALPHA (3/4)" card with crown and athlete rows, top broadcast trapezoid score bar, top-right arena title and blueprint minimap with `[+]` zoom button, bottom-left vertical WASD key pills, bottom-center stamina bar, and bottom-right circular esports action badges.
- **Files Affected:** `Assets/WorldChaseTag/Scripts/Visuals/WCTFPPArms.cs`, `Assets/WorldChaseTag/Scripts/Visuals/WCTAthleteVisuals.cs`, `Assets/WorldChaseTag/Scripts/Visuals/WCTMaterialPalette.cs`, `Assets/WorldChaseTag/Scripts/Arena/QuadArenaBuilder.cs`, `Assets/WorldChaseTag/Scripts/UI/WCTHUDController.cs`

#### [BUG-004]: Mobile Touch Controls, Gyroscope Aiming & Android Compilation Pipeline
- **Error Signature:** Missing virtual joystick, touch catch/tag button, and mobile gyroscope look; `error CS0103: The name 'NamedBuildTarget' does not exist in the current context` during batchmode Android build.
- **Root Cause:** 
  1. Input system was solely wired to keyboard and desktop mouse lock (`Cursor.lockState = CursorLockMode.Locked`).
  2. `WCTSceneBuilder.cs` referenced `NamedBuildTarget` without `using UnityEditor.Build;`.
- **Permanent Fix:**
  1. Added `using UnityEditor.Build;` and configured `PlayerSettings` for Android ARM64 IL2CPP.
  2. Implemented dynamic virtual joystick on bottom-left with outer sprint ring in `WCTHUDController.cs` forwarding to `WCTParkourController.SetVirtualJoystick()`.
  3. Implemented interactive touch action buttons on bottom-right with large highlighted Catch / Tag button triggering `TriggerCatchOrTag()`.
  4. Enabled hardware gyroscope tracking (`Input.gyro.rotationRateUnbiased`) for tilt look and swipe gesture aiming in `WCTParkourController.cs` with an on-screen `[📱 GYRO: ON/OFF]` toggle button.
- **Files Affected:** `Assets/WorldChaseTag/Scripts/Editor/WCTSceneBuilder.cs`, `Assets/WorldChaseTag/Scripts/UI/WCTHUDController.cs`, `Assets/WorldChaseTag/Scripts/Movement/WCTParkourController.cs`

#### [BUG-005]: Unhandled UnityException: Tag: Chaser / Tag: Evader is not defined
- **Error Signature:** `UnityException: Tag: Chaser is not defined. UnityEngine.GameObject.set_tag(String) in WCTSceneCoordinator.SetupChaseActors`. Game frozen on dark screen during match start.
- **Root Cause:** Attempted to assign `gameObject.tag = "Chaser"` and `"Evader"` at runtime, but these custom tags were not registered in `TagManager.asset`. Role tracking was already cleanly managed by `CurrentRole` (`ChaseRole`).
- **Permanent Fix:** Removed invalid runtime `.tag` assignments in `WCTSceneCoordinator.cs`.
- **Files Affected:** `Assets/WorldChaseTag/Scripts/Core/WCTSceneCoordinator.cs`

---

## 3. Architecture Decisions & Inviolable Conventions
- Follow `AGENTS.md` rules strictly.
- Use one single source of truth for schemas and contracts.
- Never hardcode credentials; always use `.env.example`.
- Keep changes minimal, tested, and self-contained.

---

## 4. Completed Features Registry
- [x] Initial Project Setup & Toolchain Alignment
- [x] Documentation & Agent Rules Suite

---

## 5. Token Efficiency Rules for Agents
1. Inspect this `docs/MEMORY.md` before doing broad file searches.
2. Read only targeted line ranges of large files (`StartLine`/`EndLine`) instead of dumping thousands of lines.
3. Check section 2 above before diagnosing repeated test or build errors.

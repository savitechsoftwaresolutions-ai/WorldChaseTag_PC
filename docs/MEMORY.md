# Project Memory & Token-Optimized Context Bank

> **Agent Instruction:** Read this file at the start of each task to retrieve project decisions, past bug resolutions, and context without re-reading the entire codebase. This keeps token usage minimal and prevents regressions.

- **Last Updated:** 2026-10-22
- **Estimated Token Footprint:** ~850 tokens

---

## 1. Active Project Context & Current Focus
- **Current Milestone:** Windows PC Standalone Target & Snaps Sci-Fi Industrial Integration
- **Recently Modified Core Files:**
  - `Assets/WorldChaseTag/Scripts/UI/WCTHUDController.cs` (gated mobile touch controls behind Application.isMobilePlatform; PC esports HUD with WASD keycaps and hotkey action badges)
  - `Assets/WorldChaseTag/Scripts/Movement/WCTParkourController.cs` (enforced desktop locked cursor mode and fluid FPS mouse look on PC)
  - `Assets/WorldChaseTag/Scripts/Editor/WCTSceneBuilder.cs` (added ImportSnapsPackage, UpgradeSnapsMaterialsToURP, StandaloneWindows64 active build target)
  - `Assets/AssetStoreOriginals/_SNAPS_PrototypingAssets/` (imported 159 modular industrial prefabs & 161 mesh assets)

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

#### [BUG-006]: Movement Snagging, Obstacle Sticking, Unresponsive Catch/Tag & Multi-Touch Button Conflicts
- **Error Signature:** Player getting stuck against obstacles when running; unable to vault thin hurdles; catching/tagging failing or feeling unresponsive; on-screen Catch/Tag button ignored while moving the virtual joystick on mobile.
- **Root Cause:**
  1. **Spawn Collision & Step Offset**: The Chaser spawned at `z = -5.2f` behind an 0.82m tall solid platform at `z = -4.15f`. With default 0.3m `stepOffset`, the player immediately slammed into the wall at spawn. Additionally, colliding with walls killed velocity, turning `IsSprinting` false and preventing vault detection.
  2. **Thin Hurdle Detection**: Vault detection cast downward at `wallHit.point + forward * 0.4f`, which completely overshot thin hurdle rails (~0.1m wide), hitting the floor behind it and calculating obstacle height as 0.
  3. **Multi-Touch Button Block**: In IMGUI, `GUI.Button` only responded to primary touch (finger 0). When the player held the virtual joystick with their left thumb, secondary thumb taps on the Catch/Tag button were discarded by Unity IMGUI.
  4. **Tag Reach & Occlusion**: `WCTTagSystem` had a narrow 1.35m reach and checked `hit.transform == activeEvaderTransform`. Since athlete models use child body parts, hits on limbs/torso failed equality checks. Furthermore, line-of-sight raycasts occluded on the characters' own CharacterControllers.
- **Permanent Fix:**
  1. Configured `CharacterController` on player and AI with `stepOffset = 0.5f`, `slopeLimit = 60f`, `skinWidth = 0.03f`, and dynamic wall-tangent velocity projection (`Vector3.ProjectOnPlane`).
  2. Elevated the starting plate and spawn point in `QuadArenaBuilder.cs` to sit on the starting platform overlooking the arena, eliminating the spawn obstruction.
  3. Upgraded parkour ledge sampling to multi-distance offsets (`0.12f`, `0.28f`, `0.45f`) so thin hurdles and deep blocks vault smoothly.
  4. Implemented direct multi-touch button dispatch in `WCTHUDController.UpdateMobileTouchInput` using screen-coordinate hit rectangles (`catchRect`, `jumpRect`, `slideRect`, `gyroRect`), executing actions instantly on touch down even while joystick is held.
  5. Enhanced `WCTTagSystem` with generous 2.2m reach, 160° forward cone, hierarchy-aware evader resolution (`IsChildOf`), self-occlusion filtering, forward lunge boost on tag strike, and audio feedback.
- **Files Affected:** `Assets/WorldChaseTag/Scripts/Combat/WCTTagSystem.cs`, `Assets/WorldChaseTag/Scripts/Movement/WCTParkourController.cs`, `Assets/WorldChaseTag/Scripts/Arena/QuadArenaBuilder.cs`, `Assets/WorldChaseTag/Scripts/Editor/WCTSceneBuilder.cs`, `Assets/WorldChaseTag/Scripts/Core/WCTSceneCoordinator.cs`, `Assets/WorldChaseTag/Scripts/UI/WCTHUDController.cs`, `Assets/WorldChaseTag/Scripts/AI/WCTAgentMotor.cs`, `Assets/WorldChaseTag/Scripts/AI/WCTChaserAI.cs`


#### [BUG-004]: Wall Sticking, Void Falling, Catch Rebind to Key C, and 100% Championship Controls Layout
- **Error Signature:** Player stuck against wall colliders when running close or touching surfaces; player fell into void outside arena; Catch action needed on Key C instead of F; HUD controls needed 100% exact match to reference screenshot with customizable drawer.
- **Root Cause:**
  1. Starting sphere/ray casts flush against obstacles caused PhysX sweeps to miss normals, and horizontal velocity into walls wasn't deflected, creating friction sticking.
  2. Perimeter barrier height (1.8m) was lower than max mantle height (1.85m), causing the parkour system to vault/mantle the player outside the arena into empty space.
  3. Catch was previously on Key F; user explicitly demanded Key C.
  4. HUD needed 100% parity with reference screenshot: virtual joystick with 4 cardinal arrows, auto-run locking pill, sprint & backpack buttons, curved bottom stamina meter, right thumb action cluster (Catch on C, Jump, Slide, mini action buttons ▲/▼/🤾), TPP/FPP perspective toggle with runner model visible, bottom presets bar, and right-hand "CUSTOMIZE CONTROLS" drawer panel with 4 sliders, 5 toggles, and layout preset management.
- **Permanent Fix:**
  1. Added `OnControllerColliderHit` in `WCTParkourController` to immediately subtract wall-normal velocity (`horizontalVelocity -= wallNormal * normalSpeed`), ensuring frictionless sliding along obstacle faces.
  2. Raised perimeter barriers to 3.2m, expanded collider depth outward to 4.0m, tagged as `"Boundary"`, removed top handrail collider, excluded boundaries from parkour ledge detection, and added `EnforceArenaBoundaries()` safety position clamp.
  3. Bound Catch/Tag strictly to `KeyCode.C` and Left Mouse Click (removed KeyCode.F everywhere).
  4. Implemented full Championship Controls layout and "CUSTOMIZE CONTROLS" drawer panel with live scales, opacities, toggles, perspective switching (TPP/FPP on Key V), and playerprefs layout persistence.
#### [BUG-005]: FPP View Obstructed by Player 3D Head/Torso Mesh
- **Error Signature:** In First-Person Perspective (FPP), a large flesh/peach-colored flat surface covered 40% of the screen from the bottom, blocking the view; player arms were not cleanly isolated.
- **Root Cause:**
  1. `WCTAthleteVisuals` was attached to the player GameObject and generated a 3D model (head, neck, shoulders) with `WCTMaterialPalette.GetRealisticSkinMaterial()`. The neck and head cubes sat directly at camera eye height (`y = 1.6m`), clipping right into the camera near plane.
  2. `SetBodyVisible(false)` was called prior to `Start()`, but `visualsRoot` was null at that time and the visibility boolean was not saved or enforced in `Start()`.
  3. `SetPerspective` was not called in `WCTParkourController.Start()`, leaving the 3D body active.
  4. `fppArms.gameObject.SetActive(!isThirdPerson)` in `SetPerspective` deactivated the camera holder itself when toggling perspective because `WCTFPPArms` was attached directly to the camera object.
  5. The player GameObject was created using `GameObject.CreatePrimitive(PrimitiveType.Capsule)` without destroying the default `MeshRenderer`/`MeshFilter`.
- **Permanent Fix:**
  1. Updated `WCTAthleteVisuals` with persistent `isBodyVisible` tracking, auto-detecting `WCTParkourController` to default to hidden in FPP, and explicitly deactivating `visualsRoot` and `billboardTag` on the local player.
  2. Added `SetArmsVisible(bool visible)` to `WCTFPPArms` which toggles `armsRoot` directly without affecting the camera GameObject.
  3. Initialized `SetPerspective(isThirdPerson)` at startup in `WCTParkourController.Start()`.
  4. Created player GameObjects cleanly with `new GameObject("Player_Athlete")` rather than primitive capsules.
#### [BUG-006]: Mobile Touch Alignment & Android Mobile APK Generation
- **Summary:** Built the official Android mobile package (`WorldChaseTag.apk`) featuring the championship esports HUD, touch controls, and fixed FPP perspective.
- **Key Enhancements:**
  1. Synchronized `UpdateMobileTouchInput` with the dynamic button layout scales (`joystickScale`, `buttonScale`) for Catch (`82dp`), Jump (`64dp`), Slide (`64dp`), View TPP/FPP (`48dp`), Auto-Run, and Sprint.
  2. Fixed swipe-look detection so touch look swipes do not accidentally trigger action buttons, and touch look exclusion area updates with dynamic button sizes.
  3. Built via IL2CPP for modern 64-bit Android devices (`ARM64` and `X86_64`) targeting Android API Level 34.
- **Files Affected:** `Assets/WorldChaseTag/Scripts/UI/WCTHUDController.cs`, `Assets/WorldChaseTag/Scripts/Editor/WCTSceneBuilder.cs`

#### [BUG-007]: Mobile Parkour Movement Over-Speed, Instant AI Tagging, Third-Person Wall Clipping, Safe-Area Control Customizer & Role Gating
- **Error Signature:** Player moves too fast (superhuman speed); opponent catches player almost immediately at chase start; central obstacle blocks 80% of screen in FPP; camera clips through platforms/walls in TPP; touch controls small, fixed in raw pixels, clipped behind notches/safe-areas; Catch button active and triggers for Evader.
- **Root Cause:**
  1. Movement had excessive base velocity (8.5 m/s) with instant acceleration and zero analog joystick graduation.
  2. AI opponent spawned 2.8m from player with zero reaction delay, immediately sprinting in straight lines to tag upon round start.
  3. Chaser spawned on top of an 0.82m platform directly in front of a 3.4m wide central mountain, subtending most of the 75° camera view.
  4. Third-person camera had no physics collision handling and was repeatedly overridden by FPP head-bobbing logic.
  5. Touch controls used raw pixels without safe-area inset scaling or DP scaling (`dpScale`), and Catch button was clickable while playing Evader.
- **Permanent Fix:**
  1. Calibrated realistic human athletic speeds in `WCTParkourController.cs`: Walk = 2.4 m/s, Jog = 3.6 m/s, Sprint = 6.2 m/s, Slide = 6.8 m/s decaying over 0.65s, Jump = 0.85m, smooth acceleration & deceleration (14 m/s²), and analog joystick threshold scaling (`[0.05, 0.50]` Walk, `[0.50, 0.85]` Jog, `>0.85` or Sprint button for Sprint).
  2. Rebalanced `WCTAgentMotor.cs` athletic speeds per difficulty (Easy: 4.8 m/s, Normal: 5.6 m/s, Hard: 6.1 m/s) and added post-GO reaction latency in `WCTChaserAI.cs` (Easy: 0.65s, Normal: 0.35s, Hard: 0.15s) with `Physics.Linecast` line-of-sight checks.
  3. Resized Central Mountain to compact tournament dimensions `(2.2m, 0.65m, 2.0m)` in `QuadArenaBuilder.cs` and placed start positions on court floor at `z = -4.6m` (Chaser) and `z = 4.4m` (Evader), providing a clean 9.0m open court spacing with 3.5m running flanks.
  4. Implemented third-person collision follow camera in `WCTParkourController.cs` using a head-to-camera spherecast (0.20m radius) pulling forward to `camHit.distance - 0.22m`, and set camera `nearClipPlane = 0.02f` with FOV = 78°.
  5. Implemented responsive safe-area scaled mobile controls (`dpScale = Mathf.Clamp(Mathf.Min(sw/1280, sh/720), 1.0, 2.2)`), independent multi-touch finger tracking, 6 layout presets (Classic, Two Thumb, Left-Handed, Right-Handed, Large Buttons, Compact), and role-gated Catch button (dimmed/disabled with lock icon for Evader).
- **Files Affected:** `Assets/WorldChaseTag/Scripts/Movement/WCTParkourController.cs`, `Assets/WorldChaseTag/Scripts/AI/WCTAgentMotor.cs`, `Assets/WorldChaseTag/Scripts/AI/WCTChaserAI.cs`, `Assets/WorldChaseTag/Scripts/Combat/WCTTagSystem.cs`, `Assets/WorldChaseTag/Scripts/Arena/QuadArenaBuilder.cs`, `Assets/WorldChaseTag/Scripts/UI/WCTHUDController.cs`, `Assets/WorldChaseTag/Scripts/Core/WCTSceneCoordinator.cs`, `Assets/WorldChaseTag/Scripts/Editor/WCTSceneBuilder.cs`

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

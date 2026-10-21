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

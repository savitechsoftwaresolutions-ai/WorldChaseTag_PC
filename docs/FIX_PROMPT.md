# AI Debugging & Root Cause Analysis Prompt

## 1. Failure Overview
- **Failed Step:** Environment & Secret Configuration Check (env)
- **Failed Command:** `Environment & Secret Configuration Check`
- **Working Directory:** `C:\Users\SURYAWANSHI\.gemini\antigravity-ide\scratch\WorldChaseTag_PC`
- **Timestamp:** 2026-10-21T16:39:03.936Z

> **Enforce AGENTS.md Rules (Token Efficient):**
> - Do not guess or invent abstractions.
> - Identify the smallest correct fix.
> - Preserve existing working code and conventions.
> - Verify by re-running: `Environment & Secret Configuration Check`

## 2. Token-Optimized Error Trace
```text
.gitignore does not ignore .env files (risk of credential leakage)
```


## 4. Required Fix Protocol (Prompt 15 / Root Cause Analysis)
1. **Root Cause:** Review the traceback and any memory match above.
2. **Inspect Relevant Code:** Open ONLY the failing files above. Avoid reading unrelated files to save tokens.
3. **Minimal Fix:** Implement the smallest clean fix.
4. **Verify:** Re-run:
   ```bash
   npx vibekit verify
   ```
5. **Remember Fix:** After fixing, run `npx vibekit remember --title "..." --fix "..."` so future sessions never waste tokens on this again.

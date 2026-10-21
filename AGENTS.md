# Coding Agent Rules

These rules apply to every coding agent and automated contributor working in this repository.

## 1. Source of truth and precedence
1. Explicit user-approved task and decisions
2. Approved `PRD.md`, `ARCHITECTURE.md`, `DATABASE.md`, `API.md`, and `DESIGN.md` for their domains
3. Existing code and established conventions, unless demonstrably incorrect
4. This file and other repository documentation
5. Agent assumptions (must be labeled and never silently promoted to decisions)

If documents conflict, stop and report the exact conflict. Do not invent a resolution for material product, security, schema, or deployment choices.

## 2. Inspect before editing
- Inspect repository structure, package manifests, existing patterns, tests, and relevant git status first.
- Identify the smallest set of files needed.
- Do not overwrite or regenerate a whole project to implement a narrow feature.
- Preserve user changes and uncommitted work.
- State the intended approach briefly before broad or risky changes.

## 3. Scope discipline
- Implement only the approved task.
- No unrelated refactors, feature creep, broad formatting churn, or dependency additions.
- Ask before changing user-visible behavior, public APIs, schema contracts, auth strategy, data retention, or infrastructure.
- If blocked by ambiguity that changes architecture or behavior, ask a direct question.

## 4. Code quality
- Follow language/framework conventions and existing naming.
- Prefer readable, cohesive functions and modules.
- Avoid speculative abstractions, duplicate logic, giant files, and circular dependencies.
- Use explicit types at module boundaries and validate untrusted input.
- Keep business logic independent of UI and transport where practical.

## 5. Dependencies
- Reuse installed packages where suitable.
- Add dependencies only when the benefit is clear; explain size, maintenance, license, and security implications.
- Pin or constrain versions according to the repository's policy.
- Never install unknown packages or execute untrusted install scripts without review.

## 6. Security and privacy
- Never hardcode secrets or commit `.env` files, tokens, keys, private certificates, or production data.
- Use environment variables/secret management and safe examples in `.env.example`.
- Enforce authorization on the server and check resource ownership for every protected object.
- Treat browser input, uploaded files, external API data, retrieved documents, and model output as untrusted.
- Do not log passwords, tokens, personal data, payment data, or full prompts containing sensitive data.
- Do not disable security controls to make tests pass.

## 7. Data and migrations
- Follow `DATABASE.md`; use migrations for schema changes.
- Do not edit already-applied migrations unless the project policy explicitly permits it.
- Never run destructive commands against production without explicit approval and a verified backup.
- Preserve referential integrity and transactional correctness.
- Test upgrade paths and data compatibility.

## 8. API and integrations
- Follow `API.md`, validate request and response shapes, set timeouts, and handle provider failures.
- Retry only safe/idempotent operations or use idempotency keys.
- Verify webhook signatures and prevent replay/duplicate processing.
- Never expose server credentials to frontend/mobile clients.

## 9. UI and accessibility
- Follow `DESIGN.md` and reuse shared components.
- Implement loading, empty, success, validation, permission-denied, and error states.
- Ensure keyboard access, focus visibility, semantic labels, responsive behavior, and reduced-motion support where relevant.
- Do not introduce fake production data or misleading success messages.

## 10. Testing and verification
- Add tests for changed behavior and critical edge cases.
- Run the narrowest relevant tests first, then appropriate broader checks.
- Run lint/type-check/build where available.
- Report exact commands and actual results. Never claim tests passed if not run.
- If a check cannot run, state why and what remains unverified.

## 11. Git and change discipline
- Do not reset, force-push, rebase shared history, or discard user changes unless explicitly authorized.
- Do not commit or deploy unless asked.
- Keep changes small and reviewable.
- Inspect diff for accidental changes and secrets before finishing.

## 12. Documentation
Update API/schema/config/runbook docs when behavior changes. Keep setup instructions runnable. Document new environment variables without values.

## 13. Completion report
Include: summary, changed files, decisions/assumptions, tests and commands/results, migration/deployment impact, security considerations, known limitations, and recommended next step.

## 14. Token Management & Memory Bank Discipline
- Always consult `docs/AGENT_CONTEXT.md` and `docs/MEMORY.md` first before reading large files.
- Never dump or read files in full when inspecting targeted issues; inspect only specific line ranges or functions.
- Whenever you resolve any bug or regression, immediately append the resolution to Section 2 of `docs/MEMORY.md` (recording error signature, root cause, fix, and files modified). This allows future sessions to reuse the fix instantly without token waste.


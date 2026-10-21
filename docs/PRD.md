# Product Requirements Document (PRD)

> Status: Draft until reviewed and approved.  
> Product source of truth: approved requirements in this document.  
> Last reviewed: YYYY-MM-DD

## 1. Document control
- Product name:
- Owner / decision-maker:
- Stakeholders:
- Status: Draft / In Review / Approved / Superseded
- Target release:
- Related documents: `DESIGN.md`, `ARCHITECTURE.md`, `DATABASE.md`, `API.md`, `AGENTS.md`

## 2. Executive summary
Describe the product, the user problem, the proposed outcome, and why this is valuable in 3–6 sentences.

## 3. Problem and opportunity
- Current user problem:
- Existing workarounds:
- Costs or risks of the problem:
- Evidence supporting the problem:
- Why now:

## 4. Goals and non-goals
### Goals
List measurable outcomes, not implementation details.

### Non-goals
Explicitly list what this release will not do.

## 5. Target users and stakeholders
For each persona define:
- Context and needs
- Primary tasks
- Technical familiarity / accessibility needs
- Risks and constraints
- Permissions and data visibility

## 6. Value proposition
Explain the user benefit and differentiation. Avoid unsupported claims such as “first ever” or “100% accurate.”

## 7. Scope
### MVP
Only capabilities required to validate the core value proposition.

### Post-MVP candidates
Prioritized ideas that are not required for the first release.

### Out of scope
Features explicitly excluded.

## 8. User journeys
For every critical journey document:
1. Trigger and preconditions
2. Main steps
3. System responses
4. Success outcome
5. Alternate paths and failure recovery
6. Audit or notification requirements

## 9. User stories and acceptance criteria
Use: “As a [persona], I want [capability], so that [outcome].”
Every story must have testable acceptance criteria using Given/When/Then or an equivalent checklist.

## 10. Functional requirements
Use stable IDs, e.g. `FR-001`.
For each requirement include:
- Description
- Priority: P0 / P1 / P2
- Actor
- Preconditions
- Main behavior
- Validation
- Authorization rule
- Failure behavior
- Acceptance criteria
- Dependencies

## 11. Non-functional requirements
Define measurable targets where appropriate:
- Performance and latency
- Availability and recovery objectives
- Scalability and expected concurrent users / workload
- Security and privacy
- Accessibility
- Compatibility and supported devices/browsers
- Observability
- Maintainability and operability
- Localization
- Data retention and deletion

Do not invent unrealistic targets. Mark unconfirmed targets as open questions.

## 12. Screens, pages, and navigation
For each screen specify purpose, entry points, key components, permissions, data dependencies, loading/empty/error states, and responsive behavior.

## 13. Roles and permissions
Create a role-permission matrix. Deny by default. Clarify whether permissions are global, organization-scoped, project-scoped, or resource-owned.

## 14. Data requirements
List business entities, ownership, sensitivity classification, retention, validation, source of truth, and deletion/export requirements. Detailed columns belong in `DATABASE.md`.

## 15. Integrations
For each integration record purpose, data exchanged, authentication method, timeout/retry expectations, rate limits, fallback behavior, owner, and sandbox/test plan.

## 16. Notifications and background work
Define event triggers, recipients, channel, delivery expectations, deduplication, retry policy, and user preferences.

## 17. State and failure handling
Document loading, empty, partial, offline, validation, permission-denied, not-found, conflict, rate-limited, and service-unavailable states.

## 18. Analytics and success criteria
For each metric specify definition, baseline, target, data source, review cadence, and privacy implications. Avoid collecting data without a decision-making purpose.

## 19. Risks and dependencies
List product, technical, operational, legal, and third-party risks; mitigation and owner.

## 20. Assumptions and open questions
- `ASSUMPTION:` inferred but unconfirmed statement.
- `OPEN QUESTION:` decision required before implementation.
- `DECISION:` approved decision, with owner and date.

## 21. Release acceptance
Release is acceptable only when critical user journeys pass, security checks pass, data migrations are verified, monitoring and rollback exist, and no unresolved release blocker remains.

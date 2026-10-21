# Software Architecture

> Source of truth for approved technical structure. Align with `PRD.md` and `DESIGN.md`.

## 1. Architectural goals
Describe product-specific reliability, delivery speed, security, maintainability, cost, and scale priorities.

## 2. Decisions and constraints
Record confirmed stack, hosting constraints, team capabilities, existing code, and provider restrictions. Mark recommendations separately from approved decisions.

## 3. Technology stack
For each layer specify selected technology, version policy, reason, alternatives considered, operational cost, and migration risk. Prefer a modular monolith until independently scaling/deploying components is justified.

## 4. System context
Include a Mermaid context diagram showing users, client applications, backend, database, external services, and trust boundaries.

## 5. Runtime architecture
Describe processes, modules, request lifecycle, background workers, real-time channels, and failure isolation. Identify which operations are synchronous and which are asynchronous.

## 6. Repository structure
Define folder responsibilities and dependency direction. Avoid giant catch-all files and circular imports. Keep domain logic separate from transport and persistence.

## 7. Data flow
Trace each critical journey from UI through validation, authorization, domain service, transaction, persistence, events, and response. Identify data ownership and source of truth.

## 8. Frontend
Specify routing, state management, server-state strategy, forms, API client, error boundaries, offline behavior, reusable UI, and platform-specific differences.

## 9. Backend
Specify route/controller, schema validation, service/domain, repository/data access, integration adapters, background work, and transaction boundaries. Controllers should not contain business logic.

## 10. Authentication and authorization
Specify identity provider or auth mechanism, session/token lifecycle, role and resource checks, organization scoping, revocation, expiry, and audit events. Authorization must be enforced server-side.

## 11. API and real-time communication
Link `API.md`. Specify versioning, request IDs, pagination, idempotency, socket authentication, event authorization, reconnect behavior, and compatibility rules.

## 12. Database and migrations
Link `DATABASE.md`. Define migration ownership, deployment order, expand/contract strategy, lock considerations, transaction use, backup prerequisites, and rollback limits.

## 13. AI subsystem (if applicable)
Document model/provider selection, prompts, retrieval, embedding model, vector store, model timeouts, output validation, prompt-injection defenses, evaluation, caching, cost/latency controls, and fallback behavior. Treat model output as untrusted input.

## 14. External services
Use adapters around provider APIs. Define timeout budgets, retry rules, circuit-breaking only if justified, idempotency, webhook verification, and degraded-mode behavior.

## 15. Configuration and secrets
List variable names and descriptions, not values. Validate required variables at startup. Separate development, test, staging, and production. Use a secret manager or protected deployment environment.

## 16. Observability
Define structured logs, request/correlation IDs, metrics, traces where useful, health/readiness checks, alert ownership, and sensitive-field redaction.

## 17. Security boundaries
Document public/private endpoints, trusted/untrusted inputs, upload boundaries, internal service access, database permissions, network boundaries, and data classification.

## 18. Testing
Define unit, integration, contract, end-to-end, migration, security, performance, and mobile/device tests. Make tests deterministic and isolate external providers.

## 19. Deployment and recovery
Document build artifacts, environments, migration sequencing, rollout strategy, health checks, rollback, backup/restore, disaster recovery, and manual approvals.

## 20. Scaling strategy
State current expected workload and bottlenecks. Define evidence thresholds before adding queues, Redis, replicas, service decomposition, or vector infrastructure.

## 21. Architecture decision records
For material choices record context, decision, alternatives, consequences, owner, date, and revisit trigger.

## 22. Open questions
List unresolved choices and the impact of proceeding without them.

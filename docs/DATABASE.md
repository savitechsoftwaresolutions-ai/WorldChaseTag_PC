# Database Design

> The approved schema must support `PRD.md` journeys and `ARCHITECTURE.md` security boundaries.

## 1. Database technology
Specify engine/version, hosting, extensions, licensing, operational ownership, and reasons. Distinguish current implementation from recommendation.

## 2. Modeling rules
- Use stable primary keys and explicit foreign keys.
- Store timestamps consistently (normally UTC); display in user timezone.
- Use numeric/decimal types for money; never floating point.
- Define enum/status lifecycle and transition rules.
- Normalize transactional data; denormalize only for a measured need.
- Store uploaded files in object/file storage and persist metadata, not large binary payloads, unless justified.
- Never store plaintext passwords or raw authentication secrets.

## 3. Entity catalog
For each table specify purpose, owner, sensitivity, lifecycle, and retention.

## 4. Table specifications
For each column document:
| Column | Type | Null? | Default | Constraints | Meaning |
|---|---|---|---|---|---|
Include PK, FK, unique constraints, check constraints, generated values, and deletion behavior.

## 5. Relationships
Include Mermaid ER diagram and explain cardinality, optionality, cascade behavior, and tenant/organization boundaries.

## 6. Authorization and ownership
Define how each query scopes rows to the authenticated user/organization/resource. Apply row-level security only when supported and correctly configured; it does not replace application authorization.

## 7. Indexes
For every index identify the actual query or constraint it supports. Consider composite index column order, sort/filter patterns, write cost, and query plans. Avoid speculative indexes.

## 8. Transactions and concurrency
Document transaction boundaries, isolation requirements, optimistic/pessimistic locking, uniqueness races, idempotency keys, and retry-safe operations.

## 9. Validation and integrity
Define database constraints and application-level validation. Critical invariants must not rely only on UI validation.

## 10. Migrations
Specify migration tool, naming convention, forward-only or rollback policy, review process, compatibility window, expand/contract approach, and production approval. Never rewrite an already-applied migration without an explicit migration policy.

## 11. Seed and test data
Keep seed data deterministic and non-sensitive. Production seeds must be explicitly reviewed. Never use real customer data in tests unless approved and protected.

## 12. Privacy and retention
Define sensitive fields, encryption requirements, masking, access audit, deletion/anonymization, export, retention, and legal hold rules.

## 13. Backup and recovery
Specify backup cadence, retention, encryption, restore testing, RPO/RTO targets, ownership, and recovery procedure. A backup is not verified until restoration has been tested.

## 14. Performance and maintenance
Define expected data volume, query review process, vacuum/statistics/maintenance needs, partitioning thresholds, and slow-query monitoring.

## 15. Data dictionary and open questions
Maintain business definitions and list unapproved assumptions.

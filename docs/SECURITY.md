# Security Model and Checklist

## Assets and trust boundaries
List sensitive data, credentials, business-critical actions, external providers, and trust boundaries.

## Threat model
For each threat: asset, attacker capability, entry point, impact, likelihood, control, and verification.

## Identity and access
Define authentication, role/ownership checks, tenant isolation, session lifetime, revocation, and audit events.

## Data protection
Define classification, encryption in transit/at rest, retention, deletion, backups, masking, and access logging.

## Application controls
Input validation, output encoding, CSRF where applicable, SSRF controls, upload validation, rate limits, secure headers, and dependency management.

## AI-specific controls (if applicable)
Prompt injection, untrusted retrieved content, tool permission boundaries, data leakage, output validation, evaluation, and cost/abuse limits.

## Incident response
Contact/owner, severity criteria, containment, credential rotation, evidence preservation, notification requirements, and recovery.

## Findings register
| ID | Severity | Finding | Evidence | Fix | Verification | Status |
|---|---|---|---|---|---|---|

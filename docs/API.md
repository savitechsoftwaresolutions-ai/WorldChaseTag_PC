# API Contract

> Define only approved operations. Keep this contract consistent with `PRD.md`, `ARCHITECTURE.md`, and `DATABASE.md`.

## 1. Conventions
- Base URL by environment; never commit credentials.
- Versioning policy and compatibility expectations.
- JSON encoding, UTC timestamps, identifier format, and content types.
- Request/correlation ID and idempotency conventions.
- Maximum request and upload sizes.

## 2. Authentication
For each endpoint specify required identity, role, resource ownership, scopes, and behavior for expired or invalid credentials. Do not rely on client-side route guards.

## 3. Error format
Use a consistent envelope with stable machine-readable code, safe user-facing message, request ID, and optional field errors. Never expose stack traces, SQL, tokens, or provider secrets.

## 4. Endpoint catalog
For every endpoint include:
- ID and method/path
- Purpose and actor
- Authentication/authorization
- Request schema and validation
- Success status and response schema
- Error statuses
- Pagination/filter/sort semantics
- Idempotency and side effects
- Rate limits
- Audit and privacy implications

## 5. Pagination and filtering
Use consistent cursor or offset pagination appropriate to the dataset. Enforce maximum page sizes, allowlisted sort fields, validated filters, and stable ordering.

## 6. Uploads and downloads
Define allowed MIME types, size limits, extension/content verification, malware scanning where appropriate, storage permissions, signed URL expiry, and download authorization. Do not trust client-provided MIME types.

## 7. Webhooks
Document provider, event types, signature verification, timestamp/replay defense, idempotent processing, retry behavior, and dead-letter/manual recovery path.

## 8. Real-time events
Specify event name, payload schema, authorization, room/channel membership, delivery guarantees, reconnect strategy, and compatibility. Never broadcast private data to all connected clients.

## 9. External integrations
For each provider document operations, scopes, timeout, retryable errors, rate limits, idempotency, fallback, and test environment.

## 10. Rate limiting and abuse
Define per-user/IP/tenant limits based on risk, response headers, retry-after behavior, and protection for expensive AI or upload endpoints.

## 11. Environment variables
List variable name, purpose, required environments, validation, and secret/non-secret classification. Never include real values.

## 12. Contract testing
Validate schemas, permissions, error cases, pagination, webhook signatures, and backward compatibility in CI.

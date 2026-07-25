# Contracts and Payments — Phase 0 Baseline and Decisions

Recorded on 2026-07-24 before feature code changes, from Git commit
`62557b5d025c4e25f33f59de80e920c1e03af73a`.

## Baseline

- Solution: `SmartCourt.sln`, targeting .NET 8.
- Persistence: SQL Server through EF Core 8.0.29.
- Migration baseline: `20260723233804_AddLawDocuments`; the model snapshot uses
  EF Core 8.0.29.
- Baseline build: succeeded with 0 errors and 26 existing warnings.
- Baseline tests: 138 passed, 0 failed, 0 skipped.
- Restore warning: the requested `UglyToad.PdfPig.Core`
  `0.1.9-alpha001-patch1` was unavailable, so NuGet resolved
  `1.7.0-custom-5`.

## Existing application conventions

- Authentication uses ASP.NET Core Identity with JWT bearer tokens.
- The authoritative user identifier claim is
  `ClaimTypes.NameIdentifier`; `sub` also contains the user ID.
- Roles are emitted as `ClaimTypes.Role`.
- `ICurrentUserService` exposes the authenticated user's nullable `Guid`
  and authentication state.
- The database currently seeds `Client`, `Lawyer`, and the legacy `Admin`
  role.
- Controllers wrap results in `ApiResponse<T>` or `ApiResponse`.
- Global exception middleware currently maps validation to 400,
  authentication to 401, forbidden access to 403, missing resources to 404,
  rate limiting to 429, `BusinessException` to 400, and unexpected exceptions
  to a sanitized 500 response.
- `ApplicationDbContext` uses assembly-scanned Fluent API configurations.
- No clock abstraction is currently registered; existing code uses
  `DateTime.UtcNow`.

## Frozen v1 decisions

### Authorization

Contracts and Payments use these exact, case-sensitive role names:

- `Client`
- `Lawyer`
- `Moderator`
- `FinanceAdministrator`
- `SuperAdministrator`

The legacy `Admin` role remains available to unrelated features but does not
implicitly satisfy a Contracts and Payments policy. Resource ownership and
participant checks remain service-level requirements in addition to role
authorization.

### Money and fee rounding

- Currency is exactly `EGP`.
- Persisted money uses `decimal(18,2)`.
- The platform fee is 5% of the non-refunded lawyer gross allocation.
- Calculate the fee with
  `decimal.Round(value, 2, MidpointRounding.AwayFromZero)`.
- Calculate lawyer net as `lawyer gross allocation - rounded fee`; do not
  independently round the net.
- Every settlement must satisfy:
  `gross hold = client refund + lawyer net release + platform fee`.

### Idempotency retention

- `Idempotency-Key` is scoped by actor, operation, and business resource and is
  compared using a canonical request hash.
- Replay response bodies are retained for 30 days after a terminal result.
- For financial operations, the key, scope, request hash, terminal status, and
  result reference are retained indefinitely. Cleanup may purge only the
  cached response body after 30 days.
- Non-financial response-only idempotency records may be deleted after their
  30-day expiry.
- Payment transactions, provider attempts, ledger entries, and settlement
  keys never expire or participate in idempotency cleanup.

### Payment webhook authentication

- Each payment provider has a separate configured webhook secret.
- Requests carry `X-Payment-Event-Id`, `X-Payment-Timestamp` as Unix seconds,
  and `X-Payment-Signature` as `v1=<base64 HMAC-SHA256>`.
- The signed bytes are the UTF-8 bytes of
  `<timestamp>.<exact raw request body>`.
- Signatures are compared in constant time.
- Requests outside a five-minute clock window are rejected.
- Provider event IDs are persisted and deduplicated, so a valid replay is a
  safe no-op.
- The deterministic mock provider uses the same protocol with a
  development/test-only configured secret.

### Optimistic concurrency

- SQL Server `rowversion` is the concurrency token for mutable aggregate and
  projection roots.
- HTTP responses expose the token as a strong ETag containing quoted base64,
  for example `"AQIDBA=="`.
- Mutation requests send that exact value in `If-Match`.
- Weak, wildcard, missing where required, malformed, or stale ETags are
  rejected; stale tokens become a globally wrapped 409 conflict.
- Row versions are never writable DTO business fields.

### Pagination

- Query parameters are `page` and `pageSize`.
- Both are one-based positive integers; defaults are `page=1` and
  `pageSize=10`.
- Contracts and Payments allow `pageSize` from 1 through 100.
- Responses use `PagedResult<T>` with `Items`, `Page`, `PageSize`,
  `TotalCount`, and `HasNextPage`.
- Every paged query has a documented deterministic primary sort and an `Id`
  tie-breaker.

### UTC clock

- Register `TimeProvider.System` as the application clock and inject
  `TimeProvider` into feature services, providers, jobs, and persistence
  helpers that create or compare timestamps.
- Obtain UTC through `timeProvider.GetUtcNow()` and persist UTC values as SQL
  Server `datetime2`.
- Tests use a controllable `TimeProvider`; they do not sleep or depend on the
  machine's local timezone.
- New Contracts and Payments code must not call `DateTime.Now`,
  `DateTime.UtcNow`, or other ambient wall-clock APIs.

### Persistence inheritance

The existing `BaseEntity` contains `IsDeleted`, and `AuditableEntity` inherits
it. No Contracts and Payments persistence entity will inherit either type.
These entities declare their required `Guid` key and UTC audit fields directly.
Financial, submission, dispute-resolution, evidence, state-history,
idempotency, and outbox records have no soft-delete property or query filter;
their histories are append-only.

## Known follow-up required by later phases

- Seed and policy registration must add `Moderator`,
  `FinanceAdministrator`, and `SuperAdministrator`.
- The global exception contract must be enhanced backward-compatibly so
  state, idempotency, and concurrency conflicts return wrapped 409 responses,
  and exceptional provider failures return wrapped 502 responses.
- Feature pagination must not reuse the existing `PagedRequest` maximum of 50
  without changing or overriding that limit.
- The UTC `TimeProvider` registration and feature usage are implemented with
  the feature infrastructure, not as part of this readiness record.

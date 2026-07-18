# Smart Court Comprehensive End-to-End Code Review — V2

**Review date:** 2026-07-18  
**Scope:** All implemented feature slices, shared infrastructure, persistence, providers, migrations, tests, prior review artifacts, and documented API contracts.

## Executive Assessment

**Overall status: Broken / not integration-ready.**

The code builds, and several previous fixes are present, but critical security, contract, persistence, and workflow gaps remain. Most notably:

- Anonymous IDOR vulnerabilities affect every user-verification operation.
- Production credentials remain tracked in Git.
- Diagnostic endpoints and exception middleware disclose internals.
- Auth/profile APIs remain materially incompatible with the canonical OpenAPI.
- Verification state and document replacement workflows are not reliable end-to-end.
- EF Core reports pending model changes.
- CI deploys without running tests.
- Most contracted modules are absent.

### Verification Performed

- `dotnet build SmartCourt.sln --no-restore`: **succeeded with 25 warnings**.
- `dotnet test SmartCourt.sln --no-build`: **6/6 passed**, covering only two admin-verification helper classes.
- `dotnet ef migrations has-pending-model-changes`: **failed—model changes are not migrated**.
- Focused Auth/Users OpenAPI: **24 paths / 25 operations**.
- Unified OpenAPI: **83 paths / 100 operations**.
- Current implementation: approximately **30 controller actions**, including five diagnostic/test actions.
- No source files were changed during the review. Pre-existing uncommitted seeder changes and deletion of `task.md` were preserved.
- The online dependency vulnerability scan timed out, so dependency vulnerability status remains unverified.

# Part 1: Global Issues & Open Questions

## Global Issues

### G-01 — P0: Tracked credentials must be treated as compromised

Both tracked appsettings files contain non-placeholder database, JWT, Supabase, and/or SMTP credentials. Removing them in a later commit will not remove them from Git history.

**Impact:** Database access, JWT forgery, privileged storage access, and email-account abuse.

**Required action:** Rotate every credential, purge sensitive history where appropriate, replace values with environment variables/User Secrets/a secret manager, and add automated secret scanning.

### G-02 — P0: Anonymous IDOR in the complete user-verification slice

`SmartCourt/Features/UserVerification/UserVerificationController.cs` has no `[Authorize]`. Upload, retrieval, and deletion all trust a caller-supplied `UserId`.

An anonymous caller can therefore:

- Upload documents under another user.
- Retrieve another user’s verification metadata.
- Delete another user’s verification document and storage object.

This is the clearest deployment-blocking vulnerability.

### G-03 — P0: Unsafe diagnostic and operational surfaces

- `TestController` returns stored migration exceptions.
- Anonymous endpoints can enqueue email and send SMS to arbitrary numbers.
- `ExceptionHandlingMiddleware` serializes `exception.ToString()` into 500 responses.
- `Program.cs` mounts Hangfire Dashboard before authentication/authorization and without an Admin policy.
- `DatabaseSeeder` creates predictable administrator/test accounts with hard-coded passwords in every environment.

### G-04 — P0: Verification lifecycle is not trustworthy

Submission does not derive identity from claims, does not set the account to `PendingReview`, and permits multiple “current” versions of the same document type. Account status is recalculated only during admin review.

Consequences:

- An `Active` lawyer can remain active after documents expire.
- Admin details can label an active seeded lawyer “fully verified” despite having no documents.
- Replacement documents leave both the old and new version current until review.
- Deleting a verified document has an explicit unfinished status-transition comment.
- No unique index or concurrency token prevents racing submissions/reviews.

### G-05 — P0: Contract implementation is substantially incomplete

The focused OpenAPI defines canonical surfaces such as:

- `/api/users/profile`
- `/api/legal-categories`
- `/api/lawyer-verification/*`
- `/api/files/*`
- `POST /api/auth/verify-email`

These are absent or replaced with incompatible routes and request models. The unified 100-operation contract additionally includes cases, AI analysis, matching, marketplace, proposals, contracts, payments, chat, notifications, disputes, reviews, articles, and admin management; nearly all are unimplemented.

Cross-module integration cannot start safely until the intended current scope and canonical contract are settled.

### G-06 — P1: Authentication/session hardening remains incomplete

- Refresh tokens are stored plaintext.
- Refresh requires both access and refresh tokens instead of the contracted refresh-only request.
- Refresh failures return 400 rather than 401.
- Refresh validation disables issuer and audience checking.
- Suspended, rejected, deleted, or unconfirmed state is not rechecked on refresh.
- Refresh token lifetime is hard-coded to 14 days versus the documented 7.
- Login lockout is disabled.
- Login and registration have no rate limiting.
- Concurrent refreshes can both rotate the same token.
- Existing access JWTs remain usable after profile deletion/password change until expiry because status/security-stamp checks are not applied to JWT requests.

### G-07 — P1: API response and exception standards are not global

Successful business endpoints usually use `ApiResponse<T>`, but the standard breaks at several boundaries:

- Automatic FluentValidation/model-binding failures return `ValidationProblemDetails`.
- JWT 401/403 responses are not normalized.
- Rate-limit failures return plain text.
- Test endpoints return anonymous objects and strings.
- Admin document content returns a raw file.
- CQRS handlers return `ApiResponse` failures and HTTP status codes instead of throwing domain exceptions.
- User-verification controllers manually branch on response status.

This mixes HTTP concerns into handlers/services and makes client behavior inconsistent.

### G-08 — P1: Persistence violates the Fluent API-only standard

- `[Owned]` appears on `RefreshToken`.
- `[ForeignKey]` appears twice on `UserVerificationDocument`.
- `StoredFile`, `SampleEntity`, and `UserVerificationDocument` redeclare `Id`, hiding `BaseEntity.Id`.
- Several entities lack explicit configuration, constraints, and indexes.
- No global soft-delete filter uses `BaseEntity.IsDeleted`.
- EF reports pending model changes.
- Legal taxonomy names have no length/uniqueness constraints.
- Verification has no filtered uniqueness constraint for one current document per user/type.

No DTO Data Annotations or AutoMapper usage was found, although the unused AutoMapper package remains installed.

### G-09 — P1: Startup and deployment behavior is unsafe

- `ApplicationBuilderExtensions.UseAutoMigration` swallows migration failures and continues.
- The same failure is later exposed by the test endpoint.
- Seeding uses synchronous blocking inside an async `Main`.
- Supabase initialization blocks with `GetAwaiter().GetResult()`.
- Options are not validated at startup.
- `AppUrl` is absent from both settings files, so auth emails default to `http://localhost:5000`, while the development app runs on port 5049.
- CI builds and deploys but never runs `dotnet test`.
- Warnings do not fail CI, despite current entity/nullability warnings indicating real model problems.

### G-10 — P1: Rate limiting is incorrectly scoped

Forgot-password, resend, and reset use named fixed-window limiters with no partition key. That produces one shared limit per policy/application instance, so three users can exhaust the entire hour’s capacity for everyone. Per-IP or per-account limits require a partitioned policy.

The 429 response is also plain text rather than `ApiResponse`.

### G-11 — P1: File handling is unbounded and trusts attacker-controlled metadata

- Upload buffers each complete file into a `MemoryStream`, then creates another byte-array copy.
- Admin download buffers the full object into a `byte[]`.
- No per-file, aggregate, or request-size limit exists.
- Content type and extension come from the caller.
- File signatures are not inspected.
- Provider exception messages can be returned to clients.
- Cancellation tokens are not propagated by the Supabase SDK calls.
- Storage/database compensation is incomplete.

### G-12 — P1: Automated coverage is far below integration readiness

Only six pure unit tests exist. There are no tests for:

- Any controller endpoint.
- Authentication, refresh rotation/replay, status checks, or lockout.
- Profile authorization/deletion/email changes.
- Verification upload, IDOR, replacement, deletion, or storage compensation.
- Admin review concurrency.
- Middleware response shapes.
- EF mappings/migrations.
- OpenAPI contract compatibility.

## Previous Review Verification

Several prior items were genuinely implemented:

- Dedicated change/forgot/resend/reset services.
- Password change/reset refresh-token revocation.
- Shared password validation.
- Forgot/resend anti-enumeration behavior.
- Resend confirmation now uses `userId`.
- One-hour Identity token lifetime.
- Current-user abstraction.
- Projected profile reads.
- Soft account status rather than hard deletion.
- Lawyer profile creation.
- Admin review helpers and six tests.

However, the deleted `task.md` marked several items complete that are still incomplete:

- Canonical `/api/users/profile` routes were not implemented.
- `ClientProfile` is still not created during registration.
- `AppUrl` is not configured, leaving email links broken by default.
- Email/username change is not atomic.
- Government/city are still flattened into address.
- Lawyer specialization is singular, while the contract requires a replaceable list.
- Public lawyer retrieval does not restrict results to verified lawyers.
- The wider prior-review findings on secrets, stack traces, refresh security, lockout, contracts, and response normalization remain open.

## Open Questions Requiring Input

1. Which contract is canonical: the focused OpenAPI, `profiles_and_verification_api.md`, the `/api/v1/verifications` design, or the current controllers?
2. Is the requested integration milestone Sprint 1 only, Sprint 1–2, or all 100 unified OpenAPI operations?
3. Should verification accept binary multipart uploads, or only previously uploaded `StoredFileId` values?
4. Should email confirmation set clients to `Active` and lawyers to `PendingReview`?
5. May unverified/pending lawyers log in, and which capabilities must be blocked until verification?
6. Is lawyer verification reviewed per individual document, per National-ID/Bar-card pair, or as one application?
7. Should lawyer profiles support one specialization or multiple legal categories/specializations?
8. Should public marketplace queries expose only `Active`, currently verified Lawyer-role users?
9. What does DELETE profile mean: deactivate, retention-aware erasure request, or permanent deletion?
10. Is a binary download endpoint formally exempt from the `ApiResponse<T>` rule, or should it return a short-lived signed URL?
11. Is Supabase authoritative, or should Sprint 1 use local storage as the sprint plan specifies?
12. Should migrations and seeders run automatically in production, or through a controlled deployment step?

# Part 2: Slice-by-Slice End-to-End Review

## Auth / RegisterClient

**Status:** Partially implemented  
**Violations/gaps:** Client profile and notification preferences are not created. User creation, role assignment, and email enqueue are non-transactional; role/email results are ignored. A public request may create system roles. Fields conflict with OpenAPI (`FullName`/`NationalNumber` versus first/last name/phone). Duplicate identities map to 400 rather than 409.  
**Integration readiness:** **No.** It can leave a created user without a role/profile or usable email.  
**Security/performance:** No abuse control; tracked credentials and broken default `AppUrl` affect the flow.

## Auth / RegisterLawyer

**Status:** Partially implemented / contract-dependent  
**Violations/gaps:** LawyerProfile creation is fixed, but document submission and verification initiation do not match available contracts. Gender accepts arbitrary text. Combined address can exceed the database’s 500-character limit even though individual fields validate. Government/city are discarded as distinct data. Role/email results are ignored and the workflow is non-transactional.  
**Integration readiness:** **No.** Registration cannot reliably feed profile, taxonomy, and verification modules.

## Auth / Login

**Status:** Partially implemented  
**Violations/gaps:** Response shape differs from OpenAPI; refresh lifetime is 14 rather than 7 days; lockout is disabled; refresh persistence result is ignored; role selection uses only the first role. Status handling blocks Suspended/Deleted but does not resolve the intended Unverified/PendingReview/Rejected policy.  
**Integration readiness:** **No.** Downstream clients cannot rely on the documented token/user payload or lifecycle policy.  
**Security/performance:** No rate limit; historical refresh records accumulate.

## Auth / RefreshToken

**Status:** Broken against contract and insufficiently hardened  
**Violations/gaps:** Requires an expired access token, returns full login data, maps invalid credentials to 400, disables issuer/audience validation, does not recheck account state, and stores raw tokens.  
**Integration readiness:** **No.**  
**Security/performance:** Full refresh-token collection is loaded; no token hash/index/family/concurrency protection; simultaneous reuse can issue multiple replacements.

## Auth / RevokeRefreshToken

**Status:** Partially implemented / uncontracted  
**Violations/gaps:** No explicit `[AllowAnonymous]` or `[Authorize]`; depends on permissive fallback behavior. Shares refresh validation and plaintext weaknesses. Returns 200/false for unknown tokens; intended idempotency is undocumented.  
**Integration readiness:** **No**, until its contract and authentication model are defined.

## Auth / ConfirmEmail and ConfirmEmailChange

**Status:** Broken against contract  
**Violations/gaps:** Uses GET/query primitives instead of `POST /api/auth/verify-email` with DTO/validator. No DTO or Validators folder. Malformed Base64 can produce 500. Email confirmation does not transition `UserStatus`. Email change persists email first and username second, allowing divergence if the second save fails. Query values are not safely URL-encoded.  
**Integration readiness:** **No.** Default links point to an unconfigured localhost URL.  
**Security:** State-changing tokens are exposed in URLs/logs.

## Auth / ResendVerification

**Status:** Partially implemented  
**Violations/gaps:** Prior broken `email=` parameter is fixed and anti-enumeration is correct. The configured rate limiter is application-wide rather than per caller, and the default confirmation URL remains broken because `AppUrl` is missing.  
**Integration readiness:** **No**, until URL configuration and limiter partitioning are fixed.

## Auth / ForgotPassword

**Status:** Partially implemented  
**Violations/gaps:** Anti-enumeration and one-hour token lifetime are fixed. The reset link defaults to the wrong localhost address, provider cancellation is omitted, and rate limiting is shared globally.  
**Integration readiness:** **No.**

## Auth / ResetPassword

**Status:** Mostly implemented locally  
**Violations/gaps:** Strength validation, malformed-token handling, and refresh revocation exist. Rate limiting is globally shared; error/status contracts and access-JWT invalidation remain unresolved.  
**Integration readiness:** **No**, pending common auth/session and response fixes.

## Auth / ChangePassword

**Status:** Mostly implemented locally  
**Violations/gaps:** Previous review requirements are largely satisfied, but `UpdateAsync` result is ignored and existing access JWTs remain valid.  
**Integration readiness:** **No**, pending the global active-account/security-stamp enforcement strategy.

## Users / Clients

**Status:** Partially implemented  
**Violations/gaps:** Route remains `/api/clients/profile`, not the canonical `/api/users/profile`. Registration does not create ClientProfile. Update sends an email-change token but does not persist an explicit pending-email state. Delete marks status Deleted, but the same access JWT can continue calling GET/PUT. Success text is placed in `data` rather than `message`.  
**Integration readiness:** **No.**  
**Performance/security:** The one-query profile projection is good and NationalNumber is no longer exposed.

## Users / Lawyers

**Status:** Partially implemented  
**Violations/gaps:** Canonical routes and multi-specialization contract are not implemented. Specialization existence is not validated before saving, so an invalid ID can reach a database exception. Date of birth is not required to be in the past. Public lookup does not verify Lawyer role, Active status, or current verification; passing a client ID can expose that client’s name, gender, status, and profile image as a “lawyer.” Deleted/unverified users remain publicly visible.  
**Integration readiness:** **No.**  
**Performance/security:** Projected reads are efficient, but marketplace filtering/paging/search is absent.

## UserVerification / SubmitVerificationDocuments

**Status:** Broken / critical  
**Violations/gaps:** Anonymous IDOR; unclean CQRS response handling; validator outside `Validators/`; synchronous validation; nullable collection/item/file crashes; no enum/file-size/signature checks; contract uses the wrong upload model; no mandatory role-specific document set; partial failures still return success.  
**Lifecycle gaps:** Old current versions remain current; account status is not transitioned; no uniqueness/concurrency constraint; storage/database compensation is incomplete.  
**Integration readiness:** **No.**  
**Performance/security:** Whole-file buffering, spoofable content types, provider error leakage, and missing cancellation.

## UserVerification / GetUserVerificationDocuments

**Status:** Broken / critical  
**Violations/gaps:** Anonymous IDOR and caller-supplied user ID. `DocumentId` is actually `StoredFileId`, which conflicts with the admin document ID. Returns every historical version without an explicit contract. Validation failures are returned rather than thrown.  
**Integration readiness:** **No.**  
**Performance:** Read query correctly uses `AsNoTracking`, but cancellation is omitted from the final query and an extra user lookup is performed.

## UserVerification / DeleteVerificationDocument

**Status:** Broken / unfinished  
**Violations/gaps:** Anonymous IDOR; verified-document lifecycle is a TODO; identifies a document by StoredFile ID; deletes storage before database commit; removes both dependent and cascade principal; returns failures rather than domain exceptions.  
**Integration readiness:** **No.**  
**Reliability:** A database failure after storage deletion leaves irreparable metadata; a storage failure leaves no auditable retry state.

## Admin Verifications / GetPending

**Status:** Partially implemented  
**Violations/gaps:** CQRS is acceptable, but validators are not under `Validators/` and handlers return transport responses. The default “pending” endpoint actually returns any lawyer with a current document when no status is supplied. Contract route/shape differs.  
**Integration readiness:** **No**, until queue semantics and contract are settled.  
**Performance:** Pagination and `AsNoTracking` are good; two expected queries are used for count/page.

## Admin Verifications / GetDetails

**Status:** Partially implemented  
**Violations/gaps:** Read query tracks a full document graph; Lawyer role is checked in a second query. `IsFullyVerified` is inferred from account status instead of evaluating current, unexpired required documents.  
**Integration readiness:** **No.** An Active seeded lawyer with no verification documents can be reported fully verified.

## Admin Verifications / GetDocumentContent

**Status:** Partially implemented  
**Violations/gaps:** Returns raw `FileResult`, violating the literal all-endpoints-ApiResponse rule. Invalid IDs return failure responses instead of validation exceptions.  
**Integration readiness:** **No**, pending signed-URL/binary-contract decision.  
**Performance/security:** Entire sensitive documents are loaded into memory; no audit event or download-size limit exists.

## Admin Verifications / ReviewDocument

**Status:** Partially implemented  
**Violations/gaps:** Core status evaluator is tested, but handler returns HTTP-aware failure responses. No concurrency token prevents two admins from reviewing the same document. Expiration/status reconciliation only happens when this endpoint is called. `VerifiedByAdminId` is an unconstrained string rather than a GUID foreign key/audit relation.  
**Integration readiness:** **No.**

## Legal Categories and Specializations

**Status:** Persistence-only / partially implemented  
**Violations/gaps:** Current uncommitted seeder populates categories, but there is no feature slice, controller, service, DTO, validator, or `/api/legal-categories` endpoint. Only one specialization is supported per lawyer. Names lack explicit length/unique constraints and soft-delete filters. Seeder’s `AnyAsync()` prevents repairing a partially seeded dataset.  
**Integration readiness:** **No.**

## Test Slice

**Status:** Unsafe; must not ship  
**Violations/gaps:** Does not follow VSA, returns nonstandard responses, performs provider orchestration in the controller, exposes migration internals, and enables anonymous email/SMS abuse.  
**Integration readiness:** **No.** Remove or compile/map only in controlled development environments.

## Providers and Infrastructure

**Status:** Partially implemented  
**Positive:** Feature code generally uses provider abstractions; no feature service directly instantiates external SDKs; no AutoMapper use.  
**Gaps:** Blocking Supabase initialization, real SMTP in development, missing option validation, non-cancellable provider SDK operations, full-file buffering, no retries/idempotency/outbox semantics, public Hangfire surface, and unused packages.  
**Integration readiness:** **No.**

## Persistence and Migrations

**Status:** Broken for reproducible deployment  
**Gaps:** Pending model changes, Data Annotations, hidden IDs, incomplete Fluent configurations, no query filters, weak constraints, synchronous migrations, swallowed migration errors, and unsafe universal seed accounts.  
**Integration readiness:** **No.**

## Contracted but Absent Feature Modules

**Status:** Unimplemented

Missing or effectively absent:

- Generic file management.
- Canonical current-user profile.
- Legal-category API and multi-specialization management.
- Canonical lawyer-verification API/status.
- Cases and attachments.
- AI analysis and lawyer matching.
- Marketplace search/listing.
- Proposals.
- Chat/SignalR.
- Contracts, signing, and milestones.
- Payments, escrow, releases, and webhook handling.
- Reviews.
- Disputes.
- Notifications/preferences.
- Articles/moderation.
- AI assistant/RAG.
- Admin dashboard and user management.

None are integration-ready because there are no vertical slices, persistence models, migrations, endpoints, or tests implementing their contracts.

# Part 3: Remediation Plan

## R-01 — Credential and Diagnostic Lockdown

**Proposed solution:** Rotate all committed credentials; remove runtime secrets from tracked files/history; enable secret scanning; delete or development-gate TestController; sanitize 500 responses; add Admin authorization to Hangfire after authentication middleware; remove production test users/passwords.

**Acceptance criteria:**

- No real credentials in tracked files or reachable Git history.
- Credential rotation is confirmed.
- Production 500 responses contain no stack traces.
- Test/email/SMS/migration-error endpoints return 404 outside Development.
- Hangfire requires an authenticated Admin.

## R-02 — Eliminate Verification IDOR

**Proposed solution:** Add `[Authorize(Roles = "Lawyer")]`; remove `UserId` from public commands/queries; derive it from `ICurrentUserService`; create separate explicit Admin operations.

**Acceptance criteria:**

- Users cannot provide another user’s ID.
- Cross-user upload/get/delete integration tests return 403/404.
- Anonymous calls return standardized 401.
- Admin access is implemented through Admin-only routes.

## R-03 — Rebuild Verification as One Atomic Lifecycle

**Proposed solution:** Choose the canonical upload/application model. Enforce required document sets, one current version per user/type, valid replacement transitions, account status changes, expiration reconciliation, storage compensation, and optimistic concurrency.

**Acceptance criteria:**

- Submission sets the correct account/application status.
- Database prevents duplicate current/pending records.
- Replacement, approval, rejection, expiration, resubmission, and deletion are fully tested.
- Expired documents revoke verified capabilities without waiting for manual review.
- Partial storage/database failures leave no orphaned objects or broken rows.
- Concurrent admin reviews cannot overwrite each other.

## R-04 — Complete Auth Contract and Refresh Security

**Proposed solution:** Implement refresh-token-only lookup using a SHA-256 hash and indexed token identifier; use configured seven-day expiry; validate status/email; return 401 for invalid tokens; introduce token families and atomic rotation; enable lockout and login/registration/refresh throttling; align login/refresh DTOs.

**Acceptance criteria:**

- No plaintext refresh tokens in the database.
- Refresh requires only `refreshToken`.
- Replay/concurrent-refresh tests issue at most one successor.
- Suspended/deleted/ineligible accounts cannot refresh.
- Invalid refresh returns standardized 401.
- Login and refresh response snapshots match OpenAPI.
- Lockout and abuse tests pass.

## R-05 — Define and Enforce Account Lifecycle Globally

**Proposed solution:** Agree status transitions and add authorization policies or JWT events that verify current account state for protected requests. Decide deletion/retention semantics.

**Acceptance criteria:**

- Client/lawyer email-confirmation transitions are documented and tested.
- Deleted/suspended users immediately lose protected access.
- Verification-dependent lawyer capabilities require current verification.
- Delete behavior satisfies retention/audit requirements.

## R-06 — Make Registration and Email Changes Reliable

**Proposed solution:** Seed roles at deployment, never from public requests; create user/profile/preferences/role atomically; use an outbox for email; validate result objects; implement durable pending-email state or an atomic confirmation transaction.

**Acceptance criteria:**

- Registration cannot leave a roleless/profileless partial user.
- ClientProfile and LawyerProfile exist immediately.
- Failed email enqueue is recoverable/retryable.
- Email and username never diverge.
- Duplicate identifiers map to the documented 409.
- All registration fields and maximum lengths match the contract/schema.

## R-07 — Finish Profiles, Taxonomy, and Public Marketplace Boundaries

**Proposed solution:** Implement canonical `/api/users/profile` endpoints, legal-category slice, and the chosen single/multiple-specialization model. Filter public lawyer results by role, active/current verification, and soft-delete status.

**Acceptance criteria:**

- Routes and payloads pass OpenAPI contract tests.
- Invalid specialization IDs return validation/business errors, not 500.
- Public lookup never returns clients, deleted, unverified, or expired lawyers.
- Government/city and other required fields round-trip without flattening loss.
- Legal categories are idempotently seeded and publicly retrievable.

## R-08 — Standardize Validation, Responses, and CQRS Boundaries

**Proposed solution:** Configure model-state, authentication, authorization, and rate-limit handlers to emit `ApiResponse`. Move validators into required folders. Handlers/services return domain DTOs and throw approved exceptions; controllers own HTTP status mapping.

**Acceptance criteria:**

- Every JSON endpoint—including failures—uses one response schema.
- Validation, 401, 403, 404, 409, 429, and 500 contract tests pass.
- No service/handler constructs HTTP-aware `ApiResponse` failures.
- Binary endpoint behavior is explicitly documented as an exception or replaced by signed URLs.

## R-09 — Repair EF Model and Migration Reproducibility

**Proposed solution:** Remove entity Data Annotations and duplicate IDs; add Fluent configurations for every entity; add lengths, indexes, FKs, query filters, concurrency fields, and required uniqueness constraints; generate a migration and validate from an empty database.

**Acceptance criteria:**

- `has-pending-model-changes` exits successfully.
- Build has zero entity/nullability warnings.
- Empty-database migration and seed succeed.
- Soft-delete filters and verification uniqueness constraints are tested.
- Rollback/redeployment is reproducible.

## R-10 — Harden File and Provider Operations

**Proposed solution:** Enforce request/file/count limits, inspect magic bytes, normalize extensions, stream where supported, use signed URLs, propagate cancellation, validate options at startup, and use retry/outbox/idempotency policies.

**Acceptance criteria:**

- Oversized/spoofed files are rejected before storage.
- Upload/download memory remains bounded under load tests.
- No provider exception details reach clients.
- Cancellation interrupts provider operations where supported.
- Storage/database failure tests show no orphaned state.

## R-11 — Fix Startup, CI, and Quality Gates

**Proposed solution:** Run migrations as a controlled deployment step or fail startup loudly; await seeding asynchronously and only in approved environments; run build, tests, migration-drift checks, secret scans, dependency audits, and contract tests before deploy.

**Acceptance criteria:**

- Migration failures prevent deployment/healthy startup.
- CI runs tests before publish.
- CI fails on pending migrations, leaked secrets, and selected warnings.
- Production contains no fixed-password seed accounts.
- Critical auth/profile/verification integration tests exist.

## R-12 — Establish an Executable Delivery Scope

**Proposed solution:** Select the canonical OpenAPI and identify the operation subset required before cross-module integration. Mark future endpoints explicitly deferred rather than implicitly missing.

**Acceptance criteria:**

- One versioned API contract is authoritative.
- Every in-scope operation has a slice, persistence path, authorization policy, validation, and tests.
- CI verifies the generated API description against that contract.
- Deferred modules have owners, dependencies, and planned migrations.

## Final Recommendation

Do not begin cross-module integration or expose this API to real traffic until R-01 through R-04, R-08, R-09, and R-11 are complete.

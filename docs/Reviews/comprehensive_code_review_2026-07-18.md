# Smart Court Comprehensive Code Review

**Review date:** 2026-07-18  
**Scope:** Entire Smart Court backend repository, all implemented feature slices, shared infrastructure, persistence, providers, migrations, configuration, and documented contracts.

## Executive Assessment

**Overall status: Broken / not production-ready.**

The solution builds, but critical security and workflow failures make deployment unsafe:

- Production/database/storage/JWT/SMTP secrets are committed.
- User-verification endpoints are anonymous and vulnerable to cross-account access and deletion.
- Hangfire and test/diagnostic endpoints are unauthenticated.
- Startup resolves an incompatible Identity role type and seeds before migration.
- Email-resend links are broken.
- Registration, password reset, profile, verification, and refresh-token flows do not satisfy documented contracts.
- The unified contract describes 83 API paths and a 39-table schema; the repository currently exposes 24 controller actions, including test and uncontracted routes, with only a small subset of the planned schema.
- No automated tests exist.

### Verification Performed

- dotnet build SmartCourt.sln --no-restore: succeeded with **25 warnings**.
- dotnet ef migrations has-pending-model-changes: no pending changes.
- dotnet test SmartCourt.sln --no-build: no test project/tests.
- Worktree was clean; no files were changed during the review.

# Part 1: Global Issues and Open Questions

## P0 — Critical Security and Operational Issues

### G-01: Committed credentials must be considered compromised

Tracked configuration contains a public database credential, a Supabase service-role key, a JWT signing secret, and SMTP credentials in:

- SmartCourt/appsettings.json
- SmartCourt/appsettings.Development.json

Impact:

- Database compromise.
- Full privileged access to Supabase storage.
- Forged JWTs.
- Email-account misuse.
- Removing them in a future commit is insufficient because they remain in Git history.

### G-02: Anonymous IDOR across all verification operations

SmartCourt/Features/UserVerification/UserVerificationController.cs has no Authorize attribute. Every operation accepts an arbitrary caller-supplied UserId:

- Upload documents for another account.
- List another account's sensitive verification metadata.
- Delete another account's documents.

This is a critical IDOR and privacy breach.

### G-03: Public administrative and diagnostic attack surface

- Hangfire Dashboard is enabled without authorization in Program.cs.
- /api/Test/error exposes stored migration exceptions.
- Anonymous callers can enqueue email to a hard-coded recipient.
- Anonymous callers can send SMS to arbitrary numbers.
- ExceptionHandlingMiddleware returns exception.ToString(), including stack traces and internal details.

### G-04: Predictable production administrator accounts

DatabaseSeeder always seeds administrators with hard-coded known passwords, including a personal email address. This runs in all environments.

### G-05: Startup is logically broken

Program.cs seeds users before migrations and requests RoleManager&lt;IdentityRole&gt;, while Identity registers RoleManager&lt;IdentityRole&lt;Guid&gt;&gt;.

Consequences:

- Fresh databases fail before they can be migrated.
- Existing databases fail resolving the wrong generic service.
- Migration errors are swallowed, allowing an invalid application state to continue.
- A second synchronous seed runs later using GetAwaiter().GetResult().

## Architecture and Standards Violations

### G-06: Explicit MediatR/CQRS violation

UserVerification uses IMediator, commands, queries, and handlers. MediatR is registered in DependencyInjection.cs and referenced in SmartCourt.csproj.

This directly violates the service-only architectural standard and lacks the required IUserVerificationService and UserVerificationService.

### G-07: Slice structure is inconsistent

- ConfirmEmail has no DTO or Validators folder.
- UserVerification has no service interface/implementation and places validators beside commands.
- Test has no service, interface, DTOs, or validators.
- Auth/Shared has become a cross-cutting helper inside the Auth feature.
- Documentation claims three projects—Core, Infrastructure, API—but the solution has one project, so documented dependency boundaries cannot be enforced.

### G-08: API response standard is not globally enforced

Although most controllers wrap successful results:

- Test endpoints return anonymous objects, strings, and raw BadRequest responses.
- UserVerification returns failure objects instead of throwing domain exceptions, forcing controller status branching.
- Automatic ApiController and FluentValidation failures default to ValidationProblemDetails, not ApiResponse.
- JWT 401/403 responses and unmatched routes are not normalized.

### G-09: Entity configuration violates the Fluent API-only rule

- Owned is used as an attribute on RefreshToken.
- ForeignKey is used twice on UserVerificationDocument.
- StoredFile, SampleEntity, and TestEntity lack explicit configurations.
- StoredFile, SampleEntity, and UserVerificationDocument redeclare Id, hiding BaseEntity.Id.
- Nullable-reference warnings expose incomplete entity initialization.

No DTO Data Annotations or AutoMapper usage were found. The unused AutoMapper package should nevertheless be removed.

### G-10: Async standard is violated

Blocking calls occur while:

- Initializing Supabase in DependencyInjection.cs.
- Seeding in Program.cs.

Multiple services also omit cancellation propagation or use synchronous FluentValidation inside asynchronous handlers.

## Cross-Cutting Authorization and Security Concerns

### G-11: Authentication lifecycle is incomplete

- Email confirmation does not transition UserStatus.
- Login only blocks Suspended, meaning Unverified, PendingReview, and Rejected users can authenticate once email-confirmed.
- Refresh does not re-check email confirmation or account status.
- Login disables lockout tracking.
- No rate limiting exists for login, registration, resend verification, forgot password, upload, SMS, or email.
- No ICurrentUserService exists despite being a Sprint 1 P0 contract.

### G-12: Refresh-token design is insecure and inconsistent

- Refresh tokens are stored plaintext.
- Tokens accumulate indefinitely and are loaded as a complete collection.
- No unique token index, family/device relationship, or concurrency protection exists.
- Two simultaneous refreshes can both use the same active token.
- JWT refresh validation disables issuer and audience checks.
- Reset/change password do not revoke sessions.
- Code uses 14 days while the contract specifies 7 days.
- Invalid refresh tokens produce HTTP 400 through BusinessException, while the contract requires 401.

### G-13: Sensitive personal data needs an explicit policy

National numbers, dates of birth, addresses, and verification records are stored in plaintext. Profile DTOs return the national number. Current profile access is owner/admin-only, but a separate public marketplace DTO will be needed to ensure these fields can never leak.

## Cross-Cutting Performance and Reliability Concerns

### G-14: Unbounded file processing

SupabaseFileStorageService buffers each entire upload into memory and creates another byte-array copy. No request-size or per-file size limit exists.

Content type is trusted from the client; file signatures are not inspected.

### G-15: Excess database round trips and unbounded collections

- Profile reads execute existence lookup, role lookup, then projection.
- Refresh/revoke load every historical refresh token.
- Verification lacks a database uniqueness constraint for current/pending document type.
- Several read paths omit cancellation tokens.

### G-16: Audit and deletion semantics are incomplete

- CreatedBy and LastModifiedBy are permanently set to System.
- Only SaveChangesAsync populates audit fields.
- IsDeleted has no global query filter and is ignored.
- Profile delete hard-deletes the entire Identity account, although no DELETE endpoint appears in the referenced profile contract.

## Open Questions Requiring Product or Architecture Decisions

1. **Which verification contract is canonical?**
   - Generic /api/v1/verifications applications/assets.
   - Lawyer-specific /api/lawyer-verification endpoints.
   - Current /api/UserVerification binary-upload design.

2. **What is the current delivery scope?** Are the remaining 83-path OpenAPI modules deliberately future work, or should this repository already satisfy the complete 30-day plan?

3. **What are the intended account-status transitions?** Should email confirmation make clients Active and lawyers PendingReview, with lawyers restricted until professional verification?

4. **Is client NationalNumber required at registration?** The implementation requires it, while parts of the Auth backlog/contract omit it.

5. **Must lawyer documents be submitted during registration or later?** The registration contract says multipart with four files; another contract defines separate verification submission.

6. **What does DELETE profile mean?** Hard-delete account, soft-delete/deactivate, or initiate a retention-aware erasure workflow?

7. **Should changing email require confirmation before replacing the login email?**

8. **Which storage strategy is authoritative?** The sprint plan specifies local storage; current code uses Supabase; other docs discuss a generic pre-upload workflow.

9. **Should lawyer profiles be owner/admin-only or publicly browsable through a separate marketplace endpoint?**

# Part 2: Slice-by-Slice End-to-End Review

## Auth / RegisterClient

**Status: Partially Implemented**

### Violations and gaps

- Does not create ClientProfile or notification preferences.
- Role creation, user creation, role assignment, and email enqueue are not transactional.
- AddToRoleAsync and email-provider results are ignored.
- Duplicate email/national number produces 400 rather than documented 409.
- Public requests can create missing system roles.
- Required NationalNumber conflicts with parts of the contract.
- Cancellation cannot flow through Identity operations.

### Security and performance

- No registration rate limiting or anti-automation control.
- A user can remain created without role or usable confirmation email after partial failure.

## Auth / RegisterLawyer

**Status: Broken against contract**

### Violations and gaps

- Controller accepts JSON, not contracted multipart data.
- Does not accept or store four required documents.
- Does not create LawyerProfile, verification state, or IsAvailable.
- Address validation allows combined values beyond the database's 500-character limit.
- Gender accepts arbitrary strings although a Gender enum exists.
- Same non-transactional role/email problems as client registration.

## Auth / Login

**Status: Partially Implemented**

### Positive findings

- Thin controller.
- Manual DTO response mapping.
- Identity password checking.
- JWT provider abstraction.

### Identified gaps

- Response shape differs from contract.
- Refresh lifetime is 14 rather than 7 days.
- Lockout is disabled.
- Only Suspended is blocked; Rejected and other non-active states can log in.
- Refresh-token persistence result is not checked.
- Unlimited refresh tokens accumulate.

## Auth / RefreshToken

**Status: Security-sensitive and partially broken**

- Request requires both access token and refresh token, while the documented contract requires only refresh token.
- JWT issuer/audience validation is bypassed.
- Suspended/rejected/unconfirmed users can continue refreshing.
- Replay revokes every active token without session-family semantics.
- Concurrent replay can create multiple replacement tokens.
- Plaintext tokens and unbounded Include degrade security and performance.
- Business failures map to 400 rather than 401.

## Auth / RevokeRefreshToken

**Status: Partially Implemented / uncontracted route**

- Route is absent from the supplied OpenAPI Auth contract.
- Anonymous access may be intentional, but AllowAnonymous should be explicit.
- It inherits the JWT validation and plaintext-token weaknesses.
- Returns HTTP 200 with false for unknown/inactive tokens; expected idempotency needs clarification.

## Auth / ConfirmEmail

**Status: Contract mismatch**

- Implemented as GET /api/auth/confirm-email; documented route is POST /api/auth/verify-email.
- Primitive query values replace a DTO and validator.
- Malformed Base64 tokens can become a 500.
- Does not update UserStatus.
- State-changing token appears in a URL and may be logged by proxies.

## Auth / ResendVerification

**Status: Broken**

The generated link uses an email query parameter while ConfirmEmail requires userId. Every resent link is therefore unusable.

Additional gaps:

- No rate limit despite max 3/hour contract.
- No cancellation-token propagation.
- Duplicates confirmation-email construction instead of reusing IAuthHelperService.
- Return payload puts the success text in Data rather than Message.

## Auth / ForgotPassword

**Status: Partially Implemented**

- Nonexistent email correctly returns success.
- Unconfirmed existing email throws a distinct 400, reintroducing user enumeration.
- No 3/hour rate limiting.
- Reset-token lifetime is not configured to the documented one hour.
- No cancellation propagation.
- Generated URL points at the API POST route rather than a confirmed frontend reset page.

## Auth / ResetPassword

**Status: Partially Implemented**

- Does not revoke refresh tokens as required.
- Validator only checks minimum length; full password strength is deferred to Identity.
- Malformed encoded tokens may become 500 responses.
- No cancellation propagation or throttling.

## Auth / ChangePassword

**Status: Partially Implemented**

### Positive findings

- Correctly protected with Authorize.
- User ID comes from claims rather than request input.

### Gaps

- Does not revoke existing refresh tokens or update session state.
- No cancellation token.
- Password validator does not mirror registration strength rules.
- Incorrect-current-password and policy errors are collapsed into one field.

## Users / Clients

**Status: Partially Implemented**

### Positive findings

- Required folder structure exists.
- Controller is thin.
- Endpoint is protected and owner/admin checked.
- DTO is manually mapped.

### Gaps

- Routes differ from canonical /api/users/profile contract.
- GET performs three database operations.
- Null date of birth becomes DateOnly.MinValue.
- Email and username updates are separately persisted.
- Email-change verification behavior is undefined.
- Hard-delete removes the complete account.
- No cancellation tokens.
- Egyptian phone format conflicts with lawyer registration/update formats.

### Security note

National number is returned. Current owner/admin protection limits exposure, but this DTO must never be reused for public profiles.

## Users / Lawyers

**Status: Partially Implemented**

### Positive findings

- Thin controller.
- Owner/admin guard.
- Manual projected mapping.
- Final read projection avoids entity materialization.

### Gaps

- Registration does not create LawyerProfile; update silently creates one later.
- Same multi-query, atomic email update, cancellation, and hard-delete issues as Client.
- YearsOfExperience.NotEmpty rejects zero, while the next rule and contract allow zero.
- No upper bound on years of experience.
- Specialization is an unrestricted string rather than legal-category relationships.
- Missing IsAvailable, profile picture, verification status, government/city, and specializations.
- Owner-only GET cannot serve documented marketplace browsing.

## UserVerification / SubmitVerificationDocuments

**Status: Broken / critical**

### Violations

- Anonymous IDOR.
- MediatR/CQRS instead of service classes.
- Handler returns ApiResponse failures rather than throwing exceptions.
- Validator is not under Validators/.
- Synchronous validation inside async flow.

### Logic gaps

- No maximum file count/size or signature validation.
- Null Documents, null list items, or null File can throw.
- Enum value other passes validation but is unsupported.
- Accepts spoofable ContentType and original extension.
- Does not enforce mandatory document sets by role.
- Blocks only pending documents, allowing duplicate approved/rejected/current records.
- Does not transition previous IsCurrent records.
- No unique database constraint prevents racing submissions.
- Partial or all-file failure still returns Success = true.
- Provider exception messages are returned to callers.
- Upload failures after storage succeeds can orphan objects.
- Database compensation is incomplete.
- Storage and database operations are not one consistent application workflow.

### Performance

- Entire files are buffered in memory.
- Cancellation is not passed to the pending-type query or SaveChangesAsync.

## UserVerification / GetUserVerificationDocuments

**Status: Broken / critical**

- Anonymous caller can retrieve any user's document metadata.
- Accepts route UserId instead of deriving current user.
- Route does not match either documented verification contract.
- Performs a separate user lookup.
- Read projection correctly uses AsNoTracking, but cancellation is not propagated.
- DocumentId is actually StoredFileId.
- No owner/admin role policy and no signed download flow.

## UserVerification / DeleteVerificationDocument

**Status: Broken / explicitly unfinished**

- Anonymous IDOR allows deletion of another user's storage object and database row.
- Verified-document deletion contains only an unfinished comment.
- Deletes the storage object before committing the database change.
- Deleting both the dependent verification row and cascade principal is redundant and fragile.
- No lifecycle/status transition, audit trail, admin notification, or replacement rule.
- Hard deletion ignores IsDeleted.
- Storage errors return failure objects rather than following exception policy.

## Test Slice

**Status: Unsafe; must not ship**

- Does not follow VSA.
- Several responses are not ApiResponse.
- Contains controller-level provider orchestration.
- Exposes migration errors.
- Enables anonymous email/SMS abuse.
- Hard-coded destination and deployment/version messages are production artifacts.

## Provider and Infrastructure Layer

**Status: Partially Implemented**

### Positive findings

- Feature services generally depend on IEmailProvider, ISmsProvider, IJwtProvider, or IFileStorageService.
- No feature service directly instantiates an external SDK.
- No AutoMapper usage exists.

### Gaps

- Supabase initialization blocks synchronously.
- File provider buffers whole files and lacks size/content enforcement.
- ExistsAsync lists a full directory and can dereference a null response.
- Background email/SMS returns success when merely enqueued.
- Email and SMS jobs lack cancellation/retry/idempotency contracts.
- SMTP and Supabase options have no startup validation.
- Development registers real SMTP rather than the existing mock sender.
- Serilog is installed but not configured.
- CORS, rate limiting, HSTS, and structured production health checks are absent.

## Contracted but Absent Slices

**Status: Unimplemented**

### Sprint 1 gaps

- Generic file upload/download/delete slice.
- Canonical current-user profile endpoints.
- Legal categories and lawyer specializations.
- Lawyer verification status/resubmission.
- ICurrentUserService.
- Notification preferences and legal-category seed data.
- Admin verification review, if Sprint 2 is in current scope.

### Unified OpenAPI gaps

- Cases and AI analysis/matching.
- Marketplace.
- Proposals.
- Chat.
- Contracts and milestones.
- Payments/escrow/webhook.
- Reviews.
- Disputes.
- Notifications.
- Articles.
- AI assistant.
- Admin dashboard and user management.

# Part 3: Remediation Plan

## R-01: Credential and privileged-account incident response

**Proposed solution:** Immediately rotate every committed database, Supabase, SMTP, and JWT credential. Remove secrets from tracked configuration and Git history; use environment variables or a secret manager. Remove unconditional administrator seeding and provision initial administrators through a controlled deployment process.

**Acceptance criteria:**

- All exposed credentials are revoked and replaced.
- Repository and rewritten history contain no live secrets.
- CI secret scanning passes.
- Production startup never creates default or personal administrator accounts.
- Existing forged-JWT risk is neutralized by rotating the signing key.

## R-02: Lock down operational surfaces

**Proposed solution:** Remove Test endpoints from production; protect Hangfire with an Admin authorization filter and network restrictions. Return safe generic 500 messages and log details server-side.

**Acceptance criteria:**

- Anonymous users cannot access Hangfire, migration details, email, or SMS test operations.
- Production error bodies contain no stack trace, SQL detail, path, key, or provider exception.
- Development-only diagnostics are gated by environment and authorization.
- Cancellation and response-started cases are handled safely.

## R-03: Correct startup and hosting pipeline

**Proposed solution:** Migrate once, then seed once, fully asynchronously. Use IdentityRole&lt;Guid&gt; consistently. Fail startup when migration, configuration, or required seed operations fail.

**Acceptance criteria:**

- A blank database starts successfully and receives migrations before seeds.
- Existing database startup succeeds.
- No Result, Wait, or GetAwaiter().GetResult remains.
- Role/user creation results are checked.
- Missing JWT, storage, email, or database configuration fails validation before serving requests.

## R-04: Restore VSA and remove CQRS/MediatR

**Proposed solution:** Replace UserVerification commands/queries/handlers with IUserVerificationService and UserVerificationService; keep request/response DTOs and validators in standard folders. Remove MediatR and AutoMapper packages.

**Acceptance criteria:**

- Every endpoint slice has Controller, interface, service, DTOs, and Validators.
- Controllers depend only on service interfaces.
- No IMediator, IRequest, command/query handler, or MediatR package remains.
- All entity-to-DTO mapping occurs in services.

## R-05: Normalize responses, validation, and exceptions

**Proposed solution:** Configure automatic validation, auth challenge/forbid, 404, and exception responses to use the standard envelope. Services should return data and throw typed/domain exceptions; controllers should not inspect Success.

**Acceptance criteria:**

- Every controller response, including validation and auth failures, uses ApiResponse.
- No controller returns raw strings, anonymous error objects, or manually dispatches service status codes.
- Domain conflicts produce 409; invalid credentials/tokens produce 401; forbidden access produces 403.
- Malformed Base64/token input yields controlled 400 responses.
- No service returns ApiResponse.Fail.

## R-06: Complete registration and account-state flows

**Proposed solution:** Make registration transactional at the application level: create the user, role membership, appropriate profile, notification preferences, and verification state; compensate if a later Identity/provider operation fails. Decide and enforce the canonical lawyer document flow.

**Acceptance criteria:**

- Client registration creates ClientProfile.
- Lawyer registration creates LawyerProfile with correct defaults.
- Role-assignment and email-enqueue failures cannot leave silently incomplete accounts.
- Duplicate identity fields return 409.
- Email confirmation performs the approved status transition.
- Login and refresh reject every prohibited status.
- Registration endpoints match the chosen JSON/multipart contract.

## R-07: Harden authentication sessions

**Proposed solution:** Hash refresh tokens, introduce token/session families and concurrency protection, add indexes and retention cleanup, re-check account state on refresh, and revoke sessions after password reset/change.

**Acceptance criteria:**

- Raw refresh tokens are never stored.
- A refresh token can be consumed exactly once under concurrent requests.
- Replay revokes the affected session family according to documented policy.
- Issuer, audience, signature, algorithm, and token type are validated.
- Suspended/rejected/unconfirmed accounts cannot refresh.
- Password reset/change revokes required sessions.
- One configurable lifetime is shared by code and documentation.
- Historical tokens are pruned.

## R-08: Fix email and password recovery

**Proposed solution:** Reuse a single URL-generation/template service, point links at the correct frontend/API contract, and apply per-email/IP rate limits without revealing account state.

**Acceptance criteria:**

- Resent confirmation links contain the identifier expected by confirmation.
- Confirmation route/method matches OpenAPI.
- Forgot-password returns indistinguishable responses for missing, unconfirmed, and confirmed accounts.
- Resend and forgot-password enforce 3/hour or the approved limit.
- Reset tokens use the approved lifetime.
- Cancellation flows through file and provider calls.

## R-09: Make profile updates atomic and privacy-safe

**Proposed solution:** Adopt the canonical current-user routes, derive identity from ICurrentUserService, separate private and public DTOs, define verified email-change behavior, and replace hard account deletion with the approved lifecycle.

**Acceptance criteria:**

- Services never accept a user ID for self-service operations unless an explicit admin route requires it.
- Email/username/profile changes either all commit or none do.
- Email changes trigger the approved confirmation workflow.
- Public profile DTOs exclude national number, birth date, private address, email, and verification assets.
- Null dates remain null.
- Lawyer experience accepts 0 and enforces an upper bound.
- Phone validation is consistent.
- Delete follows documented retention/soft-delete rules.

## R-10: Redesign verification around one canonical contract

**Proposed solution:** Choose one application model and implement self-service lawyer submission plus role-restricted admin review. Derive user ID from claims, validate ownership, required asset sets, state transitions, and concurrency.

**Acceptance criteria:**

- Submission/status/delete routes require correct Lawyer/Admin policies.
- No request can select another user for a self-service operation.
- Required National ID and Bar Card assets are enforced.
- Pending/approved/rejected/resubmission state transitions are explicit and tested.
- Admin approval/rejection records reviewer, timestamp, and reason.
- Completing both required approvals changes lawyer/account eligibility atomically.
- At most one current document/application per user/type is enforced by database constraints.
- The unfinished verified-delete branch is removed or fully implemented.
- Notifications are sent to admins/lawyers as contracted.

## R-11: Harden file storage

**Proposed solution:** Implement a dedicated file slice and provider contract with configurable limits, signature-based type validation, ownership metadata, private storage, and safe download authorization.

**Acceptance criteria:**

- Request, per-file, and file-count limits are enforced before buffering.
- Upload streams without copying the entire file twice.
- MIME type and magic bytes agree.
- Paths are server-generated and cannot traverse directories.
- Stored files have owners/purpose and cannot be referenced cross-account.
- Verification documents are private and served by short-lived signed URLs or an authorized download endpoint.
- Storage/database compensation is idempotent and orphan cleanup is tested.

## R-12: Repair persistence and audit design

**Proposed solution:** Move all entity mapping to Fluent API; remove duplicate IDs and annotations; add constraints/indexes; implement real current-user auditing and an intentional soft-delete strategy.

**Acceptance criteria:**

- No EF/Data Annotation attributes remain on entities.
- Every entity has an explicit configuration.
- Compiler nullable and member-hiding warnings are eliminated.
- Refresh token, verification status/type/currentness, stored-file lengths, and reviewer relationships are constrained/indexed.
- VerifiedByAdminId uses the correct Guid foreign key.
- CreatedBy/LastModifiedBy use the authenticated identity or a defined system actor.
- Sync and async saves follow the same auditing policy.
- Soft-deleted data is filtered consistently.

## R-13: Performance and cancellation pass

**Proposed solution:** Consolidate profile reads, bound token/document collections, pass cancellation tokens through every asynchronous operation, and use no-tracking projections for reads.

**Acceptance criteria:**

- Profile GET completes in one intentional query or a documented minimal query count.
- Read-only entity queries use AsNoTracking where applicable.
- All EF, file, provider, and template operations accept request cancellation.
- No unbounded refresh/document history is loaded.
- Load tests verify upload memory and authentication latency limits.

## R-14: Define scope and add a quality gate

**Proposed solution:** Mark each OpenAPI/backlog item as current, deferred, or removed, then add unit, integration, authorization, persistence, and contract tests before implementing remaining modules.

**Acceptance criteria:**

- One canonical API and schema specification exists.
- Every exposed action is represented in OpenAPI; no production test route is undocumented.
- Every current-scope OpenAPI operation has an implementation and tests.
- Integration tests cover anonymous, wrong-role, owner, cross-owner, and admin access.
- Auth tests cover refresh replay/concurrency, suspension, password reset, and email confirmation.
- Verification tests cover upload compensation and every state transition.
- CI treats warnings as errors and runs tests, secret scanning, formatting, and dependency checks.

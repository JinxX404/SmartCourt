# Smart Court Comprehensive End-to-End Code Review - V3

**Review date:** 2026-07-31  
**Reviewed baseline:** `main` at `2e12850`  
**Scope:** Current API implementation, feature slices, authorization boundaries,
persistence model, migrations, providers, background processing, deployment,
tests, prior reviews, and current sprint commitments.

## Executive Assessment

**Overall status: Not release-ready, but materially improved.**

The project has advanced significantly since the 2026-07-18 reviews. Secrets
have been replaced with placeholders, exception responses are sanitized, JWT
requests revalidate account state and security stamps, refresh tokens are
hashed, EF migrations are synchronized, and proposals, text chat, contracts,
milestones, escrow, wallets, and withdrawals now have substantial
implementations and tests.

The current release is still blocked by four security defects and several
end-to-end operational gaps:

- User verification remains an anonymous cross-user IDOR surface.
- AI document review and law-corpus administration are publicly accessible.
- Production startup seeds predictable privileged accounts in every
  environment.
- Simplified registration conflicts with the required, unique
  `NationalNumber` database column.
- The outbox and reconciliation pipeline is implemented but never started, so
  milestone scheduling and contract chat events do not run automatically.
- Production deployment silently inherits the mock payment provider and its
  local webhook secret from base settings.
- The public Cases slice is absent, so a client cannot reach the
  case-to-proposal-to-chat workflow through the API.

The recommendation is to treat the next work as a release-hardening sprint,
not as a feature-expansion sprint.

### Verification Performed

- Fast-forwarded local `main` from GitHub to `2e12850` before review.
- `dotnet build SmartCourt.sln --no-restore`: **passed**, 0 errors and 78
  warnings.
- `dotnet test SmartCourt.Tests/SmartCourt.Tests.csproj --no-restore
  --no-build`: **469 passed, 54 failed, 523 total**. The failures were caused by
  local SQL Server encryption support and denied writes to the user-level ASP.NET
  Data Protection key directory; no failing business assertion was observed.
- Focused Admin, Proposals, and Chat tests: **23/23 passed**.
- `dotnet ef migrations has-pending-model-changes`: **no pending changes**.
- NuGet advisory scan: one high-severity transitive advisory through the unused
  AutoMapper package (`AutoMapper` 12.0.1, GHSA-rvv3-g6hj-g44x).
- Static inventory: approximately 476 application C# files, 62 test files, 24
  API controllers, and 16 MediatR handlers.

## Severity Model

- **P0:** Security exposure or a defect that blocks the primary production flow.
- **P1:** Major reliability, deployment, privacy, or integration defect.
- **P2:** Maintainability, consistency, or incomplete non-core capability.

# Part 1: Global Findings

## P0 - Release Blockers

### G-01 - Anonymous IDOR across user-verification operations

`UserVerificationController` has no `[Authorize]` attribute and accepts
caller-controlled user identifiers for upload, read, and delete operations
(`SmartCourt/Features/UserVerification/UserVerificationController.cs:13`,
`SubmitVerificationDocumentsCommand.cs:9`,
`GetUserVerificationDocumentsQuery.cs:9`, and
`DeleteVerificationDocumentCommand.cs:8`).

An anonymous caller can operate on another account by supplying its ID. The
submission handler trusts that ID for user lookup, storage paths, and database
ownership (`SubmitVerificationDocumentsHandler.cs:50` and `:144`).

**Impact:** Unauthorized access to identity documents, document deletion, and
fraudulent submission under another user.

**Required action:** Require the Lawyer role, remove public `UserId` fields, and
derive ownership from `ICurrentUserService`. Keep explicit cross-user access
only under separate Admin routes.

### G-02 - Public AI and law-corpus administrative surfaces

Both document-review controllers explicitly use `[AllowAnonymous]`
(`ReviewDocumentController.cs:13` and `AskLawController.cs:13`). The law
ingestion controller has its Admin authorization commented out
(`LawIngestionController.cs:14`).

Anonymous users can therefore:

- Consume paid or rate-limited model, embedding, vector-store, storage, and
  compute resources.
- Upload documents into the legal corpus.
- List ingestion metadata and errors.
- Delete corpus records, vectors, and stored source files.

The ingestion validator also has no file size or type rule
(`IngestLawDocumentRequestValidator.cs:10`), and the service embeds the original
filename into the storage path (`LawIngestionService.cs:58`).

**Required action:** Restore Admin authorization for ingestion and define the
intended role/policy for document review and Ask Law. Add feature-specific rate
limits, upload limits, signature checks, and abuse telemetry.

### G-03 - Predictable privileged accounts are seeded in every environment

`Program` runs `DatabaseSeeder` unconditionally (`Program.cs:51`). The seeder
creates multiple known accounts with passwords stored directly in source,
including Admin users (`DatabaseSeeder.cs:43`, `:56`, and `:78`). No environment
or first-run secret gate exists.

**Impact:** A fresh production database receives publicly knowable privileged
credentials. This is an immediate account-takeover path unless every seeded
password is changed before exposure.

**Required action:** Seed roles and taxonomy only. Provision the first Admin
through a deployment secret or a one-time bootstrap process, require password
rotation, and never seed test users in Production.

### G-04 - Simplified registration conflicts with the persisted user schema

Client and lawyer registration now accept only name, email, password, and
confirmation (`RegisterClientRequest.cs:3` and `RegisterLawyerRequest.cs:3`).
Neither service assigns `NationalNumber` (`RegisterClientService.cs:46` and
`RegisterLawyerService.cs:39`).

However, `ApplicationUser.NationalNumber` defaults to an empty string
(`ApplicationUser.cs:9`), EF requires the column, and the database has a unique
index (`UserConfiguration.cs:31` and `:57`; model snapshot `:77` and `:125`).

The first simplified registration can persist an empty value. Later
registrations then collide on the unique index and surface as an internal
database error. The latest registration change included no migration and no
registration tests.

**Required action:** Choose one coherent design:

- Make `NationalNumber` nullable with a filtered unique index until profile
  completion; or
- Keep it mandatory and collect/validate it during registration.

Add relational tests for multiple client and lawyer registrations and profile
completion races.

### G-05 - The outbox and reconciliation runtime is never started

Domain services write durable outbox records, and handlers exist for milestone
scheduling and chat system messages. `HangfireContractJobScheduler` exposes
outbox and reconciliation scheduling methods
(`HangfireContractJobScheduler.cs:85` and `:107`).

There is no call site that starts either method. `Program` maps endpoints,
migrates, seeds, and runs without scheduling recurring dispatch
(`Program.cs:42`). Repository search finds only scheduler definitions. The
contract/payment implementation plan records the same blocker at lines 69-73.

Consequences include:

- Outbox rows remain pending indefinitely.
- Milestone auto-acceptance and hold-release jobs are not created.
- Contract/milestone/payment system messages are not added to chat.
- Provider and wallet reconciliation do not recover without manual calls.

**Required action:** Start supervised recurring Hangfire work for outbox
dispatch and each reconciliation loop. Add startup idempotency, monitoring,
retry/dead-letter policy, and a hosted end-to-end timing test.

### G-06 - Production silently inherits the mock payment provider

DI registers `IPaymentProvider` only when
`PaymentProvider:UseMockProvider` is true (`DependencyInjection.cs:206`). When
false or absent, no real provider is registered. Payment, wallet, escrow release,
and termination services all depend on that interface.

The deployment workflow generates Production settings for database, Supabase,
JWT, SMTP, AuthEmail, and Twilio, but no `PaymentProvider` section
(`.github/workflows/deploy.yml:27`). ASP.NET therefore inherits
`PaymentProvider:UseMockProvider=true` from base settings
(`SmartCourt/appsettings.json:5`) together with the local mock webhook secret
default (`PaymentProviderOptions.cs:9`).

**Impact:** Production quietly presents simulated payments as the active
provider, and anyone who knows the source-controlled mock secret can forge mock
webhook signatures. Disabling the mock without adding a real provider instead
breaks payment-service resolution.

**Required action:** Register and validate a real production provider, or add an
explicit demo-only production flag and secret with a startup warning that cannot
be missed. CI should resolve every payment-facing controller from the Production
service provider before deployment.

### G-07 - The case-to-proposal flow is not reachable through the public API

The Cases feature contains only entities, status enums, and a contract-access
integration service. There is no `CasesController`, create/update/submit flow,
query surface, or case attachment endpoint.

Proposal creation requires an owned case already in Submitted, Analyzed, or
Finalized state (`CreateProposalHandler.cs:26` and `:54`). Therefore a frontend
cannot create the required upstream state through the current API.

**Impact:** Proposal and chat endpoints work in focused tests but not as a full
user journey from a clean database.

**Required action:** Implement the Cases vertical slice before presenting
proposal/chat as end-to-end complete. Cover draft, update, submit, ownership,
listing, details, and legal-category validation.

## P1 - Major Reliability and Integration Findings

### G-08 - Contract file authorization uses verification documents as storage

`ContractFileAccessService.AuthorizeForUseAsync` receives a purpose and related
entity ID but authorizes files only by querying `UserVerificationDocuments`
(`ContractFileAccessService.cs:14` and `:28`). `purpose` and `relatedEntityId`
are validated but never used to prove contract, milestone, or dispute access.

This means milestone evidence is effectively limited to identity-verification
files, while participant/moderator access to genuine contract files is not
modeled.

**Required action:** Add a real owned-file record or attachment aggregate with
uploader, purpose, related entity, lifecycle, and access policy. Verification
documents must remain a separate privacy boundary.

### G-09 - Competing proposal acceptance can become an unhandled 500

The database correctly enforces one accepted proposal per case
(`ProposalConfiguration.cs:51`). However, accepting one proposal leaves every
other pending proposal unchanged (`AcceptProposalHandler.cs:45`). A second
lawyer can still attempt acceptance; the handler does not check the case's
Matched state or reject/cancel competing proposals before saving.

The resulting unique-index exception is not mapped to a business conflict, and
no test covers this race or stale inbox state.

**Required action:** In one transaction, accept the winner and close competing
pending proposals, or return a deterministic 409 after locking/rechecking the
case. Add concurrent acceptance tests against SQL Server.

### G-10 - Deployment runs migrations and deploys without a test gate

The only workflow restores, builds, publishes, and deploys
(`.github/workflows/deploy.yml:21`). It never runs tests, migration checks,
Production DI validation, or a smoke test.

At runtime, `UseAutoMigration` catches every migration failure and stores the
full exception in AppDomain state instead of failing fast
(`ApplicationBuilderExtensions.cs:11`). Startup then runs synchronous seeding
through `GetAwaiter().GetResult()` (`Program.cs:54`).

**Required action:** Add CI test and migration-validation jobs before deploy.
Run production migrations as a controlled deployment step, fail on error, and
use fully asynchronous startup initialization.

### G-11 - Cross-origin frontend integration is not configured

There is no `AddCors` or `UseCors` call in the application. The frontend is not
hosted by this backend repository, so a browser frontend on another origin
cannot call REST or SignalR unless an external reverse proxy makes both origins
identical.

**Required action:** Define environment-specific allowed origins and credentials
behavior, place CORS correctly before authentication/endpoint execution, and
test SignalR preflight and WebSocket negotiation from the deployed frontend
origin.

### G-12 - A high-severity vulnerable, unused package remains referenced

The application references
`AutoMapper.Extensions.Microsoft.DependencyInjection` 12.0.1
(`SmartCourt.csproj:10`) but contains no AutoMapper usage. The NuGet advisory
scan reports transitive `AutoMapper` 12.0.1 as high severity under
`GHSA-rvv3-g6hj-g44x`.

**Required action:** Remove the unused package. If mapping is later standardized,
choose a supported non-vulnerable version and add dependency scanning to CI.

### G-13 - Verification lifecycle and file safety remain incomplete

Beyond the IDOR, submission still has correctness and safety gaps:

- MIME type is trusted from the multipart request; file signatures are not
  inspected (`SubmitVerificationDocumentsHandler.cs:92`).
- No explicit per-file or aggregate request-size rule is applied.
- Provider exception messages are returned to clients (`:186`).
- Multiple current versions are allowed until an Admin review demotes older
  versions (`ReviewVerificationDocumentHandler.cs:87`).
- Submission does not transition the account to PendingReview.
- Storage and database compensation is best-effort and not durable.
- Persistence entities still use Data Annotations and hide `BaseEntity.Id`
  (`UserVerificationDocument.cs:11` and `:13`; `StoredFile.cs:8`).

The Admin review slice itself is substantially better: it has Admin role
protection, status filters, current-version checks, optimistic concurrency, and
clear response DTOs. Its safety is undermined by the public submission side.

### G-14 - Test execution is not portable or green by default

The suite has grown from six tests to 523, which is excellent progress. However,
several integration fixtures hard-code `Server=localhost` and create databases
directly (`OutboxIntegrationTests.cs:228`,
`ContractServiceIntegrationTests.cs:805`, and
`WalletServiceIntegrationTests.cs:348`). Auth tests use the default user-level
Data Protection key store rather than an isolated test provider.

On this machine, 54 tests fail before their assertions because of SQL encryption
support and key-directory permissions. CI currently never runs them.

**Required action:** Provide a disposable SQL Server fixture/container and
inject an ephemeral Data Protection provider. Split unit and integration test
categories while keeping both mandatory in CI.

## P2 - Architecture and Completeness Findings

### G-15 - Feature architecture is inconsistent and core services are oversized

Admin, Proposals, and Chat use MediatR/CQRS, while Contracts, Milestones, and
Payments use controller-to-service orchestration. The repository documentation
also contains contradictory rules about whether CQRS is allowed.

Several services now carry too many responsibilities:

- `PaymentEscrowService.cs`: approximately 1,703 lines.
- `MilestoneService.cs`: approximately 1,379 lines.
- `ContractService.cs`: approximately 975 lines.
- `WalletService.cs`: approximately 629 lines.

**Required action:** Agree one feature-boundary convention. Decompose by use case
or focused domain service without changing business behavior, and keep
transaction ownership explicit.

### G-16 - API validation and response contracts are not globally uniform

Automatic FluentValidation/model-binding failures are not normalized through
`ApiResponse<T>`. JWT challenge/forbid responses have no custom standardized
handlers. Some MediatR handlers return failed `ApiResponse` values while newer
services throw exceptions. Binary document content is a separate raw response.

**Required action:** Define one error contract for controller validation,
authentication, authorization, rate limiting, domain failures, and unexpected
errors. Keep HTTP status decisions at the API boundary.

### G-17 - Tracked runtime diagnostics leak local implementation details

`SmartCourt/startup-error.txt` is tracked and contains a local stack trace and
absolute workstation paths. `Program` overwrites it on startup failure
(`Program.cs:59`).

**Required action:** Remove the file from Git, add it to `.gitignore`, and rely on
structured logging with environment-appropriate sinks.

# Part 2: Feature Readiness

| Feature | Current status | Release readiness |
|---|---|---|
| Auth and sessions | Stronger JWT and refresh security; registration schema is broken and refresh rotation has no relational race test | No |
| Client/lawyer profiles | Implemented basic self-service and public lawyer filtering; profile completion contract remains unsettled | Partial |
| User verification | Public IDOR and lifecycle/file gaps | Critical blocker |
| Admin verification | Clean Admin route, filters, details, content, review, and concurrency behavior | Locally ready after user-side fix |
| Cases | Entity and contract integration only; no public vertical slice | Missing |
| Proposals | Create/list/detail/accept/reject with CQRS and tests; competing acceptance needs closure semantics | Partial |
| Chat | Participant-only text chat, history, list, SignalR, and tests | Core ready; unread, attachments, lifecycle closure, and dispute review missing |
| Contracts | Broad lifecycle and state history implemented | Partial; depends on Cases and runtime jobs |
| Milestones | Negotiation, funding readiness, submission, review, auto-accept service | Partial; timed jobs are not started |
| Payments and escrow | Extensive mock workflow, idempotency, webhook validation, release/refund services | Not production-ready; no production provider or recurring reconciliation |
| Wallets and withdrawals | Implemented with focused tests | Partial; production provider and reconciliation required |
| Disputes | Entities, enums, guards, and configuration only | Missing public/Admin workflow |
| Reviews and ratings | No review/rating entity, service, controller, migration, or tests | Missing |
| Notifications | Integration interface only | Missing runtime implementation |
| AI document review | Functional provider pipeline | Unsafe while anonymous |
| Law ingestion/RAG corpus | Upload, background ingest, status, list, delete | Unsafe while public; file validation incomplete |
| Articles, matching, marketplace, case AI | No complete public slices | Missing |

## Proposal and Chat Flow Assessment

The implemented core flow is coherent once authoritative test data already
exists:

1. A Client creates a proposal for an eligible owned case.
2. The Lawyer accepts it.
3. The proposal becomes Accepted and the case becomes Matched.
4. A one-per-proposal conversation is created immediately.
5. REST returns `ConversationId` and provides conversation/history endpoints.
6. SignalR allows only the Client and Lawyer to join and send text messages.

This is a good foundation and its focused tests pass. It is not yet a complete
frontend journey because Cases has no public API, competing proposals remain
pending, CORS is absent, and the outbox worker does not publish later contract
events into the conversation.

## Dispute and Chat Review Assessment

Admins cannot currently review chat for disputes. This is preferable to broad
unrestricted Admin chat browsing, but the controlled dispute path is still
missing.

The recommended rule remains:

1. A contract participant opens a dispute tied to a contract/milestone.
2. An authorized moderator is assigned.
3. The moderator may read only the proposal and conversation linked to that
   dispute.
4. Access is time-bound, purpose-bound, and audit-logged.
5. Chat messages remain immutable; the moderator cannot impersonate either
   participant or silently alter history.

# Part 3: Previous Review Verification

## Confirmed Fixed or Materially Improved

- Tracked application settings now use placeholders instead of live secrets.
- The public test/diagnostic controller was removed.
- Unexpected API exceptions no longer return `exception.ToString()`.
- JWT validation now checks issuer, audience, account eligibility, and security
  stamp on authenticated requests.
- Refresh tokens are hashed and use a seven-day lifetime.
- Login enables Identity lockout on failed passwords.
- Rate limiting is partitioned and returns `ApiResponse` for 429 responses.
- Client profile creation and public lawyer eligibility filtering were improved.
- Admin verification has authorization, status filtering, replacement handling,
  and optimistic concurrency.
- EF reports no pending model changes.
- Automated coverage expanded substantially, especially for contract/payment,
  proposals, chat, and Admin verification.

## Still Open from Earlier Reviews

- User-verification IDOR.
- Predictable seeded privileged accounts.
- Automatic production migrations and synchronous seeding.
- Verification upload and replacement lifecycle.
- Persistence Data Annotations and hidden base IDs.
- Global response/validation consistency.
- Deploy-without-tests workflow.
- Missing canonical product slices.

# Part 4: Ordered Remediation Plan

## R-01 - Lock Down Privileged and Personal-Data Surfaces

**Scope:** G-01, G-02, G-03.

**Acceptance criteria:**

- Anonymous verification, document-review, and law-ingestion access is rejected.
- Users cannot supply another user's ID for verification operations.
- No source-controlled password provisions a Production account.
- Admin and AI operations have explicit policies, rate limits, and audit logs.

## R-02 - Repair Registration and Profile Completion

**Scope:** G-04.

**Acceptance criteria:**

- Multiple clients and lawyers can register in a clean relational database.
- National-number nullability and uniqueness match the chosen product flow.
- Profile completion is explicit, validated, and race-safe.
- Role-assignment and email-enqueue failures cannot leave hidden partial users.

## R-03 - Start the Runtime Reliability Pipeline

**Scope:** G-05 and contract/payment plan blockers.

**Acceptance criteria:**

- Outbox dispatch runs automatically and continuously.
- Missing schedules, unknown provider outcomes, expired holds, and wallet
  projections reconcile automatically.
- Jobs are idempotent, observable, and recover after restart.
- Timed milestone and chat-event integration tests pass against hosted services.

## R-04 - Make Payment Deployment Explicit

**Scope:** G-06.

**Acceptance criteria:**

- Production DI resolves every payment-facing controller and service.
- A real provider or explicitly approved demo provider is configured with a
  rotated webhook secret.
- Production startup fails clearly for an invalid provider configuration.

## R-05 - Complete Cases and Close Proposal Races

**Scope:** G-07 and G-09.

**Acceptance criteria:**

- A Client can create, edit, submit, list, and view owned cases through the API.
- A full hosted test reaches proposal creation from a newly registered Client.
- Exactly one proposal wins per case; competitors receive deterministic closure
  states and no acceptance path returns a raw database exception.

## R-06 - Replace Verification Files as Contract Attachments

**Scope:** G-08 and G-13.

**Acceptance criteria:**

- Every stored file has explicit owner, purpose, and related-entity metadata.
- Contract, milestone, dispute, and verification access policies are separate.
- Uploads enforce size, count, signature, type, and durable compensation rules.

## R-07 - Establish a Real Release Gate

**Scope:** G-10, G-11, G-12, and G-14.

**Acceptance criteria:**

- CI runs build, unit tests, SQL integration tests, migration checks, dependency
  advisories, Production DI validation, and a post-deploy smoke test.
- The test suite is green in a documented clean environment.
- CORS and SignalR negotiation work from the actual frontend origin.
- No known high-severity vulnerable package remains.

## R-08 - Implement Disputes Before Admin Chat Review

**Scope:** Missing Disputes, Reviews, and controlled evidence access.

**Acceptance criteria:**

- Participants can open a dispute only in valid contract/milestone states.
- Moderator assignment, evidence, chat review, resolution, settlement, and audit
  history are implemented and tested.
- Admin chat access is impossible without an active authorized dispute.
- Review/rating submission is allowed only after eligible contract completion.

## R-09 - Consolidate Architecture and API Contracts

**Scope:** G-15 through G-17.

**Acceptance criteria:**

- The team documents one CQRS/service convention per feature boundary.
- Large financial services are decomposed without weakening transaction safety.
- Every API failure uses one documented response shape.
- Generated runtime diagnostics are not tracked in Git.

## Final Recommendation

Do not merge additional broad feature work into `main` until R-01 through R-05
are resolved. The most valuable next sequence is:

1. Access-control and seeder lockdown.
2. Registration/schema repair.
3. Outbox/reconciliation worker startup.
4. Production payment-provider configuration.
5. Cases API and proposal race closure.
6. CI, portable integration tests, and CORS.
7. Disputes with controlled chat review.

After those items, the existing proposal, chat, contract, milestone, and payment
work becomes a credible end-to-end MVP foundation instead of a set of strong but
partially disconnected slices.

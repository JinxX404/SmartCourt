# Contracts and Payments — End-to-End Implementation Plan

## 1. Purpose and source of truth

This plan turns the v1 Contracts and Payments design into an ordered implementation backlog for the Smart Court API. It is based on:

- `01_architecture_and_integrations.md`
- `02_database_schema.md`
- `03_state_machines.md`
- `04_api_contracts_and_dtos.md`
- `05_core_services_and_escrow.md`
- `06_dispute_resolution_flow.md`
- `contracts-and-payments-spec.md`
- `design_decisions_summary.md`

The implementation is complete only when the full lifecycle works: accepted proposal → contract draft → mutual acceptance → active contract → sequential milestone approval → milestone-specific funding → escrow hold → funded submission → manual approval or seven-day auto-acceptance → fourteen-day hold → release, refund, or dispute settlement → wallet availability/withdrawal → contract completion or termination.

## 1.1 Current implementation status and remaining work

Status recorded after Phase 9 was merged into `main` on 2026-07-30.
The last completed verification before the merge was a clean build and 517
passing tests. This does **not** mean the feature is end-to-end complete.

### Phase tracker

- [x] Phase 0.1 and 0.3: baseline decisions and architecture enforcement.
- [ ] Phase 0.2: dependency gates are only partially complete. Proposals,
  Cases, Users, and basic Files implementations now exist, but Chat and
  Notifications still have interfaces only. Refresh
  `phase_0_dependency_gates.md`, which still describes older repository state.
- [x] Phase 1: domain entities, enums, transition guards, funding verifier,
  settlement calculator, and history foundations.
- [ ] Phase 2: schema, Fluent API, DbContext, and migrations exist, but the
  complete migration chain still needs disposable SQL Server apply/rollback
  verification and database-metadata inspection in a repeatable container or
  CI environment.
- [ ] Phase 3: provider contracts, mock provider, idempotency, outbox storage,
  dispatcher, and Hangfire adapters exist. Runtime worker wiring and generic
  provider retry/reconciliation remain incomplete.
- [x] Phase 4: Contracts slice and public contract endpoints.
- [x] Phase 5: milestone negotiation, approval, sequencing, change requests,
  and public milestone endpoints.
- [ ] Phase 6: funding, webhook, payment queries, and manual retry are
  implemented, but automatic processing/retry/reconciliation is not fully
  operational at runtime.
- [ ] Phase 7: submission, review, auto-acceptance, and hold-release business
  services exist, but they depend on outbox/job execution that is not started
  automatically by the application.
- [ ] Phase 8: intentionally skipped. Only dispute persistence entities,
  enums, configurations, and transition foundations exist; the complete
  Disputes vertical slice is missing.
- [ ] Phase 9: termination, completion evaluation, wallet, and withdrawal
  code exists, but the integration gaps below must be completed.
- [ ] Phase 10: integration consumers, notifications, chat messages, and full
  privacy/file authorization are not complete.
- [ ] Phase 11: DI is partially registered; error/OpenAPI completion remains.
- [ ] Phase 12: focused tests exist, but the required relational, race, and
  end-to-end API matrix is incomplete.
- [ ] Phase 13: observability, security review, deployment, and rollback work
  has not been completed.

### Functional blockers before the feature is fully usable

- [ ] Implement Phase 8 completely: dispute DTOs, separate validators,
  `IDisputeService`/`DisputeService`, participant and moderator controllers,
  evidence authorization, assignment/investigation, immutable full
  refund/full release/partial split settlement, closing, penalties, tests,
  DI, and jobs.
- [ ] Finish and supervise the runtime background pipeline.
  - [x] Register recurring Hangfire work at startup for outbox dispatch,
    missing-schedule reconciliation, and pending withdrawal reconciliation.
  - [ ] Add provider-specific pending-transaction scanning/reconciliation and
    automatic retry for deposit, release, refund, and termination operations.
    The recurring registration does not claim this work is complete.
- [x] Replace the throwing
  `PaymentContractJobOperations.RetryProviderTransactionAsync` placeholder.
  Scheduled retries now use the existing idempotent provider reconciliation
  path for processing deposit transactions; confirmed failures continue to
  require the finance-authorized manual retry endpoint.
- [ ] Extend provider reconciliation beyond deposits. Unknown release,
  termination refund, and withdrawal outcomes need explicit safe status
  reconciliation/retry paths. Withdrawal reconciliation currently replays
  `WithdrawAsync` without the original destination reference, so it is only
  reliable with the deterministic mock behavior.
- [ ] Wire contract completion evaluation into every terminal
  release/refund/cancellation/dispute-resolution path. The evaluation method
  exists, but no settlement service calls it, so a normal contract does not
  automatically become `Completed`. Make the internal completion evaluator
  usable by jobs without requiring a participant from the current HTTP user.
- [ ] Finish termination recovery. Unknown/failed termination refunds must be
  automatically reconciled or retried and then resume termination without
  requiring the participant to repeatedly call the endpoint.
- [ ] Add the real production payment provider. DI currently registers
  `IPaymentProvider` only when `PaymentProvider:UseMockProvider` is enabled;
  disabling the mock leaves payment-dependent services unresolved. Add the
  real provider, its configuration validation, webhook verification, and a
  production policy that cannot silently treat the mock as regulated escrow.
- [ ] Implement and register Chat and Notification owning-slice services plus
  deduplicating outbox consumers for all Phase 10 events. Also add the Case
  consumer for termination/completion lifecycle updates.
- [ ] Complete file privacy authorization. The current file integration
  checks ownership through verification-document storage, but does not use
  `ContractFilePurpose` and `relatedEntityId` to prove contract participant or
  moderator access to contract attachments, submissions, and dispute
  evidence.
- [ ] Implement audited compensating wallet/ledger adjustments for authorized
  administrators. Phase 9.3 currently covers normal wallet projection and
  withdrawal only.
- [ ] Add handlers for every required event. Domain services write many
  outbox records, but the only registered feature event consumer currently
  schedules milestone auto-acceptance and hold release.

### Release, verification, and documentation checklist

- [ ] Complete Phase 11 error mapping, including wrapped concurrency conflicts
  and exceptional provider `502` behavior without local controller catches.
- [ ] Verify and publish OpenAPI for every implemented route, wrapper, role,
  `Idempotency-Key`, `If-Match`, webhook header, pagination shape, and error
  code. Reconcile the state-history response shape with the documented public
  contract.
- [ ] Add repeatable SQL Server container/CI setup. Several core service tests
  still use EF InMemory even where transaction, rowversion, filtered-index,
  locking, or constraint behavior matters.
- [ ] Complete the Phase 12 relational and race matrix, especially funding
  races, accept-vs-change, manual-vs-auto acceptance, release-vs-dispute,
  duplicate dispute resolution, termination-vs-callback, and settlement
  retries.
- [ ] Add authenticated end-to-end API journeys covering the complete
  proposal-to-withdrawal lifecycle and every dispute outcome. Current
  controller tests call controllers directly and are not full hosted API
  journeys.
- [ ] Complete Phase 13 structured logs/metrics/alerts, privacy and IDOR
  review, role matrix, mock-provider production guard, feature flags,
  deployment runbook, and forward-only financial rollback procedure.
- [ ] Run the exact workflow trace in section 5 against the hosted application
  and a disposable SQL Server before declaring the feature complete.

## 2. Non-negotiable implementation rules

Every task in this plan must follow these rules:

1. **Vertical Slice Architecture only.** Put each controller, service interface, service implementation, DTO, and validator under its owning slice:

   ```text
   SmartCourt/Features/Contracts/
   SmartCourt/Features/Milestones/
   SmartCourt/Features/Payments/
   SmartCourt/Features/Disputes/
   ```

   Use `ContractsController` + `IContractService` + `ContractService`, and equivalent slice-local components. Do not implement this feature with CQRS, command/query handlers, or MediatR. Existing MediatR usage elsewhere in the repository does not authorize its use here.

2. **Cross-feature communication is service-to-service.** When Contracts and Payments needs Cases, Proposals, Chat, Files, Notifications, Users, or another feature, inject that feature's service interface. Never inject or call another controller, and do not duplicate the other feature's business rules in this feature.
3. **Standard responses are mandatory.** Every controller action returns the existing `ApiResponse<T>`/`ApiResponse` wrapper, using helpers such as `ApiResponse<T>.Ok(data)` and `ApiResponse<T>.Created(data)`. Do not return unwrapped DTOs.
4. **Business failures are exceptions.** Services throw `BusinessException("Message")` for domain/business-rule failures and let the global exception middleware format the response. Controllers and services must not catch an exception merely to return an explicit 500 response.
5. **FluentValidation only.** Put validators in each slice's `Validators/` directory. Do not add Data Annotations to request DTOs, response DTOs, or entities.
6. **Manual mapping only.** Map entities to DTOs explicitly inside the owning service class or private service mapping helpers. Do not use AutoMapper, even though the package currently exists in the project.
7. **Provider Pattern for integrations.** External dependency interfaces belong under `SmartCourt/Infrastructure/Providers/`, grouped by capability. Implementations belong under `SmartCourt/Providers/`. Feature services depend on provider interfaces and must never reference external SDK types.
8. **EF Core with Fluent API only.** Persist with `ApplicationDbContext`; configure entities through `IEntityTypeConfiguration<T>` in `SmartCourt/Persistence/Configurations/` (or a slice-local configuration folder if the project standard is deliberately changed). Do not use persistence Data Annotations.
9. **Async all the way.** Every I/O path uses `async`/`await` and accepts a `CancellationToken`. Never use `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` in new or changed feature code, provider code, jobs, seeders, tests, or registration needed by this feature.
10. **Server-owned state.** Request DTOs never accept statuses, funding flags, escrow IDs, timestamps, fees, calculated totals, acceptance source, or row versions as mutable business fields. State changes occur only through explicit service commands.
11. **Milestone-specific money.** A deposit, hold, submission, acceptance, release, refund, or dispute must reconcile to the exact same `MilestoneId`, amount, and EGP currency. A contract/account balance or another milestone's payment can never satisfy this requirement.
12. **Append-only financial and audit history.** Never update or delete ledger entries, state-history rows, submissions, evidence, dispute resolutions, or completed provider attempts. Corrections use compensating entries/attempts.

## 3. Fixed v1 rules and implementation assumptions

- Currency is exactly `EGP`.
- Money is `decimal(18,2)`.
- Store time in UTC; clients display it in `Africa/Cairo`.
- The platform fee is 5% of the non-refunded portion and is deducted from the lawyer's gross allocation.
- Use one shared money calculator. Round fee results to two decimal places with `MidpointRounding.AwayFromZero`, then calculate lawyer net as `gross allocation - fee` so every settlement reconciles exactly.
- A contract is unique per accepted proposal and belongs to one case, one client, and one lawyer.
- At least one mutually approved milestone must exist before contract activation.
- Milestones are funded and executed sequentially. At most one milestone per contract may be processing funding or have an unsettled funded/frozen hold.
- `FundedInProgress` is reachable only after a successful deposit and creation of the exact milestone's funded escrow hold.
- Submissions are immutable and versioned. A submission must store its verified `EscrowHoldId`.
- Client review lasts seven calendar days from a valid funded submission. Manual or automatic acceptance starts a separate fourteen-day escrow hold.
- A request for changes returns the same funded milestone to `FundedInProgress`; it neither refunds nor charges again.
- Time extensions are mutual `MilestoneChangeRequest` records. A funded amount cannot change.
- A dispute is allowed only while the milestone is `AcceptedHold` and before `HoldExpiresAt`.
- v1 resolution outcomes are full refund, full release, and partial split. Penalties are manual.
- Unknown provider outcomes remain processing until webhook/reconciliation resolves them; they must not be treated as confirmed failures.

## 4. Target layout

```text
SmartCourt/
  Common/
    Enums/                         # Exact domain enums used across slices
  Features/
    Contracts/
      ContractsController.cs
      IContractService.cs
      ContractService.cs
      DTOs/
      Validators/
      Events/
    Milestones/
      MilestonesController.cs
      IMilestoneService.cs
      MilestoneService.cs
      DTOs/
      Validators/
      Events/
    Payments/
      PaymentsController.cs
      IPaymentEscrowService.cs
      PaymentEscrowService.cs
      IContractJobService.cs
      ContractJobService.cs
      DTOs/
      Validators/
      Events/
    Disputes/
      DisputesController.cs
      IDisputeService.cs
      DisputeService.cs
      DTOs/
      Validators/
      Events/
  Infrastructure/
    Providers/
      Payments/IPaymentProvider.cs
      Jobs/IContractJobScheduler.cs
      Events/IOutboxDispatcher.cs
  Providers/
    Payments/MockPaymentProvider.cs
    Jobs/HangfireContractJobScheduler.cs
  Entities/                       # Contract/payment persistence entities
  Persistence/
    Configurations/
    ApplicationDbContext.cs
  Migrations/
SmartCourt.Tests/
  Features/
    Contracts/
    Milestones/
    Payments/
    Disputes/
  Integration/
    ContractsAndPayments/
```

If Cases, Proposals, Chat, or Notifications are implemented before this work begins, use their real slice names and service interfaces. Do not create parallel replacement abstractions inside Contracts and Payments.

---

## Phase 0 — Readiness, dependency contracts, and architecture guardrails

### Task 0.1 — Confirm the baseline and freeze public design choices

1. Build the solution and run the current test suite before modifying code.
2. Record the current database provider/version, migration baseline, authentication roles, current-user abstraction, API response behavior, and global exception behavior.
3. Confirm exact role names for Client, Lawyer, Moderator, FinanceAdministrator, and SuperAdministrator.
4. Create a short decision record for:
   - fee rounding;
   - idempotency-key retention/expiry;
   - provider webhook authentication;
   - whether `If-Match` carries a base64 rowversion;
   - pagination conventions;
   - the configured UTC clock abstraction (`TimeProvider` is preferred).
5. Note that the repository's `BaseEntity` contains `IsDeleted`. Financial/audit entities must not inherit a base that enables soft deletion.

Acceptance criteria:

- The unmodified solution's build/test result is recorded.
- Role names and authentication claims are unambiguous.
- No remaining technical decision can change settlement arithmetic, concurrency semantics, or public request shapes during implementation.
- Financial/audit entity designs contain no soft-delete field or filter.

### Task 0.2 — Establish cross-feature dependency gates

1. Inventory the Cases, Proposals, Chat, File Storage, Notifications, and Users slices.
2. For each available slice, identify the service-interface method Contracts and Payments will call.
3. Where a prerequisite slice is absent, create a separately tracked prerequisite in that owning slice rather than querying hypothetical tables from Contract services. Required capabilities are:
   - proposal lookup that proves `Accepted` status and returns proposal/case/client/lawyer IDs;
   - case eligibility/ownership validation;
   - append-only proposal-conversation system messages;
   - participant/moderator file authorization and signed access;
   - notification publishing;
   - user/role/moderator eligibility checks.
4. Define small result contracts containing only the facts this feature needs. Do not expose another slice's EF entities.
5. Provide test fakes for each service interface.

Acceptance criteria:

- Each cross-feature call is represented by an injected service interface, never a controller.
- Contract creation can verify an accepted proposal and case ownership without duplicating Proposal/Case business logic.
- Chat, file, and notification integrations can be substituted with fakes in unit tests.
- The plan is blocked from production release, but not from isolated feature development, until all dependency gates have real implementations.

### Task 0.3 — Add architecture enforcement tests

Add reflection or source-structure tests that fail when:

- a Contracts/Payments feature class references MediatR;
- a feature service references a controller;
- a DTO/entity in these slices has a Data Annotation;
- a feature references AutoMapper;
- an external SDK type appears outside provider implementations;
- controller response actions bypass `ApiResponse<T>`/`ApiResponse`;
- new feature code contains `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()`.

Acceptance criteria:

- Each prohibited pattern has a test that demonstrates a failure.
- The tests pass for the completed implementation.

---

## Phase 1 — Domain model and state-transition foundation

### Task 1.1 — Add exact enums

Implement the exact v1 enums from `03_state_machines.md`:

- `ContractStatus`
- `MilestoneStatus`
- `EscrowHoldStatus`
- `MilestoneFundingStatus`
- `MilestoneAcceptanceSource`
- `DisputeStatus`
- `DisputeResolutionType`
- `ChangeRequestStatus`
- `PenaltyType`

Add the supporting persistence enums needed by the schema, including payment operation/status, ledger transaction type, settlement type, escrow account status, withdrawal status, dispute category/requested outcome, idempotency status, and outbox status. Assign explicit stable numeric values and never reorder them after migration.

Acceptance criteria:

- The documented enum values and numeric ordinals match the specification exactly.
- Supporting enums cover every persisted state without magic strings.
- Invalid persisted enum values are rejected by Fluent API check constraints.

### Task 1.2 — Implement entities and relationships

Create the minimum v1 entities described in `02_database_schema.md`:

- `Contract`, `Milestone`, `MilestoneChangeRequest`
- `MilestoneSubmission`, `MilestoneSubmissionAttachment`
- `ContractAttachment`
- `EscrowAccount`, `EscrowHold`, `EscrowLedgerEntry`
- `PaymentTransaction`
- `LawyerWallet`, `WithdrawalRequest`
- `Dispute`, `DisputeResolution`, `DisputeEvidence`, `LawyerPenalty`
- `ContractStateHistory`, `MilestoneStateHistory`
- `IdempotencyRecord`, `OutboxMessage`

Implementation notes:

1. Use `Guid` keys and UTC `DateTime`.
2. Put rowversion only on mutable aggregate/projection roots.
3. Keep navigation setters controlled enough to prevent arbitrary state mutation by controllers.
4. Do not store `Contract.TotalAmount`; derive it from approved priced milestones.
5. Do not store a client-writable milestone funded flag. Derive `MilestoneFundingStatus`.
6. Do not add soft-delete behavior to financial, dispute, submission, or audit records.
7. Store all milestone acceptance/funding/submission/hold timestamps defined by the schema.

Acceptance criteria:

- Every required schema field exists with correct nullability and CLR type.
- Exactly one escrow account can belong to a contract, and exactly one successful hold can belong to a milestone.
- A submission cannot be constructed without `MilestoneId`, `EscrowHoldId`, submitter, version, notes, and timestamp.
- No entity exposes a public operation that permits an invalid state transition without service validation.

### Task 1.3 — Centralize state and funding invariants

1. Implement slice-internal transition guards for contract, milestone, hold, dispute, and change-request transitions.
2. Implement one reusable EF query/specification that returns a `VerifiedMilestoneFunding` result only when:
   - milestone `FundedAt` is present;
   - exactly one hold belongs to the milestone;
   - hold state is valid for the requested operation;
   - a completed deposit transaction references the same milestone and hold;
   - milestone amount, hold gross, transaction amount, and EGP currency match.
3. Use this same verifier from submission, manual acceptance, auto-acceptance, and dispute opening.
4. Implement a shared settlement calculator returning gross, refund, lawyer gross allocation, fee, and lawyer net.
5. Implement append-only state-history helpers requiring previous/new state, trigger, actor, reason, correlation ID, and UTC timestamp.

Acceptance criteria:

- Unit tests prove a payment for milestone A cannot verify milestone B.
- Missing/multiple holds, missing deposit, non-completed deposit, amount mismatch, currency mismatch, wrong hold status, and missing `FundedAt` all fail verification.
- Full refund, full release, and partial split calculations reconcile exactly to gross.
- Every legal transition is listed; every unlisted transition throws `BusinessException("Message")`.

---

## Phase 2 — EF Core persistence and migration

### Task 2.1 — Add Fluent API configurations

Create one `IEntityTypeConfiguration<T>` per entity under `SmartCourt/Persistence/Configurations/`. Configure:

1. SQL types, Unicode, maximum lengths, enum conversion, UTC expectations, and `.HasPrecision(18, 2)` for every money property.
2. `.IsRowVersion()` for mutable roots/projections.
3. Required relationships with `DeleteBehavior.Restrict`.
4. Cascade deletion only for attachment join rows that can exist solely before financial activity.
5. Unique indexes:
   - `Contract.ProposalId`;
   - `Milestone(ContractId, OrderNumber)`;
   - `EscrowAccount.ContractId`;
   - `EscrowHold.MilestoneId`;
   - `MilestoneSubmission(MilestoneId, Version)`;
   - `LawyerWallet.LawyerUserId`;
   - `PaymentTransaction.IdempotencyKey`;
   - filtered provider transaction ID;
   - `DisputeResolution.DisputeId`;
   - one active dispute per milestone using an appropriate SQL Server filtered index.
6. Query indexes from the schema, including auto-accept, hold-expiry, dispute queue, payment retry, wallet request, and outbox polling indexes.
7. Check constraints for EGP, positive amounts/order/duration, non-negative financial values, valid enum ranges, required completed-deposit references, evidence content/file presence, and resolution reconciliation where a single-row constraint can express it.

Acceptance criteria:

- `ApplicationDbContext` discovers all configurations through assembly scanning.
- No new entity uses Data Annotations.
- Generated SQL uses `decimal(18,2)`, `datetime2`, rowversion, restricted deletes, required unique indexes, and check constraints.
- A relational test proves duplicate proposal contracts, duplicate milestone orders, duplicate milestone holds, duplicate submission versions, and duplicate active disputes are rejected.

### Task 2.2 — Update `ApplicationDbContext`

1. Add a `DbSet<T>` for each entity.
2. Ensure auditing does not modify append-only rows after insertion.
3. Use the injected clock/current user where audit metadata is required; do not hard-code local time.
4. Add a save interceptor or explicit guard that rejects Modified/Deleted states for immutable tables.
5. Ensure financial records are not affected by a global soft-delete query filter.

Acceptance criteria:

- An integration test attempting to modify/delete a ledger, submission, evidence, state history, or dispute resolution fails.
- All persisted timestamps are UTC.
- Existing DbContext behavior and current tests remain green.

### Task 2.3 — Generate and validate the migration

1. Generate one coherent migration for the v1 schema.
2. Review both Up and Down SQL.
3. Apply it to a disposable SQL Server database.
4. Verify constraints and filtered indexes from actual database metadata.
5. Test rollback only in a disposable environment; never delete financial production data.

Acceptance criteria:

- Migration applies from the repository baseline and the application starts.
- Migration rollback succeeds in a disposable database.
- Schema inspection confirms every required table, FK, index, precision, check constraint, and rowversion.

---

## Phase 3 — Provider, idempotency, outbox, and job infrastructure

### Task 3.1 — Define provider contracts

Under `SmartCourt/Infrastructure/Providers/`, add:

- `IPaymentProvider` with async deposit, release, refund, and withdrawal operations;
- provider request/result records that contain amount, EGP currency, business ID, provider idempotency key, correlation ID, and outcome (`Succeeded`, `Failed`, `Unknown`);
- `IContractJobScheduler` for auto-accept, hold release, provider reconciliation/retry, and scheduling reconciliation;
- an outbox-dispatch abstraction if one does not already exist.

No provider contract may reference a controller DTO or external SDK type.

Acceptance criteria:

- Feature services compile against provider interfaces only.
- Provider results distinguish confirmed failure from unknown outcome.
- Every provider operation carries correlation and idempotency data.
- All methods are asynchronous and cancellable.

### Task 3.2 — Implement deterministic mock payment provider

Implement `MockPaymentProvider` under `SmartCourt/Providers/Payments/`:

- `mock-success*` → deterministic success;
- `mock-fail*` → confirmed failure;
- `mock-timeout*` → unknown/timeout;
- equivalent deterministic behavior for release, refund, and withdrawal.

Keep all string-branching inside this provider. Register the interface/implementation through DI and configuration.

Acceptance criteria:

- Feature services contain no mock-reference branching.
- Repeating the same provider idempotency key returns the same logical result.
- Unit tests cover success, failure, unknown, cancellation, and duplicate calls.
- Configuration explicitly warns that the mock is not regulated escrow and cannot be enabled silently in production.

### Task 3.3 — Implement idempotency handling

1. Read `Idempotency-Key` in controllers for all state-changing money operations.
2. Hash the canonical request payload plus operation/user/resource scope.
3. In a transaction, insert/read `IdempotencyRecord`.
4. Return the stored result for the same key/hash.
5. Throw a business conflict for the same key with a different hash.
6. Add unique business settlement keys for each hold release/refund/resolution so two HTTP keys cannot settle the same hold twice.
7. Define expiry/cleanup only for API response records; never clean financial transactions or ledger entries.

Acceptance criteria:

- Duplicate identical funding, release, refund, withdrawal, and resolution commands do not call the provider or change balances twice.
- Reused keys with changed requests return a global-middleware-formatted conflict.
- Concurrent duplicate requests have one winner and one replay/conflict, with one financial movement.

### Task 3.4 — Implement transactional outbox

1. Write outbox messages in the same EF transaction as domain state/history.
2. Implement an async dispatcher with leasing/rowversion, retry count, exponential backoff, `AvailableAt`, error capture, and processed timestamp.
3. Make event handlers idempotent.
4. Include aggregate type/ID, event type/version, correlation ID, and minimal non-sensitive payload.
5. Implement events listed in the design, including milestone/hold/submission version on submitted and auto-accepted events.

Acceptance criteria:

- A rolled-back domain transaction leaves no outbox message.
- Duplicate dispatcher delivery does not duplicate notifications, jobs, or chat messages.
- Failed messages remain queryable and retryable.
- Sensitive dispute evidence and payment-method references never appear in event payloads/logs.

### Task 3.5 — Add Hangfire job adapters and reconciliation

1. Extend/replace the existing background-job abstraction with async scheduling support, including delayed jobs.
2. Implement Hangfire scheduling only in the provider adapter.
3. Implement job entry points in the Payments slice:
   - `AutoAcceptMilestoneAsync(MilestoneId, EscrowHoldId, SubmissionVersion)`;
   - `ReleaseExpiredHoldAsync(EscrowHoldId)`;
   - provider retry/reconciliation;
   - missing auto-accept/release schedule reconciliation;
   - pending wallet projection reconciliation;
   - outbox dispatch.
4. Job entry points call services; they do not contain duplicated domain logic.
5. Store operational job IDs only for diagnostics, never as proof of eligibility.

Acceptance criteria:

- Jobs can be safely invoked repeatedly and concurrently.
- A scheduling failure after commit is recovered by outbox/reconciliation.
- Stale jobs exit successfully with a structured no-op reason.
- No job uses `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()`.

---

## Phase 4 — Contracts vertical slice

### Task 4.1 — Add contract DTOs and validators

Under `Features/Contracts/DTOs` and `Validators`, implement:

- `CreateContractRequest`, `UpdateContractRequest`, `TerminateContractRequest`;
- `ContractSummaryDto`, `ContractDetailDto`, `ContractStateHistoryDto`;
- pagination/filter models and action/settlement response DTOs where owned by Contracts.

Validate non-empty IDs, title 3–200, terms 20–20,000, termination reason max 2,000, pagination 1–100, enum filters, and `If-Match` format. Do not expose a writable status or total.

Acceptance criteria:

- Invalid inputs fail through FluentValidation before service mutation.
- No DTO/entity contains validation Data Annotations.
- Contract total and permitted actions are response-only, derived values.

### Task 4.2 — Implement `IContractService`/`ContractService`

Implement:

- create from accepted proposal;
- list participant contracts;
- get contract detail;
- update draft;
- accept contract;
- retrieve state history;
- completion evaluation;
- termination orchestration.

Rules:

1. Only the accepted-proposal lawyer creates the draft.
2. Copy proposal/case/client/lawyer IDs from the trusted Proposal service result, not request fields.
3. Enforce one contract per proposal.
4. Draft edits require optimistic concurrency and reset both acceptance timestamps.
5. Each participant can accept once per current draft.
6. Activate only when both accept and at least one mutually approved priced milestone exists.
7. Calculate current milestone total at query time.
8. Manually map entities to DTOs in `ContractService`.
9. Append history and outbox records in the same transaction.

Acceptance criteria:

- Rejected/nonexistent proposals and mismatched lawyer/case ownership fail with `BusinessException("Message")` or the established not-found/forbidden exception handled globally.
- Editing an accepted draft clears both acceptances.
- One-sided acceptance leaves the contract Draft.
- Two-sided acceptance without an approved milestone leaves it Draft; after the prerequisite exists it becomes Active exactly once.
- Contract activation never charges any amount.
- List/detail never exposes contracts to nonparticipants except an authorized moderator path.

### Task 4.3 — Implement `ContractsController`

Implement:

- `POST /api/contracts`
- `GET /api/contracts`
- `GET /api/contracts/{id}`
- `PUT /api/contracts/{id}`
- `POST /api/contracts/{id}/accept`
- `POST /api/contracts/{id}/terminate`
- `GET /api/contracts/{id}/state-history`

Controllers handle route/body/header binding and authorization policy selection, call `IContractService`, and return `ApiResponse<T>`. They contain no EF queries or business-state decisions.

Acceptance criteria:

- Create returns HTTP 201 with `ApiResponse<ContractDetailDto>.Created(data)`.
- Successful reads/mutations return wrapped responses.
- Exceptions flow to global middleware; no controller returns an explicit 500.
- OpenAPI documents authentication, `If-Match`, idempotency where applicable, and expected wrapped responses.

---

## Phase 5 — Milestone negotiation and sequencing slice

### Task 5.1 — Add milestone/change-request DTOs and validators

Implement all milestone DTOs and requests from the API contract, plus change-request detail if needed. Validators enforce:

- positive EGP amount with no more than two decimal places;
- order number > 0;
- duration 1–365;
- title/description/reason limits;
- valid future/derived due dates;
- at least one actual change in a change request;
- no amount field on funded-milestone change requests;
- non-empty authorized stored-file IDs for submission attachments.

Acceptance criteria:

- Request DTOs cannot set funding state, `FundedAt`, hold ID, auto-accept time, fee, acceptance source, or server timestamps.
- Invalid amount scale, duration, order, and empty change requests fail without database writes.

### Task 5.2 — Implement milestone draft, edit, and mutual approval

In `IMilestoneService`/`MilestoneService`, implement:

- add milestone to a Draft contract or rolling Active contract;
- edit only Draft milestone terms;
- record each participant's approval;
- transition to `AwaitingFunding` only after both approvals;
- lawyer-only ready-for-funding signal;
- list/map milestones with derived funding status and permitted actions.

Rules:

1. Order is unique and future ordering remains sequential.
2. Every milestone amount is greater than zero.
3. Editing terms resets both milestone approvals.
4. Settled/accepted milestones are immutable.
5. Rolling future milestones may be negotiated while the contract is Active, but cannot bypass current unsettled work.
6. `ReadyForFundingAt` is set once by the lawyer and emits the notification event.

Acceptance criteria:

- A single approval does not reach `AwaitingFunding`.
- Both approvals produce one state transition/history record.
- No code treats the calculated contract total as payable.
- Editing a funded, accepted, released, refunded, cancelled, or disputed milestone fails.
- Duplicate orders are rejected in service and database layers.

### Task 5.3 — Implement time-extension change requests

1. Create one pending request per milestone.
2. Allow description, duration, and due-date changes only.
3. Allow only the non-requesting participant to approve/reject.
4. On approval, update allowed milestone fields transactionally; do not change amount, hold, fee, or funding.
5. Prohibit moving due dates backwards.
6. Append audit/history and notifications for create/approve/reject/cancel.

Acceptance criteria:

- Two simultaneous pending requests cannot exist.
- Requester cannot self-approve.
- An approved extension leaves a funded active milestone `FundedInProgress` and its payment facts unchanged.
- Amount changes require settlement/cancellation plus a replacement milestone.

### Task 5.4 — Implement `MilestonesController`

Implement all milestone and change-request endpoints documented in `04_api_contracts_and_dtos.md`. Return only standard response wrappers and delegate all business rules to `IMilestoneService`.

Acceptance criteria:

- Endpoint routes, role policies, DTOs, and response types match the API contract.
- No controller accesses `ApplicationDbContext` or another controller.

---

## Phase 6 — Funding, escrow, ledger, and webhook slice

### Task 6.1 — Implement payment DTOs and validators

Implement:

- `FundMilestoneRequest`, `PaymentDto`;
- payment/ledger history DTOs with role-appropriate visibility;
- webhook request DTO;
- retry request/header handling;
- wallet/withdrawal DTOs and validators.

Never return raw payment-method/destination references, provider secrets, internal retry payloads, or another user's wallet data.

Acceptance criteria:

- Funding and withdrawal amounts/method references are validated through FluentValidation.
- DTOs expose gross, fee, net, currency, hold state, dates, and safe provider status only.
- Money commands reject missing idempotency keys before provider execution.

### Task 6.2 — Implement milestone funding

In `IPaymentEscrowService.FundAsync`:

1. Authorize the contract client.
2. Require contract Active, milestone `AwaitingFunding`, lawyer ready signal, and mutual milestone approval.
3. Require all earlier milestones settled and no other milestone in `FundingProcessing`, `FundedInProgress`, `Submitted`, `AcceptedHold`, or `Disputed`.
4. Reserve idempotency and transition only this milestone to `FundingProcessing`.
5. Create a deposit `PaymentTransaction` attempt for exactly the milestone amount/currency.
6. Call `IPaymentProvider.DepositAsync`.
7. On success, transactionally:
   - create/get the contract escrow account;
   - create exactly one milestone hold;
   - calculate gross/5% fee/net;
   - link the completed deposit transaction and hold;
   - append the positive Deposit ledger entry;
   - update escrow totals;
   - create/update the lawyer wallet pending projection by net;
   - set `FundedAt`;
   - transition to `FundedInProgress`;
   - append history/outbox events.
8. On confirmed failure, retain the failed attempt and return the milestone to `AwaitingFunding`.
9. On unknown outcome, retain `FundingProcessing`; do not create a hold, funding timestamp, submission eligibility, or auto-accept job.

Acceptance criteria:

- The client is charged only the selected milestone gross amount.
- Success creates one completed deposit, one funded hold, one deposit ledger entry, and one milestone transition.
- Confirmed failure creates no hold and leaves no funding/submission eligibility.
- Unknown outcome remains processing and cannot be retried as a fresh deposit until reconciliation decides the original attempt.
- Funding milestone A changes no funding facts for milestone B.
- Duplicate and concurrent funding never creates duplicate charges/holds/ledger entries.

### Task 6.3 — Implement webhook and provider reconciliation

1. Validate provider signature/mock secret and deduplicate provider event ID.
2. Locate the exact pending `PaymentTransaction`.
3. Verify operation, provider transaction ID, milestone, amount, EGP currency, and current state.
4. Finalize success/failure through the same internal methods used by synchronous funding.
5. Treat duplicate callbacks as successful no-ops.
6. Reject mismatched callbacks without mutating money.
7. Reconciliation jobs query stale processing attempts and ask the provider for authoritative status.

Acceptance criteria:

- A callback cannot fund a different milestone or altered amount.
- Duplicate/out-of-order callbacks do not duplicate movement.
- A timeout followed by a valid success produces exactly one hold/deposit.
- Invalid signature and mismatched payloads are audited and rejected.

### Task 6.4 — Implement payment query/retry endpoints

Implement:

- `GET /api/contracts/{id}/payments`
- `GET /api/milestones/{id}/payment`
- `POST /api/payments/{transactionId}/retry`
- `POST /api/payments/webhook`

Retry creates a new immutable provider-attempt row while preserving the original. It must remain within the original business idempotency/settlement scope.

Acceptance criteria:

- Participant visibility is limited to their contract; finance admins see operational detail under a separate policy.
- Retry never overwrites the failed transaction.
- Every response uses `ApiResponse<T>`.

---

## Phase 7 — Funded submission, review, auto-acceptance, and hold release

### Task 7.1 — Implement funded submission transaction

In `MilestoneService.SubmitAsync`:

1. Authorize the contract lawyer.
2. Begin an EF transaction and load the milestone, exact hold, completed deposit, latest submission, and rowversion.
3. Require state exactly `FundedInProgress` and run the shared funding verifier.
4. Verify file ownership/access through the File service.
5. Create an immutable submission with the verified `EscrowHoldId` and next version.
6. Create immutable attachment links.
7. Set `SubmittedAt`, `AutoAcceptEligibleAt = SubmittedAt + 7 calendar days`, and current `SubmissionVersion`.
8. Transition to `Submitted`, append history, and add `MilestoneSubmitted` outbox message containing milestone ID, hold ID, and version.
9. Commit. Let the idempotent outbox handler schedule the delayed job and record `AutoAcceptJobId`.

Acceptance criteria:

- Draft, AwaitingFunding, FundingProcessing, or otherwise unfunded milestones cannot submit.
- Any funding-chain mismatch rolls back submission, attachments, timestamps, history, and outbox.
- Resubmission increments the version and receives a new seven-day deadline.
- File references and submissions cannot be edited after commit.

### Task 7.2 — Implement manual acceptance and request changes

Manual acceptance:

1. Authorize the client and require `Submitted`.
2. Revalidate the current submission version and the same exact funded hold/deposit.
3. Set acceptance source Manual, `AcceptedAt`, `HoldStartsAt`, and `HoldExpiresAt = AcceptedAt + 14 days`.
4. Transition milestone to `AcceptedHold`; keep hold `Funded`.
5. Append history/outbox and schedule release through an outbox handler.

Request changes:

1. Require client, current `Submitted` state, current version, and reason.
2. Return to `FundedInProgress`.
3. Clear current submission/auto-accept eligibility operational fields without deleting immutable submission history.
4. Leave the existing deposit, hold, pending wallet projection, gross, fee, and net unchanged.
5. Emit changes-requested history/event. The old scheduled job must become stale by state/version checks.

Acceptance criteria:

- Manual acceptance starts the hold exactly once.
- Request changes never creates a refund or second charge.
- Accept/change races have one committed winner; the loser gets a concurrency/business conflict.
- An old auto-accept job cannot accept after changes or resubmission.

### Task 7.3 — Implement seven-day auto-accept job

Inside one transaction, re-query and require all of:

1. milestone state exactly `Submitted`;
2. non-null `FundedAt` and elapsed `AutoAcceptEligibleAt`;
3. job submission version equals current version;
4. current immutable submission references the job hold ID;
5. hold belongs to milestone and is exactly `Funded`;
6. completed deposit matches milestone, hold, amount, and EGP;
7. no acceptance, change request, refund, release, cancellation, or dispute superseded it.

On success, perform the same acceptance transition as manual acceptance with source Automatic, schedule the fourteen-day release, and emit `MilestoneAutoAccepted`. On failure, commit no domain mutation/event and record a structured diagnostic no-op.

Acceptance criteria:

- Elapsed time alone can never cause acceptance.
- Every mismatch/stale scenario is a no-op.
- The exact seven-day UTC boundary is tested.
- Concurrent manual/automatic acceptance produces one acceptance and one release schedule.

### Task 7.4 — Implement fourteen-day release

1. Lock/reload the hold, account, milestone, wallet, and settlement key.
2. Require `AcceptedHold`, funded hold, elapsed `HoldExpiresAt`, no active dispute, and no prior settlement.
3. Create a release provider attempt.
4. On success, append `Release(lawyerNet)` and `PlatformFee(fee)` entries whose sum equals gross.
5. Update escrow totals/balance, move lawyer net from pending to available, and mark hold/milestone Released.
6. Append state history and `FundsReleased`.
7. On confirmed/unknown provider failure, retain retryable transaction state and do not change ledger/wallet twice.

Acceptance criteria:

- Release before the exact fourteen-day boundary fails/no-ops.
- Release after the boundary settles once, even under retries/concurrency.
- Net + fee equals gross and escrow current balance never becomes negative.
- Disputed/frozen/refunded/released holds cannot use the normal release path.

---

## Phase 8 — Disputes and moderator settlement slice

### Task 8.1 — Add dispute DTOs and validators

Implement all requests/responses from the API design:

- create dispute, add evidence, assign, move to review, resolve, close;
- participant/moderator list/detail DTOs;
- permitted-actions and settlement-processing fields.

Validate title/category/description/requested outcome, evidence presence, moderator ID, resolution summary, non-negative split amounts, penalty combinations, and mandatory idempotency for resolution.

Acceptance criteria:

- Client DTOs never expose internal penalties, moderator-only notes, another user's wallet, or sensitive provider data.
- Invalid split/penalty combinations fail before provider calls.

### Task 8.2 — Implement dispute opening and evidence

In one transaction:

1. Authorize either contract participant.
2. Require milestone `AcceptedHold`, unexpired hold, no existing active dispute.
3. Run the same exact funding-chain verifier.
4. Create the dispute/evidence.
5. Freeze the hold, transition milestone to `Disputed`, and contract to `SuspendedByDispute`.
6. Preserve original hold dates and make the scheduled release job ineligible; do not depend on successful Hangfire cancellation.
7. Append all state history/outbox notifications.

Evidence additions are append-only and allowed only in Open/Assigned/UnderReview after file authorization.

Acceptance criteria:

- Unfunded, submitted-but-not-accepted, expired, settled, or another milestone's hold cannot be disputed.
- Hold freeze, milestone state, contract state, dispute, evidence, history, and outbox commit atomically.
- Expiry-versus-dispute races settle/freeze at most once.
- Evidence cannot be edited/deleted.

### Task 8.3 — Implement moderator investigation workflow

1. Authorize moderator/admin roles.
2. Assign eligible moderator and transition Open → Assigned.
3. Transition Assigned → UnderReview when investigation begins.
4. Fetch proposal/contract/chat/submission/change/payment/evidence views through owning service interfaces with read-only authorization.
5. Audit every moderator view/action.
6. Notify participants without copying sensitive evidence into notification payloads.

Acceptance criteria:

- Moderators cannot mutate chats, submissions, ledger, or provider attempts.
- Nonmoderators cannot access the investigation view.
- No separate dispute chat is created in v1.

### Task 8.4 — Implement immutable dispute resolution

For each outcome, first calculate and validate:

```text
GrossHold = ClientRefund + LawyerNetRelease + PlatformFee
PlatformFee = 5% of the non-refunded gross allocation
```

Then:

- Full refund: refund gross, fee zero, lawyer release zero, reverse pending balance, mark hold/milestone Refunded.
- Full release: refund zero, release lawyer net, record 5% fee, move pending to available, mark Released.
- Partial split: refund approved client amount; calculate fee on the remaining gross allocation; release remaining lawyer net; mark Released.

Implementation steps:

1. Require assigned/under-review dispute and frozen unsettled hold.
2. Reserve idempotency and a unique hold settlement key.
3. Persist one immutable `DisputeResolution`.
4. Create separate immutable provider attempts and ledger entries for refund, release, and fee as applicable.
5. If all operations succeed, settle hold/milestone and set dispute Resolved.
6. If an external operation fails/unknown, keep the resolution immutable with settlement-processing state and schedule retry; never re-decide arithmetic.
7. Return contract to Active or invoke termination orchestration according to the moderator decision.
8. Apply a penalty only through authorized admin logic, as a separate audited record.

Acceptance criteria:

- All outcomes reconcile exactly to gross and never produce a negative balance.
- Fee is zero on refunded funds.
- Resolution replay returns the original decision and never repeats payment movement.
- A second/different resolution is rejected.
- Failed provider settlement is visible/retryable and does not partially double-adjust ledger/wallet.
- Penalty details remain private and cannot be inferred from participant responses.

### Task 8.5 — Implement close and dispute endpoints

Implement every dispute/admin endpoint in the API contract. Close only after provider operations, ledger, wallet, notifications, and reconciliation are successful.

Acceptance criteria:

- State sequence is Open → Assigned → UnderReview → Resolved → Closed.
- A failed/unknown settlement cannot be Closed.
- All controller results use `ApiResponse<T>` and all business failures flow through global middleware.

---

## Phase 9 — Termination, completion, wallet, and withdrawal

### Task 9.1 — Implement termination orchestration

1. Load contract and all milestone/hold/provider states transactionally.
2. Cancel future Draft/AwaitingFunding milestones.
3. Require a FundingProcessing attempt to reconcile/cancel before completion of termination.
4. Fully refund funded-but-unstarted eligible holds.
5. Require mutual settlement or dispute resolution for FundedInProgress, Submitted, AcceptedHold, or Disputed milestones.
6. Preserve Released/Refunded milestones and deliverables.
7. Mark Terminated only after required settlement operations succeed.
8. Emit `ContractTerminated`; notify the Case service without mutating its workflow directly.

Acceptance criteria:

- Termination cannot strand or silently discard an unsettled hold.
- Completed deliverables remain accessible to the client.
- No milestone is transferred to a future lawyer/contract.
- A provider failure leaves termination pending/retryable rather than claiming completion.

### Task 9.2 — Implement contract completion evaluation

After each release/refund/cancellation/resolution, evaluate whether:

- every approved priced milestone is terminal;
- no funding/provider/hold/dispute settlement remains;
- no approved work remains.

If true, transition Active/SuspendedByDispute to Completed once and append history/event.

Acceptance criteria:

- A contract cannot complete with a funded, submitted, held, disputed, or processing milestone.
- Concurrent terminal milestone settlements produce at most one Completed transition.

### Task 9.3 — Implement wallet and withdrawals

1. Create one EGP wallet per lawyer on first funding/release.
2. Treat wallet balances as protected projections of immutable financial operations.
3. Expose pending, available, and total released to that lawyer only.
4. For withdrawal, require idempotency and sufficient available balance.
5. Reserve the amount transactionally, create immutable request/provider attempt, and call provider.
6. On success, finalize the reduction; on confirmed failure release the reservation; on unknown leave it reserved and reconcile.
7. Audit exceptional admin adjustments as compensating ledger/projection records with actor/reason.

Acceptance criteria:

- Funds cannot be withdrawn during pending/fourteen-day hold.
- Concurrent withdrawals cannot overdraw.
- Duplicate withdrawal calls move money once.
- Failure/timeout handling never loses or creates available funds.

---

## Phase 10 — Integration events, notifications, chat, and privacy

### Task 10.1 — Wire all required events

Wire outbox handlers for:

`ContractCreated`, `ContractAccepted`, `ContractActivated`, `MilestoneReadyForFunding`, `MilestoneFundingStarted`, `MilestoneFunded`, `MilestoneFundingFailed`, `MilestoneSubmitted`, `MilestoneAutoAccepted`, `MilestoneAccepted`, `MilestoneChangesRequested`, `FundsReleased`, `FundsRefunded`, `DisputeOpened`, `DisputeAssigned`, `DisputeResolved`, `DisputeClosed`, and `ContractTerminated`.

Acceptance criteria:

- Each domain transition and event is atomic at the outbox boundary.
- Duplicate delivery is safe.
- Notification records include `RelatedEntityType`/`RelatedEntityId`.
- Submitted/auto-accepted events include milestone, hold, and submission version.

### Task 10.2 — Add proposal-conversation system messages

Use the Chat/Conversation service interface to add system messages for contract creation/acceptance, milestone funding/submission/acceptance/change request, dispute open/resolve, funds release/refund, and termination.

Acceptance criteria:

- No chat message content is copied into payment tables.
- Failed chat delivery retries through outbox and does not roll back already-committed financial state.
- System-message handlers deduplicate by event ID.

### Task 10.3 — Enforce privacy and file authorization

1. Authorize each contract attachment, submission attachment, and evidence file through the File service.
2. Use signed/read-authorized access; never expose storage secrets/paths.
3. Separate participant, moderator, finance-admin, and super-admin projections.
4. Scrub payment methods, destination references, evidence text, provider secrets, and penalty details from logs/events.

Acceptance criteria:

- Cross-user file-ID substitution is rejected.
- Moderator access is read-only and audited.
- Participant APIs never reveal internal penalties or another user's wallet/provider details.

---

## Phase 11 — API/error consistency and registration

### Task 11.1 — Register slices and providers

Add DI registrations for all feature service interfaces, provider interfaces, validators, job services, cross-feature services, and outbox handlers. Do not register MediatR handlers for this feature.

Acceptance criteria:

- Application startup resolves every controller/job dependency.
- Registration tests prove slice services use provider abstractions.
- Existing unrelated registrations continue to work.

### Task 11.2 — Align global error and concurrency responses

The current middleware maps `BusinessException` to 400, while the feature contract requires state/idempotency/concurrency conflicts to be 409. Make a backward-compatible global exception enhancement so:

- services still throw `BusinessException` for business/domain failures;
- the middleware, not controllers, selects the appropriate 4xx category/code;
- EF `DbUpdateConcurrencyException` and rowversion/`If-Match` conflicts become wrapped 409 responses;
- provider failures become 502 only when they cannot be represented as safe processing/retry state;
- unexpected failures remain globally formatted 500s without sensitive details.

Do not add local catch-and-return-500 code.

Acceptance criteria:

- Validation 400, unauthorized 401, forbidden 403, not found 404, conflict 409, and exceptional provider 502 responses all use the standard wrapper.
- Existing BusinessException behavior outside the feature is not unintentionally broken.
- Concurrency tests verify no partial state is committed.

### Task 11.3 — Verify OpenAPI contracts

Document all routes, request/response wrappers, role requirements, idempotency headers, rowversion headers, webhook authentication, pagination, and conflict codes.

Acceptance criteria:

- Generated OpenAPI contains every endpoint from `04_api_contracts_and_dtos.md`.
- No public "fund contract" or direct release/refund endpoint is exposed to normal participants.
- Internal jobs are not public API endpoints.

---

## Phase 12 — Automated verification

### Task 12.1 — Unit tests

Create focused tests for:

- every legal/illegal state transition;
- funding and settlement verifier mismatch cases;
- money/fee rounding and reconciliation;
- manual mapping and permitted actions;
- contract/milestone authorization;
- sequential milestone logic;
- change-request rules;
- deterministic provider outcomes;
- idempotency hash/replay/conflict;
- termination classifications;
- wallet projection arithmetic.

Acceptance criteria:

- Tests cover every invariant in section 3.
- Failure tests assert no state/ledger/outbox mutation.

### Task 12.2 — Relational integration tests

Use SQL Server where behavior depends on rowversion, filtered indexes, locking, or check constraints. SQLite may be used only for tests that do not depend on SQL Server semantics; EF InMemory is not acceptable for transaction/constraint verification.

Cover:

1. unique/index/check/FK constraints;
2. create/update/accept/activate flow;
3. each provider outcome and webhook completion;
4. unfunded and cross-milestone submission rejection;
5. manual accept/change/resubmit;
6. stale auto-accept jobs;
7. exact 7-day/14-day UTC boundaries;
8. release/refund/full/partial settlement ledger math;
9. dispute freeze and resolution;
10. termination by milestone category;
11. outbox retry/deduplication;
12. append-only enforcement.

Acceptance criteria:

- Tests exercise real EF transactions and relational constraints.
- Every financial scenario reconciles account, hold, ledger, transaction, and wallet.
- No integration test relies on local timezone or wall-clock sleeps.

### Task 12.3 — Concurrency and race tests

Run concurrent tests for:

- duplicate funding;
- funding two milestones on one contract;
- client accept vs request changes;
- manual vs automatic acceptance;
- hold release vs dispute open;
- duplicate dispute resolution;
- release/refund retries;
- two withdrawals against one balance;
- termination vs provider callback.

Acceptance criteria:

- Each race has one valid winner.
- Losers are safe replay/no-op/conflict outcomes.
- No duplicate hold, ledger entry, settlement, wallet movement, event, or notification occurs.

### Task 12.4 — End-to-end API tests

Build authenticated test journeys for:

- happy-path manual acceptance/release;
- happy-path auto-accept/release;
- changes and resubmission;
- funding failure and timeout/webhook recovery;
- dispute full refund;
- dispute full release;
- dispute partial split;
- graceful termination and new-lawyer handoff boundary;
- withdrawal after release.

Acceptance criteria:

- Routes, roles, response wrappers, status codes, persisted state, events, and money all match the specification.
- An unfunded milestone is never submitted, auto-accepted, held, released, or disputed in any journey.

---

## Phase 13 — Observability, security, and rollout

### Task 13.1 — Add structured observability

Add correlation-aware logs and metrics for:

- funding outcomes/retries and processing age;
- overdue client review and auto-accept no-op reasons;
- holds awaiting release and release retries;
- dispute age/state;
- outbox backlog/failures;
- wallet projection drift;
- idempotency conflicts;
- webhook rejection/mismatch.

Acceptance criteria:

- Operations can trace a contract → milestone → hold → payment attempts → ledger → dispute using IDs/correlation ID.
- Logs contain no secrets, payment method details, evidence, or legal-document content.
- Alerts exist for stuck processing/settlement and reconciliation drift.

### Task 13.2 — Security review

Verify:

- role and resource authorization in services;
- webhook signature and replay protection;
- IDOR resistance for every route/file;
- moderator/finance/super-admin separation;
- anti-overposting via request DTOs;
- immutable audit trails;
- no production representation of mock escrow as regulated payment custody.

Acceptance criteria:

- Unauthorized role/resource matrix tests pass.
- A security reviewer can identify actor, reason, and correlation ID for every sensitive mutation.
- Production configuration fails fast if mock-provider policy is violated.

### Task 13.3 — Deployment and rollback plan

1. Deploy schema before enabling endpoints/jobs.
2. Start outbox and reconciliation workers with feature endpoints disabled.
3. Enable read APIs, then contract/milestone negotiation, then mock funding/settlement in the approved environment.
4. Monitor reconciliation and stuck-state dashboards.
5. Use feature flags to stop new funding while allowing existing processing/settlement/reconciliation to finish.
6. Never roll back by deleting financial rows; use forward fixes and compensating entries.

Acceptance criteria:

- A tested deployment runbook and feature-flag matrix exist.
- Disabling the feature cannot strand already accepted funds or stop required refunds/releases.
- Operational owners know how to retry, reconcile, and escalate without editing ledger history.

---

## 5. Required end-to-end workflow trace

The executing agent must verify this exact trace before declaring the feature complete:

1. **Origin:** Proposal service returns an accepted proposal with authoritative case/client/lawyer IDs.
2. **Draft:** Lawyer creates a Contract Draft with at least one positive-price milestone.
3. **Agreement:** Client and lawyer approve milestone terms and accept the current contract draft.
4. **Activation:** Contract becomes Active; no payment occurs.
5. **Ready:** Lawyer marks the next sequential approved milestone ready for funding.
6. **Funding start:** Client funds only that milestone; state becomes FundingProcessing and a deposit attempt is recorded.
7. **Funding result:**
   - success → exact milestone hold/deposit/ledger/pending wallet → FundedInProgress;
   - confirmed failure → AwaitingFunding, no hold;
   - unknown → FundingProcessing pending webhook/reconciliation.
8. **Submission:** Lawyer submits only after the exact funding chain is transactionally verified; immutable version and seven-day deadline are stored.
9. **Review:**
   - client requests changes → same hold, FundedInProgress, later version;
   - client accepts → AcceptedHold;
   - no response → version-scoped job revalidates and auto-accepts, or safely no-ops.
10. **Fourteen-day hold:** Acceptance time starts the hold. Lawyer funds remain pending/unwithdrawable.
11. **Settlement path A:** No dispute → expiry job writes net release + fee, moves wallet pending to available, and marks Released.
12. **Settlement path B:** Eligible dispute → hold Frozen, milestone Disputed, contract SuspendedByDispute → moderator full refund/full release/partial split → immutable reconciled settlement → Refunded or Released.
13. **Sequence:** Only after the current milestone is settled may the next milestone be funded.
14. **Closure:** When no work/settlement remains, contract becomes Completed; otherwise graceful termination settles/refunds/cancels by state and preserves completed deliverables.
15. **Withdrawal:** Lawyer withdraws only available released net funds through an idempotent provider-backed request.

## 6. Required public endpoint checklist

The implementing agent must build and verify every route below. The response column names the `ApiResponse<T>` payload; controller actions must still return the standard wrapper.

### Contracts

- [x] `POST /api/contracts` → `ContractDetailDto` (201 Created)
- [x] `GET /api/contracts` → `PagedResult<ContractSummaryDto>`
- [x] `GET /api/contracts/{contractId}` → `ContractDetailDto`
- [x] `PUT /api/contracts/{contractId}` → `ContractDetailDto`
- [x] `POST /api/contracts/{contractId}/accept` → `ActionResultDto`
- [x] `POST /api/contracts/{contractId}/terminate` → `ContractDetailDto`
- [ ] `GET /api/contracts/{contractId}/state-history` → `IReadOnlyList<ContractStateHistoryDto>` — route exists, but currently returns `PagedResult<ContractStateHistoryDto>`; reconcile the documented contract.

### Milestones and change requests

- [x] `POST /api/contracts/{contractId}/milestones` → `MilestoneDto`
- [x] `PUT /api/contracts/{contractId}/milestones/{milestoneId}` → `MilestoneDto`
- [x] `POST /api/milestones/{milestoneId}/approve` → `ActionResultDto`
- [x] `POST /api/milestones/{milestoneId}/ready-for-funding` → `ActionResultDto`
- [x] `POST /api/milestones/{milestoneId}/submit` → `MilestoneDto`
- [x] `POST /api/milestones/{milestoneId}/accept` → `MilestoneDto`
- [x] `POST /api/milestones/{milestoneId}/request-changes` → `MilestoneDto`
- [x] `POST /api/milestones/{milestoneId}/change-requests` → `ActionResultDto`
- [x] `POST /api/change-requests/{changeRequestId}/approve` → `ActionResultDto`
- [x] `POST /api/change-requests/{changeRequestId}/reject` → `ActionResultDto`

### Payments, escrow, and wallet

- [x] `POST /api/milestones/{milestoneId}/fund` → `PaymentDto`
- [x] `GET /api/contracts/{contractId}/payments` → `IReadOnlyList<PaymentDto>` or the documented richer payment-history DTO
- [x] `GET /api/milestones/{milestoneId}/payment` → `PaymentDto`
- [x] `POST /api/payments/{paymentTransactionId}/retry` → `PaymentDto`
- [x] `GET /api/wallet` → `WalletDto`
- [x] `POST /api/wallet/withdrawals` → `ActionResultDto`
- [x] `POST /api/payments/webhook` → `ActionResultDto`

There is deliberately no contract-level funding route.

### Disputes

- [ ] `POST /api/disputes` → `DisputeDto`
- [ ] `GET /api/disputes` → `PagedResult<DisputeDto>`
- [ ] `GET /api/disputes/{disputeId}` → `DisputeDto`
- [ ] `POST /api/disputes/{disputeId}/evidence` → `ActionResultDto`
- [ ] `POST /api/admin/disputes/{disputeId}/assign` → `DisputeDto`
- [ ] `POST /api/admin/disputes/{disputeId}/resolve` → `DisputeDto`
- [ ] `POST /api/admin/disputes/{disputeId}/close` → `ActionResultDto`

## 7. Definition of done

The feature is done only when:

- all phases above meet their acceptance criteria;
- all endpoints return `ApiResponse<T>`/`ApiResponse`;
- all business-rule failures are thrown and globally formatted;
- all validators are FluentValidation validators in slice-local `Validators/`;
- all mappings are manual and live in services;
- no Contracts/Payments slice code uses MediatR, CQRS handlers, AutoMapper, Data Annotations, direct controller-to-controller calls, direct SDK calls, `.Result`, or `.Wait()`;
- EF Core Fluent API/migration and SQL Server relational tests prove persistence constraints;
- idempotency, rowversion concurrency, outbox, jobs, provider retry, and reconciliation are verified;
- every money movement reconciles and is append-only;
- the happy path and every failure/dispute/termination path are covered end to end;
- documentation/OpenAPI/runbooks are updated to match the shipped implementation.

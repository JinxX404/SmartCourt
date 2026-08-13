# Milestone Types Implementation Plan

**Branch:** `codex/milestone-types-feature`  
**Status:** Schema/migration phase complete; core workflow not started.  
**Decision gate:** Core workflow implementation must not begin until the migration checkpoint is approved.

## 1. Current architecture and behavior

The current Milestone workflow is a single Standard workflow:

1. A Lawyer creates and updates milestones only while the Contract is `Draft`.
2. Client and Lawyer each approve the current milestone version. The second approval moves it from `Draft` to `AwaitingFunding`.
3. Once the Contract is `Active`, the Lawyer marks the next sequential milestone ready for funding.
4. The Client funds it. Funding is idempotent, calls the configured payment provider, creates the deposit transaction, escrow account/hold/ledger entry, increments the Lawyer wallet pending balance, and moves the milestone through `FundingProcessing` to `FundedInProgress`.
5. The Lawyer submits work and attachments. This moves the milestone to `Submitted` and schedules automatic acceptance after 7 days.
6. The Client accepts the submission, or the automatic-acceptance job does so after 7 days. Acceptance moves it to `AcceptedHold` and starts the 14-day hold.
7. A scheduled job releases the hold after 14 days, moves wallet value from pending to available, writes release/fee ledger entries, and moves the milestone to `Released`.

Provider results can complete synchronously, through a webhook, or through reconciliation. The outbox is used for durable notifications, contract-chat system messages, activation, and Hangfire scheduling. Missing auto-accept and release schedules are recovered by a recurring reconciliation job.

Important current constraints:

- Funding is sequential and allows only one unsettled milestone/hold per Contract.
- The release service accepts only an `AcceptedHold` milestone with a matching elapsed hold deadline.
- Submission, acceptance, disputes, and funded-work change requests all assume the Standard workflow.
- Contract completion considers mutually approved milestones and settled financial state.
- Existing status and type enums are serialized as integer JSON values.

## 2. Proposed domain model

Add a persisted enum without renumbering any existing values:

```text
MilestoneType
  Standard = 0
  Expense  = 1
```

All existing rows are backfilled as `Standard`. Add `Milestone.Type` as required and include it in every milestone read model.

Add a new milestone state at the end of the existing enum so current numeric values remain stable:

```text
ReleasePending = 10
```

`ReleasePending` is necessary because a provider release may fail or require retry. Marking an Expense `Released` before the provider and ledger operations succeed would make the financial state dishonest; reusing `AcceptedHold` would falsely imply a work-acceptance and hold stage.

### State machines

Standard remains unchanged:

```text
Draft -> AwaitingFunding -> FundingProcessing -> FundedInProgress
      -> Submitted -> AcceptedHold -> Released
```

The existing request-changes, automatic-acceptance, dispute, refund, cancellation, and failure transitions remain unchanged.

Expense uses:

```text
Draft -> AwaitingFunding -> FundingProcessing -> ReleasePending -> Released
```

Funding failure still returns `FundingProcessing -> AwaitingFunding`. A funded Expense never enters `FundedInProgress`, `Submitted`, or `AcceptedHold`, and never receives submission, auto-acceptance, hold, or dispute timestamps/jobs.

## 3. Conditional data invariants

- Standard milestones retain the current rules exactly. `Deliverables` and `DurationDays` remain optional because changing that would be a separate breaking requirement.
- Expense milestones require `Deliverables == null` and `DurationDays == null`. Supplying either property, including a non-null empty deliverables array, is rejected rather than silently discarded.
- `DueDate` remains available for an Expense because the requirements exclude only Deliverables and DurationDays; it can represent the reimbursement due date.
- The database adds a check constraint enforcing the Expense nullability rule, in addition to application validation and entity invariants.
- Type is editable only while the milestone is `Draft`. Editing resets client approval. For a milestone on an Active Contract, the type must remain `Expense`; converting it into a mid-contract Standard milestone is rejected.
- Responses expose `type`. Expense responses serialize `deliverables` and `durationDays` as omitted/null-free type-inapplicable fields. The same contract is applied to both `MilestoneDto` and nested `ContractMilestoneDto`.

## 4. Creation, approval, rejection, and CRUD rules

### Standard

- Creation/update remains Lawyer-only and Contract-Draft-only.
- Both Lawyer and Client must approve the current version.
- The existing explicit Lawyer `ready-for-funding` action and strict Standard-to-Standard ordering remain unchanged.

### Expense

- Lawyer may create it when the Contract is `Draft` or `Active`.
- Creation records the Lawyer's acceptance because the Lawyer is the proposer.
- Client approval is always mandatory. No funding path is valid without `AcceptedByClientAt` for the current row version.
- Client approval moves it to `AwaitingFunding` and sets `ReadyForFundingAt` immediately; an additional Lawyer ready-for-funding click is not required.
- Before client approval, the Lawyer may update or withdraw the proposal. Any update clears client approval and preserves Lawyer/proposer acceptance.
- The Client receives an explicit reject action that moves the Draft Expense to `Cancelled` with a reason. This is needed to resolve a proposal cleanly and unblock Contract completion.
- No physical delete is added. Financial/audit entities already use restrictive foreign keys and append-only histories; cancellation is the appropriate delete-equivalent.

The existing create, list, update, and approve routes remain, with type-aware authorization and validation. Add explicit proposal-resolution actions only where needed:

- `POST /api/milestones/{milestoneId}/reject` — Client rejects a Draft Expense.
- `POST /api/milestones/{milestoneId}/cancel` — Lawyer withdraws a Draft Expense.

All mutation responses retain the standard `ApiResponse<T>` envelope, rate-limit metadata, localized errors, and row-version/`If-Match` protection.

## 5. Ordering and concurrency policy

Expense milestones append the next Contract `OrderNumber` for stable display and auditing, but they do not participate in Standard execution sequencing:

- Standard milestones continue to be sequential relative to other Standard milestones.
- An Expense can be approved, funded, and released while a Standard milestone is funded, submitted, or in its 14-day hold.
- A pending or failed Expense does not prevent the next Standard milestone from progressing.
- Multiple Expense reimbursements may be in payment/release processing concurrently.

The current one-active-milestone rule incidentally protects shared escrow-account and wallet projections from concurrent funding finalization. Removing that restriction for Expenses requires hardening funding completion with serializable/idempotent financial finalization and concurrency retry. Provider calls stay outside database transactions; only the local escrow/ledger/wallet state transition is retried. Existing unique provider transaction, milestone hold, idempotency, and hold-settlement constraints remain the final duplicate guards.

## 6. Funding and immediate release design

`PaymentEscrowService.CompleteFundingAsync` remains the single finalization path for synchronous provider success, webhook success, and reconciliation success. It branches only after validating and recording the common funding chain:

- Standard: transition to `FundedInProgress` exactly as today.
- Expense: transition to `ReleasePending`, keep hold dates null, and durably request immediate release.

Immediate release uses the existing release accounting and provider logic, generalized into a type-aware release operation:

- Standard eligibility: `Standard + AcceptedHold + matching elapsed 14-day deadline + no active dispute`.
- Expense eligibility: `Expense + ReleasePending + funded escrow hold`; no submission, acceptance, hold deadline, or dispute gate.
- Success writes the existing release and platform-fee ledger entries, updates escrow totals, moves wallet pending balance to available balance, settles the hold, records the provider release transaction, emits `FundsReleased`, and moves `ReleasePending -> Released`.
- Provider failure/unknown results preserve `ReleasePending` and the funded hold, using the existing retry/backoff/manual-action model.

The release request is persisted through the outbox with funding. The scheduling handler reacts to an Expense `MilestoneFunded` event and schedules release at the current UTC time. The recurring scheduling reconciliation also scans `Expense + ReleasePending` milestones and their holds, so a process crash, outbox delay, or missing Hangfire schedule cannot lose the release.

This implements “instant” as no business hold or human workflow after successful funding. Actual availability still depends on successful provider release; failures remain visible and recoverable rather than being reported as released.

## 7. Cross-feature changes

- **Milestone commands:** explicit Standard-only guards on submit, accept, request-changes, auto-accept, and funded-work change requests.
- **Payments:** type-aware funding eligibility, Standard-only sequence checks, concurrent-safe financial finalization, and Expense release recovery.
- **Disputes:** explicitly reject Expense milestones; they have no post-acceptance dispute window.
- **Contract completion:** an unresolved Active-Contract Expense proposal (`Draft`) blocks automatic completion until the Client approves or rejects it, or the Lawyer cancels it. Approved Expenses must be terminal before completion.
- **Contract termination:** a funded `ReleasePending` Expense is treated as an owed reimbursement awaiting release, not as unstarted Standard work eligible for automatic refund. Termination recovery waits for that release to settle.
- **Queries/action hints:** type-aware `PermittedActions`; Clients see `Approve`/`Reject` for pending Expenses and `Fund` once approved, while submission/acceptance actions never appear for Expenses.
- **Notifications/chat:** type-aware wording for proposal, approval, funding, release-pending, release success, rejection, and cancellation. Existing Standard messages remain unchanged.
- **Documentation:** update the Milestones, Payments, and Contract integration guides plus generated/static OpenAPI documents with enum values, conditional fields, routes, states, and examples.

## 8. Database migration phase

Create one EF Core migration that:

1. Adds `Milestones.Type int NOT NULL DEFAULT 0` so every existing milestone remains Standard.
2. Adds `CK_Milestones_Type_Range` for values 0–1.
3. Adds `CK_Milestones_ExpenseFields` requiring Expense rows to have null `Deliverables` and `DurationDays`.
4. Expands `CK_Milestones_Status_Range` from 0–9 to 0–10.
5. Adds an index supporting release recovery, expected to cover `Type`, `Status`, and `FundedAt`.
6. Updates the model snapshot and migration metadata tests.
7. Verifies forward SQL, rollback SQL, default/backfill behavior, and EF model constraints.

After this phase, stop and request approval before core workflow implementation.

## 9. Automated test strategy

### C# tests

- Enum numeric stability and transition-guard matrices for both types.
- Entity and database invariants for conditional fields and migration backfill.
- Validator matrices for Standard/Expense create and update payloads.
- Draft and Active Contract creation rules, Lawyer-only proposal, Client approval requirement, update-reset behavior, rejection, cancellation, and stale `If-Match` handling.
- Permitted actions and DTO serialization/omission in list and Contract detail responses.
- Standard regression tests for sequential funding, submission, 7-day auto-acceptance, manual acceptance, changes requested, disputes, and exact 14-day release.
- Expense tests proving there are no submissions, acceptance timestamps, auto-accept jobs, hold dates, or 14-day delay.
- Synchronous, webhook, retry, and reconciliation funding success/failure for both types.
- Immediate Expense release success, idempotent duplicate execution, provider retry/backoff, missing payout account recovery, and concurrent Expense funding/release projection safety.
- Contract completion and termination races involving pending, rejected, release-pending, released, and failed Expense milestones.
- Outbox scheduling, recurring reconciliation, notification, and contract-chat event tests.
- Controller and API E2E role/status/validation coverage.
- Architecture, persistence configuration, and full solution regression suites.

### HTTP PowerShell tests

Use the required `generate-http-test` skill to create `SmartCourt.Tests/HttpTests/MilestoneTypes_Test.ps1` and `MilestoneTypes_Report.md`.

The script will follow the repository's zero-assumption flow: register fresh Client/Lawyer accounts, extract email verification tokens, confirm, log in, complete profiles and prerequisites, create a case/proposal/contract, and then exercise every affected Milestone and Payment endpoint. It will cover both full Standard and Expense lifecycles, draft and mid-contract Expense creation, mandatory Client approval, rejection/cancellation, forbidden fields, roles, missing/stale headers, idempotency, provider success/failure/processing, and malicious/extreme/type-mismatch payloads. Every request body, response, and status is written to the Markdown report.

The script must be executed against the application and all scenarios verified before any request to commit, push, or merge.

## 10. Staggered delivery gates

1. **Plan approval (current gate):** approve or amend the decisions in this document.
2. **Schema/migration:** implement only the enum/entity persistence contract, migration, snapshot, and migration/model tests; report results and stop.
3. **Core/domain/API:** implement workflow, payments, outbox/jobs, cross-feature guards, DTOs/controllers, and C# tests; report results and stop.
4. **HTTP/docs/final verification:** invoke `generate-http-test`, generate and execute the exhaustive PowerShell suite, update docs/OpenAPI, run all automated tests, report evidence, and stop for final approval.
5. **Git publication:** no commit, push, PR, or merge unless explicitly requested after successful verification.

## 11. Baseline issue discovered during investigation

The application project currently builds, but the test project does not compile on `main`/this branch because commit `3de0c24` added the Deliverables parameter and commented-out change-request controller actions without updating all constructor calls and controller tests. These are pre-existing baseline errors, not Milestone Types changes. Since the affected tests overlap this feature, the relevant constructor/test updates should be repaired during the schema/core phases and clearly separated in the phase report. The existing untracked `MilestoneDeliverables_*` HTTP artifacts and deleted `SmartCourt/api_log.txt` will remain untouched.

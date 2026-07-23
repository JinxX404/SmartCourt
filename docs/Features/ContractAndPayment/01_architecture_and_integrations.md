# Contracts & Payments — Architecture and Integrations

## 1. Scope and v1 decisions

This document defines a new, milestone-only Contracts & Payments vertical slice for Smart Court. The current specification is the source of truth; older planning documents are not normative.

v1 decisions:

- Currency is EGP only.
- Amounts use `decimal(18,2)`.
- Timestamps are stored in UTC and displayed in `Africa/Cairo`.
- A contract belongs to one accepted proposal and one legal case.
- The lawyer drafts the contract and proposes milestones; both parties accept the contract.
- Milestones are sequential. Only one milestone may have an unsettled funded escrow hold at a time.
- Every priced milestone has its own deposit, escrow hold, submission, acceptance, 14-day hold, and settlement lifecycle. The contract total is never charged upfront.
- A milestone must reach `FundedInProgress` through a successful deposit for that exact milestone before work starts or a deliverable can be submitted.
- A time extension is a mutual change request on the active milestone; it does not create a new priced milestone.
- Client acceptance starts a 14-day escrow hold.
- If the client does not respond within seven calendar days after a funded milestone is submitted, the milestone may be auto-accepted and the hold starts.
- An unfunded milestone can never be submitted, scheduled for auto-acceptance, auto-accepted, or released.
- The platform fee is 5% of the milestone price, deducted from the lawyer’s eventual release.
- Disputes freeze the challenged hold. v1 disputes are available until the hold expires.
- Moderators choose full refund, full release, or a partial split. Penalties are manual.
- All financial records and state transitions are append-only/audited.

The mock escrow is a provider-backed simulation. It is not a regulated Egyptian payment/escrow service and must not be presented as one in production.

## 2. Vertical-slice placement

The feature is organized without MediatR:

```text
SmartCourt/
  Features/Contracts/
    ContractsController.cs
    ContractService.cs
    IContractService.cs
    DTOs/
    Validators/
  Features/Milestones/
    MilestonesController.cs
    MilestoneService.cs
    DTOs/
    Validators/
  Features/Payments/
    PaymentsController.cs
    PaymentService.cs
    IPaymentEscrowService.cs
    DTOs/
    Validators/
  Features/Disputes/
    DisputesController.cs
    DisputeService.cs
    DTOs/
    Validators/
  Features/Contracts/Events/
  Common/Entities/
  Common/Enums/
  Persistence/
    Configurations/
    ApplicationDbContext.cs
  Providers/Payments/
    MockPaymentProvider.cs
    IPaymentProvider.cs
```

Controllers perform authentication, authorization, DTO binding, and response mapping. Service classes own business rules and transactions. EF Core owns persistence. Provider interfaces isolate mock payments, future gateways, file storage, notifications, and background jobs.

## 3. Main components

| Component | Responsibility |
|---|---|
| `ContractService` | Drafting, accepting, terminating, and reading contracts |
| `MilestoneService` | Milestone proposal, funding eligibility, submission, acceptance, changes, and sequencing |
| `PaymentEscrowService` | Deposit, hold, release, refund, fee, wallet, and idempotency rules |
| `DisputeService` | Opening, assigning, evidence, resolving, and closing disputes |
| `MockPaymentProvider` | Deterministic successful/failing payment operations for development and tests |
| `NotificationService` | In-app/email/SMS notifications through existing provider abstractions |
| `ContractJobService` | Auto-acceptance, hold expiry, release retries, and stale payment retries |
| `OutboxDispatcher` | Publishes committed integration events after the database transaction commits |

## 4. End-to-end flow

### 4.1 Contract setup

1. A proposal is accepted.
2. The lawyer creates a draft contract containing the case, participants, terms, and the first milestone.
3. The client reviews and either accepts or requests edits.
4. The lawyer accepts the final draft.
5. The contract changes to `Active` only after both parties accept and at least one milestone exists.

Future milestones may be added or negotiated while the contract is active. The next milestone must be mutually approved before funding.

### 4.2 Funding

1. The lawyer marks the next approved milestone as ready for funding.
2. The client calls the funding endpoint for that milestone only; no other milestone and no contract total is charged.
3. `PaymentEscrowService` creates an idempotent payment transaction.
4. The milestone enters `FundingProcessing` while `MockPaymentProvider` returns success or a configured failure.
5. On success, a milestone-specific `EscrowHold` and immutable deposit ledger entry are created.
6. The milestone becomes `FundedInProgress`; only now is work expected or submission allowed.
7. On confirmed failure, the milestone returns to `AwaitingFunding`; no escrow hold or submission eligibility is created. An unknown/asynchronous result remains `FundingProcessing` until webhook or reconciliation confirms the outcome.
8. A notification and outbox event are emitted.

No successful deposit for the exact milestone means no `FundedInProgress` state.

### 4.3 Submission and acceptance

1. The lawyer submits notes and deliverable files.
2. Before accepting the submission, `MilestoneService` verifies all of the following in one transaction:
   - The milestone is exactly `FundedInProgress`.
   - `Milestone.FundedAt` is present.
   - The milestone has exactly one `EscrowHold` in `Funded` state.
   - That hold belongs to the same milestone, has the same EGP amount, and references a completed deposit transaction.
3. The immutable submission stores the verified `EscrowHoldId`, increments `SubmissionVersion`, and the milestone becomes `Submitted`.
4. `AutoAcceptEligibleAt` is set to `SubmittedAt + 7 calendar days`, and the job receives `MilestoneId`, `EscrowHoldId`, and `SubmissionVersion`.
5. The client accepts or requests changes.
6. Acceptance creates `AcceptedHold`, sets `HoldStartsAt = AcceptedAt`, and `HoldExpiresAt = AcceptedAt + 14 days`.
7. A Hangfire job is scheduled for hold expiry.
8. If the client does not act by `AutoAcceptEligibleAt`, the auto-accept job revalidates the same funding and submission facts before accepting.

Requesting changes returns the same milestone to `FundedInProgress`; it does not create a second payment. It clears the current auto-accept eligibility. Resubmission creates a new submission version, eligibility timestamp, and job; a stale job for an older version exits without mutation.

The auto-accept job must exit without changing state or emitting `MilestoneAutoAccepted` if any funding check fails, the hold is frozen/refunded/released, the submission version changed, or the milestone is no longer `Submitted`.

### 4.4 Hold expiry and release

1. The release job verifies that the hold has expired, is funded, undisputed, milestone-specific, and not already settled.
2. The escrow ledger records the lawyer net release and the 5% platform fee as separate entries; together they reconcile to the gross hold.
3. The lawyer’s pending balance moves to available balance for the net amount.
4. The hold and milestone become `Released`.
5. The system emits payment and notification events.

Every job is safe to retry. The hold has a unique settlement key, and the service re-reads current state inside a transaction.

### 4.5 Dispute

1. The client or lawyer opens a dispute against the funded milestone during its hold.
2. The hold becomes frozen and the milestone becomes `Disputed`.
3. The contract remains usable for already-independent work, but no next milestone may be funded until the dispute is resolved.
4. A moderator reviews the contract, proposal conversation, submissions, evidence, and ledger.
5. The moderator resolves to full refund, full release, or a partial split.
6. The service records the resolution and creates compensating ledger/provider operations.
7. The hold is settled, notifications are sent, and the milestone becomes `Refunded` or `Released`.

## 5. Integration contracts

### Cases

- Contract stores `LegalCaseId`, `ClientUserId`, and `LawyerUserId` as direct references for authorization and dashboards.
- Contract creation validates that the case belongs to the client and is eligible for representation.
- Contract activation emits `ContractActivated`.
- Termination emits `ContractTerminated`; the case remains owned by the client and may be used to select a new lawyer.
- Completed deliverables remain contract-owned. A client may explicitly share them with a new lawyer through the case module.

The Contracts slice does not mutate case workflow directly; it publishes events for the Case service to handle.

### Proposals

- Contract creation requires an accepted proposal.
- The proposal is the immutable origin link between client, lawyer, and case.
- Contract acceptance does not change proposal history; it emits `ContractCreated` and `ContractActivated`.

### Chat

- Contract and milestone actions create system messages in the proposal conversation: contract created, accepted, funded, submitted, accepted, changes requested, dispute opened, resolved, and terminated.
- Moderators receive read-only access through service authorization; no message content is copied into the payment tables.
- v1 does not create a separate dispute chat.

### Files and deliverables

- Deliverable and evidence files use the existing `StoredFile`/`IFileStorageService` path.
- File references are immutable after submission.
- Authorization checks validate that the uploader is a participant or authorized moderator.

### Notifications

Important events notify the affected participants:

`ContractCreated`, `ContractAccepted`, `MilestoneReadyForFunding`, `MilestoneFundingStarted`, `MilestoneFunded`, `MilestoneFundingFailed`, `MilestoneSubmitted`, `MilestoneAutoAccepted`, `MilestoneAccepted`, `MilestoneChangesRequested`, `FundsReleased`, `FundsRefunded`, `DisputeOpened`, `DisputeAssigned`, `DisputeResolved`, and `ContractTerminated`.

Notifications reference `RelatedEntityType` and `RelatedEntityId` so clients can navigate directly to the relevant contract, milestone, payment, or dispute.

### Outbox and transactions

State mutation and its outbox messages are committed in one EF Core transaction. A background dispatcher publishes events after commit. Handlers are idempotent and must tolerate duplicate delivery.

## 6. Authorization

- Client: read their contracts, fund milestones, accept/request changes, raise disputes, and view their financial history.
- Lawyer: draft contracts, propose milestones, submit work, request time extensions, and view pending/available balances.
- Moderator: read evidence and resolve disputes; cannot alter historical ledger entries.
- Finance administrator: retry provider operations and inspect reconciliation; cannot rewrite business history.
- Super administrator: apply penalties and perform exceptional manual adjustments, each with a reason.

Every service loads the contract and verifies the current user from the authenticated identity; IDs supplied by clients are never trusted alone.

## 7. Reliability and operational rules

- Use `rowversion`/optimistic concurrency on mutable aggregate roots.
- Use idempotency keys for funding, release, refund, and withdrawal commands.
- Never update an immutable ledger entry; use a compensating entry.
- Unknown provider outcomes remain retryable processing operations. A confirmed deposit failure returns the milestone to `AwaitingFunding`; release/refund failures retain their retryable settlement state.
- Submission and auto-accept services query the successful deposit and funded hold; they never trust a DTO flag such as `isFunded`.
- Auto-accept jobs are scoped to `(MilestoneId, EscrowHoldId, SubmissionVersion)` and revalidate all three values.
- All money operations include contract, milestone, currency, gross amount, fee, net amount, provider operation ID, and correlation ID.
- Metrics should cover funding failures, overdue client reviews, holds awaiting release, dispute age, and provider retries.

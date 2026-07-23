# Contracts & Payments — Core Services and Mock Escrow

## 1. Service interfaces

```csharp
public interface IContractService
{
    Task<ContractDetailDto> CreateAsync(CreateContractRequest request, Guid actorId, CancellationToken ct);
    Task<ContractDetailDto> GetAsync(Guid contractId, Guid actorId, CancellationToken ct);
    Task AcceptAsync(Guid contractId, Guid actorId, CancellationToken ct);
    Task TerminateAsync(Guid contractId, TerminateContractRequest request, Guid actorId, CancellationToken ct);
}

public interface IMilestoneService
{
    Task<MilestoneDto> AddAsync(Guid contractId, AddMilestoneRequest request, Guid actorId, CancellationToken ct);
    Task SubmitAsync(Guid milestoneId, SubmitMilestoneRequest request, Guid actorId, CancellationToken ct);
    Task AcceptAsync(Guid milestoneId, Guid actorId, CancellationToken ct);
    Task RequestChangesAsync(Guid milestoneId, string reason, Guid actorId, CancellationToken ct);
    Task ApproveChangeAsync(Guid requestId, Guid actorId, CancellationToken ct);
}

public interface IPaymentEscrowService
{
    Task<PaymentDto> FundAsync(Guid milestoneId, string paymentMethodReference, string idempotencyKey, Guid actorId, CancellationToken ct);
    Task ReleaseExpiredHoldAsync(Guid escrowHoldId, CancellationToken ct);
    Task RefundAsync(Guid escrowHoldId, decimal amount, string reason, string idempotencyKey, Guid actorId, CancellationToken ct);
    Task<WalletDto> GetWalletAsync(Guid lawyerId, CancellationToken ct);
}

public interface IPaymentProvider
{
    Task<ProviderResult> DepositAsync(ProviderDepositRequest request, CancellationToken ct);
    Task<ProviderResult> ReleaseAsync(ProviderReleaseRequest request, CancellationToken ct);
    Task<ProviderResult> RefundAsync(ProviderRefundRequest request, CancellationToken ct);
    Task<ProviderResult> WithdrawAsync(ProviderWithdrawalRequest request, CancellationToken ct);
}

public interface IContractJobService
{
    Task AutoAcceptMilestoneAsync(AutoAcceptMilestoneJobArgs args, CancellationToken ct);
    Task ReleaseExpiredHoldAsync(Guid escrowHoldId, CancellationToken ct);
}
```

## 2. Transaction boundaries

Business services load the aggregate, verify authorization and state, call the provider outside or at a carefully bounded boundary, then commit the provider result and ledger state transactionally. If a provider is asynchronous, the database state becomes `Processing`, and a webhook/job completes it.

The v1 mock provider is synchronous, but the service still models processing and retry states so a real provider can replace it later.

Every command:

1. Validates the idempotency key.
2. Loads the aggregate with a concurrency token.
3. Checks the state transition.
4. Performs the provider operation or records a retryable operation.
5. Writes immutable transaction/history/outbox rows.
6. Commits once.

## 3. Mock escrow mechanics

### Funding

For a milestone amount `G`:

```text
platformFee = G × 0.05
lawyerNet   = G - platformFee
```

The client is charged `G`. The mock provider returns a deposit success for references beginning with `mock-success`, a failure for `mock-fail`, and a deterministic timeout for `mock-timeout`. Production code never branches on these strings; the provider owns the simulation.

On successful funding:

- Create one `EscrowHold` for the milestone.
- Add a `Deposit` ledger entry for `G`.
- Set hold to `Funded`.
- Set `Milestone.FundedAt`.
- Set milestone to `FundedInProgress`.
- Set lawyer pending balance to `lawyerNet`.

The pending balance is a projection; the escrow ledger remains authoritative.

Funding applies only to the requested milestone. There is no contract-level charge and no deposit may be reused by another milestone. Before the provider call, the milestone transitions from `AwaitingFunding` to `FundingProcessing`. A confirmed provider failure records the attempt and returns it to `AwaitingFunding`; an unknown result remains `FundingProcessing` for webhook/reconciliation. Neither case creates submission eligibility or an auto-accept job.

### Submission funding guard

`MilestoneService.SubmitAsync` must execute the following query and validation inside the same transaction that creates the submission:

1. Load the milestone and its current escrow hold with the completed deposit transaction.
2. Require milestone state `FundedInProgress` and non-null `FundedAt`.
3. Require exactly one hold whose `MilestoneId` matches and whose state is `Funded`.
4. Require a completed deposit transaction whose milestone ID, amount, currency, and hold reference match.
5. Create `MilestoneSubmission` with the verified `EscrowHoldId` and next version.
6. Set state `Submitted`, `SubmittedAt`, and `AutoAcceptEligibleAt = SubmittedAt + 7 days`.
7. Commit the submission, history, and `MilestoneSubmitted` outbox event.
8. An idempotent outbox handler schedules the auto-accept job with milestone ID, hold ID, and submission version and stores `AutoAcceptJobId`. A reconciliation job can recover submitted rows whose job was not scheduled.

If any condition fails, throw a conflict such as `milestone_not_funded`. No submission, timestamp, event, or background job may be created.

### Release

At hold expiry:

1. Lock the hold and account row.
2. Confirm status is `Funded`/`Frozen` only when the dispute resolution explicitly authorizes release.
3. Confirm the 14-day date has passed.
4. Write `Release(lawyerNet)` and `PlatformFee(platformFee)` entries; together they equal the gross hold.
5. Move `lawyerNet` from pending to available.
6. Mark hold and milestone `Released`.
7. Publish `FundsReleased`.

The two ledger entries make gross, fee, and net amounts auditable.

### Refund

For a full client refund:

1. Confirm the hold is unsettled.
2. Call provider refund for the refundable amount.
3. Write a `Refund` entry.
4. Reverse the lawyer pending projection if it was created.
5. Mark hold/milestone `Refunded`.

For a partial split, write a refund for the client amount and release the lawyer amount. Fee treatment is calculated so the ledger balances exactly; the platform fee is not earned on the refunded portion.

Failed provider calls retain a failed `PaymentTransaction` and leave the hold retryable. No balance is changed twice.

## 4. Scheduling and retries

Use Hangfire jobs for:

- Auto-accept a verified funded submission after seven days.
- Hold expiry after 14 days.
- Provider retry with exponential backoff.
- Outbox dispatch.
- Reconciliation of pending wallet projections.

Jobs must be idempotent and use a database lock/concurrency token.

The auto-accept job must re-query and verify:

- Its milestone is still exactly `Submitted`.
- `FundedAt` and `AutoAcceptEligibleAt` exist and the deadline has passed.
- Its `SubmissionVersion` is still current.
- The current submission references the job’s `EscrowHoldId`.
- The hold is still `Funded` for the same milestone.
- The successful deposit still reconciles to the milestone amount and EGP currency.
- No acceptance, change request, dispute, refund, release, or cancellation superseded it.

Only then may it set automatic acceptance, start the 14-day hold, schedule release, and emit `MilestoneAutoAccepted`. A stale or ineligible job exits successfully without mutation or acceptance event and records a diagnostic no-op reason.

## 5. Time extensions

An extension changes only `DurationDays`/`DueDate` on the already-funded milestone and is represented by a `MilestoneChangeRequest`. v1 does not create a standalone zero-price milestone.

Rules:

- The active milestone must not be completed/released/refunded.
- The requester supplies a reason.
- The other participant approves or rejects.
- Approval updates the milestone and appends a state/audit record.
- Amount and platform fee remain unchanged.
- The extension cannot move a due date backwards.

## 6. Contract termination and handoff

Termination first settles the current state:

- Future `Draft`/`AwaitingFunding` milestones become `Cancelled`.
- A `FundingProcessing` milestone must finish or cancel its provider attempt before termination completes.
- Funded but unstarted milestones are fully refunded.
- A `FundedInProgress`/`Submitted`/held milestone requires mutual settlement or dispute resolution.
- Released milestones remain released.

After all required settlement operations succeed, the contract becomes `Terminated` and emits `ContractTerminated`.

The case remains owned by the client. The client may explicitly share completed deliverables with a new lawyer. The new lawyer creates a separate contract and new milestone plan; no milestones are inherited.

## 7. Wallet and withdrawal

Only net released funds become available. A withdrawal reserves available balance, creates a `WithdrawalRequest`, and calls the provider. On success the balance is reduced and the request completes. On failure the reservation is released and the request remains retryable.

The mock wallet is not a bank account and does not represent actual Egyptian money movement.

## 8. Testing requirements

Unit tests must cover:

- Funding before/after acceptance.
- Funding one milestone never marks another milestone as funded.
- Contract activation never charges the sum of all milestones.
- Duplicate funding idempotency.
- Provider success/failure/timeout.
- Sequential milestone enforcement.
- Submission rejection for `Draft`, `AwaitingFunding`, and `FundingProcessing`.
- Submission rejection when `FundedAt`, hold, deposit status, amount, or currency does not reconcile.
- Seven-day auto-acceptance only for a currently submitted, successfully funded milestone.
- Stale auto-accept job after change request or resubmission.
- Auto-accept no-op after refund, dispute, cancellation, or hold mismatch.
- Exact 14-day boundary behavior in UTC.
- Concurrent accept/fund/release commands.
- Full and partial refunds.
- Dispute freezing and settlement.
- Termination with each funded/unfunded milestone category.
- Wallet pending-to-available calculations.

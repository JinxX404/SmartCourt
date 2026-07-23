# Smart Court - Contracts & Payments Detailed Design Decisions

Here is the fully exhaustive extraction, ensuring that absolutely no technical rule, constraint, or state machine rule from the v1 design specs is missed.

## 1. Global & Architectural Standards
- **Currency & Precision**: EGP only. All financial amounts use `decimal(18,2)`.
- **Timezones**: Timestamps are stored in UTC (`datetime2` in SQL) and displayed in `Africa/Cairo`.
- **Architecture**: Vertical Slice Architecture without MediatR. Modules: `Contracts`, `Milestones`, `Payments`, and `Disputes`.
- **Persistence**: SQL Server with EF Core 8. Primary keys are `Guid` (`uniqueidentifier`). Strings are Unicode (`nvarchar`).
- **Concurrency**: Mutable aggregate roots (Contract, Milestone, Escrow, etc.) use optimistic concurrency via SQL Server `rowversion` (`byte[]`). Updates use `If-Match` validation.
- **Append-Only Data**: Financial records, state-history records, and dispute resolutions are strictly append-only.
- **No Soft Deletes**: Soft deletion is NOT used for financial records. `DeleteBehavior.Restrict` is enforced on contracts, milestones, escrow, payments, disputes, and users. `Cascade` is only for owned attachments where the parent is removed before any financial activity.
- **Idempotency**: An `Idempotency-Key` header is mandatory for all state-mutating money commands (funding, release, refund, withdrawal, dispute resolution). Reusing a key with a different request hash triggers a conflict.
- **Outbox Pattern**: Integration events are committed in the same EF Core transaction as the domain state and dispatched asynchronously via a background dispatcher.
- **API Responses**: Standardized JSON wrapper (`success`, `data`, `message`, `errors`, `statusCode`). Returns 400 (validation), 401/403 (auth), 404 (not found), 409 (state/concurrency conflicts), 502 (provider failures that cannot be retried).

## 2. Database Constraints & EF Core Rules
- **Money Fields**: Must use `.HasPrecision(18, 2)`.
- **String Limits**: Configured explicitly. Titles: 3–200 chars. Terms: 20–20,000 chars. Description/Reasons: max 2,000–10,000 depending on the DTO.
- **Check Constraints**: Enforce positive milestone amounts, non-negative ledger/payment amounts, Currency = 'EGP', positive order numbers, and duration validity (1–365 days).
- **Unique Indexes**: 
  - `Contract.ProposalId` (a proposal has at most one contract).
  - `Milestone(ContractId, OrderNumber)`
  - `EscrowHold.MilestoneId`
  - `LawyerWallet.LawyerUserId`
  - Open Dispute: Only one open dispute per milestone allowed.
- **General Indexes**: Included for `(ContractId, Status)`, `(MilestoneId, Status)`, `(HoldExpiresAt, Status)`, `(LawyerUserId, Status)`, `(Dispute.Status, CreatedAt)`, and outbox processing queue.

## 3. Contract Mechanics & State Machine
- **Relationships**: A contract strictly belongs to one accepted proposal and one legal case (`LegalCaseId`, `ClientUserId`, `LawyerUserId`).
- **Dynamic Totals**: The contract's `TotalAmount` is NEVER stored; it is dynamically calculated from the sum of approved priced milestones.
- **Contract States**: `Draft` → `Active` → (`SuspendedByDispute` | `Completed` | `Terminated`).
  - **Draft**: Editable. Any edit resets both client and lawyer acceptance timestamps.
  - **Active**: Requires BOTH parties to accept and the existence of at least 1 milestone.
  - **SuspendedByDispute**: Set automatically when a dispute is opened. Reverts to Active if the dispute is non-terminal.
  - **Completed**: All priced milestones released/refunded, no work remains.
- **Clean Termination**: Contract termination cancels future unstarted milestones, and refunds unstarted-but-funded milestones. The client retains completed deliverables and takes them to a new lawyer (who creates a brand new contract with no inheritance).

## 4. Milestone Workflow & State Machine
- **Sequential Execution**: Milestones are strictly sequential. Only one milestone may be funded and in-progress at a time.
- **Independent Payment Lifecycle**: Every milestone is charged and held separately. Funding milestone A does not fund milestone B, and the contract’s calculated total is never charged as one payment.
- **Milestone States**: `Draft` → `AwaitingFunding` → `FundingProcessing` → `FundedInProgress` → `Submitted` → `AcceptedHold` → (`Released` | `Refunded` | `Cancelled` | `Disputed`).
- **Funding Pre-condition**: A milestone must have `Amount > 0`. Successful funding requires a completed deposit and one `Funded` escrow hold linked to the exact milestone, amount, and EGP currency.
- **Work/Submission Gate**: Work and submission are allowed only in `FundedInProgress`. `SubmitAsync` transactionally revalidates `FundedAt`, the hold, and the completed deposit before storing a submission.
- **No Standalone Zero-Price Milestones**: v1 represents time-only extensions as change requests on the already-funded milestone.
- **Change Requests**: Mutual change requests (`Pending`, `Approved`, `Rejected`, `Cancelled`) handle duration/due-date changes. Only the non-requesting participant can approve. A funded milestone's price cannot be altered.
- **Versioned Submissions**: `MilestoneSubmission` is versioned starting at 1 and stores the verified `EscrowHoldId`. `MilestoneSubmissionAttachment` holds file links.
- **Client Auto-Acceptance**: A successfully funded submission sets `AutoAcceptEligibleAt = SubmittedAt + 7 days`. The Hangfire job is scoped to milestone ID, escrow hold ID, and submission version.
- **Reliable Scheduling**: The committed `MilestoneSubmitted` outbox event schedules the job and stores `AutoAcceptJobId`; reconciliation recovers any eligible submitted milestone whose scheduling step failed.
- **Auto-Accept Revalidation**: The job must verify current `Submitted` state, deadline, version, `FundedAt`, matching `Funded` hold, and completed matching deposit. Any mismatch is a monitored no-op with no acceptance or release event.

## 5. Payments, Escrow & Fees
- **Escrow Simulation**: A `MockPaymentProvider` is used. For testing, it branches on string prefixes: `mock-success` (deposit success), `mock-fail` (failure), `mock-timeout` (timeout).
- **Platform Fee**: The platform takes a 5% fee from the milestone price, deducted from the lawyer’s eventual release (Lawyer Net = Gross - 5%).
- **Escrow Architecture**: One `EscrowAccount` per contract. One `EscrowHold` per funded milestone.
- **No Contract-Level Funding**: The escrow account aggregates ledger entries but does not make its total balance interchangeable across milestones. Every hold is reserved for one milestone.
- **Immutable Ledgers**: `EscrowLedgerEntry` holds events (`Deposit`, `Release`, `Refund`, `PlatformFee`, `Adjustment`). `CurrentBalance` is calculated from entries (TotalDeposited - TotalReleased - TotalRefunded - TotalFees) and must never be negative.
- **14-Day Hold**: Client acceptance (manual or auto) sets `HoldStartsAt = AcceptedAt` and `HoldExpiresAt = AcceptedAt + 14 days`.
- **Release Math**: A Hangfire release job creates two ledger entries: `Release(lawyerNet)` and `PlatformFee(platformFee)`. Their sum must exactly equal the gross hold.
- **Lawyer Wallet**: Only net released funds move to the lawyer's `AvailableBalance` on `LawyerWallet`. Withdrawals reserve the available balance, call the provider, and either reduce the balance (success) or free the reservation (failure).

## 6. Dispute Resolution
- **Eligibility**: Open against an `AcceptedHold` milestone before the hold expires. Either party can raise it.
- **Dispute States**: `Open` → `Assigned` → `UnderReview` → `Resolved` → `Closed`.
- **Freezing Escrow**: Opening a dispute immediately freezes the hold and changes the contract state.
- **Moderator Access**: Moderators get read-only access to the proposal chat, deliverables, and ledger. v1 does not create a separate dispute chat.
- **Resolution Outcomes**:
  1. **Full Refund**: Gross hold refunded to client. No platform fee collected.
  2. **Full Release**: Gross hold released to lawyer (minus 5% fee).
  3. **Partial Split**: Custom split. The 5% platform fee is ONLY deducted from the lawyer's released portion.
- **Reconciliation Rule**: `Gross Hold = Client Refund + Lawyer Release + Platform Fee`. This must mathematically reconcile before the system commits the resolution.
- **Manual Penalties**: Administrators can manually apply penalties: `Warning` (hidden flag), `Suspension12Months`, `Suspension24Months`, or `PermanentTermination`.

## 7. Failure Handling & Race Conditions
- **Hangfire Jobs**: Relied upon for funded-submission 7-day auto-accept, 14-day hold expiry, provider retries (with exponential backoff), outbox dispatch, and pending-wallet reconciliation.
- **Auto-Accept Job Arguments**: `(MilestoneId, EscrowHoldId, SubmissionVersion)`. The job re-queries authoritative database state and never trusts the scheduled timestamp alone.
- **Stale Job Rule**: A change request, resubmission, dispute, refund, cancellation, hold mismatch, or funding mismatch makes the old job an idempotent no-op.
- **Processing States**: Unknown provider outcomes remain `Processing` and retain their `PaymentTransaction`; confirmed failures are recorded and move to the documented retryable business state. No balance is changed twice.
- **Deposit Outcome Rule**: A confirmed deposit failure returns the milestone to `AwaitingFunding`; an unknown/asynchronous outcome remains `FundingProcessing` until webhook/reconciliation determines success or failure.
- **Concurrency Conflicts**: If a concurrency conflict occurs, the action fails and the caller/moderator must reload. No partial DB state is committed.
- **Race Condition Prevention**: If a hold expiry job races with dispute creation, the database transaction that first locks and settles the hold wins. The loser returns a conflict and will not execute a duplicate financial movement.

## 8. Notifications & Privacy
- **Privacy**: Internal penalties and other users' wallet data are never exposed in standard APIs.
- **Notification Payloads**: Payloads include `RelatedEntityType` and `RelatedEntityId` (dispute/milestone IDs) but do NOT contain sensitive evidence content.
- **Event Triggers**: System emits notifications for `ContractCreated`, `ContractAccepted`, `MilestoneReadyForFunding`, `MilestoneFundingStarted`, `MilestoneFunded`, `MilestoneFundingFailed`, `MilestoneSubmitted`, `MilestoneAutoAccepted`, `MilestoneAccepted`, `MilestoneChangesRequested`, `FundsReleased`, `FundsRefunded`, `DisputeOpened`, `DisputeAssigned`, `DisputeResolved`, and `ContractTerminated`.
- **Event Safety**: `MilestoneSubmitted` and `MilestoneAutoAccepted` include milestone ID, escrow hold ID, and submission version. `MilestoneAutoAccepted` is emitted only after successful execution-time funding verification.

## 9. Implementation Revision Plan

1. Update the milestone enum and all switch statements to add `FundingProcessing` and rename `InProgress` to `FundedInProgress`.
2. Make milestone amounts positive and represent time-only changes through `MilestoneChangeRequest`.
3. Add `AutoAcceptEligibleAt`, `AutoAcceptJobId`, and `AcceptanceSource` to `Milestone`; add required `EscrowHoldId` to `MilestoneSubmission`.
4. Implement a single reusable funding-invariant query used by submit, manual accept, auto-accept, and dispute opening.
5. Transition funding through `AwaitingFunding → FundingProcessing → FundedInProgress`; create the escrow hold only after provider success.
6. Schedule auto-accept only after the funded submission transaction commits, passing milestone ID, hold ID, and submission version.
7. Make stale/ineligible jobs safe no-ops and add structured monitoring reasons.
8. Update DTOs to expose derived funding status while keeping all funding fields server-owned.
9. Add unit/integration tests for unfunded submission, cross-milestone payment reuse, stale jobs, and all race conditions.

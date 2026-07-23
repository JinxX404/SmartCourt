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
- **Check Constraints**: Enforce non-negative amounts, Currency = 'EGP', positive order numbers, and duration validity (1–365 days).
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
- **Milestone States**: `Draft` → `AwaitingFunding` → `InProgress` → `Submitted` → `AcceptedHold` → (`Released` | `Refunded` | `Cancelled` | `Disputed`).
- **Funding Pre-condition**: A milestone must have an `Amount > 0` to be funded. Zero-price (time-only) milestones skip escrow holding entirely.
- **Change Requests**: Mutual change requests (`Pending`, `Approved`, `Rejected`, `Cancelled`) handle duration/due-date changes. Only the non-requesting participant can approve. A funded milestone's price cannot be altered.
- **Versioned Submissions**: `MilestoneSubmission` is versioned starting at 1. `MilestoneSubmissionAttachment` holds file links.
- **Client Auto-Acceptance**: If the client does not accept or request changes within 7 calendar days after submission, a Hangfire job auto-accepts it and starts the escrow hold.

## 5. Payments, Escrow & Fees
- **Escrow Simulation**: A `MockPaymentProvider` is used. For testing, it branches on string prefixes: `mock-success` (deposit success), `mock-fail` (failure), `mock-timeout` (timeout).
- **Platform Fee**: The platform takes a 5% fee from the milestone price, deducted from the lawyer’s eventual release (Lawyer Net = Gross - 5%).
- **Escrow Architecture**: One `EscrowAccount` per contract. One `EscrowHold` per funded milestone.
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
- **Hangfire Jobs**: Relied upon for 7-day auto-accept, 14-day hold expiry, provider retries (with exponential backoff), outbox dispatch, and pending-wallet reconciliation.
- **Processing States**: A provider failure during a refund/release/deposit leaves the state as `Processing` (retryable) and retains the failed `PaymentTransaction`. No balance is changed twice.
- **Concurrency Conflicts**: If a concurrency conflict occurs, the action fails and the caller/moderator must reload. No partial DB state is committed.
- **Race Condition Prevention**: If a hold expiry job races with dispute creation, the database transaction that first locks and settles the hold wins. The loser returns a conflict and will not execute a duplicate financial movement.

## 8. Notifications & Privacy
- **Privacy**: Internal penalties and other users' wallet data are never exposed in standard APIs.
- **Notification Payloads**: Payloads include `RelatedEntityType` and `RelatedEntityId` (dispute/milestone IDs) but do NOT contain sensitive evidence content.
- **Event Triggers**: System emits notifications for `ContractCreated`, `ContractAccepted`, `MilestoneReadyForFunding`, `MilestoneFunded`, `MilestoneSubmitted`, `MilestoneAutoAccepted`, `MilestoneAccepted`, `MilestoneChangesRequested`, `FundsReleased`, `FundsRefunded`, `DisputeOpened`, `DisputeAssigned`, `DisputeResolved`, and `ContractTerminated`.

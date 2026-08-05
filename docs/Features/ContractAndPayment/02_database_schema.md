# Contracts & Payments — Database Schema

## 1. Conventions

- SQL Server with EF Core 8.
- `Guid` primary keys (`uniqueidentifier`).
- UTC `DateTime` values (`datetime2`).
- Money: `decimal(18,2)` with `Currency = "EGP"`.
- Text fields are Unicode (`nvarchar`).
- Mutable aggregate roots use SQL Server `rowversion`.
- Financial, dispute, and state-history rows are append-only.
- Soft deletion is not used for financial records.

The tables below are the minimum v1 model. Foreign keys to `ApplicationUser`, `LegalCase`, `Proposal`, `Conversation`, and `StoredFile` refer to existing or planned modules.

## 2. Contract

| Field | Type | Required | Constraints |
|---|---|---:|---|
| `Id` | `Guid` | Yes | PK |
| `ProposalId` | `Guid` | Yes | FK; unique; accepted proposal only |
| `LegalCaseId` | `Guid` | Yes | FK |
| `ClientUserId` | `Guid` | Yes | FK |
| `LawyerUserId` | `Guid` | Yes | FK |
| `Title` | `string` | Yes | max 200 |
| `TermsAndConditions` | `string` | Yes | max 20,000 |
| `Currency` | `string` | Yes | max 3; check `EGP` |
| `Status` | `ContractStatus` | Yes | enum |
| `AcceptedByClientAt` | `DateTime?` | No | UTC |
| `AcceptedByLawyerAt` | `DateTime?` | No | UTC |
| `ActivatedAt` | `DateTime?` | No | UTC |
| `CompletedAt` | `DateTime?` | No | UTC |
| `TerminatedAt` | `DateTime?` | No | UTC |
| `TerminationReason` | `string?` | No | max 2,000 |
| `TerminatedByUserId` | `Guid?` | No | FK |
| `RowVersion` | `byte[]` | Yes | rowversion |
| `CreatedAt`, `UpdatedAt` | `DateTime` | Yes | UTC |

`TotalAmount` is not the source of truth. The service calculates the current sum of approved priced milestones. This avoids claiming a lump-sum obligation when later milestones are negotiated independently.

## 3. Milestone

| Field | Type | Required | Constraints |
|---|---|---:|---|
| `Id` | `Guid` | Yes | PK |
| `ContractId` | `Guid` | Yes | FK |
| `Title` | `string` | Yes | max 200 |
| `Description` | `string?` | No | max 10,000 |
| `OrderNumber` | `int` | Yes | positive; unique per contract |
| `Amount` | `decimal` | Yes | `> 0`, scale 2; funded independently |
| `DurationDays` | `int?` | No | 1–365 when supplied |
| `DueDate` | `DateTime?` | No | derived/validated in UTC |
| `Status` | `MilestoneStatus` | Yes | enum |
| `AcceptedByClientAt` | `DateTime?` | No | UTC |
| `AcceptedByLawyerAt` | `DateTime?` | No | UTC |
| `ReadyForFundingAt` | `DateTime?` | No | UTC |
| `FundedAt` | `DateTime?` | No | UTC |
| `SubmittedAt` | `DateTime?` | No | UTC |
| `AutoAcceptEligibleAt` | `DateTime?` | No | UTC; set only by a verified funded submission |
| `AutoAcceptJobId` | `string?` | No | max 100; operational correlation only |
| `AcceptedAt` | `DateTime?` | No | UTC |
| `AcceptanceSource` | `MilestoneAcceptanceSource?` | No | Manual/Automatic |
| `HoldStartsAt`, `HoldExpiresAt` | `DateTime?` | No | UTC |
| `ReleasedAt`, `RefundedAt` | `DateTime?` | No | UTC |
| `RejectionReason` | `string?` | No | max 2,000 |
| `SubmissionVersion` | `int` | Yes | starts at 1 |
| `RowVersion` | `byte[]` | Yes | rowversion |
| `CreatedAt`, `UpdatedAt` | `DateTime` | Yes | UTC |

Every deliverable milestone has `Amount > 0` and is paid independently. v1 represents a time-only extension as a `MilestoneChangeRequest` on the already-funded active milestone, not as a standalone zero-price milestone. This keeps the submission rule absolute: every submitted milestone must have its own successful deposit and funded escrow hold.

`FundingStatus` is not duplicated as a writable Boolean/column on `Milestone`. The authoritative funding facts are:

1. `Milestone.FundedAt` is non-null.
2. Exactly one related `EscrowHold` exists.
3. For active work/review, the hold is `Funded` and the milestone is `FundedInProgress`, `Submitted`, or `AcceptedHold`; a dispute temporarily changes the hold to `Frozen`.
4. The hold references a completed deposit `PaymentTransaction` for the same milestone, amount, and currency.

API responses expose a derived `MilestoneFundingStatus`; clients cannot set it.

## 4. MilestoneChangeRequest

| Field | Type | Required | Constraints |
|---|---|---:|---|
| `Id`, `MilestoneId`, `RequestedByUserId` | `Guid` | Yes | PK/FKs |
| `ProposedDescription` | `string?` | No | max 10,000 |
| `ProposedDurationDays` | `int?` | No | 1–365 |
| `ProposedDueDate` | `DateTime?` | No | UTC |
| `Reason` | `string` | Yes | max 2,000 |
| `Status` | `ChangeRequestStatus` | Yes | Pending/Approved/Rejected/Cancelled |
| `DecidedByUserId`, `DecidedAt` | nullable | No | approval audit |
| `CreatedAt` | `DateTime` | Yes | UTC |

Only one pending change request may exist for a milestone.

## 5. MilestoneSubmission and attachments

`MilestoneSubmission` stores a versioned submission:

| Field | Type | Required |
|---|---|---:|
| `Id`, `MilestoneId`, `EscrowHoldId`, `SubmittedByUserId` | `Guid` | Yes |
| `Version` | `int` | Yes |
| `Notes` | `string` | Yes; max 10,000 |
| `SubmittedAt` | `DateTime` | Yes |

`EscrowHoldId` is required and must identify the successful funded hold for the same milestone. `MilestoneSubmissionAttachment` contains `Id`, `MilestoneSubmissionId`, `StoredFileId`, and `CreatedAt`. A submission is immutable.

## 6. Escrow and money

### EscrowAccount

One per contract:

`Id`, `ContractId` (unique), `Currency`, `TotalDeposited`, `TotalReleased`, `TotalRefunded`, `TotalFees`, `Status`, `RowVersion`, `CreatedAt`, `UpdatedAt`.

`CurrentBalance` is calculated by the service as:

```text
TotalDeposited - TotalReleased - TotalRefunded - TotalFees
```

The service verifies the result is never negative.

### EscrowHold

One per successfully funded milestone:

`Id`, `EscrowAccountId`, `ContractId`, `MilestoneId` (unique), `GrossAmount`, `PlatformFeeAmount`, `NetAmount`, `Status`, `FundedAt`, `HoldStartsAt`, `HoldExpiresAt`, `FrozenAt`, `SettledAt`, `SettlementType`, `ProviderDepositTransactionId`, `ProviderReleaseTransactionId`, `ProviderRefundTransactionId`, `RowVersion`, `CreatedAt`, `UpdatedAt`.

`ProviderDepositTransactionId`, `FundedAt`, and `Status = Funded` are mandatory before a submission can be created. `HoldStartsAt` and `HoldExpiresAt` remain null until manual or automatic acceptance.

### EscrowLedgerEntry

Immutable entries:

`Id`, `EscrowAccountId`, `EscrowHoldId?`, `TransactionType`, `Amount`, `RunningBalance`, `Currency`, `ReferenceType`, `ReferenceId`, `PaymentTransactionId?`, `Description`, `CreatedByUserId?`, `CorrelationId`, `CreatedAt`.

`TransactionType` is `Deposit`, `Release`, `Refund`, `PlatformFee`, or `Adjustment`. Amount is always positive; direction is represented by type.

### PaymentTransaction

Each provider attempt is a separate row:

`Id`, `ContractId`, `MilestoneId?`, `EscrowHoldId?`, `OperationType`, `ProviderName`, `ProviderTransactionId?`, `IdempotencyKey`, `Amount`, `Currency`, `Status`, `FailureReason?`, `ProcessedAt?`, `CreatedAt`, `UpdatedAt`.

Every milestone deposit/release/refund attempt requires `MilestoneId`; withdrawals may leave it null. Failed deposit attempts may have a null `EscrowHoldId`, while a completed deposit must reference the resulting hold. Enforce this with operation-specific validation/check constraints. Unique indexes: `(ProviderName, ProviderTransactionId)` when non-null and `IdempotencyKey`.

### LawyerWallet and WithdrawalRequest

`LawyerWallet`: `Id`, `LawyerUserId` (unique), `Currency`, `PendingBalance`, `AvailableBalance`, `RowVersion`, `CreatedAt`, `UpdatedAt`.

`WithdrawalRequest`: `Id`, `LawyerUserId`, `Amount`, `Currency`, `Status`, `ProviderTransactionId?`, `FailureReason?`, `RequestedAt`, `ProcessedAt?`, `IdempotencyKey`.

Wallet balances are projections protected by ledger transactions. They are never manually edited without an audited adjustment.

## 7. Disputes and enforcement

### Dispute

`Id`, `ContractId`, `MilestoneId`, `RaisedByUserId`, `AssignedModeratorUserId?`, `Category`, `Title`, `Description`, `Status`, `RequestedOutcome`, `ResolutionType?`, `ResolutionAmount?`, `ResolutionSummary?`, `ResolvedByUserId?`, `ResolvedAt?`, `ClosedAt?`, `RowVersion`, `CreatedAt`, `UpdatedAt`.

Only one open dispute per milestone is allowed.

### DisputeResolution

One immutable row per resolved dispute:

`Id`, `DisputeId` (unique), `ResolutionType`, `GrossHoldAmount`, `ClientRefundAmount`, `LawyerReleaseAmount`, `PlatformFeeAmount`, `Summary`, `ResolvedByUserId`, `ResolvedAt`, `CreatedAt`.

The four money values must reconcile exactly:

```text
GrossHoldAmount = ClientRefundAmount + LawyerReleaseAmount + PlatformFeeAmount
```

### DisputeEvidence

`Id`, `DisputeId`, `UploadedByUserId`, `StoredFileId?`, `Content?`, `CreatedAt`. At least one of `StoredFileId` or `Content` is required.

### ContractAttachment

`Id`, `ContractId`, `StoredFileId`, `UploadedByUserId`, and `CreatedAt`. Attachments are immutable after the contract becomes active.

### LawyerPenalty

`Id`, `LawyerUserId`, `DisputeId`, `PenaltyType`, `Reason`, `StartsAt`, `EndsAt?`, `CreatedByUserId`, `CreatedAt`. Penalties are hidden from clients.

## 8. Audit, idempotency, and outbox

`ContractStateHistory` and `MilestoneStateHistory` contain aggregate ID, previous state, new state, trigger, actor, reason, correlation ID, and timestamp.

`IdempotencyRecord` contains user, key, operation, request hash, response status/body, and expiry. Reusing a key with a different request hash is a conflict.

`OutboxMessage` contains event type, payload, aggregate type/ID, status, attempts, last error, available-at, and processed-at. It is written in the same transaction as the domain state.

## 9. EF Core configuration rules

- Configure all money with `.HasPrecision(18, 2)`.
- Configure Unicode strings explicitly with maximum lengths.
- Add unique indexes for `Contract.ProposalId`, `Milestone(ContractId, OrderNumber)`, `EscrowHold.MilestoneId`, `MilestoneSubmission(MilestoneId, Version)`, `LawyerWallet.LawyerUserId`, and open-dispute uniqueness.
- Use `DeleteBehavior.Restrict` for contracts, milestones, escrow, payments, disputes, and users.
- Use `DeleteBehavior.Cascade` only for owned attachment join rows where the parent is intentionally removed before financial activity.
- Add check constraints for positive milestone amounts, non-negative ledger/payment amounts, `Currency = 'EGP'`, positive order numbers, valid durations, and valid enum ranges.
- Use `.IsRowVersion()` for aggregate concurrency tokens.
- Add indexes for `(ContractId, Status)`, `(MilestoneId, Status)`, `(Status, AutoAcceptEligibleAt)`, `(HoldExpiresAt, Status)`, `(LawyerUserId, Status)`, `(Dispute.Status, CreatedAt)`, and outbox processing.
- EF Core cannot express the cross-table funded-submission invariant as a simple check constraint. `MilestoneService.SubmitAsync` must enforce it transactionally, and integration tests must verify that direct invalid transitions are rejected.

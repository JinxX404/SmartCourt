# Contracts & Payments — API Contracts and DTOs

## 1. HTTP conventions

Base route: `/api`. All endpoints require JWT authentication unless marked webhook/admin. Responses use the existing wrapper:

```json
{
  "success": true,
  "data": {},
  "message": "optional",
  "errors": null,
  "statusCode": 200
}
```

Validation errors return `400`; unauthorized `401`; forbidden `403`; missing resources `404`; state/concurrency conflicts `409`; provider failures `502` only when the operation cannot be safely retried.

All command endpoints accept `Idempotency-Key`. The key is mandatory for funding, release, refund, withdrawal, and dispute resolution.

## 2. Shared DTOs

```csharp
public sealed record ContractSummaryDto(
    Guid Id, Guid LegalCaseId, Guid ClientUserId, Guid LawyerUserId,
    string Title, string Currency, ContractStatus Status,
    DateTime? ActivatedAt, DateTime? CompletedAt);

public sealed record MilestoneDto(
    Guid Id, int OrderNumber, string Title, string? Description,
    decimal Amount, int? DurationDays, DateTime? DueDate,
    MilestoneStatus Status, MilestoneFundingStatus FundingStatus,
    Guid? EscrowHoldId, DateTime? FundedAt, DateTime? SubmittedAt,
    DateTime? AutoAcceptEligibleAt, DateTime? HoldExpiresAt,
    decimal? NetLawyerAmount);

public sealed record PaymentDto(
    Guid Id, Guid MilestoneId, decimal GrossAmount, decimal PlatformFee,
    decimal NetAmount, string Currency, EscrowHoldStatus Status,
    DateTime? HoldExpiresAt, DateTime? SettledAt);

public sealed record ProblemDto(string Code, string Message, string? Field = null);

public sealed record ContractDetailDto(
    Guid Id, Guid ProposalId, Guid LegalCaseId, Guid ClientUserId, Guid LawyerUserId,
    string Title, string TermsAndConditions, string Currency, ContractStatus Status,
    DateTime? AcceptedByClientAt, DateTime? AcceptedByLawyerAt, DateTime? ActivatedAt,
    DateTime? CompletedAt, DateTime? TerminatedAt, decimal CurrentMilestoneTotal,
    IReadOnlyList<MilestoneDto> Milestones, IReadOnlyList<PaymentDto> Payments,
    IReadOnlyList<string> PermittedActions);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount, bool HasNextPage);

public sealed record ContractStateHistoryDto(
    Guid Id, ContractStatus? PreviousStatus, ContractStatus NewStatus,
    string Trigger, Guid? ActorUserId, string? Reason, DateTime CreatedAt);

public sealed record WalletDto(
    Guid LawyerUserId, string Currency, decimal PendingBalance,
    decimal AvailableBalance, decimal TotalReleased);

public sealed record DisputeDto(
    Guid Id, Guid ContractId, Guid MilestoneId, Guid RaisedByUserId,
    DisputeCategory Category, DisputeStatus Status, string Title,
    string Description, DisputeResolutionType? ResolutionType,
    decimal? ClientRefundAmount, decimal? LawyerReleaseAmount,
    DateTime CreatedAt, DateTime? ResolvedAt);

public sealed record CreateContractRequest(
    Guid ProposalId, string Title, string TermsAndConditions);

public sealed record UpdateContractRequest(
    string Title, string TermsAndConditions);

public sealed record TerminateContractRequest(string Reason);

public sealed record AddMilestoneRequest(
    string Title, string? Description, int OrderNumber, decimal Amount,
    int? DurationDays, DateTime? DueDate);

public sealed record UpdateMilestoneRequest(
    string Title, string? Description, int? DurationDays, DateTime? DueDate);

public sealed record SubmitMilestoneRequest(
    string Notes, IReadOnlyList<Guid> StoredFileIds);

public sealed record RequestMilestoneChangesRequest(string Reason);

public sealed record CreateMilestoneChangeRequest(
    int? ProposedDurationDays, DateTime? ProposedDueDate, string Reason);

public sealed record RejectChangeRequest(string Reason);

public sealed record FundMilestoneRequest(string PaymentMethodReference);

public sealed record CreateWithdrawalRequest(
    decimal Amount, string DestinationReference);

public sealed record CreateDisputeRequest(
    Guid MilestoneId, DisputeCategory Category, string Title,
    string Description, IReadOnlyList<Guid> StoredFileIds);

public sealed record AddDisputeEvidenceRequest(
    string? Content, Guid? StoredFileId);

public sealed record AssignDisputeRequest(Guid ModeratorUserId);

public sealed record ResolveDisputeRequest(
    DisputeResolutionType ResolutionType,
    decimal ClientRefundAmount, decimal LawyerReleaseAmount,
    string Summary, bool ApplyPenalty, PenaltyType? PenaltyType,
    string? PenaltyReason, DateTime? PenaltyEndsAt);

public sealed record ActionResultDto(
    Guid EntityId, string Status, DateTime OccurredAt);
```

### Endpoint response mapping

| Endpoint | Request DTO | Response DTO |
|---|---|---|
| `POST /contracts` | `CreateContractRequest` | `ContractDetailDto` |
| `GET /contracts` | query parameters | `PagedResult<ContractSummaryDto>` |
| `GET /contracts/{id}` | none | `ContractDetailDto` |
| `PUT /contracts/{id}` | `UpdateContractRequest` | `ContractDetailDto` |
| `POST /contracts/{id}/accept` | none | `ActionResultDto` |
| `POST /contracts/{id}/terminate` | `TerminateContractRequest` | `ContractDetailDto` |
| `GET /contracts/{id}/state-history` | none | `IReadOnlyList<ContractStateHistoryDto>` |
| `POST /contracts/{id}/milestones` | `AddMilestoneRequest` | `MilestoneDto` |
| `PUT /contracts/{id}/milestones/{mid}` | `UpdateMilestoneRequest` | `MilestoneDto` |
| `POST /milestones/{id}/approve` | none | `ActionResultDto` |
| `POST /milestones/{id}/ready-for-funding` | none | `ActionResultDto` |
| `POST /milestones/{id}/submit` | `SubmitMilestoneRequest` | `MilestoneDto` |
| `POST /milestones/{id}/accept` | none | `MilestoneDto` |
| `POST /milestones/{id}/request-changes` | `RequestMilestoneChangesRequest` | `MilestoneDto` |
| `POST /milestones/{id}/change-requests` | `CreateMilestoneChangeRequest` | `ActionResultDto` |
| `POST /change-requests/{id}/approve` | none | `ActionResultDto` |
| `POST /change-requests/{id}/reject` | `RejectChangeRequest` | `ActionResultDto` |
| `POST /milestones/{id}/fund` | `FundMilestoneRequest` | `PaymentDto` |
| `GET /contracts/{id}/payments` | none | `IReadOnlyList<PaymentDto>` |
| `GET /milestones/{id}/payment` | none | `PaymentDto` |
| `POST /payments/{id}/retry` | none | `PaymentDto` |
| `GET /wallet` | none | `WalletDto` |
| `POST /wallet/withdrawals` | `CreateWithdrawalRequest` | `ActionResultDto` |
| `POST /payments/webhook` | provider payload | `ActionResultDto` |
| `POST /disputes` | `CreateDisputeRequest` | `DisputeDto` |
| `GET /disputes` | query parameters | `PagedResult<DisputeDto>` |
| `GET /disputes/{id}` | none | `DisputeDto` |
| `POST /disputes/{id}/evidence` | `AddDisputeEvidenceRequest` | `ActionResultDto` |
| `POST /admin/disputes/{id}/assign` | `AssignDisputeRequest` | `DisputeDto` |
| `POST /admin/disputes/{id}/resolve` | `ResolveDisputeRequest` | `DisputeDto` |
| `POST /admin/disputes/{id}/close` | none | `ActionResultDto` |

## 3. Contract endpoints

### `POST /api/contracts`

Request:

```json
{
  "proposalId": "guid",
  "title": "Commercial representation",
  "termsAndConditions": "..."
}
```

Rules: accepted proposal; current lawyer is the proposal lawyer; no existing contract; title 3–200 characters; terms 20–20,000 characters. Creates `Draft`.

Response `201`: `ContractDetailDto` containing participants, terms, acceptances, calculated milestone total, and milestones.

### `GET /api/contracts`

Query: `status`, `page`, `pageSize` (1–100). Returns only contracts where the current user is client/lawyer, ordered by updated time descending.

### `GET /api/contracts/{contractId}`

Returns full contract detail, milestones, current escrow summary, and permitted actions.

### `PUT /api/contracts/{contractId}`

Request:

```json
{ "title": "Updated title", "termsAndConditions": "Updated terms" }
```

Allowed only in `Draft` while neither party has accepted the current version. Uses `If-Match`/row-version conflict handling; any edit resets both acceptance timestamps.

### `POST /api/contracts/{contractId}/accept`

No body. Records the caller’s acceptance. When both parties have accepted, transitions to `Active`.

### `POST /api/contracts/{contractId}/terminate`

Request:

```json
{ "reason": "No agreement on the next milestone" }
```

Allowed by a participant under termination rules. Cancels future milestones and refunds eligible holds. Returns the final contract state and settlement summary.

### `GET /api/contracts/{contractId}/state-history`

Returns append-only state transition records for audit screens.

## 4. Milestone endpoints

### `POST /api/contracts/{contractId}/milestones`

Request:

```json
{
  "title": "Initial filing",
  "description": "Prepare and file the claim",
  "orderNumber": 1,
  "amount": 5000.00,
  "durationDays": 14,
  "dueDate": null
}
```

Rules: participant can propose; order is unique; amount must be greater than zero; duration is 1–365; only the next sequential milestone may enter funding. Every milestone payment is independent; this endpoint never creates a contract-level payable total.

### `PUT /api/contracts/{contractId}/milestones/{milestoneId}`

Allowed only while the milestone is `Draft` and the contract permits editing. Paid/finished milestones cannot be changed.

### `POST /api/milestones/{milestoneId}/approve`

Approves the proposed milestone terms and moves it to `AwaitingFunding`.

### `POST /api/milestones/{milestoneId}/ready-for-funding`

Lawyer-only command. Confirms the deliverable is understood and notifies the client.

### `POST /api/milestones/{milestoneId}/submit`

Multipart or pre-uploaded file IDs:

```json
{
  "notes": "The filing was submitted to court.",
  "storedFileIds": ["guid"]
}
```

Lawyer-only. The milestone must be exactly `FundedInProgress`. In the same transaction, the service must verify `FundedAt`, the matching `Funded` escrow hold, and its completed deposit transaction for this milestone, amount, and EGP currency. It then creates an immutable submission version bound to that `EscrowHoldId`, sets `AutoAcceptEligibleAt = SubmittedAt + 7 days`, and schedules the version-scoped auto-accept job.

If any funding fact is absent or inconsistent, return `409` with code `milestone_not_funded`; do not store a submission or schedule a job.

### `POST /api/milestones/{milestoneId}/accept`

Client-only; milestone must be `Submitted`. Before acceptance, the service repeats the same milestone-specific funding verification used by submission. It then sets the 14-day hold.

### `POST /api/milestones/{milestoneId}/request-changes`

Request:

```json
{ "reason": "Please attach the stamped filing copy." }
```

Returns milestone to `FundedInProgress`, clears `AutoAcceptEligibleAt`, and makes the old auto-accept job stale. The existing successful escrow deposit remains attached to the milestone.

### `POST /api/milestones/{milestoneId}/change-requests`

Request:

```json
{
  "proposedDurationDays": 21,
  "proposedDueDate": null,
  "reason": "Waiting for court papers"
}
```

Changes are mutual and audited. Amount changes are rejected for funded milestones.

### `POST /api/change-requests/{changeRequestId}/approve`

Only the non-requesting participant may approve. Applies proposed values transactionally.

### `POST /api/change-requests/{changeRequestId}/reject`

Request: `{ "reason": "..." }`.

## 5. Payment and escrow endpoints

### `POST /api/milestones/{milestoneId}/fund`

Request:

```json
{ "paymentMethodReference": "mock-card-success" }
```

Client-only. Charges only this milestone amount. The service moves `AwaitingFunding → FundingProcessing`, creates a provider attempt, and on success creates the milestone-specific escrow hold and deposit ledger entry before moving to `FundedInProgress`. A failure returns the milestone to `AwaitingFunding`; it does not make submission or auto-acceptance eligible.

There is intentionally no “fund contract” endpoint.

### `GET /api/contracts/{contractId}/payments`

Returns holds, ledger summaries, provider attempts, fees, refunds, and wallet-related settlement information visible to the caller.

### `GET /api/milestones/{milestoneId}/payment`

Returns the milestone’s payment/hold status and dates.

### `POST /api/payments/{paymentTransactionId}/retry`

Retries a failed provider attempt using a new transaction and the same business idempotency scope.

### `GET /api/wallet`

Lawyer-only. Returns pending, available, and total released balances.

### `POST /api/wallet/withdrawals`

Request:

```json
{ "amount": 1000.00, "destinationReference": "mock-bank-account" }
```

Requires sufficient available balance. Mock provider returns a deterministic result.

### `POST /api/payments/webhook`

Provider-only endpoint. Validates signature/configured mock secret, deduplicates event IDs, and updates the provider attempt.

For deposit success, the handler verifies the callback’s milestone, amount, currency, provider transaction ID, and current `FundingProcessing` state before creating the hold and transitioning to `FundedInProgress`.

## 6. Dispute endpoints

### `POST /api/disputes`

Request:

```json
{
  "milestoneId": "guid",
  "category": "DeliverableQuality",
  "title": "The filing is unusable",
  "description": "...",
  "storedFileIds": ["guid"]
}
```

Participant-only; milestone must be in `AcceptedHold` and the hold must not have expired. Freezes the hold and creates `Open`.

### `GET /api/disputes`

Participant view of their disputes; moderator view supports status and assignment filters.

### `GET /api/disputes/{disputeId}`

Returns dispute, evidence, milestone, financial hold, and permitted actions.

### `POST /api/disputes/{disputeId}/evidence`

Adds immutable text/file evidence while `Open`, `Assigned`, or `UnderReview`.

### `POST /api/admin/disputes/{disputeId}/assign`

Request: `{ "moderatorUserId": "guid" }`.

Admin/moderator only.

### `POST /api/admin/disputes/{disputeId}/resolve`

Request:

```json
{
  "resolutionType": "PartialSplit",
  "clientRefundAmount": 2500.00,
  "lawyerReleaseAmount": 2250.00,
  "summary": "..."
}
```

The two amounts plus the platform fee treatment must reconcile exactly to the gross hold. The operation is idempotent and creates immutable settlement entries.

### `POST /api/admin/disputes/{disputeId}/close`

Closes a resolved dispute after notifications and settlement reconciliation succeed.

## 7. FluentValidation rules

- All IDs must be non-empty.
- Titles: 3–200 characters.
- Descriptions/reasons: required where stated, max 2,000–20,000 according to DTO.
- Milestone amounts: EGP, scale two, and strictly greater than zero.
- Duration: 1–365 days.
- File IDs must belong to the current user or be explicitly authorized.
- Fund validates that the milestone is the next sequential `AwaitingFunding` milestone and that no other milestone has an unsettled funded hold.
- Submit requires `FundedInProgress`, `FundedAt`, a matching `Funded` hold, and a completed deposit transaction for the exact milestone amount/currency.
- Accept requires `Submitted` and revalidates the same funded hold and current submission version.
- No request DTO accepts `FundingStatus`, `FundedAt`, `EscrowHoldId`, or `AutoAcceptEligibleAt`; these are server-owned values.
- Partial resolution amounts cannot be negative and must reconcile with the escrow hold.
- A request with a reused idempotency key must have the same request hash.

## 8. Auto-accept job contract

```csharp
public sealed record AutoAcceptMilestoneJobArgs(
    Guid MilestoneId,
    Guid EscrowHoldId,
    int SubmissionVersion);
```

`AutoAcceptMilestoneAsync` is an internal job method, not a public API. It performs the full eligibility checks documented in `03_state_machines.md`. On any mismatch it completes as an idempotent no-op. Only a valid current funded submission can transition to `AcceptedHold` and emit `MilestoneAutoAccepted`.

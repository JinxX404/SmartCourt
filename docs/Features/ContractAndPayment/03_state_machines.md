# Contracts & Payments — State Machines

## 0. Exact v1 enums

```csharp
public enum ContractStatus
{
    Draft = 0,
    Active = 1,
    SuspendedByDispute = 2,
    Completed = 3,
    Terminated = 4
}

public enum MilestoneStatus
{
    Draft = 0,
    AwaitingFunding = 1,
    InProgress = 2,
    Submitted = 3,
    AcceptedHold = 4,
    Disputed = 5,
    Released = 6,
    Refunded = 7,
    Cancelled = 8
}

public enum EscrowHoldStatus
{
    PendingFunding = 0,
    Funded = 1,
    Frozen = 2,
    Released = 3,
    Refunded = 4
}

public enum DisputeStatus
{
    Open = 0,
    Assigned = 1,
    UnderReview = 2,
    Resolved = 3,
    Closed = 4
}

public enum DisputeResolutionType
{
    FullRefund = 0,
    FullRelease = 1,
    PartialSplit = 2
}

public enum ChangeRequestStatus { Pending = 0, Approved = 1, Rejected = 2, Cancelled = 3 }
public enum PenaltyType { Warning = 0, Suspension12Months = 1, Suspension24Months = 2, PermanentTermination = 3 }
```

## 1. Contract states

```text
Draft
  └─ both parties accept ─> Active
Active
  ├─ all required milestones released ─> Completed
  ├─ graceful termination ─> Terminated
  └─ active dispute ─> SuspendedByDispute
SuspendedByDispute
  ├─ dispute resolved ─> Active
  ├─ dispute resolves contract termination ─> Terminated
  └─ all milestones released ─> Completed
```

`Draft` is editable. `Active` permits milestone work and rolling milestone negotiation. `Completed` and `Terminated` are terminal.

### Contract triggers

| From | Trigger | To | Preconditions |
|---|---|---|---|
| none | Create from accepted proposal | `Draft` | Proposal accepted; unique contract absent |
| `Draft` | Client accepts | `Draft` | Current client; terms valid |
| `Draft` | Lawyer accepts | `Draft` | Current lawyer; terms valid |
| `Draft` | Both acceptance timestamps present | `Active` | At least one valid milestone |
| `Active` | Open financial dispute | `SuspendedByDispute` | Dispute targets funded milestone |
| `SuspendedByDispute` | Non-terminal dispute resolution | `Active` | Hold settled without terminating contract |
| `Active`/`SuspendedByDispute` | All priced milestones released/refunded and no work remains | `Completed` | Settlement checks pass |
| `Draft`/`Active` | Graceful termination | `Terminated` | Termination rules pass; future work cancelled |

Contract state is derived from explicit service commands, not from client-provided status values.

## 2. Milestone states

```text
Draft
  └─ mutual milestone approval ─> AwaitingFunding
AwaitingFunding
  ├─ deposit succeeds ─> InProgress
  └─ deposit fails ─> AwaitingFunding
InProgress
  ├─ lawyer submits ─> Submitted
  ├─ approved extension ─> InProgress
  └─ cancellation/refund ─> Cancelled or Refunded
Submitted
  ├─ client requests changes ─> InProgress
  ├─ client accepts / auto-accepts ─> AcceptedHold
  └─ eligible dispute after acceptance ─> Disputed
AcceptedHold
  ├─ dispute opened ─> Disputed
  └─ hold expires ─> Released
Disputed
  ├─ client refund outcome ─> Refunded
  ├─ lawyer release outcome ─> Released
  └─ partial settlement ─> Released
```

`Draft`, `AwaitingFunding`, `InProgress`, and `Submitted` are mutable workflow states. `AcceptedHold`, `Released`, `Refunded`, and `Cancelled` become commercially immutable; corrections use compensating records.

### Milestone triggers

| From | Trigger | To | Preconditions |
|---|---|---|---|
| none | Add milestone | `Draft` | Contract `Draft` or rolling active milestone proposal |
| `Draft` | Client and lawyer approve milestone terms | `AwaitingFunding` | Title, deliverable, amount, ordering valid |
| `AwaitingFunding` | Successful client funding | `InProgress` | Previous milestone settled; amount > 0 |
| `AwaitingFunding` | Provider failure | `AwaitingFunding` | Failed attempt recorded |
| `InProgress` | Lawyer submits deliverable | `Submitted` | Hold exists; attachment/notes valid |
| `Submitted` | Client requests changes | `InProgress` | Reason required |
| `Submitted` | Client accepts | `AcceptedHold` | Acceptance recorded; hold dates set |
| `Submitted` | Seven-day auto-accept job | `AcceptedHold` | No client action; job records system actor |
| `AcceptedHold` | Open dispute | `Disputed` | Funded hold not expired |
| `AcceptedHold` | Hold expiry job | `Released` | No open dispute; release idempotency check passes |
| `Disputed` | Full/partial lawyer outcome | `Released` | Resolution approved and ledger settled |
| `Disputed` | Client refund outcome | `Refunded` | Provider refund and ledger settlement recorded |
| Any unsettled state | Valid termination | `Cancelled` or `Refunded` | Refund/cancellation rules applied |

## 3. Change-request states

`Pending → Approved`, `Pending → Rejected`, and `Pending → Cancelled`.

Only the non-requesting participant approves. A request may change description, duration, and due date. A funded amount cannot be changed; a price change requires cancellation/refund and a replacement milestone.

## 4. Escrow-hold states

`PendingFunding → Funded → Frozen → Released` or `Refunded`.

- `PendingFunding`: hold record is being prepared.
- `Funded`: provider deposit succeeded.
- `Frozen`: dispute is open or administrator has placed a temporary financial freeze.
- `Released`: net funds moved to lawyer available balance.
- `Refunded`: client refund completed.

Provider processing failures do not create a terminal state; the hold remains retryable and the failed `PaymentTransaction` is retained.

## 5. Dispute states

`Open → Assigned → UnderReview → Resolved → Closed`.

Resolution is immutable. Reopening is not supported in v1; an appeal is a new administrative case linked to the original dispute if later required.

## 6. Transition invariants

- A milestone cannot be `InProgress` without a successful escrow deposit.
- A contract cannot be `Completed` while a milestone is funded, disputed, or awaiting settlement.
- A release/refund transition must create a matching immutable ledger entry.
- A milestone cannot be edited after acceptance, release, refund, or cancellation.
- A new milestone cannot be funded while another milestone is `InProgress`, `Submitted`, `AcceptedHold`, or `Disputed`.
- A dispute cannot be opened after `HoldExpiresAt`.
- Every transition stores actor, trigger, reason, timestamp, and correlation ID.

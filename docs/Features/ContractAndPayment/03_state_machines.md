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
    FundingProcessing = 2,
    FundedInProgress = 3,
    Submitted = 4,
    AcceptedHold = 5,
    Disputed = 6,
    Released = 7,
    Refunded = 8,
    Cancelled = 9
}

public enum EscrowHoldStatus
{
    Funded = 0,
    Frozen = 1,
    Released = 2,
    Refunded = 3
}

public enum MilestoneFundingStatus
{
    Unfunded = 0,
    Processing = 1,
    Funded = 2,
    Settled = 3
}

public enum MilestoneAcceptanceSource { Manual = 0, Automatic = 1 }

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
  └─ client starts milestone payment ─> FundingProcessing
FundingProcessing
  ├─ deposit succeeds for this milestone ─> FundedInProgress
  └─ deposit fails/times out ─> AwaitingFunding
FundedInProgress
  ├─ lawyer submits ─> Submitted
  ├─ approved extension ─> FundedInProgress
  └─ cancellation/refund ─> Cancelled or Refunded
Submitted
  ├─ client requests changes ─> FundedInProgress
  ├─ client accepts / auto-accepts ─> AcceptedHold
AcceptedHold
  ├─ dispute opened ─> Disputed
  └─ hold expires ─> Released
Disputed
  ├─ client refund outcome ─> Refunded
  ├─ lawyer release outcome ─> Released
  └─ partial settlement ─> Released
```

`Draft`, `AwaitingFunding`, `FundingProcessing`, `FundedInProgress`, and `Submitted` are mutable workflow states. `AcceptedHold`, `Released`, `Refunded`, and `Cancelled` become commercially immutable; corrections use compensating records.

### Milestone triggers

| From | Trigger | To | Preconditions |
|---|---|---|---|
| none | Add milestone | `Draft` | Contract `Draft` or rolling active milestone proposal |
| `Draft` | Client and lawyer approve milestone terms | `AwaitingFunding` | Title, deliverable, positive amount, ordering valid |
| `AwaitingFunding` | Client starts milestone payment | `FundingProcessing` | Previous milestone settled; this milestone amount > 0 |
| `FundingProcessing` | Provider deposit succeeds | `FundedInProgress` | Completed deposit and funded `EscrowHold` reference this exact milestone, amount, and currency |
| `FundingProcessing` | Provider confirms failure | `AwaitingFunding` | Failed attempt recorded; no funded hold exists |
| `FundingProcessing` | Provider result unknown | `FundingProcessing` | Reconciliation/webhook must determine outcome before retry |
| `FundedInProgress` | Lawyer submits deliverable | `Submitted` | `FundedAt` set; matching hold is `Funded`; deposit transaction completed; attachment/notes valid |
| `Submitted` | Client requests changes | `FundedInProgress` | Reason required; auto-accept eligibility cleared |
| `Submitted` | Client accepts | `AcceptedHold` | Current submission and its funded hold revalidated; hold dates set |
| `Submitted` | Seven-day auto-accept job | `AcceptedHold` | Funding, hold, submission version, deadline, and current state all revalidated |
| `AcceptedHold` | Open dispute | `Disputed` | Funded hold not expired |
| `AcceptedHold` | Hold expiry job | `Released` | No open dispute; release idempotency check passes |
| `Disputed` | Full/partial lawyer outcome | `Released` | Resolution approved and ledger settled |
| `Disputed` | Client refund outcome | `Refunded` | Provider refund and ledger settlement recorded |
| Any unsettled state | Valid termination | `Cancelled` or `Refunded` | Refund/cancellation rules applied |

## 3. Change-request states

`Pending → Approved`, `Pending → Rejected`, and `Pending → Cancelled`.

Only the non-requesting participant approves. A request may change description, duration, and due date. A funded amount cannot be changed; a price change requires cancellation/refund and a replacement milestone.

## 4. Escrow-hold states

`Funded → Frozen → Released` or `Refunded`.

- `Funded`: provider deposit succeeded for the hold’s exact milestone.
- `Frozen`: dispute is open or administrator has placed a temporary financial freeze.
- `Released`: net funds moved to lawyer available balance.
- `Refunded`: client refund completed.

Provider processing failures do not create a terminal state; the hold remains retryable and the failed `PaymentTransaction` is retained.

## 5. Auto-accept eligibility and job transition

The auto-accept job is created only after `SubmitAsync` has successfully committed a funded submission. Its arguments are:

```text
MilestoneId + EscrowHoldId + SubmissionVersion
```

At execution, the job opens a transaction and requires every condition below:

1. Milestone state is exactly `Submitted`.
2. `FundedAt` is non-null.
3. `AutoAcceptEligibleAt` is non-null and `UtcNow >= AutoAcceptEligibleAt`.
4. The current submission version matches the job argument.
5. The submission references the same `EscrowHoldId` supplied to the job.
6. The hold belongs to the milestone and is exactly `Funded`.
7. The hold’s completed deposit transaction matches milestone ID, EGP amount, and currency.
8. No manual acceptance, change request, refund, release, cancellation, or dispute has superseded the submission.

If any condition fails, the job performs no state transition, creates no release schedule, and emits no `MilestoneAutoAccepted` event. It records a safe no-op result for monitoring. A resubmission creates a new version and a new seven-day deadline; earlier jobs become stale.

`MilestoneFundingStatus` is a read-model value derived from state plus the related hold:

- `Unfunded`: no successful hold (`Draft`, `AwaitingFunding`, or a never-funded cancellation).
- `Processing`: `FundingProcessing`.
- `Funded`: a valid unsettled hold exists for `FundedInProgress`, `Submitted`, `AcceptedHold`, or `Disputed`.
- `Settled`: the milestone-specific hold is `Released` or `Refunded`.

## 6. Dispute states

`Open → Assigned → UnderReview → Resolved → Closed`.

Resolution is immutable. Reopening is not supported in v1; an appeal is a new administrative case linked to the original dispute if later required.

## 7. Transition invariants

- Each milestone is funded separately; funding one milestone never funds any other milestone or the contract as a whole.
- A milestone cannot be `FundedInProgress` without a completed deposit and funded escrow hold for that exact milestone.
- A milestone cannot be `Submitted` unless it is transitioning directly from `FundedInProgress` and the service revalidates the successful deposit and hold.
- `AutoAcceptEligibleAt` can be populated only when a valid funded submission is created.
- Auto-accept cannot run for `Draft`, `AwaitingFunding`, `FundingProcessing`, `FundedInProgress`, or any terminal milestone.
- A contract cannot be `Completed` while a milestone is funded, disputed, or awaiting settlement.
- A release/refund transition must create a matching immutable ledger entry.
- A milestone cannot be edited after acceptance, release, refund, or cancellation.
- A new milestone cannot be funded while another milestone is `FundingProcessing`, `FundedInProgress`, `Submitted`, `AcceptedHold`, or `Disputed`.
- A dispute cannot be opened after `HoldExpiresAt`.
- Every transition stores actor, trigger, reason, timestamp, and correlation ID.

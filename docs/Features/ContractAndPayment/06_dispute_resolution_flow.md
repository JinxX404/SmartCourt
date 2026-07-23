# Contracts & Payments — Dispute Resolution Flow

## 1. Purpose and eligibility

A dispute protects either participant when an accepted funded milestone is not delivered, is unusable, or is materially misrepresented. The primary v1 use case is a client dispute during the 14-day hold. A lawyer may also raise a dispute against an accepted hold for a contract/payment issue.

Eligibility:

- The milestone is in `AcceptedHold` with a successful escrow hold.
- The hold has not expired.
- No other open dispute exists for the milestone.
- The requester is the contract client or lawyer.

After hold expiry, a complaint may still be reported to moderation, but it does not automatically freeze or recover withdrawn funds.

## 2. Trigger

The UI displays `Raise dispute` on the milestone while it is in `AcceptedHold`. The API validates the same rule; hiding a button is not a security control.

The request includes:

- Milestone ID.
- Category.
- Title and factual description.
- Optional stored-file evidence.
- Requested outcome: refund, release, or review.

The service creates the dispute, freezes the hold, changes the milestone to `Disputed`, and emits `DisputeOpened` in one transaction.

## 3. Investigation workflow

```text
Open
  -> Assigned
  -> UnderReview
  -> Resolved
  -> Closed
```

### Open

The system notifies both participants, cancels the pending release job, and preserves the original hold expiry for audit. New evidence can be added.

### Assigned

A moderator is assigned. Assignment does not alter historical evidence or balances.

### UnderReview

The moderator receives read-only access to:

- Proposal and contract terms.
- Existing proposal conversation and system messages.
- Milestone submissions and attachment metadata.
- Change requests and acceptance timestamps.
- Funding, fee, hold, and provider transaction history.
- Dispute evidence submitted by either party.

The moderator may request additional evidence through notifications. v1 does not create a separate dispute chat.

### Resolved

The moderator records:

- Decision summary.
- Resolution type.
- Client refund amount.
- Lawyer release amount.
- Fee treatment.
- Optional penalty recommendation.

The service validates reconciliation before changing money:

```text
gross hold = client refund + lawyer release + platform fee
```

The platform fee is not earned on the refunded portion. A full client refund therefore has zero platform fee and zero lawyer release.

### Closed

The dispute closes only after all provider and ledger operations are successful or have a recorded, retryable administrative failure.

## 4. Resolution outcomes

### Full refund

- Refund the gross hold to the client through the payment provider.
- Remove the lawyer’s pending balance for that hold.
- Add a refund ledger entry.
- Mark milestone `Refunded`.
- Do not charge the platform fee.

### Full release

- Release the gross hold through the normal lawyer settlement path.
- Deduct the 5% fee.
- Move the net amount to the lawyer’s available balance.
- Mark milestone `Released`.

### Partial split

- Refund the moderator-approved client amount.
- Release the moderator-approved lawyer amount.
- Deduct the fee only from the non-refunded portion.
- Verify amounts reconcile before committing.
- Mark milestone `Released` because part of the work was financially accepted.

## 5. Contract impact

Opening a dispute changes the contract to `SuspendedByDispute` only for workflow purposes. No new milestone can be funded while the challenged milestone is unresolved. Unrelated already-settled milestones remain valid.

After resolution:

- Non-terminating outcomes return the contract to `Active`.
- A resolution that ends the relationship changes the contract to `Terminated`.
- Future unstarted milestones are cancelled during termination.
- A new lawyer receives no inherited milestone obligations.

## 6. Penalties and flags

Penalty selection is manual and restricted to an authorized administrator:

- `Warning`: hidden internal flag.
- `Suspension12Months`: account cannot accept new work or withdraw newly earned funds according to platform policy.
- `Suspension24Months`.
- `PermanentTermination`.

Each penalty records the dispute, reason, actor, start/end dates, and any appeal reference. Penalties are not exposed to the opposing party through normal APIs. Automated strike thresholds are intentionally excluded from v1.

## 7. Security and evidence rules

- Moderators cannot edit chat messages, submissions, ledger entries, or provider transactions.
- Evidence is append-only; a correction is a new evidence item.
- Every moderator view and action is audited.
- Personal/legal files use the existing file authorization and signed-access mechanism.
- Dispute descriptions and evidence are retained with the contract for legal/audit purposes.
- The system must not expose internal penalty details or another user’s wallet data.

## 8. Failure handling

- Provider refund/release failure leaves the dispute `Resolved` with settlement processing status and schedules retry.
- Duplicate resolution requests return the original resolution using the idempotency key.
- A concurrency conflict causes the moderator to reload; no partial database state is committed.
- If a hold expiry job races with dispute creation, the database transaction that first settles the hold wins; the loser returns a conflict and does not create a second financial movement.

## 9. Notifications

Notify both parties on dispute opened, moderator assigned, evidence requested, resolved, settlement completed, and closed. The moderator receives assignment and retry alerts. Notification payloads include dispute and milestone IDs, not sensitive evidence content.

# Proposal lifecycle

Frontend/API consumers should use the complete
[frontend integration guide](./frontend_integration_guide.md).

## Business rules

- Proposals can be created only for a case in `Matched` status.
- A case can have at most five active proposals. `Pending` and `Accepted`
  proposals consume a slot.
- A pending proposal expires 72 hours after creation. The recurring expiration
  job runs every minute, and proposal creation also expires overdue records
  before counting slots.
- Only the invited lawyer can accept or reject a pending proposal.
- The client can cancel a pending proposal.
- Acceptance opens one private client-lawyer conversation. It starts
  negotiation and does not assign the case.
- Either participant can terminate an accepted proposal when it has no open
  contract. The conversation becomes read-only and retains a system message
  explaining the closure.
- Activating a contract assigns its lawyer to the case and supersedes every
  other pending or accepted proposal for that case.

## Proposal statuses

| Status | Consumes a slot | Meaning |
| --- | --- | --- |
| `Pending` | Yes | Waiting for the lawyer until `expiresAt`. |
| `Accepted` | Yes | Negotiation is active and chat is open. |
| `Rejected` | No | The invited lawyer declined. |
| `Cancelled` | No | The client withdrew a pending invitation. |
| `Expired` | No | No response was received within 72 hours. |
| `Terminated` | No | A participant ended an accepted negotiation. |
| `Superseded` | No | Another contract was activated for the case. |

## Endpoints

| Method | Route | Actor |
| --- | --- | --- |
| `POST` | `/api/proposals` | Client |
| `GET` | `/api/proposals/lawyer` | Lawyer |
| `GET` | `/api/proposals/cases/{caseId}` | Owning client |
| `GET` | `/api/proposals/{proposalId}` | Proposal participant |
| `GET` | `/api/proposals/cases/{caseId}/availability` | Owning client |
| `POST` | `/api/proposals/{proposalId}/accept` | Invited lawyer |
| `POST` | `/api/proposals/{proposalId}/reject` | Invited lawyer |
| `POST` | `/api/proposals/{proposalId}/cancel` | Owning client |
| `POST` | `/api/proposals/{proposalId}/terminate` | Proposal participant |

Cancellation, rejection, and termination bodies contain a required `reason`
with a maximum length of 1,000 characters. Proposal responses include
`expiresAt`, `closedAt`, `closedByUserId`, `decisionReason`, and
`conversationId`.

The availability response is authoritative for display, but the create command
always enforces the limit again under a serializable database transaction.

Proposal lists default to page 1, page size 5, and `Pending` status. Page size
is limited to 50. Repeat the `statuses` query parameter to combine statuses,
for example `?statuses=Pending&statuses=Accepted`.

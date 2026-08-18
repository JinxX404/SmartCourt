# Proposals and chat frontend integration guide

This document is the frontend contract for the proposal lifecycle. The runtime
API and DTOs under `SmartCourt/Features/Proposals` remain the final authority.

## Authentication

Every endpoint requires a JWT access token:

```http
Authorization: Bearer <access-token>
```

The server always derives the user ID and role from the token. The frontend
must not send a client ID, lawyer ID, or inbox direction to select whose data
is returned.

## Breaking list-route change

The combined endpoint below was removed:

```http
GET /api/proposals
```

Use the role-specific endpoints instead:

```http
GET /api/proposals/lawyer
GET /api/proposals/cases/{legalCaseId}
```

## Pagination and filtering

Both proposal list endpoints accept:

| Query | Type | Default | Rules |
| --- | --- | --- | --- |
| `statuses` | repeated `ProposalStatus` | `Pending` | Multiple values are ORed. |
| `search` | string | none | Maximum 100 characters. |
| `page` | integer | `1` | Minimum 1. |
| `pageSize` | integer | `5` | From 1 through 50. |

Send multiple statuses by repeating the query key:

```http
?statuses=Pending&statuses=Accepted&page=1&pageSize=5
```

Within one filter, values use OR logic. Different filters use AND logic:

```text
(status is Pending OR Accepted) AND search matches
```

When `statuses` is omitted or empty, only pending proposals are returned. To
build an All tab, send every status explicitly.

## Proposal statuses

| Status | Meaning | Consumes one of five slots |
| --- | --- | --- |
| `Pending` | Waiting for the lawyer before `expiresAt`. | Yes |
| `Accepted` | The lawyer accepted and negotiation is active. | Yes |
| `Rejected` | The lawyer declined. | No |
| `Cancelled` | The client withdrew a pending proposal. | No |
| `Expired` | No response was received within 72 hours. | No |
| `Terminated` | A participant ended an accepted negotiation. | No |
| `Superseded` | Another proposal's contract was activated. Chat is hidden from the affected lawyer. | No |

## 1. Lawyer proposal inbox

```http
GET /api/proposals/lawyer
```

Role: `Lawyer`.

This returns proposals addressed to the authenticated lawyer across cases.
The default request returns pending proposals:

```http
GET /api/proposals/lawyer?page=1&pageSize=5
```

For active negotiations:

```http
GET /api/proposals/lawyer?statuses=Accepted&page=1&pageSize=5
```

For a combined inbox:

```http
GET /api/proposals/lawyer?statuses=Pending&statuses=Accepted&search=employment
```

## 2. Client proposals for one case

```http
GET /api/proposals/cases/{legalCaseId}
```

Role: `Client`.

The case must belong to the authenticated client. The endpoint never returns
proposals from another case or another client.

```http
GET /api/proposals/cases/11111111-1111-1111-1111-111111111111?statuses=Pending&statuses=Accepted&page=1&pageSize=5
```

## List response

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "proposal-id",
        "legalCaseId": "case-id",
        "caseTitle": "Employment dues dispute",
        "clientUserId": "client-id",
        "clientName": "Ahmed Ali",
        "lawyerUserId": "lawyer-id",
        "lawyerName": "Mona Hassan",
        "status": "Accepted",
        "caseStatus": "Assigned",
        "assignedLawyerUserId": "lawyer-id",
        "isAssignedLawyer": true,
        "contractId": "contract-id",
        "contractStatus": "Active",
        "conversationId": "conversation-id",
        "conversationStatus": "Open",
        "canChat": true,
        "permittedActions": ["OpenChat", "ViewContract"],
        "createdAt": "2026-08-10T18:00:00Z",
        "respondedAt": "2026-08-10T19:00:00Z",
        "expiresAt": "2026-08-13T18:00:00Z",
        "closedAt": null,
        "closedByUserId": null
      }
    ],
    "page": 1,
    "pageSize": 5,
    "totalCount": 1,
    "hasNextPage": false
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```

## Workflow fields

| Field | Frontend meaning |
| --- | --- |
| `status` | Proposal lifecycle status. |
| `caseStatus` | Current case state, such as `Matched` or `Assigned`. |
| `assignedLawyerUserId` | Lawyer actually assigned after contract activation. |
| `isAssignedLawyer` | Whether this proposal belongs to the assigned lawyer. |
| `contractId` | Contract created from this proposal, or `null`. |
| `contractStatus` | Current contract status, or `null`. |
| `conversationId` | Chat created for this proposal, or `null`. |
| `conversationStatus` | `Open`, `Closed`, or `null` before acceptance. |
| `canChat` | Authoritative send-message flag. |
| `permittedActions` | Buttons/actions allowed for the current user. |

Do not calculate assignment by checking only `status == Accepted`. Acceptance
starts negotiation. The case is assigned only after a contract becomes active.

## Permitted actions

Possible values are:

```text
Accept
Reject
Cancel
TerminateProposal
CreateContract
ViewContract
OpenChat
ViewChatHistory
```

Render mutation buttons from `permittedActions`. This avoids duplicating
contract and chat rules in the browser. The server still validates every
action when submitted.

## 3. Get proposal details

```http
GET /api/proposals/{proposalId}
```

Roles: `Client`, `Lawyer`.

Only a participant can access the proposal. The detail has all list workflow
fields plus the original `message`, `decisionReason`, and `updatedAt`.

## 4. Check available proposal slots

```http
GET /api/proposals/cases/{legalCaseId}/availability
```

Role: `Client`.

```json
{
  "success": true,
  "data": {
    "legalCaseId": "case-id",
    "activeProposalCount": 3,
    "proposalLimit": 5,
    "availableProposalSlots": 2,
    "canSendProposal": true
  },
  "statusCode": 200
}
```

Use this to disable the Send proposal button, but still handle `409` from the
create endpoint because another request may consume a slot afterward.

## 5. Create proposal

```http
POST /api/proposals
Content-Type: application/json
```

Role: `Client`.

```json
{
  "legalCaseId": "case-id",
  "lawyerUserId": "lawyer-id",
  "message": "I would like to discuss representation for this case."
}
```

The case must be `Matched`. The message is required and has a 2,000-character
maximum. A case may have at most five pending or accepted proposals, and only
one active proposal for the same case-lawyer pair. Success returns `201`.

## 6. Accept proposal

```http
POST /api/proposals/{proposalId}/accept
```

Role: `Lawyer`. No request body.

Acceptance creates exactly one conversation and returns its `conversationId`.
It does not assign the case. The case remains `Matched` until a contract is
accepted by both participants and activated.

### Accept only (start an inquiry)

Call only the accept endpoint. On success, navigate to the returned
`conversationId` and allow the lawyer and client to discuss the case.

### Accept and immediately submit a contract draft

This is intentionally a two-step frontend workflow, not one combined request:

```http
POST /api/proposals/{proposalId}/accept
```

After that request succeeds, immediately call:

```http
POST /api/contracts
Content-Type: application/json
```

```json
{
  "proposalId": "proposal-id",
  "title": "Legal representation agreement",
  "termsAndConditions": "The complete proposed representation terms."
}
```

Successful contract creation returns HTTP `201 Created`.

Keep contract validation and errors attached to the second step. If contract
creation fails, the accepted proposal and conversation remain valid; show the
error and allow the lawyer to correct and resubmit the draft. Do not call the
accept endpoint a second time.

## 7. Reject proposal

```http
POST /api/proposals/{proposalId}/reject
Content-Type: application/json
```

Role: `Lawyer`.

```json
{
  "reason": "I am unavailable for this case."
}
```

The reason is required and has a 1,000-character maximum. Rejection never
creates a chat and frees a proposal slot.

## 8. Cancel proposal

```http
POST /api/proposals/{proposalId}/cancel
Content-Type: application/json
```

Role: `Client`.

```json
{
  "reason": "I decided to invite another lawyer."
}
```

Only a pending proposal can be cancelled. Cancellation never creates a chat
and frees a proposal slot.

## 9. Terminate negotiation

```http
POST /api/proposals/{proposalId}/terminate
Content-Type: application/json
```

Roles: `Client`, `Lawyer`.

```json
{
  "reason": "We could not agree on the contract terms."
}
```

Only an accepted proposal without an open contract can be terminated. The
conversation becomes closed/read-only and remains available as history.

## Client UI rules

| State | Recommended UI |
| --- | --- |
| Pending | Waiting state and Cancel button. |
| Accepted without contract | Open chat and Terminate negotiation. |
| Accepted with draft/active contract | Open chat and View contract. |
| Selected active lawyer | Show Assigned lawyer using `isAssignedLawyer`. |
| Rejected/cancelled/expired | Show terminal reason/status; no chat button. |
| Superseded | Client may retain read-only history; the affected lawyer cannot access it. |
| Terminated | Show read-only chat history when available. |

## Lawyer UI rules

| State | Recommended UI |
| --- | --- |
| Pending | Accept and Reject buttons. |
| Accepted without contract | Open chat, Create contract, or Terminate. |
| Accepted with contract | Open chat and View contract. |
| Assigned through this proposal | Show Active client relationship. |
| Superseded | Show the proposal status and notification only. Remove all chat UI and identifiers. |
| Terminated | Show read-only history; no message composer. |

## Chat traceability and lifecycle

Every conversation stores and returns:

```text
conversation.id
conversation.proposalId
conversation.legalCaseId
```

The database enforces one conversation per proposal. Both `proposalId` and
`legalCaseId` are foreign keys, so support and dispute flows can trace:

```text
case -> proposal -> conversation -> messages
                  -> contract -> milestones/payments/disputes
```

Chat lifecycle:

```text
Pending / Rejected / Cancelled / Expired -> no conversation
Accepted                                 -> open conversation
Proposal Terminated                      -> closed conversation
Proposal Superseded                      -> closed conversation
Contract Completed                       -> closed conversation
Contract Terminated                      -> closed conversation
```

Closed conversations normally remain readable by both participants. A
superseded conversation is the privacy exception: it remains available to the
client but is completely hidden from the affected lawyer.

For a superseded lawyer, the backend:

- returns `null` for `conversationId` and `conversationStatus` in proposal DTOs;
- never returns `OpenChat` or `ViewChatHistory` in `permittedActions`;
- excludes the conversation from `GET /api/chat/conversations`;
- returns `404` for conversation detail, message history, and message sending;
- rejects SignalR `JoinConversation` with `Conversation was not found.`;
- creates a `proposal.superseded` notification explaining that the case was
  assigned to another lawyer and the negotiation is no longer available.

Use `404`, rather than revealing that a hidden conversation exists. Do not
cache message history after a proposal becomes superseded; remove that
conversation from the lawyer's client-side state when the notification arrives
or when proposal data refreshes.

Other closed conversations are still readable. REST and SignalR message
sending are enforced by the backend; sending to an ordinary closed
conversation returns `409`.

Chat endpoints:

```http
GET  /api/chat/conversations
GET  /api/chat/conversations/{conversationId}
GET  /api/chat/conversations/{conversationId}/messages
POST /api/chat/conversations/{conversationId}/messages
POST /api/chat/conversations/{conversationId}/attachments
GET  /api/chat/conversations/{conversationId}/attachments/{attachmentId}/download
```

### Send attachment message

```http
POST /api/chat/conversations/{conversationId}/attachments
Content-Type: multipart/form-data
```

Roles: `Client`, `Lawyer`. The caller must be a participant and the proposal
conversation must still be open. Use these multipart field names:

| Field | Required | Rules |
| --- | --- | --- |
| `caption` | No | Text shown with the files; maximum 2,000 characters. |
| `files` | Yes | Repeat for each file; from 1 through 5 files. |

Allowed formats are PDF, DOCX, TXT, PNG, and JPEG. Each file is limited to
10 MB and the combined files are limited to 25 MB. The backend verifies the
file content instead of trusting the browser-provided MIME type or extension.

```ts
const body = new FormData();
if (caption.trim()) body.append("caption", caption.trim());
for (const file of selectedFiles) body.append("files", file);

const response = await fetch(
  `${apiBase}/api/chat/conversations/${conversationId}/attachments`,
  {
    method: "POST",
    headers: { Authorization: `Bearer ${token}` },
    body,
  },
);
```

Do not manually set the `Content-Type` request header; the browser must add
the multipart boundary. Success is `201 Created` and returns the created
message:

```json
{
  "success": true,
  "data": {
    "id": "message-id",
    "conversationId": "conversation-id",
    "senderUserId": "client-or-lawyer-user-id",
    "senderName": "Ahmed Ali",
    "type": "User",
    "content": "Evidence for review.",
    "systemCode": null,
    "relatedEntityId": null,
    "createdAt": "2026-08-11T13:30:00Z",
    "isMine": true,
    "attachments": [
      {
        "id": "attachment-id",
        "fileName": "evidence.pdf",
        "contentType": "application/pdf",
        "sizeInBytes": 184320,
        "downloadUrl": "/api/chat/conversations/conversation-id/attachments/attachment-id/download"
      }
    ]
  },
  "statusCode": 201
}
```

If no caption is provided, `content` contains a server-generated attachment
label. Use the `attachments` array, not message text parsing, to render files.
Every message from history, conversation `lastMessage`, the text endpoint,
and SignalR now includes `attachments`; it is an empty array for text-only and
system messages.

### Download an attachment

`downloadUrl` is an API route, not a public storage URL. Fetch it with the JWT
and handle the response as a blob:

```ts
const response = await fetch(`${apiBase}${attachment.downloadUrl}`, {
  headers: { Authorization: `Bearer ${token}` },
});
if (!response.ok) throw new Error("Attachment is no longer available.");

const blob = await response.blob();
const objectUrl = URL.createObjectURL(blob);
// Use objectUrl for preview/download, then URL.revokeObjectURL(objectUrl).
```

The backend checks conversation access again on every download. Outsiders and
superseded lawyers receive `404`, including when they retained an old
`downloadUrl`. Remove cached attachment blobs and chat state when a proposal
becomes superseded.

### Real-time attachment delivery

Upload the binary files through the REST attachment endpoint. Do not send
base64 files through SignalR. After the upload commits, the backend broadcasts
the same complete `ChatMessageDto` through the existing `ReceiveMessage`
event, including all attachment metadata:

```ts
connection.on("ReceiveMessage", (message: ChatMessage) => {
  upsertMessageById(message);
});
```

The sender receives both the HTTP response and the SignalR event, so de-duplicate
messages by `message.id`. No extra attachment SignalR event is required.

SignalR hub:

```text
/hubs/chat
```

Before rendering a message composer, require both:

```text
conversationStatus == "Open" AND canChat == true
```

When the chat page loads `GET /api/chat/conversations` or
`GET /api/chat/conversations/{conversationId}`, use the chat DTO flags as the
final UI switch:

```text
canSendMessages == true AND canUploadAttachments == true
```

For the client's superseded proposal history, proposal data may expose
`ViewChatHistory`, but chat detail/list returns both booleans as `false`.
Show the old messages and files, hide the composer and upload button. For the
affected lawyer, proposal DTOs hide `conversationId`, chat list omits the
conversation, and direct chat access returns `404`.

## Suggested TypeScript contracts

```ts
export type ProposalStatus =
  | "Pending"
  | "Accepted"
  | "Rejected"
  | "Cancelled"
  | "Expired"
  | "Terminated"
  | "Superseded";

export type ProposalAction =
  | "Accept"
  | "Reject"
  | "Cancel"
  | "TerminateProposal"
  | "CreateContract"
  | "ViewContract"
  | "OpenChat"
  | "ViewChatHistory";

export interface ProposalListItem {
  id: string;
  legalCaseId: string;
  caseTitle: string;
  clientUserId: string;
  clientName: string;
  lawyerUserId: string;
  lawyerName: string;
  status: ProposalStatus;
  caseStatus: string;
  assignedLawyerUserId: string | null;
  isAssignedLawyer: boolean;
  contractId: string | null;
  contractStatus: string | null;
  conversationId: string | null;
  conversationStatus: "Open" | "Closed" | null;
  canChat: boolean;
  permittedActions: ProposalAction[];
  createdAt: string;
  respondedAt: string | null;
  expiresAt: string | null;
  closedAt: string | null;
  closedByUserId: string | null;
}

export interface ChatAttachment {
  id: string;
  fileName: string;
  contentType: string;
  sizeInBytes: number;
  downloadUrl: string;
}

export interface ChatMessage {
  id: string;
  conversationId: string;
  senderUserId: string | null;
  senderName: string | null;
  type: "User" | "System";
  content: string;
  systemCode: string | null;
  relatedEntityId: string | null;
  createdAt: string;
  isMine: boolean;
  attachments: ChatAttachment[];
}

export interface ChatConversation {
  id: string;
  proposalId: string;
  legalCaseId: string;
  caseTitle: string;
  client: { userId: string; name: string; role: "Client" };
  lawyer: { userId: string; name: string; role: "Lawyer" };
  status: "Open" | "Closed";
  createdAt: string;
  updatedAt: string;
  lastMessageAt: string | null;
  canSendMessages: boolean;
  canUploadAttachments: boolean;
}
```

When building query parameters, append each status separately:

```ts
for (const status of filters.statuses) {
  params.append("statuses", status);
}
```

## Error handling

| HTTP | Meaning |
| --- | --- |
| `400` | Invalid ID, filter, page, page size, message, reason, attachment count, size, name, type, or content. |
| `401` | Missing or invalid token. |
| `403` | Authenticated role cannot use the endpoint. |
| `404` | Resource is absent or does not belong to the caller. |
| `409` | Proposal expired, changed concurrently, has invalid status, reached the five-slot limit, conflicts with a contract, or the chat is closed. |

Always display `message` when present, otherwise join the `errors` array.

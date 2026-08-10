# SmartCourt — Frontend API Contract & Integration Guide
### Scope: Proposals + Chat (REST & Real-time)
> **Audience:** Frontend engineers  
> **Source of truth:** C# source code as of 2026-08-10  
> **Zero assumptions policy:** Every field, enum value, status code, and business rule below was read directly from the C# source.

---

## Table of Contents

1. [Global Conventions](#1-global-conventions)
2. [Shared Enums](#2-shared-enums)
3. [Proposals API](#3-proposals-api)
   - [3.1 Shared DTOs — Proposals](#31-shared-dtos--proposals)
   - [3.2 `POST /api/proposals` — Create Proposal](#32-post-apiproposals--create-proposal)
   - [3.3 `GET /api/proposals/cases/{legalCaseId}/availability` — Check Slot Availability](#33-get-apiproposalscaseslegalcaseidavailability--check-slot-availability)
   - [3.4 `GET /api/proposals` — List My Proposals](#34-get-apiproposals--list-my-proposals)
   - [3.5 `GET /api/proposals/{proposalId}` — Get Proposal Detail](#35-get-apiproposalsproposalid--get-proposal-detail)
   - [3.6 `POST /api/proposals/{proposalId}/accept` — Accept Proposal](#36-post-apiproposalsproposaliddaccept--accept-proposal)
   - [3.7 `POST /api/proposals/{proposalId}/reject` — Reject Proposal](#37-post-apiproposalsproposalidreject--reject-proposal)
   - [3.8 `POST /api/proposals/{proposalId}/cancel` — Cancel Proposal](#38-post-apiproposalsproposalidcancel--cancel-proposal)
   - [3.9 `POST /api/proposals/{proposalId}/terminate` — Terminate Proposal](#39-post-apiproposalsproposalidterminate--terminate-proposal)
4. [Chat REST API](#4-chat-rest-api)
   - [4.1 Shared DTOs — Chat](#41-shared-dtos--chat)
   - [4.2 `GET /api/chat/conversations` — List Conversations](#42-get-apichatconversations--list-conversations)
   - [4.3 `GET /api/chat/conversations/{conversationId}` — Get Conversation Detail](#43-get-apichatconversationsconversationid--get-conversation-detail)
   - [4.4 `GET /api/chat/conversations/{conversationId}/messages` — Get Message History](#44-get-apichatconversationsconversationidmessages--get-message-history)
   - [4.5 `POST /api/chat/conversations/{conversationId}/messages` — Send Message (HTTP)](#45-post-apichatconversationsconversationidmessages--send-message-http)
5. [Real-time Chat — SignalR Hub](#5-real-time-chat--signalr-hub)
   - [5.1 Connection](#51-connection)
   - [5.2 Hub Methods (Client → Server)](#52-hub-methods-client--server)
   - [5.3 Server Events (Server → Client)](#53-server-events-server--client)
6. [Cross-Slice Workflow: Proposal → Chat Provisioning](#6-cross-slice-workflow-proposal--chat-provisioning)
7. [System Messages Reference](#7-system-messages-reference)
8. [Business Rules Cheat-Sheet](#8-business-rules-cheat-sheet)

---

## 1. Global Conventions

### Authentication
All endpoints require a valid **JWT Bearer** token in the `Authorization` header:
```
Authorization: Bearer <access_token>
```
The token must carry the user's **role claim** (`Client` or `Lawyer`). Role requirements are documented per endpoint.

### Base Response Envelope
Every REST endpoint returns an `ApiResponse<T>` envelope:

```jsonc
{
  "success": true,          // boolean — true for 2xx outcomes, false for errors
  "statusCode": 200,        // int — mirrors the HTTP status code
  "message": "string|null", // human-readable message (present on errors, sometimes on success)
  "errors": ["string"],     // array of validation/business error strings (non-null only on failure)
  "data": { ... }           // the typed payload; null on error responses
}
```

### Date/Time Format
All `DateTime` fields are in **ISO 8601 UTC** format:  
`"2026-08-10T08:47:39Z"`

### UUIDs
All `Guid` fields are serialized as lowercase hyphenated strings:  
`"3fa85f64-5717-4562-b3fc-2c963f66afa6"`

### HTTP Status Codes Used

| Code | Meaning |
|------|---------|
| `200` | Success (GET, state-change actions) |
| `201` | Created (resource was newly created) |
| `400` | Validation failure |
| `401` | Unauthenticated (no or invalid token) |
| `403` | Forbidden (wrong role or the resource does not belong to the caller) |
| `404` | Resource not found (or caller is not a participant — intentionally opaque) |
| `409` | Business rule conflict (invalid state transition, concurrency collision, expired proposal, etc.) |

---

## 2. Shared Enums

All enum values are serialized as their **string name** (not their integer value) in JSON responses.  
When sent as query parameters, pass the **string name** exactly as listed.

### `ProposalStatus`

| String Value | Integer | Meaning |
|---|---|---|
| `"Pending"` | `0` | The proposal has been sent by the client and is awaiting a decision from the lawyer. This is the only actionable state. Proposals automatically expire after **3 days** from `CreatedAt`. |
| `"Accepted"` | `1` | The lawyer accepted the proposal. A chat conversation has been provisioned for this proposal. The case status is now `Matched`. |
| `"Rejected"` | `2` | The lawyer explicitly rejected the proposal. `DecisionReason` and `RespondedAt` are populated. The proposal is closed. |
| `"Cancelled"` | `3` | The client withdrew the proposal before it was decided. `DecisionReason`, `ClosedAt`, and `ClosedByUserId` are populated. |
| `"Expired"` | `4` | The 3-day response window elapsed without a lawyer decision. `ClosedAt` is populated. `DecisionReason` is null. |
| `"Terminated"` | `5` | An accepted proposal was ended by mutual agreement (either party). The associated contract must already be terminated before this is allowed. `DecisionReason`, `ClosedAt`, and `ClosedByUserId` are populated. |
| `"Superseded"` | `6` | The proposal (Pending or Accepted) was automatically voided because another proposal for the same case became active. `DecisionReason` is always `"Another contract was activated for this case."`. `ClosedAt` is populated. |

**Active statuses** (meaning the proposal is still open and may affect availability slots): `Pending`, `Accepted`.  
**Terminal statuses** (closed, no further transitions possible): `Rejected`, `Cancelled`, `Expired`, `Terminated`, `Superseded`.

---

### `ProposalInboxDirection`

Used as a query filter on `GET /api/proposals` to select the inbox view.

| String Value | Integer | Meaning |
|---|---|---|
| `"Sent"` | `1` | Return proposals that the current user **sent** as a client (i.e., `ClientUserId == currentUser`). Only users with the `Client` role may use this. |
| `"Received"` | `2` | Return proposals that the current user **received** as a lawyer (i.e., `LawyerUserId == currentUser`). Only users with the `Lawyer` role may use this. |

If this parameter is omitted, the server auto-selects: Lawyers default to `Received`; Clients default to `Sent`.

---

### `ChatMessageType`

Determines how a message should be rendered in the UI.

| String Value | Integer | Meaning |
|---|---|---|
| `"User"` | `1` | A message typed and sent by a human participant (client or lawyer). `SenderUserId` and `SenderName` are always populated. `SystemCode` and `RelatedEntityId` are always `null`. |
| `"System"` | `2` | An automated event notification injected by the server (e.g., "Contract is now active."). `SenderUserId` and `SenderName` are always `null`. `SystemCode` and `RelatedEntityId` are always populated. Render these as timeline events or banners, **not** as chat bubbles. |

---

## 3. Proposals API

### 3.1 Shared DTOs — Proposals

#### `ProposalDetailDto`
Returned by all state-mutation endpoints and `GET /api/proposals/{proposalId}`.

| Field | Type | Nullable? | Description |
|---|---|---|---|
| `id` | `Guid` | No | Unique identifier of the proposal. |
| `legalCaseId` | `Guid` | No | The ID of the legal case this proposal is attached to. |
| `caseTitle` | `string` | No | The title of the legal case, for display purposes. |
| `clientUserId` | `Guid` | No | User ID of the client who sent the proposal. |
| `clientName` | `string` | No | Full display name of the client. |
| `lawyerUserId` | `Guid` | No | User ID of the lawyer who received the proposal. |
| `lawyerName` | `string` | No | Full display name of the lawyer. |
| `message` | `string` | No | The cover message written by the client when sending the proposal. Max 2,000 chars. |
| `status` | `string` | No | Current status. See [`ProposalStatus`](#proposalstatus). |
| `decisionReason` | `string` | Yes | Populated only for `Rejected`, `Cancelled`, `Terminated`, and `Superseded`. Contains the reason string provided by the acting party. `null` for all other statuses. For `Rejected`, the lawyer may have chosen not to provide a reason, so this can also be `null` even when status is `Rejected`. |
| `createdAt` | `DateTime` | No | UTC timestamp when the proposal was created. |
| `respondedAt` | `DateTime` | Yes | UTC timestamp when the lawyer first decided (accepted or rejected). `null` for non-decided proposals. |
| `updatedAt` | `DateTime` | No | UTC timestamp of the last state change on the proposal. |
| `conversationId` | `Guid` | Yes | The ID of the associated chat conversation. Only non-`null` when `status == "Accepted"`. Use this to navigate to the chat UI. |
| `expiresAt` | `DateTime` | Yes | UTC deadline by which the lawyer must respond before the proposal auto-expires. Always `CreatedAt + 3 days`. `null` only for legacy proposals created before this field was introduced. |
| `closedAt` | `DateTime` | Yes | UTC timestamp when the proposal entered a terminal state (`Rejected`, `Cancelled`, `Expired`, `Terminated`, `Superseded`). `null` while the proposal is `Pending` or `Accepted`. |
| `closedByUserId` | `Guid` | Yes | User ID of whoever triggered the closure. Populated for `Cancelled` (client), `Terminated` (either party). `null` for `Rejected` (server attributes to lawyer but this field is not set), `Expired`, `Superseded`. |

---

#### `ProposalListItemDto`
Returned as items in the paginated list from `GET /api/proposals`.

| Field | Type | Nullable? | Description |
|---|---|---|---|
| `id` | `Guid` | No | Unique identifier of the proposal. |
| `legalCaseId` | `Guid` | No | The ID of the associated legal case. |
| `caseTitle` | `string` | No | Display title of the legal case. |
| `clientUserId` | `Guid` | No | User ID of the client. |
| `clientName` | `string` | No | Full display name of the client. |
| `lawyerUserId` | `Guid` | No | User ID of the lawyer. |
| `lawyerName` | `string` | No | Full display name of the lawyer. |
| `status` | `string` | No | Current status. See [`ProposalStatus`](#proposalstatus). |
| `createdAt` | `DateTime` | No | UTC timestamp of creation. |
| `respondedAt` | `DateTime` | Yes | UTC timestamp of lawyer's first decision. `null` if not yet decided. |
| `conversationId` | `Guid` | Yes | Chat conversation ID. Non-`null` only when `status == "Accepted"`. |
| `expiresAt` | `DateTime` | Yes | UTC expiry deadline (always `CreatedAt + 3 days`). |
| `closedAt` | `DateTime` | Yes | UTC timestamp of closure. `null` while open. |
| `closedByUserId` | `Guid` | Yes | User who triggered closure. See `ProposalDetailDto.closedByUserId` for full semantics. |

> **Note:** `message` and `decisionReason` are intentionally absent from the list item DTO to keep list responses lightweight. Fetch `GET /api/proposals/{proposalId}` for the full detail.

---

#### `ProposalPageDto`
The paginated wrapper returned by `GET /api/proposals`.

| Field | Type | Nullable? | Description |
|---|---|---|---|
| `items` | `ProposalListItemDto[]` | No | The current page of proposals. May be an empty array. |
| `page` | `int` | No | The current page number (1-indexed). |
| `pageSize` | `int` | No | The number of items requested per page. |
| `totalCount` | `int` | No | Total number of proposals matching the filter (across all pages). |
| `hasNextPage` | `bool` | No | `true` if there are more pages beyond the current one. |

---

#### `ProposalSlotAvailabilityDto`
Returned by `GET /api/proposals/cases/{legalCaseId}/availability`.

| Field | Type | Nullable? | Description |
|---|---|---|---|
| `legalCaseId` | `Guid` | No | The case ID echoed from the request. |
| `activeProposalCount` | `int` | No | The number of currently active proposals for this case (status is `Pending` or `Accepted`). |
| `proposalLimit` | `int` | No | The system-wide maximum number of active proposals per case. Currently `5`. |
| `availableProposalSlots` | `int` | No | How many more proposals can be sent: `max(0, proposalLimit - activeProposalCount)`. |
| `canSendProposal` | `bool` | No | `true` only when **both** conditions are met: (1) the case status is `Matched`, AND (2) `availableProposalSlots > 0`. Use this to enable/disable the "Send Proposal" button. |

---

### 3.2 `POST /api/proposals` — Create Proposal

**Purpose:** A client sends a proposal to a specific lawyer for one of their cases, initiating the engagement process.

**Authorization:** `Client` role only.

#### Request Body (`application/json`)

```jsonc
{
  "legalCaseId": "guid",    // Required. The case the client wants legal help with. Must be a case owned by the calling client.
  "lawyerUserId": "guid",   // Required. The target lawyer to invite. Must be an active user with the Lawyer role.
  "message": "string"       // Required. The client's cover message. 1–2,000 characters (whitespace-only is rejected).
}
```

**Field Validation:**

| Field | Rules |
|---|---|
| `legalCaseId` | Must not be empty GUID. Case must exist and belong to the calling client. Case status must be `Matched` (post AI-matching). |
| `lawyerUserId` | Must not be empty GUID. Target user must have an `Active` status and the `Lawyer` role. |
| `message` | Required (non-empty, non-whitespace). Max 2,000 characters. |

#### Response: `201 Created` — `ApiResponse<ProposalDetailDto>`

The newly created proposal in `Pending` status. `conversationId` is `null` (conversation is only provisioned on acceptance).

#### Error Status Codes

| Status | When |
|---|---|
| `400` | Validation failed (missing required fields, message too long). |
| `403` | Caller does not have the `Client` role. |
| `404` | `legalCaseId` does not exist or does not belong to the caller. |
| `409` | Case status is not `Matched`; OR the target lawyer is not eligible; OR an active proposal already exists for this case+lawyer pair; OR the case has already reached the 5-proposal limit. |

---

### 3.3 `GET /api/proposals/cases/{legalCaseId}/availability` — Check Slot Availability

**Purpose:** Lets the client UI check, before showing a "Send Proposal" button for a lawyer, whether: (a) the case is in the correct state, and (b) the active proposal cap has not been reached.

**Authorization:** `Client` role only.

#### Path Parameters

| Parameter | Type | Description |
|---|---|---|
| `legalCaseId` | `Guid` | The case to check. Must be owned by the calling client. |

#### Response: `200 OK` — `ApiResponse<ProposalSlotAvailabilityDto>`

> **Implementation tip:** Call this endpoint **before** showing the "Invite Lawyer" or "Send Proposal" CTA. If `canSendProposal` is `false`, disable the button and surface a reason (`availableProposalSlots == 0` → "Proposal limit reached", case not matched → "Case not yet matched").

#### Error Status Codes

| Status | When |
|---|---|
| `400` | `legalCaseId` is an empty GUID. |
| `404` | Case not found or does not belong to the calling client. |

---

### 3.4 `GET /api/proposals` — List My Proposals

**Purpose:** Returns a paginated, filterable list of proposals for the calling user's inbox (sent or received).

**Authorization:** `Client` or `Lawyer` role.

#### Query Parameters

| Parameter | Type | Required? | Default | Description |
|---|---|---|---|---|
| `direction` | `string` | No | Auto-selected | Inbox direction. See [`ProposalInboxDirection`](#proposalinboxdirection). If omitted: Lawyers see `Received`, Clients see `Sent`. |
| `status` | `string` | No | All statuses | Filter by a single status value. Must be a valid `ProposalStatus` string. |
| `search` | `string` | No | None | Free-text search. Matches against the **case title**, **client name**, or **lawyer name** (case-insensitive, `LIKE`-style). Max 100 characters. |
| `page` | `int` | No | `1` | 1-indexed page number. Must be ≥ 1. |
| `pageSize` | `int` | No | `10` | Items per page. Must be between 1 and 100 (inclusive). |

#### Response: `200 OK` — `ApiResponse<ProposalPageDto>`

Results are ordered by `createdAt` **descending** (newest first), with `id` as a secondary stable sort.

#### Error Status Codes

| Status | When |
|---|---|
| `400` | Invalid pagination values or invalid enum strings. |
| `403` | A `Client`-only user requested `direction=Received`, or a `Lawyer`-only user requested `direction=Sent`. |

---

### 3.5 `GET /api/proposals/{proposalId}` — Get Proposal Detail

**Purpose:** Retrieves full details of a single proposal, including the `message`, `decisionReason`, and `conversationId` link.

**Authorization:** `Client` or `Lawyer` role. The caller must be either the `clientUserId` or the `lawyerUserId` on the proposal.

#### Path Parameters

| Parameter | Type | Description |
|---|---|---|
| `proposalId` | `Guid` | The proposal to retrieve. |

#### Response: `200 OK` — `ApiResponse<ProposalDetailDto>`

#### Error Status Codes

| Status | When |
|---|---|
| `404` | Proposal not found, OR the caller is not a participant (deliberately returns 404, not 403). |

---

### 3.6 `POST /api/proposals/{proposalId}/accept` — Accept Proposal

**Purpose:** A lawyer accepts an incoming proposal, which: (1) transitions the proposal to `Accepted`, (2) changes the associated case status to `Matched`, (3) creates a `ChatConversation` for this proposal, (4) publishes a domain event.

**Authorization:** `Lawyer` role only. The caller must be the `lawyerUserId` on the proposal.

#### Path Parameters

| Parameter | Type | Description |
|---|---|---|
| `proposalId` | `Guid` | The proposal to accept. |

**Request Body:** None.

#### Response: `200 OK` — `ApiResponse<ProposalDetailDto>`

The updated proposal with `status: "Accepted"`. **The `conversationId` field will be populated** — the frontend should immediately use this to navigate to or open the chat for this proposal.

#### Error Status Codes

| Status | When |
|---|---|
| `400` | `proposalId` is an empty GUID. |
| `403` | Caller does not have the `Lawyer` role. |
| `404` | Proposal not found, or the caller is not the lawyer on this proposal. |
| `409` | The proposal has already expired by the time of the accept call (the server auto-transitions it to `Expired` and returns this error). Also returned on a concurrency collision. |

> **Important:** After a successful `202/200` Accept response, the `conversationId` in the returned `ProposalDetailDto.conversationId` is guaranteed non-null. Redirect the lawyer to the chat view immediately.

---

### 3.7 `POST /api/proposals/{proposalId}/reject` — Reject Proposal

**Purpose:** A lawyer declines a pending proposal. The proposal moves to `Rejected` status and is closed.

**Authorization:** `Lawyer` role only. The caller must be the `lawyerUserId` on the proposal.

#### Path Parameters

| Parameter | Type | Description |
|---|---|---|
| `proposalId` | `Guid` | The proposal to reject. |

#### Request Body (`application/json`)

```jsonc
{
  "reason": "string"   // Required. The lawyer's stated reason for rejection. 1–1,000 characters. Cannot be whitespace-only.
}
```

**Field Validation:**

| Field | Rules |
|---|---|
| `reason` | Required (non-empty, non-whitespace). Max 1,000 characters. |

#### Response: `200 OK` — `ApiResponse<ProposalDetailDto>`

Updated proposal with `status: "Rejected"`, `decisionReason`, `respondedAt`, and `closedAt` populated.

#### Error Status Codes

| Status | When |
|---|---|
| `400` | Validation failed (missing or too-long reason). |
| `403` | Caller does not have the `Lawyer` role. |
| `404` | Proposal not found or the caller is not the lawyer on this proposal. |
| `409` | Proposal has already expired (server transitions to `Expired` and returns this error); OR concurrency collision. |

---

### 3.8 `POST /api/proposals/{proposalId}/cancel` — Cancel Proposal

**Purpose:** A client withdraws a pending proposal they previously sent.

**Authorization:** `Client` role only. The caller must be the `clientUserId` on the proposal.

#### Path Parameters

| Parameter | Type | Description |
|---|---|---|
| `proposalId` | `Guid` | The proposal to cancel. |

#### Request Body (`application/json`)

```jsonc
{
  "reason": "string"   // Required. The client's stated reason for cancellation. 1–1,000 characters. Cannot be whitespace-only.
}
```

**Field Validation:**

| Field | Rules |
|---|---|
| `reason` | Required (non-empty, non-whitespace). Max 1,000 characters. |

#### Response: `200 OK` — `ApiResponse<ProposalDetailDto>`

Updated proposal with `status: "Cancelled"`, `decisionReason`, `closedAt`, and `closedByUserId` populated.

#### Error Status Codes

| Status | When |
|---|---|
| `400` | Validation failed. |
| `404` | Proposal not found or the caller is not the client on this proposal. |
| `409` | Proposal has already expired (server transitions to `Expired` and returns this error); OR proposal is not in `Pending` status; OR concurrency collision. |

---

### 3.9 `POST /api/proposals/{proposalId}/terminate` — Terminate Proposal

**Purpose:** Either party (client or lawyer) ends an **accepted** proposal (i.e., the working engagement). Used when both parties agree the engagement is over. The associated contract must already be terminated before this action is allowed.

**Authorization:** `Client` or `Lawyer` role. The caller must be either the `clientUserId` or `lawyerUserId` on the proposal.

#### Path Parameters

| Parameter | Type | Description |
|---|---|---|
| `proposalId` | `Guid` | The accepted proposal to terminate. |

#### Request Body (`application/json`)

```jsonc
{
  "reason": "string"   // Required. Explanation for the termination. 1–1,000 characters. Cannot be whitespace-only.
}
```

**Field Validation:**

| Field | Rules |
|---|---|
| `reason` | Required (non-empty, non-whitespace). Max 1,000 characters. |

#### Response: `200 OK` — `ApiResponse<ProposalDetailDto>`

Updated proposal with `status: "Terminated"`, `decisionReason`, `closedAt`, and `closedByUserId` populated. The associated chat conversation will also be closed by the server (no longer accepting messages).

#### Error Status Codes

| Status | When |
|---|---|
| `400` | Validation failed. |
| `404` | Proposal not found or caller is not a participant. |
| `409` | Proposal is not in `Accepted` status (can only terminate an accepted proposal); OR there is still an open/non-terminated contract attached to the proposal; OR concurrency collision. |

---

## 4. Chat REST API

> The Chat REST API is used for initial page load and message history. For live message delivery, see [Section 5 — SignalR Hub](#5-real-time-chat--signalr-hub).

### 4.1 Shared DTOs — Chat

#### `ChatParticipantDto`

Embedded in conversation DTOs to identify each side of the conversation.

| Field | Type | Nullable? | Description |
|---|---|---|---|
| `userId` | `Guid` | No | The user's account ID. |
| `name` | `string` | No | The user's full display name. |
| `role` | `string` | No | Always one of `"Client"` or `"Lawyer"` — the role this participant holds in the conversation. |

---

#### `ChatMessageDto`

Represents a single message in a conversation. Used in the messages list, as `lastMessage` in conversation lists, and as the real-time event payload.

| Field | Type | Nullable? | Description |
|---|---|---|---|
| `id` | `Guid` | No | Unique message identifier. Used as a stable key for rendering and deduplication. |
| `conversationId` | `Guid` | No | The ID of the conversation this message belongs to. |
| `senderUserId` | `Guid` | Yes | The user ID of the sender. `null` if `type == "System"` (system messages have no human sender). |
| `senderName` | `string` | Yes | Display name of the sender. `null` if `type == "System"`. |
| `type` | `string` | No | Message type. See [`ChatMessageType`](#chatmessagetype). Either `"User"` or `"System"`. |
| `content` | `string` | No | The human-readable text of the message. For `"User"` messages, this is what the user typed. For `"System"` messages, this is a pre-defined English sentence (see [Section 7](#7-system-messages-reference)). Max 2,000 characters. |
| `systemCode` | `string` | Yes | A machine-readable code identifying the type of system event. `null` for `"User"` messages. For `"System"` messages, this matches the `ContractConversationMessageType` name (e.g., `"ContractActivated"`, `"MilestoneFunded"`). See [Section 7](#7-system-messages-reference) for all possible values. |
| `relatedEntityId` | `Guid` | Yes | The ID of the entity that triggered this system event (e.g., the contract ID for `"ContractCreated"`, the milestone ID for `"MilestoneFunded"`). `null` for `"User"` messages. Use this to link system messages to the relevant contract/milestone UI. |
| `createdAt` | `DateTime` | No | UTC timestamp when the message was persisted. Use this for sorting and display. |
| `isMine` | `bool` | No | Server-resolved convenience flag: `true` if `senderUserId` equals the ID of the authenticated user making the request. Use this to render the message bubble on the correct side without client-side ID comparison. **Note:** For real-time events received via SignalR, this field is computed relative to the sender, so it will be `false` for the recipient. The frontend must compare `senderUserId` against the local user's ID to determine the correct side for real-time messages. |

---

#### `ChatConversationListItemDto`

One item in the conversations inbox list.

| Field | Type | Nullable? | Description |
|---|---|---|---|
| `id` | `Guid` | No | Unique conversation identifier. Use this as the route parameter for all conversation-scoped operations. |
| `proposalId` | `Guid` | No | The proposal that spawned this conversation. Useful for linking back to the proposal detail view. |
| `legalCaseId` | `Guid` | No | The legal case associated with this conversation. |
| `caseTitle` | `string` | No | Display title of the legal case. |
| `client` | `ChatParticipantDto` | No | The client participant (always has `role: "Client"`). |
| `lawyer` | `ChatParticipantDto` | No | The lawyer participant (always has `role: "Lawyer"`). |
| `status` | `string` | No | Conversation status. Either `"Open"` or `"Closed"`. A closed conversation no longer accepts new messages. |
| `createdAt` | `DateTime` | No | UTC timestamp when the conversation was first created (on proposal acceptance). |
| `updatedAt` | `DateTime` | No | UTC timestamp of the last state change (message sent, conversation closed). |
| `lastMessageAt` | `DateTime` | Yes | UTC timestamp of the most recent message in the conversation. `null` if no messages have been sent yet. |
| `lastMessage` | `ChatMessageDto` | Yes | The most recent message, pre-populated for list display. `null` if no messages exist. |

---

#### `ChatConversationDetailDto`

Returned by `GET /api/chat/conversations/{conversationId}`. Contains the full conversation header but **not** the messages (those are fetched separately via the messages endpoint).

| Field | Type | Nullable? | Description |
|---|---|---|---|
| `id` | `Guid` | No | Unique conversation identifier. |
| `proposalId` | `Guid` | No | The originating proposal ID. |
| `legalCaseId` | `Guid` | No | The associated legal case ID. |
| `caseTitle` | `string` | No | Display title of the legal case. |
| `client` | `ChatParticipantDto` | No | The client participant. |
| `lawyer` | `ChatParticipantDto` | No | The lawyer participant. |
| `status` | `string` | No | `"Open"` or `"Closed"`. Use this to disable the message input when the conversation is closed. |
| `createdAt` | `DateTime` | No | UTC creation timestamp. |
| `updatedAt` | `DateTime` | No | UTC last-update timestamp. |
| `lastMessageAt` | `DateTime` | Yes | UTC timestamp of the last message. `null` if no messages exist. |

---

#### `ChatConversationPageDto`

Paginated wrapper for the conversations list.

| Field | Type | Nullable? | Description |
|---|---|---|---|
| `items` | `ChatConversationListItemDto[]` | No | Current page of conversations. May be empty. |
| `page` | `int` | No | Current 1-indexed page number. |
| `pageSize` | `int` | No | Requested page size. |
| `totalCount` | `int` | No | Total number of matching conversations across all pages. |
| `hasNextPage` | `bool` | No | `true` if more pages exist. |

---

#### `ChatMessagePageDto`

Paginated wrapper for the messages list.

| Field | Type | Nullable? | Description |
|---|---|---|---|
| `items` | `ChatMessageDto[]` | No | Current page of messages. Always sorted **oldest-first** within the page (regardless of how the server fetched them). May be empty. |
| `page` | `int` | No | Current 1-indexed page number. |
| `pageSize` | `int` | No | Requested page size. |
| `totalCount` | `int` | No | Total number of messages in the conversation. |
| `hasNextPage` | `bool` | No | `true` if older pages exist (since the default view is newest-first pagination). |

---

### 4.2 `GET /api/chat/conversations` — List Conversations

**Purpose:** Returns a paginated inbox of all conversations the current user is a participant in. Used to render the conversation list/sidebar.

**Authorization:** `Client` or `Lawyer` role.

#### Query Parameters

| Parameter | Type | Required? | Default | Validation |
|---|---|---|---|---|
| `search` | `string` | No | None | Free-text search against case title, client name, or lawyer name. Max 200 characters. |
| `page` | `int` | No | `1` | Must be ≥ 1. |
| `pageSize` | `int` | No | `20` | Must be between 1 and 50 (inclusive). |

#### Response: `200 OK` — `ApiResponse<ChatConversationPageDto>`

Results are ordered by activity: `lastMessageAt ?? updatedAt` **descending** (most recently active first).

#### Error Status Codes

| Status | When |
|---|---|
| `400` | Invalid pagination or search exceeds 200 chars. |

---

### 4.3 `GET /api/chat/conversations/{conversationId}` — Get Conversation Detail

**Purpose:** Fetches the metadata for a single conversation. Call this when navigating to the chat view to get participant info, status, and the conversation's identity.

**Authorization:** `Client` or `Lawyer` role. The caller must be a participant (client or lawyer) of the conversation.

#### Path Parameters

| Parameter | Type | Description |
|---|---|---|
| `conversationId` | `Guid` | The conversation to retrieve. |

#### Response: `200 OK` — `ApiResponse<ChatConversationDetailDto>`

#### Error Status Codes

| Status | When |
|---|---|
| `404` | Conversation not found or caller is not a participant. |

---

### 4.4 `GET /api/chat/conversations/{conversationId}/messages` — Get Message History

**Purpose:** Fetches a paginated page of messages for a conversation. Use for initial load and "load more" (infinite scroll / pagination).

**Authorization:** `Client` or `Lawyer` role. The caller must be a participant in the conversation.

#### Path Parameters

| Parameter | Type | Description |
|---|---|---|
| `conversationId` | `Guid` | The conversation whose messages to fetch. |

#### Query Parameters

| Parameter | Type | Required? | Default | Validation |
|---|---|---|---|---|
| `page` | `int` | No | `1` | Must be ≥ 1. Values ≤ 0 are coerced to `1` by the controller. |
| `pageSize` | `int` | No | `50` | Must be between 1 and 100 (inclusive). Values ≤ 0 are coerced to `50` by the controller. |

#### Pagination Strategy

The server fetches messages ordered **newest-first** from the DB (to efficiently get the most recent page), then **re-sorts them oldest-first** before returning them in `items`. This means:

- `page=1` returns the **most recent** messages (e.g., the last 50), sorted oldest→newest within the page.
- `page=2` returns the **next older batch**, again sorted oldest→newest within the page.
- To implement infinite scroll (load older messages on scroll-up), increment the page number.

**Recommended loading approach:**
1. On conversation open: `GET …/messages?page=1&pageSize=50` → render items bottom-up.
2. On scroll to top: `GET …/messages?page=2&pageSize=50` → prepend to message list.
3. Stop loading when `hasNextPage == false`.

#### Response: `200 OK` — `ApiResponse<ChatMessagePageDto>`

#### Error Status Codes

| Status | When |
|---|---|
| `400` | Invalid pagination parameters. |
| `404` | Conversation not found or caller is not a participant. |

---

### 4.5 `POST /api/chat/conversations/{conversationId}/messages` — Send Message (HTTP)

**Purpose:** HTTP fallback for sending a message. **Prefer the SignalR `SendMessage` hub method** (see Section 5.2) in production, as it provides immediate real-time delivery confirmation. Use this HTTP endpoint when the SignalR connection is unavailable.

**Authorization:** `Client` or `Lawyer` role. The caller must be a participant.

#### Path Parameters

| Parameter | Type | Description |
|---|---|---|
| `conversationId` | `Guid` | The conversation to post to. |

#### Request Body (`application/json`)

```jsonc
{
  "content": "string"   // Required. The message text. 1–2,000 characters. Cannot be whitespace-only.
}
```

**Field Validation:**

| Field | Rules |
|---|---|
| `content` | Required (non-empty, non-whitespace). Max 2,000 characters. |

#### Response: `200 OK` — `ApiResponse<ChatMessageDto>`

The newly created message, including its server-assigned `id` and `createdAt`.

> **Side effect:** After persisting the message, the server broadcasts it to all SignalR clients in the conversation group via the `ReceiveMessage` event. If you are connected via SignalR, you will receive the message as a real-time event in addition to the HTTP response.

#### Error Status Codes

| Status | When |
|---|---|
| `400` | Validation failed (empty or too-long content). |
| `404` | Conversation not found or caller is not a participant. |
| `409` | Conversation is closed (`IsClosed == true`) OR the associated proposal status is no longer `Accepted` (the proposal was terminated or superseded). |

---

## 5. Real-time Chat — SignalR Hub

### 5.1 Connection

#### Hub URL
```
/hubs/chat
```
Connect using the `@microsoft/signalr` client library.

#### Authentication
The SignalR connection requires the same Bearer token as REST endpoints. The recommended way is to pass the token as a query string parameter (standard for SignalR WebSocket upgrades):

```javascript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/chat", {
    accessTokenFactory: () => getAccessToken() // return your JWT string
  })
  .withAutomaticReconnect()
  .build();

await connection.start();
```

#### Authorization
The hub requires the caller to have the `Client` or `Lawyer` role (same as the REST endpoints). Connections from unauthenticated or unauthorized users are rejected.

---

### 5.2 Hub Methods (Client → Server)

These are methods you **invoke** on the hub from the frontend.

---

#### `JoinConversation`

**Purpose:** Subscribes the current SignalR connection to a specific conversation's broadcast group. Must be called before you will receive `ReceiveMessage` events for that conversation. Call this immediately after opening the chat view.

**Arguments:**

| Name | Type | Description |
|---|---|---|
| `conversationId` | `Guid` (string) | The ID of the conversation to join. |

**Returns:** `void` (no return value).

**Error behavior:** If the conversation does not exist OR the caller is not a participant, the server throws a `HubException` with the message `"Conversation was not found."`. Handle this to redirect the user away from the chat view.

```javascript
try {
  await connection.invoke("JoinConversation", conversationId);
} catch (err) {
  console.error("Cannot join conversation:", err.message);
  // Navigate away or show error
}
```

---

#### `LeaveConversation`

**Purpose:** Removes the current connection from the conversation's broadcast group. Call this when navigating away from the chat view to stop receiving events for this conversation.

**Arguments:**

| Name | Type | Description |
|---|---|---|
| `conversationId` | `Guid` (string) | The ID of the conversation to leave. |

**Returns:** `void`.

**Error behavior:** This method does not throw. If the connection was not in the group, it is a no-op.

```javascript
await connection.invoke("LeaveConversation", conversationId);
```

---

#### `SendMessage`

**Purpose:** Sends a message via the real-time connection. Prefer this over the REST endpoint because it provides a direct return value (the persisted `ChatMessageDto`) without a second HTTP round-trip.

**Arguments:**

| Name | Type | Description |
|---|---|---|
| `conversationId` | `Guid` (string) | The conversation to post to. |
| `request` | `object` | A `SendChatMessageRequest` object: `{ "content": "string" }` |

**Returns:** `ChatMessageDto` — the fully persisted message object as returned from the server.

**Error behavior:** If the message cannot be sent (validation failure, conversation closed, etc.), a `HubException` is thrown. The exception message contains the human-readable error text from the server. Display it to the user.

```javascript
try {
  const message = await connection.invoke("SendMessage", conversationId, { content: "Hello!" });
  // message is a ChatMessageDto — add it to your local state
  appendMessage(message);
} catch (err) {
  showToast(err.message); // e.g., "Conversation is closed."
}
```

> **Deduplication note:** After calling `SendMessage`, the server also broadcasts the message to all group members (including the sender) via `ReceiveMessage`. Because you already have the message from the return value, you should deduplicate by `id` when rendering to avoid showing it twice.

---

### 5.3 Server Events (Server → Client)

These are events the server **pushes** to you. Register handlers **before** calling `connection.start()`.

---

#### `ReceiveMessage`

**Purpose:** Fired whenever a new message is persisted in a conversation you have joined. This includes messages sent by you (via both `SendMessage` hub method and the HTTP REST endpoint), and messages sent by the other participant, and system event messages injected by backend workflows (e.g., contract milestones).

**Trigger:** Fired by both:
1. A participant calling `SendMessage` on the hub.
2. A participant calling `POST /api/chat/conversations/{conversationId}/messages`.
3. The backend appending a system message (e.g., when a contract milestone is funded).

**Payload:** `ChatMessageDto` (full object, same schema as documented in [Section 4.1](#chatmessagedto)).

```javascript
connection.on("ReceiveMessage", (message) => {
  // message: ChatMessageDto
  // Check message.type:
  if (message.type === "User") {
    // Render as a chat bubble
    // Use message.isMine to determine left/right alignment
    // BUT: isMine is computed server-side for the SENDER's perspective
    // For messages received from the other party, isMine == false (correct)
    // For your own messages broadcast back to you, isMine == true
    // Safe approach: compare message.senderUserId === currentUserId
    const isMine = message.senderUserId === currentUser.id;
  } else if (message.type === "System") {
    // Render as a timeline event / notification banner
    // Use message.systemCode to determine the icon/color
    // Use message.relatedEntityId to deep-link to the relevant entity
    // message.content is a pre-formatted human-readable sentence
  }
});
```

**Example payload (User message):**
```jsonc
{
  "id": "a1b2c3d4-...",
  "conversationId": "e5f6a7b8-...",
  "senderUserId": "c9d0e1f2-...",
  "senderName": "Ahmed Hassan",
  "type": "User",
  "content": "Hello, I have reviewed your case and I can help.",
  "systemCode": null,
  "relatedEntityId": null,
  "createdAt": "2026-08-10T10:22:00Z",
  "isMine": false
}
```

**Example payload (System message):**
```jsonc
{
  "id": "b2c3d4e5-...",
  "conversationId": "e5f6a7b8-...",
  "senderUserId": null,
  "senderName": null,
  "type": "System",
  "content": "Contract is now active.",
  "systemCode": "ContractActivated",
  "relatedEntityId": "f6a7b8c9-...",   // The contract ID
  "createdAt": "2026-08-10T10:25:00Z",
  "isMine": false
}
```

---

## 6. Cross-Slice Workflow: Proposal → Chat Provisioning

Understanding this flow is critical for correct state management.

### Complete Flow

```
Client                     Backend                        Lawyer
  |                           |                              |
  |-- POST /api/proposals --> |                              |
  |                           | (proposal created, status=Pending)
  |                           |                              |
  |                           | <-- POST /proposals/{id}/accept --
  |                           | (1) proposal.Status = Accepted
  |                           | (2) case.Status = Matched
  |                           | (3) ChatConversation created
  |                           | (4) Domain event published (async)
  |                           |                              |
  |                           | -- 200 OK (ProposalDetailDto) ->
  |                           |    conversationId = "e5f6..."  |
  |                           |                              |
```

### What the Frontend Must Do After `Accept` Succeeds

1. **Read `conversationId` from the returned `ProposalDetailDto`.** It is guaranteed to be non-null.
2. **Navigate the Lawyer** to the chat view for `conversationId`.
3. **Notify the Client** (the client's proposal list will show `status: "Accepted"` on next poll or push notification). The client should use `proposalDetailDto.conversationId` to navigate to the chat.

### Chat Conversation Lifecycle

| Proposal Status | Conversation Status | Can Send Messages? |
|---|---|---|
| `Accepted` | `Open` | ✅ Yes |
| `Terminated` | `Closed` | ❌ No — server closes the conversation on termination |
| Any other terminal status | Conversation never exists | N/A |

### State Management Recommendation

When the conversation status is `"Closed"`:
- Disable the message input field.
- Show an informational banner: "This conversation has been closed."
- The message history remains fully readable.

---

## 7. System Messages Reference

System messages are injected into the conversation by backend workflows (not by users). The frontend receives them via the `ReceiveMessage` SignalR event or from the messages REST endpoint. Use `systemCode` to customize the rendering (icon, color, action button).

| `systemCode` | `content` (pre-set by server) | `relatedEntityId` points to |
|---|---|---|
| `ContractCreated` | "Contract draft was created." | Contract ID |
| `ContractAccepted` | "Contract draft was accepted." | Contract ID |
| `ContractActivated` | "Contract is now active." | Contract ID |
| `ContractCompleted` | "Contract was completed." | Contract ID |
| `MilestoneReadyForFunding` | "Milestone is ready for funding." | Milestone ID |
| `MilestoneFundingStarted` | "Milestone funding started." | Milestone ID |
| `MilestoneFunded` | "Milestone was funded." | Milestone ID |
| `MilestoneFundingFailed` | "Milestone funding failed." | Milestone ID |
| `MilestoneSubmitted` | "Milestone work was submitted." | Milestone ID |
| `MilestoneAutoAccepted` | "Milestone was accepted automatically." | Milestone ID |
| `MilestoneAccepted` | "Milestone was accepted." | Milestone ID |
| `MilestoneChangesRequested` | "Milestone changes were requested." | Milestone ID |
| `MilestoneChangeRequestApproved` | "Milestone change request was approved." | Milestone Change Request ID |
| `MilestoneChangeRequestRejected` | "Milestone change request was rejected." | Milestone Change Request ID |
| `MilestoneChangeRequestCancelled` | "Milestone change request was cancelled." | Milestone Change Request ID |
| `DisputeOpened` | "A dispute was opened." | Dispute ID |
| `DisputeAssigned` | "A moderator was assigned to the dispute." | Dispute ID |
| `DisputeResolved` | "Dispute was resolved." | Dispute ID |
| `DisputeClosed` | "Dispute was closed." | Dispute ID |
| `FundsReleased` | "Funds were released." | Payment/Transaction ID |
| `FundsRefunded` | "Funds were refunded." | Payment/Transaction ID |
| `ContractTerminated` | "Contract was terminated." | Contract ID |

> **Fallback:** If the server sends an unrecognized `systemCode` (future extension), the `content` will be `"Conversation was updated."`. Always render `content` as the display text, and use `systemCode` only for supplemental UI (icons, deep links).

---

## 8. Business Rules Cheat-Sheet

| Rule | Value | Where Enforced |
|---|---|---|
| Proposal expiry window | **3 days** from `createdAt` | `Proposal.ResponseWindow = TimeSpan.FromDays(3)` |
| Max active proposals per case | **5** | `ProposalPolicy.ActiveProposalLimitPerCase = 5` |
| Active statuses (count toward limit) | `Pending`, `Accepted` | `ProposalPolicy.IsActive()` |
| Max proposal message length | **2,000 characters** | Validator + Entity |
| Max cancel/reject/terminate reason length | **1,000 characters** | Validator + Entity |
| Max chat message length | **2,000 characters** | `ChatMessage.MaximumContentLength = 2_000` |
| Max conversations search term | **200 characters** | Validator |
| Max proposals search term | **100 characters** | Validator |
| Reject reason | Lawyer-provided, required by API but can be stored as `null` if explicitly empty string at domain level | Validator requires non-empty |
| Terminate requires no open contracts | A proposal can only be terminated if its contract is already `Terminated` | `TerminateProposalHandler` |
| Conversation is only created | On proposal acceptance | `AcceptProposalHandler` via `IChatConversationService.EnsureForAcceptedProposalAsync` |
| Messages only allowed in open conversations | `IsClosed == false` AND `Proposal.Status == Accepted` | `SendChatMessageHandler` |
| SignalR group name format | `"chat:conversation:{conversationId:N}"` (no-hyphens UUID) | `ChatGroups.Conversation()` — **internal only, not needed by frontend** |
| Expiry check on accept/reject/cancel | Server auto-expires and returns `409` if the proposal passed its deadline at action time | All decision handlers |

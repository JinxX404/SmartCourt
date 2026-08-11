# Realtime chat frontend integration

This is the frontend contract for the private client-lawyer chat hub and its
document attachment messages. The REST API remains the source of paginated
message history; SignalR delivers newly created messages in real time.

## Endpoint and authentication

Hub endpoint:

```text
/hubs/chat
```

The hub requires a valid JWT for a user with the `Client` or `Lawyer` role.
With the JavaScript SignalR client, supply the token through `accessTokenFactory`.
The library sends it correctly for the browser transport; do not add a custom
authorization header to the WebSocket connection.

```ts
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl(import.meta.env.DEV ? "/hubs/chat" : `${apiBase}/hubs/chat`, {
    accessTokenFactory: () => accessToken,
  })
  .withAutomaticReconnect([0, 2000, 5000, 10000])
  .build();
```

The repository's Vite configuration proxies `/hubs` to the local API and
enables WebSocket forwarding. Use the relative `/hubs/chat` URL during local
frontend development so the browser stays on one origin. For a separately
hosted frontend, configure its exact origin in backend configuration, for
example `Cors__Origins__0=https://app.example.com`. Do not use `*` with
credentialed chat connections.

Start the connection after the user is authenticated. Stop it when the user
signs out or the application is disposed.

For no-refresh delivery, register the `ReceiveMessage` handler before calling
`connection.start()`, then invoke `JoinConversation` with exactly one argument:
the conversation ID. Invoke `SendMessage` with exactly two arguments: the
conversation ID and `{ content }`. Cancellation is managed by the server and
must not be sent as an additional hub argument.

```ts
await connection.start();
await connection.invoke("JoinConversation", conversationId);
```

## Joining and leaving a conversation

| Hub method | Arguments | Result |
| --- | --- | --- |
| `JoinConversation` | `conversationId` | Subscribes this browser connection to that private conversation. |
| `LeaveConversation` | `conversationId` | Removes this browser connection from the conversation group. |
| `SendMessage` | `conversationId`, `{ content }` | Creates a text message and returns the created message. |

Join only after loading a conversation returned by the API. Call
`LeaveConversation` when the user changes chats, then join the next one:

```ts
await connection.invoke("LeaveConversation", previousConversationId);
await connection.invoke("JoinConversation", nextConversationId);
```

The server authorizes every `JoinConversation`. An outsider or a lawyer whose
proposal became `Superseded` receives a `HubException` with
`Conversation was not found.` Treat this as a privacy response: remove the
conversation and all cached message/attachment state from that lawyer's UI.

## Receiving messages

Subscribe before joining a conversation. The server broadcasts one event for
both text messages and attachment messages:

```ts
connection.on("ReceiveMessage", (message: ChatMessage) => {
  upsertMessageById(message);
});
```

Every event uses this DTO:

```ts
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
```

`attachments` is always an array. It is empty for ordinary text and system
messages. Never infer a document from `content`; render attachments from the
array.

When the sender's browser has joined the conversation, it receives both the
command response and `ReceiveMessage`. De-duplicate the local message
collection by `message.id`.

## Sending text in real time

Text can be sent through the hub:

```ts
const created = await connection.invoke<ChatMessage>(
  "SendMessage",
  conversationId,
  { content: draft.trim() },
);
upsertMessageById(created);
```

The backend also emits this exact message through `ReceiveMessage`, so the
other participant receives it immediately. A closed conversation causes a hub
error. Only render a composer when the proposal data says both
`conversationStatus === "Open"` and `canChat === true`.

## Sending documents

Documents are uploaded with REST, not through SignalR. This avoids putting
large binary files into the WebSocket and gives normal multipart upload limits
and validation. After the server stores the files, it broadcasts the resulting
`ChatMessage` through `ReceiveMessage` to both joined participants.

```http
POST /api/chat/conversations/{conversationId}/attachments
Content-Type: multipart/form-data
Authorization: Bearer <access-token>
```

Multipart fields:

| Field | Required | Rules |
| --- | --- | --- |
| `caption` | No | Maximum 2,000 characters. |
| `files` | Yes | Repeat for every selected file; 1 through 5 files. |

Allowed formats: PDF, DOCX, TXT, PNG, and JPEG. Each file is limited to 10 MB;
the combined request is limited to 25 MB. The backend validates file contents
as well as their names and extensions.

```ts
const body = new FormData();
if (caption.trim()) body.append("caption", caption.trim());
for (const file of selectedFiles) body.append("files", file);

const response = await fetch(
  `${apiBase}/api/chat/conversations/${conversationId}/attachments`,
  {
    method: "POST",
    headers: { Authorization: `Bearer ${accessToken}` },
    body,
  },
);
```

Do not set the `Content-Type` header yourself because the browser must provide
the multipart boundary. On success, the endpoint returns `201 Created`; its
`data` field is the same `ChatMessage` that SignalR broadcasts. Upsert that
response by ID; when the event arrives, it will be ignored as a duplicate.

## Downloading a document

`downloadUrl` is a protected API path, never a direct storage URL. Download it
with the JWT, then create a temporary browser URL from the returned blob:

```ts
const response = await fetch(`${apiBase}${attachment.downloadUrl}`, {
  headers: { Authorization: `Bearer ${accessToken}` },
});
if (!response.ok) throw new Error("Attachment is unavailable.");

const blob = await response.blob();
const objectUrl = URL.createObjectURL(blob);
// Open or download objectUrl, then call URL.revokeObjectURL(objectUrl).
```

The server checks chat access again on every download. A superseded lawyer or
an outsider receives `404`, even if they have a previously saved URL.

## Reconnect behavior

Automatic reconnect restores the hub connection but does not restore SignalR
groups. After `onreconnected`, rejoin the currently open conversation only
after refreshing its REST detail or proposal state:

```ts
connection.onreconnected(async () => {
  const conversation = await refreshCurrentConversation();
  const proposal = await refreshCurrentProposal();
  if (conversation?.status === "Open" && proposal?.canChat) {
    await connection.invoke("JoinConversation", conversation.id);
  }
});
```

On a `proposal.superseded` notification or a `404` from chat REST/SignalR,
leave the conversation, delete it from the lawyer's local state, and revoke
any cached attachment object URLs.

## REST history

SignalR is for new messages only. On opening a chat and after reconnecting,
load history through:

```http
GET /api/chat/conversations/{conversationId}/messages?page=1&pageSize=50
```

The response uses the same `ChatMessage` DTO and includes attachment metadata.
Merge history and events by message ID, ordered by `createdAt` then `id`.

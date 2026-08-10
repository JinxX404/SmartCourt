# Frontend Notification Integration Guide and API Contract

Status: **backend contract implemented and verified**  
Audience: SmartCourt frontend, mobile, and API-consumer teams  
Scope: consuming in-app notifications. No frontend source code is included in the backend increment.

The backend exposes a durable authenticated inbox through REST and a best-effort real-time stream through SignalR. Email and SMS are not part of this contract yet.

## Contract summary

| Concern | Contract |
|---|---|
| REST base | Same API origin used by the rest of SmartCourt. |
| Feed | `GET /api/notifications` |
| Unread badge | `GET /api/notifications/unread-count` |
| Mark one read | `PATCH /api/notifications/{notificationId}/read` |
| Mark all read | `PATCH /api/notifications/read-all` |
| SignalR hub | `/hubs/notifications` |
| Authentication | Required for every endpoint and hub connection. |
| Source of truth | REST/database inbox. |
| Real-time guarantee | Best effort; an event may be delayed, missed, or repeated. |
| Ordering | Newest first from REST; do not assume socket arrival order. |
| Client deduplication key | Notification `id`. |

There is no frontend-accessible create or delete endpoint. Notifications are created only by trusted backend business events.

## Recommended client lifecycle

1. Create one application-level SignalR connection.
2. Register all event handlers before starting it.
3. Start the authenticated connection.
4. Fetch the first REST page and merge it by notification `id`.
5. Merge SignalR events into the same store.
6. Re-fetch the first page and unread count after reconnect, app resume, or authentication refresh.
7. Stop and discard the connection on logout.

Starting SignalR before the first REST snapshot closes the usual fetch-to-connect gap. REST reconciliation and ID-based upserts make duplicate or missed socket events harmless.

## Authentication and transport

### SmartCourt browser client

The existing browser authentication uses an HttpOnly `accessToken` cookie. Use relative URLs and allow credentials; JavaScript must not read or copy the cookie.

Install the SignalR client in the frontend project when frontend implementation begins:

```powershell
npm install @microsoft/signalr
```

Create one shared connection:

```ts
import * as signalR from '@microsoft/signalr';

export const notificationConnection = new signalR.HubConnectionBuilder()
  .withUrl('/hubs/notifications', {
    withCredentials: true,
  })
  .withAutomaticReconnect([0, 2_000, 10_000, 30_000])
  .configureLogging(signalR.LogLevel.Warning)
  .build();
```

The current Vite development server already proxies `/hubs` with WebSocket support. Use `/hubs/notifications`, not a hard-coded localhost API URL.

Do not store the access token in `localStorage` and do not add an `accessTokenFactory` to the browser flow while HttpOnly cookie authentication is available.

### Native, automated, or other non-cookie clients

A trusted client holding a bearer token can use:

```ts
.withUrl(`${apiBaseUrl}/hubs/notifications`, {
  accessTokenFactory: () => getCurrentAccessToken(),
})
```

The backend accepts SignalR's `access_token` query parameter only on approved hub paths, including `/hubs/notifications`. Use HTTPS and redact query strings from infrastructure logs because WebSocket/SSE transports can place the token in the URL.

### Deployment constraints

Development is same-origin through Vite. The backend does not currently define a general cross-origin frontend contract. Hosting the frontend on a different origin requires an explicit credentialed CORS allowlist, compatible production cookie settings, HTTPS, and proxy support for WebSocket upgrades. Never combine credentials with `AllowAnyOrigin`.

The hub closes when authentication expires. Refresh authentication using the existing application flow, then establish a new connection and reconcile through REST.

## Common response envelope

Successful REST responses use the existing camel-case `ApiResponse<T>` envelope:

```ts
export interface ApiResponse<T> {
  success: boolean;
  data: T | null;
  message: string | null;
  errors: string[] | null;
  statusCode: number;
}
```

On a successful notification request:

- HTTP status is `200`;
- `success` is `true`;
- `data` contains the endpoint result;
- `statusCode` is `200`;
- `message` and `errors` are normally `null`.

Validation and not-found exceptions use the repository error envelope, but authentication middleware may return `401` without a useful JSON body. Client code must branch on the HTTP status before assuming an envelope is present.

## Shared TypeScript contract

```ts
export type NotificationSeverity =
  | 'Information'
  | 'Success'
  | 'Warning'
  | 'Critical';

export interface NotificationDto {
  id: string; // GUID
  type: string;
  severity: NotificationSeverity;
  title: string;
  body: string;
  actionUrl: string | null;
  data: Record<string, string> | null;
  createdAtUtc: string; // ISO-8601 UTC
  readAtUtc: string | null; // ISO-8601 UTC
  expiresAtUtc: string | null; // ISO-8601 UTC
}

export interface NotificationPageDto {
  items: NotificationDto[];
  nextCursor: string | null;
  unreadCount: number;
}

export interface UnreadNotificationCountDto {
  unreadCount: number;
}

export interface NotificationReadDto {
  notificationId: string;
  readAtUtc: string;
  unreadCount: number;
}

export interface NotificationsReadAllDto {
  readAtUtc: string;
  unreadCount: number;
}
```

Derive `isRead` as `notification.readAtUtc !== null`. Do not create a second independent read flag in the client model.

## REST API

### Fetch the notification feed

```http
GET /api/notifications?pageSize=20&cursor={opaqueCursor}&isRead={true|false}
```

Query parameters:

| Name | Type | Required | Default | Rules |
|---|---|---:|---:|---|
| `pageSize` | integer | No | `20` | From `1` through `50`, inclusive. |
| `cursor` | string | No | — | Opaque cursor returned by the preceding page. Do not decode or construct it. |
| `isRead` | boolean | No | — | `true` returns read rows; `false` returns unread rows; omitted returns both. |

Behavior:

- results are ordered newest first using a server sequence;
- `nextCursor` is `null` when there is no older page;
- `unreadCount` is the current count across all active unread notifications, even when `isRead=true` or the page is not the first page;
- expired notifications are excluded from the feed and unread count;
- a cursor is only meaningful with the same logical query/filter sequence;
- receiving a new notification does not invalidate an existing older-page cursor.

Example `200` response:

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "07c4a24e-d9be-4cd5-8e66-32f681688166",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/1adcab99-2725-412c-a918-a140ac6af83d",
        "data": {
          "proposalId": "1adcab99-2725-412c-a918-a140ac6af83d",
          "legalCaseId": "91ecfe53-d079-470b-af1a-c35328862687"
        },
        "createdAtUtc": "2026-08-09T12:30:00Z",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": "djE6MTIzNA",
    "unreadCount": 1
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```

Invalid `pageSize` or `cursor` returns HTTP `400`. An invalid cursor must not be replaced locally with a guessed value; restart pagination without a cursor.

### Fetch only the unread count

```http
GET /api/notifications/unread-count
```

Example `200` response:

```json
{
  "success": true,
  "data": {
    "unreadCount": 3
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```

Use this for inexpensive badge reconciliation after reconnect/resume. It does not replace fetching the inbox.

### Mark one notification read

```http
PATCH /api/notifications/{notificationId}/read
```

The request has no body. On success it returns the full updated `NotificationDto` in `ApiResponse<NotificationDto>`.

```json
{
  "success": true,
  "data": {
    "id": "07c4a24e-d9be-4cd5-8e66-32f681688166",
    "type": "proposal.created",
    "severity": "Information",
    "title": "عرض جديد",
    "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
    "actionUrl": "/proposals/1adcab99-2725-412c-a918-a140ac6af83d",
    "data": {
      "proposalId": "1adcab99-2725-412c-a918-a140ac6af83d",
      "legalCaseId": "91ecfe53-d079-470b-af1a-c35328862687"
    },
    "createdAtUtc": "2026-08-09T12:30:00Z",
    "readAtUtc": "2026-08-09T12:31:10Z",
    "expiresAtUtc": null
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```

This operation is idempotent. Repeating it returns `200` and preserves the first `readAtUtc`. A missing notification, invalid/empty route ID, or another user's notification returns `404`; ownership is intentionally not disclosed.

Client example:

```ts
async function markNotificationRead(id: string): Promise<NotificationDto> {
  const response = await apiClient.patch<ApiResponse<NotificationDto>>(
    `/api/notifications/${encodeURIComponent(id)}/read`,
  );

  if (!response.data.success || !response.data.data) {
    throw new Error(response.data.message ?? 'Could not mark notification read.');
  }

  notificationStore.getState().upsert(response.data.data);
  return response.data.data;
}
```

### Mark all notifications read

```http
PATCH /api/notifications/read-all
```

The request has no body. It is idempotent and returns:

```json
{
  "success": true,
  "data": {
    "readAtUtc": "2026-08-09T12:32:00Z",
    "unreadCount": 0
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```

Apply `readAtUtc` to unread items already in local state and set the badge to the server's `unreadCount`. A later REST fetch remains authoritative.

### Status-code contract

| Status | Meaning for this feature | Client action |
|---:|---|---|
| `200` | Request succeeded. | Merge returned data. |
| `400` | Invalid feed query, such as page size/cursor. | Stop the request; show a recoverable error and restart pagination when appropriate. |
| `401` | Missing, invalid, or expired authentication. | Run the application's authentication recovery/login flow. |
| `404` | Notification does not exist or is not owned by the caller; invalid GUID route values can also fail route matching. | Remove stale navigation state if appropriate; never infer ownership. |
| `405` | Unsupported HTTP verb. | Treat as a client integration bug. |
| `5xx` | Unexpected server/infrastructure failure. | Keep cached state and retry with bounded backoff. |

## SignalR API

The hub has no client-callable business methods. It pushes these exact, case-sensitive server-to-client method names after authentication:

### `NotificationCreated`

Payload: a complete `NotificationDto`, identical to a feed item.

```ts
notificationConnection.on(
  'NotificationCreated',
  (notification: NotificationDto) => {
    notificationStore.getState().upsert(notification);
  },
);
```

The same ID may be delivered more than once after an outbox retry. Upsert; never append blindly.

### `NotificationRead`

Payload: `NotificationReadDto`.

```ts
notificationConnection.on(
  'NotificationRead',
  (update: NotificationReadDto) => {
    notificationStore.getState().markReadFromServer(update);
  },
);
```

The event is broadcast only when the row changes from unread to read. An idempotent repeat PATCH still returns the REST DTO but does not need another socket event.

### `NotificationsReadAll`

Payload: `NotificationsReadAllDto`.

```ts
notificationConnection.on(
  'NotificationsReadAll',
  (update: NotificationsReadAllDto) => {
    notificationStore.getState().markAllReadFromServer(update);
  },
);
```

The current backend returns `unreadCount: 0` for this event. Keep the TypeScript field typed as `number` so the contract remains structurally consistent with REST.

### Delivery behavior

- Events are routed to all active connections for the authenticated user, supporting several tabs/devices.
- SignalR is best effort and potentially duplicate.
- It is not a queue and does not replay messages after a disconnect.
- A successful socket event does not replace REST reconciliation.
- Never use event arrival as proof that the user saw the notification.

## Connection manager reference

`withAutomaticReconnect` handles established-connection loss but does not retry an initial `start()` failure. Use one bounded/manual initial loop and cancel it on logout.

```ts
let stopRequested = false;
let startTask: Promise<void> | null = null;

const delay = (milliseconds: number) =>
  new Promise<void>((resolve) => window.setTimeout(resolve, milliseconds));

async function startUntilConnected(): Promise<void> {
  while (!stopRequested) {
    try {
      await notificationConnection.start();
      await Promise.all([
        notificationStore.getState().refreshFirstPage(),
        notificationStore.getState().refreshUnreadCount(),
      ]);
      return;
    } catch {
      await delay(5_000);
    }
  }
}

export function startNotifications(): Promise<void> {
  stopRequested = false;
  startTask ??= startUntilConnected().finally(() => {
    startTask = null;
  });
  return startTask;
}

notificationConnection.onreconnected(async () => {
  await Promise.all([
    notificationStore.getState().refreshFirstPage(),
    notificationStore.getState().refreshUnreadCount(),
  ]);
});

notificationConnection.onclose(() => {
  if (!stopRequested) void startNotifications();
});

export async function stopNotifications(): Promise<void> {
  stopRequested = true;
  await notificationConnection.stop();
}
```

Register `NotificationCreated`, `NotificationRead`, and `NotificationsReadAll` handlers before calling `startNotifications()`. Authentication refresh belongs to the existing auth layer; once refresh succeeds, ask this manager to reconnect.

## Store and pagination rules

- Key entities by `id` and upsert REST/SignalR data into the same map.
- Keep page cursor state separate from the entity map.
- Preserve the server's newest-first order; use `createdAtUtc` for display, not as a pagination cursor.
- Do not remove a socket-delivered item merely because an older/stale REST page omitted it.
- Use server-returned unread counts. Optimistic badge changes are allowed, but reconcile after any failure.
- When `NotificationRead` arrives for an item not currently loaded, still apply `unreadCount`; a future REST fetch supplies the row.
- When `NotificationsReadAll` arrives, update loaded unread rows and set the badge from the event.
- Fetch older pages only when requested; do not reuse a cursor across a changed `isRead` filter.
- On logout, clear all notification entities, cursors, badge state, and the connection to prevent cross-account leakage.

## Current notification types

| Type | Recipient/action | Severity | Required data keys |
|---|---|---|---|
| `proposal.created` | Lawyer; `/proposals/{proposalId}` | `Information` | `proposalId`, `legalCaseId` |
| `proposal.accepted` | Client; `/proposals/{proposalId}` | `Success` | `proposalId`, `legalCaseId` |
| `proposal.rejected` | Client; `/proposals/{proposalId}` | `Warning` | `proposalId`, `legalCaseId` |
| `contract.created` | Client; no action URL | `Information` | `contractId`, `proposalId`, `legalCaseId` |
| `contract.draft-updated` | Client; no action URL | `Warning` | `contractId`, `proposalId`, `legalCaseId` |
| `contract.acceptance-recorded` | Participant whose acceptance is still required; no action URL | `Information` | `contractId`, `proposalId`, `legalCaseId` |
| `contract.activated` | Client and Lawyer; no action URL | `Success` | `contractId`, `proposalId`, `legalCaseId` |
| `contract.completed` | Client and Lawyer; no action URL | `Success` | `contractId`, `proposalId`, `legalCaseId` |
| `contract.termination-requested` | Counterparty and, while settlement is pending, requester; no action URL | `Warning` | `contractId`, `proposalId`, `legalCaseId` |
| `contract.terminated` | Client and Lawyer; no action URL | `Warning` | `contractId`, `proposalId`, `legalCaseId` |
| `milestone.created` | Other participant; no action URL | `Information` | `milestoneId`, `contractId`, `proposalId`, `legalCaseId` |
| `milestone.draft-updated` | Other participant; no action URL | `Warning` | `milestoneId`, `contractId`, `proposalId`, `legalCaseId` |
| `milestone.acceptance-recorded` | Participant whose approval is required; no action URL | `Information` | `milestoneId`, `contractId`, `proposalId`, `legalCaseId` |
| `milestone.approved` | Client and Lawyer; no action URL | `Success` | `milestoneId`, `contractId`, `proposalId`, `legalCaseId` |
| `milestone.ready-for-funding` | Client; no action URL | `Information` | `milestoneId`, `contractId`, `proposalId`, `legalCaseId` |
| `milestone.submitted` | Client; no action URL | `Information` | `milestoneId`, `contractId`, `proposalId`, `legalCaseId` |
| `milestone.changes-requested` | Lawyer; no action URL | `Warning` | `milestoneId`, `contractId`, `proposalId`, `legalCaseId` |
| `milestone.accepted` | Lawyer; no action URL | `Success` | `milestoneId`, `contractId`, `proposalId`, `legalCaseId` |
| `milestone.auto-accepted` | Client / Lawyer; no action URL | `Warning` / `Success` | `milestoneId`, `contractId`, `proposalId`, `legalCaseId` |
| `milestone.change-request-created` | Other participant; no action URL | `Information` | Base Milestone keys plus `changeRequestId` |
| `milestone.change-request-approved` | Requester; no action URL | `Success` | Base Milestone keys plus `changeRequestId` |
| `milestone.change-request-rejected` | Requester; no action URL | `Warning` | Base Milestone keys plus `changeRequestId` |
| `milestone.change-request-cancelled` | Other participant; no action URL | `Information` | Base Milestone keys plus `changeRequestId` |
| `milestone.funding-started` | Lawyer; no action URL | `Information` | `milestoneId`, `contractId`, `proposalId`, `legalCaseId` |
| `milestone.funded` | Client and Lawyer; no action URL | `Success` | `milestoneId`, `contractId`, `proposalId`, `legalCaseId` |
| `milestone.funding-failed` | Client; no action URL | `Critical` | `milestoneId`, `contractId`, `proposalId`, `legalCaseId` |
| `funds.released` | Client and Lawyer; no action URL | `Success` | Base Milestone keys plus `escrowHoldId`, `paymentTransactionId` |
| `funds.refunded` | Client / Lawyer; no action URL | `Success` / `Information` | Base Milestone keys plus `escrowHoldId`, `paymentTransactionId` |
| `wallet.withdrawal-completed` | Lawyer; no action URL | `Success` | `withdrawalId` |
| `wallet.withdrawal-failed` | Lawyer; no action URL | `Warning` | `withdrawalId` |
| `wallet.withdrawal-delayed` | Lawyer; no action URL | `Warning` | `withdrawalId` |
| `wallet.adjusted` | Lawyer; no action URL | `Warning` | `walletAdjustmentId`, `contractId` |

Treat unknown future `type` values as generic notifications: show the server title/body and severity, but do not assume type-specific data keys. New types can be added without changing `NotificationDto`.

`type` and the keys inside `data` are intentionally English machine identifiers. The user-visible `title` and `body` are persisted Arabic snapshots created when the source event is dispatched. Fetching a notification does not regenerate or translate its copy, so clients must always render the returned `title` and `body` rather than deriving display text from `type`.

Contract, Milestone, Payment, and Wallet notifications deliberately return `actionUrl: null` in this increment. Render them normally without a navigation affordance. Do not manufacture a client-side URL from IDs in `data`; navigation will be enabled only through a later additive backend contract after the route is approved.

Exact Contract display copy is part of the API contract:

| Type | Arabic title | Arabic body |
|---|---|---|
| `contract.created` | `مسودة عقد جديدة` | `أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.` |
| `contract.draft-updated` | `تم تحديث مسودة العقد` | `تم تحديث شروط العقد، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.` |
| `contract.acceptance-recorded` | `موافقة جديدة على العقد` | `وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.` |
| `contract.activated` | `تم تفعيل العقد` | `أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.` |
| `contract.completed` | `اكتمل العقد` | `اكتملت جميع مراحل العقد وتسوياته بنجاح.` |
| `contract.termination-requested` | `تم طلب إنهاء العقد` | `تم تسجيل طلب إنهاء العقد، وتجري معالجة التسوية اللازمة.` |
| `contract.terminated` | `تم إنهاء العقد` | `اكتملت إجراءات إنهاء العقد وتسويته.` |

Exact Milestone display copy is also part of the API contract:

| Type / recipient variant | Arabic title | Arabic body |
|---|---|---|
| `milestone.created` | `مرحلة تعاقدية جديدة` | `أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.` |
| `milestone.draft-updated` | `تم تحديث المرحلة` | `تم تحديث شروط المرحلة، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.` |
| `milestone.acceptance-recorded` | `موافقة جديدة على المرحلة` | `وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك.` |
| `milestone.approved` | `تم اعتماد المرحلة` | `وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.` |
| `milestone.ready-for-funding` | `المرحلة جاهزة للتمويل` | `أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.` |
| `milestone.submitted` | `تم تسليم أعمال المرحلة` | `سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.` |
| `milestone.changes-requested` | `طُلبت تعديلات على المرحلة` | `طلب العميل تعديلات على أعمال المرحلة، ويمكنك مراجعة الطلب وإعادة التسليم.` |
| `milestone.accepted` | `تم قبول أعمال المرحلة` | `قبل العميل أعمال المرحلة، وبدأت مدة حجز المبلغ قبل إتاحته للصرف.` |
| `milestone.auto-accepted` / Client | `تم قبول المرحلة تلقائيًا` | `انتهت مدة المراجعة وقُبلت أعمال المرحلة تلقائيًا، وبدأت مدة الاعتراض.` |
| `milestone.auto-accepted` / Lawyer | `تم قبول المرحلة تلقائيًا` | `قُبلت أعمال المرحلة تلقائيًا بعد انتهاء مدة المراجعة، وبدأت مدة حجز المبلغ.` |
| `milestone.change-request-created` | `طلب تعديل جديد للمرحلة` | `أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.` |
| `milestone.change-request-approved` | `تمت الموافقة على طلب التعديل` | `وافق الطرف الآخر على طلب تعديل المرحلة، وطُبّقت الشروط المعتمدة.` |
| `milestone.change-request-rejected` | `تم رفض طلب تعديل المرحلة` | `رفض الطرف الآخر طلب تعديل المرحلة. يمكنك مراجعة الطلب لمعرفة التفاصيل.` |
| `milestone.change-request-cancelled` | `تم إلغاء طلب تعديل المرحلة` | `ألغى الطرف الآخر طلب تعديل المرحلة، ولم يعد القرار مطلوبًا منك.` |

Milestone bodies intentionally exclude descriptions, submission notes, change-request text, rejection reasons, payment amounts, and stored-file IDs. Fetch those details through the authorized Milestones APIs when the product later adds an approved route.

Exact Payment and Wallet display copy is part of the API contract:

| Type / recipient variant | Arabic title | Arabic body |
|---|---|---|
| `milestone.funding-started` / Lawyer | `بدأ تمويل المرحلة` | `بدأت معالجة تمويل المرحلة. انتظر تأكيد اكتمال التمويل قبل بدء العمل.` |
| `milestone.funded` / Client | `تم تمويل المرحلة` | `اكتمل تمويل المرحلة وحُفظ المبلغ في حساب الضمان.` |
| `milestone.funded` / Lawyer | `تم تمويل المرحلة` | `اكتمل تمويل المرحلة، ويمكنك الآن بدء العمل عليها.` |
| `milestone.funding-failed` / Client | `فشل تمويل المرحلة` | `لم تكتمل عملية تمويل المرحلة. يمكنك مراجعة وسيلة الدفع والمحاولة مرة أخرى.` |
| `funds.released` / Client | `تم تحرير أموال المرحلة` | `انتهت مدة الحجز وتم تحرير مستحقات المحامي عن المرحلة.` |
| `funds.released` / Lawyer | `أصبحت مستحقات المرحلة متاحة` | `تم تحويل مستحقات المرحلة إلى رصيد محفظتك المتاح.` |
| `funds.refunded` / Client | `تم رد أموال المرحلة` | `اكتملت تسوية المرحلة وتم رد الأموال إلى العميل.` |
| `funds.refunded` / Lawyer | `تم رد أموال المرحلة` | `اكتملت تسوية المرحلة برد الأموال إلى العميل.` |
| `wallet.withdrawal-completed` / Lawyer | `اكتمل طلب السحب` | `اكتمل طلب سحب الرصيد من محفظتك بنجاح.` |
| `wallet.withdrawal-failed` / Lawyer | `فشل طلب السحب` | `لم يكتمل طلب السحب، وأُعيد المبلغ إلى رصيد محفظتك المتاح.` |
| `wallet.withdrawal-delayed` / Lawyer | `طلب السحب يحتاج إلى مراجعة` | `تأخر حسم طلب السحب ويجري التعامل معه يدويًا. لا تنشئ طلبًا بديلًا.` |
| `wallet.adjusted` / Lawyer | `تم تصحيح رصيد المحفظة` | `أجرى مسؤول النظام تصحيحًا ماليًا على محفظتك. راجع الرصيد الحالي والتفاصيل مع الدعم عند الحاجة.` |

Financial notifications deliberately exclude amounts, currency, payment method and withdrawal destination references, provider transaction identifiers, failure details, administrative reasons, and idempotency keys. Treat every ID in `data` as an opaque lookup key; it does not grant access to the referenced resource.

## Navigation, rendering, and security

- Render `title` and `body` as text. Never use `dangerouslySetInnerHTML`.
- Navigate only to relative `actionUrl` values beginning with one `/`.
- Reject absolute URLs, schemes, protocol-relative values (`//...`), backslashes, and routes not recognized by the application.
- Prefer a client route allowlist keyed by `type` for privileged flows.
- Treat `data` as display/navigation metadata, never authorization. The destination API must enforce ownership.
- Do not log bodies, access tokens, Email addresses, or phone numbers to browser telemetry.
- Store timestamps as received and format them in the user's locale/time zone for display.
- Do not display raw `data` values as trusted HTML.

## UX guidance

- Use `unreadCount` for the badge and cap only its visual label (for example, `99+`).
- A notification remains in the inbox after a toast disappears.
- Announce genuinely new socket items through an accessible live region; do not announce REST-reconciled duplicates.
- Severity controls visual treatment only and never changes authorization.
- When SignalR is disconnected, keep the inbox usable through REST and show a subtle stale/reconnecting state.
- Mark-as-read may occur on explicit selection or after the product's chosen visibility rule; always use the PATCH endpoint.
- Disable/reconcile controls while their mutation is pending to avoid confusing repeated UI actions, even though server operations are idempotent.

## Troubleshooting

| Symptom | Verify |
|---|---|
| REST or hub returns `401` | The auth cookie/token exists, the account remains eligible, and refresh/login completed. |
| Hub negotiation works but no events arrive | Handlers were registered before `start()`, the business event completed, and REST contains the notification. |
| Notification appears only after refresh | WebSocket/proxy connectivity and client handler registration; REST behavior proves persistence. |
| Duplicate item | This can occur after retry; verify upsert by `id`. |
| Missed items after sleep/offline | Re-fetch first page and unread count after reconnect/app resume. |
| Initial connection never retries | `withAutomaticReconnect` does not retry initial `start()`; use the manual loop. |
| Badge differs between tabs | Apply read events and reconcile `/unread-count`. |
| Vite works but deployment fails | HTTPS, WebSocket upgrade forwarding, cookie attributes, origin, and credentialed CORS allowlist. |
| Another user's notification is visible | Treat as a security incident; preserve evidence and notify backend/security owners. |
| Action link is unsafe/unknown | Do not navigate; render the notification without an action and report the contract violation. |

## Frontend acceptance checklist

- [ ] Uses one shared connection to `/hubs/notifications`.
- [ ] Uses the existing HttpOnly cookie in browsers and does not persist tokens in JavaScript storage.
- [ ] Registers all three event handlers before connection start.
- [ ] Starts SignalR, then merges the authoritative first REST page by `id`.
- [ ] Treats the cursor as opaque and respects `pageSize` `1..50`.
- [ ] Supports read/unread filtering without reusing cursors across filters.
- [ ] Reconciles feed and count after reconnect, app resume, and auth refresh.
- [ ] Stops and clears notification state on logout/account change.
- [ ] Handles duplicate and missed socket events safely.
- [ ] Uses the returned REST DTO and server unread counts after mutations.
- [ ] Handles `400`, `401`, `404`, `405`, and `5xx` without assuming every error has JSON.
- [ ] Renders plain text and validates/allowlists relative action routes.
- [ ] Treats unknown notification types generically.
- [ ] Does not assume Email or SMS fallback exists.

## Compatibility rules

The REST routes, response property names, severity strings, and SignalR method names are public client contracts. Backend changes should be additive where possible. Removing or renaming a property/type/event requires a coordinated versioning and frontend rollout decision.

Adding a new notification `type` with the existing DTO is an additive change. Frontends must therefore retain a generic rendering path.

## Related documentation

- [Architecture Decision](./architecture.md)
- [Backend Producer Guide](./backend_integration_guide.md)
- [Implemented Plan and Verification](./implementation_plan.md)
- [Notification Core HTTP Verification Report](../../../SmartCourt.Tests/HttpTests/Notifications_Report.md)
- [Contracts Notification HTTP Verification Report](../../../SmartCourt.Tests/HttpTests/ContractsNotifications_Report.md)
- [Milestones Notification HTTP Verification Report](../../../SmartCourt.Tests/HttpTests/MilestonesNotifications_Report.md)
- [Payments Notification HTTP Verification Report](../../../SmartCourt.Tests/HttpTests/PaymentsNotifications_Report.md)

External references:

- [Microsoft: Authentication and authorization in ASP.NET Core SignalR](https://learn.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz)
- [Microsoft: ASP.NET Core SignalR JavaScript client](https://learn.microsoft.com/en-us/aspnet/core/signalr/javascript-client)
- [Microsoft: Use hubs in ASP.NET Core SignalR](https://learn.microsoft.com/en-us/aspnet/core/signalr/hubs)

# In-App Notifications V1 Implementation Plan

Status: **Backend implementation and notification verification complete**

Verification on 2026-08-09:

- solution build: passed;
- Notifications automated tests: 16/16 passed;
- monitored PowerShell HTTP lifecycle: 57/57 assertions passed;
- full repository suite: 650/675 passed, with 25 unrelated existing failures
  concentrated in legacy HTTP 200-vs-201 expectations plus one existing SQL
  connection failure. No Notifications test failed.

This plan implements the first approved increment of the SmartCourt notification system: a durable in-app inbox, authenticated REST APIs, SignalR delivery, and transactional-outbox triggers. Email and SMS are deliberately deferred.

## 1. Scope

### Included

- SQL Server-backed notification persistence.
- A Pure Vertical Slice under `SmartCourt/Features/Notifications` with controller, service interface/implementation, DTOs, validators, entity, event consumers, hub, and real-time notifier.
- Authenticated feed, unread-count, mark-one-read, and mark-all-read endpoints.
- Strongly typed SignalR events at `/hubs/notifications`.
- Notification creation from the existing `ProposalCreated`, `ProposalAccepted`, and `ProposalRejected` transactional outbox events.
- Effectively-once inbox materialization under at-least-once outbox delivery.
- A short-interval hosted outbox pump for near-real-time processing, with the existing Hangfire recurring dispatch retained as recovery.
- Backend unit, persistence, integration, authorization, outbox, and SignalR tests.
- A comprehensive PowerShell HTTP lifecycle test and Markdown report.

### Explicitly deferred

- Calls to `IEmailProvider` or `ISmsProvider`.
- Delivery-policy fields and `NotificationDelivery` records.
- Email/SMS scheduling, retries, receipts, templates, or escalation settings.
- Push/mobile notifications.
- User channel preferences and quiet hours.
- A distributed SignalR backplane; the initial deployment remains a single monolith instance.
- A public/test-only endpoint that directly creates notifications.
- Frontend package, store, connection-manager, bell, inbox, or layout changes. The
  frontend guide remains the server contract for a later frontend increment.

The deferred channel increment will consume the same persisted `Notification` records and event mappings. It must not require breaking changes to the V1 REST or SignalR DTOs.

## 2. Fixed architectural decisions

1. **No MediatR in the Notifications slice.** HTTP endpoints call `INotificationService`; background triggers implement the existing `IOutboxEventHandler` contract.
2. **REST is authoritative.** SignalR is a best-effort, potentially duplicate acceleration path.
3. **The existing transactional outbox is the trigger bus.** Producer features never invoke the hub directly.
4. **The entity belongs to the slice.** Use `Features/Notifications/Entities/Notification.cs`, consistent with the rule that feature-owned entities remain in their slice.
5. **Configuration follows the repository, not a hypothetical folder.** EF configuration goes in `SmartCourt/Persistence/Configurations`, because that is the actual configuration root currently applied by `ApplicationDbContext`.
6. **Cursor pagination uses a generated sequence.** A SQL `bigint IDENTITY` sequence avoids unstable page-number results and avoids database-specific GUID comparison logic.
7. **No SaveChanges dispatch interceptor in V1.** A short-interval hosted pump avoids the fact that `SavedChangesAsync` may occur before an explicit outer transaction commits.
8. **No public create endpoint.** The first E2E trigger is a real Proposal operation.

## 3. Target runtime flow

```mermaid
sequenceDiagram
    participant Client as "Client feature request"
    participant DB as "SQL Server"
    participant Pump as "Outbox pump"
    participant Handler as "Proposal notification handler"
    participant Hub as "SignalR"
    participant Lawyer as "Lawyer browser"

    Client->>DB: "Commit Proposal + ProposalCreated outbox row"
    DB-->>Client: "Committed HTTP result"
    loop "short configured interval"
        Pump->>DB: "Claim available outbox batch"
    end
    Pump->>Handler: "Handle ProposalCreated"
    Handler->>DB: "Insert Notification idempotently"
    Handler->>Hub: "NotificationCreated DTO"
    Hub-->>Lawyer: "Best-effort real-time event"
    Lawyer->>DB: "GET /api/notifications for reconciliation"
```

If SignalR delivery fails after the row is committed, the outbox handler remains retryable. A retry finds the existing notification through its idempotency key and may broadcast it again. The frontend deduplicates by notification `id`.

## 4. Domain and persistence changes

### 4.1 Notification entity

Create `SmartCourt/Features/Notifications/Entities/Notification.cs` as a sealed entity with a private EF constructor and guarded factory/mutation methods.

Fields:

| Field | Type | Rules |
|---|---|---|
| `Id` | `Guid` | Non-empty public identifier. |
| `Sequence` | `long` | Database-generated `IDENTITY`; immutable and unique. |
| `RecipientUserId` | `Guid` | Required user FK; immutable. |
| `SourceEventId` | `Guid` | Required outbox message ID; immutable. |
| `Type` | `string` | Required stable machine code, non-Unicode, max 100. |
| `Severity` | enum | `Information`, `Success`, `Warning`, `Critical`; persisted as integer. |
| `Title` | `string` | Required plain text, Unicode, max 200. |
| `Body` | `string` | Required plain text, Unicode, max 1,000. |
| `ActionUrl` | `string?` | Optional relative route, Unicode, max 500. |
| `DataJson` | `string?` | Optional non-sensitive JSON, Unicode, max 4,000. |
| `CreatedAtUtc` | `DateTime` | Required UTC value from `TimeProvider`. |
| `ReadAtUtc` | `DateTime?` | Null until read; UTC when present. |
| `ExpiresAtUtc` | `DateTime?` | Optional UTC business expiry. |
| `RowVersion` | `byte[]` | SQL rowversion concurrency token. |

Entity behavior:

- `Create(...)` validates IDs, UTC timestamps, lengths, relative action URL, valid severity, and JSON size/shape.
- `MarkRead(DateTime nowUtc)` is idempotent: the first call stores `ReadAtUtc`; later calls preserve the original timestamp.
- `IsRead` is a derived property and is not mapped as a second source of truth.
- No Data Annotations, public property setters, Email/SMS fields, soft-delete flag, or navigation collection is required.

Create `SmartCourt/Features/Notifications/Enums/NotificationSeverity.cs` with explicit values `1..4`.

### 4.2 EF Core configuration

Create `SmartCourt/Persistence/Configurations/NotificationConfiguration.cs`:

- table name `Notifications` and PK on `Id`;
- `Sequence` generated on add as `bigint IDENTITY` with a unique index;
- required/max-length/Unicode rules listed above;
- UTC property conventions for all timestamps;
- rowversion concurrency configuration;
- `RecipientUserId -> AspNetUsers.Id` with `DeleteBehavior.Restrict`;
- unique idempotency index named `UX_Notifications_Source_Recipient_Type` on `(SourceEventId, RecipientUserId, Type)`;
- feed index named `IX_Notifications_Recipient_Sequence` on `(RecipientUserId, Sequence DESC)`;
- filtered unread index named `IX_Notifications_Recipient_Unread_Sequence` on recipient/sequence with `[ReadAtUtc] IS NULL`;
- check constraint for severity range and a positive sequence where supported.

Update `ApplicationDbContext` with `DbSet<Notification> Notifications` and import only the slice entity namespace. Do not add notification auditing to the contract/payment append-only guards.

### 4.3 Migration

Generate one migration named `AddInAppNotificationsV1` from the active `ApplicationDbContext` model. Use the canonical `SmartCourt/Migrations` output directory because that directory owns the current `ApplicationDbContextModelSnapshot`. Do not hand-author the designer or snapshot.

Before accepting the migration:

1. List discoverable migrations and confirm the mixed historical `Migrations`/`Persistence/Migrations` namespaces are understood.
2. Verify the generated `Up` only creates the V1 `Notifications` table/indexes/FK/check constraints.
3. Verify it does not recreate the notification table removed by `20260806182109_RemoveNotificationsTable` in an unintended historical position.
4. Verify `Down` drops only the V1 table.
5. Generate an idempotent SQL script and inspect it.
6. Test both a fresh database and an upgrade from the current latest migration.

No `NotificationDelivery` table is created in V1.

## 5. Notification vertical slice

### 5.1 Implemented file structure

```text
SmartCourt/Features/Notifications/
├── NotificationsController.cs
├── INotificationService.cs
├── NotificationService.cs
├── DTOs/
│   ├── GetNotificationsRequest.cs
│   ├── NotificationDto.cs
│   ├── NotificationPageDto.cs
│   ├── NotificationReadDto.cs
│   ├── NotificationsReadAllDto.cs
│   └── UnreadNotificationCountDto.cs
├── Validators/
│   └── GetNotificationsRequestValidator.cs
├── Entities/
│   └── Notification.cs
├── Enums/
│   └── NotificationSeverity.cs
├── Hubs/
│   ├── INotificationClient.cs
│   └── NotificationsHub.cs
├── Realtime/
│   ├── INotificationRealtimeNotifier.cs
│   └── SignalRNotificationRealtimeNotifier.cs
├── Events/
│   ├── ProposalNotificationDefinitionMapper.cs
│   └── ProposalNotificationOutboxHandler.cs
└── Shared/
    ├── NotificationCursor.cs
    └── NotificationJson.cs
```

No command/query handler folders and no MediatR request types are added.

### 5.2 REST service contract

`INotificationService` exposes async methods with cancellation tokens:

```csharp
Task<NotificationPageDto> GetAsync(
    GetNotificationsRequest request,
    CancellationToken cancellationToken);

Task<UnreadNotificationCountDto> GetUnreadCountAsync(
    CancellationToken cancellationToken);

Task<NotificationDto> MarkReadAsync(
    Guid notificationId,
    CancellationToken cancellationToken);

Task<NotificationsReadAllDto> MarkAllReadAsync(
    CancellationToken cancellationToken);
```

Service rules:

- Require the user ID through `ICurrentUserService.RequireUserId`.
- Every query/update includes `RecipientUserId == currentUserId`.
- Use `AsNoTracking()` for reads.
- Manually map entities to DTOs; do not use AutoMapper.
- Treat a notification owned by another user as not found.
- `MarkReadAsync` is idempotent and broadcasts `NotificationRead` only after a successful database save. A repeated call returns the original read timestamp.
- `MarkAllReadAsync` changes only the caller's unread rows, returns the server timestamp and zero unread count, and emits one `NotificationsReadAll` event.
- Domain/not-found failures use the existing exception middleware; no manual 500 responses.

### 5.3 Cursor pagination

`GetNotificationsRequest` fields:

- `string? Cursor`
- `int PageSize = 20`, valid range `1..50`
- `bool? IsRead`

`NotificationCursor` Base64Url-encodes the last returned positive `Sequence` as an opaque versioned cursor. Invalid, non-positive, oversized, or unsupported cursors fail validation with a standardized 400 response.

Query behavior:

- order by `Sequence DESC`;
- when a cursor is present, filter `Sequence < decodedSequence`;
- apply the optional read filter;
- fetch `PageSize + 1` to determine `nextCursor`;
- compute `unreadCount` for the current user independently of the page filter;
- exclude expired notifications from the active feed only if an expiry is present and passed; retention remains separate.

### 5.4 HTTP endpoints

Create an `[Authorize]`, `[ApiController]` controller at `api/notifications`:

| Method | Route | Response |
|---|---|---|
| `GET` | `/api/notifications` | `ApiResponse<NotificationPageDto>` |
| `GET` | `/api/notifications/unread-count` | `ApiResponse<UnreadNotificationCountDto>` |
| `PATCH` | `/api/notifications/{notificationId:guid}/read` | `ApiResponse<NotificationDto>` |
| `PATCH` | `/api/notifications/read-all` | `ApiResponse<NotificationsReadAllDto>` |

Controllers call the service directly and wrap successful results with `ApiResponse<T>.Ok`. FluentValidation handles query validation. No endpoint accepts `RecipientUserId`, creates a notification, deletes history, or selects a delivery channel.

### 5.5 DTO contract

`NotificationDto` exposes:

- `Id`
- `Type`
- `Severity` as a stable string
- `Title`
- `Body`
- `ActionUrl`
- parsed non-sensitive `Data`
- `CreatedAtUtc`
- `ReadAtUtc`
- `ExpiresAtUtc`

It does not expose recipient IDs, source event IDs, row versions, sequence values, provider fields, or raw JSON.

## 6. Transactional outbox integration

### 6.1 Initial event coverage

Register one `ProposalNotificationOutboxHandler` for:

| Source event | Recipient | Notification type | Severity |
|---|---|---|---|
| `ProposalCreated` | `LawyerUserId` | `proposal.created` | `Information` |
| `ProposalAccepted` | `ClientUserId` | `proposal.accepted` | `Success` |
| `ProposalRejected` | `ClientUserId` | `proposal.rejected` | `Warning` |

All three are `InAppOnly` in V1. The mapper owns the plain-text title/body, safe relative `/proposals/{id}` action route, and a small JSON object containing `proposalId` and `legalCaseId`.

### 6.2 Handler algorithm

For each message:

1. Require event version `1`.
2. Deserialize `ProposalEventPayload` case-insensitively.
3. Verify `payload.ProposalId == message.AggregateId`.
4. Resolve the approved recipient/type/template through the mapper.
5. Look up an existing row by `(message.Id, recipientId, type)`.
6. If absent, construct and save the notification using `message.CreatedAt` as UTC creation time.
7. Broadcast the persisted DTO through `INotificationRealtimeNotifier`.

If a retry finds an existing row, it skips insertion and rebroadcasts the existing DTO. A unique constraint remains the final concurrency guard. No producer change is required for the existing Proposal events.

### 6.3 Direct notification requests

No generic `INotificationPublisher`/`NotificationRequestedV1` contract exists in V1. A future direct-request contract remains deferred until a real non-lifecycle use case is selected and approved. V1 establishes the durable inbox and proves the preferred semantic-event route first.

## 7. Near-real-time outbox processing

Add a general infrastructure worker, not a notification-specific timer:

- `Infrastructure/Providers/Events/OutboxDispatchOptions.cs`
- `Infrastructure/Providers/Events/OutboxDispatchBackgroundService.cs`

Implemented strongly typed settings:

```json
{
  "OutboxDispatch": {
    "Enabled": true,
    "BatchSize": 100,
    "IdleDelayMilliseconds": 1000,
    "ErrorDelayMilliseconds": 5000
  }
}
```

Validation ranges:

- batch size `1..500`;
- idle delay `250..30000` ms;
- error delay `1000..60000` ms.

Worker behavior:

- derive from `BackgroundService` and create a fresh DI scope for each dispatch call;
- call `IOutboxDispatcher.DispatchAvailableAsync` with the configured batch size;
- immediately continue when a full batch was processed;
- wait the idle interval when no messages were available;
- log infrastructure exceptions and wait the error interval;
- honor application cancellation without logging shutdown as an error;
- never hold an `ApplicationDbContext` across iterations.

The existing Hangfire recurring `contract-payment-outbox-dispatch` job remains enabled as a recovery sweep during V1. Renaming/generalizing that recurring job is allowed only if tests prove no duplicate registration or operational regression.

## 8. SignalR implementation

### Hub and routing

- Add `[Authorize] public sealed class NotificationsHub : Hub<INotificationClient>`.
- Expose no client-callable business methods.
- Do not manage per-user groups.
- Route with `IHubContext<NotificationsHub, INotificationClient>.Clients.User(recipientUserId.ToString())`; the existing JWT contains `ClaimTypes.NameIdentifier`.

### Strongly typed client methods

```csharp
Task NotificationCreated(NotificationDto notification);
Task NotificationRead(NotificationReadDto update);
Task NotificationsReadAll(NotificationsReadAllDto update);
```

`INotificationRealtimeNotifier` hides SignalR from services and outbox handlers and provides testable methods matching those events.

### Startup/authentication changes

- Register the service, notifier, event handler, options, and hosted pump in `DependencyInjection.cs`.
- Map `/hubs/notifications` in `Program.cs` and require authorization.
- Keep the existing `OnMessageReceived` path support for `/hubs/notifications`.
- Enable JWT bearer `CloseOnAuthenticationExpiration` so an expired connection must reconnect and reauthenticate.
- Do not add permissive CORS. The current frontend development path remains same-origin through Vite's `/hubs` proxy.

## 9. Frontend integration is deferred

This increment exposes and verifies the backend REST and SignalR contracts only.
It does not change `SmartCourtFE`. The existing frontend integration guide documents
how a future frontend increment can consume these contracts without changing the
backend DTOs or hub method names.

## 10. Automated test plan

### 10.1 Domain/entity tests

Create `SmartCourt.Tests/Features/Notifications/NotificationEntityTests.cs` covering:

- valid creation and immutable identity/source fields;
- empty IDs, invalid severity, non-UTC timestamps;
- maximum title/body/type/action/data lengths;
- invalid absolute/protocol-relative action URLs and malformed data JSON;
- first mark-read timestamp and repeated idempotent mark-read behavior;
- optional expiry validation.

### 10.2 Persistence and migration tests

Create notification-specific configuration/migration tests covering:

- table, columns, max lengths, Unicode choices, FK delete behavior, rowversion, and severity check;
- unique source/recipient/type index;
- unique sequence and feed/unread indexes;
- duplicate materialization rejected at the database boundary;
- migration discovery by `ApplicationDbContext`;
- clean `Up`/`Down` operations;
- SQL Server integration where provider-specific index/filter behavior matters;
- SQLite test factory compatibility for generated sequence/rowversion behavior.

Update `SmartCourtWebApplicationFactory` only as required to support the new generated sequence and rowversion; do not disable notification constraints globally.

### 10.3 Service/controller tests

Cover:

- 401 for all endpoints without authentication;
- newest-first first page and opaque next cursor;
- second page with no duplicates/gaps;
- invalid cursor and page size produce 400;
- read/unread filtering;
- unread count is scoped to the caller;
- cross-user notification is never returned or mutated;
- mark read succeeds, updates count, and is idempotent;
- mark all read affects only the caller;
- empty feed and zero-count responses;
- every successful/error response keeps the expected `ApiResponse<T>` contract.

### 10.4 Outbox/real-time tests

Cover:

- each Proposal event maps to the correct recipient, type, severity, route, and metadata;
- unsupported version or aggregate/payload mismatch fails deterministically;
- rolled-back Proposal transaction produces neither outbox nor notification;
- duplicate handler execution creates one notification;
- retry of an already-created notification can rebroadcast the same ID;
- a recording notifier receives only after the notification is saved;
- SignalR notifier targets `Clients.User` for the recipient and never `Clients.All`;
- multiple authenticated connections for one user receive the typed event;
- a different user receives nothing;
- hosted outbox pump processes available messages promptly, creates scopes per iteration, survives transient failure, and stops cleanly;
- existing recurring outbox dispatch remains safe alongside the pump.

## 11. PowerShell HTTP E2E script

Create:

- `SmartCourt.Tests/HttpTests/Notifications_Test.ps1`
- generated report `SmartCourt.Tests/HttpTests/Notifications_Report.md`

The script will reuse `TestHelpers.psm1`, use unique test users, emit explicit PASS/FAIL assertions, remove temporary files, and exit non-zero when any assertion fails. It will use bounded polling rather than fixed long sleeps for asynchronous outbox processing.

### Lifecycle exercised

1. Verify API health and assert anonymous notification access returns 401.
2. Register and confirm a Client and Lawyer using the existing mock Email log helper.
3. Log in both users and complete the minimum profiles.
4. Log in the seeded Admin, approve both accounts, then re-login for fresh eligible tokens.
5. Assert the Lawyer's notification feed is empty/known and unread count is zero before the trigger.
6. Create and finalize a real Case using the established lifecycle setup.
7. Create a Proposal as the Client for the Lawyer. This writes the existing `ProposalCreated` outbox event.
8. Poll `GET /api/notifications?isRead=false` as the Lawyer until the matching `proposal.created` item appears or the bounded timeout expires.
9. Assert type, severity, title/body presence, relative action route, proposal metadata, unread count, and `ApiResponse<T>` shape.
10. Assert the Client cannot mark the Lawyer's notification read (404) and the Lawyer's unread count remains unchanged.
11. Mark the notification read as the Lawyer; verify `ReadAtUtc` and unread count.
12. Mark it read again; verify success and the original read timestamp (idempotency).
13. Accept the Proposal as the Lawyer, poll for the Client's `proposal.accepted` notification, then exercise `read-all` as the Client.
14. Verify read/unread filters, malformed cursor validation, maximum page-size validation, and recipient isolation.
15. Write a summary containing generated IDs and totals but no tokens/passwords.

The script will not insert notifications directly into SQL, call a test-only trigger endpoint, or alter outbox/Hangfire tables. A separate automated SignalR integration test covers the actual socket broadcast; the HTTP script covers the requested login → trigger → fetch → mark-read lifecycle.

## 12. Documentation delivered

The documentation now:

- describes the backend/frontend contracts as implemented;
- records endpoint status codes, final DTO JSON, pagination, and SignalR events;
- documents the outbox producer pattern, worker settings, and operational behavior;
- identifies Email/SMS as future work;
- keeps frontend implementation explicitly outside this backend increment.

## 13. Implementation sequence and gates

### Gate A: baseline

- Confirm branch and preserve unrelated user changes.
- Run baseline backend build/tests before edits.
- Record any pre-existing failures separately.

### Gate B: domain/persistence

- Add entity, enum, EF configuration, DbSet, and migration.
- Pass domain, configuration, migration, and database tests.
- Inspect generated SQL before proceeding.

### Gate C: read/update slice

- Add DTOs, validator, service, controller, and REST integration tests.
- Verify ownership, cursor, unread count, and idempotent read behavior.

### Gate D: event and real-time path

- Add Proposal event mapper/handler, notifier, hub, outbox pump, DI, and hub mapping.
- Pass outbox idempotency, worker, and SignalR tests.

### Gate E: full verification

- Run targeted notification tests.
- Run the entire `SmartCourt.Tests` suite.
- Run migration discovery/idempotent SQL checks.
- Start the application and run `Notifications_Test.ps1` to completion.
- Verify authenticated and unauthenticated SignalR negotiation over HTTP.
- Review `git diff --check`, changed-file scope, logs, generated report, and docs.

No remote push is permitted at any gate. Any push requires all tests to pass and explicit user approval after final review.

## 14. Required commands after implementation

Exact paths/configuration may be adjusted to the installed SDK, but verification will include equivalents of:

```powershell
dotnet build SmartCourt.sln
dotnet test SmartCourt.Tests/SmartCourt.Tests.csproj --filter Notifications
dotnet test SmartCourt.sln
dotnet ef migrations list --project SmartCourt/SmartCourt.csproj --startup-project SmartCourt/SmartCourt.csproj --context ApplicationDbContext
dotnet ef migrations script --idempotent --project SmartCourt/SmartCourt.csproj --startup-project SmartCourt/SmartCourt.csproj --context ApplicationDbContext
pwsh -File SmartCourt.Tests/HttpTests/Notifications_Test.ps1
```

Commands that require package download, application startup outside the sandbox, or a local SQL Server will request approval when needed.

## 15. Acceptance criteria

The in-app V1 is complete only when:

- a committed Proposal lifecycle event creates exactly one notification for the correct recipient;
- a rolled-back or duplicate event cannot create an extra inbox row;
- an authenticated user can page/filter only their notifications;
- unread count, mark-one-read, and mark-all-read are correct and idempotent;
- a connected recipient receives the strongly typed SignalR event and other users do not;
- reconnecting clients can recover all state through REST;
- notification processing is normally near-real-time and the existing recurring sweep still recovers pending work;
- no Notification code uses MediatR, AutoMapper, Data Annotations, controllers from other slices, Email, or SMS providers;
- backend tests, complete solution tests, migration checks, and the PowerShell lifecycle script all pass;
- documentation and OpenAPI match the verified implementation;
- no code has been pushed.

## 16. Approval checkpoint

Approval of this plan authorizes implementation of the in-app V1 scope only. It does **not** authorize Email/SMS work or a remote push. After implementation and all verification gates, the result will be presented for final review before any push.

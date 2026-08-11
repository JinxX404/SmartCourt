# Per-Slice Notification Integration Plan

Status: **Gates 0–3, Gate 5, and Gate 6 implemented and verified from current local main; Gate 4 intentionally skipped; Gate 6 is stopped for review**
Branch: `codex/notification-gate-6-user-verification-from-main` (based on current local `main`; merge target: local `main`)
Scope: backend only; in-app notifications first; Arabic display copy; Email/SMS deferred.

This plan converts the approved [Notification Opportunity Catalog](./notification_opportunity_catalog.md) into small, independently reviewable slice integrations. It does not authorize implementation by itself. After each slice is implemented and tested, work stops until the user reviews and explicitly approves continuing.

## 1. Approved decisions

The user approved these recommendations:

1. Existing business rules and endpoint behavior remain unchanged.
2. When an owning slice lacks a durable fact, the only allowed source-slice change is a minimal semantic outbox event added to the existing business transaction.
3. Notifications use Arabic plain-text titles and bodies for this increment.
4. Machine contracts remain English: event names, notification types, JSON keys, enum values, and C# identifiers.
5. A user action normally notifies the affected counterparty, not the actor.
6. A system lifecycle transition may notify both affected parties.
7. Financial outcomes notify the financially affected user and may provide the initiator a durable receipt.
8. Admin/moderator work is routed to an assigned user or queue policy; it is never broadcast blindly to every privileged account.
9. Implementation order is Contracts, Milestones, Payments, Disputes, Verification, Auth/security, then optional Chat/async processing.
10. Frontend code, frontend routes, Email, SMS, and unrelated slice behavior are out of scope.

## 2. Simplifying constraints

### 2.1 No unapproved action URLs

Frontend routes have not been provided and the frontend is out of scope. New notification types therefore use `ActionUrl = null` during backend integration. Stable resource IDs remain in `data`, allowing a future frontend-approved route mapping without inventing routes now.

The existing Proposal action URL remains unchanged because it is already part of the implemented contract. Any new `actionUrl` requires explicit approval and a documented frontend contract update.

### 2.2 Arabic-only display snapshot for now

Titles and bodies are stored as concise Modern Standard Arabic plain text. The database does not gain localization tables or culture columns in this integration series. If multilingual notifications are required later, a separately approved template/localization design can replace mapper copy while preserving machine `type` values.

Arabic copy rules:

- no HTML;
- no secrets or contact/payment destination details;
- no full evidence, contract, rejection, or dispute reason;
- use a neutral safe summary and direct the user to the authorized resource details;
- do not include actor names until naming/privacy rules are approved;
- keep terminology consistent across Contract, Milestone, Payment, and Dispute mappings.

### 2.3 Event-driven stories before reminders

To keep the system simple, event-driven facts are implemented first. Time-based reminder stories such as review-deadline, hold-expiry, delayed payment, and expiring verification documents remain documented but are deferred to a later reminder stage after the core slices pass review.

This defers catalog items `CON-08`, `MIL-10`, `MIL-14`, `PAY-01`, `DSP-06`, and `VER-07`. It does not remove them from the catalog.

### 2.4 No direct notification publisher

There will be no public create endpoint and no generic feature-facing API that accepts arbitrary title/body. A feature emits a semantic business event. Notifications owns recipients, Arabic copy, severity, metadata, persistence, and SignalR.

## 3. Reusable Notifications-side pipeline

The first implementation gate creates the extension mechanism once. Later slices add mappers instead of copying persistence/SignalR code.

### 3.1 Proposed Notifications-owned contracts

```csharp
internal interface INotificationEventMapper
{
    IReadOnlyCollection<string> EventTypes { get; }

    Task<IReadOnlyCollection<NotificationDraft>> MapAsync(
        OutboxMessage message,
        CancellationToken cancellationToken);
}

internal sealed record NotificationDraft(
    Guid RecipientUserId,
    string Type,
    NotificationSeverity Severity,
    string Title,
    string Body,
    string? ActionUrl,
    IReadOnlyDictionary<string, string>? Data,
    DateTime? ExpiresAtUtc = null);
```

`NotificationDraft` is internal. Feature endpoints never construct it.

### 3.2 Generic outbox handler

Replace `ProposalNotificationOutboxHandler` with one `NotificationOutboxHandler` that:

1. receives all registered `INotificationEventMapper` implementations;
2. builds an ordinal event-type lookup;
3. fails deterministically when two notification mappers claim the same event type;
4. advertises the union of mapper event types through `IOutboxEventHandler.EventTypes`;
5. delegates event version/payload/business-fact validation to the selected mapper;
6. accepts zero, one, or several recipient drafts;
7. rejects empty recipient IDs, duplicate `(RecipientUserId, Type)` drafts, or invalid mapped content;
8. resolves existing rows by `(SourceEventId, RecipientUserId, Type)`;
9. inserts all missing rows and calls `SaveChangesAsync` once;
10. broadcasts the persisted DTO for each draft after persistence;
11. reuses the existing notification ID when an outbox retry rebroadcasts.

The database unique constraint remains the final concurrency safeguard.

### 3.3 Cross-handler retry behavior

The current `OutboxDispatcher` invokes every handler registered for an event and marks the message processed only after all succeed. Contract lifecycle events are already consumed by Chat/system-message handlers. Therefore:

- Notifications materialization must remain idempotent;
- existing Chat handlers must remain idempotent under retry;
- a notification failure may replay a Chat handler and vice versa;
- unit/integration tests must cover a retry after one consumer has already persisted its side effect.

The dispatcher itself is not changed unless a failing test proves a shared infrastructure defect. Such a change would require a separate approval because it affects every outbox consumer.

### 3.4 Cross-slice read boundary

Many existing lifecycle payloads contain only an aggregate ID. To obey `AGENTS.md`, Notifications should not reproduce Contract/Milestone/Payment/Dispute business queries internally.

Expose the existing Contract event-context resolver through a narrow integration interface, for example:

```csharp
public interface IContractNotificationContextReader
{
    Task<ContractNotificationContext> ResolveAsync(
        OutboxMessage message,
        CancellationToken cancellationToken);
}
```

The implementation remains in the Contracts integration boundary and reuses the existing resolver query behavior. The context contains identifiers and actor-independent relationship facts only. It does not return entities or authorize an HTTP request.

This is an extension boundary, not a change to Contract business rules.

### 3.5 Proposal migration and Arabic baseline

Move the three Proposal mappings into `ProposalNotificationEventMapper`, keeping their types, recipients, metadata, and existing action URL unchanged.

Approved Arabic baseline proposed for review:

| Type | Title | Body |
|---|---|---|
| `proposal.created` | `عرض جديد` | `أرسل إليك موكل عرضًا جديدًا لمراجعته.` |
| `proposal.accepted` | `تم قبول العرض` | `وافق المحامي على عرضك.` |
| `proposal.rejected` | `تم رفض العرض` | `رفض المحامي عرضك. يمكنك مراجعة التفاصيل واختيار محامٍ آخر.` |

Changing display copy does not change DTO shape or machine type. Existing Proposal tests, HTTP report expectations, and documentation are updated in the foundation gate.

## 4. Universal per-slice gate

Every slice follows the same sequence.

### A. Analyze only that slice

- Re-read controller, service/handler, DTOs, validators, entities, and emitted events.
- Confirm actual actor/recipient IDs and transaction boundaries.
- Freeze the slice's notification type/copy/data matrix.
- Identify the minimal allowed files before editing.

### B. Implement only that slice

- Add a Notifications mapper and tests.
- Reuse existing events wherever sufficient.
- When necessary, add only constants/payload records and `IOutboxWriter.EnqueueAsync(...)` to the existing transaction.
- Do not alter authorization, validation, state-transition conditions, response DTOs, endpoint routes, or HTTP status behavior.

### C. Automated verification

- build the solution;
- run Notifications domain/persistence/realtime tests;
- run the affected slice's existing automated tests;
- run new mapper, event, recipient, Arabic-copy, retry, and idempotency tests;
- run relevant authorization and recipient-isolation integration tests;
- run `git diff --check` and review changed-file scope.

### D. HTTP verification using `generate-http-test`

Create a zero-assumption PowerShell script in `SmartCourt.Tests/HttpTests` that:

- creates all users and state from scratch;
- extracts Email confirmation tokens from the existing test log;
- confirms, logs in, completes profiles, and obtains fresh eligible sessions;
- exercises every endpoint in the target slice, including success, validation, authorization, ownership, malformed input, and selected hostile/Unicode inputs;
- executes each notification-producing action through its real HTTP endpoint;
- polls the intended recipient's feed with a bounded retry window;
- asserts exact Arabic title/body, machine type, severity, data IDs, unread count, ownership isolation, mark-read idempotency, and read-all where relevant;
- proves the actor does not receive a counterparty-only notification;
- records endpoint, request body, HTTP status, and full sanitized response in a Markdown report;
- exits non-zero when any assertion fails.

### E. Live monitoring and stop

- start the real API/test environment;
- monitor API and script output while the lifecycle runs;
- inspect outbox failure/attempt state if polling times out;
- publish the assertion totals and report path;
- report unrelated baseline failures separately;
- stop work and wait for explicit approval before opening the next slice.

No slice is considered complete if its HTTP report contains a failed assertion.

## 5. Gate 0 — Shared pipeline and Proposal regression

This gate must pass before Contracts because every later slice depends on it.

### Allowed production changes

- Add `NotificationDraft` and `INotificationEventMapper` inside Notifications.
- Add the generic `NotificationOutboxHandler`.
- Convert Proposal mapping to `ProposalNotificationEventMapper`.
- Replace only the Notifications handler/mapper DI registrations.
- Do not change Proposal endpoint/business logic or its event payload.

### Tests

- mapper registry rejects duplicate event ownership;
- unsupported type/version and invalid payload fail;
- multiple drafts persist atomically and independently;
- duplicate/retry execution retains IDs and one row per recipient/type;
- SignalR failure after persistence is retry-safe;
- Arabic copy and JSON metadata round-trip;
- all existing 16 Notifications tests remain passing or are updated without losing coverage;
- rerun `Notifications_Test.ps1` from zero state and regenerate `Notifications_Report.md` with exact Arabic expectations.

### Review checkpoint

Stop after the shared pipeline and Proposal regression report. Contracts begins only after approval.

### Gate 0 execution record — 2026-08-09

Status: **implemented and awaiting review approval**.

- Added the shared mapper registry, draft contract, idempotent/atomic materializer, and post-persistence SignalR delivery.
- Migrated only the existing Proposal event mappings; no Proposal controller, service, endpoint, payload, or other slice business logic changed.
- Applied the approved Arabic Proposal titles/bodies while retaining existing machine types, recipients, severity, metadata, and `/proposals/{proposalId}` action URL.
- Focused automated result: `25 passed, 0 failed` for `SmartCourt.Tests.Features.Notifications`.
- Monitored HTTP result: `61 passed, 0 failed`, including registration, two confirmations found in mock Email logs, login/profile/approval setup, all three Proposal notifications, exact Arabic payloads, ownership isolation, pagination, read operations, validation, hostile inputs, and unsupported methods.
- The generated HTTP report is redacted and contains no passwords, access/refresh/confirmation tokens, JWTs, test Email addresses, phone numbers, or national numbers.
- Full repository result: `654 passed, 26 failed, 0 skipped` out of `680`. The failures are outside Notifications and are dominated by existing controller-test status mismatches (`201 Created` expected while the current API returns `200 OK`) plus SQL integration timeouts. Nothing was changed in those slices under this gate.
- No notification/outbox pipeline error appeared in the monitored server output. The two middleware error logs were the intentional `404` ownership/missing-notification cases asserted by the HTTP suite.
- No code was pushed. Gate 1 (Contracts) remains blocked on explicit review approval.

## 6. Gate 1 — Contracts

Catalog scope: `CON-01` through `CON-07`. `CON-08` remains in the later reminder/operations stage.

### Event matrix and Arabic copy

| Fact/type | Recipient | Severity | Arabic title | Arabic body | Source change |
|---|---|---|---|---|---|
| `contract.created` | Client | `Information` | `مسودة عقد جديدة` | `أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.` | Reuse `ContractCreated`. |
| `contract.draft-updated` | Client | `Warning` | `تم تحديث مسودة العقد` | `تم تحديث شروط العقد، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.` | Add new event after accepted timestamps are cleared. |
| `contract.acceptance-recorded` | Other participant when their acceptance is still required | `Information` | `موافقة جديدة على العقد` | `وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.` | Enrich `ContractAccepted` payload with actor ID/version and whether counterparty acceptance remains required. |
| `contract.activated` | Client and Lawyer | `Success` | `تم تفعيل العقد` | `أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.` | Reuse `ContractActivated`. |
| `contract.completed` | Client and Lawyer | `Success` | `اكتمل العقد` | `اكتملت جميع مراحل العقد وتسوياته بنجاح.` | Reuse `ContractCompleted`. |
| `contract.termination-requested` | Counterparty; requester confirmation only when settlement is pending | `Warning` | `تم طلب إنهاء العقد` | `تم تسجيل طلب إنهاء العقد، وتجري معالجة التسوية اللازمة.` | Add event when termination request is first persisted. |
| `contract.terminated` | Client and Lawyer | `Warning` | `تم إنهاء العقد` | `اكتملت إجراءات إنهاء العقد وتسويته.` | Reuse `ContractTerminated`. |

All drafts include `contractId`, `proposalId`, and `legalCaseId`. New types use `ActionUrl = null`.

### Minimal Contracts-slice extensions

- expose the narrow event-context reader interface/implementation;
- define `ContractDraftUpdated` and `ContractTerminationRequested` event constants/payloads;
- add event emission to `UpdateDraftAsync` and the first termination-request persistence;
- change `ContractAccepted` to a new versioned payload containing `ContractId` and `AcceptedByUserId`;
- keep older handlers compatible with the event type;
- do not change contract state transitions, concurrency, authorization, settlement, or response mapping.

### Contract mapper behavior

- event versions are explicit;
- recipient is derived from Contract participant IDs;
- acceptance notifies `ClientUserId` when actor is the lawyer and `LawyerUserId` when actor is the client;
- activated/completed/terminated return two drafts with the same type and different recipients;
- termination-request retry does not duplicate final termination;
- requester and counterparty never receive the wrong role-specific message.

### HTTP artifact

- `ContractsNotifications_Test.ps1`
- `ContractsNotifications_Report.md`

It covers all Contracts endpoints plus create, update/reset acceptance, both participant acceptances, activation prerequisites, termination with and without settlement, list/get/history, authentication/role/ownership, validation, concurrency headers, and all corresponding notification assertions.

### Stop condition

Stop after the Contract report passes and present changed files, automated totals, HTTP totals, exact Arabic payload samples, and any unrelated failures.

### Gate 1 execution record — 2026-08-09

Status: **implemented and awaiting review approval**.

- Contract notification mapper tests: `40 passed, 0 failed` across the Notifications-focused suite.
- Contract service integration tests: `16 passed, 0 failed`.
- Monitored HTTP lifecycle: `146 passed, 0 failed`; the report is `SmartCourt.Tests/HttpTests/ContractsNotifications_Report.md`.
- The HTTP run used the SMTP mock and confirmed all three test accounts from the generated confirmation links found in the API log.
- The final server-log segment contained exactly three expected mock Email deliveries and no notification-mapper, outbox, unhandled, fatal, or exception entries.
- The report contains no failed assertions and redacts passwords, access/refresh/confirmation tokens, Email addresses, phone numbers, national numbers, and payment references.
- Full repository result: `675 passed, 24 failed, 0 skipped` out of `699`. All 24 failures are existing controller/API expectations for `201 Created` while the current runtime returns `200 OK`; no Notification or Contract event-mapping test failed.
- No frontend source, Email/SMS delivery behavior, or unrelated slice business rule was changed. No code was pushed.
- Gate 2 (Milestones) was explicitly approved after this Gate 1 review and is recorded below.

## 7. Gate 2 — Milestones

Catalog scope: `MIL-01` through `MIL-05`, `MIL-09`, `MIL-11` through `MIL-13`, and `MIL-15` through `MIL-18`. Funding outcomes `MIL-06` through `MIL-08` are emitted by the Payments slice and remain in Gate 3. Deadline reminders `MIL-10` and `MIL-14` are deferred.

### Notification families

| Types | Recipient rule | Event readiness |
|---|---|---|
| `milestone.created`, `milestone.draft-updated`, `milestone.acceptance-recorded`, `milestone.approved` | Counterparty or both when fully approved | New semantic events in existing Milestone transactions. |
| `milestone.ready-for-funding` | Client | Existing event. |
| `milestone.submitted` | Client | Existing event and submission version. |
| `milestone.changes-requested` | Lawyer | Existing event. |
| `milestone.accepted` | Lawyer | Existing event. |
| `milestone.auto-accepted` | Client and Lawyer with role-appropriate Arabic body/severity | Existing event. |
| `milestone.change-request-created` | Other participant | Existing event payload needs requester relationship resolved. |
| `milestone.change-request-approved` / `rejected` | Requester | Existing event. |
| `milestone.change-request-cancelled` | Other participant | Existing event. |

### Minimal Milestones-slice extensions

- inject/reuse `IOutboxWriter` in `MilestoneDraftService` without changing rules;
- emit created/updated events in the same save;
- emit participant-approved/fully-approved facts from `ApproveAsync` with actor ID;
- reuse all current execution/change-request events unchanged when payloads are sufficient;
- do not alter automatic acceptance or scheduling behavior in this gate.

### Arabic copy review

Status: **implemented exactly as approved and verified**.

All Gate 2 notifications use `actionUrl: null`. Every item contains `milestoneId`, `contractId`, `proposalId`, and `legalCaseId`; formal change-request items additionally contain `changeRequestId`. Machine types and `data` keys remain English while the persisted display copy is Arabic.

| Type | Recipient | Severity | Arabic title | Arabic body |
|---|---|---|---|---|
| `milestone.created` | Other participant | `Information` | `مرحلة تعاقدية جديدة` | `أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.` |
| `milestone.draft-updated` | Other participant | `Warning` | `تم تحديث المرحلة` | `تم تحديث شروط المرحلة، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.` |
| `milestone.acceptance-recorded` | Other participant when their approval is still required | `Information` | `موافقة جديدة على المرحلة` | `وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك.` |
| `milestone.approved` | Client and Lawyer | `Success` | `تم اعتماد المرحلة` | `وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.` |
| `milestone.ready-for-funding` | Client | `Information` | `المرحلة جاهزة للتمويل` | `أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.` |
| `milestone.submitted` | Client | `Information` | `تم تسليم أعمال المرحلة` | `سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.` |
| `milestone.changes-requested` | Lawyer | `Warning` | `طُلبت تعديلات على المرحلة` | `طلب العميل تعديلات على أعمال المرحلة، ويمكنك مراجعة الطلب وإعادة التسليم.` |
| `milestone.accepted` | Lawyer | `Success` | `تم قبول أعمال المرحلة` | `قبل العميل أعمال المرحلة، وبدأت مدة حجز المبلغ قبل إتاحته للصرف.` |
| `milestone.auto-accepted` | Client | `Warning` | `تم قبول المرحلة تلقائيًا` | `انتهت مدة المراجعة وقُبلت أعمال المرحلة تلقائيًا، وبدأت مدة الاعتراض.` |
| `milestone.auto-accepted` | Lawyer | `Success` | `تم قبول المرحلة تلقائيًا` | `قُبلت أعمال المرحلة تلقائيًا بعد انتهاء مدة المراجعة، وبدأت مدة حجز المبلغ.` |
| `milestone.change-request-created` | Other participant | `Information` | `طلب تعديل جديد للمرحلة` | `أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.` |
| `milestone.change-request-approved` | Requester | `Success` | `تمت الموافقة على طلب التعديل` | `وافق الطرف الآخر على طلب تعديل المرحلة، وطُبّقت الشروط المعتمدة.` |
| `milestone.change-request-rejected` | Requester | `Warning` | `تم رفض طلب تعديل المرحلة` | `رفض الطرف الآخر طلب تعديل المرحلة. يمكنك مراجعة الطلب لمعرفة التفاصيل.` |
| `milestone.change-request-cancelled` | Other participant | `Information` | `تم إلغاء طلب تعديل المرحلة` | `ألغى الطرف الآخر طلب تعديل المرحلة، ولم يعد القرار مطلوبًا منك.` |

The notification does not copy milestone descriptions, submission notes, change-request text, rejection reasons, payment amounts, or file identifiers. Those details remain behind their authorized APIs.

### HTTP artifact

- `MilestonesNotifications_Test.ps1`
- `MilestonesNotifications_Report.md`

The script covers every Milestones endpoint, both participant roles, optimistic-concurrency headers, lifecycle prerequisites, invalid transitions, all change-request decisions, manual acceptance, and an accelerated/test-controlled auto-accept scenario without fixed long sleeps.

### Stop condition

Stop after all Milestone notification and HTTP assertions pass.

### Execution record

- The shared mapper pipeline now handles all thirteen approved Milestone notification types; display title/body snapshots are persisted in Arabic while machine types and `data` keys remain English.
- `MilestoneDraftService` writes created/update facts and `MilestoneService.ApproveAsync` writes actor-aware acceptance/approved facts inside their existing EF saves. Existing execution, automatic-acceptance, and change-request facts are reused.
- Focused results before the full regression: Notification tests `62/62`; Milestone service tests `20/20`; Milestones namespace `48 passed, 4 failed`, where all four failures are pre-existing HTTP `201 Created` expectations against the current `200 OK` runtime.
- Full repository regression: `697 passed, 24 failed, 0 skipped` out of `721`; the failure set is unchanged from the pre-Milestones baseline and contains only existing `201 Created` expectations against the current `200 OK` runtime.
- Monitored HTTP lifecycle: `143 passed, 0 failed`; all three zero-assumption accounts were confirmed from mock Email log links.
- The HTTP report is fully redacted: no raw password, access/refresh/confirmation token, JWT, Email address, phone/national number, or payment reference remains.
- The final HTTP log window contains exactly three mock Email deliveries and no notification/outbox/unhandled failure signal.
- No frontend source, Email/SMS delivery behavior, Payment logic, or unrelated slice business rule was changed. No code was pushed.
- Gate 2 was subsequently reviewed and approved; Gate 3 execution is recorded below.

## 8. Gate 3 — Payments and wallets

Catalog scope: event-driven `MIL-06` through `MIL-08`, `MIL-19`, `MIL-20`, and `PAY-02` through `PAY-06`. Delayed-provider threshold `PAY-01` is deferred.

Status: **implemented and verified; awaiting review approval**.

### Notification families

- `milestone.funding-started` → Lawyer, informational;
- `milestone.funded` → Client and Lawyer, success with role-appropriate body;
- `milestone.funding-failed` → Client, critical/actionable;
- `funds.released` → Lawyer receipt and Client confirmation;
- `funds.refunded` → Client receipt and Lawyer confirmation;
- `wallet.withdrawal-completed`, `wallet.withdrawal-failed`, `wallet.withdrawal-delayed` → Lawyer;
- `wallet.adjusted` → Lawyer.

### Minimal Payments-slice extensions

- reuse existing funding/release/refund events;
- add versioned withdrawal outcome events at final/uncertain state transitions;
- add a wallet-adjusted event inside the serializable adjustment transaction;
- include amounts/currency only when product-safe and invariant; never include destination references/provider secrets;
- do not notify on webhook/retry attempts that make no final state change.

### Arabic copy and data contract

All Gate 3 notifications use `actionUrl: null`. Financial amounts, administrative reasons, withdrawal destinations, provider identifiers, failure details, and idempotency keys are intentionally excluded. Funding items contain `milestoneId`, `contractId`, `proposalId`, and `legalCaseId`. Settlement items add `escrowHoldId` and `paymentTransactionId`; withdrawal items contain only `withdrawalId`; adjustment items contain `walletAdjustmentId` and `contractId`.

| Type / recipient variant | Severity | Arabic title | Arabic body |
|---|---|---|---|
| `milestone.funding-started` / Lawyer | `Information` | `بدأ تمويل المرحلة` | `بدأت معالجة تمويل المرحلة. انتظر تأكيد اكتمال التمويل قبل بدء العمل.` |
| `milestone.funded` / Client | `Success` | `تم تمويل المرحلة` | `اكتمل تمويل المرحلة وحُفظ المبلغ في حساب الضمان.` |
| `milestone.funded` / Lawyer | `Success` | `تم تمويل المرحلة` | `اكتمل تمويل المرحلة، ويمكنك الآن بدء العمل عليها.` |
| `milestone.funding-failed` / Client | `Critical` | `فشل تمويل المرحلة` | `لم تكتمل عملية تمويل المرحلة. يمكنك مراجعة وسيلة الدفع والمحاولة مرة أخرى.` |
| `funds.released` / Client | `Success` | `تم تحرير أموال المرحلة` | `انتهت مدة الحجز وتم تحرير مستحقات المحامي عن المرحلة.` |
| `funds.released` / Lawyer | `Success` | `أصبحت مستحقات المرحلة متاحة` | `تم تحويل مستحقات المرحلة إلى رصيد محفظتك المتاح.` |
| `funds.refunded` / Client | `Success` | `تم رد أموال المرحلة` | `اكتملت تسوية المرحلة وتم رد الأموال إلى العميل.` |
| `funds.refunded` / Lawyer | `Information` | `تم رد أموال المرحلة` | `اكتملت تسوية المرحلة برد الأموال إلى العميل.` |
| `wallet.withdrawal-completed` / Lawyer | `Success` | `اكتمل طلب السحب` | `اكتمل طلب سحب الرصيد من محفظتك بنجاح.` |
| `wallet.withdrawal-failed` / Lawyer | `Warning` | `فشل طلب السحب` | `لم يكتمل طلب السحب، وأُعيد المبلغ إلى رصيد محفظتك المتاح.` |
| `wallet.withdrawal-delayed` / Lawyer | `Warning` | `طلب السحب يحتاج إلى مراجعة` | `تأخر حسم طلب السحب ويجري التعامل معه يدويًا. لا تنشئ طلبًا بديلًا.` |
| `wallet.adjusted` / Lawyer | `Warning` | `تم تصحيح رصيد المحفظة` | `أجرى مسؤول النظام تصحيحًا ماليًا على محفظتك. راجع الرصيد الحالي والتفاصيل مع الدعم عند الحاجة.` |

The mock payment provider recognizes `mock-success*`, `mock-fail*`, and `mock-timeout*` withdrawal destination references only while the configured mock provider is active. This supplies deterministic completed, failed, and uncertain outcomes for HTTP verification; production providers and business rules are unchanged. Because the mock provider is scoped and keeps provider results in memory, an explicit retry in a later request safely remains processing when its original result cannot be confirmed; the test asserts that public contract rather than inventing a success path.

### HTTP artifact

- `PaymentsNotifications_Test.ps1`
- `PaymentsNotifications_Report.md`

The generated script covers all Payment, Wallet, AdminEscrow, and AdminWallet endpoints; direct and webhook funding; signed webhook rejection/replay; retry uncertainty; release/refund settlement; completed, failed, and delayed withdrawals; wallet adjustments; roles; idempotency; validation; unsupported methods; exact Arabic payloads; sensitive-data exclusion; and recipient isolation.

### Gate 3 execution record

- Focused mapper/wallet/adjustment tests: `24 passed, 0 failed`.
- Payments namespace regression: `87 passed, 6 failed`; all six failures are the unchanged `201 Created` versus runtime `200 OK` baseline.
- Full repository regression: `710 passed, 24 failed, 0 skipped` out of `734`; the same 24 pre-existing status-code expectations remain and no new failure was introduced.
- Complete Payments HTTP lifecycle: `210 passed, 0 failed`.
- Three zero-assumption accounts were confirmed from mock Email log links.
- The generated report contains no unredacted authentication token, password, payment reference, provider identifier, withdrawal destination, or webhook signature.
- The final run had no notification/outbox dispatch failure. Its critical wallet log is the deliberate SLA-delayed withdrawal escalation exercised by the test.
- No frontend source, Email/SMS delivery behavior, production payment provider, Payment endpoint, authorization rule, or unrelated slice business logic was changed. No code was pushed.
- Gate 4 was intentionally skipped in the requested sequence. Gate 5 was implemented from the current `main` branch, verified, and is ready for local merge into `main`.

### Stop condition

Stop after the Payment/Wallet report passes. Any provider-reconciliation uncertainty is reported explicitly rather than treated as delivery success.

## 9. Gate 4 — Disputes

Catalog scope: event-driven `DSP-01` through `DSP-05`, `DSP-07`, and `DSP-08`. Delayed settlement `DSP-06` is deferred.

### Notification families

- `dispute.opened` → counterparty; moderator queue routing deferred unless assigned;
- `dispute.evidence-added` → counterparty and assigned moderator, excluding actor;
- `dispute.assigned` → assigned moderator plus both participants;
- `dispute.review-started` → both participants;
- `dispute.resolved` → both participants with safe summary only;
- `lawyer.penalty-applied` → lawyer;
- `dispute.closed` → both participants.

### Minimal Disputes-slice extensions

- reuse Opened/Assigned/Resolved/Closed events;
- add EvidenceAdded and ReviewStarted events to their existing saves;
- add a separate penalty event only when a penalty record is committed;
- do not include evidence content, full resolution summary, or penalty reason in event/notification metadata;
- reuse FundsReleased/FundsRefunded notifications instead of duplicating settlement receipts.

### HTTP artifact

- `DisputesNotifications_Test.ps1`
- `DisputesNotifications_Report.md`

It covers every Disputes endpoint, both participants, moderator/admin roles, evidence, assignment, review, settlement outcomes, penalty authorization, close prerequisites, hostile input, recipient isolation, and Arabic notification payloads.

### Stop condition

Stop after Dispute report review.

## 10. Gate 5 — Administrative verification decisions

Catalog scope: `VER-02` through `VER-06`. Expiry reminder `VER-07` is deferred.

### Notification families

- `verification.document-approved` → document owner;
- `verification.document-rejected` → document owner;
- `verification.document-expired` → document owner;
- `account.approved` → user only on actual transition to `Active`;
- `account.rejected` → user.

### Minimal Admin-slice extensions

- enqueue `VerificationDocumentApproved`, `VerificationDocumentRejected`, `VerificationDocumentExpired`, `VerificationAccountApproved`, and `VerificationAccountRejected`, all version `1`, from the existing review/approve/reject handlers before their save;
- use the current user/account/document IDs and bounded status/document type, then let `VerificationNotificationEventMapper` resolve the recipient through the Verification-owned context reader;
- never include document storage paths, file URLs/content, private review comments, or full rejection reasons in the event or notification;
- prevent one account-approved notification per document by detecting the actual account status transition to `Active`, and emit account rejection only on transition to `Rejected`;
- preserve legacy MediatR handlers without introducing new MediatR notification dispatch.

### Gate 5 Arabic copy and data contract

All five notification types use `actionUrl: null`. `verification.document-approved` is `Success`, titled `تم اعتماد مستند التحقق`, with body `تم اعتماد أحد مستندات التحقق الخاصة بك. يمكنك متابعة حالة التحقق من حسابك.`. `verification.document-rejected` is `Warning`, titled `تم رفض مستند التحقق`, with body `تم رفض أحد مستندات التحقق الخاصة بك. يرجى مراجعة التفاصيل واستبدال المستند عند الحاجة.`. `verification.document-expired` is `Warning`, titled `انتهت صلاحية مستند التحقق`, with body `انتهت صلاحية أحد مستندات التحقق الخاصة بك. يرجى إعادة رفع مستند ساري المفعول.`. `account.approved` is `Success`, titled `تم اعتماد حسابك`, with body `تم اعتماد حسابك وأصبح جاهزًا للاستخدام.`. `account.rejected` is `Critical`, titled `تم رفض الحساب`, with body `تم رفض طلب اعتماد حسابك. يرجى مراجعة التفاصيل واتخاذ الإجراء المطلوب.`.

Document notification data contains only `documentId` and `documentType`; account notification data contains only `userId`. Storage paths, file URLs/content, full rejection reasons, private review comments, Email/phone/national numbers, provider IDs, tokens, and idempotency keys are forbidden. The shared outbox message ID makes materialization idempotent; REST is the durable delivery path and SignalR is best-effort.

### HTTP artifact

- `AdminVerificationNotifications_Test.ps1`
- `AdminVerificationNotifications_Report.md`

It covers all Admin Verification endpoints, roles, pending queue/detail/content, approve/reject/expired outcomes, account transition deduplication, concurrency behavior, validation, exact Arabic recipient notifications, forbidden metadata, recipient isolation, mock Email confirmation, and API/outbox/provider log monitoring. The corrected-from-main final report records `122 passed, 0 failed, 3 skipped`.

### Stop condition

Stop after administrative verification review.

## 11. Gate 6 — User verification submission

Catalog scope: `VER-01`. `VER-08` remains intentionally without a notification.

The temporary recipient policy is every user with the exact `Admin` role. `SuperAdministrator`, ordinary users, and the uploading user are excluded. This is an explicit temporary product policy until a dedicated verification assignment/queue model exists; the mapper must be replaced or narrowed when that model is approved.

### Minimal UserVerification-slice extensions

- `SubmitVerificationDocumentsHandler` enqueues `VerificationReviewRequested` version `1` before its existing `SaveChangesAsync` when at least one document is persisted;
- one submission request creates one event, including one event for a partial-success upload and one event for a multi-file request with its successful `documentCount`;
- failed-only validation/upload outcomes enqueue no event;
- `VerificationNotificationEventMapper` resolves Admin recipients through `IVerificationNotificationContextReader` and returns one draft per exact `Admin` role member;
- the event and notification carry only `userId` and bounded `documentCount`; no storage path, file URL/content/name, private metadata, rejection reason, contact detail, provider ID, token, or idempotency key is included;
- existing MediatR UserVerification handlers remain; no MediatR notification dispatch is introduced.

### Gate 6 Arabic copy and data contract

`verification.review-requested` uses `Information`, title `طلب مراجعة مستندات التحقق`, body `تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.`, `actionUrl: null`, and required data keys `userId` and `documentCount`. REST is the durable source of truth; SignalR broadcasts the persisted DTO best-effort. The shared outbox message ID makes replay idempotent, so each Admin receives at most one inbox row for the same committed submission event.

### Gate 6 verification

`VerificationNotificationEventMapperTests` and [`UserVerificationNotifications_Report.md`](../../../SmartCourt.Tests/HttpTests/UserVerificationNotifications_Report.md) cover every UserVerification endpoint, successful/partial/multi-file/replacement upload outcomes, failed-only no-event behavior, deletion, ownership, Admin-only recipient isolation, notification list/count/read/read-all, exact Arabic snapshots, forbidden metadata, mock Email confirmation, API/outbox/provider monitoring, API shutdown, and port release. The final monitored report records `147 passed, 0 failed, 1 documented skip`; the only skip is the optional SuperAdministrator fixture unavailable from the repository's supported setup.

### Stop condition

Stop after the Gate 6 HTTP report and review. Do not start Gate 7 automatically.

## 12. Gate 7 — Authentication/security

Catalog scope: `AUT-01` and `AUT-02`. New-device detection `AUT-03` and Email-only account deletion `AUT-05` remain deferred.

### Notification families

- `security.password-changed` → account owner;
- `security.password-reset` → account owner.

These are persisted in-app audit records with Arabic safe copy. They do not replace immediate Email security receipts, which belong to the later Email scope.

### Minimal Auth-slice extensions

- enqueue security events inside the same password change/reset transaction;
- do not include Email, token, IP, device fingerprint, or security stamp;
- retain refresh-token revocation and Identity behavior unchanged;
- no notifications for ordinary login/refresh/challenge operations.

### HTTP artifact

- `AuthSecurityNotifications_Test.ps1`
- `AuthSecurityNotifications_Report.md`

The script follows the skill's exhaustive Auth requirement. Because Auth is large, this is expected to be the most expensive HTTP gate; it covers every Auth endpoint, anonymous/authenticated behavior, challenge extraction, reset/change flows, token revocation, validation/hostile input, and security notification persistence.

### Stop condition

Stop after Auth report review.

## 13. Gate 8 — Optional/deferred slices

These are planned but not implementation-ready.

### Chat

Do not create per-message Notifications rows. First add a separately approved read-state, unread-count, mute preference, and aggregation design. Only then implement `chat.unread-activity` and run exhaustive Chat HTTP/SignalR tests.

### Case, CaseAnalysis, CaseReview, Matching

Current operations are synchronous/self-service. No notification integration. If an operation becomes a background job, add persisted requester ownership and completed/failed events, then plan that slice separately.

### DocumentReview

Current endpoints are anonymous and synchronous. No trustworthy recipient; no integration.

### LawIngestion

Defer until authorization is restored and `InitiatedByUserId` is persisted. After that, completed/failed events can notify the initiating admin and be verified with a dedicated HTTP report.

### Users, Files, Health, Notifications

No new integration for current actions. Profile review outcomes reuse Verification; file access uses audit/security monitoring; Health is operational; notification read/feed operations never self-notify.

## 14. Documentation changes per gate

Each approved slice updates, in the same gate:

- the opportunity catalog readiness from proposed to implemented where applicable;
- the backend integration guide's implemented mapper examples;
- the frontend/API contract's current type and `data` key table, without changing frontend code;
- this plan's gate status and verification totals;
- the HTTP Markdown report.

Arabic examples in documentation must exactly match mapper tests.

## 15. Test and report acceptance criteria

A slice passes only when:

- every new event commits atomically with the business change;
- event replay creates no duplicate inbox row;
- every expected recipient receives exactly one notification of that type;
- excluded actors/users receive none;
- Arabic title/body, severity, type, and metadata match the approved table;
- REST filtering/count/read/read-all remain correct;
- SignalR broadcasts the persisted DTO and duplicate broadcast is safe;
- target slice and Notifications automated tests pass;
- the generated HTTP script exits `0` with every assertion passing;
- the Markdown report contains no token, password, secret, full evidence/reason, Email, phone, or payment destination;
- no frontend file is modified;
- no remote push occurs.

## 16. Approval requested

Gates 0, 1, 2, 3, 5, and 6 have been executed under separate local gate branches. Gate 4 remains intentionally untouched. Gate 5 was re-based on current local `main` and reverified; Gate 6 uses the temporary exact-`Admin` recipient policy approved for this gate and is stopped for explicit review. Gate 7 has not started.

# Comprehensive Slice Review, Performance & Rate-Limiting Audit

**Date:** 2026-08-02  
**Audit type:** Static source review  
**Governing rules:** `.agents/AGENTS.md`  
**Scope:** `SmartCourt/Features/Contracts/`, `Milestones/`, `Payments/`, `Disputes/`, and the payment, reconciliation, job, and event provider contracts/implementations under `SmartCourt/Infrastructure/Providers/` used by those slices.

No other feature slice was reviewed. References to Files, Users, Notifications, Chat, Cases, or Idempotency are limited to an in-scope class's dependency boundary; their implementations were not inspected. Database configuration/migrations are also outside the strict directory boundary, so index findings below distinguish indexes evidenced by in-scope code from indexes that must be verified against the actual schema.

## 1. Executive Summary & Audit Matrix

The four slices contain strong foundations: authorization is consistently declared, controller responses are wrapped in `ApiResponse<T>`, request validators use FluentValidation, no MediatR/AutoMapper/Data Annotation or synchronous blocking pattern was found, read-oriented query services usually use `AsNoTracking()`, financial commands use idempotency records, webhook signatures use a constant-time HMAC comparison and a five-minute replay window, and outbox events are generally written in the same database save as state changes.

The audit nevertheless found several release-blocking gaps:

1. **Every one of the 35 scoped endpoints lacks an endpoint-level `[EnableRateLimiting]` attribute.** This includes funding, retry, withdrawal, administrative wallet adjustment, dispute resolution, and the anonymous payment webhook.
2. **`MilestoneChangesRequested` has two incompatible payload contracts.** `MilestoneService.RequestChangesAsync` emits `ContractPaymentAggregateEventPayload`, while `ContractIntegrationEventResolver` always deserializes that event type as `MilestoneChangeRequestEventPayload`. The outbox message will fail repeatedly and the associated notifications/conversation integration will not complete.
3. **Contract activation is not durably coupled to final milestone approval.** `MilestoneService.ApproveAsync` commits the milestone first and then synchronously calls `IContractService.EvaluateActivationAsync`. A failure between those operations leaves an eligible contract in `Draft`, with no outbox event or recovery job to re-evaluate it.
4. **A provider-confirmed failed normal escrow release has no retry path.** `EscrowReleaseService` returns permanently when the release transaction is `Failed`; the reconciliation batch only selects `Processing` transactions. The hold can remain funded and the milestone can remain `AcceptedHold` indefinitely.
5. **Critical reconciliation operations are optional at compile time.** `IPaymentReconciliationProvider` supplies default `null` implementations for release, refund, and withdrawal status checks. A provider can compile without implementing recovery for payouts/refunds/withdrawals, leaving unknown outcomes pending forever.
6. **Network calls occur while serializable database transactions are open** in escrow release, termination refund, and dispute resolution. This increases lock duration and deadlock risk and creates hard-to-recover partial external-success/database-failure scenarios.
7. **The dispute list has a severe N+1 query pattern.** A page of 100 disputes can trigger hundreds of extra database/eligibility calls through per-row `MapAsync` execution.
8. **Arabic domain errors are substantially better than expected:** no English-only literal was found in `BusinessException`, `ForbiddenAccessException`, `ConflictException`, `NotFoundException`, or FluentValidation messages. Remaining gaps are implicit English EF exceptions from `SingleAsync`, three internal English exception strings, dynamic replay text, one Arabic typo, and English status tokens exposed in DTOs.

### Audit matrix

| Dimension | Status | Summary |
|---|---|---|
| Rate limiting & abuse protection | **Critical / Fail** | 0 of 35 endpoints has an endpoint policy attribute; webhook also has no in-scope IP allowlist or body-size cap. |
| Performance & EF Core | **High risk** | Query services generally use no-tracking, but dispute listing is N+1; several reads over-fetch/unboundedly materialize; job reconciliation contains controlled N+1 loops. |
| Architecture rules | **Needs refactor** | Response, validation, mapping, and provider-pattern basics pass. Direct cross-slice `DbSet` access and entity-bearing service contracts violate the intended service boundary. |
| Integration integrity | **Critical / Fail** | Broken outbox payload contract, non-durable activation handoff, missing failed-release retry, optional reconciliation capabilities, and long network-in-transaction flows. |
| Arabic errors | **Mostly pass** | Domain/validation literals are Arabic. Implicit EF exceptions, dynamic replay text, internal English errors, and English response statuses remain. |
| Service complexity / SRP | **Critical debt** | `DisputeService` 1,588 lines; `PaymentEscrowService` 1,331; `MilestoneService` 1,054; `ContractService` 1,045, with confirmed duplicated/dead responsibilities. |

### Positive controls confirmed

- All scoped controller endpoints return `ApiResponse<T>` wrappers.
- All scoped controller classes/actions declare authorization; only the payment webhook is intentionally anonymous.
- FluentValidation validators live under each slice's `Validators/` directory; no Data Annotations were found in scoped DTOs/entities.
- No MediatR, AutoMapper, `.Result`, `.Wait()`, or `GetAwaiter().GetResult()` was found.
- Payment funding and withdrawal commands require an idempotency key; dispute resolution reserves a hold-settlement idempotency scope.
- Webhook verification checks event-ID/header agreement, timestamp freshness, signature format, and HMAC-SHA256 with `FixedTimeEquals` (`PaymentWebhookService.cs:201-280`).
- Read-only contract/payment query services consistently use `AsNoTracking()` and pass cancellation tokens.

## 2. Rate Limiting & Protection Audit Report

### Endpoint-level result

No `EnableRateLimiting` or `DisableRateLimiting` attribute exists anywhere in the strict audit scope. A global limiter may exist outside scope, but it does not satisfy the requested explicit endpoint classification and cannot give financial mutations different budgets from reads.

| Endpoint | Source | Risk | Missing recommended policy |
|---|---|---:|---|
| `POST /api/contracts` | `ContractsController.cs:20` | High | `contract-create` |
| `GET /api/contracts` | `ContractsController.cs:38` | Medium | `authenticated-query` |
| `GET /api/contracts/{contractId}` | `ContractsController.cs:55` | Medium | `authenticated-query` |
| `PUT /api/contracts/{contractId}` | `ContractsController.cs:71` | High | `contract-mutation` |
| `POST /api/contracts/{contractId}/accept` | `ContractsController.cs:94` | High | `contract-sign` |
| `POST /api/contracts/{contractId}/terminate` | `ContractsController.cs:115` | High | `contract-critical-mutation` |
| `GET /api/contracts/{contractId}/state-history` | `ContractsController.cs:138` | Medium | `authenticated-query` |
| `POST /api/contracts/{contractId}/milestones` | `MilestonesController.cs:23` | High | `contract-mutation` |
| `GET /api/contracts/{contractId}/milestones` | `MilestonesController.cs:43` | Medium | `authenticated-query` |
| `PUT /api/contracts/{contractId}/milestones/{milestoneId}` | `MilestonesController.cs:62` | High | `contract-mutation` |
| `POST /api/milestones/{milestoneId}/approve` | `MilestonesController.cs:88` | High | `contract-sign` |
| `POST /api/milestones/{milestoneId}/ready-for-funding` | `MilestonesController.cs:110` | High | `financial-state-mutation` |
| `POST /api/milestones/{milestoneId}/submit` | `MilestonesController.cs:132` | High | `evidence-submission` |
| `POST /api/milestones/{milestoneId}/accept` | `MilestonesController.cs:149` | High | `financial-state-mutation` |
| `POST /api/milestones/{milestoneId}/request-changes` | `MilestonesController.cs:164` | High | `contract-mutation` |
| `POST /api/milestones/{milestoneId}/change-requests` | `MilestonesController.cs:182` | High | `contract-mutation` |
| `POST /api/change-requests/{id}/approve` | `MilestonesController.cs:208` | High | `contract-sign` |
| `POST /api/change-requests/{id}/reject` | `MilestonesController.cs:230` | High | `contract-mutation` |
| `POST /api/change-requests/{id}/cancel` | `MilestonesController.cs:254` | High | `contract-mutation` |
| `POST /api/milestones/{milestoneId}/fund` | `PaymentsController.cs:27` | Critical | `funding` |
| `GET /api/contracts/{contractId}/payments` | `PaymentsController.cs:46` | High | `financial-query` |
| `GET /api/milestones/{milestoneId}/payment` | `PaymentsController.cs:65` | High | `financial-query` |
| `POST /api/payments/{transactionId}/retry` | `PaymentsController.cs:84` | Critical | `payment-retry` |
| `POST /api/payments/webhook` | `PaymentsController.cs:108` | Critical | `payment-webhook` |
| `GET /api/wallet` | `WalletsController.cs:20` | High | `financial-query` |
| `POST /api/wallet/withdrawals` | `WalletsController.cs:31` | Critical | `payout` |
| `POST /api/admin/wallets/{lawyerUserId}/adjustments` | `AdminWalletsController.cs:20` | Critical | `admin-financial-mutation` |
| `POST /api/disputes` | `DisputesController.cs:24` | Critical | `dispute-create` |
| `GET /api/disputes` | `DisputesController.cs:39` | Medium | `authenticated-query` |
| `GET /api/disputes/{disputeId}` | `DisputesController.cs:53` | Medium | `authenticated-query` |
| `POST /api/disputes/{disputeId}/evidence` | `DisputesController.cs:64` | Critical | `evidence-submission` |
| `POST /api/admin/disputes/{disputeId}/assign` | `DisputesController.cs:83` | High | `admin-case-mutation` |
| `POST /api/admin/disputes/{disputeId}/review` | `DisputesController.cs:99` | High | `admin-case-mutation` |
| `POST /api/admin/disputes/{disputeId}/resolve` | `DisputesController.cs:112` | Critical | `dispute-settlement` |
| `POST /api/admin/disputes/{disputeId}/close` | `DisputesController.cs:130` | High | `admin-case-mutation` |

### Recommended policy standards

Use `QueueLimit = 0` for financial/mutation policies so overload fails fast with `429`. Partition authenticated policies by a normalized composite key such as `userId + account/contract/hold resource + source IP`; partition webhook traffic by trusted provider identity/source IP and also retain event-ID uniqueness. Do not rely on IP alone for authenticated financial actions.

| Policy | Algorithm and budget | Partition key | Notes |
|---|---|---|---|
| `funding` | Token bucket: 5 tokens/minute, capacity 5, replenishment 1 every 12s | user + milestone + IP | Also cap 20/hour per funding account; idempotency is not a substitute for throttling. |
| `payment-retry` | Sliding window: 5/minute, 5 segments | admin user + original transaction + IP | A failed transaction should not be brute-force retried. |
| `payout` | Token bucket: 3/minute, capacity 3; 10/hour | lawyer user + wallet + IP | Add daily amount/risk controls separately. |
| `admin-financial-mutation` | Fixed window: 3/minute; 20/hour | admin user + target wallet + IP | Require audit reason and step-up authentication where supported. |
| `dispute-settlement` | Fixed window: 5/minute | moderator + dispute/hold + IP | Preserve the idempotency-key requirement. |
| `payment-webhook` | Token bucket: 120/minute per trusted provider IP, burst 30; global emergency ceiling | provider/IP | Tune to provider retry/burst documentation. Apply at edge before reading the body. Return `429` with `Retry-After`. |
| `contract-create`, `dispute-create` | Sliding window: 5/minute; 30/hour | user + IP | Prevent spam and resource creation abuse. |
| `contract-sign` | Sliding window: 10/minute | user + contract/milestone + IP | Protect acceptance/signature state changes. |
| `evidence-submission` | Token bucket: 10/minute, capacity 10 | user + dispute/milestone + IP | Add byte quota, file-count maximum, and per-dispute daily quota. |
| General authenticated mutation | Sliding window: 20/minute | user + IP | Use for ordinary draft/change actions. |
| `financial-query` | Fixed window: 60/minute | user + IP | Financial history is more expensive and sensitive than ordinary reads. |
| `authenticated-query` | Fixed window: 100/minute | user + IP | Retain pagination limits. |

### Additional protection gaps

| Finding | Evidence | Recommendation |
|---|---|---|
| Webhook body is read without an in-scope size limit. | `PaymentsController.cs:123-128` calls `ReadToEndAsync`. | Add `[RequestSizeLimit]`/server limit (for example 64-256 KiB based on provider contract) and reject oversized content before allocation/signature work. |
| No webhook IP restriction is present in the controller/service/provider contracts. | `PaymentsController.cs:108-162`; `PaymentWebhookService.cs:201-280`. | Allowlist published provider CIDRs at the edge when the provider guarantees stable ranges. Treat it as defense in depth, never as a replacement for HMAC. |
| Webhook misconfiguration is disclosed as a client business error. | `PaymentWebhookService.cs:235-240`. | Log a secure operational error and return a generic Arabic `503`/server error; do not reveal that the secret is absent. |
| Evidence/submission validators have no file-count ceiling. | `CreateDisputeRequestValidator.cs:30-36`, `AddDisputeEvidenceRequestValidator.cs:14-24`, `SubmitMilestoneRequestValidator.cs:16-28`. | Enforce a small maximum (for example 10-20 IDs/request) plus aggregate per-case limits. |
| No provider-specific webhook verifier/IP policy abstraction exists. | `IPaymentProvider.cs` only defines financial operations. | Add `IPaymentWebhookVerifier` returning a verified normalized event/provider identity; keep raw secret/HMAC details out of feature services. |

## 3. Performance & EF Core Optimization Report

### Query and allocation findings

| Severity | File / line | Finding | Recommended fix |
|---|---|---|---|
| Critical | `DisputeService.cs:187-227`, `1090-1141` | **N+1:** each dispute in a page calls `MapAsync`, which separately queries evidence, resolution, hold, settlement transactions, and user eligibility. At page size 100 this can produce roughly 300-500 extra calls. | Create `DisputeQueryService`; project the page in one base query, bulk-load evidence/statuses for the page IDs in two or three queries, resolve access once, and assemble via dictionaries. |
| High | `DisputeService.cs:237-247`, `1173-1233` | Read-only `GetAsync` reaches tracked `GetForMutationAsync`; missing `AsNoTracking()` and a query/mutation split. | Add a no-tracking authorized query path returning a DTO projection. Keep tracked loaders private to mutation commands. |
| High | `MilestoneSchedulingReconciliationService.cs:42-77`, `87-116` | **N+1:** both reconciliation loops query the current submission once per milestone. | Fetch milestone/submission pairs with a join in one query. Persist a release job ID/dispatch marker so repeated sweeps do not schedule duplicates. |
| High | `ContractQueryService.cs:145-159` and duplicate `ContractService.cs:752-766` | In-memory `milestones.Single(...)` inside `holds.OrderBy(...)` is O(holds × milestones). | Build `OrderNumberByMilestoneId` once or project/join/order in SQL. Remove the duplicate mapper from `ContractService`. |
| High | `PaymentQueryService.cs:26-67` | Contract payment history materializes every hold, attempt, and ledger row with no pagination/date bound. | Add separate paged collections or cursor/date filtering; project holds directly to `PaymentDto`. |
| High | `ContractTerminationSettlementService.cs:58-63` | Termination loads all milestones and holds as tracked entities before filtering in memory. | Query only unsettled holds and required milestone facts; use projections and explicit bounded processing. |
| High | `PaymentsController.cs:123-128` | Unbounded raw webhook body allocation. | Enforce request size before `ReadToEndAsync`; consider pooled/streaming verification if provider payloads can be large. |
| Medium | `ContractQueryService.cs:124-159` | Full milestone/hold entities are loaded where DTO projections suffice. | Project only required DTO facts and derive totals/order server-side where practical. |
| Medium | `MilestoneDraftService.cs:78-116` | All contract milestones and holds are materialized without pagination. | If contracts may have many milestones, add a bounded maximum or pagination; project to read models. |
| Medium | `DisputeService.cs:849-855` | Every historical settlement attempt for a hold/operation is loaded to choose completed/processing/latest. This grows indefinitely. | Query completed and processing attempts directly; query latest/count separately or maintain an attempt number. |
| Medium | `ContractService.cs:446-491`, `DisputeService.cs:672-696`, `WalletService.cs:204-330` | Recovery batches are bounded but execute sequential multi-query workflows per row. | Keep per-aggregate serialization, but claim work in bulk, avoid repeated authorization/read queries, and instrument query/provider latency. |
| Medium | `PaymentEscrowService.cs:203`, `427` | `CancellationToken.None` preserves uncertain financial state after request cancellation but has no deadline. | Use an independent short persistence timeout linked to application shutdown, not an unbounded token. Document this intentional compensation path. |
| Medium | `PaymentQueryService.cs:51-67` | Ledger query uses a correlated `Any` against accounts. | Prefer an explicit join on `EscrowAccountId`/`ContractId`; verify execution plan and covering index. |
| Low | `ContractService.cs:254-285` | State-history query duplicates `ContractQueryService.GetStateHistoryAsync`. | Remove the duplicate method and keep one query implementation. |
| Low | `MilestoneService.cs:785-954` | A large block of change-request helpers/event code has no callers after extraction to `MilestoneChangeRequestService`. | Delete after characterization tests; this removes dead allocations/dependencies and reduces maintenance risk. |
| Low | `PaymentEscrowService.cs:703-873`, `953-962` | Webhook authentication/matching/terminal-recording code is duplicated but unused; active code lives in `PaymentWebhookService.cs:201-378`. | Delete the dead copy and extract a single webhook verifier/component. |

### `AsNoTracking()` assessment

- **Pass:** `ContractQueryService` and `PaymentQueryService` use no-tracking for read-only entity queries.
- **Pass:** `MilestoneDraftService.ListAsync`, funding verification, contract integration resolver, and file-access fact queries use no-tracking.
- **Finding:** `DisputeService.GetAsync` uses the tracked mutation loader as described above.
- Mutation, reconciliation, outbox claim, and scheduling queries that subsequently update entities correctly remain tracked.
- Scalar/anonymous projections do not require `AsNoTracking()` because EF does not track non-entity projections.

### Index and filtering audit

Indexes explicitly evidenced by in-scope exception handling are:

- unique contract proposal: `UX_Contracts_ProposalId` (`ContractService.cs:1008-1017`);
- unique milestone order per contract: `UX/IX_Milestones_ContractId_OrderNumber` (`MilestoneService.cs:1002-1012`, `MilestoneDraftService.cs:346-355`);
- unique milestone submission version: `UX_MilestoneSubmissions_MilestoneId_Version` (`MilestoneService.cs:990-1000`);
- filtered/unique pending change request per milestone: `UX/IX_MilestoneChangeRequests_*Pending` (`MilestoneService.cs:978-988`, `MilestoneChangeRequestService.cs:420-430`).

The following are **required schema candidates with no confirming evidence inside the permitted scope**. Validate them against migrations and production execution plans before creating duplicates.

| Query evidence | Required/likely index |
|---|---|
| Contract participant/status list ordered by update (`ContractQueryService.cs:37-67`) | Separate `(ClientUserId, Status, UpdatedAt DESC, Id)` and `(LawyerUserId, Status, UpdatedAt DESC, Id)` indexes; consider rewriting the participant `OR` as a union if plans scan. |
| Contract history (`ContractQueryService.cs:94-111`) | `(ContractId, CreatedAt DESC, Id)` including displayed history columns. |
| Contract milestones and sequence checks (`MilestoneDraftService.cs:86-103`, `PaymentEscrowService.cs:995-1018`) | `(ContractId, Status, OrderNumber, Id)`; existing `(ContractId, OrderNumber)` may partially cover. |
| One hold per milestone / contract/status scans (`PaymentQueryService.cs:26-31`, `PaymentEscrowService.cs:1023-1040`) | Unique `(MilestoneId)` plus `(ContractId, Status)` and `(Status, HoldExpiresAt)` where applicable. |
| Processing payment reconciliation (`PaymentReconciliationService.cs:69-76`) | `(Status, CreatedAt, Id)` including operation/business identifiers. |
| Contract payment history (`PaymentQueryService.cs:33-49`) | `(ContractId, CreatedAt DESC, Id)` including status/operation/amount/provider fields. |
| Settlement attempt lookup (`DisputeService.cs:849-855`, `EscrowReleaseService.cs:157-163`) | `(EscrowHoldId, OperationType, CreatedAt, Id)` including status, amount, idempotency key. |
| Webhook event dedupe (`PaymentWebhookService.cs:53-57`, `369-377`) | Unique `(EventId)`; optionally include `PaymentTransactionId`. |
| Idempotent hold settlement (`DisputeService.cs:806-813`, `EscrowReleaseService.cs:135-141`) | Unique `(ResourceType, ResourceId)` for hold settlements or a composite including operation according to business exclusivity; add lookup index including status. |
| Dispute list/active check (`DisputeService.cs:93-96`, `193-219`) | Filtered uniqueness preventing more than one non-closed dispute per milestone; list indexes `(Status, AssignedModeratorUserId, CreatedAt DESC, Id)` and `(ContractId, Status)`. |
| Dispute evidence/resolution mapping (`DisputeService.cs:1095-1111`) | `(DisputeId, CreatedAt, Id)` on evidence and unique `(DisputeId)` on resolution. |
| Outbox claim/close check (`OutboxDispatcher.cs:138-153`, `DisputeService.cs:638-643`) | `(Status, AvailableAt, CreatedAt)` and `(AggregateType, AggregateId, EventType, Status)`. |
| Pending withdrawals (`WalletService.cs:208-213`) | `(Status, RequestedAt, Id)`. |

## 4. Arabic Exception & Error Message Audit Table

### Overall result

All literal messages found in scoped `BusinessException`, `ForbiddenAccessException`, `ConflictException`, `NotFoundException`, and FluentValidation `.WithMessage(...)` calls are Arabic (technical tokens such as `If-Match`, `ETag`, `base64`, and `EGP` are appropriately retained). There are no English-only hardcoded domain/validation literals to translate.

The remaining failure paths are below. `SingleAsync` rows are especially important because EF generates a generic runtime exception (commonly English) rather than the slice's descriptive Arabic domain error.

| File path | Line no. | Current text (English/generic) | Proposed Arabic message / action |
|---|---:|---|---|
| `Features/Disputes/DisputeService.cs` | 81 | Implicit `SingleAsync` failure for escrow hold | `حجز الضمان المرتبط بتمويل المرحلة غير موجود أو توجد حجوزات مكررة تحتاج إلى مراجعة.` Use `SingleOrDefaultAsync` plus a controlled exception. |
| `Features/Disputes/DisputeService.cs` | 444, 447, 450, 453 | Implicit `SingleAsync` failures for milestone/contract/hold/account during resolution | Emit entity-specific Arabic consistency errors; log correlation/dispute IDs without exposing internals. |
| `Features/Disputes/DisputeService.cs` | 725, 728, 731 | Implicit `SingleAsync` failures during settlement recovery | `تعذر استرداد تسوية النزاع لأن بيانات المرحلة أو العقد أو حجز الضمان غير مكتملة.` |
| `Features/Disputes/DisputeService.cs` | 1180 | Implicit contract `SingleAsync` failure on a read request | `العقد المرتبط بالنزاع غير موجود.` |
| `Features/Milestones/MilestoneService.cs` | 339 | Implicit hold `SingleAsync` after funding verification | `حجز الضمان الموثق للمرحلة غير موجود.` |
| `Features/Payments/WalletService.cs` | 313, 406, 435, 455, 489, 492, 517 | Implicit `SingleAsync` failures in withdrawal reservation/recovery | Replace with explicit wallet/withdrawal/idempotency consistency exceptions in Arabic; treat invariant breaks as operational errors, not a generic `500`. |
| `Features/Payments/AdminWalletAdjustmentService.cs` | 129, 132 | Implicit wallet/account `SingleAsync` failures | `تعذر تنفيذ التصحيح المالي لأن المحفظة أو حساب الضمان المرتبط غير موجود.` |
| `Features/Milestones/Events/MilestoneSchedulingOutboxHandler.cs` | 141 | `Outbox payload for {eventType} is invalid.` | `بيانات حدث صندوق الصادر من النوع {eventType} غير صالحة.` Also fix the payload collision described in section 5. |
| `Infrastructure/Providers/Events/OutboxDispatcher.cs` | 85 | `No outbox handler is registered for {eventType}.` | `لا توجد معالجة مسجلة لحدث صندوق الصادر من النوع {eventType}.` Prefer stable error codes for operations/logs. |
| `Features/Payments/ContractJobService.cs` | 123 | `Exactly one contract job operations service must be registered.` | `يجب تسجيل تنفيذ واحد فقط لعمليات مهام العقود.` This is internal configuration, not a client domain error. |
| `Infrastructure/Providers/Events/OutboxDispatcher.cs` | 35 | Generic `ArgumentOutOfRangeException(nameof(batchSize))` | `يجب أن يكون حجم دفعة صندوق الصادر أكبر من صفر.` Keep as argument exception if internal-only. |
| `Features/Milestones/MilestoneDraftService.cs` | 177 | `معرّف المرحلة مطلوب لترفيذ هذه العملية.` (typo) | `معرّف المرحلة مطلوب لتنفيذ هذه العملية.` |
| `Features/Payments/PaymentEscrowService.cs` | 1053-1055 | Replays `failure?.Message`; type is dynamic persisted text | Persist a stable Arabic public message/error code separately from private diagnostics; never replay raw exception/provider text. |
| `Features/Payments/WalletService.cs` | 539-541 | Replays `failure?.Message`; type is dynamic persisted text | Same public-code/private-diagnostics split. |
| `Features/Payments/PaymentWebhookService.cs` | 235-240 | Descriptive Arabic but reveals missing webhook secret | Return a generic Arabic service-unavailable error and log the configuration detail privately. |

### Related user-facing localization drift

These are not exception messages, but they are returned in public DTOs and should use enums/codes plus localized display text if the API promises Arabic:

- `"Duplicate"` in `PaymentWebhookService.cs:67-70`, `177-180`, `359-362`;
- `Completed/Failed/Processing` in `DisputeService.cs:1126-1137`;
- multiple `Status.ToString()` calls in payment, milestone, contract, and dispute action results;
- permitted-action strings such as `Update`, `Accept`, `Terminate`, `Approve`, and `ReadyForFunding` in contract/milestone DTO mapping.

## 5. End-to-End Flow & Integration Gaps Report

### Flow 1: contract signatures -> activation -> milestone linkage -> escrow setup

**Observed flow**

1. Contract creation validates proposal facts through `IContractCreationDependencyGate`, writes draft/history/outbox atomically, and protects proposal uniqueness (`ContractService.cs:61-121`).
2. Each party accepts a draft; the second acceptance calls `TryActivateAsync` in the same transaction (`ContractService.cs:169-226`). Activation requires at least one mutually approved, priced milestone (`ContractService.cs:629-676`).
3. If milestone approval happens after both contract acceptances, `MilestoneService.ApproveAsync` first saves the milestone, then invokes `contractService.EvaluateActivationAsync` (`MilestoneService.cs:74-105`).
4. Escrow account/hold creation is lazy: it occurs only after a successful funding result (`PaymentEscrowService.cs:500-541`), not when the contract becomes active.

**Gaps**

| Severity | Gap | Evidence / impact | Remediation |
|---|---|---|---|
| Critical | Non-durable activation handoff after milestone approval | `MilestoneService.cs:97-105` commits milestone approval before the separate activation call. Failure leaves a `Draft` contract with approved milestones and no recovery event. | In the milestone transaction, enqueue `MilestoneApproved`/`ContractActivationRequested`; handle idempotently in Contracts. Add a reconciliation job for `Draft` + both contract acceptances + approved milestone. |
| High | Escrow setup timing is ambiguous | Escrow account is created only at funding completion (`PaymentEscrowService.cs:504-515`). | Confirm whether “escrow setup” means an account at activation or a hold at funding. If account-at-activation is required, add an idempotent Payments service triggered by `ContractActivated`. |
| High | Direct cross-slice data access bypasses service interfaces | Contracts directly queries milestones, disputes, transactions, and holds (`ContractService.cs:311-345`, `508-527`, `574-650`). | Introduce narrow facts/evaluation interfaces owned by the providing slices; avoid cross-slice entity/`DbSet` coupling. |

### Flow 2: milestone work -> approval -> release -> change requests -> disputes

**Observed flow**

- Funding is verified before submission; file ownership is authorized; submission, state history, and outbox event are saved atomically (`MilestoneService.cs:222-335`).
- Manual/automatic acceptance moves the milestone to `AcceptedHold`, sets a 14-day hold, and emits an outbox event that schedules release (`MilestoneService.cs:381-435`; `MilestoneAutoAcceptanceService.cs:186-243`; `MilestoneSchedulingOutboxHandler.cs:94-118`).
- Client-requested work revisions return a submitted milestone to `FundedInProgress` (`MilestoneService.cs:448-521`). Contractual scope/duration change requests use a separate service.
- A dispute may be opened only during a valid accepted hold; the dispute, frozen hold, disputed milestone, suspended contract, history, and outbox event are committed in one serializable transaction (`DisputeService.cs:49-184`).

**Gaps**

| Severity | Gap | Evidence / impact | Remediation |
|---|---|---|---|
| Critical | Event-type/payload collision | `MilestoneService.cs:507-516` emits `MilestoneChangesRequested` with `{EntityId}`. `MilestoneChangeRequestService.cs:406-416` emits the same type with `{MilestoneId, ChangeRequestId, Status}`. Resolver always expects the latter (`ContractIntegrationEventResolver.cs:53-57`, `132-141`). | Split event types, e.g. `MilestoneSubmissionChangesRequested` and `MilestoneScopeChangeRequested`, each with a versioned payload. Add serializer contract tests for every event type/handler pair. |
| Critical | Failed normal release is terminal without retry | `EscrowReleaseService.cs:207-212` returns if the release transaction failed; reconciliation only scans `Processing` (`PaymentReconciliationService.cs:69-76`). | Add an explicit release-retry state machine with capped attempts/backoff/manual escalation, reusing deterministic provider idempotency keys as required by the provider. |
| High | Release scheduling is not persistently deduplicated | `MilestoneSchedulingOutboxHandler.cs:114-117` and reconciliation `:87-116` schedule release jobs but store no release job ID. Expired holds are rescheduled every sweep until state changes. | Store `ReleaseJobId`/schedule version or make scheduler calls idempotent by hold ID and deadline. |
| High | Notification subscription gaps | `ContractNotificationOutboxHandler.EventTypes` omits `ContractCompleted` and change-request approved/rejected/cancelled events (`:23-43`), although the conversation handler subscribes (`ContractConversationIntegrationOutboxHandler.cs:13-37`). | Confirm intent; add notification types/subscriptions or document that only conversation messages are expected. |
| Medium | No maximum file count for submissions/evidence | Validator evidence in section 2. | Add bounded counts and aggregate storage quotas. |

### Flow 3: payment intent -> gateway -> webhook -> escrow -> reconciliation -> wallet

**Observed flow**

1. Funding reserves an application idempotency key, persists `FundingProcessing` plus a `PaymentTransaction`, then calls the gateway with a derived provider key (`PaymentEscrowService.cs:50-196`).
2. Success creates the escrow account/hold, updates pending wallet balance, writes ledger/history/outbox, and completes the idempotency reservation (`PaymentEscrowService.cs:478-620`). Unknown outcomes remain processing for reconciliation.
3. The webhook validates a five-minute HMAC signature window, event ID, transaction facts, amount/currency, and provider transaction ID (`PaymentWebhookService.cs:28-199`, `201-331`).
4. Recurring reconciliation scans processing transactions in batches of 100 and delegates deposits to `IPaymentEscrowService` (`PaymentReconciliationService.cs:65-166`).
5. Hold release moves pending wallet funds to available balance and evaluates contract completion (`EscrowReleaseService.cs:314-442`). Withdrawal uses a separate idempotent wallet flow.

**Gaps**

| Severity | Gap | Evidence / impact | Remediation |
|---|---|---|---|
| Critical | Reconciliation capability can silently be absent | `IPaymentReconciliationProvider.cs:9-22` defaults release/refund/withdrawal checks to `null`. Unknown payout/refund/withdrawal results may never settle. | Remove default implementations. Require all critical operations or expose explicit provider capability flags that fail startup when a configured workflow needs an unsupported operation. |
| Critical | Provider calls inside serializable DB transactions | Release opens transaction at `EscrowReleaseService.cs:45-49` and calls provider at `:215-229`; termination opens at `ContractTerminationSettlementService.cs:48-52` and calls provider at `:267-281`; dispute resolution opens at `DisputeService.cs:429-431` and calls provider at `:492-518`. | Use a three-phase state machine: short DB transaction reserves attempt/outbox; call provider outside DB transaction with durable idempotency key and timeout; short DB transaction applies result. Reconcile any uncertain outcome. |
| High | Webhook supports deposit only | `PaymentWebhookService.cs:286-291` rejects non-deposit operations. | Confirm provider contract. If it emits release/refund/withdrawal events, add normalized handlers; otherwise document polling-only recovery and alert on prolonged processing. |
| High | No explicit per-call timeout/circuit-breaker contract | Provider methods receive only the request cancellation token (`IPaymentProvider.cs:3-23`). | Apply provider-owned timeout/retry/circuit-breaker policies. Never automatically retry an unknown financial write unless the provider guarantees idempotency. |
| High | Completion save failure is not immediately scheduled | `PaymentEscrowService.cs:600-609` reports provider success/DB failure but does not schedule reconciliation itself. It relies on periodic scanning. | Persist/retain processing state with an independent bounded token and schedule reconciliation after the failed apply; alert if scheduling fails. |
| Medium | Duplicate webhook race can still produce transient errors | Dedupe checks precede insert (`PaymentWebhookService.cs:53-71`); correctness depends on an out-of-scope unique index and exception paths. | Confirm unique `EventId`; add concurrency tests proving every duplicate terminal webhook returns a stable 200 result. |
| Medium | Public provider name derives from implementation type | `PaymentEscrowService.cs:129`, retry `:364`, and other settlement services use `paymentProvider.GetType().Name`. Decorators/proxies can change this value. | Add stable `ProviderCode` to the provider contract/configuration. |

### Flow 4: dispute -> freeze -> evidence -> resolution -> split/payout

**Observed flow**

- Opening atomically freezes the escrow hold, marks the milestone disputed, suspends the contract, and emits `DisputeOpened`.
- Evidence is authorized through the contract-scoped file interface; moderator assignment and review have explicit state guards.
- Resolution validates the split against the gross hold, creates provider refund/release attempts, persists a resolution, and either finalizes immediately or leaves a frozen hold for recovery (`DisputeService.cs:375-604`).
- Recovery locates `Resolved` + `Frozen` disputes, reconciles/retries provider operations, applies ledger/account/wallet state, resumes the contract, emits fund events, and completes idempotency (`DisputeService.cs:672-828`, `831-1087`).
- Closure requires settlement completion and a processed `DisputeResolved` outbox message (`DisputeService.cs:607-665`).

**Gaps**

| Severity | Gap | Evidence / impact | Remediation |
|---|---|---|---|
| Critical | External partial success can occur before the resolution transaction commits | Refund and release calls occur inside the serializable transaction before `SaveChanges`/commit (`DisputeService.cs:429-589`). A commit failure can leave real external money movement without the local resolution/attempt rows needed for straightforward recovery. | Persist resolution intent and provider attempts first, commit, call provider outside the transaction, then apply/finalize idempotently. |
| High | `Resolved` event is emitted before financial settlement completes | Dispute becomes `Resolved` and emits the event even when provider transactions remain pending (`DisputeService.cs:525-595`). Consumers can interpret “resolved” as money settled. | Split `DisputeDecisionIssued` from `DisputeSettlementCompleted`, or include explicit settlement status/version in the event. |
| High | Dispute slice directly owns payment-provider execution and payment entities | Constructor and imports at `DisputeService.cs:31-43`; provider calls at `:1352-1411`. | Move settlement execution/recovery behind an `IDisputeSettlementService` owned by Payments; Disputes supplies an immutable decision command and receives a settlement result. |
| Medium | Settlement status is inferred from any failed attempt | `DisputeService.cs:1126-1131` reports `Failed` if any historical attempt failed, even if another is processing and recovery may succeed. | Define precedence from aggregate state/current attempt: completed hold > current processing > terminal/manual-action-required. |

## 6. Open Questions & Ambiguity List

1. Is a global rate limiter configured outside scope? If yes, does it partition by authenticated user/account and differentiate financial writes, queries, and webhook traffic? Endpoint attributes are still needed for auditable classification.
2. Is FluentValidation automatic MVC integration enabled and are all validators registered? Several controllers rely on pipeline validation rather than explicit validator injection (for example contract/milestone/funding request bodies).
3. Does “escrow setup” require an account when the contract activates, or is lazy account/hold creation at successful funding the intended rule?
4. What is the intended semantic difference between client-requested submission revisions and contractual milestone scope change requests? They currently share `MilestoneChangesRequested`, causing a concrete payload mismatch.
5. Does the payment provider publish stable webhook IP ranges, and does it emit webhooks for release, refund, or withdrawal operations?
6. What maximum webhook payload size and provider burst rate are guaranteed? The current endpoint reads an unlimited body.
7. Are provider idempotency keys guaranteed for deposit, release, refund, and withdrawal, including after timeout and across multiple days? Recovery correctness depends on this.
8. What is the maximum allowed time in `Processing` before manual escalation? Current recurring reconciliation can return `ProviderOutcomeStillUnknown` indefinitely.
9. Should provider-confirmed failed hold release be retried automatically, manually, or converted into a dispute/operations queue? No current path advances it.
10. Are the scheduler methods idempotent by business key? `IContractJobScheduler` does not express a dedupe key and release job IDs are not persisted.
11. Should `DisputeResolved` mean “decision issued” or “money fully settled”? Current state/event means the former, while clients may infer the latter.
12. Can a contract have multiple simultaneous disputes on different milestones? Opening one changes the entire contract to `SuspendedByDispute`, while finalizing one returns it to `Active` without checking for another active dispute.
13. Are partial dispute allocations allowed to mark a milestone `Released` even when the client receives a partial refund? Current target milestone status is `Refunded` only for full refund (`DisputeService.cs:1005-1019`).
14. Should contract completion require every non-cancelled milestone to have been mutually approved? `EvaluateCompletionAsync` ignores unapproved draft milestones and completes when the approved subset is terminal (`ContractService.cs:311-356`).
15. Are status/action fields API codes or Arabic display text? The API currently returns English enum/action strings alongside Arabic errors.
16. Do schema migrations contain every index candidate in section 3, especially webhook event uniqueness, one hold per milestone, active-dispute uniqueness, outbox claim, processing transactions, and hold settlement uniqueness?
17. What is the expected behavior when notification/conversation/case handlers partially succeed? Message IDs are passed downstream, but per-handler processing state is not stored by the outbox dispatcher.
18. Are `PaymentProviderOptions` and provider-specific implementations intentionally outside `Infrastructure/Providers/Payments`? The feature imports `SmartCourt.Providers.Payments`, which weakens the otherwise clear provider boundary.

## 7. Service Decomposition & Refactoring Blueprint

### Current complexity

| Service | Lines | Primary reasons it violates SRP |
|---|---:|---|
| `DisputeService` | 1,588 | Commands, authorization, queries/mapping, state machine, provider execution, settlement calculation, ledger mutation, penalties, recovery, scheduling, completion, outbox. |
| `PaymentEscrowService` | 1,331 | Funding, retry, external execution, escrow/wallet/ledger mutation, idempotency replay, authorization, reconciliation hooks, plus dead duplicate webhook code. |
| `MilestoneService` | 1,054 | Negotiation, funding readiness, submission, review, eventing/mapping, plus dead change-request code already extracted elsewhere. |
| `ContractService` | 1,045 | Creation, draft editing, signing/activation, completion, termination orchestration/recovery, read mapping/history duplication, outbox. |
| `WalletService` | 653 | Query, reservation, provider payout, replay, financial mutation, and batch reconciliation. |
| `EscrowReleaseService` | 521 | Eligibility, provider execution, idempotency, ledger/wallet mutation, milestone/hold state, completion. |
| `ContractTerminationSettlementService` | 515 | Selection, provider refund execution/retry, financial mutation, milestone state, eventing. |

### Target slice structure

```text
Features/
  Contracts/
    Creation/IContractCreationService.cs
    Creation/ContractCreationService.cs
    Drafts/IContractDraftService.cs
    Drafts/ContractDraftService.cs
    Lifecycle/IContractLifecycleService.cs
    Lifecycle/ContractLifecycleService.cs
    Lifecycle/IContractActivationEvaluator.cs
    Lifecycle/ContractActivationEvaluator.cs
    Completion/IContractCompletionEvaluator.cs
    Completion/ContractCompletionEvaluator.cs
    Termination/IContractTerminationService.cs
    Termination/ContractTerminationService.cs
    Termination/IContractTerminationRecoveryService.cs
    Queries/IContractQueryService.cs
    Queries/ContractQueryService.cs
  Milestones/
    Negotiation/IMilestoneNegotiationService.cs
    Funding/IMilestoneFundingReadinessService.cs
    Submissions/IMilestoneSubmissionService.cs
    Review/IMilestoneReviewService.cs
    ChangeRequests/IMilestoneChangeRequestService.cs
    Queries/IMilestoneQueryService.cs
    Scheduling/IMilestoneScheduleRecoveryService.cs
  Payments/
    Funding/IFundingService.cs
    Funding/FundingService.cs
    Funding/IFundingFinalizer.cs
    Webhooks/IPaymentWebhookService.cs
    Webhooks/IPaymentWebhookVerifier.cs
    Reconciliation/IPaymentReconciliationService.cs
    Escrow/IEscrowAccountService.cs
    Escrow/IEscrowSettlementService.cs
    Escrow/IEscrowLedgerWriter.cs
    Withdrawals/IWithdrawalService.cs
    Withdrawals/IWithdrawalRecoveryService.cs
    Queries/IPaymentQueryService.cs
  Disputes/
    Opening/IDisputeOpeningService.cs
    Evidence/IDisputeEvidenceService.cs
    CaseManagement/IDisputeCaseManagementService.cs
    Resolution/IDisputeResolutionService.cs
    Queries/IDisputeQueryService.cs
  Payments/Integration/
    IDisputeSettlementService.cs
    IContractTerminationSettlementService.cs
```

### Concrete extraction rules

#### `PaymentEscrowService`

1. Delete confirmed unused webhook block (`:703-873`, `:953-962`) after tests.
2. Move `FundAsync` reservation/state-start logic into `FundingService`.
3. Move `CompleteFundingAsync`/failed result application into `FundingFinalizer`; accept immutable `FundingCompletionCommand`, not tracked `Milestone` and `PaymentTransaction` entities.
4. Move replay/failure serialization into `PaymentIdempotencyResultStore` with public Arabic error codes separated from private diagnostics.
5. Move account/hold/wallet/ledger changes into `EscrowFundingLedgerWriter` and keep one atomic DB application transaction.
6. Keep webhook parsing/authentication in `PaymentWebhookService` plus `IPaymentWebhookVerifier`; the verifier should return a normalized verified event.

#### `DisputeService`

1. Extract read/list/mapping to `DisputeQueryService` and eliminate the N+1 pattern.
2. Extract opening/freezing to `DisputeOpeningService`; it owns the local atomic state transition only.
3. Extract evidence to `DisputeEvidenceService` and enforce count/quota policy.
4. Extract assign/review/close to `DisputeCaseManagementService`.
5. Keep decision validation/persistence in `DisputeResolutionService`, but send an immutable `DisputeSettlementCommand` to `IDisputeSettlementService` in Payments.
6. Move provider attempts, ledger/wallet/hold finalization, and recovery entirely to Payments. Return `Pending/Completed/ManualActionRequired` with settlement ID.

#### `MilestoneService`

1. Remove unused change-request helpers (`:785-954`) because `MilestoneChangeRequestService` owns that behavior.
2. Extract approval/signing and durable activation request to `MilestoneNegotiationService`.
3. Extract ready-for-funding checks to `MilestoneFundingReadinessService`.
4. Extract submission/file/version behavior to `MilestoneSubmissionService`.
5. Extract manual review/request-changes to `MilestoneReviewService`; give submission revisions a distinct event type.
6. Move DTO assembly/permitted actions to a query service/projector.

#### `ContractService`

1. Keep create/update draft in separate command services.
2. Move acceptance/activation and activation recovery into `ContractLifecycleService`/`ContractActivationEvaluator`.
3. Keep completion evaluation behind the existing narrow interface, but replace direct payment/dispute `DbSet` reads with a providing-slice facts interface.
4. Move termination request/recovery to `ContractTerminationService`; Payments retains settlement execution.
5. Remove duplicate history/detail query code and delegate all reads to `IContractQueryService`.

### Cross-slice interface and DI plan

- Interfaces live in the **providing slice's** `Integration/` directory and expose commands/facts/results, never EF entities or `IQueryable`.
- Replace `IPaymentEscrowService.CompleteFundingAsync(Milestone, PaymentTransaction, ...)` (`IPaymentEscrowService.cs:22-30`) with a Payments-owned command containing IDs, expected versions, provider result, and correlation ID.
- Replace Contracts' direct milestone/dispute/payment queries with narrow interfaces such as `IContractMilestoneFacts`, `IContractSettlementFacts`, and `IContractDisputeFacts` owned by their slices.
- Replace Disputes' direct `IPaymentProvider` use with `IDisputeSettlementService` owned by Payments.
- Register each public service once as scoped; keep pure calculators/guards singleton or static. Avoid `IEnumerable<T>` “exactly one implementation” runtime checks where one concrete registration is required.
- Preserve outbox writes in the same local transaction as the state change. Consumers must be idempotent by outbox message ID/business version.
- Add architecture tests preventing references from one feature's service to another feature's `Entities` namespace and preventing feature references to concrete provider implementations.

## 8. Prioritized Action Plan

### [CRITICAL] Bugs, money-state gaps, and missing protection

1. Add and configure explicit endpoint rate-limit policies for all 35 endpoints; deploy financial/webhook policies first.
2. Split the conflicting `MilestoneChangesRequested` event into two versioned event contracts; add end-to-end outbox serialization/handler tests.
3. Make contract activation after milestone approval durable via an outbox request plus reconciliation job.
4. Implement failed escrow release retry/manual escalation so a provider-confirmed failure cannot strand funds indefinitely.
5. Remove default `null` reconciliation methods; fail startup when configured payment workflows lack deposit/release/refund/withdrawal reconciliation.
6. Refactor dispute resolution, escrow release, and termination refunds into reserve-call-apply phases so no provider network call occurs inside a serializable DB transaction.
7. Add webhook request-size limits, provider-aware throttling, and IP allowlisting where the provider supports stable ranges.
8. Confirm/enforce unique indexes for webhook event IDs, one hold per milestone, active dispute per milestone, provider/idempotency business keys, and hold settlement reservations.
9. Add alerts/dead-letter/manual-action states for financial transactions that remain processing beyond the business SLA.

### [HIGH] Performance, localization, and integration resilience

1. Build `DisputeQueryService` with bulk projection to eliminate list N+1 calls.
2. Fix scheduling reconciliation N+1 queries and persist/idempotently dedupe release scheduling.
3. Paginate/bound payment history and large milestone/contract settlement reads.
4. Replace public-path `SingleAsync` invariants with controlled Arabic errors and private diagnostics.
5. Store stable public Arabic error codes/messages separately from provider/exception diagnostics in idempotency responses.
6. Add provider timeouts/circuit breakers and operation-specific retry rules; never blind-retry unknown financial writes.
7. Split `DisputeDecisionIssued` from `DisputeSettlementCompleted` and align status semantics across API/events.
8. Confirm/fix contract completion semantics for unapproved draft milestones and multi-dispute contract resumption.
9. Add maximum attachment counts and per-contract/dispute aggregate quotas.
10. Validate index candidates with migrations and representative SQL execution plans.

### [REFACTOR] Service bloat and architecture boundaries

1. Remove dead duplicate webhook code from `PaymentEscrowService` and dead change-request code from `MilestoneService`.
2. Decompose `DisputeService`, `PaymentEscrowService`, `MilestoneService`, and `ContractService` using section 7's blueprint.
3. Split wallet query/withdrawal/recovery and escrow release provider execution/finalization.
4. Stop exposing EF entities through `IPaymentEscrowService`; use immutable commands/results.
5. Replace direct cross-feature `DbSet` access with providing-slice integration interfaces.
6. Consolidate duplicate contract detail/history mapping in `ContractQueryService`.
7. Introduce architecture tests and event-contract tests to prevent boundary and payload regressions.

## Verification Checklist for Remediation

- Every endpoint resolves to exactly one named policy and returns `429` with `Retry-After` under test.
- Webhook tests cover invalid signature, stale timestamp, oversized body, untrusted IP, concurrent duplicate events, and provider burst traffic.
- Every outbox event type can serialize, deserialize, and run every subscribed handler using its declared version.
- Fault-injection tests stop execution after each reserve/provider/apply/commit boundary and prove reconciliation reaches one correct financial result without double movement.
- Failed, unknown, and timed-out deposit/release/refund/withdrawal operations each have a bounded recovery/manual-escalation path.
- Contract activation and completion reconciliation repair intentionally introduced partial handoffs.
- Dispute list query count remains constant as page size grows.
- All public validation/domain failures are descriptive Arabic; internal diagnostics never leak provider or exception text.

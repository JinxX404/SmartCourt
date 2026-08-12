# Milestone API Contract and Frontend Integration Guide

**Code snapshot analyzed:** 2026-08-12
**Primary route prefixes:** `/api/contracts/{contractId}/milestones`, `/api/milestones/{milestoneId}/...`, `/api/change-requests/{changeRequestId}/...`
**Audience:** Web/mobile frontend developers integrating the Milestone lifecycle for a Lawyer/Client legal-services platform

> This guide describes the implementation in source code, including its current inconsistencies. It does not substitute intended product behavior for actual wire behavior.

## Wire-level conventions

| Concern | Actual behavior |
|---|---|
| Authentication | Every Milestone endpoint requiring a role is protected. Send `Authorization: Bearer <JWT>` or the application's `accessToken` HttpOnly cookie. The cookie wins if both are present. |
| Content type | Send `Content-Type: application/json` for endpoints with a body. Success and middleware-handled errors are JSON. |
| JSON naming | Response and request examples use `camelCase`. ASP.NET Core binding is case-insensitive, but frontend code should use the documented casing. |
| Enum encoding | **Enum-valued JSON fields are numbers**, because MVC has no `JsonStringEnumConverter`. Query/route binding may accept a name or its numeric value; numerics are safest. The one exception is `MilestoneActionResultDto.status`, deliberately returned as a string enum name such as `"Draft"`. |
| Dates | `DateTime` values serialize as ISO-8601 strings; stored timestamps are UTC and normally end in `Z`. Nullable dates are JSON `null` until the event occurs. |
| Money | `decimal` JSON numbers. Currency is fixed to `"EGP"`; do not submit currency or compute authoritative totals client-side. |
| Nulls | Null response properties are not suppressed. Envelopes include `message: null`, `errors: null`, and failed envelopes `data: null`. |
| Error codes | **No machine-readable application error-code field.** HTTP status plus localized `message`/`errors` is the only discriminator. Do not branch on Arabic prose when status/resource state suffices. |
| Rate limiting | All Milestone methods carry rate-limit metadata (`StandardMutation`, `SensitiveMutation`, `FinancialMutation`, `AuthenticatedQuery`) but `app.UseRateLimiter()` is commented out in `Program.cs`. The documented 429 policy is configured but inactive. |

### **Critical difference from the Contract slice: `412 Precondition Failed` is real here**

The Milestones slice **does** throw `PreconditionFailedException`, which the shared middleware maps to HTTP `412` (see `ExceptionHandlingMiddleware.cs`). This happens when:

1. The controller's manual `If-Match` validation fails (`ValidateIfMatchAsync` → missing, empty, weak `W/"..."`, wildcard `*`, malformed Base64).
2. A `DbUpdateConcurrencyException` occurs at save time in the Draft/ChangeRequest services (`SaveChangesAsync` rethrows as 412).

A **well-formed but stale** `If-Match` (valid Base64 ETag that does not match the current rowversion) instead throws `ConflictException` → HTTP **`409`**. So the three-way contract is: malformed header → `400`-free `412`, valid-but-stale token → `409`, DB write race → `412`. The Contract slice's guide reported `400` for malformed `If-Match`; do **not** port that assumption here.

### Standard success envelope

```json
{
  "success": true,
  "data": {},
  "message": null,
  "errors": null,
  "statusCode": 200
}
```

Creation endpoints (Add Milestone, Create Change Request) use `statusCode: 201`. `data` shape is endpoint-specific.

### Middleware-handled error envelope

```json
{
  "success": false,
  "data": null,
  "message": "Localized or generic error message",
  "errors": null,
  "statusCode": 400
}
```

Exceptions mapped by the middleware: `ValidationException`→400 (+`errors` array `"Field: ..."`), `AuthenticationException`→401, `BusinessException`→400, `NotFoundException`→404, `ConflictException`→409, `ForbiddenAccessException`→403, `PreconditionFailedException`→412, `TooManyRequestsException`→429, `PayloadTooLargeException`→413. Unhandled →500.

### Automatic binding/FluentValidation error shape

FluentValidation runs through `AddFluentValidationAutoValidation()` + `[ApiController]`, so request-body rule failures produce the framework validation-problem shape:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Title": ["عنوان المرحلة مطلوب."],
    "Amount": ["قيمة المرحلة يجب أن تكون أكبر من صفر بالجنيه المصري."]
  },
  "traceId": "00-..."
}
```

A class-level rule (the "at least one change" rule on `CreateMilestoneChangeRequest`) produces an empty property key (`""`). Malformed JSON, missing non-nullable body data, invalid enum text, and route/model binding errors may use framework problem details or an empty framework response rather than this shape.

### Global HTTP error behavior

| HTTP status | Source | Response behavior / meaning |
|---:|---|---|
| `400 Bad Request` | FluentValidation auto-validation or `BusinessException` | Validation problem details, otherwise custom failed envelope. Used for forbidden states (not Draft / not Active / wrong milestone status) and failed prerequisites. |
| `401 Unauthorized` | Authorization framework or `AuthenticationException` | Missing/invalid token mostly framework 401 (possibly empty body); service-side auth error uses the custom envelope. |
| `403 Forbidden` | Role policy or `ForbiddenAccessException` | Role mismatch normally framework 403; resource/actor denial uses the custom envelope. |
| `404 Not Found` | `NotFoundException` or route mismatch | Valid GUID but absent entity → custom envelope (`المرحلة غير موجودة.`, `طلب التعديل المطلوب غير موجود.`). Non-GUID path does not match `:guid` routes → framework 404. |
| `409 Conflict` | `ConflictException`, unique-index race, already-approved/decided/pending | Custom failed envelope. Distinct from Contract slice: stale-but-well-formed `If-Match` also yields 409 here. |
| `412 Precondition Failed` | `PreconditionFailedException` | **Implemented for Milestones.** Malformed/missing `If-Match` (controller validation) and DB write races → custom envelope with `message` from the validator or `تم تعديل المرحلة بواسطة عملية أخرى...`. |
| `429 Too Many Requests` | Configured limiter | Not enforced while `UseRateLimiter()` stays commented out. |
| `500 Internal Server Error` | Unhandled exception | Custom envelope `message: "An internal server error occurred."`; implementation details not exposed. |

---

## 1. Complete Endpoint Catalog

### Endpoint overview

All 12 routes below live on `MilestonesController` (`[Route("api")]`). `contractId`, `milestoneId`, and `changeRequestId` all use the ASP.NET `guid` route constraint.

| # | Method | Exact route | Controller roles | Success | Request body | If-Match required |
|---|---|---|---:|---|---|---|
| 1 | `POST` | `/api/contracts/{contractId}/milestones` | Client, Lawyer | `201` | `AddMilestoneRequest` | No |
| 2 | `GET` | `/api/contracts/{contractId}/milestones` | Client, Lawyer, Moderator, SuperAdministrator | `200` | None | No |
| 3 | `PUT` | `/api/contracts/{contractId}/milestones/{milestoneId}` | Client, Lawyer | `200` | `UpdateMilestoneRequest` | **Yes** (milestone) |
| 4 | `POST` | `/api/milestones/{milestoneId}/approve` | Client, Lawyer | `200` | None | **Yes** (milestone) |
| 5 | `POST` | `/api/milestones/{milestoneId}/ready-for-funding` | Lawyer | `200` | None | **Yes** (milestone) |
| 6 | `POST` | `/api/milestones/{milestoneId}/submit` | Lawyer | `200` | `SubmitMilestoneRequest` | No (unique-index guarded) |
| 7 | `POST` | `/api/milestones/{milestoneId}/accept` | Client | `200` | None | No |
| 8 | `POST` | `/api/milestones/{milestoneId}/request-changes` | Client | `200` | `RequestMilestoneChangesRequest` | No |
| 9 | `POST` | `/api/milestones/{milestoneId}/change-requests` | Client, Lawyer | `201` | `CreateMilestoneChangeRequest` | **Yes** (milestone) |
| 10 | `POST` | `/api/change-requests/{changeRequestId}/approve` | Client, Lawyer | `200` | None | **Yes** (change request) |
| 11 | `POST` | `/api/change-requests/{changeRequestId}/reject` | Client, Lawyer | `200` | `RejectChangeRequest` | **Yes** (change request) |
| 12 | `POST` | `/api/change-requests/{changeRequestId}/cancel` | Client, Lawyer | `200` | None | **Yes** (change request) |

**Adjacent endpoints outside this slice** (referenced by the lifecycle but documented in their own slices): `POST /api/milestones/{milestoneId}/fund` (Payments, Client-only, consumes `Idempotency-Key`), `GET /api/contracts/{contractId}/payments`, `GET /api/milestones/{milestoneId}/payment`, `GET /api/contracts/{contractId}` (detail is the primary source of a milestone's current `version`), and the Disputes slice which drives `MilestoneStatus.Disputed` / `Refunded`.

---

### 1.1 Add a Milestone (Draft)

**HTTP Method & Exact Route:** `POST /api/contracts/{contractId}/milestones`

**Purpose:** A party to a **Draft** Contract appends the next milestone with negotiated terms. Milestones are enforced as a strict sequential 1..N list (an `orderNumber` cannot be skipped or reused).

**Request Structure (What they send)**

| Location | Name | Required | Type | Details |
|---|---|---|---:|---|---|
| Header/cookie | Authentication | Yes | JWT | `Client` or `Lawyer` role, and must be a party to the contract. |
| Header | `Content-Type` | Yes | string | `application/json` |
| Route | `contractId` | Yes | UUID | Target Draft Contract. |
| Body | `title` | Yes | string | 3–200 characters. |
| Body | `description` | No | string | Nullable; max 10,000; whitespace-only invalid. |
| Body | `orderNumber` | Yes | int | Must equal `maxExistingOrder + 1` (1 for the first milestone). |
| Body | `amount` | Yes | decimal | `> 0`, at most 2 decimal places; currency is fixed `EGP`. |
| Body | `durationDays` | No | int | 1–365 when provided. |
| Body | `dueDate` | No | ISO-8601 | Must be in the future (UTC). |

```json
{
  "title": "مرحلة الإيداع: تجهيز وتقديم صحيفة الدعوى",
  "description": "تحضير المذكرة وتقديمها للمحكمة.",
  "orderNumber": 1,
  "amount": 5000.00,
  "durationDays": 14,
  "dueDate": "2026-09-01T00:00:00Z"
}
```

**Business preconditions**

- Authenticated user is the Contract Client or Lawyer (`EnsureParticipant`).
- Contract **must be `Draft`** (status `0`). The Draft service rejects Active with `يمكن تعديل المراحل أو التفاوض عليها أثناء مرحلة المسودة فقط.` (409).
- `orderNumber` must be the next sequential integer; otherwise `ترتيب المرحلة الجديدة يجب أن يكون N.` (400).
- A unique index `UX_Milestones_ContractId_OrderNumber` protects against simultaneous same-order inserts. In a real race the intended 409 `يوجد بالفعل مرحلة أخرى بنفس الترتيب داخل العقد.` is **not** reliably produced because the service matches a different index-name string (see 5.3) — the deterministic sequential gate normally returns 400 first.
- A new milestone is `Draft` (status `0`), `Unfunded`, and emits a `milestone.created` notification to the counterparty. No `If-Match` is consumed.

**Response Structure (What they get): `201 Created`**

`ApiResponse<MilestoneDto>` — full field dictionary in section 2. Initial Draft example:

```json
{
  "success": true,
  "data": {
    "id": "22222222-2222-2222-2222-222222222222",
    "orderNumber": 1,
    "title": "مرحلة الإيداع: تجهيز وتقديم صحيفة الدعوى",
    "description": "تحضير المذكرة وتقديمها للمحكمة.",
    "amount": 5000.00,
    "durationDays": 14,
    "dueDate": "2026-09-01T00:00:00Z",
    "status": 0,
    "fundingStatus": 0,
    "escrowHoldId": null,
    "fundedAt": null,
    "submittedAt": null,
    "autoAcceptEligibleAt": null,
    "holdExpiresAt": null,
    "netLawyerAmount": null,
    "version": "\"AAAAAAAAB9E=\"",
    "permittedActions": ["Update", "Approve"]
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```

**Endpoint-specific failures**

| Status | Condition | Message / response |
|---:|---|---|
| `400` | Body validation (title/length/amount/order/duration/due-date) | Automatic validation problem. |
| `400` | Wrong next `orderNumber` | `ترتيب المرحلة الجديدة يجب أن يكون N.` |
| `404` | `contractId` valid GUID but Contract absent | Contract detail service `404`. |
| `409` | Contract not Draft (e.g. Active/Terminated) | `يمكن تعديل المراحل أو التفاوض عليها أثناء مرحلة المسودة فقط.` |
| `409` | Concurrent same-order insert (intended) | `يوجد بالفعل مرحلة أخرى بنفس الترتيب داخل العقد.` — only on the race path, and currently unreliable due to the index-name mismatch (5.3); the non-race duplicate hits the `400` sequential gate. |
| `403` | Not a contract party | `هذا الإجراء متاح لطرفي العقد فقط.` |
| `401/403` | Missing token / wrong role | Framework authorization response. |

--- 

### 1.2 List the Contract's Milestones

**HTTP Method & Exact Route:** `GET /api/contracts/{contractId}/milestones`

**Purpose:** Returns **all** milestones of the contract as a flat array ordered by `orderNumber` ascending. No pagination, filtering, or sorting in this slice.

**Request Structure (What they send)**

| Location | Name | Required | Type | Details |
|---|---|---|---:|---|---|
| Header/cookie | Authentication | Yes | JWT | Client/Lawyer participant, or Moderator/SuperAdministrator under the Contract detail access policy. |
| Route | `contractId` | Yes | UUID | Target Contract. |

No query parameters or body.

**Response Structure (What they get): `200 OK`**

`ApiResponse<IReadOnlyList<MilestoneDto>>` — `data` is a JSON **array** (not a paged object):

```json
{
  "success": true,
  "data": [
    { "id": "22222222-2222-2222-2222-222222222222", "orderNumber": 1, "status": 3, "fundingStatus": 2, "version": "\"AAAAAAAAB9E=\"", "permittedActions": ["Submit"], "...": "..." },
    { "id": "33333333-3333-3333-3333-333333333333", "orderNumber": 2, "status": 0, "fundingStatus": 0, "version": "\"AAAAAAAACF0=\"", "permittedActions": ["Update","Approve"], "...": "..." }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```

**Endpoint-specific failures**

| Status | Condition | Response |
|---:|---|---|
| `403` | Role allowed but user is neither participant nor eligible moderator/admin | Contract detail access denial envelope. |
| `404` | Contract absent / non-GUID | Custom or framework 404. |

> The "current sequential milestone" flag derives from the first non-terminal (`Released`/`Refunded`/`Cancelled`) milestone by `orderNumber`. Only that milestone can surface `ReadyForFunding`.

---

### 1.3 Update a Draft Milestone

**HTTP Method & Exact Route:** `PUT /api/contracts/{contractId}/milestones/{milestoneId}`

**Purpose:** Replaces the editable fields of a Draft milestone. Any edit **clears both parties' milestone acceptances**, forcing re-approval.

**Request Structure (What they send)**

| Location | Name | Required | Type | Details |
|---|---|---|---:|---|---|
| Header/cookie | Authentication | Yes | JWT | `Client` or `Lawyer` and a Contract party. |
| Header | `If-Match` | **Yes** | string | Milestone ETag copied verbatim from `MilestoneDto.version`. |
| Header | `Content-Type` | Yes | string | `application/json` |
| Route | `contractId` | Yes | UUID | Contract containing the milestone. |
| Route | `milestoneId` | Yes | UUID | Target milestone. |
| Body | `title` | Yes | string | Complete replacement; 3–200. |
| Body | `description` | No | string | Nullable; max 10,000. |
| Body | `durationDays` | No | int | 1–365 when provided. |
| Body | `dueDate` | No | ISO-8601 | Future when provided. |

`amount` and `orderNumber` are **not** editable here.

```json
{
  "title": "مرحلة الإيداع (محدثة)",
  "description": "تم توسيع نطاق الإعداد ليشمل كشف المستندات.",
  "durationDays": 21,
  "dueDate": "2026-09-10T00:00:00Z"
}
```

**Business preconditions**

- Contract must be `Draft` and milestone must be `Draft`.
- Milestone must belong to the routed contract (`المرحلة لا تنتمي إلى العقد المحدد.`).
- Stale `If-Match` → 409; malformed → 412; DB race → 412.
- Emits `milestone.draft-updated` to the counterparty and resets `AcceptedByClientAt`/`AcceptedByLawyerAt`.

**Response Structure (What they get): `200 OK`** — `ApiResponse<MilestoneDto>` with a new `data.version` and `permittedActions`.

**Endpoint-specific failures**

| Status | Condition | Message / response |
|---:|---|---|
| `400` | Body validation | Automatic validation problem. |
| `400` | Milestone not Draft | `يمكن تنفيذ هذا الإجراء على مراحل المسودة فقط.` |
| `403` | Not a party | `هذا الإجراء متاح لطرفي العقد فقط.` |
| `404` | Milestone absent / belongs to another contract | `المرحلة المطلوبة غير موجودة.` / relation check. |
| `409` | Contract not Draft **or** stale milestone version | Draft gate / `تم تعديل المرحلة بواسطة عملية أخرى. يرجى إعادة تحميلها والمحاولة مرة أخرى.` |
| `412` | Malformed `If-Match` or save race | Manual validator message / same concurrency prose. |

---

### 1.4 Approve Milestone Terms

**HTTP Method & Exact Route:** `POST /api/milestones/{milestoneId}/approve`

**Purpose:** Records acceptance of the **current Draft milestone version** by the calling party. Both Client and Lawyer must approve independently; only the second approval transitions the milestone `Draft → AwaitingFunding`.

**Request Structure (What they send)**

| Location | Name | Required | Type | Details |
|---|---|---|---:|---|---|
| Header/cookie | Authentication | Yes | JWT | `Client` or `Lawyer`, and must be a Contract party. |
| Header | `If-Match` | **Yes** | string | Milestone ETag. |
| Route | `milestoneId` | Yes | UUID | Target milestone. |
| Body | — | No | — | Send none. `{}` is harmless. |

**Business preconditions**

- Contract must be `Draft` **or** `Active` (service gate allows both; Active is possible because a funded/active contract may host a still-Draft bonus milestone). Non-negotiable states → `لا يمكن التفاوض على مراحل عقد غير نشط أو غير موجود كمسودة.` (400).
- Milestone must be `Draft` (`لا يمكن تعديل أو اعتماد شروط مرحلة خرجت من حالة المسودة.` → 400).
- A party can approve once per version; repeat → `وافق العميل/المحامي على النسخة الحالية من المرحلة مسبقًا.` (409).
- **No required order** — either party may approve first.
- First approval emits `milestone.acceptance-recorded` to the counterparty; the second approval emits `milestone.approved` to both parties **and** enqueues an internal `ContractActivationRequested` outbox event that drives Contract-activation evaluation in the Contracts slice. Activation still requires both Contract signatures plus a positive priced milestone; it is **not** triggered by milestone approval alone.

**Response Structure (What they get): `200 OK`**

`ApiResponse<MilestoneActionResultDto>`:

```json
{
  "success": true,
  "data": {
    "entityId": "22222222-2222-2222-2222-222222222222",
    "status": "AwaitingFunding",
    "occurredAt": "2026-08-12T10:05:00Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```

After the first signature, `status` remains `"Draft"`.

> The response contains **no new `version`** and sets no HTTP ETag. The counterparty must re-fetch the milestone (list or contract detail) to obtain a fresh `version` before approving. On full approval, also re-fetch to see the new `status`.

**Endpoint-specific failures**

| Status | Condition | Message / response |
|---:|---|---|
| `400` | Contract in non-negotiable state | `لا يمكن التفاوض على مراحل عقد غير نشط أو غير موجود كمسودة.` |
| `400` | Milestone not Draft | `لا يمكن تعديل أو اعتماد شروط مرحلة خرجت من حالة المسودة.` |
| `403` | Role OK but not a party | `هذا الإجراء متاح لطرفي العقد فقط.` |
| `404` | Milestone absent | `المرحلة غير موجودة.` |
| `409` | Already approved by caller | `وافق العميل على النسخة الحالية من المرحلة مسبقًا.` / `وافق المحامي ...` |
| `409` | Stale milestone version | Concurrency message. |
| `412` | Malformed `If-Match` / save race | Manual validator message / concurrency prose. |
| `401/403` | Role not Client/Lawyer | Framework authorization response. |

---

### 1.5 Mark a Milestone Ready for Funding

**HTTP Method & Exact Route:** `POST /api/milestones/{milestoneId}/ready-for-funding`

**Purpose:** Lawyer signals the mutually approved milestone may now be funded by the Client. It does **not** change the milestone's enum status (stays `AwaitingFunding`); it only records `ReadyForFundingAt` and notifies the Client.

**Request Structure (What they send)**

| Location | Name | Required | Type | Details |
|---|---|---|---:|---|---|
| Header/cookie | Authentication | Yes | JWT | Must be the Contract's **Lawyer** (service-enforced, beyond the controller role list). |
| Header | `If-Match` | **Yes** | string | Milestone ETag. |
| Route | `milestoneId` | Yes | UUID | Target. |
| Body | — | No | — | None. |

**Business preconditions**

- Contract must be `Active` (`يجب أن يكون العقد نشطًا قبل تجهيز المرحلة للتمويل.` → 400).
- Milestone must be `AwaitingFunding` (`يمكن تجهيز المرحلة للتمويل بعد موافقة الطرفين عليها فقط.` → 400).
- It must be the **current sequential** milestone; earlier unresolved milestones block it (`يجب تسوية المراحل السابقة قبل تجهيز هذه المرحلة للتمويل.` → 400).
- No other milestone may hold an unsettled `Funded`/`Frozen` escrow hold or be `FundingProcessing` (`لا يمكن تجهيز مرحلة جديدة قبل حسم التمويل أو التسوية الحالية.` → 409).
- Already marked → `تم تجهيز المرحلة الحالية للتمويل مسبقًا.` (409).

**Response Structure (What they get): `200 OK`** — `ApiResponse<MilestoneActionResultDto>` with `status` `"AwaitingFunding"`, plus `milestone.ready-for-funding` notification to the Client. No new version returned.

**Endpoint-specific failures**

| Status | Condition | Message / response |
|---:|---|---|
| `400` | Contract not Active | `يجب أن يكون العقد نشطًا قبل تجهيز المرحلة للتمويل.` |
| `400` | Milestone not AwaitingFunding | `يمكن تجهيز المرحلة للتمويل بعد موافقة الطرفين عليها فقط.` |
| `400` | Not sequential | `يجب تسوية المراحل السابقة قبل تجهيز هذه المرحلة للتمويل.` |
| `403` | Caller is not the contract Lawyer | `محامي العقد فقط هو من يمكنه تجهيز المرحلة للتمويل.` |
| `404` | Milestone absent | `المرحلة غير موجودة.` |
| `409` | Already marked / unsettled financial activity / stale version | Respective message. |
| `412` | Malformed `If-Match` / save race | Respective message. |

---

### 1.6 Submit Milestone Work (Lawyer)

**HTTP Method & Exact Route:** `POST /api/milestones/{milestoneId}/submit`

**Purpose:** Lawyer delivers the funded milestone's work for Client review, with mandatory notes and uploaded file references. Multiple submissions are supported via a monotonically-increasing submission version.

**Request Structure (What they send)**

| Location | Name | Required | Type | Details |
|---|---|---|---:|---|---|
| Header/cookie | Authentication | Yes | JWT | Must be the Contract's **Lawyer**. |
| Header | `Content-Type` | Yes | string | `application/json` |
| Route | `milestoneId` | Yes | UUID | Target milestone. |
| Body | `notes` | Yes | string | Non-empty; max 10,000. |
| Body | `storedFileIds` | Yes | array of UUID | At least one; no `Guid.Empty`; **distinct**; every ID must be a stored file owned by the acting Lawyer. |

```json
{
  "notes": "تم إيداع صحيفة الدعوى وجميع مرفقاتها لدى المحكمة الابتدائية.",
  "storedFileIds": ["aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"]
}
```

**Business preconditions**

- Contract must be `Active` (`يجب أن يكون العقد نشطًا قبل تسليم أعمال المرحلة.`).
- Milestone must be `FundedInProgress` (`يمكن تسليم أعمال المرحلة عندما تكون ممولة وقيد التنفيذ فقط.`).
- Verified funding must match the milestone's contract, gross `amount`, and `EGP` currency (`بيانات تمويل المرحلة لا تطابق العقد أو المبلغ أو العملة المطلوبة للتسليم.`).
- All `storedFileIds` must pass `ContractFileAccessService.AuthorizeForUseAsync` (purpose `MilestoneSubmission`) **and** belong to the acting Lawyer (`تعذر التحقق من ملكية جميع ملفات تسليم المرحلة للمحامي الحالي.` → 403).
- Creates a `MilestoneSubmission` (version = previous max + 1) plus one `MilestoneSubmissionAttachment` per file.
- Unique index `UX_MilestoneSubmissions_MilestoneId_Version` turns concurrent double-submit into 409.
- On success: status → `Submitted`, `SubmittedAt` set, `AutoAcceptEligibleAt = now + 7 days`, a background auto-accept job is scheduled.

**Response Structure (What they get): `200 OK`** — `ApiResponse<MilestoneDto>` with `status: 4`, non-null `submittedAt`, `autoAcceptEligibleAt`, and `permittedActions` empty for the Lawyer (Client gets `Accept`/`RequestChanges`). Notifies `milestone.submitted` to the Client.

**Endpoint-specific failures**

| Status | Condition | Message / response |
|---:|---|---|
| `400` | Body validation (notes / files / duplicates) | Automatic validation problem. |
| `400` | Contract not Active / milestone not FundedInProgress | Respective message. |
| `400` | Funding mismatch for submission | `بيانات تمويل المرحلة لا تطابق العقد أو المبلغ أو العملة المطلوبة للتسليم.` |
| `403` | Not the contract Lawyer / files not owned by lawyer | `محامي العقد فقط هو من يمكنه تسليم أعمال المرحلة.` / file-ownership message. |
| `404` | Milestone absent | `المرحلة غير موجودة.` |
| `409` | Concurrent duplicate submission version | `تم تسجيل تسليم آخر لهذه المرحلة بالتزامن. يرجى إعادة تحميل المرحلة والمحاولة مرة أخرى.` |
| `401/403` | Role not Lawyer | Framework authorization response. |

---

### 1.7 Accept Submitted Work (Client)

**HTTP Method & Exact Route:** `POST /api/milestones/{milestoneId}/accept`

**Purpose:** Client approves the submitted work; the milestone moves `Submitted → AcceptedHold` and the escrow enters its **14-day hold/dispute window** before release.

**Request Structure (What they send)**

| Location | Name | Required | Type | Details |
|---|---|---|---:|---|---|
| Header/cookie | Authentication | Yes | JWT | Must be the Contract's **Client**. |
| Route | `milestoneId` | Yes | UUID | Target milestone. |
| Body | — | No | — | None. |

**Business preconditions**

- Contract `Active` (`يجب أن يكون العقد نشطًا قبل قبول تسليم المرحلة.`).
- Milestone `Submitted` (`يمكن قبول تسليم المرحلة عندما تكون في حالة المراجعة فقط.`).
- Current submission version verifies against the funded escrow hold.
- Sets `AcceptedAt`, `AcceptanceSource = Manual`, `HoldStartsAt = now`, `HoldExpiresAt = now + 14 days`; cancels auto-accept.
- **No `If-Match` header** exists on this endpoint.

**Response Structure (What they get): `200 OK`** — `ApiResponse<MilestoneDto>` (`status: 5`, non-null `holdExpiresAt`, `fundingStatus: 2`). Notifies `milestone.accepted` to the Lawyer. Hold release is scheduled for expiry by a background job.

**Endpoint-specific failures**

| Status | Condition | Message / response |
|---:|---|---|
| `400` | Contract not Active / milestone not Submitted | Respective message. |
| `400` | No valid current submission / hold missing / funding mismatch | `لا يوجد إصدار تسليم حالي صالح للمراجعة.` / `تعذر العثور على حجز الضمان الممول المرتبط بالمرحلة.` / verification message. |
| `403` | Caller not the contract Client | `عميل العقد فقط هو من يمكنه قبول تسليم المرحلة.` |
| `404` | Milestone absent | `المرحلة غير موجودة.` |
| `401/403` | Role not Client | Framework authorization response. |

---

### 1.8 Request Changes on Submitted Work (Client)

**HTTP Method & Exact Route:** `POST /api/milestones/{milestoneId}/request-changes`

**Purpose:** Client sends the submitted work back to the Lawyer with feedback. Milestone returns `Submitted → FundedInProgress`, ready for resubmission.

**Request Structure (What they send)**

| Location | Name | Required | Type | Details |
|---|---|---|---:|---|---|
| Header/cookie | Authentication | Yes | JWT | Must be the Contract's **Client**. |
| Header | `Content-Type` | Yes | string | `application/json` |
| Route | `milestoneId` | Yes | UUID | Target milestone. |
| Body | `reason` | Yes | string | Non-empty; max 2,000. Stored as the milestone's `RejectionReason`. |

```json
{
  "reason": "يرجى استكمال المستندات الثبوتية وإعادة رفع شهادة المحكمة."
}
```

**Business preconditions**

- Contract `Active`; milestone `Submitted`.
- On success: `SubmittedAt` cleared, `AutoAcceptEligibleAt`/`AutoAcceptJobId` cleared, `RejectionReason = reason`, milestone → `FundedInProgress`.
- **No `If-Match`.** Concurrent `accept` vs `request-changes` is last-writer-wins with no optimistic guard.

**Response Structure (What they get): `200 OK`** — `ApiResponse<MilestoneDto>` (`status: 3`, `submittedAt: null`). Notifies `milestone.changes-requested` to the Lawyer.

**Endpoint-specific failures**

| Status | Condition | Message / response |
|---:|---|---|
| `400` | Reason empty / too long | Automatic validation problem. |
| `400` | Contract not Active / milestone not Submitted | Respective message. |
| `403` | Caller not the contract Client | `عميل العقد فقط هو من يمكنه طلب تعديلات على تسليم المرحلة.` |
| `404` | Milestone absent | `المرحلة غير موجودة.` |

---

### 1.9 Create a Milestone Extension Change Request

**HTTP Method & Exact Route:** `POST /api/milestones/{milestoneId}/change-requests`

**Purpose:** Either party proposes an **extension-only** amendment (description / duration / due date) to a currently executed (`FundedInProgress`) milestone. Amount is immutable. The other party must decide.

**Request Structure (What they send)**

| Location | Name | Required | Type | Details |
|---|---|---|---:|---|---|
| Header/cookie | Authentication | Yes | JWT | `Client` or `Lawyer` Contract party. |
| Header | `If-Match` | **Yes** | string | **Milestone** ETag (`MilestoneDto.version`). |
| Header | `Content-Type` | Yes | string | `application/json` |
| Route | `milestoneId` | Yes | UUID | Target milestone. |
| Body | `proposedDescription` | No | string | Max 10,000; must differ from current description when provided. |
| Body | `proposedDurationDays` | No | int | 1–365; **must be longer** than the current duration when provided. |
| Body | `proposedDueDate` | No | ISO-8601 | Future; **must be later** than the current due date when provided. |
| Body | `reason` | Yes | string | Non-empty; max 2,000. |

At least one proposed field must be present and **actually different/more-forward** than the current milestone value.

```json
{
  "proposedDescription": "تمديد مرحلة الإيداع لتشمل جولة تقصٍّ إضافية.",
  "proposedDurationDays": 30,
  "proposedDueDate": "2026-10-01T00:00:00Z",
  "reason": "تعذر استلام رد المحكمة قبل الموعد الحالي."
}
```

**Business preconditions**

- Contract must be `Active` (via participant + milestone check), milestone must be `FundedInProgress` (`يمكن تقديم أو معالجة طلبات التعديل عندما تكون المرحلة مُمولة وقيد التنفيذ فقط.`).
- Actual & forward change required (see section 3.3 for the third set of service-side rules).
- One pending change request per milestone at a time — the `hasPendingRequest` pre-check throws `يوجد طلب تعديل معلق لهذه المرحلة بالفعل.` (409); the filtered unique index `UX_MilestoneChangeRequests_Pending` is the race backstop (also 409 intended, but degraded by the index-name mismatch in 5.3).
- On success creates a `MilestoneChangeRequest` with `Status = Pending` (0) and notifies the counterparty `milestone.change-request-created`.

**Response Structure (What they get): `201 Created`**

`ApiResponse<MilestoneActionResultDto>`:

```json
{
  "success": true,
  "data": {
    "entityId": "99999999-9999-9999-9999-999999999999",
    "status": "Pending",
    "occurredAt": "2026-08-12T11:00:00Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```

> The response returns **no change-request `version`**. As implemented, no API exposes `MilestoneChangeRequest` rowversion, so the decision endpoints' required `If-Match` cannot be satisfied by a client (see section 5.3). Treat create as a dead end in the current wire contract.

**Endpoint-specific failures**

| Status | Condition | Message / response |
|---:|---|---|
| `400` | Body validation (at-least-one-change, lengths, future date) | Automatic validation problem. |
| `400` | Milestone not `FundedInProgress` / no real change / not forward extension | Respective messages. |
| `403` | Not a party | `هذا الإجراء متاح لطرفي العقد فقط.` |
| `404` | Milestone absent | `المرحلة المطلوبة غير موجودة.` |
| `409` | Pending request already exists / stale **milestone** version | `يوجد طلب تعديل معلق لهذه المرحلة بالفعل.` / concurrency message. |
| `412` | Malformed `If-Match` / save race | Respective message. |

---

### 1.10 Approve a Milestone Extension Change Request

**HTTP Method & Exact Route:** `POST /api/change-requests/{changeRequestId}/approve`

**Purpose:** The **counterparty** (not the requester) approves the pending extension. Approved fields are applied to the milestone.

**Request Structure (What they send)**

| Location | Name | Required | Type | Details |
|---|---|---|---:|---|---|
| Header/cookie | Authentication | Yes | JWT | `Client` or `Lawyer` Contract party, **not** the requester. |
| Header | `If-Match` | **Yes** | string | **Change-request** ETag (unobtainable today — see 1.9 note). |
| Route | `changeRequestId` | Yes | UUID | Target change request. |
| Body | — | No | — | None. |

**Business preconditions**

- Requester cannot decide own request (`لا يمكن لمقدم طلب التعديل اعتماد الطلب أو رفضه.` → 403).
- Request must be `Pending`; milestone `FundedInProgress`; extension must still be forward at decision time.
- Applies proposed description/duration/dueDate to the milestone; sets `Status = Approved` (1), `DecidedByUserId`, `DecidedAt`, fixed `DecisionReason`.

**Response Structure (What they get): `200 OK`** — `ApiResponse<MilestoneActionResultDto>` with `status: "Approved"`. Notifies requester `milestone.change-request-approved`.

**Endpoint-specific failures**

| Status | Condition | Message / response |
|---:|---|---|
| `400` | Milestone not FundedInProgress / no longer a forward extension | Respective message. |
| `400` | Request not Pending | `طلب التعديل لم يعد في حالة الانتظار.` |
| `403` | Caller is the requester / not a party | `لا يمكن لمقدم طلب التعديل اتخاذ القرار عليه بنفسه.` / `هذا الإجراء متاح لطرفي العقد فقط.` |
| `404` | Change request or milestone absent | `طلب التعديل المطلوب غير موجود.` / `المرحلة المطلوبة غير موجودة.` |
| `409` | Stale change-request version | `تم تعديل طلب التعديل بواسطة عملية أخرى. ...` |
| `412` | Malformed `If-Match` / save race | Respective message. |

---

### 1.11 Reject a Milestone Extension Change Request

**HTTP Method & Exact Route:** `POST /api/change-requests/{changeRequestId}/reject`

**Purpose:** The counterparty rejects the pending extension, recording a decision reason. No milestone fields change.

**Request Structure (What they send)**

| Location | Name | Required | Type | Details |
|---|---|---|---:|---|---|
| Header/cookie | Authentication | Yes | JWT | Counterparty (participant, not requester). |
| Header | `If-Match` | **Yes** | string | Change-request ETag. |
| Header | `Content-Type` | Yes | string | `application/json` |
| Route | `changeRequestId` | Yes | UUID | Target. |
| Body | `reason` | Yes | string | Non-empty; max 2,000. Stored as `DecisionReason`. |

```json
{
  "reason": "المدة المقترحة لا تبرر التأخير في هذه المرحلة."
}
```

**Business preconditions** — same as approve (pending, counterparty-only, current version); sets `Status = Rejected` (2) with requester notified `milestone.change-request-rejected`.

**Response Structure (What they get): `200 OK`** — `ApiResponse<MilestoneActionResultDto>` with `status: "Rejected"`.

**Endpoint-specific failures** — mirror approve: 400 (not pending / milestone state), 403 (requester / not party), 404 (absent), 409 (stale version), 412 (malformed `If-Match`), plus automatic 400 for empty/long `reason`.

---

### 1.12 Cancel a Milestone Extension Change Request

**HTTP Method & Exact Route:** `POST /api/change-requests/{changeRequestId}/cancel`

**Purpose:** Only the **requester** withdraws their own pending request before the other party decides.

**Request Structure (What they send)**

| Location | Name | Required | Type | Details |
|---|---|---|---:|---|---|
| Header/cookie | Authentication | Yes | JWT | Must be the `RequestedByUserId` of the request. |
| Header | `If-Match` | **Yes** | string | Change-request ETag. |
| Route | `changeRequestId` | Yes | UUID | Target. |
| Body | — | No | — | None. |

**Business preconditions**

- Requester-only: `مقدم طلب التعديل فقط هو من يمكنه إلغاء الطلب.` (403).
- Pending only; sets `Status = Cancelled` (3); notifies counterparty `milestone.change-request-cancelled`.

**Response Structure (What they get): `200 OK`** — `ApiResponse<MilestoneActionResultDto>` with `status: "Cancelled"`.

**Endpoint-specific failures**

| Status | Condition | Message / response |
|---:|---|---|
| `400` | Request not Pending | `طلب التعديل لم يعد في حالة الانتظار.` |
| `403` | Not the requester | `مقدم طلب التعديل فقط هو من يمكنه إلغاء الطلب.` |
| `404` | Request absent | `طلب التعديل المطلوب غير موجود.` |
| `409` | Stale change-request version | Concurrency message. |
| `412` | Malformed `If-Match` / save race | Respective message. |

---

## 2. Exhaustive DTO & Field Dictionary

### 2.1 Request DTOs

#### `AddMilestoneRequest`

| Field name | Data type | Required | Description / mechanics |
|---|---|---:|---|
| `title` | string | Yes | User-facing milestone title; 3–200 chars. |
| `description` | string | No | Optional details; max 10,000; whitespace-only rejected. |
| `orderNumber` | int | Yes | Sequential position; must equal max existing + 1. Unique per contract. |
| `amount` | decimal | Yes | **Agreed payment for this milestone** in EGP, `> 0`, ≤ 2 decimals. Immutable after creation. |
| `durationDays` | int | No | Planned duration, 1–365 when set. |
| `dueDate` | DateTime | No | Scheduled due date, must be future UTC when set. |

#### `UpdateMilestoneRequest`

| Field name | Data type | Required | Description / mechanics |
|---|---|---:|---|
| `title` | string | Yes | Complete replacement; 3–200 chars; resets both acceptances. |
| `description` | string | No | Max 10,000; whitespace-only rejected. |
| `durationDays` | int | No | 1–365 when set. |
| `dueDate` | DateTime | No | Future when set. |

`amount` and `orderNumber` are intentionally absent (immutable).

#### `SubmitMilestoneRequest`

| Field name | Data type | Required | Description / mechanics |
|---|---|---:|---|
| `notes` | string | Yes | Delivery notes; non-empty, max 10,000. |
| `storedFileIds` | array of UUID | Yes | Pre-uploaded file references; ≥1, distinct, no `Guid.Empty`, all owned by the acting Lawyer. |

#### `RequestMilestoneChangesRequest`

| Field name | Data type | Required | Description / mechanics |
|---|---|---:|---|
| `reason` | string | Yes | Delivery-revision feedback; non-empty, max 2,000. Persisted as milestone `RejectionReason`. |

#### `CreateMilestoneChangeRequest`

| Field name | Data type | Required | Description / mechanics |
|---|---|---:|---|
| `proposedDescription` | string | No | Max 10,000; must differ from current. |
| `proposedDurationDays` | int | No | 1–365; must exceed current duration. |
| `proposedDueDate` | DateTime | No | Future; must be later than current due date. |
| `reason` | string | Yes | Non-empty, max 2,000. |

At least one proposed field must be set. **No `amount` field — milestone price amendments are impossible.**

#### `RejectChangeRequest`

| Field name | Data type | Required | Description / mechanics |
|---|---|---:|---|
| `reason` | string | Yes | Rejection rationale for the counterparty; non-empty, max 2,000. Stored as `DecisionReason`. |

### 2.2 Response `data` DTOs

#### `MilestoneDto`

| Field name | Data type | Nullable | Description / frontend use |
|---|---|---:|---|
| `id` | UUID string | No | Milestone id used by all `/api/milestones/{id}/...` routes. |
| `orderNumber` | int | No | Sequential execution order. |
| `title` | string | No | Current title. |
| `description` | string | Yes | Current description. |
| `amount` | decimal | No | Agreed EGP amount; server-owned, read-only. |
| `durationDays` | int | Yes | Current planned duration. |
| `dueDate` | DateTime | Yes | Current due date. |
| `status` | `MilestoneStatus` (JSON int) | No | Current lifecycle state (all values in 2.4). |
| `fundingStatus` | `MilestoneFundingStatus` (JSON int) | No | Derived funding summary (2.4). |
| `escrowHoldId` | UUID string | Yes | Escrow hold once funding is attempted/created. |
| `fundedAt` | DateTime | Yes | Successful funding time. |
| `submittedAt` | DateTime | Yes | Latest Lawyer submission time; cleared on revision request. |
| `autoAcceptEligibleAt` | DateTime | Yes | Earliest automatic acceptance; set `submittedAt + 7 days`, cleared on accept/revision. |
| `holdExpiresAt` | DateTime | Yes | End of accepted-hold/dispute window; `accepted + 14 days`. |
| `netLawyerAmount` | decimal | Yes | Escrow net payable to Lawyer after platform fee. |
| `version` | string | No | **Optimistic concurrency token:** strong quoted Base64 rowversion; send verbatim as `If-Match` for milestone mutations. |
| `permittedActions` | array of string | No | UI hints; values `Update`, `Approve`, `ReadyForFunding`, `Submit`, `Accept`, `RequestChanges`. Do not treat as authorization truth (known gaps in section 5). |

**Not exposed** (internal entity fields): `AcceptedByClientAt`, `AcceptedByLawyerAt`, `ReadyForFundingAt`, `AcceptedAt`, `AcceptanceSource`, `SubmissionVersion`, `RejectionReason`, `AutoAcceptJobId`, `HoldStartsAt`, `ReleasedAt`, `RefundedAt`, `CreatedAt`, `UpdatedAt`, raw rowversion bytes.

#### `MilestoneActionResultDto`

| Field name | Data type | Description |
|---|---|---|
| `entityId` | UUID string | The affected aggregate id (`milestoneId` for approve/ready, `changeRequestId` for change-request decisions). Generic name, not `milestoneId`. |
| `status` | string | Enum **name as text**: `Draft`, `AwaitingFunding`, `Pending`, `Approved`, `Rejected`, `Cancelled`, etc. No new `version` is included. |
| `occurredAt` | DateTime | Processing timestamp; populated even when no state transition occurred (e.g. first approval). |

#### `MilestoneChangeRequestDto` (defined but **never returned**)

| Field name | Data type | Nullable | Description |
|---|---|---:|---|
| `id` | UUID string | No | Change-request id (used by decision routes). |
| `milestoneId` | UUID string | No | Milestone being amended. |
| `requestedByUserId` | UUID string | No | Requester (cannot self-decide). |
| `proposedDescription` | string | Yes | Proposed description. |
| `proposedDurationDays` | int | Yes | Proposed duration. |
| `proposedDueDate` | DateTime | Yes | Proposed due date. |
| `reason` | string | No | Requester's rationale. |
| `status` | `ChangeRequestStatus` (JSON int) | No | `Pending`/`Approved`/`Rejected`/`Cancelled`. |
| `decidedByUserId` | UUID string | Yes | Deciding participant. |
| `decidedAt` | DateTime | Yes | Decision time. |
| `createdAt` | DateTime | No | Creation time. |
| `decisionReason` | string (init) | Yes | Decision prose (fixed text for approve/cancel, requester reason for reject). |

> No controller action returns this DTO, and it declares **no `version` field**, so the rowversion needed by the `/approve`, `/reject`, `/cancel` `If-Match` header is unavailable over the API. See `5.3`.

#### `PagedResult<T>` — **not used** by this slice

Listing returns a bare array. There is no paging wrapper anywhere in `/api/.../milestones` routes.

### 2.3 Shared response envelope

#### `ApiResponse<T>`

| Field name | Data type | Nullable | Description |
|---|---|---:|---|
| `success` | boolean | No | `true` success; `false` middleware failures. |
| `data` | `T` | Yes | Payload; `null` on failure. |
| `message` | string | Yes | Optional prose; Milestone successes do not set it. |
| `errors` | array of string | Yes | Only some custom `ValidationException`s; automatic validation uses a dictionary in problem details. |
| `statusCode` | int | No | HTTP status copied into the body (200/201 or failure code). |

### 2.4 Complete enum dictionary

#### `MilestoneStatus`

| JSON value | Enum name | Meaning |
|---:|---|---|
| `0` | `Draft` | Terms being drafted/approved; editable via PUT; acceptance required. |
| `1` | `AwaitingFunding` | Mutually approved; Lawyer may mark ready for funding. |
| `2` | `FundingProcessing` | Payment provider funding attempt in progress (set via Payments slice). |
| `3` | `FundedInProgress` | Escrow funded; Lawyer executing work. |
| `4` | `Submitted` | Work delivered for Client review. |
| `5` | `AcceptedHold` | Work accepted (manual or auto); funds in 14-day hold/dispute window. |
| `6` | `Disputed` | Under formal dispute (Disputes slice; hold frozen). |
| `7` | `Released` | Escrow released to Lawyer. Terminal. |
| `8` | `Refunded` | Escrow refunded to Client. Terminal. |
| `9` | `Cancelled` | Cancelled (contract termination). Terminal. |

#### `MilestoneFundingStatus`

| JSON value | Enum name | Meaning |
|---:|---|---|
| `0` | `Unfunded` | No escrow hold yet. |
| `1` | `Processing` | Milestone is `FundingProcessing`. |
| `2` | `Funded` | Escrow hold exists and is not settled. |
| `3` | `Settled` | Milestone/hold released or refunded. |

#### `ChangeRequestStatus`

| JSON value | Enum name | Meaning |
|---:|---|---|
| `0` | `Pending` | Awaiting counterparty decision. |
| `1` | `Approved` | Applied to the milestone. Terminal. |
| `2` | `Rejected` | Declined with reason. Terminal. |
| `3` | `Cancelled` | Withdrawn by requester. Terminal. |

#### `MilestoneAcceptanceSource` (internal, not in any DTO)

| JSON value | Enum name | Meaning |
|---:|---|---|
| `0` | `Manual` | Client explicitly accepted submission. |
| `1` | `Automatic` | Auto-accepted after the 7-day review window. |

### 2.5 Supported transitions (persistence-level guard)

All Legally allowed `MilestoneStatus` transitions per `MilestoneTransitionGuard`:

| From | To | Trigger |
|---|---|---|
| `Draft` | `AwaitingFunding` | Both parties approve terms (1.4). |
| `Draft` | `Cancelled` | Contract termination. |
| `AwaitingFunding` | `FundingProcessing` | Funding attempt (Payments). |
| `AwaitingFunding` | `Cancelled` | Contract termination. |
| `FundingProcessing` | `FundedInProgress` | Funding success (Payments). |
| `FundingProcessing` | `AwaitingFunding` | Funding failure/rollback (Payments). |
| `FundingProcessing` | `Cancelled` | Termination during processing. |
| `FundedInProgress` | `Submitted` | Lawyer submits (1.6) **or re-submits after revision**. |
| `FundedInProgress` | `Cancelled` | Contract termination. |
| `FundedInProgress` | `Refunded` | Termination refund of executed milestone. |
| `Submitted` | `FundedInProgress` | Client requests changes (1.8). |
| `Submitted` | `AcceptedHold` | Client accepts (1.7) or auto-accept job. |
| `Submitted` | `Refunded` | Termination while in review. |
| `AcceptedHold` | `Disputed` | Dispute opened (Disputes slice). |
| `AcceptedHold` | `Released` | Hold expiry release job. |
| `AcceptedHold` | `Refunded` | Dispute/termination settlement to Client. |
| `Disputed` | `Released` | Dispute resolution to Lawyer. |
| `Disputed` | `Refunded` | Dispute resolution to Client. |

`ChangeRequestStatus` transitions: `Pending → Approved | Rejected | Cancelled` only.

### 2.6 Advanced API mechanics

#### Optimistic concurrency

| Operation | Token source | Required header | Failure behavior |
|---|---|---|---|
| Update Draft Milestone | `MilestoneDto.version` | `If-Match` | Malformed → 412; valid-but-stale → 409; write race → 412. |
| Approve terms | `MilestoneDto.version` | `If-Match` | Same. Successful approve returns no new token; counterparty must re-fetch. |
| Ready-for-funding | `MilestoneDto.version` | `If-Match` | Same. |
| Create change request | `MilestoneDto.version` | `If-Match` | Same. |
| Approve/Reject/Cancel change request | **None — not exposed** | `If-Match` (change-request rowversion) | Cannot be satisfied by clients as implemented (5.3). |
| Submit | — | None | Unique `UX_MilestoneSubmissions_MilestoneId_Version` index → 409 on duplicate. |
| Accept / Request-changes | — | None | **No concurrency guard**; last-writer-wins between the two actions. |

Always send the token **verbatim with its surrounding quotes** (`"AAAAAAAAB9E="`). Weak tags (`W/"..."`) and `*` are rejected. `updatedAt` is internal and not a concurrency token.

#### Idempotency

| Operation | `Idempotency-Key` support | Retry semantics |
|---|---|---|
| Add milestone | None | The sequential `orderNumber` gate returns 400 for any retry that used a stale `max+1`; a true race falls through to the unique index, but returns 409 only if the index-name catch matches (currently unreliable, see 5.3). Resolve by re-listing. |
| Approve / Ready-for-funding | None | Guarded by rowversion; retry with stale token → 409. First-party approve is not repeatable. |
| Submit | None | Duplicate submission version race → 409; subsequent intended re-submits are valid new versions. |
| Accept / Request-changes | None | Not idempotent; retrying a timed-out accept/request produces a state-guard 400 rather than a duplicate record. |
| Change requests | None | Unique pending-index → 409 on duplicate create; decisions guarded by change-request rowversion. |
| **Fund milestone** (Payments slice) | **Yes** | `POST /api/milestones/{milestoneId}/fund` consumes `Idempotency-Key`; the only Milestone-adjacent idempotent mutation. |

---

## 3. Validation Rules Summary

### 3.1 Exact field and query validation

| DTO / input | Field | Required | Exact rule | Validator message |
|---|---|---:|---|---|
| `AddMilestoneRequest` | `title` | Yes | Not empty; length 3–200 | `عنوان المرحلة مطلوب.` / `عنوان المرحلة يجب أن يكون بين 3 و200 حرف.` |
| `AddMilestoneRequest` | `description` | No | Null or non-whitespace; max 10,000 | `وصف المرحلة لا يمكن أن يكون فارغًا.` / `وصف المرحلة يجب ألا يتجاوز 10000 حرف.` |
| `AddMilestoneRequest` | `orderNumber` | Yes | > 0 | `ترتيب المرحلة يجب أن يكون أكبر من صفر.` |
| `AddMilestoneRequest` | `amount` | Yes | > 0; ≤ 2 decimal places | `قيمة المرحلة يجب أن تكون أكبر من صفر بالجنيه المصري.` / `قيمة المرحلة يجب ألا تتجاوز منزلتين عشريتين.` |
| `AddMilestoneRequest` | `durationDays` | No | 1–365 when set | `مدة المرحلة يجب أن تكون بين يوم واحد و365 يومًا.` |
| `AddMilestoneRequest` | `dueDate` | No | Strictly future when set | `تاريخ استحقاق المرحلة يجب أن يكون في المستقبل.` |
| `UpdateMilestoneRequest` | `title` | Yes | 3–200 | Same as Add. |
| `UpdateMilestoneRequest` | `description` | No | Null/non-whitespace; max 10,000 | Same as Add. |
| `UpdateMilestoneRequest` | `durationDays` | No | 1–365 when set | Same. |
| `UpdateMilestoneRequest` | `dueDate` | No | Future when set | Same. |
| `SubmitMilestoneRequest` | `notes` | Yes | Not empty; max 10,000 | `ملاحظات التسليم مطلوبة.` / `ملاحظات التسليم يجب ألا تتجاوز 10000 حرف.` |
| `SubmitMilestoneRequest` | `storedFileIds` | Yes | Non-null; ≥1; no `Guid.Empty`; all distinct | `يجب إرفاق ملف واحد على الأقل.` / `يجب تحديد معرّفات ملفات صالحة ومصرح بها.` |
| `RequestMilestoneChangesRequest` | `reason` | Yes | Not empty; max 2,000 | `سبب طلب التعديلات مطلوب.` / `سبب طلب التعديلات يجب ألا يتجاوز 2000 حرف.` |
| `CreateMilestoneChangeRequest` | `proposedDescription` | No | Null/non-whitespace; max 10,000 | `الوصف المقترح يجب ألا يتجاوز 10000 حرف.` / `الوصف المقترح لا يمكن أن يكون فارغًا.` |
| `CreateMilestoneChangeRequest` | `proposedDurationDays` | No | 1–365 when set | `المدة المقترحة يجب أن تكون بين يوم واحد و365 يومًا.` |
| `CreateMilestoneChangeRequest` | `proposedDueDate` | No | Future when set | `تاريخ الاستحقاق المقترح يجب أن يكون في المستقبل.` |
| `CreateMilestoneChangeRequest` | *(whole request)* | — | Class-level: at least one proposed change | `يجب أن يتضمن طلب التعديل تغييرًا واحدًا على الأقل.` |
| `CreateMilestoneChangeRequest` | `reason` | Yes | Not empty; max 2,000 | `سبب طلب التعديل مطلوب.` / `سبب طلب التعديل يجب ألا يتجاوز 2000 حرف.` |
| `RejectChangeRequest` | `reason` | Yes | Not empty; max 2,000 | `سبب رفض طلب التعديل مطلوب.` / `سبب رفض طلب التعديل يجب ألا يتجاوز 2000 حرف.` |
| `If-Match` header | — | Yes on 7 endpoints | Strong quoted Base64 ETag; ≥1 decoded byte; weak/wildcard/empty rejected | Manual validator → `PreconditionFailedException` → **412**. |

### 3.2 Cross-property and domain validation

| Rule | Where enforced | Frontend behavior to mirror |
|---|---|---|
| Milestones negotiable only while Contract is `Draft` (add/update); approve additionally allowed on `Active` | Draft/Service service gates | Disable add/update after activation; allow terms-approve while Active. |
| Milestone add requires strict sequential `orderNumber` = max+1 | Draft service | Always send the next integer; a rejected add 400 should refresh the list (a counterparty may have added one). |
| Only a Contract party may mutate a milestone | `EnsureParticipant` everywhere | Gate every control by the current user's party role AND identity (not just `permittedActions`). |
| Only the Contract Lawyer may ready-for-fund / submit; only the Client may accept / request-changes | Service | Do not render Lawyer actions for Client and vice-versa. |
| Funding must match contract/amount/EGP before submit/accept; hold must exist and be Funded | Funding verifier | Treat these as non-recoverable-from-form failures. |
| Submitted work requires ≥1 owned file; files must belong to the acting Lawyer | File access service | Only offer already-uploaded files owned by the Lawyer; expect 403 otherwise. |
| Change requests only while milestone is `FundedInProgress` | Change-request service | Do not offer amendment controls in other statuses. |
| Extension-only change requests: duration must increase, due date must move later, description must change | Service `EnsureActualExtension`/`EnsureExtensionStillMovesForward` | Pre-validate forward changes in UI; server re-validates at decision time too. |
| One pending change request per milestone at a time | Service + unique index | Disable Create while a `Pending` request exists; handle 409 races. |
| Counterparty must decide; requester may cancel but not decide | `EnsureDecisionActor` in both services | Show decision controls only to the non-requester; cancel to requester. |
| Approving a milestone twice per version is forbidden | Service | Disable `Approve` for a party whose acceptance is already recorded (`permittedActions` reflects this). |
| Contract must be `Active` for ready/submit/accept/request-changes | Service | Gate those actions on Contract status 1. |
| `orderNumber`, `amount`, acceptance state rows are immutable after creation | Entity + DTO shape | Reflect immutability in forms. |

### 3.3 Important validation non-rules

- No regex anywhere; strings are not trimmed or HTML-sanitized — escape all rendered user text.
- No minimum beyond non-empty for `reason` texts (only max lengths).
- `amount` may not carry currency; 2-decimal max enforced, no upper monetary bound.
- `Accept`, `RequestChanges`, and `Submit` accept **no `If-Match`**; the only submit-gate is the unique submission-version index (409).
- Unknown JSON properties are ignored (no reject-on-unknown); all server-owned fields (`status`, `fundingStatus`, `fundedAt`, `escrowHoldId`, `autoAcceptEligibleAt`, `platformFee`, `acceptanceSource`, timestamps, versions) are absent from request bodies by design.
- Route ids have no FluentValidator; the `guid` constraint rejects non-GUIDs, while `Guid.Empty` reaches services and produces a business message depending on the operation.
- No change-request read path: although `MilestoneChangeRequestDto` exists, no endpoint returns it, so the frontend can never render a request's fields. A milestone may hold at most one `Pending` request (filtered unique index), but there is no cap on total historical requests.
- The auto-accept scheduling handler requires `AutoAcceptEligibleAt` and a single scheduled job id; a rejected/resubmitted submission re-schedules a fresh job.

---

## 4. Milestone Lifecycle Diagrams

### 4.1 State machine diagram

```mermaid
stateDiagram-v2
    [*] --> Draft: Party adds milestone to Draft contract\n(MilestoneCreated)

    state Draft {
        [*] --> AwaitingSignatures
        AwaitingSignatures --> AwaitingSignatures: Party updates draft terms via PUT\nresets both acceptances
        AwaitingSignatures --> OnePartySigned: Either party approves terms
        OnePartySigned --> AwaitingSignatures: Draft updated\nclears both acceptances
        OnePartySigned --> MutuallyApproved: Other party approves
    }

    Draft --> AwaitingFunding: Both parties approved\n(MilestoneApproved)
    Draft --> Cancelled: Contract termination

    AwaitingFunding --> FundingProcessing: Client funds (Payments slice)
    AwaitingFunding --> Cancelled: Contract termination

    FundingProcessing --> FundedInProgress: Funding success
    FundingProcessing --> AwaitingFunding: Funding failure / rollback
    FundingProcessing --> Cancelled: Termination during processing

    FundedInProgress --> Submitted: Lawyer submits work\n(MilestoneSubmitted)
    FundedInProgress --> Cancelled: Contract termination
    FundedInProgress --> Refunded: Termination refund

    Submitted --> FundedInProgress: Client requests changes\n(MilestoneChangesRequested)
    Submitted --> AcceptedHold: Client accepts (Manual)\nor review window elapses (Automatic)\n(MilestoneAccepted / MilestoneAutoAccepted)
    Submitted --> Refunded: Termination while in review

    AcceptedHold --> Released: Hold expiry release job\nfunds to Lawyer
    AcceptedHold --> Disputed: Dispute opened (Disputes slice)
    AcceptedHold --> Refunded: Settlement to Client
    Note right of AcceptedHold: 14-day Timer: hold/dispute window\nuntil HoldExpiresAt

    Disputed --> Released: Dispute resolved to Lawyer
    Disputed --> Refunded: Dispute resolved to Client

    Released --> [*]
    Refunded --> [*]
    Cancelled --> [*]
```

Every edge above is one of the 18 pairs in `MilestoneTransitionGuard.AllowedTransitions`. Re-submission after a revision is another `FundedInProgress → Submitted` firing with a new submission version (not a persisted state change). Timers (`AutoAcceptEligibleAt` = submit + 7 days; `HoldExpiresAt` = accept + 14 days) drive the automatic `Submitted → AcceptedHold` and `AcceptedHold → Released` edges via background jobs; the enum itself never encodes self-transitions.

### 4.2 Actor interaction sequence diagram

```mermaid
sequenceDiagram
    autonumber
    actor Client
    participant API as Smart Court API
    actor Lawyer
    participant Jobs as Outbox / background jobs

    Note over Client,Lawyer: Contract is Draft; both parties have signed the Contract
    Lawyer->>API: POST /api/contracts/{contractId}/milestones\nAddMilestoneRequest (orderNumber = next)
    API-->>Lawyer: 201 MilestoneDto (Draft, version)
    API-->>Client: Notification: milestone.created

    opt Lawyer or Client revises draft terms
        Lawyer->>API: PUT /api/contracts/{contractId}/milestones/{id}\nIf-Match + UpdateMilestoneRequest
        API-->>Lawyer: 200 MilestoneDto (new version, acceptances cleared)
        API-->>Client: Notification: milestone.draft-updated
    end

    Lawyer->>API: GET /api/contracts/{contractId}/milestones\n(fetch version)
    API-->>Lawyer: 200 [MilestoneDto...]
    Lawyer->>API: POST /api/milestones/{id}/approve\nIf-Match: milestone version
    API-->>Lawyer: 200 ActionResult (status "Draft", no version)
    API-->>Client: Notification: milestone.acceptance-recorded

    Client->>API: GET /api/contracts/{contractId}/milestones\n(refresh version after Lawyer signed)
    API-->>Client: 200 [MilestoneDto...]
    Client->>API: POST /api/milestones/{id}/approve\nIf-Match: fresh milestone version
    API-->>Client: 200 ActionResult (status "AwaitingFunding")
    API-->>Client: Notification: milestone.approved
    API-->>Lawyer: Notification: milestone.approved
    API-->>Jobs: ContractActivationRequested (activation re-evaluation)

    Lawyer->>API: POST /api/milestones/{id}/ready-for-funding\nIf-Match: milestone version
    API-->>Lawyer: 200 ActionResult (status "AwaitingFunding")
    API-->>Client: Notification: milestone.ready-for-funding

    Client->>API: POST /api/milestones/{id}/fund\nIdempotency-Key: unique funding key (Payments slice)
    API-->>Client: 200/202 FundingOperationDto
    Jobs->>API: Funding success -> milestone FundedInProgress + holds
    API-->>Client: Notification: milestone.funded
    API-->>Lawyer: Notification: milestone.funded

    Lawyer->>API: POST /api/milestones/{id}/submit\nSubmitMilestoneRequest {notes, storedFileIds}
    API-->>Lawyer: 200 MilestoneDto (Submitted, autoAcceptEligibleAt = +7d)
    API-->>Client: Notification: milestone.submitted
    Jobs->>API: Schedule auto-accept job at +7 days

    alt Client approves work manually
        Client->>API: POST /api/milestones/{id}/accept
        API-->>Client: 200 MilestoneDto (AcceptedHold, holdExpiresAt = +14d)
        API-->>Lawyer: Notification: milestone.accepted
        Jobs->>API: Schedule hold release at holdExpiresAt
        Jobs->>API: Release escrow -> Released; Contract completion re-evaluated
        API-->>Client: Notification: milestone/accepted or contract completion
        API-->>Lawyer: Notification: contract.completed
    else Client requests revision
        Client->>API: POST /api/milestones/{id}/request-changes\n{reason}
        API-->>Client: 200 MilestoneDto (FundedInProgress, submittedAt = null)
        API-->>Lawyer: Notification: milestone.changes-requested
    else Client is inactive for 7 days
        Jobs->>API: Auto-accept job fires (MilestoneAutoAcceptanceService)
        API-->>Client: Notification: milestone.auto-accepted
        API-->>Lawyer: Notification: milestone.auto-accepted
    end

    opt Change request (extension) during FundedInProgress
        Lawyer->>API: POST /api/milestones/{id}/change-requests\nIf-Match: milestone version + extension body
        API-->>Lawyer: 201 ActionResult (Pending)  [no version returned]
        API-->>Client: Notification: milestone.change-request-created
        Note over Client,API: Decision endpoints need a change-request If-Match\nthat no response currently exposes (see 5.3)
    end

    opt Qualification dispute on AcceptedHold
        Client->>API: POST /api/disputes (Disputes slice)
        API-->>Client: Milestone Disputed; Contract SuspendedByDispute
        Note over API,Jobs: Moderator resolves dispute; milestone -> Released/Refunded
    end
```

### Frontend refresh strategy

- Use `/hubs/notifications` (SignalR) `NotificationCreated`/`NotificationRead`/`NotificationsReadAll`, or REST `GET /api/notifications` / `GET /api/notifications/unread-count`.
- Milestone notification types: `milestone.created`, `milestone.draft-updated`, `milestone.acceptance-recorded`, `milestone.approved`, `milestone.ready-for-funding`, `milestone.submitted`, `milestone.changes-requested`, `milestone.accepted`, `milestone.auto-accepted`, `milestone.change-request-created`, `milestone.change-request-approved`, `milestone.change-request-rejected`, `milestone.change-request-cancelled`. Funding notifications (Payments mapper): `milestone.funding-started`, `milestone.funded`, `milestone.funding-failed`.
- Notification payload `data` always carries `milestoneId`, `contractId`, `proposalId`, `legalCaseId` (+ `changeRequestId` for change-request events).
- On any milestone notification, a 409 stale-version response, the first-party approve, or a payment/hold event, re-fetch `GET /api/contracts/{contractId}/milestones` (or the contract detail) **before** enabling a mutation, because approve/ready responses return no new `version`.
- Milestone lifecycle events also append system chat messages to the contract conversation asynchronously.

---

## 5. Gap Analysis & Missing Features Report

### 5.1 CRUD and lifecycle coverage

| Capability | Status | Evidence / impact |
|---|---|---|
| Create (Draft) | Implemented | Party-scoped, sequential `orderNumber`, one-milestone-at-a-time sync add, no bulk import/reorder. |
| Read list | Implemented (limited) | Flat array, no pagination, no filtering, no sorting choice, no single-milestone GET, no change-request list, no submissions/attachments read. |
| Read single milestone | Missing | Frontend must derive a single milestone from the list or contract detail. |
| Update Draft | Implemented | Lawyer/Client party, `If-Match`, resets acceptances. Amount and order immutable. |
| Update funded milestone | Partial | Only via extension change requests (description/duration/due-date); **no amount amendment** and no direct edit. |
| Approve terms | Implemented | Two-party, per-version, concurrency-full; on the second signature transitions to `AwaitingFunding`. |
| Ready / Fund / Submit / Accept / Revisions / Release | Implemented across slices | Sequence enforced; acceptance hold 14 days; auto-accept 7 days; release by background job. |
| **Client reject with revision** | **Partially implemented** | `request-changes` (`Submitted → FundedInProgress`) is the revision path, but there is **no explicit "reject" action or status**, no structured revision list, and no feedback attachment support. |
| Cancel / delete milestone | **Missing** | No per-milestone cancel/delete API or `Cancelled`-by-user action; only contract termination cancels milestones. Frontend cannot remove an erroneous Draft milestone except by contract-draft negotiation. |
| Dispute | Implemented elsewhere | Disputes slice transitions `AcceptedHold → Disputed`; not exposed in this slice. |
| State audit | **Missing** | `MilestoneStateHistory` rows are persisted for transitions, but **no endpoint returns them** (unlike Contract history). No correlation id (used internally) exposed. |

### 5.2 Specific requested checks

#### Reject / revise submitted work

- Revision exists (`request-changes`, Client-only) and returns the milestone to `FundedInProgress` for resubmission, storing `RejectionReason`.
- There is no negative review vocabulary (rejected/failed), no reject-with-files, no issue/screenshot cites, and no `If-Match` on accept/request-changes → a Client's direct `/accept` and `/request-changes` race is last-writer-wins rather than conflict-safe.

#### Partial payments / price amendments

- **Not supported.** `amount` is set once and immutable. Change requests can alter description, `durationDays`, and `dueDate` (extensions only) but **never `amount`**.
- No partial release, no splits, no renegotiation of price, no fee recompute event surfaced to the frontend beyond the existing escrow net amounts.

#### File attachments / deliverables

- Submission **accepts** pre-uploaded `storedFileIds` and persists `MilestoneSubmissionAttachment` rows, but:
  - No endpoint lists submissions, their versions, or their attachments.
  - No endpoint reads/downloads the deliverable set (file download presumably lives in the Files slice, but there is no milestone-scoped association API).
  - Change requests and revision requests accept **no file references**, so the review dialog cannot attach the revised deliverable set.
  - There is no size/type remediation surfaced in this slice.

#### List by Contract: pagination / sorting / filtering

- Listing exists and is ordered fixed by `orderNumber` ascending.
- **No pagination**, `page`/`pageSize`, status/due-date/amount filters, search, or client sorting. For many-milestone contracts this is effectively an unbounded array.
- No moderation/admin list variant distinct from the participant view.

#### Webhooks / notifications / polling

- In-app notifications (outbox + SignalR + REST) are implemented with the types and data map above.
- **No external/milestone-scoped outbound webhooks**, no long-poll/SSE for milestone state, and no event delivery status resource.
- No dedicated milestone state-history or change-request query endpoint for polling; polling falls back to re-fetching the full milestone list.

### 5.3 High-priority implementation inconsistencies and risks

| Priority | Finding | Frontend consequence | Recommended backend correction |
|---:|---|---|---|
| Critical | **Change-request `If-Match` rowversion is unobtainable:** create/decision endpoints return `MilestoneActionResultDto` (no version), and `MilestoneChangeRequestDto` is never returned and has no `version`/`rowversion` field. | `/approve`, `/reject`, `/cancel` require an `If-Match` no client can ever construct → the entire change-request decision workflow is unusable end-to-end. | Return a change-request DTO with a `version` (and ideally list endpoint) after create; include it in notify data. |
| Critical | No list/read endpoint for change requests. | Even if a `changeRequestId` is learned from a notification payload, the frontend cannot display pending/approved/rejected requests or their fields. | Add `GET /api/milestones/{id}/change-requests` (paged) returning `MilestoneChangeRequestDto`. |
| High | `MilestoneActionResultDto` returns no new milestone `version`. | After approve/ready, the counterparty's next mutation needs a fresh GET or it gets 409/412 on stale tokens. | Include refreshed `version` (or returning `MilestoneDto`) on these responses, plus real `ETag` headers. |
| High | **412 vs 409 split differs from Contracts slice.** | Don't port the Contract guide's “malformed If-Match → 400” assumption; Milestones returns 412 for format errors and 409 for stale values. | Document & standardize precondition semantics across slices (prefer 412 for *all* precondition mismatches). |
| High | `permittedActions` grants `Update`+`Approve` to both roles in Draft, and never lists change-request or cancel actions; the flag only reflects status/role, not prerequisites. | UI can render `Update` for a party whose Draft contract isn't editable, or miss offered amendment/decide actions. | Compute permitted actions from the same gate the services use; add structured per-action booleans (`canUpdate`, `canCreateChangeRequest`, `canDecide`, ...). |
| Medium | Accept/request-changes have **no concurrency token**. | Two clients racing accept vs request-changes silently pick a winner; UI may show a stale result. | Add `If-Match` (milestone version) to both, or a precondition on `SubmissionVersion`. |
| Medium | `AddMilestoneRequest` adheres to a strict sequential `orderNumber` but exposes it in the body. | Clients must compute `max+1` themselves; a stale value gets the 400 `ترتيب المرحلة الجديدة يجب أن يكون N.` rather than a clean append. | Make `orderNumber` optional/server-assigned, or return the expected next value from the list response. |
| Medium (race) | **Dead duplicate-guard catches:** `MilestoneDraftService.IsDuplicateOrderConstraintViolation` matches index `IX_Milestones_ContractId_OrderNumber` and `MilestoneChangeRequestService.IsDuplicatePendingRequestConstraintViolation` matches `IX_MilestoneChangeRequests_MilestoneId_Pending`, but the configured databases indexes are `UX_Milestones_ContractId_OrderNumber` and `UX_MilestoneChangeRequests_Pending`. | In a true simultaneous race the catch clause never matches, so the SQL exception escapes as a **500** instead of the intended 409; the deterministic pre-checks still return 400/409 in the normal path. | Align the string literals with the configured index names (or match on unique-constraint violation without index-name text), and add a race E2E test. |
| Medium | No per-milestone cancel or delete. | A miskeyed Draft milestone is stuck until contract termination; no way to remove/rename out of a locked flow. | Add Draft-scoped cancel/remove with `If-Match` and completion re-evaluation. |
| Medium | Change-request pending gate conflicts with auto-accept. | If a change request lingers past the 7-day window, auto-accept is skipped (`PendingMilestoneChangeRequestExists` no-op); frontend cannot see why. | Expose pending-request state via the list endpoint above. |

### 5.4 Production-readiness feature backlog

1. **Change-request visibility & versioning** — list/read endpoint, returned rowversion, decision `If-Match` usable, requester/decider context, notification round-trip.
2. **Revision workflow** — first-class `Rejected`/`RevisionRequested` state, revision history per submission version, structured feedback with file attachments.
3. **Amount/price amendments** — milestone amount change through a guarded amendment object (partial payments, price reduction in return-for-refund, fee recompute).
4. **Milestone archive/cancel/delete** — Draft-scoped removal, cancellation with reason, state history coverage.
5. **Reading layer** — paged/filtered/sorted list, single-milestone detail, milestone state-history endpoint (rows already exist), submission/attachment listing.
6. **Concurrency UX** — real `ETag` response headers, version on action results, `If-Match` on accept/request-changes, `412` for every precondition mismatch, refreshed representation on conflict.
7. **Event integration** — milestone outbound webhooks (signed, retryable) and a queryable outbox/delivery status, plus polling-friendly `updatedAt` exposure.
8. **Consistency** — single source for permitted actions, stable numeric/string enum encoding policy shared with the Contract slice, machine-readable error codes.
9. **Auto-accept transparency** — expose `autoAcceptEligibleAt`, pending-change-request blocker, and job status so the UI can explain a skipped auto-accept.

---

## Frontend implementation checklist

- Model `MilestoneStatus` 0–9 and `FundingStatus` 0–3 as numeric enums; model `MilestoneActionResultDto.status` as a string; keep the two models separate.
- Always escape `title`, `description`, notes, and all `reason` texts when rendering.
- Fetch the milestone list (or contract detail) immediately before Update/Approve/Ready/CreateChangeRequest and send `data.version` verbatim (with quotes) as `If-Match`.
- Handle **both** stale-token `409` and malformed-token `412`; after either, re-fetch before retrying.
- After first-party approve/ready, re-fetch the list because the response carries no new `version`.
- Treat `request-changes` and `accept` as last-writer-wins today; minimize concurrent calls and re-fetch after either.
- Gate controls by the user's actual role+identity and contract participation, not only `permittedActions`; remember the change-request decision/cancel endpoints are currently unusable due to the missing rowversion.
- Mirror exact length/range constraints; still display server validation for domain gates (status, sequential ordering, funding verification, ownership checks, extension-only rules).
- Subscribe to milestone notifications and re-fetch on every lifecycle/payment event; use notification payload ids (`milestoneId`, `contractId`, etc.) to deep-link.
- Do not submit or calculate `status`, `fundingStatus`, `amount` changes, acceptance flags, escrow ids, or `version`.
- Preserve both error parsers (`ApiResponse` and ValidationProblemDetails), plus graceful handling of empty framework 401/403/404 responses.
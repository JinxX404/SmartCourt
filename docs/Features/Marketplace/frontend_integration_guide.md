# Lawyer Marketplace — Frontend Integration Guide

> Authoritative contract for the **implemented** marketplace slice: `api/lawyers`.
> The design doc `docs/Plans/08_API_Marketplace.md` describes `/api/marketplace/*` routes — these are **NOT** implemented. Use the routes below.

## Base URL & Conventions

- Response envelope is `ApiResponse<T>` (below). All dates serialize as `yyyy-MM-dd` (`DateOnly`) or ISO-8601 (`DateTime` / `DateTimeOffset`).
- JSON property naming: **camelCase** (ASP.NET Core default in this codebase, confirms the middleware + response serializers).
- Enums serialize as **integer numbers** (default `JsonStringEnumConverter` is **not** registered for these slices — verified; fields like `Level` come back as `2`, `Specialization` as `4`, etc.).
- `Guid` values are lowercase standard UUID strings (`"3f2c..."`).
- Arabic is the platform language — `Message`, `Errors`, and `RejectionReason` are Arabic strings.

---

## 1. Response & Error Envelope

### 1.1 `ApiResponse<T>` (success & error shape)

```csharp
{
  bool   Success;
  T?     Data;
  string? Message;
  string[]? Errors;     // null on success
  int    StatusCode;
}
```

### 1.2 `PagedResponse<T>` (extends `ApiResponse<T>` — returned by `/search`)

```csharp
{
  bool Success;
  T?    Data;
  string? Message;
  string[]? Errors;
  int   StatusCode;

  int PageNumber;
  int PageSize;
  int TotalPages;
  int TotalRecords;
  bool HasNextPage;     // PageNumber < TotalPages
  bool HasPreviousPage; // PageNumber > 1
}
```

### 1.3 HTTP status → envelope mapping (from `ExceptionHandlingMiddleware`)

| HTTP | Body shape | Trigger |
|---|---|---|
| `200` | `{ Success:true, Data:..., StatusCode:200 }` | regular `Ok(...)` |
| `400` | `{ Success:false, Message, Errors:[...], StatusCode:400 }` | `ValidationException` / `BusinessException` |
| `401` | `{ Success:false, Message, StatusCode:401 }` | `AuthenticationException` / missing/invalid token |
| `403` | `{ Success:false, Message, StatusCode:403 }` | `ForbiddenAccessException` (wrong role) |
| `404` | `{ Success:false, Message, StatusCode:404 }` | `NotFoundException` (e.g. lawyer not found) |
| `409` | `{ Success:false, Message, StatusCode:409 }` | `ConflictException` |
| `412` | `{ Success:false, ... StatusCode:412 }` | `PreconditionFailedException` (If-Match, contracts) |
| `413` | `{ Success:false, ... StatusCode:413 }` | `PayloadTooLargeException` |
| `429` | `{ Success:false, Message, StatusCode:429 }` | `TooManyRequestsException` — see rate limits below |
| `500` | `{ Success:false, Message:"An internal server error occurred." StatusCode:500 }` | unhandled |

- On `400` from validators, `Errors` items look like `"FieldName: message"` (e.g. `"MinRating: يجب أن يكون الحد الأدنى للتقييم بين 0 و 5."`).
- On `400` from business rules, `Errors` is null and `Message` holds the Arabic reason.
- `401` for a missing/malformed token comes from ASP.NET auth machinery — **not** necessarily this envelope.

### 1.4 Rate limits (`SecurityRateLimitPolicies`)

| Policy | Applies to | IP bucket | User bucket |
|---|---|---|---|
| `PublicLawyerGet` | `GET /search`, `GET /public/{id}` | 120 / minute | — |
| `PrivateProfileGet` | `GET /profile` | 300 / minute | 120 / minute |
| `PrivateProfileUpdate` | `POST /profile/complete`, `PUT /profile` | 60 / 15 min | 20 / 15 min |
| `PrivateProfileDelete` | `DELETE /profile` | 10 / day | 3 / day |

- On exceeding: HTTP `429` with `Message = "لقد تجاوزت الحد المسموح من الطلبات. يرجى المحاولة مرة أخرى لاحقًا."`
- Respect `Retry-After` if present; otherwise back off 1 minute (browse) / 15 minutes (update).

---

## 2. Endpoints

### 2.1 `GET /api/lawyers/search` — Browse / Search the marketplace

- **Auth:** `[Authorize]` — any authenticated user (Client or Lawyer). Anonymous → `401`.
- **Rate limit:** `PublicLawyerGet` (120/min per IP).
- **Query string only** — DTO `SearchLawyersRequest : PagedRequest`.

#### Query parameters (full contract)

| Param | Type | Required | Default | Rules / behavior |
|---|---|---|---|---|
| `PageNumber` | int | no | `1` | `>= 1` (validator: `"رقم الصفحة يجب أن يكون 1 على الأقل."`) |
| `PageSize` | int | no | `10` | `1..50`. Values `> 50` are **silently capped to 50** in `PagedRequest` (not a validation error). Validator flags `<1` or `>50` as `"حجم الصفحة يجب أن يكون بين 1 و 50."` |
| `SearchTerm` | string? | no | — | case-insensitive substring match on `FullName` **or** `LawyerProfile.Bio` |
| `Governorate` | string? | no | — | **exact equality** against user `Governorate` (free-text, not an enum). Empty/whitespace ignored. |
| `Level` | int? (`LawyerLevel`) | no | — | must be valid enum value (1–4); else `"مستوى المحامي غير صالح."` |
| `Specialization` | int? (`Specialization`) | no | — | must be valid enum value (0–20); matches any of the lawyer's `Specializations` (filters on `Specializations.Any(...)`) |
| `MinRating` | decimal? | no | — | validator `0m..5m` inclusive, `"يجب أن يكون الحد الأدنى للتقييم بين 0 و 5."`. Numeric `>5` → validation 400. |
| `IsAvailable` | bool? | no | — | exact boolean; `true` filters `LawyerProfile.IsAvailable == true` |
| `SortBy` | int (`LawyerSortBy`) | no | `0` (Rating) | must be valid (0–2) else `"خيار الترتيب غير صالح."` |
| `SortDirection` | int (`SortDirection`) | no | `1` (Descending) | must be valid (0–1) else `"اتجاه الترتيب غير صالح."` |

#### Query semantics (server-side behavior you must match)

1. Base filter applied **always**: `LawyerProfile != null && EmailConfirmed && Status == Active`.
   Only **approved & verified lawyers** ever appear in the marketplace.
2. Filters compose with **AND**.
3. Sort matrix (note the **default-for-direction** nuances):

| SortBy | Ascending | Descending |
|---|---|---|
| `Rating` (0) | `AverageRating` asc | `AverageRating` desc |
| `ResponseTime` (1) | `AverageResponseTimeHours` asc | `AverageResponseTimeHours` desc |
| `ExperienceLevel` (2) | `Level` asc (1→4) | `Level` desc (4→1) |
| any other / fallback | — | `AverageRating` desc |

4. **Order of application matters:** `Count` is taken *after* filters (this is `TotalRecords`), then `Skip((PageNumber-1)*PageSize).Take(PageSize)`.

#### Response — `200 OK`, `PagedResponse<List<PublicLawyerProfileResponse>>`

```json
{
  "success": true,
  "data": [ { "id": "…", "name": "…", "gender": 1, "level": 3, "bio": "…",
              "isAvailable": true, "profilePictureUrl": "…" } ],
  "message": null,
  "errors": null,
  "statusCode": 200,
  "pageNumber": 1, "pageSize": 10, "totalPages": 3, "totalRecords": 25,
  "hasNextPage": true, "hasPreviousPage": false
}
```

> **CRITICAL list-vs-detail difference:** the **list item** projection (see `SearchLawyersAsync` `Select(...)`) sets **only**:
> `id`, `name`, `gender`, `level`, `bio`, `isAvailable`, `profilePictureUrl`.
> All other members are **NOT populated in list responses**:
> `governorate` → `null`, `city` → `null`, `yearsOfExperience` → `0`, `specializationName` → `null`, `specializations` → `[]`.
> You must fetch `GET /api/lawyers/public/{id}` to display real governorate/city/years/specializations.
> Cards must not render empty `governorate/city` placeholders from list data.

---

### 2.2 `GET /api/lawyers/public/{id:guid}` — Full public profile

- **Auth:** `[AllowAnonymous]` — public.
- **Rate limit:** `PublicLawyerGet` (120/min per IP).
- **Path:** lawyer user `Guid` (lowercase UUID).

#### Behavior

- Returns `ApiResponse<PublicLawyerProfileResponse>` (wrapped).
- **404** `"المحامي غير موجود"` when the user does not exist **OR** is not a marketplace-visible lawyer (`LawyerProfile != null && EmailConfirmed && Status == Active`). So a suspended / pending / rejected / non-lawyer `id` is indistinguishable from a non-existent one — do not treat 404 as "deleted", just "not browsable".

#### Response — `200 OK`

```json
{
  "success": true,
  "data": {
    "id": "…", "name": "…", "gender": 1, "level": 4,
    "bio": "…", "governorate": "…", "city": "…",
    "isAvailable": true, "profilePictureUrl": "…",
    "yearsOfExperience": 12, "specializationName": "Contracts",
    "specializations": [
      { "specialization": 10, "yearsOfExperience": 12, "casesHandled": 34 }
    ]
  },
  "message": null, "errors": null, "statusCode": 200
}
```

- Here `governorate`, `city`, `yearsOfExperience`, `specializationName`, `specializations` **are** populated.
- `specializationName` = `ToString()` of the **first** specialization entry (English enum name), or `null` if the lawyer has zero specializations.
- `yearsOfExperience` = `yearsOfExperience` of the **first** specialization entry, or `0`.
- `github`-style derivation rule you should replicate in UI fallbacks: primary specialization drives the badge and the displayed experience.

---

### 2.3 `GET /api/lawyers/profile` — Lawyer's own profile (private)

- **Auth:** `[Authorize(Roles = "Lawyer")]` — Client role → `403`.
- **Rate limit:** `PrivateProfileGet` (300/min IP, 120/min user).
- **Response:** `ApiResponse<LawyerProfileResponse>`.

```json
{
  "success": true,
  "data": {
    "id": "…", "name": "…", "email": "…", "phoneNumber": "…",
    "nationalNumber": "…", "gender": 1, "dateOfBirth": "1990-05-12",
    "level": 2, "yearsOfExperience": 7, "specializationName": "CriminalLaw",
    "bio": "…", "address": "…", "governorate": "…", "city": "…",
    "status": "PendingReview", "isAvailable": true,
    "profilePictureUrl": "…", "rejectionReason": null,
    "specializations": [ { "specialization": 4, "yearsOfExperience": 7, "casesHandled": 91 } ]
  },
  "message": null, "errors": null, "statusCode": 200
}
```

#### Fields unique to this DTO

| Field | Type | Notes |
|---|---|---|
| `email` | string | always present (falls back to `""`) |
| `phoneNumber` | string | `""` if not set |
| `nationalNumber` | string | **PII — keep out of logs / XHR payloads after use** |
| `dateOfBirth` | `date`? | `DateOnly`, `null` until completed |
| `address` | string? | not exposed publicly |
| `status` | **string** — NOT int | one of `UserStatus` **names**: `Unverified`, `PendingReview`, `Active`, `Suspended`, `Rejected`, `Deleted` |
| `rejectionReason` | string? | non-null only when `status == Rejected`; use to render the rejection banner re-edit UI |
| `level` | int `LawyerLevel` | defaults to `1` (GeneralRegistration) when no profile row exists |

- Note: `gender` here comes from `ApplicationUser` and serializes as int (`0` Male / `1` Female).

---

### 2.4 `POST /api/lawyers/profile/complete` — Complete lawyer onboarding

- **Auth:** `[Authorize(Roles = "Lawyer")]`.
- **Rate limit:** `PrivateProfileUpdate` (60/15 min IP, 20/15 min user).
- **Response `200 OK`:** `{ "success": true, "message": "تم استكمال البيانات بنجاح", "statusCode": 200 }` (no `data`).
- **Business rule:** if `user.Status == UserStatus.Active` → `400` `"تم استكمال الملف الشخصي مسبقاً."`

#### Request body — `CompleteLawyerProfileRequest` (all body fields)

| Field | Type | Required | Rule / Arabic error |
|---|---|---|---|
| `phoneNumber` | string | **yes** | regex `^\+20\d{10}$` → `"رقم الهاتف يجب أن يكون بالتنسيق المصري +20XXXXXXXXXX"` |
| `nationalNumber` | string | **yes** | exactly 14 chars → `"الرقم القومي يجب أن يتكون من 14 رقم."` |
| `gender` | int? | **yes** | not null → `"الجنس مطلوب."`; enum 0/1 → `"الجنس يجب أن يكون صالحاً."` |
| `dateOfBirth` | `date` (`DateOnly`) | **yes** | must be in the past → `"يجب أن يكون تاريخ الميلاد في الماضي."` (validation compares against `DateTime.Today`) |
| `level` | int (`LawyerLevel`) | **yes** | enum 1–4 → `"مستوى المحامي غير صالح."` |
| `bio` | string? | no | ≤ 500 chars → `"يجب ألا تتجاوز السيرة الذاتية 500 حرف."` |
| `address` | string? | no | ≤ 255 → `"يجب ألا يتجاوز العنوان 255 حرف."` |
| `governorate` | string? | no | ≤ 100 → `"يجب ألا تتجاوز المحافظة 100 حرف."` |
| `city` | string? | no | ≤ 100 → `"يجب ألا تتجاوز المدينة 100 حرف."` |
| `specializations` | array | **yes** (non-empty) | `"يجب إدخال تخصص واحد على الأقل."` |

Nested `specializations[]` items (`LawyerSpecializationDto`):
| Field | Type | Rule |
|---|---|---|
| `specialization` | int (0–20) | valid enum → `"التخصص غير صالح."` |
| `yearsOfExperience` | int | ≥ 0 → `"سنوات الخبرة يجب أن تكون 0 أو أكثر."` |
| `casesHandled` | int | ≥ 0 → `"عدد القضايا المنجزة يجب أن يكون 0 أو أكثر."` |

#### Server side-effects to reflect in UI

- Sets `Status = PendingReview` → the lawyer **disappears from marketplace search** until verified & set to `Active` by an admin.
- Creates a `LawyerProfile` row with `IsAvailable = true` (default).
- Phone changed via identity manager (a duplicate/invalid phone can produce `400` with identity errors joined as `Message`).
- Specializations are **replaced in bulk**: existing rows are removed, then the provided list is inserted. Re-submission fully replaces them.
- The entire operation runs in a transaction — any failure rolls back everything.

---

### 2.5 `PUT /api/lawyers/profile` — Update lawyer profile

- **Auth:** `[Authorize(Roles = "Lawyer")]`.
- **Rate limit:** `PrivateProfileUpdate` (60/15 min IP, 20/15 min user).
- **Response `200 OK`:** `{ "success": true, "message": "تم تحديث البيانات بنجاح", "statusCode": 200 }`.

#### Request body — `UpdateLawyerProfileRequest`

All fields optional in the DTO, but validators enforce rules **when present**:

| Field | Type | Rule / error |
|---|---|---|
| `dateOfBirth` | `date`? | (no validator rule on update) |
| `gender` | int? | only written if `HasValue` |
| `level` | int (`LawyerLevel`) | enum check in both FluentValidator and `ValidateProfileRequest` → `"مستوى المحامي غير صالح."` |
| `yearsOfExperience` | int | **accepted but design bug:** not persisted anywhere by `UpdateProfileAsync` (only reflected via the first `Specialization.YearsOfExperience`). Frontend should read/write experience through `specializations[0].yearsOfExperience`. |
| `specializationId` | Guid? | **accepted but ignored** by `UpdateProfileAsync` (no DB write). Retained for API compatibility — do not rely on it. |
| `bio` | string? | ≤ 500 |
| `address` | string? | ≤ 255 |
| `nationalNumber` | string? | required + regex `^\d{14}$` → `"الرقم القومي يجب أن يتكون من 14 رقم بالضبط."` (becomes mandatory whenever the field is sent!) |
| `governorate` | string? | ≤ 100 |
| `city` | string? | ≤ 100 |
| `specializations` | array? | if present, each item validated like `complete`; **also** duplicate `specialization` values rejected → `"لا يمكن تكرار نفس التخصص للمحامي."` |

> **Watch-out:** `nationalNumber` is `string?` in the request but the validator requires `.NotEmpty()` + 14 digits. So if you send the body with `nationalNumber: ""` → validation `400`. Only send it when you have a valid value.

#### Server side-effects

- Tracks which fields changed into `ModifiedFieldsJson` (audit list: `Address`, `DateOfBirth`, `Gender`, `NationalNumber`, `Governorate`, `City`, `Level`, `Bio`, `Specializations`).
- Sets `Status = PendingReview` **only when current status is `Active` or `Rejected`**. If the lawyer is `PendingReview`/`Suspended`/`Unverified`, status is left as-is.
  - So: an inspection re-edit after rejection returns the lawyer to review; a cosmetic edit while `PendingReview` does not reset anything.
- If `specializations` is **omitted** (`null`), existing specializations are left untouched. If an **empty array** is sent, all existing specializations are deleted.
- `Level`/`Bio` are `== null` guarded — passing `null` values is allowed but means "leave as-is" for specializations only; `Level` will still be written from the request (defaults to `0` if omitted → enum 0 is **invalid** for `LawyerLevel` whose values start at 1 → expect `400`).

---

### 2.6 `DELETE /api/lawyers/profile` — Delete account (hard delete)

- **Auth:** `[Authorize(Roles = "Lawyer")]`.
- **Rate limit:** `PrivateProfileDelete` (10/day IP, 3/day user) — heavy, frontend gate with confirmations.
- **Request body:** `{ "currentPassword": "…" }` (`DeleteAccountRequest`, `record`).

| Field | Type | Rule |
|---|---|---|
| `currentPassword` | string | required → `"كلمة المرور الحالية مطلوبة."` |

#### Behavior & failure cases

- Wrong password → `400` `"كلمة المرور الحالية غير صحيحة."`
- Active/linked contracts (`Contracts.Any(c => c.LawyerUserId == id)`) → `400` `"لا يمكن حذف الحساب لوجود قضايا وعقود مرتبطة به."` (blocking rule; message implies cases too).
- Already `Deleted` **or** user not found → **silently succeeds** with `200` (idempotent).
- Success response: `{ "success": true, "message": "تم حذف الحساب بنجاح", "statusCode": 200 }`.
- Side-effects: deletes verification documents + their storage blobs, profile picture blob, specializations, `LawyerProfile`, revokes all refresh tokens, then deletes the `ApplicationUser`. **Irreversible** — hard-delete, not soft.

---

## 3. DTO Reference (field-by-field)

### 3.1 `PublicLawyerProfileResponse` — the marketplace product

| Field | Type | List (`/search`) | Detail (`/public/{id}`) | Notes |
|---|---|---|---|---|
| `id` | Guid | ✅ | ✅ | lawyer user id |
| `name` | string | ✅ | ✅ | from `FullName` |
| `gender` | int? | ✅ | ✅ | `0` Male, `1` Female; `null` if unset |
| `level` | int | ✅ | ✅ | `LawyerLevel` 1–4 |
| `bio` | string? | ✅ | ✅ | may be `null` |
| `governorate` | string? | ❌ **null** | ✅ | free-text, e.g. `"Cairo"` |
| `city` | string? | ❌ **null** | ✅ | |
| `isAvailable` | bool | ✅ | ✅ | |
| `profilePictureUrl` | string? | ✅ | ✅ | may be `null` → render initial avatar |
| `yearsOfExperience` | int | ❌ **0** | ✅ | = first specialization's value |
| `specializationName` | string? | ❌ **null** | ✅ | = first specialization's `Specialization.ToString()` (English enum name) |
| `specializations` | array | ❌ **[]** | ✅ | full list |

### 3.2 `LawyerSpecializationDto`

| Field | Type | Notes |
|---|---|---|
| `specialization` | int | `Specialization` enum 0–20 |
| `yearsOfExperience` | int | per-specialization experience |
| `casesHandled` | int | per-specialization closed-cases count |

### 3.3 `LawyerProfileResponse` (private) — see §2.3 for the full field table.

### 3.4 `CompleteLawyerProfileRequest` / `UpdateLawyerProfileRequest` — see §2.4 / §2.5.

### 3.5 `DeleteAccountRequest`

```csharp
record DeleteAccountRequest(string CurrentPassword);
```

---

## 4. Enums & States (authoritative values)

### 4.1 `LawyerLevel` (1–4) — court-tier right-of-audience

| Value | Name | Arabic (Bar table) |
|---|---|---|
| `1` | `GeneralRegistration` | محامي جدول عام |
| `2` | `PrimaryCourt` | محامي ابتدائي |
| `3` | `AppealCourt` | محامي استئناف |
| `4` | `CassationCourt` | محامي نقض |

Defined `: byte`. **No `0`** — an omitted/zero `level` in a request is invalid.

### 4.2 `Specialization` (0–20)

| Value | Name | | Value | Name |
|---|---|---|---|---|
| 0 | `FamilyLaw` | | 11 | `IntellectualProperty` |
| 1 | `CivilLaw` | | 12 | `Arbitration` |
| 2 | `CommercialLaw` | | 13 | `BankingAndFinance` |
| 3 | `AdministrativeAndStateCouncilLaw` | | 14 | `Investment` |
| 4 | `CriminalLaw` | | 15 | `RealEstateAndPropertyRegistration` |
| 5 | `LaborLaw` | | 16 | `Execution` |
| 6 | `ConstitutionalLaw` | | 17 | `Insurance` |
| 7 | `TaxLaw` | | 18 | `Environment` |
| 8 | `CustomsLaw` | | 19 | `InformationTechnologyAndTelecommunications` |
| 9 | `CorporateLaw` | | 20 | `Cybercrimes` |
| 10 | `Contracts` | | | |

### 4.3 `LawyerSortBy` (0–2)

| Value | Name | Meaning |
|---|---|---|
| `0` | `Rating` | by `LawyerProfile.AverageRating` |
| `1` | `ResponseTime` | by `LawyerProfile.AverageResponseTimeHours` (lower = faster) |
| `2` | `ExperienceLevel` | by `LawyerLevel` tier |

**Session-default to `0` + `Descending`.** For `ResponseTime` consider defaulting direction to `Ascending` server-side — the API default is `Descending`, which ranks the *slowest* lawyers first unless the client explicitly passes `sortDirection=0`.

### 4.4 `SortDirection` (0–1)

| Value | Name |
|---|---|
| `0` | `Ascending` |
| `1` | `Descending` |

### 4.5 `Gender` (0–1)

| Value | Name |
|---|---|
| `0` | `Male` |
| `1` | `Female` |

### 4.6 `UserStatus` (lawyer approval state) — int with `0`-based default

> Serialized as **int** on user records, but the private `/profile` DTO returns it as a **string name**.

| Int | Name | Marketplace visibility | Notes |
|---|---|---|---|
| `0` | `Unverified` | ❌ hidden | registered, email maybe unconfirmed |
| `1` | `PendingReview` | ❌ hidden | profile complete → awaiting admin verification |
| `2` | `Active` | ✅ visible | only visible state; also requires `EmailConfirmed` |
| `3` | `Suspended` | ❌ hidden | |
| `4` | `Rejected` | ❌ hidden | `rejectionReason` populated on private profile |
| `5` | `Deleted` | ❌ hidden | |

`/search` & `/public/{id}` only ever return rows where `Status == Active`.

---

## 5. Frontend state/UI mapping reference

### Marketplace "lawyer availability" truth

- `isAvailable` is a **stored toggle on the profile** — it is NOT invented at query time. Respect `profileComplete → status` signals, but the marketplace card availability comes solely from `isAvailable`.

### Lifecycle a Client will observe

| Volunteer state | Lawyer visible in `/search`? | Public `/public/{id}`? |
|---|---|---|
| `Unverified` / `PendingReview` / `Suspended` / `Rejected` / `Deleted` | no | 404 |
| `Active` + `EmailConfirmed` | yes | 200 |

### Sorting UI guidance

- Standard sort dropdown values must use `sortBy` 0/1/2.
- `sortDirection` must always be sent explicitly if the user toggles asc/desc, because the API default (`Descending` for `Rating` and `ExperienceLevel`) is what you'll get otherwise.

---

## 6. Every validation / business "case" (client-side mirror checklist)

`POST /profile/complete`
- [x] Phone must match `+20` + 10 digits.
- [x] National number exactly 14 digits.
- [x] Gender required.
- [x] DateOfBirth must be before today.
- [x] Level 1–4.
- [x] Bio ≤ 500, Address ≤ 255, Governorate/City ≤ 100.
- [x] ≥ 1 specialization; each: enum 0–20, exp ≥ 0, cases ≥ 0.
- [x] If already `Active` → 400 "already completed".

`PUT /profile`
- [x] National number (if sent) exactly 14 digits.
- [x] Level 1–4.
- [x] No duplicate specializations (same enum twice).
- [x] Field lengths as above.
- [x] `level` omitted/zero → invalid enum → 400.
- [x] Empty `specializations: []` deletes all specializations (updater path applies removal, but note: when `[]` is an empty but non-null list the dedupe rule still passes; all existing specs removed).

`GET /search`
- [x] `PageSize` > 50 → silently capped to 50 (no error).
- [x] `PageNumber` < 1 → 400.
- [x] `MinRating` out of 0..5 → 400.
- [x] Invalid `Level` / `Specialization` / `SortBy` / `SortDirection` → 400 (each isolated field error).

`GET /public/{id}`
- [x] Non-Guid → 404 route (not matched).
- [x] Valid Guid, but lawyer not Active/confirmed → 404 (same body as missing).

`DELETE /profile`
- [x] Wrong password → 400.
- [x] Has contracts → 400 (blocked).
- [x] Already deleted → 200 idempotent.

---

## 7. Source of truth (files)

| Concern | File |
|---|---|
| Controller & routes | `SmartCourt/Features/Users/Lawyers/LawyersController.cs` |
| Service implementation | `SmartCourt/Features/Users/Lawyers/LawyerService.cs` |
| Service interface | `SmartCourt/Features/Users/Lawyers/ILawyerService.cs` |
| Search DTO + PagedRequest | `…/DTOs/SearchLawyersRequest.cs`, `Common/Models/PagedRequest.cs` |
| Response DTOs | `…/DTOs/PublicLawyerProfileResponse.cs`, `LawyerProfileResponse.cs`, `LawyerSpecializationDto.cs` |
| Mutations DTOs | `…/DTOs/CompleteLawyerProfileRequest.cs`, `UpdateLawyerProfileRequest.cs` |
| Validators | `…/Validators/SearchLawyersRequestValidator.cs`, `CompleteLawyerProfileRequestValidator.cs`, `UpdateLawyerProfileRequestValidator.cs`, `Users/Shared/Validators/DeleteAccountRequestValidator.cs` |
| Enums | `Common/Enums/LawyerLevel.cs`, `Specialization.cs`, `LawyerSortBy.cs`, `SortDirection.cs`, `Features/Auth/Enums/UserStatus.cs`, `Gender.cs` |
| Response envelope | `Common/Models/ApiResponse.cs`, `PagedResponse.cs`, `Middleware/ExceptionHandlingMiddleware.cs` |
| Rate limits | `Common/RateLimiting/SecurityRateLimitPolicy.cs` |
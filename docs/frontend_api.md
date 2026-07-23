# SmartCourt Frontend API Reference

This document is traced from the controller, request/response DTO, validator, and service/handler implementations currently under `SmartCourt/Features`. It describes the 25 routes exposed by those controllers. No route, field, or validation rule is inferred from planned features or older OpenAPI files.

## Wire conventions

- ASP.NET Core web JSON defaults serialize C# property names as camelCase. Exception middleware also explicitly uses camelCase.
- `ApiResponse<T>` is serialized as:

  ```json
  {
    "success": true,
    "data": {},
    "message": null,
    "errors": null,
    "statusCode": 200
  }
  ```

- `ApiResponse` (without a data payload) is serialized as `success`, `message`, `errors`, and `statusCode`.
- `PagedResponse<T>` contains the `ApiResponse<T>` fields plus `pageNumber`, `pageSize`, `totalPages`, `totalRecords`, `hasNextPage`, and `hasPreviousPage`.
- No `JsonStringEnumConverter` is registered. Enum request/response values therefore use their numeric values unless a field is explicitly a `string`.
- `DateOnly` values use the JSON date form `YYYY-MM-DD`; `DateTime` values are ISO-8601 timestamps.
- FluentValidation failures are HTTP 400. The global exception middleware maps `AuthenticationException` to 401, `BusinessException` to 400, `NotFoundException` to 404, `ForbiddenAccessException` to 403, and `TooManyRequestsException` to 429. `ConflictException` is not handled by the middleware and currently falls through to HTTP 500.
- Rate-limit rejection uses `ApiResponse<string>` with message `"Too many requests. Please try again later."` and status 429.
- For `[Authorize]` routes, send `Authorization: Bearer <access-token>`. Role-restricted routes additionally require the role stated below. Routes marked anonymous do not require this header.

## Auth

### Login — `POST /api/auth/login`

**Description:** Authenticates a user and issues an access token plus a seven-day refresh token.

**Authentication:** Anonymous. `Content-Type: application/json`.

**Request body (`LoginRequest`):**

| Field | C# type | Required/validation |
|---|---|---|
| `email` | `string` | Required; must be a valid email (`البريد الإلكتروني مطلوب.` / `البريد الإلكتروني غير صالح.`). |
| `password` | `string` | Required; minimum 8 characters (`كلمة المرور مطلوبة.` / `كلمة المرور يجب أن تكون 8 أحرف على الأقل.`). |

**Success:** HTTP 200, `ApiResponse<LoginResponse>`.

```json
{
  "success": true,
  "data": {
    "user": {
      "id": "0f2c5a4d-7a3e-4b7a-8d13-2ef7df7d54f2",
      "email": "client@example.com",
      "fullName": "Example Client",
      "role": "Client"
    },
    "accessToken": "eyJhbGciOi...",
    "expiresIn": 3600,
    "refreshToken": "base64-refresh-token",
    "refreshTokenExpiration": "2026-07-30T12:00:00Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```

**Exceptions and errors:**

- Invalid email/password, or a deleted account: `AuthenticationException("البريد الإلكتروني أو كلمة المرور غير صحيحة.")` → 401.
- Email not confirmed: `ForbiddenAccessException("يرجى تأكيد البريد الإلكتروني أولاً")` → 403.
- Suspended account: `ForbiddenAccessException("تم تعليق حسابك. تواصل مع الدعم")` → 403.
- Validation failures return 400.

### Register client — `POST /api/auth/register/client`

**Description:** Creates an unverified client account and queues a confirmation email.

**Authentication:** Anonymous. `Content-Type: application/json`.

**Request body (`RegisterClientRequest`):**

| Field | C# type | Required/validation |
|---|---|---|
| `fullName` | `string` | Required; 5–150 characters (`الاسم الكامل مطلوب.` / minimum / maximum messages). |
| `email` | `string` | Required; valid email. |
| `password` | `string` | Required; at least 8 characters and must match `^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$`. |
| `confirmPassword` | `string` | Must equal `password` (`تأكيد كلمة المرور غير مطابق.`). The service repeats the comparison and raises `ValidationException("ConfirmPassword", "كلمة المرور وتأكيد كلمة المرور غير متطابقتين.")`. |

**Success:** HTTP 201, `ApiResponse<RegisterResponse>` with message `"تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني"`.

```json
{
  "success": true,
  "data": {
    "userId": "0f2c5a4d-7a3e-4b7a-8d13-2ef7df7d54f2",
    "email": "client@example.com",
    "fullName": "Example Client",
    "role": "Client"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
```

**Exceptions and errors:**

- Existing email: `ConflictException("البريد الإلكتروني مسجل بالفعل.")`. `ConflictException` is not mapped by the current middleware, so the observed response is HTTP 500 with the generic internal-error envelope.
- Identity creation failures are returned as a `ValidationException` keyed by each identity error code → 400.
- Confirmation-email queue failure from `AuthHelperService`: `InvalidOperationException("Confirmation email could not be queued.")` → generic 500.
- Validation failures return 400.

### Register lawyer — `POST /api/auth/register/lawyer`

**Description:** Creates an unverified lawyer account from a multipart form and queues a confirmation email.

**Authentication:** Anonymous. `Content-Type: multipart/form-data`.

**Request body (`RegisterLawyerRequest`, form fields):**

| Field | C# type | Required/validation |
|---|---|---|
| `fullName` | `string` | Required; 5–150 characters. |
| `email` | `string` | Required; valid email. |
| `password` | `string` | Required; at least 8 characters and must match `^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$`. |
| `confirmPassword` | `string` | Must equal `password`; service also checks and raises the exact mismatch message above. |
| `phone` | `string` | Required; must match `^\+20\d{10}$`. |
| `address` | `string` | Required; maximum 500 characters. |
| `government` | `string` | Required; maximum 100 characters. |
| `city` | `string` | Required; maximum 100 characters. |
| `gender` | `string` | Required; only `Male` or `Female`. |
| `nationalNumber` | `string` | Required; exactly 14 characters and `^[0-9]{14}$`. |
| `nationalIdFront` | `IFormFile` | DTO field; no FluentValidation rule and not read by `RegisterLawyerService`. |
| `nationalIdBack` | `IFormFile` | DTO field; no FluentValidation rule and not read by `RegisterLawyerService`. |
| `syndicateCard` | `IFormFile` | DTO field; no FluentValidation rule and not read by `RegisterLawyerService`. |
| `personalPhoto` | `IFormFile` | DTO field; no FluentValidation rule and not read by `RegisterLawyerService`. |

**Success:** HTTP 201, `ApiResponse<RegisterResponse>` with message `"تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني"`.

```json
{
  "success": true,
  "data": {
    "userId": "0f2c5a4d-7a3e-4b7a-8d13-2ef7df7d54f2",
    "email": "lawyer@example.com",
    "fullName": "Example Lawyer",
    "role": "Lawyer"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
```

**Exceptions and errors:**

- Existing email: `ConflictException("البريد الإلكتروني مسجل بالفعل.")` → currently generic 500 as described above.
- Existing national number: `ConflictException("الرقم القومي مسجل بالفعل.")` → currently generic 500.
- Identity creation failures: `ValidationException` containing identity error descriptions → 400.
- Confirmation-email queue failure: `InvalidOperationException("Confirmation email could not be queued.")` → generic 500.
- Validation failures return 400. The four file fields have no validator rules in the current implementation.

### Refresh access token — `POST /api/auth/refresh`

**Description:** Rotates an active refresh token and returns a new access/refresh pair.

**Authentication:** No controller authorization attribute. `Content-Type: application/json`.

**Request body (`RefreshTokenRequest`):**

| Field | C# type | Required/validation |
|---|---|---|
| `refreshToken` | `string` | Required (`رمز التحديث مطلوب.`). |

**Success:** HTTP 200, `ApiResponse<RefreshTokenResponse>`.

```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOi...",
    "refreshToken": "new-base64-refresh-token",
    "expiresAt": "2026-07-30T12:00:00Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```

**Exceptions and errors:**

- Missing, ineligible, absent, expired, or already-used refresh token: `AuthenticationException("رمز التحديث غير صالح أو منتهي الصلاحية.")` → 401. An inactive token also revokes all remaining active tokens before raising the same exception.
- Failed identity update during rotation: `BusinessException` with space-joined identity error descriptions → 400.
- Validation failures return 400.

### Revoke refresh token — `POST /api/auth/revoke`

**Description:** Validates the access token without checking its lifetime and revokes the supplied refresh token.

**Authentication:** Anonymous controller action. `Content-Type: application/json`.

**Request body (`RevokeRefreshTokenRequest`):**

| Field | C# type | Required/validation |
|---|---|---|
| `token` | `string` | Required (`رمز الوصول مطلوب.`). |
| `refreshToken` | `string` | Required (`رمز التحديث مطلوب.`). |

**Success:** HTTP 200, `ApiResponse<bool>`; `data` is `true` when an active matching token was revoked and `false` when no active matching token exists.

```json
{
  "success": true,
  "data": true,
  "message": "تم إبطال رمز التحديث بنجاح.",
  "errors": null,
  "statusCode": 200
}
```

**Exceptions and errors:**

- Invalid access token or unparsable user id: `BusinessException("رمز الوصول غير صالح.")` → 400.
- User not found: `BusinessException("رمز التحديث غير صالح.")` → 400.
- Validation failures return 400.

### Change password — `POST /api/auth/change-password`

**Description:** Changes the authenticated user’s password and revokes all active refresh tokens.

**Authentication:** `Authorization: Bearer <access-token>` required. `Content-Type: application/json`. Rate limit: IP 20/15 minutes and authenticated user 5/15 minutes.

**Request body (`ChangePasswordRequest`):**

| Field | C# type | Required/validation |
|---|---|---|
| `currentPassword` | `string` | Required (`كلمة المرور الحالية مطلوبة`). |
| `newPassword` | `string` | Required; at least 8 characters; one lowercase, one uppercase, and one digit. |
| `confirmNewPassword` | `string` | Must equal `newPassword` (`كلمة المرور وتأكيد كلمة المرور غير متطابقتين`). |

**Success:** HTTP 200, `ApiResponse` with message `"تم تغيير كلمة المرور بنجاح"`.

```json
{
  "success": true,
  "message": "تم تغيير كلمة المرور بنجاح",
  "errors": null,
  "statusCode": 200
}
```

**Exceptions and errors:**

- Current user cannot be loaded: `AuthenticationException("المستخدم غير معروف")` → 401.
- Wrong current password: `ValidationException` for `CurrentPassword` with `"كلمة المرور الحالية غير صحيحة."` → 400.
- New password equals current password: `ValidationException` for `NewPassword` with `"يجب أن تختلف كلمة المرور الجديدة عن كلمة المرور الحالية."` → 400.
- Identity password-change errors: `PasswordMismatch` maps to the same current-password message; all other identity descriptions are attached to `NewPassword` → 400.
- Failed identity update: `BusinessException` with space-joined identity error descriptions → 400.
- Validation failures return 400; rate-limit rejection returns 429.

### Confirm email — `GET /api/auth/confirm-email`

**Description:** Confirms an email address from the user id and Base64URL-encoded confirmation token.

**Authentication:** Anonymous. Rate limit: IP 20/15 minutes and account key (the supplied `userId`) 5/hour.

**Query parameters:**

| Name | C# type | Required/validation |
|---|---|---|
| `userId` | `string?` | Optional at binding level; service requires nonblank, ≤64 characters, and a valid GUID. |
| `token` | `string?` | Optional at binding level; service requires nonblank and ≤2048 characters; must be Base64URL-decodable. |

**Success:** HTTP 200, `ApiResponse` with message `"تم تأكيد البريد الإلكتروني بنجاح."`. Repeating the call for an already-confirmed user whose status is already the expected role status is idempotent.

```json
{
  "success": true,
  "message": "تم تأكيد البريد الإلكتروني بنجاح.",
  "errors": null,
  "statusCode": 200
}
```

**Exceptions and errors:**

- Every invalid/expired/undecodable token condition, missing user, unsupported role, already-confirmed mismatch, confirmation failure, or update failure uses the exact `BusinessException` message `"رابط تأكيد البريد الإلكتروني غير صالح أو منتهي الصلاحية."` → 400.
- Account or IP rate-limit rejection returns 429.

### Forgot password — `POST /api/auth/forgot-password`

**Description:** Requests a password-reset email. The service intentionally returns the same success response when the email is unknown or ineligible.

**Authentication:** Anonymous. `Content-Type: application/json`. Rate limit: IP 5/15 minutes and account key 3/hour.

**Request body (`ForgotPasswordRequest`):**

| Field | C# type | Required/validation |
|---|---|---|
| `email` | `string` | Required; valid email (`عنوان البريد الإلكتروني مطلوب` / `عنوان البريد الإلكتروني غير صالح`). |

**Success:** HTTP 200, `ApiResponse` with message `"إذا كان البريد الإلكتروني مسجلاً، سيتم إرسال رابط إعادة تعيين كلمة المرور"`.

```json
{
  "success": true,
  "message": "إذا كان البريد الإلكتروني مسجلاً، سيتم إرسال رابط إعادة تعيين كلمة المرور",
  "errors": null,
  "statusCode": 200
}
```

**Exceptions and errors:**

- Email-queue failure: `InvalidOperationException("Password reset email could not be queued.")` → generic 500.
- Validation failures return 400; rate-limit rejection returns 429.

### Reset password — `POST /api/auth/reset-password`

**Description:** Resets an eligible user’s password using the Base64URL-encoded token and revokes all active refresh tokens.

**Authentication:** Anonymous. `Content-Type: application/json`. Rate limit: IP 10/15 minutes, account key by email 5/hour, and account key by token 5/hour.

**Request body (`ResetPasswordRequest`):**

| Field | C# type | Required/validation |
|---|---|---|
| `email` | `string` | Required; valid email. |
| `token` | `string?` | Nullable in the DTO; service requires nonblank, ≤2048 characters, and Base64URL-decodable. |
| `newPassword` | `string` | Required; at least 8 characters; one lowercase, one uppercase, and one digit. |
| `confirmNewPassword` | `string` | Must equal `newPassword`. |

**Success:** HTTP 200, `ApiResponse` with message `"تم إعادة تعيين كلمة المرور بنجاح"`.

```json
{
  "success": true,
  "message": "تم إعادة تعيين كلمة المرور بنجاح",
  "errors": null,
  "statusCode": 200
}
```

**Exceptions and errors:**

- Unknown/ineligible user, missing/oversized/malformed token, invalid token, or token/update failure: `BusinessException("رابط إعادة تعيين كلمة المرور غير صالح أو منتهي الصلاحية.")` → 400.
- Identity password-policy errors other than `InvalidToken`: `ValidationException` for `NewPassword` containing the space-joined identity descriptions → 400.
- Validation failures return 400; rate-limit rejection returns 429.

### Resend verification email — `POST /api/auth/resend-verification`

**Description:** Resends a confirmation email. Unknown, already-confirmed, or non-`Unverified` accounts are intentionally treated as a successful no-op.

**Authentication:** Anonymous. `Content-Type: application/json`. Rate limit: IP 5/15 minutes, account key 1/minute and 3/hour.

**Request body (`ResendVerificationRequest`):**

| Field | C# type | Required/validation |
|---|---|---|
| `email` | `string` | Required; valid email. |

**Success:** HTTP 200, `ApiResponse` with message `"تم إرسال رابط التحقق مرة أخرى"`.

```json
{
  "success": true,
  "message": "تم إرسال رابط التحقق مرة أخرى",
  "errors": null,
  "statusCode": 200
}
```

**Exceptions and errors:**

- Confirmation-email queue failure: `InvalidOperationException("Confirmation email could not be queued.")` → generic 500.
- Validation failures return 400; rate-limit rejection returns 429.

## Users

### Get client profile — `GET /api/clients/profile`

**Description:** Returns the profile for the authenticated client.

**Authentication:** `Authorization: Bearer <access-token>` with role `Client`. Rate limit: IP 300/minute and user 120/minute.

**Request:** No route, query, or body parameters.

**Success:** HTTP 200, `ApiResponse<ClientProfileResponse>`.

```json
{
  "success": true,
  "data": {
    "id": "0f2c5a4d-7a3e-4b7a-8d13-2ef7df7d54f2",
    "name": "Example Client",
    "email": "client@example.com",
    "phoneNumber": "+201012345678",
    "gender": "Male",
    "dateOfBirth": "1990-05-12",
    "address": "Cairo",
    "status": "Active"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```

`dateOfBirth` and `address` can be `null` in the response (`DateOnly?` and `string?`).

**Exceptions and errors:**

- Profile query returns no user: `NotFoundException("الموكل غير موجود")` → 404.
- Missing/invalid JWT or wrong role is rejected by ASP.NET Core authorization (401/403).
- Rate-limit rejection returns 429.

### Update client profile — `PUT /api/clients/profile`

**Description:** Updates the authenticated client’s phone number, date of birth, and address.

**Authentication:** `Authorization: Bearer <access-token>` with role `Client`. `Content-Type: application/json`. Rate limit: IP 60/15 minutes and user 20/15 minutes.

**Request body (`UpdateClientProfileRequest`):**

| Field | C# type | Required/validation |
|---|---|---|
| `phoneNumber` | `string` | Required; must match `^\+20\d{10}$` (`رقم الهاتف مطلوب` / Egyptian-format message). |
| `dateOfBirth` | `DateOnly` | Required; must be earlier than today (`تاريخ الميلاد مطلوب` / past-date message). |
| `address` | `string?` | Optional; maximum 500 characters. |

**Success:** HTTP 200, `ApiResponse` with message `"تم تحديث الملف الشخصي بنجاح."`.

```json
{
  "success": true,
  "message": "تم تحديث الملف الشخصي بنجاح.",
  "errors": null,
  "statusCode": 200
}
```

**Exceptions and errors:**

- Authenticated user cannot be found: `NotFoundException("الموكل غير موجود")` → 404.
- Failed phone update or user update: `BusinessException` with space-joined identity error descriptions → 400.
- Missing/invalid JWT or wrong role is rejected with 401/403; validation failures return 400; rate-limit rejection returns 429.

### Delete client profile — `DELETE /api/clients/profile`

**Description:** Soft-deletes the authenticated client, revokes active refresh tokens, and updates the security stamp.

**Authentication:** `Authorization: Bearer <access-token>` with role `Client`. `Content-Type: application/json`. Rate limit: IP 10/day and user 3/day.

**Request body (`DeleteAccountRequest`):**

| Field | C# type | Required/validation |
|---|---|---|
| `currentPassword` | `string` | Required (`كلمة المرور الحالية مطلوبة.`). |

**Success:** HTTP 200, `ApiResponse` with message `"تم حذف الملف الشخصي بنجاح."`. If the user is already absent or already deleted, the service returns normally and the controller still returns this success response.

```json
{
  "success": true,
  "message": "تم حذف الملف الشخصي بنجاح.",
  "errors": null,
  "statusCode": 200
}
```

**Exceptions and errors:**

- Wrong current password: `BusinessException("كلمة المرور الحالية غير صحيحة.")` → 400.
- Failed security-stamp update: `BusinessException` with space-joined identity error descriptions → 400.
- Missing/invalid JWT or wrong role is rejected with 401/403; validation failures return 400; rate-limit rejection returns 429.

### Get lawyer profile — `GET /api/lawyers/profile`

**Description:** Returns the authenticated lawyer’s private profile.

**Authentication:** `Authorization: Bearer <access-token>` with role `Lawyer`. Rate limit: IP 300/minute and user 120/minute.

**Request:** No route, query, or body parameters.

**Success:** HTTP 200, `ApiResponse<LawyerProfileResponse>`.

```json
{
  "success": true,
  "data": {
    "id": "0f2c5a4d-7a3e-4b7a-8d13-2ef7df7d54f2",
    "name": "Example Lawyer",
    "email": "lawyer@example.com",
    "phoneNumber": "+201012345678",
    "nationalNumber": "29001011234567",
    "gender": "Male",
    "dateOfBirth": "1985-03-20",
    "specializationId": "a6b4f7cb-2f0f-4f32-bf5f-08f2a6b3c701",
    "specializationName": "Civil Law",
    "categoryName": "Law",
    "yearsOfExperience": 12,
    "level": 2,
    "bio": "Civil litigation lawyer",
    "address": "Giza",
    "status": "Active",
    "isAvailable": true,
    "profilePictureUrl": "https://cdn.example/profile.jpg"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```

`specializationId`, `dateOfBirth`, `bio`, `address`, and `profilePictureUrl` are nullable in the DTO. `level` is the numeric `LawyerLevel` enum: `1=GeneralRegistration`, `2=PrimaryCourt`, `3=AppealCourt`, `4=CassationCourt`.

**Exceptions and errors:**

- Profile query returns no user: `NotFoundException("المحامي غير موجود")` → 404.
- Missing/invalid JWT or wrong role is rejected with 401/403; rate-limit rejection returns 429.

### Update lawyer profile — `PUT /api/lawyers/profile`

**Description:** Updates the authenticated lawyer’s contact, specialization, experience, level, biography, and address.

**Authentication:** `Authorization: Bearer <access-token>` with role `Lawyer`. `Content-Type: application/json`. Rate limit: IP 60/15 minutes and user 20/15 minutes.

**Request body (`UpdateLawyerProfileRequest`):**

| Field | C# type | Required/validation |
|---|---|---|
| `phoneNumber` | `string` | Required; Egyptian format `^\+20\d{10}$`. |
| `dateOfBirth` | `DateOnly` | Required; must be in the past. |
| `specializationId` | `Guid` | Required/nonempty; service additionally verifies that a non-deleted specialization exists. |
| `yearsOfExperience` | `int` | Inclusive 0–50. |
| `level` | `LawyerLevel` | Must be a defined enum value (numeric 1–4). |
| `bio` | `string?` | Optional; maximum 500 characters. |
| `address` | `string?` | Optional; maximum 255 characters. |

**Success:** HTTP 200, `ApiResponse` with message `"تم تحديث البيانات بنجاح"`.

```json
{
  "success": true,
  "message": "تم تحديث البيانات بنجاح",
  "errors": null,
  "statusCode": 200
}
```

**Exceptions and errors:**

- Missing user: `NotFoundException("المحامي غير موجود")` → 404.
- Invalid enum/date/experience from the service: `ValidationException` messages `"مستوى المحامي غير صالح."`, `"يجب أن يكون تاريخ الميلاد في الماضي."`, or `"عدد سنوات الخبرة يجب أن يكون بين 0 و 50."` → 400.
- Missing/deleted specialization: `ValidationException(nameof(request.SpecializationId), "التخصص غير صالح.")` → 400.
- Failed phone or user update: `BusinessException` with space-joined identity error descriptions → 400.
- Missing/invalid JWT or wrong role is rejected with 401/403; validation failures return 400; rate-limit rejection returns 429.

### Delete lawyer profile — `DELETE /api/lawyers/profile`

**Description:** Soft-deletes the authenticated lawyer, marks the lawyer unavailable, revokes active refresh tokens, and updates the security stamp.

**Authentication:** `Authorization: Bearer <access-token>` with role `Lawyer`. `Content-Type: application/json`. Rate limit: IP 10/day and user 3/day.

**Request body (`DeleteAccountRequest`):**

| Field | C# type | Required/validation |
|---|---|---|
| `currentPassword` | `string` | Required (`كلمة المرور الحالية مطلوبة.`). |

**Success:** HTTP 200, `ApiResponse` with message `"تم حذف الحساب بنجاح"`. Missing/already-deleted users are treated as a no-op and still return success.

```json
{
  "success": true,
  "message": "تم حذف الحساب بنجاح",
  "errors": null,
  "statusCode": 200
}
```

**Exceptions and errors:**

- Wrong current password: `BusinessException("كلمة المرور الحالية غير صحيحة.")` → 400.
- Failed security-stamp update: `BusinessException` with space-joined identity error descriptions → 400.
- Missing/invalid JWT or wrong role is rejected with 401/403; validation failures return 400; rate-limit rejection returns 429.

### Get public lawyer profile — `GET /api/lawyers/public/{id:guid}`

**Description:** Returns a public profile only when the user is a lawyer with a current lawyer profile, confirmed email, and `Active` status.

**Authentication:** Anonymous (`[AllowAnonymous]` overrides the controller-level Lawyer role). Rate limit: IP 120/minute.

**Route parameter:**

| Name | C# type | Required/validation |
|---|---|---|
| `id` | `Guid` | Required and constrained by `:guid`. |

**Success:** HTTP 200, `ApiResponse<PublicLawyerProfileResponse>`.

```json
{
  "success": true,
  "data": {
    "id": "0f2c5a4d-7a3e-4b7a-8d13-2ef7df7d54f2",
    "name": "Example Lawyer",
    "gender": "Male",
    "specializationId": "a6b4f7cb-2f0f-4f32-bf5f-08f2a6b3c701",
    "specializationName": "Civil Law",
    "categoryName": "Law",
    "yearsOfExperience": 12,
    "level": 2,
    "bio": "Civil litigation lawyer",
    "isAvailable": true,
    "profilePictureUrl": "https://cdn.example/profile.jpg"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```

**Exceptions and errors:**

- No matching public lawyer: `NotFoundException("المحامي غير موجود")` → 404.
- Invalid route GUID is rejected by ASP.NET Core model binding (400); rate-limit rejection returns 429.

## Admin verifications

All routes in this section use the exact controller prefix `api/admin/verifications` and require `Authorization: Bearer <access-token>` with role `Admin`.

### List pending verifications — `GET /api/admin/verifications`

**Description:** Returns a paginated list of lawyers whose current verification documents match the optional status filter.

**Query parameters (`GetPendingVerificationsQuery`):**

| Name | C# type | Required/validation |
|---|---|---|
| `pageNumber` | `int` | Optional; defaults to 1; must be ≥1. |
| `pageSize` | `int` | Optional; defaults to 10; validator allows 1–50. The `PagedRequest` setter clamps values above 50 to 50 before validation. |
| `search` | `string?` | Optional; maximum 100 characters when supplied. Searches trimmed full name or email. |
| `status` | `VerificationDocumentStatus?` | Optional; if supplied must be a defined numeric enum: `1=Pending`, `2=Verified`, `3=Rejected`, `4=Expired`. |

**Success:** HTTP 200, `PagedResponse<IReadOnlyList<PendingVerificationListItemDto>>`.

```json
{
  "success": true,
  "data": [
    {
      "lawyerId": "0f2c5a4d-7a3e-4b7a-8d13-2ef7df7d54f2",
      "fullName": "Example Lawyer",
      "email": "lawyer@example.com",
      "phoneNumber": "+201012345678",
      "pendingDocumentCount": 2,
      "verifiedDocumentCount": 1,
      "rejectedDocumentCount": 0
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 1,
  "totalRecords": 1,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

`phoneNumber` is nullable in `PendingVerificationListItemDto`.

**Exceptions and errors:**

- Invalid query values return the handler’s `PagedResponse` with status 400 and validator messages (the status enum message is `"Status must be a valid verification document status."`).
- A missing Lawyer role row returns an empty successful page, not an exception.
- Authorization failure is 401/403.

### Get lawyer verification details — `GET /api/admin/verifications/{lawyerId:guid}`

**Description:** Returns the lawyer’s current verification documents and account verification state.

**Route parameter:**

| Name | C# type | Required/validation |
|---|---|---|
| `lawyerId` | `Guid` | Required and constrained by `:guid`; validator requires a nonempty GUID. |

**Success:** HTTP 200, `ApiResponse<VerificationDetailsDto>`.

```json
{
  "success": true,
  "data": {
    "lawyerId": "0f2c5a4d-7a3e-4b7a-8d13-2ef7df7d54f2",
    "fullName": "Example Lawyer",
    "email": "lawyer@example.com",
    "phoneNumber": "+201012345678",
    "accountStatus": "PendingReview",
    "isFullyVerified": false,
    "documents": [
      {
        "documentId": "7ed88c0a-7e67-4a34-bdfd-7b0a6dfb4e3a",
        "documentType": "NationalIdFront",
        "status": "Pending",
        "fileName": "national-id-front.jpg",
        "contentType": "image/jpeg",
        "expirationDate": "2030-12-31",
        "reviewedAt": null,
        "rejectionReason": null,
        "contentUrl": "/api/admin/verifications/documents/7ed88c0a-7e67-4a34-bdfd-7b0a6dfb4e3a/content"
      }
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```

`phoneNumber`, `reviewedAt`, and `rejectionReason` are nullable. `documentType`, `status`, `accountStatus`, and `contentUrl` are strings because the handler calls `ToString()` for those values.

**Exceptions and errors:**

- Empty GUID validation returns `ApiResponse` 400.
- Missing user or user not in the Lawyer role: `NotFoundException("Lawyer was not found.")` → 404.
- Authorization failure is 401/403.

### Download verification document — `GET /api/admin/verifications/documents/{documentId:guid}/content`

**Description:** Downloads the current verification document bytes.

**Route parameter:**

| Name | C# type | Required/validation |
|---|---|---|
| `documentId` | `Guid` | Required and constrained by `:guid`; handler rejects `Guid.Empty`. |

**Success:** HTTP 200 binary file response, not an `ApiResponse` envelope. The response body is the stored bytes, `Content-Type` is the stored document content type, and the download filename is the stored original filename.

The handler’s exact `VerificationDocumentContentDto` is `{ Content: byte[], ContentType: string, FileName: string }`; the controller uses those three properties to produce the binary `File(...)` result instead of serializing the DTO.

**Exceptions and errors:**

- Empty GUID: `ApiResponse<VerificationDocumentContentDto>.Fail("Document id is required.")` → 400.
- Missing current document: `NotFoundException("Verification document was not found.")` → 404.
- Authorization failure is 401/403.

### Review verification document — `PATCH /api/admin/verifications/documents/{documentId:guid}`

**Description:** Approves or rejects the current pending document and recalculates the lawyer account status.

**Route parameter:**

| Name | C# type | Required/validation |
|---|---|---|
| `documentId` | `Guid` | Required and constrained by `:guid`; validator requires nonempty. |

**Request body (`ReviewVerificationDocumentRequest`):**

| Field | C# type | Required/validation |
|---|---|---|
| `decision` | `VerificationReviewDecision` | Must be numeric enum `1=Approve` or `2=Reject`. |
| `rejectionReason` | `string?` | When `decision=2`, required and maximum 500 characters. When `decision=1`, must be empty; otherwise validator message is `"A rejection reason can only be supplied when rejecting a document."`. |

**Success:** HTTP 200, `ApiResponse<ReviewVerificationDocumentResponse>`.

```json
{
  "success": true,
  "data": {
    "documentId": "7ed88c0a-7e67-4a34-bdfd-7b0a6dfb4e3a",
    "documentStatus": "Verified",
    "lawyerAccountStatus": "Active",
    "isFullyVerified": true
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```

**Exceptions and errors:**

- Validation failures return `ApiResponse` 400.
- Missing document or non-lawyer owner: `ApiResponse` `"Verification document was not found."` with 404.
- Non-current document: `"Only the current version of a document can be reviewed."` with 409.
- Non-pending document: `"Only pending documents can be reviewed."` with 409.
- Expired document: the handler marks it `Expired`, recalculates the account, and returns `"The document has expired and must be submitted again."` with 409.
- Authorization failure is 401/403.

## User verification documents

The controller route token `[controller]` expands exactly to `UserVerification`, so these paths are case-preserving templates under `api/UserVerification`. No `[Authorize]` attribute is present on this controller; the current implementation therefore does not enforce authentication at the controller boundary.

### Submit verification documents — `POST /api/UserVerification/submit-verification-documents`

**Description:** Uploads one or more verification images for a user and returns per-file successes and failures.

**Authentication:** No controller authentication requirement. `Content-Type: multipart/form-data`.

**Form body (`SubmitVerificationDocumentsCommand`):**

| Field | C# type | Required/validation |
|---|---|---|
| `userId` | `Guid` | Required/nonempty (`UserId is required`). |
| `documents` | `List<VerificationDocumentDto>` | Required/nonempty (`Documents are required.`); document `type` values must be distinct (`The same verification document cannot be submitted more than once.`). |
| `documents[i].file` | `IFormFile` | Nested DTO field. The handler requires a non-empty file and an allowed image content type; no FluentValidation rule is registered for it. |
| `documents[i].expirationDate` | `DateOnly` | Must be later than today; otherwise the item is returned in `failedDocuments` with `"This document is expired"`. |
| `documents[i].type` | `VerificationDocumentType` | Numeric enum: `1=NationalIdFront`, `2=NationalIdBack`, `3=BarAssociationCardFront`, `4=BarAssociationCardBack`, `5=other`. Types 1–4 map to storage folders; type 5 produces an upload failure. |

For multipart binding, send repeated indexed field names such as `documents[0].file`, `documents[0].expirationDate`, and `documents[0].type`.

**Success:** HTTP 200, `ApiResponse<SubmitVerificationDocumentResponseDto>`. Individual file problems do not fail the whole response; they appear in `failedDocuments`.

```json
{
  "success": true,
  "data": {
    "uploadedDocuments": [
      {
        "fileName": "id-front.jpg",
        "type": 1
      }
    ],
    "failedDocuments": [
      {
        "fileName": "old-card.jpg",
        "type": 3,
        "error": "This document is expired"
      }
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```

**Handler errors and per-document conditions:**

- Invalid command validation returns 400.
- Unknown user returns a failed `ApiResponse` with message list `"The specified user doesn't exists"` and status 400.
- A null list item is reported as `failedDocuments` with empty `fileName`, `type=5`, and `"Document is null."`.
- Zero-length file: `"Document is empty."`.
- Content types other than `image/jpeg`, `image/png`, `image/webp`, `image/heic`, or `image/heif`: `"Only JPEG, PNG, WEBP, HEIC, and HEIF images are allowed."`.
- Expired date: `"This document is expired"`.
- A type already pending for the user: `"You already uploaded this document before. Wait untill admin verifies your document"`.
- Unsupported enum type/folder: the item error is `"An error occurred while uploading the document: Unsupported verification document type."`.
- Storage/upload exceptions are returned as `"An error occurred while uploading the document: {exception message}"`.
- A database save failure deletes already-uploaded paths and returns `"An error occured while uploading your documents. Try again please.."` with status 400.

### Get user verification documents — `GET /api/UserVerification/{UserId}`

**Description:** Lists all verification documents belonging to the supplied user id.

**Authentication:** No controller authentication requirement.

**Route parameter:**

| Name | C# type | Required/validation |
|---|---|---|
| `UserId` | `Guid` | Required/nonempty (`User Id is required.`). The route template uses the exact casing `{UserId}`. |

**Success:** HTTP 200, `ApiResponse<GetUserVerificationDocumentsResponseDto>`.

```json
{
  "success": true,
  "data": {
    "documents": [
      {
        "documentId": "7ed88c0a-7e67-4a34-bdfd-7b0a6dfb4e3a",
        "documentType": 1,
        "status": 1,
        "expirationDate": "2030-12-31",
        "isCurrent": true,
        "fileName": "id-front.jpg"
      }
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```

`documentId` is populated from `StoredFileId` (the delete endpoint queries by the stored-file id). `documentType` and `status` are numeric enum values described above.

**Exceptions and errors:**

- Empty user id validation returns status 400 with `"User Id is required."`.
- Unknown user returns status 404 with error `"The specified user does not exist."`.

### Delete verification document — `DELETE /api/UserVerification`

**Description:** Deletes a user’s verification document from storage and the database.

**Authentication:** No controller authentication requirement. Parameters are query-bound.

**Query parameters (`DeleteVerificationDocumentCommand`):**

| Name | C# type | Required/validation |
|---|---|---|
| `userId` | `Guid` | Required/nonempty (`User Id is required.`). |
| `documentId` | `Guid` | Required/nonempty (`Document Id is required.`). This is matched against `StoredFile.Id`. |

**Success:** HTTP 200, `ApiResponse` with no message.

```json
{
  "success": true,
  "message": null,
  "errors": null,
  "statusCode": 200
}
```

**Exceptions and errors:**

- Unknown user: 404 with `"The specified user doesn't exist."`.
- Missing document for the user/file id: 404 with `"Verification document was not found."`.
- Storage deletion failure: 400 with `"An error occured while deleting the document. Try again please.."`.
- Validation failures return 400.

## Health

### Ping — `GET /api/Health/ping`

**Description:** Returns a live operational marker. This is the only controller response that is not wrapped in `ApiResponse<T>`.

**Authentication:** No controller authorization attribute.

**Request:** No route, query, or body parameters.

**Success:** HTTP 200, plain JSON object.

```json
{
  "message": "Pong! Smart Court API is fully operational.",
  "serverTimeUtc": "2026-07-23T12:00:00Z",
  "version": "1.0.0"
}
```

`serverTimeUtc` is generated from `DateTime.UtcNow` for each request; `version` is always `"1.0.0"`.

## Response DTO type catalog

The following tables are the exact C# response DTO properties used by the endpoint sections above. Nullable markers are part of the declared type.

### Auth response DTOs

| DTO | Property | Exact C# type |
|---|---|---|
| `LoginResponse` | `User` | `UserDto` |
|  | `AccessToken` | `string` |
|  | `ExpiresIn` | `int` (seconds, from `TokenResult.ExpiresInSeconds`) |
|  | `RefreshToken` | `string` |
|  | `RefreshTokenExpiration` | `DateTime` |
| `UserDto` | `Id` | `string` |
|  | `Email` | `string` |
|  | `FullName` | `string` |
|  | `Role` | `string` |
| `RegisterResponse` | `UserId` | `string` |
|  | `Email` | `string` |
|  | `FullName` | `string` |
|  | `Role` | `string` |
| `RefreshTokenResponse` | `AccessToken` | `string` |
|  | `RefreshToken` | `string` |
|  | `ExpiresAt` | `DateTime` |

### User profile response DTOs

| DTO | Property | Exact C# type |
|---|---|---|
| `ClientProfileResponse` | `Id` | `Guid` |
|  | `Name` | `string` |
|  | `Email` | `string` |
|  | `PhoneNumber` | `string` |
|  | `Gender` | `string` |
|  | `DateOfBirth` | `DateOnly?` |
|  | `Address` | `string?` |
|  | `Status` | `string` |
| `LawyerProfileResponse` | `Id` | `Guid` |
|  | `Name` | `string` |
|  | `Email` | `string` |
|  | `PhoneNumber` | `string` |
|  | `NationalNumber` | `string` |
|  | `Gender` | `string` |
|  | `DateOfBirth` | `DateOnly?` |
|  | `SpecializationId` | `Guid?` |
|  | `SpecializationName` | `string` |
|  | `CategoryName` | `string` |
|  | `YearsOfExperience` | `int` |
|  | `Level` | `SmartCourt.Common.Enums.LawyerLevel` |
|  | `Bio` | `string?` |
|  | `Address` | `string?` |
|  | `Status` | `string` |
|  | `IsAvailable` | `bool` |
|  | `ProfilePictureUrl` | `string?` |
| `PublicLawyerProfileResponse` | `Id` | `Guid` |
|  | `Name` | `string` |
|  | `Gender` | `string` |
|  | `SpecializationId` | `Guid?` |
|  | `SpecializationName` | `string` |
|  | `CategoryName` | `string` |
|  | `YearsOfExperience` | `int` |
|  | `Level` | `SmartCourt.Common.Enums.LawyerLevel` |
|  | `Bio` | `string?` |
|  | `IsAvailable` | `bool` |
|  | `ProfilePictureUrl` | `string?` |

### Admin verification response DTOs

| DTO | Property | Exact C# type |
|---|---|---|
| `PendingVerificationListItemDto` | `LawyerId` | `Guid` |
|  | `FullName` | `string` |
|  | `Email` | `string` |
|  | `PhoneNumber` | `string?` |
|  | `PendingDocumentCount` | `int` |
|  | `VerifiedDocumentCount` | `int` |
|  | `RejectedDocumentCount` | `int` |
| `VerificationDetailsDto` | `LawyerId` | `Guid` |
|  | `FullName` | `string` |
|  | `Email` | `string` |
|  | `PhoneNumber` | `string?` |
|  | `AccountStatus` | `string` |
|  | `IsFullyVerified` | `bool` |
|  | `Documents` | `IReadOnlyList<VerificationDocumentDetailsDto>` |
| `VerificationDocumentDetailsDto` | `DocumentId` | `Guid` |
|  | `DocumentType` | `string` |
|  | `Status` | `string` |
|  | `FileName` | `string` |
|  | `ContentType` | `string` |
|  | `ExpirationDate` | `DateOnly` |
|  | `ReviewedAt` | `DateTime?` |
|  | `RejectionReason` | `string?` |
|  | `ContentUrl` | `string` |
| `ReviewVerificationDocumentResponse` | `DocumentId` | `Guid` |
|  | `DocumentStatus` | `string` |
|  | `LawyerAccountStatus` | `string` |
|  | `IsFullyVerified` | `bool` |

### User verification response DTOs

| DTO | Property | Exact C# type |
|---|---|---|
| `SubmitVerificationDocumentResponseDto` | `UploadedDocuments` | `List<UploadedDocumentDto>` |
|  | `FailedDocuments` | `List<DocumentUploadErrorDto>` |
| `UploadedDocumentDto` | `FileName` | `string` |
|  | `Type` | `VerificationDocumentType` |
| `DocumentUploadErrorDto` | `FileName` | `string` |
|  | `Type` | `VerificationDocumentType` |
|  | `Error` | `string` |
| `GetUserVerificationDocumentsResponseDto` | `Documents` | `List<UserVerificationDocumentDto>` |
| `UserVerificationDocumentDto` | `DocumentId` | `Guid` |
|  | `DocumentType` | `VerificationDocumentType` |
|  | `Status` | `VerificationDocumentStatus` |
|  | `ExpirationDate` | `DateOnly` |
|  | `IsCurrent` | `bool` |
|  | `FileName` | `string` |
| `VerificationDocumentContentDto` | `Content` | `byte[]` |
|  | `ContentType` | `string` |
|  | `FileName` | `string` |

## Cross-reference checklist

The endpoint trace was checked against the source as follows:

1. Every controller under `SmartCourt/Features` was enumerated (15 controllers, 25 actions).
2. Each action’s verb, controller prefix, action template, route constraints, binding source, and authorization attributes were read from its controller.
3. Every request and response DTO used by an action was opened directly; inherited `PagedRequest` and shared response properties were included.
4. Every feature validator was read. Service/handler-only validation and per-item upload checks were listed separately from FluentValidation rules.
5. Each service or MediatR handler call path was scanned for explicit exception messages, returned failure envelopes, and status codes.
6. JSON examples were cross-checked against the DTO property names and C# types, including nullable fields and numeric enums.

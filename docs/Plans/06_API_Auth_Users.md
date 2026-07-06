# Smart Court — API Contracts: Auth, Users & Files

> **Version:** 1.0 | **Base URL:** `/api` | **Auth:** JWT Bearer Token
> **Content-Type:** `application/json` (unless multipart)
> **Language:** All user-facing messages in Arabic

---

## Standardized Response Wrapper

All endpoints return this wrapper:

```json
{
  "success": true,
  "statusCode": 200,
  "message": "string | null",
  "data": "T | null",
  "errors": ["string"] | null
}
```

### Standardized Error Response (Validation)

```json
{
  "success": false,
  "statusCode": 400,
  "message": "بيانات غير صالحة",
  "data": null,
  "errors": [
    "عنوان البريد الإلكتروني مطلوب",
    "كلمة المرور يجب أن تكون 8 أحرف على الأقل"
  ]
}
```

---

## Shared Types

### Pagination Request (Query Parameters)

| Param | Type | Default | Description |
|-------|------|---------|-------------|
| `pageNumber` | int | 1 | Current page |
| `pageSize` | int | 10 | Items per page (max 50) |
| `sortBy` | string | `createdAt` | Sort field |
| `sortDirection` | string | `desc` | `asc` or `desc` |
| `search` | string | null | Free-text search |

### Pagination Response Wrapper

```json
{
  "items": [],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 42,
  "totalPages": 5,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

---

## 1. Authentication Slice

---

### POST `/api/auth/register/client`

**Description:** Register a new client account.
**Auth:** `[AllowAnonymous]`

**Request Body:**

```json
{
  "fullName": "string — required, max 100 chars",
  "email": "string — required, valid email format",
  "password": "string — required, min 8 chars, must contain uppercase + lowercase + digit",
  "confirmPassword": "string — required, must match password"
}
```

**Response (201 Created):**

```json
{
  "success": true,
  "statusCode": 201,
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "data": {
    "userId": "uuid",
    "email": "string",
    "fullName": "string",
    "role": "Client"
  }
}
```

**Status Codes:**

| Code | Condition |
|------|-----------|
| 201 | Account created, verification email sent |
| 400 | Validation errors (see errors array) |
| 409 | Email already registered |

**Business Rules:**
- Creates `AspNetUsers` entry with role `Client`
- Creates `ClientProfile` with `UserId` = new user ID
- Creates `NotificationPreference` with defaults (InApp=true, Email=true, SMS=false)
- Sends verification email via `IEmailProvider`
- No JWT returned until email is verified

---

### POST `/api/auth/register/lawyer`

**Description:** Register a new lawyer account.
**Auth:** `[AllowAnonymous]`

**Content-Type:** `multipart/form-data`

**Request Body (Form Data):**

| Field | Type | Description |
|-------|------|-------------|
| `fullName` | string | required, max 100 chars |
| `email` | string | required, valid email format |
| `password` | string | required, min 8 chars, must contain uppercase + lowercase + digit |
| `confirmPassword` | string | required, must match password |
| `phone` | string | required, Egyptian format (+20xxxxxxxxxx) |
| `address` | string | required |
| `government` | string | required, Governorate name |
| `city` | string | required |
| `gender` | string | required (e.g., Male/Female) |
| `nationalNumber` | string | required, 14 digits |
| `idCardImageFront` | file | required (JPEG/PNG/PDF) |
| `idCardImageBack` | file | required (JPEG/PNG/PDF) |
| `barAssociationIdCardFront` | file | required (JPEG/PNG/PDF) |
| `barAssociationIdCardBack` | file | required (JPEG/PNG/PDF) |

**Response (201 Created):**

```json
{
  "success": true,
  "statusCode": 201,
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "data": {
    "userId": "uuid",
    "email": "string",
    "fullName": "string",
    "role": "Lawyer"
  }
}
```

**Status Codes:** Same as client registration.

**Business Rules:**
- Creates `AspNetUsers` entry with role `Lawyer`
- Creates `LawyerProfile` with `IsAvailable = false`, all verification statuses = `Pending`
- Lawyer cannot receive proposals or publish articles until verified

---

### POST `/api/auth/login`

**Description:** Authenticate and receive JWT tokens.
**Auth:** `[AllowAnonymous]`

**Request Body:**

```json
{
  "email": "string — required",
  "password": "string — required"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "statusCode": 200,
  "message": null,
  "data": {
    "accessToken": "string — JWT token",
    "refreshToken": "string — opaque refresh token",
    "expiresAt": "2026-07-03T12:00:00Z",
    "user": {
      "id": "uuid",
      "email": "string",
      "fullName": "string",
      "role": "Client | Lawyer | Admin",
      "profilePictureUrl": "string | null",
      "isVerified": true
    }
  }
}
```

**Status Codes:**

| Code | Condition |
|------|-----------|
| 200 | Login successful |
| 400 | Missing fields |
| 401 | Invalid email or password |
| 403 | Email not verified (message: "يرجى تأكيد البريد الإلكتروني أولاً") |
| 403 | Account suspended (message: "تم تعليق حسابك. تواصل مع الدعم") |

**Business Rules:**
- Checks `IsActive` flag — suspended accounts return 403
- Checks `EmailConfirmed` — unverified return 403
- Access token JWT claims: `sub` (userId), `email`, `role`, `jti`, `exp`, `iss`, `aud`
- Access token expiry: 60 minutes (configurable)
- Refresh token stored in DB, expiry: 7 days
- `isVerified` for lawyers means both NationalId and BarCard are approved

---

### POST `/api/auth/refresh`

**Description:** Refresh an expired access token.
**Auth:** `[AllowAnonymous]`

**Request Body:**

```json
{
  "refreshToken": "string — required"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "accessToken": "string — new JWT",
    "refreshToken": "string — new refresh token (rotated)",
    "expiresAt": "2026-07-03T13:00:00Z"
  }
}
```

**Status Codes:**

| Code | Condition |
|------|-----------|
| 200 | Tokens refreshed |
| 401 | Invalid or expired refresh token |

**Business Rules:**
- Old refresh token is invalidated (one-time use / rotation)
- If refresh token is reused (replay attack), revoke ALL user's refresh tokens

---

### POST `/api/auth/verify-email`

**Description:** Verify user's email address using the token from verification email.
**Auth:** `[AllowAnonymous]`

**Request Body:**

```json
{
  "userId": "uuid — required",
  "token": "string — required, URL-decoded email confirmation token"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "statusCode": 200,
  "message": "تم تأكيد البريد الإلكتروني بنجاح"
}
```

**Status Codes:**

| Code | Condition |
|------|-----------|
| 200 | Email verified |
| 400 | Invalid or expired token |
| 404 | User not found |

---

### POST `/api/auth/resend-verification`

**Description:** Resend the email verification link.
**Auth:** `[AllowAnonymous]`

**Request Body:**

```json
{
  "email": "string — required"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "statusCode": 200,
  "message": "تم إرسال رابط التحقق مرة أخرى"
}
```

**Status Codes:**

| Code | Condition |
|------|-----------|
| 200 | Always returns 200 (prevent user enumeration) |

**Business Rules:**
- If email doesn't exist or is already verified → still return 200 (no info leak)
- Rate limited: max 3 per hour per email

---

### POST `/api/auth/forgot-password`

**Description:** Request a password reset link.
**Auth:** `[AllowAnonymous]`

**Request Body:**

```json
{
  "email": "string — required"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "statusCode": 200,
  "message": "إذا كان البريد الإلكتروني مسجلاً، سيتم إرسال رابط إعادة تعيين كلمة المرور"
}
```

**Business Rules:**
- Always returns 200 regardless of email existence
- Reset token valid for 1 hour
- Rate limited: max 3 per hour per email

---

### POST `/api/auth/reset-password`

**Description:** Reset password using the token from the reset email.
**Auth:** `[AllowAnonymous]`

**Request Body:**

```json
{
  "email": "string — required",
  "token": "string — required, URL-decoded reset token",
  "newPassword": "string — required, same strength rules as registration",
  "confirmNewPassword": "string — required, must match newPassword"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "statusCode": 200,
  "message": "تم إعادة تعيين كلمة المرور بنجاح"
}
```

**Status Codes:**

| Code | Condition |
|------|-----------|
| 200 | Password reset |
| 400 | Invalid token, password validation failed |

**Business Rules:**
- Invalidates all existing refresh tokens for the user (force re-login)

---

### POST `/api/auth/change-password`

**Description:** Change password while logged in.
**Auth:** `[Authorize]`

**Request Headers:** `Authorization: Bearer {accessToken}`

**Request Body:**

```json
{
  "currentPassword": "string — required",
  "newPassword": "string — required",
  "confirmNewPassword": "string — required"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "statusCode": 200,
  "message": "تم تغيير كلمة المرور بنجاح"
}
```

**Status Codes:**

| Code | Condition |
|------|-----------|
| 200 | Password changed |
| 400 | Current password incorrect, new password validation failed |
| 401 | Not authenticated |

---

## 2. Users Slice

---

### GET `/api/users/profile`

**Description:** Get the current authenticated user's full profile.
**Auth:** `[Authorize]`

**Request Headers:** `Authorization: Bearer {accessToken}`

**Response (200 OK) — Client:**

```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "id": "uuid",
    "email": "string",
    "firstName": "string",
    "lastName": "string",
    "phoneNumber": "string",
    "profilePictureUrl": "string | null",
    "role": "Client",
    "isActive": true,
    "createdAt": "datetime",
    "clientProfile": {
      "dateOfBirth": "datetime | null",
      "nationalIdVerificationStatus": 0,
      "nationalIdVerificationStatusName": "NotSubmitted | Pending | Approved | Rejected"
    }
  }
}
```

**Response (200 OK) — Lawyer:**

```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "id": "uuid",
    "email": "string",
    "firstName": "string",
    "lastName": "string",
    "phoneNumber": "string",
    "profilePictureUrl": "string | null",
    "role": "Lawyer",
    "isActive": true,
    "createdAt": "datetime",
    "lawyerProfile": {
      "bio": "string | null",
      "officeAddress": "string | null",
      "yearsOfExperience": 0,
      "isAvailable": false,
      "nationalIdVerificationStatus": 0,
      "nationalIdVerificationStatusName": "string",
      "barCardVerificationStatus": 0,
      "barCardVerificationStatusName": "string",
      "isFullyVerified": false,
      "specializations": [
        {
          "id": "uuid",
          "name": "string"
        }
      ]
    }
  }
}
```

---

### PUT `/api/users/profile`

**Description:** Update common profile fields (shared by all roles).
**Auth:** `[Authorize]`

**Request Body:**

```json
{
  "firstName": "string — required, max 50",
  "lastName": "string — required, max 50",
  "phoneNumber": "string — required, Egyptian format",
  "profilePictureFileId": "uuid | null — StoredFile ID from file upload"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "statusCode": 200,
  "message": "تم تحديث الملف الشخصي بنجاح",
  "data": { /* full profile object as in GET */ }
}
```

---

### PUT `/api/users/profile/client`

**Description:** Update client-specific profile fields.
**Auth:** `[Authorize(Roles = "Client")]`

**Request Body:**

```json
{
  "dateOfBirth": "datetime | null"
}
```

**Response (200 OK):** Full profile object.

---

### PUT `/api/users/profile/lawyer`

**Description:** Update lawyer-specific profile fields.
**Auth:** `[Authorize(Roles = "Lawyer")]`

**Request Body:**

```json
{
  "bio": "string | null — max 2000 chars",
  "officeAddress": "string | null — max 500 chars",
  "yearsOfExperience": "int — min 0, max 60",
  "isAvailable": "bool"
}
```

**Response (200 OK):** Full profile object.

**Business Rules:**
- `isAvailable` can only be set to `true` if lawyer is fully verified
- If not verified and `isAvailable = true` → 400 "يجب إكمال التحقق أولاً"

---

### PUT `/api/users/profile/lawyer/specializations`

**Description:** Set the lawyer's legal specializations (replaces all existing).
**Auth:** `[Authorize(Roles = "Lawyer")]`

**Request Body:**

```json
{
  "legalCategoryIds": ["uuid", "uuid", "uuid"]
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "statusCode": 200,
  "message": "تم تحديث التخصصات بنجاح",
  "data": {
    "specializations": [
      { "id": "uuid", "name": "قانون مدني" },
      { "id": "uuid", "name": "قانون جنائي" }
    ]
  }
}
```

**Status Codes:**

| Code | Condition |
|------|-----------|
| 200 | Updated |
| 400 | One or more category IDs invalid |

**Business Rules:**
- Deletes all existing `LawyerSpecialization` rows and inserts new ones
- Validates all category IDs exist in `LegalCategory`
- Min 1, max 10 specializations

---

### GET `/api/legal-categories`

**Description:** List all available legal categories.
**Auth:** `[AllowAnonymous]`

**Response (200 OK):**

```json
{
  "success": true,
  "statusCode": 200,
  "data": [
    {
      "id": "uuid",
      "name": "قانون مدني",
      "description": "يشمل العقود والالتزامات والمسؤولية المدنية"
    },
    {
      "id": "uuid",
      "name": "قانون جنائي",
      "description": "يشمل الجرائم والعقوبات"
    }
  ]
}
```

**Business Rules:**
- Returns all categories (no pagination — fixed list)
- Cached in memory (IMemoryCache, 1 hour TTL)

---

## 3. Lawyer Verification Slice

---

### POST `/api/lawyer-verification/national-id`

**Description:** Submit National ID documents for verification.
**Auth:** `[Authorize(Roles = "Lawyer")]`

**Request Body:**

```json
{
  "frontFileId": "uuid — required, StoredFile ID of front image",
  "backFileId": "uuid — required, StoredFile ID of back image"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "statusCode": 200,
  "message": "تم تقديم الهوية الوطنية للمراجعة",
  "data": {
    "nationalIdVerificationStatus": 1,
    "nationalIdVerificationStatusName": "Pending"
  }
}
```

**Status Codes:**

| Code | Condition |
|------|-----------|
| 200 | Submitted |
| 400 | File IDs invalid or not images |
| 409 | Already submitted and pending/approved |

**Business Rules:**
- Sets `NationalIdFrontFileId`, `NationalIdBackFileId` on `LawyerProfile`
- Sets `NationalIdVerificationStatus = Pending`
- Creates notification for all Admin users
- Cannot resubmit if status is `Pending` or `Approved`
- Can resubmit only if `Rejected` (updates files, resets status to Pending)

---

### POST `/api/lawyer-verification/bar-card`

**Description:** Submit Bar Association card documents for verification.
**Auth:** `[Authorize(Roles = "Lawyer")]`

**Request Body:**

```json
{
  "frontFileId": "uuid — required, StoredFile ID of front image",
  "backFileId": "uuid — required, StoredFile ID of back image"
}
```

**Response (200 OK):** Same structure as national-id submission.

**Business Rules:** Same as national-id but for `BarCard*` fields.

---

### GET `/api/lawyer-verification/status`

**Description:** Get current verification status for the logged-in lawyer.
**Auth:** `[Authorize(Roles = "Lawyer")]`

**Response (200 OK):**

```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "nationalId": {
      "status": 2,
      "statusName": "Approved",
      "frontFileId": "uuid | null",
      "backFileId": "uuid | null",
      "reviewedByUserId": "uuid | null",
      "verifiedAt": "datetime | null"
    },
    "barCard": {
      "status": 1,
      "statusName": "Pending",
      "frontFileId": "uuid | null",
      "backFileId": "uuid | null",
      "reviewedByUserId": "uuid | null",
      "verifiedAt": "datetime | null"
    },
    "isFullyVerified": false
  }
}
```

---

### GET `/api/admin/verifications/pending`

**Description:** List all lawyers with pending verification requests.
**Auth:** `[Authorize(Roles = "Admin")]`

**Query Params:** Standard pagination + `type` filter (`nationalId` | `barCard` | `all`)

**Response (200 OK):**

```json
{
  "success": true,
  "statusCode": 200,
  "data": {
    "items": [
      {
        "lawyerUserId": "uuid",
        "lawyerName": "string",
        "lawyerEmail": "string",
        "verificationType": "NationalId | BarCard",
        "frontFileUrl": "string",
        "backFileUrl": "string",
        "submittedAt": "datetime"
      }
    ],
    "totalCount": 5,
    "pageNumber": 1,
    "pageSize": 10
  }
}
```

---

### PUT `/api/admin/verifications/{userId}/national-id`

**Description:** Approve or reject a lawyer's National ID verification.
**Auth:** `[Authorize(Roles = "Admin")]`

**Request Body:**

```json
{
  "status": "int — 2 (Approved) or 3 (Rejected)",
  "rejectionReason": "string | null — required if rejected"
}
```

**Response (200 OK):**

```json
{
  "success": true,
  "statusCode": 200,
  "message": "تم تحديث حالة التحقق"
}
```

**Business Rules:**
- Sets `NationalIdVerificationStatus`, `NationalIdReviewedByUserId`, `NationalIdVerifiedAt`
- Sends notification to lawyer with decision
- If both NationalId + BarCard are now Approved → lawyer becomes fully verified

---

### PUT `/api/admin/verifications/{userId}/bar-card`

**Description:** Approve or reject a lawyer's Bar Card verification.
**Auth:** `[Authorize(Roles = "Admin")]`

**Request/Response:** Same structure as national-id review.

---

## 4. File Upload Slice

---

### POST `/api/files/upload`

**Description:** Upload a file to the platform.
**Auth:** `[Authorize]`
**Content-Type:** `multipart/form-data`

**Request Body (Form Data):**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `file` | File | Yes | The file to upload |

**Response (201 Created):**

```json
{
  "success": true,
  "statusCode": 201,
  "data": {
    "id": "uuid — StoredFile ID (use this to reference the file)",
    "originalFileName": "document.pdf",
    "contentType": "application/pdf",
    "extension": ".pdf",
    "fileSize": 1048576,
    "downloadUrl": "/api/files/uuid/download",
    "createdAt": "datetime"
  }
}
```

**Status Codes:**

| Code | Condition |
|------|-----------|
| 201 | File uploaded |
| 400 | No file, file empty, invalid type, exceeds max size |
| 401 | Not authenticated |

**Business Rules:**
- Allowed extensions: `.pdf`, `.jpg`, `.jpeg`, `.png`, `.mp3`, `.mp4`, `.webm`, `.doc`, `.docx`
- Max file size: 50 MB (configurable)
- File stored via `IFileStorageProvider` (local path: `./uploads/{yyyy}/{MM}/{guid}.{ext}`)
- `StoredFile` record created with `UploadedByUserId` = current user
- Returns the `StoredFile.Id` — used as reference in other entities

---

### POST `/api/files/upload/multiple`

**Description:** Upload multiple files at once.
**Auth:** `[Authorize]`
**Content-Type:** `multipart/form-data`

**Request Body (Form Data):**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `files` | File[] | Yes | Multiple files (max 10) |

**Response (201 Created):**

```json
{
  "success": true,
  "statusCode": 201,
  "data": [
    {
      "id": "uuid",
      "originalFileName": "doc1.pdf",
      "contentType": "application/pdf",
      "extension": ".pdf",
      "fileSize": 524288,
      "downloadUrl": "/api/files/uuid/download",
      "createdAt": "datetime"
    }
  ]
}
```

---

### GET `/api/files/{id}/download`

**Description:** Download a file by its StoredFile ID.
**Auth:** `[Authorize]`

**Response:** Binary file content with appropriate `Content-Type` and `Content-Disposition` headers.

**Status Codes:**

| Code | Condition |
|------|-----------|
| 200 | File returned |
| 401 | Not authenticated |
| 404 | File not found |

**Business Rules:**
- Returns file as binary stream
- Sets `Content-Disposition: attachment; filename="original_name.pdf"`
- Access control: file owner, case participants, or admin can download

---

### DELETE `/api/files/{id}`

**Description:** Delete a previously uploaded file.
**Auth:** `[Authorize]`

**Response (200 OK):**

```json
{
  "success": true,
  "statusCode": 200,
  "message": "تم حذف الملف بنجاح"
}
```

**Status Codes:**

| Code | Condition |
|------|-----------|
| 200 | File deleted |
| 401 | Not authenticated |
| 403 | Not the file owner |
| 404 | File not found |
| 409 | File is referenced by another entity (attachment, verification doc) |

**Business Rules:**
- Only the uploader can delete their own files
- Cannot delete if file is referenced by CaseAttachment, MessageAttachment, ContractAttachment, etc.
- Deletes both the `StoredFile` DB record and the physical file

---

## Enum Reference

### VerificationStatus

| Value | Name | Description |
|-------|------|-------------|
| 0 | NotSubmitted | Documents not yet submitted |
| 1 | Pending | Submitted, awaiting admin review |
| 2 | Approved | Admin approved |
| 3 | Rejected | Admin rejected |

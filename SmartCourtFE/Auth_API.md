---
title: Default module
language_tabs:
  - shell: Shell
  - http: HTTP
  - javascript: JavaScript
  - ruby: Ruby
  - python: Python
  - php: PHP
  - java: Java
  - go: Go
toc_footers: []
includes: []
search: true
code_clipboard: true
highlight_theme: darkula
headingLevel: 2
generator: "@tarslib/widdershins v4.0.30"

---

# Default module

Base URLs:

# Authentication

# Auth

## POST Login

POST /api/auth/login

Authenticates a user and issues an access token plus a seven-day refresh token. Authentication: Anonymous. Validation errors return HTTP 400. Authentication failures return HTTP 401. Forbidden accounts return HTTP 403.

> Body Parameters

```json
{
  "email": "user@example.com",
  "password": "stringst"
}
```

### Params

|Name|Location|Type|Required|Description|
|---|---|---|---|---|
|body|body|object| yes |none|
|» email|body|string(email)| yes |Required; must be a valid email (البريد الإلكتروني مطلوب. / البريد الإلكتروني غير صالح.).|
|» password|body|string| yes |Required; minimum 8 characters (كلمة المرور مطلوبة. / كلمة المرور يجب أن تكون 8 أحرف على الأقل.).|

> Response Examples

> 200 Response

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

### Responses

|HTTP Status Code |Meaning|Description|Data schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|User authenticated successfully.|Inline|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|Validation failure.|None|
|401|[Unauthorized](https://tools.ietf.org/html/rfc7235#section-3.1)|AuthenticationException('البريد الإلكتروني أو كلمة المرور غير صحيحة.')|None|
|403|[Forbidden](https://tools.ietf.org/html/rfc7231#section-6.5.3)|ForbiddenAccessException ('يرجى تأكيد البريد الإلكتروني أولاً' or 'تم تعليق حسابك. تواصل مع الدعم')|None|

### Responses Data Schema

## POST Register client

POST /api/auth/register/client

Creates an unverified client account and queues a confirmation email. Authentication: Anonymous.

> Body Parameters

```json
{
  "fullName": "string",
  "email": "user@example.com",
  "password": "stringst",
  "confirmPassword": "string"
}
```

### Params

|Name|Location|Type|Required|Description|
|---|---|---|---|---|
|body|body|object| yes |none|
|» fullName|body|string| yes |Required; 5-150 characters (الاسم الكامل مطلوب.).|
|» email|body|string(email)| yes |Required; valid email.|
|» password|body|string| yes |Required; at least 8 characters and must match lowercase, uppercase, and digit.|
|» confirmPassword|body|string| yes |Must equal password (تأكيد كلمة المرور غير مطابق. / كلمة المرور وتأكيد كلمة المرور غير متطابقتين.).|

> Response Examples

> 201 Response

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

### Responses

|HTTP Status Code |Meaning|Description|Data schema|
|---|---|---|---|
|201|[Created](https://tools.ietf.org/html/rfc7231#section-6.3.2)|Account created successfully.|Inline|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|Validation or identity creation failure.|None|
|500|[Internal Server Error](https://tools.ietf.org/html/rfc7231#section-6.6.1)|ConflictException('البريد الإلكتروني مسجل بالفعل.') or email queue failure.|None|

### Responses Data Schema

## POST Register lawyer

POST /api/auth/register/lawyer

Creates an unverified lawyer account from a multipart form and queues a confirmation email. Authentication: Anonymous.

> Body Parameters

same data of the clients 

```


#### Enum

|Name|Value|
|---|---|
|» gender|Male|
|» gender|Female|

> Response Examples

> 201 Response

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

### Responses

|HTTP Status Code |Meaning|Description|Data schema|
|---|---|---|---|
|201|[Created](https://tools.ietf.org/html/rfc7231#section-6.3.2)|Lawyer account created successfully.|Inline|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|Validation or identity creation failure.|None|
|500|[Internal Server Error](https://tools.ietf.org/html/rfc7231#section-6.6.1)|ConflictException('البريد الإلكتروني مسجل بالفعل.' or 'الرقم القومي مسجل بالفعل.')|None|

### Responses Data Schema

## POST Refresh access token

POST /api/auth/refresh

Rotates an active refresh token and returns a new access/refresh pair.

> Body Parameters

```json
{
  "refreshToken": "string"
}
```

### Params

|Name|Location|Type|Required|Description|
|---|---|---|---|---|
|body|body|object| yes |none|
|» refreshToken|body|string| yes |Required (رمز التحديث مطلوب.).|

> Response Examples

> 200 Response

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

### Responses

|HTTP Status Code |Meaning|Description|Data schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Token refreshed successfully.|Inline|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|Validation or identity update failure.|None|
|401|[Unauthorized](https://tools.ietf.org/html/rfc7235#section-3.1)|AuthenticationException('رمز التحديث غير صالح أو منتهي الصلاحية.')|None|

### Responses Data Schema

## POST Revoke refresh token

POST /api/auth/revoke

Validates the access token without checking its lifetime and revokes the supplied refresh token.

> Body Parameters

```json
{
  "token": "string",
  "refreshToken": "string"
}
```

### Params

|Name|Location|Type|Required|Description|
|---|---|---|---|---|
|body|body|object| yes |none|
|» token|body|string| yes |Required (رمز الوصول مطلوب.).|
|» refreshToken|body|string| yes |Required (رمز التحديث مطلوب.).|

> Response Examples

> 200 Response

```json
{
    "success": true,
    "data": true,
    "message": "تم إبطال رمز التحديث بنجاح.",
    "errors": null,
    "statusCode": 200
}
```

### Responses

|HTTP Status Code |Meaning|Description|Data schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Refresh token revoked.|Inline|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|BusinessException ('رمز الوصول غير صالح.' or 'رمز التحديث غير صالح.')|None|

### Responses Data Schema

## POST Change password

POST /api/auth/change-password

Changes the authenticated user's password and revokes all active refresh tokens. Rate limit: IP 20/15 minutes and authenticated user 5/15 minutes.

> Body Parameters

```json
{
  "currentPassword": "string",
  "newPassword": "stringst",
  "confirmNewPassword": "string"
}
```

### Params

|Name|Location|Type|Required|Description|
|---|---|---|---|---|
|body|body|object| yes |none|
|» currentPassword|body|string| yes |Required (كلمة المرور الحالية مطلوبة)|
|» newPassword|body|string| yes |Required; at least 8 characters; one lowercase, one uppercase, and one digit.|
|» confirmNewPassword|body|string| yes |Must equal newPassword (كلمة المرور وتأكيد كلمة المرور غير متطابقتين)|

> Response Examples

> 200 Response

```json
{
    "success": true,
    "message": "تم تغيير كلمة المرور بنجاح",
    "errors": null,
    "statusCode": 200
}
```

### Responses

|HTTP Status Code |Meaning|Description|Data schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Password changed successfully.|[ApiResponse](#schemaapiresponse)|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|Validation failure ('كلمة المرور الحالية غير صحيحة.' or identity errors).|None|
|401|[Unauthorized](https://tools.ietf.org/html/rfc7235#section-3.1)|AuthenticationException('المستخدم غير معروف')|None|
|429|[Too Many Requests](https://tools.ietf.org/html/rfc6585#section-4)|Rate limit exceeded.|None|

## GET Confirm email

GET /api/auth/confirm-email

Confirms an email address from the user id and Base64URL-encoded confirmation token. Rate limit: IP 20/15 minutes and account key 5/hour.

### Params

|Name|Location|Type|Required|Description|
|---|---|---|---|---|
|userId|query|string| no |User ID Guid|
|token|query|string| no |Base64URL-encoded token|

> Response Examples

> 200 Response

```json
{
    "success": true,
    "message": "تم تأكيد البريد الإلكتروني بنجاح.",
    "errors": null,
    "statusCode": 200
}
```

### Responses

|HTTP Status Code |Meaning|Description|Data schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Email confirmed successfully.|[ApiResponse](#schemaapiresponse)|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|BusinessException('رابط تأكيد البريد الإلكتروني غير صالح أو منتهي الصلاحية.')|None|
|429|[Too Many Requests](https://tools.ietf.org/html/rfc6585#section-4)|Rate limit exceeded.|None|

## POST Forgot password

POST /api/auth/forgot-password

Requests a password-reset email. Rate limit: IP 5/15 minutes and account key 3/hour.

> Body Parameters

```json
{
  "email": "user@example.com"
}
```

### Params

|Name|Location|Type|Required|Description|
|---|---|---|---|---|
|body|body|object| yes |none|
|» email|body|string(email)| yes |Required; valid email (عنوان البريد الإلكتروني مطلوب / عنوان البريد الإلكتروني غير صالح).|

> Response Examples

> 200 Response

```json
{
    "success": true,
    "message": "إذا كان البريد الإلكتروني مسجلاً، سيتم إرسال رابط إعادة تعيين كلمة المرور",
    "errors": null,
    "statusCode": 200
}
```

### Responses

|HTTP Status Code |Meaning|Description|Data schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Password reset email sent if eligible.|[ApiResponse](#schemaapiresponse)|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|Validation failure.|None|
|429|[Too Many Requests](https://tools.ietf.org/html/rfc6585#section-4)|Rate limit exceeded.|None|
|500|[Internal Server Error](https://tools.ietf.org/html/rfc7231#section-6.6.1)|Queue email failure.|None|

## POST Reset password

POST /api/auth/reset-password

Resets an eligible user's password using the Base64URL-encoded token and revokes all active refresh tokens. Rate limit: IP 10/15 minutes, account key 5/hour.

> Body Parameters

```json
{
  "email": "user@example.com",
  "token": "string",
  "newPassword": "stringst",
  "confirmNewPassword": "string"
}
```

### Params

|Name|Location|Type|Required|Description|
|---|---|---|---|---|
|body|body|object| yes |none|
|» email|body|string(email)| yes |none|
|» token|body|string¦null| no |none|
|» newPassword|body|string| yes |Required; at least 8 chars; uppercase, lowercase, digit.|
|» confirmNewPassword|body|string| yes |none|

> Response Examples

> 200 Response

```json
{
    "success": true,
    "message": "تم إعادة تعيين كلمة المرور بنجاح",
    "errors": null,
    "statusCode": 200
}
```

### Responses

|HTTP Status Code |Meaning|Description|Data schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Password reset successfully.|[ApiResponse](#schemaapiresponse)|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|BusinessException('رابط إعادة تعيين كلمة المرور غير صالح أو منتهي الصلاحية.')|None|
|429|[Too Many Requests](https://tools.ietf.org/html/rfc6585#section-4)|Rate limit exceeded.|None|

## POST Resend verification email

POST /api/auth/resend-verification

Resends a confirmation email. Unknown, already-confirmed, or non-Unverified accounts are treated as a successful no-op. Rate limit: IP 5/15 minutes, account key 1/minute and 3/hour.

> Body Parameters

```json
{
  "email": "user@example.com"
}
```

### Params

|Name|Location|Type|Required|Description|
|---|---|---|---|---|
|body|body|object| yes |none|
|» email|body|string(email)| yes |none|

> Response Examples

> 200 Response

```json
{
    "success": true,
    "message": "تم إرسال رابط التحقق مرة أخرى",
    "errors": null,
    "statusCode": 200
}
```

### Responses

|HTTP Status Code |Meaning|Description|Data schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Verification link sent.|[ApiResponse](#schemaapiresponse)|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|Validation failure.|None|
|429|[Too Many Requests](https://tools.ietf.org/html/rfc6585#section-4)|Rate limit exceeded.|None|
|500|[Internal Server Error](https://tools.ietf.org/html/rfc7231#section-6.6.1)|Queue email failure.|None|

# Users

## GET Get client profile

GET /api/clients/profile

Returns the profile for the authenticated client. Rate limit: IP 300/minute and user 120/minute.

> Response Examples

> 200 Response

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

### Responses

|HTTP Status Code |Meaning|Description|Data schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Client profile returned.|Inline|
|401|[Unauthorized](https://tools.ietf.org/html/rfc7235#section-3.1)|Unauthorized.|None|
|403|[Forbidden](https://tools.ietf.org/html/rfc7231#section-6.5.3)|Forbidden (role mismatch).|None|
|404|[Not Found](https://tools.ietf.org/html/rfc7231#section-6.5.4)|NotFoundException('الموكل غير موجود')|None|
|429|[Too Many Requests](https://tools.ietf.org/html/rfc6585#section-4)|Rate limit exceeded.|None|

### Responses Data Schema

## PUT Update client profile

PUT /api/clients/profile

Updates the authenticated client's phone number, date of birth, and address. Rate limit: IP 60/15 minutes and user 20/15 minutes.

> Body Parameters

```json
{
  "phoneNumber": "string",
  "dateOfBirth": "2019-08-24",
  "address": "string"
}
```

### Params

|Name|Location|Type|Required|Description|
|---|---|---|---|---|
|body|body|object| yes |none|
|» phoneNumber|body|string| yes |Egyptian phone number|
|» dateOfBirth|body|string(date)| yes |Must be earlier than today|
|» address|body|string¦null| no |none|

> Response Examples

> 200 Response

```json
{
    "success": true,
    "message": "تم تحديث الملف الشخصي بنجاح.",
    "errors": null,
    "statusCode": 200
}
```

### Responses

|HTTP Status Code |Meaning|Description|Data schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Client profile updated.|[ApiResponse](#schemaapiresponse)|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|Validation or identity update failure.|None|
|404|[Not Found](https://tools.ietf.org/html/rfc7231#section-6.5.4)|NotFoundException('الموكل غير موجود')|None|
|429|[Too Many Requests](https://tools.ietf.org/html/rfc6585#section-4)|Rate limit exceeded.|None|

## DELETE Delete client profile

DELETE /api/clients/profile

Soft-deletes the authenticated client, revokes active refresh tokens, and updates the security stamp. Rate limit: IP 10/day and user 3/day.

> Body Parameters

```json
{
  "currentPassword": "string"
}
```

### Params

|Name|Location|Type|Required|Description|
|---|---|---|---|---|
|body|body|object| yes |none|
|» currentPassword|body|string| yes |Required (كلمة المرور الحالية مطلوبة.).|

> Response Examples

> 200 Response

```json
{
    "success": true,
    "message": "تم حذف الملف الشخصي بنجاح.",
    "errors": null,
    "statusCode": 200
}
```

### Responses

|HTTP Status Code |Meaning|Description|Data schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Account deleted.|[ApiResponse](#schemaapiresponse)|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|BusinessException('كلمة المرور الحالية غير صحيحة.')|None|
|429|[Too Many Requests](https://tools.ietf.org/html/rfc6585#section-4)|Rate limit exceeded.|None|

## GET Get lawyer profile

GET /api/lawyers/profile

Returns the authenticated lawyer's private profile. Rate limit: IP 300/minute and user 120/minute.

> Response Examples

> 200 Response

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

### Responses

|HTTP Status Code |Meaning|Description|Data schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Lawyer profile returned.|Inline|
|401|[Unauthorized](https://tools.ietf.org/html/rfc7235#section-3.1)|Unauthorized.|None|
|403|[Forbidden](https://tools.ietf.org/html/rfc7231#section-6.5.3)|Forbidden.|None|
|404|[Not Found](https://tools.ietf.org/html/rfc7231#section-6.5.4)|NotFoundException('المحامي غير موجود')|None|
|429|[Too Many Requests](https://tools.ietf.org/html/rfc6585#section-4)|Rate limit exceeded.|None|

### Responses Data Schema

## PUT Update lawyer profile

PUT /api/lawyers/profile

Updates the authenticated lawyer's contact, specialization, experience, level, biography, and address. Rate limit: IP 60/15 minutes and user 20/15 minutes.

> Body Parameters

```json
{
  "phoneNumber": "string",
  "dateOfBirth": "2019-08-24",
  "specializationId": "eba2e9ae-3dcc-4db2-900a-0e35606de355",
  "yearsOfExperience": 50,
  "level": 1,
  "bio": "string",
  "address": "string"
}
```

### Params

|Name|Location|Type|Required|Description|
|---|---|---|---|---|
|body|body|object| yes |none|
|» phoneNumber|body|string| yes |none|
|» dateOfBirth|body|string(date)| yes |none|
|» specializationId|body|string(uuid)| yes |none|
|» yearsOfExperience|body|integer| yes |none|
|» level|body|integer| yes |1=GeneralRegistration, 2=PrimaryCourt, 3=AppealCourt, 4=CassationCourt|
|» bio|body|string¦null| no |none|
|» address|body|string¦null| no |none|

#### Enum

|Name|Value|
|---|---|
|» level|1|
|» level|2|
|» level|3|
|» level|4|

> Response Examples

> 200 Response

```json
{
    "success": true,
    "message": "تم تحديث البيانات بنجاح",
    "errors": null,
    "statusCode": 200
}
```

### Responses

|HTTP Status Code |Meaning|Description|Data schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Lawyer profile updated.|[ApiResponse](#schemaapiresponse)|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|Validation or identity error ('مستوى المحامي غير صالح.' or 'التخصص غير صالح.').|None|
|404|[Not Found](https://tools.ietf.org/html/rfc7231#section-6.5.4)|NotFoundException('المحامي غير موجود')|None|
|429|[Too Many Requests](https://tools.ietf.org/html/rfc6585#section-4)|Rate limit exceeded.|None|

## DELETE Delete lawyer profile

DELETE /api/lawyers/profile

Soft-deletes the authenticated lawyer, marks the lawyer unavailable, revokes active refresh tokens, and updates the security stamp. Rate limit: IP 10/day and user 3/day.

> Body Parameters

```json
{
  "currentPassword": "string"
}
```

### Params

|Name|Location|Type|Required|Description|
|---|---|---|---|---|
|body|body|object| yes |none|
|» currentPassword|body|string| yes |none|

> Response Examples

> 200 Response

```json
{
    "success": true,
    "message": "تم حذف الحساب بنجاح",
    "errors": null,
    "statusCode": 200
}
```

### Responses

|HTTP Status Code |Meaning|Description|Data schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Account deleted.|[ApiResponse](#schemaapiresponse)|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|BusinessException('كلمة المرور الحالية غير صحيحة.')|None|
|429|[Too Many Requests](https://tools.ietf.org/html/rfc6585#section-4)|Rate limit exceeded.|None|

## GET Get public lawyer profile

GET /api/lawyers/public/{id}

Returns a public profile only when the user is a lawyer with a current lawyer profile, confirmed email, and Active status. Anonymous. Rate limit: IP 120/minute.

### Params

|Name|Location|Type|Required|Description|
|---|---|---|---|---|
|id|path|string(uuid)| yes |Lawyer GUID|

> Response Examples

> 200 Response

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

### Responses

|HTTP Status Code |Meaning|Description|Data schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Public lawyer profile returned.|Inline|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|Invalid route GUID.|None|
|404|[Not Found](https://tools.ietf.org/html/rfc7231#section-6.5.4)|NotFoundException('المحامي غير موجود')|None|
|429|[Too Many Requests](https://tools.ietf.org/html/rfc6585#section-4)|Rate limit exceeded.|None|

### Responses Data Schema

# Admin verifications

## GET List pending verifications

GET /api/admin/verifications

Returns a paginated list of lawyers whose current verification documents match the optional status filter. Requires Admin role.

### Params

|Name|Location|Type|Required|Description|
|---|---|---|---|---|
|pageNumber|query|integer| no |none|
|pageSize|query|integer| no |none|
|search|query|string| no |none|
|status|query|integer| no |1=Pending, 2=Verified, 3=Rejected, 4=Expired|

#### Enum

|Name|Value|
|---|---|
|status|1|
|status|2|
|status|3|
|status|4|

> Response Examples

> 200 Response

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

### Responses

|HTTP Status Code |Meaning|Description|Data schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Paginated list of pending verifications.|Inline|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|Invalid query values ('Status must be a valid verification document status.')|None|

### Responses Data Schema

HTTP Status Code **200**

|Name|Type|Required|Restrictions|Title|description|
|---|---|---|---|---|---|
|» success|boolean|false|none||none|
|» data|[[PendingVerificationListItemDto](#schemapendingverificationlistitemdto)]|false|none||none|
|»» lawyerId|string(uuid)|false|none||none|
|»» fullName|string|false|none||none|
|»» email|string|false|none||none|
|»» phoneNumber|string¦null|false|none||none|
|»» pendingDocumentCount|integer|false|none||none|
|»» verifiedDocumentCount|integer|false|none||none|
|»» rejectedDocumentCount|integer|false|none||none|
|» message|string¦null|false|none||none|
|» errors|[string]¦null|false|none||none|
|» statusCode|integer|false|none||none|
|» pageNumber|integer|false|none||none|
|» pageSize|integer|false|none||none|
|» totalPages|integer|false|none||none|
|» totalRecords|integer|false|none||none|
|» hasNextPage|boolean|false|none||none|
|» hasPreviousPage|boolean|false|none||none|

## GET Get lawyer verification details

GET /api/admin/verifications/{lawyerId}

Returns the lawyer's current verification documents and account verification state. Requires Admin role.

### Params

|Name|Location|Type|Required|Description|
|---|---|---|---|---|
|lawyerId|path|string(uuid)| yes |none|

> Response Examples

> 200 Response

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

### Responses

|HTTP Status Code |Meaning|Description|Data schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Lawyer verification details.|Inline|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|Empty GUID validation error.|None|
|404|[Not Found](https://tools.ietf.org/html/rfc7231#section-6.5.4)|NotFoundException('Lawyer was not found.')|None|

### Responses Data Schema

## GET Download verification document

GET /api/admin/verifications/documents/{documentId}/content

Downloads the current verification document bytes. Requires Admin role.

### Params

|Name|Location|Type|Required|Description|
|---|---|---|---|---|
|documentId|path|string(uuid)| yes |none|

> Response Examples

> 200 Response

### Responses

|HTTP Status Code |Meaning|Description|Data schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Binary file stream.|string|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|Document id is required.|None|
|404|[Not Found](https://tools.ietf.org/html/rfc7231#section-6.5.4)|NotFoundException('Verification document was not found.')|None|

## PATCH Review verification document

PATCH /api/admin/verifications/documents/{documentId}

Approves or rejects the current pending document and recalculates the lawyer account status. Requires Admin role.

> Body Parameters

```json
{
  "decision": 1,
  "rejectionReason": "string"
}
```

### Params

|Name|Location|Type|Required|Description|
|---|---|---|---|---|
|documentId|path|string(uuid)| yes |none|
|body|body|object| yes |none|
|» decision|body|integer| yes |1=Approve, 2=Reject|
|» rejectionReason|body|string¦null| no |Required when decision=2; must be empty when decision=1.|

#### Enum

|Name|Value|
|---|---|
|» decision|1|
|» decision|2|

> Response Examples

> 200 Response

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

### Responses

|HTTP Status Code |Meaning|Description|Data schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Review completed.|Inline|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|Validation error.|None|
|404|[Not Found](https://tools.ietf.org/html/rfc7231#section-6.5.4)|Verification document was not found.|None|
|409|[Conflict](https://tools.ietf.org/html/rfc7231#section-6.5.8)|Only pending documents can be reviewed / document has expired.|None|

### Responses Data Schema

# User verification documents

## POST Submit verification documents

POST /api/UserVerification/submit-verification-documents

Uploads one or more verification images for a user and returns per-file successes and failures. Anonymous controller action.

> Body Parameters

```yaml
userId: ""
documents[0].file: ""
documents[0].expirationDate: ""
documents[0].type: ""

```

### Params

|Name|Location|Type|Required|Description|
|---|---|---|---|---|
|body|body|object| yes |none|
|» userId|body|string(uuid)| yes |Required (UserId is required)|
|» documents[0].file|body|string(binary)| yes |JPEG, PNG, WEBP, HEIC, or HEIF image|
|» documents[0].expirationDate|body|string(date)| yes |Must be future date|
|» documents[0].type|body|integer| yes |1=NationalIdFront, 2=NationalIdBack, 3=BarAssociationCardFront, 4=BarAssociationCardBack, 5=other|

#### Enum

|Name|Value|
|---|---|
|» documents[0].type|1|
|» documents[0].type|2|
|» documents[0].type|3|
|» documents[0].type|4|
|» documents[0].type|5|

> Response Examples

> 200 Response

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

### Responses

|HTTP Status Code |Meaning|Description|Data schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Documents uploaded.|Inline|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|Validation or upload error.|None|

### Responses Data Schema

## GET Get user verification documents

GET /api/UserVerification/{UserId}

Lists all verification documents belonging to the supplied user id. Anonymous controller action.

### Params

|Name|Location|Type|Required|Description|
|---|---|---|---|---|
|UserId|path|string(uuid)| yes |none|

> Response Examples

> 200 Response

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

### Responses

|HTTP Status Code |Meaning|Description|Data schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|User verification documents returned.|Inline|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|User Id is required.|None|
|404|[Not Found](https://tools.ietf.org/html/rfc7231#section-6.5.4)|The specified user does not exist.|None|

### Responses Data Schema

## DELETE Delete verification document

DELETE /api/UserVerification

Deletes a user's verification document from storage and the database. Anonymous controller action.

### Params

|Name|Location|Type|Required|Description|
|---|---|---|---|---|
|userId|query|string(uuid)| yes |none|
|documentId|query|string(uuid)| yes |none|

> Response Examples

> 200 Response

```json
{
    "success": true,
    "message": null,
    "errors": null,
    "statusCode": 200
}
```

### Responses

|HTTP Status Code |Meaning|Description|Data schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Document deleted.|[ApiResponse](#schemaapiresponse)|
|400|[Bad Request](https://tools.ietf.org/html/rfc7231#section-6.5.1)|Storage deletion failure or validation error.|None|
|404|[Not Found](https://tools.ietf.org/html/rfc7231#section-6.5.4)|User or verification document not found.|None|

# Health

## GET Ping

GET /api/Health/ping

Returns a live operational marker. This is the only controller response that is not wrapped in ApiResponse<T>.

> Response Examples

> 200 Response

```json
{
    "message": "Pong! Smart Court API is fully operational.",
    "serverTimeUtc": "2026-07-23T12:00:00Z",
    "version": "1.0.0"
}
```

### Responses

|HTTP Status Code |Meaning|Description|Data schema|
|---|---|---|---|
|200|[OK](https://tools.ietf.org/html/rfc7231#section-6.3.1)|Ping successful.|[PingResponse](#schemapingresponse)|

# Data Schema

<h2 id="tocS_ApiResponse">ApiResponse</h2>

<a id="schemaapiresponse"></a>
<a id="schema_ApiResponse"></a>
<a id="tocSapiresponse"></a>
<a id="tocsapiresponse"></a>

```json
{
  "success": true,
  "message": "string",
  "errors": [
    "string"
  ],
  "statusCode": 0
}

```

### Attribute

|Name|Type|Required|Restrictions|Title|Description|
|---|---|---|---|---|---|
|success|boolean|false|none||none|
|message|string¦null|false|none||none|
|errors|[string]¦null|false|none||none|
|statusCode|integer|false|none||none|

<h2 id="tocS_User">User</h2>

<a id="schemauser"></a>
<a id="schema_User"></a>
<a id="tocSuser"></a>
<a id="tocsuser"></a>

```json
{
  "id": "string",
  "email": "string",
  "fullName": "string",
  "role": "string"
}

```

### Attribute

|Name|Type|Required|Restrictions|Title|Description|
|---|---|---|---|---|---|
|id|string|false|none||none|
|email|string|false|none||none|
|fullName|string|false|none||none|
|role|string|false|none||none|

<h2 id="tocS_LoginResponse">LoginResponse</h2>

<a id="schemaloginresponse"></a>
<a id="schema_LoginResponse"></a>
<a id="tocSloginresponse"></a>
<a id="tocsloginresponse"></a>

```json
{
  "user": {
    "id": "string",
    "email": "string",
    "fullName": "string",
    "role": "string"
  },
  "accessToken": "string",
  "expiresIn": 0,
  "refreshToken": "string",
  "refreshTokenExpiration": "2019-08-24T14:15:22Z"
}

```

### Attribute

|Name|Type|Required|Restrictions|Title|Description|
|---|---|---|---|---|---|
|user|[User](#schemauser)|false|none||none|
|accessToken|string|false|none||none|
|expiresIn|integer|false|none||none|
|refreshToken|string|false|none||none|
|refreshTokenExpiration|string(date-time)|false|none||none|

<h2 id="tocS_RegisterResponse">RegisterResponse</h2>

<a id="schemaregisterresponse"></a>
<a id="schema_RegisterResponse"></a>
<a id="tocSregisterresponse"></a>
<a id="tocsregisterresponse"></a>

```json
{
  "userId": "string",
  "email": "string",
  "fullName": "string",
  "role": "string"
}

```

### Attribute

|Name|Type|Required|Restrictions|Title|Description|
|---|---|---|---|---|---|
|userId|string|false|none||none|
|email|string|false|none||none|
|fullName|string|false|none||none|
|role|string|false|none||none|

<h2 id="tocS_RefreshTokenResponse">RefreshTokenResponse</h2>

<a id="schemarefreshtokenresponse"></a>
<a id="schema_RefreshTokenResponse"></a>
<a id="tocSrefreshtokenresponse"></a>
<a id="tocsrefreshtokenresponse"></a>

```json
{
  "accessToken": "string",
  "refreshToken": "string",
  "expiresAt": "2019-08-24T14:15:22Z"
}

```

### Attribute

|Name|Type|Required|Restrictions|Title|Description|
|---|---|---|---|---|---|
|accessToken|string|false|none||none|
|refreshToken|string|false|none||none|
|expiresAt|string(date-time)|false|none||none|

<h2 id="tocS_ClientProfileResponse">ClientProfileResponse</h2>

<a id="schemaclientprofileresponse"></a>
<a id="schema_ClientProfileResponse"></a>
<a id="tocSclientprofileresponse"></a>
<a id="tocsclientprofileresponse"></a>

```json
{
  "id": "497f6eca-6276-4993-bfeb-53cbbbba6f08",
  "name": "string",
  "email": "string",
  "phoneNumber": "string",
  "gender": "string",
  "dateOfBirth": "2019-08-24",
  "address": "string",
  "status": "string"
}

```

### Attribute

|Name|Type|Required|Restrictions|Title|Description|
|---|---|---|---|---|---|
|id|string(uuid)|false|none||none|
|name|string|false|none||none|
|email|string|false|none||none|
|phoneNumber|string|false|none||none|
|gender|string|false|none||none|
|dateOfBirth|string(date)¦null|false|none||none|
|address|string¦null|false|none||none|
|status|string|false|none||none|

<h2 id="tocS_LawyerProfileResponse">LawyerProfileResponse</h2>

<a id="schemalawyerprofileresponse"></a>
<a id="schema_LawyerProfileResponse"></a>
<a id="tocSlawyerprofileresponse"></a>
<a id="tocslawyerprofileresponse"></a>

```json
{
  "id": "497f6eca-6276-4993-bfeb-53cbbbba6f08",
  "name": "string",
  "email": "string",
  "phoneNumber": "string",
  "nationalNumber": "string",
  "gender": "string",
  "dateOfBirth": "2019-08-24",
  "specializationId": "eba2e9ae-3dcc-4db2-900a-0e35606de355",
  "specializationName": "string",
  "categoryName": "string",
  "yearsOfExperience": 0,
  "level": 0,
  "bio": "string",
  "address": "string",
  "status": "string",
  "isAvailable": true,
  "profilePictureUrl": "string"
}

```

### Attribute

|Name|Type|Required|Restrictions|Title|Description|
|---|---|---|---|---|---|
|id|string(uuid)|false|none||none|
|name|string|false|none||none|
|email|string|false|none||none|
|phoneNumber|string|false|none||none|
|nationalNumber|string|false|none||none|
|gender|string|false|none||none|
|dateOfBirth|string(date)¦null|false|none||none|
|specializationId|string(uuid)¦null|false|none||none|
|specializationName|string|false|none||none|
|categoryName|string|false|none||none|
|yearsOfExperience|integer|false|none||none|
|level|integer|false|none||1=GeneralRegistration, 2=PrimaryCourt, 3=AppealCourt, 4=CassationCourt|
|bio|string¦null|false|none||none|
|address|string¦null|false|none||none|
|status|string|false|none||none|
|isAvailable|boolean|false|none||none|
|profilePictureUrl|string¦null|false|none||none|

<h2 id="tocS_PublicLawyerProfileResponse">PublicLawyerProfileResponse</h2>

<a id="schemapubliclawyerprofileresponse"></a>
<a id="schema_PublicLawyerProfileResponse"></a>
<a id="tocSpubliclawyerprofileresponse"></a>
<a id="tocspubliclawyerprofileresponse"></a>

```json
{
  "id": "497f6eca-6276-4993-bfeb-53cbbbba6f08",
  "name": "string",
  "gender": "string",
  "specializationId": "eba2e9ae-3dcc-4db2-900a-0e35606de355",
  "specializationName": "string",
  "categoryName": "string",
  "yearsOfExperience": 0,
  "level": 0,
  "bio": "string",
  "isAvailable": true,
  "profilePictureUrl": "string"
}

```

### Attribute

|Name|Type|Required|Restrictions|Title|Description|
|---|---|---|---|---|---|
|id|string(uuid)|false|none||none|
|name|string|false|none||none|
|gender|string|false|none||none|
|specializationId|string(uuid)¦null|false|none||none|
|specializationName|string|false|none||none|
|categoryName|string|false|none||none|
|yearsOfExperience|integer|false|none||none|
|level|integer|false|none||none|
|bio|string¦null|false|none||none|
|isAvailable|boolean|false|none||none|
|profilePictureUrl|string¦null|false|none||none|

<h2 id="tocS_PendingVerificationListItemDto">PendingVerificationListItemDto</h2>

<a id="schemapendingverificationlistitemdto"></a>
<a id="schema_PendingVerificationListItemDto"></a>
<a id="tocSpendingverificationlistitemdto"></a>
<a id="tocspendingverificationlistitemdto"></a>

```json
{
  "lawyerId": "dc3dc7a7-10f8-41ce-b6b7-d3eb4d5a5ec7",
  "fullName": "string",
  "email": "string",
  "phoneNumber": "string",
  "pendingDocumentCount": 0,
  "verifiedDocumentCount": 0,
  "rejectedDocumentCount": 0
}

```

### Attribute

|Name|Type|Required|Restrictions|Title|Description|
|---|---|---|---|---|---|
|lawyerId|string(uuid)|false|none||none|
|fullName|string|false|none||none|
|email|string|false|none||none|
|phoneNumber|string¦null|false|none||none|
|pendingDocumentCount|integer|false|none||none|
|verifiedDocumentCount|integer|false|none||none|
|rejectedDocumentCount|integer|false|none||none|

<h2 id="tocS_VerificationDocumentDetailsDto">VerificationDocumentDetailsDto</h2>

<a id="schemaverificationdocumentdetailsdto"></a>
<a id="schema_VerificationDocumentDetailsDto"></a>
<a id="tocSverificationdocumentdetailsdto"></a>
<a id="tocsverificationdocumentdetailsdto"></a>

```json
{
  "documentId": "4704590c-004e-410d-adf7-acb7ca0a7052",
  "documentType": "string",
  "status": "string",
  "fileName": "string",
  "contentType": "string",
  "expirationDate": "2019-08-24",
  "reviewedAt": "2019-08-24T14:15:22Z",
  "rejectionReason": "string",
  "contentUrl": "string"
}

```

### Attribute

|Name|Type|Required|Restrictions|Title|Description|
|---|---|---|---|---|---|
|documentId|string(uuid)|false|none||none|
|documentType|string|false|none||none|
|status|string|false|none||none|
|fileName|string|false|none||none|
|contentType|string|false|none||none|
|expirationDate|string(date)|false|none||none|
|reviewedAt|string(date-time)¦null|false|none||none|
|rejectionReason|string¦null|false|none||none|
|contentUrl|string|false|none||none|

<h2 id="tocS_VerificationDetailsDto">VerificationDetailsDto</h2>

<a id="schemaverificationdetailsdto"></a>
<a id="schema_VerificationDetailsDto"></a>
<a id="tocSverificationdetailsdto"></a>
<a id="tocsverificationdetailsdto"></a>

```json
{
  "lawyerId": "dc3dc7a7-10f8-41ce-b6b7-d3eb4d5a5ec7",
  "fullName": "string",
  "email": "string",
  "phoneNumber": "string",
  "accountStatus": "string",
  "isFullyVerified": true,
  "documents": [
    {
      "documentId": "4704590c-004e-410d-adf7-acb7ca0a7052",
      "documentType": "string",
      "status": "string",
      "fileName": "string",
      "contentType": "string",
      "expirationDate": "2019-08-24",
      "reviewedAt": "2019-08-24T14:15:22Z",
      "rejectionReason": "string",
      "contentUrl": "string"
    }
  ]
}

```

### Attribute

|Name|Type|Required|Restrictions|Title|Description|
|---|---|---|---|---|---|
|lawyerId|string(uuid)|false|none||none|
|fullName|string|false|none||none|
|email|string|false|none||none|
|phoneNumber|string¦null|false|none||none|
|accountStatus|string|false|none||none|
|isFullyVerified|boolean|false|none||none|
|documents|[[VerificationDocumentDetailsDto](#schemaverificationdocumentdetailsdto)]|false|none||none|

<h2 id="tocS_ReviewVerificationDocumentResponse">ReviewVerificationDocumentResponse</h2>

<a id="schemareviewverificationdocumentresponse"></a>
<a id="schema_ReviewVerificationDocumentResponse"></a>
<a id="tocSreviewverificationdocumentresponse"></a>
<a id="tocsreviewverificationdocumentresponse"></a>

```json
{
  "documentId": "4704590c-004e-410d-adf7-acb7ca0a7052",
  "documentStatus": "string",
  "lawyerAccountStatus": "string",
  "isFullyVerified": true
}

```

### Attribute

|Name|Type|Required|Restrictions|Title|Description|
|---|---|---|---|---|---|
|documentId|string(uuid)|false|none||none|
|documentStatus|string|false|none||none|
|lawyerAccountStatus|string|false|none||none|
|isFullyVerified|boolean|false|none||none|

<h2 id="tocS_UploadedDocumentDto">UploadedDocumentDto</h2>

<a id="schemauploadeddocumentdto"></a>
<a id="schema_UploadedDocumentDto"></a>
<a id="tocSuploadeddocumentdto"></a>
<a id="tocsuploadeddocumentdto"></a>

```json
{
  "fileName": "string",
  "type": 0
}

```

### Attribute

|Name|Type|Required|Restrictions|Title|Description|
|---|---|---|---|---|---|
|fileName|string|false|none||none|
|type|integer|false|none||none|

<h2 id="tocS_DocumentUploadErrorDto">DocumentUploadErrorDto</h2>

<a id="schemadocumentuploaderrordto"></a>
<a id="schema_DocumentUploadErrorDto"></a>
<a id="tocSdocumentuploaderrordto"></a>
<a id="tocsdocumentuploaderrordto"></a>

```json
{
  "fileName": "string",
  "type": 0,
  "error": "string"
}

```

### Attribute

|Name|Type|Required|Restrictions|Title|Description|
|---|---|---|---|---|---|
|fileName|string|false|none||none|
|type|integer|false|none||none|
|error|string|false|none||none|

<h2 id="tocS_SubmitVerificationDocumentResponseDto">SubmitVerificationDocumentResponseDto</h2>

<a id="schemasubmitverificationdocumentresponsedto"></a>
<a id="schema_SubmitVerificationDocumentResponseDto"></a>
<a id="tocSsubmitverificationdocumentresponsedto"></a>
<a id="tocssubmitverificationdocumentresponsedto"></a>

```json
{
  "uploadedDocuments": [
    {
      "fileName": "string",
      "type": 0
    }
  ],
  "failedDocuments": [
    {
      "fileName": "string",
      "type": 0,
      "error": "string"
    }
  ]
}

```

### Attribute

|Name|Type|Required|Restrictions|Title|Description|
|---|---|---|---|---|---|
|uploadedDocuments|[[UploadedDocumentDto](#schemauploadeddocumentdto)]|false|none||none|
|failedDocuments|[[DocumentUploadErrorDto](#schemadocumentuploaderrordto)]|false|none||none|

<h2 id="tocS_UserVerificationDocumentDto">UserVerificationDocumentDto</h2>

<a id="schemauserverificationdocumentdto"></a>
<a id="schema_UserVerificationDocumentDto"></a>
<a id="tocSuserverificationdocumentdto"></a>
<a id="tocsuserverificationdocumentdto"></a>

```json
{
  "documentId": "4704590c-004e-410d-adf7-acb7ca0a7052",
  "documentType": 0,
  "status": 0,
  "expirationDate": "2019-08-24",
  "isCurrent": true,
  "fileName": "string"
}

```

### Attribute

|Name|Type|Required|Restrictions|Title|Description|
|---|---|---|---|---|---|
|documentId|string(uuid)|false|none||none|
|documentType|integer|false|none||none|
|status|integer|false|none||none|
|expirationDate|string(date)|false|none||none|
|isCurrent|boolean|false|none||none|
|fileName|string|false|none||none|

<h2 id="tocS_GetUserVerificationDocumentsResponseDto">GetUserVerificationDocumentsResponseDto</h2>

<a id="schemagetuserverificationdocumentsresponsedto"></a>
<a id="schema_GetUserVerificationDocumentsResponseDto"></a>
<a id="tocSgetuserverificationdocumentsresponsedto"></a>
<a id="tocsgetuserverificationdocumentsresponsedto"></a>

```json
{
  "documents": [
    {
      "documentId": "4704590c-004e-410d-adf7-acb7ca0a7052",
      "documentType": 0,
      "status": 0,
      "expirationDate": "2019-08-24",
      "isCurrent": true,
      "fileName": "string"
    }
  ]
}

```

### Attribute

|Name|Type|Required|Restrictions|Title|Description|
|---|---|---|---|---|---|
|documents|[[UserVerificationDocumentDto](#schemauserverificationdocumentdto)]|false|none||none|

<h2 id="tocS_PingResponse">PingResponse</h2>

<a id="schemapingresponse"></a>
<a id="schema_PingResponse"></a>
<a id="tocSpingresponse"></a>
<a id="tocspingresponse"></a>

```json
{
  "message": "Pong! Smart Court API is fully operational.",
  "serverTimeUtc": "2026-07-23T12:00:00Z",
  "version": "1.0.0"
}

```

### Attribute

|Name|Type|Required|Restrictions|Title|Description|
|---|---|---|---|---|---|
|message|string|true|none||none|
|serverTimeUtc|string(date-time)|true|none||none|
|version|string|true|none||none|


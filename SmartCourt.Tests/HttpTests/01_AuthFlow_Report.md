# Authentication Flow Test Report

### 1. Register Client - Missing FullName

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
  "Email": "client_auth_1307866123@test.com",
  "ConfirmPassword": "Password123!",
  "Password": "Password123!"
}
``n
**Response Status:** Error

**Response Body:**
No connection could be made because the target machine actively refused it.
---


### 2. Register Client - Missing Email

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
  "FullName": "Test Client",
  "Password": "Password123!",
  "ConfirmPassword": "Password123!"
}
``n
**Response Status:** Error

**Response Body:**
No connection could be made because the target machine actively refused it.
---


### 3. Register Client - Invalid Email Format

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
  "Email": "invalid_email",
  "FullName": "Test Client",
  "Password": "Password123!",
  "ConfirmPassword": "Password123!"
}
``n
**Response Status:** Error

**Response Body:**
No connection could be made because the target machine actively refused it.
---


### 4. Register Client - Weak Password

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
  "Email": "client_auth_1307866123@test.com",
  "FullName": "Test Client",
  "Password": "password",
  "ConfirmPassword": "password"
}
``n
**Response Status:** 400

**Response Body:**
`json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Password": [
      "كلمة المرور يجب أن تحتوي على حرف كبير وحرف صغير ورقم."
    ]
  },
  "traceId": "00-633d652b2130686dbd17c53239962840-1598d06b31eb176d-00"
}
``n---


### 5. Register Client - Mismatched ConfirmPassword

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
  "Email": "client_auth_1307866123@test.com",
  "FullName": "Test Client",
  "Password": "Password123!",
  "ConfirmPassword": "Password1234!"
}
``n
**Response Status:** 400

**Response Body:**
`json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "ConfirmPassword": [
      "تأكيد كلمة المرور غير مطابق."
    ]
  },
  "traceId": "00-f0b0601689e2bcda18d4948b1f82c1b0-7b73f1d358ba7b57-00"
}
``n---


### 6. Register Client - Valid Data

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
  "Email": "client_auth_1307866123@test.com",
  "FullName": "Test Client",
  "Password": "Password123!",
  "ConfirmPassword": "Password123!"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "98088b31-fe4e-4133-ad2f-08def48e3341",
    "email": "client_auth_1307866123@test.com",
    "fullName": "Test Client",
    "role": "Client"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


### 7. Login Client - Unconfirmed Email

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "client_auth_1307866123@test.com",
  "Password": "Password123!"
}
``n
**Response Status:** 403

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "يرجى تأكيد البريد الإلكتروني أولاً",
  "errors": null,
  "statusCode": 403
}
``n---


Found confirmation URL for client_auth_1307866123@test.com: http://localhost:5173/verify-email?userId=98088b31-fe4e-4133-ad2f-08def48e3341&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI4RnVMMlB3RmdON09DWU1RTUt0TGNMb1NwcXh0a29lenh5VVZIdnVHK3BuWjhiR2xRbGpRZHI4YkRUL0t3V1JvYmlGZ01UZEdqSitvSE52UGdJNjRab1V0cDNVRlMyNERrTHFVR29BVU43RmprT0NKQ0llR3hIaWxyc09ib1BhRkNTdlIxQ2JRVTcwSGZEdVFYd3FIMnc2RU41cEFISmFtU1VBcHZVVm9CNXB5SktRNy83NzlSREhxL2NIcFBiV3JXTnFWK09IMHd5VDZFV2MzbnY1ejVYRWU4enlXYW9PbXhPVytDZzNkNi9QZz09

### Confirm Email for client_auth_1307866123@test.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=98088b31-fe4e-4133-ad2f-08def48e3341&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI4RnVMMlB3RmdON09DWU1RTUt0TGNMb1NwcXh0a29lenh5VVZIdnVHK3BuWjhiR2xRbGpRZHI4YkRUL0t3V1JvYmlGZ01UZEdqSitvSE52UGdJNjRab1V0cDNVRlMyNERrTHFVR29BVU43RmprT0NKQ0llR3hIaWxyc09ib1BhRkNTdlIxQ2JRVTcwSGZEdVFYd3FIMnc2RU41cEFISmFtU1VBcHZVVm9CNXB5SktRNy83NzlSREhxL2NIcFBiV3JXTnFWK09IMHd5VDZFV2MzbnY1ejVYRWU4enlXYW9PbXhPVytDZzNkNi9QZz09

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "message": "تم تأكيد البريد الإلكتروني بنجاح.",
  "errors": null,
  "statusCode": 200
}
``n---


### 9. Login Client - Confirmed Email

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "client_auth_1307866123@test.com",
  "Password": "Password123!"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "98088b31-fe4e-4133-ad2f-08def48e3341",
      "email": "client_auth_1307866123@test.com",
      "fullName": "Test Client",
      "role": "Client",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5ODA4OGIzMS1mZTRlLTQxMzMtYWQyZi0wOGRlZjQ4ZTMzNDEiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6Ijk4MDg4YjMxLWZlNGUtNDEzMy1hZDJmLTA4ZGVmNDhlMzM0MSIsImVtYWlsIjoiY2xpZW50X2F1dGhfMTMwNzg2NjEyM0B0ZXN0LmNvbSIsIm5hbWUiOiJUZXN0IENsaWVudCIsInNlY3VyaXR5X3N0YW1wIjoiVE1WVVNGVjZLWjJWUE5LWVhSN082UFRFVlJDWTRLUU8iLCJqdGkiOiI2ZTk4NzM5Zi04NGZjLTQ4MjQtYmE2YS1lOTFmNDc2OWE4MDkiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDbGllbnQiLCJuYmYiOjE3ODYxMTIwNzMsImV4cCI6MTc4NjExMjk3MywiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.aJfs0j0KILoGCZup22-aLYPelaoDjg5a-6Il2gNMvBw",
    "expiresIn": 900,
    "refreshToken": "V8LSjIQjtct0YbWHxKmLYMewU3+Djk/l2p8UndGVjBn2cIQiOaU7TNdNBJhW6Uw2YJZXibhOaQ9557xINePWxA==",
    "refreshTokenExpiration": "2026-08-14T14:14:33.2432776Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---



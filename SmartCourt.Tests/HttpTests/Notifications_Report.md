# Notifications HTTP Test Report

Generated at: 2026-08-09 14:02:52 +03:00


## Health and unauthenticated access

### Health check

**Request:** GET http://localhost:5049/health

**Response Status:** 200

**Response Body:**
Healthy
---


- [PASS] **API is healthy** (status=200)
### Feed requires authentication

**Request:** GET http://localhost:5049/api/notifications

**Response Status:** 401

**Response Body:** (Empty)
---


- [PASS] **Feed requires authentication** (status=401)
### Unread count requires authentication

**Request:** GET http://localhost:5049/api/notifications/unread-count

**Response Status:** 401

**Response Body:** (Empty)
---


- [PASS] **Unread count requires authentication** (status=401)
### Mark one requires authentication

**Request:** PATCH http://localhost:5049/api/notifications/1418f28c-c11a-4876-957a-7b26688b6a4f/read

**Response Status:** 401

**Response Body:** (Empty)
---


- [PASS] **Mark one requires authentication** (status=401)
### Mark all requires authentication

**Request:** PATCH http://localhost:5049/api/notifications/read-all

**Response Status:** 401

**Response Body:** (Empty)
---


- [PASS] **Mark all requires authentication** (status=401)
### SignalR negotiate requires authentication

**Request:** POST http://localhost:5049/hubs/notifications/negotiate?negotiateVersion=1

**Response Status:** 401

**Response Body:** (Empty)
---


- [PASS] **SignalR negotiate requires authentication** (status=401)
### Malformed bearer token

**Request:** GET http://localhost:5049/api/notifications

**Response Status:** 401

**Response Body:** (Empty)
---


- [PASS] **Malformed bearer token returns 401** (status=401)

## Zero-assumption account and domain setup

### Register notification client

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
  "FullName": "Notification Client",
  "ConfirmPassword": "Password123!",
  "Password": "Password123!",
  "Email": "notifications_client_20260809140252502@example.com"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "e46868de-b3e2-46c2-f423-08def605af24",
    "email": "notifications_client_20260809140252502@example.com",
    "fullName": "Notification Client",
    "role": "Client"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


- [PASS] **Register notification client** (status=201)
Found confirmation URL for notifications_client_20260809140252502@example.com: http://localhost:5173/verify-email?userId=e46868de-b3e2-46c2-f423-08def605af24&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIrNE5yTnhNdGJvdldBY3JOeUVJZjI5UmJrMFJXS0NmN0RFZE1XamdQcGNucHNPNFk4SzR3b3RZZDZzTHNYNVJXNG9wVFJXd1R6dWFlVy91NTEzTjhuT3hqRXdoTUhGTlFJeVpWOFVvaDMxTC8vdUJId0hVeDJVYW5pdGErZmRaekd2R3ltSHRQelFYaU14VC8wK3VGU3ZSTDNkUHFidmVzdUhSZnN5b1J4cmkvYjlYOTdQZkhpVzI3Q0lOT0pCd054NnBKcE9hVFVncnQzUW5tWVpvM20rN25lNDNvWG1OMDI3ZytiK2c3dDNJQT09

### Confirm Email for notifications_client_20260809140252502@example.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=e46868de-b3e2-46c2-f423-08def605af24&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIrNE5yTnhNdGJvdldBY3JOeUVJZjI5UmJrMFJXS0NmN0RFZE1XamdQcGNucHNPNFk4SzR3b3RZZDZzTHNYNVJXNG9wVFJXd1R6dWFlVy91NTEzTjhuT3hqRXdoTUhGTlFJeVpWOFVvaDMxTC8vdUJId0hVeDJVYW5pdGErZmRaekd2R3ltSHRQelFYaU14VC8wK3VGU3ZSTDNkUHFidmVzdUhSZnN5b1J4cmkvYjlYOTdQZkhpVzI3Q0lOT0pCd054NnBKcE9hVFVncnQzUW5tWVpvM20rN25lNDNvWG1OMDI3ZytiK2c3dDNJQT09

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


### Login notification client

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "notifications_client_20260809140252502@example.com",
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
      "id": "e46868de-b3e2-46c2-f423-08def605af24",
      "email": "notifications_client_20260809140252502@example.com",
      "fullName": "Notification Client",
      "role": "Client",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJlNDY4NjhkZS1iM2UyLTQ2YzItZjQyMy0wOGRlZjYwNWFmMjQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImU0Njg2OGRlLWIzZTItNDZjMi1mNDIzLTA4ZGVmNjA1YWYyNCIsImVtYWlsIjoibm90aWZpY2F0aW9uc19jbGllbnRfMjAyNjA4MDkxNDAyNTI1MDJAZXhhbXBsZS5jb20iLCJuYW1lIjoiTm90aWZpY2F0aW9uIENsaWVudCIsInNlY3VyaXR5X3N0YW1wIjoiMlIzTTRSV05YVVVJRktBVlNDVEZFWFJINUpJRDVCV1UiLCJqdGkiOiI2NzViNzEyYi01YWE3LTQwODMtODU1OC0zZTEzMjk0MjY0ZTMiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDbGllbnQiLCJuYmYiOjE3ODYyNzMzNzMsImV4cCI6MTc4NjI3NDI3MywiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.ZFJx-mMzNaN7OBvkquF1vulepzN_RxWg_aKBu7L0W4E",
    "expiresIn": 900,
    "refreshToken": "E+oFnH/vpiSOAxIXkFHj54oQdB6EBVQcZ6ISLpybQO1pH0a84MfjUJ9eJ+Zqa3dIcHJOVltwCL+7ON8cSPBk3w==",
    "refreshTokenExpiration": "2026-08-16T11:02:53.6257269Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Login notification client** (status=200)
### Complete notification client profile

**Request:** POST http://localhost:5049/api/clients/profile/complete

**Body:**
`json
{
  "DateOfBirth": "1990-01-01",
  "Gender": 1,
  "NationalNumber": "29080926220645",
  "Address": "Cairo",
  "PhoneNumber": "+201098112563"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "message": "تم استكمال الملف الشخصي بنجاح.",
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Complete notification client profile** (status=200)
### Register notification lawyer

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
`json
{
  "FullName": "Notification Lawyer",
  "ConfirmPassword": "Password123!",
  "Password": "Password123!",
  "Email": "notifications_lawyer_20260809140252502@example.com"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "72bcdd83-2604-48d9-f424-08def605af24",
    "email": "notifications_lawyer_20260809140252502@example.com",
    "fullName": "Notification Lawyer",
    "role": "Lawyer"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


- [PASS] **Register notification lawyer** (status=201)
Found confirmation URL for notifications_lawyer_20260809140252502@example.com: http://localhost:5173/verify-email?userId=72bcdd83-2604-48d9-f424-08def605af24&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIrTTZuUEpaUUkwQmJJN1JESTJBUStZL2tIL2t6ZFNTNjV5bjdjbDNsc0ZjT2pJc3RrOTd2RkptR1VEcTkzL1BsUVRCYnJZalVidEYwWFRvejRFVnlRemg4QlpIa1Vvd1grM0I3TDZ1dkRUNDhlc3pHQ2g4cGlUOHBtaG5WNnZoWGdEajYzTkdFSE8zNnYzTlVDd0V2MXpjZDhrZllEaW05QU8yWTdMU2ZJZGdJK0lNeGp2VDFDRFh4M3JrTVpqOFc0WkFubGlJNmEyZzhYVnoxY3hYd1ljclUxTnpWZkl2eTV3OEZ4ZTRHV1hOZz09

### Confirm Email for notifications_lawyer_20260809140252502@example.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=72bcdd83-2604-48d9-f424-08def605af24&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIrTTZuUEpaUUkwQmJJN1JESTJBUStZL2tIL2t6ZFNTNjV5bjdjbDNsc0ZjT2pJc3RrOTd2RkptR1VEcTkzL1BsUVRCYnJZalVidEYwWFRvejRFVnlRemg4QlpIa1Vvd1grM0I3TDZ1dkRUNDhlc3pHQ2g4cGlUOHBtaG5WNnZoWGdEajYzTkdFSE8zNnYzTlVDd0V2MXpjZDhrZllEaW05QU8yWTdMU2ZJZGdJK0lNeGp2VDFDRFh4M3JrTVpqOFc0WkFubGlJNmEyZzhYVnoxY3hYd1ljclUxTnpWZkl2eTV3OEZ4ZTRHV1hOZz09

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


### Login notification lawyer

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "notifications_lawyer_20260809140252502@example.com",
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
      "id": "72bcdd83-2604-48d9-f424-08def605af24",
      "email": "notifications_lawyer_20260809140252502@example.com",
      "fullName": "Notification Lawyer",
      "role": "Lawyer",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3MmJjZGQ4My0yNjA0LTQ4ZDktZjQyNC0wOGRlZjYwNWFmMjQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjcyYmNkZDgzLTI2MDQtNDhkOS1mNDI0LTA4ZGVmNjA1YWYyNCIsImVtYWlsIjoibm90aWZpY2F0aW9uc19sYXd5ZXJfMjAyNjA4MDkxNDAyNTI1MDJAZXhhbXBsZS5jb20iLCJuYW1lIjoiTm90aWZpY2F0aW9uIExhd3llciIsInNlY3VyaXR5X3N0YW1wIjoiVUFPMkFGNVJFQ1o1VFlFUUVLWERZR0ZFVkFCQjdCQ1IiLCJqdGkiOiJjMjA3MzI5My1kMTE4LTRjNGQtYjg0NS05ZjU5OGRkZGI4N2IiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODYyNzMzNzYsImV4cCI6MTc4NjI3NDI3NiwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.eXsHPKtuqskCOUzzbSHkFQIW_I2neQsPRkuEATGd59s",
    "expiresIn": 900,
    "refreshToken": "+e5YtTvV8vskxpG38a70lAPgShCk/54SKTafTwi/9lT1D6BJKo/shJn8Dnap9k2U1JxZisNjOl1ZfwmamZ6X7A==",
    "refreshTokenExpiration": "2026-08-16T11:02:56.8762889Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Login notification lawyer** (status=200)
### Complete notification lawyer profile

**Request:** POST http://localhost:5049/api/lawyers/profile/complete

**Body:**
`json
{
  "Specializations": [
    {
      "Specialization": 1,
      "YearsOfExperience": 5,
      "CasesHandled": 10
    }
  ],
  "Gender": 1,
  "Level": 1,
  "Address": "Cairo",
  "DateOfBirth": "1985-01-01",
  "Bio": "Notification lifecycle test lawyer",
  "NationalNumber": "28080926885557",
  "PhoneNumber": "+201137963964"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "message": "تم استكمال البيانات بنجاح",
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Complete notification lawyer profile** (status=200)
### Login admin for account approval

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "admin@smartcourt.com",
  "Password": "Admin@123"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "a39b6312-19c2-49f7-fe42-08def48c9663",
      "email": "admin@smartcourt.com",
      "fullName": "System Administrator",
      "role": "Admin",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhMzliNjMxMi0xOWMyLTQ5ZjctZmU0Mi0wOGRlZjQ4Yzk2NjMiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImEzOWI2MzEyLTE5YzItNDlmNy1mZTQyLTA4ZGVmNDhjOTY2MyIsImVtYWlsIjoiYWRtaW5Ac21hcnRjb3VydC5jb20iLCJuYW1lIjoiU3lzdGVtIEFkbWluaXN0cmF0b3IiLCJzZWN1cml0eV9zdGFtcCI6IkI0N09OTkw1V05BVUoyMzVMUlhIVTZOUVMyUEZPWkNRIiwianRpIjoiOTMwNjc3MGQtYmNjNy00ZjQyLWJhYmYtMGY0OTllZTg5NjNkIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJuYmYiOjE3ODYyNzMzNzgsImV4cCI6MTc4NjI3NDI3OCwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.kGfSYmOHsXxEdiW1gQUn7CQeX8iRO8ym380OwX_5ax8",
    "expiresIn": 900,
    "refreshToken": "rMf1vQfbelgjjeLXolBt2T1z6xtWX321F7JH1jhFxH4AFwyw+MXHHPyY/rcCJXlia1rGGNeRrVvUNugEvGfLDg==",
    "refreshTokenExpiration": "2026-08-16T11:02:58.6804455Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Login admin for account approval** (status=200)
### Approve notification client

**Request:** PATCH http://localhost:5049/api/admin/verifications/e46868de-b3e2-46c2-f423-08def605af24/approve-account

**Body:**
`json
{}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "message": "تم اعتماد بيانات الحساب بنجاح"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Approve notification client** (status=200)
### Approve notification lawyer

**Request:** PATCH http://localhost:5049/api/admin/verifications/72bcdd83-2604-48d9-f424-08def605af24/approve-account

**Body:**
`json
{}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "message": "تم اعتماد بيانات الحساب بنجاح"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Approve notification lawyer** (status=200)
### Re-login approved client

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "notifications_client_20260809140252502@example.com",
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
      "id": "e46868de-b3e2-46c2-f423-08def605af24",
      "email": "notifications_client_20260809140252502@example.com",
      "fullName": "Notification Client",
      "role": "Client",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJlNDY4NjhkZS1iM2UyLTQ2YzItZjQyMy0wOGRlZjYwNWFmMjQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImU0Njg2OGRlLWIzZTItNDZjMi1mNDIzLTA4ZGVmNjA1YWYyNCIsImVtYWlsIjoibm90aWZpY2F0aW9uc19jbGllbnRfMjAyNjA4MDkxNDAyNTI1MDJAZXhhbXBsZS5jb20iLCJuYW1lIjoiTm90aWZpY2F0aW9uIENsaWVudCIsInNlY3VyaXR5X3N0YW1wIjoiVUNTUlY0QzNRT1RUREJZMjVOS0pXQTdKRUdQUzVBSUIiLCJqdGkiOiJiZjlmZTkzOC1kYjk5LTRjZmUtOGViZC0wMzAyNWVmYzA1NDgiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDbGllbnQiLCJuYmYiOjE3ODYyNzMzNzksImV4cCI6MTc4NjI3NDI3OSwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.CbA-TguZUbvERGSy-A797DE2qH4DZ_4Jncz-Jwlivn8",
    "expiresIn": 900,
    "refreshToken": "FXmh0TC7Z6e7tv/TFevqlsAP59fWiJK2JOqXmZdUkT2fev8H+ax41wYdY08V0ywwFdvrvxBA0gtX9JYkCo61gQ==",
    "refreshTokenExpiration": "2026-08-16T11:02:59.8465163Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Re-login approved client** (status=200)
### Re-login approved lawyer

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "notifications_lawyer_20260809140252502@example.com",
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
      "id": "72bcdd83-2604-48d9-f424-08def605af24",
      "email": "notifications_lawyer_20260809140252502@example.com",
      "fullName": "Notification Lawyer",
      "role": "Lawyer",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3MmJjZGQ4My0yNjA0LTQ4ZDktZjQyNC0wOGRlZjYwNWFmMjQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjcyYmNkZDgzLTI2MDQtNDhkOS1mNDI0LTA4ZGVmNjA1YWYyNCIsImVtYWlsIjoibm90aWZpY2F0aW9uc19sYXd5ZXJfMjAyNjA4MDkxNDAyNTI1MDJAZXhhbXBsZS5jb20iLCJuYW1lIjoiTm90aWZpY2F0aW9uIExhd3llciIsInNlY3VyaXR5X3N0YW1wIjoiVk1DUU5CUlFHRFNSREZHUks1VkxDWTdDV0lWSjVFU0EiLCJqdGkiOiIzZTUzMzQ4Yy0yZDgwLTRlOWUtODhmZC1iNDI5ZTMyOTY0N2UiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODYyNzMzODAsImV4cCI6MTc4NjI3NDI4MCwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.U5rZ_MCHmEgUWyzAklD5LyXR34JSiDtGF6hr9j3hGG8",
    "expiresIn": 900,
    "refreshToken": "64OqCkTVzcPfrTSZncoNXvcqGgqqm5m54Kpt2qhbWRdnMQnBzYxZygiPd+WB3Het42BWsKieDpvKIM3d8RXvBQ==",
    "refreshTokenExpiration": "2026-08-16T11:03:00.3812321Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Re-login approved lawyer** (status=200)
### Authenticated admin may access personal empty inbox

**Request:** GET http://localhost:5049/api/notifications

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "items": [],
    "nextCursor": null,
    "unreadCount": 0
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Notification API has no artificial role restriction** (status=200)
### Authenticated SignalR negotiate

**Request:** POST http://localhost:5049/hubs/notifications/negotiate?negotiateVersion=1

**Response Status:** 200

**Response Body:**
`json
{
  "negotiateVersion": 1,
  "connectionId": "lKKBLI_wr-7zlQKNSN0opg",
  "connectionToken": "bVQFU2axYEnvQh5HdVP3Fw",
  "availableTransports": [
    {
      "transport": "WebSockets",
      "transferFormats": [
        "Text",
        "Binary"
      ]
    },
    {
      "transport": "ServerSentEvents",
      "transferFormats": [
        "Text"
      ]
    },
    {
      "transport": "LongPolling",
      "transferFormats": [
        "Text",
        "Binary"
      ]
    }
  ]
}
``n---


- [PASS] **Authenticated SignalR hub negotiation succeeds** (status=200)
### Create case for notification lifecycle

**Request:** POST http://localhost:5049/api/Case

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "caseId": "f28f2f37-36c0-4e84-ac23-e68221001c14",
    "failedDocuments": []
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
``n---


- [PASS] **Create case for notification lifecycle** (status=200)
### Review notification lifecycle case

**Request:** POST http://localhost:5049/api/cases/f28f2f37-36c0-4e84-ac23-e68221001c14/review

**Body:**
`json
{}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "8716ea03-d419-4b6e-9eee-468c98d46383",
    "caseId": "f28f2f37-36c0-4e84-ac23-e68221001c14",
    "isLatest": true,
    "createdAt": "2026-08-09T11:03:04.0745103Z",
    "reviewPoints": [
      {
        "id": "6cb833be-cf26-4b37-9f54-08cf50e812e6",
        "description": "تتمثل نقطة القوة الأساسية في صياغة الموضوع بوضوح حول 'Notification lifecycle case 20260809140252502'، وتوافر السند المبدئي الذي يرجح كفة الموكل في إثبات أصل الالتزام وتفوقه إثباتياً على الخصم.",
        "type": "Strength"
      },
      {
        "id": "5fbc955c-0f10-456d-966c-4014ceeb022e",
        "description": "ميزة الخصم تتمثل في غياب التوثيق الرسمي للتنبيهات أو الإخطارات المتبادلة بين الأطراف، مما يتيح له إنكار الاستلام أو الدفع بالتراخي في المطالبة.",
        "type": "Weakness"
      },
      {
        "id": "741dab3c-4d53-4c57-a8d6-f25742e54aea",
        "description": "يحتاج الملف إلى استيفاء النقاط والمعلومات التالية لضمان صياغة صحيفة الدعوى بشكل مكتمل: حصر وتفصيص المبالغ المالية المطلوبة والتعويضات الدقيقة عن الضرر المادي والمعنوي، وإدراج التواريخ الرسمية الدقيقة لبدء النزاع وتاريخ الإخلال بالتعهدات.",
        "type": "MissingCaseInfo"
      },
      {
        "id": "5b2e82e3-20d8-4e0f-83f9-7e585c34b77d",
        "description": "المستندات المحددة المطلوبة لإكمال الملف: أصل العقد/الاتفاق المبرم، صورة بطاقة الرقم القومي سارية لكل أطراف الدعوى، إيصالات التحويل أو السداد المالي، وأي إنذارات رسمية على يد محضر.",
        "type": "MissingCaseDoc"
      },
      {
        "id": "76c59bd3-8205-45b9-9983-35ae672ea9a0",
        "description": "قم بإعادة هيكلة وصف القضية في صورة جدول زمني متسلسل، يبدأ من تاريخ التعهد الأول، مروراً بتاريخ الإخلال، وصولاً إلى حجم الأضرار المترتبة حالياً.",
        "type": "Suggestion"
      },
      {
        "id": "315a425c-ef52-4cd9-aaf7-f76de0d855e1",
        "description": "قم بتفقيط وقسمة كافة المطالبات المالية إلى بنود مستقلة (أصل الدين، الفوائد أو التعويض عن المماطلة، والرسوم) وتوثيق كل بند بسند كتابي مستقل.",
        "type": "Suggestion"
      },
      {
        "id": "4f16873c-2e68-4cbe-91a6-db50f8ee631d",
        "description": "قم بتنظيم وثائق الملف في مجلد مرتب حسب التاريخ، وتأكد من مسح الأوراق ضوئياً بدقة عالية لضمان سهولة الإسناد والفحص القضائي.",
        "type": "Suggestion"
      }
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Review notification lifecycle case** (status=200)
### Finalize notification lifecycle case

**Request:** POST http://localhost:5049/api/Case/f28f2f37-36c0-4e84-ac23-e68221001c14/finalize

**Body:**
`json
{}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "caseId": "f28f2f37-36c0-4e84-ac23-e68221001c14",
    "totalEligibleLawyers": 0,
    "recommendations": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Finalize notification lifecycle case** (status=200)

## Proposal-created and proposal-rejected notification lifecycle

### Create proposal that will be rejected

**Request:** POST http://localhost:5049/api/proposals

**Body:**
`json
{
  "LawyerUserId": "72bcdd83-2604-48d9-f424-08def605af24",
  "Message": "Notification HTTP lifecycle proposal 140332114",
  "LegalCaseId": "f28f2f37-36c0-4e84-ac23-e68221001c14"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "b5a4ffdc-6bd2-42f8-9eac-290b68e0585d",
    "legalCaseId": "f28f2f37-36c0-4e84-ac23-e68221001c14",
    "caseTitle": "Notification lifecycle case 20260809140252502",
    "clientUserId": "e46868de-b3e2-46c2-f423-08def605af24",
    "clientName": "Notification Client",
    "lawyerUserId": "72bcdd83-2604-48d9-f424-08def605af24",
    "lawyerName": "Notification Lawyer",
    "message": "Notification HTTP lifecycle proposal 140332114",
    "status": "Pending",
    "decisionReason": null,
    "createdAt": "2026-08-09T11:03:32.2434927",
    "respondedAt": null,
    "updatedAt": "2026-08-09T11:03:32.2434927",
    "conversationId": null
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
``n---


- [PASS] **Create proposal that will be rejected** (status=200)
### Poll lawyer inbox for proposal.created

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "items": [],
    "nextCursor": null,
    "unreadCount": 0
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Poll lawyer inbox for proposal.created

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "6a97a3d5-2a2e-4d20-b6c7-e167c8834a93",
        "type": "proposal.created",
        "severity": "Information",
        "title": "New proposal",
        "body": "A client sent you a new proposal.",
        "actionUrl": "/proposals/b5a4ffdc-6bd2-42f8-9eac-290b68e0585d",
        "data": {
          "proposalId": "b5a4ffdc-6bd2-42f8-9eac-290b68e0585d",
          "legalCaseId": "f28f2f37-36c0-4e84-ac23-e68221001c14"
        },
        "createdAtUtc": "2026-08-09T11:03:32.2893185",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 1
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Lawyer receives durable proposal.created** 
- [PASS] **Created payload contract** 
### Reject first proposal

**Request:** POST http://localhost:5049/api/proposals/b5a4ffdc-6bd2-42f8-9eac-290b68e0585d/reject

**Body:**
`json
{
  "Reason": "Unable to take this matter during the requested period."
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "b5a4ffdc-6bd2-42f8-9eac-290b68e0585d",
    "legalCaseId": "f28f2f37-36c0-4e84-ac23-e68221001c14",
    "caseTitle": "Notification lifecycle case 20260809140252502",
    "clientUserId": "e46868de-b3e2-46c2-f423-08def605af24",
    "clientName": "Notification Client",
    "lawyerUserId": "72bcdd83-2604-48d9-f424-08def605af24",
    "lawyerName": "Notification Lawyer",
    "message": "Notification HTTP lifecycle proposal 140332114",
    "status": "Rejected",
    "decisionReason": "Unable to take this matter during the requested period.",
    "createdAt": "2026-08-09T11:03:32.2434927",
    "respondedAt": "2026-08-09T11:03:33.8349741",
    "updatedAt": "2026-08-09T11:03:33.8349741",
    "conversationId": null
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Reject first proposal** (status=200)
### Poll client inbox for proposal.rejected

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "items": [],
    "nextCursor": null,
    "unreadCount": 0
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Poll client inbox for proposal.rejected

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "a5e5fb6f-64aa-429a-a35e-36f8caf4d33e",
        "type": "proposal.rejected",
        "severity": "Warning",
        "title": "Proposal rejected",
        "body": "A lawyer rejected your proposal.",
        "actionUrl": "/proposals/b5a4ffdc-6bd2-42f8-9eac-290b68e0585d",
        "data": {
          "proposalId": "b5a4ffdc-6bd2-42f8-9eac-290b68e0585d",
          "legalCaseId": "f28f2f37-36c0-4e84-ac23-e68221001c14"
        },
        "createdAtUtc": "2026-08-09T11:03:33.8359384",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 1
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Client receives durable proposal.rejected** 
- [PASS] **Rejected severity is Warning** 

## Proposal-accepted lifecycle and cursor pagination

### Create proposal that will be accepted

**Request:** POST http://localhost:5049/api/proposals

**Body:**
`json
{
  "LawyerUserId": "72bcdd83-2604-48d9-f424-08def605af24",
  "Message": "Notification HTTP lifecycle proposal 140334953",
  "LegalCaseId": "f28f2f37-36c0-4e84-ac23-e68221001c14"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "16b8dfd0-7a88-4e45-b4da-c96f8ea5e907",
    "legalCaseId": "f28f2f37-36c0-4e84-ac23-e68221001c14",
    "caseTitle": "Notification lifecycle case 20260809140252502",
    "clientUserId": "e46868de-b3e2-46c2-f423-08def605af24",
    "clientName": "Notification Client",
    "lawyerUserId": "72bcdd83-2604-48d9-f424-08def605af24",
    "lawyerName": "Notification Lawyer",
    "message": "Notification HTTP lifecycle proposal 140334953",
    "status": "Pending",
    "decisionReason": null,
    "createdAt": "2026-08-09T11:03:35.2147959",
    "respondedAt": null,
    "updatedAt": "2026-08-09T11:03:35.2147959",
    "conversationId": null
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
``n---


- [PASS] **Create proposal that will be accepted** (status=200)
### Poll lawyer inbox for second proposal.created

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "6a97a3d5-2a2e-4d20-b6c7-e167c8834a93",
        "type": "proposal.created",
        "severity": "Information",
        "title": "New proposal",
        "body": "A client sent you a new proposal.",
        "actionUrl": "/proposals/b5a4ffdc-6bd2-42f8-9eac-290b68e0585d",
        "data": {
          "proposalId": "b5a4ffdc-6bd2-42f8-9eac-290b68e0585d",
          "legalCaseId": "f28f2f37-36c0-4e84-ac23-e68221001c14"
        },
        "createdAtUtc": "2026-08-09T11:03:32.2893185",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 1
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Poll lawyer inbox for second proposal.created

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "02f924de-86f5-4f66-85b4-2cf614a56745",
        "type": "proposal.created",
        "severity": "Information",
        "title": "New proposal",
        "body": "A client sent you a new proposal.",
        "actionUrl": "/proposals/16b8dfd0-7a88-4e45-b4da-c96f8ea5e907",
        "data": {
          "proposalId": "16b8dfd0-7a88-4e45-b4da-c96f8ea5e907",
          "legalCaseId": "f28f2f37-36c0-4e84-ac23-e68221001c14"
        },
        "createdAtUtc": "2026-08-09T11:03:35.2150817",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "6a97a3d5-2a2e-4d20-b6c7-e167c8834a93",
        "type": "proposal.created",
        "severity": "Information",
        "title": "New proposal",
        "body": "A client sent you a new proposal.",
        "actionUrl": "/proposals/b5a4ffdc-6bd2-42f8-9eac-290b68e0585d",
        "data": {
          "proposalId": "b5a4ffdc-6bd2-42f8-9eac-290b68e0585d",
          "legalCaseId": "f28f2f37-36c0-4e84-ac23-e68221001c14"
        },
        "createdAtUtc": "2026-08-09T11:03:32.2893185",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 2
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Lawyer receives second proposal.created** 
### Accept second proposal

**Request:** POST http://localhost:5049/api/proposals/16b8dfd0-7a88-4e45-b4da-c96f8ea5e907/accept

**Body:**
`json
{}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "16b8dfd0-7a88-4e45-b4da-c96f8ea5e907",
    "legalCaseId": "f28f2f37-36c0-4e84-ac23-e68221001c14",
    "caseTitle": "Notification lifecycle case 20260809140252502",
    "clientUserId": "e46868de-b3e2-46c2-f423-08def605af24",
    "clientName": "Notification Client",
    "lawyerUserId": "72bcdd83-2604-48d9-f424-08def605af24",
    "lawyerName": "Notification Lawyer",
    "message": "Notification HTTP lifecycle proposal 140334953",
    "status": "Accepted",
    "decisionReason": null,
    "createdAt": "2026-08-09T11:03:35.2147959",
    "respondedAt": "2026-08-09T11:03:36.8069767",
    "updatedAt": "2026-08-09T11:03:36.8069767",
    "conversationId": "09025f05-c598-4a60-8d3e-98e24dca342d"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Accept second proposal** (status=200)
### Poll client inbox for proposal.accepted

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "a5e5fb6f-64aa-429a-a35e-36f8caf4d33e",
        "type": "proposal.rejected",
        "severity": "Warning",
        "title": "Proposal rejected",
        "body": "A lawyer rejected your proposal.",
        "actionUrl": "/proposals/b5a4ffdc-6bd2-42f8-9eac-290b68e0585d",
        "data": {
          "proposalId": "b5a4ffdc-6bd2-42f8-9eac-290b68e0585d",
          "legalCaseId": "f28f2f37-36c0-4e84-ac23-e68221001c14"
        },
        "createdAtUtc": "2026-08-09T11:03:33.8359384",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 1
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Poll client inbox for proposal.accepted

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "9445a928-8589-46e8-b5be-5a7f9a2b9c54",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "Proposal accepted",
        "body": "A lawyer accepted your proposal.",
        "actionUrl": "/proposals/16b8dfd0-7a88-4e45-b4da-c96f8ea5e907",
        "data": {
          "proposalId": "16b8dfd0-7a88-4e45-b4da-c96f8ea5e907",
          "legalCaseId": "f28f2f37-36c0-4e84-ac23-e68221001c14"
        },
        "createdAtUtc": "2026-08-09T11:03:36.9210957",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "a5e5fb6f-64aa-429a-a35e-36f8caf4d33e",
        "type": "proposal.rejected",
        "severity": "Warning",
        "title": "Proposal rejected",
        "body": "A lawyer rejected your proposal.",
        "actionUrl": "/proposals/b5a4ffdc-6bd2-42f8-9eac-290b68e0585d",
        "data": {
          "proposalId": "b5a4ffdc-6bd2-42f8-9eac-290b68e0585d",
          "legalCaseId": "f28f2f37-36c0-4e84-ac23-e68221001c14"
        },
        "createdAtUtc": "2026-08-09T11:03:33.8359384",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 2
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Client receives durable proposal.accepted** 
- [PASS] **Accepted severity is Success** 
### Lawyer feed first cursor page

**Request:** GET http://localhost:5049/api/notifications?pageSize=1&isRead=false

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "02f924de-86f5-4f66-85b4-2cf614a56745",
        "type": "proposal.created",
        "severity": "Information",
        "title": "New proposal",
        "body": "A client sent you a new proposal.",
        "actionUrl": "/proposals/16b8dfd0-7a88-4e45-b4da-c96f8ea5e907",
        "data": {
          "proposalId": "16b8dfd0-7a88-4e45-b4da-c96f8ea5e907",
          "legalCaseId": "f28f2f37-36c0-4e84-ac23-e68221001c14"
        },
        "createdAtUtc": "2026-08-09T11:03:35.2150817",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": "djE6OTc",
    "unreadCount": 2
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **First cursor page has one item and nextCursor** 
### Lawyer feed second cursor page

**Request:** GET http://localhost:5049/api/notifications?pageSize=1&isRead=false&cursor=djE6OTc

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "6a97a3d5-2a2e-4d20-b6c7-e167c8834a93",
        "type": "proposal.created",
        "severity": "Information",
        "title": "New proposal",
        "body": "A client sent you a new proposal.",
        "actionUrl": "/proposals/b5a4ffdc-6bd2-42f8-9eac-290b68e0585d",
        "data": {
          "proposalId": "b5a4ffdc-6bd2-42f8-9eac-290b68e0585d",
          "legalCaseId": "f28f2f37-36c0-4e84-ac23-e68221001c14"
        },
        "createdAtUtc": "2026-08-09T11:03:32.2893185",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 2
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Second cursor page returns a different item** 

## Ownership, read state, and idempotency

### Client cannot mutate lawyer notification

**Request:** PATCH http://localhost:5049/api/notifications/6a97a3d5-2a2e-4d20-b6c7-e167c8834a93/read

**Response Status:** 404

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "Entity \"Notification\" (6a97a3d5-2a2e-4d20-b6c7-e167c8834a93) was not found.",
  "errors": null,
  "statusCode": 404
}
``n---


- [PASS] **Cross-user notification is hidden as 404** (status=404)
### Mark accepted notification read

**Request:** PATCH http://localhost:5049/api/notifications/9445a928-8589-46e8-b5be-5a7f9a2b9c54/read

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "9445a928-8589-46e8-b5be-5a7f9a2b9c54",
    "type": "proposal.accepted",
    "severity": "Success",
    "title": "Proposal accepted",
    "body": "A lawyer accepted your proposal.",
    "actionUrl": "/proposals/16b8dfd0-7a88-4e45-b4da-c96f8ea5e907",
    "data": {
      "proposalId": "16b8dfd0-7a88-4e45-b4da-c96f8ea5e907",
      "legalCaseId": "f28f2f37-36c0-4e84-ac23-e68221001c14"
    },
    "createdAtUtc": "2026-08-09T11:03:36.9210957",
    "readAtUtc": "2026-08-09T11:03:40.3727118Z",
    "expiresAtUtc": null
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Mark one read succeeds** (status=200)
### Repeat mark accepted notification read

**Request:** PATCH http://localhost:5049/api/notifications/9445a928-8589-46e8-b5be-5a7f9a2b9c54/read

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "9445a928-8589-46e8-b5be-5a7f9a2b9c54",
    "type": "proposal.accepted",
    "severity": "Success",
    "title": "Proposal accepted",
    "body": "A lawyer accepted your proposal.",
    "actionUrl": "/proposals/16b8dfd0-7a88-4e45-b4da-c96f8ea5e907",
    "data": {
      "proposalId": "16b8dfd0-7a88-4e45-b4da-c96f8ea5e907",
      "legalCaseId": "f28f2f37-36c0-4e84-ac23-e68221001c14"
    },
    "createdAtUtc": "2026-08-09T11:03:36.9210957",
    "readAtUtc": "2026-08-09T11:03:40.3727118",
    "expiresAtUtc": null
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Repeated mark-read preserves timestamp** 
### Fetch read-only feed

**Request:** GET http://localhost:5049/api/notifications?isRead=true&pageSize=50

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "9445a928-8589-46e8-b5be-5a7f9a2b9c54",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "Proposal accepted",
        "body": "A lawyer accepted your proposal.",
        "actionUrl": "/proposals/16b8dfd0-7a88-4e45-b4da-c96f8ea5e907",
        "data": {
          "proposalId": "16b8dfd0-7a88-4e45-b4da-c96f8ea5e907",
          "legalCaseId": "f28f2f37-36c0-4e84-ac23-e68221001c14"
        },
        "createdAtUtc": "2026-08-09T11:03:36.9210957",
        "readAtUtc": "2026-08-09T11:03:40.3727118",
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 1
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Read filter contains accepted notification** 
### Fetch unread-only feed

**Request:** GET http://localhost:5049/api/notifications?isRead=false&pageSize=50

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "a5e5fb6f-64aa-429a-a35e-36f8caf4d33e",
        "type": "proposal.rejected",
        "severity": "Warning",
        "title": "Proposal rejected",
        "body": "A lawyer rejected your proposal.",
        "actionUrl": "/proposals/b5a4ffdc-6bd2-42f8-9eac-290b68e0585d",
        "data": {
          "proposalId": "b5a4ffdc-6bd2-42f8-9eac-290b68e0585d",
          "legalCaseId": "f28f2f37-36c0-4e84-ac23-e68221001c14"
        },
        "createdAtUtc": "2026-08-09T11:03:33.8359384",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 1
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Unread filter excludes accepted notification** 
### Get unread count before read-all

**Request:** GET http://localhost:5049/api/notifications/unread-count

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "unreadCount": 1
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Unread-count endpoint reconciles feed** 
### Mark all client notifications read

**Request:** PATCH http://localhost:5049/api/notifications/read-all

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "readAtUtc": "2026-08-09T11:03:42.5667787Z",
    "unreadCount": 0
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Read-all returns zero** 
### Repeat mark-all read

**Request:** PATCH http://localhost:5049/api/notifications/read-all

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "readAtUtc": "2026-08-09T11:03:43.1369531Z",
    "unreadCount": 0
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Repeated read-all is idempotent** 
### Get unread count after read-all

**Request:** GET http://localhost:5049/api/notifications/unread-count

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "unreadCount": 0
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [PASS] **Unread count remains zero** 

## Validation, type coercion, malicious input, and methods

### Page size below minimum

**Request:** GET http://localhost:5049/api/notifications?pageSize=0

**Response Status:** 400

**Response Body:**
`json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "PageSize": [
      "'Page Size' must be between 1 and 50. You entered 0."
    ]
  },
  "traceId": "00-cd10d57c97628267e3beaa2c3d221c68-3a7cca990a85a71e-00"
}
``n---


- [PASS] **Page size below minimum returns 400** (status=400)
### Page size above maximum

**Request:** GET http://localhost:5049/api/notifications?pageSize=51

**Response Status:** 400

**Response Body:**
`json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "PageSize": [
      "'Page Size' must be between 1 and 50. You entered 51."
    ]
  },
  "traceId": "00-837915b63bd0bdf9deb21e319c5a1bb6-4e7749f30a37b128-00"
}
``n---


- [PASS] **Page size above maximum returns 400** (status=400)
### Page size wrong type

**Request:** GET http://localhost:5049/api/notifications?pageSize=abc

**Response Status:** 400

**Response Body:**
`json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "PageSize": [
      "The value 'abc' is not valid for PageSize."
    ]
  },
  "traceId": "00-e7d16d9a365801e50fc99a2d0e3bb614-d0d7415ea5e79dd7-00"
}
``n---


- [PASS] **Page size wrong type returns 400** (status=400)
### Boolean wrong type

**Request:** GET http://localhost:5049/api/notifications?isRead=banana

**Response Status:** 400

**Response Body:**
`json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "IsRead": [
      "The value 'banana' is not valid for IsRead."
    ]
  },
  "traceId": "00-9576c0c80979a0feeaf63e9e9d759493-6f875ef3f2130c1b-00"
}
``n---


- [PASS] **Boolean wrong type returns 400** (status=400)
### Malformed cursor

**Request:** GET http://localhost:5049/api/notifications?cursor=not-base64

**Response Status:** 400

**Response Body:**
`json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Cursor": [
      "Cursor is invalid or unsupported."
    ]
  },
  "traceId": "00-288bab09fa2c3e8790086f370efd291d-8c0ed4378ae0b97c-00"
}
``n---


- [PASS] **Malformed cursor returns 400** (status=400)
### Unicode cursor

**Request:** GET http://localhost:5049/api/notifications?cursor=%E2%9A%96%EF%B8%8F%20%D8%A5%D8%B4%D8%B9%D8%A7%D8%B1

**Response Status:** 400

**Response Body:**
`json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Cursor": [
      "Cursor is invalid or unsupported."
    ]
  },
  "traceId": "00-653d19d588e75e88e087f9739dd7e4f7-5f0b3f8ac7116733-00"
}
``n---


- [PASS] **Unicode cursor returns 400** (status=400)
### Oversized cursor

**Request:** GET http://localhost:5049/api/notifications?cursor=AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA

**Response Status:** 400

**Response Body:**
`json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Cursor": [
      "Cursor is invalid or unsupported."
    ]
  },
  "traceId": "00-457fd8b2f1d1f12a5ef04640012905bc-c42917ae70a8a3a3-00"
}
``n---


- [PASS] **Oversized cursor returns 400** (status=400)
### SQL-like cursor

**Request:** GET http://localhost:5049/api/notifications?cursor=%27%20OR%201%3D1%20--

**Response Status:** 400

**Response Body:**
`json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Cursor": [
      "Cursor is invalid or unsupported."
    ]
  },
  "traceId": "00-3c45a9cefea4a2b1ddf6c272548241b3-935abed2db1e3ba8-00"
}
``n---


- [PASS] **SQL-like cursor returns 400** (status=400)
### Unknown notification id

**Request:** PATCH http://localhost:5049/api/notifications/d3982342-9b8d-44f2-bbfa-57283cdd6eed/read

**Response Status:** 404

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "Entity \"Notification\" (d3982342-9b8d-44f2-bbfa-57283cdd6eed) was not found.",
  "errors": null,
  "statusCode": 404
}
``n---


- [PASS] **Unknown notification returns 404** (status=404)
### Non-Guid notification route

**Request:** PATCH http://localhost:5049/api/notifications/not-a-guid/read

**Response Status:** 404

**Response Body:** (Empty)
---


- [PASS] **Non-Guid route does not match** (status=404)
### Unsupported POST on feed

**Request:** POST http://localhost:5049/api/notifications

**Body:**
`json
{}
``n
**Response Status:** 405

**Response Body:** (Empty)
---


- [PASS] **Unsupported POST returns 405** (status=405)
### Unsupported DELETE on feed

**Request:** DELETE http://localhost:5049/api/notifications

**Response Status:** 405

**Response Body:** (Empty)
---


- [PASS] **Unsupported DELETE returns 405** (status=405)

## Execution summary


| Metric | Count |
|---|---:|
| Passed assertions | 57 |
| Failed assertions | 0 |


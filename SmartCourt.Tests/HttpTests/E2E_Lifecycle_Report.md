# Smart Court E2E Lifecycle Report

Generated at 2026-08-08 22:22:30



## Phase 1: Account and Profile Setup

### Register Client

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
  "Email": "e2e_client_20260808222230@example.com",
  "FullName": "E2E Client",
  "ConfirmPassword": "Password123!",
  "Password": "Password123!"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "f139c5a7-e7f5-4f8f-1f6d-08def5797003",
    "email": "e2e_client_20260808222230@example.com",
    "fullName": "E2E Client",
    "role": "Client"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


- [OK] **Register Client** 

Found confirmation URL for e2e_client_20260808222230@example.com: http://localhost:5173/verify-email?userId=f139c5a7-e7f5-4f8f-1f6d-08def5797003&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI4Wm5weHd4dEF4U0lWTG9vMXdDbExEdHAwK3greUxuNi9YaTRuQ0Y5aHZ5S0xwcTRXNlRaYkhsOWNKV2NxbHZhNHAvbzBLeTdvRzBiWXRRN203cjAzN3lrUFZjZ0s1em5HZlJUTEVmZTZLamJVQ0hQRDY3Qld5QVRlcmlLQU9JTmJIMzJMTWkwcEZKVjZ3U3FRaUpvRDdhQU1xeFNPQ2p0dm11MUF1azZyWXJuT2NndlN4R0V6eWl4QXAyU0UwN0xDUExFOE9HYmhhcmNxVnY1dmowOURERlFRZHpYN0xwYkN3OEQyMTNmZ3BLZz09

### Confirm Email for e2e_client_20260808222230@example.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=f139c5a7-e7f5-4f8f-1f6d-08def5797003&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI4Wm5weHd4dEF4U0lWTG9vMXdDbExEdHAwK3greUxuNi9YaTRuQ0Y5aHZ5S0xwcTRXNlRaYkhsOWNKV2NxbHZhNHAvbzBLeTdvRzBiWXRRN203cjAzN3lrUFZjZ0s1em5HZlJUTEVmZTZLamJVQ0hQRDY3Qld5QVRlcmlLQU9JTmJIMzJMTWkwcEZKVjZ3U3FRaUpvRDdhQU1xeFNPQ2p0dm11MUF1azZyWXJuT2NndlN4R0V6eWl4QXAyU0UwN0xDUExFOE9HYmhhcmNxVnY1dmowOURERlFRZHpYN0xwYkN3OEQyMTNmZ3BLZz09

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


### Login Client

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Password": "Password123!",
  "Email": "e2e_client_20260808222230@example.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "f139c5a7-e7f5-4f8f-1f6d-08def5797003",
      "email": "e2e_client_20260808222230@example.com",
      "fullName": "E2E Client",
      "role": "Client",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJmMTM5YzVhNy1lN2Y1LTRmOGYtMWY2ZC0wOGRlZjU3OTcwMDMiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImYxMzljNWE3LWU3ZjUtNGY4Zi0xZjZkLTA4ZGVmNTc5NzAwMyIsImVtYWlsIjoiZTJlX2NsaWVudF8yMDI2MDgwODIyMjIzMEBleGFtcGxlLmNvbSIsIm5hbWUiOiJFMkUgQ2xpZW50Iiwic2VjdXJpdHlfc3RhbXAiOiI1SzVMVk9MQ1RTTTRCTlo0RlZOS0hYSE1RTUtWMjM2WCIsImp0aSI6IjQzYTE2YjIxLWU4YjMtNDIxNy1iZTk5LTU2ZWNiMTM2ODQxOCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNsaWVudCIsIm5iZiI6MTc4NjIxNjk1MCwiZXhwIjoxNzg2MjE3ODUwLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.a4TnwWh5Xrm_DIk24jvr42ZccSVSFnMNnq--gyyVUFY",
    "expiresIn": 900,
    "refreshToken": "EZUuqkCnLCV7Ynh1UYPhnN+pfpOSpgKe6zJKy8jKGPSaS0UcdAHN9qdDdng2jUw0xEGjvWO7cc/BjTzkbRazlw==",
    "refreshTokenExpiration": "2026-08-15T19:22:30.9016592Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **Client Login Token** 

### Complete Client Profile

**Request:** POST http://localhost:5049/api/clients/profile/complete

**Body:**
`json
{
  "PhoneNumber": "+201011111111",
  "Gender": 1,
  "DateOfBirth": "1990-01-01",
  "NationalNumber": "29001015657735",
  "Address": "Cairo"
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


### Re-Login Client

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Password": "Password123!",
  "Email": "e2e_client_20260808222230@example.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "f139c5a7-e7f5-4f8f-1f6d-08def5797003",
      "email": "e2e_client_20260808222230@example.com",
      "fullName": "E2E Client",
      "role": "Client",
      "status": "PendingReview",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJmMTM5YzVhNy1lN2Y1LTRmOGYtMWY2ZC0wOGRlZjU3OTcwMDMiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImYxMzljNWE3LWU3ZjUtNGY4Zi0xZjZkLTA4ZGVmNTc5NzAwMyIsImVtYWlsIjoiZTJlX2NsaWVudF8yMDI2MDgwODIyMjIzMEBleGFtcGxlLmNvbSIsIm5hbWUiOiJFMkUgQ2xpZW50Iiwic2VjdXJpdHlfc3RhbXAiOiJZUFNUNVdGSkFUSkxJTTNOR0I3TUQzSFZQUlZOWTJHTiIsImp0aSI6ImUwMzMzYmQ2LTQxZTYtNGE2NS1iN2M2LTU0ZGQyNGY5N2QwZiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNsaWVudCIsIm5iZiI6MTc4NjIxNjk1MSwiZXhwIjoxNzg2MjE3ODUxLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.1raxXUyiiglRJldDxHhEnb1CRHgB6-nZ2Lwo-lfQiAI",
    "expiresIn": 900,
    "refreshToken": "Etu7XY6HVkc136HwvmFDBGOChwJsLm/IRT+NKmEJ5jkC4LO+T0Cydgwmtn2uPk9HT1JPZdmTws+4QYSn+Uy6AQ==",
    "refreshTokenExpiration": "2026-08-15T19:22:31.0796197Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Register Lawyer

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
`json
{
  "Email": "e2e_lawyer_20260808222230@example.com",
  "FullName": "E2E Lawyer",
  "ConfirmPassword": "Password123!",
  "Password": "Password123!"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "41ad4ba8-158e-4342-1f6e-08def5797003",
    "email": "e2e_lawyer_20260808222230@example.com",
    "fullName": "E2E Lawyer",
    "role": "Lawyer"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


- [OK] **Register Lawyer** 

Found confirmation URL for e2e_lawyer_20260808222230@example.com: http://localhost:5173/verify-email?userId=41ad4ba8-158e-4342-1f6e-08def5797003&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI4Y24yNERkN1g3THNnekQzRnpaOW1tYVZCcWRDSlJweXF2RDBEZTVwcm5NTFRjNlphQnoyaGZ5SEQ2QmlublE5UWFPcXpDREttTzBJMFc4elBZSlg3elk0eHZmZU9CaUJSMmtWWUpBNGhIaitzbC95a2JUa21PNlN6ZDh0Y2NCcnFJaUQ1WVJKL3BlVlFZTW8wVGN6VUJjQ3BQVmUwbnVTTnN6SVhxYW9LUGhVU1RVUzhQQzdpeVJCY1l3SFZRLyt5UmRLUWZ2UFNxcGh2aVJSMWt5L2JyVkFjd2d2OUpCQkJ1WVJlbEUzMWZ1UT09

### Confirm Email for e2e_lawyer_20260808222230@example.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=41ad4ba8-158e-4342-1f6e-08def5797003&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI4Y24yNERkN1g3THNnekQzRnpaOW1tYVZCcWRDSlJweXF2RDBEZTVwcm5NTFRjNlphQnoyaGZ5SEQ2QmlublE5UWFPcXpDREttTzBJMFc4elBZSlg3elk0eHZmZU9CaUJSMmtWWUpBNGhIaitzbC95a2JUa21PNlN6ZDh0Y2NCcnFJaUQ1WVJKL3BlVlFZTW8wVGN6VUJjQ3BQVmUwbnVTTnN6SVhxYW9LUGhVU1RVUzhQQzdpeVJCY1l3SFZRLyt5UmRLUWZ2UFNxcGh2aVJSMWt5L2JyVkFjd2d2OUpCQkJ1WVJlbEUzMWZ1UT09

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


### Login Lawyer

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Password": "Password123!",
  "Email": "e2e_lawyer_20260808222230@example.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "41ad4ba8-158e-4342-1f6e-08def5797003",
      "email": "e2e_lawyer_20260808222230@example.com",
      "fullName": "E2E Lawyer",
      "role": "Lawyer",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0MWFkNGJhOC0xNThlLTQzNDItMWY2ZS0wOGRlZjU3OTcwMDMiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjQxYWQ0YmE4LTE1OGUtNDM0Mi0xZjZlLTA4ZGVmNTc5NzAwMyIsImVtYWlsIjoiZTJlX2xhd3llcl8yMDI2MDgwODIyMjIzMEBleGFtcGxlLmNvbSIsIm5hbWUiOiJFMkUgTGF3eWVyIiwic2VjdXJpdHlfc3RhbXAiOiJUVUIyUUpCUFpDWFVTV0pNN1pPUFBVSU1FWTJJWldUSiIsImp0aSI6ImJjMWY2ZmNlLTk3NDgtNDMxMC1iMWYwLTc5OGJlMTA1ZmQ2NSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NjIxNjk1MSwiZXhwIjoxNzg2MjE3ODUxLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.trRnOpslULJnCAxZ_VfPTjEGB-jc4_PbwL_QkyfKsW4",
    "expiresIn": 900,
    "refreshToken": "GOq9LT5z9yWw8nPMwoeVbUFTBolEAQgpK0ujMUdE9G/xalE/5aN5vHhcFvIlUM5odMVyMU6hwACwx8drrpyNDw==",
    "refreshTokenExpiration": "2026-08-15T19:22:31.5729104Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **Lawyer Login Token** 

### Complete Lawyer Profile

**Request:** POST http://localhost:5049/api/lawyers/profile/complete

**Body:**
`json
{
  "Gender": 1,
  "PhoneNumber": "+201022222222",
  "Address": "Cairo",
  "Specializations": [
    {
      "YearsOfExperience": 5,
      "CasesHandled": 10,
      "Specialization": 1
    }
  ],
  "Bio": "Expert E2E Lawyer",
  "Level": 1,
  "DateOfBirth": "1985-01-01",
  "NationalNumber": "28501019358718"
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


### Login Admin

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Password": "Admin@123",
  "Email": "admin@smartcourt.com"
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
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhMzliNjMxMi0xOWMyLTQ5ZjctZmU0Mi0wOGRlZjQ4Yzk2NjMiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImEzOWI2MzEyLTE5YzItNDlmNy1mZTQyLTA4ZGVmNDhjOTY2MyIsImVtYWlsIjoiYWRtaW5Ac21hcnRjb3VydC5jb20iLCJuYW1lIjoiU3lzdGVtIEFkbWluaXN0cmF0b3IiLCJzZWN1cml0eV9zdGFtcCI6IkI0N09OTkw1V05BVUoyMzVMUlhIVTZOUVMyUEZPWkNRIiwianRpIjoiZmNkN2YwOWYtMWQ3ZC00MTA5LWJlOWEtOGJhMTRlZDViYTRlIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJuYmYiOjE3ODYyMTY5NTEsImV4cCI6MTc4NjIxNzg1MSwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.IwY1NQKIhQzomGRzp6KiAxg67_i-61ZN9Pv1_71b1Iw",
    "expiresIn": 900,
    "refreshToken": "as9C7rL4qBDJKDVSHSdLTodGSXc8cFTR19mgmac40/2VihZnzYu2wfzll2aMIrSlmpqeMpaOmRyqDp5hnOcOjQ==",
    "refreshTokenExpiration": "2026-08-15T19:22:31.7405889Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **Admin Login** 

### Admin Approve Lawyer

**Request:** PATCH http://localhost:5049/api/admin/verifications/41ad4ba8-158e-4342-1f6e-08def5797003/approve-account

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


### Admin Approve Client

**Request:** PATCH http://localhost:5049/api/admin/verifications/f139c5a7-e7f5-4f8f-1f6d-08def5797003/approve-account

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


### Re-Login Client (post-verify)

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Password": "Password123!",
  "Email": "e2e_client_20260808222230@example.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "f139c5a7-e7f5-4f8f-1f6d-08def5797003",
      "email": "e2e_client_20260808222230@example.com",
      "fullName": "E2E Client",
      "role": "Client",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJmMTM5YzVhNy1lN2Y1LTRmOGYtMWY2ZC0wOGRlZjU3OTcwMDMiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImYxMzljNWE3LWU3ZjUtNGY4Zi0xZjZkLTA4ZGVmNTc5NzAwMyIsImVtYWlsIjoiZTJlX2NsaWVudF8yMDI2MDgwODIyMjIzMEBleGFtcGxlLmNvbSIsIm5hbWUiOiJFMkUgQ2xpZW50Iiwic2VjdXJpdHlfc3RhbXAiOiJZUFNUNVdGSkFUSkxJTTNOR0I3TUQzSFZQUlZOWTJHTiIsImp0aSI6IjRiZTQxYTkwLTI0MWEtNDgyYy1hMDI1LTI5ZTczYTk5YTA5OSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNsaWVudCIsIm5iZiI6MTc4NjIxNjk1MSwiZXhwIjoxNzg2MjE3ODUxLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.2BYV5tEf0vwXyaNwitbSlKlrT_GDBlGOwUXpViTlpLs",
    "expiresIn": 900,
    "refreshToken": "lUay60H7g7sttZDNk70dPgs+mapi/j5LJj7TRVzzX65NxduKcM2CpHJ7yZPe1R7DA6H1dHz/fETxKm0b8A7KzQ==",
    "refreshTokenExpiration": "2026-08-15T19:22:31.9115883Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Re-Login Lawyer (post-verify)

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Password": "Password123!",
  "Email": "e2e_lawyer_20260808222230@example.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "41ad4ba8-158e-4342-1f6e-08def5797003",
      "email": "e2e_lawyer_20260808222230@example.com",
      "fullName": "E2E Lawyer",
      "role": "Lawyer",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0MWFkNGJhOC0xNThlLTQzNDItMWY2ZS0wOGRlZjU3OTcwMDMiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjQxYWQ0YmE4LTE1OGUtNDM0Mi0xZjZlLTA4ZGVmNTc5NzAwMyIsImVtYWlsIjoiZTJlX2xhd3llcl8yMDI2MDgwODIyMjIzMEBleGFtcGxlLmNvbSIsIm5hbWUiOiJFMkUgTGF3eWVyIiwic2VjdXJpdHlfc3RhbXAiOiJJUUFZVlFDVUg1T0tNTkI3SkZNNUZPQ1NWTzI0M0U0TyIsImp0aSI6IjU2YmU2NWQxLTYxZDEtNDRhNy1iNTM0LTgxMTA3MjJmNjI2MiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NjIxNjk1MiwiZXhwIjoxNzg2MjE3ODUyLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.nohIyElePB-Wn4V8nEER9qEolWto4c30J3-oB3j_AjQ",
    "expiresIn": 900,
    "refreshToken": "3tbA1N11xyX3/RfBwqZMjn79PjkVQc5JxzrmDnObJsmli2AtKlwvKY0ZiBxJ3Zegs+q5OzNt7dksNXhJ+CIaUA==",
    "refreshTokenExpiration": "2026-08-15T19:22:32.0103597Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **Both Verified and Tokens Refreshed** 


## Phase 2: Case and Proposal Workflows

### Create Case

**Request:** POST http://localhost:5049/api/Case

**Body:**
(multipart/form-data)

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "caseId": "ba94aca9-4d5f-4453-be51-1117023f905b",
    "failedDocuments": [
      {
        "fileName": "dummy_e2e.pdf",
        "error": "Error while uploading document : Invalid Compact JWS"
      }
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
``n---


- [OK] **Case Created** 

### Request Case AI Review

**Request:** POST http://localhost:5049/api/cases/ba94aca9-4d5f-4453-be51-1117023f905b/review

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
    "id": "16cce6c0-8a2a-4500-93a5-a726ad1d296f",
    "caseId": "ba94aca9-4d5f-4453-be51-1117023f905b",
    "isLatest": true,
    "createdAt": "2026-08-08T19:22:36.8344854Z",
    "reviewPoints": [
      {
        "id": "88379ea2-9f9b-450b-a820-2db1fb06b535",
        "description": "تتمثل نقطة القوة الأساسية في صياغة الموضوع بوضوح حول 'E2E Test Case 222232'، وتوافر السند المبدئي الذي يرجح كفة الموكل في إثبات أصل الالتزام وتفوقه إثباتياً على الخصم.",
        "type": "Strength"
      },
      {
        "id": "295af758-d5db-4606-a711-cef8ac3531ed",
        "description": "ميزة الخصم تتمثل في غياب التوثيق الرسمي للتنبيهات أو الإخطارات المتبادلة بين الأطراف، مما يتيح له إنكار الاستلام أو الدفع بالتراخي في المطالبة.",
        "type": "Weakness"
      },
      {
        "id": "0a140a51-2087-4e3c-a566-d1189d522123",
        "description": "يحتاج الملف إلى استيفاء النقاط والمعلومات التالية لضمان صياغة صحيفة الدعوى بشكل مكتمل: حصر وتفصيص المبالغ المالية المطلوبة والتعويضات الدقيقة عن الضرر المادي والمعنوي، وإدراج التواريخ الرسمية الدقيقة لبدء النزاع وتاريخ الإخلال بالتعهدات.",
        "type": "MissingCaseInfo"
      },
      {
        "id": "8aac9391-3c5b-47a5-b5fc-2c9177ee51a7",
        "description": "المستندات المحددة المطلوبة لإكمال الملف: أصل العقد/الاتفاق المبرم، صورة بطاقة الرقم القومي سارية لكل أطراف الدعوى، إيصالات التحويل أو السداد المالي، وأي إنذارات رسمية على يد محضر.",
        "type": "MissingCaseDoc"
      },
      {
        "id": "f305cea3-33e7-49c0-ba69-4a13cf8011eb",
        "description": "قم بإعادة هيكلة وصف القضية في صورة جدول زمني متسلسل، يبدأ من تاريخ التعهد الأول، مروراً بتاريخ الإخلال، وصولاً إلى حجم الأضرار المترتبة حالياً.",
        "type": "Suggestion"
      },
      {
        "id": "a571ea17-ea0a-4aff-b52f-1fd1e30a35a8",
        "description": "قم بتفقيط وقسمة كافة المطالبات المالية إلى بنود مستقلة (أصل الدين، الفوائد أو التعويض عن المماطلة، والرسوم) وتوثيق كل بند بسند كتابي مستقل.",
        "type": "Suggestion"
      },
      {
        "id": "06b1c4ee-24e0-4570-ac96-855575363765",
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


### Get Latest Review

**Request:** GET http://localhost:5049/api/cases/ba94aca9-4d5f-4453-be51-1117023f905b/reviews/latest

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "16cce6c0-8a2a-4500-93a5-a726ad1d296f",
    "caseId": "ba94aca9-4d5f-4453-be51-1117023f905b",
    "isLatest": true,
    "createdAt": "2026-08-08T19:22:36.8344854",
    "reviewPoints": [
      {
        "id": "a571ea17-ea0a-4aff-b52f-1fd1e30a35a8",
        "description": "قم بتفقيط وقسمة كافة المطالبات المالية إلى بنود مستقلة (أصل الدين، الفوائد أو التعويض عن المماطلة، والرسوم) وتوثيق كل بند بسند كتابي مستقل.",
        "type": "Suggestion"
      },
      {
        "id": "8aac9391-3c5b-47a5-b5fc-2c9177ee51a7",
        "description": "المستندات المحددة المطلوبة لإكمال الملف: أصل العقد/الاتفاق المبرم، صورة بطاقة الرقم القومي سارية لكل أطراف الدعوى، إيصالات التحويل أو السداد المالي، وأي إنذارات رسمية على يد محضر.",
        "type": "MissingCaseDoc"
      },
      {
        "id": "88379ea2-9f9b-450b-a820-2db1fb06b535",
        "description": "تتمثل نقطة القوة الأساسية في صياغة الموضوع بوضوح حول 'E2E Test Case 222232'، وتوافر السند المبدئي الذي يرجح كفة الموكل في إثبات أصل الالتزام وتفوقه إثباتياً على الخصم.",
        "type": "Strength"
      },
      {
        "id": "f305cea3-33e7-49c0-ba69-4a13cf8011eb",
        "description": "قم بإعادة هيكلة وصف القضية في صورة جدول زمني متسلسل، يبدأ من تاريخ التعهد الأول، مروراً بتاريخ الإخلال، وصولاً إلى حجم الأضرار المترتبة حالياً.",
        "type": "Suggestion"
      },
      {
        "id": "06b1c4ee-24e0-4570-ac96-855575363765",
        "description": "قم بتنظيم وثائق الملف في مجلد مرتب حسب التاريخ، وتأكد من مسح الأوراق ضوئياً بدقة عالية لضمان سهولة الإسناد والفحص القضائي.",
        "type": "Suggestion"
      },
      {
        "id": "295af758-d5db-4606-a711-cef8ac3531ed",
        "description": "ميزة الخصم تتمثل في غياب التوثيق الرسمي للتنبيهات أو الإخطارات المتبادلة بين الأطراف، مما يتيح له إنكار الاستلام أو الدفع بالتراخي في المطالبة.",
        "type": "Weakness"
      },
      {
        "id": "0a140a51-2087-4e3c-a566-d1189d522123",
        "description": "يحتاج الملف إلى استيفاء النقاط والمعلومات التالية لضمان صياغة صحيفة الدعوى بشكل مكتمل: حصر وتفصيص المبالغ المالية المطلوبة والتعويضات الدقيقة عن الضرر المادي والمعنوي، وإدراج التواريخ الرسمية الدقيقة لبدء النزاع وتاريخ الإخلال بالتعهدات.",
        "type": "MissingCaseInfo"
      }
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Finalize Case

**Request:** POST http://localhost:5049/api/Case/ba94aca9-4d5f-4453-be51-1117023f905b/finalize

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
    "caseId": "ba94aca9-4d5f-4453-be51-1117023f905b",
    "totalEligibleLawyers": 0,
    "recommendations": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **Case Finalized** 

### Client Creates Proposal

**Request:** POST http://localhost:5049/api/proposals

**Body:**
`json
{
  "Message": "Please take my case.",
  "LawyerUserId": "41ad4ba8-158e-4342-1f6e-08def5797003",
  "LegalCaseId": "ba94aca9-4d5f-4453-be51-1117023f905b"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "cdee13a2-a1cd-4135-92aa-5b22e0f7f244",
    "legalCaseId": "ba94aca9-4d5f-4453-be51-1117023f905b",
    "caseTitle": "E2E Test Case 222232",
    "clientUserId": "f139c5a7-e7f5-4f8f-1f6d-08def5797003",
    "clientName": "E2E Client",
    "lawyerUserId": "41ad4ba8-158e-4342-1f6e-08def5797003",
    "lawyerName": "E2E Lawyer",
    "message": "Please take my case.",
    "status": "Pending",
    "decisionReason": null,
    "createdAt": "2026-08-08T19:22:39.482979",
    "respondedAt": null,
    "updatedAt": "2026-08-08T19:22:39.482979",
    "conversationId": null
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
``n---


- [OK] **Proposal Created** 

### Lawyer Accepts Proposal

**Request:** POST http://localhost:5049/api/proposals/cdee13a2-a1cd-4135-92aa-5b22e0f7f244/accept

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
    "id": "cdee13a2-a1cd-4135-92aa-5b22e0f7f244",
    "legalCaseId": "ba94aca9-4d5f-4453-be51-1117023f905b",
    "caseTitle": "E2E Test Case 222232",
    "clientUserId": "f139c5a7-e7f5-4f8f-1f6d-08def5797003",
    "clientName": "E2E Client",
    "lawyerUserId": "41ad4ba8-158e-4342-1f6e-08def5797003",
    "lawyerName": "E2E Lawyer",
    "message": "Please take my case.",
    "status": "Accepted",
    "decisionReason": null,
    "createdAt": "2026-08-08T19:22:39.482979",
    "respondedAt": "2026-08-08T19:22:39.5389522",
    "updatedAt": "2026-08-08T19:22:39.5389522",
    "conversationId": "f76182c1-1f64-4705-8c46-11437012bf0b"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **Proposal Accepted** 


## Phase 3: Contract, Milestones and Chat

### Lawyer Creates Contract

**Request:** POST http://localhost:5049/api/contracts

**Body:**
`json
{
  "ProposalId": "cdee13a2-a1cd-4135-92aa-5b22e0f7f244",
  "Title": "E2E Legal Contract",
  "TermsAndConditions": "These terms govern the E2E test contract for integration testing of Smart Court. Both parties agree to all provisions herein."
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "782899ac-baa7-4fc7-b992-e1aa8ad4b8ec",
    "proposalId": "cdee13a2-a1cd-4135-92aa-5b22e0f7f244",
    "legalCaseId": "ba94aca9-4d5f-4453-be51-1117023f905b",
    "clientUserId": "f139c5a7-e7f5-4f8f-1f6d-08def5797003",
    "lawyerUserId": "41ad4ba8-158e-4342-1f6e-08def5797003",
    "title": "E2E Legal Contract",
    "termsAndConditions": "These terms govern the E2E test contract for integration testing of Smart Court. Both parties agree to all provisions herein.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": null,
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 0,
    "version": "\"AAAAAAAAQZQ=\"",
    "milestones": [],
    "payments": [],
    "permittedActions": [
      "Update",
      "Accept",
      "Terminate"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
``n---


- [OK] **Contract Created** 

### Get Contract (Client ETag)

**Request:** GET http://localhost:5049/api/contracts/782899ac-baa7-4fc7-b992-e1aa8ad4b8ec

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "782899ac-baa7-4fc7-b992-e1aa8ad4b8ec",
    "proposalId": "cdee13a2-a1cd-4135-92aa-5b22e0f7f244",
    "legalCaseId": "ba94aca9-4d5f-4453-be51-1117023f905b",
    "clientUserId": "f139c5a7-e7f5-4f8f-1f6d-08def5797003",
    "lawyerUserId": "41ad4ba8-158e-4342-1f6e-08def5797003",
    "title": "E2E Legal Contract",
    "termsAndConditions": "These terms govern the E2E test contract for integration testing of Smart Court. Both parties agree to all provisions herein.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": null,
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 0,
    "version": "\"AAAAAAAAQZQ=\"",
    "milestones": [],
    "payments": [],
    "permittedActions": [
      "Update",
      "Accept",
      "Terminate"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Client Accepts Contract

**Request:** POST http://localhost:5049/api/contracts/782899ac-baa7-4fc7-b992-e1aa8ad4b8ec/accept

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
    "entityId": "782899ac-baa7-4fc7-b992-e1aa8ad4b8ec",
    "status": "Draft",
    "occurredAt": "2026-08-08T19:22:39.6626841Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **Client Accepted Contract** 

### Get Contract (Lawyer ETag)

**Request:** GET http://localhost:5049/api/contracts/782899ac-baa7-4fc7-b992-e1aa8ad4b8ec

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "782899ac-baa7-4fc7-b992-e1aa8ad4b8ec",
    "proposalId": "cdee13a2-a1cd-4135-92aa-5b22e0f7f244",
    "legalCaseId": "ba94aca9-4d5f-4453-be51-1117023f905b",
    "clientUserId": "f139c5a7-e7f5-4f8f-1f6d-08def5797003",
    "lawyerUserId": "41ad4ba8-158e-4342-1f6e-08def5797003",
    "title": "E2E Legal Contract",
    "termsAndConditions": "These terms govern the E2E test contract for integration testing of Smart Court. Both parties agree to all provisions herein.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": "2026-08-08T19:22:39.6626841",
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 0,
    "version": "\"AAAAAAAAQZY=\"",
    "milestones": [],
    "payments": [],
    "permittedActions": [
      "Update",
      "Accept",
      "Terminate"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Lawyer Accepts Contract

**Request:** POST http://localhost:5049/api/contracts/782899ac-baa7-4fc7-b992-e1aa8ad4b8ec/accept

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
    "entityId": "782899ac-baa7-4fc7-b992-e1aa8ad4b8ec",
    "status": "Draft",
    "occurredAt": "2026-08-08T19:22:39.7772642Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **Lawyer Accepted Contract** 

### Create Milestone 1

**Request:** POST http://localhost:5049/api/contracts/782899ac-baa7-4fc7-b992-e1aa8ad4b8ec/milestones

**Body:**
`json
{
  "Description": "Comprehensive research for the case.",
  "OrderNumber": 1,
  "Title": "Phase 1: Research",
  "Amount": 1500.0,
  "DurationDays": 14
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "b451bd97-5999-4ffb-8a30-235105f31fa9",
    "orderNumber": 1,
    "title": "Phase 1: Research",
    "description": "Comprehensive research for the case.",
    "amount": 1500.0,
    "durationDays": 14,
    "dueDate": null,
    "status": 0,
    "fundingStatus": 0,
    "escrowHoldId": null,
    "fundedAt": null,
    "submittedAt": null,
    "autoAcceptEligibleAt": null,
    "holdExpiresAt": null,
    "netLawyerAmount": null,
    "version": "\"AAAAAAAAQZo=\"",
    "permittedActions": [
      "Update",
      "Approve"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
``n---


- [OK] **Milestone 1 Created** 

### List M1 (Client Approve)

**Request:** GET http://localhost:5049/api/contracts/782899ac-baa7-4fc7-b992-e1aa8ad4b8ec/milestones

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": [
    {
      "id": "b451bd97-5999-4ffb-8a30-235105f31fa9",
      "orderNumber": 1,
      "title": "Phase 1: Research",
      "description": "Comprehensive research for the case.",
      "amount": 1500.0,
      "durationDays": 14,
      "dueDate": null,
      "status": 0,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAQZo=\"",
      "permittedActions": [
        "Update",
        "Approve"
      ]
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Client Approves M1

**Request:** POST http://localhost:5049/api/milestones/b451bd97-5999-4ffb-8a30-235105f31fa9/approve

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
    "entityId": "b451bd97-5999-4ffb-8a30-235105f31fa9",
    "status": "Draft",
    "occurredAt": "2026-08-08T19:22:39.9992215Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **Client Approved Milestone 1** 

### List M1 (Lawyer Approve)

**Request:** GET http://localhost:5049/api/contracts/782899ac-baa7-4fc7-b992-e1aa8ad4b8ec/milestones

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": [
    {
      "id": "b451bd97-5999-4ffb-8a30-235105f31fa9",
      "orderNumber": 1,
      "title": "Phase 1: Research",
      "description": "Comprehensive research for the case.",
      "amount": 1500.0,
      "durationDays": 14,
      "dueDate": null,
      "status": 0,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAQZs=\"",
      "permittedActions": [
        "Update",
        "Approve"
      ]
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Lawyer Approves M1

**Request:** POST http://localhost:5049/api/milestones/b451bd97-5999-4ffb-8a30-235105f31fa9/approve

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
    "entityId": "b451bd97-5999-4ffb-8a30-235105f31fa9",
    "status": "AwaitingFunding",
    "occurredAt": "2026-08-08T19:22:40.0716291Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **Lawyer Approved Milestone 1** 

### Get Chat Conversations

**Request:** GET http://localhost:5049/api/chat/conversations

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "f76182c1-1f64-4705-8c46-11437012bf0b",
        "proposalId": "cdee13a2-a1cd-4135-92aa-5b22e0f7f244",
        "legalCaseId": "ba94aca9-4d5f-4453-be51-1117023f905b",
        "caseTitle": "E2E Test Case 222232",
        "client": {
          "userId": "f139c5a7-e7f5-4f8f-1f6d-08def5797003",
          "name": "E2E Client",
          "role": "Client"
        },
        "lawyer": {
          "userId": "41ad4ba8-158e-4342-1f6e-08def5797003",
          "name": "E2E Lawyer",
          "role": "Lawyer"
        },
        "status": "Open",
        "createdAt": "2026-08-08T19:22:39.5389522",
        "updatedAt": "2026-08-08T19:22:39.5389522",
        "lastMessageAt": null,
        "lastMessage": null
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 1,
    "hasNextPage": false
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Client Sends Chat Message

**Request:** POST http://localhost:5049/api/chat/conversations/f76182c1-1f64-4705-8c46-11437012bf0b/messages

**Body:**
`json
{
  "Content": "Hello, ready to begin the case!"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "89d97d0a-4cf9-4d4f-9588-b38963032cf1",
    "conversationId": "f76182c1-1f64-4705-8c46-11437012bf0b",
    "senderUserId": "f139c5a7-e7f5-4f8f-1f6d-08def5797003",
    "senderName": "E2E Client",
    "type": "User",
    "content": "Hello, ready to begin the case!",
    "systemCode": null,
    "relatedEntityId": null,
    "createdAt": "2026-08-08T19:22:40.3184025",
    "isMine": true
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **Chat Message Sent** 

### Poll Contract

**Request:** GET http://localhost:5049/api/contracts/782899ac-baa7-4fc7-b992-e1aa8ad4b8ec

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "782899ac-baa7-4fc7-b992-e1aa8ad4b8ec",
    "proposalId": "cdee13a2-a1cd-4135-92aa-5b22e0f7f244",
    "legalCaseId": "ba94aca9-4d5f-4453-be51-1117023f905b",
    "clientUserId": "f139c5a7-e7f5-4f8f-1f6d-08def5797003",
    "lawyerUserId": "41ad4ba8-158e-4342-1f6e-08def5797003",
    "title": "E2E Legal Contract",
    "termsAndConditions": "These terms govern the E2E test contract for integration testing of Smart Court. Both parties agree to all provisions herein.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": "2026-08-08T19:22:39.6626841",
    "acceptedByLawyerAt": "2026-08-08T19:22:39.7772642",
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 1500.0,
    "version": "\"AAAAAAAAQZg=\"",
    "milestones": [
      {
        "id": "b451bd97-5999-4ffb-8a30-235105f31fa9",
        "orderNumber": 1,
        "title": "Phase 1: Research",
        "description": "Comprehensive research for the case.",
        "amount": 1500.0,
        "durationDays": 14,
        "dueDate": null,
        "status": 1,
        "fundingStatus": 0,
        "escrowHoldId": null,
        "fundedAt": null,
        "submittedAt": null,
        "autoAcceptEligibleAt": null,
        "holdExpiresAt": null,
        "netLawyerAmount": null,
        "version": "\"AAAAAAAAQZw=\""
      }
    ],
    "payments": [],
    "permittedActions": [
      "Update",
      "Terminate"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Poll Contract

**Request:** GET http://localhost:5049/api/contracts/782899ac-baa7-4fc7-b992-e1aa8ad4b8ec

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "782899ac-baa7-4fc7-b992-e1aa8ad4b8ec",
    "proposalId": "cdee13a2-a1cd-4135-92aa-5b22e0f7f244",
    "legalCaseId": "ba94aca9-4d5f-4453-be51-1117023f905b",
    "clientUserId": "f139c5a7-e7f5-4f8f-1f6d-08def5797003",
    "lawyerUserId": "41ad4ba8-158e-4342-1f6e-08def5797003",
    "title": "E2E Legal Contract",
    "termsAndConditions": "These terms govern the E2E test contract for integration testing of Smart Court. Both parties agree to all provisions herein.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": "2026-08-08T19:22:39.6626841",
    "acceptedByLawyerAt": "2026-08-08T19:22:39.7772642",
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 1500.0,
    "version": "\"AAAAAAAAQZg=\"",
    "milestones": [
      {
        "id": "b451bd97-5999-4ffb-8a30-235105f31fa9",
        "orderNumber": 1,
        "title": "Phase 1: Research",
        "description": "Comprehensive research for the case.",
        "amount": 1500.0,
        "durationDays": 14,
        "dueDate": null,
        "status": 1,
        "fundingStatus": 0,
        "escrowHoldId": null,
        "fundedAt": null,
        "submittedAt": null,
        "autoAcceptEligibleAt": null,
        "holdExpiresAt": null,
        "netLawyerAmount": null,
        "version": "\"AAAAAAAAQZw=\""
      }
    ],
    "payments": [],
    "permittedActions": [
      "Update",
      "Terminate"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Poll Contract

**Request:** GET http://localhost:5049/api/contracts/782899ac-baa7-4fc7-b992-e1aa8ad4b8ec

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "782899ac-baa7-4fc7-b992-e1aa8ad4b8ec",
    "proposalId": "cdee13a2-a1cd-4135-92aa-5b22e0f7f244",
    "legalCaseId": "ba94aca9-4d5f-4453-be51-1117023f905b",
    "clientUserId": "f139c5a7-e7f5-4f8f-1f6d-08def5797003",
    "lawyerUserId": "41ad4ba8-158e-4342-1f6e-08def5797003",
    "title": "E2E Legal Contract",
    "termsAndConditions": "These terms govern the E2E test contract for integration testing of Smart Court. Both parties agree to all provisions herein.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": "2026-08-08T19:22:39.6626841",
    "acceptedByLawyerAt": "2026-08-08T19:22:39.7772642",
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 1500.0,
    "version": "\"AAAAAAAAQZg=\"",
    "milestones": [
      {
        "id": "b451bd97-5999-4ffb-8a30-235105f31fa9",
        "orderNumber": 1,
        "title": "Phase 1: Research",
        "description": "Comprehensive research for the case.",
        "amount": 1500.0,
        "durationDays": 14,
        "dueDate": null,
        "status": 1,
        "fundingStatus": 0,
        "escrowHoldId": null,
        "fundedAt": null,
        "submittedAt": null,
        "autoAcceptEligibleAt": null,
        "holdExpiresAt": null,
        "netLawyerAmount": null,
        "version": "\"AAAAAAAAQZw=\""
      }
    ],
    "payments": [],
    "permittedActions": [
      "Update",
      "Terminate"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Poll Contract

**Request:** GET http://localhost:5049/api/contracts/782899ac-baa7-4fc7-b992-e1aa8ad4b8ec

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "782899ac-baa7-4fc7-b992-e1aa8ad4b8ec",
    "proposalId": "cdee13a2-a1cd-4135-92aa-5b22e0f7f244",
    "legalCaseId": "ba94aca9-4d5f-4453-be51-1117023f905b",
    "clientUserId": "f139c5a7-e7f5-4f8f-1f6d-08def5797003",
    "lawyerUserId": "41ad4ba8-158e-4342-1f6e-08def5797003",
    "title": "E2E Legal Contract",
    "termsAndConditions": "These terms govern the E2E test contract for integration testing of Smart Court. Both parties agree to all provisions herein.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": "2026-08-08T19:22:39.6626841",
    "acceptedByLawyerAt": "2026-08-08T19:22:39.7772642",
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 1500.0,
    "version": "\"AAAAAAAAQZg=\"",
    "milestones": [
      {
        "id": "b451bd97-5999-4ffb-8a30-235105f31fa9",
        "orderNumber": 1,
        "title": "Phase 1: Research",
        "description": "Comprehensive research for the case.",
        "amount": 1500.0,
        "durationDays": 14,
        "dueDate": null,
        "status": 1,
        "fundingStatus": 0,
        "escrowHoldId": null,
        "fundedAt": null,
        "submittedAt": null,
        "autoAcceptEligibleAt": null,
        "holdExpiresAt": null,
        "netLawyerAmount": null,
        "version": "\"AAAAAAAAQZw=\""
      }
    ],
    "payments": [],
    "permittedActions": [
      "Update",
      "Terminate"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Poll Contract

**Request:** GET http://localhost:5049/api/contracts/782899ac-baa7-4fc7-b992-e1aa8ad4b8ec

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "782899ac-baa7-4fc7-b992-e1aa8ad4b8ec",
    "proposalId": "cdee13a2-a1cd-4135-92aa-5b22e0f7f244",
    "legalCaseId": "ba94aca9-4d5f-4453-be51-1117023f905b",
    "clientUserId": "f139c5a7-e7f5-4f8f-1f6d-08def5797003",
    "lawyerUserId": "41ad4ba8-158e-4342-1f6e-08def5797003",
    "title": "E2E Legal Contract",
    "termsAndConditions": "These terms govern the E2E test contract for integration testing of Smart Court. Both parties agree to all provisions herein.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": "2026-08-08T19:22:39.6626841",
    "acceptedByLawyerAt": "2026-08-08T19:22:39.7772642",
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 1500.0,
    "version": "\"AAAAAAAAQZg=\"",
    "milestones": [
      {
        "id": "b451bd97-5999-4ffb-8a30-235105f31fa9",
        "orderNumber": 1,
        "title": "Phase 1: Research",
        "description": "Comprehensive research for the case.",
        "amount": 1500.0,
        "durationDays": 14,
        "dueDate": null,
        "status": 1,
        "fundingStatus": 0,
        "escrowHoldId": null,
        "fundedAt": null,
        "submittedAt": null,
        "autoAcceptEligibleAt": null,
        "holdExpiresAt": null,
        "netLawyerAmount": null,
        "version": "\"AAAAAAAAQZw=\""
      }
    ],
    "payments": [],
    "permittedActions": [
      "Update",
      "Terminate"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Poll Contract

**Request:** GET http://localhost:5049/api/contracts/782899ac-baa7-4fc7-b992-e1aa8ad4b8ec

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "782899ac-baa7-4fc7-b992-e1aa8ad4b8ec",
    "proposalId": "cdee13a2-a1cd-4135-92aa-5b22e0f7f244",
    "legalCaseId": "ba94aca9-4d5f-4453-be51-1117023f905b",
    "clientUserId": "f139c5a7-e7f5-4f8f-1f6d-08def5797003",
    "lawyerUserId": "41ad4ba8-158e-4342-1f6e-08def5797003",
    "title": "E2E Legal Contract",
    "termsAndConditions": "These terms govern the E2E test contract for integration testing of Smart Court. Both parties agree to all provisions herein.",
    "currency": "EGP",
    "status": 1,
    "acceptedByClientAt": "2026-08-08T19:22:39.6626841",
    "acceptedByLawyerAt": "2026-08-08T19:22:39.7772642",
    "activatedAt": "2026-08-08T19:23:01.5794505",
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 1500.0,
    "version": "\"AAAAAAAAQbE=\"",
    "milestones": [
      {
        "id": "b451bd97-5999-4ffb-8a30-235105f31fa9",
        "orderNumber": 1,
        "title": "Phase 1: Research",
        "description": "Comprehensive research for the case.",
        "amount": 1500.0,
        "durationDays": 14,
        "dueDate": null,
        "status": 1,
        "fundingStatus": 0,
        "escrowHoldId": null,
        "fundedAt": null,
        "submittedAt": null,
        "autoAcceptEligibleAt": null,
        "holdExpiresAt": null,
        "netLawyerAmount": null,
        "version": "\"AAAAAAAAQZw=\""
      }
    ],
    "payments": [],
    "permittedActions": [
      "Terminate"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **Contract is Active** 


## Phase 4: Payments - Funding Milestone 1

### List M1 (pre-RFF)

**Request:** GET http://localhost:5049/api/contracts/782899ac-baa7-4fc7-b992-e1aa8ad4b8ec/milestones

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": [
    {
      "id": "b451bd97-5999-4ffb-8a30-235105f31fa9",
      "orderNumber": 1,
      "title": "Phase 1: Research",
      "description": "Comprehensive research for the case.",
      "amount": 1500.0,
      "durationDays": 14,
      "dueDate": null,
      "status": 1,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAQZw=\"",
      "permittedActions": [
        "ReadyForFunding"
      ]
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Lawyer: ReadyForFunding M1

**Request:** POST http://localhost:5049/api/milestones/b451bd97-5999-4ffb-8a30-235105f31fa9/ready-for-funding

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
    "entityId": "b451bd97-5999-4ffb-8a30-235105f31fa9",
    "status": "AwaitingFunding",
    "occurredAt": "2026-08-08T19:23:05.8836051Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **Milestone 1 Marked ReadyForFunding** 

### Client Funds M1 (mock-success)

**Request:** POST http://localhost:5049/api/milestones/b451bd97-5999-4ffb-8a30-235105f31fa9/fund

**Body:**
`json
{
  "PaymentMethodReference": "mock-success"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "bf9e88b1-c76e-4999-9522-2fd0e6d54faf",
    "milestoneId": "b451bd97-5999-4ffb-8a30-235105f31fa9",
    "grossAmount": 1500.0,
    "platformFee": 75.0,
    "netAmount": 1425.0,
    "currency": "EGP",
    "status": 0,
    "holdExpiresAt": null,
    "settledAt": null
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **M1 Funding Call Succeeded** 

### List M1 (post-fund)

**Request:** GET http://localhost:5049/api/contracts/782899ac-baa7-4fc7-b992-e1aa8ad4b8ec/milestones

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": [
    {
      "id": "b451bd97-5999-4ffb-8a30-235105f31fa9",
      "orderNumber": 1,
      "title": "Phase 1: Research",
      "description": "Comprehensive research for the case.",
      "amount": 1500.0,
      "durationDays": 14,
      "dueDate": null,
      "status": 3,
      "fundingStatus": 2,
      "escrowHoldId": "bf9e88b1-c76e-4999-9522-2fd0e6d54faf",
      "fundedAt": "2026-08-08T19:23:05.9768476",
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": 1425.0,
      "version": "\"AAAAAAAAQb4=\"",
      "permittedActions": []
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **M1 = FundedInProgress (3)** (status=3)

### Get Contract Payments

**Request:** GET http://localhost:5049/api/contracts/782899ac-baa7-4fc7-b992-e1aa8ad4b8ec/payments

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "payments": [
      {
        "id": "bf9e88b1-c76e-4999-9522-2fd0e6d54faf",
        "milestoneId": "b451bd97-5999-4ffb-8a30-235105f31fa9",
        "grossAmount": 1500.0,
        "platformFee": 75.0,
        "netAmount": 1425.0,
        "currency": "EGP",
        "status": 0,
        "holdExpiresAt": null,
        "settledAt": null
      }
    ],
    "attempts": [
      {
        "id": "17f71917-2fcc-45d3-9f8f-573650ea85d9",
        "milestoneId": "b451bd97-5999-4ffb-8a30-235105f31fa9",
        "operationType": 0,
        "status": 1,
        "amount": 1500.0,
        "currency": "EGP",
        "providerName": "MockPaymentProvider",
        "providerAttemptCount": 0,
        "nextRetryAt": null,
        "requiresManualAction": false,
        "manualActionRequiredAt": null,
        "createdAt": "2026-08-08T19:23:05.9528404",
        "processedAt": "2026-08-08T19:23:05.9768476"
      }
    ],
    "ledgerEntries": [
      {
        "id": "b337b385-a530-4c56-8b8e-aae46bfcc67c",
        "escrowHoldId": "bf9e88b1-c76e-4999-9522-2fd0e6d54faf",
        "transactionType": 0,
        "amount": 1500.0,
        "runningBalance": 1500.0,
        "currency": "EGP",
        "description": "إيداع تمويل المرحلة في حساب الضمان.",
        "createdAt": "2026-08-08T19:23:05.9768476"
      }
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **Payment History Retrieved** 

- [OK] **EscrowHold Organically Created (Status=Funded=0)** (holds=1)

- [OK] **Deposit PaymentTransaction Completed (Status=1)** (txns=1)


## Phase 5: Milestone Delivery and Acceptance

### Lawyer Submits M1 (1st)

**Request:** POST http://localhost:5049/api/milestones/b451bd97-5999-4ffb-8a30-235105f31fa9/submit

**Body:**
`json
{
  "Notes": "Research complete. All documents attached.",
  "StoredFileIds": [
    "f94dc857-7bd8-484d-a150-85add793486d"
  ]
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "b451bd97-5999-4ffb-8a30-235105f31fa9",
    "orderNumber": 1,
    "title": "Phase 1: Research",
    "description": "Comprehensive research for the case.",
    "amount": 1500.0,
    "durationDays": 14,
    "dueDate": null,
    "status": 4,
    "fundingStatus": 2,
    "escrowHoldId": "bf9e88b1-c76e-4999-9522-2fd0e6d54faf",
    "fundedAt": "2026-08-08T19:23:05.9768476",
    "submittedAt": "2026-08-08T19:23:08.4749329Z",
    "autoAcceptEligibleAt": "2026-08-15T19:23:08.4749329Z",
    "holdExpiresAt": null,
    "netLawyerAmount": 1425.0,
    "version": "\"AAAAAAAAQcQ=\"",
    "permittedActions": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **M1 Submitted (1st)** 

### Client Requests Changes

**Request:** POST http://localhost:5049/api/milestones/b451bd97-5999-4ffb-8a30-235105f31fa9/request-changes

**Body:**
`json
{
  "Reason": "Please add more detail to the research findings section."
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "b451bd97-5999-4ffb-8a30-235105f31fa9",
    "orderNumber": 1,
    "title": "Phase 1: Research",
    "description": "Comprehensive research for the case.",
    "amount": 1500.0,
    "durationDays": 14,
    "dueDate": null,
    "status": 3,
    "fundingStatus": 2,
    "escrowHoldId": "bf9e88b1-c76e-4999-9522-2fd0e6d54faf",
    "fundedAt": "2026-08-08T19:23:05.9768476",
    "submittedAt": null,
    "autoAcceptEligibleAt": null,
    "holdExpiresAt": null,
    "netLawyerAmount": 1425.0,
    "version": "\"AAAAAAAAQcY=\"",
    "permittedActions": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **Client Requested Changes** 

### Lawyer Submits M1 (2nd)

**Request:** POST http://localhost:5049/api/milestones/b451bd97-5999-4ffb-8a30-235105f31fa9/submit

**Body:**
`json
{
  "Notes": "Research complete. All documents attached.",
  "StoredFileIds": [
    "f94dc857-7bd8-484d-a150-85add793486d"
  ]
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "b451bd97-5999-4ffb-8a30-235105f31fa9",
    "orderNumber": 1,
    "title": "Phase 1: Research",
    "description": "Comprehensive research for the case.",
    "amount": 1500.0,
    "durationDays": 14,
    "dueDate": null,
    "status": 4,
    "fundingStatus": 2,
    "escrowHoldId": "bf9e88b1-c76e-4999-9522-2fd0e6d54faf",
    "fundedAt": "2026-08-08T19:23:05.9768476",
    "submittedAt": "2026-08-08T19:23:08.6466964Z",
    "autoAcceptEligibleAt": "2026-08-15T19:23:08.6466964Z",
    "holdExpiresAt": null,
    "netLawyerAmount": 1425.0,
    "version": "\"AAAAAAAAQcg=\"",
    "permittedActions": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **M1 Submitted (2nd)** 

### Client Accepts M1

**Request:** POST http://localhost:5049/api/milestones/b451bd97-5999-4ffb-8a30-235105f31fa9/accept

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
    "id": "b451bd97-5999-4ffb-8a30-235105f31fa9",
    "orderNumber": 1,
    "title": "Phase 1: Research",
    "description": "Comprehensive research for the case.",
    "amount": 1500.0,
    "durationDays": 14,
    "dueDate": null,
    "status": 5,
    "fundingStatus": 2,
    "escrowHoldId": "bf9e88b1-c76e-4999-9522-2fd0e6d54faf",
    "fundedAt": "2026-08-08T19:23:05.9768476",
    "submittedAt": "2026-08-08T19:23:08.6466964",
    "autoAcceptEligibleAt": null,
    "holdExpiresAt": "2026-08-22T19:23:08.7071333Z",
    "netLawyerAmount": 1425.0,
    "version": "\"AAAAAAAAQcs=\"",
    "permittedActions": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **M1 Accepted by Client** 

### List M1 (post-accept)

**Request:** GET http://localhost:5049/api/contracts/782899ac-baa7-4fc7-b992-e1aa8ad4b8ec/milestones

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": [
    {
      "id": "b451bd97-5999-4ffb-8a30-235105f31fa9",
      "orderNumber": 1,
      "title": "Phase 1: Research",
      "description": "Comprehensive research for the case.",
      "amount": 1500.0,
      "durationDays": 14,
      "dueDate": null,
      "status": 5,
      "fundingStatus": 2,
      "escrowHoldId": "bf9e88b1-c76e-4999-9522-2fd0e6d54faf",
      "fundedAt": "2026-08-08T19:23:05.9768476",
      "submittedAt": "2026-08-08T19:23:08.6466964",
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": "2026-08-22T19:23:08.7071333",
      "netLawyerAmount": 1425.0,
      "version": "\"AAAAAAAAQcs=\"",
      "permittedActions": []
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **M1 = AcceptedHold (5)** (status=5)


## Phase 6: Escrow Release and Wallet Verification

- [OK] **MilestoneAccepted in Outbox** 

- [OK] **MilestoneAccepted Outbox Processed (Status=2)** 

- [OK] **HoldExpiresAt moved to past (timing helper)** (holdId=BF9E88B1-C76E-4999-9522-2FD0E6D54FAF)

### Poll M1 Status

**Request:** GET http://localhost:5049/api/contracts/782899ac-baa7-4fc7-b992-e1aa8ad4b8ec/milestones

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": [
    {
      "id": "b451bd97-5999-4ffb-8a30-235105f31fa9",
      "orderNumber": 1,
      "title": "Phase 1: Research",
      "description": "Comprehensive research for the case.",
      "amount": 1500.0,
      "durationDays": 14,
      "dueDate": null,
      "status": 5,
      "fundingStatus": 2,
      "escrowHoldId": "bf9e88b1-c76e-4999-9522-2fd0e6d54faf",
      "fundedAt": "2026-08-08T19:23:05.9768476",
      "submittedAt": "2026-08-08T19:23:08.6466964",
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": "2026-08-08T19:23:01.87",
      "netLawyerAmount": 1425.0,
      "version": "\"AAAAAAAAQec=\"",
      "permittedActions": []
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Poll M1 Status

**Request:** GET http://localhost:5049/api/contracts/782899ac-baa7-4fc7-b992-e1aa8ad4b8ec/milestones

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": [
    {
      "id": "b451bd97-5999-4ffb-8a30-235105f31fa9",
      "orderNumber": 1,
      "title": "Phase 1: Research",
      "description": "Comprehensive research for the case.",
      "amount": 1500.0,
      "durationDays": 14,
      "dueDate": null,
      "status": 7,
      "fundingStatus": 3,
      "escrowHoldId": "bf9e88b1-c76e-4999-9522-2fd0e6d54faf",
      "fundedAt": "2026-08-08T19:23:05.9768476",
      "submittedAt": "2026-08-08T19:23:08.6466964",
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": "2026-08-08T19:23:01.87",
      "netLawyerAmount": 1425.0,
      "version": "\"AAAAAAAAQe4=\"",
      "permittedActions": []
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **M1 Released (7)** 

- [OK] **Release PaymentTransaction Created and Completed** (txns=1)

### Get Lawyer Wallet

**Request:** GET http://localhost:5049/api/wallet

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "lawyerUserId": "41ad4ba8-158e-4342-1f6e-08def5797003",
    "currency": "EGP",
    "pendingBalance": 0.0,
    "availableBalance": 1425.0,
    "totalReleased": 1425.0
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


- [OK] **Lawyer Wallet Credited (AvailableBalance > 0)** (balance=1425)


## Phase 7: Outbox and Hangfire Audit

- [OK] **No Stuck Outbox Messages** (stuck=0)

- [OK] **No Failed Outbox Messages (Status=3)** (failed=0)

- [OK] **No Failed Hangfire Release/Accept Jobs** (failures=0)


### Outbox Message Summary
` `text
ContractAccepted 2 2
ContractActivated 2 1
ContractActivationRequested 2 1
ContractCompleted 0 1
ContractCreated 2 1
MilestoneAccepted 2 1
MilestoneChangesRequested 2 1
MilestoneFunded 2 1
MilestoneFundingStarted 2 1
MilestoneReadyForFunding 2 1
MilestoneSubmitted 2 2
` ` `n

## Test Execution Summary

---

| Entity | Value | Final Status |
|--------|-------|--------------|
| Client ID | f139c5a7-e7f5-4f8f-1f6d-08def5797003 | Verified |
| Lawyer ID | 41ad4ba8-158e-4342-1f6e-08def5797003 | Verified |
| Case ID | ba94aca9-4d5f-4453-be51-1117023f905b | Finalized |
| Proposal ID | cdee13a2-a1cd-4135-92aa-5b22e0f7f244 | Accepted |
| Contract ID | 782899ac-baa7-4fc7-b992-e1aa8ad4b8ec | Active |
| Milestone 1 | b451bd97-5999-4ffb-8a30-235105f31fa9 | Released (7) |
| Escrow Hold | BF9E88B1-C76E-4999-9522-2FD0E6D54FAF | Released |

**Completed at: 2026-08-08 22:24:09**

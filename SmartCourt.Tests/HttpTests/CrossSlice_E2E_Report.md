# Cross-Slice End-to-End Integration Test Report


This report covers comprehensive cross-slice workflows between Auth, Users, UserVerification, and AdminVerification.


### 1a. Setup - Login Admin

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "moatazmohammed2392003@gmail.com",
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
      "id": "42b7c6b9-8690-456c-716a-08dee31497fe",
      "email": "moatazmohammed2392003@gmail.com",
      "fullName": "Moataz Mohammed",
      "role": "Admin"
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0MmI3YzZiOS04NjkwLTQ1NmMtNzE2YS0wOGRlZTMxNDk3ZmUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjQyYjdjNmI5LTg2OTAtNDU2Yy03MTZhLTA4ZGVlMzE0OTdmZSIsImVtYWlsIjoibW9hdGF6bW9oYW1tZWQyMzkyMDAzQGdtYWlsLmNvbSIsIm5hbWUiOiJNb2F0YXogTW9oYW1tZWQiLCJzZWN1cml0eV9zdGFtcCI6IlFWMkhOTEtXUENLUVNQMlE1VTJPQkNBVTRYVFdYUUJTIiwianRpIjoiMDEzZWYzNjctZjhkNy00MjllLTlhNGItMTM1YjE0YmNjYTNiIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJuYmYiOjE3ODU5NTExNjgsImV4cCI6MTc4NTk1NDc2OCwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.qrQKwWbO-T5O9efxBFunPCZvDRuOGN25st9uib3NBF0",
    "expiresIn": 3600,
    "refreshToken": "CMjtWZWyrE0P/ULlNnFZR01H2FzZi448Ukb2yIJ1Oj861y02m/ksyACfgv6ugN4uvuCAuQ9Lsd3q0sUcaABg1A==",
    "refreshTokenExpiration": "2026-08-12T17:32:48.3623444Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 1b. Setup - Register Lawyer

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
`json
{
  "FullName": "Lawyer E2E",
  "Password": "Password123!",
  "ConfirmPassword": "Password123!",
  "Email": "lawyer_e2e_308114618@test.com"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "ae4391e2-732f-4773-0b13-08def31747a9",
    "email": "lawyer_e2e_308114618@test.com",
    "fullName": "Lawyer E2E",
    "role": "Lawyer"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


Found confirmation URL for lawyer_e2e_308114618@test.com: http://localhost:5173/verify-email?userId=ae4391e2-732f-4773-0b13-08def31747a9&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI4Z09LRHo4TDBJMUxlY0MxZXpLRWh4MHlsL1AwRW9nU0wvSzRpK3prSGlRRDhUTmlXUkZKTSsvNklidUhaZUt2NVhydGE5UXc3czJLOEsvR3F4ZEFkY3JZU2hCSUN4ZDZDNnJwcUR2MnV6K3lzelh3VWRuQWxTRWFIb1hPQWRsL2s1bzRyazBMZStNa01jaDJKVkdxVXJDb1VDRGhLdkNQZ0R1VmdxeDM5bkVLTHg1U1gxSkZHNDBZOTdBczlYYkFDa0hwaGIxdGhVR01vTTE0bFV2TWp5dmtybTRaRTMwb2FuRE5wd3pLcHo3UT09

### Confirm Email for lawyer_e2e_308114618@test.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=ae4391e2-732f-4773-0b13-08def31747a9&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI4Z09LRHo4TDBJMUxlY0MxZXpLRWh4MHlsL1AwRW9nU0wvSzRpK3prSGlRRDhUTmlXUkZKTSsvNklidUhaZUt2NVhydGE5UXc3czJLOEsvR3F4ZEFkY3JZU2hCSUN4ZDZDNnJwcUR2MnV6K3lzelh3VWRuQWxTRWFIb1hPQWRsL2s1bzRyazBMZStNa01jaDJKVkdxVXJDb1VDRGhLdkNQZ0R1VmdxeDM5bkVLTHg1U1gxSkZHNDBZOTdBczlYYkFDa0hwaGIxdGhVR01vTTE0bFV2TWp5dmtybTRaRTMwb2FuRE5wd3pLcHo3UT09

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


### 1c. Setup - Login Lawyer

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "lawyer_e2e_308114618@test.com",
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
      "id": "ae4391e2-732f-4773-0b13-08def31747a9",
      "email": "lawyer_e2e_308114618@test.com",
      "fullName": "Lawyer E2E",
      "role": "Lawyer"
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZTQzOTFlMi03MzJmLTQ3NzMtMGIxMy0wOGRlZjMxNzQ3YTkiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImFlNDM5MWUyLTczMmYtNDc3My0wYjEzLTA4ZGVmMzE3NDdhOSIsImVtYWlsIjoibGF3eWVyX2UyZV8zMDgxMTQ2MThAdGVzdC5jb20iLCJuYW1lIjoiTGF3eWVyIEUyRSIsInNlY3VyaXR5X3N0YW1wIjoiSVVaTkNWNjdFWlZHUFJXMzJTTTJUNVpFUVVJQTc0QVEiLCJqdGkiOiJjODJkMGJhNy04N2ZmLTQwYzctYmJhNS0zY2RhNDRlMzVmMTQiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODU5NTExNzQsImV4cCI6MTc4NTk1NDc3NCwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.75L-cPAD3li27Ns3lbBWgfLAGQjVM4LefhxPRLFwKlc",
    "expiresIn": 3600,
    "refreshToken": "0TwFFsXAfDxHI3nA2NfMg0z/NypMIjvz+fYDt8TnQySSKuHdnasxF09ltJKjWsTMCboskUuC3rrH3WkuiKlffQ==",
    "refreshTokenExpiration": "2026-08-12T17:32:54.7250872Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 1d. Setup - Register Client

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
  "FullName": "Client E2E",
  "Password": "Password123!",
  "ConfirmPassword": "Password123!",
  "Email": "client_e2e_308114618@test.com"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "1bc3a90c-e4db-4e96-0b14-08def31747a9",
    "email": "client_e2e_308114618@test.com",
    "fullName": "Client E2E",
    "role": "Client"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


Found confirmation URL for client_e2e_308114618@test.com: http://localhost:5173/verify-email?userId=1bc3a90c-e4db-4e96-0b14-08def31747a9&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5Vnk3N25pQnJsa1Rsb1o4d1BDZlA2WEIwbnNhNnBBMUEyN2lzZ2dRTFVhSjR5UWZKM0Vrc2E3dmFJd202ZlVKUTRkbk4yKzZsZzNrWmZFTVFqblcrVXFReDFWaGROakl5RUR2UGdZaU00L2d1RXhpV3RlbmhXcHhKUTRudnNnRVpuQXlUZGVjYlkvRmZRYXJFY1lmYWxmS251NkRkQkRhVFV6bGJ4bXFCRlZIekNoaHQwL2IydzlYQXljcTNvMlM2U2IzR2hFOWFIWVpqSVVZaVlvQ1Q3eUJZem9HNXZ4MkdYd2pWZ0hsem5jUT09

### Confirm Email for client_e2e_308114618@test.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=1bc3a90c-e4db-4e96-0b14-08def31747a9&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5Vnk3N25pQnJsa1Rsb1o4d1BDZlA2WEIwbnNhNnBBMUEyN2lzZ2dRTFVhSjR5UWZKM0Vrc2E3dmFJd202ZlVKUTRkbk4yKzZsZzNrWmZFTVFqblcrVXFReDFWaGROakl5RUR2UGdZaU00L2d1RXhpV3RlbmhXcHhKUTRudnNnRVpuQXlUZGVjYlkvRmZRYXJFY1lmYWxmS251NkRkQkRhVFV6bGJ4bXFCRlZIekNoaHQwL2IydzlYQXljcTNvMlM2U2IzR2hFOWFIWVpqSVVZaVlvQ1Q3eUJZem9HNXZ4MkdYd2pWZ0hsem5jUT09

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


### 1e. Setup - Login Client

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "client_e2e_308114618@test.com",
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
      "id": "1bc3a90c-e4db-4e96-0b14-08def31747a9",
      "email": "client_e2e_308114618@test.com",
      "fullName": "Client E2E",
      "role": "Client"
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxYmMzYTkwYy1lNGRiLTRlOTYtMGIxNC0wOGRlZjMxNzQ3YTkiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjFiYzNhOTBjLWU0ZGItNGU5Ni0wYjE0LTA4ZGVmMzE3NDdhOSIsImVtYWlsIjoiY2xpZW50X2UyZV8zMDgxMTQ2MThAdGVzdC5jb20iLCJuYW1lIjoiQ2xpZW50IEUyRSIsInNlY3VyaXR5X3N0YW1wIjoiSkNXVENTR0hLSUI2UElKUklWMlFZRTZXVUdPNFdJRjUiLCJqdGkiOiIyZDFhOWExZS0wZDZjLTRmNWUtYTlmOC01MDg1OGQ4ODAwNWEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDbGllbnQiLCJuYmYiOjE3ODU5NTExODAsImV4cCI6MTc4NTk1NDc4MCwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.w6Oimhh5TRTRNUioQ4U7_SQRSx3WlaxorC4rSFCOvn8",
    "expiresIn": 3600,
    "refreshToken": "yIyARHaIv5rgmUO0sLq1Iur9tRqaTjCwmsYtQ0bF6cNBgW79dxyViAISmSGBEM0/lebq2SXQz3o5xWqZXibSPw==",
    "refreshTokenExpiration": "2026-08-12T17:33:00.1038358Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 2a. Profile - Complete Client Profile (Valid)

**Request:** POST http://localhost:5049/api/clients/profile/complete

**Body:**
`json
{
  "Gender": 1,
  "PhoneNumber": "+201012345678",
  "DateOfBirth": "1990-01-01",
  "Address": "Riyadh"
}
``n
**Response Status:** 400

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "تم استكمال الملف الشخصي مسبقاً.",
  "errors": null,
  "statusCode": 400
}
``n---


### 2b. Profile - Get Client Profile

**Request:** GET http://localhost:5049/api/clients/profile

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "1bc3a90c-e4db-4e96-0b14-08def31747a9",
    "name": "Client E2E",
    "email": "client_e2e_308114618@test.com",
    "phoneNumber": "",
    "gender": null,
    "dateOfBirth": null,
    "address": null,
    "status": "Active"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 2c. Profile - Update Client Profile (Valid)

**Request:** PUT http://localhost:5049/api/clients/profile

**Body:**
`json
{
  "FullName": "Client E2E Updated",
  "PhoneNumber": "+201012345679",
  "Gender": 1,
  "DateOfBirth": "1990-01-02",
  "Address": "Jeddah"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "message": "تم تحديث الملف الشخصي بنجاح.",
  "errors": null,
  "statusCode": 200
}
``n---


### 2c2. Setup - Re-Login Client (After Profile Update)

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "client_e2e_308114618@test.com",
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
      "id": "1bc3a90c-e4db-4e96-0b14-08def31747a9",
      "email": "client_e2e_308114618@test.com",
      "fullName": "Client E2E",
      "role": "Client"
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxYmMzYTkwYy1lNGRiLTRlOTYtMGIxNC0wOGRlZjMxNzQ3YTkiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjFiYzNhOTBjLWU0ZGItNGU5Ni0wYjE0LTA4ZGVmMzE3NDdhOSIsImVtYWlsIjoiY2xpZW50X2UyZV8zMDgxMTQ2MThAdGVzdC5jb20iLCJuYW1lIjoiQ2xpZW50IEUyRSIsInNlY3VyaXR5X3N0YW1wIjoiSFdOQjM2MjNYUE9QUFJQV0JISllXQ0hTQzZDWTJOWUsiLCJqdGkiOiI0ZTIwNmQ0Ni03NzE1LTQwMjUtYWU4Mi01YjZkZDhiZjQ5MGEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDbGllbnQiLCJuYmYiOjE3ODU5NTExODAsImV4cCI6MTc4NTk1NDc4MCwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.gpN21puDQDtd1kI-7I1ECFLbDNMJ7YKvmyOSuDPDR50",
    "expiresIn": 3600,
    "refreshToken": "y7godqnu4dExFLfAIEyKmuaIXLTS4OWRXLxlCjHdTRTi92OuJczlzjOW37FzBa0xOhRFHsiZnjx+iCrb6swNYQ==",
    "refreshTokenExpiration": "2026-08-12T17:33:00.5046919Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 2d. Profile - Complete Lawyer Profile (Valid)

**Request:** POST http://localhost:5049/api/lawyers/profile/complete

**Body:**
`json
{
  "DateOfBirth": "1985-05-05",
  "Level": 1,
  "PhoneNumber": "+201098765432",
  "NationalNumber": "22586385563366",
  "Address": "Riyadh",
  "Gender": 1,
  "Bio": "Expert"
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


### 2e. Profile - Get Lawyer Profile

**Request:** GET http://localhost:5049/api/lawyers/profile

**Response Status:** 401

**Response Body:**
Response status code does not indicate success: 401 (Unauthorized).
---


### 2f. Profile - Update Lawyer Profile (Valid)

**Request:** PUT http://localhost:5049/api/lawyers/profile

**Body:**
`json
{
  "DateOfBirth": "1985-05-06",
  "Level": 1,
  "PhoneNumber": "+201098765433",
  "NationalNumber": "22586385563366",
  "Address": "Dammam",
  "Gender": 1,
  "FullName": "Lawyer E2E Updated"
}
``n
**Response Status:** 401

**Response Body:**
Response status code does not indicate success: 401 (Unauthorized).
---


### 2f2. Setup - Re-Login Lawyer (After Profile Update)

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "lawyer_e2e_308114618@test.com",
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
      "id": "ae4391e2-732f-4773-0b13-08def31747a9",
      "email": "lawyer_e2e_308114618@test.com",
      "fullName": "Lawyer E2E",
      "role": "Lawyer"
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZTQzOTFlMi03MzJmLTQ3NzMtMGIxMy0wOGRlZjMxNzQ3YTkiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImFlNDM5MWUyLTczMmYtNDc3My0wYjEzLTA4ZGVmMzE3NDdhOSIsImVtYWlsIjoibGF3eWVyX2UyZV8zMDgxMTQ2MThAdGVzdC5jb20iLCJuYW1lIjoiTGF3eWVyIEUyRSIsInNlY3VyaXR5X3N0YW1wIjoiRERWMzJJVEtWTjNNMjNIWFlWU05SQVdCNzRaTUZYVDMiLCJqdGkiOiI5OWYwODUwMy1hNjgxLTQ2ZDgtOThhOS02ODI2ZDJlMzM1N2UiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODU5NTExODAsImV4cCI6MTc4NTk1NDc4MCwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.QoFd8YimRe_qJWNEf2grzomLDO9JsVYAiTshN-cvuzo",
    "expiresIn": 3600,
    "refreshToken": "xu3pEIU9DuSMTRGZQm9yWwmKL9bVLSzen8azwOeYqLUQYf82ph8oi52EZrGJu8oIko6FH4IwBDhNzqLaWCHacg==",
    "refreshTokenExpiration": "2026-08-12T17:33:00.9586592Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 2g. Security - Lawyer Token on Client Endpoint (403)

**Request:** GET http://localhost:5049/api/clients/profile

**Response Status:** 403

**Response Body:**
Response status code does not indicate success: 403 (Forbidden).
---


### 2h. Security - Client Token on Lawyer Endpoint (403)

**Request:** GET http://localhost:5049/api/lawyers/profile

**Response Status:** 403

**Response Body:**
Response status code does not indicate success: 403 (Forbidden).
---


### 2i. Validation - Complete Client Profile (Invalid Data 400)

**Request:** POST http://localhost:5049/api/clients/profile/complete

**Body:**
`json
{
  "Gender": 99,
  "PhoneNumber": "",
  "DateOfBirth": "3000-01-01"
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
    "Gender": [
      "الجنس يجب أن يكون صالحاً"
    ],
    "DateOfBirth": [
      "تاريخ الميلاد يجب أن يكون في الماضي"
    ],
    "PhoneNumber": [
      "رقم الهاتف مطلوب",
      "رقم الهاتف يجب أن يكون بالتنسيق المصري +20XXXXXXXXXX"
    ]
  },
  "traceId": "00-aba7fadb743b8a26ac7f96528a3786c2-5d0674361050aad9-00"
}
``n---


### 3a. Verification - Lawyer Uploads Document (Valid)

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**

- Documents[0].ExpirationDate = 2030-01-01

- UserId = ae4391e2-732f-4773-0b13-08def31747a9

- Documents[0].Type = 1

- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\dummy_e2e_id.jpg]


**Response Status:** 200

**Response Body:**
```json
{"success":true,"data":{"uploadedDocuments":[{"fileName":"dummy_e2e_id.jpg","type":1}],"failedDocuments":[]},"message":null,"errors":null,"statusCode":200}
```n---


### 3b. Verification - Client Uploads Document (Valid)

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**

- Documents[0].ExpirationDate = 2030-01-01

- UserId = 1bc3a90c-e4db-4e96-0b14-08def31747a9

- Documents[0].Type = 1

- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\dummy_e2e_id.jpg]


**Response Status:** 200

**Response Body:**
```json
{"success":true,"data":{"uploadedDocuments":[{"fileName":"dummy_e2e_id.jpg","type":1}],"failedDocuments":[]},"message":null,"errors":null,"statusCode":200}
```n---


### 3c. Security - Upload without Token (401)

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**

- Documents[0].ExpirationDate = 2030-01-01

- UserId = 1bc3a90c-e4db-4e96-0b14-08def31747a9

- Documents[0].Type = 1

- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\dummy_e2e_id.jpg]


**Response Status:** 200

**Response Body:**
```json
{"success":true,"data":{"uploadedDocuments":[],"failedDocuments":[{"fileName":"dummy_e2e_id.jpg","type":1,"error":"You already uploaded this document before. Wait untill admin verifies your document"}]},"message":null,"errors":null,"statusCode":200}
```n---


### 3d. Security - Lawyer Uploads for Client (Should Fail)

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**

- Documents[0].ExpirationDate = 2030-01-01

- UserId = 1bc3a90c-e4db-4e96-0b14-08def31747a9

- Documents[0].Type = 2

- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\dummy_e2e_id.jpg]


**Response Status:** 200

**Response Body:**
```json
{"success":true,"data":{"uploadedDocuments":[{"fileName":"dummy_e2e_id.jpg","type":2}],"failedDocuments":[]},"message":null,"errors":null,"statusCode":200}
```n---


### 4a. Admin - Get Pending Verifications List

**Request:** GET http://localhost:5049/api/admin/verifications?PageNumber=1&PageSize=20

**Response Status:** 200

**Response Body:**
`json
{
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 1,
  "totalRecords": 13,
  "hasNextPage": false,
  "hasPreviousPage": false,
  "success": true,
  "data": [
    {
      "lawyerId": "2458bfd5-a6a7-466e-91be-08def3127d6a",
      "fullName": "Lawyer E2E",
      "email": "lawyer_e2e_724565145@test.com",
      "phoneNumber": null,
      "pendingDocumentCount": 1,
      "verifiedDocumentCount": 0,
      "rejectedDocumentCount": 1
    },
    {
      "lawyerId": "dd7c9310-d497-4f08-91c0-08def3127d6a",
      "fullName": "Lawyer E2E",
      "email": "lawyer_e2e_363853164@test.com",
      "phoneNumber": null,
      "pendingDocumentCount": 1,
      "verifiedDocumentCount": 0,
      "rejectedDocumentCount": 1
    },
    {
      "lawyerId": "ac6b4677-e497-415d-62ad-08def313b097",
      "fullName": "Lawyer E2E",
      "email": "lawyer_e2e_56933503@test.com",
      "phoneNumber": null,
      "pendingDocumentCount": 1,
      "verifiedDocumentCount": 0,
      "rejectedDocumentCount": 1
    },
    {
      "lawyerId": "e01a7132-3b90-4e74-62af-08def313b097",
      "fullName": "Lawyer E2E",
      "email": "lawyer_e2e_589242065@test.com",
      "phoneNumber": null,
      "pendingDocumentCount": 1,
      "verifiedDocumentCount": 0,
      "rejectedDocumentCount": 1
    },
    {
      "lawyerId": "2273249e-d5af-43d8-62b1-08def313b097",
      "fullName": "Lawyer E2E",
      "email": "lawyer_e2e_323761447@test.com",
      "phoneNumber": "+201098765433",
      "pendingDocumentCount": 1,
      "verifiedDocumentCount": 0,
      "rejectedDocumentCount": 1
    },
    {
      "lawyerId": "976b0827-6e51-4741-62b3-08def313b097",
      "fullName": "Lawyer E2E",
      "email": "lawyer_e2e_886200843@test.com",
      "phoneNumber": "+201098765432",
      "pendingDocumentCount": 1,
      "verifiedDocumentCount": 0,
      "rejectedDocumentCount": 1
    },
    {
      "lawyerId": "17ecb2e6-931b-485b-62b5-08def313b097",
      "fullName": "Lawyer E2E",
      "email": "lawyer_e2e_860841786@test.com",
      "phoneNumber": "+201098765432",
      "pendingDocumentCount": 1,
      "verifiedDocumentCount": 0,
      "rejectedDocumentCount": 1
    },
    {
      "lawyerId": "ae4391e2-732f-4773-0b13-08def31747a9",
      "fullName": "Lawyer E2E",
      "email": "lawyer_e2e_308114618@test.com",
      "phoneNumber": "+201098765432",
      "pendingDocumentCount": 1,
      "verifiedDocumentCount": 0,
      "rejectedDocumentCount": 0
    },
    {
      "lawyerId": "65371797-7bf8-44f5-e472-08def300e686",
      "fullName": "Lawyer Verification",
      "email": "lawyer_verification_1518525995@test.com",
      "phoneNumber": null,
      "pendingDocumentCount": 1,
      "verifiedDocumentCount": 0,
      "rejectedDocumentCount": 0
    },
    {
      "lawyerId": "a2f8005f-2feb-4e6a-e474-08def300e686",
      "fullName": "Lawyer Verification",
      "email": "lawyer_verification_852046148@test.com",
      "phoneNumber": null,
      "pendingDocumentCount": 2,
      "verifiedDocumentCount": 0,
      "rejectedDocumentCount": 0
    },
    {
      "lawyerId": "6a7b8032-b6a3-46bf-e476-08def300e686",
      "fullName": "Lawyer Verification",
      "email": "lawyer_verification_423361630@test.com",
      "phoneNumber": null,
      "pendingDocumentCount": 1,
      "verifiedDocumentCount": 0,
      "rejectedDocumentCount": 1
    },
    {
      "lawyerId": "88404714-5076-4ed1-e479-08def300e686",
      "fullName": "Lawyer Verification",
      "email": "lawyer_verification_730984203@test.com",
      "phoneNumber": null,
      "pendingDocumentCount": 2,
      "verifiedDocumentCount": 0,
      "rejectedDocumentCount": 0
    },
    {
      "lawyerId": "58cfbed4-4c92-4c32-b0fd-08def30c75ec",
      "fullName": "Lawyer Verification",
      "email": "lawyer_verification_324320347@test.com",
      "phoneNumber": null,
      "pendingDocumentCount": 1,
      "verifiedDocumentCount": 0,
      "rejectedDocumentCount": 0
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 4b. Admin - Get Lawyer Verification Details

**Request:** GET http://localhost:5049/api/admin/verifications/ae4391e2-732f-4773-0b13-08def31747a9

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "lawyerId": "ae4391e2-732f-4773-0b13-08def31747a9",
    "fullName": "Lawyer E2E",
    "email": "lawyer_e2e_308114618@test.com",
    "phoneNumber": "+201098765432",
    "accountStatus": "PendingReview",
    "isFullyVerified": false,
    "documents": [
      {
        "documentId": "edffa175-539a-483a-4b8d-08def3175505",
        "documentType": "NationalIdFront",
        "status": "Pending",
        "fileName": "dummy_e2e_id.jpg",
        "contentType": "image/jpeg",
        "expirationDate": "2030-01-01",
        "reviewedAt": null,
        "rejectionReason": null,
        "contentUrl": "/api/admin/verifications/documents/edffa175-539a-483a-4b8d-08def3175505/content"
      }
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 4c. Admin - Get Client Verification Details

**Request:** GET http://localhost:5049/api/admin/verifications/1bc3a90c-e4db-4e96-0b14-08def31747a9

**Response Status:** 404

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "Lawyer was not found.",
  "errors": null,
  "statusCode": 404
}
``n---


### 4d. Admin - Get Lawyer Document Content

**Request:** GET http://localhost:5049/api/admin/verifications/documents/edffa175-539a-483a-4b8d-08def3175505/content

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "downloadUrl": "https://msahvjipdwvgdartpeqj.supabase.co/storage/v1/object/public/smart-court-files/ae4391e2-732f-4773-0b13-08def31747a9/national-id/8b4f1189-bdd4-4b4b-9b05-2c00941f351b.jpg",
    "contentType": "image/jpeg",
    "fileName": "dummy_e2e_id.jpg"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 4e. Admin - Reject Lawyer Document

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/edffa175-539a-483a-4b8d-08def3175505

**Body:**
`json
{
  "RejectionReason": "Image is too blurry.",
  "Decision": 2
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "documentId": "edffa175-539a-483a-4b8d-08def3175505",
    "documentStatus": "Rejected",
    "lawyerAccountStatus": "Rejected",
    "isFullyVerified": false
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 4f. Lawyer - Delete Rejected Document

**Request:** DELETE http://localhost:5049/api/UserVerification?UserId=ae4391e2-732f-4773-0b13-08def31747a9&DocumentId=edffa175-539a-483a-4b8d-08def3175505

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 4g. Lawyer - Re-uploads Document

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**

- Documents[0].ExpirationDate = 2030-01-01

- UserId = ae4391e2-732f-4773-0b13-08def31747a9

- Documents[0].Type = 2

- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\dummy_e2e_id.jpg]


**Response Status:** 200

**Response Body:**
```json
{"success":true,"data":{"uploadedDocuments":[{"fileName":"dummy_e2e_id.jpg","type":2}],"failedDocuments":[]},"message":null,"errors":null,"statusCode":200}
```n---


### 4h. Admin - Get Lawyer Verification Details (Re-upload)

**Request:** GET http://localhost:5049/api/admin/verifications/ae4391e2-732f-4773-0b13-08def31747a9

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "lawyerId": "ae4391e2-732f-4773-0b13-08def31747a9",
    "fullName": "Lawyer E2E",
    "email": "lawyer_e2e_308114618@test.com",
    "phoneNumber": "+201098765432",
    "accountStatus": "Rejected",
    "isFullyVerified": false,
    "documents": [
      {
        "documentId": "2e6e9561-b7ac-475c-4b90-08def3175505",
        "documentType": "NationalIdBack",
        "status": "Pending",
        "fileName": "dummy_e2e_id.jpg",
        "contentType": "image/jpeg",
        "expirationDate": "2030-01-01",
        "reviewedAt": null,
        "rejectionReason": null,
        "contentUrl": "/api/admin/verifications/documents/2e6e9561-b7ac-475c-4b90-08def3175505/content"
      }
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 4i. Admin - Approve Lawyer Document

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/2e6e9561-b7ac-475c-4b90-08def3175505

**Body:**
`json
{
  "RejectionReason": null,
  "Decision": 1
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "documentId": "2e6e9561-b7ac-475c-4b90-08def3175505",
    "documentStatus": "Verified",
    "lawyerAccountStatus": "Unverified",
    "isFullyVerified": false
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 4k. Security - Lawyer on Admin Endpoint (403)

**Request:** GET http://localhost:5049/api/admin/verifications

**Response Status:** 401

**Response Body:**
Response status code does not indicate success: 401 (Unauthorized).
---


### 5a. Lawyer - Get Public Profile (Anonymous)

**Request:** GET http://localhost:5049/api/lawyers/public/ae4391e2-732f-4773-0b13-08def31747a9

**Response Status:** 404

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "المحامي غير موجود",
  "errors": null,
  "statusCode": 404
}
``n---


### 5b. Client - Delete Profile

**Request:** DELETE http://localhost:5049/api/clients/profile

**Body:**
`json
{
  "CurrentPassword": "Password123!"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "message": "تم حذف الملف الشخصي بنجاح.",
  "errors": null,
  "statusCode": 200
}
``n---


### 5b2. Setup - Re-Login Lawyer (After Approval)

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "lawyer_e2e_308114618@test.com",
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
      "id": "ae4391e2-732f-4773-0b13-08def31747a9",
      "email": "lawyer_e2e_308114618@test.com",
      "fullName": "Lawyer E2E",
      "role": "Lawyer"
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZTQzOTFlMi03MzJmLTQ3NzMtMGIxMy0wOGRlZjMxNzQ3YTkiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImFlNDM5MWUyLTczMmYtNDc3My0wYjEzLTA4ZGVmMzE3NDdhOSIsImVtYWlsIjoibGF3eWVyX2UyZV8zMDgxMTQ2MThAdGVzdC5jb20iLCJuYW1lIjoiTGF3eWVyIEUyRSIsInNlY3VyaXR5X3N0YW1wIjoiRERWMzJJVEtWTjNNMjNIWFlWU05SQVdCNzRaTUZYVDMiLCJqdGkiOiI3NWUyMjdmNi00YTg3LTQxNGItYmU0Yy04MDhhODZiYTllM2IiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODU5NTExODcsImV4cCI6MTc4NTk1NDc4NywiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.KoaPPr9nn00i3dCBx8mAXwKUpMK67n96n_GB5UZGirs",
    "expiresIn": 3600,
    "refreshToken": "RCbcOvuiKkKqs2QrwnUPjlrSkXBx/nC99OkXqcOYDIRBxTXIqdvg5f1eo/zpLiRmkZYvxLNb4A5faPYeTbdKug==",
    "refreshTokenExpiration": "2026-08-12T17:33:07.8019244Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 5c. Lawyer - Delete Profile

**Request:** DELETE http://localhost:5049/api/lawyers/profile

**Body:**
`json
{
  "CurrentPassword": "Password123!"
}
``n
**Response Status:** 401

**Response Body:**
Response status code does not indicate success: 401 (Unauthorized).
---


### 5d. Setup - Login Client After Delete (Should Fail)

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "client_e2e_308114618@test.com",
  "Password": "Password123!"
}
``n
**Response Status:** 401

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "البريد الإلكتروني أو كلمة المرور غير صحيحة.",
  "errors": null,
  "statusCode": 401
}
``n---



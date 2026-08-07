# Case Slice HTTP Tests End-to-End Workflow Report

Generated at 2026-08-06 17:43:07


### Register Client

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
  "Password": "Password123!",
  "FullName": "Test Client",
  "ConfirmPassword": "Password123!",
  "Email": "client_case_test_20260806174307@example.com"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "e9c43dfc-5ae8-4a49-b8c2-08def3c8bd8a",
    "email": "client_case_test_20260806174307@example.com",
    "fullName": "Test Client",
    "role": "Client"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


Found confirmation URL for client_case_test_20260806174307@example.com: http://localhost:5173/verify-email?userId=e9c43dfc-5ae8-4a49-b8c2-08def3c8bd8a&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5bjhwVmUxaXdDc3gxL1ZCeGREV1hienQ0ZVdtb0xHR2dEcUNqckFZdlZGZUlhWXJ2R2ZxODJiSnA0TkttNUYvS3ZNWXdCWCtXSkRWc0ExazkyUExMUW1hMXF1bE9YS2dJc09mMGtNMERjN0FINy9rYlR1eW1DYVNYUCs4UW5RcEtGUHN0Z0plNHkrUGZnUDcremJ1QzI4N0x6WUdpUlF6Nzd5QTA3U3IweDBSem9ZUTNjdHpkYVR4QlZwWlBOTjlzc094NnphWDBxcWl2NGdwd0dDZFMvQ2xPYzkxRVpwYU02dVJyTHVpTkxhZz09

### Confirm Email for client_case_test_20260806174307@example.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=e9c43dfc-5ae8-4a49-b8c2-08def3c8bd8a&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5bjhwVmUxaXdDc3gxL1ZCeGREV1hienQ0ZVdtb0xHR2dEcUNqckFZdlZGZUlhWXJ2R2ZxODJiSnA0TkttNUYvS3ZNWXdCWCtXSkRWc0ExazkyUExMUW1hMXF1bE9YS2dJc09mMGtNMERjN0FINy9rYlR1eW1DYVNYUCs4UW5RcEtGUHN0Z0plNHkrUGZnUDcremJ1QzI4N0x6WUdpUlF6Nzd5QTA3U3IweDBSem9ZUTNjdHpkYVR4QlZwWlBOTjlzc094NnphWDBxcWl2NGdwd0dDZFMvQ2xPYzkxRVpwYU02dVJyTHVpTkxhZz09

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
  "Email": "client_case_test_20260806174307@example.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "e9c43dfc-5ae8-4a49-b8c2-08def3c8bd8a",
      "email": "client_case_test_20260806174307@example.com",
      "fullName": "Test Client",
      "role": "Client"
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJlOWM0M2RmYy01YWU4LTRhNDktYjhjMi0wOGRlZjNjOGJkOGEiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImU5YzQzZGZjLTVhZTgtNGE0OS1iOGMyLTA4ZGVmM2M4YmQ4YSIsImVtYWlsIjoiY2xpZW50X2Nhc2VfdGVzdF8yMDI2MDgwNjE3NDMwN0BleGFtcGxlLmNvbSIsIm5hbWUiOiJUZXN0IENsaWVudCIsInNlY3VyaXR5X3N0YW1wIjoiNVQ0UTVVSlNaVkNGNU5WNFNYV1RDRlRFSEJMNTJQSzUiLCJqdGkiOiIzOGFmODQ1NS1lN2QxLTQ2YjUtOTkxOS0xY2NlMzI2ZWJlMWQiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDbGllbnQiLCJuYmYiOjE3ODYwMjczODcsImV4cCI6MTc4NjAzMDk4NywiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.VvQjEnkfQUSsWUvyMppeWmDQvLsjsKC-_K4_8hBdMAQ",
    "expiresIn": 3600,
    "refreshToken": "xSE5dFsrRYTn3u9rrUGgHzIvvLhyXaMaxSqJrIRFbPL2dgoS8W595jRVJZ/1waUUbVTmS2EgFEQeYyPnDUNpRA==",
    "refreshTokenExpiration": "2026-08-13T14:43:07.6492767Z"
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
  "Password": "Password123!",
  "FullName": "Test Lawyer",
  "ConfirmPassword": "Password123!",
  "Email": "lawyer_case_test_20260806174307@example.com"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "a9d74605-d399-4ef8-b8c3-08def3c8bd8a",
    "email": "lawyer_case_test_20260806174307@example.com",
    "fullName": "Test Lawyer",
    "role": "Lawyer"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


Found confirmation URL for lawyer_case_test_20260806174307@example.com: http://localhost:5173/verify-email?userId=a9d74605-d399-4ef8-b8c3-08def3c8bd8a&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIveGNpSXRsQ0lJdm1ZeklEb2xjTEQ4bytxSDJFcUtlQkJMK0ErdVlCdVd3UkxtUXFkTnFvRjNLdzVtSU5Yc0NvUitPdnRadlBxUHZrSWZXbE1VNW95MWE4b3JJYU15OTQ1c2xOMlVkVU9rMlc5QVJUN1RUTkFIa3AyZzRFMTRReHFRYWtXYzJBREZnZ045Rk5na0FaazNPSEw3aXhtdVJJRHN3UlZUaG94STNHempVZ1FkSDhCZ01USVhqdXp2bkY3bVh2NDNGdlErRnljSittNXdHdEk1VVBoNmNxcmRiaVVIVnl1QWRmNGJZZz09

### Confirm Email for lawyer_case_test_20260806174307@example.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=a9d74605-d399-4ef8-b8c3-08def3c8bd8a&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIveGNpSXRsQ0lJdm1ZeklEb2xjTEQ4bytxSDJFcUtlQkJMK0ErdVlCdVd3UkxtUXFkTnFvRjNLdzVtSU5Yc0NvUitPdnRadlBxUHZrSWZXbE1VNW95MWE4b3JJYU15OTQ1c2xOMlVkVU9rMlc5QVJUN1RUTkFIa3AyZzRFMTRReHFRYWtXYzJBREZnZ045Rk5na0FaazNPSEw3aXhtdVJJRHN3UlZUaG94STNHempVZ1FkSDhCZ01USVhqdXp2bkY3bVh2NDNGdlErRnljSittNXdHdEk1VVBoNmNxcmRiaVVIVnl1QWRmNGJZZz09

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
  "Email": "lawyer_case_test_20260806174307@example.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "a9d74605-d399-4ef8-b8c3-08def3c8bd8a",
      "email": "lawyer_case_test_20260806174307@example.com",
      "fullName": "Test Lawyer",
      "role": "Lawyer"
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhOWQ3NDYwNS1kMzk5LTRlZjgtYjhjMy0wOGRlZjNjOGJkOGEiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImE5ZDc0NjA1LWQzOTktNGVmOC1iOGMzLTA4ZGVmM2M4YmQ4YSIsImVtYWlsIjoibGF3eWVyX2Nhc2VfdGVzdF8yMDI2MDgwNjE3NDMwN0BleGFtcGxlLmNvbSIsIm5hbWUiOiJUZXN0IExhd3llciIsInNlY3VyaXR5X3N0YW1wIjoiMlQ3VFdBVEdENjQzWkZGTE81WEFRUUpJRVJMQ0tXNloiLCJqdGkiOiI3MGE2YTg1Ni1kNGQ3LTQ2ZDItOWViMS1iMzI2MGYwY2UwYmEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODYwMjczODgsImV4cCI6MTc4NjAzMDk4OCwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.AA3Mpv0Ac5YIJIW2INrkxNLmZL6zIqoBgeFsJiOvvNk",
    "expiresIn": 3600,
    "refreshToken": "6WBrT9P3NftYf5WPl2gEV6jOrzb87xxlPs4skkhfZ4QRyD8wnDbO21cTj8pjn/RcJM1fw5xTLrdJyAdtCx6SwQ==",
    "refreshTokenExpiration": "2026-08-13T14:43:08.0685269Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Create Case (400 Validation Error)

**Request:** POST http://localhost:5049/api/Case

**Body:**
(multipart/form-data)

**Response Status:** 400

**Response Body:**
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 84 105 116 108 101 34 58 91 34 84 104 101 32 84 105 116 108 101 32 102 105 101 108 100 32 105 115 32 114 101 113 117 105 114 101 100 46 34 44 34 67 97 115 101 32 116 105 116 108 101 32 99 97 110 39 116 32 98 101 32 101 109 112 116 121 34 93 44 34 68 111 99 117 109 101 110 116 115 34 58 91 34 84 104 101 32 68 111 99 117 109 101 110 116 115 32 102 105 101 108 100 32 105 115 32 114 101 113 117 105 114 101 100 46 34 93 44 34 68 101 115 99 114 105 112 116 105 111 110 34 58 91 34 84 104 101 32 68 101 115 99 114 105 112 116 105 111 110 32 102 105 101 108 100 32 105 115 32 114 101 113 117 105 114 101 100 46 34 44 34 67 97 115 101 32 100 101 115 99 114 105 112 116 105 111 110 32 99 97 110 39 116 32 98 101 32 101 109 112 116 121 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 97 101 53 49 97 53 97 51 53 97 52 56 54 99 100 48 49 53 49 51 100 49 50 56 48 49 100 54 49 54 51 56 45 55 51 52 97 48 49 57 51 51 100 99 98 102 52 50 99 45 48 48 34 125
---


### Create Case (Valid Success)

**Request:** POST http://localhost:5049/api/Case

**Body:**
(multipart/form-data)

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "caseId": "e78cdfdb-e6f2-4b3e-bb9f-8bd4841bdfeb",
    "failedDocuments": []
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
``n---


### Get Case By ID

**Request:** GET http://localhost:5049/api/Case/e78cdfdb-e6f2-4b3e-bb9f-8bd4841bdfeb

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "e78cdfdb-e6f2-4b3e-bb9f-8bd4841bdfeb",
    "clientId": "e9c43dfc-5ae8-4a49-b8c2-08def3c8bd8a",
    "title": "Valid Case Title",
    "description": "Detailed description of the case for testing.",
    "governorate": "Cairo",
    "city": "Maadi",
    "status": "Submitted",
    "createdAt": "2026-08-06T14:43:10.0590601",
    "documents": [
      {
        "id": "67594610-56e9-4e87-d720-08def3c9091e",
        "fileName": "dummy_case.pdf",
        "fileUrl": "e9c43dfc-5ae8-4a49-b8c2-08def3c8bd8a/case-documents/e9d124fe-1751-441a-825f-a0a733f69d0e.pdf",
        "contentType": "application/octet-stream"
      }
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Get Case By ID (404 Not Found)

**Request:** GET http://localhost:5049/api/Case/30f1abc5-31c5-497b-8244-49634686b534

**Response Status:** 404

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": [
    "Case not found"
  ],
  "statusCode": 404
}
``n---


### Get All Cases

**Request:** GET http://localhost:5049/api/Case

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": [
    {
      "id": "e78cdfdb-e6f2-4b3e-bb9f-8bd4841bdfeb",
      "title": "Valid Case Title",
      "status": "Submitted",
      "createdAt": "2026-08-06T14:43:10.0590601",
      "documentCount": 1
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Update Case (Valid Success)

**Request:** PUT http://localhost:5049/api/Case/e78cdfdb-e6f2-4b3e-bb9f-8bd4841bdfeb

**Body:**
(multipart/form-data)

**Response Status:** 400

**Response Body:**
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 68 111 99 117 109 101 110 116 115 34 58 91 34 84 104 101 32 68 111 99 117 109 101 110 116 115 32 102 105 101 108 100 32 105 115 32 114 101 113 117 105 114 101 100 46 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 57 101 48 54 97 99 48 48 57 99 52 53 99 102 52 49 54 54 49 53 52 48 57 52 54 52 52 48 100 98 101 54 45 55 56 53 49 99 54 102 49 57 97 99 50 52 57 56 53 45 48 48 34 125
---


### Create Case to Delete

**Request:** POST http://localhost:5049/api/Case

**Body:**
(multipart/form-data)

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "caseId": "40a2a9f5-cf4b-4189-8901-5cc12bb7b50e",
    "failedDocuments": []
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
``n---


### Delete Case (Success)

**Request:** DELETE http://localhost:5049/api/Case/40a2a9f5-cf4b-4189-8901-5cc12bb7b50e

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


### Create Case (Stress - Malicious Payload)

**Request:** POST http://localhost:5049/api/Case

**Body:**
(multipart/form-data)

**Response Status:** 400

**Response Body:**
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 68 111 99 117 109 101 110 116 115 34 58 91 34 84 104 101 32 68 111 99 117 109 101 110 116 115 32 102 105 101 108 100 32 105 115 32 114 101 113 117 105 114 101 100 46 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 54 52 98 56 48 97 98 54 56 49 49 101 53 50 56 99 50 97 102 49 50 102 102 54 99 53 99 53 99 50 100 53 45 102 99 57 54 102 52 102 55 51 48 97 50 49 51 48 52 45 48 48 34 125
---


### Review Case (AI Request)

**Request:** POST http://localhost:5049/api/cases/e78cdfdb-e6f2-4b3e-bb9f-8bd4841bdfeb/review

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
    "id": "8aa71741-71ac-4b82-b460-52571b24adc0",
    "caseId": "e78cdfdb-e6f2-4b3e-bb9f-8bd4841bdfeb",
    "isLatest": true,
    "createdAt": "2026-08-06T14:43:19.6970224Z",
    "reviewPoints": [
      {
        "id": "9b211956-b4fe-4960-bd7e-caf4a1756289",
        "description": "تمت مراجعة القضية بنجاح.",
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

**Request:** GET http://localhost:5049/api/cases/e78cdfdb-e6f2-4b3e-bb9f-8bd4841bdfeb/reviews/latest

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "8aa71741-71ac-4b82-b460-52571b24adc0",
    "caseId": "e78cdfdb-e6f2-4b3e-bb9f-8bd4841bdfeb",
    "isLatest": true,
    "createdAt": "2026-08-06T14:43:19.6970224",
    "reviewPoints": [
      {
        "id": "9b211956-b4fe-4960-bd7e-caf4a1756289",
        "description": "تمت مراجعة القضية بنجاح.",
        "type": "Suggestion"
      }
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Finalize Case (Transition to Matched)

**Request:** POST http://localhost:5049/api/Case/e78cdfdb-e6f2-4b3e-bb9f-8bd4841bdfeb/finalize

**Body:**
`json
{}
``n
**Response Status:** 400

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "AI analysis failed. Please try again.",
  "errors": null,
  "statusCode": 400
}
``n---


### Create Proposal (Client to Lawyer)

**Request:** POST http://localhost:5049/api/proposals

**Body:**
`json
{
  "Message": "I would like to hire you for this case.",
  "LegalCaseId": "e78cdfdb-e6f2-4b3e-bb9f-8bd4841bdfeb",
  "LawyerUserId": "a9d74605-d399-4ef8-b8c3-08def3c8bd8a"
}
``n
**Response Status:** 404

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "Case was not found.",
  "errors": null,
  "statusCode": 404
}
``n---


### Lawyer Get Proposals

**Request:** GET http://localhost:5049/api/proposals

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "items": [],
    "page": 1,
    "pageSize": 10,
    "totalCount": 0,
    "hasNextPage": false
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Lawyer Accepts Proposal

**Request:** POST http://localhost:5049/api/proposals//accept

**Body:**
`json
{}
``n
**Response Status:** 404

**Response Body:**
Response status code does not indicate success: 404 (Not Found).
---



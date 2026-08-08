# Case Slice HTTP Tests End-to-End Workflow Report

Generated at 2026-08-08 19:21:20


### Register Client

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
  "ConfirmPassword": "Password123!",
  "Password": "Password123!",
  "FullName": "Test Client",
  "Email": "client_case_test_20260808192120@example.com"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "0a1eccf3-7e73-4b8c-7805-08def54e43c0",
    "email": "client_case_test_20260808192120@example.com",
    "fullName": "Test Client",
    "role": "Client"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


Found confirmation URL for client_case_test_20260808192120@example.com: http://localhost:5173/verify-email?userId=0a1eccf3-7e73-4b8c-7805-08def54e43c0&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5K3BsYU9wT05GbjVOanZGTkVBSkpaZkp0eDlXZjhFRUdiYXhMakxDdml0ODFWcUNJeWVzUDJ1L0NOcHlZZWVnSVA5Yld4ZDlDeG5tUG5XeDVBVzlQVzVBbHdwd1poODlQeGhiVEFUQnVURE56Q3JadlVHUnhFalJKS2FDTll1S2JNbVVQNEtRbTRMbmtZQy84MkNXZmgzckJxOEdBditkOWVJZC8vNU9iNStVTVdMa015cTFoNTJJRlMva041ZGU0VFhBR0N4ajdRb3NHVk1sMEgzV2VXZjN3VWlIaXhSU0tuUzZ0MzVFekVnZz09

### Confirm Email for client_case_test_20260808192120@example.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=0a1eccf3-7e73-4b8c-7805-08def54e43c0&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5K3BsYU9wT05GbjVOanZGTkVBSkpaZkp0eDlXZjhFRUdiYXhMakxDdml0ODFWcUNJeWVzUDJ1L0NOcHlZZWVnSVA5Yld4ZDlDeG5tUG5XeDVBVzlQVzVBbHdwd1poODlQeGhiVEFUQnVURE56Q3JadlVHUnhFalJKS2FDTll1S2JNbVVQNEtRbTRMbmtZQy84MkNXZmgzckJxOEdBditkOWVJZC8vNU9iNStVTVdMa015cTFoNTJJRlMva041ZGU0VFhBR0N4ajdRb3NHVk1sMEgzV2VXZjN3VWlIaXhSU0tuUzZ0MzVFekVnZz09

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
  "Email": "client_case_test_20260808192120@example.com",
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
      "id": "0a1eccf3-7e73-4b8c-7805-08def54e43c0",
      "email": "client_case_test_20260808192120@example.com",
      "fullName": "Test Client",
      "role": "Client",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwYTFlY2NmMy03ZTczLTRiOGMtNzgwNS0wOGRlZjU0ZTQzYzAiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjBhMWVjY2YzLTdlNzMtNGI4Yy03ODA1LTA4ZGVmNTRlNDNjMCIsImVtYWlsIjoiY2xpZW50X2Nhc2VfdGVzdF8yMDI2MDgwODE5MjEyMEBleGFtcGxlLmNvbSIsIm5hbWUiOiJUZXN0IENsaWVudCIsInNlY3VyaXR5X3N0YW1wIjoiSUc2M1BINUs1VlpFSzNTWFpJTzJGSURCWlRQNEhIQjIiLCJqdGkiOiJhNTA2ZmE1YS00NWViLTQ4ODYtYWZkMC1hZGE3ZGU2YTVmZmUiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDbGllbnQiLCJuYmYiOjE3ODYyMDYwODAsImV4cCI6MTc4NjIwNjk4MCwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.68Gn9KwIrurW-yK7xyofwChGyvLU10acDNgJOI7veT4",
    "expiresIn": 900,
    "refreshToken": "hFIgMDwPfvcjjhzKRiTbBVV4+WOP/FR1ulGWa7NISGVsGHNiAbpNX6dcJc1RE9gOItfztLewcsDe4Df1AKALRA==",
    "refreshTokenExpiration": "2026-08-15T16:21:20.7726594Z"
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
  "ConfirmPassword": "Password123!",
  "Password": "Password123!",
  "FullName": "Test Lawyer",
  "Email": "lawyer_case_test_20260808192120@example.com"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "e3f0b230-f763-4642-7806-08def54e43c0",
    "email": "lawyer_case_test_20260808192120@example.com",
    "fullName": "Test Lawyer",
    "role": "Lawyer"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


Found confirmation URL for lawyer_case_test_20260808192120@example.com: http://localhost:5173/verify-email?userId=e3f0b230-f763-4642-7806-08def54e43c0&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIreGcrUlZtTjUzbURXUitCdlIzRXAybG4ycGFHWnJ3RDJwQ3d4UXZrUTBZdGkwY0NMVDJkOFQvbU82V0cvQjI2cGducGhtTHN6aGtOQWNOQStzcGRqYVZVSFhzS2hhY0JNTjdLeWMwdHc5amg5aHlqR3R1VmU4TTM4Ukx3NmhPa1ROSENZWDVBYjdPd1FyQXRNR3JvYTVHcHRwVlgvMmJyQ0FuYkM0OUh3K3k0QTFmbGlsZmRmWEp5d1FsaVFCUkNoeG1YUEpzL2pFTHFWK1dpaFRxWTYyUEpVMWZJUjByeGdSOVRtbDVmUEFvQT09

### Confirm Email for lawyer_case_test_20260808192120@example.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=e3f0b230-f763-4642-7806-08def54e43c0&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIreGcrUlZtTjUzbURXUitCdlIzRXAybG4ycGFHWnJ3RDJwQ3d4UXZrUTBZdGkwY0NMVDJkOFQvbU82V0cvQjI2cGducGhtTHN6aGtOQWNOQStzcGRqYVZVSFhzS2hhY0JNTjdLeWMwdHc5amg5aHlqR3R1VmU4TTM4Ukx3NmhPa1ROSENZWDVBYjdPd1FyQXRNR3JvYTVHcHRwVlgvMmJyQ0FuYkM0OUh3K3k0QTFmbGlsZmRmWEp5d1FsaVFCUkNoeG1YUEpzL2pFTHFWK1dpaFRxWTYyUEpVMWZJUjByeGdSOVRtbDVmUEFvQT09

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
  "Email": "lawyer_case_test_20260808192120@example.com",
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
      "id": "e3f0b230-f763-4642-7806-08def54e43c0",
      "email": "lawyer_case_test_20260808192120@example.com",
      "fullName": "Test Lawyer",
      "role": "Lawyer",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJlM2YwYjIzMC1mNzYzLTQ2NDItNzgwNi0wOGRlZjU0ZTQzYzAiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImUzZjBiMjMwLWY3NjMtNDY0Mi03ODA2LTA4ZGVmNTRlNDNjMCIsImVtYWlsIjoibGF3eWVyX2Nhc2VfdGVzdF8yMDI2MDgwODE5MjEyMEBleGFtcGxlLmNvbSIsIm5hbWUiOiJUZXN0IExhd3llciIsInNlY3VyaXR5X3N0YW1wIjoiQVZZSzQ1S1RNMklXWlBYWktXMzI0SVVRRFAzSUZJQ0wiLCJqdGkiOiI2Njc1ODhjOC03MDVmLTQ3MGItOTE5OC02MTVmYjkxZThmN2UiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODYyMDYwODIsImV4cCI6MTc4NjIwNjk4MiwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.7alUCBdCD1x-TgbIyL1PVS71rAQTbGqjf_0IaJQcyHY",
    "expiresIn": 900,
    "refreshToken": "bivKVvlnWjNGgPwJ5gTzuB25zP8VF0BkaxxUrTT4bpjIVmy9EOz91F6gurvi8bPeFgh2MTt+QzBBVrG/tSb6Qw==",
    "refreshTokenExpiration": "2026-08-15T16:21:22.1873077Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Setup - Complete Client Profile

**Request:** POST http://localhost:5049/api/clients/profile/complete

**Body:**
`json
{
  "NationalNumber": "29001018610436",
  "DateOfBirth": "1990-01-01",
  "PhoneNumber": "+201011111111",
  "Address": "Cairo",
  "Gender": 1
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


### Setup - Complete Lawyer Profile

**Request:** POST http://localhost:5049/api/lawyers/profile/complete

**Body:**
`json
{
  "DateOfBirth": "1985-01-01",
  "Gender": 1,
  "Bio": "Expert Lawyer",
  "PhoneNumber": "+201022222222",
  "Level": 1,
  "Address": "Cairo",
  "NationalNumber": "28501014919486",
  "Specializations": [
    {
      "Specialization": 1,
      "YearsOfExperience": 5,
      "CasesHandled": 10
    }
  ]
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


### Setup - Login Admin

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
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhMzliNjMxMi0xOWMyLTQ5ZjctZmU0Mi0wOGRlZjQ4Yzk2NjMiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImEzOWI2MzEyLTE5YzItNDlmNy1mZTQyLTA4ZGVmNDhjOTY2MyIsImVtYWlsIjoiYWRtaW5Ac21hcnRjb3VydC5jb20iLCJuYW1lIjoiU3lzdGVtIEFkbWluaXN0cmF0b3IiLCJzZWN1cml0eV9zdGFtcCI6IkI0N09OTkw1V05BVUoyMzVMUlhIVTZOUVMyUEZPWkNRIiwianRpIjoiNGU5NjBlY2MtN2YzZC00YTVhLWI1MWQtZTQ0Y2MxZDY1ZmExIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJuYmYiOjE3ODYyMDYwODIsImV4cCI6MTc4NjIwNjk4MiwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.awqPXmcBIOqaJNm-616HILKMxgL16Xhq1tVBR1E7-Mc",
    "expiresIn": 900,
    "refreshToken": "lg6Wb4nkOYWhSLSjQT5DIAXS8V/BpLK1PK8dGoPnjMLNBYtwOMhXaX4nKQcYbmAZdxq7RbH2uNxvmcUZc5cHgQ==",
    "refreshTokenExpiration": "2026-08-15T16:21:22.4043374Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Setup - Admin Approve Lawyer

**Request:** PATCH http://localhost:5049/api/admin/verifications/e3f0b230-f763-4642-7806-08def54e43c0/approve-account

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


### Setup - Admin Approve Client

**Request:** PATCH http://localhost:5049/api/admin/verifications/0a1eccf3-7e73-4b8c-7805-08def54e43c0/approve-account

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


### Setup - Re-Login Lawyer

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "lawyer_case_test_20260808192120@example.com",
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
      "id": "e3f0b230-f763-4642-7806-08def54e43c0",
      "email": "lawyer_case_test_20260808192120@example.com",
      "fullName": "Test Lawyer",
      "role": "Lawyer",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJlM2YwYjIzMC1mNzYzLTQ2NDItNzgwNi0wOGRlZjU0ZTQzYzAiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImUzZjBiMjMwLWY3NjMtNDY0Mi03ODA2LTA4ZGVmNTRlNDNjMCIsImVtYWlsIjoibGF3eWVyX2Nhc2VfdGVzdF8yMDI2MDgwODE5MjEyMEBleGFtcGxlLmNvbSIsIm5hbWUiOiJUZXN0IExhd3llciIsInNlY3VyaXR5X3N0YW1wIjoiQ0tJN1RJR1FaNlE3WVNJVVFXTEY0TUlaSjY1UkFGNEciLCJqdGkiOiI1Mzg0YTk2MC05MzhjLTRiNjQtYTIzYS0wNWQ4NjZmMDk5OTciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODYyMDYwODIsImV4cCI6MTc4NjIwNjk4MiwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.sxI9ZjTBWzOfY9g6We8KARJL51CXB28b9GCYa976Tiw",
    "expiresIn": 900,
    "refreshToken": "9DOzAmWdmk7i/SAIVAQ4sNEPx7fZgvJQs/BWhqVzNNe/WdHR+mArOfT64IPz0epC6hH/9qlV4/Jt+IYS7UqWcA==",
    "refreshTokenExpiration": "2026-08-15T16:21:22.6801349Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Setup - Re-Login Client

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "client_case_test_20260808192120@example.com",
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
      "id": "0a1eccf3-7e73-4b8c-7805-08def54e43c0",
      "email": "client_case_test_20260808192120@example.com",
      "fullName": "Test Client",
      "role": "Client",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwYTFlY2NmMy03ZTczLTRiOGMtNzgwNS0wOGRlZjU0ZTQzYzAiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjBhMWVjY2YzLTdlNzMtNGI4Yy03ODA1LTA4ZGVmNTRlNDNjMCIsImVtYWlsIjoiY2xpZW50X2Nhc2VfdGVzdF8yMDI2MDgwODE5MjEyMEBleGFtcGxlLmNvbSIsIm5hbWUiOiJUZXN0IENsaWVudCIsInNlY3VyaXR5X3N0YW1wIjoiMjQyTFlPR0pKWlFCQkJPT01GRkhEV1paSVNGQjY2T1MiLCJqdGkiOiI0MmUzYWZiZC03NWE2LTQ2ZWYtYWMyYy0yZmZmNTY1ZmIzNTQiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDbGllbnQiLCJuYmYiOjE3ODYyMDYwODIsImV4cCI6MTc4NjIwNjk4MiwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.hHx5kCnvD107QA-Dwd2DFnFrmziaxWXL9pD4hwIBD4w",
    "expiresIn": 900,
    "refreshToken": "lGnNS/vw/o68yiyaarZvJswv5VNNm8Va7mkCDi/uWnt5+vxsWdiAhHI6l+8O9MNpixgUcpb2ndnyGK2nvt0tGA==",
    "refreshTokenExpiration": "2026-08-15T16:21:22.821145Z"
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
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 84 105 116 108 101 34 58 91 34 84 104 101 32 84 105 116 108 101 32 102 105 101 108 100 32 105 115 32 114 101 113 117 105 114 101 100 46 34 44 34 67 97 115 101 32 116 105 116 108 101 32 99 97 110 39 116 32 98 101 32 101 109 112 116 121 34 93 44 34 68 101 115 99 114 105 112 116 105 111 110 34 58 91 34 84 104 101 32 68 101 115 99 114 105 112 116 105 111 110 32 102 105 101 108 100 32 105 115 32 114 101 113 117 105 114 101 100 46 34 44 34 67 97 115 101 32 100 101 115 99 114 105 112 116 105 111 110 32 99 97 110 39 116 32 98 101 32 101 109 112 116 121 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 100 52 56 99 56 101 52 56 50 51 57 99 101 97 100 99 102 99 56 99 48 57 100 56 51 50 49 98 102 98 49 101 45 99 101 52 100 53 98 49 50 55 101 98 50 52 51 100 102 45 48 48 34 125
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
    "caseId": "7b330644-2c92-4d74-a914-9cd94b34f63b",
    "failedDocuments": []
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
``n---


### Get Case By ID

**Request:** GET http://localhost:5049/api/Case/7b330644-2c92-4d74-a914-9cd94b34f63b

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "7b330644-2c92-4d74-a914-9cd94b34f63b",
    "clientId": "0a1eccf3-7e73-4b8c-7805-08def54e43c0",
    "title": "Valid Case Title",
    "description": "Detailed description of the case for testing.",
    "governorate": "Cairo",
    "city": "Maadi",
    "status": "Submitted",
    "createdAt": "2026-08-08T16:21:24.1623211",
    "documents": [
      {
        "id": "37c20403-ad92-4900-6e06-08def54e45f8",
        "fileName": "dummy_case.pdf",
        "fileUrl": "0a1eccf3-7e73-4b8c-7805-08def54e43c0/case-documents/a8670968-7d60-43ed-a3de-ec9a019c3e7f.pdf",
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

**Request:** GET http://localhost:5049/api/Case/0c946e44-859c-454c-b737-422d7ce7d2e0

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
      "id": "8ac1f7c1-febe-41f7-bf00-0240a3b35aee",
      "title": "Case for Contract",
      "status": "Matched",
      "createdAt": "2026-08-07T18:00:33.2128229",
      "documentCount": 1
    },
    {
      "id": "04b7175b-c436-4fe3-867b-02e738fe8ddb",
      "title": "Case for Contract",
      "status": "Submitted",
      "createdAt": "2026-08-07T17:30:06.4337712",
      "documentCount": 1
    },
    {
      "id": "a0463806-4274-4aaa-b71d-11c9d9de28da",
      "title": "<script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> ",
      "status": "Submitted",
      "createdAt": "2026-08-08T16:12:18.7492037",
      "documentCount": 0
    },
    {
      "id": "04707188-0e2e-47d7-9492-15d06a654d76",
      "title": "Case for Milestones",
      "status": "Matched",
      "createdAt": "2026-08-08T12:26:58.2081756",
      "documentCount": 1
    },
    {
      "id": "48b702ed-3c12-46e5-aca1-18ae3f081c12",
      "title": "Case for Milestones",
      "status": "Matched",
      "createdAt": "2026-08-08T13:09:26.3904182",
      "documentCount": 1
    },
    {
      "id": "386f13d6-18a1-4e26-b6c1-1d0df99ebcb2",
      "title": "Case for Contract",
      "status": "Matched",
      "createdAt": "2026-08-08T10:02:40.1582251",
      "documentCount": 1
    },
    {
      "id": "d25ecafe-2135-4008-8003-419bdabd864e",
      "title": "Case for Contract",
      "status": "Matched",
      "createdAt": "2026-08-08T09:19:47.8180544",
      "documentCount": 1
    },
    {
      "id": "c9324aa4-b7b6-411c-9055-5103903f0c71",
      "title": "Case for Milestones",
      "status": "Matched",
      "createdAt": "2026-08-08T12:23:36.6919111",
      "documentCount": 1
    },
    {
      "id": "622c28bb-3456-4522-bf86-5d731a144d48",
      "title": "Case for Milestones",
      "status": "Matched",
      "createdAt": "2026-08-08T14:10:33.7878274",
      "documentCount": 1
    },
    {
      "id": "e8b3564f-70b2-4292-bdec-5e99ad0d4fba",
      "title": "Case for Milestones",
      "status": "Matched",
      "createdAt": "2026-08-08T14:09:14.580902",
      "documentCount": 1
    },
    {
      "id": "23124ac5-c36e-4ea3-89b5-644d5afe284e",
      "title": "Valid Case Title",
      "status": "Matched",
      "createdAt": "2026-08-07T17:53:29.1139717",
      "documentCount": 1
    },
    {
      "id": "87393346-9233-43af-9631-797904b3905e",
      "title": "Case for Contract",
      "status": "Matched",
      "createdAt": "2026-08-07T17:32:49.0003707",
      "documentCount": 1
    },
    {
      "id": "dc6c2569-1cb3-4c04-9c7e-82312702a853",
      "title": "Updated Case Title",
      "status": "Matched",
      "createdAt": "2026-08-08T15:30:56.4541072",
      "documentCount": 1
    },
    {
      "id": "b1cccb67-35bf-4d08-a27a-83621f688a37",
      "title": "Case for Milestones",
      "status": "Matched",
      "createdAt": "2026-08-08T13:49:36.6125865",
      "documentCount": 1
    },
    {
      "id": "8a0e092e-fb58-4e21-b758-8acb2efa6dec",
      "title": "Case for Milestones",
      "status": "Matched",
      "createdAt": "2026-08-08T13:45:41.5509136",
      "documentCount": 1
    },
    {
      "id": "9f976409-35e6-4378-8cd8-91232d9eb650",
      "title": "Case for Milestones",
      "status": "Matched",
      "createdAt": "2026-08-08T13:16:45.7791025",
      "documentCount": 1
    },
    {
      "id": "1ac07014-ac41-4ce9-838e-953d95412440",
      "title": "Case for Milestones",
      "status": "Matched",
      "createdAt": "2026-08-08T11:55:55.6813735",
      "documentCount": 1
    },
    {
      "id": "7b330644-2c92-4d74-a914-9cd94b34f63b",
      "title": "Valid Case Title",
      "status": "Submitted",
      "createdAt": "2026-08-08T16:21:24.1623211",
      "documentCount": 1
    },
    {
      "id": "1eae106d-cb23-4a2d-a483-9ed45e75df49",
      "title": "Case for Milestones",
      "status": "Matched",
      "createdAt": "2026-08-08T12:58:49.6171152",
      "documentCount": 1
    },
    {
      "id": "47e80cdc-0b1b-4ace-a33c-9f133c3a416e",
      "title": "Case for Milestones",
      "status": "Matched",
      "createdAt": "2026-08-08T12:00:46.6079885",
      "documentCount": 1
    },
    {
      "id": "b43edf8d-75ae-4499-9e8e-a21b72b52ed0",
      "title": "Case for Contract",
      "status": "Matched",
      "createdAt": "2026-08-08T09:27:32.7449391",
      "documentCount": 1
    },
    {
      "id": "f3093962-b95f-4aa2-a21b-b16c68f64eee",
      "title": "Case for Milestones",
      "status": "Matched",
      "createdAt": "2026-08-08T11:51:52.7452239",
      "documentCount": 1
    },
    {
      "id": "8b0b5509-f314-4040-93a2-b691523bf6cb",
      "title": "Case for Contract",
      "status": "Matched",
      "createdAt": "2026-08-08T09:36:47.8799435",
      "documentCount": 1
    },
    {
      "id": "f67a065c-3651-42f9-a127-b6edaef1669d",
      "title": "Updated Case Title",
      "status": "Matched",
      "createdAt": "2026-08-08T15:29:18.7574899",
      "documentCount": 1
    },
    {
      "id": "a138749c-dee7-4811-a0dc-b73865337772",
      "title": "Case for Contract",
      "status": "Matched",
      "createdAt": "2026-08-08T09:41:38.0056125",
      "documentCount": 1
    },
    {
      "id": "094f2653-06ca-4d69-8ed0-bac2f588564d",
      "title": "Updated Case Title",
      "status": "Matched",
      "createdAt": "2026-08-08T16:12:17.0635371",
      "documentCount": 1
    },
    {
      "id": "c1c60bc2-59dd-4a1d-88c7-d2f1ab6bc2e7",
      "title": "Case for Contract",
      "status": "Matched",
      "createdAt": "2026-08-08T09:31:10.7091392",
      "documentCount": 1
    },
    {
      "id": "259f619b-f2a6-49eb-8420-d37afde28b87",
      "title": "Case for Milestones",
      "status": "Matched",
      "createdAt": "2026-08-08T13:46:59.1361218",
      "documentCount": 1
    },
    {
      "id": "a050d102-e8f1-4085-893e-d6f2bb347e1b",
      "title": "Case for Contract",
      "status": "Matched",
      "createdAt": "2026-08-08T09:48:22.1611546",
      "documentCount": 1
    },
    {
      "id": "248ba32e-657a-47cf-9e05-d988f889666c",
      "title": "Case for Milestones",
      "status": "Matched",
      "createdAt": "2026-08-08T13:54:42.2121401",
      "documentCount": 1
    },
    {
      "id": "2af4f432-92f9-49bb-b59f-db3641805f04",
      "title": "Case for Contract",
      "status": "Matched",
      "createdAt": "2026-08-08T09:40:18.1668738",
      "documentCount": 1
    },
    {
      "id": "05bfe442-3b90-44f0-911f-dd830dd0cd8e",
      "title": "<script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> ",
      "status": "Submitted",
      "createdAt": "2026-08-08T15:30:57.2035105",
      "documentCount": 0
    },
    {
      "id": "56574048-34ae-467c-a567-df4259d82fc0",
      "title": "Case for Milestones",
      "status": "Matched",
      "createdAt": "2026-08-08T13:22:11.452286",
      "documentCount": 1
    },
    {
      "id": "ffa792ac-508f-474a-ae1f-dfb03b53203e",
      "title": "Case for Milestones",
      "status": "Matched",
      "createdAt": "2026-08-08T13:26:48.1624825",
      "documentCount": 1
    },
    {
      "id": "5a272f0e-9978-4e6e-81b1-e62e5b32b847",
      "title": "<script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> <script>alert('XSS')</script> ",
      "status": "Submitted",
      "createdAt": "2026-08-08T15:29:19.9076168",
      "documentCount": 0
    },
    {
      "id": "603b1e88-4c0d-4b3a-b5a4-f2eaa97f9c63",
      "title": "Case for Milestones",
      "status": "Matched",
      "createdAt": "2026-08-08T13:48:32.0341946",
      "documentCount": 1
    },
    {
      "id": "187fb477-8401-41e9-9ccc-fcf034641bb7",
      "title": "Case for Contract",
      "status": "Matched",
      "createdAt": "2026-08-07T17:38:42.7649349",
      "documentCount": 1
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Update Case (Valid Success)

**Request:** PUT http://localhost:5049/api/Case/7b330644-2c92-4d74-a914-9cd94b34f63b

**Body:**
(multipart/form-data)

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "caseId": "7b330644-2c92-4d74-a914-9cd94b34f63b",
    "failedDocuments": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


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
    "caseId": "ee1c0975-b38b-4b42-9085-569fa048eab6",
    "failedDocuments": []
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
``n---


### Delete Case (Success)

**Request:** DELETE http://localhost:5049/api/Case/ee1c0975-b38b-4b42-9085-569fa048eab6

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

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "caseId": "3eeeb49f-8072-499a-bbf8-40ac6b2c2828",
    "failedDocuments": []
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
``n---


### Review Case (AI Request)

**Request:** POST http://localhost:5049/api/cases/7b330644-2c92-4d74-a914-9cd94b34f63b/review

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
    "id": "741d27cf-fd1e-4989-85d4-48c899bfb139",
    "caseId": "7b330644-2c92-4d74-a914-9cd94b34f63b",
    "isLatest": true,
    "createdAt": "2026-08-08T16:21:28.0208478Z",
    "reviewPoints": [
      {
        "id": "b3bcf5cd-e682-4c24-9922-faf5e0fe3461",
        "description": "تتمثل نقطة القوة الأساسية في صياغة الموضوع بوضوح حول 'Updated Case Title'، وتوافر السند المبدئي الذي يرجح كفة الموكل في إثبات أصل الالتزام وتفوقه إثباتياً على الخصم.",
        "type": "Strength"
      },
      {
        "id": "b417202f-2466-455d-9477-22f4ac5268f6",
        "description": "ميزة الخصم تتمثل في غياب التوثيق الرسمي للتنبيهات أو الإخطارات المتبادلة بين الأطراف، مما يتيح له إنكار الاستلام أو الدفع بالتراخي في المطالبة.",
        "type": "Weakness"
      },
      {
        "id": "0fcd0c1d-3a85-42b0-943d-acc04e1b0709",
        "description": "يحتاج الملف إلى استيفاء النقاط والمعلومات التالية لضمان صياغة صحيفة الدعوى بشكل مكتمل: حصر وتفصيص المبالغ المالية المطلوبة والتعويضات الدقيقة عن الضرر المادي والمعنوي، وإدراج التواريخ الرسمية الدقيقة لبدء النزاع وتاريخ الإخلال بالتعهدات.",
        "type": "MissingCaseInfo"
      },
      {
        "id": "a0fc2520-d45e-4776-9627-da2fc4539f77",
        "description": "المستندات المحددة المطلوبة لإكمال الملف: أصل العقد/الاتفاق المبرم، صورة بطاقة الرقم القومي سارية لكل أطراف الدعوى، إيصالات التحويل أو السداد المالي، وأي إنذارات رسمية على يد محضر.",
        "type": "MissingCaseDoc"
      },
      {
        "id": "9de61cb5-17b5-4478-8242-b11e279cfaf5",
        "description": "قم بإعادة هيكلة وصف القضية في صورة جدول زمني متسلسل، يبدأ من تاريخ التعهد الأول، مروراً بتاريخ الإخلال، وصولاً إلى حجم الأضرار المترتبة حالياً.",
        "type": "Suggestion"
      },
      {
        "id": "75a6f2bf-c036-40a4-876b-34aaaa2e8e29",
        "description": "قم بتفقيط وقسمة كافة المطالبات المالية إلى بنود مستقلة (أصل الدين، الفوائد أو التعويض عن المماطلة، والرسوم) وتوثيق كل بند بسند كتابي مستقل.",
        "type": "Suggestion"
      },
      {
        "id": "ab4a906c-2ec1-4e42-a25a-25d6894f6b3c",
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

**Request:** GET http://localhost:5049/api/cases/7b330644-2c92-4d74-a914-9cd94b34f63b/reviews/latest

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "741d27cf-fd1e-4989-85d4-48c899bfb139",
    "caseId": "7b330644-2c92-4d74-a914-9cd94b34f63b",
    "isLatest": true,
    "createdAt": "2026-08-08T16:21:28.0208478",
    "reviewPoints": [
      {
        "id": "b417202f-2466-455d-9477-22f4ac5268f6",
        "description": "ميزة الخصم تتمثل في غياب التوثيق الرسمي للتنبيهات أو الإخطارات المتبادلة بين الأطراف، مما يتيح له إنكار الاستلام أو الدفع بالتراخي في المطالبة.",
        "type": "Weakness"
      },
      {
        "id": "ab4a906c-2ec1-4e42-a25a-25d6894f6b3c",
        "description": "قم بتنظيم وثائق الملف في مجلد مرتب حسب التاريخ، وتأكد من مسح الأوراق ضوئياً بدقة عالية لضمان سهولة الإسناد والفحص القضائي.",
        "type": "Suggestion"
      },
      {
        "id": "75a6f2bf-c036-40a4-876b-34aaaa2e8e29",
        "description": "قم بتفقيط وقسمة كافة المطالبات المالية إلى بنود مستقلة (أصل الدين، الفوائد أو التعويض عن المماطلة، والرسوم) وتوثيق كل بند بسند كتابي مستقل.",
        "type": "Suggestion"
      },
      {
        "id": "0fcd0c1d-3a85-42b0-943d-acc04e1b0709",
        "description": "يحتاج الملف إلى استيفاء النقاط والمعلومات التالية لضمان صياغة صحيفة الدعوى بشكل مكتمل: حصر وتفصيص المبالغ المالية المطلوبة والتعويضات الدقيقة عن الضرر المادي والمعنوي، وإدراج التواريخ الرسمية الدقيقة لبدء النزاع وتاريخ الإخلال بالتعهدات.",
        "type": "MissingCaseInfo"
      },
      {
        "id": "9de61cb5-17b5-4478-8242-b11e279cfaf5",
        "description": "قم بإعادة هيكلة وصف القضية في صورة جدول زمني متسلسل، يبدأ من تاريخ التعهد الأول، مروراً بتاريخ الإخلال، وصولاً إلى حجم الأضرار المترتبة حالياً.",
        "type": "Suggestion"
      },
      {
        "id": "a0fc2520-d45e-4776-9627-da2fc4539f77",
        "description": "المستندات المحددة المطلوبة لإكمال الملف: أصل العقد/الاتفاق المبرم، صورة بطاقة الرقم القومي سارية لكل أطراف الدعوى، إيصالات التحويل أو السداد المالي، وأي إنذارات رسمية على يد محضر.",
        "type": "MissingCaseDoc"
      },
      {
        "id": "b3bcf5cd-e682-4c24-9922-faf5e0fe3461",
        "description": "تتمثل نقطة القوة الأساسية في صياغة الموضوع بوضوح حول 'Updated Case Title'، وتوافر السند المبدئي الذي يرجح كفة الموكل في إثبات أصل الالتزام وتفوقه إثباتياً على الخصم.",
        "type": "Strength"
      }
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Finalize Case (Transition to Matched)

**Request:** POST http://localhost:5049/api/Case/7b330644-2c92-4d74-a914-9cd94b34f63b/finalize

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
    "caseId": "7b330644-2c92-4d74-a914-9cd94b34f63b",
    "totalEligibleLawyers": 0,
    "recommendations": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Create Proposal (Client to Lawyer)

**Request:** POST http://localhost:5049/api/proposals

**Body:**
`json
{
  "LawyerUserId": "e3f0b230-f763-4642-7806-08def54e43c0",
  "Message": "I would like to hire you for this case.",
  "LegalCaseId": "7b330644-2c92-4d74-a914-9cd94b34f63b"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "5d4a0cf9-8acc-40e3-a5c5-d747e5bbd376",
    "legalCaseId": "7b330644-2c92-4d74-a914-9cd94b34f63b",
    "caseTitle": "Updated Case Title",
    "clientUserId": "0a1eccf3-7e73-4b8c-7805-08def54e43c0",
    "clientName": "Test Client",
    "lawyerUserId": "e3f0b230-f763-4642-7806-08def54e43c0",
    "lawyerName": "Test Lawyer",
    "message": "I would like to hire you for this case.",
    "status": "Pending",
    "decisionReason": null,
    "createdAt": "2026-08-08T16:21:32.1332488",
    "respondedAt": null,
    "updatedAt": "2026-08-08T16:21:32.1332488",
    "conversationId": null
  },
  "message": null,
  "errors": null,
  "statusCode": 201
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
    "items": [
      {
        "id": "5d4a0cf9-8acc-40e3-a5c5-d747e5bbd376",
        "legalCaseId": "7b330644-2c92-4d74-a914-9cd94b34f63b",
        "caseTitle": "Updated Case Title",
        "clientUserId": "0a1eccf3-7e73-4b8c-7805-08def54e43c0",
        "clientName": "Test Client",
        "lawyerUserId": "e3f0b230-f763-4642-7806-08def54e43c0",
        "lawyerName": "Test Lawyer",
        "status": "Pending",
        "createdAt": "2026-08-08T16:21:32.1332488",
        "respondedAt": null,
        "conversationId": null
      }
    ],
    "page": 1,
    "pageSize": 10,
    "totalCount": 1,
    "hasNextPage": false
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Lawyer Accepts Proposal

**Request:** POST http://localhost:5049/api/proposals/5d4a0cf9-8acc-40e3-a5c5-d747e5bbd376/accept

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
    "id": "5d4a0cf9-8acc-40e3-a5c5-d747e5bbd376",
    "legalCaseId": "7b330644-2c92-4d74-a914-9cd94b34f63b",
    "caseTitle": "Updated Case Title",
    "clientUserId": "0a1eccf3-7e73-4b8c-7805-08def54e43c0",
    "clientName": "Test Client",
    "lawyerUserId": "e3f0b230-f763-4642-7806-08def54e43c0",
    "lawyerName": "Test Lawyer",
    "message": "I would like to hire you for this case.",
    "status": "Accepted",
    "decisionReason": null,
    "createdAt": "2026-08-08T16:21:32.1332488",
    "respondedAt": "2026-08-08T16:21:32.2986356",
    "updatedAt": "2026-08-08T16:21:32.2986356",
    "conversationId": "0998db50-73ad-405f-8e71-295744417286"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---



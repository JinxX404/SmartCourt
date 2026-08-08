# Milestones Slice HTTP Tests End-to-End Workflow Report

Generated at 2026-08-08 20:16:49


### Setup - Register Client

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
  "Email": "client_ms_20260808201649@example.com",
  "ConfirmPassword": "Password123!",
  "FullName": "Test Client",
  "Password": "Password123!"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "20131d70-b70d-4317-7825-08def54e43c0",
    "email": "client_ms_20260808201649@example.com",
    "fullName": "Test Client",
    "role": "Client"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


Found confirmation URL for client_ms_20260808201649@example.com: http://localhost:5173/verify-email?userId=20131d70-b70d-4317-7825-08def54e43c0&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI4aGVPZmVIYmZyVWExcmNZaDRLMkpxY0VrZ0ZzUG9DTTh3K1A4RExLc2JFS2NyRWdxVzNQNjlqSDZaUy9xdWcvd0lIYTRaMUZTUHJQbGp6cnluZjFueHErT296MFJHMDRWbUZJdmxpRit4cUIxV3lkSDdPVUtnNmxGR2RIY09UK3lpNjUyOHJ3M2xNMW0xK0QycXpsaGpyamplSTNRNy8vTGNXZEo3UE9wd25QWWhPS0VLVENuY1RpNG9NQ2lkTkU2TlY3REw5bVFjbGR0cjViTW5BOFFCVTZzcFZ1WjM0WlVhTy9NL1hvYWFrZz09

### Confirm Email for client_ms_20260808201649@example.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=20131d70-b70d-4317-7825-08def54e43c0&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI4aGVPZmVIYmZyVWExcmNZaDRLMkpxY0VrZ0ZzUG9DTTh3K1A4RExLc2JFS2NyRWdxVzNQNjlqSDZaUy9xdWcvd0lIYTRaMUZTUHJQbGp6cnluZjFueHErT296MFJHMDRWbUZJdmxpRit4cUIxV3lkSDdPVUtnNmxGR2RIY09UK3lpNjUyOHJ3M2xNMW0xK0QycXpsaGpyamplSTNRNy8vTGNXZEo3UE9wd25QWWhPS0VLVENuY1RpNG9NQ2lkTkU2TlY3REw5bVFjbGR0cjViTW5BOFFCVTZzcFZ1WjM0WlVhTy9NL1hvYWFrZz09

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


### Setup - Login Client

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Password": "Password123!",
  "Email": "client_ms_20260808201649@example.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "20131d70-b70d-4317-7825-08def54e43c0",
      "email": "client_ms_20260808201649@example.com",
      "fullName": "Test Client",
      "role": "Client",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyMDEzMWQ3MC1iNzBkLTQzMTctNzgyNS0wOGRlZjU0ZTQzYzAiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjIwMTMxZDcwLWI3MGQtNDMxNy03ODI1LTA4ZGVmNTRlNDNjMCIsImVtYWlsIjoiY2xpZW50X21zXzIwMjYwODA4MjAxNjQ5QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgQ2xpZW50Iiwic2VjdXJpdHlfc3RhbXAiOiJIQjdaMkRWVEZSSU9SQ1VCTkRLNElYNDU1TjJOQ09VWSIsImp0aSI6Ijg0YTljOWUyLWJmYWUtNDM2YS04Mjc1LTQ5MmExN2Q0NWZiMyIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNsaWVudCIsIm5iZiI6MTc4NjIwOTQxMCwiZXhwIjoxNzg2MjEwMzEwLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.LpJNm5BRwm7GK9lGBfUw9HpC6em4V_DJnSjr9gqAx-I",
    "expiresIn": 900,
    "refreshToken": "wzb9xsrNfUqvixPoMEy09P+gO3/aqSPpuh89woDu0BN026CpgFGOAWjckotNrX+SQuiNgtCjonRGHmmuzg8EQA==",
    "refreshTokenExpiration": "2026-08-15T17:16:50.3325644Z"
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
  "NationalNumber": "29001013519173",
  "Address": "Cairo",
  "Gender": 1,
  "PhoneNumber": "+201011111111",
  "DateOfBirth": "1990-01-01"
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


### Setup - Re-Login Client

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Password": "Password123!",
  "Email": "client_ms_20260808201649@example.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "20131d70-b70d-4317-7825-08def54e43c0",
      "email": "client_ms_20260808201649@example.com",
      "fullName": "Test Client",
      "role": "Client",
      "status": "PendingReview",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyMDEzMWQ3MC1iNzBkLTQzMTctNzgyNS0wOGRlZjU0ZTQzYzAiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjIwMTMxZDcwLWI3MGQtNDMxNy03ODI1LTA4ZGVmNTRlNDNjMCIsImVtYWlsIjoiY2xpZW50X21zXzIwMjYwODA4MjAxNjQ5QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgQ2xpZW50Iiwic2VjdXJpdHlfc3RhbXAiOiJONUZKR0lRUEw2S0wzNElGS1JBV0NTRlJQVkRRVlpISSIsImp0aSI6ImZjNTQ0Njg2LTI2YzktNDE1Yi04OTA2LWZhNmNmNWFjMWZiMCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNsaWVudCIsIm5iZiI6MTc4NjIwOTQxMCwiZXhwIjoxNzg2MjEwMzEwLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.LszHtbMZFIuS1eaBwCO4I5NWZTSg_gx5-6qIM4NQ8bo",
    "expiresIn": 900,
    "refreshToken": "XiqXEbxTSeILZvLTWlJ51vcoE04hKosMyRaqe5HK203jcSvklFWoE2L5m4gtnTXPJK/m6dmXPFUjUkL+JVb+NQ==",
    "refreshTokenExpiration": "2026-08-15T17:16:50.6211735Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Setup - Register Lawyer

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
`json
{
  "Email": "lawyer_ms_20260808201650@example.com",
  "ConfirmPassword": "Password123!",
  "FullName": "Test Lawyer",
  "Password": "Password123!"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "8cae8669-35b7-4c0e-7826-08def54e43c0",
    "email": "lawyer_ms_20260808201650@example.com",
    "fullName": "Test Lawyer",
    "role": "Lawyer"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


Found confirmation URL for lawyer_ms_20260808201650@example.com: http://localhost:5173/verify-email?userId=8cae8669-35b7-4c0e-7826-08def54e43c0&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5L0xiRlMzUWk2c1ltNThaTlltREJKa3d5Y05JZnlBVVpaMi9BeGFUYUpvSG1NaE40R0NuQ3pzS2tLZmdacE8vYTh0Z3QzdjVTOC9SaTZ1bWhSblhNRjVVOUFETkU1ZklxWVBMNTh5OUszWkg3bEFqekdndlBRcUhGQ1NkNmFZcC9Qbmx0M0UrMm5xSzJBa25FSkxIL3Iwd3V2T2Q4QzdtbXc0Z1BGei9aZmJLbTRBL09lelZab2Y2aEFscXByL3hiczdnNW1NSjRIZGhMaDlLQS9SS3NSL2ZyRG1WM1AwbW1wNWwrSEpzUlFrZz09

### Confirm Email for lawyer_ms_20260808201650@example.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=8cae8669-35b7-4c0e-7826-08def54e43c0&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5L0xiRlMzUWk2c1ltNThaTlltREJKa3d5Y05JZnlBVVpaMi9BeGFUYUpvSG1NaE40R0NuQ3pzS2tLZmdacE8vYTh0Z3QzdjVTOC9SaTZ1bWhSblhNRjVVOUFETkU1ZklxWVBMNTh5OUszWkg3bEFqekdndlBRcUhGQ1NkNmFZcC9Qbmx0M0UrMm5xSzJBa25FSkxIL3Iwd3V2T2Q4QzdtbXc0Z1BGei9aZmJLbTRBL09lelZab2Y2aEFscXByL3hiczdnNW1NSjRIZGhMaDlLQS9SS3NSL2ZyRG1WM1AwbW1wNWwrSEpzUlFrZz09

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


### Setup - Login Lawyer

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Password": "Password123!",
  "Email": "lawyer_ms_20260808201650@example.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "8cae8669-35b7-4c0e-7826-08def54e43c0",
      "email": "lawyer_ms_20260808201650@example.com",
      "fullName": "Test Lawyer",
      "role": "Lawyer",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4Y2FlODY2OS0zNWI3LTRjMGUtNzgyNi0wOGRlZjU0ZTQzYzAiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjhjYWU4NjY5LTM1YjctNGMwZS03ODI2LTA4ZGVmNTRlNDNjMCIsImVtYWlsIjoibGF3eWVyX21zXzIwMjYwODA4MjAxNjUwQGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgTGF3eWVyIiwic2VjdXJpdHlfc3RhbXAiOiIyV0FIRUJSNUNVU0FKNElTUUtIQkJPU0pRWk1IWDRXSCIsImp0aSI6IjhkYWFjM2U2LTgzZGEtNGU2ZC04N2M2LWMyZDZmN2IyZTQ0ZiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NjIwOTQxMiwiZXhwIjoxNzg2MjEwMzEyLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.YLEzj0pZekD8LRCoW15fqXMr-NbKTklh-RHN91NMMM8",
    "expiresIn": 900,
    "refreshToken": "vkgJTDuBwHkSiCQboOYcCKVMYkYpslXE11kx3bbHyDLsArVy6LY/myVGrnpXbi74Shn7xmKPgr7sL1SXsgJSBA==",
    "refreshTokenExpiration": "2026-08-15T17:16:52.1618606Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Setup - Complete Lawyer Profile

**Request:** POST http://localhost:5049/api/lawyers/profile/complete

**Body:**
`json
{
  "Level": 1,
  "PhoneNumber": "+201022222222",
  "Address": "Cairo",
  "Specializations": [
    {
      "Specialization": 1,
      "YearsOfExperience": 5,
      "CasesHandled": 10
    }
  ],
  "Gender": 1,
  "DateOfBirth": "1985-01-01",
  "Bio": "Expert Lawyer",
  "NationalNumber": "28501017861851"
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
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhMzliNjMxMi0xOWMyLTQ5ZjctZmU0Mi0wOGRlZjQ4Yzk2NjMiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImEzOWI2MzEyLTE5YzItNDlmNy1mZTQyLTA4ZGVmNDhjOTY2MyIsImVtYWlsIjoiYWRtaW5Ac21hcnRjb3VydC5jb20iLCJuYW1lIjoiU3lzdGVtIEFkbWluaXN0cmF0b3IiLCJzZWN1cml0eV9zdGFtcCI6IkI0N09OTkw1V05BVUoyMzVMUlhIVTZOUVMyUEZPWkNRIiwianRpIjoiZDQ0NjU3ZGUtMTU3Ni00NzVjLWI1YmItYWZmNWMxNDk4MjRhIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJuYmYiOjE3ODYyMDk0MTIsImV4cCI6MTc4NjIxMDMxMiwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.EP2klpYu5_OXLFQgAQQzZBjUDf2EJ5paZ4m1l_TRR-g",
    "expiresIn": 900,
    "refreshToken": "ypWxDlteldhrTOOrAwORCuz1GpAAOax7TX5L7ZUFDubQ/w6HFvSufvpGHTBYcH0ouIOkTp0Mn1pT/u8ryr4Exw==",
    "refreshTokenExpiration": "2026-08-15T17:16:52.4241498Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Setup - Admin Approve Lawyer

**Request:** PATCH http://localhost:5049/api/admin/verifications/8cae8669-35b7-4c0e-7826-08def54e43c0/approve-account

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

**Request:** PATCH http://localhost:5049/api/admin/verifications/20131d70-b70d-4317-7825-08def54e43c0/approve-account

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
  "Password": "Password123!",
  "Email": "lawyer_ms_20260808201650@example.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "8cae8669-35b7-4c0e-7826-08def54e43c0",
      "email": "lawyer_ms_20260808201650@example.com",
      "fullName": "Test Lawyer",
      "role": "Lawyer",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4Y2FlODY2OS0zNWI3LTRjMGUtNzgyNi0wOGRlZjU0ZTQzYzAiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjhjYWU4NjY5LTM1YjctNGMwZS03ODI2LTA4ZGVmNTRlNDNjMCIsImVtYWlsIjoibGF3eWVyX21zXzIwMjYwODA4MjAxNjUwQGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgTGF3eWVyIiwic2VjdXJpdHlfc3RhbXAiOiJMUFpDTVBCUzRDNUM1QkhFTE1LU0lLWkNQQ0c2SFE1TyIsImp0aSI6IjQ4NzFkYThjLTAxN2UtNDJhYS1iMTg3LTA2ODA4ZmMxODJlNSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NjIwOTQxMiwiZXhwIjoxNzg2MjEwMzEyLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.Iry6Mulyuly4aJWMV332X9fcYVL2PuemdXM5srItd94",
    "expiresIn": 900,
    "refreshToken": "25dRydInYykH4BzeANrevztxq8Yxsc7A/nyIqQ0MuAR3qNe/oyVSGkMX9Wpmfxbx4CL4byXni4qhEw5GqXhNrQ==",
    "refreshTokenExpiration": "2026-08-15T17:16:52.6510733Z"
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
  "Password": "Password123!",
  "Email": "client_ms_20260808201649@example.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "20131d70-b70d-4317-7825-08def54e43c0",
      "email": "client_ms_20260808201649@example.com",
      "fullName": "Test Client",
      "role": "Client",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyMDEzMWQ3MC1iNzBkLTQzMTctNzgyNS0wOGRlZjU0ZTQzYzAiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjIwMTMxZDcwLWI3MGQtNDMxNy03ODI1LTA4ZGVmNTRlNDNjMCIsImVtYWlsIjoiY2xpZW50X21zXzIwMjYwODA4MjAxNjQ5QGV4YW1wbGUuY29tIiwibmFtZSI6IlRlc3QgQ2xpZW50Iiwic2VjdXJpdHlfc3RhbXAiOiJONUZKR0lRUEw2S0wzNElGS1JBV0NTRlJQVkRRVlpISSIsImp0aSI6IjhhYzE4MjUyLTEzYzUtNGVjMS05ZTU5LTVhMjM3NzI0OTRiMCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNsaWVudCIsIm5iZiI6MTc4NjIwOTQxMiwiZXhwIjoxNzg2MjEwMzEyLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.tgE_dst-DjN5gaFhamgdQqVrfIcjnbI3oi7dvONowLQ",
    "expiresIn": 900,
    "refreshToken": "4gCuUbYs/mmAIa3Sgiipt1jGFRjJxDmI/CKBE4E3t6Ow7uo49nPYG0YIznsrkZgtPCKvHplvlyRejKW/RdaAwQ==",
    "refreshTokenExpiration": "2026-08-15T17:16:52.8343497Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Setup - Register Attacker

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
  "Email": "attacker_ms_20260808201652@example.com",
  "ConfirmPassword": "Password123!",
  "FullName": "Test Attacker",
  "Password": "Password123!"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "68297071-42bb-4560-7827-08def54e43c0",
    "email": "attacker_ms_20260808201652@example.com",
    "fullName": "Test Attacker",
    "role": "Client"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


Found confirmation URL for attacker_ms_20260808201652@example.com: http://localhost:5173/verify-email?userId=68297071-42bb-4560-7827-08def54e43c0&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5aGJFRDBSdWV3ZXh1WXZ6ZW02MjJtTFRNc2tNZ0IybGZSNU1paldBM0xsK1N3eFAvTFExUnZHUzI0WFRrd1kxakRNR2lhNVNQdDNQRDFjNm1ZU0lBZGo4QU5EdGh6a3YvdG50dWgxTmdqSGlnck9RVVpiNDVCR1pYRXh3aW9EQWRlYWdxYng5OUwvSTJWRHdFMyt3Uy9XOVg5NWZVa08xQ1NEeXhZRnlLNDFmZ2RsSExSY2U2ems1R1JyQk5NdlJRUTFhNnV3clM2ZmoxNmovUFZPN1Q3WXdNYXF0M1l2b3dIRmR2WnJlanpVUT09

### Confirm Email for attacker_ms_20260808201652@example.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=68297071-42bb-4560-7827-08def54e43c0&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5aGJFRDBSdWV3ZXh1WXZ6ZW02MjJtTFRNc2tNZ0IybGZSNU1paldBM0xsK1N3eFAvTFExUnZHUzI0WFRrd1kxakRNR2lhNVNQdDNQRDFjNm1ZU0lBZGo4QU5EdGh6a3YvdG50dWgxTmdqSGlnck9RVVpiNDVCR1pYRXh3aW9EQWRlYWdxYng5OUwvSTJWRHdFMyt3Uy9XOVg5NWZVa08xQ1NEeXhZRnlLNDFmZ2RsSExSY2U2ems1R1JyQk5NdlJRUTFhNnV3clM2ZmoxNmovUFZPN1Q3WXdNYXF0M1l2b3dIRmR2WnJlanpVUT09

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


### Setup - Login Attacker

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Password": "Password123!",
  "Email": "attacker_ms_20260808201652@example.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "68297071-42bb-4560-7827-08def54e43c0",
      "email": "attacker_ms_20260808201652@example.com",
      "fullName": "Test Attacker",
      "role": "Client",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2ODI5NzA3MS00MmJiLTQ1NjAtNzgyNy0wOGRlZjU0ZTQzYzAiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjY4Mjk3MDcxLTQyYmItNDU2MC03ODI3LTA4ZGVmNTRlNDNjMCIsImVtYWlsIjoiYXR0YWNrZXJfbXNfMjAyNjA4MDgyMDE2NTJAZXhhbXBsZS5jb20iLCJuYW1lIjoiVGVzdCBBdHRhY2tlciIsInNlY3VyaXR5X3N0YW1wIjoiSlUyREtRVUlXREtSMlE1QUNQREY3NkhaTFlVV003SlYiLCJqdGkiOiIxN2MwMTUwYS0zMjg4LTRhMmUtYWY2OS02MTJkMmEwNjZmYWUiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDbGllbnQiLCJuYmYiOjE3ODYyMDk0MTQsImV4cCI6MTc4NjIxMDMxNCwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.1st6Li6fh2wycIijCPB1l1fMZXAF1XI9K8xUcBhB3aM",
    "expiresIn": 900,
    "refreshToken": "VFkTdkMvJMAfNfFlk6KWtstdbNvr7lPg1WnRlfzoXMkk7Zdb6gjZfasvDEnS9UwmdxH3qKdvVD8qxuQChuL6ZA==",
    "refreshTokenExpiration": "2026-08-15T17:16:54.5861347Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Setup - Create Case

**Request:** POST http://localhost:5049/api/Case

**Body:**
(multipart/form-data)

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "caseId": "fc3b727e-d85d-40a0-a013-a3827e8ff2ca",
    "failedDocuments": []
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
``n---


### Setup - Review Case (AI Request)

**Request:** POST http://localhost:5049/api/cases/fc3b727e-d85d-40a0-a013-a3827e8ff2ca/review

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
    "id": "0d302142-6fd5-4d0f-b6d4-0189d3eff01f",
    "caseId": "fc3b727e-d85d-40a0-a013-a3827e8ff2ca",
    "isLatest": true,
    "createdAt": "2026-08-08T17:17:30.2938145Z",
    "reviewPoints": [
      {
        "id": "8a5c71c8-8331-4949-985f-4b5e7ab76311",
        "description": "تتمثل نقطة القوة الأساسية في صياغة الموضوع بوضوح حول 'Case for Milestones'، وتوافر السند المبدئي الذي يرجح كفة الموكل في إثبات أصل الالتزام وتفوقه إثباتياً على الخصم.",
        "type": "Strength"
      },
      {
        "id": "b96423aa-abae-4c78-ba6b-06bf562c3086",
        "description": "ميزة الخصم تتمثل في غياب التوثيق الرسمي للتنبيهات أو الإخطارات المتبادلة بين الأطراف، مما يتيح له إنكار الاستلام أو الدفع بالتراخي في المطالبة.",
        "type": "Weakness"
      },
      {
        "id": "46cdf6dd-55c0-4f2f-99fe-68bc906784c1",
        "description": "يحتاج الملف إلى استيفاء النقاط والمعلومات التالية لضمان صياغة صحيفة الدعوى بشكل مكتمل: حصر وتفصيص المبالغ المالية المطلوبة والتعويضات الدقيقة عن الضرر المادي والمعنوي، وإدراج التواريخ الرسمية الدقيقة لبدء النزاع وتاريخ الإخلال بالتعهدات.",
        "type": "MissingCaseInfo"
      },
      {
        "id": "fc575ac1-2e53-4b2d-a9cf-6bb17e7494c9",
        "description": "المستندات المحددة المطلوبة لإكمال الملف: أصل العقد/الاتفاق المبرم، صورة بطاقة الرقم القومي سارية لكل أطراف الدعوى، إيصالات التحويل أو السداد المالي، وأي إنذارات رسمية على يد محضر.",
        "type": "MissingCaseDoc"
      },
      {
        "id": "5a2a9f4f-f3bc-4681-9514-c0c5315c93f3",
        "description": "قم بإعادة هيكلة وصف القضية في صورة جدول زمني متسلسل، يبدأ من تاريخ التعهد الأول، مروراً بتاريخ الإخلال، وصولاً إلى حجم الأضرار المترتبة حالياً.",
        "type": "Suggestion"
      },
      {
        "id": "c0d417f4-2e44-4c7e-ab11-2053eee63450",
        "description": "قم بتفقيط وقسمة كافة المطالبات المالية إلى بنود مستقلة (أصل الدين، الفوائد أو التعويض عن المماطلة، والرسوم) وتوثيق كل بند بسند كتابي مستقل.",
        "type": "Suggestion"
      },
      {
        "id": "2e206e48-0aa4-4c02-a2bb-cf6a683cf55b",
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


### Setup - Get Latest Review

**Request:** GET http://localhost:5049/api/cases/fc3b727e-d85d-40a0-a013-a3827e8ff2ca/reviews/latest

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "0d302142-6fd5-4d0f-b6d4-0189d3eff01f",
    "caseId": "fc3b727e-d85d-40a0-a013-a3827e8ff2ca",
    "isLatest": true,
    "createdAt": "2026-08-08T17:17:30.2938145",
    "reviewPoints": [
      {
        "id": "b96423aa-abae-4c78-ba6b-06bf562c3086",
        "description": "ميزة الخصم تتمثل في غياب التوثيق الرسمي للتنبيهات أو الإخطارات المتبادلة بين الأطراف، مما يتيح له إنكار الاستلام أو الدفع بالتراخي في المطالبة.",
        "type": "Weakness"
      },
      {
        "id": "c0d417f4-2e44-4c7e-ab11-2053eee63450",
        "description": "قم بتفقيط وقسمة كافة المطالبات المالية إلى بنود مستقلة (أصل الدين، الفوائد أو التعويض عن المماطلة، والرسوم) وتوثيق كل بند بسند كتابي مستقل.",
        "type": "Suggestion"
      },
      {
        "id": "8a5c71c8-8331-4949-985f-4b5e7ab76311",
        "description": "تتمثل نقطة القوة الأساسية في صياغة الموضوع بوضوح حول 'Case for Milestones'، وتوافر السند المبدئي الذي يرجح كفة الموكل في إثبات أصل الالتزام وتفوقه إثباتياً على الخصم.",
        "type": "Strength"
      },
      {
        "id": "46cdf6dd-55c0-4f2f-99fe-68bc906784c1",
        "description": "يحتاج الملف إلى استيفاء النقاط والمعلومات التالية لضمان صياغة صحيفة الدعوى بشكل مكتمل: حصر وتفصيص المبالغ المالية المطلوبة والتعويضات الدقيقة عن الضرر المادي والمعنوي، وإدراج التواريخ الرسمية الدقيقة لبدء النزاع وتاريخ الإخلال بالتعهدات.",
        "type": "MissingCaseInfo"
      },
      {
        "id": "fc575ac1-2e53-4b2d-a9cf-6bb17e7494c9",
        "description": "المستندات المحددة المطلوبة لإكمال الملف: أصل العقد/الاتفاق المبرم، صورة بطاقة الرقم القومي سارية لكل أطراف الدعوى، إيصالات التحويل أو السداد المالي، وأي إنذارات رسمية على يد محضر.",
        "type": "MissingCaseDoc"
      },
      {
        "id": "5a2a9f4f-f3bc-4681-9514-c0c5315c93f3",
        "description": "قم بإعادة هيكلة وصف القضية في صورة جدول زمني متسلسل، يبدأ من تاريخ التعهد الأول، مروراً بتاريخ الإخلال، وصولاً إلى حجم الأضرار المترتبة حالياً.",
        "type": "Suggestion"
      },
      {
        "id": "2e206e48-0aa4-4c02-a2bb-cf6a683cf55b",
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


### Setup - Finalize Case

**Request:** POST http://localhost:5049/api/Case/fc3b727e-d85d-40a0-a013-a3827e8ff2ca/finalize

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
    "caseId": "fc3b727e-d85d-40a0-a013-a3827e8ff2ca",
    "totalEligibleLawyers": 0,
    "recommendations": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Setup - Client Creates Proposal

**Request:** POST http://localhost:5049/api/proposals

**Body:**
`json
{
  "LegalCaseId": "fc3b727e-d85d-40a0-a013-a3827e8ff2ca",
  "Message": "Let's make a contract.",
  "LawyerUserId": "8cae8669-35b7-4c0e-7826-08def54e43c0"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "cad781a9-766f-4d06-a827-b77e513aff9b",
    "legalCaseId": "fc3b727e-d85d-40a0-a013-a3827e8ff2ca",
    "caseTitle": "Case for Milestones",
    "clientUserId": "20131d70-b70d-4317-7825-08def54e43c0",
    "clientName": "Test Client",
    "lawyerUserId": "8cae8669-35b7-4c0e-7826-08def54e43c0",
    "lawyerName": "Test Lawyer",
    "message": "Let's make a contract.",
    "status": "Pending",
    "decisionReason": null,
    "createdAt": "2026-08-08T17:17:38.079549",
    "respondedAt": null,
    "updatedAt": "2026-08-08T17:17:38.079549",
    "conversationId": null
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
``n---


### Setup - Lawyer Accepts Proposal

**Request:** POST http://localhost:5049/api/proposals/cad781a9-766f-4d06-a827-b77e513aff9b/accept

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
    "id": "cad781a9-766f-4d06-a827-b77e513aff9b",
    "legalCaseId": "fc3b727e-d85d-40a0-a013-a3827e8ff2ca",
    "caseTitle": "Case for Milestones",
    "clientUserId": "20131d70-b70d-4317-7825-08def54e43c0",
    "clientName": "Test Client",
    "lawyerUserId": "8cae8669-35b7-4c0e-7826-08def54e43c0",
    "lawyerName": "Test Lawyer",
    "message": "Let's make a contract.",
    "status": "Accepted",
    "decisionReason": null,
    "createdAt": "2026-08-08T17:17:38.079549",
    "respondedAt": "2026-08-08T17:17:38.1443497",
    "updatedAt": "2026-08-08T17:17:38.1443497",
    "conversationId": "d45ad302-6a39-47de-9a7f-f45c90adb649"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Setup - Lawyer Creates Contract

**Request:** POST http://localhost:5049/api/contracts

**Body:**
`json
{
  "TermsAndConditions": "These are the complete terms and conditions that govern the contract and must be adhered to by both parties.",
  "Title": "Legal Representation Contract",
  "ProposalId": "cad781a9-766f-4d06-a827-b77e513aff9b"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "73ce8400-3bd5-4898-8feb-7793fee38ef1",
    "proposalId": "cad781a9-766f-4d06-a827-b77e513aff9b",
    "legalCaseId": "fc3b727e-d85d-40a0-a013-a3827e8ff2ca",
    "clientUserId": "20131d70-b70d-4317-7825-08def54e43c0",
    "lawyerUserId": "8cae8669-35b7-4c0e-7826-08def54e43c0",
    "title": "Legal Representation Contract",
    "termsAndConditions": "These are the complete terms and conditions that govern the contract and must be adhered to by both parties.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": null,
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 0,
    "version": "\"AAAAAAAAM04=\"",
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


### Setup - Lawyer Gets Contract

**Request:** GET http://localhost:5049/api/contracts/73ce8400-3bd5-4898-8feb-7793fee38ef1

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "73ce8400-3bd5-4898-8feb-7793fee38ef1",
    "proposalId": "cad781a9-766f-4d06-a827-b77e513aff9b",
    "legalCaseId": "fc3b727e-d85d-40a0-a013-a3827e8ff2ca",
    "clientUserId": "20131d70-b70d-4317-7825-08def54e43c0",
    "lawyerUserId": "8cae8669-35b7-4c0e-7826-08def54e43c0",
    "title": "Legal Representation Contract",
    "termsAndConditions": "These are the complete terms and conditions that govern the contract and must be adhered to by both parties.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": null,
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 0,
    "version": "\"AAAAAAAAM04=\"",
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


### Setup - Lawyer Gets Contract (Again)

**Request:** GET http://localhost:5049/api/contracts/73ce8400-3bd5-4898-8feb-7793fee38ef1

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "73ce8400-3bd5-4898-8feb-7793fee38ef1",
    "proposalId": "cad781a9-766f-4d06-a827-b77e513aff9b",
    "legalCaseId": "fc3b727e-d85d-40a0-a013-a3827e8ff2ca",
    "clientUserId": "20131d70-b70d-4317-7825-08def54e43c0",
    "lawyerUserId": "8cae8669-35b7-4c0e-7826-08def54e43c0",
    "title": "Legal Representation Contract",
    "termsAndConditions": "These are the complete terms and conditions that govern the contract and must be adhered to by both parties.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": "2026-08-08T17:17:38.4272458",
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 0,
    "version": "\"AAAAAAAAM1A=\"",
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


### POST Milestone - Negative Amount (400)

**Request:** POST http://localhost:5049/api/contracts/73ce8400-3bd5-4898-8feb-7793fee38ef1/milestones

**Body:**
`json
{
  "Description": "Details.",
  "DurationDays": 10,
  "Amount": -100,
  "Title": "Draft Milestone 1",
  "OrderNumber": 1
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
    "Amount": [
      "قيمة المرحلة يجب أن تكون أكبر من صفر بالجنيه المصري."
    ]
  },
  "traceId": "00-867e47f3d7f281cb91e9ba60b3e49fd8-6a3f9bbbe0854e22-00"
}
``n---


### POST Milestone - Missing Title (400)

**Request:** POST http://localhost:5049/api/contracts/73ce8400-3bd5-4898-8feb-7793fee38ef1/milestones

**Body:**
`json
{
  "Description": "Details.",
  "DurationDays": 10,
  "Amount": 1000,
  "OrderNumber": 1
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
    "Title": [
      "The Title field is required.",
      "عنوان المرحلة مطلوب."
    ]
  },
  "traceId": "00-6f7c6c2100a6946b31f7781047d06deb-976e166d42d27619-00"
}
``n---


### POST Milestone - Happy Path (201)

**Request:** POST http://localhost:5049/api/contracts/73ce8400-3bd5-4898-8feb-7793fee38ef1/milestones

**Body:**
`json
{
  "Description": "Detailed research for the case.",
  "DurationDays": 10,
  "Amount": 1000.5,
  "Title": "Phase 1: Research",
  "OrderNumber": 1
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "9a5b08f6-0ad6-493d-951e-a79efdbdb898",
    "orderNumber": 1,
    "title": "Phase 1: Research",
    "description": "Detailed research for the case.",
    "amount": 1000.5,
    "durationDays": 10,
    "dueDate": null,
    "status": 0,
    "fundingStatus": 0,
    "escrowHoldId": null,
    "fundedAt": null,
    "submittedAt": null,
    "autoAcceptEligibleAt": null,
    "holdExpiresAt": null,
    "netLawyerAmount": null,
    "version": "\"AAAAAAAAM1Q=\"",
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


### GET Milestones - List (200)

**Request:** GET http://localhost:5049/api/contracts/73ce8400-3bd5-4898-8feb-7793fee38ef1/milestones

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": [
    {
      "id": "9a5b08f6-0ad6-493d-951e-a79efdbdb898",
      "orderNumber": 1,
      "title": "Phase 1: Research",
      "description": "Detailed research for the case.",
      "amount": 1000.5,
      "durationDays": 10,
      "dueDate": null,
      "status": 0,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAM1Q=\"",
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


### GET Milestones - Attacker (403)

**Request:** GET http://localhost:5049/api/contracts/73ce8400-3bd5-4898-8feb-7793fee38ef1/milestones

**Response Status:** 403

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "غير مصرح لك بالاطلاع على هذا العقد.",
  "errors": null,
  "statusCode": 403
}
``n---


### PUT Milestone - Missing If-Match (400)

**Request:** PUT http://localhost:5049/api/contracts/73ce8400-3bd5-4898-8feb-7793fee38ef1/milestones/9a5b08f6-0ad6-493d-951e-a79efdbdb898

**Body:**
`json
{
  "Description": "More details.",
  "DurationDays": 15,
  "Title": "Phase 1: Deep Research"
}
``n
**Response Status:** 412

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "قيمة If-Match مطلوبة.",
  "errors": null,
  "statusCode": 412
}
``n---


### PUT Milestone - Outdated If-Match (412)

**Request:** PUT http://localhost:5049/api/contracts/73ce8400-3bd5-4898-8feb-7793fee38ef1/milestones/9a5b08f6-0ad6-493d-951e-a79efdbdb898

**Body:**
`json
{
  "Description": "More details.",
  "DurationDays": 15,
  "Title": "Phase 1: Deep Research"
}
``n
**Response Status:** PreconditionFailed

**Response Body:** (Empty)
---


### PUT Milestone - Happy Path (200)

**Request:** PUT http://localhost:5049/api/contracts/73ce8400-3bd5-4898-8feb-7793fee38ef1/milestones/9a5b08f6-0ad6-493d-951e-a79efdbdb898

**Body:**
`json
{
  "Description": "More details.",
  "DurationDays": 15,
  "Title": "Phase 1: Deep Research"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "9a5b08f6-0ad6-493d-951e-a79efdbdb898",
    "orderNumber": 1,
    "title": "Phase 1: Deep Research",
    "description": "More details.",
    "amount": 1000.5,
    "durationDays": 15,
    "dueDate": null,
    "status": 0,
    "fundingStatus": 0,
    "escrowHoldId": null,
    "fundedAt": null,
    "submittedAt": null,
    "autoAcceptEligibleAt": null,
    "holdExpiresAt": null,
    "netLawyerAmount": null,
    "version": "\"AAAAAAAAM1U=\"",
    "permittedActions": [
      "Update",
      "Approve"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### POST Milestone 2 (201)

**Request:** POST http://localhost:5049/api/contracts/73ce8400-3bd5-4898-8feb-7793fee38ef1/milestones

**Body:**
`json
{
  "Description": "Do the work.",
  "DurationDays": 5,
  "Amount": 2000,
  "Title": "Phase 2: Execution",
  "OrderNumber": 2
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "7e20f862-2b26-4bec-97b7-7630f6ab2a81",
    "orderNumber": 2,
    "title": "Phase 2: Execution",
    "description": "Do the work.",
    "amount": 2000,
    "durationDays": 5,
    "dueDate": null,
    "status": 0,
    "fundingStatus": 0,
    "escrowHoldId": null,
    "fundedAt": null,
    "submittedAt": null,
    "autoAcceptEligibleAt": null,
    "holdExpiresAt": null,
    "netLawyerAmount": null,
    "version": "\"AAAAAAAAM1Y=\"",
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


### GET Milestones - Refresh ETag before Approve

**Request:** GET http://localhost:5049/api/contracts/73ce8400-3bd5-4898-8feb-7793fee38ef1/milestones

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": [
    {
      "id": "9a5b08f6-0ad6-493d-951e-a79efdbdb898",
      "orderNumber": 1,
      "title": "Phase 1: Deep Research",
      "description": "More details.",
      "amount": 1000.5,
      "durationDays": 15,
      "dueDate": null,
      "status": 0,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAM1U=\"",
      "permittedActions": [
        "Update",
        "Approve"
      ]
    },
    {
      "id": "7e20f862-2b26-4bec-97b7-7630f6ab2a81",
      "orderNumber": 2,
      "title": "Phase 2: Execution",
      "description": "Do the work.",
      "amount": 2000.0,
      "durationDays": 5,
      "dueDate": null,
      "status": 0,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAM1Y=\"",
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


### POST Approve - Outdated If-Match (412)

**Request:** POST http://localhost:5049/api/milestones/9a5b08f6-0ad6-493d-951e-a79efdbdb898/approve

**Body:**
`json
{}
``n
**Response Status:** PreconditionFailed

**Response Body:** (Empty)
---


### POST Approve - Happy Path (200)

**Request:** POST http://localhost:5049/api/milestones/9a5b08f6-0ad6-493d-951e-a79efdbdb898/approve

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
    "entityId": "9a5b08f6-0ad6-493d-951e-a79efdbdb898",
    "status": "Draft",
    "occurredAt": "2026-08-08T17:17:39.4139081Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### GET Milestones - Refresh ETag before Lawyer Approve

**Request:** GET http://localhost:5049/api/contracts/73ce8400-3bd5-4898-8feb-7793fee38ef1/milestones

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": [
    {
      "id": "9a5b08f6-0ad6-493d-951e-a79efdbdb898",
      "orderNumber": 1,
      "title": "Phase 1: Deep Research",
      "description": "More details.",
      "amount": 1000.5,
      "durationDays": 15,
      "dueDate": null,
      "status": 0,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAM1c=\"",
      "permittedActions": [
        "Update",
        "Approve"
      ]
    },
    {
      "id": "7e20f862-2b26-4bec-97b7-7630f6ab2a81",
      "orderNumber": 2,
      "title": "Phase 2: Execution",
      "description": "Do the work.",
      "amount": 2000.0,
      "durationDays": 5,
      "dueDate": null,
      "status": 0,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAM1Y=\"",
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


### POST Approve (Lawyer) - Happy Path (200)

**Request:** POST http://localhost:5049/api/milestones/9a5b08f6-0ad6-493d-951e-a79efdbdb898/approve

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
    "entityId": "9a5b08f6-0ad6-493d-951e-a79efdbdb898",
    "status": "AwaitingFunding",
    "occurredAt": "2026-08-08T17:17:39.5748429Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### GET Milestones - Refresh ETag M2

**Request:** GET http://localhost:5049/api/contracts/73ce8400-3bd5-4898-8feb-7793fee38ef1/milestones

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": [
    {
      "id": "9a5b08f6-0ad6-493d-951e-a79efdbdb898",
      "orderNumber": 1,
      "title": "Phase 1: Deep Research",
      "description": "More details.",
      "amount": 1000.5,
      "durationDays": 15,
      "dueDate": null,
      "status": 1,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAM1g=\"",
      "permittedActions": []
    },
    {
      "id": "7e20f862-2b26-4bec-97b7-7630f6ab2a81",
      "orderNumber": 2,
      "title": "Phase 2: Execution",
      "description": "Do the work.",
      "amount": 2000.0,
      "durationDays": 5,
      "dueDate": null,
      "status": 0,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAM1Y=\"",
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


### GET Milestones - Refresh ETag M2 for Lawyer

**Request:** GET http://localhost:5049/api/contracts/73ce8400-3bd5-4898-8feb-7793fee38ef1/milestones

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": [
    {
      "id": "9a5b08f6-0ad6-493d-951e-a79efdbdb898",
      "orderNumber": 1,
      "title": "Phase 1: Deep Research",
      "description": "More details.",
      "amount": 1000.5,
      "durationDays": 15,
      "dueDate": null,
      "status": 1,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAM1g=\"",
      "permittedActions": [
        "ReadyForFunding"
      ]
    },
    {
      "id": "7e20f862-2b26-4bec-97b7-7630f6ab2a81",
      "orderNumber": 2,
      "title": "Phase 2: Execution",
      "description": "Do the work.",
      "amount": 2000.0,
      "durationDays": 5,
      "dueDate": null,
      "status": 0,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAM1o=\"",
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


### GET Milestones - Refresh ETag before ReadyForFunding

**Request:** GET http://localhost:5049/api/contracts/73ce8400-3bd5-4898-8feb-7793fee38ef1/milestones

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": [
    {
      "id": "9a5b08f6-0ad6-493d-951e-a79efdbdb898",
      "orderNumber": 1,
      "title": "Phase 1: Deep Research",
      "description": "More details.",
      "amount": 1000.5,
      "durationDays": 15,
      "dueDate": null,
      "status": 1,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAM1g=\"",
      "permittedActions": [
        "ReadyForFunding"
      ]
    },
    {
      "id": "7e20f862-2b26-4bec-97b7-7630f6ab2a81",
      "orderNumber": 2,
      "title": "Phase 2: Execution",
      "description": "Do the work.",
      "amount": 2000.0,
      "durationDays": 5,
      "dueDate": null,
      "status": 1,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAM1s=\"",
      "permittedActions": []
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### POST ReadyForFunding - Happy Path (200)

**Request:** POST http://localhost:5049/api/milestones/9a5b08f6-0ad6-493d-951e-a79efdbdb898/ready-for-funding

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
    "entityId": "9a5b08f6-0ad6-493d-951e-a79efdbdb898",
    "status": "AwaitingFunding",
    "occurredAt": "2026-08-08T17:18:10.0343773Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### POST Submit - Happy Path (200)

**Request:** POST http://localhost:5049/api/milestones/9a5b08f6-0ad6-493d-951e-a79efdbdb898/submit

**Body:**
`json
{
  "Notes": "Work completed. Check files.",
  "StoredFileIds": [
    "3ad700ac-8d37-45b4-bdf2-74bda90a3164"
  ]
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "9a5b08f6-0ad6-493d-951e-a79efdbdb898",
    "orderNumber": 1,
    "title": "Phase 1: Deep Research",
    "description": "More details.",
    "amount": 1000.5,
    "durationDays": 15,
    "dueDate": null,
    "status": 4,
    "fundingStatus": 2,
    "escrowHoldId": "34c896c7-9371-4d64-ac77-1dad00d7fc6b",
    "fundedAt": "2026-08-08T17:18:10.27",
    "submittedAt": "2026-08-08T17:18:11.0782424Z",
    "autoAcceptEligibleAt": "2026-08-15T17:18:11.0782424Z",
    "holdExpiresAt": null,
    "netLawyerAmount": 1000.5,
    "version": "\"AAAAAAAAM38=\"",
    "permittedActions": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### POST RequestChanges - Happy Path (200)

**Request:** POST http://localhost:5049/api/milestones/9a5b08f6-0ad6-493d-951e-a79efdbdb898/request-changes

**Body:**
`json
{
  "Reason": "Need more details in report."
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "9a5b08f6-0ad6-493d-951e-a79efdbdb898",
    "orderNumber": 1,
    "title": "Phase 1: Deep Research",
    "description": "More details.",
    "amount": 1000.5,
    "durationDays": 15,
    "dueDate": null,
    "status": 3,
    "fundingStatus": 2,
    "escrowHoldId": "34c896c7-9371-4d64-ac77-1dad00d7fc6b",
    "fundedAt": "2026-08-08T17:18:10.27",
    "submittedAt": null,
    "autoAcceptEligibleAt": null,
    "holdExpiresAt": null,
    "netLawyerAmount": 1000.5,
    "version": "\"AAAAAAAAM4E=\"",
    "permittedActions": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### POST Submit Again - Happy Path (200)

**Request:** POST http://localhost:5049/api/milestones/9a5b08f6-0ad6-493d-951e-a79efdbdb898/submit

**Body:**
`json
{
  "Notes": "Work completed. Check files.",
  "StoredFileIds": [
    "3ad700ac-8d37-45b4-bdf2-74bda90a3164"
  ]
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "9a5b08f6-0ad6-493d-951e-a79efdbdb898",
    "orderNumber": 1,
    "title": "Phase 1: Deep Research",
    "description": "More details.",
    "amount": 1000.5,
    "durationDays": 15,
    "dueDate": null,
    "status": 4,
    "fundingStatus": 2,
    "escrowHoldId": "34c896c7-9371-4d64-ac77-1dad00d7fc6b",
    "fundedAt": "2026-08-08T17:18:10.27",
    "submittedAt": "2026-08-08T17:18:11.627718Z",
    "autoAcceptEligibleAt": "2026-08-15T17:18:11.627718Z",
    "holdExpiresAt": null,
    "netLawyerAmount": 1000.5,
    "version": "\"AAAAAAAAM4M=\"",
    "permittedActions": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### POST Accept - Happy Path (200)

**Request:** POST http://localhost:5049/api/milestones/9a5b08f6-0ad6-493d-951e-a79efdbdb898/accept

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
    "id": "9a5b08f6-0ad6-493d-951e-a79efdbdb898",
    "orderNumber": 1,
    "title": "Phase 1: Deep Research",
    "description": "More details.",
    "amount": 1000.5,
    "durationDays": 15,
    "dueDate": null,
    "status": 5,
    "fundingStatus": 2,
    "escrowHoldId": "34c896c7-9371-4d64-ac77-1dad00d7fc6b",
    "fundedAt": "2026-08-08T17:18:10.27",
    "submittedAt": "2026-08-08T17:18:11.627718",
    "autoAcceptEligibleAt": null,
    "holdExpiresAt": "2026-08-22T17:18:11.7787325Z",
    "netLawyerAmount": 1000.5,
    "version": "\"AAAAAAAAM4Y=\"",
    "permittedActions": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### GET Milestones - Refresh ETag before ChangeRequest

**Request:** GET http://localhost:5049/api/contracts/73ce8400-3bd5-4898-8feb-7793fee38ef1/milestones

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": [
    {
      "id": "9a5b08f6-0ad6-493d-951e-a79efdbdb898",
      "orderNumber": 1,
      "title": "Phase 1: Deep Research",
      "description": "More details.",
      "amount": 1000.5,
      "durationDays": 15,
      "dueDate": null,
      "status": 5,
      "fundingStatus": 2,
      "escrowHoldId": "34c896c7-9371-4d64-ac77-1dad00d7fc6b",
      "fundedAt": "2026-08-08T17:18:10.27",
      "submittedAt": "2026-08-08T17:18:11.627718",
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": "2026-08-22T17:18:11.7787325",
      "netLawyerAmount": 1000.5,
      "version": "\"AAAAAAAAM4Y=\"",
      "permittedActions": []
    },
    {
      "id": "7e20f862-2b26-4bec-97b7-7630f6ab2a81",
      "orderNumber": 2,
      "title": "Phase 2: Execution",
      "description": "Do the work.",
      "amount": 2000.0,
      "durationDays": 5,
      "dueDate": null,
      "status": 1,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAM1s=\"",
      "permittedActions": []
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### GET Milestones - Refresh ETag after M2 Funding

**Request:** GET http://localhost:5049/api/contracts/73ce8400-3bd5-4898-8feb-7793fee38ef1/milestones

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": [
    {
      "id": "9a5b08f6-0ad6-493d-951e-a79efdbdb898",
      "orderNumber": 1,
      "title": "Phase 1: Deep Research",
      "description": "More details.",
      "amount": 1000.5,
      "durationDays": 15,
      "dueDate": null,
      "status": 5,
      "fundingStatus": 2,
      "escrowHoldId": "34c896c7-9371-4d64-ac77-1dad00d7fc6b",
      "fundedAt": "2026-08-08T17:18:10.27",
      "submittedAt": "2026-08-08T17:18:11.627718",
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": "2026-08-22T17:18:11.7787325",
      "netLawyerAmount": 1000.5,
      "version": "\"AAAAAAAAM4Y=\"",
      "permittedActions": []
    },
    {
      "id": "7e20f862-2b26-4bec-97b7-7630f6ab2a81",
      "orderNumber": 2,
      "title": "Phase 2: Execution",
      "description": "Do the work.",
      "amount": 2000.0,
      "durationDays": 5,
      "dueDate": null,
      "status": 3,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": "2026-08-08T17:18:12.1",
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAM4g=\"",
      "permittedActions": [
        "Submit"
      ]
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### POST ChangeRequest - Happy Path (201)

**Request:** POST http://localhost:5049/api/milestones/7e20f862-2b26-4bec-97b7-7630f6ab2a81/change-requests

**Body:**
`json
{
  "ProposedDescription": "Do the hard work.",
  "ProposedDurationDays": 10,
  "Reason": "Need more time."
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "entityId": "2261eb85-7d75-4d5c-90f5-b728b26da6ca",
    "status": "Pending",
    "occurredAt": "2026-08-08T17:18:12.2235533Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
``n---


### POST Approve ChangeRequest - Happy Path (200)

**Request:** POST http://localhost:5049/api/change-requests/2261eb85-7d75-4d5c-90f5-b728b26da6ca/approve

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
    "entityId": "2261eb85-7d75-4d5c-90f5-b728b26da6ca",
    "status": "Approved",
    "occurredAt": "2026-08-08T17:18:12.5477038Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---



# Account Creation & Verification Report

Generated at: 2026-08-16 17:08:54

---

### 1. Register Client Account

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
    "Email":  "client_20260816170854@smartcourt.test",
    "FullName":  "Client Account (20260816170854)",
    "ConfirmPassword":  "SmartCourt@2026!",
    "Password":  "SmartCourt@2026!"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "userId":  "6624019f-ed2d-4de4-35d9-08defb9d0105",
                 "email":  "client_20260816170854@smartcourt.test",
                 "fullName":  "Client Account (20260816170854)",
                 "role":  "Client"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
``n---


**Confirmation URL Found for client_20260816170854@smartcourt.test:** http://localhost:5173/verify-email?userId=6624019f-ed2d-4de4-35d9-08defb9d0105&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI4RHJBdDBsTXMzK1FVTVFCM0ZiUWxwWnh1b2F6M29SUWlnZ08vaHhwVFY3eDR0cnlSMnlwVW9rS21qSmxmMmlkWHhJRlQrRGYwdExzUTAvcFAzWVRkNWpHcXNQMHFicHM2TDRkVGZPT3Q5VklCZWlBYy8yR204RVFYNWZvUGRpYnlGMVdXZHU4dHhNOVY5KzVPVE9yUXB2eVRzVG5XTmI1a2dxTmhQbHBJLzRoK2xSaWJFMTR3dkZ0VWtUTXVaUzh6OVNqMVIyOFdVeXQxbGx0MUhhVFYyT3FCWks4Rkk5UktVMGkrMVN3c2NTdz09


### Confirm Email for client_20260816170854@smartcourt.test

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=6624019f-ed2d-4de4-35d9-08defb9d0105&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI4RHJBdDBsTXMzK1FVTVFCM0ZiUWxwWnh1b2F6M29SUWlnZ08vaHhwVFY3eDR0cnlSMnlwVW9rS21qSmxmMmlkWHhJRlQrRGYwdExzUTAvcFAzWVRkNWpHcXNQMHFicHM2TDRkVGZPT3Q5VklCZWlBYy8yR204RVFYNWZvUGRpYnlGMVdXZHU4dHhNOVY5KzVPVE9yUXB2eVRzVG5XTmI1a2dxTmhQbHBJLzRoK2xSaWJFMTR3dkZ0VWtUTXVaUzh6OVNqMVIyOFdVeXQxbGx0MUhhVFYyT3FCWks4Rkk5UktVMGkrMVN3c2NTdz09

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "message":  "تم تأكيد البريد الإلكتروني بنجاح.",
    "errors":  null,
    "statusCode":  200
}
``n---


### 2. Login Client Account (Post-Verification)

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "SmartCourt@2026!",
    "Email":  "client_20260816170854@smartcourt.test"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "6624019f-ed2d-4de4-35d9-08defb9d0105",
                              "email":  "client_20260816170854@smartcourt.test",
                              "fullName":  "Client Account (20260816170854)",
                              "role":  "Client",
                              "status":  "Unverified",
                              "rejectionReason":  null
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2NjI0MDE5Zi1lZDJkLTRkZTQtMzVkOS0wOGRlZmI5ZDAxMDUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjY2MjQwMTlmLWVkMmQtNGRlNC0zNWQ5LTA4ZGVmYjlkMDEwNSIsImVtYWlsIjoiY2xpZW50XzIwMjYwODE2MTcwODU0QHNtYXJ0Y291cnQudGVzdCIsIm5hbWUiOiJDbGllbnQgQWNjb3VudCAoMjAyNjA4MTYxNzA4NTQpIiwic2VjdXJpdHlfc3RhbXAiOiJHTzRQMklNNVFLWjI3SlUyQlBKM0dZTzZYS0NJUUlQNSIsImp0aSI6ImJiMDgxYjYzLWJlOTItNDY5MS1iZTkxLWE1ZjcyODI3Mzk4ZCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNsaWVudCIsIm5iZiI6MTc4Njg4OTMzNiwiZXhwIjoxNzg2ODkyOTM2LCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.XmpYyTXpPA8_-DpgHl_EQAXtPTPJCZUPf7ccHUo-bDo",
                 "expiresIn":  3600,
                 "refreshToken":  "dyzFyUQA8WJBM6173zw5Tm3hPgde1Lu3wAEb5MH/tGmX/XPKwrfQFsMaWpKFmLyIAsYvlgs6C367Ako4Ub4yGQ==",
                 "refreshTokenExpiration":  "2026-08-23T14:08:56.9451018+00:00"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 3. Register Lawyer Account

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
`json
{
    "Email":  "lawyer_20260816170854@smartcourt.test",
    "FullName":  "Lawyer Account (20260816170854)",
    "ConfirmPassword":  "SmartCourt@2026!",
    "Password":  "SmartCourt@2026!"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "userId":  "3222935a-24e1-453a-35da-08defb9d0105",
                 "email":  "lawyer_20260816170854@smartcourt.test",
                 "fullName":  "Lawyer Account (20260816170854)",
                 "role":  "Lawyer"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
``n---


**Confirmation URL Found for lawyer_20260816170854@smartcourt.test:** http://localhost:5173/verify-email?userId=3222935a-24e1-453a-35da-08defb9d0105&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIrbGtZMEUvTXBsVERzNlFpd1hVUlJJL1hoR0R5UEY1NXVxY3kzMmpoMXRjT1Z0NWY3aFRZUkl3UVQ0eDY5N3FnNzhSMGJTUmE1SGZrRWUydVh3SHQ4YUd4TXh2a1pZU2xTMmFOaXVPRDJxeit0a3VQTjE4S0FRTUVXOHFhaTRsL3pSaGlRKy9HQ0Eycld3QVdVVlAzbEZld25RVjRrT0pNMzI2dnFqUGJ2WGZTWERVTzB2QkVEMnN5TmRBSHNVL0ZlSHZGbW8vdEdGNzdzTGplS1V1czJZUGtkajM2NGI3SWx1eEZGd1Y4ZjlEdz09


### Confirm Email for lawyer_20260816170854@smartcourt.test

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=3222935a-24e1-453a-35da-08defb9d0105&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIrbGtZMEUvTXBsVERzNlFpd1hVUlJJL1hoR0R5UEY1NXVxY3kzMmpoMXRjT1Z0NWY3aFRZUkl3UVQ0eDY5N3FnNzhSMGJTUmE1SGZrRWUydVh3SHQ4YUd4TXh2a1pZU2xTMmFOaXVPRDJxeit0a3VQTjE4S0FRTUVXOHFhaTRsL3pSaGlRKy9HQ0Eycld3QVdVVlAzbEZld25RVjRrT0pNMzI2dnFqUGJ2WGZTWERVTzB2QkVEMnN5TmRBSHNVL0ZlSHZGbW8vdEdGNzdzTGplS1V1czJZUGtkajM2NGI3SWx1eEZGd1Y4ZjlEdz09

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "message":  "تم تأكيد البريد الإلكتروني بنجاح.",
    "errors":  null,
    "statusCode":  200
}
``n---


### 4. Login Lawyer Account (Post-Verification)

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "SmartCourt@2026!",
    "Email":  "lawyer_20260816170854@smartcourt.test"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "3222935a-24e1-453a-35da-08defb9d0105",
                              "email":  "lawyer_20260816170854@smartcourt.test",
                              "fullName":  "Lawyer Account (20260816170854)",
                              "role":  "Lawyer",
                              "status":  "Unverified",
                              "rejectionReason":  null
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzMjIyOTM1YS0yNGUxLTQ1M2EtMzVkYS0wOGRlZmI5ZDAxMDUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjMyMjI5MzVhLTI0ZTEtNDUzYS0zNWRhLTA4ZGVmYjlkMDEwNSIsImVtYWlsIjoibGF3eWVyXzIwMjYwODE2MTcwODU0QHNtYXJ0Y291cnQudGVzdCIsIm5hbWUiOiJMYXd5ZXIgQWNjb3VudCAoMjAyNjA4MTYxNzA4NTQpIiwic2VjdXJpdHlfc3RhbXAiOiJaUlBXTklGRkpaVjdFWFRORTJNUU9CNURNQVlZTkxVNiIsImp0aSI6IjA1ZjQ4Y2EwLWFiZDQtNGEzOS05NGFlLTJlNTZkMTY3ZTI3YyIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4Njg4OTMzOCwiZXhwIjoxNzg2ODkyOTM4LCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.sp7F5rDO1FfdOfMWfitiVAcI1R9heVHUPqZHClOgYtw",
                 "expiresIn":  3600,
                 "refreshToken":  "rd9txCLo79KtjREwD2kI+s0yA9kD0pwX0IWiPx5o92I6bCC4Ex9EQbbbqI3al1NywqN+PxLPDI4cltQIaiB26Q==",
                 "refreshTokenExpiration":  "2026-08-23T14:08:58.6612733+00:00"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 5. Complete Lawyer Profile

**Request:** POST http://localhost:5049/api/lawyers/profile/complete

**Body:**
`json
{
    "PhoneNumber":  "+201012345678",
    "Governorate":  "Cairo",
    "Specializations":  [
                            {
                                "YearsOfExperience":  7,
                                "CasesHandled":  25,
                                "Specialization":  1
                            },
                            {
                                "YearsOfExperience":  4,
                                "CasesHandled":  15,
                                "Specialization":  2
                            }
                        ],
    "City":  "Nasr City",
    "Level":  2,
    "Bio":  "Experienced attorney specializing in civil and commercial law.",
    "DateOfBirth":  "1990-01-01",
    "Address":  "Cairo, Egypt",
    "NationalNumber":  "29001018614043",
    "Gender":  1
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "message":  "تم استكمال البيانات بنجاح",
    "errors":  null,
    "statusCode":  200
}
``n---


### 6. Re-Login Lawyer after profile completion

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "SmartCourt@2026!",
    "Email":  "lawyer_20260816170854@smartcourt.test"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "3222935a-24e1-453a-35da-08defb9d0105",
                              "email":  "lawyer_20260816170854@smartcourt.test",
                              "fullName":  "Lawyer Account (20260816170854)",
                              "role":  "Lawyer",
                              "status":  "PendingReview",
                              "rejectionReason":  null
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzMjIyOTM1YS0yNGUxLTQ1M2EtMzVkYS0wOGRlZmI5ZDAxMDUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjMyMjI5MzVhLTI0ZTEtNDUzYS0zNWRhLTA4ZGVmYjlkMDEwNSIsImVtYWlsIjoibGF3eWVyXzIwMjYwODE2MTcwODU0QHNtYXJ0Y291cnQudGVzdCIsIm5hbWUiOiJMYXd5ZXIgQWNjb3VudCAoMjAyNjA4MTYxNzA4NTQpIiwic2VjdXJpdHlfc3RhbXAiOiJQNTJCSlFDSllHSlI1TUo1WVlVSTVOWE40VzVPTEpJNCIsImp0aSI6IjU3OWVkYjU3LTg0NDQtNDQ1Zi05NWE0LWIxOGE2MzVkYWFjNSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4Njg4OTMzOSwiZXhwIjoxNzg2ODkyOTM5LCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.PFzfJsNLglYehXZ8X3Fv7wE1mI_AEcV0GAZjSY4JWTk",
                 "expiresIn":  3600,
                 "refreshToken":  "oZMKDiRwiiGP6cb6gDPZJ5+0DSrMwpI2cwSY9RmHkyO3vyLiIPxJtnvxn1oF86HkD6gvCUTBtIewzEFL2ngXSw==",
                 "refreshTokenExpiration":  "2026-08-23T14:08:59.0653349+00:00"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---



# Final Created Accounts Summary

| Role | Full Name | Email | Password | User ID | Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Client** | Client Account (20260816170854) | client_20260816170854@smartcourt.test | SmartCourt@2026! | 6624019f-ed2d-4de4-35d9-08defb9d0105 | Verified & Active |
| **Lawyer** | Lawyer Account (20260816170854) | lawyer_20260816170854@smartcourt.test | SmartCourt@2026! | 3222935a-24e1-453a-35da-08defb9d0105 | Verified & Profile Completed |


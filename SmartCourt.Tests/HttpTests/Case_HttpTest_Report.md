# Case Slice HTTP Tests End-to-End Workflow Report

Generated at 2026-08-20 08:34:14


### Register Client

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
    "Email":  "client_case_test_20260820083414@example.com",
    "FullName":  "Test Client",
    "ConfirmPassword":  "Password123!",
    "Password":  "Password123!"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "userId":  "e978531d-742c-419e-045a-08defe7cae1c",
                 "email":  "client_case_test_20260820083414@example.com",
                 "fullName":  "Test Client",
                 "role":  "Client"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
``n---


Failed to read api_log.txt for client_case_test_20260820083414@example.com

### Login Client

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Password123!",
    "Email":  "client_case_test_20260820083414@example.com"
}
``n
**Response Status:** 403

**Response Body:**
`json
{
    "success":  false,
    "data":  null,
    "message":  "يرجى تأكيد البريد الإلكتروني أولاً",
    "errors":  null,
    "statusCode":  403
}
``n---


### Register Lawyer

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
`json
{
    "Email":  "lawyer_case_test_20260820083439@example.com",
    "FullName":  "Test Lawyer",
    "ConfirmPassword":  "Password123!",
    "Password":  "Password123!"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "userId":  "57c6b237-9e51-48ce-045b-08defe7cae1c",
                 "email":  "lawyer_case_test_20260820083439@example.com",
                 "fullName":  "Test Lawyer",
                 "role":  "Lawyer"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
``n---


Failed to read api_log.txt for lawyer_case_test_20260820083439@example.com

### Login Lawyer

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Password123!",
    "Email":  "lawyer_case_test_20260820083439@example.com"
}
``n
**Response Status:** 403

**Response Body:**
`json
{
    "success":  false,
    "data":  null,
    "message":  "يرجى تأكيد البريد الإلكتروني أولاً",
    "errors":  null,
    "statusCode":  403
}
``n---


### Setup - Complete Client Profile

**Request:** POST http://localhost:5049/api/clients/profile/complete

**Body:**
`json
{
    "NationalNumber":  "29001013044517",
    "DateOfBirth":  "1990-01-01",
    "PhoneNumber":  "+201011111111",
    "Gender":  1,
    "Address":  "Cairo"
}
``n
**Response Status:** 401

**Response Body:** (Empty)
---


### Setup - Complete Lawyer Profile

**Request:** POST http://localhost:5049/api/lawyers/profile/complete

**Body:**
`json
{
    "DateOfBirth":  "1985-01-01",
    "Level":  1,
    "Bio":  "Expert Lawyer",
    "Gender":  1,
    "PhoneNumber":  "+201022222222",
    "Specializations":  [
                            {
                                "YearsOfExperience":  5,
                                "CasesHandled":  10,
                                "Specialization":  1
                            }
                        ],
    "NationalNumber":  "28501012469627",
    "Address":  "Cairo"
}
``n
**Response Status:** 401

**Response Body:** (Empty)
---


### Setup - Login Admin

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Admin@123",
    "Email":  "admin@smartcourt.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "7b88b1f8-8e75-4c81-814f-08dee91d7c8f",
                              "email":  "admin@smartcourt.com",
                              "fullName":  "System Administrator",
                              "role":  "Admin",
                              "status":  "Active",
                              "rejectionReason":  null
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3Yjg4YjFmOC04ZTc1LTRjODEtODE0Zi0wOGRlZTkxZDdjOGYiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjdiODhiMWY4LThlNzUtNGM4MS04MTRmLTA4ZGVlOTFkN2M4ZiIsImVtYWlsIjoiYWRtaW5Ac21hcnRjb3VydC5jb20iLCJuYW1lIjoiU3lzdGVtIEFkbWluaXN0cmF0b3IiLCJzZWN1cml0eV9zdGFtcCI6IjRDUVNJQVJOWU9aN1VVTjVMRVU1TUlONzdOTUxQVDc1IiwianRpIjoiMjc0MGYxYWEtZjg1NS00YmQ3LTlmZWMtYjUyMTkyYzY4MDU5IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJuYmYiOjE3ODcyMDQwOTgsImV4cCI6MTc4NzIwNzY5OCwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.thWCv6jr28W5hjszjfY3-ZuLpAXQDoWaucd3WlN3pSY",
                 "expiresIn":  3600,
                 "refreshToken":  "QE1nuImUYA6ASQ7wY8hdnNWA0rsm9YxlHxzzBK/+B+7HfgZz+bW083hj9NU6C87kHpbdJ2IM+X8Ycy9n3BrWkg==",
                 "refreshTokenExpiration":  "2026-08-27T05:34:58.8554703+00:00"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### Setup - Admin Approve Lawyer

**Request:** PATCH http://localhost:5049/api/admin/verifications//approve-account

**Body:**
`json
{

}
``n
**Response Status:** 404

**Response Body:** (Empty)
---


### Setup - Admin Approve Client

**Request:** PATCH http://localhost:5049/api/admin/verifications//approve-account

**Body:**
`json
{

}
``n
**Response Status:** 404

**Response Body:** (Empty)
---


### Setup - Re-Login Lawyer

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Password123!",
    "Email":  "lawyer_case_test_20260820083439@example.com"
}
``n
**Response Status:** 403

**Response Body:**
`json
{
    "success":  false,
    "data":  null,
    "message":  "يرجى تأكيد البريد الإلكتروني أولاً",
    "errors":  null,
    "statusCode":  403
}
``n---


### Setup - Re-Login Client

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Password123!",
    "Email":  "client_case_test_20260820083414@example.com"
}
``n
**Response Status:** 403

**Response Body:**
`json
{
    "success":  false,
    "data":  null,
    "message":  "يرجى تأكيد البريد الإلكتروني أولاً",
    "errors":  null,
    "statusCode":  403
}
``n---


### Create Case (400 Validation Error)

**Request:** POST http://localhost:5049/api/Case

**Body:**
(multipart/form-data)

**Response Status:** 401

**Response Body:** (Empty)
---


### Create Case (Valid Success)

**Request:** POST http://localhost:5049/api/Case

**Body:**
(multipart/form-data)

**Response Status:** 401

**Response Body:** (Empty)
---



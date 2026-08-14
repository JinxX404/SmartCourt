# Case Slice HTTP Tests End-to-End Workflow Report

Generated at 2026-08-14 21:11:03


### Register Client

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
    "Email":  "client_case_test_20260814211103@example.com",
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
                 "userId":  "8a763330-edaa-4f26-8a66-08defa2f6750",
                 "email":  "client_case_test_20260814211103@example.com",
                 "fullName":  "Test Client",
                 "role":  "Client"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
``n---


Found confirmation URL for client_case_test_20260814211103@example.com: http://localhost:5173/verify-email?userId=8a763330-edaa-4f26-8a66-08defa2f6750&token=Q2ZESjhPNXpQSmRGK0NWT3Z5RWxYSTRjQmNJQU05U3JJbHQ4c1YvK2dUZXdpZit2UTF1MUw2YjJDSlIrcnczaVpNbHZYc1Rtak1QcDlFczhMWUtuTzJ5NXdDem0xWnplOWFEYTRqS0Z1NUxQeHBHd3pDckhJUFNBRE5nRXcyMTlkaFVaS0plQ1psQi9kSEYzU3NBMlFqOE1VbStwVlVHUE1SZE9sQ0ZLM1F6bUtwVDRkalhzc1prNjUrSTZFVDNxMVJmVFluRm9IUXZHS0xYZWZsMXZ5RmlyWDF4SEx4SEg4V1loYVJXZVk4MldXRml1Ritwb2NPYW0xT3lYYTEydjVvS0ZuZz09

### Confirm Email for client_case_test_20260814211103@example.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=8a763330-edaa-4f26-8a66-08defa2f6750&token=Q2ZESjhPNXpQSmRGK0NWT3Z5RWxYSTRjQmNJQU05U3JJbHQ4c1YvK2dUZXdpZit2UTF1MUw2YjJDSlIrcnczaVpNbHZYc1Rtak1QcDlFczhMWUtuTzJ5NXdDem0xWnplOWFEYTRqS0Z1NUxQeHBHd3pDckhJUFNBRE5nRXcyMTlkaFVaS0plQ1psQi9kSEYzU3NBMlFqOE1VbStwVlVHUE1SZE9sQ0ZLM1F6bUtwVDRkalhzc1prNjUrSTZFVDNxMVJmVFluRm9IUXZHS0xYZWZsMXZ5RmlyWDF4SEx4SEg4V1loYVJXZVk4MldXRml1Ritwb2NPYW0xT3lYYTEydjVvS0ZuZz09

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


### Login Client

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Password123!",
    "Email":  "client_case_test_20260814211103@example.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "8a763330-edaa-4f26-8a66-08defa2f6750",
                              "email":  "client_case_test_20260814211103@example.com",
                              "fullName":  "Test Client",
                              "role":  "Client",
                              "status":  "Unverified",
                              "rejectionReason":  null
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4YTc2MzMzMC1lZGFhLTRmMjYtOGE2Ni0wOGRlZmEyZjY3NTAiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjhhNzYzMzMwLWVkYWEtNGYyNi04YTY2LTA4ZGVmYTJmNjc1MCIsImVtYWlsIjoiY2xpZW50X2Nhc2VfdGVzdF8yMDI2MDgxNDIxMTEwM0BleGFtcGxlLmNvbSIsIm5hbWUiOiJUZXN0IENsaWVudCIsInNlY3VyaXR5X3N0YW1wIjoiVkJQSTRLMzU0MjZTNE1BTlZYMldESEVYM0VNS0NFWVkiLCJqdGkiOiI4Njc5ZWYyMS03ODkxLTRkOTMtYmQ0Ni1iMTUzNmZhOWMwZWYiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDbGllbnQiLCJuYmYiOjE3ODY3MzEwNjQsImV4cCI6MTc4NjczNDY2NCwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.EoLrJt56mKs0A7Em5XE3cdDypnvH80q3SGdOY8MFe34",
                 "expiresIn":  3600,
                 "refreshToken":  "sPHA4zFdlk8CCkDr5bJTCqGt11LwReL11pkoRnpN9G4GIkel7ZVbRphM4D8ZvQGKgDLz9uNRUQG9FzsxTHJxXQ==",
                 "refreshTokenExpiration":  "2026-08-21T18:11:04.3576902Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### Register Lawyer

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
`json
{
    "Email":  "lawyer_case_test_20260814211104@example.com",
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
                 "userId":  "6bf9633a-589d-4325-8a67-08defa2f6750",
                 "email":  "lawyer_case_test_20260814211104@example.com",
                 "fullName":  "Test Lawyer",
                 "role":  "Lawyer"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
``n---


Found confirmation URL for lawyer_case_test_20260814211104@example.com: http://localhost:5173/verify-email?userId=6bf9633a-589d-4325-8a67-08defa2f6750&token=Q2ZESjhPNXpQSmRGK0NWT3Z5RWxYSTRjQmNKQmVJSnBuMFhqMEVZV2tPN0hOZ3V2NG15N2gwNTZXSmdScGR3VFdiUkVwL1Y2dkQ5NjRTTk83YlR2ajNLWFpmMWg1TVlmck44TFR3b2F0RzNocmMzZ29FUnI3VHowbnNUNUJrOWtBR2V0a2VpcmxYWUhKUC93ZnJTR1ZmSGJmeC83cXYzRnNjRTc2ZTg2REM2a3ZQSldDQ2Q0WCswSkZQWkJtVnhrcWJ3Rit2Y1B2T2laQ01LUzhmakJFNVJ4MmovdGdzLzRJRXVCL1liQkFyWnBDZW1lcDlkazlnVXN0MXB5S0ljbWZiRlA3UT09

### Confirm Email for lawyer_case_test_20260814211104@example.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=6bf9633a-589d-4325-8a67-08defa2f6750&token=Q2ZESjhPNXpQSmRGK0NWT3Z5RWxYSTRjQmNKQmVJSnBuMFhqMEVZV2tPN0hOZ3V2NG15N2gwNTZXSmdScGR3VFdiUkVwL1Y2dkQ5NjRTTk83YlR2ajNLWFpmMWg1TVlmck44TFR3b2F0RzNocmMzZ29FUnI3VHowbnNUNUJrOWtBR2V0a2VpcmxYWUhKUC93ZnJTR1ZmSGJmeC83cXYzRnNjRTc2ZTg2REM2a3ZQSldDQ2Q0WCswSkZQWkJtVnhrcWJ3Rit2Y1B2T2laQ01LUzhmakJFNVJ4MmovdGdzLzRJRXVCL1liQkFyWnBDZW1lcDlkazlnVXN0MXB5S0ljbWZiRlA3UT09

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


### Login Lawyer

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Password123!",
    "Email":  "lawyer_case_test_20260814211104@example.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "6bf9633a-589d-4325-8a67-08defa2f6750",
                              "email":  "lawyer_case_test_20260814211104@example.com",
                              "fullName":  "Test Lawyer",
                              "role":  "Lawyer",
                              "status":  "Unverified",
                              "rejectionReason":  null
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2YmY5NjMzYS01ODlkLTQzMjUtOGE2Ny0wOGRlZmEyZjY3NTAiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjZiZjk2MzNhLTU4OWQtNDMyNS04YTY3LTA4ZGVmYTJmNjc1MCIsImVtYWlsIjoibGF3eWVyX2Nhc2VfdGVzdF8yMDI2MDgxNDIxMTEwNEBleGFtcGxlLmNvbSIsIm5hbWUiOiJUZXN0IExhd3llciIsInNlY3VyaXR5X3N0YW1wIjoiUkpUWVNKU0pKU0tRSzVISDVaSUZKNkZEUEFVRkNIQUUiLCJqdGkiOiJhMjc0NWM2MS1iZjQ3LTQ5YTEtOTNkZi03NjFhZTBlZjkyMWEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODY3MzEwNjUsImV4cCI6MTc4NjczNDY2NSwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.LPGrvMYh7FBbI3W4OSWjGIlk26kz0f4w4r5vCxuYXWM",
                 "expiresIn":  3600,
                 "refreshToken":  "q1i8k3wj5VoyRUxhReFaiqiS0EyQlopaNBiN+115ZnA3VvCbiCDL+bRuP/amRrPatwf4llmrvKQsODaJzncDSQ==",
                 "refreshTokenExpiration":  "2026-08-21T18:11:05.0927078Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### Setup - Complete Client Profile

**Request:** POST http://localhost:5049/api/clients/profile/complete

**Body:**
`json
{
    "NationalNumber":  "29001015236902",
    "DateOfBirth":  "1990-01-01",
    "PhoneNumber":  "+201011111111",
    "Gender":  1,
    "Address":  "Cairo"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "message":  "تم استكمال الملف الشخصي بنجاح.",
    "errors":  null,
    "statusCode":  200
}
``n---


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
    "NationalNumber":  "28501018542598",
    "Address":  "Cairo"
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
                              "id":  "e03cad7c-2b6f-453c-ab7c-08def602b5a0",
                              "email":  "admin@smartcourt.com",
                              "fullName":  "System Administrator",
                              "role":  "Admin",
                              "status":  "Active",
                              "rejectionReason":  null
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJlMDNjYWQ3Yy0yYjZmLTQ1M2MtYWI3Yy0wOGRlZjYwMmI1YTAiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImUwM2NhZDdjLTJiNmYtNDUzYy1hYjdjLTA4ZGVmNjAyYjVhMCIsImVtYWlsIjoiYWRtaW5Ac21hcnRjb3VydC5jb20iLCJuYW1lIjoiU3lzdGVtIEFkbWluaXN0cmF0b3IiLCJzZWN1cml0eV9zdGFtcCI6IlJPU0tHSVFTTzRXS1RWRTc0WklFUVdMU0dJWUdYN0RHIiwianRpIjoiNDMxZDU1ZGItYzIzMS00YjAxLTgwNmEtODM4YjVlMjUzNmE2IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJuYmYiOjE3ODY3MzEwNjUsImV4cCI6MTc4NjczNDY2NSwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.4pwY9zR5-EooI3uvf-lZpSRh26G7xfktD1AscLgCRNU",
                 "expiresIn":  3600,
                 "refreshToken":  "SXsJ4naIrCBaFcvjKSYQzSvRhS0JIRJfeSLPTNA6sl8eZfn9mH6SLKMjFJIXsF7U/ajcLotgiJmoxswgzjtUxQ==",
                 "refreshTokenExpiration":  "2026-08-21T18:11:05.7026374Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### Setup - Admin Approve Lawyer

**Request:** PATCH http://localhost:5049/api/admin/verifications/6bf9633a-589d-4325-8a67-08defa2f6750/approve-account

**Body:**
`json
{

}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "message":  "تم اعتماد بيانات الحساب بنجاح"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### Setup - Admin Approve Client

**Request:** PATCH http://localhost:5049/api/admin/verifications/8a763330-edaa-4f26-8a66-08defa2f6750/approve-account

**Body:**
`json
{

}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "message":  "تم اعتماد بيانات الحساب بنجاح"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### Setup - Re-Login Lawyer

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Password123!",
    "Email":  "lawyer_case_test_20260814211104@example.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "6bf9633a-589d-4325-8a67-08defa2f6750",
                              "email":  "lawyer_case_test_20260814211104@example.com",
                              "fullName":  "Test Lawyer",
                              "role":  "Lawyer",
                              "status":  "Active",
                              "rejectionReason":  null
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2YmY5NjMzYS01ODlkLTQzMjUtOGE2Ny0wOGRlZmEyZjY3NTAiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjZiZjk2MzNhLTU4OWQtNDMyNS04YTY3LTA4ZGVmYTJmNjc1MCIsImVtYWlsIjoibGF3eWVyX2Nhc2VfdGVzdF8yMDI2MDgxNDIxMTEwNEBleGFtcGxlLmNvbSIsIm5hbWUiOiJUZXN0IExhd3llciIsInNlY3VyaXR5X3N0YW1wIjoiTlRWQURDVkI2WDdPVVNCV0dOUkxFMzNaNlBTNkxEUEEiLCJqdGkiOiIwN2QxYTg4Ny0xYjU5LTRiZjgtODg4My05YjkzMDg3ZGFiNDQiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODY3MzEwNjYsImV4cCI6MTc4NjczNDY2NiwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.mzkdzHYvTB8Bn6m0mh7ch1UbWHCYqehzGNj3Pgy57wM",
                 "expiresIn":  3600,
                 "refreshToken":  "aSIa+G4FGy/accaSPSS9nyzRXZH4vUyovPL/YnUut4m5CxH9o1rIsBfcWj0EeO/4bvVrEDzY5K6qcjqLRfK/XA==",
                 "refreshTokenExpiration":  "2026-08-21T18:11:06.1024936Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### Setup - Re-Login Client

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Password123!",
    "Email":  "client_case_test_20260814211103@example.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "8a763330-edaa-4f26-8a66-08defa2f6750",
                              "email":  "client_case_test_20260814211103@example.com",
                              "fullName":  "Test Client",
                              "role":  "Client",
                              "status":  "Active",
                              "rejectionReason":  null
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4YTc2MzMzMC1lZGFhLTRmMjYtOGE2Ni0wOGRlZmEyZjY3NTAiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjhhNzYzMzMwLWVkYWEtNGYyNi04YTY2LTA4ZGVmYTJmNjc1MCIsImVtYWlsIjoiY2xpZW50X2Nhc2VfdGVzdF8yMDI2MDgxNDIxMTEwM0BleGFtcGxlLmNvbSIsIm5hbWUiOiJUZXN0IENsaWVudCIsInNlY3VyaXR5X3N0YW1wIjoiQVAySUJJM1BOVTRORVRET0FRTDdCUVdURFA3QkRIMjQiLCJqdGkiOiIyYzNiYjZlNC04MDhhLTRkNmMtOTMyYi02MDJjMGU2NGQ4NWQiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDbGllbnQiLCJuYmYiOjE3ODY3MzEwNjYsImV4cCI6MTc4NjczNDY2NiwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.XhExkCxn0z0rEiVH6GIBMKledBCErZcEkLLyElbUc6E",
                 "expiresIn":  3600,
                 "refreshToken":  "ulO1nHQFDCxTIaj07+2xwWUKShhH53gEU3Jlw49gi501a1oo7Z/cB9nNRzM6MxXsaiwzb6UNUNevaorQngCclA==",
                 "refreshTokenExpiration":  "2026-08-21T18:11:06.3065249Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### Create Case (400 Validation Error)

**Request:** POST http://localhost:5049/api/Case

**Body:**
(multipart/form-data)

**Response Status:** 400

**Response Body:**
`json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "Title":  [
                                 "The Title field is required.",
                                 "Case title can\u0027t be empty"
                             ],
                   "Description":  [
                                       "The Description field is required.",
                                       "Case description can\u0027t be empty"
                                   ]
               },
    "traceId":  "00-4a038ccdb4c5b07838f96f5eb78f3288-ce7fb45eec97ca4f-00"
}
``n---


### Create Case (Valid Success)

**Request:** POST http://localhost:5049/api/Case

**Body:**
(multipart/form-data)

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "caseId":  "0d8329e6-bb6e-4c55-86f5-24630ddf2a45",
                 "failedDocuments":  [

                                     ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  201
}
``n---


### Get Case By ID (Before Review)

**Request:** GET http://localhost:5049/api/Case/0d8329e6-bb6e-4c55-86f5-24630ddf2a45

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "0d8329e6-bb6e-4c55-86f5-24630ddf2a45",
                 "clientId":  "8a763330-edaa-4f26-8a66-08defa2f6750",
                 "lawyerId":  null,
                 "lastReviewId":  null,
                 "chatId":  null,
                 "title":  "Valid Case Title",
                 "description":  "Detailed description of the case for testing.",
                 "governorate":  "Cairo",
                 "city":  "Maadi",
                 "status":  "Submitted",
                 "createdAt":  "2026-08-14T18:11:06.7626218",
                 "documents":  [
                                   {
                                       "id":  "3ebc36b8-470e-4eb6-91fc-08defa2f6915",
                                       "fileName":  "dummy_case.pdf",
                                       "fileUrl":  "8a763330-edaa-4f26-8a66-08defa2f6750/case-documents/8865bdf5-c44d-4e1f-a3fc-84bb815ddc90.pdf",
                                       "contentType":  "application/pdf"
                                   }
                               ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### Get Case By ID (404 Not Found)

**Request:** GET http://localhost:5049/api/Case/806babfa-2e61-4a48-921e-8fc408dfa773

**Response Status:** 404

**Response Body:**
`json
{
    "success":  false,
    "data":  null,
    "message":  null,
    "errors":  [
                   "Case not found"
               ],
    "statusCode":  404
}
``n---


### Get All Cases

**Request:** GET http://localhost:5049/api/Case

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  [
                 {
                     "id":  "0d8329e6-bb6e-4c55-86f5-24630ddf2a45",
                     "title":  "Valid Case Title",
                     "status":  "Submitted",
                     "createdAt":  "2026-08-14T18:11:06.7626218",
                     "documentCount":  1,
                     "lawyerId":  null,
                     "lastReviewId":  null,
                     "chatId":  null
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### Update Case (Valid Success)

**Request:** PUT http://localhost:5049/api/Case/0d8329e6-bb6e-4c55-86f5-24630ddf2a45

**Body:**
(multipart/form-data)

**Response Status:** 405

**Response Body:** (Empty)
---


### Create Case to Delete

**Request:** POST http://localhost:5049/api/Case

**Body:**
(multipart/form-data)

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "caseId":  "55aa64aa-e2af-4dc3-8887-046a7a51ff60",
                 "failedDocuments":  [

                                     ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  201
}
``n---


### Delete Case (Success)

**Request:** DELETE http://localhost:5049/api/Case/55aa64aa-e2af-4dc3-8887-046a7a51ff60

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "message":  null,
    "errors":  null,
    "statusCode":  200
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
    "success":  true,
    "data":  {
                 "caseId":  "3f7387be-8e1f-4306-86d0-5add151d2230",
                 "failedDocuments":  [

                                     ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  201
}
``n---


### Review Case (AI Request)

**Request:** POST http://localhost:5049/api/cases/0d8329e6-bb6e-4c55-86f5-24630ddf2a45/review

**Body:**
`json
{

}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "066d664f-dd36-4465-9809-70d055ebb1b4",
                 "caseId":  "0d8329e6-bb6e-4c55-86f5-24630ddf2a45",
                 "isLatest":  true,
                 "createdAt":  "2026-08-14T18:11:34.4166571Z",
                 "reviewPoints":  [
                                      {
                                          "id":  "92f69758-7be0-453c-9275-2281300a3b90",
                                          "description":  "يُعد وجود عقد مكتوب موقَّع بين الطرفين — ولو كان مرفقًا كملف PDF باسم \u0027dummy_case.pdf\u0027 — عنصرًا قويًّا جدًّا في الدعوى وفقًا للمادة (107) من القانون المدني المصري، التي تنص على أن العقد شريعة المتعاقدين ما لم يخالف نصًّا في القانون أو نظامًا عامًّا أو آدابًا. كما أن وجود عنوان محدد في محافظة القاهرة ومدينة المعادي يعزز الاختصاص المحلي للمحكمة الابتدائية في جنوب القاهرة وفقًا للمادة (25) من قانون المرافعات رقم 13 لسنة 1968، ما يُقلل احتمال الطعن بالاختصاص النوعي أو المكاني. إضافةً إلى ذلك، فإن صيغة الوصف التفصيلي للقضية توحي بوجود سرد زمني منطقي وعناصر فاعلة في العلاقة التعاقدية (مثل التزام، إخلال، ضرر)، مما يشكل أساسًا متينًا لرفع دعوى مدنية أو تجارية وفقًا لأحكام المواد (219–221) من القانون المدني.",
                                          "type":  "Strength"
                                      },
                                      {
                                          "id":  "7eb7f4da-5e5f-4bbe-9b9b-fec1b7ce96b7",
                                          "description":  "يُعد غياب أي إشارة إلى إنذار رسمي مُسلَّم على يد محضر (إنذار قانوني وفق المادة 214 من قانون المرافعات) نقطة ضعف جوهرية، إذ لا يُعتد غالبًا بالإخلال بالالتزام إلا بعد توجيه إنذار كتابي رسمي يُثبت علم المدين بالتأخير ويمنحه مهلة معقولة، خاصة في الدعاوى المتعلقة بالتنفيذ أو التعويض عن التأخير. كما أن عدم تحديد تاريخ بدء العلاقة التعاقدية أو تاريخ الإخلال أو تاريخ تقديم الطلب يجعل من الصعب إثبات سريان المدة الزمنية للدعوى أو استحقاق الفوائد أو التعويضات وفقًا للمادة (226) من القانون المدني. كذلك، غياب بيانات هوية الأطراف (أرقام البطاقات الوطنية أو السجل التجاري في حالة الشركات) يُضعف إمكانية التحقق من الصفة والصفة القانونية أمام المحكمة، وقد يُستند إليه في طلب رفض الدعوى شكليًّا وفق المادة (100) من قانون المرافعات.",
                                          "type":  "Weakness"
                                      },
                                      {
                                          "id":  "c85b69a8-7a1f-4b1c-99e8-306c5c7f2b7f",
                                          "description":  "يجب إعادة هيكلة وصف القضية ليشمل: (أ) اسم الطرفين الكامل مع ذكر نوع كل منهما (فرد/شركة) وبيانات هويته القانونية (رقم البطاقة أو السجل التجاري)، (ب) تاريخ توقيع العقد وتاريخ سريانه وتاريخ انتهاء مدته، (ج) وصف دقيق للالتزام المُخلَّ به (مثال: \u0027عدم تسليم البضاعة في الموعد المتفق عليه بتاريخ ١٥/٠٣/٢٠٢٤\u0027)، (د) تاريخ الإنذار الرسمي المسلَّم على يد محضر مع رقم الإيصال واسم المحضر، (هـ) حساب دقيق للمبلغ المطالب به مقسَّمًا إلى أصل الدين + فوائد تأخيرية (حسب الاتفاق أو ٤٪ سنويًّا وفق المادة ٢٢٦ مدني) + تعويض مادي وأدبي مُبرَّر، (و) ذكر واضح للضرر الواقع (مثل خسارة صفقة، تكاليف إضافية، توقف نشاط تجاري) مع ربطه مباشرةً بالإخلال.",
                                          "type":  "Suggestion"
                                      },
                                      {
                                          "id":  "c53ebdcf-04cd-445d-8fb8-390a898e150f",
                                          "description":  "المعلومات الناقصة تشمل: (١) الأسماء الكاملة والأرقام القومية أو السجلات التجارية للأطراف، (٢) التاريخ الدقيق لتوقيع العقد ولإخلال الطرف الآخر بالتزامه، (٣) مدة المهلة الممنوحة بعد الإنذار قبل رفع الدعوى، (٤) المبلغ المالي المطالب به مع تفصيله إلى أصل الدين وفوائد وتعويضات، (٥) وصف دقيق للضرر الواقع ومصدره المباشر من الإخلال، (٦) اسم المحكمة التي يُراد رفع الدعوى أمامها (مثل محكمة جنوب القاهرة الابتدائية)، (٧) نوع الدعوى المطلوبة (دعوى تنفيذ، دعوى تعويض، دعوى فسخ عقد).",
                                          "type":  "MissingCaseInfo"
                                      },
                                      {
                                          "id":  "69f02c5c-bbea-4412-8c0d-58372dc2b3c4",
                                          "description":  "المستندات المفقودة الضرورية لرفع الدعوى وفق القانون المصري تشمل: (١) صورة شخصية من بطاقة الرقم القومي سارية لكل طرف طبيعي، أو صورة من السجل التجاري وشهادة ملكية المشروع للطرف التجاري، (٢) نسخة أصلية أو مصدَّقة من العقد المكتوب الموقع من الطرفين، (٣) إشعار إنذار رسمي مسلَّم على يد محضر مع إيصال تسليم مُوثَّق برقم وتاريخ، (٤) إثباتات الدفع أو التوريد (مثل إيصالات بنكية، فواتير ضريبية، سندات قبض)، (٥) مستندات إثبات الضرر (مثل عقود سابقة ملغاة، فواتير إضافية، تقارير فنية أو محاسبية)، (٦) في حالة الدعاوى التجارية: شهادة من الغرفة التجارية تفيد بمزاولة النشاط، (٧) توكيل رسمي في حال توكيل محامٍ أو ممثل قانوني.",
                                          "type":  "MissingCaseDoc"
                                      }
                                  ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### Get Latest Review

**Request:** GET http://localhost:5049/api/cases/0d8329e6-bb6e-4c55-86f5-24630ddf2a45/reviews/latest

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "066d664f-dd36-4465-9809-70d055ebb1b4",
                 "caseId":  "0d8329e6-bb6e-4c55-86f5-24630ddf2a45",
                 "isLatest":  true,
                 "createdAt":  "2026-08-14T18:11:34.4166571",
                 "reviewPoints":  [
                                      {
                                          "id":  "92f69758-7be0-453c-9275-2281300a3b90",
                                          "description":  "يُعد وجود عقد مكتوب موقَّع بين الطرفين — ولو كان مرفقًا كملف PDF باسم \u0027dummy_case.pdf\u0027 — عنصرًا قويًّا جدًّا في الدعوى وفقًا للمادة (107) من القانون المدني المصري، التي تنص على أن العقد شريعة المتعاقدين ما لم يخالف نصًّا في القانون أو نظامًا عامًّا أو آدابًا. كما أن وجود عنوان محدد في محافظة القاهرة ومدينة المعادي يعزز الاختصاص المحلي للمحكمة الابتدائية في جنوب القاهرة وفقًا للمادة (25) من قانون المرافعات رقم 13 لسنة 1968، ما يُقلل احتمال الطعن بالاختصاص النوعي أو المكاني. إضافةً إلى ذلك، فإن صيغة الوصف التفصيلي للقضية توحي بوجود سرد زمني منطقي وعناصر فاعلة في العلاقة التعاقدية (مثل التزام، إخلال، ضرر)، مما يشكل أساسًا متينًا لرفع دعوى مدنية أو تجارية وفقًا لأحكام المواد (219–221) من القانون المدني.",
                                          "type":  "Strength"
                                      },
                                      {
                                          "id":  "c85b69a8-7a1f-4b1c-99e8-306c5c7f2b7f",
                                          "description":  "يجب إعادة هيكلة وصف القضية ليشمل: (أ) اسم الطرفين الكامل مع ذكر نوع كل منهما (فرد/شركة) وبيانات هويته القانونية (رقم البطاقة أو السجل التجاري)، (ب) تاريخ توقيع العقد وتاريخ سريانه وتاريخ انتهاء مدته، (ج) وصف دقيق للالتزام المُخلَّ به (مثال: \u0027عدم تسليم البضاعة في الموعد المتفق عليه بتاريخ ١٥/٠٣/٢٠٢٤\u0027)، (د) تاريخ الإنذار الرسمي المسلَّم على يد محضر مع رقم الإيصال واسم المحضر، (هـ) حساب دقيق للمبلغ المطالب به مقسَّمًا إلى أصل الدين + فوائد تأخيرية (حسب الاتفاق أو ٤٪ سنويًّا وفق المادة ٢٢٦ مدني) + تعويض مادي وأدبي مُبرَّر، (و) ذكر واضح للضرر الواقع (مثل خسارة صفقة، تكاليف إضافية، توقف نشاط تجاري) مع ربطه مباشرةً بالإخلال.",
                                          "type":  "Suggestion"
                                      },
                                      {
                                          "id":  "c53ebdcf-04cd-445d-8fb8-390a898e150f",
                                          "description":  "المعلومات الناقصة تشمل: (١) الأسماء الكاملة والأرقام القومية أو السجلات التجارية للأطراف، (٢) التاريخ الدقيق لتوقيع العقد ولإخلال الطرف الآخر بالتزامه، (٣) مدة المهلة الممنوحة بعد الإنذار قبل رفع الدعوى، (٤) المبلغ المالي المطالب به مع تفصيله إلى أصل الدين وفوائد وتعويضات، (٥) وصف دقيق للضرر الواقع ومصدره المباشر من الإخلال، (٦) اسم المحكمة التي يُراد رفع الدعوى أمامها (مثل محكمة جنوب القاهرة الابتدائية)، (٧) نوع الدعوى المطلوبة (دعوى تنفيذ، دعوى تعويض، دعوى فسخ عقد).",
                                          "type":  "MissingCaseInfo"
                                      },
                                      {
                                          "id":  "69f02c5c-bbea-4412-8c0d-58372dc2b3c4",
                                          "description":  "المستندات المفقودة الضرورية لرفع الدعوى وفق القانون المصري تشمل: (١) صورة شخصية من بطاقة الرقم القومي سارية لكل طرف طبيعي، أو صورة من السجل التجاري وشهادة ملكية المشروع للطرف التجاري، (٢) نسخة أصلية أو مصدَّقة من العقد المكتوب الموقع من الطرفين، (٣) إشعار إنذار رسمي مسلَّم على يد محضر مع إيصال تسليم مُوثَّق برقم وتاريخ، (٤) إثباتات الدفع أو التوريد (مثل إيصالات بنكية، فواتير ضريبية، سندات قبض)، (٥) مستندات إثبات الضرر (مثل عقود سابقة ملغاة، فواتير إضافية، تقارير فنية أو محاسبية)، (٦) في حالة الدعاوى التجارية: شهادة من الغرفة التجارية تفيد بمزاولة النشاط، (٧) توكيل رسمي في حال توكيل محامٍ أو ممثل قانوني.",
                                          "type":  "MissingCaseDoc"
                                      },
                                      {
                                          "id":  "7eb7f4da-5e5f-4bbe-9b9b-fec1b7ce96b7",
                                          "description":  "يُعد غياب أي إشارة إلى إنذار رسمي مُسلَّم على يد محضر (إنذار قانوني وفق المادة 214 من قانون المرافعات) نقطة ضعف جوهرية، إذ لا يُعتد غالبًا بالإخلال بالالتزام إلا بعد توجيه إنذار كتابي رسمي يُثبت علم المدين بالتأخير ويمنحه مهلة معقولة، خاصة في الدعاوى المتعلقة بالتنفيذ أو التعويض عن التأخير. كما أن عدم تحديد تاريخ بدء العلاقة التعاقدية أو تاريخ الإخلال أو تاريخ تقديم الطلب يجعل من الصعب إثبات سريان المدة الزمنية للدعوى أو استحقاق الفوائد أو التعويضات وفقًا للمادة (226) من القانون المدني. كذلك، غياب بيانات هوية الأطراف (أرقام البطاقات الوطنية أو السجل التجاري في حالة الشركات) يُضعف إمكانية التحقق من الصفة والصفة القانونية أمام المحكمة، وقد يُستند إليه في طلب رفض الدعوى شكليًّا وفق المادة (100) من قانون المرافعات.",
                                          "type":  "Weakness"
                                      }
                                  ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### Get Case By ID (After Review)

**Request:** GET http://localhost:5049/api/Case/0d8329e6-bb6e-4c55-86f5-24630ddf2a45

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "0d8329e6-bb6e-4c55-86f5-24630ddf2a45",
                 "clientId":  "8a763330-edaa-4f26-8a66-08defa2f6750",
                 "lawyerId":  null,
                 "lastReviewId":  "066d664f-dd36-4465-9809-70d055ebb1b4",
                 "chatId":  null,
                 "title":  "Valid Case Title",
                 "description":  "Detailed description of the case for testing.",
                 "governorate":  "Cairo",
                 "city":  "Maadi",
                 "status":  "Reviewed",
                 "createdAt":  "2026-08-14T18:11:06.7626218",
                 "documents":  [
                                   {
                                       "id":  "3ebc36b8-470e-4eb6-91fc-08defa2f6915",
                                       "fileName":  "dummy_case.pdf",
                                       "fileUrl":  "8a763330-edaa-4f26-8a66-08defa2f6750/case-documents/8865bdf5-c44d-4e1f-a3fc-84bb815ddc90.pdf",
                                       "contentType":  "application/pdf"
                                   }
                               ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### Get All Cases (After Review)

**Request:** GET http://localhost:5049/api/Case

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  [
                 {
                     "id":  "0d8329e6-bb6e-4c55-86f5-24630ddf2a45",
                     "title":  "Valid Case Title",
                     "status":  "Reviewed",
                     "createdAt":  "2026-08-14T18:11:06.7626218",
                     "documentCount":  1,
                     "lawyerId":  null,
                     "lastReviewId":  "066d664f-dd36-4465-9809-70d055ebb1b4",
                     "chatId":  null
                 },
                 {
                     "id":  "3f7387be-8e1f-4306-86d0-5add151d2230",
                     "title":  "\u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e \u003cscript\u003ealert(\u0027XSS\u0027)\u003c/script\u003e ",
                     "status":  "Submitted",
                     "createdAt":  "2026-08-14T18:11:07.7319352",
                     "documentCount":  0,
                     "lawyerId":  null,
                     "lastReviewId":  null,
                     "chatId":  null
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### Finalize Case (Transition to Matched)

**Request:** POST http://localhost:5049/api/Case/0d8329e6-bb6e-4c55-86f5-24630ddf2a45/finalize

**Body:**
`json
{

}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "caseId":  "0d8329e6-bb6e-4c55-86f5-24630ddf2a45",
                 "totalEligibleLawyers":  0,
                 "pageNumber":  1,
                 "pageSize":  10,
                 "totalPages":  0,
                 "hasNextPage":  false,
                 "hasPreviousPage":  false,
                 "recommendations":  [

                                     ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### Create Proposal (Client to Lawyer)

**Request:** POST http://localhost:5049/api/proposals

**Body:**
`json
{
    "Message":  "I would like to hire you for this case.",
    "LegalCaseId":  "0d8329e6-bb6e-4c55-86f5-24630ddf2a45",
    "LawyerUserId":  "6bf9633a-589d-4325-8a67-08defa2f6750"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "6be5b304-c480-426a-83eb-01513e6a0166",
                 "legalCaseId":  "0d8329e6-bb6e-4c55-86f5-24630ddf2a45",
                 "caseTitle":  "Valid Case Title",
                 "clientUserId":  "8a763330-edaa-4f26-8a66-08defa2f6750",
                 "clientName":  "Test Client",
                 "lawyerUserId":  "6bf9633a-589d-4325-8a67-08defa2f6750",
                 "lawyerName":  "Test Lawyer",
                 "message":  "I would like to hire you for this case.",
                 "status":  "Pending",
                 "decisionReason":  null,
                 "caseStatus":  "Matched",
                 "assignedLawyerUserId":  null,
                 "isAssignedLawyer":  false,
                 "contractId":  null,
                 "contractStatus":  null,
                 "conversationId":  null,
                 "conversationStatus":  null,
                 "canChat":  false,
                 "permittedActions":  [
                                          "Cancel"
                                      ],
                 "createdAt":  "2026-08-14T18:11:36.9187412",
                 "respondedAt":  null,
                 "updatedAt":  "2026-08-14T18:11:36.9187412",
                 "expiresAt":  "2026-08-17T18:11:36.9187412",
                 "closedAt":  null,
                 "closedByUserId":  null
             },
    "message":  null,
    "errors":  null,
    "statusCode":  201
}
``n---


### Lawyer Get Proposals

**Request:** GET http://localhost:5049/api/proposals

**Response Status:** 405

**Response Body:** (Empty)
---


### Lawyer Accepts Proposal

**Request:** POST http://localhost:5049/api/proposals/6be5b304-c480-426a-83eb-01513e6a0166/accept

**Body:**
`json
{

}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "6be5b304-c480-426a-83eb-01513e6a0166",
                 "legalCaseId":  "0d8329e6-bb6e-4c55-86f5-24630ddf2a45",
                 "caseTitle":  "Valid Case Title",
                 "clientUserId":  "8a763330-edaa-4f26-8a66-08defa2f6750",
                 "clientName":  "Test Client",
                 "lawyerUserId":  "6bf9633a-589d-4325-8a67-08defa2f6750",
                 "lawyerName":  "Test Lawyer",
                 "message":  "I would like to hire you for this case.",
                 "status":  "Accepted",
                 "decisionReason":  null,
                 "caseStatus":  "Matched",
                 "assignedLawyerUserId":  null,
                 "isAssignedLawyer":  false,
                 "contractId":  null,
                 "contractStatus":  null,
                 "conversationId":  "4d63289e-9c06-455f-943f-fd8db27ab212",
                 "conversationStatus":  "Open",
                 "canChat":  true,
                 "permittedActions":  [
                                          "OpenChat",
                                          "TerminateProposal",
                                          "CreateContract"
                                      ],
                 "createdAt":  "2026-08-14T18:11:36.9187412",
                 "respondedAt":  "2026-08-14T18:11:37.4507655",
                 "updatedAt":  "2026-08-14T18:11:37.4507655",
                 "expiresAt":  "2026-08-17T18:11:36.9187412",
                 "closedAt":  null,
                 "closedByUserId":  null
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### Get Case By ID (After Proposal Acceptance)

**Request:** GET http://localhost:5049/api/Case/0d8329e6-bb6e-4c55-86f5-24630ddf2a45

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "0d8329e6-bb6e-4c55-86f5-24630ddf2a45",
                 "clientId":  "8a763330-edaa-4f26-8a66-08defa2f6750",
                 "lawyerId":  null,
                 "lastReviewId":  "066d664f-dd36-4465-9809-70d055ebb1b4",
                 "chatId":  null,
                 "title":  "Valid Case Title",
                 "description":  "Detailed description of the case for testing.",
                 "governorate":  "Cairo",
                 "city":  "Maadi",
                 "status":  "Matched",
                 "createdAt":  "2026-08-14T18:11:06.7626218",
                 "documents":  [
                                   {
                                       "id":  "3ebc36b8-470e-4eb6-91fc-08defa2f6915",
                                       "fileName":  "dummy_case.pdf",
                                       "fileUrl":  "8a763330-edaa-4f26-8a66-08defa2f6750/case-documents/8865bdf5-c44d-4e1f-a3fc-84bb815ddc90.pdf",
                                       "contentType":  "application/pdf"
                                   }
                               ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---



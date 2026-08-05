# Authentication Flow Test Report

### 1. Register Client - Missing FullName

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
    "Email":  "client_auth_1055316627@test.com",
    "ConfirmPassword":  "Password123!",
    "Password":  "Password123!"
}
``n
**Response Status:** 400

**Response Body:**
`json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "FullName":  [
                                    "The FullName field is required.",
                                    "الاسم الكامل مطلوب."
                                ]
               },
    "traceId":  "00-6431f0234a45678b69abad068f893c61-be601ece7e4c2e47-00"
}
``n---


### 2. Register Client - Missing Email

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
    "FullName":  "Test Client",
    "ConfirmPassword":  "Password123!",
    "Password":  "Password123!"
}
``n
**Response Status:** 400

**Response Body:**
`json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "Email":  [
                                 "The Email field is required.",
                                 "البريد الإلكتروني مطلوب."
                             ]
               },
    "traceId":  "00-acff9111757234437efc33dc609c106e-2cace485f16cbfbc-00"
}
``n---


### 3. Register Client - Invalid Email Format

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
    "Email":  "invalid_email",
    "FullName":  "Test Client",
    "ConfirmPassword":  "Password123!",
    "Password":  "Password123!"
}
``n
**Response Status:** 400

**Response Body:**
`json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "Email":  [
                                 "البريد الإلكتروني غير صالح."
                             ]
               },
    "traceId":  "00-221a1193ae72a8070adc3c15ab142355-8080c2b0cc577b79-00"
}
``n---


### 4. Register Client - Weak Password

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
    "Email":  "client_auth_1055316627@test.com",
    "FullName":  "Test Client",
    "ConfirmPassword":  "password",
    "Password":  "password"
}
``n
**Response Status:** 400

**Response Body:**
`json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "Password":  [
                                    "كلمة المرور يجب أن تحتوي على حرف كبير وحرف صغير ورقم."
                                ]
               },
    "traceId":  "00-225d9b9ec56bbb8f6520daa515e2d91d-6dd823cdf0583ab1-00"
}
``n---


### 5. Register Client - Mismatched ConfirmPassword

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
    "Email":  "client_auth_1055316627@test.com",
    "FullName":  "Test Client",
    "ConfirmPassword":  "Password1234!",
    "Password":  "Password123!"
}
``n
**Response Status:** 400

**Response Body:**
`json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "ConfirmPassword":  [
                                           "تأكيد كلمة المرور غير مطابق."
                                       ]
               },
    "traceId":  "00-91d651091ce5108114c0fc31e14b600c-e51180bb6d645c6b-00"
}
``n---


### 6. Register Client - Valid Data

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
    "Email":  "client_auth_1055316627@test.com",
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
                 "userId":  "63b8cce2-b926-45c1-810f-08def2e76edd",
                 "email":  "client_auth_1055316627@test.com",
                 "fullName":  "Test Client",
                 "role":  "Client"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
``n---


### 7. Login Client - Unconfirmed Email

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Password123!",
    "Email":  "client_auth_1055316627@test.com"
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


Found confirmation URL for client_auth_1055316627@test.com: http://localhost:5173/verify-email?userId=63b8cce2-b926-45c1-810f-08def2e76edd&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIvNlJablgxNmdhNXJXUDJ3cXB6aHdMeXgzQjZhQ2pQUjBSTkxtRlRuVUxhMDBWUlpSbHpJVE5hdWRWSE5sT1V2MzlEUGYzRVltT1k0UENKbWwvOTlwRUR5WG5uaVBzVi9qUFBPUTJpZVdBUE1KL2NEeEkrZEhpOFV3MjdBRDQrTk55NE85YUZVR1VLdmhzWUJQQ1FZUURhZTh6ek1YVWg3cXBlYVRMYzZOMXhCWEdTa2xBVkNqV3B5WWgwckZNZXpxMGdCUmZwVERlQ1N2RFE3cUZ2YTlSaW5iQW9RK0k2WllnS2dscWxiaUZkUT09

### Confirm Email for client_auth_1055316627@test.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=63b8cce2-b926-45c1-810f-08def2e76edd&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIvNlJablgxNmdhNXJXUDJ3cXB6aHdMeXgzQjZhQ2pQUjBSTkxtRlRuVUxhMDBWUlpSbHpJVE5hdWRWSE5sT1V2MzlEUGYzRVltT1k0UENKbWwvOTlwRUR5WG5uaVBzVi9qUFBPUTJpZVdBUE1KL2NEeEkrZEhpOFV3MjdBRDQrTk55NE85YUZVR1VLdmhzWUJQQ1FZUURhZTh6ek1YVWg3cXBlYVRMYzZOMXhCWEdTa2xBVkNqV3B5WWgwckZNZXpxMGdCUmZwVERlQ1N2RFE3cUZ2YTlSaW5iQW9RK0k2WllnS2dscWxiaUZkUT09

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


### 9. Login Client - Confirmed Email

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Password123!",
    "Email":  "client_auth_1055316627@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "63b8cce2-b926-45c1-810f-08def2e76edd",
                              "email":  "client_auth_1055316627@test.com",
                              "fullName":  "Test Client",
                              "role":  "Client"
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2M2I4Y2NlMi1iOTI2LTQ1YzEtODEwZi0wOGRlZjJlNzZlZGQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjYzYjhjY2UyLWI5MjYtNDVjMS04MTBmLTA4ZGVmMmU3NmVkZCIsImVtYWlsIjoiY2xpZW50X2F1dGhfMTA1NTMxNjYyN0B0ZXN0LmNvbSIsIm5hbWUiOiJUZXN0IENsaWVudCIsInNlY3VyaXR5X3N0YW1wIjoiV0k3NzJONUM2Mk9UR1RXWVdKMkpXR1JNMkJQSVZDT0MiLCJqdGkiOiJlNTljY2ZiYS1kMjY0LTRmN2QtOTQxYy1jNDI3YWUxZDNkNDciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDbGllbnQiLCJuYmYiOjE3ODU5MzE1MjUsImV4cCI6MTc4NTkzNTEyNSwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.IKk6997fU3NnT2dBPnJVHOLTUP5z63DRzEGZiSqhwBE",
                 "expiresIn":  3600,
                 "refreshToken":  "L2nBAgrqhEZdhoR0kvqFthVvkV8tAShNZ7dzN+T2D2KuktY22FPLiu2HxT6s8JPWsQy89ej1XbnCqqyv1prTCA==",
                 "refreshTokenExpiration":  "2026-08-12T12:05:25.9240427Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---



# Quota and Token Bundles E2E Test Report


### 0a. Setup - Login Admin

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Admin@123",
    "Email":  "moatazmohammed2392003@gmail.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "47783598-7dc3-41cc-8150-08dee91d7c8f",
                              "email":  "moatazmohammed2392003@gmail.com",
                              "fullName":  "Moataz Mohammed",
                              "role":  "Admin",
                              "status":  "Active",
                              "rejectionReason":  null
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0Nzc4MzU5OC03ZGMzLTQxY2MtODE1MC0wOGRlZTkxZDdjOGYiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjQ3NzgzNTk4LTdkYzMtNDFjYy04MTUwLTA4ZGVlOTFkN2M4ZiIsImVtYWlsIjoibW9hdGF6bW9oYW1tZWQyMzkyMDAzQGdtYWlsLmNvbSIsIm5hbWUiOiJNb2F0YXogTW9oYW1tZWQiLCJzZWN1cml0eV9zdGFtcCI6IlRHREFXSFdVUEFCRjVMQTU1N0ZMNkdGS1RKQUhMWFI0IiwianRpIjoiZDRkMzVjMjQtODYyZC00MjJlLWE4MmYtNDQ2OWEwM2ZhYTU3IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJuYmYiOjE3ODcwOTMxODAsImV4cCI6MTc4NzA5Njc4MCwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.2wET06HGqdvLx71k__7SQhHD71tSG1C7uFY03HlSMPg",
                 "expiresIn":  3600,
                 "refreshToken":  "3e31DKXToyd5O3QRwhXK2agI6e5EM08LdKtxQYrhfOArk3FNaPrj+G1qO1GbIWl52qH/TAhIxRpoe6l2gukmqA==",
                 "refreshTokenExpiration":  "2026-08-25T22:46:20.3719111+00:00"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 0b. Setup - Register Client

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
    "Email":  "client_quota_551095366@test.com",
    "FullName":  "Client QuotaTest",
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
                 "userId":  "cf577db1-cc33-4a9a-9913-08defd7a85e2",
                 "email":  "client_quota_551095366@test.com",
                 "fullName":  "Client QuotaTest",
                 "role":  "Client"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
``n---


Failed to read api_log.txt for client_quota_551095366@test.com

### 0c. Setup - Login Client

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Password123!",
    "Email":  "client_quota_551095366@test.com"
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



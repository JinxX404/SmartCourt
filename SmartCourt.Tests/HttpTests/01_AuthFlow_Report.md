# Authentication Flow Test Report

### 1. Register Client - Missing FullName

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
    "Email":  "client_auth_158804931@test.com",
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
    "traceId":  "00-a5c5f64b5bf2cbae929ae5430fcd1c8d-bf2b4e0a6d030d25-00"
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
    "traceId":  "00-3430b1941e7fa6ef96cf7c239e1be346-472a0c8f421fc3bc-00"
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
    "traceId":  "00-9e106f4d269c416d2f4563c0bc92f745-d065f5dd85cd967b-00"
}
``n---


### 4. Register Client - Weak Password

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
    "Email":  "client_auth_158804931@test.com",
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
    "traceId":  "00-6ba40c098c871bbfcfe7f3164be85e42-b97a97a25c5599fc-00"
}
``n---


### 5. Register Client - Mismatched ConfirmPassword

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
    "Email":  "client_auth_158804931@test.com",
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
    "traceId":  "00-49dabc9ced1d81fbf86781be9afed878-9b7570f11dab63a8-00"
}
``n---


### 6. Register Client - Valid Data

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
    "Email":  "client_auth_158804931@test.com",
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
                 "userId":  "2c4688f1-579a-4372-e602-08def30d5f49",
                 "email":  "client_auth_158804931@test.com",
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
    "Email":  "client_auth_158804931@test.com"
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


Found confirmation URL for client_auth_158804931@test.com: http://localhost:5173/verify-email?userId=2c4688f1-579a-4372-e602-08def30d5f49&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI4d3RLS0UyVmVraVFpUXpLS2lEQjRIci8wWHFmLzdsZVRsbWZoLzJtc2lmRHhCTFFlUXJ5QU5pOWlwSS9KcTRsU2wwNDVUM1djUDZMNEY0clh1YUFHUEhCNWNVcVBPeTFBR1ZURGpwU1VBV1ZqK010aFoxYTMwVkJPMjU2MWdUK05KODZ2UkpKMVBBanFES0l2aDBtaDZ2cHk3Q0dGUGZEb3ptOEJBbXp1T2JBTlk0V3lZaktKNXRSYkxJZ3dwRGFpS09VUjZKQm4ydnczSm9LMlZWcEs0Z1hUMXN1WGpmTkQ1bGV0QVp1VjlXUT09

### Confirm Email for client_auth_158804931@test.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=2c4688f1-579a-4372-e602-08def30d5f49&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI4d3RLS0UyVmVraVFpUXpLS2lEQjRIci8wWHFmLzdsZVRsbWZoLzJtc2lmRHhCTFFlUXJ5QU5pOWlwSS9KcTRsU2wwNDVUM1djUDZMNEY0clh1YUFHUEhCNWNVcVBPeTFBR1ZURGpwU1VBV1ZqK010aFoxYTMwVkJPMjU2MWdUK05KODZ2UkpKMVBBanFES0l2aDBtaDZ2cHk3Q0dGUGZEb3ptOEJBbXp1T2JBTlk0V3lZaktKNXRSYkxJZ3dwRGFpS09VUjZKQm4ydnczSm9LMlZWcEs0Z1hUMXN1WGpmTkQ1bGV0QVp1VjlXUT09

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
    "Email":  "client_auth_158804931@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "2c4688f1-579a-4372-e602-08def30d5f49",
                              "email":  "client_auth_158804931@test.com",
                              "fullName":  "Test Client",
                              "role":  "Client"
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyYzQ2ODhmMS01NzlhLTQzNzItZTYwMi0wOGRlZjMwZDVmNDkiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjJjNDY4OGYxLTU3OWEtNDM3Mi1lNjAyLTA4ZGVmMzBkNWY0OSIsImVtYWlsIjoiY2xpZW50X2F1dGhfMTU4ODA0OTMxQHRlc3QuY29tIiwibmFtZSI6IlRlc3QgQ2xpZW50Iiwic2VjdXJpdHlfc3RhbXAiOiJHSFBFMkNBTERPV1JDRERHNUo0Vlc3Mk1FQ0xOS0VYQSIsImp0aSI6Ijg5M2RjNTQwLTdlMTYtNDJhMy1hYWFkLTg1ZTI5MzYxMmJmNyIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNsaWVudCIsIm5iZiI6MTc4NTk0Njc5MywiZXhwIjoxNzg1OTUwMzkzLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.B_1y4QtcXTguJCxbnnWxxXXfMprbrjFnLjCi6FrozfA",
                 "expiresIn":  3600,
                 "refreshToken":  "3MkE9bSAq3odjn94H0qBLyLKS0I6PfBAz+PgkOvJuE/dTAqZpozUSWq0QJlio6OlQN8lsSpGJvramLPvDt+lrA==",
                 "refreshTokenExpiration":  "2026-08-12T16:19:53.315614Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---



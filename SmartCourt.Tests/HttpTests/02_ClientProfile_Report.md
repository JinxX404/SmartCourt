# Client Profile CRUD Test Report

### 0. Setup - Register Client

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
    "Email":  "client_crud_248866429@test.com",
    "FullName":  "Client Crud",
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
                 "userId":  "0ded073a-e12a-4890-4dd5-08def2fabdf7",
                 "email":  "client_crud_248866429@test.com",
                 "fullName":  "Client Crud",
                 "role":  "Client"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
``n---


Found confirmation URL for client_crud_248866429@test.com: http://localhost:5173/verify-email?userId=0ded073a-e12a-4890-4dd5-08def2fabdf7&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5aXRsWGt5cGJ4QzdCeXhuL0Z0K281OFZYOEF1SzNZU0NBbDIxVFB2TkROTWpJM1hsbldMSCt0VGhlRlNScXc2dlRTUmNIMCtJb1RSdEFFV0RzQ1RmeXZTckNOWFg5ZmdwcEZOTHJ1OUwybWtMVVhsMmU3OHBkTEdZN3pTWWsvYlFhNkdQeXJ4NFNLc0RqZmZIQnRuclV0Rk92ZHZ0ajU0UUg1di9HdGNKQVZJcGJVZnVqalVraFpySzZsT0FDaklWYm05RUJGcCtLZnl0ZnVpeWZEdGVHa3hTaXVLSjhybGRFdjRaWklhWE9wZz09

### Confirm Email for client_crud_248866429@test.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=0ded073a-e12a-4890-4dd5-08def2fabdf7&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5aXRsWGt5cGJ4QzdCeXhuL0Z0K281OFZYOEF1SzNZU0NBbDIxVFB2TkROTWpJM1hsbldMSCt0VGhlRlNScXc2dlRTUmNIMCtJb1RSdEFFV0RzQ1RmeXZTckNOWFg5ZmdwcEZOTHJ1OUwybWtMVVhsMmU3OHBkTEdZN3pTWWsvYlFhNkdQeXJ4NFNLc0RqZmZIQnRuclV0Rk92ZHZ0ajU0UUg1di9HdGNKQVZJcGJVZnVqalVraFpySzZsT0FDaklWYm05RUJGcCtLZnl0ZnVpeWZEdGVHa3hTaXVLSjhybGRFdjRaWklhWE9wZz09

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


### 0. Setup - Login Client

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Password123!",
    "Email":  "client_crud_248866429@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "0ded073a-e12a-4890-4dd5-08def2fabdf7",
                              "email":  "client_crud_248866429@test.com",
                              "fullName":  "Client Crud",
                              "role":  "Client"
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwZGVkMDczYS1lMTJhLTQ4OTAtNGRkNS0wOGRlZjJmYWJkZjciLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjBkZWQwNzNhLWUxMmEtNDg5MC00ZGQ1LTA4ZGVmMmZhYmRmNyIsImVtYWlsIjoiY2xpZW50X2NydWRfMjQ4ODY2NDI5QHRlc3QuY29tIiwibmFtZSI6IkNsaWVudCBDcnVkIiwic2VjdXJpdHlfc3RhbXAiOiI3UzNES1A1U05RVExOSEFUWUE3MkVRRk82SlgzNVo0TyIsImp0aSI6IjY0YmIyZTRjLTc1OWEtNGUyYi1iMzNmLThiZDVlZmM4NWRmZCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNsaWVudCIsIm5iZiI6MTc4NTkzODc5MSwiZXhwIjoxNzg1OTQyMzkxLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.eYBABL5CbGhfqf_LWqinqNCiw2i8HNvAy0MnfUwQ3Dg",
                 "expiresIn":  3600,
                 "refreshToken":  "aQ5zeSK60VYOIkG5IdRGNCX2q0ZR4ICYWtlG5w6Cv8IeKj6SrHhG9D5gr/WyfIX4Emwm+7XTlIxguiYfIUVZ0w==",
                 "refreshTokenExpiration":  "2026-08-12T14:06:31.8969325Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 1. Client Complete - Missing Phone & DOB

**Request:** POST http://localhost:5049/api/clients/profile/complete

**Body:**
`json
{
    "Gender":  1,
    "Address":  "Cairo"
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
                   "DateOfBirth":  [
                                       "تاريخ الميلاد مطلوب"
                                   ],
                   "PhoneNumber":  [
                                       "رقم الهاتف مطلوب",
                                       "رقم الهاتف يجب أن يكون بالتنسيق المصري +20XXXXXXXXXX"
                                   ]
               },
    "traceId":  "00-d49bdde9d3c0891a0abe4346820f8725-df04e86a2f1cc9ed-00"
}
``n---


### 2. Client Complete - Invalid Phone Format

**Request:** POST http://localhost:5049/api/clients/profile/complete

**Body:**
`json
{
    "DateOfBirth":  "1990-01-01",
    "PhoneNumber":  "123456789",
    "Gender":  1,
    "Address":  "Cairo"
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
                   "PhoneNumber":  [
                                       "رقم الهاتف يجب أن يكون بالتنسيق المصري +20XXXXXXXXXX"
                                   ]
               },
    "traceId":  "00-2a8e50a3319c769c9363f27c7820b48c-abace2bcebbf78d0-00"
}
``n---


### 3. Client Complete - Future Date of Birth

**Request:** POST http://localhost:5049/api/clients/profile/complete

**Body:**
`json
{
    "DateOfBirth":  "2026-08-06",
    "PhoneNumber":  "+201011111111",
    "Gender":  1,
    "Address":  "Cairo"
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
                   "DateOfBirth":  [
                                       "تاريخ الميلاد يجب أن يكون في الماضي"
                                   ]
               },
    "traceId":  "00-5c633f7bce85c666c6dfffd9db946e01-f63ee41f902a5cef-00"
}
``n---


### 4. Client Complete - Valid Data

**Request:** POST http://localhost:5049/api/clients/profile/complete

**Body:**
`json
{
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


### 5. Re-Login Client (Token Refresh)

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Password123!",
    "Email":  "client_crud_248866429@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "0ded073a-e12a-4890-4dd5-08def2fabdf7",
                              "email":  "client_crud_248866429@test.com",
                              "fullName":  "Client Crud",
                              "role":  "Client"
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwZGVkMDczYS1lMTJhLTQ4OTAtNGRkNS0wOGRlZjJmYWJkZjciLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjBkZWQwNzNhLWUxMmEtNDg5MC00ZGQ1LTA4ZGVmMmZhYmRmNyIsImVtYWlsIjoiY2xpZW50X2NydWRfMjQ4ODY2NDI5QHRlc3QuY29tIiwibmFtZSI6IkNsaWVudCBDcnVkIiwic2VjdXJpdHlfc3RhbXAiOiJQVE5XQk9RQ1hLU0RONVlIUjZNUUs1MlNINVFTUlE3NCIsImp0aSI6IjUxMjQ5ZGFjLWMzMDYtNGRmMC05YzBhLTBhYTU2YjliOTEzNCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNsaWVudCIsIm5iZiI6MTc4NTkzODc5MiwiZXhwIjoxNzg1OTQyMzkyLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.DWBq8hCKc0guimNNlCbJoX4tJY44uOETWpkGeEhbrkU",
                 "expiresIn":  3600,
                 "refreshToken":  "v25ChO4FZF/fu/ArDCd7SmqUjVTdkiqauaoncLzzgaA4pmhlmPOFvM4o7PTfRbYJayP7AsHuYSDnLjHKxntBgA==",
                 "refreshTokenExpiration":  "2026-08-12T14:06:32.6237215Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 6. Client GET Private Profile

**Request:** GET http://localhost:5049/api/clients/profile

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "0ded073a-e12a-4890-4dd5-08def2fabdf7",
                 "name":  "Client Crud",
                 "email":  "client_crud_248866429@test.com",
                 "phoneNumber":  "+201011111111",
                 "gender":  1,
                 "dateOfBirth":  "1990-01-01",
                 "address":  "Cairo",
                 "status":  "Active"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 7. Client Update - Invalid Phone Format

**Request:** PUT http://localhost:5049/api/clients/profile

**Body:**
`json
{
    "Address":  "Alexandria",
    "PhoneNumber":  "invalid_phone"
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
                   "PhoneNumber":  [
                                       "رقم الهاتف يجب أن يكون بالتنسيق المصري +20XXXXXXXXXX"
                                   ]
               },
    "traceId":  "00-e9094cc0cd9f56d96f66b18d6e56b366-793d879cff1f02a6-00"
}
``n---


### 8. Client Update - Valid Data

**Request:** PUT http://localhost:5049/api/clients/profile

**Body:**
`json
{
    "Address":  "Alexandria",
    "PhoneNumber":  "+201222222222"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "message":  "تم تحديث الملف الشخصي بنجاح.",
    "errors":  null,
    "statusCode":  200
}
``n---


### 8b. Re-Login Client (Token Refresh)

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Password123!",
    "Email":  "client_crud_248866429@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "0ded073a-e12a-4890-4dd5-08def2fabdf7",
                              "email":  "client_crud_248866429@test.com",
                              "fullName":  "Client Crud",
                              "role":  "Client"
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwZGVkMDczYS1lMTJhLTQ4OTAtNGRkNS0wOGRlZjJmYWJkZjciLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjBkZWQwNzNhLWUxMmEtNDg5MC00ZGQ1LTA4ZGVmMmZhYmRmNyIsImVtYWlsIjoiY2xpZW50X2NydWRfMjQ4ODY2NDI5QHRlc3QuY29tIiwibmFtZSI6IkNsaWVudCBDcnVkIiwic2VjdXJpdHlfc3RhbXAiOiJZN1c3VVlEMlBZNVVKNldWVjJHNllJSzY3WTdDNE9GQSIsImp0aSI6IjRmMjM5NzViLWIwY2EtNGMxNS1iYTJlLTQ5ZTZiMGM4ZThhMiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkNsaWVudCIsIm5iZiI6MTc4NTkzODc5MywiZXhwIjoxNzg1OTQyMzkzLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.mUCedAaf-0CAtPHteCWDuMoaxcQ3dY3MjsNuUVYcr7o",
                 "expiresIn":  3600,
                 "refreshToken":  "Vdi+UUeHFGSe9/kFl8iUKDrx+FIDcuXSHVveLfoxTahqyIj7Gp70kTbPAWrIbLC0Xzf4DPElNMJHl8MGLj1WEg==",
                 "refreshTokenExpiration":  "2026-08-12T14:06:33.266491Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 9. Client GET Private Profile (Verify Update)

**Request:** GET http://localhost:5049/api/clients/profile

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "0ded073a-e12a-4890-4dd5-08def2fabdf7",
                 "name":  "Client Crud",
                 "email":  "client_crud_248866429@test.com",
                 "phoneNumber":  "+201222222222",
                 "gender":  1,
                 "dateOfBirth":  "1990-01-01",
                 "address":  "Alexandria",
                 "status":  "Active"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 10. Client Delete Account - Wrong Password

**Request:** DELETE http://localhost:5049/api/clients/profile

**Body:**
`json
{
    "CurrentPassword":  "WrongPassword!"
}
``n
**Response Status:** 400

**Response Body:**
`json
{
    "success":  false,
    "data":  null,
    "message":  "كلمة المرور الحالية غير صحيحة.",
    "errors":  null,
    "statusCode":  400
}
``n---


### 11. Client Delete Account - Success

**Request:** DELETE http://localhost:5049/api/clients/profile

**Body:**
`json
{
    "CurrentPassword":  "Password123!"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "message":  "تم حذف الملف الشخصي بنجاح.",
    "errors":  null,
    "statusCode":  200
}
``n---



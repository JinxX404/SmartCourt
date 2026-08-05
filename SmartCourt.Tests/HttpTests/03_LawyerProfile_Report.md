# Lawyer Profile CRUD Test Report

### 0. Setup - Register Lawyer

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
`json
{
    "Email":  "lawyer_crud_374814721@test.com",
    "FullName":  "Lawyer Crud",
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
                 "userId":  "f4d3a503-c8b2-44cc-4dd6-08def2fabdf7",
                 "email":  "lawyer_crud_374814721@test.com",
                 "fullName":  "Lawyer Crud",
                 "role":  "Lawyer"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
``n---


Found confirmation URL for lawyer_crud_374814721@test.com: http://localhost:5173/verify-email?userId=f4d3a503-c8b2-44cc-4dd6-08def2fabdf7&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI4bndFR0NhSzI5eUVId09paVN5WHRTVEE2UEM5V1puS1NRalpiQzl6TS9nbUplNnd6V2NrN1NKaWpTa2t3WitEcUdweFVSWEJuOG9STzRLeTVLRzh3b1JzRmRRZ21iYW91LzU2ME1pMkNaakY3U25xTzRQd0xFWC9pWUQwZ3lSSmtKUjFtWWcxaytXOW1aMGUwTGhIZEk1aVNQeHlSeFVDazBQc3c1ZEMyYm5udWFHR0NZSTNIVkRZN29pc0JMdHE4VTVuUXdsZ3lUN3pGd1hkc0xHdk8xRHFDL0pienNUWmhSYVkvQUxPMkZWdz09

### Confirm Email for lawyer_crud_374814721@test.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=f4d3a503-c8b2-44cc-4dd6-08def2fabdf7&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI4bndFR0NhSzI5eUVId09paVN5WHRTVEE2UEM5V1puS1NRalpiQzl6TS9nbUplNnd6V2NrN1NKaWpTa2t3WitEcUdweFVSWEJuOG9STzRLeTVLRzh3b1JzRmRRZ21iYW91LzU2ME1pMkNaakY3U25xTzRQd0xFWC9pWUQwZ3lSSmtKUjFtWWcxaytXOW1aMGUwTGhIZEk1aVNQeHlSeFVDazBQc3c1ZEMyYm5udWFHR0NZSTNIVkRZN29pc0JMdHE4VTVuUXdsZ3lUN3pGd1hkc0xHdk8xRHFDL0pienNUWmhSYVkvQUxPMkZWdz09

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


### 0. Setup - Login Lawyer

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Password123!",
    "Email":  "lawyer_crud_374814721@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "f4d3a503-c8b2-44cc-4dd6-08def2fabdf7",
                              "email":  "lawyer_crud_374814721@test.com",
                              "fullName":  "Lawyer Crud",
                              "role":  "Lawyer"
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJmNGQzYTUwMy1jOGIyLTQ0Y2MtNGRkNi0wOGRlZjJmYWJkZjciLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImY0ZDNhNTAzLWM4YjItNDRjYy00ZGQ2LTA4ZGVmMmZhYmRmNyIsImVtYWlsIjoibGF3eWVyX2NydWRfMzc0ODE0NzIxQHRlc3QuY29tIiwibmFtZSI6Ikxhd3llciBDcnVkIiwic2VjdXJpdHlfc3RhbXAiOiJCT0ZUTTI3VFZGR0Q3UTdLNEpXVkhJSEZBNFFTVERYQSIsImp0aSI6IjU2ZWUzMDQ3LTk2OTUtNGIxOC1hZWYxLWVjMjI3MmZkMzE5YiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NTkzODgyNSwiZXhwIjoxNzg1OTQyNDI1LCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.iCnYsl5WYjMcmpVTNVQOkh4N4-8u8ZIUDHFdEkwNsNw",
                 "expiresIn":  3600,
                 "refreshToken":  "1+kJsEUJ6CWtwkHnQMddJlF0SZD8PUGo0pT8Sx2nFyhP8J/iSfdj/BN1a1ZQeP7Txr/jS2e1enOzAk71kzIzaw==",
                 "refreshTokenExpiration":  "2026-08-12T14:07:05.5562811Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 1. Lawyer Complete - Missing NationalNumber & Bio

**Request:** POST http://localhost:5049/api/lawyers/profile/complete

**Body:**
`json
{
    "DateOfBirth":  "1990-01-01",
    "PhoneNumber":  "+201011111111",
    "Gender":  1,
    "Address":  "Law Firm 1"
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
                   "Level":  [
                                 "مستوى المحامي غير صالح."
                             ],
                   "NationalNumber":  [
                                          "\u0027National Number\u0027 must not be empty.",
                                          "الرقم القومي يجب أن يتكون من 14 رقم."
                                      ]
               },
    "traceId":  "00-4460b72534e3da00a6fddb2b0441c415-567741b6b9871f9b-00"
}
``n---


### 2. Lawyer Complete - Invalid National Number Length

**Request:** POST http://localhost:5049/api/lawyers/profile/complete

**Body:**
`json
{
    "Level":  1,
    "DateOfBirth":  "1990-01-01",
    "Gender":  1,
    "Address":  "Law Firm 1",
    "NationalNumber":  "123",
    "Bio":  "Hello",
    "PhoneNumber":  "+201011111111"
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
                   "NationalNumber":  [
                                          "الرقم القومي يجب أن يتكون من 14 رقم."
                                      ]
               },
    "traceId":  "00-b4586bc45a383698349a9ceebbb57456-3c701077037a26a0-00"
}
``n---


### 3. Lawyer Complete - Invalid Lawyer Level

**Request:** POST http://localhost:5049/api/lawyers/profile/complete

**Body:**
`json
{
    "Level":  999,
    "DateOfBirth":  "1990-01-01",
    "Gender":  1,
    "Address":  "Law Firm 1",
    "NationalNumber":  "29001013035641",
    "Bio":  "Hello",
    "PhoneNumber":  "+201011111111"
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
                   "request":  [
                                   "The request field is required."
                               ],
                   "$.Level":  [
                                   "The JSON value could not be converted to SmartCourt.Common.Enums.LawyerLevel. Path: $.Level | LineNumber: 1 | BytePositionInLine: 17."
                               ]
               },
    "traceId":  "00-c8ee58e5a3b0f8c82a1c007a6ca9debd-20e627f9e1d11bf5-00"
}
``n---


### 4. Lawyer Complete - Valid Data

**Request:** POST http://localhost:5049/api/lawyers/profile/complete

**Body:**
`json
{
    "Level":  1,
    "DateOfBirth":  "1990-01-01",
    "Gender":  1,
    "Address":  "Law Firm 1",
    "NationalNumber":  "29001013035641",
    "Bio":  "Hello",
    "PhoneNumber":  "+201011111111"
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


### 5. Re-Login Lawyer (Token Refresh)

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Password123!",
    "Email":  "lawyer_crud_374814721@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "f4d3a503-c8b2-44cc-4dd6-08def2fabdf7",
                              "email":  "lawyer_crud_374814721@test.com",
                              "fullName":  "Lawyer Crud",
                              "role":  "Lawyer"
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJmNGQzYTUwMy1jOGIyLTQ0Y2MtNGRkNi0wOGRlZjJmYWJkZjciLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImY0ZDNhNTAzLWM4YjItNDRjYy00ZGQ2LTA4ZGVmMmZhYmRmNyIsImVtYWlsIjoibGF3eWVyX2NydWRfMzc0ODE0NzIxQHRlc3QuY29tIiwibmFtZSI6Ikxhd3llciBDcnVkIiwic2VjdXJpdHlfc3RhbXAiOiJSVk5JWUJQRFRRMzRUVkw1WlVUQUtFSTJLUTJBU0FNUCIsImp0aSI6Ijc4YTlhZGRhLWU0YWMtNDYyNC1iNTBiLTExODgyMjcxOTAzNCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NTkzODgyNiwiZXhwIjoxNzg1OTQyNDI2LCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.1AFbm4haFKKQD3fj0NFqeJLVKys-qW9Rx4UuXAKP0AM",
                 "expiresIn":  3600,
                 "refreshToken":  "LGORKSkfhrdEVEUkEaiRN7/1EHQ9lSIXqcpKTB7FpZMSo0bDCaldO4Q7tf8CIIJTUawEcFa2FYjFw4mhWKB+7Q==",
                 "refreshTokenExpiration":  "2026-08-12T14:07:06.09319Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 6. Lawyer GET Private Profile

**Request:** GET http://localhost:5049/api/lawyers/profile

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "f4d3a503-c8b2-44cc-4dd6-08def2fabdf7",
                 "name":  "Lawyer Crud",
                 "email":  "lawyer_crud_374814721@test.com",
                 "phoneNumber":  "+201011111111",
                 "nationalNumber":  "29001013035641",
                 "gender":  1,
                 "dateOfBirth":  "1990-01-01",
                 "level":  1,
                 "bio":  "Hello",
                 "address":  "Law Firm 1",
                 "status":  "PendingReview",
                 "isAvailable":  true,
                 "profilePictureUrl":  null
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 7. Lawyer GET Public Profile (Anonymous)

**Request:** GET http://localhost:5049/api/lawyers/public/f4d3a503-c8b2-44cc-4dd6-08def2fabdf7

**Response Status:** 404

**Response Body:**
`json
{
    "success":  false,
    "data":  null,
    "message":  "المحامي غير موجود",
    "errors":  null,
    "statusCode":  404
}
``n---


### 8. Lawyer Update - Bio Exceeds Max Length

**Request:** PUT http://localhost:5049/api/lawyers/profile

**Body:**
`json
{
    "Bio":  "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
    "Level":  2,
    "PhoneNumber":  "+201222222222",
    "Address":  "New Address"
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
                   "Bio":  [
                               "يجب ألا تتجاوز السيرة الذاتية 500 حرف."
                           ]
               },
    "traceId":  "00-aabdffe28896e9d4721f1b0bfb2f701e-6cb3ac23a7698edb-00"
}
``n---


### 9. Lawyer Update - Valid Data

**Request:** PUT http://localhost:5049/api/lawyers/profile

**Body:**
`json
{
    "Bio":  "Updated Bio",
    "Level":  2,
    "PhoneNumber":  "+201222222222",
    "Address":  "New Address"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "message":  "تم تحديث البيانات بنجاح",
    "errors":  null,
    "statusCode":  200
}
``n---


### 9b. Re-Login Lawyer (Token Refresh)

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Password123!",
    "Email":  "lawyer_crud_374814721@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "f4d3a503-c8b2-44cc-4dd6-08def2fabdf7",
                              "email":  "lawyer_crud_374814721@test.com",
                              "fullName":  "Lawyer Crud",
                              "role":  "Lawyer"
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJmNGQzYTUwMy1jOGIyLTQ0Y2MtNGRkNi0wOGRlZjJmYWJkZjciLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImY0ZDNhNTAzLWM4YjItNDRjYy00ZGQ2LTA4ZGVmMmZhYmRmNyIsImVtYWlsIjoibGF3eWVyX2NydWRfMzc0ODE0NzIxQHRlc3QuY29tIiwibmFtZSI6Ikxhd3llciBDcnVkIiwic2VjdXJpdHlfc3RhbXAiOiJVNTZXNUpDSlZLUUVKUU5CTTZaMlVCVUVTS1JGUjQ3QSIsImp0aSI6IjlkZGFjN2JkLWIwYWYtNGU2Mi04YzA4LTA5OWQwOWFhNTg0YSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NTkzODgyNywiZXhwIjoxNzg1OTQyNDI3LCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.znVC6so1_a4CUlytLcZ5IL40Pp2FrOE3UGHLgB-SPmA",
                 "expiresIn":  3600,
                 "refreshToken":  "POtTQzbuFKCkDohGJsFQdyilvqlkm7QMm452TR92RuWufYaKJTAq/6cf1MenNKQfALW3Nz7rMFTt0A0UqTtJEA==",
                 "refreshTokenExpiration":  "2026-08-12T14:07:07.5197302Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 10. Lawyer GET Private Profile (Verify Update)

**Request:** GET http://localhost:5049/api/lawyers/profile

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "id":  "f4d3a503-c8b2-44cc-4dd6-08def2fabdf7",
                 "name":  "Lawyer Crud",
                 "email":  "lawyer_crud_374814721@test.com",
                 "phoneNumber":  "+201222222222",
                 "nationalNumber":  "29001013035641",
                 "gender":  1,
                 "dateOfBirth":  "1990-01-01",
                 "level":  2,
                 "bio":  "Updated Bio",
                 "address":  "New Address",
                 "status":  "PendingReview",
                 "isAvailable":  true,
                 "profilePictureUrl":  null
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 11. Lawyer Delete Account - Wrong Password

**Request:** DELETE http://localhost:5049/api/lawyers/profile

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


### 12. Lawyer Delete Account - Success

**Request:** DELETE http://localhost:5049/api/lawyers/profile

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
    "message":  "تم حذف الحساب بنجاح",
    "errors":  null,
    "statusCode":  200
}
``n---


### 13. Lawyer GET Public Profile (After Delete)

**Request:** GET http://localhost:5049/api/lawyers/public/f4d3a503-c8b2-44cc-4dd6-08def2fabdf7

**Response Status:** 404

**Response Body:**
`json
{
    "success":  false,
    "data":  null,
    "message":  "المحامي غير موجود",
    "errors":  null,
    "statusCode":  404
}
``n---



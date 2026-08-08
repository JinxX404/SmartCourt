# Lawyer Profile CRUD Test Report

### 0. Setup - Register Lawyer

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
`json
{
  "ConfirmPassword": "Password123!",
  "FullName": "Lawyer Crud",
  "Email": "lawyer_crud_2128036412@test.com",
  "Password": "Password123!"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "e9c7c453-695b-4164-f779-08def48f6968",
    "email": "lawyer_crud_2128036412@test.com",
    "fullName": "Lawyer Crud",
    "role": "Lawyer"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


Found confirmation URL for lawyer_crud_2128036412@test.com: http://localhost:5173/verify-email?userId=e9c7c453-695b-4164-f779-08def48f6968&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5WHd3cVNRWGdkMElKNkdMZVEzMXNYZW15RG5YVW5QcnQvdFNmTm1OZmphSHdTUWI2OW5pRWg4TDNUZFo0SjZ2OHJ3OEdmWWdrZFJjdk1QdVpPZlNtMWJNNnc2RUw4NGMxK1E2eU9XYnZ4MjZabG5qaU93UXV2VUxYREdhVnJVTmJoMW5VVVVrdmRGQUIxczgwUVJxSDFLR0lxYVN0a0VYaCtxa3p6MXBzNGtSYUNWRUkyb2xucTZEWlFPV2ZFbmVQOFpPMTN6NUFmejZSTDd1NFVEZzRTSVZFRVpYcCtwY0VFSjRCamplNmczUT09

### Confirm Email for lawyer_crud_2128036412@test.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=e9c7c453-695b-4164-f779-08def48f6968&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5WHd3cVNRWGdkMElKNkdMZVEzMXNYZW15RG5YVW5QcnQvdFNmTm1OZmphSHdTUWI2OW5pRWg4TDNUZFo0SjZ2OHJ3OEdmWWdrZFJjdk1QdVpPZlNtMWJNNnc2RUw4NGMxK1E2eU9XYnZ4MjZabG5qaU93UXV2VUxYREdhVnJVTmJoMW5VVVVrdmRGQUIxczgwUVJxSDFLR0lxYVN0a0VYaCtxa3p6MXBzNGtSYUNWRUkyb2xucTZEWlFPV2ZFbmVQOFpPMTN6NUFmejZSTDd1NFVEZzRTSVZFRVpYcCtwY0VFSjRCamplNmczUT09

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


### 0. Setup - Login Lawyer

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "lawyer_crud_2128036412@test.com",
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
      "id": "e9c7c453-695b-4164-f779-08def48f6968",
      "email": "lawyer_crud_2128036412@test.com",
      "fullName": "Lawyer Crud",
      "role": "Lawyer",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJlOWM3YzQ1My02OTViLTQxNjQtZjc3OS0wOGRlZjQ4ZjY5NjgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImU5YzdjNDUzLTY5NWItNDE2NC1mNzc5LTA4ZGVmNDhmNjk2OCIsImVtYWlsIjoibGF3eWVyX2NydWRfMjEyODAzNjQxMkB0ZXN0LmNvbSIsIm5hbWUiOiJMYXd5ZXIgQ3J1ZCIsInNlY3VyaXR5X3N0YW1wIjoiUkpXT0dEU0NaVVpSTURFWjJVTkZKWEZHVEFSUkI3RzUiLCJqdGkiOiJhZWI2MTI0Yy0zZjRiLTQ3ZDgtYmRhOC1mYjM0YTZjMTg5MWMiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODYxMTMxNzIsImV4cCI6MTc4NjExNDA3MiwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.CyTAkZB6ka5C7Ufil1MCiBhqey6zfYE9jtyrRdrP6CQ",
    "expiresIn": 900,
    "refreshToken": "xWXHYK1tA/yWTDjIw7ROQ8m+xbFBhCCGD6R+ZxHWbHdu/ICjfEpxl+rC9GeVQF0JCU7nwAcupJznWGg7xeVQYw==",
    "refreshTokenExpiration": "2026-08-14T14:32:52.4162727Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 1. Lawyer Complete - Missing NationalNumber & Bio

**Request:** POST http://localhost:5049/api/lawyers/profile/complete

**Body:**
`json
{
  "Gender": 1,
  "PhoneNumber": "+201011111111",
  "Address": "Law Firm 1",
  "DateOfBirth": "1990-01-01"
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
    "Level": [
      "مستوى المحامي غير صالح."
    ],
    "NationalNumber": [
      "'National Number' must not be empty.",
      "الرقم القومي يجب أن يتكون من 14 رقم."
    ],
    "Specializations": [
      "يجب إدخال تخصص واحد على الأقل."
    ]
  },
  "traceId": "00-72f3951479110c01667db61c734dcb3d-f658b4509f47b960-00"
}
``n---


### 2. Lawyer Complete - Invalid National Number Length

**Request:** POST http://localhost:5049/api/lawyers/profile/complete

**Body:**
`json
{
  "Level": 1,
  "Address": "Law Firm 1",
  "NationalNumber": "123",
  "PhoneNumber": "+201011111111",
  "Gender": 1,
  "Specializations": [
    {
      "YearsOfExperience": 5,
      "CasesHandled": 10,
      "Specialization": 1
    }
  ],
  "Bio": "Hello",
  "DateOfBirth": "1990-01-01"
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
    "NationalNumber": [
      "الرقم القومي يجب أن يتكون من 14 رقم."
    ]
  },
  "traceId": "00-8738f8236d167a5bb8122896540da79a-bd09fdda83131dff-00"
}
``n---


### 3. Lawyer Complete - Invalid Lawyer Level

**Request:** POST http://localhost:5049/api/lawyers/profile/complete

**Body:**
`json
{
  "Level": 999,
  "Address": "Law Firm 1",
  "NationalNumber": "29001014304533",
  "PhoneNumber": "+201011111111",
  "Gender": 1,
  "Specializations": [
    {
      "YearsOfExperience": 5,
      "CasesHandled": 10,
      "Specialization": 1
    }
  ],
  "Bio": "Hello",
  "DateOfBirth": "1990-01-01"
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
    "request": [
      "The request field is required."
    ],
    "$.Level": [
      "The JSON value could not be converted to SmartCourt.Common.Enums.LawyerLevel. Path: $.Level | LineNumber: 1 | BytePositionInLine: 14."
    ]
  },
  "traceId": "00-d791934bc2b4506132e09d8d4cf18c5f-709d03c65834f7b0-00"
}
``n---


### 4. Lawyer Complete - Valid Data

**Request:** POST http://localhost:5049/api/lawyers/profile/complete

**Body:**
`json
{
  "Level": 1,
  "Address": "Law Firm 1",
  "NationalNumber": "29001014304533",
  "PhoneNumber": "+201011111111",
  "Gender": 1,
  "Specializations": [
    {
      "YearsOfExperience": 5,
      "CasesHandled": 10,
      "Specialization": 1
    }
  ],
  "Bio": "Hello",
  "DateOfBirth": "1990-01-01"
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


### 5. Re-Login Lawyer (Token Refresh)

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "lawyer_crud_2128036412@test.com",
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
      "id": "e9c7c453-695b-4164-f779-08def48f6968",
      "email": "lawyer_crud_2128036412@test.com",
      "fullName": "Lawyer Crud",
      "role": "Lawyer",
      "status": "PendingReview",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJlOWM3YzQ1My02OTViLTQxNjQtZjc3OS0wOGRlZjQ4ZjY5NjgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImU5YzdjNDUzLTY5NWItNDE2NC1mNzc5LTA4ZGVmNDhmNjk2OCIsImVtYWlsIjoibGF3eWVyX2NydWRfMjEyODAzNjQxMkB0ZXN0LmNvbSIsIm5hbWUiOiJMYXd5ZXIgQ3J1ZCIsInNlY3VyaXR5X3N0YW1wIjoiMkUyUDRGUE9TVEJNNUhBUzRCTkkzTkxJWUhRQUw0VzMiLCJqdGkiOiJlNzI4MzU2Ny0wODA5LTQ5MTgtOWFiNS03Y2MyNTUzMGU5Y2YiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODYxMTMxNzQsImV4cCI6MTc4NjExNDA3NCwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.mN2sDzQXcK1yALDiwDnPNQbupfdeaKLUgaAsUWmThBk",
    "expiresIn": 900,
    "refreshToken": "UeYGPQliuwAzHQJT1hH1uxJjcH3Rsa0TJmIyCwDs3XYsYiqfHmKW5+LVM0AiSXsrw56TI0sIoDUEn3C5Sjmf0w==",
    "refreshTokenExpiration": "2026-08-14T14:32:54.9335667Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 6. Lawyer GET Private Profile

**Request:** GET http://localhost:5049/api/lawyers/profile

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "e9c7c453-695b-4164-f779-08def48f6968",
    "name": "Lawyer Crud",
    "email": "lawyer_crud_2128036412@test.com",
    "phoneNumber": "+201011111111",
    "nationalNumber": "29001014304533",
    "gender": 1,
    "dateOfBirth": "1990-01-01",
    "level": 1,
    "yearsOfExperience": 5,
    "specializationName": "CivilLaw",
    "bio": "Hello",
    "address": "Law Firm 1",
    "governorate": null,
    "city": null,
    "status": "PendingReview",
    "isAvailable": true,
    "profilePictureUrl": null,
    "rejectionReason": null,
    "specializations": [
      {
        "specialization": 1,
        "yearsOfExperience": 5,
        "casesHandled": 10
      }
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 7. Lawyer GET Public Profile (Anonymous)

**Request:** GET http://localhost:5049/api/lawyers/public/e9c7c453-695b-4164-f779-08def48f6968

**Response Status:** 404

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "المحامي غير موجود",
  "errors": null,
  "statusCode": 404
}
``n---


### 8. Lawyer Update - Bio Exceeds Max Length

**Request:** PUT http://localhost:5049/api/lawyers/profile

**Body:**
`json
{
  "Level": 2,
  "NationalNumber": "29001014304533",
  "Address": "New Address",
  "PhoneNumber": "+201222222222",
  "Bio": "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
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
    "Bio": [
      "يجب ألا تتجاوز السيرة الذاتية 500 حرف."
    ]
  },
  "traceId": "00-dc12603d898fc72db2fdccf166f8b69b-5bf392274aa82c3e-00"
}
``n---


### 9. Lawyer Update - Valid Data

**Request:** PUT http://localhost:5049/api/lawyers/profile

**Body:**
`json
{
  "Level": 2,
  "NationalNumber": "29001014304533",
  "Address": "New Address",
  "PhoneNumber": "+201222222222",
  "Bio": "Updated Bio"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "message": "تم تحديث البيانات بنجاح",
  "errors": null,
  "statusCode": 200
}
``n---


### 9b. Re-Login Lawyer (Token Refresh)

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Email": "lawyer_crud_2128036412@test.com",
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
      "id": "e9c7c453-695b-4164-f779-08def48f6968",
      "email": "lawyer_crud_2128036412@test.com",
      "fullName": "Lawyer Crud",
      "role": "Lawyer",
      "status": "PendingReview",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJlOWM3YzQ1My02OTViLTQxNjQtZjc3OS0wOGRlZjQ4ZjY5NjgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImU5YzdjNDUzLTY5NWItNDE2NC1mNzc5LTA4ZGVmNDhmNjk2OCIsImVtYWlsIjoibGF3eWVyX2NydWRfMjEyODAzNjQxMkB0ZXN0LmNvbSIsIm5hbWUiOiJMYXd5ZXIgQ3J1ZCIsInNlY3VyaXR5X3N0YW1wIjoiMkUyUDRGUE9TVEJNNUhBUzRCTkkzTkxJWUhRQUw0VzMiLCJqdGkiOiJiMTlmMDY4MS0wNWNjLTQxZjktYjlmMC1iMGYzNzFlMjdiNzkiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODYxMTMxNzYsImV4cCI6MTc4NjExNDA3NiwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.2PxY1e7soY6fRpkpoVv2cFWQWAl09eRTSQXCND95wDg",
    "expiresIn": 900,
    "refreshToken": "HA22jiZDw+UQbnorIg8LVEH+rx2SDBg4UajwQdt8kzg0NHtMPvbIz3pqQsFW7AVZfeWvV9MBttNr45Yqlx0ZXw==",
    "refreshTokenExpiration": "2026-08-14T14:32:56.1507608Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 10. Lawyer GET Private Profile (Verify Update)

**Request:** GET http://localhost:5049/api/lawyers/profile

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "e9c7c453-695b-4164-f779-08def48f6968",
    "name": "Lawyer Crud",
    "email": "lawyer_crud_2128036412@test.com",
    "phoneNumber": "+201011111111",
    "nationalNumber": "29001014304533",
    "gender": 1,
    "dateOfBirth": null,
    "level": 2,
    "yearsOfExperience": 5,
    "specializationName": "CivilLaw",
    "bio": "Updated Bio",
    "address": "New Address",
    "governorate": null,
    "city": null,
    "status": "PendingReview",
    "isAvailable": true,
    "profilePictureUrl": null,
    "rejectionReason": null,
    "specializations": [
      {
        "specialization": 1,
        "yearsOfExperience": 5,
        "casesHandled": 10
      }
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 11. Lawyer Delete Account - Wrong Password

**Request:** DELETE http://localhost:5049/api/lawyers/profile

**Body:**
`json
{
  "CurrentPassword": "WrongPassword!"
}
``n
**Response Status:** 400

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "كلمة المرور الحالية غير صحيحة.",
  "errors": null,
  "statusCode": 400
}
``n---


### 12. Lawyer Delete Account - Success

**Request:** DELETE http://localhost:5049/api/lawyers/profile

**Body:**
`json
{
  "CurrentPassword": "Password123!"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "message": "تم حذف الحساب بنجاح",
  "errors": null,
  "statusCode": 200
}
``n---


### 13. Lawyer GET Public Profile (After Delete)

**Request:** GET http://localhost:5049/api/lawyers/public/e9c7c453-695b-4164-f779-08def48f6968

**Response Status:** 404

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "المحامي غير موجود",
  "errors": null,
  "statusCode": 404
}
``n---



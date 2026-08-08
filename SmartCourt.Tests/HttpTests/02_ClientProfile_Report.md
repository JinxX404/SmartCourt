# Client Profile CRUD Test Report

### 0. Setup - Register Client

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
  "FullName": "Client Crud",
  "Password": "Password123!",
  "Email": "client_crud_1360799765@test.com",
  "ConfirmPassword": "Password123!"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "62efcf3f-7d9a-4369-f77c-08def48f6968",
    "email": "client_crud_1360799765@test.com",
    "fullName": "Client Crud",
    "role": "Client"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


Found confirmation URL for client_crud_1360799765@test.com: http://localhost:5173/verify-email?userId=62efcf3f-7d9a-4369-f77c-08def48f6968&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5Znh2N0FQTFQzbE5lcitOQlRPdW1BQ3VTdVp6ZTVmbkY4L2tGTUIrckw5NTg5UE1lR0NxaFR1UTZRclhKUUxqSEIxa1hUNU51K0d2T2w3VHFhU3hVV1pKa3RUUm5UbzhvQklwbHp6cm42QmFRdFRRZGxiZGZaWlh0bjltREpLQkJEaGRjM1V5T09LUWZ2cnI0QzJTWjc3K3FJaFlCTjFOaUhyV2kyZVBEd0wvVkw3VFFhUmlhQWF6ZTlJdkdzKzBHS093Q0s0SnRmbkdSdGRjT3lOYW1UZEFyeENmUjBERVpaM1hLOUlOa1hQUT09

### Confirm Email for client_crud_1360799765@test.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=62efcf3f-7d9a-4369-f77c-08def48f6968&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5Znh2N0FQTFQzbE5lcitOQlRPdW1BQ3VTdVp6ZTVmbkY4L2tGTUIrckw5NTg5UE1lR0NxaFR1UTZRclhKUUxqSEIxa1hUNU51K0d2T2w3VHFhU3hVV1pKa3RUUm5UbzhvQklwbHp6cm42QmFRdFRRZGxiZGZaWlh0bjltREpLQkJEaGRjM1V5T09LUWZ2cnI0QzJTWjc3K3FJaFlCTjFOaUhyV2kyZVBEd0wvVkw3VFFhUmlhQWF6ZTlJdkdzKzBHS093Q0s0SnRmbkdSdGRjT3lOYW1UZEFyeENmUjBERVpaM1hLOUlOa1hQUT09

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


### 0. Setup - Login Client

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Password": "Password123!",
  "Email": "client_crud_1360799765@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "62efcf3f-7d9a-4369-f77c-08def48f6968",
      "email": "client_crud_1360799765@test.com",
      "fullName": "Client Crud",
      "role": "Client",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2MmVmY2YzZi03ZDlhLTQzNjktZjc3Yy0wOGRlZjQ4ZjY5NjgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjYyZWZjZjNmLTdkOWEtNDM2OS1mNzdjLTA4ZGVmNDhmNjk2OCIsImVtYWlsIjoiY2xpZW50X2NydWRfMTM2MDc5OTc2NUB0ZXN0LmNvbSIsIm5hbWUiOiJDbGllbnQgQ3J1ZCIsInNlY3VyaXR5X3N0YW1wIjoiMkpBQTU1TFo3WFhHUkQzSEFNRFUzSEVIUjNaTzRFRjMiLCJqdGkiOiIyYzJhOThlYy0zZWQxLTRiNDMtOWMyNi00ODBlYTYyNGRkNzUiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDbGllbnQiLCJuYmYiOjE3ODYxMTMzNjAsImV4cCI6MTc4NjExNDI2MCwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.-mLCwSpZRUBXi7Tt0FIcmKucQhJ0O9OzODSdAPushU0",
    "expiresIn": 900,
    "refreshToken": "QQ0yYqAAERq7DNJm9PiNwi1RsNafrJURvxOSIzHvlZUD6ZtbS5zDZ/uvPFCAt/EQ/aDJ4CHYGl8JYAKjdcn/uw==",
    "refreshTokenExpiration": "2026-08-14T14:36:00.0516601Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 1. Client Complete - Missing Phone & DOB

**Request:** POST http://localhost:5049/api/clients/profile/complete

**Body:**
`json
{
  "Gender": 1,
  "Address": "Cairo"
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
    "DateOfBirth": [
      "تاريخ الميلاد مطلوب"
    ],
    "PhoneNumber": [
      "رقم الهاتف مطلوب",
      "رقم الهاتف يجب أن يكون بالتنسيق المصري +20XXXXXXXXXX"
    ],
    "NationalNumber": [
      "'National Number' must not be empty."
    ]
  },
  "traceId": "00-b9813aec466e77513a37e0bc46c03dcf-64ceef420f69a171-00"
}
``n---


### 2. Client Complete - Invalid Phone Format

**Request:** POST http://localhost:5049/api/clients/profile/complete

**Body:**
`json
{
  "PhoneNumber": "123456789",
  "Address": "Cairo",
  "DateOfBirth": "1990-01-01",
  "Gender": 1
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
    "PhoneNumber": [
      "رقم الهاتف يجب أن يكون بالتنسيق المصري +20XXXXXXXXXX"
    ],
    "NationalNumber": [
      "'National Number' must not be empty."
    ]
  },
  "traceId": "00-1db7d6ee5122ff2da4d75a06fdbc0b0d-cfedeba38da58ddc-00"
}
``n---


### 3. Client Complete - Future Date of Birth

**Request:** POST http://localhost:5049/api/clients/profile/complete

**Body:**
`json
{
  "PhoneNumber": "+201011111111",
  "Address": "Cairo",
  "DateOfBirth": "2026-08-08",
  "Gender": 1
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
    "DateOfBirth": [
      "تاريخ الميلاد يجب أن يكون في الماضي"
    ],
    "NationalNumber": [
      "'National Number' must not be empty."
    ]
  },
  "traceId": "00-a120a8b825cb5aa3c65ce2c46579295b-a92678cd162ac3f3-00"
}
``n---


### 4. Client Complete - Valid Data

**Request:** POST http://localhost:5049/api/clients/profile/complete

**Body:**
`json
{
  "PhoneNumber": "+201011111111",
  "Address": "Cairo",
  "NationalNumber": "29001018665640",
  "DateOfBirth": "1990-01-01",
  "Gender": 1
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


### 5. Re-Login Client (Token Refresh)

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Password": "Password123!",
  "Email": "client_crud_1360799765@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "62efcf3f-7d9a-4369-f77c-08def48f6968",
      "email": "client_crud_1360799765@test.com",
      "fullName": "Client Crud",
      "role": "Client",
      "status": "PendingReview",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2MmVmY2YzZi03ZDlhLTQzNjktZjc3Yy0wOGRlZjQ4ZjY5NjgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjYyZWZjZjNmLTdkOWEtNDM2OS1mNzdjLTA4ZGVmNDhmNjk2OCIsImVtYWlsIjoiY2xpZW50X2NydWRfMTM2MDc5OTc2NUB0ZXN0LmNvbSIsIm5hbWUiOiJDbGllbnQgQ3J1ZCIsInNlY3VyaXR5X3N0YW1wIjoiWEUyWFU1NVlPQ0tNMk9HM0ZZREhIRVoyT0lPQzNKU0giLCJqdGkiOiJmMTI0ZTU3MC1jZjY0LTQxYWItOGVkZi00MmExNWJiM2E5NjkiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDbGllbnQiLCJuYmYiOjE3ODYxMTMzNjAsImV4cCI6MTc4NjExNDI2MCwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.CB1qsFCtrPIzdVESVPKJvO7zSlzmkCB-3a_QGY1SFRs",
    "expiresIn": 900,
    "refreshToken": "2ojVaGDru7O9scORE+qrAdVdgNHE3rS2Wj/w6kKnd+jd4F+tpX0Oy7PXy7OAvk3sPkNPXyR2MZ+xxwX7lK+p7g==",
    "refreshTokenExpiration": "2026-08-14T14:36:00.599125Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 6. Client GET Private Profile

**Request:** GET http://localhost:5049/api/clients/profile

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "62efcf3f-7d9a-4369-f77c-08def48f6968",
    "name": "Client Crud",
    "email": "client_crud_1360799765@test.com",
    "phoneNumber": "+201011111111",
    "nationalNumber": "29001018665640",
    "gender": 1,
    "dateOfBirth": "1990-01-01",
    "address": "Cairo",
    "governorate": null,
    "city": null,
    "status": "PendingReview",
    "rejectionReason": null
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 7. Client Update - Invalid Phone Format

**Request:** PUT http://localhost:5049/api/clients/profile

**Body:**
`json
{
  "PhoneNumber": "invalid_phone",
  "Address": "Alexandria",
  "NationalNumber": "29001018665640"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "message": "تم تحديث الملف الشخصي بنجاح.",
  "errors": null,
  "statusCode": 200
}
``n---


### 8. Client Update - Valid Data

**Request:** PUT http://localhost:5049/api/clients/profile

**Body:**
`json
{
  "PhoneNumber": "+201222222222",
  "Address": "Alexandria",
  "NationalNumber": "29001018665640"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "message": "تم تحديث الملف الشخصي بنجاح.",
  "errors": null,
  "statusCode": 200
}
``n---


### 8b. Re-Login Client (Token Refresh)

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Password": "Password123!",
  "Email": "client_crud_1360799765@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "62efcf3f-7d9a-4369-f77c-08def48f6968",
      "email": "client_crud_1360799765@test.com",
      "fullName": "Client Crud",
      "role": "Client",
      "status": "PendingReview",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2MmVmY2YzZi03ZDlhLTQzNjktZjc3Yy0wOGRlZjQ4ZjY5NjgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjYyZWZjZjNmLTdkOWEtNDM2OS1mNzdjLTA4ZGVmNDhmNjk2OCIsImVtYWlsIjoiY2xpZW50X2NydWRfMTM2MDc5OTc2NUB0ZXN0LmNvbSIsIm5hbWUiOiJDbGllbnQgQ3J1ZCIsInNlY3VyaXR5X3N0YW1wIjoiWEUyWFU1NVlPQ0tNMk9HM0ZZREhIRVoyT0lPQzNKU0giLCJqdGkiOiIwYmJjMjBkMC05ZDc0LTQ5NTMtOTMyYS1kMjRjN2I0N2I0MWMiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDbGllbnQiLCJuYmYiOjE3ODYxMTMzNjEsImV4cCI6MTc4NjExNDI2MSwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.hHP0YDyK2g5NhL62DfFrOrGFM-5L5NIQhAgH2av-oRk",
    "expiresIn": 900,
    "refreshToken": "AW1vhImK6qLic3QsYZX5afTZkp94tCknITJ/coTsRW4E3/kJxi00P3QbtN2/YXezB1KBjGmSHfGhB7V2MGDqlA==",
    "refreshTokenExpiration": "2026-08-14T14:36:01.195843Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 9. Client GET Private Profile (Verify Update)

**Request:** GET http://localhost:5049/api/clients/profile

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "62efcf3f-7d9a-4369-f77c-08def48f6968",
    "name": "Client Crud",
    "email": "client_crud_1360799765@test.com",
    "phoneNumber": "+201011111111",
    "nationalNumber": "29001018665640",
    "gender": 1,
    "dateOfBirth": null,
    "address": "Alexandria",
    "governorate": null,
    "city": null,
    "status": "PendingReview",
    "rejectionReason": null
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 10. Client Delete Account - Wrong Password

**Request:** DELETE http://localhost:5049/api/clients/profile

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


### 11. Client Delete Account - Success

**Request:** DELETE http://localhost:5049/api/clients/profile

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
  "message": "تم حذف الملف الشخصي بنجاح.",
  "errors": null,
  "statusCode": 200
}
``n---



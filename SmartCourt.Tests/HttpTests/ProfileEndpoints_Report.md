# Profile Endpoints API Test Report

### 1. Register Client

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
{
  "Email": "client_test_37823556@test.com",
  "FullName": "Test Client",
  "Password": "Password123!",
  "ConfirmPassword": "Password123!"
}

**Response Status:** 201

**Response Body:**
{"success":true,"data":{"userId":"c6b495a5-0c2e-4147-f788-08def48f6968","email":"client_test_37823556@test.com","fullName":"Test Client","role":"Client"},"message":"تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني","errors":null,"statusCode":201}
---

### 2. Register Lawyer

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
{
  "Email": "lawyer_test_1099231217@test.com",
  "FullName": "Test Lawyer",
  "Password": "Password123!",
  "ConfirmPassword": "Password123!"
}

**Response Status:** 201

**Response Body:**
{"success":true,"data":{"userId":"7ebaeb4b-e06e-48f7-f789-08def48f6968","email":"lawyer_test_1099231217@test.com","fullName":"Test Lawyer","role":"Lawyer"},"message":"تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني","errors":null,"statusCode":201}
---

Found confirmation URL for client_test_37823556@test.com: http://localhost:5173/verify-email?userId=c6b495a5-0c2e-4147-f788-08def48f6968&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIrQXdzZ2c1YS8zTEVrc1hJYW1xdHpiMTEyampMc3NaYnVNOHNYd2NURTZ1TjJoM0hEaXZ5bHk0WkdMZ1F0RWx5MCtVazRubVdqNmpXd0NjeVFuU3A1U0ZncG16c3hCVVFOc3U5QVU5and2V1NlOTB1dHJaYWtONGM4L2NmeVZDWHh5U21GSXlNT3M5Qlh1Vk9WQ3p4bjY4dXI1UkhIcFdKeTczRW1GUE1Db3ZkVWpZWXZNbUNkTk1GaHpVa3dBQkExbUpDL0ZycGVEaTdEcHY0ZExvM2doMHlVam1idHptMmUyVGRQdEFYTlVLdz09

### Confirm Email for client_test_37823556@test.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=c6b495a5-0c2e-4147-f788-08def48f6968&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIrQXdzZ2c1YS8zTEVrc1hJYW1xdHpiMTEyampMc3NaYnVNOHNYd2NURTZ1TjJoM0hEaXZ5bHk0WkdMZ1F0RWx5MCtVazRubVdqNmpXd0NjeVFuU3A1U0ZncG16c3hCVVFOc3U5QVU5and2V1NlOTB1dHJaYWtONGM4L2NmeVZDWHh5U21GSXlNT3M5Qlh1Vk9WQ3p4bjY4dXI1UkhIcFdKeTczRW1GUE1Db3ZkVWpZWXZNbUNkTk1GaHpVa3dBQkExbUpDL0ZycGVEaTdEcHY0ZExvM2doMHlVam1idHptMmUyVGRQdEFYTlVLdz09

**Response Status:** 200

**Response Body:**
{"success":true,"message":"تم تأكيد البريد الإلكتروني بنجاح.","errors":null,"statusCode":200}
---

Found confirmation URL for lawyer_test_1099231217@test.com: http://localhost:5173/verify-email?userId=7ebaeb4b-e06e-48f7-f789-08def48f6968&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5ZFRJM3ZwcHFsY1BXaGxQbjNva2tMWVRPa0F2Uys5Ym1wUUgyQlZDL1d0dmw1ejdCZ1Z5MzZLY3dVNm5Qdm15SWd3YTBIV1h1VFc4V0oyL0g3KzNmSXV0Qk5jYUZvdWtHNjhsdmFUZm1iZ2ZGMk56SERoSVJ6alJkWlZCMzYrWUpTZkdncmZBL1BLM3Vna3B0bGU2Q2prTVF3QkNPK2NsSlFwQ0RIWWFHZWUvY0xVWXI3N0lUSlpDTzVNaGFHVFB2Uk1qVi81TjN1aWR1ekUrbEJkRFFHaVZQb0hWeGNFUjhmSzd5aWRaVVczZz09

### Confirm Email for lawyer_test_1099231217@test.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=7ebaeb4b-e06e-48f7-f789-08def48f6968&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5ZFRJM3ZwcHFsY1BXaGxQbjNva2tMWVRPa0F2Uys5Ym1wUUgyQlZDL1d0dmw1ejdCZ1Z5MzZLY3dVNm5Qdm15SWd3YTBIV1h1VFc4V0oyL0g3KzNmSXV0Qk5jYUZvdWtHNjhsdmFUZm1iZ2ZGMk56SERoSVJ6alJkWlZCMzYrWUpTZkdncmZBL1BLM3Vna3B0bGU2Q2prTVF3QkNPK2NsSlFwQ0RIWWFHZWUvY0xVWXI3N0lUSlpDTzVNaGFHVFB2Uk1qVi81TjN1aWR1ekUrbEJkRFFHaVZQb0hWeGNFUjhmSzd5aWRaVVczZz09

**Response Status:** 200

**Response Body:**
{"success":true,"message":"تم تأكيد البريد الإلكتروني بنجاح.","errors":null,"statusCode":200}
---

### 3. Login Client

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
{
  "Email": "client_test_37823556@test.com",
  "Password": "Password123!"
}

**Response Status:** 200

**Response Body:**
{"success":true,"data":{"user":{"id":"c6b495a5-0c2e-4147-f788-08def48f6968","email":"client_test_37823556@test.com","fullName":"Test Client","role":"Client","status":"Unverified","rejectionReason":null},"accessToken":"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJjNmI0OTVhNS0wYzJlLTQxNDctZjc4OC0wOGRlZjQ4ZjY5NjgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImM2YjQ5NWE1LTBjMmUtNDE0Ny1mNzg4LTA4ZGVmNDhmNjk2OCIsImVtYWlsIjoiY2xpZW50X3Rlc3RfMzc4MjM1NTZAdGVzdC5jb20iLCJuYW1lIjoiVGVzdCBDbGllbnQiLCJzZWN1cml0eV9zdGFtcCI6IlJBNU1QU081MkpJRjRDNlBaRklKVE1PNFlISjJEUFJSIiwianRpIjoiN2M5MmYzYWUtMDBhMy00ODhhLThmY2QtYzgwZWVhODAzYTMzIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQ2xpZW50IiwibmJmIjoxNzg2MTEzNzQwLCJleHAiOjE3ODYxMTQ2NDAsImlzcyI6IlNtYXJ0Q291cnRBUEkiLCJhdWQiOiJTbWFydENvdXJ0Q2xpZW50In0.7gak88UJmltumludC65LBBTs6NuBZwJp0akZd788Kkw","expiresIn":900,"refreshToken":"w2g1Gm+iYFYzNQiB5Gtk8wk+FxGxw6JdG0rXYFcfFxhaJW0ER6QAjYl/ZdOABk+bbc/wErL8u9/5ojpypVoiFA==","refreshTokenExpiration":"2026-08-14T14:42:20.4636091Z"},"message":null,"errors":null,"statusCode":200}
---

### 4. Login Lawyer

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
{
  "Email": "lawyer_test_1099231217@test.com",
  "Password": "Password123!"
}

**Response Status:** 200

**Response Body:**
{"success":true,"data":{"user":{"id":"7ebaeb4b-e06e-48f7-f789-08def48f6968","email":"lawyer_test_1099231217@test.com","fullName":"Test Lawyer","role":"Lawyer","status":"Unverified","rejectionReason":null},"accessToken":"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3ZWJhZWI0Yi1lMDZlLTQ4ZjctZjc4OS0wOGRlZjQ4ZjY5NjgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjdlYmFlYjRiLWUwNmUtNDhmNy1mNzg5LTA4ZGVmNDhmNjk2OCIsImVtYWlsIjoibGF3eWVyX3Rlc3RfMTA5OTIzMTIxN0B0ZXN0LmNvbSIsIm5hbWUiOiJUZXN0IExhd3llciIsInNlY3VyaXR5X3N0YW1wIjoiVkVDUUpNUE1IUTZMWjRIVk80U1dVWDdVTjJaTUFMNzQiLCJqdGkiOiIwM2U1ZTk5MS1kZWM5LTRhYmQtOGM1Yy1kMmQyMWRlOGQxNDEiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODYxMTM3NDAsImV4cCI6MTc4NjExNDY0MCwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.XdLwuzB7Ic75lObvTWON9l6z2WDcc0cE7pBbJrDDIzg","expiresIn":900,"refreshToken":"qPUPq9n0sbyYcE6O1UV+VeOqHfMHqfl9hy5dF/tn9UsWYrb6UG+Yu0EQnw4sxU3njlwhFao1LVxrPZiaI8uJlA==","refreshTokenExpiration":"2026-08-14T14:42:20.6258491Z"},"message":null,"errors":null,"statusCode":200}
---

### 8. Client Complete Profile - Valid Data

**Request:** POST http://localhost:5049/api/clients/profile/complete

**Body:**
{
  "Address": "Test Address",
  "NationalNumber": "29001014304533",
  "PhoneNumber": "+201000000000",
  "DateOfBirth": "1990-01-01",
  "Gender": 1
}

**Response Status:** 200

**Response Body:**
{"success":true,"message":"تم استكمال الملف الشخصي بنجاح.","errors":null,"statusCode":200}
---

### 9b. Re-Login Client

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
{
  "Email": "client_test_37823556@test.com",
  "Password": "Password123!"
}

**Response Status:** 200

**Response Body:**
{"success":true,"data":{"user":{"id":"c6b495a5-0c2e-4147-f788-08def48f6968","email":"client_test_37823556@test.com","fullName":"Test Client","role":"Client","status":"PendingReview","rejectionReason":null},"accessToken":"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJjNmI0OTVhNS0wYzJlLTQxNDctZjc4OC0wOGRlZjQ4ZjY5NjgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImM2YjQ5NWE1LTBjMmUtNDE0Ny1mNzg4LTA4ZGVmNDhmNjk2OCIsImVtYWlsIjoiY2xpZW50X3Rlc3RfMzc4MjM1NTZAdGVzdC5jb20iLCJuYW1lIjoiVGVzdCBDbGllbnQiLCJzZWN1cml0eV9zdGFtcCI6IkhCRVo0SFdTWlo3MkZBUFVPSVU0QzRQMllUTkZNVklMIiwianRpIjoiYTZmZDk1M2QtYWJiYS00M2VmLWJjMmItNmNmMjgwYjA5ZjM1IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQ2xpZW50IiwibmJmIjoxNzg2MTEzNzQxLCJleHAiOjE3ODYxMTQ2NDEsImlzcyI6IlNtYXJ0Q291cnRBUEkiLCJhdWQiOiJTbWFydENvdXJ0Q2xpZW50In0._usmUvX5bB7eV-zmr78yxKi-qSB8PBMWWhBQi3tUVgs","expiresIn":900,"refreshToken":"lUYRHZOE32/IwjB5uRGOLO06yM5N1KowWdYMyi0eIPCK/yWY9yaoHf1h1qDiWR+W3kcKfNa/M5REeR1cSNcCNw==","refreshTokenExpiration":"2026-08-14T14:42:21.4787775Z"},"message":null,"errors":null,"statusCode":200}
---

### 15. Lawyer Complete Profile - Valid Data

**Request:** POST http://localhost:5049/api/lawyers/profile/complete

**Body:**
{
  "PhoneNumber": "+201000000000",
  "DateOfBirth": "1990-01-01",
  "Gender": 1,
  "Level": 1,
  "NationalNumber": "29001011347387",
  "Bio": "Hello I am a lawyer",
  "Specializations": [
    {
      "YearsOfExperience": 5,
      "Specialization": 1,
      "CasesHandled": 10
    }
  ],
  "Address": "Law Firm 1"
}

**Response Status:** 200

**Response Body:**
{"success":true,"message":"تم استكمال البيانات بنجاح","errors":null,"statusCode":200}
---

### 16b. Re-Login Lawyer

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
{
  "Email": "lawyer_test_1099231217@test.com",
  "Password": "Password123!"
}

**Response Status:** 200

**Response Body:**
{"success":true,"data":{"user":{"id":"7ebaeb4b-e06e-48f7-f789-08def48f6968","email":"lawyer_test_1099231217@test.com","fullName":"Test Lawyer","role":"Lawyer","status":"PendingReview","rejectionReason":null},"accessToken":"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI3ZWJhZWI0Yi1lMDZlLTQ4ZjctZjc4OS0wOGRlZjQ4ZjY5NjgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjdlYmFlYjRiLWUwNmUtNDhmNy1mNzg5LTA4ZGVmNDhmNjk2OCIsImVtYWlsIjoibGF3eWVyX3Rlc3RfMTA5OTIzMTIxN0B0ZXN0LmNvbSIsIm5hbWUiOiJUZXN0IExhd3llciIsInNlY3VyaXR5X3N0YW1wIjoiRE9FNzZYUVBFN1o3RENBSkI0UUNXNk9PNU1ZUVpTSTciLCJqdGkiOiI0ZGQzODFkYy04MzQxLTRhYTktOTY2Ny1hNjU4MjFhNjA3NmUiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODYxMTM3NDIsImV4cCI6MTc4NjExNDY0MiwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.VqBTbnSTwyngrF_ktWlE8V6BQwgc8Nn3CmHLk4NwNvk","expiresIn":900,"refreshToken":"pCSa6B7jR5j0Sfbe794c9WEi3hiqVUAZ1v0IbpZQcptrc0HWscl+YxFldmGqMfCPwJB+zFjzpjm/hhXzD8Qj5A==","refreshTokenExpiration":"2026-08-14T14:42:22.0173433Z"},"message":null,"errors":null,"statusCode":200}
---

### 19. Client GET Profile - Ensure fields are correct

**Request:** GET http://localhost:5049/api/clients/profile

**Response Status:** 200

**Response Body:**
{"success":true,"data":{"id":"c6b495a5-0c2e-4147-f788-08def48f6968","name":"Test Client","email":"client_test_37823556@test.com","phoneNumber":"+201000000000","nationalNumber":"29001014304533","gender":1,"dateOfBirth":"1990-01-01","address":"Test Address","governorate":null,"city":null,"status":"PendingReview","rejectionReason":null},"message":null,"errors":null,"statusCode":200}
---

### 20. Lawyer GET Profile - Ensure fields are correct

**Request:** GET http://localhost:5049/api/lawyers/profile

**Response Status:** 200

**Response Body:**
{"success":true,"data":{"id":"7ebaeb4b-e06e-48f7-f789-08def48f6968","name":"Test Lawyer","email":"lawyer_test_1099231217@test.com","phoneNumber":"+201000000000","nationalNumber":"29001011347387","gender":1,"dateOfBirth":"1990-01-01","level":1,"yearsOfExperience":5,"specializationName":"CivilLaw","bio":"Hello I am a lawyer","address":"Law Firm 1","governorate":null,"city":null,"status":"PendingReview","isAvailable":true,"profilePictureUrl":null,"rejectionReason":null,"specializations":[{"specialization":1,"yearsOfExperience":5,"casesHandled":10}]},"message":null,"errors":null,"statusCode":200}
---


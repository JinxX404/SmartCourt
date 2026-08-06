# Authentication Extended Flow Test Report

### 1. Register Lawyer - Missing Name & Weak Password

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
`json
{
    "Email":  "lawyer_ext_797261284@test.com",
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
                   "FullName":  [
                                    "The FullName field is required.",
                                    "الاسم الكامل مطلوب."
                                ],
                   "Password":  [
                                    "كلمة المرور يجب أن تحتوي على حرف كبير وحرف صغير ورقم."
                                ]
               },
    "traceId":  "00-bfc9a3a5e1009b8944351aaa6f903043-45f33d33900eb8e7-00"
}
``n---


### 2. Register Lawyer - Valid

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
`json
{
    "Email":  "lawyer_ext_797261284@test.com",
    "FullName":  "Test Lawyer Ext",
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
                 "userId":  "1585af02-d567-4eca-e603-08def30d5f49",
                 "email":  "lawyer_ext_797261284@test.com",
                 "fullName":  "Test Lawyer Ext",
                 "role":  "Lawyer"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
``n---


### 3. Resend Verification

**Request:** POST http://localhost:5049/api/auth/resend-verification

**Body:**
`json
{
    "Email":  "lawyer_ext_797261284@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "message":  "تم إرسال رابط التحقق مرة أخرى",
    "errors":  null,
    "statusCode":  200
}
``n---


Found confirmation URL for lawyer_ext_797261284@test.com: http://localhost:5173/verify-email?userId=1585af02-d567-4eca-e603-08def30d5f49&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIrbWUvb3RpUXFtdGF5c3J2bi9Xa1dIeWJ1TDRHZDdwMkFJeStCMlJ5VEFTNFJ0dXBzSUt5TEpqbEF0enQxZDJUSGJycmoxT3A4QXRZZDU1emRvZmhvTHZrMW1KdTZQVDE5UkdHcHBMVG45b0d2NUcwRDAvZnU3bmJGM29CNEJUTnJKTGtTUUkvQnFCU1NQenpLVlRZUENYTUpVeDBOQXE1TFpUUytzNmVMbEtxWWN2R0JmMUFaYnZ2RzhFaWEyY2laNlQyWnBvRHhOMnRuWnloY1dqd2ZnRUhaRVdjUUxuUGd6Y0ZpR0lkeENEdz09

### Confirm Email for lawyer_ext_797261284@test.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=1585af02-d567-4eca-e603-08def30d5f49&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIrbWUvb3RpUXFtdGF5c3J2bi9Xa1dIeWJ1TDRHZDdwMkFJeStCMlJ5VEFTNFJ0dXBzSUt5TEpqbEF0enQxZDJUSGJycmoxT3A4QXRZZDU1emRvZmhvTHZrMW1KdTZQVDE5UkdHcHBMVG45b0d2NUcwRDAvZnU3bmJGM29CNEJUTnJKTGtTUUkvQnFCU1NQenpLVlRZUENYTUpVeDBOQXE1TFpUUytzNmVMbEtxWWN2R0JmMUFaYnZ2RzhFaWEyY2laNlQyWnBvRHhOMnRuWnloY1dqd2ZnRUhaRVdjUUxuUGd6Y0ZpR0lkeENEdz09

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


### 5. Login

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Password123!",
    "Email":  "lawyer_ext_797261284@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "1585af02-d567-4eca-e603-08def30d5f49",
                              "email":  "lawyer_ext_797261284@test.com",
                              "fullName":  "Test Lawyer Ext",
                              "role":  "Lawyer"
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNTg1YWYwMi1kNTY3LTRlY2EtZTYwMy0wOGRlZjMwZDVmNDkiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjE1ODVhZjAyLWQ1NjctNGVjYS1lNjAzLTA4ZGVmMzBkNWY0OSIsImVtYWlsIjoibGF3eWVyX2V4dF83OTcyNjEyODRAdGVzdC5jb20iLCJuYW1lIjoiVGVzdCBMYXd5ZXIgRXh0Iiwic2VjdXJpdHlfc3RhbXAiOiJaVU9SVDZIVlc1S0QyTURNTDJWTEtTUENSN0xYVlVMWiIsImp0aSI6IjA2YjM3MmU0LTI1NzgtNGYwZi04OWI1LWJlNzdkNmJmYWY3ZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NTk0NjgxNiwiZXhwIjoxNzg1OTUwNDE2LCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.6jQuuCbWPl3NUdU75v1ts0Di0XteAorizDTp83RObao",
                 "expiresIn":  3600,
                 "refreshToken":  "LMmwBqwfPSgv7LWQcVrsh4Hv1Z0pocqVwD9fFXxotTKBA5XfxpqmGdAvejVblGt8L1gyma8FLlRGQtig3jHhsQ==",
                 "refreshTokenExpiration":  "2026-08-12T16:20:16.2472287Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 6. Complete Lawyer Profile

**Request:** POST http://localhost:5049/api/lawyers/profile/complete

**Body:**
`json
{
    "Level":  1,
    "DateOfBirth":  "1990-01-01",
    "Gender":  1,
    "Address":  "Law Firm 1",
    "NationalNumber":  "29001016758887",
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


### 6b. Re-Login Lawyer (Token Refresh)

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Password123!",
    "Email":  "lawyer_ext_797261284@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "1585af02-d567-4eca-e603-08def30d5f49",
                              "email":  "lawyer_ext_797261284@test.com",
                              "fullName":  "Test Lawyer Ext",
                              "role":  "Lawyer"
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNTg1YWYwMi1kNTY3LTRlY2EtZTYwMy0wOGRlZjMwZDVmNDkiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjE1ODVhZjAyLWQ1NjctNGVjYS1lNjAzLTA4ZGVmMzBkNWY0OSIsImVtYWlsIjoibGF3eWVyX2V4dF83OTcyNjEyODRAdGVzdC5jb20iLCJuYW1lIjoiVGVzdCBMYXd5ZXIgRXh0Iiwic2VjdXJpdHlfc3RhbXAiOiJWWlNKQ0RDNlpUVFdGQ1NKQk5DN0VFRENLT0ZWVzRHTiIsImp0aSI6Ijg5MmNmMTFhLWU0M2ItNDRiZi04NDQ1LTJkMDc0MDViOWE4ZiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NTk0NjgxNiwiZXhwIjoxNzg1OTUwNDE2LCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.YhKzAZ4uzVTN1AsjczzSn5miSH7cFPBlkqBAHIJGqDU",
                 "expiresIn":  3600,
                 "refreshToken":  "7GWJohZSktcjeGM9WoZLrUvWVHLmbuiM/QbGwCa7c7aLSzYR/k8KsyxPu5XLI2xkWLYMBtWcCmf+kqvAlFcwXg==",
                 "refreshTokenExpiration":  "2026-08-12T16:20:16.623231Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 7. Change Password - Invalid Current Password

**Request:** POST http://localhost:5049/api/auth/change-password

**Body:**
`json
{
    "NewPassword":  "NewPassword123!",
    "ConfirmNewPassword":  "NewPassword123!",
    "CurrentPassword":  "WrongPassword!"
}
``n
**Response Status:** 400

**Response Body:**
`json
{
    "success":  false,
    "data":  null,
    "message":  "One or more validation failures have occurred.",
    "errors":  [
                   "CurrentPassword: كلمة المرور الحالية غير صحيحة."
               ],
    "statusCode":  400
}
``n---


### 8. Change Password - Valid

**Request:** POST http://localhost:5049/api/auth/change-password

**Body:**
`json
{
    "NewPassword":  "NewPassword123!",
    "ConfirmNewPassword":  "NewPassword123!",
    "CurrentPassword":  "Password123!"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "message":  "تم تغيير كلمة المرور بنجاح",
    "errors":  null,
    "statusCode":  200
}
``n---


### 8b. Re-Login Lawyer (New Password)

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "NewPassword123!",
    "Email":  "lawyer_ext_797261284@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "1585af02-d567-4eca-e603-08def30d5f49",
                              "email":  "lawyer_ext_797261284@test.com",
                              "fullName":  "Test Lawyer Ext",
                              "role":  "Lawyer"
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNTg1YWYwMi1kNTY3LTRlY2EtZTYwMy0wOGRlZjMwZDVmNDkiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjE1ODVhZjAyLWQ1NjctNGVjYS1lNjAzLTA4ZGVmMzBkNWY0OSIsImVtYWlsIjoibGF3eWVyX2V4dF83OTcyNjEyODRAdGVzdC5jb20iLCJuYW1lIjoiVGVzdCBMYXd5ZXIgRXh0Iiwic2VjdXJpdHlfc3RhbXAiOiIyNjY0QkxXRkc1VVdaM0NTNVRJRzdMRVU0N0pISEM1TSIsImp0aSI6IjUxN2QyMDEwLTY3ZDItNGFmNi1hN2I1LWVlMDdiY2NhY2M4OSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NTk0NjgxNywiZXhwIjoxNzg1OTUwNDE3LCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.B8MN-4CgL87ZV0FIQCYdzs2rzUTVQGA4RANRcvDUpH8",
                 "expiresIn":  3600,
                 "refreshToken":  "+PQP8DjoVQGN+alT71BsyOVjhRwSF2cdvPe/SDpq3E4Zv5twIiB586YGLHqnllnAnYfEndEaQYWDC1Uq4boUBA==",
                 "refreshTokenExpiration":  "2026-08-12T16:20:17.2111284Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 9. Refresh Token

**Request:** POST http://localhost:5049/api/auth/refresh

**Body:**
`json
{
    "AccessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNTg1YWYwMi1kNTY3LTRlY2EtZTYwMy0wOGRlZjMwZDVmNDkiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjE1ODVhZjAyLWQ1NjctNGVjYS1lNjAzLTA4ZGVmMzBkNWY0OSIsImVtYWlsIjoibGF3eWVyX2V4dF83OTcyNjEyODRAdGVzdC5jb20iLCJuYW1lIjoiVGVzdCBMYXd5ZXIgRXh0Iiwic2VjdXJpdHlfc3RhbXAiOiIyNjY0QkxXRkc1VVdaM0NTNVRJRzdMRVU0N0pISEM1TSIsImp0aSI6IjUxN2QyMDEwLTY3ZDItNGFmNi1hN2I1LWVlMDdiY2NhY2M4OSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NTk0NjgxNywiZXhwIjoxNzg1OTUwNDE3LCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.B8MN-4CgL87ZV0FIQCYdzs2rzUTVQGA4RANRcvDUpH8",
    "RefreshToken":  "+PQP8DjoVQGN+alT71BsyOVjhRwSF2cdvPe/SDpq3E4Zv5twIiB586YGLHqnllnAnYfEndEaQYWDC1Uq4boUBA=="
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNTg1YWYwMi1kNTY3LTRlY2EtZTYwMy0wOGRlZjMwZDVmNDkiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjE1ODVhZjAyLWQ1NjctNGVjYS1lNjAzLTA4ZGVmMzBkNWY0OSIsImVtYWlsIjoibGF3eWVyX2V4dF83OTcyNjEyODRAdGVzdC5jb20iLCJuYW1lIjoiVGVzdCBMYXd5ZXIgRXh0Iiwic2VjdXJpdHlfc3RhbXAiOiIyNjY0QkxXRkc1VVdaM0NTNVRJRzdMRVU0N0pISEM1TSIsImp0aSI6IjUzZmZjMmZmLWIwNWItNGJiYy1hZjlkLTc1NTIzZWE1MzdjNyIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NTk0NjgxNywiZXhwIjoxNzg1OTUwNDE3LCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.NPZoToGjpE6f1Pe5s2LYpoH8slo0wnU8vaXn5kS8q94",
                 "refreshToken":  "eQyOKNLNDGnkRPzbIHcBVmdH2LsGR+4SheKNu+5PHMi0zgaJqsafENEJawkXBLK+MdesuUt5HmV0Rvwp9dfuhg==",
                 "expiresAt":  "2026-08-12T16:20:17.3082613Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 10. Revoke Refresh Token

**Request:** POST http://localhost:5049/api/auth/revoke

**Body:**
`json
{
    "Token":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNTg1YWYwMi1kNTY3LTRlY2EtZTYwMy0wOGRlZjMwZDVmNDkiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjE1ODVhZjAyLWQ1NjctNGVjYS1lNjAzLTA4ZGVmMzBkNWY0OSIsImVtYWlsIjoibGF3eWVyX2V4dF83OTcyNjEyODRAdGVzdC5jb20iLCJuYW1lIjoiVGVzdCBMYXd5ZXIgRXh0Iiwic2VjdXJpdHlfc3RhbXAiOiIyNjY0QkxXRkc1VVdaM0NTNVRJRzdMRVU0N0pISEM1TSIsImp0aSI6IjUzZmZjMmZmLWIwNWItNGJiYy1hZjlkLTc1NTIzZWE1MzdjNyIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NTk0NjgxNywiZXhwIjoxNzg1OTUwNDE3LCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.NPZoToGjpE6f1Pe5s2LYpoH8slo0wnU8vaXn5kS8q94",
    "RefreshToken":  "eQyOKNLNDGnkRPzbIHcBVmdH2LsGR+4SheKNu+5PHMi0zgaJqsafENEJawkXBLK+MdesuUt5HmV0Rvwp9dfuhg=="
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  true,
    "message":  "تم إبطال رمز التحديث بنجاح.",
    "errors":  null,
    "statusCode":  200
}
``n---


### 11. Refresh Token - After Revocation (Should Fail)

**Request:** POST http://localhost:5049/api/auth/refresh

**Body:**
`json
{
    "AccessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNTg1YWYwMi1kNTY3LTRlY2EtZTYwMy0wOGRlZjMwZDVmNDkiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjE1ODVhZjAyLWQ1NjctNGVjYS1lNjAzLTA4ZGVmMzBkNWY0OSIsImVtYWlsIjoibGF3eWVyX2V4dF83OTcyNjEyODRAdGVzdC5jb20iLCJuYW1lIjoiVGVzdCBMYXd5ZXIgRXh0Iiwic2VjdXJpdHlfc3RhbXAiOiIyNjY0QkxXRkc1VVdaM0NTNVRJRzdMRVU0N0pISEM1TSIsImp0aSI6IjUzZmZjMmZmLWIwNWItNGJiYy1hZjlkLTc1NTIzZWE1MzdjNyIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NTk0NjgxNywiZXhwIjoxNzg1OTUwNDE3LCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.NPZoToGjpE6f1Pe5s2LYpoH8slo0wnU8vaXn5kS8q94",
    "RefreshToken":  "eQyOKNLNDGnkRPzbIHcBVmdH2LsGR+4SheKNu+5PHMi0zgaJqsafENEJawkXBLK+MdesuUt5HmV0Rvwp9dfuhg=="
}
``n
**Response Status:** 401

**Response Body:**
`json
{
    "success":  false,
    "data":  null,
    "message":  "رمز التحديث غير صالح أو منتهي الصلاحية.",
    "errors":  null,
    "statusCode":  401
}
``n---


### 11. Forgot Password

**Request:** POST http://localhost:5049/api/auth/forgot-password

**Body:**
`json
{
    "Email":  "lawyer_ext_797261284@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "message":  "إذا كان البريد الإلكتروني مسجلاً، سيتم إرسال رابط إعادة تعيين كلمة المرور",
    "errors":  null,
    "statusCode":  200
}
``n---


### 13. Reset Password

**Request:** POST http://localhost:5049/api/auth/reset-password

**Body:**
`json
{
    "Email":  "lawyer_ext_797261284@test.com",
    "ConfirmNewPassword":  "ResetPassword123!",
    "NewPassword":  "ResetPassword123!",
    "Token":  "Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIrL3ZqSkYyOHM0M2ZEeXJnL1RiUXpHRGRiMWxNQ0ZNZm9NSHA2amYzMmZCemI4a3pKOTVqNEVlTXUwSVQvVjZQK2EwY2duQmhob1Z6N2xEb2NaTVR2akdLUXQ4azRyd0xQYWc0ZksyUGdRS2lFc2VlMVJxRGdaZUpTQmRZbW1mdU1Ec2dZSjl6SlVOVkZNZ0hKd0FDcFl0U3hFb3JPNUtaclRtc05MbXozL1Z0dmt3WGJOazJsdHp4cVcxc1JESjZiMVFTZ2xNc0tmNmxGZ0M5RjF1R0N0"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "message":  "تم إعادة تعيين كلمة المرور بنجاح",
    "errors":  null,
    "statusCode":  200
}
``n---


### 14. Login - With Reset Password

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "ResetPassword123!",
    "Email":  "lawyer_ext_797261284@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "1585af02-d567-4eca-e603-08def30d5f49",
                              "email":  "lawyer_ext_797261284@test.com",
                              "fullName":  "Test Lawyer Ext",
                              "role":  "Lawyer"
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxNTg1YWYwMi1kNTY3LTRlY2EtZTYwMy0wOGRlZjMwZDVmNDkiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjE1ODVhZjAyLWQ1NjctNGVjYS1lNjAzLTA4ZGVmMzBkNWY0OSIsImVtYWlsIjoibGF3eWVyX2V4dF83OTcyNjEyODRAdGVzdC5jb20iLCJuYW1lIjoiVGVzdCBMYXd5ZXIgRXh0Iiwic2VjdXJpdHlfc3RhbXAiOiI3Wk5YSDdVTlpVU1hPSlRJQkdSUTZZTEJCTElEVFAzSCIsImp0aSI6ImUzNjUzOTE2LTg5MTktNGI2MS1iYzBhLWIxYzRiODc3ZWVhOCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NTk0NjgyMCwiZXhwIjoxNzg1OTUwNDIwLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.MQRpCeyNMUwRKImMJwge3wSBGlrHCgsHIcvWHF4GMa8",
                 "expiresIn":  3600,
                 "refreshToken":  "dIYKJL1j1C2cbZb/2mFjG5pv3sBKzcAL0aeO7FTdR+WoxHk4c6YRLWUaVfPG09AZNV1slRkL3RFoA/9CcK5Hkw==",
                 "refreshTokenExpiration":  "2026-08-12T16:20:20.6854357Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---



# Authentication Extended Flow Test Report

### 1. Register Lawyer - Missing Name & Weak Password

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
`json
{
  "Password": "password",
  "Email": "lawyer_ext_518724157@test.com",
  "ConfirmPassword": "password"
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
    "FullName": [
      "The FullName field is required.",
      "الاسم الكامل مطلوب."
    ],
    "Password": [
      "كلمة المرور يجب أن تحتوي على حرف كبير وحرف صغير ورقم."
    ]
  },
  "traceId": "00-f4b62fea08259d77f6e69d6839b728e8-6efedfe70970e2f5-00"
}
``n---


### 2. Register Lawyer - Valid

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
`json
{
  "Password": "Password123!",
  "FullName": "Test Lawyer Ext",
  "ConfirmPassword": "Password123!",
  "Email": "lawyer_ext_518724157@test.com"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "182da6db-6e4b-42cf-f77f-08def48f6968",
    "email": "lawyer_ext_518724157@test.com",
    "fullName": "Test Lawyer Ext",
    "role": "Lawyer"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


### 3. Resend Verification

**Request:** POST http://localhost:5049/api/auth/resend-verification

**Body:**
`json
{
  "Email": "lawyer_ext_518724157@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "message": "تم إرسال رابط التحقق مرة أخرى",
  "errors": null,
  "statusCode": 200
}
``n---


Found confirmation URL for lawyer_ext_518724157@test.com: http://localhost:5173/verify-email?userId=182da6db-6e4b-42cf-f77f-08def48f6968&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIrQTRKcXNEOVhyODM0WFc4NGI5UTNGbGlXWThNSHR6cnAreXB5YlplUUwwOXg3VThQdWNhaFVMYkNuMzBCa003Z3FKdGZ5SGFyMFNDTy9rN05NbGFhV1V1WkVVcUdnYTNQY0ZYNVVQRnJZNlRGaUwrWXd4bkMwdHd2Q0tZVGhZT3hUZUYxelB2RTA2QUwvbEx5a0d1azNBcEZ2QnJ0NXFZNUEyaVVrdUNDeXZCNENXNGtva2F6NFlnRnRIOWh5T2JDS0V3UnVXdzYyQUdjeHVJYnNlQ1pvb0dWZVRoRzNUZVQ1MEtHdXh3MG9Idz09

### Confirm Email for lawyer_ext_518724157@test.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=182da6db-6e4b-42cf-f77f-08def48f6968&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIrQTRKcXNEOVhyODM0WFc4NGI5UTNGbGlXWThNSHR6cnAreXB5YlplUUwwOXg3VThQdWNhaFVMYkNuMzBCa003Z3FKdGZ5SGFyMFNDTy9rN05NbGFhV1V1WkVVcUdnYTNQY0ZYNVVQRnJZNlRGaUwrWXd4bkMwdHd2Q0tZVGhZT3hUZUYxelB2RTA2QUwvbEx5a0d1azNBcEZ2QnJ0NXFZNUEyaVVrdUNDeXZCNENXNGtva2F6NFlnRnRIOWh5T2JDS0V3UnVXdzYyQUdjeHVJYnNlQ1pvb0dWZVRoRzNUZVQ1MEtHdXh3MG9Idz09

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


### 5. Login

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Password": "Password123!",
  "Email": "lawyer_ext_518724157@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "182da6db-6e4b-42cf-f77f-08def48f6968",
      "email": "lawyer_ext_518724157@test.com",
      "fullName": "Test Lawyer Ext",
      "role": "Lawyer",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxODJkYTZkYi02ZTRiLTQyY2YtZjc3Zi0wOGRlZjQ4ZjY5NjgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjE4MmRhNmRiLTZlNGItNDJjZi1mNzdmLTA4ZGVmNDhmNjk2OCIsImVtYWlsIjoibGF3eWVyX2V4dF81MTg3MjQxNTdAdGVzdC5jb20iLCJuYW1lIjoiVGVzdCBMYXd5ZXIgRXh0Iiwic2VjdXJpdHlfc3RhbXAiOiJEUE5LV0xUSEkzV0tUN0VXUURNNkFWUEg0MjZSS0FRNiIsImp0aSI6IjZjY2MxNjcxLTBkNWUtNDY3Yi1iMWM5LWViNWZhZjExN2E0YiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NjExMzYwOSwiZXhwIjoxNzg2MTE0NTA5LCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.zTeComc7YMMKnhdh9lI8zzqXF9KiQQDVEV6Wt2TIkK8",
    "expiresIn": 900,
    "refreshToken": "1aKu/aws3qfSvFuRMABLHk7SKm6UoZ0jDGFYm6FekAGAh8pwx1wIG5wcx3vCUpcoDhzqyXq672Eetcs8uWIRWQ==",
    "refreshTokenExpiration": "2026-08-14T14:40:09.0275027Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 6. Complete Lawyer Profile

**Request:** POST http://localhost:5049/api/lawyers/profile/complete

**Body:**
`json
{
  "Specializations": [
    {
      "YearsOfExperience": 5,
      "CasesHandled": 10,
      "Specialization": 1
    }
  ],
  "NationalNumber": "29001019086916",
  "PhoneNumber": "+201011111111",
  "DateOfBirth": "1990-01-01",
  "Level": 1,
  "Gender": 1,
  "Address": "Law Firm 1",
  "Bio": "Hello"
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


### 6b. Re-Login Lawyer (Token Refresh)

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Password": "Password123!",
  "Email": "lawyer_ext_518724157@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "182da6db-6e4b-42cf-f77f-08def48f6968",
      "email": "lawyer_ext_518724157@test.com",
      "fullName": "Test Lawyer Ext",
      "role": "Lawyer",
      "status": "PendingReview",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxODJkYTZkYi02ZTRiLTQyY2YtZjc3Zi0wOGRlZjQ4ZjY5NjgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjE4MmRhNmRiLTZlNGItNDJjZi1mNzdmLTA4ZGVmNDhmNjk2OCIsImVtYWlsIjoibGF3eWVyX2V4dF81MTg3MjQxNTdAdGVzdC5jb20iLCJuYW1lIjoiVGVzdCBMYXd5ZXIgRXh0Iiwic2VjdXJpdHlfc3RhbXAiOiJYRU1aSlNJSFk1U1ZPVE5URVkyVjdVNlRONFVKTk8zUyIsImp0aSI6IjRiNDkyNDNjLTZlZjMtNDVmZS05MWQxLTA1NmRhZGE4NzFkMiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NjExMzYwOSwiZXhwIjoxNzg2MTE0NTA5LCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.ZC5ojYxqnqKE7cnSUuH7k8GKvMBVneVilWgRB_CcPSo",
    "expiresIn": 900,
    "refreshToken": "ZMx3mLwqtBI+FLQsRDABQ+Nt39GUS1nzXU5H619RwMDBMSjZRe5wpz2buah1uKnjwOd25h1/yfpFADdrJt6Bog==",
    "refreshTokenExpiration": "2026-08-14T14:40:09.44709Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 7. Change Password - Invalid Current Password

**Request:** POST http://localhost:5049/api/auth/change-password

**Body:**
`json
{
  "CurrentPassword": "WrongPassword!",
  "ConfirmNewPassword": "NewPassword123!",
  "NewPassword": "NewPassword123!"
}
``n
**Response Status:** 400

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "One or more validation failures have occurred.",
  "errors": [
    "CurrentPassword: كلمة المرور الحالية غير صحيحة."
  ],
  "statusCode": 400
}
``n---


### 8. Change Password - Valid

**Request:** POST http://localhost:5049/api/auth/change-password

**Body:**
`json
{
  "CurrentPassword": "Password123!",
  "ConfirmNewPassword": "NewPassword123!",
  "NewPassword": "NewPassword123!"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "message": "تم تغيير كلمة المرور بنجاح",
  "errors": null,
  "statusCode": 200
}
``n---


### 8b. Re-Login Lawyer (New Password)

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Password": "NewPassword123!",
  "Email": "lawyer_ext_518724157@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "182da6db-6e4b-42cf-f77f-08def48f6968",
      "email": "lawyer_ext_518724157@test.com",
      "fullName": "Test Lawyer Ext",
      "role": "Lawyer",
      "status": "PendingReview",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxODJkYTZkYi02ZTRiLTQyY2YtZjc3Zi0wOGRlZjQ4ZjY5NjgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjE4MmRhNmRiLTZlNGItNDJjZi1mNzdmLTA4ZGVmNDhmNjk2OCIsImVtYWlsIjoibGF3eWVyX2V4dF81MTg3MjQxNTdAdGVzdC5jb20iLCJuYW1lIjoiVGVzdCBMYXd5ZXIgRXh0Iiwic2VjdXJpdHlfc3RhbXAiOiIzTVFJWTZJVTJONVQ0QUtENkhHT1BMUkhGMk03RVI2TyIsImp0aSI6IjZhNWEzNjMzLTc3NmMtNDMxNC04YjdmLTA0OWQ2MzU5NmFiMiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NjExMzYxMCwiZXhwIjoxNzg2MTE0NTEwLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.My8sbd60atcrhz2npRDy6BD7-biqOsTex4nsjryxFq8",
    "expiresIn": 900,
    "refreshToken": "dNVDX8vVyAkPwu8Iz5K49zXQ7qVABzgvxoC2PS31W+pC3QMciCUEmqtSnn+9edoCsbkvsf84zh4W1OmSDNeIAA==",
    "refreshTokenExpiration": "2026-08-14T14:40:10.5664255Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 9. Refresh Token

**Request:** POST http://localhost:5049/api/auth/refresh

**Body:**
`json
{
  "RefreshToken": "dNVDX8vVyAkPwu8Iz5K49zXQ7qVABzgvxoC2PS31W+pC3QMciCUEmqtSnn+9edoCsbkvsf84zh4W1OmSDNeIAA==",
  "AccessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxODJkYTZkYi02ZTRiLTQyY2YtZjc3Zi0wOGRlZjQ4ZjY5NjgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjE4MmRhNmRiLTZlNGItNDJjZi1mNzdmLTA4ZGVmNDhmNjk2OCIsImVtYWlsIjoibGF3eWVyX2V4dF81MTg3MjQxNTdAdGVzdC5jb20iLCJuYW1lIjoiVGVzdCBMYXd5ZXIgRXh0Iiwic2VjdXJpdHlfc3RhbXAiOiIzTVFJWTZJVTJONVQ0QUtENkhHT1BMUkhGMk03RVI2TyIsImp0aSI6IjZhNWEzNjMzLTc3NmMtNDMxNC04YjdmLTA0OWQ2MzU5NmFiMiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NjExMzYxMCwiZXhwIjoxNzg2MTE0NTEwLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.My8sbd60atcrhz2npRDy6BD7-biqOsTex4nsjryxFq8"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxODJkYTZkYi02ZTRiLTQyY2YtZjc3Zi0wOGRlZjQ4ZjY5NjgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjE4MmRhNmRiLTZlNGItNDJjZi1mNzdmLTA4ZGVmNDhmNjk2OCIsImVtYWlsIjoibGF3eWVyX2V4dF81MTg3MjQxNTdAdGVzdC5jb20iLCJuYW1lIjoiVGVzdCBMYXd5ZXIgRXh0Iiwic2VjdXJpdHlfc3RhbXAiOiIzTVFJWTZJVTJONVQ0QUtENkhHT1BMUkhGMk03RVI2TyIsImp0aSI6IjU1NDJmY2Y2LTM4ZDEtNDhkMi05ODZmLWNkYjg2MGJiMzA3YiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NjExMzYxMCwiZXhwIjoxNzg2MTE0NTEwLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.obbAJcncIGNHpb7lRFIy9SUZaxfHeUbwf2EngOLZvb4",
    "accessTokenExpiresInSeconds": 900,
    "refreshToken": "utZU4Pp0F1PIcZjOEXo84a5wNGN5qRFD+/mf9MVaROiFMEnlJTd2SF6V9QC24ff5LF/LJNy88kUIjrlAhS6xjw==",
    "expiresAt": "2026-08-14T14:40:10.7806482Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 10. Revoke Refresh Token

**Request:** POST http://localhost:5049/api/auth/revoke

**Body:**
`json
{
  "RefreshToken": "utZU4Pp0F1PIcZjOEXo84a5wNGN5qRFD+/mf9MVaROiFMEnlJTd2SF6V9QC24ff5LF/LJNy88kUIjrlAhS6xjw==",
  "Token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxODJkYTZkYi02ZTRiLTQyY2YtZjc3Zi0wOGRlZjQ4ZjY5NjgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjE4MmRhNmRiLTZlNGItNDJjZi1mNzdmLTA4ZGVmNDhmNjk2OCIsImVtYWlsIjoibGF3eWVyX2V4dF81MTg3MjQxNTdAdGVzdC5jb20iLCJuYW1lIjoiVGVzdCBMYXd5ZXIgRXh0Iiwic2VjdXJpdHlfc3RhbXAiOiIzTVFJWTZJVTJONVQ0QUtENkhHT1BMUkhGMk03RVI2TyIsImp0aSI6IjU1NDJmY2Y2LTM4ZDEtNDhkMi05ODZmLWNkYjg2MGJiMzA3YiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NjExMzYxMCwiZXhwIjoxNzg2MTE0NTEwLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.obbAJcncIGNHpb7lRFIy9SUZaxfHeUbwf2EngOLZvb4"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": true,
  "message": "تم إبطال رمز التحديث بنجاح.",
  "errors": null,
  "statusCode": 200
}
``n---


### 11. Refresh Token - After Revocation (Should Fail)

**Request:** POST http://localhost:5049/api/auth/refresh

**Body:**
`json
{
  "RefreshToken": "utZU4Pp0F1PIcZjOEXo84a5wNGN5qRFD+/mf9MVaROiFMEnlJTd2SF6V9QC24ff5LF/LJNy88kUIjrlAhS6xjw==",
  "AccessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxODJkYTZkYi02ZTRiLTQyY2YtZjc3Zi0wOGRlZjQ4ZjY5NjgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjE4MmRhNmRiLTZlNGItNDJjZi1mNzdmLTA4ZGVmNDhmNjk2OCIsImVtYWlsIjoibGF3eWVyX2V4dF81MTg3MjQxNTdAdGVzdC5jb20iLCJuYW1lIjoiVGVzdCBMYXd5ZXIgRXh0Iiwic2VjdXJpdHlfc3RhbXAiOiIzTVFJWTZJVTJONVQ0QUtENkhHT1BMUkhGMk03RVI2TyIsImp0aSI6IjU1NDJmY2Y2LTM4ZDEtNDhkMi05ODZmLWNkYjg2MGJiMzA3YiIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NjExMzYxMCwiZXhwIjoxNzg2MTE0NTEwLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.obbAJcncIGNHpb7lRFIy9SUZaxfHeUbwf2EngOLZvb4"
}
``n
**Response Status:** 401

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "رمز التحديث غير صالح أو منتهي الصلاحية.",
  "errors": null,
  "statusCode": 401
}
``n---


### 11. Forgot Password

**Request:** POST http://localhost:5049/api/auth/forgot-password

**Body:**
`json
{
  "Email": "lawyer_ext_518724157@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "message": "إذا كان البريد الإلكتروني مسجلاً، سيتم إرسال رابط إعادة تعيين كلمة المرور",
  "errors": null,
  "statusCode": 200
}
``n---


### 13. Reset Password

**Request:** POST http://localhost:5049/api/auth/reset-password

**Body:**
`json
{
  "Token": "Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI4TkJGWFBBUitQb0hZS1JGY2Z0Y0Rnc1hYNjd2eVJjZ0FhVlZXM0tvbFhBNXNwQWJNNVp2SWVKZ2Q4UFFzYmVHTGZYUnR1VlpwWXRrcjlWcGlUR2ZuOU8zV3B5WGVSUURDZUVJOWxPb1c5Z2tsK0ZuaWFLYzRtS1E2TE50MlZiREd0NUNlUUNOa1VEMTMxTzdXTjBLMkF1UFAwQTFkL280c2xzbTM5bk9mcTQwOURBMUpRR29Fc1UyMU11NXF1OVRSMG1pTzhpeWFjM2VlVFg3VVE3ZVB0",
  "Email": "lawyer_ext_518724157@test.com",
  "ConfirmNewPassword": "ResetPassword123!",
  "NewPassword": "ResetPassword123!"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "message": "تم إعادة تعيين كلمة المرور بنجاح",
  "errors": null,
  "statusCode": 200
}
``n---


### 14. Login - With Reset Password

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Password": "ResetPassword123!",
  "Email": "lawyer_ext_518724157@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "182da6db-6e4b-42cf-f77f-08def48f6968",
      "email": "lawyer_ext_518724157@test.com",
      "fullName": "Test Lawyer Ext",
      "role": "Lawyer",
      "status": "PendingReview",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxODJkYTZkYi02ZTRiLTQyY2YtZjc3Zi0wOGRlZjQ4ZjY5NjgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjE4MmRhNmRiLTZlNGItNDJjZi1mNzdmLTA4ZGVmNDhmNjk2OCIsImVtYWlsIjoibGF3eWVyX2V4dF81MTg3MjQxNTdAdGVzdC5jb20iLCJuYW1lIjoiVGVzdCBMYXd5ZXIgRXh0Iiwic2VjdXJpdHlfc3RhbXAiOiJLNVFJU1FEU0RBUzZFVTNQWkk0T1A2RkRRNTRQWlRETyIsImp0aSI6IjVjNjdhNmEyLTRhM2EtNGRjNi1iMGYzLTJlMGRmYTI5MzYxNCIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NjExMzYxNSwiZXhwIjoxNzg2MTE0NTE1LCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.L9yjWI7JvzxT-SYCUdihHUo-9sKiX8_gl5ZKSJx67Do",
    "expiresIn": 900,
    "refreshToken": "1rKXEWZ7dES+bdS19gK8O8BdzH3LOpr+zeg0Q7U2bpd42eeQdfLfETERSu9fIqkfLG0d+8+JCYbEaTH+ujfjlg==",
    "refreshTokenExpiration": "2026-08-14T14:40:15.1103557Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---



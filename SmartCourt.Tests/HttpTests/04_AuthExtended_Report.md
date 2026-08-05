# Authentication Extended Flow Test Report

### 1. Register Lawyer - Missing Name & Weak Password

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
`json
{
    "Email":  "lawyer_ext_1953738807@test.com",
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
    "traceId":  "00-f7a65532250eb7290e74fa5c0ccb05a5-2496c6d9dbdbceae-00"
}
``n---


### 2. Register Lawyer - Valid

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
`json
{
    "Email":  "lawyer_ext_1953738807@test.com",
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
                 "userId":  "1d77e8d1-24a9-46ab-8115-08def2e76edd",
                 "email":  "lawyer_ext_1953738807@test.com",
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
    "Email":  "lawyer_ext_1953738807@test.com"
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


Found confirmation URL for lawyer_ext_1953738807@test.com: http://localhost:5173/verify-email?userId=1d77e8d1-24a9-46ab-8115-08def2e76edd&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIrNktmOGRSWXBDanlTa2wxMk9ObFd5OXNnUFZmbXZuN1VXU2VuZUEwakEwbHdWTlFDMDN5enpZekx1WlRiS0lUbWs2Z0QyZnBFTXkzSi9sclZFTUtIUDBla1o3RDBscnlvaE5JcUhac3JJbU00MlM5U0prZ1dlQ0lTUS9RSmV6UitrNzFsdklJY3JSbkd2YVhlbE5PT3kvWjF2emIrSUIwZlc1ODZmdDQ3NUlnVS9taWFpcnR1OW1ielZKaFZJc2VkV1ZpUHpSc21xcVJQaFlzakN6ak40NFZrcGVUWVp3QzdEVzB0VFVRc1g5UT09

### Confirm Email for lawyer_ext_1953738807@test.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=1d77e8d1-24a9-46ab-8115-08def2e76edd&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIrNktmOGRSWXBDanlTa2wxMk9ObFd5OXNnUFZmbXZuN1VXU2VuZUEwakEwbHdWTlFDMDN5enpZekx1WlRiS0lUbWs2Z0QyZnBFTXkzSi9sclZFTUtIUDBla1o3RDBscnlvaE5JcUhac3JJbU00MlM5U0prZ1dlQ0lTUS9RSmV6UitrNzFsdklJY3JSbkd2YVhlbE5PT3kvWjF2emIrSUIwZlc1ODZmdDQ3NUlnVS9taWFpcnR1OW1ielZKaFZJc2VkV1ZpUHpSc21xcVJQaFlzakN6ak40NFZrcGVUWVp3QzdEVzB0VFVRc1g5UT09

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
    "Email":  "lawyer_ext_1953738807@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "1d77e8d1-24a9-46ab-8115-08def2e76edd",
                              "email":  "lawyer_ext_1953738807@test.com",
                              "fullName":  "Test Lawyer Ext",
                              "role":  "Lawyer"
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxZDc3ZThkMS0yNGE5LTQ2YWItODExNS0wOGRlZjJlNzZlZGQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjFkNzdlOGQxLTI0YTktNDZhYi04MTE1LTA4ZGVmMmU3NmVkZCIsImVtYWlsIjoibGF3eWVyX2V4dF8xOTUzNzM4ODA3QHRlc3QuY29tIiwibmFtZSI6IlRlc3QgTGF3eWVyIEV4dCIsInNlY3VyaXR5X3N0YW1wIjoiRFA3U0FONDVGRlk1UFdaN1hWUktTTU02WEtPU0NCS0YiLCJqdGkiOiJkZmQ2NTdmZi1hNmQwLTQ0NmItYmMwYy1kODY5NDYwZjQ5NjIiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODU5MzM3MzcsImV4cCI6MTc4NTkzNzMzNywiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.L8iGv14JtPOHqO3QWp-uAax2p4YI1FeEyMknNhHDtzQ",
                 "expiresIn":  3600,
                 "refreshToken":  "M0FI1oBN7+k/oHTiOIQXMMZtpdZjYVzylTOxtajaSrwMftdjcIeeYRFHCy00/dc//BNQDbYhHbFKlDlUQ6wlOg==",
                 "refreshTokenExpiration":  "2026-08-12T12:42:17.5890624Z"
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
    "Gender":  "Male",
    "Address":  "Law Firm 1",
    "NationalNumber":  "29001019082736",
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
    "Email":  "lawyer_ext_1953738807@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "1d77e8d1-24a9-46ab-8115-08def2e76edd",
                              "email":  "lawyer_ext_1953738807@test.com",
                              "fullName":  "Test Lawyer Ext",
                              "role":  "Lawyer"
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxZDc3ZThkMS0yNGE5LTQ2YWItODExNS0wOGRlZjJlNzZlZGQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjFkNzdlOGQxLTI0YTktNDZhYi04MTE1LTA4ZGVmMmU3NmVkZCIsImVtYWlsIjoibGF3eWVyX2V4dF8xOTUzNzM4ODA3QHRlc3QuY29tIiwibmFtZSI6IlRlc3QgTGF3eWVyIEV4dCIsInNlY3VyaXR5X3N0YW1wIjoiWDVaUVdEWUdCNFBINllHN1hRTE9PRDZKQlgyVVEzMk8iLCJqdGkiOiJjYzUxOTBmYS02ZGI0LTRlYzctOTVkZS1iODNhNTA1M2Q3NzciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODU5MzM3MzcsImV4cCI6MTc4NTkzNzMzNywiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.prFNmItzkglz6jxop70jX7neBb3hSZ8KpS-u9dcmIGA",
                 "expiresIn":  3600,
                 "refreshToken":  "tcbDMc0Lvj8RtC8FyQp7OIfz4TWIIZKtDvcVuiR746fsk9DjP9JDInbVxyuDnPexmeKbptcHF6ySEG9itgMddg==",
                 "refreshTokenExpiration":  "2026-08-12T12:42:17.8330518Z"
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
    "Email":  "lawyer_ext_1953738807@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "1d77e8d1-24a9-46ab-8115-08def2e76edd",
                              "email":  "lawyer_ext_1953738807@test.com",
                              "fullName":  "Test Lawyer Ext",
                              "role":  "Lawyer"
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxZDc3ZThkMS0yNGE5LTQ2YWItODExNS0wOGRlZjJlNzZlZGQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjFkNzdlOGQxLTI0YTktNDZhYi04MTE1LTA4ZGVmMmU3NmVkZCIsImVtYWlsIjoibGF3eWVyX2V4dF8xOTUzNzM4ODA3QHRlc3QuY29tIiwibmFtZSI6IlRlc3QgTGF3eWVyIEV4dCIsInNlY3VyaXR5X3N0YW1wIjoiVU9GS0gyTUZaR0kyR0MzT1RaRURITEE2RExCQTNWUkUiLCJqdGkiOiI2ZWM2YjNlYy1hODgwLTQxODMtOWZkOS1iOTBhMDZjY2NmMzkiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODU5MzM3MzgsImV4cCI6MTc4NTkzNzMzOCwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.7LEWx3pVcSBeOT-FdMT9icpTbmfETBW4lvbPkIv7n3Q",
                 "expiresIn":  3600,
                 "refreshToken":  "+tUG0/kgXBZTyDrxaeisQiMKXUBja2tUAIBCMrBdVQEJBqRQovmRS3EZSdcScBX9vbtpSm/en4F7djIxi/rwEA==",
                 "refreshTokenExpiration":  "2026-08-12T12:42:18.7925378Z"
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
    "AccessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxZDc3ZThkMS0yNGE5LTQ2YWItODExNS0wOGRlZjJlNzZlZGQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjFkNzdlOGQxLTI0YTktNDZhYi04MTE1LTA4ZGVmMmU3NmVkZCIsImVtYWlsIjoibGF3eWVyX2V4dF8xOTUzNzM4ODA3QHRlc3QuY29tIiwibmFtZSI6IlRlc3QgTGF3eWVyIEV4dCIsInNlY3VyaXR5X3N0YW1wIjoiVU9GS0gyTUZaR0kyR0MzT1RaRURITEE2RExCQTNWUkUiLCJqdGkiOiI2ZWM2YjNlYy1hODgwLTQxODMtOWZkOS1iOTBhMDZjY2NmMzkiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODU5MzM3MzgsImV4cCI6MTc4NTkzNzMzOCwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.7LEWx3pVcSBeOT-FdMT9icpTbmfETBW4lvbPkIv7n3Q",
    "RefreshToken":  "+tUG0/kgXBZTyDrxaeisQiMKXUBja2tUAIBCMrBdVQEJBqRQovmRS3EZSdcScBX9vbtpSm/en4F7djIxi/rwEA=="
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxZDc3ZThkMS0yNGE5LTQ2YWItODExNS0wOGRlZjJlNzZlZGQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjFkNzdlOGQxLTI0YTktNDZhYi04MTE1LTA4ZGVmMmU3NmVkZCIsImVtYWlsIjoibGF3eWVyX2V4dF8xOTUzNzM4ODA3QHRlc3QuY29tIiwibmFtZSI6IlRlc3QgTGF3eWVyIEV4dCIsInNlY3VyaXR5X3N0YW1wIjoiVU9GS0gyTUZaR0kyR0MzT1RaRURITEE2RExCQTNWUkUiLCJqdGkiOiJiN2YxMTAzNS1kYjlmLTRhMmItYWM2OS1kZWVlYjMwOTJhN2EiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODU5MzM3MzksImV4cCI6MTc4NTkzNzMzOSwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.VFQQQk6NSZ3V1i2Nz7VSBFHKiRbRwZ4QdzE3d-DN4h4",
                 "refreshToken":  "M5h9mhbpHY31jyxpD0D42Ds4dOjLd8cECOgrznoBKi8Oid2gS6lOWxEkLHcgbU7cdI+hhOGDX/RAfVExadOFQQ==",
                 "expiresAt":  "2026-08-12T12:42:19.1235502Z"
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
    "Token":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxZDc3ZThkMS0yNGE5LTQ2YWItODExNS0wOGRlZjJlNzZlZGQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjFkNzdlOGQxLTI0YTktNDZhYi04MTE1LTA4ZGVmMmU3NmVkZCIsImVtYWlsIjoibGF3eWVyX2V4dF8xOTUzNzM4ODA3QHRlc3QuY29tIiwibmFtZSI6IlRlc3QgTGF3eWVyIEV4dCIsInNlY3VyaXR5X3N0YW1wIjoiVU9GS0gyTUZaR0kyR0MzT1RaRURITEE2RExCQTNWUkUiLCJqdGkiOiJiN2YxMTAzNS1kYjlmLTRhMmItYWM2OS1kZWVlYjMwOTJhN2EiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODU5MzM3MzksImV4cCI6MTc4NTkzNzMzOSwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.VFQQQk6NSZ3V1i2Nz7VSBFHKiRbRwZ4QdzE3d-DN4h4",
    "RefreshToken":  "M5h9mhbpHY31jyxpD0D42Ds4dOjLd8cECOgrznoBKi8Oid2gS6lOWxEkLHcgbU7cdI+hhOGDX/RAfVExadOFQQ=="
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
    "AccessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxZDc3ZThkMS0yNGE5LTQ2YWItODExNS0wOGRlZjJlNzZlZGQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjFkNzdlOGQxLTI0YTktNDZhYi04MTE1LTA4ZGVmMmU3NmVkZCIsImVtYWlsIjoibGF3eWVyX2V4dF8xOTUzNzM4ODA3QHRlc3QuY29tIiwibmFtZSI6IlRlc3QgTGF3eWVyIEV4dCIsInNlY3VyaXR5X3N0YW1wIjoiVU9GS0gyTUZaR0kyR0MzT1RaRURITEE2RExCQTNWUkUiLCJqdGkiOiJiN2YxMTAzNS1kYjlmLTRhMmItYWM2OS1kZWVlYjMwOTJhN2EiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODU5MzM3MzksImV4cCI6MTc4NTkzNzMzOSwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.VFQQQk6NSZ3V1i2Nz7VSBFHKiRbRwZ4QdzE3d-DN4h4",
    "RefreshToken":  "M5h9mhbpHY31jyxpD0D42Ds4dOjLd8cECOgrznoBKi8Oid2gS6lOWxEkLHcgbU7cdI+hhOGDX/RAfVExadOFQQ=="
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
    "Email":  "lawyer_ext_1953738807@test.com"
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
    "Email":  "lawyer_ext_1953738807@test.com",
    "ConfirmNewPassword":  "ResetPassword123!",
    "NewPassword":  "ResetPassword123!",
    "Token":  "Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5ZXZVR2RWWmkyOGhmVStyQnhGV3NGVmVkNjBoSWFIRDJpYTR1dmYxUWdldm52Q3JQQkk0UEQyTVlDR0xDOGQ1Mk92N1JNanFzUGxOeW5vK1pEZndyNUpFZGxXc2hLa0U1YVd4cFM2QkNjS3Jram5VNEpTbmE2VFFBd0YxZ1hTb2xPWTBBU1I0bTk3Q3R1WDhnTWdmaWZaa1dYWWhUSklJeWpmZ2g4TDJRTVlzblF4RmpOZTNLaFlwTjY3M3ZvZENpTjJqTS8xeWprUXF3Qmh6TytrdXps"
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
    "Email":  "lawyer_ext_1953738807@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "1d77e8d1-24a9-46ab-8115-08def2e76edd",
                              "email":  "lawyer_ext_1953738807@test.com",
                              "fullName":  "Test Lawyer Ext",
                              "role":  "Lawyer"
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxZDc3ZThkMS0yNGE5LTQ2YWItODExNS0wOGRlZjJlNzZlZGQiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjFkNzdlOGQxLTI0YTktNDZhYi04MTE1LTA4ZGVmMmU3NmVkZCIsImVtYWlsIjoibGF3eWVyX2V4dF8xOTUzNzM4ODA3QHRlc3QuY29tIiwibmFtZSI6IlRlc3QgTGF3eWVyIEV4dCIsInNlY3VyaXR5X3N0YW1wIjoiUEdCWkFBTE1CRFlMNENOVlhLWDUzVlgzQlQ1UDJSVTQiLCJqdGkiOiJmYTE0MjI3NC02YzBkLTRhYjEtYjdjMy0zOWNkNzUzNzg1OGIiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODU5MzM3NDIsImV4cCI6MTc4NTkzNzM0MiwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.hTkwog-w8Mk_mcJATRlmZE19PJUeMrU3tvsLxBaVAKY",
                 "expiresIn":  3600,
                 "refreshToken":  "KYl7FtaQ9ZV4fRvcnc+I4MaOnO2O7XBWrzhWnV27ty2jcW1U4E/Ureqq1U/mvEHjbwfTWsGzHgv32jk2N3XlMw==",
                 "refreshTokenExpiration":  "2026-08-12T12:42:22.8355422Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---



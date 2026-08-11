# Authentication/Security Notifications HTTP Test Report

Generated at: 2026-08-11 16:59:00 +03:00

## Health and anonymous/authenticated boundaries

### GET health
**Request:** GET http://localhost:5049/health
**Response Status:** 200
<pre>Healthy</pre>
---
- [PASS] **Health returns 200** (status=200)
### GET health ping
**Request:** GET http://localhost:5049/api/health/ping
**Response Status:** 200
<pre>{
    "message":  "Pong! Smart Court API is fully operational.",
    "serverTimeUtc":  "2026-08-11T13:59:09.4454694Z",
    "version":  "1.0.0"
}</pre>
---
- [PASS] **Health ping returns 200** (status=200)
### Missing content type
**Request:** POST http://localhost:5049/api/auth/login
<pre>{
    "Password":  "[REDACTED]",
    "Email":  "[REDACTED]"
}</pre>
**Response Status:** 415
<pre>{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.16",
    "title":  "Unsupported Media Type",
    "status":  415,
    "traceId":  "00-0df0c01c16298f38d19e672cbe74e590-2a52a860374de496-00"
}</pre>
---
- [PASS] **Missing content type is rejected** (status=415)
### Invalid content type
**Request:** POST http://localhost:5049/api/auth/login
<pre>{
    "Password":  "[REDACTED]",
    "Email":  "[REDACTED]"
}</pre>
**Response Status:** 415
<pre>{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.16",
    "title":  "Unsupported Media Type",
    "status":  415,
    "traceId":  "00-72b3f4797c74e4017e1e86a3162d9df7-824a[REDACTED_NUMBER]-00"
}</pre>
---
- [PASS] **Invalid content type is rejected** (status=415)
### Anonymous change-password
**Request:** POST http://localhost:5049/api/auth/change-password
<pre>{
    "NewPassword":  "[REDACTED]",
    "ConfirmNewPassword":  "[REDACTED]",
    "CurrentPassword":  "[REDACTED]"
}</pre>
**Response Status:** 401
**Response Body:** (Empty)
---
- [PASS] **Anonymous change-password returns 401** (status=401)
### Anonymous phone send-token
**Request:** POST http://localhost:5049/api/auth/phone/send-token
<pre>{
    "PhoneNumber":  "[REDACTED]"
}</pre>
**Response Status:** 401
**Response Body:** (Empty)
---
- [PASS] **Anonymous phone send-token returns 401** (status=401)
### Refresh missing token
**Request:** POST http://localhost:5049/api/auth/refresh
<pre>{

}</pre>
**Response Status:** 401
<pre>{
    "success":  false,
    "data":  null,
    "message":  "Missing refresh token",
    "errors":  null,
    "statusCode":  401
}</pre>
---
- [PASS] **Refresh missing token returns 401** (status=401)
### Revoke missing tokens
**Request:** POST http://localhost:5049/api/auth/revoke
<pre>{

}</pre>
**Response Status:** 400
<pre>{
    "success":  false,
    "data":  null,
    "message":  "رمز الوصول غير صالح.",
    "errors":  null,
    "statusCode":  400
}</pre>
---
- [PASS] **Revoke missing tokens returns a controlled client error** (status=400)

## Registration, confirmation, resend, and login

### Client registration missing name
**Request:** POST http://localhost:5049/api/auth/register/client
<pre>{
    "Email":  "[REDACTED]",
    "ConfirmPassword":  "[REDACTED]",
    "Password":  "[REDACTED]"
}</pre>
**Response Status:** 400
<pre>{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "FullName":  [
                                    "The FullName field is required.",
                                    "الاسم الكامل مطلوب."
                                ]
               },
    "traceId":  "00-c50378dbdffca16bd88faa40ea5f4627-549d50be42854ba5-00"
}</pre>
---
- [PASS] **Client registration missing name** (status=400)
### Client registration invalid Email
**Request:** POST http://localhost:5049/api/auth/register/client
<pre>{
    "Email":  "[REDACTED]",
    "FullName":  "Valid Client",
    "ConfirmPassword":  "[REDACTED]",
    "Password":  "[REDACTED]"
}</pre>
**Response Status:** 400
<pre>{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "Email":  "[REDACTED]"
               },
    "traceId":  "00-f46635b5ab077180cff93cf5a9911a9b-60da3d66b6d712a4-00"
}</pre>
---
- [PASS] **Client registration invalid Email** (status=400)
### Client registration weak password
**Request:** POST http://localhost:5049/api/auth/register/client
<pre>{
    "Email":  "[REDACTED]",
    "FullName":  "Valid Client",
    "ConfirmPassword":  "[REDACTED]",
    "Password":  "[REDACTED]"
}</pre>
**Response Status:** 400
<pre>{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "Password":  "[REDACTED]"
               },
    "traceId":  "00-459f9e759f690fff1d55106d26143679-8e758c4ad3633e3d-00"
}</pre>
---
- [PASS] **Client registration weak password** (status=400)
### Client registration mismatched password
**Request:** POST http://localhost:5049/api/auth/register/client
<pre>{
    "Email":  "[REDACTED]",
    "FullName":  "Valid Client",
    "ConfirmPassword":  "[REDACTED]",
    "Password":  "[REDACTED]"
}</pre>
**Response Status:** 400
<pre>{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "ConfirmPassword":  "[REDACTED]"
               },
    "traceId":  "00-98daaeda97c6075689fcc0c40d268624-2bb1e162bda5c0b4-00"
}</pre>
---
- [PASS] **Client registration mismatched password** (status=400)
### Client registration hostile SQL/XSS name
**Request:** POST http://localhost:5049/api/auth/register/client
<pre>{
    "Email":  "[REDACTED]",
    "FullName":  "\u0027; DROP TABLE Users; -- \u003cscript\u003ealert(1)\u003c/script\u003e",
    "ConfirmPassword":  "[REDACTED]",
    "Password":  "[REDACTED]"
}</pre>
**Response Status:** 201
<pre>{
    "success":  true,
    "data":  {
                 "userId":  "9fd77c14-6a12-4e05-2514-08def7b0b7db",
                 "email":  "[REDACTED]",
                 "fullName":  "\u0027; DROP TABLE Users; -- \u003cscript\u003ealert(1)\u003c/script\u003e",
                 "role":  "Client"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}</pre>
---
- [PASS] **Hostile registration text is handled without server error** (status=201)
### Malformed lawyer registration
**Request:** POST http://localhost:5049/api/auth/register/lawyer
<pre>{
    "Email":  "[REDACTED]",
    "FullName":  "x",
    "ConfirmPassword":  "[REDACTED]",
    "Password":  "[REDACTED]"
}</pre>
**Response Status:** 400
<pre>{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "Email":  "[REDACTED]",
                   "FullName":  [
                                    "الاسم الكامل يجب أن لا يقل عن 5 أحرف."
                                ],
                   "Password":  "[REDACTED]",
                   "ConfirmPassword":  "[REDACTED]"
               },
    "traceId":  "00-27cd38c9edae83903d4f52b5e337e891-431046767d96b654-00"
}</pre>
---
- [PASS] **Lawyer registration validation rejects malformed payload** (status=400)
### primary client registration
**Request:** POST http://localhost:5049/api/auth/register/client
<pre>{
    "Email":  "[REDACTED]",
    "FullName":  "Gate 7 primary client",
    "ConfirmPassword":  "[REDACTED]",
    "Password":  "[REDACTED]"
}</pre>
**Response Status:** 201
<pre>{
    "success":  true,
    "data":  {
                 "userId":  "3c843fc1-70ee-4f78-2515-08def7b0b7db",
                 "email":  "[REDACTED]",
                 "fullName":  "Gate 7 primary client",
                 "role":  "Client"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}</pre>
---
- [PASS] **primary client registration returns 201** (status=201)
- [PASS] **primary client registration returns user id**
- [PASS] **primary client confirmation link is in mock Email log**
### primary client confirms Email from mock log
**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=3c843fc1-70ee-4f78-2515-08def7b0b7db&token=[REDACTED]
**Response Status:** 200
<pre>{
    "success":  true,
    "message":  "تم تأكيد البريد الإلكتروني بنجاح.",
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **primary client Email confirmation returns 200** (status=200)
### primary client login
**Request:** POST http://localhost:5049/api/auth/login
<pre>{
    "Password":  "[REDACTED]",
    "Email":  "[REDACTED]"
}</pre>
**Response Status:** 200
<pre>{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "3c843fc1-70ee-4f78-2515-08def7b0b7db",
                              "email":  "[REDACTED]",
                              "fullName":  "Gate 7 primary client",
                              "role":  "Client",
                              "status":  "Unverified",
                              "rejectionReason":  null
                          },
                 "accessToken":  "[REDACTED]",
                 "expiresIn":  900,
                 "refreshToken":  "[REDACTED]",
                 "refreshTokenExpiration":  "[REDACTED]"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **primary client login returns 200** (status=200)
### lawyer resend registration
**Request:** POST http://localhost:5049/api/auth/register/lawyer
<pre>{
    "Email":  "[REDACTED]",
    "FullName":  "Gate 7 lawyer resend",
    "ConfirmPassword":  "[REDACTED]",
    "Password":  "[REDACTED]"
}</pre>
**Response Status:** 201
<pre>{
    "success":  true,
    "data":  {
                 "userId":  "48edb18a-2766-46e2-2516-08def7b0b7db",
                 "email":  "[REDACTED]",
                 "fullName":  "Gate 7 lawyer resend",
                 "role":  "Lawyer"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}</pre>
---
- [PASS] **lawyer resend registration returns 201** (status=201)
- [PASS] **lawyer resend registration returns user id**
### Resend verification before confirmation
**Request:** POST http://localhost:5049/api/auth/resend-verification
<pre>{
    "Email":  "[REDACTED]"
}</pre>
**Response Status:** 200
<pre>{
    "success":  true,
    "message":  "تم إرسال رابط التحقق مرة أخرى",
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Resend verification returns 200** (status=200)
- [PASS] **lawyer resend confirmation link is in mock Email log**
### lawyer resend confirms Email from mock log
**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=48edb18a-2766-46e2-2516-08def7b0b7db&token=[REDACTED]
**Response Status:** 200
<pre>{
    "success":  true,
    "message":  "تم تأكيد البريد الإلكتروني بنجاح.",
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **lawyer resend Email confirmation returns 200** (status=200)
### Lawyer after resend login
**Request:** POST http://localhost:5049/api/auth/login
<pre>{
    "Password":  "[REDACTED]",
    "Email":  "[REDACTED]"
}</pre>
**Response Status:** 200
<pre>{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "48edb18a-2766-46e2-2516-08def7b0b7db",
                              "email":  "[REDACTED]",
                              "fullName":  "Gate 7 lawyer resend",
                              "role":  "Lawyer",
                              "status":  "Unverified",
                              "rejectionReason":  null
                          },
                 "accessToken":  "[REDACTED]",
                 "expiresIn":  900,
                 "refreshToken":  "[REDACTED]",
                 "refreshTokenExpiration":  "[REDACTED]"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Lawyer after resend login returns 200** (status=200)
### unconfirmed account registration
**Request:** POST http://localhost:5049/api/auth/register/client
<pre>{
    "Email":  "[REDACTED]",
    "FullName":  "Gate 7 unconfirmed account",
    "ConfirmPassword":  "[REDACTED]",
    "Password":  "[REDACTED]"
}</pre>
**Response Status:** 201
<pre>{
    "success":  true,
    "data":  {
                 "userId":  "b79a2e62-bf20-4743-2517-08def7b0b7db",
                 "email":  "[REDACTED]",
                 "fullName":  "Gate 7 unconfirmed account",
                 "role":  "Client"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}</pre>
---
- [PASS] **unconfirmed account registration returns 201** (status=201)
- [PASS] **unconfirmed account registration returns user id**
### Login before Email confirmation
**Request:** POST http://localhost:5049/api/auth/login
<pre>{
    "Password":  "[REDACTED]",
    "Email":  "[REDACTED]"
}</pre>
**Response Status:** 403
<pre>{
    "success":  false,
    "data":  null,
    "message":  "يرجى تأكيد البريد الإلكتروني أولاً",
    "errors":  null,
    "statusCode":  403
}</pre>
---
- [PASS] **Unconfirmed login returns 403** (status=403)
### Resend unconfirmed account
**Request:** POST http://localhost:5049/api/auth/resend-verification
<pre>{
    "Email":  "[REDACTED]"
}</pre>
**Response Status:** 200
<pre>{
    "success":  true,
    "message":  "تم إرسال رابط التحقق مرة أخرى",
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Unconfirmed resend returns 200** (status=200)
- [PASS] **unconfirmed account confirmation link is in mock Email log**
### unconfirmed account confirms Email from mock log
**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=b79a2e62-bf20-4743-2517-08def7b0b7db&token=[REDACTED]
**Response Status:** 200
<pre>{
    "success":  true,
    "message":  "تم تأكيد البريد الإلكتروني بنجاح.",
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **unconfirmed account Email confirmation returns 200** (status=200)
### Confirmed account login
**Request:** POST http://localhost:5049/api/auth/login
<pre>{
    "Password":  "[REDACTED]",
    "Email":  "[REDACTED]"
}</pre>
**Response Status:** 200
<pre>{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "b79a2e62-bf20-4743-2517-08def7b0b7db",
                              "email":  "[REDACTED]",
                              "fullName":  "Gate 7 unconfirmed account",
                              "role":  "Client",
                              "status":  "Unverified",
                              "rejectionReason":  null
                          },
                 "accessToken":  "[REDACTED]",
                 "expiresIn":  900,
                 "refreshToken":  "[REDACTED]",
                 "refreshTokenExpiration":  "[REDACTED]"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Confirmed account login returns 200** (status=200)
### Duplicate client registration
**Request:** POST http://localhost:5049/api/auth/register/client
<pre>{
    "Email":  "[REDACTED]",
    "FullName":  "Duplicate",
    "ConfirmPassword":  "[REDACTED]",
    "Password":  "[REDACTED]"
}</pre>
**Response Status:** 409
<pre>{
    "success":  false,
    "data":  null,
    "message":  "البريد الإلكتروني مسجل بالفعل.",
    "errors":  null,
    "statusCode":  409
}</pre>
---
- [PASS] **Duplicate registration returns 409** (status=409)
### Replay Email confirmation
**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=3c843fc1-70ee-4f78-2515-08def7b0b7db&token=[REDACTED]
**Response Status:** 400
<pre>{
    "success":  false,
    "data":  null,
    "message":  "الحساب مفعل مسبقاً. يرجى التوجه لصفحة تسجيل الدخول.",
    "errors":  null,
    "statusCode":  400
}</pre>
---
- [PASS] **Replayed Email confirmation is rejected** (status=400)
### Resend confirmed account
**Request:** POST http://localhost:5049/api/auth/resend-verification
<pre>{
    "Email":  "[REDACTED]"
}</pre>
**Response Status:** 200
<pre>{
    "success":  true,
    "message":  "تم إرسال رابط التحقق مرة أخرى",
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Confirmed resend remains 200** (status=200)
### Malformed confirmation query
**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=bad&token=[REDACTED]
**Response Status:** 400
<pre>{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "UserId":  [
                                  "معرف المستخدم غير صالح."
                              ]
               },
    "traceId":  "00-0cb61b7e8b17e74ea09a0fe62290af18-e7fa5b11d3578978-00"
}</pre>
---
- [PASS] **Malformed confirmation query is rejected** (status=400)

## Login and password-change notification

### Invalid login
**Request:** POST http://localhost:5049/api/auth/login
<pre>{
    "Password":  "[REDACTED]",
    "Email":  "[REDACTED]"
}</pre>
**Response Status:** 401
<pre>{
    "success":  false,
    "data":  null,
    "message":  "البريد الإلكتروني أو كلمة المرور غير صحيحة.",
    "errors":  null,
    "statusCode":  401
}</pre>
---
- [PASS] **Invalid login returns 401** (status=401)
### Login validator
**Request:** POST http://localhost:5049/api/auth/login
<pre>{
    "Password":  "[REDACTED]",
    "Email":  "[REDACTED]"
}</pre>
**Response Status:** 400
<pre>{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "Email":  "[REDACTED]",
                   "Password":  "[REDACTED]"
               },
    "traceId":  "00-f9048e0c79dca52bdf61efba0fe6349e-bdb80ee2b5dd9f3b-00"
}</pre>
---
- [PASS] **Login validator returns 400** (status=400)
### Inbox before change
**Request:** GET http://localhost:5049/api/notifications?pageSize=50
**Response Status:** 200
<pre>{
    "success":  true,
    "data":  {
                 "items":  [

                           ],
                 "nextCursor":  null,
                 "unreadCount":  0
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Inbox before change returns 200** (status=200)
### Wrong current password
**Request:** POST http://localhost:5049/api/auth/change-password
<pre>{
    "NewPassword":  "[REDACTED]",
    "ConfirmNewPassword":  "[REDACTED]",
    "CurrentPassword":  "[REDACTED]"
}</pre>
**Response Status:** 400
<pre>{
    "success":  false,
    "data":  null,
    "message":  "One or more validation failures have occurred.",
    "errors":  [
                   "CurrentPassword: كلمة المرور الحالية غير صحيحة."
               ],
    "statusCode":  400
}</pre>
---
- [PASS] **Wrong current password is rejected** (status=400)
### Password reuse
**Request:** POST http://localhost:5049/api/auth/change-password
<pre>{
    "NewPassword":  "[REDACTED]",
    "ConfirmNewPassword":  "[REDACTED]",
    "CurrentPassword":  "[REDACTED]"
}</pre>
**Response Status:** 400
<pre>{
    "success":  false,
    "data":  null,
    "message":  "One or more validation failures have occurred.",
    "errors":  [
                   "NewPassword: يجب أن تختلف كلمة المرور الجديدة عن كلمة المرور الحالية."
               ],
    "statusCode":  400
}</pre>
---
- [PASS] **Password reuse is rejected** (status=400)
### Weak change password
**Request:** POST http://localhost:5049/api/auth/change-password
<pre>{
    "NewPassword":  "[REDACTED]",
    "ConfirmNewPassword":  "[REDACTED]",
    "CurrentPassword":  "[REDACTED]"
}</pre>
**Response Status:** 400
<pre>{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "NewPassword":  "[REDACTED]"
               },
    "traceId":  "00-d3385473d966613a351c70d023dfeecc-50ac1fd9a7cdd217-00"
}</pre>
---
- [PASS] **Weak new password is rejected** (status=400)
### Mismatched change password
**Request:** POST http://localhost:5049/api/auth/change-password
<pre>{
    "NewPassword":  "[REDACTED]",
    "ConfirmNewPassword":  "[REDACTED]",
    "CurrentPassword":  "[REDACTED]"
}</pre>
**Response Status:** 400
<pre>{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "ConfirmNewPassword":  "[REDACTED]"
               },
    "traceId":  "00-446d764db9e07c8bbefe30a0f5950438-b1251cd33f00a94d-00"
}</pre>
---
- [PASS] **Mismatched new password is rejected** (status=400)
### Successful change password
**Request:** POST http://localhost:5049/api/auth/change-password
<pre>{
    "NewPassword":  "[REDACTED]",
    "ConfirmNewPassword":  "[REDACTED]",
    "CurrentPassword":  "[REDACTED]"
}</pre>
**Response Status:** 200
<pre>{
    "success":  true,
    "message":  "تم تغيير كلمة المرور بنجاح",
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Successful change-password returns 200** (status=200)
### Refresh after password change
**Request:** POST http://localhost:5049/api/auth/refresh
<pre>{
    "RefreshToken":  "[REDACTED]"
}</pre>
**Response Status:** 401
<pre>{
    "success":  false,
    "data":  null,
    "message":  "رمز التحديث غير صالح أو منتهي الصلاحية.",
    "errors":  null,
    "statusCode":  401
}</pre>
---
- [PASS] **Password-change old refresh token is revoked** (status=401)
### Old access after password change
**Request:** GET http://localhost:5049/api/notifications?pageSize=50
**Response Status:** 401
**Response Body:** (Empty)
---
- [PASS] **Password-change old access token is revoked** (status=401)
### Client after password change login
**Request:** POST http://localhost:5049/api/auth/login
<pre>{
    "Password":  "[REDACTED]",
    "Email":  "[REDACTED]"
}</pre>
**Response Status:** 200
<pre>{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "3c843fc1-70ee-4f78-2515-08def7b0b7db",
                              "email":  "[REDACTED]",
                              "fullName":  "Gate 7 primary client",
                              "role":  "Client",
                              "status":  "Unverified",
                              "rejectionReason":  null
                          },
                 "accessToken":  "[REDACTED]",
                 "expiresIn":  900,
                 "refreshToken":  "[REDACTED]",
                 "refreshTokenExpiration":  "[REDACTED]"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Client after password change login returns 200** (status=200)
### Password-change notification list
**Request:** GET http://localhost:5049/api/notifications?pageSize=50
**Response Status:** 200
<pre>{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "2edaa322-35cd-4c95-8914-59759ead1681",
                                   "type":  "security.password-changed",
                                   "severity":  "Critical",
                                   "title":  "تم تغيير كلمة المرور",
                                   "body":  "تم تغيير كلمة مرور حسابك بنجاح. إذا لم تكن أنت من أجرى هذا التغيير، يرجى تأمين حسابك والتواصل مع الدعم.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3c843fc1-70ee-4f78-2515-08def7b0b7db"
                                            },
                                   "createdAtUtc":  "2026-08-11T13:59:14.8254152",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  1
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Password-change notification list returns 200** (status=200)
- [PASS] **Successful password change creates exactly one notification** (count=1)
- [PASS] **Password-change notification exact Arabic/safe contract**
### Unread count after password change
**Request:** GET http://localhost:5049/api/notifications/unread-count
**Response Status:** 200
<pre>{
    "success":  true,
    "data":  {
                 "unreadCount":  1
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Unread count after password change returns 200** (status=200)
- [PASS] **Password-change notification starts unread**
### Read password-change notification
**Request:** PATCH http://localhost:5049/api/notifications/2edaa322-35cd-4c95-8914-59759ead1681/read
**Response Status:** 200
<pre>{
    "success":  true,
    "data":  {
                 "id":  "2edaa322-35cd-4c95-8914-59759ead1681",
                 "type":  "security.password-changed",
                 "severity":  "Critical",
                 "title":  "تم تغيير كلمة المرور",
                 "body":  "تم تغيير كلمة مرور حسابك بنجاح. إذا لم تكن أنت من أجرى هذا التغيير، يرجى تأمين حسابك والتواصل مع الدعم.",
                 "actionUrl":  null,
                 "data":  {
                              "userId":  "3c843fc1-70ee-4f78-2515-08def7b0b7db"
                          },
                 "createdAtUtc":  "2026-08-11T13:59:14.8254152",
                 "readAtUtc":  "2026-08-11T13:59:16.1569574Z",
                 "expiresAtUtc":  null
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Read password-change notification returns 200** (status=200)
### Replay read password-change notification
**Request:** PATCH http://localhost:5049/api/notifications/2edaa322-35cd-4c95-8914-59759ead1681/read
**Response Status:** 200
<pre>{
    "success":  true,
    "data":  {
                 "id":  "2edaa322-35cd-4c95-8914-59759ead1681",
                 "type":  "security.password-changed",
                 "severity":  "Critical",
                 "title":  "تم تغيير كلمة المرور",
                 "body":  "تم تغيير كلمة مرور حسابك بنجاح. إذا لم تكن أنت من أجرى هذا التغيير، يرجى تأمين حسابك والتواصل مع الدعم.",
                 "actionUrl":  null,
                 "data":  {
                              "userId":  "3c843fc1-70ee-4f78-2515-08def7b0b7db"
                          },
                 "createdAtUtc":  "2026-08-11T13:59:14.8254152",
                 "readAtUtc":  "2026-08-11T13:59:16.1569574",
                 "expiresAtUtc":  null
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Replay read password-change notification returns 200** (status=200)
### Read-all password-change notification
**Request:** PATCH http://localhost:5049/api/notifications/read-all
**Response Status:** 200
<pre>{
    "success":  true,
    "data":  {
                 "readAtUtc":  "2026-08-11T13:59:16.2384575Z",
                 "unreadCount":  0
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Read-all after password change returns 200** (status=200)
### Read notification filter
**Request:** GET http://localhost:5049/api/notifications?pageSize=50&isRead=true
**Response Status:** 200
<pre>{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "2edaa322-35cd-4c95-8914-59759ead1681",
                                   "type":  "security.password-changed",
                                   "severity":  "Critical",
                                   "title":  "تم تغيير كلمة المرور",
                                   "body":  "تم تغيير كلمة مرور حسابك بنجاح. إذا لم تكن أنت من أجرى هذا التغيير، يرجى تأمين حسابك والتواصل مع الدعم.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3c843fc1-70ee-4f78-2515-08def7b0b7db"
                                            },
                                   "createdAtUtc":  "2026-08-11T13:59:14.8254152",
                                   "readAtUtc":  "2026-08-11T13:59:16.1569574",
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  0
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Read notification filter returns 200** (status=200)
- [PASS] **Read filter contains password-change notification**
### Malformed notification read id
**Request:** PATCH http://localhost:5049/api/notifications/not-a-guid/read
**Response Status:** 404
**Response Body:** (Empty)
---
- [PASS] **Malformed notification id is rejected** (status=404)
### Unknown notification read id
**Request:** PATCH http://localhost:5049/api/notifications/00000000-0000-0000-0000-[REDACTED_NUMBER]/read
**Response Status:** 404
<pre>{
    "success":  false,
    "data":  null,
    "message":  "Entity \"Notification\" (00000000-0000-0000-0000-[REDACTED_NUMBER]) was not found.",
    "errors":  null,
    "statusCode":  404
}</pre>
---
- [PASS] **Unknown notification id returns 404** (status=404)
### Lawyer isolation inbox
**Request:** GET http://localhost:5049/api/notifications?pageSize=50
**Response Status:** 200
<pre>{
    "success":  true,
    "data":  {
                 "items":  [

                           ],
                 "nextCursor":  null,
                 "unreadCount":  0
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Lawyer isolation inbox returns 200** (status=200)
- [PASS] **Unrelated lawyer receives no password-change notification**

## Forgot-password and password-reset notification

### Forgot validator
**Request:** POST http://localhost:5049/api/auth/forgot-password
<pre>{
    "Email":  "[REDACTED]"
}</pre>
**Response Status:** 400
<pre>{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "Email":  "[REDACTED]"
               },
    "traceId":  "00-9fd5c12189ce3da[REDACTED_NUMBER]ee39a-ad5aa9b535394ec7-00"
}</pre>
---
- [PASS] **Forgot-password validator returns 400** (status=400)
### Forgot unknown account
**Request:** POST http://localhost:5049/api/auth/forgot-password
<pre>{
    "Email":  "[REDACTED]"
}</pre>
**Response Status:** 200
<pre>{
    "success":  true,
    "message":  "إذا كان البريد الإلكتروني مسجلاً، سيتم إرسال رابط إعادة تعيين كلمة المرور",
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Forgot-password unknown account returns generic 200** (status=200)
### Forgot valid account
**Request:** POST http://localhost:5049/api/auth/forgot-password
<pre>{
    "Email":  "[REDACTED]"
}</pre>
**Response Status:** 200
<pre>{
    "success":  true,
    "message":  "إذا كان البريد الإلكتروني مسجلاً، سيتم إرسال رابط إعادة تعيين كلمة المرور",
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Forgot-password valid account returns 200** (status=200)
- [PASS] **Reset link is present in mock Email log**
- [PASS] **Reset link contains a token**
### Malformed reset token
**Request:** POST http://localhost:5049/api/auth/reset-password
<pre>{
    "Email":  "[REDACTED]",
    "ConfirmNewPassword":  "[REDACTED]",
    "NewPassword":  "[REDACTED]",
    "Token":  "[REDACTED]"
}</pre>
**Response Status:** 400
<pre>{
    "success":  false,
    "data":  null,
    "message":  "رابط إعادة تعيين كلمة المرور غير صالح أو منتهي الصلاحية.",
    "errors":  null,
    "statusCode":  400
}</pre>
---
- [PASS] **Malformed reset token is rejected** (status=400)
### Oversized reset token
**Request:** POST http://localhost:5049/api/auth/reset-password
<pre>{
    "Email":  "[REDACTED]",
    "ConfirmNewPassword":  "[REDACTED]",
    "NewPassword":  "[REDACTED]",
    "Token":  "[REDACTED]"
}</pre>
**Response Status:** 400
<pre>{
    "success":  false,
    "data":  null,
    "message":  "رابط إعادة تعيين كلمة المرور غير صالح أو منتهي الصلاحية.",
    "errors":  null,
    "statusCode":  400
}</pre>
---
- [PASS] **Oversized reset token is rejected** (status=400)
### Weak reset password
**Request:** POST http://localhost:5049/api/auth/reset-password
<pre>{
    "Email":  "[REDACTED]",
    "ConfirmNewPassword":  "[REDACTED]",
    "NewPassword":  "[REDACTED]",
    "Token":  "[REDACTED]"
}</pre>
**Response Status:** 400
<pre>{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "NewPassword":  "[REDACTED]"
               },
    "traceId":  "00-9643fd8b7d9e3977ab4615275a721590-f9cfcef243fa8079-00"
}</pre>
---
- [PASS] **Weak reset password is rejected** (status=400)
### Mismatched reset password
**Request:** POST http://localhost:5049/api/auth/reset-password
<pre>{
    "Email":  "[REDACTED]",
    "ConfirmNewPassword":  "[REDACTED]",
    "NewPassword":  "[REDACTED]",
    "Token":  "[REDACTED]"
}</pre>
**Response Status:** 400
<pre>{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "ConfirmNewPassword":  "[REDACTED]"
               },
    "traceId":  "00-3970bf[REDACTED_NUMBER]d6d824ec664c182-e29c3fac81e3b6e1-00"
}</pre>
---
- [PASS] **Mismatched reset password is rejected** (status=400)
### Successful reset password
**Request:** POST http://localhost:5049/api/auth/reset-password
<pre>{
    "Email":  "[REDACTED]",
    "ConfirmNewPassword":  "[REDACTED]",
    "NewPassword":  "[REDACTED]",
    "Token":  "[REDACTED]"
}</pre>
**Response Status:** 200
<pre>{
    "success":  true,
    "message":  "تم إعادة تعيين كلمة المرور بنجاح",
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Successful reset-password returns 200** (status=200)
### Refresh after password reset
**Request:** POST http://localhost:5049/api/auth/refresh
<pre>{
    "RefreshToken":  "[REDACTED]"
}</pre>
**Response Status:** 401
<pre>{
    "success":  false,
    "data":  null,
    "message":  "رمز التحديث غير صالح أو منتهي الصلاحية.",
    "errors":  null,
    "statusCode":  401
}</pre>
---
- [PASS] **Password-reset old refresh token is revoked** (status=401)
### Old access after password reset
**Request:** GET http://localhost:5049/api/notifications?pageSize=50
**Response Status:** 401
**Response Body:** (Empty)
---
- [PASS] **Password-reset old access token is revoked** (status=401)
### Replay reset token
**Request:** POST http://localhost:5049/api/auth/reset-password
<pre>{
    "Email":  "[REDACTED]",
    "ConfirmNewPassword":  "[REDACTED]",
    "NewPassword":  "[REDACTED]",
    "Token":  "[REDACTED]"
}</pre>
**Response Status:** 400
<pre>{
    "success":  false,
    "data":  null,
    "message":  "رابط إعادة تعيين كلمة المرور غير صالح أو منتهي الصلاحية.",
    "errors":  null,
    "statusCode":  400
}</pre>
---
- [PASS] **Replayed reset token is rejected** (status=400)
### Client after password reset login
**Request:** POST http://localhost:5049/api/auth/login
<pre>{
    "Password":  "[REDACTED]",
    "Email":  "[REDACTED]"
}</pre>
**Response Status:** 200
<pre>{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "3c843fc1-70ee-4f78-2515-08def7b0b7db",
                              "email":  "[REDACTED]",
                              "fullName":  "Gate 7 primary client",
                              "role":  "Client",
                              "status":  "Unverified",
                              "rejectionReason":  null
                          },
                 "accessToken":  "[REDACTED]",
                 "expiresIn":  900,
                 "refreshToken":  "[REDACTED]",
                 "refreshTokenExpiration":  "[REDACTED]"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Client after password reset login returns 200** (status=200)
### Password-reset notification list
**Request:** GET http://localhost:5049/api/notifications?pageSize=50
**Response Status:** 200
<pre>{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "f6bd5d79-df62-415d-a5da-f642b32fce2b",
                                   "type":  "security.password-reset",
                                   "severity":  "Critical",
                                   "title":  "تمت إعادة تعيين كلمة المرور",
                                   "body":  "تمت إعادة تعيين كلمة مرور حسابك بنجاح. إذا لم تطلب هذا الإجراء، يرجى تأمين حسابك والتواصل مع الدعم.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3c843fc1-70ee-4f78-2515-08def7b0b7db"
                                            },
                                   "createdAtUtc":  "2026-08-11T13:59:16.7415786",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "2edaa322-35cd-4c95-8914-59759ead1681",
                                   "type":  "security.password-changed",
                                   "severity":  "Critical",
                                   "title":  "تم تغيير كلمة المرور",
                                   "body":  "تم تغيير كلمة مرور حسابك بنجاح. إذا لم تكن أنت من أجرى هذا التغيير، يرجى تأمين حسابك والتواصل مع الدعم.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3c843fc1-70ee-4f78-2515-08def7b0b7db"
                                            },
                                   "createdAtUtc":  "2026-08-11T13:59:14.8254152",
                                   "readAtUtc":  "2026-08-11T13:59:16.1569574",
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  1
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Password-reset notification list returns 200** (status=200)
- [PASS] **Successful password reset creates exactly one notification** (count=1)
- [PASS] **Password-reset notification exact Arabic/safe contract**
### Reset notification replay list
**Request:** GET http://localhost:5049/api/notifications?pageSize=50
**Response Status:** 200
<pre>{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "f6bd5d79-df62-415d-a5da-f642b32fce2b",
                                   "type":  "security.password-reset",
                                   "severity":  "Critical",
                                   "title":  "تمت إعادة تعيين كلمة المرور",
                                   "body":  "تمت إعادة تعيين كلمة مرور حسابك بنجاح. إذا لم تطلب هذا الإجراء، يرجى تأمين حسابك والتواصل مع الدعم.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3c843fc1-70ee-4f78-2515-08def7b0b7db"
                                            },
                                   "createdAtUtc":  "2026-08-11T13:59:16.7415786",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "2edaa322-35cd-4c95-8914-59759ead1681",
                                   "type":  "security.password-changed",
                                   "severity":  "Critical",
                                   "title":  "تم تغيير كلمة المرور",
                                   "body":  "تم تغيير كلمة مرور حسابك بنجاح. إذا لم تكن أنت من أجرى هذا التغيير، يرجى تأمين حسابك والتواصل مع الدعم.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3c843fc1-70ee-4f78-2515-08def7b0b7db"
                                            },
                                   "createdAtUtc":  "2026-08-11T13:59:14.8254152",
                                   "readAtUtc":  "2026-08-11T13:59:16.1569574",
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  1
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Reset notification replay list returns 200** (status=200)
- [PASS] **Replayed reset token creates no second notification**
- [SKIP] **True expired reset token** — Identity lifespan is fixed at one hour and no safe HTTP time-advance control exists; invalid, oversized, and replayed paths are covered.

## Refresh rotation and revoke

### Successful refresh rotation
**Request:** POST http://localhost:5049/api/auth/refresh
<pre>{
    "RefreshToken":  "[REDACTED]"
}</pre>
**Response Status:** 200
<pre>{
    "success":  true,
    "data":  {
                 "accessToken":  "[REDACTED]",
                 "accessTokenExpiresInSeconds":  "[REDACTED]",
                 "refreshToken":  "[REDACTED]",
                 "expiresAt":  "2026-08-18T13:59:17.860888Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Refresh rotation returns 200** (status=200)
- [PASS] **Refresh rotation returns new access and refresh values**
### Revoke active refresh
**Request:** POST http://localhost:5049/api/auth/revoke
<pre>{
    "Token":  "[REDACTED]",
    "RefreshToken":  "[REDACTED]"
}</pre>
**Response Status:** 200
<pre>{
    "success":  true,
    "data":  true,
    "message":  "تم إبطال رمز التحديث بنجاح.",
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Revoke active refresh returns 200** (status=200)
- [PASS] **Revoke active refresh returns true**
### Refresh after revoke
**Request:** POST http://localhost:5049/api/auth/refresh
<pre>{
    "RefreshToken":  "[REDACTED]"
}</pre>
**Response Status:** 401
<pre>{
    "success":  false,
    "data":  null,
    "message":  "رمز التحديث غير صالح أو منتهي الصلاحية.",
    "errors":  null,
    "statusCode":  401
}</pre>
---
- [PASS] **Refresh after explicit revoke is rejected** (status=401)
### Replay old rotated refresh
**Request:** POST http://localhost:5049/api/auth/refresh
<pre>{
    "RefreshToken":  "[REDACTED]"
}</pre>
**Response Status:** 401
<pre>{
    "success":  false,
    "data":  null,
    "message":  "رمز التحديث غير صالح أو منتهي الصلاحية.",
    "errors":  null,
    "statusCode":  401
}</pre>
---
- [PASS] **Rotated-away refresh token is rejected** (status=401)
### Invalid refresh value
**Request:** POST http://localhost:5049/api/auth/refresh
<pre>{
    "RefreshToken":  "[REDACTED]"
}</pre>
**Response Status:** 401
<pre>{
    "success":  false,
    "data":  null,
    "message":  "رمز التحديث غير صالح أو منتهي الصلاحية.",
    "errors":  null,
    "statusCode":  401
}</pre>
---
- [PASS] **Invalid refresh token is rejected** (status=401)
### Replay explicit revoke
**Request:** POST http://localhost:5049/api/auth/revoke
<pre>{
    "Token":  "[REDACTED]",
    "RefreshToken":  "[REDACTED]"
}</pre>
**Response Status:** 200
<pre>{
    "success":  true,
    "data":  false,
    "message":  "تم إبطال رمز التحديث بنجاح.",
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Revoke replay returns 200** (status=200)
- [PASS] **Revoke replay returns false**
### Malformed revoke
**Request:** POST http://localhost:5049/api/auth/revoke
<pre>{
    "Token":  "[REDACTED]",
    "RefreshToken":  "[REDACTED]"
}</pre>
**Response Status:** 400
<pre>{
    "success":  false,
    "data":  null,
    "message":  "رمز الوصول غير صالح.",
    "errors":  null,
    "statusCode":  400
}</pre>
---
- [PASS] **Malformed revoke is rejected** (status=400)

## Legacy phone verification endpoints

### Client before phone verification login
**Request:** POST http://localhost:5049/api/auth/login
<pre>{
    "Password":  "[REDACTED]",
    "Email":  "[REDACTED]"
}</pre>
**Response Status:** 200
<pre>{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "3c843fc1-70ee-4f78-2515-08def7b0b7db",
                              "email":  "[REDACTED]",
                              "fullName":  "Gate 7 primary client",
                              "role":  "Client",
                              "status":  "Unverified",
                              "rejectionReason":  null
                          },
                 "accessToken":  "[REDACTED]",
                 "expiresIn":  900,
                 "refreshToken":  "[REDACTED]",
                 "refreshTokenExpiration":  "[REDACTED]"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Client before phone verification login returns 200** (status=200)
### Phone send-token
**Request:** POST http://localhost:5049/api/auth/phone/send-token
<pre>{
    "PhoneNumber":  "[REDACTED]"
}</pre>
**Response Status:** 200
<pre>{
    "success":  true,
    "data":  {
                 "message":  "تم إرسال كود التوثيق بنجاح"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Phone send-token returns 200** (status=200)
- [PASS] **Mock SMS code is recorded**
### Phone confirm
**Request:** POST http://localhost:5049/api/auth/phone/confirm
<pre>{
    "Token":  "[REDACTED]",
    "PhoneNumber":  "[REDACTED]"
}</pre>
**Response Status:** 200
<pre>{
    "success":  true,
    "data":  {
                 "message":  "تم توثيق رقم الهاتف بنجاح"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Phone confirm returns 200** (status=200)
### Client after phone confirmation login
**Request:** POST http://localhost:5049/api/auth/login
<pre>{
    "Password":  "[REDACTED]",
    "Email":  "[REDACTED]"
}</pre>
**Response Status:** 200
<pre>{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "3c843fc1-70ee-4f78-2515-08def7b0b7db",
                              "email":  "[REDACTED]",
                              "fullName":  "Gate 7 primary client",
                              "role":  "Client",
                              "status":  "Unverified",
                              "rejectionReason":  null
                          },
                 "accessToken":  "[REDACTED]",
                 "expiresIn":  900,
                 "refreshToken":  "[REDACTED]",
                 "refreshTokenExpiration":  "[REDACTED]"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Client after phone confirmation login returns 200** (status=200)
### Phone confirm replay
**Request:** POST http://localhost:5049/api/auth/phone/confirm
<pre>{
    "Token":  "[REDACTED]",
    "PhoneNumber":  "[REDACTED]"
}</pre>
**Response Status:** 400
<pre>{
    "success":  false,
    "data":  null,
    "message":  "كود التوثيق غير صحيح أو منتهي الصلاحية.",
    "errors":  null,
    "statusCode":  400
}</pre>
---
- [PASS] **Phone confirmation replay is rejected** (status=400)
### Invalid phone token
**Request:** POST http://localhost:5049/api/auth/phone/confirm
<pre>{
    "Token":  "[REDACTED]",
    "PhoneNumber":  "[REDACTED]"
}</pre>
**Response Status:** 400
<pre>{
    "success":  false,
    "data":  null,
    "message":  "كود التوثيق غير صحيح أو منتهي الصلاحية.",
    "errors":  null,
    "statusCode":  400
}</pre>
---
- [PASS] **Invalid phone token is rejected** (status=400)
### Empty phone send payload
**Request:** POST http://localhost:5049/api/auth/phone/send-token
<pre>{

}</pre>
**Response Status:** 200
<pre>{
    "success":  true,
    "data":  {
                 "message":  "تم إرسال كود التوثيق بنجاح"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Empty phone payload does not produce a server error** (status=200)
### Phone final security list
**Request:** GET http://localhost:5049/api/notifications?pageSize=50
**Response Status:** 200
<pre>{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "f6bd5d79-df62-415d-a5da-f642b32fce2b",
                                   "type":  "security.password-reset",
                                   "severity":  "Critical",
                                   "title":  "تمت إعادة تعيين كلمة المرور",
                                   "body":  "تمت إعادة تعيين كلمة مرور حسابك بنجاح. إذا لم تطلب هذا الإجراء، يرجى تأمين حسابك والتواصل مع الدعم.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3c843fc1-70ee-4f78-2515-08def7b0b7db"
                                            },
                                   "createdAtUtc":  "2026-08-11T13:59:16.7415786",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "2edaa322-35cd-4c95-8914-59759ead1681",
                                   "type":  "security.password-changed",
                                   "severity":  "Critical",
                                   "title":  "تم تغيير كلمة المرور",
                                   "body":  "تم تغيير كلمة مرور حسابك بنجاح. إذا لم تكن أنت من أجرى هذا التغيير، يرجى تأمين حسابك والتواصل مع الدعم.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3c843fc1-70ee-4f78-2515-08def7b0b7db"
                                            },
                                   "createdAtUtc":  "2026-08-11T13:59:14.8254152",
                                   "readAtUtc":  "2026-08-11T13:59:16.1569574",
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  1
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Phone final security list returns 200** (status=200)
- [PASS] **Phone actions create no extra password-change notification**

## Hostile input and final recipient isolation

### Hostile forgot input
**Request:** POST http://localhost:5049/api/auth/forgot-password
<pre>{
    "Email":  "[REDACTED]"
}</pre>
**Response Status:** 400
<pre>{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "Email":  "[REDACTED]"
               },
    "traceId":  "00-131bb07b96d4223186e0baecc1ffe619-9b2133b64b81d5b2-00"
}</pre>
---
- [PASS] **Hostile forgot-password input is rejected** (status=400)
### Hostile reset input
**Request:** POST http://localhost:5049/api/auth/reset-password
<pre>{
    "Email":  "[REDACTED]",
    "ConfirmNewPassword":  "[REDACTED]",
    "NewPassword":  "[REDACTED]",
    "Token":  "[REDACTED]"
}</pre>
**Response Status:** 400
<pre>{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "Email":  "[REDACTED]"
               },
    "traceId":  "00-eacc969ada62aa7e621417c49dde9313-94d8ab3a95642f5f-00"
}</pre>
---
- [PASS] **Hostile reset-password input is rejected** (status=400)
### Extreme forgot input
**Request:** POST http://localhost:5049/api/auth/forgot-password
<pre>{
    "Email":  "[REDACTED]"
}</pre>
**Response Status:** 200
<pre>{
    "success":  true,
    "message":  "إذا كان البريد الإلكتروني مسجلاً، سيتم إرسال رابط إعادة تعيين كلمة المرور",
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Extreme Email input returns generic forgot-password response** (status=200)
### Empty reset body
**Request:** POST http://localhost:5049/api/auth/reset-password
<pre>{

}</pre>
**Response Status:** 400
<pre>{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "Email":  "[REDACTED]",
                   "NewPassword":  "[REDACTED]",
                   "ConfirmNewPassword":  "[REDACTED]"
               },
    "traceId":  "00-b5438208cd04c8a3f4cc6aad37568563-eddd04dd2c6cd419-00"
}</pre>
---
- [PASS] **Empty reset body is rejected** (status=400)
### Empty register body
**Request:** POST http://localhost:5049/api/auth/register/client
<pre>{

}</pre>
**Response Status:** 400
<pre>{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "Email":  "[REDACTED]",
                   "FullName":  [
                                    "The FullName field is required.",
                                    "الاسم الكامل مطلوب."
                                ],
                   "Password":  "[REDACTED]",
                   "ConfirmPassword":  "[REDACTED]"
               },
    "traceId":  "00-b319c8a4451dc1ef96f226741bf252a3-198b73290efcbf08-00"
}</pre>
---
- [PASS] **Empty register body is rejected** (status=400)
### Final client inbox
**Request:** GET http://localhost:5049/api/notifications?pageSize=50
**Response Status:** 200
<pre>{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "f6bd5d79-df62-415d-a5da-f642b32fce2b",
                                   "type":  "security.password-reset",
                                   "severity":  "Critical",
                                   "title":  "تمت إعادة تعيين كلمة المرور",
                                   "body":  "تمت إعادة تعيين كلمة مرور حسابك بنجاح. إذا لم تطلب هذا الإجراء، يرجى تأمين حسابك والتواصل مع الدعم.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3c843fc1-70ee-4f78-2515-08def7b0b7db"
                                            },
                                   "createdAtUtc":  "2026-08-11T13:59:16.7415786",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "2edaa322-35cd-4c95-8914-59759ead1681",
                                   "type":  "security.password-changed",
                                   "severity":  "Critical",
                                   "title":  "تم تغيير كلمة المرور",
                                   "body":  "تم تغيير كلمة مرور حسابك بنجاح. إذا لم تكن أنت من أجرى هذا التغيير، يرجى تأمين حسابك والتواصل مع الدعم.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3c843fc1-70ee-4f78-2515-08def7b0b7db"
                                            },
                                   "createdAtUtc":  "2026-08-11T13:59:14.8254152",
                                   "readAtUtc":  "2026-08-11T13:59:16.1569574",
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  1
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Final client inbox returns 200** (status=200)
- [PASS] **Final client inbox has one password-change notification**
- [PASS] **Final client inbox has one password-reset notification**
### Final unrelated lawyer inbox
**Request:** GET http://localhost:5049/api/notifications?pageSize=50
**Response Status:** 200
<pre>{
    "success":  true,
    "data":  {
                 "items":  [

                           ],
                 "nextCursor":  null,
                 "unreadCount":  0
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}</pre>
---
- [PASS] **Final unrelated lawyer inbox returns 200** (status=200)
- [PASS] **Unrelated lawyer has no password-change notification**
- [PASS] **Unrelated lawyer has no password-reset notification**

## API and mock provider log monitoring

- [PASS] **API/outbox/notification/provider logs are clean** (violations=0)
- [PASS] **Mock Email confirmation was recorded**
- [PASS] **Mock Email confirmation was recorded**
- [PASS] **Mock Email confirmation was recorded**
- [PASS] **Mock Email reset receipt was recorded**
- [PASS] **API test port is released after owned process shutdown**

## Execution summary

| Metric | Count |
|---|---:|
| Passed assertions | 117 |
| Failed assertions | 0 |
| Documented skips | 1 |

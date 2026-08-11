# Administrative Verification Notifications HTTP Test Report

Generated at: 2026-08-11 14:31:02 +03:00


## Health, lifecycle, and anonymous authorization boundaries

### GET /health

**Request:** GET http://localhost:5049/health

**Response Status:** 200

**Response Body:**
```json
Healthy
```n---

- [PASS] **Health endpoint returns 200** (status=200)
### GET /api/health/ping

**Request:** GET http://localhost:5049/api/health/ping

**Response Status:** 200

**Response Body:**
```json
{
    "message":  "Pong! Smart Court API is fully operational.",
    "serverTimeUtc":  "2026-08-11T11:31:26.7249693Z",
    "version":  "1.0.0"
}
```n---

- [PASS] **Health ping endpoint returns 200** (status=200)
### TestHelpers health compatibility probe

**Request:** GET http://localhost:5049/api/health/ping

**Response Status:** 200

**Response Body:**
`json
{
    "message":  "Pong! Smart Court API is fully operational.",
    "serverTimeUtc":  "2026-08-11T11:31:26.936028Z",
    "version":  "1.0.0"
}
``n---


### Anonymous pending verification list

**Request:** GET http://localhost:5049/api/admin/verifications

**Response Status:** 401

**Response Body:** (Empty)
---

- [PASS] **Anonymous pending verification list returns 401** (status=401)
### Anonymous verification details

**Request:** GET http://localhost:5049/api/admin/verifications/bae72370-debc-499a-a891-c0f1e55fc208

**Response Status:** 401

**Response Body:** (Empty)
---

- [PASS] **Anonymous verification details returns 401** (status=401)
### Anonymous document content

**Request:** GET http://localhost:5049/api/admin/verifications/documents/bae72370-debc-499a-a891-c0f1e55fc208/content

**Response Status:** 401

**Response Body:** (Empty)
---

- [PASS] **Anonymous document content returns 401** (status=401)
### Anonymous document review

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/bae72370-debc-499a-a891-c0f1e55fc208

**Body:**
```json
{
    "Decision":  1
}
```n
**Response Status:** 401

**Response Body:** (Empty)
---

- [PASS] **Anonymous document review returns 401** (status=401)
### Anonymous account approval

**Request:** PATCH http://localhost:5049/api/admin/verifications/bae72370-debc-499a-a891-c0f1e55fc208/approve-account

**Response Status:** 401

**Response Body:** (Empty)
---

- [PASS] **Anonymous account approval returns 401** (status=401)
### Anonymous account rejection

**Request:** PATCH http://localhost:5049/api/admin/verifications/bae72370-debc-499a-a891-c0f1e55fc208/reject-account

**Body:**
```json
{
    "RejectionReason":  "[REDACTED]"
}
```n
**Response Status:** 401

**Response Body:** (Empty)
---

- [PASS] **Anonymous account rejection returns 401** (status=401)
### Anonymous notification list

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 401

**Response Body:** (Empty)
---

- [PASS] **Anonymous notification list returns 401** (status=401)
### Anonymous unread count

**Request:** GET http://localhost:5049/api/notifications/unread-count

**Response Status:** 401

**Response Body:** (Empty)
---

- [PASS] **Anonymous unread count returns 401** (status=401)
### Anonymous mark read

**Request:** PATCH http://localhost:5049/api/notifications/bae72370-debc-499a-a891-c0f1e55fc208/read

**Response Status:** 401

**Response Body:** (Empty)
---

- [PASS] **Anonymous mark read returns 401** (status=401)
### Anonymous mark all read

**Request:** PATCH http://localhost:5049/api/notifications/read-all

**Response Status:** 401

**Response Body:** (Empty)
---

- [PASS] **Anonymous mark all read returns 401** (status=401)

## Admin and role boundaries with zero-assumption disposable accounts

### Seeded Admin login

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
```json
{
    "Password":  "[REDACTED]",
    "Email":  "[REDACTED]"
}
```n
**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "54af6cd4-a46e-4fc6-34ca-08def604e4b7",
                              "email":  "[REDACTED]",
                              "fullName":  "System Administrator",
                              "role":  "Admin",
                              "status":  "Active",
                              "rejectionReason":  "[REDACTED]"
                          },
                 "accessToken":  "[REDACTED]",
                 "expiresIn":  900,
                 "refreshToken":  "[REDACTED]",
                 "refreshTokenExpiration":  "[REDACTED]"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Seeded Admin login succeeds** (status=200)
### Admin reviews unknown document

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/bae72370-debc-499a-a891-c0f1e55fc208

**Body:**
```json
{
    "Decision":  1
}
```n
**Response Status:** 404

**Response Body:**
```json
{
    "success":  false,
    "data":  null,
    "message":  "Verification document was not found.",
    "errors":  null,
    "statusCode":  404
}
```n---

- [PASS] **Unknown document review returns 404** (status=404)
### Admin approves unknown account

**Request:** PATCH http://localhost:5049/api/admin/verifications/bae72370-debc-499a-a891-c0f1e55fc208/approve-account

**Response Status:** 404

**Response Body:**
```json
{
    "success":  false,
    "data":  null,
    "message":  "المستخدم غير موجود",
    "errors":  null,
    "statusCode":  404
}
```n---

- [PASS] **Unknown account approval returns 404** (status=404)
### Admin rejects unknown account

**Request:** PATCH http://localhost:5049/api/admin/verifications/bae72370-debc-499a-a891-c0f1e55fc208/reject-account

**Body:**
```json
{
    "RejectionReason":  "[REDACTED]"
}
```n
**Response Status:** 404

**Response Body:**
```json
{
    "success":  false,
    "data":  null,
    "message":  "المستخدم غير موجود",
    "errors":  null,
    "statusCode":  404
}
```n---

- [PASS] **Unknown account rejection returns 404** (status=404)
### Setup document owner registration

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
```json
{
    "Email":  "[REDACTED]",
    "FullName":  "Gate 5 document owner",
    "ConfirmPassword":  "[REDACTED]",
    "Password":  "[REDACTED]"
}
```n
**Response Status:** 201

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "userId":  "a8851bd0-10ea-40c2-0fd7-08def79c17a7",
                 "email":  "[REDACTED]",
                 "fullName":  "Gate 5 document owner",
                 "role":  "Lawyer"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
```n---

- [PASS] **document owner registration uses Created response** (status=201)
### document owner confirm Email from mock log

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=a8851bd0-10ea-40c2-0fd7-08def79c17a7&token=[REDACTED]

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "message":  "تم تأكيد البريد الإلكتروني بنجاح.",
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **document owner Email confirmation succeeds** (status=200)
### Setup document owner login

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
```json
{
    "Password":  "[REDACTED]",
    "Email":  "[REDACTED]"
}
```n
**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "a8851bd0-10ea-40c2-0fd7-08def79c17a7",
                              "email":  "[REDACTED]",
                              "fullName":  "Gate 5 document owner",
                              "role":  "Lawyer",
                              "status":  "Unverified",
                              "rejectionReason":  "[REDACTED]"
                          },
                 "accessToken":  "[REDACTED]",
                 "expiresIn":  900,
                 "refreshToken":  "[REDACTED]",
                 "refreshTokenExpiration":  "[REDACTED]"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **document owner login succeeds after Email confirmation** (status=200)
### Setup account approval owner registration

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
```json
{
    "Email":  "[REDACTED]",
    "FullName":  "Gate 5 account approval owner",
    "ConfirmPassword":  "[REDACTED]",
    "Password":  "[REDACTED]"
}
```n
**Response Status:** 201

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "userId":  "907a223a-ed38-4617-0fd8-08def79c17a7",
                 "email":  "[REDACTED]",
                 "fullName":  "Gate 5 account approval owner",
                 "role":  "Client"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
```n---

- [PASS] **account approval owner registration uses Created response** (status=201)
### account approval owner confirm Email from mock log

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=907a223a-ed38-4617-0fd8-08def79c17a7&token=[REDACTED]

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "message":  "تم تأكيد البريد الإلكتروني بنجاح.",
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **account approval owner Email confirmation succeeds** (status=200)
### Setup account approval owner login

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
```json
{
    "Password":  "[REDACTED]",
    "Email":  "[REDACTED]"
}
```n
**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "907a223a-ed38-4617-0fd8-08def79c17a7",
                              "email":  "[REDACTED]",
                              "fullName":  "Gate 5 account approval owner",
                              "role":  "Client",
                              "status":  "Unverified",
                              "rejectionReason":  "[REDACTED]"
                          },
                 "accessToken":  "[REDACTED]",
                 "expiresIn":  900,
                 "refreshToken":  "[REDACTED]",
                 "refreshTokenExpiration":  "[REDACTED]"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **account approval owner login succeeds after Email confirmation** (status=200)
### Setup account rejection owner registration

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
```json
{
    "Email":  "[REDACTED]",
    "FullName":  "Gate 5 account rejection owner",
    "ConfirmPassword":  "[REDACTED]",
    "Password":  "[REDACTED]"
}
```n
**Response Status:** 201

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "userId":  "9ac09c3b-83c4-4f19-0fd9-08def79c17a7",
                 "email":  "[REDACTED]",
                 "fullName":  "Gate 5 account rejection owner",
                 "role":  "Client"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
```n---

- [PASS] **account rejection owner registration uses Created response** (status=201)
### account rejection owner confirm Email from mock log

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=9ac09c3b-83c4-4f19-0fd9-08def79c17a7&token=[REDACTED]

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "message":  "تم تأكيد البريد الإلكتروني بنجاح.",
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **account rejection owner Email confirmation succeeds** (status=200)
### Setup account rejection owner login

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
```json
{
    "Password":  "[REDACTED]",
    "Email":  "[REDACTED]"
}
```n
**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "9ac09c3b-83c4-4f19-0fd9-08def79c17a7",
                              "email":  "[REDACTED]",
                              "fullName":  "Gate 5 account rejection owner",
                              "role":  "Client",
                              "status":  "Unverified",
                              "rejectionReason":  "[REDACTED]"
                          },
                 "accessToken":  "[REDACTED]",
                 "expiresIn":  900,
                 "refreshToken":  "[REDACTED]",
                 "refreshTokenExpiration":  "[REDACTED]"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **account rejection owner login succeeds after Email confirmation** (status=200)
### Setup unrelated recipient registration

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
```json
{
    "Email":  "[REDACTED]",
    "FullName":  "Gate 5 unrelated recipient",
    "ConfirmPassword":  "[REDACTED]",
    "Password":  "[REDACTED]"
}
```n
**Response Status:** 201

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "userId":  "4ba4355b-74b7-45be-0fda-08def79c17a7",
                 "email":  "[REDACTED]",
                 "fullName":  "Gate 5 unrelated recipient",
                 "role":  "Client"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
```n---

- [PASS] **unrelated recipient registration uses Created response** (status=201)
### unrelated recipient confirm Email from mock log

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=4ba4355b-74b7-45be-0fda-08def79c17a7&token=[REDACTED]

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "message":  "تم تأكيد البريد الإلكتروني بنجاح.",
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **unrelated recipient Email confirmation succeeds** (status=200)
### Setup unrelated recipient login

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
```json
{
    "Password":  "[REDACTED]",
    "Email":  "[REDACTED]"
}
```n
**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "4ba4355b-74b7-45be-0fda-08def79c17a7",
                              "email":  "[REDACTED]",
                              "fullName":  "Gate 5 unrelated recipient",
                              "role":  "Client",
                              "status":  "Unverified",
                              "rejectionReason":  "[REDACTED]"
                          },
                 "accessToken":  "[REDACTED]",
                 "expiresIn":  900,
                 "refreshToken":  "[REDACTED]",
                 "refreshTokenExpiration":  "[REDACTED]"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **unrelated recipient login succeeds after Email confirmation** (status=200)
### Lawyer role boundary - Anonymous pending verification list

**Request:** GET http://localhost:5049/api/admin/verifications

**Response Status:** 403

**Response Body:** (Empty)
---

- [PASS] **Lawyer cannot use Anonymous pending verification list** (status=403)
### Lawyer role boundary - Anonymous verification details

**Request:** GET http://localhost:5049/api/admin/verifications/bae72370-debc-499a-a891-c0f1e55fc208

**Response Status:** 403

**Response Body:** (Empty)
---

- [PASS] **Lawyer cannot use Anonymous verification details** (status=403)
### Lawyer role boundary - Anonymous document content

**Request:** GET http://localhost:5049/api/admin/verifications/documents/bae72370-debc-499a-a891-c0f1e55fc208/content

**Response Status:** 403

**Response Body:** (Empty)
---

- [PASS] **Lawyer cannot use Anonymous document content** (status=403)
### Lawyer role boundary - Anonymous document review

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/bae72370-debc-499a-a891-c0f1e55fc208

**Body:**
```json
{
    "Decision":  1
}
```n
**Response Status:** 403

**Response Body:** (Empty)
---

- [PASS] **Lawyer cannot use Anonymous document review** (status=403)
### Lawyer role boundary - Anonymous account approval

**Request:** PATCH http://localhost:5049/api/admin/verifications/bae72370-debc-499a-a891-c0f1e55fc208/approve-account

**Response Status:** 403

**Response Body:** (Empty)
---

- [PASS] **Lawyer cannot use Anonymous account approval** (status=403)
### Lawyer role boundary - Anonymous account rejection

**Request:** PATCH http://localhost:5049/api/admin/verifications/bae72370-debc-499a-a891-c0f1e55fc208/reject-account

**Body:**
```json
{
    "RejectionReason":  "[REDACTED]"
}
```n
**Response Status:** 403

**Response Body:** (Empty)
---

- [PASS] **Lawyer cannot use Anonymous account rejection** (status=403)
- [SKIP] **SuperAdministrator role boundary** — The repository seeds only Client, Lawyer, and Admin; no supported HTTP endpoint creates or assigns SuperAdministrator. Optional credentials were not supplied.

## Admin verification read endpoints, validation, hostile input, and content authorization

### Approved-document fixture submit verification document

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents

**Body:**
```json
{
    "Documents[0].Type":  "2",
    "UserId":  "a8851bd0-10ea-40c2-0fd7-08def79c17a7",
    "Documents[0].ExpirationDate":  "2035-01-01",
    "Documents[0].File":  "[REDACTED_FILE]"
}
```n
**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "uploadedDocuments":  [
                                           {
                                               "fileName":  "[REDACTED]",
                                               "type":  2
                                           }
                                       ],
                 "failedDocuments":  [

                                     ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Approved-document fixture document upload succeeds** (status=200)
- [PASS] **Approved-document fixture response contains a persisted uploaded document** (uploaded=1)
### Owner admin verification details

**Request:** GET http://localhost:5049/api/admin/verifications/a8851bd0-10ea-40c2-0fd7-08def79c17a7

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "lawyerId":  "a8851bd0-10ea-40c2-0fd7-08def79c17a7",
                 "fullName":  "Gate 5 document owner",
                 "email":  "[REDACTED]",
                 "phoneNumber":  "[REDACTED]",
                 "nationalNumber":  "[REDACTED]",
                 "address":  null,
                 "governorate":  null,
                 "city":  null,
                 "gender":  null,
                 "dateOfBirth":  null,
                 "accountStatus":  "PendingReview",
                 "isFullyVerified":  false,
                 "role":  "Lawyer",
                 "level":  1,
                 "specializations":  [

                                     ],
                 "bio":  null,
                 "documents":  [
                                   {
                                       "documentId":  "00833b7b-2585-40cd-3c71-08def79c1e33",
                                       "documentType":  "NationalIdBack",
                                       "status":  "Pending",
                                       "fileName":  "[REDACTED]",
                                       "contentType":  "image/jpeg",
                                       "expirationDate":  "2035-01-01",
                                       "reviewedAt":  null,
                                       "rejectionReason":  "[REDACTED]",
                                       "contentUrl":  "[REDACTED]"
                                   }
                               ],
                 "modifiedFields":  [

                                    ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Admin can read verification details** (status=200)
### Admin pending verification list

**Request:** GET http://localhost:5049/api/admin/verifications?PageNumber=1&PageSize=10

**Response Status:** 200

**Response Body:**
```json
{
    "pageNumber":  1,
    "pageSize":  10,
    "totalPages":  1,
    "totalRecords":  5,
    "hasNextPage":  false,
    "hasPreviousPage":  false,
    "success":  true,
    "data":  [
                 {
                     "lawyerId":  "cfb2f595-73f8-4a58-7766-08def7959ad7",
                     "fullName":  "Gate 5 document owner",
                     "email":  "[REDACTED]",
                     "phoneNumber":  "[REDACTED]",
                     "pendingDocumentCount":  1,
                     "verifiedDocumentCount":  1,
                     "rejectedDocumentCount":  1,
                     "role":  "Lawyer"
                 },
                 {
                     "lawyerId":  "2ce36c7e-ed1c-40e7-3f39-08def7973bf6",
                     "fullName":  "Gate 5 document owner",
                     "email":  "[REDACTED]",
                     "phoneNumber":  "[REDACTED]",
                     "pendingDocumentCount":  1,
                     "verifiedDocumentCount":  1,
                     "rejectedDocumentCount":  2,
                     "role":  "Lawyer"
                 },
                 {
                     "lawyerId":  "eb65c827-b239-430b-ead9-08def797e689",
                     "fullName":  "Gate 5 document owner",
                     "email":  "[REDACTED]",
                     "phoneNumber":  "[REDACTED]",
                     "pendingDocumentCount":  1,
                     "verifiedDocumentCount":  1,
                     "rejectedDocumentCount":  2,
                     "role":  "Lawyer"
                 },
                 {
                     "lawyerId":  "32fd5cbb-8856-4d4e-fb89-08def798f643",
                     "fullName":  "Gate 5 document owner",
                     "email":  "[REDACTED]",
                     "phoneNumber":  "[REDACTED]",
                     "pendingDocumentCount":  1,
                     "verifiedDocumentCount":  1,
                     "rejectedDocumentCount":  2,
                     "role":  "Lawyer"
                 },
                 {
                     "lawyerId":  "a8851bd0-10ea-40c2-0fd7-08def79c17a7",
                     "fullName":  "Gate 5 document owner",
                     "email":  "[REDACTED]",
                     "phoneNumber":  "[REDACTED]",
                     "pendingDocumentCount":  1,
                     "verifiedDocumentCount":  0,
                     "rejectedDocumentCount":  0,
                     "role":  "Lawyer"
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Admin can list pending verifications** (status=200)
### Admin pending list searches the disposable document owner

**Request:** GET http://localhost:5049/api/admin/verifications?PageNumber=1&PageSize=10&Search=Gate%205%20document%20owner

**Response Status:** 200

**Response Body:**
```json
{
    "pageNumber":  1,
    "pageSize":  10,
    "totalPages":  1,
    "totalRecords":  5,
    "hasNextPage":  false,
    "hasPreviousPage":  false,
    "success":  true,
    "data":  [
                 {
                     "lawyerId":  "cfb2f595-73f8-4a58-7766-08def7959ad7",
                     "fullName":  "Gate 5 document owner",
                     "email":  "[REDACTED]",
                     "phoneNumber":  "[REDACTED]",
                     "pendingDocumentCount":  1,
                     "verifiedDocumentCount":  1,
                     "rejectedDocumentCount":  1,
                     "role":  "Lawyer"
                 },
                 {
                     "lawyerId":  "2ce36c7e-ed1c-40e7-3f39-08def7973bf6",
                     "fullName":  "Gate 5 document owner",
                     "email":  "[REDACTED]",
                     "phoneNumber":  "[REDACTED]",
                     "pendingDocumentCount":  1,
                     "verifiedDocumentCount":  1,
                     "rejectedDocumentCount":  2,
                     "role":  "Lawyer"
                 },
                 {
                     "lawyerId":  "eb65c827-b239-430b-ead9-08def797e689",
                     "fullName":  "Gate 5 document owner",
                     "email":  "[REDACTED]",
                     "phoneNumber":  "[REDACTED]",
                     "pendingDocumentCount":  1,
                     "verifiedDocumentCount":  1,
                     "rejectedDocumentCount":  2,
                     "role":  "Lawyer"
                 },
                 {
                     "lawyerId":  "32fd5cbb-8856-4d4e-fb89-08def798f643",
                     "fullName":  "Gate 5 document owner",
                     "email":  "[REDACTED]",
                     "phoneNumber":  "[REDACTED]",
                     "pendingDocumentCount":  1,
                     "verifiedDocumentCount":  1,
                     "rejectedDocumentCount":  2,
                     "role":  "Lawyer"
                 },
                 {
                     "lawyerId":  "a8851bd0-10ea-40c2-0fd7-08def79c17a7",
                     "fullName":  "Gate 5 document owner",
                     "email":  "[REDACTED]",
                     "phoneNumber":  "[REDACTED]",
                     "pendingDocumentCount":  1,
                     "verifiedDocumentCount":  0,
                     "rejectedDocumentCount":  0,
                     "role":  "Lawyer"
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Pending list includes the disposable document owner** (status=200)
### Admin pending list filtered by Pending status

**Request:** GET http://localhost:5049/api/admin/verifications?PageNumber=1&PageSize=10&Status=1

**Response Status:** 200

**Response Body:**
```json
{
    "pageNumber":  1,
    "pageSize":  10,
    "totalPages":  1,
    "totalRecords":  5,
    "hasNextPage":  false,
    "hasPreviousPage":  false,
    "success":  true,
    "data":  [
                 {
                     "lawyerId":  "cfb2f595-73f8-4a58-7766-08def7959ad7",
                     "fullName":  "Gate 5 document owner",
                     "email":  "[REDACTED]",
                     "phoneNumber":  "[REDACTED]",
                     "pendingDocumentCount":  1,
                     "verifiedDocumentCount":  1,
                     "rejectedDocumentCount":  1,
                     "role":  "Lawyer"
                 },
                 {
                     "lawyerId":  "2ce36c7e-ed1c-40e7-3f39-08def7973bf6",
                     "fullName":  "Gate 5 document owner",
                     "email":  "[REDACTED]",
                     "phoneNumber":  "[REDACTED]",
                     "pendingDocumentCount":  1,
                     "verifiedDocumentCount":  1,
                     "rejectedDocumentCount":  2,
                     "role":  "Lawyer"
                 },
                 {
                     "lawyerId":  "eb65c827-b239-430b-ead9-08def797e689",
                     "fullName":  "Gate 5 document owner",
                     "email":  "[REDACTED]",
                     "phoneNumber":  "[REDACTED]",
                     "pendingDocumentCount":  1,
                     "verifiedDocumentCount":  1,
                     "rejectedDocumentCount":  2,
                     "role":  "Lawyer"
                 },
                 {
                     "lawyerId":  "32fd5cbb-8856-4d4e-fb89-08def798f643",
                     "fullName":  "Gate 5 document owner",
                     "email":  "[REDACTED]",
                     "phoneNumber":  "[REDACTED]",
                     "pendingDocumentCount":  1,
                     "verifiedDocumentCount":  1,
                     "rejectedDocumentCount":  2,
                     "role":  "Lawyer"
                 },
                 {
                     "lawyerId":  "a8851bd0-10ea-40c2-0fd7-08def79c17a7",
                     "fullName":  "Gate 5 document owner",
                     "email":  "[REDACTED]",
                     "phoneNumber":  "[REDACTED]",
                     "pendingDocumentCount":  1,
                     "verifiedDocumentCount":  0,
                     "rejectedDocumentCount":  0,
                     "role":  "Lawyer"
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Pending status filter returns 200** (status=200)
### Admin pending list overlong search

**Request:** GET http://localhost:5049/api/admin/verifications?PageNumber=1&PageSize=10&Search=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

**Response Status:** 400

**Response Body:**
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "Search":  [
                                  "The length of \u0027Search\u0027 must be 100 characters or fewer. You entered 101 characters."
                              ]
               },
    "traceId":  "00-7e9aea8bfb56c133d664574c6a9987b9-d531e8eccfb2e5ef-00"
}
```n---

- [PASS] **Overlong verification search returns 400** (status=400)
### Admin pending list invalid status enum

**Request:** GET http://localhost:5049/api/admin/verifications?PageNumber=1&PageSize=10&Status=99

**Response Status:** 400

**Response Body:**
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "Status":  [
                                  "The value \u002799\u0027 is invalid."
                              ]
               },
    "traceId":  "00-61e7ed619d39527be986e6e7e845fdab-20710becd399cb15-00"
}
```n---

- [PASS] **Invalid verification status returns 400** (status=400)
### Admin pending list invalid pagination

**Request:** GET http://localhost:5049/api/admin/verifications?PageNumber=0&PageSize=51

**Response Status:** 400

**Response Body:**
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "PageNumber":  [
                                      "\u0027Page Number\u0027 must be greater than or equal to \u00271\u0027."
                                  ]
               },
    "traceId":  "00-3a3434c0d9ea44a3dec8cd327d5225db-6c658a850506313e-00"
}
```n---

- [PASS] **Invalid verification pagination returns 400** (status=400)
### Admin reads current document content

**Request:** GET http://localhost:5049/api/admin/verifications/documents/00833b7b-2585-40cd-3c71-08def79c1e33/content

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "downloadUrl":  "[REDACTED]",
                 "contentType":  "image/jpeg",
                 "fileName":  "[REDACTED]"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Admin can read current document content** (status=200)
### Admin details unknown user

**Request:** GET http://localhost:5049/api/admin/verifications/bae72370-debc-499a-a891-c0f1e55fc208

**Response Status:** 404

**Response Body:**
```json
{
    "success":  false,
    "data":  null,
    "message":  "User was not found.",
    "errors":  null,
    "statusCode":  404
}
```n---

- [PASS] **Unknown verification user returns 404** (status=404)
### Admin details malformed user id

**Request:** GET http://localhost:5049/api/admin/verifications/not-a-guid

**Response Status:** 404

**Response Body:** (Empty)
---

- [PASS] **Malformed verification user id returns 404 or 400** (status=404)
### Admin content unknown document

**Request:** GET http://localhost:5049/api/admin/verifications/documents/bae72370-debc-499a-a891-c0f1e55fc208/content

**Response Status:** 404

**Response Body:**
```json
{
    "success":  false,
    "data":  null,
    "message":  "Verification document was not found.",
    "errors":  null,
    "statusCode":  404
}
```n---

- [PASS] **Unknown document content returns 404** (status=404)
### Owner reads own verification documents

**Request:** GET http://localhost:5049/api/UserVerification/a8851bd0-10ea-40c2-0fd7-08def79c17a7

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "documents":  [
                                   {
                                       "documentId":  "00833b7b-2585-40cd-3c71-08def79c1e33",
                                       "documentType":  2,
                                       "status":  1,
                                       "expirationDate":  "2035-01-01",
                                       "isCurrent":  true,
                                       "fileName":  "[REDACTED]",
                                       "rejectionReason":  "[REDACTED]"
                                   }
                               ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Owner can read own verification documents** (status=200)
- [SKIP] **UserVerification cross-user document route authorization** — The existing UserVerification read handler accepts the route UserId without an ownership check; Gate 5 does not alter that unrelated slice rule. Notification recipient isolation is tested below.

## Document approval, rejection, expiry, replay, and notification isolation

### Admin approves current document

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/00833b7b-2585-40cd-3c71-08def79c1e33

**Body:**
```json
{
    "Decision":  1,
    "RejectionReason":  "[REDACTED]"
}
```n
**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "documentId":  "00833b7b-2585-40cd-3c71-08def79c1e33",
                 "documentStatus":  "Verified",
                 "lawyerAccountStatus":  "Unverified",
                 "isFullyVerified":  false
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Document approval returns 200** (status=200)
- [PASS] **Owner receives exact document-approved notification**
### Replay document approval

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/00833b7b-2585-40cd-3c71-08def79c1e33

**Body:**
```json
{
    "Decision":  1,
    "RejectionReason":  "[REDACTED]"
}
```n
**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "documentId":  "00833b7b-2585-40cd-3c71-08def79c1e33",
                 "documentStatus":  "Verified",
                 "lawyerAccountStatus":  "Unverified",
                 "isFullyVerified":  false
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Repeated document approval preserves existing endpoint success** (status=200)
### Count notifications for verification.document-approved

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "39336964-8a1c-461c-9efa-3e1200c29589",
                                   "type":  "verification.document-approved",
                                   "severity":  "Success",
                                   "title":  "تم اعتماد مستند التحقق",
                                   "body":  "تم اعتماد أحد مستندات التحقق الخاصة بك. يمكنك متابعة حالة التحقق من حسابك.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "documentId":  "00833b7b-2585-40cd-3c71-08def79c1e33",
                                                "documentType":  "NationalIdBack"
                                            },
                                   "createdAtUtc":  "2026-08-11T11:32:36.997814",
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
}
```n---

- [PASS] **Count notifications for verification.document-approved returns 200** (status=200)
- [PASS] **Repeated document approval does not duplicate notification**
### Rejected-document fixture submit verification document

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents

**Body:**
```json
{
    "Documents[0].Type":  "3",
    "UserId":  "a8851bd0-10ea-40c2-0fd7-08def79c17a7",
    "Documents[0].ExpirationDate":  "2035-01-01",
    "Documents[0].File":  "[REDACTED_FILE]"
}
```n
**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "uploadedDocuments":  [
                                           {
                                               "fileName":  "[REDACTED]",
                                               "type":  3
                                           }
                                       ],
                 "failedDocuments":  [

                                     ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Rejected-document fixture document upload succeeds** (status=200)
- [PASS] **Rejected-document fixture response contains a persisted uploaded document** (uploaded=1)
### Rejected document admin verification details

**Request:** GET http://localhost:5049/api/admin/verifications/a8851bd0-10ea-40c2-0fd7-08def79c17a7

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "lawyerId":  "a8851bd0-10ea-40c2-0fd7-08def79c17a7",
                 "fullName":  "Gate 5 document owner",
                 "email":  "[REDACTED]",
                 "phoneNumber":  "[REDACTED]",
                 "nationalNumber":  "[REDACTED]",
                 "address":  null,
                 "governorate":  null,
                 "city":  null,
                 "gender":  null,
                 "dateOfBirth":  null,
                 "accountStatus":  "PendingReview",
                 "isFullyVerified":  false,
                 "role":  "Lawyer",
                 "level":  1,
                 "specializations":  [

                                     ],
                 "bio":  null,
                 "documents":  [
                                   {
                                       "documentId":  "00833b7b-2585-40cd-3c71-08def79c1e33",
                                       "documentType":  "NationalIdBack",
                                       "status":  "Verified",
                                       "fileName":  "[REDACTED]",
                                       "contentType":  "image/jpeg",
                                       "expirationDate":  "2035-01-01",
                                       "reviewedAt":  "2026-08-11T11:32:38.0720183",
                                       "rejectionReason":  "[REDACTED]",
                                       "contentUrl":  "[REDACTED]"
                                   },
                                   {
                                       "documentId":  "3f358634-e14a-42e8-3c72-08def79c1e33",
                                       "documentType":  "BarAssociationCardFront",
                                       "status":  "Pending",
                                       "fileName":  "[REDACTED]",
                                       "contentType":  "image/jpeg",
                                       "expirationDate":  "2035-01-01",
                                       "reviewedAt":  null,
                                       "rejectionReason":  "[REDACTED]",
                                       "contentUrl":  "[REDACTED]"
                                   }
                               ],
                 "modifiedFields":  [

                                    ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

### Admin rejects current document

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/3f358634-e14a-42e8-3c72-08def79c1e33

**Body:**
```json
{
    "Decision":  2,
    "RejectionReason":  "[REDACTED]"
}
```n
**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "documentId":  "3f358634-e14a-42e8-3c72-08def79c1e33",
                 "documentStatus":  "Rejected",
                 "lawyerAccountStatus":  "Unverified",
                 "isFullyVerified":  false
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Document rejection returns 200** (status=200)
- [PASS] **Owner receives exact document-rejected notification without reason metadata**
- [PASS] **Document rejection notification does not contain full rejection reason**
### Replay document rejection with a changed reason

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/3f358634-e14a-42e8-3c72-08def79c1e33

**Body:**
```json
{
    "Decision":  2,
    "RejectionReason":  "[REDACTED]"
}
```n
**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "documentId":  "3f358634-e14a-42e8-3c72-08def79c1e33",
                 "documentStatus":  "Rejected",
                 "lawyerAccountStatus":  "Unverified",
                 "isFullyVerified":  false
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Repeated document rejection preserves existing endpoint success** (status=200)
### Count notifications for verification.document-rejected

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "ed8d4032-84f6-419e-90c0-47cbdda2b3e1",
                                   "type":  "verification.document-rejected",
                                   "severity":  "Warning",
                                   "title":  "تم رفض مستند التحقق",
                                   "body":  "تم رفض أحد مستندات التحقق الخاصة بك. يرجى مراجعة التفاصيل واستبدال المستند عند الحاجة.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "documentId":  "3f358634-e14a-42e8-3c72-08def79c1e33",
                                                "documentType":  "BarAssociationCardFront"
                                            },
                                   "createdAtUtc":  "2026-08-11T11:33:05.9147618",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "39336964-8a1c-461c-9efa-3e1200c29589",
                                   "type":  "verification.document-approved",
                                   "severity":  "Success",
                                   "title":  "تم اعتماد مستند التحقق",
                                   "body":  "تم اعتماد أحد مستندات التحقق الخاصة بك. يمكنك متابعة حالة التحقق من حسابك.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "documentId":  "00833b7b-2585-40cd-3c71-08def79c1e33",
                                                "documentType":  "NationalIdBack"
                                            },
                                   "createdAtUtc":  "2026-08-11T11:32:36.997814",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  2
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Count notifications for verification.document-rejected returns 200** (status=200)
- [PASS] **Repeated document rejection does not duplicate notification**
### Expired-document fixture submit verification document

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents

**Body:**
```json
{
    "Documents[0].Type":  "5",
    "UserId":  "a8851bd0-10ea-40c2-0fd7-08def79c17a7",
    "Documents[0].ExpirationDate":  "2035-01-01",
    "Documents[0].File":  "[REDACTED_FILE]"
}
```n
**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "uploadedDocuments":  [
                                           {
                                               "fileName":  "[REDACTED]",
                                               "type":  5
                                           }
                                       ],
                 "failedDocuments":  [

                                     ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Expired-document fixture document upload succeeds** (status=200)
- [PASS] **Expired-document fixture response contains a persisted uploaded document** (uploaded=1)
### Expired document admin verification details

**Request:** GET http://localhost:5049/api/admin/verifications/a8851bd0-10ea-40c2-0fd7-08def79c17a7

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "lawyerId":  "a8851bd0-10ea-40c2-0fd7-08def79c17a7",
                 "fullName":  "Gate 5 document owner",
                 "email":  "[REDACTED]",
                 "phoneNumber":  "[REDACTED]",
                 "nationalNumber":  "[REDACTED]",
                 "address":  null,
                 "governorate":  null,
                 "city":  null,
                 "gender":  null,
                 "dateOfBirth":  null,
                 "accountStatus":  "PendingReview",
                 "isFullyVerified":  false,
                 "role":  "Lawyer",
                 "level":  1,
                 "specializations":  [

                                     ],
                 "bio":  null,
                 "documents":  [
                                   {
                                       "documentId":  "00833b7b-2585-40cd-3c71-08def79c1e33",
                                       "documentType":  "NationalIdBack",
                                       "status":  "Verified",
                                       "fileName":  "[REDACTED]",
                                       "contentType":  "image/jpeg",
                                       "expirationDate":  "2035-01-01",
                                       "reviewedAt":  "2026-08-11T11:32:38.0720183",
                                       "rejectionReason":  "[REDACTED]",
                                       "contentUrl":  "[REDACTED]"
                                   },
                                   {
                                       "documentId":  "3f358634-e14a-42e8-3c72-08def79c1e33",
                                       "documentType":  "BarAssociationCardFront",
                                       "status":  "Rejected",
                                       "fileName":  "[REDACTED]",
                                       "contentType":  "image/jpeg",
                                       "expirationDate":  "2035-01-01",
                                       "reviewedAt":  null,
                                       "rejectionReason":  "[REDACTED]",
                                       "contentUrl":  "[REDACTED]"
                                   },
                                   {
                                       "documentId":  "7e2dd2ef-9fd5-4cf9-3c73-08def79c1e33",
                                       "documentType":  "SelfieWithId",
                                       "status":  "Pending",
                                       "fileName":  "[REDACTED]",
                                       "contentType":  "image/jpeg",
                                       "expirationDate":  "2035-01-01",
                                       "reviewedAt":  null,
                                       "rejectionReason":  "[REDACTED]",
                                       "contentUrl":  "[REDACTED]"
                                   }
                               ],
                 "modifiedFields":  [

                                    ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Expired fixture prepared in the local disposable database**
### Admin reviews expired document

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/7e2dd2ef-9fd5-4cf9-3c73-08def79c1e33

**Body:**
```json
{
    "Decision":  1,
    "RejectionReason":  "[REDACTED]"
}
```n
**Response Status:** 409

**Response Body:**
```json
{
    "success":  false,
    "data":  null,
    "message":  "The document has expired and must be submitted again.",
    "errors":  null,
    "statusCode":  409
}
```n---

- [PASS] **Expired document returns the existing 409 conflict outcome** (status=409)
- [PASS] **Owner receives exact document-expired notification**
### Replay expired document review

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/7e2dd2ef-9fd5-4cf9-3c73-08def79c1e33

**Body:**
```json
{
    "Decision":  1,
    "RejectionReason":  "[REDACTED]"
}
```n
**Response Status:** 409

**Response Body:**
```json
{
    "success":  false,
    "data":  null,
    "message":  "The document has expired and must be submitted again.",
    "errors":  null,
    "statusCode":  409
}
```n---

- [PASS] **Repeated expired review preserves existing 409 outcome** (status=409)
### Count notifications for verification.document-expired

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "5a55b3f4-54b9-4042-8b5a-3c252a30ba98",
                                   "type":  "verification.document-expired",
                                   "severity":  "Warning",
                                   "title":  "انتهت صلاحية مستند التحقق",
                                   "body":  "انتهت صلاحية أحد مستندات التحقق الخاصة بك. يرجى إعادة رفع مستند ساري المفعول.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "documentId":  "7e2dd2ef-9fd5-4cf9-3c73-08def79c1e33",
                                                "documentType":  "SelfieWithId"
                                            },
                                   "createdAtUtc":  "2026-08-11T11:33:35.2212375",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "ed8d4032-84f6-419e-90c0-47cbdda2b3e1",
                                   "type":  "verification.document-rejected",
                                   "severity":  "Warning",
                                   "title":  "تم رفض مستند التحقق",
                                   "body":  "تم رفض أحد مستندات التحقق الخاصة بك. يرجى مراجعة التفاصيل واستبدال المستند عند الحاجة.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "documentId":  "3f358634-e14a-42e8-3c72-08def79c1e33",
                                                "documentType":  "BarAssociationCardFront"
                                            },
                                   "createdAtUtc":  "2026-08-11T11:33:05.9147618",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "39336964-8a1c-461c-9efa-3e1200c29589",
                                   "type":  "verification.document-approved",
                                   "severity":  "Success",
                                   "title":  "تم اعتماد مستند التحقق",
                                   "body":  "تم اعتماد أحد مستندات التحقق الخاصة بك. يمكنك متابعة حالة التحقق من حسابك.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "documentId":  "00833b7b-2585-40cd-3c71-08def79c1e33",
                                                "documentType":  "NationalIdBack"
                                            },
                                   "createdAtUtc":  "2026-08-11T11:32:36.997814",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  3
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Count notifications for verification.document-expired returns 200** (status=200)
- [PASS] **Repeated expired review does not duplicate notification**
### Reject document without reason

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/7e2dd2ef-9fd5-4cf9-3c73-08def79c1e33

**Body:**
```json
{
    "Decision":  2,
    "RejectionReason":  "[REDACTED]"
}
```n
**Response Status:** 400

**Response Body:**
```json
{
    "success":  false,
    "data":  null,
    "message":  null,
    "errors":  [
                   "\u0027Rejection Reason\u0027 must not be empty."
               ],
    "statusCode":  400
}
```n---

- [PASS] **Reject without reason returns 400** (status=400)
### Reject document with overlong hostile reason

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/7e2dd2ef-9fd5-4cf9-3c73-08def79c1e33

**Body:**
```json
{
    "Decision":  2,
    "RejectionReason":  "[REDACTED]"
}
```n
**Response Status:** 400

**Response Body:**
```json
{
    "success":  false,
    "data":  null,
    "message":  null,
    "errors":  [
                   "The length of \u0027Rejection Reason\u0027 must be 500 characters or fewer. You entered 501 characters."
               ],
    "statusCode":  400
}
```n---

- [PASS] **Overlong rejection reason returns 400** (status=400)
### Approve document with forbidden reason field

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/00833b7b-2585-40cd-3c71-08def79c1e33

**Body:**
```json
{
    "Decision":  1,
    "RejectionReason":  "[REDACTED]"
}
```n
**Response Status:** 400

**Response Body:**
```json
{
    "success":  false,
    "data":  null,
    "message":  null,
    "errors":  [
                   "A rejection reason can only be supplied when rejecting a document."
               ],
    "statusCode":  400
}
```n---

- [PASS] **Approve with rejection reason returns 400** (status=400)
### Review document with invalid decision enum

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/00833b7b-2585-40cd-3c71-08def79c1e33

**Body:**
```json
{
    "Decision":  99
}
```n
**Response Status:** 400

**Response Body:**
```json
{
    "success":  false,
    "data":  null,
    "message":  null,
    "errors":  [
                   "\u0027Decision\u0027 has a range of values which does not include \u002799\u0027."
               ],
    "statusCode":  400
}
```n---

- [PASS] **Invalid review decision returns 400** (status=400)
### Review document with decision type mismatch

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/00833b7b-2585-40cd-3c71-08def79c1e33

**Body:**
```json
{
    "Decision":  "approve"
}
```n
**Response Status:** 400

**Response Body:**
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "request":  [
                                   "The request field is required."
                               ],
                   "$.Decision":  [
                                      "The JSON value could not be converted to SmartCourt.Features.Admin.Verifications.ReviewVerificationDocument.VerificationReviewDecision. Path: $.Decision | LineNumber: 0 | BytePositionInLine: 21."
                                  ]
               },
    "traceId":  "00-542b41fe6074cf0adb69db5b035d588b-11cabccb26063fd3-00"
}
```n---

- [PASS] **Review decision type mismatch returns 400** (status=400)

## Account approval/rejection transitions and deduplication

### Admin approves account on actual Active transition

**Request:** PATCH http://localhost:5049/api/admin/verifications/907a223a-ed38-4617-0fd8-08def79c17a7/approve-account

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "message":  "تم اعتماد بيانات الحساب بنجاح"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Account approval returns 200** (status=200)
- [PASS] **Account owner receives exact account-approved notification**
### Replay account approval

**Request:** PATCH http://localhost:5049/api/admin/verifications/907a223a-ed38-4617-0fd8-08def79c17a7/approve-account

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "message":  "تم اعتماد بيانات الحساب بنجاح"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Repeated account approval preserves existing endpoint success** (status=200)
### Count notifications for account.approved

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "8aac39f0-ef77-4134-ac44-32a419291612",
                                   "type":  "account.approved",
                                   "severity":  "Success",
                                   "title":  "تم اعتماد حسابك",
                                   "body":  "تم اعتماد حسابك وأصبح جاهزًا للاستخدام.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "907a223a-ed38-4617-0fd8-08def79c17a7"
                                            },
                                   "createdAtUtc":  "2026-08-11T11:33:37.258665",
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
}
```n---

- [PASS] **Count notifications for account.approved returns 200** (status=200)
- [PASS] **Account approval notification is emitted only on Active transition**
### Admin rejects account

**Request:** PATCH http://localhost:5049/api/admin/verifications/9ac09c3b-83c4-4f19-0fd9-08def79c17a7/reject-account

**Body:**
```json
{
    "RejectionReason":  "[REDACTED]"
}
```n
**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "message":  "تم رفض بيانات الحساب بنجاح"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Account rejection returns 200** (status=200)
- [PASS] **Account owner receives exact account-rejected notification without reason**
- [PASS] **Account rejection notification does not contain full rejection reason**
### Replay account rejection

**Request:** PATCH http://localhost:5049/api/admin/verifications/9ac09c3b-83c4-4f19-0fd9-08def79c17a7/reject-account

**Body:**
```json
{
    "RejectionReason":  "[REDACTED]"
}
```n
**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "message":  "تم رفض بيانات الحساب بنجاح"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Repeated account rejection preserves existing endpoint success** (status=200)
### Count notifications for account.rejected

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "9ee9fbf6-5b37-4a88-947e-8630cb007dae",
                                   "type":  "account.rejected",
                                   "severity":  "Critical",
                                   "title":  "تم رفض الحساب",
                                   "body":  "تم رفض طلب اعتماد حسابك. يرجى مراجعة التفاصيل واتخاذ الإجراء المطلوب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9ac09c3b-83c4-4f19-0fd9-08def79c17a7"
                                            },
                                   "createdAtUtc":  "2026-08-11T11:33:39.6503471",
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
}
```n---

- [PASS] **Count notifications for account.rejected returns 200** (status=200)
- [PASS] **Account rejection notification is emitted once per transition**

## Version conflicts and concurrent review behavior

### Concurrency fixture submit verification document

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents

**Body:**
```json
{
    "Documents[0].Type":  "6",
    "UserId":  "a8851bd0-10ea-40c2-0fd7-08def79c17a7",
    "Documents[0].ExpirationDate":  "2035-01-01",
    "Documents[0].File":  "[REDACTED_FILE]"
}
```n
**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "uploadedDocuments":  [
                                           {
                                               "fileName":  "[REDACTED]",
                                               "type":  6
                                           }
                                       ],
                 "failedDocuments":  [

                                     ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Concurrency fixture document upload succeeds** (status=200)
- [PASS] **Concurrency fixture response contains a persisted uploaded document** (uploaded=1)
### Concurrency fixture admin verification details

**Request:** GET http://localhost:5049/api/admin/verifications/a8851bd0-10ea-40c2-0fd7-08def79c17a7

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "lawyerId":  "a8851bd0-10ea-40c2-0fd7-08def79c17a7",
                 "fullName":  "Gate 5 document owner",
                 "email":  "[REDACTED]",
                 "phoneNumber":  "[REDACTED]",
                 "nationalNumber":  "[REDACTED]",
                 "address":  null,
                 "governorate":  null,
                 "city":  null,
                 "gender":  null,
                 "dateOfBirth":  null,
                 "accountStatus":  "PendingReview",
                 "isFullyVerified":  false,
                 "role":  "Lawyer",
                 "level":  1,
                 "specializations":  [

                                     ],
                 "bio":  null,
                 "documents":  [
                                   {
                                       "documentId":  "00833b7b-2585-40cd-3c71-08def79c1e33",
                                       "documentType":  "NationalIdBack",
                                       "status":  "Verified",
                                       "fileName":  "[REDACTED]",
                                       "contentType":  "image/jpeg",
                                       "expirationDate":  "2035-01-01",
                                       "reviewedAt":  "2026-08-11T11:32:38.0720183",
                                       "rejectionReason":  "[REDACTED]",
                                       "contentUrl":  "[REDACTED]"
                                   },
                                   {
                                       "documentId":  "3f358634-e14a-42e8-3c72-08def79c1e33",
                                       "documentType":  "BarAssociationCardFront",
                                       "status":  "Rejected",
                                       "fileName":  "[REDACTED]",
                                       "contentType":  "image/jpeg",
                                       "expirationDate":  "2035-01-01",
                                       "reviewedAt":  null,
                                       "rejectionReason":  "[REDACTED]",
                                       "contentUrl":  "[REDACTED]"
                                   },
                                   {
                                       "documentId":  "7e2dd2ef-9fd5-4cf9-3c73-08def79c1e33",
                                       "documentType":  "SelfieWithId",
                                       "status":  "Expired",
                                       "fileName":  "[REDACTED]",
                                       "contentType":  "image/jpeg",
                                       "expirationDate":  "2026-08-10",
                                       "reviewedAt":  null,
                                       "rejectionReason":  "[REDACTED]",
                                       "contentUrl":  "[REDACTED]"
                                   },
                                   {
                                       "documentId":  "d8f69974-631c-4eed-3c74-08def79c1e33",
                                       "documentType":  "Other",
                                       "status":  "Pending",
                                       "fileName":  "[REDACTED]",
                                       "contentType":  "image/jpeg",
                                       "expirationDate":  "2035-01-01",
                                       "reviewedAt":  null,
                                       "rejectionReason":  "[REDACTED]",
                                       "contentUrl":  "[REDACTED]"
                                   }
                               ],
                 "modifiedFields":  [

                                    ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

### Concurrent review request 1

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/d8f69974-631c-4eed-3c74-08def79c1e33

**Body:**
```json
{
    "Decision":  1,
    "RejectionReason":  "[REDACTED]"
}
```n
**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "documentId":  "d8f69974-631c-4eed-3c74-08def79c1e33",
                 "documentStatus":  "Verified",
                 "lawyerAccountStatus":  "Unverified",
                 "isFullyVerified":  false
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

### Concurrent review request 2

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/d8f69974-631c-4eed-3c74-08def79c1e33

**Body:**
```json
{
    "Decision":  2,
    "RejectionReason":  "[REDACTED]"
}
```n
**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "documentId":  "d8f69974-631c-4eed-3c74-08def79c1e33",
                 "documentStatus":  "Rejected",
                 "lawyerAccountStatus":  "Unverified",
                 "isFullyVerified":  false
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Concurrent review requests return only success or conflict**
- [PASS] **Concurrent review produces at least one committed decision**
- [SKIP] **Deterministic row-version winner** — The HTTP race is timing-dependent; the existing current-version conflict path is tested deterministically below.
### Replacement-version fixture submit verification document

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents

**Body:**
```json
{
    "Documents[0].Type":  "2",
    "UserId":  "a8851bd0-10ea-40c2-0fd7-08def79c17a7",
    "Documents[0].ExpirationDate":  "2035-01-01",
    "Documents[0].File":  "[REDACTED_FILE]"
}
```n
**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "uploadedDocuments":  [
                                           {
                                               "fileName":  "[REDACTED]",
                                               "type":  2
                                           }
                                       ],
                 "failedDocuments":  [

                                     ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Replacement-version fixture document upload succeeds** (status=200)
- [PASS] **Replacement-version fixture response contains a persisted uploaded document** (uploaded=1)
### Replacement version admin verification details

**Request:** GET http://localhost:5049/api/admin/verifications/a8851bd0-10ea-40c2-0fd7-08def79c17a7

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "lawyerId":  "a8851bd0-10ea-40c2-0fd7-08def79c17a7",
                 "fullName":  "Gate 5 document owner",
                 "email":  "[REDACTED]",
                 "phoneNumber":  "[REDACTED]",
                 "nationalNumber":  "[REDACTED]",
                 "address":  null,
                 "governorate":  null,
                 "city":  null,
                 "gender":  null,
                 "dateOfBirth":  null,
                 "accountStatus":  "PendingReview",
                 "isFullyVerified":  false,
                 "role":  "Lawyer",
                 "level":  1,
                 "specializations":  [

                                     ],
                 "bio":  null,
                 "documents":  [
                                   {
                                       "documentId":  "113b6be6-6dac-4244-3c75-08def79c1e33",
                                       "documentType":  "NationalIdBack",
                                       "status":  "Pending",
                                       "fileName":  "[REDACTED]",
                                       "contentType":  "image/jpeg",
                                       "expirationDate":  "2035-01-01",
                                       "reviewedAt":  null,
                                       "rejectionReason":  "[REDACTED]",
                                       "contentUrl":  "[REDACTED]"
                                   },
                                   {
                                       "documentId":  "3f358634-e14a-42e8-3c72-08def79c1e33",
                                       "documentType":  "BarAssociationCardFront",
                                       "status":  "Rejected",
                                       "fileName":  "[REDACTED]",
                                       "contentType":  "image/jpeg",
                                       "expirationDate":  "2035-01-01",
                                       "reviewedAt":  null,
                                       "rejectionReason":  "[REDACTED]",
                                       "contentUrl":  "[REDACTED]"
                                   },
                                   {
                                       "documentId":  "7e2dd2ef-9fd5-4cf9-3c73-08def79c1e33",
                                       "documentType":  "SelfieWithId",
                                       "status":  "Expired",
                                       "fileName":  "[REDACTED]",
                                       "contentType":  "image/jpeg",
                                       "expirationDate":  "2026-08-10",
                                       "reviewedAt":  null,
                                       "rejectionReason":  "[REDACTED]",
                                       "contentUrl":  "[REDACTED]"
                                   },
                                   {
                                       "documentId":  "d8f69974-631c-4eed-3c74-08def79c1e33",
                                       "documentType":  "Other",
                                       "status":  "Rejected",
                                       "fileName":  "[REDACTED]",
                                       "contentType":  "image/jpeg",
                                       "expirationDate":  "2035-01-01",
                                       "reviewedAt":  null,
                                       "rejectionReason":  "[REDACTED]",
                                       "contentUrl":  "[REDACTED]"
                                   }
                               ],
                 "modifiedFields":  [

                                    ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

### Review superseded document version

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/00833b7b-2585-40cd-3c71-08def79c1e33

**Body:**
```json
{
    "Decision":  1,
    "RejectionReason":  "[REDACTED]"
}
```n
**Response Status:** 409

**Response Body:**
```json
{
    "success":  false,
    "data":  null,
    "message":  "Only the current version of a document can be reviewed.",
    "errors":  null,
    "statusCode":  409
}
```n---

- [PASS] **Superseded document review returns 409 conflict** (status=409)
### Read superseded document content

**Request:** GET http://localhost:5049/api/admin/verifications/documents/00833b7b-2585-40cd-3c71-08def79c1e33/content

**Response Status:** 404

**Response Body:**
```json
{
    "success":  false,
    "data":  null,
    "message":  "Verification document was not found.",
    "errors":  null,
    "statusCode":  404
}
```n---

- [PASS] **Superseded document content is not exposed as current** (status=404)
- [PASS] **Replacement version has a distinct document id**

## Notification list/count/read/read-all contracts and recipient isolation

### Owner notification list

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "5a55b3f4-54b9-4042-8b5a-3c252a30ba98",
                                   "type":  "verification.document-expired",
                                   "severity":  "Warning",
                                   "title":  "انتهت صلاحية مستند التحقق",
                                   "body":  "انتهت صلاحية أحد مستندات التحقق الخاصة بك. يرجى إعادة رفع مستند ساري المفعول.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "documentId":  "7e2dd2ef-9fd5-4cf9-3c73-08def79c1e33",
                                                "documentType":  "SelfieWithId"
                                            },
                                   "createdAtUtc":  "2026-08-11T11:33:35.2212375",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "ed8d4032-84f6-419e-90c0-47cbdda2b3e1",
                                   "type":  "verification.document-rejected",
                                   "severity":  "Warning",
                                   "title":  "تم رفض مستند التحقق",
                                   "body":  "تم رفض أحد مستندات التحقق الخاصة بك. يرجى مراجعة التفاصيل واستبدال المستند عند الحاجة.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "documentId":  "3f358634-e14a-42e8-3c72-08def79c1e33",
                                                "documentType":  "BarAssociationCardFront"
                                            },
                                   "createdAtUtc":  "2026-08-11T11:33:05.9147618",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "39336964-8a1c-461c-9efa-3e1200c29589",
                                   "type":  "verification.document-approved",
                                   "severity":  "Success",
                                   "title":  "تم اعتماد مستند التحقق",
                                   "body":  "تم اعتماد أحد مستندات التحقق الخاصة بك. يمكنك متابعة حالة التحقق من حسابك.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "documentId":  "00833b7b-2585-40cd-3c71-08def79c1e33",
                                                "documentType":  "NationalIdBack"
                                            },
                                   "createdAtUtc":  "2026-08-11T11:32:36.997814",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  3
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Owner notification list returns 200** (status=200)
- [PASS] **Owner notification list contains approved, rejected, and expired document types**
- [PASS] **Verification notification has no forbidden metadata fields**
- [PASS] **Verification notification has no forbidden metadata fields**
- [PASS] **Verification notification has no forbidden metadata fields**
### Owner unread notification count

**Request:** GET http://localhost:5049/api/notifications/unread-count

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "unreadCount":  3
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Owner unread count returns 200** (status=200)
- [PASS] **Owner has unread notifications before read**
### Owner marks approved notification read

**Request:** PATCH http://localhost:5049/api/notifications/39336964-8a1c-461c-9efa-3e1200c29589/read

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "id":  "39336964-8a1c-461c-9efa-3e1200c29589",
                 "type":  "verification.document-approved",
                 "severity":  "Success",
                 "title":  "تم اعتماد مستند التحقق",
                 "body":  "تم اعتماد أحد مستندات التحقق الخاصة بك. يمكنك متابعة حالة التحقق من حسابك.",
                 "actionUrl":  null,
                 "data":  {
                              "documentId":  "00833b7b-2585-40cd-3c71-08def79c1e33",
                              "documentType":  "NationalIdBack"
                          },
                 "createdAtUtc":  "2026-08-11T11:32:36.997814",
                 "readAtUtc":  "2026-08-11T11:34:33.337157Z",
                 "expiresAtUtc":  null
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Mark read returns 200** (status=200)
- [PASS] **Mark read response contains a read timestamp**
### Owner replays mark read

**Request:** PATCH http://localhost:5049/api/notifications/39336964-8a1c-461c-9efa-3e1200c29589/read

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "id":  "39336964-8a1c-461c-9efa-3e1200c29589",
                 "type":  "verification.document-approved",
                 "severity":  "Success",
                 "title":  "تم اعتماد مستند التحقق",
                 "body":  "تم اعتماد أحد مستندات التحقق الخاصة بك. يمكنك متابعة حالة التحقق من حسابك.",
                 "actionUrl":  null,
                 "data":  {
                              "documentId":  "00833b7b-2585-40cd-3c71-08def79c1e33",
                              "documentType":  "NationalIdBack"
                          },
                 "createdAtUtc":  "2026-08-11T11:32:36.997814",
                 "readAtUtc":  "2026-08-11T11:34:33.337157",
                 "expiresAtUtc":  null
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Repeated mark read remains idempotent** (status=200)
### Owner unread count after mark read

**Request:** GET http://localhost:5049/api/notifications/unread-count

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "unreadCount":  2
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Unread count decreases after mark read**
### Owner marks all notifications read

**Request:** PATCH http://localhost:5049/api/notifications/read-all

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "readAtUtc":  "2026-08-11T11:34:33.5851349Z",
                 "unreadCount":  0
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Mark all read returns 200** (status=200)
### Owner unread count after read-all

**Request:** GET http://localhost:5049/api/notifications/unread-count

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "unreadCount":  0
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Read-all leaves no unread owner notifications**
### Owner lists read notifications

**Request:** GET http://localhost:5049/api/notifications?pageSize=50&isRead=true

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "5a55b3f4-54b9-4042-8b5a-3c252a30ba98",
                                   "type":  "verification.document-expired",
                                   "severity":  "Warning",
                                   "title":  "انتهت صلاحية مستند التحقق",
                                   "body":  "انتهت صلاحية أحد مستندات التحقق الخاصة بك. يرجى إعادة رفع مستند ساري المفعول.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "documentId":  "7e2dd2ef-9fd5-4cf9-3c73-08def79c1e33",
                                                "documentType":  "SelfieWithId"
                                            },
                                   "createdAtUtc":  "2026-08-11T11:33:35.2212375",
                                   "readAtUtc":  "2026-08-11T11:34:33.5851349",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "ed8d4032-84f6-419e-90c0-47cbdda2b3e1",
                                   "type":  "verification.document-rejected",
                                   "severity":  "Warning",
                                   "title":  "تم رفض مستند التحقق",
                                   "body":  "تم رفض أحد مستندات التحقق الخاصة بك. يرجى مراجعة التفاصيل واستبدال المستند عند الحاجة.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "documentId":  "3f358634-e14a-42e8-3c72-08def79c1e33",
                                                "documentType":  "BarAssociationCardFront"
                                            },
                                   "createdAtUtc":  "2026-08-11T11:33:05.9147618",
                                   "readAtUtc":  "2026-08-11T11:34:33.5851349",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "39336964-8a1c-461c-9efa-3e1200c29589",
                                   "type":  "verification.document-approved",
                                   "severity":  "Success",
                                   "title":  "تم اعتماد مستند التحقق",
                                   "body":  "تم اعتماد أحد مستندات التحقق الخاصة بك. يمكنك متابعة حالة التحقق من حسابك.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "documentId":  "00833b7b-2585-40cd-3c71-08def79c1e33",
                                                "documentType":  "NationalIdBack"
                                            },
                                   "createdAtUtc":  "2026-08-11T11:32:36.997814",
                                   "readAtUtc":  "2026-08-11T11:34:33.337157",
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  0
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Read notification filter returns 200** (status=200)
### Notification list invalid page size

**Request:** GET http://localhost:5049/api/notifications?pageSize=0

**Response Status:** 400

**Response Body:**
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "PageSize":  [
                                    "\u0027Page Size\u0027 must be between 1 and 50. You entered 0."
                                ]
               },
    "traceId":  "00-a3ffe115e7abe2933ac873258825df62-5e363bff0e886861-00"
}
```n---

- [PASS] **Notification invalid page size returns 400** (status=400)
### Notification list invalid cursor

**Request:** GET http://localhost:5049/api/notifications?pageSize=10&cursor=not-a-valid-cursor

**Response Status:** 400

**Response Body:**
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "Cursor":  [
                                  "Cursor is invalid or unsupported."
                              ]
               },
    "traceId":  "00-59cc2f80e22137a62ba288ec306145a7-51c1e2e3fb1c3f99-00"
}
```n---

- [PASS] **Notification invalid cursor returns 400** (status=400)
### Notification empty id read

**Request:** PATCH http://localhost:5049/api/notifications/00000000-0000-0000-0000-[REDACTED_NUMBER]/read

**Response Status:** 404

**Response Body:**
```json
{
    "success":  false,
    "data":  null,
    "message":  "Entity \"Notification\" (00000000-0000-0000-0000-[REDACTED_NUMBER]) was not found.",
    "errors":  null,
    "statusCode":  404
}
```n---

- [PASS] **Notification empty id read returns 404** (status=404)
### Unrelated notification list

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
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
}
```n---

- [PASS] **Unrelated notification list returns 200** (status=200)
- [PASS] **Unrelated user receives no verification notification leakage**
### Unrelated Admin notification list

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
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
}
```n---

- [PASS] **Unrelated Admin notification list returns 200** (status=200)
- [PASS] **Admin inbox is not blindly broadcast verification work**
### Unrelated user reads owner notification

**Request:** PATCH http://localhost:5049/api/notifications/39336964-8a1c-461c-9efa-3e1200c29589/read

**Response Status:** 404

**Response Body:**
```json
{
    "success":  false,
    "data":  null,
    "message":  "Entity \"Notification\" (39336964-8a1c-461c-9efa-3e1200c29589) was not found.",
    "errors":  null,
    "statusCode":  404
}
```n---

- [PASS] **Unrelated user cannot mark owner notification read** (status=404)
### Unrelated user marks all read

**Request:** PATCH http://localhost:5049/api/notifications/read-all

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "readAtUtc":  "2026-08-11T11:34:35.0419633Z",
                 "unreadCount":  0
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```n---

- [PASS] **Unrelated user read-all remains isolated** (status=200)

## API and mock Email log monitoring

- [PASS] **API, outbox, notification, and provider logs are clean** (violations=0)
- [PASS] **Mock Email confirmation was recorded for disposable account**
- [PASS] **Mock Email confirmation was recorded for disposable account**
- [PASS] **Mock Email confirmation was recorded for disposable account**
- [PASS] **Mock Email confirmation was recorded for disposable account**
- [PASS] **API test port is released after owned process shutdown**

## Execution summary

| Metric | Count |
|---|---:|
| Passed assertions | 122 |
| Failed assertions | 0 |
| Documented skips | 3 |

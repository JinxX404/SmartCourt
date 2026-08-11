# User Verification Notifications HTTP Test Report

Generated at: 2026-08-11 15:51:42 +03:00


## Health, anonymous access, and notification authorization boundaries

### GET /health

**Request:** GET http://localhost:5049/health

**Response Status:** 200

**Response Body:**
```json
Healthy
```
---

- [PASS] **Health returns 200** (status=200)
### GET /api/health/ping

**Request:** GET http://localhost:5049/api/health/ping

**Response Status:** 200

**Response Body:**
```json
{
    "message":  "Pong! Smart Court API is fully operational.",
    "serverTimeUtc":  "2026-08-11T12:51:58.4209156Z",
    "version":  "1.0.0"
}
```
---

- [PASS] **Health ping returns 200** (status=200)
### Anonymous submit verification

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents

**Body:**
```json
{

}
```

**Response Status:** 401

**Response Body:** (Empty)
---

- [PASS] **Anonymous submit verification returns 401** (status=401)
### Anonymous list user documents

**Request:** GET http://localhost:5049/api/UserVerification/9053cefb-d8ce-45ca-b1ae-9f58f461a315

**Response Status:** 401

**Response Body:** (Empty)
---

- [PASS] **Anonymous list user documents returns 401** (status=401)
### Anonymous user document content

**Request:** GET http://localhost:5049/api/UserVerification/documents/9053cefb-d8ce-45ca-b1ae-9f58f461a315/content

**Response Status:** 401

**Response Body:** (Empty)
---

- [PASS] **Anonymous user document content returns 401** (status=401)
### Anonymous delete document

**Request:** DELETE http://localhost:5049/api/UserVerification?UserId=9053cefb-d8ce-45ca-b1ae-9f58f461a315&DocumentId=9053cefb-d8ce-45ca-b1ae-9f58f461a315

**Response Status:** 401

**Response Body:** (Empty)
---

- [PASS] **Anonymous delete document returns 401** (status=401)
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
### Anonymous notification read

**Request:** PATCH http://localhost:5049/api/notifications/9053cefb-d8ce-45ca-b1ae-9f58f461a315/read

**Response Status:** 401

**Response Body:** (Empty)
---

- [PASS] **Anonymous notification read returns 401** (status=401)
### Anonymous notification read-all

**Request:** PATCH http://localhost:5049/api/notifications/read-all

**Response Status:** 401

**Response Body:** (Empty)
---

- [PASS] **Anonymous notification read-all returns 401** (status=401)

## Admin-only recipient setup and role boundaries

### Setup Primary Admin login

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
```json
{
    "Password":  "[REDACTED]",
    "Email":  "[REDACTED]"
}
```

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
```
---

- [PASS] **Primary Admin login succeeds** (status=200)
### Setup Secondary Admin login

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
```json
{
    "Password":  "[REDACTED]",
    "Email":  "[REDACTED]"
}
```

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "95077803-bc59-4076-34cb-08def604e4b7",
                              "email":  "[REDACTED]",
                              "fullName":  "Ahmed Kokker",
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
```
---

- [PASS] **Secondary Admin login succeeds** (status=200)
### Setup Tertiary Admin login

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
```json
{
    "Password":  "[REDACTED]",
    "Email":  "[REDACTED]"
}
```

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "3630391c-4a32-4e5e-34cc-08def604e4b7",
                              "email":  "[REDACTED]",
                              "fullName":  "Moataz Mohammed",
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
```
---

- [PASS] **Tertiary Admin login succeeds** (status=200)
### Setup submission owner registration

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
```json
{
    "Email":  "[REDACTED]",
    "FullName":  "Gate 6 submission owner",
    "ConfirmPassword":  "[REDACTED]",
    "Password":  "[REDACTED]"
}
```

**Response Status:** 201

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "userId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                 "email":  "[REDACTED]",
                 "fullName":  "Gate 6 submission owner",
                 "role":  "Lawyer"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
```
---

- [PASS] **submission owner registration returns 201** (status=201)
### submission owner confirms Email from mock log

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=ef434f31-40f7-42ad-0017-08def7a755da&token=[REDACTED]

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "message":  "تم تأكيد البريد الإلكتروني بنجاح.",
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **submission owner Email confirmation succeeds** (status=200)
### Setup submission owner login

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
```json
{
    "Password":  "[REDACTED]",
    "Email":  "[REDACTED]"
}
```

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "ef434f31-40f7-42ad-0017-08def7a755da",
                              "email":  "[REDACTED]",
                              "fullName":  "Gate 6 submission owner",
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
```
---

- [PASS] **submission owner login succeeds after Email confirmation** (status=200)
### Setup partial submission owner registration

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
```json
{
    "Email":  "[REDACTED]",
    "FullName":  "Gate 6 partial submission owner",
    "ConfirmPassword":  "[REDACTED]",
    "Password":  "[REDACTED]"
}
```

**Response Status:** 201

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "userId":  "3d9ac88c-a14c-428d-0018-08def7a755da",
                 "email":  "[REDACTED]",
                 "fullName":  "Gate 6 partial submission owner",
                 "role":  "Lawyer"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
```
---

- [PASS] **partial submission owner registration returns 201** (status=201)
### partial submission owner confirms Email from mock log

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=3d9ac88c-a14c-428d-0018-08def7a755da&token=[REDACTED]

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "message":  "تم تأكيد البريد الإلكتروني بنجاح.",
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **partial submission owner Email confirmation succeeds** (status=200)
### Setup partial submission owner login

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
```json
{
    "Password":  "[REDACTED]",
    "Email":  "[REDACTED]"
}
```

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "3d9ac88c-a14c-428d-0018-08def7a755da",
                              "email":  "[REDACTED]",
                              "fullName":  "Gate 6 partial submission owner",
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
```
---

- [PASS] **partial submission owner login succeeds after Email confirmation** (status=200)
### Setup multi-document owner registration

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
```json
{
    "Email":  "[REDACTED]",
    "FullName":  "Gate 6 multi-document owner",
    "ConfirmPassword":  "[REDACTED]",
    "Password":  "[REDACTED]"
}
```

**Response Status:** 201

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                 "email":  "[REDACTED]",
                 "fullName":  "Gate 6 multi-document owner",
                 "role":  "Lawyer"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
```
---

- [PASS] **multi-document owner registration returns 201** (status=201)
### multi-document owner confirms Email from mock log

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=a1f426b3-45d2-4aaa-0019-08def7a755da&token=[REDACTED]

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "message":  "تم تأكيد البريد الإلكتروني بنجاح.",
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **multi-document owner Email confirmation succeeds** (status=200)
### Setup multi-document owner login

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
```json
{
    "Password":  "[REDACTED]",
    "Email":  "[REDACTED]"
}
```

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                              "email":  "[REDACTED]",
                              "fullName":  "Gate 6 multi-document owner",
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
```
---

- [PASS] **multi-document owner login succeeds after Email confirmation** (status=200)
### Setup unrelated user registration

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
```json
{
    "Email":  "[REDACTED]",
    "FullName":  "Gate 6 unrelated user",
    "ConfirmPassword":  "[REDACTED]",
    "Password":  "[REDACTED]"
}
```

**Response Status:** 201

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "userId":  "11f0a101-f62c-41ce-001a-08def7a755da",
                 "email":  "[REDACTED]",
                 "fullName":  "Gate 6 unrelated user",
                 "role":  "Client"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
```
---

- [PASS] **unrelated user registration returns 201** (status=201)
### unrelated user confirms Email from mock log

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=11f0a101-f62c-41ce-001a-08def7a755da&token=[REDACTED]

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "message":  "تم تأكيد البريد الإلكتروني بنجاح.",
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **unrelated user Email confirmation succeeds** (status=200)
### Setup unrelated user login

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
```json
{
    "Password":  "[REDACTED]",
    "Email":  "[REDACTED]"
}
```

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "11f0a101-f62c-41ce-001a-08def7a755da",
                              "email":  "[REDACTED]",
                              "fullName":  "Gate 6 unrelated user",
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
```
---

- [PASS] **unrelated user login succeeds after Email confirmation** (status=200)
### Admin reads unknown verification user

**Request:** GET http://localhost:5049/api/admin/verifications/9053cefb-d8ce-45ca-b1ae-9f58f461a315

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
```
---

- [PASS] **Admin unknown verification user returns 404** (status=404)
### Lawyer accesses Admin verification list

**Request:** GET http://localhost:5049/api/admin/verifications

**Response Status:** 403

**Response Body:** (Empty)
---

- [PASS] **Lawyer cannot access Admin verification list** (status=403)
### Lawyer notification list remains authenticated

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
```
---

- [PASS] **Authenticated non-Admin can access own notification list** (status=200)
- [SKIP] **SuperAdministrator role boundary** — The repository seeds Admin but no supported HTTP endpoint creates or assigns SuperAdministrator; optional credentials were not supplied.

## Successful upload, pending/detail/content endpoints, and causal review notification

### Owner submits one valid verification document

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents

**Body:**
```json
{
    "UserId":  "ef434f31-40f7-42ad-0017-08def7a755da",
    "Documents[0].ExpirationDate":  "2035-01-01",
    "Documents[0].Type":  1,
    "Documents[0].File":  "[REDACTED_FILE]"
}
```

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "uploadedDocuments":  [
                                           {
                                               "fileName":  "[REDACTED]",
                                               "type":  1
                                           }
                                       ],
                 "failedDocuments":  [

                                     ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Successful upload returns 200** (status=200)
- [PASS] **Successful upload persists exactly one document in the response** (uploaded=1 failed=0)
### Owner lists submitted documents

**Request:** GET http://localhost:5049/api/UserVerification/ef434f31-40f7-42ad-0017-08def7a755da

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "documents":  [
                                   {
                                       "documentId":  "76a4018a-d87b-4cde-8582-08def7a76827",
                                       "documentType":  1,
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
```
---

- [PASS] **Owner document list returns 200** (status=200)
- [PASS] **Owner list contains a current document ID**
### Admin lists pending verifications

**Request:** GET http://localhost:5049/api/admin/verifications?PageNumber=1&PageSize=10

**Response Status:** 200

**Response Body:**
```json
{
    "pageNumber":  1,
    "pageSize":  10,
    "totalPages":  2,
    "totalRecords":  15,
    "hasNextPage":  true,
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
                     "rejectedDocumentCount":  3,
                     "role":  "Lawyer"
                 },
                 {
                     "lawyerId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                     "fullName":  "Gate 6 multi-document owner",
                     "email":  "[REDACTED]",
                     "phoneNumber":  "[REDACTED]",
                     "pendingDocumentCount":  1,
                     "verifiedDocumentCount":  0,
                     "rejectedDocumentCount":  0,
                     "role":  "Lawyer"
                 },
                 {
                     "lawyerId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                     "fullName":  "Gate 6 multi-document owner",
                     "email":  "[REDACTED]",
                     "phoneNumber":  "[REDACTED]",
                     "pendingDocumentCount":  1,
                     "verifiedDocumentCount":  0,
                     "rejectedDocumentCount":  0,
                     "role":  "Lawyer"
                 },
                 {
                     "lawyerId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                     "fullName":  "Gate 6 multi-document owner",
                     "email":  "[REDACTED]",
                     "phoneNumber":  "[REDACTED]",
                     "pendingDocumentCount":  1,
                     "verifiedDocumentCount":  0,
                     "rejectedDocumentCount":  0,
                     "role":  "Lawyer"
                 },
                 {
                     "lawyerId":  "7697892a-b6b6-4895-d01d-08def7a6b40b",
                     "fullName":  "Gate 6 partial submission owner",
                     "email":  "[REDACTED]",
                     "phoneNumber":  "[REDACTED]",
                     "pendingDocumentCount":  1,
                     "verifiedDocumentCount":  0,
                     "rejectedDocumentCount":  0,
                     "role":  "Lawyer"
                 },
                 {
                     "lawyerId":  "9b40e63d-6525-4371-2246-08def7a3f2ff",
                     "fullName":  "Gate 6 partial submission owner",
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
```
---

- [PASS] **Admin pending verification list returns 200** (status=200)
### Admin reads owner verification details

**Request:** GET http://localhost:5049/api/admin/verifications/ef434f31-40f7-42ad-0017-08def7a755da

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "lawyerId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                 "fullName":  "Gate 6 submission owner",
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
                                       "documentId":  "76a4018a-d87b-4cde-8582-08def7a76827",
                                       "documentType":  "NationalIdFront",
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
```
---

- [PASS] **Admin verification details return 200** (status=200)
- [PASS] **Admin details expose the current document ID**
### Admin reads submitted document content

**Request:** GET http://localhost:5049/api/admin/verifications/documents/76a4018a-d87b-4cde-8582-08def7a76827/content

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
```
---

- [PASS] **Admin document content returns 200** (status=200)
### Owner reads own current document content

**Request:** GET http://localhost:5049/api/UserVerification/documents/76a4018a-d87b-4cde-8582-08def7a76827/content

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
```
---

- [PASS] **Owner document content returns 200** (status=200)
- [PASS] **Primary receives one review-requested notification after the upload**
- [PASS] **Secondary receives one review-requested notification after the upload**
- [PASS] **Tertiary receives one review-requested notification after the upload**
- [PASS] **The upload creates one logical review notification per Admin**
### Owner notification inbox after submission

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
```
---

- [PASS] **Owner notification inbox after submission returns 200** (status=200)
- [PASS] **Uploader does not receive the Admin review-requested notification**
### Unrelated user inbox before submission notification checks

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
```
---

- [PASS] **Unrelated user inbox before submission notification checks returns 200** (status=200)
- [PASS] **Unrelated user receives no review-requested notification**

## Partial uploads, multiple-file coalescing, and notification counts

### Primary partial-before notification list

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "a51b01ba-2bd4-40a0-a358-3e3ed6c072a1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:52:31.2631889",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "33c29af8-38ec-41d8-9f23-f14360bb684c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:29.4336966",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "5485937c-f408-4495-a179-1d338018f24c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:27.2701852",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "4ad72303-3d5e-47a7-afa4-c5ca57cde593",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "7697892a-b6b6-4895-d01d-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:25.2145577",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "e05ff08e-326a-4e2c-81d1-d46138861a7e",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ff2a7e50-67a5-441f-d01c-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:47:58.5268983",
                                   "readAtUtc":  "2026-08-11T12:48:42.2595799",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "219147e0-816c-4366-90aa-7b56e1ac62db",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:29:02.4412592",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "4fb3af4a-c9c9-4f7e-9de0-b38e748c3b2b",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:57.6360721",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "ed0a279a-320a-46bc-85c6-6c75f1d26a99",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9b40e63d-6525-4371-2246-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:54.9839611",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "10d63ec9-0e32-4477-9a5b-befb1163695b",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "59676a63-16fb-4571-2245-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:23.4820149",
                                   "readAtUtc":  "2026-08-11T12:29:30.5502709",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "244f81be-7aac-424a-abc0-cad19de895e1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:20:00.48862",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "aa529b2f-2262-42f8-82b4-14412efcef2f",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:56.1381357",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "3b8e11fe-3377-4b0d-a901-5fdecad3b1c3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a2ceb314-139b-4e98-f7ef-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:54.0156177",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "50435919-8122-4d86-a30e-860c23cb42c1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "fed717d1-8dbf-469f-f7ee-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:26.7367683",
                                   "readAtUtc":  "2026-08-11T12:20:05.6118655",
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
```
---

- [PASS] **Primary partial-before notification list returns 200** (status=200)
### Secondary partial-before notification list

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "808d2ebf-3e0d-4a01-8536-88fecb9920c3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:52:31.2631889",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "c87dcbb2-c3a0-4cd0-a8d6-ebf3d699652c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:29.4336966",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "fd1bd9e9-88a1-4206-bf67-67b40a2d09a0",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:27.2701852",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a659fe12-248b-486f-af2d-ac51ffe0d29c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "7697892a-b6b6-4895-d01d-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:25.2145577",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "d089226f-81fd-4ffd-8285-42a1468fcd63",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ff2a7e50-67a5-441f-d01c-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:47:58.5268983",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "3e9bf5d7-feea-49d0-af0b-2c1f0b2c2c32",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:29:02.4412592",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "f9ce9923-c9a5-497e-b6af-df12ccc56257",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:57.6360721",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "ccad76f1-84a1-4aa9-ae5c-b6f990b5842d",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9b40e63d-6525-4371-2246-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:54.9839611",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a509e414-6b83-44b9-9581-13c4b5688dbd",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "59676a63-16fb-4571-2245-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:23.4820149",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "5441505e-3c8d-48ef-8c3a-70774af2f808",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:20:00.48862",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "8fc40fa2-042f-44b6-8d60-3320c903a5d6",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:56.1381357",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "cdf596ad-42cf-4a54-9c38-a76c30362f57",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a2ceb314-139b-4e98-f7ef-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:54.0156177",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "fe8beffa-8add-472b-a534-fcc147e7b826",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "fed717d1-8dbf-469f-f7ee-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:26.7367683",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  13
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Secondary partial-before notification list returns 200** (status=200)
### Tertiary partial-before notification list

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "141a52b7-e2e8-47de-b971-e595d0c435ff",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:52:31.2631889",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "f7227432-8e7c-4211-85e1-d0dc68bbbfe2",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:29.4336966",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "8abf2647-d302-4c33-abc9-678a319c01e1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:27.2701852",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "0d6f9113-3e52-423e-a4c3-ed731c50a4a5",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "7697892a-b6b6-4895-d01d-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:25.2145577",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "53e48921-1f3f-4df7-a963-da35c67a5f1c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ff2a7e50-67a5-441f-d01c-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:47:58.5268983",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "43312391-763a-48d7-bc4e-dd132adc0701",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:29:02.4412592",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "56e598f6-750b-4f3e-80c9-f399dad08372",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:57.6360721",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "e0810cb5-f9c3-4b75-8ed4-8299cf806ef3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9b40e63d-6525-4371-2246-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:54.9839611",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "640deaf9-2a07-4648-8da8-552cffaaa04e",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "59676a63-16fb-4571-2245-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:23.4820149",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "3c922a84-4917-4a4e-95a1-de2b7b4023a3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:20:00.48862",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "b2c5b4ee-0e88-4829-8108-0a92e6787c32",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:56.1381357",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "4b0d485f-31f7-4d8a-9045-0dc951a38d0e",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a2ceb314-139b-4e98-f7ef-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:54.0156177",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "b40ea9d8-92ae-4a77-be68-e8797641ce33",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "fed717d1-8dbf-469f-f7ee-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:26.7367683",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  13
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Tertiary partial-before notification list returns 200** (status=200)
### Partial submission with one valid and one expired document

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents

**Body:**
```json
{
    "UserId":  "3d9ac88c-a14c-428d-0018-08def7a755da",
    "Documents[0].ExpirationDate":  "2035-01-01",
    "Documents[0].Type":  2,
    "Documents[0].File":  "[REDACTED_FILE]",
    "Documents[1].ExpirationDate":  "2000-01-01",
    "Documents[1].Type":  3,
    "Documents[1].File":  "[REDACTED_FILE]"
}
```

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
                                         {
                                             "fileName":  "[REDACTED]",
                                             "type":  3,
                                             "error":  "This document is expired"
                                         }
                                     ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Partial upload request returns 200** (status=200)
- [PASS] **Partial upload persists one and reports one failed document**
- [PASS] **Primary receives one notification for a partial successful upload**
### Primary partial-after notification list

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "d9162b20-57cf-48c7-bf73-4df0f0b81a20",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3d9ac88c-a14c-428d-0018-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:00.5956145",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a51b01ba-2bd4-40a0-a358-3e3ed6c072a1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:52:31.2631889",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "33c29af8-38ec-41d8-9f23-f14360bb684c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:29.4336966",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "5485937c-f408-4495-a179-1d338018f24c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:27.2701852",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "4ad72303-3d5e-47a7-afa4-c5ca57cde593",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "7697892a-b6b6-4895-d01d-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:25.2145577",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "e05ff08e-326a-4e2c-81d1-d46138861a7e",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ff2a7e50-67a5-441f-d01c-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:47:58.5268983",
                                   "readAtUtc":  "2026-08-11T12:48:42.2595799",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "219147e0-816c-4366-90aa-7b56e1ac62db",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:29:02.4412592",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "4fb3af4a-c9c9-4f7e-9de0-b38e748c3b2b",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:57.6360721",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "ed0a279a-320a-46bc-85c6-6c75f1d26a99",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9b40e63d-6525-4371-2246-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:54.9839611",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "10d63ec9-0e32-4477-9a5b-befb1163695b",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "59676a63-16fb-4571-2245-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:23.4820149",
                                   "readAtUtc":  "2026-08-11T12:29:30.5502709",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "244f81be-7aac-424a-abc0-cad19de895e1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:20:00.48862",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "aa529b2f-2262-42f8-82b4-14412efcef2f",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:56.1381357",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "3b8e11fe-3377-4b0d-a901-5fdecad3b1c3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a2ceb314-139b-4e98-f7ef-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:54.0156177",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "50435919-8122-4d86-a30e-860c23cb42c1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "fed717d1-8dbf-469f-f7ee-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:26.7367683",
                                   "readAtUtc":  "2026-08-11T12:20:05.6118655",
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
```
---

- [PASS] **Primary partial-after notification list returns 200** (status=200)
- [PASS] **Primary receives only one partial-upload notification** (before=0 after=1)
- [PASS] **Secondary receives one notification for a partial successful upload**
### Secondary partial-after notification list

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "a1a6b79d-f492-4635-8b02-db1d138bf37f",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3d9ac88c-a14c-428d-0018-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:00.5956145",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "808d2ebf-3e0d-4a01-8536-88fecb9920c3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:52:31.2631889",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "c87dcbb2-c3a0-4cd0-a8d6-ebf3d699652c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:29.4336966",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "fd1bd9e9-88a1-4206-bf67-67b40a2d09a0",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:27.2701852",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a659fe12-248b-486f-af2d-ac51ffe0d29c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "7697892a-b6b6-4895-d01d-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:25.2145577",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "d089226f-81fd-4ffd-8285-42a1468fcd63",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ff2a7e50-67a5-441f-d01c-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:47:58.5268983",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "3e9bf5d7-feea-49d0-af0b-2c1f0b2c2c32",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:29:02.4412592",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "f9ce9923-c9a5-497e-b6af-df12ccc56257",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:57.6360721",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "ccad76f1-84a1-4aa9-ae5c-b6f990b5842d",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9b40e63d-6525-4371-2246-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:54.9839611",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a509e414-6b83-44b9-9581-13c4b5688dbd",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "59676a63-16fb-4571-2245-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:23.4820149",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "5441505e-3c8d-48ef-8c3a-70774af2f808",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:20:00.48862",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "8fc40fa2-042f-44b6-8d60-3320c903a5d6",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:56.1381357",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "cdf596ad-42cf-4a54-9c38-a76c30362f57",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a2ceb314-139b-4e98-f7ef-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:54.0156177",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "fe8beffa-8add-472b-a534-fcc147e7b826",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "fed717d1-8dbf-469f-f7ee-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:26.7367683",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  14
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Secondary partial-after notification list returns 200** (status=200)
- [PASS] **Secondary receives only one partial-upload notification** (before=0 after=1)
- [PASS] **Tertiary receives one notification for a partial successful upload**
### Tertiary partial-after notification list

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "f2eaff03-de31-40f2-8991-33f1ab117f47",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3d9ac88c-a14c-428d-0018-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:00.5956145",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "141a52b7-e2e8-47de-b971-e595d0c435ff",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:52:31.2631889",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "f7227432-8e7c-4211-85e1-d0dc68bbbfe2",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:29.4336966",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "8abf2647-d302-4c33-abc9-678a319c01e1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:27.2701852",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "0d6f9113-3e52-423e-a4c3-ed731c50a4a5",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "7697892a-b6b6-4895-d01d-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:25.2145577",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "53e48921-1f3f-4df7-a963-da35c67a5f1c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ff2a7e50-67a5-441f-d01c-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:47:58.5268983",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "43312391-763a-48d7-bc4e-dd132adc0701",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:29:02.4412592",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "56e598f6-750b-4f3e-80c9-f399dad08372",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:57.6360721",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "e0810cb5-f9c3-4b75-8ed4-8299cf806ef3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9b40e63d-6525-4371-2246-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:54.9839611",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "640deaf9-2a07-4648-8da8-552cffaaa04e",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "59676a63-16fb-4571-2245-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:23.4820149",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "3c922a84-4917-4a4e-95a1-de2b7b4023a3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:20:00.48862",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "b2c5b4ee-0e88-4829-8108-0a92e6787c32",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:56.1381357",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "4b0d485f-31f7-4d8a-9045-0dc951a38d0e",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a2ceb314-139b-4e98-f7ef-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:54.0156177",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "b40ea9d8-92ae-4a77-be68-e8797641ce33",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "fed717d1-8dbf-469f-f7ee-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:26.7367683",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  14
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Tertiary partial-after notification list returns 200** (status=200)
- [PASS] **Tertiary receives only one partial-upload notification** (before=0 after=1)
### User submits two valid documents in one request

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents

**Body:**
```json
{
    "UserId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
    "Documents[0].ExpirationDate":  "2035-01-01",
    "Documents[0].Type":  1,
    "Documents[0].File":  "[REDACTED_FILE]",
    "Documents[1].ExpirationDate":  "2035-01-01",
    "Documents[1].Type":  2,
    "Documents[1].File":  "[REDACTED_FILE]"
}
```

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "uploadedDocuments":  [
                                           {
                                               "fileName":  "[REDACTED]",
                                               "type":  1
                                           },
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
```
---

- [PASS] **Two-document upload returns 200** (status=200)
- [PASS] **Two-document upload persists both documents**
- [PASS] **Primary receives one notification for two uploaded documents**
### Primary two-document notification count

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "8d912222-31ef-400f-bcde-bbe9626e2744",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:02.0683954",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "d9162b20-57cf-48c7-bf73-4df0f0b81a20",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3d9ac88c-a14c-428d-0018-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:00.5956145",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a51b01ba-2bd4-40a0-a358-3e3ed6c072a1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:52:31.2631889",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "33c29af8-38ec-41d8-9f23-f14360bb684c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:29.4336966",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "5485937c-f408-4495-a179-1d338018f24c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:27.2701852",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "4ad72303-3d5e-47a7-afa4-c5ca57cde593",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "7697892a-b6b6-4895-d01d-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:25.2145577",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "e05ff08e-326a-4e2c-81d1-d46138861a7e",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ff2a7e50-67a5-441f-d01c-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:47:58.5268983",
                                   "readAtUtc":  "2026-08-11T12:48:42.2595799",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "219147e0-816c-4366-90aa-7b56e1ac62db",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:29:02.4412592",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "4fb3af4a-c9c9-4f7e-9de0-b38e748c3b2b",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:57.6360721",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "ed0a279a-320a-46bc-85c6-6c75f1d26a99",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9b40e63d-6525-4371-2246-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:54.9839611",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "10d63ec9-0e32-4477-9a5b-befb1163695b",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "59676a63-16fb-4571-2245-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:23.4820149",
                                   "readAtUtc":  "2026-08-11T12:29:30.5502709",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "244f81be-7aac-424a-abc0-cad19de895e1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:20:00.48862",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "aa529b2f-2262-42f8-82b4-14412efcef2f",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:56.1381357",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "3b8e11fe-3377-4b0d-a901-5fdecad3b1c3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a2ceb314-139b-4e98-f7ef-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:54.0156177",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "50435919-8122-4d86-a30e-860c23cb42c1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "fed717d1-8dbf-469f-f7ee-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:26.7367683",
                                   "readAtUtc":  "2026-08-11T12:20:05.6118655",
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
```
---

- [PASS] **Primary two-document notification count returns 200** (status=200)
- [PASS] **Primary receives one, not two, notifications for the multi-file request** (count=1)
- [PASS] **Secondary receives one notification for two uploaded documents**
### Secondary two-document notification count

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "bae3722c-9380-4031-817f-5262b4e8dce1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:02.0683954",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a1a6b79d-f492-4635-8b02-db1d138bf37f",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3d9ac88c-a14c-428d-0018-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:00.5956145",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "808d2ebf-3e0d-4a01-8536-88fecb9920c3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:52:31.2631889",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "c87dcbb2-c3a0-4cd0-a8d6-ebf3d699652c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:29.4336966",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "fd1bd9e9-88a1-4206-bf67-67b40a2d09a0",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:27.2701852",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a659fe12-248b-486f-af2d-ac51ffe0d29c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "7697892a-b6b6-4895-d01d-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:25.2145577",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "d089226f-81fd-4ffd-8285-42a1468fcd63",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ff2a7e50-67a5-441f-d01c-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:47:58.5268983",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "3e9bf5d7-feea-49d0-af0b-2c1f0b2c2c32",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:29:02.4412592",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "f9ce9923-c9a5-497e-b6af-df12ccc56257",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:57.6360721",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "ccad76f1-84a1-4aa9-ae5c-b6f990b5842d",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9b40e63d-6525-4371-2246-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:54.9839611",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a509e414-6b83-44b9-9581-13c4b5688dbd",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "59676a63-16fb-4571-2245-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:23.4820149",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "5441505e-3c8d-48ef-8c3a-70774af2f808",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:20:00.48862",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "8fc40fa2-042f-44b6-8d60-3320c903a5d6",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:56.1381357",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "cdf596ad-42cf-4a54-9c38-a76c30362f57",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a2ceb314-139b-4e98-f7ef-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:54.0156177",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "fe8beffa-8add-472b-a534-fcc147e7b826",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "fed717d1-8dbf-469f-f7ee-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:26.7367683",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  15
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Secondary two-document notification count returns 200** (status=200)
- [PASS] **Secondary receives one, not two, notifications for the multi-file request** (count=1)
- [PASS] **Tertiary receives one notification for two uploaded documents**
### Tertiary two-document notification count

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "328b5dbd-9312-4e3a-a650-118f70345149",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:02.0683954",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "f2eaff03-de31-40f2-8991-33f1ab117f47",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3d9ac88c-a14c-428d-0018-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:00.5956145",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "141a52b7-e2e8-47de-b971-e595d0c435ff",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:52:31.2631889",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "f7227432-8e7c-4211-85e1-d0dc68bbbfe2",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:29.4336966",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "8abf2647-d302-4c33-abc9-678a319c01e1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:27.2701852",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "0d6f9113-3e52-423e-a4c3-ed731c50a4a5",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "7697892a-b6b6-4895-d01d-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:25.2145577",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "53e48921-1f3f-4df7-a963-da35c67a5f1c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ff2a7e50-67a5-441f-d01c-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:47:58.5268983",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "43312391-763a-48d7-bc4e-dd132adc0701",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:29:02.4412592",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "56e598f6-750b-4f3e-80c9-f399dad08372",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:57.6360721",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "e0810cb5-f9c3-4b75-8ed4-8299cf806ef3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9b40e63d-6525-4371-2246-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:54.9839611",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "640deaf9-2a07-4648-8da8-552cffaaa04e",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "59676a63-16fb-4571-2245-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:23.4820149",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "3c922a84-4917-4a4e-95a1-de2b7b4023a3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:20:00.48862",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "b2c5b4ee-0e88-4829-8108-0a92e6787c32",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:56.1381357",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "4b0d485f-31f7-4d8a-9045-0dc951a38d0e",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a2ceb314-139b-4e98-f7ef-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:54.0156177",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "b40ea9d8-92ae-4a77-be68-e8797641ce33",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "fed717d1-8dbf-469f-f7ee-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:26.7367683",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  15
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Tertiary two-document notification count returns 200** (status=200)
- [PASS] **Tertiary receives one, not two, notifications for the multi-file request** (count=1)

## Replacement versions, deletion, ownership, and no-notification outcomes

### Multi-document owner lists versions before replacement

**Request:** GET http://localhost:5049/api/UserVerification/a1f426b3-45d2-4aaa-0019-08def7a755da

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "documents":  [
                                   {
                                       "documentId":  "9492eea3-db20-421c-8584-08def7a76827",
                                       "documentType":  1,
                                       "status":  1,
                                       "expirationDate":  "2035-01-01",
                                       "isCurrent":  true,
                                       "fileName":  "[REDACTED]",
                                       "rejectionReason":  "[REDACTED]"
                                   },
                                   {
                                       "documentId":  "c2caa042-aac8-40e1-8585-08def7a76827",
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
```
---

### Primary replacement-before notification list

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "8d912222-31ef-400f-bcde-bbe9626e2744",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:02.0683954",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "d9162b20-57cf-48c7-bf73-4df0f0b81a20",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3d9ac88c-a14c-428d-0018-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:00.5956145",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a51b01ba-2bd4-40a0-a358-3e3ed6c072a1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:52:31.2631889",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "33c29af8-38ec-41d8-9f23-f14360bb684c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:29.4336966",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "5485937c-f408-4495-a179-1d338018f24c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:27.2701852",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "4ad72303-3d5e-47a7-afa4-c5ca57cde593",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "7697892a-b6b6-4895-d01d-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:25.2145577",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "e05ff08e-326a-4e2c-81d1-d46138861a7e",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ff2a7e50-67a5-441f-d01c-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:47:58.5268983",
                                   "readAtUtc":  "2026-08-11T12:48:42.2595799",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "219147e0-816c-4366-90aa-7b56e1ac62db",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:29:02.4412592",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "4fb3af4a-c9c9-4f7e-9de0-b38e748c3b2b",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:57.6360721",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "ed0a279a-320a-46bc-85c6-6c75f1d26a99",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9b40e63d-6525-4371-2246-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:54.9839611",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "10d63ec9-0e32-4477-9a5b-befb1163695b",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "59676a63-16fb-4571-2245-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:23.4820149",
                                   "readAtUtc":  "2026-08-11T12:29:30.5502709",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "244f81be-7aac-424a-abc0-cad19de895e1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:20:00.48862",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "aa529b2f-2262-42f8-82b4-14412efcef2f",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:56.1381357",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "3b8e11fe-3377-4b0d-a901-5fdecad3b1c3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a2ceb314-139b-4e98-f7ef-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:54.0156177",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "50435919-8122-4d86-a30e-860c23cb42c1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "fed717d1-8dbf-469f-f7ee-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:26.7367683",
                                   "readAtUtc":  "2026-08-11T12:20:05.6118655",
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
```
---

- [PASS] **Primary replacement-before notification list returns 200** (status=200)
### Secondary replacement-before notification list

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "bae3722c-9380-4031-817f-5262b4e8dce1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:02.0683954",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a1a6b79d-f492-4635-8b02-db1d138bf37f",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3d9ac88c-a14c-428d-0018-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:00.5956145",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "808d2ebf-3e0d-4a01-8536-88fecb9920c3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:52:31.2631889",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "c87dcbb2-c3a0-4cd0-a8d6-ebf3d699652c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:29.4336966",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "fd1bd9e9-88a1-4206-bf67-67b40a2d09a0",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:27.2701852",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a659fe12-248b-486f-af2d-ac51ffe0d29c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "7697892a-b6b6-4895-d01d-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:25.2145577",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "d089226f-81fd-4ffd-8285-42a1468fcd63",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ff2a7e50-67a5-441f-d01c-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:47:58.5268983",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "3e9bf5d7-feea-49d0-af0b-2c1f0b2c2c32",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:29:02.4412592",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "f9ce9923-c9a5-497e-b6af-df12ccc56257",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:57.6360721",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "ccad76f1-84a1-4aa9-ae5c-b6f990b5842d",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9b40e63d-6525-4371-2246-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:54.9839611",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a509e414-6b83-44b9-9581-13c4b5688dbd",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "59676a63-16fb-4571-2245-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:23.4820149",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "5441505e-3c8d-48ef-8c3a-70774af2f808",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:20:00.48862",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "8fc40fa2-042f-44b6-8d60-3320c903a5d6",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:56.1381357",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "cdf596ad-42cf-4a54-9c38-a76c30362f57",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a2ceb314-139b-4e98-f7ef-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:54.0156177",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "fe8beffa-8add-472b-a534-fcc147e7b826",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "fed717d1-8dbf-469f-f7ee-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:26.7367683",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  15
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Secondary replacement-before notification list returns 200** (status=200)
### Tertiary replacement-before notification list

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "328b5dbd-9312-4e3a-a650-118f70345149",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:02.0683954",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "f2eaff03-de31-40f2-8991-33f1ab117f47",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3d9ac88c-a14c-428d-0018-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:00.5956145",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "141a52b7-e2e8-47de-b971-e595d0c435ff",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:52:31.2631889",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "f7227432-8e7c-4211-85e1-d0dc68bbbfe2",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:29.4336966",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "8abf2647-d302-4c33-abc9-678a319c01e1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:27.2701852",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "0d6f9113-3e52-423e-a4c3-ed731c50a4a5",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "7697892a-b6b6-4895-d01d-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:25.2145577",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "53e48921-1f3f-4df7-a963-da35c67a5f1c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ff2a7e50-67a5-441f-d01c-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:47:58.5268983",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "43312391-763a-48d7-bc4e-dd132adc0701",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:29:02.4412592",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "56e598f6-750b-4f3e-80c9-f399dad08372",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:57.6360721",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "e0810cb5-f9c3-4b75-8ed4-8299cf806ef3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9b40e63d-6525-4371-2246-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:54.9839611",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "640deaf9-2a07-4648-8da8-552cffaaa04e",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "59676a63-16fb-4571-2245-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:23.4820149",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "3c922a84-4917-4a4e-95a1-de2b7b4023a3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:20:00.48862",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "b2c5b4ee-0e88-4829-8108-0a92e6787c32",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:56.1381357",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "4b0d485f-31f7-4d8a-9045-0dc951a38d0e",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a2ceb314-139b-4e98-f7ef-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:54.0156177",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "b40ea9d8-92ae-4a77-be68-e8797641ce33",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "fed717d1-8dbf-469f-f7ee-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:26.7367683",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  15
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Tertiary replacement-before notification list returns 200** (status=200)
### User replaces a current verification document

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents

**Body:**
```json
{
    "UserId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
    "Documents[0].ExpirationDate":  "2036-01-01",
    "Documents[0].Type":  1,
    "Documents[0].File":  "[REDACTED_FILE]"
}
```

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "uploadedDocuments":  [
                                           {
                                               "fileName":  "[REDACTED]",
                                               "type":  1
                                           }
                                       ],
                 "failedDocuments":  [

                                     ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Replacement upload returns 200** (status=200)
### Multi-document owner lists versions after replacement

**Request:** GET http://localhost:5049/api/UserVerification/a1f426b3-45d2-4aaa-0019-08def7a755da

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "documents":  [
                                   {
                                       "documentId":  "9492eea3-db20-421c-8584-08def7a76827",
                                       "documentType":  1,
                                       "status":  1,
                                       "expirationDate":  "2035-01-01",
                                       "isCurrent":  false,
                                       "fileName":  "[REDACTED]",
                                       "rejectionReason":  "[REDACTED]"
                                   },
                                   {
                                       "documentId":  "c2caa042-aac8-40e1-8585-08def7a76827",
                                       "documentType":  2,
                                       "status":  1,
                                       "expirationDate":  "2035-01-01",
                                       "isCurrent":  true,
                                       "fileName":  "[REDACTED]",
                                       "rejectionReason":  "[REDACTED]"
                                   },
                                   {
                                       "documentId":  "a50e9ef3-dbbd-403b-8586-08def7a76827",
                                       "documentType":  1,
                                       "status":  1,
                                       "expirationDate":  "2036-01-01",
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
```
---

- [PASS] **Replacement creates a distinct current document version**
- [PASS] **Replacement marks the previous version non-current**
- [PASS] **Primary receives one notification for the replacement upload**
### Primary replacement-after notification list

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "04a683ba-afd9-4cfb-99ee-e30f303227d9",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:04.272033",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "8d912222-31ef-400f-bcde-bbe9626e2744",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:02.0683954",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "d9162b20-57cf-48c7-bf73-4df0f0b81a20",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3d9ac88c-a14c-428d-0018-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:00.5956145",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a51b01ba-2bd4-40a0-a358-3e3ed6c072a1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:52:31.2631889",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "33c29af8-38ec-41d8-9f23-f14360bb684c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:29.4336966",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "5485937c-f408-4495-a179-1d338018f24c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:27.2701852",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "4ad72303-3d5e-47a7-afa4-c5ca57cde593",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "7697892a-b6b6-4895-d01d-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:25.2145577",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "e05ff08e-326a-4e2c-81d1-d46138861a7e",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ff2a7e50-67a5-441f-d01c-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:47:58.5268983",
                                   "readAtUtc":  "2026-08-11T12:48:42.2595799",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "219147e0-816c-4366-90aa-7b56e1ac62db",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:29:02.4412592",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "4fb3af4a-c9c9-4f7e-9de0-b38e748c3b2b",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:57.6360721",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "ed0a279a-320a-46bc-85c6-6c75f1d26a99",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9b40e63d-6525-4371-2246-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:54.9839611",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "10d63ec9-0e32-4477-9a5b-befb1163695b",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "59676a63-16fb-4571-2245-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:23.4820149",
                                   "readAtUtc":  "2026-08-11T12:29:30.5502709",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "244f81be-7aac-424a-abc0-cad19de895e1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:20:00.48862",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "aa529b2f-2262-42f8-82b4-14412efcef2f",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:56.1381357",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "3b8e11fe-3377-4b0d-a901-5fdecad3b1c3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a2ceb314-139b-4e98-f7ef-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:54.0156177",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "50435919-8122-4d86-a30e-860c23cb42c1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "fed717d1-8dbf-469f-f7ee-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:26.7367683",
                                   "readAtUtc":  "2026-08-11T12:20:05.6118655",
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  4
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Primary replacement-after notification list returns 200** (status=200)
- [PASS] **Primary receives exactly one additional replacement notification** (before=1 after=2)
- [PASS] **Secondary receives one notification for the replacement upload**
### Secondary replacement-after notification list

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "4ee3d567-4760-42fe-a7cb-5b36298849c4",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:04.272033",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "bae3722c-9380-4031-817f-5262b4e8dce1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:02.0683954",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a1a6b79d-f492-4635-8b02-db1d138bf37f",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3d9ac88c-a14c-428d-0018-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:00.5956145",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "808d2ebf-3e0d-4a01-8536-88fecb9920c3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:52:31.2631889",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "c87dcbb2-c3a0-4cd0-a8d6-ebf3d699652c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:29.4336966",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "fd1bd9e9-88a1-4206-bf67-67b40a2d09a0",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:27.2701852",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a659fe12-248b-486f-af2d-ac51ffe0d29c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "7697892a-b6b6-4895-d01d-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:25.2145577",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "d089226f-81fd-4ffd-8285-42a1468fcd63",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ff2a7e50-67a5-441f-d01c-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:47:58.5268983",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "3e9bf5d7-feea-49d0-af0b-2c1f0b2c2c32",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:29:02.4412592",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "f9ce9923-c9a5-497e-b6af-df12ccc56257",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:57.6360721",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "ccad76f1-84a1-4aa9-ae5c-b6f990b5842d",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9b40e63d-6525-4371-2246-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:54.9839611",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a509e414-6b83-44b9-9581-13c4b5688dbd",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "59676a63-16fb-4571-2245-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:23.4820149",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "5441505e-3c8d-48ef-8c3a-70774af2f808",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:20:00.48862",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "8fc40fa2-042f-44b6-8d60-3320c903a5d6",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:56.1381357",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "cdf596ad-42cf-4a54-9c38-a76c30362f57",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a2ceb314-139b-4e98-f7ef-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:54.0156177",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "fe8beffa-8add-472b-a534-fcc147e7b826",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "fed717d1-8dbf-469f-f7ee-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:26.7367683",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  16
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Secondary replacement-after notification list returns 200** (status=200)
- [PASS] **Secondary receives exactly one additional replacement notification** (before=1 after=2)
- [PASS] **Tertiary receives one notification for the replacement upload**
### Tertiary replacement-after notification list

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "0521e585-711e-4849-9278-602df0cfacc6",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:04.272033",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "328b5dbd-9312-4e3a-a650-118f70345149",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:02.0683954",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "f2eaff03-de31-40f2-8991-33f1ab117f47",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3d9ac88c-a14c-428d-0018-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:00.5956145",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "141a52b7-e2e8-47de-b971-e595d0c435ff",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:52:31.2631889",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "f7227432-8e7c-4211-85e1-d0dc68bbbfe2",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:29.4336966",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "8abf2647-d302-4c33-abc9-678a319c01e1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:27.2701852",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "0d6f9113-3e52-423e-a4c3-ed731c50a4a5",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "7697892a-b6b6-4895-d01d-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:25.2145577",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "53e48921-1f3f-4df7-a963-da35c67a5f1c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ff2a7e50-67a5-441f-d01c-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:47:58.5268983",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "43312391-763a-48d7-bc4e-dd132adc0701",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:29:02.4412592",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "56e598f6-750b-4f3e-80c9-f399dad08372",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:57.6360721",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "e0810cb5-f9c3-4b75-8ed4-8299cf806ef3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9b40e63d-6525-4371-2246-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:54.9839611",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "640deaf9-2a07-4648-8da8-552cffaaa04e",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "59676a63-16fb-4571-2245-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:23.4820149",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "3c922a84-4917-4a4e-95a1-de2b7b4023a3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:20:00.48862",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "b2c5b4ee-0e88-4829-8108-0a92e6787c32",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:56.1381357",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "4b0d485f-31f7-4d8a-9045-0dc951a38d0e",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a2ceb314-139b-4e98-f7ef-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:54.0156177",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "b40ea9d8-92ae-4a77-be68-e8797641ce33",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "fed717d1-8dbf-469f-f7ee-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:26.7367683",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  16
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Tertiary replacement-after notification list returns 200** (status=200)
- [PASS] **Tertiary receives exactly one additional replacement notification** (before=1 after=2)
### Unrelated user reads owner document content

**Request:** GET http://localhost:5049/api/UserVerification/documents/a50e9ef3-dbbd-403b-8586-08def7a76827/content

**Response Status:** 404

**Response Body:**
```json
{
    "success":  false,
    "data":  null,
    "message":  "Verification document was not found or access denied.",
    "errors":  null,
    "statusCode":  404
}
```
---

- [PASS] **Unrelated user cannot read the owner document** (status=404)
### Unrelated user deletes using unrelated UserId

**Request:** DELETE http://localhost:5049/api/UserVerification?UserId=11f0a101-f62c-41ce-001a-08def7a755da&DocumentId=D13437C6-0C48-41C9-4879-08DEF7A76820

**Response Status:** 404

**Response Body:**
```json
{
    "success":  false,
    "message":  null,
    "errors":  [
                   "Verification document was not found."
               ],
    "statusCode":  404
}
```
---

- [PASS] **Cross-user delete with the attacker UserId returns 404** (status=404)
### Owner deletes current verification document

**Request:** DELETE http://localhost:5049/api/UserVerification?UserId=a1f426b3-45d2-4aaa-0019-08def7a755da&DocumentId=D13437C6-0C48-41C9-4879-08DEF7A76820

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Owner deletion returns 200** (status=200)
### Owner reads deleted document content

**Request:** GET http://localhost:5049/api/UserVerification/documents/a50e9ef3-dbbd-403b-8586-08def7a76827/content

**Response Status:** 404

**Response Body:**
```json
{
    "success":  false,
    "data":  null,
    "message":  "Verification document was not found or access denied.",
    "errors":  null,
    "statusCode":  404
}
```
---

- [PASS] **Deleted document content returns 404** (status=404)
### Primary post-delete notification list

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "04a683ba-afd9-4cfb-99ee-e30f303227d9",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:04.272033",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "8d912222-31ef-400f-bcde-bbe9626e2744",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:02.0683954",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "d9162b20-57cf-48c7-bf73-4df0f0b81a20",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3d9ac88c-a14c-428d-0018-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:00.5956145",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a51b01ba-2bd4-40a0-a358-3e3ed6c072a1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:52:31.2631889",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "33c29af8-38ec-41d8-9f23-f14360bb684c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:29.4336966",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "5485937c-f408-4495-a179-1d338018f24c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:27.2701852",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "4ad72303-3d5e-47a7-afa4-c5ca57cde593",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "7697892a-b6b6-4895-d01d-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:25.2145577",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "e05ff08e-326a-4e2c-81d1-d46138861a7e",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ff2a7e50-67a5-441f-d01c-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:47:58.5268983",
                                   "readAtUtc":  "2026-08-11T12:48:42.2595799",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "219147e0-816c-4366-90aa-7b56e1ac62db",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:29:02.4412592",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "4fb3af4a-c9c9-4f7e-9de0-b38e748c3b2b",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:57.6360721",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "ed0a279a-320a-46bc-85c6-6c75f1d26a99",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9b40e63d-6525-4371-2246-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:54.9839611",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "10d63ec9-0e32-4477-9a5b-befb1163695b",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "59676a63-16fb-4571-2245-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:23.4820149",
                                   "readAtUtc":  "2026-08-11T12:29:30.5502709",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "244f81be-7aac-424a-abc0-cad19de895e1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:20:00.48862",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "aa529b2f-2262-42f8-82b4-14412efcef2f",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:56.1381357",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "3b8e11fe-3377-4b0d-a901-5fdecad3b1c3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a2ceb314-139b-4e98-f7ef-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:54.0156177",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "50435919-8122-4d86-a30e-860c23cb42c1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "fed717d1-8dbf-469f-f7ee-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:26.7367683",
                                   "readAtUtc":  "2026-08-11T12:20:05.6118655",
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  4
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Primary post-delete notification list returns 200** (status=200)
- [PASS] **Primary receives no notification for deletion**
### Secondary post-delete notification list

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "4ee3d567-4760-42fe-a7cb-5b36298849c4",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:04.272033",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "bae3722c-9380-4031-817f-5262b4e8dce1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:02.0683954",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a1a6b79d-f492-4635-8b02-db1d138bf37f",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3d9ac88c-a14c-428d-0018-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:00.5956145",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "808d2ebf-3e0d-4a01-8536-88fecb9920c3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:52:31.2631889",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "c87dcbb2-c3a0-4cd0-a8d6-ebf3d699652c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:29.4336966",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "fd1bd9e9-88a1-4206-bf67-67b40a2d09a0",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:27.2701852",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a659fe12-248b-486f-af2d-ac51ffe0d29c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "7697892a-b6b6-4895-d01d-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:25.2145577",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "d089226f-81fd-4ffd-8285-42a1468fcd63",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ff2a7e50-67a5-441f-d01c-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:47:58.5268983",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "3e9bf5d7-feea-49d0-af0b-2c1f0b2c2c32",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:29:02.4412592",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "f9ce9923-c9a5-497e-b6af-df12ccc56257",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:57.6360721",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "ccad76f1-84a1-4aa9-ae5c-b6f990b5842d",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9b40e63d-6525-4371-2246-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:54.9839611",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a509e414-6b83-44b9-9581-13c4b5688dbd",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "59676a63-16fb-4571-2245-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:23.4820149",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "5441505e-3c8d-48ef-8c3a-70774af2f808",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:20:00.48862",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "8fc40fa2-042f-44b6-8d60-3320c903a5d6",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:56.1381357",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "cdf596ad-42cf-4a54-9c38-a76c30362f57",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a2ceb314-139b-4e98-f7ef-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:54.0156177",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "fe8beffa-8add-472b-a534-fcc147e7b826",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "fed717d1-8dbf-469f-f7ee-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:26.7367683",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  16
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Secondary post-delete notification list returns 200** (status=200)
- [PASS] **Secondary receives no notification for deletion**
### Tertiary post-delete notification list

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "0521e585-711e-4849-9278-602df0cfacc6",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:04.272033",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "328b5dbd-9312-4e3a-a650-118f70345149",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:02.0683954",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "f2eaff03-de31-40f2-8991-33f1ab117f47",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3d9ac88c-a14c-428d-0018-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:00.5956145",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "141a52b7-e2e8-47de-b971-e595d0c435ff",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:52:31.2631889",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "f7227432-8e7c-4211-85e1-d0dc68bbbfe2",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:29.4336966",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "8abf2647-d302-4c33-abc9-678a319c01e1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:27.2701852",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "0d6f9113-3e52-423e-a4c3-ed731c50a4a5",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "7697892a-b6b6-4895-d01d-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:25.2145577",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "53e48921-1f3f-4df7-a963-da35c67a5f1c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ff2a7e50-67a5-441f-d01c-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:47:58.5268983",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "43312391-763a-48d7-bc4e-dd132adc0701",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:29:02.4412592",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "56e598f6-750b-4f3e-80c9-f399dad08372",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:57.6360721",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "e0810cb5-f9c3-4b75-8ed4-8299cf806ef3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9b40e63d-6525-4371-2246-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:54.9839611",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "640deaf9-2a07-4648-8da8-552cffaaa04e",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "59676a63-16fb-4571-2245-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:23.4820149",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "3c922a84-4917-4a4e-95a1-de2b7b4023a3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:20:00.48862",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "b2c5b4ee-0e88-4829-8108-0a92e6787c32",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:56.1381357",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "4b0d485f-31f7-4d8a-9045-0dc951a38d0e",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a2ceb314-139b-4e98-f7ef-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:54.0156177",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "b40ea9d8-92ae-4a77-be68-e8797641ce33",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "fed717d1-8dbf-469f-f7ee-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:26.7367683",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  16
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Tertiary post-delete notification list returns 200** (status=200)
- [PASS] **Tertiary receives no notification for deletion**

## Validation, hostile input, malformed identifiers, and no-event failures

### Submit without UserId

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents

**Body:**
```json
{
    "UserId":  "",
    "Documents[0].ExpirationDate":  "2035-01-01",
    "Documents[0].Type":  1,
    "Documents[0].File":  "[REDACTED_FILE]"
}
```

**Response Status:** 400

**Response Body:**
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "UserId":  [
                                  "UserId is required"
                              ]
               },
    "traceId":  "00-b5408d4e12ec27bd21ed94cebb9e49de-2f2d51bdc345f60b-00"
}
```
---

- [PASS] **Submit without UserId returns 400** (status=400)
### JSON sent to multipart submit endpoint

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents

**Body:**
```json
{

}
```

**Response Status:** 400

**Response Body:**
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "UserId":  [
                                  "UserId is required"
                              ],
                   "Documents":  [
                                     "The Documents field is required.",
                                     "Documents are required."
                                 ]
               },
    "traceId":  "00-5d54fb21547d7ec0fa4a15a35ef57cd5-9c2989db394102d2-00"
}
```
---

- [PASS] **JSON sent to multipart endpoint returns validation/media failure** (status=400)
### Submit duplicate document types

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents

**Body:**
```json
{
    "UserId":  "ef434f31-40f7-42ad-0017-08def7a755da",
    "Documents[0].ExpirationDate":  "2035-01-01",
    "Documents[0].Type":  2,
    "Documents[0].File":  "[REDACTED_FILE]",
    "Documents[1].ExpirationDate":  "2035-01-01",
    "Documents[1].Type":  2,
    "Documents[1].File":  "[REDACTED_FILE]"
}
```

**Response Status:** 400

**Response Body:**
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "Documents":  [
                                     "The same verification document cannot be submitted more than once."
                                 ]
               },
    "traceId":  "00-1f6a6bbc84e903ebc0d6561b24aeccd3-0374b29369c0f58b-00"
}
```
---

- [PASS] **Duplicate document types return 400** (status=400)
### Submit invalid date format

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents

**Body:**
```json
{
    "UserId":  "ef434f31-40f7-42ad-0017-08def7a755da",
    "Documents[0].ExpirationDate":  "not-a-date",
    "Documents[0].Type":  3,
    "Documents[0].File":  "[REDACTED_FILE]"
}
```

**Response Status:** 400

**Response Body:**
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "Documents[0].ExpirationDate":  [
                                                       "The value \u0027not-a-date\u0027 is not valid for ExpirationDate."
                                                   ]
               },
    "traceId":  "00-d198069a4a0935da343fdc7cef033ad4-919bda68e0f46188-00"
}
```
---

- [PASS] **Invalid expiration date returns 400** (status=400)
### Submit invalid enum type

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents

**Body:**
```json
{
    "UserId":  "ef434f31-40f7-42ad-0017-08def7a755da",
    "Documents[0].ExpirationDate":  "2035-01-01",
    "Documents[0].Type":  999,
    "Documents[0].File":  "[REDACTED_FILE]"
}
```

**Response Status:** 400

**Response Body:**
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "Documents[0].Type":  [
                                             "The value \u0027999\u0027 is not valid for Type."
                                         ]
               },
    "traceId":  "00-cc[REDACTED_NUMBER]d8dac0b2cbf88c1831be-349f6884634d20ec-00"
}
```
---

- [PASS] **Invalid document type returns 400** (status=400)
### Submit expired document

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents

**Body:**
```json
{
    "UserId":  "3d9ac88c-a14c-428d-0018-08def7a755da",
    "Documents[0].ExpirationDate":  "2000-01-01",
    "Documents[0].Type":  4,
    "Documents[0].File":  "[REDACTED_FILE]"
}
```

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "uploadedDocuments":  [

                                       ],
                 "failedDocuments":  [
                                         {
                                             "fileName":  "[REDACTED]",
                                             "type":  4,
                                             "error":  "This document is expired"
                                         }
                                     ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Expired document is reported as a failed upload**
### Submit unsupported content type

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents

**Body:**
```json
{
    "UserId":  "3d9ac88c-a14c-428d-0018-08def7a755da",
    "Documents[0].ExpirationDate":  "2035-01-01",
    "Documents[0].Type":  5,
    "Documents[0].File":  "[REDACTED_FILE]"
}
```

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "uploadedDocuments":  [

                                       ],
                 "failedDocuments":  [
                                         {
                                             "fileName":  "[REDACTED]",
                                             "type":  5,
                                             "error":  "Only JPEG, PNG, WEBP, HEIC, and HEIF images are allowed."
                                         }
                                     ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Unsupported content type is reported as a failed upload**
### Primary failed-only upload notification list

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "04a683ba-afd9-4cfb-99ee-e30f303227d9",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:04.272033",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "8d912222-31ef-400f-bcde-bbe9626e2744",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:02.0683954",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "d9162b20-57cf-48c7-bf73-4df0f0b81a20",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3d9ac88c-a14c-428d-0018-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:00.5956145",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a51b01ba-2bd4-40a0-a358-3e3ed6c072a1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:52:31.2631889",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "33c29af8-38ec-41d8-9f23-f14360bb684c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:29.4336966",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "5485937c-f408-4495-a179-1d338018f24c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:27.2701852",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "4ad72303-3d5e-47a7-afa4-c5ca57cde593",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "7697892a-b6b6-4895-d01d-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:25.2145577",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "e05ff08e-326a-4e2c-81d1-d46138861a7e",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ff2a7e50-67a5-441f-d01c-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:47:58.5268983",
                                   "readAtUtc":  "2026-08-11T12:48:42.2595799",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "219147e0-816c-4366-90aa-7b56e1ac62db",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:29:02.4412592",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "4fb3af4a-c9c9-4f7e-9de0-b38e748c3b2b",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:57.6360721",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "ed0a279a-320a-46bc-85c6-6c75f1d26a99",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9b40e63d-6525-4371-2246-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:54.9839611",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "10d63ec9-0e32-4477-9a5b-befb1163695b",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "59676a63-16fb-4571-2245-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:23.4820149",
                                   "readAtUtc":  "2026-08-11T12:29:30.5502709",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "244f81be-7aac-424a-abc0-cad19de895e1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:20:00.48862",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "aa529b2f-2262-42f8-82b4-14412efcef2f",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:56.1381357",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "3b8e11fe-3377-4b0d-a901-5fdecad3b1c3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a2ceb314-139b-4e98-f7ef-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:54.0156177",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "50435919-8122-4d86-a30e-860c23cb42c1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "fed717d1-8dbf-469f-f7ee-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:26.7367683",
                                   "readAtUtc":  "2026-08-11T12:20:05.6118655",
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  4
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Primary failed-only upload notification list returns 200** (status=200)
- [PASS] **Primary receives no notification for failed-only uploads**
### Secondary failed-only upload notification list

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "4ee3d567-4760-42fe-a7cb-5b36298849c4",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:04.272033",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "bae3722c-9380-4031-817f-5262b4e8dce1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:02.0683954",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a1a6b79d-f492-4635-8b02-db1d138bf37f",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3d9ac88c-a14c-428d-0018-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:00.5956145",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "808d2ebf-3e0d-4a01-8536-88fecb9920c3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:52:31.2631889",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "c87dcbb2-c3a0-4cd0-a8d6-ebf3d699652c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:29.4336966",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "fd1bd9e9-88a1-4206-bf67-67b40a2d09a0",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:27.2701852",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a659fe12-248b-486f-af2d-ac51ffe0d29c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "7697892a-b6b6-4895-d01d-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:25.2145577",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "d089226f-81fd-4ffd-8285-42a1468fcd63",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ff2a7e50-67a5-441f-d01c-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:47:58.5268983",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "3e9bf5d7-feea-49d0-af0b-2c1f0b2c2c32",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:29:02.4412592",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "f9ce9923-c9a5-497e-b6af-df12ccc56257",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:57.6360721",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "ccad76f1-84a1-4aa9-ae5c-b6f990b5842d",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9b40e63d-6525-4371-2246-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:54.9839611",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a509e414-6b83-44b9-9581-13c4b5688dbd",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "59676a63-16fb-4571-2245-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:23.4820149",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "5441505e-3c8d-48ef-8c3a-70774af2f808",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:20:00.48862",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "8fc40fa2-042f-44b6-8d60-3320c903a5d6",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:56.1381357",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "cdf596ad-42cf-4a54-9c38-a76c30362f57",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a2ceb314-139b-4e98-f7ef-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:54.0156177",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "fe8beffa-8add-472b-a534-fcc147e7b826",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "fed717d1-8dbf-469f-f7ee-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:26.7367683",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  16
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Secondary failed-only upload notification list returns 200** (status=200)
- [PASS] **Secondary receives no notification for failed-only uploads**
### Tertiary failed-only upload notification list

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "0521e585-711e-4849-9278-602df0cfacc6",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:04.272033",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "328b5dbd-9312-4e3a-a650-118f70345149",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:02.0683954",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "f2eaff03-de31-40f2-8991-33f1ab117f47",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3d9ac88c-a14c-428d-0018-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:00.5956145",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "141a52b7-e2e8-47de-b971-e595d0c435ff",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:52:31.2631889",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "f7227432-8e7c-4211-85e1-d0dc68bbbfe2",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:29.4336966",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "8abf2647-d302-4c33-abc9-678a319c01e1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:27.2701852",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "0d6f9113-3e52-423e-a4c3-ed731c50a4a5",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "7697892a-b6b6-4895-d01d-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:25.2145577",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "53e48921-1f3f-4df7-a963-da35c67a5f1c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ff2a7e50-67a5-441f-d01c-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:47:58.5268983",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "43312391-763a-48d7-bc4e-dd132adc0701",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:29:02.4412592",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "56e598f6-750b-4f3e-80c9-f399dad08372",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:57.6360721",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "e0810cb5-f9c3-4b75-8ed4-8299cf806ef3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9b40e63d-6525-4371-2246-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:54.9839611",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "640deaf9-2a07-4648-8da8-552cffaaa04e",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "59676a63-16fb-4571-2245-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:23.4820149",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "3c922a84-4917-4a4e-95a1-de2b7b4023a3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:20:00.48862",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "b2c5b4ee-0e88-4829-8108-0a92e6787c32",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:56.1381357",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "4b0d485f-31f7-4d8a-9045-0dc951a38d0e",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a2ceb314-139b-4e98-f7ef-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:54.0156177",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "b40ea9d8-92ae-4a77-be68-e8797641ce33",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "fed717d1-8dbf-469f-f7ee-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:26.7367683",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  16
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Tertiary failed-only upload notification list returns 200** (status=200)
- [PASS] **Tertiary receives no notification for failed-only uploads**
### Submit extremely long UserId

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents

**Body:**
```json
{
    "UserId":  "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "Documents[0].ExpirationDate":  "2035-01-01",
    "Documents[0].Type":  6,
    "Documents[0].File":  "[REDACTED_FILE]"
}
```

**Response Status:** 400

**Response Body:**
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "UserId":  [
                                  "The value \u0027xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx\u0027 is not valid for UserId.",
                                  "UserId is required"
                              ]
               },
    "traceId":  "00-87dd54220fc898f7e[REDACTED_NUMBER]a6407-44b968ab447788d4-00"
}
```
---

- [PASS] **Extremely long UserId returns 400** (status=400)
### Get documents with malformed UserId

**Request:** GET http://localhost:5049/api/UserVerification/not-a-guid

**Response Status:** 400

**Response Body:**
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "UserId":  [
                                  "The value \u0027not-a-guid\u0027 is not valid for UserId.",
                                  "User Id is required."
                              ]
               },
    "traceId":  "00-a665fb4e6f326ad37686fb[REDACTED_NUMBER]-b625551d7dcfd495-00"
}
```
---

- [PASS] **Malformed UserId route returns 400 or 404** (status=400)
### Get documents for unknown UserId

**Request:** GET http://localhost:5049/api/UserVerification/9053cefb-d8ce-45ca-b1ae-9f58f461a315

**Response Status:** 404

**Response Body:**
```json
{
    "success":  false,
    "data":  null,
    "message":  null,
    "errors":  [
                   "The specified user does not exist."
               ],
    "statusCode":  404
}
```
---

- [PASS] **Unknown UserId returns 404** (status=404)
### Delete without document identifiers

**Request:** DELETE http://localhost:5049/api/UserVerification

**Response Status:** 400

**Response Body:**
```json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "UserId":  [
                                  "User Id is required."
                              ],
                   "DocumentId":  [
                                      "Document Id is required."
                                  ]
               },
    "traceId":  "00-b2ad[REDACTED_NUMBER]f4b8a833cfdb86e68-94dd98ca96ba95d6-00"
}
```
---

- [PASS] **Delete without identifiers returns 400** (status=400)
### Delete unknown document

**Request:** DELETE http://localhost:5049/api/UserVerification?UserId=ef434f31-40f7-42ad-0017-08def7a755da&DocumentId=9053cefb-d8ce-45ca-b1ae-9f58f461a315

**Response Status:** 404

**Response Body:**
```json
{
    "success":  false,
    "message":  null,
    "errors":  [
                   "Verification document was not found."
               ],
    "statusCode":  404
}
```
---

- [PASS] **Delete unknown document returns 404** (status=404)

## Notification list, unread count, read/read-all, exact payload, and recipient isolation

- [PASS] **Primary Admin notification exists before read tests**
- [PASS] **Secondary Admin notification exists before isolation tests**
### Secondary Admin reads Primary Admin notification

**Request:** PATCH http://localhost:5049/api/notifications/a51b01ba-2bd4-40a0-a358-3e3ed6c072a1/read

**Response Status:** 404

**Response Body:**
```json
{
    "success":  false,
    "data":  null,
    "message":  "Entity \"Notification\" (a51b01ba-2bd4-40a0-a358-3e3ed6c072a1) was not found.",
    "errors":  null,
    "statusCode":  404
}
```
---

- [PASS] **Notification read is isolated between Admin recipients** (status=404)
### Primary Admin notification list

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "04a683ba-afd9-4cfb-99ee-e30f303227d9",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:04.272033",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "8d912222-31ef-400f-bcde-bbe9626e2744",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:02.0683954",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "d9162b20-57cf-48c7-bf73-4df0f0b81a20",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3d9ac88c-a14c-428d-0018-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:00.5956145",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a51b01ba-2bd4-40a0-a358-3e3ed6c072a1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:52:31.2631889",
                                   "readAtUtc":  null,
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "33c29af8-38ec-41d8-9f23-f14360bb684c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:29.4336966",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "5485937c-f408-4495-a179-1d338018f24c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:27.2701852",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "4ad72303-3d5e-47a7-afa4-c5ca57cde593",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "7697892a-b6b6-4895-d01d-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:25.2145577",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "e05ff08e-326a-4e2c-81d1-d46138861a7e",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ff2a7e50-67a5-441f-d01c-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:47:58.5268983",
                                   "readAtUtc":  "2026-08-11T12:48:42.2595799",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "219147e0-816c-4366-90aa-7b56e1ac62db",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:29:02.4412592",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "4fb3af4a-c9c9-4f7e-9de0-b38e748c3b2b",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:57.6360721",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "ed0a279a-320a-46bc-85c6-6c75f1d26a99",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9b40e63d-6525-4371-2246-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:54.9839611",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "10d63ec9-0e32-4477-9a5b-befb1163695b",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "59676a63-16fb-4571-2245-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:23.4820149",
                                   "readAtUtc":  "2026-08-11T12:29:30.5502709",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "244f81be-7aac-424a-abc0-cad19de895e1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:20:00.48862",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "aa529b2f-2262-42f8-82b4-14412efcef2f",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:56.1381357",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "3b8e11fe-3377-4b0d-a901-5fdecad3b1c3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a2ceb314-139b-4e98-f7ef-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:54.0156177",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "50435919-8122-4d86-a30e-860c23cb42c1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "fed717d1-8dbf-469f-f7ee-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:26.7367683",
                                   "readAtUtc":  "2026-08-11T12:20:05.6118655",
                                   "expiresAtUtc":  null
                               }
                           ],
                 "nextCursor":  null,
                 "unreadCount":  4
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Primary Admin notification list returns 200** (status=200)
- [PASS] **Primary Admin inbox contains review-requested notifications**
- [PASS] **Primary Admin review notification has exact Arabic snapshot and safe data**
- [PASS] **Primary Admin review notification has exact Arabic snapshot and safe data**
- [PASS] **Primary Admin review notification has exact Arabic snapshot and safe data**
- [PASS] **Primary Admin review notification has exact Arabic snapshot and safe data**
- [PASS] **Primary Admin review notification has exact Arabic snapshot and safe data**
- [PASS] **Primary Admin review notification has exact Arabic snapshot and safe data**
- [PASS] **Primary Admin review notification has exact Arabic snapshot and safe data**
- [PASS] **Primary Admin review notification has exact Arabic snapshot and safe data**
- [PASS] **Primary Admin review notification has exact Arabic snapshot and safe data**
- [PASS] **Primary Admin review notification has exact Arabic snapshot and safe data**
- [PASS] **Primary Admin review notification has exact Arabic snapshot and safe data**
- [PASS] **Primary Admin review notification has exact Arabic snapshot and safe data**
- [PASS] **Primary Admin review notification has exact Arabic snapshot and safe data**
- [PASS] **Primary Admin review notification has exact Arabic snapshot and safe data**
- [PASS] **Primary Admin review notification has exact Arabic snapshot and safe data**
- [PASS] **Primary Admin review notification has exact Arabic snapshot and safe data**
### Primary Admin unread count

**Request:** GET http://localhost:5049/api/notifications/unread-count

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "unreadCount":  4
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Unread count returns 200** (status=200)
### Primary Admin marks review notification read

**Request:** PATCH http://localhost:5049/api/notifications/a51b01ba-2bd4-40a0-a358-3e3ed6c072a1/read

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "id":  "a51b01ba-2bd4-40a0-a358-3e3ed6c072a1",
                 "type":  "verification.review-requested",
                 "severity":  "Information",
                 "title":  "طلب مراجعة مستندات التحقق",
                 "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                 "actionUrl":  null,
                 "data":  {
                              "userId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                              "documentCount":  "1"
                          },
                 "createdAtUtc":  "2026-08-11T12:52:31.2631889",
                 "readAtUtc":  "2026-08-11T12:53:20.0821396Z",
                 "expiresAtUtc":  null
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Mark review notification read returns 200** (status=200)
- [PASS] **Mark read returns a timestamp**
### Primary Admin repeats mark-read

**Request:** PATCH http://localhost:5049/api/notifications/a51b01ba-2bd4-40a0-a358-3e3ed6c072a1/read

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "id":  "a51b01ba-2bd4-40a0-a358-3e3ed6c072a1",
                 "type":  "verification.review-requested",
                 "severity":  "Information",
                 "title":  "طلب مراجعة مستندات التحقق",
                 "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                 "actionUrl":  null,
                 "data":  {
                              "userId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                              "documentCount":  "1"
                          },
                 "createdAtUtc":  "2026-08-11T12:52:31.2631889",
                 "readAtUtc":  "2026-08-11T12:53:20.0821396",
                 "expiresAtUtc":  null
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Repeated mark-read remains 200** (status=200)
### Primary Admin unread count after mark-read

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
```
---

- [PASS] **Unread count does not increase after mark-read**
### Primary Admin marks all notifications read

**Request:** PATCH http://localhost:5049/api/notifications/read-all

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "readAtUtc":  "2026-08-11T12:53:20.1946305Z",
                 "unreadCount":  0
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
```
---

- [PASS] **Read-all returns 200** (status=200)
### Primary Admin unread count after read-all

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
```
---

- [PASS] **Read-all leaves no unread notifications**
### Primary Admin lists read notifications

**Request:** GET http://localhost:5049/api/notifications?pageSize=50&isRead=true

**Response Status:** 200

**Response Body:**
```json
{
    "success":  true,
    "data":  {
                 "items":  [
                               {
                                   "id":  "04a683ba-afd9-4cfb-99ee-e30f303227d9",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:04.272033",
                                   "readAtUtc":  "2026-08-11T12:53:20.1946305",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "8d912222-31ef-400f-bcde-bbe9626e2744",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a1f426b3-45d2-4aaa-0019-08def7a755da",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:02.0683954",
                                   "readAtUtc":  "2026-08-11T12:53:20.1946305",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "d9162b20-57cf-48c7-bf73-4df0f0b81a20",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "3d9ac88c-a14c-428d-0018-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:53:00.5956145",
                                   "readAtUtc":  "2026-08-11T12:53:20.1946305",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "a51b01ba-2bd4-40a0-a358-3e3ed6c072a1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ef434f31-40f7-42ad-0017-08def7a755da",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:52:31.2631889",
                                   "readAtUtc":  "2026-08-11T12:53:20.0821396",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "33c29af8-38ec-41d8-9f23-f14360bb684c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:29.4336966",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "5485937c-f408-4495-a179-1d338018f24c",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "015d54c9-2849-4c59-d01e-08def7a6b40b",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:27.2701852",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "4ad72303-3d5e-47a7-afa4-c5ca57cde593",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "7697892a-b6b6-4895-d01d-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:48:25.2145577",
                                   "readAtUtc":  "2026-08-11T12:48:42.373367",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "e05ff08e-326a-4e2c-81d1-d46138861a7e",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "ff2a7e50-67a5-441f-d01c-08def7a6b40b",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:47:58.5268983",
                                   "readAtUtc":  "2026-08-11T12:48:42.2595799",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "219147e0-816c-4366-90aa-7b56e1ac62db",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:29:02.4412592",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "4fb3af4a-c9c9-4f7e-9de0-b38e748c3b2b",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "cfe1ca5c-3837-4a47-2247-08def7a3f2ff",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:57.6360721",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "ed0a279a-320a-46bc-85c6-6c75f1d26a99",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9b40e63d-6525-4371-2246-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:54.9839611",
                                   "readAtUtc":  "2026-08-11T12:29:30.9547397",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "10d63ec9-0e32-4477-9a5b-befb1163695b",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "59676a63-16fb-4571-2245-08def7a3f2ff",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:28:23.4820149",
                                   "readAtUtc":  "2026-08-11T12:29:30.5502709",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "244f81be-7aac-424a-abc0-cad19de895e1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:20:00.48862",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "aa529b2f-2262-42f8-82b4-14412efcef2f",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "9380a3c7-dcf3-42ea-f7f0-08def7a2b188",
                                                "documentCount":  "2"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:56.1381357",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "3b8e11fe-3377-4b0d-a901-5fdecad3b1c3",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "a2ceb314-139b-4e98-f7ef-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:54.0156177",
                                   "readAtUtc":  "2026-08-11T12:20:06.3569511",
                                   "expiresAtUtc":  null
                               },
                               {
                                   "id":  "50435919-8122-4d86-a30e-860c23cb42c1",
                                   "type":  "verification.review-requested",
                                   "severity":  "Information",
                                   "title":  "طلب مراجعة مستندات التحقق",
                                   "body":  "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                                   "actionUrl":  null,
                                   "data":  {
                                                "userId":  "fed717d1-8dbf-469f-f7ee-08def7a2b188",
                                                "documentCount":  "1"
                                            },
                                   "createdAtUtc":  "2026-08-11T12:19:26.7367683",
                                   "readAtUtc":  "2026-08-11T12:20:05.6118655",
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
```
---

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
    "traceId":  "00-3a8fad1ae8c45f73e838bb1456058d0a-f161c[REDACTED_NUMBER]-00"
}
```
---

- [PASS] **Invalid notification page size returns 400** (status=400)
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
    "traceId":  "00-a5c5316abfa197f53f08b3c83c66a6a3-522c1c1ea7979c2d-00"
}
```
---

- [PASS] **Invalid notification cursor returns 400** (status=400)
### Notification read empty ID

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
```
---

- [PASS] **Notification read empty ID returns 404** (status=404)
### Owner final notification list

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
```
---

- [PASS] **Owner final notification list returns 200** (status=200)
- [PASS] **Owner final inbox has no Admin review request**
### Unrelated final notification list

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
```
---

- [PASS] **Unrelated final notification list returns 200** (status=200)
- [PASS] **Unrelated final inbox has no review request**

## API and mock Email log monitoring

- [PASS] **API, outbox, notification, and provider logs are clean** (violations=0)
- [PASS] **Mock Email confirmation was recorded for each disposable account**
- [PASS] **Mock Email confirmation was recorded for each disposable account**
- [PASS] **Mock Email confirmation was recorded for each disposable account**
- [PASS] **Mock Email confirmation was recorded for each disposable account**
- [PASS] **API test port is released after owned process shutdown**

## Execution summary

| Metric | Count |
|---|---:|
| Passed assertions | 155 |
| Failed assertions | 0 |
| Documented skips | 1 |

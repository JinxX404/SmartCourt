# Admin Verifications Slice Test Report


### 0a. Setup - Login Admin

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Admin@123",
    "Email":  "moatazmohammed2392003@gmail.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "42b7c6b9-8690-456c-716a-08dee31497fe",
                              "email":  "moatazmohammed2392003@gmail.com",
                              "fullName":  "Moataz Mohammed",
                              "role":  "Admin"
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0MmI3YzZiOS04NjkwLTQ1NmMtNzE2YS0wOGRlZTMxNDk3ZmUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjQyYjdjNmI5LTg2OTAtNDU2Yy03MTZhLTA4ZGVlMzE0OTdmZSIsImVtYWlsIjoibW9hdGF6bW9oYW1tZWQyMzkyMDAzQGdtYWlsLmNvbSIsIm5hbWUiOiJNb2F0YXogTW9oYW1tZWQiLCJzZWN1cml0eV9zdGFtcCI6IlFWMkhOTEtXUENLUVNQMlE1VTJPQkNBVTRYVFdYUUJTIiwianRpIjoiMTA0MGY0MzktZWZhMy00OTg0LTg5NjMtMDZhZTJkZjA4MWY0IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJuYmYiOjE3ODU5NDQ2MDUsImV4cCI6MTc4NTk0ODIwNSwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.OFJ2uktGACr9XE8-NbPAmnyMiSF06yTASnRT-1pDD78",
                 "expiresIn":  3600,
                 "refreshToken":  "w/idkOw+Trzd+47j8lVEbgXZJXP1KSVTr21WuhEvNK1BL04A601nLFRCo6D+61jcnLcAqCkzhnSW3Or/BDu62g==",
                 "refreshTokenExpiration":  "2026-08-12T15:43:25.0731047Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 0b. Setup - Register Lawyer

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
`json
{
    "Email":  "lawyer_verification_192134597@test.com",
    "FullName":  "Lawyer Verification",
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
                 "userId":  "d19a6821-6f53-4d0b-e47a-08def300e686",
                 "email":  "lawyer_verification_192134597@test.com",
                 "fullName":  "Lawyer Verification",
                 "role":  "Lawyer"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
``n---


Found confirmation URL for lawyer_verification_192134597@test.com: http://localhost:5173/verify-email?userId=d19a6821-6f53-4d0b-e47a-08def300e686&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5MXcvSG5EaUEvbGpUa3BRdjdaRjFBakdqNXBSYk1aWGpPNnZXZTRPYW1hQ2U1bzR6cklRYWxuMDc2YjhsRmxjZVFtejNTYloxL1hTNnVGZFh4YmFWMU84UXhyajhiMU5RUEJYOUFyMkpHSlI1SGhJT3lueWRnUzhHTllBVFY1V2tzYU1QTGRJZFJxd3ZqcCtvMjc3YzJxTDZFL1FFOHNTM24ybHpDV3o4eVhOUzlaQ3VNcHRicm92WFhld0lCaFVGUm9GTlRvd1o5VVErQ0dWQzZYelFCTFZ0UC9pbWg2MXpBVUxpUjdwUlZCdz09

### Confirm Email for lawyer_verification_192134597@test.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=d19a6821-6f53-4d0b-e47a-08def300e686&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5MXcvSG5EaUEvbGpUa3BRdjdaRjFBakdqNXBSYk1aWGpPNnZXZTRPYW1hQ2U1bzR6cklRYWxuMDc2YjhsRmxjZVFtejNTYloxL1hTNnVGZFh4YmFWMU84UXhyajhiMU5RUEJYOUFyMkpHSlI1SGhJT3lueWRnUzhHTllBVFY1V2tzYU1QTGRJZFJxd3ZqcCtvMjc3YzJxTDZFL1FFOHNTM24ybHpDV3o4eVhOUzlaQ3VNcHRicm92WFhld0lCaFVGUm9GTlRvd1o5VVErQ0dWQzZYelFCTFZ0UC9pbWg2MXpBVUxpUjdwUlZCdz09

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


### 0c. Setup - Login Lawyer

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Password123!",
    "Email":  "lawyer_verification_192134597@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "d19a6821-6f53-4d0b-e47a-08def300e686",
                              "email":  "lawyer_verification_192134597@test.com",
                              "fullName":  "Lawyer Verification",
                              "role":  "Lawyer"
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJkMTlhNjgyMS02ZjUzLTRkMGItZTQ3YS0wOGRlZjMwMGU2ODYiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImQxOWE2ODIxLTZmNTMtNGQwYi1lNDdhLTA4ZGVmMzAwZTY4NiIsImVtYWlsIjoibGF3eWVyX3ZlcmlmaWNhdGlvbl8xOTIxMzQ1OTdAdGVzdC5jb20iLCJuYW1lIjoiTGF3eWVyIFZlcmlmaWNhdGlvbiIsInNlY3VyaXR5X3N0YW1wIjoiSFVYVlFKSk5ONlMyV1ZQNFZOMzVFTkY3R0VRSDNMSEIiLCJqdGkiOiI3OGJkZWQ3OS04MGViLTQ4MzMtOWExNy1iYmFlYzllODlkNDciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODU5NDQ2MTAsImV4cCI6MTc4NTk0ODIxMCwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.T7BsUELH4PwhHaPMcfxWUcaaEGR22cVq7oOdLdQrMI4",
                 "expiresIn":  3600,
                 "refreshToken":  "NMLfrmks6KfsIef3ff2VnnmVdUvtoOYg7UTWOusOpZGIXtvxClu6Dji55ly7iaH+vnKoLupwTUc0A9ISWb0oEA==",
                 "refreshTokenExpiration":  "2026-08-12T15:43:30.5389596Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 0d. Setup - Lawyer Uploads Document

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**
- Documents[0].Type = 1
- UserId = d19a6821-6f53-4d0b-e47a-08def300e686
- Documents[0].ExpirationDate = 2030-01-01
- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\dummy_admin_id.jpg]


**Response Status:** 200

**Response Body:**
```json
{"success":true,"data":{"uploadedDocuments":[{"fileName":"dummy_admin_id.jpg","type":1}],"failedDocuments":[]},"message":null,"errors":null,"statusCode":200}
```n---


### 1. Get Pending Verifications (Admin)

**Request:** GET http://localhost:5049/api/admin/verifications?PageNumber=1&PageSize=10

**Response Status:** 200

**Response Body:**
`json
{
    "pageNumber":  1,
    "pageSize":  10,
    "totalPages":  1,
    "totalRecords":  4,
    "hasNextPage":  false,
    "hasPreviousPage":  false,
    "success":  true,
    "data":  [
                 {
                     "lawyerId":  "65371797-7bf8-44f5-e472-08def300e686",
                     "fullName":  "Lawyer Verification",
                     "email":  "lawyer_verification_1518525995@test.com",
                     "phoneNumber":  null,
                     "pendingDocumentCount":  1,
                     "verifiedDocumentCount":  0,
                     "rejectedDocumentCount":  0
                 },
                 {
                     "lawyerId":  "a2f8005f-2feb-4e6a-e474-08def300e686",
                     "fullName":  "Lawyer Verification",
                     "email":  "lawyer_verification_852046148@test.com",
                     "phoneNumber":  null,
                     "pendingDocumentCount":  2,
                     "verifiedDocumentCount":  0,
                     "rejectedDocumentCount":  0
                 },
                 {
                     "lawyerId":  "6a7b8032-b6a3-46bf-e476-08def300e686",
                     "fullName":  "Lawyer Verification",
                     "email":  "lawyer_verification_423361630@test.com",
                     "phoneNumber":  null,
                     "pendingDocumentCount":  1,
                     "verifiedDocumentCount":  0,
                     "rejectedDocumentCount":  1
                 },
                 {
                     "lawyerId":  "d19a6821-6f53-4d0b-e47a-08def300e686",
                     "fullName":  "Lawyer Verification",
                     "email":  "lawyer_verification_192134597@test.com",
                     "phoneNumber":  null,
                     "pendingDocumentCount":  1,
                     "verifiedDocumentCount":  0,
                     "rejectedDocumentCount":  0
                 }
             ],
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 2. Get Verification Details (Admin)

**Request:** GET http://localhost:5049/api/admin/verifications/d19a6821-6f53-4d0b-e47a-08def300e686

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "lawyerId":  "d19a6821-6f53-4d0b-e47a-08def300e686",
                 "fullName":  "Lawyer Verification",
                 "email":  "lawyer_verification_192134597@test.com",
                 "phoneNumber":  null,
                 "accountStatus":  "Unverified",
                 "isFullyVerified":  false,
                 "documents":  [
                                   {
                                       "documentId":  "87a80219-2d48-4e51-f75f-08def3010d2a",
                                       "documentType":  "NationalIdFront",
                                       "status":  "Pending",
                                       "fileName":  "dummy_admin_id.jpg",
                                       "contentType":  "image/jpeg",
                                       "expirationDate":  "2030-01-01",
                                       "reviewedAt":  null,
                                       "rejectionReason":  null,
                                       "contentUrl":  "/api/admin/verifications/documents/87a80219-2d48-4e51-f75f-08def3010d2a/content"
                                   }
                               ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 3. Get Document Content (Admin)

**Request:** GET http://localhost:5049/api/admin/verifications/documents/87a80219-2d48-4e51-f75f-08def3010d2a/content

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "downloadUrl":  "https://msahvjipdwvgdartpeqj.supabase.co/storage/v1/object/public/smart-court-files/d19a6821-6f53-4d0b-e47a-08def300e686/national-id/1e473386-48eb-49b9-9325-11fc14f4364e.jpg",
                 "contentType":  "image/jpeg",
                 "fileName":  "dummy_admin_id.jpg"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 4. Review Verification Document - Reject

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/87a80219-2d48-4e51-f75f-08def3010d2a

**Body:**
`json
{
    "Decision":  2,
    "RejectionReason":  "Image is too blurry."
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "documentId":  "87a80219-2d48-4e51-f75f-08def3010d2a",
                 "documentStatus":  "Rejected",
                 "lawyerAccountStatus":  "Rejected",
                 "isFullyVerified":  false
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 5a. Setup - Lawyer Re-uploads Document

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**
- Documents[0].Type = 2
- UserId = d19a6821-6f53-4d0b-e47a-08def300e686
- Documents[0].ExpirationDate = 2030-01-01
- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\dummy_admin_id.jpg]


**Response Status:** 200

**Response Body:**
```json
{"success":true,"data":{"uploadedDocuments":[{"fileName":"dummy_admin_id.jpg","type":2}],"failedDocuments":[]},"message":null,"errors":null,"statusCode":200}
```n---


### 5b. Setup - Get Verification Details Again

**Request:** GET http://localhost:5049/api/admin/verifications/d19a6821-6f53-4d0b-e47a-08def300e686

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "lawyerId":  "d19a6821-6f53-4d0b-e47a-08def300e686",
                 "fullName":  "Lawyer Verification",
                 "email":  "lawyer_verification_192134597@test.com",
                 "phoneNumber":  null,
                 "accountStatus":  "Rejected",
                 "isFullyVerified":  false,
                 "documents":  [
                                   {
                                       "documentId":  "87a80219-2d48-4e51-f75f-08def3010d2a",
                                       "documentType":  "NationalIdFront",
                                       "status":  "Rejected",
                                       "fileName":  "dummy_admin_id.jpg",
                                       "contentType":  "image/jpeg",
                                       "expirationDate":  "2030-01-01",
                                       "reviewedAt":  null,
                                       "rejectionReason":  "Image is too blurry.",
                                       "contentUrl":  "/api/admin/verifications/documents/87a80219-2d48-4e51-f75f-08def3010d2a/content"
                                   },
                                   {
                                       "documentId":  "424c0eba-a235-43ce-f760-08def3010d2a",
                                       "documentType":  "NationalIdBack",
                                       "status":  "Pending",
                                       "fileName":  "dummy_admin_id.jpg",
                                       "contentType":  "image/jpeg",
                                       "expirationDate":  "2030-01-01",
                                       "reviewedAt":  null,
                                       "rejectionReason":  null,
                                       "contentUrl":  "/api/admin/verifications/documents/424c0eba-a235-43ce-f760-08def3010d2a/content"
                                   }
                               ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 6. Review Verification Document - Approve

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/424c0eba-a235-43ce-f760-08def3010d2a

**Body:**
`json
{
    "Decision":  1,
    "RejectionReason":  null
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "documentId":  "424c0eba-a235-43ce-f760-08def3010d2a",
                 "documentStatus":  "Verified",
                 "lawyerAccountStatus":  "Rejected",
                 "isFullyVerified":  false
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 7. Unauthorized Access (Lawyer Token)

**Request:** GET http://localhost:5049/api/admin/verifications

**Response Status:** 401

**Response Body:** (Empty)
---


### 8. Lawyer Deletes Rejected Document

**Request:** DELETE http://localhost:5049/api/UserVerification?UserId=d19a6821-6f53-4d0b-e47a-08def300e686&DocumentId=87a80219-2d48-4e51-f75f-08def3010d2a

**Response Status:** 404

**Response Body:**
`json
{
    "success":  false,
    "message":  null,
    "errors":  [
                   "Verification document was not found."
               ],
    "statusCode":  404
}
``n---


### 9. Get User Documents After Approval

**Request:** GET http://localhost:5049/api/UserVerification/d19a6821-6f53-4d0b-e47a-08def300e686

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "documents":  [
                                   {
                                       "documentId":  "d67afd0a-7f50-4db1-c6dc-08def3010d27",
                                       "documentType":  1,
                                       "status":  3,
                                       "expirationDate":  "2030-01-01",
                                       "isCurrent":  true,
                                       "fileName":  "dummy_admin_id.jpg"
                                   },
                                   {
                                       "documentId":  "f239a570-c159-4265-c6dd-08def3010d27",
                                       "documentType":  2,
                                       "status":  2,
                                       "expirationDate":  "2030-01-01",
                                       "isCurrent":  true,
                                       "fileName":  "dummy_admin_id.jpg"
                                   }
                               ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 10. Read - GET list invalid pagination

**Request:** GET http://localhost:5049/api/admin/verifications?PageNumber=0&PageSize=-1

**Response Status:** 400

**Response Body:**
`json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "PageSize":  [
                                    "\u0027Page Size\u0027 must be between 1 and 50. You entered -1."
                                ],
                   "PageNumber":  [
                                      "\u0027Page Number\u0027 must be greater than or equal to \u00271\u0027."
                                  ]
               },
    "traceId":  "00-aa36cc6db96e522b40beabc9d5d7a288-e261399bac9c871c-00"
}
``n---


### 11. Read - GET list missing token

**Request:** GET http://localhost:5049/api/admin/verifications?PageNumber=1&PageSize=10

**Response Status:** 401

**Response Body:** (Empty)
---


### 12. Read - GET details malformed Guid

**Request:** GET http://localhost:5049/api/admin/verifications/not-a-guid

**Response Status:** 404

**Response Body:** (Empty)
---


### 13. Read - GET details non-existent LawyerId

**Request:** GET http://localhost:5049/api/admin/verifications/00261808-5515-4ba5-829a-b81d98189261

**Response Status:** 404

**Response Body:**
`json
{
    "success":  false,
    "data":  null,
    "message":  "Lawyer was not found.",
    "errors":  null,
    "statusCode":  404
}
``n---


### 14a. Setup - Register Client

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
    "Email":  "client_192134597@test.com",
    "FullName":  "Client Verification",
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
                 "userId":  "adb48d0c-6e45-4869-e47b-08def300e686",
                 "email":  "client_192134597@test.com",
                 "fullName":  "Client Verification",
                 "role":  "Client"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
``n---


### 14b. Read - GET details for Client user

**Request:** GET http://localhost:5049/api/admin/verifications/adb48d0c-6e45-4869-e47b-08def300e686

**Response Status:** 404

**Response Body:**
`json
{
    "success":  false,
    "data":  null,
    "message":  "Lawyer was not found.",
    "errors":  null,
    "statusCode":  404
}
``n---


### 15. Read - GET content non-existent DocumentId

**Request:** GET http://localhost:5049/api/admin/verifications/documents/209a19d1-441d-4ebe-8967-55838f44ccdf/content

**Response Status:** 404

**Response Body:**
`json
{
    "success":  false,
    "data":  null,
    "message":  "Verification document was not found.",
    "errors":  null,
    "statusCode":  404
}
``n---


### 16. Update - PATCH Reject without Reason

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/424c0eba-a235-43ce-f760-08def3010d2a

**Body:**
`json
{
    "Decision":  2,
    "RejectionReason":  ""
}
``n
**Response Status:** 400

**Response Body:**
`json
{
    "success":  false,
    "data":  null,
    "message":  null,
    "errors":  [
                   "\u0027Rejection Reason\u0027 must not be empty."
               ],
    "statusCode":  400
}
``n---


### 17. Update - PATCH Approve with Reason

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/424c0eba-a235-43ce-f760-08def3010d2a

**Body:**
`json
{
    "Decision":  1,
    "RejectionReason":  "This should fail because you can\u0027t have a reason for approve"
}
``n
**Response Status:** 400

**Response Body:**
`json
{
    "success":  false,
    "data":  null,
    "message":  null,
    "errors":  [
                   "A rejection reason can only be supplied when rejecting a document."
               ],
    "statusCode":  400
}
``n---


### 18. Update - PATCH invalid Decision enum

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/424c0eba-a235-43ce-f760-08def3010d2a

**Body:**
`json
{
    "Decision":  99,
    "RejectionReason":  null
}
``n
**Response Status:** 400

**Response Body:**
`json
{
    "success":  false,
    "data":  null,
    "message":  null,
    "errors":  [
                   "\u0027Decision\u0027 has a range of values which does not include \u002799\u0027."
               ],
    "statusCode":  400
}
``n---


### 19. Update - PATCH non-existent DocumentId

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/5cacca8b-ccb3-4a15-868d-0290dd7f3129

**Body:**
`json
{
    "Decision":  1,
    "RejectionReason":  null
}
``n
**Response Status:** 404

**Response Body:**
`json
{
    "success":  false,
    "data":  null,
    "message":  "Verification document was not found.",
    "errors":  null,
    "statusCode":  404
}
``n---



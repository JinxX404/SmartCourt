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
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0MmI3YzZiOS04NjkwLTQ1NmMtNzE2YS0wOGRlZTMxNDk3ZmUiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjQyYjdjNmI5LTg2OTAtNDU2Yy03MTZhLTA4ZGVlMzE0OTdmZSIsImVtYWlsIjoibW9hdGF6bW9oYW1tZWQyMzkyMDAzQGdtYWlsLmNvbSIsIm5hbWUiOiJNb2F0YXogTW9oYW1tZWQiLCJzZWN1cml0eV9zdGFtcCI6IlFWMkhOTEtXUENLUVNQMlE1VTJPQkNBVTRYVFdYUUJTIiwianRpIjoiOTVkNzBkNTUtMjNiYi00N2RmLTg1YzUtY2IxM2Q5NmU2Y2UyIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJuYmYiOjE3ODU5NDYzOTcsImV4cCI6MTc4NTk0OTk5NywiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.bPqrIdQ9YrpkjRKTwzsAEEQwP7YoUpWxKVH6qZKEMps",
                 "expiresIn":  3600,
                 "refreshToken":  "FK2qqNEqwzCcvNuHDB9iXppAq0B5WELpgPu3CUCxZhQsypPTVRSd8R6lJV4tEAUJ1kzufjpT+h5EU3eZfmuDYQ==",
                 "refreshTokenExpiration":  "2026-08-12T16:13:17.329293Z"
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
    "Email":  "lawyer_verification_93095493@test.com",
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
                 "userId":  "6673da4c-b515-4fdf-b0fb-08def30c75ec",
                 "email":  "lawyer_verification_93095493@test.com",
                 "fullName":  "Lawyer Verification",
                 "role":  "Lawyer"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
``n---


Found confirmation URL for lawyer_verification_93095493@test.com: http://localhost:5173/verify-email?userId=6673da4c-b515-4fdf-b0fb-08def30c75ec&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5b24vNTBoeWRYak1ZVUIxeDVQVzBRTGpDT2JmbURTWFJxVFVrSnFoclNGRFRvN0hEdFZuNEI1ZlNlNTdzODZ2Q2kwMHN2TG1IYjdzU0o4YTJmTjFEbDRrbitsNkRxUVh4ZEs1MmZYUGRxVG9zL2dPcWw1cEtRa1F0NFdRd1hoNkhOaVV4bWV0S2IySVFJYnljSjFJYWdzcVgySUl5VVhTVVczZi8zVmFIR2oxSUVONmFWRGlJcXJlOU1SYStJYUMzUEVwOW84WTUvQmEvcVJ4Z3lGTldrQ0ZVVEpSMFRkMHNWQW95U3VRdm5hUT09

### Confirm Email for lawyer_verification_93095493@test.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=6673da4c-b515-4fdf-b0fb-08def30c75ec&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5b24vNTBoeWRYak1ZVUIxeDVQVzBRTGpDT2JmbURTWFJxVFVrSnFoclNGRFRvN0hEdFZuNEI1ZlNlNTdzODZ2Q2kwMHN2TG1IYjdzU0o4YTJmTjFEbDRrbitsNkRxUVh4ZEs1MmZYUGRxVG9zL2dPcWw1cEtRa1F0NFdRd1hoNkhOaVV4bWV0S2IySVFJYnljSjFJYWdzcVgySUl5VVhTVVczZi8zVmFIR2oxSUVONmFWRGlJcXJlOU1SYStJYUMzUEVwOW84WTUvQmEvcVJ4Z3lGTldrQ0ZVVEpSMFRkMHNWQW95U3VRdm5hUT09

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
    "Email":  "lawyer_verification_93095493@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "6673da4c-b515-4fdf-b0fb-08def30c75ec",
                              "email":  "lawyer_verification_93095493@test.com",
                              "fullName":  "Lawyer Verification",
                              "role":  "Lawyer"
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI2NjczZGE0Yy1iNTE1LTRmZGYtYjBmYi0wOGRlZjMwYzc1ZWMiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjY2NzNkYTRjLWI1MTUtNGZkZi1iMGZiLTA4ZGVmMzBjNzVlYyIsImVtYWlsIjoibGF3eWVyX3ZlcmlmaWNhdGlvbl85MzA5NTQ5M0B0ZXN0LmNvbSIsIm5hbWUiOiJMYXd5ZXIgVmVyaWZpY2F0aW9uIiwic2VjdXJpdHlfc3RhbXAiOiJON1FPQ1dSWDVIUEs1UUhTVVBXNFlTQk4zNERFNEI1VSIsImp0aSI6IjgyOTYzNWEzLWJiNjUtNGY3OC04YWYwLTdhMWJlNWIwYzQwZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6Ikxhd3llciIsIm5iZiI6MTc4NTk0NjQwMywiZXhwIjoxNzg1OTUwMDAzLCJpc3MiOiJTbWFydENvdXJ0QVBJIiwiYXVkIjoiU21hcnRDb3VydENsaWVudCJ9.UsfzERq9jLV5qfNtdX7KWimHFawMxi2fgv95Q2RYv1A",
                 "expiresIn":  3600,
                 "refreshToken":  "yk2fuPG9G7xvL85+BqGfBvUTLU9fPtgf0wWuMWwTn6RKXu+JUmhO2L+JsqcbzOYb3MsFNLrQPXUJ6qQANftm7g==",
                 "refreshTokenExpiration":  "2026-08-12T16:13:23.2670097Z"
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
- UserId = 6673da4c-b515-4fdf-b0fb-08def30c75ec
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
    "totalRecords":  5,
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
                     "lawyerId":  "88404714-5076-4ed1-e479-08def300e686",
                     "fullName":  "Lawyer Verification",
                     "email":  "lawyer_verification_730984203@test.com",
                     "phoneNumber":  null,
                     "pendingDocumentCount":  2,
                     "verifiedDocumentCount":  0,
                     "rejectedDocumentCount":  0
                 },
                 {
                     "lawyerId":  "6673da4c-b515-4fdf-b0fb-08def30c75ec",
                     "fullName":  "Lawyer Verification",
                     "email":  "lawyer_verification_93095493@test.com",
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

**Request:** GET http://localhost:5049/api/admin/verifications/6673da4c-b515-4fdf-b0fb-08def30c75ec

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "lawyerId":  "6673da4c-b515-4fdf-b0fb-08def30c75ec",
                 "fullName":  "Lawyer Verification",
                 "email":  "lawyer_verification_93095493@test.com",
                 "phoneNumber":  null,
                 "accountStatus":  "PendingReview",
                 "isFullyVerified":  false,
                 "documents":  [
                                   {
                                       "documentId":  "b06b6d29-3a42-44ff-e66e-08def30c7c70",
                                       "documentType":  "NationalIdFront",
                                       "status":  "Pending",
                                       "fileName":  "dummy_admin_id.jpg",
                                       "contentType":  "image/jpeg",
                                       "expirationDate":  "2030-01-01",
                                       "reviewedAt":  null,
                                       "rejectionReason":  null,
                                       "contentUrl":  "/api/admin/verifications/documents/b06b6d29-3a42-44ff-e66e-08def30c7c70/content"
                                   }
                               ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 3. Get Document Content (Admin)

**Request:** GET http://localhost:5049/api/admin/verifications/documents/b06b6d29-3a42-44ff-e66e-08def30c7c70/content

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "downloadUrl":  "https://msahvjipdwvgdartpeqj.supabase.co/storage/v1/object/public/smart-court-files/6673da4c-b515-4fdf-b0fb-08def30c75ec/national-id/b72c0189-99e5-4bef-846c-c760d9e32aae.jpg",
                 "contentType":  "image/jpeg",
                 "fileName":  "dummy_admin_id.jpg"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 4. Review Verification Document - Reject

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/b06b6d29-3a42-44ff-e66e-08def30c7c70

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
                 "documentId":  "b06b6d29-3a42-44ff-e66e-08def30c7c70",
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
- UserId = 6673da4c-b515-4fdf-b0fb-08def30c75ec
- Documents[0].ExpirationDate = 2030-01-01
- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\dummy_admin_id.jpg]


**Response Status:** 200

**Response Body:**
```json
{"success":true,"data":{"uploadedDocuments":[{"fileName":"dummy_admin_id.jpg","type":2}],"failedDocuments":[]},"message":null,"errors":null,"statusCode":200}
```n---


### 5b. Setup - Get Verification Details Again

**Request:** GET http://localhost:5049/api/admin/verifications/6673da4c-b515-4fdf-b0fb-08def30c75ec

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "lawyerId":  "6673da4c-b515-4fdf-b0fb-08def30c75ec",
                 "fullName":  "Lawyer Verification",
                 "email":  "lawyer_verification_93095493@test.com",
                 "phoneNumber":  null,
                 "accountStatus":  "Rejected",
                 "isFullyVerified":  false,
                 "documents":  [
                                   {
                                       "documentId":  "b06b6d29-3a42-44ff-e66e-08def30c7c70",
                                       "documentType":  "NationalIdFront",
                                       "status":  "Rejected",
                                       "fileName":  "dummy_admin_id.jpg",
                                       "contentType":  "image/jpeg",
                                       "expirationDate":  "2030-01-01",
                                       "reviewedAt":  null,
                                       "rejectionReason":  "Image is too blurry.",
                                       "contentUrl":  "/api/admin/verifications/documents/b06b6d29-3a42-44ff-e66e-08def30c7c70/content"
                                   },
                                   {
                                       "documentId":  "5b9674d6-68f2-4e20-e66f-08def30c7c70",
                                       "documentType":  "NationalIdBack",
                                       "status":  "Pending",
                                       "fileName":  "dummy_admin_id.jpg",
                                       "contentType":  "image/jpeg",
                                       "expirationDate":  "2030-01-01",
                                       "reviewedAt":  null,
                                       "rejectionReason":  null,
                                       "contentUrl":  "/api/admin/verifications/documents/5b9674d6-68f2-4e20-e66f-08def30c7c70/content"
                                   }
                               ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 6. Review Verification Document - Approve

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/5b9674d6-68f2-4e20-e66f-08def30c7c70

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
                 "documentId":  "5b9674d6-68f2-4e20-e66f-08def30c7c70",
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

**Request:** DELETE http://localhost:5049/api/UserVerification?UserId=6673da4c-b515-4fdf-b0fb-08def30c75ec&DocumentId=b06b6d29-3a42-44ff-e66e-08def30c7c70

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

**Request:** GET http://localhost:5049/api/UserVerification/6673da4c-b515-4fdf-b0fb-08def30c75ec

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "documents":  [
                                   {
                                       "documentId":  "0aedaf31-7a97-4661-3b51-08def30c7c6d",
                                       "documentType":  1,
                                       "status":  3,
                                       "expirationDate":  "2030-01-01",
                                       "isCurrent":  true,
                                       "fileName":  "dummy_admin_id.jpg"
                                   },
                                   {
                                       "documentId":  "e368b786-9e57-4d36-3b52-08def30c7c6d",
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
    "traceId":  "00-677c76c6fc0ad1d5759eb5b699e6b3a6-23f289f31688d128-00"
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

**Request:** GET http://localhost:5049/api/admin/verifications/f1dfd8c3-b756-449b-b3c4-52376d3ffbf5

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
    "Email":  "client_93095493@test.com",
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
                 "userId":  "73bd7346-9696-401c-b0fc-08def30c75ec",
                 "email":  "client_93095493@test.com",
                 "fullName":  "Client Verification",
                 "role":  "Client"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
``n---


### 14b. Read - GET details for Client user

**Request:** GET http://localhost:5049/api/admin/verifications/73bd7346-9696-401c-b0fc-08def30c75ec

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

**Request:** GET http://localhost:5049/api/admin/verifications/documents/263c04a1-1906-4bd3-baf2-c4891f9258ff/content

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

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/5b9674d6-68f2-4e20-e66f-08def30c7c70

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

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/5b9674d6-68f2-4e20-e66f-08def30c7c70

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

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/5b9674d6-68f2-4e20-e66f-08def30c7c70

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

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/1bab5084-c3d7-4c9a-84c7-1eaa5c97c68d

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



# User Verification Slice Test Report

### 0. Setup - Register Lawyer

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
`json
{
    "Email":  "lawyer_verification_324320347@test.com",
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
                 "userId":  "58cfbed4-4c92-4c32-b0fd-08def30c75ec",
                 "email":  "lawyer_verification_324320347@test.com",
                 "fullName":  "Lawyer Verification",
                 "role":  "Lawyer"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
``n---


Found confirmation URL for lawyer_verification_324320347@test.com: http://localhost:5173/verify-email?userId=58cfbed4-4c92-4c32-b0fd-08def30c75ec&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5K1NJcGdCMGgvdXFHWFBLanhvdmdTRkdDRklJQW1OZFBZM1hhNnd1Q2hMaTBDcjhnYkx2dGh3M2JVUWR1Q2l1U1FhUW5UVlpPaXpBWHNZUUpwa0ppVkhRSFp0Z1dsQlk1dXFDeFNlR2p5MXFYcVVhczlKNjBYdzM4TVZORVNETVk1M29MNG8yYnRUL1l2RFk1TFBpeURkQTlsQ0RINHhVMjlzL0dqaTVxMGFyNlBsZ3BmTHkrWDhsdEZ6c0ZUSlNkUDJncmlKMURjNHpENXVIZ3NRU2EyNlFNUUVNZ2MvT0dzMDVwKzdHU1JWdz09

### Confirm Email for lawyer_verification_324320347@test.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=58cfbed4-4c92-4c32-b0fd-08def30c75ec&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI5K1NJcGdCMGgvdXFHWFBLanhvdmdTRkdDRklJQW1OZFBZM1hhNnd1Q2hMaTBDcjhnYkx2dGh3M2JVUWR1Q2l1U1FhUW5UVlpPaXpBWHNZUUpwa0ppVkhRSFp0Z1dsQlk1dXFDeFNlR2p5MXFYcVVhczlKNjBYdzM4TVZORVNETVk1M29MNG8yYnRUL1l2RFk1TFBpeURkQTlsQ0RINHhVMjlzL0dqaTVxMGFyNlBsZ3BmTHkrWDhsdEZ6c0ZUSlNkUDJncmlKMURjNHpENXVIZ3NRU2EyNlFNUUVNZ2MvT0dzMDVwKzdHU1JWdz09

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


### 0. Setup - Login

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Password123!",
    "Email":  "lawyer_verification_324320347@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "58cfbed4-4c92-4c32-b0fd-08def30c75ec",
                              "email":  "lawyer_verification_324320347@test.com",
                              "fullName":  "Lawyer Verification",
                              "role":  "Lawyer"
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI1OGNmYmVkNC00YzkyLTRjMzItYjBmZC0wOGRlZjMwYzc1ZWMiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjU4Y2ZiZWQ0LTRjOTItNGMzMi1iMGZkLTA4ZGVmMzBjNzVlYyIsImVtYWlsIjoibGF3eWVyX3ZlcmlmaWNhdGlvbl8zMjQzMjAzNDdAdGVzdC5jb20iLCJuYW1lIjoiTGF3eWVyIFZlcmlmaWNhdGlvbiIsInNlY3VyaXR5X3N0YW1wIjoiTVhDSUJZQjZFRUVDNVNIU0FUTUFFQU00WDZSRkZJVkgiLCJqdGkiOiIyZjRlNWE3My1mZjhlLTRkN2EtOGQzMC1jMTg2MDVlMTllMDciLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODU5NDY0NjQsImV4cCI6MTc4NTk1MDA2NCwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.Cd0lhrbmK-jnA_5Y6NBbILrMOEXyAjR5r164C6BclD4",
                 "expiresIn":  3600,
                 "refreshToken":  "mfYTW8YEIhfaf+DMQ83JvN5A0uwkt3gwoZXTc1OI5lx2FRI91EMDWRZy2gbd0cDrptKZKUsauWssz/v/4TKi4g==",
                 "refreshTokenExpiration":  "2026-08-12T16:14:24.7383229Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 1. Submit Verification Documents - Valid

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**
- Documents[0].Type = 1
- UserId = 58cfbed4-4c92-4c32-b0fd-08def30c75ec
- Documents[0].ExpirationDate = 2030-01-01
- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\dummy_id.jpg]


**Response Status:** 200

**Response Body:**
`json
{"success":true,"data":{"uploadedDocuments":[{"fileName":"dummy_id.jpg","type":1}],"failedDocuments":[]},"message":null,"errors":null,"statusCode":200}
``n---


### 2. Submit Verification - Missing UserId (400)

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**
- Documents[0].ExpirationDate = 2030-01-01
- Documents[0].Type = 2
- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\dummy_id.jpg]


**Response Status:** 400

**Response Body:**
`json
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"UserId":["UserId is required"]},"traceId":"00-ffe6667d1c741b0c9133f7504dfbb660-549090e0893d8a4a-00"}
``n---


### 3a. Submit Verification - Empty Documents (400)

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents

**Body:**
`json
{
    "UserId":  "58cfbed4-4c92-4c32-b0fd-08def30c75ec",
    "Documents":  [

                  ]
}
``n
**Response Status:** 500

**Response Body:**
`json
{
    "success":  false,
    "data":  null,
    "message":  "An internal server error occurred.",
    "errors":  null,
    "statusCode":  500
}
``n---


### 3b. Submit Verification - Invalid Type (400)

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**
- Documents[0].Type = 999
- UserId = 58cfbed4-4c92-4c32-b0fd-08def30c75ec
- Documents[0].ExpirationDate = 2030-01-01
- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\dummy_id.jpg]


**Response Status:** 400

**Response Body:**
`json
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"Documents[0].Type":["The value '999' is not valid for Type."]},"traceId":"00-6cc0dfd3d795106e9b1926a09c7261bf-14f107eb3b9984d7-00"}
``n---


### 3c. Submit Verification - Malicious Payload (400)

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**
- Documents[0].Type = 1
- UserId = 1' OR '1'='1
- Documents[0].ExpirationDate = 2030-01-01
- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\dummy_id.jpg]


**Response Status:** 400

**Response Body:**
`json
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"UserId":["The value '1' OR '1'='1' is not valid for UserId.","UserId is required"]},"traceId":"00-9e55c5cf34545261feaba1973b4af175-9d5c3aa036315785-00"}
``n---


### 4. Submit Verification - No Token (401)

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**
- Documents[0].Type = 1
- UserId = 58cfbed4-4c92-4c32-b0fd-08def30c75ec
- Documents[0].ExpirationDate = 2030-01-01
- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\dummy_id.jpg]


**Response Status:** 200

**Response Body:**
`json
{"success":true,"data":{"uploadedDocuments":[],"failedDocuments":[{"fileName":"dummy_id.jpg","type":1,"error":"You already uploaded this document before. Wait untill admin verifies your document"}]},"message":null,"errors":null,"statusCode":200}
``n---


### 5. Get User Documents - Valid

**Request:** GET http://localhost:5049/api/UserVerification/58cfbed4-4c92-4c32-b0fd-08def30c75ec

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "documents":  [
                                   {
                                       "documentId":  "d8baf648-842e-4fee-3b53-08def30c7c6d",
                                       "documentType":  1,
                                       "status":  1,
                                       "expirationDate":  "2030-01-01",
                                       "isCurrent":  true,
                                       "fileName":  "dummy_id.jpg"
                                   }
                               ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 6. Get User Documents - Invalid UserId

**Request:** GET http://localhost:5049/api/UserVerification/00000000-0000-0000-0000-000000000000

**Response Status:** 400

**Response Body:**
`json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "UserId":  [
                                  "User Id is required."
                              ]
               },
    "traceId":  "00-0cb22148ec5ddb3c62c6740361a1385e-f7253653a0832776-00"
}
``n---


### 7. Get User Documents - Malicious UserId

**Request:** GET http://localhost:5049/api/UserVerification/DROP_TABLE_USERS

**Response Status:** 400

**Response Body:**
`json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "UserId":  [
                                  "The value \u0027DROP_TABLE_USERS\u0027 is not valid for UserId.",
                                  "User Id is required."
                              ]
               },
    "traceId":  "00-4928f045bcc95482902a887ce2423e98-f36d3c134e939602-00"
}
``n---


### 8. Get User Documents - No Token (401)

**Request:** GET http://localhost:5049/api/UserVerification/58cfbed4-4c92-4c32-b0fd-08def30c75ec

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "documents":  [
                                   {
                                       "documentId":  "d8baf648-842e-4fee-3b53-08def30c7c6d",
                                       "documentType":  1,
                                       "status":  1,
                                       "expirationDate":  "2030-01-01",
                                       "isCurrent":  true,
                                       "fileName":  "dummy_id.jpg"
                                   }
                               ]
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 9. Delete User Document - Missing DocumentId

**Request:** DELETE http://localhost:5049/api/UserVerification?UserId=58cfbed4-4c92-4c32-b0fd-08def30c75ec

**Response Status:** 400

**Response Body:**
`json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "DocumentId":  [
                                      "Document Id is required."
                                  ]
               },
    "traceId":  "00-2f3d53209f61f02d9869f2e6630bb0b9-fb1fb05eaaef7876-00"
}
``n---


### 10. Delete User Document - Missing UserId

**Request:** DELETE http://localhost:5049/api/UserVerification?DocumentId=d8baf648-842e-4fee-3b53-08def30c7c6d

**Response Status:** 400

**Response Body:**
`json
{
    "type":  "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title":  "One or more validation errors occurred.",
    "status":  400,
    "errors":  {
                   "UserId":  [
                                  "User Id is required."
                              ]
               },
    "traceId":  "00-05af73fdc9e3476e9535e1240da23495-3d932be7d33ed8a9-00"
}
``n---


### 11. Delete User Document - No Token (401)

**Request:** DELETE http://localhost:5049/api/UserVerification?UserId=58cfbed4-4c92-4c32-b0fd-08def30c75ec&DocumentId=d8baf648-842e-4fee-3b53-08def30c7c6d

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 12. Delete User Document - Valid

**Request:** DELETE http://localhost:5049/api/UserVerification?UserId=58cfbed4-4c92-4c32-b0fd-08def30c75ec&DocumentId=d8baf648-842e-4fee-3b53-08def30c7c6d

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


### 13. Delete User Document - Not Found (Already deleted)

**Request:** DELETE http://localhost:5049/api/UserVerification?UserId=58cfbed4-4c92-4c32-b0fd-08def30c75ec&DocumentId=d8baf648-842e-4fee-3b53-08def30c7c6d

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


### 14. Submit Verification - Large File (3MB)

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**
- Documents[0].Type = 1
- UserId = 58cfbed4-4c92-4c32-b0fd-08def30c75ec
- Documents[0].ExpirationDate = 2030-01-01
- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\large_id.jpg]


**Response Status:** 200

**Response Body:**
`json
{"success":true,"data":{"uploadedDocuments":[{"fileName":"large_id.jpg","type":1}],"failedDocuments":[]},"message":null,"errors":null,"statusCode":200}
``n---


### 15. Submit Verification - Invalid File Extension (.exe)

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**
- Documents[0].Type = 1
- UserId = 58cfbed4-4c92-4c32-b0fd-08def30c75ec
- Documents[0].ExpirationDate = 2030-01-01
- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\malicious.exe]


**Response Status:** 200

**Response Body:**
`json
{"success":true,"data":{"uploadedDocuments":[],"failedDocuments":[{"fileName":"malicious.exe","type":1,"error":"You already uploaded this document before. Wait untill admin verifies your document"}]},"message":null,"errors":null,"statusCode":200}
``n---


### 16. Submit Verification - Extremely Long UserId

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**
- Documents[0].Type = 1
- UserId = aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
- Documents[0].ExpirationDate = 2030-01-01
- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\dummy_id.jpg]


**Response Status:** 400

**Response Body:**
`json
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"UserId":["The value 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa' is not valid for UserId.","UserId is required"]},"traceId":"00-36dcfad7efc3ff1a38690b3fb6e4d41a-f74d65fcd58e63e5-00"}
``n---


### 17. Submit Verification - Past Expiration Date

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**
- Documents[0].Type = 1
- UserId = 58cfbed4-4c92-4c32-b0fd-08def30c75ec
- Documents[0].ExpirationDate = 1800-01-01
- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\dummy_id.jpg]


**Response Status:** 200

**Response Body:**
`json
{"success":true,"data":{"uploadedDocuments":[],"failedDocuments":[{"fileName":"dummy_id.jpg","type":1,"error":"This document is expired"}]},"message":null,"errors":null,"statusCode":200}
``n---


### 18a. Setup - Register Attacker

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
`json
{
    "Email":  "attacker_324320347@test.com",
    "FullName":  "Attacker User",
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
                 "userId":  "bdd03951-73f7-4490-b0fe-08def30c75ec",
                 "email":  "attacker_324320347@test.com",
                 "fullName":  "Attacker User",
                 "role":  "Lawyer"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
``n---


Found confirmation URL for attacker_324320347@test.com: http://localhost:5173/verify-email?userId=bdd03951-73f7-4490-b0fe-08def30c75ec&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIrMlo4amVSZDVtRFFKdEpzZlUyRGVkK2Z3RWR5T3AxZmRtbG9mREFPS0VETC9Oek80SHdTMXAzZWhHaEZ0Uzd4c1lPUVhGWmJ1amZ5NTAzcUtHRnFKbnY0NDNxMnBIMzNZOTdqZ0NzWVJEaHYxV3E1QTZ3TmZGUGVFUWh1T2Nlb3lRWE1JYm0xNC9yQnZXSkZNTTB6azJzTCtMaE9hcFE4SmxwZzM3VVljeE9qbU5TUmhrVXlaa0Fzakg5TUZ3SGZwVWVHWEg3NEFjN0tFVTVlWTVmOXgxdHV3dVRJT1F0ZWJhb2dXU0lDeW5IUT09

### Confirm Email for attacker_324320347@test.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=bdd03951-73f7-4490-b0fe-08def30c75ec&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIrMlo4amVSZDVtRFFKdEpzZlUyRGVkK2Z3RWR5T3AxZmRtbG9mREFPS0VETC9Oek80SHdTMXAzZWhHaEZ0Uzd4c1lPUVhGWmJ1amZ5NTAzcUtHRnFKbnY0NDNxMnBIMzNZOTdqZ0NzWVJEaHYxV3E1QTZ3TmZGUGVFUWh1T2Nlb3lRWE1JYm0xNC9yQnZXSkZNTTB6azJzTCtMaE9hcFE4SmxwZzM3VVljeE9qbU5TUmhrVXlaa0Fzakg5TUZ3SGZwVWVHWEg3NEFjN0tFVTVlWTVmOXgxdHV3dVRJT1F0ZWJhb2dXU0lDeW5IUT09

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


### 18b. Setup - Login Attacker

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
    "Password":  "Password123!",
    "Email":  "attacker_324320347@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "bdd03951-73f7-4490-b0fe-08def30c75ec",
                              "email":  "attacker_324320347@test.com",
                              "fullName":  "Attacker User",
                              "role":  "Lawyer"
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJiZGQwMzk1MS03M2Y3LTQ0OTAtYjBmZS0wOGRlZjMwYzc1ZWMiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImJkZDAzOTUxLTczZjctNDQ5MC1iMGZlLTA4ZGVmMzBjNzVlYyIsImVtYWlsIjoiYXR0YWNrZXJfMzI0MzIwMzQ3QHRlc3QuY29tIiwibmFtZSI6IkF0dGFja2VyIFVzZXIiLCJzZWN1cml0eV9zdGFtcCI6IjM3WFFHSTZWVkxHSEc0TkRTUExGU0FXWUc0TkFNVkE2IiwianRpIjoiYWE3OGM0MTgtYzNlOC00ZmJiLWEwNjgtYTQ1MmI4NDBlNTAyIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiTGF3eWVyIiwibmJmIjoxNzg1OTQ2NTY0LCJleHAiOjE3ODU5NTAxNjQsImlzcyI6IlNtYXJ0Q291cnRBUEkiLCJhdWQiOiJTbWFydENvdXJ0Q2xpZW50In0.UmcetHIA8fGlmmoNwPpwZ3hi92K396UeLm9kcGPU-4E",
                 "expiresIn":  3600,
                 "refreshToken":  "57cxkfKxYo7oQpx4g1TDMukrezDx03U+EguQcoyEC+Rn4nFKqiOyJ9w1SRU40MFxnl4cojUJwvaS2l8ceh6azw==",
                 "refreshTokenExpiration":  "2026-08-12T16:16:04.2191455Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 18c. Delete User Document - Cross-User (Attacker Token)

**Request:** DELETE http://localhost:5049/api/UserVerification?UserId=58cfbed4-4c92-4c32-b0fd-08def30c75ec&DocumentId=d8baf648-842e-4fee-3b53-08def30c7c6d

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


### 19. Submit Verification - Invalid Date Format

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**
- Documents[0].Type = 1
- UserId = 58cfbed4-4c92-4c32-b0fd-08def30c75ec
- Documents[0].ExpirationDate = 13-13-2030
- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\dummy_id.jpg]


**Response Status:** 400

**Response Body:**
`json
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"Documents[0].ExpirationDate":["The value '13-13-2030' is not valid for ExpirationDate."]},"traceId":"00-491aace6585675cd83ea74365b981d7a-9aadee62ebdcae0a-00"}
``n---


### 20. Validation - Submit Missing UserId

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**
- Documents[0].ExpirationDate = 2030-01-01
- Documents[0].Type = 1
- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\dummy_id.jpg]


**Response Status:** 400

**Response Body:**
`json
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"UserId":["UserId is required"]},"traceId":"00-1d6757fbfe5e152c6d844901c4a00dfa-2c7514c2d42a958e-00"}
``n---


### 21. Validation - Submit Empty Documents

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**
- UserId = 58cfbed4-4c92-4c32-b0fd-08def30c75ec


**Response Status:** 500

**Response Body:**
`json
{"success":false,"data":null,"message":"An internal server error occurred.","errors":null,"statusCode":500}
``n---


### 22. Validation - Submit Malformed UserId

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**
- Documents[0].Type = 1
- UserId = not-a-guid
- Documents[0].ExpirationDate = 2030-01-01
- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\dummy_id.jpg]


**Response Status:** 400

**Response Body:**
`json
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"UserId":["The value 'not-a-guid' is not valid for UserId.","UserId is required"]},"traceId":"00-4b10421344e1b8ce28b7cb8df6964db1-9a2109c7a52e3cf9-00"}
``n---


### 23. Validation - Submit Duplicate DocumentTypes

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents

**Body:**
`json
{

}
``n
**Response Status:** 500

**Response Body:**
`json
{
    "success":  false,
    "data":  null,
    "message":  "An internal server error occurred.",
    "errors":  null,
    "statusCode":  500
}
``n---


### 23. Validation - Submit Duplicate DocumentTypes

**Response Status:** 400

**Response Body:**
```json
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"Documents":["The same verification document cannot be submitted more than once."]},"traceId":"00-0aa08dcaf57442af83efc02011c5e235-1add14490f4bef8c-00"}
```n---


### 24. Stress - 35MB file stress test

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**
- Documents[0].Type = 1
- UserId = 58cfbed4-4c92-4c32-b0fd-08def30c75ec
- Documents[0].ExpirationDate = 2030-01-01
- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\giant_id.jpg]


**Response Status:** 400

**Response Body:**
`json
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"":["Failed to read the request form. Request body too large. The max request body size is 30000000 bytes."]},"traceId":"00-18f071b89ebb9efde14243ab6d2e382c-54dc57f4c740a54b-00"}
``n---


**Response Status:** Error

**Response Body:**
Cannot process argument because the value of argument "name" is not valid. Change the value of the "name" argument and run the operation again.
---


### 25a. HTTP Method - POST to GET endpoint

**Request:** POST http://localhost:5049/api/UserVerification/58cfbed4-4c92-4c32-b0fd-08def30c75ec

**Body:**
`json
{

}
``n
**Response Status:** 405

**Response Body:** (Empty)
---


### 25b. HTTP Method - GET to DELETE endpoint

**Request:** GET http://localhost:5049/api/UserVerification?UserId=58cfbed4-4c92-4c32-b0fd-08def30c75ec&DocumentId=82be6c38-df10-4e4d-abd8-9ab43da08ae1

**Response Status:** 405

**Response Body:** (Empty)
---


### 26. Read - Get documents for non-existent UserId

**Request:** GET http://localhost:5049/api/UserVerification/2bb76308-6a47-4a48-bf03-74758f4afda8

**Response Status:** 404

**Response Body:**
`json
{
    "success":  false,
    "data":  null,
    "message":  null,
    "errors":  [
                   "The specified user does not exist."
               ],
    "statusCode":  404
}
``n---



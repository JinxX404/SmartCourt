# User Verification Slice Test Report

### 0. Setup - Register Lawyer

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
`json
{
    "Email":  "lawyer_verification_730984203@test.com",
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
                 "userId":  "88404714-5076-4ed1-e479-08def300e686",
                 "email":  "lawyer_verification_730984203@test.com",
                 "fullName":  "Lawyer Verification",
                 "role":  "Lawyer"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
``n---


Found confirmation URL for lawyer_verification_730984203@test.com: http://localhost:5173/verify-email?userId=88404714-5076-4ed1-e479-08def300e686&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIrZDZvcFRNaHQybC80SXZrdm45SE1hdjN6VmpvYWl2QXhHemZTb0ZNNjdpdjVtZnhmZWJ2bk8wWldyUm8wZWxBQUtIU1hSWlZ5L3NHRUxURS83VFR6UXZxRll3akQzdVdKWFNqMDdENGx5TmlZSHFkVm50MVJuRU1hMytEKy9GREpQV1BsQnh2K2RUN1FGZFVNOFBURTVtTlljZFVONHRjdEpaMkEyQ0NtdVkvZG01bE8zeElXdFd0enRmcFUwc25HK2Y1RmNDZFI1Ni9DYnp0UGczWGs4S0pCR3BVWmVuRlRzK1BZNFlDSThydz09

### Confirm Email for lawyer_verification_730984203@test.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=88404714-5076-4ed1-e479-08def300e686&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIrZDZvcFRNaHQybC80SXZrdm45SE1hdjN6VmpvYWl2QXhHemZTb0ZNNjdpdjVtZnhmZWJ2bk8wWldyUm8wZWxBQUtIU1hSWlZ5L3NHRUxURS83VFR6UXZxRll3akQzdVdKWFNqMDdENGx5TmlZSHFkVm50MVJuRU1hMytEKy9GREpQV1BsQnh2K2RUN1FGZFVNOFBURTVtTlljZFVONHRjdEpaMkEyQ0NtdVkvZG01bE8zeElXdFd0enRmcFUwc25HK2Y1RmNDZFI1Ni9DYnp0UGczWGs4S0pCR3BVWmVuRlRzK1BZNFlDSThydz09

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
    "Email":  "lawyer_verification_730984203@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "88404714-5076-4ed1-e479-08def300e686",
                              "email":  "lawyer_verification_730984203@test.com",
                              "fullName":  "Lawyer Verification",
                              "role":  "Lawyer"
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI4ODQwNDcxNC01MDc2LTRlZDEtZTQ3OS0wOGRlZjMwMGU2ODYiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6Ijg4NDA0NzE0LTUwNzYtNGVkMS1lNDc5LTA4ZGVmMzAwZTY4NiIsImVtYWlsIjoibGF3eWVyX3ZlcmlmaWNhdGlvbl83MzA5ODQyMDNAdGVzdC5jb20iLCJuYW1lIjoiTGF3eWVyIFZlcmlmaWNhdGlvbiIsInNlY3VyaXR5X3N0YW1wIjoiSDdBUEtRTDVNUkdPRFdWQzREMlJQSk9UUEI2TFdFTlAiLCJqdGkiOiI4NjlhYzhhZS02ZjBmLTRmZGEtOWI3NC03NTBmY2JjNGE5NGQiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODU5NDQ1OTAsImV4cCI6MTc4NTk0ODE5MCwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.fkHT0Gh8X2jVgNYEfNJ2irGUdO2UyZbgvwRnzBSV3RE",
                 "expiresIn":  3600,
                 "refreshToken":  "tM/V7Gj9Od21yncZtjY4vvPTTGFTejFhMlspNkXba1JR+P9FGt5pPbOuKj76neJZeB8U2YthiJAWxYkwZaw5oA==",
                 "refreshTokenExpiration":  "2026-08-12T15:43:10.9865375Z"
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
- UserId = 88404714-5076-4ed1-e479-08def300e686
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
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"UserId":["UserId is required"]},"traceId":"00-23d358be405232d05ad535f10629711e-383d028f684e5c74-00"}
``n---


### 3a. Submit Verification - Empty Documents (400)

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents

**Body:**
`json
{
    "UserId":  "88404714-5076-4ed1-e479-08def300e686",
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
- UserId = 88404714-5076-4ed1-e479-08def300e686
- Documents[0].ExpirationDate = 2030-01-01
- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\dummy_id.jpg]


**Response Status:** 400

**Response Body:**
`json
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"Documents[0].Type":["The value '999' is not valid for Type."]},"traceId":"00-a063556889425b3ddb922eebb562b12d-6b480774574edc75-00"}
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
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"UserId":["The value '1' OR '1'='1' is not valid for UserId.","UserId is required"]},"traceId":"00-a0bc2a908d63a3777d9abc772f347ccc-32f5eed717bf74f3-00"}
``n---


### 4. Submit Verification - No Token (401)

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**
- Documents[0].Type = 1
- UserId = 88404714-5076-4ed1-e479-08def300e686
- Documents[0].ExpirationDate = 2030-01-01
- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\dummy_id.jpg]


**Response Status:** 200

**Response Body:**
`json
{"success":true,"data":{"uploadedDocuments":[],"failedDocuments":[{"fileName":"dummy_id.jpg","type":1,"error":"You already uploaded this document before. Wait untill admin verifies your document"}]},"message":null,"errors":null,"statusCode":200}
``n---


### 5. Get User Documents - Valid

**Request:** GET http://localhost:5049/api/UserVerification/88404714-5076-4ed1-e479-08def300e686

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "documents":  [
                                   {
                                       "documentId":  "ee2b2ab7-eed8-4c72-c6db-08def3010d27",
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
    "traceId":  "00-ab5c168b47a3910f4a599581e5b76bfd-32a5bd8832d2d5a4-00"
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
    "traceId":  "00-cb58d3a3eaaf171c8bdb3234fcd14887-5744000e18e5701c-00"
}
``n---


### 8. Get User Documents - No Token (401)

**Request:** GET http://localhost:5049/api/UserVerification/88404714-5076-4ed1-e479-08def300e686

**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "documents":  [
                                   {
                                       "documentId":  "ee2b2ab7-eed8-4c72-c6db-08def3010d27",
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

**Request:** DELETE http://localhost:5049/api/UserVerification?UserId=88404714-5076-4ed1-e479-08def300e686

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
    "traceId":  "00-205d7f60e252aa031975a039dcfaecd9-9243fae1516d8bbc-00"
}
``n---


### 10. Delete User Document - Missing UserId

**Request:** DELETE http://localhost:5049/api/UserVerification?DocumentId=ee2b2ab7-eed8-4c72-c6db-08def3010d27

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
    "traceId":  "00-3d855985e4afc5697070ac33fa7baa82-4dffc200d750204d-00"
}
``n---


### 11. Delete User Document - No Token (401)

**Request:** DELETE http://localhost:5049/api/UserVerification?UserId=88404714-5076-4ed1-e479-08def300e686&DocumentId=ee2b2ab7-eed8-4c72-c6db-08def3010d27

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

**Request:** DELETE http://localhost:5049/api/UserVerification?UserId=88404714-5076-4ed1-e479-08def300e686&DocumentId=ee2b2ab7-eed8-4c72-c6db-08def3010d27

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

**Request:** DELETE http://localhost:5049/api/UserVerification?UserId=88404714-5076-4ed1-e479-08def300e686&DocumentId=ee2b2ab7-eed8-4c72-c6db-08def3010d27

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
- UserId = 88404714-5076-4ed1-e479-08def300e686
- Documents[0].ExpirationDate = 2030-01-01
- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\large_id.jpg]


**Response Status:** Error

**Response Body:**
Exception calling "GetResult" with "0" argument(s): "A task was canceled." - A task was canceled.
---


### 15. Submit Verification - Invalid File Extension (.exe)

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**
- Documents[0].Type = 1
- UserId = 88404714-5076-4ed1-e479-08def300e686
- Documents[0].ExpirationDate = 2030-01-01
- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\malicious.exe]


**Response Status:** 200

**Response Body:**
`json
{"success":true,"data":{"uploadedDocuments":[{"fileName":"malicious.exe","type":1}],"failedDocuments":[]},"message":null,"errors":null,"statusCode":200}
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
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"UserId":["The value 'aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa' is not valid for UserId.","UserId is required"]},"traceId":"00-ae848e58f76b0d1ce170d958524b7836-3a35f909e59f667b-00"}
``n---


### 17. Submit Verification - Past Expiration Date

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**
- Documents[0].Type = 1
- UserId = 88404714-5076-4ed1-e479-08def300e686
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
    "Email":  "attacker_730984203@test.com",
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
                 "userId":  "4de07fb8-c7a7-4b3d-e47c-08def300e686",
                 "email":  "attacker_730984203@test.com",
                 "fullName":  "Attacker User",
                 "role":  "Lawyer"
             },
    "message":  "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
    "errors":  null,
    "statusCode":  201
}
``n---


Found confirmation URL for attacker_730984203@test.com: http://localhost:5173/verify-email?userId=4de07fb8-c7a7-4b3d-e47c-08def300e686&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI4QUJ6VUhLa2tDN1Npdk85WDU4QXNkTFp0TDk0REhMbWFZU0kvOEY3eWlGYnRxcHNpNG1CZ3J1aThCSXFZVzEveHFPeXRWM201dldwZkRPMnFaeHNFS1VLSEFwSmxDNmhNRitRR0xJclN6TlZvc1JpdnBxQW40ZlVjUkUxMENMY2Z6Z25qOTlqTzhZOEZlVHhBUmxEajVYLzRVQlExcHZmbGhYOUYybEMyUGFVbGhUUVFhZmhYWEFBQisxQnBQRmRKajRMQ1dJTk93Q00zUnp1YmpkR0srNjVOdlJwb0lUTmhRdlIwcHl1dDdWdz09

### Confirm Email for attacker_730984203@test.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=4de07fb8-c7a7-4b3d-e47c-08def300e686&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI4QUJ6VUhLa2tDN1Npdk85WDU4QXNkTFp0TDk0REhMbWFZU0kvOEY3eWlGYnRxcHNpNG1CZ3J1aThCSXFZVzEveHFPeXRWM201dldwZkRPMnFaeHNFS1VLSEFwSmxDNmhNRitRR0xJclN6TlZvc1JpdnBxQW40ZlVjUkUxMENMY2Z6Z25qOTlqTzhZOEZlVHhBUmxEajVYLzRVQlExcHZmbGhYOUYybEMyUGFVbGhUUVFhZmhYWEFBQisxQnBQRmRKajRMQ1dJTk93Q00zUnp1YmpkR0srNjVOdlJwb0lUTmhRdlIwcHl1dDdWdz09

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
    "Email":  "attacker_730984203@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
    "success":  true,
    "data":  {
                 "user":  {
                              "id":  "4de07fb8-c7a7-4b3d-e47c-08def300e686",
                              "email":  "attacker_730984203@test.com",
                              "fullName":  "Attacker User",
                              "role":  "Lawyer"
                          },
                 "accessToken":  "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI0ZGUwN2ZiOC1jN2E3LTRiM2QtZTQ3Yy0wOGRlZjMwMGU2ODYiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjRkZTA3ZmI4LWM3YTctNGIzZC1lNDdjLTA4ZGVmMzAwZTY4NiIsImVtYWlsIjoiYXR0YWNrZXJfNzMwOTg0MjAzQHRlc3QuY29tIiwibmFtZSI6IkF0dGFja2VyIFVzZXIiLCJzZWN1cml0eV9zdGFtcCI6IlFJN0pBNlJWWjZZWFBGNUVWMkZIWEhIRkNCWVBJS0xCIiwianRpIjoiY2E2NTUzNGYtMzExOC00NzUxLWI5MjgtYWUxZThmMGUzMmMyIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiTGF3eWVyIiwibmJmIjoxNzg1OTQ0NzAzLCJleHAiOjE3ODU5NDgzMDMsImlzcyI6IlNtYXJ0Q291cnRBUEkiLCJhdWQiOiJTbWFydENvdXJ0Q2xpZW50In0.1A2L7gYhrwMtFlb1ED8wP-f9-mbGqn_lSpcxBGwd-fE",
                 "expiresIn":  3600,
                 "refreshToken":  "Krbn0DbghysiUuzi+r0131qyGKxV6T3HCIEEn9CI7rAmZc1MbvWGluktQsVA65LkJu1CRN0xjFmyRnLYIxNViA==",
                 "refreshTokenExpiration":  "2026-08-12T15:45:03.7839994Z"
             },
    "message":  null,
    "errors":  null,
    "statusCode":  200
}
``n---


### 18c. Delete User Document - Cross-User (Attacker Token)

**Request:** DELETE http://localhost:5049/api/UserVerification?UserId=88404714-5076-4ed1-e479-08def300e686&DocumentId=ee2b2ab7-eed8-4c72-c6db-08def3010d27

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
- UserId = 88404714-5076-4ed1-e479-08def300e686
- Documents[0].ExpirationDate = 13-13-2030
- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\dummy_id.jpg]


**Response Status:** 400

**Response Body:**
`json
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"Documents[0].ExpirationDate":["The value '13-13-2030' is not valid for ExpirationDate."]},"traceId":"00-41802e44f3ee6eb98b17481e7b28f204-e482b99539c48997-00"}
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
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"UserId":["UserId is required"]},"traceId":"00-f63a1feed03c6b3d4bfa1363d28f92bd-7052b5b713b6ce87-00"}
``n---


### 21. Validation - Submit Empty Documents

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**
- UserId = 88404714-5076-4ed1-e479-08def300e686


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
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"UserId":["The value 'not-a-guid' is not valid for UserId.","UserId is required"]},"traceId":"00-bf6c8dbd90c2f0288d72c1ad1f0dccd1-7b7af887ff33e3f5-00"}
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
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"Documents":["The same verification document cannot be submitted more than once."]},"traceId":"00-b19f1f42299a351ae96fe4cf1f20fae4-d614164a92754147-00"}
```n---


### 24. Stress - 35MB file stress test

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**
- Documents[0].Type = 1
- UserId = 88404714-5076-4ed1-e479-08def300e686
- Documents[0].ExpirationDate = 2030-01-01
- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\giant_id.jpg]


**Response Status:** 400

**Response Body:**
`json
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"":["Failed to read the request form. Request body too large. The max request body size is 30000000 bytes."]},"traceId":"00-2ef2cc38af3bba1204dc6a130b65db1d-bf9d436e2b64ecdc-00"}
``n---


**Response Status:** Error

**Response Body:**
Cannot process argument because the value of argument "name" is not valid. Change the value of the "name" argument and run the operation again.
---


### 25a. HTTP Method - POST to GET endpoint

**Request:** POST http://localhost:5049/api/UserVerification/88404714-5076-4ed1-e479-08def300e686

**Body:**
`json
{

}
``n
**Response Status:** 405

**Response Body:** (Empty)
---


### 25b. HTTP Method - GET to DELETE endpoint

**Request:** GET http://localhost:5049/api/UserVerification?UserId=88404714-5076-4ed1-e479-08def300e686&DocumentId=c7be6940-677d-41d9-8353-55eab71cb203

**Response Status:** 405

**Response Body:** (Empty)
---


### 26. Read - Get documents for non-existent UserId

**Request:** GET http://localhost:5049/api/UserVerification/b8f4198d-33c7-4a7f-acd1-f77dcb5eb26a

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



# Admin Verifications Slice Test Report


### 0a. Setup - Login Admin

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Password": "Admin@123",
  "Email": "moatazmohammed2392003@gmail.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "e918dc31-0a83-4a97-fe44-08def48c9663",
      "email": "moatazmohammed2392003@gmail.com",
      "fullName": "Moataz Mohammed",
      "role": "Admin",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJlOTE4ZGMzMS0wYTgzLTRhOTctZmU0NC0wOGRlZjQ4Yzk2NjMiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImU5MThkYzMxLTBhODMtNGE5Ny1mZTQ0LTA4ZGVmNDhjOTY2MyIsImVtYWlsIjoibW9hdGF6bW9oYW1tZWQyMzkyMDAzQGdtYWlsLmNvbSIsIm5hbWUiOiJNb2F0YXogTW9oYW1tZWQiLCJzZWN1cml0eV9zdGFtcCI6IlE1U0lYQVRQRTNVTVRYSDZYUUNSUFBHQzJQRjRVQ01HIiwianRpIjoiYWY2ZDYzZWItMWZkMi00ZTUyLWI2YWYtMTMzYTc2MjYxZmQ0IiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiQWRtaW4iLCJuYmYiOjE3ODYxMTI1OTEsImV4cCI6MTc4NjExMzQ5MSwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.Xs0-ZtjzlUt3nhYr-0RhXfSQ1tJTEgT2-ZHSYe5dRV0",
    "expiresIn": 900,
    "refreshToken": "y8KXqAYXLg8IjqvlW3wHMEQmxO8HWDwPzx0HbtjTZo7iEyApa1sIP33JiqvjkBNO6j5pRkaoUvlntSqpOJ4uGg==",
    "refreshTokenExpiration": "2026-08-14T14:23:11.4633947Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 0b. Setup - Register Lawyer

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
`json
{
  "Password": "Password123!",
  "FullName": "Lawyer Verification",
  "Email": "lawyer_verification_700979407@test.com",
  "ConfirmPassword": "Password123!"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "bab640b1-4c18-45b5-f777-08def48f6968",
    "email": "lawyer_verification_700979407@test.com",
    "fullName": "Lawyer Verification",
    "role": "Lawyer"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


Found confirmation URL for lawyer_verification_700979407@test.com: http://localhost:5173/verify-email?userId=bab640b1-4c18-45b5-f777-08def48f6968&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIrbCs4Y1RUWW9VUXhzVXJwT2phaHJxZ3ZoU0lVUVh5ZVNMS2k2UmJmNW1YK21nNUpFYnEwSVJJL1FXMTVxSWpWaW0yRjF2SS9qU2J5Vzc1VlFSL2dZZzhYV0RudVdMWFJmKzRhc0VkZW04SkRFY0c3YTh4QWd4aTl2L3QyU0poR3p0UVBicjljREIwM1RnY0lUQTI0WEV0amE5UnplcEJoSXRZT2hZQ3Y3eTk0SHI3OXg2ejAraEVCSGtQKytGZi9FSkJhSUR3ekVqT09UeXgrczFrcHdIWDRoVHQ3MXFhSmpGbGw5TkVwejdOZz09

### Confirm Email for lawyer_verification_700979407@test.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=bab640b1-4c18-45b5-f777-08def48f6968&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIrbCs4Y1RUWW9VUXhzVXJwT2phaHJxZ3ZoU0lVUVh5ZVNMS2k2UmJmNW1YK21nNUpFYnEwSVJJL1FXMTVxSWpWaW0yRjF2SS9qU2J5Vzc1VlFSL2dZZzhYV0RudVdMWFJmKzRhc0VkZW04SkRFY0c3YTh4QWd4aTl2L3QyU0poR3p0UVBicjljREIwM1RnY0lUQTI0WEV0amE5UnplcEJoSXRZT2hZQ3Y3eTk0SHI3OXg2ejAraEVCSGtQKytGZi9FSkJhSUR3ekVqT09UeXgrczFrcHdIWDRoVHQ3MXFhSmpGbGw5TkVwejdOZz09

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


### 0c. Setup - Login Lawyer

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Password": "Password123!",
  "Email": "lawyer_verification_700979407@test.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "bab640b1-4c18-45b5-f777-08def48f6968",
      "email": "lawyer_verification_700979407@test.com",
      "fullName": "Lawyer Verification",
      "role": "Lawyer",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJiYWI2NDBiMS00YzE4LTQ1YjUtZjc3Ny0wOGRlZjQ4ZjY5NjgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImJhYjY0MGIxLTRjMTgtNDViNS1mNzc3LTA4ZGVmNDhmNjk2OCIsImVtYWlsIjoibGF3eWVyX3ZlcmlmaWNhdGlvbl83MDA5Nzk0MDdAdGVzdC5jb20iLCJuYW1lIjoiTGF3eWVyIFZlcmlmaWNhdGlvbiIsInNlY3VyaXR5X3N0YW1wIjoiVENYUzVBQzc0NjdaNkJJRUQzRkJYMzNTQTdTRFlCNDUiLCJqdGkiOiJkN2VjMWZiYS1iZjEwLTRjMGYtODY1MS0xMjk3MTQ2ZGM3OWUiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODYxMTI1OTQsImV4cCI6MTc4NjExMzQ5NCwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.ueI8-26xCehnnRXibMOSvpeRicHUNm9V7nRrU-70W5A",
    "expiresIn": 900,
    "refreshToken": "ZnSBkSVmPZdSDSxyZfh9Old+aWQ7S//SP5v4TwA8sxvcAN1nwWVLFHRXIzteBplPYikwlUhCMFKlsEKuqpzqBQ==",
    "refreshTokenExpiration": "2026-08-14T14:23:14.5533094Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 0d. Setup - Lawyer Uploads Document

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**
- UserId = bab640b1-4c18-45b5-f777-08def48f6968
- Documents[0].ExpirationDate = 2030-01-01
- Documents[0].Type = 1
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
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 1,
  "totalRecords": 1,
  "hasNextPage": false,
  "hasPreviousPage": false,
  "success": true,
  "data": [
    {
      "lawyerId": "bab640b1-4c18-45b5-f777-08def48f6968",
      "fullName": "Lawyer Verification",
      "email": "lawyer_verification_700979407@test.com",
      "phoneNumber": null,
      "pendingDocumentCount": 1,
      "verifiedDocumentCount": 0,
      "rejectedDocumentCount": 0,
      "role": "Lawyer"
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 2. Get Verification Details (Admin)

**Request:** GET http://localhost:5049/api/admin/verifications/bab640b1-4c18-45b5-f777-08def48f6968

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "lawyerId": "bab640b1-4c18-45b5-f777-08def48f6968",
    "fullName": "Lawyer Verification",
    "email": "lawyer_verification_700979407@test.com",
    "phoneNumber": null,
    "nationalNumber": null,
    "address": null,
    "governorate": null,
    "city": null,
    "gender": null,
    "dateOfBirth": null,
    "accountStatus": "PendingReview",
    "isFullyVerified": false,
    "role": "Lawyer",
    "level": 1,
    "specializations": [],
    "bio": null,
    "documents": [
      {
        "documentId": "b5700cee-9aba-4d45-b8b0-08def48f6c9f",
        "documentType": "NationalIdFront",
        "status": "Pending",
        "fileName": "dummy_admin_id.jpg",
        "contentType": "image/jpeg",
        "expirationDate": "2030-01-01",
        "reviewedAt": null,
        "rejectionReason": null,
        "contentUrl": "/api/admin/verifications/documents/b5700cee-9aba-4d45-b8b0-08def48f6c9f/content"
      }
    ],
    "modifiedFields": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 3. Get Document Content (Admin)

**Request:** GET http://localhost:5049/api/admin/verifications/documents/b5700cee-9aba-4d45-b8b0-08def48f6c9f/content

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "downloadUrl": "https://pvqeapcynhjklikvukqe.supabase.co/storage/v1/object/sign/SmartCourt/bab640b1-4c18-45b5-f777-08def48f6968/national-id/3b1c8a0f-060b-4365-b620-4c13e713c436.jpg?token=eyJraWQiOiJzdG9yYWdlLXVybC1zaWduaW5nLWtleV9hYWUyYTY1YS01NWVjLTQxYzItYTdmZS1iNzI2YTY3YTI2OTMiLCJhbGciOiJIUzI1NiJ9.eyJ1cmwiOiJTbWFydENvdXJ0L2JhYjY0MGIxLTRjMTgtNDViNS1mNzc3LTA4ZGVmNDhmNjk2OC9uYXRpb25hbC1pZC8zYjFjOGEwZi0wNjBiLTQzNjUtYjYyMC00YzEzZTcxM2M0MzYuanBnIiwic2NvcGUiOiJkb3dubG9hZCIsImlhdCI6MTc4NjExMjU5OSwiZXhwIjoxNzg2MTE2MTk5fQ.RtthHzKvF7bVX29Q059nOIeXbzTcY8Fly4mX1D-45q8",
    "contentType": "image/jpeg",
    "fileName": "dummy_admin_id.jpg"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 4. Review Verification Document - Reject

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/b5700cee-9aba-4d45-b8b0-08def48f6c9f

**Body:**
`json
{
  "RejectionReason": "Image is too blurry.",
  "Decision": 2
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "documentId": "b5700cee-9aba-4d45-b8b0-08def48f6c9f",
    "documentStatus": "Rejected",
    "lawyerAccountStatus": "Unverified",
    "isFullyVerified": false
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 5a. Setup - Lawyer Re-uploads Document

**Request:** POST http://localhost:5049/api/UserVerification/submit-verification-documents (Multipart Form Data)

**Form Data:**
- UserId = bab640b1-4c18-45b5-f777-08def48f6968
- Documents[0].ExpirationDate = 2030-01-01
- Documents[0].Type = 2
- Documents[0].File = [File: P:\Projects\Smart Court\SmartCourt.Tests\HttpTests\dummy_admin_id.jpg]


**Response Status:** 200

**Response Body:**
```json
{"success":true,"data":{"uploadedDocuments":[{"fileName":"dummy_admin_id.jpg","type":2}],"failedDocuments":[]},"message":null,"errors":null,"statusCode":200}
```n---


### 5b. Setup - Get Verification Details Again

**Request:** GET http://localhost:5049/api/admin/verifications/bab640b1-4c18-45b5-f777-08def48f6968

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "lawyerId": "bab640b1-4c18-45b5-f777-08def48f6968",
    "fullName": "Lawyer Verification",
    "email": "lawyer_verification_700979407@test.com",
    "phoneNumber": null,
    "nationalNumber": null,
    "address": null,
    "governorate": null,
    "city": null,
    "gender": null,
    "dateOfBirth": null,
    "accountStatus": "PendingReview",
    "isFullyVerified": false,
    "role": "Lawyer",
    "level": 1,
    "specializations": [],
    "bio": null,
    "documents": [
      {
        "documentId": "b5700cee-9aba-4d45-b8b0-08def48f6c9f",
        "documentType": "NationalIdFront",
        "status": "Rejected",
        "fileName": "dummy_admin_id.jpg",
        "contentType": "image/jpeg",
        "expirationDate": "2030-01-01",
        "reviewedAt": null,
        "rejectionReason": "Image is too blurry.",
        "contentUrl": "/api/admin/verifications/documents/b5700cee-9aba-4d45-b8b0-08def48f6c9f/content"
      },
      {
        "documentId": "04833ffe-6166-41ac-b8b1-08def48f6c9f",
        "documentType": "NationalIdBack",
        "status": "Pending",
        "fileName": "dummy_admin_id.jpg",
        "contentType": "image/jpeg",
        "expirationDate": "2030-01-01",
        "reviewedAt": null,
        "rejectionReason": null,
        "contentUrl": "/api/admin/verifications/documents/04833ffe-6166-41ac-b8b1-08def48f6c9f/content"
      }
    ],
    "modifiedFields": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 6. Review Verification Document - Approve

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/04833ffe-6166-41ac-b8b1-08def48f6c9f

**Body:**
`json
{
  "RejectionReason": null,
  "Decision": 1
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "documentId": "04833ffe-6166-41ac-b8b1-08def48f6c9f",
    "documentStatus": "Verified",
    "lawyerAccountStatus": "Unverified",
    "isFullyVerified": false
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 7. Unauthorized Access (Lawyer Token)

**Request:** GET http://localhost:5049/api/admin/verifications

**Response Status:** 403

**Response Body:**
Response status code does not indicate success: 403 (Forbidden).
---


### 8. Lawyer Deletes Rejected Document

**Request:** DELETE http://localhost:5049/api/UserVerification?UserId=bab640b1-4c18-45b5-f777-08def48f6968&DocumentId=b5700cee-9aba-4d45-b8b0-08def48f6c9f

**Response Status:** 404

**Response Body:**
`json
{
  "success": false,
  "message": null,
  "errors": [
    "Verification document was not found."
  ],
  "statusCode": 404
}
``n---


### 9. Get User Documents After Approval

**Request:** GET http://localhost:5049/api/UserVerification/bab640b1-4c18-45b5-f777-08def48f6968

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "documents": [
      {
        "documentId": "b5700cee-9aba-4d45-b8b0-08def48f6c9f",
        "documentType": 1,
        "status": 3,
        "expirationDate": "2030-01-01",
        "isCurrent": true,
        "fileName": "dummy_admin_id.jpg",
        "rejectionReason": "Image is too blurry."
      },
      {
        "documentId": "04833ffe-6166-41ac-b8b1-08def48f6c9f",
        "documentType": 2,
        "status": 2,
        "expirationDate": "2030-01-01",
        "isCurrent": true,
        "fileName": "dummy_admin_id.jpg",
        "rejectionReason": null
      }
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 10. Read - GET list invalid pagination

**Request:** GET http://localhost:5049/api/admin/verifications?PageNumber=0&PageSize=-1

**Response Status:** 400

**Response Body:**
`json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "PageSize": [
      "'Page Size' must be between 1 and 50. You entered -1."
    ],
    "PageNumber": [
      "'Page Number' must be greater than or equal to '1'."
    ]
  },
  "traceId": "00-c74ca444c49e521c7f7e96a1aa13bf59-2d7cd6c3b7c4bf66-00"
}
``n---


### 11. Read - GET list missing token

**Request:** GET http://localhost:5049/api/admin/verifications?PageNumber=1&PageSize=10

**Response Status:** 401

**Response Body:**
Response status code does not indicate success: 401 (Unauthorized).
---


### 12. Read - GET details malformed Guid

**Request:** GET http://localhost:5049/api/admin/verifications/not-a-guid

**Response Status:** 404

**Response Body:**
Response status code does not indicate success: 404 (Not Found).
---


### 13. Read - GET details non-existent LawyerId

**Request:** GET http://localhost:5049/api/admin/verifications/54368e76-f50e-47e3-abec-f9273e54c446

**Response Status:** 404

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "User was not found.",
  "errors": null,
  "statusCode": 404
}
``n---


### 14a. Setup - Register Client

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
  "Password": "Password123!",
  "FullName": "Client Verification",
  "Email": "client_700979407@test.com",
  "ConfirmPassword": "Password123!"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "d4f80212-8065-4d74-f778-08def48f6968",
    "email": "client_700979407@test.com",
    "fullName": "Client Verification",
    "role": "Client"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


### 14b. Read - GET details for Client user

**Request:** GET http://localhost:5049/api/admin/verifications/d4f80212-8065-4d74-f778-08def48f6968

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "lawyerId": "d4f80212-8065-4d74-f778-08def48f6968",
    "fullName": "Client Verification",
    "email": "client_700979407@test.com",
    "phoneNumber": null,
    "nationalNumber": null,
    "address": null,
    "governorate": null,
    "city": null,
    "gender": null,
    "dateOfBirth": null,
    "accountStatus": "Unverified",
    "isFullyVerified": false,
    "role": "Client",
    "level": null,
    "specializations": [],
    "bio": null,
    "documents": [],
    "modifiedFields": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 15. Read - GET content non-existent DocumentId

**Request:** GET http://localhost:5049/api/admin/verifications/documents/6eeca536-6f7e-4db8-97a8-f78f2f9186ab/content

**Response Status:** 404

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "Verification document was not found.",
  "errors": null,
  "statusCode": 404
}
``n---


### 16. Update - PATCH Reject without Reason

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/04833ffe-6166-41ac-b8b1-08def48f6c9f

**Body:**
`json
{
  "RejectionReason": "",
  "Decision": 2
}
``n
**Response Status:** 400

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": [
    "'Rejection Reason' must not be empty."
  ],
  "statusCode": 400
}
``n---


### 17. Update - PATCH Approve with Reason

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/04833ffe-6166-41ac-b8b1-08def48f6c9f

**Body:**
`json
{
  "RejectionReason": "This should fail because you can't have a reason for approve",
  "Decision": 1
}
``n
**Response Status:** 400

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": [
    "A rejection reason can only be supplied when rejecting a document."
  ],
  "statusCode": 400
}
``n---


### 18. Update - PATCH invalid Decision enum

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/04833ffe-6166-41ac-b8b1-08def48f6c9f

**Body:**
`json
{
  "RejectionReason": null,
  "Decision": 99
}
``n
**Response Status:** 400

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": [
    "'Decision' has a range of values which does not include '99'."
  ],
  "statusCode": 400
}
``n---


### 19. Update - PATCH non-existent DocumentId

**Request:** PATCH http://localhost:5049/api/admin/verifications/documents/1a5769b8-7f36-4b91-a9aa-fe7452cee1dd

**Body:**
`json
{
  "RejectionReason": null,
  "Decision": 1
}
``n
**Response Status:** 404

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "Verification document was not found.",
  "errors": null,
  "statusCode": 404
}
``n---


### 20. Admin Approve User Account

**Request:** PATCH http://localhost:5049/api/admin/verifications/bab640b1-4c18-45b5-f777-08def48f6968/approve-account

**Body:**
`json
{}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "message": "تم اعتماد بيانات الحساب بنجاح"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### 21. Admin Reject User Account

**Request:** PATCH http://localhost:5049/api/admin/verifications/bab640b1-4c18-45b5-f777-08def48f6968/reject-account

**Body:**
`json
{
  "RejectionReason": "Incomplete profile information."
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "message": "تم رفض بيانات الحساب بنجاح"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---



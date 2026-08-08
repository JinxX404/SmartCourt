# Case Slice HTTP Tests End-to-End Workflow Report

Generated at 2026-08-07 20:53:24


### Register Client

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
`json
{
  "Email": "client_case_test_20260807205324@example.com",
  "FullName": "Test Client",
  "ConfirmPassword": "Password123!",
  "Password": "Password123!"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "2698aa96-e033-4402-f796-08def48f6968",
    "email": "client_case_test_20260807205324@example.com",
    "fullName": "Test Client",
    "role": "Client"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


Found confirmation URL for client_case_test_20260807205324@example.com: http://localhost:5173/verify-email?userId=2698aa96-e033-4402-f796-08def48f6968&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI4TUZkUUxmSHlobExaTU1HYXM5ZENOdkxiektpL2VHZVZ2WWdFQWFPL2xLNGpIYzByL1hNZUdIUFpsem8zWEo4SnBLdmNaYWx3c1NlZERITVFOZUxqUU90cEJFM2dhWnBXazJkQmU4L25QU1hDajFJeHFkb3FjSGdyeGRqbld4Zk44UkJWc0FBVmx1M2piLzNINndxQThwR1hlOGNYMkI1RmVZWkkwd1FhQ1VHdzBESlBTbE43NVNFR0F1TnFKdDhoWVpXOVdicU1IVUVpa0tDSEZoRGpSbkpncFVEd2pBTy9Ralh3YXl0VktKUT09

### Confirm Email for client_case_test_20260807205324@example.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=2698aa96-e033-4402-f796-08def48f6968&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTI4TUZkUUxmSHlobExaTU1HYXM5ZENOdkxiektpL2VHZVZ2WWdFQWFPL2xLNGpIYzByL1hNZUdIUFpsem8zWEo4SnBLdmNaYWx3c1NlZERITVFOZUxqUU90cEJFM2dhWnBXazJkQmU4L25QU1hDajFJeHFkb3FjSGdyeGRqbld4Zk44UkJWc0FBVmx1M2piLzNINndxQThwR1hlOGNYMkI1RmVZWkkwd1FhQ1VHdzBESlBTbE43NVNFR0F1TnFKdDhoWVpXOVdicU1IVUVpa0tDSEZoRGpSbkpncFVEd2pBTy9Ralh3YXl0VktKUT09

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


### Login Client

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Password": "Password123!",
  "Email": "client_case_test_20260807205324@example.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "2698aa96-e033-4402-f796-08def48f6968",
      "email": "client_case_test_20260807205324@example.com",
      "fullName": "Test Client",
      "role": "Client",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIyNjk4YWE5Ni1lMDMzLTQ0MDItZjc5Ni0wOGRlZjQ4ZjY5NjgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjI2OThhYTk2LWUwMzMtNDQwMi1mNzk2LTA4ZGVmNDhmNjk2OCIsImVtYWlsIjoiY2xpZW50X2Nhc2VfdGVzdF8yMDI2MDgwNzIwNTMyNEBleGFtcGxlLmNvbSIsIm5hbWUiOiJUZXN0IENsaWVudCIsInNlY3VyaXR5X3N0YW1wIjoiSU5XNlJPU0FJSFBYN1Y2UTNVNlNRQlg1N01SNzczWEgiLCJqdGkiOiI0NmJiZDY4YS0xZDg1LTRmM2ItYmQ1NS1jY2E2M2U0ZmRmZGQiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJDbGllbnQiLCJuYmYiOjE3ODYxMjUyMDUsImV4cCI6MTc4NjEyNjEwNSwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.BiOKg_JpKGV-RyjhuZDqxeWRQ0kEEDShES2ywMd51j8",
    "expiresIn": 900,
    "refreshToken": "XjTfqka7mrqOgduMpepTppJnOo/8N0nvRTQKW6o4bFAhfWpN+Xu9OKtMhI/ebRn1BvBqm1mR84RgTiaw2KNhbw==",
    "refreshTokenExpiration": "2026-08-14T17:53:25.1740679Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Register Lawyer

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
`json
{
  "Email": "lawyer_case_test_20260807205325@example.com",
  "FullName": "Test Lawyer",
  "ConfirmPassword": "Password123!",
  "Password": "Password123!"
}
``n
**Response Status:** 201

**Response Body:**
`json
{
  "success": true,
  "data": {
    "userId": "dbe20365-8037-48d3-f797-08def48f6968",
    "email": "lawyer_case_test_20260807205325@example.com",
    "fullName": "Test Lawyer",
    "role": "Lawyer"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
``n---


Found confirmation URL for lawyer_case_test_20260807205325@example.com: http://localhost:5173/verify-email?userId=dbe20365-8037-48d3-f797-08def48f6968&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIvTTEvcFV3NnhwME5yRGtqdkVBSmtiWDhzK1VnaUJoSEpmNkZpTE1TaFh6TGxsa1pqQVRMZzNHa2NJdVRQeUcxMHRDMkF5NTl5clpPTDJ5MTVFODk4bXdpLzVsQ3hVbm1MVjdWV3VsdW13N1dUdTIxNHp1Y1REWVFKZkxVVkl6ai85ZEgwN0NucGovYWkyMEVHV1dFd1hqZzEzeHBhRzQ1WlR6d1p5NFBPK21hMVltTkN0R0RBZXk5MXIyVVBBL3A5ZHprY0xZRkhSMENxNXBFSkFTNWx3aDVqOVNDY1Q3Z0pIb2JBRGRTeG1UQT09

### Confirm Email for lawyer_case_test_20260807205325@example.com

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=dbe20365-8037-48d3-f797-08def48f6968&token=Q2ZESjhNenUwZzNvQjZ4UHRJeUhHc293MTIvTTEvcFV3NnhwME5yRGtqdkVBSmtiWDhzK1VnaUJoSEpmNkZpTE1TaFh6TGxsa1pqQVRMZzNHa2NJdVRQeUcxMHRDMkF5NTl5clpPTDJ5MTVFODk4bXdpLzVsQ3hVbm1MVjdWV3VsdW13N1dUdTIxNHp1Y1REWVFKZkxVVkl6ai85ZEgwN0NucGovYWkyMEVHV1dFd1hqZzEzeHBhRzQ1WlR6d1p5NFBPK21hMVltTkN0R0RBZXk5MXIyVVBBL3A5ZHprY0xZRkhSMENxNXBFSkFTNWx3aDVqOVNDY1Q3Z0pIb2JBRGRTeG1UQT09

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


### Login Lawyer

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
`json
{
  "Password": "Password123!",
  "Email": "lawyer_case_test_20260807205325@example.com"
}
``n
**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "user": {
      "id": "dbe20365-8037-48d3-f797-08def48f6968",
      "email": "lawyer_case_test_20260807205325@example.com",
      "fullName": "Test Lawyer",
      "role": "Lawyer",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJkYmUyMDM2NS04MDM3LTQ4ZDMtZjc5Ny0wOGRlZjQ4ZjY5NjgiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImRiZTIwMzY1LTgwMzctNDhkMy1mNzk3LTA4ZGVmNDhmNjk2OCIsImVtYWlsIjoibGF3eWVyX2Nhc2VfdGVzdF8yMDI2MDgwNzIwNTMyNUBleGFtcGxlLmNvbSIsIm5hbWUiOiJUZXN0IExhd3llciIsInNlY3VyaXR5X3N0YW1wIjoiU1hJRldDSk1WUEdHSURGQVlOVFJFQ1RSM1VXNUhHUjIiLCJqdGkiOiI3NGI3OWY5Ni01ZWU2LTQ3NGYtYTJmZC03YzE5OGU3MDk2MDQiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJMYXd5ZXIiLCJuYmYiOjE3ODYxMjUyMDYsImV4cCI6MTc4NjEyNjEwNiwiaXNzIjoiU21hcnRDb3VydEFQSSIsImF1ZCI6IlNtYXJ0Q291cnRDbGllbnQifQ.fEJvohaq3FGGf_8NVNMBxvKt5gMGhX8WEsIaIt-YjMI",
    "expiresIn": 900,
    "refreshToken": "fE50cpNVrslkeGgb73V4H68rQVnhj5925+bB1YKTBnlLW8vhZCd6WgPSR/G/pnR9g9k3hK1mHPtnFzqwQ5jhWQ==",
    "refreshTokenExpiration": "2026-08-14T17:53:26.7473269Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Create Case (400 Validation Error)

**Request:** POST http://localhost:5049/api/Case

**Body:**
(multipart/form-data)

**Response Status:** 400

**Response Body:**
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 84 105 116 108 101 34 58 91 34 84 104 101 32 84 105 116 108 101 32 102 105 101 108 100 32 105 115 32 114 101 113 117 105 114 101 100 46 34 44 34 67 97 115 101 32 116 105 116 108 101 32 99 97 110 39 116 32 98 101 32 101 109 112 116 121 34 93 44 34 68 111 99 117 109 101 110 116 115 34 58 91 34 84 104 101 32 68 111 99 117 109 101 110 116 115 32 102 105 101 108 100 32 105 115 32 114 101 113 117 105 114 101 100 46 34 93 44 34 68 101 115 99 114 105 112 116 105 111 110 34 58 91 34 84 104 101 32 68 101 115 99 114 105 112 116 105 111 110 32 102 105 101 108 100 32 105 115 32 114 101 113 117 105 114 101 100 46 34 44 34 67 97 115 101 32 100 101 115 99 114 105 112 116 105 111 110 32 99 97 110 39 116 32 98 101 32 101 109 112 116 121 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 100 51 55 53 50 57 99 54 99 101 49 54 100 48 54 102 52 49 52 50 52 48 49 98 98 51 57 99 51 57 57 51 45 102 100 51 48 57 98 55 101 56 51 56 100 55 101 102 57 45 48 48 34 125
---


### Create Case (Valid Success)

**Request:** POST http://localhost:5049/api/Case

**Body:**
(multipart/form-data)

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "caseId": "23124ac5-c36e-4ea3-89b5-644d5afe284e",
    "failedDocuments": []
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
``n---


### Get Case By ID

**Request:** GET http://localhost:5049/api/Case/23124ac5-c36e-4ea3-89b5-644d5afe284e

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "23124ac5-c36e-4ea3-89b5-644d5afe284e",
    "clientId": "2698aa96-e033-4402-f796-08def48f6968",
    "title": "Valid Case Title",
    "description": "Detailed description of the case for testing.",
    "governorate": "Cairo",
    "city": "Maadi",
    "status": "Submitted",
    "createdAt": "2026-08-07T17:53:29.1139717",
    "documents": [
      {
        "id": "b5915695-259b-4b1f-0280-08def48f6c9b",
        "fileName": "dummy_case.pdf",
        "fileUrl": "2698aa96-e033-4402-f796-08def48f6968/case-documents/92c2c68a-4100-4c9c-aec6-c339b05ac443.pdf",
        "contentType": "application/octet-stream"
      }
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Get Case By ID (404 Not Found)

**Request:** GET http://localhost:5049/api/Case/58c10dc5-d980-41f8-a386-a908006a16e0

**Response Status:** 404

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": null,
  "errors": [
    "Case not found"
  ],
  "statusCode": 404
}
``n---


### Get All Cases

**Request:** GET http://localhost:5049/api/Case

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": [
    {
      "id": "04b7175b-c436-4fe3-867b-02e738fe8ddb",
      "title": "Case for Contract",
      "status": "Submitted",
      "createdAt": "2026-08-07T17:30:06.4337712",
      "documentCount": 1
    },
    {
      "id": "23124ac5-c36e-4ea3-89b5-644d5afe284e",
      "title": "Valid Case Title",
      "status": "Submitted",
      "createdAt": "2026-08-07T17:53:29.1139717",
      "documentCount": 1
    },
    {
      "id": "87393346-9233-43af-9631-797904b3905e",
      "title": "Case for Contract",
      "status": "Matched",
      "createdAt": "2026-08-07T17:32:49.0003707",
      "documentCount": 1
    },
    {
      "id": "187fb477-8401-41e9-9ccc-fcf034641bb7",
      "title": "Case for Contract",
      "status": "Matched",
      "createdAt": "2026-08-07T17:38:42.7649349",
      "documentCount": 1
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Update Case (Valid Success)

**Request:** PUT http://localhost:5049/api/Case/23124ac5-c36e-4ea3-89b5-644d5afe284e

**Body:**
(multipart/form-data)

**Response Status:** 400

**Response Body:**
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 68 111 99 117 109 101 110 116 115 34 58 91 34 84 104 101 32 68 111 99 117 109 101 110 116 115 32 102 105 101 108 100 32 105 115 32 114 101 113 117 105 114 101 100 46 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 51 55 57 99 55 99 97 97 97 48 99 57 48 51 100 49 102 49 102 101 56 53 51 100 102 48 57 53 52 49 98 53 45 51 56 102 49 54 53 99 51 48 97 97 53 99 97 48 98 45 48 48 34 125
---


### Create Case to Delete

**Request:** POST http://localhost:5049/api/Case

**Body:**
(multipart/form-data)

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "caseId": "aab8a9eb-5c3c-41cd-b9f0-89059dff586b",
    "failedDocuments": []
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
``n---


### Delete Case (Success)

**Request:** DELETE http://localhost:5049/api/Case/aab8a9eb-5c3c-41cd-b9f0-89059dff586b

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Create Case (Stress - Malicious Payload)

**Request:** POST http://localhost:5049/api/Case

**Body:**
(multipart/form-data)

**Response Status:** 400

**Response Body:**
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 68 111 99 117 109 101 110 116 115 34 58 91 34 84 104 101 32 68 111 99 117 109 101 110 116 115 32 102 105 101 108 100 32 105 115 32 114 101 113 117 105 114 101 100 46 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 100 54 49 53 50 99 52 49 50 102 102 101 97 102 100 100 52 57 55 98 52 100 98 51 99 98 57 56 52 99 55 98 45 51 102 102 50 101 52 101 56 50 53 52 54 56 56 48 48 45 48 48 34 125
---


### Review Case (AI Request)

**Request:** POST http://localhost:5049/api/cases/23124ac5-c36e-4ea3-89b5-644d5afe284e/review

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
    "id": "b3375525-2893-4279-9141-eda45fb3bee5",
    "caseId": "23124ac5-c36e-4ea3-89b5-644d5afe284e",
    "isLatest": true,
    "createdAt": "2026-08-07T17:53:36.0638585Z",
    "reviewPoints": [
      {
        "id": "1faa23f7-b03b-418c-8338-de706847436b",
        "description": "تتمثل نقطة القوة الأساسية في صياغة الموضوع بوضوح حول 'Valid Case Title'، وتوافر السند المبدئي الذي يرجح كفة الموكل في إثبات أصل الالتزام وتفوقه إثباتياً على الخصم.",
        "type": "Strength"
      },
      {
        "id": "ac56c0d3-ddba-49a5-a6d1-869b1cc464f1",
        "description": "ميزة الخصم تتمثل في غياب التوثيق الرسمي للتنبيهات أو الإخطارات المتبادلة بين الأطراف، مما يتيح له إنكار الاستلام أو الدفع بالتراخي في المطالبة.",
        "type": "Weakness"
      },
      {
        "id": "2b1117be-58f7-4195-be71-a54c14f1426a",
        "description": "يحتاج الملف إلى استيفاء النقاط والمعلومات التالية لضمان صياغة صحيفة الدعوى بشكل مكتمل: حصر وتفصيص المبالغ المالية المطلوبة والتعويضات الدقيقة عن الضرر المادي والمعنوي، وإدراج التواريخ الرسمية الدقيقة لبدء النزاع وتاريخ الإخلال بالتعهدات.",
        "type": "MissingCaseInfo"
      },
      {
        "id": "cf63d7bf-e339-4764-83a0-2748af33a088",
        "description": "المستندات المحددة المطلوبة لإكمال الملف: أصل العقد/الاتفاق المبرم، صورة بطاقة الرقم القومي سارية لكل أطراف الدعوى، إيصالات التحويل أو السداد المالي، وأي إنذارات رسمية على يد محضر.",
        "type": "MissingCaseDoc"
      },
      {
        "id": "d170662d-ffd3-4de5-87c8-0a7efda796f2",
        "description": "قم بإعادة هيكلة وصف القضية في صورة جدول زمني متسلسل، يبدأ من تاريخ التعهد الأول، مروراً بتاريخ الإخلال، وصولاً إلى حجم الأضرار المترتبة حالياً.",
        "type": "Suggestion"
      },
      {
        "id": "5a3f2d41-48d9-4ddd-a8e3-4ae62a405838",
        "description": "قم بتفقيط وقسمة كافة المطالبات المالية إلى بنود مستقلة (أصل الدين، الفوائد أو التعويض عن المماطلة، والرسوم) وتوثيق كل بند بسند كتابي مستقل.",
        "type": "Suggestion"
      },
      {
        "id": "4eba92ae-85a8-4419-809c-5480a926e8ea",
        "description": "قم بتنظيم وثائق الملف في مجلد مرتب حسب التاريخ، وتأكد من مسح الأوراق ضوئياً بدقة عالية لضمان سهولة الإسناد والفحص القضائي.",
        "type": "Suggestion"
      }
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Get Latest Review

**Request:** GET http://localhost:5049/api/cases/23124ac5-c36e-4ea3-89b5-644d5afe284e/reviews/latest

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "id": "b3375525-2893-4279-9141-eda45fb3bee5",
    "caseId": "23124ac5-c36e-4ea3-89b5-644d5afe284e",
    "isLatest": true,
    "createdAt": "2026-08-07T17:53:36.0638585",
    "reviewPoints": [
      {
        "id": "d170662d-ffd3-4de5-87c8-0a7efda796f2",
        "description": "قم بإعادة هيكلة وصف القضية في صورة جدول زمني متسلسل، يبدأ من تاريخ التعهد الأول، مروراً بتاريخ الإخلال، وصولاً إلى حجم الأضرار المترتبة حالياً.",
        "type": "Suggestion"
      },
      {
        "id": "cf63d7bf-e339-4764-83a0-2748af33a088",
        "description": "المستندات المحددة المطلوبة لإكمال الملف: أصل العقد/الاتفاق المبرم، صورة بطاقة الرقم القومي سارية لكل أطراف الدعوى، إيصالات التحويل أو السداد المالي، وأي إنذارات رسمية على يد محضر.",
        "type": "MissingCaseDoc"
      },
      {
        "id": "5a3f2d41-48d9-4ddd-a8e3-4ae62a405838",
        "description": "قم بتفقيط وقسمة كافة المطالبات المالية إلى بنود مستقلة (أصل الدين، الفوائد أو التعويض عن المماطلة، والرسوم) وتوثيق كل بند بسند كتابي مستقل.",
        "type": "Suggestion"
      },
      {
        "id": "4eba92ae-85a8-4419-809c-5480a926e8ea",
        "description": "قم بتنظيم وثائق الملف في مجلد مرتب حسب التاريخ، وتأكد من مسح الأوراق ضوئياً بدقة عالية لضمان سهولة الإسناد والفحص القضائي.",
        "type": "Suggestion"
      },
      {
        "id": "ac56c0d3-ddba-49a5-a6d1-869b1cc464f1",
        "description": "ميزة الخصم تتمثل في غياب التوثيق الرسمي للتنبيهات أو الإخطارات المتبادلة بين الأطراف، مما يتيح له إنكار الاستلام أو الدفع بالتراخي في المطالبة.",
        "type": "Weakness"
      },
      {
        "id": "2b1117be-58f7-4195-be71-a54c14f1426a",
        "description": "يحتاج الملف إلى استيفاء النقاط والمعلومات التالية لضمان صياغة صحيفة الدعوى بشكل مكتمل: حصر وتفصيص المبالغ المالية المطلوبة والتعويضات الدقيقة عن الضرر المادي والمعنوي، وإدراج التواريخ الرسمية الدقيقة لبدء النزاع وتاريخ الإخلال بالتعهدات.",
        "type": "MissingCaseInfo"
      },
      {
        "id": "1faa23f7-b03b-418c-8338-de706847436b",
        "description": "تتمثل نقطة القوة الأساسية في صياغة الموضوع بوضوح حول 'Valid Case Title'، وتوافر السند المبدئي الذي يرجح كفة الموكل في إثبات أصل الالتزام وتفوقه إثباتياً على الخصم.",
        "type": "Strength"
      }
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Finalize Case (Transition to Matched)

**Request:** POST http://localhost:5049/api/Case/23124ac5-c36e-4ea3-89b5-644d5afe284e/finalize

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
    "caseId": "23124ac5-c36e-4ea3-89b5-644d5afe284e",
    "totalEligibleLawyers": 0,
    "recommendations": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Create Proposal (Client to Lawyer)

**Request:** POST http://localhost:5049/api/proposals

**Body:**
`json
{
  "LawyerUserId": "dbe20365-8037-48d3-f797-08def48f6968",
  "LegalCaseId": "23124ac5-c36e-4ea3-89b5-644d5afe284e",
  "Message": "I would like to hire you for this case."
}
``n
**Response Status:** 404

**Response Body:**
`json
{
  "success": false,
  "data": null,
  "message": "Case was not found.",
  "errors": null,
  "statusCode": 404
}
``n---


### Lawyer Get Proposals

**Request:** GET http://localhost:5049/api/proposals

**Response Status:** 200

**Response Body:**
`json
{
  "success": true,
  "data": {
    "items": [],
    "page": 1,
    "pageSize": 10,
    "totalCount": 0,
    "hasNextPage": false
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
``n---


### Lawyer Accepts Proposal

**Request:** POST http://localhost:5049/api/proposals//accept

**Body:**
`json
{}
``n
**Response Status:** 404

**Response Body:**
Response status code does not indicate success: 404 (Not Found).
---



# Milestones Notifications HTTP Test Report

Generated at: 2026-08-09 18:34:37 +03:00


## Health and Milestones authorization boundary

### API is healthy

**Request:** GET http://localhost:5049/health

**Response Status:** 200

**Response Body:**
```text
Healthy
```
---


- [PASS] **API is healthy** (status=200)
### Add requires authentication

**Request:** POST http://localhost:5049/api/contracts/f9d10d80-96fd-42cb-8859-4e3ea75f3e5c/milestones

**Body:**
```json
{}
```

**Response Status:** 401

**Response Body:**
(Empty)
---


- [PASS] **Add requires authentication** (status=401)
### List requires authentication

**Request:** GET http://localhost:5049/api/contracts/f9d10d80-96fd-42cb-8859-4e3ea75f3e5c/milestones

**Response Status:** 401

**Response Body:**
(Empty)
---


- [PASS] **List requires authentication** (status=401)
### Update requires authentication

**Request:** PUT http://localhost:5049/api/contracts/f9d10d80-96fd-42cb-8859-4e3ea75f3e5c/milestones/f9d10d80-96fd-42cb-8859-4e3ea75f3e5c

**Body:**
```json
{}
```

**Response Status:** 401

**Response Body:**
(Empty)
---


- [PASS] **Update requires authentication** (status=401)
### Approve requires authentication

**Request:** POST http://localhost:5049/api/milestones/f9d10d80-96fd-42cb-8859-4e3ea75f3e5c/approve

**Body:**
```json
{}
```

**Response Status:** 401

**Response Body:**
(Empty)
---


- [PASS] **Approve requires authentication** (status=401)
### Ready requires authentication

**Request:** POST http://localhost:5049/api/milestones/f9d10d80-96fd-42cb-8859-4e3ea75f3e5c/ready-for-funding

**Body:**
```json
{}
```

**Response Status:** 401

**Response Body:**
(Empty)
---


- [PASS] **Ready requires authentication** (status=401)
### Submit requires authentication

**Request:** POST http://localhost:5049/api/milestones/f9d10d80-96fd-42cb-8859-4e3ea75f3e5c/submit

**Body:**
```json
{}
```

**Response Status:** 401

**Response Body:**
(Empty)
---


- [PASS] **Submit requires authentication** (status=401)
### Accept requires authentication

**Request:** POST http://localhost:5049/api/milestones/f9d10d80-96fd-42cb-8859-4e3ea75f3e5c/accept

**Body:**
```json
{}
```

**Response Status:** 401

**Response Body:**
(Empty)
---


- [PASS] **Accept requires authentication** (status=401)
### Request changes requires authentication

**Request:** POST http://localhost:5049/api/milestones/f9d10d80-96fd-42cb-8859-4e3ea75f3e5c/request-changes

**Body:**
```json
{}
```

**Response Status:** 401

**Response Body:**
(Empty)
---


- [PASS] **Request changes requires authentication** (status=401)
### Create change request requires authentication

**Request:** POST http://localhost:5049/api/milestones/f9d10d80-96fd-42cb-8859-4e3ea75f3e5c/change-requests

**Body:**
```json
{}
```

**Response Status:** 401

**Response Body:**
(Empty)
---


- [PASS] **Create change request requires authentication** (status=401)
### Approve change request requires authentication

**Request:** POST http://localhost:5049/api/change-requests/f9d10d80-96fd-42cb-8859-4e3ea75f3e5c/approve

**Body:**
```json
{}
```

**Response Status:** 401

**Response Body:**
(Empty)
---


- [PASS] **Approve change request requires authentication** (status=401)
### Reject change request requires authentication

**Request:** POST http://localhost:5049/api/change-requests/f9d10d80-96fd-42cb-8859-4e3ea75f3e5c/reject

**Body:**
```json
{}
```

**Response Status:** 401

**Response Body:**
(Empty)
---


- [PASS] **Reject change request requires authentication** (status=401)
### Cancel change request requires authentication

**Request:** POST http://localhost:5049/api/change-requests/f9d10d80-96fd-42cb-8859-4e3ea75f3e5c/cancel

**Body:**
```json
{}
```

**Response Status:** 401

**Response Body:**
(Empty)
---


- [PASS] **Cancel change request requires authentication** (status=401)

## Zero-assumption accounts with mock Email confirmation

### Login admin

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
```json
{
  "Password": "[REDACTED]",
  "Email": "[REDACTED]"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "user": {
      "id": "54af6cd4-a46e-4fc6-34ca-08def604e4b7",
      "email": "[REDACTED]",
      "fullName": "System Administrator",
      "role": "Admin",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "[REDACTED]",
    "expiresIn": 900,
    "refreshToken": "[REDACTED]",
    "refreshTokenExpiration": "2026-08-16T15:34:37.9288738Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Login admin** (status=200)
### Register client

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
```json
{
  "ConfirmPassword": "[REDACTED]",
  "Email": "[REDACTED]",
  "Password": "[REDACTED]",
  "FullName": "Contracts Notifications client"
}
```

**Response Status:** 201

**Response Body:**
```json
{
  "success": true,
  "data": {
    "userId": "49a02ff4-6241-4855-f4df-08def628c2bc",
    "email": "[REDACTED]",
    "fullName": "Contracts Notifications client",
    "role": "Client"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **Register client** (status=201)
- [PASS] **Mock Email log contains client confirmation**
### Confirm client Email from mock log

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=49a02ff4-6241-4855-f4df-08def628c2bc&token=[REDACTED]

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "message": "تم تأكيد البريد الإلكتروني بنجاح.",
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Mock Email client confirmation succeeds** (status=200)
### Login client

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
```json
{
  "Password": "[REDACTED]",
  "Email": "[REDACTED]"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "user": {
      "id": "49a02ff4-6241-4855-f4df-08def628c2bc",
      "email": "[REDACTED]",
      "fullName": "Contracts Notifications client",
      "role": "Client",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "[REDACTED]",
    "expiresIn": 900,
    "refreshToken": "[REDACTED]",
    "refreshTokenExpiration": "2026-08-16T15:34:38.9740306Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Login client** (status=200)
### Complete client profile

**Request:** POST http://localhost:5049/api/clients/profile/complete

**Body:**
```json
{
  "Gender": 1,
  "DateOfBirth": "1990-01-01",
  "NationalNumber": "[REDACTED]",
  "Address": "Cairo",
  "PhoneNumber": "[REDACTED]"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "message": "تم استكمال الملف الشخصي بنجاح.",
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Complete client profile** (status=200)
### Approve client account

**Request:** PATCH http://localhost:5049/api/admin/verifications/49a02ff4-6241-4855-f4df-08def628c2bc/approve-account

**Body:**
```json
{}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "message": "تم اعتماد بيانات الحساب بنجاح"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Approve client account** (status=200)
### Re-login approved client

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
```json
{
  "Password": "[REDACTED]",
  "Email": "[REDACTED]"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "user": {
      "id": "49a02ff4-6241-4855-f4df-08def628c2bc",
      "email": "[REDACTED]",
      "fullName": "Contracts Notifications client",
      "role": "Client",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "[REDACTED]",
    "expiresIn": 900,
    "refreshToken": "[REDACTED]",
    "refreshTokenExpiration": "2026-08-16T15:34:39.5853905Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Re-login approved client** (status=200)
### Register lawyer

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
```json
{
  "ConfirmPassword": "[REDACTED]",
  "Email": "[REDACTED]",
  "Password": "[REDACTED]",
  "FullName": "Contracts Notifications lawyer"
}
```

**Response Status:** 201

**Response Body:**
```json
{
  "success": true,
  "data": {
    "userId": "7955b16b-9125-456d-f4e0-08def628c2bc",
    "email": "[REDACTED]",
    "fullName": "Contracts Notifications lawyer",
    "role": "Lawyer"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **Register lawyer** (status=201)
- [PASS] **Mock Email log contains lawyer confirmation**
### Confirm lawyer Email from mock log

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=7955b16b-9125-456d-f4e0-08def628c2bc&token=[REDACTED]

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "message": "تم تأكيد البريد الإلكتروني بنجاح.",
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Mock Email lawyer confirmation succeeds** (status=200)
### Login lawyer

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
```json
{
  "Password": "[REDACTED]",
  "Email": "[REDACTED]"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "user": {
      "id": "7955b16b-9125-456d-f4e0-08def628c2bc",
      "email": "[REDACTED]",
      "fullName": "Contracts Notifications lawyer",
      "role": "Lawyer",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "[REDACTED]",
    "expiresIn": 900,
    "refreshToken": "[REDACTED]",
    "refreshTokenExpiration": "2026-08-16T15:34:41.4447826Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Login lawyer** (status=200)
### Complete lawyer profile

**Request:** POST http://localhost:5049/api/lawyers/profile/complete

**Body:**
```json
{
  "NationalNumber": "[REDACTED]",
  "Level": 1,
  "Address": "Cairo",
  "Specializations": [
    {
      "YearsOfExperience": 5,
      "Specialization": 1,
      "CasesHandled": 10
    }
  ],
  "PhoneNumber": "[REDACTED]",
  "Gender": 1,
  "Bio": "Contracts notification lifecycle lawyer",
  "DateOfBirth": "1985-01-01"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "message": "تم استكمال البيانات بنجاح",
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Complete lawyer profile** (status=200)
### Approve lawyer account

**Request:** PATCH http://localhost:5049/api/admin/verifications/7955b16b-9125-456d-f4e0-08def628c2bc/approve-account

**Body:**
```json
{}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "message": "تم اعتماد بيانات الحساب بنجاح"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Approve lawyer account** (status=200)
### Re-login approved lawyer

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
```json
{
  "Password": "[REDACTED]",
  "Email": "[REDACTED]"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "user": {
      "id": "7955b16b-9125-456d-f4e0-08def628c2bc",
      "email": "[REDACTED]",
      "fullName": "Contracts Notifications lawyer",
      "role": "Lawyer",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "[REDACTED]",
    "expiresIn": 900,
    "refreshToken": "[REDACTED]",
    "refreshTokenExpiration": "2026-08-16T15:34:41.9876986Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Re-login approved lawyer** (status=200)
### Register attacker

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
```json
{
  "ConfirmPassword": "[REDACTED]",
  "Email": "[REDACTED]",
  "Password": "[REDACTED]",
  "FullName": "Contracts Notifications attacker"
}
```

**Response Status:** 201

**Response Body:**
```json
{
  "success": true,
  "data": {
    "userId": "eec9dd6c-8521-400c-f4e1-08def628c2bc",
    "email": "[REDACTED]",
    "fullName": "Contracts Notifications attacker",
    "role": "Client"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **Register attacker** (status=201)
- [PASS] **Mock Email log contains attacker confirmation**
### Confirm attacker Email from mock log

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=eec9dd6c-8521-400c-f4e1-08def628c2bc&token=[REDACTED]

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "message": "تم تأكيد البريد الإلكتروني بنجاح.",
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Mock Email attacker confirmation succeeds** (status=200)
### Login attacker

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
```json
{
  "Password": "[REDACTED]",
  "Email": "[REDACTED]"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "user": {
      "id": "eec9dd6c-8521-400c-f4e1-08def628c2bc",
      "email": "[REDACTED]",
      "fullName": "Contracts Notifications attacker",
      "role": "Client",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "[REDACTED]",
    "expiresIn": 900,
    "refreshToken": "[REDACTED]",
    "refreshTokenExpiration": "2026-08-16T15:34:44.027202Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Login attacker** (status=200)
### Complete attacker profile

**Request:** POST http://localhost:5049/api/clients/profile/complete

**Body:**
```json
{
  "Gender": 1,
  "DateOfBirth": "1990-01-01",
  "NationalNumber": "[REDACTED]",
  "Address": "Cairo",
  "PhoneNumber": "[REDACTED]"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "message": "تم استكمال الملف الشخصي بنجاح.",
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Complete attacker profile** (status=200)
### Approve attacker account

**Request:** PATCH http://localhost:5049/api/admin/verifications/eec9dd6c-8521-400c-f4e1-08def628c2bc/approve-account

**Body:**
```json
{}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "message": "تم اعتماد بيانات الحساب بنجاح"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Approve attacker account** (status=200)
### Re-login approved attacker

**Request:** POST http://localhost:5049/api/auth/login

**Body:**
```json
{
  "Password": "[REDACTED]",
  "Email": "[REDACTED]"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "user": {
      "id": "eec9dd6c-8521-400c-f4e1-08def628c2bc",
      "email": "[REDACTED]",
      "fullName": "Contracts Notifications attacker",
      "role": "Client",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "[REDACTED]",
    "expiresIn": 900,
    "refreshToken": "[REDACTED]",
    "refreshTokenExpiration": "2026-08-16T15:34:44.5804376Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Re-login approved attacker** (status=200)

## Foundation and add endpoint

### milestones-primary - create case

**Request:** POST http://localhost:5049/api/Case

**Body:**
```json
{
  "Title": "milestones-primary case 183444797",
  "Description": "Complete case foundation for milestones-primary contract notifications.",
  "City": "Maadi",
  "Governorate": "Cairo"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "caseId": "8454613c-7106-432c-bf00-a319395d94d4",
    "failedDocuments": []
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **milestones-primary - create case** (status=200)
### milestones-primary - review case

**Request:** POST http://localhost:5049/api/cases/8454613c-7106-432c-bf00-a319395d94d4/review

**Body:**
```json
{}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "a143ac69-edea-4653-82e3-d9b24a2f3570",
    "caseId": "8454613c-7106-432c-bf00-a319395d94d4",
    "isLatest": true,
    "createdAt": "2026-08-09T15:34:46.2870549Z",
    "reviewPoints": [
      {
        "id": "e4769fee-77ab-4e95-bead-5b3bdcdbe818",
        "description": "تتمثل نقطة القوة الأساسية في صياغة الموضوع بوضوح حول 'milestones-primary case 183444797'، وتوافر السند المبدئي الذي يرجح كفة الموكل في إثبات أصل الالتزام وتفوقه إثباتياً على الخصم.",
        "type": "Strength"
      },
      {
        "id": "b264fbc4-ed9b-4633-b90c-65b1ee7107af",
        "description": "ميزة الخصم تتمثل في غياب التوثيق الرسمي للتنبيهات أو الإخطارات المتبادلة بين الأطراف، مما يتيح له إنكار الاستلام أو الدفع بالتراخي في المطالبة.",
        "type": "Weakness"
      },
      {
        "id": "613294d1-0afd-47a5-9a51-9463fd1cc595",
        "description": "يحتاج الملف إلى استيفاء النقاط والمعلومات التالية لضمان صياغة صحيفة الدعوى بشكل مكتمل: حصر وتفصيص المبالغ المالية المطلوبة والتعويضات الدقيقة عن الضرر المادي والمعنوي، وإدراج التواريخ الرسمية الدقيقة لبدء النزاع وتاريخ الإخلال بالتعهدات.",
        "type": "MissingCaseInfo"
      },
      {
        "id": "fcd523bb-aeef-46b9-958d-8d7508f79187",
        "description": "المستندات المحددة المطلوبة لإكمال الملف: أصل العقد/الاتفاق المبرم، صورة بطاقة الرقم القومي سارية لكل أطراف الدعوى، إيصالات التحويل أو السداد المالي، وأي إنذارات رسمية على يد محضر.",
        "type": "MissingCaseDoc"
      },
      {
        "id": "a0bf0c92-bbf8-494f-887e-53152fdce1ed",
        "description": "قم بإعادة هيكلة وصف القضية في صورة جدول زمني متسلسل، يبدأ من تاريخ التعهد الأول، مروراً بتاريخ الإخلال، وصولاً إلى حجم الأضرار المترتبة حالياً.",
        "type": "Suggestion"
      },
      {
        "id": "077e8007-f4e7-421b-8a55-094f62c2da02",
        "description": "قم بتفقيط وقسمة كافة المطالبات المالية إلى بنود مستقلة (أصل الدين، الفوائد أو التعويض عن المماطلة، والرسوم) وتوثيق كل بند بسند كتابي مستقل.",
        "type": "Suggestion"
      },
      {
        "id": "287f9d09-3425-4b88-a7b4-835c19ada8a0",
        "description": "قم بتنظيم وثائق الملف في مجلد مرتب حسب التاريخ، وتأكد من مسح الأوراق ضوئياً بدقة عالية لضمان سهولة الإسناد والفحص القضائي.",
        "type": "Suggestion"
      }
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **milestones-primary - review case** (status=200)
### milestones-primary - finalize case

**Request:** POST http://localhost:5049/api/Case/8454613c-7106-432c-bf00-a319395d94d4/finalize

**Body:**
```json
{}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "caseId": "8454613c-7106-432c-bf00-a319395d94d4",
    "totalEligibleLawyers": 0,
    "recommendations": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **milestones-primary - finalize case** (status=200)
### milestones-primary - create proposal

**Request:** POST http://localhost:5049/api/proposals

**Body:**
```json
{
  "LegalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
  "Message": "milestones-primary proposal for contract notification lifecycle.",
  "LawyerUserId": "7955b16b-9125-456d-f4e0-08def628c2bc"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "e63169c3-e99a-455e-b151-607f3d03600a",
    "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
    "caseTitle": "milestones-primary case 183444797",
    "clientUserId": "49a02ff4-6241-4855-f4df-08def628c2bc",
    "clientName": "Contracts Notifications client",
    "lawyerUserId": "7955b16b-9125-456d-f4e0-08def628c2bc",
    "lawyerName": "Contracts Notifications lawyer",
    "message": "milestones-primary proposal for contract notification lifecycle.",
    "status": "Pending",
    "decisionReason": null,
    "createdAt": "2026-08-09T15:34:47.5709549",
    "respondedAt": null,
    "updatedAt": "2026-08-09T15:34:47.5709549",
    "conversationId": null
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **milestones-primary - create proposal** (status=200)
### milestones-primary - accept proposal

**Request:** POST http://localhost:5049/api/proposals/e63169c3-e99a-455e-b151-607f3d03600a/accept

**Body:**
```json
{}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "e63169c3-e99a-455e-b151-607f3d03600a",
    "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
    "caseTitle": "milestones-primary case 183444797",
    "clientUserId": "49a02ff4-6241-4855-f4df-08def628c2bc",
    "clientName": "Contracts Notifications client",
    "lawyerUserId": "7955b16b-9125-456d-f4e0-08def628c2bc",
    "lawyerName": "Contracts Notifications lawyer",
    "message": "milestones-primary proposal for contract notification lifecycle.",
    "status": "Accepted",
    "decisionReason": null,
    "createdAt": "2026-08-09T15:34:47.5709549",
    "respondedAt": "2026-08-09T15:34:47.7076643",
    "updatedAt": "2026-08-09T15:34:47.7076643",
    "conversationId": "26b97971-7ecb-4a86-9b1c-03014bd5dcf2"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **milestones-primary - accept proposal** (status=200)
### milestones-primary - create contract

**Request:** POST http://localhost:5049/api/contracts

**Body:**
```json
{
  "Title": "milestones-primary legal representation contract",
  "ProposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
  "TermsAndConditions": "These complete contract terms are used for the milestones-primary notification lifecycle and are accepted by both participants."
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "4f48425d-4302-4ba8-9972-2fa711bce61c",
    "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
    "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
    "clientUserId": "49a02ff4-6241-4855-f4df-08def628c2bc",
    "lawyerUserId": "7955b16b-9125-456d-f4e0-08def628c2bc",
    "title": "milestones-primary legal representation contract",
    "termsAndConditions": "These complete contract terms are used for the milestones-primary notification lifecycle and are accepted by both participants.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": null,
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 0,
    "version": "\"AAAAAAAADgE=\"",
    "milestones": [],
    "payments": [],
    "permittedActions": [
      "Update",
      "Accept",
      "Terminate"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **milestones-primary - create contract** (status=200)
- [PASS] **milestones-primary create envelope retains logical 201**
### Add empty body

**Request:** POST http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c/milestones

**Body:**
```json
{}
```

**Response Status:** 400

**Response Body:**
```text
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 84 105 116 108 101 34 58 91 34 84 104 101 32 84 105 116 108 101 32 102 105 101 108 100 32 105 115 32 114 101 113 117 105 114 101 100 46 34 44 34 216 185 217 134 217 136 216 167 217 134 32 216 167 217 132 217 133 216 177 216 173 217 132 216 169 32 217 133 216 183 217 132 217 136 216 168 46 34 93 44 34 65 109 111 117 110 116 34 58 91 34 217 130 217 138 217 133 216 169 32 216 167 217 132 217 133 216 177 216 173 217 132 216 169 32 217 138 216 172 216 168 32 216 163 217 134 32 216 170 217 131 217 136 217 134 32 216 163 217 131 216 168 216 177 32 217 133 217 134 32 216 181 217 129 216 177 32 216 168 216 167 217 132 216 172 217 134 217 138 217 135 32 216 167 217 132 217 133 216 181 216 177 217 138 46 34 93 44 34 79 114 100 101 114 78 117 109 98 101 114 34 58 91 34 216 170 216 177 216 170 217 138 216 168 32 216 167 217 132 217 133 216 177 216 173 217 132 216 169 32 217 138 216 172 216 168 32 216 163 217 134 32 217 138 217 131 217 136 217 134 32 216 163 217 131 216 168 216 177 32 217 133 217 134 32 216 181 217 129 216 177 46 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 49 53 57 49 54 57 99 49 97 51 53 97 53 100 97 101 53 98 102 51 57 98 55 99 52 98 53 56 56 99 55 52 45 101 56 99 51 53 49 51 48 57 49 56 48 102 48 48 56 45 48 48 34 125
```
---


- [PASS] **Add empty body returns 400** (status=400)
### Add negative and past values

**Request:** POST http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c/milestones

**Body:**
```json
{
  "OrderNumber": -1,
  "Title": "Bad",
  "DueDate": "2000-01-01T00:00:00Z",
  "DurationDays": 0,
  "Amount": -10
}
```

**Response Status:** 400

**Response Body:**
```text
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 65 109 111 117 110 116 34 58 91 34 217 130 217 138 217 133 216 169 32 216 167 217 132 217 133 216 177 216 173 217 132 216 169 32 217 138 216 172 216 168 32 216 163 217 134 32 216 170 217 131 217 136 217 134 32 216 163 217 131 216 168 216 177 32 217 133 217 134 32 216 181 217 129 216 177 32 216 168 216 167 217 132 216 172 217 134 217 138 217 135 32 216 167 217 132 217 133 216 181 216 177 217 138 46 34 93 44 34 68 117 101 68 97 116 101 34 58 91 34 216 170 216 167 216 177 217 138 216 174 32 216 167 216 179 216 170 216 173 217 130 216 167 217 130 32 216 167 217 132 217 133 216 177 216 173 217 132 216 169 32 217 138 216 172 216 168 32 216 163 217 134 32 217 138 217 131 217 136 217 134 32 217 129 217 138 32 216 167 217 132 217 133 216 179 216 170 217 130 216 168 217 132 46 34 93 44 34 79 114 100 101 114 78 117 109 98 101 114 34 58 91 34 216 170 216 177 216 170 217 138 216 168 32 216 167 217 132 217 133 216 177 216 173 217 132 216 169 32 217 138 216 172 216 168 32 216 163 217 134 32 217 138 217 131 217 136 217 134 32 216 163 217 131 216 168 216 177 32 217 133 217 134 32 216 181 217 129 216 177 46 34 93 44 34 68 117 114 97 116 105 111 110 68 97 121 115 34 58 91 34 217 133 216 175 216 169 32 216 167 217 132 217 133 216 177 216 173 217 132 216 169 32 217 138 216 172 216 168 32 216 163 217 134 32 216 170 217 131 217 136 217 134 32 216 168 217 138 217 134 32 217 138 217 136 217 133 32 217 136 216 167 216 173 216 175 32 217 136 51 54 53 32 217 138 217 136 217 133 217 139 216 167 46 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 50 102 57 53 51 51 57 99 50 100 57 97 49 56 54 48 54 55 51 101 102 102 97 49 98 97 48 54 56 57 48 99 45 99 48 49 54 100 55 98 57 53 52 54 53 48 51 57 55 45 48 48 34 125
```
---


- [PASS] **Add negative and past values returns 400** (status=400)
### Add extreme title

**Request:** POST http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c/milestones

**Body:**
```json
{
  "OrderNumber": 1,
  "Title": "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
  "Description": "valid",
  "DurationDays": 1,
  "Amount": 10
}
```

**Response Status:** 400

**Response Body:**
```text
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 84 105 116 108 101 34 58 91 34 216 185 217 134 217 136 216 167 217 134 32 216 167 217 132 217 133 216 177 216 173 217 132 216 169 32 217 138 216 172 216 168 32 216 163 217 134 32 217 138 217 131 217 136 217 134 32 216 168 217 138 217 134 32 51 32 217 136 50 48 48 32 216 173 216 177 217 129 46 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 52 101 51 102 50 56 51 99 53 98 56 54 50 53 48 98 54 98 98 101 57 57 52 52 49 50 57 49 101 100 52 57 45 54 51 53 100 98 102 54 55 99 49 50 52 99 57 51 102 45 48 48 34 125
```
---


- [PASS] **Add extreme title returns 400** (status=400)
### Add type mismatch

**Request:** POST http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c/milestones

**Body:**
```json
{
  "title": "مرحلة",
  "orderNumber": "bad",
  "amount": "bad"
}
```

**Response Status:** 400

**Response Body:**
```text
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 114 101 113 117 101 115 116 34 58 91 34 84 104 101 32 114 101 113 117 101 115 116 32 102 105 101 108 100 32 105 115 32 114 101 113 117 105 114 101 100 46 34 93 44 34 36 46 111 114 100 101 114 78 117 109 98 101 114 34 58 91 34 84 104 101 32 74 83 79 78 32 118 97 108 117 101 32 99 111 117 108 100 32 110 111 116 32 98 101 32 99 111 110 118 101 114 116 101 100 32 116 111 32 83 109 97 114 116 67 111 117 114 116 46 70 101 97 116 117 114 101 115 46 77 105 108 101 115 116 111 110 101 115 46 68 84 79 115 46 65 100 100 77 105 108 101 115 116 111 110 101 82 101 113 117 101 115 116 46 32 80 97 116 104 58 32 36 46 111 114 100 101 114 78 117 109 98 101 114 32 124 32 76 105 110 101 78 117 109 98 101 114 58 32 48 32 124 32 66 121 116 101 80 111 115 105 116 105 111 110 73 110 76 105 110 101 58 32 52 49 46 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 50 55 98 99 50 53 54 50 98 100 54 57 50 99 53 52 97 51 53 48 54 52 55 50 102 48 98 53 51 48 55 57 45 50 97 49 56 49 101 102 56 98 49 99 57 57 52 57 51 45 48 48 34 125
```
---


- [PASS] **Add type mismatch returns 400** (status=400)
### Add hostile payload

**Request:** POST http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c/milestones

**Body:**
```json
{
  "OrderNumber": 0,
  "Title": "<script>alert(1)</script>'' OR 1=1--",
  "Description": "Valid hostile-looking text",
  "DurationDays": 366,
  "Amount": 1.001
}
```

**Response Status:** 400

**Response Body:**
```text
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 65 109 111 117 110 116 34 58 91 34 217 130 217 138 217 133 216 169 32 216 167 217 132 217 133 216 177 216 173 217 132 216 169 32 217 138 216 172 216 168 32 216 163 217 132 216 167 32 216 170 216 170 216 172 216 167 217 136 216 178 32 217 133 217 134 216 178 217 132 216 170 217 138 217 134 32 216 185 216 180 216 177 217 138 216 170 217 138 217 134 46 34 93 44 34 79 114 100 101 114 78 117 109 98 101 114 34 58 91 34 216 170 216 177 216 170 217 138 216 168 32 216 167 217 132 217 133 216 177 216 173 217 132 216 169 32 217 138 216 172 216 168 32 216 163 217 134 32 217 138 217 131 217 136 217 134 32 216 163 217 131 216 168 216 177 32 217 133 217 134 32 216 181 217 129 216 177 46 34 93 44 34 68 117 114 97 116 105 111 110 68 97 121 115 34 58 91 34 217 133 216 175 216 169 32 216 167 217 132 217 133 216 177 216 173 217 132 216 169 32 217 138 216 172 216 168 32 216 163 217 134 32 216 170 217 131 217 136 217 134 32 216 168 217 138 217 134 32 217 138 217 136 217 133 32 217 136 216 167 216 173 216 175 32 217 136 51 54 53 32 217 138 217 136 217 133 217 139 216 167 46 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 51 99 99 49 53 49 97 97 99 48 102 52 48 51 56 100 99 55 100 50 99 55 55 102 52 55 100 102 50 102 97 99 45 99 98 97 98 48 102 51 100 102 48 98 55 53 99 50 56 45 48 48 34 125
```
---


- [PASS] **Add hostile payload returns 400** (status=400)
### Unrelated user cannot add milestone

**Request:** POST http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c/milestones

**Body:**
```json
{
  "DueDate": "2026-08-29T15:34:48.2962535Z",
  "OrderNumber": 1,
  "Description": "وصف عربي شامل للمرحلة الأولى.",
  "Title": "المرحلة الأولى لتنفيذ الأعمال",
  "DurationDays": 10,
  "Amount": 1000.0
}
```

**Response Status:** 403

**Response Body:**
```json
{
  "success": false,
  "data": null,
  "message": "غير مصرح لك بالاطلاع على هذا العقد.",
  "errors": null,
  "statusCode": 403
}
```
---


- [PASS] **Unrelated add is forbidden** (status=403)
### Lawyer adds milestone

**Request:** POST http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c/milestones

**Body:**
```json
{
  "DueDate": "2026-08-29T15:34:48.2962535Z",
  "OrderNumber": 1,
  "Description": "وصف عربي شامل للمرحلة الأولى.",
  "Title": "المرحلة الأولى لتنفيذ الأعمال",
  "DurationDays": 10,
  "Amount": 1000.0
}
```

**Response Status:** 201

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
    "orderNumber": 1,
    "title": "المرحلة الأولى لتنفيذ الأعمال",
    "description": "وصف عربي شامل للمرحلة الأولى.",
    "amount": 1000.0,
    "durationDays": 10,
    "dueDate": "2026-08-29T15:34:48.2962535Z",
    "status": 0,
    "fundingStatus": 0,
    "escrowHoldId": null,
    "fundedAt": null,
    "submittedAt": null,
    "autoAcceptEligibleAt": null,
    "holdExpiresAt": null,
    "netLawyerAmount": null,
    "version": "\"AAAAAAAADgM=\"",
    "permittedActions": [
      "Update",
      "Approve"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **Lawyer adds milestone** (status=201)
### Poll client for milestone creation

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "4b117158-e888-4117-a27d-908c9f0e1f08",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.7168952",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 1
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll client for milestone creation

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "50bb9c8d-5e13-43cd-9b67-c441da71a4b7",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:48.4656041",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "3d653c37-99ec-4cc2-a62e-578c78417519",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.8428007",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4b117158-e888-4117-a27d-908c9f0e1f08",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.7168952",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 3
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client receives exact milestone creation**

## List, update, ownership, concurrency, and draft notifications

### Client lists milestones

**Request:** GET http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
      "orderNumber": 1,
      "title": "المرحلة الأولى لتنفيذ الأعمال",
      "description": "وصف عربي شامل للمرحلة الأولى.",
      "amount": 1000.0,
      "durationDays": 10,
      "dueDate": "2026-08-29T15:34:48.2962535",
      "status": 0,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAADgM=\"",
      "permittedActions": [
        "Update",
        "Approve"
      ]
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client lists milestones** (status=200)
- [PASS] **Client list contains milestone**
### Unrelated user cannot list milestones

**Request:** GET http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c/milestones

**Response Status:** 403

**Response Body:**
```json
{
  "success": false,
  "data": null,
  "message": "غير مصرح لك بالاطلاع على هذا العقد.",
  "errors": null,
  "statusCode": 403
}
```
---


- [PASS] **Unrelated list is forbidden** (status=403)
### Get milestone for update

**Request:** GET http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
      "orderNumber": 1,
      "title": "المرحلة الأولى لتنفيذ الأعمال",
      "description": "وصف عربي شامل للمرحلة الأولى.",
      "amount": 1000.0,
      "durationDays": 10,
      "dueDate": "2026-08-29T15:34:48.2962535",
      "status": 0,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAADgM=\"",
      "permittedActions": [
        "Update",
        "Approve"
      ]
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Get milestone for update** (status=200)
- [PASS] **Get milestone for update contains target milestone**
### Update missing If-Match

**Request:** PUT http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c/milestones/cf3fc657-597d-4ab2-a4af-d610db9a9d57

**Body:**
```json
{
  "DurationDays": 12,
  "Title": "عنوان صالح"
}
```

**Response Status:** 412

**Response Body:**
```json
{
  "success": false,
  "data": null,
  "message": "قيمة If-Match مطلوبة.",
  "errors": null,
  "statusCode": 412
}
```
---


- [PASS] **Update missing If-Match returns 412** (status=412)
### Unrelated user cannot update milestone

**Request:** PUT http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c/milestones/cf3fc657-597d-4ab2-a4af-d610db9a9d57

**Body:**
```json
{
  "DurationDays": 12,
  "Title": "عنوان صالح"
}
```

**Response Status:** 403

**Response Body:**
```json
{
  "success": false,
  "data": null,
  "message": "غير مصرح لك بالاطلاع على هذا العقد.",
  "errors": null,
  "statusCode": 403
}
```
---


- [PASS] **Unrelated update is forbidden** (status=403)
### Client updates milestone draft

**Request:** PUT http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c/milestones/cf3fc657-597d-4ab2-a4af-d610db9a9d57

**Body:**
```json
{
  "Title": "المرحلة الأولى بعد التحديث",
  "Description": "وصف عربي محدث وآمن للمرحلة.",
  "DurationDays": 12,
  "DueDate": "2026-09-03T15:34:49.9337079Z"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
    "orderNumber": 1,
    "title": "المرحلة الأولى بعد التحديث",
    "description": "وصف عربي محدث وآمن للمرحلة.",
    "amount": 1000.0,
    "durationDays": 12,
    "dueDate": "2026-09-03T15:34:49.9337079Z",
    "status": 0,
    "fundingStatus": 0,
    "escrowHoldId": null,
    "fundedAt": null,
    "submittedAt": null,
    "autoAcceptEligibleAt": null,
    "holdExpiresAt": null,
    "netLawyerAmount": null,
    "version": "\"AAAAAAAADgs=\"",
    "permittedActions": [
      "Update",
      "Approve"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client updates milestone draft** (status=200)
### Poll lawyer for milestone update

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "5d9f29c5-e4f8-4f89-ae66-abc62b9a1c1a",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.5711378",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 1
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll lawyer for milestone update

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "7717c90a-5764-4322-b058-04ab901348b7",
        "type": "milestone.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث المرحلة",
        "body": "تم تحديث شروط المرحلة، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:50.0378639",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5d9f29c5-e4f8-4f89-ae66-abc62b9a1c1a",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.5711378",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 2
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Lawyer receives exact milestone update**
### Stale update is rejected

**Request:** PUT http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c/milestones/cf3fc657-597d-4ab2-a4af-d610db9a9d57

**Body:**
```json
{
  "DurationDays": 13,
  "Title": "عنوان آخر"
}
```

**Response Status:** 409

**Response Body:**
```json
{
  "success": false,
  "data": null,
  "message": "تم تعديل المرحلة بواسطة عملية أخرى. يرجى إعادة تحميلها والمحاولة مرة أخرى.",
  "errors": null,
  "statusCode": 409
}
```
---


- [PASS] **Stale update returns 412** (status=409)

## Participant approval and approved notifications

### Get milestone for lawyer approval

**Request:** GET http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
      "orderNumber": 1,
      "title": "المرحلة الأولى بعد التحديث",
      "description": "وصف عربي محدث وآمن للمرحلة.",
      "amount": 1000.0,
      "durationDays": 12,
      "dueDate": "2026-09-03T15:34:49.9337079",
      "status": 0,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAADgs=\"",
      "permittedActions": [
        "Update",
        "Approve"
      ]
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Get milestone for lawyer approval** (status=200)
- [PASS] **Get milestone for lawyer approval contains target milestone**
### Lawyer approves milestone

**Request:** POST http://localhost:5049/api/milestones/cf3fc657-597d-4ab2-a4af-d610db9a9d57/approve

**Body:**
```json
{}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "entityId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
    "status": "Draft",
    "occurredAt": "2026-08-09T15:34:51.3581006Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Lawyer approves milestone** (status=200)
### Poll client for milestone acceptance

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "50bb9c8d-5e13-43cd-9b67-c441da71a4b7",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:48.4656041",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "3d653c37-99ec-4cc2-a62e-578c78417519",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.8428007",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4b117158-e888-4117-a27d-908c9f0e1f08",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.7168952",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 3
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll client for milestone acceptance

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "bf078e91-f5af-4015-8573-ef6170bbf566",
        "type": "milestone.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على المرحلة",
        "body": "وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:51.3581465",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "50bb9c8d-5e13-43cd-9b67-c441da71a4b7",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:48.4656041",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "3d653c37-99ec-4cc2-a62e-578c78417519",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.8428007",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4b117158-e888-4117-a27d-908c9f0e1f08",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.7168952",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 4
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client receives first milestone approval**
### Get milestone for client approval

**Request:** GET http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
      "orderNumber": 1,
      "title": "المرحلة الأولى بعد التحديث",
      "description": "وصف عربي محدث وآمن للمرحلة.",
      "amount": 1000.0,
      "durationDays": 12,
      "dueDate": "2026-09-03T15:34:49.9337079",
      "status": 0,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAADhA=\"",
      "permittedActions": [
        "Update",
        "Approve"
      ]
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Get milestone for client approval** (status=200)
- [PASS] **Get milestone for client approval contains target milestone**
### Attacker cannot approve milestone

**Request:** POST http://localhost:5049/api/milestones/cf3fc657-597d-4ab2-a4af-d610db9a9d57/approve

**Body:**
```json
{}
```

**Response Status:** 403

**Response Body:**
```json
{
  "success": false,
  "data": null,
  "message": "غير مصرح لك بالاطلاع على هذا العقد.",
  "errors": null,
  "statusCode": 403
}
```
---


- [PASS] **Attacker approval is forbidden** (status=403)
### Client completes milestone approval

**Request:** POST http://localhost:5049/api/milestones/cf3fc657-597d-4ab2-a4af-d610db9a9d57/approve

**Body:**
```json
{}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "entityId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
    "status": "AwaitingFunding",
    "occurredAt": "2026-08-09T15:34:52.4909226Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client completes milestone approval** (status=200)
### Poll for Client receives milestone approved

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "bf078e91-f5af-4015-8573-ef6170bbf566",
        "type": "milestone.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على المرحلة",
        "body": "وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:51.3581465",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "50bb9c8d-5e13-43cd-9b67-c441da71a4b7",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:48.4656041",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "3d653c37-99ec-4cc2-a62e-578c78417519",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.8428007",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4b117158-e888-4117-a27d-908c9f0e1f08",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.7168952",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 4
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll for Client receives milestone approved

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "bf078e91-f5af-4015-8573-ef6170bbf566",
        "type": "milestone.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على المرحلة",
        "body": "وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:51.3581465",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "50bb9c8d-5e13-43cd-9b67-c441da71a4b7",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:48.4656041",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "3d653c37-99ec-4cc2-a62e-578c78417519",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.8428007",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4b117158-e888-4117-a27d-908c9f0e1f08",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.7168952",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 4
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll for Client receives milestone approved

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "d51a0203-c2b6-432d-bbbb-38fbc4455b05",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "bf078e91-f5af-4015-8573-ef6170bbf566",
        "type": "milestone.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على المرحلة",
        "body": "وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:51.3581465",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "50bb9c8d-5e13-43cd-9b67-c441da71a4b7",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:48.4656041",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "3d653c37-99ec-4cc2-a62e-578c78417519",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.8428007",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4b117158-e888-4117-a27d-908c9f0e1f08",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.7168952",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 5
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client receives milestone approved**
### Poll for Lawyer receives milestone approved

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "5f5b4323-733d-4ea1-9ba3-9ff023c517cd",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "7717c90a-5764-4322-b058-04ab901348b7",
        "type": "milestone.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث المرحلة",
        "body": "تم تحديث شروط المرحلة، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:50.0378639",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5d9f29c5-e4f8-4f89-ae66-abc62b9a1c1a",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.5711378",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 3
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Lawyer receives milestone approved**

## Activation, ready-for-funding, and funded execution

### milestones-primary - contract ETag for client acceptance

**Request:** GET http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "4f48425d-4302-4ba8-9972-2fa711bce61c",
    "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
    "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
    "clientUserId": "49a02ff4-6241-4855-f4df-08def628c2bc",
    "lawyerUserId": "7955b16b-9125-456d-f4e0-08def628c2bc",
    "title": "milestones-primary legal representation contract",
    "termsAndConditions": "These complete contract terms are used for the milestones-primary notification lifecycle and are accepted by both participants.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": null,
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 1000.0,
    "version": "\"AAAAAAAADgE=\"",
    "milestones": [
      {
        "id": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
        "orderNumber": 1,
        "title": "المرحلة الأولى بعد التحديث",
        "description": "وصف عربي محدث وآمن للمرحلة.",
        "amount": 1000.0,
        "durationDays": 12,
        "dueDate": "2026-09-03T15:34:49.9337079",
        "status": 1,
        "fundingStatus": 0,
        "escrowHoldId": null,
        "fundedAt": null,
        "submittedAt": null,
        "autoAcceptEligibleAt": null,
        "holdExpiresAt": null,
        "netLawyerAmount": null,
        "version": "\"AAAAAAAADhU=\""
      }
    ],
    "payments": [],
    "permittedActions": [
      "Update",
      "Accept",
      "Terminate"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **milestones-primary - contract ETag for client acceptance** (status=200)
### milestones-primary - client accepts contract

**Request:** POST http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c/accept

**Body:**
```json
{}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "entityId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
    "status": "Draft",
    "occurredAt": "2026-08-09T15:34:54.8575815Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **milestones-primary - client accepts contract** (status=200)
### milestones-primary - contract ETag for lawyer acceptance

**Request:** GET http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "4f48425d-4302-4ba8-9972-2fa711bce61c",
    "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
    "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
    "clientUserId": "49a02ff4-6241-4855-f4df-08def628c2bc",
    "lawyerUserId": "7955b16b-9125-456d-f4e0-08def628c2bc",
    "title": "milestones-primary legal representation contract",
    "termsAndConditions": "These complete contract terms are used for the milestones-primary notification lifecycle and are accepted by both participants.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": "2026-08-09T15:34:54.8575815",
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 1000.0,
    "version": "\"AAAAAAAADh4=\"",
    "milestones": [
      {
        "id": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
        "orderNumber": 1,
        "title": "المرحلة الأولى بعد التحديث",
        "description": "وصف عربي محدث وآمن للمرحلة.",
        "amount": 1000.0,
        "durationDays": 12,
        "dueDate": "2026-09-03T15:34:49.9337079",
        "status": 1,
        "fundingStatus": 0,
        "escrowHoldId": null,
        "fundedAt": null,
        "submittedAt": null,
        "autoAcceptEligibleAt": null,
        "holdExpiresAt": null,
        "netLawyerAmount": null,
        "version": "\"AAAAAAAADhU=\""
      }
    ],
    "payments": [],
    "permittedActions": [
      "Update",
      "Accept",
      "Terminate"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **milestones-primary - contract ETag for lawyer acceptance** (status=200)
### milestones-primary - lawyer accepts contract

**Request:** POST http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c/accept

**Body:**
```json
{}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "entityId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
    "status": "Active",
    "occurredAt": "2026-08-09T15:34:55.0704924Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **milestones-primary - lawyer accepts contract** (status=200)
### Get milestone for ready funding

**Request:** GET http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
      "orderNumber": 1,
      "title": "المرحلة الأولى بعد التحديث",
      "description": "وصف عربي محدث وآمن للمرحلة.",
      "amount": 1000.0,
      "durationDays": 12,
      "dueDate": "2026-09-03T15:34:49.9337079",
      "status": 1,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAADhU=\"",
      "permittedActions": [
        "ReadyForFunding"
      ]
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Get milestone for ready funding** (status=200)
- [PASS] **Get milestone for ready funding contains target milestone**
### Client cannot mark ready

**Request:** POST http://localhost:5049/api/milestones/cf3fc657-597d-4ab2-a4af-d610db9a9d57/ready-for-funding

**Body:**
```json
{}
```

**Response Status:** 403

**Response Body:**
(Empty)
---


- [PASS] **Client ready-for-funding is forbidden** (status=403)
### milestones-primary - list milestone before funding

**Request:** GET http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
      "orderNumber": 1,
      "title": "المرحلة الأولى بعد التحديث",
      "description": "وصف عربي محدث وآمن للمرحلة.",
      "amount": 1000.0,
      "durationDays": 12,
      "dueDate": "2026-09-03T15:34:49.9337079",
      "status": 1,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAADhU=\"",
      "permittedActions": [
        "ReadyForFunding"
      ]
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **milestones-primary - list milestone before funding** (status=200)
### milestones-primary - mark milestone ready for funding

**Request:** POST http://localhost:5049/api/milestones/cf3fc657-597d-4ab2-a4af-d610db9a9d57/ready-for-funding

**Body:**
```json
{}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "entityId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
    "status": "AwaitingFunding",
    "occurredAt": "2026-08-09T15:34:55.5924033Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **milestones-primary - mark milestone ready for funding** (status=200)
### milestones-primary - fund milestone through mock provider

**Request:** POST http://localhost:5049/api/milestones/cf3fc657-597d-4ab2-a4af-d610db9a9d57/fund

**Body:**
```json
{
  "PaymentMethodReference": "[REDACTED]"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "a0798185-9d30-4f95-b89e-febd6daa7ebd",
    "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
    "grossAmount": 1000.0,
    "platformFee": 50.0,
    "netAmount": 950.0,
    "currency": "EGP",
    "status": 0,
    "holdExpiresAt": null,
    "settledAt": null
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **milestones-primary - fund milestone through mock provider** (status=200)
### Poll client for ready funding

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "567162f0-c05c-490d-87a9-d16a7e606ebe",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "d51a0203-c2b6-432d-bbbb-38fbc4455b05",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "bf078e91-f5af-4015-8573-ef6170bbf566",
        "type": "milestone.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على المرحلة",
        "body": "وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:51.3581465",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "50bb9c8d-5e13-43cd-9b67-c441da71a4b7",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:48.4656041",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "3d653c37-99ec-4cc2-a62e-578c78417519",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.8428007",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4b117158-e888-4117-a27d-908c9f0e1f08",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.7168952",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 6
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll client for ready funding

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "23e10d77-6701-4a6c-9ee1-952a82e64350",
        "type": "milestone.ready-for-funding",
        "severity": "Information",
        "title": "المرحلة جاهزة للتمويل",
        "body": "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.592442",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "567162f0-c05c-490d-87a9-d16a7e606ebe",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "d51a0203-c2b6-432d-bbbb-38fbc4455b05",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "bf078e91-f5af-4015-8573-ef6170bbf566",
        "type": "milestone.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على المرحلة",
        "body": "وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:51.3581465",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "50bb9c8d-5e13-43cd-9b67-c441da71a4b7",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:48.4656041",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "3d653c37-99ec-4cc2-a62e-578c78417519",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.8428007",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4b117158-e888-4117-a27d-908c9f0e1f08",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.7168952",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 7
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client receives ready-for-funding**

## Formal change-request endpoints and notifications

### Get for invalid CR

**Request:** GET http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
      "orderNumber": 1,
      "title": "المرحلة الأولى بعد التحديث",
      "description": "وصف عربي محدث وآمن للمرحلة.",
      "amount": 1000.0,
      "durationDays": 12,
      "dueDate": "2026-09-03T15:34:49.9337079",
      "status": 3,
      "fundingStatus": 2,
      "escrowHoldId": "a0798185-9d30-4f95-b89e-febd6daa7ebd",
      "fundedAt": "2026-08-09T15:34:55.7934504",
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": 950.0,
      "version": "\"AAAAAAAADjQ=\"",
      "permittedActions": []
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Get for invalid CR** (status=200)
- [PASS] **Get for invalid CR contains target milestone**
### Empty change request is invalid

**Request:** POST http://localhost:5049/api/milestones/cf3fc657-597d-4ab2-a4af-d610db9a9d57/change-requests

**Body:**
```json
{}
```

**Response Status:** 400

**Response Body:**
```text
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 34 58 91 34 217 138 216 172 216 168 32 216 163 217 134 32 217 138 216 170 216 182 217 133 217 134 32 216 183 217 132 216 168 32 216 167 217 132 216 170 216 185 216 175 217 138 217 132 32 216 170 216 186 217 138 217 138 216 177 217 139 216 167 32 217 136 216 167 216 173 216 175 217 139 216 167 32 216 185 217 132 217 137 32 216 167 217 132 216 163 217 130 217 132 46 34 93 44 34 82 101 97 115 111 110 34 58 91 34 84 104 101 32 82 101 97 115 111 110 32 102 105 101 108 100 32 105 115 32 114 101 113 117 105 114 101 100 46 34 44 34 216 179 216 168 216 168 32 216 183 217 132 216 168 32 216 167 217 132 216 170 216 185 216 175 217 138 217 132 32 217 133 216 183 217 132 217 136 216 168 46 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 56 57 51 97 100 57 57 98 52 52 56 99 52 54 49 97 100 54 48 50 48 56 97 102 98 55 52 54 56 49 100 50 45 55 49 99 49 102 97 49 55 48 97 97 57 49 48 102 57 45 48 48 34 125
```
---


- [PASS] **Empty change request returns 400** (status=400)
### client CR for approval list for milestone ETag

**Request:** GET http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
      "orderNumber": 1,
      "title": "المرحلة الأولى بعد التحديث",
      "description": "وصف عربي محدث وآمن للمرحلة.",
      "amount": 1000.0,
      "durationDays": 12,
      "dueDate": "2026-09-03T15:34:49.9337079",
      "status": 3,
      "fundingStatus": 2,
      "escrowHoldId": "a0798185-9d30-4f95-b89e-febd6daa7ebd",
      "fundedAt": "2026-08-09T15:34:55.7934504",
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": 950.0,
      "version": "\"AAAAAAAADjQ=\"",
      "permittedActions": []
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **client CR for approval list for milestone ETag** (status=200)
- [PASS] **client CR for approval list for milestone ETag contains target milestone**
### client CR for approval create change request

**Request:** POST http://localhost:5049/api/milestones/cf3fc657-597d-4ab2-a4af-d610db9a9d57/change-requests

**Body:**
```json
{
  "ProposedDescription": "وصف جديد بعد موافقة الطرف الآخر.",
  "ProposedDurationDays": 20,
  "Reason": "سبب واضح ومحدد لطلب تعديل شروط المرحلة."
}
```

**Response Status:** 201

**Response Body:**
```json
{
  "success": true,
  "data": {
    "entityId": "2970841a-eee0-4904-a1b8-bc5b6a9bf67d",
    "status": "Pending",
    "occurredAt": "2026-08-09T15:34:57.2558044Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **client CR for approval create change request** (status=201)
### Poll lawyer for created CR

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "2047772e-18bc-49ba-b4d9-29bc500c19d8",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "01245fe7-9b7f-48ba-ad5b-08fa86278ec9",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:54.8678438",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5f5b4323-733d-4ea1-9ba3-9ff023c517cd",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "7717c90a-5764-4322-b058-04ab901348b7",
        "type": "milestone.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث المرحلة",
        "body": "تم تحديث شروط المرحلة، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:50.0378639",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5d9f29c5-e4f8-4f89-ae66-abc62b9a1c1a",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.5711378",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 5
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll lawyer for created CR

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "f20d165c-64f5-4373-9406-38f62219b42f",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "2970841a-eee0-4904-a1b8-bc5b6a9bf67d"
        },
        "createdAtUtc": "2026-08-09T15:34:57.2565009",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "2047772e-18bc-49ba-b4d9-29bc500c19d8",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "01245fe7-9b7f-48ba-ad5b-08fa86278ec9",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:54.8678438",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5f5b4323-733d-4ea1-9ba3-9ff023c517cd",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "7717c90a-5764-4322-b058-04ab901348b7",
        "type": "milestone.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث المرحلة",
        "body": "تم تحديث شروط المرحلة، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:50.0378639",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5d9f29c5-e4f8-4f89-ae66-abc62b9a1c1a",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.5711378",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 6
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Lawyer receives created change request**
### Lawyer approves change request

**Request:** POST http://localhost:5049/api/change-requests/2970841a-eee0-4904-a1b8-bc5b6a9bf67d/approve

**Body:**
```json
{}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "entityId": "2970841a-eee0-4904-a1b8-bc5b6a9bf67d",
    "status": "Approved",
    "occurredAt": "2026-08-09T15:34:58.7537761Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Lawyer approves change request** (status=200)
### Poll client for approved CR

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "23e10d77-6701-4a6c-9ee1-952a82e64350",
        "type": "milestone.ready-for-funding",
        "severity": "Information",
        "title": "المرحلة جاهزة للتمويل",
        "body": "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.592442",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "567162f0-c05c-490d-87a9-d16a7e606ebe",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "d51a0203-c2b6-432d-bbbb-38fbc4455b05",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "bf078e91-f5af-4015-8573-ef6170bbf566",
        "type": "milestone.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على المرحلة",
        "body": "وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:51.3581465",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "50bb9c8d-5e13-43cd-9b67-c441da71a4b7",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:48.4656041",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "3d653c37-99ec-4cc2-a62e-578c78417519",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.8428007",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4b117158-e888-4117-a27d-908c9f0e1f08",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.7168952",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 7
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll client for approved CR

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "976f0e7b-f3dd-40c2-aedd-cfece1f2ed2c",
        "type": "milestone.change-request-approved",
        "severity": "Success",
        "title": "تمت الموافقة على طلب التعديل",
        "body": "وافق الطرف الآخر على طلب تعديل المرحلة، وطُبّقت الشروط المعتمدة.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "2970841a-eee0-4904-a1b8-bc5b6a9bf67d"
        },
        "createdAtUtc": "2026-08-09T15:34:58.7538494",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "23e10d77-6701-4a6c-9ee1-952a82e64350",
        "type": "milestone.ready-for-funding",
        "severity": "Information",
        "title": "المرحلة جاهزة للتمويل",
        "body": "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.592442",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "567162f0-c05c-490d-87a9-d16a7e606ebe",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "d51a0203-c2b6-432d-bbbb-38fbc4455b05",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "bf078e91-f5af-4015-8573-ef6170bbf566",
        "type": "milestone.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على المرحلة",
        "body": "وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:51.3581465",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "50bb9c8d-5e13-43cd-9b67-c441da71a4b7",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:48.4656041",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "3d653c37-99ec-4cc2-a62e-578c78417519",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.8428007",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4b117158-e888-4117-a27d-908c9f0e1f08",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.7168952",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 8
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client receives approved change request**
### lawyer CR for rejection list for milestone ETag

**Request:** GET http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
      "orderNumber": 1,
      "title": "المرحلة الأولى بعد التحديث",
      "description": "وصف جديد بعد موافقة الطرف الآخر.",
      "amount": 1000.0,
      "durationDays": 20,
      "dueDate": "2026-09-03T15:34:49.9337079",
      "status": 3,
      "fundingStatus": 2,
      "escrowHoldId": "a0798185-9d30-4f95-b89e-febd6daa7ebd",
      "fundedAt": "2026-08-09T15:34:55.7934504",
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": 950.0,
      "version": "\"AAAAAAAADkY=\"",
      "permittedActions": [
        "Submit"
      ]
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **lawyer CR for rejection list for milestone ETag** (status=200)
- [PASS] **lawyer CR for rejection list for milestone ETag contains target milestone**
### lawyer CR for rejection create change request

**Request:** POST http://localhost:5049/api/milestones/cf3fc657-597d-4ab2-a4af-d610db9a9d57/change-requests

**Body:**
```json
{
  "ProposedDescription": "وصف يقترحه المحامي ليختبر الرفض.",
  "ProposedDurationDays": 25,
  "Reason": "سبب واضح ومحدد لطلب تعديل شروط المرحلة."
}
```

**Response Status:** 201

**Response Body:**
```json
{
  "success": true,
  "data": {
    "entityId": "21765f91-670d-4888-bce9-9cde08249716",
    "status": "Pending",
    "occurredAt": "2026-08-09T15:34:59.9615412Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **lawyer CR for rejection create change request** (status=201)
### Reject CR missing reason

**Request:** POST http://localhost:5049/api/change-requests/21765f91-670d-4888-bce9-9cde08249716/reject

**Body:**
```json
{}
```

**Response Status:** 400

**Response Body:**
```text
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 82 101 97 115 111 110 34 58 91 34 84 104 101 32 82 101 97 115 111 110 32 102 105 101 108 100 32 105 115 32 114 101 113 117 105 114 101 100 46 34 44 34 216 179 216 168 216 168 32 216 177 217 129 216 182 32 216 183 217 132 216 168 32 216 167 217 132 216 170 216 185 216 175 217 138 217 132 32 217 133 216 183 217 132 217 136 216 168 46 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 49 99 100 56 102 56 98 57 55 57 52 99 97 52 98 55 101 102 98 56 102 52 100 99 53 55 101 49 98 57 51 98 45 51 53 53 48 51 97 50 53 52 51 51 50 98 54 50 102 45 48 48 34 125
```
---


- [PASS] **Reject CR missing reason returns 400** (status=400)
### Client rejects change request

**Request:** POST http://localhost:5049/api/change-requests/21765f91-670d-4888-bce9-9cde08249716/reject

**Body:**
```json
{
  "Reason": "لا تتوافق التعديلات المقترحة مع نطاق المرحلة."
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "entityId": "21765f91-670d-4888-bce9-9cde08249716",
    "status": "Rejected",
    "occurredAt": "2026-08-09T15:35:00.4503089Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client rejects change request** (status=200)
### Poll lawyer for rejected CR

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "f20d165c-64f5-4373-9406-38f62219b42f",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "2970841a-eee0-4904-a1b8-bc5b6a9bf67d"
        },
        "createdAtUtc": "2026-08-09T15:34:57.2565009",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "2047772e-18bc-49ba-b4d9-29bc500c19d8",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "01245fe7-9b7f-48ba-ad5b-08fa86278ec9",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:54.8678438",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5f5b4323-733d-4ea1-9ba3-9ff023c517cd",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "7717c90a-5764-4322-b058-04ab901348b7",
        "type": "milestone.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث المرحلة",
        "body": "تم تحديث شروط المرحلة، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:50.0378639",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5d9f29c5-e4f8-4f89-ae66-abc62b9a1c1a",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.5711378",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 6
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll lawyer for rejected CR

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "c66eec0b-7862-4697-9219-5b536aa1db66",
        "type": "milestone.change-request-rejected",
        "severity": "Warning",
        "title": "تم رفض طلب تعديل المرحلة",
        "body": "رفض الطرف الآخر طلب تعديل المرحلة. يمكنك مراجعة الطلب لمعرفة التفاصيل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "21765f91-670d-4888-bce9-9cde08249716"
        },
        "createdAtUtc": "2026-08-09T15:35:00.4503662",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f20d165c-64f5-4373-9406-38f62219b42f",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "2970841a-eee0-4904-a1b8-bc5b6a9bf67d"
        },
        "createdAtUtc": "2026-08-09T15:34:57.2565009",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "2047772e-18bc-49ba-b4d9-29bc500c19d8",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "01245fe7-9b7f-48ba-ad5b-08fa86278ec9",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:54.8678438",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5f5b4323-733d-4ea1-9ba3-9ff023c517cd",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "7717c90a-5764-4322-b058-04ab901348b7",
        "type": "milestone.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث المرحلة",
        "body": "تم تحديث شروط المرحلة، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:50.0378639",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5d9f29c5-e4f8-4f89-ae66-abc62b9a1c1a",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.5711378",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 7
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Lawyer receives rejected change request**
### client CR for cancellation list for milestone ETag

**Request:** GET http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
      "orderNumber": 1,
      "title": "المرحلة الأولى بعد التحديث",
      "description": "وصف جديد بعد موافقة الطرف الآخر.",
      "amount": 1000.0,
      "durationDays": 20,
      "dueDate": "2026-09-03T15:34:49.9337079",
      "status": 3,
      "fundingStatus": 2,
      "escrowHoldId": "a0798185-9d30-4f95-b89e-febd6daa7ebd",
      "fundedAt": "2026-08-09T15:34:55.7934504",
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": 950.0,
      "version": "\"AAAAAAAADkY=\"",
      "permittedActions": []
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **client CR for cancellation list for milestone ETag** (status=200)
- [PASS] **client CR for cancellation list for milestone ETag contains target milestone**
### client CR for cancellation create change request

**Request:** POST http://localhost:5049/api/milestones/cf3fc657-597d-4ab2-a4af-d610db9a9d57/change-requests

**Body:**
```json
{
  "ProposedDescription": "وصف مؤقت سيقوم العميل بإلغاء طلبه.",
  "ProposedDurationDays": 30,
  "Reason": "سبب واضح ومحدد لطلب تعديل شروط المرحلة."
}
```

**Response Status:** 201

**Response Body:**
```json
{
  "success": true,
  "data": {
    "entityId": "1eb59f8e-561e-475e-85cb-2852c1cf754d",
    "status": "Pending",
    "occurredAt": "2026-08-09T15:35:01.6957616Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **client CR for cancellation create change request** (status=201)
### Counterparty cannot cancel CR

**Request:** POST http://localhost:5049/api/change-requests/1eb59f8e-561e-475e-85cb-2852c1cf754d/cancel

**Body:**
```json
{}
```

**Response Status:** 403

**Response Body:**
```json
{
  "success": false,
  "data": null,
  "message": "مقدم طلب التعديل فقط هو من يمكنه إلغاء الطلب.",
  "errors": null,
  "statusCode": 403
}
```
---


- [PASS] **Counterparty cancellation is forbidden** (status=403)
### Client cancels own change request

**Request:** POST http://localhost:5049/api/change-requests/1eb59f8e-561e-475e-85cb-2852c1cf754d/cancel

**Body:**
```json
{}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "entityId": "1eb59f8e-561e-475e-85cb-2852c1cf754d",
    "status": "Cancelled",
    "occurredAt": "2026-08-09T15:35:02.2669133Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client cancels own change request** (status=200)
### Poll lawyer for cancelled CR

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "98a967a8-a4a2-41b4-a64c-023f4735d487",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "1eb59f8e-561e-475e-85cb-2852c1cf754d"
        },
        "createdAtUtc": "2026-08-09T15:35:01.6959202",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "c66eec0b-7862-4697-9219-5b536aa1db66",
        "type": "milestone.change-request-rejected",
        "severity": "Warning",
        "title": "تم رفض طلب تعديل المرحلة",
        "body": "رفض الطرف الآخر طلب تعديل المرحلة. يمكنك مراجعة الطلب لمعرفة التفاصيل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "21765f91-670d-4888-bce9-9cde08249716"
        },
        "createdAtUtc": "2026-08-09T15:35:00.4503662",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f20d165c-64f5-4373-9406-38f62219b42f",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "2970841a-eee0-4904-a1b8-bc5b6a9bf67d"
        },
        "createdAtUtc": "2026-08-09T15:34:57.2565009",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "2047772e-18bc-49ba-b4d9-29bc500c19d8",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "01245fe7-9b7f-48ba-ad5b-08fa86278ec9",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:54.8678438",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5f5b4323-733d-4ea1-9ba3-9ff023c517cd",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "7717c90a-5764-4322-b058-04ab901348b7",
        "type": "milestone.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث المرحلة",
        "body": "تم تحديث شروط المرحلة، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:50.0378639",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5d9f29c5-e4f8-4f89-ae66-abc62b9a1c1a",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.5711378",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 8
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll lawyer for cancelled CR

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "98a967a8-a4a2-41b4-a64c-023f4735d487",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "1eb59f8e-561e-475e-85cb-2852c1cf754d"
        },
        "createdAtUtc": "2026-08-09T15:35:01.6959202",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "c66eec0b-7862-4697-9219-5b536aa1db66",
        "type": "milestone.change-request-rejected",
        "severity": "Warning",
        "title": "تم رفض طلب تعديل المرحلة",
        "body": "رفض الطرف الآخر طلب تعديل المرحلة. يمكنك مراجعة الطلب لمعرفة التفاصيل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "21765f91-670d-4888-bce9-9cde08249716"
        },
        "createdAtUtc": "2026-08-09T15:35:00.4503662",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f20d165c-64f5-4373-9406-38f62219b42f",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "2970841a-eee0-4904-a1b8-bc5b6a9bf67d"
        },
        "createdAtUtc": "2026-08-09T15:34:57.2565009",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "2047772e-18bc-49ba-b4d9-29bc500c19d8",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "01245fe7-9b7f-48ba-ad5b-08fa86278ec9",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:54.8678438",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5f5b4323-733d-4ea1-9ba3-9ff023c517cd",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "7717c90a-5764-4322-b058-04ab901348b7",
        "type": "milestone.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث المرحلة",
        "body": "تم تحديث شروط المرحلة، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:50.0378639",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5d9f29c5-e4f8-4f89-ae66-abc62b9a1c1a",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.5711378",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 8
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll lawyer for cancelled CR

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "9653addb-c9be-4181-a7b6-d8627863695c",
        "type": "milestone.change-request-cancelled",
        "severity": "Information",
        "title": "تم إلغاء طلب تعديل المرحلة",
        "body": "ألغى الطرف الآخر طلب تعديل المرحلة، ولم يعد القرار مطلوبًا منك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "1eb59f8e-561e-475e-85cb-2852c1cf754d"
        },
        "createdAtUtc": "2026-08-09T15:35:02.2669675",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "98a967a8-a4a2-41b4-a64c-023f4735d487",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "1eb59f8e-561e-475e-85cb-2852c1cf754d"
        },
        "createdAtUtc": "2026-08-09T15:35:01.6959202",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "c66eec0b-7862-4697-9219-5b536aa1db66",
        "type": "milestone.change-request-rejected",
        "severity": "Warning",
        "title": "تم رفض طلب تعديل المرحلة",
        "body": "رفض الطرف الآخر طلب تعديل المرحلة. يمكنك مراجعة الطلب لمعرفة التفاصيل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "21765f91-670d-4888-bce9-9cde08249716"
        },
        "createdAtUtc": "2026-08-09T15:35:00.4503662",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f20d165c-64f5-4373-9406-38f62219b42f",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "2970841a-eee0-4904-a1b8-bc5b6a9bf67d"
        },
        "createdAtUtc": "2026-08-09T15:34:57.2565009",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "2047772e-18bc-49ba-b4d9-29bc500c19d8",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "01245fe7-9b7f-48ba-ad5b-08fa86278ec9",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:54.8678438",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5f5b4323-733d-4ea1-9ba3-9ff023c517cd",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "7717c90a-5764-4322-b058-04ab901348b7",
        "type": "milestone.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث المرحلة",
        "body": "تم تحديث شروط المرحلة، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:50.0378639",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5d9f29c5-e4f8-4f89-ae66-abc62b9a1c1a",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.5711378",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 9
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Lawyer receives cancelled change request**

## Submission, requested changes, resubmission, and manual acceptance

- [PASS] **manual lawyer-owned stored-file fixture created**
### Submit missing files

**Request:** POST http://localhost:5049/api/milestones/cf3fc657-597d-4ab2-a4af-d610db9a9d57/submit

**Body:**
```json
{
  "StoredFileIds": [],
  "Notes": "ملاحظات صالحة"
}
```

**Response Status:** 400

**Response Body:**
```text
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 83 116 111 114 101 100 70 105 108 101 73 100 115 34 58 91 34 217 138 216 172 216 168 32 216 170 216 173 216 175 217 138 216 175 32 217 133 216 185 216 177 217 145 217 129 216 167 216 170 32 217 133 217 132 217 129 216 167 216 170 32 216 181 216 167 217 132 216 173 216 169 32 217 136 217 133 216 181 216 177 216 173 32 216 168 217 135 216 167 46 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 50 48 102 101 97 50 100 51 98 54 49 49 102 56 97 101 54 54 101 98 57 49 100 53 100 97 52 52 101 97 52 54 45 51 102 55 51 49 57 54 102 54 102 55 102 48 102 50 100 45 48 48 34 125
```
---


- [PASS] **Submit missing files returns 400** (status=400)
### Client cannot submit work

**Request:** POST http://localhost:5049/api/milestones/cf3fc657-597d-4ab2-a4af-d610db9a9d57/submit

**Body:**
```json
{
  "StoredFileIds": [
    "4f84052a-5b5e-4168-ba58-feb2d4c93c7e"
  ],
  "Notes": "محاولة غير مصرح بها"
}
```

**Response Status:** 403

**Response Body:**
(Empty)
---


- [PASS] **Client submit is forbidden** (status=403)
### Lawyer submits milestone work

**Request:** POST http://localhost:5049/api/milestones/cf3fc657-597d-4ab2-a4af-d610db9a9d57/submit

**Body:**
```json
{
  "StoredFileIds": [
    "4f84052a-5b5e-4168-ba58-feb2d4c93c7e"
  ],
  "Notes": "اكتملت أعمال المرحلة وأصبحت جاهزة للمراجعة."
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
    "orderNumber": 1,
    "title": "المرحلة الأولى بعد التحديث",
    "description": "وصف جديد بعد موافقة الطرف الآخر.",
    "amount": 1000.0,
    "durationDays": 20,
    "dueDate": "2026-09-03T15:34:49.9337079",
    "status": 4,
    "fundingStatus": 2,
    "escrowHoldId": "a0798185-9d30-4f95-b89e-febd6daa7ebd",
    "fundedAt": "2026-08-09T15:34:55.7934504",
    "submittedAt": "2026-08-09T15:35:04.7803832Z",
    "autoAcceptEligibleAt": "2026-08-16T15:35:04.7803832Z",
    "holdExpiresAt": null,
    "netLawyerAmount": 950.0,
    "version": "\"AAAAAAAADmA=\"",
    "permittedActions": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Lawyer submits milestone work** (status=200)
### Poll client for submission

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "0063a8e5-cc4e-4ce0-8514-746e959b4077",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "21765f91-670d-4888-bce9-9cde08249716"
        },
        "createdAtUtc": "2026-08-09T15:34:59.9616866",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "976f0e7b-f3dd-40c2-aedd-cfece1f2ed2c",
        "type": "milestone.change-request-approved",
        "severity": "Success",
        "title": "تمت الموافقة على طلب التعديل",
        "body": "وافق الطرف الآخر على طلب تعديل المرحلة، وطُبّقت الشروط المعتمدة.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "2970841a-eee0-4904-a1b8-bc5b6a9bf67d"
        },
        "createdAtUtc": "2026-08-09T15:34:58.7538494",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "23e10d77-6701-4a6c-9ee1-952a82e64350",
        "type": "milestone.ready-for-funding",
        "severity": "Information",
        "title": "المرحلة جاهزة للتمويل",
        "body": "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.592442",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "567162f0-c05c-490d-87a9-d16a7e606ebe",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "d51a0203-c2b6-432d-bbbb-38fbc4455b05",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "bf078e91-f5af-4015-8573-ef6170bbf566",
        "type": "milestone.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على المرحلة",
        "body": "وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:51.3581465",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "50bb9c8d-5e13-43cd-9b67-c441da71a4b7",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:48.4656041",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "3d653c37-99ec-4cc2-a62e-578c78417519",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.8428007",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4b117158-e888-4117-a27d-908c9f0e1f08",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.7168952",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 9
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll client for submission

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "0063a8e5-cc4e-4ce0-8514-746e959b4077",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "21765f91-670d-4888-bce9-9cde08249716"
        },
        "createdAtUtc": "2026-08-09T15:34:59.9616866",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "976f0e7b-f3dd-40c2-aedd-cfece1f2ed2c",
        "type": "milestone.change-request-approved",
        "severity": "Success",
        "title": "تمت الموافقة على طلب التعديل",
        "body": "وافق الطرف الآخر على طلب تعديل المرحلة، وطُبّقت الشروط المعتمدة.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "2970841a-eee0-4904-a1b8-bc5b6a9bf67d"
        },
        "createdAtUtc": "2026-08-09T15:34:58.7538494",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "23e10d77-6701-4a6c-9ee1-952a82e64350",
        "type": "milestone.ready-for-funding",
        "severity": "Information",
        "title": "المرحلة جاهزة للتمويل",
        "body": "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.592442",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "567162f0-c05c-490d-87a9-d16a7e606ebe",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "d51a0203-c2b6-432d-bbbb-38fbc4455b05",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "bf078e91-f5af-4015-8573-ef6170bbf566",
        "type": "milestone.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على المرحلة",
        "body": "وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:51.3581465",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "50bb9c8d-5e13-43cd-9b67-c441da71a4b7",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:48.4656041",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "3d653c37-99ec-4cc2-a62e-578c78417519",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.8428007",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4b117158-e888-4117-a27d-908c9f0e1f08",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.7168952",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 9
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll client for submission

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "5f03e480-9709-46f5-bac9-94dfd33fc354",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:35:04.7812945",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "0063a8e5-cc4e-4ce0-8514-746e959b4077",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "21765f91-670d-4888-bce9-9cde08249716"
        },
        "createdAtUtc": "2026-08-09T15:34:59.9616866",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "976f0e7b-f3dd-40c2-aedd-cfece1f2ed2c",
        "type": "milestone.change-request-approved",
        "severity": "Success",
        "title": "تمت الموافقة على طلب التعديل",
        "body": "وافق الطرف الآخر على طلب تعديل المرحلة، وطُبّقت الشروط المعتمدة.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "2970841a-eee0-4904-a1b8-bc5b6a9bf67d"
        },
        "createdAtUtc": "2026-08-09T15:34:58.7538494",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "23e10d77-6701-4a6c-9ee1-952a82e64350",
        "type": "milestone.ready-for-funding",
        "severity": "Information",
        "title": "المرحلة جاهزة للتمويل",
        "body": "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.592442",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "567162f0-c05c-490d-87a9-d16a7e606ebe",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "d51a0203-c2b6-432d-bbbb-38fbc4455b05",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "bf078e91-f5af-4015-8573-ef6170bbf566",
        "type": "milestone.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على المرحلة",
        "body": "وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:51.3581465",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "50bb9c8d-5e13-43cd-9b67-c441da71a4b7",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:48.4656041",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "3d653c37-99ec-4cc2-a62e-578c78417519",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.8428007",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4b117158-e888-4117-a27d-908c9f0e1f08",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.7168952",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 10
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client receives submission**
### Request changes missing reason

**Request:** POST http://localhost:5049/api/milestones/cf3fc657-597d-4ab2-a4af-d610db9a9d57/request-changes

**Body:**
```json
{}
```

**Response Status:** 400

**Response Body:**
```text
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 82 101 97 115 111 110 34 58 91 34 84 104 101 32 82 101 97 115 111 110 32 102 105 101 108 100 32 105 115 32 114 101 113 117 105 114 101 100 46 34 44 34 216 179 216 168 216 168 32 216 183 217 132 216 168 32 216 167 217 132 216 170 216 185 216 175 217 138 217 132 216 167 216 170 32 217 133 216 183 217 132 217 136 216 168 46 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 57 54 52 49 97 102 57 101 48 50 48 54 100 55 101 55 99 99 51 50 101 54 52 98 51 48 56 54 98 51 53 100 45 100 100 100 50 55 99 56 55 54 101 98 54 99 50 102 53 45 48 48 34 125
```
---


- [PASS] **Request changes missing reason returns 400** (status=400)
### Client requests work changes

**Request:** POST http://localhost:5049/api/milestones/cf3fc657-597d-4ab2-a4af-d610db9a9d57/request-changes

**Body:**
```json
{
  "Reason": "يرجى استكمال المستند الختامي."
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
    "orderNumber": 1,
    "title": "المرحلة الأولى بعد التحديث",
    "description": "وصف جديد بعد موافقة الطرف الآخر.",
    "amount": 1000.0,
    "durationDays": 20,
    "dueDate": "2026-09-03T15:34:49.9337079",
    "status": 3,
    "fundingStatus": 2,
    "escrowHoldId": "a0798185-9d30-4f95-b89e-febd6daa7ebd",
    "fundedAt": "2026-08-09T15:34:55.7934504",
    "submittedAt": null,
    "autoAcceptEligibleAt": null,
    "holdExpiresAt": null,
    "netLawyerAmount": 950.0,
    "version": "\"AAAAAAAADmY=\"",
    "permittedActions": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client requests work changes** (status=200)
### Poll lawyer for requested changes

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "9653addb-c9be-4181-a7b6-d8627863695c",
        "type": "milestone.change-request-cancelled",
        "severity": "Information",
        "title": "تم إلغاء طلب تعديل المرحلة",
        "body": "ألغى الطرف الآخر طلب تعديل المرحلة، ولم يعد القرار مطلوبًا منك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "1eb59f8e-561e-475e-85cb-2852c1cf754d"
        },
        "createdAtUtc": "2026-08-09T15:35:02.2669675",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "98a967a8-a4a2-41b4-a64c-023f4735d487",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "1eb59f8e-561e-475e-85cb-2852c1cf754d"
        },
        "createdAtUtc": "2026-08-09T15:35:01.6959202",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "c66eec0b-7862-4697-9219-5b536aa1db66",
        "type": "milestone.change-request-rejected",
        "severity": "Warning",
        "title": "تم رفض طلب تعديل المرحلة",
        "body": "رفض الطرف الآخر طلب تعديل المرحلة. يمكنك مراجعة الطلب لمعرفة التفاصيل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "21765f91-670d-4888-bce9-9cde08249716"
        },
        "createdAtUtc": "2026-08-09T15:35:00.4503662",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f20d165c-64f5-4373-9406-38f62219b42f",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "2970841a-eee0-4904-a1b8-bc5b6a9bf67d"
        },
        "createdAtUtc": "2026-08-09T15:34:57.2565009",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "2047772e-18bc-49ba-b4d9-29bc500c19d8",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "01245fe7-9b7f-48ba-ad5b-08fa86278ec9",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:54.8678438",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5f5b4323-733d-4ea1-9ba3-9ff023c517cd",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "7717c90a-5764-4322-b058-04ab901348b7",
        "type": "milestone.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث المرحلة",
        "body": "تم تحديث شروط المرحلة، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:50.0378639",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5d9f29c5-e4f8-4f89-ae66-abc62b9a1c1a",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.5711378",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 9
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll lawyer for requested changes

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "8160e1c4-1a4b-463d-a034-f804d301420e",
        "type": "milestone.changes-requested",
        "severity": "Warning",
        "title": "طُلبت تعديلات على المرحلة",
        "body": "طلب العميل تعديلات على أعمال المرحلة، ويمكنك مراجعة الطلب وإعادة التسليم.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:35:06.8033973",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "9653addb-c9be-4181-a7b6-d8627863695c",
        "type": "milestone.change-request-cancelled",
        "severity": "Information",
        "title": "تم إلغاء طلب تعديل المرحلة",
        "body": "ألغى الطرف الآخر طلب تعديل المرحلة، ولم يعد القرار مطلوبًا منك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "1eb59f8e-561e-475e-85cb-2852c1cf754d"
        },
        "createdAtUtc": "2026-08-09T15:35:02.2669675",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "98a967a8-a4a2-41b4-a64c-023f4735d487",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "1eb59f8e-561e-475e-85cb-2852c1cf754d"
        },
        "createdAtUtc": "2026-08-09T15:35:01.6959202",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "c66eec0b-7862-4697-9219-5b536aa1db66",
        "type": "milestone.change-request-rejected",
        "severity": "Warning",
        "title": "تم رفض طلب تعديل المرحلة",
        "body": "رفض الطرف الآخر طلب تعديل المرحلة. يمكنك مراجعة الطلب لمعرفة التفاصيل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "21765f91-670d-4888-bce9-9cde08249716"
        },
        "createdAtUtc": "2026-08-09T15:35:00.4503662",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f20d165c-64f5-4373-9406-38f62219b42f",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "2970841a-eee0-4904-a1b8-bc5b6a9bf67d"
        },
        "createdAtUtc": "2026-08-09T15:34:57.2565009",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "2047772e-18bc-49ba-b4d9-29bc500c19d8",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "01245fe7-9b7f-48ba-ad5b-08fa86278ec9",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:54.8678438",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5f5b4323-733d-4ea1-9ba3-9ff023c517cd",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "7717c90a-5764-4322-b058-04ab901348b7",
        "type": "milestone.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث المرحلة",
        "body": "تم تحديث شروط المرحلة، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:50.0378639",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5d9f29c5-e4f8-4f89-ae66-abc62b9a1c1a",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.5711378",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 10
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Lawyer receives requested changes**
### Lawyer resubmits milestone work

**Request:** POST http://localhost:5049/api/milestones/cf3fc657-597d-4ab2-a4af-d610db9a9d57/submit

**Body:**
```json
{
  "StoredFileIds": [
    "4f84052a-5b5e-4168-ba58-feb2d4c93c7e"
  ],
  "Notes": "تم استكمال التعديلات المطلوبة."
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
    "orderNumber": 1,
    "title": "المرحلة الأولى بعد التحديث",
    "description": "وصف جديد بعد موافقة الطرف الآخر.",
    "amount": 1000.0,
    "durationDays": 20,
    "dueDate": "2026-09-03T15:34:49.9337079",
    "status": 4,
    "fundingStatus": 2,
    "escrowHoldId": "a0798185-9d30-4f95-b89e-febd6daa7ebd",
    "fundedAt": "2026-08-09T15:34:55.7934504",
    "submittedAt": "2026-08-09T15:35:07.9834374Z",
    "autoAcceptEligibleAt": "2026-08-16T15:35:07.9834374Z",
    "holdExpiresAt": null,
    "netLawyerAmount": 950.0,
    "version": "\"AAAAAAAADms=\"",
    "permittedActions": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Lawyer resubmits milestone work** (status=200)
### Lawyer cannot accept submission

**Request:** POST http://localhost:5049/api/milestones/cf3fc657-597d-4ab2-a4af-d610db9a9d57/accept

**Body:**
```json
{}
```

**Response Status:** 403

**Response Body:**
(Empty)
---


- [PASS] **Lawyer acceptance is forbidden** (status=403)
### Client accepts milestone work

**Request:** POST http://localhost:5049/api/milestones/cf3fc657-597d-4ab2-a4af-d610db9a9d57/accept

**Body:**
```json
{}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
    "orderNumber": 1,
    "title": "المرحلة الأولى بعد التحديث",
    "description": "وصف جديد بعد موافقة الطرف الآخر.",
    "amount": 1000.0,
    "durationDays": 20,
    "dueDate": "2026-09-03T15:34:49.9337079",
    "status": 5,
    "fundingStatus": 2,
    "escrowHoldId": "a0798185-9d30-4f95-b89e-febd6daa7ebd",
    "fundedAt": "2026-08-09T15:34:55.7934504",
    "submittedAt": "2026-08-09T15:35:07.9834374",
    "autoAcceptEligibleAt": null,
    "holdExpiresAt": "2026-08-23T15:35:08.1526287Z",
    "netLawyerAmount": 950.0,
    "version": "\"AAAAAAAADm4=\"",
    "permittedActions": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client accepts milestone work** (status=200)
### Poll lawyer for manual acceptance

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "8160e1c4-1a4b-463d-a034-f804d301420e",
        "type": "milestone.changes-requested",
        "severity": "Warning",
        "title": "طُلبت تعديلات على المرحلة",
        "body": "طلب العميل تعديلات على أعمال المرحلة، ويمكنك مراجعة الطلب وإعادة التسليم.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:35:06.8033973",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "9653addb-c9be-4181-a7b6-d8627863695c",
        "type": "milestone.change-request-cancelled",
        "severity": "Information",
        "title": "تم إلغاء طلب تعديل المرحلة",
        "body": "ألغى الطرف الآخر طلب تعديل المرحلة، ولم يعد القرار مطلوبًا منك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "1eb59f8e-561e-475e-85cb-2852c1cf754d"
        },
        "createdAtUtc": "2026-08-09T15:35:02.2669675",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "98a967a8-a4a2-41b4-a64c-023f4735d487",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "1eb59f8e-561e-475e-85cb-2852c1cf754d"
        },
        "createdAtUtc": "2026-08-09T15:35:01.6959202",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "c66eec0b-7862-4697-9219-5b536aa1db66",
        "type": "milestone.change-request-rejected",
        "severity": "Warning",
        "title": "تم رفض طلب تعديل المرحلة",
        "body": "رفض الطرف الآخر طلب تعديل المرحلة. يمكنك مراجعة الطلب لمعرفة التفاصيل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "21765f91-670d-4888-bce9-9cde08249716"
        },
        "createdAtUtc": "2026-08-09T15:35:00.4503662",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f20d165c-64f5-4373-9406-38f62219b42f",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "2970841a-eee0-4904-a1b8-bc5b6a9bf67d"
        },
        "createdAtUtc": "2026-08-09T15:34:57.2565009",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "2047772e-18bc-49ba-b4d9-29bc500c19d8",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "01245fe7-9b7f-48ba-ad5b-08fa86278ec9",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:54.8678438",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5f5b4323-733d-4ea1-9ba3-9ff023c517cd",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "7717c90a-5764-4322-b058-04ab901348b7",
        "type": "milestone.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث المرحلة",
        "body": "تم تحديث شروط المرحلة، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:50.0378639",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5d9f29c5-e4f8-4f89-ae66-abc62b9a1c1a",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.5711378",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 10
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll lawyer for manual acceptance

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "f7de5021-20fd-4e3e-ba8d-f5c0fd456a80",
        "type": "milestone.accepted",
        "severity": "Success",
        "title": "تم قبول أعمال المرحلة",
        "body": "قبل العميل أعمال المرحلة، وبدأت مدة حجز المبلغ قبل إتاحته للصرف.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:35:08.1527704",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8160e1c4-1a4b-463d-a034-f804d301420e",
        "type": "milestone.changes-requested",
        "severity": "Warning",
        "title": "طُلبت تعديلات على المرحلة",
        "body": "طلب العميل تعديلات على أعمال المرحلة، ويمكنك مراجعة الطلب وإعادة التسليم.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:35:06.8033973",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "9653addb-c9be-4181-a7b6-d8627863695c",
        "type": "milestone.change-request-cancelled",
        "severity": "Information",
        "title": "تم إلغاء طلب تعديل المرحلة",
        "body": "ألغى الطرف الآخر طلب تعديل المرحلة، ولم يعد القرار مطلوبًا منك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "1eb59f8e-561e-475e-85cb-2852c1cf754d"
        },
        "createdAtUtc": "2026-08-09T15:35:02.2669675",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "98a967a8-a4a2-41b4-a64c-023f4735d487",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "1eb59f8e-561e-475e-85cb-2852c1cf754d"
        },
        "createdAtUtc": "2026-08-09T15:35:01.6959202",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "c66eec0b-7862-4697-9219-5b536aa1db66",
        "type": "milestone.change-request-rejected",
        "severity": "Warning",
        "title": "تم رفض طلب تعديل المرحلة",
        "body": "رفض الطرف الآخر طلب تعديل المرحلة. يمكنك مراجعة الطلب لمعرفة التفاصيل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "21765f91-670d-4888-bce9-9cde08249716"
        },
        "createdAtUtc": "2026-08-09T15:35:00.4503662",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f20d165c-64f5-4373-9406-38f62219b42f",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "2970841a-eee0-4904-a1b8-bc5b6a9bf67d"
        },
        "createdAtUtc": "2026-08-09T15:34:57.2565009",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "2047772e-18bc-49ba-b4d9-29bc500c19d8",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "01245fe7-9b7f-48ba-ad5b-08fa86278ec9",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:54.8678438",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5f5b4323-733d-4ea1-9ba3-9ff023c517cd",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "7717c90a-5764-4322-b058-04ab901348b7",
        "type": "milestone.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث المرحلة",
        "body": "تم تحديث شروط المرحلة، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:50.0378639",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5d9f29c5-e4f8-4f89-ae66-abc62b9a1c1a",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.5711378",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 11
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Lawyer receives manual acceptance**

## Automatic acceptance through accelerated Hangfire schedule

### milestones-auto - create case

**Request:** POST http://localhost:5049/api/Case

**Body:**
```json
{
  "Title": "milestones-auto case 183509067",
  "Description": "Complete case foundation for milestones-auto contract notifications.",
  "City": "Maadi",
  "Governorate": "Cairo"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "caseId": "95e1707e-9223-43a3-9fef-0da124dfacae",
    "failedDocuments": []
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **milestones-auto - create case** (status=200)
### milestones-auto - review case

**Request:** POST http://localhost:5049/api/cases/95e1707e-9223-43a3-9fef-0da124dfacae/review

**Body:**
```json
{}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "bf030cac-3ce2-48ae-b648-82c3536a80f7",
    "caseId": "95e1707e-9223-43a3-9fef-0da124dfacae",
    "isLatest": true,
    "createdAt": "2026-08-09T15:35:09.7215595Z",
    "reviewPoints": [
      {
        "id": "2edf6799-51b2-4c15-bce3-4a9650c74b33",
        "description": "تتمثل نقطة القوة الأساسية في صياغة الموضوع بوضوح حول 'milestones-auto case 183509067'، وتوافر السند المبدئي الذي يرجح كفة الموكل في إثبات أصل الالتزام وتفوقه إثباتياً على الخصم.",
        "type": "Strength"
      },
      {
        "id": "1063b508-0bba-4548-be97-53a8dbe4a64d",
        "description": "ميزة الخصم تتمثل في غياب التوثيق الرسمي للتنبيهات أو الإخطارات المتبادلة بين الأطراف، مما يتيح له إنكار الاستلام أو الدفع بالتراخي في المطالبة.",
        "type": "Weakness"
      },
      {
        "id": "110e1551-4165-41b6-80c9-ef7d06ea0717",
        "description": "يحتاج الملف إلى استيفاء النقاط والمعلومات التالية لضمان صياغة صحيفة الدعوى بشكل مكتمل: حصر وتفصيص المبالغ المالية المطلوبة والتعويضات الدقيقة عن الضرر المادي والمعنوي، وإدراج التواريخ الرسمية الدقيقة لبدء النزاع وتاريخ الإخلال بالتعهدات.",
        "type": "MissingCaseInfo"
      },
      {
        "id": "09448e05-b07c-4858-95c0-2e35d2287644",
        "description": "المستندات المحددة المطلوبة لإكمال الملف: أصل العقد/الاتفاق المبرم، صورة بطاقة الرقم القومي سارية لكل أطراف الدعوى، إيصالات التحويل أو السداد المالي، وأي إنذارات رسمية على يد محضر.",
        "type": "MissingCaseDoc"
      },
      {
        "id": "7f8de780-976f-4a59-a6ff-a8de546d6564",
        "description": "قم بإعادة هيكلة وصف القضية في صورة جدول زمني متسلسل، يبدأ من تاريخ التعهد الأول، مروراً بتاريخ الإخلال، وصولاً إلى حجم الأضرار المترتبة حالياً.",
        "type": "Suggestion"
      },
      {
        "id": "ab4a9bd9-1edf-446e-8684-23f6f04f0436",
        "description": "قم بتفقيط وقسمة كافة المطالبات المالية إلى بنود مستقلة (أصل الدين، الفوائد أو التعويض عن المماطلة، والرسوم) وتوثيق كل بند بسند كتابي مستقل.",
        "type": "Suggestion"
      },
      {
        "id": "8c68cfb7-2326-4137-9654-7a093310df1c",
        "description": "قم بتنظيم وثائق الملف في مجلد مرتب حسب التاريخ، وتأكد من مسح الأوراق ضوئياً بدقة عالية لضمان سهولة الإسناد والفحص القضائي.",
        "type": "Suggestion"
      }
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **milestones-auto - review case** (status=200)
### milestones-auto - finalize case

**Request:** POST http://localhost:5049/api/Case/95e1707e-9223-43a3-9fef-0da124dfacae/finalize

**Body:**
```json
{}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "caseId": "95e1707e-9223-43a3-9fef-0da124dfacae",
    "totalEligibleLawyers": 0,
    "recommendations": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **milestones-auto - finalize case** (status=200)
### milestones-auto - create proposal

**Request:** POST http://localhost:5049/api/proposals

**Body:**
```json
{
  "LegalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae",
  "Message": "milestones-auto proposal for contract notification lifecycle.",
  "LawyerUserId": "7955b16b-9125-456d-f4e0-08def628c2bc"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "02825a5b-00e4-46c6-94b9-070d4d23775e",
    "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae",
    "caseTitle": "milestones-auto case 183509067",
    "clientUserId": "49a02ff4-6241-4855-f4df-08def628c2bc",
    "clientName": "Contracts Notifications client",
    "lawyerUserId": "7955b16b-9125-456d-f4e0-08def628c2bc",
    "lawyerName": "Contracts Notifications lawyer",
    "message": "milestones-auto proposal for contract notification lifecycle.",
    "status": "Pending",
    "decisionReason": null,
    "createdAt": "2026-08-09T15:35:10.5412682",
    "respondedAt": null,
    "updatedAt": "2026-08-09T15:35:10.5412682",
    "conversationId": null
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **milestones-auto - create proposal** (status=200)
### milestones-auto - accept proposal

**Request:** POST http://localhost:5049/api/proposals/02825a5b-00e4-46c6-94b9-070d4d23775e/accept

**Body:**
```json
{}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "02825a5b-00e4-46c6-94b9-070d4d23775e",
    "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae",
    "caseTitle": "milestones-auto case 183509067",
    "clientUserId": "49a02ff4-6241-4855-f4df-08def628c2bc",
    "clientName": "Contracts Notifications client",
    "lawyerUserId": "7955b16b-9125-456d-f4e0-08def628c2bc",
    "lawyerName": "Contracts Notifications lawyer",
    "message": "milestones-auto proposal for contract notification lifecycle.",
    "status": "Accepted",
    "decisionReason": null,
    "createdAt": "2026-08-09T15:35:10.5412682",
    "respondedAt": "2026-08-09T15:35:10.6729834",
    "updatedAt": "2026-08-09T15:35:10.6729834",
    "conversationId": "ba5b900d-df1a-4c95-90bb-15b134cd5d29"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **milestones-auto - accept proposal** (status=200)
### milestones-auto - create contract

**Request:** POST http://localhost:5049/api/contracts

**Body:**
```json
{
  "Title": "milestones-auto legal representation contract",
  "ProposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
  "TermsAndConditions": "These complete contract terms are used for the milestones-auto notification lifecycle and are accepted by both participants."
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "2d798205-6229-4377-bfe8-dfcebaa8e887",
    "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
    "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae",
    "clientUserId": "49a02ff4-6241-4855-f4df-08def628c2bc",
    "lawyerUserId": "7955b16b-9125-456d-f4e0-08def628c2bc",
    "title": "milestones-auto legal representation contract",
    "termsAndConditions": "These complete contract terms are used for the milestones-auto notification lifecycle and are accepted by both participants.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": null,
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 0,
    "version": "\"AAAAAAAADn4=\"",
    "milestones": [],
    "payments": [],
    "permittedActions": [
      "Update",
      "Accept",
      "Terminate"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **milestones-auto - create contract** (status=200)
- [PASS] **milestones-auto create envelope retains logical 201**
### milestones-auto - add milestone

**Request:** POST http://localhost:5049/api/contracts/2d798205-6229-4377-bfe8-dfcebaa8e887/milestones

**Body:**
```json
{
  "OrderNumber": 1,
  "Title": "milestones-auto execution milestone",
  "Description": "Approved milestone used for the contract lifecycle.",
  "DurationDays": 10,
  "Amount": 1000.0
}
```

**Response Status:** 201

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
    "orderNumber": 1,
    "title": "milestones-auto execution milestone",
    "description": "Approved milestone used for the contract lifecycle.",
    "amount": 1000.0,
    "durationDays": 10,
    "dueDate": null,
    "status": 0,
    "fundingStatus": 0,
    "escrowHoldId": null,
    "fundedAt": null,
    "submittedAt": null,
    "autoAcceptEligibleAt": null,
    "holdExpiresAt": null,
    "netLawyerAmount": null,
    "version": "\"AAAAAAAADoA=\"",
    "permittedActions": [
      "Update",
      "Approve"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **milestones-auto - add milestone** (status=201)
### milestones-auto - list milestone for client ETag

**Request:** GET http://localhost:5049/api/contracts/2d798205-6229-4377-bfe8-dfcebaa8e887/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
      "orderNumber": 1,
      "title": "milestones-auto execution milestone",
      "description": "Approved milestone used for the contract lifecycle.",
      "amount": 1000.0,
      "durationDays": 10,
      "dueDate": null,
      "status": 0,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAADoA=\"",
      "permittedActions": [
        "Update",
        "Approve"
      ]
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **milestones-auto - list milestone for client ETag** (status=200)
### milestones-auto - client approves milestone

**Request:** POST http://localhost:5049/api/milestones/0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4/approve

**Body:**
```json
{}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "entityId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
    "status": "Draft",
    "occurredAt": "2026-08-09T15:35:11.4279782Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **milestones-auto - client approves milestone** (status=200)
### milestones-auto - list milestone for lawyer ETag

**Request:** GET http://localhost:5049/api/contracts/2d798205-6229-4377-bfe8-dfcebaa8e887/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
      "orderNumber": 1,
      "title": "milestones-auto execution milestone",
      "description": "Approved milestone used for the contract lifecycle.",
      "amount": 1000.0,
      "durationDays": 10,
      "dueDate": null,
      "status": 0,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAADoI=\"",
      "permittedActions": [
        "Update",
        "Approve"
      ]
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **milestones-auto - list milestone for lawyer ETag** (status=200)
### milestones-auto - lawyer approves milestone

**Request:** POST http://localhost:5049/api/milestones/0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4/approve

**Body:**
```json
{}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "entityId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
    "status": "AwaitingFunding",
    "occurredAt": "2026-08-09T15:35:11.7854455Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **milestones-auto - lawyer approves milestone** (status=200)
### milestones-auto - contract ETag for client acceptance

**Request:** GET http://localhost:5049/api/contracts/2d798205-6229-4377-bfe8-dfcebaa8e887

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "2d798205-6229-4377-bfe8-dfcebaa8e887",
    "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
    "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae",
    "clientUserId": "49a02ff4-6241-4855-f4df-08def628c2bc",
    "lawyerUserId": "7955b16b-9125-456d-f4e0-08def628c2bc",
    "title": "milestones-auto legal representation contract",
    "termsAndConditions": "These complete contract terms are used for the milestones-auto notification lifecycle and are accepted by both participants.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": null,
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 1000.0,
    "version": "\"AAAAAAAADn4=\"",
    "milestones": [
      {
        "id": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
        "orderNumber": 1,
        "title": "milestones-auto execution milestone",
        "description": "Approved milestone used for the contract lifecycle.",
        "amount": 1000.0,
        "durationDays": 10,
        "dueDate": null,
        "status": 1,
        "fundingStatus": 0,
        "escrowHoldId": null,
        "fundedAt": null,
        "submittedAt": null,
        "autoAcceptEligibleAt": null,
        "holdExpiresAt": null,
        "netLawyerAmount": null,
        "version": "\"AAAAAAAADoY=\""
      }
    ],
    "payments": [],
    "permittedActions": [
      "Update",
      "Accept",
      "Terminate"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **milestones-auto - contract ETag for client acceptance** (status=200)
### milestones-auto - client accepts contract

**Request:** POST http://localhost:5049/api/contracts/2d798205-6229-4377-bfe8-dfcebaa8e887/accept

**Body:**
```json
{}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "entityId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
    "status": "Draft",
    "occurredAt": "2026-08-09T15:35:11.9839606Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **milestones-auto - client accepts contract** (status=200)
### milestones-auto - contract ETag for lawyer acceptance

**Request:** GET http://localhost:5049/api/contracts/2d798205-6229-4377-bfe8-dfcebaa8e887

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "2d798205-6229-4377-bfe8-dfcebaa8e887",
    "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
    "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae",
    "clientUserId": "49a02ff4-6241-4855-f4df-08def628c2bc",
    "lawyerUserId": "7955b16b-9125-456d-f4e0-08def628c2bc",
    "title": "milestones-auto legal representation contract",
    "termsAndConditions": "These complete contract terms are used for the milestones-auto notification lifecycle and are accepted by both participants.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": "2026-08-09T15:35:11.9839606",
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 1000.0,
    "version": "\"AAAAAAAADo8=\"",
    "milestones": [
      {
        "id": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
        "orderNumber": 1,
        "title": "milestones-auto execution milestone",
        "description": "Approved milestone used for the contract lifecycle.",
        "amount": 1000.0,
        "durationDays": 10,
        "dueDate": null,
        "status": 1,
        "fundingStatus": 0,
        "escrowHoldId": null,
        "fundedAt": null,
        "submittedAt": null,
        "autoAcceptEligibleAt": null,
        "holdExpiresAt": null,
        "netLawyerAmount": null,
        "version": "\"AAAAAAAADoY=\""
      }
    ],
    "payments": [],
    "permittedActions": [
      "Update",
      "Accept",
      "Terminate"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **milestones-auto - contract ETag for lawyer acceptance** (status=200)
### milestones-auto - lawyer accepts contract

**Request:** POST http://localhost:5049/api/contracts/2d798205-6229-4377-bfe8-dfcebaa8e887/accept

**Body:**
```json
{}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "entityId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
    "status": "Active",
    "occurredAt": "2026-08-09T15:35:12.1949839Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **milestones-auto - lawyer accepts contract** (status=200)
### milestones-auto - list milestone before funding

**Request:** GET http://localhost:5049/api/contracts/2d798205-6229-4377-bfe8-dfcebaa8e887/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
      "orderNumber": 1,
      "title": "milestones-auto execution milestone",
      "description": "Approved milestone used for the contract lifecycle.",
      "amount": 1000.0,
      "durationDays": 10,
      "dueDate": null,
      "status": 1,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAADoY=\"",
      "permittedActions": [
        "ReadyForFunding"
      ]
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **milestones-auto - list milestone before funding** (status=200)
### milestones-auto - mark milestone ready for funding

**Request:** POST http://localhost:5049/api/milestones/0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4/ready-for-funding

**Body:**
```json
{}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "entityId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
    "status": "AwaitingFunding",
    "occurredAt": "2026-08-09T15:35:14.1721751Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **milestones-auto - mark milestone ready for funding** (status=200)
### milestones-auto - fund milestone through mock provider

**Request:** POST http://localhost:5049/api/milestones/0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4/fund

**Body:**
```json
{
  "PaymentMethodReference": "[REDACTED]"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "5a3e84e3-bb1a-4a61-840a-bd6833256d0b",
    "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
    "grossAmount": 1000.0,
    "platformFee": 50.0,
    "netAmount": 950.0,
    "currency": "EGP",
    "status": 0,
    "holdExpiresAt": null,
    "settledAt": null
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **milestones-auto - fund milestone through mock provider** (status=200)
- [PASS] **auto lawyer-owned stored-file fixture created**
### Submit auto-accept milestone

**Request:** POST http://localhost:5049/api/milestones/0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4/submit

**Body:**
```json
{
  "StoredFileIds": [
    "2aef14f6-514d-4118-8b20-597c67fdb68c"
  ],
  "Notes": "تسليم لاختبار القبول التلقائي."
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
    "orderNumber": 1,
    "title": "milestones-auto execution milestone",
    "description": "Approved milestone used for the contract lifecycle.",
    "amount": 1000.0,
    "durationDays": 10,
    "dueDate": null,
    "status": 4,
    "fundingStatus": 2,
    "escrowHoldId": "5a3e84e3-bb1a-4a61-840a-bd6833256d0b",
    "fundedAt": "2026-08-09T15:35:14.279839",
    "submittedAt": "2026-08-09T15:35:14.5360842Z",
    "autoAcceptEligibleAt": "2026-08-16T15:35:14.5360842Z",
    "holdExpiresAt": null,
    "netLawyerAmount": 950.0,
    "version": "\"AAAAAAAADro=\"",
    "permittedActions": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Submit auto-accept milestone** (status=200)
- [PASS] **Auto-accept Hangfire job was scheduled**
- [PASS] **Auto-accept schedule accelerated by scoped test fixture**
### Poll client for automatic acceptance

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "1f932114-f3cf-4268-a11f-78a386f552fc",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:14.5362507",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8941574c-a99e-442f-9ca1-aba85acc8733",
        "type": "milestone.ready-for-funding",
        "severity": "Information",
        "title": "المرحلة جاهزة للتمويل",
        "body": "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:14.1722181",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "48c3f39d-cbd9-493d-bee9-b03363569a3e",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:12.2174038",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "58ef3a0c-aeba-4c7e-8026-279c1188976f",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:11.7855637",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "10ebf3a0-80dc-4754-9961-174ad10eff3b",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:11.1089584",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8ee7829d-0b18-4d73-b07f-6035045ece9d",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:10.7793364",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "fe3ab84e-cb32-4076-a979-f8f7b34225ea",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/02825a5b-00e4-46c6-94b9-070d4d23775e",
        "data": {
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:10.6772061",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "19fbb0ed-5001-4930-8ae8-9da90c878c46",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:35:07.983613",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5f03e480-9709-46f5-bac9-94dfd33fc354",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:35:04.7812945",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "0063a8e5-cc4e-4ce0-8514-746e959b4077",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "21765f91-670d-4888-bce9-9cde08249716"
        },
        "createdAtUtc": "2026-08-09T15:34:59.9616866",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "976f0e7b-f3dd-40c2-aedd-cfece1f2ed2c",
        "type": "milestone.change-request-approved",
        "severity": "Success",
        "title": "تمت الموافقة على طلب التعديل",
        "body": "وافق الطرف الآخر على طلب تعديل المرحلة، وطُبّقت الشروط المعتمدة.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "2970841a-eee0-4904-a1b8-bc5b6a9bf67d"
        },
        "createdAtUtc": "2026-08-09T15:34:58.7538494",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "23e10d77-6701-4a6c-9ee1-952a82e64350",
        "type": "milestone.ready-for-funding",
        "severity": "Information",
        "title": "المرحلة جاهزة للتمويل",
        "body": "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.592442",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "567162f0-c05c-490d-87a9-d16a7e606ebe",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "d51a0203-c2b6-432d-bbbb-38fbc4455b05",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "bf078e91-f5af-4015-8573-ef6170bbf566",
        "type": "milestone.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على المرحلة",
        "body": "وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:51.3581465",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "50bb9c8d-5e13-43cd-9b67-c441da71a4b7",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:48.4656041",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "3d653c37-99ec-4cc2-a62e-578c78417519",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.8428007",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4b117158-e888-4117-a27d-908c9f0e1f08",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.7168952",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 18
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll client for automatic acceptance

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "1f932114-f3cf-4268-a11f-78a386f552fc",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:14.5362507",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8941574c-a99e-442f-9ca1-aba85acc8733",
        "type": "milestone.ready-for-funding",
        "severity": "Information",
        "title": "المرحلة جاهزة للتمويل",
        "body": "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:14.1722181",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "48c3f39d-cbd9-493d-bee9-b03363569a3e",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:12.2174038",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "58ef3a0c-aeba-4c7e-8026-279c1188976f",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:11.7855637",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "10ebf3a0-80dc-4754-9961-174ad10eff3b",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:11.1089584",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8ee7829d-0b18-4d73-b07f-6035045ece9d",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:10.7793364",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "fe3ab84e-cb32-4076-a979-f8f7b34225ea",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/02825a5b-00e4-46c6-94b9-070d4d23775e",
        "data": {
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:10.6772061",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "19fbb0ed-5001-4930-8ae8-9da90c878c46",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:35:07.983613",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5f03e480-9709-46f5-bac9-94dfd33fc354",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:35:04.7812945",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "0063a8e5-cc4e-4ce0-8514-746e959b4077",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "21765f91-670d-4888-bce9-9cde08249716"
        },
        "createdAtUtc": "2026-08-09T15:34:59.9616866",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "976f0e7b-f3dd-40c2-aedd-cfece1f2ed2c",
        "type": "milestone.change-request-approved",
        "severity": "Success",
        "title": "تمت الموافقة على طلب التعديل",
        "body": "وافق الطرف الآخر على طلب تعديل المرحلة، وطُبّقت الشروط المعتمدة.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "2970841a-eee0-4904-a1b8-bc5b6a9bf67d"
        },
        "createdAtUtc": "2026-08-09T15:34:58.7538494",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "23e10d77-6701-4a6c-9ee1-952a82e64350",
        "type": "milestone.ready-for-funding",
        "severity": "Information",
        "title": "المرحلة جاهزة للتمويل",
        "body": "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.592442",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "567162f0-c05c-490d-87a9-d16a7e606ebe",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "d51a0203-c2b6-432d-bbbb-38fbc4455b05",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "bf078e91-f5af-4015-8573-ef6170bbf566",
        "type": "milestone.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على المرحلة",
        "body": "وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:51.3581465",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "50bb9c8d-5e13-43cd-9b67-c441da71a4b7",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:48.4656041",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "3d653c37-99ec-4cc2-a62e-578c78417519",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.8428007",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4b117158-e888-4117-a27d-908c9f0e1f08",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.7168952",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 18
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll client for automatic acceptance

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "1f932114-f3cf-4268-a11f-78a386f552fc",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:14.5362507",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8941574c-a99e-442f-9ca1-aba85acc8733",
        "type": "milestone.ready-for-funding",
        "severity": "Information",
        "title": "المرحلة جاهزة للتمويل",
        "body": "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:14.1722181",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "48c3f39d-cbd9-493d-bee9-b03363569a3e",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:12.2174038",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "58ef3a0c-aeba-4c7e-8026-279c1188976f",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:11.7855637",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "10ebf3a0-80dc-4754-9961-174ad10eff3b",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:11.1089584",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8ee7829d-0b18-4d73-b07f-6035045ece9d",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:10.7793364",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "fe3ab84e-cb32-4076-a979-f8f7b34225ea",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/02825a5b-00e4-46c6-94b9-070d4d23775e",
        "data": {
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:10.6772061",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "19fbb0ed-5001-4930-8ae8-9da90c878c46",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:35:07.983613",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5f03e480-9709-46f5-bac9-94dfd33fc354",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:35:04.7812945",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "0063a8e5-cc4e-4ce0-8514-746e959b4077",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "21765f91-670d-4888-bce9-9cde08249716"
        },
        "createdAtUtc": "2026-08-09T15:34:59.9616866",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "976f0e7b-f3dd-40c2-aedd-cfece1f2ed2c",
        "type": "milestone.change-request-approved",
        "severity": "Success",
        "title": "تمت الموافقة على طلب التعديل",
        "body": "وافق الطرف الآخر على طلب تعديل المرحلة، وطُبّقت الشروط المعتمدة.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "2970841a-eee0-4904-a1b8-bc5b6a9bf67d"
        },
        "createdAtUtc": "2026-08-09T15:34:58.7538494",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "23e10d77-6701-4a6c-9ee1-952a82e64350",
        "type": "milestone.ready-for-funding",
        "severity": "Information",
        "title": "المرحلة جاهزة للتمويل",
        "body": "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.592442",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "567162f0-c05c-490d-87a9-d16a7e606ebe",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "d51a0203-c2b6-432d-bbbb-38fbc4455b05",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "bf078e91-f5af-4015-8573-ef6170bbf566",
        "type": "milestone.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على المرحلة",
        "body": "وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:51.3581465",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "50bb9c8d-5e13-43cd-9b67-c441da71a4b7",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:48.4656041",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "3d653c37-99ec-4cc2-a62e-578c78417519",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.8428007",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4b117158-e888-4117-a27d-908c9f0e1f08",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.7168952",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 18
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll client for automatic acceptance

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "1f932114-f3cf-4268-a11f-78a386f552fc",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:14.5362507",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8941574c-a99e-442f-9ca1-aba85acc8733",
        "type": "milestone.ready-for-funding",
        "severity": "Information",
        "title": "المرحلة جاهزة للتمويل",
        "body": "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:14.1722181",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "48c3f39d-cbd9-493d-bee9-b03363569a3e",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:12.2174038",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "58ef3a0c-aeba-4c7e-8026-279c1188976f",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:11.7855637",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "10ebf3a0-80dc-4754-9961-174ad10eff3b",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:11.1089584",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8ee7829d-0b18-4d73-b07f-6035045ece9d",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:10.7793364",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "fe3ab84e-cb32-4076-a979-f8f7b34225ea",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/02825a5b-00e4-46c6-94b9-070d4d23775e",
        "data": {
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:10.6772061",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "19fbb0ed-5001-4930-8ae8-9da90c878c46",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:35:07.983613",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5f03e480-9709-46f5-bac9-94dfd33fc354",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:35:04.7812945",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "0063a8e5-cc4e-4ce0-8514-746e959b4077",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "21765f91-670d-4888-bce9-9cde08249716"
        },
        "createdAtUtc": "2026-08-09T15:34:59.9616866",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "976f0e7b-f3dd-40c2-aedd-cfece1f2ed2c",
        "type": "milestone.change-request-approved",
        "severity": "Success",
        "title": "تمت الموافقة على طلب التعديل",
        "body": "وافق الطرف الآخر على طلب تعديل المرحلة، وطُبّقت الشروط المعتمدة.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "2970841a-eee0-4904-a1b8-bc5b6a9bf67d"
        },
        "createdAtUtc": "2026-08-09T15:34:58.7538494",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "23e10d77-6701-4a6c-9ee1-952a82e64350",
        "type": "milestone.ready-for-funding",
        "severity": "Information",
        "title": "المرحلة جاهزة للتمويل",
        "body": "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.592442",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "567162f0-c05c-490d-87a9-d16a7e606ebe",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "d51a0203-c2b6-432d-bbbb-38fbc4455b05",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "bf078e91-f5af-4015-8573-ef6170bbf566",
        "type": "milestone.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على المرحلة",
        "body": "وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:51.3581465",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "50bb9c8d-5e13-43cd-9b67-c441da71a4b7",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:48.4656041",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "3d653c37-99ec-4cc2-a62e-578c78417519",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.8428007",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4b117158-e888-4117-a27d-908c9f0e1f08",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.7168952",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 18
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll client for automatic acceptance

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "1f932114-f3cf-4268-a11f-78a386f552fc",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:14.5362507",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8941574c-a99e-442f-9ca1-aba85acc8733",
        "type": "milestone.ready-for-funding",
        "severity": "Information",
        "title": "المرحلة جاهزة للتمويل",
        "body": "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:14.1722181",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "48c3f39d-cbd9-493d-bee9-b03363569a3e",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:12.2174038",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "58ef3a0c-aeba-4c7e-8026-279c1188976f",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:11.7855637",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "10ebf3a0-80dc-4754-9961-174ad10eff3b",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:11.1089584",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8ee7829d-0b18-4d73-b07f-6035045ece9d",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:10.7793364",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "fe3ab84e-cb32-4076-a979-f8f7b34225ea",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/02825a5b-00e4-46c6-94b9-070d4d23775e",
        "data": {
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:10.6772061",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "19fbb0ed-5001-4930-8ae8-9da90c878c46",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:35:07.983613",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5f03e480-9709-46f5-bac9-94dfd33fc354",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:35:04.7812945",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "0063a8e5-cc4e-4ce0-8514-746e959b4077",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "21765f91-670d-4888-bce9-9cde08249716"
        },
        "createdAtUtc": "2026-08-09T15:34:59.9616866",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "976f0e7b-f3dd-40c2-aedd-cfece1f2ed2c",
        "type": "milestone.change-request-approved",
        "severity": "Success",
        "title": "تمت الموافقة على طلب التعديل",
        "body": "وافق الطرف الآخر على طلب تعديل المرحلة، وطُبّقت الشروط المعتمدة.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "2970841a-eee0-4904-a1b8-bc5b6a9bf67d"
        },
        "createdAtUtc": "2026-08-09T15:34:58.7538494",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "23e10d77-6701-4a6c-9ee1-952a82e64350",
        "type": "milestone.ready-for-funding",
        "severity": "Information",
        "title": "المرحلة جاهزة للتمويل",
        "body": "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.592442",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "567162f0-c05c-490d-87a9-d16a7e606ebe",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "d51a0203-c2b6-432d-bbbb-38fbc4455b05",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "bf078e91-f5af-4015-8573-ef6170bbf566",
        "type": "milestone.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على المرحلة",
        "body": "وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:51.3581465",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "50bb9c8d-5e13-43cd-9b67-c441da71a4b7",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:48.4656041",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "3d653c37-99ec-4cc2-a62e-578c78417519",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.8428007",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4b117158-e888-4117-a27d-908c9f0e1f08",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.7168952",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 18
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll client for automatic acceptance

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "1f932114-f3cf-4268-a11f-78a386f552fc",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:14.5362507",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8941574c-a99e-442f-9ca1-aba85acc8733",
        "type": "milestone.ready-for-funding",
        "severity": "Information",
        "title": "المرحلة جاهزة للتمويل",
        "body": "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:14.1722181",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "48c3f39d-cbd9-493d-bee9-b03363569a3e",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:12.2174038",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "58ef3a0c-aeba-4c7e-8026-279c1188976f",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:11.7855637",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "10ebf3a0-80dc-4754-9961-174ad10eff3b",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:11.1089584",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8ee7829d-0b18-4d73-b07f-6035045ece9d",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:10.7793364",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "fe3ab84e-cb32-4076-a979-f8f7b34225ea",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/02825a5b-00e4-46c6-94b9-070d4d23775e",
        "data": {
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:10.6772061",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "19fbb0ed-5001-4930-8ae8-9da90c878c46",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:35:07.983613",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5f03e480-9709-46f5-bac9-94dfd33fc354",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:35:04.7812945",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "0063a8e5-cc4e-4ce0-8514-746e959b4077",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "21765f91-670d-4888-bce9-9cde08249716"
        },
        "createdAtUtc": "2026-08-09T15:34:59.9616866",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "976f0e7b-f3dd-40c2-aedd-cfece1f2ed2c",
        "type": "milestone.change-request-approved",
        "severity": "Success",
        "title": "تمت الموافقة على طلب التعديل",
        "body": "وافق الطرف الآخر على طلب تعديل المرحلة، وطُبّقت الشروط المعتمدة.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "2970841a-eee0-4904-a1b8-bc5b6a9bf67d"
        },
        "createdAtUtc": "2026-08-09T15:34:58.7538494",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "23e10d77-6701-4a6c-9ee1-952a82e64350",
        "type": "milestone.ready-for-funding",
        "severity": "Information",
        "title": "المرحلة جاهزة للتمويل",
        "body": "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.592442",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "567162f0-c05c-490d-87a9-d16a7e606ebe",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "d51a0203-c2b6-432d-bbbb-38fbc4455b05",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "bf078e91-f5af-4015-8573-ef6170bbf566",
        "type": "milestone.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على المرحلة",
        "body": "وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:51.3581465",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "50bb9c8d-5e13-43cd-9b67-c441da71a4b7",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:48.4656041",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "3d653c37-99ec-4cc2-a62e-578c78417519",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.8428007",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4b117158-e888-4117-a27d-908c9f0e1f08",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.7168952",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 18
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll client for automatic acceptance

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "1f932114-f3cf-4268-a11f-78a386f552fc",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:14.5362507",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8941574c-a99e-442f-9ca1-aba85acc8733",
        "type": "milestone.ready-for-funding",
        "severity": "Information",
        "title": "المرحلة جاهزة للتمويل",
        "body": "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:14.1722181",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "48c3f39d-cbd9-493d-bee9-b03363569a3e",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:12.2174038",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "58ef3a0c-aeba-4c7e-8026-279c1188976f",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:11.7855637",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "10ebf3a0-80dc-4754-9961-174ad10eff3b",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:11.1089584",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8ee7829d-0b18-4d73-b07f-6035045ece9d",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:10.7793364",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "fe3ab84e-cb32-4076-a979-f8f7b34225ea",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/02825a5b-00e4-46c6-94b9-070d4d23775e",
        "data": {
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:10.6772061",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "19fbb0ed-5001-4930-8ae8-9da90c878c46",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:35:07.983613",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5f03e480-9709-46f5-bac9-94dfd33fc354",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:35:04.7812945",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "0063a8e5-cc4e-4ce0-8514-746e959b4077",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "21765f91-670d-4888-bce9-9cde08249716"
        },
        "createdAtUtc": "2026-08-09T15:34:59.9616866",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "976f0e7b-f3dd-40c2-aedd-cfece1f2ed2c",
        "type": "milestone.change-request-approved",
        "severity": "Success",
        "title": "تمت الموافقة على طلب التعديل",
        "body": "وافق الطرف الآخر على طلب تعديل المرحلة، وطُبّقت الشروط المعتمدة.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "2970841a-eee0-4904-a1b8-bc5b6a9bf67d"
        },
        "createdAtUtc": "2026-08-09T15:34:58.7538494",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "23e10d77-6701-4a6c-9ee1-952a82e64350",
        "type": "milestone.ready-for-funding",
        "severity": "Information",
        "title": "المرحلة جاهزة للتمويل",
        "body": "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.592442",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "567162f0-c05c-490d-87a9-d16a7e606ebe",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "d51a0203-c2b6-432d-bbbb-38fbc4455b05",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "bf078e91-f5af-4015-8573-ef6170bbf566",
        "type": "milestone.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على المرحلة",
        "body": "وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:51.3581465",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "50bb9c8d-5e13-43cd-9b67-c441da71a4b7",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:48.4656041",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "3d653c37-99ec-4cc2-a62e-578c78417519",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.8428007",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4b117158-e888-4117-a27d-908c9f0e1f08",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.7168952",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 18
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll client for automatic acceptance

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "1f932114-f3cf-4268-a11f-78a386f552fc",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:14.5362507",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8941574c-a99e-442f-9ca1-aba85acc8733",
        "type": "milestone.ready-for-funding",
        "severity": "Information",
        "title": "المرحلة جاهزة للتمويل",
        "body": "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:14.1722181",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "48c3f39d-cbd9-493d-bee9-b03363569a3e",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:12.2174038",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "58ef3a0c-aeba-4c7e-8026-279c1188976f",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:11.7855637",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "10ebf3a0-80dc-4754-9961-174ad10eff3b",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:11.1089584",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8ee7829d-0b18-4d73-b07f-6035045ece9d",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:10.7793364",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "fe3ab84e-cb32-4076-a979-f8f7b34225ea",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/02825a5b-00e4-46c6-94b9-070d4d23775e",
        "data": {
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:10.6772061",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "19fbb0ed-5001-4930-8ae8-9da90c878c46",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:35:07.983613",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5f03e480-9709-46f5-bac9-94dfd33fc354",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:35:04.7812945",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "0063a8e5-cc4e-4ce0-8514-746e959b4077",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "21765f91-670d-4888-bce9-9cde08249716"
        },
        "createdAtUtc": "2026-08-09T15:34:59.9616866",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "976f0e7b-f3dd-40c2-aedd-cfece1f2ed2c",
        "type": "milestone.change-request-approved",
        "severity": "Success",
        "title": "تمت الموافقة على طلب التعديل",
        "body": "وافق الطرف الآخر على طلب تعديل المرحلة، وطُبّقت الشروط المعتمدة.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "2970841a-eee0-4904-a1b8-bc5b6a9bf67d"
        },
        "createdAtUtc": "2026-08-09T15:34:58.7538494",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "23e10d77-6701-4a6c-9ee1-952a82e64350",
        "type": "milestone.ready-for-funding",
        "severity": "Information",
        "title": "المرحلة جاهزة للتمويل",
        "body": "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.592442",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "567162f0-c05c-490d-87a9-d16a7e606ebe",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "d51a0203-c2b6-432d-bbbb-38fbc4455b05",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "bf078e91-f5af-4015-8573-ef6170bbf566",
        "type": "milestone.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على المرحلة",
        "body": "وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:51.3581465",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "50bb9c8d-5e13-43cd-9b67-c441da71a4b7",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:48.4656041",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "3d653c37-99ec-4cc2-a62e-578c78417519",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.8428007",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4b117158-e888-4117-a27d-908c9f0e1f08",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.7168952",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 18
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll client for automatic acceptance

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "f8e36262-1ed2-4ea1-9981-bd6a07c2fae2",
        "type": "milestone.auto-accepted",
        "severity": "Warning",
        "title": "تم قبول المرحلة تلقائيًا",
        "body": "انتهت مدة المراجعة وقُبلت أعمال المرحلة تلقائيًا، وبدأت مدة الاعتراض.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:24.9186209",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "1f932114-f3cf-4268-a11f-78a386f552fc",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:14.5362507",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8941574c-a99e-442f-9ca1-aba85acc8733",
        "type": "milestone.ready-for-funding",
        "severity": "Information",
        "title": "المرحلة جاهزة للتمويل",
        "body": "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:14.1722181",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "48c3f39d-cbd9-493d-bee9-b03363569a3e",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:12.2174038",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "58ef3a0c-aeba-4c7e-8026-279c1188976f",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:11.7855637",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "10ebf3a0-80dc-4754-9961-174ad10eff3b",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:11.1089584",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8ee7829d-0b18-4d73-b07f-6035045ece9d",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:10.7793364",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "fe3ab84e-cb32-4076-a979-f8f7b34225ea",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/02825a5b-00e4-46c6-94b9-070d4d23775e",
        "data": {
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:10.6772061",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "19fbb0ed-5001-4930-8ae8-9da90c878c46",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:35:07.983613",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5f03e480-9709-46f5-bac9-94dfd33fc354",
        "type": "milestone.submitted",
        "severity": "Information",
        "title": "تم تسليم أعمال المرحلة",
        "body": "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:35:04.7812945",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "0063a8e5-cc4e-4ce0-8514-746e959b4077",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "21765f91-670d-4888-bce9-9cde08249716"
        },
        "createdAtUtc": "2026-08-09T15:34:59.9616866",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "976f0e7b-f3dd-40c2-aedd-cfece1f2ed2c",
        "type": "milestone.change-request-approved",
        "severity": "Success",
        "title": "تمت الموافقة على طلب التعديل",
        "body": "وافق الطرف الآخر على طلب تعديل المرحلة، وطُبّقت الشروط المعتمدة.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "2970841a-eee0-4904-a1b8-bc5b6a9bf67d"
        },
        "createdAtUtc": "2026-08-09T15:34:58.7538494",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "23e10d77-6701-4a6c-9ee1-952a82e64350",
        "type": "milestone.ready-for-funding",
        "severity": "Information",
        "title": "المرحلة جاهزة للتمويل",
        "body": "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.592442",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "567162f0-c05c-490d-87a9-d16a7e606ebe",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "d51a0203-c2b6-432d-bbbb-38fbc4455b05",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "bf078e91-f5af-4015-8573-ef6170bbf566",
        "type": "milestone.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على المرحلة",
        "body": "وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:51.3581465",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "50bb9c8d-5e13-43cd-9b67-c441da71a4b7",
        "type": "milestone.created",
        "severity": "Information",
        "title": "مرحلة تعاقدية جديدة",
        "body": "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:48.4656041",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "3d653c37-99ec-4cc2-a62e-578c78417519",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.8428007",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4b117158-e888-4117-a27d-908c9f0e1f08",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.7168952",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 19
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll lawyer for automatic acceptance

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "57403d90-3eff-48ec-8b11-38d413eafff1",
        "type": "milestone.auto-accepted",
        "severity": "Success",
        "title": "تم قبول المرحلة تلقائيًا",
        "body": "قُبلت أعمال المرحلة تلقائيًا بعد انتهاء مدة المراجعة، وبدأت مدة حجز المبلغ.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:24.9186209",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "45460acf-b15e-4515-84ff-2a698959fe61",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:12.2174038",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "0fd8313e-9484-4825-9b84-3bc3ace8fea9",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:11.999068",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "a82ab3fd-8b21-42f1-a177-f70301c6867a",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:11.7855637",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "61f5ab7f-d8cc-4e3b-86cb-9ea4376d31d4",
        "type": "milestone.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على المرحلة",
        "body": "وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "0ac8fbef-b1e5-4ef4-a86c-6bb00be157e4",
          "contractId": "2d798205-6229-4377-bfe8-dfcebaa8e887",
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:11.4280228",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "cf0fecf5-54ef-4a83-b649-2dd59aa133c1",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/02825a5b-00e4-46c6-94b9-070d4d23775e",
        "data": {
          "proposalId": "02825a5b-00e4-46c6-94b9-070d4d23775e",
          "legalCaseId": "95e1707e-9223-43a3-9fef-0da124dfacae"
        },
        "createdAtUtc": "2026-08-09T15:35:10.5414215",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f7de5021-20fd-4e3e-ba8d-f5c0fd456a80",
        "type": "milestone.accepted",
        "severity": "Success",
        "title": "تم قبول أعمال المرحلة",
        "body": "قبل العميل أعمال المرحلة، وبدأت مدة حجز المبلغ قبل إتاحته للصرف.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:35:08.1527704",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8160e1c4-1a4b-463d-a034-f804d301420e",
        "type": "milestone.changes-requested",
        "severity": "Warning",
        "title": "طُلبت تعديلات على المرحلة",
        "body": "طلب العميل تعديلات على أعمال المرحلة، ويمكنك مراجعة الطلب وإعادة التسليم.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:35:06.8033973",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "9653addb-c9be-4181-a7b6-d8627863695c",
        "type": "milestone.change-request-cancelled",
        "severity": "Information",
        "title": "تم إلغاء طلب تعديل المرحلة",
        "body": "ألغى الطرف الآخر طلب تعديل المرحلة، ولم يعد القرار مطلوبًا منك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "1eb59f8e-561e-475e-85cb-2852c1cf754d"
        },
        "createdAtUtc": "2026-08-09T15:35:02.2669675",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "98a967a8-a4a2-41b4-a64c-023f4735d487",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "1eb59f8e-561e-475e-85cb-2852c1cf754d"
        },
        "createdAtUtc": "2026-08-09T15:35:01.6959202",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "c66eec0b-7862-4697-9219-5b536aa1db66",
        "type": "milestone.change-request-rejected",
        "severity": "Warning",
        "title": "تم رفض طلب تعديل المرحلة",
        "body": "رفض الطرف الآخر طلب تعديل المرحلة. يمكنك مراجعة الطلب لمعرفة التفاصيل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "21765f91-670d-4888-bce9-9cde08249716"
        },
        "createdAtUtc": "2026-08-09T15:35:00.4503662",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f20d165c-64f5-4373-9406-38f62219b42f",
        "type": "milestone.change-request-created",
        "severity": "Information",
        "title": "طلب تعديل جديد للمرحلة",
        "body": "أنشأ الطرف الآخر طلبًا لتعديل شروط المرحلة، ويحتاج الطلب إلى قرارك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4",
          "changeRequestId": "2970841a-eee0-4904-a1b8-bc5b6a9bf67d"
        },
        "createdAtUtc": "2026-08-09T15:34:57.2565009",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "2047772e-18bc-49ba-b4d9-29bc500c19d8",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:55.0816108",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "01245fe7-9b7f-48ba-ad5b-08fa86278ec9",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:54.8678438",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5f5b4323-733d-4ea1-9ba3-9ff023c517cd",
        "type": "milestone.approved",
        "severity": "Success",
        "title": "تم اعتماد المرحلة",
        "body": "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:52.4910555",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "7717c90a-5764-4322-b058-04ab901348b7",
        "type": "milestone.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث المرحلة",
        "body": "تم تحديث شروط المرحلة، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "milestoneId": "cf3fc657-597d-4ab2-a4af-d610db9a9d57",
          "contractId": "4f48425d-4302-4ba8-9972-2fa711bce61c",
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:50.0378639",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "5d9f29c5-e4f8-4f89-ae66-abc62b9a1c1a",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/e63169c3-e99a-455e-b151-607f3d03600a",
        "data": {
          "proposalId": "e63169c3-e99a-455e-b151-607f3d03600a",
          "legalCaseId": "8454613c-7106-432c-bf00-a319395d94d4"
        },
        "createdAtUtc": "2026-08-09T15:34:47.5711378",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 17
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client receives role-specific auto acceptance**
- [PASS] **Lawyer receives role-specific auto acceptance**

## Unsupported methods and recipient isolation

### DELETE milestone collection unsupported

**Request:** DELETE http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c/milestones

**Response Status:** 405

**Response Body:**
(Empty)
---


- [PASS] **DELETE milestone collection unsupported** (status=405)
### PATCH milestone unsupported

**Request:** PATCH http://localhost:5049/api/contracts/4f48425d-4302-4ba8-9972-2fa711bce61c/milestones/cf3fc657-597d-4ab2-a4af-d610db9a9d57

**Response Status:** 405

**Response Body:**
(Empty)
---


- [PASS] **PATCH milestone unsupported** (status=405)
### DELETE change request unsupported

**Request:** DELETE http://localhost:5049/api/change-requests/2970841a-eee0-4904-a1b8-bc5b6a9bf67d

**Response Status:** 404

**Response Body:**
(Empty)
---


- [PASS] **DELETE change request unsupported** (status=404)
### Get unrelated user notifications

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [],
    "nextCursor": null,
    "unreadCount": 0
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **No Milestone notification leaks to unrelated user**

## Execution summary

| Metric | Count |
|---|---:|
| Passed assertions | 143 |
| Failed assertions | 0 |

# Contracts Notifications HTTP Test Report

Generated at: 2026-08-09 17:28:19 +03:00


## Health and Contracts authorization boundary

### Health check

**Request:** GET http://localhost:5049/health

**Response Status:** 200

**Response Body:**
```text
Healthy
```
---


- [PASS] **API is healthy** (status=200)
### Create requires authentication

**Request:** POST http://localhost:5049/api/contracts

**Body:**
```json
{}
```

**Response Status:** 401

**Response Body:**
(Empty)
---


- [PASS] **Create requires authentication** (status=401)
### List requires authentication

**Request:** GET http://localhost:5049/api/contracts

**Response Status:** 401

**Response Body:**
(Empty)
---


- [PASS] **List requires authentication** (status=401)
### Detail requires authentication

**Request:** GET http://localhost:5049/api/contracts/b3553fd2-4252-4ed9-a0b9-914f27f2ecc1

**Response Status:** 401

**Response Body:**
(Empty)
---


- [PASS] **Detail requires authentication** (status=401)
### Update requires authentication

**Request:** PUT http://localhost:5049/api/contracts/b3553fd2-4252-4ed9-a0b9-914f27f2ecc1

**Body:**
```json
{}
```

**Response Status:** 401

**Response Body:**
(Empty)
---


- [PASS] **Update requires authentication** (status=401)
### Accept requires authentication

**Request:** POST http://localhost:5049/api/contracts/b3553fd2-4252-4ed9-a0b9-914f27f2ecc1/accept

**Body:**
```json
{}
```

**Response Status:** 401

**Response Body:**
(Empty)
---


- [PASS] **Accept requires authentication** (status=401)
### Terminate requires authentication

**Request:** POST http://localhost:5049/api/contracts/b3553fd2-4252-4ed9-a0b9-914f27f2ecc1/terminate

**Body:**
```json
{}
```

**Response Status:** 401

**Response Body:**
(Empty)
---


- [PASS] **Terminate requires authentication** (status=401)
### History requires authentication

**Request:** GET http://localhost:5049/api/contracts/b3553fd2-4252-4ed9-a0b9-914f27f2ecc1/state-history

**Response Status:** 401

**Response Body:**
(Empty)
---


- [PASS] **History requires authentication** (status=401)

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
    "refreshTokenExpiration": "2026-08-16T14:28:20.0146665Z"
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
    "userId": "0d0244fc-4e43-45be-9c64-08def62157b7",
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

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=0d0244fc-4e43-45be-9c64-08def62157b7&token=[REDACTED]

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
      "id": "0d0244fc-4e43-45be-9c64-08def62157b7",
      "email": "[REDACTED]",
      "fullName": "Contracts Notifications client",
      "role": "Client",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "[REDACTED]",
    "expiresIn": 900,
    "refreshToken": "[REDACTED]",
    "refreshTokenExpiration": "2026-08-16T14:28:22.2663091Z"
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
  "DateOfBirth": "1990-01-01",
  "Gender": 1,
  "PhoneNumber": "[REDACTED]",
  "Address": "Cairo",
  "NationalNumber": "[REDACTED]"
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

**Request:** PATCH http://localhost:5049/api/admin/verifications/0d0244fc-4e43-45be-9c64-08def62157b7/approve-account

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
      "id": "0d0244fc-4e43-45be-9c64-08def62157b7",
      "email": "[REDACTED]",
      "fullName": "Contracts Notifications client",
      "role": "Client",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "[REDACTED]",
    "expiresIn": 900,
    "refreshToken": "[REDACTED]",
    "refreshTokenExpiration": "2026-08-16T14:28:22.8306023Z"
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
    "userId": "834d9fb0-5727-4d61-9c65-08def62157b7",
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

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=834d9fb0-5727-4d61-9c65-08def62157b7&token=[REDACTED]

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
      "id": "834d9fb0-5727-4d61-9c65-08def62157b7",
      "email": "[REDACTED]",
      "fullName": "Contracts Notifications lawyer",
      "role": "Lawyer",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "[REDACTED]",
    "expiresIn": 900,
    "refreshToken": "[REDACTED]",
    "refreshTokenExpiration": "2026-08-16T14:28:24.7022006Z"
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
  "PhoneNumber": "[REDACTED]",
  "Address": "Cairo",
  "Bio": "Contracts notification lifecycle lawyer",
  "Specializations": [
    {
      "YearsOfExperience": 5,
      "Specialization": 1,
      "CasesHandled": 10
    }
  ],
  "Gender": 1,
  "DateOfBirth": "1985-01-01",
  "NationalNumber": "[REDACTED]",
  "Level": 1
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

**Request:** PATCH http://localhost:5049/api/admin/verifications/834d9fb0-5727-4d61-9c65-08def62157b7/approve-account

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
      "id": "834d9fb0-5727-4d61-9c65-08def62157b7",
      "email": "[REDACTED]",
      "fullName": "Contracts Notifications lawyer",
      "role": "Lawyer",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "[REDACTED]",
    "expiresIn": 900,
    "refreshToken": "[REDACTED]",
    "refreshTokenExpiration": "2026-08-16T14:28:25.2828668Z"
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
    "userId": "079478ba-5e1b-4d2e-9c66-08def62157b7",
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

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=079478ba-5e1b-4d2e-9c66-08def62157b7&token=[REDACTED]

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
      "id": "079478ba-5e1b-4d2e-9c66-08def62157b7",
      "email": "[REDACTED]",
      "fullName": "Contracts Notifications attacker",
      "role": "Client",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "[REDACTED]",
    "expiresIn": 900,
    "refreshToken": "[REDACTED]",
    "refreshTokenExpiration": "2026-08-16T14:28:27.4101586Z"
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
  "DateOfBirth": "1990-01-01",
  "Gender": 1,
  "PhoneNumber": "[REDACTED]",
  "Address": "Cairo",
  "NationalNumber": "[REDACTED]"
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

**Request:** PATCH http://localhost:5049/api/admin/verifications/079478ba-5e1b-4d2e-9c66-08def62157b7/approve-account

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
      "id": "079478ba-5e1b-4d2e-9c66-08def62157b7",
      "email": "[REDACTED]",
      "fullName": "Contracts Notifications attacker",
      "role": "Client",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "[REDACTED]",
    "expiresIn": 900,
    "refreshToken": "[REDACTED]",
    "refreshTokenExpiration": "2026-08-16T14:28:28.160159Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Re-login approved attacker** (status=200)

## Contract create endpoint validation and creation notification

### primary - create case

**Request:** POST http://localhost:5049/api/Case

**Body:**
```json
{
  "Description": "Complete case foundation for primary contract notifications.",
  "City": "Maadi",
  "Governorate": "Cairo",
  "Title": "primary case 172828287"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "caseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0",
    "failedDocuments": []
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **primary - create case** (status=200)
### primary - review case

**Request:** POST http://localhost:5049/api/cases/d4cc8af6-91b4-4fb3-908c-b730247d01a0/review

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
    "id": "ded26cd3-a11b-4193-829b-50f148d9836f",
    "caseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0",
    "isLatest": true,
    "createdAt": "2026-08-09T14:28:30.4027632Z",
    "reviewPoints": [
      {
        "id": "620f7b64-8374-4c59-8c65-ac907d2912e2",
        "description": "تتمثل نقطة القوة الأساسية في صياغة الموضوع بوضوح حول 'primary case 172828287'، وتوافر السند المبدئي الذي يرجح كفة الموكل في إثبات أصل الالتزام وتفوقه إثباتياً على الخصم.",
        "type": "Strength"
      },
      {
        "id": "4b9695cf-779d-4076-8625-4ed03d5faf6b",
        "description": "ميزة الخصم تتمثل في غياب التوثيق الرسمي للتنبيهات أو الإخطارات المتبادلة بين الأطراف، مما يتيح له إنكار الاستلام أو الدفع بالتراخي في المطالبة.",
        "type": "Weakness"
      },
      {
        "id": "71956bd3-58e1-4bb9-aaa4-28f2494928d3",
        "description": "يحتاج الملف إلى استيفاء النقاط والمعلومات التالية لضمان صياغة صحيفة الدعوى بشكل مكتمل: حصر وتفصيص المبالغ المالية المطلوبة والتعويضات الدقيقة عن الضرر المادي والمعنوي، وإدراج التواريخ الرسمية الدقيقة لبدء النزاع وتاريخ الإخلال بالتعهدات.",
        "type": "MissingCaseInfo"
      },
      {
        "id": "66a17eb5-2b75-408e-90ea-8350c21c7f9b",
        "description": "المستندات المحددة المطلوبة لإكمال الملف: أصل العقد/الاتفاق المبرم، صورة بطاقة الرقم القومي سارية لكل أطراف الدعوى، إيصالات التحويل أو السداد المالي، وأي إنذارات رسمية على يد محضر.",
        "type": "MissingCaseDoc"
      },
      {
        "id": "a294941e-152f-4f6c-9c35-b43fe8ce0d1c",
        "description": "قم بإعادة هيكلة وصف القضية في صورة جدول زمني متسلسل، يبدأ من تاريخ التعهد الأول، مروراً بتاريخ الإخلال، وصولاً إلى حجم الأضرار المترتبة حالياً.",
        "type": "Suggestion"
      },
      {
        "id": "90721cc8-abba-4cf9-b288-616912b3a908",
        "description": "قم بتفقيط وقسمة كافة المطالبات المالية إلى بنود مستقلة (أصل الدين، الفوائد أو التعويض عن المماطلة، والرسوم) وتوثيق كل بند بسند كتابي مستقل.",
        "type": "Suggestion"
      },
      {
        "id": "05a015dd-fef0-42ac-b2de-2afe065c2579",
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


- [PASS] **primary - review case** (status=200)
### primary - finalize case

**Request:** POST http://localhost:5049/api/Case/d4cc8af6-91b4-4fb3-908c-b730247d01a0/finalize

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
    "caseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0",
    "totalEligibleLawyers": 0,
    "recommendations": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **primary - finalize case** (status=200)
### primary - create proposal

**Request:** POST http://localhost:5049/api/proposals

**Body:**
```json
{
  "LegalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0",
  "Message": "primary proposal for contract notification lifecycle.",
  "LawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "60694a52-77d0-46cc-ac37-03fc63084417",
    "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0",
    "caseTitle": "primary case 172828287",
    "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
    "clientName": "Contracts Notifications client",
    "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
    "lawyerName": "Contracts Notifications lawyer",
    "message": "primary proposal for contract notification lifecycle.",
    "status": "Pending",
    "decisionReason": null,
    "createdAt": "2026-08-09T14:28:31.0566731",
    "respondedAt": null,
    "updatedAt": "2026-08-09T14:28:31.0566731",
    "conversationId": null
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **primary - create proposal** (status=200)
### primary - accept proposal

**Request:** POST http://localhost:5049/api/proposals/60694a52-77d0-46cc-ac37-03fc63084417/accept

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
    "id": "60694a52-77d0-46cc-ac37-03fc63084417",
    "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0",
    "caseTitle": "primary case 172828287",
    "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
    "clientName": "Contracts Notifications client",
    "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
    "lawyerName": "Contracts Notifications lawyer",
    "message": "primary proposal for contract notification lifecycle.",
    "status": "Accepted",
    "decisionReason": null,
    "createdAt": "2026-08-09T14:28:31.0566731",
    "respondedAt": "2026-08-09T14:28:31.1031084",
    "updatedAt": "2026-08-09T14:28:31.1031084",
    "conversationId": "6b8e1217-1ba3-4089-9ede-c84e4945c448"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **primary - accept proposal** (status=200)
### Create missing ProposalId

**Request:** POST http://localhost:5049/api/contracts

**Body:**
```json
{
  "TermsAndConditions": "Valid contract terms long enough for validation.",
  "Title": "Valid contract title"
}
```

**Response Status:** 400

**Response Body:**
```text
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 80 114 111 112 111 115 97 108 73 100 34 58 91 34 217 133 216 185 216 177 217 145 217 129 32 216 167 217 132 216 185 216 177 216 182 32 217 133 216 183 217 132 217 136 216 168 46 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 53 57 98 54 101 52 98 56 53 98 100 101 56 55 55 50 55 48 97 54 51 56 53 97 101 55 102 97 99 50 50 52 45 48 53 54 50 97 55 102 57 97 49 50 48 56 54 98 56 45 48 48 34 125
```
---


- [PASS] **Create missing ProposalId returns 400** (status=400)
### Client cannot create contract

**Request:** POST http://localhost:5049/api/contracts

**Body:**
```json
{
  "ProposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
  "TermsAndConditions": "This valid body must still be rejected by role authorization.",
  "Title": "Client attempted contract"
}
```

**Response Status:** 403

**Response Body:**
(Empty)
---


- [PASS] **Client create is forbidden** (status=403)
### Create extreme title

**Request:** POST http://localhost:5049/api/contracts

**Body:**
```json
{
  "ProposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
  "TermsAndConditions": "Valid terms remain present while title exceeds its allowed length.",
  "Title": "ععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععععع"
}
```

**Response Status:** 400

**Response Body:**
```text
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 84 105 116 108 101 34 58 91 34 216 185 217 134 217 136 216 167 217 134 32 216 167 217 132 216 185 217 130 216 175 32 217 138 216 172 216 168 32 216 163 217 134 32 217 138 217 131 217 136 217 134 32 216 168 217 138 217 134 32 51 32 217 136 50 48 48 32 216 173 216 177 217 129 46 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 57 102 97 52 100 50 55 49 100 56 54 101 49 55 51 56 51 99 51 51 102 57 54 99 55 97 53 99 99 51 101 50 45 56 53 97 51 57 57 100 97 102 53 57 99 98 100 57 50 45 48 48 34 125
```
---


- [PASS] **Extreme create title returns 400** (status=400)
### Create hostile body against unknown proposal

**Request:** POST http://localhost:5049/api/contracts

**Body:**
```json
{
  "ProposalId": "104981dc-fef8-47cf-9723-9bdd2692393f",
  "TermsAndConditions": "' OR 1=1; DROP TABLE Contracts;-- with enough bounded text.",
  "Title": "<script>alert('xss')</script> عقد ☠"
}
```

**Response Status:** 400

**Response Body:**
```json
{
  "success": false,
  "data": null,
  "message": "العرض غير موجود أو لم تتم الموافقة عليه.",
  "errors": null,
  "statusCode": 400
}
```
---


- [PASS] **Hostile create is rejected without 500** (status=400)
### primary - create contract

**Request:** POST http://localhost:5049/api/contracts

**Body:**
```json
{
  "ProposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
  "TermsAndConditions": "These complete contract terms are used for the primary notification lifecycle and are accepted by both participants.",
  "Title": "primary legal representation contract"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "4e821858-2345-4120-bc2e-6417d80c7658",
    "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
    "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0",
    "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
    "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
    "title": "primary legal representation contract",
    "termsAndConditions": "These complete contract terms are used for the primary notification lifecycle and are accepted by both participants.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": null,
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 0,
    "version": "\"AAAAAAAACfc=\"",
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


- [PASS] **primary - create contract** (status=200)
- [PASS] **primary create envelope retains logical 201**
### Poll client for contract.created

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


### Poll client for contract.created

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "f70f04eb-fa76-494a-9abb-5f39d3e9a260",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.2904189",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "e4d1ebaa-3b82-4b07-b614-be8d060403dd",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/60694a52-77d0-46cc-ac37-03fc63084417",
        "data": {
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.1064461",
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


- [PASS] **Client receives exact contract.created**
### Duplicate contract for proposal

**Request:** POST http://localhost:5049/api/contracts

**Body:**
```json
{
  "ProposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
  "TermsAndConditions": "This otherwise valid contract must be rejected as duplicate.",
  "Title": "Duplicate proposal contract"
}
```

**Response Status:** 409

**Response Body:**
```json
{
  "success": false,
  "data": null,
  "message": "تم إنشاء عقد لهذا العرض مسبقًا.",
  "errors": null,
  "statusCode": 409
}
```
---


- [PASS] **Duplicate proposal returns 409** (status=409)

## List, detail, history, filtering, ownership, and headers

### List client contracts

**Request:** GET http://localhost:5049/api/contracts?page=1&pageSize=10

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "4e821858-2345-4120-bc2e-6417d80c7658",
        "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0",
        "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
        "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
        "title": "primary legal representation contract",
        "currency": "EGP",
        "status": 0,
        "activatedAt": null,
        "completedAt": null
      }
    ],
    "page": 1,
    "pageSize": 10,
    "totalCount": 1,
    "hasNextPage": false
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client list contains primary contract**
### List lawyer contracts

**Request:** GET http://localhost:5049/api/contracts?page=1&pageSize=10

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "4e821858-2345-4120-bc2e-6417d80c7658",
        "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0",
        "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
        "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
        "title": "primary legal representation contract",
        "currency": "EGP",
        "status": 0,
        "activatedAt": null,
        "completedAt": null
      }
    ],
    "page": 1,
    "pageSize": 10,
    "totalCount": 1,
    "hasNextPage": false
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Lawyer list contains primary contract**
### Unrelated client list is isolated

**Request:** GET http://localhost:5049/api/contracts?page=1&pageSize=10

**Response Status:** 200

**Response Body:**
```json
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
```
---


- [PASS] **Unrelated list does not leak contract**
### List validation - Negative page

**Request:** GET http://localhost:5049/api/contracts?page=-1&pageSize=10

**Response Status:** 400

**Response Body:**
```text
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 80 97 103 101 34 58 91 34 216 177 217 130 217 133 32 216 167 217 132 216 181 217 129 216 173 216 169 32 217 138 216 172 216 168 32 216 163 217 134 32 217 138 217 131 217 136 217 134 32 49 32 216 163 217 136 32 216 163 217 131 216 168 216 177 46 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 101 52 97 100 100 49 56 49 99 101 51 99 100 49 99 102 54 100 102 100 98 50 54 49 102 52 53 54 51 55 98 54 45 49 55 49 51 97 57 100 102 51 98 98 98 100 101 49 57 45 48 48 34 125
```
---


- [PASS] **List Negative page returns 400** (status=400)
### List validation - Oversized page

**Request:** GET http://localhost:5049/api/contracts?page=1&pageSize=101

**Response Status:** 400

**Response Body:**
```text
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 80 97 103 101 83 105 122 101 34 58 91 34 216 173 216 172 217 133 32 216 167 217 132 216 181 217 129 216 173 216 169 32 217 138 216 172 216 168 32 216 163 217 134 32 217 138 217 131 217 136 217 134 32 216 168 217 138 217 134 32 49 32 217 136 49 48 48 46 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 98 102 102 52 48 52 99 52 57 55 100 100 102 48 55 56 97 55 51 51 49 101 49 102 55 51 99 57 51 57 98 52 45 97 51 52 99 51 48 50 97 57 53 55 52 97 99 53 55 45 48 48 34 125
```
---


- [PASS] **List Oversized page returns 400** (status=400)
### List validation - Wrong page type

**Request:** GET http://localhost:5049/api/contracts?page=abc&pageSize=10

**Response Status:** 400

**Response Body:**
```text
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 80 97 103 101 34 58 91 34 84 104 101 32 118 97 108 117 101 32 39 97 98 99 39 32 105 115 32 110 111 116 32 118 97 108 105 100 46 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 102 50 99 97 99 100 97 97 101 101 52 48 101 97 100 99 49 50 56 101 55 101 99 53 100 54 51 100 53 98 49 99 45 55 51 102 51 101 97 54 53 51 53 50 56 51 49 54 48 45 48 48 34 125
```
---


- [PASS] **List Wrong page type returns 400** (status=400)
### List validation - Invalid status

**Request:** GET http://localhost:5049/api/contracts?status=not-a-status

**Response Status:** 400

**Response Body:**
```text
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 83 116 97 116 117 115 34 58 91 34 84 104 101 32 118 97 108 117 101 32 39 110 111 116 45 97 45 115 116 97 116 117 115 39 32 105 115 32 110 111 116 32 118 97 108 105 100 46 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 50 50 49 51 102 51 54 57 55 57 49 56 56 100 97 48 55 101 97 56 53 51 49 102 52 48 53 50 52 99 101 50 45 50 53 100 53 50 102 97 50 102 49 50 99 98 98 50 52 45 48 48 34 125
```
---


- [PASS] **List Invalid status returns 400** (status=400)
### List validation - Unicode status

**Request:** GET http://localhost:5049/api/contracts?status=%D8%B9%D9%82%D8%AF%E2%98%A0

**Response Status:** 400

**Response Body:**
```text
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 83 116 97 116 117 115 34 58 91 34 84 104 101 32 118 97 108 117 101 32 39 216 185 217 130 216 175 226 152 160 39 32 105 115 32 110 111 116 32 118 97 108 105 100 46 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 57 55 52 49 100 55 49 99 49 50 49 55 52 57 56 97 52 53 101 56 52 99 56 102 53 51 98 55 100 48 50 56 45 52 52 55 98 51 97 101 100 48 99 49 101 56 51 51 51 45 48 48 34 125
```
---


- [PASS] **List Unicode status returns 400** (status=400)
### Get primary contract as client

**Request:** GET http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "4e821858-2345-4120-bc2e-6417d80c7658",
    "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
    "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0",
    "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
    "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
    "title": "primary legal representation contract",
    "termsAndConditions": "These complete contract terms are used for the primary notification lifecycle and are accepted by both participants.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": null,
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 0,
    "version": "\"AAAAAAAACfc=\"",
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
  "statusCode": 200
}
```
---


- [PASS] **Get primary contract as client** (status=200)
### Unrelated user cannot read detail

**Request:** GET http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658

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


- [PASS] **Unrelated detail is forbidden** (status=403)
### Unknown contract detail

**Request:** GET http://localhost:5049/api/contracts/b3553fd2-4252-4ed9-a0b9-914f27f2ecc1

**Response Status:** 404

**Response Body:**
```json
{
  "success": false,
  "data": null,
  "message": "العقد غير موجود.",
  "errors": null,
  "statusCode": 404
}
```
---


- [PASS] **Unknown detail returns 404** (status=404)
### Non-Guid contract route

**Request:** GET http://localhost:5049/api/contracts/not-a-guid

**Response Status:** 404

**Response Body:**
(Empty)
---


- [PASS] **Non-Guid detail route returns 404** (status=404)
### Detail with unusual Accept header

**Request:** GET http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "4e821858-2345-4120-bc2e-6417d80c7658",
    "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
    "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0",
    "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
    "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
    "title": "primary legal representation contract",
    "termsAndConditions": "These complete contract terms are used for the primary notification lifecycle and are accepted by both participants.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": null,
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 0,
    "version": "\"AAAAAAAACfc=\"",
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
  "statusCode": 200
}
```
---


- [PASS] **Unusual Accept header never causes 500** (status=200)
### Get primary state history

**Request:** GET http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658/state-history?page=1&pageSize=20

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "b687d353-827e-4ebe-a58b-3106b828456e",
        "previousStatus": null,
        "newStatus": 0,
        "trigger": "ContractCreated",
        "actorUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
        "reason": "تم إنشاء مسودة العقد من العرض المقبول.",
        "createdAt": "2026-08-09T14:28:31.2897427"
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 1,
    "hasNextPage": false
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **History returns creation audit**
### Unrelated user cannot read history

**Request:** GET http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658/state-history

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


- [PASS] **Unrelated history is forbidden** (status=403)
### History invalid page size

**Request:** GET http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658/state-history?page=0&pageSize=999

**Response Status:** 400

**Response Body:**
```text
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 80 97 103 101 34 58 91 34 216 177 217 130 217 133 32 216 167 217 132 216 181 217 129 216 173 216 169 32 217 138 216 172 216 168 32 216 163 217 134 32 217 138 217 131 217 136 217 134 32 49 32 216 163 217 136 32 216 163 217 131 216 168 216 177 46 34 93 44 34 80 97 103 101 83 105 122 101 34 58 91 34 216 173 216 172 217 133 32 216 167 217 132 216 181 217 129 216 173 216 169 32 217 138 216 172 216 168 32 216 163 217 134 32 217 138 217 131 217 136 217 134 32 216 168 217 138 217 134 32 49 32 217 136 49 48 48 46 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 57 100 97 98 98 97 101 49 54 99 50 50 49 50 101 102 52 101 55 101 53 101 101 53 102 50 49 102 57 53 49 98 45 56 56 56 98 98 100 55 98 97 98 48 48 48 51 49 49 45 48 48 34 125
```
---


- [PASS] **History invalid paging returns 400** (status=400)

## Acceptance, update reset, and draft-updated notification

### Client first acceptance

**Request:** POST http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658/accept

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
    "entityId": "4e821858-2345-4120-bc2e-6417d80c7658",
    "status": "Draft",
    "occurredAt": "2026-08-09T14:28:32.8742352Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client first acceptance** (status=200)
### Poll lawyer for contract.acceptance-recorded

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "19ac3cb1-78ea-4d1c-aee7-0eecc9dc0c99",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/60694a52-77d0-46cc-ac37-03fc63084417",
        "data": {
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.0568395",
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


### Poll lawyer for contract.acceptance-recorded

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "d69c72f5-0168-43ff-8af8-02f3235a6686",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:32.8758527",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "19ac3cb1-78ea-4d1c-aee7-0eecc9dc0c99",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/60694a52-77d0-46cc-ac37-03fc63084417",
        "data": {
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.0568395",
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


- [PASS] **Lawyer receives exact first acceptance**
### Refresh contract before update

**Request:** GET http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "4e821858-2345-4120-bc2e-6417d80c7658",
    "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
    "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0",
    "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
    "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
    "title": "primary legal representation contract",
    "termsAndConditions": "These complete contract terms are used for the primary notification lifecycle and are accepted by both participants.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": "2026-08-09T14:28:32.8742352",
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 0,
    "version": "\"AAAAAAAACgY=\"",
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
  "statusCode": 200
}
```
---


- [PASS] **Refresh contract before update** (status=200)
### Client cannot update draft

**Request:** PUT http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658

**Body:**
```json
{
  "TermsAndConditions": "تم تحديث شروط العقد بصورة واضحة، ويلزم الطرفان مراجعتها والموافقة عليها من جديد.",
  "Title": "عقد التمثيل القانوني المعدل ⚖"
}
```

**Response Status:** 403

**Response Body:**
(Empty)
---


- [PASS] **Client update is forbidden** (status=403)
### Update missing If-Match

**Request:** PUT http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658

**Body:**
```json
{
  "TermsAndConditions": "تم تحديث شروط العقد بصورة واضحة، ويلزم الطرفان مراجعتها والموافقة عليها من جديد.",
  "Title": "عقد التمثيل القانوني المعدل ⚖"
}
```

**Response Status:** 400

**Response Body:**
```json
{
  "success": false,
  "data": null,
  "message": "قيمة If-Match مطلوبة. قيمة If-Match يجب أن تكون وسم ETag قويًا يحتوي على rowversion مشفّر بصيغة base64 بين علامتي اقتباس.",
  "errors": null,
  "statusCode": 400
}
```
---


- [PASS] **Update missing If-Match returns 400** (status=400)
### Update extreme terms

**Request:** PUT http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658

**Body:**
```json
{
  "TermsAndConditions": "ششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششششش",
  "Title": "Valid updated title"
}
```

**Response Status:** 400

**Response Body:**
```text
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 84 101 114 109 115 65 110 100 67 111 110 100 105 116 105 111 110 115 34 58 91 34 216 180 216 177 217 136 216 183 32 217 136 216 163 216 173 217 131 216 167 217 133 32 216 167 217 132 216 185 217 130 216 175 32 217 138 216 172 216 168 32 216 163 217 134 32 216 170 217 131 217 136 217 134 32 216 168 217 138 217 134 32 50 48 32 217 136 50 48 48 48 48 32 216 173 216 177 217 129 46 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 101 100 99 48 53 49 52 97 101 52 57 99 98 54 101 97 99 97 51 51 53 52 51 52 97 55 97 57 52 52 102 49 45 52 48 98 53 102 54 57 53 55 99 53 48 102 49 56 49 45 48 48 34 125
```
---


- [PASS] **Extreme update terms returns 400** (status=400)
### Lawyer updates draft

**Request:** PUT http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658

**Body:**
```json
{
  "TermsAndConditions": "تم تحديث شروط العقد بصورة واضحة، ويلزم الطرفان مراجعتها والموافقة عليها من جديد.",
  "Title": "عقد التمثيل القانوني المعدل ⚖"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "4e821858-2345-4120-bc2e-6417d80c7658",
    "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
    "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0",
    "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
    "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
    "title": "عقد التمثيل القانوني المعدل ⚖",
    "termsAndConditions": "تم تحديث شروط العقد بصورة واضحة، ويلزم الطرفان مراجعتها والموافقة عليها من جديد.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": null,
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 0,
    "version": "\"AAAAAAAACgs=\"",
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
  "statusCode": 200
}
```
---


- [PASS] **Lawyer updates draft** (status=200)
- [PASS] **Update clears both acceptances**
### Poll client for draft update

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "f70f04eb-fa76-494a-9abb-5f39d3e9a260",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.2904189",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "e4d1ebaa-3b82-4b07-b614-be8d060403dd",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/60694a52-77d0-46cc-ac37-03fc63084417",
        "data": {
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.1064461",
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


### Poll client for draft update

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "f70f04eb-fa76-494a-9abb-5f39d3e9a260",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.2904189",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "e4d1ebaa-3b82-4b07-b614-be8d060403dd",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/60694a52-77d0-46cc-ac37-03fc63084417",
        "data": {
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.1064461",
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


### Poll client for draft update

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "17764033-8b8b-47ae-ade4-baaf04353645",
        "type": "contract.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث مسودة العقد",
        "body": "تم تحديث شروط العقد، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:33.8489103",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f70f04eb-fa76-494a-9abb-5f39d3e9a260",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.2904189",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "e4d1ebaa-3b82-4b07-b614-be8d060403dd",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/60694a52-77d0-46cc-ac37-03fc63084417",
        "data": {
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.1064461",
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


- [PASS] **Client receives exact draft update**
### Update with stale version

**Request:** PUT http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658

**Body:**
```json
{
  "TermsAndConditions": "تم تحديث شروط العقد بصورة واضحة، ويلزم الطرفان مراجعتها والموافقة عليها من جديد.",
  "Title": "عقد التمثيل القانوني المعدل ⚖"
}
```

**Response Status:** 409

**Response Body:**
```json
{
  "success": false,
  "data": null,
  "message": "تم تعديل العقد بواسطة عملية أخرى. يرجى إعادة تحميله والمحاولة مرة أخرى.",
  "errors": null,
  "statusCode": 409
}
```
---


- [PASS] **Stale update is rejected** (status=409)

## Milestone prerequisite and contract activation notifications

### primary - add milestone

**Request:** POST http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658/milestones

**Body:**
```json
{
  "Description": "Approved milestone used for the contract lifecycle.",
  "DurationDays": 10,
  "Amount": 1000.0,
  "Title": "primary execution milestone",
  "OrderNumber": 1
}
```

**Response Status:** 201

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "0b7d0bd5-b513-44f1-880a-f5c6d364f546",
    "orderNumber": 1,
    "title": "primary execution milestone",
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
    "version": "\"AAAAAAAAChA=\"",
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


- [PASS] **primary - add milestone** (status=201)
### primary - list milestone for client ETag

**Request:** GET http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "0b7d0bd5-b513-44f1-880a-f5c6d364f546",
      "orderNumber": 1,
      "title": "primary execution milestone",
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
      "version": "\"AAAAAAAAChA=\"",
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


- [PASS] **primary - list milestone for client ETag** (status=200)
### primary - client approves milestone

**Request:** POST http://localhost:5049/api/milestones/0b7d0bd5-b513-44f1-880a-f5c6d364f546/approve

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
    "entityId": "0b7d0bd5-b513-44f1-880a-f5c6d364f546",
    "status": "Draft",
    "occurredAt": "2026-08-09T14:28:35.6719468Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **primary - client approves milestone** (status=200)
### primary - list milestone for lawyer ETag

**Request:** GET http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "0b7d0bd5-b513-44f1-880a-f5c6d364f546",
      "orderNumber": 1,
      "title": "primary execution milestone",
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
      "version": "\"AAAAAAAAChE=\"",
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


- [PASS] **primary - list milestone for lawyer ETag** (status=200)
### primary - lawyer approves milestone

**Request:** POST http://localhost:5049/api/milestones/0b7d0bd5-b513-44f1-880a-f5c6d364f546/approve

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
    "entityId": "0b7d0bd5-b513-44f1-880a-f5c6d364f546",
    "status": "AwaitingFunding",
    "occurredAt": "2026-08-09T14:28:35.7170808Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **primary - lawyer approves milestone** (status=200)
### Refresh for repeated client acceptance

**Request:** GET http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "4e821858-2345-4120-bc2e-6417d80c7658",
    "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
    "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0",
    "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
    "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
    "title": "عقد التمثيل القانوني المعدل ⚖",
    "termsAndConditions": "تم تحديث شروط العقد بصورة واضحة، ويلزم الطرفان مراجعتها والموافقة عليها من جديد.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": null,
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 1000.0,
    "version": "\"AAAAAAAACgs=\"",
    "milestones": [
      {
        "id": "0b7d0bd5-b513-44f1-880a-f5c6d364f546",
        "orderNumber": 1,
        "title": "primary execution milestone",
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
        "version": "\"AAAAAAAAChI=\""
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


- [PASS] **Refresh for repeated client acceptance** (status=200)
### Client accepts revised draft

**Request:** POST http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658/accept

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
    "entityId": "4e821858-2345-4120-bc2e-6417d80c7658",
    "status": "Draft",
    "occurredAt": "2026-08-09T14:28:35.772092Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client accepts revised draft** (status=200)
### Refresh after client acceptance

**Request:** GET http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "4e821858-2345-4120-bc2e-6417d80c7658",
    "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
    "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0",
    "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
    "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
    "title": "عقد التمثيل القانوني المعدل ⚖",
    "termsAndConditions": "تم تحديث شروط العقد بصورة واضحة، ويلزم الطرفان مراجعتها والموافقة عليها من جديد.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": "2026-08-09T14:28:35.772092",
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 1000.0,
    "version": "\"AAAAAAAAChQ=\"",
    "milestones": [
      {
        "id": "0b7d0bd5-b513-44f1-880a-f5c6d364f546",
        "orderNumber": 1,
        "title": "primary execution milestone",
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
        "version": "\"AAAAAAAAChI=\""
      }
    ],
    "payments": [],
    "permittedActions": [
      "Update",
      "Terminate"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Refresh after client acceptance** (status=200)
### Client repeats acceptance

**Request:** POST http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658/accept

**Body:**
```json
{}
```

**Response Status:** 409

**Response Body:**
```json
{
  "success": false,
  "data": null,
  "message": "قام العميل بقبول النسخة الحالية من العقد مسبقًا.",
  "errors": null,
  "statusCode": 409
}
```
---


- [PASS] **Repeated acceptance returns 409** (status=409)
### Refresh for final lawyer acceptance

**Request:** GET http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "4e821858-2345-4120-bc2e-6417d80c7658",
    "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
    "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0",
    "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
    "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
    "title": "عقد التمثيل القانوني المعدل ⚖",
    "termsAndConditions": "تم تحديث شروط العقد بصورة واضحة، ويلزم الطرفان مراجعتها والموافقة عليها من جديد.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": "2026-08-09T14:28:35.772092",
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 1000.0,
    "version": "\"AAAAAAAAChQ=\"",
    "milestones": [
      {
        "id": "0b7d0bd5-b513-44f1-880a-f5c6d364f546",
        "orderNumber": 1,
        "title": "primary execution milestone",
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
        "version": "\"AAAAAAAAChI=\""
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


- [PASS] **Refresh for final lawyer acceptance** (status=200)
### Attacker cannot accept

**Request:** POST http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658/accept

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
  "message": "هذا الإجراء متاح لطرفي العقد فقط.",
  "errors": null,
  "statusCode": 403
}
```
---


- [PASS] **Attacker acceptance is forbidden** (status=403)
### Lawyer final acceptance

**Request:** POST http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658/accept

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
    "entityId": "4e821858-2345-4120-bc2e-6417d80c7658",
    "status": "Active",
    "occurredAt": "2026-08-09T14:28:35.9414443Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Lawyer final acceptance** (status=200)
### Poll client for activation

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "17764033-8b8b-47ae-ade4-baaf04353645",
        "type": "contract.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث مسودة العقد",
        "body": "تم تحديث شروط العقد، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:33.8489103",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f70f04eb-fa76-494a-9abb-5f39d3e9a260",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.2904189",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "e4d1ebaa-3b82-4b07-b614-be8d060403dd",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/60694a52-77d0-46cc-ac37-03fc63084417",
        "data": {
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.1064461",
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


### Poll client for activation

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "17764033-8b8b-47ae-ade4-baaf04353645",
        "type": "contract.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث مسودة العقد",
        "body": "تم تحديث شروط العقد، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:33.8489103",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f70f04eb-fa76-494a-9abb-5f39d3e9a260",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.2904189",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "e4d1ebaa-3b82-4b07-b614-be8d060403dd",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/60694a52-77d0-46cc-ac37-03fc63084417",
        "data": {
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.1064461",
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


### Poll client for activation

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "f86e4b55-2aa6-4137-ae6d-32f662feed2f",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:35.9582728",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "17764033-8b8b-47ae-ade4-baaf04353645",
        "type": "contract.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث مسودة العقد",
        "body": "تم تحديث شروط العقد، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:33.8489103",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f70f04eb-fa76-494a-9abb-5f39d3e9a260",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.2904189",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "e4d1ebaa-3b82-4b07-b614-be8d060403dd",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/60694a52-77d0-46cc-ac37-03fc63084417",
        "data": {
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.1064461",
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


### Poll lawyer for activation

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "d7ce2978-4f92-4234-9890-a67b7aa9d256",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:35.9582728",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4e615ddd-c9cb-413b-9b49-88fbed1840c1",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:35.7739812",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "d69c72f5-0168-43ff-8af8-02f3235a6686",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:32.8758527",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "19ac3cb1-78ea-4d1c-aee7-0eecc9dc0c99",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/60694a52-77d0-46cc-ac37-03fc63084417",
        "data": {
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.0568395",
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


- [PASS] **Client receives exact activation**
- [PASS] **Lawyer receives exact activation**

## Termination without settlement and notifications

### Refresh active contract before termination

**Request:** GET http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "4e821858-2345-4120-bc2e-6417d80c7658",
    "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
    "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0",
    "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
    "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
    "title": "عقد التمثيل القانوني المعدل ⚖",
    "termsAndConditions": "تم تحديث شروط العقد بصورة واضحة، ويلزم الطرفان مراجعتها والموافقة عليها من جديد.",
    "currency": "EGP",
    "status": 1,
    "acceptedByClientAt": "2026-08-09T14:28:35.772092",
    "acceptedByLawyerAt": "2026-08-09T14:28:35.9414443",
    "activatedAt": "2026-08-09T14:28:35.9414443",
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 1000.0,
    "version": "\"AAAAAAAAChs=\"",
    "milestones": [
      {
        "id": "0b7d0bd5-b513-44f1-880a-f5c6d364f546",
        "orderNumber": 1,
        "title": "primary execution milestone",
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
        "version": "\"AAAAAAAAChI=\""
      }
    ],
    "payments": [],
    "permittedActions": [
      "Terminate"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Refresh active contract before termination** (status=200)
### Terminate missing reason

**Request:** POST http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658/terminate

**Body:**
```json
{}
```

**Response Status:** 400

**Response Body:**
```text
123 34 116 121 112 101 34 58 34 104 116 116 112 115 58 47 47 116 111 111 108 115 46 105 101 116 102 46 111 114 103 47 104 116 109 108 47 114 102 99 57 49 49 48 35 115 101 99 116 105 111 110 45 49 53 46 53 46 49 34 44 34 116 105 116 108 101 34 58 34 79 110 101 32 111 114 32 109 111 114 101 32 118 97 108 105 100 97 116 105 111 110 32 101 114 114 111 114 115 32 111 99 99 117 114 114 101 100 46 34 44 34 115 116 97 116 117 115 34 58 52 48 48 44 34 101 114 114 111 114 115 34 58 123 34 82 101 97 115 111 110 34 58 91 34 84 104 101 32 82 101 97 115 111 110 32 102 105 101 108 100 32 105 115 32 114 101 113 117 105 114 101 100 46 34 44 34 216 179 216 168 216 168 32 216 165 217 134 217 135 216 167 216 161 32 216 167 217 132 216 185 217 130 216 175 32 217 133 216 183 217 132 217 136 216 168 46 34 93 125 44 34 116 114 97 99 101 73 100 34 58 34 48 48 45 57 55 99 102 99 100 99 49 97 53 56 101 55 100 51 98 98 53 51 48 49 101 57 102 101 54 55 97 48 50 99 99 45 51 100 51 50 49 54 54 48 53 54 101 99 55 97 100 52 45 48 48 34 125
```
---


- [PASS] **Terminate missing reason returns 400** (status=400)
### Attacker cannot terminate

**Request:** POST http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658/terminate

**Body:**
```json
{
  "Reason": "Unauthorized termination attempt."
}
```

**Response Status:** 403

**Response Body:**
```json
{
  "success": false,
  "data": null,
  "message": "هذا الإجراء متاح لطرفي العقد فقط.",
  "errors": null,
  "statusCode": 403
}
```
---


- [PASS] **Attacker termination is forbidden** (status=403)
### Client terminates unfunded contract

**Request:** POST http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658/terminate

**Body:**
```json
{
  "Reason": "اتفق الطرفان على إنهاء العقد قبل بدء التمويل."
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "4e821858-2345-4120-bc2e-6417d80c7658",
    "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
    "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0",
    "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
    "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
    "title": "عقد التمثيل القانوني المعدل ⚖",
    "termsAndConditions": "تم تحديث شروط العقد بصورة واضحة، ويلزم الطرفان مراجعتها والموافقة عليها من جديد.",
    "currency": "EGP",
    "status": 4,
    "acceptedByClientAt": "2026-08-09T14:28:35.772092",
    "acceptedByLawyerAt": "2026-08-09T14:28:35.9414443",
    "activatedAt": "2026-08-09T14:28:35.9414443",
    "completedAt": null,
    "terminatedAt": "2026-08-09T14:28:37.8242625Z",
    "currentMilestoneTotal": 0,
    "version": "\"AAAAAAAACiY=\"",
    "milestones": [
      {
        "id": "0b7d0bd5-b513-44f1-880a-f5c6d364f546",
        "orderNumber": 1,
        "title": "primary execution milestone",
        "description": "Approved milestone used for the contract lifecycle.",
        "amount": 1000.0,
        "durationDays": 10,
        "dueDate": null,
        "status": 9,
        "fundingStatus": 0,
        "escrowHoldId": null,
        "fundedAt": null,
        "submittedAt": null,
        "autoAcceptEligibleAt": null,
        "holdExpiresAt": null,
        "netLawyerAmount": null,
        "version": "\"AAAAAAAACic=\""
      }
    ],
    "payments": [],
    "permittedActions": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client terminates unfunded contract** (status=200)
### Poll lawyer for termination request

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "d7ce2978-4f92-4234-9890-a67b7aa9d256",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:35.9582728",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4e615ddd-c9cb-413b-9b49-88fbed1840c1",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:35.7739812",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "d69c72f5-0168-43ff-8af8-02f3235a6686",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:32.8758527",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "19ac3cb1-78ea-4d1c-aee7-0eecc9dc0c99",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/60694a52-77d0-46cc-ac37-03fc63084417",
        "data": {
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.0568395",
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


### Poll lawyer for termination request

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "e71df79c-be86-498f-8f30-49096394f841",
        "type": "contract.terminated",
        "severity": "Warning",
        "title": "تم إنهاء العقد",
        "body": "اكتملت إجراءات إنهاء العقد وتسويته.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:37.8408834",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "3e7d8253-eb47-4140-ba3f-d743f3a8b5ba",
        "type": "contract.termination-requested",
        "severity": "Warning",
        "title": "تم طلب إنهاء العقد",
        "body": "تم تسجيل طلب إنهاء العقد، وتجري معالجة التسوية اللازمة.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:37.7823417",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "d7ce2978-4f92-4234-9890-a67b7aa9d256",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:35.9582728",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4e615ddd-c9cb-413b-9b49-88fbed1840c1",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:35.7739812",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "d69c72f5-0168-43ff-8af8-02f3235a6686",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:32.8758527",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "19ac3cb1-78ea-4d1c-aee7-0eecc9dc0c99",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/60694a52-77d0-46cc-ac37-03fc63084417",
        "data": {
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.0568395",
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


- [PASS] **Counterparty receives termination request**
### Poll client for termination

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "2d93d6f7-52b9-4580-a568-e1e99b4809e8",
        "type": "contract.terminated",
        "severity": "Warning",
        "title": "تم إنهاء العقد",
        "body": "اكتملت إجراءات إنهاء العقد وتسويته.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:37.8408834",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f86e4b55-2aa6-4137-ae6d-32f662feed2f",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:35.9582728",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "17764033-8b8b-47ae-ade4-baaf04353645",
        "type": "contract.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث مسودة العقد",
        "body": "تم تحديث شروط العقد، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:33.8489103",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f70f04eb-fa76-494a-9abb-5f39d3e9a260",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.2904189",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "e4d1ebaa-3b82-4b07-b614-be8d060403dd",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/60694a52-77d0-46cc-ac37-03fc63084417",
        "data": {
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.1064461",
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


### Poll lawyer for termination

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "e71df79c-be86-498f-8f30-49096394f841",
        "type": "contract.terminated",
        "severity": "Warning",
        "title": "تم إنهاء العقد",
        "body": "اكتملت إجراءات إنهاء العقد وتسويته.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:37.8408834",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "3e7d8253-eb47-4140-ba3f-d743f3a8b5ba",
        "type": "contract.termination-requested",
        "severity": "Warning",
        "title": "تم طلب إنهاء العقد",
        "body": "تم تسجيل طلب إنهاء العقد، وتجري معالجة التسوية اللازمة.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:37.7823417",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "d7ce2978-4f92-4234-9890-a67b7aa9d256",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:35.9582728",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4e615ddd-c9cb-413b-9b49-88fbed1840c1",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:35.7739812",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "d69c72f5-0168-43ff-8af8-02f3235a6686",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:32.8758527",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "19ac3cb1-78ea-4d1c-aee7-0eecc9dc0c99",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/60694a52-77d0-46cc-ac37-03fc63084417",
        "data": {
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.0568395",
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


- [PASS] **Client receives exact termination**
- [PASS] **Lawyer receives exact termination**
### Get terminated contract

**Request:** GET http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "4e821858-2345-4120-bc2e-6417d80c7658",
    "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
    "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0",
    "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
    "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
    "title": "عقد التمثيل القانوني المعدل ⚖",
    "termsAndConditions": "تم تحديث شروط العقد بصورة واضحة، ويلزم الطرفان مراجعتها والموافقة عليها من جديد.",
    "currency": "EGP",
    "status": 4,
    "acceptedByClientAt": "2026-08-09T14:28:35.772092",
    "acceptedByLawyerAt": "2026-08-09T14:28:35.9414443",
    "activatedAt": "2026-08-09T14:28:35.9414443",
    "completedAt": null,
    "terminatedAt": "2026-08-09T14:28:37.8242625",
    "currentMilestoneTotal": 0,
    "version": "\"AAAAAAAACiY=\"",
    "milestones": [
      {
        "id": "0b7d0bd5-b513-44f1-880a-f5c6d364f546",
        "orderNumber": 1,
        "title": "primary execution milestone",
        "description": "Approved milestone used for the contract lifecycle.",
        "amount": 1000.0,
        "durationDays": 10,
        "dueDate": null,
        "status": 9,
        "fundingStatus": 0,
        "escrowHoldId": null,
        "fundedAt": null,
        "submittedAt": null,
        "autoAcceptEligibleAt": null,
        "holdExpiresAt": null,
        "netLawyerAmount": null,
        "version": "\"AAAAAAAACic=\""
      }
    ],
    "payments": [],
    "permittedActions": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Get terminated contract** (status=200)
### Cannot terminate twice

**Request:** POST http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658/terminate

**Body:**
```json
{
  "Reason": "Repeated termination."
}
```

**Response Status:** 400

**Response Body:**
```json
{
  "success": false,
  "data": null,
  "message": "لا يمكن إنهاء عقد مكتمل أو منتهٍ.",
  "errors": null,
  "statusCode": 400
}
```
---


- [PASS] **Repeated final termination returns 400** (status=400)

## Funded settlement termination lifecycle

### settlement - create case

**Request:** POST http://localhost:5049/api/Case

**Body:**
```json
{
  "Description": "Complete case foundation for settlement contract notifications.",
  "City": "Maadi",
  "Governorate": "Cairo",
  "Title": "settlement case 172838914"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "caseId": "5486d524-1510-4df4-86e3-fbb20bdb1158",
    "failedDocuments": []
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **settlement - create case** (status=200)
### settlement - review case

**Request:** POST http://localhost:5049/api/cases/5486d524-1510-4df4-86e3-fbb20bdb1158/review

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
    "id": "abaafb42-e5b6-4230-81b2-1830d8bac259",
    "caseId": "5486d524-1510-4df4-86e3-fbb20bdb1158",
    "isLatest": true,
    "createdAt": "2026-08-09T14:28:40.341674Z",
    "reviewPoints": [
      {
        "id": "f896e9c0-2c61-4c31-b658-8ba8d0092428",
        "description": "تتمثل نقطة القوة الأساسية في صياغة الموضوع بوضوح حول 'settlement case 172838914'، وتوافر السند المبدئي الذي يرجح كفة الموكل في إثبات أصل الالتزام وتفوقه إثباتياً على الخصم.",
        "type": "Strength"
      },
      {
        "id": "808f578d-6bc5-44c4-b1d3-0cb32416871e",
        "description": "ميزة الخصم تتمثل في غياب التوثيق الرسمي للتنبيهات أو الإخطارات المتبادلة بين الأطراف، مما يتيح له إنكار الاستلام أو الدفع بالتراخي في المطالبة.",
        "type": "Weakness"
      },
      {
        "id": "6ff6604f-c52c-422a-8ec5-83cfb44eff15",
        "description": "يحتاج الملف إلى استيفاء النقاط والمعلومات التالية لضمان صياغة صحيفة الدعوى بشكل مكتمل: حصر وتفصيص المبالغ المالية المطلوبة والتعويضات الدقيقة عن الضرر المادي والمعنوي، وإدراج التواريخ الرسمية الدقيقة لبدء النزاع وتاريخ الإخلال بالتعهدات.",
        "type": "MissingCaseInfo"
      },
      {
        "id": "80b19db0-bc92-4ef8-b3e8-da40253264d0",
        "description": "المستندات المحددة المطلوبة لإكمال الملف: أصل العقد/الاتفاق المبرم، صورة بطاقة الرقم القومي سارية لكل أطراف الدعوى، إيصالات التحويل أو السداد المالي، وأي إنذارات رسمية على يد محضر.",
        "type": "MissingCaseDoc"
      },
      {
        "id": "aacfedae-9c81-43ab-8a18-d49767a4d5e0",
        "description": "قم بإعادة هيكلة وصف القضية في صورة جدول زمني متسلسل، يبدأ من تاريخ التعهد الأول، مروراً بتاريخ الإخلال، وصولاً إلى حجم الأضرار المترتبة حالياً.",
        "type": "Suggestion"
      },
      {
        "id": "96b8c838-74f9-4c13-a3b8-0d41e1e27602",
        "description": "قم بتفقيط وقسمة كافة المطالبات المالية إلى بنود مستقلة (أصل الدين، الفوائد أو التعويض عن المماطلة، والرسوم) وتوثيق كل بند بسند كتابي مستقل.",
        "type": "Suggestion"
      },
      {
        "id": "247d88d7-7047-4102-8022-69f4584b8e81",
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


- [PASS] **settlement - review case** (status=200)
### settlement - finalize case

**Request:** POST http://localhost:5049/api/Case/5486d524-1510-4df4-86e3-fbb20bdb1158/finalize

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
    "caseId": "5486d524-1510-4df4-86e3-fbb20bdb1158",
    "totalEligibleLawyers": 0,
    "recommendations": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **settlement - finalize case** (status=200)
### settlement - create proposal

**Request:** POST http://localhost:5049/api/proposals

**Body:**
```json
{
  "LegalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158",
  "Message": "settlement proposal for contract notification lifecycle.",
  "LawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
    "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158",
    "caseTitle": "settlement case 172838914",
    "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
    "clientName": "Contracts Notifications client",
    "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
    "lawyerName": "Contracts Notifications lawyer",
    "message": "settlement proposal for contract notification lifecycle.",
    "status": "Pending",
    "decisionReason": null,
    "createdAt": "2026-08-09T14:28:41.1680347",
    "respondedAt": null,
    "updatedAt": "2026-08-09T14:28:41.1680347",
    "conversationId": null
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **settlement - create proposal** (status=200)
### settlement - accept proposal

**Request:** POST http://localhost:5049/api/proposals/fd7d8ea0-14f7-4606-bddc-0c2f40826276/accept

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
    "id": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
    "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158",
    "caseTitle": "settlement case 172838914",
    "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
    "clientName": "Contracts Notifications client",
    "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
    "lawyerName": "Contracts Notifications lawyer",
    "message": "settlement proposal for contract notification lifecycle.",
    "status": "Accepted",
    "decisionReason": null,
    "createdAt": "2026-08-09T14:28:41.1680347",
    "respondedAt": "2026-08-09T14:28:41.2274368",
    "updatedAt": "2026-08-09T14:28:41.2274368",
    "conversationId": "72b4ca33-d73e-4e0e-9eac-4a7b8a87d43c"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **settlement - accept proposal** (status=200)
### settlement - create contract

**Request:** POST http://localhost:5049/api/contracts

**Body:**
```json
{
  "ProposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
  "TermsAndConditions": "These complete contract terms are used for the settlement notification lifecycle and are accepted by both participants.",
  "Title": "settlement legal representation contract"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "24ecff4b-a397-4113-9766-2045b0f60a45",
    "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
    "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158",
    "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
    "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
    "title": "settlement legal representation contract",
    "termsAndConditions": "These complete contract terms are used for the settlement notification lifecycle and are accepted by both participants.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": null,
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 0,
    "version": "\"AAAAAAAACjQ=\"",
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


- [PASS] **settlement - create contract** (status=200)
- [PASS] **settlement create envelope retains logical 201**
### settlement - add milestone

**Request:** POST http://localhost:5049/api/contracts/24ecff4b-a397-4113-9766-2045b0f60a45/milestones

**Body:**
```json
{
  "Description": "Approved milestone used for the contract lifecycle.",
  "DurationDays": 10,
  "Amount": 1000.0,
  "Title": "settlement execution milestone",
  "OrderNumber": 1
}
```

**Response Status:** 201

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "13546473-178e-42a7-9b1f-ac3e83c56ab2",
    "orderNumber": 1,
    "title": "settlement execution milestone",
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
    "version": "\"AAAAAAAACjY=\"",
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


- [PASS] **settlement - add milestone** (status=201)
### settlement - list milestone for client ETag

**Request:** GET http://localhost:5049/api/contracts/24ecff4b-a397-4113-9766-2045b0f60a45/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "13546473-178e-42a7-9b1f-ac3e83c56ab2",
      "orderNumber": 1,
      "title": "settlement execution milestone",
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
      "version": "\"AAAAAAAACjY=\"",
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


- [PASS] **settlement - list milestone for client ETag** (status=200)
### settlement - client approves milestone

**Request:** POST http://localhost:5049/api/milestones/13546473-178e-42a7-9b1f-ac3e83c56ab2/approve

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
    "entityId": "13546473-178e-42a7-9b1f-ac3e83c56ab2",
    "status": "Draft",
    "occurredAt": "2026-08-09T14:28:41.5343383Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **settlement - client approves milestone** (status=200)
### settlement - list milestone for lawyer ETag

**Request:** GET http://localhost:5049/api/contracts/24ecff4b-a397-4113-9766-2045b0f60a45/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "13546473-178e-42a7-9b1f-ac3e83c56ab2",
      "orderNumber": 1,
      "title": "settlement execution milestone",
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
      "version": "\"AAAAAAAACjw=\"",
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


- [PASS] **settlement - list milestone for lawyer ETag** (status=200)
### settlement - lawyer approves milestone

**Request:** POST http://localhost:5049/api/milestones/13546473-178e-42a7-9b1f-ac3e83c56ab2/approve

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
    "entityId": "13546473-178e-42a7-9b1f-ac3e83c56ab2",
    "status": "AwaitingFunding",
    "occurredAt": "2026-08-09T14:28:41.5996291Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **settlement - lawyer approves milestone** (status=200)
### settlement - contract ETag for client acceptance

**Request:** GET http://localhost:5049/api/contracts/24ecff4b-a397-4113-9766-2045b0f60a45

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "24ecff4b-a397-4113-9766-2045b0f60a45",
    "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
    "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158",
    "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
    "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
    "title": "settlement legal representation contract",
    "termsAndConditions": "These complete contract terms are used for the settlement notification lifecycle and are accepted by both participants.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": null,
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 1000.0,
    "version": "\"AAAAAAAACjQ=\"",
    "milestones": [
      {
        "id": "13546473-178e-42a7-9b1f-ac3e83c56ab2",
        "orderNumber": 1,
        "title": "settlement execution milestone",
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
        "version": "\"AAAAAAAACkI=\""
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


- [PASS] **settlement - contract ETag for client acceptance** (status=200)
### settlement - client accepts contract

**Request:** POST http://localhost:5049/api/contracts/24ecff4b-a397-4113-9766-2045b0f60a45/accept

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
    "entityId": "24ecff4b-a397-4113-9766-2045b0f60a45",
    "status": "Draft",
    "occurredAt": "2026-08-09T14:28:41.7422528Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **settlement - client accepts contract** (status=200)
### settlement - contract ETag for lawyer acceptance

**Request:** GET http://localhost:5049/api/contracts/24ecff4b-a397-4113-9766-2045b0f60a45

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "24ecff4b-a397-4113-9766-2045b0f60a45",
    "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
    "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158",
    "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
    "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
    "title": "settlement legal representation contract",
    "termsAndConditions": "These complete contract terms are used for the settlement notification lifecycle and are accepted by both participants.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": "2026-08-09T14:28:41.7422528",
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 1000.0,
    "version": "\"AAAAAAAACkg=\"",
    "milestones": [
      {
        "id": "13546473-178e-42a7-9b1f-ac3e83c56ab2",
        "orderNumber": 1,
        "title": "settlement execution milestone",
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
        "version": "\"AAAAAAAACkI=\""
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


- [PASS] **settlement - contract ETag for lawyer acceptance** (status=200)
### settlement - lawyer accepts contract

**Request:** POST http://localhost:5049/api/contracts/24ecff4b-a397-4113-9766-2045b0f60a45/accept

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
    "entityId": "24ecff4b-a397-4113-9766-2045b0f60a45",
    "status": "Active",
    "occurredAt": "2026-08-09T14:28:41.7941115Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **settlement - lawyer accepts contract** (status=200)
### settlement - list milestone before funding

**Request:** GET http://localhost:5049/api/contracts/24ecff4b-a397-4113-9766-2045b0f60a45/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "13546473-178e-42a7-9b1f-ac3e83c56ab2",
      "orderNumber": 1,
      "title": "settlement execution milestone",
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
      "version": "\"AAAAAAAACkI=\"",
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


- [PASS] **settlement - list milestone before funding** (status=200)
### settlement - mark milestone ready for funding

**Request:** POST http://localhost:5049/api/milestones/13546473-178e-42a7-9b1f-ac3e83c56ab2/ready-for-funding

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
    "entityId": "13546473-178e-42a7-9b1f-ac3e83c56ab2",
    "status": "AwaitingFunding",
    "occurredAt": "2026-08-09T14:28:41.9237709Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **settlement - mark milestone ready for funding** (status=200)
### settlement - fund milestone through mock provider

**Request:** POST http://localhost:5049/api/milestones/13546473-178e-42a7-9b1f-ac3e83c56ab2/fund

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
    "id": "97f55090-7145-43c0-821f-ceab83629f5b",
    "milestoneId": "13546473-178e-42a7-9b1f-ac3e83c56ab2",
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


- [PASS] **settlement - fund milestone through mock provider** (status=200)
### Refresh funded contract

**Request:** GET http://localhost:5049/api/contracts/24ecff4b-a397-4113-9766-2045b0f60a45

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "24ecff4b-a397-4113-9766-2045b0f60a45",
    "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
    "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158",
    "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
    "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
    "title": "settlement legal representation contract",
    "termsAndConditions": "These complete contract terms are used for the settlement notification lifecycle and are accepted by both participants.",
    "currency": "EGP",
    "status": 1,
    "acceptedByClientAt": "2026-08-09T14:28:41.7422528",
    "acceptedByLawyerAt": "2026-08-09T14:28:41.7941115",
    "activatedAt": "2026-08-09T14:28:41.7941115",
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 1000.0,
    "version": "\"AAAAAAAACko=\"",
    "milestones": [
      {
        "id": "13546473-178e-42a7-9b1f-ac3e83c56ab2",
        "orderNumber": 1,
        "title": "settlement execution milestone",
        "description": "Approved milestone used for the contract lifecycle.",
        "amount": 1000.0,
        "durationDays": 10,
        "dueDate": null,
        "status": 3,
        "fundingStatus": 2,
        "escrowHoldId": "97f55090-7145-43c0-821f-ceab83629f5b",
        "fundedAt": "2026-08-09T14:28:42.0445242",
        "submittedAt": null,
        "autoAcceptEligibleAt": null,
        "holdExpiresAt": null,
        "netLawyerAmount": 950.0,
        "version": "\"AAAAAAAAClY=\""
      }
    ],
    "payments": [
      {
        "id": "97f55090-7145-43c0-821f-ceab83629f5b",
        "milestoneId": "13546473-178e-42a7-9b1f-ac3e83c56ab2",
        "grossAmount": 1000.0,
        "platformFee": 50.0,
        "netAmount": 950.0,
        "currency": "EGP",
        "status": 0,
        "holdExpiresAt": null,
        "settledAt": null
      }
    ],
    "permittedActions": [
      "Terminate"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Refresh funded contract** (status=200)
### Terminate funded contract with refund settlement

**Request:** POST http://localhost:5049/api/contracts/24ecff4b-a397-4113-9766-2045b0f60a45/terminate

**Body:**
```json
{
  "Reason": "إنهاء العقد مع رد مبلغ المرحلة الممولة."
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "24ecff4b-a397-4113-9766-2045b0f60a45",
    "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
    "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158",
    "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
    "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
    "title": "settlement legal representation contract",
    "termsAndConditions": "These complete contract terms are used for the settlement notification lifecycle and are accepted by both participants.",
    "currency": "EGP",
    "status": 4,
    "acceptedByClientAt": "2026-08-09T14:28:41.7422528",
    "acceptedByLawyerAt": "2026-08-09T14:28:41.7941115",
    "activatedAt": "2026-08-09T14:28:41.7941115",
    "completedAt": null,
    "terminatedAt": "2026-08-09T14:28:42.4466741Z",
    "currentMilestoneTotal": 1000.0,
    "version": "\"AAAAAAAACmY=\"",
    "milestones": [
      {
        "id": "13546473-178e-42a7-9b1f-ac3e83c56ab2",
        "orderNumber": 1,
        "title": "settlement execution milestone",
        "description": "Approved milestone used for the contract lifecycle.",
        "amount": 1000.0,
        "durationDays": 10,
        "dueDate": null,
        "status": 8,
        "fundingStatus": 3,
        "escrowHoldId": "97f55090-7145-43c0-821f-ceab83629f5b",
        "fundedAt": "2026-08-09T14:28:42.0445242",
        "submittedAt": null,
        "autoAcceptEligibleAt": null,
        "holdExpiresAt": null,
        "netLawyerAmount": 950.0,
        "version": "\"AAAAAAAACmM=\""
      }
    ],
    "payments": [
      {
        "id": "97f55090-7145-43c0-821f-ceab83629f5b",
        "milestoneId": "13546473-178e-42a7-9b1f-ac3e83c56ab2",
        "grossAmount": 1000.0,
        "platformFee": 50.0,
        "netAmount": 950.0,
        "currency": "EGP",
        "status": 3,
        "holdExpiresAt": null,
        "settledAt": "2026-08-09T14:28:42.3147079"
      }
    ],
    "permittedActions": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Terminate funded contract with refund settlement** (status=200)
### Poll client for settled termination

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "8a08cff9-bbdb-4a2f-b8c9-59215563ccb8",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.2909251",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "9eeaf2d6-c993-4a0f-a9ef-dee4be547f73",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/fd7d8ea0-14f7-4606-bddc-0c2f40826276",
        "data": {
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.2300824",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "2d93d6f7-52b9-4580-a568-e1e99b4809e8",
        "type": "contract.terminated",
        "severity": "Warning",
        "title": "تم إنهاء العقد",
        "body": "اكتملت إجراءات إنهاء العقد وتسويته.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:37.8408834",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f86e4b55-2aa6-4137-ae6d-32f662feed2f",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:35.9582728",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "17764033-8b8b-47ae-ade4-baaf04353645",
        "type": "contract.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث مسودة العقد",
        "body": "تم تحديث شروط العقد، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:33.8489103",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f70f04eb-fa76-494a-9abb-5f39d3e9a260",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.2904189",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "e4d1ebaa-3b82-4b07-b614-be8d060403dd",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/60694a52-77d0-46cc-ac37-03fc63084417",
        "data": {
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.1064461",
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


### Poll client for settled termination

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "145c76d1-fcbd-4721-be1f-de92fbf56b85",
        "type": "contract.terminated",
        "severity": "Warning",
        "title": "تم إنهاء العقد",
        "body": "اكتملت إجراءات إنهاء العقد وتسويته.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:42.4624789",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "da7d2b27-662d-4d57-9d22-75f9a5bc1376",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.8030866",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8a08cff9-bbdb-4a2f-b8c9-59215563ccb8",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.2909251",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "9eeaf2d6-c993-4a0f-a9ef-dee4be547f73",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/fd7d8ea0-14f7-4606-bddc-0c2f40826276",
        "data": {
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.2300824",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "2d93d6f7-52b9-4580-a568-e1e99b4809e8",
        "type": "contract.terminated",
        "severity": "Warning",
        "title": "تم إنهاء العقد",
        "body": "اكتملت إجراءات إنهاء العقد وتسويته.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:37.8408834",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f86e4b55-2aa6-4137-ae6d-32f662feed2f",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:35.9582728",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "17764033-8b8b-47ae-ade4-baaf04353645",
        "type": "contract.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث مسودة العقد",
        "body": "تم تحديث شروط العقد، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:33.8489103",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f70f04eb-fa76-494a-9abb-5f39d3e9a260",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.2904189",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "e4d1ebaa-3b82-4b07-b614-be8d060403dd",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/60694a52-77d0-46cc-ac37-03fc63084417",
        "data": {
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.1064461",
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


### Poll lawyer for settled termination

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "77d29f72-290a-4c5c-ae63-81f2cb248653",
        "type": "contract.terminated",
        "severity": "Warning",
        "title": "تم إنهاء العقد",
        "body": "اكتملت إجراءات إنهاء العقد وتسويته.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:42.4624789",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f9742c38-b49a-436f-92ed-b4f08f008a01",
        "type": "contract.termination-requested",
        "severity": "Warning",
        "title": "تم طلب إنهاء العقد",
        "body": "تم تسجيل طلب إنهاء العقد، وتجري معالجة التسوية اللازمة.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:42.2383664",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "ba01da06-40b2-4412-8567-e1ff48c5f520",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.8030866",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "efc26f74-ea71-4714-bae1-5f39016c7592",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.7444825",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "95b67eeb-24dd-4ede-b361-95b643c1ff7a",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/fd7d8ea0-14f7-4606-bddc-0c2f40826276",
        "data": {
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.16817",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "e71df79c-be86-498f-8f30-49096394f841",
        "type": "contract.terminated",
        "severity": "Warning",
        "title": "تم إنهاء العقد",
        "body": "اكتملت إجراءات إنهاء العقد وتسويته.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:37.8408834",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "3e7d8253-eb47-4140-ba3f-d743f3a8b5ba",
        "type": "contract.termination-requested",
        "severity": "Warning",
        "title": "تم طلب إنهاء العقد",
        "body": "تم تسجيل طلب إنهاء العقد، وتجري معالجة التسوية اللازمة.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:37.7823417",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "d7ce2978-4f92-4234-9890-a67b7aa9d256",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:35.9582728",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4e615ddd-c9cb-413b-9b49-88fbed1840c1",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:35.7739812",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "d69c72f5-0168-43ff-8af8-02f3235a6686",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:32.8758527",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "19ac3cb1-78ea-4d1c-aee7-0eecc9dc0c99",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/60694a52-77d0-46cc-ac37-03fc63084417",
        "data": {
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.0568395",
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


- [PASS] **Funded termination notifies client**
- [PASS] **Funded termination notifies lawyer**

## Contract completion lifecycle through mock funding

### completion - create case

**Request:** POST http://localhost:5049/api/Case

**Body:**
```json
{
  "Description": "Complete case foundation for completion contract notifications.",
  "City": "Maadi",
  "Governorate": "Cairo",
  "Title": "completion case 172843352"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "caseId": "c11d707b-29b8-4623-a29b-36f481ba1723",
    "failedDocuments": []
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **completion - create case** (status=200)
### completion - review case

**Request:** POST http://localhost:5049/api/cases/c11d707b-29b8-4623-a29b-36f481ba1723/review

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
    "id": "a68a0f28-897b-404e-85b7-e6a1dad2e0eb",
    "caseId": "c11d707b-29b8-4623-a29b-36f481ba1723",
    "isLatest": true,
    "createdAt": "2026-08-09T14:28:44.0355957Z",
    "reviewPoints": [
      {
        "id": "daaeecc0-0338-4ba5-94b0-a542c27b1bce",
        "description": "تتمثل نقطة القوة الأساسية في صياغة الموضوع بوضوح حول 'completion case 172843352'، وتوافر السند المبدئي الذي يرجح كفة الموكل في إثبات أصل الالتزام وتفوقه إثباتياً على الخصم.",
        "type": "Strength"
      },
      {
        "id": "2bd8ae1c-586d-433f-a334-36d64f1b32aa",
        "description": "ميزة الخصم تتمثل في غياب التوثيق الرسمي للتنبيهات أو الإخطارات المتبادلة بين الأطراف، مما يتيح له إنكار الاستلام أو الدفع بالتراخي في المطالبة.",
        "type": "Weakness"
      },
      {
        "id": "79d1bdc3-a310-4d49-bdb8-bab399fca138",
        "description": "يحتاج الملف إلى استيفاء النقاط والمعلومات التالية لضمان صياغة صحيفة الدعوى بشكل مكتمل: حصر وتفصيص المبالغ المالية المطلوبة والتعويضات الدقيقة عن الضرر المادي والمعنوي، وإدراج التواريخ الرسمية الدقيقة لبدء النزاع وتاريخ الإخلال بالتعهدات.",
        "type": "MissingCaseInfo"
      },
      {
        "id": "68947bbf-c349-4ecf-9e71-80f2d420cd85",
        "description": "المستندات المحددة المطلوبة لإكمال الملف: أصل العقد/الاتفاق المبرم، صورة بطاقة الرقم القومي سارية لكل أطراف الدعوى، إيصالات التحويل أو السداد المالي، وأي إنذارات رسمية على يد محضر.",
        "type": "MissingCaseDoc"
      },
      {
        "id": "6cdc7fd4-f6d0-492e-aeb4-32adb3613b35",
        "description": "قم بإعادة هيكلة وصف القضية في صورة جدول زمني متسلسل، يبدأ من تاريخ التعهد الأول، مروراً بتاريخ الإخلال، وصولاً إلى حجم الأضرار المترتبة حالياً.",
        "type": "Suggestion"
      },
      {
        "id": "c7f23bf0-b4e1-4fb5-8e86-7d1ddd81870a",
        "description": "قم بتفقيط وقسمة كافة المطالبات المالية إلى بنود مستقلة (أصل الدين، الفوائد أو التعويض عن المماطلة، والرسوم) وتوثيق كل بند بسند كتابي مستقل.",
        "type": "Suggestion"
      },
      {
        "id": "1fa5476a-348e-4287-ae25-1e265251d403",
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


- [PASS] **completion - review case** (status=200)
### completion - finalize case

**Request:** POST http://localhost:5049/api/Case/c11d707b-29b8-4623-a29b-36f481ba1723/finalize

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
    "caseId": "c11d707b-29b8-4623-a29b-36f481ba1723",
    "totalEligibleLawyers": 0,
    "recommendations": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **completion - finalize case** (status=200)
### completion - create proposal

**Request:** POST http://localhost:5049/api/proposals

**Body:**
```json
{
  "LegalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723",
  "Message": "completion proposal for contract notification lifecycle.",
  "LawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "64d317ed-fa12-476e-92dd-e7b79117b5af",
    "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723",
    "caseTitle": "completion case 172843352",
    "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
    "clientName": "Contracts Notifications client",
    "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
    "lawyerName": "Contracts Notifications lawyer",
    "message": "completion proposal for contract notification lifecycle.",
    "status": "Pending",
    "decisionReason": null,
    "createdAt": "2026-08-09T14:28:44.6350626",
    "respondedAt": null,
    "updatedAt": "2026-08-09T14:28:44.6350626",
    "conversationId": null
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **completion - create proposal** (status=200)
### completion - accept proposal

**Request:** POST http://localhost:5049/api/proposals/64d317ed-fa12-476e-92dd-e7b79117b5af/accept

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
    "id": "64d317ed-fa12-476e-92dd-e7b79117b5af",
    "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723",
    "caseTitle": "completion case 172843352",
    "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
    "clientName": "Contracts Notifications client",
    "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
    "lawyerName": "Contracts Notifications lawyer",
    "message": "completion proposal for contract notification lifecycle.",
    "status": "Accepted",
    "decisionReason": null,
    "createdAt": "2026-08-09T14:28:44.6350626",
    "respondedAt": "2026-08-09T14:28:44.6879729",
    "updatedAt": "2026-08-09T14:28:44.6879729",
    "conversationId": "27d57dad-6693-4a41-b468-74fefbd44053"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **completion - accept proposal** (status=200)
### completion - create contract

**Request:** POST http://localhost:5049/api/contracts

**Body:**
```json
{
  "ProposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
  "TermsAndConditions": "These complete contract terms are used for the completion notification lifecycle and are accepted by both participants.",
  "Title": "completion legal representation contract"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "9a62defb-5eba-4fe9-b216-3a8fe58d3d4f",
    "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
    "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723",
    "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
    "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
    "title": "completion legal representation contract",
    "termsAndConditions": "These complete contract terms are used for the completion notification lifecycle and are accepted by both participants.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": null,
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 0,
    "version": "\"AAAAAAAACoY=\"",
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


- [PASS] **completion - create contract** (status=200)
- [PASS] **completion create envelope retains logical 201**
### completion - add milestone

**Request:** POST http://localhost:5049/api/contracts/9a62defb-5eba-4fe9-b216-3a8fe58d3d4f/milestones

**Body:**
```json
{
  "Description": "Approved milestone used for the contract lifecycle.",
  "DurationDays": 10,
  "Amount": 1000.0,
  "Title": "completion execution milestone",
  "OrderNumber": 1
}
```

**Response Status:** 201

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "ac157709-6505-43bf-b83f-ac3dd23b3d9b",
    "orderNumber": 1,
    "title": "completion execution milestone",
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
    "version": "\"AAAAAAAACog=\"",
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


- [PASS] **completion - add milestone** (status=201)
### completion - list milestone for client ETag

**Request:** GET http://localhost:5049/api/contracts/9a62defb-5eba-4fe9-b216-3a8fe58d3d4f/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "ac157709-6505-43bf-b83f-ac3dd23b3d9b",
      "orderNumber": 1,
      "title": "completion execution milestone",
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
      "version": "\"AAAAAAAACog=\"",
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


- [PASS] **completion - list milestone for client ETag** (status=200)
### completion - client approves milestone

**Request:** POST http://localhost:5049/api/milestones/ac157709-6505-43bf-b83f-ac3dd23b3d9b/approve

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
    "entityId": "ac157709-6505-43bf-b83f-ac3dd23b3d9b",
    "status": "Draft",
    "occurredAt": "2026-08-09T14:28:44.9938332Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **completion - client approves milestone** (status=200)
### completion - list milestone for lawyer ETag

**Request:** GET http://localhost:5049/api/contracts/9a62defb-5eba-4fe9-b216-3a8fe58d3d4f/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "ac157709-6505-43bf-b83f-ac3dd23b3d9b",
      "orderNumber": 1,
      "title": "completion execution milestone",
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
      "version": "\"AAAAAAAACok=\"",
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


- [PASS] **completion - list milestone for lawyer ETag** (status=200)
### completion - lawyer approves milestone

**Request:** POST http://localhost:5049/api/milestones/ac157709-6505-43bf-b83f-ac3dd23b3d9b/approve

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
    "entityId": "ac157709-6505-43bf-b83f-ac3dd23b3d9b",
    "status": "AwaitingFunding",
    "occurredAt": "2026-08-09T14:28:45.0597258Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **completion - lawyer approves milestone** (status=200)
### completion - contract ETag for client acceptance

**Request:** GET http://localhost:5049/api/contracts/9a62defb-5eba-4fe9-b216-3a8fe58d3d4f

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "9a62defb-5eba-4fe9-b216-3a8fe58d3d4f",
    "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
    "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723",
    "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
    "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
    "title": "completion legal representation contract",
    "termsAndConditions": "These complete contract terms are used for the completion notification lifecycle and are accepted by both participants.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": null,
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 1000.0,
    "version": "\"AAAAAAAACoY=\"",
    "milestones": [
      {
        "id": "ac157709-6505-43bf-b83f-ac3dd23b3d9b",
        "orderNumber": 1,
        "title": "completion execution milestone",
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
        "version": "\"AAAAAAAACoo=\""
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


- [PASS] **completion - contract ETag for client acceptance** (status=200)
### completion - client accepts contract

**Request:** POST http://localhost:5049/api/contracts/9a62defb-5eba-4fe9-b216-3a8fe58d3d4f/accept

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
    "entityId": "9a62defb-5eba-4fe9-b216-3a8fe58d3d4f",
    "status": "Draft",
    "occurredAt": "2026-08-09T14:28:45.0935877Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **completion - client accepts contract** (status=200)
### completion - contract ETag for lawyer acceptance

**Request:** GET http://localhost:5049/api/contracts/9a62defb-5eba-4fe9-b216-3a8fe58d3d4f

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "9a62defb-5eba-4fe9-b216-3a8fe58d3d4f",
    "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
    "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723",
    "clientUserId": "0d0244fc-4e43-45be-9c64-08def62157b7",
    "lawyerUserId": "834d9fb0-5727-4d61-9c65-08def62157b7",
    "title": "completion legal representation contract",
    "termsAndConditions": "These complete contract terms are used for the completion notification lifecycle and are accepted by both participants.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": "2026-08-09T14:28:45.0935877",
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 1000.0,
    "version": "\"AAAAAAAACow=\"",
    "milestones": [
      {
        "id": "ac157709-6505-43bf-b83f-ac3dd23b3d9b",
        "orderNumber": 1,
        "title": "completion execution milestone",
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
        "version": "\"AAAAAAAACoo=\""
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


- [PASS] **completion - contract ETag for lawyer acceptance** (status=200)
### completion - lawyer accepts contract

**Request:** POST http://localhost:5049/api/contracts/9a62defb-5eba-4fe9-b216-3a8fe58d3d4f/accept

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
    "entityId": "9a62defb-5eba-4fe9-b216-3a8fe58d3d4f",
    "status": "Active",
    "occurredAt": "2026-08-09T14:28:45.128722Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **completion - lawyer accepts contract** (status=200)
### completion - list milestone before funding

**Request:** GET http://localhost:5049/api/contracts/9a62defb-5eba-4fe9-b216-3a8fe58d3d4f/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "ac157709-6505-43bf-b83f-ac3dd23b3d9b",
      "orderNumber": 1,
      "title": "completion execution milestone",
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
      "version": "\"AAAAAAAACoo=\"",
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


- [PASS] **completion - list milestone before funding** (status=200)
### completion - mark milestone ready for funding

**Request:** POST http://localhost:5049/api/milestones/ac157709-6505-43bf-b83f-ac3dd23b3d9b/ready-for-funding

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
    "entityId": "ac157709-6505-43bf-b83f-ac3dd23b3d9b",
    "status": "AwaitingFunding",
    "occurredAt": "2026-08-09T14:28:45.2515572Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **completion - mark milestone ready for funding** (status=200)
### completion - fund milestone through mock provider

**Request:** POST http://localhost:5049/api/milestones/ac157709-6505-43bf-b83f-ac3dd23b3d9b/fund

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
    "id": "9f5fb2b4-1c30-4613-a3eb-043ebe3f5617",
    "milestoneId": "ac157709-6505-43bf-b83f-ac3dd23b3d9b",
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


- [PASS] **completion - fund milestone through mock provider** (status=200)
- [PASS] **Lawyer-owned stored-file fixture created for HTTP submission**
### Lawyer submits funded milestone

**Request:** POST http://localhost:5049/api/milestones/ac157709-6505-43bf-b83f-ac3dd23b3d9b/submit

**Body:**
```json
{
  "StoredFileIds": [
    "f92d70c9-c43e-4205-ac1e-bee4e713ba76"
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
    "id": "ac157709-6505-43bf-b83f-ac3dd23b3d9b",
    "orderNumber": 1,
    "title": "completion execution milestone",
    "description": "Approved milestone used for the contract lifecycle.",
    "amount": 1000.0,
    "durationDays": 10,
    "dueDate": null,
    "status": 4,
    "fundingStatus": 2,
    "escrowHoldId": "9f5fb2b4-1c30-4613-a3eb-043ebe3f5617",
    "fundedAt": "2026-08-09T14:28:45.3412879",
    "submittedAt": "2026-08-09T14:28:45.7830259Z",
    "autoAcceptEligibleAt": "2026-08-16T14:28:45.7830259Z",
    "holdExpiresAt": null,
    "netLawyerAmount": 950.0,
    "version": "\"AAAAAAAACrg=\"",
    "permittedActions": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Lawyer submits funded milestone** (status=200)
### Client accepts delivered milestone

**Request:** POST http://localhost:5049/api/milestones/ac157709-6505-43bf-b83f-ac3dd23b3d9b/accept

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
    "id": "ac157709-6505-43bf-b83f-ac3dd23b3d9b",
    "orderNumber": 1,
    "title": "completion execution milestone",
    "description": "Approved milestone used for the contract lifecycle.",
    "amount": 1000.0,
    "durationDays": 10,
    "dueDate": null,
    "status": 5,
    "fundingStatus": 2,
    "escrowHoldId": "9f5fb2b4-1c30-4613-a3eb-043ebe3f5617",
    "fundedAt": "2026-08-09T14:28:45.3412879",
    "submittedAt": "2026-08-09T14:28:45.7830259",
    "autoAcceptEligibleAt": null,
    "holdExpiresAt": "2026-08-23T14:28:45.9721801Z",
    "netLawyerAmount": 950.0,
    "version": "\"AAAAAAAACr8=\"",
    "permittedActions": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client accepts delivered milestone** (status=200)
- [PASS] **Disposable SuperAdministrator fixture created for escrow release**
### Refresh disposable SuperAdministrator token

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
      "id": "079478ba-5e1b-4d2e-9c66-08def62157b7",
      "email": "[REDACTED]",
      "fullName": "Contracts Notifications attacker",
      "role": "Client",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "[REDACTED]",
    "expiresIn": 900,
    "refreshToken": "[REDACTED]",
    "refreshTokenExpiration": "2026-08-16T14:28:46.3883521Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Refresh disposable SuperAdministrator token** (status=200)
### Admin releases accepted escrow hold

**Request:** POST http://localhost:5049/api/admin/milestones/ac157709-6505-43bf-b83f-ac3dd23b3d9b/release

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
    "entityId": "ac157709-6505-43bf-b83f-ac3dd23b3d9b",
    "status": "Released",
    "occurredAt": "2026-08-09T14:28:46.9569501Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Admin releases accepted escrow hold** (status=200)
### Poll client for completion

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "ff10d098-9996-45a8-bebb-1639cfe9e0c3",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "9a62defb-5eba-4fe9-b216-3a8fe58d3d4f",
          "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
          "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723"
        },
        "createdAtUtc": "2026-08-09T14:28:45.1412093",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "a923acb0-7d96-447e-94d6-fac5508250f9",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "9a62defb-5eba-4fe9-b216-3a8fe58d3d4f",
          "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
          "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723"
        },
        "createdAtUtc": "2026-08-09T14:28:44.8333785",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "31cf0c09-a70c-4360-8b02-e4624a5c0b2e",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/64d317ed-fa12-476e-92dd-e7b79117b5af",
        "data": {
          "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
          "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723"
        },
        "createdAtUtc": "2026-08-09T14:28:44.6918011",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "145c76d1-fcbd-4721-be1f-de92fbf56b85",
        "type": "contract.terminated",
        "severity": "Warning",
        "title": "تم إنهاء العقد",
        "body": "اكتملت إجراءات إنهاء العقد وتسويته.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:42.4624789",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "da7d2b27-662d-4d57-9d22-75f9a5bc1376",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.8030866",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8a08cff9-bbdb-4a2f-b8c9-59215563ccb8",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.2909251",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "9eeaf2d6-c993-4a0f-a9ef-dee4be547f73",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/fd7d8ea0-14f7-4606-bddc-0c2f40826276",
        "data": {
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.2300824",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "2d93d6f7-52b9-4580-a568-e1e99b4809e8",
        "type": "contract.terminated",
        "severity": "Warning",
        "title": "تم إنهاء العقد",
        "body": "اكتملت إجراءات إنهاء العقد وتسويته.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:37.8408834",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f86e4b55-2aa6-4137-ae6d-32f662feed2f",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:35.9582728",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "17764033-8b8b-47ae-ade4-baaf04353645",
        "type": "contract.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث مسودة العقد",
        "body": "تم تحديث شروط العقد، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:33.8489103",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f70f04eb-fa76-494a-9abb-5f39d3e9a260",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.2904189",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "e4d1ebaa-3b82-4b07-b614-be8d060403dd",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/60694a52-77d0-46cc-ac37-03fc63084417",
        "data": {
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.1064461",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 12
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll client for completion

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "ff10d098-9996-45a8-bebb-1639cfe9e0c3",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "9a62defb-5eba-4fe9-b216-3a8fe58d3d4f",
          "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
          "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723"
        },
        "createdAtUtc": "2026-08-09T14:28:45.1412093",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "a923acb0-7d96-447e-94d6-fac5508250f9",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "9a62defb-5eba-4fe9-b216-3a8fe58d3d4f",
          "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
          "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723"
        },
        "createdAtUtc": "2026-08-09T14:28:44.8333785",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "31cf0c09-a70c-4360-8b02-e4624a5c0b2e",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/64d317ed-fa12-476e-92dd-e7b79117b5af",
        "data": {
          "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
          "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723"
        },
        "createdAtUtc": "2026-08-09T14:28:44.6918011",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "145c76d1-fcbd-4721-be1f-de92fbf56b85",
        "type": "contract.terminated",
        "severity": "Warning",
        "title": "تم إنهاء العقد",
        "body": "اكتملت إجراءات إنهاء العقد وتسويته.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:42.4624789",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "da7d2b27-662d-4d57-9d22-75f9a5bc1376",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.8030866",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8a08cff9-bbdb-4a2f-b8c9-59215563ccb8",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.2909251",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "9eeaf2d6-c993-4a0f-a9ef-dee4be547f73",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/fd7d8ea0-14f7-4606-bddc-0c2f40826276",
        "data": {
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.2300824",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "2d93d6f7-52b9-4580-a568-e1e99b4809e8",
        "type": "contract.terminated",
        "severity": "Warning",
        "title": "تم إنهاء العقد",
        "body": "اكتملت إجراءات إنهاء العقد وتسويته.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:37.8408834",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f86e4b55-2aa6-4137-ae6d-32f662feed2f",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:35.9582728",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "17764033-8b8b-47ae-ade4-baaf04353645",
        "type": "contract.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث مسودة العقد",
        "body": "تم تحديث شروط العقد، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:33.8489103",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f70f04eb-fa76-494a-9abb-5f39d3e9a260",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.2904189",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "e4d1ebaa-3b82-4b07-b614-be8d060403dd",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/60694a52-77d0-46cc-ac37-03fc63084417",
        "data": {
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.1064461",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 12
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll client for completion

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "ff10d098-9996-45a8-bebb-1639cfe9e0c3",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "9a62defb-5eba-4fe9-b216-3a8fe58d3d4f",
          "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
          "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723"
        },
        "createdAtUtc": "2026-08-09T14:28:45.1412093",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "a923acb0-7d96-447e-94d6-fac5508250f9",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "9a62defb-5eba-4fe9-b216-3a8fe58d3d4f",
          "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
          "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723"
        },
        "createdAtUtc": "2026-08-09T14:28:44.8333785",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "31cf0c09-a70c-4360-8b02-e4624a5c0b2e",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/64d317ed-fa12-476e-92dd-e7b79117b5af",
        "data": {
          "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
          "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723"
        },
        "createdAtUtc": "2026-08-09T14:28:44.6918011",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "145c76d1-fcbd-4721-be1f-de92fbf56b85",
        "type": "contract.terminated",
        "severity": "Warning",
        "title": "تم إنهاء العقد",
        "body": "اكتملت إجراءات إنهاء العقد وتسويته.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:42.4624789",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "da7d2b27-662d-4d57-9d22-75f9a5bc1376",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.8030866",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8a08cff9-bbdb-4a2f-b8c9-59215563ccb8",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.2909251",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "9eeaf2d6-c993-4a0f-a9ef-dee4be547f73",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/fd7d8ea0-14f7-4606-bddc-0c2f40826276",
        "data": {
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.2300824",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "2d93d6f7-52b9-4580-a568-e1e99b4809e8",
        "type": "contract.terminated",
        "severity": "Warning",
        "title": "تم إنهاء العقد",
        "body": "اكتملت إجراءات إنهاء العقد وتسويته.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:37.8408834",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f86e4b55-2aa6-4137-ae6d-32f662feed2f",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:35.9582728",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "17764033-8b8b-47ae-ade4-baaf04353645",
        "type": "contract.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث مسودة العقد",
        "body": "تم تحديث شروط العقد، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:33.8489103",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f70f04eb-fa76-494a-9abb-5f39d3e9a260",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.2904189",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "e4d1ebaa-3b82-4b07-b614-be8d060403dd",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/60694a52-77d0-46cc-ac37-03fc63084417",
        "data": {
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.1064461",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 12
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll client for completion

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "ff10d098-9996-45a8-bebb-1639cfe9e0c3",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "9a62defb-5eba-4fe9-b216-3a8fe58d3d4f",
          "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
          "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723"
        },
        "createdAtUtc": "2026-08-09T14:28:45.1412093",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "a923acb0-7d96-447e-94d6-fac5508250f9",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "9a62defb-5eba-4fe9-b216-3a8fe58d3d4f",
          "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
          "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723"
        },
        "createdAtUtc": "2026-08-09T14:28:44.8333785",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "31cf0c09-a70c-4360-8b02-e4624a5c0b2e",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/64d317ed-fa12-476e-92dd-e7b79117b5af",
        "data": {
          "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
          "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723"
        },
        "createdAtUtc": "2026-08-09T14:28:44.6918011",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "145c76d1-fcbd-4721-be1f-de92fbf56b85",
        "type": "contract.terminated",
        "severity": "Warning",
        "title": "تم إنهاء العقد",
        "body": "اكتملت إجراءات إنهاء العقد وتسويته.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:42.4624789",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "da7d2b27-662d-4d57-9d22-75f9a5bc1376",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.8030866",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8a08cff9-bbdb-4a2f-b8c9-59215563ccb8",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.2909251",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "9eeaf2d6-c993-4a0f-a9ef-dee4be547f73",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/fd7d8ea0-14f7-4606-bddc-0c2f40826276",
        "data": {
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.2300824",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "2d93d6f7-52b9-4580-a568-e1e99b4809e8",
        "type": "contract.terminated",
        "severity": "Warning",
        "title": "تم إنهاء العقد",
        "body": "اكتملت إجراءات إنهاء العقد وتسويته.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:37.8408834",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f86e4b55-2aa6-4137-ae6d-32f662feed2f",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:35.9582728",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "17764033-8b8b-47ae-ade4-baaf04353645",
        "type": "contract.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث مسودة العقد",
        "body": "تم تحديث شروط العقد، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:33.8489103",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f70f04eb-fa76-494a-9abb-5f39d3e9a260",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.2904189",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "e4d1ebaa-3b82-4b07-b614-be8d060403dd",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/60694a52-77d0-46cc-ac37-03fc63084417",
        "data": {
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.1064461",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 12
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll client for completion

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "ff10d098-9996-45a8-bebb-1639cfe9e0c3",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "9a62defb-5eba-4fe9-b216-3a8fe58d3d4f",
          "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
          "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723"
        },
        "createdAtUtc": "2026-08-09T14:28:45.1412093",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "a923acb0-7d96-447e-94d6-fac5508250f9",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "9a62defb-5eba-4fe9-b216-3a8fe58d3d4f",
          "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
          "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723"
        },
        "createdAtUtc": "2026-08-09T14:28:44.8333785",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "31cf0c09-a70c-4360-8b02-e4624a5c0b2e",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/64d317ed-fa12-476e-92dd-e7b79117b5af",
        "data": {
          "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
          "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723"
        },
        "createdAtUtc": "2026-08-09T14:28:44.6918011",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "145c76d1-fcbd-4721-be1f-de92fbf56b85",
        "type": "contract.terminated",
        "severity": "Warning",
        "title": "تم إنهاء العقد",
        "body": "اكتملت إجراءات إنهاء العقد وتسويته.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:42.4624789",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "da7d2b27-662d-4d57-9d22-75f9a5bc1376",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.8030866",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8a08cff9-bbdb-4a2f-b8c9-59215563ccb8",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.2909251",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "9eeaf2d6-c993-4a0f-a9ef-dee4be547f73",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/fd7d8ea0-14f7-4606-bddc-0c2f40826276",
        "data": {
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.2300824",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "2d93d6f7-52b9-4580-a568-e1e99b4809e8",
        "type": "contract.terminated",
        "severity": "Warning",
        "title": "تم إنهاء العقد",
        "body": "اكتملت إجراءات إنهاء العقد وتسويته.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:37.8408834",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f86e4b55-2aa6-4137-ae6d-32f662feed2f",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:35.9582728",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "17764033-8b8b-47ae-ade4-baaf04353645",
        "type": "contract.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث مسودة العقد",
        "body": "تم تحديث شروط العقد، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:33.8489103",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f70f04eb-fa76-494a-9abb-5f39d3e9a260",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.2904189",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "e4d1ebaa-3b82-4b07-b614-be8d060403dd",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/60694a52-77d0-46cc-ac37-03fc63084417",
        "data": {
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.1064461",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 12
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll client for completion

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "ff10d098-9996-45a8-bebb-1639cfe9e0c3",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "9a62defb-5eba-4fe9-b216-3a8fe58d3d4f",
          "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
          "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723"
        },
        "createdAtUtc": "2026-08-09T14:28:45.1412093",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "a923acb0-7d96-447e-94d6-fac5508250f9",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "9a62defb-5eba-4fe9-b216-3a8fe58d3d4f",
          "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
          "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723"
        },
        "createdAtUtc": "2026-08-09T14:28:44.8333785",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "31cf0c09-a70c-4360-8b02-e4624a5c0b2e",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/64d317ed-fa12-476e-92dd-e7b79117b5af",
        "data": {
          "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
          "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723"
        },
        "createdAtUtc": "2026-08-09T14:28:44.6918011",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "145c76d1-fcbd-4721-be1f-de92fbf56b85",
        "type": "contract.terminated",
        "severity": "Warning",
        "title": "تم إنهاء العقد",
        "body": "اكتملت إجراءات إنهاء العقد وتسويته.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:42.4624789",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "da7d2b27-662d-4d57-9d22-75f9a5bc1376",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.8030866",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8a08cff9-bbdb-4a2f-b8c9-59215563ccb8",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.2909251",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "9eeaf2d6-c993-4a0f-a9ef-dee4be547f73",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/fd7d8ea0-14f7-4606-bddc-0c2f40826276",
        "data": {
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.2300824",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "2d93d6f7-52b9-4580-a568-e1e99b4809e8",
        "type": "contract.terminated",
        "severity": "Warning",
        "title": "تم إنهاء العقد",
        "body": "اكتملت إجراءات إنهاء العقد وتسويته.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:37.8408834",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f86e4b55-2aa6-4137-ae6d-32f662feed2f",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:35.9582728",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "17764033-8b8b-47ae-ade4-baaf04353645",
        "type": "contract.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث مسودة العقد",
        "body": "تم تحديث شروط العقد، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:33.8489103",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f70f04eb-fa76-494a-9abb-5f39d3e9a260",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.2904189",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "e4d1ebaa-3b82-4b07-b614-be8d060403dd",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/60694a52-77d0-46cc-ac37-03fc63084417",
        "data": {
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.1064461",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 12
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll client for completion

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "60b78f27-5fdf-4eb5-b554-80793d13fe86",
        "type": "contract.completed",
        "severity": "Success",
        "title": "اكتمل العقد",
        "body": "اكتملت جميع مراحل العقد وتسوياته بنجاح.",
        "actionUrl": null,
        "data": {
          "contractId": "9a62defb-5eba-4fe9-b216-3a8fe58d3d4f",
          "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
          "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723"
        },
        "createdAtUtc": "2026-08-09T14:28:46.9179358",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "ff10d098-9996-45a8-bebb-1639cfe9e0c3",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "9a62defb-5eba-4fe9-b216-3a8fe58d3d4f",
          "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
          "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723"
        },
        "createdAtUtc": "2026-08-09T14:28:45.1412093",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "a923acb0-7d96-447e-94d6-fac5508250f9",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "9a62defb-5eba-4fe9-b216-3a8fe58d3d4f",
          "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
          "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723"
        },
        "createdAtUtc": "2026-08-09T14:28:44.8333785",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "31cf0c09-a70c-4360-8b02-e4624a5c0b2e",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/64d317ed-fa12-476e-92dd-e7b79117b5af",
        "data": {
          "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
          "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723"
        },
        "createdAtUtc": "2026-08-09T14:28:44.6918011",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "145c76d1-fcbd-4721-be1f-de92fbf56b85",
        "type": "contract.terminated",
        "severity": "Warning",
        "title": "تم إنهاء العقد",
        "body": "اكتملت إجراءات إنهاء العقد وتسويته.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:42.4624789",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "da7d2b27-662d-4d57-9d22-75f9a5bc1376",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.8030866",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "8a08cff9-bbdb-4a2f-b8c9-59215563ccb8",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.2909251",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "9eeaf2d6-c993-4a0f-a9ef-dee4be547f73",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/fd7d8ea0-14f7-4606-bddc-0c2f40826276",
        "data": {
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.2300824",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "2d93d6f7-52b9-4580-a568-e1e99b4809e8",
        "type": "contract.terminated",
        "severity": "Warning",
        "title": "تم إنهاء العقد",
        "body": "اكتملت إجراءات إنهاء العقد وتسويته.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:37.8408834",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f86e4b55-2aa6-4137-ae6d-32f662feed2f",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:35.9582728",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "17764033-8b8b-47ae-ade4-baaf04353645",
        "type": "contract.draft-updated",
        "severity": "Warning",
        "title": "تم تحديث مسودة العقد",
        "body": "تم تحديث شروط العقد، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:33.8489103",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f70f04eb-fa76-494a-9abb-5f39d3e9a260",
        "type": "contract.created",
        "severity": "Information",
        "title": "مسودة عقد جديدة",
        "body": "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.2904189",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "e4d1ebaa-3b82-4b07-b614-be8d060403dd",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/60694a52-77d0-46cc-ac37-03fc63084417",
        "data": {
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.1064461",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 13
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


### Poll lawyer for completion

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "e6211c7b-f68b-4a22-bde5-ab1728c892be",
        "type": "contract.completed",
        "severity": "Success",
        "title": "اكتمل العقد",
        "body": "اكتملت جميع مراحل العقد وتسوياته بنجاح.",
        "actionUrl": null,
        "data": {
          "contractId": "9a62defb-5eba-4fe9-b216-3a8fe58d3d4f",
          "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
          "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723"
        },
        "createdAtUtc": "2026-08-09T14:28:46.9179358",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "1d9e6845-0fdc-4f9a-b8f3-f885f5680b6b",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "9a62defb-5eba-4fe9-b216-3a8fe58d3d4f",
          "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
          "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723"
        },
        "createdAtUtc": "2026-08-09T14:28:45.1412093",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "63ca9a33-b4f3-4c11-b7cc-e1db21867884",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "9a62defb-5eba-4fe9-b216-3a8fe58d3d4f",
          "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
          "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723"
        },
        "createdAtUtc": "2026-08-09T14:28:45.0947086",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "1c0fd1b4-539c-4cc9-bcb4-044287b10aa4",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/64d317ed-fa12-476e-92dd-e7b79117b5af",
        "data": {
          "proposalId": "64d317ed-fa12-476e-92dd-e7b79117b5af",
          "legalCaseId": "c11d707b-29b8-4623-a29b-36f481ba1723"
        },
        "createdAtUtc": "2026-08-09T14:28:44.6352028",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "77d29f72-290a-4c5c-ae63-81f2cb248653",
        "type": "contract.terminated",
        "severity": "Warning",
        "title": "تم إنهاء العقد",
        "body": "اكتملت إجراءات إنهاء العقد وتسويته.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:42.4624789",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "f9742c38-b49a-436f-92ed-b4f08f008a01",
        "type": "contract.termination-requested",
        "severity": "Warning",
        "title": "تم طلب إنهاء العقد",
        "body": "تم تسجيل طلب إنهاء العقد، وتجري معالجة التسوية اللازمة.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:42.2383664",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "ba01da06-40b2-4412-8567-e1ff48c5f520",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.8030866",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "efc26f74-ea71-4714-bae1-5f39016c7592",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "24ecff4b-a397-4113-9766-2045b0f60a45",
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.7444825",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "95b67eeb-24dd-4ede-b361-95b643c1ff7a",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/fd7d8ea0-14f7-4606-bddc-0c2f40826276",
        "data": {
          "proposalId": "fd7d8ea0-14f7-4606-bddc-0c2f40826276",
          "legalCaseId": "5486d524-1510-4df4-86e3-fbb20bdb1158"
        },
        "createdAtUtc": "2026-08-09T14:28:41.16817",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "e71df79c-be86-498f-8f30-49096394f841",
        "type": "contract.terminated",
        "severity": "Warning",
        "title": "تم إنهاء العقد",
        "body": "اكتملت إجراءات إنهاء العقد وتسويته.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:37.8408834",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "3e7d8253-eb47-4140-ba3f-d743f3a8b5ba",
        "type": "contract.termination-requested",
        "severity": "Warning",
        "title": "تم طلب إنهاء العقد",
        "body": "تم تسجيل طلب إنهاء العقد، وتجري معالجة التسوية اللازمة.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:37.7823417",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "d7ce2978-4f92-4234-9890-a67b7aa9d256",
        "type": "contract.activated",
        "severity": "Success",
        "title": "تم تفعيل العقد",
        "body": "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:35.9582728",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "4e615ddd-c9cb-413b-9b49-88fbed1840c1",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:35.7739812",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "d69c72f5-0168-43ff-8af8-02f3235a6686",
        "type": "contract.acceptance-recorded",
        "severity": "Information",
        "title": "موافقة جديدة على العقد",
        "body": "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
        "actionUrl": null,
        "data": {
          "contractId": "4e821858-2345-4120-bc2e-6417d80c7658",
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:32.8758527",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "19ac3cb1-78ea-4d1c-aee7-0eecc9dc0c99",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/60694a52-77d0-46cc-ac37-03fc63084417",
        "data": {
          "proposalId": "60694a52-77d0-46cc-ac37-03fc63084417",
          "legalCaseId": "d4cc8af6-91b4-4fb3-908c-b730247d01a0"
        },
        "createdAtUtc": "2026-08-09T14:28:31.0568395",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": null,
    "unreadCount": 15
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client receives exact completion**
- [PASS] **Lawyer receives exact completion**

## Unsupported methods and final notification isolation

### DELETE collection is unsupported

**Request:** DELETE http://localhost:5049/api/contracts

**Response Status:** 405

**Response Body:**
(Empty)
---


- [PASS] **DELETE collection is unsupported** (status=405)
### PATCH detail is unsupported

**Request:** PATCH http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658

**Response Status:** 405

**Response Body:**
(Empty)
---


- [PASS] **PATCH detail is unsupported** (status=405)
### DELETE detail is unsupported

**Request:** DELETE http://localhost:5049/api/contracts/4e821858-2345-4120-bc2e-6417d80c7658

**Response Status:** 405

**Response Body:**
(Empty)
---


- [PASS] **DELETE detail is unsupported** (status=405)
### Unrelated user notification feed remains isolated

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


- [PASS] **No Contract notification leaks to unrelated user**

## Execution summary

| Metric | Count |
|---|---:|
| Passed assertions | 146 |
| Failed assertions | 0 |

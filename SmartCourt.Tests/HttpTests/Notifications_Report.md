# Notifications HTTP Test Report

Generated at: 2026-08-09 16:44:44 +03:00


## Health and unauthenticated access

### Health check

**Request:** GET http://localhost:5049/health

**Response Status:** 200

**Response Body:**
```text
Healthy
```
---


- [PASS] **API is healthy** (status=200)
### Feed requires authentication

**Request:** GET http://localhost:5049/api/notifications

**Response Status:** 401

**Response Body:**
(Empty)
---


- [PASS] **Feed requires authentication** (status=401)
### Unread count requires authentication

**Request:** GET http://localhost:5049/api/notifications/unread-count

**Response Status:** 401

**Response Body:**
(Empty)
---


- [PASS] **Unread count requires authentication** (status=401)
### Mark one requires authentication

**Request:** PATCH http://localhost:5049/api/notifications/57394fba-d667-4034-b058-7a08442a5a91/read

**Response Status:** 401

**Response Body:**
(Empty)
---


- [PASS] **Mark one requires authentication** (status=401)
### Mark all requires authentication

**Request:** PATCH http://localhost:5049/api/notifications/read-all

**Response Status:** 401

**Response Body:**
(Empty)
---


- [PASS] **Mark all requires authentication** (status=401)
### SignalR negotiate requires authentication

**Request:** POST http://localhost:5049/hubs/notifications/negotiate?negotiateVersion=1

**Response Status:** 401

**Response Body:**
(Empty)
---


- [PASS] **SignalR negotiate requires authentication** (status=401)
### Malformed bearer token

**Request:** GET http://localhost:5049/api/notifications

**Response Status:** 401

**Response Body:**
(Empty)
---


- [PASS] **Malformed bearer token returns 401** (status=401)

## Zero-assumption account and domain setup

### Register notification client

**Request:** POST http://localhost:5049/api/auth/register/client

**Body:**
```json
{
  "FullName": "Notification Client",
  "Email": "[REDACTED]",
  "Password": "[REDACTED]",
  "ConfirmPassword": "[REDACTED]"
}
```

**Response Status:** 201

**Response Body:**
```json
{
  "success": true,
  "data": {
    "userId": "dd35bce5-236a-4774-8768-08def61c5f35",
    "email": "[REDACTED]",
    "fullName": "Notification Client",
    "role": "Client"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **Register notification client** (status=201)
- [PASS] **Mock Email log contains client confirmation**
### Confirm client Email from mock log

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=dd35bce5-236a-4774-8768-08def61c5f35&token=[REDACTED]

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
### Login notification client

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
      "id": "dd35bce5-236a-4774-8768-08def61c5f35",
      "email": "[REDACTED]",
      "fullName": "Notification Client",
      "role": "Client",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "[REDACTED]",
    "expiresIn": 900,
    "refreshToken": "[REDACTED]",
    "refreshTokenExpiration": "2026-08-16T13:44:46.6179484Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Login notification client** (status=200)
### Complete notification client profile

**Request:** POST http://localhost:5049/api/clients/profile/complete

**Body:**
```json
{
  "Address": "Cairo",
  "Gender": 1,
  "PhoneNumber": "[REDACTED]",
  "NationalNumber": "[REDACTED]",
  "DateOfBirth": "1990-01-01"
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


- [PASS] **Complete notification client profile** (status=200)
### Register notification lawyer

**Request:** POST http://localhost:5049/api/auth/register/lawyer

**Body:**
```json
{
  "FullName": "Notification Lawyer",
  "Email": "[REDACTED]",
  "Password": "[REDACTED]",
  "ConfirmPassword": "[REDACTED]"
}
```

**Response Status:** 201

**Response Body:**
```json
{
  "success": true,
  "data": {
    "userId": "f2314004-f7f2-4800-8769-08def61c5f35",
    "email": "[REDACTED]",
    "fullName": "Notification Lawyer",
    "role": "Lawyer"
  },
  "message": "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني",
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **Register notification lawyer** (status=201)
- [PASS] **Mock Email log contains lawyer confirmation**
### Confirm lawyer Email from mock log

**Request:** GET http://localhost:5049/api/auth/confirm-email?userId=f2314004-f7f2-4800-8769-08def61c5f35&token=[REDACTED]

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
### Login notification lawyer

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
      "id": "f2314004-f7f2-4800-8769-08def61c5f35",
      "email": "[REDACTED]",
      "fullName": "Notification Lawyer",
      "role": "Lawyer",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "[REDACTED]",
    "expiresIn": 900,
    "refreshToken": "[REDACTED]",
    "refreshTokenExpiration": "2026-08-16T13:44:47.9445222Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Login notification lawyer** (status=200)
### Complete notification lawyer profile

**Request:** POST http://localhost:5049/api/lawyers/profile/complete

**Body:**
```json
{
  "Gender": 1,
  "Specializations": [
    {
      "Specialization": 1,
      "YearsOfExperience": 5,
      "CasesHandled": 10
    }
  ],
  "PhoneNumber": "[REDACTED]",
  "DateOfBirth": "1985-01-01",
  "NationalNumber": "[REDACTED]",
  "Bio": "Notification lifecycle test lawyer",
  "Address": "Cairo",
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


- [PASS] **Complete notification lawyer profile** (status=200)
### Login admin for account approval

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
    "refreshTokenExpiration": "2026-08-16T13:44:48.5116812Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Login admin for account approval** (status=200)
### Approve notification client

**Request:** PATCH http://localhost:5049/api/admin/verifications/dd35bce5-236a-4774-8768-08def61c5f35/approve-account

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


- [PASS] **Approve notification client** (status=200)
### Approve notification lawyer

**Request:** PATCH http://localhost:5049/api/admin/verifications/f2314004-f7f2-4800-8769-08def61c5f35/approve-account

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


- [PASS] **Approve notification lawyer** (status=200)
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
      "id": "dd35bce5-236a-4774-8768-08def61c5f35",
      "email": "[REDACTED]",
      "fullName": "Notification Client",
      "role": "Client",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "[REDACTED]",
    "expiresIn": 900,
    "refreshToken": "[REDACTED]",
    "refreshTokenExpiration": "2026-08-16T13:44:48.8648208Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Re-login approved client** (status=200)
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
      "id": "f2314004-f7f2-4800-8769-08def61c5f35",
      "email": "[REDACTED]",
      "fullName": "Notification Lawyer",
      "role": "Lawyer",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "[REDACTED]",
    "expiresIn": 900,
    "refreshToken": "[REDACTED]",
    "refreshTokenExpiration": "2026-08-16T13:44:49.0495109Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Re-login approved lawyer** (status=200)
### Authenticated admin may access personal empty inbox

**Request:** GET http://localhost:5049/api/notifications

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


- [PASS] **Notification API has no artificial role restriction** (status=200)
### Authenticated SignalR negotiate

**Request:** POST http://localhost:5049/hubs/notifications/negotiate?negotiateVersion=1

**Response Status:** 200

**Response Body:**
```json
{
  "negotiateVersion": 1,
  "connectionId": "57q5Rd8OfU3u_518a7xjNA",
  "connectionToken": "0B9l9gUDbqYto3WVpfGApQ",
  "availableTransports": [
    {
      "transport": "WebSockets",
      "transferFormats": [
        "Text",
        "Binary"
      ]
    },
    {
      "transport": "ServerSentEvents",
      "transferFormats": [
        "Text"
      ]
    },
    {
      "transport": "LongPolling",
      "transferFormats": [
        "Text",
        "Binary"
      ]
    }
  ]
}
```
---


- [PASS] **Authenticated SignalR hub negotiation succeeds** (status=200)
### Create case for notification lifecycle

**Request:** POST http://localhost:5049/api/Case

**Body:**
```json
{
  "Description": "A complete case used to verify durable in-app proposal notifications.",
  "Title": "Notification lifecycle case 20260809164444466",
  "Governorate": "Cairo",
  "City": "Maadi"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "caseId": "1dfa0711-25d4-4519-b50e-b62440e5133c",
    "failedDocuments": []
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **Create case for notification lifecycle** (status=200)
### Review notification lifecycle case

**Request:** POST http://localhost:5049/api/cases/1dfa0711-25d4-4519-b50e-b62440e5133c/review

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
    "id": "df29f039-4ce9-4f22-b149-855dc19219e3",
    "caseId": "1dfa0711-25d4-4519-b50e-b62440e5133c",
    "isLatest": true,
    "createdAt": "2026-08-09T13:44:50.7986403Z",
    "reviewPoints": [
      {
        "id": "157b556a-095c-4cd0-bd10-c3fbd0a16563",
        "description": "تتمثل نقطة القوة الأساسية في صياغة الموضوع بوضوح حول 'Notification lifecycle case 20260809164444466'، وتوافر السند المبدئي الذي يرجح كفة الموكل في إثبات أصل الالتزام وتفوقه إثباتياً على الخصم.",
        "type": "Strength"
      },
      {
        "id": "d65b175b-5fe9-47c6-a6d8-66d10142553e",
        "description": "ميزة الخصم تتمثل في غياب التوثيق الرسمي للتنبيهات أو الإخطارات المتبادلة بين الأطراف، مما يتيح له إنكار الاستلام أو الدفع بالتراخي في المطالبة.",
        "type": "Weakness"
      },
      {
        "id": "6caaa5e0-e245-4508-adc7-998c9f710028",
        "description": "يحتاج الملف إلى استيفاء النقاط والمعلومات التالية لضمان صياغة صحيفة الدعوى بشكل مكتمل: حصر وتفصيص المبالغ المالية المطلوبة والتعويضات الدقيقة عن الضرر المادي والمعنوي، وإدراج التواريخ الرسمية الدقيقة لبدء النزاع وتاريخ الإخلال بالتعهدات.",
        "type": "MissingCaseInfo"
      },
      {
        "id": "1d40e30b-341f-4038-8367-e263d06e6e57",
        "description": "المستندات المحددة المطلوبة لإكمال الملف: أصل العقد/الاتفاق المبرم، صورة بطاقة الرقم القومي سارية لكل أطراف الدعوى، إيصالات التحويل أو السداد المالي، وأي إنذارات رسمية على يد محضر.",
        "type": "MissingCaseDoc"
      },
      {
        "id": "1c0e601b-97f8-4fac-8cd1-517a10eac51c",
        "description": "قم بإعادة هيكلة وصف القضية في صورة جدول زمني متسلسل، يبدأ من تاريخ التعهد الأول، مروراً بتاريخ الإخلال، وصولاً إلى حجم الأضرار المترتبة حالياً.",
        "type": "Suggestion"
      },
      {
        "id": "5611a195-8859-4326-8a18-8cb2d4f0598c",
        "description": "قم بتفقيط وقسمة كافة المطالبات المالية إلى بنود مستقلة (أصل الدين، الفوائد أو التعويض عن المماطلة، والرسوم) وتوثيق كل بند بسند كتابي مستقل.",
        "type": "Suggestion"
      },
      {
        "id": "ddee59cc-c6f4-426f-a4fe-556fa21f987a",
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


- [PASS] **Review notification lifecycle case** (status=200)
### Finalize notification lifecycle case

**Request:** POST http://localhost:5049/api/Case/1dfa0711-25d4-4519-b50e-b62440e5133c/finalize

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
    "caseId": "1dfa0711-25d4-4519-b50e-b62440e5133c",
    "totalEligibleLawyers": 0,
    "recommendations": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Finalize notification lifecycle case** (status=200)

## Proposal-created and proposal-rejected notification lifecycle

### Create proposal that will be rejected

**Request:** POST http://localhost:5049/api/proposals

**Body:**
```json
{
  "LegalCaseId": "1dfa0711-25d4-4519-b50e-b62440e5133c",
  "Message": "Notification HTTP lifecycle proposal 164451786",
  "LawyerUserId": "f2314004-f7f2-4800-8769-08def61c5f35"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "f6fd1d50-36e5-4947-9bb8-5472bd8ed4ad",
    "legalCaseId": "1dfa0711-25d4-4519-b50e-b62440e5133c",
    "caseTitle": "Notification lifecycle case 20260809164444466",
    "clientUserId": "dd35bce5-236a-4774-8768-08def61c5f35",
    "clientName": "Notification Client",
    "lawyerUserId": "f2314004-f7f2-4800-8769-08def61c5f35",
    "lawyerName": "Notification Lawyer",
    "message": "Notification HTTP lifecycle proposal 164451786",
    "status": "Pending",
    "decisionReason": null,
    "createdAt": "2026-08-09T13:44:51.9100098",
    "respondedAt": null,
    "updatedAt": "2026-08-09T13:44:51.9100098",
    "conversationId": null
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **Create proposal that will be rejected** (status=200)
### Poll lawyer inbox for proposal.created

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


### Poll lawyer inbox for proposal.created

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


### Poll lawyer inbox for proposal.created

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "2e9ad752-8d1c-4ea6-beb0-02389802025b",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/f6fd1d50-36e5-4947-9bb8-5472bd8ed4ad",
        "data": {
          "proposalId": "f6fd1d50-36e5-4947-9bb8-5472bd8ed4ad",
          "legalCaseId": "1dfa0711-25d4-4519-b50e-b62440e5133c"
        },
        "createdAtUtc": "2026-08-09T13:44:51.9510322",
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


- [PASS] **Lawyer receives durable proposal.created**
- [PASS] **Created payload contract**
### Reject first proposal

**Request:** POST http://localhost:5049/api/proposals/f6fd1d50-36e5-4947-9bb8-5472bd8ed4ad/reject

**Body:**
```json
{
  "Reason": "Unable to take this matter during the requested period."
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "f6fd1d50-36e5-4947-9bb8-5472bd8ed4ad",
    "legalCaseId": "1dfa0711-25d4-4519-b50e-b62440e5133c",
    "caseTitle": "Notification lifecycle case 20260809164444466",
    "clientUserId": "dd35bce5-236a-4774-8768-08def61c5f35",
    "clientName": "Notification Client",
    "lawyerUserId": "f2314004-f7f2-4800-8769-08def61c5f35",
    "lawyerName": "Notification Lawyer",
    "message": "Notification HTTP lifecycle proposal 164451786",
    "status": "Rejected",
    "decisionReason": "Unable to take this matter during the requested period.",
    "createdAt": "2026-08-09T13:44:51.9100098",
    "respondedAt": "2026-08-09T13:44:53.986348",
    "updatedAt": "2026-08-09T13:44:53.986348",
    "conversationId": null
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Reject first proposal** (status=200)
### Poll client inbox for proposal.rejected

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


### Poll client inbox for proposal.rejected

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "df71eccc-3b85-47b2-b01f-4fd2c1e06de5",
        "type": "proposal.rejected",
        "severity": "Warning",
        "title": "تم رفض العرض",
        "body": "رفض المحامي عرضك. يمكنك مراجعة التفاصيل واختيار محامٍ آخر.",
        "actionUrl": "/proposals/f6fd1d50-36e5-4947-9bb8-5472bd8ed4ad",
        "data": {
          "proposalId": "f6fd1d50-36e5-4947-9bb8-5472bd8ed4ad",
          "legalCaseId": "1dfa0711-25d4-4519-b50e-b62440e5133c"
        },
        "createdAtUtc": "2026-08-09T13:44:53.987404",
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


- [PASS] **Client receives durable proposal.rejected**
- [PASS] **Rejected Arabic payload contract**

## Proposal-accepted lifecycle and cursor pagination

### Create proposal that will be accepted

**Request:** POST http://localhost:5049/api/proposals

**Body:**
```json
{
  "LegalCaseId": "1dfa0711-25d4-4519-b50e-b62440e5133c",
  "Message": "Notification HTTP lifecycle proposal 164454989",
  "LawyerUserId": "f2314004-f7f2-4800-8769-08def61c5f35"
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "7379fa37-801e-44d1-aca3-6a80412fc21d",
    "legalCaseId": "1dfa0711-25d4-4519-b50e-b62440e5133c",
    "caseTitle": "Notification lifecycle case 20260809164444466",
    "clientUserId": "dd35bce5-236a-4774-8768-08def61c5f35",
    "clientName": "Notification Client",
    "lawyerUserId": "f2314004-f7f2-4800-8769-08def61c5f35",
    "lawyerName": "Notification Lawyer",
    "message": "Notification HTTP lifecycle proposal 164454989",
    "status": "Pending",
    "decisionReason": null,
    "createdAt": "2026-08-09T13:44:55.1517623",
    "respondedAt": null,
    "updatedAt": "2026-08-09T13:44:55.1517623",
    "conversationId": null
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **Create proposal that will be accepted** (status=200)
### Poll lawyer inbox for second proposal.created

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "2e9ad752-8d1c-4ea6-beb0-02389802025b",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/f6fd1d50-36e5-4947-9bb8-5472bd8ed4ad",
        "data": {
          "proposalId": "f6fd1d50-36e5-4947-9bb8-5472bd8ed4ad",
          "legalCaseId": "1dfa0711-25d4-4519-b50e-b62440e5133c"
        },
        "createdAtUtc": "2026-08-09T13:44:51.9510322",
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


### Poll lawyer inbox for second proposal.created

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "a0407e21-0ca7-4b07-bfeb-5f4e76a895d6",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/7379fa37-801e-44d1-aca3-6a80412fc21d",
        "data": {
          "proposalId": "7379fa37-801e-44d1-aca3-6a80412fc21d",
          "legalCaseId": "1dfa0711-25d4-4519-b50e-b62440e5133c"
        },
        "createdAtUtc": "2026-08-09T13:44:55.1521796",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "2e9ad752-8d1c-4ea6-beb0-02389802025b",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/f6fd1d50-36e5-4947-9bb8-5472bd8ed4ad",
        "data": {
          "proposalId": "f6fd1d50-36e5-4947-9bb8-5472bd8ed4ad",
          "legalCaseId": "1dfa0711-25d4-4519-b50e-b62440e5133c"
        },
        "createdAtUtc": "2026-08-09T13:44:51.9510322",
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


- [PASS] **Lawyer receives second proposal.created**
### Accept second proposal

**Request:** POST http://localhost:5049/api/proposals/7379fa37-801e-44d1-aca3-6a80412fc21d/accept

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
    "id": "7379fa37-801e-44d1-aca3-6a80412fc21d",
    "legalCaseId": "1dfa0711-25d4-4519-b50e-b62440e5133c",
    "caseTitle": "Notification lifecycle case 20260809164444466",
    "clientUserId": "dd35bce5-236a-4774-8768-08def61c5f35",
    "clientName": "Notification Client",
    "lawyerUserId": "f2314004-f7f2-4800-8769-08def61c5f35",
    "lawyerName": "Notification Lawyer",
    "message": "Notification HTTP lifecycle proposal 164454989",
    "status": "Accepted",
    "decisionReason": null,
    "createdAt": "2026-08-09T13:44:55.1517623",
    "respondedAt": "2026-08-09T13:44:56.2338584",
    "updatedAt": "2026-08-09T13:44:56.2338584",
    "conversationId": "24a2bfd0-825b-479d-86f5-47f9d061ab39"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Accept second proposal** (status=200)
### Poll client inbox for proposal.accepted

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "df71eccc-3b85-47b2-b01f-4fd2c1e06de5",
        "type": "proposal.rejected",
        "severity": "Warning",
        "title": "تم رفض العرض",
        "body": "رفض المحامي عرضك. يمكنك مراجعة التفاصيل واختيار محامٍ آخر.",
        "actionUrl": "/proposals/f6fd1d50-36e5-4947-9bb8-5472bd8ed4ad",
        "data": {
          "proposalId": "f6fd1d50-36e5-4947-9bb8-5472bd8ed4ad",
          "legalCaseId": "1dfa0711-25d4-4519-b50e-b62440e5133c"
        },
        "createdAtUtc": "2026-08-09T13:44:53.987404",
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


### Poll client inbox for proposal.accepted

**Request:** GET http://localhost:5049/api/notifications?pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "7a82f390-e7f0-4d87-956d-668ac9f772a8",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/7379fa37-801e-44d1-aca3-6a80412fc21d",
        "data": {
          "proposalId": "7379fa37-801e-44d1-aca3-6a80412fc21d",
          "legalCaseId": "1dfa0711-25d4-4519-b50e-b62440e5133c"
        },
        "createdAtUtc": "2026-08-09T13:44:56.3166872",
        "readAtUtc": null,
        "expiresAtUtc": null
      },
      {
        "id": "df71eccc-3b85-47b2-b01f-4fd2c1e06de5",
        "type": "proposal.rejected",
        "severity": "Warning",
        "title": "تم رفض العرض",
        "body": "رفض المحامي عرضك. يمكنك مراجعة التفاصيل واختيار محامٍ آخر.",
        "actionUrl": "/proposals/f6fd1d50-36e5-4947-9bb8-5472bd8ed4ad",
        "data": {
          "proposalId": "f6fd1d50-36e5-4947-9bb8-5472bd8ed4ad",
          "legalCaseId": "1dfa0711-25d4-4519-b50e-b62440e5133c"
        },
        "createdAtUtc": "2026-08-09T13:44:53.987404",
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


- [PASS] **Client receives durable proposal.accepted**
- [PASS] **Accepted Arabic payload contract**
### Lawyer feed first cursor page

**Request:** GET http://localhost:5049/api/notifications?pageSize=1&isRead=false

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "a0407e21-0ca7-4b07-bfeb-5f4e76a895d6",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/7379fa37-801e-44d1-aca3-6a80412fc21d",
        "data": {
          "proposalId": "7379fa37-801e-44d1-aca3-6a80412fc21d",
          "legalCaseId": "1dfa0711-25d4-4519-b50e-b62440e5133c"
        },
        "createdAtUtc": "2026-08-09T13:44:55.1521796",
        "readAtUtc": null,
        "expiresAtUtc": null
      }
    ],
    "nextCursor": "djE6MTE",
    "unreadCount": 2
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **First cursor page has one item and nextCursor**
### Lawyer feed second cursor page

**Request:** GET http://localhost:5049/api/notifications?pageSize=1&isRead=false&cursor=djE6MTE

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "2e9ad752-8d1c-4ea6-beb0-02389802025b",
        "type": "proposal.created",
        "severity": "Information",
        "title": "عرض جديد",
        "body": "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        "actionUrl": "/proposals/f6fd1d50-36e5-4947-9bb8-5472bd8ed4ad",
        "data": {
          "proposalId": "f6fd1d50-36e5-4947-9bb8-5472bd8ed4ad",
          "legalCaseId": "1dfa0711-25d4-4519-b50e-b62440e5133c"
        },
        "createdAtUtc": "2026-08-09T13:44:51.9510322",
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


- [PASS] **Second cursor page returns a different item**

## Ownership, read state, and idempotency

### Client cannot mutate lawyer notification

**Request:** PATCH http://localhost:5049/api/notifications/2e9ad752-8d1c-4ea6-beb0-02389802025b/read

**Response Status:** 404

**Response Body:**
```json
{
  "success": false,
  "data": null,
  "message": "Entity \"Notification\" (2e9ad752-8d1c-4ea6-beb0-02389802025b) was not found.",
  "errors": null,
  "statusCode": 404
}
```
---


- [PASS] **Cross-user notification is hidden as 404** (status=404)
### Mark accepted notification read

**Request:** PATCH http://localhost:5049/api/notifications/7a82f390-e7f0-4d87-956d-668ac9f772a8/read

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "7a82f390-e7f0-4d87-956d-668ac9f772a8",
    "type": "proposal.accepted",
    "severity": "Success",
    "title": "تم قبول العرض",
    "body": "وافق المحامي على عرضك.",
    "actionUrl": "/proposals/7379fa37-801e-44d1-aca3-6a80412fc21d",
    "data": {
      "proposalId": "7379fa37-801e-44d1-aca3-6a80412fc21d",
      "legalCaseId": "1dfa0711-25d4-4519-b50e-b62440e5133c"
    },
    "createdAtUtc": "2026-08-09T13:44:56.3166872",
    "readAtUtc": "2026-08-09T13:44:57.450889Z",
    "expiresAtUtc": null
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Mark one read succeeds** (status=200)
### Repeat mark accepted notification read

**Request:** PATCH http://localhost:5049/api/notifications/7a82f390-e7f0-4d87-956d-668ac9f772a8/read

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "7a82f390-e7f0-4d87-956d-668ac9f772a8",
    "type": "proposal.accepted",
    "severity": "Success",
    "title": "تم قبول العرض",
    "body": "وافق المحامي على عرضك.",
    "actionUrl": "/proposals/7379fa37-801e-44d1-aca3-6a80412fc21d",
    "data": {
      "proposalId": "7379fa37-801e-44d1-aca3-6a80412fc21d",
      "legalCaseId": "1dfa0711-25d4-4519-b50e-b62440e5133c"
    },
    "createdAtUtc": "2026-08-09T13:44:56.3166872",
    "readAtUtc": "2026-08-09T13:44:57.450889",
    "expiresAtUtc": null
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Repeated mark-read preserves timestamp**
### Fetch read-only feed

**Request:** GET http://localhost:5049/api/notifications?isRead=true&pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "7a82f390-e7f0-4d87-956d-668ac9f772a8",
        "type": "proposal.accepted",
        "severity": "Success",
        "title": "تم قبول العرض",
        "body": "وافق المحامي على عرضك.",
        "actionUrl": "/proposals/7379fa37-801e-44d1-aca3-6a80412fc21d",
        "data": {
          "proposalId": "7379fa37-801e-44d1-aca3-6a80412fc21d",
          "legalCaseId": "1dfa0711-25d4-4519-b50e-b62440e5133c"
        },
        "createdAtUtc": "2026-08-09T13:44:56.3166872",
        "readAtUtc": "2026-08-09T13:44:57.450889",
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


- [PASS] **Read filter contains accepted notification**
### Fetch unread-only feed

**Request:** GET http://localhost:5049/api/notifications?isRead=false&pageSize=50

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "df71eccc-3b85-47b2-b01f-4fd2c1e06de5",
        "type": "proposal.rejected",
        "severity": "Warning",
        "title": "تم رفض العرض",
        "body": "رفض المحامي عرضك. يمكنك مراجعة التفاصيل واختيار محامٍ آخر.",
        "actionUrl": "/proposals/f6fd1d50-36e5-4947-9bb8-5472bd8ed4ad",
        "data": {
          "proposalId": "f6fd1d50-36e5-4947-9bb8-5472bd8ed4ad",
          "legalCaseId": "1dfa0711-25d4-4519-b50e-b62440e5133c"
        },
        "createdAtUtc": "2026-08-09T13:44:53.987404",
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


- [PASS] **Unread filter excludes accepted notification**
### Get unread count before read-all

**Request:** GET http://localhost:5049/api/notifications/unread-count

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "unreadCount": 1
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Unread-count endpoint reconciles feed**
### Mark all client notifications read

**Request:** PATCH http://localhost:5049/api/notifications/read-all

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "readAtUtc": "2026-08-09T13:44:57.5754387Z",
    "unreadCount": 0
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Read-all returns zero**
### Repeat mark-all read

**Request:** PATCH http://localhost:5049/api/notifications/read-all

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "readAtUtc": "2026-08-09T13:44:57.6379355Z",
    "unreadCount": 0
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Repeated read-all is idempotent**
### Get unread count after read-all

**Request:** GET http://localhost:5049/api/notifications/unread-count

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "unreadCount": 0
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Unread count remains zero**

## Validation, type coercion, malicious input, and methods

### Page size below minimum

**Request:** GET http://localhost:5049/api/notifications?pageSize=0

**Response Status:** 400

**Response Body:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "PageSize": [
      "'Page Size' must be between 1 and 50. You entered 0."
    ]
  },
  "traceId": "00-c612f0ca6d72830350dbbde7ada66f1b-3b46c617fa40b4a3-00"
}
```
---


- [PASS] **Page size below minimum returns 400** (status=400)
### Page size above maximum

**Request:** GET http://localhost:5049/api/notifications?pageSize=51

**Response Status:** 400

**Response Body:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "PageSize": [
      "'Page Size' must be between 1 and 50. You entered 51."
    ]
  },
  "traceId": "00-b54f91141cfd0fb5fdcb7a8ee028b9b0-f4a36642e73b7cd3-00"
}
```
---


- [PASS] **Page size above maximum returns 400** (status=400)
### Page size wrong type

**Request:** GET http://localhost:5049/api/notifications?pageSize=abc

**Response Status:** 400

**Response Body:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "PageSize": [
      "The value 'abc' is not valid for PageSize."
    ]
  },
  "traceId": "00-6bc3940f66b3b3311c80a508b7cad611-94665804938ccd89-00"
}
```
---


- [PASS] **Page size wrong type returns 400** (status=400)
### Boolean wrong type

**Request:** GET http://localhost:5049/api/notifications?isRead=banana

**Response Status:** 400

**Response Body:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "IsRead": [
      "The value 'banana' is not valid for IsRead."
    ]
  },
  "traceId": "00-d3d0f516c9310334aa3e2504251856a7-73b9ba363b8a8edb-00"
}
```
---


- [PASS] **Boolean wrong type returns 400** (status=400)
### Malformed cursor

**Request:** GET http://localhost:5049/api/notifications?cursor=not-base64

**Response Status:** 400

**Response Body:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Cursor": [
      "Cursor is invalid or unsupported."
    ]
  },
  "traceId": "00-7ec71d43582d598d549aff2b929cf310-11e0f462cd2a82f5-00"
}
```
---


- [PASS] **Malformed cursor returns 400** (status=400)
### Unicode cursor

**Request:** GET http://localhost:5049/api/notifications?cursor=%E2%9A%96%EF%B8%8F%20%D8%A5%D8%B4%D8%B9%D8%A7%D8%B1

**Response Status:** 400

**Response Body:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Cursor": [
      "Cursor is invalid or unsupported."
    ]
  },
  "traceId": "00-07249bcd2088f316aab20b784a7ac85f-8d62dcd1068b1db4-00"
}
```
---


- [PASS] **Unicode cursor returns 400** (status=400)
### Oversized cursor

**Request:** GET http://localhost:5049/api/notifications?cursor=AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA

**Response Status:** 400

**Response Body:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Cursor": [
      "Cursor is invalid or unsupported."
    ]
  },
  "traceId": "00-fd6bb9ecad5373c7613ae4e6a561ab02-d43ad13203efb54b-00"
}
```
---


- [PASS] **Oversized cursor returns 400** (status=400)
### SQL-like cursor

**Request:** GET http://localhost:5049/api/notifications?cursor=%27%20OR%201%3D1%20--

**Response Status:** 400

**Response Body:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Cursor": [
      "Cursor is invalid or unsupported."
    ]
  },
  "traceId": "00-14e1e7ef3f0a7d203c3e9bd1c4a13d1a-7178f6cd4285ad3e-00"
}
```
---


- [PASS] **SQL-like cursor returns 400** (status=400)
### Unknown notification id

**Request:** PATCH http://localhost:5049/api/notifications/e1f39346-9dff-416d-b9cc-2acf8e31daa6/read

**Response Status:** 404

**Response Body:**
```json
{
  "success": false,
  "data": null,
  "message": "Entity \"Notification\" (e1f39346-9dff-416d-b9cc-2acf8e31daa6) was not found.",
  "errors": null,
  "statusCode": 404
}
```
---


- [PASS] **Unknown notification returns 404** (status=404)
### Non-Guid notification route

**Request:** PATCH http://localhost:5049/api/notifications/not-a-guid/read

**Response Status:** 404

**Response Body:**
(Empty)
---


- [PASS] **Non-Guid route does not match** (status=404)
### Unsupported POST on feed

**Request:** POST http://localhost:5049/api/notifications

**Body:**
```json
{}
```

**Response Status:** 405

**Response Body:**
(Empty)
---


- [PASS] **Unsupported POST returns 405** (status=405)
### Unsupported DELETE on feed

**Request:** DELETE http://localhost:5049/api/notifications

**Response Status:** 405

**Response Body:**
(Empty)
---


- [PASS] **Unsupported DELETE returns 405** (status=405)

## Execution summary


| Metric | Count |
|---|---:|
| Passed assertions | 61 |
| Failed assertions | 0 |


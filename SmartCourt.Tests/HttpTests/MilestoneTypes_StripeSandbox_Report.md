# Milestone Types - Real Stripe Sandbox HTTP Test Report

Generated at: 2026-08-13 21:40:22 +03:00

This suite uses the application's real Stripe Connect provider with Stripe test-mode
objects. Authentication tokens, confirmation links, client secrets, provider object
identifiers, and application/Stripe secrets are redacted.

## Provider safety and authorization boundaries

### Read active payment provider configuration

**Request:** GET http://localhost:5051/api/payments/config

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "providerCode": "StripeConnect",
    "publishableKey": "[REDACTED_TEST_KEY]",
    "currency": "EGP",
    "sandboxOnly": true,
    "confirmationTokensEnabled": true,
    "savedPaymentMethodsEnabled": true
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Payment provider configuration is reachable** (status=200)
- [PASS] **Real Stripe Connect provider is active**
- [PASS] **Provider is strictly sandbox-only**
- [PASS] **Publishable key is a Stripe test key**
### Anonymous milestone list is rejected

**Request:** GET http://localhost:5051/api/contracts/fae3243d-2419-421f-869e-4e1b7a972f79/milestones

**Response Status:** 401

**Response Body:**
```text
Response status code does not indicate success: 401 (Unauthorized).
```
---


- [PASS] **Anonymous milestone list is rejected** (status=401, expected=401)
### Anonymous milestone creation is rejected

**Request:** POST http://localhost:5051/api/contracts/fae3243d-2419-421f-869e-4e1b7a972f79/milestones

**Body:**
```json
{}
```

**Response Status:** 401

**Response Body:**
```text
Response status code does not indicate success: 401 (Unauthorized).
```
---


- [PASS] **Anonymous milestone creation is rejected** (status=401, expected=401)
### Anonymous funding is rejected

**Request:** POST http://localhost:5051/api/milestones/fae3243d-2419-421f-869e-4e1b7a972f79/fund

**Body:**
```json
{}
```

**Response Status:** 401

**Response Body:**
```text
Response status code does not indicate success: 401 (Unauthorized).
```
---


- [PASS] **Anonymous funding is rejected** (status=401, expected=401)
### Anonymous payout account access is rejected

**Request:** GET http://localhost:5051/api/wallet/payout-account

**Response Status:** 401

**Response Body:**
```text
Response status code does not indicate success: 401 (Unauthorized).
```
---


- [PASS] **Anonymous payout account access is rejected** (status=401, expected=401)

## Zero-assumption users and mock Email confirmation

### Login test SuperAdministrator

**Request:** POST http://localhost:5051/api/auth/login

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
    "expiresIn": 3600,
    "refreshToken": "[REDACTED]",
    "refreshTokenExpiration": "2026-08-20T18:40:23.6763176Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Login test SuperAdministrator** (status=200)
### Register client

**Request:** POST http://localhost:5051/api/auth/register/client

**Body:**
```json
{
  "ConfirmPassword": "[REDACTED]",
  "FullName": "Stripe sandbox milestone client",
  "Email": "[REDACTED]",
  "Password": "[REDACTED]"
}
```

**Response Status:** 201

**Response Body:**
```json
{
  "success": true,
  "data": {
    "userId": "ba55ffb1-0d93-4cc0-9355-08def96a560b",
    "email": "[REDACTED]",
    "fullName": "Stripe sandbox milestone client",
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

**Request:** GET http://localhost:5051/api/auth/confirm-email?userId=ba55ffb1-0d93-4cc0-9355-08def96a560b&token=[REDACTED]

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

**Request:** POST http://localhost:5051/api/auth/login

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
      "id": "ba55ffb1-0d93-4cc0-9355-08def96a560b",
      "email": "[REDACTED]",
      "fullName": "Stripe sandbox milestone client",
      "role": "Client",
      "status": "Unverified",
      "rejectionReason": null
    },
    "accessToken": "[REDACTED]",
    "expiresIn": 3600,
    "refreshToken": "[REDACTED]",
    "refreshTokenExpiration": "2026-08-20T18:40:24.2716522Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Login client** (status=200)
### Complete client profile

**Request:** POST http://localhost:5051/api/clients/profile/complete

**Body:**
```json
{
  "NationalNumber": "[REDACTED]",
  "DateOfBirth": "1990-01-01",
  "Address": "Cairo",
  "PhoneNumber": "[REDACTED]",
  "Gender": 1
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

**Request:** PATCH http://localhost:5051/api/admin/verifications/ba55ffb1-0d93-4cc0-9355-08def96a560b/approve-account

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

**Request:** POST http://localhost:5051/api/auth/login

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
      "id": "ba55ffb1-0d93-4cc0-9355-08def96a560b",
      "email": "[REDACTED]",
      "fullName": "Stripe sandbox milestone client",
      "role": "Client",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "[REDACTED]",
    "expiresIn": 3600,
    "refreshToken": "[REDACTED]",
    "refreshTokenExpiration": "2026-08-20T18:40:24.6690316Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Re-login approved client** (status=200)
### Login existing onboarded test lawyer

**Request:** POST http://localhost:5051/api/auth/login

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
      "id": "57e9e9d6-09f1-487d-67e5-08def964d49c",
      "email": "[REDACTED]",
      "fullName": "Stripe sandbox milestone lawyer",
      "role": "Lawyer",
      "status": "Active",
      "rejectionReason": null
    },
    "accessToken": "[REDACTED]",
    "expiresIn": 3600,
    "refreshToken": "[REDACTED]",
    "refreshTokenExpiration": "2026-08-20T18:40:24.7887921Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Login existing onboarded test lawyer** (status=200)

## Real Stripe saved payment method

- [PASS] **Stripe secret is test-mode only**
### Create SetupIntent through Smart Court

**Request:** POST http://localhost:5051/api/payment-methods/setup-session

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
    "setupIntentId": "seti_1U43YY09HdpVdJvPACm7m2ds",
    "clientSecret": "[REDACTED]",
    "status": "requires_payment_method"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Create SetupIntent through Smart Court** (status=200)
### Confirm SetupIntent with Stripe test PaymentMethod

**Request:** POST https://api.stripe.com/v1/setup_intents/[REDACTED]/confirm

**Body:**
```text
payment_method=pm_card_visa
```

**Response Status:** 200

**Response Body:**
```json
{
  "livemode": false,
  "status": "succeeded",
  "id": "[REDACTED]",
  "payment_method": "[REDACTED]"
}
```
---


- [PASS] **Stripe confirms the test SetupIntent**
### List Stripe-backed saved payment methods

**Request:** GET http://localhost:5051/api/payment-methods

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "paymentMethodReference": "[REDACTED]",
      "type": "card",
      "brand": "visa",
      "last4": "4242",
      "expiryMonth": 8,
      "expiryYear": 2027,
      "holderName": null,
      "isDefault": false
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **List Stripe-backed saved payment methods** (status=200)
- [PASS] **Stripe test Visa is visible through Smart Court**
### Set Stripe test card as default

**Request:** PUT http://localhost:5051/api/payment-methods/pm_1U43ZO09HdpVdJvPcEuKprO6/default

**Body:**
```json
{}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": "Default payment method updated.",
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Set Stripe test card as default** (status=200)

## Stripe Connect test payout onboarding

### Read lawyer payout account

**Request:** GET http://localhost:5051/api/wallet/payout-account

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "46544a8f-9f31-4dc7-bbee-a996fb971e91",
    "providerCode": "StripeConnect",
    "status": "Enabled",
    "detailsSubmitted": true,
    "transfersEnabled": true,
    "payoutsEnabled": true,
    "country": "US",
    "defaultCurrency": "usd",
    "maskedDestination": null,
    "lastSynchronizedAt": "2026-08-13T18:41:53.3778736Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Read lawyer payout account** (status=200)
- [PASS] **Stripe test recipient can receive transfers**
- [PASS] **Stripe test recipient can receive payouts**

## Contract foundation and Draft milestone types

### Create a fresh client case

**Request:** POST http://localhost:5051/api/Case

**Body:**
```json
{
  "Governorate": "Cairo",
  "Title": "Stripe milestone types case 20260813214023242",
  "City": "Maadi",
  "Description": "Real Stripe sandbox verification for Standard and Expense milestones."
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "caseId": "9f3379db-c236-4299-9fb9-92825cd3c9c0",
    "failedDocuments": []
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **Create a fresh client case** (status=200)
### Run case review

**Request:** POST http://localhost:5051/api/cases/9f3379db-c236-4299-9fb9-92825cd3c9c0/review

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
    "id": "7bb7eb24-511b-4ed3-964e-bc44c1cdab2c",
    "caseId": "9f3379db-c236-4299-9fb9-92825cd3c9c0",
    "isLatest": true,
    "createdAt": "2026-08-13T18:42:27.3645393Z",
    "reviewPoints": [
      {
        "id": "a84f9dd1-16a4-401d-91fd-e81ac2d3a391",
        "description": "لا توجد نقاط قوة قانونية مُستخلصة من واقع القضية المقدمة، إذ يفتقر الوصف تمامًا إلى أي عنصر قانوني أو واقعي يمكن أن يشكل أساساً لدعوى أو دفاع أمام القضاء المصري؛ فعبارة 'Real Stripe sandbox verification for Standard and Expense milestones' لا تمثل نزاعاً قانونياً قائماً ولا تشير إلى طرف آخر أو انتهاك أو التزام مدني أو تجاري أو عقد مبرم أو خرق للقانون المصري رقم 120 لسنة 2008 بشأن التجارة الإلكترونية أو القانون رقم 17 لسنة 1999 بشأن تنظيم ممارسة الأنشطة المالية غير المصرفية أو حتى أحكام القانون المدني رقم 131 لسنة 1948 المتعلقة بالالتزامات والعقود؛ وبالتالي لا يوجد في الحالة ما يُعتبر 'قوة قانونية' تُعزز موقف العميل أمام المحكمة المصرية.",
        "type": "Strength"
      },
      {
        "id": "b26cba54-22e2-4d2e-aef2-06cff03f3e57",
        "description": "الضعف الجوهري يكمن في غياب أي سند قانوني أو واقعي يُشكّل نزاعاً قابلاً للنظر أمام القضاء المصري: لا وجود لطرف منازع، ولا عقد مكتوب أو إلكتروني موثق، ولا إشعار رسمي بطلب تنفيذ أو إنذار بفسخ، ولا دليل على تقديم خدمة أو توريد أو دفع أو تأخير أو ضرر مادي أو معنوي، ولا حتى تحديد لطبيعة العلاقة (توكيل، مقاولة، وكالة إلكترونية، علاقة توزيع، إلخ)، مما يُتيح لأي طرف منازع أن يتمسك بعدم اختصاص المحكمة لانعدام ركن الدعوى المتمثل في 'المحل' و'السبب' وفقاً للمادة 25 من قانون المرافعات رقم 13 لسنة 1968، كما يُمكن الادعاء بعدم توافر شرط الاختصاص النوعي أو المحلي بموجب المادة 28 من ذات القانون، خاصةً وأن 'Stripe' شركة أجنبية غير مسجلة رسمياً في السجل التجاري المصري كفرع أو كشركة مصرية، ولا يُمكن تطبيق أحكام قانون الاستثمار أو قانون الشركات عليها دون وجود علاقة قانونية محددة ومُوثقة داخل النطاق الإقليمي المصري.",
        "type": "Weakness"
      },
      {
        "id": "9ecc0d72-3d24-47d6-b1d0-dd9d1f588946",
        "description": "يجب إعادة هيكلة وصف القضية بشكل كامل ليشمل: (أ) تحديد واضح للطرف الآخر (اسم الشركة أو الشخص الطبيعي، وعنوانه القانوني الكامل في مصر أو خارجها)، (ب) نوع العلاقة القانونية (عقد إلكتروني موقّع؟ اتفاقية شروط الخدمة المُعتمدة من قبل Stripe؟ سياسة الاستخدام المُطبقة في مصر؟)، (ج) وصف دقيق للحدث المُنازع فيه (مثل: رفض تفعيل حساب تجاري في بيئة الـsandbox رغم استيفاء الشروط، أو حجب أموال مخصصة لمراحل دفع 'Milestones'، أو خطأ تقني أدى إلى خسارة مالية مُثبتة)، (د) ذكر جميع التواريخ الرسمية بدقة (تاريخ التسجيل، تاريخ الطلب، تاريخ الرفض، تاريخ الإخطار الأولي)، (هـ) تحديد المطالبة القانونية بدقة (إجبار على التفعيل؟ تعويض مالي عن خسارة مباشرة؟ إلغاء شرط تعسفي في شروط الخدمة؟)، مع تفصيل المبلغ المطالب به إلى أصل الدين وفوائد تأخير وفقاً للمادة 226 من القانون المدني، وأضرار مادية مُستند إليها.",
        "type": "Suggestion"
      },
      {
        "id": "a32bd36d-dadb-473c-8b58-de3cc2c74f61",
        "description": "المعلومات المفقودة جوهرية وتتضمن: اسم الطرف الآخر الكامل وبياناته القانونية، نوع العقد أو الاتفاق الإلكتروني المُبرم، رقم العقد أو هوية الحساب على منصة Stripe، تواريخ محددة (تاريخ إنشاء الحساب، تاريخ تقديم طلب التحقق، تاريخ الرفض أو التعطيل، تاريخ أول محاولة اتصال رسمية)، وصف دقيق لـ'Standard' و'Expense milestones' من حيث طبيعتها القانونية (دفعة مقابل خدمة؟ مكافأة أداء؟ مبلغ استثماري؟)، المبلغ المالي الفعلي المتأثر أو المُطالب به مع تفصيل حسابي مدعوم بالعملة والأساس القانوني لاحتسابه، وتحديد ما إذا كانت المسألة تتعلق بنظام 'sandbox' فقط أم أن هناك انتقالاً فعلياً إلى البيئة الإنتاجية وتم تفعيل عمليات مالية فعلية عبر النظام المصرفي المصري.",
        "type": "MissingCaseInfo"
      },
      {
        "id": "aaf111e7-03d5-4da3-9391-3279352bb23e",
        "description": "المستندات المطلوبة وفقاً لأحكام القانون المصري تشمل: (1) صورة من البطاقة الشخصية أو السجل التجاري للعميل (حسب كونه شخصاً طبيعياً أو اعتبارياً)، (2) نسخة موثقة من عقد الاستخدام أو شروط الخدمة المُطبقة من قبل Stripe عند التسجيل (مع ترجمة معتمدة إن كانت بالإنجليزية)، (3) لقطات شاشة رسمية مُؤرخة ومُوثقة من خلال برنامج موثوق (مثل أدوات التوثيق الإلكتروني المعتمدة من الهيئة القومية للبريد) توضح حالة الحساب ورسائل الخطأ أو الرفض، (4) إشعارات إلكترونية رسمية مرسلة عبر البريد الإلكتروني المؤكد للطرف الآخر مع إثبات إرسال واستلام (باستخدام خدمة البريد الإلكتروني المصرفي أو خدمة التوثيق الإلكتروني)، (5) سندات دفع أو تحويلات بنكية مصرية تثبت وجود علاقة مالية فعلية (مثل إيصالات سداد رسوم تفعيل أو تحويلات لحساب Stripe عبر بنك مصري)، (6) شهادة من هيئة تنظيم الاتصالات أو البنك المركزي المصري – في حال وجود شكاوى سابقة متعلقة بنفس الموضوع، إن وجدت.",
        "type": "MissingCaseDoc"
      }
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Run case review** (status=200)
### Finalize case

**Request:** POST http://localhost:5051/api/Case/9f3379db-c236-4299-9fb9-92825cd3c9c0/finalize

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
    "caseId": "9f3379db-c236-4299-9fb9-92825cd3c9c0",
    "totalEligibleLawyers": 0,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 0,
    "hasNextPage": false,
    "hasPreviousPage": false,
    "recommendations": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Finalize case** (status=200)
### Client proposes lawyer engagement

**Request:** POST http://localhost:5051/api/proposals

**Body:**
```json
{
  "LawyerUserId": "57e9e9d6-09f1-487d-67e5-08def964d49c",
  "LegalCaseId": "9f3379db-c236-4299-9fb9-92825cd3c9c0",
  "Message": "Real Stripe sandbox milestone type workflow."
}
```

**Response Status:** 201

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "d43ef042-a9de-4580-8a3f-75861d99034c",
    "legalCaseId": "9f3379db-c236-4299-9fb9-92825cd3c9c0",
    "caseTitle": "Stripe milestone types case 20260813214023242",
    "clientUserId": "ba55ffb1-0d93-4cc0-9355-08def96a560b",
    "clientName": "Stripe sandbox milestone client",
    "lawyerUserId": "57e9e9d6-09f1-487d-67e5-08def964d49c",
    "lawyerName": "Stripe sandbox milestone lawyer",
    "message": "Real Stripe sandbox milestone type workflow.",
    "status": "Pending",
    "decisionReason": null,
    "caseStatus": "Matched",
    "assignedLawyerUserId": null,
    "isAssignedLawyer": false,
    "contractId": null,
    "contractStatus": null,
    "conversationId": null,
    "conversationStatus": null,
    "canChat": false,
    "permittedActions": [
      "Cancel"
    ],
    "createdAt": "2026-08-13T18:42:37.6831442",
    "respondedAt": null,
    "updatedAt": "2026-08-13T18:42:37.6831442",
    "expiresAt": "2026-08-16T18:42:37.6831442",
    "closedAt": null,
    "closedByUserId": null
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **Client proposes lawyer engagement** (status=201)
### Lawyer accepts proposal

**Request:** POST http://localhost:5051/api/proposals/d43ef042-a9de-4580-8a3f-75861d99034c/accept

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
    "id": "d43ef042-a9de-4580-8a3f-75861d99034c",
    "legalCaseId": "9f3379db-c236-4299-9fb9-92825cd3c9c0",
    "caseTitle": "Stripe milestone types case 20260813214023242",
    "clientUserId": "ba55ffb1-0d93-4cc0-9355-08def96a560b",
    "clientName": "Stripe sandbox milestone client",
    "lawyerUserId": "57e9e9d6-09f1-487d-67e5-08def964d49c",
    "lawyerName": "Stripe sandbox milestone lawyer",
    "message": "Real Stripe sandbox milestone type workflow.",
    "status": "Accepted",
    "decisionReason": null,
    "caseStatus": "Matched",
    "assignedLawyerUserId": null,
    "isAssignedLawyer": false,
    "contractId": null,
    "contractStatus": null,
    "conversationId": "03cd5827-8822-460e-9b2b-53a38a21dca7",
    "conversationStatus": "Open",
    "canChat": true,
    "permittedActions": [
      "OpenChat",
      "TerminateProposal",
      "CreateContract"
    ],
    "createdAt": "2026-08-13T18:42:37.6831442",
    "respondedAt": "2026-08-13T18:42:37.7994305",
    "updatedAt": "2026-08-13T18:42:37.7994305",
    "expiresAt": "2026-08-16T18:42:37.6831442",
    "closedAt": null,
    "closedByUserId": null
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Lawyer accepts proposal** (status=200)
### Lawyer creates Draft contract

**Request:** POST http://localhost:5051/api/contracts

**Body:**
```json
{
  "TermsAndConditions": "Complete terms for real Stripe sandbox verification of Standard deliverables and Expense reimbursement.",
  "ProposalId": "d43ef042-a9de-4580-8a3f-75861d99034c",
  "Title": "Milestone types Stripe sandbox contract"
}
```

**Response Status:** 201

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "ed1ebc46-c99b-4c36-83ad-c8143edb2ade",
    "proposalId": "d43ef042-a9de-4580-8a3f-75861d99034c",
    "legalCaseId": "9f3379db-c236-4299-9fb9-92825cd3c9c0",
    "clientUserId": "ba55ffb1-0d93-4cc0-9355-08def96a560b",
    "lawyerUserId": "57e9e9d6-09f1-487d-67e5-08def964d49c",
    "title": "Milestone types Stripe sandbox contract",
    "termsAndConditions": "Complete terms for real Stripe sandbox verification of Standard deliverables and Expense reimbursement.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": null,
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 0,
    "version": "\"AAAAAAAAK4g=\"",
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


- [PASS] **Lawyer creates Draft contract** (status=201)
### Client cannot create milestones

**Request:** POST http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/milestones

**Body:**
```json
{
  "OrderNumber": 90,
  "Amount": 50,
  "Type": 1,
  "Title": "Unauthorized expense"
}
```

**Response Status:** 403

**Response Body:**
```text
Response status code does not indicate success: 403 (Forbidden).
```
---


- [PASS] **Client cannot create milestones** (status=403, expected=403)
### Unknown milestone type is rejected

**Request:** POST http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/milestones

**Body:**
```json
{
  "OrderNumber": 91,
  "Amount": 50,
  "Type": 99,
  "Title": "Unknown type"
}
```

**Response Status:** 400

**Response Body:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Type": [
      "نوع المرحلة غير صالح."
    ]
  },
  "traceId": "00-e72647c5f18e9a725a2de2ad8963d35c-aaaf54822f052839-00"
}
```
---


- [PASS] **Unknown milestone type is rejected** (status=400, expected=400)
### Expense rejects Standard-only duration and deliverables

**Request:** POST http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/milestones

**Body:**
```json
{
  "Title": "Invalid expense",
  "Amount": 200,
  "Description": "Must not accept Standard fields.",
  "DurationDays": 5,
  "Type": 1,
  "Deliverables": [
    "Receipt"
  ],
  "OrderNumber": 92
}
```

**Response Status:** 400

**Response Body:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Deliverables": [
      "مرحلة المصروفات لا تقبل مخرجات عمل."
    ],
    "DurationDays": [
      "مرحلة المصروفات لا تقبل مدة تنفيذ."
    ]
  },
  "traceId": "00-f4fbf47596122b5e9d0acd00a1735903-2f1681401b6e39ef-00"
}
```
---


- [PASS] **Expense rejects Standard-only duration and deliverables** (status=400, expected=400)
### Create Draft Standard milestone

**Request:** POST http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/milestones

**Body:**
```json
{
  "Title": "Standard written deliverable",
  "Amount": 1100,
  "Description": "Prepare and submit the written legal deliverable.",
  "DurationDays": 10,
  "Type": 0,
  "Deliverables": [
    "Written legal memorandum"
  ],
  "OrderNumber": 1
}
```

**Response Status:** 201

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "177792ae-8a92-4483-a555-d80eb0787db1",
    "orderNumber": 1,
    "title": "Standard written deliverable",
    "description": "Prepare and submit the written legal deliverable.",
    "deliverables": [
      "Written legal memorandum"
    ],
    "amount": 1100,
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
    "version": "\"AAAAAAAAK4o=\"",
    "type": 0,
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


- [PASS] **Create Draft Standard milestone** (status=201)
### Create Draft Expense milestone

**Request:** POST http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/milestones

**Body:**
```json
{
  "OrderNumber": 2,
  "Amount": 300,
  "Type": 1,
  "Title": "Draft filing fee expense",
  "Description": "Court filing fee reimbursement."
}
```

**Response Status:** 201

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "8a6f4fe3-74ae-4208-8736-37df38f48982",
    "orderNumber": 2,
    "title": "Draft filing fee expense",
    "description": "Court filing fee reimbursement.",
    "amount": 300,
    "dueDate": null,
    "status": 0,
    "fundingStatus": 0,
    "escrowHoldId": null,
    "fundedAt": null,
    "submittedAt": null,
    "autoAcceptEligibleAt": null,
    "holdExpiresAt": null,
    "netLawyerAmount": null,
    "version": "\"AAAAAAAAK4w=\"",
    "type": 1,
    "permittedActions": [
      "Update",
      "Cancel"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **Create Draft Expense milestone** (status=201)
- [PASS] **Draft Expense response exposes Expense type**
- [PASS] **Draft Expense response omits Deliverables**
- [PASS] **Draft Expense response omits DurationDays**
### Expense update requires If-Match

**Request:** PUT http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/milestones/8a6f4fe3-74ae-4208-8736-37df38f48982

**Body:**
```json
{
  "Type": 1,
  "Title": "Updated filing fee",
  "Description": "Updated receipt amount."
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


- [PASS] **Expense update requires If-Match** (status=412, expected=412)
### Read Draft Expense for update

**Request:** GET http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "177792ae-8a92-4483-a555-d80eb0787db1",
      "orderNumber": 1,
      "title": "Standard written deliverable",
      "description": "Prepare and submit the written legal deliverable.",
      "deliverables": [
        "Written legal memorandum"
      ],
      "amount": 1100.0,
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
      "version": "\"AAAAAAAAK4o=\"",
      "type": 0,
      "permittedActions": [
        "Update",
        "Approve"
      ]
    },
    {
      "id": "8a6f4fe3-74ae-4208-8736-37df38f48982",
      "orderNumber": 2,
      "title": "Draft filing fee expense",
      "description": "Court filing fee reimbursement.",
      "amount": 300.0,
      "dueDate": null,
      "status": 0,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK4w=\"",
      "type": 1,
      "permittedActions": [
        "Update",
        "Cancel"
      ]
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Read Draft Expense for update** (status=200)
### Update Draft Expense

**Request:** PUT http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/milestones/8a6f4fe3-74ae-4208-8736-37df38f48982

**Body:**
```json
{
  "Type": 1,
  "Title": "Updated filing fee expense",
  "Description": "Updated court filing fee receipt."
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "8a6f4fe3-74ae-4208-8736-37df38f48982",
    "orderNumber": 2,
    "title": "Updated filing fee expense",
    "description": "Updated court filing fee receipt.",
    "amount": 300.0,
    "dueDate": null,
    "status": 0,
    "fundingStatus": 0,
    "escrowHoldId": null,
    "fundedAt": null,
    "submittedAt": null,
    "autoAcceptEligibleAt": null,
    "holdExpiresAt": null,
    "netLawyerAmount": null,
    "version": "\"AAAAAAAAK5w=\"",
    "type": 1,
    "permittedActions": [
      "Update",
      "Cancel"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Update Draft Expense** (status=200)
- [PASS] **Updated Expense still omits Standard-only fields**
### Client explicitly approves Draft Expense - read current milestone version

**Request:** GET http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "177792ae-8a92-4483-a555-d80eb0787db1",
      "orderNumber": 1,
      "title": "Standard written deliverable",
      "description": "Prepare and submit the written legal deliverable.",
      "deliverables": [
        "Written legal memorandum"
      ],
      "amount": 1100.0,
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
      "version": "\"AAAAAAAAK4o=\"",
      "type": 0,
      "permittedActions": [
        "Approve"
      ]
    },
    {
      "id": "8a6f4fe3-74ae-4208-8736-37df38f48982",
      "orderNumber": 2,
      "title": "Updated filing fee expense",
      "description": "Updated court filing fee receipt.",
      "amount": 300.0,
      "dueDate": null,
      "status": 0,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK5w=\"",
      "type": 1,
      "permittedActions": [
        "Approve",
        "Reject"
      ]
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client explicitly approves Draft Expense - read current milestone version** (status=200)
### Client explicitly approves Draft Expense - approve milestone

**Request:** POST http://localhost:5051/api/milestones/8a6f4fe3-74ae-4208-8736-37df38f48982/approve

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
    "entityId": "8a6f4fe3-74ae-4208-8736-37df38f48982",
    "status": "AwaitingFunding",
    "occurredAt": "2026-08-13T18:42:38.734331Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client explicitly approves Draft Expense - approve milestone** (status=200)
### Client approves Standard milestone - read current milestone version

**Request:** GET http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "177792ae-8a92-4483-a555-d80eb0787db1",
      "orderNumber": 1,
      "title": "Standard written deliverable",
      "description": "Prepare and submit the written legal deliverable.",
      "deliverables": [
        "Written legal memorandum"
      ],
      "amount": 1100.0,
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
      "version": "\"AAAAAAAAK4o=\"",
      "type": 0,
      "permittedActions": [
        "Approve"
      ]
    },
    {
      "id": "8a6f4fe3-74ae-4208-8736-37df38f48982",
      "orderNumber": 2,
      "title": "Updated filing fee expense",
      "description": "Updated court filing fee receipt.",
      "amount": 300.0,
      "dueDate": null,
      "status": 1,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK6M=\"",
      "type": 1,
      "permittedActions": [
        "Fund"
      ]
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client approves Standard milestone - read current milestone version** (status=200)
### Client approves Standard milestone - approve milestone

**Request:** POST http://localhost:5051/api/milestones/177792ae-8a92-4483-a555-d80eb0787db1/approve

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
    "entityId": "177792ae-8a92-4483-a555-d80eb0787db1",
    "status": "Draft",
    "occurredAt": "2026-08-13T18:42:38.8571733Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client approves Standard milestone - approve milestone** (status=200)
### Lawyer approves Standard milestone - read current milestone version

**Request:** GET http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "177792ae-8a92-4483-a555-d80eb0787db1",
      "orderNumber": 1,
      "title": "Standard written deliverable",
      "description": "Prepare and submit the written legal deliverable.",
      "deliverables": [
        "Written legal memorandum"
      ],
      "amount": 1100.0,
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
      "version": "\"AAAAAAAAK6Y=\"",
      "type": 0,
      "permittedActions": [
        "Update",
        "Approve"
      ]
    },
    {
      "id": "8a6f4fe3-74ae-4208-8736-37df38f48982",
      "orderNumber": 2,
      "title": "Updated filing fee expense",
      "description": "Updated court filing fee receipt.",
      "amount": 300.0,
      "dueDate": null,
      "status": 1,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK6M=\"",
      "type": 1,
      "permittedActions": []
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Lawyer approves Standard milestone - read current milestone version** (status=200)
### Lawyer approves Standard milestone - approve milestone

**Request:** POST http://localhost:5051/api/milestones/177792ae-8a92-4483-a555-d80eb0787db1/approve

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
    "entityId": "177792ae-8a92-4483-a555-d80eb0787db1",
    "status": "AwaitingFunding",
    "occurredAt": "2026-08-13T18:42:38.8987357Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Lawyer approves Standard milestone - approve milestone** (status=200)
### Read contract for client acceptance

**Request:** GET http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "ed1ebc46-c99b-4c36-83ad-c8143edb2ade",
    "proposalId": "d43ef042-a9de-4580-8a3f-75861d99034c",
    "legalCaseId": "9f3379db-c236-4299-9fb9-92825cd3c9c0",
    "clientUserId": "ba55ffb1-0d93-4cc0-9355-08def96a560b",
    "lawyerUserId": "57e9e9d6-09f1-487d-67e5-08def964d49c",
    "title": "Milestone types Stripe sandbox contract",
    "termsAndConditions": "Complete terms for real Stripe sandbox verification of Standard deliverables and Expense reimbursement.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": null,
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 1400.0,
    "version": "\"AAAAAAAAK4g=\"",
    "milestones": [
      {
        "id": "177792ae-8a92-4483-a555-d80eb0787db1",
        "orderNumber": 1,
        "title": "Standard written deliverable",
        "description": "Prepare and submit the written legal deliverable.",
        "amount": 1100.0,
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
        "version": "\"AAAAAAAAK6g=\"",
        "type": 0,
        "deliverables": [
          "Written legal memorandum"
        ]
      },
      {
        "id": "8a6f4fe3-74ae-4208-8736-37df38f48982",
        "orderNumber": 2,
        "title": "Updated filing fee expense",
        "description": "Updated court filing fee receipt.",
        "amount": 300.0,
        "dueDate": null,
        "status": 1,
        "fundingStatus": 0,
        "escrowHoldId": null,
        "fundedAt": null,
        "submittedAt": null,
        "autoAcceptEligibleAt": null,
        "holdExpiresAt": null,
        "netLawyerAmount": null,
        "version": "\"AAAAAAAAK6M=\"",
        "type": 1
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


- [PASS] **Read contract for client acceptance** (status=200)
### Client accepts contract

**Request:** POST http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/accept

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
    "entityId": "ed1ebc46-c99b-4c36-83ad-c8143edb2ade",
    "status": "Draft",
    "occurredAt": "2026-08-13T18:42:38.9568501Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client accepts contract** (status=200)
### Read contract for lawyer acceptance

**Request:** GET http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "ed1ebc46-c99b-4c36-83ad-c8143edb2ade",
    "proposalId": "d43ef042-a9de-4580-8a3f-75861d99034c",
    "legalCaseId": "9f3379db-c236-4299-9fb9-92825cd3c9c0",
    "clientUserId": "ba55ffb1-0d93-4cc0-9355-08def96a560b",
    "lawyerUserId": "57e9e9d6-09f1-487d-67e5-08def964d49c",
    "title": "Milestone types Stripe sandbox contract",
    "termsAndConditions": "Complete terms for real Stripe sandbox verification of Standard deliverables and Expense reimbursement.",
    "currency": "EGP",
    "status": 0,
    "acceptedByClientAt": "2026-08-13T18:42:38.9568501",
    "acceptedByLawyerAt": null,
    "activatedAt": null,
    "completedAt": null,
    "terminatedAt": null,
    "currentMilestoneTotal": 1400.0,
    "version": "\"AAAAAAAAK6s=\"",
    "milestones": [
      {
        "id": "177792ae-8a92-4483-a555-d80eb0787db1",
        "orderNumber": 1,
        "title": "Standard written deliverable",
        "description": "Prepare and submit the written legal deliverable.",
        "amount": 1100.0,
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
        "version": "\"AAAAAAAAK6g=\"",
        "type": 0,
        "deliverables": [
          "Written legal memorandum"
        ]
      },
      {
        "id": "8a6f4fe3-74ae-4208-8736-37df38f48982",
        "orderNumber": 2,
        "title": "Updated filing fee expense",
        "description": "Updated court filing fee receipt.",
        "amount": 300.0,
        "dueDate": null,
        "status": 1,
        "fundingStatus": 0,
        "escrowHoldId": null,
        "fundedAt": null,
        "submittedAt": null,
        "autoAcceptEligibleAt": null,
        "holdExpiresAt": null,
        "netLawyerAmount": null,
        "version": "\"AAAAAAAAK6M=\"",
        "type": 1
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


- [PASS] **Read contract for lawyer acceptance** (status=200)
### Lawyer accepts contract

**Request:** POST http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/accept

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
    "entityId": "ed1ebc46-c99b-4c36-83ad-c8143edb2ade",
    "status": "Active",
    "occurredAt": "2026-08-13T18:42:39.0107345Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Lawyer accepts contract** (status=200)

## Mid-contract Expense approval and forbidden work stages

### Lawyer proposes mid-contract Expense

**Request:** POST http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/milestones

**Body:**
```json
{
  "OrderNumber": 3,
  "Amount": 450,
  "Type": 1,
  "Title": "Mid-contract courier expense",
  "Description": "Urgent legal document courier reimbursement."
}
```

**Response Status:** 201

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "497c84d9-f187-49fc-8e3d-006d083a828b",
    "orderNumber": 3,
    "title": "Mid-contract courier expense",
    "description": "Urgent legal document courier reimbursement.",
    "amount": 450,
    "dueDate": null,
    "status": 0,
    "fundingStatus": 0,
    "escrowHoldId": null,
    "fundedAt": null,
    "submittedAt": null,
    "autoAcceptEligibleAt": null,
    "holdExpiresAt": null,
    "netLawyerAmount": null,
    "version": "\"AAAAAAAAK7A=\"",
    "type": 1,
    "permittedActions": [
      "Update",
      "Cancel"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **Lawyer proposes mid-contract Expense** (status=201)
### Client cannot fund Expense before explicit approval

**Request:** POST http://localhost:5051/api/milestones/497c84d9-f187-49fc-8e3d-006d083a828b/fund

**Body:**
```json
{
  "PaymentMethodReference": "[REDACTED]"
}
```

**Response Status:** 400

**Response Body:**
```json
{
  "success": false,
  "data": null,
  "message": "المرحلة ليست جاهزة للتمويل في حالتها الحالية.",
  "errors": null,
  "statusCode": 400
}
```
---


- [PASS] **Client cannot fund Expense before explicit approval** (status=400, expected=400,409)
### Lawyer cannot fund Expense

**Request:** POST http://localhost:5051/api/milestones/497c84d9-f187-49fc-8e3d-006d083a828b/fund

**Body:**
```json
{
  "PaymentMethodReference": "[REDACTED]"
}
```

**Response Status:** 403

**Response Body:**
```text
Response status code does not indicate success: 403 (Forbidden).
```
---


- [PASS] **Lawyer cannot fund Expense** (status=403, expected=403)
### Expense cannot enter Submission

**Request:** POST http://localhost:5051/api/milestones/497c84d9-f187-49fc-8e3d-006d083a828b/submit

**Body:**
```json
{
  "StoredFileIds": [],
  "Notes": "This stage must be unavailable."
}
```

**Response Status:** 400

**Response Body:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "StoredFileIds": [
      "يجب تحديد معرّفات ملفات صالحة ومصرح بها."
    ]
  },
  "traceId": "00-98e5b268c47e3082eddee0dabb1fd9db-dd6c4aef5db9b23f-00"
}
```
---


- [PASS] **Expense cannot enter Submission** (status=400, expected=400,409)
### Expense cannot enter Acceptance

**Request:** POST http://localhost:5051/api/milestones/497c84d9-f187-49fc-8e3d-006d083a828b/accept

**Body:**
```json
{}
```

**Response Status:** 400

**Response Body:**
```json
{
  "success": false,
  "data": null,
  "message": "مراحل المصروفات لا تمر بمراحل التجهيز أو التسليم أو القبول.",
  "errors": null,
  "statusCode": 400
}
```
---


- [PASS] **Expense cannot enter Acceptance** (status=400, expected=400,409)
### Expense cannot enter request-changes stage

**Request:** POST http://localhost:5051/api/milestones/497c84d9-f187-49fc-8e3d-006d083a828b/request-changes

**Body:**
```json
{
  "Reason": "Not applicable"
}
```

**Response Status:** 400

**Response Body:**
```json
{
  "success": false,
  "data": null,
  "message": "مراحل المصروفات لا تمر بمراحل التجهيز أو التسليم أو القبول.",
  "errors": null,
  "statusCode": 400
}
```
---


- [PASS] **Expense cannot enter request-changes stage** (status=400, expected=400,409)
### Expense cannot create a milestone change request

**Request:** POST http://localhost:5051/api/milestones/497c84d9-f187-49fc-8e3d-006d083a828b/change-requests

**Body:**
```json
{
  "Reason": "Not applicable",
  "ProposedDescription": "Not allowed",
  "ProposedDurationDays": 2
}
```

**Response Status:** 400

**Response Body:**
```json
{
  "success": false,
  "data": null,
  "message": "طلبات تعديل العمل متاحة للمراحل القياسية فقط.",
  "errors": null,
  "statusCode": 400
}
```
---


- [PASS] **Expense cannot create a milestone change request** (status=400, expected=400,409,412)
### Client explicitly approves mid-contract Expense - read current milestone version

**Request:** GET http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "177792ae-8a92-4483-a555-d80eb0787db1",
      "orderNumber": 1,
      "title": "Standard written deliverable",
      "description": "Prepare and submit the written legal deliverable.",
      "deliverables": [
        "Written legal memorandum"
      ],
      "amount": 1100.0,
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
      "version": "\"AAAAAAAAK6g=\"",
      "type": 0,
      "permittedActions": []
    },
    {
      "id": "8a6f4fe3-74ae-4208-8736-37df38f48982",
      "orderNumber": 2,
      "title": "Updated filing fee expense",
      "description": "Updated court filing fee receipt.",
      "amount": 300.0,
      "dueDate": null,
      "status": 1,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK6M=\"",
      "type": 1,
      "permittedActions": [
        "Fund"
      ]
    },
    {
      "id": "497c84d9-f187-49fc-8e3d-006d083a828b",
      "orderNumber": 3,
      "title": "Mid-contract courier expense",
      "description": "Urgent legal document courier reimbursement.",
      "amount": 450.0,
      "dueDate": null,
      "status": 0,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK7A=\"",
      "type": 1,
      "permittedActions": [
        "Approve",
        "Reject"
      ]
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client explicitly approves mid-contract Expense - read current milestone version** (status=200)
### Client explicitly approves mid-contract Expense - approve milestone

**Request:** POST http://localhost:5051/api/milestones/497c84d9-f187-49fc-8e3d-006d083a828b/approve

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
    "entityId": "497c84d9-f187-49fc-8e3d-006d083a828b",
    "status": "AwaitingFunding",
    "occurredAt": "2026-08-13T18:42:41.9610781Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client explicitly approves mid-contract Expense - approve milestone** (status=200)
### Read approved mid-contract Expense

**Request:** GET http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "177792ae-8a92-4483-a555-d80eb0787db1",
      "orderNumber": 1,
      "title": "Standard written deliverable",
      "description": "Prepare and submit the written legal deliverable.",
      "deliverables": [
        "Written legal memorandum"
      ],
      "amount": 1100.0,
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
      "version": "\"AAAAAAAAK6g=\"",
      "type": 0,
      "permittedActions": []
    },
    {
      "id": "8a6f4fe3-74ae-4208-8736-37df38f48982",
      "orderNumber": 2,
      "title": "Updated filing fee expense",
      "description": "Updated court filing fee receipt.",
      "amount": 300.0,
      "dueDate": null,
      "status": 1,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK6M=\"",
      "type": 1,
      "permittedActions": [
        "Fund"
      ]
    },
    {
      "id": "497c84d9-f187-49fc-8e3d-006d083a828b",
      "orderNumber": 3,
      "title": "Mid-contract courier expense",
      "description": "Urgent legal document courier reimbursement.",
      "amount": 450.0,
      "dueDate": null,
      "status": 1,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK88=\"",
      "type": 1,
      "permittedActions": [
        "Fund"
      ]
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Read approved mid-contract Expense** (status=200)
- [PASS] **Approved Expense is immediately awaiting funding**
### Create Expense rejection candidate

**Request:** POST http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/milestones

**Body:**
```json
{
  "OrderNumber": 4,
  "Amount": 80,
  "Type": 1,
  "Title": "Expense to reject",
  "Description": "Client rejection workflow."
}
```

**Response Status:** 201

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "b43a594c-8839-4bc5-a907-1d08bdc2ac67",
    "orderNumber": 4,
    "title": "Expense to reject",
    "description": "Client rejection workflow.",
    "amount": 80,
    "dueDate": null,
    "status": 0,
    "fundingStatus": 0,
    "escrowHoldId": null,
    "fundedAt": null,
    "submittedAt": null,
    "autoAcceptEligibleAt": null,
    "holdExpiresAt": null,
    "netLawyerAmount": null,
    "version": "\"AAAAAAAAK9E=\"",
    "type": 1,
    "permittedActions": [
      "Update",
      "Cancel"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **Create Expense rejection candidate** (status=201)
### Client rejects proposed Expense

**Request:** POST http://localhost:5051/api/milestones/b43a594c-8839-4bc5-a907-1d08bdc2ac67/reject

**Body:**
```json
{
  "Reason": "Receipt is insufficient."
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "entityId": "b43a594c-8839-4bc5-a907-1d08bdc2ac67",
    "status": "Cancelled",
    "occurredAt": "2026-08-13T18:42:42.0270606Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client rejects proposed Expense** (status=200)
### Create Expense cancellation candidate

**Request:** POST http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/milestones

**Body:**
```json
{
  "OrderNumber": 5,
  "Amount": 90,
  "Type": 1,
  "Title": "Expense to cancel",
  "Description": "Lawyer cancellation workflow."
}
```

**Response Status:** 201

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "577ed42e-3f49-49ab-af08-e282d3256ab9",
    "orderNumber": 5,
    "title": "Expense to cancel",
    "description": "Lawyer cancellation workflow.",
    "amount": 90,
    "dueDate": null,
    "status": 0,
    "fundingStatus": 0,
    "escrowHoldId": null,
    "fundedAt": null,
    "submittedAt": null,
    "autoAcceptEligibleAt": null,
    "holdExpiresAt": null,
    "netLawyerAmount": null,
    "version": "\"AAAAAAAAK9Q=\"",
    "type": 1,
    "permittedActions": [
      "Update",
      "Cancel"
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 201
}
```
---


- [PASS] **Create Expense cancellation candidate** (status=201)
### Lawyer cancels proposed Expense

**Request:** POST http://localhost:5051/api/milestones/577ed42e-3f49-49ab-af08-e282d3256ab9/cancel

**Body:**
```json
{
  "Reason": "Charge was reversed."
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "entityId": "577ed42e-3f49-49ab-af08-e282d3256ab9",
    "status": "Cancelled",
    "occurredAt": "2026-08-13T18:42:42.1079726Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Lawyer cancels proposed Expense** (status=200)

## Real Stripe Expense funding and instant release

### Read wallet before Expense funding

**Request:** GET http://localhost:5051/api/wallet

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "lawyerUserId": "57e9e9d6-09f1-487d-67e5-08def964d49c",
    "currency": "EGP",
    "pendingBalance": 0.0,
    "availableBalance": 427.5,
    "totalReleased": 427.5
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Read wallet before Expense funding** (status=200)
### Fund Expense with real Stripe test card

**Request:** POST http://localhost:5051/api/milestones/497c84d9-f187-49fc-8e3d-006d083a828b/fund

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
    "paymentTransactionId": "b70420db-4774-44c6-ac77-d8cfe719f5fd",
    "milestoneId": "497c84d9-f187-49fc-8e3d-006d083a828b",
    "status": "Succeeded",
    "clientActionType": null,
    "clientSecret": null,
    "redirectUrl": null,
    "payment": {
      "id": "2d0c65f7-774c-4eb6-94e1-ce245721cf16",
      "milestoneId": "497c84d9-f187-49fc-8e3d-006d083a828b",
      "grossAmount": 450.0,
      "platformFee": 22.5,
      "netAmount": 427.5,
      "currency": "EGP",
      "status": 0,
      "holdExpiresAt": null,
      "settledAt": null
    },
    "occurredAt": "2026-08-13T18:42:46.1342476Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Fund Expense with real Stripe test card** (status=200)
- [PASS] **Real Expense funding creates a payment transaction**
### Repeat Expense funding idempotently

**Request:** POST http://localhost:5051/api/milestones/497c84d9-f187-49fc-8e3d-006d083a828b/fund

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
    "paymentTransactionId": "b70420db-4774-44c6-ac77-d8cfe719f5fd",
    "milestoneId": "497c84d9-f187-49fc-8e3d-006d083a828b",
    "status": "Succeeded",
    "clientActionType": null,
    "clientSecret": null,
    "redirectUrl": null,
    "payment": {
      "id": "2d0c65f7-774c-4eb6-94e1-ce245721cf16",
      "milestoneId": "497c84d9-f187-49fc-8e3d-006d083a828b",
      "grossAmount": 450.0,
      "platformFee": 22.5,
      "netAmount": 427.5,
      "currency": "EGP",
      "status": 0,
      "holdExpiresAt": null,
      "settledAt": null
    },
    "occurredAt": "2026-08-13T18:42:53.835661Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Repeat Expense funding idempotently** (status=200)
- [PASS] **Repeated funding returns the same transaction**
### Poll expense release status

**Request:** GET http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "177792ae-8a92-4483-a555-d80eb0787db1",
      "orderNumber": 1,
      "title": "Standard written deliverable",
      "description": "Prepare and submit the written legal deliverable.",
      "deliverables": [
        "Written legal memorandum"
      ],
      "amount": 1100.0,
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
      "version": "\"AAAAAAAAK6g=\"",
      "type": 0,
      "permittedActions": []
    },
    {
      "id": "8a6f4fe3-74ae-4208-8736-37df38f48982",
      "orderNumber": 2,
      "title": "Updated filing fee expense",
      "description": "Updated court filing fee receipt.",
      "amount": 300.0,
      "dueDate": null,
      "status": 1,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK6M=\"",
      "type": 1,
      "permittedActions": [
        "Fund"
      ]
    },
    {
      "id": "497c84d9-f187-49fc-8e3d-006d083a828b",
      "orderNumber": 3,
      "title": "Mid-contract courier expense",
      "description": "Urgent legal document courier reimbursement.",
      "amount": 450.0,
      "dueDate": null,
      "status": 10,
      "fundingStatus": 2,
      "escrowHoldId": "2d0c65f7-774c-4eb6-94e1-ce245721cf16",
      "fundedAt": "2026-08-13T18:42:46.1342476",
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": 427.5,
      "version": "\"AAAAAAAAK+o=\"",
      "type": 1,
      "permittedActions": []
    },
    {
      "id": "b43a594c-8839-4bc5-a907-1d08bdc2ac67",
      "orderNumber": 4,
      "title": "Expense to reject",
      "description": "Client rejection workflow.",
      "amount": 80.0,
      "dueDate": null,
      "status": 9,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK9M=\"",
      "type": 1,
      "permittedActions": []
    },
    {
      "id": "577ed42e-3f49-49ab-af08-e282d3256ab9",
      "orderNumber": 5,
      "title": "Expense to cancel",
      "description": "Lawyer cancellation workflow.",
      "amount": 90.0,
      "dueDate": null,
      "status": 9,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK9Y=\"",
      "type": 1,
      "permittedActions": []
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Poll expense release status** (status=200)
### Poll expense release status

**Request:** GET http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "177792ae-8a92-4483-a555-d80eb0787db1",
      "orderNumber": 1,
      "title": "Standard written deliverable",
      "description": "Prepare and submit the written legal deliverable.",
      "deliverables": [
        "Written legal memorandum"
      ],
      "amount": 1100.0,
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
      "version": "\"AAAAAAAAK6g=\"",
      "type": 0,
      "permittedActions": []
    },
    {
      "id": "8a6f4fe3-74ae-4208-8736-37df38f48982",
      "orderNumber": 2,
      "title": "Updated filing fee expense",
      "description": "Updated court filing fee receipt.",
      "amount": 300.0,
      "dueDate": null,
      "status": 1,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK6M=\"",
      "type": 1,
      "permittedActions": [
        "Fund"
      ]
    },
    {
      "id": "497c84d9-f187-49fc-8e3d-006d083a828b",
      "orderNumber": 3,
      "title": "Mid-contract courier expense",
      "description": "Urgent legal document courier reimbursement.",
      "amount": 450.0,
      "dueDate": null,
      "status": 10,
      "fundingStatus": 2,
      "escrowHoldId": "2d0c65f7-774c-4eb6-94e1-ce245721cf16",
      "fundedAt": "2026-08-13T18:42:46.1342476",
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": 427.5,
      "version": "\"AAAAAAAAK+o=\"",
      "type": 1,
      "permittedActions": []
    },
    {
      "id": "b43a594c-8839-4bc5-a907-1d08bdc2ac67",
      "orderNumber": 4,
      "title": "Expense to reject",
      "description": "Client rejection workflow.",
      "amount": 80.0,
      "dueDate": null,
      "status": 9,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK9M=\"",
      "type": 1,
      "permittedActions": []
    },
    {
      "id": "577ed42e-3f49-49ab-af08-e282d3256ab9",
      "orderNumber": 5,
      "title": "Expense to cancel",
      "description": "Lawyer cancellation workflow.",
      "amount": 90.0,
      "dueDate": null,
      "status": 9,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK9Y=\"",
      "type": 1,
      "permittedActions": []
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Poll expense release status** (status=200)
### Poll expense release status

**Request:** GET http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "177792ae-8a92-4483-a555-d80eb0787db1",
      "orderNumber": 1,
      "title": "Standard written deliverable",
      "description": "Prepare and submit the written legal deliverable.",
      "deliverables": [
        "Written legal memorandum"
      ],
      "amount": 1100.0,
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
      "version": "\"AAAAAAAAK6g=\"",
      "type": 0,
      "permittedActions": []
    },
    {
      "id": "8a6f4fe3-74ae-4208-8736-37df38f48982",
      "orderNumber": 2,
      "title": "Updated filing fee expense",
      "description": "Updated court filing fee receipt.",
      "amount": 300.0,
      "dueDate": null,
      "status": 1,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK6M=\"",
      "type": 1,
      "permittedActions": [
        "Fund"
      ]
    },
    {
      "id": "497c84d9-f187-49fc-8e3d-006d083a828b",
      "orderNumber": 3,
      "title": "Mid-contract courier expense",
      "description": "Urgent legal document courier reimbursement.",
      "amount": 450.0,
      "dueDate": null,
      "status": 10,
      "fundingStatus": 2,
      "escrowHoldId": "2d0c65f7-774c-4eb6-94e1-ce245721cf16",
      "fundedAt": "2026-08-13T18:42:46.1342476",
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": 427.5,
      "version": "\"AAAAAAAAK+o=\"",
      "type": 1,
      "permittedActions": []
    },
    {
      "id": "b43a594c-8839-4bc5-a907-1d08bdc2ac67",
      "orderNumber": 4,
      "title": "Expense to reject",
      "description": "Client rejection workflow.",
      "amount": 80.0,
      "dueDate": null,
      "status": 9,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK9M=\"",
      "type": 1,
      "permittedActions": []
    },
    {
      "id": "577ed42e-3f49-49ab-af08-e282d3256ab9",
      "orderNumber": 5,
      "title": "Expense to cancel",
      "description": "Lawyer cancellation workflow.",
      "amount": 90.0,
      "dueDate": null,
      "status": 9,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK9Y=\"",
      "type": 1,
      "permittedActions": []
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Poll expense release status** (status=200)
### Poll expense release status

**Request:** GET http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "177792ae-8a92-4483-a555-d80eb0787db1",
      "orderNumber": 1,
      "title": "Standard written deliverable",
      "description": "Prepare and submit the written legal deliverable.",
      "deliverables": [
        "Written legal memorandum"
      ],
      "amount": 1100.0,
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
      "version": "\"AAAAAAAAK6g=\"",
      "type": 0,
      "permittedActions": []
    },
    {
      "id": "8a6f4fe3-74ae-4208-8736-37df38f48982",
      "orderNumber": 2,
      "title": "Updated filing fee expense",
      "description": "Updated court filing fee receipt.",
      "amount": 300.0,
      "dueDate": null,
      "status": 1,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK6M=\"",
      "type": 1,
      "permittedActions": [
        "Fund"
      ]
    },
    {
      "id": "497c84d9-f187-49fc-8e3d-006d083a828b",
      "orderNumber": 3,
      "title": "Mid-contract courier expense",
      "description": "Urgent legal document courier reimbursement.",
      "amount": 450.0,
      "dueDate": null,
      "status": 10,
      "fundingStatus": 2,
      "escrowHoldId": "2d0c65f7-774c-4eb6-94e1-ce245721cf16",
      "fundedAt": "2026-08-13T18:42:46.1342476",
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": 427.5,
      "version": "\"AAAAAAAAK+o=\"",
      "type": 1,
      "permittedActions": []
    },
    {
      "id": "b43a594c-8839-4bc5-a907-1d08bdc2ac67",
      "orderNumber": 4,
      "title": "Expense to reject",
      "description": "Client rejection workflow.",
      "amount": 80.0,
      "dueDate": null,
      "status": 9,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK9M=\"",
      "type": 1,
      "permittedActions": []
    },
    {
      "id": "577ed42e-3f49-49ab-af08-e282d3256ab9",
      "orderNumber": 5,
      "title": "Expense to cancel",
      "description": "Lawyer cancellation workflow.",
      "amount": 90.0,
      "dueDate": null,
      "status": 9,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK9Y=\"",
      "type": 1,
      "permittedActions": []
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Poll expense release status** (status=200)
### Poll expense release status

**Request:** GET http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "177792ae-8a92-4483-a555-d80eb0787db1",
      "orderNumber": 1,
      "title": "Standard written deliverable",
      "description": "Prepare and submit the written legal deliverable.",
      "deliverables": [
        "Written legal memorandum"
      ],
      "amount": 1100.0,
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
      "version": "\"AAAAAAAAK6g=\"",
      "type": 0,
      "permittedActions": []
    },
    {
      "id": "8a6f4fe3-74ae-4208-8736-37df38f48982",
      "orderNumber": 2,
      "title": "Updated filing fee expense",
      "description": "Updated court filing fee receipt.",
      "amount": 300.0,
      "dueDate": null,
      "status": 1,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK6M=\"",
      "type": 1,
      "permittedActions": [
        "Fund"
      ]
    },
    {
      "id": "497c84d9-f187-49fc-8e3d-006d083a828b",
      "orderNumber": 3,
      "title": "Mid-contract courier expense",
      "description": "Urgent legal document courier reimbursement.",
      "amount": 450.0,
      "dueDate": null,
      "status": 10,
      "fundingStatus": 2,
      "escrowHoldId": "2d0c65f7-774c-4eb6-94e1-ce245721cf16",
      "fundedAt": "2026-08-13T18:42:46.1342476",
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": 427.5,
      "version": "\"AAAAAAAAK+o=\"",
      "type": 1,
      "permittedActions": []
    },
    {
      "id": "b43a594c-8839-4bc5-a907-1d08bdc2ac67",
      "orderNumber": 4,
      "title": "Expense to reject",
      "description": "Client rejection workflow.",
      "amount": 80.0,
      "dueDate": null,
      "status": 9,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK9M=\"",
      "type": 1,
      "permittedActions": []
    },
    {
      "id": "577ed42e-3f49-49ab-af08-e282d3256ab9",
      "orderNumber": 5,
      "title": "Expense to cancel",
      "description": "Lawyer cancellation workflow.",
      "amount": 90.0,
      "dueDate": null,
      "status": 9,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK9Y=\"",
      "type": 1,
      "permittedActions": []
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Poll expense release status** (status=200)
### Poll expense release status

**Request:** GET http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "177792ae-8a92-4483-a555-d80eb0787db1",
      "orderNumber": 1,
      "title": "Standard written deliverable",
      "description": "Prepare and submit the written legal deliverable.",
      "deliverables": [
        "Written legal memorandum"
      ],
      "amount": 1100.0,
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
      "version": "\"AAAAAAAAK6g=\"",
      "type": 0,
      "permittedActions": []
    },
    {
      "id": "8a6f4fe3-74ae-4208-8736-37df38f48982",
      "orderNumber": 2,
      "title": "Updated filing fee expense",
      "description": "Updated court filing fee receipt.",
      "amount": 300.0,
      "dueDate": null,
      "status": 1,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK6M=\"",
      "type": 1,
      "permittedActions": [
        "Fund"
      ]
    },
    {
      "id": "497c84d9-f187-49fc-8e3d-006d083a828b",
      "orderNumber": 3,
      "title": "Mid-contract courier expense",
      "description": "Urgent legal document courier reimbursement.",
      "amount": 450.0,
      "dueDate": null,
      "status": 10,
      "fundingStatus": 2,
      "escrowHoldId": "2d0c65f7-774c-4eb6-94e1-ce245721cf16",
      "fundedAt": "2026-08-13T18:42:46.1342476",
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": 427.5,
      "version": "\"AAAAAAAAK+o=\"",
      "type": 1,
      "permittedActions": []
    },
    {
      "id": "b43a594c-8839-4bc5-a907-1d08bdc2ac67",
      "orderNumber": 4,
      "title": "Expense to reject",
      "description": "Client rejection workflow.",
      "amount": 80.0,
      "dueDate": null,
      "status": 9,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK9M=\"",
      "type": 1,
      "permittedActions": []
    },
    {
      "id": "577ed42e-3f49-49ab-af08-e282d3256ab9",
      "orderNumber": 5,
      "title": "Expense to cancel",
      "description": "Lawyer cancellation workflow.",
      "amount": 90.0,
      "dueDate": null,
      "status": 9,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK9Y=\"",
      "type": 1,
      "permittedActions": []
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Poll expense release status** (status=200)
### Poll expense release status

**Request:** GET http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "177792ae-8a92-4483-a555-d80eb0787db1",
      "orderNumber": 1,
      "title": "Standard written deliverable",
      "description": "Prepare and submit the written legal deliverable.",
      "deliverables": [
        "Written legal memorandum"
      ],
      "amount": 1100.0,
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
      "version": "\"AAAAAAAAK6g=\"",
      "type": 0,
      "permittedActions": []
    },
    {
      "id": "8a6f4fe3-74ae-4208-8736-37df38f48982",
      "orderNumber": 2,
      "title": "Updated filing fee expense",
      "description": "Updated court filing fee receipt.",
      "amount": 300.0,
      "dueDate": null,
      "status": 1,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK6M=\"",
      "type": 1,
      "permittedActions": [
        "Fund"
      ]
    },
    {
      "id": "497c84d9-f187-49fc-8e3d-006d083a828b",
      "orderNumber": 3,
      "title": "Mid-contract courier expense",
      "description": "Urgent legal document courier reimbursement.",
      "amount": 450.0,
      "dueDate": null,
      "status": 7,
      "fundingStatus": 3,
      "escrowHoldId": "2d0c65f7-774c-4eb6-94e1-ce245721cf16",
      "fundedAt": "2026-08-13T18:42:46.1342476",
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": 427.5,
      "version": "\"AAAAAAAAK/o=\"",
      "type": 1,
      "permittedActions": []
    },
    {
      "id": "b43a594c-8839-4bc5-a907-1d08bdc2ac67",
      "orderNumber": 4,
      "title": "Expense to reject",
      "description": "Client rejection workflow.",
      "amount": 80.0,
      "dueDate": null,
      "status": 9,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK9M=\"",
      "type": 1,
      "permittedActions": []
    },
    {
      "id": "577ed42e-3f49-49ab-af08-e282d3256ab9",
      "orderNumber": 5,
      "title": "Expense to cancel",
      "description": "Lawyer cancellation workflow.",
      "amount": 90.0,
      "dueDate": null,
      "status": 9,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK9Y=\"",
      "type": 1,
      "permittedActions": []
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Poll expense release status** (status=200)
- [PASS] **Expense is released without Submission or Acceptance**
- [PASS] **Expense never receives SubmittedAt**
- [PASS] **Expense never receives a 14-day HoldExpiresAt**
### Read released Expense payment

**Request:** GET http://localhost:5051/api/milestones/497c84d9-f187-49fc-8e3d-006d083a828b/payment

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "2d0c65f7-774c-4eb6-94e1-ce245721cf16",
    "milestoneId": "497c84d9-f187-49fc-8e3d-006d083a828b",
    "grossAmount": 450.0,
    "platformFee": 22.5,
    "netAmount": 427.5,
    "currency": "EGP",
    "status": 2,
    "holdExpiresAt": null,
    "settledAt": "2026-08-13T18:42:48.3429172"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Read released Expense payment** (status=200)
- [PASS] **Expense escrow hold is Released**
### Read wallet after Expense release

**Request:** GET http://localhost:5051/api/wallet

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "lawyerUserId": "57e9e9d6-09f1-487d-67e5-08def964d49c",
    "currency": "EGP",
    "pendingBalance": 0.0,
    "availableBalance": 855.0,
    "totalReleased": 855.0
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Read wallet after Expense release** (status=200)
- [PASS] **Expense release increases lawyer available balance**

## Unchanged Standard funding, Submission, Acceptance, and hold

### Read Standard before ready-for-funding

**Request:** GET http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "177792ae-8a92-4483-a555-d80eb0787db1",
      "orderNumber": 1,
      "title": "Standard written deliverable",
      "description": "Prepare and submit the written legal deliverable.",
      "deliverables": [
        "Written legal memorandum"
      ],
      "amount": 1100.0,
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
      "version": "\"AAAAAAAAK6g=\"",
      "type": 0,
      "permittedActions": [
        "ReadyForFunding"
      ]
    },
    {
      "id": "8a6f4fe3-74ae-4208-8736-37df38f48982",
      "orderNumber": 2,
      "title": "Updated filing fee expense",
      "description": "Updated court filing fee receipt.",
      "amount": 300.0,
      "dueDate": null,
      "status": 1,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK6M=\"",
      "type": 1,
      "permittedActions": []
    },
    {
      "id": "497c84d9-f187-49fc-8e3d-006d083a828b",
      "orderNumber": 3,
      "title": "Mid-contract courier expense",
      "description": "Urgent legal document courier reimbursement.",
      "amount": 450.0,
      "dueDate": null,
      "status": 7,
      "fundingStatus": 3,
      "escrowHoldId": "2d0c65f7-774c-4eb6-94e1-ce245721cf16",
      "fundedAt": "2026-08-13T18:42:46.1342476",
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": 427.5,
      "version": "\"AAAAAAAAK/o=\"",
      "type": 1,
      "permittedActions": []
    },
    {
      "id": "b43a594c-8839-4bc5-a907-1d08bdc2ac67",
      "orderNumber": 4,
      "title": "Expense to reject",
      "description": "Client rejection workflow.",
      "amount": 80.0,
      "dueDate": null,
      "status": 9,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK9M=\"",
      "type": 1,
      "permittedActions": []
    },
    {
      "id": "577ed42e-3f49-49ab-af08-e282d3256ab9",
      "orderNumber": 5,
      "title": "Expense to cancel",
      "description": "Lawyer cancellation workflow.",
      "amount": 90.0,
      "dueDate": null,
      "status": 9,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK9Y=\"",
      "type": 1,
      "permittedActions": []
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Read Standard before ready-for-funding** (status=200)
### Lawyer marks Standard ready for funding

**Request:** POST http://localhost:5051/api/milestones/177792ae-8a92-4483-a555-d80eb0787db1/ready-for-funding

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
    "entityId": "177792ae-8a92-4483-a555-d80eb0787db1",
    "status": "AwaitingFunding",
    "occurredAt": "2026-08-13T18:43:06.3087464Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Lawyer marks Standard ready for funding** (status=200)
### Fund Standard with real Stripe test card

**Request:** POST http://localhost:5051/api/milestones/177792ae-8a92-4483-a555-d80eb0787db1/fund

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
    "paymentTransactionId": "d98f2d0f-7ab1-4395-8d80-366cbc307818",
    "milestoneId": "177792ae-8a92-4483-a555-d80eb0787db1",
    "status": "Succeeded",
    "clientActionType": null,
    "clientSecret": null,
    "redirectUrl": null,
    "payment": {
      "id": "2fee286c-3ff7-486b-82d8-09623790989c",
      "milestoneId": "177792ae-8a92-4483-a555-d80eb0787db1",
      "grossAmount": 1100.0,
      "platformFee": 55.0,
      "netAmount": 1045.0,
      "currency": "EGP",
      "status": 0,
      "holdExpiresAt": null,
      "settledAt": null
    },
    "occurredAt": "2026-08-13T18:43:13.791195Z"
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Fund Standard with real Stripe test card** (status=200)
### Read funded Standard milestone

**Request:** GET http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/milestones

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [
    {
      "id": "177792ae-8a92-4483-a555-d80eb0787db1",
      "orderNumber": 1,
      "title": "Standard written deliverable",
      "description": "Prepare and submit the written legal deliverable.",
      "deliverables": [
        "Written legal memorandum"
      ],
      "amount": 1100.0,
      "durationDays": 10,
      "dueDate": null,
      "status": 3,
      "fundingStatus": 2,
      "escrowHoldId": "2fee286c-3ff7-486b-82d8-09623790989c",
      "fundedAt": "2026-08-13T18:43:13.791195",
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": 1045.0,
      "version": "\"AAAAAAAALBE=\"",
      "type": 0,
      "permittedActions": []
    },
    {
      "id": "8a6f4fe3-74ae-4208-8736-37df38f48982",
      "orderNumber": 2,
      "title": "Updated filing fee expense",
      "description": "Updated court filing fee receipt.",
      "amount": 300.0,
      "dueDate": null,
      "status": 1,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK6M=\"",
      "type": 1,
      "permittedActions": [
        "Fund"
      ]
    },
    {
      "id": "497c84d9-f187-49fc-8e3d-006d083a828b",
      "orderNumber": 3,
      "title": "Mid-contract courier expense",
      "description": "Urgent legal document courier reimbursement.",
      "amount": 450.0,
      "dueDate": null,
      "status": 7,
      "fundingStatus": 3,
      "escrowHoldId": "2d0c65f7-774c-4eb6-94e1-ce245721cf16",
      "fundedAt": "2026-08-13T18:42:46.1342476",
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": 427.5,
      "version": "\"AAAAAAAAK/o=\"",
      "type": 1,
      "permittedActions": []
    },
    {
      "id": "b43a594c-8839-4bc5-a907-1d08bdc2ac67",
      "orderNumber": 4,
      "title": "Expense to reject",
      "description": "Client rejection workflow.",
      "amount": 80.0,
      "dueDate": null,
      "status": 9,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK9M=\"",
      "type": 1,
      "permittedActions": []
    },
    {
      "id": "577ed42e-3f49-49ab-af08-e282d3256ab9",
      "orderNumber": 5,
      "title": "Expense to cancel",
      "description": "Lawyer cancellation workflow.",
      "amount": 90.0,
      "dueDate": null,
      "status": 9,
      "fundingStatus": 0,
      "escrowHoldId": null,
      "fundedAt": null,
      "submittedAt": null,
      "autoAcceptEligibleAt": null,
      "holdExpiresAt": null,
      "netLawyerAmount": null,
      "version": "\"AAAAAAAAK9Y=\"",
      "type": 1,
      "permittedActions": []
    }
  ],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Read funded Standard milestone** (status=200)
- [PASS] **Standard remains FundedInProgress after funding**
- [PASS] **Standard is not instantly released**
- [PASS] **Create lawyer-owned submission file fixture**
### Lawyer submits Standard deliverable

**Request:** POST http://localhost:5051/api/milestones/177792ae-8a92-4483-a555-d80eb0787db1/submit

**Body:**
```json
{
  "StoredFileIds": [
    "0933c69d-b698-4127-845a-4c860cfd3e40"
  ],
  "Notes": "Completed written legal memorandum."
}
```

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "177792ae-8a92-4483-a555-d80eb0787db1",
    "orderNumber": 1,
    "title": "Standard written deliverable",
    "description": "Prepare and submit the written legal deliverable.",
    "deliverables": [
      "Written legal memorandum"
    ],
    "amount": 1100.0,
    "durationDays": 10,
    "dueDate": null,
    "status": 4,
    "fundingStatus": 2,
    "escrowHoldId": "2fee286c-3ff7-486b-82d8-09623790989c",
    "fundedAt": "2026-08-13T18:43:13.791195",
    "submittedAt": "2026-08-13T18:43:14.1799028Z",
    "autoAcceptEligibleAt": "2026-08-20T18:43:14.1799028Z",
    "holdExpiresAt": null,
    "netLawyerAmount": 1045.0,
    "version": "\"AAAAAAAALBY=\"",
    "type": 0,
    "permittedActions": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Lawyer submits Standard deliverable** (status=200)
- [PASS] **Standard enters Submitted**
### Client accepts Standard deliverable

**Request:** POST http://localhost:5051/api/milestones/177792ae-8a92-4483-a555-d80eb0787db1/accept

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
    "id": "177792ae-8a92-4483-a555-d80eb0787db1",
    "orderNumber": 1,
    "title": "Standard written deliverable",
    "description": "Prepare and submit the written legal deliverable.",
    "deliverables": [
      "Written legal memorandum"
    ],
    "amount": 1100.0,
    "durationDays": 10,
    "dueDate": null,
    "status": 5,
    "fundingStatus": 2,
    "escrowHoldId": "2fee286c-3ff7-486b-82d8-09623790989c",
    "fundedAt": "2026-08-13T18:43:13.791195",
    "submittedAt": "2026-08-13T18:43:14.1799028",
    "autoAcceptEligibleAt": null,
    "holdExpiresAt": "2026-08-27T18:43:14.2533686Z",
    "netLawyerAmount": 1045.0,
    "version": "\"AAAAAAAALBo=\"",
    "type": 0,
    "permittedActions": []
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Client accepts Standard deliverable** (status=200)
- [PASS] **Standard enters AcceptedHold**
- [PASS] **Standard retains approximately 14-day hold** (days=14)
### Read Standard escrow hold

**Request:** GET http://localhost:5051/api/milestones/177792ae-8a92-4483-a555-d80eb0787db1/payment

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "id": "2fee286c-3ff7-486b-82d8-09623790989c",
    "milestoneId": "177792ae-8a92-4483-a555-d80eb0787db1",
    "grossAmount": 1100.0,
    "platformFee": 55.0,
    "netAmount": 1045.0,
    "currency": "EGP",
    "status": 0,
    "holdExpiresAt": "2026-08-27T18:43:14.2533686",
    "settledAt": null
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Read Standard escrow hold** (status=200)
- [PASS] **Standard payment remains held, not released**
- [PASS] **Standard payment exposes the same hold expiry**
### Read complete contract payment history

**Request:** GET http://localhost:5051/api/contracts/ed1ebc46-c99b-4c36-83ad-c8143edb2ade/payments

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": {
    "payments": [
      {
        "id": "2d0c65f7-774c-4eb6-94e1-ce245721cf16",
        "milestoneId": "497c84d9-f187-49fc-8e3d-006d083a828b",
        "grossAmount": 450.0,
        "platformFee": 22.5,
        "netAmount": 427.5,
        "currency": "EGP",
        "status": 2,
        "holdExpiresAt": null,
        "settledAt": "2026-08-13T18:42:48.3429172"
      },
      {
        "id": "2fee286c-3ff7-486b-82d8-09623790989c",
        "milestoneId": "177792ae-8a92-4483-a555-d80eb0787db1",
        "grossAmount": 1100.0,
        "platformFee": 55.0,
        "netAmount": 1045.0,
        "currency": "EGP",
        "status": 0,
        "holdExpiresAt": "2026-08-27T18:43:14.2533686",
        "settledAt": null
      }
    ],
    "attempts": [
      {
        "id": "d98f2d0f-7ab1-4395-8d80-366cbc307818",
        "milestoneId": "177792ae-8a92-4483-a555-d80eb0787db1",
        "operationType": 0,
        "status": 1,
        "amount": 1100.0,
        "currency": "EGP",
        "providerName": "StripePaymentProvider",
        "providerAttemptCount": 0,
        "nextRetryAt": null,
        "requiresManualAction": false,
        "manualActionRequiredAt": null,
        "createdAt": "2026-08-13T18:43:07.0721259",
        "processedAt": "2026-08-13T18:43:13.791195"
      },
      {
        "id": "1741ece8-8e61-487c-ac79-e5579c9e9c18",
        "milestoneId": "497c84d9-f187-49fc-8e3d-006d083a828b",
        "operationType": 1,
        "status": 1,
        "amount": 427.5,
        "currency": "EGP",
        "providerName": "StripePaymentProvider",
        "providerAttemptCount": 1,
        "nextRetryAt": null,
        "requiresManualAction": false,
        "manualActionRequiredAt": null,
        "createdAt": "2026-08-13T18:42:48.3429172",
        "processedAt": "2026-08-13T18:42:48.3429172"
      },
      {
        "id": "b70420db-4774-44c6-ac77-d8cfe719f5fd",
        "milestoneId": "497c84d9-f187-49fc-8e3d-006d083a828b",
        "operationType": 0,
        "status": 1,
        "amount": 450.0,
        "currency": "EGP",
        "providerName": "StripePaymentProvider",
        "providerAttemptCount": 0,
        "nextRetryAt": null,
        "requiresManualAction": false,
        "manualActionRequiredAt": null,
        "createdAt": "2026-08-13T18:42:43.4296366",
        "processedAt": "2026-08-13T18:42:46.1342476"
      }
    ],
    "ledgerEntries": [
      {
        "id": "8dda513d-1bb7-4a8b-802a-81dbf573b2f3",
        "escrowHoldId": "2d0c65f7-774c-4eb6-94e1-ce245721cf16",
        "transactionType": 0,
        "amount": 450.0,
        "runningBalance": 450.0,
        "currency": "EGP",
        "description": "إيداع تمويل المرحلة في حساب الضمان.",
        "createdAt": "2026-08-13T18:42:46.1342476"
      },
      {
        "id": "8397daea-01d2-4f66-84a8-16f8a313f661",
        "escrowHoldId": "2d0c65f7-774c-4eb6-94e1-ce245721cf16",
        "transactionType": 1,
        "amount": 427.5,
        "runningBalance": 22.5,
        "currency": "EGP",
        "description": "تحرير صافي مستحقات المحامي من حجز ضمان المرحلة.",
        "createdAt": "2026-08-13T18:42:48.3429172"
      },
      {
        "id": "8391bd64-46de-447c-8462-719ae8ccf5b5",
        "escrowHoldId": "2d0c65f7-774c-4eb6-94e1-ce245721cf16",
        "transactionType": 3,
        "amount": 22.5,
        "runningBalance": 0.0,
        "currency": "EGP",
        "description": "تسجيل رسوم المنصة المستحقة عن المرحلة.",
        "createdAt": "2026-08-13T18:42:48.3429172"
      },
      {
        "id": "2ce190c7-3e31-48e8-a621-1cc89da5c04f",
        "escrowHoldId": "2fee286c-3ff7-486b-82d8-09623790989c",
        "transactionType": 0,
        "amount": 1100.0,
        "runningBalance": 1100.0,
        "currency": "EGP",
        "description": "إيداع تمويل المرحلة في حساب الضمان.",
        "createdAt": "2026-08-13T18:43:13.791195"
      }
    ]
  },
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Read complete contract payment history** (status=200)
- [PASS] **Payment history records Stripe provider attempts**
- [PASS] **Both Standard and Expense escrow holds are recorded**
### Remove Stripe test payment method

**Request:** DELETE http://localhost:5051/api/payment-methods/pm_1U43ZO09HdpVdJvPcEuKprO6

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": "Payment method removed.",
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **Remove Stripe test payment method** (status=200)
### List payment methods after removal

**Request:** GET http://localhost:5051/api/payment-methods

**Response Status:** 200

**Response Body:**
```json
{
  "success": true,
  "data": [],
  "message": null,
  "errors": null,
  "statusCode": 200
}
```
---


- [PASS] **List payment methods after removal** (status=200)
- [PASS] **Removed Stripe payment method is no longer listed**

## Summary

- Passed assertions: 109
- Failed assertions: 0

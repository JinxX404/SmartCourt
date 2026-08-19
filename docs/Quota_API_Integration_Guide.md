# Quota API Contract and Frontend Integration Guide

**Code snapshot analyzed:** 2026-08-19  
**Primary route prefixes:** `/api/agent/quota`, `/api/admin/quotas`  
**Audience:** Web/mobile frontend developers integrating the Quota (AI usage limits and credits) feature

> This guide describes the implementation in source code, including its current inconsistencies. It does not substitute intended product behavior for actual wire behavior.

## Wire-level conventions

| Concern | Actual behavior |
|---|---|
| Authentication | Protected endpoints require `Authorization: Bearer <JWT>` or use the application's `accessToken` HttpOnly cookie. The cookie wins if both are present because the JWT handler explicitly reads it. `GET /api/agent/quota/default` is Public. |
| Content type | Send `Content-Type: application/json` for endpoints with a body. Success and middleware-handled errors are JSON. |
| JSON naming | Response and request examples use `camelCase`. ASP.NET Core binding is case-insensitive, but frontend code should use the documented casing. |
| Nulls | Null response properties are not globally suppressed. Envelopes therefore normally include `message: null`, `errors: null`, and failed envelopes include `data: null`. |
| Error codes | There is **no machine-readable application error-code field**. HTTP status plus localized `message`/`errors` is the only implemented discriminator. Frontend logic must not depend on the Arabic prose when an HTTP status or current resource state can be used. |

### Standard success envelope

```json
{
  "success": true,
  "data": {},
  "message": null,
  "errors": null,
  "statusCode": 200
}
```

### Insufficient Quota Error Envelope (429 Too Many Requests)

When a client consumes AI tokens and exhausts their daily limits and purchased credits, the API returns a structured HTTP `429 Too Many Requests`. This structure must be used by the frontend to display an "Upgrade Plan" or "Limit Reached" prompt.

```json
{
  "success": false,
  "data": {
    "dailyLimitCredits": 10.0,
    "consumedCredits": 10.0,
    "remainingCredits": 0.0,
    "requestedCredits": 1.5,
    "nextResetAt": "2026-08-20T00:00:00+00:00"
  },
  "message": "عفواً، رصيدك الحالي لا يكفي. يرجى شحن رصيدك أو المحاولة لاحقاً.",
  "errors": null,
  "statusCode": 429
}
```

---

## 1. Complete Endpoint Catalog

### Endpoint overview

| Method | Exact route | Allowed controller roles | Success | Request body |
|---|---|---|---:|---|
| `GET` | `/api/agent/quota/default` | Public (Anonymous allowed) | `200` | None |
| `GET` | `/api/agent/quota` | Client | `200` | None |
| `GET` | `/api/agent/quota/history` | Client | `200` | None |
| `GET` | `/api/agent/quota/transactions` | Client | `200` | None |
| `GET` | `/api/lawyer/subscription` | Lawyer | `200` | None |
| `GET` | `/api/lawyer/subscription/plans` | Lawyer | `200` | None |
| `GET` | `/api/lawyer/subscription/bundles` | Lawyer | `200` | None |
| `POST` | `/api/lawyer/subscription/change-plan` | Lawyer | `200` | `LawyerChangePlanRequest` |
| `POST` | `/api/lawyer/subscription/buy-bundle` | Lawyer | `200` | `LawyerPurchaseBundleRequest` |
| `GET` | `/api/admin/quotas/default-limit` | Admin | `200` | None |
| `PUT` | `/api/admin/quotas/default-limit` | Admin | `200` | `UpdateDailyLimitRequest` |
| `GET` | `/api/admin/quotas/clients` | Admin | `200` | None |
| `GET` | `/api/admin/quotas/clients/{clientId}` | Admin | `200` | None |
| `PUT` | `/api/admin/quotas/clients/{clientId}/limit` | Admin | `200` | `UpdateDailyLimitRequest` |
| `POST` | `/api/admin/quotas/clients/{clientId}/adjust` | Admin | `200` | `AdjustQuotaRequest` |
| `GET` | `/api/admin/quotas/clients/{clientId}/transactions` | Admin | `200` | None |
| `GET` | `/api/admin/quotas/purchases` | Admin | `200` | None |
| `GET` | `/api/admin/lawyer-subscriptions` | Admin | `200` | None |
| `GET` | `/api/admin/lawyer-subscriptions/{lawyerId}` | Admin | `200` | None |
| `POST` | `/api/admin/lawyer-subscriptions/{lawyerId}/adjust` | Admin | `200` | `AdjustQuotaRequest` |
| `POST` | `/api/admin/lawyer-subscriptions/{lawyerId}/change-plan` | Admin | `200` | `AdminChangeLawyerPlanRequest` |

---

### 1.1 Client Endpoints

#### View Global Default Quota Limit
**HTTP Method & Exact Route:** `GET /api/agent/quota/default`  
**Purpose:** To retrieve the default daily limit granted to new users. This endpoint is public and can be used on marketing/pricing pages before the user signs up.  
**Response (`200 OK`):** `data` contains:
```json
{
  "dailyCreditLimit": 10.0
}
```

#### View Current User Quota
**HTTP Method & Exact Route:** `GET /api/agent/quota`  
**Purpose:** To display the active client's available credits, usage, and next reset time. This is used in the main chat UI to warn users before they run out.  
**Authentication:** Required (Client).  
**Response (`200 OK`):** `data` contains:
```json
{
  "dailyLimitCredits": 10.0,
  "consumedDailyCredits": 8.0,
  "remainingDailyCredits": 2.0,
  "availableAdditionalCredits": 50.0,
  "totalRemainingCredits": 52.0,
  "nextResetAt": "2026-08-20T00:00:00+00:00"
}
```

#### View Quota Usage History
**HTTP Method & Exact Route:** `GET /api/agent/quota/history`  
**Purpose:** To display a chart of the client's token usage over the past X days.  
**Query Parameters:** `days` (Default 7, max 30).  
**Authentication:** Required (Client).  
**Response (`200 OK`):** `data` is an array of `DailyQuotaUsageDto`.

#### View Quota Transactions
**HTTP Method & Exact Route:** `GET /api/agent/quota/transactions`  
**Purpose:** To list a client's historical token ledger transactions (e.g. deductions, refunds, or manual additions).  
**Query Parameters:** `page`, `pageSize`.  
**Authentication:** Required (Client).  
**Response (`200 OK`):** `data` is a paginated list of `QuotaTransactionDto`.

---

### 1.2 Admin Moderation & Management Endpoints

#### Get All Clients Quota Summary
**HTTP Method & Exact Route:** `GET /api/admin/quotas/clients`  
**Purpose:** To display a paginated dashboard of all clients, showing their daily limits, daily consumption, and any additional purchased balance.  
**Query Parameters:** `search` (name or email), `isExhausted` (boolean filter), `hasAdditionalBalance` (boolean filter), `page`, `pageSize`.  
**Authentication:** Required (Admin).  
**Response (`200 OK`):** `data` is a paginated list of `AdminQuotaClientSummaryDto`.

#### View Client Quota Detail
**HTTP Method & Exact Route:** `GET /api/admin/quotas/clients/{clientId}`  
**Purpose:** Same response as `GET /api/agent/quota`, but requested by an Admin for a specific user.  
**Authentication:** Required (Admin).  

#### View Client Transactions
**HTTP Method & Exact Route:** `GET /api/admin/quotas/clients/{clientId}/transactions`  
**Purpose:** Same as the client's own view, but retrieved by an Admin.  
**Query Parameters:** `page`, `pageSize`.  
**Authentication:** Required (Admin).  

#### Set Client Custom Daily Limit
**HTTP Method & Exact Route:** `PUT /api/admin/quotas/clients/{clientId}/limit`  
**Purpose:** Overrides the global default limit for a specific client (e.g. VIP clients).  
**Request Body:** `application/json`
```json
{
  "DailyCreditLimit": 500.0
}
```
**Authentication:** Required (Admin).  
**Response (`200 OK`):** `message` string confirming success.

#### Adjust Client Balance Manually
**HTTP Method & Exact Route:** `POST /api/admin/quotas/clients/{clientId}/adjust`  
**Purpose:** To manually grant or deduct paid credits from a user's wallet (e.g., compensation, bonuses, manual refunds).  
**Request Body:** `application/json`
```json
{
  "CreditAmount": 100.0,
  "Reason": "Compensation for outage"
}
```
**Authentication:** Required (Admin).  
**Response (`200 OK`):** `message` string confirming success.

#### Get Global Default Limit
**HTTP Method & Exact Route:** `GET /api/admin/quotas/default-limit`  
**Purpose:** To show admins the current baseline default limit assigned to newly registered users.  
**Authentication:** Required (Admin).  
**Response (`200 OK`):** `data` contains `dailyCreditLimit`.

#### Set Global Default Limit
**HTTP Method & Exact Route:** `PUT /api/admin/quotas/default-limit`  
**Purpose:** To update the global baseline quota given to all users who do not have a custom profile limit set.  
**Request Body:** `application/json`
```json
{
  "DailyCreditLimit": 15.0
}
```
**Authentication:** Required (Admin).  
**Response (`200 OK`):** `message` string confirming success.

#### View All Token Bundle Purchases
**HTTP Method & Exact Route:** `GET /api/admin/quotas/purchases`  
**Purpose:** To audit all real-money purchases of Token Bundles made by users platform-wide.  
**Query Parameters:** `page`, `pageSize`.  
**Authentication:** Required (Admin).  
**Response (`200 OK`):** `data` is a paginated list of `TokenBundlePurchaseDto`.

---

### 1.3 Lawyer Subscription & Quota Endpoints

#### View Current Subscription and Quota
**HTTP Method & Exact Route:** `GET /api/lawyer/subscription`  
**Purpose:** To retrieve the lawyer's current subscription plan, token limits, and additional token balance.
**Authentication:** Required (Lawyer).  
**Response (`200 OK`):** `data` contains:
```json
{
  "dailyCreditLimit": 10.0,
  "consumedDailyCredits": 0.0,
  "remainingDailyCredits": 10.0,
  "availableAdditionalCredits": 0.0,
  "totalRemainingCredits": 10.0,
  "planName": "Free",
  "nextResetAt": "2026-08-20T00:00:00+00:00"
}
```

#### Get Available Plans
**HTTP Method & Exact Route:** `GET /api/lawyer/subscription/plans`  
**Purpose:** Returns the list of subscription plans (e.g., Free, Professional, Business) available for lawyers, with their pricing and limits.
**Authentication:** Required (Lawyer).  

#### Get Available Bundles
**HTTP Method & Exact Route:** `GET /api/lawyer/subscription/bundles`  
**Purpose:** Returns the list of token bundles available for purchase by the lawyer (shared with client bundles).
**Authentication:** Required (Lawyer).  

#### Change Subscription Plan
**HTTP Method & Exact Route:** `POST /api/lawyer/subscription/change-plan`  
**Purpose:** Upgrades or downgrades the lawyer's subscription plan. Generates a Stripe checkout session if the plan is paid.
**Authentication:** Required (Lawyer).  
**Request Body:** `application/json`
```json
{
  "NewPlan": "Professional",
  "ConfirmationTokenReference": "token_abc123",
  "IdempotencyKey": "unique-uuid"
}
```

#### Purchase Token Bundle
**HTTP Method & Exact Route:** `POST /api/lawyer/subscription/buy-bundle`  
**Purpose:** Purchases an extra bundle of tokens. Generates a Stripe checkout session if payment is required.
**Authentication:** Required (Lawyer).  
**Request Body:** `application/json`
```json
{
  "BundleId": "bundle-uuid",
  "ConfirmationTokenReference": "token_abc123",
  "IdempotencyKey": "unique-uuid"
}
```

---

### 1.4 Admin Lawyer Management Endpoints

#### Get All Lawyers Quota Summary
**HTTP Method & Exact Route:** `GET /api/admin/lawyer-subscriptions`  
**Purpose:** To display a paginated dashboard of all lawyers, showing their plans, daily limits, and any additional purchased balance.  
**Query Parameters:** `search` (name or email), `page`, `pageSize`.  
**Authentication:** Required (Admin).  

#### View Lawyer Quota Detail
**HTTP Method & Exact Route:** `GET /api/admin/lawyer-subscriptions/{lawyerId}`  
**Purpose:** Same response as `GET /api/lawyer/subscription`, but requested by an Admin for a specific lawyer.  
**Authentication:** Required (Admin).  

#### Adjust Lawyer Balance Manually
**HTTP Method & Exact Route:** `POST /api/admin/lawyer-subscriptions/{lawyerId}/adjust`  
**Purpose:** To manually grant or deduct paid credits from a lawyer's wallet.  
**Request Body:** `application/json`
```json
{
  "CreditAmount": 100.0,
  "Reason": "Compensation for outage"
}
```
**Authentication:** Required (Admin).  

#### Force Change Lawyer Plan
**HTTP Method & Exact Route:** `POST /api/admin/lawyer-subscriptions/{lawyerId}/change-plan`  
**Purpose:** Admin override to forcefully change a lawyer's subscription plan without payment.  
**Request Body:** `application/json`
```json
{
  "NewPlan": "Business"
}
```
**Authentication:** Required (Admin).

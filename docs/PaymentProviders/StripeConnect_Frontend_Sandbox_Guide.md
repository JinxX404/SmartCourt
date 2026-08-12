# Stripe Connect browser sandbox handoff

> Audience: Smart Court frontend team and demo operator  
> Environment: Development + Stripe test sandbox only  
> Full API source of truth: [`Payments_API_Integration_Guide.md`](../Payments_API_Integration_Guide.md)

## 1. What the backend already does

The frontend must never implement money movement itself. The backend owns:

- Stripe test-only startup validation and safe publishable-key discovery.
- Client Stripe Customer creation and SetupIntents.
- Milestone amount/currency validation and PaymentIntent creation/confirmation.
- ConfirmationToken and saved-PaymentMethod payments, plus Client retry.
- Signed, deduplicated platform and Connect webhooks.
- Full/partial refunds triggered by Contract/Dispute settlement rules.
- Accounts v2 Lawyer recipient creation and hosted onboarding links.
- The real 14-day hold, separate Transfer, 5% fee, wallet, Payout, and reconciliation.
- Withdrawal history and payment safety/idempotency rules.

The browser owns screens, Stripe.js/Elements calls, API sequencing, redirects, polling/refetching, and safe presentation of masked/status data.

## 2. Frontend prerequisites

```bash
npm install @stripe/stripe-js @stripe/react-stripe-js
```

Use the existing same-origin Vite API proxy where possible. Every authenticated `fetch` must use `credentials: "include"` because login writes HttpOnly cookies. If using Bearer auth, send `Authorization: Bearer {accessToken}` consistently.

Never place `sk_test_...`, `whsec_...`, raw card data, or connected-account bank data in frontend source/config. The only key the browser receives is `pk_test_...` from `GET /api/payments/config`.

## 3. Existing Smart Court account prerequisites

Registration, email confirmation, profile completion, verification, and administrator approval work exactly as before; Payments does not bypass or modify them. Use normally created and activated Client/Lawyer accounts, then continue through Case → Proposal → Contract → multiple milestones → both-party approval → Lawyer marks the first milestone ready for funding.

## 4. Stripe bootstrap

Call `GET /api/payments/config` and require:

```ts
type PaymentProviderConfig = {
  providerCode: "StripeConnect";
  publishableKey: string; // pk_test_...
  currency: "EGP";
  sandboxOnly: true;
  confirmationTokensEnabled: true;
  savedPaymentMethodsEnabled: true;
};

const config = response.data;
if (
  config.providerCode !== "StripeConnect" ||
  !config.sandboxOnly ||
  !config.publishableKey.startsWith("pk_test_")
) throw new Error("Stripe sandbox is not configured.");

const stripePromise = loadStripe(config.publishableKey);
```

## 5. One-time milestone checkout

### 5.1 Mount deferred Payment Element

Get the amount from the existing milestone API. Convert EGP major units to minor units for Elements display only; the backend independently loads the authoritative amount.

```tsx
const options = {
  mode: "payment" as const,
  amount: Math.round(milestone.amount * 100),
  currency: "egp",
};

<Elements stripe={stripePromise} options={options}>
  <PaymentElement />
</Elements>
```

### 5.2 Tokenize and submit

```ts
const submit = await elements.submit();
if (submit.error) return showStripeError(submit.error);

const { error, confirmationToken } = await stripe.createConfirmationToken({
  elements,
  params: { return_url: `${window.location.origin}/payments/return` },
});
if (error) return showStripeError(error);
```

Create one UUID per logical Pay click. Reuse it after a timeout.

```http
POST /api/milestones/{milestoneId}/payment-session
Idempotency-Key: {uuid}
Content-Type: application/json

{ "confirmationTokenReference": "ctoken_..." }
```

- `200`, `Succeeded`, non-null `payment`: funded.
- `202`, `RequiresCustomerAction`: call `stripe.handleNextAction` below.
- `202`, `Processing`: show pending and poll; never create a replacement charge.
- `400`: show safe message and allow an intentional new attempt where applicable.
- `409`: stop duplicate submission and query payment state.
- `429`: honor `Retry-After`; retain the same idempotency key.

```ts
if (result.data.clientActionType === "ConfirmPayment" && result.data.clientSecret) {
  const { error } = await stripe.handleNextAction({
    clientSecret: result.data.clientSecret,
  });
  if (error) showStripeError(error);
}
```

Afterward, poll `GET /api/milestones/{milestoneId}/payment` or refetch `GET /api/contracts/{contractId}/payments`. Webhooks are authoritative.

### 5.3 Retry a failed attempt

Create a fresh Payment Element, ConfirmationToken, and logical-action UUID:

```http
POST /api/payments/{failedPaymentTransactionId}/retry-session
Idempotency-Key: {new-uuid}

{ "confirmationTokenReference": "ctoken_..." }
```

Only the contract Client can retry, and only a provider-confirmed failed deposit is eligible.

## 6. Save and manage Client cards

Show explicit consent before saving.

1. `POST /api/payment-methods/setup-session` with a new `Idempotency-Key`; no body.
2. Initialize Elements with `data.clientSecret` and mount Payment Element.
3. Call `elements.submit()`, then:

```ts
const { error } = await stripe.confirmSetup({
  elements,
  clientSecret: setupSession.clientSecret,
  confirmParams: {
    return_url: `${window.location.origin}/payment-methods/return`,
  },
  redirect: "if_required",
});
```

4. Refetch `GET /api/payment-methods`.
5. Set default with `PUT /api/payment-methods/{paymentMethodReference}/default`.
6. Remove with `DELETE /api/payment-methods/{paymentMethodReference}`.
7. Pay using a listed saved method:

```http
POST /api/milestones/{milestoneId}/fund
Idempotency-Key: {uuid}

{ "paymentMethodReference": "pm_..." }
```

Saved-card funding can require 3DS; use `stripe.handleNextAction` with the returned client secret.

## 7. Lawyer connected account and bank destination

Smart Court must not render a bank-account input. Stripe-hosted onboarding collects identity and external-bank data.

1. `GET /api/wallet/payout-account`.
2. If null, `Onboarding`, or `Restricted`, call `POST /api/wallet/payout-account/onboarding-link`.
3. Immediately navigate to `data.url`; it is short-lived and single-use.
4. `/wallet/payout-account/return`: call GET payout-account and display readiness.
5. `/wallet/payout-account/refresh`: create a new onboarding link and navigate to it.
6. Enable withdrawal only when status is `Enabled` and all three readiness booleans are true.
7. Optionally use `POST /api/wallet/payout-account/dashboard-link` for Stripe Express.

If the demo already has an `acct_...`, an admin linking endpoint exists, but normal UI onboarding above does not require an admin.

## 8. Work, acceptance, hold, release, withdrawal

1. Lawyer submits the funded milestone through Milestones UI.
2. Client accepts it. Backend sets `holdExpiresAt = acceptedAt + 14 days`; Lawyer net is pending.
3. The normal scheduled release runs after 14 days. The existing Super Administrator release route remains the only manual acceleration mechanism.
4. Lawyer calls `GET /api/wallet`.
5. Withdraw:

```http
POST /api/wallet/withdrawals
Idempotency-Key: {uuid}

{ "amount": 95.00, "destinationReference": "" }
```

`destinationReference` is legacy compatibility, not a bank selector. Stripe uses the connected account's configured external bank.

6. Show `GET /api/wallet/withdrawals`; numeric status is `0 Processing`, `1 Completed`, `2 Failed`.
7. A failed payout restores reserved funds; pending/unknown stays reserved until webhook/reconciliation resolves it.

## 9. Test cards

Type these only into Stripe Element fields. Use expiry `12/34`, CVC `123`, and any other form values.

| Scenario | Card number | Expected UI |
|---|---:|---|
| Success | `4242 4242 4242 4242` | Payment succeeds. |
| 3D Secure | `4000 0025 0000 3155` | Stripe opens authentication. |
| Generic decline | `4000 0000 0000 0002` | Show decline and allow a new attempt. |

Official reference: [Stripe test cards](https://docs.stripe.com/testing#cards).

## 10. Required frontend pages/components

- Provider bootstrap/error boundary.
- Client checkout and payment-return page.
- Failed-payment retry UI.
- Saved-method page and SetupIntent return page.
- Lawyer payout setup/return/refresh/dashboard UI.
- Payment history and 14-day countdown.
- Wallet, withdrawal form, and withdrawal history.
- Shared handling for `200`, `202`, `400`, `401`, `403`, `404`, `409`, `429`, and `500`.

Never log/store raw card fields, client secrets, Account Link URLs, secret keys, or webhook secrets.

## 11. Backend/DevOps webhook prerequisite

The frontend never calls webhook routes. The operator must forward/configure:

- Platform → `http://localhost:5049/api/payment-providers/stripe/webhooks/platform`
- Connected accounts → `http://localhost:5049/api/payment-providers/stripe/webhooks/connect`

Each listener's `whsec_...` belongs in its matching uncommitted Development setting. Localhost requires Stripe CLI forwarding or a public tunnel.

## 12. Official Stripe references

- [ConfirmationToken browser-to-server flow](https://docs.stripe.com/payments/payment-element/migration-ct)
- [Finalize payments and handle next actions](https://docs.stripe.com/payments/finalize-payments-on-the-server)
- [Save methods with SetupIntents](https://docs.stripe.com/payments/save-and-reuse?client=react&platform=web&ui=elements)
- [Existing-customer payments](https://docs.stripe.com/payments/existing-customers?platform=web&ui=direct-api)
- [Accounts v2](https://docs.stripe.com/connect/accounts-v2)
- [Accounts v2 Account Links](https://docs.stripe.com/api/v2/core/account-links/create)
- [Separate charges and transfers](https://docs.stripe.com/connect/separate-charges-and-transfers)
- [Refunds](https://docs.stripe.com/refunds)
- [Test cards](https://docs.stripe.com/testing#cards)

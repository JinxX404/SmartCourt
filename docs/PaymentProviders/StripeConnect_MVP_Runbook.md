# Stripe Connect MVP Runbook

This is the implementation handoff for the Smart Court Stripe Connect test
MVP. It complements `StripeConnect_Setup_Guide.md` and
`StripeConnect_Implementation_Plan.md`.

> This integration is sandbox-only. It is not a regulated escrow product and
> must not be enabled for live Egyptian payments. The 14-day hold is a Smart
> Court application rule, not a Stripe legal escrow service.

## Implemented lifecycle

1. A browser gets the safe `pk_test_...` value from `GET /api/payments/config`.
2. A client funds a milestone using a Stripe.js ConfirmationToken, or an owned saved PaymentMethod.
3. Smart Court derives amount/currency from the milestone and creates and confirms an automatic-capture PaymentIntent.
4. Success creates the escrow hold. A PaymentIntent needing 3DS returns HTTP
   202 with its client secret and remains processing.
5. Signed webhooks and exact-ID reconciliation finish the pending deposit.
6. After client acceptance, the existing 14-day hold starts.
7. At expiry, a separate Transfer sends only the lawyer allocation from the
   original Charge to the lawyer's enabled connected account.
8. Full and partial refunds target the original PaymentIntent.
9. Withdrawal reserves both the EGP wallet amount and the provider's
   transferred minor-unit balance, then creates a connected-account Payout.
10. Withdrawal remains processing until `payout.paid`. A confirmed failed or
   canceled payout restores both reserved balances.

## Required local configuration

For this test MVP, place these values only in ignored
`SmartCourt/appsettings.Development.json`. Do not add them to committed
`appsettings.json` or frontend configuration:

```json
{
  "PaymentProvider": {
    "UseMockProvider": false,
    "ProviderCode": "StripeConnect",
    "Stripe": {
      "SecretKey": "sk_test_...",
      "PublishableKey": "pk_test_...",
      "PlatformWebhookSecret": "whsec_...",
      "ConnectWebhookSecret": "whsec_...",
      "WebhookToleranceSeconds": 300,
      "MaxNetworkRetries": 2,
      "ConnectReturnUrl": "http://localhost:5173/wallet/payout-account/return",
      "ConnectRefreshUrl": "http://localhost:5173/wallet/payout-account/refresh",
      "DefaultConnectedAccountCountry": "US",
      "SandboxOnly": true
    }
  }
}
```

Startup fails when Stripe is selected and a required test key, endpoint secret,
or URL is missing. Only `sk_test_` and `pk_test_` keys are accepted.

## Database

```powershell
dotnet ef database update --project SmartCourt/SmartCourt.csproj --startup-project SmartCourt/SmartCourt.csproj
```

The API also auto-applies SQL Server migrations at startup. Confirm both
payment migrations are applied:

- `20260811165755_AddStripeConnectPaymentLifecycle`
- `20260812143235_AddClientPaymentCustomers`

They add connected payout accounts, Client Customer mappings, provider
object/status/money fields, webhook processing state, and withdrawal linkage.

## Webhook endpoints

- Platform: `POST /api/payment-providers/stripe/webhooks/platform`
- Connected accounts: `POST /api/payment-providers/stripe/webhooks/connect`

Use the endpoint-specific signing secret for each route. Both routes require
the raw body and `Stripe-Signature` header.

Platform events:

- `payment_intent.succeeded`
- `payment_intent.payment_failed`
- `payment_intent.processing`
- `payment_intent.canceled`
- `refund.updated`
- `charge.refunded`

Connected-account events:

- `account.updated`
- `payout.paid`
- `payout.failed`
- `payout.canceled`

For local testing, either forward two Stripe CLI listeners and use their
endpoint-specific secrets, or use one listener with both `--forward-to` and
`--forward-connect-to`. A single listener uses the same printed `whsec_...`
secret for both local routes.

## Browser configuration and Client payment APIs

- `GET /api/payments/config` — anonymous; returns only publishable configuration.
- `POST /api/milestones/{milestoneId}/payment-session` — recommended ConfirmationToken checkout.
- `POST /api/payments/{paymentTransactionId}/retry-session` — owning Client retry.
- `POST /api/payment-methods/setup-session` — Customer + SetupIntent creation.
- `GET /api/payment-methods` — masked saved methods.
- `PUT /api/payment-methods/{paymentMethodReference}/default`
- `DELETE /api/payment-methods/{paymentMethodReference}`
- `POST /api/milestones/{milestoneId}/fund` — saved/legacy PaymentMethod checkout.

All payment, retry, setup, and withdrawal mutations require `Idempotency-Key`.
The frontend implementation sequence and Stripe.js examples are in
`StripeConnect_Frontend_Sandbox_Guide.md`; the exact DTOs/errors are in
`../Payments_API_Integration_Guide.md`.

## Lawyer payout-account API

- `GET /api/wallet/payout-account`
- `POST /api/wallet/payout-account/onboarding-link`
- `POST /api/wallet/payout-account/dashboard-link`

Normal browser setup does **not** link database rows manually. The Lawyer calls
the onboarding-link endpoint; the backend creates the local mapping and Stripe
Accounts v2 recipient, then Stripe hosts identity/bank onboarding.

For QA only, sandbox administrators may link an existing test account:

```http
POST /api/admin/payment-providers/stripe/connected-accounts/link
Content-Type: application/json

{
  "lawyerUserId": "00000000-0000-0000-0000-000000000000",
  "providerAccountId": "acct_..."
}
```

Stripe release and withdrawal are blocked until details are submitted and both
transfers and payouts are enabled.

## Client funding response

`POST /api/milestones/{milestoneId}/payment-session` returns `FundingOperationDto`:

- HTTP 200 with `payment` populated when funding completed.
- HTTP 202 with `clientSecret` when customer action is required.
- HTTP 202 without a completed payment while processing.

The frontend creates `ctoken_...` with Stripe.js and must handle 3DS using
`stripe.handleNextAction({ clientSecret })` when returned.
Raw card details must never be sent to Smart Court.

## Failed-card retry

The owning Client retries a provider-confirmed failed deposit with a fresh
browser ConfirmationToken:

```http
POST /api/payments/{failedPaymentTransactionId}/retry-session
Idempotency-Key: a-new-unique-key
Content-Type: application/json

{
  "confirmationTokenReference": "ctoken_..."
}
```

The original failed transaction is retained for audit. The retry creates a
new transaction and a new Stripe idempotency key derived from both attempts.

## Demo sequence

1. Apply the migration and start the API with test settings.
2. Use normally registered, email-confirmed, verified, and approved Smart Court users; authentication behavior is unchanged.
3. Let the Lawyer create the connected account through onboarding-link and finish Stripe-hosted onboarding.
4. From the browser, fetch payment config, mount Payment Element, create a ConfirmationToken, and fund a ready milestone.
5. Complete 3DS if the response supplies a client secret.
6. Confirm the webhook funds the milestone and creates the hold.
7. Accept the milestone and use the existing admin force-release endpoint so
   the demo does not wait 14 days.
8. Confirm the wallet and connected-account provider balance were credited.
9. Submit a withdrawal and confirm its initial processing state/history.
10. Trigger/wait for the test payout event and confirm completion. A failed test
   payout demonstrates restoration of both reserved balances.
11. Use another case to demonstrate full or partial refund.

## Verification performed

The following sandbox lifecycle was executed against local SQL Server on
2026-08-12:

1. A real Stripe PaymentIntent charged a test client **EGP 100.00**.
2. Smart Court created an escrow hold with **EGP 5.00** platform fee and
   **EGP 95.00** lawyer allocation.
3. Client acceptance started the configured 14-day hold; the test used the
   existing administrator force-release endpoint to avoid waiting.
4. A separate Stripe Transfer completed and credited the lawyer wallet.
5. A connected-account Payout completed with Stripe status `paid`.
6. Smart Court reduced the lawyer wallet and tracked provider balance to zero.
7. Separate real test PaymentIntents verified a full refund and a partial
   refund. The partial test refunded **EGP 40.00**, then refunded the remaining
   **EGP 60.00** to leave no test charge outstanding; every refund returned
   `succeeded`.
8. Fresh signed `payment_intent.succeeded`, `charge.refunded`,
   `refund.updated`, and connected `payout.paid` webhook deliveries returned
   HTTP 200 and were checkpointed as processed.
9. Focused persistence/payment/API tests after adding replacement-card retry
   and webhook checkpoint support: **44/44 passed**.
10. Full solution build: **0 errors**; existing repository warnings remain.

## Full-stack completion checklist

The sandbox is end-to-end ready when all of these pass:

- Existing registration/email confirmation/verification/approval works unchanged.
- `GET /api/payments/config` returns `StripeConnect`, `pk_test_...`, and `sandboxOnly=true`.
- Client can type success, 3DS, and decline cards in Stripe Payment Element.
- Successful payment creates the local hold after webhook/query reconciliation.
- Declined payment can be retried by the owning Client.
- Client can save, list, default, remove, and pay with an owned saved card.
- Lawyer can create and finish Stripe-hosted connected-account onboarding without a database edit.
- Accepted milestone starts the real 14-day hold; existing admin release may accelerate the demo.
- Release creates the Stripe Transfer and moves pending to available wallet balance.
- Lawyer can withdraw and see processing/completed/failed history.
- Contract/Dispute termination demonstrates full or partial refund.
- Both webhook endpoints reject bad signatures and process configured test events.

The broad test project run reached **806 passing tests**. Ten failures were
observed before focused corrections: six deterministic compatibility failures
were fixed and now pass; the remaining four were SQL Server deadlock/timeout
failures caused by the database-heavy suite running concurrently.
The targeted SQL-backed payment lifecycle was also executed successfully
against the local database as described above.

The test connected account settles in USD. Stripe therefore converted the
EGP lawyer allocation to **USD 1.89** before transfer/payout. Smart Court still
kept its contractual ledger in EGP. For an Egypt production rollout, use a
supported connected-account country/bank/currency configuration and confirm
the commercial/legal availability with Stripe before relying on this model.

### Test-environment notes

- Sandbox keys and CLI webhook secrets are stored only in ignored
  `appsettings.Development.json`; no Windows environment variables are used.
- The local network DNS server could not resolve Stripe. DNS was changed
  temporarily for the sandbox run and must be restored afterward.
- One initial deposit remained `Processing` after the pre-request DNS failure.
  It has no Stripe object ID and is intentionally retained for manual
  reconciliation instead of being silently retried.
- The application-level full/partial refund paths use the original
  PaymentIntent plus the requested amount and are covered by settlement and
  provider tests. Use a separate funded contract for a live refund demo so it
  does not conflict with the release/withdrawal demo case.

# Stripe Connect Test MVP — Dashboard Setup Guide

> Project: Smart Court  
> Provider: Stripe Connect  
> Environment: Stripe sandbox/test mode only  
> Phase: 2 — manual account and dashboard preparation  
> Last reviewed: 11 August 2026

## 1. What this setup is for

This guide prepares a free Stripe testing environment for the Smart Court MVP demonstration:

```text
Test client card
    -> Smart Court platform PaymentIntent
    -> Smart Court's internal escrow-style ledger and 14-day rule
    -> Stripe Transfer to a test lawyer connected account
    -> Stripe Payout to a test lawyer bank account
```

It will let the application demonstrate:

- a client deposit;
- successful, failed, and 3D Secure card payments;
- full and partial refunds;
- a Smart Court-controlled 14-day hold;
- release of the lawyer's net payment to a connected account;
- a separate lawyer withdrawal to a simulated bank account;
- webhook-driven status updates.

Stripe does not move real money in a sandbox. Stripe states that an account can use testing environments before live activation, and test payouts do not reach a bank. See [Stripe accounts](https://docs.stripe.com/get-started/account) and [Testing Stripe Connect](https://docs.stripe.com/connect/testing).

## 2. Important rules before starting

1. Keep the Dashboard in **Sandbox** or **Test mode** throughout this guide.
2. Do not click **Activate payments** or attempt to enable live payments for the MVP.
3. Do not provide a false production country, company, tax ID, address, or bank account.
4. Test data may be fictional only when Stripe clearly shows that you are editing a sandbox/test object.
5. Never paste `sk_test_...` or `whsec_...` into chat, source control, screenshots, tickets, or frontend code.
6. Never enter a real card or bank account in test mode. Use Stripe's published test values.

Egypt is not currently listed as a supported country for activating a Stripe platform account. That prevents an Egyptian Smart Court entity from going live; it does not prevent this sandbox demonstration. See [Stripe global availability](https://stripe.com/global).

## 3. Values you will collect

Keep the following checklist in a private password manager or local secret store. Do **not** put the actual values in this document.

| Item | Expected format | Used by |
|---|---|---|
| Sandbox publishable key | `pk_test_...` | Smart Court frontend |
| Sandbox secret key | `sk_test_...` | Smart Court API only |
| Test lawyer connected-account ID | `acct_...` | Release and withdrawal testing |
| Platform webhook signing secret | `whsec_...` | Platform payment/refund/transfer webhooks |
| Connect webhook signing secret | `whsec_...` | Lawyer-account/payout webhooks |

The two webhook secrets cannot be finalized until Smart Court has a reachable webhook URL. Section 9 explains the safe options. Having the publishable key, secret key, and test connected-account ID is enough to complete the immediate dashboard preparation.

## 4. Create the free Stripe test account

1. Open [Stripe registration](https://dashboard.stripe.com/register).
2. Enter your email address, full name, and a strong unique password.
3. Verify your email when Stripe sends the verification message.
4. Enable two-step authentication when prompted.
5. Sign in to the [Stripe Dashboard](https://dashboard.stripe.com/).
6. Do not start live-account activation.

Stripe does not require live bank details to use a testing environment. Live pricing and country eligibility do not apply to simulated test transactions.

### If registration asks for a country

- Use your real business/residence information for the main Stripe account.
- Do not select another country merely to bypass Stripe's live availability rules.
- If Stripe prevents account creation before it lets you reach a sandbox, stop and record the exact message. Do not continue with false data.

## 5. Create and select a dedicated sandbox

Stripe is transitioning from a single test-mode toggle to named sandboxes, so the exact Dashboard wording can vary.

### Preferred interface: named sandbox

1. Open the account picker near the upper-left corner of the Dashboard.
2. Select **Create sandbox**.
3. Name it `Smart Court MVP`.
4. Select the new sandbox.
5. Confirm that the Dashboard displays a sandbox/test banner.

### Older interface: test-mode toggle

1. Find **Test mode** or **View test data** in the Dashboard.
2. Turn it on.
3. Confirm that all displayed payments are test data.

From this point onward, stop if the Dashboard is not visibly in sandbox/test mode.

## 6. Enable and configure Stripe Connect

Smart Court needs Connect because a lawyer is a separate payment recipient. A normal Stripe Payments integration can charge and refund clients, but it cannot give every lawyer an independently withdrawable balance.

1. While the `Smart Court MVP` sandbox is selected, open **Connect**.
2. Select **Get started** or **Set up Connect**.
3. Describe the business model as a **platform or marketplace** that collects a client payment and later pays a service provider.
4. Use `Smart Court` as the test platform name.
5. Use a test description such as `Legal-services marketplace MVP — sandbox only`.
6. Complete only the sandbox platform profile required to create test connected accounts.
7. Do not complete live production activation.

### Connected-account configuration to select

Stripe's current Dashboard may describe the configuration through controller properties instead of the older Standard/Express/Custom names. Choose the configuration with these outcomes:

| Setting | Smart Court test choice |
|---|---|
| Account Dashboard access | Stripe-hosted **Express Dashboard** or equivalent limited Stripe-hosted access |
| Account onboarding | Stripe-hosted onboarding |
| Payment charges | Created by the Smart Court platform |
| Required capability | `transfers` |
| Stripe fees | Smart Court platform is responsible |
| Negative balances/disputes | Smart Court platform is responsible |
| Payout schedule | Platform-controlled; implementation will set `manual` |

Do not request `card_payments` for the lawyer unless Stripe's selected configuration requires it automatically. The client charge belongs to the Smart Court platform; the lawyer only needs to receive Transfers and request Payouts. Stripe documents `transfers` as the capability used by platforms to pay connected accounts. See [Account capabilities](https://docs.stripe.com/connect/account-capabilities).

### Products required for the MVP

- **Stripe Payments:** platform PaymentIntents for client deposits and Refunds.
- **Stripe Connect:** connected lawyer accounts and Transfers.
- **Stripe Payouts through Connect:** withdrawal from a lawyer's connected balance to their external bank account.
- **Workbench/Webhooks:** asynchronous payment and payout status notifications.

Do not create a Stripe Product, recurring Price, subscription, invoice, Treasury account, Issuing card, or Global Payouts recipient. Those are different Stripe concepts and are not required for Smart Court.

The selected flow is **Separate Charges and Transfers**: the platform charges the client and later creates a distinct Transfer for the lawyer. See [Stripe's Separate Charges and Transfers guide](https://docs.stripe.com/connect/separate-charges-and-transfers).

## 7. Get the sandbox API keys

1. Confirm that `Smart Court MVP` sandbox/test mode is active.
2. Open **Developers → API keys**. In newer Dashboard layouts, open **Workbench → API keys**.
3. Under **Standard keys**, copy the **Publishable key** beginning with `pk_test_`.
4. Select **Reveal test key** for the **Secret key**.
5. Copy the secret key beginning with `sk_test_` into a private password manager or local .NET secret store.
6. Confirm that neither value contains `_live_`.

Stripe explains the key types and Dashboard locations in [API keys](https://docs.stripe.com/keys).

### Key-handling rules

- The publishable key can be used by the browser frontend.
- The secret key must only be read by the Smart Court backend.
- Do not add the secret key to `appsettings.json` in source control.
- Phase 4 will place local secrets in .NET User Secrets or environment variables.
- If a secret is exposed, roll it in the Dashboard before continuing.

See [Stripe's secret-key best practices](https://docs.stripe.com/keys-best-practices).

## 8. Create the test receiver/lawyer account

This creates a simulated lawyer who can receive a release and request a withdrawal. It does not create a real merchant or bank account.

### 8.1 Create the connected account

1. Keep the `Smart Court MVP` sandbox selected.
2. Open **Connect → Connected accounts**.
3. Select **+ Create**.
4. Choose the Express/limited Stripe-hosted Dashboard configuration prepared in Section 6.
5. Set the account's display name to `Smart Court Demo Lawyer`.
6. Use an email address different from the platform account's email. An accessible plus-address such as `yourname+smartcourt-lawyer@example.com` is convenient.
7. Request the `transfers` capability if the form presents a capability choice.
8. Select **Create** or **Continue**.

Stripe may generate a single-use onboarding link. This is expected. Stripe documents manual test-account creation under [Manage individual connected accounts](https://docs.stripe.com/connect/dashboard/managing-individual-accounts).

### 8.2 Complete test onboarding

1. Open the generated onboarding link in a private/incognito browser window.
2. Confirm that the page is a Stripe test/sandbox onboarding flow.
3. Enter clearly fictional test identity and business details.
4. If SMS verification appears, use Stripe's test code `000-000`.
5. For a phone validation value, Stripe documents `0000000000` as a successful test token.
6. For a test business website, use `https://accessible.stripe.com` if a website is required.
7. Complete every required sandbox field until the account reports that details were submitted.

The requested fields depend on the simulated connected-account country and current Stripe requirements. Use the test values offered by Stripe in the onboarding UI or [Testing Stripe Connect](https://docs.stripe.com/connect/testing); never use another person's real identity.

### 8.3 Add a successful test bank account

For the simplest payout simulation, use a United States test connected account **only if that country is supported by the sandbox configuration you selected**. Stripe publishes these successful payout values:

| Field | Test value |
|---|---|
| Routing number | `110000000` |
| Account number | `000123456789` |
| Account holder | `Smart Court Demo Lawyer` |
| Account type | Checking |

For a non-US test account, use the country selector and test bank values displayed in Stripe's [Connect payout testing documentation](https://docs.stripe.com/connect/testing). Do not enter real bank details.

If Stripe does not allow a US connected account for the sandbox's region, keep the platform and connected account in the same region and use Stripe's documented test bank data for that region. This avoids unsupported cross-region Transfer errors.

### 8.4 Record and verify the account

1. Return to **Connect → Connected accounts**.
2. Open `Smart Court Demo Lawyer`.
3. Copy its ID beginning with `acct_` into your private setup notes.
4. Confirm that **Details submitted** is true/complete.
5. Confirm that **Transfers** is active or enabled.
6. Confirm that **Payouts** is enabled.
7. Do not worry if **Charges** is inactive; Smart Court creates charges on the platform.

The application will eventually store this `acct_...` value against the lawyer's verified payout profile. It must not accept an arbitrary account ID supplied with each withdrawal request.

### 8.5 Payout schedule

Smart Court requires two separate operations:

1. **Release:** Transfer platform funds to the lawyer's connected Stripe balance.
2. **Withdraw:** create a Payout from that connected balance to the lawyer's verified bank account.

Therefore, automatic payouts must not remove the balance before the lawyer requests withdrawal. Do not manually change settings if the Dashboard does not expose this control. Phase 3 will specify, and Phase 4 will implement, a platform-controlled `manual` payout schedule. Stripe defines `manual` as preventing automatic payouts until the platform creates a Payout. See [Manage payout schedules](https://docs.stripe.com/connect/manage-payout-schedule).

## 9. Prepare the webhook configuration

### Why there will be two signing secrets

Smart Court needs events from two scopes:

- **Platform events:** client PaymentIntents, charges, refunds, and Transfers.
- **Connected-account events:** lawyer account updates and Payouts.

Stripe event destinations distinguish events on the platform account from events on connected accounts. Each destination has its own `whsec_...` signing secret. Phase 3 will finalize the endpoint design, expected event set, and configuration names.

### Do not create an unreachable endpoint now

At this phase the Stripe webhook controller does not exist, so do not register `localhost` in the Dashboard and do not use an unrelated public URL. Stripe cannot deliver Dashboard webhooks directly to a private localhost address.

Choose one of these approaches after the endpoint exists:

#### Local development — Stripe CLI

The Stripe CLI can forward sandbox events to the local API. Running `stripe listen` prints a temporary `whsec_...` secret for that CLI session. This secret changes when a new listener session is created unless the CLI configuration preserves it.

#### Deployed demo — Dashboard event destinations

1. Open **Workbench → Webhooks**.
2. Select **Create new destination**.
3. Create a platform-account destination for the HTTPS Smart Court platform webhook URL.
4. Create a connected-accounts destination for the HTTPS Smart Court Connect webhook URL.
5. Open each destination and select **Reveal signing secret**.
6. Store both `whsec_...` values privately.

Stripe documents these steps in [Manage event destinations](https://docs.stripe.com/workbench/event-destinations) and explains signature verification in [Receive webhook events](https://docs.stripe.com/webhooks).

### Planned event families

The exact list will be approved in Phase 3. Expect the implementation to cover at least:

- `payment_intent.succeeded`
- `payment_intent.processing`
- `payment_intent.payment_failed`
- refund lifecycle events
- Transfer creation/reversal updates relevant to releases
- `account.updated`
- `payout.created`
- `payout.updated`
- `payout.paid`
- `payout.failed`
- `payout.canceled`

Do not select **all events** for the final integration. Subscribe only to events the application handles.

## 10. Prepare the test client/card flow

A test client does not need a Stripe connected account. The client remains a normal Smart Court user and enters a Stripe test card in Stripe Checkout or Stripe Elements.

Use these values only after the frontend payment form is implemented:

| Scenario | Card number | Expiry | CVC |
|---|---|---|---|
| Successful payment | `4242 4242 4242 4242` | Any future date, e.g. `12/34` | Any 3 digits |
| 3D Secure authentication | `4000 0025 0000 3155` | Any future date | Any 3 digits |
| Insufficient funds | `4000 0000 0000 9995` | Any future date | Any 3 digits |

Use any fictional cardholder name and billing postal code. Never use a real card in test mode. See [Stripe test cards](https://docs.stripe.com/testing).

Smart Court stores all contract and wallet values in EGP. Stripe lists EGP as a supported card presentment currency and treats it as a two-decimal currency, so `100.00 EGP` is sent to Stripe as `10000` minor units. Depending on the sandbox's simulated country, Stripe may display settlement or payout conversion into the sandbox's settlement currency. The application-side ledger remains EGP; Phase 3 will explicitly define the provider amount and currency rules. See [Stripe supported currencies](https://docs.stripe.com/currencies).

## 11. What not to configure

Do not configure any of the following for this MVP:

- live-mode API keys;
- live card or bank details;
- Stripe Atlas or a foreign legal entity;
- subscriptions or recurring prices;
- manual card authorization as the 14-day hold;
- destination charges that immediately transfer client money to the lawyer;
- automatic lawyer payouts;
- Stripe Treasury, Issuing, Billing, Invoicing, or Global Payouts;
- a claim that Stripe provides regulated escrow.

The 14-day period is enforced by Smart Court after milestone acceptance. It is not a 14-day card authorization. Stripe also states that manual payout timing is not an escrow service. See [Stripe manual payouts](https://docs.stripe.com/connect/manual-payouts).

## 12. Completion checklist

Before confirming that Phase 2 is complete, verify all applicable boxes:

- [ ] Stripe account created and email verified.
- [ ] Two-step authentication enabled.
- [ ] `Smart Court MVP` sandbox/test mode selected.
- [ ] Connect sandbox setup started and platform profile completed enough to create test accounts.
- [ ] Separate Charges and Transfers chosen as the intended architecture.
- [ ] Sandbox publishable key `pk_test_...` stored privately.
- [ ] Sandbox secret key `sk_test_...` stored privately.
- [ ] `Smart Court Demo Lawyer` test connected account created.
- [ ] Lawyer connected-account ID `acct_...` stored privately.
- [ ] `transfers` capability active or the exact outstanding requirement recorded.
- [ ] Test external bank account added and payouts enabled, or the exact outstanding requirement recorded.
- [ ] No live key, real card, real bank account, or false live-business information used.
- [ ] Webhook-secret creation understood as deferred until a reachable endpoint/Stripe CLI listener exists.

When finished, confirm completion without sending any secret values. A safe response is:

> Stripe sandbox ready. I have the `pk_test`, `sk_test`, and test lawyer `acct_` ID stored privately. Webhook secrets are deferred until the endpoint exists.

If any step is blocked, send only the Dashboard section name and exact error message. Redact keys, identity information, email addresses, and account IDs from screenshots.

## 13. Official references

- [Stripe accounts and sandboxes](https://docs.stripe.com/get-started/account)
- [Stripe API keys](https://docs.stripe.com/keys)
- [Stripe key-security best practices](https://docs.stripe.com/keys-best-practices)
- [How Stripe Connect works](https://docs.stripe.com/connect/how-connect-works)
- [Testing Stripe Connect](https://docs.stripe.com/connect/testing)
- [Manage connected accounts in the Dashboard](https://docs.stripe.com/connect/dashboard/managing-individual-accounts)
- [Connect account capabilities](https://docs.stripe.com/connect/account-capabilities)
- [Separate Charges and Transfers](https://docs.stripe.com/connect/separate-charges-and-transfers)
- [Manage connected-account payout schedules](https://docs.stripe.com/connect/manage-payout-schedule)
- [Stripe test cards](https://docs.stripe.com/testing)
- [Stripe supported currencies](https://docs.stripe.com/currencies)
- [Stripe event destinations](https://docs.stripe.com/workbench/event-destinations)
- [Stripe webhook signatures](https://docs.stripe.com/webhooks)
- [Stripe global availability](https://stripe.com/global)


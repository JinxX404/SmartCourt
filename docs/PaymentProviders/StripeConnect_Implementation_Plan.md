# Stripe Connect Test MVP — End-to-End Implementation Plan

> Project: Smart Court (.NET 8 API)  
> Provider: Stripe Connect through the official Stripe.net SDK  
> Environment: sandbox/test mode only  
> Phase: 3 — pre-code review  
> Last reviewed: 11 August 2026  
> Code status: no Stripe implementation code has been written

## 1. Outcome of this implementation

After this plan is approved and Phase 4 is completed, Smart Court will support this complete sandbox lifecycle:

```mermaid
sequenceDiagram
    actor Client
    participant API as Smart Court API
    participant Stripe as Stripe platform
    participant Ledger as Smart Court escrow ledger
    participant Lawyer as Lawyer connected account
    participant Bank as Test bank account

    Client->>API: Fund milestone with Stripe PaymentMethod
    API->>Stripe: Create and confirm PaymentIntent in EGP
    alt Immediate success
        Stripe-->>API: payment_intent.succeeded
        API->>Ledger: Fund hold and credit lawyer PendingBalance
    else 3DS or asynchronous processing
        Stripe-->>API: requires_action or processing + client secret
        API-->>Client: Complete Stripe action
        Stripe->>API: Signed payment webhook
        API->>Ledger: Fund hold exactly once
    else Confirmed decline
        Stripe-->>API: requires_payment_method / card error
        API->>Ledger: Fail attempt; no hold created
    end

    Note over API,Ledger: Work, 7-day review, acceptance, then existing 14-day hold

    API->>Stripe: Transfer lawyer net amount using source Charge
    Stripe->>Lawyer: Credit connected-account balance
    API->>Ledger: PendingBalance to AvailableBalance; retain platform fee

    Lawyer->>API: Withdraw available balance
    API->>Ledger: Reserve amount
    API->>Stripe: Create Payout as connected account
    Stripe-->>API: pending / in_transit
    Stripe->>API: Signed payout.paid or payout.failed webhook
    alt Paid
        Stripe->>Bank: Simulated bank payout
        API->>Ledger: Complete withdrawal
    else Failed or canceled
        API->>Ledger: Fail withdrawal and restore reserved balance
    end
```

Full and partial refund branches use Stripe Refunds against the original PaymentIntent while the hold is still unreleased. Dispute and contract-termination accounting remains controlled by the existing Smart Court settlement services.

## 2. Non-negotiable design decisions

### 2.1 Stripe is an adapter, not the domain model

- `Stripe.*` SDK types may appear only below `SmartCourt/Providers/Payments/Stripe`.
- Feature services continue to depend on domain-named interfaces under `Infrastructure/Providers/Payments`.
- No Stripe status string, exception, account object, or client library leaks into Contracts, Milestones, Disputes, Wallets, or other feature slices.

This follows the repository's `add-infra-provider` rule.

### 2.2 Capture the client payment when funding succeeds

Smart Court will create a PaymentIntent with automatic capture. It will not use a 14-day card authorization.

The Smart Court hold begins after an unbounded work period, a seven-day review opportunity, acceptance, and then another fourteen days. A card authorization is not reliable for that duration. The captured money remains in the platform-side Stripe balance until Smart Court authorizes a Transfer.

### 2.3 Use Separate Charges and Transfers

- Deposit: platform PaymentIntent.
- Release: Transfer from platform to the lawyer connected account.
- Refund: Refund the platform PaymentIntent.
- Withdraw: Payout while acting as the lawyer connected account.

The Transfer will contain:

- the lawyer's persisted and verified `acct_...` destination;
- the original Stripe Charge as `source_transaction` when Stripe's regional/currency rules permit it;
- Smart Court transaction, hold, contract, milestone, and lawyer IDs as metadata;
- a `transfer_group` derived from the escrow hold;
- the lawyer **net** amount, never the gross hold amount.

Stripe describes this model in [Separate Charges and Transfers](https://docs.stripe.com/connect/separate-charges-and-transfers).

### 2.4 Release and withdrawal remain separate

The connected account will use a platform-controlled `manual` payout schedule:

- Release changes `PendingBalance -> AvailableBalance` after Stripe creates the Transfer.
- Withdraw reserves Smart Court's available balance and creates a Stripe Payout.
- A created Payout is not considered completed until Stripe reports `paid`.
- A failed or canceled Payout restores the reserved Smart Court balance exactly once.

Stripe documents `manual` as disabling automatic payouts until the platform creates one through the API. See [Manage payout schedules](https://docs.stripe.com/connect/manage-payout-schedule).

### 2.5 Never trust a withdrawal destination supplied by the browser

`CreateWithdrawalRequest.DestinationReference` will be removed. A withdrawal will use the authenticated lawyer's persisted `LawyerPayoutAccount`, which must:

- belong to that lawyer;
- match the selected provider;
- have an active `transfers` capability;
- have payouts enabled;
- have completed Stripe-hosted onboarding;
- contain no raw bank account number in Smart Court.

### 2.6 Known asynchronous state is not an unknown outcome

`ProviderOperationOutcome` will be expanded:

| Outcome | Meaning |
|---|---|
| `Succeeded` | Provider confirms the financial operation reached its required terminal success state |
| `Failed` | Provider confirms no further success will occur without a new action/request |
| `Processing` | Provider accepted the operation and supplied an object ID, but it is not terminal |
| `RequiresCustomerAction` | Deposit exists and the client must complete Stripe.js/3DS action |
| `Unknown` | Smart Court cannot determine whether Stripe accepted or executed the request |

Application `PaymentTransactionStatus` and `WithdrawalStatus` remain `Processing`, `Completed`, and `Failed`. `Processing` covers known provider processing, action-required, and unknown outcomes; provider status fields retain the distinction for reconciliation.

### 2.7 Stripe object IDs must be persisted immediately

Whenever Stripe returns an object—even if it is pending or requires action—Smart Court will save its ID before returning to the client:

- `pi_...` for a deposit;
- `re_...` for a refund;
- `tr_...` for a release;
- `po_...` for a withdrawal;
- `ch_...` as the deposit's source Charge when available.

Reconciliation must retrieve these IDs directly. It will not guess an object from amount/date or rely on an arbitrary client reference.

### 2.8 Sandbox EGP and provider settlement currency are different concepts

Smart Court's authoritative contract, hold, wallet, and ledger currency remains EGP. Stripe supports EGP as a two-decimal card presentment currency, so `100.00 EGP` becomes `10000` Stripe minor units. See [Supported currencies](https://docs.stripe.com/currencies).

A Stripe platform outside Egypt can settle an EGP charge into its platform settlement currency. Therefore, the implementation will persist both:

- business money: Smart Court decimal amount + `EGP`;
- provider money: Stripe integer minor-unit amount + provider settlement currency.

For a release, the provider will retrieve the original PaymentIntent with its latest Charge and balance transaction. It will calculate the connected-account transfer from the provider-side gross settlement amount using the Smart Court lawyer-net/gross ratio. The lawyer amount is rounded down to the provider currency's minor unit so the transfer never exceeds its allocation; the remainder stays with the platform.

No hidden fixed FX rate will be invented. Stripe's returned balance-transaction values are the source of truth for provider-side settlement. Stripe documents FX across charges and transfers in [Connect currencies](https://docs.stripe.com/connect/currencies).

For the sandbox discussion demo, Stripe fees and FX are informational provider-side costs borne by the simulated platform. Smart Court's existing 5% platform fee remains the business fee. A future live provider decision must define whether PSP fees reduce platform revenue or require a new business ledger entry.

## 3. Provider contracts

### 3.1 `ProviderResult`

`Infrastructure/Providers/Payments/Models/ProviderResult.cs` will gain:

- normalized provider status;
- provider object type;
- optional provider-side minor-unit amount and currency;
- optional next-client-action data;
- existing business amount/currency and correlation fields;
- existing provider object ID and safe failure reason.

New provider-neutral models:

```text
ProviderClientAction
  Type: ConfirmPayment | Redirect
  ClientSecret: optional
  RedirectUrl: optional

ProviderMoney
  AmountMinor: Int64
  Currency: lowercase ISO code
```

The client secret is returned only to the authenticated contract client. It is not logged or placed in notification/outbox payloads.

### 3.2 Deposit requests

`ProviderDepositRequest` remains based on:

- EGP business amount;
- milestone business ID;
- provider idempotency key;
- correlation ID;
- tokenized Stripe PaymentMethod reference (`pm_...`).

`ProviderDepositRetryRequest` will also require a new PaymentMethod reference. Smart Court will not persist or silently reuse an old failed card PaymentMethod.

### 3.3 Release request

`ProviderReleaseRequest` will add:

- original deposit PaymentIntent ID;
- original Charge ID when already known;
- verified lawyer connected-account ID;
- gross EGP amount for ratio validation;
- `Amount` changed to the lawyer net EGP amount.

### 3.4 Refund request

`ProviderRefundRequest` will add the original deposit PaymentIntent ID. Stripe supports full and partial Refunds by PaymentIntent. A refund is allowed only while the Smart Court hold has not already been released.

### 3.5 Withdrawal request

`ProviderWithdrawalRequest` will add the persisted connected-account ID and remove free-form destination input. Stripe will use the account's verified default external payout account.

### 3.6 Reconciliation requests

All four status request records will add the provider object ID. Deposit status may also include the source PaymentIntent ID and expected operation metadata. The adapter will retrieve:

- PaymentIntent for deposit;
- Transfer for release;
- Refund for refund;
- Payout with `RequestOptions.StripeAccount` for withdrawal.

If no provider object ID was received after a connection failure, the provider will return `null`/`Unknown`; the existing SLA/manual-action path remains authoritative. An identical financial POST can be retried only with the same Stripe idempotency key and identical parameters.

Stripe documents safe POST retries in [Idempotent requests](https://docs.stripe.com/api/idempotent_requests).

## 4. Stripe status mapping

### 4.1 PaymentIntent — Deposit

| Stripe status/result | `ProviderOperationOutcome` | Smart Court action |
|---|---|---|
| `succeeded` | `Succeeded` | Create escrow hold and credit lawyer pending balance |
| `requires_action` | `RequiresCustomerAction` | Persist `pi_...`; return client secret; wait for confirmation/webhook |
| `processing` | `Processing` | Persist `pi_...`; wait for webhook/reconciliation |
| `requires_payment_method` | `Failed` | Mark attempt failed and return milestone to awaiting funding |
| `canceled` | `Failed` | Mark attempt failed |
| `requires_confirmation` after provider attempted confirmation | `Unknown` | Keep processing; configuration/manual investigation |
| `requires_capture` | `Unknown` | Automatic-capture invariant violated; do not create hold |
| unrecognized status | `Unknown` | Keep processing and reconcile |

The webhook handler will retrieve the current PaymentIntent before finalizing, so out-of-order events cannot regress a terminal state.

### 4.2 Transfer — Release

Stripe Transfer creation is synchronous and does not expose a normal pending/failed status lifecycle:

| Stripe result | Outcome | Smart Court action |
|---|---|---|
| Transfer object returned | `Succeeded` | Complete release ledger transition once |
| Deterministic invalid destination/capability/amount error | `Failed` | Use existing release retry/manual-action policy |
| Timeout, connection loss, 5xx, or uncertain idempotency result | `Unknown` | Leave hold funded; reconcile/retry safely |
| Retrieved full transfer reversal | `Failed` for reconciliation | Require finance manual action; never silently rewrite released append-only ledger |

### 4.3 Refund

| Stripe Refund status | Outcome | Smart Court action |
|---|---|---|
| `succeeded` | `Succeeded` | Complete full/partial refund ledger settlement |
| `pending` | `Processing` | Keep settlement processing |
| `requires_action` | `Processing` | Keep processing and surface manual/provider requirement |
| `failed` | `Failed` | Confirm failure; preserve funded hold for retry/recovery |
| `canceled` | `Failed` | Confirm failure |
| unrecognized | `Unknown` | Reconcile |

### 4.4 Payout — Withdraw

| Stripe Payout status | Outcome | Smart Court action |
|---|---|---|
| `pending` | `Processing` | Keep wallet amount reserved; return accepted status |
| `in_transit` | `Processing` | Keep reserved |
| `paid` | `Succeeded` | Complete withdrawal |
| `failed` | `Failed` | Fail withdrawal and restore reserved amount once |
| `canceled` | `Failed` | Fail withdrawal and restore reserved amount once |
| unrecognized | `Unknown` | Keep reserved and reconcile |

Creating a Payout successfully does **not** immediately complete `WithdrawalRequest`. This corrects the current service behavior.

## 5. Exception and retry policy

The Stripe adapter will map errors without exposing secrets or raw Stripe payloads:

| Failure class | Mapping |
|---|---|
| Card decline with a terminal PaymentIntent | `Failed`, sanitized decline message/code |
| Invalid request before object creation | `Failed`; log Stripe request ID and safe code |
| Authentication/permission/configuration error | `Failed` plus critical operator log; never reveal API key details |
| Rate limit | `Unknown`; SDK retry, then reconciliation |
| Stripe API/server error (`5xx`) | `Unknown` |
| Connection/timeout after dispatch | `Unknown` |
| Idempotency mismatch/conflict | `Unknown` plus manual-action diagnostic |
| Caller cancellation before dispatch | rethrow cancellation |
| Cancellation after dispatch cannot be proven safe | persist processing/unknown before returning |

The official SDK's bounded network retries will be enabled with Stripe idempotency keys. Services will not blindly create replacement operations after unknown outcomes. See [Stripe API errors](https://docs.stripe.com/api/errors) and [advanced error handling](https://docs.stripe.com/error-low-level).

Logs may include Smart Court IDs, Stripe request ID, Stripe object ID, error type/code, and correlation ID. Logs must not include API keys, webhook secrets, client secrets, PaymentMethod details, raw bank details, or full webhook bodies.

## 6. Client funding API changes

### Request

`FundMilestoneRequest` keeps `PaymentMethodReference`, which must be a Stripe-created `pm_...` token in Stripe mode. Validation stays provider-neutral at the feature boundary; Stripe-specific shape validation happens in the adapter.

`RetryPaymentRequest` gains `PaymentMethodReference` and becomes a client-owned retry operation after Smart Court verifies the same contract client. A finance administrator must not supply or reuse a client's card token.

### Response

`FundAsync` and `RetryAsync` will return a new `FundingOperationDto`:

```text
PaymentTransactionId
MilestoneId
Status: Completed | Processing | RequiresCustomerAction
ClientSecret: optional, authenticated client only
Payment: optional PaymentDto when the hold already exists
OccurredAt
```

- Immediate `succeeded`: HTTP 200 with `Completed` and `PaymentDto`.
- `requires_action`: HTTP 200/202 with client secret; the frontend calls Stripe.js `confirmCardPayment`.
- `processing`: HTTP 202.
- Confirmed failure: existing business-error response; milestone returns to `AwaitingFunding`.

The HTTP idempotency record represents acceptance of the initiation request, not eventual bank settlement. It will store the safe response DTO so a replay cannot create a second PaymentIntent. Webhook completion updates the payment transaction and hold without requiring the HTTP idempotency row to remain `Processing`.

For the Swagger/demo happy path, Stripe's predefined test PaymentMethod `pm_card_visa` can exercise immediate success without handling raw card data. A browser-based 3DS demonstration still requires Stripe.js/Elements in the frontend.

## 7. Lawyer payout-account lifecycle

### Persistence: `LawyerPayoutAccount`

A new payment-feature entity will contain:

- Smart Court ID;
- lawyer user ID, unique per provider;
- provider code;
- provider connected-account ID, unique per provider;
- normalized onboarding status;
- details-submitted flag;
- transfers-enabled flag;
- payouts-enabled flag;
- sandbox/live flag;
- country and default provider currency;
- safe masked external-account label if Stripe supplies one;
- last provider status/error code;
- last synchronized UTC time;
- created/updated UTC times and row version.

No account number, routing number, IBAN, card number, identity document, or Stripe client secret will be stored.

### Provider-neutral interface

Add `ILawyerPayoutAccountProvider` with domain-named operations:

- create a recipient account;
- create a hosted onboarding link;
- retrieve recipient capability status;
- create a hosted account-management/login link when supported;
- set platform-controlled payout scheduling.

### API endpoints

Add authenticated lawyer endpoints under `api/wallet/payout-account`:

- `GET` — current onboarding/capability status;
- `POST onboarding-link` — create/reuse connected account and return a single-use Stripe-hosted onboarding URL;
- `POST dashboard-link` — optional Express account-management link after onboarding.

For the manually created Phase 2 test account, add a sandbox-only Finance/Super Administrator endpoint that links an existing `acct_...` to a lawyer only after retrieving it from Stripe and verifying:

- `livemode == false`;
- it is not already linked in Smart Court;
- it is controlled by the current platform;
- any existing `smart_court_lawyer_user_id` metadata is empty or matches;
- its email/identity is not exposed in the response;
- the provider metadata is then stamped with the lawyer ID.

This bootstrap endpoint will refuse to run when `StripeOptions.SandboxOnly` is false.

`account.updated` webhooks and explicit status refreshes keep the local profile synchronized. Withdrawals and releases both fail closed when the account is not ready.

## 8. Release, refund, and dispute changes

### `EscrowReleaseService`

- Find the completed deposit transaction linked to the hold.
- Find the contract lawyer's enabled payout account.
- Create the release `PaymentTransaction` using `hold.NetAmount`, not `hold.GrossAmount`.
- Send original PaymentIntent/Charge and connected-account IDs.
- Keep the existing exact UTC expiry, dispute guard, serializable transaction, idempotency reservation, retry policy, append-only ledger, wallet transition, and outbox event.
- Retain `hold.PlatformFeeAmount` on the platform and record it only in Smart Court's ledger.

### `ContractTerminationSettlementService`

- Resolve the original completed deposit for each hold.
- Send its PaymentIntent ID with every full refund.
- Treat `pending` as processing, not success or failure.
- Finalize the refund ledger only after Stripe reports `succeeded`.

### `DisputeService`

- Full refund: refund full EGP hold against original PaymentIntent.
- Full release: transfer the lawyer release amount only; retain platform fee.
- Partial split: refund the exact client EGP amount and transfer the exact lawyer-net allocation after provider conversion.
- Provider calls remain outside long-held SQL transactions as the current recovery design intends.
- If either leg is pending/unknown, settlement remains recoverable and no final ledger state is claimed.
- A refund never automatically reverses a transfer. Current domain guards must prevent refund settlement after a release; any external reversal becomes a finance incident.

## 9. Withdrawal service changes

`WalletService.WithdrawAsync` will:

1. authenticate the lawyer;
2. load the lawyer wallet and enabled payout account;
3. reserve available EGP balance atomically;
4. persist the withdrawal, connected-account snapshot, and provider metadata;
5. create a Stripe Payout while acting as that connected account;
6. persist `po_...` immediately;
7. return `Processing` for `pending`/`in_transit` without releasing the reservation;
8. complete only for `paid`;
9. restore the reservation once for `failed`/`canceled`;
10. retain unknown outcomes for reconciliation/manual action.

Add `GET api/wallet/withdrawals/{withdrawalId}` so the lawyer can observe processing, completed, or failed status after the initial HTTP request.

## 10. Webhook controller and processing design

### Endpoints

Add an anonymous, rate-limited `StripeWebhooksController` with two raw-body endpoints:

- `POST /api/webhooks/stripe/platform`
- `POST /api/webhooks/stripe/connect`

The separate routes use separate secrets and avoid guessing which secret signed an event.

### Verification

- Enforce the configured maximum raw-body size before deserialization.
- Read the body exactly once without JSON reserialization.
- Read `Stripe-Signature`.
- Verify through Stripe.net `EventUtility.ConstructEvent` with the route-specific `whsec_...` and a 300-second default tolerance.
- Reject invalid signatures with 400.
- Do not depend on a fixed IP allowlist; the Stripe signature is authoritative.
- Reject live events while `SandboxOnly` is true.
- Never bind a provider-defined DTO before signature verification.

### Normalization and idempotency

The Stripe verifier converts the SDK Event to a provider-neutral envelope. Processing will:

- deduplicate by provider + Stripe `event.id`;
- also protect terminal transitions by provider object ID and operation;
- map through persisted object IDs and metadata, never client-supplied Smart Court IDs alone;
- tolerate duplicate and out-of-order delivery;
- retrieve the current Stripe object when an event could regress local state;
- acknowledge irrelevant, valid events with 200 `Ignored`;
- return non-2xx for a transient internal failure so Stripe retries.

Stripe explicitly does not guarantee event order and recommends recording event IDs to handle duplicates. See [Stripe webhook best practices](https://docs.stripe.com/webhooks).

### Event set

Platform endpoint:

- `payment_intent.succeeded`
- `payment_intent.processing`
- `payment_intent.payment_failed`
- `payment_intent.canceled`
- `refund.created`
- `refund.updated`
- `refund.failed`
- `transfer.created`
- `transfer.updated`
- `transfer.reversed`

Connected-account endpoint:

- `account.updated`
- `payout.created`
- `payout.updated`
- `payout.paid`
- `payout.failed`
- `payout.canceled`

The exact event names will be verified against Stripe.net 52.1.1 while coding; only SDK-supported/public event types will be subscribed.

### Existing webhook model

Replace the custom `X-Payment-*` signature and custom `PaymentWebhookRequest` flow for Stripe. Mock-provider webhook support can remain behind its own test-only route if tests still require it.

`PaymentWebhookEvent` will be generalized to support account events that do not belong to a `PaymentTransaction`:

- nullable payment transaction ID;
- nullable withdrawal ID;
- nullable payout-account ID;
- provider code;
- provider event ID and type;
- provider object ID;
- scope (`Platform` or `ConnectedAccount`);
- received/processed UTC timestamps and processing result.

The event record remains append-only.

## 11. Configuration

### `StripeOptions`

Add `SmartCourt/Providers/Payments/Stripe/StripeOptions.cs` and bind it with `IOptions<StripeOptions>`:

```json
{
  "PaymentProvider": {
    "UseMockProvider": true,
    "ProviderCode": "MockPaymentProvider",
    "ProcessingSlaMinutes": 1440,
    "WebhookMaximumBodySizeBytes": 65536,
    "Stripe": {
      "SecretKey": "",
      "PublishableKey": "",
      "PlatformWebhookSecret": "",
      "ConnectWebhookSecret": "",
      "WebhookToleranceSeconds": 300,
      "MaxNetworkRetries": 2,
      "ConnectReturnUrl": "https://localhost:5173/wallet/payout-account/return",
      "ConnectRefreshUrl": "https://localhost:5173/wallet/payout-account/refresh",
      "DefaultConnectedAccountCountry": "US",
      "SandboxOnly": true
    }
  }
}
```

The main and development appsettings files receive the same non-secret shape. The default remains the mock provider until keys are available, allowing builds and ordinary tests to run without Stripe.

When Stripe is selected, startup validation requires:

- `ProviderCode == StripeConnect`;
- `UseMockProvider == false`;
- `sk_test_...` secret while `SandboxOnly` is true;
- `pk_test_...` publishable key;
- both `whsec_...` values before webhook-enabled execution;
- absolute HTTPS return/refresh URLs, with localhost HTTPS allowed in development;
- webhook tolerance between 60 and 600 seconds;
- network retries between 0 and 5.

Secrets will be supplied later without editing tracked JSON:

```powershell
dotnet user-secrets set "PaymentProvider:Stripe:SecretKey" "sk_test_REDACTED" --project SmartCourt
dotnet user-secrets set "PaymentProvider:Stripe:PublishableKey" "pk_test_REDACTED" --project SmartCourt
dotnet user-secrets set "PaymentProvider:Stripe:PlatformWebhookSecret" "whsec_REDACTED" --project SmartCourt
dotnet user-secrets set "PaymentProvider:Stripe:ConnectWebhookSecret" "whsec_REDACTED" --project SmartCourt
dotnet user-secrets set "PaymentProvider:UseMockProvider" "false" --project SmartCourt
dotnet user-secrets set "PaymentProvider:ProviderCode" "StripeConnect" --project SmartCourt
```

The values above are placeholders only. Actual secrets must never be committed or posted in chat.

This follows the repository's `add-app-setting` rule: feature services do not inject `IConfiguration`.

## 12. Dependency injection and SDK

### Package

Add the current stable official SDK:

```xml
<PackageReference Include="Stripe.net" Version="52.1.1" />
```

Version 52.1.1 is the current stable release identified during Phase 3; prerelease 52.2 packages will not be used. Stripe recommends the `StripeClient` instance pattern instead of global `StripeConfiguration`. See the [official stripe-dotnet repository](https://github.com/stripe/stripe-dotnet).

### Registration

`DependencyInjection.cs` will:

- remove the unconditional `|| true` mock registration;
- use an explicit provider-selection switch;
- bind and validate `StripeOptions` only when Stripe is selected;
- create one configured `StripeClient` without a global static API key;
- register one scoped `StripePaymentProvider` instance;
- expose that same scoped instance as `IPaymentProvider` and `IPaymentReconciliationProvider`;
- register the payout-account and webhook verifier interfaces;
- keep Mock as the safe default only when explicitly configured;
- leave the current Paymob placeholder unselected and unchanged unless compilation requires contract updates.

`PaymentProviderStartupValidator` will restore strict validation: exactly one operational provider and one reconciliation provider, and both must resolve to the same scoped object.

## 13. Persistence and migration

### `PaymentTransaction`

Add:

- `ProviderStatus`;
- `ProviderObjectType`;
- `ProviderRelatedTransactionId` for source Charge;
- `ProviderAmountMinor`;
- `ProviderCurrency`.

Indexes will support provider + object ID lookup while preserving the existing unique transaction ID constraint.

### `WithdrawalRequest`

Add:

- `LawyerPayoutAccountId`;
- connected-account ID snapshot;
- provider status;
- provider minor-unit amount and currency.

The snapshot preserves auditability if a lawyer later changes their Stripe account.

### `LawyerPayoutAccount`

Add the entity, enum, EF configuration, DbSet, timestamp validation registration, unique ownership/provider indexes, capability checks, and restrictive foreign keys.

### `PaymentWebhookEvent`

Generalize the foreign keys and event metadata as described in Section 10 while preserving append-only behavior and provider-event uniqueness.

### Migration

Create a single descriptive EF Core migration after all model changes. Migration tests will verify:

- new columns and tables;
- EGP business constraints remain intact;
- provider currency is a separate three-letter value;
- unique lawyer/provider and provider-object constraints;
- safe nullable backfill for existing mock transactions;
- webhook events can reference a payment, withdrawal, payout account, or a permitted account-only event.

## 14. Exact file plan

### Add

- `SmartCourt/Providers/Payments/Stripe/StripeOptions.cs`
- `SmartCourt/Providers/Payments/Stripe/StripePaymentProvider.cs`
- `SmartCourt/Providers/Payments/Stripe/StripeWebhookVerifier.cs`
- `SmartCourt/Providers/Payments/Stripe/StripePayoutAccountProvider.cs` or a provider-internal collaborator
- `SmartCourt/Infrastructure/Providers/Payments/Interfaces/ILawyerPayoutAccountProvider.cs`
- `SmartCourt/Infrastructure/Providers/Payments/Interfaces/IPaymentWebhookVerifier.cs`
- `SmartCourt/Infrastructure/Providers/Payments/Models/ProviderPayoutAccountModels.cs`
- `SmartCourt/Infrastructure/Providers/Payments/Models/ProviderWebhookModels.cs`
- `SmartCourt/Features/Payments/Entities/LawyerPayoutAccount.cs`
- `SmartCourt/Features/Payments/Enums/LawyerPayoutAccountStatus.cs`
- `SmartCourt/Persistence/Configurations/LawyerPayoutAccountConfiguration.cs`
- `SmartCourt/Features/Payments/PayoutAccounts/ILawyerPayoutAccountService.cs`
- `SmartCourt/Features/Payments/PayoutAccounts/LawyerPayoutAccountService.cs`
- `SmartCourt/Features/Payments/PayoutAccounts/LawyerPayoutAccountsController.cs`
- `SmartCourt/Features/Payments/Webhooks/StripeWebhooksController.cs`
- one EF Core migration and updated model snapshot
- Stripe provider, webhook, payout-account, lifecycle, registration, and migration tests

### Modify

- `SmartCourt/SmartCourt.csproj`
- `SmartCourt/Infrastructure/Providers/Payments/Interfaces/IPaymentProvider.cs`
- `SmartCourt/Infrastructure/Providers/Payments/Interfaces/IPaymentReconciliationProvider.cs`
- `SmartCourt/Infrastructure/Providers/Payments/Models/PaymentProviderRequests.cs`
- `SmartCourt/Infrastructure/Providers/Payments/Models/ProviderOperationOutcome.cs`
- `SmartCourt/Infrastructure/Providers/Payments/Models/ProviderResult.cs`
- `SmartCourt/Features/Payments/DTOs/PaymentRequests.cs`
- `SmartCourt/Features/Payments/DTOs/PaymentResponseDtos.cs`
- `SmartCourt/Features/Payments/Validators/FundMilestoneRequestValidator.cs`
- `SmartCourt/Features/Payments/Validators/RetryPaymentRequestValidator.cs`
- `SmartCourt/Features/Payments/Validators/CreateWithdrawalRequestValidator.cs`
- `SmartCourt/Features/Payments/Entities/PaymentTransaction.cs`
- `SmartCourt/Features/Payments/Entities/WithdrawalRequest.cs`
- `SmartCourt/Features/Payments/Entities/PaymentWebhookEvent.cs`
- `SmartCourt/Persistence/ApplicationDbContext.cs`
- corresponding payment persistence configurations
- `SmartCourt/Features/Payments/Escrow/IPaymentEscrowService.cs`
- `SmartCourt/Features/Payments/Escrow/PaymentEscrowService.cs`
- `SmartCourt/Features/Payments/Escrow/EscrowReleaseService.cs`
- `SmartCourt/Features/Payments/Escrow/PaymentsController.cs`
- `SmartCourt/Features/Payments/Webhooks/IPaymentWebhookService.cs`
- `SmartCourt/Features/Payments/Webhooks/PaymentWebhookService.cs`
- `SmartCourt/Features/Payments/Wallets/IWalletService.cs`
- `SmartCourt/Features/Payments/Wallets/WalletService.cs`
- `SmartCourt/Features/Payments/Wallets/WalletsController.cs`
- `SmartCourt/Features/Payments/Settlement/ContractTerminationSettlementService.cs`
- `SmartCourt/Features/Disputes/DisputeManagement/DisputeService.cs`
- both payment and wallet reconciliation services
- `SmartCourt/Providers/Payments/MockPaymentProvider.cs`
- existing Paymob placeholder only as required to compile the expanded provider contract
- `SmartCourt/Providers/Payments/PaymentProviderStartupValidator.cs`
- `SmartCourt/Providers/Payments/PaymentProviderOptions.cs`
- `SmartCourt/DependencyInjection.cs`
- `SmartCourt/appsettings.json`
- `SmartCourt/appsettings.Development.json`
- existing provider, payment, dispute, wallet, migration, and architecture tests

The exact migration filename is generated by EF Core and therefore is not predetermined.

## 15. Test and verification plan

### Unit tests

- EGP decimal-to-minor conversion, including rounding rejection beyond two decimals.
- Provider settlement allocation and lawyer-net ratio rounding.
- Every PaymentIntent, Refund, Payout, and exception mapping row in this document.
- Metadata generation and maximum lengths.
- No secret/client-secret logging.
- Connected-account readiness and ownership guards.

### Provider adapter tests

- Correct Stripe PaymentIntent create/confirm parameters and idempotency key.
- 3DS action response.
- Full and partial Refund parameters.
- Transfer uses source Charge, transfer group, connected account, and net—not gross—allocation.
- Payout uses `RequestOptions.StripeAccount` and no browser destination.
- Status retrieval by exact Stripe object ID.

### Webhook tests

- Valid platform and Connect signatures.
- Wrong secret, altered body, stale timestamp, oversized body, and live-event rejection.
- Duplicate event delivery.
- Two different events for the same object/status.
- Out-of-order events and current-object retrieval.
- Payment success/failure, refund success/failure, payout paid/failed/canceled, account capability updates, and transfer reversal escalation.

### Application integration tests

- Immediate successful deposit creates one hold and one ledger deposit.
- Action-required deposit creates no hold until success webhook.
- Decline returns milestone to awaiting funding.
- Exact existing 7-day review and 14-day post-acceptance hold remain unchanged.
- Release sends only lawyer net and moves pending to available once.
- Full contract-termination refund.
- Dispute full refund, full release, and partial split.
- Withdrawal reservation remains while Stripe payout is pending.
- `payout.paid` completes once.
- `payout.failed` restores balance once.
- Unknown provider outcomes never create duplicate financial writes.
- Idempotent HTTP replay and Stripe idempotency replay.
- Finance force-release remains usable for the time-compressed demo and retains its authorization.

### Build gates

Run at minimum:

```powershell
dotnet restore SmartCourt.sln
dotnet build SmartCourt.sln --no-restore
dotnet test SmartCourt.Tests/SmartCourt.Tests.csproj --no-build
```

If the full suite contains unrelated pre-existing failures, they will be reported separately; all changed and newly added payment tests must pass.

### Optional real-sandbox smoke test after keys arrive

1. Link/onboard the test lawyer.
2. Fund using `pm_card_visa`.
3. Verify PaymentIntent and hold.
4. Accept milestone and use the existing authorized force-release operation for the demo.
5. Verify Stripe Transfer and Smart Court available wallet balance.
6. Request withdrawal.
7. Forward/send `payout.paid` and verify completion.
8. Run separate full-refund, partial-refund, 3DS, decline, and failed-payout scenarios.

No real-sandbox test is required to compile the implementation, and no secret will be embedded in a test fixture.

## 16. Frontend boundary

The .NET implementation will expose everything required for a frontend, but a browser 3DS flow requires Stripe.js or React Stripe.js:

- create/tokenize the card without sending card data to Smart Court;
- send the resulting PaymentMethod ID to the funding endpoint;
- call `confirmCardPayment` when `RequiresCustomerAction` returns a client secret;
- poll/query the Smart Court payment result or respond to application notifications.

The current `SmartCourtFE` package does not contain Stripe dependencies or payment screens found during this review. Phase 4, as originally scoped, is the C# API/provider implementation. A successful no-3DS lifecycle can be demonstrated through Swagger with Stripe's test PaymentMethod. A full browser payment screen is a separate frontend change unless explicitly added to Phase 4 scope.

## 17. Demo and production boundaries

The implementation will be complete for a Stripe sandbox MVP, but it must continue to display these boundaries:

- test keys only;
- simulated cards, connected accounts, balances, and payouts;
- no claim of regulated escrow;
- no claim that an Egypt-incorporated Smart Court platform can activate Stripe live;
- no real lawyer onboarding or bank payout;
- production selection remains subject to country, legal-services underwriting, Connect, cross-border, currency, fee, and regulatory approval.

## 18. Phase 3 approval gate

No C# code, package reference, configuration block, entity, migration, controller, or test described in this plan will be created until this plan receives explicit approval.

Approval phrase:

> Approve Stripe Connect implementation plan and proceed to Phase 4.


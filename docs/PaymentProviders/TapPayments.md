# Tap Payments — Provider Research (Escrow-Style Legal Platform)

> Scope: collect → hold ~14 days → release payout to lawyer → refunds/withdrawals.
> Company focus: MENA (incl. Egypt). Research date: August 2026.
> **Bottom line:** Tap's **Marketplace / Delayed Split** is genuinely escrow-like — funds land in the **marketplace's Tap wallet** and sit there (payout disabled) until you release. BUT: no programmatic payout initiation (dashboard / Tap payout-cycle only), card-level auth holds cap at **7 days**, marketplace access is sales-gated, and **Egypt onboarding/payout is unconfirmed** (docs conflict).

---

## 1. Company, regulation, geography

| Item | Finding | Reference |
|---|---|---|
| What | Tap Payments, founded 2014; 100k+ businesses; GCC + Egypt + Jordan + Lebanon | https://www.tap.company/en-us/company/about |
| Saudi | SAMA-licensed; mada-certified | https://blog.tap.company/tap-payments-secures-electronic-payment-service-provider-license-2/ |
| Kuwait | CBK Electronic Payment Service Provider license (Mar 2024) | https://blog.tap.company/tap-payments-secures-electronic-payment-service-provider-license-2/ |
| UAE | CBUAE Retail Payment Services License (Apr 2025) | https://blog.tap.company/central-bank-of-the-uae-awards-tap-payments-full-license-gcc/ |
| Qatar / Oman / Bahrain | Licensed (GCC completion Apr 2025) | https://blog.tap.company/central-bank-of-the-uae-awards-tap-payments-full-license-gcc/ |
| **Egypt** | **CBE Payment Facilitator & Service Provider license (May 2025)** — the key license for us | https://blog.tap.company/tap-payments-egypt-central-bank-license-2025/ |
| ⚠️ Egypt onboarding | Support article (Apr 2025) still says "no new merchant from Egypt/Jordan/Lebanon" — **conflicts with the May 2025 license**; must confirm with sales | https://support.tap.company/en/support/solutions/articles/153000140298-what-countries-are-supported-by-tap-payments- |
| Currencies | 10 currencies (AED, BHD, EUR, GBP, KWD, OMR, QAR, SAR, USD…); **EGP not in 2024 list** though EGP appears in webhook docs | https://support.tap.company/en/support/solutions/articles/153000140299-what-currencies-are-supported-by-tap-payments- ; https://developers.tap.company/docs/webhook |

---

## 2. Escrow-relevant capabilities

### 2.1 The core = Marketplace / Delayed Split (this is the escrow mechanism)
- Charge includes optional `destinations.destination[]` (retailer id, amount, currency); remainder goes to the Marketplace account. — https://developers.tap.company/docs/marketplace-split-payments ; https://developers.tap.company/reference/destinations
- **Two models:** — https://developers.tap.company/docs/marketplace-overview
  - **Instant Split:** retailer share → retailer Tap wallet immediately (auto-payout per cycle).
  - **Delayed Split (escrow-like):** charge captured under **marketplace only**; **marketplace payout disabled; funds remain securely held in the marketplace wallet**; marketplace later calls the **Update Charge API** to allocate the retailer share → retailer wallet → auto-payout to retailer bank. You withdraw your own commission by creating yourself as a retailer with payout enabled.
- ⚠️ **Docs contradiction:** Overview says use Update Charge to allocate share, but Update Charge reference only documents `description/metadata/receipt`. **Confirm with Tap integration team.** — https://developers.tap.company/reference/update-a-charge
- Marketplace is **merchant of record and bears all liability (incl. chargebacks)**. — https://developers.tap.company/docs/marketplace-overview

### 2.2 Payouts (to the lawyer's bank)
- **No "initiate payout" endpoint.** Payouts API is read-only: Retrieve/List/Download payouts. Payouts run on Tap's standardized cycle, or **manually from dashboard after Tap disables auto-settlement**. — https://developers.tap.company/reference/payout ; https://developers.tap.company/docs/marketplace-getting-started
- Retailers need Tap **KYC approval** before their payouts are enabled (commercial registration, IDs, IBAN/bank statement); webhook notifies you when enabled. — https://developers.tap.company/docs/marketplace-overview
- ⚠️ **Payout country list (Oct 2024): KSA, Kuwait, Bahrain, UAE, Oman, Qatar — Egypt NOT listed.** — https://support.tap.company/en/support/solutions/articles/153000140204-what-countries-does-tap-process-payouts-to-
- Payout statuses via webhook: `PENDING, INITIATED, FAILED, PAID_OUT`. — https://developers.tap.company/reference/webhook-api

### 2.3 Authorize & Capture (card-level hold)
- Authorize places a **hold on the card**; capture later via Create Charge `source.id=authorize_id`; auto `.auto.time` `VOID/CAPTURE`. — https://developers.tap.company/reference/create-an-authorize ; https://developers.tap.company/docs/authorize-and-capture
- ⚠️ **`auto.time` max 168h (7 days)** for normal cards (720h only for tokenized/saved cards) → **14-day card-hold impossible**; use Delayed Split wallet-hold instead. — https://developers.tap.company/reference/create-an-authorize
- Not all methods support authorize. — https://developers.tap.company/docs/get-started

### 2.4 Refunds / void
- `POST /v2/refunds`: full or partial on a `charge_id`; requires amount/currency/reason; supports `destinations` for marketplace split-refunds + `reverse_destination`. — https://developers.tap.company/reference/create-a-refund
- Void/release: auto `VOID` or Update an Authorize. — https://developers.tap.company/reference/update-an-authorize

### 2.5 Settlement schedule
- UAE: settle 5 business days → weekly to bank; Min settle AED 100 corp / 1000 individual; fee AED 15 below min. — https://support.tap.company/en/support/solutions/articles/153000140304-when-are-payouts-made-to-bank-accounts-in-uae-
- Marketplace standard Visa/MC: **5 business days**. — https://developers.tap.company/docs/marketplace-overview

---

## 3. Sandbox / testing

- ✅ Free test mode; `sk_test_...`/`pk_test_...`; all resources in test; no real money. — https://developers.tap.company/docs/get-started
- Test cards for all schemes + decline triggers. — https://developers.tap.company/reference/testing-cards
- Test-mode nuance: card "won't be charged but everything else behaves as live." — https://developers.tap.company/reference/create-a-charge
- ⚠️ **KSA test mode validates real data** (Create Lead validates via third-party providers). — https://developers.tap.company/docs/marketplace-overview
- Authorize→capture / authorize→auto-void are schedulable in sandbox. — https://developers.tap.company/reference/create-an-authorize
- Payouts in test: only ledger/webhook/download simulation, **no bank transfer**. — https://developers.tap.company/reference/retrieve-a-payout

---

## 4. Costs (per-merchant / negotiated; no public rate card)

- No setup/monthly on standard onboarding; sandbox free. — https://apis.io/plans/tap-payments/tap-payments-plans-pricing/
- Kuwait official T&C: KNET 1%+100 fils; local Visa/MC 2.75%+100 fils; global 3.75%+100 fils; monthly up to 25 KWD. — https://web-account.tap.company/kw/en/terms-conditions
- KSA mada 1% capped SAR 200 (since Sep 2023). — https://blog.tap.company/new-mada-fees-saudi/
- Egypt: third-party ~**2–2.75%** typical. — https://paymentproviders.io/egyptian-payment-providers
- Marketplace/Tap Connect: **custom, contact sales**. — https://gulfsaasreview.com/review/tap-payments

---

## 5. Webhooks & signature

- `POST` raw JSON to `post.url`; fires for charges, authorizes, refunds, invoices, payouts; **2 retry attempts**; invalid SSL blocks localhost. — https://developers.tap.company/docs/webhook
- **HMAC-SHA256** verification: concat in exact order `x_id+id+x_amount+amount+x_currency+currency+x_gateway_reference+reference.gateway+x_payment_reference+reference.payment+x_status+status+x_created+created`, HMAC-SHA256 with secret API key, compare to `hashstring` header. — https://developers.tap.company/docs/webhook

---

## 6. Risk / limitation highlights for 14-day escrow

1. **No API-triggered payout.** Release = dashboard / Tap cycle (up to 5 extra business days). Use Delayed Split and release via Update Charge at day-14.
2. **Card-level hold cap 7 days** → capture-at-day-0 & hold wallet instead.
3. **Marketplace is sales-gated** + negotiated payout cycle; not self-serve.
4. Funds in a **Tap wallet under your merchant account**; marketplace is merchant-of-record with full chargeback liability — not regulated escrow.
5. Docs contradiction on the release primitive (Update Charge).
6. **Egypt unconfirmed:** onboarding, EGP settlement, EGP payout rails; support page still says no new Egyptian merchants. Verify with Tap sales.

## 7. Verdict for Smart Court
- **Strongest "escrow-substitute" mechanics of the MENA players:** delayed split = collect to marketplace wallet → hold → release. Perfect fit for our Deposit/Release if payout rails confirmed.
- **Blocking/unverified:** Egypt merchant onboarding, EGP payout rails, programmatic release endpoint (Update Charge destinations), and payout to Kuwait.
- Target position: *staging-ready*, need **sales confirmation** before production.

---
*References verified Aug 2026. Tap has no public rate card; numbers above are official per-country T&C / third-party where flagged.*
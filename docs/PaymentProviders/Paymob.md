# Paymob — Payment Provider Research (Escrow-Style Legal Platform)

> Scope: collect from client → hold ~14 days → release payout to a lawyer → refunds/withdrawals.
> Company focus: Egypt-first. Research date: August 2026.
> **Bottom line:** Paymob is a payments *acquirer / Payment Facilitator (CBE license)*, **not an escrow agent** and does **not** offer a documented "held funds / escrow balance" product. A 14-day hold is achievable two ways — (1) card **Auth/Capture** (auto-void at 14 days, card-only) or (2) collect-full → hold in **your** merchant balance → push payout on release via **Paymob Payouts** API. See risk section.

---

## 1. Company, regulation, geography

| Item | Finding | Reference |
|---|---|---|
| HQ / founding | Cairo, founded 2015; ~390k merchants, Egypt-profitable since Q2 2024 | https://paymob.com/ ; https://paymob.com/en/about-us |
| Egypt license | First-ever **CBE Payment Facilitator license (2018)** — aggregator/PF role, **not** bank/escrow license | https://paymob.com/en/about-us ; https://www.heliosinvestment.com/investments/paymob |
| Other licenses | UAE: CBUAE Retail Payment Services (2025); Oman: CBO PSP (2023); KSA: SAMA PTSP; Pakistan: USL | https://thefintechtimes.com/paymob-secures-full-uae-licence-ceo-talks-digital-economy-inflection-point-and-payment-growth/ ; https://www.zawya.com/en/press-release/companies-news/paymob-secures-central-bank-of-omans-psp-license-we9hximo |
| **Kuwait / Bahrain / Qatar** | **Not present** (no company/entity). Kuwait especially has no operations | https://www.payselect.ae/providers/paymob |
| Currencies | EGP/EUR via Integration ID; merchant must match currency of chosen integration | https://developers.paymob.com/paymob-docs/developers/intention-apis/create-intention |

---

## 2. Product map relevant to your flow

| Product | What it does | Reference |
|---|---|---|
| Paymob Accept | Online gateway, Checkout, Payment Links, Subscriptions, Installments | https://paymob.com/en/online-payment |
| **MarketPlace** | "Accept payments and distribute payouts… complete control over payout timing, money movement" — this is the platform/payout product | https://paymob.com/en/marketplace |
| **Paymob Send / Payouts** | Mass payouts (freelancers, suppliers, employees) to wallets, bank cards, instant bank, Aman | https://paymob.com/en/payouts ; https://payouts.paymobsolutions.com/docs/ |
| Split Amount / Split Payment | Split at-payment-time among merchant + sub-accounts | https://developers.paymob.com/paymob-docs/payments-and-features/core-features |
| Convenience Fee | Platform adds %/fixed surcharge per method | same URL |

---

## 3. Escrow / hold / release capabilities (exact)

### 3.1 How a platform "collects and holds"
- The Marketplace model is **merchant-of-record**: money lands in **your** settlement balance; *you* control when/how sellers get paid. Paymob holds it, **no FBO/trust/escrow account**.
  - "Control your funds flow… complete control over onboarding, **payout timing**, complex money movement." — https://paymob.com/en/marketplace
- So the 14-day hold is one of two designs:
  1. **Auth/Capture (card only)** — Card authorized & held on customer's card; **auto-void after 14 days** if not captured. Window fits exactly.
  2. **Collect-now-pay-later** — charge full amount to your merchant balance; you (platform) keep DB ledger 14 days, then push payout to lawyer via Send API.

### 3.2 On-demand payout to a receiver (lawyer)
**Yes — Paymob Payouts (Send) API** (`POST {ENV}/disburse/`):
- Instant disbursement to a receiver that does **not** need a merchant account.
- Issuers: `vodafone`, `etisalat`, `orange`, `bank_wallet`, `bank_card`, `instant_bank`, `post` (Egypt Post). — https://payouts.paymobsolutions.com/docs/instant_cashin_api/
- Requirements: recipient `national_id` **required** (Egypt); bank transfer needs live `msisdn` or IBAN + `full_name` + `bank_code` + `bank_transaction_type`; minimum payout for `instant_bank` = **112 EGP**. — same URL
- Bulk: `disburse/bulk_transaction/` + inquiry; budget model (pre-fund via top-up). — https://apis.io/apis/paymob/paymob-disbursement-api/ ; https://payouts.paymobsolutions.com/docs/budget_inquiry/
- ⚠️ **No documented "scheduled/delayed release" field** — you trigger the disbursement when you want.

### 3.3 Auth/Capture (hold) — duration
- Set AUTH/payment_type in Intention → funds held → `POST /api/acceptance/capture` to finalize (full/partial) or `void_refund/void` to cancel. **Not captured within 14 days ⇒ auto-void.** — https://developers.paymob.com/paymob-docs/payments-and-features/core-features/auth-capture ; https://github.com/PaymobAccept/API-Postman-Collections (README "Refund, Void & Capture")
- ⚠️ **Card-only.** No auth/hold for wallets, BNPL, kiosk, A2A.

### 3.4 Refund / Void / Chargeback
- **Refund:** `POST /api/acceptance/void_refund/refund` — full or partial; cards & wallets; **NOT** most BNPL/kiosk/installment. — https://apis.io/apis/paymob/paymob-refund-api/
- **Void:** `POST /api/acceptance/void_refund/void` (card only, pre-settlement). — https://apis.io/apis/paymob/paymob-void-api/
- **Chargeback:** scheme-driven, merchant (you) bears; evidence via dashboard; CBE oversight.

### 3.5 Settlement to you
- Egypt default **weekly**; card / Apple-Google Pay T+1; UAE daily settlement add-on ~AED 210/mo. — https://paymob.com/en/pricing ; https://woocommerce.com/products/paymob/
- ⚠️ Weekly settlement + your 14-day window means the lawyer payout may rely on **pre-funded budget** (top-up) to release right after day 14.

---

## 4. Sandbox / testing

- ✅ **Free self-serve sandbox:** split Test/Live by which keys/Integration IDs you use; same base URLs. — https://developers.paymob.com/paymob-docs/getting-started/overview
- **Official test cards:** Mastercard `5123456789012346`, `5123450000000008`; Visa `4111111111111111`; wallet `01010101010`; MPin/OTP 123456; expiry 01/39 CVV 123. — https://developers.paymob.com/paymob-docs/need-help/faq/test-credits-credentials
- Payouts staging endpoints exist per country (Egypt `stagingpayouts.paymobsolutions.com`). — https://github.com/PaymobAccept/API-Postman-Collections
- Webhook testing tool available. — https://developers.paymob.com/paymob-docs/developers/webhook-callbacks-and-hmac/webhook-testing-tool
- Live requires **commercial approval** (business review, contract, data verification). — https://community.paymob.com/t/the-final-step-to-start-accepting-online-payments/62

---

## 5. Costs (documented)

- **Egypt cards (public):** 2.75% + 3 EGP; no setup/monthly; weekly settlement. — https://paymob.com/en/pricing ; https://paymob.com/en/online-payment
- **UAE public:** 2.9% + 1 AED; daily settlement add-on ~AED 210/mo; chargeback ~AED 75 (reseller). — https://paymob.ae/en/online-payment ; https://www.payselect.ae/providers/paymob
- Third-party range 2.5–3.25% by local/intl card (confirm contract). — https://samirgeorge.com/writing/payment-gateways-mena/
- Payouts (Send) are metered **per disbursement + VAT**; optional `customer_bears_fees=true`. — https://payouts.paymobsolutions.com/payouts
- Onboarding: commercial registration, tax card, owner IDs, company IBAN, store link; approval 2–5 business days. — https://community.paymob.com (onboarding guide) ; https://m.media-amazon.com/images/G/01/APS/onboarding/legal_documents/saudi/legal-docs_egypt.pdf

---

## 6. Webhooks & signature

- **HMAC-SHA512** over the concatenation of **20 fields (no separator, set order)**, hex digest, sent as **`hmac` query param** (not header, not body hash). — https://developers.paymob.com/paymob-docs/developers/webhook-callbacks-and-hmac ; algorithm walk-through https://hookdeck.com/webhooks/platforms/guide-to-paymob-webhooks-features-and-best-practices
- **No event names** — `type` always `TRANSACTION`; read `success`, `is_capture`, `is_refunded`, `is_voided`, `pending`. Callbacks sent only on success/decline. — same URLs

---

## 7. International / can a Kuwaiti or UAE lawyer be paid?

- Paymob countries: Egypt, KSA, UAE, Oman, Pakistan. **No Kuwait/Bahrain/Qatar.** — https://paymob.com/
- Paymob Payouts (Send): **Egypt, UAE, KSA only.** — https://github.com/PaymobAccept/API-Postman-Collections (README)
- Payout rails are **per-country + require national_ID (Egypt subject)**; no cross-border payout rail documented. A Kuwaiti lawyer **cannot** currently be paid via Paymob; a UAE lawyer would need its own UAE rail (and inter-country EGP→AED movement is not a documented feature — you'd move funds yourself after settlement). — https://payouts.paymobsolutions.com/docs/instant_cashin_api/

---

## 8. Weaknesses / risks for 14-day hold + lawyer payout

1. **No escrow/trust/held-account product.** Funds are either card-auth holds or your own settled balance. Regulatory exposure: holding third-party money may require EACH own trust/escrow/money-transmission authorization; being a CBE PT is not a legally segregated escrow title. — https://paymob.com/en/about-us
2. **Auth/Capture is card-only; window = 14 days exactly.** Wallets/kiosks/BNPL cannot be held. Missing the 14-day capture ⇒ auto-void ⇒ cash returns to client. — docs above
3. **Split-at-payment conflicts with hold.** If Split Amount is enabled, seller's share leaves at payment time — cannot hold. You must collect-all and pay-later, without auto-split. 
4. **Weekly settlement float.** You may not have balance to release on day-14 without a pre-funded budget/top-up; refunds/chargebacks could create negative balance after you've already paid a lawyer.
5. **Refund limitations.** Kiosk/BNPL/installment can't be refunded; chargeback liability sits with you.
6. **No per-settlement / payout-settled webhooks** — you track owed-per-lawyer state in your DB.
7. **Geography.** No Kuwait/Bahrain; payout rails limited to EG/UAE/KSA; national-ID requirement.
8. **Feature gating.** Auth/Capture, Split, Subscriptions are enabled per-account by Paymob — verify in sandbox before relying.

---

## 9. Verdict for Smart Court

- **Use-case fit (Egypt-only):** 
  - Card escrow: **Intention API + Auth/Capture** with DC (day-14 monitor) to capture/void/refund reliably.
  - Wallet/instalment escrow: collect-full → hold in ledger → release with **Payouts (Send) Instant-Cashin** (matches your `Withdraw`).
  - Refund/void for the client branch.
- **Before build:** confirm with Paymob sales: (a) payout API availability + fees/limits, (b) true balance-hold release-with-API vs console-only, (c) per-receiver KYC, (d) chargeback-reverse mechanics, (e) whether your holding model needs a separate escrow arrangement under CBE rules.

---
*References verified Aug 2026; where third-party aggregator numbers differ from Paymob's contract, the contract prevails.*
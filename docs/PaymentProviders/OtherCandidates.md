# Other Candidates — Regional & Global Providers (Escrow-Style Legal Platform)

> Covers: **Telr, MyFatoorah, HyperPay, YallaPay, CyberSource, Payoneer, Escrow.com, Tipalti, dLocal**.
> Research date: August 2026. ✅ = verified · ⚠️ = partially verified/secondary · ❌ = unverified · ⛔ = confirmed unavailable.

---

## A) Telr — telr.com

- **Region:** Dubai HQ; officially serves **UAE, KSA, Jordan, Bahrain** (country selector; **no Egypt page**); CBUAE in-principle license; 120+ currencies. https://www.zawya.com/en/press-release/companies-news/telr-granted-in-principle-approval-for-the-retail-payment-services-and-card-schemes-license-from-cbuae-klih704d ; https://telr.com/
- **Escrow-relevant:** ✅ Split Payments API (primary + sub-accounts, flat/%/remaining splits, payouts, reconciliation). ✅ Auth/Capture (split fields **not** on auth but supported at capture). ✅ Refund API. ⚠️ Split docs hosted under a Cashfree domain (`telr-docs.cashfree.com`) suggesting a white-label split engine.
- **Sandbox:** ✅ public docs + test cards (Visa/MC/Amex/JCB/MADA), simulated 3DS & decline codes. https://docs.telr.com/reference/test-cards
- **Settlement:** ✅ T+2 documented. https://telr-docs.cashfree.com/payments/split/vendor/dashboard/settlements
- **Egypt verdict:** ⛔ **no verified EGP acquiring** → not viable in Egypt today (re-confirm with sales).

---

## B) MyFatoorah — myfatoorah.com (Kuwait/GCC/Egypt)

- **Region:** Kuwait; serves **KSA, UAE, Qatar, Egypt, Bahrain, Oman, Jordan**; **Egypt live endpoint `api-eg.myfatoorah.com`** (Meeza supported). https://github.com/api-evangelist/myfatoorah/blob/main/README.md
- **Escrow-relevant:** ✅ **Multi-Vendors/suppliers** — onboard suppliers, per-method commissions, **TransferBalance push/pull**, split settlements from master account. https://www.myfatoorah.com/en/multi-vendors/ ; https://docs.myfatoorah.com/docs/transferbalance. ✅ Refund API (full/partial, supplier refunds). ⚠️ **On-demand bank payout API in Egypt unverified** ("Payout API" listed as Administrative service; public endpoints operate on MyFatoorah supplier balances, not raw IBAN).
- **Hold ~14d:** ⚠️ no classic auth/capture documented; hold would be supplier-balance retention before TransferBalance push.
- **Sandbox:** ✅ free demo at registertest.myfatoorah.com; OpenAPI+Postman, no onboarding to read docs; bearer-token auth (not HMAC; verify via GetPaymentStatus). https://docs.myfatoorah.com/docs/get-started ; https://docs.myfatoorah.com/docs/test-token
- **Pricing:** ⚠️ contact; community ~2.5% cards / ~2% KNET.
- **Verdict:** ✅ best **EGP collect+split** fit; ⚠️ **payout-to-lawyer in Egypt unverified**.

---

## C) HyperPay — hyperpay.com (KSA)

- **Region:** KSA HQ; serves **Jordan, Egypt, KSA, Lebanon, UAE**; EGP among currencies; SAMA-licensed, PCI DSS L1. https://www.hyperpay.com/faq/what-is-the-cost-of-becoming-a-merchant-with-hyperpay/ ; https://docs.ecosire.com/odoo-modules/hyperpay-gateway
- **Escrow-relevant:** ✅ COPYandPAY / prepareCheckout collection; ✅ **HyperSplit** — beneficiary gets funds directly **in bank within ~24h** (payouts to drivers/couriers/sellers/marketplaces). https://www.hyperpay.com/hypersplit/ ; ✅ Refund full/partial via `paymentType=RF`. ⚠️ Pre-auth/capture exists but hold-length unverified (issuer-dependent).
- **Sandbox:** ⚠️ sandbox account created by HyperPay (not self-serve); HMAC-SHA256 webhooks.
- **Pricing:** ❌ not public ("each country different"; ~2.5–3.5% industry).
- **Verdict:** ✅ viable regional candidate (EGP + bank payouts) — confirm **on-demand single payout + hold semantics** + Egypt onboarding.

---

## D) YallaPay — yallapay.net (UAE) / yalla.online (Egypt) — ⚠️ identity trap

- **Important:** the UAE **YallaPay gateway** (`yallapay.net`, "YALLA TECHNOLOGIES LLC") is a **different company** from the Egyptian **Yalla (yalla.online, Yalla Money)** consumer wallet.
- UAE gateway: collect via hosted request, **settled to UAE bank in dirhams**; ⚠️ no hold/split/refund API documented. https://yallapay.net/docs/payment/api
- Egypt Yalla: consumer wallet/prepaid, **no B2B escrow/payout API found**.
- **Verdict:** ⛔ **drop** — no Egypt viability; not matching the ask.

---

## E) CyberSource (Visa) — cybersource.com

- **What:** Visa's enterprise payment platform; **Payouts product = Original Credit Transaction (OCT)/Visa Direct** push to debit/prepaid/credit cards (≈30 min). https://www.cybersource.com/en-us/solutions/payment-acceptance/payouts.html
- **Egypt:** appears in domestic-OCT region list + as a `recipientInformation.country` where `purposeOfPayment` required for Visa → **push payouts to Egyptian cards defined**. https://developer.cybersource.com/docs/cybs/en-us/mandates/relnote/all/na/mandates-october-2023/oct-domestic.html
- Auth/capture ✅ true holds; refunds standard (OCT itself has no refunds). 
- **Access:** needs a **merchant account via an acquiring bank** in Egypt — not self-serve.
- **Sandbox:** ✅ free merchant sandbox. **Auth:** HTTP Signature (JWS), not HMAC.
- **Verdict:** ⚠️ realistic **card rails** (collect + auth/capture + Visa-Direct payout to Egyptian cardholders) but enterprise/bank-dependency, not an embedded marketplace wallet.

---

## F) Payoneer — payoneer.com (global payout rail, Egypt-friendly)

- **Coverage:** Mass Payouts API v4 (REST, OAuth2); **190+ countries, 70 currencies**; batch; payee KYC; sandbox. https://developer.payoneer.com/docs/mass-payouts-v4-getting-started.html ; https://www.payoneer.com/marketplace/mass-payouts-platform/
- **Egypt:** de facto standard receiving rail for Egyptian freelancers; withdraw to Egyptian banks **in EGP**, CBE-compliant. https://payoutmap.com/country/egypt ; https://xpay.app/blog/wise-or-payoneer-egypt
- **Role:** payout rail, **not an escrow holder** — you hold funds and call the API on release; pair with your own ledger/legal-trust accounting.
- **Verdict:** ✅ **best "pay-the-lawyer" leg** (incl. lawyers with Payoneer accounts), combined with an EG collector.

---

## G) Escrow.com — escrow.com (true escrow, worldwide but ⛔ Egypt)

- **True escrow:** ✅ regulated/licensed online escrow (US state escrow licences, trust accounts). https://www.escrow.com/escrow-licenses
- **API:** ✅ `api.escrow.com/2017-09-01/` + **sandbox `api.escrow-sandbox.com`**; create customer/transaction, fund, agree, **disburse**, cancel, webhooks (~26 events). Milestone & Contracted Services transaction types — **fits a lawyer engagement**. https://www.escrow.com/api/docs ; https://www.escrow.com/api/docs/disburse-transaction
- **Fees:** standard ≤$5k **2.6% (min $50)** scaling to 0.7%; **+3.05% when buyer pays by card/PayPal**; ACH free, intl wire $20, +$25 intermediary-bank fee for intl-wire buyers. https://www.escrow.com/fee-calculator
- **Currencies:** ⛔ **USD, AUD, EUR, GBP, CAD only — no EGP**. https://www.escrow.com/support/faqs/what-currencies-does-escrowcom-support
- **Countries:** ⛔ **Egypt NOT supported** (list: https://www.escrow.com/support/faqs/what-countries-regions-does-escrowcom-support).
- **Verdict:** ⛔ **not an Egypt solution** — only for non-Egypt pilots/cross-border clients. Two-party model (one seller per transaction, not multi-seller marketplace).

---

## H) Tipalti — tipalti.com (enterprise global payout)

- Mass-payment API: payees, mass payments, tax/compliance; 200+ countries, 120 currencies; sandbox. https://tipalti.com/mass-payments/payout-api/
- ⚠️ Egypt appears in their currency-conversion stats (local conversion 10.2%) → global ACH/wire to Egypt implied, but **no official EGP spec**; enterprise/sales-driven.
- **Verdict:** capable but overkill for MVP; confirm Egypt EGP rail with sales.

---

## I) dLocal — dlocal.com (bonus: explicit Egypt payouts)

- **Payouts in Egypt explicitly marketed** — bank transfers + mobile money in **local currency EGP** for platforms. https://www.dlocal.com/blog/markets-and-consumers/payouts-in-egypt ; https://docs.dlocal.com/
- **Verdict:** ✅ strong candidate for the **lawyer-payout leg in EGP** (esp. when combined with a local collector).

---

## Quick comparison matrix (collect / hold / payout / refund / sandbox / Egypt-verified)

| Provider | Collect (EGP) | Hold ~14d | Payout lawyer | Refund | Sandbox | Egypt |
|---|---|---|---|---|---|---|
| Telr | ✅ multi-curr | ✅ auth/capture+split | ⚠️ split/payout | ✅ | ✅ | ⛔ no EGP acquiring |
| MyFatoorah | ✅ (Meeza) | ⚠️ supplier-balance | ⚠️ unverified | ✅ | ✅ | ✅ |
| HyperPay | ✅ | ⚠️ | ✅ HyperSplit ~24h | ✅ | ⚠️ by-request | ✅ |
| YallaPay | ⚠️ UAE | ❌ | ❌/marketing | ❌ | ⚠️ | ⛔ |
| CyberSource | ✅ via acquirer | ✅ | ✅ Visa Direct→card | ✅ | ✅ | ⚠️ needs acquirer |
| Payoneer | ❌ (not collector) | n/a | ✅ 190+ countries, EGP | n/a | ✅ | ✅ |
| Escrow.com | ❌ (EGP) | ✅ true escrow | ✅ per-transaction | ✅ | ✅ | ⛔ not supported |
| Tipalti | ❌ | n/a | ⚠️ | n/a | ✅ | ⚠️ |
| dLocal | ⚠️ | n/a | ✅ EGP payouts | ⚠️ | ✅ | ✅ |

---
*All URLs verified Aug 2026. Unverified/partially-verified claims are explicitly flagged above.*
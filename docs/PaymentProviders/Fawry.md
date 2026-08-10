# Fawry — Provider Research (Escrow-Style Legal Platform)

> Scope: collect → hold ~14 days → release payout to a lawyer → refunds/withdrawals.
> Company focus: Egypt-native. Research date: August 2026.
> **Bottom line:** Fawry is the heavyweight Egyptian PSP for **collecting** (cards, cash at kiosk/agent, wallets) and **refunding** (full/partial) and has a splitter/payout portal — but there is **no documented on-demand disbursement API**, and "hold" = **card Auth/Capture** (short honour window ~7 days, risky for 14) or your settlement timing. Escrow-style is *collect-now, hold in your settlement/ledger, pay later* — not a Fawry-held balance.

---

## 1. Company, regulation, geography

| Item | Finding | Reference |
|---|---|---|
| Company | Fawry for Banking Technology and Electronic Payment S.A.E., Cairo; **listed on EGX** (FWRY) since 2019 IPO (oversubscribed ~30x) | https://www.fawry.com/investor-relations/disclosures/ ; https://www.reuters.com/article/business/egyptian-digital-payments-company-fawry-ipo-oversubscribed-30-times-idUSKCN1UV1ES/ |
| Regulatory | **CBE-regulated PSP/acquiring**; FawrPlus first banking agent authorized by CBE; microfinance FRA-licensed; PCI DSS 4.0 | https://www.fawry.com/about/what-we-do/ ; https://www.fawry.com/business/agent-banking-solutions |
| Geography | Egypt-only acquiring/settlement (domestic); offices in 9 countries but **EGP rails domestic** | https://atfawry.com/pricing ; https://payatlas.com/countries/egypt-eg |

---

## 2. Payment methods (asynchronous ones matter)

Enums in Charge API: `PayAtFawry` (reference code, cash/agent/kiosk), `CARD` (+3DS), `MWALLET` (QR or R2P), `VALU` (BNPL), `CASH_ON_DELIVERY`, bank installments. — https://developer.fawrystaging.com/docs/server-apis/server-apis-overview ; https://apis.io/apis/fawry/fawry-payments-api/

**Asynchronous channel:** `PAYATFAWRY` gives a reference number and payment is confirmed later (cash at anagram/agent/kiosk or app) — you need a status-pull/webhook to know it's paid. — https://developer.fawrystaging.com/docs/server-apis/create-payment-refno-apis

---

## 3. Escrow / hold / release capabilities

### 3.1 "Payment Split" (marketplace-ish)
- **FawryPay Payment Split**: split a collection across pre-configured sub-accounts, **each with its own settlement cycle and bank account**. Settlement goes directly to each configured account. — https://developer.fawry.com/docs/payment-split/payment-split
- ⚠️ This is **not** a platform-pool → on-demand disbursement; it settles to sub-accounts on their cycles. There's **no public API to pull funds from a platform balance to arbitrary recipients on demand**.

### 3.2 Hold ~14 days → pay lawyer: only Auth/Capture on cards
- Authorize (+hold) and Capture (full/partial `captureAmount`) documented; authorize → wallet-free. — https://developer.fawrystaging.com/docs/server-apis/auth-capture-payment-apis
- ⚠️ **UNVERIFIED honour period.** Faw docs don't publish the number. Card-network norms: Visa brief ~7 days, MC up to 30, honour window often much shorter; capture after 14 days typically needs **re-authorization**. Confirm with Faw before relying on a 14-day hold. — https://paymentsandrisk.com/docs/payments/reference/auth-windows/
- Auth/hold is **card-only**; wallets/PAYATFAWRY cannot be held.

### 3.3 Refund API
- `POST .../Fawry/payments/refund` with `merchantCode,referenceNumber,refundAmount,reason,sha256 sig` — **full and partial**. Authorized-but-not-captured can't be refunded. — https://developer.fawrystaging.com/docs/api/refund-issue-api

### 3.4 Settlement to the platform
- Bill/cash collection: **T+1 business day** (bank-facing doc). — https://www.fawry.com/financial-institutions/cashcollection/
- Faw urban pricing page: **"Settlement: Weekly"**. — https://atfawry.com/pricing
- Third-party: T+1–T+2 (cards) / Flutterwave says ~5 business days. → For a 14-day escrow that's **plenty of lead time** but the "hold" is your settlement schedule + your payout rule, **not a Fawry promise**.

### 3.5 On-demand payout to the lawyer's wallet/bank
- **Faw Pay-Out products** exist (Fawry OutPay / wallet / yellowcard / retail) with marketing "upload or **use an online integration for automated processes**" — https://www.fawry.com/payout/ ; https://www.fawry.com/business/pay-out/
- ⚠️ **NO public Payout/Disbursement API endpoint documented** (their dev-portal API list = charges, 3DS, refund, cancel, status, wallet QR/R2P, tokens, links, split — no payout). → on-demand **API** payout to individuals is **UNVERIFIED**; exists likely in B2B/finance contracts. — https://developer.fawrystaging.com/docs/get-started ; https://github.com/dwhiteland/fawry-api
- **InstaPay** is the generic corridor (Fawry integrates for payroll/payouts). — https://www.fawry.com/payroll/

---

## 4. Sandbox / testing

- ✅ **Free self-signup staging** (register at `fawrypay.online/merchant/register` / staging URLs); docs freely readable. — https://developer.fawrystaging.com/docs/get-started
- **Test cards (staging only):** Visa `4508 7500 1574 1019`, MC `5123 4500 0000 0008`, MJywallet CVV 100 / expiry 01/39 / OTP 123456. — https://developer.fawrystaging.com/docs/testing/testing'
- Test and dummy sub-accounts allowed in staging for Split Payment. — https://developer.fawry.com/docs/payment-split/payment-split
- Production requires commercial KYC (commercial registration, tax card).

---

## 5. Costs (documented, Egypt)

- **Fawry Pay pricing (2026):** Setup **999 EGP once**; monthly minimum **499 EGP/mo**; Card **2.75% + 3 EGP/tx**; wallet QR 1.5%; reference code (PayAtFawry) 2.75%; SMS 0.16 EGP; settlement weekly. — https://atfawry.com/pricing (mirror https://fawrypay.online/pricing)
- Third-party compare: PayMob cards 2.25%+0.5 local / 3.25%+1 intl; Faw 2.5–2.75%. — https://samirgeorge.com/writing/payment-gateways-mena/

---

## 6. Webhooks & signature

- **Server Notification V2**: push to `orderWebHookUrl` on paid/expired/refunded; carries `messageSignature` = **SHA-256 over concatenated fields + secureKey**. Verify server-side. — https://developer.fawrystaging.com/docs/payment-notifications/server-notification-v2
- All charge/refund/split/authorize/capture requests likewise sign `sha256(merchantCode+refNumber+...)`.
- Authorization signature = HMAC-style SHA-256(concat + secureKey). (Not TLS token.)

---

## 7. Egyptian-only / international lawyers

- **Payout rails are Egypt-only**: Egyptian bank accounts, e-wallets, yellow-card, retail agents. No evidence of disbursing to non-Egyptian IBANs. A **non-Egyptian lawyer cannot be paid** via Fawry politically. — https://www.fawry.com/payout/
- The platform (merchant) should be an **Egyptian legal entity**; EGP-only settlement.

---

## 8. Weaknesses / risks

1. **No escrow-hold balance**: documented "hold" is card Auth, short-lived; wallets/cash can't be held.
2. **No public automated disbursement API**: "instant to any wallet/bank" is marketing; the S-release on demand at scale is **unverified** — confirm the vendor-payout/"Out" API in the MSA.
3. **Auth-honour period** unpubl اب.; 14-day capture risky — capture early & hold your ledger instead.
4. Payouts **inside Egypt only**.
5. Settlement default weekly; cadence/Min threshold is commercial.

## 9. Verdict for Smart Court
- **Collect + refund:** excellent, simple, Egyptian-PSP credibility — best-in-class card+`cash-at-agent` coverage.
- **Hold:** do it via card Auth/Capture (short) or **your own ledger with weekly settlement**; ramp 14-day as platform rule.
- **Lawyer release:** possible **IF** the payout/"Outer" API is in the contract — **unverified**, must confirm with Fawry.
- Escrow legality (client funds, partner) requires the same scrutiny as all PSPs.

---
*References verified Aug 2026; figures from Fawry's own pricing page & official docs; payout-API claims unverified.*
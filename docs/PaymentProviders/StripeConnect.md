# Stripe Connect — Global Provider Research (Escrow-Style Testing + Possible Production Entity)

> Scope: collect from client → hold ~14 days → release payout to a lawyer → refunds/withdrawals.
> Position: **Stripe has NO Egypt entity** — not usable for *live* EGP processing inside Egypt. But it is the **best free sandbox for developing/testing the full hold→release pipeline**, and a valid *production* path if the platform incorporates in a supported country (US via Atlas, UAE, UK, EEA…). EG/KW lawyers can be **connected accounts** receiving payouts via cross-border rails. Research date: Aug 2026.

---

## 1. Stripe availability (incl. Egypt)

- Official supported-country list at https://stripe.com/global **does NOT include Egypt** (any tier) — nor Kuwait. **UAE (AE) is fully live**.
  - Independent confirmation: https://razorfile.com/egypt/stripe ("Egypt appears in no tier – Egyptians cannot hold a Stripe account"), https://cheerfulcompanies.com/stripe-egypt
- Workaround (documented): **stripe Atlas** → incorporate a **US LLC + EIN + US bank** to process. https://stripe.com/global#Stripe-Atlas
- Egypt is not sanctioned / not listed in Restricted businesses, so it's not "barred" the way those with embargoed geos are — but it needs a supported-company entity. https://stripe.com/legal/restricted-businesses

---

## 2. Connect models — which allow a platform to HOLD money

Full doc: https://docs.stripe.com/connect and https://docs.stripe.com/connect/accounts

| Account type | Who onboards | Where the money sits / control | Good for escrow? |
|---|---|---|---|
| Standard | Stripe | Seller is a full merchant, money in seller account | No (hold sits with seller) |
| **Express** | Stripe KYC | Platform controls payouts; can use destination or separate-charges+transfers | Yes |
| **Custom** | Platform collects KYC | Platform **fully controls balances/payouts** programmatically | **Yes (fullest control)** |
| Note | Connected country lists include **AE, EG, KW** when using cross-border payouts | | |

Charge models: https://docs.stripe.com/connect/charges
- **Direct:** money lands in seller's balance. ❌
- **Destination charges:** platform creates charge; a portion transfers **immediately & automatically** to connected account's pending balance at charge time. ❌ for a 14-day hold unless you DON'T transfer immediately.
- **Separate charges + transfers (="transfers deferred"):** charge lands in your **platform balance** (pending → available); you call **`POST /v1/transfers`** later (hour 14) to send exactly the money to the connected account. ✅ **This is THE hold mechanism.** https://docs.stripe.com/connect/separate-charges-and-transfers
  - "Hold funds in the platform balance before transferring them" is the documented marketplace pattern (rentals/deliveries etc.): https://docs.stripe.com/connect/account-balances#holding-funds

---

## 3. Holds / release — exact semantics

### 3.1 Balance hold on the platform + Transfer at day 14
- Steps: **charge → funds in platform balance → `POST /v1/transfers` (day 14) → connected account → auto/manual payout to lawyer's bank**. https://docs.stripe.com/api/transfers/create
- **How long can you hold? (manual-payout / balance rules)** — by business country:
  - US: 2 years
  - Thailand: 10 days
  - **All other countries (incl. Egypt, UAE, etc.): 90 days**
  - → **14 days is comfortably inside the 90-day allowance.** https://docs.stripe.com/connect/account-balances#holding-funds ; https://docs.stripe.com/connect/manual-payouts
- ⚠️ **Escrow disclaimer:** "Escrow has a precise legal definition, and Stripe doesn't provide escrow services." Holding = manual payout-delay, not Stripe-branded escrow. https://docs.stripe.com/connect/manual-payouts

### 3.2 Transfer/balance errors
- Transfers over available balance fail "Insufficient Funds"; Stripe does **not auto-retry** — you must explicitly re-issue. https://docs.stripe.com/connect/account-balances
- Negative balances pause payouts until positive (auto-debit from bank where supported). 
- **Application fee** = `application_fee_amount`/ `transfer_data[amount]`, prorated on refunds via `refund_application_fee`. https://docs.stripe.com/connect/destination-charges

### 3.3 Card-level hold (authorization)
- Standard **auth + capture = 7 days** (network-driven) for online cards. — https://docs.stripe.com/payments/place-a-hold-on-a-payment-method ; FAQ "7-day limit, we don't support longer" https://support.stripe.com/questions/does-stripe-support-holding-an-authorization-for-more-than-7-days-before-capture
- **Extended authorization: up to 30 days** for eligible online merchants. https://docs.stripe.com/payments/extended-authorization
- → **14 days NOT via vanilla auth; only via 30-day extended authorization OR hold the *money* in platform balance (capture day-0).**

### 3.4 Payout schedule/delay
- Connect `delay_days_override` **up to 31 days** for auto-payouts. https://docs.stripe.com/connect/manage-payout-schedule
- Settlement timing per country: US T+2, EU T+3, AE ~T+5; first payout 7–14 days after first payment. https://docs.stripe.com/payouts
- **Instant payouts** (~30 min, 1% fee) available in supported countries incl **AE** but **NOT EG or KW**. https://docs.stripe.com/connect/instant-payouts

---

## 4. Fees (Connect, "you handle pricing")

- **$2/month per active connected account**; **0.25% + $0.25 per payout**; instant **1%** on volume; cross-border payouts **0.25%**; account debits 1.5%. — https://stripe.com/connect/pricing
- Sandbox/test is **free**.

---

## 5. Test mode / webhooks / idempotency

- Free full sandbox: test API keys, test cards (4242…), test PaymentMethods, decline/3DS/dispute simulators; test payouts are simulated (no real bank). — https://docs.stripe.com/testing
- **Webhook signature:** `Stripe-Signature` header `t=…,v1=…` HMAC-SHA256 over `timestamp.payload` with `whsec_…`; ~5-min replay window; ignore non-`v1`. — https://docs.stripe.com/webhooks/signatures
- Idempotency keys accepted on all POSTs (results cached ≥24h). — https://docs.stripe.com/api/idempotent_requests

---

## 6. Can an Egypt/Kuwait/UAE lawyer be the receiver (payout)?

- **Yes as connected accounts (Require Express/Custom):** connected-account country lists **include AE·EG·KW**, many only available via cross-border payouts. https://docs.stripe.com/connect/accounts ; https://docs.stripe.com/connect/cross-border-payouts (platform must be US/UK/EEA/CA/CH)
- Bank formats: **EG** SWIFT/BIC+IBAN (29ch) "only available for Cross-border payouts"; **KW** same; **AE** IBAN(23). https://docs.stripe.com/payouts (country table)
- Cross-border payout fee 0.25%; FX conversion applied. Independent corroboration (Remote Space doc): EG/KW/ maybe a supported without AED platform via USD/EUR routes. https://support.remote.com/hc/en-us/articles/7046120384909-Stripe-Connect-Country-Availability-for-Local-Currency-Payouts

---

## 7. Risks / limitations for our case

1. **No Stripe in Egypt** → no live EGP. Production needs a supported entity (US/UAE/UK).
2. **Escrow & "law-firm collecting funds" are restricted business** categories — need underwriting aproval; don't describe the product as "escrow". https://stripe.com/legal/restricted-businesses
3. Hold cap 90 days (non-US) is fine for 14 days but you **build** the day-14 trigger yourself (queue/job).
4. Card auth hold is 7 days (or 30 via extended auth) → prefer capture-day-0 + balance hold.
5. You're a money transmitter in many jurisdiction by receiving funds — legal advice; conversation yields MTU compliance.
6. EG/KW cannot get Instant bulk payouts — only normal cross-border.

---

## 8. Comparison to `IPaymentProvider` — mapping (design reference)

| Domain op | Stripe primitive | Stripe API |
|---|---|---|
| `Deposit` | Create charge on platform (separate charges) → funds in **platform pending** balance | PaymentIntent |
| `Release` (day-14) | `POST /v1/transfers` platform→connected account (+ `source_transaction`, `transfer_group`) | account.transfer |
| `Withdraw` (refund/void) | Refund client: `POST /v1/refunds` (+ `reverse_transfer=true` + `refund_application_fee`); platform withdraws its own money: payout to its bank | Refund / Payout |
| Hold timing | You run day-14 job; 90-day max hold | manage-payout-schedule / manual-payouts |
| Card-level hold | auth+`capture_method=manual` (7d | or extended 30d) | PaymentIntent capture |

→ This is **the blueprint our `BalanceProvider` abstraction is closest to** (platform balance ↔ transfer ↔ payout), so a Stripe adapter is a *logical* first real-implementation when the platform entity exists.

## 9. Verdict for Smart Court
- **Test/dev:** use Stripe sandbox **today** → maps 1:1 to our `Deposit/Release/Withdraw` (balance hold + transfer). 100% free.
- **Live Egypt:** not possible until you incorporate a supported entity; then this is the cleanest cross-border ("lawyer in EG/KW/UAE") path.
- Consult legal before ever saying the word "escrow" to Stripe underwriting.

---
*References verified Aug 2026; Instant Payout and connected-account availability subject to change — re-check.*
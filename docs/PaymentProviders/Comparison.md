# Payment Provider Research — Comparison & Recommendation

> **Goal:** Client pays into a legal-service escrow → hold ~14 days → release payout to the lawyer → refunds/withdrawals.
> **Research date:** August 2026. Companys researched: Paymob, Tap Payments, Fawry, PayTabs, Stripe Connect, + regional/global (Telr, MyFatoorah, HyperPay, YallaPay, CyberSource, Payoneer, Escrow.com, Tipalti, dLocal).
> Detailed per-provider files: `Paymob.md`, `TapPayments.md`, `Fawry.md`, `PayTabs.md`, `Stripe Connect.md`, `OtherCandidates.md`.

---

## The core truth (read first)

**No PSP has a product literally called "escrow" for Egypt.** What everyone actually offers is one of:

| Mechanism | What "hold" means | Who holds it | True 14-day escrow? |
|---|---|---|---|
| **Card Auth/Capture** (Paymob, Stripe, Tap, Payon, Fawry) | Funds frozen on the *customer's card* until capture | The card scheme | ⚠️ Auth windows 7 days (scheme-driven); 14-day only achievable with Paymob's exact 14d auto-void, Stripe's 30-day extended auth, or "capture-day-0 + hold-in-balance". |
| **Merchant/aggregator balance hold** (Paymob MarketPlace, Tap Delayed Split, My Fatoorah suppliers) | Money settles into **your (platform) balance**, you delay **paying out** | Your merchant balance (you = merchant of record) | ✅ Yes, as a *platform rule* — funds sit with the merchant until you push a payout. NOT legally segregated escrow. |
| **Payout rails after hold** (Paymob Payouts, PayTabs External Payouts, HyperDel Split, Payoneer, dLocal) | You call a payout/disbursement to a lawyer's wallet/bank | The PSP moves funds on demand | ✅ The "release" leg |
| **True licensed escrow** (Escrow.com) | Real trust-account escrow | The escrow company | ✅ But Egypt is not supported, no EGP. |

**Consequently the 14-day flow = a design of (collect → hold in merchant balance → release via payout rail), implemented in *our own* ledger**, exactly what our `EscrowHold` + `IPaymentProvider` architecture is for. The PSP provides collection + movement; escrow correctness is ours.

---

## Comparison matrix (Egypt-live capable)

| Provider | Egypt entity | EGP collect | Free sandbox | Hold ~14d | API payout→lawyer | Refund | Webhook HMAC | Cost (EG, cards) | Egypt verdict |
|---|---|---|---|---|---|---|---|---|---|
| **Paymob** | ✅ CBE PF | ✅ | ✅ | ✅ via Auth/Capture (14d auto-void) or merchant balance | ✅ **Paymob Payouts** (wallet/bank/card; needs national-ID) | ✅ (full/partial; not BNPL/kiosk) | ✅ HMAC-SHA512 (20-field) | 2.75% + 3 EGP | **Best Egypt-first fit** |
| **Tap Payments** | ✅ CBE (May'25, onboarding pending confirm) | ⚠️ dev-confirm | ✅ | ✅ **Delayed Split** (held in marketplace wallet | ⚠️ **no initiate-payout API** (cycle/dashboard) | ✅ | ✅ HMAC-SHA256 | ~2–2.75% | Strong **if** Egypt onboarding/EGP rails confirmed |
| **Fawry** | ✅ Egypt corporate | ✅ (PayAtFawry cash, card, wallet, Meeza) | ✅ | ⚠️ only card Auth (short honour) | ❌ **no public disbursement API** | ✅ full/partial | ✅ SHA-256 (messageSignature) | 2.75% + 3 EGP; 499 EGP/mo min | Great collector; **payout-API unverified** |
| **PayTabs Egypt** | ✅ CBE PSP | ✅ | ✅ | ⚠️ auth (issuer-bank window; not garanteed 14d) | ✅ **External Payouts** (batch) + Split Payouts | ✅ | ✅ HMAC-SHA256 | 2.5% + 2–3 EGP | Solid option if payouts enabled |
| **TelStripe** (read-only test blueprint) | ❌ no Egypt | ❌ EGP not available | ✅ **free full sandbox** | ✅ balance-hold + transfer (90d guardrail) | ✅ `transfers` + cross-border (EG/KW/AE receivers) | ✅ | ✅ HMAC-SHA256 | per-item: $2/mo + 0.25%+0.25/payout + tx fees | ⛔ live Egypt impossible; **best dev/test sandbox + blueprint** |
| MyFantabah | ✅ (api-eg) | ✅ Meeza | ✅ | ⚠️ supplier-balance | ⚠️ unverified Egypt | ✅ | ⚠️ bearer + GetPaymentStatus | ~2.5% | Good collect/split; confirm payout |
| HyperPay | ✅ leaf | ✅ EGP | ⚠️ by-request | ⚠️ | ✅ HyperSplit ~24h | ✅ | ✅ HMAC-SHA256 | contact | Viable regional |
| TelTel | ❌ no Egypt | ❌ | ✅ | ✅ | ⚠️ | ✅ | ✅ HMAC | — | ⛔ not for Egypt |
| Escow.com | ❌ Egypt & EGP not supported | ❌ | ✅ | ✅ true escrow | ✅ | ✅ | ✅ webhooks | 2.6%+ (est) | ⛔ only non-EG pilots |
| Payoneer / dLocal | n/a (payout rails) | n/a | ✅ | n/a (you hold) | ✅ **EGP lawyer payouts** | n/a | ✅ | per payout | ✅ **as the payout leg** |

✔ Key columns to decide: **Egypt-viable entity + real payout API + free sandbox.** 

---

## Recommendation (progression)

1. **Development & testing TODAY (code-first):** **Stripe Connect sandbox** — free, complete hold→transfer→payout lifecycle, maps 1:1 onto our `IPaymentProvider.Deposit/Release/Withdraw/Refund` and fills our test harness without a merchant license. 
2. **MVP live (Egypt):** **Paymob** — CBE-licensed, EGP, free sandbox, documented Auth/Capture (14-day auto-void) **+** Payouts (Send) API for the lawyer-release leg + refunds. This is the most complete *matrix-perfect* Egyptian PSP today.
3. **Egypt runner-up / negotiation backup:** **PayTabs Egypt** — paid via External Payouts (batch) and split; requires auth-hold confirmation + payout fee/limits.
4. **Payout leg if gateway lacks on-demand disbursement:** **Payoneer** or **dLocal** (both proven EGP-to-lawyer rails) behind our own ledger hold; or confirm Fawry's vendor-payout API in contract (currently **unverified**).
5. **True-escrow alternative for cross-border/non-EG pilots only:** **Escrow.com** — not Egypt-suitable, keep on the shelf.

### What to confirm with each vendor BEFORE production (common list)
- (a) Is there an API to **trigger payout/release on-demand** (Paymob Send ✓ · Tap no · Fawry unverified · PayTabs External Payouts ✓ · PayPal ✓);
- (b) **Bit of auth-hold duration** and 14-day guarantee (card auth windows);
- (c) Per-receiver **KYC** (national-ID, IBAN, limited names);
- (d) **Chargeback / refund reverse mechanics** after a payout has been made (clawback);
- (e) **Settlement to the merchant** cadence vs the day-14 release (pre-funding needs);
- (f) Whether holding client funds needs a **separate trust/escrow authorization** under CBE rules in your case.

---

*All figures verified Aug 2026; per-merchant/enterprise rates are subject to contract.*
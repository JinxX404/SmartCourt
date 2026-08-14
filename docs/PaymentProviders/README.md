# Payment Provider Research — Index

Exhaustive provider research for the **escrow-style legal platform** flow:

> Client pays → funds **held ~14 days** → **payout released** to the lawyer on acceptance → **refund / withdrawal** branches.

**Research date:** August 2026 · Scope: Egyptian-capable PSPs + global/testing options.

## Files

| File | Provider / Topic |
|---|---|
| [`Phase_1_End_To_End_Payment_Gateway_Analysis.md`](./Phase_1_End_To_End_Payment_Gateway_Analysis.md) | Current code trace, Stripe vs. Paymob architecture, gaps, and Phase 1 decision |
| [`StripeConnect_Setup_Guide.md`](./StripeConnect_Setup_Guide.md) | Phase 2 dashboard setup for the approved Stripe Connect test MVP |
| [`StripeConnect_Implementation_Plan.md`](./StripeConnect_Implementation_Plan.md) | Phase 3 pre-code plan for the complete Stripe Connect lifecycle |
| [`StripeConnect_MVP_Runbook.md`](./StripeConnect_MVP_Runbook.md) | Phase 4 configuration, endpoints, demo sequence, and verification handoff |
| [`StripeConnect_Frontend_Sandbox_Guide.md`](./StripeConnect_Frontend_Sandbox_Guide.md) | Exact frontend Stripe Elements sequence for the complete zero-seeding sandbox lifecycle |
| [`Comparison.md`](./Comparison.md) | Matrix + recommendation + decision list (read first) |
| [`Paymob.md`](./Paymob.md) | **Paymob** — recommended Egypt-first (MarketPlace, Payouts/Send, Auth-Capture) |
| [`TapPayments.md`](./TapPayments.md) | **Tap Payments** — marketplace Delayed Split (escrow-like wallet hold) |
| [`Fawry.md`](./Fawry.md) | **Fawry** — heavyweight Egyptian collector; payout-API unverified |
| [`PayTabs.md`](./PayTabs.md) | **PayTabs Egypt** — External/Split Payouts (note: no "Disperse" product) |
| [`StripeConnect.md`](./StripeConnect.md) | **Stripe Connect** — free dev/test sandbox + production blueprint (no Egypt entity) |
| [`OtherCandidates.md`](./OtherCandidates.md) | Telr, MyFanootah, HyperPay, YallaPay, CyberSource, Payoneer, Escrow.com, Tipalti, dLocal |

## TL;DR

- **Best Egypt-live candidate:** **Paymob** (CBE-licensed, EGP, free sandbox, hold via Auth/Capture **or** merchant-balance, on-demand payouts via Paymob Payouts, refunds).
- **Best dev/test sandbox + long-term blueprint:** **Stripe Connect** (fee: full lifecycle, maps 1:1 to our `IPaymentProvider`).
- **No PSP offers regulated escrow in Egypt** — the 14-day hold is a *platform-balance + payout-delay* rule that *we* implement (which our `EscrowHold` ledger already does).
- **Lawyer-payout leg** (if the gateway lacks on-demand disbursement): **Payoneer**/**dLocal** (EGP rails).

---
*All claims carry reference URLs inside each file; third-party prices flagged; contracts prevail.*

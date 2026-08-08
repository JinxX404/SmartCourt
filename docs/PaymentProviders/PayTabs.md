# PayTabs — Provider Research (Escrow-Style Legal Platform)

> Scope: collect → hold ~14 days → release payout to a lawyer → refunds/withdrawals.
> Company focus: MENA + Egypt. Research date: August 2026.
> **Bottom line:** PayTabs Egypt (CBE-licensed) gives us the classic gateway toolkit — `auth`/`capture`, `refund` (full/partial), **External Payouts** (batch mass disbursement to lawyers) and **Split Payouts** (marketplace revenue split) via API. There is **no product named "Disperse"** (name is mistaken — actual products: External / Split / Internal Payouts). The 14-day hold depends on **issuer-bank auth windows** (often &lt;14 days) — confirm with PayTabs Egypt before committing; mitigations = capture-at-day-0 + hold, or Split Payouts.

---

## 1. Company, regulation, geography

| Item | Finding | Reference |
|---|---|---|
| Global | PayTabs LLC (HQ Riyadh), founded 2014, ~144 employees, ~$16.2M revenue; acquired by Tamara | https://www.paytabs.com |
| Egypt | **2020 launch** via EFG Hermes JV + 3 partner banks; **CBE PSP license (Apr 2020)**; **full ownership taken by PayTabs Group Mar 2025** | https://ar20.efgholdings.com/PayTabs.html ; https://ai.paytabs.com/pr/paytabs-group-strengthens-its-commitment-to-egypts-digital-payments-ecosystem-with-full-ownership-of-paytabs-egypt/ |
| CBE framework | June 2025 CBE licensing/registration for PSOs/PSPs (Banking Law 194/2020). Tax/license number for PayTabs Egypt **not publicly verified** | https://shehatalaw.com/law-update/cbe-regulations-licensing-payment-operators/ |
| Currencies | EGP settlement default (also USD/EUR/GBP/SAR/AED) | https://docs.paytabs.com |

---

## 2. Payout products (actual names — "Disperse" does NOT exist there)

| Product | Mode | Best fit | Reference |
|---|---|---|---|
| **External Payouts** | API + Dashboard; batch disbursements | Paying lawyers from your platform balance | https://docs.paytabs.com/manuals/PT-API-Endpoints/Deposit-and-Payouts/Deposit-and-Payouts-Landing |
| **Split Payouts** | API-only; marketplace revenue sharing | Split at capture time | same + split prerequisites doc |
| **Internal Payouts** | Dashboard-only | Wallets/balances | same |

- External Payouts endpoint examples: `POST https://secure-egypt.paytabs.com/payout/batch/new` with `authorization: <Server Key>` + `profile_id`.
- Split Payouts: `POST ...,/payout/split/payout`.

---

## 3. Escrow-relevant API

### 3.1 Auth + hold
- `tran_type:"auth"` — authorize funds on a 3D-Secured transaction, **not captured/settled**. — https://docs.paytabs.com
- ⚠️ **Auth/capture must be explicitly enabled** by emailing customercare@paytabs.com.
- ⚠️ **Auth-hold duration is issuer-bank dependent** — docs say "certain period of time/days differing from one issuer bank to another". A 14-day hold is likely **beyond standard card-scheme auth windows** (typically 5–7 days). Confirm.
- Refunds only on **card** transactions.

### 3.2 Capture (release after 14 days)
- `tran_type:"capture"` — full or **partial** capture; use the original `payment_id`.

### 3.3 Refund / Void
- Full/partial refund on `sale`/`capture`; full refund only on `auth`. — https://docs.paytabs.com/manuals/PT-API-Endpoints/Integration-Types-Manuals/Managed/ Hosted-Payment-Page/Manage-Transactions/...Refund
- Statuses: `A=Authorized, H=On Hold (fraud), P=Pending-R refund, V=Voided, F=Captured? ...` — https://support.paytabs.com/en/support/solutions/articles/60000711358-what-is-response-code-vs-the-response-status-
- Error **601 = On Hold** (anti-fraud).

### 3.4 AuthExt
- php-SDK exposes "**AuthExt** (Auth Extension to refresh fund holds)" — possibly extends the hold. **Verify availability in Egypt/PT-API.** — https://github.com/PayTabscom/php-SDK/python

---

## 4. Sandbox / testing

- ✅ **Free Test profile auto-created on signup.** Test cards for visa/MC + 3DS sim built in. — https://support.paytabs.com/en/support/solutions/articles/60000712315-what-are-the-test-cards-available-to-perform-payments- ; https://support.paytabs.com/en/support/solutions/articles/60001070859-what-is-a-test-profile-vs-live-profile-
- Sandbox domain sample: `https://secure.paytabs.com/payment/request` (test) vs region endpoints per country. — https://support.paytabs.com/en/support/solutions/articles/60000718070-what-is-my-test-endpoint- or region-endpoint doc

---

## 5. Pricing (Egypt, official)

| Plan | Fees | Out-of-threshold (EGP/mo) |
|---|---|---|
| **Insta (Paymes)** | 2.5% + 3 EGP | ≤ 50k |
| **Growth** | 2.5% + 2 EGP | 5k–250k |
| **Enterprise** | custom | > 250k |

Sources: https://ai.paytabs.com/en/egypt/. Payout/disbursement fees **not published** — get commercial quote.

---

## 6. Webhooks & signature verification

- Webhook POST → verify **HMAC-SHA256 of full raw payload with the Server Key** in the `Signature` header.
- Return-URL: strip the `signature` param, concatenate ordered fields, recompute. — https://support.paytabs.com/en/support/solutions/articles/60000718961-how-to-verify-the-response-received-from-payments-signature-verification-

---

## 7. Coverage / local methods (Egypt)

- Local: **Meeza, Nimic (Fawry), Mobile Wallets**; partner-bank settlement for PSPs vs dashboard "Accounts" for aggregation. — https://support.paytabs.com/en/support/solutions/articles/60001048862-accounts-menu-via-fully-integrated-merchant-dashboard

---

## 8. Risks / unverified items

| # | Risk/gap | Impact | Mitigation |
|---|---|---|---|
| 1 | "Disperse" doesn't exist as a PayTabs product | expectation mismatch | use External/Split Payouts |
| 2 | 14-day auth-hold unverified; may require capture at 0 and refund later | funds may hit card limits | capture-day-0→hold-wallet→refund OR (confirm + AuthExt) |
| 3 | Escrow regulatory position in Egypt (legal-services escrow) unverified | legal/compliance | CBE-agent bank partner / local counsel |
| 4 | Auth/capture needs enablement ticket | rollout blocked | request enablement first |
| 5 | Payout fee unpublished | budget drift | bespoke quote |
| 6 | Cardholder refund latency (days) | UX | mirror in your side |
| 7 | Refund API-only limits | operational | rate-limit ops |

## 9. Verdict for Smart Paytabs
- **Concise:** Doable **technically** with External Payouts + split; **needs confirmation** on: (a) Egypt auth-hold duration vs 14 days, (b) AuthExt availability, (c) external-payout API + fees + recipient KYC, (d) CBE escrow-legal fit.
- Recommended gateway-agnostic implementation: **capture-day-0 → hold in your ledger → External Payout at day 14 → refund via API for client branch.**

---
*References verified Aug 2026; currency of "Disperse" product page is officially absent from their docs.*
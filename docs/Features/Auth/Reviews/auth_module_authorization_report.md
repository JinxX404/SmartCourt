# Auth Module — Authorization Implementation Report

> **Purpose:** Evaluate whether **authentication / authorization** on each Auth endpoint is **correct**, **complete**, and aligned with the API contract (`docs/Plans/06_API_Auth_Users.md`).  
> **Date:** 2026-07-16  
> **Scope:** All endpoints under `api/auth/*`, plus platform auth plumbing that affects them (JWT, policies, filters).  
> **Companion:** Fix plan → `auth_module_fix_plan_v1_v2_gaps.md`

---

## 1. Executive summary

| Question | Answer |
|----------|--------|
| Are Auth **endpoint auth attributes** mostly correct? | **Yes** for documented public vs protected surfaces |
| Is authorization **complete** end-to-end? | **No** — gaps in explicit attributes, session invalidation, policies, and operational endpoints |
| Only endpoint that must require a logged-in user | `POST /api/auth/change-password` → **`[Authorize]` is correct** |
| Platform default security | **Permissive** — no global `FallbackPolicy` requiring auth; unmarked endpoints stay anonymous |

### Overall authorization grade: **C+ (usable for dev, incomplete for production)**

**What works**

- Public auth flows correctly marked `[AllowAnonymous]` (register, login, confirm, resend, forgot, reset).  
- Change-password correctly requires authentication and binds identity from JWT claims (`User.GetUserId()`).  
- JWT Bearer is registered; roles are emitted as `ClaimTypes.Role` (usable with `[Authorize(Roles = "...")]` elsewhere).  
- Login enforces **authorization business rules** beyond attributes: email confirmed + not suspended → 403.

**What does not**

- `refresh` and `revoke` have **no** `[AllowAnonymous]` / `[Authorize]` (works today only because there is no global authorize-all policy).  
- No policies for “email confirmed”, “active account”, or rate-limit-backed authz.  
- Password change/reset does not **authorize session termination** (refresh tokens remain valid).  
- Hangfire dashboard and (dev) Swagger are not authorization-protected.  
- Auth module does not use role-based gates (correct for these endpoints; completeness is about **system** readiness).

---

## 2. Platform authorization plumbing

### 2.1 Pipeline

```text
ExceptionHandlingMiddleware → UseAuthentication → UseAuthorization → MapControllers
```

Order is **correct** (authN before authZ).

### 2.2 Authentication scheme

| Setting | Value | Assessment |
|---------|-------|------------|
| Default scheme | JWT Bearer | Correct for API |
| Validate issuer/audience/lifetime/signing key | Yes (middleware) | Correct |
| Clock skew | 1 minute | Acceptable |
| Role claims in JWT | `ClaimTypes.Role` | Correct for ASP.NET role checks |
| `sub` + `NameIdentifier` | Both set to user id | Correct for `GetUserId()` |

### 2.3 Authorization services

```csharp
services.AddAuthorization(); // no custom policies, no FallbackPolicy
```

| Capability | Present? | Impact |
|------------|----------|--------|
| Default deny unauthenticated | **No** | Controllers without attributes are public |
| Named policies (e.g. `EmailConfirmed`, `ActiveUser`) | **No** | Only ad-hoc checks in login service |
| Role policies | Framework default only | OK if roles used on controllers |
| `AuthorizeOwnerAttribute` | Yes (Users slice) | **Not used** on Auth endpoints (N/A) |

### 2.4 Implication for Auth endpoints

| If code has… | Effective access today |
|--------------|------------------------|
| `[AllowAnonymous]` | Public |
| `[Authorize]` | Requires valid JWT |
| **No attribute** | **Public** (because no fallback policy) |

Therefore missing attributes on `refresh` / `revoke` are a **documentation & future-safety** problem: adding a global authorize policy later would **break** refresh unless `[AllowAnonymous]` is added first.

---

## 3. Endpoint-by-endpoint authorization matrix

### Legend

| Verdict | Meaning |
|---------|---------|
| **Correct** | Matches contract + secure intent |
| **Correct (implicit)** | Behavior OK today but attribute missing/unsafe for global policies |
| **Incomplete** | Attribute OK but related authZ/session rules missing |
| **Wrong** | Attribute or access model conflicts with contract/security |

---

### 3.1 `POST /api/auth/register/client`

| Item | Detail |
|------|--------|
| **Contract** | `[AllowAnonymous]` |
| **Implementation** | `[AllowAnonymous]` on action |
| **Verdict** | **Correct** |
| Notes | Public registration is intentional. No role required. |

---

### 3.2 `POST /api/auth/register/lawyer`

| Item | Detail |
|------|--------|
| **Contract** | `[AllowAnonymous]` |
| **Implementation** | `[AllowAnonymous]` on action |
| **Verdict** | **Correct** |
| Notes | Same as client. Authorization for lawyer *capabilities* (proposals, etc.) is out of Auth slice and must be enforced later via roles + verification — not on this endpoint. |

---

### 3.3 `POST /api/auth/login`

| Item | Detail |
|------|--------|
| **Contract** | `[AllowAnonymous]` |
| **Implementation** | `[AllowAnonymous]` |
| **Verdict** | **Correct** (attribute) / **Partial** (post-auth business authZ) |
| Business checks | Unverified → 403; Suspended → 403; bad password → 401 |
| Missing | Lockout; rate limiting; `LastLoginAt`; no check for other statuses (e.g. Rejected) if product requires |

---

### 3.4 `POST /api/auth/refresh`

| Item | Detail |
|------|--------|
| **Contract** | `[AllowAnonymous]` |
| **Implementation** | **No** auth attribute |
| **Verdict** | **Correct (implicit)** — functionally public; **incomplete** for explicit contract/safety |
| Auth model | Validates access token (lifetime optional) + refresh token in body |
| Issues | 1) Should declare `[AllowAnonymous]`. 2) Contract does not require access token. 3) Failures are 400 not 401. 4) Not a substitute for RT secrecy. |

**Recommendation**

```csharp
[HttpPost]
[AllowAnonymous]
public async Task<ActionResult<...>> Refresh(...)
```

And treat invalid RT as **401 Unauthorized**, not business 400.

---

### 3.5 `POST /api/auth/revoke` (extra; not in contract)

| Item | Detail |
|------|--------|
| **Contract** | Not defined |
| **Implementation** | No attribute; service validates body tokens |
| **Verdict** | **Correct (implicit)** for token-body design; **incomplete** documentation |
| Design choice A | Keep public + require AT+RT in body → add `[AllowAnonymous]`, document as logout |
| Design choice B | Require `[Authorize]` and revoke current user’s RT from claims + body RT only | Cleaner for “logged-in logout” |

**Security note:** Returning `false` for inactive RT with **200** is weak signaling; prefer 401 for invalid credentials/tokens.

---

### 3.6 Email verification (`GET /api/auth/confirm-email`)

| Item | Detail |
|------|--------|
| **Contract** | `POST /verify-email` → `[AllowAnonymous]` |
| **Implementation** | `GET confirm-email` + `[AllowAnonymous]` |
| **Verdict** | **Correct** for anonymity; **path/method unaligned** (not an authZ failure) |
| Notes | Public by design (token proves possession of email). Do **not** require JWT. |

---

### 3.7 `POST /api/auth/resend-verification`

| Item | Detail |
|------|--------|
| **Contract** | `[AllowAnonymous]` |
| **Implementation** | `[AllowAnonymous]` |
| **Verdict** | **Correct** attribute; **incomplete** abuse controls (rate limit) |
| Notes | Always-200 is an **authorization privacy** control (anti-enumeration). Rate limit is part of authorization posture against email bombing. |

---

### 3.8 `POST /api/auth/forgot-password`

| Item | Detail |
|------|--------|
| **Contract** | `[AllowAnonymous]` |
| **Implementation** | `[AllowAnonymous]` |
| **Verdict** | **Correct** attribute; **Wrong behavior** for authZ privacy when unverified throws 400 |
| Notes | Throwing on unverified **leaks account state** (authorization/privacy defect, not attribute defect). |

---

### 3.9 `POST /api/auth/reset-password`

| Item | Detail |
|------|--------|
| **Contract** | `[AllowAnonymous]` |
| **Implementation** | `[AllowAnonymous]` |
| **Verdict** | **Correct** attribute; **Incomplete** session authZ |
| Missing | Must invalidate all refresh tokens so old sessions cannot continue (contract rule) |
| Auth of action | Possession of email + reset token (not JWT) — correct |

---

### 3.10 `POST /api/auth/change-password`

| Item | Detail |
|------|--------|
| **Contract** | `[Authorize]` + Bearer access token |
| **Implementation** | Class-level `[Authorize]`; `User.GetUserId()` |
| **Verdict** | **Correct** for endpoint protection; **Incomplete** for session lifecycle |
| Flow | Unauthenticated → 401 (framework). Authenticated → change own password only (user id from token, not body) — **correct ownership model** |
| Missing | 1) Revoke all refresh tokens after success. 2) Optional: reject if account suspended even with valid JWT. 3) Optional: step-up / re-auth for high risk (future). |

```csharp
[Authorize] // correct
public class ChangePasswordController ...
{
    var userId = User.GetUserId(); // correct — never trust body userId
}
```

---

## 4. Summary table (Auth endpoints)

| Endpoint | Contract auth | Code attribute | Effective access | Verdict |
|----------|---------------|----------------|------------------|---------|
| `POST .../register/client` | Anonymous | `[AllowAnonymous]` | Public | **Correct** |
| `POST .../register/lawyer` | Anonymous | `[AllowAnonymous]` | Public | **Correct** |
| `POST .../login` | Anonymous | `[AllowAnonymous]` | Public | **Correct** |
| `POST .../refresh` | Anonymous | *(none)* | Public | **Correct (implicit)** — add explicit |
| `POST .../revoke` | *(undocumented)* | *(none)* | Public | **Correct (implicit)** — document + attribute |
| `GET .../confirm-email` | Anonymous* | `[AllowAnonymous]` | Public | **Correct** (*contract path differs*) |
| `POST .../resend-verification` | Anonymous | `[AllowAnonymous]` | Public | **Correct** |
| `POST .../forgot-password` | Anonymous | `[AllowAnonymous]` | Public | **Correct** attribute |
| `POST .../reset-password` | Anonymous | `[AllowAnonymous]` | Public | **Correct** attribute |
| `POST .../change-password` | Authorize | `[Authorize]` | JWT required | **Correct** |

\*Contract: `POST /verify-email` AllowAnonymous.

**Wrong attributes?** None of the Auth endpoints currently use the **opposite** of the contract (e.g. Authorize on register).  
**Incomplete authorization?** Yes — see sections 5–6.

---

## 5. Authorization completeness checklist

### 5.1 Endpoint-level (attributes)

| Check | Status |
|-------|--------|
| Every public auth endpoint explicitly `[AllowAnonymous]` | **Incomplete** (`refresh`, `revoke`) |
| Change-password `[Authorize]` | **Complete** |
| No accidental `[Authorize(Roles=...)]` blocking public flows | **OK** |
| Documented contract vs code attributes | **Mostly OK** |

### 5.2 Token / session authorization

| Check | Status |
|-------|--------|
| Access JWT required for protected ops | **Complete** for change-password |
| Refresh rotation (one-time RT) | **Complete** (logic) |
| Replay → revoke all RTs | **Complete** (logic) |
| Reset password revokes sessions | **Missing** |
| Change password revokes sessions | **Missing** |
| RT stored hashed | **Missing** |
| Suspended user blocked at login | **Complete** |
| Suspended user blocked on refresh / API with old JWT | **Missing** on refresh (and no global status check middleware) |
| Email-unconfirmed blocked at login | **Complete** |
| Email-unconfirmed cannot use JWT if somehow issued | N/A if never issued; no global policy |

### 5.3 Claim / policy completeness

| Check | Status |
|-------|--------|
| `NameIdentifier` / `sub` for user id | **Present** |
| Role claims for `[Authorize(Roles)]` | **Present** |
| Policy `EmailConfirmed` | **Missing** |
| Policy `ActiveUser` / not suspended | **Missing** (login only) |
| Global fallback authorize | **Missing** (by design permissive) |

### 5.4 Related (non-Auth) but relevant

| Component | AuthZ status | Note |
|-----------|--------------|------|
| `ClientsController` / `LawyersController` | `[Authorize]` + `[AuthorizeOwner]` | Good pattern; separate from Auth module |
| Hangfire dashboard | **Open** | Authorization gap in pipeline |
| Swagger (dev) | Open | Acceptable in Development only |

---

## 6. Gaps that are “authorization” even when attributes look fine

These are often misclassified as pure business bugs; they **are** authorization/session control:

| Gap | Why it is authZ |
|-----|-----------------|
| Forgot-password 400 for unverified | Information disclosure / account state authZ |
| No rate limiting on public auth | Authorization of resource abuse (email, brute force) |
| No lockout | Authorization of repeated credential attempts |
| RT not revoked on password change | Authorization of continued access after credential change |
| Refresh does not re-check Suspended | Authorization of continued access after admin suspend |
| Plaintext RT in DB | Authorization credential storage hygiene |
| 500 leaks exception | Information disclosure |

---

## 7. Recommended authorization fixes (ordered)

### P0

1. Add `[AllowAnonymous]` to `RefreshTokenController` and `RevokeRefreshTokenController`.  
2. On **reset** and **change** password: revoke all active refresh tokens.  
3. On **refresh**: re-check user `Status != Suspended` (and optionally email confirmed); fail with 401/403.  
4. Map invalid refresh/revoke to **401** (`AuthenticationException`).

### P1

5. Introduce optional global secure default for non-dev:

   ```csharp
   options.FallbackPolicy = new AuthorizationPolicyBuilder()
       .RequireAuthenticatedUser()
       .Build();
   ```

   Then **every** public endpoint **must** have `[AllowAnonymous]` (including refresh/revoke).

6. Policies:

   ```csharp
   options.AddPolicy("ActiveUser", p => p.RequireAuthenticatedUser()
       /* custom handler reading Status if claim or DB */);
   ```

7. Rate limiting policies on login/forgot/resend/refresh.  
8. Enable Identity lockout on failed login.  
9. Hash refresh tokens at rest.  
10. Secure Hangfire dashboard.

### P2

11. Document `POST /api/auth/revoke` in API contract with chosen auth model.  
12. Consider short-lived access tokens + mandatory RT rotation client guide.  
13. Add integration tests: anonymous can call public auth; change-password without JWT → 401; owner id only from claims.

---

## 8. Suggested “done” definition for Auth authorization

Authorization implementation is **complete** when:

- [ ] Every Auth endpoint has an **explicit** `[AllowAnonymous]` or `[Authorize]` matching the contract  
- [ ] `change-password` requires valid JWT and only mutates the token subject  
- [ ] Invalid/missing auth always returns **401**, forbidden business states **403**  
- [ ] Password reset/change invalidates **all** refresh sessions  
- [ ] Refresh re-validates account still allowed to use the system  
- [ ] Public endpoints are rate-limited; login lockout enabled  
- [ ] No secrets or raw refresh tokens exposed  
- [ ] Optional: global fallback deny + explicit anonymous on public routes  
- [ ] Tests cover the matrix in §4  

**Current state:** attributes are **mostly correct**; session and policy layer is **not complete**.

---

## 9. Bottom line

| Layer | Status |
|-------|--------|
| **Attribute placement on Auth endpoints** | **Mostly correct** — only missing explicit anonymous on refresh/revoke |
| **Only protected Auth endpoint** | `change-password` — **correct** |
| **Authorization completeness** | **Incomplete** — session revoke, status re-check on refresh, rate limit, lockout, policies, operational endpoints |
| **Wrong/inverted Authorize on Auth** | **None found** |

Fix attributes for safety, then treat **session invalidation and continuous authorization checks** as first-class authorization work—not optional polish.

---

## Related docs

- `docs/Reviews/auth_module_fix_plan_v1_v2_gaps.md` — implementation plan for V1/V2 gaps  
- `docs/Reviews/auth_module_architectural_review_v2.md` — full architecture review  
- `docs/Plans/06_API_Auth_Users.md` — API contract  

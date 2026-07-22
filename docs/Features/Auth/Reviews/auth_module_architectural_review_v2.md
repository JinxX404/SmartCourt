# Authentication Module — Architecture & Code Review

**Version:** 2.0  
**Date:** 2026-07-16  
**Baseline:** V1 review (pre-slice / no refresh tokens)  
**Current codebase:** Post-refactor Auth module (vertical slices + refresh tokens)  
**Contract source:** `docs/Plans/06_API_Auth_Users.md`  
**Scope:** `SmartCourt/Features/Auth/**`, `IAuthService`, `IJwtProvider`, DI, exception middleware, `ApiResponse`

---

## Document history

| Version | Focus |
|---------|--------|
| **V1** | Initial full-module review against contract |
| **V1.1** (interim re-review) | First pass after slice split + refresh tokens |
| **V2** (this document) | Standalone authoritative re-baseline of the **current** Auth module |

> **Git context at review time:** Latest Auth-related commits include refresh tokens (`36a786f`), feature/auth merge (`21a6120`), and email template extraction (`90379cc`). Working tree Auth source matches that state; no additional uncommitted Auth code changes beyond this review doc series.

---

## 1. Executive Summary

### Overall health: **Fair — improved architecture, incomplete contract & security**

The Auth module has moved from a single monolithic service to a **credible vertical-slice layout** with refresh-token infrastructure. Controllers remain thin. Core happy paths (register, login with RT, confirm email, refresh rotation, revoke, password flows) exist.

It is **not yet contract-complete** with `06_API_Auth_Users.md` and **not production-ready** without hardening.

| Dimension | V1 | V2 (current) | Trend |
|-----------|----|--------------|--------|
| Controller SRP | Good | **Good** | → |
| Service SRP / slices | Fair | **Good** | ↑ |
| Contract compliance | Poor | **Poor–Fair** | ↑ |
| Security posture | Poor–Fair | **Fair** | ↑ |
| Domain completeness | Poor | **Poor** | → |
| Testability | Poor | **Fair** | ↑ |
| Production readiness | Not ready | **Not ready** | → |

### Scorecard (V2)

| Area | Grade | One-line assessment |
|------|-------|---------------------|
| Controllers | **A** | HTTP-only; no business/DB leakage |
| Slice services | **A-** | Clean; residual `AuthService` remains |
| Contract fidelity | **C-** | Refresh exists; shapes/paths/status codes diverge |
| Token security | **C+** | Rotation + replay handling good; plaintext RT, no password-event revoke |
| Domain side-effects | **D** | No profiles/prefs on register; status lifecycle incomplete |
| Operational security | **D** | Secrets in config, no rate limit/lockout, error leakage |
| Tests | **F** | No auth unit/integration tests found |

### Top P0 blockers (must fix before real traffic)

1. **Login/refresh response schemas** do not match contract (`accessToken`, nested `user`, `expiresAt`).
2. **Refresh request contract** requires access+refresh; docs specify refresh-only; failures return **400** not **401**.
3. **Registration does not create** `ClientProfile` / `LawyerProfile` (or notification preferences).
4. **Resend-verification URL is broken** (`email=` vs confirm’s `userId=`).
5. **Password reset/change do not revoke** refresh tokens.
6. **Refresh tokens stored in plaintext**; login does not `Include` existing RTs.
7. **Secrets committed** in `appsettings*.json`.
8. **Lockout off, no rate limiting, forgot-password enumerates** unverified accounts.

### What V2 recognizes as done well

- Per-feature services + interfaces + DI registration  
- `IJwtProvider` (generate + validate)  
- `RefreshTokens` owned entity + EF migration  
- RT rotation and **reuse → revoke-all active**  
- `POST /api/auth/revoke` (extra capability)  
- HTML email templates under `Shared/Templates/`  
- `IAuthHelperService` for shared role/email/RT generation  
- FluentValidation + Identity password hashing  
- Arabic messages on most user-facing success/auth paths  

---

## 2. Contract & Functionality Report

### Legend

| Status | Meaning |
|--------|---------|
| Compliant | Matches contract |
| Partial | Works but shape/status/message differs |
| Non-compliant | Wrong surface or broken intent |
| Missing | Not implemented |

---

### 2.1 `POST /api/auth/register/client`

| Check | Status | Detail |
|-------|--------|--------|
| Route / `[AllowAnonymous]` / 201 | Compliant | `RegisterClientController` |
| Arabic success message | Compliant | Set on `ApiResponse` |
| Request | Partial | Extra required `nationalNumber` (entity-driven; not in auth contract body) |
| Response data | Compliant | `userId`, `email`, `fullName`, `role: "Client"` |
| 409 email exists | Non-compliant | Identity errors → **400** `ValidationException` |
| Create `ClientProfile` | **Missing** | `RegisterClientService` never sets profile |
| Create `NotificationPreference` | **Missing** | Feature/entity absent |
| Send verification email | Compliant | Via `IAuthHelperService` |
| No JWT until verified | Compliant | |

**Edge cases:** password match checked in validator **and** service; no DB transaction around create + role + email; user remains `UserStatus.Unverified` even after later email confirm.

---

### 2.2 `POST /api/auth/register/lawyer`

| Check | Status | Detail |
|-------|--------|--------|
| Route / 201 | Compliant | |
| Content-Type | **Non-compliant** | Contract: `multipart/form-data` + 4 files. Code: `[FromBody]` JSON, no files |
| Core fields | Partial | Phone, address, gov, city, gender, national number validated |
| `LawyerProfile` + verification defaults | **Missing** | No profile row; no pending verification docs |
| Role `Lawyer` | Compliant | |

---

### 2.3 `POST /api/auth/login`

| Check | Status | Detail |
|-------|--------|--------|
| Invalid credentials → 401 | Compliant | Generic Arabic message |
| Unverified → 403 | Compliant | Correct message |
| Suspended → 403 | Partial | Uses `UserStatus.Suspended`; contract also mentions `IsActive` (not on entity) |
| Issues refresh token | Compliant (feature) | Stored + returned |
| Response schema | **Non-compliant** | See comparison below |
| Lockout on failure | Non-compliant | `lockoutOnFailure: false` |
| Last login tracking | Missing | No `LastLoginAt` |

**Contract data:**

```json
{
  "accessToken": "string",
  "refreshToken": "string",
  "expiresAt": "datetime",
  "user": {
    "id": "uuid",
    "email": "string",
    "fullName": "string",
    "role": "Client | Lawyer | Admin",
    "profilePictureUrl": "string | null",
    "isVerified": true
  }
}
```

**Actual `LoginResponse`:**

```csharp
(Id, Email, FullName, Role, Token, ExpiresIn, RefreshToken, RefreshTokenExpiration)
```

| Contract field | Implementation |
|----------------|----------------|
| `accessToken` | `token` |
| `refreshToken` | `refreshToken` ✓ |
| `expiresAt` (access) | `expiresIn` (seconds int) |
| nested `user` | flattened properties |
| `profilePictureUrl` | missing |
| `isVerified` | missing |
| refresh TTL | **14 days hardcoded** (contract: **7 days**) |

---

### 2.4 `POST /api/auth/refresh` — implemented (V2 focus)

| Check | Status | Detail |
|-------|--------|--------|
| Endpoint exists | Compliant | `api/auth/refresh` |
| Request body | **Non-compliant** | Code: `{ token, refreshToken }`. Contract: `{ refreshToken }` only |
| Response body | Partial | Full `LoginResponse` returned. Contract: `{ accessToken, refreshToken, expiresAt }` |
| Rotation | Compliant | Old RT `RevokedOn` set; new RT issued |
| Replay attack handling | Compliant | Inactive RT reuse revokes **all** active tokens |
| Invalid/expired → 401 | **Non-compliant** | `BusinessException` → **400** + English messages |
| AllowAnonymous | Implicit | No global authorize assumed |

**Design risk:** User resolution depends on **access token** (lifetime validation disabled). Valid refresh alone is insufficient if access token is missing/corrupt — diverges from contract and common RT-only designs.

---

### 2.5 `POST /api/auth/revoke` — extra (not in contract)

| Status | Positive optional feature |
|--------|---------------------------|
| Issues | Dual-token requirement; English errors; inactive RT returns `false` (200) instead of consistent 401 |

Useful building block for logout; should be documented if kept.

---

### 2.6 Email verification

| Contract | Implementation | Status |
|----------|----------------|--------|
| `POST /api/auth/verify-email` + body | `GET /api/auth/confirm-email?userId&token` | **Non-compliant** |
| Confirm success message | Present (`data: true` + message) | Partial |
| `EmailConfirmed = true` | Yes (Identity) | Compliant |
| Promote status to Active | No | Gap |
| Initial email link | Uses `userId=` | Compliant |

---

### 2.7 `POST /api/auth/resend-verification`

| Check | Status | Detail |
|-------|--------|--------|
| Always 200 (anti-enum) | Compliant | Unknown / already verified → silent |
| Rate limit 3/hour | **Missing** | |
| Link correctness | **Broken** | Builds `?email={email}&token=...`; confirm needs **`userId`** |
| Message placement | Partial | String placed in **`data`**, not `message` |

```csharp
// AuthService.ResendVerificationEmailAsync — broken
var confirmationUrl = $"{_appUrl}/api/auth/confirm-email?email={email}&token={encodedToken}";

// AuthHelperService.SendConfirmationEmailAsync — correct
// ...confirm-email?userId={user.Id}&token={encodedToken}
```

---

### 2.8 `POST /api/auth/forgot-password`

| Check | Status | Detail |
|-------|--------|--------|
| Always 200 | **Non-compliant** | Unverified → **400** `"البريد الإلكتروني غير مؤكد"` (enumeration) |
| Unknown email | Compliant | Silent return |
| Template | Compliant | `ResetPasswordEmail.html` |
| Rate limit | Missing | |
| Token TTL 1 hour | Unconfigured | Identity default |

---

### 2.9 `POST /api/auth/reset-password`

| Check | Status | Detail |
|-------|--------|--------|
| Body shape | Compliant | email, token, newPassword, confirmNewPassword |
| Success | Partial | Message in `data` |
| Invalidate all refresh tokens | **Missing** | Contract requires it |
| Strength | Partial | Validator min 8; Identity enforces digit/upper/lower |

---

### 2.10 `POST /api/auth/change-password`

| Check | Status | Detail |
|-------|--------|--------|
| `[Authorize]` + claim user id | Compliant | |
| Wrong current password | Compliant | 400 via validation mapping |
| Revoke sessions / RTs | **Missing** | Recommended for security |
| Message placement | Partial | In `data` |

---

### 2.11 Cross-cutting response/error behavior

| Topic | Status |
|-------|--------|
| `ApiResponse` fields | Compliant structure |
| Success messages via `Ok("text")` | Partial — ends up in **`data`** |
| Unhandled 500 body | Non-compliant — includes `exception.ToString()` |
| Locale consistency | Partial — refresh/revoke English; rest Arabic |

---

### 2.12 Endpoint compliance matrix (V2)

| Endpoint | Implemented | Contract match | Notes |
|----------|-------------|----------------|-------|
| `POST /register/client` | Yes | Partial | No profile/prefs; no 409 |
| `POST /register/lawyer` | Yes | Poor | Not multipart; no profile/docs |
| `POST /login` | Yes | Partial | RT yes; schema wrong |
| `POST /refresh` | Yes | Partial | Rotation OK; body/status/shape wrong |
| `POST /revoke` | Yes (extra) | N/A | Document or keep as extension |
| `POST /verify-email` | No | Poor | GET `confirm-email` instead |
| `POST /resend-verification` | Yes | Poor | Broken URL; no rate limit |
| `POST /forgot-password` | Yes | Partial | Enumerates unverified |
| `POST /reset-password` | Yes | Partial | No RT revoke |
| `POST /change-password` | Yes | Partial | No RT revoke |

---

## 3. Architecture & SRP Report

### 3.1 Controllers — **A**

Responsibilities observed:

- Routing and HTTP verbs  
- Auth attributes  
- DTO binding  
- Call service  
- Wrap `ApiResponse`  

**No** business logic, DB access, or exception formatting in controllers.

**Nits**

- Register controllers mutate `Message` after `Created()` factory.  
- Residual password/resend controllers still depend on fat residual `IAuthService`.

---

### 3.2 Services — **A- / C mixed**

| Component | SRP | Notes |
|-----------|-----|-------|
| `LoginService` | Good | Auth + token issue; no HTTP |
| `RegisterClientService` | Good | Missing required domain side-effects |
| `RegisterLawyerService` | Good | Same; incomplete vs product rules |
| `ConfirmEmailService` | Good | No status transition |
| `RefreshTokenService` | Good core | Wrong exception type/status vs auth failures |
| `RevokeRefreshTokenService` | Good | |
| `AuthHelperService` | Good shared | AppUrl via raw config; template I/O |
| `AuthService` (residual) | Weak | Still owns 4 flows; should be split like others |
| `JwtProvider` : `IJwtProvider` | Good | Validate skips iss/aud |

**Services correctly throw domain exceptions** rather than returning status codes.

**Service should do but doesn’t**

- Profile + prefs on register  
- Status → Active on confirm  
- Hash RTs / include RTs on login  
- Revoke RTs on password change/reset  
- Map duplicate email to conflict semantics (or dedicated exception)  
- Configurable RT expiry (7 days)

**Service should not do (minor)**

- Duplicate confirm-password checks (validator owns this)  
- Large template path composition (acceptable; could be `IEmailTemplateProvider`)

---

### 3.3 Recommended refactors (V2)

#### A. Contract DTOs

```csharp
public record AuthTokensDto(string AccessToken, string RefreshToken, DateTime ExpiresAt);

public record LoginUserDto(
    string Id, string Email, string FullName, string Role,
    string? ProfilePictureUrl, bool IsVerified);

public record LoginResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt, LoginUserDto User);
```

#### B. Refresh by refresh token only + 401

```csharp
// Lookup by SHA256(refreshToken); on failure throw AuthenticationException
Task<AuthTokensDto> RefreshAsync(string refreshToken, CancellationToken ct);
```

#### C. Register with profile

```csharp
var user = new ApplicationUser { /* ... */, ClientProfile = new ClientProfile() };
// Lawyer: LawyerProfile = new() { /* verification defaults when model exists */ }
```

#### D. Fix resend URL + always-200 forgot

```csharp
// resend
$"{_appUrl}/api/auth/confirm-email?userId={user.Id}&token={encodedToken}";

// forgot: if (!user.EmailConfirmed) return; // do not throw
```

#### E. Revoke RTs on password events

```csharp
foreach (var t in user.RefreshTokens.Where(x => x.IsActive))
    t.RevokedOn = DateTime.UtcNow;
```

#### F. Finish vertical slice split

Move residual `AuthService` methods into:

- `ForgotPasswordService`  
- `ResetPasswordService`  
- `ChangePasswordService`  
- `ResendVerificationService`  

#### G. Success responses

```csharp
return Ok(ApiResponse<object?>.Ok(null, "تم تغيير كلمة المرور بنجاح"));
```

---

### 3.4 Persistence model gaps

| Expected (docs) | V2 state |
|-----------------|----------|
| Refresh tokens | **Present** (`RefreshTokens` owned + migration) |
| Client/Lawyer profiles on register | **Not created** |
| Lawyer verification fields / availability | Stub profile only |
| `LastLoginAt`, `IsActive` | Missing on `ApplicationUser` |
| NotificationPreference | Missing |
| RT hashed at rest | **Plaintext** |

---

### 3.5 Architecture diagram (current)

```
Controllers (HTTP)
    │
    ├─ LoginController ──────────────► ILoginService
    ├─ RegisterClientController ─────► IRegisterClientService
    ├─ RegisterLawyerController ─────► IRegisterLawyerService
    ├─ ConfirmEmailController ───────► IConfirmEmailService
    ├─ RefreshTokenController ───────► IRefreshTokenService
    ├─ RevokeRefreshTokenController ─► IRevokeRefreshTokenService
    └─ Forgot/Reset/Change/Resend ───► IAuthService (residual)

Shared: IAuthHelperService, IJwtProvider, IEmailProvider, UserManager/SignInManager
```

---

## 4. Code Quality Report

### 4.1 Security

| Severity | Finding |
|----------|---------|
| **Critical** | Secrets in repo (`appsettings.json` / Development): JWT secret, DB password, SMTP credentials, Supabase **service role** key |
| **High** | Refresh tokens stored **plaintext** |
| **High** | Password reset/change do **not** revoke refresh tokens |
| **High** | Account lockout disabled |
| **High** | Forgot-password reveals unverified state |
| **High** | Middleware returns full exception text on 500 |
| **Medium** | Broken resend verification links |
| **Medium** | No rate limiting (login / forgot / resend / refresh) |
| **Medium** | Auth failures on refresh/revoke mapped as business 400 |
| **Medium** | `ValidateToken` does not validate issuer/audience |
| **Low** | Seeded default passwords; default `AppUrl` localhost |

**Strengths:** Identity password hashing; crypto-strong RT generation (64 random bytes); RT rotation; replay → revoke-all.

### 4.2 Logic & correctness bugs

1. Resend verification uses `email=` instead of `userId=`.  
2. Login uses `FindByEmailAsync` without `Include(RefreshTokens)` — owned collection tracking risk.  
3. Confirm email does not set `UserStatus.Active`.  
4. Register does not create profiles.  
5. Forgot-password anti-enumeration broken for unverified.  
6. Duplicate email → 400 not 409.  
7. RT expiry hardcoded 14d vs contract 7d / config.  
8. Success message placement in `data`.  
9. Unused usings in `LoginService` (`EF Core`, `Cryptography` appear unused).  

### 4.3 Best practices vs AGENTS.md

| Rule | V2 |
|------|----|
| Vertical slices + services | **Mostly yes** (residual `AuthService`) |
| `ApiResponse<T>` | Yes |
| Domain exceptions | Yes (wrong type for some auth failures) |
| FluentValidation | Yes |
| No AutoMapper | Yes |
| Provider pattern | Yes (`IEmailProvider`, `IJwtProvider`) |
| async/await | Yes in Auth paths |
| Options pattern | Partial (`JwtOptions` yes; AppUrl / RT days no) |

### 4.4 Redundancy

| Duplication | Fix |
|-------------|-----|
| Password rules in multiple validators | Shared FluentValidation extension |
| Confirm password in service + validator | Validator only |
| RT expiry days in Login + Refresh | `JwtOptions.RefreshTokenExpiryDays` |
| AppUrl in AuthService + AuthHelper | Shared `AppOptions` |
| Template load/replace | Shared template helper |

### 4.5 Testability

**No automated Auth tests.**

Easier than V1:

- Small interfaces per use case  
- Mockable `IJwtProvider`, `IAuthHelperService`  

Still hard:

- Heavy Identity managers  
- File-based templates  
- No clock abstraction  

**Minimum V2 test matrix**

| Case | Expected |
|------|----------|
| Login success | AT + RT persisted |
| Login unverified/suspended | 403 |
| Refresh rotates | Old RT inactive; new active |
| Refresh replay | All active revoked; 401 |
| Register client | `ClientProfile` row exists |
| Resend | Confirm URL contains `userId` |
| Forgot unknown/unverified | Always 200 |
| Reset/change password | All RTs revoked |

---

## 5. Gap Analysis Report — Path to a production-ready Auth module

### 5.1 Must-have

| Feature | V2 state | Action |
|---------|----------|--------|
| Contract login/refresh DTOs | Partial | Align names + nesting |
| Refresh by RT only | No | Hash lookup; drop AT requirement (or document extension) |
| 401 on bad refresh | No | `AuthenticationException` |
| Profile creation | No | Client/Lawyer on register |
| Lawyer multipart/docs | No | Implement or formally defer in contract |
| Verify-email alignment | No | POST body or versioned API note |
| Fix resend URL | Broken | Use `userId` |
| Revoke RTs on password events | No | Implement |
| Hash RTs at rest | No | SHA-256 store/compare |
| Rate limiting | No | Public auth endpoints |
| Lockout | Off | Enable Identity lockout |
| Anti-enumeration forgot | No | Never throw on unverified |
| 409 duplicate email | No | Map Identity codes |
| Secrets management | Unsafe | Env/KeyVault; rotate exposed keys |
| Token provider TTLs | Default | Email/reset 1h |
| Status lifecycle | Incomplete | Unverified → Active |

### 5.2 Should-have

- `LastLoginAt` + auth audit log  
- Single source of truth: `UserStatus` vs `IsActive`  
- Configurable RT expiry (7 days)  
- Cap/prune active RTs per user  
- SPA deep links for confirm/reset  
- Sanitize 500 responses  
- Hangfire/Swagger protection  
- Shared password validation matching Identity  

### 5.3 Nice-to-have

- 2FA/MFA (TOTP/SMS via `ISmsProvider`)  
- Session list / remote logout UI  
- CAPTCHA on public endpoints  
- Breached-password check  
- Full integration suite + OWASP checklist  

### 5.4 Recommended delivery phases

#### Phase 1 — Contract + critical bugs (highest ROI)

1. Fix resend `userId` URL  
2. Align login/refresh response DTOs  
3. Create profiles on register  
4. Revoke all RTs on reset/change  
5. Hash RTs; `Include(RefreshTokens)` on login  
6. Forgot always 200  
7. Secrets out of git + rotate  

#### Phase 2 — Hardening

1. Lockout + rate limiting  
2. RT expiry config (7 days)  
3. Refresh 401 + Arabic errors  
4. Verify-email surface alignment  
5. Status transitions + 409  
6. Middleware sanitization  
7. Finish residual `AuthService` split  

#### Phase 3 — Completeness

1. Lawyer registration documents / verification model  
2. Audit / LastLogin  
3. MFA readiness  
4. Automated tests  

---

## V1 → V2 progress tracker

| V1 P0 item | V2 status |
|------------|-----------|
| No refresh tokens | **Done** (gaps on contract shape remain) |
| Login schema wrong | **Open** |
| No profile creation | **Open** |
| Broken resend link | **Open** |
| Secrets committed | **Open** |
| Monolithic service | **Mostly done** (residual password/resend) |
| No `IJwtProvider` | **Done** |
| Inline email HTML | **Done** (templates) |

---

## Bottom line (V2)

The Auth module is a **solid intermediate implementation**: vertical slices, JWT abstraction, and refresh-token rotation/replay handling are real architectural wins over V1.

It is **not yet the documented production Auth API** and **not yet safe for production traffic**. V2 priority is no longer “add structure” — it is **contract alignment, correctness bugs (especially resend URL and profiles), session invalidation, token hashing, and operational security (secrets, lockout, rate limits).**

---

## Appendix A — Files reviewed (V2)

| Path | Role |
|------|------|
| `Features/Auth/Login/*` | Login slice |
| `Features/Auth/RegisterClient/*` | Client registration |
| `Features/Auth/RegisterLawyer/*` | Lawyer registration |
| `Features/Auth/ConfirmEmail/*` | Email confirmation |
| `Features/Auth/RefreshToken/*` | Refresh entity + API |
| `Features/Auth/RevokeRefreshToken/*` | Revoke API |
| `Features/Auth/AuthService.cs`, `Interfaces/IAuthService.cs` | Residual password/resend |
| `Features/Auth/Shared/*` | Helper + templates |
| `Features/Auth/JwtProvider.cs`, `Interfaces/Providers/IJwtProvider.cs` | JWT |
| `Features/Auth/ApplicationUser.cs`, profiles, `UserConfiguration.cs` | Model |
| Controllers: Change/Forgot/Reset/Resend | Residual HTTP |
| `DependencyInjection.cs` | Identity + service registration |
| `Middleware/ExceptionHandlingMiddleware.cs` | Error mapping |
| `Migrations/*AddRefreshTokensTable*` | RT persistence |
| `docs/Plans/06_API_Auth_Users.md` | API contract |

---

## Appendix B — Quick fix checklist (copy into sprint board)

- [ ] Resend verification URL uses `userId`  
- [ ] Login response matches contract (`accessToken`, nested `user`, `expiresAt`)  
- [ ] Refresh request/response matches contract; invalid → 401  
- [ ] Register client/lawyer creates profiles  
- [ ] Reset + change password revoke all refresh tokens  
- [ ] Store hashed refresh tokens; include collection on login  
- [ ] Forgot-password always 200  
- [ ] Duplicate email → 409  
- [ ] Confirm email sets `UserStatus.Active`  
- [ ] Enable lockout + rate limiting  
- [ ] Remove secrets from appsettings; rotate keys  
- [ ] Sanitize unhandled exception responses  
- [ ] Split residual `AuthService` into slices  
- [ ] Add Auth unit/integration tests  

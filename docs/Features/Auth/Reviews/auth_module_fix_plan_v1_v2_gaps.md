# Auth Module — Implementation Fix Plan (V1 / V2 Gaps)

> **Purpose:** Actionable plan for everything still **missing**, **illogical**, or **unaligned** with `docs/Plans/06_API_Auth_Users.md` and production security, as identified in Auth reviews V1 and V2.  
> **Date:** 2026-07-16  
> **Sources:** V1 review, V2 review (`auth_module_architectural_review_v2.md`), current code under `SmartCourt/Features/Auth/`

---

## 1. How to use this plan

| Priority | Meaning |
|----------|---------|
| **P0** | Broken, insecure, or blocks contract clients — fix first |
| **P1** | Contract or security hardening — required for production |
| **P2** | Quality, maintainability, completeness |

Each item lists: **problem → target state → suggested approach → effort**.

---

## 2. Gap inventory (V1 + V2 still open)

### 2.1 P0 — Critical correctness & contract blockers

| ID | Area | Problem (current) | Target (contract / secure) | Suggested implementation | Effort |
|----|------|-------------------|----------------------------|--------------------------|--------|
| **P0-01** | Resend verification | URL uses `?email=` but confirm expects `userId` — **links never work** | Same URL shape as initial confirm (`userId` + token) | In `AuthService.ResendVerificationEmailAsync`, build URL like `AuthHelperService` (`userId={user.Id}`) | XS |
| **P0-02** | Login response | Returns `token`, `expiresIn`, flattened user | `{ accessToken, refreshToken, expiresAt, user: { id, email, fullName, role, profilePictureUrl, isVerified } }` | Redesign `LoginResponse` + map in `LoginService`; keep camelCase JSON | S |
| **P0-03** | Refresh request | Requires `{ token, refreshToken }` | `{ refreshToken }` only | Lookup user by **hashed** RT (query all active or index hash); drop access-token dependency | M |
| **P0-04** | Refresh response | Full `LoginResponse` | `{ accessToken, refreshToken, expiresAt }` only | New `RefreshTokenResponse` / `AuthTokensDto` | S |
| **P0-05** | Refresh status codes | Invalid RT → `BusinessException` → **400** | **401** | Throw `AuthenticationException` (Arabic message) | XS |
| **P0-06** | Register client/lawyer | No `ClientProfile` / `LawyerProfile` created | Create profile rows per contract/sequence | Set navigation on user before/after create; ensure cascade save | S |
| **P0-07** | Password reset/change | Does **not** revoke refresh tokens | Invalidate **all** active RTs (force re-login) | Load user + `Include(RefreshTokens)`, set `RevokedOn` on all active | S |
| **P0-08** | Secrets | JWT/DB/SMTP/Supabase keys in `appsettings*.json` | Env vars / User Secrets / Key Vault; rotate leaked keys | Remove secrets from git; use configuration providers | S |
| **P0-09** | RT storage | Refresh tokens stored **plaintext** | Store **hash** (e.g. SHA-256); compare hash only | Hash on issue; never log raw RT | S |
| **P0-10** | Login RT tracking | `FindByEmailAsync` without `Include(RefreshTokens)` | Load owned collection before add/update | `Users.Include(u => u.RefreshTokens)...` | XS |

### 2.2 P1 — Contract alignment & production security

| ID | Area | Problem | Target | Approach | Effort |
|----|------|---------|--------|----------|--------|
| **P1-01** | Verify email surface | `GET /confirm-email` | `POST /api/auth/verify-email` + body `{ userId, token }` | Add endpoint (keep GET as redirect compatibility optional) | S |
| **P1-02** | Duplicate email | Identity → 400 | **409 Conflict** | Map `DuplicateEmail` / unique violation to dedicated exception or middleware status | S |
| **P1-03** | Forgot password | Unverified → 400 (enumeration) | Always **200** | Silent return when user null **or** unconfirmed | XS |
| **P1-04** | RT expiry | Hardcoded **14** days | Config **7** days (contract) | `JwtOptions.RefreshTokenExpiryDays = 7` used by Login + Refresh | XS |
| **P1-05** | Rate limiting | None | max 3/hour email on resend + forgot; login/refresh IP limits | ASP.NET Rate Limiter policies | M |
| **P1-06** | Account lockout | `lockoutOnFailure: false` | Enable Identity lockout | Configure lockout options; handle locked → 403/401 | S |
| **P1-07** | Email confirm status | Only sets `EmailConfirmed` | Set `UserStatus.Active` (or agreed lifecycle) | Update in `ConfirmEmailService` after success | XS |
| **P1-08** | Lawyer register | JSON body, no files | Contract multipart + ID/bar docs **or** documented deferral | Either implement multipart upload + profile verification fields, or update contract intentionally | L |
| **P1-09** | NotificationPreference | Missing entirely | Create defaults on register | Entity + create on register | M |
| **P1-10** | `isVerified` on login | Missing | Lawyers: both NationalId + BarCard approved; clients: define rule | Compute when mapping login user DTO | M (depends on profile model) |
| **P1-11** | `profilePictureUrl` | Missing | null or resolved URL | Map from future file field | S |
| **P1-12** | ApiResponse messages | Success text often in `data` | Text in `message`, `data` null for voids | `ApiResponse.Ok(null, message)` or `OkWithMessage` helper | XS |
| **P1-13** | 500 responses | Leak `exception.ToString()` | Generic Arabic/English safe message; log server-side only | Fix `ExceptionHandlingMiddleware` | XS |
| **P1-14** | Token TTLs | Identity defaults | Confirm/reset tokens **1 hour** (forgot contract) | `DataProtectionTokenProviderOptions.TokenLifespan` | XS |
| **P1-15** | Register client body | Extra required `nationalNumber` | Align contract **or** document as intentional product rule | Update OpenAPI/plan if keeping field | XS |
| **P1-16** | Refresh/revoke errors | English strings | Arabic consistent with rest of API | Message constants | XS |

### 2.3 P2 — Quality, architecture, completeness

| ID | Area | Problem | Target | Approach | Effort |
|----|------|---------|--------|----------|--------|
| **P2-01** | Residual `AuthService` | Forgot/reset/change/resend still monolithic | Per-slice services like Login/Register | Split + DI register | M |
| **P2-02** | Password rules | Duplicated across validators; reset/change weaker than register | Shared FluentValidation password rule matching Identity | Extension method | S |
| **P2-03** | Confirm password | Checked in service + validator | Validator only | Delete service checks | XS |
| **P2-04** | AppUrl config | Raw `IConfiguration` in 2 places | Typed `AppOptions` | Options pattern | S |
| **P2-05** | Templates | File I/O per email | Cache templates / provider | `IAuthEmailComposer` | S |
| **P2-06** | LastLoginAt | Missing | Set on successful login | Add column + migration | S |
| **P2-07** | IsActive vs Status | Contract uses IsActive; code uses enum | Single source of truth | Document mapping; enforce Suspended everywhere | S |
| **P2-08** | Tests | None | Unit + integration for matrix in V2 | xUnit + WebApplicationFactory | L |
| **P2-09** | OpenAPI | Stale (`firstName`/`lastName`) | Sync with plan + implementation | Update OpenAPI files | S |
| **P2-10** | Hangfire / Swagger | Unauthenticated dashboard | Secure or disable in prod | Auth filter / env gate | S |
| **P2-11** | Transactions | Partial register if email fails after create | Transaction or outbox | `IDbContextTransaction` / compensate | M |
| **P2-12** | MFA / audit / session list | Missing | Phase 3 “perfect auth” | Separate backlog | L |

---

## 3. Illogical / inconsistent behaviors (fix even if “works”)

| # | Behavior | Why it’s illogical | Fix |
|---|----------|--------------------|-----|
| 1 | Initial confirm email uses `userId`; resend uses `email` | Same product flow, two URL formats; one always fails | Unify on `userId` |
| 2 | Refresh requires access token **and** refresh token | Contract is RT-only; expired clients may not have a parseable AT | RT-only lookup |
| 3 | Replay revoke uses 400 | Auth failures should be 401 | `AuthenticationException` |
| 4 | Forgot password reveals “email not confirmed” | Defeats anti-enumeration on a public endpoint | Always 200 |
| 5 | RT expiry 14d hardcoded while JWT options exist for access TTL | Inconsistent configuration story | Options |
| 6 | Login returns user fields; refresh returns same bulky payload | Different contracts; clients hard to version | Separate DTOs |
| 7 | Success messages put in `data` as `string` | Breaks wrapper semantics (`message` vs `data`) | Fix factory usage |
| 8 | Password change does not kill other sessions | User changes password but stolen RT still works | Revoke all RTs |
| 9 | Register creates role user without profile | Later profile APIs depend on profile existence | Create on register |
| 10 | Lawyer “registration” without documents | Contract says verification starts at register | Implement or change contract |
| 11 | Status stays `Unverified` after email confirm | Enum becomes meaningless for access control | Transition to Active |
| 12 | Dual password confirmation in service | Already FluentValidated | Remove redundancy |

---

## 4. Unaligned items (implementation ≠ contract)

| Contract rule | Implementation | Unaligned? |
|---------------|----------------|------------|
| Register client fields | + `nationalNumber` required | Yes (stricter) |
| Register lawyer multipart + 4 files | JSON, no files | Yes |
| Create ClientProfile / LawyerProfile | Not created | Yes |
| NotificationPreference defaults | Missing | Yes |
| Login response shape | Different property names/structure | Yes |
| Access expiry as `expiresAt` datetime | `expiresIn` seconds | Yes |
| Refresh body = refreshToken only | token + refreshToken | Yes |
| Refresh response minimal tokens | Full login DTO | Yes |
| Refresh invalid → 401 | 400 | Yes |
| RT expiry 7 days | 14 days | Yes |
| Verify-email POST | confirm-email GET | Yes |
| Resend always 200 + rate limit | 200 yes; rate limit no; broken link | Partial |
| Forgot always 200 + rate limit | Not always 200; no rate limit | Yes |
| Reset invalidates RTs | No | Yes |
| Change-password `[Authorize]` | Correct attribute | OK (see Report 2) |
| JWT claims sub, email, role, jti, exp, iss, aud | Present (role via `ClaimTypes.Role`) | Mostly OK |
| Suspended / unverified 403 | Implemented on login | OK |
| User-facing Arabic | Mostly; refresh/revoke English | Partial |

---

## 5. Recommended implementation order (sprint plan)

### Sprint A — Stop the bleeding (P0 correctness)

1. **P0-01** Fix resend URL (`userId`)  
2. **P0-10** Include refresh tokens on login  
3. **P0-07** Revoke all RTs on reset + change password  
4. **P0-06** Create Client/Lawyer profiles on register  
5. **P1-03** Forgot-password always 200  
6. **P1-12** Fix success `message` vs `data`  
7. **P1-13** Sanitize 500 middleware  

**Exit criteria:** Email resend works; password change logs out other sessions; profiles exist after register.

### Sprint B — Contract-facing API

1. **P0-02** Login DTO alignment  
2. **P0-03 / P0-04 / P0-05** Refresh contract + 401  
3. **P0-09** Hash refresh tokens  
4. **P1-01** POST verify-email (optional keep GET)  
5. **P1-04** RT expiry 7 days via options  
6. **P1-02** 409 on duplicate email  
7. **P1-07** Status → Active on confirm  
8. **P1-16** Arabic auth error messages  

**Exit criteria:** Mobile/web clients can implement against `06_API_Auth_Users.md` without custom mapping hacks.

### Sprint C — Hardening

1. **P0-08** Secrets out of repo + rotation  
2. **P1-05** Rate limiting  
3. **P1-06** Lockout  
4. **P1-14** Token provider 1h lifespan  
5. **P2-10** Secure Hangfire/Swagger  

**Exit criteria:** Abuse-resistant public auth surface; no secrets in git.

### Sprint D — Completeness & quality

1. **P1-08 / P1-09 / P1-10** Lawyer docs, notification prefs, isVerified  
2. **P2-01** Finish slice split of residual `AuthService`  
3. **P2-02 / P2-03** Shared validation  
4. **P2-08** Automated tests  
5. **P2-09** OpenAPI sync  
6. **P2-06** LastLoginAt  

**Exit criteria:** Auth module matches product docs for registration/verification; CI covers critical paths.

---

## 6. Suggested task breakdown (engineering checklist)

### Domain / API

- [ ] `LoginResponse` + `LoginUserDto` + `RefreshTokenResponse`  
- [ ] `RefreshTokenRequest` = refresh only  
- [ ] `VerifyEmailRequest` + POST endpoint  
- [ ] Profile creation in register services  
- [ ] Shared `RevokeAllRefreshTokensAsync(user)` helper  
- [ ] Hash RT helper on issue + lookup  

### Security / infra

- [ ] `JwtOptions.RefreshTokenExpiryDays`  
- [ ] `AppOptions.AppUrl`  
- [ ] Rate limiter policies: `auth-forgot`, `auth-resend`, `auth-login`  
- [ ] Identity lockout options  
- [ ] Token lifespan 1 hour  
- [ ] Middleware safe 500 body  
- [ ] Secrets migration runbook  

### Data

- [ ] Ensure `RefreshTokens` migration applied  
- [ ] Profile rows for new users  
- [ ] Optional: `LastLoginAt` migration  
- [ ] Optional: `NotificationPreference` table  

### Tests

- [ ] Resend URL contains `userId`  
- [ ] Login schema snapshot/contract test  
- [ ] Refresh rotation + replay  
- [ ] Reset password revokes RTs  
- [ ] Forgot always 200  
- [ ] Register creates profile  

---

## 7. Out of scope for this plan (track separately)

- Full Users/profile API contract (`GET/PUT /api/users/profile`)  
- Lawyer admin verification workflow  
- 2FA/MFA product design  
- Social login  

---

## 8. Bottom line

V1/V2 already built **structure** (slices, JWT provider, refresh rotation).  
This plan is only about what is **still wrong or incomplete**:

1. **Broken:** resend link  
2. **Unaligned:** login/refresh/verify surfaces and DTOs  
3. **Missing domain side-effects:** profiles, RT revoke on password events, prefs  
4. **Unsafe defaults:** plaintext RTs, secrets, no rate limit/lockout, enumeration  

Execute **Sprint A → B → C → D** in order; do not expand lawyer multipart until A/B are green.

---

## Related docs

- `docs/Reviews/auth_module_architectural_review_v2.md` — full V2 findings  
- `docs/Reviews/auth_module_authorization_report.md` — authorization per endpoint (companion report)  
- `docs/Plans/06_API_Auth_Users.md` — contract  

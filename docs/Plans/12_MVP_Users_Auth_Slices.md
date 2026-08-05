# MVP implementation plan: user profiles and account recovery

**Plan date:** 2026-07-18

**Slices:** Users/Clients, Users/Lawyers, ChangePassword, ResetPassword, ForgotPassword, ResendVerification, ConfirmEmail, and ConfirmEmailChange

**Objective:** make these slices safe and complete enough for a small, single-instance MVP without implementing the larger production-hardening backlog.

## 1. MVP decisions

These decisions intentionally keep the implementation small.

1. The MVP runs as one API instance. In-memory rate-limit partitions are acceptable for now.
2. Email change is **not an MVP feature**. Email remains visible in profile responses but is removed from both profile update requests. `ConfirmEmailChange` is removed/disabled.
3. Access JWT lifetime is reduced from 60 minutes to 15 minutes.
4. Immediate password-related JWT invalidation uses the existing Identity `SecurityStamp`; no new session-version column is introduced.
5. Every authenticated request validates current account state and SecurityStamp. This database lookup is acceptable at MVP traffic levels.
6. Clients may use authenticated product endpoints only in `Active` state. Lawyers may use their private profile while `PendingReview` or `Active`. `Suspended`, `Rejected`, and `Deleted` users are denied.
7. Public lawyer profiles are visible only for `Active`, email-confirmed lawyers with a real lawyer profile. `Active` is treated as professional approval/publication for MVP.
8. Profile deletion is soft deletion. Automatic anonymization/purge is deferred, but deletion immediately blocks access, revokes sessions, and hides the lawyer publicly.
9. Existing Hangfire email delivery is retained. SMTP failures must throw so Hangfire retries; no transactional outbox is added yet.
10. Initial email confirmation may remain a state-changing `GET` for MVP, provided malformed input is safe and the operation is idempotent. Frontend landing page plus explicit POST is M1.

## 2. Explicitly out of scope

Do not add these during the MVP implementation unless requirements change:

- Email-change pending tables, dual-address approval, or email-change UI.
- Redis/distributed rate limiting, WAF rules, adaptive risk scoring, or CAPTCHA.
- Transactional email outbox/dead-letter administration UI.
- New session-version database fields; use Identity SecurityStamp.
- Full account anonymization/purge automation.
- Device/session management UI.
- MFA implementation.
- Broad refactoring of unrelated auth, verification, or storage modules.
- Password-policy migration beyond preserving the existing Identity policy.
- Rewriting the project around commands/handlers or another architecture.

## 3. Delivery sequence

Implement in the order below. Each phase should remain buildable and independently reviewable.

### Phase 1 — shared security foundation

#### 1.1 Stop production exception leakage

**Files**

- `SmartCourt/Middleware/ExceptionHandlingMiddleware.cs`

**Changes**

- Replace the default `exception.ToString()` response with a generic internal-error message.
- Keep the exception and a generated/request trace identifier in server logs.
- Return the same `ApiResponse` shape used by other errors.
- Never include stack trace, SQL/provider details, tokens, or credentials in a production response.

**Acceptance criteria**

- An unexpected exception returns HTTP 500 with a generic message and trace/correlation ID.
- Full exception detail remains available in protected logs.
- Existing known business/validation/authentication exception mappings continue working.

#### 1.2 Validate account state and SecurityStamp on JWT requests

**Files**

- `SmartCourt/Providers/Jwt/JwtProvider.cs`
- `SmartCourt/DependencyInjection.cs`
- `SmartCourt/Features/Auth/RefreshToken/RefreshTokenService.cs`
- New small shared constants/helper under `SmartCourt/Features/Auth/Shared/` if needed

**Changes**

- Add the user’s Identity SecurityStamp as a JWT claim when issuing an access token.
- In `JwtBearerEvents.OnTokenValidated`, parse the subject, load the current user, and reject when:
  - user does not exist;
  - email is not confirmed;
  - status is `Suspended`, `Rejected`, or `Deleted`;
  - JWT SecurityStamp is absent or differs from the current Identity value.
- Permit only `Active` or `PendingReview` through the shared JWT check.
- Keep role authorization on controllers.
- In `RefreshTokenService`, independently reject users that are not email-confirmed or not `Active`/`PendingReview`, even if the refresh token is otherwise valid.
- Check every `UserManager.UpdateAsync` result in refresh-token rotation/reuse handling.
- Set the configured access-token lifetime to 15 minutes.

**Why this is the minimum**

Password change/reset already updates Identity SecurityStamp. Comparing it on authenticated requests invalidates older access JWTs without adding a new session table/version. Current account lookup also makes suspension/deletion effective immediately.

**Acceptance criteria**

- A token issued before password change/reset is rejected on its next request.
- `Suspended`, `Rejected`, and `Deleted` users cannot use access or refresh tokens.
- `PendingReview` lawyers can access their private profile.
- An `Active` client/lawyer continues to work normally.
- A JWT with a missing/incorrect SecurityStamp is rejected.

#### 1.3 Correct middleware order

**Files**

- `SmartCourt/Program.cs`

**Changes**

- Use the order: exception handling, authentication, rate limiting, authorization, controllers.
- If the production deployment is behind a reverse proxy, add forwarded headers before authentication/rate limiting and restrict known proxy/network values. If there is no proxy in the MVP deployment, explicitly document that assumption.
- Do not expose the Hangfire dashboard publicly; restrict it to development or require authenticated administrator access.

**Acceptance criteria**

- User-partitioned limiters can read the authenticated user ID.
- Hangfire dashboard is not anonymous in production.
- The IP partition represents the client for the documented deployment topology.

### Phase 2 — minimum rate limiting

#### 2.1 Replace global shared named buckets

**Files**

- `SmartCourt/DependencyInjection.cs`
- Reviewed controllers under `SmartCourt/Features/Auth/` and `SmartCourt/Features/Users/`
- New reusable rate-limit component under `SmartCourt/Common/RateLimiting/`

**Implementation shape**

- Use ASP.NET Core partitioned policies for IP/user limits available from `HttpContext`.
- Add a small in-memory account-key limiter invoked after validated MVC model binding for email/user/token-key limits.
- Derive account keys using HMAC-SHA256 of `NormalizeEmail(email)` with a required server secret. Never store/log raw email as the limiter key.
- Derive token/request keys from a one-way HMAC/hash; never store/log raw tokens.
- Use independent buckets. A request succeeds only if every applicable bucket allows it.
- Set `QueueLimit = 0` for security operations.
- Return HTTP 429 using `ApiResponse.Fail(...)`; optionally add a coarse `Retry-After` without identifying the bucket that rejected the request.

**MVP limits**

| Endpoint | Minimum independent limits |
|---|---|
| `GET /api/clients/profile` | 120/min/user + 300/min/IP |
| `PUT /api/clients/profile` | 20/15 min/user + 60/15 min/IP |
| `DELETE /api/clients/profile` | 3/day/user + 10/day/IP |
| `GET /api/lawyers/profile` | 120/min/user + 300/min/IP |
| `PUT /api/lawyers/profile` | 20/15 min/user + 60/15 min/IP |
| `DELETE /api/lawyers/profile` | 3/day/user + 10/day/IP |
| `GET /api/lawyers/public/{id}` | 120/min/IP |
| `POST /api/auth/change-password` | 5/15 min/user + 20/15 min/IP |
| `POST /api/auth/forgot-password` | 5/15 min/IP + 3/hour/account key |
| `POST /api/auth/resend-verification` | 5/15 min/IP + 1/min/account key + 3/hour/account key |
| `POST /api/auth/reset-password` | 10/15 min/IP + 5/hour/account key + 5/hour/token key |
| `GET /api/auth/confirm-email` | 20/15 min/IP + 5/hour/user ID key |

`ConfirmEmailChange` gets no policy because it will not be exposed in the MVP.

**Acceptance criteria**

- One email/account cannot consume another account’s allowance.
- One IP cannot spray unlimited different accounts.
- One caller cannot exhaust a single global bucket for all users.
- Invalid/nonexistent email values consume the same account-key allowance as real accounts.
- 429 responses use the standard JSON envelope and reveal no PII or partition details.

### Phase 3 — client and lawyer profiles

#### 3.1 Remove email mutation from ordinary profile updates

**Files**

- `SmartCourt/Features/Users/Clients/DTOs/UpdateClientProfileRequest.cs`
- `SmartCourt/Features/Users/Clients/Validators/UpdateClientProfileRequestValidator.cs`
- `SmartCourt/Features/Users/Clients/ClientService.cs`
- `SmartCourt/Features/Users/Lawyers/DTOs/UpdateLawyerProfileRequest.cs`
- `SmartCourt/Features/Users/Lawyers/Validators/UpdateLawyerProfileRequestValidator.cs`
- `SmartCourt/Features/Users/Lawyers/LawyerService.cs`

**Changes**

- Remove `Email` from both update DTOs and their validators.
- Delete the `SendChangeEmailConfirmationAsync` branches from both services.
- Keep email in read response DTOs as read-only data.
- Remove the now-unused auth-helper dependency from profile services if no longer needed except for deletion/session revocation.

**Acceptance criteria**

- Profile update cannot request or cause an email change.
- Existing phone/DOB/address and lawyer professional fields remain updateable.
- API contract clearly treats email as read-only for MVP.

#### 3.2 Validate client updates minimally

**Files**

- `SmartCourt/Features/Users/Clients/Validators/UpdateClientProfileRequestValidator.cs`
- `SmartCourt/Features/Users/Clients/ClientService.cs`

**Changes**

- Retain Egyptian phone validation and address maximum.
- Retain past-date validation for DOB.
- Do not introduce a minimum-age rule until product/legal requirements define it.
- Check every Identity update result.

**Acceptance criteria**

- Future/empty DOB, malformed phone, and oversized address are rejected as validation errors.
- Valid owner update succeeds; another user ID can never be supplied.

#### 3.3 Fix lawyer validation and public visibility

**Files**

- `SmartCourt/Features/Users/Lawyers/Validators/UpdateLawyerProfileRequestValidator.cs`
- `SmartCourt/Features/Users/Lawyers/LawyerService.cs`

**Changes**

- Add `IsInEnum()` for `LawyerLevel`.
- Require DOB to be in the past.
- Make experience range match its message: 0 through 50 inclusive.
- Before update, require the selected `LegalSpecialization` to exist and have `IsDeleted == false`; return a validation/business error instead of allowing a foreign-key failure.
- Use `ApplicationUser.Address` as the MVP source of truth. Do not write `LawyerProfile.Address`; document/remove the duplicate later with a dedicated migration.
- Change public lookup to require all of:
  - target user ID;
  - `LawyerProfile != null`;
  - `EmailConfirmed == true`;
  - `Status == Active`.
- Return 404 for every other state.
- Do not return internal status from `PublicLawyerProfileResponse`; it is always Active by definition and is unnecessary disclosure.

**Acceptance criteria**

- A client ID, pending/rejected/suspended/deleted lawyer, unconfirmed lawyer, or user without lawyer profile returns 404.
- An active confirmed lawyer returns the public DTO.
- Invalid level/specialization/DOB/experience produces a controlled 400, never a database 500.

#### 3.4 Require password confirmation for profile deletion

**Files**

- `SmartCourt/Features/Users/Clients/ClientsController.cs`
- `SmartCourt/Features/Users/Clients/IClientService.cs`
- `SmartCourt/Features/Users/Clients/ClientService.cs`
- `SmartCourt/Features/Users/Lawyers/LawyersController.cs`
- `SmartCourt/Features/Users/Lawyers/ILawyerService.cs`
- `SmartCourt/Features/Users/Lawyers/LawyerService.cs`
- New shared/simple `DeleteAccountRequest` DTO and validator

**Changes**

- Require `CurrentPassword` in the deletion request body.
- Verify it using Identity before mutation.
- In one database transaction:
  - set status to `Deleted`;
  - set lawyer `IsAvailable = false` when applicable;
  - revoke all active refresh tokens;
  - update the Identity SecurityStamp;
  - check every Identity result;
  - commit.
- If the account is already deleted, return a safe idempotent success or the same generic unavailable result—choose one response and test it.
- Do not add anonymization/purge logic in this phase.

**Acceptance criteria**

- Wrong password causes no state or token change.
- Successful deletion immediately invalidates access/refresh tokens and removes lawyer public visibility.
- A partial database failure rolls back status/token/security-stamp changes.

### Phase 4 — password change and reset

#### 4.1 Complete ChangePassword

**Files**

- `SmartCourt/Features/Auth/ChangePassword/ChangePasswordService.cs`
- Existing DTO/validator/controller

**Changes**

- Reject new password equal to current password.
- Load user and refresh tokens as currently done.
- Wrap `ChangePasswordAsync`, refresh-token revocation, and final update in an EF transaction.
- Check the result of the refresh-token update; rollback on failure.
- After success, SecurityStamp validation invalidates old access tokens and refresh tokens are revoked, so require the client to log in again.
- Apply the Phase 2 user/IP limiter.

**Acceptance criteria**

- Wrong current password, same password, or invalid new password changes nothing.
- Success invalidates the current access token and all refresh tokens.
- Simulated failure after password mutation rolls back the full operation.

#### 4.2 Complete ResetPassword

**Files**

- `SmartCourt/Features/Auth/ResetPassword/ResetPasswordService.cs`
- Existing DTO/validator/controller

**Changes**

- Use `FindByEmailAsync`/normalized Identity lookup instead of direct case-sensitive `u.Email == email`.
- Return one generic “invalid or expired reset request” response for nonexistent user, malformed token, invalid token, expired token, disallowed status, and replay.
- Allow reset only for email-confirmed `Active` or `PendingReview` accounts.
- Bound token input length and safely catch Base64Url decoding failures.
- Wrap password reset and refresh-token revocation/update in one transaction and inspect every result.
- Do not issue tokens or automatically log in after reset.
- Apply IP/account/token limits.

**Acceptance criteria**

- All invalid account/token combinations have the same status and response shape.
- A valid token is single-use.
- Success invalidates all existing access and refresh sessions.
- Deleted/suspended/rejected users cannot reset through this endpoint.

### Phase 5 — forgot, resend, and confirmation

#### 5.1 Fix public URL and email configuration once

**Files**

- `SmartCourt/Features/Auth/Shared/AuthHelperService.cs`
- `SmartCourt/Features/Auth/ForgotPassword/ForgotPasswordService.cs`
- `SmartCourt/Providers/Email/MailKitOptions.cs`
- `SmartCourt/Providers/Email/SmtpEmailSender.cs`
- `SmartCourt/Providers/Email/BackgroundEmailProvider.cs`
- `SmartCourt/DependencyInjection.cs`
- Environment settings/secrets documentation

**Changes**

- Add strongly typed public URL options with a required HTTPS base URL outside Development.
- Remove production fallback to `http://localhost:5000`.
- Build query strings using `QueryHelpers.AddQueryString`; never interpolate email/token values directly.
- HTML-encode `FullName` and other user-controlled template substitutions.
- Validate SMTP/public-URL configuration at startup.
- Change the SMTP job contract to throw on failed delivery rather than returning `false`; allow Hangfire retries.
- Keep enqueue success semantics explicit: API acceptance means queued, not delivered.

**Acceptance criteria**

- Addresses containing `+` and other valid reserved characters produce usable links.
- Production cannot start with missing/HTTP public URL or incomplete SMTP settings.
- A transient SMTP failure causes a failed/retried Hangfire job, not a successful job.
- Logs do not contain raw reset/confirmation tokens.

#### 5.2 Complete ForgotPassword

**Files**

- `SmartCourt/Features/Auth/ForgotPassword/ForgotPasswordService.cs`
- Existing controller/validator

**Changes**

- Keep the same generic 200 response for all inputs.
- Use normalized Identity lookup.
- Send only for email-confirmed `Active` or `PendingReview` accounts.
- Apply independent IP/account limits before expensive email work.
- Avoid logging whether an account exists.
- Do not add artificial sleeps. Queueing already removes SMTP latency; precise timing equalization is M1.

**Acceptance criteria**

- Missing, unconfirmed, deleted, suspended, and valid accounts receive the same HTTP response body/status.
- Only an eligible account queues an email.
- Repeated requests are bounded independently by account and IP.

#### 5.3 Complete ResendVerification

**Files**

- `SmartCourt/Features/Auth/ResendVerification/ResendVerificationService.cs`
- Existing controller/validator

**Changes**

- Keep generic 200 response for every input.
- Send only when account exists, email is unconfirmed, and status is `Unverified`.
- Apply IP limit plus one-minute/account cooldown and hourly/account limit.
- Use the shared safe URL/email implementation.

**Acceptance criteria**

- Existing confirmed and nonexistent emails cannot be distinguished by response.
- Suspended/rejected/deleted accounts never queue verification email.
- A valid unverified account cannot enqueue more than the configured cooldown/hourly allowance.

#### 5.4 Complete ConfirmEmail and disable ConfirmEmailChange

**Files**

- `SmartCourt/Features/Auth/ConfirmEmail/ConfirmEmailController.cs`
- `SmartCourt/Features/Auth/ConfirmEmail/ConfirmEmailService.cs`
- `SmartCourt/Features/Auth/ConfirmEmail/IConfirmEmailService.cs`
- New confirmation request DTO/validator if useful
- `SmartCourt/Features/Auth/Shared/AuthHelperService.cs`

**Changes**

- Bound `userId` and token inputs; use `Guid.TryParse` and safe Base64Url decoding.
- Use one generic invalid/expired response for malformed user ID/token, missing user, invalid token, expired token, and disallowed state.
- Permit confirmation only from `Unverified`.
- After successful Identity confirmation:
  - Client role -> `Active`;
  - Lawyer role -> `PendingReview`.
- Persist email confirmation and status transition in one transaction and inspect all results.
- If the user is already confirmed in the correct post-confirmation state, return idempotent success without changing anything.
- Apply IP/user-ID-key limits.
- Remove the `/api/auth/confirm-email-change` route and `ConfirmEmailChangeAsync` from the MVP interface/service.
- Remove `SendChangeEmailConfirmationAsync` if it has no remaining call sites.

**Acceptance criteria**

- Malformed and invalid inputs never produce an unhandled 500.
- Client and lawyer receive the correct post-confirmation state.
- Deleted/suspended/rejected users are never reactivated.
- A successful link replay is safe/idempotent; an invalid link receives the generic invalid result.
- No email-change endpoint is exposed in route discovery for MVP.

### Phase 6 — schema and verification gate

#### 6.1 Reconcile migrations

**Files**

- `SmartCourt/Migrations/` as generated by EF tooling
- Model configuration/entity files only if required to remove unintended drift

**Changes**

- Resolve `dotnet ef migrations has-pending-model-changes`.
- Do not hand-edit generated migration designer/snapshot content.
- Apply migrations to a fresh test database and verify startup migration behavior.
- Do not include unrelated schema redesign.

**Acceptance criteria**

- `dotnet ef migrations has-pending-model-changes` reports no pending changes.
- Fresh database migration succeeds.
- Existing development database migration succeeds without data loss outside the intended changes.

#### 6.2 Add the minimum automated test suite

**Files**

- New tests under matching folders in `SmartCourt.Tests/Features/`
- Minimal test infrastructure/fixtures required for Identity + EF integration tests

**Required tests**

1. **JWT/account state**
   - Active succeeds.
   - Suspended/rejected/deleted fails.
   - Old SecurityStamp fails after password reset/change.
   - Disallowed user cannot refresh.
2. **Clients**
   - Owner GET/PUT succeeds.
   - Email cannot be changed by PUT.
   - Wrong deletion password leaves account active.
   - Successful deletion revokes sessions.
3. **Lawyers**
   - Public active confirmed lawyer succeeds.
   - Client, pending, unconfirmed, suspended, deleted, and missing profile all return 404.
   - Invalid enum, specialization, DOB, and 51 years fail with 400; 50 succeeds.
4. **Change/reset password**
   - Wrong/same password changes nothing.
   - Successful change/reset invalidates old access and refresh tokens.
   - Malformed, expired, replayed, missing-account reset cases share the same safe result.
5. **Forgot/resend/confirm**
   - Forgot/resend responses are identical for existing and nonexistent accounts.
   - Only eligible statuses enqueue email.
   - Malformed confirmation never returns 500.
   - Confirmation transitions Client to Active and Lawyer to PendingReview.
6. **Rate limiting**
   - Account A cannot exhaust account B’s bucket.
   - One IP is constrained across different account keys.
   - One caller cannot create an application-global lockout.
   - 429 uses the normal response envelope.
7. **Email job**
   - Transient SMTP failure throws and is retryable.
   - URL encoding handles an address containing `+`.

**Release commands**

```powershell
dotnet build SmartCourt.sln --no-restore
dotnet test SmartCourt.sln --no-build
dotnet ef migrations has-pending-model-changes --project SmartCourt --startup-project SmartCourt
```

All commands must succeed. Warnings already present outside the slice should be recorded, but no new warning attributable to this work is acceptable.

## 4. Endpoint definition of done

| Endpoint | MVP definition of done |
|---|---|
| `GET /api/clients/profile` | Client role; current Active account/SecurityStamp verified; owner only; rate-limited. |
| `PUT /api/clients/profile` | Same authorization; no email mutation; validated phone/DOB/address; checked update; rate-limited. |
| `DELETE /api/clients/profile` | Current password required; atomic deleted status + SecurityStamp + refresh revocation; rate-limited. |
| `GET /api/lawyers/profile` | Lawyer role; PendingReview/Active account allowed; owner only; rate-limited. |
| `PUT /api/lawyers/profile` | Same authorization; no email mutation; enum/reference/DOB/experience validated; checked update. |
| `DELETE /api/lawyers/profile` | Current password required; atomic deletion/session revocation; availability false; immediately non-public. |
| `GET /api/lawyers/public/{id}` | Anonymous but IP-limited; only Active + confirmed + real lawyer profile; all other states 404. |
| `POST /api/auth/change-password` | Valid current password; transaction; old access/refresh sessions invalidated; user/IP limits. |
| `POST /api/auth/forgot-password` | Generic 200; normalized lookup; eligible states only; safe HTTPS URL; retryable delivery; IP/account limits. |
| `POST /api/auth/resend-verification` | Generic 200; Unverified only; safe delivery; cooldown + IP/account limits. |
| `POST /api/auth/reset-password` | Generic invalid result; safe token decode; eligible states; atomic reset/session revocation; IP/account/token limits. |
| `GET /api/auth/confirm-email` | Safe bounded input; generic invalid result; idempotent; correct Client/Lawyer status transition; limited. |
| `/api/auth/confirm-email-change` | Not exposed in the MVP. |

## 5. Suggested pull-request breakdown

Keep the work reviewable with the following six changesets:

1. **Shared auth safety:** exception response, JWT SecurityStamp/status validation, refresh-state checks, 15-minute access tokens, pipeline/dashboard protection.
2. **MVP rate limiting:** reusable IP/user/account/token partitions and endpoint policies.
3. **Profile safety:** remove email mutation, public lawyer predicate, validators/reference checks, deletion password/session transaction.
4. **Password flows:** transactional ChangePassword and ResetPassword with generic reset errors.
5. **Email flows:** typed URL configuration, encoded links, SMTP retry behavior, Forgot/Resend rules, ConfirmEmail transition, disable ConfirmEmailChange.
6. **Release gate:** EF model reconciliation, integration/security tests, clean build/test/migration verification.

If a changeset reveals an unrelated defect, record it separately rather than expanding the MVP scope unless it directly prevents one of the acceptance criteria above.

## 6. Final MVP release checklist

- [ ] Email cannot be changed through client/lawyer profile update.
- [ ] ConfirmEmailChange route is absent.
- [ ] Deleted/suspended/rejected accounts cannot use access or refresh tokens.
- [ ] Password change/reset invalidates old JWTs through SecurityStamp and revokes refresh tokens.
- [ ] Public lawyer endpoint cannot return a client or non-public lawyer.
- [ ] All security rate limits are partitioned; no global three-request bucket remains.
- [ ] Production responses contain no stack traces.
- [ ] Recovery/confirmation links use configured HTTPS URLs and safe query encoding.
- [ ] SMTP failures are retryable Hangfire failures.
- [ ] ConfirmEmail safely handles malformed/replayed input and sets correct role status.
- [ ] Lawyer enum/reference/date/experience validation is enforced.
- [ ] EF reports no pending model changes and a fresh migration succeeds.
- [ ] Minimum security/integration tests pass.
- [ ] Build and tests pass with no new slice-related warnings.

Completion of this checklist is sufficient for these slices in the defined single-instance MVP. The M1/M2 items in the focused review remain improvements, not hidden release blockers.

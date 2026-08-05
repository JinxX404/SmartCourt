# Minimum implementation plan to close user/auth slices for MVP

**Date:** 2026-07-18

**Scope:** Users/Clients, Users/Lawyers, ChangePassword, ResetPassword, ForgotPassword, ResendVerification, ConfirmEmail, and ConfirmEmailChange.

## Outcome

Complete the exposed MVP behavior without building the larger M1/M2 security architecture. Every task in this document is mandatory. A feature that is not implemented safely is disabled rather than partially exposed.

The implementation is split into four pull requests/batches. These are work packages, not four product versions.

## Fixed MVP decisions

- Single API instance for MVP.
- Email change is excluded: email is read-only in profiles and `ConfirmEmailChange` is not exposed.
- Account deletion remains supported and requires the current password.
- JWTs use Identity SecurityStamp validation and a 15-minute lifetime; no new session-version table.
- Hangfire remains the email queue; SMTP failures throw for built-in retries; no outbox.
- Initial email confirmation remains GET for MVP, with safe input handling and idempotency.
- In-memory partitioned rate limiting is sufficient for the single instance.
- No MFA, CAPTCHA, Redis, dual-email confirmation, automated PII purge, or advanced risk engine.

## Batch 1 — shared authorization and security controls

### 1. Add live account and SecurityStamp validation

**Change**

- Add a SecurityStamp claim in `SmartCourt/Providers/Jwt/JwtProvider.cs`.
- In JWT `OnTokenValidated` in `SmartCourt/DependencyInjection.cs`, load the current user and reject the token when:
  - the user is missing;
  - email is unconfirmed;
  - status is not `Active` or `PendingReview`;
  - the SecurityStamp claim is missing or differs from the database value.
- Set access-token expiry to 15 minutes in deployment configuration.
- In `SmartCourt/Features/Auth/RefreshToken/RefreshTokenService.cs`, apply the same email/status eligibility before rotating a refresh token and check every Identity update result.

**Result**

- Suspended, rejected, and deleted users immediately lose API and refresh access.
- Password change/reset invalidates existing JWTs through Identity SecurityStamp.
- Pending-review lawyers retain access to their private profile.

### 2. Stop exception disclosure

In `SmartCourt/Middleware/ExceptionHandlingMiddleware.cs`:

- Log the full exception with `HttpContext.TraceIdentifier`.
- Return a generic HTTP 500 `ApiResponse`; do not return `exception.ToString()`.
- Preserve the existing controlled mappings for validation, authentication, forbidden, not-found, and business exceptions.

### 3. Correct the request pipeline and operational exposure

In `SmartCourt/Program.cs`:

- Order middleware as exception handling -> authentication -> rate limiting -> authorization -> controllers.
- Do not expose the Hangfire dashboard anonymously in production. Disable it outside Development for MVP unless admin authorization already exists.
- Document whether the MVP is directly hosted or behind a trusted reverse proxy. If proxied, configure forwarded headers only for the known proxy before IP limiting.

### 4. Replace the shared global rate buckets

Implement reusable in-memory partitioned policies in `SmartCourt/DependencyInjection.cs` plus one small account-key limiter component.

- User key: authenticated user ID.
- IP key: resolved client IP.
- Account key: SHA-256/HMAC of normalized email, never logged.
- Token key: SHA-256/HMAC of the submitted encoded token, never logged.
- No queued security requests (`QueueLimit = 0`).
- All 429 responses use the normal JSON `ApiResponse` and do not reveal which bucket fired.

**Minimum limits**

| Endpoint group | Limits |
|---|---|
| Private profile GET | 120/min/user + 300/min/IP |
| Private profile PUT | 20/15 min/user + 60/15 min/IP |
| Private profile DELETE | 3/day/user + 10/day/IP |
| Public lawyer GET | 120/min/IP |
| ChangePassword | 5/15 min/user + 20/15 min/IP |
| ForgotPassword | 5/15 min/IP + 3/hour/account |
| ResendVerification | 5/15 min/IP + 1/min/account + 3/hour/account |
| ResetPassword | 10/15 min/IP + 5/hour/account + 5/hour/token |
| ConfirmEmail | 20/15 min/IP + 5/hour/user ID |

**Batch 1 acceptance gate**

- Old JWT fails after password SecurityStamp changes.
- Suspended/rejected/deleted access and refresh requests fail.
- Active users and pending-review lawyers work normally.
- One caller cannot exhaust a global limiter for every user.
- Unexpected exceptions contain no stack trace in the response.
- Hangfire dashboard is not anonymous in production.

## Batch 2 — close Client and Lawyer profiles

### 1. Make email read-only

Remove `Email` from:

- `SmartCourt/Features/Users/Clients/DTOs/UpdateClientProfileRequest.cs`
- `SmartCourt/Features/Users/Clients/Validators/UpdateClientProfileRequestValidator.cs`
- `SmartCourt/Features/Users/Lawyers/DTOs/UpdateLawyerProfileRequest.cs`
- `SmartCourt/Features/Users/Lawyers/Validators/UpdateLawyerProfileRequestValidator.cs`

Remove email-change branches from:

- `SmartCourt/Features/Users/Clients/ClientService.cs`
- `SmartCourt/Features/Users/Lawyers/LawyerService.cs`

Email remains in profile response DTOs. Sending `email` in an update must not change the account email; configure JSON handling consistently according to the project’s contract policy (reject unknown fields if enabled, otherwise ignore it).

### 2. Correct the public lawyer query

In `LawyerService.GetPublicProfileAsync`, query only a user satisfying all conditions:

- requested ID matches;
- `LawyerProfile` exists;
- `EmailConfirmed` is true;
- `Status == Active`.

Return 404 for all other states. Remove internal `Status` from `PublicLawyerProfileResponse` because a returned public profile is Active by definition.

### 3. Complete lawyer validation

In `UpdateLawyerProfileRequestValidator` and `LawyerService`:

- Validate `Level` with `IsInEnum()`.
- Require DOB in the past.
- Accept experience from 0 through 50 inclusive.
- Check that `SpecializationId` exists and `IsDeleted == false` before assigning it.
- Keep `ApplicationUser.Address` as the MVP address source; do not write the duplicate lawyer-profile address field.
- Map invalid inputs to controlled 400 responses, never a database 500.

For clients, retain the existing past DOB, phone, and address validation; do not add an undefined minimum-age rule.

### 4. Secure account deletion

Add a small `DeleteAccountRequest` containing `CurrentPassword` and use it for both Client and Lawyer deletion.

For each deletion service:

1. Load current user with refresh tokens and profile.
2. Verify the current password; stop without mutation on failure.
3. Begin an EF transaction.
4. Set status to `Deleted`.
5. Set `LawyerProfile.IsAvailable = false` for lawyers.
6. Revoke all active refresh tokens.
7. Update Identity SecurityStamp.
8. Check every Identity result and commit; rollback on any failure.

Repeated deletion must be safe and must not expose account information. No automatic PII purge is added in MVP.

### 5. Correct response construction

Use `ApiResponse.Ok(message)` for mutation success messages rather than placing a success message inside `ApiResponse<string>.Data`.

**Batch 2 acceptance gate**

- Client/lawyer PUT cannot initiate or complete email change.
- Client IDs and all non-public lawyer states return 404 from the public lawyer endpoint.
- Invalid lawyer enum/reference/DOB/experience returns 400.
- Wrong deletion password changes nothing.
- Successful deletion immediately invalidates access/refresh sessions and hides a lawyer publicly.

## Batch 3 — close password and recovery flows

### 1. ChangePassword

In `SmartCourt/Features/Auth/ChangePassword/ChangePasswordService.cs`:

- Retain current-password verification.
- Reject new password equal to current password.
- Run password change, refresh-token revocation, and final checked update inside one EF transaction.
- Roll back the transaction on any Identity/update failure.
- Do not issue replacement tokens; require login after success.
- Apply the Batch 1 user/IP policy.

### 2. ResetPassword

In `SmartCourt/Features/Auth/ResetPassword/ResetPasswordService.cs`:

- Use `FindByEmailAsync`/Identity-normalized lookup.
- Bound token length and safely decode Base64Url.
- Allow only email-confirmed `Active` or `PendingReview` accounts.
- Return the same generic invalid/expired response for nonexistent email, malformed/expired/invalid/replayed token, and disallowed account state.
- Run password reset, refresh-token revocation, and the checked update inside one EF transaction.
- Do not automatically sign in or issue tokens.
- Apply IP/account/token limits.

### 3. ForgotPassword

In `SmartCourt/Features/Auth/ForgotPassword/ForgotPasswordService.cs`:

- Preserve one generic HTTP 200 response for every submitted email.
- Use normalized Identity lookup.
- Queue email only for email-confirmed `Active` or `PendingReview` users.
- Apply IP/account limits before token/template work.
- Never log whether the account exists.

### 4. ResendVerification

In `SmartCourt/Features/Auth/ResendVerification/ResendVerificationService.cs`:

- Preserve one generic HTTP 200 response.
- Queue only when the user exists, email is unconfirmed, and status is exactly `Unverified`.
- Apply IP/account cooldown and hourly limits.
- Never send for suspended, rejected, or deleted users.

### 5. Make recovery/verification email usable and retryable

Update `AuthHelperService`, `ForgotPasswordService`, email options, and SMTP provider:

- Require a configured public HTTPS base URL outside Development; remove production localhost fallback.
- Build links with `QueryHelpers.AddQueryString`.
- HTML-encode names inserted into email templates.
- Validate public URL and SMTP settings at startup.
- Change SMTP delivery failure to throw so Hangfire retries the job.
- Do not log raw confirmation/reset URLs or tokens.

**Batch 3 acceptance gate**

- Successful password change/reset invalidates old access and refresh sessions.
- Failure at any step does not leave password and session state partially updated.
- Every invalid reset account/token case returns the same safe response.
- Forgot/resend responses are identical for existing and nonexistent accounts.
- Only eligible statuses enqueue messages.
- Addresses containing `+` produce correct links.
- SMTP transient failure produces a retryable failed job.

## Batch 4 — close confirmation and pass the release gate

### 1. Complete ConfirmEmail

Update `ConfirmEmailController`, `ConfirmEmailService`, and its interface:

- Validate bounded `userId` and token inputs.
- Use `Guid.TryParse` and safe Base64Url decoding.
- Return one generic invalid/expired result for malformed ID/token, missing user, invalid/expired token, and disallowed status.
- Allow first confirmation only from `Unverified`.
- Run Identity confirmation and status update in one EF transaction:
  - Client -> `Active`;
  - Lawyer -> `PendingReview`.
- Never reactivate suspended, rejected, or deleted users.
- Treat a correctly confirmed user already in the expected state as idempotent success.
- Apply IP/user-ID rate limits.

### 2. Disable ConfirmEmailChange

- Remove the `/api/auth/confirm-email-change` route.
- Remove `ConfirmEmailChangeAsync` from `IConfirmEmailService` and `ConfirmEmailService`.
- Remove `SendChangeEmailConfirmationAsync` from the auth helper after confirming it has no callers.
- Confirm route discovery/OpenAPI no longer exposes email change.

### 3. Reconcile EF migrations

- Resolve current pending model changes using normal EF migration generation.
- Verify migration from an empty database.
- Do not mix unrelated schema redesign into this migration.

### 4. Add only the release-blocking tests

Add integration/service tests for these cases:

1. Suspended/rejected/deleted JWT and refresh requests fail.
2. Old JWT fails after password change/reset.
3. Profile PUT cannot change email.
4. Public lawyer returns 404 for a client and non-Active lawyer; Active confirmed lawyer succeeds.
5. Lawyer invalid enum and specialization fail with 400.
6. Wrong deletion password changes nothing; successful deletion revokes sessions.
7. ChangePassword success revokes sessions; simulated post-change failure rolls back.
8. Reset malformed/nonexistent/expired/replayed cases share one response; success revokes sessions.
9. Forgot/resend existing and nonexistent cases share one response; only eligible state queues email.
10. Confirm malformed input never returns 500; Client becomes Active; Lawyer becomes PendingReview; replay is safe.
11. Rate limits isolate accounts and IPs; no application-global lockout; 429 uses `ApiResponse`.
12. SMTP failure throws and a `+` email address produces a usable encoded link.

### 5. Run the release commands

```powershell
dotnet build SmartCourt.sln --no-restore
dotnet test SmartCourt.sln --no-build
dotnet ef migrations has-pending-model-changes --project SmartCourt --startup-project SmartCourt
```

All must succeed, and EF must report no pending model changes.

**Batch 4 acceptance gate**

- ConfirmEmail handles all malformed and lifecycle states without 500 or reactivation.
- Client/Lawyer confirmation transitions are correct.
- ConfirmEmailChange is absent.
- Fresh database migration succeeds.
- All 12 security test groups pass.

## Per-slice closure definition

| Slice | Closed for MVP when |
|---|---|
| Users / Clients | Owner-only Active access works; email is read-only; validated update works; password-confirmed deletion revokes all sessions. |
| Users / Lawyers | PendingReview/Active private access works; public endpoint returns only Active confirmed lawyer profiles; validation is controlled; deletion removes access/public visibility. |
| Auth / ChangePassword | Current password is required; operation is atomic; all old access/refresh sessions fail afterward; endpoint is limited. |
| Auth / ResetPassword | Lookup/token failures are generic; status is enforced; reset/session revocation is atomic; token is single-use; endpoint is limited. |
| Auth / ForgotPassword | Response is generic; eligible accounts only; URL works; delivery retries; IP/account limits work. |
| Auth / ResendVerification | Response is generic; Unverified only; delivery retries; cooldown and IP/account limits work. |
| Auth / ConfirmEmail | Input is safe; generic invalid result; idempotent success; Client -> Active and Lawyer -> PendingReview; endpoint is limited. |
| Auth / ConfirmEmailChange | Route and implementation are not exposed in MVP. |

## Final mandatory checklist

- [ ] Live status and SecurityStamp are validated for JWTs.
- [ ] Refresh flow rejects blocked account states.
- [ ] Email is removed from profile mutation.
- [ ] Public lawyer query publishes only Active confirmed lawyer profiles.
- [ ] Profile deletion verifies password and revokes access/refresh sessions.
- [ ] ChangePassword and ResetPassword are atomic and revoke sessions.
- [ ] Forgot, Resend, Reset, Confirm, ChangePassword, and public lawyer endpoints have partitioned limits.
- [ ] Confirmation safely transitions Client and Lawyer status.
- [ ] ConfirmEmailChange is disabled.
- [ ] Production errors contain no stack traces.
- [ ] Recovery links use configured HTTPS and correct encoding.
- [ ] SMTP failures retry through Hangfire.
- [ ] EF migrations are reproducible with no model drift.
- [ ] The 12 release-blocking test groups pass.

When this checklist passes, these slices are closed for the defined MVP. No M1/M2 item is required to declare them complete.

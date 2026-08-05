# Focused end-to-end review: user profiles and account recovery

**Review date:** 2026-07-18

**Scope:** Users/Clients, Users/Lawyers, ChangePassword, ResetPassword, ForgotPassword, ResendVerification, ConfirmEmail, and ConfirmEmailChange

**Primary criteria:** business logic, authorization, security, completeness, failure handling, and endpoint-specific rate limiting

## Executive verdict

The reviewed slices are structurally understandable and have some good foundations: private profile endpoints derive ownership from the authenticated user rather than accepting a user ID, role attributes separate client and lawyer operations, change-password verifies the current password, recovery endpoints use Identity tokens, and forgot/resend return generic success messages.

They are **not complete or production-ready end to end**, however. The most important blockers are:

1. A suspended or soft-deleted user with a still-valid access token can continue calling authorized endpoints. Account status and session validity are not enforced when a JWT is used.
2. Profile `PUT` treats an email change like an ordinary profile edit. It requires neither the current password, MFA, nor recent authentication, and it has no durable pending-change record or old-address notification.
3. Password change/reset and refresh-token revocation are separate persistence operations. The password can change while session revocation silently fails, and existing access tokens remain usable in every case.
4. `GET /api/lawyers/public/{id}` does not prove that the target is a lawyer, active, verified, non-deleted, or has a lawyer profile. A client ID can therefore be returned as a public “lawyer” profile.
5. The three configured rate-limit policies are single process-wide buckets, shared by all callers. They are not partitioned by IP, account, or user, so a few requests can deny service to everybody while distributed attackers remain poorly controlled.
6. Confirmation links mutate state through `GET`, do not safely handle malformed IDs/Base64, do not complete the account status transition, and are vulnerable to automated email-link prefetch.
7. Email enqueueing is reported as success even if later SMTP delivery fails, while the SMTP implementation swallows the failure. The recovery and verification flows therefore cannot reliably promise delivery or retry.
8. There are no automated tests for any reviewed slice. The six current tests cover unrelated verification helpers only.

The OpenAPI document was treated as historical context only. Conclusions below come from controllers, services, validators, entities, Identity configuration, middleware, email infrastructure, migrations, and executable build/test checks.

## MVP ranking: what is mandatory and what can wait

The reviewed code is **not generally corrupted** and does not require a rewrite. The controller/service structure and use of ASP.NET Identity are workable. The problem is that several sensitive lifecycle rules are incomplete. The MVP should fix the small set of issues that can cause account takeover, unauthorized access, data exposure, global denial of service, or a broken production flow. More sophisticated durability, auditing, and distributed controls can be delivered incrementally.

### M0 — mandatory before an MVP release

These are release blockers for any internet-accessible MVP.

| Slice | Minimum safe MVP requirement | Pragmatic implementation |
|---|---|---|
| All authenticated slices | Deny `Deleted` and `Suspended` users even when their JWT is still valid. | Add one centralized account-state authorization/JWT validation check. Do not duplicate it in every service. Keep access tokens short-lived. Full server-side access-token revocation can follow in M1 if the MVP reliably revokes refresh tokens. |
| Public lawyer profile | Return only real, publishable lawyers. | Require Lawyer role/discriminator, existing profile, confirmed email, approved verification/publication state, and `Active` status in one query. Return 404 for every other state. |
| Client/lawyer profile email field | Do not allow bearer-token-only email takeover. | **Simplest MVP:** remove/ignore `Email` in general profile `PUT` and do not offer email change yet. If the feature is mandatory, implement the dedicated M0 email-change flow described below. |
| Profile deletion | Prevent deletion through a stolen bearer token and terminate future sessions. | Require current password or recent authentication, set `DeletedAt`/status, revoke all refresh tokens, and make repeat deletion safe. Immediate PII anonymization/purge automation can wait if a documented retention rule exists. |
| ChangePassword | Current password must be checked, failures throttled, and sessions revoked reliably. | Keep the existing Identity call, inspect every result, revoke refresh tokens in the same transaction where feasible, and send the user back to login. A session-version system is M1 if access JWTs are short-lived. |
| ForgotPassword | Do not enumerate users, globally block all users, or send unusable links. | Keep one generic response, use normalized lookup, restrict eligible statuses, configure a real HTTPS base URL, URL-encode the link, and use independent IP/account throttles. Exact timing equalization can be M1, but avoid obviously expensive work only for valid users. |
| ResendVerification | Same minimum protections as ForgotPassword. | Generic response, eligible account states only, valid HTTPS link, URL encoding, and IP/account cooldowns. |
| ResetPassword | Invalid email/token combinations must not disclose account state; successful reset must terminate sessions. | One generic invalid/expired error, normalized lookup, status policy, per-IP/account/token throttling, checked refresh-token revocation, and no automatic login. |
| ConfirmEmail | Malformed public input must not cause 500s, and confirmation must complete account state. | Safely parse/decode input, return one generic invalid/expired result, and atomically transition Client to `Active` or Lawyer to `PendingReview`. Never reactivate deleted/suspended users. |
| ConfirmEmailChange | Do not ship the current implementation. | **Simplest MVP:** disable the route until email change is implemented safely. If required, use a dedicated request, current-password/recent-auth check, new-address confirmation, final uniqueness check, atomic email/username update, refresh-token revocation, and old-address notification. |
| Existing rate limiter | Replace the process-wide shared three-request bucket. | Partition public recovery policies by IP and HMAC-normalized account key. Partition authenticated policies by user plus IP. In-memory limiting is acceptable for a single-instance MVP; distributed Redis/gateway enforcement becomes mandatory only when multiple instances are deployed. |
| Email sending | SMTP failure must not be recorded as successful. | Make the Hangfire job throw on transient send failure so normal retries work, validate mail/base-URL configuration at startup, and alert on terminal failures. A transactional outbox can wait until M1 unless email is business-critical or multiple instances are introduced. |
| Error handling | Do not expose stack traces or database details. | Return a generic production 500 with correlation ID; keep exception details only in protected logs/Development. |
| Lawyer data validation/deployment | Reject invalid lawyer values and make schema deployment reproducible. | Add enum/reference/DOB validation, fix the 50-year boundary/address source, reconcile EF model drift, and verify migration from an empty database. |
| Tests | Protect the security boundaries most likely to regress. | Add a small release suite covering role/status authorization, public-lawyer filtering, malformed/replayed tokens, password/reset session revocation, generic forgot/reset responses, and independent rate-limit partitions. Full matrix/chaos testing is M1/M2. |

### M0 only if email change must be included in the MVP

Email change is optional as a product feature, but it cannot safely remain embedded in profile update. The smallest acceptable version is:

1. `POST /api/auth/email-change/request` from an active authenticated user.
2. Require current password or recent authentication; use MFA if the MVP already supports it.
3. Validate/normalize the new email and generate a short-lived Identity change token.
4. Email a correctly encoded HTTPS confirmation link to the new address and notify the old address that a change was requested.
5. Confirm with safe input handling and a final uniqueness check.
6. Update email and username atomically, revoke refresh tokens, and require login again.

A pending-email database table, dual confirmation, cancellation UI, and transactional outbox are recommended M1 improvements, not required to launch a small single-instance MVP if the minimal flow above is implemented carefully.

### M1 — next security/reliability version

These should be planned soon after MVP but do not have to block a controlled single-instance launch:

- Add a session/security version to JWT validation for immediate access-token invalidation after password/email/deletion/security events.
- Replace state-changing confirmation `GET` endpoints with a frontend landing page plus explicit API `POST`, reducing link-scanner/prefetch risk.
- Add durable pending records for email change, verification, and reset requests, including consumed/cancelled/expiry metadata.
- Add a transactional outbox and dead-letter operations for security email.
- Make sensitive Identity changes, session revocation, audit, and outbox writes fully atomic with failure-injection tests.
- Normalize forgot/resend response timing more rigorously.
- Add security notifications and structured audit records for every password, email, deletion, and recovery event.
- Configure purpose-specific token lifetimes and explicitly persist/protect Data Protection keys for the actual hosting environment.
- Add optimistic concurrency to profile updates and complete the broader endpoint test matrix.
- Move confirmation from raw query parameters toward an opaque one-time request ID to minimize PII/token leakage.

### M2 — scale and mature-production hardening

These are valuable but would be over-engineering for an early, single-instance MVP unless scale/compliance requirements already demand them:

- Distributed Redis/database rate-limit counters and gateway/WAF global controls.
- Advanced adaptive risk scoring, device intelligence, CAPTCHA escalation, and bot-management analytics.
- Dual-address approval for email change instead of new-address confirmation plus old-address notification.
- Automated retention/anonymization/purge workflows beyond a documented manual MVP process.
- Extensive chaos testing of SMTP, queue, database, and multi-node partial failures.
- Breached-password intelligence and revised password UX/policy rollout, although accepting long passwords and not truncating them should be supported early.
- Fine-grained dashboards, security-event correlation, anomaly detection, and operational SLOs.

### Slice-by-slice MVP disposition

| Slice | Current disposition | Can ship after |
|---|---|---|
| Users / Clients | **Needs targeted fixes; no rewrite** | Central status enforcement, safer deletion, email removed from ordinary update, minimal validation/tests. |
| Users / Lawyers | **Blocked in current form** | Public-profile authorization/filter corrected, validation/model drift fixed, status/deletion protections added, email removed from ordinary update. |
| Auth / ChangePassword | **Needs targeted fixes** | Reliable refresh-session revocation, status enforcement, rate limit, checked errors, and security test. |
| Auth / ForgotPassword | **Near MVP-ready** | Correct partitioned limit, HTTPS/encoded link, eligible-status rule, SMTP retry behavior, and generic behavior test. |
| Auth / ResendVerification | **Near MVP-ready** | Same as ForgotPassword plus per-account cooldown. |
| Auth / ResetPassword | **Needs targeted fixes** | Generic invalid result, normalized lookup/status rule, reliable session revocation, correct throttling, and replay tests. |
| Auth / ConfirmEmail | **Needs targeted fixes** | Safe malformed-input handling, generic invalid result, role-specific status transition, and replay/idempotency behavior. |
| Auth / ConfirmEmailChange | **Do not ship as implemented** | Disable for MVP, or replace it with the minimal dedicated email-change flow above. |

### Practical MVP conclusion

Do **not** implement every P1/P2 recommendation before launch. For a small single-instance MVP, M0 is the correct boundary. M1 should be scheduled as the first security/reliability iteration, and M2 should be driven by deployment scale, usage, and compliance needs.

The two genuinely dangerous current behaviors are the public-lawyer lookup and bearer-token-driven email change. The global rate-limit configuration is also a real availability defect, not optional polish. Most remaining work is targeted lifecycle completion around status, sessions, confirmation, and error handling—not an architectural rewrite.

## Review method and verification

The review traced each endpoint across controller, request DTO and validator, service, Identity operations, Entity Framework persistence, token/session handling, email generation/delivery, exception mapping, and rate-limiter/middleware configuration.

Verification results:

- `dotnet build SmartCourt.sln --no-restore`: succeeds, with 22 warnings outside most of this scope.
- `dotnet test SmartCourt.sln --no-build`: 6/6 pass, but none exercise these endpoints.
- `dotnet ef migrations has-pending-model-changes`: reports model drift. This makes a clean deployment of the current lawyer model, including the new lawyer level, non-reproducible until the model and migrations are reconciled.

## Severity-ranked findings

### P0 — release blockers

#### P0.1 Account state is not enforced for JWT-authenticated requests

Role authorization alone is insufficient. A JWT contains a role that remains trusted until expiry. Deleting a profile sets `UserStatus.Deleted` and revokes custom refresh tokens, but neither action immediately invalidates the current access token. The same problem applies to suspended accounts. As a result, an already authenticated deleted/suspended client or lawyer can continue reading or modifying the profile, changing the password, and reaching other role-authorized operations.

**Required correction**

- Add a request-time authorization/session check after JWT validation that loads or securely caches the user status and rejects any state not permitted for that endpoint.
- Include a server-controlled session/security version in access tokens and compare it during JWT validation. Increment it after password changes/resets, email changes, deletion, suspension, and an explicit “sign out everywhere.”
- Keep access tokens short-lived, but do not treat short expiry as immediate revocation.
- Define a state matrix rather than ad hoc checks. For example: `Unverified` may resend/confirm only; `PendingReview` may access a deliberately limited lawyer experience; `Active` may use normal APIs; `Suspended` and `Deleted` must be denied except for narrowly defined recovery/support flows.

#### P0.2 Email change is an unsafe ordinary profile update

Both client and lawyer profile updates accept `Email`. If it differs, the service generates a change token and emails the requested address. Possession of a bearer token is therefore enough to initiate takeover: there is no current-password check, MFA/recent-auth requirement, durable `PendingEmail`, expiry/cancellation record, or notification to the old address.

The email job is enqueued before all profile persistence is known to have succeeded. If a later database operation fails, the user can still receive and use a valid email-change link for an API operation that reported failure. Duplicate-email conflicts are generally deferred until confirmation, after the original profile call has reported success.

**Required correction**

- Remove email from general profile mutation, or ignore it there and expose an explicit `POST /api/auth/email-change/request`.
- Require current password for password-only accounts and a fresh MFA/recent-auth challenge where available.
- Normalize and preflight the new address, while still handling the final uniqueness race at confirmation.
- Store a hashed pending request containing user ID, normalized pending email, expiry, nonce/version, attempt counters, and consumed/cancelled timestamps.
- Commit the pending state first, then dispatch email through a durable outbox.
- Notify the old address immediately; send confirmation to the new address. Consider requiring approval from both addresses for high-risk accounts.
- Supersede older pending requests and make confirmation single-use and atomic.

OWASP explicitly treats email/password changes as sensitive actions requiring reauthentication and recommends notifying the old address while confirming the new one. See the [Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html) and [Email Address Validation and Verification Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Email_Validation_and_Verification_Cheat_Sheet.html).

#### P0.3 Password mutation and session revocation are not atomic

`ChangePasswordAsync`/`ResetPasswordAsync` changes the Identity password and security stamp first. The implementation then revokes custom refresh tokens through another `UserManager.UpdateAsync` call, but does not inspect that second result. A database/concurrency failure can leave the new password committed while refresh sessions remain valid. Existing access JWTs are never invalidated by the changed security stamp because JWT requests do not compare it.

**Required correction**

- Execute the password change/reset, session-version increment, refresh-token revocation, audit record, and outbox security notification in one database transaction wherever the chosen store supports it.
- Check and map every `IdentityResult`; never ignore the revocation update.
- Enforce the new session/security version during access-token validation.
- Decide whether the current session survives a change-password operation. A safe default is to revoke all sessions and issue a fresh access/refresh pair only after successful reauthentication.
- Add failure-injection tests proving rollback when any post-password step fails.

#### P0.4 Public lawyer lookup does not authorize publication

`GET /api/lawyers/public/{id}` is intentionally anonymous, but `LawyerService.GetPublicProfileAsync` filters essentially by user ID. It does not require the `Lawyer` role, a lawyer profile, an allowed user status, email confirmation, or any verification/publication state. Supplying a client user ID can expose that client through a response labeled as a lawyer. Deleted or unverified accounts may remain public.

**Required correction**

Query from the publishable lawyer profile set, with all conditions in the database predicate:

- user has the Lawyer role or an invariant lawyer account discriminator;
- `LawyerProfile` exists;
- account is `Active`;
- email is confirmed;
- professional verification/publication status is approved;
- record is not deleted and is opted into public discovery, if product policy supports opt-out.

Return `404` for every non-publishable state to avoid disclosing why a profile is absent. Do not expose internal workflow status values in the public DTO unless the product intentionally maps them to safe public labels.

#### P0.5 Current rate limiting is globally shared and easy to abuse

The named fixed-window policies for ForgotPassword, ResendVerification, and ResetPassword do not select a partition key. Each is therefore a single in-memory bucket for the entire application instance. Three requests by any caller can exhaust an hourly policy and deny service to all other users on that instance. Conversely, attackers can distribute attempts across instances or IPs without an account-scoped control.

`UseRateLimiter()` is also before `UseAuthentication()`, so authenticated user identity cannot reliably partition policies. No trusted forwarded-header configuration was found, so an IP policy behind a reverse proxy may see the proxy address or may become unsafe if arbitrary forwarded headers are trusted.

**Required correction**

- Use independent per-IP and per-account/per-user limiters; do not combine IP and account into one composite key. Independent buckets stop both broad spraying and concentrated attacks against one account.
- For unauthenticated email input, derive the account key as an HMAC of the normalized email using a server secret. Apply it even when the account does not exist, and never log the raw email as the partition key.
- Put trusted forwarded-header processing first, configured only for known proxies/networks; then authentication; then rate limiting for policies that use identity; then authorization.
- Use a distributed store or gateway/WAF controls for multi-instance enforcement. ASP.NET Core’s in-memory limiter alone cannot provide a cluster-wide quota.
- Return the normal `ApiResponse` envelope on `429`, with a generic message and a coarse `Retry-After`. Never expose whether the IP, account, token, or global bucket fired.
- Record security metrics without tokens, reset links, raw emails, or other PII.

Microsoft documents partitioning by authenticated identity or IP in the [ASP.NET Core rate-limiting guidance](https://learn.microsoft.com/is-is/aspnet/core/performance/rate-limit?view=aspnetcore-9.0). Proxy-derived addresses must be accepted only from configured proxies; see [Forwarded Headers Middleware guidance](https://learn.microsoft.com/mt-mt/aspnet/core/host-and-deploy/proxy-load-balancer?view=aspnetcore-8.0). OWASP recommends separate account and IP controls in its [Bot Management and Anti-Automation Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Bot_Management_and_Anti-Automation_Cheat_Sheet.html).

### P1 — high-priority correctness and security gaps

#### P1.1 Confirmation endpoints can produce 500 responses for malformed input

`ConfirmEmailController` accepts raw query strings without DTO validation. Malformed GUID/Base64 values are not consistently converted into a safe invalid-or-expired result. The global exception middleware returns `exception.ToString()` for unexpected errors, leaking stack traces and implementation details.

Use validated request models, `Guid.TryParse`, bounded token lengths, safe Base64Url decoding, and one generic invalid/expired response. Log a correlation ID internally and return no exception detail outside Development.

#### P1.2 Confirmation uses state-changing GET requests

Both email-confirmation routes use `GET`. Mail security scanners and link-preview systems can automatically follow links, causing confirmation or email change without an intentional user action.

Use a frontend landing page for the clickable `GET`; have the user explicitly submit a `POST` to the API. The server must still make the operation single-use, expiry-bound, and idempotent for an already consumed valid link.

#### P1.3 ConfirmEmail does not complete the domain lifecycle

Identity email confirmation succeeds, but the application status is not transitioned. The required transition must be explicit and atomic. A reasonable baseline is:

- confirmed Client: `Unverified -> Active`;
- confirmed Lawyer: `Unverified -> PendingReview`, not `Active`, until professional verification is approved.

Do not reactivate suspended/deleted accounts. Record the transition and emit a security/audit event.

#### P1.4 ConfirmEmailChange can leave Email and UserName inconsistent

`ChangeEmailAsync` persists the new email, then `SetUserNameAsync` runs as a separate operation. If the latter fails, email and username diverge. The flow also does not revoke sessions or refresh claims containing the old email.

Wrap email change, username normalization, pending-request consumption, session-version increment, refresh-token revocation, audit, and outbox notifications in one transaction. Microsoft confirms that `ChangeEmailAsync` changes the email after token validation; application-specific username/session work remains the application’s responsibility: [UserManager.ChangeEmailAsync](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.identity.usermanager-1.changeemailasync?view=aspnetcore-10.0).

#### P1.5 Recovery responses are textually generic but timing-distinguishable

ForgotPassword and ResendVerification return a good generic success body for missing/ineligible accounts, but those branches return quickly. A confirmed/eligible account performs token generation, template work, and job enqueueing. The latency difference can reveal account and verification state.

Move all delivery work off the request path and normalize the response-time envelope, or perform equivalent bounded work for every branch. The endpoint should always return the same status, body shape, and observably similar timing. OWASP’s [Forgot Password Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Forgot_Password_Cheat_Sheet.html) requires consistent messages and timing and recommends per-account abuse controls.

#### P1.6 ResetPassword leaks account/token distinctions and mishandles state

The reset service performs a direct email comparison rather than normalized Identity lookup. A missing email produces an “invalid data” business error, while an existing user with a bad token yields different Identity errors attached to `NewPassword`. This makes account/token states distinguishable and maps the error to the wrong field.

It also permits a reset attempt for deleted/suspended users unless Identity happens to reject it. Define the policy explicitly. Return a single generic invalid-or-expired response for every invalid account/token combination, while keeping detailed reason codes only in protected telemetry.

After a successful reset, revoke all sessions atomically and send a security notification. Do not automatically sign the user in. Do not lock an account solely because reset tokens are attacked; an attacker could use that as denial of service.

#### P1.7 Email delivery does not have reliable failure semantics

`BackgroundEmailProvider` returns `true` after enqueueing, not after delivery. `SmtpEmailSender` catches delivery exceptions and returns `false`; the background job therefore completes normally instead of throwing for Hangfire retry. Services often ignore the returned boolean. Verification and recovery mail can be permanently lost while the API reports success and the job system records success.

Use a durable outbox and make the job throw a retryable exception on transient SMTP failure. Persist attempt count, next attempt, terminal failure, and correlation ID. Treat provider acceptance as asynchronous acceptance only. Add dead-letter monitoring and an operational resend path.

#### P1.8 URL construction and deployment configuration are unsafe/incomplete

Links are built through string interpolation instead of a query builder. Valid addresses containing `+`, `&`, or other reserved characters can break. Reset/change links place email addresses and tokens in URLs, which may reach browser history, referrer headers, access logs, analytics, or support screenshots.

`AppUrl` is absent from the checked settings, and code can fall back to `http://localhost:5000`, while local launch settings use another port. This makes delivered links unreliable and risks insecure HTTP URLs outside local development.

Required baseline:

- Validate a required HTTPS `FrontendBaseUrl`/`PublicApiBaseUrl` at startup outside Development; no production localhost fallback.
- Use `QueryHelpers.AddQueryString` or an equivalent URI builder and Base64Url encoding.
- Prefer an opaque one-time request ID instead of putting email/PII in the link.
- Apply `Referrer-Policy: no-referrer` to confirmation/recovery pages and redact query strings from logs.
- HTML-encode names and all other user-controlled values inserted into email templates.

#### P1.9 Data-protection keys and token lifetimes need deployment design

Only basic data protection registration was found. In a multi-node deployment, all nodes must share a protected key ring and application name or tokens generated by one node may fail on another. Restarts/deployments also need deliberate key retention.

A single global one-hour token lifetime applies to multiple purposes. Configure purpose-specific providers: password reset and email change can reasonably start at 30–60 minutes, while initial email confirmation commonly needs a longer product-defined window such as 24 hours. All tokens must remain single-use through a consumed request/security-version transition.

#### P1.10 Sensitive mutations have no audit trail or security notifications

Password changed/reset, email-change requested/completed, account deletion, repeated recovery requests, and limiter/risk challenges should emit structured audit events. Password and email completion should notify the appropriate established address(es). Events must never include passwords, raw tokens, or full sensitive query strings.

#### P1.11 Password policy is weaker and less user-friendly than current guidance

The policy requires eight characters plus upper/lower/digit composition. For accounts without MFA, current OWASP guidance considers passwords shorter than 15 characters weak, recommends allowing at least 64 characters, and advises against mandatory composition rules. Add breached-password screening, allow password managers/paste, and keep a reasonable maximum that prevents resource abuse. This is a product migration decision, but it should not remain accidental.

### P2 — endpoint correctness and design debt

#### P2.1 Client profile flow

**What is good**

- Controller authorization is restricted to `Client`.
- The service derives the subject from authenticated claims rather than accepting an arbitrary target ID, preventing ordinary horizontal IDOR.
- The profile read is projected in a single query.
- A missing `ClientProfile` can be created lazily during update.

**Gaps**

- Status/session enforcement is absent.
- Email change has all P0/P1 defects described above.
- Deletion requires no current password, MFA, recent-auth, or confirmation.
- Deletion sets status and revokes refresh tokens but keeps all PII with no `DeletedAt`, reason, retention/anonymization policy, or purge job.
- A still-valid access token continues working after deletion.
- Date of birth is only checked to be in the past. Define and validate a minimum age and a plausible lower bound according to the product/legal policy.
- No optimistic concurrency/version protects simultaneous edits.
- Registration does not create a client profile; lazy creation works but leaves lifecycle semantics inconsistent.
- Update/delete success strings are passed as `ApiResponse<string>.Ok(value)`, so the text is data rather than the response message used elsewhere.

**Complete target flow**

1. Authenticate, validate session version, and require `Active Client`.
2. Read/update only the current subject; use a concurrency token.
3. Keep email outside ordinary updates.
4. Validate DOB/business rules consistently and store an audit record for material changes.
5. For deletion, require recent authentication and explicit confirmation; atomically mark deleted, set `DeletedAt`, increment session version, revoke refresh tokens, disable public/availability state, and enqueue a notification.
6. Apply a documented retention/anonymization/purge policy and make repeated deletion requests safely idempotent.

#### P2.2 Lawyer profile flow

**What is good**

- Private operations require the Lawyer role and target the current authenticated subject.
- The owner-only response may legitimately contain NationalNumber, while the public DTO omits NationalNumber, email, and phone.
- The public route explicitly overrides controller authorization with `AllowAnonymous`, making its intended accessibility clear.

**Gaps**

- Public publication rules are missing, as described in P0.4.
- Status/session enforcement is absent on private operations.
- Email change has all P0/P1 defects above.
- `SpecializationId` is not prevalidated as an existing, non-soft-deleted allowed specialization. A foreign-key violation can become a 500 and the exception middleware can leak its stack.
- `Level` lacks `IsInEnum`, so arbitrary numeric values can be bound and persisted.
- Lawyer DOB lacks even the client endpoint’s past-date check.
- `YearsOfExperience` uses `< 50` while its message says 0–50; exactly 50 is rejected.
- The request’s `Address` updates `ApplicationUser.Address`, while `LawyerProfile.Address` also exists but is unused. One source of truth is required.
- `IsAvailable` and `ProfilePicture` are returned but cannot be changed through this slice; document separate flows or complete the update design.
- Delete leaves professional/public availability concerns unresolved and has the same weak reauthentication, retention, and access-token behavior as client deletion.
- Current EF model drift means the lawyer-level change is not cleanly deployable from the checked migration state.

**Complete target flow**

1. Enforce active lawyer session state for private operations.
2. Validate all enum and reference IDs against active reference data.
3. Establish one address field/source and one clear profile creation lifecycle.
4. Use an explicit verification/publication state machine independent of email confirmation.
5. Publish only approved, active, opted-in profiles; make deletion/suspension immediately remove public visibility.
6. Reconcile the model snapshot/migrations and test an empty-database migration before release.

#### P2.3 ChangePassword

**What is good**

- Requires authentication.
- Resolves the current user from claims.
- Requires and checks the current password through Identity.
- Uses the shared new-password validator.

**Gaps and completion requirements**

- Enforce active account/session state.
- Explicitly reject a new password equal to the current password.
- Make password/session changes atomic and check every Identity result.
- Invalidate access and refresh sessions as a defined policy.
- Send a security notification and audit the event.
- Rate-limit failed current-password attempts. These attempts do not automatically contribute to ordinary login lockout, so the endpoint otherwise becomes a brute-force oracle from a stolen session.
- Require a fresh MFA/recent-auth challenge for elevated-risk sessions.

#### P2.4 ForgotPassword

**What is good**

- Public by design.
- Uses a validated email request.
- Textually returns the same success result for missing/unconfirmed users.

**Gaps and completion requirements**

- Normalize timing and enqueue work uniformly.
- Define eligibility by status; do not send to deleted/suspended accounts unless a separate reinstatement flow requires it.
- Use a normalized Identity lookup.
- Replace unsafe URL building/config fallback.
- Persist a request/rate record and use a durable outbox.
- Respect cancellation before durable commit; do not pretend cancellation can retract an already committed job.
- Keep the same generic response for all cases.

#### P2.5 ResendVerification

**What is good**

- Public by design and request validated.
- Textually generic response reduces direct enumeration.

**Gaps and completion requirements**

- Same timing enumeration, delivery, URL, status, and global-limiter defects as ForgotPassword.
- No persistent cooldown, daily send count, or last-send timestamp.
- Repeated requests generate/send tokens without a durable superseding request model.
- Only unverified accounts in an eligible state should be considered; response remains generic regardless.

#### P2.6 ConfirmEmail

**Gaps and completion requirements**

- Change API mutation to POST behind an intentional landing-page action.
- Validate bounded user ID/token inputs without throwing.
- Use one generic invalid/expired response for not-found, malformed, expired, or invalid tokens.
- Make a valid replay idempotent without leaking account existence.
- Atomically confirm Identity email, transition domain status by role, record audit, consume the request, and enqueue notification.
- Never reactivate deleted/suspended users.

#### P2.7 ConfirmEmailChange

In addition to ConfirmEmail requirements:

- Require that a matching durable pending-email request exists.
- Bind token, user, normalized new email, nonce/version, and expiry.
- Atomically update email and username, consume the pending request, increment session version, revoke refresh tokens, and enqueue old/new-address notifications.
- Resolve final uniqueness races with a generic safe error and keep the old address unchanged.
- Do not expose the raw new email in a link if an opaque request ID can represent it.

## Recommended rate limits

These are conservative **starting baselines**, not universal constants. Load-test them, observe legitimate mobile retry behavior and NAT traffic, then tune. Each “+” means independent buckets must all allow the request. Limits should be distributed across application nodes where reliability matters.

| Endpoint | Recommended independent limits | Notes |
|---|---|---|
| `GET /api/clients/profile` | 120/min/user, burst 30 **+** 300/min/IP | Read-only; add short caching only if claims/state checks remain correct. |
| `PUT /api/clients/profile` | 20/15 min/user **+** 60/15 min/IP | If email remains temporarily embedded, additionally apply 3/hour/user and 10/hour/IP only when email differs. Preferred: split email change. |
| `DELETE /api/clients/profile` | 3/day/user **+** 10/day/IP | Also require recent auth and idempotency; rate limiting is not the security control. |
| `GET /api/lawyers/profile` | 120/min/user, burst 30 **+** 300/min/IP | Private owner read. |
| `PUT /api/lawyers/profile` | 20/15 min/user **+** 60/15 min/IP | Apply the separate email-change policy if needed. |
| `DELETE /api/lawyers/profile` | 3/day/user **+** 10/day/IP | Require recent auth; remove public visibility atomically. |
| `GET /api/lawyers/public/{id}` | 120/min/IP, burst 30 **+** edge/global capacity ceiling | Cache publishable responses for 1–5 minutes; do not cache internal/non-public states. |
| `POST /api/auth/change-password` | 5/15 min/user **+** 20/15 min/IP; at most 3 successful changes/day/user | Step up authentication after repeated failures. Do not rely on login lockout. |
| `POST /api/auth/forgot-password` | 5/15 min/IP **+** 3/hour/account-key **+** 10/day/account-key | Risk/CAPTCHA challenge after threshold. Same limits for nonexistent keys. Never account-lock from this flow. |
| `POST /api/auth/resend-verification` | 5/15 min/IP **+** 1/min/account-key **+** 3/hour/account-key **+** 10/day/account-key | The one-minute bucket is a durable send cooldown. |
| `POST /api/auth/reset-password` | 10/15 min/IP **+** 5/hour/account-key **+** 5 invalid attempts/token fingerprint | Token fingerprint must be an HMAC/hash, never the raw token. Successful use consumes the token. |
| `POST /api/auth/confirm-email` | 20/15 min/IP **+** 5/hour/user-or-request key | A valid consumed request should be safely idempotent. |
| `POST /api/auth/confirm-email-change` | 10/15 min/IP **+** 5/hour/user-or-request key | Single-use pending request; final uniqueness check remains mandatory. |
| Proposed `POST /api/auth/email-change/request` | 10/15 min/IP **+** 3/hour/user **+** 5/day/user | Require recent auth/current password or MFA; notify old email. |

Additional controls:

- Do not queue auth/security requests inside the rate limiter; reject immediately with 429.
- Add a global edge capacity limit as operational protection, but never substitute it for identity/account partitions.
- Use a sliding window or token bucket for general API traffic and durable fixed/sliding business counters for hourly/daily email/security actions.
- Add randomized backoff/risk challenge rather than disclosing exact account bucket state.
- Make configuration centrally adjustable and expose metrics for allowed/rejected requests by policy and coarse reason.

## Required target state by flow

### Profile read/update

`JWT validation -> session-version/status check -> role policy -> current subject -> DTO validation -> domain/reference validation -> concurrency check -> transaction -> audit/outbox -> consistent ApiResponse`

### Password change

`active authenticated session -> rate/risk check -> recent auth/current password (+ MFA when required) -> password validation -> atomic password + session revocation + audit/outbox -> fresh session or sign-in-required response`

### Forgot/reset password

`generic forgot request -> independent IP/account limits -> uniform response timing -> durable request/outbox -> one-time opaque link -> explicit POST reset -> generic token validation -> atomic password/session revocation/audit -> notification`

### Email confirmation

`clickable frontend landing GET -> explicit API POST -> bounded generic validation -> atomic Identity confirmation + domain state transition + request consumption + audit/outbox -> safe idempotent result`

### Email change

`active session -> recent-auth/MFA -> normalized uniqueness preflight -> durable pending request -> old-address notice + new-address confirmation -> explicit POST -> final uniqueness check -> atomic email/username/session/audit/outbox update -> notify both addresses`

## Test plan required for release

### Authorization and account state

- Anonymous, wrong-role, missing-subject-claim, forged/stale role, deleted, suspended, unverified, pending-review, and active cases for every endpoint.
- Prove that deletion/suspension/password reset/email change invalidates current access tokens and all intended refresh tokens.
- Public lawyer lookup must return 404 for client IDs, missing profiles, unverified lawyers, non-approved lawyers, suspended/deleted lawyers, and opted-out profiles.

### Client and lawyer profiles

- Owner read/update happy paths and validation failures.
- Client DOB minimum/maximum boundaries.
- Lawyer invalid numeric enum, inactive/missing specialization, DOB boundaries, 0/50 years-of-experience boundaries, and address source-of-truth.
- Concurrent update conflict and retry behavior.
- Email-change job/persistence failure ordering and duplicate/case/plus-address scenarios.
- Deletion replay/idempotency, retention metadata, and immediate public/session effects.

### Password and recovery

- Wrong current password, new equals current, policy boundaries, breached-password rejection, and MFA/recent-auth policy.
- Failure injection after password mutation to prove complete rollback/session revocation.
- Forgot/resend identical status/body and acceptable timing envelope for nonexistent, confirmed, unconfirmed, deleted, and suspended accounts.
- Reset malformed/expired/replayed/wrong-user token cases return one safe result.
- Successful reset never signs in automatically and invalidates all required sessions.

### Confirmation and email delivery

- Malformed GUID, oversized token, malformed Base64Url, expired token, replay, concurrent confirmation, and link-prefetch simulation.
- Correct Client and Lawyer post-confirmation status transitions; no deleted/suspended reactivation.
- Email/username/pending-request transaction atomicity and final uniqueness race.
- SMTP transient failure retries, permanent failure/dead-letter behavior, and outbox recovery after process restart.
- Token generated on node A is accepted on node B and across an application restart using the shared key ring.

### Rate limiting

- Independent IP and account/user buckets; one user cannot exhaust everybody’s allowance.
- Distributed-node consistency or documented gateway behavior.
- Trusted proxy address resolution and rejection/ignoring of untrusted forwarded headers.
- Same generic 429 envelope for all partitions; no raw account/email/token keys in logs.
- Exact boundary, window rollover, concurrency, clock behavior, and successful-action daily caps.

## Implementation order

1. Enforce account status and session version on every authenticated request.
2. Fix public lawyer publication predicates immediately.
3. Split email change from profile update and implement a durable pending-change/outbox flow.
4. Make password/reset/email/session mutations transactional and enforce access-token invalidation.
5. Replace global rate-limit buckets with independent IP and account/user partitions; correct middleware/proxy ordering and add distributed enforcement.
6. Make confirmation an explicit POST, add safe validation/idempotency, and complete role-specific status transitions.
7. Repair email retry semantics, safe URL building, required HTTPS configuration, logging redaction, and shared data-protection keys.
8. Correct lawyer validation/model inconsistencies and reconcile EF model drift.
9. Add the endpoint/security/integration tests above before release.

## Final assessment

The reviewed code is a useful skeleton, but the security boundaries are presently centered on role claims and Identity token calls rather than on complete account lifecycles. The release criterion should be stricter: status-aware authorization, immediately enforceable session invalidation, atomic sensitive mutations, publishable-lawyer predicates, durable and private recovery/email workflows, independent distributed abuse controls, and automated tests proving both successful and adversarial paths. Until the P0 and P1 items are resolved, these slices should not be considered complete end to end.

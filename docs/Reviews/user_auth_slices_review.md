# Prioritized Review Feedback for Auth Slices

This document organizes the review feedback for your Auth slices (Confirm Email, Login, Register Client, Register Lawyer, Refresh Token, Revoke) by priority. This serves as an actionable checklist.

## 🔴 P0: Critical (Must Fix Immediately - Security & Blockers)
These issues expose the application to immediate security risks or fundamental data corruption.

- [x] **Plaintext Secrets:** Resolved (Refresh tokens are now hashed with SHA-256 before storing).
- [x] **Tracked Credentials:** Secrets (JWT keys, DB passwords, SMTP) are committed in `appsettings.json`.
- [x] **Session Revocation:** Password change and password reset flows **do not** revoke active refresh tokens.
- [x] **Token Exposure in URLs:** Confirm Email uses `GET /api/auth/confirm-email?userId&token`. State-changing tokens are exposed in URLs and server logs.
- [x] **Refresh Token Lookup Risk:** Refresh depends on the access token (with lifetime validation disabled) to resolve the user. This is a design risk. It should rely *only* on a hashed lookup of the refresh token itself.
- [x] **Entity Tracking Bug:** Login uses `FindByEmailAsync` without `Include(RefreshTokens)`, which risks EF Core owned-collection tracking issues.

## 🟠 P1: High (Contract Violations & Core Flow Breakages)
These issues break the API contract, prevent frontend integration, or cause core business logic to fail.

- [x] **Confirm Email Contract Mismatch:** Implements `GET` instead of the contracted `POST /api/auth/verify-email` with a JSON body.
- [x] **Confirm Email State Transition:** Does not promote `UserStatus` to `Active` after a successful confirmation.
- [x] **Refresh Token Contract Mismatch (Request):** Expects `{ token, refreshToken }` but the contract only expects `{ refreshToken }`.
- [x] **Refresh Token Contract Mismatch (Response):** Returns the full `LoginResponse` instead of just `{ accessToken, refreshToken, expiresAt }`.
- [x] **Refresh Token Error Codes:** Invalid/expired refresh returns `BusinessException` (400) instead of a standard `401 Unauthorized`.
- [x] **Login Response Mismatch:** The response schema is flattened instead of matching the contract (which expects a nested `user` object, `accessToken`, and `expiresAt`).
- [x] **Login Lockout:** Lockout is disabled (`lockoutOnFailure: false`). It must be enabled.
- [x] **Register Lawyer Contract Mismatch:** Content-Type expects `multipart/form-data` + 4 files as per contract, but the code expects `[FromBody]` JSON.
- [x] **Registration Transactions:** There is no DB transaction around user creation, role assignment, and email sending. If one fails, the database is left in a partial state.
- [x] **Registration Error Codes:** Duplicate identities map to 400 `ValidationException` instead of the standard 409 Conflict.
- [x] **Revoke Authorization:** No explicit `[AllowAnonymous]` or `[Authorize]`. Returns 200 instead of 401 for an inactive RT.

## 🟡 P2: Medium (Data Completeness, UX, & Minor Issues)
These issues cause data loss, bad UX, or minor deviations that should be fixed before production.

- [x] **Missing Profiles:** Registration does not create the required `ClientProfile` or `LawyerProfile` entities, nor `NotificationPreference`.
- [x] **Hardcoded Refresh TTL:** Refresh TTL is hardcoded to 14 days in code; the contract specifies 7 days.
- [x] **API Response Wrapping:** `ApiResponse` success messages often incorrectly end up in the `data` field instead of the `message` field.
- [x] **Lawyer Registration Data Loss:** Combined address can exceed the DB's 500-character limit. Government/city fields are discarded as distinct data.
- [x] **Lawyer Registration Validation:** Gender accepts arbitrary text.
- [x] **Client Registration Schema:** Extra required `nationalNumber` is present but not in the auth contract body.
- [x] **Login Audit:** No `LastLoginAt` tracking.
- [x] **Confirm Email Error Handling:** Malformed Base64 strings can produce 500 errors.

## 🟢 Fine / Compliant (What you did well!)
No action needed here, these parts are working exactly as intended.

- **Routing & HTTP Verbs:** Controllers correctly handle routes and 201 statuses.
- **Success Messages:** Arabic success messages are present.
- **Role Assignment:** Roles (`Client`, `Lawyer`) are assigned correctly.
- **Email Sending:** Auth helper correctly sends emails.
- **Login Rejections:** Returns 401/403 properly for invalid/unverified login attempts.
- **Refresh Token Rotation:** Rotation works, and replay handling correctly revokes all active tokens if an inactive token is reused.
- **Revoke Endpoint:** `POST /api/auth/revoke` is implemented and seen as a positive optional feature.
- **Initial Confirm Link:** Uses `userId=` correctly in the generated email link.

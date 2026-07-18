# Auth & Users Slice Fixes Checklist

## 1. Auth > Change Password
- `[x]` **Session Invalidation (P0):** Revoke existing refresh tokens after password change. (Load `User.Include(u => u.RefreshTokens)` and set `RevokedOn` on all active tokens).
- `[x]` **Validation:** Mirror registration password strength rules using a shared FluentValidation extension.
- `[x]` **Error Reporting:** Separate incorrect-current-password errors from password policy errors.
- `[x]` **Response Formatting:** Move success message from `data` to `message` property.
- `[x]` **Cancellation:** Add `CancellationToken` propagation.
- `[x]` **Architecture:** Move logic from monolithic `AuthService` into a dedicated `ChangePasswordService`.

## 2. Auth > Forgot Password
- `[x]` **Anti-Enumeration Leak (P1):** Ensure unverified emails return a silent 200 OK instead of throwing a 400 error.
- `[x]` **Rate Limiting:** Implement rate limiting (max 3/hour).
- `[x]` **Token Lifetime:** Explicitly configure the reset-token lifetime to 1 hour instead of Identity's default.
- `[x]` **URL Target:** Update the generated reset URL to point to a confirmed frontend UI page instead of the API POST route.
- `[x]` **Cancellation:** Add `CancellationToken` propagation.
- `[x]` **Architecture:** Move logic from monolithic `AuthService` into a dedicated `ForgotPasswordService`.

## 3. Auth > Resend Verification
- `[x]` **Broken URL Generation (P0):** Change query parameter from `?email={email}` to `?userId={user.Id}`.
- `[x]` **Code Duplication:** Reuse `IAuthHelperService.SendConfirmationEmailAsync` instead of manually duplicating logic.
- `[x]` **Rate Limiting:** Implement rate limiting (max 3/hour).
- `[x]` **Response Formatting:** Move success message from `data` to `message` property.
- `[x]` **Cancellation:** Add `CancellationToken` propagation.
- `[x]` **Architecture:** Move logic from monolithic `AuthService` into a dedicated `ResendVerificationService`.

## 4. Auth > Reset Password
- `[x]` **Session Invalidation (P0):** Revoke existing refresh tokens after password reset.
- `[x]` **Validation:** Mirror registration password strength rules.
- `[x]` **Error Handling:** Add safe handling for malformed encoded tokens to prevent unhandled 500 errors.
- `[x]` **Response Formatting:** Move success message from `data` to `message` property.
- `[x]` **Cancellation & Throttling:** Add `CancellationToken` propagation and throttling.
- `[x]` **Architecture:** Move logic from monolithic `AuthService` into a dedicated `ResetPasswordService`.

## 5. Users Slice > Clients
- `[ ]` **Sensitive Data Exposure:** Ensure the National Number is NOT returned in public profile DTOs.
- `[ ]` **Data Integrity:** Implement soft-delete rules / retention policies instead of a hard-delete of the Identity account.
- `[ ]` **Atomicity:** Ensure email and username updates are persisted atomically (e.g., within a transaction).
- `[ ]` **Email Verification:** Implement email-change verification workflow.
- `[ ]` **Data Mapping:** Fix null dates of birth so they remain `null` instead of `DateOnly.MinValue`.
- `[ ]` **Validation:** Align Egyptian phone format validation with lawyer registration.
- `[ ]` **Contract Alignment:** Align routes with the canonical `/api/users/profile` contract.
- `[ ]` **Performance:** Optimize the GET endpoint into a single query instead of three separate database operations.
- `[ ]` **Cancellation:** Add `CancellationToken` propagation across asynchronous reads.

## 6. Users Slice > Lawyers
- `[ ]` **Profile Creation Bug:** Ensure `LawyerProfile` is actually created during registration, not just upon a later update.
- `[ ]` **Marketplace Browsing:** Provide a separate public DTO/endpoint for public marketplace browsing (since the current GET is owner-only).
- `[ ]` **Validation Errors:** Fix `YearsOfExperience` to allow `0` and enforce an upper bound.
- `[ ]` **Validation Details:** Map Specialization to legal-category relationships instead of an unrestricted string.
- `[ ]` **Missing Data Fields:** Add `IsAvailable`, profile picture, verification status, government/city, and specializations to the profile.
- `[ ]` **Shared Client Issues:** Apply the same fixes for hard-delete, atomicity, and query optimization as in the Clients slice.

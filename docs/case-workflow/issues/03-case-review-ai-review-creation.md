# 03 — CaseReview Slice: AI Review with Review Report

**What to build:** A client with a `Submitted` case can request an AI review. The system calls `IChatModelProvider` to analyze the case title, description, and documents, then parses the AI response into structured `ReviewPoint` items (Strength, Weakness, Suggestion, MissingCaseInfo, MissingCaseDoc). A new `CaseReviewReport` is created and any previous report has `IsLatest` set to `false`. The case transitions from `Submitted → Reviewed`.

This is the `Features/CaseReview` slice: controller, `ICaseReviewService`/`CaseReviewService`, DTOs, validators.

The endpoint is `POST /api/cases/{id}/review`. It returns the created `CaseReviewReportDto` wrapped in `ApiResponse<T>`.

Error scenarios:
- Case not found → `BusinessException` (404)
- Caller is not the case owner → `BusinessException` (403)
- Case not in `Submitted` status → `BusinessException` (400)
- AI timeout/failure → exception propagates, case remains `Submitted`, no partial report created

**Blocked by:** 01 — CaseStatus transition guard (needs `Submitted → Reviewed` transition).

**Status:** ready-for-agent

- [ ] `POST /api/cases/{id}/review` endpoint exists on `CaseReviewController`
- [ ] Only the case owner can call it (403 for non-owners)
- [ ] Case must be in `Submitted` status (400 otherwise)
- [ ] AI is called via `IChatModelProvider.GenerateAsync()` with case content
- [ ] AI response is parsed into `ReviewPoint` entities with correct `ReviewPointType` values
- [ ] A `CaseReviewReport` is created with `IsLatest = true`
- [ ] Previous reports for the same case have `IsLatest` set to `false`
- [ ] Case status transitions to `Reviewed` via the transition guard
- [ ] Response returns `CaseReviewReportDto` with nested `ReviewPoint` items
- [ ] AI failure leaves case in `Submitted` status with no partial data
- [ ] Service-level tests cover: happy path, invalid status, non-owner, `IsLatest` flag management

# 04 — CaseReview Slice: Review Report Retrieval & Re-Review Loop

**What to build:** A client can retrieve their review history and iterate on their case. Two new read endpoints on `CaseReviewController`:

- `GET /api/cases/{id}/reviews` — returns all review reports for the case, ordered by creation date descending.
- `GET /api/cases/{id}/reviews/latest` — returns only the report where `IsLatest == true`.

Additionally, the re-review loop is wired up: when a client edits a `Reviewed` case (via the existing `PUT /api/cases/{id}` endpoint), the case status reverts from `Reviewed → Submitted`. This enables the client to request another review, creating a new report while preserving all history. The full review iteration loop is demoable end-to-end: submit → review → edit → re-submit → re-review.

**Blocked by:** 03 — CaseReview AI review creation (needs review reports to exist to retrieve them).

**Status:** ready-for-agent

- [ ] `GET /api/cases/{id}/reviews` returns all `CaseReviewReportDto` items ordered by `CreatedAt DESC`
- [ ] `GET /api/cases/{id}/reviews/latest` returns only the report with `IsLatest == true`
- [ ] Both endpoints enforce case ownership (403 for non-owners)
- [ ] Both endpoints return 404 if case not found
- [ ] Editing a `Reviewed` case (existing `PUT` endpoint) reverts status to `Submitted`
- [ ] After reverting, the client can call `POST /api/cases/{id}/review` again to get a new review
- [ ] The new review creates a new report with `IsLatest = true` and demotes the previous one
- [ ] Full loop is testable: create → submit → review → edit → re-submit → re-review → verify two reports exist with correct `IsLatest` flags

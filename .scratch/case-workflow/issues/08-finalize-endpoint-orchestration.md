# 08 — Finalize Endpoint: Orchestration

**What to build:** `POST /api/cases/{id}/finalize` on the existing `CaseController` orchestrates the entire analysis → matching → explanation pipeline in a single synchronous call.

Preconditions:
- Case must exist (404).
- Caller must be the case owner (403).
- Case must be in `Reviewed` status (400).

Orchestration sequence:
1. Transition case to `FinalSubmitted`.
2. Call `ICaseAnalysisService.AnalyzeCaseAsync()` → produces `CaseProfile`.
3. Transition case to `Analyzed`.
4. Call `IMatchingService` → eligibility filtering → qualification scoring.
5. Call `IChatModelProvider` (via matching service) → batch explanation generation.
6. Persist `CaseRecommendation` entities.
7. Transition case to `Matched`.
8. Return `FinalizeResultDto` with all recommendations.

**Transactionality**: If any step fails (AI timeout, parsing error), the entire operation rolls back. Case reverts to `Reviewed`. No partial `CaseProfile` or `CaseRecommendation` entities are persisted. The client can retry safely.

**Idempotency**: If the case is already in `Matched` status, return the existing recommendations without re-running the pipeline. A duplicate finalize call is not an error.

**Empty result**: If no lawyers are eligible, `TotalEligibleLawyers = 0` with an empty recommendations list. This is a valid outcome — the case still transitions to `Matched`.

**Blocked by:** 03 — CaseReview (case must reach `Reviewed` to finalize), 05 — CaseAnalysis (finalize calls it), 07 — Matching explanation & persistence (finalize calls it).

**Status:** ready-for-agent

- [ ] `POST /api/cases/{id}/finalize` exists on `CaseController`
- [ ] Only the case owner can call it (403 for non-owners)
- [ ] Case must be in `Reviewed` status (400 otherwise)
- [ ] Calls `ICaseAnalysisService` which produces a `CaseProfile`
- [ ] Calls `IMatchingService` which produces scored, ranked, explained recommendations
- [ ] Case transitions through `FinalSubmitted → Analyzed → Matched`
- [ ] Response is `FinalizeResultDto` with `TotalEligibleLawyers` and ranked `Recommendations`
- [ ] If AI fails at any step, case reverts to `Reviewed` with no partial data
- [ ] Duplicate call on a `Matched` case returns existing recommendations (idempotent)
- [ ] Empty eligible pool returns `TotalEligibleLawyers = 0` — not an error
- [ ] Tests cover: full happy path, transactionality on AI failure, idempotent re-call, empty eligible pool, wrong status, non-owner

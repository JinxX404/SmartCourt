# 07 — Matching Slice: Explanation Generation & Recommendation Persistence

**What to build:** After the matching engine scores eligible lawyers (ticket 06), the system makes one batch `IChatModelProvider.GenerateAsync()` call to produce a natural language explanation for each recommended lawyer. The AI receives: case summary, each lawyer's name, per-factor scores, total score, and the matching strategy used. It returns one explanation string per lawyer.

The results (scores + explanations) are persisted as `CaseRecommendation` entities, one per recommended lawyer, with a `Rank` assigned by descending `TotalScore`.

A retrieval endpoint `GET /api/cases/{id}/recommendations` on `MatchingController` returns the persisted recommendations as a `FinalizeResultDto`:

```
FinalizeResultDto {
    CaseId: Guid
    TotalEligibleLawyers: int
    Recommendations: [
        {
            LawyerId, LawyerName, TotalScore,
            LocationScore, ExperienceScore, RatingScore, ResponseTimeScore,
            Explanation, Rank
        }
    ]
}
```

Error: if case is not in `Matched` status, return `BusinessException("Recommendations are not available. The case has not been matched yet.")`.

**Blocked by:** 06 — Matching eligibility & scoring (needs scored lawyers to generate explanations for).

**Status:** ready-for-agent

- [ ] One batch `IChatModelProvider.GenerateAsync()` call generates explanations for all scored lawyers
- [ ] AI prompt includes case summary, lawyer names, per-factor scores, total scores, and matching strategy
- [ ] `CaseRecommendation` entities are persisted with all scores, explanation, and rank
- [ ] `GET /api/cases/{id}/recommendations` exists on `MatchingController`
- [ ] Endpoint returns `FinalizeResultDto` wrapped in `ApiResponse<T>`
- [ ] Endpoint enforces case ownership (403 for non-owners)
- [ ] Returns 400 with descriptive message if case is not in `Matched` status
- [ ] If no eligible lawyers exist, `TotalEligibleLawyers = 0` with empty `Recommendations` list (not an error)
- [ ] Tests cover: explanation generation call, recommendation persistence, retrieval endpoint, non-matched case error

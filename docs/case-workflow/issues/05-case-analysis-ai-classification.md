# 05 — CaseAnalysis Slice: AI Classification into CaseProfile

**What to build:** An internal-only service (`ICaseAnalysisService` / `CaseAnalysisService`) that takes a finalized case's content (title, description, documents) and calls `IChatModelProvider` to classify it into a `CaseProfile`. The AI determines three enum values:

- `Specialization` — which legal specialization the case requires (from the `Specialization` enum)
- `RequiredLawyerLevel` — the minimum lawyer level by Egyptian law (from the `LawyerLevel` enum)
- `Complexity` — the complexity tier (from the `CaseComplexity` enum, drives matching strategy selection)

The result is persisted as a `CaseProfile` entity with a 1:1 relationship to the `Case`. There is **no external API** — this is called only by the finalize orchestration.

The `CaseProfile` is an internal-only entity. It must never be exposed to clients via any API response.

**Blocked by:** 02 — Entity changes (needs `CaseProfile` navigation property on `Case`).

**Status:** ready-for-agent

- [ ] `ICaseAnalysisService` interface exists in `Features/CaseAnalysis/`
- [ ] `CaseAnalysisService` implements it, injecting `IChatModelProvider` and `ApplicationDbContext`
- [ ] `AnalyzeCaseAsync(Guid caseId)` sends case content to the AI and parses the response into a `CaseProfile`
- [ ] AI prompt instructs the model to choose from the predefined `Specialization`, `LawyerLevel`, and `CaseComplexity` enum values
- [ ] `CaseProfile` is persisted with correct FK to `Case`
- [ ] If AI returns an unparseable response, a `BusinessException("AI analysis failed. Please try again.")` is thrown and the raw response is logged
- [ ] No controller or external API endpoint exists for this slice
- [ ] Tests cover: happy path parsing, each enum value mapping, unparseable response handling

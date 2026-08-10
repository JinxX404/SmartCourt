# 06 — Matching Slice: Eligibility Filtering & Qualification Scoring

**What to build:** The core matching engine with two layers:

**Layer 1 — Eligibility (deterministic filtering):**
- Lawyer must have a `LawyerSpecialization` matching the `CaseProfile.Specialization`.
- Lawyer's `Level` must be ≥ `CaseProfile.RequiredLawyerLevel`.
- `LawyerProfile.IsAvailable` must be `true`.
- Ineligible lawyers are removed entirely.

**Layer 2 — Qualification (weighted scoring):**
Each eligible lawyer gets a `TotalScore = (LocationWeight × LocationScore) + (ExperienceWeight × ExperienceScore) + (RatingWeight × RatingScore) + (ResponseTimeWeight × ResponseTimeScore)`.

Factor scoring:
- **Location**: `GovernorateRegions` static class maps Egypt's governorates into regions (Greater Cairo, Alexandria Region, Delta, Canal Zone, Upper Egypt North, Upper Egypt South, Red Sea/Sinai, New Valley). Same governorate = 1.0, same region = 0.5, different region = 0.0.
- **Experience**: For the matched specialization: `0.85 × normalizedYears + 0.15 × normalizedCasesHandled`. Min-max normalized across the eligible pool.
- **Rating**: `AverageRating / 5.0`. Default 0.5 if no ratings (value is 0).
- **Response Time**: Inverted and min-max normalized. Default 0.5 if no data (value is 0).

Strategy weights come from the existing `MatchingStrategy` class, selected by `CaseProfile.Complexity`.

Edge cases: if only one lawyer is eligible, all normalized factors = 1.0. If all lawyers have the same value for a factor, all get 1.0.

All eligible lawyers are returned sorted by descending `TotalScore`. No cap.

The scoring computation should be independently testable (pure function, no I/O).

**Blocked by:** 02 — Entity changes (needs `LawyerSpecialization` entity), 05 — CaseAnalysis (needs `CaseProfile` to drive eligibility and strategy selection).

**Status:** ready-for-agent

- [ ] `IMatchingService` interface exists in `Features/Matching/`
- [ ] `MatchingService` implements eligibility filtering: specialization match, level ≥ required, `IsAvailable`
- [ ] `GovernorateRegions` static class maps all 27 Egyptian governorates to regions
- [ ] Location scoring: same governorate = 1.0, same region = 0.5, different = 0.0
- [ ] Experience scoring: `0.85 × normalizedYears + 0.15 × normalizedCases` with min-max normalization
- [ ] Rating scoring: `AverageRating / 5.0`, default 0.5 for zero
- [ ] Response time scoring: inverted min-max, default 0.5 for zero
- [ ] `MatchingStrategy` selects weights by `CaseComplexity`
- [ ] Edge case: single eligible lawyer gets 1.0 for all normalized factors
- [ ] Edge case: all-same-value factor gets 1.0 for all lawyers
- [ ] Scoring logic is testable as pure computation (independent from DB/AI)
- [ ] Tests cover: eligibility filtering (each filter independently), scoring formula, normalization edge cases, strategy weight selection

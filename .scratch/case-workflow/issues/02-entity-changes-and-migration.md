# 02 — Entity Changes: Case, LawyerProfile, ApplicationUser, LawyerSpecialization, CaseRecommendation

**What to build:** Apply all the schema/entity changes required by the Case Workflow spec in one migration. After this ticket, the data model supports multi-specialization per lawyer (with per-specialization experience), case location (governorate/city), case recommendations, and the corrected review report structure.

Specific changes:

- **Case entity**: Add `Governorate` (string, nullable) and `City` (string, nullable). Add navigation properties to `CaseProfile`, `ICollection<CaseReviewReport>`, and `ICollection<CaseRecommendation>`.
- **CaseReviewReport entity**: Remove the `CaseComplexity` property (complexity belongs exclusively to `CaseProfile`).
- **LawyerProfile entity**: Remove `SpecializationId`, `Specialization` (single FK), `YearsOfExperience`, and `Address`. Add `ICollection<LawyerSpecialization>` navigation, `AverageRating` (decimal), and `AverageResponseTimeHours` (decimal).
- **ApplicationUser entity**: Rename `Government` → `Governorate`.
- **New LawyerSpecialization entity**: `Id`, `LawyerProfileUserId` (FK), `Specialization` (enum), `YearsOfExperience` (int), `CasesHandled` (int). Supports many-to-many with per-specialization experience data.
- **New CaseRecommendation entity**: `Id`, `CaseId` (FK), `LawyerId` (FK), `TotalScore`, `LocationScore`, `ExperienceScore`, `RatingScore`, `ResponseTimeScore`, `Explanation`, `Rank`. Inherits `AuditableEntity`.
- EF Core Fluent API configurations for all new/modified entities.
- EF Core migration generated and tested.

**Blocked by:** 01 — CaseStatus enum expansion (the Case entity references the new enum values).

**Status:** ready-for-agent

- [ ] `Case` entity has `Governorate`, `City` properties and navigation properties to `CaseProfile`, `ReviewReports`, `Recommendations`
- [ ] `CaseReviewReport` no longer has a `CaseComplexity` property
- [ ] `LawyerProfile` no longer has `SpecializationId`, `Specialization`, `YearsOfExperience`, or `Address`
- [ ] `LawyerProfile` has `ICollection<LawyerSpecialization>`, `AverageRating`, `AverageResponseTimeHours`
- [ ] `ApplicationUser.Government` is renamed to `Governorate`
- [ ] `LawyerSpecialization` entity exists with `Specialization` enum, `YearsOfExperience`, `CasesHandled`
- [ ] `CaseRecommendation` entity exists with all score fields, `Explanation`, and `Rank`
- [ ] EF Core configurations use Fluent API (no data annotations)
- [ ] Migration generates cleanly and applies without errors
- [ ] Existing tests still pass after the migration (no regressions from renamed/removed properties)

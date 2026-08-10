# 01 — Expand CaseStatus Enum & Add Transition Guard

**What to build:** The `CaseStatus` enum currently has 3 values (`Draft`, `Submited`, `Closed`). Extend it to 7 values that model the full case workflow lifecycle: `Draft (0)`, `Submitted (1)` (typo fix from `Submited`), `Reviewed (2)`, `FinalSubmitted (3)`, `Analyzed (4)`, `Matched (5)`, `Closed (6)`. Create a `CaseStatusTransitionGuard` that enforces only the documented valid transitions, following the exact pattern of the existing `ContractTransitionGuard`. When an invalid transition is attempted, the guard throws a `BusinessException` with a descriptive message.

The valid transitions are:
- `Draft → Submitted`
- `Submitted → Reviewed`
- `Reviewed → Submitted` (re-review after edit)
- `Reviewed → FinalSubmitted`
- `FinalSubmitted → Analyzed`
- `Analyzed → Matched`
- `Matched → Closed`

**Blocked by:** None — can start immediately.

**Status:** ready-for-agent

- [ ] `CaseStatus` enum has 7 values: `Draft(0)`, `Submitted(1)`, `Reviewed(2)`, `FinalSubmitted(3)`, `Analyzed(4)`, `Matched(5)`, `Closed(6)`
- [ ] `Submited` is renamed to `Submitted` (integer value 1 unchanged — safe for existing DB data)
- [ ] `CaseStatusTransitionGuard` static class exists with an `EnsureCanTransition(CaseStatus from, CaseStatus to)` method
- [ ] Invalid transitions throw `BusinessException`
- [ ] Exhaustive transition matrix test covers every possible `(from, to)` pair, asserting exactly which are allowed and which throw — following the `ContractAndPaymentInvariantTests.ContractTransitionGuard_AllowsOnlyDocumentedTransitions` pattern

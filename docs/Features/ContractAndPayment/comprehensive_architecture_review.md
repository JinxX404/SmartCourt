# Comprehensive Architecture Review: Contracts, Milestones, Payments & Wallets

> **Review Date**: 2026-07-31 · **Status**: Design-Only (No Code Changes) · **Scope**: 4 Vertical Slices + Infrastructure Providers

---

## Table of Contents
1. [Executive Summary & End-to-End Completeness Audit](#1-executive-summary--end-to-end-completeness-audit)
2. [Service Breakdown & Bloat Analysis](#2-service-breakdown--bloat-analysis)
3. [Provider & Background Job Analysis](#3-provider--background-job-analysis)
4. [Illogical & Over-Engineered Code Findings](#4-illogical--over-engineered-code-findings)
5. [Proposed Class Deconstruction Blueprint](#5-proposed-class-deconstruction-blueprint)
6. [Zero-Regression Refactoring Roadmap](#6-zero-regression-refactoring-roadmap)

---

## 1. Executive Summary & End-to-End Completeness Audit

### Overall Assessment

The financial pipeline is **architecturally sound and end-to-end complete**. Every lifecycle stage from proposal acceptance through lawyer withdrawal is implemented with proper state transition guards, idempotency, and append-only audit trails. The primary issue is **service bloat** — four services carry 300–1,300 lines more than ideal, making them harder to navigate, test in isolation, and review during code changes.

### Lifecycle Verification Matrix

| # | Stage | Owning Service(s) | Status | Notes |
|---|-------|--------------------|--------|-------|
| 1 | **Proposal Acceptance** | `ContractCreationDependencyGate` | ✅ Complete | Gate validates proposal facts, idempotent via `UX_Contracts_ProposalId` |
| 2 | **Contract Draft & Acceptance** | `ContractService.CreateAsync` → `AcceptAsync` | ✅ Complete | Dual-party approval, `AcceptedByClientAt`/`AcceptedByLawyerAt`, history + outbox |
| 3 | **Contract Activation** | `ContractService.TryActivateAsync` + `EvaluateActivationAsync` | ✅ Complete | Activates when both parties accepted + ≥1 approved milestone with amount > 0 |
| 4 | **Milestone Negotiation & Approval** | `MilestoneService.AddAsync` → `ApproveAsync` | ✅ Complete | Sequential ordering, dual-party approval, triggers `EvaluateActivationAsync` |
| 5 | **Ready For Funding** | `MilestoneService.MarkReadyForFundingAsync` | ✅ Complete | Lawyer-only, sequential gate, unsettled-hold guard |
| 6 | **Milestone Funding** | `PaymentEscrowService.FundAsync` | ✅ Complete | Idempotent, provider call, escrow hold + ledger entry, wallet pending balance |
| 7 | **Funding Reconciliation** | `PaymentEscrowService.ReconcileProviderTransactionAsync` | ✅ Complete | Background job, re-queries provider for `Processing` transactions |
| 8 | **Webhook Handling** | `PaymentEscrowService.HandleWebhookAsync` | ✅ Complete | HMAC-SHA256 signature, timestamp window (±300s), idempotent event recording |
| 9 | **Deliverable Submission** | `MilestoneService.SubmitAsync` | ✅ Complete | Funding verification, file authorization, explicit DB transaction, version tracking |
| 10 | **7-Day Auto-Acceptance** | `MilestoneAutoAcceptanceService.AutoAcceptAsync` | ✅ Complete | Job scheduled via outbox → handler → Hangfire. Extensive guard checks |
| 11 | **Manual Accept / Request Changes** | `MilestoneService.AcceptAsync` / `RequestChangesAsync` | ✅ Complete | Client-only, funding re-verified, hold timestamps set |
| 12 | **14-Day Escrow Hold** | `EscrowReleaseService.ReleaseExpiredHoldAsync` | ✅ Complete | Serializable isolation, dispute check, provider call, double ledger entries (Release + Fee) |
| 13 | **Escrow Release & 5% Fee** | `EscrowReleaseService` + `SettlementCalculator` | ✅ Complete | `SettlementCalculator.Calculate` decomposes gross → net + fee; ledger entries are append-only |
| 14 | **Lawyer Wallet & Withdrawal** | `WalletService.WithdrawAsync` | ✅ Complete | Idempotent, atomic balance reservation (`ExecuteUpdateAsync`), provider call |
| 15 | **Withdrawal Reconciliation** | `WalletService.ReconcilePendingWithdrawalsAsync` | ✅ Complete | Batch of 100, respects idempotency reservations |
| 16 | **Contract Completion** | `ContractService.EvaluateCompletionAsync` | ✅ Complete | All approved milestones terminal + no disputes + no processing payments + no unsettled holds |
| 17 | **Contract Termination** | `ContractService.TerminateAsync` + `ContractTerminationSettlementService` | ✅ Complete | Refunds unstarted funded holds, cancels draft milestones, transition guard |
| 18 | **Change Requests** | `MilestoneService.CreateChangeRequestAsync` / `Approve` / `Reject` / `Cancel` | ✅ Complete | Bi-directional, transition guards, extension-only validation for duration/due date |

> [!NOTE]
> All 18 lifecycle stages have complete implementations with no broken links. Cross-slice communication correctly uses injected service interfaces (e.g., `MilestoneService` → `IContractService`, `PaymentEscrowService` → `IContractService`).

### Cross-Cutting Concern Completeness

| Concern | Implementation | Verdict |
|---------|---------------|---------|
| **Idempotency** | `IdempotencyService` + `CanonicalIdempotencyRequestHasher` + per-hold settlement reservations | ✅ Solid |
| **Append-only audit** | `MilestoneStateHistory`, `ContractStateHistory`, `EscrowLedgerEntry`, `PaymentWebhookEvent` — all insert-only | ✅ Compliant |
| **Concurrency control** | `RowVersion` (EF concurrency token) + `If-Match` header parsing + `DbUpdateConcurrencyException` handling | ✅ Solid |
| **Outbox pattern** | `OutboxWriter` → `OutboxDispatcher` → `IOutboxEventHandler` with lease-based claiming | ✅ Complete |
| **Transition guards** | `MilestoneTransitionGuard`, `ContractTransitionGuard`, `EscrowHoldTransitionGuard`, `ChangeRequestTransitionGuard` | ✅ Used consistently |

---

## 2. Service Breakdown & Bloat Analysis

### Line Count Summary

| Service | Lines | Max Target | Excess | Severity |
|---------|-------|-----------|--------|----------|
| [PaymentEscrowService.cs](file:///p:/Projects/Smart%20Court/SmartCourt/Features/Payments/PaymentEscrowService.cs) | **1,704** | 400 | +1,304 | 🔴 Critical |
| [MilestoneService.cs](file:///p:/Projects/Smart%20Court/SmartCourt/Features/Milestones/MilestoneService.cs) | **1,380** | 400 | +980 | 🔴 Critical |
| [ContractService.cs](file:///p:/Projects/Smart%20Court/SmartCourt/Features/Contracts/ContractService.cs) | **976** | 400 | +576 | 🟡 High |
| [WalletService.cs](file:///p:/Projects/Smart%20Court/SmartCourt/Features/Payments/WalletService.cs) | **630** | 400 | +230 | 🟡 Moderate |
| [EscrowReleaseService.cs](file:///p:/Projects/Smart%20Court/SmartCourt/Features/Payments/EscrowReleaseService.cs) | 517 | 400 | +117 | 🟢 Acceptable |
| [ContractTerminationSettlementService.cs](file:///p:/Projects/Smart%20Court/SmartCourt/Features/Payments/ContractTerminationSettlementService.cs) | 484 | 400 | +84 | 🟢 Acceptable |

### Responsibility Inventory per Service

#### PaymentEscrowService (1,704 lines) — 7 distinct responsibilities

| Responsibility | Methods | Approx Lines | Proposed Extraction |
|----------------|---------|-------------|---------------------|
| **Funding initiation** | `FundAsync` | ~200 | Keep in `PaymentEscrowService` |
| **Retry funding** | `RetryAsync` | ~220 | Keep in `PaymentEscrowService` |
| **Funding completion (shared)** | `CompleteFundingAsync`, `FailFundingAsync`, `KeepUnknownAndThrowAsync`, `KeepProcessingForReconciliationAsync` | ~200 | Keep (called by multiple paths) |
| **Webhook handling** | `HandleWebhookAsync`, `ValidateWebhookAuthentication`, `EnsureWebhookMatchesTransaction`, `RecordTerminalWebhookAsync`, `FinalizeFailedExternalResultAsync` | ~350 | Extract → `PaymentWebhookService` |
| **Provider reconciliation** | `ReconcileProviderTransactionAsync` | ~120 | Extract → `PaymentReconciliationService` |
| **Payment queries** | `GetContractPaymentsAsync`, `GetMilestonePaymentAsync` | ~100 | Extract → `PaymentQueryService` |
| **Private helpers** | `EnsureFundingAllowedAsync`, `ReplayAsync`, `FailReservationAsync`, mapping, auth checks, idempotency key creation | ~500 | Distribute with callers |

#### MilestoneService (1,380 lines) — 6 distinct responsibilities

| Responsibility | Methods | Approx Lines | Proposed Extraction |
|----------------|---------|-------------|---------------------|
| **Draft CRUD** | `AddAsync`, `UpdateDraftAsync`, `ListAsync` | ~160 | Extract → `MilestoneDraftService` |
| **Approval workflow** | `ApproveAsync`, `MarkReadyForFundingAsync` | ~170 | Keep in `MilestoneService` |
| **Submission & review** | `SubmitAsync`, `AcceptAsync`, `RequestChangesAsync` | ~310 | Extract → `MilestoneReviewService` |
| **Change requests** | `CreateChangeRequestAsync`, `ApproveChangeRequestAsync`, `RejectChangeRequestAsync`, `CancelChangeRequestAsync` | ~200 | Extract → `MilestoneChangeRequestService` |
| **Guard helpers** | `EnsureParticipant`, `EnsureDraft`, `EnsureFundedWorkCanBeChanged`, `EnsureActualExtension`, `EnsureExpectedVersion` × 2, etc. | ~200 | Keep as static helpers or share via domain |
| **Mapping & queries** | `MapMilestone`, `GetFundingStatus`, `GetPermittedActions`, `IsCurrentSequentialMilestoneAsync` | ~180 | Extract mapping; keep queries |

#### ContractService (976 lines) — 5 distinct responsibilities

| Responsibility | Methods | Approx Lines | Proposed Extraction |
|----------------|---------|-------------|---------------------|
| **Draft CRUD** | `CreateAsync`, `UpdateDraftAsync` | ~150 | Keep |
| **Listing & queries** | `ListAsync`, `GetAsync`, `GetStateHistoryAsync`, `MapDetailAsync` | ~200 | Extract → `ContractQueryService` |
| **Acceptance & activation** | `AcceptAsync`, `TryActivateAsync`, `EvaluateActivationAsync` | ~120 | Keep |
| **Completion & termination** | `EvaluateCompletionAsync`, `TerminateAsync` | ~200 | Keep (termination is complex) |
| **Private helpers** | `GetAuthorizedContractAsync`, `GetContractForMutationAsync`, `EnsureExpectedVersion`, `ParseIfMatch`, mapping, etc. | ~300 | Distribute |

#### WalletService (630 lines) — 3 distinct responsibilities

| Responsibility | Methods | Approx Lines | Proposed Extraction |
|----------------|---------|-------------|---------------------|
| **Wallet query** | `GetAsync` | ~25 | Keep |
| **Withdrawal flow** | `WithdrawAsync`, `ReserveBalanceAsync`, `CompleteAsync`, `FailAndReleaseReservationAsync`, `KeepProcessingAsync`, `ReplayAsync` | ~400 | Keep (tightly coupled) |
| **Withdrawal reconciliation** | `ReconcilePendingWithdrawalsAsync` | ~130 | Extract → `WalletReconciliationService` |

---

## 3. Provider & Background Job Analysis

### 3.1 MockPaymentProvider

**File**: [MockPaymentProvider.cs](file:///p:/Projects/Smart%20Court/SmartCourt/Providers/Payments/MockPaymentProvider.cs) (216 lines)

| Aspect | Finding |
|--------|---------|
| **Interface coverage** | ✅ Implements both `IPaymentProvider` and `IPaymentReconciliationProvider` |
| **Behavior control** | ✅ Uses `behaviorReference` prefix (`mock-success`, `mock-fail`, `mock-timeout`) to deterministically control outcomes |
| **Idempotency** | ✅ `ConcurrentDictionary` caches results by `{operation}:{providerIdempotencyKey}` — same key always returns same result |
| **Reconciliation support** | ✅ `GetDepositStatusAsync` looks up the cached result from the original deposit call |
| **Deterministic IDs** | ✅ `CreateDeterministicId` uses SHA256 so provider transaction IDs are stable across retries |
| **Missing operations** | ⚠️ `ReleaseAsync` defaults to `mock-success-release` unless the key starts with `mock-` — fine for testing but doesn't exercise failure paths automatically |

> [!TIP]
> The mock is well-designed for integration tests. Consider adding a `mock-partial-release` behavior path for testing partial-failure scenarios in `EscrowReleaseService`.

### 3.2 Hangfire Job Pipeline

**Architecture**: `ContractRecurringJobRegistrar` → `IContractJobService` → `IContractJobOperations` → actual services

| Component | File | Verdict |
|-----------|------|---------|
| [ContractRecurringJobRegistrar](file:///p:/Projects/Smart%20Court/SmartCourt/Providers/Jobs/ContractRecurringJobRegistrar.cs) | 41L | ✅ Clean. Registers 3 recurring jobs: outbox dispatch (1 min), schedule reconciliation (5 min), wallet reconciliation (5 min) |
| [HangfireContractJobScheduler](file:///p:/Projects/Smart%20Court/SmartCourt/Providers/Jobs/HangfireContractJobScheduler.cs) | 146L | ✅ Clean. 7 scheduling methods with UTC validation |
| [ContractJobService](file:///p:/Projects/Smart%20Court/SmartCourt/Features/Payments/ContractJobService.cs) | 103L | ✅ Clean. Thin dispatcher to `IContractJobOperations` + `IOutboxDispatcher` + `IMilestoneSchedulingReconciliationService` |
| [PaymentContractJobOperations](file:///p:/Projects/Smart%20Court/SmartCourt/Features/Payments/PaymentContractJobOperations.cs) | 64L | ⚠️ `RetryProviderTransactionAsync` delegates to `ReconcileProviderTransactionAsync` — identical behavior. See [Finding F-4](#f-4) |

### 3.3 Outbox Pipeline

| Component | File | Verdict |
|-----------|------|---------|
| [OutboxWriter](file:///p:/Projects/Smart%20Court/SmartCourt/Infrastructure/Providers/Events/OutboxWriter.cs) | 79L | ✅ Validates payload, serializes, adds to `DbContext` (participates in caller's transaction) |
| [OutboxDispatcher](file:///p:/Projects/Smart%20Court/SmartCourt/Infrastructure/Providers/Events/OutboxDispatcher.cs) | 184L | ✅ Lease-based claiming with Serializable isolation. Exponential backoff on failure. Handles concurrent claims gracefully |
| [MilestoneSchedulingOutboxHandler](file:///p:/Projects/Smart%20Court/SmartCourt/Features/Milestones/Events/MilestoneSchedulingOutboxHandler.cs) | 144L | ✅ Correctly schedules auto-accept on `MilestoneSubmitted` and hold release on `MilestoneAccepted`/`MilestoneAutoAccepted` |

### 3.4 Race Condition Assessment: Background Reconciliation vs. Webhooks

> [!IMPORTANT]
> **No critical race conditions were found**, but there is a theoretical window worth documenting.

**Scenario**: Webhook arrives while reconciliation job is running for the same `PaymentTransaction`.

**Why it's safe**:
1. Both paths check `paymentTransaction.Status != PaymentTransactionStatus.Processing` before proceeding
2. `CompleteFundingAsync` uses `DbUpdateException` catch to handle the case where escrow hold/ledger was already created
3. The webhook path additionally checks `WebhookEventExistsAsync` to return `Duplicate` if the event was already processed
4. The `MilestoneTransitionGuard.EnsureCanTransition` will throw if the milestone already moved past `FundingProcessing`

**Residual risk**: If both paths pass the `Processing` check simultaneously, the first `SaveChangesAsync` winner creates the hold; the loser gets a `DbUpdateException` and the webhook path returns "Duplicate". This is correct behavior.

### 3.5 Scheduling Reconciliation Service

[MilestoneSchedulingReconciliationService](file:///p:/Projects/Smart%20Court/SmartCourt/Features/Milestones/MilestoneSchedulingReconciliationService.cs) (121 lines)

✅ Correctly discovers milestones in `Submitted` status with `AutoAcceptJobId == null` and reschedules them. Also discovers `AcceptedHold` milestones with expired holds and schedules release jobs. Batch-limited to 100.

---

## 4. Illogical & Over-Engineered Code Findings

### F-1: Duplicated `GetFundingStatus` Logic
<a id="f-1"></a>

**Location**: [ContractService.cs:L729-L741](file:///p:/Projects/Smart%20Court/SmartCourt/Features/Contracts/ContractService.cs#L729-L741) and [MilestoneService.cs:L1004-L1021](file:///p:/Projects/Smart%20Court/SmartCourt/Features/Milestones/MilestoneService.cs#L1004-L1021)

The `MilestoneFundingStatus` derivation logic is **copy-pasted** between `ContractService.MapMilestone` and `MilestoneService.GetFundingStatus`. Both contain the exact same switch expression. This should be consolidated into a single static method in the Milestones Domain folder.

---

### F-2: Duplicated `EnsureParticipant` Guard
<a id="f-2"></a>

**Location**: [ContractService.cs:L840-L850](file:///p:/Projects/Smart%20Court/SmartCourt/Features/Contracts/ContractService.cs#L840-L850) and [MilestoneService.cs:L1067-L1077](file:///p:/Projects/Smart%20Court/SmartCourt/Features/Milestones/MilestoneService.cs#L1067-L1077)

Both services have an identical `EnsureParticipant(contract, actorUserId)` static method. This should live in the Contracts Domain folder as a reusable guard.

---

### F-3: Duplicated `ParseIfMatch` / `EnsureExpectedVersion`
<a id="f-3"></a>

**Location**: [ContractService.cs:L792-L838](file:///p:/Projects/Smart%20Court/SmartCourt/Features/Contracts/ContractService.cs#L792-L838), [MilestoneService.cs:L1201-L1239](file:///p:/Projects/Smart%20Court/SmartCourt/Features/Milestones/MilestoneService.cs#L1201-L1239)

The `ParseIfMatch` method is duplicated verbatim. `EnsureExpectedVersion` follows the same pattern with minor variations for the entity type. This belongs in a shared concurrency helper (e.g., `Common/ConcurrencyGuard.cs`).

---

### F-4: `RetryProviderTransactionAsync` Delegates to Reconciliation
<a id="f-4"></a>

**Location**: [PaymentContractJobOperations.cs:L45-L53](file:///p:/Projects/Smart%20Court/SmartCourt/Features/Payments/PaymentContractJobOperations.cs#L45-L53)

```csharp
public async Task<JobExecutionResult> RetryProviderTransactionAsync(...)
    => await paymentEscrowService.ReconcileProviderTransactionAsync(...);
```

`RetryProviderTransactionAsync` calls the exact same method as `ReconcileProviderTransactionAsync`. If the intent is different (retry = re-call provider, reconcile = check status), the behavior should differ. If they are intentionally the same, the interface should expose only one method.

---

### F-5: Duplicated `GetActorUserId` in Every Service
<a id="f-5"></a>

**Location**: Every service — `ContractService`, `MilestoneService`, `PaymentEscrowService`, `WalletService`.

Each service has its own identical `GetActorUserId()` method checking `!currentUserService.IsAuthenticated || !currentUserService.UserId.HasValue || currentUserService.UserId.Value == Guid.Empty`. This should be a single method on `ICurrentUserService` itself (e.g., `RequireUserId()`).

---

### F-6: Duplicated `ProviderResultMatches` Across Services
<a id="f-6"></a>

**Location**: [PaymentEscrowService.cs:L1631-L1646](file:///p:/Projects/Smart%20Court/SmartCourt/Features/Payments/PaymentEscrowService.cs#L1631-L1646), [WalletService.cs:L599-L614](file:///p:/Projects/Smart%20Court/SmartCourt/Features/Payments/WalletService.cs#L599-L614), [EscrowReleaseService.cs:L489-L504](file:///p:/Projects/Smart%20Court/SmartCourt/Features/Payments/EscrowReleaseService.cs#L489-L504)

Three near-identical `ProviderResultMatches` methods exist. They all compare `Amount`, `Currency`, `BusinessId`, `ProviderIdempotencyKey`, and `CorrelationId`. This should be a single static method on `ProviderResult` or in a shared `PaymentProviderResultValidator`.

---

### F-7: Double Query for Sequential Milestone Check
<a id="f-7"></a>

**Location**: [PaymentEscrowService.cs:L1366-L1406](file:///p:/Projects/Smart%20Court/SmartCourt/Features/Payments/PaymentEscrowService.cs#L1366-L1406)

`EnsureFundingAllowedAsync` fires **three** separate DB queries:
1. `hasUnsettledEarlierMilestone` — milestones with lower order that aren't terminal
2. `hasOtherActiveMilestone` — milestones in various active states
3. `hasOtherUnsettledHold` — escrow holds that are Funded or Frozen

Queries 2 and 3 overlap conceptually. The first query already establishes the sequential constraint. The overlap between query 2 and query 3 is that any `FundedInProgress` milestone will always have a `Funded` hold. These could be consolidated into **two** queries at most.

---

### F-8: Redundant Transition Guard Calls in `PaymentEscrowService`
<a id="f-8"></a>

**Location**: [PaymentEscrowService.cs:L1544-L1546](file:///p:/Projects/Smart%20Court/SmartCourt/Features/Payments/PaymentEscrowService.cs#L1544-L1546)

The `AddHistory` private method calls `MilestoneTransitionGuard.EnsureCanTransition` internally, but the callers of `AddHistory` (e.g., `CompleteFundingAsync` at L949) also call `MilestoneTransitionGuard.EnsureCanTransition` just before calling `AddHistory`. This double-validation is harmless but redundant. One call should be authoritative.

---

### F-9: Hardcoded "EGP" Currency Checks
<a id="f-9"></a>

**Location**: Multiple files — [MilestoneService.cs:L364-L366](file:///p:/Projects/Smart%20Court/SmartCourt/Features/Milestones/MilestoneService.cs#L364-L366), [PaymentEscrowService.cs:L1673](file:///p:/Projects/Smart%20Court/SmartCourt/Features/Payments/PaymentEscrowService.cs#L1673), [EscrowReleaseService.cs:L458-L464](file:///p:/Projects/Smart%20Court/SmartCourt/Features/Payments/EscrowReleaseService.cs#L458-L464)

The string `"EGP"` is compared in at least 8 places without a shared constant. A single `Currency.EGP` constant in the Payments domain would prevent silent typos and make future multi-currency support more tractable.

---

### F-10: Redundant `Guid.Empty` Validation After FluentValidation
<a id="f-10"></a>

**Location**: Multiple — `GetMilestoneForMutationAsync` checks `milestoneId == Guid.Empty`, but FluentValidation on the request DTO should already reject empty GUIDs before they reach the service. This is defense-in-depth but adds dead-code paths that never fire in production.

> [!NOTE]
> F-10 is acceptable as defense-in-depth. The other findings (F-1 through F-9) represent genuine code quality improvements.

---

## 5. Proposed Class Deconstruction Blueprint

### 5.1 Payments Slice — `PaymentEscrowService` → 4 Services

#### [NEW] `IPaymentWebhookService` / `PaymentWebhookService`
- **Responsibilities**: `HandleWebhookAsync`, `ValidateWebhookAuthentication`, `EnsureWebhookMatchesTransaction`, `RecordTerminalWebhookAsync`, `FinalizeFailedExternalResultAsync`
- **Dependencies**: `ApplicationDbContext`, `IPaymentEscrowFundingService` (for `CompleteFundingAsync`), `IOptions<PaymentProviderOptions>`, `ILogger`, `TimeProvider`
- **Estimated size**: ~350 lines

#### [NEW] `IPaymentReconciliationService` / `PaymentReconciliationService`
- **Responsibilities**: `ReconcileProviderTransactionAsync`
- **Dependencies**: `ApplicationDbContext`, `IPaymentReconciliationProvider`, `IPaymentEscrowFundingService`, `ILogger`, `TimeProvider`
- **Estimated size**: ~150 lines

#### [NEW] `IPaymentQueryService` / `PaymentQueryService`
- **Responsibilities**: `GetContractPaymentsAsync`, `GetMilestonePaymentAsync`
- **Dependencies**: `ApplicationDbContext`, `ICurrentUserService`, `IContractUserEligibilityService`
- **Estimated size**: ~120 lines

#### [MODIFY] `IPaymentEscrowService` / `PaymentEscrowService` (retains core funding)
- **Responsibilities**: `FundAsync`, `RetryAsync`, `CompleteFundingAsync`, `FailFundingAsync`, `EnsureFundingAllowedAsync`, idempotency helpers
- **Estimated size**: ~400 lines

**Registration** in `DependencyInjection.cs`:
```csharp
services.AddScoped<IPaymentEscrowService, PaymentEscrowService>();
services.AddScoped<IPaymentWebhookService, PaymentWebhookService>();
services.AddScoped<IPaymentReconciliationService, PaymentReconciliationService>();
services.AddScoped<IPaymentQueryService, PaymentQueryService>();
```

---

### 5.2 Milestones Slice — `MilestoneService` → 3 Services

#### [NEW] `IMilestoneDraftService` / `MilestoneDraftService`
- **Responsibilities**: `AddAsync`, `UpdateDraftAsync`, `ListAsync`
- **Dependencies**: `ApplicationDbContext`, `ICurrentUserService`, `IContractService`, `TimeProvider`
- **Estimated size**: ~200 lines

#### [NEW] `IMilestoneChangeRequestService` / `MilestoneChangeRequestService`
- **Responsibilities**: `CreateChangeRequestAsync`, `ApproveChangeRequestAsync`, `RejectChangeRequestAsync`, `CancelChangeRequestAsync`
- **Dependencies**: `ApplicationDbContext`, `ICurrentUserService`, `IContractService`, `IOutboxWriter`, `TimeProvider`
- **Estimated size**: ~250 lines

#### [MODIFY] `IMilestoneService` / `MilestoneService` (retains lifecycle operations)
- **Responsibilities**: `ApproveAsync`, `MarkReadyForFundingAsync`, `SubmitAsync`, `AcceptAsync`, `RequestChangesAsync`
- **Estimated size**: ~400 lines

**Registration** in `DependencyInjection.cs`:
```csharp
services.AddScoped<IMilestoneDraftService, MilestoneDraftService>();
services.AddScoped<IMilestoneService, MilestoneService>();
services.AddScoped<IMilestoneChangeRequestService, MilestoneChangeRequestService>();
```

---

### 5.3 Contracts Slice — `ContractService` → 2 Services

#### [NEW] `IContractQueryService` / `ContractQueryService`
- **Responsibilities**: `ListAsync`, `GetAsync`, `GetStateHistoryAsync`, `MapDetailAsync`
- **Dependencies**: `ApplicationDbContext`, `ICurrentUserService`, `IContractUserEligibilityService`
- **Estimated size**: ~250 lines

#### [MODIFY] `IContractService` / `ContractService`
- **Responsibilities**: `CreateAsync`, `UpdateDraftAsync`, `AcceptAsync`, `EvaluateActivationAsync`, `EvaluateCompletionAsync`, `TerminateAsync`
- **Estimated size**: ~400 lines

**Registration**:
```csharp
services.AddScoped<IContractService, ContractService>();
services.AddScoped<IContractQueryService, ContractQueryService>();
```

---

### 5.4 Wallet — `WalletService` → 2 Services

#### [NEW] `IWalletReconciliationService` / `WalletReconciliationService`
- **Responsibilities**: `ReconcilePendingWithdrawalsAsync`
- **Dependencies**: `ApplicationDbContext`, `IPaymentProvider`, `IIdempotencyService`, `ILogger`, `TimeProvider`
- **Estimated size**: ~170 lines

#### [MODIFY] `IWalletService` / `WalletService`
- **Responsibilities**: `GetAsync`, `WithdrawAsync`
- **Estimated size**: ~350 lines

---

### 5.5 Shared Extractions (Cross-Slice Domain Helpers)

| Proposed File | Content | Current Duplications |
|---------------|---------|---------------------|
| `Common/Domain/ConcurrencyGuard.cs` | `ParseIfMatch(string)`, `EnsureExpectedVersion<T>(DbContext, T, byte[], string)` | F-3 |
| `Common/Domain/ParticipantGuard.cs` | `EnsureParticipant(clientId, lawyerId, actorId)` | F-2 |
| `Payments/Domain/ProviderResultValidator.cs` | `Matches(ProviderResult, PaymentProviderRequest)` | F-6 |
| `Payments/Domain/CurrencyConstants.cs` | `const string EGP = "EGP"` | F-9 |
| Extension on `ICurrentUserService` | `RequireUserId()` | F-5 |
| `Milestones/Domain/MilestoneFundingStatusResolver.cs` | `GetFundingStatus(MilestoneStatus, EscrowHold?)` | F-1 |

---

## 6. Zero-Regression Refactoring Roadmap

> [!IMPORTANT]
> Each phase is designed so that **all 526 tests pass at every commit boundary**. No phase changes external API contracts or endpoint signatures.

### Phase 0: Baseline Verification
- Run full test suite: `dotnet test` — confirm 526 pass
- Record baseline code coverage for the 4 target slices

### Phase 1: Extract Shared Domain Helpers (No Behavior Change)
**Risk**: Lowest · **Duration**: ~2 hours

1. Create `Common/Domain/ConcurrencyGuard.cs` with `ParseIfMatch` and generic `EnsureExpectedVersion`
2. Create `Payments/Domain/CurrencyConstants.cs` with `const string EGP`
3. Create `Payments/Domain/ProviderResultValidator.cs`
4. Create `Milestones/Domain/MilestoneFundingStatusResolver.cs`
5. Add `RequireUserId()` extension to `ICurrentUserService`
6. Update all callers to use the shared implementations
7. ✅ Run tests — all 526 must pass

### Phase 2: Extract `PaymentQueryService` + `PaymentWebhookService`
**Risk**: Low · **Duration**: ~3 hours

1. Create `IPaymentQueryService` interface with `GetContractPaymentsAsync`, `GetMilestonePaymentAsync`
2. Move implementations from `PaymentEscrowService`
3. Create `IPaymentWebhookService` interface with `HandleWebhookAsync`
4. Move webhook methods from `PaymentEscrowService`
5. Update `PaymentsController` to inject both new services alongside `IPaymentEscrowService`
6. Register in `DependencyInjection.cs`
7. ✅ Run tests — all 526 must pass

### Phase 3: Extract `PaymentReconciliationService`
**Risk**: Low · **Duration**: ~1 hour

1. Create `IPaymentReconciliationService` with `ReconcileProviderTransactionAsync`
2. Move implementation from `PaymentEscrowService`
3. Update `PaymentContractJobOperations` to inject `IPaymentReconciliationService`
4. ✅ Run tests — all 526 must pass

### Phase 4: Extract `MilestoneDraftService` + `MilestoneChangeRequestService`
**Risk**: Low-Medium · **Duration**: ~3 hours

1. Create `IMilestoneDraftService` interface with `AddAsync`, `UpdateDraftAsync`, `ListAsync`
2. Move implementations from `MilestoneService`, carrying the relevant helpers
3. Create `IMilestoneChangeRequestService` with 4 change request methods
4. Move implementations from `MilestoneService`
5. Update `MilestonesController` to inject the new services
6. Register in `DependencyInjection.cs`
7. ✅ Run tests — all 526 must pass

### Phase 5: Extract `ContractQueryService`
**Risk**: Low · **Duration**: ~2 hours

1. Create `IContractQueryService` with `ListAsync`, `GetAsync`, `GetStateHistoryAsync`
2. Move `MapDetailAsync` and its helpers to the new service
3. Update `ContractsController`
4. ✅ Run tests — all 526 must pass

### Phase 6: Extract `WalletReconciliationService`
**Risk**: Lowest · **Duration**: ~1 hour

1. Create `IWalletReconciliationService` with `ReconcilePendingWithdrawalsAsync`
2. Move implementation from `WalletService`
3. Update `PaymentContractJobOperations` to inject the new interface
4. ✅ Run tests — all 526 must pass

### Phase 7: Remove `RetryProviderTransactionAsync` Dead Code (F-4)
**Risk**: Low · **Duration**: ~30 minutes

1. Decide whether `RetryProviderTransactionAsync` should be distinct from reconciliation
2. If identical, remove from `IContractJobOperations` and `IContractJobService`
3. Remove from `HangfireContractJobScheduler.ScheduleProviderRetryAsync` if unused
4. ✅ Run tests — all 526 must pass

### Phase 8: Final Cleanup
**Duration**: ~1 hour

1. Remove double `MilestoneTransitionGuard.EnsureCanTransition` calls (F-8)
2. Consolidate the 3 overlapping queries in `EnsureFundingAllowedAsync` (F-7) into 2
3. Run full test suite + review code coverage delta
4. ✅ Run tests — all 526 must pass

---

### Post-Refactoring Line Count Projections

| Service (After) | Projected Lines | Δ from Current |
|-----------------|----------------|----------------|
| `PaymentEscrowService` | ~400 | −1,304 |
| `PaymentWebhookService` | ~350 | NEW |
| `PaymentQueryService` | ~120 | NEW |
| `PaymentReconciliationService` | ~150 | NEW |
| `MilestoneService` | ~400 | −980 |
| `MilestoneDraftService` | ~200 | NEW |
| `MilestoneChangeRequestService` | ~250 | NEW |
| `ContractService` | ~400 | −576 |
| `ContractQueryService` | ~250 | NEW |
| `WalletService` | ~350 | −280 |
| `WalletReconciliationService` | ~170 | NEW |

**Total new files**: 7 service classes + 7 interfaces + 6 shared helpers = **20 new files**
**Net line increase**: ~0 (redistribution, plus removal of ~200 lines of duplicated code)
**Maximum single-file length after refactoring**: ≤ 400 lines ✅

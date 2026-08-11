# SmartCourt Notifications Documentation

Status: **in-app backend V1 with Proposal, Contract, Milestone, Payments/Wallet, Administrative Verification, User Verification Submission, and Auth security integrations implemented and verified; Gate 7 stopped for review**

This directory is the documentation entry point for the SmartCourt notification system. The current implementation is backend-only: a durable database inbox, authenticated REST endpoints, SignalR events, and Proposal/Contract/Milestone/Payments/Administrative Verification/User Verification/Auth security lifecycle outbox triggers. No frontend source code, Email delivery, or SMS delivery is included; existing Auth Email/SMS receipts remain unchanged.

## Choose the right document

| Document | Audience | Purpose |
|---|---|---|
| [Architecture Decision](./architecture.md) | Backend leads and reviewers | Explains why SQL + transactional outbox + SignalR was selected and records deferred channel decisions. |
| [Backend Integration Guide](./backend_integration_guide.md) | Backend feature developers | Shows how an owning slice produces a semantic event and how Notifications maps it safely. |
| [Frontend Integration Guide and API Contract](./frontend_integration_guide.md) | Frontend/mobile/API consumers | Defines authentication, REST routes, DTOs, SignalR events, state reconciliation, errors, and security behavior. |
| [Notification Opportunity Catalog](./notification_opportunity_catalog.md) | Product, backend leads, and slice owners | Inventories notification-worthy actions and user stories across every feature slice, including priorities, recipients, event readiness, and exclusions. |
| [Per-Slice Integration Plan](./slice_integration_plan.md) | Backend leads, QA, and reviewers | Defines the reusable mapper pipeline, Arabic-copy policy, per-slice changes, exhaustive HTTP artifacts, and mandatory review gates. |
| [Implementation Plan](./implementation_plan.md) | Maintainers and reviewers | Records delivered files, constraints, test coverage, and verification results. |
| [Notification Core HTTP Test Report](../../../SmartCourt.Tests/HttpTests/Notifications_Report.md) | QA and reviewers | Records the monitored notification inbox lifecycle results. |
| [Contracts Notification HTTP Test Report](../../../SmartCourt.Tests/HttpTests/ContractsNotifications_Report.md) | QA and reviewers | Records the monitored Contract lifecycle and notification results. |
| [Milestones Notification HTTP Test Report](../../../SmartCourt.Tests/HttpTests/MilestonesNotifications_Report.md) | QA and reviewers | Records the monitored Milestone lifecycle, change-request, and automatic-acceptance notification results. |
| [Payments Notification HTTP Test Report](../../../SmartCourt.Tests/HttpTests/PaymentsNotifications_Report.md) | QA and reviewers | Records funding, webhook, settlement, wallet, withdrawal, and notification results. |
| [Administrative Verification Notification HTTP Test Report](../../../SmartCourt.Tests/HttpTests/AdminVerificationNotifications_Report.md) | QA and reviewers | Records verification decision, account transition, concurrency, recipient-isolation, Arabic-copy, and log-monitoring results. |
| [User Verification Notification HTTP Test Report](../../../SmartCourt.Tests/HttpTests/UserVerificationNotifications_Report.md) | QA and reviewers | Records submission, partial upload, replacement, deletion, Admin-only delivery, recipient isolation, Arabic-copy, and log-monitoring results. |
| [Auth Security Notification HTTP Test Report](../../../SmartCourt.Tests/HttpTests/AuthSecurityNotifications_Report.md) | QA and reviewers | Records password-change/reset receipts, Auth boundaries, token revocation, recipient isolation, Arabic-copy, and log-monitoring results. |

## Authoritative contract snapshot

- REST source of truth: `/api/notifications` and its read/count endpoints.
- Real-time endpoint: `/hubs/notifications`.
- Server events: `NotificationCreated`, `NotificationRead`, `NotificationsReadAll`.
- Current Proposal producer events: `ProposalCreated`, `ProposalAccepted`, `ProposalRejected`, version `1`.
- Current Contract producer events: `ContractCreated` V1, `ContractDraftUpdated` V1, `ContractAccepted` V2, `ContractActivated` V1, `ContractCompleted` V1, `ContractTerminationRequested` V1, and `ContractTerminated` V1. Historical `ContractAccepted` V1 is a safe no-op.
- Current Milestone producer events: `MilestoneCreated`, `MilestoneDraftUpdated`, `MilestoneAcceptanceRecorded`, `MilestoneApproved`, `MilestoneReadyForFunding`, `MilestoneSubmitted`, `MilestoneChangesRequested`, `MilestoneAccepted`, `MilestoneAutoAccepted`, and the four `MilestoneChangeRequest*` decision events, all version `1`.
- Current Payments producer events: existing `MilestoneFundingStarted`, `MilestoneFunded`, `MilestoneFundingFailed`, `FundsReleased`, and `FundsRefunded` facts plus `WithdrawalCompleted`, `WithdrawalFailed`, `WithdrawalDelayed`, and `WalletAdjusted`, all version `1`.
- Current Administrative Verification producer events: `VerificationDocumentApproved`, `VerificationDocumentRejected`, `VerificationDocumentExpired`, `VerificationAccountApproved`, `VerificationAccountRejected`, and `VerificationReviewRequested`, all version `1`.
- Current Auth producer events: `PasswordChanged` and `PasswordReset`, both version `1`.
- Current notification types: the three `proposal.*` types, seven `contract.*` types, sixteen `milestone.*` types, two `funds.*` types, four `wallet.*` types, six `verification.*`/`account.*` types, and two `security.*` types documented in the frontend contract.
- Authentication: required; recipient identity always comes from the authenticated principal.
- Delivery semantics: durable REST inbox, best-effort and potentially duplicate SignalR.
- Deferred: Email, SMS, public/direct notification creation, frontend components/store, and distributed SignalR backplane.

## Administrative Verification contract (Gate 5)

Gate 5 maps only committed semantic verification facts. Document decisions go to the authoritative document owner; account outcomes go only to the affected account owner. The mapper persists Arabic title/body snapshots, uses `actionUrl: null`, and exposes only `documentId` plus `documentType` for document events or `userId` for account events. Storage paths, file URLs/content, full rejection reasons, private review comments, contact details, provider identifiers, and idempotency keys are forbidden. Replays are idempotent through the outbox message ID, and repeated account/document decisions do not enqueue a second notification for the same state transition. The expiry-reminder story `VER-07` remains deferred.

## User Verification Submission contract (Gate 6)

The User Verification slice emits `VerificationReviewRequested` V1 inside the same EF unit of work as a successful upload request. One logical event is created when at least one document is persisted; a partial request with one successful document also creates one event, and a multi-file request creates one event with its successful `documentCount`. The Notifications mapper resolves every user with the exact `Admin` role from the authoritative Identity tables. `SuperAdministrator` and ordinary users receive no row. Each Admin receives one inbox row per committed outbox message, with replay deduplicated by the outbox message ID.

The persisted notification is `verification.review-requested` with `Information` severity, Arabic title `طلب مراجعة مستندات التحقق`, Arabic body `تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.`, `actionUrl: null`, and data keys `userId` and `documentCount`. Storage paths, file URLs/content, file names, private metadata, rejection reasons, contact details, provider IDs, tokens, and idempotency keys are forbidden. REST remains the durable source of truth and SignalR broadcasts the persisted DTO best-effort; clients reconcile by notification ID. `VER-08` remains intentionally without a notification.

Gate 6 is verified by `SmartCourt.Tests/Features/Notifications/VerificationNotificationEventMapperTests.cs` and [`UserVerificationNotifications_Report.md`](../../../SmartCourt.Tests/HttpTests/UserVerificationNotifications_Report.md). The monitored HTTP artifact records `155 passed, 0 failed, 1 documented skip` and covers every UserVerification route, causal upload-to-notification sequencing, partial and multi-file submissions, failed-only no-event behavior, replacement versions, deletion, ownership boundaries, exact Arabic snapshots, forbidden metadata, Admin-only recipient isolation, mock Email confirmation, clean API/outbox/provider logs, and released test port.

## Auth security contract (Gate 7)

The Auth slice emits `PasswordChanged` V1 only after a successful authenticated password change has revoked active refresh tokens, and `PasswordReset` V1 only after a successful reset-token password reset has revoked active refresh tokens. Both events are committed in the same existing EF transaction as the password/session mutation. Failed validation, failed password operations, ordinary registration/login/refresh/logout/revoke/phone challenge actions, and failed or replayed reset tokens create no security notification.

Both events map to the account owner only. The persisted types are `security.password-changed` and `security.password-reset`, both `Critical`, with `actionUrl: null`, data containing only `userId`, and these exact Arabic snapshots:

| Type | Arabic title | Arabic body |
|---|---|---|
| `security.password-changed` | `تم تغيير كلمة المرور` | `تم تغيير كلمة مرور حسابك بنجاح. إذا لم تكن أنت من أجرى هذا التغيير، يرجى تأمين حسابك والتواصل مع الدعم.` |
| `security.password-reset` | `تمت إعادة تعيين كلمة المرور` | `تمت إعادة تعيين كلمة مرور حسابك بنجاح. إذا لم تطلب هذا الإجراء، يرجى تأمين حسابك والتواصل مع الدعم.` |

Notification data never contains Email addresses, passwords or hints, reset/access/refresh tokens, IP addresses, device fingerprints, security stamps, reset URLs, provider IDs, or idempotency keys. Existing Email security receipts remain supplementary and are not replaced. Outbox-message replay is idempotent; REST is durable and SignalR is best-effort.

Gate 7 is verified by `SmartCourt.Tests/Features/Notifications/AuthNotificationEventMapperTests.cs`, Auth service tests, and [`AuthSecurityNotifications_Report.md`](../../../SmartCourt.Tests/HttpTests/AuthSecurityNotifications_Report.md), which records `117 passed, 0 failed, 1 documented skip`. The HTTP artifact proves successful action responses are followed by exactly one persisted notification, failed actions create none, revoked sessions cannot read the inbox, reset replay is rejected, unrelated users receive nothing, exact Arabic snapshots and forbidden fields are enforced, mock Email links are extracted from `api_log.txt`, and API/outbox/provider logs are clean with the test port released.

When documentation and code appear to disagree, the implemented controller/DTO/hub contracts under `SmartCourt/Features/Notifications` are the runtime authority; update these documents in the same change as any contract modification.

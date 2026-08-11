# SmartCourt Notifications Documentation

Status: **in-app backend V1 with Proposal, Contract, Milestone, and Payments/Wallet integrations implemented and verified**

This directory is the documentation entry point for the SmartCourt notification system. The current implementation is backend-only: a durable database inbox, authenticated REST endpoints, SignalR events, and Proposal/Contract/Milestone/Payments lifecycle outbox triggers. No frontend source code, Email delivery, or SMS delivery is included.

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

## Authoritative contract snapshot

- REST source of truth: `/api/notifications` and its read/count endpoints.
- Real-time endpoint: `/hubs/notifications`.
- Server events: `NotificationCreated`, `NotificationRead`, `NotificationsReadAll`.
- Current Proposal producer events: `ProposalCreated`, `ProposalAccepted`, `ProposalRejected`, version `1`.
- Current Contract producer events: `ContractCreated` V1, `ContractDraftUpdated` V1, `ContractAccepted` V2, `ContractActivated` V1, `ContractCompleted` V1, `ContractTerminationRequested` V1, and `ContractTerminated` V1. Historical `ContractAccepted` V1 is a safe no-op.
- Current Milestone producer events: `MilestoneCreated`, `MilestoneDraftUpdated`, `MilestoneAcceptanceRecorded`, `MilestoneApproved`, `MilestoneReadyForFunding`, `MilestoneSubmitted`, `MilestoneChangesRequested`, `MilestoneAccepted`, `MilestoneAutoAccepted`, and the four `MilestoneChangeRequest*` decision events, all version `1`.
- Current Payments producer events: existing `MilestoneFundingStarted`, `MilestoneFunded`, `MilestoneFundingFailed`, `FundsReleased`, and `FundsRefunded` facts plus `WithdrawalCompleted`, `WithdrawalFailed`, `WithdrawalDelayed`, and `WalletAdjusted`, all version `1`.
- Current notification types: the three `proposal.*` types, seven `contract.*` types, sixteen `milestone.*` types, two `funds.*` types, and four `wallet.*` types documented in the frontend contract.
- Authentication: required; recipient identity always comes from the authenticated principal.
- Delivery semantics: durable REST inbox, best-effort and potentially duplicate SignalR.
- Deferred: Email, SMS, public/direct notification creation, frontend components/store, and distributed SignalR backplane.

When documentation and code appear to disagree, the implemented controller/DTO/hub contracts under `SmartCourt/Features/Notifications` are the runtime authority; update these documents in the same change as any contract modification.

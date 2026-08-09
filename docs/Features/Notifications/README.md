# SmartCourt Notifications Documentation

Status: **in-app backend V1 implemented and verified**

This directory is the documentation entry point for the SmartCourt notification system. The current implementation is backend-only: a durable database inbox, authenticated REST endpoints, SignalR events, and proposal-lifecycle outbox triggers. No frontend source code, Email delivery, or SMS delivery is included.

## Choose the right document

| Document | Audience | Purpose |
|---|---|---|
| [Architecture Decision](./architecture.md) | Backend leads and reviewers | Explains why SQL + transactional outbox + SignalR was selected and records deferred channel decisions. |
| [Backend Integration Guide](./backend_integration_guide.md) | Backend feature developers | Shows how an owning slice produces a semantic event and how Notifications maps it safely. |
| [Frontend Integration Guide and API Contract](./frontend_integration_guide.md) | Frontend/mobile/API consumers | Defines authentication, REST routes, DTOs, SignalR events, state reconciliation, errors, and security behavior. |
| [Implementation Plan](./implementation_plan.md) | Maintainers and reviewers | Records delivered files, constraints, test coverage, and verification results. |
| [HTTP Test Report](../../../SmartCourt.Tests/HttpTests/Notifications_Report.md) | QA and reviewers | Records the monitored end-to-end HTTP lifecycle results. |

## Authoritative contract snapshot

- REST source of truth: `/api/notifications` and its read/count endpoints.
- Real-time endpoint: `/hubs/notifications`.
- Server events: `NotificationCreated`, `NotificationRead`, `NotificationsReadAll`.
- Current producer events: `ProposalCreated`, `ProposalAccepted`, `ProposalRejected`, version `1`.
- Current notification types: `proposal.created`, `proposal.accepted`, `proposal.rejected`.
- Authentication: required; recipient identity always comes from the authenticated principal.
- Delivery semantics: durable REST inbox, best-effort and potentially duplicate SignalR.
- Deferred: Email, SMS, public/direct notification creation, frontend components/store, and distributed SignalR backplane.

When documentation and code appear to disagree, the implemented controller/DTO/hub contracts under `SmartCourt/Features/Notifications` are the runtime authority; update these documents in the same change as any contract modification.

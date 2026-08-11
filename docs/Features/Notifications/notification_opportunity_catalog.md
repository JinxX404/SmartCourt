# Notification Opportunity Catalog

Status: **analysis and product backlog; not an implementation plan**
Reviewed against the SmartCourt backend on: **2026-08-09**

This document inventories actions across every backend feature slice that should, could, or should not create a notification. It is the product/engineering input for a later integration plan. Only the three Proposal notifications marked `Implemented` exist today.

## How to read the catalog

### Priority

| Priority | Meaning |
|---|---|
| `P0` | Already implemented and verified. |
| `P1` | Needed: actionable, financial, account/security, dispute, or deadline-sensitive. |
| `P2` | Recommended: useful cross-user lifecycle awareness. |
| `P3` | Optional/conditional: add only when the prerequisite product capability exists or product explicitly requests it. |
| `None` | Do not create an inbox notification for the current synchronous action. |

### Integration readiness

| Readiness | Meaning |
|---|---|
| `Implemented` | Notifications already consumes the event. |
| `Existing event` | A durable outbox event already exists and normally contains enough resource identity for a handler to load recipients. |
| `Enrich event` | An event exists, but its payload needs actor/recipient/context data to produce the correct message reliably. |
| `New event` | The owning slice needs a new semantic outbox event in the business transaction. |
| `Scheduled trigger` | Requires a durable time-based job/reminder plus an idempotent event. |
| `Prerequisite` | Do not implement until the named missing capability exists. |
| `No integration` | Intentionally no notification work. |

### Future channel notation

- **In-app** means persist to the current Notifications inbox and broadcast SignalR.
- **Email fallback** means a later unread escalation; it is not implemented today.
- **SMS fallback** is reserved for a small number of critical deadlines/security/financial actions.
- **Email immediate** is a security/account channel, not an unread fallback.

Channel suggestions are product recommendations, not implemented behavior.

## Decision rules applied across all slices

1. Notify a person about a meaningful state change performed by another actor or the system.
2. Do not notify the initiating actor merely to repeat a successful synchronous HTTP response, except for durable financial/security receipts.
3. Use one notification per recipient and business fact. Do not duplicate the same fact through Contract, Chat system-message, and Payment mappings.
4. Prefer existing semantic outbox events. Add a new event only when the existing event cannot identify the business fact safely.
5. Never put secrets, access tokens, document contents, payment destination details, evidence text, or unrestricted rejection/termination text into notification metadata.
6. Reasons may be summarized safely in the body, but the complete reason belongs behind the authorized resource endpoint.
7. A notification/action target never grants access. The destination slice must continue enforcing ownership/role authorization.
8. Role/queue notifications must not blindly create one row for every administrator. Use an assigned user, an operational queue, or an explicit bounded recipient policy.
9. Automatic/recovery transitions generate the same semantic event as the normal successful transition. Do not notify on every retry attempt.
10. SignalR delivery is not proof of reading; future Email/SMS fallback is based on persisted unread/deadline state.

## Recommended type conventions

- Use lower-case dotted names: `<aggregate>.<fact>`.
- Use past-tense facts for completed transitions, such as `contract.activated`.
- Use action-oriented facts when the recipient must act, such as `milestone.ready-for-funding`.
- Keep one stable meaning per type. Add a new type rather than changing an established meaning.
- Current severity values are `Information`, `Success`, `Warning`, and `Critical`.

## 1. Proposals

Repository actions: create, accept, reject, list, and get.

| ID | When this happens | Actor → recipient | User story / message intent | Type | Priority / severity | Readiness | Suggested channel |
|---|---|---|---|---|---|---|---|
| `PRP-01` | A client creates a proposal for a lawyer. | Client → Lawyer | As the lawyer, I want to know a client sent a proposal so I can review it. | `proposal.created` | `P0 / Information` | `Implemented` from `ProposalCreated` | In-app |
| `PRP-02` | A lawyer accepts a proposal. | Lawyer → Client | As the client, I want to know my proposal was accepted so I can continue to the contract. | `proposal.accepted` | `P0 / Success` | `Implemented` from `ProposalAccepted` | In-app |
| `PRP-03` | A lawyer rejects a proposal. | Lawyer → Client | As the client, I want to know my proposal was rejected so I can review the outcome and choose another lawyer. | `proposal.rejected` | `P0 / Warning` | `Implemented` from `ProposalRejected` | In-app |

No additional Proposal action currently requires a notification. Read/list actions never notify.

## 2. Contracts

Repository actions and automatic transitions: create draft, update draft, accept current version, activate after both participants and an approved milestone, complete after final settlement, request/finalize termination, and recover pending termination settlement.

| ID | When this happens | Actor → recipient | User story / message intent | Type | Priority / severity | Readiness | Suggested channel |
|---|---|---|---|---|---|---|---|
| `CON-01` | A lawyer creates a contract draft from an accepted proposal. | Lawyer → Client | As the client, I want to know a contract draft is ready so I can review its terms. | `contract.created` | `P1 / Information` | **Implemented** from `ContractCreated` V1 | In-app; Email deferred |
| `CON-02` | The lawyer updates the contract draft and prior acceptances are cleared. | Lawyer → Client | As the client, I want to know the terms changed and my previous acceptance no longer applies so I can review the new version. | `contract.draft-updated` | `P1 / Warning` | **Implemented** from `ContractDraftUpdated` V1 | In-app; Email deferred |
| `CON-03` | One participant accepts the current draft while the other has not. | Client/Lawyer → Other participant | As the other participant, I want to know the current version was accepted and is waiting for my acceptance. | `contract.acceptance-recorded` | `P2 / Information` | **Implemented** from actor-aware `ContractAccepted` V2 | In-app |
| `CON-04` | Both participants and at least one priced milestone satisfy activation requirements. | System/last approver → Client and Lawyer | As either participant, I want confirmation that the contract is active so work and funding can begin. | `contract.activated` | `P1 / Success` | **Implemented** from `ContractActivated` V1 | In-app; Email deferred |
| `CON-05` | All approved milestones and settlements finish and the contract completes. | System → Client and Lawyer | As either participant, I want a durable record that the contract completed successfully. | `contract.completed` | `P2 / Success` | **Implemented** from `ContractCompleted` V1 | In-app |
| `CON-06` | A participant requests termination but financial settlement is still pending. | Client/Lawyer → Other participant; requester confirmation | As the counterparty, I want immediate notice that termination was requested; as the requester, I want to know settlement is still processing. | `contract.termination-requested` | `P1 / Warning` | **Implemented** from `ContractTerminationRequested` V1 | In-app; Email deferred |
| `CON-07` | Termination and required settlement finish, including recovery-job completion. | Participant/System → Client and Lawyer | As either participant, I want to know the contract is terminated and settlement is finalized. | `contract.terminated` | `P1 / Warning` | **Implemented** from `ContractTerminated` V1 | In-app; Email deferred |
| `CON-08` | Termination remains pending beyond an operational threshold. | System → Requester and Finance/Super Admin queue | As the requester, I want to know settlement needs more time; as operations, I need an actionable stuck-settlement alert. | `contract.termination-delayed` | `P1 / Warning` | `Scheduled trigger`; threshold/config required | In-app → Email fallback; operational alert |

Draft GET/list/state-history queries do not notify. A successful update should not notify the lawyer who made it.

## 3. Milestone negotiation and execution

Repository actions and jobs: add/update draft, participant approval, ready-for-funding, funding, submit work, manual/automatic acceptance, request changes, change-request decisions, hold expiry, release/refund, and scheduling reconciliation.

Implementation status: `MIL-01`–`MIL-09`, `MIL-11`–`MIL-13`, and `MIL-15`–`MIL-20` are implemented and verified across the Milestones and Payments gates. Scheduled reminders `MIL-10` and `MIL-14` remain deferred.

### Draft and approval

| ID | When this happens | Actor → recipient | User story / message intent | Type | Priority / severity | Readiness | Suggested channel |
|---|---|---|---|---|---|---|---|
| `MIL-01` | Either participant adds a milestone draft. | Client/Lawyer → Other participant | As the other participant, I want to know a milestone was proposed so I can review it. | `milestone.created` | `P2 / Information` | **Implemented:** `MilestoneCreated` | In-app |
| `MIL-02` | Either participant edits a milestone draft and both approvals reset. | Client/Lawyer → Other participant | As the other participant, I want to know the milestone terms changed and require fresh approval. | `milestone.draft-updated` | `P1 / Warning` | **Implemented:** `MilestoneDraftUpdated` | In-app; Email fallback deferred |
| `MIL-03` | One participant approves the current milestone version. | Client/Lawyer → Other participant | As the other participant, I want to know the milestone is waiting for my approval. | `milestone.acceptance-recorded` | `P2 / Information` | **Implemented:** actor-aware `MilestoneAcceptanceRecorded` | In-app |
| `MIL-04` | Both participants approve a milestone and it moves to awaiting funding. | Last approver/System → Client and Lawyer | As the participants, I want confirmation that milestone terms are approved and the funding workflow can continue. | `milestone.approved` | `P2 / Success` | **Implemented:** `MilestoneApproved` | In-app |
| `MIL-05` | The lawyer marks the current approved milestone ready for funding. | Lawyer → Client | As the client, I want to know funding is required so work can begin. | `milestone.ready-for-funding` | `P1 / Information` | **Implemented:** existing `MilestoneReadyForFunding` | In-app; Email fallback deferred |

### Funding and work review

| ID | When this happens | Actor → recipient | User story / message intent | Type | Priority / severity | Readiness | Suggested channel |
|---|---|---|---|---|---|---|---|
| `MIL-06` | Funding is accepted by the API/provider and remains processing. | Client/System → Lawyer | As the lawyer, I want to know funding has started but is not final so I do not begin based on an uncertain payment. | `milestone.funding-started` | `P2 / Information` | **Implemented:** existing `MilestoneFundingStarted` | In-app |
| `MIL-07` | Funding completes through the direct flow, webhook, or reconciliation. | Provider/System → Client and Lawyer | As both parties, we want confirmation that the milestone is funded; the lawyer can begin work. | `milestone.funded` | `P1 / Success` | **Implemented:** existing `MilestoneFunded` | In-app; Email fallback deferred |
| `MIL-08` | Funding definitively fails. | Provider/System → Client | As the client, I want to know funding failed so I can retry or correct payment details. | `milestone.funding-failed` | `P1 / Critical` | **Implemented:** existing `MilestoneFundingFailed` | In-app; Email/SMS fallback deferred |
| `MIL-09` | The lawyer submits milestone work. | Lawyer → Client | As the client, I want to know work is ready for review and when automatic acceptance will occur. | `milestone.submitted` | `P1 / Information` | **Implemented:** existing `MilestoneSubmitted`; includes submission version | In-app; Email fallback deferred |
| `MIL-10` | The automatic-acceptance deadline is approaching and the submission is still awaiting review. | System → Client | As the client, I want a reminder before work is automatically accepted so I can review or request changes. | `milestone.review-deadline-approaching` | `P1 / Warning` | `Scheduled trigger`; recommended threshold such as 24 hours | In-app → Email fallback; SMS only if explicitly approved |
| `MIL-11` | The client requests changes to submitted work. | Client → Lawyer | As the lawyer, I want to know changes were requested so I can revise and resubmit. | `milestone.changes-requested` | `P1 / Warning` | **Implemented:** existing `MilestoneChangesRequested` | In-app; Email fallback deferred |
| `MIL-12` | The client manually accepts submitted work and the hold period begins. | Client → Lawyer | As the lawyer, I want to know the work was accepted and funds entered the hold period. | `milestone.accepted` | `P1 / Success` | **Implemented:** existing `MilestoneAccepted` | In-app |
| `MIL-13` | The system automatically accepts an unchanged submission after the review deadline. | System → Client and Lawyer | As both parties, we want a durable record that automatic acceptance occurred and the hold period began. | `milestone.auto-accepted` | `P1 / Warning` for Client; `Success` for Lawyer | **Implemented:** existing `MilestoneAutoAccepted` | In-app; Email fallback deferred |
| `MIL-14` | The dispute/hold window is approaching expiry and no dispute exists. | System → Client | As the client, I want a final reminder before held funds become eligible for release. | `milestone.hold-expiry-approaching` | `P1 / Warning` | `Scheduled trigger`; threshold required | In-app → Email fallback |

### Formal milestone change requests

| ID | When this happens | Actor → recipient | User story / message intent | Type | Priority / severity | Readiness | Suggested channel |
|---|---|---|---|---|---|---|---|
| `MIL-15` | Either participant creates a formal change request. | Client/Lawyer → Other participant | As the other participant, I want to review and decide the proposed milestone change. | `milestone.change-request-created` | `P1 / Information` | **Implemented:** existing `MilestoneChangeRequestCreated` | In-app; Email fallback deferred |
| `MIL-16` | The counterparty approves the change request. | Deciding participant → Requester | As the requester, I want to know my proposed milestone change was approved. | `milestone.change-request-approved` | `P1 / Success` | **Implemented:** existing `MilestoneChangeRequestApproved` | In-app |
| `MIL-17` | The counterparty rejects the change request. | Deciding participant → Requester | As the requester, I want to know the request was rejected and where to review the reason. | `milestone.change-request-rejected` | `P1 / Warning` | **Implemented:** existing `MilestoneChangeRequestRejected` | In-app; Email fallback deferred |
| `MIL-18` | The requester cancels a pending change request. | Requester → Other participant | As the other participant, I want to know the pending decision is no longer required. | `milestone.change-request-cancelled` | `P2 / Information` | **Implemented:** existing `MilestoneChangeRequestCancelled` | In-app |

### Settlement

| ID | When this happens | Actor → recipient | User story / message intent | Type | Priority / severity | Readiness | Suggested channel |
|---|---|---|---|---|---|---|---|
| `MIL-19` | Held funds are released normally, administratively, or after dispute settlement. | System/Admin → Lawyer; Client confirmation | As the lawyer, I want to know funds were credited; as the client, I want a settlement receipt. | `funds.released` | `P1 / Success` | **Implemented:** existing `FundsReleased` | In-app; Email fallback deferred |
| `MIL-20` | Funds are refunded after termination/dispute settlement. | System/Admin → Client; Lawyer confirmation | As the client, I want to know funds were returned; as the lawyer, I want to know settlement reduced/removed the expected release. | `funds.refunded` | `P1 / Success` for Client; `Information` for Lawyer | **Implemented:** existing `FundsRefunded` | In-app; Email fallback deferred |

Scheduling reconciliation and no-op jobs do not notify. They emit the normal final fact only if they actually cause a transition.

## 4. Payments and wallets

Milestone funding/release/refund notifications are defined once in the Milestone section even though the Payments slice produces many of those events. This avoids duplicate customer notifications.

| ID | When this happens | Actor → recipient | User story / message intent | Type | Priority / severity | Readiness | Suggested channel |
|---|---|---|---|---|---|---|---|
| `PAY-01` | A provider transaction remains unknown/processing beyond its normal reconciliation window. | System → Affected Client/Lawyer and Finance Admin queue | As the affected user, I want to know not to repeat the payment; as finance operations, I need a stuck-transaction alert. | `payment.processing-delayed` | `P1 / Warning` | `Scheduled trigger`; safe public reason only | In-app → Email fallback; operational alert |
| `PAY-02` | A finance administrator retries a payment and it reaches a final funded/failed state. | Admin/System → Affected participants | As the affected participant, I want the final outcome, not a notification for each retry attempt. | Reuse `milestone.funded` or `milestone.funding-failed` | `P1` | **Implemented:** reuse final outbox facts; uncertain retries do not notify | In-app |
| `PAY-03` | A lawyer withdrawal completes successfully. | Provider/System → Lawyer | As the lawyer, I want a durable withdrawal receipt without exposing destination details. | `wallet.withdrawal-completed` | `P1 / Success` | **Implemented:** `WithdrawalCompleted` | In-app; Email fallback deferred |
| `PAY-04` | A withdrawal definitively fails and reserved funds return to available balance. | Provider/System → Lawyer | As the lawyer, I want to know the withdrawal failed and funds are available again. | `wallet.withdrawal-failed` | `P1 / Warning` | **Implemented:** `WithdrawalFailed` | In-app; Email fallback deferred |
| `PAY-05` | A withdrawal remains uncertain and funds stay reserved for reconciliation/manual action. | System → Lawyer and Finance Admin queue | As the lawyer, I want to know not to submit a duplicate; as operations, I need to investigate. | `wallet.withdrawal-delayed` | `P1 / Warning` | **Implemented for Lawyer:** `WithdrawalDelayed`; finance queue remains operational logging | In-app; Email/SMS fallback deferred |
| `PAY-06` | A super administrator adjusts a lawyer wallet. | Super Admin → Lawyer | As the lawyer, I want a transparent record that my pending/available balance was administratively adjusted and where to request details. | `wallet.adjusted` | `P1 / Warning` | **Implemented:** `WalletAdjusted` in the adjustment transaction | In-app; Email fallback deferred |
| `PAY-07` | A webhook is invalid, duplicated, or rejected without changing business state. | Provider → Operations | Customer notification is inappropriate; this is a log/metric/security alert. | — | `None` | `No integration` | Monitoring only |

Funding initiation by the client and withdrawal initiation by the lawyer already return synchronously. The durable notification should represent the final or delayed outcome, not merely echo the accepted HTTP request.

## 5. Disputes

Repository actions: open, add evidence, assign moderator, start review, resolve and settle, recover settlement, and close.

| ID | When this happens | Actor → recipient | User story / message intent | Type | Priority / severity | Readiness | Suggested channel |
|---|---|---|---|---|---|---|---|
| `DSP-01` | A client or lawyer opens a dispute and the contract/hold is suspended. | Raising participant → Counterparty; moderation queue | As the counterparty, I want immediate notice that a dispute froze the milestone; as moderators, we need it in the work queue. | `dispute.opened` | `P1 / Critical` | `Existing event` `DisputeOpened`; queue recipient policy needed | In-app → Email fallback |
| `DSP-02` | A participant or moderator adds evidence. | Evidence author → Other participant and assigned moderator, excluding actor | As an involved party, I want to know new evidence is available so I can review/respond. | `dispute.evidence-added` | `P1 / Information` | `New event`; include IDs only, never evidence content | In-app |
| `DSP-03` | A moderator is assigned. | Admin/Moderator → Assigned moderator and both participants | As the assigned moderator, I need a work item; as participants, we want to know who is handling the dispute. | `dispute.assigned` | `P1 / Information` | `Existing event` `DisputeAssigned` | In-app → Email fallback for assigned moderator if unread |
| `DSP-04` | The assigned moderator starts review. | Moderator → Client and Lawyer | As both participants, we want to know the dispute is actively under review. | `dispute.review-started` | `P2 / Information` | `New event` | In-app |
| `DSP-05` | The moderator records a dispute resolution. | Moderator → Client and Lawyer | As both participants, we need the decision and a safe summary, with full details behind authorization. | `dispute.resolved` | `P1 / Critical` | `Existing event` `DisputeResolved` | In-app → Email fallback |
| `DSP-06` | Resolution is recorded but provider settlement remains pending. | System → Both participants and Finance Admin queue | As participants, we want to know the decision is final but money is still processing; operations need the stuck settlement. | `dispute.settlement-delayed` | `P1 / Warning` | `Scheduled trigger`/new event after threshold | In-app → Email fallback; operational alert |
| `DSP-07` | Resolution applies a lawyer penalty. | Super Admin → Lawyer | As the lawyer, I need explicit notice of an administrative penalty and where to review/appeal it. | `lawyer.penalty-applied` | `P1 / Critical` | `New event`; do not expose sensitive reason in metadata | In-app → Email immediate |
| `DSP-08` | Settlement is complete and the moderator closes the dispute. | Moderator/System → Client and Lawyer | As both participants, we want confirmation that the dispute workflow and settlement are closed. | `dispute.closed` | `P2 / Success` | `Existing event` `DisputeClosed` | In-app |

Refund/release results caused by a dispute use `funds.refunded` and `funds.released`; do not create duplicate settlement notifications from `DisputeResolved`.

## 6. User verification and administrative review

Repository actions: submit/delete verification documents, review individual documents, manually approve/reject accounts, and derive account status.

| ID | When this happens | Actor → recipient | User story / message intent | Type | Priority / severity | Readiness | Suggested channel |
|---|---|---|---|---|---|---|---|
| `VER-01` | A user submits one or more verification documents and at least one document is persisted, including a partial-success request. | User → every exact `Admin` role member | As verification staff, we need one durable review request for each successful submission while a dedicated queue is not yet available. | `verification.review-requested` | `P2 / Information` | **Implemented** from `VerificationReviewRequested` V1; one event per submission and one inbox row per Admin | In-app; Email deferred |
| `VER-02` | An administrator approves an individual current document. | Admin → User | As the user, I want to know that a submitted document was approved and whether further items remain. | `verification.document-approved` | `P2 / Success` | **Implemented** from `VerificationDocumentApproved` V1 | In-app; Email deferred |
| `VER-03` | An administrator rejects an individual current document. | Admin → User | As the user, I want to know which document needs replacement and where to view the reason. | `verification.document-rejected` | `P1 / Warning` | **Implemented** from `VerificationDocumentRejected` V1 | In-app; Email deferred |
| `VER-04` | Review discovers that the current document is expired. | Admin/System → User | As the user, I want to know the expired document must be replaced. | `verification.document-expired` | `P1 / Warning` | **Implemented** from `VerificationDocumentExpired` V1 on the expired transition | In-app; Email deferred |
| `VER-05` | The account becomes active after approval/requirements are satisfied. | Admin/System → User | As the user, I want confirmation that my account is approved and usable. | `account.approved` | `P1 / Success` | **Implemented** from `VerificationAccountApproved` V1; actual transition to `Active` only | In-app; Email deferred |
| `VER-06` | An administrator rejects the account/profile. | Admin → User | As the user, I want to know the account was rejected and where to review corrective action. | `account.rejected` | `P1 / Critical` | **Implemented** from `VerificationAccountRejected` V1; actual transition to `Rejected` only | In-app; Email deferred |
| `VER-07` | A current required document will expire soon. | System → User | As the user, I want advance notice so I can replace it before account eligibility is affected. | `verification.document-expiry-approaching` | `P1 / Warning` | `Scheduled trigger`; expiry thresholds required | In-app → Email fallback |
| `VER-08` | A user deletes or re-uploads a document and immediately receives the operation result. | User → Same user | No durable inbox item; later admin decisions are the meaningful facts. | — | `None` | `No integration` | None |

Account approval should not be emitted once per approved document. Emit it only when the account status actually transitions to `Active`.

### Implemented Gate 5 contract

The five Gate 5 mappings persist the exact Arabic title/body snapshots below. Every mapping uses `actionUrl: null`, the document owner or affected account owner is resolved from authoritative Verification context, and the outbox message ID provides idempotent replay behavior. Document data is limited to `documentId` and `documentType`; account data is limited to `userId`. Storage paths, file URLs/content, full rejection reasons, private review comments, contact details, provider IDs, tokens, and idempotency keys are forbidden. REST is durable and SignalR is best-effort. Verification is recorded in `AdminVerificationNotifications_Report.md` with `0 failed`; the expiry reminder `VER-07` remains deferred.

| Type | Severity | Arabic title | Arabic body |
|---|---|---|---|
| `verification.document-approved` | `Success` | `تم اعتماد مستند التحقق` | `تم اعتماد أحد مستندات التحقق الخاصة بك. يمكنك متابعة حالة التحقق من حسابك.` |
| `verification.document-rejected` | `Warning` | `تم رفض مستند التحقق` | `تم رفض أحد مستندات التحقق الخاصة بك. يرجى مراجعة التفاصيل واستبدال المستند عند الحاجة.` |
| `verification.document-expired` | `Warning` | `انتهت صلاحية مستند التحقق` | `انتهت صلاحية أحد مستندات التحقق الخاصة بك. يرجى إعادة رفع مستند ساري المفعول.` |
| `account.approved` | `Success` | `تم اعتماد حسابك` | `تم اعتماد حسابك وأصبح جاهزًا للاستخدام.` |
| `account.rejected` | `Critical` | `تم رفض الحساب` | `تم رفض طلب اعتماد حسابك. يرجى مراجعة التفاصيل واتخاذ الإجراء المطلوب.` |

### Implemented Gate 6 contract

`VerificationReviewRequested` V1 is queued by `SubmitVerificationDocumentsHandler` before the existing EF save when at least one document succeeds. A request with multiple successful documents produces one event with `documentCount`; a partial-success request also produces one event for its successful documents; a failed-only request produces none. The Notifications mapper resolves the exact `Admin` role membership from authoritative Identity tables and creates one `verification.review-requested` row per Admin. It excludes `SuperAdministrator`, ordinary users, and the uploading user. All use `actionUrl: null`, data keys are only `userId` and `documentCount`, and persisted Arabic copy is `طلب مراجعة مستندات التحقق` / `تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.` with `Information` severity. Storage paths, file URLs/content, file names, private metadata, rejection reasons, contact details, provider IDs, tokens, and idempotency keys are forbidden. Outbox message ID replay is idempotent; REST is durable and SignalR is best-effort. Verification is recorded in [`UserVerificationNotifications_Report.md`](../../../SmartCourt.Tests/HttpTests/UserVerificationNotifications_Report.md) with `147 passed, 0 failed, 1 documented skip`.

## 7. Authentication and account security

Authentication actions often occur before inbox access or intentionally revoke sessions. Existing confirmation/reset messages remain provider-specific Email/SMS flows; not every auth endpoint belongs in the in-app inbox.

| ID | When this happens | Actor → recipient | User story / message intent | Type | Priority / severity | Readiness | Suggested channel |
|---|---|---|---|---|---|---|---|
| `AUT-01` | An authenticated user changes their password and all refresh tokens are revoked. | User/System → Account owner | As the account owner, I want a security receipt and instructions if I did not make the change. | `security.password-changed` | `P1 / Critical` | `New event`; add to password-change transaction | Email immediate; optional in-app audit |
| `AUT-02` | A password reset succeeds and refresh tokens are revoked. | User/System → Account owner | As the account owner, I want a security receipt and recovery guidance if unauthorized. | `security.password-reset` | `P1 / Critical` | `New event`; add to reset transaction | Email immediate; optional in-app audit |
| `AUT-03` | A materially new device/location signs in. | System → Account owner | As the account owner, I want to detect unauthorized access. | `security.new-sign-in` | `P3 / Warning` | `Prerequisite`: trusted device/session fingerprint and false-positive policy | Email immediate + optional in-app; SMS only high confidence |
| `AUT-04` | Registration, confirmation resend, forgot-password, Email confirmation, phone token send/confirm, login, refresh, or token revoke completes normally. | User/System | The current response and existing Email/SMS challenge are sufficient; an inbox item would be inaccessible, redundant, or noisy. | — | `None` | `No integration` | Existing auth channels only |
| `AUT-05` | A client or lawyer deletes their account/profile. | User/System → former account owner | As the former owner, I may need a deletion receipt, but in-app is unavailable after deletion. | `account.deleted` | `P2 / Information` | `New event` only if retention/legal policy requires it | Email immediate only |

Security notifications must not include IP address, token, reset URL, security stamp, or raw device fingerprint in notification metadata.

## 8. Chat

Chat already persists messages and broadcasts `ReceiveMessage` through its own SignalR hub. It currently has no read receipt, unread counter, mute preference, or presence-based delivery contract.

| ID | When this happens | Actor → recipient | User story / message intent | Type | Priority / severity | Readiness | Suggested channel |
|---|---|---|---|---|---|---|---|
| `CHT-01` | A participant sends a user message that remains unread after a delay. | Client/Lawyer → Other participant | As the recipient, I want a reminder about unread conversation activity without receiving one inbox item per message while chatting. | `chat.unread-activity` | `P3 / Information` | `Prerequisite`: read state, mute preferences, aggregation window, and durable event | Aggregated in-app; Email fallback later |
| `CHT-02` | Contract/payment/dispute outbox events add a system chat message. | System → Participants | Do not create a chat-message notification when the same business event already creates a lifecycle notification. | — | `None` | `No integration` | None |

Recommendation: defer Chat-to-Notifications integration until Chat has read tracking. Per-message notification rows would duplicate Chat SignalR and produce excessive noise.

## 9. Cases, matching, analysis, and reviews

Current Case creation/update/delete/finalize, CaseReview, Matching, and CaseAnalysis paths return their results synchronously to the same authenticated user.

| ID | When this happens | Actor → recipient | User story / message intent | Type | Priority / severity | Readiness | Suggested channel |
|---|---|---|---|---|---|---|---|
| `CAS-01` | A user creates, updates, deletes, or finalizes their own case and receives the result synchronously. | User → Same user | No inbox item; the HTTP result and case UI are sufficient. | — | `None` | `No integration` | None |
| `CAS-02` | Matching recommendations are generated synchronously during finalize or fetched. | System → Requesting user | No notification while synchronous. If processing becomes asynchronous, notify on completed/failed outcome. | `case.matching-completed` / `case.matching-failed` | `P3` | `Prerequisite`: asynchronous workflow with requester identity | In-app when async |
| `CAS-03` | A case review report is generated synchronously. | System → Requesting user | No notification while synchronous. If moved to a background job, notify when the report is ready or failed. | `case-review.completed` / `case-review.failed` | `P3` | `Prerequisite`: asynchronous workflow | In-app when async |
| `CAS-04` | Contract completion/termination updates the legal-case lifecycle. | Contract system → Case owner | Do not duplicate the corresponding `contract.completed` or `contract.terminated` notification unless the Case slice later introduces a distinct user action. | — | `None` | `No integration` | None |

## 10. Document review and legal Q&A

`review-document` and `ask-law` are currently anonymous/synchronous AI operations.

| ID | When this happens | Actor → recipient | User story / message intent | Type | Priority / severity | Readiness | Suggested channel |
|---|---|---|---|---|---|---|---|
| `DOC-01` | Document review or legal Q&A completes synchronously. | System → Requester | No notification: there may be no authenticated recipient and the HTTP response already contains the result. | — | `None` | `No integration` | None |
| `DOC-02` | A future authenticated long-running review completes or fails. | System → Requesting user | As the requester, I want to know a background analysis is ready without polling. | `document-review.completed` / `document-review.failed` | `P3` | `Prerequisite`: authentication, persisted job ownership, and durable result | In-app |

Do not persist uploaded document text, extracted text, or AI prompt/answer content in a notification.

## 11. Law ingestion

Law ingestion is asynchronous and has meaningful completed/failed states, but the current controller's admin authorization is disabled and `LawDocument` does not store the initiating user ID. There is therefore no trustworthy notification recipient today.

| ID | When this happens | Actor → recipient | User story / message intent | Type | Priority / severity | Readiness | Suggested channel |
|---|---|---|---|---|---|---|---|
| `LAW-01` | A law-document ingestion job completes. | System → Initiating admin | As the admin, I want to know the document is searchable and how many chunks/pages were processed. | `law-ingestion.completed` | `P2 / Success` | `Prerequisite`: restore authorization and persist `InitiatedByUserId`; then `New event` | In-app |
| `LAW-02` | A law-document ingestion job fails after being queued. | System → Initiating admin/operations | As the admin, I want to know ingestion failed and where to inspect a safe failure summary. | `law-ingestion.failed` | `P1 / Warning` | Same prerequisite as `LAW-01` | In-app + operational alert |
| `LAW-03` | An admin starts/deletes ingestion and receives the synchronous result. | Admin → Same admin | Do not echo the accepted/deleted action into the inbox. | — | `None` | `No integration` | None |

Never put raw exception details, storage paths, extracted law text, or vector-store identifiers in the notification payload.

## 12. User profiles

Client/Lawyer get, complete, and update profile operations are self-service and synchronous.

| ID | When this happens | Actor → recipient | User story / message intent | Type | Priority / severity | Readiness | Suggested channel |
|---|---|---|---|---|---|---|---|
| `USR-01` | A user completes or updates their own profile and receives success immediately. | User → Same user | No notification unless the change actually moves the account into an administrative review state; that transition uses `verification.review-requested`. | — | `None` | `No integration` | None |
| `USR-02` | An account/profile is approved or rejected by an administrator. | Admin → User | Use `account.approved` or `account.rejected`; do not create a second profile-specific notification. | Reuse verification types | `P1` | Covered in Verification | Same as verification outcome |
| `USR-03` | Public lawyer search/profile views occur. | Viewer → Lawyer | Do not notify lawyers about searches/profile views; this is noisy and creates privacy concerns. | — | `None` | `No integration` | None |

Account deletion is covered by `AUT-05` because the only viable channel is outside the deleted inbox.

## 13. Files, health, and Notifications itself

| Slice/action | Decision | Reason |
|---|---|---|
| Contract/user file upload, download, or authorized content retrieval | No customer notification by default. | The operation is synchronous; access audit remains in the owning slice. Add security monitoring for anomalous access rather than ordinary inbox rows. |
| File-access denial | No notification. | Return authorization failure and log/audit it; avoid leaking resource existence. |
| Health ping | No notification. | Monitoring endpoint, not a business fact. |
| Notification feed/read/count actions | No notification about notifications. | Would recurse and create noise. |
| Outbox retries, worker idle loops, scheduling reconciliation no-ops | No customer notification. | Use logs/metrics; notify only when the underlying business state finally changes. |

## Coverage matrix by top-level slice

This table confirms that every directory under `SmartCourt/Features` was reviewed.

| Slice | Result |
|---|---|
| `Admin` | Verification decisions and queue work identified (`VER-*`). |
| `Auth` | Security receipts identified; normal token/challenge flows excluded (`AUT-*`). |
| `Case` | Current self-service synchronous actions excluded; async future condition documented (`CAS-*`). |
| `CaseAnalysis` | Covered with Case asynchronous-analysis condition. |
| `CaseReview` | Covered with synchronous exclusion/future async story (`CAS-03`). |
| `Chat` | Deferred pending read/aggregation capability (`CHT-*`). |
| `Contracts` | Contract lifecycle and termination recovery covered (`CON-*`). |
| `Disputes` | Participant, moderator, penalty, settlement, and closure stories covered (`DSP-*`). |
| `DocumentReview` | Anonymous synchronous exclusion and future async condition covered (`DOC-*`). |
| `Files` | Routine access excluded; security monitoring separated. |
| `Health` | Excluded. |
| `LawIngestion` | Async completion/failure stories documented with missing-recipient prerequisite (`LAW-*`). |
| `Matching` | Covered with Case analysis condition. |
| `Milestones` | Draft, approval, funding, review deadlines, change requests, and settlement covered (`MIL-*`). |
| `Notifications` | Read/feed operations excluded from self-notification. |
| `Payments` | Final outcomes reused; delayed payments, withdrawals, and adjustments covered (`PAY-*`). |
| `Proposals` | Three implemented stories recorded (`PRP-*`). |
| `Users` | Self-service profile actions excluded; review outcomes reused (`USR-*`). |
| `UserVerification` | Submission, document decisions, account state, and expiry covered (`VER-*`). |

## Candidate backlog for later planning

This is a grouping aid, not authorization to implement.

### Highest-value existing-event consumers

These provide broad coverage without changing source-slice HTTP logic:

- `ContractCreated`, `ContractActivated`, `ContractCompleted`, `ContractTerminated`;
- `MilestoneReadyForFunding`, `MilestoneFundingStarted`, `MilestoneFunded`, `MilestoneFundingFailed`;
- `MilestoneSubmitted`, `MilestoneAccepted`, `MilestoneAutoAccepted`, `MilestoneChangesRequested`;
- all four Milestone change-request events;
- `FundsReleased`, `FundsRefunded`;
- `DisputeOpened`, `DisputeAssigned`, `DisputeResolved`, `DisputeClosed`.

### Highest-value new events

- contract draft updated and termination requested;
- milestone draft created/updated/approved;
- dispute evidence added and review started;
- verification document/account outcomes;
- withdrawal final/delayed outcomes and wallet adjustment;
- scheduled milestone review/hold-expiry reminders.

### Explicitly deferred/conditional

- aggregated unread Chat activity;
- asynchronous Case/AI/document-review completion;
- law-ingestion completion/failure until authenticated initiator ownership exists;
- new-device login detection;
- Email/SMS delivery policies and provider-confirmed delivery receipts.

## Planning questions that need product approval

Before turning this catalog into an implementation plan, decide:

1. Which `P1` stories form the first integration batch?
2. Should both parties receive financial receipts, or only the party whose balance/action changed?
3. What safe portion of rejection, termination, dispute, and failure reasons may appear in notification bodies?
4. What are the auto-acceptance and hold-expiry reminder thresholds?
5. Which notification types qualify for future Email fallback, and which rare types may use SMS?
6. How should admin/moderator work queues assign a specific recipient without broadcasting to every privileged user?
7. **Decided:** notification title/body strings are initially Arabic plain text; machine types/data keys remain English. Localization/template storage is deferred.
8. What frontend resource routes will be the approved `actionUrl` allowlist for contracts, milestones, disputes, verification, payments, and wallets?
9. What retention is required for financial, security, account, and ordinary informational notifications?
10. Should self-initiated financial operations produce durable receipts even when the HTTP response is already successful? This catalog recommends yes for final settlement/withdrawal outcomes.

## Source areas reviewed

- All controllers and feature directories under `SmartCourt/Features`.
- Transactional event types/payloads under `Infrastructure/Providers/Events`.
- Contract, milestone, payment, dispute, withdrawal, and recovery services/jobs.
- Verification and account-decision handlers.
- Chat persistence/SignalR behavior and absence of read tracking.
- Law-ingestion background lifecycle and absence of initiator ownership.
- Existing Notifications REST, SignalR, entity, and proposal-event consumer.

## Related documentation

- [Notifications Documentation Index](./README.md)
- [Architecture Decision](./architecture.md)
- [Backend Integration Guide](./backend_integration_guide.md)
- [Frontend Integration Guide and API Contract](./frontend_integration_guide.md)
- [Implemented V1 Plan](./implementation_plan.md)

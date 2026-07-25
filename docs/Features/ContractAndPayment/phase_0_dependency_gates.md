# Contracts and Payments — Phase 0 Dependency Gates

This inventory records the owning-slice capabilities required by Contracts,
Milestones, Payments, and Disputes. The integration contracts are intentionally
narrow and expose facts or commands rather than another slice's EF entities.
They are not registered in dependency injection until their owning slices have
real implementations.

## Inventory

| Gate | Current repository state | Owning contract | Required production implementation |
| --- | --- | --- | --- |
| CAP-PREREQ-001 Proposals | Slice absent | `IProposalContractAccessService.FindAcceptedForContractAsync` | The Proposals slice returns authoritative proposal, case, client, and lawyer IDs only for an accepted proposal. |
| CAP-PREREQ-002 Cases | Slice absent | `ICaseContractAccessService.FindEligibleForContractAsync` | The Cases slice proves case eligibility and authoritative client ownership. |
| CAP-PREREQ-003 Chat | Slice absent | `IContractConversationService.AppendSystemMessageAsync` | The Chat slice appends idempotent system messages to the proposal conversation by event ID. |
| CAP-PREREQ-004 Files | Storage provider exists; authorization slice absent | `IContractFileAccessService.AuthorizeForUseAsync` and `GetAuthorizedReadAccessAsync` | The Files slice checks ownership/participant/moderator access and issues signed read access without exposing storage paths or secrets. |
| CAP-PREREQ-005 Notifications | Slice absent | `IContractNotificationService.PublishAsync` | The Notifications slice deduplicates by event ID and publishes non-sensitive participant/admin notifications. |
| CAP-PREREQ-006 Users | Client/lawyer profile services exist; eligibility capability absent | `IContractUserEligibilityService.FindEligibilityAsync` | The Users slice returns active-state and exact role eligibility for participants, moderators, finance administrators, and super administrators. |

## Contract creation gate

`ContractCreationDependencyGate` composes only the owning service interfaces.
It:

1. requests an accepted proposal from Proposals;
2. ensures the authenticated actor is the proposal lawyer;
3. requests an eligible case from Cases;
4. verifies the authoritative proposal client matches the authoritative case
   owner;
5. requests participant eligibility from Users; and
6. returns the authoritative IDs needed to construct a contract.

The gate does not query proposal, case, Identity, or profile tables and does not
reimplement accepted-proposal or case-eligibility rules.

## Isolated development

Reusable fakes for all six integration contracts live under
`SmartCourt.Tests/TestDoubles/ContractAndPayment`. They allow Contracts and
Payments slices to be developed and unit-tested before prerequisite slices are
implemented.

Production release remains blocked until CAP-PREREQ-001 through
CAP-PREREQ-006 have real owning-slice implementations, DI registrations,
authorization tests, and integration tests. No fallback implementation may
query hypothetical prerequisite tables or call another controller.

# Contracts & Payments System Specification

Based on the `smartcourt-transcript.md` document, here are the detailed extracted requirements and workflows for the Contracts and Payments system. This guide breaks down the core architecture, state flows, and business rules you'll need to implement.

## 1. Core Architecture: Milestone-Based Contracts

The entire contract system revolves around **Milestones** as the fundamental building block. There is no concept of a single "lump sum" contract delivery.

### Milestone Attributes
- **Deliverable:** A milestone must represent a tangible, finished piece of work. If a contract is terminated, the client should be able to take this completed deliverable to another lawyer.
- **Price:** The agreed-upon cost for completing the specific milestone.
- **Time/Deadline:** The duration expected to complete the milestone.
- **Can be "Time-Only":** A milestone can exist without a price. For example, if there is a court adjournment causing a delay, the milestone's time can be extended without increasing the financial cost.

## 2. The Escrow Payment Flow

The platform acts as an escrow agent to protect both the client and the lawyer. The client does not pay the total contract value upfront.

### Funding a Milestone
1. **Initiation:** The lawyer signals they are ready to start the next milestone.
2. **Client Approval & Payment:** The client agrees and pays the specific amount for that milestone to the platform.
3. **Escrow Hold:** **Crucial Rule:** No milestone officially starts, and no work is expected, until the platform successfully holds the funds for that specific milestone.

### Milestone Completion & The 14-Day Rule
1. **Submission:** The lawyer marks the milestone as completed/delivered.
2. **Client Acceptance:** The client accepts the delivery.
3. **Fund Release with Hold:** The funds are released to the lawyer's account on the platform, **but** there is a mandatory **14-day hold period**. The lawyer cannot withdraw the funds during these 14 days. This window is reserved for dispute resolution.

## 3. Contract Adjustments & Flexibility

Adjustments to the contract are made at the milestone level, not by rewriting the entire overall contract.

- **Mutual Consent:** Any changes to price or time require mutual consent. 
- **Extending Time:** If a delay occurs (e.g., waiting on court papers), it is preferred to edit the active milestone to increase its duration rather than creating a new milestone. The lawyer requests the edit, and the client must approve it on the platform.
- **Immutability:** Once a milestone is finished and paid, it **cannot** be edited.

## 4. Contract Termination & Handoff

Because a contract is a series of independent deliverables, termination is straightforward.

- **Disagreement on Next Milestone:** If the client and lawyer finish milestone #2 but cannot agree on the price/terms for milestone #3, the contract is gracefully terminated. 
- **Handoff to New Lawyer:** The client can take the completed deliverables and their case to a new lawyer.
- **No Milestone Inheritance:** The new lawyer does *not* adopt the remaining milestones of the old contract. Instead, the new lawyer evaluates what is left of the case from scratch and creates a brand-new contract with their own milestone breakdown.

## 5. Dispute Resolution

- **Raising a Dispute:** If during a handoff, a new lawyer points out that the previous lawyer scammed the client or the deliverables are fake/useless, the client has the right to raise a dispute (ideally within the 14-day fund-holding window).
- **Investigation:** The platform's moderation team (legal experts) is granted access to the chat logs and milestone deliverables to evaluate the claim.
- **Refunds & Penalties:** If proven true, funds are returned to the client. The offending lawyer receives a hidden "flag" (warning), or for severe/repeated offenses, is suspended (1-2 years) or terminated from the platform.

---

## 6. MISSING DETAILS: What you still need to define for Implementation
The transcript provides great business logic, but it is missing strict technical definitions. If you use this file as your context, you will need to design or decide on the following missing pieces:

### A. Database Entities & Schemas
- **Fields & Types:** The exact properties for a `Contract` and `Milestone`. (e.g., `Title`, `Description`, `TargetDate`, `Amount`, `Currency`).
- **State Machines:** The exact Enums/Statuses for a Milestone (e.g., `Draft`, `PendingPayment`, `Active`, `UnderReview`, `Completed`, `Disputed`, `Terminated`).

### B. Payment Details
- **Platform Fees:** The transcript mentions fees/percentages loosely ("takes 5% of the total"), but doesn't define if the platform takes a cut from the milestone price, or if the client pays a fee on top of it.
- **Mock vs. Stripe Implementation:** What exactly does the "mock escrow" look like? You'll need to define a mock wallet or transaction table to simulate holding and releasing funds if Stripe isn't used.
- **Refunds:** If a contract is terminated mid-way, or a dispute is won, the technical flow for refunding the money held in escrow needs to be mapped.

### C. Dispute Triggers
- **Technical Trigger:** How does a client actually raise a dispute? Is it a button on the completed milestone? Does it freeze the 14-day timer automatically?

### D. API Contracts
- The transcript says to "not agree on every single field as long as you understand the goal," so the exact API Endpoints and DTOs (Data Transfer Objects) are completely up to you to design based on the vertical slice architecture.

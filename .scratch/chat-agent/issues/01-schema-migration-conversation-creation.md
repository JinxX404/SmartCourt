# 01 — Schema, Migration & Conversation Creation

**What to build:**
Entities (`AgentConversation`, `AgentMessage`), EF Core Configurations, DbContext integration, EF Migration, and the `POST /api/agent/conversations` endpoint. Users (Clients and Lawyers) can create new chat agent conversations, optionally linking a Case with ownership authorization checks.

**Blocked by:** None — can start immediately

**Status:** completed

- [x] Create `AgentConversation` and `AgentMessage` entities in `Features/ChatAgent/Entities/`
- [x] Create EF Core entity configurations in `Features/ChatAgent/Persistence/`
- [x] Add `DbSet<AgentConversation>` and `DbSet<AgentMessage>` to `ApplicationDbContext`
- [x] Create `CreateAgentConversationRequest` DTO and validator
- [x] Implement `IChatAgentService.CreateConversationAsync` with case ownership check
- [x] Create `ChatAgentController` with `POST /api/agent/conversations`
- [x] Add EF Core migration for ChatAgent tables
- [x] Register `IChatAgentService` in DI
- [x] Write unit tests verifying conversation creation and case authorization

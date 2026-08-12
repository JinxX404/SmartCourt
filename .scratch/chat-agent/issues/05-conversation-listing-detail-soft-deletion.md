# 05 — Conversation Listing, Detail & Soft Deletion

**What to build:**
Endpoints to list the user's conversations (`GET /api/agent/conversations` with offset pagination), retrieve conversation details (`GET /api/agent/conversations/{id}`), and soft-delete a conversation (`DELETE /api/agent/conversations/{id}`).

**Blocked by:** 01 — Schema, Migration & Conversation Creation

**Status:** completed

- [x] Implement `ListConversationsAsync` in `ChatAgentService` (paginated, user-filtered, ordered by `UpdatedAt` desc)
- [x] Implement `GetConversationAsync` in `ChatAgentService` (with case details)
- [x] Implement `DeleteConversationAsync` in `ChatAgentService` (soft-delete flag)
- [x] Expose `GET /api/agent/conversations`, `GET /api/agent/conversations/{id}`, and `DELETE /api/agent/conversations/{id}` endpoints on `ChatAgentController`
- [x] Write unit tests verifying conversation listing, detail lookup, ownership enforcement, and soft-delete exclusion

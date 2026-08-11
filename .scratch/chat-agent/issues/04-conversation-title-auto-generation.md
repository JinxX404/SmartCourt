# 04 — Conversation Title Auto-Generation

**What to build:**
Automatic background title generation using the LLM after the first user message exchange, assigning a short descriptive Arabic title to the conversation without interrupting message flow.

**Blocked by:** 03 — RAG Pipeline & Message Exchange (`SendMessage`)

**Status:** completed

- [x] Implement `TryGenerateTitleAsync` helper in `ChatAgentService`
- [x] Trigger title generation during `SendMessageAsync` when `conversation.Title` is null
- [x] Clean generated title (strip quotes, trim, max 100 chars)
- [x] Update `conversation.Title` in DB
- [x] Handle LLM title generation failures gracefully (log warning, do not fail message send)
- [x] Write unit tests verifying title generation on first message, title retention on follow-up messages, and graceful failure handling

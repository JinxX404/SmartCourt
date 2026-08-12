# 03 — RAG Pipeline & Message Exchange (`SendMessage`)

**What to build:**
`POST /api/agent/conversations/{id}/messages` endpoint. Integrates `IEmbeddingProvider`, Qdrant vector store search (`IVectorStoreProvider`), and LLM (`IChatModelProvider`) with Egyptian legal AI persona, law article context, case context, and last 20 messages of context memory.

**Blocked by:** 02 — Case Document Text Extraction & Context Caching

**Status:** completed

- [x] Implement `SendMessageAsync` in `ChatAgentService`
- [x] Save user message in DB
- [x] Retrieve last 20 messages as conversation history
- [x] Fetch case context via `GetOrFetchCaseContextAsync`
- [x] Embed query using `IEmbeddingProvider`
- [x] Search Qdrant `egyptian_law` collection via `IVectorStoreProvider`
- [x] Compose Egyptian Legal AI system prompt + RAG law context + case context + history + question
- [x] Generate response via `IChatModelProvider`
- [x] Save assistant message and update conversation `UpdatedAt`
- [x] Expose `POST /api/agent/conversations/{id}/messages` endpoint on `ChatAgentController`
- [x] Write unit tests verifying RAG prompt generation and message saving

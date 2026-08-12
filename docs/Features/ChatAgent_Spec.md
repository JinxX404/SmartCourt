# Spec: Chat Agent — AI Legal Assistant

## Problem Statement

SmartCourt users (Clients and Lawyers) currently have no way to ask the system ad-hoc legal questions or get AI-powered guidance about their cases. The existing Chat feature (`Features/Chat`) is strictly human-to-human messaging between Clients and Lawyers tied to Proposals. Meanwhile, the platform already has a full AI infrastructure — LLM providers (DeepSeek/Gemini), an embedding pipeline (Gemini), and a vector store (Qdrant) loaded with Egyptian law articles — but these are only accessible through structured, one-shot operations like CaseReview and DocumentReview. There is no **conversational** interface that lets users interact with the AI iteratively, maintain conversation history, or get contextual assistance about a specific case across multiple exchanges.

## Solution

Add a new **Chat Agent** feature that provides a conversational AI assistant to every authenticated user. The agent is powered by the existing `IChatModelProvider`, `IEmbeddingProvider`, and `IVectorStoreProvider` infrastructure using Retrieval-Augmented Generation (RAG) against the ingested Egyptian law corpus.

Users can:
- **Create multiple conversations** with the AI agent, each tracked independently
- **Navigate previous conversations** through a paginated conversation list with auto-generated titles
- **Link a specific case** to a conversation so the AI has full context — including parsed case documents from Supabase — for every message in that conversation
- **Chat freely** without linking a case, for general Egyptian legal questions

The AI agent responds in formal legal Arabic, positioning itself as an Egyptian legal guidance assistant (not providing legal advice). Each response is enriched with relevant law articles retrieved from the Qdrant vector store.

## User Stories

1. As a Client, I want to start a new conversation with the AI legal assistant, so that I can ask questions about Egyptian law without needing to contact a Lawyer first.
2. As a Lawyer, I want to start a new conversation with the AI legal assistant, so that I can quickly research legal precedents and articles relevant to my work.
3. As a Client, I want to link one of my cases to a new conversation, so that the AI has full context about my case details and documents when answering my questions.
4. As a Lawyer, I want to link a case I'm assigned to when starting a conversation, so that the AI can reference the case specifics in its responses.
5. As a user, I want the system to verify I have access to a case before linking it to a conversation, so that case data is not exposed to unauthorized users.
6. As a user, I want to start a general conversation without linking any case, so that I can ask broad legal questions unrelated to a specific case.
7. As a user, I want to send a message to the AI and receive a response, so that I can get legal guidance on my question.
8. As a user, I want the AI to search relevant Egyptian law articles when answering my question, so that responses are grounded in actual legislation rather than hallucinated.
9. As a user, I want the AI to consider my case details (title, description, governorate, status) when I've linked a case, so that responses are tailored to my situation.
10. As a user, I want the AI to consider the full text content of my case's uploaded documents (PDFs, DOCX), so that the assistant understands the complete context of my legal situation.
11. As a user, I want the AI to remember what I said earlier in the same conversation, so that I can have a natural back-and-forth dialogue without repeating myself.
12. As a user, I want the conversation to have an auto-generated title based on my first message, so that I can identify conversations later without manually naming them.
13. As a user, I want to see a paginated list of my previous conversations ordered by most recent activity, so that I can easily find and resume past chats.
14. As a user, I want to open a previous conversation and see its full message history, so that I can review what was discussed.
15. As a user, I want to load older messages in a conversation by scrolling up (cursor-based pagination), so that long conversations remain performant.
16. As a user, I want to continue sending messages in a previous conversation, so that I can follow up on earlier topics.
17. As a user, I want to delete a conversation I no longer need, so that my conversation list stays organized.
18. As a user, I want deleted conversations to no longer appear in my list, so that the interface remains clean.
19. As a user, I want the AI to respond in formal legal Arabic, so that the guidance matches the professional context of the platform.
20. As a user, I want the system to protect me from excessive usage (rate limiting), so that the platform remains stable and fair for all users.
21. As a user, I want clear error messages when I exceed the rate limit, so that I understand why my message wasn't sent.
22. As a user, I want the system to only show me my own conversations, so that my chat history is private.
23. As a Client, I want to be prevented from linking a case that belongs to another Client, so that case data is kept confidential.
24. As a Lawyer, I want to be prevented from linking a case I'm not assigned to, so that I only access authorized case data.
25. As a user, I want the case documents to be parsed and cached the first time I link a case, so that subsequent messages in the conversation respond quickly without re-downloading files.
26. As a user, I want conversation details to show which case (if any) is linked, so that I know the context the AI is operating in.

## Implementation Decisions

### Architecture

- **New vertical slice**: `Features/ChatAgent`, completely separate from the existing human-to-human `Features/Chat`. No shared entities, controllers, or services.
- **No MediatR**: Following project convention, use a simple service class (`IChatAgentService` / `ChatAgentService`) for all business logic.
- **Provider Pattern**: The service depends on existing provider interfaces (`IChatModelProvider`, `IEmbeddingProvider`, `IVectorStoreProvider`, `IFileStorageService`, `IDocumentParsingProvider`) — no new external SDK integrations needed.

### Entities & Schema

- **`AgentConversation`**: Tracks each conversation. Fields: `Id`, `UserId` (owner), `CaseId` (optional FK to Cases), `Title` (nullable, auto-generated), `CachedCaseContext` (nvarchar(max) — parsed document text, cached on first access), `CreatedAt`, `UpdatedAt`, `IsDeleted` (soft delete flag). Navigation to `Case` and `AgentMessage` collection.
- **`AgentMessage`**: Tracks each message. Fields: `Id`, `ConversationId` (FK), `Role` (enum: `User = 1`, `Assistant = 2`), `Content` (nvarchar(max) — AI responses can be long), `CreatedAt`. Navigation to `AgentConversation`.
- **`AgentMessageRole`**: Enum with values `User` and `Assistant`.
- EF Fluent API configurations in a dedicated file within the slice. No Data Annotations.
- New EF migration to create `AgentConversations` and `AgentMessages` tables.

### RAG Pipeline (per message)

1. Normalize the user's question (Arabic text normalization).
2. Embed the question via `IEmbeddingProvider`.
3. Search the `egyptian_law` Qdrant collection via `IVectorStoreProvider` (top 5 results).
4. Build the LLM prompt combining: system prompt (Egyptian legal AI persona) + retrieved law article context + cached case context (if conversation has a linked case) + last 20 messages of conversation history + user question.
5. Call `IChatModelProvider.GenerateAsync` with the composed prompt.
6. Save both the user message and the assistant response to the database.

### Case Context & Document Caching

- When a conversation is created with a `CaseId`, validate ownership: the current user must be the Case's `ClientId` OR the Case's `LawyerId`.
- On the first message sent in a case-linked conversation (when `CachedCaseContext` is null), download all case documents from Supabase via `IFileStorageService.DownloadAsync`, parse them via `IDocumentParsingProvider.ExtractTextAsync`, concatenate the text, and store it in `AgentConversation.CachedCaseContext`.
- Subsequent messages reuse `CachedCaseContext` without re-downloading.
- Case metadata (Title, Description, Governorate, City, Status) is always fetched live from the DB and injected into the prompt alongside the cached document text.

### Conversation Title Auto-Generation

- After saving the first user message and AI response, make a secondary LLM call with a prompt like: "Generate a short Arabic title (max 10 words) for this conversation based on the following message: {firstUserMessage}".
- Update the conversation's `Title` field with the result.
- This is a fire-and-forget optimization — if it fails, the title stays null and the frontend can fall back to the creation date.

### Conversation Memory

- Each `SendMessage` call loads the last 20 messages (ordered by `CreatedAt`) from the conversation and formats them as alternating user/assistant turns in the LLM prompt.
- This provides continuity without sending unbounded context.

### API Contract

- **Route prefix**: `api/agent`
- **Authorization**: `[Authorize(Roles = "Client,Lawyer")]` on the controller.
- **Endpoints**:
  - `POST /api/agent/conversations` — Create conversation. Body: `{ caseId?: Guid }`. Returns `AgentConversationDto`.
  - `GET /api/agent/conversations?page={page}&pageSize={pageSize}` — List conversations (offset-based, ordered by `UpdatedAt` desc). Returns `AgentConversationListDto`.
  - `GET /api/agent/conversations/{id}` — Get conversation detail. Returns `AgentConversationDetailDto`.
  - `DELETE /api/agent/conversations/{id}` — Soft-delete conversation. Returns `ApiResponse<object>`.
  - `POST /api/agent/conversations/{id}/messages` — Send message. Body: `{ content: string }`. Returns `AgentMessageDto` (the assistant's response). Rate-limited.
  - `GET /api/agent/conversations/{id}/messages?before={messageId}&limit={n}` — List messages (cursor-based using `before` message ID, ordered by `CreatedAt` desc). Returns `AgentMessageListDto`.
- All responses wrapped in `ApiResponse<T>`.

### Pagination

- **Conversations list**: Offset-based (`page`, `pageSize`) — consistent with other listing endpoints.
- **Messages within a conversation**: Cursor-based (`before` message ID + `limit`) — optimal for chat UI infinite scroll. Response includes `HasMore` boolean.

### Validation

- FluentValidation in `Validators/` subfolder.
- `CreateAgentConversationRequestValidator`: If `CaseId` is provided, must not be `Guid.Empty`.
- `SendAgentMessageRequestValidator`: `Content` is required, max 2000 characters.

### Rate Limiting

- New policy `ChatAgentSend` added to `SecurityRateLimitPolicies`:
  - IP bucket: 60 requests per minute
  - User bucket: 20 requests per minute
- Applied only to the `POST .../messages` endpoint via the existing `[SecurityRateLimit]` attribute.

### DI Registration

- `IChatAgentService` → `ChatAgentService` registered as Scoped in `Program.cs`.

### AI Persona (System Prompt)

- Egyptian Legal AI Assistant.
- Responds in formal legal Arabic.
- Provides legal guidance, not legal advice.
- Disclaims that it is an AI assistant and recommends consulting a licensed lawyer for official legal advice.
- When case context is present, references specific case details in its responses.

## Testing Decisions

### Testing Seam

The primary test boundary is the **`IChatAgentService` / `ChatAgentService`** service layer. This is the same seam used by `CaseReviewServiceTests` and is the established pattern across the project. Tests exercise the service's public methods while mocking all external dependencies.

### What Makes a Good Test

- Tests verify **external behavior** (what the service returns, what it persists, what exceptions it throws) — not internal implementation details.
- Tests should not assert on the exact LLM prompt text (implementation detail) but rather on the observable outcomes: messages saved correctly, conversation state updated, correct errors thrown for invalid inputs.

### Unit Tests (xUnit + EF InMemory)

The following modules will be tested at the service level:

- **Conversation creation**: Happy path (with/without case), case not found, case ownership denied, unauthenticated user.
- **Send message**: Happy path (AI response saved), conversation not found, conversation owned by different user, empty/too-long content.
- **List conversations**: Returns only current user's non-deleted conversations, correct ordering, pagination.
- **Get conversation**: Happy path, not found, ownership check.
- **Delete conversation**: Soft-delete sets flag, already deleted, not found, ownership check.
- **Get messages**: Cursor-based pagination correctness, empty conversation, ownership check.

**Test doubles needed**: `TestChatModelProvider`, `TestEmbeddingProvider`, `TestVectorStoreProvider`, `TestFileStorageService`, `TestDocumentParsingProvider` — following the existing pattern in `SmartCourt.Tests/TestDoubles/`.

### HTTP Tests (PowerShell)

Following the established pattern in `HttpTests/`, a comprehensive PowerShell test script will exercise the full API flow end-to-end:
- Auth → Create conversation → Send messages → List conversations → Get messages (cursor pagination) → Delete conversation
- Case-linked conversation flow
- Error cases (unauthorized, rate-limited, invalid input)

### Prior Art

- `SmartCourt.Tests/Features/CaseReview/CaseReviewServiceTests.cs` — same pattern of in-memory DB + test doubles for AI providers.
- `SmartCourt.Tests/HttpTests/ChatAndProposals_Exhaustive_Test.ps1` — prior art for HTTP-level testing of chat-adjacent features.

## Out of Scope

- **Streaming responses (SSE/WebSocket)**: The agent returns full responses synchronously. Streaming can be added later if needed.
- **Multi-case conversations**: A conversation can link to at most one case. Switching or mentioning multiple cases mid-conversation is not supported.
- **File uploads in agent messages**: Users cannot upload documents directly in the agent chat. They must upload documents to the case first.
- **Admin access to agent conversations**: Only the conversation owner can view/manage their conversations. No admin panel for agent chat oversight.
- **Conversation sharing or export**: No ability to share or export agent conversations.
- **Message editing or regeneration**: Users cannot edit sent messages or request the AI to regenerate a response.
- **Frontend implementation**: This spec covers only the backend API. Frontend integration will be a separate effort.
- **Custom system prompts**: The AI persona is fixed. Users cannot customize the agent's behavior or system prompt.
- **Token counting / usage tracking**: No per-user token consumption tracking or billing.

## Further Notes

- The `CachedCaseContext` column uses `nvarchar(max)` because parsed legal documents can be substantial. For very large case files, consider adding a character limit or summarization step in a future iteration.
- The title auto-generation LLM call is intentionally non-blocking. If it fails, the conversation remains usable with a null title. The frontend should handle this gracefully (e.g., showing "Untitled Conversation" or the creation date).
- The `egyptian_law` Qdrant collection name is hardcoded, matching the existing convention in `DocumentReviewService` and `LawIngestionService`.
- The last-20-messages context window is a pragmatic choice balancing conversational continuity against LLM token limits. This value could be made configurable via `appsettings.json` in the future.
- Soft-delete was chosen over hard-delete to preserve audit trails and allow potential future "restore" functionality.

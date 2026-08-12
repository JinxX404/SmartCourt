# 02 — Case Document Text Extraction & Context Caching

**What to build:**
On-demand extraction and caching of case document text. When a conversation is linked to a case, the system fetches all case PDF/DOCX files from Supabase using `IFileStorageService`, extracts text using `IDocumentParsingProvider`, and caches it in `AgentConversation.CachedCaseContext` for fast reuse without repeated network or parsing overhead.

**Blocked by:** 01 — Schema, Migration & Conversation Creation

**Status:** completed

- [x] Implement `GetOrFetchCaseContextAsync` helper in `ChatAgentService`
- [x] Query case documents and files via EF Core
- [x] Download files from Supabase via `IFileStorageService.DownloadAsync`
- [x] Extract text using `IDocumentParsingProvider.ExtractTextAsync`
- [x] Format and store parsed text in `AgentConversation.CachedCaseContext`
- [x] Write unit tests verifying text extraction, caching on first access, and caching reuse on subsequent calls

using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Configuration;
using SmartCourt.Common.Exceptions;
using SmartCourt.Entities;
using SmartCourt.Features.ChatAgent.DTOs;
using SmartCourt.Features.ChatAgent.Entities;
using SmartCourt.Interfaces;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using CaseEntity = SmartCourt.Entities.Case;

namespace SmartCourt.Features.ChatAgent;

public class ChatAgentService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IChatModelProvider chatModelProvider,
    IEmbeddingProvider embeddingProvider,
    IVectorStoreProvider vectorStoreProvider,
    IFileStorageService fileStorageService,
    IDocumentParsingProvider documentParsingProvider,
    IRerankerProvider? rerankerProvider = null,
    IOptions<RagOptions>? ragOptions = null,
    IHttpContextAccessor? httpContextAccessor = null,
    TimeProvider? timeProvider = null,
    ILogger<ChatAgentService>? logger = null,
    IServiceScopeFactory? serviceScopeFactory = null) : IChatAgentService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IChatModelProvider _chatModelProvider = chatModelProvider;
    private readonly IEmbeddingProvider _embeddingProvider = embeddingProvider;
    private readonly IVectorStoreProvider _vectorStoreProvider = vectorStoreProvider;
    private readonly IFileStorageService _fileStorageService = fileStorageService;
    private readonly IDocumentParsingProvider _documentParsingProvider = documentParsingProvider;
    private readonly IRerankerProvider? _rerankerProvider = rerankerProvider;
    private readonly RagOptions _ragOptions = ragOptions?.Value ?? new RagOptions();
    private readonly IHttpContextAccessor? _httpContextAccessor = httpContextAccessor;
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;
    private readonly ILogger<ChatAgentService>? _logger = logger;
    private readonly IServiceScopeFactory? _serviceScopeFactory = serviceScopeFactory;

    public async Task<AgentConversationDto> CreateConversationAsync(
        CreateAgentConversationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
        {
            throw new AuthenticationException("المستخدم غير مسجل الدخول.");
        }

        var currentUserId = _currentUserService.UserId.Value;
        CaseEntity? caseEntity = null;

        if (request.CaseId.HasValue)
        {
            caseEntity = await _dbContext.Cases
                .FirstOrDefaultAsync(c => c.Id == request.CaseId.Value, cancellationToken);

            if (caseEntity is null)
            {
                throw new NotFoundException("القضية غير موجودة.");
            }

            if (caseEntity.ClientId != currentUserId && caseEntity.LawyerId != currentUserId)
            {
                throw new ForbiddenAccessException("غير مصرح لك بالوصول إلى هذه القضية.");
            }
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var conversation = AgentConversation.Create(
            Guid.NewGuid(),
            currentUserId,
            request.CaseId,
            utcNow);

        _dbContext.AgentConversations.Add(conversation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(conversation, caseEntity?.Title);
    }

    public async Task<AgentConversationListDto> ListConversationsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
        {
            throw new AuthenticationException("المستخدم غير مسجل الدخول.");
        }

        var currentUserId = _currentUserService.UserId.Value;

        var actualPage = page <= 0 ? 1 : page;
        var actualPageSize = pageSize <= 0 ? 20 : (pageSize > 100 ? 100 : pageSize);

        var query = _dbContext.AgentConversations
            .AsNoTracking()
            .Include(c => c.Case)
            .Where(c => c.UserId == currentUserId && !c.IsDeleted);

        var totalCount = await query.CountAsync(cancellationToken);

        var conversations = await query
            .OrderByDescending(c => c.UpdatedAt)
            .Skip((actualPage - 1) * actualPageSize)
            .Take(actualPageSize)
            .ToListAsync(cancellationToken);

        var dtos = conversations.Select(c => MapToDto(c, c.Case?.Title)).ToList();

        return new AgentConversationListDto(dtos, actualPage, actualPageSize, totalCount);
    }

    public async Task<AgentConversationDetailDto> GetConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
        {
            throw new AuthenticationException("المستخدم غير مسجل الدخول.");
        }

        var currentUserId = _currentUserService.UserId.Value;

        var conversation = await _dbContext.AgentConversations
            .AsNoTracking()
            .Include(c => c.Case)
            .FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted, cancellationToken);

        if (conversation is null)
        {
            throw new NotFoundException("المحادثة غير موجودة.");
        }

        if (conversation.UserId != currentUserId)
        {
            throw new ForbiddenAccessException("غير مصرح لك بالوصول إلى هذه المحادثة.");
        }

        return new AgentConversationDetailDto(
            conversation.Id,
            conversation.Title,
            conversation.CaseId,
            conversation.Case?.Title,
            conversation.Case?.Description,
            conversation.CreatedAt,
            conversation.UpdatedAt);
    }

    public async Task DeleteConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
        {
            throw new AuthenticationException("المستخدم غير مسجل الدخول.");
        }

        var currentUserId = _currentUserService.UserId.Value;

        var conversation = await _dbContext.AgentConversations
            .FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted, cancellationToken);

        if (conversation is null)
        {
            throw new NotFoundException("المحادثة غير موجودة.");
        }

        if (conversation.UserId != currentUserId)
        {
            throw new ForbiddenAccessException("غير مصرح لك بالوصول إلى هذه المحادثة.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        conversation.SoftDelete(utcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<AgentMessageDto> SendMessageAsync(
        Guid conversationId,
        SendAgentMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        var totalStopwatch = Stopwatch.StartNew();
        var phaseStopwatch = new Stopwatch();
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
        {
            throw new AuthenticationException("المستخدم غير مسجل الدخول.");
        }

        var currentUserId = _currentUserService.UserId.Value;

        var conversation = await _dbContext.AgentConversations
            .Include(c => c.Case)
            .FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted, cancellationToken);

        if (conversation is null)
        {
            throw new NotFoundException("المحادثة غير موجودة.");
        }

        if (conversation.UserId != currentUserId)
        {
            throw new ForbiddenAccessException("غير مصرح لك بالوصول إلى هذه المحادثة.");
        }

        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        var userMessage = AgentMessage.CreateUserMessage(
            Guid.NewGuid(),
            conversation.Id,
            request.Content,
            utcNow);

        phaseStopwatch.Restart();
        _dbContext.AgentMessages.Add(userMessage);
        conversation.MarkMessageAdded(utcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Perform RAG embedding API call in parallel with DB operations
        var normalizedQuery = SmartCourt.Providers.PdfParser.ArabicTextNormalizer.Normalize(request.Content);
        var embeddingTask = _embeddingProvider.GenerateEmbeddingsAsync(new[] { normalizedQuery }, cancellationToken);

        // Load conversation history (last 20 messages before the current one)
        var historyMessages = await _dbContext.AgentMessages
            .Where(m => m.ConversationId == conversationId && m.Id != userMessage.Id)
            .OrderByDescending(m => m.CreatedAt)
            .Take(20)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);
        phaseStopwatch.Stop();
        var dbOpsMs = phaseStopwatch.ElapsedMilliseconds;

        phaseStopwatch.Restart();
        // Fetch or get cached case context
        var caseContextText = await GetOrFetchCaseContextAsync(conversation, cancellationToken);
        phaseStopwatch.Stop();
        var caseContextMs = phaseStopwatch.ElapsedMilliseconds;

        phaseStopwatch.Restart();
        List<string> retrievedLawArticles = [];
        long rerankMs = 0;
        try
        {
            var queryEmbeddings = await embeddingTask;
            if (queryEmbeddings.Count > 0)
            {
                var searchResults = await _vectorStoreProvider.SearchAsync(
                    _ragOptions.LegalCollectionName,
                    queryEmbeddings[0],
                    topK: _ragOptions.CandidateCount,
                    filters: null,
                    cancellationToken: cancellationToken);

                // Extract chunks above minimum similarity
                retrievedLawArticles = searchResults
                    .Where(r => r.Score >= _ragOptions.MinimumSimilarityScore)
                    .Select(r => r.Payload.TryGetValue("chunk_text", out var chunkVal) ? chunkVal?.ToString()
                               : r.Payload.TryGetValue("text", out var textVal) ? textVal?.ToString() : null)
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Select(t => t!)
                    .ToList();

                // Rerank to keep only the most relevant chunks
                if (retrievedLawArticles.Count > 0 && _rerankerProvider != null)
                {
                    try
                    {
                        var rerankStopwatch = Stopwatch.StartNew();
                        var topN = Math.Min(_ragOptions.RerankedCount, retrievedLawArticles.Count);
                        var reranked = await _rerankerProvider.RerankAsync(
                            normalizedQuery, retrievedLawArticles, topN, cancellationToken);
                            
                        retrievedLawArticles = reranked
                            .Where(r => r.Index >= 0 && r.Index < retrievedLawArticles.Count)
                            .OrderByDescending(r => r.RelevanceScore)
                            .Select(r => retrievedLawArticles[r.Index])
                            .ToList();
                        
                        rerankStopwatch.Stop();
                        rerankMs = rerankStopwatch.ElapsedMilliseconds;
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Reranker failed for conversation {ConversationId}; using unranked results", conversationId);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to perform RAG vector search for conversation {ConversationId}", conversationId);
        }
        phaseStopwatch.Stop();
        var ragRetrievalMs = phaseStopwatch.ElapsedMilliseconds;

        // Determine role-based prompt guidelines
        bool isLawyer = _httpContextAccessor?.HttpContext?.User?.IsInRole("Lawyer") == true;
        if (!isLawyer && _httpContextAccessor?.HttpContext?.User?.IsInRole("Client") != true)
        {
            isLawyer = await _dbContext.UserRoles
                .AnyAsync(ur => ur.UserId == currentUserId &&
                    _dbContext.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Lawyer"),
                    cancellationToken);
        }

        // Build System Prompt based on User Role
        var systemPromptBuilder = new System.Text.StringBuilder();
        if (isLawyer)
        {
            systemPromptBuilder.AppendLine(@"أنت مساعد ومناظر قانوني ذكي متخصص في القانون المصري لمنصة SmartCourt، وتعمل كمساعد مباشر للمحامي (Pair Lawyer).
طبيعة دورك وهويتك:
1. تصرف كزميل محامي خبير ومحترف، وتحدث بتفصيل تحليلي وعميق حول كل استفسار يقدمه المحامي (act as a pair lawyer and talk in details about each inquiry).
2. قدم تحليلاً قانونياً شاملاً يشمل التكييف القانوني للوقائع، الدفوع الموضوعية والإجرائية، الاختصاص القضائي، والثغرات القانونية المحتملة.
3. استخدم المصطلحات والأساليب القانونية الفنية الدقيقة والمتبعة بين القضاة والمحامين.
4. استند إلى نصوص المواد والقوانين المصرية والمبادئ القضائية المتاحة في السياق بأسلوب تفصيلي يخدم صياغة المذكرات وبناء الاستراتيجية القضائية.
5. اربط تحليلك بتفاصيل ومستندات القضية المرفقة بأسلوب عميق ودقيق.");
        }
        else
        {
            systemPromptBuilder.AppendLine(@"أنت مستشار ومساعد قانوني ذكي موجه للموكل (العميل) عبر منصة SmartCourt.
طبيعة دورك وهويتك:
1. قدم نصائح وإرشادات قانونية موجهة ومبسطة في صورة خطوات إجرائية عمليّة ومحددة يمكن للموكل اتخاذها (Procedural, actionable steps).
2. تجنب التعقيد المصطلحي المفرط أو التفاصيل الأكاديمية الجافة، واستخدم أسلوباً واضحاً ومبسطاً وتوعوياً بصفتك مستشاراً قانونياً (Act as a legal advisor, don't be super technical).
3. التزم بالأمانة والنزاهة المطلقة: يمنع منعاً باتاً دعم أو مساندة أي استفسارات أو طلبات مشبوهة، خبيثة، أو غير مشروعة (Don't support malicious inquiries).
4. وضح للموكل دائماً بأسلوب مهني أن هذه الإرشادات لبناء الوعي وتحديد الخطوات العملية، ومتابعة الدعوى رسمياً يتم عبر محاميه المختص.
5. اربط إجابتك ببيانات ومستندات القضية المرفقة بأسلوب إجرائي مبسط وعملي.");
        }

        systemPromptBuilder.AppendLine(@"
[تعليمات تنسيق الإخراج - Markdown]:
You MUST format your response using Markdown. Use ** for bold text. You MUST use line breaks \n and bullet points - for lists. DO NOT use inline numbering like (1) or (a) in a single paragraph.
1. صِغ إجابتك بالكامل بتنسيق ماركداون قياسي (Standard GitHub Flavored Markdown) متوافق تماماً مع مكتبة react-markdown في الواجهة الأمامية.
2. استخدم العناوين الرئيسية والفرعية بأسلوب واضح (مثل: ## و ###) مع ترك مسافة بعد الهاش (مثال: ## العنوان).
3. عند الاقتباس من مواد وقوانين، استخدم مربع الاقتباس الماركداون بأسلوب ( > **مادة (رقم):** نص المادة...).
4. استخدم القوائم المنقطة (- بند) أو الرقمية (1. بند) مع ترك أسطر فارغة قبل القوائم وبعدها.
5. يمنع منعاً باتاً استخدام وسوم HTML مثل (<br>, <div>, <b>, <span>) ويجب الاستعاضة عنها بالتنسيق القياسي للماركداون.
6. اترك أسطراً فارغة بين الأقسام الرئيسية لضمان الوضوح والرؤية البصرية بأسلوب منظم وسلس.");

        if (retrievedLawArticles.Count > 0)
        {
            systemPromptBuilder.AppendLine("\n[مواد ونصوص القانون المصري ذات الصلة]:");
            for (int i = 0; i < retrievedLawArticles.Count; i++)
            {
                systemPromptBuilder.AppendLine($"--- النص {i + 1} ---");
                systemPromptBuilder.AppendLine(retrievedLawArticles[i]);
            }
        }

        if (!string.IsNullOrWhiteSpace(caseContextText))
        {
            systemPromptBuilder.AppendLine("\n[بيانات ومستندات القضية المرتبطة بالمحادثة]:");
            systemPromptBuilder.AppendLine(caseContextText);
        }

        // Build User Prompt with History
        var userPromptBuilder = new System.Text.StringBuilder();
        if (historyMessages.Count > 0)
        {
            userPromptBuilder.AppendLine("[تاريخ المحادثة السابقة]:");
            foreach (var msg in historyMessages)
            {
                var roleName = msg.Role == SmartCourt.Features.ChatAgent.Enums.AgentMessageRole.User ? "المستخدم" : "المساعد القانوني";
                userPromptBuilder.AppendLine($"{roleName}: {msg.Content}");
            }
            userPromptBuilder.AppendLine();
        }

        userPromptBuilder.AppendLine($"المستخدم: {request.Content}");

        var systemPromptText = systemPromptBuilder.ToString();

        phaseStopwatch.Restart();
        var aiResponseText = await _chatModelProvider.GenerateAsync(
            systemPromptBuilder.ToString(),
            userPromptBuilder.ToString(),
            cancellationToken);
        phaseStopwatch.Stop();
        var llmGenerationMs = phaseStopwatch.ElapsedMilliseconds;

        if (string.IsNullOrWhiteSpace(aiResponseText))
        {
            aiResponseText = "تمت مراجعة طلبك، ويرجى توضيح السؤال للحصول على تفاصيل أكثر.";
        }
        else
        {
            aiResponseText = SanitizeMarkdown(aiResponseText);
        }

        phaseStopwatch.Restart();
        var responseTime = _timeProvider.GetUtcNow().UtcDateTime;

        var assistantMessage = AgentMessage.CreateAssistantMessage(
            Guid.NewGuid(),
            conversation.Id,
            aiResponseText,
            responseTime);

        _dbContext.AgentMessages.Add(assistantMessage);
        conversation.MarkMessageAdded(responseTime);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (_serviceScopeFactory != null)
        {
            _ = Task.Run(() => TryGenerateTitleAsync(conversation.Id, request.Content));
        }
        else
        {
            await TryGenerateTitleAsync(conversation.Id, request.Content, cancellationToken);
        }
        phaseStopwatch.Stop();
        var postProcessMs = phaseStopwatch.ElapsedMilliseconds;

        totalStopwatch.Stop();

        _logger?.LogInformation(
            "[ChatAgent Perf] ConversationId={ConversationId} " +
            "Total={TotalMs}ms | DB={DbMs}ms | CaseCtx={CaseCtxMs}ms | " +
            "RAG={RagMs}ms | Rerank={RerankMs}ms | LLM={LlmMs}ms | PostProcess={PostMs}ms",
            conversationId, totalStopwatch.ElapsedMilliseconds,
            dbOpsMs, caseContextMs, ragRetrievalMs, rerankMs, llmGenerationMs, postProcessMs);

        return new AgentMessageDto(
            assistantMessage.Id,
            assistantMessage.Role.ToString(),
            assistantMessage.Content,
            assistantMessage.CreatedAt);
    }

    private async Task TryGenerateTitleAsync(
        Guid conversationId,
        string userMessageContent,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ApplicationDbContext dbContext = _dbContext;
            IChatModelProvider chatModelProvider = _chatModelProvider;
            TimeProvider timeProvider = _timeProvider;
            IServiceScope? scope = null;

            if (_serviceScopeFactory != null)
            {
                scope = _serviceScopeFactory.CreateScope();
                dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                chatModelProvider = scope.ServiceProvider.GetRequiredService<IChatModelProvider>();
                timeProvider = scope.ServiceProvider.GetService<TimeProvider>() ?? TimeProvider.System;
            }

            try
            {
                var conversation = await dbContext.AgentConversations
                    .FirstOrDefaultAsync(c => c.Id == conversationId, cancellationToken);

                if (conversation is null || !string.IsNullOrWhiteSpace(conversation.Title))
                {
                    return;
                }

                var systemPrompt = ChatAgentPrompts.GetTitleGenerationPrompt();
                var generatedTitle = await chatModelProvider.GenerateAsync(systemPrompt, userMessageContent, cancellationToken);

                if (!string.IsNullOrWhiteSpace(generatedTitle))
                {
                    var cleanTitle = generatedTitle
                        .Replace("\"", "")
                        .Replace("'", "")
                        .Replace("«", "")
                        .Replace("»", "")
                        .Replace("\r", "")
                        .Replace("\n", " ")
                        .Trim();

                    if (cleanTitle.Length > 150)
                    {
                        cleanTitle = cleanTitle[..150].Trim();
                    }

                    if (!string.IsNullOrWhiteSpace(cleanTitle))
                    {
                        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
                        conversation.UpdateTitle(cleanTitle, utcNow);
                        await dbContext.SaveChangesAsync(cancellationToken);
                    }
                }
            }
            finally
            {
                scope?.Dispose();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to auto-generate conversation title for ConversationId {ConversationId}", conversationId);
        }
    }

    public async Task<AgentMessageListDto> GetMessagesAsync(
        Guid conversationId,
        Guid? beforeMessageId,
        int limit,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
        {
            throw new AuthenticationException("المستخدم غير مسجل الدخول.");
        }

        var currentUserId = _currentUserService.UserId.Value;

        var conversation = await _dbContext.AgentConversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted, cancellationToken);

        if (conversation is null)
        {
            throw new NotFoundException("المحادثة غير موجودة.");
        }

        if (conversation.UserId != currentUserId)
        {
            throw new ForbiddenAccessException("غير مصرح لك بالوصول إلى هذه المحادثة.");
        }

        var actualLimit = limit <= 0 ? 20 : (limit > 100 ? 100 : limit);

        var query = _dbContext.AgentMessages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId);

        if (beforeMessageId.HasValue && beforeMessageId.Value != Guid.Empty)
        {
            var anchorMessage = await _dbContext.AgentMessages
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == beforeMessageId.Value && m.ConversationId == conversationId, cancellationToken);

            if (anchorMessage is not null)
            {
                query = query.Where(m => m.CreatedAt < anchorMessage.CreatedAt);
            }
        }

        var fetchedMessages = await query
            .OrderByDescending(m => m.CreatedAt)
            .Take(actualLimit + 1)
            .ToListAsync(cancellationToken);

        var hasMore = fetchedMessages.Count > actualLimit;

        var pageMessages = fetchedMessages
            .Take(actualLimit)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new AgentMessageDto(
                m.Id,
                m.Role.ToString(),
                m.Content,
                m.CreatedAt))
            .ToList();

        return new AgentMessageListDto(pageMessages, hasMore);
    }

    public async Task<string?> GetOrFetchCaseContextAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated || _currentUserService.UserId is null)
        {
            throw new AuthenticationException("المستخدم غير مسجل الدخول.");
        }

        var conversation = await _dbContext.AgentConversations
            .FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted, cancellationToken);

        if (conversation is null)
        {
            throw new NotFoundException("المحادثة غير موجودة.");
        }

        if (conversation.UserId != _currentUserService.UserId.Value)
        {
            throw new ForbiddenAccessException("غير مصرح لك بالوصول إلى هذه المحادثة.");
        }

        return await GetOrFetchCaseContextAsync(conversation, cancellationToken);
    }

    public async Task<string?> GetOrFetchCaseContextAsync(
        AgentConversation conversation,
        CancellationToken cancellationToken = default)
    {
        if (!conversation.CaseId.HasValue)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(conversation.CachedCaseContext))
        {
            return conversation.CachedCaseContext;
        }

        var caseEntity = await _dbContext.Cases
            .Include(c => c.Documents)
                .ThenInclude(d => d.StoredFile)
            .FirstOrDefaultAsync(c => c.Id == conversation.CaseId.Value, cancellationToken);

        if (caseEntity is null)
        {
            return null;
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("[تفاصيل القضية]");
        sb.AppendLine($"العنوان: {caseEntity.Title}");
        sb.AppendLine($"الوصف: {caseEntity.Description}");
        if (!string.IsNullOrWhiteSpace(caseEntity.Governorate))
            sb.AppendLine($"المحافظة: {caseEntity.Governorate}");
        if (!string.IsNullOrWhiteSpace(caseEntity.City))
            sb.AppendLine($"المدينة: {caseEntity.City}");
        sb.AppendLine($"الحالة: {caseEntity.Status}");

        if (caseEntity.Documents.Count > 0)
        {
            sb.AppendLine("\n[مستندات القضية]");
            foreach (var doc in caseEntity.Documents)
            {
                var storedFile = doc.StoredFile;
                var name = storedFile?.OriginalFileName ?? "Document";
                var path = !string.IsNullOrWhiteSpace(storedFile?.FileUrl)
                    ? storedFile.FileUrl
                    : storedFile?.StoredFileName;

                sb.AppendLine($"--- مستند: {name} ---");

                if (!string.IsNullOrWhiteSpace(path))
                {
                    try
                    {
                        var fileBytes = await _fileStorageService.DownloadAsync(path, cancellationToken);
                        if (fileBytes.Length > 0)
                        {
                            using var stream = new MemoryStream(fileBytes);
                            var extractedText = await _documentParsingProvider.ExtractTextAsync(stream, name, cancellationToken);

                            if (!string.IsNullOrWhiteSpace(extractedText))
                            {
                                const int maxDocChars = 4000;
                                var textToAppend = extractedText.Length > maxDocChars
                                    ? string.Concat(extractedText.AsSpan(0, maxDocChars), "\n[... تم اقتطاع جزء من المحتوى لكبر الحجم ...]")
                                    : extractedText;

                                sb.AppendLine(textToAppend);
                            }
                            else
                            {
                                sb.AppendLine("[لم يتم استخراج أي نص من هذا المستند]");
                            }
                        }
                        else
                        {
                            sb.AppendLine("[ملف المستند فارغ]");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Failed to download or parse document {FileName} for CaseId {CaseId}", name, conversation.CaseId);
                        sb.AppendLine("[تعذر استخراج محتوى هذا المستند]");
                    }
                }
                else
                {
                    sb.AppendLine("[لا يوجد مسار تخزين للمستند]");
                }
            }
        }

        var contextText = sb.ToString().Trim();
        var utcNow = _timeProvider.GetUtcNow().UtcDateTime;
        conversation.CacheCaseContext(contextText, utcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return contextText;
    }

    public static string SanitizeMarkdown(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        // Convert HTML breaks to Markdown double newlines
        var sanitized = System.Text.RegularExpressions.Regex.Replace(
            input,
            @"<br\s*/?>",
            "\n\n",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        // Strip remaining HTML tags (e.g. <div>, <span>, <b>, <i>, <p>)
        sanitized = System.Text.RegularExpressions.Regex.Replace(
            sanitized,
            @"<[^>]+>",
            string.Empty);

        // Ensure space after markdown header hashes (#Title -> # Title, ##Title -> ## Title)
        sanitized = System.Text.RegularExpressions.Regex.Replace(
            sanitized,
            @"(?m)^(#{1,6})([^\s#])",
            "$1 $2");

        // Normalize 3+ consecutive newlines down to 2 newlines
        sanitized = System.Text.RegularExpressions.Regex.Replace(
            sanitized,
            @"(\r?\n){3,}",
            "\n\n");

        return sanitized.Trim();
    }

    private static AgentConversationDto MapToDto(AgentConversation conversation, string? caseTitle)
    {
        return new AgentConversationDto(
            conversation.Id,
            conversation.Title,
            conversation.CaseId,
            caseTitle,
            conversation.CreatedAt,
            conversation.UpdatedAt);
    }
}

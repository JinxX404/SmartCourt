using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Configuration;
using SmartCourt.Common.Exceptions;
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
    IQuotaService quotaService,
    SmartCourt.Features.LawyerSubscription.ILawyerQuotaService lawyerQuotaService,
    ICostCalculatorService costCalculatorService,
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
    private readonly IQuotaService _quotaService = quotaService;
    private readonly SmartCourt.Features.LawyerSubscription.ILawyerQuotaService _lawyerQuotaService = lawyerQuotaService;
    private readonly ICostCalculatorService _costCalculatorService = costCalculatorService;
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
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == null)
        {
            throw new AuthenticationException("يجب تسجيل الدخول لبدء محادثة.");
        }

        bool isClient = _httpContextAccessor?.HttpContext?.User?.IsInRole("Client") == true;
        bool isLawyer = _httpContextAccessor?.HttpContext?.User?.IsInRole("Lawyer") == true;

        if (isClient && currentUserId.HasValue)
        {
            var quota = await _quotaService.GetQuotaAsync(currentUserId.Value, cancellationToken);
            if (quota.TotalRemainingCredits <= 0)
            {
                throw new BusinessException("لقد استنفدت الرصيد المتاح لك، ولا يمكنك بدء محادثة جديدة.");
            }
        }
        else if (isLawyer && currentUserId.HasValue)
        {
            var quota = await _lawyerQuotaService.GetQuotaAsync(currentUserId.Value, cancellationToken);
            if (quota.TotalRemainingCredits <= 0)
            {
                throw new BusinessException("لقد استنفدت الرصيد المتاح لك، ولا يمكنك بدء محادثة جديدة.");
            }
        }

        CaseEntity? caseEntity = null;

        if (currentUserId == null)
        {
            throw new AuthenticationException("يجب تسجيل الدخول لإنشاء محادثة.");
        }

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

        var utcNow = _timeProvider.GetUtcNow();
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
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;

        var actualPage = page <= 0 ? 1 : page;
        var actualPageSize = pageSize <= 0 ? 20 : (pageSize > 100 ? 100 : pageSize);

        if (currentUserId == null)
        {
            return new AgentConversationListDto([], actualPage, actualPageSize, 0);
        }

        var query = _dbContext.AgentConversations
            .AsNoTracking()
            .Include(c => c.Case)
            .Where(c => c.UserId == currentUserId && !c.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();
            query = query.Where(c => c.Title != null && c.Title.Contains(searchTerm));
        }

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
        var currentUserId = _currentUserService.UserId;

        var conversation = await _dbContext.AgentConversations
            .AsNoTracking()
            .Include(c => c.Case)
            .FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted, cancellationToken);

        if (conversation is null)
        {
            throw new NotFoundException("المحادثة غير موجودة.");
        }

        if (conversation.UserId != null && conversation.UserId != currentUserId)
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
        var currentUserId = _currentUserService.UserId;

        var conversation = await _dbContext.AgentConversations
            .FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted, cancellationToken);

        if (conversation is null)
        {
            throw new NotFoundException("المحادثة غير موجودة.");
        }

        if (conversation.UserId != null && conversation.UserId != currentUserId)
        {
            throw new ForbiddenAccessException("غير مصرح لك بالوصول إلى هذه المحادثة.");
        }

        var utcNow = _timeProvider.GetUtcNow();
        conversation.SoftDelete(utcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<AgentConversationDto> UpdateConversationTitleAsync(
        Guid conversationId,
        UpdateAgentConversationTitleRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;

        var conversation = await _dbContext.AgentConversations
            .Include(c => c.Case)
            .FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted, cancellationToken);

        if (conversation is null)
        {
            throw new NotFoundException("المحادثة غير موجودة.");
        }

        if (conversation.UserId != null)
        {
            if (currentUserId == null)
            {
                throw new AuthenticationException("يجب تسجيل الدخول لتعديل هذه المحادثة.");
            }

            if (conversation.UserId != currentUserId)
            {
                throw new ForbiddenAccessException("غير مصرح لك بتعديل هذه المحادثة.");
            }
        }

        var utcNow = _timeProvider.GetUtcNow();
        conversation.UpdateTitle(request.Title, utcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(conversation, conversation.Case?.Title);
    }

    public async Task<AgentMessageDto> SendMessageAsync(
        Guid conversationId,
        SendAgentMessageRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        bool isClient = _httpContextAccessor?.HttpContext?.User?.IsInRole("Client") == true;
        bool isLawyerCheck = _httpContextAccessor?.HttpContext?.User?.IsInRole("Lawyer") == true;

        QuotaReservation? totalReservation = null;
        int actualTokensUsed = 0;
        var normalizedQuery = SmartCourt.Providers.PdfParser.ArabicTextNormalizer.Normalize(request.Content);

        if (isClient && currentUserId.HasValue)
        {
            int stage1Ceiling = Math.Min(normalizedQuery.Length, 2000);
            totalReservation = await _quotaService.ReserveQuotaAsync(currentUserId.Value, stage1Ceiling, cancellationToken);
        }
        else if (isLawyerCheck && currentUserId.HasValue)
        {
            int stage1Ceiling = Math.Min(normalizedQuery.Length, 2000);
            totalReservation = await _lawyerQuotaService.ReserveQuotaAsync(currentUserId.Value, stage1Ceiling, cancellationToken);
        }

        try
        {
            var totalSw = System.Diagnostics.Stopwatch.StartNew();
            var ragApiSw = System.Diagnostics.Stopwatch.StartNew();

            // 1. Start External I/O for RAG Pipeline (Embedding -> Qdrant) concurrently
            async Task<(SmartCourt.Interfaces.Providers.EmbeddingResponse Response, List<string> Articles, long ElapsedMs)> GetEmbeddingAndQdrantAsync()
            {
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var response = await _embeddingProvider.GenerateEmbeddingsAsync(new[] { normalizedQuery }, cancellationToken);
                List<string> lawArticles = [];
                
                if (response.Embeddings.Count > 0)
                {
                    var searchResults = await _vectorStoreProvider.SearchAsync(
                        _ragOptions.LegalCollectionName,
                        response.Embeddings[0],
                        topK: _ragOptions.CandidateCount,
                        filters: null,
                        cancellationToken: cancellationToken);

                    lawArticles = searchResults
                        .Where(r => r.Score >= _ragOptions.MinimumSimilarityScore)
                        .Select(r => r.Payload.TryGetValue("chunk_text", out var chunkVal) ? chunkVal?.ToString()
                                   : r.Payload.TryGetValue("text", out var textVal) ? textVal?.ToString() : null)
                        .Where(t => !string.IsNullOrWhiteSpace(t))
                        .Select(t => t!)
                        .ToList();
                }
                sw.Stop();
                return (response, lawArticles, sw.ElapsedMilliseconds);
            }

            var ragPipelineTask = GetEmbeddingAndQdrantAsync();

            var dbOpsSw = System.Diagnostics.Stopwatch.StartNew();

            // 2. Perform DB operations sequentially to avoid EF Core DbContext concurrency issues
            var conversation = await _dbContext.AgentConversations
                .Include(c => c.Case)
                .FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted, cancellationToken);

            if (conversation is null)
            {
                throw new NotFoundException("المحادثة غير موجودة.");
            }

            if (conversation.UserId != null && conversation.UserId != currentUserId)
            {
                throw new ForbiddenAccessException("غير مصرح لك بالوصول إلى هذه المحادثة.");
            }

            var utcNow = _timeProvider.GetUtcNow();
            var userMessage = AgentMessage.CreateUserMessage(
                Guid.NewGuid(),
                conversation.Id,
                request.Content,
                utcNow);

            _dbContext.AgentMessages.Add(userMessage);
            conversation.MarkMessageAdded(utcNow);
            await _dbContext.SaveChangesAsync(cancellationToken);

            // Load conversation history (last 20 messages before the current one)
            var historyMessages = await _dbContext.AgentMessages
                .Where(m => m.ConversationId == conversationId && m.Id != userMessage.Id)
                .OrderByDescending(m => m.CreatedAt)
                .Take(20)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync(cancellationToken);

            // Fetch or get cached case context
            var caseContextText = await GetOrFetchCaseContextAsync(conversation, cancellationToken);

            bool isLawyer = await IsUserLawyerAsync(currentUserId, cancellationToken);

            dbOpsSw.Stop();

            // 3. Await the external API pipeline results
            var (embeddingResponse, retrievedLawArticles, ragElapsedMs) = await ragPipelineTask;
            ragApiSw.Stop();

            _logger?.LogInformation("ChatAgent Concurrency: DB Ops took {DbOpsMs}ms. RAG API Ops took {RagApiMs}ms. Total concurrent wait took {TotalMs}ms.", 
                dbOpsSw.ElapsedMilliseconds, ragElapsedMs, ragApiSw.ElapsedMilliseconds);

            int embeddingTokens = embeddingResponse.InputTokens;
            int rerankTokens = 0;
            
            if (isClient && currentUserId.HasValue)
            {
                if (embeddingTokens > 0)
                {
                    actualTokensUsed += embeddingTokens;
                }
                else
                {
                    // Fallback ceiling if Alibaba usage is missing
                    int fallback = Math.Min(normalizedQuery.Length, 2000);
                    _logger?.LogWarning("Alibaba embedding returned 0 usage. Using conservative ceiling: {Tokens}", fallback);
                    actualTokensUsed += fallback;
                }
            }
            else if (isLawyerCheck && currentUserId.HasValue)
            {
                if (embeddingTokens > 0)
                {
                    actualTokensUsed += embeddingTokens;
                }
                else
                {
                    int fallback = Math.Min(normalizedQuery.Length, 2000);
                    _logger?.LogWarning("Alibaba embedding returned 0 usage. Using conservative ceiling: {Tokens}", fallback);
                    actualTokensUsed += fallback;
                }
            }
            var systemPromptText = ChatAgentPrompts.BuildSystemPrompt(isLawyer, retrievedLawArticles, caseContextText);
            var userPromptText = BuildUserPrompt(historyMessages, request.Content);

            // Stage 2 Reservation
            if (isClient && currentUserId.HasValue)
            {
                int rerankCeiling = retrievedLawArticles.Sum(doc => normalizedQuery.Length + doc.Length + 50);
                int chatInputCeiling = systemPromptText.Length + userPromptText.Length;
                int chatOutputCeiling = 5000;
                
                int stage2Ceiling = rerankCeiling + chatInputCeiling + chatOutputCeiling;
                var stage2Reservation = await _quotaService.ReserveQuotaAsync(currentUserId.Value, stage2Ceiling, cancellationToken);
                
                if (totalReservation != null)
                {
                    totalReservation = new QuotaReservation
                    {
                        TotalReservedTokens = totalReservation.TotalReservedTokens + stage2Reservation.TotalReservedTokens,
                        FreeReservedTokens = totalReservation.FreeReservedTokens + stage2Reservation.FreeReservedTokens,
                        PaidReservedTokens = totalReservation.PaidReservedTokens + stage2Reservation.PaidReservedTokens
                    };
                }
                else
                {
                    totalReservation = stage2Reservation;
                }
            }
            else if (isLawyerCheck && currentUserId.HasValue)
            {
                int rerankCeiling = retrievedLawArticles.Sum(doc => normalizedQuery.Length + doc.Length + 50);
                int chatInputCeiling = systemPromptText.Length + userPromptText.Length;
                int chatOutputCeiling = 5000;
                
                int stage2Ceiling = rerankCeiling + chatInputCeiling + chatOutputCeiling;
                var stage2Reservation = await _lawyerQuotaService.ReserveQuotaAsync(currentUserId.Value, stage2Ceiling, cancellationToken);
                
                if (totalReservation != null)
                {
                    totalReservation = new QuotaReservation
                    {
                        TotalReservedTokens = totalReservation.TotalReservedTokens + stage2Reservation.TotalReservedTokens,
                        FreeReservedTokens = totalReservation.FreeReservedTokens + stage2Reservation.FreeReservedTokens,
                        PaidReservedTokens = totalReservation.PaidReservedTokens + stage2Reservation.PaidReservedTokens
                    };
                }
                else
                {
                    totalReservation = stage2Reservation;
                }
            }

            // Rerank
            var rerankSw = System.Diagnostics.Stopwatch.StartNew();
            if (retrievedLawArticles.Count > 0 && _rerankerProvider != null)
            {
                try
                {
                    var topN = Math.Min(_ragOptions.RerankedCount, retrievedLawArticles.Count);
                    var rerankResponse = await _rerankerProvider.RerankAsync(normalizedQuery, retrievedLawArticles, topN, cancellationToken);
                    rerankTokens = rerankResponse.InputTokens;

                    if (isClient && currentUserId.HasValue)
                    {
                        if (rerankTokens > 0)
                        {
                            actualTokensUsed += rerankTokens;
                        }
                        else
                        {
                            int fallback = retrievedLawArticles.Sum(doc => normalizedQuery.Length + doc.Length + 50);
                            _logger?.LogWarning("Alibaba reranker returned 0 usage. Using conservative ceiling: {Tokens}", fallback);
                            actualTokensUsed += fallback;
                        }
                    }
                    else if (isLawyerCheck && currentUserId.HasValue)
                    {
                        if (rerankTokens > 0)
                        {
                            actualTokensUsed += rerankTokens;
                        }
                        else
                        {
                            int fallback = retrievedLawArticles.Sum(doc => normalizedQuery.Length + doc.Length + 50);
                            _logger?.LogWarning("Alibaba reranker returned 0 usage. Using conservative ceiling: {Tokens}", fallback);
                            actualTokensUsed += fallback;
                        }
                    }

                    retrievedLawArticles = rerankResponse.Results
                        .Where(r => r.Index >= 0 && r.Index < retrievedLawArticles.Count)
                        .OrderByDescending(r => r.RelevanceScore)
                        .Select(r => retrievedLawArticles[r.Index])
                        .ToList();

                    // Rebuild prompt with reranked docs
                    systemPromptText = ChatAgentPrompts.BuildSystemPrompt(isLawyer, retrievedLawArticles, caseContextText);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Reranker failed for conversation {ConversationId}; using unranked results", conversationId);
                }
            }
            rerankSw.Stop();

            var llmSw = System.Diagnostics.Stopwatch.StartNew();
            var aiResponse = await _chatModelProvider.GenerateAsync(
                systemPromptText,
                userPromptText,
                cancellationToken);
            llmSw.Stop();
                
            var aiResponseText = aiResponse.Content;

            if (string.IsNullOrWhiteSpace(aiResponseText))
            {
                aiResponseText = "تمت مراجعة طلبك، ويرجى توضيح السؤال للحصول على تفاصيل أكثر.";
            }
            else
            {
                aiResponseText = SanitizeMarkdown(aiResponseText);
            }

            if (isClient && currentUserId.HasValue)
            {
                int chatTokens = aiResponse.Usage?.TotalTokens ?? 0;
                if (chatTokens > 0)
                {
                    actualTokensUsed += chatTokens;
                }
                else
                {
                    int fallback = systemPromptText.Length + userPromptText.Length + aiResponseText.Length;
                    _logger?.LogWarning("Alibaba chat returned 0 usage. Using conservative ceiling: {Tokens}", fallback);
                    actualTokensUsed += fallback;
                }

                var usages = new ModelUsageRecord[]
                {
                    new("text-embedding-v4", embeddingTokens, 0),
                    new("qwen3-rerank", rerankTokens, 0),
                    new("qwen-flash", aiResponse.Usage?.InputTokens ?? 0, aiResponse.Usage?.OutputTokens ?? 0)
                };

                await _costCalculatorService.RecordUsageAndCostAsync(currentUserId.Value, conversationId, usages, "Singapore", cancellationToken);
            }
            else if (isLawyerCheck && currentUserId.HasValue)
            {
                int chatTokens = aiResponse.Usage?.TotalTokens ?? 0;
                if (chatTokens > 0)
                {
                    actualTokensUsed += chatTokens;
                }
                else
                {
                    int fallback = systemPromptText.Length + userPromptText.Length + aiResponseText.Length;
                    _logger?.LogWarning("Alibaba chat returned 0 usage. Using conservative ceiling: {Tokens}", fallback);
                    actualTokensUsed += fallback;
                }
                
                var usages = new ModelUsageRecord[]
                {
                    new("text-embedding-v4", embeddingTokens, 0),
                    new("qwen3-rerank", rerankTokens, 0),
                    new("qwen-flash", aiResponse.Usage?.InputTokens ?? 0, aiResponse.Usage?.OutputTokens ?? 0)
                };
                
                await _costCalculatorService.RecordUsageAndCostAsync(currentUserId.Value, conversationId, usages, "Singapore", cancellationToken);
            }

            var responseTime = _timeProvider.GetUtcNow();

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
                _ = Task.Run(() => TryGenerateTitleAsync(conversation.Id, request.Content, currentUserId));
            }
            else
            {
                await TryGenerateTitleAsync(conversation.Id, request.Content, currentUserId, cancellationToken);
            }

            totalSw.Stop();
            _logger?.LogInformation("ChatAgent Total Latency: Reranker {RerankerMs}ms. LLM {LlmMs}ms. Total Request {TotalMs}ms.",
                rerankSw.ElapsedMilliseconds, llmSw.ElapsedMilliseconds, totalSw.ElapsedMilliseconds);

            return new AgentMessageDto(
                assistantMessage.Id,
                assistantMessage.Role.ToString(),
                assistantMessage.Content,
                assistantMessage.CreatedAt);
        }
        finally
        {
            if (isClient && currentUserId.HasValue && totalReservation != null && totalReservation.TotalReservedTokens > 0)
            {
                await _quotaService.SettleQuotaAsync(currentUserId.Value, totalReservation, actualTokensUsed, CancellationToken.None);
            }
            else if (isLawyerCheck && currentUserId.HasValue && totalReservation != null && totalReservation.TotalReservedTokens > 0)
            {
                await _lawyerQuotaService.SettleQuotaAsync(currentUserId.Value, totalReservation, actualTokensUsed, CancellationToken.None);
            }
        }
    }

    private async Task TryGenerateTitleAsync(
        Guid conversationId,
        string userMessageContent,
        Guid? currentUserId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            ApplicationDbContext dbContext = _dbContext;
            IChatModelProvider chatModelProvider = _chatModelProvider;
            TimeProvider timeProvider = _timeProvider;
            IQuotaService quotaService = _quotaService;
            SmartCourt.Features.LawyerSubscription.ILawyerQuotaService lawyerQuotaService = _lawyerQuotaService;
            IHttpContextAccessor? httpContextAccessor = _httpContextAccessor;
            IServiceScope? scope = null;

            if (_serviceScopeFactory != null)
            {
                scope = _serviceScopeFactory.CreateScope();
                dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                chatModelProvider = scope.ServiceProvider.GetRequiredService<IChatModelProvider>();
                timeProvider = scope.ServiceProvider.GetService<TimeProvider>() ?? TimeProvider.System;
                quotaService = scope.ServiceProvider.GetRequiredService<IQuotaService>();
                lawyerQuotaService = scope.ServiceProvider.GetRequiredService<SmartCourt.Features.LawyerSubscription.ILawyerQuotaService>();
                httpContextAccessor = scope.ServiceProvider.GetService<IHttpContextAccessor>();
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
                var titleResponse = await chatModelProvider.GenerateAsync(systemPrompt, userMessageContent, cancellationToken);
                var generatedTitle = titleResponse.Content;

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

                    if (currentUserId.HasValue)
                    {
                        bool isClient = false;
                        bool isLawyer = false;
                        if (httpContextAccessor?.HttpContext?.User != null)
                        {
                            isClient = httpContextAccessor.HttpContext.User.IsInRole("Client");
                            isLawyer = httpContextAccessor.HttpContext.User.IsInRole("Lawyer");
                        }
                        else
                        {
                            // Fallback to checking the database if no http context available in background task
                            isClient = await dbContext.UserRoles.AnyAsync(ur => ur.UserId == currentUserId.Value && dbContext.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Client"), cancellationToken);
                            isLawyer = await dbContext.UserRoles.AnyAsync(ur => ur.UserId == currentUserId.Value && dbContext.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Lawyer"), cancellationToken);
                        }
                        
                        if (isClient)
                        {
                            int titleTokens = titleResponse.Usage?.TotalTokens ?? 0;
                            if (titleTokens <= 0)
                            {
                                titleTokens = (systemPrompt.Length + userMessageContent.Length + cleanTitle.Length) / 4;
                            }
                            await quotaService.ConsumeQuotaAsync(currentUserId.Value, titleTokens, cancellationToken);
                        }
                        else if (isLawyer)
                        {
                            int titleTokens = titleResponse.Usage?.TotalTokens ?? 0;
                            if (titleTokens <= 0)
                            {
                                titleTokens = (systemPrompt.Length + userMessageContent.Length + cleanTitle.Length) / 4;
                            }
                            await lawyerQuotaService.ConsumeQuotaAsync(currentUserId.Value, titleTokens, cancellationToken);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(cleanTitle))
                    {
                        var utcNow = timeProvider.GetUtcNow();
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
        var currentUserId = _currentUserService.UserId;

        var conversation = await _dbContext.AgentConversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted, cancellationToken);

        if (conversation is null)
        {
            throw new NotFoundException("المحادثة غير موجودة.");
        }

        if (conversation.UserId != null && conversation.UserId != currentUserId)
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
        var currentUserId = _currentUserService.UserId;

        var conversation = await _dbContext.AgentConversations
            .FirstOrDefaultAsync(c => c.Id == conversationId && !c.IsDeleted, cancellationToken);

        if (conversation is null)
        {
            throw new NotFoundException("المحادثة غير موجودة.");
        }

        if (conversation.UserId != null && conversation.UserId != currentUserId)
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
            var docsText = await ExtractCaseDocumentsTextAsync(caseEntity.Documents, conversation.CaseId.Value, cancellationToken);
            sb.Append(docsText);
        }

        var contextText = sb.ToString().Trim();
        var utcNow = _timeProvider.GetUtcNow();
        conversation.CacheCaseContext(contextText, utcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return contextText;
    }

    private async Task<(List<string> Articles, int EmbeddingTokens, int RerankerTokens)> RetrieveRelevantLawArticlesAsync(
        Task<SmartCourt.Interfaces.Providers.EmbeddingResponse> embeddingTask,
        string normalizedQuery,
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        List<string> retrievedLawArticles = [];
        int embTokens = 0;
        int rerankTokens = 0;
        try
        {
            var embeddingResponse = await embeddingTask;
            embTokens = embeddingResponse.InputTokens;
            var queryEmbeddings = embeddingResponse.Embeddings;
            
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
                        var topN = Math.Min(_ragOptions.RerankedCount, retrievedLawArticles.Count);
                        var rerankResponse = await _rerankerProvider.RerankAsync(
                            normalizedQuery, retrievedLawArticles, topN, cancellationToken);
                            
                        rerankTokens = rerankResponse.InputTokens;
                        var reranked = rerankResponse.Results;

                        retrievedLawArticles = reranked
                            .Where(r => r.Index >= 0 && r.Index < retrievedLawArticles.Count)
                            .OrderByDescending(r => r.RelevanceScore)
                            .Select(r => retrievedLawArticles[r.Index])
                            .ToList();
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
        return (retrievedLawArticles, embTokens, rerankTokens);
    }

    private async Task<bool> IsUserLawyerAsync(Guid? currentUserId, CancellationToken cancellationToken)
    {
        bool isLawyer = _httpContextAccessor?.HttpContext?.User?.IsInRole("Lawyer") == true;
        if (!isLawyer && _httpContextAccessor?.HttpContext?.User?.IsInRole("Client") != true && currentUserId.HasValue)
        {
            isLawyer = await _dbContext.UserRoles
                .AnyAsync(ur => ur.UserId == currentUserId.Value &&
                    _dbContext.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Lawyer"),
                    cancellationToken);
        }
        return isLawyer;
    }

    private static string BuildUserPrompt(List<AgentMessage> historyMessages, string userContent)
    {
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

        userPromptBuilder.AppendLine($"المستخدم: {userContent}");
        return userPromptBuilder.ToString();
    }

    private async Task<string> ExtractCaseDocumentsTextAsync(ICollection<SmartCourt.Entities.CaseDocument> documents, Guid caseId, CancellationToken cancellationToken)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var doc in documents)
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
                        using var stream = new System.IO.MemoryStream(fileBytes);
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
                    _logger?.LogWarning(ex, "Failed to download or parse document {FileName} for CaseId {CaseId}", name, caseId);
                    sb.AppendLine("[تعذر استخراج محتوى هذا المستند]");
                }
            }
            else
            {
                sb.AppendLine("[لا يوجد مسار تخزين للمستند]");
            }
        }
        return sb.ToString();
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

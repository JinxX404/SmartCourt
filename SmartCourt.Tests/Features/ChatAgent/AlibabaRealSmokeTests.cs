using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Configuration;
using SmartCourt.Common.Domain;
using SmartCourt.Features.ChatAgent;
using SmartCourt.Features.ChatAgent.DTOs;
using SmartCourt.Features.ChatAgent.Entities;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using SmartCourt.Providers.ChatModel;
using SmartCourt.Providers.Embedding;
using SmartCourt.Providers.Payments;
using SmartCourt.Providers.Reranker;
using SmartCourt.Tests.TestDoubles;
using Xunit;
using Xunit.Abstractions;

namespace SmartCourt.Tests.Features.ChatAgent;

public class AlibabaRealSmokeTests
{
    private readonly ITestOutputHelper _output;
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;

    public AlibabaRealSmokeTests(ITestOutputHelper output)
    {
        _output = output;
        
        var projectDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../../../../SmartCourt"));
        
        _config = new ConfigurationBuilder()
            .SetBasePath(projectDir)
            .AddJsonFile("appsettings.Development.json", optional: false)
            .Build();
            
        _httpClient = new HttpClient();
    }

    private AlibabaEmbeddingProvider CreateEmbeddingProvider()
    {
        var options = new AlibabaEmbeddingOptions
        {
            ApiKey = _config["AlibabaEmbedding:ApiKey"]!,
            Model = _config["AlibabaEmbedding:Model"]!,
            Dimensions = int.Parse(_config["AlibabaEmbedding:Dimensions"]!),
            BaseUrl = _config["AlibabaEmbedding:BaseUrl"]!
        };
        return new AlibabaEmbeddingProvider(new HttpClient(), Options.Create(options), NullLogger<AlibabaEmbeddingProvider>.Instance);
    }

    private AlibabaRerankerProvider CreateRerankerProvider()
    {
        var options = new AlibabaRerankerOptions
        {
            ApiKey = _config["AlibabaReranker:ApiKey"]!,
            Model = _config["AlibabaReranker:Model"]!,
            BaseUrl = _config["AlibabaReranker:BaseUrl"]!
        };
        return new AlibabaRerankerProvider(new HttpClient(), Options.Create(options), NullLogger<AlibabaRerankerProvider>.Instance);
    }

    private AlibabaChatModelProvider CreateChatProvider()
    {
        var options = new AlibabaChatModelOptions
        {
            ApiKey = _config["AlibabaChatModel:ApiKey"]!,
            Model = _config["AlibabaChatModel:Model"]!,
            BaseUrl = _config["AlibabaChatModel:BaseUrl"]!,
            MaxTokens = int.Parse(_config["AlibabaChatModel:MaxTokens"]!)
        };
        return new AlibabaChatModelProvider(new HttpClient(), Options.Create(options), NullLogger<AlibabaChatModelProvider>.Instance);
    }

    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("SmokeTestDb_" + Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    [Trait("Category", "Smoke")]
    public async Task TestA_Embedding_RealRequest_ReturnsUsage()
    {
        var provider = CreateEmbeddingProvider();
        
        var response = await provider.GenerateEmbeddingsAsync(new[] { "محكمة النقض المصرية" }, CancellationToken.None);
        
        _output.WriteLine("Embedding:");
        _output.WriteLine("  Model: text-embedding-v4");
        _output.WriteLine($"  Usage returned: {(response.InputTokens > 0 ? "YES" : "NO")}");
        _output.WriteLine($"  Total tokens: {response.InputTokens}");
        
        Assert.NotNull(response);
        Assert.NotEmpty(response.Embeddings);
        
        if (response.InputTokens <= 0)
        {
            Assert.Fail("Alibaba API returned missing or zero usage for text-embedding-v4.");
        }
    }

    [Fact]
    [Trait("Category", "Smoke")]
    public async Task TestB_Reranker_RealRequest_ReturnsUsage()
    {
        var provider = CreateRerankerProvider();
        
        var documents = new[]
        {
            "المادة 1 من القانون المدني تنص على تسري النصوص التشريعية على جميع المسائل.",
            "الجو مشمس اليوم في القاهرة.",
            "قانون العقوبات يعاقب على السرقة بالحبس."
        };
        
        var response = await provider.RerankAsync("قانون مدني", documents, 3, CancellationToken.None);
        
        _output.WriteLine("Reranker:");
        _output.WriteLine("  Model: qwen3-rerank");
        _output.WriteLine($"  Usage returned: {(response.InputTokens > 0 ? "YES" : "NO")}");
        _output.WriteLine($"  Total tokens: {response.InputTokens}");
        
        Assert.NotNull(response);
        Assert.NotEmpty(response.Results);
        
        if (response.InputTokens <= 0)
        {
            Assert.Fail("Alibaba API returned missing or zero usage for qwen3-rerank.");
        }
    }

    [Fact]
    [Trait("Category", "Smoke")]
    public async Task TestC_Chat_RealRequest_ReturnsUsage()
    {
        var provider = CreateChatProvider();
        
        var response = await provider.GenerateAsync("You are a helpful assistant.", "Explain what a contract is in one sentence.", CancellationToken.None);
        
        _output.WriteLine("Chat:");
        _output.WriteLine("  Model: qwen-flash");
        _output.WriteLine($"  Usage returned: {(response.Usage?.TotalTokens > 0 ? "YES" : "NO")}");
        _output.WriteLine($"  Input tokens: {response.Usage?.InputTokens}");
        _output.WriteLine($"  Output tokens: {response.Usage?.OutputTokens}");
        _output.WriteLine($"  Total tokens: {response.Usage?.TotalTokens}");
        
        Assert.NotNull(response);
        Assert.False(string.IsNullOrWhiteSpace(response.Content));
        
        if (response.Usage == null || response.Usage.InputTokens <= 0 || response.Usage.OutputTokens <= 0 || response.Usage.TotalTokens <= 0)
        {
            Assert.Fail("Alibaba API returned missing or zero usage for qwen-flash.");
        }
        
        Assert.Equal(response.Usage.InputTokens + response.Usage.OutputTokens, response.Usage.TotalTokens);
    }

    [Fact]
    [Trait("Category", "Smoke")]
    public async Task TestD_ChatAgentPipeline_EndToEnd()
    {
        var dbContext = CreateInMemoryDbContext();
        var userId = Guid.NewGuid();
        var conversation = AgentConversation.Create(Guid.NewGuid(), userId, caseId: null, DateTimeOffset.UtcNow);
        dbContext.AgentConversations.Add(conversation);
        await dbContext.SaveChangesAsync();

        var quotaTracker = new FakeQuotaService(1000000);
        
        var reqContext = new DefaultHttpContext();
        reqContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Client") }));

        var service = new ChatAgentService(
            dbContext,
            new TestCurrentUserService { UserId = userId },
            CreateChatProvider(),
            CreateEmbeddingProvider(),
            new TestVectorStoreProvider { SearchResultsToReturn = new List<VectorSearchResult> { new(Guid.NewGuid(), 0.95f, new Dictionary<string, object> { { "text", "محكمة النقض قررت كذا كذا" } }) } },
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            quotaTracker,
            new TestCostCalculatorService(),
            CreateRerankerProvider(),
            Options.Create(new RagOptions { MinimumSimilarityScore = 0.5f, CandidateCount = 5, RerankedCount = 2 }),
            new HttpContextAccessor { HttpContext = reqContext },
            TimeProvider.System,
            NullLogger<ChatAgentService>.Instance);

        var initialQuota = await quotaTracker.GetQuotaAsync(userId);
        _output.WriteLine($"Initial Quota Credits: {initialQuota.TotalRemainingCredits}");

        await service.SendMessageAsync(conversation.Id, new SendAgentMessageRequest("ما هي محكمة النقض؟"));

        var finalQuota = await quotaTracker.GetQuotaAsync(userId);
        _output.WriteLine($"Final Quota Credits: {finalQuota.TotalRemainingCredits}");
        _output.WriteLine($"Reserved: {quotaTracker.LastReserved}");
        _output.WriteLine($"Actual Used: {quotaTracker.LastActualUsed}");
        _output.WriteLine($"Refunded: {quotaTracker.LastReserved - quotaTracker.LastActualUsed}");

        Assert.True(quotaTracker.LastReserved > 0);
        Assert.True(quotaTracker.LastActualUsed > 0);
        Assert.True(quotaTracker.LastReserved >= quotaTracker.LastActualUsed);
        
        Assert.True(CreditConverter.ToTokens(finalQuota.TotalRemainingCredits) <= CreditConverter.ToTokens(initialQuota.TotalRemainingCredits) - quotaTracker.LastActualUsed);
    }

    [Fact]
    [Trait("Category", "Smoke")]
    public async Task TestE_PartialFailure_ReflectsAccurateUsage()
    {
        var dbContext = CreateInMemoryDbContext();
        var userId = Guid.NewGuid();
        var conversation = AgentConversation.Create(Guid.NewGuid(), userId, caseId: null, DateTimeOffset.UtcNow);
        dbContext.AgentConversations.Add(conversation);
        await dbContext.SaveChangesAsync();

        var quotaTracker = new FakeQuotaService(1000000);
        var reqContext = new DefaultHttpContext();
        reqContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Client") }));

        var failingChatProvider = new FaultyChatProvider();

        var service = new ChatAgentService(
            dbContext,
            new TestCurrentUserService { UserId = userId },
            failingChatProvider,
            CreateEmbeddingProvider(),
            new TestVectorStoreProvider { SearchResultsToReturn = new List<VectorSearchResult> { new(Guid.NewGuid(), 0.95f, new Dictionary<string, object> { { "text", "محكمة النقض قررت كذا كذا" } }) } },
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            quotaTracker,
            new TestCostCalculatorService(),
            CreateRerankerProvider(),
            Options.Create(new RagOptions { MinimumSimilarityScore = 0.5f, CandidateCount = 5, RerankedCount = 2 }),
            new HttpContextAccessor { HttpContext = reqContext },
            TimeProvider.System,
            NullLogger<ChatAgentService>.Instance);

        var initialQuota = await quotaTracker.GetQuotaAsync(userId);

        await Assert.ThrowsAsync<Exception>(() => service.SendMessageAsync(conversation.Id, new SendAgentMessageRequest("Hello")));

        var finalQuota = await quotaTracker.GetQuotaAsync(userId);
        
        Assert.True(quotaTracker.LastActualUsed > 0);
        Assert.True(quotaTracker.LastReserved > quotaTracker.LastActualUsed);
        Assert.True(CreditConverter.ToTokens(finalQuota.TotalRemainingCredits) <= CreditConverter.ToTokens(initialQuota.TotalRemainingCredits) - quotaTracker.LastActualUsed);
    }

    [Fact]
    [Trait("Category", "Smoke")]
    public async Task TestF_MissingUsage_ReflectsFallback()
    {
        var dbContext = CreateInMemoryDbContext();
        var userId = Guid.NewGuid();
        var conversation = AgentConversation.Create(Guid.NewGuid(), userId, caseId: null, DateTimeOffset.UtcNow);
        dbContext.AgentConversations.Add(conversation);
        await dbContext.SaveChangesAsync();

        var quotaTracker = new FakeQuotaService(1000000);
        var reqContext = new DefaultHttpContext();
        reqContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, "Client") }));

        var missingUsageChatProvider = new MissingUsageChatProvider();

        var service = new ChatAgentService(
            dbContext,
            new TestCurrentUserService { UserId = userId },
            missingUsageChatProvider,
            CreateEmbeddingProvider(),
            new TestVectorStoreProvider { SearchResultsToReturn = new List<VectorSearchResult> { new(Guid.NewGuid(), 0.95f, new Dictionary<string, object> { { "text", "test docs" } }) } },
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            quotaTracker,
            new TestCostCalculatorService(),
            CreateRerankerProvider(),
            Options.Create(new RagOptions { MinimumSimilarityScore = 0.5f, CandidateCount = 5, RerankedCount = 2 }),
            new HttpContextAccessor { HttpContext = reqContext },
            TimeProvider.System,
            NullLogger<ChatAgentService>.Instance);

        var initialQuota = await quotaTracker.GetQuotaAsync(userId);
        await service.SendMessageAsync(conversation.Id, new SendAgentMessageRequest("Hello"));
        var finalQuota = await quotaTracker.GetQuotaAsync(userId);
        
        Assert.True(quotaTracker.LastActualUsed > 0);
        Assert.True(CreditConverter.ToTokens(finalQuota.TotalRemainingCredits) <= CreditConverter.ToTokens(initialQuota.TotalRemainingCredits) - quotaTracker.LastActualUsed);
    }

    private class FaultyChatProvider : IChatModelProvider
    {
        public Task<ChatModelResponse> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
        {
            throw new Exception("Chat API failed unexpectedly!");
        }
    }

    private class MissingUsageChatProvider : IChatModelProvider
    {
        public Task<ChatModelResponse> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
        {
            // Missing usage metadata intentionally
            return Task.FromResult(new ChatModelResponse("Dummy response", new TokenUsageMetadata(0, 0, 0, "test-model")));
        }
    }

    private class FakeQuotaService : IQuotaService
    {
        private int _balance;
        public int LastReserved { get; private set; }
        public int LastActualUsed { get; private set; }

        public FakeQuotaService(int initialBalance) => _balance = initialBalance;

        public Task<QuotaReservation> ConsumeQuotaAsync(Guid clientId, int tokenAmount, CancellationToken cancellationToken = default)
        {
            if (_balance < tokenAmount) throw new SmartCourt.Common.Exceptions.BusinessException("لقد استنفدت رصيد الكلمات (الرموز) المتاح لك.");
            _balance -= tokenAmount;
            return Task.FromResult(new QuotaReservation { TotalReservedTokens = tokenAmount, FreeReservedTokens = tokenAmount, PaidReservedTokens = 0 });
        }
        
        public Task<QuotaInfoResponse> GetQuotaAsync(Guid clientId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new QuotaInfoResponse(
                CreditConverter.ToCredits(5000000), 
                CreditConverter.ToCredits(5000000 - _balance), 
                CreditConverter.ToCredits(_balance), 
                0, 
                CreditConverter.ToCredits(_balance), 
                DateTimeOffset.UtcNow));
        }

        public Task<QuotaTransactionListDto> GetQuotaTransactionsAsync(Guid clientId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new QuotaTransactionListDto(new List<QuotaTransactionDto>(), 0));
        }
        public Task RefundAsync(Guid clientId, int tokenAmount, CancellationToken cancellationToken = default)
        {
            _balance += tokenAmount;
            return Task.CompletedTask;
        }

        public Task<QuotaHistoryResponse> GetQuotaHistoryAsync(Guid clientId, int days, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new QuotaHistoryResponse(new List<DailyQuotaUsageDto>()));
        }

        public Task<QuotaReservation> ReserveQuotaAsync(Guid clientId, int estimatedMaxTokens, CancellationToken cancellationToken = default)
        {
            LastReserved += estimatedMaxTokens;
            return ConsumeQuotaAsync(clientId, estimatedMaxTokens, cancellationToken);
        }

        public Task SettleQuotaAsync(Guid clientId, QuotaReservation reservation, int actualTokensUsed, CancellationToken cancellationToken = default)
        {
            LastActualUsed += actualTokensUsed;
            int unusedTokens = reservation.TotalReservedTokens - actualTokensUsed;
            if (unusedTokens > 0)
            {
                _balance += unusedTokens;
            }
            return Task.CompletedTask;
        }

        public Task<DefaultQuotaResponse> GetDefaultQuotaAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new DefaultQuotaResponse(100));
        }
    }
}

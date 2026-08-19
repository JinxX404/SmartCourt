using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SmartCourt.Common.Configuration;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.ChatAgent;
using SmartCourt.Features.ChatAgent.DTOs;
using SmartCourt.Features.ChatAgent.Entities;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using SmartCourt.Tests.TestDoubles;
using Xunit;

namespace SmartCourt.Tests.Features.ChatAgent;

public class ChatAgentQuotaTests
{
    private static DbContextOptions<ApplicationDbContext> CreateInMemoryOptions()
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
    }

    private static IHttpContextAccessor CreateClientHttpContextAccessor()
    {
        var context = new DefaultHttpContext();
        var claims = new[] { new Claim(ClaimTypes.Role, "Client") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        context.User = new ClaimsPrincipal(identity);

        return new HttpContextAccessor { HttpContext = context };
    }

    private class TrackingQuotaService : IQuotaService
    {
        public int TotalReserved { get; private set; }
        public int SettledTotalReserved { get; private set; }
        public int SettledActualUsed { get; private set; }
        public bool SettleCalled { get; private set; }
        public bool ShouldThrowOnReserve { get; set; }

        public Task<QuotaReservation> ConsumeQuotaAsync(Guid clientId, int tokenAmount, CancellationToken cancellationToken = default) => Task.FromResult(new QuotaReservation { TotalReservedTokens = tokenAmount, FreeReservedTokens = tokenAmount, PaidReservedTokens = 0 });

        public Task<QuotaInfoResponse> GetQuotaAsync(Guid clientId, CancellationToken cancellationToken = default) => Task.FromResult(new QuotaInfoResponse(100, 0, 100, 0, 100, DateTimeOffset.UtcNow));

        public Task RefundAsync(Guid clientId, int tokenAmount, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<QuotaHistoryResponse> GetQuotaHistoryAsync(Guid clientId, int days, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new QuotaHistoryResponse(new List<DailyQuotaUsageDto>()));
        }

        public Task<QuotaTransactionListDto> GetQuotaTransactionsAsync(Guid clientId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new QuotaTransactionListDto(new List<QuotaTransactionDto>(), 0));
        }

        public Task<QuotaReservation> ReserveQuotaAsync(Guid clientId, int estimatedMaxTokens, CancellationToken cancellationToken = default)
        {
            if (ShouldThrowOnReserve) throw new InsufficientQuotaException(100, 100, estimatedMaxTokens, DateTimeOffset.UtcNow.AddDays(1));
            TotalReserved += estimatedMaxTokens;
            return Task.FromResult(new QuotaReservation { TotalReservedTokens = estimatedMaxTokens, FreeReservedTokens = estimatedMaxTokens, PaidReservedTokens = 0 });
        }

        public Task SettleQuotaAsync(Guid clientId, QuotaReservation reservation, int actualTokensUsed, CancellationToken cancellationToken = default)
        {
            SettleCalled = true;
            SettledTotalReserved = reservation.TotalReservedTokens;
            SettledActualUsed = actualTokensUsed;
            return Task.CompletedTask;
        }
    }

    private class ConfigurableProviders
    {
        public bool ThrowOnEmbedding { get; set; }
        public bool ThrowOnRerank { get; set; }
        public bool ThrowOnChat { get; set; }

        public int EmbeddingUsageToReturn { get; set; } = 150;
        public int RerankUsageToReturn { get; set; } = 250;
        public int ChatTotalUsageToReturn { get; set; } = 800;
        public int ChatInputUsageToReturn { get; set; } = 300;
        public int ChatOutputUsageToReturn { get; set; } = 500;
    }

    private class TestableEmbeddingProvider(ConfigurableProviders config) : IEmbeddingProvider
    {
        public int Dimensions => 1536;
        public Task<EmbeddingResponse> GenerateEmbeddingsAsync(IReadOnlyList<string> texts, CancellationToken cancellationToken = default)
        {
            if (config.ThrowOnEmbedding) throw new Exception("Embedding failed");
            return Task.FromResult(new EmbeddingResponse(new List<float[]> { new float[1536] }, config.EmbeddingUsageToReturn));
        }
    }

    private class TestableRerankerProvider(ConfigurableProviders config) : IRerankerProvider
    {
        public Task<RerankResponse> RerankAsync(string query, IReadOnlyList<string> documents, int topN, CancellationToken cancellationToken = default)
        {
            if (config.ThrowOnRerank) throw new Exception("Reranker failed");
            var results = documents.Take(topN).Select((d, i) => new RerankedResult(i, 0.9f)).ToList();
            return Task.FromResult(new RerankResponse(results, config.RerankUsageToReturn));
        }
    }

    private class TestableChatModelProvider(ConfigurableProviders config) : IChatModelProvider
    {
        public Task<ChatModelResponse> GenerateAsync(string systemPrompt, string userPrompt, CancellationToken cancellationToken = default)
        {
            if (config.ThrowOnChat) throw new Exception("Chat failed");
            var metadata = new TokenUsageMetadata(config.ChatInputUsageToReturn, config.ChatOutputUsageToReturn, config.ChatTotalUsageToReturn, "model");
            return Task.FromResult(new ChatModelResponse("AI response", metadata));
        }
    }

    [Fact]
    public async Task SendMessage_HappyPath_AccuratelyAccumulatesTokens()
    {
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);
        dbContext.Database.EnsureCreated();

        var userId = Guid.NewGuid();
        var conversation = AgentConversation.Create(Guid.NewGuid(), userId, caseId: null, DateTimeOffset.UtcNow);
        dbContext.AgentConversations.Add(conversation);
        await dbContext.SaveChangesAsync();

        var quotaTracker = new TrackingQuotaService();
        var config = new ConfigurableProviders();

        var service = new ChatAgentService(
            dbContext,
            new TestCurrentUserService { UserId = userId },
            new TestableChatModelProvider(config),
            new TestableEmbeddingProvider(config),
            new TestVectorStoreProvider { SearchResultsToReturn = new List<VectorSearchResult> { new(Guid.NewGuid(), 0.95f, new Dictionary<string, object> { { "text", "Law article" } }) } },
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            quotaTracker,
            new TestCostCalculatorService(),
            new TestableRerankerProvider(config),
            Microsoft.Extensions.Options.Options.Create(new RagOptions()),
            CreateClientHttpContextAccessor(),
            TimeProvider.System,
            NullLogger<ChatAgentService>.Instance);

        await service.SendMessageAsync(conversation.Id, new SendAgentMessageRequest("Test Request"));

        Assert.True(quotaTracker.SettleCalled);
        Assert.Equal(150 + 250 + 800, quotaTracker.SettledActualUsed);
        Assert.Equal(quotaTracker.TotalReserved, quotaTracker.SettledTotalReserved);
        Assert.True(quotaTracker.TotalReserved > 0);
    }

    [Fact]
    public async Task SendMessage_MissingAlibabaUsage_UsesConservativeCeiling()
    {
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);
        dbContext.Database.EnsureCreated();

        var userId = Guid.NewGuid();
        var conversation = AgentConversation.Create(Guid.NewGuid(), userId, caseId: null, DateTimeOffset.UtcNow);
        dbContext.AgentConversations.Add(conversation);
        await dbContext.SaveChangesAsync();

        var quotaTracker = new TrackingQuotaService();
        var config = new ConfigurableProviders
        {
            EmbeddingUsageToReturn = 0,
            RerankUsageToReturn = 0,
            ChatTotalUsageToReturn = 0,
            ChatInputUsageToReturn = 0,
            ChatOutputUsageToReturn = 0
        };

        var service = new ChatAgentService(
            dbContext,
            new TestCurrentUserService { UserId = userId },
            new TestableChatModelProvider(config),
            new TestableEmbeddingProvider(config),
            new TestVectorStoreProvider { SearchResultsToReturn = new List<VectorSearchResult> { new(Guid.NewGuid(), 0.95f, new Dictionary<string, object> { { "text", "Law article" } }) } },
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            quotaTracker,
            new TestCostCalculatorService(),
            new TestableRerankerProvider(config),
            Microsoft.Extensions.Options.Options.Create(new RagOptions()),
            CreateClientHttpContextAccessor(),
            TimeProvider.System,
            NullLogger<ChatAgentService>.Instance);

        await service.SendMessageAsync(conversation.Id, new SendAgentMessageRequest("Test Request"));

        Assert.True(quotaTracker.SettleCalled);
        Assert.True(quotaTracker.SettledActualUsed > 0);
    }

    [Fact]
    public async Task SendMessage_EmbeddingThrowsException_RefundsStage1Reservation()
    {
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);
        dbContext.Database.EnsureCreated();

        var userId = Guid.NewGuid();
        var conversation = AgentConversation.Create(Guid.NewGuid(), userId, caseId: null, DateTimeOffset.UtcNow);
        dbContext.AgentConversations.Add(conversation);
        await dbContext.SaveChangesAsync();

        var quotaTracker = new TrackingQuotaService();
        var config = new ConfigurableProviders { ThrowOnEmbedding = true };

        var service = new ChatAgentService(
            dbContext,
            new TestCurrentUserService { UserId = userId },
            new TestableChatModelProvider(config),
            new TestableEmbeddingProvider(config),
            new TestVectorStoreProvider(),
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            quotaTracker,
            new TestCostCalculatorService(),
            new TestableRerankerProvider(config),
            Microsoft.Extensions.Options.Options.Create(new RagOptions()),
            CreateClientHttpContextAccessor(),
            TimeProvider.System,
            NullLogger<ChatAgentService>.Instance);

        await Assert.ThrowsAsync<Exception>(() => service.SendMessageAsync(conversation.Id, new SendAgentMessageRequest("Test")));

        Assert.True(quotaTracker.SettleCalled);
        Assert.Equal(0, quotaTracker.SettledActualUsed);
        Assert.True(quotaTracker.SettledTotalReserved > 0);
    }

    [Fact]
    public async Task SendMessage_RerankerThrowsException_RefundsStage2ButChargesStage1()
    {
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);
        dbContext.Database.EnsureCreated();

        var userId = Guid.NewGuid();
        var conversation = AgentConversation.Create(Guid.NewGuid(), userId, caseId: null, DateTimeOffset.UtcNow);
        dbContext.AgentConversations.Add(conversation);
        await dbContext.SaveChangesAsync();

        var quotaTracker = new TrackingQuotaService();
        var config = new ConfigurableProviders { ThrowOnRerank = true };

        var service = new ChatAgentService(
            dbContext,
            new TestCurrentUserService { UserId = userId },
            new TestableChatModelProvider(config),
            new TestableEmbeddingProvider(config),
            new TestVectorStoreProvider { SearchResultsToReturn = new List<VectorSearchResult> { new(Guid.NewGuid(), 0.95f, new Dictionary<string, object> { { "text", "Law article" } }) } },
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            quotaTracker,
            new TestCostCalculatorService(),
            new TestableRerankerProvider(config),
            Microsoft.Extensions.Options.Options.Create(new RagOptions()),
            CreateClientHttpContextAccessor(),
            TimeProvider.System,
            NullLogger<ChatAgentService>.Instance);

        // We use Assert.ThrowsAsync<Exception> assuming rerank failure escapes, but in ChatAgentService, 
        // the reranker catch block logs and continues without throwing! 
        // Let's actually test that Reranker failing doesn't crash the pipeline, 
        // but DOES NOT charge Reranker usage!
        await service.SendMessageAsync(conversation.Id, new SendAgentMessageRequest("Test"));

        Assert.True(quotaTracker.SettleCalled);
        Assert.Equal(150 + 800, quotaTracker.SettledActualUsed); // Only Embedding and Chat charged
    }

    [Fact]
    public async Task SendMessage_ChatThrowsException_RefundsStage2ButChargesStage1AndReranker()
    {
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);
        dbContext.Database.EnsureCreated();

        var userId = Guid.NewGuid();
        var conversation = AgentConversation.Create(Guid.NewGuid(), userId, caseId: null, DateTimeOffset.UtcNow);
        dbContext.AgentConversations.Add(conversation);
        await dbContext.SaveChangesAsync();

        var quotaTracker = new TrackingQuotaService();
        var config = new ConfigurableProviders { ThrowOnChat = true };

        var service = new ChatAgentService(
            dbContext,
            new TestCurrentUserService { UserId = userId },
            new TestableChatModelProvider(config),
            new TestableEmbeddingProvider(config),
            new TestVectorStoreProvider { SearchResultsToReturn = new List<VectorSearchResult> { new(Guid.NewGuid(), 0.95f, new Dictionary<string, object> { { "text", "Law article" } }) } },
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            quotaTracker,
            new TestCostCalculatorService(),
            new TestableRerankerProvider(config),
            Microsoft.Extensions.Options.Options.Create(new RagOptions()),
            CreateClientHttpContextAccessor(),
            TimeProvider.System,
            NullLogger<ChatAgentService>.Instance);

        await Assert.ThrowsAsync<Exception>(() => service.SendMessageAsync(conversation.Id, new SendAgentMessageRequest("Test")));

        Assert.True(quotaTracker.SettleCalled);
        // Ensure that embedding and reranker were successfully charged before chat crashed
        Assert.Equal(150 + 250, quotaTracker.SettledActualUsed);
        Assert.Equal(quotaTracker.TotalReserved, quotaTracker.SettledTotalReserved); // Full reservation passed down to be partially refunded
    }

    [Fact]
    public async Task SendMessage_InsufficientQuota_ThrowsBusinessException()
    {
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);
        dbContext.Database.EnsureCreated();

        var userId = Guid.NewGuid();
        var conversation = AgentConversation.Create(Guid.NewGuid(), userId, caseId: null, DateTimeOffset.UtcNow);
        dbContext.AgentConversations.Add(conversation);
        await dbContext.SaveChangesAsync();

        var quotaTracker = new TrackingQuotaService { ShouldThrowOnReserve = true };
        var config = new ConfigurableProviders();

        var service = new ChatAgentService(
            dbContext,
            new TestCurrentUserService { UserId = userId },
            new TestableChatModelProvider(config),
            new TestableEmbeddingProvider(config),
            new TestVectorStoreProvider(),
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            quotaTracker,
            new TestCostCalculatorService(),
            new TestableRerankerProvider(config),
            Microsoft.Extensions.Options.Options.Create(new RagOptions()),
            CreateClientHttpContextAccessor(),
            TimeProvider.System,
            NullLogger<ChatAgentService>.Instance);

        await Assert.ThrowsAsync<InsufficientQuotaException>(() => service.SendMessageAsync(conversation.Id, new SendAgentMessageRequest("Test")));

        Assert.False(quotaTracker.SettleCalled); // Settlement shouldn't occur if reservation throws
    }

    [Fact]
    public async Task ConsumeQuota_ExactBoundary_Succeeds()
    {
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);
        dbContext.Database.EnsureCreated();

        var userId = Guid.NewGuid();
        var dateStr = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3)).ToString("yyyy-MM-dd");
        
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
        var midnight = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
        var date = new DateTimeOffset(midnight, tz.GetUtcOffset(midnight));
        var usage = DailyUsage.Create(userId, date);
        usage.ConsumedTokens = 99999;
        dbContext.DailyUsages.Add(usage);
        await dbContext.SaveChangesAsync();

        var service = new QuotaService(dbContext, TimeProvider.System, NullLogger<QuotaService>.Instance, Microsoft.Extensions.Options.Options.Create(new QuotaOptions { DailyFreeTokens = 100000, Timezone = "Egypt Standard Time" }));

        // Request 1
        await service.ConsumeQuotaAsync(userId, 1, CancellationToken.None);

        var updated = await dbContext.DailyUsages.AsNoTracking().FirstAsync(x => x.ClientId == userId);
        Assert.Equal(100000, updated.ConsumedTokens);
    }

    [Fact]
    public async Task ConsumeQuota_ExactBoundaryExceeded_Throws()
    {
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);
        dbContext.Database.EnsureCreated();

        var userId = Guid.NewGuid();
        var dateStr = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3)).ToString("yyyy-MM-dd");
        
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
        var midnight = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
        var date = new DateTimeOffset(midnight, tz.GetUtcOffset(midnight));
        var usage = DailyUsage.Create(userId, date);
        usage.ConsumedTokens = 100000;
        dbContext.DailyUsages.Add(usage);
        await dbContext.SaveChangesAsync();

        var service = new QuotaService(dbContext, TimeProvider.System, NullLogger<QuotaService>.Instance, Microsoft.Extensions.Options.Options.Create(new QuotaOptions { DailyFreeTokens = 100000, Timezone = "Egypt Standard Time" }));

        // Request 1
        await Assert.ThrowsAsync<InsufficientQuotaException>(() => service.ConsumeQuotaAsync(userId, 1, CancellationToken.None));

        var updated = await dbContext.DailyUsages.AsNoTracking().FirstAsync(x => x.ClientId == userId);
        Assert.Equal(100000, updated.ConsumedTokens); // Unchanged
    }

    [Fact]
    public async Task ConsumeQuota_LargeExactBoundary_Succeeds()
    {
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);
        dbContext.Database.EnsureCreated();

        var userId = Guid.NewGuid();
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
        var midnight = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
        var date = new DateTimeOffset(midnight, tz.GetUtcOffset(midnight));
        var usage = DailyUsage.Create(userId, date);
        usage.ConsumedTokens = 50000;
        dbContext.DailyUsages.Add(usage);
        await dbContext.SaveChangesAsync();

        var service = new QuotaService(dbContext, TimeProvider.System, NullLogger<QuotaService>.Instance, Microsoft.Extensions.Options.Options.Create(new QuotaOptions { DailyFreeTokens = 100000, Timezone = "Egypt Standard Time" }));

        await service.ConsumeQuotaAsync(userId, 50000, CancellationToken.None);

        var updated = await dbContext.DailyUsages.AsNoTracking().FirstAsync(x => x.ClientId == userId);
        Assert.Equal(100000, updated.ConsumedTokens);
    }

    [Fact]
    public async Task ConsumeQuota_LargeBoundaryExceeded_Throws()
    {
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);
        dbContext.Database.EnsureCreated();

        var userId = Guid.NewGuid();
        var tz = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
        var midnight = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).Date;
        var date = new DateTimeOffset(midnight, tz.GetUtcOffset(midnight));
        var usage = DailyUsage.Create(userId, date);
        usage.ConsumedTokens = 50000;
        dbContext.DailyUsages.Add(usage);
        await dbContext.SaveChangesAsync();

        var service = new QuotaService(dbContext, TimeProvider.System, NullLogger<QuotaService>.Instance, Microsoft.Extensions.Options.Options.Create(new QuotaOptions { DailyFreeTokens = 100000, Timezone = "Egypt Standard Time" }));

        await Assert.ThrowsAsync<InsufficientQuotaException>(() => service.ConsumeQuotaAsync(userId, 50001, CancellationToken.None));

        var updated = await dbContext.DailyUsages.AsNoTracking().FirstAsync(x => x.ClientId == userId);
        Assert.Equal(50000, updated.ConsumedTokens);
    }
}

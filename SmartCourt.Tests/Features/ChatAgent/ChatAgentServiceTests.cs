using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Entities;
using SmartCourt.Features.ChatAgent;
using SmartCourt.Features.ChatAgent.DTOs;
using SmartCourt.Features.ChatAgent.Entities;
using SmartCourt.Features.ChatAgent.Enums;
using SmartCourt.Persistence;
using SmartCourt.Tests.Mocks.Providers;
using SmartCourt.Tests.TestDoubles;
using Xunit;
using CaseEntity = SmartCourt.Entities.Case;

namespace SmartCourt.Tests.Features.ChatAgent;

public sealed class ChatAgentServiceTests
{
    private static DbContextOptions<ApplicationDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task CreateConversation_HappyPath_WithoutCase_CreatesConversation()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();
        var currentUserService = new TestCurrentUserService { UserId = userId };
        var chatModelProvider = new TestChatModelProvider();
        var embeddingProvider = new TestEmbeddingProvider();
        var vectorStoreProvider = new TestVectorStoreProvider();
        var fileStorageService = new TestFileStorageService();
        var documentParsingProvider = new TestDocumentParsingProvider();

        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            chatModelProvider,
            embeddingProvider,
            vectorStoreProvider,
            fileStorageService,
            documentParsingProvider,
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(), new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        var request = new CreateAgentConversationRequest(CaseId: null);

        // Act
        var result = await service.CreateConversationAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Null(result.CaseId);
        Assert.Null(result.CaseTitle);
        Assert.Null(result.Title);

        var savedConversation = await dbContext.AgentConversations.FirstOrDefaultAsync(c => c.Id == result.Id);
        Assert.NotNull(savedConversation);
        Assert.Equal(userId, savedConversation.UserId);
        Assert.False(savedConversation.IsDeleted);
    }

    [Fact]
    public async Task CreateConversation_HappyPath_WithCaseAsClient_CreatesConversation()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var clientId = Guid.NewGuid();
        var caseId = Guid.NewGuid();
        var caseEntity = new CaseEntity
        {
            Id = caseId,
            Title = "قضية تعويض نزاع عقاري",
            Description = "وصف القضية للاختبار",
            ClientId = clientId
        };
        dbContext.Cases.Add(caseEntity);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = clientId };
        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            new TestEmbeddingProvider(),
            new TestVectorStoreProvider(),
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(), new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        var request = new CreateAgentConversationRequest(CaseId: caseId);

        // Act
        var result = await service.CreateConversationAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(caseId, result.CaseId);
        Assert.Equal("قضية تعويض نزاع عقاري", result.CaseTitle);
    }

    [Fact]
    public async Task CreateConversation_HappyPath_WithCaseAsLawyer_CreatesConversation()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var clientId = Guid.NewGuid();
        var lawyerId = Guid.NewGuid();
        var caseId = Guid.NewGuid();
        var caseEntity = new CaseEntity
        {
            Id = caseId,
            Title = "دعوى عمالية",
            Description = "مطالبة بمستحقات مالية",
            ClientId = clientId,
            LawyerId = lawyerId
        };
        dbContext.Cases.Add(caseEntity);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = lawyerId };
        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            new TestEmbeddingProvider(),
            new TestVectorStoreProvider(),
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(), new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        var request = new CreateAgentConversationRequest(CaseId: caseId);

        // Act
        var result = await service.CreateConversationAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(caseId, result.CaseId);
        Assert.Equal("دعوى عمالية", result.CaseTitle);
    }

    [Fact]
    public async Task CreateConversation_UnauthenticatedUser_ThrowsAuthenticationException()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var currentUserService = new TestCurrentUserService { UserId = null };
        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            new TestEmbeddingProvider(),
            new TestVectorStoreProvider(),
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(), new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        var request = new CreateAgentConversationRequest(CaseId: null);

        // Act & Assert
        await Assert.ThrowsAsync<AuthenticationException>(() => service.CreateConversationAsync(request));
    }

    [Fact]
    public async Task CreateConversation_CaseNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();
        var currentUserService = new TestCurrentUserService { UserId = userId };
        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            new TestEmbeddingProvider(),
            new TestVectorStoreProvider(),
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(), new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        var request = new CreateAgentConversationRequest(CaseId: Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.CreateConversationAsync(request));
    }

    [Fact]
    public async Task CreateConversation_UnauthorizedCaseAccess_ThrowsForbiddenAccessException()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var caseOwnerId = Guid.NewGuid();
        var foreignUserId = Guid.NewGuid();
        var caseId = Guid.NewGuid();

        var caseEntity = new CaseEntity
        {
            Id = caseId,
            Title = "قضية شخصية",
            Description = "وصف",
            ClientId = caseOwnerId
        };
        dbContext.Cases.Add(caseEntity);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = foreignUserId };
        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            new TestEmbeddingProvider(),
            new TestVectorStoreProvider(),
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(), new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        var request = new CreateAgentConversationRequest(CaseId: caseId);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() => service.CreateConversationAsync(request));
    }

    [Fact]
    public async Task GetOrFetchCaseContext_UnlinkedConversation_ReturnsNull()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();
        var currentUserService = new TestCurrentUserService { UserId = userId };

        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            new TestEmbeddingProvider(),
            new TestVectorStoreProvider(),
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(), new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        var conversation = await service.CreateConversationAsync(new CreateAgentConversationRequest(CaseId: null));

        // Act
        var result = await service.GetOrFetchCaseContextAsync(conversation.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrFetchCaseContext_FirstAccess_DownloadsParsesAndCachesContext()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();
        var caseId = Guid.NewGuid();
        var storedFileId = Guid.NewGuid();

        var storedFile = new StoredFile
        {
            Id = storedFileId,
            StoredFileName = "contract_123.pdf",
            OriginalFileName = "عقد_إيجار.pdf",
            FileUrl = "cases/contract_123.pdf",
            ContentType = "application/pdf",
            Extension = ".pdf",
            SizeInBytes = 1024
        };
        dbContext.StoredFiles.Add(storedFile);

        var caseEntity = new CaseEntity
        {
            Id = caseId,
            Title = "نزاع إيجاري سكني",
            Description = "تأخر في السداد",
            Governorate = "القاهرة",
            City = "مدينة نصر",
            ClientId = userId,
            Documents = new[]
            {
                new CaseDocument
                {
                    Id = Guid.NewGuid(),
                    CaseId = caseId,
                    StoredFileId = storedFileId,
                    StoredFile = storedFile
                }
            }
        };
        dbContext.Cases.Add(caseEntity);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = userId };
        var fileStorage = new TestFileStorageService
        {
            DownloadBytesToReturn = System.Text.Encoding.UTF8.GetBytes("محتوى وهمي")
        };
        var parsingProvider = new TestDocumentParsingProvider
        {
            ExtractedTextToReturn = "البند الأول: يلتزم المستأجر بسداد الأجرة في أول كل شهر."
        };

        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            new TestEmbeddingProvider(),
            new TestVectorStoreProvider(),
            fileStorage,
            parsingProvider,
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(), new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        var conversationDto = await service.CreateConversationAsync(new CreateAgentConversationRequest(CaseId: caseId));

        // Act
        var context = await service.GetOrFetchCaseContextAsync(conversationDto.Id);

        // Assert
        Assert.NotNull(context);
        Assert.Contains("نزاع إيجاري سكني", context);
        Assert.Contains("عقد_إيجار.pdf", context);
        Assert.Contains("البند الأول: يلتزم المستأجر بسداد الأجرة", context);

        var dbConversation = await dbContext.AgentConversations.FirstOrDefaultAsync(c => c.Id == conversationDto.Id);
        Assert.NotNull(dbConversation);
        Assert.Equal(context, dbConversation.CachedCaseContext);
    }

    [Fact]
    public async Task GetOrFetchCaseContext_SecondAccess_ReturnsCachedContextWithoutRedownloading()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();
        var caseId = Guid.NewGuid();

        var caseEntity = new CaseEntity
        {
            Id = caseId,
            Title = "قضية تجارية",
            Description = "نزاع شراكة",
            ClientId = userId
        };
        dbContext.Cases.Add(caseEntity);

        var conversation = AgentConversation.Create(Guid.NewGuid(), userId, caseId, DateTimeOffset.UtcNow);
        conversation.CacheCaseContext("محتوى مخزن مسبقاً من الجلسة السابقة", DateTimeOffset.UtcNow);
        dbContext.AgentConversations.Add(conversation);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = userId };
        var fileStorage = new TestFileStorageService();
        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            new TestEmbeddingProvider(),
            new TestVectorStoreProvider(),
            fileStorage,
            new TestDocumentParsingProvider(),
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(), new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        // Act
        var context = await service.GetOrFetchCaseContextAsync(conversation.Id);

        // Assert
        Assert.Equal("محتوى مخزن مسبقاً من الجلسة السابقة", context);
    }

    [Fact]
    public async Task SendMessage_HappyPath_SavesUserAndAssistantMessagesAndReturnsAiResponse()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();
        var currentUserService = new TestCurrentUserService { UserId = userId };
        var chatModelProvider = new TestChatModelProvider { OutputToReturn = "وفقاً للمادة 157 من القانون المدني المصري، يجوز الفسخ عند عدم الوفاء بالالتزام." };
        var vectorStoreProvider = new TestVectorStoreProvider
        {
            SearchResultsToReturn = new List<SmartCourt.Interfaces.Providers.VectorSearchResult>
            {
                new(Guid.NewGuid(), 0.95f, new System.Collections.Generic.Dictionary<string, object>
                {
                    { "chunk_text", "المادة 157: في العقود الإلزامية للجانبين إذا لم يوف أحد المتعاقدين بالتزامه جاز للمتعاقد الآخر أن يطالب بالفسخ." }
                })
            }
        };

        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            chatModelProvider,
            new TestEmbeddingProvider(),
            vectorStoreProvider,
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(), new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        var conversation = await service.CreateConversationAsync(new CreateAgentConversationRequest(CaseId: null));

        // Act
        var result = await service.SendMessageAsync(conversation.Id, new SendAgentMessageRequest("ما هو حكم فسخ العقد لعدم التنفيذ؟"));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Assistant", result.Role);
        Assert.Contains("وفقاً للمادة 157", result.Content);

        var messagesInDb = await dbContext.AgentMessages
            .Where(m => m.ConversationId == conversation.Id)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        Assert.Equal(2, messagesInDb.Count);
        Assert.Equal(AgentMessageRole.User, messagesInDb[0].Role);
        Assert.Equal("ما هو حكم فسخ العقد لعدم التنفيذ؟", messagesInDb[0].Content);
        Assert.Equal(AgentMessageRole.Assistant, messagesInDb[1].Role);
        Assert.Equal(result.Content, messagesInDb[1].Content);
    }

    [Fact]
    public async Task SendMessage_ConversationNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();
        var currentUserService = new TestCurrentUserService { UserId = userId };
        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            new TestEmbeddingProvider(),
            new TestVectorStoreProvider(),
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(), new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => service.SendMessageAsync(Guid.NewGuid(), new SendAgentMessageRequest("مرحباً")));
    }

    [Fact]
    public async Task SendMessage_UnauthorizedUser_ThrowsForbiddenAccessException()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var ownerId = Guid.NewGuid();
        var foreignUserId = Guid.NewGuid();

        var conversation = AgentConversation.Create(Guid.NewGuid(), ownerId, caseId: null, DateTimeOffset.UtcNow);
        dbContext.AgentConversations.Add(conversation);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = foreignUserId };
        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            new TestEmbeddingProvider(),
            new TestVectorStoreProvider(),
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(), new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() => service.SendMessageAsync(conversation.Id, new SendAgentMessageRequest("مرحباً")));
    }

    [Fact]
    public async Task SendMessage_FirstMessage_AutoGeneratesConversationTitle()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();
        var currentUserService = new TestCurrentUserService { UserId = userId };
        var chatModelProvider = new TestChatModelProvider { OutputToReturn = "فسخ عقد البيع لعدم السداد" };

        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            chatModelProvider,
            new TestEmbeddingProvider(),
            new TestVectorStoreProvider(),
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(), new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        var conversation = await service.CreateConversationAsync(new CreateAgentConversationRequest(CaseId: null));
        Assert.Null(conversation.Title);

        // Act
        await service.SendMessageAsync(conversation.Id, new SendAgentMessageRequest("كيف يمكنني فسخ عقد بيع شقة؟"));

        // Assert
        var dbConversation = await dbContext.AgentConversations.FirstOrDefaultAsync(c => c.Id == conversation.Id);
        Assert.NotNull(dbConversation);
        Assert.Equal("فسخ عقد البيع لعدم السداد", dbConversation.Title);
    }

    [Fact]
    public async Task SendMessage_FollowUpMessage_DoesNotOverwriteExistingTitle()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();
        var conversation = AgentConversation.Create(Guid.NewGuid(), userId, caseId: null, DateTimeOffset.UtcNow);
        conversation.UpdateTitle("استفسارات قانون العمل", DateTimeOffset.UtcNow);
        dbContext.AgentConversations.Add(conversation);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = userId };
        var chatModelProvider = new TestChatModelProvider { OutputToReturn = "عنوان جديد غير متوقع" };

        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            chatModelProvider,
            new TestEmbeddingProvider(),
            new TestVectorStoreProvider(),
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(), new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        // Act
        await service.SendMessageAsync(conversation.Id, new SendAgentMessageRequest("سؤال إضافي حول ساعات العمل"));

        // Assert
        var dbConversation = await dbContext.AgentConversations.FirstOrDefaultAsync(c => c.Id == conversation.Id);
        Assert.NotNull(dbConversation);
        Assert.Equal("استفسارات قانون العمل", dbConversation.Title);
    }

    [Fact]
    public async Task ListConversations_ReturnsOnlyCurrentUserNonDeletedConversationsOrderedByUpdatedAt()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();
        var foreignUserId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var conv1 = AgentConversation.Create(Guid.NewGuid(), userId, caseId: null, now.AddMinutes(-10));
        var conv2 = AgentConversation.Create(Guid.NewGuid(), userId, caseId: null, now);
        var deletedConv = AgentConversation.Create(Guid.NewGuid(), userId, caseId: null, now.AddMinutes(-5));
        deletedConv.SoftDelete(now);
        var foreignConv = AgentConversation.Create(Guid.NewGuid(), foreignUserId, caseId: null, now);

        dbContext.AgentConversations.AddRange(conv1, conv2, deletedConv, foreignConv);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = userId };
        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            new TestEmbeddingProvider(),
            new TestVectorStoreProvider(),
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(), new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        // Act
        var result = await service.ListConversationsAsync(page: 1, pageSize: 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(conv2.Id, result.Items[0].Id);
        Assert.Equal(conv1.Id, result.Items[1].Id);
    }

    [Fact]
    public async Task GetConversation_HappyPath_ReturnsDetailWithCase()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();
        var caseId = Guid.NewGuid();

        var caseEntity = new CaseEntity
        {
            Id = caseId,
            Title = "نزاع عقد مقاولة",
            Description = "تاخير في التنفيذ",
            ClientId = userId
        };
        dbContext.Cases.Add(caseEntity);

        var conversation = AgentConversation.Create(Guid.NewGuid(), userId, caseId, DateTimeOffset.UtcNow);
        conversation.UpdateTitle("استفسار عن عقد المقاولة", DateTimeOffset.UtcNow);
        dbContext.AgentConversations.Add(conversation);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = userId };
        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            new TestEmbeddingProvider(),
            new TestVectorStoreProvider(),
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(), new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        // Act
        var result = await service.GetConversationAsync(conversation.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(conversation.Id, result.Id);
        Assert.Equal("استفسار عن عقد المقاولة", result.Title);
        Assert.Equal(caseId, result.CaseId);
        Assert.Equal("نزاع عقد مقاولة", result.CaseTitle);
        Assert.Equal("تاخير في التنفيذ", result.CaseDescription);
    }

    [Fact]
    public async Task DeleteConversation_HappyPath_SetsIsDeletedFlag()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();
        var conversation = AgentConversation.Create(Guid.NewGuid(), userId, caseId: null, DateTimeOffset.UtcNow);
        dbContext.AgentConversations.Add(conversation);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = userId };
        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            new TestEmbeddingProvider(),
            new TestVectorStoreProvider(),
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(), new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        // Act
        await service.DeleteConversationAsync(conversation.Id);

        // Assert
        var dbConv = await dbContext.AgentConversations.FirstOrDefaultAsync(c => c.Id == conversation.Id);
        Assert.NotNull(dbConv);
        Assert.True(dbConv.IsDeleted);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetConversationAsync(conversation.Id));
    }

    [Fact]
    public async Task GetMessages_WithoutCursor_ReturnsFirstPageAndCalculatesHasMore()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();
        var conversation = AgentConversation.Create(Guid.NewGuid(), userId, caseId: null, DateTimeOffset.UtcNow);
        dbContext.AgentConversations.Add(conversation);

        var baseTime = DateTimeOffset.UtcNow.AddHours(-1);
        for (int i = 1; i <= 5; i++)
        {
            var msg = AgentMessage.CreateUserMessage(Guid.NewGuid(), conversation.Id, $"رسالة {i}", baseTime.AddMinutes(i));
            dbContext.AgentMessages.Add(msg);
        }
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = userId };
        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            new TestEmbeddingProvider(),
            new TestVectorStoreProvider(),
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(), new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        // Act
        var result = await service.GetMessagesAsync(conversation.Id, beforeMessageId: null, limit: 3);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.HasMore);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal("رسالة 3", result.Items[0].Content);
        Assert.Equal("رسالة 4", result.Items[1].Content);
        Assert.Equal("رسالة 5", result.Items[2].Content);
    }

    [Fact]
    public async Task GetMessages_WithCursor_ReturnsMessagesBeforeCursor()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();
        var conversation = AgentConversation.Create(Guid.NewGuid(), userId, caseId: null, DateTimeOffset.UtcNow);
        dbContext.AgentConversations.Add(conversation);

        var baseTime = DateTimeOffset.UtcNow.AddHours(-1);
        var msg1 = AgentMessage.CreateUserMessage(Guid.NewGuid(), conversation.Id, "رسالة 1", baseTime.AddMinutes(1));
        var msg2 = AgentMessage.CreateUserMessage(Guid.NewGuid(), conversation.Id, "رسالة 2", baseTime.AddMinutes(2));
        var msg3 = AgentMessage.CreateUserMessage(Guid.NewGuid(), conversation.Id, "رسالة 3", baseTime.AddMinutes(3));
        dbContext.AgentMessages.AddRange(msg1, msg2, msg3);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = userId };
        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            new TestEmbeddingProvider(),
            new TestVectorStoreProvider(),
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(), new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        // Act
        var result = await service.GetMessagesAsync(conversation.Id, beforeMessageId: msg3.Id, limit: 10);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.HasMore);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("رسالة 1", result.Items[0].Content);
        Assert.Equal("رسالة 2", result.Items[1].Content);
    }

    [Fact]
    public void SanitizeMarkdown_ConvertsHtmlBreaksStripsTagsAndFixesHeaders()
    {
        // Arrange
        var rawInput = "##التكييف القانوني<br/>فقرة تفصيلية تحتوي على <span>وسم إتش تي إم إم</span><br><br>###الدفوع الموضوعية";

        // Act
        var sanitized = ChatAgentService.SanitizeMarkdown(rawInput);

        // Assert
        Assert.DoesNotContain("<br", sanitized, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<span>", sanitized);
        Assert.Contains("## التكييف القانوني", sanitized);
        Assert.Contains("### الدفوع الموضوعية", sanitized);
        Assert.Contains("وسم إتش تي إم إم", sanitized);
    }

    [Fact]
    public async Task SendMessage_SanitizesAiResponseMarkdown_SavesAndReturnsCleanMarkdown()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();
        var conversation = AgentConversation.Create(Guid.NewGuid(), userId, caseId: null, DateTimeOffset.UtcNow);
        dbContext.AgentConversations.Add(conversation);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = userId };
        var chatModelProvider = new TestChatModelProvider
        {
            OutputToReturn = "##الاستشارة القانونية<br>هذا نص الاستشارة الإجرائية مع <b>وسم غامق</b>."
        };

        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            chatModelProvider,
            new TestEmbeddingProvider(),
            new TestVectorStoreProvider(),
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(), new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        var request = new SendAgentMessageRequest("ما هي إجراءات رفع دعوى صحة ونفاذ؟");

        // Act
        var responseDto = await service.SendMessageAsync(conversation.Id, request);

        // Assert
        Assert.NotNull(responseDto);
        Assert.Contains("## الاستشارة القانونية", responseDto.Content);
        Assert.DoesNotContain("<br>", responseDto.Content);
        Assert.DoesNotContain("<b>", responseDto.Content);
        Assert.Contains("هذا نص الاستشارة الإجرائية مع وسم غامق.", responseDto.Content);

        var savedAssistantMessage = await dbContext.AgentMessages.FirstOrDefaultAsync(m => m.Id == responseDto.Id);
        Assert.NotNull(savedAssistantMessage);
        Assert.Equal(responseDto.Content, savedAssistantMessage.Content);
    }

    [Fact]
    public async Task UpdateConversationTitle_HappyPath_UpdatesTitleAndReturnsDto()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();
        var conversation = AgentConversation.Create(Guid.NewGuid(), userId, caseId: null, DateTimeOffset.UtcNow);
        dbContext.AgentConversations.Add(conversation);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = userId };
        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            new TestEmbeddingProvider(),
            new TestVectorStoreProvider(),
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(),
            new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        var request = new UpdateAgentConversationTitleRequest("عنوان مخصص جديد");

        // Act
        var result = await service.UpdateConversationTitleAsync(conversation.Id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("عنوان مخصص جديد", result.Title);

        var updatedInDb = await dbContext.AgentConversations.FirstOrDefaultAsync(c => c.Id == conversation.Id);
        Assert.NotNull(updatedInDb);
        Assert.Equal("عنوان مخصص جديد", updatedInDb.Title);
    }

    [Fact]
    public async Task UpdateConversationTitle_NotFound_ThrowsNotFoundException()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var currentUserService = new TestCurrentUserService { UserId = Guid.NewGuid() };
        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            new TestEmbeddingProvider(),
            new TestVectorStoreProvider(),
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(),
            new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        var request = new UpdateAgentConversationTitleRequest("عنوان جديد");

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateConversationTitleAsync(Guid.NewGuid(), request));
    }

    [Fact]
    public async Task UpdateConversationTitle_ForbiddenAccess_ThrowsForbiddenAccessException()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var ownerId = Guid.NewGuid();
        var conversation = AgentConversation.Create(Guid.NewGuid(), ownerId, caseId: null, DateTimeOffset.UtcNow);
        dbContext.AgentConversations.Add(conversation);
        await dbContext.SaveChangesAsync();

        var attackerId = Guid.NewGuid();
        var currentUserService = new TestCurrentUserService { UserId = attackerId };
        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            new TestEmbeddingProvider(),
            new TestVectorStoreProvider(),
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(),
            new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        var request = new UpdateAgentConversationTitleRequest("محاولة اختراق العنوان");

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            service.UpdateConversationTitleAsync(conversation.Id, request));
    }

    [Fact]
    public async Task UpdateConversationTitle_UnauthenticatedUser_ThrowsAuthenticationException()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var ownerId = Guid.NewGuid();
        var conversation = AgentConversation.Create(Guid.NewGuid(), ownerId, caseId: null, DateTimeOffset.UtcNow);
        dbContext.AgentConversations.Add(conversation);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = null };
        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            new TestEmbeddingProvider(),
            new TestVectorStoreProvider(),
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(),
            new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        var request = new UpdateAgentConversationTitleRequest("تعديل بدون تسجيل");

        // Act & Assert
        await Assert.ThrowsAsync<AuthenticationException>(() =>
            service.UpdateConversationTitleAsync(conversation.Id, request));
    }

    [Fact]
    public async Task ListConversations_MatchingTitleSearch_ReturnsMatchingConversations()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();
        var conv1 = AgentConversation.Create(Guid.NewGuid(), userId, caseId: null, DateTimeOffset.UtcNow);
        conv1.UpdateTitle("استشارة في قانون العمل المصري", DateTimeOffset.UtcNow);

        var conv2 = AgentConversation.Create(Guid.NewGuid(), userId, caseId: null, DateTimeOffset.UtcNow);
        conv2.UpdateTitle("استفسار حول عقود الإيجار", DateTimeOffset.UtcNow);

        var conv3 = AgentConversation.Create(Guid.NewGuid(), userId, caseId: null, DateTimeOffset.UtcNow);
        conv3.UpdateTitle("تفاصيل قانون العمل والتعويضات", DateTimeOffset.UtcNow);

        dbContext.AgentConversations.AddRange(conv1, conv2, conv3);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = userId };
        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            new TestEmbeddingProvider(),
            new TestVectorStoreProvider(),
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(),
            new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        // Act
        var result = await service.ListConversationsAsync(page: 1, pageSize: 20, search: "قانون العمل");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, item => Assert.Contains("قانون العمل", item.Title));
    }

    [Fact]
    public async Task ListConversations_NoMatchSearch_ReturnsEmptyList()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();
        var conv1 = AgentConversation.Create(Guid.NewGuid(), userId, caseId: null, DateTimeOffset.UtcNow);
        conv1.UpdateTitle("استشارة جنائية", DateTimeOffset.UtcNow);
        dbContext.AgentConversations.Add(conv1);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = userId };
        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            new TestEmbeddingProvider(),
            new TestVectorStoreProvider(),
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(),
            new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        // Act
        var result = await service.ListConversationsAsync(page: 1, pageSize: 20, search: "مدني وتجاري");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task ListConversations_WithSearchAndPagination_CalculatesCorrectSkipTakeAndTotalCount()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();
        for (int i = 1; i <= 5; i++)
        {
            var conv = AgentConversation.Create(Guid.NewGuid(), userId, caseId: null, DateTimeOffset.UtcNow.AddMinutes(i));
            conv.UpdateTitle($"محادثة رقم {i}", DateTimeOffset.UtcNow.AddMinutes(i));
            dbContext.AgentConversations.Add(conv);
        }
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = userId };
        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            new TestEmbeddingProvider(),
            new TestVectorStoreProvider(),
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(),
            new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        // Act
        var result = await service.ListConversationsAsync(page: 2, pageSize: 2, search: "محادثة");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task ListConversations_WithSearchExcludesDeletedAndOtherUserConversations()
    {
        // Arrange
        var dbOptions = CreateInMemoryOptions();
        await using var dbContext = new ApplicationDbContext(dbOptions);

        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var myActiveConv = AgentConversation.Create(Guid.NewGuid(), userId, caseId: null, DateTimeOffset.UtcNow);
        myActiveConv.UpdateTitle("محادثة نشطة لي", DateTimeOffset.UtcNow);

        var myDeletedConv = AgentConversation.Create(Guid.NewGuid(), userId, caseId: null, DateTimeOffset.UtcNow);
        myDeletedConv.UpdateTitle("محادثة محذوفة لي", DateTimeOffset.UtcNow);
        myDeletedConv.SoftDelete(DateTimeOffset.UtcNow);

        var otherUserConv = AgentConversation.Create(Guid.NewGuid(), otherUserId, caseId: null, DateTimeOffset.UtcNow);
        otherUserConv.UpdateTitle("محادثة نشطة لمستخدم آخر", DateTimeOffset.UtcNow);

        dbContext.AgentConversations.AddRange(myActiveConv, myDeletedConv, otherUserConv);
        await dbContext.SaveChangesAsync();

        var currentUserService = new TestCurrentUserService { UserId = userId };
        var service = new ChatAgentService(
            dbContext,
            currentUserService,
            new TestChatModelProvider(),
            new TestEmbeddingProvider(),
            new TestVectorStoreProvider(),
            new TestFileStorageService(),
            new TestDocumentParsingProvider(),
            new TestQuotaService(),
            new SmartCourt.Tests.TestDoubles.TestLawyerQuotaService(), new TestCostCalculatorService(),
            new TestRerankerProvider(),
            Microsoft.Extensions.Options.Options.Create(new SmartCourt.Common.Configuration.RagOptions()));

        // Act
        var result = await service.ListConversationsAsync(page: 1, pageSize: 20, search: "محادثة");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal(myActiveConv.Id, result.Items[0].Id);
    }
}


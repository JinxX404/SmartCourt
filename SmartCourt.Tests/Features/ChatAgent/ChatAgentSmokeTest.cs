using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SmartCourt.Features.ChatAgent;
using SmartCourt.Features.ChatAgent.DTOs;
using SmartCourt.Features.ChatAgent.Entities;
using SmartCourt.Interfaces;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;
using SmartCourt.Common.Domain;
using Xunit;
using Xunit.Abstractions;

namespace SmartCourt.Tests.Features.ChatAgent;

public class TestCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; set; }
    public bool IsAuthenticated => UserId.HasValue;
}

public class ChatAgentSmokeTest
{
    private readonly ITestOutputHelper _output;

    public ChatAgentSmokeTest(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    [Trait("Category", "SmokeTest")]
    public async Task CreateConversation_ComputeAllTokens()
    {
        // 1. Setup Configuration
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var today = new DateTimeOffset(new DateTime(2025, 1, 1), TimeSpan.Zero);
        var mockTime = new Mock<TimeProvider>();
        mockTime.Setup(t => t.GetUtcNow()).Returns(today);
        
        // 2. Setup DI
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<TimeProvider>(mockTime.Object);
        services.AddLogging(c => c.ClearProviders()); // We'll log to test output manually if needed

        // Use Sqlite in-memory for testing DB
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connection));

        // Add SmartCourt dependencies
        SmartCourt.DependencyInjection.AddInfrastructureServices(services, configuration, true);

        // Override user service to return a valid Client
        var clientId = Guid.NewGuid();
        services.AddSingleton<ICurrentUserService>(new TestCurrentUserService { UserId = clientId });
        
        // Mock HttpContext to simulate "Client" role
        var claims = new[] { new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Client") };
        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var user = new System.Security.Claims.ClaimsPrincipal(identity);
        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = user };
        var httpContextAccessor = new Microsoft.AspNetCore.Http.HttpContextAccessor { HttpContext = httpContext };
        services.AddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor>(httpContextAccessor);

        var serviceProvider = services.BuildServiceProvider();

        // 3. Initialize Database
        using (var scope = serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();

            // Seed Quota
            db.QuotaProfiles.Add(QuotaProfile.Create(clientId, 5000000));
            await db.SaveChangesAsync();

            await db.Database.ExecuteSqlRawAsync(
                $"INSERT INTO QuotaLedgers (ClientId, AdditionalTokenBalance, RowVersion) " +
                $"VALUES ('{clientId.ToString().ToUpperInvariant()}', 5000000, x'0000000000000001')");

            var usage = DailyUsage.Create(clientId, today);
            db.DailyUsages.Add(usage);
            await db.SaveChangesAsync();
        }

        // 4. Run the Pipeline
        using (var scope = serviceProvider.CreateScope())
        {
            var chatAgentService = scope.ServiceProvider.GetRequiredService<IChatAgentService>();
            var quotaService = scope.ServiceProvider.GetRequiredService<IQuotaService>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var initialQuota = await quotaService.GetQuotaAsync(clientId);
            _output.WriteLine($"Initial Total Remaining Credits: {initialQuota.TotalRemainingCredits}");

            // The isolated question
            var question = "هل الجهل بالقانون يعفي من المسئولية";
            var createRequest = new CreateAgentConversationRequest(CaseId: null);
            
            // Create Conversation
            var conversationResult = await chatAgentService.CreateConversationAsync(createRequest);
            var conversationId = conversationResult.Id;

            _output.WriteLine($"\n--- Sending Request ---");
            _output.WriteLine($"User Query: {question}");

            using (var scope2 = serviceProvider.CreateScope())
            {
                var dbCheck = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var checkLedger = await dbCheck.QuotaLedgers.FirstOrDefaultAsync(x => x.ClientId == clientId);
                _output.WriteLine($"DB Check Ledger Found: {checkLedger != null}, Balance: {checkLedger?.AdditionalTokenBalance}");
            }

            // Call the service
            var sendRequest = new SendAgentMessageRequest(Content: question);
            var result = await chatAgentService.SendMessageAsync(conversationId, sendRequest);

            _output.WriteLine($"\n--- LLM Response ---");
            _output.WriteLine(result.Content);

            // Wait a moment for the background Title generation to finish
            await Task.Delay(3000);

            // 5. Compute Final Tokens
            var finalQuota = await quotaService.GetQuotaAsync(clientId);
            int tokensDeducted = CreditConverter.ToTokens(initialQuota.TotalRemainingCredits) - CreditConverter.ToTokens(finalQuota.TotalRemainingCredits);
            
            _output.WriteLine($"\n--- Token Breakdown ---");
            _output.WriteLine($" Actual Raw Tokens Used (Pipeline): {tokensDeducted}");
            _output.WriteLine($" Final Total Remaining Credits: {finalQuota.TotalRemainingCredits}");

            var usages = await db.ModelUsageHistories
                .Where(x => x.ClientId == clientId)
                .ToListAsync();

            _output.WriteLine($"\n--- Monetary Cost & Token Usage Breakdown ---");
            decimal totalCost = 0;
            foreach (var usage in usages)
            {
                _output.WriteLine($"Model: {usage.ModelName}");
                _output.WriteLine($"  Tokens: Input={usage.InputTokens}, Output={usage.OutputTokens}, Total={usage.TotalTokens}");
                _output.WriteLine($"  Cost  : Input=${usage.InputCost:F6}, Output=${usage.OutputCost:F6}, Total=${usage.TotalCost:F6}");
                totalCost += usage.TotalCost;
            }
            _output.WriteLine($"\n Total Request Cost: ${totalCost:F6}");

            Assert.True(tokensDeducted > 0, "No tokens were deducted!");
            Assert.NotEmpty(usages);
            Assert.True(totalCost > 0, "Total cost should be greater than zero");
        }
    }
}

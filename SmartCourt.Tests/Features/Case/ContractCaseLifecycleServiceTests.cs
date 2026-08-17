using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Enums;
using SmartCourt.Entities;
using SmartCourt.Features.Case.Integration;
using SmartCourt.Features.Contracts.Integration;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Case;

public class ContractCaseLifecycleServiceTests
{
    private static ApplicationDbContext CreateInMemoryContext(TimeProvider? timeProvider = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"CaseLifecycle_{Guid.NewGuid()}")
            .Options;
        return new ApplicationDbContext(options, timeProvider);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }

    [Fact]
    public async Task ApplyAsync_WhenContractCompleted_TransitionsCaseStatusToClosed()
    {
        var now = DateTimeOffset.UtcNow;
        await using var context = CreateInMemoryContext(new FixedTimeProvider(now));
        var clientId = Guid.NewGuid();
        var clientUser = new ApplicationUser
        {
            Id = clientId,
            UserName = "client@example.com",
            Email = "client@example.com",
            FullName = "Test Client"
        };
        var clientProfile = new ClientProfile { UserId = clientId, User = clientUser };
        context.Users.Add(clientUser);
        context.ClientProfile.Add(clientProfile);

        var legalCase = new SmartCourt.Entities.Case
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            ClientProfile = clientProfile,
            Title = "Test Case",
            Description = "Case description",
            Status = CaseStatus.Assigned
        };
        context.Cases.Add(legalCase);
        await context.SaveChangesAsync();

        var service = new ContractCaseLifecycleService(context);
        var update = new ContractCaseLifecycleUpdate(
            Guid.NewGuid(),
            legalCase.Id,
            Guid.NewGuid(),
            ContractCaseLifecycleTransition.ContractCompleted,
            now);

        await service.ApplyAsync(update, CancellationToken.None);

        var updatedCase = await context.Cases.FindAsync(legalCase.Id);
        Assert.NotNull(updatedCase);
        Assert.Equal(CaseStatus.Closed, updatedCase.Status);
        Assert.Equal(now.UtcDateTime, updatedCase.UpdatedAt);
    }

    [Fact]
    public async Task ApplyAsync_WhenCaseNotFound_DoesNotThrow()
    {
        await using var context = CreateInMemoryContext();
        var service = new ContractCaseLifecycleService(context);
        var update = new ContractCaseLifecycleUpdate(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            ContractCaseLifecycleTransition.ContractCompleted,
            DateTimeOffset.UtcNow);

        var exception = await Record.ExceptionAsync(() => service.ApplyAsync(update, CancellationToken.None));
        Assert.Null(exception);
    }
}

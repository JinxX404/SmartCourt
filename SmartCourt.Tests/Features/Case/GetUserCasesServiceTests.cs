using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Exceptions;
using SmartCourt.Entities;
using SmartCourt.Features.Case.GetUserCases;
using SmartCourt.Features.Case.GetUserCases.DTOs;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using Xunit;
using CaseEntity = SmartCourt.Entities.Case;

namespace SmartCourt.Tests.Features.Case;

public sealed class FakeCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; set; }
    public bool IsAuthenticated => UserId.HasValue && UserId.Value != Guid.Empty;
}

public sealed class GetUserCasesServiceTests
{
    private static DbContextOptions<ApplicationDbContext> CreateDbContextOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    private static ApplicationUser CreateUser(ApplicationDbContext db, Guid id = default)
    {
        var uid = id == default ? Guid.NewGuid() : id;
        var user = new ApplicationUser
        {
            Id = uid,
            UserName = $"user_{uid:N}@example.com",
            Email = $"user_{uid:N}@example.com",
            FullName = $"User {uid:N}",
            NationalNumber = $"{Random.Shared.NextInt64(10000000000000L, 99999999999999L)}"
        };
        var profile = new ClientProfile { UserId = uid, User = user };
        db.Users.Add(user);
        db.ClientProfile.Add(profile);
        return user;
    }

    [Fact]
    public async Task GetUserCasesAsync_ReturnsOnlyCasesOwnedByCurrentLoggedInUser()
    {
        // Arrange
        var options = CreateDbContextOptions();
        using var db = new ApplicationDbContext(options);

        var userA = CreateUser(db);
        var userB = CreateUser(db);

        var case1 = new CaseEntity
        {
            Id = Guid.NewGuid(),
            Title = "UserA Case 1",
            Description = "Desc 1",
            ClientId = userA.Id,
            Status = CaseStatus.Submitted,
            CreatedAt = DateTime.UtcNow
        };
        var case2 = new CaseEntity
        {
            Id = Guid.NewGuid(),
            Title = "UserA Case 2",
            Description = "Desc 2",
            ClientId = userA.Id,
            Status = CaseStatus.Reviewed,
            CreatedAt = DateTime.UtcNow
        };
        var case3 = new CaseEntity
        {
            Id = Guid.NewGuid(),
            Title = "UserB Case 1",
            Description = "Desc 3",
            ClientId = userB.Id,
            Status = CaseStatus.Submitted,
            CreatedAt = DateTime.UtcNow
        };

        db.Cases.AddRange(case1, case2, case3);
        await db.SaveChangesAsync();

        var fakeCurrentUserService = new FakeCurrentUserService { UserId = userA.Id };
        var service = new GetUserCasesService(db, fakeCurrentUserService);

        // Act
        var result = await service.GetUserCasesAsync(new GetUserCasesQuery());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, item => Assert.StartsWith("UserA", item.Title));
    }

    [Fact]
    public async Task GetUserCasesAsync_FiltersByStatusCorrectly()
    {
        // Arrange
        var options = CreateDbContextOptions();
        using var db = new ApplicationDbContext(options);

        var user = CreateUser(db);

        db.Cases.AddRange(
            new CaseEntity { Id = Guid.NewGuid(), Title = "Submitted Case", Description = "Desc", ClientId = user.Id, Status = CaseStatus.Submitted, CreatedAt = DateTime.UtcNow },
            new CaseEntity { Id = Guid.NewGuid(), Title = "Reviewed Case", Description = "Desc", ClientId = user.Id, Status = CaseStatus.Reviewed, CreatedAt = DateTime.UtcNow },
            new CaseEntity { Id = Guid.NewGuid(), Title = "Closed Case", Description = "Desc", ClientId = user.Id, Status = CaseStatus.Closed, CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var fakeCurrentUserService = new FakeCurrentUserService { UserId = user.Id };
        var service = new GetUserCasesService(db, fakeCurrentUserService);

        // Act
        var result = await service.GetUserCasesAsync(new GetUserCasesQuery(Status: CaseStatus.Reviewed));

        // Assert
        Assert.Single(result.Items);
        Assert.Equal("Reviewed Case", result.Items[0].Title);
        Assert.Equal(CaseStatus.Reviewed.ToString(), result.Items[0].Status);
    }

    [Fact]
    public async Task GetUserCasesAsync_FiltersBySearchTerm()
    {
        // Arrange
        var options = CreateDbContextOptions();
        using var db = new ApplicationDbContext(options);

        var user = CreateUser(db);

        db.Cases.AddRange(
            new CaseEntity { Id = Guid.NewGuid(), Title = "Commercial Contract Dispute", Description = "Desc", ClientId = user.Id, Status = CaseStatus.Submitted, CreatedAt = DateTime.UtcNow },
            new CaseEntity { Id = Guid.NewGuid(), Title = "Property Registration", Description = "Desc", ClientId = user.Id, Status = CaseStatus.Submitted, CreatedAt = DateTime.UtcNow }
        );
        await db.SaveChangesAsync();

        var fakeCurrentUserService = new FakeCurrentUserService { UserId = user.Id };
        var service = new GetUserCasesService(db, fakeCurrentUserService);

        // Act
        var result = await service.GetUserCasesAsync(new GetUserCasesQuery(SearchTerm: "Contract"));

        // Assert
        Assert.Single(result.Items);
        Assert.Equal("Commercial Contract Dispute", result.Items[0].Title);
    }

    [Fact]
    public async Task GetUserCasesAsync_PaginatesResultsCorrectly()
    {
        // Arrange
        var options = CreateDbContextOptions();
        using var db = new ApplicationDbContext(options);

        var user = CreateUser(db);

        for (int i = 1; i <= 5; i++)
        {
            db.Cases.Add(new CaseEntity
            {
                Id = Guid.NewGuid(),
                Title = $"Case {i}",
                Description = "Desc",
                ClientId = user.Id,
                Status = CaseStatus.Submitted,
                CreatedAt = DateTime.UtcNow.AddMinutes(i)
            });
        }
        await db.SaveChangesAsync();

        var fakeCurrentUserService = new FakeCurrentUserService { UserId = user.Id };
        var service = new GetUserCasesService(db, fakeCurrentUserService);

        // Act
        var page1Result = await service.GetUserCasesAsync(new GetUserCasesQuery(Page: 1, PageSize: 2));
        var page2Result = await service.GetUserCasesAsync(new GetUserCasesQuery(Page: 2, PageSize: 2));

        // Assert
        Assert.Equal(5, page1Result.TotalCount);
        Assert.Equal(2, page1Result.Items.Count);
        Assert.True(page1Result.HasNextPage);

        Assert.Equal(5, page2Result.TotalCount);
        Assert.Equal(2, page2Result.Items.Count);
        Assert.True(page2Result.HasNextPage);
    }

    [Fact]
    public async Task GetUserCasesAsync_ThrowsAuthenticationException_WhenUserNotAuthenticated()
    {
        // Arrange
        var options = CreateDbContextOptions();
        using var db = new ApplicationDbContext(options);

        var fakeCurrentUserService = new FakeCurrentUserService { UserId = null };
        var service = new GetUserCasesService(db, fakeCurrentUserService);

        // Act & Assert
        await Assert.ThrowsAsync<AuthenticationException>(() =>
            service.GetUserCasesAsync(new GetUserCasesQuery()));
    }
}

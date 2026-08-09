using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Enums;
using SmartCourt.Entities;
using SmartCourt.Features.Case.GetCaseById;
using SmartCourt.Features.Case.GetCaseById.DTOs;
using SmartCourt.Features.Case.GetCases;
using SmartCourt.Features.Case.GetCases.DTOs;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Persistence;
using SmartCourt.Tests.TestDoubles;
using Xunit;

namespace SmartCourt.Tests.Features.Case;

public class GetCasesHandlerTests
{
    private static DbContextOptions<ApplicationDbContext> CreateSQLiteOptions()
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite($"Data Source={Guid.NewGuid()}.db")
            .Options;
    }

    private static ApplicationUser SeedClient(ApplicationDbContext db, Guid clientId)
    {
        var user = new ApplicationUser
        {
            Id = clientId,
            UserName = $"client_{clientId:N}@example.com",
            Email = $"client_{clientId:N}@example.com",
            FullName = $"Client {clientId:N}"
        };
        var clientProfile = new ClientProfile { UserId = clientId, User = user };
        db.Users.Add(user);
        db.ClientProfile.Add(clientProfile);
        return user;
    }

    private static ApplicationUser SeedLawyer(ApplicationDbContext db, Guid lawyerId)
    {
        var user = new ApplicationUser
        {
            Id = lawyerId,
            UserName = $"lawyer_{lawyerId:N}@example.com",
            Email = $"lawyer_{lawyerId:N}@example.com",
            FullName = $"Lawyer {lawyerId:N}"
        };
        var lawyerProfile = new LawyerProfile { UserId = lawyerId, User = user, Level = LawyerLevel.PrimaryCourt, IsAvailable = true };
        db.Users.Add(user);
        db.LawyerProfiles.Add(lawyerProfile);
        return user;
    }

    [Fact]
    public async Task Handle_GetCases_ReturnsCasesWithAssignedLawyerIdDirectlyOnCase()
    {
        // Arrange
        var options = CreateSQLiteOptions();
        var clientId = Guid.NewGuid();
        var lawyerId = Guid.NewGuid();

        using (var db = new ApplicationDbContext(options))
        {
            db.Database.EnsureCreated();

            SeedClient(db, clientId);
            SeedLawyer(db, lawyerId);

            var clientRole = new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = "Client", NormalizedName = "CLIENT" };
            db.Roles.Add(clientRole);

            var userRole = new IdentityUserRole<Guid> { UserId = clientId, RoleId = clientRole.Id };
            db.UserRoles.Add(userRole);

            var caseEntity = new SmartCourt.Entities.Case
            {
                Id = Guid.NewGuid(),
                Title = "Test Case Title",
                Description = "Test Case Description",
                ClientId = clientId,
                LawyerId = lawyerId,
                Status = CaseStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };
            db.Cases.Add(caseEntity);
            await db.SaveChangesAsync();
        }

        using (var db = new ApplicationDbContext(options))
        {
            var currentUserService = new TestCurrentUserService { UserId = clientId };
            var handler = new GetCasesHandler(db, currentUserService);
            var query = new GetCasesQuery();

            // Act
            var response = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            var item = Assert.Single(response.Data);
            Assert.Equal("Test Case Title", item.Title);
            Assert.Equal(lawyerId, item.LawyerId);
        }
    }

    [Fact]
    public async Task Handle_GetCases_ReturnsLawyerIdFromAcceptedProposalWhenCaseLawyerIdIsNull()
    {
        // Arrange
        var options = CreateSQLiteOptions();
        var clientId = Guid.NewGuid();
        var lawyerId = Guid.NewGuid();
        var caseId = Guid.NewGuid();
        var proposalId = Guid.NewGuid();

        using (var db = new ApplicationDbContext(options))
        {
            db.Database.EnsureCreated();

            SeedClient(db, clientId);
            SeedLawyer(db, lawyerId);

            var clientRole = new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = "Client", NormalizedName = "CLIENT" };
            db.Roles.Add(clientRole);

            var userRole = new IdentityUserRole<Guid> { UserId = clientId, RoleId = clientRole.Id };
            db.UserRoles.Add(userRole);

            var caseEntity = new SmartCourt.Entities.Case
            {
                Id = caseId,
                Title = "Proposal Case Title",
                Description = "Proposal Case Description",
                ClientId = clientId,
                LawyerId = null,
                Status = CaseStatus.Matched,
                CreatedAt = DateTime.UtcNow
            };
            db.Cases.Add(caseEntity);

            var proposal = new Proposal(
                proposalId,
                caseId,
                clientId,
                lawyerId,
                "Proposal Cover Letter",
                DateTime.UtcNow
            );
            proposal.Accept(DateTime.UtcNow);
            db.Proposals.Add(proposal);

            await db.SaveChangesAsync();
        }

        using (var db = new ApplicationDbContext(options))
        {
            var currentUserService = new TestCurrentUserService { UserId = clientId };
            var handler = new GetCasesHandler(db, currentUserService);
            var query = new GetCasesQuery();

            // Act
            var response = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            var item = Assert.Single(response.Data);
            Assert.Equal(lawyerId, item.LawyerId);
        }
    }

    [Fact]
    public async Task Handle_GetCaseById_ReturnsLawyerIdCorrectly()
    {
        // Arrange
        var options = CreateSQLiteOptions();
        var clientId = Guid.NewGuid();
        var lawyerId = Guid.NewGuid();
        var caseId = Guid.NewGuid();

        using (var db = new ApplicationDbContext(options))
        {
            db.Database.EnsureCreated();

            SeedClient(db, clientId);
            SeedLawyer(db, lawyerId);

            var caseEntity = new SmartCourt.Entities.Case
            {
                Id = caseId,
                Title = "Single Case Title",
                Description = "Single Case Description",
                ClientId = clientId,
                LawyerId = lawyerId,
                Status = CaseStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };
            db.Cases.Add(caseEntity);
            await db.SaveChangesAsync();
        }

        using (var db = new ApplicationDbContext(options))
        {
            var handler = new GetCaseByIdHandler(db);
            var query = new GetCaseByIdQuery { CaseId = caseId };

            // Act
            var response = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            Assert.Equal(caseId, response.Data.Id);
            Assert.Equal(lawyerId, response.Data.LawyerId);
        }
    }

    [Fact]
    public async Task Handle_GetCases_CaseWithoutLawyer_ReturnsNullLawyerId()
    {
        // Arrange
        var options = CreateSQLiteOptions();
        var clientId = Guid.NewGuid();

        using (var db = new ApplicationDbContext(options))
        {
            db.Database.EnsureCreated();

            SeedClient(db, clientId);

            var clientRole = new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = "Client", NormalizedName = "CLIENT" };
            db.Roles.Add(clientRole);

            var userRole = new IdentityUserRole<Guid> { UserId = clientId, RoleId = clientRole.Id };
            db.UserRoles.Add(userRole);

            var caseEntity = new SmartCourt.Entities.Case
            {
                Id = Guid.NewGuid(),
                Title = "Unassigned Case Title",
                Description = "Unassigned Case Description",
                ClientId = clientId,
                LawyerId = null,
                Status = CaseStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };
            db.Cases.Add(caseEntity);
            await db.SaveChangesAsync();
        }

        using (var db = new ApplicationDbContext(options))
        {
            var currentUserService = new TestCurrentUserService { UserId = clientId };
            var handler = new GetCasesHandler(db, currentUserService);
            var query = new GetCasesQuery();

            // Act
            var response = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(response.Success);
            Assert.NotNull(response.Data);
            var item = Assert.Single(response.Data);
            Assert.Null(item.LawyerId);
        }
    }
}

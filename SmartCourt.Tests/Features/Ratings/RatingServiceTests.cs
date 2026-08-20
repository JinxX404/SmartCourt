using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Ratings;
using SmartCourt.Features.Ratings.DTOs;
using SmartCourt.Features.Ratings.Entities;
using SmartCourt.Features.Ratings.Enums;
using SmartCourt.Features.Ratings.Validators;
using SmartCourt.Features.Users.Integration;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Ratings;

public sealed class RatingServiceTests
{
    private readonly DateTime _utcNow = new(2026, 8, 16, 12, 0, 0, DateTimeKind.Utc);
    private readonly Guid _clientUserId = Guid.NewGuid();
    private readonly Guid _lawyerUserId = Guid.NewGuid();
    private readonly Guid _moderatorUserId = Guid.NewGuid();
    private readonly Guid _otherUserId = Guid.NewGuid();

    private readonly IValidator<SubmitRatingRequest> _submitValidator = new SubmitRatingRequestValidator();
    private readonly IValidator<LawyerRatingsQuery> _queryValidator = new LawyerRatingsQueryValidator();
    private readonly IValidator<UpdateRatingRequest> _updateValidator = new UpdateRatingRequestValidator();

    private sealed class MutableCurrentUserService(Guid userId) : ICurrentUserService
    {
        public Guid? UserId { get; set; } = userId;
        public bool IsAuthenticated => UserId.HasValue;
    }

    private sealed class StubUserEligibilityService : IContractUserEligibilityService
    {
        public Task<ContractUserEligibilityFacts?> FindEligibilityAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            var isModerator = userId != Guid.Empty && userId.ToString().Contains("mod", StringComparison.OrdinalIgnoreCase);
            return Task.FromResult<ContractUserEligibilityFacts?>(
                new ContractUserEligibilityFacts(
                    UserId: userId,
                    IsActive: true,
                    CanActAsClient: true,
                    CanActAsLawyer: true,
                    CanActAsModerator: isModerator,
                    CanActAsFinanceAdministrator: false,
                    CanActAsSuperAdministrator: isModerator));
        }
    }

    private sealed class ExplicitEligibilityService(bool isModerator) : IContractUserEligibilityService
    {
        public Task<ContractUserEligibilityFacts?> FindEligibilityAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<ContractUserEligibilityFacts?>(
                new ContractUserEligibilityFacts(
                    UserId: userId,
                    IsActive: true,
                    CanActAsClient: true,
                    CanActAsLawyer: true,
                    CanActAsModerator: isModerator,
                    CanActAsFinanceAdministrator: false,
                    CanActAsSuperAdministrator: isModerator));
        }
    }

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(utcNow, TimeSpan.Zero);
    }

    private ApplicationDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new ApplicationDbContext(options, new FixedTimeProvider(_utcNow));
    }

    private async Task SeedUsersAsync(ApplicationDbContext context)
    {
        var client = new ApplicationUser
        {
            Id = _clientUserId,
            UserName = "client@test.com",
            Email = "client@test.com",
            FullName = "أحمد العميل"
        };
        var lawyer = new ApplicationUser
        {
            Id = _lawyerUserId,
            UserName = "lawyer@test.com",
            Email = "lawyer@test.com",
            FullName = "محمود المحامي"
        };
        var moderator = new ApplicationUser
        {
            Id = _moderatorUserId,
            UserName = "mod@test.com",
            Email = "mod@test.com",
            FullName = "طارق المشرف"
        };
        var other = new ApplicationUser
        {
            Id = _otherUserId,
            UserName = "other@test.com",
            Email = "other@test.com",
            FullName = "مستخدم آخر"
        };

        context.Users.AddRange(client, lawyer, moderator, other);
        await context.SaveChangesAsync();
    }

    private RatingService CreateService(
        ApplicationDbContext context,
        ICurrentUserService currentUser,
        IContractUserEligibilityService? eligibilityService = null,
        TimeProvider? timeProvider = null)
    {
        return new RatingService(
            context,
            currentUser,
            eligibilityService ?? new StubUserEligibilityService(),
            timeProvider ?? new FixedTimeProvider(_utcNow),
            _submitValidator,
            _queryValidator,
            _updateValidator);
    }

    private Contract CreateCompletedContract(Guid contractId, DateTime completedAt)
    {
        var contract = new Contract(
            id: contractId,
            proposalId: Guid.NewGuid(),
            legalCaseId: Guid.NewGuid(),
            clientUserId: _clientUserId,
            lawyerUserId: _lawyerUserId,
            title: "Test Contract",
            termsAndConditions: "Terms and conditions for test",
            createdAt: completedAt.AddDays(-10));

        contract.Status = ContractStatus.Completed;
        contract.CompletedAt = completedAt;
        return contract;
    }

    private Contract CreateTerminatedContract(Guid contractId, DateTime terminatedAt)
    {
        var contract = new Contract(
            id: contractId,
            proposalId: Guid.NewGuid(),
            legalCaseId: Guid.NewGuid(),
            clientUserId: _clientUserId,
            lawyerUserId: _lawyerUserId,
            title: "Test Contract",
            termsAndConditions: "Terms and conditions for test",
            createdAt: terminatedAt.AddDays(-10));

        contract.Status = ContractStatus.Terminated;
        contract.TerminatedAt = terminatedAt;
        contract.TerminationReason = "Dispute settled";
        return contract;
    }

    private Contract CreateActiveContract(Guid contractId)
    {
        var contract = new Contract(
            id: contractId,
            proposalId: Guid.NewGuid(),
            legalCaseId: Guid.NewGuid(),
            clientUserId: _clientUserId,
            lawyerUserId: _lawyerUserId,
            title: "Test Contract",
            termsAndConditions: "Terms and conditions for test",
            createdAt: _utcNow.AddDays(-5));

        contract.Status = ContractStatus.Active;
        contract.ActivatedAt = _utcNow.AddDays(-5);
        return contract;
    }

    [Fact]
    public async Task SubmitAsync_ClientRatesLawyer_SuccessfullyCreatesRatingAndUpdatesLawyerAverage()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        await SeedUsersAsync(context);

        var lawyerProfile = new LawyerProfile
        {
            UserId = _lawyerUserId,
            AverageRating = 0m,
            TotalRatingSum = 0,
            TotalRatingCount = 0
        };
        context.Set<LawyerProfile>().Add(lawyerProfile);

        var contractId = Guid.NewGuid();
        var contract = CreateCompletedContract(contractId, _utcNow.AddDays(-2));
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();

        var currentUser = new MutableCurrentUserService(_clientUserId);
        var service = CreateService(context, currentUser);

        var request = new SubmitRatingRequest(5, "Great lawyer!");
        var result = await service.SubmitAsync(contractId, request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(contractId, result.ContractId);
        Assert.Equal("أحمد العميل", result.RaterName);
        Assert.Equal("محمود المحامي", result.RatedName);
        Assert.Equal(RaterRole.Client, result.RaterRole);
        Assert.Equal(5, result.Stars);
        Assert.Equal("Great lawyer!", result.Comment);

        var updatedLawyer = await context.Set<LawyerProfile>().FindAsync(_lawyerUserId);
        Assert.NotNull(updatedLawyer);
        Assert.Equal(1, updatedLawyer.TotalRatingCount);
        Assert.Equal(5, updatedLawyer.TotalRatingSum);
        Assert.Equal(5.00m, updatedLawyer.AverageRating);
    }

    [Fact]
    public async Task SubmitAsync_MultipleClientRatings_AccuratelyComputesLawyerAverage()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        await SeedUsersAsync(context);

        var lawyerProfile = new LawyerProfile
        {
            UserId = _lawyerUserId,
            AverageRating = 4.00m,
            TotalRatingSum = 4,
            TotalRatingCount = 1
        };
        context.Set<LawyerProfile>().Add(lawyerProfile);

        var contractId = Guid.NewGuid();
        var contract = CreateCompletedContract(contractId, _utcNow.AddDays(-1));
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();

        var currentUser = new MutableCurrentUserService(_clientUserId);
        var service = CreateService(context, currentUser);

        var request = new SubmitRatingRequest(5, "Second rating");
        await service.SubmitAsync(contractId, request, CancellationToken.None);

        var updatedLawyer = await context.Set<LawyerProfile>().FindAsync(_lawyerUserId);
        Assert.NotNull(updatedLawyer);
        Assert.Equal(2, updatedLawyer.TotalRatingCount);
        Assert.Equal(9, updatedLawyer.TotalRatingSum);
        Assert.Equal(4.50m, updatedLawyer.AverageRating);
    }

    [Fact]
    public async Task SubmitAsync_LawyerRatesClient_DoesNotAffectLawyerProfileAggregates()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        await SeedUsersAsync(context);

        var lawyerProfile = new LawyerProfile
        {
            UserId = _lawyerUserId,
            AverageRating = 5.00m,
            TotalRatingSum = 5,
            TotalRatingCount = 1
        };
        context.Set<LawyerProfile>().Add(lawyerProfile);

        var contractId = Guid.NewGuid();
        var contract = CreateTerminatedContract(contractId, _utcNow.AddDays(-3));
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();

        var currentUser = new MutableCurrentUserService(_lawyerUserId);
        var service = CreateService(context, currentUser);

        var request = new SubmitRatingRequest(4, "Good client");
        var result = await service.SubmitAsync(contractId, request, CancellationToken.None);

        Assert.Equal(RaterRole.Lawyer, result.RaterRole);
        Assert.Equal("محمود المحامي", result.RaterName);
        Assert.Equal("أحمد العميل", result.RatedName);

        var updatedLawyer = await context.Set<LawyerProfile>().FindAsync(_lawyerUserId);
        Assert.NotNull(updatedLawyer);
        Assert.Equal(1, updatedLawyer.TotalRatingCount);
        Assert.Equal(5, updatedLawyer.TotalRatingSum);
        Assert.Equal(5.00m, updatedLawyer.AverageRating);
    }

    [Fact]
    public async Task SubmitAsync_ActiveContract_ThrowsBusinessException()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        await SeedUsersAsync(context);

        var contractId = Guid.NewGuid();
        var contract = CreateActiveContract(contractId);
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();

        var currentUser = new MutableCurrentUserService(_clientUserId);
        var service = CreateService(context, currentUser);

        var request = new SubmitRatingRequest(5);
        var ex = await Assert.ThrowsAsync<BusinessException>(
            () => service.SubmitAsync(contractId, request, CancellationToken.None));

        Assert.Contains("لم ينتهِ بعد", ex.Message);
    }

    [Fact]
    public async Task SubmitAsync_After14DayWindow_ThrowsBusinessException()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        await SeedUsersAsync(context);

        var contractId = Guid.NewGuid();
        var contract = CreateCompletedContract(contractId, _utcNow.AddDays(-15)); // 15 days ago
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();

        var currentUser = new MutableCurrentUserService(_clientUserId);
        var service = CreateService(context, currentUser);

        var request = new SubmitRatingRequest(5);
        var ex = await Assert.ThrowsAsync<BusinessException>(
            () => service.SubmitAsync(contractId, request, CancellationToken.None));

        Assert.Contains("انتهت مهلة التقييم", ex.Message);
    }

    [Fact]
    public async Task SubmitAsync_NonPartyUser_ThrowsBusinessException()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        await SeedUsersAsync(context);

        var contractId = Guid.NewGuid();
        var contract = CreateCompletedContract(contractId, _utcNow.AddDays(-2));
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();

        var currentUser = new MutableCurrentUserService(_otherUserId);
        var service = CreateService(context, currentUser);

        var request = new SubmitRatingRequest(5);
        var ex = await Assert.ThrowsAsync<BusinessException>(
            () => service.SubmitAsync(contractId, request, CancellationToken.None));

        Assert.Contains("لست طرفًا", ex.Message);
    }

    [Fact]
    public async Task SubmitAsync_DuplicateRating_ThrowsBusinessException()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        await SeedUsersAsync(context);

        var contractId = Guid.NewGuid();
        var contract = CreateCompletedContract(contractId, _utcNow.AddDays(-2));
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();

        var currentUser = new MutableCurrentUserService(_clientUserId);
        var service = CreateService(context, currentUser);

        var request = new SubmitRatingRequest(5);
        await service.SubmitAsync(contractId, request, CancellationToken.None);

        var ex = await Assert.ThrowsAsync<BusinessException>(
            () => service.SubmitAsync(contractId, request, CancellationToken.None));

        Assert.Contains("مسبقًا", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public async Task SubmitAsync_InvalidStars_ThrowsBusinessException(int stars)
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        await SeedUsersAsync(context);

        var contractId = Guid.NewGuid();
        var currentUser = new MutableCurrentUserService(_clientUserId);
        var service = CreateService(context, currentUser);

        var request = new SubmitRatingRequest(stars);
        await Assert.ThrowsAsync<BusinessException>(
            () => service.SubmitAsync(contractId, request, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateAsync_ClientUpdatesRatingAndComment_SuccessfullyUpdatesAndRecalculatesLawyerAverage()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        await SeedUsersAsync(context);

        var lawyerProfile = new LawyerProfile
        {
            UserId = _lawyerUserId,
            AverageRating = 3.00m,
            TotalRatingSum = 3,
            TotalRatingCount = 1
        };
        context.Set<LawyerProfile>().Add(lawyerProfile);

        var contractId = Guid.NewGuid();
        var contract = CreateCompletedContract(contractId, _utcNow.AddDays(-2));
        context.Contracts.Add(contract);

        var rating = new ContractRating(
            Guid.NewGuid(),
            contractId,
            _clientUserId,
            _lawyerUserId,
            RaterRole.Client,
            3,
            "Initial comment",
            _utcNow.AddDays(-2));
        context.ContractRatings.Add(rating);
        await context.SaveChangesAsync();

        var currentUser = new MutableCurrentUserService(_clientUserId);
        var service = CreateService(context, currentUser);

        var request = new UpdateRatingRequest(5, "Updated comment: Excellent!");
        var result = await service.UpdateAsync(contractId, request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("أحمد العميل", result.RaterName);
        Assert.Equal("محمود المحامي", result.RatedName);
        Assert.Equal(5, result.Stars);
        Assert.Equal("Updated comment: Excellent!", result.Comment);

        var updatedLawyer = await context.Set<LawyerProfile>().FindAsync(_lawyerUserId);
        Assert.NotNull(updatedLawyer);
        Assert.Equal(1, updatedLawyer.TotalRatingCount);
        Assert.Equal(5, updatedLawyer.TotalRatingSum);
        Assert.Equal(5.00m, updatedLawyer.AverageRating);
    }

    [Fact]
    public async Task UpdateAsync_LawyerUpdatesRating_DoesNotAffectLawyerProfile()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        await SeedUsersAsync(context);

        var lawyerProfile = new LawyerProfile
        {
            UserId = _lawyerUserId,
            AverageRating = 5.00m,
            TotalRatingSum = 5,
            TotalRatingCount = 1
        };
        context.Set<LawyerProfile>().Add(lawyerProfile);

        var contractId = Guid.NewGuid();
        var contract = CreateCompletedContract(contractId, _utcNow.AddDays(-2));
        context.Contracts.Add(contract);

        var rating = new ContractRating(
            Guid.NewGuid(),
            contractId,
            _lawyerUserId,
            _clientUserId,
            RaterRole.Lawyer,
            2,
            "Initial lawyer note",
            _utcNow.AddDays(-2));
        context.ContractRatings.Add(rating);
        await context.SaveChangesAsync();

        var currentUser = new MutableCurrentUserService(_lawyerUserId);
        var service = CreateService(context, currentUser);

        var request = new UpdateRatingRequest(4, "Updated lawyer note");
        var result = await service.UpdateAsync(contractId, request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("محمود المحامي", result.RaterName);
        Assert.Equal("أحمد العميل", result.RatedName);
        Assert.Equal(4, result.Stars);
        Assert.Equal("Updated lawyer note", result.Comment);

        var updatedLawyer = await context.Set<LawyerProfile>().FindAsync(_lawyerUserId);
        Assert.NotNull(updatedLawyer);
        Assert.Equal(1, updatedLawyer.TotalRatingCount);
        Assert.Equal(5, updatedLawyer.TotalRatingSum);
        Assert.Equal(5.00m, updatedLawyer.AverageRating);
    }

    [Fact]
    public async Task UpdateAsync_NotRatedYet_ThrowsBusinessException()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        await SeedUsersAsync(context);

        var contractId = Guid.NewGuid();
        var contract = CreateCompletedContract(contractId, _utcNow.AddDays(-2));
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();

        var currentUser = new MutableCurrentUserService(_clientUserId);
        var service = CreateService(context, currentUser);

        var request = new UpdateRatingRequest(5, "Update without rating first");
        var ex = await Assert.ThrowsAsync<BusinessException>(
            () => service.UpdateAsync(contractId, request, CancellationToken.None));

        Assert.Contains("لم تقم بتقديم تقييم", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_ActiveContract_ThrowsBusinessException()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        await SeedUsersAsync(context);

        var contractId = Guid.NewGuid();
        var contract = CreateActiveContract(contractId);
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();

        var currentUser = new MutableCurrentUserService(_clientUserId);
        var service = CreateService(context, currentUser);

        var request = new UpdateRatingRequest(5);
        var ex = await Assert.ThrowsAsync<BusinessException>(
            () => service.UpdateAsync(contractId, request, CancellationToken.None));

        Assert.Contains("لم ينتهِ بعد", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_After14DayWindow_ThrowsBusinessException()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        await SeedUsersAsync(context);

        var contractId = Guid.NewGuid();
        var contract = CreateCompletedContract(contractId, _utcNow.AddDays(-15));
        context.Contracts.Add(contract);

        var rating = new ContractRating(
            Guid.NewGuid(),
            contractId,
            _clientUserId,
            _lawyerUserId,
            RaterRole.Client,
            3,
            "Old comment",
            _utcNow.AddDays(-15));
        context.ContractRatings.Add(rating);
        await context.SaveChangesAsync();

        var currentUser = new MutableCurrentUserService(_clientUserId);
        var service = CreateService(context, currentUser);

        var request = new UpdateRatingRequest(5);
        var ex = await Assert.ThrowsAsync<BusinessException>(
            () => service.UpdateAsync(contractId, request, CancellationToken.None));

        Assert.Contains("انتهت مهلة", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_NonPartyUser_ThrowsBusinessException()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        await SeedUsersAsync(context);

        var contractId = Guid.NewGuid();
        var contract = CreateCompletedContract(contractId, _utcNow.AddDays(-2));
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();

        var currentUser = new MutableCurrentUserService(_otherUserId);
        var service = CreateService(context, currentUser);

        var request = new UpdateRatingRequest(5);
        var ex = await Assert.ThrowsAsync<BusinessException>(
            () => service.UpdateAsync(contractId, request, CancellationToken.None));

        Assert.Contains("لست طرفًا", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public async Task UpdateAsync_InvalidStars_ThrowsBusinessException(int stars)
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        await SeedUsersAsync(context);

        var contractId = Guid.NewGuid();
        var currentUser = new MutableCurrentUserService(_clientUserId);
        var service = CreateService(context, currentUser);

        var request = new UpdateRatingRequest(stars);
        await Assert.ThrowsAsync<BusinessException>(
            () => service.UpdateAsync(contractId, request, CancellationToken.None));
    }

    [Fact]
    public async Task GetByContractAsync_SingleRatingWithinWindow_SealsUnsubmittedSideFromOtherParty()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        await SeedUsersAsync(context);

        var contractId = Guid.NewGuid();
        var contract = CreateCompletedContract(contractId, _utcNow.AddDays(-2)); // within 14 days
        context.Contracts.Add(contract);

        var clientRating = new ContractRating(
            Guid.NewGuid(),
            contractId,
            _clientUserId,
            _lawyerUserId,
            RaterRole.Client,
            5,
            "Client comment",
            _utcNow.AddDays(-2));
        context.ContractRatings.Add(clientRating);
        await context.SaveChangesAsync();

        var eligibilityService = new ExplicitEligibilityService(isModerator: false);

        // 1. Client views -> sees own rating, lawyer rating is null, AreRevealed = false
        var clientUser = new MutableCurrentUserService(_clientUserId);
        var clientService = CreateService(context, clientUser, eligibilityService);
        var clientView = await clientService.GetByContractAsync(contractId, CancellationToken.None);

        Assert.False(clientView.AreRevealed);
        Assert.NotNull(clientView.ClientRating);
        Assert.Equal("أحمد العميل", clientView.ClientRating.RaterName);
        Assert.Equal("محمود المحامي", clientView.ClientRating.RatedName);
        Assert.Equal(5, clientView.ClientRating.Stars);
        Assert.Null(clientView.LawyerRating);

        // 2. Lawyer views -> sees null for client rating (sealed!), lawyer rating is null, AreRevealed = false
        var lawyerUser = new MutableCurrentUserService(_lawyerUserId);
        var lawyerService = CreateService(context, lawyerUser, eligibilityService);
        var lawyerView = await lawyerService.GetByContractAsync(contractId, CancellationToken.None);

        Assert.False(lawyerView.AreRevealed);
        Assert.Null(lawyerView.ClientRating);
        Assert.Null(lawyerView.LawyerRating);
    }

    [Fact]
    public async Task GetByContractAsync_BothSubmitted_RevealsBothRatingsToAllParties()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        await SeedUsersAsync(context);

        var contractId = Guid.NewGuid();
        var contract = CreateCompletedContract(contractId, _utcNow.AddDays(-3));
        context.Contracts.Add(contract);

        var clientRating = new ContractRating(
            Guid.NewGuid(),
            contractId,
            _clientUserId,
            _lawyerUserId,
            RaterRole.Client,
            5,
            "Client comment",
            _utcNow.AddDays(-3));

        var lawyerRating = new ContractRating(
            Guid.NewGuid(),
            contractId,
            _lawyerUserId,
            _clientUserId,
            RaterRole.Lawyer,
            4,
            "Lawyer comment",
            _utcNow.AddDays(-2));

        context.ContractRatings.AddRange(clientRating, lawyerRating);
        await context.SaveChangesAsync();

        var eligibilityService = new ExplicitEligibilityService(isModerator: false);

        var clientUser = new MutableCurrentUserService(_clientUserId);
        var clientService = CreateService(context, clientUser, eligibilityService);
        var summary = await clientService.GetByContractAsync(contractId, CancellationToken.None);

        Assert.True(summary.AreRevealed);
        Assert.NotNull(summary.ClientRating);
        Assert.Equal("أحمد العميل", summary.ClientRating.RaterName);
        Assert.Equal("محمود المحامي", summary.ClientRating.RatedName);
        Assert.Equal(5, summary.ClientRating.Stars);
        Assert.NotNull(summary.LawyerRating);
        Assert.Equal("محمود المحامي", summary.LawyerRating.RaterName);
        Assert.Equal("أحمد العميل", summary.LawyerRating.RatedName);
        Assert.Equal(4, summary.LawyerRating.Stars);
    }

    [Fact]
    public async Task GetByContractAsync_SingleRatingAfterWindowExpired_RevealsRating()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        await SeedUsersAsync(context);

        var contractId = Guid.NewGuid();
        var contract = CreateCompletedContract(contractId, _utcNow.AddDays(-16)); // 16 days ago (> 14 days)
        context.Contracts.Add(contract);

        var clientRating = new ContractRating(
            Guid.NewGuid(),
            contractId,
            _clientUserId,
            _lawyerUserId,
            RaterRole.Client,
            5,
            "Client comment",
            _utcNow.AddDays(-16));
        context.ContractRatings.Add(clientRating);
        await context.SaveChangesAsync();

        var eligibilityService = new ExplicitEligibilityService(isModerator: false);

        // Lawyer views -> because window expired, AreRevealed = true and lawyer sees client's rating!
        var lawyerUser = new MutableCurrentUserService(_lawyerUserId);
        var lawyerService = CreateService(context, lawyerUser, eligibilityService);
        var lawyerView = await lawyerService.GetByContractAsync(contractId, CancellationToken.None);

        Assert.True(lawyerView.AreRevealed);
        Assert.NotNull(lawyerView.ClientRating);
        Assert.Equal("أحمد العميل", lawyerView.ClientRating.RaterName);
        Assert.Equal("محمود المحامي", lawyerView.ClientRating.RatedName);
        Assert.Equal(5, lawyerView.ClientRating.Stars);
        Assert.Null(lawyerView.LawyerRating);
    }

    [Fact]
    public async Task GetByContractAsync_Moderator_AlwaysSeesAllRatingsRegardlessOfRevealState()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        await SeedUsersAsync(context);

        var contractId = Guid.NewGuid();
        var contract = CreateCompletedContract(contractId, _utcNow.AddDays(-2)); // within 14 days
        context.Contracts.Add(contract);

        var clientRating = new ContractRating(
            Guid.NewGuid(),
            contractId,
            _clientUserId,
            _lawyerUserId,
            RaterRole.Client,
            5,
            "Client comment",
            _utcNow.AddDays(-2));
        context.ContractRatings.Add(clientRating);
        await context.SaveChangesAsync();

        var eligibilityService = new ExplicitEligibilityService(isModerator: true);

        var modUser = new MutableCurrentUserService(_moderatorUserId);
        var modService = CreateService(context, modUser, eligibilityService);
        var modView = await modService.GetByContractAsync(contractId, CancellationToken.None);

        Assert.False(modView.AreRevealed);
        Assert.NotNull(modView.ClientRating);
        Assert.Equal("أحمد العميل", modView.ClientRating.RaterName);
        Assert.Equal("محمود المحامي", modView.ClientRating.RatedName);
        Assert.Equal(5, modView.ClientRating.Stars);
        Assert.Null(modView.LawyerRating);
    }

    [Fact]
    public async Task GetByContractAsync_NonPartyNonAdmin_ThrowsBusinessException()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        await SeedUsersAsync(context);

        var contractId = Guid.NewGuid();
        var contract = CreateCompletedContract(contractId, _utcNow.AddDays(-2));
        context.Contracts.Add(contract);
        await context.SaveChangesAsync();

        var eligibilityService = new ExplicitEligibilityService(isModerator: false);

        var otherUser = new MutableCurrentUserService(_otherUserId);
        var service = CreateService(context, otherUser, eligibilityService);

        var ex = await Assert.ThrowsAsync<BusinessException>(
            () => service.GetByContractAsync(contractId, CancellationToken.None));

        Assert.Contains("لست طرفًا", ex.Message);
    }

    [Fact]
    public async Task GetByLawyerAsync_ReturnsOnlyRevealedClientRatingsAndPaginates()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        await SeedUsersAsync(context);

        var lawyerProfile = new LawyerProfile
        {
            UserId = _lawyerUserId,
            AverageRating = 4.5m,
            TotalRatingSum = 9,
            TotalRatingCount = 2
        };
        context.Set<LawyerProfile>().Add(lawyerProfile);

        // Contract 1: Revealed because both parties rated (within window)
        var contract1Id = Guid.NewGuid();
        var contract1 = CreateCompletedContract(contract1Id, _utcNow.AddDays(-3));
        var rating1Client = new ContractRating(Guid.NewGuid(), contract1Id, _clientUserId, _lawyerUserId, RaterRole.Client, 5, "C1", _utcNow.AddDays(-3));
        var rating1Lawyer = new ContractRating(Guid.NewGuid(), contract1Id, _lawyerUserId, _clientUserId, RaterRole.Lawyer, 5, "C1 L", _utcNow.AddDays(-2));

        // Contract 2: Revealed because window expired (> 14 days), only client rated
        var contract2Id = Guid.NewGuid();
        var contract2 = CreateCompletedContract(contract2Id, _utcNow.AddDays(-20));
        var rating2Client = new ContractRating(Guid.NewGuid(), contract2Id, _clientUserId, _lawyerUserId, RaterRole.Client, 4, "C2", _utcNow.AddDays(-20));

        // Contract 3: NOT revealed because only client rated within window (2 days ago)
        var contract3Id = Guid.NewGuid();
        var contract3 = CreateCompletedContract(contract3Id, _utcNow.AddDays(-2));
        var rating3Client = new ContractRating(Guid.NewGuid(), contract3Id, _clientUserId, _lawyerUserId, RaterRole.Client, 1, "C3 secret", _utcNow.AddDays(-2));

        context.Contracts.AddRange(contract1, contract2, contract3);
        context.ContractRatings.AddRange(rating1Client, rating1Lawyer, rating2Client, rating3Client);
        await context.SaveChangesAsync();

        var anyUser = new MutableCurrentUserService(_otherUserId);
        var service = CreateService(context, anyUser);

        var query = new LawyerRatingsQuery(Page: 1, PageSize: 10);
        var result = await service.GetByLawyerAsync(_lawyerUserId, query, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, r => r.Comment == "C1");
        Assert.Contains(result.Items, r => r.Comment == "C2");
        Assert.DoesNotContain(result.Items, r => r.Comment == "C3 secret");
        Assert.All(result.Items, r =>
        {
            Assert.Equal(RaterRole.Client, r.RaterRole);
            Assert.Equal("أحمد العميل", r.RaterName);
            Assert.Equal("محمود المحامي", r.RatedName);
        });
    }

    [Fact]
    public async Task GetByLawyerAsync_NonExistentLawyer_ThrowsBusinessException()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var context = CreateContext(dbName);
        await SeedUsersAsync(context);

        var anyUser = new MutableCurrentUserService(_otherUserId);
        var service = CreateService(context, anyUser);

        var ex = await Assert.ThrowsAsync<BusinessException>(
            () => service.GetByLawyerAsync(Guid.NewGuid(), new LawyerRatingsQuery(), CancellationToken.None));

        Assert.Contains("المحامي غير موجود", ex.Message);
    }
}

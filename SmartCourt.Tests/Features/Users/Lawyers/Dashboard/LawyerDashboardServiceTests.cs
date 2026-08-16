using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Entities;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Exceptions;
using SmartCourt.Entities;
using SmartCourt.Features.Consultations.Domain.Entities;
using SmartCourt.Features.Consultations.Domain.Enums;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Disputes.Entities;
using SmartCourt.Features.Disputes.Enums;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Notifications.Entities;
using SmartCourt.Features.Notifications.Enums;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Features.Ratings.Entities;
using SmartCourt.Features.Ratings.Enums;
using SmartCourt.Features.Users.Lawyers.Dashboard;
using SmartCourt.Features.Users.Lawyers.Dashboard.DTOs;
using SmartCourt.Features.Users.Lawyers.Dashboard.Validators;
using SmartCourt.Persistence;
using Xunit;
using CaseEntity = SmartCourt.Entities.Case;

namespace SmartCourt.Tests.Features.Users.Lawyers.Dashboard;

public sealed class LawyerDashboardServiceTests
{
    private static ApplicationDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new ApplicationDbContext(options);
    }

    private static LawyerDashboardService CreateService(ApplicationDbContext db)
    {
        var activityValidator = new LawyerActivityQueryValidator();
        var earningsValidator = new LawyerEarningsQueryValidator();
        var deadlinesValidator = new LawyerDeadlinesQueryValidator();
        var calendarValidator = new LawyerCalendarQueryValidator();
        return new LawyerDashboardService(db, activityValidator, earningsValidator, deadlinesValidator, calendarValidator);
    }

    [Fact]
    public async Task GetStatsAsync_NewLawyer_ReturnsZeroedMetrics()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);
        var service = CreateService(db);
        var lawyerUserId = Guid.NewGuid();

        var stats = await service.GetStatsAsync(lawyerUserId, CancellationToken.None);

        Assert.NotNull(stats);
        Assert.Equal(0, stats.ActiveContractsCount);
        Assert.Equal(0, stats.PendingProposalsCount);
        Assert.Equal(0, stats.RecommendedCasesCount);
        Assert.Equal(0, stats.UpcomingConsultationsCount);
        Assert.Equal(0, stats.PendingConsultationRequestsCount);
        Assert.Equal(0m, stats.AvailableBalance);
        Assert.Equal(0m, stats.PendingBalance);
        Assert.Equal(0m, stats.LifetimeEarnings);
        Assert.Equal("EGP", stats.Currency);
        Assert.Equal(0m, stats.AverageRating);
        Assert.Equal(0, stats.TotalReviewsCount);
        Assert.Equal(0, stats.UnreadNotificationsCount);
        Assert.Equal(0, stats.ActiveDisputesCount);
    }

    [Fact]
    public async Task GetStatsAsync_ActiveLawyer_CalculatesAllMetricsAccurately()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);
        var service = CreateService(db);
        var lawyerUserId = Guid.NewGuid();
        var clientUserId = Guid.NewGuid();
        var nowUtc = DateTimeOffset.UtcNow;

        // 1. Contracts: 1 Active, 1 Completed
        var activeContract = new Contract(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            clientUserId,
            lawyerUserId,
            "عقد قيد التنفيذ",
            "شروط العقد",
            nowUtc.AddDays(-10))
        {
            Status = ContractStatus.Active
        };

        var completedContract = new Contract(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            clientUserId,
            lawyerUserId,
            "عقد مكتمل",
            "شروط العقد",
            nowUtc.AddDays(-20))
        {
            Status = ContractStatus.Completed
        };
        db.Contracts.AddRange(activeContract, completedContract);

        // 2. Proposals: 1 Pending, 1 Accepted
        var pendingProposal = new Proposal(
            Guid.NewGuid(),
            Guid.NewGuid(),
            clientUserId,
            lawyerUserId,
            "عرض معلق",
            nowUtc.AddDays(-1))
        {
            Status = ProposalStatus.Pending
        };

        var acceptedProposal = new Proposal(
            Guid.NewGuid(),
            Guid.NewGuid(),
            clientUserId,
            lawyerUserId,
            "عرض مقبول",
            nowUtc.AddDays(-3))
        {
            Status = ProposalStatus.Accepted
        };
        db.Proposals.AddRange(pendingProposal, acceptedProposal);

        // 3. Recommended Cases: 1 Matched with recommendation, 1 not recommended
        var matchedCase = new CaseEntity
        {
            Id = Guid.NewGuid(),
            ClientId = clientUserId,
            Title = "قضية مرشحة",
            Description = "تفاصيل القضية",
            Status = CaseStatus.Matched,
            LawyerId = null
        };
        var recommendation = new CaseRecommendation
        {
            Id = Guid.NewGuid(),
            CaseId = matchedCase.Id,
            LawyerId = lawyerUserId,
            TotalScore = 0.95m,
            Explanation = "مطابقة قوية"
        };
        matchedCase.Recommendations.Add(recommendation);
        db.Cases.Add(matchedCase);

        // 4. Consultations: 1 upcoming confirmed, 1 past confirmed, 1 pending payment
        var upcomingBooking = new ConsultationBooking
        {
            Id = Guid.NewGuid(),
            OfferingId = Guid.NewGuid(),
            SlotId = Guid.NewGuid(),
            LawyerId = lawyerUserId,
            ClientId = clientUserId,
            OfferingTitle = "استشارة تجارية",
            Status = ConsultationBookingStatus.Confirmed,
            StartAtUtc = nowUtc.AddDays(2),
            EndAtUtc = nowUtc.AddDays(2).AddHours(1),
            CreatedAt = nowUtc,
            UpdatedAt = nowUtc
        };

        var pastBooking = new ConsultationBooking
        {
            Id = Guid.NewGuid(),
            OfferingId = Guid.NewGuid(),
            SlotId = Guid.NewGuid(),
            LawyerId = lawyerUserId,
            ClientId = clientUserId,
            OfferingTitle = "استشارة سابقة",
            Status = ConsultationBookingStatus.Confirmed,
            StartAtUtc = nowUtc.AddDays(-2),
            EndAtUtc = nowUtc.AddDays(-2).AddHours(1),
            CreatedAt = nowUtc.AddDays(-3),
            UpdatedAt = nowUtc.AddDays(-3)
        };

        var pendingBooking = new ConsultationBooking
        {
            Id = Guid.NewGuid(),
            OfferingId = Guid.NewGuid(),
            SlotId = Guid.NewGuid(),
            LawyerId = lawyerUserId,
            ClientId = clientUserId,
            OfferingTitle = "استشارة بانتظار الدفع",
            Status = ConsultationBookingStatus.AwaitingPayment,
            StartAtUtc = nowUtc.AddDays(3),
            EndAtUtc = nowUtc.AddDays(3).AddHours(1),
            CreatedAt = nowUtc,
            UpdatedAt = nowUtc
        };
        db.ConsultationBookings.AddRange(upcomingBooking, pastBooking, pendingBooking);

        // 5. Wallet & Withdrawals: 5000 available, 2000 pending, 3000 completed withdrawal -> Lifetime = 8000
        var wallet = new LawyerWallet(Guid.NewGuid(), lawyerUserId, nowUtc.AddMonths(-1))
        {
            AvailableBalance = 5000m,
            PendingBalance = 2000m,
            Currency = "EGP"
        };
        var withdrawal = new WithdrawalRequest(Guid.NewGuid(), lawyerUserId, 3000m, "idemp-1", nowUtc.AddDays(-15))
        {
            Status = WithdrawalStatus.Completed
        };
        db.LawyerWallets.Add(wallet);
        db.WithdrawalRequests.Add(withdrawal);

        // 6. Profile reputation: 4.85 stars, 12 reviews
        var profile = new LawyerProfile
        {
            UserId = lawyerUserId,
            AverageRating = 4.85m,
            TotalRatingCount = 12,
            TotalRatingSum = 58
        };
        db.LawyerProfiles.Add(profile);

        // 7. Notifications: 1 unread, 1 read
        var unreadNotification = Notification.Create(
            Guid.NewGuid(),
            lawyerUserId,
            Guid.NewGuid(),
            "ContractAccepted",
            NotificationSeverity.Information,
            "تم قبول العقد",
            "قام العميل بقبول العقد بنجاح",
            null,
            null,
            nowUtc.AddMinutes(-10),
            null);

        var readNotification = Notification.Create(
            Guid.NewGuid(),
            lawyerUserId,
            Guid.NewGuid(),
            "MilestoneApproved",
            NotificationSeverity.Information,
            "تم اعتماد المرحلة",
            "تم اعتماد المرحلة",
            null,
            null,
            nowUtc.AddDays(-2),
            null);
        readNotification.MarkRead(nowUtc.AddDays(-1));

        db.Notifications.AddRange(unreadNotification, readNotification);

        // 8. Disputes: 1 Open on lawyer's contract, 1 Resolved on lawyer's contract
        var openDispute = new Dispute(
            Guid.NewGuid(),
            activeContract.Id,
            Guid.NewGuid(),
            clientUserId,
            DisputeCategory.DeliverableQuality,
            "نزاع جودة",
            "تفاصيل النزاع",
            DisputeRequestedOutcome.Refund,
            nowUtc.AddDays(-1))
        {
            Status = DisputeStatus.Open
        };

        var resolvedDispute = new Dispute(
            Guid.NewGuid(),
            completedContract.Id,
            Guid.NewGuid(),
            clientUserId,
            DisputeCategory.DeliverableQuality,
            "نزاع محلول",
            "تم حله",
            DisputeRequestedOutcome.Release,
            nowUtc.AddDays(-5))
        {
            Status = DisputeStatus.Resolved
        };
        db.Disputes.AddRange(openDispute, resolvedDispute);

        await db.SaveChangesAsync();

        // Act
        var stats = await service.GetStatsAsync(lawyerUserId, CancellationToken.None);

        // Assert
        Assert.NotNull(stats);
        Assert.Equal(1, stats.ActiveContractsCount);
        Assert.Equal(1, stats.PendingProposalsCount);
        Assert.Equal(1, stats.RecommendedCasesCount);
        Assert.Equal(1, stats.UpcomingConsultationsCount);
        Assert.Equal(1, stats.PendingConsultationRequestsCount);
        Assert.Equal(5000m, stats.AvailableBalance);
        Assert.Equal(2000m, stats.PendingBalance);
        Assert.Equal(8000m, stats.LifetimeEarnings);
        Assert.Equal("EGP", stats.Currency);
        Assert.Equal(4.85m, stats.AverageRating);
        Assert.Equal(12, stats.TotalReviewsCount);
        Assert.Equal(1, stats.UnreadNotificationsCount);
        Assert.Equal(1, stats.ActiveDisputesCount);
    }

    [Fact]
    public async Task GetActivityAsync_WithMultiSourceEvents_UnionsAndOrdersByOccurredAtDescending()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);
        var service = CreateService(db);
        var lawyerUserId = Guid.NewGuid();
        var clientUserId = Guid.NewGuid();
        var nowUtc = DateTimeOffset.UtcNow;

        var contract = new Contract(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            clientUserId,
            lawyerUserId,
            "عقد تجاري",
            "شروط العقد",
            nowUtc.AddDays(-10));
        db.Contracts.Add(contract);

        var contractHistory = new ContractStateHistory(
            Guid.NewGuid(),
            contract.Id,
            ContractStatus.Draft,
            ContractStatus.Active,
            "Accepted",
            clientUserId,
            "تم تفعيل العقد",
            Guid.NewGuid(),
            nowUtc.AddDays(-6));
        db.ContractStateHistories.Add(contractHistory);

        var milestone = new Milestone(
            Guid.NewGuid(),
            contract.Id,
            "مرحلة المراجعة",
            "وصف المرحلة",
            1,
            1500m,
            7,
            nowUtc.AddDays(5),
            nowUtc.AddDays(-8));
        db.Milestones.Add(milestone);

        var milestoneHistory = new MilestoneStateHistory(
            Guid.NewGuid(),
            milestone.Id,
            MilestoneStatus.FundedInProgress,
            MilestoneStatus.Submitted,
            "Submitted",
            lawyerUserId,
            "تم تسليم المرحلة الأولى",
            Guid.NewGuid(),
            nowUtc.AddDays(-3));
        db.MilestoneStateHistories.Add(milestoneHistory);

        var proposal = new Proposal(
            Guid.NewGuid(),
            Guid.NewGuid(),
            clientUserId,
            lawyerUserId,
            "عرض أسعار جديد",
            nowUtc.AddDays(-4))
        {
            Status = ProposalStatus.Accepted,
            UpdatedAt = nowUtc.AddDays(-4)
        };
        db.Proposals.Add(proposal);

        var booking = new ConsultationBooking
        {
            Id = Guid.NewGuid(),
            OfferingId = Guid.NewGuid(),
            SlotId = Guid.NewGuid(),
            LawyerId = lawyerUserId,
            ClientId = clientUserId,
            OfferingTitle = "استشارة عقارية",
            Subject = "استفسار عن تسجيل عقار",
            Status = ConsultationBookingStatus.Completed,
            StartAtUtc = nowUtc.AddDays(-2),
            EndAtUtc = nowUtc.AddDays(-2).AddHours(1),
            CreatedAt = nowUtc.AddDays(-5),
            CompletedAtUtc = nowUtc.AddDays(-2),
            UpdatedAt = nowUtc.AddDays(-2)
        };
        db.ConsultationBookings.Add(booking);

        var rating = new ContractRating(
            Guid.NewGuid(),
            contract.Id,
            clientUserId,
            lawyerUserId,
            RaterRole.Client,
            5,
            "عمل ممتاز",
            nowUtc.AddDays(-1).UtcDateTime);
        db.ContractRatings.Add(rating);

        var dispute = new Dispute(
            Guid.NewGuid(),
            contract.Id,
            milestone.Id,
            clientUserId,
            DisputeCategory.Payment,
            "استفسار مالي",
            "تفاصيل النزاع",
            DisputeRequestedOutcome.Review,
            nowUtc.AddHours(-2));
        db.Disputes.Add(dispute);

        await db.SaveChangesAsync();

        var query = new LawyerActivityQuery(1, 10);
        var result = await service.GetActivityAsync(lawyerUserId, query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(6, result.TotalCount);
        Assert.Equal(6, result.Items.Count);
        Assert.False(result.HasNextPage);

        // Verify order: Dispute (hours ago), Rating (-1d), Consultation (-2d), Milestone (-3d), Proposal (-4d), Contract (-6d)
        Assert.Equal(LawyerActivityType.DisputeRaised, result.Items[0].ActivityType);
        Assert.Equal(LawyerActivityType.RatingReceived, result.Items[1].ActivityType);
        Assert.Equal(LawyerActivityType.ConsultationCompleted, result.Items[2].ActivityType);
        Assert.Equal(LawyerActivityType.MilestoneStateChanged, result.Items[3].ActivityType);
        Assert.Equal(LawyerActivityType.ProposalStateChanged, result.Items[4].ActivityType);
        Assert.Equal(LawyerActivityType.ContractStateChanged, result.Items[5].ActivityType);
    }

    [Fact]
    public async Task GetActivityAsync_Pagination_WorksCorrectly()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);
        var service = CreateService(db);
        var lawyerUserId = Guid.NewGuid();
        var nowUtc = DateTimeOffset.UtcNow;

        for (var i = 1; i <= 5; i++)
        {
            var proposal = new Proposal(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                lawyerUserId,
                $"عرض {i}",
                nowUtc.AddDays(-i))
            {
                UpdatedAt = nowUtc.AddDays(-i)
            };
            db.Proposals.Add(proposal);
        }
        await db.SaveChangesAsync();

        // Page 1 of size 2
        var page1 = await service.GetActivityAsync(lawyerUserId, new LawyerActivityQuery(1, 2), CancellationToken.None);
        Assert.Equal(5, page1.TotalCount);
        Assert.Equal(2, page1.Items.Count);
        Assert.True(page1.HasNextPage);

        // Page 2 of size 2
        var page2 = await service.GetActivityAsync(lawyerUserId, new LawyerActivityQuery(2, 2), CancellationToken.None);
        Assert.Equal(2, page2.Items.Count);
        Assert.True(page2.HasNextPage);

        // Page 3 of size 2
        var page3 = await service.GetActivityAsync(lawyerUserId, new LawyerActivityQuery(3, 2), CancellationToken.None);
        Assert.Single(page3.Items);
        Assert.False(page3.HasNextPage);
    }

    [Fact]
    public async Task GetActivityAsync_InvalidPage_ThrowsBusinessException()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);
        var service = CreateService(db);

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.GetActivityAsync(Guid.NewGuid(), new LawyerActivityQuery(0, 10), CancellationToken.None));
    }

    [Fact]
    public async Task GetEarningsAsync_WithEarnings_CalculatesTotalsAndPeriodPointsAccurately()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);
        var service = CreateService(db);
        var lawyerUserId = Guid.NewGuid();
        var clientUserId = Guid.NewGuid();
        var nowUtc = DateTimeOffset.UtcNow;

        // 1. Contract with released milestone
        var contract = new Contract(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            clientUserId,
            lawyerUserId,
            "عقد تجاري كبير",
            "شروط العقد",
            nowUtc.AddMonths(-2))
        {
            Status = ContractStatus.Active
        };
        db.Contracts.Add(contract);

        var milestone = new Milestone(
            Guid.NewGuid(),
            contract.Id,
            "مرحلة الصياغة",
            "صياغة الاتفاقية",
            1,
            4000m,
            14,
            nowUtc.AddMonths(-1),
            nowUtc.AddMonths(-2))
        {
            Status = MilestoneStatus.Released,
            UpdatedAt = nowUtc.AddMonths(-1)
        };
        db.Milestones.Add(milestone);

        // 2. Completed consultation
        var booking = new ConsultationBooking
        {
            Id = Guid.NewGuid(),
            OfferingId = Guid.NewGuid(),
            SlotId = Guid.NewGuid(),
            LawyerId = lawyerUserId,
            ClientId = clientUserId,
            OfferingTitle = "استشارة عامة",
            GrossAmount = 1000m,
            PlatformFeeAmount = 100m,
            LawyerNetAmount = 900m,
            Status = ConsultationBookingStatus.Completed,
            StartAtUtc = nowUtc.AddDays(-10),
            EndAtUtc = nowUtc.AddDays(-10).AddHours(1),
            CreatedAt = nowUtc.AddDays(-15),
            CompletedAtUtc = nowUtc.AddDays(-10),
            UpdatedAt = nowUtc.AddDays(-10)
        };
        db.ConsultationBookings.Add(booking);

        // 3. Wallet and recent withdrawal
        var wallet = new LawyerWallet(Guid.NewGuid(), lawyerUserId, nowUtc.AddMonths(-3))
        {
            AvailableBalance = 10000m,
            PendingBalance = 2000m,
            Currency = "EGP"
        };
        db.LawyerWallets.Add(wallet);

        var withdrawal = new WithdrawalRequest(
            Guid.NewGuid(),
            lawyerUserId,
            2500m,
            "idemp-payout-1",
            nowUtc.AddDays(-5))
        {
            Status = WithdrawalStatus.Completed,
            ProcessedAt = nowUtc.AddDays(-4)
        };
        db.WithdrawalRequests.Add(withdrawal);

        await db.SaveChangesAsync();

        var query = new LawyerEarningsQuery("6months", "monthly");
        var result = await service.GetEarningsAsync(lawyerUserId, query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(4900m, result.TotalEarnedInPeriod);
        Assert.Equal(4000m, result.ContractEarningsInPeriod);
        Assert.Equal(900m, result.ConsultationEarningsInPeriod);
        Assert.Equal(100m, result.PlatformFeesPaidInPeriod);
        Assert.Equal(10000m, result.CurrentAvailableBalance);
        Assert.Equal(2000m, result.CurrentPendingBalance);
        Assert.Equal("EGP", result.Currency);
        Assert.NotEmpty(result.PeriodBreakdown);
        Assert.Single(result.RecentWithdrawals);
        Assert.Equal(2500m, result.RecentWithdrawals[0].Amount);
    }

    [Fact]
    public async Task GetEarningsAsync_WeeklyGrouping_ReturnsWeeklyBuckets()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);
        var service = CreateService(db);
        var lawyerUserId = Guid.NewGuid();

        var query = new LawyerEarningsQuery("3months", "weekly");
        var result = await service.GetEarningsAsync(lawyerUserId, query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotEmpty(result.PeriodBreakdown);
        Assert.Equal(0m, result.TotalEarnedInPeriod);
    }

    [Fact]
    public async Task GetEarningsAsync_InvalidPeriod_ThrowsBusinessException()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);
        var service = CreateService(db);

        var query = new LawyerEarningsQuery("invalid_period", "monthly");
        await Assert.ThrowsAsync<BusinessException>(() =>
            service.GetEarningsAsync(Guid.NewGuid(), query, CancellationToken.None));
    }

    [Fact]
    public async Task GetUpcomingDeadlinesAsync_CalculatesUrgencyTiersAndOrdersByDueDate()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);
        var service = CreateService(db);
        var lawyerUserId = Guid.NewGuid();
        var clientUserId = Guid.NewGuid();
        var nowUtc = DateTimeOffset.UtcNow;

        var client = new ApplicationUser
        {
            Id = clientUserId,
            UserName = "client1@test.com",
            Email = "client1@test.com",
            FullName = "أحمد العميل"
        };
        db.Users.Add(client);

        var activeContract = new Contract(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            clientUserId,
            lawyerUserId,
            "عقد قضايا الشركات",
            "شروط العقد",
            nowUtc.AddMonths(-1))
        {
            Status = ContractStatus.Active,
            Currency = "EGP"
        };
        db.Contracts.Add(activeContract);

        // M1: Overdue (-2 days)
        var mOverdue = new Milestone(
            Guid.NewGuid(),
            activeContract.Id,
            "مرحلة متأخرة",
            "تسليم اللائحة",
            1,
            2000m,
            10,
            nowUtc.AddDays(-2),
            nowUtc.AddMonths(-1))
        {
            Status = MilestoneStatus.FundedInProgress
        };

        // M2: Critical (+1 day)
        var mCritical = new Milestone(
            Guid.NewGuid(),
            activeContract.Id,
            "مرحلة حرجة",
            "حضور الجلسة",
            2,
            3000m,
            10,
            nowUtc.AddDays(1),
            nowUtc.AddMonths(-1))
        {
            Status = MilestoneStatus.FundedInProgress
        };

        // M3: Approaching (+5 days)
        var mApproaching = new Milestone(
            Guid.NewGuid(),
            activeContract.Id,
            "مرحلة قريبة",
            "مذكرة دفاع",
            3,
            4000m,
            10,
            nowUtc.AddDays(5),
            nowUtc.AddMonths(-1))
        {
            Status = MilestoneStatus.FundedInProgress
        };

        // M4: Normal (+20 days)
        var mNormal = new Milestone(
            Guid.NewGuid(),
            activeContract.Id,
            "مرحلة عادية",
            "المرافعة الختامية",
            4,
            5000m,
            10,
            nowUtc.AddDays(20),
            nowUtc.AddMonths(-1))
        {
            Status = MilestoneStatus.FundedInProgress
        };

        // M5: Released (should be excluded)
        var mReleased = new Milestone(
            Guid.NewGuid(),
            activeContract.Id,
            "مرحلة مفرجة",
            "تم تسليمها",
            5,
            1000m,
            10,
            nowUtc.AddDays(3),
            nowUtc.AddMonths(-1))
        {
            Status = MilestoneStatus.Released
        };

        db.Milestones.AddRange(mOverdue, mCritical, mApproaching, mNormal, mReleased);
        await db.SaveChangesAsync();

        var query = new LawyerDeadlinesQuery(30);
        var result = await service.GetUpcomingDeadlinesAsync(lawyerUserId, query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(4, result.Count);

        // Verification of ordering and urgency
        Assert.Equal(mOverdue.Id, result[0].MilestoneId);
        Assert.Equal(DeadlineUrgency.Overdue, result[0].Urgency);
        Assert.Equal("أحمد العميل", result[0].ClientName);

        Assert.Equal(mCritical.Id, result[1].MilestoneId);
        Assert.Equal(DeadlineUrgency.Critical, result[1].Urgency);

        Assert.Equal(mApproaching.Id, result[2].MilestoneId);
        Assert.Equal(DeadlineUrgency.Approaching, result[2].Urgency);

        Assert.Equal(mNormal.Id, result[3].MilestoneId);
        Assert.Equal(DeadlineUrgency.Normal, result[3].Urgency);
    }

    [Fact]
    public async Task GetUpcomingDeadlinesAsync_InvalidDaysAhead_ThrowsBusinessException()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);
        var service = CreateService(db);

        var query = new LawyerDeadlinesQuery(0);
        await Assert.ThrowsAsync<BusinessException>(() =>
            service.GetUpcomingDeadlinesAsync(Guid.NewGuid(), query, CancellationToken.None));
    }

    [Fact]
    public async Task GetCalendarScheduleAsync_MergesConsultationsAndMilestonesChronologically()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);
        var service = CreateService(db);
        var lawyerUserId = Guid.NewGuid();
        var clientUserId = Guid.NewGuid();
        var nowUtc = DateTimeOffset.UtcNow;

        var client = new ApplicationUser
        {
            Id = clientUserId,
            UserName = "client2@test.com",
            Email = "client2@test.com",
            FullName = "سارة العميل"
        };
        db.Users.Add(client);

        // 1. Contract with Milestone deadline (+1 day)
        var activeContract = new Contract(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            clientUserId,
            lawyerUserId,
            "عقد استشارة قانونية",
            "شروط العقد",
            nowUtc.AddMonths(-1))
        {
            Status = ContractStatus.Active,
            Currency = "EGP"
        };
        db.Contracts.Add(activeContract);

        var milestone = new Milestone(
            Guid.NewGuid(),
            activeContract.Id,
            "تسليم اللائحة",
            "مراجعة وصياغة اللائحة",
            1,
            3000m,
            7,
            nowUtc.AddDays(1),
            nowUtc.AddMonths(-1))
        {
            Status = MilestoneStatus.FundedInProgress
        };
        db.Milestones.Add(milestone);

        // 2. Confirmed consultation booking (+2 days)
        var consultation = new ConsultationBooking
        {
            Id = Guid.NewGuid(),
            OfferingId = Guid.NewGuid(),
            SlotId = Guid.NewGuid(),
            LawyerId = lawyerUserId,
            ClientId = clientUserId,
            OfferingTitle = "استشارة تأسيس شركات",
            Subject = "استفسار عن الشركاء",
            Status = ConsultationBookingStatus.Confirmed,
            StartAtUtc = nowUtc.AddDays(2),
            EndAtUtc = nowUtc.AddDays(2).AddHours(1),
            MeetingUrl = "https://meet.smartcourt.eg/room-123",
            CreatedAt = nowUtc.AddDays(-1),
            UpdatedAt = nowUtc.AddDays(-1)
        };
        db.ConsultationBookings.Add(consultation);

        // 3. Cancelled consultation (should be excluded)
        var cancelledConsultation = new ConsultationBooking
        {
            Id = Guid.NewGuid(),
            OfferingId = Guid.NewGuid(),
            SlotId = Guid.NewGuid(),
            LawyerId = lawyerUserId,
            ClientId = clientUserId,
            OfferingTitle = "استشارة ملغاة",
            Subject = "تم الإلغاء",
            Status = ConsultationBookingStatus.Cancelled,
            StartAtUtc = nowUtc.AddDays(3),
            EndAtUtc = nowUtc.AddDays(3).AddHours(1),
            CreatedAt = nowUtc.AddDays(-2),
            UpdatedAt = nowUtc.AddDays(-1)
        };
        db.ConsultationBookings.Add(cancelledConsultation);

        await db.SaveChangesAsync();

        var query = new LawyerCalendarQuery(nowUtc.AddDays(-1), nowUtc.AddDays(10));
        var result = await service.GetCalendarScheduleAsync(lawyerUserId, query, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(2, result.Events.Count);

        // Event 1: Milestone (+1 day)
        Assert.Equal(CalendarEventType.MilestoneDeadline, result.Events[0].EventType);
        Assert.Equal(milestone.Id, result.Events[0].ReferenceId);
        Assert.Equal("سارة العميل", result.Events[0].ClientName);

        // Event 2: Consultation (+2 days)
        Assert.Equal(CalendarEventType.Consultation, result.Events[1].EventType);
        Assert.Equal(consultation.Id, result.Events[1].ReferenceId);
        Assert.Equal("https://meet.smartcourt.eg/room-123", result.Events[1].MeetingUrlOrLocation);
    }

    [Fact]
    public async Task GetCalendarScheduleAsync_InvalidDateRange_ThrowsBusinessException()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = CreateContext(dbName);
        var service = CreateService(db);
        var nowUtc = DateTimeOffset.UtcNow;

        // FromUtc > ToUtc
        var invalidQuery = new LawyerCalendarQuery(nowUtc.AddDays(5), nowUtc.AddDays(1));
        await Assert.ThrowsAsync<BusinessException>(() =>
            service.GetCalendarScheduleAsync(Guid.NewGuid(), invalidQuery, CancellationToken.None));
    }
}

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Enums;
using SmartCourt.Common.Models;
using SmartCourt.Common.Validation;
using SmartCourt.Features.Consultations.Domain.Enums;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Disputes.Enums;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Features.Ratings.Enums;
using SmartCourt.Features.Users.Lawyers.Dashboard.DTOs;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Users.Lawyers.Dashboard;

public sealed class LawyerDashboardService(
    ApplicationDbContext dbContext,
    IValidator<LawyerActivityQuery> activityQueryValidator,
    IValidator<LawyerEarningsQuery> earningsQueryValidator,
    IValidator<LawyerDeadlinesQuery> deadlinesQueryValidator,
    IValidator<LawyerCalendarQuery> calendarQueryValidator) : ILawyerDashboardService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly IValidator<LawyerActivityQuery> _activityQueryValidator = activityQueryValidator;
    private readonly IValidator<LawyerEarningsQuery> _earningsQueryValidator = earningsQueryValidator;
    private readonly IValidator<LawyerDeadlinesQuery> _deadlinesQueryValidator = deadlinesQueryValidator;
    private readonly IValidator<LawyerCalendarQuery> _calendarQueryValidator = calendarQueryValidator;

    public async Task<LawyerDashboardStatsDto> GetStatsAsync(
        Guid lawyerUserId,
        CancellationToken cancellationToken = default)
    {
        var nowUtc = DateTimeOffset.UtcNow;

        var activeContractsCount = await _dbContext.Contracts
            .AsNoTracking()
            .CountAsync(c => c.LawyerUserId == lawyerUserId && c.Status == ContractStatus.Active, cancellationToken);

        var pendingProposalsCount = await _dbContext.Proposals
            .AsNoTracking()
            .CountAsync(p => p.LawyerUserId == lawyerUserId && p.Status == ProposalStatus.Pending, cancellationToken);

        var recommendedCasesCount = await _dbContext.Cases
            .AsNoTracking()
            .CountAsync(c => c.Recommendations.Any(r => r.LawyerId == lawyerUserId)
                             && c.Status == CaseStatus.Matched
                             && c.LawyerId == null, cancellationToken);

        var upcomingConsultationsCount = await _dbContext.ConsultationBookings
            .AsNoTracking()
            .CountAsync(b => b.LawyerId == lawyerUserId
                             && b.Status == ConsultationBookingStatus.Confirmed
                             && b.StartAtUtc >= nowUtc, cancellationToken);

        var pendingConsultationsCount = await _dbContext.ConsultationBookings
            .AsNoTracking()
            .CountAsync(b => b.LawyerId == lawyerUserId
                             && b.Status == ConsultationBookingStatus.AwaitingPayment, cancellationToken);

        var wallet = await _dbContext.LawyerWallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.LawyerUserId == lawyerUserId, cancellationToken);

        var completedWithdrawalAmounts = await _dbContext.WithdrawalRequests
            .AsNoTracking()
            .Where(w => w.LawyerUserId == lawyerUserId && w.Status == WithdrawalStatus.Completed)
            .Select(w => w.Amount)
            .ToListAsync(cancellationToken);

        var totalWithdrawn = completedWithdrawalAmounts.Sum();

        var profile = await _dbContext.LawyerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == lawyerUserId, cancellationToken);

        var unreadNotificationsCount = await _dbContext.Notifications
            .AsNoTracking()
            .CountAsync(n => n.RecipientUserId == lawyerUserId && n.ReadAtUtc == null, cancellationToken);

        var lawyerContractIds = _dbContext.Contracts
            .AsNoTracking()
            .Where(c => c.LawyerUserId == lawyerUserId)
            .Select(c => c.Id);

        var activeDisputesCount = await _dbContext.Disputes
            .AsNoTracking()
            .CountAsync(d => lawyerContractIds.Contains(d.ContractId)
                             && d.Status != DisputeStatus.Resolved
                             && d.Status != DisputeStatus.Closed, cancellationToken);

        var availableBalance = wallet?.AvailableBalance ?? 0m;
        var pendingBalance = wallet?.PendingBalance ?? 0m;
        var currency = wallet?.Currency ?? "EGP";
        var lifetimeEarnings = totalWithdrawn + availableBalance;

        var averageRating = profile?.AverageRating ?? 0m;
        var totalReviewsCount = profile?.TotalRatingCount ?? 0;

        return new LawyerDashboardStatsDto(
            ActiveContractsCount: activeContractsCount,
            PendingProposalsCount: pendingProposalsCount,
            RecommendedCasesCount: recommendedCasesCount,
            UpcomingConsultationsCount: upcomingConsultationsCount,
            PendingConsultationRequestsCount: pendingConsultationsCount,
            AvailableBalance: availableBalance,
            PendingBalance: pendingBalance,
            LifetimeEarnings: lifetimeEarnings,
            Currency: currency,
            AverageRating: averageRating,
            TotalReviewsCount: totalReviewsCount,
            UnreadNotificationsCount: unreadNotificationsCount,
            ActiveDisputesCount: activeDisputesCount
        );
    }

    public async Task<PagedResult<LawyerActivityItemDto>> GetActivityAsync(
        Guid lawyerUserId,
        LawyerActivityQuery query,
        CancellationToken cancellationToken = default)
    {
        await _activityQueryValidator.ValidateAndThrowBusinessExceptionAsync(
            query,
            cancellationToken);

        // 1. Contract state changes
        var contractEvents = await (
            from c in _dbContext.Contracts.AsNoTracking()
            join h in _dbContext.ContractStateHistories.AsNoTracking() on c.Id equals h.ContractId
            where c.LawyerUserId == lawyerUserId
            select new LawyerActivityItemDto(
                h.Id,
                LawyerActivityType.ContractStateChanged,
                c.Title,
                string.IsNullOrWhiteSpace(h.Reason) ? $"تغيرت حالة العقد إلى {h.NewStatus}" : h.Reason,
                c.Id,
                "Contract",
                h.CreatedAt)
        ).ToListAsync(cancellationToken);

        // 2. Milestone state changes
        var milestoneEvents = await (
            from c in _dbContext.Contracts.AsNoTracking()
            join m in _dbContext.Milestones.AsNoTracking() on c.Id equals m.ContractId
            join h in _dbContext.MilestoneStateHistories.AsNoTracking() on m.Id equals h.MilestoneId
            where c.LawyerUserId == lawyerUserId
            select new LawyerActivityItemDto(
                h.Id,
                LawyerActivityType.MilestoneStateChanged,
                m.Title,
                string.IsNullOrWhiteSpace(h.Reason) ? $"تغيرت حالة المرحلة إلى {h.NewStatus}" : h.Reason,
                m.Id,
                "Milestone",
                h.CreatedAt)
        ).ToListAsync(cancellationToken);

        // 3. Proposal updates
        var proposalEvents = await _dbContext.Proposals
            .AsNoTracking()
            .Where(p => p.LawyerUserId == lawyerUserId)
            .Select(p => new LawyerActivityItemDto(
                p.Id,
                LawyerActivityType.ProposalStateChanged,
                "عرض أسعار",
                $"حالة العرض: {p.Status}",
                p.Id,
                "Proposal",
                p.UpdatedAt))
            .ToListAsync(cancellationToken);

        // 4. Consultations
        var consultationEvents = await _dbContext.ConsultationBookings
            .AsNoTracking()
            .Where(b => b.LawyerId == lawyerUserId)
            .Select(b => new LawyerActivityItemDto(
                b.Id,
                b.Status == ConsultationBookingStatus.Completed ? LawyerActivityType.ConsultationCompleted : LawyerActivityType.ConsultationBooked,
                b.OfferingTitle,
                $"استشارة: {b.Subject} (الحالة: {b.Status})",
                b.Id,
                "Consultation",
                b.Status == ConsultationBookingStatus.Completed && b.CompletedAtUtc.HasValue ? b.CompletedAtUtc.Value : b.CreatedAt))
            .ToListAsync(cancellationToken);

        // 5. Ratings
        var ratingEvents = await _dbContext.ContractRatings
            .AsNoTracking()
            .Where(r => r.RatedUserId == lawyerUserId && r.RaterRole == RaterRole.Client)
            .Select(r => new LawyerActivityItemDto(
                r.Id,
                LawyerActivityType.RatingReceived,
                "تقييم من عميل",
                $"حصلت على تقييم {r.Stars} نجوم",
                r.ContractId,
                "Contract",
                new DateTimeOffset(r.CreatedAt, TimeSpan.Zero)))
            .ToListAsync(cancellationToken);

        // 6. Disputes
        var disputeEvents = await (
            from c in _dbContext.Contracts.AsNoTracking()
            join d in _dbContext.Disputes.AsNoTracking() on c.Id equals d.ContractId
            where c.LawyerUserId == lawyerUserId
            select new LawyerActivityItemDto(
                d.Id,
                LawyerActivityType.DisputeRaised,
                d.Title,
                d.Description,
                d.Id,
                "Dispute",
                d.CreatedAt)
        ).ToListAsync(cancellationToken);

        var allEvents = contractEvents
            .Concat(milestoneEvents)
            .Concat(proposalEvents)
            .Concat(consultationEvents)
            .Concat(ratingEvents)
            .Concat(disputeEvents)
            .OrderByDescending(e => e.OccurredAtUtc)
            .ToList();

        var totalCount = allEvents.Count;
        var items = allEvents
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var hasNextPage = (query.Page * query.PageSize) < totalCount;
        return new PagedResult<LawyerActivityItemDto>(items, query.Page, query.PageSize, totalCount, hasNextPage);
    }

    public async Task<LawyerEarningsSummaryDto> GetEarningsAsync(
        Guid lawyerUserId,
        LawyerEarningsQuery query,
        CancellationToken cancellationToken = default)
    {
        await _earningsQueryValidator.ValidateAndThrowBusinessExceptionAsync(
            query,
            cancellationToken);

        var nowUtc = DateTimeOffset.UtcNow;
        var normalizedPeriod = query.Period?.Trim().ToLowerInvariant() ?? "6months";
        var normalizedGroupBy = query.GroupBy?.Trim().ToLowerInvariant() ?? "monthly";

        var startDateUtc = normalizedPeriod switch
        {
            "3months" => nowUtc.AddMonths(-3),
            "1year" => nowUtc.AddYears(-1),
            _ => nowUtc.AddMonths(-6)
        };

        // 1. Released milestones in period
        var releasedMilestones = await (
            from c in _dbContext.Contracts.AsNoTracking()
            join m in _dbContext.Milestones.AsNoTracking() on c.Id equals m.ContractId
            where c.LawyerUserId == lawyerUserId
                  && m.Status == MilestoneStatus.Released
                  && m.UpdatedAt >= startDateUtc
                  && m.UpdatedAt <= nowUtc
            select new {
                m.Amount,
                Date = m.UpdatedAt
            }
        ).ToListAsync(cancellationToken);

        // 2. Completed consultations in period
        var completedConsultations = await _dbContext.ConsultationBookings
            .AsNoTracking()
            .Where(b => b.LawyerId == lawyerUserId
                        && b.Status == ConsultationBookingStatus.Completed
                        && (b.CompletedAtUtc ?? b.UpdatedAt) >= startDateUtc
                        && (b.CompletedAtUtc ?? b.UpdatedAt) <= nowUtc)
            .Select(b => new {
                GrossAmount = b.GrossAmount,
                PlatformFee = b.PlatformFeeAmount,
                NetAmount = b.LawyerNetAmount,
                Date = b.CompletedAtUtc ?? b.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        // 3. Wallet
        var wallet = await _dbContext.LawyerWallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.LawyerUserId == lawyerUserId, cancellationToken);

        // 4. Recent withdrawals
        var recentWithdrawals = await _dbContext.WithdrawalRequests
            .AsNoTracking()
            .Where(w => w.LawyerUserId == lawyerUserId)
            .OrderByDescending(w => w.RequestedAt)
            .Take(5)
            .Select(w => new RecentPayoutDto(
                w.Id,
                w.Amount,
                w.Currency,
                w.Status.ToString(),
                w.RequestedAt,
                w.ProcessedAt))
            .ToListAsync(cancellationToken);

        // 5. Bucketing
        var periodPoints = new List<EarningsPeriodPointDto>();

        if (normalizedGroupBy == "weekly")
        {
            var cursor = startDateUtc;
            while (cursor <= nowUtc)
            {
                var bucketStart = cursor;
                var bucketEnd = cursor.AddDays(7);
                var label = $"{bucketStart:yyyy-MM-dd}";

                var contractSum = releasedMilestones
                    .Where(m => m.Date >= bucketStart && m.Date < bucketEnd)
                    .Sum(m => m.Amount);

                var consultationSum = completedConsultations
                    .Where(c => c.Date >= bucketStart && c.Date < bucketEnd)
                    .Sum(c => c.NetAmount);

                periodPoints.Add(new EarningsPeriodPointDto(
                    PeriodLabel: label,
                    PeriodStartUtc: bucketStart,
                    PeriodEndUtc: bucketEnd,
                    ContractAmount: contractSum,
                    ConsultationAmount: consultationSum,
                    TotalAmount: contractSum + consultationSum
                ));

                cursor = bucketEnd;
            }
        }
        else
        {
            var cursor = new DateTimeOffset(startDateUtc.Year, startDateUtc.Month, 1, 0, 0, 0, TimeSpan.Zero);
            var currentMonthEnd = new DateTimeOffset(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, TimeSpan.Zero).AddMonths(1);

            while (cursor < currentMonthEnd)
            {
                var bucketStart = cursor;
                var bucketEnd = cursor.AddMonths(1);
                var label = cursor.ToString("yyyy-MM");

                var contractSum = releasedMilestones
                    .Where(m => m.Date >= bucketStart && m.Date < bucketEnd)
                    .Sum(m => m.Amount);

                var consultationSum = completedConsultations
                    .Where(c => c.Date >= bucketStart && c.Date < bucketEnd)
                    .Sum(c => c.NetAmount);

                periodPoints.Add(new EarningsPeriodPointDto(
                    PeriodLabel: label,
                    PeriodStartUtc: bucketStart,
                    PeriodEndUtc: bucketEnd,
                    ContractAmount: contractSum,
                    ConsultationAmount: consultationSum,
                    TotalAmount: contractSum + consultationSum
                ));

                cursor = cursor.AddMonths(1);
            }
        }

        var contractEarningsTotal = releasedMilestones.Sum(m => m.Amount);
        var consultationEarningsTotal = completedConsultations.Sum(c => c.NetAmount);
        var platformFeesTotal = completedConsultations.Sum(c => c.PlatformFee);
        var totalEarned = contractEarningsTotal + consultationEarningsTotal;

        var availableBalance = wallet?.AvailableBalance ?? 0m;
        var pendingBalance = wallet?.PendingBalance ?? 0m;
        var currency = wallet?.Currency ?? "EGP";

        return new LawyerEarningsSummaryDto(
            TotalEarnedInPeriod: totalEarned,
            ContractEarningsInPeriod: contractEarningsTotal,
            ConsultationEarningsInPeriod: consultationEarningsTotal,
            PlatformFeesPaidInPeriod: platformFeesTotal,
            CurrentAvailableBalance: availableBalance,
            CurrentPendingBalance: pendingBalance,
            Currency: currency,
            PeriodBreakdown: periodPoints,
            RecentWithdrawals: recentWithdrawals
        );
    }

    public async Task<IReadOnlyList<UpcomingDeadlineItemDto>> GetUpcomingDeadlinesAsync(
        Guid lawyerUserId,
        LawyerDeadlinesQuery query,
        CancellationToken cancellationToken = default)
    {
        await _deadlinesQueryValidator.ValidateAndThrowBusinessExceptionAsync(
            query,
            cancellationToken);

        var nowUtc = DateTimeOffset.UtcNow;
        var maxDeadlineUtc = nowUtc.AddDays(query.DaysAhead);

        var excludedStatuses = new[]
        {
            MilestoneStatus.Released,
            MilestoneStatus.Refunded,
            MilestoneStatus.Cancelled
        };

        var rawMilestones = await (
            from c in _dbContext.Contracts.AsNoTracking()
            join m in _dbContext.Milestones.AsNoTracking() on c.Id equals m.ContractId
            join u in _dbContext.Users.AsNoTracking() on c.ClientUserId equals u.Id into users
            from clientUser in users.DefaultIfEmpty()
            where c.LawyerUserId == lawyerUserId
                  && c.Status == ContractStatus.Active
                  && !excludedStatuses.Contains(m.Status)
                  && m.DueDate.HasValue
                  && m.DueDate.Value <= maxDeadlineUtc
            select new {
                ContractId = c.Id,
                ContractTitle = c.Title,
                MilestoneId = m.Id,
                MilestoneTitle = m.Title,
                MilestoneOrder = m.OrderNumber,
                Amount = m.Amount,
                Currency = c.Currency,
                MilestoneStatus = m.Status.ToString(),
                DueDateUtc = m.DueDate,
                ClientId = c.ClientUserId,
                ClientName = clientUser != null && !string.IsNullOrWhiteSpace(clientUser.FullName)
                    ? clientUser.FullName
                    : "عميل"
            }
        ).ToListAsync(cancellationToken);

        var deadlineItems = rawMilestones
            .Select(m => {
                var dueDate = m.DueDateUtc!.Value;
                var totalDaysRemaining = (dueDate - nowUtc).TotalDays;
                var daysRemaining = (int)Math.Ceiling(totalDaysRemaining);

                DeadlineUrgency urgency;
                if (dueDate < nowUtc)
                {
                    urgency = DeadlineUrgency.Overdue;
                }
                else if (totalDaysRemaining <= 2.0)
                {
                    urgency = DeadlineUrgency.Critical;
                }
                else if (totalDaysRemaining <= 7.0)
                {
                    urgency = DeadlineUrgency.Approaching;
                }
                else
                {
                    urgency = DeadlineUrgency.Normal;
                }

                return new UpcomingDeadlineItemDto(
                    ContractId: m.ContractId,
                    ContractTitle: m.ContractTitle,
                    MilestoneId: m.MilestoneId,
                    MilestoneTitle: m.MilestoneTitle,
                    MilestoneOrder: m.MilestoneOrder,
                    Amount: m.Amount,
                    Currency: m.Currency,
                    MilestoneStatus: m.MilestoneStatus,
                    DueDateUtc: m.DueDateUtc,
                    DaysRemaining: daysRemaining,
                    Urgency: urgency,
                    ClientId: m.ClientId,
                    ClientName: m.ClientName
                );
            })
            .OrderBy(d => d.DueDateUtc)
            .ToList();

        return deadlineItems;
    }

    public async Task<LawyerCalendarScheduleDto> GetCalendarScheduleAsync(
        Guid lawyerUserId,
        LawyerCalendarQuery query,
        CancellationToken cancellationToken = default)
    {
        await _calendarQueryValidator.ValidateAndThrowBusinessExceptionAsync(
            query,
            cancellationToken);

        var nowUtc = DateTimeOffset.UtcNow;
        var fromUtc = query.FromUtc ?? new DateTimeOffset(nowUtc.Year, nowUtc.Month, nowUtc.Day, 0, 0, 0, TimeSpan.Zero);
        var toUtc = query.ToUtc ?? fromUtc.AddDays(30);

        // 1. Consultations within window
        var consultations = await (
            from b in _dbContext.ConsultationBookings.AsNoTracking()
            join u in _dbContext.Users.AsNoTracking() on b.ClientId equals u.Id into users
            from clientUser in users.DefaultIfEmpty()
            where b.LawyerId == lawyerUserId
                  && b.Status != ConsultationBookingStatus.Cancelled
                  && b.StartAtUtc >= fromUtc
                  && b.StartAtUtc <= toUtc
            select new LawyerCalendarEventDto(
                b.Id,
                CalendarEventType.Consultation,
                b.OfferingTitle,
                b.Subject,
                b.StartAtUtc,
                b.EndAtUtc,
                b.Status.ToString(),
                b.Id,
                "Consultation",
                b.ClientId,
                clientUser != null && !string.IsNullOrWhiteSpace(clientUser.FullName) ? clientUser.FullName : "عميل",
                !string.IsNullOrWhiteSpace(b.MeetingUrl) ? b.MeetingUrl : b.OfficeLocation
            )
        ).ToListAsync(cancellationToken);

        // 2. Milestone deadlines within window
        var excludedStatuses = new[]
        {
            MilestoneStatus.Released,
            MilestoneStatus.Refunded,
            MilestoneStatus.Cancelled
        };

        var milestones = await (
            from c in _dbContext.Contracts.AsNoTracking()
            join m in _dbContext.Milestones.AsNoTracking() on c.Id equals m.ContractId
            join u in _dbContext.Users.AsNoTracking() on c.ClientUserId equals u.Id into users
            from clientUser in users.DefaultIfEmpty()
            where c.LawyerUserId == lawyerUserId
                  && c.Status == ContractStatus.Active
                  && !excludedStatuses.Contains(m.Status)
                  && m.DueDate.HasValue
                  && m.DueDate.Value >= fromUtc
                  && m.DueDate.Value <= toUtc
            select new LawyerCalendarEventDto(
                m.Id,
                CalendarEventType.MilestoneDeadline,
                $"موعد تسليم: {m.Title}",
                m.Description,
                m.DueDate!.Value,
                null,
                m.Status.ToString(),
                m.Id,
                "Milestone",
                c.ClientUserId,
                clientUser != null && !string.IsNullOrWhiteSpace(clientUser.FullName) ? clientUser.FullName : "عميل",
                null
            )
        ).ToListAsync(cancellationToken);

        var allEvents = consultations
            .Concat(milestones)
            .OrderBy(e => e.StartUtc)
            .ToList();

        return new LawyerCalendarScheduleDto(
            FromUtc: fromUtc,
            ToUtc: toUtc,
            Events: allEvents
        );
    }
}

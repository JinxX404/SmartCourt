using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Features.Disputes.DTOs;
using SmartCourt.Features.Disputes.Entities;
using SmartCourt.Features.Users.Integration;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Disputes.Penalties;

public sealed class LawyerPenaltyService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IContractUserEligibilityService userEligibilityService,
    TimeProvider timeProvider)
    : ILawyerPenaltyService
{
    public async Task<PagedResult<LawyerPenaltyDto>> ListAsync(
        LawyerPenaltyFilterQuery query,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var eligibility = await userEligibilityService.FindEligibilityAsync(
            actorUserId,
            cancellationToken);
        if (eligibility is null
            || !eligibility.IsActive
            || (!eligibility.CanActAsModerator && !eligibility.CanActAsSuperAdministrator))
        {
            throw new ForbiddenAccessException(
                "الوصول إلى سجل العقوبات متاح للمشرفين المخولين فقط.");
        }

        var now = timeProvider.GetUtcNow();
        var penalties = dbContext.LawyerPenalties.AsNoTracking().AsQueryable();

        if (query.LawyerUserId.HasValue)
        {
            penalties = penalties.Where(p => p.LawyerUserId == query.LawyerUserId.Value);
        }

        if (query.PenaltyType.HasValue)
        {
            penalties = penalties.Where(p => p.PenaltyType == query.PenaltyType.Value);
        }

        if (query.IsActiveOnly == true)
        {
            penalties = penalties.Where(p => !p.IsRevoked && (!p.EndsAt.HasValue || p.EndsAt.Value > now));
        }

        var totalCount = await penalties.CountAsync(cancellationToken);
        var entities = await penalties
            .OrderByDescending(p => p.CreatedAt)
            .ThenBy(p => p.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var lawyerIds = entities.Select(p => p.LawyerUserId).Distinct().ToList();
        var lawyerNames = await dbContext.Users
            .AsNoTracking()
            .Where(u => lawyerIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName ?? string.Empty, cancellationToken);

        var items = entities.Select(p => MapToDto(p, lawyerNames, now)).ToList();

        return new PagedResult<LawyerPenaltyDto>(
            items,
            query.Page,
            query.PageSize,
            totalCount,
            query.Page * query.PageSize < totalCount);
    }

    public async Task<PagedResult<LawyerPenaltyDto>> GetMyPenaltiesAsync(
        LawyerPenaltyFilterQuery query,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var isLawyer = await (
            from userRole in dbContext.UserRoles
            join role in dbContext.Roles on userRole.RoleId equals role.Id
            where userRole.UserId == actorUserId && role.Name == "Lawyer"
            select userRole.UserId).AnyAsync(cancellationToken);

        if (!isLawyer)
        {
            throw new ForbiddenAccessException(
                "الوصول إلى سجل العقوبات الشخصي متاح للمحامين فقط.");
        }

        var now = timeProvider.GetUtcNow();
        var penalties = dbContext.LawyerPenalties
            .AsNoTracking()
            .Where(p => p.LawyerUserId == actorUserId);

        if (query.PenaltyType.HasValue)
        {
            penalties = penalties.Where(p => p.PenaltyType == query.PenaltyType.Value);
        }

        if (query.IsActiveOnly == true)
        {
            penalties = penalties.Where(p => !p.IsRevoked && (!p.EndsAt.HasValue || p.EndsAt.Value > now));
        }

        var totalCount = await penalties.CountAsync(cancellationToken);
        var entities = await penalties
            .OrderByDescending(p => p.CreatedAt)
            .ThenBy(p => p.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var currentUserName = await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == actorUserId)
            .Select(u => u.FullName ?? string.Empty)
            .SingleOrDefaultAsync(cancellationToken) ?? string.Empty;

        var lawyerNames = new Dictionary<Guid, string> { [actorUserId] = currentUserName };
        var items = entities.Select(p => MapToDto(p, lawyerNames, now)).ToList();

        return new PagedResult<LawyerPenaltyDto>(
            items,
            query.Page,
            query.PageSize,
            totalCount,
            query.Page * query.PageSize < totalCount);
    }

    public async Task<LawyerPenaltyDto> RevokeAsync(
        Guid penaltyId,
        RevokeLawyerPenaltyRequest request,
        CancellationToken cancellationToken)
    {
        var actorUserId = GetActorUserId();
        var eligibility = await userEligibilityService.FindEligibilityAsync(
            actorUserId,
            cancellationToken);
        if (eligibility is null || !eligibility.IsActive || !eligibility.CanActAsSuperAdministrator)
        {
            throw new ForbiddenAccessException(
                "إلغاء العقوبات متاح للمشرف العام فقط.");
        }

        var penalty = await dbContext.LawyerPenalties
            .SingleOrDefaultAsync(p => p.Id == penaltyId, cancellationToken)
            ?? throw new NotFoundException("العقوبة المطلوبة غير موجودة.");

        var now = timeProvider.GetUtcNow();
        penalty.Revoke(actorUserId, request.Reason.Trim(), now);
        await dbContext.SaveChangesAsync(cancellationToken);

        var lawyerName = await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == penalty.LawyerUserId)
            .Select(u => u.FullName ?? string.Empty)
            .SingleOrDefaultAsync(cancellationToken) ?? string.Empty;

        var lawyerNames = new Dictionary<Guid, string> { [penalty.LawyerUserId] = lawyerName };
        return MapToDto(penalty, lawyerNames, now);
    }

    private static LawyerPenaltyDto MapToDto(
        LawyerPenalty p,
        IReadOnlyDictionary<Guid, string> lawyerNames,
        DateTimeOffset now)
    {
        lawyerNames.TryGetValue(p.LawyerUserId, out var name);
        var isActive = !p.IsRevoked && (!p.EndsAt.HasValue || p.EndsAt.Value > now);
        return new LawyerPenaltyDto(
            p.Id,
            p.LawyerUserId,
            name ?? string.Empty,
            p.DisputeId,
            p.PenaltyType,
            p.Reason,
            isActive,
            p.StartsAt,
            p.EndsAt,
            p.IsRevoked,
            p.RevokedAt,
            p.RevokedByUserId,
            p.RevocationReason,
            p.CreatedAt);
    }

    private Guid GetActorUserId()
    {
        if (!currentUserService.IsAuthenticated
            || !currentUserService.UserId.HasValue
            || currentUserService.UserId.Value == Guid.Empty)
        {
            throw new AuthenticationException(
                "يجب تسجيل الدخول للوصول إلى العقوبات.");
        }

        return currentUserService.UserId.Value;
    }
}

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Common.Validation;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Ratings.DTOs;
using SmartCourt.Features.Ratings.Entities;
using SmartCourt.Features.Ratings.Enums;
using SmartCourt.Features.Users.Integration;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Ratings;

public sealed class RatingService(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IContractUserEligibilityService userEligibilityService,
    TimeProvider timeProvider,
    IValidator<SubmitRatingRequest> submitRatingValidator,
    IValidator<LawyerRatingsQuery> lawyerRatingsQueryValidator,
    IValidator<UpdateRatingRequest> updateRatingValidator) : IRatingService
{
    private const int RatingWindowDays = 14;

    public async Task<ContractRatingDto> SubmitAsync(
        Guid contractId,
        SubmitRatingRequest request,
        CancellationToken cancellationToken)
    {
        await submitRatingValidator.ValidateAndThrowBusinessExceptionAsync(
            request,
            cancellationToken);

        if (!currentUserService.IsAuthenticated || currentUserService.UserId is null)
        {
            throw new BusinessException("يجب تسجيل الدخول لتقييم العقد.");
        }

        var currentUserId = currentUserService.UserId.Value;

        var contract = await dbContext.Contracts
            .FirstOrDefaultAsync(c => c.Id == contractId, cancellationToken);

        if (contract is null)
        {
            throw new BusinessException("العقد غير موجود.");
        }

        if (contract.Status is not (ContractStatus.Completed or ContractStatus.Terminated or ContractStatus.CompletedOnHold))
        {
            throw new BusinessException("لا يمكن تقييم عقد لم ينتهِ بعد.");
        }

        var endedAt = contract.Status == ContractStatus.Completed
            ? contract.CompletedAt
            : contract.Status == ContractStatus.Terminated
                ? contract.TerminatedAt
                : contract.UpdatedAt;

        if (endedAt is null)
        {
            throw new BusinessException("تاريخ انتهاء العقد غير متوفر.");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (now > endedAt.Value.AddDays(RatingWindowDays))
        {
            throw new BusinessException($"انتهت مهلة التقييم المحددة بـ {RatingWindowDays} يومًا من تاريخ انتهاء العقد.");
        }

        RaterRole raterRole;
        Guid ratedUserId;

        if (currentUserId == contract.ClientUserId)
        {
            raterRole = RaterRole.Client;
            ratedUserId = contract.LawyerUserId;
        }
        else if (currentUserId == contract.LawyerUserId)
        {
            raterRole = RaterRole.Lawyer;
            ratedUserId = contract.ClientUserId;
        }
        else
        {
            throw new BusinessException("أنت لست طرفًا في هذا العقد.");
        }

        var hasAlreadyRated = await dbContext.ContractRatings
            .AnyAsync(
                r => r.ContractId == contractId && r.RaterUserId == currentUserId,
                cancellationToken);

        if (hasAlreadyRated)
        {
            throw new BusinessException("لقد قمت بتقديم تقييم لهذا العقد مسبقًا.");
        }

        var rating = new ContractRating(
            id: Guid.NewGuid(),
            contractId: contractId,
            raterUserId: currentUserId,
            ratedUserId: ratedUserId,
            raterRole: raterRole,
            stars: request.Stars,
            comment: request.Comment,
            createdAt: now);

        dbContext.ContractRatings.Add(rating);

        if (raterRole == RaterRole.Client)
        {
            var lawyerProfile = await dbContext.LawyerProfiles
                .FirstOrDefaultAsync(p => p.UserId == ratedUserId, cancellationToken);

            if (lawyerProfile is not null)
            {
                lawyerProfile.TotalRatingSum += request.Stars;
                lawyerProfile.TotalRatingCount += 1;
                lawyerProfile.AverageRating = Math.Round(
                    (decimal)lawyerProfile.TotalRatingSum / lawyerProfile.TotalRatingCount,
                    2,
                    MidpointRounding.AwayFromZero);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        var userNames = await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == currentUserId || u.Id == ratedUserId)
            .Select(u => new { u.Id, u.FullName })
            .ToDictionaryAsync(u => u.Id, u => u.FullName, cancellationToken);

        var raterName = userNames.GetValueOrDefault(currentUserId) ?? string.Empty;
        var ratedName = userNames.GetValueOrDefault(ratedUserId) ?? string.Empty;

        return MapToDto(rating, raterName, ratedName);
    }

    public async Task<ContractRatingDto> UpdateAsync(
        Guid contractId,
        UpdateRatingRequest request,
        CancellationToken cancellationToken)
    {
        await updateRatingValidator.ValidateAndThrowBusinessExceptionAsync(
            request,
            cancellationToken);

        if (!currentUserService.IsAuthenticated || currentUserService.UserId is null)
        {
            throw new BusinessException("يجب تسجيل الدخول لتعديل التقييم.");
        }

        var currentUserId = currentUserService.UserId.Value;

        var contract = await dbContext.Contracts
            .FirstOrDefaultAsync(c => c.Id == contractId, cancellationToken);

        if (contract is null)
        {
            throw new BusinessException("العقد غير موجود.");
        }

        if (contract.Status is not (ContractStatus.Completed or ContractStatus.Terminated or ContractStatus.CompletedOnHold))
        {
            throw new BusinessException("لا يمكن تعديل تقييم عقد لم ينتهِ بعد.");
        }

        var endedAt = contract.Status == ContractStatus.Completed
            ? contract.CompletedAt
            : contract.Status == ContractStatus.Terminated
                ? contract.TerminatedAt
                : contract.UpdatedAt;

        if (endedAt is null)
        {
            throw new BusinessException("تاريخ انتهاء العقد غير متوفر.");
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (now > endedAt.Value.AddDays(RatingWindowDays))
        {
            throw new BusinessException($"انتهت مهلة التعديل المحددة بـ {RatingWindowDays} يومًا من تاريخ انتهاء العقد.");
        }

        if (currentUserId != contract.ClientUserId && currentUserId != contract.LawyerUserId)
        {
            throw new BusinessException("أنت لست طرفًا في هذا العقد.");
        }

        var rating = await dbContext.ContractRatings
            .FirstOrDefaultAsync(
                r => r.ContractId == contractId && r.RaterUserId == currentUserId,
                cancellationToken);

        if (rating is null)
        {
            throw new BusinessException("لم تقم بتقديم تقييم لهذا العقد بعد.");
        }

        if (rating.RaterRole == RaterRole.Client)
        {
            var starDifference = request.Stars - rating.Stars;
            if (starDifference != 0)
            {
                var lawyerProfile = await dbContext.LawyerProfiles
                    .FirstOrDefaultAsync(p => p.UserId == rating.RatedUserId, cancellationToken);

                if (lawyerProfile is not null)
                {
                    lawyerProfile.TotalRatingSum += starDifference;
                    lawyerProfile.AverageRating = lawyerProfile.TotalRatingCount > 0
                        ? Math.Round(
                            (decimal)lawyerProfile.TotalRatingSum / lawyerProfile.TotalRatingCount,
                            2,
                            MidpointRounding.AwayFromZero)
                        : 0m;
                }
            }
        }

        rating.Update(request.Stars, request.Comment);

        await dbContext.SaveChangesAsync(cancellationToken);

        var userNames = await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == rating.RaterUserId || u.Id == rating.RatedUserId)
            .Select(u => new { u.Id, u.FullName })
            .ToDictionaryAsync(u => u.Id, u => u.FullName, cancellationToken);

        var raterName = userNames.GetValueOrDefault(rating.RaterUserId) ?? string.Empty;
        var ratedName = userNames.GetValueOrDefault(rating.RatedUserId) ?? string.Empty;

        return MapToDto(rating, raterName, ratedName);
    }

    public async Task<ContractRatingSummaryDto> GetByContractAsync(
        Guid contractId,
        CancellationToken cancellationToken)
    {
        if (!currentUserService.IsAuthenticated || currentUserService.UserId is null)
        {
            throw new BusinessException("يجب تسجيل الدخول لعرض تقييمات العقد.");
        }

        var currentUserId = currentUserService.UserId.Value;

        var contract = await dbContext.Contracts
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == contractId, cancellationToken);

        if (contract is null)
        {
            throw new BusinessException("العقد غير موجود.");
        }

        var eligibility = await userEligibilityService.FindEligibilityAsync(
            currentUserId,
            cancellationToken);

        var isAdminOrModerator = eligibility is not null
            && eligibility.UserId == currentUserId
            && (eligibility.CanActAsModerator || eligibility.CanActAsSuperAdministrator);

        var isClient = currentUserId == contract.ClientUserId;
        var isLawyer = currentUserId == contract.LawyerUserId;

        if (!isAdminOrModerator && !isClient && !isLawyer)
        {
            throw new BusinessException("أنت لست طرفًا في هذا العقد.");
        }

        var ratings = await dbContext.ContractRatings
            .AsNoTracking()
            .Where(r => r.ContractId == contractId)
            .ToListAsync(cancellationToken);

        var clientRating = ratings.FirstOrDefault(r => r.RaterRole == RaterRole.Client);
        var lawyerRating = ratings.FirstOrDefault(r => r.RaterRole == RaterRole.Lawyer);

        var bothSubmitted = clientRating is not null && lawyerRating is not null;

        var endedAt = contract.Status == ContractStatus.Completed
            ? contract.CompletedAt
            : contract.TerminatedAt;

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var windowExpired = endedAt.HasValue && now > endedAt.Value.AddDays(RatingWindowDays);

        var areRevealed = bothSubmitted || windowExpired;

        var userIds = ratings.Select(r => r.RaterUserId)
            .Concat(ratings.Select(r => r.RatedUserId))
            .Distinct()
            .ToList();

        var userNames = userIds.Count > 0
            ? await dbContext.Users
                .AsNoTracking()
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FullName })
                .ToDictionaryAsync(u => u.Id, u => u.FullName, cancellationToken)
            : new Dictionary<Guid, string>();

        ContractRatingDto? clientRatingDto = null;
        ContractRatingDto? lawyerRatingDto = null;

        if (isAdminOrModerator || areRevealed)
        {
            clientRatingDto = clientRating is null ? null : MapToDto(clientRating, userNames.GetValueOrDefault(clientRating.RaterUserId) ?? string.Empty, userNames.GetValueOrDefault(clientRating.RatedUserId) ?? string.Empty);
            lawyerRatingDto = lawyerRating is null ? null : MapToDto(lawyerRating, userNames.GetValueOrDefault(lawyerRating.RaterUserId) ?? string.Empty, userNames.GetValueOrDefault(lawyerRating.RatedUserId) ?? string.Empty);
        }
        else if (isClient)
        {
            clientRatingDto = clientRating is null ? null : MapToDto(clientRating, userNames.GetValueOrDefault(clientRating.RaterUserId) ?? string.Empty, userNames.GetValueOrDefault(clientRating.RatedUserId) ?? string.Empty);
            lawyerRatingDto = null;
        }
        else if (isLawyer)
        {
            clientRatingDto = null;
            lawyerRatingDto = lawyerRating is null ? null : MapToDto(lawyerRating, userNames.GetValueOrDefault(lawyerRating.RaterUserId) ?? string.Empty, userNames.GetValueOrDefault(lawyerRating.RatedUserId) ?? string.Empty);
        }

        return new ContractRatingSummaryDto(
            contractId,
            areRevealed,
            clientRatingDto,
            lawyerRatingDto);
    }

    public async Task<PagedResult<ContractRatingDto>> GetByLawyerAsync(
        Guid lawyerUserId,
        LawyerRatingsQuery query,
        CancellationToken cancellationToken)
    {
        await lawyerRatingsQueryValidator.ValidateAndThrowBusinessExceptionAsync(
            query,
            cancellationToken);

        var lawyerExists = await dbContext.LawyerProfiles
            .AnyAsync(p => p.UserId == lawyerUserId, cancellationToken);

        if (!lawyerExists)
        {
            throw new BusinessException("المحامي غير موجود.");
        }

        var queryable = from rating in dbContext.ContractRatings.AsNoTracking()
                        join contract in dbContext.Contracts.AsNoTracking() on rating.ContractId equals contract.Id
                        join rater in dbContext.Users.AsNoTracking() on rating.RaterUserId equals rater.Id into raterJoin
                        from rater in raterJoin.DefaultIfEmpty()
                        join rated in dbContext.Users.AsNoTracking() on rating.RatedUserId equals rated.Id into ratedJoin
                        from rated in ratedJoin.DefaultIfEmpty()
                        where rating.RatedUserId == lawyerUserId && rating.RaterRole == RaterRole.Client
                        select new
                        {
                            Rating = rating,
                            RaterName = rater != null ? rater.FullName : string.Empty,
                            RatedName = rated != null ? rated.FullName : string.Empty
                        };

        var totalCount = await queryable.CountAsync(cancellationToken);
        var pagedResults = await queryable
            .OrderByDescending(r => r.Rating.CreatedAt)
            .ThenBy(r => r.Rating.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var items = pagedResults
            .Select(x => MapToDto(x.Rating, x.RaterName, x.RatedName))
            .ToList();

        return new PagedResult<ContractRatingDto>(
            items,
            query.Page,
            query.PageSize,
            totalCount,
            query.Page * query.PageSize < totalCount);
    }

    private static ContractRatingDto MapToDto(ContractRating rating, string raterName, string ratedName) =>
        new(
            rating.Id,
            rating.ContractId,
            raterName,
            ratedName,
            rating.RaterRole,
            rating.Stars,
            rating.Comment,
            rating.CreatedAt);
}


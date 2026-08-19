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
    IValidator<LawyerRatingsQuery> lawyerRatingsQueryValidator) : IRatingService
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

        return MapToDto(rating);
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

        ContractRatingDto? clientRatingDto = null;
        ContractRatingDto? lawyerRatingDto = null;

        if (isAdminOrModerator || areRevealed)
        {
            clientRatingDto = clientRating is null ? null : MapToDto(clientRating);
            lawyerRatingDto = lawyerRating is null ? null : MapToDto(lawyerRating);
        }
        else if (isClient)
        {
            clientRatingDto = clientRating is null ? null : MapToDto(clientRating);
            lawyerRatingDto = null;
        }
        else if (isLawyer)
        {
            clientRatingDto = null;
            lawyerRatingDto = lawyerRating is null ? null : MapToDto(lawyerRating);
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

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var windowThreshold = now.AddDays(-RatingWindowDays);

        var queryable = from rating in dbContext.ContractRatings.AsNoTracking()
                        join contract in dbContext.Contracts.AsNoTracking() on rating.ContractId equals contract.Id
                        where rating.RatedUserId == lawyerUserId && rating.RaterRole == RaterRole.Client
                        where dbContext.ContractRatings.Any(other => other.ContractId == rating.ContractId && other.RaterRole == RaterRole.Lawyer)
                           || (contract.Status == ContractStatus.Completed && contract.CompletedAt <= windowThreshold)
                           || (contract.Status == ContractStatus.Terminated && contract.TerminatedAt <= windowThreshold)
                        select rating;

        var totalCount = await queryable.CountAsync(cancellationToken);
        var ratings = await queryable
            .OrderByDescending(r => r.CreatedAt)
            .ThenBy(r => r.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var items = ratings.Select(MapToDto).ToList();

        return new PagedResult<ContractRatingDto>(
            items,
            query.Page,
            query.PageSize,
            totalCount,
            query.Page * query.PageSize < totalCount);
    }

    private static ContractRatingDto MapToDto(ContractRating rating) =>
        new(
            rating.Id,
            rating.ContractId,
            rating.RaterUserId,
            rating.RatedUserId,
            rating.RaterRole,
            rating.Stars,
            rating.Comment,
            rating.CreatedAt);
}

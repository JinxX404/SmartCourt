using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Models;
using SmartCourt.Features.Consultations.Domain.Entities;
using SmartCourt.Features.Consultations.Domain.Enums;
using SmartCourt.Features.Consultations.DTOs;
using SmartCourt.Features.Consultations.Shared;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;
using SmartCourt.Providers.Payments;

namespace SmartCourt.Features.Consultations.Offerings;

public sealed record CreateConsultationOfferingCommand(CreateConsultationOfferingRequest Request)
    : IRequest<ApiResponse<ConsultationOfferingDto>>;
public sealed record UpdateConsultationOfferingCommand(Guid OfferingId, UpdateConsultationOfferingRequest Request)
    : IRequest<ApiResponse<ConsultationOfferingDto>>;
public sealed record SetConsultationOfferingStatusCommand(Guid OfferingId, bool IsActive)
    : IRequest<ApiResponse<ConsultationOfferingDto>>;
public sealed record GetMyConsultationOfferingsQuery
    : IRequest<ApiResponse<IReadOnlyList<ConsultationOfferingDto>>>;

public sealed class ConsultationOfferingHandler(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IPaymentProvider paymentProvider,
    IOptions<PaymentProviderOptions> paymentOptions,
    TimeProvider timeProvider,
    IValidator<CreateConsultationOfferingRequest> createValidator,
    IValidator<UpdateConsultationOfferingRequest> updateValidator)
    : IRequestHandler<CreateConsultationOfferingCommand, ApiResponse<ConsultationOfferingDto>>,
      IRequestHandler<UpdateConsultationOfferingCommand, ApiResponse<ConsultationOfferingDto>>,
      IRequestHandler<SetConsultationOfferingStatusCommand, ApiResponse<ConsultationOfferingDto>>,
      IRequestHandler<GetMyConsultationOfferingsQuery, ApiResponse<IReadOnlyList<ConsultationOfferingDto>>>
{
    public async Task<ApiResponse<ConsultationOfferingDto>> Handle(
        CreateConsultationOfferingCommand command,
        CancellationToken cancellationToken)
    {
        var validation = await createValidator.ValidateAsync(command.Request, cancellationToken);
        if (!validation.IsValid)
            return ApiResponse<ConsultationOfferingDto>.Fail(validation.Errors.Select(item => item.ErrorMessage).ToList());

        var lawyerId = ConsultationAccess.RequireUserId(currentUserService);
        var eligibilityError = await ValidateLawyerAsync(
            lawyerId, command.Request.Specialization, command.Request.IsActive, cancellationToken);
        if (eligibilityError is not null)
            return ApiResponse<ConsultationOfferingDto>.Fail(eligibilityError, 409);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var offering = new ConsultationOffering
        {
            Id = Guid.NewGuid(),
            LawyerId = lawyerId,
            Mode = command.Request.Mode,
            Specialization = command.Request.Specialization,
            Title = command.Request.Title.Trim(),
            Description = command.Request.Description.Trim(),
            DurationMinutes = command.Request.DurationMinutes,
            Price = command.Request.Price,
            OfficeLocation = NormalizeOffice(command.Request.Mode, command.Request.OfficeLocation),
            IsActive = command.Request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };
        ReplaceInclusions(offering, command.Request.Inclusions);
        dbContext.ConsultationOfferings.Add(offering);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ApiResponse<ConsultationOfferingDto>.Created(Map(offering, null, true));
    }

    public async Task<ApiResponse<ConsultationOfferingDto>> Handle(
        UpdateConsultationOfferingCommand command,
        CancellationToken cancellationToken)
    {
        var validation = await updateValidator.ValidateAsync(command.Request, cancellationToken);
        if (!validation.IsValid)
            return ApiResponse<ConsultationOfferingDto>.Fail(validation.Errors.Select(item => item.ErrorMessage).ToList());

        var lawyerId = ConsultationAccess.RequireUserId(currentUserService);
        var offering = await dbContext.ConsultationOfferings.Include(item => item.Inclusions)
            .SingleOrDefaultAsync(item => item.Id == command.OfferingId && item.LawyerId == lawyerId, cancellationToken);
        if (offering is null)
            return ApiResponse<ConsultationOfferingDto>.Fail("Consultation offering was not found.", 404);

        var eligibilityError = await ValidateLawyerAsync(
            lawyerId, command.Request.Specialization, offering.IsActive, cancellationToken);
        if (eligibilityError is not null)
            return ApiResponse<ConsultationOfferingDto>.Fail(eligibilityError, 409);

        var hasFutureCommitment = await dbContext.ConsultationBookings.AnyAsync(
            item => item.OfferingId == offering.Id
                && item.StartAtUtc > timeProvider.GetUtcNow().UtcDateTime
                && (item.Status == ConsultationBookingStatus.Confirmed
                    || item.Status == ConsultationBookingStatus.AwaitingClientConfirmation),
            cancellationToken);
        if (hasFutureCommitment
            && (offering.Mode != command.Request.Mode
                || offering.DurationMinutes != command.Request.DurationMinutes))
        {
            return ApiResponse<ConsultationOfferingDto>.Fail(
                "Mode and duration cannot change while the offering has future paid bookings.", 409);
        }

        offering.Mode = command.Request.Mode;
        offering.Specialization = command.Request.Specialization;
        offering.Title = command.Request.Title.Trim();
        offering.Description = command.Request.Description.Trim();
        offering.DurationMinutes = command.Request.DurationMinutes;
        offering.Price = command.Request.Price;
        offering.OfficeLocation = NormalizeOffice(command.Request.Mode, command.Request.OfficeLocation);
        offering.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
        dbContext.ConsultationOfferingInclusions.RemoveRange(offering.Inclusions);
        offering.Inclusions.Clear();
        ReplaceInclusions(offering, command.Request.Inclusions);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ApiResponse<ConsultationOfferingDto>.Ok(Map(offering, null, true));
    }

    public async Task<ApiResponse<ConsultationOfferingDto>> Handle(
        SetConsultationOfferingStatusCommand command,
        CancellationToken cancellationToken)
    {
        var lawyerId = ConsultationAccess.RequireUserId(currentUserService);
        var offering = await dbContext.ConsultationOfferings.Include(item => item.Inclusions)
            .SingleOrDefaultAsync(item => item.Id == command.OfferingId && item.LawyerId == lawyerId, cancellationToken);
        if (offering is null)
            return ApiResponse<ConsultationOfferingDto>.Fail("Consultation offering was not found.", 404);

        if (command.IsActive)
        {
            var error = await ValidateLawyerAsync(lawyerId, offering.Specialization, true, cancellationToken);
            if (error is not null)
                return ApiResponse<ConsultationOfferingDto>.Fail(error, 409);
        }

        offering.IsActive = command.IsActive;
        offering.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
        await dbContext.SaveChangesAsync(cancellationToken);
        var next = await NextAvailableAsync(offering.Id, cancellationToken);
        return ApiResponse<ConsultationOfferingDto>.Ok(Map(offering, next, true));
    }

    public async Task<ApiResponse<IReadOnlyList<ConsultationOfferingDto>>> Handle(
        GetMyConsultationOfferingsQuery request,
        CancellationToken cancellationToken)
    {
        var lawyerId = ConsultationAccess.RequireUserId(currentUserService);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var offerings = await dbContext.ConsultationOfferings.AsNoTracking()
            .Include(item => item.Inclusions)
            .Where(item => item.LawyerId == lawyerId)
            .OrderByDescending(item => item.IsActive).ThenBy(item => item.Title)
            .ToListAsync(cancellationToken);
        var nextSlots = await dbContext.ConsultationAvailabilitySlots.AsNoTracking()
            .Where(slot => slot.LawyerId == lawyerId && slot.StartAtUtc > now
                && (slot.Status == ConsultationSlotStatus.Available
                    || slot.Status == ConsultationSlotStatus.Reserved && slot.ReservedUntilUtc <= now))
            .GroupBy(slot => slot.OfferingId)
            .Select(group => new { OfferingId = group.Key, Next = group.Min(item => item.StartAtUtc) })
            .ToDictionaryAsync(item => item.OfferingId, item => (DateTime?)item.Next, cancellationToken);
        return ApiResponse<IReadOnlyList<ConsultationOfferingDto>>.Ok(
            offerings.Select(item => Map(item, nextSlots.GetValueOrDefault(item.Id), true)).ToList());
    }

    private async Task<string?> ValidateLawyerAsync(
        Guid lawyerId,
        Common.Enums.Specialization specialization,
        bool activating,
        CancellationToken cancellationToken)
    {
        var hasSpecialization = await dbContext.LawyerSpecializations.AnyAsync(
            item => item.LawyerProfileUserId == lawyerId && item.Specialization == specialization,
            cancellationToken);
        if (!hasSpecialization)
            return "The offering specialization must belong to the lawyer's verified profile.";

        if (!activating || paymentProvider is not ILawyerPayoutAccountProvider)
            return null;

        var providerCode = paymentOptions.Value.ProviderCode;
        var payoutReady = await dbContext.LawyerPayoutAccounts.AnyAsync(
            item => item.LawyerUserId == lawyerId
                && item.ProviderCode == providerCode
                && item.Status == LawyerPayoutAccountStatus.Enabled
                && item.TransfersEnabled,
            cancellationToken);
        return payoutReady ? null : "Complete and enable the payout account before activating consultations.";
    }

    private async Task<DateTime?> NextAvailableAsync(Guid offeringId, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        return await dbContext.ConsultationAvailabilitySlots
            .Where(item => item.OfferingId == offeringId && item.StartAtUtc > now
                && item.Status == ConsultationSlotStatus.Available)
            .MinAsync(item => (DateTime?)item.StartAtUtc, cancellationToken);
    }

    internal static ConsultationOfferingDto Map(
        ConsultationOffering item,
        DateTime? nextAvailable,
        bool includePrivateLocation) => new(
            item.Id, item.LawyerId, item.Mode, item.Specialization, item.Title,
            item.Description, item.DurationMinutes, item.Price, item.Currency,
            includePrivateLocation ? item.OfficeLocation : null,
            item.IsActive,
            item.Inclusions.OrderBy(value => value.SortOrder).Select(value => value.Text).ToList(),
            nextAvailable);

    private static void ReplaceInclusions(ConsultationOffering offering, IReadOnlyList<string> inclusions)
    {
        for (var index = 0; index < inclusions.Count; index++)
            offering.Inclusions.Add(new ConsultationOfferingInclusion
            {
                Id = Guid.NewGuid(), OfferingId = offering.Id,
                Text = inclusions[index].Trim(), SortOrder = index
            });
    }

    private static string? NormalizeOffice(ConsultationMode mode, string? location) =>
        mode == ConsultationMode.InOffice ? location?.Trim() : null;
}

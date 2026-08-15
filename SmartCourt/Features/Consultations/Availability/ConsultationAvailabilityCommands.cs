using System.Data;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Models;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Consultations.Domain.Entities;
using SmartCourt.Features.Consultations.Domain.Enums;
using SmartCourt.Features.Consultations.DTOs;
using SmartCourt.Features.Consultations.Shared;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Consultations.Availability;

public sealed record CreateConsultationSlotsCommand(
    Guid OfferingId,
    CreateConsultationSlotsRequest Request)
    : IRequest<ApiResponse<IReadOnlyList<ConsultationSlotDto>>>;

public sealed record CancelConsultationSlotCommand(Guid SlotId)
    : IRequest<ApiResponse<ConsultationSlotDto>>;

public sealed record GetConsultationSlotsQuery(
    Guid OfferingId,
    DateTime? FromUtc,
    DateTime? ToUtc,
    bool Mine = false)
    : IRequest<ApiResponse<IReadOnlyList<ConsultationSlotDto>>>;

public sealed class ConsultationAvailabilityHandler(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    TimeProvider timeProvider,
    IValidator<CreateConsultationSlotsRequest> validator)
    : IRequestHandler<CreateConsultationSlotsCommand, ApiResponse<IReadOnlyList<ConsultationSlotDto>>>,
      IRequestHandler<CancelConsultationSlotCommand, ApiResponse<ConsultationSlotDto>>,
      IRequestHandler<GetConsultationSlotsQuery, ApiResponse<IReadOnlyList<ConsultationSlotDto>>>
{
    public async Task<ApiResponse<IReadOnlyList<ConsultationSlotDto>>> Handle(
        CreateConsultationSlotsCommand command,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command.Request, cancellationToken);
        if (!validation.IsValid)
            return ApiResponse<IReadOnlyList<ConsultationSlotDto>>.Fail(
                validation.Errors.Select(item => item.ErrorMessage).ToList());

        var lawyerId = ConsultationAccess.RequireUserId(currentUserService);
        var offering = await dbContext.ConsultationOfferings.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == command.OfferingId && item.LawyerId == lawyerId, cancellationToken);
        if (offering is null)
            return ApiResponse<IReadOnlyList<ConsultationSlotDto>>.Fail("Consultation offering was not found.", 404);

        var settings = await dbContext.LawyerConsultationSettings.AsNoTracking()
            .SingleOrDefaultAsync(item => item.LawyerId == lawyerId, cancellationToken);
        if (settings is null)
            return ApiResponse<IReadOnlyList<ConsultationSlotDto>>.Fail(
                "Configure consultation settings before adding availability.", 409);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var earliest = now.AddHours(settings.MinimumBookingNoticeHours);
        var latest = now.AddDays(settings.MaximumAdvanceBookingDays);
        var candidates = command.Request.Slots
            .Select(item => new
            {
                Start = item.StartAtUtc,
                End = item.StartAtUtc.AddMinutes(offering.DurationMinutes)
            }).OrderBy(item => item.Start).ToList();

        if (candidates.Any(item => item.Start < earliest || item.Start > latest))
            return ApiResponse<IReadOnlyList<ConsultationSlotDto>>.Fail(
                "Slots must respect the configured booking notice and advance-booking window.");

        for (var index = 1; index < candidates.Count; index++)
        {
            if (candidates[index].Start < candidates[index - 1].End.AddMinutes(settings.BufferMinutes))
                return ApiResponse<IReadOnlyList<ConsultationSlotDto>>.Fail(
                    "Submitted slots overlap or do not respect the configured buffer.", 409);
        }

        await using var transaction = dbContext.Database.IsRelational()
            ? await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
            : null;

        var rangeStart = candidates.Min(item => item.Start).AddMinutes(-settings.BufferMinutes);
        var rangeEnd = candidates.Max(item => item.End).AddMinutes(settings.BufferMinutes);
        var existing = await dbContext.ConsultationAvailabilitySlots
            .Where(item => item.LawyerId == lawyerId
                && item.Status != ConsultationSlotStatus.Cancelled
                && item.StartAtUtc < rangeEnd && item.EndAtUtc > rangeStart)
            .ToListAsync(cancellationToken);
        if (candidates.Any(candidate => existing.Any(slot =>
                candidate.Start < slot.EndAtUtc.AddMinutes(settings.BufferMinutes)
                && candidate.End.AddMinutes(settings.BufferMinutes) > slot.StartAtUtc)))
            return ApiResponse<IReadOnlyList<ConsultationSlotDto>>.Fail(
                "One or more slots overlap existing lawyer availability.", 409);

        var slots = candidates.Select(candidate => new ConsultationAvailabilitySlot
        {
            Id = Guid.NewGuid(), LawyerId = lawyerId, OfferingId = offering.Id,
            StartAtUtc = candidate.Start, EndAtUtc = candidate.End,
            Status = ConsultationSlotStatus.Available,
            CreatedAt = now, UpdatedAt = now
        }).ToList();
        dbContext.ConsultationAvailabilitySlots.AddRange(slots);
        await dbContext.SaveChangesAsync(cancellationToken);
        if (transaction is not null)
            await transaction.CommitAsync(cancellationToken);

        return ApiResponse<IReadOnlyList<ConsultationSlotDto>>.Created(slots.Select(Map).ToList());
    }

    public async Task<ApiResponse<ConsultationSlotDto>> Handle(
        CancelConsultationSlotCommand command,
        CancellationToken cancellationToken)
    {
        var lawyerId = ConsultationAccess.RequireUserId(currentUserService);
        var slot = await dbContext.ConsultationAvailabilitySlots.SingleOrDefaultAsync(
            item => item.Id == command.SlotId && item.LawyerId == lawyerId,
            cancellationToken);
        if (slot is null)
            return ApiResponse<ConsultationSlotDto>.Fail("Consultation slot was not found.", 404);
        if (slot.Status is ConsultationSlotStatus.Booked or ConsultationSlotStatus.Reserved)
            return ApiResponse<ConsultationSlotDto>.Fail(
                "A reserved or booked slot must be handled through its booking.", 409);

        slot.Status = ConsultationSlotStatus.Cancelled;
        slot.ReservedUntilUtc = null;
        slot.UpdatedAt = timeProvider.GetUtcNow().UtcDateTime;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ApiResponse<ConsultationSlotDto>.Ok(Map(slot));
    }

    public async Task<ApiResponse<IReadOnlyList<ConsultationSlotDto>>> Handle(
        GetConsultationSlotsQuery query,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var from = query.FromUtc ?? now;
        var to = query.ToUtc ?? now.AddDays(30);
        if (from.Kind != DateTimeKind.Utc || to.Kind != DateTimeKind.Utc || from > to || to > now.AddDays(366))
            return ApiResponse<IReadOnlyList<ConsultationSlotDto>>.Fail("A valid UTC availability range is required.");

        Guid? lawyerId = null;
        if (query.Mine)
            lawyerId = ConsultationAccess.RequireUserId(currentUserService);

        var offeringQuery = dbContext.ConsultationOfferings.AsNoTracking()
            .Where(item => item.Id == query.OfferingId);
        if (query.Mine)
            offeringQuery = offeringQuery.Where(item => item.LawyerId == lawyerId);
        else
            offeringQuery = offeringQuery.Where(item => item.IsActive
                && dbContext.LawyerConsultationSettings.Any(settings =>
                    settings.LawyerId == item.LawyerId && settings.IsEnabled)
                && dbContext.Users.Any(user => user.Id == item.LawyerId
                    && user.Status == UserStatus.Active && user.EmailConfirmed));
        if (!await offeringQuery.AnyAsync(cancellationToken))
            return ApiResponse<IReadOnlyList<ConsultationSlotDto>>.Fail("Consultation offering was not found.", 404);

        var slots = await dbContext.ConsultationAvailabilitySlots
            .Where(item => item.OfferingId == query.OfferingId
                && item.StartAtUtc >= from && item.StartAtUtc <= to
                && (query.Mine || item.Status == ConsultationSlotStatus.Available
                    || item.Status == ConsultationSlotStatus.Reserved && item.ReservedUntilUtc <= now))
            .OrderBy(item => item.StartAtUtc)
            .ToListAsync(cancellationToken);

        foreach (var expired in slots.Where(item => item.Status == ConsultationSlotStatus.Reserved
                     && item.ReservedUntilUtc <= now))
        {
            expired.Status = ConsultationSlotStatus.Available;
            expired.ReservedUntilUtc = null;
            expired.UpdatedAt = now;
        }
        if (dbContext.ChangeTracker.HasChanges())
            await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<IReadOnlyList<ConsultationSlotDto>>.Ok(
            slots.Where(item => query.Mine || item.Status == ConsultationSlotStatus.Available)
                .Select(Map).ToList());
    }

    private static ConsultationSlotDto Map(ConsultationAvailabilitySlot item) => new(
        item.Id, item.OfferingId, item.StartAtUtc, item.EndAtUtc,
        item.Status, item.ReservedUntilUtc);
}

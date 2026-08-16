using System.Data;
using System.Text.Json;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Models;
using SmartCourt.Features.Consultations.Domain.Entities;
using SmartCourt.Features.Consultations.Domain.Enums;
using SmartCourt.Features.Consultations.DTOs;
using SmartCourt.Features.Consultations.Events;
using SmartCourt.Features.Consultations.Payments;
using SmartCourt.Features.Consultations.Shared;
using SmartCourt.Interfaces;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Consultations.Bookings;

public sealed record CreateConsultationBookingCommand(CreateConsultationBookingRequest Request)
    : IRequest<ApiResponse<ConsultationBookingDto>>;
public sealed record GetConsultationBookingQuery(Guid BookingId)
    : IRequest<ApiResponse<ConsultationBookingDto>>;
public sealed record GetClientConsultationBookingsQuery(ConsultationBookingFilter Filter)
    : IRequest<ApiResponse<ConsultationPageDto<ConsultationBookingDto>>>;
public sealed record GetLawyerConsultationBookingsQuery(ConsultationBookingFilter Filter)
    : IRequest<ApiResponse<ConsultationPageDto<ConsultationBookingDto>>>;
public sealed record CancelConsultationBookingCommand(Guid BookingId, string Reason)
    : IRequest<ApiResponse<ConsultationBookingDto>>;
public sealed record MarkConsultationPerformedCommand(Guid BookingId, string? MeetingUrl)
    : IRequest<ApiResponse<ConsultationBookingDto>>;
public sealed record SetConsultationDeliveryDetailsCommand(Guid BookingId, string MeetingUrl)
    : IRequest<ApiResponse<ConsultationBookingDto>>;
public sealed record ConfirmConsultationCompletionCommand(Guid BookingId)
    : IRequest<ApiResponse<ConsultationBookingDto>>;
public sealed record OpenConsultationDisputeCommand(Guid BookingId, string Reason)
    : IRequest<ApiResponse<ConsultationBookingDto>>;
public sealed record SettleConsultationDisputeCommand(Guid BookingId, decimal ClientRefundAmount, string Reason)
    : IRequest<ApiResponse<ConsultationBookingDto>>;

public interface IConsultationJobService
{
    Task ExpireUnpaidBookingAsync(Guid bookingId, CancellationToken cancellationToken);
    Task AutoCompleteAsync(Guid bookingId, CancellationToken cancellationToken);
    Task ReleaseAsync(Guid bookingId, CancellationToken cancellationToken);
}

public sealed class ConsultationBookingHandler(
    ApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IConsultationPaymentService paymentService,
    IBackgroundJobProvider backgroundJobs,
    IOutboxWriter outboxWriter,
    TimeProvider timeProvider,
    IValidator<CreateConsultationBookingRequest> createValidator,
    IValidator<ConsultationBookingFilter> filterValidator)
    : IRequestHandler<CreateConsultationBookingCommand, ApiResponse<ConsultationBookingDto>>,
      IRequestHandler<GetConsultationBookingQuery, ApiResponse<ConsultationBookingDto>>,
      IRequestHandler<GetClientConsultationBookingsQuery, ApiResponse<ConsultationPageDto<ConsultationBookingDto>>>,
      IRequestHandler<GetLawyerConsultationBookingsQuery, ApiResponse<ConsultationPageDto<ConsultationBookingDto>>>,
      IRequestHandler<CancelConsultationBookingCommand, ApiResponse<ConsultationBookingDto>>,
      IRequestHandler<MarkConsultationPerformedCommand, ApiResponse<ConsultationBookingDto>>,
      IRequestHandler<SetConsultationDeliveryDetailsCommand, ApiResponse<ConsultationBookingDto>>,
      IRequestHandler<ConfirmConsultationCompletionCommand, ApiResponse<ConsultationBookingDto>>,
      IRequestHandler<OpenConsultationDisputeCommand, ApiResponse<ConsultationBookingDto>>,
      IRequestHandler<SettleConsultationDisputeCommand, ApiResponse<ConsultationBookingDto>>
{
    public async Task<ApiResponse<ConsultationBookingDto>> Handle(
        CreateConsultationBookingCommand command,
        CancellationToken cancellationToken)
    {
        var validation = await createValidator.ValidateAsync(command.Request, cancellationToken);
        if (!validation.IsValid)
            return ApiResponse<ConsultationBookingDto>.Fail(validation.Errors.Select(item => item.ErrorMessage).ToList());

        var clientId = ConsultationAccess.RequireUserId(currentUserService);
        if (!await ConsultationAccess.HasRoleAsync(dbContext, clientId, "Client", cancellationToken))
            return ApiResponse<ConsultationBookingDto>.Fail("Only clients can book consultations.", 403);

        var now = timeProvider.GetUtcNow();
        await using var transaction = dbContext.Database.IsRelational()
            ? await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken)
            : null;
        var offering = await dbContext.ConsultationOfferings.Include(item => item.Inclusions)
            .SingleOrDefaultAsync(item => item.Id == command.Request.OfferingId && item.IsActive, cancellationToken);
        if (offering is null || offering.LawyerId == clientId)
            return ApiResponse<ConsultationBookingDto>.Fail("Consultation offering is not bookable.", 409);
        var settings = await dbContext.LawyerConsultationSettings.AsNoTracking()
            .SingleOrDefaultAsync(item => item.LawyerId == offering.LawyerId && item.IsEnabled, cancellationToken);
        if (settings is null)
            return ApiResponse<ConsultationBookingDto>.Fail("The lawyer is not accepting consultations.", 409);

        var slot = await dbContext.ConsultationAvailabilitySlots.SingleOrDefaultAsync(
            item => item.Id == command.Request.SlotId && item.OfferingId == offering.Id,
            cancellationToken);
        if (slot is null)
            return ApiResponse<ConsultationBookingDto>.Fail("Consultation slot was not found.", 404);
        if (slot.Status == ConsultationSlotStatus.Reserved && slot.ReservedUntilUtc <= now)
        {
            slot.Status = ConsultationSlotStatus.Available;
            slot.ReservedUntilUtc = null;
        }
        if (slot.Status != ConsultationSlotStatus.Available
            || slot.StartAtUtc < now.AddHours(settings.MinimumBookingNoticeHours))
            return ApiResponse<ConsultationBookingDto>.Fail("The selected slot is no longer available.", 409);

        var settlement = ConsultationPolicy.CalculateSettlement(offering.Price);
        var paymentExpires = now.AddMinutes(ConsultationPolicy.PaymentReservationMinutes);
        var booking = new ConsultationBooking
        {
            Id = Guid.NewGuid(), OfferingId = offering.Id, SlotId = slot.Id,
            LawyerId = offering.LawyerId, ClientId = clientId,
            Mode = offering.Mode, Specialization = offering.Specialization,
            OfferingTitle = offering.Title, OfferingDescription = offering.Description,
            InclusionsJson = JsonSerializer.Serialize(
                offering.Inclusions.OrderBy(item => item.SortOrder).Select(item => item.Text)),
            DurationMinutes = offering.DurationMinutes,
            GrossAmount = offering.Price, PlatformFeeAmount = settlement.Fee,
            LawyerNetAmount = settlement.Net, Currency = offering.Currency,
            Subject = command.Request.Subject.Trim(), MatterSummary = command.Request.MatterSummary.Trim(),
            OfficeLocation = offering.OfficeLocation,
            StartAtUtc = slot.StartAtUtc, EndAtUtc = slot.EndAtUtc,
            Status = ConsultationBookingStatus.AwaitingPayment,
            PaymentExpiresAtUtc = paymentExpires, CreatedAt = now, UpdatedAt = now
        };
        slot.Status = ConsultationSlotStatus.Reserved;
        slot.ReservedUntilUtc = paymentExpires;
        slot.UpdatedAt = now;
        dbContext.ConsultationBookings.Add(booking);
        await ConsultationOutbox.EnqueueAsync(
            outboxWriter, ConsultationEventTypes.BookingCreated,
            booking, clientId, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        if (transaction is not null)
            await transaction.CommitAsync(cancellationToken);

        await backgroundJobs.ScheduleAsync<IConsultationJobService>(
            service => service.ExpireUnpaidBookingAsync(booking.Id, CancellationToken.None),
            paymentExpires, cancellationToken);
        return ApiResponse<ConsultationBookingDto>.Created(
            (await ConsultationReadModel.FindBookingAsync(
                dbContext, booking.Id, clientId, false, cancellationToken))!);
    }

    public async Task<ApiResponse<ConsultationBookingDto>> Handle(
        GetConsultationBookingQuery query,
        CancellationToken cancellationToken)
    {
        var actorId = ConsultationAccess.RequireUserId(currentUserService);
        var isAdmin = await IsAdministratorAsync(actorId, cancellationToken);
        var booking = await ConsultationReadModel.FindBookingAsync(
            dbContext, query.BookingId, actorId, isAdmin, cancellationToken);
        return booking is null
            ? ApiResponse<ConsultationBookingDto>.Fail("Consultation booking was not found.", 404)
            : ApiResponse<ConsultationBookingDto>.Ok(booking);
    }

    public Task<ApiResponse<ConsultationPageDto<ConsultationBookingDto>>> Handle(
        GetClientConsultationBookingsQuery query,
        CancellationToken cancellationToken) =>
        ListAsync(query.Filter, clientScope: true, cancellationToken);

    public Task<ApiResponse<ConsultationPageDto<ConsultationBookingDto>>> Handle(
        GetLawyerConsultationBookingsQuery query,
        CancellationToken cancellationToken) =>
        ListAsync(query.Filter, clientScope: false, cancellationToken);

    public async Task<ApiResponse<ConsultationBookingDto>> Handle(
        CancelConsultationBookingCommand command,
        CancellationToken cancellationToken)
    {
        var actorId = ConsultationAccess.RequireUserId(currentUserService);
        var booking = await dbContext.ConsultationBookings.SingleOrDefaultAsync(
            item => item.Id == command.BookingId
                && (item.ClientId == actorId || item.LawyerId == actorId), cancellationToken);
        if (booking is null)
            return ApiResponse<ConsultationBookingDto>.Fail("Consultation booking was not found.", 404);
        if (booking.Status is not (ConsultationBookingStatus.AwaitingPayment or ConsultationBookingStatus.Confirmed))
            return ApiResponse<ConsultationBookingDto>.Fail("This booking can no longer be cancelled.", 409);

        var now = timeProvider.GetUtcNow();
        var clientLateCancellation = actorId == booking.ClientId
            && booking.Status == ConsultationBookingStatus.Confirmed
            && booking.StartAtUtc < now.AddHours(24);
        if (clientLateCancellation)
        {
            booking.Status = ConsultationBookingStatus.Disputed;
            booking.DisputeReason = command.Reason.Trim();
            var hold = await dbContext.ConsultationEscrowHolds.SingleAsync(item => item.BookingId == booking.Id, cancellationToken);
            hold.Status = Features.Payments.Enums.EscrowHoldStatus.Frozen;
            hold.FrozenAtUtc = now;
            hold.UpdatedAt = now;
        }
        else
        {
            if (booking.Status == ConsultationBookingStatus.Confirmed)
                await paymentService.RefundAsync(booking.Id, booking.GrossAmount, command.Reason, cancellationToken);
            booking.Status = booking.Status == ConsultationBookingStatus.Confirmed
                ? ConsultationBookingStatus.Refunded : ConsultationBookingStatus.Cancelled;
            booking.CancelledAtUtc = now;
            booking.CancellationReason = command.Reason.Trim();
            var slot = await dbContext.ConsultationAvailabilitySlots.SingleAsync(item => item.Id == booking.SlotId, cancellationToken);
            slot.Status = slot.StartAtUtc > now ? ConsultationSlotStatus.Available : ConsultationSlotStatus.Cancelled;
            slot.ReservedUntilUtc = null;
            slot.UpdatedAt = now;
        }
        booking.UpdatedAt = now;
        await ConsultationOutbox.EnqueueAsync(
            outboxWriter,
            clientLateCancellation
                ? ConsultationEventTypes.ConsultationDisputed
                : ConsultationEventTypes.BookingCancelled,
            booking, actorId, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ApiResponse<ConsultationBookingDto>.Ok((await ConsultationReadModel.FindBookingAsync(
            dbContext, booking.Id, actorId, false, cancellationToken))!);
    }

    public async Task<ApiResponse<ConsultationBookingDto>> Handle(
        SetConsultationDeliveryDetailsCommand command,
        CancellationToken cancellationToken)
    {
        var lawyerId = ConsultationAccess.RequireUserId(currentUserService);
        var booking = await dbContext.ConsultationBookings.SingleOrDefaultAsync(
            item => item.Id == command.BookingId && item.LawyerId == lawyerId, cancellationToken);
        if (booking is null)
            return ApiResponse<ConsultationBookingDto>.Fail("Consultation booking was not found.", 404);
        if (booking.Status != ConsultationBookingStatus.Confirmed
            || booking.Mode != ConsultationMode.VideoMeeting)
            return ApiResponse<ConsultationBookingDto>.Fail(
                "Delivery details can only be set for a confirmed video consultation.", 409);
        if (!Uri.TryCreate(command.MeetingUrl, UriKind.Absolute, out var uri)
            || uri.Scheme != Uri.UriSchemeHttps)
            return ApiResponse<ConsultationBookingDto>.Fail("A valid HTTPS meeting URL is required.");
        booking.MeetingUrl = command.MeetingUrl.Trim();
        booking.UpdatedAt = timeProvider.GetUtcNow();
        await dbContext.SaveChangesAsync(cancellationToken);
        return ApiResponse<ConsultationBookingDto>.Ok((await ConsultationReadModel.FindBookingAsync(
            dbContext, booking.Id, lawyerId, false, cancellationToken))!);
    }

    public async Task<ApiResponse<ConsultationBookingDto>> Handle(
        MarkConsultationPerformedCommand command,
        CancellationToken cancellationToken)
    {
        var lawyerId = ConsultationAccess.RequireUserId(currentUserService);
        var booking = await dbContext.ConsultationBookings.SingleOrDefaultAsync(
            item => item.Id == command.BookingId && item.LawyerId == lawyerId, cancellationToken);
        var now = timeProvider.GetUtcNow();
        if (booking is null)
            return ApiResponse<ConsultationBookingDto>.Fail("Consultation booking was not found.", 404);
        if (booking.Status != ConsultationBookingStatus.Confirmed || now < booking.EndAtUtc)
            return ApiResponse<ConsultationBookingDto>.Fail("The consultation cannot be marked performed yet.", 409);
        if (booking.Mode == ConsultationMode.VideoMeeting && !string.IsNullOrWhiteSpace(command.MeetingUrl))
            booking.MeetingUrl = command.MeetingUrl.Trim();
        booking.Status = ConsultationBookingStatus.AwaitingClientConfirmation;
        booking.PerformedAtUtc = now;
        booking.UpdatedAt = now;
        await ConsultationOutbox.EnqueueAsync(
            outboxWriter, ConsultationEventTypes.ConsultationPerformed,
            booking, lawyerId, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await backgroundJobs.ScheduleAsync<IConsultationJobService>(
            service => service.AutoCompleteAsync(booking.Id, CancellationToken.None),
            now.AddHours(ConsultationPolicy.ClientReviewHours), cancellationToken);
        return ApiResponse<ConsultationBookingDto>.Ok((await ConsultationReadModel.FindBookingAsync(
            dbContext, booking.Id, lawyerId, false, cancellationToken))!);
    }

    public async Task<ApiResponse<ConsultationBookingDto>> Handle(
        ConfirmConsultationCompletionCommand command,
        CancellationToken cancellationToken)
    {
        var clientId = ConsultationAccess.RequireUserId(currentUserService);
        var booking = await dbContext.ConsultationBookings.SingleOrDefaultAsync(
            item => item.Id == command.BookingId && item.ClientId == clientId, cancellationToken);
        if (booking is null)
            return ApiResponse<ConsultationBookingDto>.Fail("Consultation booking was not found.", 404);
        if (booking.Status != ConsultationBookingStatus.AwaitingClientConfirmation)
            return ApiResponse<ConsultationBookingDto>.Fail("The consultation is not awaiting confirmation.", 409);
        booking.Status = ConsultationBookingStatus.Completed;
        booking.CompletedAtUtc = timeProvider.GetUtcNow();
        booking.UpdatedAt = booking.CompletedAtUtc.Value;
        await ConsultationOutbox.EnqueueAsync(
            outboxWriter, ConsultationEventTypes.ConsultationCompleted,
            booking, clientId, cancellationToken);
        await paymentService.StartCompletionHoldAsync(booking.Id, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ApiResponse<ConsultationBookingDto>.Ok((await ConsultationReadModel.FindBookingAsync(
            dbContext, booking.Id, clientId, false, cancellationToken))!);
    }

    public async Task<ApiResponse<ConsultationBookingDto>> Handle(
        OpenConsultationDisputeCommand command,
        CancellationToken cancellationToken)
    {
        var clientId = ConsultationAccess.RequireUserId(currentUserService);
        var booking = await dbContext.ConsultationBookings.SingleOrDefaultAsync(
            item => item.Id == command.BookingId && item.ClientId == clientId, cancellationToken);
        if (booking is null)
            return ApiResponse<ConsultationBookingDto>.Fail("Consultation booking was not found.", 404);
        if (booking.Status is not (ConsultationBookingStatus.AwaitingClientConfirmation or ConsultationBookingStatus.Completed))
            return ApiResponse<ConsultationBookingDto>.Fail("A dispute cannot be opened for this booking.", 409);
        var hold = await dbContext.ConsultationEscrowHolds.SingleAsync(item => item.BookingId == booking.Id, cancellationToken);
        if (hold.Status != Features.Payments.Enums.EscrowHoldStatus.Funded)
            return ApiResponse<ConsultationBookingDto>.Fail("The consultation payment is no longer disputable.", 409);
        var now = timeProvider.GetUtcNow();
        booking.Status = ConsultationBookingStatus.Disputed;
        booking.DisputeReason = command.Reason.Trim();
        booking.UpdatedAt = now;
        hold.Status = Features.Payments.Enums.EscrowHoldStatus.Frozen;
        hold.FrozenAtUtc = now;
        hold.UpdatedAt = now;
        await ConsultationOutbox.EnqueueAsync(
            outboxWriter, ConsultationEventTypes.ConsultationDisputed,
            booking, clientId, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ApiResponse<ConsultationBookingDto>.Ok((await ConsultationReadModel.FindBookingAsync(
            dbContext, booking.Id, clientId, false, cancellationToken))!);
    }

    public async Task<ApiResponse<ConsultationBookingDto>> Handle(
        SettleConsultationDisputeCommand command,
        CancellationToken cancellationToken)
    {
        var adminId = ConsultationAccess.RequireUserId(currentUserService);
        if (!await IsAdministratorAsync(adminId, cancellationToken))
            return ApiResponse<ConsultationBookingDto>.Fail("Administrator access is required.", 403);
        var booking = await dbContext.ConsultationBookings.SingleOrDefaultAsync(
            item => item.Id == command.BookingId, cancellationToken);
        if (booking is null)
            return ApiResponse<ConsultationBookingDto>.Fail("Consultation booking was not found.", 404);
        if (booking.Status != ConsultationBookingStatus.Disputed
            || command.ClientRefundAmount < 0 || command.ClientRefundAmount > booking.GrossAmount)
            return ApiResponse<ConsultationBookingDto>.Fail("The dispute settlement is invalid.", 409);
        await paymentService.SettleDisputeAsync(
            booking.Id, command.ClientRefundAmount, command.Reason, cancellationToken);
        booking.Status = command.ClientRefundAmount == booking.GrossAmount
            ? ConsultationBookingStatus.Refunded : ConsultationBookingStatus.Completed;
        booking.CompletedAtUtc ??= timeProvider.GetUtcNow();
        booking.UpdatedAt = timeProvider.GetUtcNow();
        await ConsultationOutbox.EnqueueAsync(
            outboxWriter, ConsultationEventTypes.DisputeSettled,
            booking, adminId, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ApiResponse<ConsultationBookingDto>.Ok((await ConsultationReadModel.FindBookingAsync(
            dbContext, booking.Id, adminId, true, cancellationToken))!);
    }

    private async Task<ApiResponse<ConsultationPageDto<ConsultationBookingDto>>> ListAsync(
        ConsultationBookingFilter filter,
        bool clientScope,
        CancellationToken cancellationToken)
    {
        var validation = await filterValidator.ValidateAsync(filter, cancellationToken);
        if (!validation.IsValid)
            return ApiResponse<ConsultationPageDto<ConsultationBookingDto>>.Fail(
                validation.Errors.Select(item => item.ErrorMessage).ToList());
        var actorId = ConsultationAccess.RequireUserId(currentUserService);
        var query = dbContext.ConsultationBookings.AsNoTracking()
            .Where(item => clientScope ? item.ClientId == actorId : item.LawyerId == actorId);
        if (filter.Statuses is { Length: > 0 })
            query = query.Where(item => filter.Statuses.Contains(item.Status));
        if (filter.FromUtc.HasValue)
            query = query.Where(item => item.StartAtUtc >= filter.FromUtc.Value);
        if (filter.ToUtc.HasValue)
            query = query.Where(item => item.StartAtUtc <= filter.ToUtc.Value);
        var total = await query.CountAsync(cancellationToken);
        var bookings = await query.OrderByDescending(item => item.StartAtUtc)
            .Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize)
            .ToListAsync(cancellationToken);
        var items = await ConsultationReadModel.MapManyAsync(
            dbContext, bookings, actorId, false, cancellationToken);
        return ApiResponse<ConsultationPageDto<ConsultationBookingDto>>.Ok(new(
            items, filter.Page, filter.PageSize, total,
            total == 0 ? 0 : (int)Math.Ceiling(total / (double)filter.PageSize)));
    }

    private async Task<bool> IsAdministratorAsync(Guid userId, CancellationToken cancellationToken) =>
        await ConsultationAccess.HasRoleAsync(dbContext, userId, "SuperAdministrator", cancellationToken)
        || await ConsultationAccess.HasRoleAsync(dbContext, userId, "FinanceAdministrator", cancellationToken);
}

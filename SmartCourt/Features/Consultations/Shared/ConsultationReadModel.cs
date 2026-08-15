using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Consultations.Domain.Entities;
using SmartCourt.Features.Consultations.Domain.Enums;
using SmartCourt.Features.Consultations.DTOs;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Consultations.Shared;

internal static class ConsultationReadModel
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    internal static async Task<ConsultationBookingDto?> FindBookingAsync(
        ApplicationDbContext dbContext,
        Guid bookingId,
        Guid actorId,
        bool isAdministrator,
        CancellationToken cancellationToken)
    {
        var booking = await dbContext.ConsultationBookings.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == bookingId
                && (isAdministrator || item.ClientId == actorId || item.LawyerId == actorId),
                cancellationToken);
        return booking is null
            ? null
            : await MapAsync(dbContext, booking, actorId, isAdministrator, cancellationToken);
    }

    internal static async Task<IReadOnlyList<ConsultationBookingDto>> MapManyAsync(
        ApplicationDbContext dbContext,
        IReadOnlyList<ConsultationBooking> bookings,
        Guid actorId,
        bool isAdministrator,
        CancellationToken cancellationToken)
    {
        var result = new List<ConsultationBookingDto>(bookings.Count);
        foreach (var booking in bookings)
            result.Add(await MapAsync(dbContext, booking, actorId, isAdministrator, cancellationToken));
        return result;
    }

    private static async Task<ConsultationBookingDto> MapAsync(
        ApplicationDbContext dbContext,
        ConsultationBooking booking,
        Guid actorId,
        bool isAdministrator,
        CancellationToken cancellationToken)
    {
        var names = await dbContext.Users.AsNoTracking()
            .Where(user => user.Id == booking.ClientId || user.Id == booking.LawyerId)
            .Select(user => new { user.Id, user.FullName })
            .ToDictionaryAsync(item => item.Id, item => item.FullName, cancellationToken);
        var payment = await dbContext.ConsultationPaymentTransactions.AsNoTracking()
            .Where(item => item.BookingId == booking.Id)
            .OrderByDescending(item => item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        var paid = await dbContext.ConsultationEscrowHolds.AsNoTracking()
            .AnyAsync(item => item.BookingId == booking.Id, cancellationToken);
        var exposeDelivery = paid && (actorId == booking.ClientId || actorId == booking.LawyerId || isAdministrator);

        return new ConsultationBookingDto(
            booking.Id, booking.OfferingId, booking.SlotId, booking.LawyerId,
            names.GetValueOrDefault(booking.LawyerId, "Lawyer"),
            booking.ClientId, names.GetValueOrDefault(booking.ClientId, "Client"),
            booking.Mode, booking.Specialization, booking.OfferingTitle,
            JsonSerializer.Deserialize<List<string>>(booking.InclusionsJson, SerializerOptions) ?? [],
            booking.DurationMinutes, booking.GrossAmount, booking.PlatformFeeAmount,
            booking.LawyerNetAmount, booking.Currency, booking.Subject, booking.MatterSummary,
            booking.StartAtUtc, booking.EndAtUtc, booking.Status, booking.PaymentExpiresAtUtc,
            exposeDelivery ? booking.OfficeLocation : null,
            exposeDelivery ? booking.MeetingUrl : null,
            booking.PerformedAtUtc, booking.CompletedAtUtc, booking.CancellationReason,
            booking.DisputeReason,
            payment is null ? null : new ConsultationPaymentDto(
                payment.Id, booking.Id, payment.OperationType, payment.Status,
                payment.Amount, payment.Currency, null, null, null,
                payment.FailureReason, payment.CreatedAt),
            PermittedActions(booking, actorId, isAdministrator));
    }

    private static IReadOnlyList<string> PermittedActions(
        ConsultationBooking booking,
        Guid actorId,
        bool isAdministrator)
    {
        var actions = new List<string>();
        if (actorId == booking.ClientId)
        {
            if (booking.Status == ConsultationBookingStatus.AwaitingPayment)
                actions.AddRange(["Pay", "Cancel"]);
            if (booking.Status == ConsultationBookingStatus.Confirmed)
                actions.Add("Cancel");
            if (booking.Status == ConsultationBookingStatus.AwaitingClientConfirmation)
                actions.AddRange(["ConfirmCompletion", "OpenDispute"]);
            if (booking.Status == ConsultationBookingStatus.Completed)
                actions.Add("OpenDispute");
        }
        if (actorId == booking.LawyerId)
        {
            if (booking.Status == ConsultationBookingStatus.Confirmed)
                actions.AddRange(["MarkPerformed", "Cancel"]);
        }
        if (isAdministrator && booking.Status == ConsultationBookingStatus.Disputed)
            actions.Add("SettleDispute");
        return actions;
    }
}

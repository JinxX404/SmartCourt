using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Consultations.Domain.Enums;
using SmartCourt.Features.Consultations.Payments;
using SmartCourt.Features.Consultations.Events;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Consultations.Bookings;

public sealed class ConsultationJobService(
    ApplicationDbContext dbContext,
    IConsultationPaymentService paymentService,
    IBackgroundJobProvider backgroundJobs,
    IOutboxWriter outboxWriter,
    TimeProvider timeProvider,
    ILogger<ConsultationJobService> logger)
    : IConsultationJobService
{
    public async Task ExpireUnpaidBookingAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        var booking = await dbContext.ConsultationBookings.SingleOrDefaultAsync(
            item => item.Id == bookingId, cancellationToken);
        if (booking is null || booking.Status != ConsultationBookingStatus.AwaitingPayment)
            return;
        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (booking.PaymentExpiresAtUtc > now)
            return;

        var hasProcessingPayment = await dbContext.ConsultationPaymentTransactions.AnyAsync(
            item => item.BookingId == bookingId
                && item.Status == Features.Payments.Enums.PaymentTransactionStatus.Processing,
            cancellationToken);
        if (hasProcessingPayment)
        {
            await backgroundJobs.ScheduleAsync<IConsultationJobService>(
                service => service.ExpireUnpaidBookingAsync(bookingId, CancellationToken.None),
                new DateTimeOffset(now.AddMinutes(10), TimeSpan.Zero), cancellationToken);
            return;
        }

        var slot = await dbContext.ConsultationAvailabilitySlots.SingleAsync(
            item => item.Id == booking.SlotId, cancellationToken);
        booking.Status = ConsultationBookingStatus.Expired;
        booking.UpdatedAt = now;
        slot.Status = slot.StartAtUtc > now
            ? ConsultationSlotStatus.Available
            : ConsultationSlotStatus.Cancelled;
        slot.ReservedUntilUtc = null;
        slot.UpdatedAt = now;
        await ConsultationOutbox.EnqueueAsync(
            outboxWriter, ConsultationEventTypes.BookingExpired,
            booking, null, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Expired unpaid consultation booking {BookingId}.", bookingId);
    }

    public async Task AutoCompleteAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        var booking = await dbContext.ConsultationBookings.SingleOrDefaultAsync(
            item => item.Id == bookingId, cancellationToken);
        if (booking is null || booking.Status != ConsultationBookingStatus.AwaitingClientConfirmation
            || !booking.PerformedAtUtc.HasValue)
            return;
        var dueAt = booking.PerformedAtUtc.Value.AddHours(Shared.ConsultationPolicy.ClientReviewHours);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        if (dueAt > now)
            return;
        booking.Status = ConsultationBookingStatus.Completed;
        booking.CompletedAtUtc = now;
        booking.UpdatedAt = now;
        await ConsultationOutbox.EnqueueAsync(
            outboxWriter, ConsultationEventTypes.ConsultationCompleted,
            booking, null, cancellationToken);
        await paymentService.StartCompletionHoldAsync(bookingId, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task ReleaseAsync(Guid bookingId, CancellationToken cancellationToken) =>
        paymentService.ReleaseAsync(bookingId, cancellationToken);
}

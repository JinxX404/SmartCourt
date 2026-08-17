using System.Text.Json;
using SmartCourt.Features.Consultations.Events;
using SmartCourt.Features.Notifications.Enums;
using SmartCourt.Features.Notifications.Events;
using SmartCourt.Infrastructure.Persistence.Entities;
using Xunit;

namespace SmartCourt.Tests.Features.Notifications;

public class ConsultationNotificationEventMapperTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private static OutboxMessage CreateMessage(string eventType, int eventVersion, string payload, Guid aggregateId)
    {
        return new OutboxMessage(
            Guid.NewGuid(),
            eventType,
            eventVersion,
            payload,
            "ConsultationBooking",
            aggregateId,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow);
    }

    [Fact]
    public void EventTypes_ContainsAllConsultationNotificationEvents()
    {
        var mapper = new ConsultationNotificationEventMapper();

        Assert.Contains(ConsultationEventTypes.BookingCreated, mapper.EventTypes);
        Assert.Contains(ConsultationEventTypes.PaymentFunded, mapper.EventTypes);
        Assert.Contains(ConsultationEventTypes.BookingCancelled, mapper.EventTypes);
        Assert.Contains(ConsultationEventTypes.BookingExpired, mapper.EventTypes);
        Assert.Contains(ConsultationEventTypes.ConsultationPerformed, mapper.EventTypes);
        Assert.Contains(ConsultationEventTypes.ConsultationCompleted, mapper.EventTypes);
        Assert.Contains(ConsultationEventTypes.ConsultationDisputed, mapper.EventTypes);
        Assert.Contains(ConsultationEventTypes.DisputeSettled, mapper.EventTypes);
        Assert.Contains(ConsultationEventTypes.PaymentReleased, mapper.EventTypes);
    }

    [Fact]
    public async Task MapAsync_WithUnsupportedVersion_ThrowsInvalidOperationException()
    {
        var mapper = new ConsultationNotificationEventMapper();
        var message = CreateMessage(ConsultationEventTypes.BookingCreated, 2, "{}", Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => mapper.MapAsync(message, CancellationToken.None));
    }

    [Fact]
    public async Task MapAsync_WithUnsupportedEventType_ThrowsInvalidOperationException()
    {
        var mapper = new ConsultationNotificationEventMapper();
        var bookingId = Guid.NewGuid();
        var payload = new ConsultationEventPayload(
            bookingId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "Consultation",
            DateTimeOffset.UtcNow);

        var message = CreateMessage(
            "unknown.consultation.event",
            1,
            JsonSerializer.Serialize(payload, SerializerOptions),
            bookingId);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => mapper.MapAsync(message, CancellationToken.None));
    }

    [Fact]
    public async Task MapAsync_WithAggregateIdMismatch_ThrowsInvalidOperationException()
    {
        var mapper = new ConsultationNotificationEventMapper();
        var bookingId = Guid.NewGuid();
        var payload = new ConsultationEventPayload(
            bookingId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "Consultation",
            DateTimeOffset.UtcNow);

        var message = CreateMessage(
            ConsultationEventTypes.BookingCreated,
            1,
            JsonSerializer.Serialize(payload, SerializerOptions),
            Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => mapper.MapAsync(message, CancellationToken.None));
    }

    [Fact]
    public async Task MapAsync_BookingCreated_ReturnsDraftInArabicForLawyer()
    {
        var mapper = new ConsultationNotificationEventMapper();
        var bookingId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var lawyerId = Guid.NewGuid();
        var startAt = DateTimeOffset.UtcNow.AddDays(1);

        var payload = new ConsultationEventPayload(bookingId, clientId, lawyerId, clientId, "Consultation 1", startAt);
        var message = CreateMessage(
            ConsultationEventTypes.BookingCreated,
            1,
            JsonSerializer.Serialize(payload, SerializerOptions),
            bookingId);

        var drafts = await mapper.MapAsync(message, CancellationToken.None);

        var draft = Assert.Single(drafts);
        Assert.Equal(lawyerId, draft.RecipientUserId);
        Assert.Equal("consultation.booking.created", draft.Type);
        Assert.Equal(NotificationSeverity.Information, draft.Severity);
        Assert.Equal("حجز استشارة جديد", draft.Title);
        Assert.Equal("قام عميل بحجز موعد استشارة وهو بصدد إتمام الدفع.", draft.Body);
        Assert.Equal($"/consultations/bookings/{bookingId}", draft.ActionUrl);
        Assert.Equal(bookingId.ToString(), draft.Data!["bookingId"]);
    }

    [Fact]
    public async Task MapAsync_PaymentFunded_ReturnsDraftsInArabicForBothParties()
    {
        var mapper = new ConsultationNotificationEventMapper();
        var bookingId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var lawyerId = Guid.NewGuid();

        var payload = new ConsultationEventPayload(bookingId, clientId, lawyerId, clientId, "Consultation", DateTimeOffset.UtcNow);
        var message = CreateMessage(
            ConsultationEventTypes.PaymentFunded,
            1,
            JsonSerializer.Serialize(payload, SerializerOptions),
            bookingId);

        var drafts = await mapper.MapAsync(message, CancellationToken.None);

        Assert.Equal(2, drafts.Count);
        Assert.Contains(drafts, d => d.RecipientUserId == clientId && d.Title == "تم تأكيد الاستشارة" && d.Body == "تم سداد رسوم الاستشارة وتأكيد موعد الجلسة بنجاح.");
        Assert.Contains(drafts, d => d.RecipientUserId == lawyerId && d.Title == "تم تأكيد الاستشارة" && d.Body == "تم سداد رسوم الاستشارة وتأكيد موعد الجلسة بنجاح.");
    }

    [Fact]
    public async Task MapAsync_BookingCancelled_ByClient_NotifiesLawyerInArabic()
    {
        var mapper = new ConsultationNotificationEventMapper();
        var bookingId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var lawyerId = Guid.NewGuid();

        var payload = new ConsultationEventPayload(bookingId, clientId, lawyerId, clientId, "Consultation", DateTimeOffset.UtcNow);
        var message = CreateMessage(
            ConsultationEventTypes.BookingCancelled,
            1,
            JsonSerializer.Serialize(payload, SerializerOptions),
            bookingId);

        var drafts = await mapper.MapAsync(message, CancellationToken.None);

        var draft = Assert.Single(drafts);
        Assert.Equal(lawyerId, draft.RecipientUserId);
        Assert.Equal("consultation.booking.cancelled", draft.Type);
        Assert.Equal(NotificationSeverity.Warning, draft.Severity);
        Assert.Equal("تم إلغاء الاستشارة", draft.Title);
        Assert.Equal("قام الطرف الآخر بإلغاء موعد الاستشارة.", draft.Body);
    }

    [Fact]
    public async Task MapAsync_BookingCancelled_ByLawyer_NotifiesClientInArabic()
    {
        var mapper = new ConsultationNotificationEventMapper();
        var bookingId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var lawyerId = Guid.NewGuid();

        var payload = new ConsultationEventPayload(bookingId, clientId, lawyerId, lawyerId, "Consultation", DateTimeOffset.UtcNow);
        var message = CreateMessage(
            ConsultationEventTypes.BookingCancelled,
            1,
            JsonSerializer.Serialize(payload, SerializerOptions),
            bookingId);

        var drafts = await mapper.MapAsync(message, CancellationToken.None);

        var draft = Assert.Single(drafts);
        Assert.Equal(clientId, draft.RecipientUserId);
        Assert.Equal("تم إلغاء الاستشارة", draft.Title);
        Assert.Equal("قام الطرف الآخر بإلغاء موعد الاستشارة.", draft.Body);
    }

    [Fact]
    public async Task MapAsync_BookingExpired_NotifiesClientInArabic()
    {
        var mapper = new ConsultationNotificationEventMapper();
        var bookingId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var lawyerId = Guid.NewGuid();

        var payload = new ConsultationEventPayload(bookingId, clientId, lawyerId, null, "Consultation", DateTimeOffset.UtcNow);
        var message = CreateMessage(
            ConsultationEventTypes.BookingExpired,
            1,
            JsonSerializer.Serialize(payload, SerializerOptions),
            bookingId);

        var drafts = await mapper.MapAsync(message, CancellationToken.None);

        var draft = Assert.Single(drafts);
        Assert.Equal(clientId, draft.RecipientUserId);
        Assert.Equal("consultation.booking.expired", draft.Type);
        Assert.Equal(NotificationSeverity.Warning, draft.Severity);
        Assert.Equal("انتهت صلاحية حجز الاستشارة", draft.Title);
        Assert.Equal("لم يتم إتمام عملية الدفع قبل انتهاء مهلة حجز الموعد.", draft.Body);
    }

    [Fact]
    public async Task MapAsync_ConsultationPerformed_NotifiesClientInArabic()
    {
        var mapper = new ConsultationNotificationEventMapper();
        var bookingId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var lawyerId = Guid.NewGuid();

        var payload = new ConsultationEventPayload(bookingId, clientId, lawyerId, lawyerId, "Consultation", DateTimeOffset.UtcNow);
        var message = CreateMessage(
            ConsultationEventTypes.ConsultationPerformed,
            1,
            JsonSerializer.Serialize(payload, SerializerOptions),
            bookingId);

        var drafts = await mapper.MapAsync(message, CancellationToken.None);

        var draft = Assert.Single(drafts);
        Assert.Equal(clientId, draft.RecipientUserId);
        Assert.Equal("consultation.performed", draft.Type);
        Assert.Equal(NotificationSeverity.Information, draft.Severity);
        Assert.Equal("تأكيد إتمام الاستشارة", draft.Title);
        Assert.Equal("أشار المحامي إلى إتمام الاستشارة، يرجى تأكيد ذلك أو تقديم اعتراض خلال 24 ساعة.", draft.Body);
    }

    [Fact]
    public async Task MapAsync_ConsultationCompleted_NotifiesLawyerInArabic()
    {
        var mapper = new ConsultationNotificationEventMapper();
        var bookingId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var lawyerId = Guid.NewGuid();

        var payload = new ConsultationEventPayload(bookingId, clientId, lawyerId, clientId, "Consultation", DateTimeOffset.UtcNow);
        var message = CreateMessage(
            ConsultationEventTypes.ConsultationCompleted,
            1,
            JsonSerializer.Serialize(payload, SerializerOptions),
            bookingId);

        var drafts = await mapper.MapAsync(message, CancellationToken.None);

        var draft = Assert.Single(drafts);
        Assert.Equal(lawyerId, draft.RecipientUserId);
        Assert.Equal("consultation.completed", draft.Type);
        Assert.Equal(NotificationSeverity.Success, draft.Severity);
        Assert.Equal("اكتملت الاستشارة", draft.Title);
        Assert.Equal("تم تأكيد إتمام الاستشارة وبدأت فترة حجز المبلغ قبل تحريره إلى حسابك.", draft.Body);
    }

    [Fact]
    public async Task MapAsync_ConsultationDisputed_NotifiesLawyerInArabic()
    {
        var mapper = new ConsultationNotificationEventMapper();
        var bookingId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var lawyerId = Guid.NewGuid();

        var payload = new ConsultationEventPayload(bookingId, clientId, lawyerId, clientId, "Consultation", DateTimeOffset.UtcNow);
        var message = CreateMessage(
            ConsultationEventTypes.ConsultationDisputed,
            1,
            JsonSerializer.Serialize(payload, SerializerOptions),
            bookingId);

        var drafts = await mapper.MapAsync(message, CancellationToken.None);

        var draft = Assert.Single(drafts);
        Assert.Equal(lawyerId, draft.RecipientUserId);
        Assert.Equal("consultation.disputed", draft.Type);
        Assert.Equal(NotificationSeverity.Warning, draft.Severity);
        Assert.Equal("تم تجميد مستحقات الاستشارة", draft.Title);
        Assert.Equal("فتح العميل نزاعًا بشأن الاستشارة، وستظل الأموال معلقة حتى مراجعة النزاع والفصل فيه.", draft.Body);
    }

    [Fact]
    public async Task MapAsync_DisputeSettled_NotifiesBothPartiesInArabic()
    {
        var mapper = new ConsultationNotificationEventMapper();
        var bookingId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var lawyerId = Guid.NewGuid();

        var payload = new ConsultationEventPayload(bookingId, clientId, lawyerId, null, "Consultation", DateTimeOffset.UtcNow);
        var message = CreateMessage(
            ConsultationEventTypes.DisputeSettled,
            1,
            JsonSerializer.Serialize(payload, SerializerOptions),
            bookingId);

        var drafts = await mapper.MapAsync(message, CancellationToken.None);

        Assert.Equal(2, drafts.Count);
        Assert.Contains(drafts, d => d.RecipientUserId == clientId && d.Title == "تمت تسوية نزاع الاستشارة" && d.Body == "أنهت إدارة المنصة تسوية النزاع وتوزيع مستحقات الاستشارة.");
        Assert.Contains(drafts, d => d.RecipientUserId == lawyerId && d.Title == "تمت تسوية نزاع الاستشارة" && d.Body == "أنهت إدارة المنصة تسوية النزاع وتوزيع مستحقات الاستشارة.");
    }

    [Fact]
    public async Task MapAsync_PaymentReleased_NotifiesLawyerInArabic()
    {
        var mapper = new ConsultationNotificationEventMapper();
        var bookingId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var lawyerId = Guid.NewGuid();

        var payload = new ConsultationEventPayload(bookingId, clientId, lawyerId, null, "Consultation", DateTimeOffset.UtcNow);
        var message = CreateMessage(
            ConsultationEventTypes.PaymentReleased,
            1,
            JsonSerializer.Serialize(payload, SerializerOptions),
            bookingId);

        var drafts = await mapper.MapAsync(message, CancellationToken.None);

        var draft = Assert.Single(drafts);
        Assert.Equal(lawyerId, draft.RecipientUserId);
        Assert.Equal("consultation.payment.released", draft.Type);
        Assert.Equal(NotificationSeverity.Success, draft.Severity);
        Assert.Equal("تم تحرير مستحقات الاستشارة", draft.Title);
        Assert.Equal("أصبح صافي مستحقات الاستشارة متاحًا الآن في محفظتك.", draft.Body);
    }
}

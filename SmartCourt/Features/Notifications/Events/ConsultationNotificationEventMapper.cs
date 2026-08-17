using System.Text.Json;
using SmartCourt.Features.Consultations.Events;
using SmartCourt.Features.Notifications.Enums;
using SmartCourt.Infrastructure.Persistence.Entities;

namespace SmartCourt.Features.Notifications.Events;

internal sealed class ConsultationNotificationEventMapper : INotificationEventMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public IReadOnlyCollection<string> EventTypes =>
    [
        ConsultationEventTypes.BookingCreated,
        ConsultationEventTypes.PaymentFunded,
        ConsultationEventTypes.BookingCancelled,
        ConsultationEventTypes.BookingExpired,
        ConsultationEventTypes.ConsultationPerformed,
        ConsultationEventTypes.ConsultationCompleted,
        ConsultationEventTypes.ConsultationDisputed,
        ConsultationEventTypes.DisputeSettled,
        ConsultationEventTypes.PaymentReleased
    ];

    public Task<IReadOnlyCollection<NotificationDraft>> MapAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (message.EventVersion != 1)
            throw new InvalidOperationException("Unsupported consultation notification event version.");
        var payload = JsonSerializer.Deserialize<ConsultationEventPayload>(message.Payload, JsonOptions)
            ?? throw new InvalidOperationException("Consultation notification payload is invalid.");
        if (payload.BookingId != message.AggregateId)
            throw new InvalidOperationException("Consultation notification aggregate does not match its payload.");

        var data = new Dictionary<string, string>
        {
            ["bookingId"] = payload.BookingId.ToString(),
            ["clientUserId"] = payload.ClientUserId.ToString(),
            ["lawyerUserId"] = payload.LawyerUserId.ToString(),
            ["offeringTitle"] = payload.OfferingTitle,
            ["startAtUtc"] = payload.StartAtUtc.ToString("O")
        };
        var url = NotificationActionUrls.Consultation(payload.BookingId);
        var drafts = message.EventType switch
        {
            ConsultationEventTypes.BookingCreated => One(payload.LawyerUserId,
                "consultation.booking.created", NotificationSeverity.Information,
                "حجز استشارة جديد", "قام عميل بحجز موعد استشارة وهو بصدد إتمام الدفع.", url, data),
            ConsultationEventTypes.PaymentFunded => Two(payload.ClientUserId, payload.LawyerUserId,
                "consultation.payment.funded", NotificationSeverity.Success,
                "تم تأكيد الاستشارة", "تم سداد رسوم الاستشارة وتأكيد موعد الجلسة بنجاح.", url, data),
            ConsultationEventTypes.BookingCancelled => One(OtherParticipant(payload),
                "consultation.booking.cancelled", NotificationSeverity.Warning,
                "تم إلغاء الاستشارة", "قام الطرف الآخر بإلغاء موعد الاستشارة.", url, data),
            ConsultationEventTypes.BookingExpired => One(payload.ClientUserId,
                "consultation.booking.expired", NotificationSeverity.Warning,
                "انتهت صلاحية حجز الاستشارة", "لم يتم إتمام عملية الدفع قبل انتهاء مهلة حجز الموعد.", url, data),
            ConsultationEventTypes.ConsultationPerformed => One(payload.ClientUserId,
                "consultation.performed", NotificationSeverity.Information,
                "تأكيد إتمام الاستشارة", "أشار المحامي إلى إتمام الاستشارة، يرجى تأكيد ذلك أو تقديم اعتراض خلال 24 ساعة.", url, data),
            ConsultationEventTypes.ConsultationCompleted => One(payload.LawyerUserId,
                "consultation.completed", NotificationSeverity.Success,
                "اكتملت الاستشارة", "تم تأكيد إتمام الاستشارة وبدأت فترة حجز المبلغ قبل تحريره إلى حسابك.", url, data),
            ConsultationEventTypes.ConsultationDisputed => One(payload.LawyerUserId,
                "consultation.disputed", NotificationSeverity.Warning,
                "تم تجميد مستحقات الاستشارة", "فتح العميل نزاعًا بشأن الاستشارة، وستظل الأموال معلقة حتى مراجعة النزاع والفصل فيه.", url, data),
            ConsultationEventTypes.DisputeSettled => Two(payload.ClientUserId, payload.LawyerUserId,
                "consultation.dispute.settled", NotificationSeverity.Information,
                "تمت تسوية نزاع الاستشارة", "أنهت إدارة المنصة تسوية النزاع وتوزيع مستحقات الاستشارة.", url, data),
            ConsultationEventTypes.PaymentReleased => One(payload.LawyerUserId,
                "consultation.payment.released", NotificationSeverity.Success,
                "تم تحرير مستحقات الاستشارة", "أصبح صافي مستحقات الاستشارة متاحًا الآن في محفظتك.", url, data),
            _ => throw new InvalidOperationException($"Unsupported consultation event '{message.EventType}'.")
        };
        return Task.FromResult(drafts);
    }

    private static IReadOnlyCollection<NotificationDraft> One(
        Guid recipient, string type, NotificationSeverity severity,
        string title, string body, string url, IReadOnlyDictionary<string, string> data) =>
        [new(recipient, type, severity, title, body, url, data)];

    private static IReadOnlyCollection<NotificationDraft> Two(
        Guid first, Guid second, string type, NotificationSeverity severity,
        string title, string body, string url, IReadOnlyDictionary<string, string> data) =>
        [new(first, type, severity, title, body, url, data),
         new(second, type, severity, title, body, url, data)];

    private static Guid OtherParticipant(ConsultationEventPayload payload) =>
        payload.ActorUserId == payload.ClientUserId ? payload.LawyerUserId : payload.ClientUserId;
}

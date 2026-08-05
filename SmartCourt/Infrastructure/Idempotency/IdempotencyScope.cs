using SmartCourt.Common.Exceptions;

namespace SmartCourt.Infrastructure.Idempotency;

public sealed record IdempotencyScope
{
    public const string HoldSettlementResourceType =
        "EscrowHoldSettlement";

    public IdempotencyScope(
        Guid userId,
        string operation,
        string resourceType,
        Guid resourceId)
    {
        if (userId == Guid.Empty)
        {
            throw new BusinessException(
                "يجب تسجيل الدخول لاستخدام حماية تكرار الطلبات.");
        }

        if (string.IsNullOrWhiteSpace(operation))
        {
            throw new BusinessException(
                "اسم العملية مطلوب لحماية الطلب من التكرار.");
        }

        if (string.IsNullOrWhiteSpace(resourceType))
        {
            throw new BusinessException(
                "نوع السجل المرتبط مطلوب لحماية الطلب من التكرار.");
        }

        if (resourceId == Guid.Empty)
        {
            throw new BusinessException(
                "معرّف السجل المرتبط مطلوب لحماية الطلب من التكرار.");
        }

        UserId = userId;
        Operation = operation.Trim();
        ResourceType = resourceType.Trim();
        ResourceId = resourceId;
    }

    public Guid UserId { get; }
    public string Operation { get; }
    public string ResourceType { get; }
    public Guid ResourceId { get; }

    public static IdempotencyScope ForHoldSettlement(
        Guid userId,
        string operation,
        Guid escrowHoldId)
    {
        return new IdempotencyScope(
            userId,
            operation,
            HoldSettlementResourceType,
            escrowHoldId);
    }
}

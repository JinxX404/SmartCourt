using SmartCourt.Common.Domain;

namespace SmartCourt.Features.ChatAgent.Entities;

public sealed class QuotaLedger
{
    private QuotaLedger()
    {
    }

    internal QuotaLedger(Guid clientId)
    {
        ClientId = EntityGuard.NotEmpty(clientId, nameof(clientId));
        AdditionalTokenBalance = 0;
    }

    public Guid ClientId { get; internal set; }
    public int AdditionalTokenBalance { get; internal set; }
    public byte[] RowVersion { get; internal set; } = [];

    public static QuotaLedger Create(Guid clientId)
    {
        return new QuotaLedger(clientId);
    }

    public void AddBalance(int amount)
    {
        EntityGuard.Positive(amount, nameof(amount));
        AdditionalTokenBalance += amount;
    }

    public void DeductBalance(int amount)
    {
        EntityGuard.Positive(amount, nameof(amount));
        if (AdditionalTokenBalance < amount)
        {
            throw new Common.Exceptions.BusinessException("رصيد الطلبات الإضافية غير كافٍ.");
        }
        AdditionalTokenBalance -= amount;
    }
}

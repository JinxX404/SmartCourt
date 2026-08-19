using SmartCourt.Common.Domain;

namespace SmartCourt.Features.ChatAgent.Entities;

public static class QuotaTransactionReason
{
    public const string BundlePurchase = "BundlePurchase";
    public const string AdminAdjustment = "AdminAdjustment";
    public const string LlmConsumption = "LlmConsumption";
}

public sealed class QuotaTransaction
{
    private QuotaTransaction()
    {
    }

    internal QuotaTransaction(
        Guid id,
        Guid clientId,
        int amount,
        string reason,
        string? referenceId,
        DateTimeOffset createdAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        ClientId = EntityGuard.NotEmpty(clientId, nameof(clientId));
        Amount = amount; // Can be negative for deductions
        Reason = EntityGuard.Required(reason, nameof(reason));
        ReferenceId = referenceId;
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
    }

    public Guid Id { get; internal set; }
    public Guid ClientId { get; internal set; }
    public int Amount { get; internal set; }
    public string Reason { get; internal set; }
    public string? ReferenceId { get; internal set; }
    public DateTimeOffset CreatedAt { get; internal set; }

    public static QuotaTransaction Create(
        Guid id,
        Guid clientId,
        int amount,
        string reason,
        string? referenceId,
        DateTimeOffset createdAt)
    {
        return new QuotaTransaction(id, clientId, amount, reason, referenceId, createdAt);
    }
}

using SmartCourt.Common.Domain;

namespace SmartCourt.Features.Payments.Entities;

public sealed class ClientPaymentCustomer
{
    private ClientPaymentCustomer() { }

    internal ClientPaymentCustomer(
        Guid id,
        Guid clientUserId,
        string providerCode,
        string providerCustomerId,
        bool isLive,
        DateTimeOffset createdAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        ClientUserId = EntityGuard.NotEmpty(clientUserId, nameof(clientUserId));
        ProviderCode = EntityGuard.Required(providerCode, nameof(providerCode));
        ProviderCustomerId = EntityGuard.Required(
            providerCustomerId,
            nameof(providerCustomerId));
        IsLive = isLive;
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; internal set; }
    public Guid ClientUserId { get; internal set; }
    public string ProviderCode { get; internal set; } = string.Empty;
    public string ProviderCustomerId { get; internal set; } = string.Empty;
    public bool IsLive { get; internal set; }
    public DateTimeOffset CreatedAt { get; internal set; }
    public DateTimeOffset UpdatedAt { get; internal set; }
    public byte[] RowVersion { get; internal set; } = [];
}

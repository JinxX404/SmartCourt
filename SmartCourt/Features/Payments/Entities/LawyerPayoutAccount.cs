using SmartCourt.Common.Domain;
using SmartCourt.Features.Payments.Enums;

namespace SmartCourt.Features.Payments.Entities;

public sealed class LawyerPayoutAccount
{
    private LawyerPayoutAccount()
    {
    }

    internal LawyerPayoutAccount(
        Guid id,
        Guid lawyerUserId,
        string providerCode,
        string providerAccountId,
        bool isLive,
        DateTime createdAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        LawyerUserId = EntityGuard.NotEmpty(lawyerUserId, nameof(lawyerUserId));
        ProviderCode = EntityGuard.Required(providerCode, nameof(providerCode));
        ProviderAccountId = EntityGuard.Required(
            providerAccountId,
            nameof(providerAccountId));
        IsLive = isLive;
        Status = LawyerPayoutAccountStatus.Pending;
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; internal set; }
    public Guid LawyerUserId { get; internal set; }
    public string ProviderCode { get; internal set; } = string.Empty;
    public string ProviderAccountId { get; internal set; } = string.Empty;
    public LawyerPayoutAccountStatus Status { get; internal set; }
    public bool DetailsSubmitted { get; internal set; }
    public bool TransfersEnabled { get; internal set; }
    public bool PayoutsEnabled { get; internal set; }
    public bool IsLive { get; internal set; }
    public string Country { get; internal set; } = string.Empty;
    public string DefaultCurrency { get; internal set; } = string.Empty;
    public long AvailableProviderAmountMinor { get; internal set; }
    public string? MaskedDestination { get; internal set; }
    public string? LastProviderStatus { get; internal set; }
    public string? LastProviderErrorCode { get; internal set; }
    public DateTime? LastSynchronizedAt { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime UpdatedAt { get; internal set; }
    public byte[] RowVersion { get; internal set; } = [];
}

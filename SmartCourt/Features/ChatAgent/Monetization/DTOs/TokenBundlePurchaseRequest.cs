using System.ComponentModel.DataAnnotations;

namespace SmartCourt.Features.ChatAgent.Monetization.DTOs;

public sealed record TokenBundlePurchaseRequest
{
    public string BundleId { get; init; } = string.Empty;

    public string ConfirmationTokenReference { get; init; } = string.Empty;
}

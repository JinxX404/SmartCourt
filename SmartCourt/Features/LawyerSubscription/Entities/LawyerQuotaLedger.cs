using System;

namespace SmartCourt.Features.LawyerSubscription.Entities;

public sealed class LawyerQuotaLedger
{
    public Guid LawyerId { get; set; }
    public int PurchasedTokenBalance { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

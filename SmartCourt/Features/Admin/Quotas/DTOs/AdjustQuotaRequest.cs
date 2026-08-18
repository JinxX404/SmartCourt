namespace SmartCourt.Features.Admin.Quotas.DTOs;

public class AdjustQuotaRequest
{
    public decimal CreditAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

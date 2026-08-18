namespace SmartCourt.Common.Configuration;

public sealed class TokenBundleOptions
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal CreditAmount { get; set; }
    public decimal PriceEgp { get; set; }
}

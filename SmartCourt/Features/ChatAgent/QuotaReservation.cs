namespace SmartCourt.Features.ChatAgent;

public sealed class QuotaReservation
{
    public int TotalReservedTokens { get; init; }
    public int FreeReservedTokens { get; init; }
    public int PaidReservedTokens { get; init; }
}

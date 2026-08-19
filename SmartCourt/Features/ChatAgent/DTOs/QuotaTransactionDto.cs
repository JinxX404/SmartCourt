using System;

namespace SmartCourt.Features.ChatAgent.DTOs;

public sealed record QuotaTransactionDto(
    Guid Id,
    decimal CreditAmount,
    string Reason,
    string? ReferenceId,
    DateTimeOffset CreatedAt
);

public sealed record QuotaTransactionListDto(
    List<QuotaTransactionDto> Transactions,
    int TotalCount
);

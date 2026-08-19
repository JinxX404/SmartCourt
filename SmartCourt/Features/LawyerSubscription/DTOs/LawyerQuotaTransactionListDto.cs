using System;
using System.Collections.Generic;

namespace SmartCourt.Features.LawyerSubscription.DTOs;

public sealed record LawyerQuotaTransactionDto(
    Guid Id,
    decimal CreditAmount,
    string Reason,
    string? ReferenceId,
    DateTimeOffset CreatedAt
);

public sealed record LawyerQuotaTransactionListDto(
    List<LawyerQuotaTransactionDto> Transactions,
    int TotalCount
);

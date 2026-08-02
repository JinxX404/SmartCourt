using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Infrastructure.Providers.Jobs;

namespace SmartCourt.Features.Payments;

public interface IWalletService
{
    Task<WalletDto> GetAsync(
        CancellationToken cancellationToken);

    Task<PaymentActionResultDto> WithdrawAsync(
        CreateWithdrawalRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken);

    Task<JobExecutionResult> ReconcilePendingWithdrawalsAsync(
        CancellationToken cancellationToken);
}

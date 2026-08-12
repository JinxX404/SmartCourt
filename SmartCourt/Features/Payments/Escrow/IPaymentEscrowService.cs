using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Providers.Payments;

namespace SmartCourt.Features.Payments;

public interface IPaymentEscrowService
{
    Task<FundingOperationDto> FundAsync(
        Guid milestoneId,
        FundMilestoneRequest request,
        string? idempotencyKey,
        CancellationToken cancellationToken);

    Task<FundingOperationDto> FundWithConfirmationTokenAsync(
        Guid milestoneId,
        string confirmationTokenReference,
        string? idempotencyKey,
        CancellationToken cancellationToken)
        => throw new NotSupportedException();

    Task<FundingOperationDto> RetryAsync(
        Guid paymentTransactionId,
        string paymentMethodReference,
        string? idempotencyKey,
        CancellationToken cancellationToken);

    Task<FundingOperationDto> RetryWithConfirmationTokenAsync(
        Guid paymentTransactionId,
        string confirmationTokenReference,
        string? idempotencyKey,
        CancellationToken cancellationToken)
        => throw new NotSupportedException();

    Task<PaymentDto> CompleteFundingAsync(
        Milestone milestone,
        Guid lawyerUserId,
        PaymentTransaction paymentTransaction,
        ProviderResult providerResult,
        Guid? reservationId,
        Guid? actorUserId,
        Guid correlationId,
        CancellationToken cancellationToken);

    Task<PaymentActionResultDto> FinalizeFailedExternalResultAsync(
        Milestone milestone,
        PaymentTransaction paymentTransaction,
        string? providerTransactionId,
        Guid? reservationId,
        Guid correlationId,
        CancellationToken cancellationToken);

    Task<Guid?> FindProcessingFundingReservationIdAsync(
        Guid milestoneId,
        CancellationToken cancellationToken);
}

using SmartCourt.Features.Payments.DTOs;

namespace SmartCourt.Features.Payments;

public interface ILawyerPayoutAccountService
{
    Task<LawyerPayoutAccountDto?> GetAsync(CancellationToken cancellationToken);
    Task<PayoutAccountLinkDto> CreateOnboardingLinkAsync(
        CancellationToken cancellationToken);
    Task<PayoutAccountLinkDto> CreateDashboardLinkAsync(
        CancellationToken cancellationToken);
    Task<LawyerPayoutAccountDto> LinkSandboxAccountAsync(
        LinkLawyerPayoutAccountRequest request,
        CancellationToken cancellationToken);

    Task SynchronizeProviderAccountAsync(
        string providerAccountId,
        CancellationToken cancellationToken);
}

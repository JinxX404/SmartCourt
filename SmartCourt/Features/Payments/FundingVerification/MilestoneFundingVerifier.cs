using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Payments.FundingVerification;

internal sealed class MilestoneFundingVerifier(
    ApplicationDbContext dbContext) : IMilestoneFundingVerifier
{
    public async Task<VerifiedMilestoneFunding> VerifyAsync(
        Guid milestoneId,
        FundingVerificationOperation operation,
        CancellationToken cancellationToken)
    {
        var verifiedFunding = await VerifiedMilestoneFundingQuery
            .Create(
                dbContext.Set<Milestone>().AsNoTracking(),
                dbContext.Set<EscrowAccount>().AsNoTracking(),
                dbContext.Set<EscrowHold>().AsNoTracking(),
                dbContext.Set<PaymentTransaction>().AsNoTracking(),
                milestoneId,
                operation)
            .SingleOrDefaultAsync(cancellationToken);

        return verifiedFunding
            ?? throw new BusinessException(
                "Milestone funding could not be verified.");
    }
}

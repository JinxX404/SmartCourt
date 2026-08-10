using Microsoft.EntityFrameworkCore;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Proposals.Shared;

internal static class ProposalPersistence
{
    public static async Task<bool> TrySaveAsync(
        ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }
}

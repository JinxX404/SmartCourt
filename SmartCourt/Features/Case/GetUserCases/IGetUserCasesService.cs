using System.Threading;
using System.Threading.Tasks;
using SmartCourt.Common.Models;
using SmartCourt.Features.Case.GetUserCases.DTOs;

namespace SmartCourt.Features.Case.GetUserCases;

public interface IGetUserCasesService
{
    Task<PagedResult<GetUserCaseSummaryDto>> GetUserCasesAsync(
        GetUserCasesQuery query,
        CancellationToken cancellationToken = default);
}

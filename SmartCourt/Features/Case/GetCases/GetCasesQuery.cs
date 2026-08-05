using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Case.GetCases.DTOs;

namespace SmartCourt.Features.Case.GetCases;

public class GetCasesQuery : IRequest<ApiResponse<List<CaseListItemDto>>>
{
}

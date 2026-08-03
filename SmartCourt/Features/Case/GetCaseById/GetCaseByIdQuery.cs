using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Case.GetCaseById.DTOs;

namespace SmartCourt.Features.Case.GetCaseById;

public class GetCaseByIdQuery : IRequest<ApiResponse<CaseDto>>
{
    public Guid CaseId { get; set; }
}

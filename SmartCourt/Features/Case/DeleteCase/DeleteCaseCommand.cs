using MediatR;
using SmartCourt.Common.Models;

namespace SmartCourt.Features.Case.DeleteCase;

public class DeleteCaseCommand : IRequest<ApiResponse>
{
    public Guid CaseId { get; set; }
}

using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Case.CreateCase.DTOs;
using SmartCourt.Features.Case.UpdateCase.DTOs;

namespace SmartCourt.Features.Case.UpdateCase;

public class UpdateCaseCommand : IRequest<ApiResponse<UpdateCaseResponse>>
{
    public Guid CaseId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string? Governorate { get; set; }
    public string? City { get; set; }
    public IReadOnlyCollection<IFormFile>? Documents { get; set; }
}

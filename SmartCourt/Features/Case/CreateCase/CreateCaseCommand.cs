using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Case.CreateCase.DTOs;

namespace SmartCourt.Features.Case.CreateCase
{
    public class CreateCaseCommand : IRequest<ApiResponse<CreateCaseResponse>>
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string? Governorate { get; set; }
        public string? City { get; set; }
        public IReadOnlyCollection<IFormFile>? Documents { get; set; }
    }
}

using MediatR;
using SmartCourt.Common;
using SmartCourt.Features.UserVerification.SubmitVerificationDocuments.DTOs;

namespace SmartCourt.Features.UserVerification.SubmitVerificationDocuments
{
    public class SubmitVerificationDocumentsCommand : IRequest<ApiResponse<SubmitVerificationDocumentResponseDto>>
    {
        public Guid UserId { get; set; }
        public List<VerificationDocumentDto> Documents { get; set; }
    }
}

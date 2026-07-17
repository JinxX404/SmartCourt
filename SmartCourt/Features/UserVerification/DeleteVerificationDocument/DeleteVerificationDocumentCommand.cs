using MediatR;
using SmartCourt.Common;

namespace SmartCourt.Features.UserVerification.DeleteVerificationDocument
{
    public sealed record DeleteVerificationDocumentCommand : IRequest<ApiResponse>
    {
        public string UserId { get; set; }
        public Guid DocumentId { get; set; }
    }
}

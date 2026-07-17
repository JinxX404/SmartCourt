using MediatR;
using SmartCourt.Common;
using SmartCourt.Features.UserVerification.GetUserVerificationDocuments.DTOs;

namespace SmartCourt.Features.UserVerification.GetUserVerificationDocuments
{
    public sealed record GetUserVerificationDocumentsQuery : IRequest<ApiResponse<GetUserVerificationDocumentsResponseDto>>
    {
        public string UserId { get; set; }
    }
}

using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.UserVerification.GetUserVerificationDocumentContent.DTOs;

namespace SmartCourt.Features.UserVerification.GetUserVerificationDocumentContent;

public sealed record GetUserVerificationDocumentContentQuery(Guid UserId, Guid DocumentId)
    : IRequest<ApiResponse<UserVerificationDocumentContentDto>>;

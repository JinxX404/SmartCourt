using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Admin.Verifications.GetVerificationDocumentContent.DTOs;

namespace SmartCourt.Features.Admin.Verifications.GetVerificationDocumentContent;

public sealed record GetVerificationDocumentContentQuery(Guid DocumentId)
    : IRequest<ApiResponse<VerificationDocumentContentDto>>;

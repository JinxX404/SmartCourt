using SmartCourt.Common.Models;
﻿using MediatR;
using SmartCourt.Features.UserVerification.SubmitVerificationDocuments.DTOs;

namespace SmartCourt.Features.UserVerification.SubmitVerificationDocuments
{
    public class SubmitVerificationDocumentsCommand : IRequest<ApiResponse<SubmitVerificationDocumentResponseDto>>
    {
        public Guid UserId { get; set; }
        public List<VerificationDocumentDto> Documents { get; set; }
    }
}

using SmartCourt.Common.Models;
﻿using MediatR;
using SmartCourt.Features.UserVerification.GetUserVerificationDocuments.DTOs;

namespace SmartCourt.Features.UserVerification.GetUserVerificationDocuments
{
    public sealed record GetUserVerificationDocumentsQuery : IRequest<ApiResponse<GetUserVerificationDocumentsResponseDto>>
    {
        public Guid UserId { get; set; }
    }
}

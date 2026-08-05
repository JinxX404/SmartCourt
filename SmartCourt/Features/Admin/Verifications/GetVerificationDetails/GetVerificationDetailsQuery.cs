using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Admin.Verifications.GetVerificationDetails.DTOs;

namespace SmartCourt.Features.Admin.Verifications.GetVerificationDetails;

public sealed record GetVerificationDetailsQuery(Guid LawyerId)
    : IRequest<ApiResponse<VerificationDetailsDto>>;

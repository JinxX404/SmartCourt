using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Auth.PhoneVerification.DTOs;

namespace SmartCourt.Features.Auth.PhoneVerification;

public record ConfirmPhoneVerificationCommand(ConfirmPhoneVerificationRequest Request) : IRequest<ApiResponse<object>>;

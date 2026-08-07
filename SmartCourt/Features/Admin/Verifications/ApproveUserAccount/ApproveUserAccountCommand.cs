using MediatR;
using SmartCourt.Common.Models;

namespace SmartCourt.Features.Admin.Verifications.ApproveUserAccount;

public sealed record ApproveUserAccountCommand(Guid UserId) : IRequest<ApiResponse<object>>;

using MediatR;
using SmartCourt.Common.Models;

namespace SmartCourt.Features.Admin.Verifications.RejectUserAccount;

public sealed record RejectUserAccountCommand(Guid UserId, string RejectionReason) : IRequest<ApiResponse<object>>;

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Features.Admin.Verifications.GetPendingVerifications;
using SmartCourt.Features.Admin.Verifications.GetVerificationDetails;
using SmartCourt.Features.Admin.Verifications.GetVerificationDocumentContent;
using SmartCourt.Features.Admin.Verifications.ReviewVerificationDocument;
using SmartCourt.Features.Admin.Verifications.ReviewVerificationDocument.DTOs;

namespace SmartCourt.Features.Admin.Verifications;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/verifications")]
public sealed class AdminVerificationsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPendingAsync(
        [FromQuery] GetPendingVerificationsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{lawyerId:guid}")]
    public async Task<IActionResult> GetDetailsAsync(Guid lawyerId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetVerificationDetailsQuery(lawyerId), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("documents/{documentId:guid}/content")]
    public async Task<IActionResult> GetDocumentContentAsync(Guid documentId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetVerificationDocumentContentQuery(documentId), cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("documents/{documentId:guid}")]
    public async Task<IActionResult> ReviewAsync(
        Guid documentId,
        [FromBody] ReviewVerificationDocumentRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ReviewVerificationDocumentCommand(
            documentId,
            request.Decision,
            request.RejectionReason);

        var result = await mediator.Send(command, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("{userId:guid}/approve-account")]
    public async Task<IActionResult> ApproveUserAccountAsync(Guid userId, CancellationToken cancellationToken)
    {
        var command = new SmartCourt.Features.Admin.Verifications.ApproveUserAccount.ApproveUserAccountCommand(userId);
        var result = await mediator.Send(command, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("{userId:guid}/reject-account")]
    public async Task<IActionResult> RejectUserAccountAsync(
        Guid userId, 
        [FromBody] SmartCourt.Features.Admin.Verifications.RejectUserAccount.RejectUserAccountRequest request, 
        CancellationToken cancellationToken)
    {
        var command = new SmartCourt.Features.Admin.Verifications.RejectUserAccount.RejectUserAccountCommand(userId, request.RejectionReason);
        var result = await mediator.Send(command, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}

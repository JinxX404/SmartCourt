using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Common.Validation;

namespace SmartCourt.Features.Contracts.Files;

[ApiController]
[Route("api/contracts/{contractId:guid}/files")]
[Authorize(Roles = "Client,Lawyer")]
public sealed class ContractFilesController(
    IContractFileService contractFileService,
    IValidator<UploadContractFilesRequest> validator) : ControllerBase
{
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(27 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 27 * 1024 * 1024)]
    [SecurityRateLimit(RateLimitPolicyNames.SensitiveMutation)]
    [ProducesResponseType(
        typeof(ApiResponse<IReadOnlyList<ContractFileDto>>),
        StatusCodes.Status201Created)]
    public async Task<
        ActionResult<ApiResponse<IReadOnlyList<ContractFileDto>>>> UploadAsync(
            Guid contractId,
            [FromForm] UploadContractFilesRequest request,
            CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowBusinessExceptionAsync(
            request,
            cancellationToken);
        var files = await contractFileService.UploadAsync(
            contractId,
            request,
            cancellationToken);
        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<IReadOnlyList<ContractFileDto>>.Created(files));
    }

    [HttpDelete("{storedFileId:guid}")]
    [SecurityRateLimit(RateLimitPolicyNames.StandardMutation)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> DeleteAsync(
        Guid contractId,
        Guid storedFileId,
        CancellationToken cancellationToken)
    {
        await contractFileService.DeleteAsync(
            contractId,
            storedFileId,
            cancellationToken);
        return Ok(ApiResponse.Ok("تم حذف الملف بنجاح."));
    }
}

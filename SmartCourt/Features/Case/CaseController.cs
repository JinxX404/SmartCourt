using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using SmartCourt.Common.Models;
using SmartCourt.Features.Case.CreateCase;
using SmartCourt.Features.Case.CreateCase.DTOs;
using SmartCourt.Features.Case.UpdateCase;
using SmartCourt.Features.Case.UpdateCase.DTOs;
using SmartCourt.Features.Case.DeleteCase;
using SmartCourt.Features.Case.GetCaseById;
using SmartCourt.Features.Case.GetCaseById.DTOs;
using SmartCourt.Features.Case.GetCases;
using SmartCourt.Features.Case.GetCases.DTOs;
using SmartCourt.Features.Case.DownloadCaseDocument;

using System.Threading;
using SmartCourt.Features.Case.AddCaseDocument;
using SmartCourt.Features.Case.AddCaseDocument.DTOs;

namespace SmartCourt.Features.Case
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CaseController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IAddCaseDocumentService _addCaseDocumentService;

        public CaseController(IMediator mediator, IAddCaseDocumentService addCaseDocumentService)
        {
            _mediator = mediator;
            _addCaseDocumentService = addCaseDocumentService;
        }

        [HttpPost]
        [Authorize(Roles = "Client")]
        public async Task<ActionResult<ApiResponse<CreateCaseResponse>>> Create([FromForm] CreateCaseCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        [HttpPut]
        [Authorize(Roles = "Client")]
        public async Task<ActionResult<ApiResponse<UpdateCaseResponse>>> Update([FromForm] UpdateCaseCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Client")]
        public async Task<ActionResult<ApiResponse>> Delete([FromRoute] Guid id)
        {
            var command = new DeleteCaseCommand { CaseId = id };
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Client,Lawyer")]
        public async Task<ActionResult<ApiResponse<CaseDto>>> GetById([FromRoute] Guid id)
        {
            var query = new GetCaseByIdQuery { CaseId = id };
            var result = await _mediator.Send(query);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Client,Lawyer")]
        public async Task<ActionResult<ApiResponse<List<CaseListItemDto>>>> GetAll()
        {
            var query = new GetCasesQuery();
            var result = await _mediator.Send(query);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        [HttpPost("{id:guid}/finalize")]
        [Authorize(Roles = "Client")]
        public async Task<ActionResult<ApiResponse<SmartCourt.Features.Matching.DTOs.FinalizeResultDto>>> Finalize([FromRoute] Guid id)
        {
            var command = new SmartCourt.Features.Case.FinalizeCase.FinalizeCaseCommand { CaseId = id };
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        [HttpGet("{caseId:guid}/documents/{documentId:guid}/download")]
        [Authorize(Roles = "Client,Lawyer")]
        public async Task<IActionResult> DownloadDocument([FromRoute] Guid caseId, [FromRoute] Guid documentId)
        {
            var query = new DownloadCaseDocumentQuery
            {
                CaseId = caseId,
                DocumentId = documentId
            };

            var result = await _mediator.Send(query);

            return File(result.FileBytes, result.ContentType, result.FileName);
        }

        [HttpPost("{caseId:guid}/documents")]
        [Authorize(Roles = "Client")]
        public async Task<ActionResult<ApiResponse<AddCaseDocumentResponse>>> AddDocuments(
            [FromRoute] Guid caseId,
            [FromForm] AddCaseDocumentRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _addCaseDocumentService.AddDocumentsAsync(caseId, request, cancellationToken);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }
    }
}

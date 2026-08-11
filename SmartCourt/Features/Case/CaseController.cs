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

namespace SmartCourt.Features.Case
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CaseController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CaseController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CreateCaseResponse>>> Create([FromForm] CreateCaseCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<UpdateCaseResponse>>> Update([FromRoute] Guid id, [FromForm] UpdateCaseCommand command)
        {
            command.CaseId = id;

            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse>> Delete([FromRoute] Guid id)
        {
            var command = new DeleteCaseCommand { CaseId = id };
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CaseDto>>> GetById([FromRoute] Guid id)
        {
            var query = new GetCaseByIdQuery { CaseId = id };
            var result = await _mediator.Send(query);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<CaseListItemDto>>>> GetAll()
        {
            var query = new GetCasesQuery();
            var result = await _mediator.Send(query);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        [HttpPost("{id:guid}/finalize")]
        public async Task<ActionResult<ApiResponse<SmartCourt.Features.Matching.DTOs.FinalizeResultDto>>> Finalize([FromRoute] Guid id)
        {
            var command = new SmartCourt.Features.Case.FinalizeCase.FinalizeCaseCommand { CaseId = id };
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        [HttpGet("{caseId:guid}/documents/{documentId:guid}/download")]
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
    }
}

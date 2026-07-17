using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common;
using SmartCourt.Features.UserVerification.DeleteVerificationDocument;
using SmartCourt.Features.UserVerification.GetUserVerificationDocuments;
using SmartCourt.Features.UserVerification.GetUserVerificationDocuments.DTOs;
using SmartCourt.Features.UserVerification.SubmitVerificationDocuments;
using SmartCourt.Features.UserVerification.SubmitVerificationDocuments.DTOs;

namespace SmartCourt.Features.UserVerification
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserVerificationController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserVerificationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("submit-verification-documents")]
        public async Task<ActionResult<ApiResponse<SubmitVerificationDocumentResponseDto>>> SubmitVerificationDocuments([FromForm]SubmitVerificationDocumentsCommand command)
         {
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        [HttpGet("{UserId}")]
        public async Task<ActionResult<ApiResponse<GetUserVerificationDocumentsResponseDto>>> GetUserVerificationDocuments([FromRoute] GetUserVerificationDocumentsQuery query)
        {
            var result = await _mediator.Send(query);

            if(!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        [HttpDelete]
        public async Task<ActionResult<ApiResponse>> DeleteVerificationDocument([FromQuery] DeleteVerificationDocumentCommand command)
        {
            var result = await _mediator.Send(command);

            if(!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }
    }
}

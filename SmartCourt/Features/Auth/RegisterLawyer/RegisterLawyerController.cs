using SmartCourt.Common.Models;
using SmartCourt.Features.Auth.RegisterLawyer.DTOs;
using SmartCourt.Features.Auth.RegisterClient.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace SmartCourt.Features.Auth.RegisterLawyer;

[ApiController]
[Route("api/auth/register/lawyer")]
public class RegisterLawyerController(IRegisterLawyerService registerLawyerService) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Post([FromBody] RegisterLawyerRequest request, CancellationToken cancellationToken)
    {
        var response = await registerLawyerService.RegisterLawyerAsync(request, cancellationToken);
        var apiResponse = ApiResponse<RegisterResponse>.Created(response);
        apiResponse.Message = "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني";
        return Created(string.Empty, apiResponse);
    }
}

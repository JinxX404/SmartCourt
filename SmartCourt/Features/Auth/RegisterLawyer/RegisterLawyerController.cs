using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartCourt.Common;

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

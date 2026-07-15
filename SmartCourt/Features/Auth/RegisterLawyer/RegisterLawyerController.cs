using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartCourt.Common;
using SmartCourt.Interfaces;
using SmartCourt.Features.Auth;

namespace SmartCourt.Features.Auth.RegisterLawyer;

[ApiController]
[Route("api/auth/register/lawyer")]
public class RegisterLawyerController : ControllerBase
{
    private readonly IAuthService _authService;

    public RegisterLawyerController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Post([FromBody] RegisterLawyerRequest request)
    {
        var response = await _authService.RegisterLawyerAsync(request);
        var apiResponse = ApiResponse<RegisterResponse>.Created(response);
        apiResponse.Message = "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني";
        return Created(string.Empty, apiResponse);
    }
}

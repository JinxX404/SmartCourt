using SmartCourt.Common.Models;
using SmartCourt.Features.Auth.RegisterClient.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace SmartCourt.Features.Auth.RegisterClient;

[ApiController]
[Route("api/auth/register/client")]
public class RegisterClientController(IRegisterClientService registerClientService) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Post(RegisterClientRequest request, CancellationToken cancellationToken)
    {
        var response = await registerClientService.RegisterClientAsync(request, cancellationToken);
        var apiResponse = ApiResponse<RegisterResponse>.Created(response);
        apiResponse.Message = "تم إنشاء الحساب بنجاح. يرجى تأكيد البريد الإلكتروني";
        return Created(string.Empty, apiResponse);
    }
}

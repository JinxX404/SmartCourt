using System;
using Microsoft.AspNetCore.Mvc;

namespace SmartCourt.API.Features.Test;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { Message = "SmartCourt API is running!" });
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new 
        { 
            Message = "Pong! The automated deployment is fully operational.",
            ServerTimeUtc = DateTime.UtcNow,
            Version = "1.0.1"
        });
    }

    [HttpGet("error")]
    public IActionResult GetError()
    {
        var error = AppDomain.CurrentDomain.GetData("MigrationError") as string;
        if (string.IsNullOrEmpty(error)) return Ok("No migration error recorded on startup.");
        return Ok(error);
    }

    [HttpPost("email")]
    public async System.Threading.Tasks.Task<IActionResult> SendTestEmail([FromServices] SmartCourt.Core.Interfaces.Providers.IEmailProvider emailProvider)
    {
        var result = await emailProvider.SendEmailAsync(
            to: "moatazmohammed2392003@gmail.com",
            subject: "Smart Court - Hangfire Test",
            body: "If you are reading this, the background Hangfire email provider is fully operational on Smart Court!",
            isHtml: false);

        return Ok(SmartCourt.API.Common.ApiResponse<object>.Ok(new { Enqueued = result }, "Email has been enqueued to Hangfire!"));
    }
}

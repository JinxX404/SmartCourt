using SmartCourt.Common.Models;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace SmartCourt.Features.Test;

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
    public async System.Threading.Tasks.Task<IActionResult> SendTestEmail([FromServices] SmartCourt.Interfaces.Providers.IEmailProvider emailProvider, CancellationToken cancellationToken)
    {
        var result = await emailProvider.SendEmailAsync(
            to: "moatazmohammed2392003@gmail.com",
            subject: "Smart Court - Hangfire Test",
            body: "If you are reading this, the background Hangfire email provider is fully operational on Smart Court!",
            isHtml: false,
            cancellationToken: cancellationToken);

        return Ok(SmartCourt.Common.Models.ApiResponse<object>.Ok(new { Enqueued = result }, "Email has been enqueued to Hangfire!"));
    }

    [HttpPost("sms")]
    public async System.Threading.Tasks.Task<IActionResult> SendTestSms([FromServices] SmartCourt.Interfaces.Providers.ISmsProvider smsProvider, [FromQuery] string to)
    {
        if (string.IsNullOrEmpty(to)) return BadRequest("Please provide a 'to' query parameter with your phone number.");

        var result = await smsProvider.SendSmsAsync(
            phoneNumber: to,
            message: "Smart Court - If you are reading this, the background Hangfire Twilio SMS provider is fully operational!");

        return Ok(SmartCourt.Common.Models.ApiResponse<object>.Ok(new { Enqueued = result }, "SMS has been enqueued to Hangfire!"));
    }
}

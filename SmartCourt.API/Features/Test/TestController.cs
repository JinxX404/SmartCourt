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
}

using Microsoft.AspNetCore.Mvc;

namespace SmartCourt.Features.Health;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new 
        { 
            Message = "Pong! Mostashar API is fully operational.",
            ServerTimeUtc = DateTimeOffset.UtcNow,
            Version = "1.0.0"
        });
    }
}

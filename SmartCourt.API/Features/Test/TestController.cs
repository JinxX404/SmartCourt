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
}

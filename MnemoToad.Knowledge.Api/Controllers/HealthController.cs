using Microsoft.AspNetCore.Mvc;

namespace MnemoToad.Knowledge.Api.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() =>
        Ok(new { status = "pass" });
}

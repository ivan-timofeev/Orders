using Microsoft.AspNetCore.Mvc;

namespace Orders.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthCheckController : ControllerBase
{
    // GET: api/healthcheck
    [HttpGet]
    public IActionResult HealthCheck()
    {
        return Ok("I'm alive!");
    }
}
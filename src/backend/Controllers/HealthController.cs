using Microsoft.AspNetCore.Mvc;

namespace SterlingAuctions.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Basic health check endpoint
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { 
            status = "healthy", 
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        });
    }

    /// <summary>
    /// Detailed health check
    /// </summary>
    [HttpGet("detailed")]
    public IActionResult GetDetailed()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            uptime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime,
            machine = Environment.MachineName,
            platform = Environment.OSVersion.VersionString
        });
    }
}

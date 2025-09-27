using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SterlingAuctions.SimpleAPI.Middleware;
using SterlingAuctions.SimpleAPI.Services;

namespace SterlingAuctions.SimpleAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessionController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly ILogger<SessionController> _logger;

    public SessionController(ISessionService sessionService, ILogger<SessionController> logger)
    {
        _sessionService = sessionService;
        _logger = logger;
    }

    /// <summary>
    /// Get current session information
    /// </summary>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrentSession()
    {
        try
        {
            var sessionId = Request.Headers["X-Session-Id"].FirstOrDefault();
            if (string.IsNullOrEmpty(sessionId))
            {
                return BadRequest("Session ID not provided");
            }

            var session = await _sessionService.GetSessionAsync(sessionId);
            if (session == null)
            {
                return NotFound("Session not found or expired");
            }

            _logger.LogInformation("Retrieved session {SessionId} for user {UserId}", sessionId, session.UserId);
            
            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current session");
            return StatusCode(500, "An error occurred while retrieving session");
        }
    }

    /// <summary>
    /// Create a new session
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateSession()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var deviceInfo = Request.Headers["User-Agent"].FirstOrDefault();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            
            var session = await _sessionService.CreateSessionAsync(userId, deviceInfo, ipAddress);
            
            _logger.LogInformation("Created session {SessionId} for user {UserId}", session.SessionId, userId);
            
            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session");
            return StatusCode(500, "An error occurred while creating session");
        }
    }

    /// <summary>
    /// Update session data
    /// </summary>
    [HttpPut("{sessionId}")]
    public async Task<IActionResult> UpdateSession(string sessionId, [FromBody] Dictionary<string, object> sessionData)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _sessionService.UpdateSessionAsync(sessionId, sessionData);
            
            if (!result)
            {
                return NotFound("Session not found or expired");
            }
            
            _logger.LogInformation("Updated session {SessionId} for user {UserId}", sessionId, userId);
            
            return Ok(new { message = "Session updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session {SessionId}", sessionId);
            return StatusCode(500, "An error occurred while updating session");
        }
    }

    /// <summary>
    /// Extend session expiration
    /// </summary>
    [HttpPost("{sessionId}/extend")]
    public async Task<IActionResult> ExtendSession(string sessionId, [FromBody] ExtendSessionDto extendDto)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var extension = extendDto.ExtensionMinutes.HasValue 
                ? TimeSpan.FromMinutes(extendDto.ExtensionMinutes.Value) 
                : (TimeSpan?)null;
                
            var result = await _sessionService.ExtendSessionAsync(sessionId, extension);
            
            if (!result)
            {
                return NotFound("Session not found or expired");
            }
            
            _logger.LogInformation("Extended session {SessionId} for user {UserId}", sessionId, userId);
            
            return Ok(new { message = "Session extended successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extending session {SessionId}", sessionId);
            return StatusCode(500, "An error occurred while extending session");
        }
    }

    /// <summary>
    /// Invalidate current session
    /// </summary>
    [HttpDelete("{sessionId}")]
    public async Task<IActionResult> InvalidateSession(string sessionId)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _sessionService.InvalidateSessionAsync(sessionId);
            
            if (!result)
            {
                return NotFound("Session not found");
            }
            
            _logger.LogInformation("Invalidated session {SessionId} for user {UserId}", sessionId, userId);
            
            return Ok(new { message = "Session invalidated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating session {SessionId}", sessionId);
            return StatusCode(500, "An error occurred while invalidating session");
        }
    }

    /// <summary>
    /// Invalidate all sessions for current user
    /// </summary>
    [HttpDelete("all")]
    public async Task<IActionResult> InvalidateAllSessions()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _sessionService.InvalidateUserSessionsAsync(userId);
            
            if (!result)
            {
                return BadRequest("Unable to invalidate sessions");
            }
            
            _logger.LogInformation("Invalidated all sessions for user {UserId}", userId);
            
            return Ok(new { message = "All sessions invalidated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating all sessions for user");
            return StatusCode(500, "An error occurred while invalidating sessions");
        }
    }

    /// <summary>
    /// Get all sessions for current user
    /// </summary>
    [HttpGet("my-sessions")]
    public async Task<IActionResult> GetMySessions()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var sessions = await _sessionService.GetUserSessionsAsync(userId);
            
            _logger.LogInformation("Retrieved {Count} sessions for user {UserId}", sessions.Count(), userId);
            
            return Ok(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user sessions");
            return StatusCode(500, "An error occurred while retrieving sessions");
        }
    }

    /// <summary>
    /// Check if session is valid
    /// </summary>
    [HttpGet("{sessionId}/validate")]
    public async Task<IActionResult> ValidateSession(string sessionId)
    {
        try
        {
            var isValid = await _sessionService.IsSessionValidAsync(sessionId);
            
            _logger.LogDebug("Validated session {SessionId}: {IsValid}", sessionId, isValid);
            
            return Ok(new { isValid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating session {SessionId}", sessionId);
            return StatusCode(500, "An error occurred while validating session");
        }
    }

    /// <summary>
    /// Get session statistics (Admin only)
    /// </summary>
    [HttpGet("statistics")]
    [AdminOnly]
    public async Task<IActionResult> GetSessionStatistics()
    {
        try
        {
            var statistics = await _sessionService.GetSessionStatisticsAsync();
            
            _logger.LogInformation("Retrieved session statistics");
            
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session statistics");
            return StatusCode(500, "An error occurred while retrieving session statistics");
        }
    }

    /// <summary>
    /// Cleanup expired sessions (Admin only)
    /// </summary>
    [HttpPost("cleanup")]
    [AdminOnly]
    public async Task<IActionResult> CleanupExpiredSessions()
    {
        try
        {
            await _sessionService.CleanupExpiredSessionsAsync();
            
            _logger.LogInformation("Cleaned up expired sessions");
            
            return Ok(new { message = "Expired sessions cleaned up successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired sessions");
            return StatusCode(500, "An error occurred while cleaning up sessions");
        }
    }
}

public class ExtendSessionDto
{
    public int? ExtensionMinutes { get; set; }
}

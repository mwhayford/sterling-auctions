using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SterlingAuctions.SimpleAPI.Middleware;

namespace SterlingAuctions.SimpleAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Public endpoint - no authentication required
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult PublicEndpoint()
    {
        return Ok(new
        {
            message = "This is a public endpoint - no authentication required",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Protected endpoint - requires authentication only
    /// </summary>
    [HttpGet("protected")]
    [Authorize]
    public IActionResult ProtectedEndpoint()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var userRoles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();

        _logger.LogInformation("User {UserId} accessed protected endpoint", userId);

        return Ok(new
        {
            message = "This is a protected endpoint - authentication required",
            user = new
            {
                id = userId,
                email = userEmail,
                roles = userRoles
            },
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Admin only endpoint - requires Admin role
    /// </summary>
    [HttpGet("admin-only")]
    [AdminOnly]
    public IActionResult AdminOnlyEndpoint()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("Admin {UserId} accessed admin-only endpoint", userId);

        return Ok(new
        {
            message = "This is an admin-only endpoint",
            user = new
            {
                id = userId,
                email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList()
            },
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Member or Admin endpoint - requires Member or Admin role
    /// </summary>
    [HttpGet("member-or-admin")]
    [MemberOrAdmin]
    public IActionResult MemberOrAdminEndpoint()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("User {UserId} accessed member-or-admin endpoint", userId);

        return Ok(new
        {
            message = "This endpoint requires Member or Admin role",
            user = new
            {
                id = userId,
                email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList()
            },
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Custom role requirement - requires specific roles
    /// </summary>
    [HttpGet("custom-roles")]
    [RequireRole("Admin", "Member")]
    public IActionResult CustomRolesEndpoint()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("User {UserId} accessed custom-roles endpoint", userId);

        return Ok(new
        {
            message = "This endpoint requires Admin or Member role",
            user = new
            {
                id = userId,
                email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList()
            },
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Multiple roles requirement - requires ALL specified roles
    /// </summary>
    [HttpGet("multiple-roles")]
    [RequireRole(true, "Admin", "Member")] // requireAll = true
    public IActionResult MultipleRolesEndpoint()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("User {UserId} accessed multiple-roles endpoint", userId);

        return Ok(new
        {
            message = "This endpoint requires ALL specified roles (Admin AND Member)",
            user = new
            {
                id = userId,
                email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList()
            },
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Test authorization policies
    /// </summary>
    [HttpGet("policy-test")]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult PolicyTestEndpoint()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("User {UserId} accessed policy-test endpoint", userId);

        return Ok(new
        {
            message = "This endpoint uses the AdminOnly policy",
            user = new
            {
                id = userId,
                email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList()
            },
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Test auction permissions
    /// </summary>
    [HttpGet("auction-permissions")]
    [RequireAuctionPermission(AuctionPermission.View)]
    public IActionResult AuctionPermissionsEndpoint()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("User {UserId} accessed auction-permissions endpoint", userId);

        return Ok(new
        {
            message = "This endpoint requires auction view permission",
            user = new
            {
                id = userId,
                email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList()
            },
            permissions = new
            {
                canView = true,
                canCreate = User.IsInRole("Member") || User.IsInRole("Admin"),
                canBid = User.IsInRole("Member") || User.IsInRole("Admin"),
                canManage = User.IsInRole("Admin")
            },
            timestamp = DateTime.UtcNow
        });
    }
}

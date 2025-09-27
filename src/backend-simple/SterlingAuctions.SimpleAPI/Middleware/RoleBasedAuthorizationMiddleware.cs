using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SterlingAuctions.SimpleAPI.Middleware;

/// <summary>
/// Middleware for role-based authorization
/// </summary>
public class RoleBasedAuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RoleBasedAuthorizationMiddleware> _logger;

    public RoleBasedAuthorizationMiddleware(RequestDelegate next, ILogger<RoleBasedAuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if the endpoint requires authorization
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<AuthorizeAttribute>() != null)
        {
            await ProcessAuthorizationAsync(context);
        }

        await _next(context);
    }

    private async Task ProcessAuthorizationAsync(HttpContext context)
    {
        try
        {
            var user = context.User;
            
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogWarning("Unauthenticated user attempted to access protected resource at {Path}", context.Request.Path);
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized: Authentication required");
                return;
            }

            // Check if user has required roles
            var authorizeAttribute = context.GetEndpoint()?.Metadata?.GetMetadata<AuthorizeAttribute>();
            if (authorizeAttribute?.Roles != null)
            {
                var requiredRoles = authorizeAttribute.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries);
                var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

                if (!requiredRoles.Any(role => userRoles.Contains(role.Trim())))
                {
                    _logger.LogWarning("User {UserId} with roles [{UserRoles}] attempted to access resource requiring roles [{RequiredRoles}] at {Path}", 
                        user.FindFirstValue(ClaimTypes.NameIdentifier),
                        string.Join(", ", userRoles),
                        string.Join(", ", requiredRoles),
                        context.Request.Path);
                    
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("Forbidden: Insufficient permissions");
                    return;
                }
            }

            // Log successful authorization
            _logger.LogDebug("User {UserId} with roles [{UserRoles}] successfully authorized for {Path}", 
                user.FindFirstValue(ClaimTypes.NameIdentifier),
                string.Join(", ", user.FindAll(ClaimTypes.Role).Select(c => c.Value)),
                context.Request.Path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during authorization processing");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal server error during authorization");
        }
    }
}

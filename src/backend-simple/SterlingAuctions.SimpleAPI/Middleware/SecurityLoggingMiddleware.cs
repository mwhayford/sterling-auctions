using Serilog;
using Serilog.Context;
using System.Security.Claims;

namespace SterlingAuctions.SimpleAPI.Middleware;

/// <summary>
/// Security logging middleware for authentication and authorization events
/// </summary>
public class SecurityLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityLoggingMiddleware> _logger;

    public SecurityLoggingMiddleware(RequestDelegate next, ILogger<SecurityLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        var userId = context.User?.Identity?.Name;
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        var userAgent = context.Request.Headers.UserAgent.ToString();
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? "";

        // Log authentication events
        LogAuthenticationEvents(context, userId, ipAddress, userAgent);

        try
        {
            await _next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            LogAuthorizationFailure(context, ex, userId, ipAddress);
            throw;
        }
        catch (Exception ex)
        {
            LogSecurityException(context, ex, userId, ipAddress);
            throw;
        }
        finally
        {
            // Log authorization events
            LogAuthorizationEvents(context, userId, ipAddress, method, path);
        }
    }

    private void LogAuthenticationEvents(HttpContext context, string userId, string ipAddress, string userAgent)
    {
        // Check for authentication-related paths
        if (IsAuthenticationPath(context.Request.Path))
        {
            using (LogContext.PushProperty("Component", "Security"))
            using (LogContext.PushProperty("EventType", "AuthenticationAttempt"))
            using (LogContext.PushProperty("UserId", userId))
            using (LogContext.PushProperty("IpAddress", ipAddress))
            using (LogContext.PushProperty("UserAgent", userAgent))
            using (LogContext.PushProperty("RequestPath", context.Request.Path))
            using (LogContext.PushProperty("RequestMethod", context.Request.Method))
            {
                _logger.Information("Authentication attempt: {RequestMethod} {RequestPath} by user {UserId} from {IpAddress}", 
                    context.Request.Method, context.Request.Path, userId, ipAddress);
            }
        }

        // Log successful authentication
        if (context.User?.Identity?.IsAuthenticated == true && !string.IsNullOrEmpty(userId))
        {
            var authMethod = GetAuthenticationMethod(context.User);
            using (LogContext.PushProperty("Component", "Security"))
            using (LogContext.PushProperty("EventType", "AuthenticationSuccess"))
            using (LogContext.PushProperty("UserId", userId))
            using (LogContext.PushProperty("IpAddress", ipAddress))
            using (LogContext.PushProperty("AuthMethod", authMethod))
            {
                _logger.Information("Authentication successful: User {UserId} authenticated via {AuthMethod} from {IpAddress}", 
                    userId, authMethod, ipAddress);
            }
        }
    }

    private void LogAuthorizationEvents(HttpContext context, string userId, string ipAddress, string method, string path)
    {
        // Log access to sensitive endpoints
        if (IsSensitiveEndpoint(path))
        {
            var isAuthorized = context.Response.StatusCode != 403;
            var eventType = isAuthorized ? "AuthorizedAccess" : "UnauthorizedAccess";
            
            using (LogContext.PushProperty("Component", "Security"))
            using (LogContext.PushProperty("EventType", eventType))
            using (LogContext.PushProperty("UserId", userId))
            using (LogContext.PushProperty("IpAddress", ipAddress))
            using (LogContext.PushProperty("RequestMethod", method))
            using (LogContext.PushProperty("RequestPath", path))
            using (LogContext.PushProperty("ResponseStatusCode", context.Response.StatusCode))
            {
                var level = isAuthorized ? LogEventLevel.Information : LogEventLevel.Warning;
                _logger.Write(level, "Authorization Event: {EventType} - {Method} {Path} by user {UserId} from {IpAddress} -> {StatusCode}", 
                    eventType, method, path, userId, ipAddress, context.Response.StatusCode);
            }
        }

        // Log admin access
        if (IsAdminEndpoint(path) && context.User?.IsInRole("Admin") == true)
        {
            using (LogContext.PushProperty("Component", "Security"))
            using (LogContext.PushProperty("EventType", "AdminAccess"))
            using (LogContext.PushProperty("UserId", userId))
            using (LogContext.PushProperty("IpAddress", ipAddress))
            using (LogContext.PushProperty("RequestMethod", method))
            using (LogContext.PushProperty("RequestPath", path))
            {
                _logger.Information("Admin access: {Method} {Path} by admin user {UserId} from {IpAddress}", 
                    method, path, userId, ipAddress);
            }
        }
    }

    private void LogAuthorizationFailure(HttpContext context, UnauthorizedAccessException ex, string userId, string ipAddress)
    {
        using (LogContext.PushProperty("Component", "Security"))
        using (LogContext.PushProperty("EventType", "AuthorizationFailure"))
        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("IpAddress", ipAddress))
        using (LogContext.PushProperty("RequestPath", context.Request.Path))
        using (LogContext.PushProperty("RequestMethod", context.Request.Method))
        using (LogContext.PushProperty("Exception", ex.Message))
        {
            _logger.Warning("Authorization failure: {Method} {Path} by user {UserId} from {IpAddress}. Reason: {Exception}", 
                context.Request.Method, context.Request.Path, userId, ipAddress, ex.Message);
        }
    }

    private void LogSecurityException(HttpContext context, Exception ex, string userId, string ipAddress)
    {
        // Check if this is a security-related exception
        if (IsSecurityException(ex))
        {
            using (LogContext.PushProperty("Component", "Security"))
            using (LogContext.PushProperty("EventType", "SecurityException"))
            using (LogContext.PushProperty("UserId", userId))
            using (LogContext.PushProperty("IpAddress", ipAddress))
            using (LogContext.PushProperty("RequestPath", context.Request.Path))
            using (LogContext.PushProperty("RequestMethod", context.Request.Method))
            using (LogContext.PushProperty("Exception", ex.Message))
            {
                _logger.Error(ex, "Security exception: {Method} {Path} by user {UserId} from {IpAddress}", 
                    context.Request.Method, context.Request.Path, userId, ipAddress);
            }
        }
    }

    private bool IsAuthenticationPath(PathString path)
    {
        var authPaths = new[] { "/api/auth/login", "/api/auth/register", "/api/auth/google", "/api/auth/logout" };
        return authPaths.Any(authPath => path.StartsWithSegments(authPath));
    }

    private bool IsSensitiveEndpoint(string path)
    {
        var sensitivePaths = new[] { "/api/admin", "/api/payments", "/api/users", "/api/auctions/create", "/api/auctions/update" };
        return sensitivePaths.Any(sensitivePath => path.StartsWith(sensitivePath));
    }

    private bool IsAdminEndpoint(string path)
    {
        return path.StartsWith("/api/admin");
    }

    private bool IsSecurityException(Exception ex)
    {
        var securityExceptionTypes = new[]
        {
            typeof(UnauthorizedAccessException),
            typeof(System.Security.SecurityException),
            typeof(System.Security.Authentication.AuthenticationException)
        };

        return securityExceptionTypes.Contains(ex.GetType()) || 
               ex.GetType().Name.Contains("Security") ||
               ex.GetType().Name.Contains("Authentication") ||
               ex.GetType().Name.Contains("Authorization");
    }

    private string GetAuthenticationMethod(ClaimsPrincipal user)
    {
        var authMethod = user.FindFirst("auth_method")?.Value;
        if (!string.IsNullOrEmpty(authMethod))
            return authMethod;

        // Determine auth method from claims
        if (user.HasClaim("google_id", ""))
            return "Google OAuth";
        if (user.HasClaim("sub", ""))
            return "JWT";
        
        return "Unknown";
    }
}

/// <summary>
/// Extension method to register security logging middleware
/// </summary>
public static class SecurityLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityLoggingMiddleware>();
    }
}

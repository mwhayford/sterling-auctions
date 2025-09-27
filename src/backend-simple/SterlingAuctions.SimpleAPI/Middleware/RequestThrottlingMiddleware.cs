using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SterlingAuctions.SimpleAPI.Services;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace SterlingAuctions.SimpleAPI.Middleware;

/// <summary>
/// Middleware for request throttling and rate limiting
/// </summary>
public class RequestThrottlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestThrottlingMiddleware> _logger;
    private readonly ConcurrentDictionary<string, ClientRequestInfo> _clientRequests;
    private readonly Timer _cleanupTimer;

    // Rate limiting configuration
    private readonly int _maxRequestsPerMinute = 1000; // Increased for development
    private readonly int _maxRequestsPerHour = 10000; // Increased for development
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(1);

    public RequestThrottlingMiddleware(
        RequestDelegate next,
        ILogger<RequestThrottlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _clientRequests = new ConcurrentDictionary<string, ClientRequestInfo>();
        
        // Start cleanup timer
        _cleanupTimer = new Timer(CleanupExpiredEntries, null, _cleanupInterval, _cleanupInterval);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var now = DateTime.UtcNow;

        // Get or create client request info
        var clientInfo = _clientRequests.GetOrAdd(clientId, _ => new ClientRequestInfo());

        // Check rate limits
        if (IsRateLimitExceeded(clientInfo, now))
        {
            _logger.LogWarning("Rate limit exceeded for client: {ClientId}", clientId);
            
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["Retry-After"] = "60"; // Retry after 60 seconds
            
            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            return;
        }

        // Update request counts
        clientInfo.RequestsPerMinute.Add(now);
        clientInfo.RequestsPerHour.Add(now);
        clientInfo.LastRequestTime = now;

        // Add rate limit headers
        context.Response.Headers["X-RateLimit-Limit-Minute"] = _maxRequestsPerMinute.ToString();
        context.Response.Headers["X-RateLimit-Remaining-Minute"] = 
            Math.Max(0, _maxRequestsPerMinute - clientInfo.RequestsPerMinute.Count).ToString();
        context.Response.Headers["X-RateLimit-Limit-Hour"] = _maxRequestsPerHour.ToString();
        context.Response.Headers["X-RateLimit-Remaining-Hour"] = 
            Math.Max(0, _maxRequestsPerHour - clientInfo.RequestsPerHour.Count).ToString();

        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Try to get authenticated user ID first
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst("sub")?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                return $"user:{userId}";
            }
        }

        // Fall back to IP address
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ipAddress}";
    }

    private bool IsRateLimitExceeded(ClientRequestInfo clientInfo, DateTime now)
    {
        // Clean up old requests
        clientInfo.RequestsPerMinute.RemoveAll(time => now - time > TimeSpan.FromMinutes(1));
        clientInfo.RequestsPerHour.RemoveAll(time => now - time > TimeSpan.FromHours(1));

        // Check limits
        return clientInfo.RequestsPerMinute.Count >= _maxRequestsPerMinute ||
               clientInfo.RequestsPerHour.Count >= _maxRequestsPerHour;
    }

    private void CleanupExpiredEntries(object? state)
    {
        var now = DateTime.UtcNow;
        var expiredClients = new List<string>();

        foreach (var kvp in _clientRequests)
        {
            var clientInfo = kvp.Value;
            
            // Clean up old requests
            clientInfo.RequestsPerMinute.RemoveAll(time => now - time > TimeSpan.FromMinutes(1));
            clientInfo.RequestsPerHour.RemoveAll(time => now - time > TimeSpan.FromHours(1));

            // Remove clients with no recent activity
            if (now - clientInfo.LastRequestTime > TimeSpan.FromHours(1))
            {
                expiredClients.Add(kvp.Key);
            }
        }

        // Remove expired clients
        foreach (var clientId in expiredClients)
        {
            _clientRequests.TryRemove(clientId, out _);
        }

        _logger.LogDebug("Cleaned up {ExpiredCount} expired client entries", expiredClients.Count);
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}

/// <summary>
/// Client request information for rate limiting
/// </summary>
public class ClientRequestInfo
{
    public List<DateTime> RequestsPerMinute { get; set; } = new();
    public List<DateTime> RequestsPerHour { get; set; } = new();
    public DateTime LastRequestTime { get; set; } = DateTime.UtcNow;
}

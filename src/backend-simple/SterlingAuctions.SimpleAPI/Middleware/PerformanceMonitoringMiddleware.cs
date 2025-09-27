using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SterlingAuctions.SimpleAPI.Services;
using System.Diagnostics;

namespace SterlingAuctions.SimpleAPI.Middleware;

/// <summary>
/// Middleware for automatic performance monitoring
/// </summary>
public class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
    private readonly IPerformanceOptimizationService _performanceService;

    public PerformanceMonitoringMiddleware(
        RequestDelegate next,
        ILogger<PerformanceMonitoringMiddleware> logger,
        IPerformanceOptimizationService performanceService)
    {
        _next = next;
        _logger = logger;
        _performanceService = performanceService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestPath = context.Request.Path.Value ?? "";
        var requestMethod = context.Request.Method;
        var operation = $"{requestMethod} {requestPath}";

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            
            var metadata = new Dictionary<string, object>
            {
                ["StatusCode"] = context.Response.StatusCode,
                ["UserAgent"] = context.Request.Headers.UserAgent.ToString(),
                ["ContentLength"] = context.Response.ContentLength ?? 0,
                ["RequestSize"] = context.Request.ContentLength ?? 0
            };

            // Add user information if available
            if (context.User.Identity?.IsAuthenticated == true)
            {
                metadata["UserId"] = context.User.FindFirst("sub")?.Value ?? "unknown";
                metadata["UserRole"] = string.Join(",", context.User.FindAll("role").Select(c => c.Value));
            }

            // Add IP address
            metadata["ClientIP"] = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            await _performanceService.LogPerformanceMetricAsync(
                operation, 
                stopwatch.Elapsed, 
                metadata);

            // Log slow requests
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning("Slow request detected: {Operation} took {Duration}ms", 
                    operation, stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
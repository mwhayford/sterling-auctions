using Serilog;
using Serilog.Context;
using Serilog.Events;
using System.Diagnostics;

namespace SterlingAuctions.SimpleAPI.Middleware;

/// <summary>
/// Performance monitoring middleware for API requests
/// </summary>
public class PerformanceMonitoringMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceMonitoringMiddleware> _logger;

    public PerformanceMonitoringMiddleware(RequestDelegate next, ILogger<PerformanceMonitoringMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var startTime = DateTime.UtcNow;

        // Add performance tracking to the activity
        using var activity = Activity.Current?.StartActivity("HTTP Request");
        activity?.SetTag("http.method", context.Request.Method);
        activity?.SetTag("http.url", context.Request.Path);
        activity?.SetTag("stopwatch", stopwatch);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var duration = stopwatch.ElapsedMilliseconds;
            var endTime = DateTime.UtcNow;

            // Log performance metrics
            LogPerformanceMetrics(context, duration, startTime, endTime);

            // Add performance headers
            context.Response.Headers.Add("X-Response-Time", $"{duration}ms");
            context.Response.Headers.Add("X-Request-Start", startTime.ToString("O"));
            context.Response.Headers.Add("X-Request-End", endTime.ToString("O"));
        }
    }

    private void LogPerformanceMetrics(HttpContext context, long durationMs, DateTime startTime, DateTime endTime)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? "";
        var statusCode = context.Response.StatusCode;
        var userId = context.User?.Identity?.Name;
        var userAgent = context.Request.Headers.UserAgent.ToString();
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();

        using (LogContext.PushProperty("Component", "Performance"))
        using (LogContext.PushProperty("EventType", "RequestPerformance"))
        using (LogContext.PushProperty("RequestMethod", method))
        using (LogContext.PushProperty("RequestPath", path))
        using (LogContext.PushProperty("ResponseStatusCode", statusCode))
        using (LogContext.PushProperty("DurationMs", durationMs))
        using (LogContext.PushProperty("StartTime", startTime))
        using (LogContext.PushProperty("EndTime", endTime))
        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("UserAgent", userAgent))
        using (LogContext.PushProperty("IpAddress", ipAddress))
        {
            // Determine log level based on performance
            var logLevel = DetermineLogLevel(durationMs, statusCode);
            
            _logger.Write(logLevel, 
                "Request Performance: {Method} {Path} -> {StatusCode} in {DurationMs}ms by user {UserId} from {IpAddress}",
                method, path, statusCode, durationMs, userId, ipAddress);

            // Log slow requests as warnings
            if (durationMs > 1000)
            {
                _logger.Warning("Slow Request: {Method} {Path} took {DurationMs}ms (threshold: 1000ms)", 
                    method, path, durationMs);
            }

            // Log error responses
            if (statusCode >= 400)
            {
                _logger.Warning("Error Response: {Method} {Path} returned {StatusCode} in {DurationMs}ms", 
                    method, path, statusCode, durationMs);
            }
        }
    }

    private LogEventLevel DetermineLogLevel(long durationMs, int statusCode)
    {
        // Critical errors
        if (statusCode >= 500)
            return LogEventLevel.Error;
        
        // Client errors
        if (statusCode >= 400)
            return LogEventLevel.Warning;
        
        // Slow requests
        if (durationMs > 2000)
            return LogEventLevel.Warning;
        
        // Normal requests
        return LogEventLevel.Information;
    }
}

/// <summary>
/// Extension method to register performance monitoring middleware
/// </summary>
public static class PerformanceMonitoringMiddlewareExtensions
{
    public static IApplicationBuilder UsePerformanceMonitoring(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<PerformanceMonitoringMiddleware>();
    }
}

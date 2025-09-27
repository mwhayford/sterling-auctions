using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SterlingAuctions.SimpleAPI.Services;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace SterlingAuctions.SimpleAPI.Middleware;

/// <summary>
/// Middleware for connection pooling optimization
/// </summary>
public class ConnectionPoolOptimizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ConnectionPoolOptimizationMiddleware> _logger;
    private readonly IPerformanceOptimizationService _performanceService;
    private readonly ConcurrentDictionary<string, ConnectionInfo> _activeConnections;
    private readonly Timer _monitoringTimer;

    // Connection pool configuration
    private readonly int _maxConcurrentConnections = 100;
    private readonly TimeSpan _connectionTimeout = TimeSpan.FromSeconds(30);
    private readonly TimeSpan _monitoringInterval = TimeSpan.FromSeconds(10);

    public ConnectionPoolOptimizationMiddleware(
        RequestDelegate next,
        ILogger<ConnectionPoolOptimizationMiddleware> logger,
        IPerformanceOptimizationService performanceService)
    {
        _next = next;
        _logger = logger;
        _performanceService = performanceService;
        _activeConnections = new ConcurrentDictionary<string, ConnectionInfo>();
        
        // Start monitoring timer
        _monitoringTimer = new Timer(MonitorConnections, null, _monitoringInterval, _monitoringInterval);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var connectionId = context.Connection.Id;
        var startTime = DateTime.UtcNow;

        // Check if we're at connection limit
        if (_activeConnections.Count >= _maxConcurrentConnections)
        {
            _logger.LogWarning("Connection limit reached: {ActiveConnections}/{MaxConnections}", 
                _activeConnections.Count, _maxConcurrentConnections);
            
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.Headers["Retry-After"] = "5";
            
            await context.Response.WriteAsync("Service temporarily unavailable. Please try again later.");
            return;
        }

        // Register connection
        var connectionInfo = new ConnectionInfo
        {
            ConnectionId = connectionId,
            StartTime = startTime,
            RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            UserAgent = context.Request.Headers.UserAgent.ToString(),
            Path = context.Request.Path.Value ?? "",
            Method = context.Request.Method
        };

        _activeConnections.TryAdd(connectionId, connectionInfo);

        try
        {
            // Add connection metrics headers
            context.Response.Headers["X-Active-Connections"] = _activeConnections.Count.ToString();
            context.Response.Headers["X-Max-Connections"] = _maxConcurrentConnections.ToString();
            context.Response.Headers["X-Connection-Id"] = connectionId;

            await _next(context);
        }
        finally
        {
            // Unregister connection
            _activeConnections.TryRemove(connectionId, out var removedConnection);
            
            if (removedConnection != null)
            {
                var duration = DateTime.UtcNow - removedConnection.StartTime;
                
                // Log connection metrics
                await _performanceService.LogPerformanceMetricAsync(
                    "ConnectionDuration",
                    duration,
                    new Dictionary<string, object>
                    {
                        ["ConnectionId"] = connectionId,
                        ["RemoteIpAddress"] = removedConnection.RemoteIpAddress,
                        ["Path"] = removedConnection.Path,
                        ["Method"] = removedConnection.Method,
                        ["StatusCode"] = context.Response.StatusCode
                    });

                // Log long-running connections
                if (duration > TimeSpan.FromMinutes(5))
                {
                    _logger.LogWarning("Long-running connection detected: {ConnectionId} for {Duration}",
                        connectionId, duration);
                }
            }
        }
    }

    private void MonitorConnections(object? state)
    {
        var now = DateTime.UtcNow;
        var expiredConnections = new List<string>();

        foreach (var kvp in _activeConnections)
        {
            var connectionInfo = kvp.Value;
            
            // Check for expired connections
            if (now - connectionInfo.StartTime > _connectionTimeout)
            {
                expiredConnections.Add(kvp.Key);
            }
        }

        // Remove expired connections
        foreach (var connectionId in expiredConnections)
        {
            if (_activeConnections.TryRemove(connectionId, out var expiredConnection))
            {
                _logger.LogWarning("Removed expired connection: {ConnectionId} (duration: {Duration})",
                    connectionId, now - expiredConnection.StartTime);
            }
        }

        // Log connection pool status
        if (_activeConnections.Count > _maxConcurrentConnections * 0.8)
        {
            _logger.LogWarning("Connection pool utilization high: {ActiveConnections}/{MaxConnections}",
                _activeConnections.Count, _maxConcurrentConnections);
        }
    }

    public void Dispose()
    {
        _monitoringTimer?.Dispose();
    }
}

/// <summary>
/// Connection information for monitoring
/// </summary>
public class ConnectionInfo
{
    public string ConnectionId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public string RemoteIpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
}

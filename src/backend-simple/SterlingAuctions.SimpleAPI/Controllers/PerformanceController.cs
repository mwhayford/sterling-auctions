using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SterlingAuctions.SimpleAPI.Services;
using System.Diagnostics;

namespace SterlingAuctions.SimpleAPI.Controllers;

/// <summary>
/// Controller for performance optimization and monitoring
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PerformanceController : ControllerBase
{
    private readonly IPerformanceOptimizationService _performanceService;
    private readonly ILogger<PerformanceController> _logger;

    public PerformanceController(
        IPerformanceOptimizationService performanceService,
        ILogger<PerformanceController> logger)
    {
        _performanceService = performanceService;
        _logger = logger;
    }

    /// <summary>
    /// Gets current performance metrics
    /// </summary>
    /// <returns>Performance metrics</returns>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(PerformanceMetrics), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPerformanceMetrics()
    {
        try
        {
            var metrics = await _performanceService.GetPerformanceMetricsAsync();
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance metrics");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error retrieving performance metrics" });
        }
    }

    /// <summary>
    /// Gets database performance metrics
    /// </summary>
    /// <returns>Database performance metrics</returns>
    [HttpGet("database")]
    [ProducesResponseType(typeof(DatabasePerformanceMetrics), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDatabasePerformance()
    {
        try
        {
            var metrics = await _performanceService.GetDatabasePerformanceAsync();
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting database performance metrics");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error retrieving database performance metrics" });
        }
    }

    /// <summary>
    /// Gets memory metrics
    /// </summary>
    /// <returns>Memory metrics</returns>
    [HttpGet("memory")]
    [ProducesResponseType(typeof(MemoryMetrics), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMemoryMetrics()
    {
        try
        {
            var metrics = await _performanceService.GetMemoryMetricsAsync();
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting memory metrics");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error retrieving memory metrics" });
        }
    }

    /// <summary>
    /// Gets connection pool metrics
    /// </summary>
    /// <returns>Connection pool metrics</returns>
    [HttpGet("connections")]
    [ProducesResponseType(typeof(ConnectionPoolMetrics), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConnectionPoolMetrics()
    {
        try
        {
            var metrics = await _performanceService.GetConnectionPoolMetricsAsync();
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting connection pool metrics");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error retrieving connection pool metrics" });
        }
    }

    /// <summary>
    /// Checks if the system performance is healthy
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet("health")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPerformanceHealth()
    {
        try
        {
            var isHealthy = await _performanceService.IsPerformanceHealthyAsync();
            return Ok(new { 
                isHealthy, 
                timestamp = DateTime.UtcNow,
                message = isHealthy ? "System performance is healthy" : "System performance issues detected"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking performance health");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error checking performance health" });
        }
    }

    /// <summary>
    /// Optimizes database performance
    /// </summary>
    /// <returns>Optimization result</returns>
    [HttpPost("database/optimize")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> OptimizeDatabase()
    {
        try
        {
            await _performanceService.OptimizeDatabaseAsync();
            return Ok(new { 
                message = "Database optimization completed",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing database");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error optimizing database" });
        }
    }

    /// <summary>
    /// Forces garbage collection
    /// </summary>
    /// <returns>GC result</returns>
    [HttpPost("memory/gc")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForceGarbageCollection()
    {
        try
        {
            await _performanceService.ForceGarbageCollectionAsync();
            return Ok(new { 
                message = "Garbage collection completed",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error forcing garbage collection");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error forcing garbage collection" });
        }
    }

    /// <summary>
    /// Optimizes connection pools
    /// </summary>
    /// <returns>Optimization result</returns>
    [HttpPost("connections/optimize")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> OptimizeConnectionPools()
    {
        try
        {
            await _performanceService.OptimizeConnectionPoolsAsync();
            return Ok(new { 
                message = "Connection pool optimization completed",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing connection pools");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error optimizing connection pools" });
        }
    }

    /// <summary>
    /// Clears application cache
    /// </summary>
    /// <returns>Cache clear result</returns>
    [HttpPost("cache/clear")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearCache()
    {
        try
        {
            await _performanceService.ClearCacheAsync();
            return Ok(new { 
                message = "Cache cleared successfully",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error clearing cache" });
        }
    }

    /// <summary>
    /// Logs a performance metric
    /// </summary>
    /// <param name="request">Performance metric request</param>
    /// <returns>Logging result</returns>
    [HttpPost("metrics/log")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> LogPerformanceMetric([FromBody] LogPerformanceMetricRequest request)
    {
        try
        {
            await _performanceService.LogPerformanceMetricAsync(
                request.Operation, 
                request.Duration, 
                request.Metadata);
            
            return Ok(new { 
                message = "Performance metric logged successfully",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging performance metric");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error logging performance metric" });
        }
    }
}

/// <summary>
/// Request model for logging performance metrics
/// </summary>
public class LogPerformanceMetricRequest
{
    public string Operation { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

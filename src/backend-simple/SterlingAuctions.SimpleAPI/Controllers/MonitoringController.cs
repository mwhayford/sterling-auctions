using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SterlingAuctions.SimpleAPI.Services;
using System.Diagnostics;
using System.Security.Claims;

namespace SterlingAuctions.SimpleAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MonitoringController : ControllerBase
{
    private readonly ICombinedLoggingService _combinedLoggingService;
    private readonly ILogger<MonitoringController> _logger;

    public MonitoringController(
        ICombinedLoggingService combinedLoggingService,
        ILogger<MonitoringController> logger)
    {
        _combinedLoggingService = combinedLoggingService;
        _logger = logger;
    }

    private string GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
    }

    /// <summary>
    /// Test metrics collection
    /// </summary>
    [HttpPost("test-metrics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> TestMetrics()
    {
        try
        {
            var userId = GetUserId();
            var stopwatch = Stopwatch.StartNew();

            // Simulate some work
            await Task.Delay(100);

            stopwatch.Stop();

            // Record various metrics
            await _metricsService.RecordApiRequestAsync("POST", "/api/monitoring/test-metrics", 200, stopwatch.ElapsedMilliseconds);
            await _metricsService.RecordUserActionAsync(userId, "TestMetrics", new { Duration = stopwatch.ElapsedMilliseconds });
            await _metricsService.RecordPerformanceMetricAsync("TestMetricsDuration", stopwatch.ElapsedMilliseconds, "Milliseconds");

            return Ok(new { message = "Metrics recorded successfully", duration = stopwatch.ElapsedMilliseconds });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing metrics");
            return StatusCode(500, new { message = "Error testing metrics" });
        }
    }

    /// <summary>
    /// Test logging functionality
    /// </summary>
    [HttpPost("test-logging")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> TestLogging()
    {
        try
        {
            var userId = GetUserId();

            // Test different log levels
            await _combinedLoggingService.LogInfoAsync("Test info log", new { UserId = userId, TestType = "Info" });
            await _combinedLoggingService.LogWarningAsync("Test warning log", new { UserId = userId, TestType = "Warning" });
            await _combinedLoggingService.LogDebugAsync("Test debug log", new { UserId = userId, TestType = "Debug" });

            // Test structured logging
            await _combinedLoggingService.LogUserEventAsync(userId, "TestLogging", new { Timestamp = DateTime.UtcNow });
            await _combinedLoggingService.LogAuctionEventAsync(1, "TestAuctionEvent", new { TestData = "value" });
            await _combinedLoggingService.LogPaymentEventAsync(1, "TestPaymentEvent", new { TestData = "value" });
            await _combinedLoggingService.LogSecurityEventAsync("TestSecurityEvent", userId, new { TestData = "value" });
            await _combinedLoggingService.LogPerformanceEventAsync("TestPerformanceEvent", 150, new { TestData = "value" });

            return Ok(new { message = "Logging test completed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing logging");
            return StatusCode(500, new { message = "Error testing logging" });
        }
    }

    /// <summary>
    /// Test error logging
    /// </summary>
    [HttpPost("test-error-logging")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> TestErrorLogging()
    {
        try
        {
            var userId = GetUserId();

            // Test error logging
            var testException = new InvalidOperationException("This is a test exception for logging");
            await _combinedLoggingService.LogErrorAsync("Test error log", testException, new { UserId = userId, TestType = "Error" });

            // Test critical logging
            await _combinedLoggingService.LogCriticalAsync("Test critical log", testException, new { UserId = userId, TestType = "Critical" });

            // Record error metrics
            await _metricsService.RecordErrorAsync("TestError", "MonitoringController");

            return Ok(new { message = "Error logging test completed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing error logging");
            return StatusCode(500, new { message = "Error testing error logging" });
        }
    }

    /// <summary>
    /// Test auction event logging and metrics
    /// </summary>
    [HttpPost("test-auction-events")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> TestAuctionEvents()
    {
        try
        {
            var userId = GetUserId();
            var auctionId = 123;
            var bidAmount = 150.00m;

            // Log auction events
            await _combinedLoggingService.LogAuctionEventAsync(auctionId, "AuctionCreated", new { CreatedBy = userId });
            await _combinedLoggingService.LogAuctionEventAsync(auctionId, "AuctionStarted", new { StartTime = DateTime.UtcNow });
            await _combinedLoggingService.LogAuctionEventAsync(auctionId, "BidPlaced", new { BidderId = userId, Amount = bidAmount });

            // Record auction metrics
            await _metricsService.RecordAuctionEventAsync("AuctionCreated", auctionId);
            await _metricsService.RecordAuctionEventAsync("AuctionStarted", auctionId);
            await _metricsService.RecordBidEventAsync(auctionId, bidAmount);

            return Ok(new { message = "Auction events test completed successfully", auctionId, bidAmount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing auction events");
            return StatusCode(500, new { message = "Error testing auction events" });
        }
    }

    /// <summary>
    /// Test payment event logging and metrics
    /// </summary>
    [HttpPost("test-payment-events")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> TestPaymentEvents()
    {
        try
        {
            var userId = GetUserId();
            var paymentId = 456;
            var amount = 200.00m;

            // Log payment events
            await _combinedLoggingService.LogPaymentEventAsync(paymentId, "PaymentCreated", new { UserId = userId, Amount = amount });
            await _combinedLoggingService.LogPaymentEventAsync(paymentId, "PaymentProcessing", new { UserId = userId, Amount = amount });
            await _combinedLoggingService.LogPaymentEventAsync(paymentId, "PaymentCompleted", new { UserId = userId, Amount = amount });

            // Record payment metrics
            await _metricsService.RecordPaymentEventAsync(paymentId, amount, true);

            return Ok(new { message = "Payment events test completed successfully", paymentId, amount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing payment events");
            return StatusCode(500, new { message = "Error testing payment events" });
        }
    }

    /// <summary>
    /// Test security event logging
    /// </summary>
    [HttpPost("test-security-events")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> TestSecurityEvents()
    {
        try
        {
            var userId = GetUserId();

            // Log security events
            await _combinedLoggingService.LogSecurityEventAsync("LoginAttempt", userId, new { Success = true, IPAddress = "192.168.1.1" });
            await _combinedLoggingService.LogSecurityEventAsync("FailedLogin", userId, new { Success = false, IPAddress = "192.168.1.1" });
            await _combinedLoggingService.LogSecurityEventAsync("UnauthorizedAccess", userId, new { Resource = "/api/admin", Action = "GET" });

            // Record security metrics
            await _metricsService.RecordAuthenticationAsync("Password", true);
            await _metricsService.RecordAuthenticationAsync("Password", false);
            await _metricsService.RecordAuthorizationAsync("AdminPanel", "Read", false);

            return Ok(new { message = "Security events test completed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing security events");
            return StatusCode(500, new { message = "Error testing security events" });
        }
    }

    /// <summary>
    /// Test performance metrics
    /// </summary>
    [HttpPost("test-performance-metrics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> TestPerformanceMetrics()
    {
        try
        {
            var userId = GetUserId();
            var stopwatch = Stopwatch.StartNew();

            // Simulate different operations with different durations
            var operations = new[]
            {
                new { Name = "DatabaseQuery", Duration = 50 },
                new { Name = "CacheLookup", Duration = 5 },
                new { Name = "ExternalAPI", Duration = 200 },
                new { Name = "FileProcessing", Duration = 100 }
            };

            foreach (var operation in operations)
            {
                await Task.Delay(operation.Duration);
                
                await _combinedLoggingService.LogPerformanceEventAsync(operation.Name, operation.Duration, new { UserId = userId });
                await _metricsService.RecordPerformanceMetricAsync(operation.Name, operation.Duration, "Milliseconds");
            }

            stopwatch.Stop();

            return Ok(new { 
                message = "Performance metrics test completed successfully", 
                totalDuration = stopwatch.ElapsedMilliseconds,
                operations = operations
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing performance metrics");
            return StatusCode(500, new { message = "Error testing performance metrics" });
        }
    }

    /// <summary>
    /// Get monitoring status
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetMonitoringStatus()
    {
        try
        {
            var status = new
            {
                Timestamp = DateTime.UtcNow,
                UserId = GetUserId(),
                Services = new
                {
                    Metrics = "Active",
                    Logging = "Active",
                    CloudWatch = "Configured",
                    Seq = "Configured"
                },
                Environment = new
                {
                    MachineName = Environment.MachineName,
                    ProcessId = Environment.ProcessId,
                    WorkingSet = Environment.WorkingSet,
                    ProcessorCount = Environment.ProcessorCount
                }
            };

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monitoring status");
            return StatusCode(500, new { message = "Error getting monitoring status" });
        }
    }
}

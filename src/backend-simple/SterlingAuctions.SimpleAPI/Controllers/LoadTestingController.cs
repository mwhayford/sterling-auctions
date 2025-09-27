using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SterlingAuctions.SimpleAPI.Services;

namespace SterlingAuctions.SimpleAPI.Controllers;

/// <summary>
/// Controller for load testing and performance validation
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LoadTestingController : ControllerBase
{
    private readonly ILoadTestingService _loadTestingService;
    private readonly ILogger<LoadTestingController> _logger;

    public LoadTestingController(
        ILoadTestingService loadTestingService,
        ILogger<LoadTestingController> logger)
    {
        _loadTestingService = loadTestingService;
        _logger = logger;
    }

    /// <summary>
    /// Runs a load test with specified configuration
    /// </summary>
    /// <param name="config">Load test configuration</param>
    /// <returns>Load test results</returns>
    [HttpPost("load")]
    [ProducesResponseType(typeof(LoadTestResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RunLoadTest([FromBody] LoadTestConfiguration config)
    {
        try
        {
            _logger.LogInformation("Starting load test: {TestName}", config.TestName);
            
            var result = await _loadTestingService.RunLoadTestAsync(config);
            
            _logger.LogInformation("Load test completed: {TestName}, Success rate: {SuccessRate:P1}",
                config.TestName, result.SuccessRate);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running load test: {TestName}", config.TestName);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error running load test", error = ex.Message });
        }
    }

    /// <summary>
    /// Runs a stress test with specified configuration
    /// </summary>
    /// <param name="config">Stress test configuration</param>
    /// <returns>Stress test results</returns>
    [HttpPost("stress")]
    [ProducesResponseType(typeof(LoadTestResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RunStressTest([FromBody] StressTestConfiguration config)
    {
        try
        {
            _logger.LogInformation("Starting stress test: {TestName}", config.TestName);
            
            var result = await _loadTestingService.RunStressTestAsync(config);
            
            _logger.LogInformation("Stress test completed: {TestName}, Success rate: {SuccessRate:P1}",
                config.TestName, result.SuccessRate);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running stress test: {TestName}", config.TestName);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error running stress test", error = ex.Message });
        }
    }

    /// <summary>
    /// Runs a spike test with specified configuration
    /// </summary>
    /// <param name="config">Spike test configuration</param>
    /// <returns>Spike test results</returns>
    [HttpPost("spike")]
    [ProducesResponseType(typeof(LoadTestResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RunSpikeTest([FromBody] SpikeTestConfiguration config)
    {
        try
        {
            _logger.LogInformation("Starting spike test: {TestName}", config.TestName);
            
            var result = await _loadTestingService.RunSpikeTestAsync(config);
            
            _logger.LogInformation("Spike test completed: {TestName}, Success rate: {SuccessRate:P1}",
                config.TestName, result.SuccessRate);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running spike test: {TestName}", config.TestName);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error running spike test", error = ex.Message });
        }
    }

    /// <summary>
    /// Runs a volume test with specified configuration
    /// </summary>
    /// <param name="config">Volume test configuration</param>
    /// <returns>Volume test results</returns>
    [HttpPost("volume")]
    [ProducesResponseType(typeof(LoadTestResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RunVolumeTest([FromBody] VolumeTestConfiguration config)
    {
        try
        {
            _logger.LogInformation("Starting volume test: {TestName}", config.TestName);
            
            var result = await _loadTestingService.RunVolumeTestAsync(config);
            
            _logger.LogInformation("Volume test completed: {TestName}, Success rate: {SuccessRate:P1}",
                config.TestName, result.SuccessRate);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running volume test: {TestName}", config.TestName);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error running volume test", error = ex.Message });
        }
    }

    /// <summary>
    /// Validates system performance against specified criteria
    /// </summary>
    /// <param name="config">Performance validation configuration</param>
    /// <returns>Performance validation results</returns>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(PerformanceValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ValidatePerformance([FromBody] PerformanceValidationConfiguration config)
    {
        try
        {
            _logger.LogInformation("Starting performance validation");
            
            var result = await _loadTestingService.ValidatePerformanceAsync(config);
            
            _logger.LogInformation("Performance validation completed. Overall valid: {IsValid}",
                result.IsOverallValid);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating performance");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error validating performance", error = ex.Message });
        }
    }

    /// <summary>
    /// Checks if the system is ready for load testing
    /// </summary>
    /// <returns>System readiness status</returns>
    [HttpGet("readiness")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckSystemReadiness()
    {
        try
        {
            var isReady = await _loadTestingService.IsSystemReadyForLoadAsync();
            
            return Ok(new { 
                isReady, 
                timestamp = DateTime.UtcNow,
                message = isReady ? "System is ready for load testing" : "System is not ready for load testing"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking system readiness");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error checking system readiness", error = ex.Message });
        }
    }

    /// <summary>
    /// Runs a quick load test on auction endpoints
    /// </summary>
    /// <param name="concurrentUsers">Number of concurrent users</param>
    /// <param name="durationMinutes">Test duration in minutes</param>
    /// <returns>Load test results</returns>
    [HttpPost("auctions")]
    [ProducesResponseType(typeof(LoadTestResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> TestAuctionEndpoints(
        [FromQuery] int concurrentUsers = 10,
        [FromQuery] int durationMinutes = 2)
    {
        try
        {
            _logger.LogInformation("Starting auction endpoints test with {ConcurrentUsers} users for {DurationMinutes} minutes",
                concurrentUsers, durationMinutes);
            
            var result = await _loadTestingService.TestAuctionEndpointsAsync(
                concurrentUsers, 
                TimeSpan.FromMinutes(durationMinutes));
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing auction endpoints");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error testing auction endpoints", error = ex.Message });
        }
    }

    /// <summary>
    /// Runs a quick load test on authentication endpoints
    /// </summary>
    /// <param name="concurrentUsers">Number of concurrent users</param>
    /// <param name="durationMinutes">Test duration in minutes</param>
    /// <returns>Load test results</returns>
    [HttpPost("authentication")]
    [ProducesResponseType(typeof(LoadTestResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> TestAuthenticationEndpoints(
        [FromQuery] int concurrentUsers = 10,
        [FromQuery] int durationMinutes = 2)
    {
        try
        {
            _logger.LogInformation("Starting authentication endpoints test with {ConcurrentUsers} users for {DurationMinutes} minutes",
                concurrentUsers, durationMinutes);
            
            var result = await _loadTestingService.TestAuthenticationEndpointsAsync(
                concurrentUsers, 
                TimeSpan.FromMinutes(durationMinutes));
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing authentication endpoints");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error testing authentication endpoints", error = ex.Message });
        }
    }

    /// <summary>
    /// Runs a quick load test on payment endpoints
    /// </summary>
    /// <param name="concurrentUsers">Number of concurrent users</param>
    /// <param name="durationMinutes">Test duration in minutes</param>
    /// <returns>Load test results</returns>
    [HttpPost("payments")]
    [ProducesResponseType(typeof(LoadTestResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> TestPaymentEndpoints(
        [FromQuery] int concurrentUsers = 10,
        [FromQuery] int durationMinutes = 2)
    {
        try
        {
            _logger.LogInformation("Starting payment endpoints test with {ConcurrentUsers} users for {DurationMinutes} minutes",
                concurrentUsers, durationMinutes);
            
            var result = await _loadTestingService.TestPaymentEndpointsAsync(
                concurrentUsers, 
                TimeSpan.FromMinutes(durationMinutes));
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing payment endpoints");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error testing payment endpoints", error = ex.Message });
        }
    }

    /// <summary>
    /// Runs a quick load test on SignalR endpoints
    /// </summary>
    /// <param name="concurrentUsers">Number of concurrent users</param>
    /// <param name="durationMinutes">Test duration in minutes</param>
    /// <returns>Load test results</returns>
    [HttpPost("signalr")]
    [ProducesResponseType(typeof(LoadTestResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> TestSignalREndpoints(
        [FromQuery] int concurrentUsers = 10,
        [FromQuery] int durationMinutes = 2)
    {
        try
        {
            _logger.LogInformation("Starting SignalR endpoints test with {ConcurrentUsers} users for {DurationMinutes} minutes",
                concurrentUsers, durationMinutes);
            
            var result = await _loadTestingService.TestSignalREndpointsAsync(
                concurrentUsers, 
                TimeSpan.FromMinutes(durationMinutes));
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing SignalR endpoints");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error testing SignalR endpoints", error = ex.Message });
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SterlingAuctions.SimpleAPI.Middleware;
using SterlingAuctions.SimpleAPI.Services;

namespace SterlingAuctions.SimpleAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CacheController : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly ICacheKeyGenerator _keyGenerator;
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly ILogger<CacheController> _logger;

    public CacheController(
        ICacheService cacheService,
        ICacheKeyGenerator keyGenerator,
        ICacheInvalidationService cacheInvalidationService,
        ILogger<CacheController> logger)
    {
        _cacheService = cacheService;
        _keyGenerator = keyGenerator;
        _cacheInvalidationService = cacheInvalidationService;
        _logger = logger;
    }

    /// <summary>
    /// Get cache statistics
    /// </summary>
    [HttpGet("stats")]
    [AdminOnly]
    public async Task<IActionResult> GetCacheStats()
    {
        try
        {
            var stats = new
            {
                timestamp = DateTime.UtcNow,
                cacheKeys = await _cacheService.GetKeysAsync("*"),
                totalKeys = (await _cacheService.GetKeysAsync("*")).Count()
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache statistics");
            return StatusCode(500, "Error retrieving cache statistics");
        }
    }

    /// <summary>
    /// Test cache functionality
    /// </summary>
    [HttpPost("test")]
    [MemberOrAdmin]
    public async Task<IActionResult> TestCache([FromBody] CacheTestDto dto)
    {
        try
        {
            var testKey = $"test:{Guid.NewGuid()}";
            var testData = new
            {
                message = dto.Message,
                timestamp = DateTime.UtcNow,
                userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            };

            // Set cache
            await _cacheService.SetAsync(testKey, testData, TimeSpan.FromMinutes(5));

            // Get cache
            var cachedData = await _cacheService.GetAsync<object>(testKey);

            // Test exists
            var exists = await _cacheService.ExistsAsync(testKey);

            // Test increment
            var counterKey = $"counter:{Guid.NewGuid()}";
            await _cacheService.IncrementAsync(counterKey, 1);
            await _cacheService.IncrementAsync(counterKey, 2);
            // Note: Counter values are stored as strings in Redis, so we get them as strings
            var counterValue = await _cacheService.GetAsync<string>(counterKey);

            return Ok(new
            {
                testKey,
                cachedData,
                exists,
                counterKey,
                counterValue,
                message = "Cache test completed successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing cache functionality");
            return StatusCode(500, "Error testing cache functionality");
        }
    }

    /// <summary>
    /// Clear specific cache key
    /// </summary>
    [HttpDelete("key/{key}")]
    [AdminOnly]
    public async Task<IActionResult> ClearCacheKey(string key)
    {
        try
        {
            await _cacheService.RemoveAsync(key);
            _logger.LogInformation("Admin {UserId} cleared cache key: {Key}", 
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, key);

            return Ok(new { message = $"Cache key '{key}' cleared successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache key: {Key}", key);
            return StatusCode(500, "Error clearing cache key");
        }
    }

    /// <summary>
    /// Clear cache by pattern
    /// </summary>
    [HttpDelete("pattern/{pattern}")]
    [AdminOnly]
    public async Task<IActionResult> ClearCachePattern(string pattern)
    {
        try
        {
            await _cacheService.RemoveByPatternAsync(pattern);
            _logger.LogInformation("Admin {UserId} cleared cache pattern: {Pattern}", 
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, pattern);

            return Ok(new { message = $"Cache pattern '{pattern}' cleared successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache pattern: {Pattern}", pattern);
            return StatusCode(500, "Error clearing cache pattern");
        }
    }

    /// <summary>
    /// Clear all cache
    /// </summary>
    [HttpDelete("all")]
    [AdminOnly]
    public async Task<IActionResult> ClearAllCache()
    {
        try
        {
            await _cacheInvalidationService.InvalidateAllCacheAsync();
            _logger.LogInformation("Admin {UserId} cleared all cache", 
                User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);

            return Ok(new { message = "All cache cleared successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all cache");
            return StatusCode(500, "Error clearing all cache");
        }
    }

    /// <summary>
    /// Test cache invalidation
    /// </summary>
    [HttpPost("invalidate/test")]
    [AdminOnly]
    public async Task<IActionResult> TestCacheInvalidation()
    {
        try
        {
            // Create some test cache entries
            var testAuctionId = 999;
            var testUserId = "test-user-123";

            // Test auction cache invalidation
            await _cacheInvalidationService.InvalidateAuctionCacheAsync(testAuctionId);

            // Test user cache invalidation
            await _cacheInvalidationService.InvalidateUserCacheAsync(testUserId);

            // Test statistics cache invalidation
            await _cacheInvalidationService.InvalidateStatisticsCacheAsync();

            return Ok(new
            {
                message = "Cache invalidation test completed",
                invalidatedCaches = new[]
                {
                    $"auction:{testAuctionId}",
                    $"user:{testUserId}",
                    "statistics"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing cache invalidation");
            return StatusCode(500, "Error testing cache invalidation");
        }
    }

    /// <summary>
    /// Get cache keys by pattern
    /// </summary>
    [HttpGet("keys")]
    [AdminOnly]
    public async Task<IActionResult> GetCacheKeys([FromQuery] string pattern = "*")
    {
        try
        {
            var keys = await _cacheService.GetKeysAsync(pattern);
            return Ok(new
            {
                pattern,
                keys = keys.ToArray(),
                count = keys.Count()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache keys for pattern: {Pattern}", pattern);
            return StatusCode(500, "Error getting cache keys");
        }
    }
}

// DTOs
public class CacheTestDto
{
    public required string Message { get; set; }
}

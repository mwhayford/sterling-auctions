using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SterlingAuctions.SimpleAPI.Services;
using System.Text.Json;

namespace SterlingAuctions.SimpleAPI.Middleware;

/// <summary>
/// Middleware for caching optimization
/// </summary>
public class CachingOptimizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CachingOptimizationMiddleware> _logger;
    private readonly IPerformanceOptimizationService _performanceService;
    private readonly HashSet<string> _cacheablePaths;

    public CachingOptimizationMiddleware(
        RequestDelegate next,
        ILogger<CachingOptimizationMiddleware> logger,
        IPerformanceOptimizationService performanceService)
    {
        _next = next;
        _logger = logger;
        _performanceService = performanceService;
        
        // Define cacheable paths
        _cacheablePaths = new HashSet<string>
        {
            "/api/auctions",
            "/api/categories",
            "/api/auctions/search",
            "/api/auctions/statistics"
        };
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestPath = context.Request.Path.Value ?? "";
        var requestMethod = context.Request.Method;

        // Only cache GET requests for specific paths
        if (requestMethod == "GET" && _cacheablePaths.Any(path => requestPath.StartsWith(path)))
        {
            var cacheKey = GenerateCacheKey(context);
            
            try
            {
                // Try to get from cache first
                var cachedResponse = await _performanceService.GetCachedAsync<CachedResponse>(cacheKey, async () => null);
                
                if (cachedResponse != null && !IsExpired(cachedResponse))
                {
                    _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
                    
                    context.Response.StatusCode = cachedResponse.StatusCode;
                    context.Response.ContentType = cachedResponse.ContentType;
                    
                    foreach (var header in cachedResponse.Headers)
                    {
                        context.Response.Headers[header.Key] = header.Value;
                    }
                    
                    await context.Response.WriteAsync(cachedResponse.Body);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error retrieving from cache for key: {CacheKey}", cacheKey);
            }

            // Capture response
            var originalBodyStream = context.Response.Body;
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            // Cache successful responses
            if (context.Response.StatusCode == 200)
            {
                try
                {
                    responseBody.Seek(0, SeekOrigin.Begin);
                    var responseBodyText = await new StreamReader(responseBody).ReadToEndAsync();
                    
                    var cachedResponse = new CachedResponse
                    {
                        StatusCode = context.Response.StatusCode,
                        ContentType = context.Response.ContentType ?? "application/json",
                        Body = responseBodyText,
                        Headers = context.Response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                        CachedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(5) // 5 minute cache
                    };

                    await _performanceService.SetCachedAsync(cacheKey, cachedResponse, TimeSpan.FromMinutes(5));
                    _logger.LogDebug("Cached response for key: {CacheKey}", cacheKey);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error caching response for key: {CacheKey}", cacheKey);
                }
            }

            // Restore original response body
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
            context.Response.Body = originalBodyStream;
        }
        else
        {
            await _next(context);
        }
    }

    private string GenerateCacheKey(HttpContext context)
    {
        var requestPath = context.Request.Path.Value ?? "";
        var queryString = context.Request.QueryString.Value ?? "";
        var userAgent = context.Request.Headers.UserAgent.ToString();
        var acceptLanguage = context.Request.Headers.AcceptLanguage.ToString();
        
        // Include user-specific data if authenticated
        var userId = context.User.Identity?.IsAuthenticated == true 
            ? context.User.FindFirst("sub")?.Value ?? "anonymous"
            : "anonymous";

        return $"cache:{requestPath}:{queryString}:{userId}:{userAgent}:{acceptLanguage}";
    }

    private bool IsExpired(CachedResponse cachedResponse)
    {
        return DateTime.UtcNow > cachedResponse.ExpiresAt;
    }
}

/// <summary>
/// Cached response data structure
/// </summary>
public class CachedResponse
{
    public int StatusCode { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public DateTime CachedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

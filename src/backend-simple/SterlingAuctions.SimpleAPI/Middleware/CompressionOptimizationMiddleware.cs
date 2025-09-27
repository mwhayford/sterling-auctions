using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SterlingAuctions.SimpleAPI.Services;
using System.Diagnostics;

namespace SterlingAuctions.SimpleAPI.Middleware;

/// <summary>
/// Middleware for compression optimization
/// </summary>
public class CompressionOptimizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CompressionOptimizationMiddleware> _logger;
    private readonly IPerformanceOptimizationService _performanceService;

    // Minimum size for compression (bytes)
    private readonly int _minimumCompressionSize = 1024;
    
    // Compressible content types
    private readonly HashSet<string> _compressibleContentTypes = new()
    {
        "application/json",
        "application/xml",
        "text/html",
        "text/plain",
        "text/css",
        "text/javascript",
        "application/javascript",
        "application/x-javascript"
    };

    public CompressionOptimizationMiddleware(
        RequestDelegate next,
        ILogger<CompressionOptimizationMiddleware> logger,
        IPerformanceOptimizationService performanceService)
    {
        _next = next;
        _logger = logger;
        _performanceService = performanceService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check if client supports compression
        var acceptEncoding = context.Request.Headers.AcceptEncoding.ToString();
        if (!acceptEncoding.Contains("gzip") && !acceptEncoding.Contains("deflate"))
        {
            await _next(context);
            return;
        }

        // Check if response should be compressed
        if (!ShouldCompress(context))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;

        try
        {
            using var compressedStream = new MemoryStream();
            context.Response.Body = compressedStream;

            await _next(context);

            // Check if response is successful and large enough to compress
            if (context.Response.StatusCode == 200 && compressedStream.Length >= _minimumCompressionSize)
            {
                var originalSize = compressedStream.Length;
                var compressedData = await CompressData(compressedStream.ToArray());
                var compressedSize = compressedData.Length;

                // Only use compression if it provides significant savings
                if (compressedSize < originalSize * 0.9) // At least 10% reduction
                {
                    context.Response.Headers["Content-Encoding"] = "gzip";
                    context.Response.Headers["Content-Length"] = compressedSize.ToString();
                    
                    // Add compression ratio header for monitoring
                    var compressionRatio = (double)compressedSize / originalSize;
                    context.Response.Headers["X-Compression-Ratio"] = compressionRatio.ToString("F2");

                    await originalBodyStream.WriteAsync(compressedData, 0, compressedData.Length);
                    
                    _logger.LogDebug("Compressed response: {OriginalSize} -> {CompressedSize} bytes ({Ratio:P1} reduction)",
                        originalSize, compressedSize, 1 - compressionRatio);
                }
                else
                {
                    // Compression didn't provide significant savings, send original
                    compressedStream.Seek(0, SeekOrigin.Begin);
                    await compressedStream.CopyToAsync(originalBodyStream);
                }
            }
            else
            {
                // Response too small or not successful, send original
                compressedStream.Seek(0, SeekOrigin.Begin);
                await compressedStream.CopyToAsync(originalBodyStream);
            }
        }
        finally
        {
            context.Response.Body = originalBodyStream;
            stopwatch.Stop();
            
            // Log performance metric
            await _performanceService.LogPerformanceMetricAsync(
                "CompressionMiddleware", 
                stopwatch.Elapsed,
                new Dictionary<string, object>
                {
                    ["Path"] = context.Request.Path.Value ?? "",
                    ["Method"] = context.Request.Method,
                    ["StatusCode"] = context.Response.StatusCode
                });
        }
    }

    private bool ShouldCompress(HttpContext context)
    {
        // Don't compress if already compressed
        if (context.Response.Headers.ContainsKey("Content-Encoding"))
        {
            return false;
        }

        // Check content type
        var contentType = context.Response.ContentType;
        if (string.IsNullOrEmpty(contentType))
        {
            return false;
        }

        // Extract base content type (before semicolon)
        var baseContentType = contentType.Split(';')[0].Trim().ToLowerInvariant();
        
        return _compressibleContentTypes.Contains(baseContentType);
    }

    private async Task<byte[]> CompressData(byte[] data)
    {
        using var outputStream = new MemoryStream();
        using var gzipStream = new System.IO.Compression.GZipStream(outputStream, System.IO.Compression.CompressionLevel.Optimal);
        
        await gzipStream.WriteAsync(data, 0, data.Length);
        await gzipStream.FlushAsync();
        
        return outputStream.ToArray();
    }
}

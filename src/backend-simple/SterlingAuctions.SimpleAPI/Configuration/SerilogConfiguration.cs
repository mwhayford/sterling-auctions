using Serilog;
using Serilog.Events;
using Serilog.Sinks.File;
using Serilog.Formatting.Json;
using Serilog.Context;
using Serilog.Filters;
using Serilog.Core;
using Serilog.Core.Enrichers;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace SterlingAuctions.SimpleAPI.Configuration;

/// <summary>
/// Serilog configuration for Sterling Auctions
/// </summary>
public static class SerilogConfiguration
{
    /// <summary>
    /// Configure Serilog with CloudWatch, Seq, and file logging
    /// </summary>
    public static void ConfigureSerilog(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var logLevel = configuration.GetValue<string>("Serilog:MinimumLevel:Default") ?? "Information";
        var filePath = configuration.GetValue<string>("Serilog:File:Path") ?? "logs/sterling-auctions-.log";

        var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(Enum.Parse<LogEventLevel>(logLevel))
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "SterlingAuctions")
            .Enrich.WithProperty("Environment", environment.EnvironmentName)
            .Enrich.WithProperty("Version", GetApplicationVersion())
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .WriteTo.File(
                path: filePath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");

        Log.Logger = loggerConfig.CreateLogger();
    }

    /// <summary>
    /// Get application version for logging
    /// </summary>
    private static string GetApplicationVersion()
    {
        try
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version?.ToString() ?? "1.0.0";
        }
        catch
        {
            return "1.0.0";
        }
    }
}

/// <summary>
/// Default log stream provider for CloudWatch
/// </summary>
public class DefaultLogStreamProvider
{
    public string GetLogStreamName()
    {
        return $"sterling-auctions-{Environment.MachineName}-{Environment.ProcessId}";
    }
}

/// <summary>
/// HTTP context enricher for correlation ID
/// </summary>
public class CorrelationIdEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var correlationId = httpContext.TraceIdentifier;
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CorrelationId", correlationId));
            
            var userAgent = httpContext.Request.Headers.UserAgent.ToString();
            if (!string.IsNullOrEmpty(userAgent))
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserAgent", userAgent));
            }

            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(ipAddress))
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("IpAddress", ipAddress));
            }

            var userId = httpContext.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", userId));
            }
        }
    }
}

/// <summary>
/// Performance monitoring enricher
/// </summary>
public class PerformanceEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var stopwatch = Activity.Current?.GetTagItem("stopwatch") as Stopwatch;
        if (stopwatch != null)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Duration", stopwatch.ElapsedMilliseconds));
        }

        var memoryUsage = GC.GetTotalMemory(false);
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("MemoryUsage", memoryUsage));

        var threadCount = Process.GetCurrentProcess().Threads.Count;
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ThreadCount", threadCount));
    }
}

/// <summary>
/// Security event enricher
/// </summary>
public class SecurityEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = HttpContext.Current;
        if (httpContext != null)
        {
            var requestPath = httpContext.Request.Path;
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestPath", requestPath));

            var requestMethod = httpContext.Request.Method;
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestMethod", requestMethod));

            var responseStatusCode = httpContext.Response.StatusCode;
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ResponseStatusCode", responseStatusCode));
        }
    }
}

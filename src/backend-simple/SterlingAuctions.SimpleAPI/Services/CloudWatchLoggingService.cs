using Amazon.CloudWatchLogs;
using Amazon.CloudWatchLogs.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.AwsCloudWatch;
using System.Text.Json;

namespace SterlingAuctions.SimpleAPI.Services;

/// <summary>
/// CloudWatch logging service for structured logging
/// </summary>
public interface ICloudWatchLoggingService
{
    Task LogAsync(LogLevel level, string message, object data = null, Exception exception = null);
    Task LogInfoAsync(string message, object data = null);
    Task LogWarningAsync(string message, object data = null, Exception exception = null);
    Task LogErrorAsync(string message, Exception exception = null, object data = null);
    Task LogCriticalAsync(string message, Exception exception = null, object data = null);
    Task LogDebugAsync(string message, object data = null);
    Task LogTraceAsync(string message, object data = null);
}

/// <summary>
/// Implementation of CloudWatch logging service
/// </summary>
public class CloudWatchLoggingService : ICloudWatchLoggingService
{
    private readonly IAmazonCloudWatchLogs _cloudWatchLogsClient;
    private readonly ILogger<CloudWatchLoggingService> _logger;
    private readonly string _logGroupName;
    private readonly bool _enabled;

    public CloudWatchLoggingService(
        IAmazonCloudWatchLogs cloudWatchLogsClient,
        ILogger<CloudWatchLoggingService> logger,
        IConfiguration configuration)
    {
        _cloudWatchLogsClient = cloudWatchLogsClient;
        _logger = logger;
        _logGroupName = configuration.GetValue<string>("CloudWatch:LogGroup") ?? "/aws/sterling-auctions/application";
        _enabled = configuration.GetValue<bool>("CloudWatch:Enabled") && !string.IsNullOrEmpty(_logGroupName);
    }

    public async Task LogAsync(LogLevel level, string message, object data = null, Exception exception = null)
    {
        if (!_enabled)
        {
            Log.Debug("CloudWatch logging disabled, skipping log: {Level} - {Message}", level, message);
            return;
        }

        try
        {
            var logEvent = new InputLogEvent
            {
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Message = FormatLogMessage(level, message, data, exception)
            };

            var request = new PutLogEventsRequest
            {
                LogGroupName = _logGroupName,
                LogStreamName = GetLogStreamName(),
                LogEvents = new List<InputLogEvent> { logEvent }
            };

            await _cloudWatchLogsClient.PutLogEventsAsync(request);
            
            Log.Debug("Published CloudWatch log: {Level} - {Message}", level, message);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to publish CloudWatch log: {Level} - {Message}", level, message);
        }
    }

    public async Task LogInfoAsync(string message, object data = null)
    {
        await LogAsync(LogLevel.Information, message, data);
    }

    public async Task LogWarningAsync(string message, object data = null, Exception exception = null)
    {
        await LogAsync(LogLevel.Warning, message, data, exception);
    }

    public async Task LogErrorAsync(string message, Exception exception = null, object data = null)
    {
        await LogAsync(LogLevel.Error, message, data, exception);
    }

    public async Task LogCriticalAsync(string message, Exception exception = null, object data = null)
    {
        await LogAsync(LogLevel.Critical, message, data, exception);
    }

    public async Task LogDebugAsync(string message, object data = null)
    {
        await LogAsync(LogLevel.Debug, message, data);
    }

    public async Task LogTraceAsync(string message, object data = null)
    {
        await LogAsync(LogLevel.Trace, message, data);
    }

    private string FormatLogMessage(LogLevel level, string message, object data, Exception exception)
    {
        var logEntry = new
        {
            Timestamp = DateTime.UtcNow,
            Level = level.ToString(),
            Message = message,
            Data = data,
            Exception = exception != null ? new
            {
                Type = exception.GetType().Name,
                Message = exception.Message,
                StackTrace = exception.StackTrace
            } : null
        };

        return JsonSerializer.Serialize(logEntry, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    private string GetLogStreamName()
    {
        // Use a combination of date and instance identifier
        var date = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var instanceId = Environment.MachineName;
        return $"{date}/{instanceId}";
    }
}

/// <summary>
/// Serilog CloudWatch sink configuration
/// </summary>
public static class CloudWatchLoggingExtensions
{
    public static LoggerConfiguration WriteToCloudWatch(
        this LoggerConfiguration loggerConfiguration,
        IConfiguration configuration)
    {
        var logGroupName = configuration.GetValue<string>("CloudWatch:LogGroup") ?? "/aws/sterling-auctions/application";
        var region = configuration.GetValue<string>("AWS:Region") ?? "us-east-1";
        var enabled = configuration.GetValue<bool>("CloudWatch:Enabled");

        if (!enabled)
        {
            return loggerConfiguration;
        }

        return loggerConfiguration.WriteTo.AmazonCloudWatch(
            logGroup: logGroupName,
            logStreamPrefix: "sterling-auctions-",
            region: Amazon.RegionEndpoint.GetBySystemName(region),
            restrictedToMinimumLevel: LogEventLevel.Information,
            textFormatter: new Serilog.Formatting.Json.JsonFormatter(),
            logGroupRetentionPolicy: CloudWatchLogsRetentionPolicy.TwoWeeks
        );
    }
}

/// <summary>
/// Application logging service for business logic logging
/// </summary>
public interface IApplicationLoggingService
{
    Task LogUserActionAsync(string userId, string action, object data = null);
    Task LogAuctionActionAsync(int auctionId, string action, string userId = null, object data = null);
    Task LogPaymentActionAsync(int paymentId, string action, string userId = null, object data = null);
    Task LogSecurityEventAsync(string eventType, string userId = null, object data = null);
    Task LogPerformanceEventAsync(string eventType, long durationMs, object data = null);
    Task LogBusinessEventAsync(string eventType, object data = null);
}

/// <summary>
/// Implementation of application logging service
/// </summary>
public class ApplicationLoggingService : IApplicationLoggingService
{
    private readonly ICloudWatchLoggingService _cloudWatchService;
    private readonly ILogger<ApplicationLoggingService> _logger;

    public ApplicationLoggingService(ICloudWatchLoggingService cloudWatchService, ILogger<ApplicationLoggingService> logger)
    {
        _cloudWatchService = cloudWatchService;
        _logger = logger;
    }

    public async Task LogUserActionAsync(string userId, string action, object data = null)
    {
        var logData = new
        {
            UserId = userId,
            Action = action,
            Timestamp = DateTime.UtcNow,
            AdditionalData = data
        };

        await _cloudWatchService.LogInfoAsync($"User Action: {action}", logData);
    }

    public async Task LogAuctionActionAsync(int auctionId, string action, string userId = null, object data = null)
    {
        var logData = new
        {
            AuctionId = auctionId,
            UserId = userId,
            Action = action,
            Timestamp = DateTime.UtcNow,
            AdditionalData = data
        };

        await _cloudWatchService.LogInfoAsync($"Auction Action: {action}", logData);
    }

    public async Task LogPaymentActionAsync(int paymentId, string action, string userId = null, object data = null)
    {
        var logData = new
        {
            PaymentId = paymentId,
            UserId = userId,
            Action = action,
            Timestamp = DateTime.UtcNow,
            AdditionalData = data
        };

        await _cloudWatchService.LogInfoAsync($"Payment Action: {action}", logData);
    }

    public async Task LogSecurityEventAsync(string eventType, string userId = null, object data = null)
    {
        var logData = new
        {
            UserId = userId,
            EventType = eventType,
            Timestamp = DateTime.UtcNow,
            AdditionalData = data
        };

        await _cloudWatchService.LogWarningAsync($"Security Event: {eventType}", logData);
    }

    public async Task LogPerformanceEventAsync(string eventType, long durationMs, object data = null)
    {
        var logData = new
        {
            EventType = eventType,
            DurationMs = durationMs,
            Timestamp = DateTime.UtcNow,
            AdditionalData = data
        };

        await _cloudWatchService.LogInfoAsync($"Performance Event: {eventType}", logData);
    }

    public async Task LogBusinessEventAsync(string eventType, object data = null)
    {
        var logData = new
        {
            EventType = eventType,
            Timestamp = DateTime.UtcNow,
            AdditionalData = data
        };

        await _cloudWatchService.LogInfoAsync($"Business Event: {eventType}", logData);
    }
}

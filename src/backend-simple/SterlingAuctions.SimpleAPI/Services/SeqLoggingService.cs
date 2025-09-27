using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Seq;
using System.Text.Json;

namespace SterlingAuctions.SimpleAPI.Services;

/// <summary>
/// Seq logging service for structured logging and log analysis
/// </summary>
public interface ISeqLoggingService
{
    Task LogAsync(LogLevel level, string message, object data = null, Exception exception = null);
    Task LogInfoAsync(string message, object data = null);
    Task LogWarningAsync(string message, object data = null, Exception exception = null);
    Task LogErrorAsync(string message, Exception exception = null, object data = null);
    Task LogCriticalAsync(string message, Exception exception = null, object data = null);
    Task LogDebugAsync(string message, object data = null);
    Task LogTraceAsync(string message, object data = null);
    Task LogUserEventAsync(string userId, string eventType, object data = null);
    Task LogAuctionEventAsync(int auctionId, string eventType, object data = null);
    Task LogPaymentEventAsync(int paymentId, string eventType, object data = null);
    Task LogSecurityEventAsync(string eventType, string userId = null, object data = null);
    Task LogPerformanceEventAsync(string eventType, long durationMs, object data = null);
}

/// <summary>
/// Implementation of Seq logging service
/// </summary>
public class SeqLoggingService : ISeqLoggingService
{
    private readonly ILogger<SeqLoggingService> _logger;
    private readonly string _seqUrl;
    private readonly string _apiKey;
    private readonly bool _enabled;

    public SeqLoggingService(ILogger<SeqLoggingService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _seqUrl = configuration.GetValue<string>("Seq:Url") ?? "http://localhost:5341";
        _apiKey = configuration.GetValue<string>("Seq:ApiKey") ?? "";
        _enabled = configuration.GetValue<bool>("Seq:Enabled") && !string.IsNullOrEmpty(_seqUrl);
    }

    public async Task LogAsync(LogLevel level, string message, object data = null, Exception exception = null)
    {
        if (!_enabled)
        {
            Log.Debug("Seq logging disabled, skipping log: {Level} - {Message}", level, message);
            return;
        }

        try
        {
            var logEvent = new
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

            // Use Serilog to send to Seq
            Log.Write(ConvertToSerilogLevel(level), exception, "{Message} {@Data}", message, logEvent);
            
            Log.Debug("Published Seq log: {Level} - {Message}", level, message);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to publish Seq log: {Level} - {Message}", level, message);
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

    public async Task LogUserEventAsync(string userId, string eventType, object data = null)
    {
        var logData = new
        {
            UserId = userId,
            EventType = eventType,
            Timestamp = DateTime.UtcNow,
            AdditionalData = data
        };

        await LogInfoAsync($"User Event: {eventType}", logData);
    }

    public async Task LogAuctionEventAsync(int auctionId, string eventType, object data = null)
    {
        var logData = new
        {
            AuctionId = auctionId,
            EventType = eventType,
            Timestamp = DateTime.UtcNow,
            AdditionalData = data
        };

        await LogInfoAsync($"Auction Event: {eventType}", logData);
    }

    public async Task LogPaymentEventAsync(int paymentId, string eventType, object data = null)
    {
        var logData = new
        {
            PaymentId = paymentId,
            EventType = eventType,
            Timestamp = DateTime.UtcNow,
            AdditionalData = data
        };

        await LogInfoAsync($"Payment Event: {eventType}", logData);
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

        await LogWarningAsync($"Security Event: {eventType}", logData);
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

        await LogInfoAsync($"Performance Event: {eventType}", logData);
    }

    private LogEventLevel ConvertToSerilogLevel(LogLevel level)
    {
        return level switch
        {
            LogLevel.Trace => LogEventLevel.Verbose,
            LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Information => LogEventLevel.Information,
            LogLevel.Warning => LogEventLevel.Warning,
            LogLevel.Error => LogEventLevel.Error,
            LogLevel.Critical => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }
}

/// <summary>
/// Serilog Seq sink configuration
/// </summary>
public static class SeqLoggingExtensions
{
    public static LoggerConfiguration WriteToSeq(
        this LoggerConfiguration loggerConfiguration,
        IConfiguration configuration)
    {
        var seqUrl = configuration.GetValue<string>("Seq:Url") ?? "http://localhost:5341";
        var apiKey = configuration.GetValue<string>("Seq:ApiKey") ?? "";
        var enabled = configuration.GetValue<bool>("Seq:Enabled");

        if (!enabled)
        {
            return loggerConfiguration;
        }

        var seqConfig = loggerConfiguration.WriteTo.Seq(
            serverUrl: seqUrl,
            apiKey: string.IsNullOrEmpty(apiKey) ? null : apiKey,
            restrictedToMinimumLevel: LogEventLevel.Information,
            bufferBaseFilename: "logs/seq-buffer",
            period: TimeSpan.FromSeconds(5),
            batchPostingLimit: 100,
            eventBodyLimitBytes: 256 * 1024,
            controlLevelSwitch: new Serilog.Core.LoggingLevelSwitch(LogEventLevel.Information)
        );

        return seqConfig;
    }
}

/// <summary>
/// Combined logging service that uses both CloudWatch and Seq
/// </summary>
public interface ICombinedLoggingService
{
    Task LogAsync(LogLevel level, string message, object data = null, Exception exception = null);
    Task LogInfoAsync(string message, object data = null);
    Task LogWarningAsync(string message, object data = null, Exception exception = null);
    Task LogErrorAsync(string message, Exception exception = null, object data = null);
    Task LogCriticalAsync(string message, Exception exception = null, object data = null);
    Task LogDebugAsync(string message, object data = null);
    Task LogTraceAsync(string message, object data = null);
    Task LogUserEventAsync(string userId, string eventType, object data = null);
    Task LogAuctionEventAsync(int auctionId, string eventType, object data = null);
    Task LogPaymentEventAsync(int paymentId, string eventType, object data = null);
    Task LogSecurityEventAsync(string eventType, string userId = null, object data = null);
    Task LogPerformanceEventAsync(string eventType, long durationMs, object data = null);
}

/// <summary>
/// Implementation of combined logging service
/// </summary>
public class CombinedLoggingService : ICombinedLoggingService
{
    private readonly ICloudWatchLoggingService _cloudWatchService;
    private readonly ISeqLoggingService _seqService;
    private readonly ILogger<CombinedLoggingService> _logger;

    public CombinedLoggingService(
        ICloudWatchLoggingService cloudWatchService,
        ISeqLoggingService seqService,
        ILogger<CombinedLoggingService> logger)
    {
        _cloudWatchService = cloudWatchService;
        _seqService = seqService;
        _logger = logger;
    }

    public async Task LogAsync(LogLevel level, string message, object data = null, Exception exception = null)
    {
        var tasks = new List<Task>
        {
            _cloudWatchService.LogAsync(level, message, data, exception),
            _seqService.LogAsync(level, message, data, exception)
        };

        await Task.WhenAll(tasks);
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

    public async Task LogUserEventAsync(string userId, string eventType, object data = null)
    {
        var tasks = new List<Task>
        {
            _cloudWatchService.LogInfoAsync($"User Event: {eventType}", new { UserId = userId, EventType = eventType, AdditionalData = data }),
            _seqService.LogUserEventAsync(userId, eventType, data)
        };

        await Task.WhenAll(tasks);
    }

    public async Task LogAuctionEventAsync(int auctionId, string eventType, object data = null)
    {
        var tasks = new List<Task>
        {
            _cloudWatchService.LogInfoAsync($"Auction Event: {eventType}", new { AuctionId = auctionId, EventType = eventType, AdditionalData = data }),
            _seqService.LogAuctionEventAsync(auctionId, eventType, data)
        };

        await Task.WhenAll(tasks);
    }

    public async Task LogPaymentEventAsync(int paymentId, string eventType, object data = null)
    {
        var tasks = new List<Task>
        {
            _cloudWatchService.LogInfoAsync($"Payment Event: {eventType}", new { PaymentId = paymentId, EventType = eventType, AdditionalData = data }),
            _seqService.LogPaymentEventAsync(paymentId, eventType, data)
        };

        await Task.WhenAll(tasks);
    }

    public async Task LogSecurityEventAsync(string eventType, string userId = null, object data = null)
    {
        var tasks = new List<Task>
        {
            _cloudWatchService.LogWarningAsync($"Security Event: {eventType}", new { UserId = userId, EventType = eventType, AdditionalData = data }),
            _seqService.LogSecurityEventAsync(eventType, userId, data)
        };

        await Task.WhenAll(tasks);
    }

    public async Task LogPerformanceEventAsync(string eventType, long durationMs, object data = null)
    {
        var tasks = new List<Task>
        {
            _cloudWatchService.LogInfoAsync($"Performance Event: {eventType}", new { EventType = eventType, DurationMs = durationMs, AdditionalData = data }),
            _seqService.LogPerformanceEventAsync(eventType, durationMs, data)
        };

        await Task.WhenAll(tasks);
    }
}

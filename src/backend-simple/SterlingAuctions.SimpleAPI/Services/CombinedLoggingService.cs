using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;

namespace SterlingAuctions.SimpleAPI.Services;

/// <summary>
/// Combined logging service interface for Sterling Auctions
/// </summary>
public interface ICombinedLoggingService
{
    // Basic logging methods
    Task LogInfoAsync(string message, object? data = null);
    Task LogWarningAsync(string message, object? data = null);
    Task LogErrorAsync(string message, Exception? exception = null, object? data = null);
    Task LogDebugAsync(string message, object? data = null);
    Task LogCriticalAsync(string message, Exception? exception = null, object? data = null);

    // Business event logging
    Task LogUserEventAsync(string userId, string eventType, object? data = null);
    Task LogAuctionEventAsync(int auctionId, string eventType, object? data = null);
    Task LogPaymentEventAsync(int paymentId, string eventType, object? data = null);
    Task LogSecurityEventAsync(string eventType, string? userId = null, object? data = null);
    Task LogPerformanceEventAsync(string eventType, double value, object? data = null);
}

/// <summary>
/// Combined logging service implementation
/// </summary>
public class CombinedLoggingService : ICombinedLoggingService
{
    private readonly ILogger<CombinedLoggingService> _logger;
    private readonly Serilog.ILogger _serilogLogger;

    public CombinedLoggingService(ILogger<CombinedLoggingService> logger)
    {
        _logger = logger;
        _serilogLogger = Log.Logger;
    }

    public async Task LogInfoAsync(string message, object? data = null)
    {
        using (LogContext.PushProperty("Data", data))
        {
            _logger.LogInformation(message);
            _serilogLogger.Information(message);
        }
        await Task.CompletedTask;
    }

    public async Task LogWarningAsync(string message, object? data = null)
    {
        using (LogContext.PushProperty("Data", data))
        {
            _logger.LogWarning(message);
            _serilogLogger.Warning(message);
        }
        await Task.CompletedTask;
    }

    public async Task LogErrorAsync(string message, Exception? exception = null, object? data = null)
    {
        using (LogContext.PushProperty("Data", data))
        {
            if (exception != null)
            {
                _logger.LogError(exception, message);
                _serilogLogger.Error(exception, message);
            }
            else
            {
                _logger.LogError(message);
                _serilogLogger.Error(message);
            }
        }
        await Task.CompletedTask;
    }

    public async Task LogDebugAsync(string message, object? data = null)
    {
        using (LogContext.PushProperty("Data", data))
        {
            _logger.LogDebug(message);
            _serilogLogger.Debug(message);
        }
        await Task.CompletedTask;
    }

    public async Task LogCriticalAsync(string message, Exception? exception = null, object? data = null)
    {
        using (LogContext.PushProperty("Data", data))
        {
            if (exception != null)
            {
                _logger.LogCritical(exception, message);
                _serilogLogger.Fatal(exception, message);
            }
            else
            {
                _logger.LogCritical(message);
                _serilogLogger.Fatal(message);
            }
        }
        await Task.CompletedTask;
    }

    public async Task LogUserEventAsync(string userId, string eventType, object? data = null)
    {
        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("EventType", eventType))
        using (LogContext.PushProperty("Data", data))
        {
            var message = $"User Event: {eventType} for User {userId}";
            _logger.LogInformation(message);
            _serilogLogger.Information(message);
        }
        await Task.CompletedTask;
    }

    public async Task LogAuctionEventAsync(int auctionId, string eventType, object? data = null)
    {
        using (LogContext.PushProperty("AuctionId", auctionId))
        using (LogContext.PushProperty("EventType", eventType))
        using (LogContext.PushProperty("Data", data))
        {
            var message = $"Auction Event: {eventType} for Auction {auctionId}";
            _logger.LogInformation(message);
            _serilogLogger.Information(message);
        }
        await Task.CompletedTask;
    }

    public async Task LogPaymentEventAsync(int paymentId, string eventType, object? data = null)
    {
        using (LogContext.PushProperty("PaymentId", paymentId))
        using (LogContext.PushProperty("EventType", eventType))
        using (LogContext.PushProperty("Data", data))
        {
            var message = $"Payment Event: {eventType} for Payment {paymentId}";
            _logger.LogInformation(message);
            _serilogLogger.Information(message);
        }
        await Task.CompletedTask;
    }

    public async Task LogSecurityEventAsync(string eventType, string? userId = null, object? data = null)
    {
        using (LogContext.PushProperty("UserId", userId))
        using (LogContext.PushProperty("EventType", eventType))
        using (LogContext.PushProperty("Data", data))
        {
            var message = $"Security Event: {eventType}";
            if (!string.IsNullOrEmpty(userId))
            {
                message += $" for User {userId}";
            }
            _logger.LogWarning(message);
            _serilogLogger.Warning(message);
        }
        await Task.CompletedTask;
    }

    public async Task LogPerformanceEventAsync(string eventType, double value, object? data = null)
    {
        using (LogContext.PushProperty("EventType", eventType))
        using (LogContext.PushProperty("Value", value))
        using (LogContext.PushProperty("Data", data))
        {
            var message = $"Performance Event: {eventType} = {value}";
            _logger.LogInformation(message);
            _serilogLogger.Information(message);
        }
        await Task.CompletedTask;
    }
}

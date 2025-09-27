using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace SterlingAuctions.SimpleAPI.Services;

public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly ILogger<RedisHealthCheck> _logger;

    public RedisHealthCheck(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisHealthCheck> logger)
    {
        _connectionMultiplexer = connectionMultiplexer;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var database = _connectionMultiplexer.GetDatabase();
            var pingResult = await database.PingAsync();
            
            var isConnected = _connectionMultiplexer.IsConnected;
            var serverCount = _connectionMultiplexer.GetEndPoints().Length;
            
            var data = new Dictionary<string, object>
            {
                { "ping", pingResult.TotalMilliseconds },
                { "isConnected", isConnected },
                { "serverCount", serverCount },
                { "endpoints", _connectionMultiplexer.GetEndPoints().Select(e => e.ToString()).ToArray() }
            };

            if (isConnected && pingResult.TotalMilliseconds < 1000)
            {
                return HealthCheckResult.Healthy("Redis is healthy", data);
            }
            else if (isConnected)
            {
                return HealthCheckResult.Degraded("Redis is connected but slow");
            }
            else
            {
                return HealthCheckResult.Unhealthy("Redis is not connected");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed");
            return HealthCheckResult.Unhealthy("Redis health check failed", ex);
        }
    }
}
namespace SterlingAuctions.SimpleAPI.Configuration;

/// <summary>
/// Connection manager configuration settings
/// </summary>
public class ConnectionManagerSettings
{
    public int HealthCheckInterval { get; set; } = 30000; // 30 seconds
    public int MaxFailedAttempts { get; set; } = 3;
    public int LatencyThreshold { get; set; } = 1000; // 1 second
    public int HeartbeatTimeout { get; set; } = 60000; // 1 minute
    public bool AutoSwitchTransport { get; set; } = true;
    public bool AutoReconnect { get; set; } = true;
    public string[] PreferredTransports { get; set; } = { "websockets", "sse", "longpolling", "polling" };
    public int MaxConnectionsPerUser { get; set; } = 5;
    public int ConnectionCleanupInterval { get; set; } = 300000; // 5 minutes
    public bool EnableConnectionLogging { get; set; } = true;
    public bool EnablePerformanceMetrics { get; set; } = true;
}

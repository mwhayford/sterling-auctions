using System.ComponentModel.DataAnnotations;

namespace SterlingAuctions.SimpleAPI.Models;

/// <summary>
/// SignalR connection performance metrics
/// </summary>
public class SignalRConnectionMetrics
{
    public int Id { get; set; }
    
    [Required]
    public string ConnectionId { get; set; } = string.Empty;
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public string Transport { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string ClientIp { get; set; } = string.Empty;
    
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DisconnectedAt { get; set; }
    
    public int MessagesSent { get; set; } = 0;
    public int MessagesReceived { get; set; } = 0;
    public int BytesSent { get; set; } = 0;
    public int BytesReceived { get; set; } = 0;
    
    public double AverageLatency { get; set; } = 0;
    public int MaxLatency { get; set; } = 0;
    public int MinLatency { get; set; } = int.MaxValue;
    
    public int ReconnectCount { get; set; } = 0;
    public int ErrorCount { get; set; } = 0;
    public int HeartbeatCount { get; set; } = 0;
    
    public DateTime LastHeartbeat { get; set; } = DateTime.UtcNow;
    public DateTime LastMessageSent { get; set; } = DateTime.UtcNow;
    public DateTime LastMessageReceived { get; set; } = DateTime.UtcNow;
    
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ApplicationUser? User { get; set; }
}

/// <summary>
/// SignalR message performance metrics
/// </summary>
public class SignalRMessageMetrics
{
    public int Id { get; set; }
    
    [Required]
    public string ConnectionId { get; set; } = string.Empty;
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string MessageType { get; set; } = string.Empty;
    
    [Required]
    public string MessageName { get; set; } = string.Empty;
    
    public string Direction { get; set; } = string.Empty; // "inbound" or "outbound"
    public int MessageSize { get; set; } = 0;
    public int ProcessingTime { get; set; } = 0; // milliseconds
    public int Latency { get; set; } = 0; // milliseconds
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }
    
    // Navigation properties
    public ApplicationUser? User { get; set; }
}

/// <summary>
/// SignalR hub performance metrics
/// </summary>
public class SignalRHubMetrics
{
    public int Id { get; set; }
    
    [Required]
    public string HubName { get; set; } = string.Empty;
    
    public int ActiveConnections { get; set; } = 0;
    public int TotalConnections { get; set; } = 0;
    public int MessagesPerSecond { get; set; } = 0;
    public int BytesPerSecond { get; set; } = 0;
    
    public double AverageLatency { get; set; } = 0;
    public int MaxLatency { get; set; } = 0;
    public int MinLatency { get; set; } = int.MaxValue;
    
    public int ErrorRate { get; set; } = 0; // percentage
    public int ReconnectRate { get; set; } = 0; // percentage
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// SignalR performance summary DTO
/// </summary>
public class SignalRPerformanceSummary
{
    public int TotalConnections { get; set; }
    public int ActiveConnections { get; set; }
    public int DisconnectedConnections { get; set; }
    public int TotalMessages { get; set; }
    public int MessagesPerSecond { get; set; }
    public int TotalBytes { get; set; }
    public int BytesPerSecond { get; set; }
    public double AverageLatency { get; set; }
    public int MaxLatency { get; set; }
    public int MinLatency { get; set; }
    public int TotalErrors { get; set; }
    public int TotalReconnects { get; set; }
    public double ErrorRate { get; set; }
    public double ReconnectRate { get; set; }
    public Dictionary<string, int> ConnectionsByTransport { get; set; } = new();
    public Dictionary<string, int> MessagesByType { get; set; } = new();
    public Dictionary<string, int> ErrorsByType { get; set; } = new();
    public List<SignalRConnectionMetrics> TopConnections { get; set; } = new();
    public List<SignalRHubMetrics> HubMetrics { get; set; } = new();
}

/// <summary>
/// SignalR connection health status
/// </summary>
public class SignalRConnectionHealth
{
    public string ConnectionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Transport { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public int Latency { get; set; }
    public DateTime LastHeartbeat { get; set; }
    public int MessageCount { get; set; }
    public int ErrorCount { get; set; }
    public string HealthStatus { get; set; } = string.Empty; // "healthy", "warning", "critical"
    public List<string> Issues { get; set; } = new();
}

/// <summary>
/// SignalR performance alert
/// </summary>
public class SignalRPerformanceAlert
{
    public int Id { get; set; }
    
    [Required]
    public string AlertType { get; set; } = string.Empty;
    
    [Required]
    public string Severity { get; set; } = string.Empty; // "low", "medium", "high", "critical"
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    public string? ConnectionId { get; set; }
    public string? UserId { get; set; }
    public string? HubName { get; set; }
    
    public Dictionary<string, object> Metrics { get; set; } = new();
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public bool IsResolved { get; set; } = false;
    
    public string? ResolutionNotes { get; set; }
}

/// <summary>
/// SignalR performance configuration
/// </summary>
public class SignalRPerformanceConfig
{
    public int LatencyWarningThreshold { get; set; } = 500; // milliseconds
    public int LatencyCriticalThreshold { get; set; } = 1000; // milliseconds
    public int ErrorRateWarningThreshold { get; set; } = 5; // percentage
    public int ErrorRateCriticalThreshold { get; set; } = 10; // percentage
    public int ReconnectRateWarningThreshold { get; set; } = 10; // percentage
    public int ReconnectRateCriticalThreshold { get; set; } = 20; // percentage
    public int HeartbeatTimeout { get; set; } = 60000; // milliseconds
    public int MessageSizeWarningThreshold { get; set; } = 1024 * 1024; // 1MB
    public int MessageSizeCriticalThreshold { get; set; } = 5 * 1024 * 1024; // 5MB
    public int MaxConnectionsPerUser { get; set; } = 5;
    public int MetricsRetentionDays { get; set; } = 30;
    public bool EnableRealTimeMonitoring { get; set; } = true;
    public bool EnablePerformanceAlerts { get; set; } = true;
    public int AlertCooldownMinutes { get; set; } = 5;
}

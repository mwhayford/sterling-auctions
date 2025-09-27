using System.ComponentModel.DataAnnotations;

namespace SterlingAuctions.SimpleAPI.Models;

/// <summary>
/// SignalR load test configuration
/// </summary>
public class SignalRLoadTestConfig
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    public LoadTestType TestType { get; set; } = LoadTestType.Load;
    public LoadTestScenario Scenario { get; set; } = LoadTestScenario.AuctionBidding;
    
    public int ConcurrentUsers { get; set; } = 100;
    public int DurationMinutes { get; set; } = 10;
    public int RampUpMinutes { get; set; } = 2;
    public int RampDownMinutes { get; set; } = 2;
    
    public int MessagesPerSecond { get; set; } = 10;
    public int MessageSizeBytes { get; set; } = 1024;
    public int HeartbeatIntervalSeconds { get; set; } = 30;
    
    public string HubUrl { get; set; } = string.Empty;
    public string[] TransportTypes { get; set; } = { "websockets", "sse", "longpolling" };
    
    public bool EnablePerformanceMonitoring { get; set; } = true;
    public bool EnableErrorTracking { get; set; } = true;
    public bool EnableLatencyTracking { get; set; } = true;
    
    public Dictionary<string, object> CustomParameters { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastRunAt { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// SignalR load test execution
/// </summary>
public class SignalRLoadTestExecution
{
    public int Id { get; set; }
    
    [Required]
    public int ConfigId { get; set; }
    
    [Required]
    public string ExecutionId { get; set; } = string.Empty;
    
    public LoadTestStatus Status { get; set; } = LoadTestStatus.Pending;
    public LoadTestType TestType { get; set; } = LoadTestType.Load;
    
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    
    public int PlannedUsers { get; set; } = 0;
    public int ActualUsers { get; set; } = 0;
    public int SuccessfulConnections { get; set; } = 0;
    public int FailedConnections { get; set; } = 0;
    
    public int TotalMessages { get; set; } = 0;
    public int SuccessfulMessages { get; set; } = 0;
    public int FailedMessages { get; set; } = 0;
    
    public double AverageLatency { get; set; } = 0;
    public int MinLatency { get; set; } = int.MaxValue;
    public int MaxLatency { get; set; } = 0;
    public int P95Latency { get; set; } = 0;
    public int P99Latency { get; set; } = 0;
    
    public double MessagesPerSecond { get; set; } = 0;
    public double BytesPerSecond { get; set; } = 0;
    public double ErrorRate { get; set; } = 0;
    public double ConnectionSuccessRate { get; set; } = 0;
    
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
    
    // Navigation properties
    public SignalRLoadTestConfig? Config { get; set; }
}

/// <summary>
/// SignalR load test result
/// </summary>
public class SignalRLoadTestResult
{
    public int Id { get; set; }
    
    [Required]
    public int ExecutionId { get; set; }
    
    [Required]
    public string ConnectionId { get; set; } = string.Empty;
    
    public string UserId { get; set; } = string.Empty;
    public string Transport { get; set; } = string.Empty;
    
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DisconnectedAt { get; set; }
    
    public int MessagesSent { get; set; } = 0;
    public int MessagesReceived { get; set; } = 0;
    public int MessagesFailed { get; set; } = 0;
    
    public double AverageLatency { get; set; } = 0;
    public int MinLatency { get; set; } = int.MaxValue;
    public int MaxLatency { get; set; } = 0;
    
    public int Errors { get; set; } = 0;
    public int Reconnects { get; set; } = 0;
    
    public bool IsSuccessful { get; set; } = true;
    public string? ErrorMessage { get; set; }
    
    public Dictionary<string, object> CustomMetrics { get; set; } = new();
    
    // Navigation properties
    public SignalRLoadTestExecution? Execution { get; set; }
}

/// <summary>
/// Load test types
/// </summary>
public enum LoadTestType
{
    Load = 0,           // Normal expected load
    Stress = 1,         // Beyond normal capacity
    Spike = 2,          // Sudden load increase
    Volume = 3,         // Large amount of data
    Endurance = 4,      // Extended duration
    Scalability = 5     // Scaling behavior
}

/// <summary>
/// Load test scenarios
/// </summary>
public enum LoadTestScenario
{
    AuctionBidding = 0,
    AuctionWatching = 1,
    ChatMessaging = 2,
    Notifications = 3,
    MixedWorkload = 4,
    ConnectionStress = 5,
    MessageFlood = 6,
    HeartbeatTest = 7
}

/// <summary>
/// Load test status
/// </summary>
public enum LoadTestStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4,
    Paused = 5
}

/// <summary>
/// Load test summary DTO
/// </summary>
public class LoadTestSummaryDto
{
    public int TotalExecutions { get; set; }
    public int SuccessfulExecutions { get; set; }
    public int FailedExecutions { get; set; }
    public int CancelledExecutions { get; set; }
    
    public double AverageLatency { get; set; }
    public int MaxLatency { get; set; }
    public int MinLatency { get; set; }
    
    public double AverageMessagesPerSecond { get; set; }
    public double AverageErrorRate { get; set; }
    public double AverageConnectionSuccessRate { get; set; }
    
    public Dictionary<string, int> ExecutionsByType { get; set; } = new();
    public Dictionary<string, int> ExecutionsByScenario { get; set; } = new();
    public Dictionary<string, int> ExecutionsByStatus { get; set; } = new();
    
    public List<SignalRLoadTestExecution> RecentExecutions { get; set; } = new();
}

/// <summary>
/// Load test execution request DTO
/// </summary>
public class LoadTestExecutionRequestDto
{
    [Required]
    public int ConfigId { get; set; }
    
    public string? CustomName { get; set; }
    public Dictionary<string, object>? OverrideParameters { get; set; }
}

/// <summary>
/// Load test configuration request DTO
/// </summary>
public class LoadTestConfigRequestDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public LoadTestType TestType { get; set; }
    
    [Required]
    public LoadTestScenario Scenario { get; set; }
    
    [Range(1, 10000)]
    public int ConcurrentUsers { get; set; } = 100;
    
    [Range(1, 1440)]
    public int DurationMinutes { get; set; } = 10;
    
    [Range(0, 60)]
    public int RampUpMinutes { get; set; } = 2;
    
    [Range(0, 60)]
    public int RampDownMinutes { get; set; } = 2;
    
    [Range(1, 1000)]
    public int MessagesPerSecond { get; set; } = 10;
    
    [Range(1, 10485760)]
    public int MessageSizeBytes { get; set; } = 1024;
    
    [Range(1, 300)]
    public int HeartbeatIntervalSeconds { get; set; } = 30;
    
    [Required]
    [MaxLength(500)]
    public string HubUrl { get; set; } = string.Empty;
    
    public string[] TransportTypes { get; set; } = { "websockets", "sse", "longpolling" };
    
    public bool EnablePerformanceMonitoring { get; set; } = true;
    public bool EnableErrorTracking { get; set; } = true;
    public bool EnableLatencyTracking { get; set; } = true;
    
    public Dictionary<string, object> CustomParameters { get; set; } = new();
}

/// <summary>
/// Load test execution status DTO
/// </summary>
public class LoadTestExecutionStatusDto
{
    public int Id { get; set; }
    public string ExecutionId { get; set; } = string.Empty;
    public LoadTestStatus Status { get; set; }
    public LoadTestType TestType { get; set; }
    
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? Duration { get; set; }
    
    public int PlannedUsers { get; set; }
    public int ActualUsers { get; set; }
    public int SuccessfulConnections { get; set; }
    public int FailedConnections { get; set; }
    
    public int TotalMessages { get; set; }
    public int SuccessfulMessages { get; set; }
    public int FailedMessages { get; set; }
    
    public double AverageLatency { get; set; }
    public int MinLatency { get; set; }
    public int MaxLatency { get; set; }
    
    public double MessagesPerSecond { get; set; }
    public double BytesPerSecond { get; set; }
    public double ErrorRate { get; set; }
    public double ConnectionSuccessRate { get; set; }
    
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
}

/// <summary>
/// Load test real-time metrics DTO
/// </summary>
public class LoadTestRealTimeMetricsDto
{
    public string ExecutionId { get; set; } = string.Empty;
    public LoadTestStatus Status { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    public int ActiveConnections { get; set; }
    public int TotalConnections { get; set; }
    public int FailedConnections { get; set; }
    
    public int MessagesInLastSecond { get; set; }
    public int MessagesInLastMinute { get; set; }
    public int TotalMessages { get; set; }
    
    public double CurrentLatency { get; set; }
    public double AverageLatency { get; set; }
    public int MaxLatency { get; set; }
    
    public double CurrentErrorRate { get; set; }
    public double AverageErrorRate { get; set; }
    
    public Dictionary<string, int> ConnectionsByTransport { get; set; } = new();
    public Dictionary<string, int> MessagesByType { get; set; } = new();
    public Dictionary<string, int> ErrorsByType { get; set; } = new();
}

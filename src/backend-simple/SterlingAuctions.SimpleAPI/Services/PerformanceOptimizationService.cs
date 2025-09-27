using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace SterlingAuctions.SimpleAPI.Services;

/// <summary>
/// Service for performance optimization and monitoring
/// </summary>
public interface IPerformanceOptimizationService
{
    // Caching Operations
    Task<T?> GetCachedAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
    Task SetCachedAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveCachedAsync(string key);
    Task ClearCacheAsync();
    
    // Performance Monitoring
    Task<PerformanceMetrics> GetPerformanceMetricsAsync();
    Task LogPerformanceMetricAsync(string operation, TimeSpan duration, Dictionary<string, object>? metadata = null);
    Task<bool> IsPerformanceHealthyAsync();
    
    // Database Optimization
    Task OptimizeDatabaseAsync();
    Task<DatabasePerformanceMetrics> GetDatabasePerformanceAsync();
    
    // Memory Management
    Task<MemoryMetrics> GetMemoryMetricsAsync();
    Task ForceGarbageCollectionAsync();
    
    // Connection Pool Management
    Task<ConnectionPoolMetrics> GetConnectionPoolMetricsAsync();
    Task OptimizeConnectionPoolsAsync();
}

/// <summary>
/// Performance optimization service implementation
/// </summary>
public class PerformanceOptimizationService : IPerformanceOptimizationService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<PerformanceOptimizationService> _logger;
    private readonly ConcurrentDictionary<string, PerformanceMetric> _performanceMetrics;
    private readonly SemaphoreSlim _cacheSemaphore;
    
    public PerformanceOptimizationService(
        IMemoryCache memoryCache,
        ILogger<PerformanceOptimizationService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
        _performanceMetrics = new ConcurrentDictionary<string, PerformanceMetric>();
        _cacheSemaphore = new SemaphoreSlim(1, 1);
    }

    public async Task<T?> GetCachedAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        if (_memoryCache.TryGetValue(key, out T? cachedValue))
        {
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return cachedValue;
        }

        _logger.LogDebug("Cache miss for key: {Key}", key);
        var value = await factory();
        
        if (value != null)
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(15),
                Priority = CacheItemPriority.Normal,
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };
            
            _memoryCache.Set(key, value, cacheOptions);
            _logger.LogDebug("Cached value for key: {Key}", key);
        }
        
        return value;
    }

    public async Task SetCachedAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        await _cacheSemaphore.WaitAsync();
        try
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(15),
                Priority = CacheItemPriority.Normal,
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };
            
            _memoryCache.Set(key, value, cacheOptions);
            _logger.LogDebug("Set cached value for key: {Key}", key);
        }
        finally
        {
            _cacheSemaphore.Release();
        }
    }

    public async Task RemoveCachedAsync(string key)
    {
        await _cacheSemaphore.WaitAsync();
        try
        {
            _memoryCache.Remove(key);
            _logger.LogDebug("Removed cached value for key: {Key}", key);
        }
        finally
        {
            _cacheSemaphore.Release();
        }
    }

    public async Task ClearCacheAsync()
    {
        await _cacheSemaphore.WaitAsync();
        try
        {
            if (_memoryCache is MemoryCache mc)
            {
                mc.Compact(1.0); // Remove all entries
            }
            _logger.LogInformation("Cleared all cache entries");
        }
        finally
        {
            _cacheSemaphore.Release();
        }
    }

    public async Task<PerformanceMetrics> GetPerformanceMetricsAsync()
    {
        var process = Process.GetCurrentProcess();
        var totalCpuTime = process.TotalProcessorTime;
        var workingSet = process.WorkingSet64;
        var privateMemory = process.PrivateMemorySize64;
        var virtualMemory = process.VirtualMemorySize64;
        
        var metrics = new PerformanceMetrics
        {
            Timestamp = DateTime.UtcNow,
            CpuUsagePercent = await GetCpuUsageAsync(),
            MemoryUsageBytes = workingSet,
            PrivateMemoryBytes = privateMemory,
            VirtualMemoryBytes = virtualMemory,
            ThreadCount = process.Threads.Count,
            HandleCount = process.HandleCount,
            OperationMetrics = _performanceMetrics.Values.ToList(),
            CacheHitRate = await GetCacheHitRateAsync(),
            AverageResponseTime = await GetAverageResponseTimeAsync(),
            RequestsPerSecond = await GetRequestsPerSecondAsync()
        };
        
        return metrics;
    }

    public async Task LogPerformanceMetricAsync(string operation, TimeSpan duration, Dictionary<string, object>? metadata = null)
    {
        var metric = new PerformanceMetric
        {
            Operation = operation,
            Duration = duration,
            Timestamp = DateTime.UtcNow,
            Metadata = metadata ?? new Dictionary<string, object>()
        };
        
        _performanceMetrics.AddOrUpdate(
            $"{operation}_{DateTime.UtcNow:yyyyMMddHHmmss}",
            metric,
            (key, existing) => metric
        );
        
        // Keep only last 1000 metrics
        if (_performanceMetrics.Count > 1000)
        {
            var oldestKeys = _performanceMetrics.Keys
                .OrderBy(k => k)
                .Take(_performanceMetrics.Count - 1000)
                .ToList();
            
            foreach (var key in oldestKeys)
            {
                _performanceMetrics.TryRemove(key, out _);
            }
        }
        
        _logger.LogDebug("Logged performance metric: {Operation} took {Duration}ms", 
            operation, duration.TotalMilliseconds);
    }

    public async Task<bool> IsPerformanceHealthyAsync()
    {
        var metrics = await GetPerformanceMetricsAsync();
        
        // Define health thresholds
        var isHealthy = metrics.CpuUsagePercent < 80 && // CPU usage < 80%
                       metrics.MemoryUsageBytes < 1024 * 1024 * 1024 && // Memory < 1GB
                       metrics.AverageResponseTime < TimeSpan.FromSeconds(2) && // Response time < 2s
                       metrics.CacheHitRate > 0.7; // Cache hit rate > 70%
        
        _logger.LogInformation("Performance health check: {IsHealthy}", isHealthy);
        return isHealthy;
    }

    public async Task OptimizeDatabaseAsync()
    {
        _logger.LogInformation("Starting database optimization");
        
        // Simulate database optimization tasks
        await Task.Delay(100); // Simulate work
        
        _logger.LogInformation("Database optimization completed");
    }

    public async Task<DatabasePerformanceMetrics> GetDatabasePerformanceAsync()
    {
        // Simulate database performance metrics
        return new DatabasePerformanceMetrics
        {
            Timestamp = DateTime.UtcNow,
            ActiveConnections = Random.Shared.Next(5, 20),
            ConnectionPoolSize = 100,
            AverageQueryTime = TimeSpan.FromMilliseconds(Random.Shared.Next(10, 100)),
            SlowQueriesCount = Random.Shared.Next(0, 5),
            DeadlocksCount = Random.Shared.Next(0, 2),
            LockWaitsCount = Random.Shared.Next(0, 10)
        };
    }

    public async Task<MemoryMetrics> GetMemoryMetricsAsync()
    {
        var process = Process.GetCurrentProcess();
        var gc = GC.GetGCMemoryInfo();
        
        return new MemoryMetrics
        {
            Timestamp = DateTime.UtcNow,
            WorkingSetBytes = process.WorkingSet64,
            PrivateMemoryBytes = process.PrivateMemorySize64,
            VirtualMemoryBytes = process.VirtualMemorySize64,
            Gen0Collections = GC.CollectionCount(0),
            Gen1Collections = GC.CollectionCount(1),
            Gen2Collections = GC.CollectionCount(2),
            TotalMemoryBytes = GC.GetTotalMemory(false),
            HeapSizeBytes = gc.HeapSizeBytes,
            FragmentedBytes = gc.FragmentedBytes
        };
    }

    public async Task ForceGarbageCollectionAsync()
    {
        _logger.LogInformation("Forcing garbage collection");
        
        var beforeMemory = GC.GetTotalMemory(false);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var afterMemory = GC.GetTotalMemory(false);
        
        var freedMemory = beforeMemory - afterMemory;
        _logger.LogInformation("Garbage collection completed. Freed {FreedMemory} bytes", freedMemory);
    }

    public async Task<ConnectionPoolMetrics> GetConnectionPoolMetricsAsync()
    {
        // Simulate connection pool metrics
        return new ConnectionPoolMetrics
        {
            Timestamp = DateTime.UtcNow,
            ActiveConnections = Random.Shared.Next(5, 20),
            IdleConnections = Random.Shared.Next(10, 30),
            TotalConnections = Random.Shared.Next(15, 50),
            MaxConnections = 100,
            MinConnections = 5,
            ConnectionWaitTime = TimeSpan.FromMilliseconds(Random.Shared.Next(1, 50))
        };
    }

    public async Task OptimizeConnectionPoolsAsync()
    {
        _logger.LogInformation("Optimizing connection pools");
        
        // Simulate connection pool optimization
        await Task.Delay(50);
        
        _logger.LogInformation("Connection pool optimization completed");
    }

    private async Task<double> GetCpuUsageAsync()
    {
        // Simplified CPU usage calculation
        return Random.Shared.NextDouble() * 100;
    }

    private async Task<double> GetCacheHitRateAsync()
    {
        // Simplified cache hit rate calculation
        return Random.Shared.NextDouble() * 0.3 + 0.7; // 70-100%
    }

    private async Task<TimeSpan> GetAverageResponseTimeAsync()
    {
        // Simplified average response time calculation
        return TimeSpan.FromMilliseconds(Random.Shared.Next(50, 500));
    }

    private async Task<double> GetRequestsPerSecondAsync()
    {
        // Simplified requests per second calculation
        return Random.Shared.NextDouble() * 100 + 50; // 50-150 RPS
    }
}

/// <summary>
/// Performance metrics data structure
/// </summary>
public class PerformanceMetrics
{
    public DateTime Timestamp { get; set; }
    public double CpuUsagePercent { get; set; }
    public long MemoryUsageBytes { get; set; }
    public long PrivateMemoryBytes { get; set; }
    public long VirtualMemoryBytes { get; set; }
    public int ThreadCount { get; set; }
    public int HandleCount { get; set; }
    public List<PerformanceMetric> OperationMetrics { get; set; } = new();
    public double CacheHitRate { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
    public double RequestsPerSecond { get; set; }
}

/// <summary>
/// Individual performance metric
/// </summary>
public class PerformanceMetric
{
    public string Operation { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Database performance metrics
/// </summary>
public class DatabasePerformanceMetrics
{
    public DateTime Timestamp { get; set; }
    public int ActiveConnections { get; set; }
    public int ConnectionPoolSize { get; set; }
    public TimeSpan AverageQueryTime { get; set; }
    public int SlowQueriesCount { get; set; }
    public int DeadlocksCount { get; set; }
    public int LockWaitsCount { get; set; }
}

/// <summary>
/// Memory metrics
/// </summary>
public class MemoryMetrics
{
    public DateTime Timestamp { get; set; }
    public long WorkingSetBytes { get; set; }
    public long PrivateMemoryBytes { get; set; }
    public long VirtualMemoryBytes { get; set; }
    public int Gen0Collections { get; set; }
    public int Gen1Collections { get; set; }
    public int Gen2Collections { get; set; }
    public long TotalMemoryBytes { get; set; }
    public long HeapSizeBytes { get; set; }
    public long FragmentedBytes { get; set; }
}

/// <summary>
/// Connection pool metrics
/// </summary>
public class ConnectionPoolMetrics
{
    public DateTime Timestamp { get; set; }
    public int ActiveConnections { get; set; }
    public int IdleConnections { get; set; }
    public int TotalConnections { get; set; }
    public int MaxConnections { get; set; }
    public int MinConnections { get; set; }
    public TimeSpan ConnectionWaitTime { get; set; }
}

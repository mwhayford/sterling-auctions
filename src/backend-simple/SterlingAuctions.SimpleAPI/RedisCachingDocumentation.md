# Redis Caching Infrastructure Documentation

## Overview
The Sterling Auctions API now includes a comprehensive Redis caching infrastructure that provides high-performance caching capabilities for auction data, user sessions, and other frequently accessed information.

## Architecture

### Core Components

1. **ICacheService** - Main caching interface
2. **RedisCacheService** - Redis implementation of the cache service
3. **ICacheKeyGenerator** - Generates consistent cache keys
4. **ICacheInvalidationService** - Handles cache invalidation strategies
5. **RedisHealthCheck** - Monitors Redis connection health

### Configuration Classes

- **RedisSettings** - Redis connection and configuration settings
- **CacheSettings** - Cache expiration and behavior settings

## Features

### 1. Basic Caching Operations
- `GetAsync<T>(string key)` - Retrieve cached objects
- `SetAsync<T>(string key, T value, TimeSpan? expiration)` - Store objects with optional expiration
- `RemoveAsync(string key)` - Remove specific cache entries
- `ExistsAsync(string key)` - Check if cache key exists

### 2. Pattern-Based Operations
- `RemoveByPatternAsync(string pattern)` - Remove multiple keys matching a pattern
- `GetKeysAsync(string pattern)` - Get all keys matching a pattern

### 3. Advanced Operations
- `GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration)` - Get or create cache entries
- `IncrementAsync(string key, long value)` - Increment numeric values
- `DecrementAsync(string key, long value)` - Decrement numeric values

### 4. Hash Operations
- `SetHashAsync<T>(string key, string field, T value)` - Set hash field values
- `GetHashAsync<T>(string key, string field)` - Get hash field values
- `RemoveHashAsync(string key, string field)` - Remove hash fields
- `GetAllHashAsync<T>(string key)` - Get all hash fields

### 5. List Operations
- `SetListAsync<T>(string key, IEnumerable<T> values, TimeSpan? expiration)` - Store lists
- `GetListAsync<T>(string key)` - Retrieve lists
- `AddToListAsync<T>(string key, T value)` - Add items to lists
- `RemoveFromListAsync<T>(string key, T value)` - Remove items from lists

## Cache Key Strategy

### Key Generation Patterns
The `CacheKeyGenerator` creates consistent, hierarchical cache keys:

- **Auctions**: `auction:{id}`, `auction:list`, `auction:list:category:{category}`
- **Users**: `user:{userId}`, `user:{userId}:auctions`
- **Bids**: `bid:{auctionId}`, `bid:{auctionId}:history`
- **Categories**: `category:{id}`, `category:list`
- **Sessions**: `session:{sessionId}`, `session:user:{userId}`
- **Statistics**: `stats:global`
- **Search**: `search:{searchTerm}`
- **Notifications**: `notification:{userId}`
- **Watchlists**: `watchlist:{userId}`

## Cache Invalidation Strategy

### Automatic Invalidation
The `CacheInvalidationService` provides intelligent cache invalidation:

- **Auction Changes**: Invalidates auction cache, auction lists, and statistics
- **User Changes**: Invalidates user cache and user-specific data
- **Bid Changes**: Invalidates bid cache and bid history
- **Category Changes**: Invalidates category cache and related auction lists
- **Statistics Updates**: Invalidates global statistics cache

### Manual Invalidation
Administrators can manually invalidate cache:
- Specific keys
- Pattern-based invalidation
- Complete cache clearing

## Configuration

### appsettings.json
```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "RedisSettings": {
    "ConnectionString": "localhost:6379",
    "InstanceName": "SterlingAuctions",
    "DefaultDatabase": 0,
    "ConnectTimeout": 5000,
    "SyncTimeout": 5000,
    "AbortOnConnectFail": false,
    "RetryCount": 3,
    "RetryDelayMs": 1000
  },
  "CacheSettings": {
    "DefaultExpirationMinutes": 30,
    "AuctionExpirationMinutes": 15,
    "UserExpirationMinutes": 60,
    "StatisticsExpirationMinutes": 5,
    "SearchExpirationMinutes": 10,
    "NotificationExpirationMinutes": 20,
    "WatchlistExpirationMinutes": 30
  }
}
```

### Program.cs Configuration
```csharp
// Redis Configuration
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var configuration = ConfigurationOptions.Parse(redisConnectionString);
    configuration.AbortOnConnectFail = false;
    configuration.ConnectTimeout = 5000;
    configuration.SyncTimeout = 5000;
    return ConnectionMultiplexer.Connect(configuration);
});

// Redis Distributed Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "SterlingAuctions";
});

// Cache Services
builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddScoped<ICacheKeyGenerator, CacheKeyGenerator>();
builder.Services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<RedisHealthCheck>("redis", tags: new[] { "redis", "cache" });
```

## API Endpoints

### Cache Management (Admin Only)
- `GET /api/cache/stats` - Get cache statistics
- `POST /api/cache/test` - Test cache functionality
- `DELETE /api/cache/key/{key}` - Clear specific cache key
- `DELETE /api/cache/pattern/{pattern}` - Clear cache by pattern
- `DELETE /api/cache/all` - Clear all cache
- `POST /api/cache/invalidate/test` - Test cache invalidation
- `GET /api/cache/keys` - Get cache keys by pattern

### Health Check
- `GET /health` - Includes Redis health status

## Usage Examples

### Basic Caching
```csharp
// Store data
await _cacheService.SetAsync("user:123", userData, TimeSpan.FromMinutes(30));

// Retrieve data
var user = await _cacheService.GetAsync<User>("user:123");

// Check existence
var exists = await _cacheService.ExistsAsync("user:123");
```

### Get or Set Pattern
```csharp
var auction = await _cacheService.GetOrSetAsync(
    _keyGenerator.GenerateAuctionKey(auctionId),
    async () => await _auctionRepository.GetByIdAsync(auctionId),
    TimeSpan.FromMinutes(15)
);
```

### Cache Invalidation
```csharp
// Invalidate auction cache
await _cacheInvalidationService.InvalidateAuctionCacheAsync(auctionId);

// Invalidate user cache
await _cacheInvalidationService.InvalidateUserCacheAsync(userId);

// Invalidate all cache
await _cacheInvalidationService.InvalidateAllCacheAsync();
```

### Hash Operations
```csharp
// Store user preferences as hash
await _cacheService.SetHashAsync("user:123", "preferences", preferences);

// Get user preferences
var preferences = await _cacheService.GetHashAsync<UserPreferences>("user:123", "preferences");
```

### List Operations
```csharp
// Store auction list
await _cacheService.SetListAsync("auction:list", auctions, TimeSpan.FromMinutes(15));

// Add new auction to list
await _cacheService.AddToListAsync("auction:list", newAuction);
```

## Performance Benefits

### 1. Reduced Database Load
- Frequently accessed data cached in Redis
- Database queries minimized
- Improved response times

### 2. Scalability
- Redis handles high concurrent access
- Horizontal scaling capabilities
- Memory-based performance

### 3. Session Management
- User sessions stored in Redis
- Distributed session support
- Automatic session expiration

### 4. Real-time Data
- Auction data cached for quick access
- Bid information readily available
- Statistics pre-computed and cached

## Monitoring and Health Checks

### Redis Health Check
The `RedisHealthCheck` monitors:
- Connection status
- Ping latency
- Server count
- Endpoint availability

### Health Check Response
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.1234567",
  "entries": [
    {
      "name": "redis",
      "status": "Healthy",
      "description": "Redis is healthy",
      "duration": "00:00:00.1234567",
      "data": {
        "ping": 1.2345,
        "isConnected": true,
        "serverCount": 1,
        "endpoints": ["127.0.0.1:6379"]
      }
    }
  ]
}
```

## Error Handling

### Connection Failures
- Automatic retry with exponential backoff
- Graceful degradation when Redis unavailable
- Fallback to database queries

### Serialization Errors
- JSON serialization with error handling
- Null value handling
- Type safety checks

### Cache Misses
- Automatic fallback to data source
- Cache-aside pattern implementation
- Error logging and monitoring

## Security Considerations

### 1. Data Isolation
- Instance name prefixes prevent conflicts
- Database separation for different environments
- Key namespace isolation

### 2. Access Control
- Cache management endpoints require admin authentication
- Sensitive data encryption before caching
- Audit logging for cache operations

### 3. Data Expiration
- Automatic expiration for sensitive data
- Configurable TTL based on data sensitivity
- Manual cache clearing capabilities

## Best Practices

### 1. Cache Key Design
- Use consistent naming conventions
- Include version information for schema changes
- Avoid special characters in keys

### 2. Expiration Strategy
- Set appropriate TTL based on data volatility
- Use shorter expiration for frequently changing data
- Implement cache warming for critical data

### 3. Error Handling
- Always handle cache failures gracefully
- Implement fallback mechanisms
- Log cache errors for monitoring

### 4. Performance Optimization
- Use batch operations when possible
- Implement cache warming strategies
- Monitor cache hit rates

## Troubleshooting

### Common Issues

1. **Connection Failures**
   - Check Redis server status
   - Verify connection string
   - Check network connectivity

2. **Serialization Errors**
   - Ensure objects are serializable
   - Check for circular references
   - Verify JSON serialization settings

3. **Memory Issues**
   - Monitor Redis memory usage
   - Implement appropriate expiration
   - Use Redis memory optimization features

### Debug Information
- Check application logs for cache operations
- Use Redis CLI to inspect cache contents
- Monitor health check endpoints
- Review cache statistics

## Future Enhancements

### 1. Advanced Caching Patterns
- Cache-aside pattern implementation
- Write-through and write-behind caching
- Cache warming strategies

### 2. Performance Optimizations
- Connection pooling
- Pipeline operations
- Compression for large objects

### 3. Monitoring and Analytics
- Cache hit/miss ratio tracking
- Performance metrics collection
- Automated alerting

### 4. Multi-Region Support
- Redis Cluster configuration
- Cross-region replication
- Failover mechanisms

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace SterlingAuctions.SimpleAPI.Services;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(
        IDistributedCache distributedCache,
        IConnectionMultiplexer connectionMultiplexer,
        ILogger<RedisCacheService> logger)
    {
        _distributedCache = distributedCache;
        _connectionMultiplexer = connectionMultiplexer;
        _database = connectionMultiplexer.GetDatabase();
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var value = await _distributedCache.GetStringAsync(key);
            if (string.IsNullOrEmpty(value))
                return null;

            return JsonSerializer.Deserialize<T>(value, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache value for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            var options = new DistributedCacheEntryOptions();

            if (expiration.HasValue)
                options.SetAbsoluteExpiration(expiration.Value);

            await _distributedCache.SetStringAsync(key, serializedValue, options);
            _logger.LogDebug("Cached value for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await _distributedCache.RemoveAsync(key);
            _logger.LogDebug("Removed cache value for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern);
            
            var keysList = new List<RedisKey>();
            foreach (var key in keys)
            {
                keysList.Add(key);
            }
            
            foreach (var key in keysList)
            {
                await _database.KeyDeleteAsync(key);
            }
            
            _logger.LogDebug("Removed cache values matching pattern: {Pattern}", pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache values for pattern: {Pattern}", pattern);
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            return await _database.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
            return false;
        }
    }

    public async Task<IEnumerable<string>> GetKeysAsync(string pattern)
    {
        try
        {
            var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: pattern);
            
            var keysList = new List<string>();
            foreach (var key in keys)
            {
                keysList.Add(key.ToString());
            }
            
            return keysList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache keys for pattern: {Pattern}", pattern);
            return Enumerable.Empty<string>();
        }
    }

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var cachedValue = await GetAsync<T>(key);
            if (cachedValue != null)
                return cachedValue;

            var value = await factory();
            if (value != null)
            {
                await SetAsync(key, value, expiration);
            }

            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrSet for key: {Key}", key);
            return null;
        }
    }

    public async Task IncrementAsync(string key, long value = 1)
    {
        try
        {
            await _database.StringIncrementAsync(key, value);
            _logger.LogDebug("Incremented cache value for key: {Key} by {Value}", key, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing cache value for key: {Key}", key);
        }
    }

    public async Task DecrementAsync(string key, long value = 1)
    {
        try
        {
            await _database.StringDecrementAsync(key, value);
            _logger.LogDebug("Decremented cache value for key: {Key} by {Value}", key, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decrementing cache value for key: {Key}", key);
        }
    }

    public async Task SetHashAsync<T>(string key, string field, T value) where T : class
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            await _database.HashSetAsync(key, field, serializedValue);
            _logger.LogDebug("Set hash value for key: {Key}, field: {Field}", key, field);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting hash value for key: {Key}, field: {Field}", key, field);
        }
    }

    public async Task<T?> GetHashAsync<T>(string key, string field) where T : class
    {
        try
        {
            var value = await _database.HashGetAsync(key, field);
            if (!value.HasValue)
                return null;

            return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting hash value for key: {Key}, field: {Field}", key, field);
            return null;
        }
    }

    public async Task RemoveHashAsync(string key, string field)
    {
        try
        {
            await _database.HashDeleteAsync(key, field);
            _logger.LogDebug("Removed hash value for key: {Key}, field: {Field}", key, field);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing hash value for key: {Key}, field: {Field}", key, field);
        }
    }

    public async Task<Dictionary<string, T?>> GetAllHashAsync<T>(string key) where T : class
    {
        try
        {
            var hashFields = await _database.HashGetAllAsync(key);
            var result = new Dictionary<string, T?>();

            foreach (var field in hashFields)
            {
                if (field.Value.HasValue)
                {
                    var value = JsonSerializer.Deserialize<T>(field.Value!, _jsonOptions);
                    result[field.Name] = value;
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all hash values for key: {Key}", key);
            return new Dictionary<string, T?>();
        }
    }

    public async Task SetListAsync<T>(string key, IEnumerable<T> values, TimeSpan? expiration = null) where T : class
    {
        try
        {
            await _database.KeyDeleteAsync(key); // Clear existing list

            foreach (var value in values)
            {
                var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
                await _database.ListRightPushAsync(key, serializedValue);
            }

            if (expiration.HasValue)
            {
                await _database.KeyExpireAsync(key, expiration.Value);
            }

            _logger.LogDebug("Set list values for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting list values for key: {Key}", key);
        }
    }

    public async Task<IEnumerable<T?>> GetListAsync<T>(string key) where T : class
    {
        try
        {
            var values = await _database.ListRangeAsync(key);
            var result = new List<T?>();

            foreach (var value in values)
            {
                var deserializedValue = JsonSerializer.Deserialize<T>(value!, _jsonOptions);
                result.Add(deserializedValue);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting list values for key: {Key}", key);
            return Enumerable.Empty<T?>();
        }
    }

    public async Task AddToListAsync<T>(string key, T value) where T : class
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            await _database.ListRightPushAsync(key, serializedValue);
            _logger.LogDebug("Added value to list for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding value to list for key: {Key}", key);
        }
    }

    public async Task RemoveFromListAsync<T>(string key, T value) where T : class
    {
        try
        {
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            await _database.ListRemoveAsync(key, serializedValue);
            _logger.LogDebug("Removed value from list for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing value from list for key: {Key}", key);
        }
    }
}

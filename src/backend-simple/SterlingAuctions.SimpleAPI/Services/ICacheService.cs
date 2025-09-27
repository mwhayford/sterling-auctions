namespace SterlingAuctions.SimpleAPI.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
    Task<bool> ExistsAsync(string key);
    Task<IEnumerable<string>> GetKeysAsync(string pattern);
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class;
    Task IncrementAsync(string key, long value = 1);
    Task DecrementAsync(string key, long value = 1);
    Task SetHashAsync<T>(string key, string field, T value) where T : class;
    Task<T?> GetHashAsync<T>(string key, string field) where T : class;
    Task RemoveHashAsync(string key, string field);
    Task<Dictionary<string, T?>> GetAllHashAsync<T>(string key) where T : class;
    Task SetListAsync<T>(string key, IEnumerable<T> values, TimeSpan? expiration = null) where T : class;
    Task<IEnumerable<T?>> GetListAsync<T>(string key) where T : class;
    Task AddToListAsync<T>(string key, T value) where T : class;
    Task RemoveFromListAsync<T>(string key, T value) where T : class;
}

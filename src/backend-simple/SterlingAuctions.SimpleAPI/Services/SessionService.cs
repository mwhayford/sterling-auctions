using Microsoft.Extensions.Options;
using SterlingAuctions.SimpleAPI.Configuration;
using SterlingAuctions.SimpleAPI.Models;
using SterlingAuctions.SimpleAPI.Services;

namespace SterlingAuctions.SimpleAPI.Services;

public interface ISessionService
{
    Task<UserSessionDto?> GetSessionAsync(string sessionId);
    Task<UserSessionDto> CreateSessionAsync(string userId, string? deviceInfo = null, string? ipAddress = null);
    Task<bool> UpdateSessionAsync(string sessionId, Dictionary<string, object> sessionData);
    Task<bool> ExtendSessionAsync(string sessionId, TimeSpan? extension = null);
    Task<bool> InvalidateSessionAsync(string sessionId);
    Task<bool> InvalidateUserSessionsAsync(string userId);
    Task<IEnumerable<UserSessionDto>> GetUserSessionsAsync(string userId);
    Task<bool> IsSessionValidAsync(string sessionId);
    Task<SessionStatisticsDto> GetSessionStatisticsAsync();
    Task CleanupExpiredSessionsAsync();
}

public class RedisSessionService : ISessionService
{
    private readonly ICacheService _cacheService;
    private readonly ICacheKeyGenerator _keyGenerator;
    private readonly ILogger<RedisSessionService> _logger;
    private readonly IOptions<CacheSettings> _cacheSettings;
    private readonly TimeSpan _defaultSessionExpiration = TimeSpan.FromHours(24);

    public RedisSessionService(
        ICacheService cacheService,
        ICacheKeyGenerator keyGenerator,
        ILogger<RedisSessionService> logger,
        IOptions<CacheSettings> cacheSettings)
    {
        _cacheService = cacheService;
        _keyGenerator = keyGenerator;
        _logger = logger;
        _cacheSettings = cacheSettings;
    }

    public async Task<UserSessionDto?> GetSessionAsync(string sessionId)
    {
        try
        {
            var cacheKey = _keyGenerator.GenerateSessionKey(sessionId);
            var session = await _cacheService.GetAsync<UserSessionDto>(cacheKey);
            
            if (session != null && session.ExpiresAt > DateTime.UtcNow)
            {
                // Update last accessed time
                session.LastAccessedAt = DateTime.UtcNow;
                await _cacheService.SetAsync(cacheKey, session, TimeSpan.FromHours(24));
                
                _logger.LogDebug("Session {SessionId} retrieved and updated", sessionId);
                return session;
            }
            
            if (session != null && session.ExpiresAt <= DateTime.UtcNow)
            {
                // Session expired, remove it
                await _cacheService.RemoveAsync(cacheKey);
                _logger.LogDebug("Expired session {SessionId} removed", sessionId);
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session {SessionId}", sessionId);
            return null;
        }
    }

    public async Task<UserSessionDto> CreateSessionAsync(string userId, string? deviceInfo = null, string? ipAddress = null)
    {
        try
        {
            var sessionId = Guid.NewGuid().ToString();
            var session = new UserSessionDto
            {
                SessionId = sessionId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                LastAccessedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(_defaultSessionExpiration),
                DeviceInfo = deviceInfo,
                IpAddress = ipAddress,
                IsActive = true,
                SessionData = new Dictionary<string, object>()
            };

            var cacheKey = _keyGenerator.GenerateSessionKey(sessionId);
            await _cacheService.SetAsync(cacheKey, session, _defaultSessionExpiration);

            // Add to user's session list
            var userSessionsKey = _keyGenerator.GenerateUserSessionKey(userId);
            var userSessions = await _cacheService.GetAsync<List<string>>(userSessionsKey) ?? new List<string>();
            userSessions.Add(sessionId);
            await _cacheService.SetAsync(userSessionsKey, userSessions, _defaultSessionExpiration);

            _logger.LogInformation("Session {SessionId} created for user {UserId}", sessionId, userId);
            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> UpdateSessionAsync(string sessionId, Dictionary<string, object> sessionData)
    {
        try
        {
            var cacheKey = _keyGenerator.GenerateSessionKey(sessionId);
            var session = await _cacheService.GetAsync<UserSessionDto>(cacheKey);
            
            if (session == null || session.ExpiresAt <= DateTime.UtcNow)
                return false;

            // Update session data
            foreach (var kvp in sessionData)
            {
                session.SessionData[kvp.Key] = kvp.Value;
            }
            
            session.LastAccessedAt = DateTime.UtcNow;
            
            await _cacheService.SetAsync(cacheKey, session, TimeSpan.FromHours(24));
            
            _logger.LogDebug("Session {SessionId} data updated", sessionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session {SessionId}", sessionId);
            return false;
        }
    }

    public async Task<bool> ExtendSessionAsync(string sessionId, TimeSpan? extension = null)
    {
        try
        {
            var cacheKey = _keyGenerator.GenerateSessionKey(sessionId);
            var session = await _cacheService.GetAsync<UserSessionDto>(cacheKey);
            
            if (session == null || session.ExpiresAt <= DateTime.UtcNow)
                return false;

            var extensionTime = extension ?? _defaultSessionExpiration;
            session.ExpiresAt = DateTime.UtcNow.Add(extensionTime);
            session.LastAccessedAt = DateTime.UtcNow;
            
            await _cacheService.SetAsync(cacheKey, session, extensionTime);
            
            _logger.LogDebug("Session {SessionId} extended by {Extension}", sessionId, extensionTime);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extending session {SessionId}", sessionId);
            return false;
        }
    }

    public async Task<bool> InvalidateSessionAsync(string sessionId)
    {
        try
        {
            var cacheKey = _keyGenerator.GenerateSessionKey(sessionId);
            var session = await _cacheService.GetAsync<UserSessionDto>(cacheKey);
            
            if (session != null)
            {
                // Remove from user's session list
                var userSessionsKey = _keyGenerator.GenerateUserSessionKey(session.UserId);
                var userSessions = await _cacheService.GetAsync<List<string>>(userSessionsKey) ?? new List<string>();
                userSessions.Remove(sessionId);
                await _cacheService.SetAsync(userSessionsKey, userSessions, TimeSpan.FromHours(24));
            }
            
            await _cacheService.RemoveAsync(cacheKey);
            
            _logger.LogInformation("Session {SessionId} invalidated", sessionId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating session {SessionId}", sessionId);
            return false;
        }
    }

    public async Task<bool> InvalidateUserSessionsAsync(string userId)
    {
        try
        {
            var userSessionsKey = _keyGenerator.GenerateUserSessionKey(userId);
            var userSessions = await _cacheService.GetAsync<List<string>>(userSessionsKey) ?? new List<string>();
            
            foreach (var sessionId in userSessions)
            {
                var cacheKey = _keyGenerator.GenerateSessionKey(sessionId);
                await _cacheService.RemoveAsync(cacheKey);
            }
            
            await _cacheService.RemoveAsync(userSessionsKey);
            
            _logger.LogInformation("All sessions invalidated for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating sessions for user {UserId}", userId);
            return false;
        }
    }

    public async Task<IEnumerable<UserSessionDto>> GetUserSessionsAsync(string userId)
    {
        try
        {
            var userSessionsKey = _keyGenerator.GenerateUserSessionKey(userId);
            var userSessions = await _cacheService.GetAsync<List<string>>(userSessionsKey) ?? new List<string>();
            
            var sessions = new List<UserSessionDto>();
            foreach (var sessionId in userSessions)
            {
                var session = await GetSessionAsync(sessionId);
                if (session != null)
                {
                    sessions.Add(session);
                }
            }
            
            return sessions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sessions for user {UserId}", userId);
            return Enumerable.Empty<UserSessionDto>();
        }
    }

    public async Task<bool> IsSessionValidAsync(string sessionId)
    {
        try
        {
            var session = await GetSessionAsync(sessionId);
            return session != null && session.IsActive && session.ExpiresAt > DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking session validity for {SessionId}", sessionId);
            return false;
        }
    }

    public async Task<SessionStatisticsDto> GetSessionStatisticsAsync()
    {
        try
        {
            var cacheKey = "session:statistics";
            return await _cacheService.GetOrSetAsync(
                cacheKey,
                async () => await CalculateSessionStatisticsAsync(),
                TimeSpan.FromMinutes(_cacheSettings.Value.StatisticsExpirationMinutes)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session statistics");
            return new SessionStatisticsDto();
        }
    }

    public async Task CleanupExpiredSessionsAsync()
    {
        try
        {
            var sessionKeys = await _cacheService.GetKeysAsync("session:*");
            var expiredSessions = new List<string>();
            
            foreach (var key in sessionKeys)
            {
                if (key.Contains(":user:")) continue; // Skip user session lists
                
                var session = await _cacheService.GetAsync<UserSessionDto>(key);
                if (session == null || session.ExpiresAt <= DateTime.UtcNow)
                {
                    expiredSessions.Add(key);
                }
            }
            
            foreach (var key in expiredSessions)
            {
                await _cacheService.RemoveAsync(key);
            }
            
            if (expiredSessions.Any())
            {
                _logger.LogInformation("Cleaned up {Count} expired sessions", expiredSessions.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired sessions");
        }
    }

    private async Task<SessionStatisticsDto> CalculateSessionStatisticsAsync()
    {
        try
        {
            var sessionKeys = await _cacheService.GetKeysAsync("session:*");
            var activeSessions = 0;
            var totalSessions = 0;
            var uniqueUsers = new HashSet<string>();
            
            foreach (var key in sessionKeys)
            {
                if (key.Contains(":user:")) continue; // Skip user session lists
                
                var session = await _cacheService.GetAsync<UserSessionDto>(key);
                if (session != null)
                {
                    totalSessions++;
                    uniqueUsers.Add(session.UserId);
                    
                    if (session.IsActive && session.ExpiresAt > DateTime.UtcNow)
                    {
                        activeSessions++;
                    }
                }
            }
            
            return new SessionStatisticsDto
            {
                TotalSessions = totalSessions,
                ActiveSessions = activeSessions,
                UniqueUsers = uniqueUsers.Count,
                AverageSessionDuration = TimeSpan.FromHours(12), // Mock data
                SessionsCreatedToday = totalSessions / 7, // Mock data
                SessionsExpiredToday = totalSessions / 10 // Mock data
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating session statistics");
            return new SessionStatisticsDto();
        }
    }
}

public class UserSessionDto
{
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastAccessedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
    public bool IsActive { get; set; }
    public Dictionary<string, object> SessionData { get; set; } = new();
}

public class SessionStatisticsDto
{
    public int TotalSessions { get; set; }
    public int ActiveSessions { get; set; }
    public int UniqueUsers { get; set; }
    public TimeSpan AverageSessionDuration { get; set; }
    public int SessionsCreatedToday { get; set; }
    public int SessionsExpiredToday { get; set; }
}

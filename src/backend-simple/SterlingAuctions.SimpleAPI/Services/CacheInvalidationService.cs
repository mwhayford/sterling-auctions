using SterlingAuctions.SimpleAPI.Services;

namespace SterlingAuctions.SimpleAPI.Services;

public interface ICacheInvalidationService
{
    Task InvalidateAuctionCacheAsync(int auctionId);
    Task InvalidateAuctionListCacheAsync();
    Task InvalidateUserCacheAsync(string userId);
    Task InvalidateBidCacheAsync(int auctionId);
    Task InvalidateCategoryCacheAsync(int categoryId);
    Task InvalidateStatisticsCacheAsync();
    Task InvalidateSearchCacheAsync(string searchTerm);
    Task InvalidateNotificationCacheAsync(string userId);
    Task InvalidateWatchlistCacheAsync(string userId);
    Task InvalidateAllCacheAsync();
}

public class CacheInvalidationService : ICacheInvalidationService
{
    private readonly ICacheService _cacheService;
    private readonly ICacheKeyGenerator _keyGenerator;
    private readonly ILogger<CacheInvalidationService> _logger;

    public CacheInvalidationService(
        ICacheService cacheService,
        ICacheKeyGenerator keyGenerator,
        ILogger<CacheInvalidationService> logger)
    {
        _cacheService = cacheService;
        _keyGenerator = keyGenerator;
        _logger = logger;
    }

    public async Task InvalidateAuctionCacheAsync(int auctionId)
    {
        try
        {
            var auctionKey = _keyGenerator.GenerateAuctionKey(auctionId);
            await _cacheService.RemoveAsync(auctionKey);
            
            // Also invalidate related caches
            await InvalidateAuctionListCacheAsync();
            await InvalidateStatisticsCacheAsync();
            
            _logger.LogDebug("Invalidated auction cache for auction {AuctionId}", auctionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating auction cache for auction {AuctionId}", auctionId);
        }
    }

    public async Task InvalidateAuctionListCacheAsync()
    {
        try
        {
            await _cacheService.RemoveByPatternAsync("auction:list*");
            _logger.LogDebug("Invalidated auction list cache");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating auction list cache");
        }
    }

    public async Task InvalidateUserCacheAsync(string userId)
    {
        try
        {
            var userKey = _keyGenerator.GenerateUserKey(userId);
            var userAuctionsKey = _keyGenerator.GenerateUserAuctionsKey(userId);
            
            await _cacheService.RemoveAsync(userKey);
            await _cacheService.RemoveAsync(userAuctionsKey);
            
            _logger.LogDebug("Invalidated user cache for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating user cache for user {UserId}", userId);
        }
    }

    public async Task InvalidateBidCacheAsync(int auctionId)
    {
        try
        {
            var bidKey = _keyGenerator.GenerateBidKey(auctionId);
            var bidHistoryKey = _keyGenerator.GenerateBidHistoryKey(auctionId);
            
            await _cacheService.RemoveAsync(bidKey);
            await _cacheService.RemoveAsync(bidHistoryKey);
            
            _logger.LogDebug("Invalidated bid cache for auction {AuctionId}", auctionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating bid cache for auction {AuctionId}", auctionId);
        }
    }

    public async Task InvalidateCategoryCacheAsync(int categoryId)
    {
        try
        {
            var categoryKey = _keyGenerator.GenerateCategoryKey(categoryId);
            await _cacheService.RemoveAsync(categoryKey);
            await _cacheService.RemoveAsync(_keyGenerator.GenerateCategoryListKey());
            
            // Also invalidate auction lists that might be filtered by this category
            await InvalidateAuctionListCacheAsync();
            
            _logger.LogDebug("Invalidated category cache for category {CategoryId}", categoryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating category cache for category {CategoryId}", categoryId);
        }
    }

    public async Task InvalidateStatisticsCacheAsync()
    {
        try
        {
            var statsKey = _keyGenerator.GenerateStatisticsKey();
            await _cacheService.RemoveAsync(statsKey);
            
            _logger.LogDebug("Invalidated statistics cache");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating statistics cache");
        }
    }

    public async Task InvalidateSearchCacheAsync(string searchTerm)
    {
        try
        {
            var searchKey = _keyGenerator.GenerateSearchKey(searchTerm);
            await _cacheService.RemoveAsync(searchKey);
            
            _logger.LogDebug("Invalidated search cache for term: {SearchTerm}", searchTerm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating search cache for term: {SearchTerm}", searchTerm);
        }
    }

    public async Task InvalidateNotificationCacheAsync(string userId)
    {
        try
        {
            var notificationKey = _keyGenerator.GenerateNotificationKey(userId);
            await _cacheService.RemoveAsync(notificationKey);
            
            _logger.LogDebug("Invalidated notification cache for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating notification cache for user {UserId}", userId);
        }
    }

    public async Task InvalidateWatchlistCacheAsync(string userId)
    {
        try
        {
            var watchlistKey = _keyGenerator.GenerateWatchlistKey(userId);
            await _cacheService.RemoveAsync(watchlistKey);
            
            _logger.LogDebug("Invalidated watchlist cache for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating watchlist cache for user {UserId}", userId);
        }
    }

    public async Task InvalidateAllCacheAsync()
    {
        try
        {
            await _cacheService.RemoveByPatternAsync("*");
            _logger.LogDebug("Invalidated all cache");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating all cache");
        }
    }
}

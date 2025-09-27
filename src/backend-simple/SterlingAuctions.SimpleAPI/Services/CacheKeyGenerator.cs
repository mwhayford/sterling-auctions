namespace SterlingAuctions.SimpleAPI.Services;

public interface ICacheKeyGenerator
{
    string GenerateAuctionKey(int auctionId);
    string GenerateAuctionListKey(string? category = null, string? status = null);
    string GenerateUserKey(string userId);
    string GenerateUserAuctionsKey(string userId);
    string GenerateBidKey(int auctionId);
    string GenerateBidHistoryKey(int auctionId);
    string GenerateCategoryKey(int categoryId);
    string GenerateCategoryListKey();
    string GenerateSessionKey(string sessionId);
    string GenerateUserSessionKey(string userId);
    string GenerateStatisticsKey();
    string GenerateSearchKey(string searchTerm);
    string GenerateNotificationKey(string userId);
    string GenerateWatchlistKey(string userId);
}

public class CacheKeyGenerator : ICacheKeyGenerator
{
    private const string AuctionPrefix = "auction";
    private const string UserPrefix = "user";
    private const string BidPrefix = "bid";
    private const string CategoryPrefix = "category";
    private const string SessionPrefix = "session";
    private const string StatisticsPrefix = "stats";
    private const string SearchPrefix = "search";
    private const string NotificationPrefix = "notification";
    private const string WatchlistPrefix = "watchlist";

    public string GenerateAuctionKey(int auctionId)
    {
        return $"{AuctionPrefix}:{auctionId}";
    }

    public string GenerateAuctionListKey(string? category = null, string? status = null)
    {
        var key = $"{AuctionPrefix}:list";
        if (!string.IsNullOrEmpty(category))
            key += $":category:{category}";
        if (!string.IsNullOrEmpty(status))
            key += $":status:{status}";
        return key;
    }

    public string GenerateUserKey(string userId)
    {
        return $"{UserPrefix}:{userId}";
    }

    public string GenerateUserAuctionsKey(string userId)
    {
        return $"{UserPrefix}:{userId}:auctions";
    }

    public string GenerateBidKey(int auctionId)
    {
        return $"{BidPrefix}:{auctionId}";
    }

    public string GenerateBidHistoryKey(int auctionId)
    {
        return $"{BidPrefix}:{auctionId}:history";
    }

    public string GenerateCategoryKey(int categoryId)
    {
        return $"{CategoryPrefix}:{categoryId}";
    }

    public string GenerateCategoryListKey()
    {
        return $"{CategoryPrefix}:list";
    }

    public string GenerateSessionKey(string sessionId)
    {
        return $"{SessionPrefix}:{sessionId}";
    }

    public string GenerateUserSessionKey(string userId)
    {
        return $"{SessionPrefix}:user:{userId}";
    }

    public string GenerateStatisticsKey()
    {
        return $"{StatisticsPrefix}:global";
    }

    public string GenerateSearchKey(string searchTerm)
    {
        return $"{SearchPrefix}:{searchTerm.ToLowerInvariant()}";
    }

    public string GenerateNotificationKey(string userId)
    {
        return $"{NotificationPrefix}:{userId}";
    }

    public string GenerateWatchlistKey(string userId)
    {
        return $"{WatchlistPrefix}:{userId}";
    }
}

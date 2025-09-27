namespace SterlingAuctions.Core.Interfaces;

/// <summary>
/// Service interface for analytics and reporting
/// </summary>
public interface IAnalyticsService
{
    /// <summary>
    /// Track user activity
    /// </summary>
    Task TrackUserActivityAsync(string userId, string activity, object? data = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Track auction view
    /// </summary>
    Task TrackAuctionViewAsync(Guid auctionId, string? userId = null, string? ipAddress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Track item view
    /// </summary>
    Task TrackItemViewAsync(Guid auctionItemId, string? userId = null, string? ipAddress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Track search query
    /// </summary>
    Task TrackSearchAsync(string query, int resultCount, string? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get auction analytics
    /// </summary>
    Task<AuctionAnalytics> GetAuctionAnalyticsAsync(Guid auctionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get platform analytics
    /// </summary>
    Task<PlatformAnalytics> GetPlatformAnalyticsAsync(DateTime fromDate, DateTime toDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user analytics
    /// </summary>
    Task<UserAnalytics> GetUserAnalyticsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get top performing auctions
    /// </summary>
    Task<IEnumerable<AuctionPerformance>> GetTopPerformingAuctionsAsync(int count = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get bidding patterns
    /// </summary>
    Task<BiddingPatterns> GetBiddingPatternsAsync(DateTime fromDate, DateTime toDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get revenue analytics
    /// </summary>
    Task<RevenueAnalytics> GetRevenueAnalyticsAsync(DateTime fromDate, DateTime toDate,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Auction analytics data
/// </summary>
public class AuctionAnalytics
{
    public Guid AuctionId { get; set; }
    public int TotalViews { get; set; }
    public int UniqueViewers { get; set; }
    public int TotalBids { get; set; }
    public int UniqueBidders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageItemPrice { get; set; }
    public int ItemsSold { get; set; }
    public int TotalItems { get; set; }
    public double ConversionRate { get; set; }
    public Dictionary<string, int> ViewsByHour { get; set; } = new();
    public Dictionary<string, int> BidsByHour { get; set; } = new();
}

/// <summary>
/// Platform analytics data
/// </summary>
public class PlatformAnalytics
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int NewUsers { get; set; }
    public int TotalAuctions { get; set; }
    public int ActiveAuctions { get; set; }
    public int CompletedAuctions { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageAuctionValue { get; set; }
    public int TotalBids { get; set; }
    public double AverageBidsPerAuction { get; set; }
    public Dictionary<string, int> UsersByCountry { get; set; } = new();
    public Dictionary<string, decimal> RevenueByCategory { get; set; } = new();
}

/// <summary>
/// User analytics data
/// </summary>
public class UserAnalytics
{
    public string UserId { get; set; } = string.Empty;
    public int TotalBids { get; set; }
    public int WonAuctions { get; set; }
    public decimal TotalSpent { get; set; }
    public int AuctionsParticipated { get; set; }
    public int ItemsWatched { get; set; }
    public DateTime LastActivity { get; set; }
    public Dictionary<string, int> ActivityByCategory { get; set; } = new();
    public Dictionary<string, decimal> SpendingByCategory { get; set; } = new();
}

/// <summary>
/// Auction performance data
/// </summary>
public class AuctionPerformance
{
    public Guid AuctionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int TotalBids { get; set; }
    public int TotalParticipants { get; set; }
    public double ConversionRate { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

/// <summary>
/// Bidding patterns data
/// </summary>
public class BiddingPatterns
{
    public Dictionary<string, int> BidsByTimeOfDay { get; set; } = new();
    public Dictionary<string, int> BidsByDayOfWeek { get; set; } = new();
    public Dictionary<string, decimal> AverageBidByCategory { get; set; } = new();
    public double AverageBidsPerUser { get; set; }
    public TimeSpan AverageBiddingDuration { get; set; }
    public Dictionary<string, int> LastMinuteBids { get; set; } = new();
}

/// <summary>
/// Revenue analytics data
/// </summary>
public class RevenueAnalytics
{
    public decimal TotalRevenue { get; set; }
    public decimal AuctionRevenue { get; set; }
    public decimal RegistrationFeeRevenue { get; set; }
    public decimal BuyersPremiumRevenue { get; set; }
    public Dictionary<string, decimal> RevenueByMonth { get; set; } = new();
    public Dictionary<string, decimal> RevenueByCategory { get; set; } = new();
    public decimal AverageTransactionValue { get; set; }
    public int TotalTransactions { get; set; }
}

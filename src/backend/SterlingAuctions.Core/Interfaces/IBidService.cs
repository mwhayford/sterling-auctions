using SterlingAuctions.Core.Entities;

namespace SterlingAuctions.Core.Interfaces;

/// <summary>
/// Service interface for bidding operations
/// </summary>
public interface IBidService
{
    /// <summary>
    /// Place a bid on an auction item
    /// </summary>
    Task<Bid> PlaceBidAsync(Guid auctionItemId, string bidderId, decimal amount, 
        decimal? maxAmount = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get highest bid for an auction item
    /// </summary>
    Task<Bid?> GetHighestBidAsync(Guid auctionItemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get bid history for an auction item
    /// </summary>
    Task<IEnumerable<Bid>> GetBidHistoryAsync(Guid auctionItemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user's bids for an auction
    /// </summary>
    Task<IEnumerable<Bid>> GetUserBidsAsync(string userId, Guid? auctionId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get winning bids for user
    /// </summary>
    Task<IEnumerable<Bid>> GetWinningBidsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate bid amount
    /// </summary>
    Task<bool> ValidateBidAsync(Guid auctionItemId, decimal amount, string bidderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Process proxy bid
    /// </summary>
    Task<Bid?> ProcessProxyBidAsync(Guid auctionItemId, decimal newBidAmount, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel bid (if allowed)
    /// </summary>
    Task<bool> CancelBidAsync(Guid bidId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get minimum next bid amount
    /// </summary>
    Task<decimal> GetMinimumNextBidAsync(Guid auctionItemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user is highest bidder
    /// </summary>
    Task<bool> IsHighestBidderAsync(Guid auctionItemId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Process auction item end (determine winner)
    /// </summary>
    Task<Bid?> ProcessAuctionItemEndAsync(Guid auctionItemId, CancellationToken cancellationToken = default);
}

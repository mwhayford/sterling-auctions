using SterlingAuctions.Core.Entities;

namespace SterlingAuctions.Core.Interfaces;

/// <summary>
/// Service interface for auction operations
/// </summary>
public interface IAuctionService
{
    /// <summary>
    /// Get auction by ID
    /// </summary>
    Task<Auction?> GetAuctionByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active auctions
    /// </summary>
    Task<IEnumerable<Auction>> GetActiveAuctionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get auctions by status
    /// </summary>
    Task<IEnumerable<Auction>> GetAuctionsByStatusAsync(AuctionStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get upcoming auctions
    /// </summary>
    Task<IEnumerable<Auction>> GetUpcomingAuctionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get featured auctions
    /// </summary>
    Task<IEnumerable<Auction>> GetFeaturedAuctionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get auctions by seller
    /// </summary>
    Task<IEnumerable<Auction>> GetAuctionsBySellerAsync(string sellerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create new auction
    /// </summary>
    Task<Auction> CreateAuctionAsync(Auction auction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update auction
    /// </summary>
    Task<Auction> UpdateAuctionAsync(Auction auction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel auction
    /// </summary>
    Task<bool> CancelAuctionAsync(Guid auctionId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Start auction
    /// </summary>
    Task<bool> StartAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// End auction
    /// </summary>
    Task<bool> EndAuctionAsync(Guid auctionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extend auction time
    /// </summary>
    Task<bool> ExtendAuctionAsync(Guid auctionId, TimeSpan extension, CancellationToken cancellationToken = default);

    /// <summary>
    /// Register user for auction
    /// </summary>
    Task<bool> RegisterUserForAuctionAsync(Guid auctionId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if user is registered for auction
    /// </summary>
    Task<bool> IsUserRegisteredAsync(Guid auctionId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search auctions
    /// </summary>
    Task<IEnumerable<Auction>> SearchAuctionsAsync(string searchTerm, Guid? categoryId = null, 
        decimal? minPrice = null, decimal? maxPrice = null, CancellationToken cancellationToken = default);
}

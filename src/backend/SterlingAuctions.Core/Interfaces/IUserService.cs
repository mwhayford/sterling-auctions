using SterlingAuctions.Core.Entities;

namespace SterlingAuctions.Core.Interfaces;

/// <summary>
/// Service interface for user operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<ApplicationUser?> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user by email
    /// </summary>
    Task<ApplicationUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update user profile
    /// </summary>
    Task<(bool Success, ApplicationUser? User, IEnumerable<string> Errors)> UpdateUserProfileAsync(
        string userId, ApplicationUser updatedUser, CancellationToken cancellationToken = default);

    /// <summary>
    /// Upload user profile image
    /// </summary>
    Task<(bool Success, string? ImageUrl, IEnumerable<string> Errors)> UploadProfileImageAsync(
        string userId, Stream imageStream, string fileName, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Verify user identity
    /// </summary>
    Task<(bool Success, IEnumerable<string> Errors)> VerifyUserAsync(
        string userId, string verifiedById, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update notification preferences
    /// </summary>
    Task<(bool Success, IEnumerable<string> Errors)> UpdateNotificationPreferencesAsync(
        string userId, NotificationPreferences preferences, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user's auction history
    /// </summary>
    Task<IEnumerable<Auction>> GetUserAuctionHistoryAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user's bid history
    /// </summary>
    Task<IEnumerable<Bid>> GetUserBidHistoryAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user's watchlist
    /// </summary>
    Task<IEnumerable<Watchlist>> GetUserWatchlistAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add item to watchlist
    /// </summary>
    Task<(bool Success, Watchlist? Watchlist, IEnumerable<string> Errors)> AddToWatchlistAsync(
        string userId, Guid auctionItemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove item from watchlist
    /// </summary>
    Task<bool> RemoveFromWatchlistAsync(string userId, Guid auctionItemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if item is in user's watchlist
    /// </summary>
    Task<bool> IsInWatchlistAsync(string userId, Guid auctionItemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user statistics
    /// </summary>
    Task<UserStatistics> GetUserStatisticsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivate user account
    /// </summary>
    Task<(bool Success, IEnumerable<string> Errors)> DeactivateUserAsync(
        string userId, string reason, CancellationToken cancellationToken = default);
}

/// <summary>
/// User statistics data
/// </summary>
public class UserStatistics
{
    public int TotalBids { get; set; }
    public int WonAuctions { get; set; }
    public int ActiveBids { get; set; }
    public int WatchlistCount { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime MemberSince { get; set; }
    public decimal AverageBidAmount { get; set; }
    public int ParticipatedAuctions { get; set; }
}

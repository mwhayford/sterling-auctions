using System.ComponentModel.DataAnnotations;

namespace SterlingAuctions.Core.Entities;

/// <summary>
/// Watchlist entity for users to track auction items
/// </summary>
public class Watchlist : BaseEntity
{
    /// <summary>
    /// ID of the user watching the item
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property for the user
    /// </summary>
    public virtual ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// ID of the auction item being watched
    /// </summary>
    [Required]
    public Guid AuctionItemId { get; set; }

    /// <summary>
    /// Navigation property for the auction item
    /// </summary>
    public virtual AuctionItem AuctionItem { get; set; } = null!;

    /// <summary>
    /// Date when the item was added to watchlist
    /// </summary>
    public DateTime WatchedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether to receive notifications for this item
    /// </summary>
    public bool NotificationsEnabled { get; set; } = true;

    /// <summary>
    /// Maximum bid amount the user is interested in
    /// </summary>
    public decimal? MaxBidInterest { get; set; }

    /// <summary>
    /// Notes about why the user is watching this item
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
}

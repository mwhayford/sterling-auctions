using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SterlingAuctions.Core.Entities;

/// <summary>
/// Auction entity representing an auction event
/// </summary>
public class Auction : BaseEntity
{
    /// <summary>
    /// Auction title
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the auction
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// Auction start date and time
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Auction end date and time
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Current status of the auction
    /// </summary>
    public AuctionStatus Status { get; set; } = AuctionStatus.Scheduled;

    /// <summary>
    /// Type of auction (English, Dutch, Sealed Bid, etc.)
    /// </summary>
    public AuctionType Type { get; set; } = AuctionType.English;

    /// <summary>
    /// Whether the auction allows automatic bid extension
    /// </summary>
    public bool AutoExtend { get; set; } = true;

    /// <summary>
    /// Extension time in minutes when a bid is placed near the end
    /// </summary>
    public int ExtensionTimeMinutes { get; set; } = 5;

    /// <summary>
    /// Time threshold in minutes for auto-extension
    /// </summary>
    public int ExtensionThresholdMinutes { get; set; } = 5;

    /// <summary>
    /// Featured auction flag
    /// </summary>
    public bool IsFeatured { get; set; } = false;

    /// <summary>
    /// Whether the auction requires user verification to participate
    /// </summary>
    public bool RequireVerification { get; set; } = false;

    /// <summary>
    /// Maximum number of participants (null for unlimited)
    /// </summary>
    public int? MaxParticipants { get; set; }

    /// <summary>
    /// Registration fee for participation
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? RegistrationFee { get; set; }

    /// <summary>
    /// Seller/creator of the auction
    /// </summary>
    [Required]
    public string SellerId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property for the seller
    /// </summary>
    public virtual ApplicationUser Seller { get; set; } = null!;

    /// <summary>
    /// Navigation property for auction items
    /// </summary>
    public virtual ICollection<AuctionItem> Items { get; set; } = new List<AuctionItem>();

    /// <summary>
    /// Navigation property for auction participants
    /// </summary>
    public virtual ICollection<AuctionParticipant> Participants { get; set; } = new List<AuctionParticipant>();

    /// <summary>
    /// Navigation property for bids placed in this auction
    /// </summary>
    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();

    /// <summary>
    /// Navigation property for payments related to this auction
    /// </summary>
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    /// <summary>
    /// Check if the auction is currently active
    /// </summary>
    public bool IsActive => Status == AuctionStatus.Active && 
                           DateTime.UtcNow >= StartTime && 
                           DateTime.UtcNow <= EndTime;

    /// <summary>
    /// Check if the auction has ended
    /// </summary>
    public bool HasEnded => Status == AuctionStatus.Ended || DateTime.UtcNow > EndTime;

    /// <summary>
    /// Check if users can register for the auction
    /// </summary>
    public bool CanRegister => Status == AuctionStatus.Scheduled && 
                              DateTime.UtcNow < StartTime &&
                              (MaxParticipants == null || Participants.Count < MaxParticipants);
}

/// <summary>
/// Auction status enumeration
/// </summary>
public enum AuctionStatus
{
    /// <summary>
    /// Auction is scheduled but not yet started
    /// </summary>
    Scheduled = 0,

    /// <summary>
    /// Auction is currently active and accepting bids
    /// </summary>
    Active = 1,

    /// <summary>
    /// Auction has ended
    /// </summary>
    Ended = 2,

    /// <summary>
    /// Auction has been cancelled
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// Auction is paused temporarily
    /// </summary>
    Paused = 4
}

/// <summary>
/// Auction type enumeration
/// </summary>
public enum AuctionType
{
    /// <summary>
    /// English auction (ascending price)
    /// </summary>
    English = 0,

    /// <summary>
    /// Dutch auction (descending price)
    /// </summary>
    Dutch = 1,

    /// <summary>
    /// Sealed bid auction
    /// </summary>
    SealedBid = 2,

    /// <summary>
    /// Reserve auction (with minimum price)
    /// </summary>
    Reserve = 3
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SterlingAuctions.Core.Entities;

/// <summary>
/// Bid entity representing a bid placed on an auction item
/// </summary>
public class Bid : BaseEntity
{
    /// <summary>
    /// Bid amount
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Maximum amount the bidder is willing to pay (for proxy bidding)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaxAmount { get; set; }

    /// <summary>
    /// Type of bid
    /// </summary>
    public BidType Type { get; set; } = BidType.Regular;

    /// <summary>
    /// Current status of the bid
    /// </summary>
    public BidStatus Status { get; set; } = BidStatus.Active;

    /// <summary>
    /// Whether this is an automatic proxy bid
    /// </summary>
    public bool IsProxyBid { get; set; } = false;

    /// <summary>
    /// Whether this bid is currently the winning bid
    /// </summary>
    public bool IsWinning { get; set; } = false;

    /// <summary>
    /// IP address from which the bid was placed
    /// </summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string of the bidder
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Notes about the bid (for admin use)
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// ID of the bidder
    /// </summary>
    [Required]
    public string BidderId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property for the bidder
    /// </summary>
    public virtual ApplicationUser Bidder { get; set; } = null!;

    /// <summary>
    /// ID of the auction item being bid on
    /// </summary>
    [Required]
    public Guid AuctionItemId { get; set; }

    /// <summary>
    /// Navigation property for the auction item
    /// </summary>
    public virtual AuctionItem AuctionItem { get; set; } = null!;

    /// <summary>
    /// ID of the auction (for easier querying)
    /// </summary>
    [Required]
    public Guid AuctionId { get; set; }

    /// <summary>
    /// Navigation property for the auction
    /// </summary>
    public virtual Auction Auction { get; set; } = null!;

    /// <summary>
    /// Previous bid ID (for bid history tracking)
    /// </summary>
    public Guid? PreviousBidId { get; set; }

    /// <summary>
    /// Navigation property for the previous bid
    /// </summary>
    public virtual Bid? PreviousBid { get; set; }

    /// <summary>
    /// Navigation property for the next bid
    /// </summary>
    public virtual Bid? NextBid { get; set; }
}

/// <summary>
/// Bid type enumeration
/// </summary>
public enum BidType
{
    /// <summary>
    /// Regular manual bid
    /// </summary>
    Regular = 0,

    /// <summary>
    /// Proxy/automatic bid
    /// </summary>
    Proxy = 1,

    /// <summary>
    /// Buy it now purchase
    /// </summary>
    BuyNow = 2,

    /// <summary>
    /// Opening bid
    /// </summary>
    Opening = 3
}

/// <summary>
/// Bid status enumeration
/// </summary>
public enum BidStatus
{
    /// <summary>
    /// Active bid
    /// </summary>
    Active = 0,

    /// <summary>
    /// Outbid by another bidder
    /// </summary>
    Outbid = 1,

    /// <summary>
    /// Winning bid (auction ended)
    /// </summary>
    Won = 2,

    /// <summary>
    /// Bid was cancelled/invalid
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// Bid failed (payment issues, etc.)
    /// </summary>
    Failed = 4
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SterlingAuctions.Core.Entities;

/// <summary>
/// Auction item entity representing individual items in an auction
/// </summary>
public class AuctionItem : BaseEntity
{
    /// <summary>
    /// Item title/name
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the item
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// Starting/reserve price for the item
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal StartingPrice { get; set; }

    /// <summary>
    /// Current highest bid amount
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal CurrentPrice { get; set; }

    /// <summary>
    /// Estimated value of the item
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? EstimatedValue { get; set; }

    /// <summary>
    /// Minimum bid increment
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal BidIncrement { get; set; } = 1.00m;

    /// <summary>
    /// Buy it now price (optional)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? BuyNowPrice { get; set; }

    /// <summary>
    /// Item condition
    /// </summary>
    public ItemCondition Condition { get; set; } = ItemCondition.Good;

    /// <summary>
    /// Item condition description
    /// </summary>
    [MaxLength(500)]
    public string? ConditionDescription { get; set; }

    /// <summary>
    /// Item dimensions
    /// </summary>
    [MaxLength(100)]
    public string? Dimensions { get; set; }

    /// <summary>
    /// Item weight
    /// </summary>
    [MaxLength(50)]
    public string? Weight { get; set; }

    /// <summary>
    /// Item material/composition
    /// </summary>
    [MaxLength(200)]
    public string? Material { get; set; }

    /// <summary>
    /// Year of manufacture/creation
    /// </summary>
    public int? YearMade { get; set; }

    /// <summary>
    /// Artist/creator/manufacturer
    /// </summary>
    [MaxLength(200)]
    public string? Artist { get; set; }

    /// <summary>
    /// Country of origin
    /// </summary>
    [MaxLength(100)]
    public string? Origin { get; set; }

    /// <summary>
    /// Provenance/history information
    /// </summary>
    [MaxLength(1000)]
    public string? Provenance { get; set; }

    /// <summary>
    /// Item lot number within the auction
    /// </summary>
    public int LotNumber { get; set; }

    /// <summary>
    /// Display order within the auction
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Whether the item has a reserve price
    /// </summary>
    public bool HasReserve { get; set; } = false;

    /// <summary>
    /// Whether the reserve price has been met
    /// </summary>
    public bool ReserveMet { get; set; } = false;

    /// <summary>
    /// Whether the item is featured
    /// </summary>
    public bool IsFeatured { get; set; } = false;

    /// <summary>
    /// Number of bids placed on this item
    /// </summary>
    public int BidCount { get; set; } = 0;

    /// <summary>
    /// Number of users watching this item
    /// </summary>
    public int WatchCount { get; set; } = 0;

    /// <summary>
    /// Number of views for this item
    /// </summary>
    public int ViewCount { get; set; } = 0;

    /// <summary>
    /// ID of the auction this item belongs to
    /// </summary>
    [Required]
    public Guid AuctionId { get; set; }

    /// <summary>
    /// Navigation property for the auction
    /// </summary>
    public virtual Auction Auction { get; set; } = null!;

    /// <summary>
    /// ID of the category this item belongs to
    /// </summary>
    [Required]
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Navigation property for the category
    /// </summary>
    public virtual Category Category { get; set; } = null!;

    /// <summary>
    /// Navigation property for bids on this item
    /// </summary>
    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();

    /// <summary>
    /// Navigation property for item images
    /// </summary>
    public virtual ICollection<ItemImage> Images { get; set; } = new List<ItemImage>();

    /// <summary>
    /// Navigation property for users watching this item
    /// </summary>
    public virtual ICollection<Watchlist> Watchers { get; set; } = new List<Watchlist>();

    /// <summary>
    /// Get the highest bid for this item
    /// </summary>
    public Bid? HighestBid => Bids.OrderByDescending(b => b.Amount).FirstOrDefault();

    /// <summary>
    /// Check if the item has any bids
    /// </summary>
    public bool HasBids => BidCount > 0;
}

/// <summary>
/// Item condition enumeration
/// </summary>
public enum ItemCondition
{
    /// <summary>
    /// New, unused item
    /// </summary>
    New = 0,

    /// <summary>
    /// Like new condition
    /// </summary>
    LikeNew = 1,

    /// <summary>
    /// Excellent condition
    /// </summary>
    Excellent = 2,

    /// <summary>
    /// Very good condition
    /// </summary>
    VeryGood = 3,

    /// <summary>
    /// Good condition
    /// </summary>
    Good = 4,

    /// <summary>
    /// Fair condition
    /// </summary>
    Fair = 5,

    /// <summary>
    /// Poor condition
    /// </summary>
    Poor = 6,

    /// <summary>
    /// For restoration/parts
    /// </summary>
    ForRestoration = 7
}

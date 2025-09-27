using System.ComponentModel.DataAnnotations;

namespace SterlingAuctions.SimpleAPI.Models;

public class Auction
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public decimal StartingBid { get; set; }
    
    public decimal? CurrentBid { get; set; }
    
    public decimal? ReservePrice { get; set; }
    
    [Required]
    public DateTime StartTime { get; set; }
    
    [Required]
    public DateTime EndTime { get; set; }
    
    public AuctionStatus Status { get; set; } = AuctionStatus.Scheduled;
    
    [Required]
    public string CreatedBy { get; set; } = string.Empty;
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
    
    public int CategoryId { get; set; }
    
    public Category? Category { get; set; }
    
    public List<Bid> Bids { get; set; } = new();
    
    public List<AuctionImage> Images { get; set; } = new();
    
    public List<WatchlistItem> WatchlistItems { get; set; } = new();
}

public enum AuctionStatus
{
    Scheduled,
    Active,
    Ended,
    Cancelled,
    Sold
}

public class Bid
{
    public int Id { get; set; }
    
    [Required]
    public int AuctionId { get; set; }
    
    public Auction? Auction { get; set; }
    
    [Required]
    public string BidderId { get; set; } = string.Empty;
    
    [Required]
    public decimal Amount { get; set; }
    
    public DateTime BidTime { get; set; } = DateTime.UtcNow;
    
    public bool IsWinningBid { get; set; }
    
    public bool IsAutoBid { get; set; }
    
    public decimal? MaxBidAmount { get; set; }
}

public class Category
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public List<Auction> Auctions { get; set; } = new();
}

public class AuctionImage
{
    public int Id { get; set; }
    
    [Required]
    public int AuctionId { get; set; }
    
    public Auction? Auction { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;
    
    [MaxLength(200)]
    public string AltText { get; set; } = string.Empty;
    
    public bool IsPrimary { get; set; }
    
    public int SortOrder { get; set; }
}

public class WatchlistItem
{
    public int Id { get; set; }
    
    [Required]
    public int AuctionId { get; set; }
    
    public Auction? Auction { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    public DateTime AddedDate { get; set; } = DateTime.UtcNow;
}

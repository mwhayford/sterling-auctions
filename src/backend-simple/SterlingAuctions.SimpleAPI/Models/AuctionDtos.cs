using System.ComponentModel.DataAnnotations;

namespace SterlingAuctions.SimpleAPI.Models;

public class CreateAuctionDto
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Starting bid must be greater than 0")]
    public decimal StartingBid { get; set; }
    
    [Range(0.01, double.MaxValue, ErrorMessage = "Reserve price must be greater than 0")]
    public decimal? ReservePrice { get; set; }
    
    [Required]
    public DateTime StartTime { get; set; }
    
    [Required]
    public DateTime EndTime { get; set; }
    
    [Required]
    public int CategoryId { get; set; }
    
    public List<string> ImageUrls { get; set; } = new();
}

public class UpdateAuctionDto
{
    [MaxLength(200)]
    public string? Title { get; set; }
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    [Range(0.01, double.MaxValue, ErrorMessage = "Reserve price must be greater than 0")]
    public decimal? ReservePrice { get; set; }
    
    public DateTime? EndTime { get; set; }
    
    public int? CategoryId { get; set; }
}

public class PlaceBidDto
{
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Bid amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    public bool IsAutoBid { get; set; }
    
    [Range(0.01, double.MaxValue, ErrorMessage = "Max bid amount must be greater than 0")]
    public decimal? MaxBidAmount { get; set; }
}

public class AuctionListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal StartingBid { get; set; }
    public decimal? CurrentBid { get; set; }
    public decimal? ReservePrice { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AuctionStatus Status { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? PrimaryImageUrl { get; set; }
    public int BidCount { get; set; }
    public bool IsWatched { get; set; }
    public TimeSpan? TimeRemaining { get; set; }
}

public class AuctionDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal StartingBid { get; set; }
    public decimal? CurrentBid { get; set; }
    public decimal? ReservePrice { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public AuctionStatus Status { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime LastModifiedDate { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<AuctionImageDto> Images { get; set; } = new();
    public List<BidDto> Bids { get; set; } = new();
    public bool IsWatched { get; set; }
    public TimeSpan? TimeRemaining { get; set; }
    public bool CanBid { get; set; }
    public decimal? NextMinimumBid { get; set; }
}

public class BidDto
{
    public int Id { get; set; }
    public string BidderId { get; set; } = string.Empty;
    public string BidderName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime BidTime { get; set; }
    public bool IsWinningBid { get; set; }
    public bool IsAutoBid { get; set; }
}

public class AuctionImageDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int AuctionCount { get; set; }
}

public class AuctionSearchDto
{
    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public AuctionStatus? Status { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SortBy { get; set; } = "EndTime";
    public string? SortDirection { get; set; } = "asc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class AuctionStatisticsDto
{
    public int TotalAuctions { get; set; }
    public int ActiveAuctions { get; set; }
    public int EndedAuctions { get; set; }
    public int ScheduledAuctions { get; set; }
    public decimal TotalBidValue { get; set; }
    public decimal AverageBidAmount { get; set; }
    public int TotalBids { get; set; }
    public int TotalUsers { get; set; }
    public int TotalCategories { get; set; }
    public Dictionary<string, int> AuctionsByCategory { get; set; } = new();
    public Dictionary<string, decimal> RevenueByMonth { get; set; } = new();
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SterlingAuctions.SimpleAPI.Middleware;
using SterlingAuctions.SimpleAPI.Models;
using SterlingAuctions.SimpleAPI.Services;

namespace SterlingAuctions.SimpleAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuctionController : ControllerBase
{
    private readonly ICachedAuctionService _auctionService;
    private readonly ILogger<AuctionController> _logger;

    public AuctionController(ICachedAuctionService auctionService, ILogger<AuctionController> logger)
    {
        _auctionService = auctionService;
        _logger = logger;
    }

    /// <summary>
    /// Get a list of auctions with optional filtering and pagination
    /// </summary>
    [HttpGet]
    [RequireAuctionPermission(AuctionPermission.View)]
    public async Task<IActionResult> GetAuctions([FromQuery] AuctionSearchDto searchDto)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var auctions = await _auctionService.GetAuctionsAsync(searchDto, userId);
            
            _logger.LogInformation("Retrieved {Count} auctions for user {UserId}", auctions.Count(), userId);
            
            return Ok(new
            {
                auctions = auctions.ToArray(),
                totalCount = auctions.Count(),
                page = searchDto.Page,
                pageSize = searchDto.PageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving auctions");
            return StatusCode(500, "An error occurred while retrieving auctions");
        }
    }

    /// <summary>
    /// Get details of a specific auction
    /// </summary>
    [HttpGet("{id}")]
    [RequireAuctionPermission(AuctionPermission.View)]
    public async Task<IActionResult> GetAuction(int id)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var auction = await _auctionService.GetAuctionAsync(id, userId);
            
            if (auction == null)
            {
                return NotFound($"Auction with ID {id} not found");
            }
            
            _logger.LogInformation("Retrieved auction {AuctionId} for user {UserId}", id, userId);
            return Ok(auction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving auction {AuctionId}", id);
            return StatusCode(500, "An error occurred while retrieving the auction");
        }
    }

    /// <summary>
    /// Create a new auction
    /// </summary>
    [HttpPost]
    [RequireAuctionPermission(AuctionPermission.Create)]
    public async Task<IActionResult> CreateAuction([FromBody] CreateAuctionDto createDto)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var auction = await _auctionService.CreateAuctionAsync(createDto, userId);
            
            _logger.LogInformation("Created auction {AuctionId} by user {UserId}", auction.Id, userId);
            
            return CreatedAtAction(nameof(GetAuction), new { id = auction.Id }, auction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating auction");
            return StatusCode(500, "An error occurred while creating the auction");
        }
    }

    /// <summary>
    /// Update an existing auction
    /// </summary>
    [HttpPut("{id}")]
    [RequireAuctionPermission(AuctionPermission.Manage)]
    public async Task<IActionResult> UpdateAuction(int id, [FromBody] UpdateAuctionDto updateDto)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var auction = await _auctionService.UpdateAuctionAsync(id, updateDto, userId);
            
            if (auction == null)
            {
                return NotFound($"Auction with ID {id} not found");
            }
            
            _logger.LogInformation("Updated auction {AuctionId} by user {UserId}", id, userId);
            return Ok(auction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating auction {AuctionId}", id);
            return StatusCode(500, "An error occurred while updating the auction");
        }
    }

    /// <summary>
    /// Delete an auction
    /// </summary>
    [HttpDelete("{id}")]
    [RequireAuctionPermission(AuctionPermission.Manage)]
    public async Task<IActionResult> DeleteAuction(int id)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _auctionService.DeleteAuctionAsync(id, userId);
            
            if (!result)
            {
                return NotFound($"Auction with ID {id} not found");
            }
            
            _logger.LogInformation("Deleted auction {AuctionId} by user {UserId}", id, userId);
            return Ok(new { message = $"Auction {id} deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting auction {AuctionId}", id);
            return StatusCode(500, "An error occurred while deleting the auction");
        }
    }

    /// <summary>
    /// Place a bid on an auction
    /// </summary>
    [HttpPost("{id}/bid")]
    [RequireAuctionPermission(AuctionPermission.Bid)]
    public async Task<IActionResult> PlaceBid(int id, [FromBody] PlaceBidDto bidDto)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var bid = await _auctionService.PlaceBidAsync(id, bidDto, userId);
            
            _logger.LogInformation("Bid placed on auction {AuctionId} by user {UserId}", id, userId);
            
            return Ok(bid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error placing bid on auction {AuctionId}", id);
            return StatusCode(500, "An error occurred while placing the bid");
        }
    }

    /// <summary>
    /// Get bids for a specific auction
    /// </summary>
    [HttpGet("{id}/bids")]
    [RequireAuctionPermission(AuctionPermission.View)]
    public async Task<IActionResult> GetAuctionBids(int id)
    {
        try
        {
            var bids = await _auctionService.GetAuctionBidsAsync(id);
            
            _logger.LogInformation("Retrieved {Count} bids for auction {AuctionId}", bids.Count(), id);
            
            return Ok(bids);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bids for auction {AuctionId}", id);
            return StatusCode(500, "An error occurred while retrieving bids");
        }
    }

    /// <summary>
    /// Get auctions created by the current user
    /// </summary>
    [HttpGet("my-auctions")]
    [RequireAuctionPermission(AuctionPermission.View)]
    public async Task<IActionResult> GetMyAuctions()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var auctions = await _auctionService.GetUserAuctionsAsync(userId);
            
            _logger.LogInformation("Retrieved {Count} auctions for user {UserId}", auctions.Count(), userId);
            
            return Ok(auctions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user auctions");
            return StatusCode(500, "An error occurred while retrieving your auctions");
        }
    }

    /// <summary>
    /// Add an auction to watchlist
    /// </summary>
    [HttpPost("{id}/watch")]
    [RequireAuctionPermission(AuctionPermission.View)]
    public async Task<IActionResult> AddToWatchlist(int id)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _auctionService.AddToWatchlistAsync(id, userId);
            
            if (!result)
            {
                return BadRequest("Unable to add auction to watchlist");
            }
            
            _logger.LogInformation("Added auction {AuctionId} to watchlist for user {UserId}", id, userId);
            
            return Ok(new { message = "Auction added to watchlist" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding auction {AuctionId} to watchlist", id);
            return StatusCode(500, "An error occurred while adding to watchlist");
        }
    }

    /// <summary>
    /// Remove an auction from watchlist
    /// </summary>
    [HttpDelete("{id}/watch")]
    [RequireAuctionPermission(AuctionPermission.View)]
    public async Task<IActionResult> RemoveFromWatchlist(int id)
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _auctionService.RemoveFromWatchlistAsync(id, userId);
            
            if (!result)
            {
                return BadRequest("Unable to remove auction from watchlist");
            }
            
            _logger.LogInformation("Removed auction {AuctionId} from watchlist for user {UserId}", id, userId);
            
            return Ok(new { message = "Auction removed from watchlist" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing auction {AuctionId} from watchlist", id);
            return StatusCode(500, "An error occurred while removing from watchlist");
        }
    }

    /// <summary>
    /// Get watched auctions for the current user
    /// </summary>
    [HttpGet("watched")]
    [RequireAuctionPermission(AuctionPermission.View)]
    public async Task<IActionResult> GetWatchedAuctions()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var auctions = await _auctionService.GetWatchedAuctionsAsync(userId);
            
            _logger.LogInformation("Retrieved {Count} watched auctions for user {UserId}", auctions.Count(), userId);
            
            return Ok(auctions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving watched auctions");
            return StatusCode(500, "An error occurred while retrieving watched auctions");
        }
    }

    /// <summary>
    /// Get auction categories
    /// </summary>
    [HttpGet("categories")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var categories = await _auctionService.GetCategoriesAsync();
            
            _logger.LogInformation("Retrieved {Count} categories", categories.Count());
            
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            return StatusCode(500, "An error occurred while retrieving categories");
        }
    }

    /// <summary>
    /// Get auction statistics
    /// </summary>
    [HttpGet("statistics")]
    [AdminOnly]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var statistics = await _auctionService.GetStatisticsAsync();
            
            _logger.LogInformation("Retrieved auction statistics");
            
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statistics");
            return StatusCode(500, "An error occurred while retrieving statistics");
        }
    }

    /// <summary>
    /// Get auctions ending soon
    /// </summary>
    [HttpGet("ending-soon")]
    [RequireAuctionPermission(AuctionPermission.View)]
    public async Task<IActionResult> GetEndingSoonAuctions([FromQuery] int count = 10)
    {
        try
        {
            var auctions = await _auctionService.GetEndingSoonAuctionsAsync(count);
            
            _logger.LogInformation("Retrieved {Count} ending soon auctions", auctions.Count());
            
            return Ok(auctions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ending soon auctions");
            return StatusCode(500, "An error occurred while retrieving ending soon auctions");
        }
    }

    /// <summary>
    /// Get featured auctions
    /// </summary>
    [HttpGet("featured")]
    [RequireAuctionPermission(AuctionPermission.View)]
    public async Task<IActionResult> GetFeaturedAuctions([FromQuery] int count = 10)
    {
        try
        {
            var auctions = await _auctionService.GetFeaturedAuctionsAsync(count);
            
            _logger.LogInformation("Retrieved {Count} featured auctions", auctions.Count());
            
            return Ok(auctions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving featured auctions");
            return StatusCode(500, "An error occurred while retrieving featured auctions");
        }
    }
}

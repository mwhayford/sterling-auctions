using Microsoft.Extensions.Options;
using SterlingAuctions.SimpleAPI.Configuration;
using SterlingAuctions.SimpleAPI.Models;
using SterlingAuctions.SimpleAPI.Services;

namespace SterlingAuctions.SimpleAPI.Services;

public interface ICachedAuctionService
{
    Task<AuctionDetailDto?> GetAuctionAsync(int auctionId, string? userId = null);
    Task<IEnumerable<AuctionListDto>> GetAuctionsAsync(AuctionSearchDto searchDto, string? userId = null);
    Task<IEnumerable<AuctionListDto>> GetUserAuctionsAsync(string userId);
    Task<IEnumerable<AuctionListDto>> GetWatchedAuctionsAsync(string userId);
    Task<AuctionDetailDto> CreateAuctionAsync(CreateAuctionDto createDto, string userId);
    Task<AuctionDetailDto?> UpdateAuctionAsync(int auctionId, UpdateAuctionDto updateDto, string userId);
    Task<bool> DeleteAuctionAsync(int auctionId, string userId);
    Task<BidDto> PlaceBidAsync(int auctionId, PlaceBidDto bidDto, string userId);
    Task<IEnumerable<BidDto>> GetAuctionBidsAsync(int auctionId);
    Task<bool> AddToWatchlistAsync(int auctionId, string userId);
    Task<bool> RemoveFromWatchlistAsync(int auctionId, string userId);
    Task<IEnumerable<CategoryDto>> GetCategoriesAsync();
    Task<CategoryDto?> GetCategoryAsync(int categoryId);
    Task<AuctionStatisticsDto> GetStatisticsAsync();
    Task<IEnumerable<AuctionListDto>> GetEndingSoonAuctionsAsync(int count = 10);
    Task<IEnumerable<AuctionListDto>> GetFeaturedAuctionsAsync(int count = 10);
}

public class CachedAuctionService : ICachedAuctionService
{
    private readonly ICacheService _cacheService;
    private readonly ICacheKeyGenerator _keyGenerator;
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly ILogger<CachedAuctionService> _logger;
    private readonly IOptions<CacheSettings> _cacheSettings;

    public CachedAuctionService(
        ICacheService cacheService,
        ICacheKeyGenerator keyGenerator,
        ICacheInvalidationService cacheInvalidationService,
        ILogger<CachedAuctionService> logger,
        IOptions<CacheSettings> cacheSettings)
    {
        _cacheService = cacheService;
        _keyGenerator = keyGenerator;
        _cacheInvalidationService = cacheInvalidationService;
        _logger = logger;
        _cacheSettings = cacheSettings;
    }

    public async Task<AuctionDetailDto?> GetAuctionAsync(int auctionId, string? userId = null)
    {
        try
        {
            var cacheKey = _keyGenerator.GenerateAuctionKey(auctionId);
            var auction = await _cacheService.GetOrSetAsync(
                cacheKey,
                async () => await GetAuctionFromDatabaseAsync(auctionId),
                TimeSpan.FromMinutes(_cacheSettings.Value.AuctionExpirationMinutes)
            );

            if (auction == null)
                return null;

            // Check if user is watching this auction
            if (!string.IsNullOrEmpty(userId))
            {
                var watchlistKey = _keyGenerator.GenerateWatchlistKey(userId);
                var watchlist = await _cacheService.GetAsync<List<int>>(watchlistKey);
                auction.IsWatched = watchlist?.Contains(auctionId) ?? false;
            }

            return auction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting auction {AuctionId} from cache", auctionId);
            return await GetAuctionFromDatabaseAsync(auctionId);
        }
    }

    public async Task<IEnumerable<AuctionListDto>> GetAuctionsAsync(AuctionSearchDto searchDto, string? userId = null)
    {
        try
        {
            var cacheKey = GenerateSearchCacheKey(searchDto);
            var auctions = await _cacheService.GetOrSetAsync(
                cacheKey,
                async () => await GetAuctionsFromDatabaseAsync(searchDto),
                TimeSpan.FromMinutes(_cacheSettings.Value.SearchExpirationMinutes)
            );

            if (!string.IsNullOrEmpty(userId))
            {
                var watchlistKey = _keyGenerator.GenerateWatchlistKey(userId);
                var watchlist = await _cacheService.GetAsync<List<int>>(watchlistKey);
                
                foreach (var auction in auctions)
                {
                    auction.IsWatched = watchlist?.Contains(auction.Id) ?? false;
                }
            }

            return auctions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting auctions from cache");
            return await GetAuctionsFromDatabaseAsync(searchDto);
        }
    }

    public async Task<IEnumerable<AuctionListDto>> GetUserAuctionsAsync(string userId)
    {
        try
        {
            var cacheKey = _keyGenerator.GenerateUserAuctionsKey(userId);
            return await _cacheService.GetOrSetAsync(
                cacheKey,
                async () => await GetUserAuctionsFromDatabaseAsync(userId),
                TimeSpan.FromMinutes(_cacheSettings.Value.UserExpirationMinutes)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user auctions from cache for user {UserId}", userId);
            return await GetUserAuctionsFromDatabaseAsync(userId);
        }
    }

    public async Task<IEnumerable<AuctionListDto>> GetWatchedAuctionsAsync(string userId)
    {
        try
        {
            var watchlistKey = _keyGenerator.GenerateWatchlistKey(userId);
            var watchlist = await _cacheService.GetAsync<List<int>>(watchlistKey);
            
            if (watchlist == null || !watchlist.Any())
                return Enumerable.Empty<AuctionListDto>();

            var auctions = new List<AuctionListDto>();
            foreach (var auctionId in watchlist)
            {
                var auction = await GetAuctionAsync(auctionId, userId);
                if (auction != null)
                {
                    auctions.Add(new AuctionListDto
                    {
                        Id = auction.Id,
                        Title = auction.Title,
                        Description = auction.Description,
                        StartingBid = auction.StartingBid,
                        CurrentBid = auction.CurrentBid,
                        ReservePrice = auction.ReservePrice,
                        StartTime = auction.StartTime,
                        EndTime = auction.EndTime,
                        Status = auction.Status,
                        CreatedBy = auction.CreatedBy,
                        CategoryId = auction.CategoryId,
                        CategoryName = auction.CategoryName,
                        PrimaryImageUrl = auction.Images.FirstOrDefault()?.ImageUrl,
                        BidCount = auction.Bids.Count,
                        IsWatched = true,
                        TimeRemaining = auction.TimeRemaining
                    });
                }
            }

            return auctions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting watched auctions from cache for user {UserId}", userId);
            return Enumerable.Empty<AuctionListDto>();
        }
    }

    public async Task<AuctionDetailDto> CreateAuctionAsync(CreateAuctionDto createDto, string userId)
    {
        try
        {
            var auction = await CreateAuctionInDatabaseAsync(createDto, userId);
            
            // Invalidate related caches
            await _cacheInvalidationService.InvalidateAuctionListCacheAsync();
            await _cacheInvalidationService.InvalidateStatisticsCacheAsync();
            
            _logger.LogInformation("Auction {AuctionId} created by user {UserId}", auction.Id, userId);
            return auction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating auction for user {UserId}", userId);
            throw;
        }
    }

    public async Task<AuctionDetailDto?> UpdateAuctionAsync(int auctionId, UpdateAuctionDto updateDto, string userId)
    {
        try
        {
            var auction = await UpdateAuctionInDatabaseAsync(auctionId, updateDto, userId);
            
            if (auction != null)
            {
                // Invalidate specific auction cache
                await _cacheInvalidationService.InvalidateAuctionCacheAsync(auctionId);
                
                _logger.LogInformation("Auction {AuctionId} updated by user {UserId}", auctionId, userId);
            }
            
            return auction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating auction {AuctionId} for user {UserId}", auctionId, userId);
            throw;
        }
    }

    public async Task<bool> DeleteAuctionAsync(int auctionId, string userId)
    {
        try
        {
            var result = await DeleteAuctionFromDatabaseAsync(auctionId, userId);
            
            if (result)
            {
                // Invalidate related caches
                await _cacheInvalidationService.InvalidateAuctionCacheAsync(auctionId);
                await _cacheInvalidationService.InvalidateAuctionListCacheAsync();
                await _cacheInvalidationService.InvalidateStatisticsCacheAsync();
                
                _logger.LogInformation("Auction {AuctionId} deleted by user {UserId}", auctionId, userId);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting auction {AuctionId} for user {UserId}", auctionId, userId);
            throw;
        }
    }

    public async Task<BidDto> PlaceBidAsync(int auctionId, PlaceBidDto bidDto, string userId)
    {
        try
        {
            var bid = await PlaceBidInDatabaseAsync(auctionId, bidDto, userId);
            
            // Invalidate auction and bid caches
            await _cacheInvalidationService.InvalidateAuctionCacheAsync(auctionId);
            await _cacheInvalidationService.InvalidateBidCacheAsync(auctionId);
            
            _logger.LogInformation("Bid placed on auction {AuctionId} by user {UserId}", auctionId, userId);
            return bid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error placing bid on auction {AuctionId} for user {UserId}", auctionId, userId);
            throw;
        }
    }

    public async Task<IEnumerable<BidDto>> GetAuctionBidsAsync(int auctionId)
    {
        try
        {
            var cacheKey = _keyGenerator.GenerateBidHistoryKey(auctionId);
            return await _cacheService.GetOrSetAsync(
                cacheKey,
                async () => await GetAuctionBidsFromDatabaseAsync(auctionId),
                TimeSpan.FromMinutes(_cacheSettings.Value.AuctionExpirationMinutes)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting auction bids from cache for auction {AuctionId}", auctionId);
            return await GetAuctionBidsFromDatabaseAsync(auctionId);
        }
    }

    public async Task<bool> AddToWatchlistAsync(int auctionId, string userId)
    {
        try
        {
            var result = await AddToWatchlistInDatabaseAsync(auctionId, userId);
            
            if (result)
            {
                // Update watchlist cache
                var watchlistKey = _keyGenerator.GenerateWatchlistKey(userId);
                var watchlist = await _cacheService.GetAsync<List<int>>(watchlistKey) ?? new List<int>();
                if (!watchlist.Contains(auctionId))
                {
                    watchlist.Add(auctionId);
                    await _cacheService.SetAsync(watchlistKey, watchlist, TimeSpan.FromMinutes(_cacheSettings.Value.WatchlistExpirationMinutes));
                }
                
                _logger.LogInformation("Auction {AuctionId} added to watchlist for user {UserId}", auctionId, userId);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding auction {AuctionId} to watchlist for user {UserId}", auctionId, userId);
            throw;
        }
    }

    public async Task<bool> RemoveFromWatchlistAsync(int auctionId, string userId)
    {
        try
        {
            var result = await RemoveFromWatchlistInDatabaseAsync(auctionId, userId);
            
            if (result)
            {
                // Update watchlist cache
                var watchlistKey = _keyGenerator.GenerateWatchlistKey(userId);
                var watchlist = await _cacheService.GetAsync<List<int>>(watchlistKey) ?? new List<int>();
                watchlist.Remove(auctionId);
                await _cacheService.SetAsync(watchlistKey, watchlist, TimeSpan.FromMinutes(_cacheSettings.Value.WatchlistExpirationMinutes));
                
                _logger.LogInformation("Auction {AuctionId} removed from watchlist for user {UserId}", auctionId, userId);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing auction {AuctionId} from watchlist for user {UserId}", auctionId, userId);
            throw;
        }
    }

    public async Task<IEnumerable<CategoryDto>> GetCategoriesAsync()
    {
        try
        {
            var cacheKey = _keyGenerator.GenerateCategoryListKey();
            return await _cacheService.GetOrSetAsync(
                cacheKey,
                async () => await GetCategoriesFromDatabaseAsync(),
                TimeSpan.FromMinutes(_cacheSettings.Value.DefaultExpirationMinutes)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories from cache");
            return await GetCategoriesFromDatabaseAsync();
        }
    }

    public async Task<CategoryDto?> GetCategoryAsync(int categoryId)
    {
        try
        {
            var cacheKey = _keyGenerator.GenerateCategoryKey(categoryId);
            return await _cacheService.GetOrSetAsync(
                cacheKey,
                async () => await GetCategoryFromDatabaseAsync(categoryId),
                TimeSpan.FromMinutes(_cacheSettings.Value.DefaultExpirationMinutes)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category {CategoryId} from cache", categoryId);
            return await GetCategoryFromDatabaseAsync(categoryId);
        }
    }

    public async Task<AuctionStatisticsDto> GetStatisticsAsync()
    {
        try
        {
            var cacheKey = _keyGenerator.GenerateStatisticsKey();
            return await _cacheService.GetOrSetAsync(
                cacheKey,
                async () => await GetStatisticsFromDatabaseAsync(),
                TimeSpan.FromMinutes(_cacheSettings.Value.StatisticsExpirationMinutes)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistics from cache");
            return await GetStatisticsFromDatabaseAsync();
        }
    }

    public async Task<IEnumerable<AuctionListDto>> GetEndingSoonAuctionsAsync(int count = 10)
    {
        try
        {
            var cacheKey = $"auction:ending-soon:{count}";
            return await _cacheService.GetOrSetAsync(
                cacheKey,
                async () => await GetEndingSoonAuctionsFromDatabaseAsync(count),
                TimeSpan.FromMinutes(_cacheSettings.Value.AuctionExpirationMinutes)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ending soon auctions from cache");
            return await GetEndingSoonAuctionsFromDatabaseAsync(count);
        }
    }

    public async Task<IEnumerable<AuctionListDto>> GetFeaturedAuctionsAsync(int count = 10)
    {
        try
        {
            var cacheKey = $"auction:featured:{count}";
            return await _cacheService.GetOrSetAsync(
                cacheKey,
                async () => await GetFeaturedAuctionsFromDatabaseAsync(count),
                TimeSpan.FromMinutes(_cacheSettings.Value.AuctionExpirationMinutes)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting featured auctions from cache");
            return await GetFeaturedAuctionsFromDatabaseAsync(count);
        }
    }

    // Private helper methods for database operations (mock implementations)
    private async Task<AuctionDetailDto?> GetAuctionFromDatabaseAsync(int auctionId)
    {
        // Mock implementation - in real app, this would query the database
        await Task.Delay(10); // Simulate database delay
        return new AuctionDetailDto
        {
            Id = auctionId,
            Title = $"Sample Auction {auctionId}",
            Description = "This is a sample auction description",
            StartingBid = 100.00m,
            CurrentBid = 150.00m,
            StartTime = DateTime.UtcNow.AddDays(-1),
            EndTime = DateTime.UtcNow.AddDays(2),
            Status = AuctionStatus.Active,
            CreatedBy = "sample-user",
            CategoryId = 1,
            CategoryName = "Electronics",
            Images = new List<AuctionImageDto>
            {
                new() { ImageUrl = "https://example.com/image1.jpg", IsPrimary = true }
            },
            Bids = new List<BidDto>
            {
                new() { Amount = 150.00m, BidderName = "bidder1", BidTime = DateTime.UtcNow.AddHours(-1) }
            },
            TimeRemaining = TimeSpan.FromDays(2),
            CanBid = true,
            NextMinimumBid = 160.00m
        };
    }

    private async Task<IEnumerable<AuctionListDto>> GetAuctionsFromDatabaseAsync(AuctionSearchDto searchDto)
    {
        await Task.Delay(50); // Simulate database delay
        return Enumerable.Range(1, searchDto.PageSize).Select(i => new AuctionListDto
        {
            Id = i,
            Title = $"Sample Auction {i}",
            Description = "Sample description",
            StartingBid = 100.00m + i * 10,
            CurrentBid = 150.00m + i * 10,
            StartTime = DateTime.UtcNow.AddDays(-1),
            EndTime = DateTime.UtcNow.AddDays(2),
            Status = AuctionStatus.Active,
            CreatedBy = "sample-user",
            CategoryId = 1,
            CategoryName = "Electronics",
            BidCount = i * 2,
            TimeRemaining = TimeSpan.FromDays(2)
        });
    }

    private async Task<IEnumerable<AuctionListDto>> GetUserAuctionsFromDatabaseAsync(string userId)
    {
        await Task.Delay(30);
        return Enumerable.Range(1, 5).Select(i => new AuctionListDto
        {
            Id = i,
            Title = $"User Auction {i}",
            Description = "User's auction",
            StartingBid = 50.00m + i * 5,
            CurrentBid = 75.00m + i * 5,
            StartTime = DateTime.UtcNow.AddDays(-2),
            EndTime = DateTime.UtcNow.AddDays(1),
            Status = AuctionStatus.Active,
            CreatedBy = userId,
            CategoryId = 1,
            CategoryName = "Electronics",
            BidCount = i,
            TimeRemaining = TimeSpan.FromDays(1)
        });
    }

    private async Task<AuctionDetailDto> CreateAuctionInDatabaseAsync(CreateAuctionDto createDto, string userId)
    {
        await Task.Delay(100);
        return new AuctionDetailDto
        {
            Id = new Random().Next(1000, 9999),
            Title = createDto.Title,
            Description = createDto.Description,
            StartingBid = createDto.StartingBid,
            ReservePrice = createDto.ReservePrice,
            StartTime = createDto.StartTime,
            EndTime = createDto.EndTime,
            Status = AuctionStatus.Scheduled,
            CreatedBy = userId,
            CategoryId = createDto.CategoryId,
            CategoryName = "Electronics",
            Images = createDto.ImageUrls.Select((url, index) => new AuctionImageDto
            {
                ImageUrl = url,
                IsPrimary = index == 0,
                SortOrder = index
            }).ToList(),
            Bids = new List<BidDto>(),
            TimeRemaining = createDto.EndTime - DateTime.UtcNow,
            CanBid = false
        };
    }

    private async Task<AuctionDetailDto?> UpdateAuctionInDatabaseAsync(int auctionId, UpdateAuctionDto updateDto, string userId)
    {
        await Task.Delay(50);
        var auction = await GetAuctionFromDatabaseAsync(auctionId);
        if (auction != null)
        {
            if (!string.IsNullOrEmpty(updateDto.Title))
                auction.Title = updateDto.Title;
            if (!string.IsNullOrEmpty(updateDto.Description))
                auction.Description = updateDto.Description;
            if (updateDto.ReservePrice.HasValue)
                auction.ReservePrice = updateDto.ReservePrice;
            if (updateDto.EndTime.HasValue)
                auction.EndTime = updateDto.EndTime.Value;
            if (updateDto.CategoryId.HasValue)
                auction.CategoryId = updateDto.CategoryId.Value;
        }
        return auction;
    }

    private async Task<bool> DeleteAuctionFromDatabaseAsync(int auctionId, string userId)
    {
        await Task.Delay(30);
        return true; // Mock success
    }

    private async Task<BidDto> PlaceBidInDatabaseAsync(int auctionId, PlaceBidDto bidDto, string userId)
    {
        await Task.Delay(50);
        return new BidDto
        {
            Id = new Random().Next(1000, 9999),
            BidderId = userId,
            BidderName = "Current User",
            Amount = bidDto.Amount,
            BidTime = DateTime.UtcNow,
            IsWinningBid = true,
            IsAutoBid = bidDto.IsAutoBid
        };
    }

    private async Task<IEnumerable<BidDto>> GetAuctionBidsFromDatabaseAsync(int auctionId)
    {
        await Task.Delay(30);
        return Enumerable.Range(1, 5).Select(i => new BidDto
        {
            Id = i,
            BidderId = $"bidder-{i}",
            BidderName = $"Bidder {i}",
            Amount = 100.00m + i * 10,
            BidTime = DateTime.UtcNow.AddHours(-i),
            IsWinningBid = i == 5,
            IsAutoBid = i % 2 == 0
        });
    }

    private async Task<bool> AddToWatchlistInDatabaseAsync(int auctionId, string userId)
    {
        await Task.Delay(20);
        return true;
    }

    private async Task<bool> RemoveFromWatchlistInDatabaseAsync(int auctionId, string userId)
    {
        await Task.Delay(20);
        return true;
    }

    private async Task<IEnumerable<CategoryDto>> GetCategoriesFromDatabaseAsync()
    {
        await Task.Delay(20);
        return new List<CategoryDto>
        {
            new() { Id = 1, Name = "Electronics", Description = "Electronic devices", AuctionCount = 25 },
            new() { Id = 2, Name = "Art", Description = "Art and collectibles", AuctionCount = 15 },
            new() { Id = 3, Name = "Jewelry", Description = "Fine jewelry", AuctionCount = 30 },
            new() { Id = 4, Name = "Antiques", Description = "Antique items", AuctionCount = 20 }
        };
    }

    private async Task<CategoryDto?> GetCategoryFromDatabaseAsync(int categoryId)
    {
        await Task.Delay(10);
        var categories = await GetCategoriesFromDatabaseAsync();
        return categories.FirstOrDefault(c => c.Id == categoryId);
    }

    private async Task<AuctionStatisticsDto> GetStatisticsFromDatabaseAsync()
    {
        await Task.Delay(100);
        return new AuctionStatisticsDto
        {
            TotalAuctions = 100,
            ActiveAuctions = 25,
            EndedAuctions = 60,
            ScheduledAuctions = 15,
            TotalBidValue = 50000.00m,
            AverageBidAmount = 250.00m,
            TotalBids = 200,
            TotalUsers = 150,
            TotalCategories = 4,
            AuctionsByCategory = new Dictionary<string, int>
            {
                { "Electronics", 25 },
                { "Art", 15 },
                { "Jewelry", 30 },
                { "Antiques", 20 }
            }
        };
    }

    private async Task<IEnumerable<AuctionListDto>> GetEndingSoonAuctionsFromDatabaseAsync(int count)
    {
        await Task.Delay(30);
        return Enumerable.Range(1, count).Select(i => new AuctionListDto
        {
            Id = i,
            Title = $"Ending Soon Auction {i}",
            Description = "This auction is ending soon",
            StartingBid = 100.00m,
            CurrentBid = 200.00m,
            StartTime = DateTime.UtcNow.AddDays(-6),
            EndTime = DateTime.UtcNow.AddHours(i),
            Status = AuctionStatus.Active,
            CreatedBy = "sample-user",
            CategoryId = 1,
            CategoryName = "Electronics",
            BidCount = i * 3,
            TimeRemaining = TimeSpan.FromHours(i)
        });
    }

    private async Task<IEnumerable<AuctionListDto>> GetFeaturedAuctionsFromDatabaseAsync(int count)
    {
        await Task.Delay(30);
        return Enumerable.Range(1, count).Select(i => new AuctionListDto
        {
            Id = i + 100,
            Title = $"Featured Auction {i}",
            Description = "This is a featured auction",
            StartingBid = 500.00m,
            CurrentBid = 750.00m,
            StartTime = DateTime.UtcNow.AddDays(-2),
            EndTime = DateTime.UtcNow.AddDays(5),
            Status = AuctionStatus.Active,
            CreatedBy = "featured-user",
            CategoryId = 2,
            CategoryName = "Art",
            BidCount = i * 5,
            TimeRemaining = TimeSpan.FromDays(5)
        });
    }

    private string GenerateSearchCacheKey(AuctionSearchDto searchDto)
    {
        var key = "auction:search";
        if (!string.IsNullOrEmpty(searchDto.SearchTerm))
            key += $":term:{searchDto.SearchTerm.ToLowerInvariant()}";
        if (searchDto.CategoryId.HasValue)
            key += $":category:{searchDto.CategoryId}";
        if (searchDto.Status.HasValue)
            key += $":status:{searchDto.Status}";
        if (searchDto.MinPrice.HasValue)
            key += $":minprice:{searchDto.MinPrice}";
        if (searchDto.MaxPrice.HasValue)
            key += $":maxprice:{searchDto.MaxPrice}";
        key += $":page:{searchDto.Page}:size:{searchDto.PageSize}";
        key += $":sort:{searchDto.SortBy}:{searchDto.SortDirection}";
        return key;
    }
}

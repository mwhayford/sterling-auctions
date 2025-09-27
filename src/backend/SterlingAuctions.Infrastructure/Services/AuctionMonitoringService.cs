using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SterlingAuctions.Core.Entities;
using SterlingAuctions.Infrastructure.Data;

namespace SterlingAuctions.Infrastructure.Services;

/// <summary>
/// Background service for monitoring auction status and automated processes
/// </summary>
public class AuctionMonitoringService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AuctionMonitoringService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Check every minute

    public AuctionMonitoringService(IServiceProvider serviceProvider, ILogger<AuctionMonitoringService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Auction Monitoring Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                await ProcessScheduledAuctions(context, stoppingToken);
                await ProcessEndingAuctions(context, stoppingToken);
                await ProcessExpiredAuctions(context, stoppingToken);

                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in auction monitoring service");
                
                // Wait a bit longer on error to avoid tight error loops
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("Auction Monitoring Service stopped");
    }

    private async Task ProcessScheduledAuctions(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        try
        {
            // Find auctions that should start now
            var auctionsToStart = await context.Auctions
                .Where(a => a.Status == AuctionStatus.Scheduled && 
                           a.StartTime <= DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            foreach (var auction in auctionsToStart)
            {
                auction.Status = AuctionStatus.Active;
                auction.UpdatedAt = DateTime.UtcNow;
                
                _logger.LogInformation("Started auction {AuctionId}: {Title}", auction.Id, auction.Title);
                
                // TODO: Send notifications to registered participants
                // TODO: Update cache
            }

            if (auctionsToStart.Any())
            {
                await context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing scheduled auctions");
        }
    }

    private async Task ProcessEndingAuctions(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        try
        {
            // Find auctions ending soon (within 5 minutes) that haven't sent notifications
            var auctionsEndingSoon = await context.Auctions
                .Where(a => a.Status == AuctionStatus.Active && 
                           a.EndTime <= DateTime.UtcNow.AddMinutes(5) &&
                           a.EndTime > DateTime.UtcNow)
                .ToListAsync(cancellationToken);

            foreach (var auction in auctionsEndingSoon)
            {
                _logger.LogInformation("Auction {AuctionId} ending soon: {Title}", auction.Id, auction.Title);
                
                // TODO: Send ending soon notifications
                // TODO: Check for last-minute bids that might extend the auction
            }

            // Check for auctions that need time extension due to last-minute bids
            await ProcessAuctionExtensions(context, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ending auctions");
        }
    }

    private async Task ProcessExpiredAuctions(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        try
        {
            // Find auctions that should end now
            var auctionsToEnd = await context.Auctions
                .Where(a => a.Status == AuctionStatus.Active && 
                           a.EndTime <= DateTime.UtcNow)
                .Include(a => a.Items)
                    .ThenInclude(i => i.Bids.OrderByDescending(b => b.Amount).Take(1))
                .Include(a => a.Items)
                    .ThenInclude(i => i.Bids)
                        .ThenInclude(b => b.Bidder)
                .ToListAsync(cancellationToken);

            foreach (var auction in auctionsToEnd)
            {
                auction.Status = AuctionStatus.Ended;
                auction.UpdatedAt = DateTime.UtcNow;

                // Process each item to determine winners
                foreach (var item in auction.Items)
                {
                    var winningBid = item.Bids.OrderByDescending(b => b.Amount).FirstOrDefault();
                    if (winningBid != null)
                    {
                        // Update all bids for this item
                        foreach (var bid in item.Bids)
                        {
                            bid.Status = bid.Id == winningBid.Id ? BidStatus.Won : BidStatus.Outbid;
                            bid.IsWinning = bid.Id == winningBid.Id;
                        }

                        _logger.LogInformation("Item {ItemId} won by bidder {BidderId} for {Amount:C}", 
                            item.Id, winningBid.BidderId, winningBid.Amount);
                        
                        // TODO: Send winner notifications
                        // TODO: Generate invoices
                    }
                }

                _logger.LogInformation("Ended auction {AuctionId}: {Title}", auction.Id, auction.Title);
                
                // TODO: Send auction ended notifications to all participants
            }

            if (auctionsToEnd.Any())
            {
                await context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing expired auctions");
        }
    }

    private async Task ProcessAuctionExtensions(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        try
        {
            // Find auctions that might need extension due to recent bids
            var auctionsForExtension = await context.Auctions
                .Where(a => a.Status == AuctionStatus.Active && 
                           a.AutoExtend &&
                           a.EndTime <= DateTime.UtcNow.AddMinutes(a.ExtensionThresholdMinutes) &&
                           a.EndTime > DateTime.UtcNow)
                .Include(a => a.Bids.OrderByDescending(b => b.CreatedAt).Take(1))
                .ToListAsync(cancellationToken);

            foreach (var auction in auctionsForExtension)
            {
                // Check if there's a recent bid within the extension threshold
                var recentBid = auction.Bids
                    .Where(b => b.CreatedAt >= DateTime.UtcNow.AddMinutes(-auction.ExtensionThresholdMinutes))
                    .OrderByDescending(b => b.CreatedAt)
                    .FirstOrDefault();

                if (recentBid != null)
                {
                    // Extend the auction
                    var originalEndTime = auction.EndTime;
                    auction.EndTime = auction.EndTime.AddMinutes(auction.ExtensionTimeMinutes);
                    auction.UpdatedAt = DateTime.UtcNow;

                    _logger.LogInformation("Extended auction {AuctionId} from {OriginalEndTime} to {NewEndTime} due to recent bid", 
                        auction.Id, originalEndTime, auction.EndTime);
                    
                    // TODO: Send extension notifications
                }
            }

            await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing auction extensions");
        }
    }
}

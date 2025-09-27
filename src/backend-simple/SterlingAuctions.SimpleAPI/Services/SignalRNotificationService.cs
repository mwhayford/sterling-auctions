using Microsoft.AspNetCore.SignalR;
using SterlingAuctions.SimpleAPI.Hubs;
using SterlingAuctions.SimpleAPI.Models;
using SterlingAuctions.SimpleAPI.Services;

namespace SterlingAuctions.SimpleAPI.Services;

/// <summary>
/// Service for managing SignalR notifications and real-time communication
/// </summary>
public interface INotificationService
{
    Task NotifyAuctionBidPlacedAsync(int auctionId, BidDto bid, string bidderId);
    Task NotifyAuctionEndedAsync(int auctionId, string winnerId);
    Task NotifyAuctionStartingAsync(int auctionId);
    Task NotifyAuctionCreatedAsync(int auctionId, string sellerId);
    Task NotifyAuctionUpdatedAsync(int auctionId);
    Task NotifyAuctionCancelledAsync(int auctionId, string reason);
    Task NotifyUserWonAuctionAsync(string userId, int auctionId, decimal winningBid);
    Task NotifyUserLostAuctionAsync(string userId, int auctionId, decimal finalBid);
    Task NotifyAuctionEndingSoonAsync(int auctionId, TimeSpan timeRemaining);
    Task NotifySystemAnnouncementAsync(string message, string? targetGroup = null);
    Task NotifyAdminAlertAsync(string message, object? data = null);
    Task NotifyNewAuctionInCategoryAsync(int categoryId, int auctionId);
    Task NotifyAuctionReserveMetAsync(int auctionId, decimal reservePrice);
    Task NotifyAuctionReserveNotMetAsync(int auctionId, decimal reservePrice);
}

public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<AuctionHub> _auctionHub;
    private readonly IHubContext<NotificationHub> _notificationHub;
    private readonly ICachedAuctionService _auctionService;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(
        IHubContext<AuctionHub> auctionHub,
        IHubContext<NotificationHub> notificationHub,
        ICachedAuctionService auctionService,
        ILogger<SignalRNotificationService> logger)
    {
        _auctionHub = auctionHub;
        _notificationHub = notificationHub;
        _auctionService = auctionService;
        _logger = logger;
    }

    public async Task NotifyAuctionBidPlacedAsync(int auctionId, BidDto bid, string bidderId)
    {
        try
        {
            var auctionGroup = $"auction_{auctionId}";
            var auctionWatchersGroup = $"auction_watchers_{auctionId}";
            var auctionNotificationsGroup = $"auction_notifications_{auctionId}";

            var bidNotification = new
            {
                AuctionId = auctionId,
                Bid = bid,
                BidderId = bidderId,
                NewCurrentBid = bid.Amount,
                Timestamp = DateTime.UtcNow,
                Type = "BidPlaced"
            };

            // Notify users in the auction room
            await _auctionHub.Clients.Group(auctionGroup).SendAsync("BidPlaced", bidNotification);

            // Notify auction watchers
            await _auctionHub.Clients.Group(auctionWatchersGroup).SendAsync("AuctionBidUpdate", new
            {
                AuctionId = auctionId,
                NewBid = bid.Amount,
                BidderName = bid.BidderName,
                Timestamp = DateTime.UtcNow
            });

            // Notify users subscribed to auction notifications
            await _notificationHub.Clients.Group(auctionNotificationsGroup).SendAsync("AuctionNotification", new
            {
                Type = "BidPlaced",
                AuctionId = auctionId,
                Message = $"New bid of ${bid.Amount:F2} placed by {bid.BidderName}",
                Data = bidNotification,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Bid notification sent for auction {AuctionId}, bidder {BidderId}, amount {Amount}", 
                auctionId, bidderId, bid.Amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bid notification for auction {AuctionId}", auctionId);
        }
    }

    public async Task NotifyAuctionEndedAsync(int auctionId, string winnerId)
    {
        try
        {
            var auctionGroup = $"auction_{auctionId}";
            var auctionWatchersGroup = $"auction_watchers_{auctionId}";
            var auctionNotificationsGroup = $"auction_notifications_{auctionId}";

            var auction = await _auctionService.GetAuctionAsync(auctionId);
            var endNotification = new
            {
                AuctionId = auctionId,
                WinnerId = winnerId,
                FinalBid = auction?.CurrentBid ?? 0,
                Timestamp = DateTime.UtcNow,
                Type = "AuctionEnded"
            };

            // Notify users in the auction room
            await _auctionHub.Clients.Group(auctionGroup).SendAsync("AuctionEnded", endNotification);

            // Notify auction watchers
            await _auctionHub.Clients.Group(auctionWatchersGroup).SendAsync("AuctionEnded", endNotification);

            // Notify users subscribed to auction notifications
            await _notificationHub.Clients.Group(auctionNotificationsGroup).SendAsync("AuctionNotification", new
            {
                Type = "AuctionEnded",
                AuctionId = auctionId,
                Message = $"Auction ended. Winner: {winnerId}",
                Data = endNotification,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Auction ended notification sent for auction {AuctionId}, winner {WinnerId}", 
                auctionId, winnerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending auction ended notification for auction {AuctionId}", auctionId);
        }
    }

    public async Task NotifyAuctionStartingAsync(int auctionId)
    {
        try
        {
            var auctionGroup = $"auction_{auctionId}";
            var auctionWatchersGroup = $"auction_watchers_{auctionId}";
            var auctionNotificationsGroup = $"auction_notifications_{auctionId}";

            var startNotification = new
            {
                AuctionId = auctionId,
                Timestamp = DateTime.UtcNow,
                Type = "AuctionStarting"
            };

            await _auctionHub.Clients.Group(auctionGroup).SendAsync("AuctionStarting", startNotification);
            await _auctionHub.Clients.Group(auctionWatchersGroup).SendAsync("AuctionStarting", startNotification);
            await _notificationHub.Clients.Group(auctionNotificationsGroup).SendAsync("AuctionNotification", new
            {
                Type = "AuctionStarting",
                AuctionId = auctionId,
                Message = "Auction is now starting!",
                Data = startNotification,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Auction starting notification sent for auction {AuctionId}", auctionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending auction starting notification for auction {AuctionId}", auctionId);
        }
    }

    public async Task NotifyAuctionCreatedAsync(int auctionId, string sellerId)
    {
        try
        {
            var auction = await _auctionService.GetAuctionAsync(auctionId);
            var categoryId = auction?.CategoryId ?? 0;

            var createNotification = new
            {
                AuctionId = auctionId,
                SellerId = sellerId,
                Title = auction?.Title ?? "New Auction",
                CategoryId = categoryId,
                StartingBid = auction?.StartingBid ?? 0,
                Timestamp = DateTime.UtcNow,
                Type = "AuctionCreated"
            };

            // Notify general users
            await _notificationHub.Clients.Group("general_notifications").SendAsync("NewAuction", createNotification);

            // Notify users subscribed to this category
            await _notificationHub.Clients.Group($"category_notifications_{categoryId}")
                .SendAsync("NewAuctionInCategory", createNotification);

            _logger.LogInformation("Auction created notification sent for auction {AuctionId}, seller {SellerId}", 
                auctionId, sellerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending auction created notification for auction {AuctionId}", auctionId);
        }
    }

    public async Task NotifyAuctionUpdatedAsync(int auctionId)
    {
        try
        {
            var auctionGroup = $"auction_{auctionId}";
            var auctionWatchersGroup = $"auction_watchers_{auctionId}";
            var auctionNotificationsGroup = $"auction_notifications_{auctionId}";

            var updateNotification = new
            {
                AuctionId = auctionId,
                Timestamp = DateTime.UtcNow,
                Type = "AuctionUpdated"
            };

            await _auctionHub.Clients.Group(auctionGroup).SendAsync("AuctionUpdated", updateNotification);
            await _auctionHub.Clients.Group(auctionWatchersGroup).SendAsync("AuctionUpdated", updateNotification);
            await _notificationHub.Clients.Group(auctionNotificationsGroup).SendAsync("AuctionNotification", new
            {
                Type = "AuctionUpdated",
                AuctionId = auctionId,
                Message = "Auction details have been updated",
                Data = updateNotification,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Auction updated notification sent for auction {AuctionId}", auctionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending auction updated notification for auction {AuctionId}", auctionId);
        }
    }

    public async Task NotifyAuctionCancelledAsync(int auctionId, string reason)
    {
        try
        {
            var auctionGroup = $"auction_{auctionId}";
            var auctionWatchersGroup = $"auction_watchers_{auctionId}";
            var auctionNotificationsGroup = $"auction_notifications_{auctionId}";

            var cancelNotification = new
            {
                AuctionId = auctionId,
                Reason = reason,
                Timestamp = DateTime.UtcNow,
                Type = "AuctionCancelled"
            };

            await _auctionHub.Clients.Group(auctionGroup).SendAsync("AuctionCancelled", cancelNotification);
            await _auctionHub.Clients.Group(auctionWatchersGroup).SendAsync("AuctionCancelled", cancelNotification);
            await _notificationHub.Clients.Group(auctionNotificationsGroup).SendAsync("AuctionNotification", new
            {
                Type = "AuctionCancelled",
                AuctionId = auctionId,
                Message = $"Auction cancelled: {reason}",
                Data = cancelNotification,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Auction cancelled notification sent for auction {AuctionId}, reason: {Reason}", 
                auctionId, reason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending auction cancelled notification for auction {AuctionId}", auctionId);
        }
    }

    public async Task NotifyUserWonAuctionAsync(string userId, int auctionId, decimal winningBid)
    {
        try
        {
            var userGroup = $"user_notifications_{userId}";
            var auction = await _auctionService.GetAuctionAsync(auctionId);

            var winNotification = new
            {
                AuctionId = auctionId,
                AuctionTitle = auction?.Title ?? "Auction",
                WinningBid = winningBid,
                Timestamp = DateTime.UtcNow,
                Type = "AuctionWon"
            };

            await _notificationHub.Clients.Group(userGroup).SendAsync("AuctionWon", winNotification);

            _logger.LogInformation("Auction won notification sent to user {UserId} for auction {AuctionId}", 
                userId, auctionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending auction won notification to user {UserId} for auction {AuctionId}", 
                userId, auctionId);
        }
    }

    public async Task NotifyUserLostAuctionAsync(string userId, int auctionId, decimal finalBid)
    {
        try
        {
            var userGroup = $"user_notifications_{userId}";
            var auction = await _auctionService.GetAuctionAsync(auctionId);

            var loseNotification = new
            {
                AuctionId = auctionId,
                AuctionTitle = auction?.Title ?? "Auction",
                FinalBid = finalBid,
                Timestamp = DateTime.UtcNow,
                Type = "AuctionLost"
            };

            await _notificationHub.Clients.Group(userGroup).SendAsync("AuctionLost", loseNotification);

            _logger.LogInformation("Auction lost notification sent to user {UserId} for auction {AuctionId}", 
                userId, auctionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending auction lost notification to user {UserId} for auction {AuctionId}", 
                userId, auctionId);
        }
    }

    public async Task NotifyAuctionEndingSoonAsync(int auctionId, TimeSpan timeRemaining)
    {
        try
        {
            var auctionGroup = $"auction_{auctionId}";
            var auctionWatchersGroup = $"auction_watchers_{auctionId}";
            var auctionNotificationsGroup = $"auction_notifications_{auctionId}";
            var endingSoonGroup = "ending_soon_notifications";

            var endingSoonNotification = new
            {
                AuctionId = auctionId,
                TimeRemaining = timeRemaining,
                MinutesRemaining = (int)timeRemaining.TotalMinutes,
                Timestamp = DateTime.UtcNow,
                Type = "AuctionEndingSoon"
            };

            await _auctionHub.Clients.Group(auctionGroup).SendAsync("AuctionEndingSoon", endingSoonNotification);
            await _auctionHub.Clients.Group(auctionWatchersGroup).SendAsync("AuctionEndingSoon", endingSoonNotification);
            await _notificationHub.Clients.Group(auctionNotificationsGroup).SendAsync("AuctionNotification", new
            {
                Type = "AuctionEndingSoon",
                AuctionId = auctionId,
                Message = $"Auction ending in {timeRemaining.TotalMinutes:F0} minutes",
                Data = endingSoonNotification,
                Timestamp = DateTime.UtcNow
            });
            await _notificationHub.Clients.Group(endingSoonGroup).SendAsync("AuctionEndingSoon", endingSoonNotification);

            _logger.LogInformation("Auction ending soon notification sent for auction {AuctionId}, {MinutesRemaining} minutes remaining", 
                auctionId, endingSoonNotification.MinutesRemaining);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending auction ending soon notification for auction {AuctionId}", auctionId);
        }
    }

    public async Task NotifySystemAnnouncementAsync(string message, string? targetGroup = null)
    {
        try
        {
            var announcement = new
            {
                Message = message,
                Timestamp = DateTime.UtcNow,
                Type = "SystemAnnouncement"
            };

            if (string.IsNullOrEmpty(targetGroup))
            {
                await _notificationHub.Clients.Group("general_notifications").SendAsync("SystemAnnouncement", announcement);
            }
            else
            {
                await _notificationHub.Clients.Group(targetGroup).SendAsync("SystemAnnouncement", announcement);
            }

            _logger.LogInformation("System announcement sent: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending system announcement: {Message}", message);
        }
    }

    public async Task NotifyAdminAlertAsync(string message, object? data = null)
    {
        try
        {
            var alert = new
            {
                Message = message,
                Data = data,
                Timestamp = DateTime.UtcNow,
                Type = "AdminAlert"
            };

            await _notificationHub.Clients.Group("admin_notifications").SendAsync("AdminAlert", alert);

            _logger.LogInformation("Admin alert sent: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending admin alert: {Message}", message);
        }
    }

    public async Task NotifyNewAuctionInCategoryAsync(int categoryId, int auctionId)
    {
        try
        {
            var auction = await _auctionService.GetAuctionAsync(auctionId);
            var categoryNotification = new
            {
                CategoryId = categoryId,
                AuctionId = auctionId,
                AuctionTitle = auction?.Title ?? "New Auction",
                StartingBid = auction?.StartingBid ?? 0,
                Timestamp = DateTime.UtcNow,
                Type = "NewAuctionInCategory"
            };

            await _notificationHub.Clients.Group($"category_notifications_{categoryId}")
                .SendAsync("NewAuctionInCategory", categoryNotification);

            _logger.LogInformation("New auction in category notification sent for category {CategoryId}, auction {AuctionId}", 
                categoryId, auctionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending new auction in category notification for category {CategoryId}", categoryId);
        }
    }

    public async Task NotifyAuctionReserveMetAsync(int auctionId, decimal reservePrice)
    {
        try
        {
            var auctionGroup = $"auction_{auctionId}";
            var auctionWatchersGroup = $"auction_watchers_{auctionId}";
            var auctionNotificationsGroup = $"auction_notifications_{auctionId}";

            var reserveMetNotification = new
            {
                AuctionId = auctionId,
                ReservePrice = reservePrice,
                Timestamp = DateTime.UtcNow,
                Type = "ReserveMet"
            };

            await _auctionHub.Clients.Group(auctionGroup).SendAsync("ReserveMet", reserveMetNotification);
            await _auctionHub.Clients.Group(auctionWatchersGroup).SendAsync("ReserveMet", reserveMetNotification);
            await _notificationHub.Clients.Group(auctionNotificationsGroup).SendAsync("AuctionNotification", new
            {
                Type = "ReserveMet",
                AuctionId = auctionId,
                Message = $"Reserve price of ${reservePrice:F2} has been met!",
                Data = reserveMetNotification,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Reserve met notification sent for auction {AuctionId}, reserve price {ReservePrice}", 
                auctionId, reservePrice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending reserve met notification for auction {AuctionId}", auctionId);
        }
    }

    public async Task NotifyAuctionReserveNotMetAsync(int auctionId, decimal reservePrice)
    {
        try
        {
            var auctionGroup = $"auction_{auctionId}";
            var auctionWatchersGroup = $"auction_watchers_{auctionId}";
            var auctionNotificationsGroup = $"auction_notifications_{auctionId}";

            var reserveNotMetNotification = new
            {
                AuctionId = auctionId,
                ReservePrice = reservePrice,
                Timestamp = DateTime.UtcNow,
                Type = "ReserveNotMet"
            };

            await _auctionHub.Clients.Group(auctionGroup).SendAsync("ReserveNotMet", reserveNotMetNotification);
            await _auctionHub.Clients.Group(auctionWatchersGroup).SendAsync("ReserveNotMet", reserveNotMetNotification);
            await _notificationHub.Clients.Group(auctionNotificationsGroup).SendAsync("AuctionNotification", new
            {
                Type = "ReserveNotMet",
                AuctionId = auctionId,
                Message = $"Reserve price of ${reservePrice:F2} was not met",
                Data = reserveNotMetNotification,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Reserve not met notification sent for auction {AuctionId}, reserve price {ReservePrice}", 
                auctionId, reservePrice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending reserve not met notification for auction {AuctionId}", auctionId);
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SterlingAuctions.SimpleAPI.Models;
using SterlingAuctions.SimpleAPI.Services;
using System.Security.Claims;

namespace SterlingAuctions.SimpleAPI.Hubs;

/// <summary>
/// SignalR hub for system-wide notifications and announcements
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ICachedAuctionService _auctionService;
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(
        ICachedAuctionService auctionService,
        ILogger<NotificationHub> logger)
    {
        _auctionService = auctionService;
        _logger = logger;
    }

    /// <summary>
    /// Join the general notifications group for all users
    /// </summary>
    public async Task JoinGeneralNotifications()
    {
        var userId = GetUserId();
        var groupName = "general_notifications";
        
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserId} joined general notifications group", userId);
        
        await Clients.Caller.SendAsync("JoinedGeneralNotifications", new
        {
            Message = "You are now receiving general notifications",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Leave the general notifications group
    /// </summary>
    public async Task LeaveGeneralNotifications()
    {
        var userId = GetUserId();
        var groupName = "general_notifications";
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserId} left general notifications group", userId);
    }

    /// <summary>
    /// Join the admin notifications group
    /// </summary>
    public async Task JoinAdminNotifications()
    {
        var userId = GetUserId();
        var userRole = GetUserRole();
        
        if (userRole != "Admin")
        {
            await Clients.Caller.SendAsync("Error", "Unauthorized: Admin role required");
            return;
        }
        
        var groupName = "admin_notifications";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("Admin user {UserId} joined admin notification group", userId);
        
        await Clients.Caller.SendAsync("JoinedAdminNotifications", new
        {
            Message = "You are now receiving admin notifications",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Join the seller notifications group
    /// </summary>
    public async Task JoinSellerNotifications()
    {
        var userId = GetUserId();
        var groupName = "seller_notifications";
        
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserId} joined seller notifications group", userId);
        
        await Clients.Caller.SendAsync("JoinedSellerNotifications", new
        {
            Message = "You are now receiving seller notifications",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Join the bidder notifications group
    /// </summary>
    public async Task JoinBidderNotifications()
    {
        var userId = GetUserId();
        var groupName = "bidder_notifications";
        
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserId} joined bidder notifications group", userId);
        
        await Clients.Caller.SendAsync("JoinedBidderNotifications", new
        {
            Message = "You are now receiving bidder notifications",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Subscribe to notifications for a specific auction
    /// </summary>
    public async Task SubscribeToAuction(int auctionId)
    {
        var userId = GetUserId();
        var groupName = $"auction_notifications_{auctionId}";
        
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserId} subscribed to notifications for auction {AuctionId}", userId, auctionId);
        
        await Clients.Caller.SendAsync("SubscribedToAuction", new
        {
            AuctionId = auctionId,
            Message = $"You are now receiving notifications for auction {auctionId}",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Unsubscribe from notifications for a specific auction
    /// </summary>
    public async Task UnsubscribeFromAuction(int auctionId)
    {
        var userId = GetUserId();
        var groupName = $"auction_notifications_{auctionId}";
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserId} unsubscribed from notifications for auction {AuctionId}", userId, auctionId);
        
        await Clients.Caller.SendAsync("UnsubscribedFromAuction", new
        {
            AuctionId = auctionId,
            Message = $"You are no longer receiving notifications for auction {auctionId}",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Subscribe to notifications for auctions ending soon
    /// </summary>
    public async Task SubscribeToEndingSoon()
    {
        var userId = GetUserId();
        var groupName = "ending_soon_notifications";
        
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserId} subscribed to ending soon notifications", userId);
        
        await Clients.Caller.SendAsync("SubscribedToEndingSoon", new
        {
            Message = "You are now receiving notifications for auctions ending soon",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Subscribe to notifications for new auctions in specific categories
    /// </summary>
    public async Task SubscribeToCategory(int categoryId)
    {
        var userId = GetUserId();
        var groupName = $"category_notifications_{categoryId}";
        
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserId} subscribed to notifications for category {CategoryId}", userId, categoryId);
        
        await Clients.Caller.SendAsync("SubscribedToCategory", new
        {
            CategoryId = categoryId,
            Message = $"You are now receiving notifications for category {categoryId}",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get current notification preferences
    /// </summary>
    public async Task GetNotificationPreferences()
    {
        var userId = GetUserId();
        
        // In a real implementation, this would fetch from database
        var preferences = new
        {
            UserId = userId,
            GeneralNotifications = true,
            EmailNotifications = true,
            PushNotifications = true,
            AuctionEndingSoon = true,
            NewBids = true,
            AuctionWon = true,
            AuctionLost = true,
            NewAuctions = false,
            CategoryNotifications = new List<int>()
        };
        
        await Clients.Caller.SendAsync("NotificationPreferences", preferences);
    }

    /// <summary>
    /// Update notification preferences
    /// </summary>
    public async Task UpdateNotificationPreferences(object preferences)
    {
        var userId = GetUserId();
        
        _logger.LogInformation("User {UserId} updated notification preferences", userId);
        
        await Clients.Caller.SendAsync("NotificationPreferencesUpdated", new
        {
            Message = "Notification preferences updated successfully",
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Client connection established
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var connectionId = Context.ConnectionId;
        
        _logger.LogInformation("User {UserId} connected to notification hub with connection {ConnectionId}", userId, connectionId);
        
        // Automatically join general notifications
        await Groups.AddToGroupAsync(connectionId, "general_notifications");
        
        // Send welcome message
        await Clients.Caller.SendAsync("Connected", new
        {
            UserId = userId,
            ConnectionId = connectionId,
            Message = "Connected to notification hub",
            Timestamp = DateTime.UtcNow
        });
        
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Client connection terminated
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        var connectionId = Context.ConnectionId;
        
        _logger.LogInformation("User {UserId} disconnected from notification hub with connection {ConnectionId}", userId, connectionId);
        
        if (exception != null)
        {
            _logger.LogWarning(exception, "User {UserId} disconnected from notification hub with exception", userId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    #region Helper Methods

    private string GetUserId()
    {
        return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
    }

    private string GetUserRole()
    {
        return Context.User?.FindFirst(ClaimTypes.Role)?.Value ?? "User";
    }

    #endregion
}

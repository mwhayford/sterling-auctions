using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SterlingAuctions.SimpleAPI.Models;
using SterlingAuctions.SimpleAPI.Services;
using System.Security.Claims;

namespace SterlingAuctions.SimpleAPI.Hubs;

/// <summary>
/// SignalR hub for real-time auction operations including bidding, notifications, and updates
/// </summary>
[Authorize]
public class AuctionHub : Hub
{
    private readonly ICachedAuctionService _auctionService;
    private readonly ISessionService _sessionService;
    private readonly ILogger<AuctionHub> _logger;

    public AuctionHub(
        ICachedAuctionService auctionService,
        ISessionService sessionService,
        ILogger<AuctionHub> logger)
    {
        _auctionService = auctionService;
        _sessionService = sessionService;
        _logger = logger;
    }

    /// <summary>
    /// Join a specific auction room to receive real-time updates
    /// </summary>
    public async Task JoinAuction(int auctionId)
    {
        var userId = GetUserId();
        var groupName = GetAuctionGroupName(auctionId);
        
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        // Add user to auction watchers group
        await Groups.AddToGroupAsync(Context.ConnectionId, $"auction_watchers_{auctionId}");
        
        _logger.LogInformation("User {UserId} joined auction {AuctionId} room", userId, auctionId);
        
        // Send current auction state to the user
            var auction = await _auctionService.GetAuctionAsync(auctionId, userId);
        if (auction != null)
        {
            await Clients.Caller.SendAsync("AuctionJoined", auction);
        }
        
        // Notify other users that someone joined
        await Clients.GroupExcept(groupName, Context.ConnectionId)
            .SendAsync("UserJoinedAuction", new { UserId = userId, AuctionId = auctionId });
    }

    /// <summary>
    /// Leave a specific auction room
    /// </summary>
    public async Task LeaveAuction(int auctionId)
    {
        var userId = GetUserId();
        var groupName = GetAuctionGroupName(auctionId);
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"auction_watchers_{auctionId}");
        
        _logger.LogInformation("User {UserId} left auction {AuctionId} room", userId, auctionId);
        
        // Notify other users that someone left
        await Clients.GroupExcept(groupName, Context.ConnectionId)
            .SendAsync("UserLeftAuction", new { UserId = userId, AuctionId = auctionId });
    }

    /// <summary>
    /// Place a bid on an auction with real-time updates
    /// </summary>
    public async Task PlaceBid(int auctionId, decimal amount)
    {
        var userId = GetUserId();
        var groupName = GetAuctionGroupName(auctionId);
        
        try
        {
            var placeBidDto = new PlaceBidDto { Amount = amount };
            var bid = await _auctionService.PlaceBidAsync(auctionId, placeBidDto, userId);
            
            if (bid != null)
            {
                // Broadcast the new bid to all users in the auction room
                await Clients.Group(groupName).SendAsync("BidPlaced", new
                {
                    AuctionId = auctionId,
                    Bid = bid,
                    NewCurrentBid = bid.Amount,
                    BidderId = userId,
                    Timestamp = DateTime.UtcNow
                });
                
                // Send notification to auction watchers
                await Clients.Group($"auction_watchers_{auctionId}")
                    .SendAsync("AuctionBidUpdate", new
                    {
                        AuctionId = auctionId,
                        NewBid = bid.Amount,
                        BidderName = bid.BidderName,
                        Timestamp = DateTime.UtcNow
                    });
                
                _logger.LogInformation("Bid placed by user {UserId} on auction {AuctionId} for amount {Amount}", 
                    userId, auctionId, amount);
            }
            else
            {
                await Clients.Caller.SendAsync("BidFailed", new
                {
                    AuctionId = auctionId,
                    Message = "Failed to place bid. Please check the auction status and bid amount."
                });
            }
        }
        catch (InvalidOperationException ex)
        {
            await Clients.Caller.SendAsync("BidFailed", new
            {
                AuctionId = auctionId,
                Message = ex.Message
            });
            
            _logger.LogWarning("Bid failed for user {UserId} on auction {AuctionId}: {Error}", 
                userId, auctionId, ex.Message);
        }
    }

    /// <summary>
    /// Join the user's personal notification group
    /// </summary>
    public async Task JoinUserNotifications()
    {
        var userId = GetUserId();
        var groupName = GetUserNotificationGroupName(userId);
        
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserId} joined notification group", userId);
    }

    /// <summary>
    /// Leave the user's personal notification group
    /// </summary>
    public async Task LeaveUserNotifications()
    {
        var userId = GetUserId();
        var groupName = GetUserNotificationGroupName(userId);
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogInformation("User {UserId} left notification group", userId);
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
    }

    /// <summary>
    /// Send a message to all users in an auction room
    /// </summary>
    public async Task SendAuctionMessage(int auctionId, string message)
    {
        var userId = GetUserId();
        var groupName = GetAuctionGroupName(auctionId);
        
        var messageData = new
        {
            AuctionId = auctionId,
            UserId = userId,
            Message = message,
            Timestamp = DateTime.UtcNow
        };
        
        await Clients.Group(groupName).SendAsync("AuctionMessage", messageData);
        
        _logger.LogInformation("Message sent by user {UserId} in auction {AuctionId}: {Message}", 
            userId, auctionId, message);
    }

    /// <summary>
    /// Get current auction statistics
    /// </summary>
    public async Task GetAuctionStats()
    {
        try
        {
            var stats = await _auctionService.GetStatisticsAsync();
            await Clients.Caller.SendAsync("AuctionStats", stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving auction statistics");
            await Clients.Caller.SendAsync("Error", "Failed to retrieve auction statistics");
        }
    }

    /// <summary>
    /// Client connection established
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var connectionId = Context.ConnectionId;
        
        _logger.LogInformation("User {UserId} connected with connection {ConnectionId}", userId, connectionId);
        
        // Add user to their personal group for notifications
        await Groups.AddToGroupAsync(connectionId, GetUserNotificationGroupName(userId));
        
        // Send welcome message
        await Clients.Caller.SendAsync("Connected", new
        {
            UserId = userId,
            ConnectionId = connectionId,
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
        
        _logger.LogInformation("User {UserId} disconnected from connection {ConnectionId}", userId, connectionId);
        
        if (exception != null)
        {
            _logger.LogWarning(exception, "User {UserId} disconnected with exception", userId);
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

    private static string GetAuctionGroupName(int auctionId)
    {
        return $"auction_{auctionId}";
    }

    private static string GetUserNotificationGroupName(string userId)
    {
        return $"user_notifications_{userId}";
    }

    #endregion
}

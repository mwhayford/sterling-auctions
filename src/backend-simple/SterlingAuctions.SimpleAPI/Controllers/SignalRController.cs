using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SterlingAuctions.SimpleAPI.Hubs;
using SterlingAuctions.SimpleAPI.Models;
using SterlingAuctions.SimpleAPI.Services;
using System.Security.Claims;

namespace SterlingAuctions.SimpleAPI.Controllers;

/// <summary>
/// Controller for SignalR hub testing and management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SignalRController : ControllerBase
{
    private readonly IHubContext<AuctionHub> _auctionHub;
    private readonly IHubContext<NotificationHub> _notificationHub;
    private readonly INotificationService _notificationService;
    private readonly ICachedAuctionService _auctionService;
    private readonly ILogger<SignalRController> _logger;

    public SignalRController(
        IHubContext<AuctionHub> auctionHub,
        IHubContext<NotificationHub> notificationHub,
        INotificationService notificationService,
        ICachedAuctionService auctionService,
        ILogger<SignalRController> logger)
    {
        _auctionHub = auctionHub;
        _notificationHub = notificationHub;
        _notificationService = notificationService;
        _auctionService = auctionService;
        _logger = logger;
    }

    /// <summary>
    /// Test SignalR connection by sending a test message
    /// </summary>
    [HttpPost("test-connection")]
    public async Task<IActionResult> TestConnection()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var testMessage = new
        {
            Message = "SignalR connection test successful",
            UserId = userId,
            Timestamp = DateTime.UtcNow
        };

        // Send to user's personal notification group
        await _notificationHub.Clients.Group($"user_notifications_{userId}")
            .SendAsync("TestMessage", testMessage);

        _logger.LogInformation("SignalR test message sent to user {UserId}", userId);

        return Ok(new { message = "Test message sent successfully", data = testMessage });
    }

    /// <summary>
    /// Send a test bid notification for a specific auction
    /// </summary>
    [HttpPost("test-bid-notification/{auctionId}")]
    public async Task<IActionResult> TestBidNotification(int auctionId, [FromBody] TestBidRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        try
        {
            // Create a mock bid for testing
            var mockBid = new BidDto
            {
                Id = 999,
                BidderName = "Test User",
                Amount = request.Amount,
                BidTime = DateTime.UtcNow
            };

            // Send the notification
            await _notificationService.NotifyAuctionBidPlacedAsync(auctionId, mockBid, userId);

            _logger.LogInformation("Test bid notification sent for auction {AuctionId} by user {UserId}", auctionId, userId);

            return Ok(new { message = "Test bid notification sent successfully", auctionId, bid = mockBid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test bid notification for auction {AuctionId}", auctionId);
            return BadRequest(new { message = "Failed to send test notification", error = ex.Message });
        }
    }

    /// <summary>
    /// Send a system announcement to all users
    /// </summary>
    [HttpPost("send-announcement")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendSystemAnnouncement([FromBody] AnnouncementRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        try
        {
            await _notificationService.NotifySystemAnnouncementAsync(request.Message, request.TargetGroup);

            _logger.LogInformation("System announcement sent by admin {UserId}: {Message}", userId, request.Message);

            return Ok(new { message = "System announcement sent successfully", announcement = request });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending system announcement by admin {UserId}", userId);
            return BadRequest(new { message = "Failed to send announcement", error = ex.Message });
        }
    }

    /// <summary>
    /// Send an admin alert
    /// </summary>
    [HttpPost("send-admin-alert")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendAdminAlert([FromBody] AdminAlertRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        try
        {
            await _notificationService.NotifyAdminAlertAsync(request.Message, request.Data);

            _logger.LogInformation("Admin alert sent by {UserId}: {Message}", userId, request.Message);

            return Ok(new { message = "Admin alert sent successfully", alert = request });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending admin alert by {UserId}", userId);
            return BadRequest(new { message = "Failed to send admin alert", error = ex.Message });
        }
    }

    /// <summary>
    /// Test auction ending soon notification
    /// </summary>
    [HttpPost("test-ending-soon/{auctionId}")]
    public async Task<IActionResult> TestEndingSoonNotification(int auctionId, [FromBody] EndingSoonRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        try
        {
            var timeRemaining = TimeSpan.FromMinutes(request.MinutesRemaining);
            await _notificationService.NotifyAuctionEndingSoonAsync(auctionId, timeRemaining);

            _logger.LogInformation("Test ending soon notification sent for auction {AuctionId} by user {UserId}", auctionId, userId);

            return Ok(new { 
                message = "Test ending soon notification sent successfully", 
                auctionId, 
                minutesRemaining = request.MinutesRemaining 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test ending soon notification for auction {AuctionId}", auctionId);
            return BadRequest(new { message = "Failed to send test notification", error = ex.Message });
        }
    }

    /// <summary>
    /// Get SignalR hub connection information
    /// </summary>
    [HttpGet("connection-info")]
    public IActionResult GetConnectionInfo()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";
        
        var connectionInfo = new
        {
            UserId = userId,
            UserRole = userRole,
            AuctionHubUrl = "/auctionHub",
            NotificationHubUrl = "/notificationHub",
            AvailableGroups = new[]
            {
                $"user_notifications_{userId}",
                "general_notifications",
                userRole == "Admin" ? "admin_notifications" : null,
                "seller_notifications",
                "bidder_notifications"
            }.Where(g => g != null),
            AvailableMethods = new
            {
                AuctionHub = new[]
                {
                    "JoinAuction",
                    "LeaveAuction", 
                    "PlaceBid",
                    "JoinUserNotifications",
                    "SendAuctionMessage",
                    "GetAuctionStats"
                },
                NotificationHub = new[]
                {
                    "JoinGeneralNotifications",
                    "JoinAdminNotifications",
                    "JoinSellerNotifications",
                    "JoinBidderNotifications",
                    "SubscribeToAuction",
                    "SubscribeToEndingSoon",
                    "GetNotificationPreferences"
                }
            },
            Timestamp = DateTime.UtcNow
        };

        return Ok(connectionInfo);
    }
}

/// <summary>
/// Request model for test bid notifications
/// </summary>
public class TestBidRequest
{
    public decimal Amount { get; set; }
}

/// <summary>
/// Request model for system announcements
/// </summary>
public class AnnouncementRequest
{
    public string Message { get; set; } = string.Empty;
    public string? TargetGroup { get; set; }
}

/// <summary>
/// Request model for admin alerts
/// </summary>
public class AdminAlertRequest
{
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
}

/// <summary>
/// Request model for ending soon notifications
/// </summary>
public class EndingSoonRequest
{
    public int MinutesRemaining { get; set; }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SterlingAuctions.SimpleAPI.Models;
using SterlingAuctions.SimpleAPI.Services;
using System.Security.Claims;

namespace SterlingAuctions.SimpleAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PushNotificationController : ControllerBase
{
    private readonly IPushNotificationService _pushNotificationService;
    private readonly ILogger<PushNotificationController> _logger;

    public PushNotificationController(
        IPushNotificationService pushNotificationService,
        ILogger<PushNotificationController> logger)
    {
        _pushNotificationService = pushNotificationService;
        _logger = logger;
    }

    private string GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
               throw new UnauthorizedAccessException("User ID not found.");
    }

    /// <summary>
    /// Subscribe user to push notifications
    /// </summary>
    /// <param name="subscription">Push subscription details</param>
    /// <returns>Success status</returns>
    [HttpPost("subscribe")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Subscribe([FromBody] PushSubscriptionDto subscription)
    {
        try
        {
            var userId = GetUserId();
            var success = await _pushNotificationService.SubscribeUserAsync(userId, subscription);
            
            if (success)
            {
                return Ok(new { message = "Successfully subscribed to push notifications" });
            }
            
            return BadRequest(new { message = "Failed to subscribe to push notifications" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt to subscribe to push notifications");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to push notifications");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error subscribing to push notifications" });
        }
    }

    /// <summary>
    /// Unsubscribe user from push notifications
    /// </summary>
    /// <param name="endpoint">Push subscription endpoint to unsubscribe</param>
    /// <returns>Success status</returns>
    [HttpPost("unsubscribe")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Unsubscribe([FromBody] string endpoint)
    {
        try
        {
            var userId = GetUserId();
            var success = await _pushNotificationService.UnsubscribeUserAsync(userId, endpoint);
            
            if (success)
            {
                return Ok(new { message = "Successfully unsubscribed from push notifications" });
            }
            
            return BadRequest(new { message = "Failed to unsubscribe from push notifications" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt to unsubscribe from push notifications");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from push notifications");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error unsubscribing from push notifications" });
        }
    }

    /// <summary>
    /// Unsubscribe user from all push notifications
    /// </summary>
    /// <returns>Success status</returns>
    [HttpPost("unsubscribe-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UnsubscribeAll()
    {
        try
        {
            var userId = GetUserId();
            var success = await _pushNotificationService.UnsubscribeAllUserAsync(userId);
            
            if (success)
            {
                return Ok(new { message = "Successfully unsubscribed from all push notifications" });
            }
            
            return BadRequest(new { message = "Failed to unsubscribe from all push notifications" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt to unsubscribe from all push notifications");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from all push notifications");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error unsubscribing from all push notifications" });
        }
    }

    /// <summary>
    /// Get user's push notification subscriptions
    /// </summary>
    /// <returns>List of subscriptions</returns>
    [HttpGet("subscriptions")]
    [ProducesResponseType(typeof(IEnumerable<PushNotificationSubscription>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSubscriptions()
    {
        try
        {
            var userId = GetUserId();
            var subscriptions = await _pushNotificationService.GetUserSubscriptionsAsync(userId);
            return Ok(subscriptions);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt to get push notification subscriptions");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting push notification subscriptions");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error getting push notification subscriptions" });
        }
    }

    /// <summary>
    /// Check if user is subscribed to push notifications
    /// </summary>
    /// <returns>Subscription status</returns>
    [HttpGet("is-subscribed")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> IsSubscribed()
    {
        try
        {
            var userId = GetUserId();
            var isSubscribed = await _pushNotificationService.IsUserSubscribedAsync(userId);
            return Ok(new { isSubscribed });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt to check push notification subscription");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking push notification subscription");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error checking push notification subscription" });
        }
    }

    /// <summary>
    /// Send a push notification to a specific user
    /// </summary>
    /// <param name="notification">Notification details</param>
    /// <returns>Success status</returns>
    [HttpPost("send")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendNotification([FromBody] SendPushNotificationDto notification)
    {
        try
        {
            var success = await _pushNotificationService.SendNotificationAsync(notification);
            
            if (success)
            {
                return Ok(new { message = "Push notification sent successfully" });
            }
            
            return BadRequest(new { message = "Failed to send push notification" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt to send push notification");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error sending push notification" });
        }
    }

    /// <summary>
    /// Send a push notification to all users
    /// </summary>
    /// <param name="notification">Notification details</param>
    /// <returns>Success status</returns>
    [HttpPost("send-to-all")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SendToAll([FromBody] SendPushNotificationDto notification)
    {
        try
        {
            var success = await _pushNotificationService.SendNotificationToAllUsersAsync(notification);
            
            if (success)
            {
                return Ok(new { message = "Push notification sent to all users successfully" });
            }
            
            return BadRequest(new { message = "Failed to send push notification to all users" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt to send push notification to all users");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to all users");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error sending push notification to all users" });
        }
    }

    /// <summary>
    /// Get user's push notification history
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of notifications</returns>
    [HttpGet("history")]
    [ProducesResponseType(typeof(IEnumerable<PushNotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotificationHistory(int page = 1, int pageSize = 20)
    {
        try
        {
            var userId = GetUserId();
            var notifications = await _pushNotificationService.GetUserNotificationsAsync(userId, page, pageSize);
            return Ok(notifications);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt to get push notification history");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting push notification history");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error getting push notification history" });
        }
    }

    /// <summary>
    /// Mark a notification as clicked
    /// </summary>
    /// <param name="notificationId">Notification ID</param>
    /// <returns>Success status</returns>
    [HttpPost("{notificationId}/click")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsClicked(int notificationId)
    {
        try
        {
            var success = await _pushNotificationService.MarkNotificationAsClickedAsync(notificationId);
            
            if (success)
            {
                return Ok(new { message = "Notification marked as clicked" });
            }
            
            return NotFound(new { message = "Notification not found" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt to mark notification as clicked");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as clicked");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error marking notification as clicked" });
        }
    }

    /// <summary>
    /// Get user's push notification preferences
    /// </summary>
    /// <returns>User preferences</returns>
    [HttpGet("preferences")]
    [ProducesResponseType(typeof(PushNotificationPreferencesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPreferences()
    {
        try
        {
            var userId = GetUserId();
            var preferences = await _pushNotificationService.GetUserPreferencesAsync(userId);
            return Ok(preferences);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt to get push notification preferences");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting push notification preferences");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error getting push notification preferences" });
        }
    }

    /// <summary>
    /// Update user's push notification preferences
    /// </summary>
    /// <param name="preferences">Updated preferences</param>
    /// <returns>Success status</returns>
    [HttpPut("preferences")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdatePreferences([FromBody] PushNotificationPreferencesDto preferences)
    {
        try
        {
            var userId = GetUserId();
            var success = await _pushNotificationService.UpdateUserPreferencesAsync(userId, preferences);
            
            if (success)
            {
                return Ok(new { message = "Preferences updated successfully" });
            }
            
            return BadRequest(new { message = "Failed to update preferences" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt to update push notification preferences");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating push notification preferences");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error updating push notification preferences" });
        }
    }

    /// <summary>
    /// Get push notification statistics
    /// </summary>
    /// <returns>Statistics</returns>
    [HttpGet("statistics")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(PushNotificationStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var statistics = await _pushNotificationService.GetStatisticsAsync();
            return Ok(statistics);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt to get push notification statistics");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting push notification statistics");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error getting push notification statistics" });
        }
    }

    /// <summary>
    /// Get user's push notification statistics
    /// </summary>
    /// <returns>User statistics</returns>
    [HttpGet("statistics/user")]
    [ProducesResponseType(typeof(PushNotificationStatisticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserStatistics()
    {
        try
        {
            var userId = GetUserId();
            var statistics = await _pushNotificationService.GetUserStatisticsAsync(userId);
            return Ok(statistics);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt to get user push notification statistics");
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user push notification statistics");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { message = "Error getting user push notification statistics" });
        }
    }
}

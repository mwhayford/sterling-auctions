using SterlingAuctions.SimpleAPI.Models;
using SterlingAuctions.SimpleAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace SterlingAuctions.SimpleAPI.Services;

/// <summary>
/// Push notification service interface
/// </summary>
public interface IPushNotificationService
{
    // Subscription Management
    Task<bool> SubscribeUserAsync(string userId, PushSubscriptionDto subscription);
    Task<bool> UnsubscribeUserAsync(string userId, string endpoint);
    Task<bool> UnsubscribeAllUserAsync(string userId);
    Task<IEnumerable<PushNotificationSubscription>> GetUserSubscriptionsAsync(string userId);
    Task<bool> IsUserSubscribedAsync(string userId);
    
    // Notification Sending
    Task<bool> SendNotificationAsync(SendPushNotificationDto notification);
    Task<bool> SendNotificationToUserAsync(string userId, SendPushNotificationDto notification);
    Task<bool> SendNotificationToAllUsersAsync(SendPushNotificationDto notification);
    Task<bool> SendBulkNotificationsAsync(IEnumerable<SendPushNotificationDto> notifications);
    
    // Notification Management
    Task<PushNotificationDto?> GetNotificationAsync(int notificationId);
    Task<IEnumerable<PushNotificationDto>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20);
    Task<bool> MarkNotificationAsClickedAsync(int notificationId);
    Task<bool> MarkNotificationAsDeliveredAsync(int notificationId);
    
    // Preferences
    Task<PushNotificationPreferencesDto> GetUserPreferencesAsync(string userId);
    Task<bool> UpdateUserPreferencesAsync(string userId, PushNotificationPreferencesDto preferences);
    
    // Statistics
    Task<PushNotificationStatisticsDto> GetStatisticsAsync();
    Task<PushNotificationStatisticsDto> GetUserStatisticsAsync(string userId);
    
    // Utility Methods
    Task<bool> IsQuietHoursAsync(string userId);
    Task<bool> ShouldSendNotificationAsync(string userId, PushNotificationType type);
    Task CleanupExpiredSubscriptionsAsync();
    Task RetryFailedNotificationsAsync();
}

/// <summary>
/// Web Push service interface for handling web push protocol
/// </summary>
public interface IWebPushService
{
    Task<bool> SendPushAsync(string endpoint, string p256dh, string auth, WebPushPayloadDto payload);
    Task<bool> ValidateSubscriptionAsync(string endpoint, string p256dh, string auth);
    Task<string> GenerateVapidKeysAsync();
    Task<bool> IsEndpointValidAsync(string endpoint);
}

/// <summary>
/// Push notification service implementation
/// </summary>
public class PushNotificationService : IPushNotificationService
{
    private readonly ILogger<PushNotificationService> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IWebPushService _webPushService;
    private readonly ICacheService _cacheService;
    private readonly INotificationService _signalRService;

    public PushNotificationService(
        ILogger<PushNotificationService> logger,
        ApplicationDbContext context,
        IWebPushService webPushService,
        ICacheService cacheService,
        INotificationService signalRService)
    {
        _logger = logger;
        _context = context;
        _webPushService = webPushService;
        _cacheService = cacheService;
        _signalRService = signalRService;
    }

    public async Task<bool> SubscribeUserAsync(string userId, PushSubscriptionDto subscription)
    {
        try
        {
            _logger.LogInformation("Subscribing user {UserId} to push notifications", userId);

            // Check if subscription already exists
            var existingSubscription = await _context.PushNotificationSubscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Endpoint == subscription.Endpoint);

            if (existingSubscription != null)
            {
                // Update existing subscription
                existingSubscription.P256dh = subscription.P256dh;
                existingSubscription.Auth = subscription.Auth;
                existingSubscription.UserAgent = subscription.UserAgent;
                existingSubscription.DeviceInfo = subscription.DeviceInfo;
                existingSubscription.IsActive = true;
                existingSubscription.LastUsedAt = DateTime.UtcNow;
                existingSubscription.ExpiresAt = DateTime.UtcNow.AddDays(30); // Default 30 days
            }
            else
            {
                // Create new subscription
                var newSubscription = new PushNotificationSubscription
                {
                    UserId = userId,
                    Endpoint = subscription.Endpoint,
                    P256dh = subscription.P256dh,
                    Auth = subscription.Auth,
                    UserAgent = subscription.UserAgent,
                    DeviceInfo = subscription.DeviceInfo,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    LastUsedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(30)
                };

                _context.PushNotificationSubscriptions.Add(newSubscription);
            }

            await _context.SaveChangesAsync();

            // Clear user preferences cache
            await _cacheService.RemoveAsync($"user_preferences_{userId}");

            _logger.LogInformation("Successfully subscribed user {UserId} to push notifications", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing user {UserId} to push notifications", userId);
            return false;
        }
    }

    public async Task<bool> UnsubscribeUserAsync(string userId, string endpoint)
    {
        try
        {
            _logger.LogInformation("Unsubscribing user {UserId} from push notifications", userId);

            var subscription = await _context.PushNotificationSubscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Endpoint == endpoint);

            if (subscription != null)
            {
                subscription.IsActive = false;
                await _context.SaveChangesAsync();

                // Clear user preferences cache
                await _cacheService.RemoveAsync($"user_preferences_{userId}");

                _logger.LogInformation("Successfully unsubscribed user {UserId} from push notifications", userId);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing user {UserId} from push notifications", userId);
            return false;
        }
    }

    public async Task<bool> UnsubscribeAllUserAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Unsubscribing user {UserId} from all push notifications", userId);

            var subscriptions = await _context.PushNotificationSubscriptions
                .Where(s => s.UserId == userId)
                .ToListAsync();

            foreach (var subscription in subscriptions)
            {
                subscription.IsActive = false;
            }

            await _context.SaveChangesAsync();

            // Clear user preferences cache
            await _cacheService.RemoveAsync($"user_preferences_{userId}");

            _logger.LogInformation("Successfully unsubscribed user {UserId} from all push notifications", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing user {UserId} from all push notifications", userId);
            return false;
        }
    }

    public async Task<IEnumerable<PushNotificationSubscription>> GetUserSubscriptionsAsync(string userId)
    {
        return await _context.PushNotificationSubscriptions
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync();
    }

    public async Task<bool> IsUserSubscribedAsync(string userId)
    {
        return await _context.PushNotificationSubscriptions
            .AnyAsync(s => s.UserId == userId && s.IsActive);
    }

    public async Task<bool> SendNotificationAsync(SendPushNotificationDto notification)
    {
        try
        {
            _logger.LogInformation("Sending push notification to user {UserId}", notification.UserId);

            // Check if user should receive this notification
            if (!await ShouldSendNotificationAsync(notification.UserId, notification.Type))
            {
                _logger.LogInformation("User {UserId} should not receive notification of type {Type}", 
                    notification.UserId, notification.Type);
                return false;
            }

            // Get user subscriptions
            var subscriptions = await GetUserSubscriptionsAsync(notification.UserId);
            if (!subscriptions.Any())
            {
                _logger.LogWarning("No active subscriptions found for user {UserId}", notification.UserId);
                return false;
            }

            // Create notification record
            var pushNotification = new PushNotification
            {
                UserId = notification.UserId,
                Title = notification.Title,
                Body = notification.Body,
                Icon = notification.Icon,
                Badge = notification.Badge,
                Image = notification.Image,
                Tag = notification.Tag,
                Data = notification.Data,
                Url = notification.Url,
                Type = notification.Type,
                Status = PushNotificationStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.PushNotifications.Add(pushNotification);
            await _context.SaveChangesAsync();

            // Send to all active subscriptions
            bool anySuccess = false;
            foreach (var subscription in subscriptions)
            {
                try
                {
                    var payload = new WebPushPayloadDto
                    {
                        Title = notification.Title,
                        Body = notification.Body,
                        Icon = notification.Icon ?? "/icons/icon-192x192.png",
                        Badge = notification.Badge ?? "/icons/badge-72x72.png",
                        Image = notification.Image,
                        Tag = notification.Tag,
                        Data = notification.Data,
                        Url = notification.Url,
                        Ttl = notification.Ttl,
                        RequireInteraction = notification.RequireInteraction,
                        Silent = notification.Silent
                    };

                    var success = await _webPushService.SendPushAsync(
                        subscription.Endpoint,
                        subscription.P256dh,
                        subscription.Auth,
                        payload);

                    if (success)
                    {
                        subscription.LastUsedAt = DateTime.UtcNow;
                        anySuccess = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending push notification to subscription {SubscriptionId}", 
                        subscription.Id);
                }
            }

            // Update notification status
            pushNotification.Status = anySuccess ? PushNotificationStatus.Sent : PushNotificationStatus.Failed;
            pushNotification.SentAt = anySuccess ? DateTime.UtcNow : null;
            pushNotification.ErrorMessage = anySuccess ? null : "Failed to send to all subscriptions";
            await _context.SaveChangesAsync();

            _logger.LogInformation("Push notification sent to user {UserId}, success: {Success}", 
                notification.UserId, anySuccess);

            return anySuccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to user {UserId}", notification.UserId);
            return false;
        }
    }

    public async Task<bool> SendNotificationToUserAsync(string userId, SendPushNotificationDto notification)
    {
        notification.UserId = userId;
        return await SendNotificationAsync(notification);
    }

    public async Task<bool> SendNotificationToAllUsersAsync(SendPushNotificationDto notification)
    {
        try
        {
            _logger.LogInformation("Sending push notification to all users");

            var activeSubscriptions = await _context.PushNotificationSubscriptions
                .Where(s => s.IsActive)
                .ToListAsync();

            var userIds = activeSubscriptions.Select(s => s.UserId).Distinct().ToList();
            var successCount = 0;

            foreach (var userId in userIds)
            {
                var userNotification = new SendPushNotificationDto
                {
                    UserId = userId,
                    Title = notification.Title,
                    Body = notification.Body,
                    Icon = notification.Icon,
                    Badge = notification.Badge,
                    Image = notification.Image,
                    Tag = notification.Tag,
                    Data = notification.Data,
                    Url = notification.Url,
                    Type = notification.Type,
                    Ttl = notification.Ttl,
                    RequireInteraction = notification.RequireInteraction,
                    Silent = notification.Silent
                };

                if (await SendNotificationAsync(userNotification))
                {
                    successCount++;
                }
            }

            _logger.LogInformation("Push notification sent to {SuccessCount}/{TotalUsers} users", 
                successCount, userIds.Count);

            return successCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification to all users");
            return false;
        }
    }

    public async Task<bool> SendBulkNotificationsAsync(IEnumerable<SendPushNotificationDto> notifications)
    {
        try
        {
            _logger.LogInformation("Sending {Count} bulk push notifications", notifications.Count());

            var successCount = 0;
            foreach (var notification in notifications)
            {
                if (await SendNotificationAsync(notification))
                {
                    successCount++;
                }
            }

            _logger.LogInformation("Bulk push notifications sent: {SuccessCount}/{TotalCount}", 
                successCount, notifications.Count());

            return successCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk push notifications");
            return false;
        }
    }

    public async Task<PushNotificationDto?> GetNotificationAsync(int notificationId)
    {
        var notification = await _context.PushNotifications
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == notificationId);

        return notification != null ? MapToDto(notification) : null;
    }

    public async Task<IEnumerable<PushNotificationDto>> GetUserNotificationsAsync(string userId, int page = 1, int pageSize = 20)
    {
        var notifications = await _context.PushNotifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return notifications.Select(MapToDto);
    }

    public async Task<bool> MarkNotificationAsClickedAsync(int notificationId)
    {
        try
        {
            var notification = await _context.PushNotifications
                .FirstOrDefaultAsync(n => n.Id == notificationId);

            if (notification != null)
            {
                notification.ClickedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as clicked", notificationId);
            return false;
        }
    }

    public async Task<bool> MarkNotificationAsDeliveredAsync(int notificationId)
    {
        try
        {
            var notification = await _context.PushNotifications
                .FirstOrDefaultAsync(n => n.Id == notificationId);

            if (notification != null)
            {
                notification.DeliveredAt = DateTime.UtcNow;
                notification.Status = PushNotificationStatus.Delivered;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as delivered", notificationId);
            return false;
        }
    }

    public async Task<PushNotificationPreferencesDto> GetUserPreferencesAsync(string userId)
    {
        var cacheKey = $"user_preferences_{userId}";
        var cachedPreferences = await _cacheService.GetAsync<PushNotificationPreferencesDto>(cacheKey);
        
        if (cachedPreferences != null)
        {
            return cachedPreferences;
        }

        // Default preferences (in a real app, these would be stored in the database)
        var preferences = new PushNotificationPreferencesDto();
        
        await _cacheService.SetAsync(cacheKey, preferences, TimeSpan.FromHours(1));
        return preferences;
    }

    public async Task<bool> UpdateUserPreferencesAsync(string userId, PushNotificationPreferencesDto preferences)
    {
        try
        {
            var cacheKey = $"user_preferences_{userId}";
            await _cacheService.SetAsync(cacheKey, preferences, TimeSpan.FromHours(1));
            
            _logger.LogInformation("Updated push notification preferences for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating push notification preferences for user {UserId}", userId);
            return false;
        }
    }

    public async Task<PushNotificationStatisticsDto> GetStatisticsAsync()
    {
        var totalSubscriptions = await _context.PushNotificationSubscriptions.CountAsync();
        var activeSubscriptions = await _context.PushNotificationSubscriptions.CountAsync(s => s.IsActive);
        var totalNotifications = await _context.PushNotifications.CountAsync();
        var sentNotifications = await _context.PushNotifications.CountAsync(n => n.Status == PushNotificationStatus.Sent);
        var deliveredNotifications = await _context.PushNotifications.CountAsync(n => n.Status == PushNotificationStatus.Delivered);
        var clickedNotifications = await _context.PushNotifications.CountAsync(n => n.ClickedAt != null);
        var failedNotifications = await _context.PushNotifications.CountAsync(n => n.Status == PushNotificationStatus.Failed);

        var deliveryRate = totalNotifications > 0 ? (decimal)deliveredNotifications / totalNotifications : 0;
        var clickRate = deliveredNotifications > 0 ? (decimal)clickedNotifications / deliveredNotifications : 0;

        var notificationsByType = await _context.PushNotifications
            .GroupBy(n => n.Type.ToString())
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count);

        var notificationsByStatus = await _context.PushNotifications
            .GroupBy(n => n.Status.ToString())
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        var notificationsByDay = await _context.PushNotifications
            .GroupBy(n => n.CreatedAt.Date)
            .Select(g => new { Date = g.Key.ToString("yyyy-MM-dd"), Count = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Count);

        return new PushNotificationStatisticsDto
        {
            TotalSubscriptions = totalSubscriptions,
            ActiveSubscriptions = activeSubscriptions,
            TotalNotificationsSent = sentNotifications,
            TotalNotificationsDelivered = deliveredNotifications,
            TotalNotificationsClicked = clickedNotifications,
            TotalNotificationsFailed = failedNotifications,
            DeliveryRate = deliveryRate,
            ClickRate = clickRate,
            NotificationsByType = notificationsByType,
            NotificationsByStatus = notificationsByStatus,
            NotificationsByDay = notificationsByDay
        };
    }

    public async Task<PushNotificationStatisticsDto> GetUserStatisticsAsync(string userId)
    {
        var userNotifications = _context.PushNotifications.Where(n => n.UserId == userId);
        
        var totalNotifications = await userNotifications.CountAsync();
        var sentNotifications = await userNotifications.CountAsync(n => n.Status == PushNotificationStatus.Sent);
        var deliveredNotifications = await userNotifications.CountAsync(n => n.Status == PushNotificationStatus.Delivered);
        var clickedNotifications = await userNotifications.CountAsync(n => n.ClickedAt != null);
        var failedNotifications = await userNotifications.CountAsync(n => n.Status == PushNotificationStatus.Failed);

        var deliveryRate = totalNotifications > 0 ? (decimal)deliveredNotifications / totalNotifications : 0;
        var clickRate = deliveredNotifications > 0 ? (decimal)clickedNotifications / deliveredNotifications : 0;

        var notificationsByType = await userNotifications
            .GroupBy(n => n.Type.ToString())
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count);

        var notificationsByStatus = await userNotifications
            .GroupBy(n => n.Status.ToString())
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        var notificationsByDay = await userNotifications
            .GroupBy(n => n.CreatedAt.Date)
            .Select(g => new { Date = g.Key.ToString("yyyy-MM-dd"), Count = g.Count() })
            .ToDictionaryAsync(x => x.Date, x => x.Count);

        return new PushNotificationStatisticsDto
        {
            TotalSubscriptions = await _context.PushNotificationSubscriptions.CountAsync(s => s.UserId == userId),
            ActiveSubscriptions = await _context.PushNotificationSubscriptions.CountAsync(s => s.UserId == userId && s.IsActive),
            TotalNotificationsSent = sentNotifications,
            TotalNotificationsDelivered = deliveredNotifications,
            TotalNotificationsClicked = clickedNotifications,
            TotalNotificationsFailed = failedNotifications,
            DeliveryRate = deliveryRate,
            ClickRate = clickRate,
            NotificationsByType = notificationsByType,
            NotificationsByStatus = notificationsByStatus,
            NotificationsByDay = notificationsByDay
        };
    }

    public async Task<bool> IsQuietHoursAsync(string userId)
    {
        var preferences = await GetUserPreferencesAsync(userId);
        
        if (!preferences.EnableQuietHours)
        {
            return false;
        }

        var currentHour = DateTime.UtcNow.Hour;
        return currentHour >= preferences.QuietHoursStart || currentHour < preferences.QuietHoursEnd;
    }

    public async Task<bool> ShouldSendNotificationAsync(string userId, PushNotificationType type)
    {
        var preferences = await GetUserPreferencesAsync(userId);
        
        if (!preferences.EnablePushNotifications)
        {
            return false;
        }

        // Check quiet hours
        if (await IsQuietHoursAsync(userId))
        {
            return false;
        }

        // Check type-specific preferences
        return type switch
        {
            PushNotificationType.AuctionStarting or PushNotificationType.AuctionEndingSoon or PushNotificationType.AuctionEnded => preferences.EnableAuctionNotifications,
            PushNotificationType.BidPlaced or PushNotificationType.AuctionWon or PushNotificationType.AuctionLost => preferences.EnableBidNotifications,
            PushNotificationType.PaymentReceived or PushNotificationType.PaymentFailed => preferences.EnablePaymentNotifications,
            PushNotificationType.SystemAnnouncement => preferences.EnableSystemNotifications,
            PushNotificationType.AdminAlert => preferences.EnableAdminAlerts,
            _ => true
        };
    }

    public async Task CleanupExpiredSubscriptionsAsync()
    {
        try
        {
            var expiredSubscriptions = await _context.PushNotificationSubscriptions
                .Where(s => s.ExpiresAt != null && s.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();

            foreach (var subscription in expiredSubscriptions)
            {
                subscription.IsActive = false;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Cleaned up {Count} expired push notification subscriptions", 
                expiredSubscriptions.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired push notification subscriptions");
        }
    }

    public async Task RetryFailedNotificationsAsync()
    {
        try
        {
            var failedNotifications = await _context.PushNotifications
                .Where(n => n.Status == PushNotificationStatus.Failed && n.RetryCount < 3)
                .ToListAsync();

            foreach (var notification in failedNotifications)
            {
                var sendDto = new SendPushNotificationDto
                {
                    UserId = notification.UserId,
                    Title = notification.Title,
                    Body = notification.Body,
                    Icon = notification.Icon,
                    Badge = notification.Badge,
                    Image = notification.Image,
                    Tag = notification.Tag,
                    Data = notification.Data,
                    Url = notification.Url,
                    Type = notification.Type
                };

                if (await SendNotificationAsync(sendDto))
                {
                    notification.RetryCount++;
                    notification.Status = PushNotificationStatus.Sent;
                    notification.SentAt = DateTime.UtcNow;
                    notification.ErrorMessage = null;
                }
                else
                {
                    notification.RetryCount++;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Retried {Count} failed push notifications", failedNotifications.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying failed push notifications");
        }
    }

    private PushNotificationDto MapToDto(PushNotification notification)
    {
        return new PushNotificationDto
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Title = notification.Title,
            Body = notification.Body,
            Icon = notification.Icon,
            Badge = notification.Badge,
            Image = notification.Image,
            Tag = notification.Tag,
            Data = notification.Data,
            Url = notification.Url,
            Type = notification.Type,
            Status = notification.Status,
            CreatedAt = notification.CreatedAt,
            SentAt = notification.SentAt,
            DeliveredAt = notification.DeliveredAt,
            ClickedAt = notification.ClickedAt,
            ErrorMessage = notification.ErrorMessage,
            RetryCount = notification.RetryCount
        };
    }
}

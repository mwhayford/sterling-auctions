using SterlingAuctions.Core.Entities;

namespace SterlingAuctions.Core.Interfaces;

/// <summary>
/// Service interface for notification operations
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Send notification to user
    /// </summary>
    Task<Notification> SendNotificationAsync(string userId, string title, string message, 
        NotificationType type, NotificationPriority priority = NotificationPriority.Normal,
        string? actionUrl = null, object? data = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send outbid notification
    /// </summary>
    Task SendOutbidNotificationAsync(Guid bidId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send auction ending soon notification
    /// </summary>
    Task SendAuctionEndingSoonNotificationAsync(Guid auctionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send auction ended notification
    /// </summary>
    Task SendAuctionEndedNotificationAsync(Guid auctionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send auction won notification
    /// </summary>
    Task SendAuctionWonNotificationAsync(Guid bidId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send payment required notification
    /// </summary>
    Task SendPaymentRequiredNotificationAsync(Guid bidId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send payment received notification
    /// </summary>
    Task SendPaymentReceivedNotificationAsync(Guid paymentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send new auction notification to interested users
    /// </summary>
    Task SendNewAuctionNotificationAsync(Guid auctionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user notifications
    /// </summary>
    Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, bool? isRead = null, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark notification as read
    /// </summary>
    Task<bool> MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark all notifications as read for user
    /// </summary>
    Task<bool> MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get unread notification count
    /// </summary>
    Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete notification
    /// </summary>
    Task<bool> DeleteNotificationAsync(Guid notificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send bulk notifications
    /// </summary>
    Task SendBulkNotificationAsync(IEnumerable<string> userIds, string title, string message,
        NotificationType type, NotificationPriority priority = NotificationPriority.Normal,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Process notification queue
    /// </summary>
    Task ProcessNotificationQueueAsync(CancellationToken cancellationToken = default);
}

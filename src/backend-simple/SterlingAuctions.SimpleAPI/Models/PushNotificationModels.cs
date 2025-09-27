using System.ComponentModel.DataAnnotations;

namespace SterlingAuctions.SimpleAPI.Models;

/// <summary>
/// Push notification subscription model
/// </summary>
public class PushNotificationSubscription
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string Endpoint { get; set; } = string.Empty;
    
    [Required]
    public string P256dh { get; set; } = string.Empty;
    
    [Required]
    public string Auth { get; set; } = string.Empty;
    
    public string? UserAgent { get; set; }
    public string? DeviceInfo { get; set; }
    
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    
    // Navigation properties
    public ApplicationUser? User { get; set; }
}

/// <summary>
/// Push notification model
/// </summary>
public class PushNotification
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Body { get; set; } = string.Empty;
    
    public string? Icon { get; set; }
    public string? Badge { get; set; }
    public string? Image { get; set; }
    public string? Tag { get; set; }
    public string? Data { get; set; }
    public string? Url { get; set; }
    
    public PushNotificationType Type { get; set; } = PushNotificationType.General;
    public PushNotificationStatus Status { get; set; } = PushNotificationStatus.Pending;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ClickedAt { get; set; }
    
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; } = 0;
    
    // Navigation properties
    public ApplicationUser? User { get; set; }
}

/// <summary>
/// Push notification type enumeration
/// </summary>
public enum PushNotificationType
{
    General = 0,
    AuctionStarting = 1,
    AuctionEndingSoon = 2,
    AuctionEnded = 3,
    BidPlaced = 4,
    AuctionWon = 5,
    AuctionLost = 6,
    PaymentReceived = 7,
    PaymentFailed = 8,
    SystemAnnouncement = 9,
    AdminAlert = 10
}

/// <summary>
/// Push notification status enumeration
/// </summary>
public enum PushNotificationStatus
{
    Pending = 0,
    Sent = 1,
    Delivered = 2,
    Failed = 3,
    Expired = 4
}

/// <summary>
/// Push notification subscription DTO
/// </summary>
public class PushSubscriptionDto
{
    [Required]
    public string Endpoint { get; set; } = string.Empty;
    
    [Required]
    public string P256dh { get; set; } = string.Empty;
    
    [Required]
    public string Auth { get; set; } = string.Empty;
    
    public string? UserAgent { get; set; }
    public string? DeviceInfo { get; set; }
}

/// <summary>
/// Push notification request DTO
/// </summary>
public class SendPushNotificationDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string Body { get; set; } = string.Empty;
    
    public string? Icon { get; set; }
    public string? Badge { get; set; }
    public string? Image { get; set; }
    public string? Tag { get; set; }
    public string? Data { get; set; }
    public string? Url { get; set; }
    
    public PushNotificationType Type { get; set; } = PushNotificationType.General;
    public int? Ttl { get; set; } = 86400; // 24 hours default
    public bool RequireInteraction { get; set; } = false;
    public bool Silent { get; set; } = false;
}

/// <summary>
/// Push notification response DTO
/// </summary>
public class PushNotificationDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Badge { get; set; }
    public string? Image { get; set; }
    public string? Tag { get; set; }
    public string? Data { get; set; }
    public string? Url { get; set; }
    public PushNotificationType Type { get; set; }
    public PushNotificationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ClickedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
}

/// <summary>
/// Push notification preferences DTO
/// </summary>
public class PushNotificationPreferencesDto
{
    public bool EnablePushNotifications { get; set; } = true;
    public bool EnableAuctionNotifications { get; set; } = true;
    public bool EnableBidNotifications { get; set; } = true;
    public bool EnablePaymentNotifications { get; set; } = true;
    public bool EnableSystemNotifications { get; set; } = true;
    public bool EnableAdminAlerts { get; set; } = true;
    public bool EnableSound { get; set; } = true;
    public bool EnableVibration { get; set; } = true;
    public int QuietHoursStart { get; set; } = 22; // 10 PM
    public int QuietHoursEnd { get; set; } = 8; // 8 AM
    public bool EnableQuietHours { get; set; } = true;
}

/// <summary>
/// Push notification statistics DTO
/// </summary>
public class PushNotificationStatisticsDto
{
    public int TotalSubscriptions { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int TotalNotificationsSent { get; set; }
    public int TotalNotificationsDelivered { get; set; }
    public int TotalNotificationsClicked { get; set; }
    public int TotalNotificationsFailed { get; set; }
    public decimal DeliveryRate { get; set; }
    public decimal ClickRate { get; set; }
    public Dictionary<string, int> NotificationsByType { get; set; } = new();
    public Dictionary<string, int> NotificationsByStatus { get; set; } = new();
    public Dictionary<string, int> NotificationsByDay { get; set; } = new();
}

/// <summary>
/// Web Push payload DTO
/// </summary>
public class WebPushPayloadDto
{
    [Required]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public string Body { get; set; } = string.Empty;
    
    public string? Icon { get; set; }
    public string? Badge { get; set; }
    public string? Image { get; set; }
    public string? Tag { get; set; }
    public string? Data { get; set; }
    public string? Url { get; set; }
    public int? Ttl { get; set; }
    public bool RequireInteraction { get; set; } = false;
    public bool Silent { get; set; } = false;
    public string[]? Actions { get; set; }
}

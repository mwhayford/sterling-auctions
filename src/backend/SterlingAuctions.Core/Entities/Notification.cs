using System.ComponentModel.DataAnnotations;

namespace SterlingAuctions.Core.Entities;

/// <summary>
/// Notification entity for user notifications
/// </summary>
public class Notification : BaseEntity
{
    /// <summary>
    /// Notification title
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Notification message/content
    /// </summary>
    [Required]
    [MaxLength(1000)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Type of notification
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// Priority level of the notification
    /// </summary>
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    /// <summary>
    /// Whether the notification has been read
    /// </summary>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// Date when the notification was read
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Whether the notification has been sent via email
    /// </summary>
    public bool EmailSent { get; set; } = false;

    /// <summary>
    /// Whether the notification has been sent via SMS
    /// </summary>
    public bool SmsSent { get; set; } = false;

    /// <summary>
    /// Whether the notification has been sent as push notification
    /// </summary>
    public bool PushSent { get; set; } = false;

    /// <summary>
    /// URL to navigate to when notification is clicked
    /// </summary>
    [MaxLength(500)]
    public string? ActionUrl { get; set; }

    /// <summary>
    /// Additional data in JSON format
    /// </summary>
    public string? Data { get; set; }

    /// <summary>
    /// ID of the user receiving the notification
    /// </summary>
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property for the user
    /// </summary>
    public virtual ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// ID of the related auction (if applicable)
    /// </summary>
    public Guid? AuctionId { get; set; }

    /// <summary>
    /// Navigation property for the auction
    /// </summary>
    public virtual Auction? Auction { get; set; }

    /// <summary>
    /// ID of the related auction item (if applicable)
    /// </summary>
    public Guid? AuctionItemId { get; set; }

    /// <summary>
    /// Navigation property for the auction item
    /// </summary>
    public virtual AuctionItem? AuctionItem { get; set; }

    /// <summary>
    /// ID of the related bid (if applicable)
    /// </summary>
    public Guid? BidId { get; set; }

    /// <summary>
    /// Navigation property for the bid
    /// </summary>
    public virtual Bid? Bid { get; set; }
}

/// <summary>
/// Notification type enumeration
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// General information notification
    /// </summary>
    Info = 0,

    /// <summary>
    /// User has been outbid
    /// </summary>
    Outbid = 1,

    /// <summary>
    /// Auction is ending soon
    /// </summary>
    AuctionEndingSoon = 2,

    /// <summary>
    /// Auction has ended
    /// </summary>
    AuctionEnded = 3,

    /// <summary>
    /// User won an auction
    /// </summary>
    AuctionWon = 4,

    /// <summary>
    /// Payment required
    /// </summary>
    PaymentRequired = 5,

    /// <summary>
    /// Payment received
    /// </summary>
    PaymentReceived = 6,

    /// <summary>
    /// New auction in followed category
    /// </summary>
    NewAuction = 7,

    /// <summary>
    /// Auction cancelled
    /// </summary>
    AuctionCancelled = 8,

    /// <summary>
    /// Account verification required
    /// </summary>
    VerificationRequired = 9,

    /// <summary>
    /// Account verified
    /// </summary>
    AccountVerified = 10,

    /// <summary>
    /// Security alert
    /// </summary>
    SecurityAlert = 11,

    /// <summary>
    /// System maintenance notification
    /// </summary>
    SystemMaintenance = 12
}

/// <summary>
/// Notification priority enumeration
/// </summary>
public enum NotificationPriority
{
    /// <summary>
    /// Low priority notification
    /// </summary>
    Low = 0,

    /// <summary>
    /// Normal priority notification
    /// </summary>
    Normal = 1,

    /// <summary>
    /// High priority notification
    /// </summary>
    High = 2,

    /// <summary>
    /// Urgent notification
    /// </summary>
    Urgent = 3
}

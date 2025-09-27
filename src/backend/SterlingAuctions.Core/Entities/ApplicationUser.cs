using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SterlingAuctions.Core.Entities;

/// <summary>
/// Application user entity extending IdentityUser
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// User's first name
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// User's full name (computed property)
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// User's profile image URL
    /// </summary>
    [MaxLength(500)]
    public string? ProfileImageUrl { get; set; }

    /// <summary>
    /// Indicates if the user account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date and time when the user was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the user was last modified
    /// </summary>
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date of birth
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// User's address
    /// </summary>
    [MaxLength(200)]
    public string? Address { get; set; }

    /// <summary>
    /// City
    /// </summary>
    [MaxLength(100)]
    public string? City { get; set; }

    /// <summary>
    /// State or province
    /// </summary>
    [MaxLength(100)]
    public string? State { get; set; }

    /// <summary>
    /// Postal/ZIP code
    /// </summary>
    [MaxLength(20)]
    public string? PostalCode { get; set; }

    /// <summary>
    /// Country
    /// </summary>
    [MaxLength(100)]
    public string? Country { get; set; }

    /// <summary>
    /// Whether the user's identity has been verified
    /// </summary>
    public bool IsVerified { get; set; } = false;

    /// <summary>
    /// Date when the user was verified
    /// </summary>
    public DateTime? VerifiedAt { get; set; }

    /// <summary>
    /// User's preferred notification settings
    /// </summary>
    public NotificationPreferences NotificationPreferences { get; set; } = new();

    /// <summary>
    /// Date and time when the user was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the user was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property for user's auctions
    /// </summary>
    public virtual ICollection<Auction> Auctions { get; set; } = new List<Auction>();

    /// <summary>
    /// Navigation property for user's bids
    /// </summary>
    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();

    /// <summary>
    /// Navigation property for user's payments
    /// </summary>
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    /// <summary>
    /// Navigation property for user's notifications
    /// </summary>
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    /// <summary>
    /// Navigation property for user's watchlisted auctions
    /// </summary>
    public virtual ICollection<Watchlist> WatchlistedAuctions { get; set; } = new List<Watchlist>();
}

/// <summary>
/// User's notification preferences
/// </summary>
public class NotificationPreferences
{
    /// <summary>
    /// Receive email notifications for outbid alerts
    /// </summary>
    public bool EmailOutbidAlerts { get; set; } = true;

    /// <summary>
    /// Receive email notifications for auction ending soon
    /// </summary>
    public bool EmailAuctionEndingSoon { get; set; } = true;

    /// <summary>
    /// Receive email notifications for new auctions in followed categories
    /// </summary>
    public bool EmailNewAuctions { get; set; } = false;

    /// <summary>
    /// Receive SMS notifications for outbid alerts
    /// </summary>
    public bool SmsOutbidAlerts { get; set; } = false;

    /// <summary>
    /// Receive SMS notifications for auction ending soon
    /// </summary>
    public bool SmsAuctionEndingSoon { get; set; } = false;

    /// <summary>
    /// Receive push notifications
    /// </summary>
    public bool PushNotifications { get; set; } = true;

    /// <summary>
    /// Marketing email subscription
    /// </summary>
    public bool MarketingEmails { get; set; } = false;
}

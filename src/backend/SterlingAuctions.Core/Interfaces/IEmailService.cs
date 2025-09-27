namespace SterlingAuctions.Core.Interfaces;

/// <summary>
/// Service interface for email operations
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Send email
    /// </summary>
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send email to multiple recipients
    /// </summary>
    Task<bool> SendEmailAsync(IEnumerable<string> to, string subject, string body, bool isHtml = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send welcome email
    /// </summary>
    Task<bool> SendWelcomeEmailAsync(string to, string firstName, string confirmationUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send email confirmation
    /// </summary>
    Task<bool> SendEmailConfirmationAsync(string to, string firstName, string confirmationUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send password reset email
    /// </summary>
    Task<bool> SendPasswordResetEmailAsync(string to, string firstName, string resetUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send outbid notification email
    /// </summary>
    Task<bool> SendOutbidNotificationEmailAsync(string to, string firstName, string auctionTitle,
        string itemTitle, decimal newBidAmount, string auctionUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send auction ending soon email
    /// </summary>
    Task<bool> SendAuctionEndingSoonEmailAsync(string to, string firstName, string auctionTitle,
        DateTime endTime, string auctionUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send auction won email
    /// </summary>
    Task<bool> SendAuctionWonEmailAsync(string to, string firstName, string auctionTitle,
        string itemTitle, decimal winningAmount, string paymentUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send payment confirmation email
    /// </summary>
    Task<bool> SendPaymentConfirmationEmailAsync(string to, string firstName, decimal amount,
        string transactionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send new auction notification email
    /// </summary>
    Task<bool> SendNewAuctionNotificationEmailAsync(string to, string firstName, string auctionTitle,
        DateTime startTime, string auctionUrl, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send verification required email
    /// </summary>
    Task<bool> SendVerificationRequiredEmailAsync(string to, string firstName, string verificationUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send account verified email
    /// </summary>
    Task<bool> SendAccountVerifiedEmailAsync(string to, string firstName,
        CancellationToken cancellationToken = default);
}

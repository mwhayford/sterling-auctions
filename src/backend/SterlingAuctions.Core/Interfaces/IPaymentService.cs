using SterlingAuctions.Core.Entities;

namespace SterlingAuctions.Core.Interfaces;

/// <summary>
/// Service interface for payment operations
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Create payment intent for auction registration fee
    /// </summary>
    Task<(bool Success, string? PaymentIntentId, string? ClientSecret, IEnumerable<string> Errors)> 
        CreateRegistrationFeePaymentAsync(Guid auctionId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create payment intent for winning bid
    /// </summary>
    Task<(bool Success, string? PaymentIntentId, string? ClientSecret, IEnumerable<string> Errors)> 
        CreateAuctionPaymentAsync(Guid bidId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Process payment completion
    /// </summary>
    Task<(bool Success, Payment? Payment, IEnumerable<string> Errors)> ProcessPaymentAsync(
        string paymentIntentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refund payment
    /// </summary>
    Task<(bool Success, Payment? Refund, IEnumerable<string> Errors)> RefundPaymentAsync(
        Guid paymentId, decimal? amount = null, string? reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get payment by ID
    /// </summary>
    Task<Payment?> GetPaymentByIdAsync(Guid paymentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get payments by user
    /// </summary>
    Task<IEnumerable<Payment>> GetUserPaymentsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get payments by auction
    /// </summary>
    Task<IEnumerable<Payment>> GetAuctionPaymentsAsync(Guid auctionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate total amount due for winning bid
    /// </summary>
    Task<decimal> CalculateTotalAmountDueAsync(Guid bidId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if payment is required for user in auction
    /// </summary>
    Task<bool> IsPaymentRequiredAsync(Guid auctionId, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark payment as failed
    /// </summary>
    Task<bool> MarkPaymentAsFailedAsync(Guid paymentId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending payments for user
    /// </summary>
    Task<IEnumerable<Payment>> GetPendingPaymentsAsync(string userId, CancellationToken cancellationToken = default);
}

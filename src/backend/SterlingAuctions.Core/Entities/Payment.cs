using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SterlingAuctions.Core.Entities;

/// <summary>
/// Payment entity for handling payments and transactions
/// </summary>
public class Payment : BaseEntity
{
    /// <summary>
    /// Payment amount
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Payment type
    /// </summary>
    public PaymentType Type { get; set; }

    /// <summary>
    /// Current payment status
    /// </summary>
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>
    /// Payment method used
    /// </summary>
    public PaymentMethod Method { get; set; }

    /// <summary>
    /// External payment provider transaction ID
    /// </summary>
    [MaxLength(255)]
    public string? TransactionId { get; set; }

    /// <summary>
    /// Stripe payment intent ID
    /// </summary>
    [MaxLength(255)]
    public string? StripePaymentIntentId { get; set; }

    /// <summary>
    /// Currency code (ISO 4217)
    /// </summary>
    [Required]
    [MaxLength(3)]
    public string Currency { get; set; } = "USD";

    /// <summary>
    /// Exchange rate used (if different from base currency)
    /// </summary>
    [Column(TypeName = "decimal(18,6)")]
    public decimal? ExchangeRate { get; set; }

    /// <summary>
    /// Payment description
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Date when payment was initiated
    /// </summary>
    public DateTime? PaymentDate { get; set; }

    /// <summary>
    /// Date when payment was completed
    /// </summary>
    public DateTime? CompletedDate { get; set; }

    /// <summary>
    /// Failure reason (if payment failed)
    /// </summary>
    [MaxLength(1000)]
    public string? FailureReason { get; set; }

    /// <summary>
    /// Notes about the payment
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>
    /// ID of the user making the payment
    /// </summary>
    [Required]
    public string PayerId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property for the payer
    /// </summary>
    public virtual ApplicationUser Payer { get; set; } = null!;

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
/// Payment type enumeration
/// </summary>
public enum PaymentType
{
    /// <summary>
    /// Registration fee for auction participation
    /// </summary>
    RegistrationFee = 0,

    /// <summary>
    /// Deposit for bidding eligibility
    /// </summary>
    Deposit = 1,

    /// <summary>
    /// Payment for won auction item
    /// </summary>
    AuctionPayment = 2,

    /// <summary>
    /// Buyer's premium payment
    /// </summary>
    BuyersPremium = 3,

    /// <summary>
    /// Shipping and handling fee
    /// </summary>
    ShippingFee = 4,

    /// <summary>
    /// Tax payment
    /// </summary>
    Tax = 5,

    /// <summary>
    /// Refund for various reasons
    /// </summary>
    Refund = 6
}

/// <summary>
/// Payment status enumeration
/// </summary>
public enum PaymentStatus
{
    /// <summary>
    /// Payment is pending/processing
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Payment completed successfully
    /// </summary>
    Completed = 1,

    /// <summary>
    /// Payment failed
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Payment was cancelled
    /// </summary>
    Cancelled = 3,

    /// <summary>
    /// Payment was refunded
    /// </summary>
    Refunded = 4,

    /// <summary>
    /// Payment is partially refunded
    /// </summary>
    PartiallyRefunded = 5
}

/// <summary>
/// Payment method enumeration
/// </summary>
public enum PaymentMethod
{
    /// <summary>
    /// Credit card payment
    /// </summary>
    CreditCard = 0,

    /// <summary>
    /// Debit card payment
    /// </summary>
    DebitCard = 1,

    /// <summary>
    /// Bank transfer
    /// </summary>
    BankTransfer = 2,

    /// <summary>
    /// PayPal payment
    /// </summary>
    PayPal = 3,

    /// <summary>
    /// Wire transfer
    /// </summary>
    WireTransfer = 4,

    /// <summary>
    /// Check payment
    /// </summary>
    Check = 5,

    /// <summary>
    /// Cash payment
    /// </summary>
    Cash = 6,

    /// <summary>
    /// Digital wallet (Apple Pay, Google Pay, etc.)
    /// </summary>
    DigitalWallet = 7
}

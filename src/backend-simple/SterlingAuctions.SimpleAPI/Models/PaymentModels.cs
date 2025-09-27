using System.ComponentModel.DataAnnotations;

namespace SterlingAuctions.SimpleAPI.Models;

/// <summary>
/// Payment status enumeration
/// </summary>
public enum PaymentStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4,
    Refunded = 5,
    PartiallyRefunded = 6
}

/// <summary>
/// Payment method enumeration
/// </summary>
public enum PaymentMethod
{
    CreditCard = 0,
    DebitCard = 1,
    BankTransfer = 2,
    PayPal = 3,
    ApplePay = 4,
    GooglePay = 5,
    Stripe = 6
}

/// <summary>
/// Payment model for auction transactions
/// </summary>
public class Payment
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public int AuctionId { get; set; }
    
    [Required]
    public decimal Amount { get; set; }
    
    [Required]
    public string Currency { get; set; } = "USD";
    
    [Required]
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    
    [Required]
    public PaymentMethod Method { get; set; } = PaymentMethod.Stripe;
    
    public string? StripePaymentIntentId { get; set; }
    public string? StripeChargeId { get; set; }
    public string? StripeCustomerId { get; set; }
    
    public string? TransactionId { get; set; }
    public string? Reference { get; set; }
    
    public string? FailureReason { get; set; }
    public string? RefundReason { get; set; }
    
    public decimal? RefundAmount { get; set; }
    public DateTime? RefundedAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    
    // Navigation properties
    public ApplicationUser? User { get; set; }
    public Auction? Auction { get; set; }
}

/// <summary>
/// Payment intent creation DTO
/// </summary>
public class CreatePaymentIntentDto
{
    [Required]
    public int AuctionId { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    [Required]
    [StringLength(3, MinimumLength = 3)]
    public string Currency { get; set; } = "USD";
    
    public string? Description { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Payment intent response DTO
/// </summary>
public class PaymentIntentDto
{
    public string Id { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}

/// <summary>
/// Payment confirmation DTO
/// </summary>
public class ConfirmPaymentDto
{
    [Required]
    public string PaymentIntentId { get; set; } = string.Empty;
    
    public string? PaymentMethodId { get; set; }
    public bool SavePaymentMethod { get; set; } = false;
}

/// <summary>
/// Payment response DTO
/// </summary>
public class PaymentDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int AuctionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public PaymentMethod Method { get; set; }
    public string? TransactionId { get; set; }
    public string? Reference { get; set; }
    public string? FailureReason { get; set; }
    public decimal? RefundAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    
    // Related data
    public string? AuctionTitle { get; set; }
    public string? UserName { get; set; }
}

/// <summary>
/// Refund request DTO
/// </summary>
public class RefundRequestDto
{
    [Required]
    public int PaymentId { get; set; }
    
    [Range(0.01, double.MaxValue, ErrorMessage = "Refund amount must be greater than 0")]
    public decimal? Amount { get; set; } // If null, refund full amount
    
    [Required]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;
    
    public bool RefundApplicationFee { get; set; } = false;
}

/// <summary>
/// Payment method DTO
/// </summary>
public class PaymentMethodDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Last4 { get; set; }
    public int? ExpMonth { get; set; }
    public int? ExpYear { get; set; }
    public bool IsDefault { get; set; } = false;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Payment statistics DTO
/// </summary>
public class PaymentStatisticsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalRefunds { get; set; }
    public decimal NetRevenue { get; set; }
    public int TotalPayments { get; set; }
    public int SuccessfulPayments { get; set; }
    public int FailedPayments { get; set; }
    public int RefundedPayments { get; set; }
    public decimal SuccessRate { get; set; }
    public Dictionary<string, decimal> RevenueByCurrency { get; set; } = new();
    public Dictionary<string, int> PaymentsByMethod { get; set; } = new();
    public Dictionary<string, int> PaymentsByStatus { get; set; } = new();
}

/// <summary>
/// Webhook event DTO
/// </summary>
public class WebhookEventDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public object? Data { get; set; }
    public bool Livemode { get; set; }
    public int PendingWebhooks { get; set; }
    public string? RequestId { get; set; }
}

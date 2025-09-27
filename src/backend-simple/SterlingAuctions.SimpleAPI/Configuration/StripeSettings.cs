namespace SterlingAuctions.SimpleAPI.Configuration;

/// <summary>
/// Stripe configuration settings
/// </summary>
public class StripeSettings
{
    public const string SectionName = "StripeSettings";
    
    /// <summary>
    /// Stripe secret key
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Stripe publishable key
    /// </summary>
    public string PublishableKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Stripe webhook secret
    /// </summary>
    public string WebhookSecret { get; set; } = string.Empty;
    
    /// <summary>
    /// Application fee percentage (0-100)
    /// </summary>
    public decimal ApplicationFeePercentage { get; set; } = 5.0m;
    
    /// <summary>
    /// Minimum application fee amount
    /// </summary>
    public decimal MinimumApplicationFee { get; set; } = 0.50m;
    
    /// <summary>
    /// Maximum application fee amount
    /// </summary>
    public decimal MaximumApplicationFee { get; set; } = 50.00m;
    
    /// <summary>
    /// Default currency
    /// </summary>
    public string DefaultCurrency { get; set; } = "usd";
    
    /// <summary>
    /// Supported currencies
    /// </summary>
    public string[] SupportedCurrencies { get; set; } = { "usd", "eur", "gbp", "cad", "aud" };
    
    /// <summary>
    /// Payment method types
    /// </summary>
    public string[] PaymentMethodTypes { get; set; } = { "card" };
    
    /// <summary>
    /// Automatic payment methods
    /// </summary>
    public string[] AutomaticPaymentMethods { get; set; } = { "card" };
    
    /// <summary>
    /// Capture method (automatic or manual)
    /// </summary>
    public string CaptureMethod { get; set; } = "automatic";
    
    /// <summary>
    /// Confirmation method (automatic or manual)
    /// </summary>
    public string ConfirmationMethod { get; set; } = "automatic";
    
    /// <summary>
    /// Setup future usage
    /// </summary>
    public string? SetupFutureUsage { get; set; }
    
    /// <summary>
    /// Enable save payment methods
    /// </summary>
    public bool EnableSavePaymentMethods { get; set; } = true;
    
    /// <summary>
    /// Enable refunds
    /// </summary>
    public bool EnableRefunds { get; set; } = true;
    
    /// <summary>
    /// Refund time limit in days
    /// </summary>
    public int RefundTimeLimitDays { get; set; } = 30;
    
    /// <summary>
    /// Enable partial refunds
    /// </summary>
    public bool EnablePartialRefunds { get; set; } = true;
    
    /// <summary>
    /// Webhook endpoint URL
    /// </summary>
    public string WebhookEndpoint { get; set; } = "/api/payments/webhook";
    
    /// <summary>
    /// Enable webhook signature verification
    /// </summary>
    public bool EnableWebhookVerification { get; set; } = true;
    
    /// <summary>
    /// Test mode
    /// </summary>
    public bool TestMode { get; set; } = true;
    
    /// <summary>
    /// Enable logging
    /// </summary>
    public bool EnableLogging { get; set; } = true;
    
    /// <summary>
    /// Log level (Debug, Info, Warning, Error)
    /// </summary>
    public string LogLevel { get; set; } = "Info";
}

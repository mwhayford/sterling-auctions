using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SterlingAuctions.SimpleAPI.Models;
using SterlingAuctions.SimpleAPI.Configuration;
using SterlingAuctions.SimpleAPI.Data;
using SterlingAuctions.SimpleAPI.Services;

namespace SterlingAuctions.SimpleAPI.Services;

/// <summary>
/// Payment service interface for handling payments
/// </summary>
public interface IPaymentService
{
    // Payment Operations
    Task<PaymentDto> CreatePaymentAsync(CreatePaymentIntentDto request, string userId);
    Task<PaymentDto> GetPaymentAsync(int paymentId, string userId);
    Task<IEnumerable<PaymentDto>> GetUserPaymentsAsync(string userId, int page = 1, int pageSize = 20);
    Task<IEnumerable<PaymentDto>> GetAuctionPaymentsAsync(int auctionId, string userId);
    
    // Refund Operations
    Task<PaymentDto> ProcessRefundAsync(RefundRequestDto request, string userId);
    Task<PaymentDto> GetRefundStatusAsync(int paymentId, string userId);
    
    // Statistics
    Task<PaymentStatisticsDto> GetPaymentStatisticsAsync(string userId);
    Task<PaymentStatisticsDto> GetAdminPaymentStatisticsAsync();
    
    // Validation
    Task<bool> ValidatePaymentAmountAsync(decimal amount, string currency);
    Task<bool> CanRefundPaymentAsync(int paymentId, string userId);
    Task<bool> IsPaymentCompletedAsync(int paymentId);
}

/// <summary>
/// Simple payment service implementation (mock for development)
/// </summary>
public class SimplePaymentService : IPaymentService
{
    private readonly ILogger<SimplePaymentService> _logger;
    private readonly IOptions<StripeSettings> _stripeSettings;
    private readonly ApplicationDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly INotificationService _notificationService;

    public SimplePaymentService(
        ILogger<SimplePaymentService> logger,
        IOptions<StripeSettings> stripeSettings,
        ApplicationDbContext context,
        ICacheService cacheService,
        INotificationService notificationService)
    {
        _logger = logger;
        _stripeSettings = stripeSettings;
        _context = context;
        _cacheService = cacheService;
        _notificationService = notificationService;
    }

    public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentIntentDto request, string userId)
    {
        try
        {
            _logger.LogInformation("Creating payment for user {UserId}, auction {AuctionId}, amount {Amount}", 
                userId, request.AuctionId, request.Amount);

            // Validate payment amount
            if (!await ValidatePaymentAmountAsync(request.Amount, request.Currency))
            {
                throw new ArgumentException("Invalid payment amount or currency");
            }

            // Create payment record
            var payment = new Payment
            {
                UserId = userId,
                AuctionId = request.AuctionId,
                Amount = request.Amount,
                Currency = request.Currency.ToUpper(),
                Status = PaymentStatus.Completed, // Mock as completed for development
                Method = PaymentMethod.Stripe,
                TransactionId = $"TXN-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..16].ToUpper(),
                Reference = $"PAY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..16].ToUpper(),
                CompletedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created payment record {PaymentId} for user {UserId}", 
                payment.Id, userId);

            // Send notification
            await _notificationService.NotifyUserWonAuctionAsync(
                payment.UserId, 
                payment.AuctionId, 
                payment.Amount
            );

            return MapToPaymentDto(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment for user {UserId}", userId);
            throw;
        }
    }

    public async Task<PaymentDto> GetPaymentAsync(int paymentId, string userId)
    {
        var payment = await _context.Payments
            .Include(p => p.Auction)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == paymentId && p.UserId == userId);

        if (payment == null)
        {
            throw new ArgumentException("Payment not found");
        }

        return MapToPaymentDto(payment);
    }

    public async Task<IEnumerable<PaymentDto>> GetUserPaymentsAsync(string userId, int page = 1, int pageSize = 20)
    {
        var payments = await _context.Payments
            .Include(p => p.Auction)
            .Include(p => p.User)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return payments.Select(MapToPaymentDto);
    }

    public async Task<IEnumerable<PaymentDto>> GetAuctionPaymentsAsync(int auctionId, string userId)
    {
        var payments = await _context.Payments
            .Include(p => p.Auction)
            .Include(p => p.User)
            .Where(p => p.AuctionId == auctionId && p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return payments.Select(MapToPaymentDto);
    }

    public async Task<PaymentDto> ProcessRefundAsync(RefundRequestDto request, string userId)
    {
        try
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == request.PaymentId && p.UserId == userId);

            if (payment == null)
            {
                throw new ArgumentException("Payment not found");
            }

            if (!await CanRefundPaymentAsync(request.PaymentId, userId))
            {
                throw new InvalidOperationException("Payment cannot be refunded");
            }

            var refundAmount = request.Amount ?? payment.Amount;

            // Update payment record
            payment.Status = refundAmount == payment.Amount ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded;
            payment.RefundAmount = refundAmount;
            payment.RefundedAt = DateTime.UtcNow;
            payment.RefundReason = request.Reason;
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Processed refund for payment {PaymentId}", payment.Id);

            // Send notification
            await _notificationService.NotifySystemAnnouncementAsync(
                $"Refund processed for auction #{payment.AuctionId}. Amount: ${refundAmount:F2}"
            );

            return MapToPaymentDto(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund for payment {PaymentId}", request.PaymentId);
            throw;
        }
    }

    public async Task<PaymentDto> GetRefundStatusAsync(int paymentId, string userId)
    {
        return await GetPaymentAsync(paymentId, userId);
    }

    public async Task<PaymentStatisticsDto> GetPaymentStatisticsAsync(string userId)
    {
        var payments = await _context.Payments
            .Where(p => p.UserId == userId)
            .ToListAsync();

        var totalRevenue = payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount);
        var totalRefunds = payments.Where(p => p.Status == PaymentStatus.Refunded).Sum(p => p.RefundAmount ?? 0);
        var netRevenue = totalRevenue - totalRefunds;
        var totalPayments = payments.Count;
        var successfulPayments = payments.Count(p => p.Status == PaymentStatus.Completed);
        var failedPayments = payments.Count(p => p.Status == PaymentStatus.Failed);
        var refundedPayments = payments.Count(p => p.Status == PaymentStatus.Refunded);
        var successRate = totalPayments > 0 ? (decimal)successfulPayments / totalPayments * 100 : 0;

        return new PaymentStatisticsDto
        {
            TotalRevenue = totalRevenue,
            TotalRefunds = totalRefunds,
            NetRevenue = netRevenue,
            TotalPayments = totalPayments,
            SuccessfulPayments = successfulPayments,
            FailedPayments = failedPayments,
            RefundedPayments = refundedPayments,
            SuccessRate = successRate,
            RevenueByCurrency = payments
                .Where(p => p.Status == PaymentStatus.Completed)
                .GroupBy(p => p.Currency)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount)),
            PaymentsByMethod = payments
                .GroupBy(p => p.Method.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            PaymentsByStatus = payments
                .GroupBy(p => p.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public async Task<PaymentStatisticsDto> GetAdminPaymentStatisticsAsync()
    {
        var payments = await _context.Payments.ToListAsync();
        
        var totalRevenue = payments.Where(p => p.Status == PaymentStatus.Completed).Sum(p => p.Amount);
        var totalRefunds = payments.Where(p => p.Status == PaymentStatus.Refunded).Sum(p => p.RefundAmount ?? 0);
        var netRevenue = totalRevenue - totalRefunds;
        var totalPayments = payments.Count;
        var successfulPayments = payments.Count(p => p.Status == PaymentStatus.Completed);
        var failedPayments = payments.Count(p => p.Status == PaymentStatus.Failed);
        var refundedPayments = payments.Count(p => p.Status == PaymentStatus.Refunded);
        var successRate = totalPayments > 0 ? (decimal)successfulPayments / totalPayments * 100 : 0;

        return new PaymentStatisticsDto
        {
            TotalRevenue = totalRevenue,
            TotalRefunds = totalRefunds,
            NetRevenue = netRevenue,
            TotalPayments = totalPayments,
            SuccessfulPayments = successfulPayments,
            FailedPayments = failedPayments,
            RefundedPayments = refundedPayments,
            SuccessRate = successRate,
            RevenueByCurrency = payments
                .Where(p => p.Status == PaymentStatus.Completed)
                .GroupBy(p => p.Currency)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount)),
            PaymentsByMethod = payments
                .GroupBy(p => p.Method.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            PaymentsByStatus = payments
                .GroupBy(p => p.Status.ToString())
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public async Task<bool> ValidatePaymentAmountAsync(decimal amount, string currency)
    {
        if (amount <= 0) return false;
        if (!_stripeSettings.Value.SupportedCurrencies.Contains(currency.ToLower())) return false;
        return true;
    }

    public async Task<bool> CanRefundPaymentAsync(int paymentId, string userId)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == paymentId && p.UserId == userId);

        if (payment == null) return false;
        if (payment.Status != PaymentStatus.Completed) return false;
        if (payment.RefundedAt.HasValue) return false;
        
        var refundTimeLimit = payment.CompletedAt?.AddDays(_stripeSettings.Value.RefundTimeLimitDays);
        if (refundTimeLimit.HasValue && DateTime.UtcNow > refundTimeLimit.Value) return false;

        return true;
    }

    public async Task<bool> IsPaymentCompletedAsync(int paymentId)
    {
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        return payment?.Status == PaymentStatus.Completed;
    }

    private PaymentDto MapToPaymentDto(Payment payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            UserId = payment.UserId,
            AuctionId = payment.AuctionId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Status = payment.Status,
            Method = payment.Method,
            TransactionId = payment.TransactionId,
            Reference = payment.Reference,
            FailureReason = payment.FailureReason,
            RefundAmount = payment.RefundAmount,
            CreatedAt = payment.CreatedAt,
            UpdatedAt = payment.UpdatedAt,
            CompletedAt = payment.CompletedAt,
            RefundedAt = payment.RefundedAt,
            AuctionTitle = payment.Auction?.Title,
            UserName = payment.User?.UserName
        };
    }
}

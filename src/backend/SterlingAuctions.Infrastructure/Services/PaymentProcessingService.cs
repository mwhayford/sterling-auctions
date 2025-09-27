using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SterlingAuctions.Core.Entities;
using SterlingAuctions.Infrastructure.Data;

namespace SterlingAuctions.Infrastructure.Services;

/// <summary>
/// Background service for processing payments and payment-related tasks
/// </summary>
public class PaymentProcessingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaymentProcessingService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); // Check every 5 minutes

    public PaymentProcessingService(IServiceProvider serviceProvider, ILogger<PaymentProcessingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Payment Processing Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                await ProcessPendingPayments(context, stoppingToken);
                await ProcessPaymentReminders(context, stoppingToken);
                await ProcessExpiredPayments(context, stoppingToken);

                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in payment processing service");
                
                // Wait longer on error
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        _logger.LogInformation("Payment Processing Service stopped");
    }

    private async Task ProcessPendingPayments(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        try
        {
            // Find payments that have been pending for too long
            var stalePendingPayments = await context.Payments
                .Where(p => p.Status == PaymentStatus.Pending && 
                           p.CreatedAt <= DateTime.UtcNow.AddHours(-2)) // Pending for more than 2 hours
                .ToListAsync(cancellationToken);

            foreach (var payment in stalePendingPayments)
            {
                _logger.LogWarning("Payment {PaymentId} has been pending for over 2 hours, marking as failed", payment.Id);
                
                payment.Status = PaymentStatus.Failed;
                payment.FailureReason = "Payment timeout - pending for over 2 hours";
                payment.UpdatedAt = DateTime.UtcNow;
                
                // TODO: Send payment failed notification
                // TODO: Update related bid status if applicable
            }

            if (stalePendingPayments.Any())
            {
                await context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing pending payments");
        }
    }

    private async Task ProcessPaymentReminders(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        try
        {
            // Find winning bids that need payment but haven't been paid yet
            var unpaidWinningBids = await context.Bids
                .Where(b => b.Status == BidStatus.Won && 
                           b.CreatedAt <= DateTime.UtcNow.AddDays(-1)) // Won more than 1 day ago
                .Include(b => b.Bidder)
                .Include(b => b.AuctionItem)
                    .ThenInclude(i => i.Auction)
                .Where(b => !context.Payments.Any(p => p.BidId == b.Id && 
                                                      p.Type == PaymentType.AuctionPayment && 
                                                      p.Status == PaymentStatus.Completed))
                .ToListAsync(cancellationToken);

            foreach (var bid in unpaidWinningBids)
            {
                var daysSinceWin = (DateTime.UtcNow - bid.CreatedAt).Days;
                
                if (daysSinceWin == 1)
                {
                    _logger.LogInformation("Sending payment reminder for bid {BidId} - 1 day overdue", bid.Id);
                    // TODO: Send first payment reminder
                }
                else if (daysSinceWin == 3)
                {
                    _logger.LogInformation("Sending second payment reminder for bid {BidId} - 3 days overdue", bid.Id);
                    // TODO: Send second payment reminder
                }
                else if (daysSinceWin == 7)
                {
                    _logger.LogWarning("Payment for bid {BidId} is 7 days overdue, escalating", bid.Id);
                    // TODO: Send final payment reminder
                    // TODO: Notify administrators
                }
                else if (daysSinceWin >= 14)
                {
                    _logger.LogError("Payment for bid {BidId} is 14+ days overdue, considering cancellation", bid.Id);
                    // TODO: Consider cancelling the sale and offering to next highest bidder
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment reminders");
        }
    }

    private async Task ProcessExpiredPayments(ApplicationDbContext context, CancellationToken cancellationToken)
    {
        try
        {
            // Find payments that have been in various states for too long
            var expiredPayments = await context.Payments
                .Where(p => (p.Status == PaymentStatus.Pending && p.CreatedAt <= DateTime.UtcNow.AddDays(-7)) ||
                           (p.Status == PaymentStatus.Failed && p.CreatedAt <= DateTime.UtcNow.AddDays(-30)))
                .ToListAsync(cancellationToken);

            foreach (var payment in expiredPayments)
            {
                if (payment.Status == PaymentStatus.Pending)
                {
                    _logger.LogWarning("Cancelling payment {PaymentId} - pending for over 7 days", payment.Id);
                    payment.Status = PaymentStatus.Cancelled;
                    payment.FailureReason = "Payment cancelled due to timeout";
                }
                
                payment.UpdatedAt = DateTime.UtcNow;
            }

            if (expiredPayments.Any())
            {
                await context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing expired payments");
        }
    }
}

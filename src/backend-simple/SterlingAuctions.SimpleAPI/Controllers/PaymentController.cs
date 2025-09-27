using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SterlingAuctions.SimpleAPI.Models;
using SterlingAuctions.SimpleAPI.Services;
using System.Security.Claims;

namespace SterlingAuctions.SimpleAPI.Controllers;

/// <summary>
/// Payment controller for handling payment operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Create a payment record
    /// </summary>
    /// <param name="request">Payment creation request</param>
    /// <returns>Created payment details</returns>
    [HttpPost("create")]
    public async Task<ActionResult<PaymentDto>> CreatePayment([FromBody] CreatePaymentIntentDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            _logger.LogInformation("Creating payment for user {UserId}, auction {AuctionId}", userId, request.AuctionId);

            var payment = await _paymentService.CreatePaymentAsync(request, userId);
            return Ok(payment);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid payment creation request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating payment");
            return StatusCode(500, "An error occurred while creating the payment");
        }
    }

    /// <summary>
    /// Get payment details by ID
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <returns>Payment details</returns>
    [HttpGet("{paymentId}")]
    public async Task<ActionResult<PaymentDto>> GetPayment(int paymentId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var payment = await _paymentService.GetPaymentAsync(paymentId, userId);
            return Ok(payment);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Payment not found: {PaymentId}", paymentId);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment {PaymentId}", paymentId);
            return StatusCode(500, "An error occurred while retrieving the payment");
        }
    }

    /// <summary>
    /// Get user's payment history
    /// </summary>
    /// <param name="page">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>List of user payments</returns>
    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetUserPayments(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var payments = await _paymentService.GetUserPaymentsAsync(userId, page, pageSize);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user payments");
            return StatusCode(500, "An error occurred while retrieving payments");
        }
    }

    /// <summary>
    /// Get payments for a specific auction
    /// </summary>
    /// <param name="auctionId">Auction ID</param>
    /// <returns>List of auction payments</returns>
    [HttpGet("auction/{auctionId}")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAuctionPayments(int auctionId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var payments = await _paymentService.GetAuctionPaymentsAsync(auctionId, userId);
            return Ok(payments);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving auction payments for auction {AuctionId}", auctionId);
            return StatusCode(500, "An error occurred while retrieving auction payments");
        }
    }

    /// <summary>
    /// Process a refund for a payment
    /// </summary>
    /// <param name="request">Refund request</param>
    /// <returns>Updated payment details</returns>
    [HttpPost("refund")]
    public async Task<ActionResult<PaymentDto>> ProcessRefund([FromBody] RefundRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            _logger.LogInformation("Processing refund for payment {PaymentId} by user {UserId}", 
                request.PaymentId, userId);

            var payment = await _paymentService.ProcessRefundAsync(request, userId);
            return Ok(payment);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid refund request");
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Refund not allowed");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing refund");
            return StatusCode(500, "An error occurred while processing the refund");
        }
    }

    /// <summary>
    /// Get refund status for a payment
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <returns>Payment details with refund status</returns>
    [HttpGet("refund-status/{paymentId}")]
    public async Task<ActionResult<PaymentDto>> GetRefundStatus(int paymentId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var payment = await _paymentService.GetRefundStatusAsync(paymentId, userId);
            return Ok(payment);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Payment not found: {PaymentId}", paymentId);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving refund status for payment {PaymentId}", paymentId);
            return StatusCode(500, "An error occurred while retrieving refund status");
        }
    }

    /// <summary>
    /// Get payment statistics for the current user
    /// </summary>
    /// <returns>User payment statistics</returns>
    [HttpGet("statistics")]
    public async Task<ActionResult<PaymentStatisticsDto>> GetPaymentStatistics()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var statistics = await _paymentService.GetPaymentStatisticsAsync(userId);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving payment statistics");
            return StatusCode(500, "An error occurred while retrieving payment statistics");
        }
    }

    /// <summary>
    /// Get admin payment statistics
    /// </summary>
    /// <returns>Admin payment statistics</returns>
    [HttpGet("admin/statistics")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PaymentStatisticsDto>> GetAdminPaymentStatistics()
    {
        try
        {
            var statistics = await _paymentService.GetAdminPaymentStatisticsAsync();
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin payment statistics");
            return StatusCode(500, "An error occurred while retrieving admin payment statistics");
        }
    }

    /// <summary>
    /// Validate payment amount and currency
    /// </summary>
    /// <param name="amount">Payment amount</param>
    /// <param name="currency">Payment currency</param>
    /// <returns>Validation result</returns>
    [HttpGet("validate")]
    public async Task<ActionResult<bool>> ValidatePayment([FromQuery] decimal amount, [FromQuery] string currency)
    {
        try
        {
            var isValid = await _paymentService.ValidatePaymentAmountAsync(amount, currency);
            return Ok(new { isValid });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating payment");
            return StatusCode(500, "An error occurred while validating the payment");
        }
    }

    /// <summary>
    /// Check if a payment can be refunded
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <returns>Refund eligibility</returns>
    [HttpGet("can-refund/{paymentId}")]
    public async Task<ActionResult<bool>> CanRefundPayment(int paymentId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var canRefund = await _paymentService.CanRefundPaymentAsync(paymentId, userId);
            return Ok(new { canRefund });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking refund eligibility for payment {PaymentId}", paymentId);
            return StatusCode(500, "An error occurred while checking refund eligibility");
        }
    }

    /// <summary>
    /// Check if a payment is completed
    /// </summary>
    /// <param name="paymentId">Payment ID</param>
    /// <returns>Payment completion status</returns>
    [HttpGet("is-completed/{paymentId}")]
    public async Task<ActionResult<bool>> IsPaymentCompleted(int paymentId)
    {
        try
        {
            var isCompleted = await _paymentService.IsPaymentCompletedAsync(paymentId);
            return Ok(new { isCompleted });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking payment completion status for payment {PaymentId}", paymentId);
            return StatusCode(500, "An error occurred while checking payment completion status");
        }
    }
}

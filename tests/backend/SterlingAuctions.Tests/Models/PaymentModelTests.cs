using FluentAssertions;
using SterlingAuctions.SimpleAPI.Models;

namespace SterlingAuctions.Tests.Models;

/// <summary>
/// Basic tests for Payment model
/// </summary>
public class PaymentModelTests
{
    [Test]
    public void Payment_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var payment = new Payment
        {
            Id = 1,
            UserId = "user123",
            AuctionId = 1,
            Amount = 150.00m,
            Currency = "USD",
            Status = PaymentStatus.Pending,
            Method = PaymentMethod.Stripe,
            TransactionId = "txn_123",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        payment.Should().NotBeNull();
        payment.Id.Should().Be(1);
        payment.UserId.Should().Be("user123");
        payment.AuctionId.Should().Be(1);
        payment.Amount.Should().Be(150.00m);
        payment.Currency.Should().Be("USD");
        payment.Status.Should().Be(PaymentStatus.Pending);
        payment.Method.Should().Be(PaymentMethod.Stripe);
        payment.TransactionId.Should().Be("txn_123");
    }

    [Test]
    public void PaymentStatus_ShouldHaveCorrectValues()
    {
        // Assert
        PaymentStatus.Pending.Should().Be(PaymentStatus.Pending);
        PaymentStatus.Processing.Should().Be(PaymentStatus.Processing);
        PaymentStatus.Completed.Should().Be(PaymentStatus.Completed);
        PaymentStatus.Failed.Should().Be(PaymentStatus.Failed);
        PaymentStatus.Cancelled.Should().Be(PaymentStatus.Cancelled);
        PaymentStatus.Refunded.Should().Be(PaymentStatus.Refunded);
        PaymentStatus.PartiallyRefunded.Should().Be(PaymentStatus.PartiallyRefunded);
    }

    [Test]
    public void PaymentMethod_ShouldHaveCorrectValues()
    {
        // Assert
        PaymentMethod.CreditCard.Should().Be(PaymentMethod.CreditCard);
        PaymentMethod.DebitCard.Should().Be(PaymentMethod.DebitCard);
        PaymentMethod.BankTransfer.Should().Be(PaymentMethod.BankTransfer);
        PaymentMethod.PayPal.Should().Be(PaymentMethod.PayPal);
        PaymentMethod.ApplePay.Should().Be(PaymentMethod.ApplePay);
        PaymentMethod.GooglePay.Should().Be(PaymentMethod.GooglePay);
        PaymentMethod.Stripe.Should().Be(PaymentMethod.Stripe);
    }
}

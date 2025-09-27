using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SterlingAuctions.SimpleAPI.Configuration;
using SterlingAuctions.SimpleAPI.Models;
using SterlingAuctions.SimpleAPI.Services;

namespace SterlingAuctions.Tests.Services;

/// <summary>
/// Basic tests for SimplePaymentService
/// </summary>
public class SimplePaymentServiceTests
{
    [Test]
    public void PaymentService_Interface_ShouldBeDefined()
    {
        // Assert
        typeof(IPaymentService).Should().NotBeNull();
        typeof(IPaymentService).IsInterface.Should().BeTrue();
    }

    [Test]
    public void PaymentService_ShouldHaveRequiredMethods()
    {
        // Arrange
        var interfaceType = typeof(IPaymentService);
        
        // Act & Assert
        interfaceType.GetMethod("CreatePaymentAsync").Should().NotBeNull();
        interfaceType.GetMethod("GetPaymentAsync").Should().NotBeNull();
        interfaceType.GetMethod("GetUserPaymentsAsync").Should().NotBeNull();
        interfaceType.GetMethod("GetAuctionPaymentsAsync").Should().NotBeNull();
        interfaceType.GetMethod("ProcessRefundAsync").Should().NotBeNull();
        interfaceType.GetMethod("GetRefundStatusAsync").Should().NotBeNull();
        interfaceType.GetMethod("GetPaymentStatisticsAsync").Should().NotBeNull();
        interfaceType.GetMethod("GetAdminPaymentStatisticsAsync").Should().NotBeNull();
        interfaceType.GetMethod("ValidatePaymentAmountAsync").Should().NotBeNull();
        interfaceType.GetMethod("CanRefundPaymentAsync").Should().NotBeNull();
        interfaceType.GetMethod("IsPaymentCompletedAsync").Should().NotBeNull();
    }

    [Test]
    public void PaymentModels_ShouldBeDefined()
    {
        // Assert
        typeof(Payment).Should().NotBeNull();
        typeof(PaymentStatus).Should().NotBeNull();
        typeof(PaymentMethod).Should().NotBeNull();
        typeof(CreatePaymentIntentDto).Should().NotBeNull();
        typeof(PaymentDto).Should().NotBeNull();
        typeof(PaymentStatisticsDto).Should().NotBeNull();
        typeof(RefundRequestDto).Should().NotBeNull();
    }

    [Test]
    public void PaymentEnums_ShouldHaveCorrectValues()
    {
        // Assert PaymentStatus
        Enum.GetValues<PaymentStatus>().Should().HaveCount(7);
        Enum.GetValues<PaymentStatus>().Should().Contain(PaymentStatus.Pending);
        Enum.GetValues<PaymentStatus>().Should().Contain(PaymentStatus.Processing);
        Enum.GetValues<PaymentStatus>().Should().Contain(PaymentStatus.Completed);
        Enum.GetValues<PaymentStatus>().Should().Contain(PaymentStatus.Failed);
        Enum.GetValues<PaymentStatus>().Should().Contain(PaymentStatus.Cancelled);
        Enum.GetValues<PaymentStatus>().Should().Contain(PaymentStatus.Refunded);
        Enum.GetValues<PaymentStatus>().Should().Contain(PaymentStatus.PartiallyRefunded);

        // Assert PaymentMethod
        Enum.GetValues<PaymentMethod>().Should().HaveCount(7);
        Enum.GetValues<PaymentMethod>().Should().Contain(PaymentMethod.CreditCard);
        Enum.GetValues<PaymentMethod>().Should().Contain(PaymentMethod.DebitCard);
        Enum.GetValues<PaymentMethod>().Should().Contain(PaymentMethod.BankTransfer);
        Enum.GetValues<PaymentMethod>().Should().Contain(PaymentMethod.PayPal);
        Enum.GetValues<PaymentMethod>().Should().Contain(PaymentMethod.ApplePay);
        Enum.GetValues<PaymentMethod>().Should().Contain(PaymentMethod.GooglePay);
        Enum.GetValues<PaymentMethod>().Should().Contain(PaymentMethod.Stripe);
    }
}

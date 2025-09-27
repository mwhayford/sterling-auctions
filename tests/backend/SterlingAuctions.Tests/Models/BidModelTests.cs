using FluentAssertions;
using SterlingAuctions.SimpleAPI.Models;

namespace SterlingAuctions.Tests.Models;

/// <summary>
/// Basic tests for Bid model
/// </summary>
public class BidModelTests
{
    [Test]
    public void Bid_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var bid = new Bid
        {
            Id = 1,
            AuctionId = 1,
            BidderId = "user123",
            Amount = 150.00m,
            BidTime = DateTime.UtcNow,
            IsWinningBid = true
        };

        // Assert
        bid.Should().NotBeNull();
        bid.Id.Should().Be(1);
        bid.AuctionId.Should().Be(1);
        bid.BidderId.Should().Be("user123");
        bid.Amount.Should().Be(150.00m);
        bid.IsWinningBid.Should().BeTrue();
    }
}

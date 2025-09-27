using FluentAssertions;
using SterlingAuctions.SimpleAPI.Models;

namespace SterlingAuctions.Tests.Models;

/// <summary>
/// Basic tests for Auction model
/// </summary>
public class AuctionModelTests
{
    [Test]
    public void Auction_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var auction = new Auction
        {
            Id = 1,
            Title = "Test Auction",
            Description = "Test Description",
            StartingBid = 100.00m,
            CurrentBid = 100.00m,
            CreatedBy = "user123",
            CreatedDate = DateTime.UtcNow,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddDays(7),
            Status = AuctionStatus.Active,
            CategoryId = 1
        };

        // Assert
        auction.Should().NotBeNull();
        auction.Id.Should().Be(1);
        auction.Title.Should().Be("Test Auction");
        auction.Description.Should().Be("Test Description");
        auction.StartingBid.Should().Be(100.00m);
        auction.CurrentBid.Should().Be(100.00m);
        auction.CreatedBy.Should().Be("user123");
        auction.Status.Should().Be(AuctionStatus.Active);
        auction.CategoryId.Should().Be(1);
    }

    [Test]
    public void AuctionStatus_ShouldHaveCorrectValues()
    {
        // Assert
        AuctionStatus.Scheduled.Should().Be(AuctionStatus.Scheduled);
        AuctionStatus.Active.Should().Be(AuctionStatus.Active);
        AuctionStatus.Ended.Should().Be(AuctionStatus.Ended);
        AuctionStatus.Cancelled.Should().Be(AuctionStatus.Cancelled);
        AuctionStatus.Sold.Should().Be(AuctionStatus.Sold);
    }
}

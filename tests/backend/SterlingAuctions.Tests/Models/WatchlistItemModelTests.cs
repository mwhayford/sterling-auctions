using FluentAssertions;
using SterlingAuctions.SimpleAPI.Models;

namespace SterlingAuctions.Tests.Models;

/// <summary>
/// Basic tests for WatchlistItem model
/// </summary>
public class WatchlistItemModelTests
{
    [Test]
    public void WatchlistItem_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var watchlistItem = new WatchlistItem
        {
            Id = 1,
            UserId = "user123",
            AuctionId = 1,
            AddedDate = DateTime.UtcNow
        };

        // Assert
        watchlistItem.Should().NotBeNull();
        watchlistItem.Id.Should().Be(1);
        watchlistItem.UserId.Should().Be("user123");
        watchlistItem.AuctionId.Should().Be(1);
    }
}

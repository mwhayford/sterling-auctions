using FluentAssertions;
using SterlingAuctions.SimpleAPI.Models;

namespace SterlingAuctions.Tests.Models;

/// <summary>
/// Basic tests for AuctionImage model
/// </summary>
public class AuctionImageModelTests
{
    [Test]
    public void AuctionImage_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var image = new AuctionImage
        {
            Id = 1,
            AuctionId = 1,
            ImageUrl = "https://example.com/image.jpg",
            AltText = "Test image",
            IsPrimary = true,
            SortOrder = 1
        };

        // Assert
        image.Should().NotBeNull();
        image.Id.Should().Be(1);
        image.AuctionId.Should().Be(1);
        image.ImageUrl.Should().Be("https://example.com/image.jpg");
        image.AltText.Should().Be("Test image");
        image.IsPrimary.Should().BeTrue();
        image.SortOrder.Should().Be(1);
    }
}

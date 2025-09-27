using FluentAssertions;
using SterlingAuctions.SimpleAPI.Models;

namespace SterlingAuctions.Tests.Models;

/// <summary>
/// Basic tests for Category model
/// </summary>
public class CategoryModelTests
{
    [Test]
    public void Category_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var category = new Category
        {
            Id = 1,
            Name = "Electronics",
            Description = "Electronic items",
            IsActive = true
        };

        // Assert
        category.Should().NotBeNull();
        category.Id.Should().Be(1);
        category.Name.Should().Be("Electronics");
        category.Description.Should().Be("Electronic items");
        category.IsActive.Should().BeTrue();
    }
}

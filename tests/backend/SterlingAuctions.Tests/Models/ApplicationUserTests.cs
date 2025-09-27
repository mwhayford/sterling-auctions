using FluentAssertions;
using SterlingAuctions.SimpleAPI.Models;

namespace SterlingAuctions.Tests.Models;

/// <summary>
/// Basic tests for ApplicationUser model
/// </summary>
public class ApplicationUserTests
{
    [Test]
    public void ApplicationUser_ShouldCreateSuccessfully()
    {
        // Arrange & Act
        var user = new ApplicationUser
        {
            Id = "user123",
            UserName = "testuser",
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().Be("user123");
        user.UserName.Should().Be("testuser");
        user.Email.Should().Be("test@example.com");
        user.FirstName.Should().Be("Test");
        user.LastName.Should().Be("User");
        user.IsActive.Should().BeTrue();
    }

    [Test]
    public void ApplicationUser_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var user = new ApplicationUser();

        // Assert
        user.Should().NotBeNull();
        user.IsActive.Should().BeTrue(); // Default value
    }
}

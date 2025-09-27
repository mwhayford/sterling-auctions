using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SterlingAuctions.API.Models;

/// <summary>
/// Application user entity extending IdentityUser
/// </summary>
public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// User's first name
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// User's full name (computed property)
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// User's profile image URL
    /// </summary>
    [MaxLength(500)]
    public string? ProfileImageUrl { get; set; }

    /// <summary>
    /// Indicates if the user account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Date and time when the user was created
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the user was last modified
    /// </summary>
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
}

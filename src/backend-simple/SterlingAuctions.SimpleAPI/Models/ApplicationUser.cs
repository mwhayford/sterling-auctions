using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SterlingAuctions.SimpleAPI.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;
}

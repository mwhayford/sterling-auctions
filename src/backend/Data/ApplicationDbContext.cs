using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SterlingAuctions.API.Models;

namespace SterlingAuctions.API.Data;

/// <summary>
/// Application database context
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.LastModifiedDate).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}

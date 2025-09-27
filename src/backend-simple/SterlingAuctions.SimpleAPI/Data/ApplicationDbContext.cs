using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SterlingAuctions.SimpleAPI.Models;

namespace SterlingAuctions.SimpleAPI.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Auction> Auctions { get; set; }
    public DbSet<Bid> Bids { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<AuctionImage> AuctionImages { get; set; }
    public DbSet<WatchlistItem> WatchlistItems { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ApplicationUser configuration
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        // Payment configuration
        builder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.RefundAmount).HasPrecision(18, 2);
            entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Method).HasConversion<string>();
            entity.Property(e => e.StripePaymentIntentId).HasMaxLength(100);
            entity.Property(e => e.StripeChargeId).HasMaxLength(100);
            entity.Property(e => e.StripeCustomerId).HasMaxLength(100);
            entity.Property(e => e.TransactionId).HasMaxLength(100);
            entity.Property(e => e.Reference).HasMaxLength(50);
            entity.Property(e => e.FailureReason).HasMaxLength(500);
            entity.Property(e => e.RefundReason).HasMaxLength(500);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Auction)
                .WithMany()
                .HasForeignKey(e => e.AuctionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.AuctionId);
            entity.HasIndex(e => e.StripePaymentIntentId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Auction configuration
        builder.Entity<Auction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.StartingBid).HasPrecision(18, 2);
            entity.Property(e => e.CurrentBid).HasPrecision(18, 2);
            entity.Property(e => e.ReservePrice).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(450);

            entity.HasOne(e => e.Category)
                .WithMany()
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.CreatedBy);
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartTime);
            entity.HasIndex(e => e.EndTime);
        });

        // Bid configuration
        builder.Entity<Bid>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.MaxBidAmount).HasPrecision(18, 2);
            entity.Property(e => e.BidderId).IsRequired().HasMaxLength(450);

            entity.HasOne(e => e.Auction)
                .WithMany()
                .HasForeignKey(e => e.AuctionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.AuctionId);
            entity.HasIndex(e => e.BidderId);
            entity.HasIndex(e => e.BidTime);
        });

        // Category configuration
        builder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // AuctionImage configuration
        builder.Entity<AuctionImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(500);
            entity.Property(e => e.AltText).HasMaxLength(200);

            entity.HasOne(e => e.Auction)
                .WithMany()
                .HasForeignKey(e => e.AuctionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.AuctionId);
        });

        // WatchlistItem configuration
        builder.Entity<WatchlistItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);

            entity.HasOne(e => e.Auction)
                .WithMany()
                .HasForeignKey(e => e.AuctionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.AuctionId);
            entity.HasIndex(e => new { e.UserId, e.AuctionId }).IsUnique();
        });
    }
}

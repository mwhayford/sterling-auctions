using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SterlingAuctions.Core.Entities;
using System.Linq.Expressions;
using System.Text.Json;

namespace SterlingAuctions.Infrastructure.Data;

/// <summary>
/// Application database context
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets for entities
    public DbSet<Auction> Auctions { get; set; }
    public DbSet<AuctionItem> AuctionItems { get; set; }
    public DbSet<AuctionParticipant> AuctionParticipants { get; set; }
    public DbSet<Bid> Bids { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ItemImage> ItemImages { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Watchlist> Watchlists { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure ApplicationUser
        ConfigureApplicationUser(builder);

        // Configure entities
        ConfigureAuction(builder);
        ConfigureAuctionItem(builder);
        ConfigureAuctionParticipant(builder);
        ConfigureBid(builder);
        ConfigureCategory(builder);
        ConfigureItemImage(builder);
        ConfigureNotification(builder);
        ConfigurePayment(builder);
        ConfigureWatchlist(builder);

        // Configure indexes for performance
        ConfigureIndexes(builder);

        // Apply global query filters for soft delete
        ApplyGlobalFilters(builder);
    }

    private static void ConfigureApplicationUser(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);
            entity.Property(e => e.Address).HasMaxLength(200);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Country).HasMaxLength(100);

            // Configure NotificationPreferences as owned entity
            entity.OwnsOne(e => e.NotificationPreferences, np =>
            {
                np.Property(p => p.EmailOutbidAlerts).HasDefaultValue(true);
                np.Property(p => p.EmailAuctionEndingSoon).HasDefaultValue(true);
                np.Property(p => p.EmailNewAuctions).HasDefaultValue(false);
                np.Property(p => p.SmsOutbidAlerts).HasDefaultValue(false);
                np.Property(p => p.SmsAuctionEndingSoon).HasDefaultValue(false);
                np.Property(p => p.PushNotifications).HasDefaultValue(true);
                np.Property(p => p.MarketingEmails).HasDefaultValue(false);
            });

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.IsVerified);
        });
    }

    private static void ConfigureAuction(ModelBuilder builder)
    {
        builder.Entity<Auction>(entity =>
        {
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.ExtensionTimeMinutes).HasDefaultValue(5);
            entity.Property(e => e.ExtensionThresholdMinutes).HasDefaultValue(5);
            entity.Property(e => e.AutoExtend).HasDefaultValue(true);
            entity.Property(e => e.IsFeatured).HasDefaultValue(false);
            entity.Property(e => e.RequireVerification).HasDefaultValue(false);

            // Foreign key relationships
            entity.HasOne(e => e.Seller)
                  .WithMany(u => u.Auctions)
                  .HasForeignKey(e => e.SellerId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Configure relationships
            entity.HasMany(e => e.Items)
                  .WithOne(i => i.Auction)
                  .HasForeignKey(i => i.AuctionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Participants)
                  .WithOne(p => p.Auction)
                  .HasForeignKey(p => p.AuctionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Bids)
                  .WithOne(b => b.Auction)
                  .HasForeignKey(b => b.AuctionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Payments)
                  .WithOne(p => p.Auction)
                  .HasForeignKey(p => p.AuctionId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureAuctionItem(ModelBuilder builder)
    {
        builder.Entity<AuctionItem>(entity =>
        {
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Condition).HasConversion<string>();
            entity.Property(e => e.ConditionDescription).HasMaxLength(500);
            entity.Property(e => e.Dimensions).HasMaxLength(100);
            entity.Property(e => e.Weight).HasMaxLength(50);
            entity.Property(e => e.Material).HasMaxLength(200);
            entity.Property(e => e.Artist).HasMaxLength(200);
            entity.Property(e => e.Origin).HasMaxLength(100);
            entity.Property(e => e.Provenance).HasMaxLength(1000);
            entity.Property(e => e.BidIncrement).HasDefaultValue(1.00m);
            entity.Property(e => e.HasReserve).HasDefaultValue(false);
            entity.Property(e => e.ReserveMet).HasDefaultValue(false);
            entity.Property(e => e.IsFeatured).HasDefaultValue(false);
            entity.Property(e => e.BidCount).HasDefaultValue(0);
            entity.Property(e => e.WatchCount).HasDefaultValue(0);
            entity.Property(e => e.ViewCount).HasDefaultValue(0);
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);

            // Foreign key relationships
            entity.HasOne(e => e.Category)
                  .WithMany(c => c.AuctionItems)
                  .HasForeignKey(e => e.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Configure relationships
            entity.HasMany(e => e.Bids)
                  .WithOne(b => b.AuctionItem)
                  .HasForeignKey(b => b.AuctionItemId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Images)
                  .WithOne(i => i.AuctionItem)
                  .HasForeignKey(i => i.AuctionItemId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.Watchers)
                  .WithOne(w => w.AuctionItem)
                  .HasForeignKey(w => w.AuctionItemId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureAuctionParticipant(ModelBuilder builder)
    {
        builder.Entity<AuctionParticipant>(entity =>
        {
            entity.Property(e => e.RegistrationDate).HasDefaultValueSql("NOW()");
            entity.Property(e => e.IsApproved).HasDefaultValue(true);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            // Composite unique key
            entity.HasIndex(e => new { e.AuctionId, e.ParticipantId }).IsUnique();

            // Foreign key relationships
            entity.HasOne(e => e.Participant)
                  .WithMany()
                  .HasForeignKey(e => e.ParticipantId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ApprovedBy)
                  .WithMany()
                  .HasForeignKey(e => e.ApprovedById)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureBid(ModelBuilder builder)
    {
        builder.Entity<Bid>(entity =>
        {
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.IsProxyBid).HasDefaultValue(false);
            entity.Property(e => e.IsWinning).HasDefaultValue(false);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            // Foreign key relationships
            entity.HasOne(e => e.Bidder)
                  .WithMany(u => u.Bids)
                  .HasForeignKey(e => e.BidderId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PreviousBid)
                  .WithOne(b => b.NextBid)
                  .HasForeignKey<Bid>(e => e.PreviousBidId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureCategory(ModelBuilder builder)
    {
        builder.Entity<Category>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IconUrl).HasMaxLength(500);
            entity.Property(e => e.Slug).HasMaxLength(100).IsRequired();
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            // Self-referencing relationship for hierarchical categories
            entity.HasOne(e => e.ParentCategory)
                  .WithMany(c => c.ChildCategories)
                  .HasForeignKey(e => e.ParentCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.IsActive);
        });
    }

    private static void ConfigureItemImage(ModelBuilder builder)
    {
        builder.Entity<ItemImage>(entity =>
        {
            entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.ImageUrl).HasMaxLength(500).IsRequired();
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(500);
            entity.Property(e => e.MediumUrl).HasMaxLength(500);
            entity.Property(e => e.AltText).HasMaxLength(255);
            entity.Property(e => e.Caption).HasMaxLength(500);
            entity.Property(e => e.MimeType).HasMaxLength(100);
            entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
            entity.Property(e => e.IsPrimary).HasDefaultValue(false);
        });
    }

    private static void ConfigureNotification(ModelBuilder builder)
    {
        builder.Entity<Notification>(entity =>
        {
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Message).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Priority).HasConversion<string>();
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.EmailSent).HasDefaultValue(false);
            entity.Property(e => e.SmsSent).HasDefaultValue(false);
            entity.Property(e => e.PushSent).HasDefaultValue(false);
            entity.Property(e => e.ActionUrl).HasMaxLength(500);

            // Configure Data property as JSON string
            entity.Property(e => e.Data);

            // Foreign key relationships
            entity.HasOne(e => e.User)
                  .WithMany(u => u.Notifications)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Auction)
                  .WithMany()
                  .HasForeignKey(e => e.AuctionId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.AuctionItem)
                  .WithMany()
                  .HasForeignKey(e => e.AuctionItemId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Bid)
                  .WithMany()
                  .HasForeignKey(e => e.BidId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigurePayment(ModelBuilder builder)
    {
        builder.Entity<Payment>(entity =>
        {
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Method).HasConversion<string>();
            entity.Property(e => e.TransactionId).HasMaxLength(255);
            entity.Property(e => e.StripePaymentIntentId).HasMaxLength(255);
            entity.Property(e => e.Currency).HasMaxLength(3).HasDefaultValue("USD");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.FailureReason).HasMaxLength(1000);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            // Foreign key relationships
            entity.HasOne(e => e.Payer)
                  .WithMany(u => u.Payments)
                  .HasForeignKey(e => e.PayerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AuctionItem)
                  .WithMany()
                  .HasForeignKey(e => e.AuctionItemId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Bid)
                  .WithMany()
                  .HasForeignKey(e => e.BidId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private static void ConfigureWatchlist(ModelBuilder builder)
    {
        builder.Entity<Watchlist>(entity =>
        {
            entity.Property(e => e.WatchedDate).HasDefaultValueSql("NOW()");
            entity.Property(e => e.NotificationsEnabled).HasDefaultValue(true);
            entity.Property(e => e.Notes).HasMaxLength(500);

            // Composite unique key
            entity.HasIndex(e => new { e.UserId, e.AuctionItemId }).IsUnique();

            // Foreign key relationships
            entity.HasOne(e => e.User)
                  .WithMany(u => u.WatchlistedAuctions)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureIndexes(ModelBuilder builder)
    {
        // Auction indexes
        builder.Entity<Auction>()
               .HasIndex(e => e.Status);
        builder.Entity<Auction>()
               .HasIndex(e => e.StartTime);
        builder.Entity<Auction>()
               .HasIndex(e => e.EndTime);
        builder.Entity<Auction>()
               .HasIndex(e => e.IsFeatured);
        builder.Entity<Auction>()
               .HasIndex(e => e.SellerId);

        // AuctionItem indexes
        builder.Entity<AuctionItem>()
               .HasIndex(e => e.AuctionId);
        builder.Entity<AuctionItem>()
               .HasIndex(e => e.CategoryId);
        builder.Entity<AuctionItem>()
               .HasIndex(e => e.CurrentPrice);
        builder.Entity<AuctionItem>()
               .HasIndex(e => e.IsFeatured);
        builder.Entity<AuctionItem>()
               .HasIndex(e => e.LotNumber);

        // Bid indexes
        builder.Entity<Bid>()
               .HasIndex(e => e.AuctionItemId);
        builder.Entity<Bid>()
               .HasIndex(e => e.BidderId);
        builder.Entity<Bid>()
               .HasIndex(e => e.Amount);
        builder.Entity<Bid>()
               .HasIndex(e => e.IsWinning);
        builder.Entity<Bid>()
               .HasIndex(e => e.Status);

        // Notification indexes
        builder.Entity<Notification>()
               .HasIndex(e => e.UserId);
        builder.Entity<Notification>()
               .HasIndex(e => e.IsRead);
        builder.Entity<Notification>()
               .HasIndex(e => e.Type);
        builder.Entity<Notification>()
               .HasIndex(e => e.CreatedAt);

        // Payment indexes
        builder.Entity<Payment>()
               .HasIndex(e => e.PayerId);
        builder.Entity<Payment>()
               .HasIndex(e => e.Status);
        builder.Entity<Payment>()
               .HasIndex(e => e.TransactionId);
        builder.Entity<Payment>()
               .HasIndex(e => e.StripePaymentIntentId);
    }

    private static void ApplyGlobalFilters(ModelBuilder builder)
    {
        // Apply soft delete filter to all entities that inherit from BaseEntity
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var filter = Expression.Lambda(Expression.Equal(property, Expression.Constant(false)), parameter);
                builder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update timestamps before saving
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Deleted:
                    // Implement soft delete
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

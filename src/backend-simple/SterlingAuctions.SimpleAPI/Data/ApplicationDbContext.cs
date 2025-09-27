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
    
    // Push Notifications
    public DbSet<PushNotificationSubscription> PushNotificationSubscriptions { get; set; }
    public DbSet<PushNotification> PushNotifications { get; set; }
    
    // SignalR Performance Monitoring
    public DbSet<SignalRConnectionMetrics> SignalRConnectionMetrics { get; set; }
    public DbSet<SignalRMessageMetrics> SignalRMessageMetrics { get; set; }
    public DbSet<SignalRHubMetrics> SignalRHubMetrics { get; set; }
    public DbSet<SignalRPerformanceAlert> SignalRPerformanceAlerts { get; set; }
    
    // SignalR Load Testing
    public DbSet<SignalRLoadTestConfig> SignalRLoadTestConfigs { get; set; }
    public DbSet<SignalRLoadTestExecution> SignalRLoadTestExecutions { get; set; }
    public DbSet<SignalRLoadTestResult> SignalRLoadTestResults { get; set; }

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
        
        // PushNotificationSubscription configuration
        builder.Entity<PushNotificationSubscription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.Endpoint).IsRequired().HasMaxLength(500);
            entity.Property(e => e.P256dh).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Auth).IsRequired().HasMaxLength(200);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.DeviceInfo).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.LastUsedAt).IsRequired();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.Endpoint }).IsUnique();
            entity.HasIndex(e => e.IsActive);
        });
        
        // PushNotification configuration
        builder.Entity<PushNotification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Body).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Icon).HasMaxLength(500);
            entity.Property(e => e.Badge).HasMaxLength(500);
            entity.Property(e => e.Image).HasMaxLength(500);
            entity.Property(e => e.Tag).HasMaxLength(100);
            entity.Property(e => e.Data).HasMaxLength(1000);
            entity.Property(e => e.Url).HasMaxLength(500);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Type);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
        });
        
        // SignalRConnectionMetrics configuration
        builder.Entity<SignalRConnectionMetrics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ConnectionId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.Transport).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.ClientIp).HasMaxLength(45);
            entity.Property(e => e.ConnectedAt).IsRequired();
            entity.Property(e => e.LastHeartbeat).IsRequired();
            entity.Property(e => e.LastMessageSent).IsRequired();
            entity.Property(e => e.LastMessageReceived).IsRequired();
            entity.Property(e => e.AverageLatency).HasPrecision(18, 2);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ConnectionId).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Transport);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.ConnectedAt);
        });
        
        // SignalRMessageMetrics configuration
        builder.Entity<SignalRMessageMetrics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ConnectionId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.Property(e => e.MessageType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MessageName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Direction).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.Timestamp).IsRequired();

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ConnectionId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.MessageType);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Success);
        });
        
        // SignalRHubMetrics configuration
        builder.Entity<SignalRHubMetrics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.HubName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AverageLatency).HasPrecision(18, 2);
            entity.Property(e => e.Timestamp).IsRequired();

            entity.HasIndex(e => e.HubName);
            entity.HasIndex(e => e.Timestamp);
        });
        
        // SignalRPerformanceAlert configuration
        builder.Entity<SignalRPerformanceAlert>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AlertType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Severity).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.ConnectionId).HasMaxLength(100);
            entity.Property(e => e.UserId).HasMaxLength(450);
            entity.Property(e => e.HubName).HasMaxLength(100);
            entity.Property(e => e.ResolutionNotes).HasMaxLength(1000);
            entity.Property(e => e.TriggeredAt).IsRequired();

            entity.HasIndex(e => e.AlertType);
            entity.HasIndex(e => e.Severity);
            entity.HasIndex(e => e.IsResolved);
            entity.HasIndex(e => e.TriggeredAt);
        });
        
        // SignalRLoadTestConfig configuration
        builder.Entity<SignalRLoadTestConfig>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.HubUrl).IsRequired().HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            
            // Ignore complex properties that can't be mapped to database
            entity.Ignore(e => e.CustomParameters);
            entity.Ignore(e => e.TransportTypes);

            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.TestType);
            entity.HasIndex(e => e.Scenario);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.CreatedAt);
        });
        
        // SignalRLoadTestExecution configuration
        builder.Entity<SignalRLoadTestExecution>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExecutionId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.StartedAt).IsRequired();
            entity.Property(e => e.AverageLatency).HasPrecision(18, 2);
            entity.Property(e => e.MessagesPerSecond).HasPrecision(18, 2);
            entity.Property(e => e.BytesPerSecond).HasPrecision(18, 2);
            entity.Property(e => e.ErrorRate).HasPrecision(18, 2);
            entity.Property(e => e.ConnectionSuccessRate).HasPrecision(18, 2);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            
            // Ignore complex properties that can't be mapped to database
            entity.Ignore(e => e.Metrics);

            entity.HasOne(e => e.Config)
                .WithMany()
                .HasForeignKey(e => e.ConfigId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ExecutionId).IsUnique();
            entity.HasIndex(e => e.ConfigId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.TestType);
            entity.HasIndex(e => e.StartedAt);
        });
        
        // SignalRLoadTestResult configuration
        builder.Entity<SignalRLoadTestResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExecutionId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ConnectionId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UserId).HasMaxLength(450);
            entity.Property(e => e.Transport).HasMaxLength(50);
            entity.Property(e => e.ConnectedAt).IsRequired();
            entity.Property(e => e.AverageLatency).HasPrecision(18, 2);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            
            // Ignore complex properties that can't be mapped to database
            entity.Ignore(e => e.CustomMetrics);

            entity.HasOne(e => e.Execution)
                .WithMany()
                .HasForeignKey(e => e.ExecutionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ExecutionId);
            entity.HasIndex(e => e.ConnectionId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Transport);
            entity.HasIndex(e => e.IsSuccessful);
            entity.HasIndex(e => e.ConnectedAt);
        });
    }
}

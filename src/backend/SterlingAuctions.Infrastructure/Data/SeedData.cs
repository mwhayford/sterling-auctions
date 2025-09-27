using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SterlingAuctions.Core.Entities;

namespace SterlingAuctions.Infrastructure.Data;

/// <summary>
/// Data seeding class for initial database setup
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Initialize database with seed data
    /// </summary>
    public static async Task Initialize(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed roles
        await SeedRoles(roleManager);

        // Seed admin user
        await SeedAdminUser(userManager);

        // Seed categories
        await SeedCategories(context);

        // Seed sample data if in development
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
        {
            await SeedSampleData(context, userManager);
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "SuperAdmin", "Admin", "Moderator", "Seller", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedAdminUser(UserManager<ApplicationUser> userManager)
    {
        const string adminEmail = "admin@sterling-auctions.com";
        
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                IsVerified = true,
                VerifiedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@123!");
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
            }
        }
    }

    private static async Task SeedCategories(ApplicationDbContext context)
    {
        if (await context.Categories.AnyAsync())
            return;

        var categories = new List<Category>
        {
            new Category
            {
                Name = "Art & Antiques",
                Description = "Fine art, paintings, sculptures, and antique collectibles",
                Slug = "art-antiques",
                DisplayOrder = 1,
                IsActive = true
            },
            new Category
            {
                Name = "Jewelry & Watches",
                Description = "Fine jewelry, vintage watches, and precious gems",
                Slug = "jewelry-watches",
                DisplayOrder = 2,
                IsActive = true
            },
            new Category
            {
                Name = "Furniture & Decor",
                Description = "Antique and contemporary furniture, home decor items",
                Slug = "furniture-decor",
                DisplayOrder = 3,
                IsActive = true
            },
            new Category
            {
                Name = "Books & Manuscripts",
                Description = "Rare books, manuscripts, and literary collectibles",
                Slug = "books-manuscripts",
                DisplayOrder = 4,
                IsActive = true
            },
            new Category
            {
                Name = "Coins & Currency",
                Description = "Rare coins, paper money, and numismatic items",
                Slug = "coins-currency",
                DisplayOrder = 5,
                IsActive = true
            },
            new Category
            {
                Name = "Electronics & Technology",
                Description = "Vintage electronics, computers, and technology items",
                Slug = "electronics-technology",
                DisplayOrder = 6,
                IsActive = true
            },
            new Category
            {
                Name = "Sports Memorabilia",
                Description = "Sports cards, autographed items, and sports collectibles",
                Slug = "sports-memorabilia",
                DisplayOrder = 7,
                IsActive = true
            },
            new Category
            {
                Name = "Musical Instruments",
                Description = "Vintage and contemporary musical instruments",
                Slug = "musical-instruments",
                DisplayOrder = 8,
                IsActive = true
            },
            new Category
            {
                Name = "Vehicles & Transportation",
                Description = "Classic cars, motorcycles, and transportation collectibles",
                Slug = "vehicles-transportation",
                DisplayOrder = 9,
                IsActive = true
            },
            new Category
            {
                Name = "Other Collectibles",
                Description = "Miscellaneous collectibles and unique items",
                Slug = "other-collectibles",
                DisplayOrder = 10,
                IsActive = true
            }
        };

        context.Categories.AddRange(categories);
        await context.SaveChangesAsync();

        // Add subcategories for Art & Antiques
        var artCategory = categories.First(c => c.Slug == "art-antiques");
        var artSubcategories = new List<Category>
        {
            new Category
            {
                Name = "Paintings",
                Description = "Oil paintings, watercolors, and mixed media",
                Slug = "paintings",
                ParentCategoryId = artCategory.Id,
                DisplayOrder = 1,
                IsActive = true
            },
            new Category
            {
                Name = "Sculptures",
                Description = "Bronze, marble, and contemporary sculptures",
                Slug = "sculptures",
                ParentCategoryId = artCategory.Id,
                DisplayOrder = 2,
                IsActive = true
            },
            new Category
            {
                Name = "Photography",
                Description = "Vintage and contemporary photography",
                Slug = "photography",
                ParentCategoryId = artCategory.Id,
                DisplayOrder = 3,
                IsActive = true
            }
        };

        context.Categories.AddRange(artSubcategories);
        await context.SaveChangesAsync();
    }

    private static async Task SeedSampleData(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        // Skip if we already have auctions
        if (await context.Auctions.AnyAsync())
            return;

        // Create sample users
        var sampleUsers = await CreateSampleUsers(userManager);
        var artCategory = await context.Categories.FirstAsync(c => c.Slug == "art-antiques");
        var paintingsCategory = await context.Categories.FirstAsync(c => c.Slug == "paintings");

        // Create sample auction
        var sampleAuction = new Auction
        {
            Title = "Fine Art & Antiques Auction",
            Description = "A curated selection of fine art and antique pieces from various estates and collections.",
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(8),
            Status = AuctionStatus.Scheduled,
            Type = AuctionType.English,
            IsFeatured = true,
            SellerId = sampleUsers.First().Id,
            AutoExtend = true,
            ExtensionTimeMinutes = 5,
            ExtensionThresholdMinutes = 5
        };

        context.Auctions.Add(sampleAuction);
        await context.SaveChangesAsync();

        // Create sample auction items
        var sampleItems = new List<AuctionItem>
        {
            new AuctionItem
            {
                Title = "Abstract Expressionist Painting",
                Description = "Original oil on canvas painting by emerging artist. Vibrant colors and dynamic composition.",
                StartingPrice = 500.00m,
                CurrentPrice = 500.00m,
                EstimatedValue = 1200.00m,
                BidIncrement = 50.00m,
                Condition = ItemCondition.Excellent,
                ConditionDescription = "Excellent condition with original frame",
                Dimensions = "24\" x 36\"",
                YearMade = 2020,
                Artist = "Jane Smith",
                LotNumber = 1,
                DisplayOrder = 1,
                AuctionId = sampleAuction.Id,
                CategoryId = paintingsCategory.Id,
                IsFeatured = true
            },
            new AuctionItem
            {
                Title = "Vintage Landscape Painting",
                Description = "Beautiful countryside landscape painting from the early 20th century.",
                StartingPrice = 200.00m,
                CurrentPrice = 200.00m,
                EstimatedValue = 800.00m,
                BidIncrement = 25.00m,
                Condition = ItemCondition.Good,
                ConditionDescription = "Good condition with minor age-related wear",
                Dimensions = "18\" x 24\"",
                YearMade = 1920,
                Artist = "Unknown",
                LotNumber = 2,
                DisplayOrder = 2,
                AuctionId = sampleAuction.Id,
                CategoryId = paintingsCategory.Id
            },
            new AuctionItem
            {
                Title = "Modern Art Print Collection",
                Description = "Set of 5 limited edition prints by contemporary artists.",
                StartingPrice = 100.00m,
                CurrentPrice = 100.00m,
                EstimatedValue = 300.00m,
                BidIncrement = 10.00m,
                Condition = ItemCondition.New,
                ConditionDescription = "Mint condition, never framed",
                Dimensions = "Various sizes",
                YearMade = 2023,
                LotNumber = 3,
                DisplayOrder = 3,
                AuctionId = sampleAuction.Id,
                CategoryId = artCategory.Id
            }
        };

        context.AuctionItems.AddRange(sampleItems);
        await context.SaveChangesAsync();

        // Register users for auction
        foreach (var user in sampleUsers.Skip(1)) // Skip the seller
        {
            var participant = new AuctionParticipant
            {
                AuctionId = sampleAuction.Id,
                ParticipantId = user.Id,
                RegistrationDate = DateTime.UtcNow,
                IsApproved = true,
                ApprovalDate = DateTime.UtcNow
            };
            context.AuctionParticipants.Add(participant);
        }

        await context.SaveChangesAsync();
    }

    private static async Task<List<ApplicationUser>> CreateSampleUsers(UserManager<ApplicationUser> userManager)
    {
        var users = new List<ApplicationUser>();
        var sampleUserData = new[]
        {
            new { Email = "seller@example.com", FirstName = "John", LastName = "Seller", Role = "Seller" },
            new { Email = "bidder1@example.com", FirstName = "Alice", LastName = "Johnson", Role = "User" },
            new { Email = "bidder2@example.com", FirstName = "Bob", LastName = "Williams", Role = "User" },
            new { Email = "bidder3@example.com", FirstName = "Carol", LastName = "Davis", Role = "User" }
        };

        foreach (var userData in sampleUserData)
        {
            var existingUser = await userManager.FindByEmailAsync(userData.Email);
            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = userData.Email,
                    Email = userData.Email,
                    FirstName = userData.FirstName,
                    LastName = userData.LastName,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(user, "User@123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, userData.Role);
                    users.Add(user);
                }
            }
            else
            {
                users.Add(existingUser);
            }
        }

        return users;
    }
}

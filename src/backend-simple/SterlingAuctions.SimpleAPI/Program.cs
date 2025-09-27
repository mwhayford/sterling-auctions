using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Text;
using SterlingAuctions.SimpleAPI.Models;
using SterlingAuctions.SimpleAPI.Data;
using SterlingAuctions.SimpleAPI.Configuration;
using SterlingAuctions.SimpleAPI.Middleware;
using SterlingAuctions.SimpleAPI.Services;
using SterlingAuctions.SimpleAPI.Hubs;
using Amazon.CloudWatch;
using Amazon.CloudWatchLogs;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Database Configuration (In-Memory for simplicity)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("SterlingAuctions"));

// Configuration
builder.Services.Configure<GoogleOAuthSettings>(builder.Configuration.GetSection("GoogleOAuth"));
builder.Services.Configure<RedisSettings>(builder.Configuration.GetSection("RedisSettings"));
builder.Services.Configure<CacheSettings>(builder.Configuration.GetSection("CacheSettings"));
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("StripeSettings"));

// Redis Configuration
var redisConnectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var configuration = ConfigurationOptions.Parse(redisConnectionString);
    configuration.AbortOnConnectFail = false;
    configuration.ConnectTimeout = 5000;
    configuration.SyncTimeout = 5000;
    return ConnectionMultiplexer.Connect(configuration);
});

// Redis Distributed Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
    options.InstanceName = "SterlingAuctions";
});

// Cache Services
builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddScoped<ICacheKeyGenerator, CacheKeyGenerator>();
builder.Services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();

// Auction Services
builder.Services.AddScoped<ICachedAuctionService, CachedAuctionService>();

// Session Services
builder.Services.AddScoped<ISessionService, RedisSessionService>();

// Payment Services
builder.Services.AddScoped<IPaymentService, SimplePaymentService>();

// AWS Services
// builder.Services.AddAWSService<IAmazonCloudWatch>();
// builder.Services.AddAWSService<IAmazonCloudWatchLogs>();

// Monitoring Services
// builder.Services.AddScoped<ICloudWatchMetricsService, CloudWatchMetricsService>();
// builder.Services.AddScoped<ICloudWatchLoggingService, CloudWatchLoggingService>();
// builder.Services.AddScoped<ISeqLoggingService, SeqLoggingService>();
// builder.Services.AddScoped<ICombinedLoggingService, CombinedLoggingService>();
// builder.Services.AddScoped<IApplicationMetricsService, ApplicationMetricsService>();
// builder.Services.AddScoped<IApplicationLoggingService, ApplicationLoggingService>();

// Performance Optimization Services
builder.Services.AddScoped<IPerformanceOptimizationService, PerformanceOptimizationService>();
builder.Services.AddScoped<ILoadTestingService, LoadTestingService>();

// Memory Cache for performance optimization
builder.Services.AddMemoryCache();

// Health Checks
builder.Services.AddHealthChecks()
    .AddCheck<RedisHealthCheck>("redis", tags: new[] { "redis", "cache" });

// Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Configuration
var jwtSecret = builder.Configuration["JwtSettings:SecretKey"] ?? "your-super-secret-key-that-is-at-least-32-characters-long!";
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"] ?? "sterling-auctions",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"] ?? "sterling-auctions-users",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
})
.AddGoogle(options =>
{
    var googleSettings = builder.Configuration.GetSection("GoogleOAuth");
    options.ClientId = googleSettings["ClientId"] ?? "";
    options.ClientSecret = googleSettings["ClientSecret"] ?? "";
    options.CallbackPath = "/api/auth/google-callback";
    
    // Configure scopes
    options.Scope.Add("email");
    options.Scope.Add("profile");
    
    // Save tokens for later use
    options.SaveTokens = true;
});

// Authorization
builder.Services.AddAuthorization(options =>
{
    // Default policy requires authentication
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    // Admin-only policy
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Admin"));

    // Member or Admin policy
    options.AddPolicy("MemberOrAdmin", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Member", "Admin"));

    // Auction-specific policies
    options.AddPolicy("AuctionView", policy =>
        policy.RequireAuthenticatedUser());

    options.AddPolicy("AuctionCreate", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Member", "Admin"));

    options.AddPolicy("AuctionBid", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Member", "Admin"));

    options.AddPolicy("AuctionManage", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Admin"));
});

// Register custom authorization policy provider
builder.Services.AddSingleton<IAuthorizationPolicyProvider, RoleBasedPolicyProvider>();

// Register custom authorization handlers
builder.Services.AddScoped<IAuthorizationHandler, AuctionAuthorizationHandler>();

// Add controllers
builder.Services.AddControllers();

// SignalR Configuration
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
});

// Notification Services
builder.Services.AddScoped<INotificationService, SignalRNotificationService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for SignalR
    });
});

// OpenAPI/Swagger Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Sterling Auctions API",
        Version = "v1",
        Description = "JWT Authentication enabled API for Sterling Auctions"
    });
    
    // JWT Security Definition
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sterling Auctions API v1");
    c.RoutePrefix = "swagger";
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

// Performance optimization middleware
app.UseMiddleware<PerformanceMonitoringMiddleware>();
app.UseMiddleware<CachingOptimizationMiddleware>();
app.UseMiddleware<RequestThrottlingMiddleware>();
app.UseMiddleware<CompressionOptimizationMiddleware>();
app.UseMiddleware<ConnectionPoolOptimizationMiddleware>();

app.UseAuthentication();

// Add custom authorization middleware
app.UseMiddleware<RoleBasedAuthorizationMiddleware>();

app.UseAuthorization();

app.MapControllers();

// SignalR Hub Mapping
app.MapHub<AuctionHub>("/auctionHub");
app.MapHub<NotificationHub>("/notificationHub");

// Basic health check
app.MapGet("/", () => new 
{
    name = "Sterling Auctions API",
    version = "1.0.0",
    features = new[] { "JWT Authentication", "Identity Management", "Swagger Documentation" },
    endpoints = new
    {
        swagger = "/swagger",
        auth = "/api/auth"
    }
}).AllowAnonymous();

app.MapGet("/health", () => Results.Ok(new 
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    version = "1.0.0"
})).AllowAnonymous();

// Initialize database and roles
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    // Ensure database is created
    await context.Database.EnsureCreatedAsync();
    
    // Create roles
    string[] roles = { "Admin", "Member" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
    
    // Create admin user
    var adminEmail = "admin@sterling-auctions.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "System",
            LastName = "Administrator",
            EmailConfirmed = true,
            IsActive = true
        };
        
        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

app.Run();
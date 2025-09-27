using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using SterlingAuctions.API.Models;
using SterlingAuctions.API.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/sterling-auctions-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Database Configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
    "Host=localhost;Database=sterling_auctions;Username=postgres;Password=password";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false; // Set to true in production
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "your-super-secret-key-that-is-at-least-32-characters-long!";
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "sterling-auctions",
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"] ?? "sterling-auctions-users",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    
    // SignalR JWT support
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// Google OAuth Configuration
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["GoogleAuth:ClientId"] ?? "";
        options.ClientSecret = builder.Configuration["GoogleAuth:ClientSecret"] ?? "";
    });

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("MemberOrAdmin", policy => policy.RequireRole("Member", "Admin"));
    options.AddPolicy("RequireEmailConfirmed", policy => 
        policy.RequireClaim("email_verified", "true"));
});

// Add services to the container
builder.Services.AddControllers();

// Repository Pattern (simplified for now)
// builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:3001")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Redis Configuration (will be implemented later)
// builder.Services.AddStackExchangeRedisCache(options =>
// {
//     options.Configuration = builder.Configuration.GetConnectionString("Redis");
// });

// SignalR Configuration (will be implemented later)
// builder.Services.AddSignalR();

// API Documentation with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Sterling Auctions API",
        Version = "v1",
        Description = "API for Sterling Auctions platform with JWT Authentication",
        Contact = new OpenApiContact
        {
            Name = "Sterling Auctions Team",
            Email = "dev@sterling-auctions.com"
        }
    });
    
    // JWT Security Definition
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
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

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>("database")
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API is running"));

var app = builder.Build();

// Configure the HTTP request pipeline
// Enable Swagger in all environments for development/testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sterling Auctions API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "Sterling Auctions API Documentation";
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Request logging
app.UseSerilogRequestLogging();

// HTTPS redirection
app.UseHttpsRedirection();

// CORS
app.UseCors("AllowedOrigins");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Health checks
app.MapHealthChecks("/health").AllowAnonymous();
app.MapHealthChecks("/health/detailed", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            duration = report.TotalDuration,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                duration = entry.Value.Duration
            })
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
}).AllowAnonymous();

// API routes
app.MapControllers();

// Basic info endpoint
app.MapGet("/", () => new
{
    name = "Sterling Auctions API",
    version = "1.0.0",
    environment = app.Environment.EnvironmentName,
    timestamp = DateTime.UtcNow,
    documentation = "/swagger",
    features = new
    {
        authentication = "JWT + Identity",
        database = "PostgreSQL + Entity Framework",
        documentation = "Swagger/OpenAPI 3.0",
        logging = "Serilog",
        health_checks = "/health"
    }
}).AllowAnonymous();

// Database initialization and seeding
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    // Ensure database is created
    await context.Database.EnsureCreatedAsync();
    
    // Seed roles and admin user (will be implemented)
    // await SeedData.Initialize(context, userManager, roleManager);
    
    Log.Information("Database initialized successfully");
}
catch (Exception ex)
{
    Log.Error(ex, "An error occurred while initializing the database");
}

try
{
    Log.Information("Starting Sterling Auctions API with JWT Authentication");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
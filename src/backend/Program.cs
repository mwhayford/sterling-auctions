using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// API Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Sterling Auctions API",
        Version = "v1",
        Description = "API for Sterling Auctions platform",
        Contact = new OpenApiContact
        {
            Name = "Sterling Auctions Team",
            Email = "dev@sterling-auctions.com"
        }
    });
});

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
// Enable Swagger in all environments for development/testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sterling Auctions API v1");
    c.RoutePrefix = "swagger"; // Serve at /swagger
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

// Health checks
app.MapHealthChecks("/health");

// API routes
app.MapControllers();

// Basic info endpoint
app.MapGet("/", () => new
{
    name = "Sterling Auctions API",
    version = "1.0.0",
    environment = app.Environment.EnvironmentName,
    timestamp = DateTime.UtcNow,
    documentation = "/swagger"
});

try
{
    Log.Information("Starting Sterling Auctions API");
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

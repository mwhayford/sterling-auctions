using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using SterlingAuctions.SimpleAPI.Models;

namespace SterlingAuctions.SimpleAPI.Middleware;

/// <summary>
/// Custom authorization handler for auction-specific permissions
/// </summary>
public class AuctionAuthorizationHandler : AuthorizationHandler<AuctionRequirement>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AuctionAuthorizationHandler> _logger;

    public AuctionAuthorizationHandler(
        UserManager<ApplicationUser> userManager,
        ILogger<AuctionAuthorizationHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AuctionRequirement requirement)
    {
        try
        {
            var user = context.User;
            var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims");
                context.Fail();
                return;
            }

            var applicationUser = await _userManager.FindByIdAsync(userId);
            if (applicationUser == null)
            {
                _logger.LogWarning("User {UserId} not found in database", userId);
                context.Fail();
                return;
            }

            // Check if user is active
            if (!applicationUser.IsActive)
            {
                _logger.LogWarning("Inactive user {UserId} attempted to access auction resource", userId);
                context.Fail();
                return;
            }

            // Check user roles
            var userRoles = await _userManager.GetRolesAsync(applicationUser);
            
            // Admin users have all permissions
            if (userRoles.Contains("Admin"))
            {
                _logger.LogDebug("Admin user {UserId} granted access to auction resource", userId);
                context.Succeed(requirement);
                return;
            }

            // Check specific auction permissions based on requirement
            switch (requirement.Permission)
            {
                case AuctionPermission.Create:
                    if (userRoles.Contains("Member") || userRoles.Contains("Admin"))
                    {
                        _logger.LogDebug("User {UserId} granted create permission for auction", userId);
                        context.Succeed(requirement);
                    }
                    else
                    {
                        _logger.LogWarning("User {UserId} denied create permission for auction", userId);
                        context.Fail();
                    }
                    break;

                case AuctionPermission.Bid:
                    if (userRoles.Contains("Member") || userRoles.Contains("Admin"))
                    {
                        _logger.LogDebug("User {UserId} granted bid permission for auction", userId);
                        context.Succeed(requirement);
                    }
                    else
                    {
                        _logger.LogWarning("User {UserId} denied bid permission for auction", userId);
                        context.Fail();
                    }
                    break;

                case AuctionPermission.Manage:
                    if (userRoles.Contains("Admin"))
                    {
                        _logger.LogDebug("User {UserId} granted manage permission for auction", userId);
                        context.Succeed(requirement);
                    }
                    else
                    {
                        _logger.LogWarning("User {UserId} denied manage permission for auction", userId);
                        context.Fail();
                    }
                    break;

                case AuctionPermission.View:
                    // All authenticated users can view auctions
                    _logger.LogDebug("User {UserId} granted view permission for auction", userId);
                    context.Succeed(requirement);
                    break;

                default:
                    _logger.LogWarning("Unknown auction permission: {Permission}", requirement.Permission);
                    context.Fail();
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during auction authorization");
            context.Fail();
        }
    }
}

/// <summary>
/// Requirement for auction-specific permissions
/// </summary>
public class AuctionRequirement : IAuthorizationRequirement
{
    public AuctionPermission Permission { get; }

    public AuctionRequirement(AuctionPermission permission)
    {
        Permission = permission;
    }
}

/// <summary>
/// Auction-specific permissions
/// </summary>
public enum AuctionPermission
{
    View,
    Create,
    Bid,
    Manage
}

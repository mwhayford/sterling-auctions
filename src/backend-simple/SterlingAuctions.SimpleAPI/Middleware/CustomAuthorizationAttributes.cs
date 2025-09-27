using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace SterlingAuctions.SimpleAPI.Middleware;

/// <summary>
/// Custom authorization attribute for role-based authorization
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _requiredRoles;
    private readonly bool _requireAll;

    public RequireRoleAttribute(params string[] roles)
    {
        _requiredRoles = roles ?? throw new ArgumentNullException(nameof(roles));
        _requireAll = false;
    }

    public RequireRoleAttribute(bool requireAll, params string[] roles)
    {
        _requiredRoles = roles ?? throw new ArgumentNullException(nameof(roles));
        _requireAll = requireAll;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Authentication required" });
            return;
        }

        var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        if (_requireAll)
        {
            // User must have ALL required roles
            if (!_requiredRoles.All(role => userRoles.Contains(role)))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
        else
        {
            // User must have AT LEAST ONE required role
            if (!_requiredRoles.Any(role => userRoles.Contains(role)))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}

/// <summary>
/// Custom authorization attribute for auction permissions
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireAuctionPermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly AuctionPermission _permission;

    public RequireAuctionPermissionAttribute(AuctionPermission permission)
    {
        _permission = permission;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Authentication required" });
            return;
        }

        var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        // Check permission based on user roles
        switch (_permission)
        {
            case AuctionPermission.View:
                // All authenticated users can view
                break;

            case AuctionPermission.Create:
            case AuctionPermission.Bid:
                if (!userRoles.Any(role => role == "Member" || role == "Admin"))
                {
                    context.Result = new ForbidResult();
                    return;
                }
                break;

            case AuctionPermission.Manage:
                if (!userRoles.Contains("Admin"))
                {
                    context.Result = new ForbidResult();
                    return;
                }
                break;

            default:
                context.Result = new ForbidResult();
                return;
        }
    }
}

/// <summary>
/// Custom authorization attribute for admin-only access
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AdminOnlyAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Authentication required" });
            return;
        }

        var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        if (!userRoles.Contains("Admin"))
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}

/// <summary>
/// Custom authorization attribute for member or admin access
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class MemberOrAdminAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Authentication required" });
            return;
        }

        var userRoles = user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        if (!userRoles.Any(role => role == "Member" || role == "Admin"))
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}

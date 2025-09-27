using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.Options;

namespace SterlingAuctions.SimpleAPI.Middleware;

/// <summary>
/// Custom authorization policy provider for role-based authorization
/// </summary>
public class RoleBasedPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
    private readonly ILogger<RoleBasedPolicyProvider> _logger;

    public RoleBasedPolicyProvider(IOptions<AuthorizationOptions> options, ILogger<RoleBasedPolicyProvider> logger)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        _logger = logger;
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        _logger.LogDebug("Requesting policy: {PolicyName}", policyName);

        // Handle custom role-based policies
        if (policyName.StartsWith("Role:"))
        {
            var role = policyName.Substring(5); // Remove "Role:" prefix
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireRole(role)
                .Build();

            _logger.LogDebug("Created custom role policy for role: {Role}", role);
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Handle multiple roles policy
        if (policyName.StartsWith("Roles:"))
        {
            var roles = policyName.Substring(6).Split(',', StringSplitOptions.RemoveEmptyEntries);
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireRole(roles)
                .Build();

            _logger.LogDebug("Created custom roles policy for roles: {Roles}", string.Join(", ", roles));
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Handle admin-only policy
        if (policyName == "AdminOnly")
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireRole("Admin")
                .Build();

            _logger.LogDebug("Created AdminOnly policy");
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Handle member-or-admin policy
        if (policyName == "MemberOrAdmin")
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireRole("Member", "Admin")
                .Build();

            _logger.LogDebug("Created MemberOrAdmin policy");
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Fall back to default policy provider
        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallbackPolicyProvider.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackPolicyProvider.GetFallbackPolicyAsync();
    }
}

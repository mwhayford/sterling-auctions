using SterlingAuctions.Core.Entities;

namespace SterlingAuctions.Core.Interfaces;

/// <summary>
/// Service interface for authentication operations
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Register a new user
    /// </summary>
    Task<(bool Success, ApplicationUser? User, IEnumerable<string> Errors)> RegisterAsync(
        string email, string password, string firstName, string lastName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Login user with email and password
    /// </summary>
    Task<(bool Success, string? Token, ApplicationUser? User, IEnumerable<string> Errors)> LoginAsync(
        string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Login user with Google OAuth
    /// </summary>
    Task<(bool Success, string? Token, ApplicationUser? User, IEnumerable<string> Errors)> GoogleLoginAsync(
        string googleToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    Task<(bool Success, string? Token, IEnumerable<string> Errors)> RefreshTokenAsync(
        string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logout user
    /// </summary>
    Task<bool> LogoutAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Request password reset
    /// </summary>
    Task<bool> RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset password
    /// </summary>
    Task<(bool Success, IEnumerable<string> Errors)> ResetPasswordAsync(
        string email, string token, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Change password
    /// </summary>
    Task<(bool Success, IEnumerable<string> Errors)> ChangePasswordAsync(
        string userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirm email address
    /// </summary>
    Task<(bool Success, IEnumerable<string> Errors)> ConfirmEmailAsync(
        string userId, string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resend email confirmation
    /// </summary>
    Task<bool> ResendEmailConfirmationAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate JWT token for user
    /// </summary>
    Task<string> GenerateJwtTokenAsync(ApplicationUser user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate JWT token
    /// </summary>
    Task<(bool IsValid, string? UserId)> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
}

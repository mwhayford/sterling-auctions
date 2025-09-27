using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SterlingAuctions.API.Models;

namespace SterlingAuctions.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Add default role
                await _userManager.AddToRoleAsync(user, "Member");

                // Generate email confirmation token (if email confirmation is required)
                // var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                
                _logger.LogInformation("User {Email} registered successfully", model.Email);

                var token = await GenerateJwtToken(user);
                return Ok(new AuthResponseModel
                {
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Roles = await _userManager.GetRolesAsync(user)
                    }
                });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during user registration");
            return StatusCode(500, "An error occurred during registration");
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized("Invalid email or password");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                if (!user.IsActive)
                {
                    return Unauthorized("Account is deactivated");
                }

                var token = await GenerateJwtToken(user);
                
                _logger.LogInformation("User {Email} logged in successfully", model.Email);

                return Ok(new AuthResponseModel
                {
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Roles = await _userManager.GetRolesAsync(user)
                    }
                });
            }

            if (result.IsLockedOut)
            {
                return Unauthorized("Account is locked out");
            }

            return Unauthorized("Invalid email or password");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during user login");
            return StatusCode(500, "An error occurred during login");
        }
    }

    [HttpPost("refresh-token")]
    [Authorize]
    public async Task<IActionResult> RefreshToken()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null || !user.IsActive)
            {
                return Unauthorized();
            }

            var token = await GenerateJwtToken(user);

            return Ok(new { Token = token });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during token refresh");
            return StatusCode(500, "An error occurred during token refresh");
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await _signInManager.SignOutAsync();
            return Ok(new { Message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during logout");
            return StatusCode(500, "An error occurred during logout");
        }
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Roles = await _userManager.GetRolesAsync(user)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching user profile");
            return StatusCode(500, "An error occurred while fetching profile");
        }
    }

    [HttpPut("profile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileModel model)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.LastModifiedDate = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return Ok(new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    Roles = await _userManager.GetRolesAsync(user)
                });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating user profile");
            return StatusCode(500, "An error occurred while updating profile");
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                return Ok(new { Message = "Password changed successfully" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while changing password");
            return StatusCode(500, "An error occurred while changing password");
        }
    }

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? "your-super-secret-key-that-is-at-least-32-characters-long!";
        var key = Encoding.ASCII.GetBytes(secretKey);

        var userRoles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("first_name", user.FirstName ?? string.Empty),
            new("last_name", user.LastName ?? string.Empty),
            new("user_id", user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Add role claims
        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["TokenExpirationInMinutes"] ?? "60")),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

// DTOs
public class RegisterModel
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? PhoneNumber { get; set; }
}

public class LoginModel
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class UpdateProfileModel
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? PhoneNumber { get; set; }
}

public class ChangePasswordModel
{
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}

public class AuthResponseModel
{
    public required string Token { get; set; }
    public required UserDto User { get; set; }
}

public class UserDto
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public required IList<string> Roles { get; set; }
}

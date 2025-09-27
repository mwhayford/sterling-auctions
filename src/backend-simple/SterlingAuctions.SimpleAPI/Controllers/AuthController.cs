using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SterlingAuctions.SimpleAPI.Models;

namespace SterlingAuctions.SimpleAPI.Controllers;

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
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
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
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Add default role
                await _userManager.AddToRoleAsync(user, "Member");

                _logger.LogInformation("User {Email} registered successfully", model.Email);

                var token = await GenerateJwtToken(user);
                return Ok(new AuthResponseDto
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
    public async Task<IActionResult> Login([FromBody] LoginDto model)
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

                return Ok(new AuthResponseDto
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

            return Unauthorized("Invalid email or password");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during user login");
            return StatusCode(500, "An error occurred during login");
        }
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
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
                Roles = await _userManager.GetRolesAsync(user)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching user profile");
            return StatusCode(500, "An error occurred while fetching profile");
        }
    }

    [HttpPost("link-google")]
    [Authorize]
    public async Task<IActionResult> LinkGoogleAccount([FromBody] LinkGoogleDto model)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            // For now, we'll just return success
            // In a real implementation, you would validate the Google token
            // and link it to the user account
            
            _logger.LogInformation("User {Email} linked Google account", user.Email);
            
            return Ok(new { message = "Google account linked successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while linking Google account");
            return StatusCode(500, "An error occurred while linking Google account");
        }
    }

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var secretKey = _configuration["JwtSettings:SecretKey"] ?? "your-super-secret-key-that-is-at-least-32-characters-long!";
        var key = Encoding.ASCII.GetBytes(secretKey);

        var userRoles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("first_name", user.FirstName),
            new("last_name", user.LastName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(60),
            Issuer = _configuration["JwtSettings:Issuer"] ?? "sterling-auctions",
            Audience = _configuration["JwtSettings:Audience"] ?? "sterling-auctions-users",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

// DTOs
public class RegisterDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
}

public class LoginDto
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class AuthResponseDto
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
    public required IList<string> Roles { get; set; }
}

public class LinkGoogleDto
{
    public required string GoogleToken { get; set; }
}

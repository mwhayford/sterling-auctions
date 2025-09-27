using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
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
public class GoogleAuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public GoogleAuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    [HttpGet("google-login")]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("GoogleCallback")
        };
        
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        
        if (!result.Succeeded)
        {
            return BadRequest(new { message = "Google authentication failed" });
        }

        var claims = result.Principal?.Claims.ToList();
        if (claims == null || !claims.Any())
        {
            return BadRequest(new { message = "No claims received from Google" });
        }

        var email = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
        var name = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
        var givenName = claims.FirstOrDefault(x => x.Type == ClaimTypes.GivenName)?.Value;
        var surname = claims.FirstOrDefault(x => x.Type == ClaimTypes.Surname)?.Value;

        if (string.IsNullOrEmpty(email))
        {
            return BadRequest(new { message = "Email not provided by Google" });
        }

        // Check if user exists
        var user = await _userManager.FindByEmailAsync(email);
        
        if (user == null)
        {
            // Create new user
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = givenName ?? name?.Split(' ').FirstOrDefault() ?? "Google",
                LastName = surname ?? name?.Split(' ').LastOrDefault() ?? "User",
                EmailConfirmed = true,
                IsActive = true
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return BadRequest(new { message = "Failed to create user", errors = createResult.Errors });
            }

            // Add to Member role by default
            await _userManager.AddToRoleAsync(user, "Member");
        }

        // Generate JWT token
        var token = GenerateJwtToken(user);
        
        // Return success with token
        return Ok(new
        {
            message = "Google authentication successful",
            token = token,
            user = new
            {
                id = user.Id,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                roles = await _userManager.GetRolesAsync(user)
            }
        });
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        var jwtSecret = _configuration["JwtSettings:SecretKey"] ?? "your-super-secret-key-that-is-at-least-32-characters-long!";
        var key = Encoding.ASCII.GetBytes(jwtSecret);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email ?? ""),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new("firstName", user.FirstName),
            new("lastName", user.LastName)
        };

        // Add roles
        var roles = _userManager.GetRolesAsync(user).Result;
        foreach (var role in roles)
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

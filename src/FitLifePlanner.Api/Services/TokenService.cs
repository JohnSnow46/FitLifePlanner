using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FitLifePlanner.Domain.Users;
using Microsoft.IdentityModel.Tokens;

namespace FitLifePlanner.Api.Services;

public class TokenService(IConfiguration configuration)
{
    public string GenerateToken(User user)
    {
        var key = configuration["Jwt:Key"]!;
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];

        var claims = new[]
        {
            // Sub is mapped directly to ClaimTypes.NameIdentifier so ClaimsPrincipalExtensions.GetUserId() can read it.
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email)
        };

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

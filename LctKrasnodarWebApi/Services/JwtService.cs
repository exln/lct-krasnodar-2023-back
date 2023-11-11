using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LctKrasnodarWebApi.Data;
using LctKrasnodarWebApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LctKrasnodarWebApi.Services;

public class JwtService
{
    public static string CreateToken(User user, IConfiguration _configuration)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.GivenName, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration.GetSection("AppSettings:Token").Value!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(21),
            signingCredentials: creds
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        
        Console.WriteLine(jwt);

        return jwt;
    }

    public static async Task<UserIdDto> ValidateJwtToken(string token, ApiDbContext _context)
    {
        var handler = new JwtSecurityTokenHandler();
        
        Console.WriteLine(token);
        
        var decodedToken = handler.ReadJwtToken(token);
        
        var email = decodedToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value;
        
        var id = Guid.Parse(decodedToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value!);
        
        var user = await _context.Users
            .FirstOrDefaultAsync(user => user.Email == email && user.Id == id);

        if (user is null) return null;

        return new UserIdDto
        {
            UserId = user.Id
        };
    }

    public static string? GetCookieValue(HttpRequest request)
    {
        if (request.Headers.TryGetValue("Cookies", out var cookieValue1)) return cookieValue1;

        if (request.Headers.TryGetValue("cookie", out var cookieValue2)) return cookieValue2;

        return null;
    }

    public static Dictionary<string, string> ParseCookies(string cookieValue)
    {
        return cookieValue.Split(';')
            .Select(x => x.Trim())
            .ToDictionary(x => x.Split('=')[0], x => x.Split('=')[1]);
    }
}
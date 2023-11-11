using LctKrasnodarWebApi.Data;
using LctKrasnodarWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JwtServices = LctKrasnodarWebApi.Services.JwtService;

namespace LctKrasnodarWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ApiDbContext _context;

    public AuthController(
        IConfiguration configuration,
        ApiDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }
    
    [HttpPost("Authorize")]
    [ProducesResponseType(200, Type = typeof(UserWithTokenRead))]
    [ProducesResponseType(400, Type = typeof(string))]
    public async Task<IActionResult> Authorize(UserLoginDto request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(user => user.Email == request.Email);

        if (user is null) return BadRequest("Пользователь не найден.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) return BadRequest("Неверный пароль.");

        var token = JwtServices.CreateToken(user, _configuration);

        var userWithToken = new UserWithTokenRead
        {
            User = new UserRead
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname,
                Lastname = user.Lastname,
                Role = GetRole(user.Role)
            },
            Token = token
        };

        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            Expires = DateTime.Now.AddDays(21),
            Path = "/"
        });
        return Ok(userWithToken);
    }
    
    [HttpPost("Login")]
    [ProducesResponseType(200, Type = typeof(UserWithTokenRead))]
    [ProducesResponseType(400, Type = typeof(string))]
    public async Task<IActionResult> Login(UserLoginDto request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(user => user.Email == request.Email);

        if (user is null) return BadRequest("Пользователь не найден.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) return BadRequest("Неверный пароль.");

        var token = JwtServices.CreateToken(user, _configuration);

        var userWithToken = new UserWithTokenRead
        {
            User = new UserRead
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname,
                Lastname = user.Lastname,
                Role = GetRole(user.Role)
            },
            Token = token
        };

        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            Expires = DateTime.Now.AddDays(21),
            Path = "/"
        });
        return Ok(userWithToken);
    }
    
    [HttpGet("Logout")]
    [ProducesResponseType(200, Type = typeof(string))]
    [ProducesResponseType(401, Type = typeof(string))]
    public async Task<IActionResult> Logout()
    {
        foreach (var header in Request.Headers) Console.WriteLine($"{header.Key}: {header.Value}");

        DeleteJwtCookie();
        return Ok("Вы вышли из аккаунта.");
    }
    
    [HttpGet("Me")]
    [ProducesResponseType(200, Type = typeof(UserRead))]
    [ProducesResponseType(400, Type = typeof(string))]
    [ProducesResponseType(401, Type = typeof(string))]
    public async Task<IActionResult> Me()
    {
        var token = GetCookieValue(Request);
        
        string? tokenWOjwt = token?[4..];

        if (tokenWOjwt is null) return Unauthorized("Вы не авторизованы.");

        var validatedUser = await JwtServices.ValidateJwtToken(tokenWOjwt, _context);

        var user = await _context.Users
            .FirstOrDefaultAsync(user => user.Id == validatedUser.UserId);

        if (user is null) return BadRequest("Пользователь не найден.");

        var userRead = new UserRead
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Surname = user.Surname,
            Lastname = user.Lastname,
            Role = GetRole(user.Role)
        };

        return Ok(userRead);
    }
    
    private string? GetCookieValue(HttpRequest request)
    {
        if (request.Headers.TryGetValue("Cookies", out var cookieValue1)) return cookieValue1;

        if (request.Headers.TryGetValue("cookie", out var cookieValue2)) return cookieValue2;
        
        // try to get cookie from cookies
        try
        {
            return Request.Cookies["jwt"]?[4..] ?? null;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }
    
    private void DeleteJwtCookie()
    {
        Response.Cookies.Delete("jwt");
    }
    
    private string GetRole(Role role)
    {
        return role switch
        {
            Role.Administrator => "Администратор",
            Role.Manager => "Менеджер",
            Role.Worker => "Сотрудник",
            _ => "Неизвестно"
        };
    }

    /*[HttpPost("Login")]
    [ProducesResponseType(200, Type = typeof(UserWithTokenRead))]
    [ProducesResponseType(400, Type = typeof(string))]
    public async Task<IActionResult> Login(UserLoginDto request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(user => user.Email == request.Email);

        if (user is null) return BadRequest("Пользователь не найден.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) return BadRequest("Неверный пароль.");

        var token = JwtServices.CreateToken(user, _configuration);

        var userWithToken = new UserWithTokenRead
        {
            User = new UserRead
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname,
                Lastname = user.Lastname
            },
            Token = token
        };

        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            Expires = DateTime.Now.AddDays(1),
            Path = "/"
        });
        return Ok(userWithToken);
    }

    [HttpGet("Authorize")]
    [ProducesResponseType(200, Type = typeof(UserRead))]
    [ProducesResponseType(400, Type = typeof(string))]
    [ProducesResponseType(401, Type = typeof(string))]
    public async Task<IActionResult> Get()
    {
        var token = GetCookieValue(Request);

        if (token is null) return Unauthorized("Вы не авторизованы.");

        var validatedUser = await JwtServices.ValidateJwtToken(token, _context);

        var user = await _context.Users
            .FirstOrDefaultAsync(user => user.Id == validatedUser.UserId);

        if (user is null) return BadRequest("Пользователь не найден.");

        var userRead = CreateUserRead(user);

        if (userRead is null) return BadRequest("Пользователь не найден.");

        return Ok(userRead);
    }

    [HttpGet("Logout")]
    [ProducesResponseType(200, Type = typeof(string))]
    [ProducesResponseType(401, Type = typeof(string))]
    public async Task<IActionResult> Logout()
    {
        foreach (var header in Request.Headers) Console.WriteLine($"{header.Key}: {header.Value}");

        var cookieValue = GetCookieValue(Request);
        if (string.IsNullOrEmpty(cookieValue)) return Unauthorized("Вы не авторизованы.");

        var cookies = ParseCookies(cookieValue);
        if (!cookies.TryGetValue("jwt", out var token)) return Unauthorized("Вы не авторизованы.");

        if (Request.Cookies["jwt"] is null) return Unauthorized("Вы не авторизованы.");

        DeleteJwtCookie();
        return Ok("Вы вышли из аккаунта.");
    }

    [HttpGet("сheck", Name = "CheckIfEmailAlreadyExists")]
    [ProducesResponseType(200, Type = typeof(string))]
    [ProducesResponseType(401, Type = typeof(string))]
    public async Task<IActionResult> CheckIfEmailAlreadyExists(string email)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(user => user.Email == email);

        if (user is null) return Ok("Пользователь не найден.");

        return Ok("Пользователь найден.");
    }


    private void DeleteJwtCookie()
    {
        Response.Cookies.Delete("jwt");
    }

    private UserRead? CreateUserRead(User? user)
    {
        if (user is null) return null;

        return new UserRead
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Surname = user.Surname,
            Lastname = user.Lastname
        };
    }

    public static string? GetCookieValue(HttpRequest request)
    {
        if (request.Headers.TryGetValue("Cookies", out var cookieValue1)) return cookieValue1;

        if (request.Headers.TryGetValue("cookie", out var cookieValue2)) return cookieValue2;

        return null;
    }

    private Dictionary<string, string> ParseCookies(string cookieValue)
    {
        return cookieValue.Split(';')
            .Select(x => x.Trim())
            .ToDictionary(x => x.Split('=')[0], x => x.Split('=')[1]);
    }*/
}
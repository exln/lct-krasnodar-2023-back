using System.Reflection;
using LctKrasnodarWebApi.Data;
using LctKrasnodarWebApi.Models;
using LctKrasnodarWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LctKrasnodarWebApi.Controllers;

using JwtServices = JwtService;

[ApiController]
[Route("[controller]")]
public class UserController : Controller
{
    private readonly ApiDbContext _context;

    public UserController(ApiDbContext context)
    {
        _context = context;
    }

    [HttpPost("New")]
    [ProducesResponseType(200, Type = typeof(User))]
    [ProducesResponseType(400, Type = typeof(string))]
    public async Task<IActionResult> CreateNewAdmin([FromBody] UserCreationDto userCreationDto)
    {
        var existingUser = await _context.Users
                .FirstOrDefaultAsync(user => user.Email == userCreationDto.Email);

        if (existingUser is not null) return BadRequest("Пользователь с таким email уже существует.");
        try
        {
            var newUser = new User
            {
                Name = userCreationDto.Name,
                Surname = userCreationDto.Surname,
                Lastname = userCreationDto.Lastname,
                Email = userCreationDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userCreationDto.Password),
                Role = userCreationDto.Role,
                Location = userCreationDto.Location,
                LocationCoordinates = userCreationDto.LocationCoordinates,
                Grade = userCreationDto.Grade
            };
            await _context.Users.AddAsync(newUser);
            
            if (userCreationDto.Role == Role.Worker)
            {
                var workerCase = new WorkerCase
                {
                    Id = newUser.Id,
                    Case = WrkrСase.Work
                };
                await _context.WorkerCases.AddAsync(workerCase);
            }
            
            await _context.SaveChangesAsync();

            return Ok(newUser); 
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }

    }

    [HttpGet("GetShort", Name = "Get all users")]
    [ProducesResponseType(200, Type = typeof(List<UserShortRead>))]
    public async Task<IActionResult> GetAllUsersAdmin()
    {
        try
        {
            return Ok(await _context.Users
                .Where(u => u.Role == Role.Worker)
                .Select(user => new UserShortRead
                {
                    Id = user.Id,
                    Name = user.Name,
                    Surname = user.Surname,
                    Lastname = user.Lastname,
                    Email = user.Email,
                    Location = user.Location,
                    LocationCoordinates = user.LocationCoordinates,
                    Grade = GetGrade(user.Grade!.Value)
                })
                .ToListAsync());
        }
        catch (Exception ex)
        {
              return BadRequest(ex);
        }
    }
    
    [HttpGet("Get", Name = "Get workers with cases")]
    [ProducesResponseType(200, Type = typeof(List<UserShortWCaseRead>))]
    public async Task<IActionResult> GetWorkersWithCases()
    {
        try
        {
            return Ok(await _context.Users
                .Where(u => u.Role == Role.Worker)
                .Join(_context.WorkerCases,
                    user => user.Id,
                    workerCase => workerCase.Id,
                    (user, workerCase) => new UserShortWCaseRead
                    {
                        Id = user.Id,
                        Name = user.Name,
                        Surname = user.Surname,
                        Lastname = user.Lastname,
                        Email = user.Email,
                        Location = user.Location,
                        LocationCoordinates = user.LocationCoordinates,
                        Grade = GetGrade(user.Grade!.Value),
                        Case = new WorkerCaseRead()
                        {
                            _сase = workerCase.Case
                        }.Case
                    })
                .ToListAsync());
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
    
    [HttpPost("Patch", Name = "Patch user")]
    [ProducesResponseType(200, Type = typeof(UserShortRead))]
    public async Task<IActionResult> PatchUser([FromBody] UserPatchDto userPatchDto)
    {
        var userToPatch = await _context.Users
            .FirstOrDefaultAsync(user => user.Id == userPatchDto.UserId);

        if (userToPatch is null) return BadRequest("Пользователь не найден.");

        if (userPatchDto.Name != null) userToPatch.Name = userPatchDto.Name;
        if (userPatchDto.Surname != null) userToPatch.Surname = userPatchDto.Surname;
        if (userPatchDto.Lastname != null) userToPatch.Lastname = userPatchDto.Lastname;
        if (userPatchDto.Email != null) userToPatch.Email = userPatchDto.Email;
        if (userPatchDto.Password != null)
            userToPatch.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userPatchDto.Password);
        if (userPatchDto.Location != null) userToPatch.Location = userPatchDto.Location;
        if (userPatchDto.LocationCoordinates != null)
            userToPatch.LocationCoordinates = userPatchDto.LocationCoordinates;
        if (userPatchDto.Grade != null) userToPatch.Grade = userPatchDto.Grade;

        try
        {
            _context.Users.Update(userToPatch);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
        
        return Ok(new UserShortRead
        {
            Id = userToPatch.Id,
            Name = userToPatch.Name,
            Surname = userToPatch.Surname,
            Lastname = userToPatch.Lastname,
            Email = userToPatch.Email,
            Location = userToPatch.Location,
            LocationCoordinates = userToPatch.LocationCoordinates,
            Grade = GetGrade(userToPatch.Grade!.Value)
        });
    }
    
    [HttpPost("PatchCase", Name = "Patch worker case")]
    [ProducesResponseType(200, Type = typeof(string) )]
    public async Task<IActionResult> PatchWorkerCase([FromBody] WorkerCasePatchDto workerCasePatchDto)
    {
        var workerCaseToPatch = await _context.WorkerCases
            .FirstOrDefaultAsync(workerCase => workerCase.Id == workerCasePatchDto.Id);

        if (workerCaseToPatch is null) return BadRequest("Пользователь не найден.");

        if (workerCasePatchDto.Case != null)
        {
            workerCaseToPatch.Case = workerCasePatchDto.Case switch
            {
                "Отпуск" => WrkrСase.Vacation,
                "Больничный" => WrkrСase.Sick,
                "Доступен" => WrkrСase.Work,
                _ => workerCaseToPatch.Case
            };
        }

        try
        {
            _context.WorkerCases.Update(workerCaseToPatch);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
        
        return Ok("Кейс успешно изменен.");
    }


    [HttpPost("Delete", Name = "Delete worker")]
    [ProducesResponseType(200, Type = typeof(string))]
    public async Task<IActionResult> DeleteUserAdmin([FromBody] UserIdDto userIdDto)
    {
        var userToDelete = await _context.Users
            .FirstOrDefaultAsync(user => user.Id == userIdDto.UserId);

        if (userToDelete is null) return BadRequest("Пользователь не найден.");

        try
        {
            _context.Users.Remove(userToDelete);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }

        return Ok("Пользователи успешно удалены.");
    }
    
    [HttpGet("/Office/Get")]
    [ProducesResponseType(200, Type=typeof(List<Office>))]
    public async Task<IActionResult> GetOffice()
    {
        try
        {
            return Ok(await _context.Offices
                .ToListAsync());
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
        

    /*
    [HttpPost("New", Name = "Create new manager as administrator or new worker as manager or administrator")]
    [ProducesResponseType(200, Type = typeof(List<User>))]
    public async Task<IActionResult> CreateNewUser([FromBody] List<UserCreationDto> userCreationDtos)
    {
        // only admin can create new manager
        // only manager can create new worker

        // check for authorization

        var token = JwtServices.GetCookieValue(Request);

        if (token is null) return Unauthorized("Вы не авторизованы.");

        var validatedUser = await JwtServices.ValidateJwtToken(token, _context);

        var user = await _context.Users
            .FirstOrDefaultAsync(user => user.Id == validatedUser.UserId);

        if (user is null) return Unauthorized("Вы не авторизованы.");

        if (user.Role != Role.Administrator && user.Role != Role.Manager)
            return Unauthorized("Вы не можете создавать новых пользователей.");

        // case: user is manager and wants to create new worker -> ok
        // case: user is manager and wants to create new manager -> not ok
        // case: user is admin and wants to create new manager -> ok
        // case: user is admin and wants to create new worker -> ok

        var createdUsers = new List<User>();

        foreach (var userCreationDto in userCreationDtos)
        {
            if (user.Role == Role.Manager && userCreationDto.Role == Role.Manager)
                return Unauthorized("Вы не можете создавать новых менеджеров.");

            // check for existing user with same email

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(user => user.Email == userCreationDto.Email);

            if (existingUser is not null) return BadRequest("Пользователь с таким email уже существует.");

            // worker creation
            if (user.Role == Role.Manager || user.Role == Role.Administrator)
                try
                {
                    var newUser = new User
                    {
                        Name = userCreationDto.Name,
                        Surname = userCreationDto.Surname,
                        Lastname = userCreationDto.Lastname,
                        Email = userCreationDto.Email,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(userCreationDto.Password),
                        Role = userCreationDto.Role,
                        Location = userCreationDto.Location,
                        LocationCoordinates = userCreationDto.LocationCoordinates,
                        Grade = userCreationDto.Grade
                    };
                    await _context.Users.AddAsync(newUser);
                    await _context.SaveChangesAsync();
                    createdUsers.Add(newUser);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex);
                }

            // manager creation
            if (user.Role == Role.Administrator)
                try
                {
                    var newUser = new User
                    {
                        Name = userCreationDto.Name,
                        Surname = userCreationDto.Surname,
                        Lastname = userCreationDto.Lastname,
                        Email = userCreationDto.Email,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(userCreationDto.Password),
                        Role = userCreationDto.Role
                    };
                    await _context.Users.AddAsync(newUser);
                    await _context.SaveChangesAsync();
                    createdUsers.Add(newUser);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex);
                }
        }

        return Ok(createdUsers);
    }
    */
    /*
    [HttpGet("GetShort", Name = "Get all users as administrator or manager or workers as manager")]
    [ProducesResponseType(200, Type = typeof(List<UserShortRead>))]
    public async Task<IActionResult> GetAllUsers()
    {
        // only admin can get all users
        // only manager can get all workers

        // check for authorization

        var token = JwtServices.GetCookieValue(Request);

        if (token is null) return Unauthorized("Вы не авторизованы.");

        var validatedUser = await JwtServices.ValidateJwtToken(token, _context);

        var user = await _context.Users
            .FirstOrDefaultAsync(user => user.Id == validatedUser.UserId);

        if (user is null) return Unauthorized("Вы не авторизованы.");

        if (user.Role != Role.Administrator && user.Role != Role.Manager)
            return Unauthorized("Вы не можете получать список пользователей.");

        // case: user is manager and wants to get all workers -> ok
        // case: user is manager and wants to get all managers -> not ok
        // case: user is admin and wants to get all managers -> ok
        // case: user is admin and wants to get all workers -> ok

        if (user.Role == Role.Manager)
            return Ok(await _context.Users
                .Where(user => user.Role == Role.Worker)
                .Select(user => new UserShortRead
                {
                    Id = user.Id,
                    Name = user.Name,
                    Surname = user.Surname,
                    Lastname = user.Lastname,
                    Email = user.Email,
                    Location = user.Location,
                    LocationCoordinates = user.LocationCoordinates,
                    Grade = GetGrade(user.Grade!.Value)
                })
                .ToListAsync());

        return Ok(await _context.Users
            .Select(user => new UserShortRead
            {
                Id = user.Id,
                Name = user.Name,
                Surname = user.Surname,
                Lastname = user.Lastname,
                Email = user.Email,
                Location = user.Location,
                Grade = GetGrade(user.Grade!.Value)
            })
            .ToListAsync());
    }
    */
    /*
    // delete worker as manager or administrator or delete manager as administrator
    [HttpDelete("Delete", Name = "Delete worker as manager or administrator or delete manager as administrator")]
    [ProducesResponseType(200, Type = typeof(string))]
    public async Task<IActionResult> DeleteUser([FromBody] List<UserIdDto> userIdDtos)
    {
        // only admin can delete manager
        // only manager can delete worker

        // check for authorization

        var token = JwtServices.GetCookieValue(Request);

        if (token is null) return Unauthorized("Вы не авторизованы.");

        var validatedUser = await JwtServices.ValidateJwtToken(token, _context);

        var user = await _context.Users
            .FirstOrDefaultAsync(user => user.Id == validatedUser.UserId);

        if (user is null) return Unauthorized("Вы не авторизованы.");

        if (user.Role != Role.Administrator && user.Role != Role.Manager)
            return Unauthorized("Вы не можете удалять пользователей.");

        // case: user is manager and wants to delete worker -> ok
        // case: user is manager and wants to delete manager -> not ok
        // case: user is admin and wants to delete manager -> ok
        // case: user is admin and wants to delete worker -> ok

        foreach (var userIdDto in userIdDtos)
        {
            var userToDelete = await _context.Users
                .FirstOrDefaultAsync(user => user.Id == userIdDto.UserId);

            if (userToDelete is null) return BadRequest("Пользователь не найден.");

            if (user.Role == Role.Manager && userToDelete.Role == Role.Manager)
                return Unauthorized("Вы не можете удалять менеджеров.");

            try
            {
                _context.Users.Remove(userToDelete);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        return Ok("Пользователи успешно удалены.");
    }
    */
    public static string? GetGrade(Grade grade) => grade switch
    {
        Grade.Senior => "Сеньор",
        Grade.Middle => "Мидл",
        Grade.Junior => "Джун",
        _ => null
    };
}
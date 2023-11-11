using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LctKrasnodarWebApi.Models;

[PrimaryKey("Id")]
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    [Required] public required string Email { get; set; } = string.Empty;
    [Required] public required string Name { get; set; } = string.Empty;
    [Required] public required string Surname { get; set; } = string.Empty;
    [Required] public required string Lastname { get; set; } = string.Empty;
    [Required] public required Role Role { get; set; }
    [Required] public required string PasswordHash { get; set; } = string.Empty;
    public string? Location { get; set; } = string.Empty;
    public List<double>? LocationCoordinates { get; set; }
    public Grade? Grade { get; set; }
}

public class UserRead
{
    public Guid Id { get; set; }
    [Required] public required string Name { get; set; } = string.Empty;
    [Required] public required string Surname { get; set; } = string.Empty;
    [Required] public required string Lastname { get; set; } = string.Empty;
    [Required] public required string Email { get; set; } = string.Empty;
    [Required] public required string Role { get; set; }
}

public class UserWithTokenRead
{
    [Required] public required UserRead User { get; set; }
    [Required] public required string Token { get; set; } = string.Empty;
}

public class UserShortRead
{
    [Required] public required Guid Id { get; set; }
    [Required] public required string Name { get; set; } = string.Empty;
    [Required] public required string Surname { get; set; } = string.Empty;
    [Required] public required string Lastname { get; set; } = string.Empty;
    [Required] public required string Email { get; set; } = string.Empty;
    public string? Location { get; set; } = string.Empty;
    public List<double>? LocationCoordinates { get; set; }
    public string? Grade { get; set; }
}

public class UserShortWCaseRead: UserShortRead
{
    [Required] public required string Case { get; set; }
}

public enum Role
{
    Administrator,
    Manager,
    Worker
}

public enum Grade
{
    Senior,
    Middle,
    Junior
}
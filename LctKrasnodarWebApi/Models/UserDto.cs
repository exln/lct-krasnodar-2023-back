using System.ComponentModel.DataAnnotations;

namespace LctKrasnodarWebApi.Models;

public class UserLoginDto
{
    [Required] public required string Email { get; set; }
    [Required] public required string Password { get; set; }
}

public class UserRegisterDto
{
    [Required] public required string Name { get; set; }
    [Required] public required string Surname { get; set; }
    [Required] public required string Lastname { get; set; }
    [Required] public required string Password { get; set; }
    [Required] public required string Email { get; set; }
}

public class UserIdDto
{
    [Required] public required Guid UserId { get; set; }
}

public class UserCreationDto
{
    [Required] public required string Name { get; set; }
    [Required] public required string Surname { get; set; }
    [Required] public required string Lastname { get; set; }
    [Required] public required string Password { get; set; }
    [Required] public required string Email { get; set; }
    [Required] public required Role Role { get; set; }
    public string? Location { get; set; }
    public List<double>? LocationCoordinates { get; set; }
    public Grade? Grade { get; set; }
}

public class UserPatchDto : UserIdDto
{
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? Lastname { get; set; }
    public string? Password { get; set; }
    public string? Email { get; set; }
    public Role? Role { get; set; }
    public string? Location { get; set; }
    public List<double>? LocationCoordinates { get; set; }
    public Grade? Grade { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace LctKrasnodarWebApi.Models;

public class MatrixRequest
{
    [Required] public required string[] Addresses { get; set; }
}
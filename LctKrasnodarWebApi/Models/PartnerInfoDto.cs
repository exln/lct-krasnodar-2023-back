using System.ComponentModel.DataAnnotations;

namespace LctKrasnodarWebApi.Models;

public class PartnerInfoCreationDto
{
    [Required] public required string Address { get; set; } = string.Empty;
    [Required] public required List<double> LocationCoordinates { get; set; }
}

public class PartnerInfoReadDto
{
    [Required] public required int Id { get; set; }
    [Required] public required string Address { get; set; } = string.Empty;
    [Required] public required List<double> LocationCoordinates { get; set; }
    [Required] public required string WhenPointConnected { get; set; }
    [Required] public required string AreCardsAndMaterialsDelivered { get; set; }
    [Required] public required int DaysSinceLastCardIssue { get; set; }
    [Required] public required int NumberOfApprovedApplications { get; set; }
    [Required] public required int NumberOfGivenCards { get; set; }
    [Required] public required bool IsActive { get; set; }
}

public class PartnerIdDto
{
    [Required] public required int Id { get; set; }
}

public class PartnerStatsPatchDto: PartnerIdDto
{
    public string? WhenPointConnected { get; set; }
    public string? AreCardsAndMaterialsDelivered { get; set; }
    public int? DaysSinceLastCardIssue { get; set; }
    public int? NumberOfApprovedApplications { get; set; }
    public int? NumberOfGivenCards { get; set; }
}

public class PartnerInfoPatchDto: PartnerIdDto
{
    public string? Address { get; set; }
    public List<double>? LocationCoordinates { get; set; }
}

public class PartnerShortInfoWTask
{
    [Required] public int Id { get; set; }
    [Required] public required List<double> LocationCoordinates { get; set; }
    [Required] public required string Address { get; set; }
    [Required] public required bool IsActive { get; set; }
    [Required] public required List<ConstantTaskSize> Tasks { get; set; }
}

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LctKrasnodarWebApi.Models;

[PrimaryKey("Id")]
public class PartnerInfo
{
    public int Id { get; set; }
    [Required] public required string Address { get; set; } = string.Empty;
    [Required] public required List<double> LocationCoordinates { get; set; }
    public WhenConnected? WhenPointConnected { get; set; } = WhenConnected.Yesterday;
    public YesNo? AreCardsAndMaterialsDelivered { get; set; } = YesNo.No;
    public int? DaysSinceLastCardIssue { get; set; } = 0;
    public int? NumberOfApprovedApplications { get; set; } = 0;
    public int? NumberOfGivenCards { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}

public enum WhenConnected
{
    Long,
    Yesterday
}

public enum YesNo
{
    Yes,
    No
}
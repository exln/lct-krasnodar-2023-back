using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace LctKrasnodarWebApi.Models;

public class Assign
{
    
}

public class WorkerTask
{
    public int Id;
    public string Description;
    public int Tag;
    public int Size;
}

[PrimaryKey("Id")]
public class AssignedTask
{
    public int Id { get; set; }
    public Guid? CourierId { get; set; }
    [Required] public required int PartnerId { get; set; }
    [Required] public required int TaskId { get; set; }
    [Required] public required DateTime Date { get; set; }
    [Required] public required int Size { get; set; }
    [Required] public required List<double> LocationCoordinatesTo { get; set; }
    public List<double>? LocationCoordinatesFrom { get; set; }
    [Required] public required string AddressTo { get; set; }
    public string? AddressFrom { get; set; }
    public string? Polyline { get; set; }
    public int? TravelTime { get; set; }
    [Required] public required bool IsDone { get; set; }
    [Required] public required Priority Priority { get; set; }
    [Required] public required List<Grade> Grades { get; set; }
    [Required] public required DateTime CreatedAt { get; set; }
}

public class AssignedTaskRead
{
    public int Id { get; set; }
    public Guid? CourierId { get; set; }
    [Required] public required int PartnerId { get; set; }
    [Required] public required int TaskId { get; set; }
    [Required] public required DateTime Date { get; set; }
    [Required] public required int Size { get; set; }
    [Required] public required List<double> LocationCoordinatesTo { get; set; }
    public List<double>? LocationCoordinatesFrom { get; set; }
    [Required] public required string AddressTo { get; set; }
    public string? AddressFrom { get; set; }
    public string? Polyline { get; set; }
    public int? TravelTime { get; set; }
    [Required] public required bool IsDone { get; set; }
    [Required] public required Priority Priority { get; set; }
    [Required] public required List<Grade> Grades { get; set; }
    [Required] public required DateTime CreatedAt { get; set; }
}

public class AssignedTaskPatch
{
    [Required] public required int Id { get; set; }
    public int? WorkerId { get; set; }
    public int? TaskId { get; set; }
    public DateTime? Date { get; set; }
    public int? Size { get; set; }
    public List<double>? LocationCoordinatesTo { get; set; }
    public List<double>? LocationCoordinatesFrom { get; set; }
    public string? AddressTo { get; set; }
    public string? AddressFrom { get; set; }
    public string? Polyline { get; set; }
    public int? TravelTime { get; set; }
    public int? Tag { get; set; }
    public bool? IsDone { get; set; }
    public DateTime? CreationDate { get; set; }
}

public class AssignedTaskCreation
{
    [Required] public required int WorkerId { get; set; }
    [Required] public required int TaskId { get; set; }
    [Required] public required DateTime Date { get; set; }
    [Required] public required int Size { get; set; }
    [Required] public required List<double> LocationCoordinatesTo { get; set; }
    [Required] public required List<double> LocationCoordinatesFrom { get; set; }
    [Required] public required string AddressTo { get; set; }
    [Required] public required string AddressFrom { get; set; }
    [Required] public required string Polyline { get; set; }
    [Required] public required int TravelTime { get; set; }
    [Required] public required int Tag { get; set; }
    [Required] public required bool IsDone { get; set; }
    [Required] public required DateTime CreationDate { get; set; }
}

public class AssignedTaskId
{
    [Required] public required int Id { get; set; }
}

[PrimaryKey("Id")]
public class AssignTags
{
    [Required] public required int Id { get; set; }
    [Required] public required string Name { get; set; }
}

public class AssignTagsCreation
{
    [Required] public required string Name { get; set; }
}

public class AssignTagsId
{
    [Required] public required int Id { get; set; }
}

[PrimaryKey("Id")]
public class AssignTagChanges
{
    [Required] public required int Id { get; set; }
    [Required] public required int TaskId { get; set; }
    [Required] public required int TagId { get; set; }
    [Required] public required DateTime Date { get; set; }
}

public class AssignTagChangesCreation
{
    [Required] public required int TaskId { get; set; }
    [Required] public required int TagId { get; set; }
    [Required] public required DateTime Date { get; set; }
}

public class AssignTagChangesId
{
    [Required] public required int Id { get; set; }
}

public class AssignChangesRead
{
    [Required] public required int Id { get; set; }
    [Required] public required AssignedTask Task { get; set; }
    [Required] public required string Tag { get; set; }
    [Required] public required DateTime Date { get; set; }
}

[PrimaryKey("Id")]
 public class AvailableWorkerPosition
 {
     public int Id { get; set; }
     [Required] public required int WorkerId { get; set; }
     [Required] public required List<double> LocationCoordinates { get; set; }
 }



[PrimaryKey("Id")]
public class WorkerPosition
{
    public int Id { get; set; }
    [Required] public required Guid WorkerId { get; set; }
    [Required] public required List<double> LocationCoordinates { get; set; }
    [Required] public DateTime ModifiedAt { get; set; } = DateTimeOffset.Now.UtcDateTime;
}

public class WorkerPositionCreation
{
    [Required] public required Guid WorkerId { get; set; }
    [Required] public required List<double> LocationCoordinates { get; set; }
    [Required] public DateTime ModifiedAt { get; set; }
}

public class DaysSolution
{
    [Required] public required List<DayTasks>  DayTasksList { get; set; }
}

public class DayTasks
{
    [Required] public required List<DayTaskSolution> Tasks { get; set; }
    [Required] public required List<List<double>> PolylineExtremities { get; set; }
}

public class DayTaskSolution
{
    [Required] public required User Worker { get; set; }
    [Required] public required List<AssignedTaskShort> Tasks { get; set; }
    [Required] public required string AproximateDayEndTime { get; set; }
}

public class AssignedTaskShort
{
    [Required] public required string TaskName { get; set; }
    [Required] public required string AddressTo { get; set; }
    [Required] public required int TaskTime { get; set; }
    [Required] public required int TravelTime { get; set; }
    [Required] public required string ApproximateArrivingTime { get; set; }
    [Required] public required List<double> LocationCoordinatesTo { get; set; }
    [Required] public required List<double> LocationCoordinatesFrom { get; set; }
    [Required] public required PolylineDto Polyline { get; set; }
    [Required] public required Priority Priority { get; set; }
    [Required] public required List<Grade> Grades { get; set; }
    
}

public class PolylineDto
{
    [Required] public required List<List<double>> PolylineExtremities { get; set; }  
    [Required] public required string Shape { get; set; }
}

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LctKrasnodarWebApi.Models;

// model to keep report of workers work on tasks such as:
// - how many tasks were completed
// - how many tasks of each type were completed
// = how many kilometers were passed
// - how many time was spent on tasks
// - main office location
// - how many tasks of each grade were completed
// - the most popular task
// - date of report creation
// - days without rest (if any)

[PrimaryKey("Id")]
public class WorkerReport
{
    public int Id { get; set; }
    [Required] public required Guid WorkerId { get; set; }
    [Required] public required int CompletedTasks { get; set; }
    [Required] public required List<int> CompletedTasksOfEachType { get; set; }
    [Required] public required int KilometersPassed { get; set; }
    [Required] public required int TimeSpentOnTasks { get; set; }
    [Required] public required List<double> MainOfficeLocation { get; set; }
    [Required] public required List<int> CompletedTasksOfEachGrade { get; set; }
    [Required] public required string MostPopularTask { get; set; }
    [Required] public required int DaysWithoutRest { get; set; }
    [Required] public required DateTime DateOfReportCreation { get; set; } = DateTimeOffset.Now.UtcDateTime;
    [Required] public required DateTime UpdatedAt { get; set; }
}

public class WorkerReportCreationDto
{
    [Required] public required Guid WorkerId { get; set; }
    [Required] public required int CompletedTasks { get; set; }
    [Required] public required List<int> CompletedTasksOfEachType { get; set; }
    [Required] public required int KilometersPassed { get; set; }
    [Required] public required int TimeSpentOnTasks { get; set; }
    [Required] public required List<double> MainOfficeLocation { get; set; }
    [Required] public required List<int> CompletedTasksOfEachGrade { get; set; }
    [Required] public required string MostPopularTask { get; set; }
    [Required] public required int DaysWithoutRest { get; set; }
    [Required] public required DateTime DateOfReportCreation { get; set; } = DateTimeOffset.Now.UtcDateTime;
}

public class WorkerReportIdDto
{
    public Guid WorkerId { get; set; }
}

public class WorkerReportShow
{
    [Required] public string WorkerName { get; set; }
    [Required] public string WorkerSurname { get; set; }
    [Required] public string WorkerLastname { get; set; }
    [Required] public string WorkerCases { get; set; }
    [Required] public string WorkerEmail { get; set; }
    [Required] public string WorkerRole { get; set; }
    [Required] public int CompletedTasks { get; set; }
    [Required] public List<int> CompletedTasksOfEachType { get; set; }
    [Required] public int KilometersPassed { get; set; }
    [Required] public int TimeSpentOnTasks { get; set; }
    [Required] public string MainOffice { get; set; }
    [Required] public List<double> MainOfficeLocation { get; set; }
    [Required] public List<int> CompletedTasksOfEachGrade { get; set; }
    [Required] public string MostPopularTask { get; set; }
    [Required] public int DaysWithoutRest { get; set; }
    [Required] public DateTime UpdatedAt { get; set; }
}

public class ReportShow
{
    [Required] public int ManagersCount { get; set; }
    [Required] public int WorkersCount { get; set; }
    [Required] public int OfficesCount { get; set; }
    [Required] public int AssignedTasksCount { get; set; }
    [Required] public int ConstantTaskSizesCount { get; set; }
    [Required] public int AvailableWorkers { get; set; }
    
}



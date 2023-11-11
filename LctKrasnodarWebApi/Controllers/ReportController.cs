using System.Text;
using LctKrasnodarWebApi.Data;
using LctKrasnodarWebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace LctKrasnodarWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ReportController: Controller
{
    private readonly ApiDbContext _context;
    
    public ReportController(
        ApiDbContext context)
    {
        _context = context;
    }
    
    [HttpPost("GetReport")]
    [ProducesResponseType(200, Type = typeof(WorkerReportShow))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult GetReport([FromBody] WorkerReportIdDto workerReportIdDto)
    {
        var user = _context.Users
            .FirstOrDefault(user => user.Id == workerReportIdDto.WorkerId);
        if (user == null)
        {
            return BadRequest("Пользователь не найден");
        }
        var report = CreateReport(_context, user);
        return Ok(report);
    }

    private WorkerReportShow CreateReport(ApiDbContext _context, User user)
    {
        Guid workerId = user.Id;
        var constants = _context.ConstantTaskSizes
            .ToList();
        var assignedTasks = _context.AssignedTasks
            .Where(task => task.CourierId == workerId)
            .ToList();
        var courierDto = this._context.CourierDtos
            .FirstOrDefault(courier => courier.WorkerId == workerId);
        var workerCases = _context.WorkerCases.FirstOrDefault(workerCase => workerCase.Id == workerId);
        
        WorkerReportShow report = new WorkerReportShow();
        report.WorkerName = user.Name;
        report.WorkerSurname = user.Surname;
        report.WorkerLastname = user.Lastname;
        report.WorkerCases = GetWorkerCaseString(workerCases.Case);
        report.WorkerEmail = user.Email;
        report.WorkerRole = GetRoleString(user.Role);
        report.CompletedTasks = assignedTasks.Count;
        report.CompletedTasksOfEachType = new List<int>()
            {
                assignedTasks.Count(task => task.TaskId == 1),
                assignedTasks.Count(task => task.TaskId == 2),
                assignedTasks.Count(task => task.TaskId == 3)
            };
        report.KilometersPassed = (int)assignedTasks.Sum(task => task.TravelTime)!;
        report.TimeSpentOnTasks = (int)assignedTasks.Sum(task => task.TravelTime + task.Size)!;
        report.MainOffice = user.Location;
        report.MainOfficeLocation = user.LocationCoordinates;
        report.CompletedTasksOfEachGrade = new List<int>()
            {
                assignedTasks.Count(task => task.Grades.Contains(Grade.Senior)),
                assignedTasks.Count(task => task.Grades.Contains(Grade.Middle)),
                assignedTasks.Count(task => task.Grades.Contains(Grade.Junior))
            };
        report.MostPopularTask = GetMostPopularTask(assignedTasks);
        report.DaysWithoutRest = (int)courierDto.TaskIds.Count/3;
        report.UpdatedAt = DateTimeOffset.Now.UtcDateTime;
        
        return report;
    }
    
    private string GetMostPopularTask(List<AssignedTask> assignedTasks)
    {
        var taskIds = assignedTasks.Select(task => task.TaskId);
        var mostPopularTaskId = taskIds
            .GroupBy(taskId => taskId)
            .OrderByDescending(taskId => taskId.Count())
            .Select(taskId => taskId.Key)
            .FirstOrDefault();
        var mostPopularTask = _context.ConstantTaskSizes
            .FirstOrDefault(task => task.Id == mostPopularTaskId);
        return mostPopularTask.Name;
    }

    private string GetWorkerCaseString(WrkrСase workerCase)
    {
        switch (workerCase)
        {
            case WrkrСase.Work: return "Работает";
            case WrkrСase.Rest: return "Отдыхает";
            case WrkrСase.Vacation: return "В отпуске";
            case WrkrСase.Sick: return "На больничном";
            default: return "Неизвестно";
        }
    }
    
    private string GetRoleString(Role role)
    {
        switch (role)
        {
            case Role.Administrator: return "Администратор";
            case Role.Manager: return "Менеджер";
            case Role.Worker: return "Работник";
            default: return "Неизвестно";
        }
    }
}
using System.Data;
using System.Text;
using Google.OrTools.ConstraintSolver;
using Google.OrTools.Sat;
using Google.Protobuf.WellKnownTypes;
using LctKrasnodarWebApi.Data;
using LctKrasnodarWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using IntVar = Google.OrTools.ConstraintSolver.IntVar;

namespace LctKrasnodarWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AssignController : Controller
{
    private readonly ApiDbContext _context;

    public AssignController(
        ApiDbContext context)
    {
        _context = context;
    }

    [HttpPost("GetRules", Name = "Get Rules for Partner with given id")]
    [ProducesResponseType(200, Type = typeof(List<ConstantTaskRule>))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult GetRules([FromBody] PartnerIdDto partnerIdDto)
    {
        var partnerInfo = _context.PartnerInfos
            .FirstOrDefault(p => p.Id == partnerIdDto.Id);
        if (partnerInfo == null)
        {
            return BadRequest("Партнер с данным id не найден");
        }

        var rules = CheckPartnerStats(partnerInfo, _context);
        return Ok(rules);
    }

    [HttpPost("GetTasks", Name = "Get Tasks for Partner with given id")]
    [ProducesResponseType(200, Type = typeof(List<ConstantTaskSize>))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult GetTasks([FromBody] PartnerIdDto partnerIdDto)
    {
        var partnerInfo = _context.PartnerInfos
            .FirstOrDefault(p => p.Id == partnerIdDto.Id);
        if (partnerInfo == null)
        {
            return BadRequest("Партнер с данным id не найден");
        }

        var rules = CheckPartnerStats(partnerInfo, _context);
        var tasks = GetTasks(rules, _context);
        return Ok(tasks);
    }

    [HttpGet("/Partner/GetTasks", Name = "Get Tasks for Partners")]
    [ProducesResponseType(200, Type = typeof(List<PartnerShortInfoWTask>))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult GetTasks()
    {
        var partners = _context.PartnerInfos
            .ToList();
        var result = new List<PartnerShortInfoWTask>();
        foreach (var partner in partners)
        {
            var rules = CheckPartnerStats(partner, _context);
            var tasks = GetTasks(rules, _context);
            var partnerShortInfoWTask = new PartnerShortInfoWTask
            {
                Id = partner.Id,
                Address = partner.Address,
                LocationCoordinates = partner.LocationCoordinates,
                IsActive = partner.IsActive,
                Tasks = tasks
            };
            result.Add(partnerShortInfoWTask);
        }

        return Ok(result);
    }
    
    [HttpGet("SubmitTasks", Name = "Submit Tasks for Partners")]
    [ProducesResponseType(200, Type = typeof(List<AssignedTask>))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult SubmitTasks()
    {
        var partners = _context.PartnerInfos
            .Where(p => p.IsActive)
            .ToList();
        
        var result = new List<AssignedTask>();
        foreach (var partner in partners)
        {
            var rules = CheckPartnerStats(partner, _context);
            var tasks = GetTasks(rules, _context);
            var assignedTasks = SubmitTasks(tasks, partner, _context);
            result.AddRange(assignedTasks);
        }

        return Ok(result);
    }
    
    [HttpGet("ReleaseTasks", Name = "Release Tasks for Partners")]
    [ProducesResponseType(200, Type = typeof(string))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult ReleaseTasks()
    {
        var assignedTasks = _context.AssignedTasks
            .ToList();
        foreach (var assignedTask in assignedTasks)
        {
            assignedTask.CourierId = null;
            assignedTask.LocationCoordinatesFrom = null;
            assignedTask.TravelTime = null;
            assignedTask.IsDone = false;
            assignedTask.Polyline = null;
        }
        _context.SaveChanges();
        return Ok("Tasks released");
    }
    
    [HttpGet("SubmitWorkers", Name = "Submit Workers for today")]
    [ProducesResponseType(200, Type = typeof(List<CourierDto>))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult SubmitWorkers()
    {
        var AvailableWorkersId = _context.WorkerCases
            .Where(wc => wc.Case == WrkrСase.Work)
            .Select(wc => wc.Id)
            .ToList();
        var Workers = _context.Users
            .Where(u => u.Role == Role.Worker && AvailableWorkersId.Contains(u.Id))
            .ToList();
        var result = new List<CourierDto>();
        foreach (var worker in Workers)
        {
            var courierDto = ConvertWorkerToCourierDto(worker);
            _context.CourierDtos.Add(courierDto);
            _context.SaveChanges();
            result.Add(courierDto);
        }
        return Ok(result);
    }
    
    [HttpGet("ReleaseWorkers", Name = "Release Workers for today")]
    [ProducesResponseType(200, Type = typeof(string))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult ReleaseWorkers()
    {
       // delete all workers from courier dto for today
       var courierDtos = _context.CourierDtos
           .ToList();
       foreach (var courierDto in courierDtos)
       {
           WorkerPosition workerPosition = _context.WorkerPositions
               .Where(wp => wp.WorkerId == courierDto.WorkerId)
               .FirstOrDefault();
           courierDto.Status = "idle";
           courierDto.WorkTime = 0;
           courierDto.LocationCoordinates = workerPosition.LocationCoordinates; 
           courierDto.TaskIds = new List<int>();
           // return courier location to default
           ;
       }
       
       // set workers cases to work
       var workers = _context.WorkerCases
           .Where(wc => wc.Case == WrkrСase.Rest)
           .ToList();
       foreach (var worker in workers)
       {
           worker.Case = WrkrСase.Work;
       }
       _context.SaveChanges();
       return Ok("Workers released");
    }
    
    //[HttpGet("AssignTask", Name = "Assign task to couriers with matching parameters and")]
    
    [HttpGet("Distribution", Name = "Distribute task to couriers with matching parameters")]
    [ProducesResponseType(200, Type = typeof(string))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult Distribution()
    {
        try
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync("http://87.242.88.146:8080/Assign/ReleaseWorkers").Result;
                var response2 = client.GetAsync("http://87.242.88.146:8080/Assign/ReleaseTasks").Result;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        // start assigning tasks to couriers with matching parameters for the next few days (while there are tasks)
        bool theEnd = false;
        while (!theEnd)
        {
            var sortedTasks = SortTasks(_context.AssignedTasks
                .Where(at => !at.IsDone)
                .ToList());
            var AvailableWorkersId = this._context.WorkerCases
                .Where(wc => wc.Case == WrkrСase.Work)
                .Select(wc => wc.Id)
                .ToList();

            var Workers = _context.Users
                .Where(u => u.Role == Role.Worker && AvailableWorkersId.Contains(u.Id))
                .ToList();
            
            for (int i = 0; i < sortedTasks.Count; i++)
            {
                var task = sortedTasks[i];
                for (int j = 0; j < Workers.Count; j++)
                {
                    // check if worker grade is matching task
                    if (!CheckWorkerGrade(task, Workers[j]))
                    {
                        continue;
                    }

                    // check if worker work time is less than 540 minutes (9 hours)
                    if (_context.CourierDtos
                            .FirstOrDefault(c => c.WorkerId == Workers[j].Id)
                            .WorkTime > 540)
                    {
                        continue;
                    }

                    var timeForNewTask = GetDistanceInSec(
                            _context.CourierDtos
                                .FirstOrDefault(c => c.WorkerId == Workers[j].Id)
                                .LocationCoordinates,
                            task.LocationCoordinatesTo,
                            "auto")
                        .Result;

                    var timeSizeofNewTask = task.Size;

                    var newTaskTime = timeForNewTask / 60 + timeSizeofNewTask;

                    // if worker work time with this task is more than 540 minutes (9 hours) we should assign task to next worker
                    if (_context.CourierDtos
                            .FirstOrDefault(c => c.WorkerId == Workers[j].Id)
                            .WorkTime + newTaskTime > 540)
                    {
                        continue;
                    }

                    // if worker grade is matching task and worker work time is less than 540 minutes (9 hours) we can assign task to this worker
                    var assignedTask = _context.AssignedTasks
                        .FirstOrDefault(at => at.TaskId == task.TaskId && at.CourierId == null);
                    if (assignedTask != null) assignedTask.CourierId = Workers[j].Id;
                    else continue;

                    Guid workerId = Workers[j].Id;

                    var Courier = _context.CourierDtos
                        .FirstOrDefault(wp => wp.WorkerId == workerId);

                    if (Courier != null)
                    {
                        if (Courier.TaskIds.Count == 0)
                        {
                            assignedTask.LocationCoordinatesFrom = _context.WorkerPositions
                                .Where(wp => wp.WorkerId == workerId)
                                .FirstOrDefault()
                                .LocationCoordinates;
                        }

                        assignedTask.LocationCoordinatesFrom = Courier.LocationCoordinates;
                        Courier.LocationCoordinates = assignedTask.LocationCoordinatesTo;
                        Courier.TaskIds.Add(assignedTask.Id);
                    }

                    // now as we know coordinates we can get travel time
                    var travelTime = GetDistanceInSec(
                            assignedTask.LocationCoordinatesFrom,
                            assignedTask.LocationCoordinatesTo,
                            "auto")
                        .Result;
                    assignedTask.TravelTime = travelTime / 60;

                    // now as we know travel time we can add work time
                    var workTime = assignedTask.TravelTime + assignedTask.Size;
                    if (workTime != null) Courier.WorkTime += workTime.Value;

                    // now as we know work time we can set status
                    Courier.Status = "busy";
                    

                    sortedTasks.Remove(assignedTask);

                    // set is done to true
                    assignedTask.IsDone = true;

                    _context.SaveChanges();
                }
            }

            // set all not done tasks to the next day
            foreach (var task in sortedTasks)
            {
                task.Date = task.Date.AddDays(1);
            }
            
            // add to everyone's task ids breaking task like -1 to indicate that after that next day is starting
            var courierDtos = _context.CourierDtos
                .ToList();
            foreach (var courierDto in courierDtos)
            {
                courierDto.TaskIds.Add(-1);
                courierDto.LocationCoordinates = _context.WorkerPositions
                    .Where(wp => wp.WorkerId == courierDto.WorkerId)
                    .Select(wp => wp.LocationCoordinates)
                    .FirstOrDefault();
                courierDto.WorkTime = 0; 
            }
            
            Console.WriteLine("Tasks distributed");
            Console.WriteLine(sortedTasks.Count);
            
            if (sortedTasks.Count == 0)
            {
                theEnd = true;
            }
            
            _context.SaveChanges();
        }

        return Ok("Tasks distributed");
    }
    
    
    // Method to get tasks for courier with given id
    [HttpPost("GetCourierTasks", Name = "Get Tasks for Courier with given id")]
    [ProducesResponseType(200, Type = typeof(List<AssignedTask>))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult GetCourierTasks([FromBody] UserIdDto userIdDto)
    {
        var worker = _context.Users
            .FirstOrDefault(u => u.Id == userIdDto.UserId);
        var courier = _context.CourierDtos
            .FirstOrDefault(c => c.WorkerId == userIdDto.UserId);
        var courierTasks = courier.TaskIds;
        var result = new List<AssignedTask>();
        foreach (var courierTask in courierTasks)
        {
            var task = _context.AssignedTasks
                .FirstOrDefault(at => at.Id == courierTask);
            if (task != null) result.Add(task);
        }
        return Ok(result);
    }
    
    // Method to set polyline for all tasks with not null location coordinates from and to
    [HttpGet("SetPolyline", Name = "Set Polyline for taken task")]
    [ProducesResponseType(200, Type = typeof(string))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult GetPolylines()
    {
        var tasks = _context.AssignedTasks
            .Where(at => at.LocationCoordinatesFrom != null && at.LocationCoordinatesTo != null)
            .ToList();
        foreach (var task in tasks)
        {
            if (task.LocationCoordinatesFrom != null)
            {
                try
                {
                    var polyline = GetValhallaPolyline(task.LocationCoordinatesFrom, task.LocationCoordinatesTo)
                        .Result;
                    task.Polyline = polyline;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        _context.SaveChanges();
        return Ok("Polyline set");
    }
    
    // Method to return DayTasks for all couriers with not empty task ids
    [HttpGet("GetTodayTasks", Name = "Get Today Tasks for all couriers")]
    [ProducesResponseType(200, Type = typeof(DaysSolution))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult GetTodayTasks()
    {
        // send set polyline request
        try
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync("http://87.242.88.146:8080/Assign/SetPolyline").Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);;
        }

        DaysSolution daySolution = new DaysSolution()
        {
            DayTasksList = new List<DayTasks>()
        };
        
        var courierDtos = _context.CourierDtos
            .Where(c => c.TaskIds.Count > 0)
            .ToList();
        
        // get the maximum sise of converterted to list list to get the number of days (int lis)
        int maxDays = 0;
        foreach (var courierDto in courierDtos)
        {
            if (ConvertListToListOfLists(courierDto.TaskIds).Count > maxDays)
            {
                maxDays = ConvertListToListOfLists(courierDto.TaskIds).Count;
            }
        }
        
        Console.WriteLine("тут метод с роутами ");
        
        for (int lis = 0; lis < maxDays; lis++)
        {
            var result = new List<DayTaskSolution>();
            var allCoordinates = new List<List<double>>();
            foreach (var courierDto in courierDtos)
            {
                var convertedIds = ConvertListToListOfLists(courierDto.TaskIds);
                var worker = _context.Users
                    .FirstOrDefault(u => u.Id == courierDto.WorkerId);
                if (worker != null)
                {
                    int SunTime = 0;
                    allCoordinates.Add(worker.LocationCoordinates);
                    var tasks = new List<AssignedTaskShort>();
                    // check if courier has tasks for today
                    if (lis >= convertedIds.Count)
                    {
                        continue;
                    }
                    foreach (var taskId in convertedIds[lis])
                    {
                        var TaskSunTime = 0;
                        var task = _context.AssignedTasks
                            .FirstOrDefault(at => at.Id == taskId);
                        if (task != null)
                        {
                            allCoordinates.Add(task.LocationCoordinatesTo);
                            var assignedTaskShort = new AssignedTaskShort
                            {
                                TaskName = _context.ConstantTaskSizes
                                    .FirstOrDefault(cts => cts.Id == task.TaskId)!
                                    .Name,
                                AddressTo = task.AddressTo,
                                TaskTime = task.Size,
                                TravelTime = (int)task.TravelTime!,
                                ApproximateArrivingTime = GetApproximateArrivingTime((int)task.TravelTime, task.Size),
                                LocationCoordinatesTo = task.LocationCoordinatesTo,
                                LocationCoordinatesFrom = task.LocationCoordinatesFrom!,
                                Polyline = GetSummary(task.LocationCoordinatesFrom!, task.LocationCoordinatesTo)
                                    .Result,
                                Priority = task.Priority,
                                Grades = task.Grades
                            };
                            tasks.Add(assignedTaskShort);
                        }

                        TaskSunTime += task!.Size;
                        TaskSunTime += (int)task.TravelTime!;
                        SunTime += TaskSunTime;
                    }

                    var todayTaskSolution = new DayTaskSolution
                    {
                        Worker = worker,
                        Tasks = tasks,
                        AproximateDayEndTime = GetApproximateArrivingTime(SunTime, 0)
                    };
                    result.Add(todayTaskSolution);
                }
            }

            var todayTasks = new DayTasks
            {
                Tasks = result,
                PolylineExtremities = GetPolylineExtremities(allCoordinates)
            };

            daySolution.DayTasksList.Add(todayTasks);
        }

        return Ok(daySolution);
    }
    
    // Method to return DayTasks for only one requested courier
    [HttpPost("GetTodayTasksForCourier", Name = "Get Today Tasks for one courier")]
    [ProducesResponseType(200, Type = typeof(DaysSolution))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult GetTodayTasksForCourier([FromBody] UserIdDto userIdDto)
    {
        // check if user exists and is worker
        var user = _context.Users
            .FirstOrDefault(u => u.Id == userIdDto.UserId);
        if (user == null)
        {
            return BadRequest("Пользователь с данным id не найден");
        }
        if (user.Role != Role.Worker)
        {
            return BadRequest("Пользователь с данным id не является внештатным сотрудником");
        }
        
        // send set polyline request
        try
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync("http://87.242.88.146:8080/Assign/SetPolyline").Result;
                var responseString = response.Content.ReadAsStringAsync().Result;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);;
        }

        DaysSolution daySolution = new DaysSolution()
        {
            DayTasksList = new List<DayTasks>()
        };

        var courierDtos = _context.CourierDtos
            .Where(c => c.TaskIds.Count > 0)
            .Where(cd => cd.WorkerId == userIdDto.UserId)
            .ToList();

        // get the maximum sise of converterted to list list to get the number of days (int lis)
        int maxDays = 0;
        foreach (var courierDto in courierDtos)
        {
            if (ConvertListToListOfLists(courierDto.TaskIds)
                    .Count > maxDays)
            {
                maxDays = ConvertListToListOfLists(courierDto.TaskIds).Count;
            }
        }

        Console.WriteLine("тут метод с роутами для одного курьера");
        for (int lis = 0; lis < maxDays; lis++)
        {
            var result = new List<DayTaskSolution>();
            var allCoordinates = new List<List<double>>();
            foreach (var courierDto in courierDtos)
            {
                if (courierDto.WorkerId != userIdDto.UserId)
                {
                    continue;
                }
                var convertedIds = ConvertListToListOfLists(courierDto.TaskIds);
                var worker = _context.Users
                    .FirstOrDefault(u => u.Id == courierDto.WorkerId);
                if (worker != null)
                {
                    int SunTime = 0;
                    allCoordinates.Add(worker.LocationCoordinates);
                    var tasks = new List<AssignedTaskShort>();
                    // check if courier has tasks for today
                    if (lis >= convertedIds.Count)
                    {
                        continue;
                    }

                    foreach (var taskId in convertedIds[lis])
                    {
                        var TaskSunTime = 0;
                        var task = _context.AssignedTasks
                            .FirstOrDefault(at => at.Id == taskId);
                        if (task != null)
                        {
                            allCoordinates.Add(task.LocationCoordinatesTo);
                            var assignedTaskShort = new AssignedTaskShort
                            {
                                TaskName = _context.ConstantTaskSizes
                                    .FirstOrDefault(cts => cts.Id == task.TaskId)!
                                    .Name,
                                AddressTo = task.AddressTo,
                                TaskTime = task.Size,
                                TravelTime = (int)task.TravelTime!,
                                ApproximateArrivingTime = GetApproximateArrivingTime((int)task.TravelTime, task.Size),
                                LocationCoordinatesTo = task.LocationCoordinatesTo,
                                LocationCoordinatesFrom = task.LocationCoordinatesFrom!,
                                Polyline = GetSummary(task.LocationCoordinatesFrom!, task.LocationCoordinatesTo)
                                    .Result,
                                Priority = task.Priority,
                                Grades = task.Grades
                            };
                            tasks.Add(assignedTaskShort);
                        }

                        TaskSunTime += task!.Size;
                        TaskSunTime += (int)task.TravelTime!;
                        SunTime += TaskSunTime;
                    }

                    var todayTaskSolution = new DayTaskSolution
                    {
                        Worker = worker,
                        Tasks = tasks,
                        AproximateDayEndTime = GetApproximateArrivingTime(SunTime, 0)
                    };
                    result.Add(todayTaskSolution);
                }
            }

            var todayTasks = new DayTasks
            {
                Tasks = result,
                PolylineExtremities = GetPolylineExtremities(allCoordinates)
            };

            daySolution.DayTasksList.Add(todayTasks);
        }

        return Ok(daySolution);
    }

    // Method to return all couriers
    [HttpGet("GetCouriers", Name = "Get all couriers")]
    [ProducesResponseType(200, Type = typeof(List<CourierDto>))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult GetCouriers()
    {
        var courierDtos = _context.CourierDtos
            .ToList();
        return Ok(courierDtos);
    }
    
    // Method to return all couriers with not empty task ids
    [HttpGet("GetBusyCouriers", Name = "Get busy couriers")]
    [ProducesResponseType(200, Type = typeof(List<CourierDto>))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult GetBusyCouriers()
    {
        var courierDtos = _context.CourierDtos
            .Where(c => c.TaskIds.Count > 0)
            .ToList();
        return Ok(courierDtos);
    }
    
    // Method to convert list to list list
    public static List<List<int>> ConvertListToListOfLists(List<int> lst)
    {
        List<List<int>> result = new List<List<int>>();
        List<int> sublist = new List<int>();
        
        foreach (int element in lst)
        {
            if (element == -1)
            {
                if (sublist.Count > 0)
                {
                    result.Add(sublist);
                    sublist = new List<int>();
                }
            }
            else
            {
                sublist.Add(element);
            }
        }
        
        if (sublist.Count > 0)
        {
            result.Add(sublist);
        }
        
        return result;
    }
    
    // Method that returns string like "9:30" from given task and travel time in minutes (9:00 - is start time of the day)
    private string GetApproximateArrivingTime(int travelTime, int taskTime)
    {
        var time = 540 + travelTime + taskTime;
        var hours = time / 60;
        var minutes = time % 60;
        // two digits
        var hoursString = hours.ToString();
        var minutesString = minutes.ToString();
        if (hoursString.Length == 1)
        {
            hoursString = "0" + hoursString;
        }
        if (minutesString.Length == 1)
        {
            minutesString = "0" + minutesString;
        }
        return hoursString + ":" + minutesString;
    }
    
    // Method to get min lat min lon max lat max lon from given coordinates
    private List<List<double>> GetPolylineExtremities(List<List<double>> coordinates)
    {
        double minLat = coordinates[0][0];
        double minLon = coordinates[0][1];
        double maxLat = coordinates[0][0];
        double maxLon = coordinates[0][1];

        foreach (var coordinate in coordinates)
        {
            if (coordinate[0] < minLat)
                minLat = coordinate[0];
            if (coordinate[0] > maxLat)
                maxLat = coordinate[0];
            if (coordinate[1] < minLon)
                minLon = coordinate[1];
            if (coordinate[1] > maxLon)
                maxLon = coordinate[1];
        }

        return new List<List<double>>
        {
            new List<double>
            {
                minLat,
                minLon
            },
            new List<double>
            {
                maxLat,
                maxLon
            }
        };
    }
    
    // Method to get polyline from points ids via valhalla api
    private async Task<string> GetValhallaPolyline(List<double> source, List<double> target)
    {
        using var client = new HttpClient();
        var content = new StringContent(JsonConvert.SerializeObject(new RoutingRequest
        {
            locations = new List<VallhallaCoordinate>
            {
                new()
                {
                    lat = source[0],
                    lon = source[1]
                },
                new()
                {
                    lat = target[0],
                    lon = target[1]
                }
            },
            costing = "auto",
            units = "kilometers",
            language = "en"
        }), Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync("http://45.9.25.174/valhalla/route", content);
            var responseString = await response.Content.ReadAsStringAsync();
            dynamic jsonObject = JsonConvert.DeserializeObject(responseString);

            string shape = jsonObject.trip.legs[0].shape;

            return shape;

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return "";
    }
    
    // Method to get min lat min lon max lat max lon from given coordinates
    private async Task<PolylineDto> GetSummary(List<double> source, List<double> target)
    {
        using var client = new HttpClient();
        var content = new StringContent(JsonConvert.SerializeObject(new RoutingRequest
        {
            locations = new List<VallhallaCoordinate>
            {
                new()
                {
                    lat = source[0],
                    lon = source[1]
                },
                new()
                {
                    lat = target[0],
                    lon = target[1]
                }
            },
            costing = "auto",
            units = "kilometers",
            language = "en"
        }), Encoding.UTF8, "application/json");

        var response = await client.PostAsync("http://45.9.25.174/valhalla/route", content);

        var responseString = await response.Content.ReadAsStringAsync();
        dynamic jsonObject = JsonConvert.DeserializeObject(responseString);

        string shape = jsonObject.trip.legs[0].shape;
        double minLat = jsonObject.trip.legs[0].summary.min_lat;
        double minLon = jsonObject.trip.legs[0].summary.min_lon;
        double maxLat = jsonObject.trip.legs[0].summary.max_lat;
        double maxLon = jsonObject.trip.legs[0].summary.max_lon;

        return new PolylineDto
        {
            Shape = shape,
            PolylineExtremities = new List<List<double>>
            {
                new List<double>
                {
                    minLat,
                    minLon
                },
                new List<double>
                {
                    maxLat,
                    maxLon
                }
            }
        };
    }
    
    // Method to check if worker grade is matching task
    private bool CheckWorkerGrade(AssignedTask task, User worker)
    {
        if (task.Grades.Contains(worker.Grade.Value))
        {   
            return true;
        }
        return false;
    }
    
    // Method to get nearest worker with matching grade
    private Guid GetNearestWorkerWGrade(AssignedTask task, List<User> workers, ApiDbContext _context)
    {
        var courierDtos = _context.CourierDtos
            .ToList();
        
        Dictionary<Guid, int> workerLocationsWDistance = new Dictionary<Guid, int>();
        
        var workerLocationsWithGrade = new List<CourierDto>();
        foreach (var workerLocation in courierDtos)
        {
            var worker = _context.Users
                .FirstOrDefault(u => u.Id == workerLocation.WorkerId);
            if (worker != null)
            {
                if (task.Grades.Contains(worker.Grade.Value))
                {
                    workerLocationsWithGrade.Add(workerLocation);
                }
            }
        }
        int minDistance = 0;
        var workerLocationsWithGradeAndDistance = new List<CourierDto>();
        foreach (var workerLocation in workerLocationsWithGrade)
        {
            var distance = GetDistanceInSec(
                workerLocation.LocationCoordinates,
                task.LocationCoordinatesTo,
                "auto")
                .Result;
            if (minDistance == 0)
            {
                minDistance = distance;
            }
            else if (distance < minDistance)
            {
                minDistance = distance;
            }
            workerLocationsWDistance.Add(workerLocation.WorkerId, distance);
        }

        // make courier busy and set task id
        var minDistanceId = workerLocationsWDistance.FirstOrDefault(w => w.Value == minDistance).Key;

        var courier = _context.CourierDtos
            .AsEnumerable() // Switch to client-side evaluation
            .FirstOrDefault(c => c.WorkerId == minDistanceId);
        
        if (courier != null) courier.Status = "busy";
        _context.SaveChanges();
        
        return workerLocationsWDistance.FirstOrDefault(w => w.Value == minDistance).Key;
    }
    
    // Sorter method
    private List<AssignedTask> SortTasks(List<AssignedTask> tasks)
    {
        var result = new List<AssignedTask>();
        var highPriorityTasks = tasks
            .Where(t => t.Priority == Priority.High)
            .ToList();
        highPriorityTasks = SortTasksBySize(highPriorityTasks);
        var mediumPriorityTasks = tasks
            .Where(t => t.Priority == Priority.Medium)
            .ToList();
        mediumPriorityTasks = SortTasksBySize(mediumPriorityTasks);
        var lowPriorityTasks = tasks
            .Where(t => t.Priority == Priority.Low)
            .ToList();
        lowPriorityTasks = SortTasksBySize(lowPriorityTasks);
        result.AddRange(highPriorityTasks);
        result.AddRange(mediumPriorityTasks);
        result.AddRange(lowPriorityTasks);
        return result;
    }
    
    // Method to sort tasks by size
    private List<AssignedTask> SortTasksBySize(List<AssignedTask> tasks)
    {
        var result = new List<AssignedTask>();
        var bigSizeTasks = tasks
            .Where(t => t.Size == 240)
            .ToList();
        var mediumSizeTasks = tasks
            .Where(t => t.Size == 120)
            .ToList();
        var smallSizeTasks = tasks
            .Where(t => t.Size == 90)
            .ToList();
        result.AddRange(smallSizeTasks);
        result.AddRange(mediumSizeTasks);
        result.AddRange(bigSizeTasks);
        return result;
    }
    
    // Method to sort tasks by priority
    private List<AssignedTask> SortTasksByPriority(List<AssignedTask> tasks)
    {
        var result = new List<AssignedTask>();
        var highPriorityTasks = tasks
            .Where(t => t.Priority == Priority.High)
            .ToList();
        var mediumPriorityTasks = tasks
            .Where(t => t.Priority == Priority.Medium)
            .ToList();
        var lowPriorityTasks = tasks
            .Where(t => t.Priority == Priority.Low)
            .ToList();
        result.AddRange(highPriorityTasks);
        result.AddRange(mediumPriorityTasks);
        result.AddRange(lowPriorityTasks);
        return result;
    }
    
    // Method to submit tasks for given partner
    private List<AssignedTask> SubmitTasks(List<ConstantTaskSize> tasks, PartnerInfo partner, ApiDbContext _context)
    {
        var result = new List<AssignedTask>();
        foreach (var task in tasks)
        {
            var assignedTask = new AssignedTask
            {
                TaskId = task.Id,
                Date = DateTimeOffset.Now.UtcDateTime,
                Size = int.Parse(task.Value),
                LocationCoordinatesTo = partner.LocationCoordinates,
                AddressTo = partner.Address,
                PartnerId = partner.Id,
                CreatedAt = DateTimeOffset.Now.UtcDateTime,
                IsDone = false,
                Priority = task.Priority,
                Grades = task.Grades,
            };
            _context.AssignedTasks.Add(assignedTask);
            _context.SaveChanges();
            result.Add(assignedTask);
        }

        return result;
    }
    
    // Method to assign task to nearest worker with matching grade: return worker
    
    // Method to convert worker to courier dto
    private CourierDto ConvertWorkerToCourierDto(User worker)
    {
        var courierDto = new CourierDto
        {
            WorkerId = worker.Id,
            Status = "idle",
            Grade = worker.Grade.Value,
            Date = DateTimeOffset.Now.UtcDateTime,
            WorkTime = 0,
            TaskIds = new List<int>() {},
            LocationCoordinates = worker.LocationCoordinates,
            ModifiedAt = DateTimeOffset.Now.UtcDateTime
        };
        return courierDto;
    }
    
    // Method To set location coordinates for given task dto
    private void SetLocationCoordinatesForTaskDto(TaskDto taskDto, PartnerInfo partner, ApiDbContext _context)
    {
        
    }
    
    // Method to convert given tasks list to task dto and saving it to database with given partner id and todays date
    private List<TaskDto> ConvertTasksToDto(List<ConstantTaskSize> tasks)
    {
        var result = new List<TaskDto>();
        foreach (var task in tasks)
        {
            var taskDto = new TaskDto
            {
                TaskId = task.Id,
                Duration = int.Parse(task.Value),
                CreatedAt = DateTimeOffset.Now.UtcDateTime,
                isDone = false,
                Priority = task.Priority,
                Grades = task.Grades,
            };
            result.Add(taskDto);
        }

        return result;
    }

    /*// Method to create cost matrix for given partners and workers
    // By first iteration workers should get tasks with highest priority and minimum distance to them
    // By second iteration workers should get tasks with second highest priority and minimum distance to them and so on
    [HttpGet("GetMatrix", Name = "Get Matrix for Partners and Workers")]
    [ProducesResponseType(200, Type = typeof(int[,]))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult GetMatrix()
    {
        var costMatrix = CreateCostMatrix(_context);
        PrintCostMatrix(costMatrix);
        return Ok(costMatrix);
    }*/
    
    // [HttpGet("GetFirstMatrix", Name = "Get Matrix for Partners and Workers first")]
    // [ProducesResponseType(200, Type = typeof(int[,]))]
    // [ProducesResponseType(400, Type = typeof(string))]
    // public IActionResult GetFirstMatrix()
    // {
    //     var costMatrix = CreateFirstCostMatrix(_context);
    //     PrintCostMatrix(costMatrix);
    //     return Ok(costMatrix);
    // }

    /*// First assign should give workers tasks with highest cost
    [HttpGet("GetFirstAssign", Name = "Get First Assign for Partners and Workers")]
    [ProducesResponseType(200, Type = typeof(List<AssignedTask>))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult GetFirstAssign()
    {
        int[,] matrix = CreateFirstCostMatrix(_context);;
        int rowCount = matrix.GetLength(0);
        int colCount = matrix.GetLength(1);

        int[,] costs = new int[colCount, rowCount];

        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < colCount; j++)
            {
                costs[j, i] = matrix[i, j];
            }
        }
        
        int numWorkers = costs.GetLength(0);
        int numTasks = costs.GetLength(1);

        // Model.
        CpModel model = new CpModel();

        // Variables.
        BoolVar[,] x = new BoolVar[numWorkers, numTasks];
        // Variables in a 1-dim array.
        for (int worker = 0; worker < numWorkers; ++worker)
        {
            for (int task = 0; task < numTasks; ++task)
            {
                x[worker, task] = model.NewBoolVar($"worker_{worker}_task_{task}");
            }
        }

        // Constraints
        // Each worker is assigned to at most one task.
        for (int worker = 0; worker < numWorkers; ++worker)
        {
            List<ILiteral> tasks = new List<ILiteral>();
            for (int task = 0; task < numTasks; ++task)
            {
                tasks.Add(x[worker, task]);
            }
            model.AddAtMostOne(tasks);
        }
        
        // Each task is assigned to exactly one worker.
        for (int task = 0; task < numTasks; ++task)
        {
            List<ILiteral> workers = new List<ILiteral>();
            for (int worker = 0; worker < numWorkers; ++worker)
            {
                workers.Add(x[worker, task]);
            }
            model.AddExactlyOne(workers);
        }


        // Objective
        LinearExprBuilder obj = LinearExpr.NewBuilder();
        Console.WriteLine($"{numWorkers} {numTasks}");  
        Console.WriteLine($"{x.GetLength(0)} {x.GetLength(1)}");  
        Console.WriteLine($"{costs.GetLength(0)} {costs.GetLength(1)}");
        for (int worker = 0; worker < numWorkers; ++worker)
        {
            for (int task = 0; task < numTasks; ++task)
            {
                Console.WriteLine($"{task} {worker}");
                obj.AddTerm(x[worker, task], costs[worker, task]);
            }
        }

        model.Minimize(obj);

        // Solve
        CpSolver solver = new CpSolver();
        CpSolverStatus status = solver.Solve(model);
        Console.WriteLine($"Solve status: {status}");

        // Print solution.
        // Check that the problem has a feasible solution.
        if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
        {
            Console.WriteLine($"Total cost: {solver.ObjectiveValue}\n");
            for (int i = 0; i < numWorkers; ++i)
            {
                for (int j = 0; j < numTasks; ++j)
                {
                    if (solver.Value(x[i, j]) > 0.5)
                    {
                        Console.WriteLine($"Worker {i} assigned to task {j}. Cost: {costs[i, j]}");
                    }
                }
            }
        }
        else
        {
            Console.WriteLine("No solution found.");
        }

        Console.WriteLine("Statistics");
        Console.WriteLine($"  - conflicts : {solver.NumConflicts()}");
        Console.WriteLine($"  - branches  : {solver.NumBranches()}");
        Console.WriteLine($"  - wall time : {solver.WallTime()}s");

        return Ok();
    }*/
    
    // Set default locations for available couriers 
    /*private void SetDefaultLocCouriers(ApiDbContext _context)
    {
        var availableWorkers = _context.CourierDtos
            .ToList();
        foreach (var availableWorker in availableWorkers)
        {
            var worker = _context.Users
                .FirstOrDefault(u => u.Id == availableWorker.WorkerId);
            if (worker != null)
            {
                var defaultWorkerLocation = _context.CourierDtos
                    .FirstOrDefault(awp => awp.WorkerId == worker.Id);
                if (defaultWorkerLocation == null)
                {
                    var newWorkerPosition = new CourierDtoPatch()
                    {
                        Id = availableWorker.Id,
                        WorkerId = worker.Id,
                        LocationCoordinates = worker.LocationCoordinates,
                    };
                    _context.CourierDtos.Update(newWorkerPosition));
                    _context.SaveChanges();
                }
            }
        }
    }*/
    
    // Set default location for available workers to WorkerPosition table
    private void SetDefaultLocationForAvailableWorkers(ApiDbContext _context)
    {
        var availableWorkers = _context.WorkerCases
            .Where(wc => wc.Case == WrkrСase.Work)
            .ToList();
        foreach (var availableWorker in availableWorkers)
        {
            var worker = _context.Users
                .FirstOrDefault(u => u.Id == availableWorker.Id);
            if (worker != null)
            {
                var defaultWorkerLocation = _context.WorkerPositions
                    .FirstOrDefault(awp => awp.WorkerId == worker.Id);
                if (defaultWorkerLocation == null)
                {
                    var newWorkerPosition = new WorkerPosition
                    {
                        WorkerId = worker.Id,
                        LocationCoordinates = worker.LocationCoordinates,
                    };
                    _context.WorkerPositions.Add(newWorkerPosition);
                    _context.SaveChanges();
                }
            }
        }
    }
    
    /*// Method for creating first cost matrix when all workers on their default locations
    private int[,] CreateFirstCostMatrix(ApiDbContext _context)
    {
        var Constants = _context.ConstantTaskSizes
            .ToList();
        var PartnerInfos = _context.PartnerInfos
            .Where(p => p.IsActive)
            .ToList();
        var AvailableWorkersId = this._context.WorkerCases
            .Where(wc => wc.Case == WrkrСase.Work)
            .Select(wc => wc.Id)
            .ToList();
        var Workers = _context.Users
            .Where(u => u.Role == Role.Worker && AvailableWorkersId.Contains(u.Id))
            .ToList();
        
        int[,] costMatrix = new int[PartnerInfos.Count, Workers.Count];
        
        for (var i = 0; i < PartnerInfos.Count; i++)
        {
            for (var j = 0; j < Workers.Count; j++)
            {
                SetDefaultLocationForAvailableWorkers(_context);
                var defaultWorkerLocation = _context.WorkerPositions
                    .FirstOrDefault(awp => awp.WorkerId == Workers[j].Id);
                if (defaultWorkerLocation != null)
                {
                    var workerCoordinates = defaultWorkerLocation.LocationCoordinates;
                    var rules = CheckPartnerStats(PartnerInfos[i], _context);
                    var tasks = GetTasks(rules, _context).FirstOrDefault();
                    if (tasks is null)
                    {
                        costMatrix[i, j] = 1;
                        continue;
                    }
                    var taskSize = GetCostForWorker(Workers[j], tasks);
                    Console.WriteLine("taskSize:" + taskSize);
                    if (taskSize != 0)
                    {
                        var partnerCoordinates = PartnerInfos[i].LocationCoordinates;
                        // total cost = task size + travel time (all in minutes)
                        var time = GetDistanceInSec(
                                workerCoordinates,
                                partnerCoordinates,
                                "auto")
                            .Result;
                        Console.WriteLine("time" + time);
                        var delta = ConvertDistanceToCost(time);
                        Console.WriteLine("delta: " + delta);
                        var totalCost = GetTotalCost(delta, taskSize);
                        Console.WriteLine(totalCost);
                        costMatrix[i, j] = totalCost;
                    }
                    else
                    {
                        costMatrix[i, j] = 1;
                    }
                    
                }
                else
                {
                    costMatrix[i, j] = 1;
                }
            }
        }
        
        return costMatrix;
    }*/
    
    /*// Method for creating cost matrix
    private int[,] CreateCostMatrix(ApiDbContext _context)
    {
        var Constants = _context.ConstantTaskSizes
            .ToList();
        var PartnerInfos = _context.PartnerInfos
            .Where(p => p.IsActive)
            .ToList();
        var AvailableWorkersId = this._context.WorkerCases
            .Where(wc => wc.Case == WrkrСase.Work)
            .Select(wc => wc.Id)
            .ToList();
        var Workers = _context.Users
            .Where(u => u.Role == Role.Worker && AvailableWorkersId.Contains(u.Id))
            .ToList();
        
        var costMatrix = new int[Constants.Count + PartnerInfos.Count, Workers.Count];
        
        for (var i = 0; i < Constants.Count ; i++)
        {
            for (var j = 0; j < Workers.Count; j++)
            {
                var workerGrade = Workers[j].Grade;
                var taskSize = Constants[i].Value;
                var taskGrade = Constants[i].Grades;
                
                if (taskGrade.Contains((Grade)workerGrade!))
                {
                    costMatrix[i, j] = Convert.ToInt32(taskSize);
                }
                else
                {
                    costMatrix[i, j] = 0;
                }
            }
        }
        
        for (var i = Constants.Count; i < Constants.Count + PartnerInfos.Count; i++)
        {
            for (var j = 0; j < Workers.Count; j++)
            {
                SetDefaultLocationForAvailableWorkers(_context);
                var defaultWorkerLocation = _context.WorkerPositions
                    .FirstOrDefault(awp => awp.WorkerId == Workers[j].Id);
                if (defaultWorkerLocation != null)
                {
                    var workerCoordinates = defaultWorkerLocation.LocationCoordinates;
                    var partnerCoordinates = PartnerInfos[i - Constants.Count].LocationCoordinates;
                    var time = GetDistanceInSec(
                        workerCoordinates,
                        partnerCoordinates,
                        "auto")
                        .Result;
                    Console.WriteLine(time);
                    costMatrix[i, j] = time;
                }
                else
                {
                    costMatrix[i, j] = 0;
                }
            }
        }
        
        return costMatrix;
    }*/
    
    /*// Method for printing cost martix to console
    private void PrintCostMatrix(int[,] costMatrix)
    {
        for (var i = 0; i < costMatrix.GetLength(0); i++)
        {
            for (var j = 0; j < costMatrix.GetLength(1); j++)
            {
                Console.Write(costMatrix[i, j] + " ");
            }
            Console.WriteLine();
        }
    }
    */
    
    // Method to add travel time (in seconds) to task size (in minutes) to get total cost 
    private int GetTotalCost(double travelTime, int taskSize)
    {
        var totalCost = travelTime * taskSize;
        return (int)totalCost;
    }
    
    // Method to convert distance (in seconds) to cost. You simply take the inverse of the distance.
    private double ConvertDistanceToCost(int distance)
    {
        if (distance < 60)
        {
            return 1;
        }

        var distanceInMinutes = (double)distance / 60;
        var cost = 1 / distanceInMinutes;
        return cost;
    }
    
    // Method to get distance between two points by their coordinates via Valhalla for given costing like auto or bicycle
    private async Task<int> GetDistanceInSec(List<double> coordinatesFrom, List<double> coordinatesTo, string costing)
    {
        using var client = new HttpClient();
        var content = new StringContent(JsonConvert.SerializeObject(new DistanceRequest
        {
            sources = new List<VallhallaCoordinate>
            {
                new()
                {
                    lat = coordinatesFrom[0],
                    lon = coordinatesFrom[1]
                }
            },
            targets = new List<VallhallaCoordinate>
            {
                new()
                {
                    lat = coordinatesTo[0],
                    lon = coordinatesTo[1]
                }
            },
            costing = costing
        }), Encoding.UTF8, "application/json");

        var response = await client.PostAsync("http://45.9.25.174/valhalla/sources_to_targets", content);

        var responseString = await response.Content.ReadAsStringAsync();

        if (responseString == null)
            // обработка случая, когда responseString равно null
            return 0;
   
        
        var responseJson = JsonConvert.DeserializeObject<RouteResponse>(responseString);

        if (responseJson == null || responseJson.sources_to_targets.Count == 0)
            // обработка случая, когда responseJson равно null или sources_to_targets пустой
            return 0;

        return responseJson.sources_to_targets[0][0].time;
    }

    // Method that return cost for exact worker and task due to their grade match
    private int GetCostForWorker(User worker, ConstantTaskSize constantTaskSize)
    {
        int cost = GetCost(constantTaskSize);
        if (worker.Grade != null)
        {
            Grade workerGrade = worker.Grade.Value;
            foreach (Grade grade in constantTaskSize.Grades)
            {
                if (grade == workerGrade)
                {
                    return cost;
                }
            }
        }

        return 0;
    }
    
    // Method that returns cost for given task by its priority and value
    private int GetCost(ConstantTaskSize constantTaskSize)
    {
        int cost = int.Parse(constantTaskSize.Value);
       
        switch (constantTaskSize.Priority)
        {
            case Priority.Low:
                cost *= 1;
                break;
            case Priority.Medium:
                cost *= 2;
                break;
            case Priority.High:
                cost *= 3;
                break;
        }
        
        return cost;
    }
    
    // Method that returns task/tasks by given list of suitable rules
    private List<ConstantTaskSize> GetTasks(List<ConstantTaskRule> rules, ApiDbContext _context)
    {
        List<int> ruleIds = new List<int>();
        
        foreach (ConstantTaskRule rule in rules)
        {
            ruleIds.Add(rule.Id);
        }
        List<ConstantTaskSize> tasks = _context.ConstantTaskSizes
            .ToList();

        List<ConstantTaskSize> result = new List<ConstantTaskSize> {};
        
        foreach (ConstantTaskSize task in tasks)
        {
            if (task.RuleQuantor == RuleQuantor.All)
            {
                if (task.Rules.All(ruleIds.Contains))
                {
                    result.Add(task);
                }
            }
            else if (task.RuleQuantor == RuleQuantor.Any)
            {
                if (task.Rules.Any(ruleIds.Contains))
                {
                    result.Add(task);
                }
            }
        }
        
        return result;
    }
    
    // Method that checks given partner stats through the existing rules
    private List<ConstantTaskRule> CheckPartnerStats(PartnerInfo partnerInfo, ApiDbContext _context)
    {
        List<ConstantTaskRule> constantTaskRules = _context.ConstantTaskRules.ToList();

        List<ConstantTaskRule> result = new List<ConstantTaskRule>();

        foreach (ConstantTaskRule constantTaskRule in constantTaskRules)
        {
            var conditions = constantTaskRule.Conditions;
            var targets = constantTaskRule.Targets;
            var values = constantTaskRule.Values;

            bool allConditionsMet = true;
            
            /* public enum Condition
            {
                Equal,
                NotEqual,
                Greater,
                Less,
                GreaterOrEqual,
                LessOrEqual,
                Between,
                NotBetween,
                In,
                NotIn,
                NotNullOrZero,
                NullOrZero,
                MoreThanAHalf
            }*/

            for (int i = 0; i < conditions.Count; i++)
            {
                switch (targets[i])
                {
                    case Target.WhenPointConnected:
                        if (conditions[i] == Condition.Equal)
                        {
                            if (partnerInfo.WhenPointConnected != WhenConnected.Yesterday && values[i]=="Вчера")
                            {
                                allConditionsMet = false;
                            }
                        }
                        else if (conditions[i] == Condition.NotEqual)
                        {
                            if (partnerInfo.WhenPointConnected == WhenConnected.Yesterday && values[i]!="Вчера")
                            {
                                allConditionsMet = false;
                            }
                        }

                        break;
                    case Target.AreCardsAndMaterialsDelivered:
                        if (conditions[i] == Condition.Equal)
                        {
                            if (partnerInfo.AreCardsAndMaterialsDelivered != YesNo.No && values[i]=="Нет")
                            {
                                allConditionsMet = false;
                            }
                        }
                        else if (conditions[i] == Condition.NotEqual)
                        {
                            if (partnerInfo.AreCardsAndMaterialsDelivered == YesNo.No && values[i]!="Нет")
                            {
                                allConditionsMet = false;
                            }
                        }

                        break;
                    case Target.DaysSinceLastCardIssue:
                        if (conditions[i] == Condition.Equal)
                        {
                            if (partnerInfo.DaysSinceLastCardIssue != int.Parse(values[i]))
                            {
                                allConditionsMet = false;
                            }
                        }
                        else if (conditions[i] == Condition.NotEqual)
                        {
                            if (partnerInfo.DaysSinceLastCardIssue == int.Parse(values[i]))
                            {
                                allConditionsMet = false;
                            }
                        }
                        else if (conditions[i] == Condition.Greater)
                        {
                            if (partnerInfo.DaysSinceLastCardIssue <= int.Parse(values[i]))
                            {
                                allConditionsMet = false;
                            }
                        }
                        else if (conditions[i] == Condition.Less)
                        {
                            if (partnerInfo.DaysSinceLastCardIssue >= int.Parse(values[i]))
                            {
                                allConditionsMet = false;
                            }
                        }
                        else if (conditions[i] == Condition.GreaterOrEqual)
                        {
                            if (partnerInfo.DaysSinceLastCardIssue < int.Parse(values[i]))
                            {
                                allConditionsMet = false;
                            }
                        }
                        else if (conditions[i] == Condition.LessOrEqual)
                        {
                            if (partnerInfo.DaysSinceLastCardIssue > int.Parse(values[i]))
                            {
                                allConditionsMet = false;
                            }
                        }
                        else if (conditions[i] == Condition.Between)
                        {
                            if (partnerInfo.DaysSinceLastCardIssue <= int.Parse(values[i]) ||
                                partnerInfo.DaysSinceLastCardIssue >= int.Parse(values[i]))
                            {
                                allConditionsMet = false;
                            }
                        }
                        else if (conditions[i] == Condition.NotBetween)
                        {
                            if (partnerInfo.DaysSinceLastCardIssue >= int.Parse(values[i]) &&
                                partnerInfo.DaysSinceLastCardIssue <= int.Parse(values[i]))
                            {
                                allConditionsMet = false;
                            }
                        }
                        else if (conditions[i] == Condition.In)
                        {
                            if (partnerInfo.DaysSinceLastCardIssue != int.Parse(values[i]))
                            {
                                allConditionsMet = false;
                            }
                        }
                        else if (conditions[i] == Condition.NotIn)
                        {
                            if (partnerInfo.DaysSinceLastCardIssue == int.Parse(values[i]))
                            {
                                allConditionsMet = false;
                            }
                        }
                        else if (conditions[i] == Condition.NotNullOrZero)
                        {
                            if (partnerInfo.DaysSinceLastCardIssue == 0)
                            {
                                allConditionsMet = false;
                            }
                        }
                        else if (conditions[i] == Condition.NullOrZero)
                        {
                            if (partnerInfo.DaysSinceLastCardIssue != 0)
                            {
                                allConditionsMet = false;
                            }
                        }

                        break;
                    case Target.NumberOfApprovedApplications:
                        if (conditions[i] == Condition.Equal)
                        {
                            if (partnerInfo.NumberOfApprovedApplications != int.Parse(values[i]))
                            {
                                allConditionsMet = false;
                            }
                        }
                        else if (conditions[i] == Condition.NotEqual)
                        {
                            if (partnerInfo.NumberOfApprovedApplications == int.Parse(values[i]))
                            {
                                allConditionsMet = false;
                            }
                        }
                        else if (conditions[i] == Condition.Greater)
                        {
                            if (partnerInfo.NumberOfApprovedApplications <= int.Parse(values[i]))
                            {
                                allConditionsMet = false;
                            }
                        }
                        else if (conditions[i] == Condition.Less)
                        {
                            if (partnerInfo.NumberOfApprovedApplications >= int.Parse(values[i]))
                            {
                                allConditionsMet = false;
                            }
                        }
                        else if (conditions[i] == Condition.GreaterOrEqual)
                        {
                            if (partnerInfo.NumberOfApprovedApplications < int.Parse(values[i]))
                            {
                                allConditionsMet = false;
                            }
                        }
                        else if (conditions[i] == Condition.LessOrEqual)
                        {
                            if (partnerInfo.NumberOfApprovedApplications > int.Parse(values[i]))
                            {
                                allConditionsMet = false;
                            }
                        }
                        else if (conditions[i] == Condition.Between)
                        {
                            if (partnerInfo.NumberOfApprovedApplications <= int.Parse(values[i]) ||
                                partnerInfo.NumberOfApprovedApplications >= int.Parse(values[i]))
                            {
                                allConditionsMet = false;
                            }
                        }
                        else if (conditions[i] == Condition.MoreThanAHalf)
                        {
                            
                            if (partnerInfo.NumberOfGivenCards == 0 || (partnerInfo.NumberOfGivenCards >= partnerInfo.NumberOfApprovedApplications * 0.5))
                            {
                                allConditionsMet = false;
                                
                            }
                        }

                        break;
                    case Target.NumberOfGivenCards:
                        if (conditions[i] == Condition.Equal)
                        {
                            if (partnerInfo.NumberOfGivenCards != int.Parse(values[i]))
                            {
                                allConditionsMet = false;
                            }
                        }
                        else if (conditions[i] == Condition.NotEqual)
                        {
                            if (partnerInfo.NumberOfGivenCards == int.Parse(values[i]))
                            {
                                allConditionsMet = false;
                            }
                        }
                        else if (conditions[i] == Condition.Greater)
                        {
                            if (partnerInfo.NumberOfGivenCards <= int.Parse(values[i]))
                            {
                                allConditionsMet = false;
                            }
                        }
                        else if (conditions[i] == Condition.Less)
                        {
                            if (partnerInfo.NumberOfGivenCards >= int.Parse(values[i]))
                            {
                                allConditionsMet = false;
                            }
                        }
                        else if (conditions[i] == Condition.GreaterOrEqual)
                        {
                            if (partnerInfo.NumberOfGivenCards < int.Parse(values[i]))
                            {
                                allConditionsMet = false;
                            }
                        }
                        else if (conditions[i] == Condition.LessOrEqual)
                        {
                            if (partnerInfo.NumberOfGivenCards > int.Parse(values[i]))
                            {
                                allConditionsMet = false;
                            }
                        }
                        else if (conditions[i] == Condition.Between)
                        {
                            if (partnerInfo.NumberOfGivenCards <= int.Parse(values[i]) ||
                                partnerInfo.NumberOfGivenCards >= int.Parse(values[i]))
                            {
                                allConditionsMet = false;
                            }
                        }

                        break;
                }

                if (!allConditionsMet)
                {
                    break;
                }
            }

            if (allConditionsMet)
            {
                result.Add(constantTaskRule);
            }
        }

        return result;
    }
}
            
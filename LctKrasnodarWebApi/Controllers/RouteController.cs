using System.Text;
using Google.OrTools.ConstraintSolver;
using LctKrasnodarWebApi.Data;
using LctKrasnodarWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;


namespace LctKrasnodarWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class RouteController : Controller
{
    private readonly ApiDbContext _context;

    public RouteController(ApiDbContext context)
    {
        _context = context;
    }

    [HttpGet("Today", Name = "Get today's routes")]
    [ProducesResponseType(200, Type = typeof(Solutions))]
    public IActionResult GetTodayRoutes()
    {
        Solutions solutions = new() { SolutionList = new List<SolutionRead>() };

        // Get all available workers
        var workers = GetAvailableWorkers().Result;

        // Get all used coordinates
        var createdDataModel = CreateDataModel(_context);
        var coordinates = createdDataModel.AllCoordinates;
        
        
        // Get all endpoints for workers
        var routes = GetOptimalRoute(createdDataModel);

        try
        {
            for (var i = 0; i < workers.Count; i++)
                foreach (var route in routes)
                    if (route.VehicleId - 1 == i)
                    {
                        var solution = new SolutionRead
                        {
                            User = new UserShortRead
                            {
                                Id = workers[i].Id,
                                Name = workers[i].Name,
                                Surname = workers[i].Surname,
                                Lastname = workers[i].Lastname,
                                Email = workers[i].Email,
                                Location = workers[i].Location,
                                LocationCoordinates = workers[i].LocationCoordinates,
                                Grade = GetGrade(workers[i].Grade!.Value)
                            },
                            EndPointsList = new Dictionary<int, Endpnt>()
                        };
                        Console.WriteLine(route.Route.Count);
                        for (int a = 0; a < route.Route.Count - 1; a++)
                        {
                            var point = route.Route[a];
                            
                            Console.WriteLine(a);
                            Console.WriteLine(route.Route.Count);
                            Console.WriteLine(point);
                            
                            var endPoint = new Endpnt
                            {
                                Coordinates = coordinates[point],
                                RouteToEndpoint = GetValhallaRoute(coordinates[point],
                                    coordinates[point + 1]).Result
                            };
                            solution.EndPointsList.Add(point, endPoint);
                        }

                        solutions.SolutionList.Add(solution);
                    }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return Ok(solutions);
    }

    // Method to create Polyline from route
    public static LineString CreatePolyline(List<Coordinate> coordinates)
    {
        var coordinateArray = coordinates.ToArray();
        var geometryFactory = new GeometryFactory();
        var lineString = geometryFactory.CreateLineString(coordinateArray);
        return lineString;
    }

    // Method to get route from points ids via valhalla api
    private async Task<string> GetValhallaRoute(List<double> source, List<double> target)
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

        return shape;
    }

    // Method to create data model for vehicle routing problem
    private static DataModel CreateDataModel(ApiDbContext _context)
    {
        var data = new DataModel();


        // Configure start points: if worker location coordinates are same as office location coordinates
        // then start point is this office with its id-1
        var offices = _context.Offices.ToList();
        // Get all available workers (that are with WrkrСase Work)
        var availableList = _context.WorkerCases
            .Where(x => x.Case == WrkrСase.Work)
            .Select(x => x.Id)
            .ToList();
        
        var workers = _context.Users
            .Where(x => x.Role == Role.Worker)
            .Where(x => availableList.Contains(x.Id))
            .ToList();
        
        var starts = new List<int>();

        foreach (var worker in workers)
        foreach (var office in offices)
            if (worker.LocationCoordinates[0] == office.LocationCoordinates[0] &&
                worker.LocationCoordinates[1] == office.LocationCoordinates[1])
            {
                Console.WriteLine("ХУЙХУЙХУЙ");
                starts.Add(office.Id - 1);
            }
        
        var workNum = starts.Count;
        data.Ends = new int[workNum];

        data.Starts = starts.ToArray();

        // Configure vehicle number
        data.VehicleNumber = workNum;


        // Create distance matrix
        var partnerInfos = _context.PartnerInfos.ToList();
        var distanceMatrix = new double[partnerInfos.Count + offices.Count, partnerInfos.Count + offices.Count];

        // save all coordinates to data model
        var coordinates = new List<List<double>>();
        foreach (var office in offices) coordinates.Add(office.LocationCoordinates);
        foreach (var partnerInfo in partnerInfos) coordinates.Add(partnerInfo.LocationCoordinates);


        // on office x partner distance matrix is distance between office and partner
        for (var i = offices.Count; i < offices.Count + partnerInfos.Count; i++)
        for (var j = 0; j < offices.Count + partnerInfos.Count; j++)
        {
            var distance = CalculateDistance(coordinates[i], coordinates[j]);
            distanceMatrix[i, j] = distance.Result;
        }

        // on partner x office distance matrix is distance between partner and office
        for (var i = 0; i < offices.Count + partnerInfos.Count; i++)
        for (var j = offices.Count; j < offices.Count + partnerInfos.Count; j++)
        {
            var distance = CalculateDistance(coordinates[i], coordinates[j]);
            distanceMatrix[i, j] = distance.Result;
        }

        // on office x office distance matrix is 0
        for (var i = 0; i < offices.Count; i++)
        for (var j = 0; j < offices.Count; j++)
            if (i == j || i == 0 || j == 0)
                distanceMatrix[i, j] = 0;


        data.AllCoordinates = coordinates;

        data.DistanceMatrix = distanceMatrix;

        // console log distance matrix and coordinates
        Console.WriteLine("Distance matrix:");
        for (var i = 0; i < data.DistanceMatrix.GetLength(0); ++i)
        {
            for (var j = 0; j < data.DistanceMatrix.GetLength(1); ++j)
                Console.Write("{0, 6}", data.DistanceMatrix[i, j]);

            Console.WriteLine();
        }

        return data;
    }

    // Method to calculate distance between two points
    private static async Task<int> CalculateDistance(List<double> source, List<double> target)
    {
        using var client = new HttpClient();
        var content = new StringContent(JsonConvert.SerializeObject(new DistanceRequest
        {
            sources = new List<VallhallaCoordinate>
            {
                new()
                {
                    lat = source[0],
                    lon = source[1]
                }
            },
            targets = new List<VallhallaCoordinate>
            {
                new()
                {
                    lat = target[0],
                    lon = target[1]
                }
            },
            costing = "auto"
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

        return (int)(responseJson.sources_to_targets[0][0].distance * 10);
    }


    // Method to get optimal route
    private static List<RouteModel> GetOptimalRoute(DataModel data)
    {
        // Instantiate the data problem.
        Console.WriteLine(data.DistanceMatrix.GetLength(0));
        Console.WriteLine(data.VehicleNumber);
        Console.WriteLine(data.Starts.Length);
        Console.WriteLine(data.Ends.Length);
        // Create Routing Index Manager
        var manager =
            new RoutingIndexManager(data.DistanceMatrix.GetLength(0), data.VehicleNumber, data.Starts, data.Ends);

        // Create Routing Model.
        var routing = new RoutingModel(manager);

        // Create and register a transit callback.
        var transitCallbackIndex = routing.RegisterTransitCallback((fromIndex, toIndex) =>
        {
            // Convert from routing variable Index to
            // distance matrix NodeIndex.
            var fromNode = manager.IndexToNode(fromIndex);
            var toNode = manager.IndexToNode(toIndex);
            return (long)data.DistanceMatrix[fromNode, toNode];
        });

        // Define cost of each arc.
        routing.SetArcCostEvaluatorOfAllVehicles(transitCallbackIndex);

        // Add Distance constraint.
        routing.AddDimension(transitCallbackIndex, 0, 2000,
            true, // start cumul to zero
            "Distance");
        var distanceDimension = routing.GetMutableDimension("Distance");
        distanceDimension.SetGlobalSpanCostCoefficient(100);

        // Setting first solution heuristic.
        var searchParameters =
            operations_research_constraint_solver.DefaultRoutingSearchParameters();
        searchParameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;

        // Solve the problem.
        var solution = routing.SolveWithParameters(searchParameters);

        // Print solution on console.
        var routes = PrintSolution(data, routing, manager, solution);

        // Convert routes to JSON
        var json = JsonConvert.SerializeObject(routes, Formatting.Indented);

        return routes;
    }

    // Method to resolve simple vrp
    private static List<RouteModel> PrintSolution(in DataModel data, in RoutingModel routing,
        in RoutingIndexManager manager,
        in Assignment solution)
    {
        // Inspect solution.
        long maxRouteDistance = 0;
        var routes = new List<RouteModel>();

        for (var i = 0; i < data.VehicleNumber; ++i)
        {
            var route = new RouteModel
            {
                VehicleId = i,
                Route = new List<int>(),
                RouteDistance = 0
            };

            var index = routing.Start(i);
            while (routing.IsEnd(index) == false)
            {
                route.Route.Add(manager.IndexToNode((int)index));
                var previousIndex = index;
                index = solution.Value(routing.NextVar(index));
                route.RouteDistance += routing.GetArcCostForVehicle(previousIndex, index, 0);
            }

            routes.Add(route);
            maxRouteDistance = Math.Max(route.RouteDistance, maxRouteDistance);
        }

        Console.WriteLine("Maximum distance of the routes: {0}m", maxRouteDistance);

        return routes;
    }

    // Method to assign restrictions to workers by their grades
    private void AssignRestrictionsToWorkersByGrades()
    {
        var workers = GetAvailableWorkers().Result;

        var officesWithStaffersAndGrades = new OfficesWithStaffersAndGrades
            { OfficeWithStaffersAndGradesList = new List<OfficeWithStaffersAndTags>() };

        // add worker grade to office with staffers
        foreach (var worker in workers)
        foreach (var officeWithStaffersAndGrades in officesWithStaffersAndGrades.OfficeWithStaffersAndGradesList)
        {
            if (worker.Grade is null) continue;
            // if worker grade is junior it can be assign only to junior partner
            // if worker grade is middle it can be assign to junior and middle partner
            // if worker grade is senior it can be assign to junior, middle and senior partner
            if (worker.Grade == Grade.Senior)
                officeWithStaffersAndGrades.LocationTags.Add(LocationTag.Senior);
            else if (worker.Grade == Grade.Middle)
                officeWithStaffersAndGrades.LocationTags.Add(LocationTag.Middle);
            else if (worker.Grade == Grade.Junior)
                officeWithStaffersAndGrades.LocationTags.Add(LocationTag.Junior);
        }
    }

    // Method to assign vehicles to depots (offices)
    private OfficesWithStaffers AssignVehiclesToDepots()
    {
        var workers = GetAvailableWorkers().Result;
        var offices = GetOffices().Result;

        var officesWithStaffers = new OfficesWithStaffers { OfficeWithStaffersList = new List<OfficeWithStaffers>() };

        // if worker location coordinates are same as office location coordinates then assign this worker to this office
        foreach (var worker in workers)
        foreach (var office in offices)
        {
            if (worker.LocationCoordinates is null) continue;
            if (worker.LocationCoordinates == office.LocationCoordinates)
            {
                // assign worker to office
                var officeWithStaffers = new OfficeWithStaffers
                {
                    Office = office, Workers = new List<UserShortRead>
                    {
                        new()
                        {
                            Id = worker.Id, Name = worker.Name, Surname = worker.Surname,
                            Lastname = worker.Lastname, Location = worker.Location, Email = worker.Email,
                            LocationCoordinates = worker.LocationCoordinates, Grade = GetGrade(worker.Grade!.Value)
                        }
                    }
                };
            }
        }

        return officesWithStaffers;
    }

    // Method to get all available workers
    private async Task<List<User>> GetAvailableWorkers()
    {
        var availableList = _context.WorkerCases
            .Where(x => x.Case == WrkrСase.Work)
            .Select(x => x.Id)
            .ToList();
        
        return await _context.Users
            .Where(x => x.Role == Role.Worker)
            .Where(x => availableList.Contains(x.Id))
            .ToListAsync();
    }

    // Method to get all available offices
    private async Task<List<Office>> GetOffices()
    {
        return await _context.Offices
            .ToListAsync();
    }
    
    public static string? GetGrade(Grade grade) => grade switch
    {
        Grade.Senior => "Сеньор",
        Grade.Middle => "Мидл",
        Grade.Junior => "Джун",
        _ => null
    };
}
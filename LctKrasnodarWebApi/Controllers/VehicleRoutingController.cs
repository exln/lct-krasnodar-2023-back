using System.Text;
using Google.OrTools.ConstraintSolver;
using LctKrasnodarWebApi.Data;
using LctKrasnodarWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LctKrasnodarWebApi.Controllers;

[Route("[controller]")]
[ApiController]
public class VehicleRoutingController : Controller
{
    private readonly ApiDbContext _context;

    public VehicleRoutingController(ApiDbContext context)
    {
        _context = context;
    }

    [HttpPost("solve", Name = "Solve vehicle routing problem")]
    public IActionResult SolveVehicleRouting()
    {
        var routingData = CreateData();

        // Calculate distance matrix
        CalculateDistanceMatrix(routingData);

        var manager = CreateRoutingIndexManager(routingData);
        var routing = CreateRoutingModel(manager, routingData);

        var searchParameters = operations_research_constraint_solver.DefaultRoutingSearchParameters();
        searchParameters.FirstSolutionStrategy = FirstSolutionStrategy.Types.Value.PathCheapestArc;

        var solution = SolveRoutingProblemAssign(routing, searchParameters);

        var result = GetSolution(routingData, routing, manager, solution);

        return Ok(result);
    }

    private RoutingData CreateData()
    {
        var routingData = new RoutingData
        {
            VehicleNumber = _context.Users
                .Count(x => x.Role == Role.Worker),
            Depot = 0
        };

        // Get all depots from User table
        var depots = _context.Users
            .Where(x => x.Role == Role.Worker)
            .Select(x => x.LocationCoordinates)
            .ToList();

        routingData.Depots = depots;

        return routingData;
    }

    private void CalculateDistanceMatrix(RoutingData routingData)
    {
        var partnerInfos = _context.PartnerInfos.ToList();
        var distanceMatrix = new double[partnerInfos.Count, partnerInfos.Count];

        for (var i = 0; i < partnerInfos.Count; i++)
        for (var j = 0; j < partnerInfos.Count; j++)
        {
            var distance = CalculateDistance(partnerInfos[i].LocationCoordinates, partnerInfos[j].LocationCoordinates);
            distanceMatrix[i, j] = distance.Result;
        }

        routingData.DistanceMatrix = distanceMatrix;
    }

    private async Task<double> CalculateDistance(List<double> source, List<double> target)
    {
        // send request to valhalla api to endpoint /sources_to_targets
        // with source and target coordinates
        // and get distance in response

        using var client = new HttpClient();
        var content = new StringContent(JsonConvert.SerializeObject(new DistanceRequest
        {
            sources = source.Select((x, i) => new VallhallaCoordinate
            {
                lat = i == 0 ? x : source[0],
                lon = i == 1 ? x : source[1]
            }).ToList(),
            targets = target.Select((x, i) => new VallhallaCoordinate
            {
                lat = i == 0 ? x : source[0],
                lon = i == 1 ? x : source[1]
            }).ToList(),
            costing = "auto"
        }), Encoding.UTF8, "application/json");

        var response = await client.PostAsync("http://45.9.25.174/valhalla/sources_to_targets", content);

        Console.WriteLine(content.ReadAsStringAsync().Result);

        var responseString = await response.Content.ReadAsStringAsync();

        Console.WriteLine(responseString);

        if (responseString == null)
            // обработка случая, когда responseString равно null
            return 0;

        var responseJson = JsonConvert.DeserializeObject<RouteResponse>(responseString);

        if (responseJson == null || responseJson.sources_to_targets.Count == 0)
            // обработка случая, когда responseJson равно null или sources_to_targets пустой
            return 0;

        return responseJson.sources_to_targets[0][0].distance;
    }

    private RoutingIndexManager CreateRoutingIndexManager(RoutingData routingData)
    {
        return new RoutingIndexManager(routingData.DistanceMatrix.GetLength(0), routingData.VehicleNumber,
            routingData.Depots.Count);
    }

    private RoutingModel CreateRoutingModel(RoutingIndexManager manager, RoutingData routingData)
    {
        var routing = new RoutingModel(manager);

        var transitCallbackIndex = routing.RegisterTransitCallback((fromIndex, toIndex) =>
        {
            var fromNode = manager.IndexToNode(fromIndex);
            var toNode = manager.IndexToNode(toIndex);
            return (long)routingData.DistanceMatrix[fromNode, toNode];
        });

        routing.SetArcCostEvaluatorOfAllVehicles(transitCallbackIndex);

        routing.AddDimension(transitCallbackIndex, 0, 3000,
            false, "Distance");
        var distanceDimension = routing.GetMutableDimension("Distance");
        distanceDimension.SetGlobalSpanCostCoefficient(100);

        return routing;
    }

    private Assignment SolveRoutingProblemAssign(RoutingModel routing, RoutingSearchParameters searchParameters)
    {
        return routing.SolveWithParameters(searchParameters);
    }

    private string GetSolution(RoutingData routingData, RoutingModel routing, RoutingIndexManager manager,
        Assignment solution)
    {
        var output = new StringBuilder();
        output.AppendLine($"Objective {solution.ObjectiveValue()}:");

        // Inspect solution.
        for (var i = 0; i < routingData.VehicleNumber; ++i)
        {
            output.AppendLine($"Route for Vehicle {i}:");
            var index = routing.Start(i);
            while (routing.IsEnd(index) == false)
            {
                output.Append($"{manager.IndexToNode((int)index)} -> ");
                index = solution.Value(routing.NextVar(index));
            }

            output.AppendLine();
        }

        return output.ToString();
    }
}
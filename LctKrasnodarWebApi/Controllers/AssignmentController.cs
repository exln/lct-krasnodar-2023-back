using System.Text;
using Google.OrTools.Sat;
using LctKrasnodarWebApi.Data;
using LctKrasnodarWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace LctKrasnodarWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AssignmentController: Controller
{
    private readonly ApiDbContext _context;
    
    public AssignmentController(
        ApiDbContext context)
    {
        _context = context;
    }
    
    [HttpGet("GetMatrix")]
    [ProducesResponseType(200, Type = typeof(int[,]))]
    [ProducesResponseType(400, Type = typeof(string))]
    public IActionResult GetMatrix()
    {
        var costMatrix = CreateCostMatrix(_context);
        PrintCostMatrix(costMatrix);
        return Ok(costMatrix);
    }
    
    // Method for printing cost martix to console
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
    
    // Method for creating cost matrix
    private int[,] CreateCostMatrix(ApiDbContext _context)
    {
        var Constants = _context.ConstantTaskSizes
            .ToList();
        var PartnerInfos = _context.PartnerInfos.ToList();
        var Workers = _context.Users
            .Where(u => u.Role == Role.Worker)
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
                    costMatrix[i, j] = Int32.MaxValue;
                }
            }
        }
        
        for (var i = Constants.Count; i < Constants.Count + PartnerInfos.Count; i++)
        {
            for (var j = 0; j < Workers.Count; j++)
            {
                var workerCoordinates = Workers[j].LocationCoordinates;
                var partnerCoordinates = PartnerInfos[i - Constants.Count].LocationCoordinates;
                var time = GetValhallaRouteTime(workerCoordinates, partnerCoordinates).Result;
                costMatrix[i, j] = time;
            }
        }
        
        return costMatrix;
    }
    
    // Method to get route via valhalla api
    private async Task<int> GetValhallaRouteTime(List<double> source, List<double> target)
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

        float timeInSec = (float)jsonObject.trip.summary.time;
        int timeInMin = (int)Math.Round(timeInSec / 60);
        return timeInMin;
    }
    
}

public class Example
{
    public static void Main(String[] args)
    {
        // Data.
        int[,] costs =
        {
            { 90, 76, 75, 70, 50, 74 }, { 35, 85, 55, 65, 48, 101 }, { 125, 95, 90, 105, 59, 120 },
            { 45, 110, 95, 115, 104, 83 }, { 60, 105, 80, 75, 59, 62 }, { 45, 65, 110, 95, 47, 31 },
            { 38, 51, 107, 41, 69, 99 }, { 47, 85, 57, 71, 92, 77 }, { 39, 63, 97, 49, 118, 56 },
            { 47, 101, 71, 60, 88, 109 }, { 17, 39, 103, 64, 61, 92 }, { 101, 45, 83, 59, 92, 27 },
        };
        int numWorkers = costs.GetLength(0);
        int numTasks = costs.GetLength(1);

        int[] allWorkers = Enumerable.Range(0, numWorkers).ToArray();
        int[] allTasks = Enumerable.Range(0, numTasks).ToArray();

        int[] taskSizes = { 10, 7, 3, 12, 15, 4, 11, 5 };
        // Maximum total of task sizes for any worker
        int totalSizeMax = 15;

        // Allowed groups of workers:
        long[,] group1 =
        {
            { 0, 0, 1, 1 }, // Workers 2, 3
            { 0, 1, 0, 1 }, // Workers 1, 3
            { 0, 1, 1, 0 }, // Workers 1, 2
            { 1, 1, 0, 0 }, // Workers 0, 1
            { 1, 0, 1, 0 }, // Workers 0, 2
        };

        long[,] group2 =
        {
            { 0, 0, 1, 1 }, // Workers 6, 7
            { 0, 1, 0, 1 }, // Workers 5, 7
            { 0, 1, 1, 0 }, // Workers 5, 6
            { 1, 1, 0, 0 }, // Workers 4, 5
            { 1, 0, 0, 1 }, // Workers 4, 7
        };

        long[,] group3 =
        {
            { 0, 0, 1, 1 }, // Workers 10, 11
            { 0, 1, 0, 1 }, // Workers 9, 11
            { 0, 1, 1, 0 }, // Workers 9, 10
            { 1, 0, 1, 0 }, // Workers 8, 10
            { 1, 0, 0, 1 }, // Workers 8, 11
        };

        // Model.
        CpModel model = new CpModel();

        // Variables.
        BoolVar[,] x = new BoolVar[numWorkers, numTasks];
        // Variables in a 1-dim array.
        foreach (int worker in allWorkers)
        {
            foreach (int task in allTasks)
            {
                x[worker, task] = model.NewBoolVar($"x[{worker},{task}]");
            }
        }

        // Constraints
        // Each worker is assigned to at most max task size.
        foreach (int worker in allWorkers)
        {
            BoolVar[] vars = new BoolVar[numTasks];
            foreach (int task in allTasks)
            {
                vars[task] = x[worker, task];
            }
            model.Add(LinearExpr.WeightedSum(vars, taskSizes) <= totalSizeMax);
        }

        // Each task is assigned to exactly one worker.
        foreach (int task in allTasks)
        {
            List<ILiteral> workers = new List<ILiteral>();
            foreach (int worker in allWorkers)
            {
                workers.Add(x[worker, task]);
            }

            model.AddExactlyOne(workers);
        }

        // Create variables for each worker, indicating whether they work on some task.
        BoolVar[] work = new BoolVar[numWorkers];
        foreach (int worker in allWorkers)
        {
            work[worker] = model.NewBoolVar($"work[{worker}]");
        }

        foreach (int worker in allWorkers)
        {
            List<ILiteral> tasks = new List<ILiteral>();
            foreach (int task in allTasks)
            {
                tasks.Add(x[worker, task]);
            }

            model.Add(work[worker] == LinearExpr.Sum(tasks));
        }

        // Define the allowed groups of worders
        model.AddAllowedAssignments(new IntVar[] { work[0], work[1], work[2], work[3] }).AddTuples(group1);
        model.AddAllowedAssignments(new IntVar[] { work[4], work[5], work[6], work[7] }).AddTuples(group2);
        model.AddAllowedAssignments(new IntVar[] { work[8], work[9], work[10], work[11] }).AddTuples(group3);

        // Objective
        LinearExprBuilder obj = LinearExpr.NewBuilder();
        foreach (int worker in allWorkers)
        {
            foreach (int task in allTasks)
            {
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
            foreach (int worker in allWorkers)
            {
                foreach (int task in allTasks)
                {
                    if (solver.Value(x[worker, task]) > 0.5)
                    {
                        Console.WriteLine($"Worker {worker} assigned to task {task}. " +
                                          $"Cost: {costs[worker, task]}");
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
    }
}
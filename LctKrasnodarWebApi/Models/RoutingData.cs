namespace LctKrasnodarWebApi.Models;

public class RoutingData
{
    public double[,] DistanceMatrix { get; set; }
    public int VehicleNumber { get; set; }
    public int Depot { get; set; }
    public List<List<double>> Depots { get; set; } // Add this line
}

public class DataModel
{
    public double[,] DistanceMatrix { get; set; }
    public int[] Ends { get; set; }
    public int[] Starts { get; set; }
    public int VehicleNumber { get; set; }
    public List<List<double>> AllCoordinates { get; set; }
}

public class RouteModel
{
    public int VehicleId { get; set; }
    public List<int> Route { get; set; }
    public long RouteDistance { get; set; }
}
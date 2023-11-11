namespace LctKrasnodarWebApi.Models;

public class Resource
{
    public string Name { get; set; }
    public int Size { get; set; }
}

public class Coordinates
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class RoutingDto
{
    public List<Resource> ResourceList { get; set; }
    public List<Coordinates> SourceList { get; set; }
    public List<Coordinates> DepotList { get; set; }
}

public class CalculateDistanceMatrixRequest
{
    public RoutingDto RoutingDto { get; set; }
    public RoutingData RoutingData { get; set; }
}
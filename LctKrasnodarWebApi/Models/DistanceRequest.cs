namespace LctKrasnodarWebApi.Models;

public class DistanceRequest
{
    public List<VallhallaCoordinate> sources { get; set; }
    public List<VallhallaCoordinate> targets { get; set; }
    public string costing { get; set; }
}

public class VallhallaCoordinate
{
    public double lon { get; set; }
    public double lat { get; set; }
}

public class RoutingRequest
{
    public List<VallhallaCoordinate> locations { get; set; }
    public string costing { get; set; }
    public string directions_options { get; set; }
    public string units { get; set; }
    public string language { get; set; }
}
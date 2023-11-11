namespace LctKrasnodarWebApi.Models;

public class RouteResponse
{
    public string algorithm { get; set; }
    public string units { get; set; }
    public List<List<Coordinatas>> sources { get; set; }
    public List<List<Coordinatas>> targets { get; set; }
    public List<List<RouteDetail>> sources_to_targets { get; set; }
}

public class Coordinatas
{
    public double lon { get; set; }
    public double lat { get; set; }
}

public class RouteDetail
{
    public double distance { get; set; }
    public int time { get; set; }
    public int to_index { get; set; }
    public int from_index { get; set; }
}
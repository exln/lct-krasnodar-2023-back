namespace LctKrasnodarWebApi.Models;

public class RoutingResponse
{
}

public class trip
{
    public List<location> locations { get; set; }
    public List<leg> legs { get; set; }
    public summary summary { get; set; }
    public string status_message { get; set; }
    public int status { get; set; }
    public string units { get; set; }
    public string language { get; set; }
}

public class location
{
    public string type { get; set; }
    public double lat { get; set; }
    public double lon { get; set; }
    public string side_of_street { get; set; }
    public int original_index { get; set; }
}

public class leg
{
    public List<maneuver> maneuvers { get; set; }
    public summary summary { get; set; }
    public string shape { get; set; }
}

public class maneuver
{
    public int type { get; set; }
    public string instruction { get; set; }
    public string verbal_succinct_transition_instruction { get; set; }
    public string verbal_pre_transition_instruction { get; set; }
    public string verbal_post_transition_instruction { get; set; }
    public List<string> street_names { get; set; }
    public double time { get; set; }
    public double length { get; set; }
    public double cost { get; set; }
    public int begin_shape_index { get; set; }
    public int end_shape_index { get; set; }
    public bool verbal_multi_cue { get; set; }
    public string travel_mode { get; set; }
    public string travel_type { get; set; }
}

public class summary
{
    public bool has_time_restrictions { get; set; }
    public bool has_toll { get; set; }
    public bool has_highway { get; set; }
    public bool has_ferry { get; set; }
    public double min_lat { get; set; }
    public double min_lon { get; set; }
    public double max_lat { get; set; }
    public double maxLon { get; set; }
    public double time { get; set; }
    public double length { get; set; }
    public double cost { get; set; }
}
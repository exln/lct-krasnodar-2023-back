using System.ComponentModel.DataAnnotations;

namespace LctKrasnodarWebApi.Models;

public class YndxModels
{
    
}

public class GeocodeRequest
{
    [Required] public required string Address { get; set; }
}

public class GeocodeResponse
{
    [Required] public required Response Response { get; set; }
}

public class Response
{
    [Required] public required GeoObjectCollection GeoObjectCollection { get; set; }
}

public class GeoObjectCollection
{
    [Required] public required MetaDataProperty MetaDataProperty { get; set; }
    [Required] public required FeatureMember[] FeatureMember { get; set; }
}

public class MetaDataProperty
{
    [Required] public required GeocoderResponseMetaData GeocoderResponseMetaData { get; set; }
}

public class GeocoderResponseMetaData
{
    [Required] public required string Request { get; set; }
    [Required] public required string Found { get; set; }
    [Required] public required string Results { get; set; }
    [Required] public required string Point { get; set; }
}

public class AddressDetails
{
    [Required] public required Country Country { get; set; }
}

public class Country
{
    [Required] public required string AddressLine { get; set; }
    [Required] public required string CountryNameCode { get; set; }
    [Required] public required string CountryName { get; set; }
    [Required] public required AdministrativeArea AdministrativeArea { get; set; }
}

public class AdministrativeArea
{
    [Required] public required string AdministrativeAreaName { get; set; }
    [Required] public required SubAdministrativeArea SubAdministrativeArea { get; set; }
}

public class SubAdministrativeArea
{
    [Required] public required string SubAdministrativeAreaName { get; set; }
    [Required] public required Locality Locality { get; set; }
}

public class Locality
{
    [Required] public required string LocalityName { get; set; }
    [Required] public required Thoroughfare Thoroughfare { get; set; }
}

public class Thoroughfare
{
    [Required] public required string ThoroughfareName { get; set; }
    [Required] public required Premise Premise { get; set; }
}

public class Premise
{
    [Required] public required string PremiseNumber { get; set; }
}

public class FeatureMember
{
    [Required] public required GeoObject GeoObject { get; set; }
}

public class GeoObject
{
    [Required] public required string Name { get; set; }
    [Required] public required string Description { get; set; }
    [Required] public required BoundedBy BoundedBy { get; set; }
    [Required] public required Point Point { get; set; }
    [Required] public required MetaDataProperty MetaDataProperty { get; set; }
}

public class BoundedBy
{
    [Required] public required Envelope Envelope { get; set; }
}

public class Envelope
{
    [Required] public required string LowerCorner { get; set; }
    [Required] public required string UpperCorner { get; set; }
}

public class Point
{
    [Required] public required string Pos { get; set; }
}

public class GeocoderCustomResponse
{
    [Required] public required string AddressLine { get; set; }
    [Required] public required List<double> Poss { get; set; }
}

public class GeosuggestResponse
{
    [Required] public required Results Results { get; set; }
}

public class Results
{
    [Required] public required Result[] Result { get; set; }
}

public class Result
{
    [Required] public required Title Title { get; set; }
    [Required] public required Subtitle Subtitle { get; set; }
    [Required] public required string[] Tags { get; set; }
    [Required] public required Distance Distance { get; set; }
    [Required] public required Address Address { get; set; }
    [Required] public required string Uri { get; set; }
}

public class Title
{
    [Required] public required string Text { get; set; }
    [Required] public required Hl[] Hl { get; set; }
}

public class Hl
{
    [Required] public required int Begin { get; set; }
    [Required] public required int End { get; set; }
}

public class Subtitle
{
    [Required] public required string Text { get; set; }
}

public class Distance
{
    [Required] public required string Text { get; set; }
    [Required] public required double Value { get; set; }
}

public class Address
{
    [Required] public required string FormattedAddress { get; set; }
    [Required] public required Component[] Component { get; set; }
}

public class Component
{
    [Required] public required string Name { get; set; }
    [Required] public required string[] Kind { get; set; }
}

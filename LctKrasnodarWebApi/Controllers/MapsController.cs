using LctKrasnodarWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LctKrasnodarWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class MapsController : Controller
{    
    private readonly string _geocoderApiKey = "879d5ea4-0b90-4c83-bb59-c6adeb9e2978";
    private readonly string _geosuggestApiKey = "fe995c75-eb7d-4489-8593-95491e671a8e";
    
    [HttpPost("Geocoder", Name = "Geocoder")]
    [ProducesResponseType(200, Type = typeof(GeocoderCustomResponse))]
    [ProducesResponseType(400, Type = typeof(string))]
    public async Task<IActionResult> Geocoder([FromBody] GeocodeRequest geocodeRequest)
    {
        using (var httpClient = new HttpClient())
        {
            var requestUri = new UriBuilder("https://geocode-maps.yandex.ru/1.x/")
            {
                Query = $"apikey={_geocoderApiKey}&format=json&geocode={geocodeRequest.Address}"
            };
            var response = await httpClient.GetAsync(requestUri.Uri);
            var content = await response.Content.ReadAsStringAsync();
            
            dynamic geocodeResponse = JsonConvert.DeserializeObject(content);
            
            string addressLine = geocodeResponse.response.GeoObjectCollection.featureMember[0].GeoObject
                .metaDataProperty.GeocoderMetaData.AddressDetails.Country.AddressLine;
            string pos = geocodeResponse.response.GeoObjectCollection.featureMember[0].GeoObject.Point.pos;
            var coordinates = new List<double>
            {
                double.Parse(pos.Split(' ')[1]),
                double.Parse(pos.Split(' ')[0])
            };
            
            var geocoderCustomResponse = new GeocoderCustomResponse()
            {
                AddressLine = addressLine,
                Poss = coordinates
            };
            
            
            return Ok(geocoderCustomResponse);
            
        }
        
    }

    [HttpPost("Geosuggest", Name = "Geosuggest")]
    [ProducesResponseType(200, Type = typeof(GeocodeResponse))]
    [ProducesResponseType(400, Type = typeof(string))]
    public async Task<IActionResult> Geosuggest([FromBody] GeocodeRequest geocodeRequest)
    {
        
        using (var httpClient = new HttpClient())
        {
            var requestUri = new UriBuilder("https://suggest-maps.yandex.ru/suggest-geo")
            {
                Query = $"apikey={_geosuggestApiKey}&format=json&text={geocodeRequest.Address}"
            };
            var response = await httpClient.GetAsync(requestUri.Uri);
            var content = await response.Content.ReadAsStringAsync();
            var geocodeResponse = JsonConvert.DeserializeObject<GeosuggestResponse>(content);
            return Ok(geocodeResponse);
        }
    }
}

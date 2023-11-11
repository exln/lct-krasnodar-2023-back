using LctKrasnodarWebApi.Data;
using LctKrasnodarWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace LctKrasnodarWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class PartnerController : Controller
{
    private readonly ApiDbContext _context;

    public PartnerController(
        ApiDbContext context)
    {
        _context = context;
    }

    [HttpPost("New", Name = "Create new partner")]
    [ProducesResponseType(200, Type = typeof(PartnerInfoReadDto))]
    [ProducesResponseType(400, Type = typeof(string))]
    [ProducesResponseType(404, Type = typeof(string))]
    public async Task<IActionResult> CreateNewPartner([FromBody] PartnerInfoCreationDto partnerInfoCreationDto)
    {
        try
        {
            var partnerInfo = CreatePartnerInfo(partnerInfoCreationDto.Address, partnerInfoCreationDto.LocationCoordinates);
            await SavePartnerInfo(partnerInfo);
            
            PartnerInfoReadDto partnerInfoReadDto = new()
            {
                Id = partnerInfo.Id,
                Address = partnerInfo.Address,
                LocationCoordinates = partnerInfo.LocationCoordinates,
                WhenPointConnected = GetWhenConnectedString(partnerInfo.WhenPointConnected.Value),
                AreCardsAndMaterialsDelivered = GetYesNoString(partnerInfo.AreCardsAndMaterialsDelivered.Value),
                DaysSinceLastCardIssue = partnerInfo.DaysSinceLastCardIssue.Value,
                NumberOfApprovedApplications = partnerInfo.NumberOfApprovedApplications.Value,
                NumberOfGivenCards = partnerInfo.NumberOfGivenCards.Value,
                IsActive = partnerInfo.IsActive
            };

            return Ok(partnerInfoReadDto);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
    
    [HttpPost("Patch", Name = "Patch partner")]
    [ProducesResponseType(200, Type = typeof(PartnerInfoReadDto))]
    [ProducesResponseType(400, Type = typeof(string))]
    [ProducesResponseType(404, Type = typeof(string))]
    public async Task<IActionResult> PatchPartner([FromBody] PartnerInfoPatchDto partnerInfoPatchDto)
    {
        try
        {
            var partnerInfo = await _context.PartnerInfos.FindAsync(partnerInfoPatchDto.Id);
            if (partnerInfo == null) return NotFound();
            if (partnerInfoPatchDto.Address != null)
            {
                partnerInfo.Address = partnerInfoPatchDto.Address;
            }

            if (partnerInfoPatchDto.LocationCoordinates != null)
                partnerInfo.LocationCoordinates = partnerInfoPatchDto.LocationCoordinates;
            await _context.SaveChangesAsync();
            
            PartnerInfoReadDto partnerInfoReadDto = new()
            {
                Id = partnerInfo.Id,
                Address = partnerInfo.Address,
                LocationCoordinates = partnerInfo.LocationCoordinates,
                WhenPointConnected = GetWhenConnectedString(partnerInfo.WhenPointConnected.Value),
                AreCardsAndMaterialsDelivered = GetYesNoString(partnerInfo.AreCardsAndMaterialsDelivered.Value),
                DaysSinceLastCardIssue = partnerInfo.DaysSinceLastCardIssue.Value,
                NumberOfApprovedApplications = partnerInfo.NumberOfApprovedApplications.Value,
                NumberOfGivenCards = partnerInfo.NumberOfGivenCards.Value,
                IsActive = partnerInfo.IsActive
            };
            
            return Ok(partnerInfoReadDto);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
    
    [HttpGet("GetAll", Name = "Get all partners")]
    [ProducesResponseType(200, Type = typeof(List<PartnerInfoReadDto>))]
    [ProducesResponseType(400, Type = typeof(string))]
    [ProducesResponseType(404, Type = typeof(string))]
    public async Task<IActionResult> GetAllPartners()
    {
        try
        {
            var partnerInfos = await _context.PartnerInfos.ToListAsync();
            
            List<PartnerInfoReadDto> partnerInfoReadDtos = new();
            foreach (var partnerInfo in partnerInfos)
            {
                partnerInfoReadDtos.Add(new PartnerInfoReadDto()
                {
                    Id = partnerInfo.Id,
                    Address = partnerInfo.Address,
                    LocationCoordinates = partnerInfo.LocationCoordinates,
                    WhenPointConnected = GetWhenConnectedString(partnerInfo.WhenPointConnected.Value),
                    AreCardsAndMaterialsDelivered = GetYesNoString(partnerInfo.AreCardsAndMaterialsDelivered.Value),
                    DaysSinceLastCardIssue = partnerInfo.DaysSinceLastCardIssue.Value,
                    NumberOfApprovedApplications = partnerInfo.NumberOfApprovedApplications.Value,
                    NumberOfGivenCards = partnerInfo.NumberOfGivenCards.Value,
                    IsActive = partnerInfo.IsActive
                });
            }
            
            return Ok(partnerInfoReadDtos);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
    
    [HttpPost("Get", Name = "Get partner")]
    [ProducesResponseType(200, Type = typeof(PartnerInfoReadDto))]
    [ProducesResponseType(400, Type = typeof(string))]
    [ProducesResponseType(404, Type = typeof(string))]
    public async Task<IActionResult> GetPartner([FromBody] PartnerIdDto partnerIdDto)
    {
        try
        {
            var partnerInfo = await _context.PartnerInfos.FindAsync(partnerIdDto.Id);
            if (partnerInfo == null) return NotFound();
            
            PartnerInfoReadDto partnerInfoReadDto = new()
            {
                Id = partnerInfo.Id,
                Address = partnerInfo.Address,
                LocationCoordinates = partnerInfo.LocationCoordinates,
                WhenPointConnected = GetWhenConnectedString(partnerInfo.WhenPointConnected.Value),
                AreCardsAndMaterialsDelivered = GetYesNoString(partnerInfo.AreCardsAndMaterialsDelivered.Value),
                DaysSinceLastCardIssue = partnerInfo.DaysSinceLastCardIssue.Value,
                NumberOfApprovedApplications = partnerInfo.NumberOfApprovedApplications.Value,
                NumberOfGivenCards = partnerInfo.NumberOfGivenCards.Value,
                IsActive = partnerInfo.IsActive
            };
            
            return Ok(partnerInfoReadDto);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
    
    [HttpPost("Delete", Name = "Delete partner")]
    [ProducesResponseType(200, Type = typeof(PartnerInfoReadDto))]
    [ProducesResponseType(400, Type = typeof(string))]
    [ProducesResponseType(404, Type = typeof(string))]
    public async Task<IActionResult> DeletePartner([FromBody] PartnerIdDto partnerIdDto)
    {
        try
        {
            var partnerInfo = await _context.PartnerInfos.FindAsync(partnerIdDto.Id);
            if (partnerInfo == null) return NotFound();
            _context.PartnerInfos.Remove(partnerInfo);
            await _context.SaveChangesAsync();
            
            PartnerInfoReadDto partnerInfoReadDto = new()
            {
                Id = partnerInfo.Id,
                Address = partnerInfo.Address,
                LocationCoordinates = partnerInfo.LocationCoordinates,
                WhenPointConnected = GetWhenConnectedString(partnerInfo.WhenPointConnected.Value),
                AreCardsAndMaterialsDelivered = GetYesNoString(partnerInfo.AreCardsAndMaterialsDelivered.Value),
                DaysSinceLastCardIssue = partnerInfo.DaysSinceLastCardIssue.Value,
                NumberOfApprovedApplications = partnerInfo.NumberOfApprovedApplications.Value,
                NumberOfGivenCards = partnerInfo.NumberOfGivenCards.Value,
                IsActive = partnerInfo.IsActive
            };
            
            return Ok(partnerInfoReadDto);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
    
    [HttpPost("Switch", Name = "Reverse partner status")]
    [ProducesResponseType(200, Type = typeof(PartnerInfoReadDto))]
    [ProducesResponseType(400, Type = typeof(string))]
    [ProducesResponseType(404, Type = typeof(string))]
    public async Task<IActionResult> ActivatePartner([FromBody] PartnerIdDto partnerIdDto)
    {
        try
        {
            var partnerInfo = await _context.PartnerInfos.FindAsync(partnerIdDto.Id);
            if (partnerInfo == null) return NotFound();
            
            partnerInfo.IsActive = !partnerInfo.IsActive;
            await _context.SaveChangesAsync();
            
            PartnerInfoReadDto partnerInfoReadDto = new()
            {
                Id = partnerInfo.Id,
                Address = partnerInfo.Address,
                LocationCoordinates = partnerInfo.LocationCoordinates,
                WhenPointConnected = GetWhenConnectedString(partnerInfo.WhenPointConnected.Value),
                AreCardsAndMaterialsDelivered = GetYesNoString(partnerInfo.AreCardsAndMaterialsDelivered.Value),
                DaysSinceLastCardIssue = partnerInfo.DaysSinceLastCardIssue.Value,
                NumberOfApprovedApplications = partnerInfo.NumberOfApprovedApplications.Value,
                NumberOfGivenCards = partnerInfo.NumberOfGivenCards.Value,
                IsActive = partnerInfo.IsActive
            };
            
            return Ok(partnerInfoReadDto);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    [HttpPost("Stats/Patch", Name = "Patch partner statistics")]
    [ProducesResponseType(200, Type = typeof(PartnerInfoReadDto))]
    [ProducesResponseType(400, Type = typeof(string))]
    [ProducesResponseType(404, Type = typeof(string))]
    public async Task<IActionResult> PatchPartnerStats([FromBody] PartnerStatsPatchDto partnerStatsPatchDto)
    {
        try
        {
            var partnerInfo = await _context.PartnerInfos.FindAsync(partnerStatsPatchDto.Id);
            if (partnerInfo == null) return NotFound();
            if (partnerStatsPatchDto.WhenPointConnected != null)
                partnerInfo.WhenPointConnected = SetWhenConnected(partnerStatsPatchDto.WhenPointConnected);
            if (partnerStatsPatchDto.AreCardsAndMaterialsDelivered != null)
                partnerInfo.AreCardsAndMaterialsDelivered = SetYesNo(partnerStatsPatchDto.AreCardsAndMaterialsDelivered);
            if (partnerStatsPatchDto.DaysSinceLastCardIssue != null)
                partnerInfo.DaysSinceLastCardIssue = partnerStatsPatchDto.DaysSinceLastCardIssue;
            if (partnerStatsPatchDto.NumberOfApprovedApplications != null)
                partnerInfo.NumberOfApprovedApplications = partnerStatsPatchDto.NumberOfApprovedApplications;
            if (partnerStatsPatchDto.NumberOfGivenCards != null)
                partnerInfo.NumberOfGivenCards = partnerStatsPatchDto.NumberOfGivenCards;
            await _context.SaveChangesAsync();
            
            PartnerInfoReadDto partnerInfoReadDto = new()
            {
                Id = partnerInfo.Id,
                Address = partnerInfo.Address,
                LocationCoordinates = partnerInfo.LocationCoordinates,
                WhenPointConnected = GetWhenConnectedString(partnerInfo.WhenPointConnected.Value),
                AreCardsAndMaterialsDelivered = GetYesNoString(partnerInfo.AreCardsAndMaterialsDelivered.Value),
                DaysSinceLastCardIssue = partnerInfo.DaysSinceLastCardIssue.Value,
                NumberOfApprovedApplications = partnerInfo.NumberOfApprovedApplications.Value,
                NumberOfGivenCards = partnerInfo.NumberOfGivenCards.Value,
                IsActive = partnerInfo.IsActive
            };
            
            return Ok(partnerInfoReadDto);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
    
    [HttpPost("Stats/Delete", Name = "Delete partner statistics")]
    [ProducesResponseType(200, Type = typeof(PartnerInfoReadDto))]
    [ProducesResponseType(400, Type = typeof(string))]
    [ProducesResponseType(404, Type = typeof(string))]
    public async Task<IActionResult> DeletePartnerStats([FromBody] PartnerIdDto partnerIdDto)
    {
        try
        {
            var partnerInfo = await _context.PartnerInfos.FindAsync(partnerIdDto.Id);
            if (partnerInfo == null) return NotFound();
            partnerInfo.WhenPointConnected = WhenConnected.Yesterday;
            partnerInfo.AreCardsAndMaterialsDelivered = YesNo.No;
            partnerInfo.DaysSinceLastCardIssue = 0;
            partnerInfo.NumberOfApprovedApplications = 0;
            partnerInfo.NumberOfGivenCards = 0;
            await _context.SaveChangesAsync();
            
            PartnerInfoReadDto partnerInfoReadDto = new()
            {
                Id = partnerInfo.Id,
                Address = partnerInfo.Address,
                LocationCoordinates = partnerInfo.LocationCoordinates,
                WhenPointConnected = GetWhenConnectedString(partnerInfo.WhenPointConnected.Value),
                AreCardsAndMaterialsDelivered = GetYesNoString(partnerInfo.AreCardsAndMaterialsDelivered.Value),
                DaysSinceLastCardIssue = partnerInfo.DaysSinceLastCardIssue.Value,
                NumberOfApprovedApplications = partnerInfo.NumberOfApprovedApplications.Value,
                NumberOfGivenCards = partnerInfo.NumberOfGivenCards.Value,
                IsActive = partnerInfo.IsActive
            };
            
            return Ok(partnerInfoReadDto);
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }
    
    private string GetYesNoString(YesNo yesNo)
    {
        return yesNo switch
        {
            YesNo.Yes => "Да",
            YesNo.No => "Нет",
            _ => throw new ArgumentOutOfRangeException(nameof(yesNo), yesNo, null)
        };
    }
    
    private YesNo SetYesNo(string yesNo)
    {
        return yesNo switch
        {
            "Да" => YesNo.Yes,
            "Нет" => YesNo.No,
            _ => throw new ArgumentOutOfRangeException(nameof(yesNo), yesNo, null)
        };
    }
    
    private string GetWhenConnectedString(WhenConnected whenConnected)
    {
        return whenConnected switch
        {
            WhenConnected.Long => "Давно",
            WhenConnected.Yesterday => "Вчера",
            _ => throw new ArgumentOutOfRangeException(nameof(whenConnected), whenConnected, null)
        };
    }
    
    private WhenConnected SetWhenConnected(string whenConnected)
    {
        return whenConnected switch
        {
            "Давно" => WhenConnected.Long,
            "Вчера" => WhenConnected.Yesterday,
            _ => throw new ArgumentOutOfRangeException(nameof(whenConnected), whenConnected, null)
        };
    }
    
    private async Task<List<double>> GetLocationCoordinates(string address)
    {
        string apiKey = "879d5ea4-0b90-4c83-bb59-c6adeb9e2978";
        using var httpClient = new HttpClient();
        var requestUri = new UriBuilder("https://geocode-maps.yandex.ru/1.x/")
        {
            Query = $"apikey={apiKey}&format=json&geocode={address}"
        };
        var response = await httpClient.GetAsync(requestUri.Uri);
        var content = await response.Content.ReadAsStringAsync();
        var geocodeResponse = JsonConvert.DeserializeObject<GeocodeResponse>(content);

        var point = geocodeResponse.Response.GeoObjectCollection.FeatureMember[0].GeoObject.Point.Pos;
        var locationCoordinates = new List<double>
        {
            double.Parse(point.Split(' ')[1]),
            double.Parse(point.Split(' ')[0])
        };
        return locationCoordinates;
    }

    private PartnerInfo CreatePartnerInfo(string address, List<double> locationCoordinates)
    {
        var partnerInfo = new PartnerInfo
        {
            Address = address,
            LocationCoordinates = locationCoordinates
        };
        return partnerInfo;
    }

    private async Task SavePartnerInfo(PartnerInfo partnerInfo)
    {
        _context.PartnerInfos.Add(partnerInfo);
        await _context.SaveChangesAsync();
    }
}
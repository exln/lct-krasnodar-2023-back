using Microsoft.AspNetCore.Mvc;

namespace LctKrasnodarWebApi.Controllers;

[Route("[controller]")]
[ApiController]
public class StatusController : Controller
{
    [HttpGet]
    public async Task<IActionResult> GetStatus()
    {
        return Ok("Бебебе");
    }
}
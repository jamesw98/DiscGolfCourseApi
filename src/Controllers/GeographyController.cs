using DiscGolfCourseApi.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DiscGolfCourseApi.Controllers;

[Route("api/geography")]
public class GeographyController : Controller<GeographyRepository>
{
    public GeographyController(GeographyRepository repo) : base(repo)
    {
    }

    [HttpGet]
    [Route("zipcode/{zipCode}")]
    public async Task<IActionResult> GetZipCodeGeography(string zipCode)
    {
        return Ok();
    }
}
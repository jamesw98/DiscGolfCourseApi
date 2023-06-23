using DiscGolfCourseApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DiscGolfCourseApi.Controllers;

[Route("api/details")]
public class DetailsController : Controller<DetailsRepository>
{
    public DetailsController(DetailsRepository repo) : base(repo)
    {
    }

    [HttpGet]
    [SwaggerOperation("Gets all details to filter on (zipcode (optional), tee type, target type, etc)")]
    public IActionResult GetFilterDetails([FromQuery] bool includeZipcodes)
    {
        return Ok(Repo.GetFilterDetails(includeZipcodes));
    }
}